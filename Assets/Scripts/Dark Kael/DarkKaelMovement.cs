using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class DarkKaelMovement : MonoBehaviour
{
    #region Variables
    private const int STATE_IDLE = 0;
    private const int STATE_WALKING = 1;
    private const int STATE_RUNNING = 2;
    private const int STATE_JUMPING = 3;

    [Header("Stats")]
    public float jumpHeight = 10f;

    [Header("Target Settings")]
    [SerializeField] private string targetTag = "Player";

    [Header("Movement Settings")]
    [SerializeField] private float walkDistanceThreshold = 5f;
    [SerializeField] private float idleDistanceThreshold = 2f;
    [SerializeField] private float runSpeed = 16f;
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float centerAlignSpeed = 20f;
    [SerializeField] private float movementSmoothing = 0.05f;
    [SerializeField] private float centerPositionTolerance = 0.1f;
    [SerializeField] private bool enableIdleState = true;
    [SerializeField] private bool enableWalkingState = true;

    [Header("Jump Settings")]
    [SerializeField] private float jumpCooldown = 0.4f;
    [SerializeField] private float groundCheckDelayAfterJump = 0.2f;
    [SerializeField] private float jumpPredictionDistance = 2f;
    [SerializeField] private float upwardRayLength = 5f;
    [SerializeField] private float upwardRayCircleRadius = 0.5f;
    [SerializeField] private float obstacleWaitTime = 0.5f;

    [Header("Ground Detection")]
    [SerializeField] private float groundRayLength = 3.5f;
    [SerializeField] private float groundRayHorizontalOffset = 0.5f;

    [Header("Wall Detection")]
    [SerializeField] private float wallRayLength = 2f;
    [SerializeField] private float rayHeight = 1.5f;

    [Header("Collider Settings - Idle")]
    [SerializeField] private Vector2 idleColliderOffset = new Vector2(0f, 0f);
    [SerializeField] private Vector2 idleColliderSize = new Vector2(1f, 2f);

    [Header("Collider Settings - Run")]
    [SerializeField] private Vector2 runColliderOffset = new Vector2(0f, -0.2f);
    [SerializeField] private Vector2 runColliderSize = new Vector2(0.9f, 1.8f);

    [Header("Collider Settings - Jump")]
    [SerializeField] private Vector2 jumpColliderOffset = new Vector2(0f, -0.5f);
    [SerializeField] private Vector2 jumpColliderSize = new Vector2(0.8f, 1.5f);

    [Header("DEBUG")]
    public bool DEBUGMODE = false;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LayerMask whatIsGround;

    private float speed = 0f;
    private bool movingRight = true;
    private bool canJump = true;
    private bool moveEnabled = true;
    private bool isGrounded = false;
    private bool isIdle = false;
    private bool waitingForObstacle = false;
    private bool needsToCenter = false;
    private bool isJumpingUpward = false;
    private int currentState = STATE_IDLE;
    private int targetState = STATE_IDLE;
    private Vector2 currentVelocity = Vector2.zero;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Transform target;

    private RaycastHit2D leftInfoGround;
    private RaycastHit2D rightInfoGround;
    private RaycastHit2D leftInfoWall;
    private RaycastHit2D rightInfoWall;
    private RaycastHit2D upwardInfo;
    #endregion

    #region Unity Callbacks
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        if (animator == null || spriteRenderer == null)
        {
            Debug.LogError("Animator atau SpriteRenderer belum di-assign.");
            enabled = false;
            return;
        }

        rb.freezeRotation = true;
        ResetColliderToIdle();
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            FindTargetByTag();
            if (target == null) return;
        }

        UpdateRaycasts();
        CheckForTargetAbove();
        HandleMovementState();

        if (!isIdle)
            HandleJumpLogic();

        UpdateAnimation();
        ApplyGravityModifiers();
        UpdateCollider();
    }

    void FixedUpdate()
    {
        if (!isGrounded || waitingForObstacle) return;
        HandleMovement();
    }
    #endregion

    #region Target Helpers
    private void FindTargetByTag()
    {
        GameObject obj = GameObject.FindGameObjectWithTag(targetTag);
        if (obj && obj.activeInHierarchy)
        {
            target = obj.transform;
            if (DEBUGMODE)
            {
                Debug.Log("DarkKaelMovement: Target ditemukan! Nama target: " + obj.name);
            }
        }
        else
        {
            target = null;
            if (DEBUGMODE)
            {
                Debug.LogWarning("DarkKaelMovement: Target TIDAK ditemukan dengan tag: " + targetTag);
            }
        }
    }

    private void CheckForTargetAbove()
    {
        Vector2 center = (Vector2)transform.position + Vector2.up * upwardRayLength;
        bool targetInside = target != null && Vector2.Distance(center, target.position) <= upwardRayCircleRadius;

        if (targetInside && !needsToCenter && !waitingForObstacle && isGrounded)
        {
            needsToCenter = true;
            moveEnabled = true;
            isIdle = false;
        }
    }
    #endregion

    #region Movement
    private void HandleMovementState()
    {
        float dist = Vector2.Distance(transform.position, target.position);

        bool dir = (target.position.x - transform.position.x) >= 0f;
        if (dir != movingRight)
        {
            movingRight = dir;
            spriteRenderer.flipX = !movingRight;
        }

        if (needsToCenter)
        {
            if (Mathf.Abs(transform.position.x - target.position.x) < centerPositionTolerance)
            {
                needsToCenter = false;
                if (upwardInfo.collider != null)
                    StartCoroutine(PerformCenteredJump());
                return;
            }

            speed = centerAlignSpeed;
            SetTargetState(STATE_RUNNING);
            return;
        }

        if (enableIdleState && dist <= idleDistanceThreshold)
        {
            speed = 0f;
            SetTargetState(STATE_IDLE);
            return;
        }

        if (enableWalkingState && dist <= walkDistanceThreshold)
        {
            speed = walkSpeed;
            SetTargetState(STATE_WALKING);
            return;
        }

        speed = runSpeed;
        SetTargetState(STATE_RUNNING);
    }

    private void HandleMovement()
    {
        if (!moveEnabled) return;

        Vector2 dir = (target.position - transform.position).normalized;
        float horiz = dir.x * speed;

        Rigidbody2D targetRB = target.GetComponent<Rigidbody2D>();
        if (targetRB != null)
        {
            float prediction = targetRB.linearVelocity.x * Time.deltaTime * jumpPredictionDistance;
            horiz = Mathf.Clamp(horiz + prediction, -speed, speed);
        }

        Vector2 targetVel = new Vector2(horiz, rb.linearVelocity.y);
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVel, ref currentVelocity, movementSmoothing);
    }
    #endregion

    #region Jumping
    private void HandleJumpLogic()
    {
        if (isIdle || !canJump || !isGrounded || waitingForObstacle) return;

        bool targetAbove = target.position.y > transform.position.y + 2f;
        float horizontalDist = Mathf.Abs(target.position.x - transform.position.x);

        if (targetAbove && horizontalDist < 3f && !isJumpingUpward && !needsToCenter)
        {
            StartCoroutine(Jump(jumpHeight * 1.2f, true));
            return;
        }

        if (!isJumpingUpward && !needsToCenter)
        {
            if (leftInfoGround.collider == null && rightInfoGround.collider != null)
            {
                if (leftInfoWall.collider == null)
                    StartCoroutine(Jump(jumpHeight, false));
            }
            else if (leftInfoGround.collider != null && rightInfoGround.collider == null)
            {
                if (rightInfoWall.collider == null)
                    StartCoroutine(Jump(jumpHeight, false));
            }

            if (isGrounded)
            {
                if (leftInfoWall.collider && leftInfoWall.distance <= wallRayLength / 1.5f && !movingRight)
                    StartCoroutine(Jump(jumpHeight, false));

                if (rightInfoWall.collider && rightInfoWall.distance <= wallRayLength / 1.5f && movingRight)
                    StartCoroutine(Jump(jumpHeight, false));
            }
        }
    }

    private IEnumerator Jump(float force, bool upward)
    {
        if (!canJump) yield break;

        canJump = false;
        isGrounded = false;
        isIdle = false;
        currentState = STATE_JUMPING;
        targetState = STATE_JUMPING;

        if (upward)
        {
            rb.linearVelocity = new Vector2(0, force);
            isJumpingUpward = true;
        }
        else
        {
            rb.linearVelocity = new Vector2(movingRight ? force / 2 : -force / 2, force);
            isJumpingUpward = false;
        }

        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
        isJumpingUpward = false;
    }

    private IEnumerator PerformCenteredJump()
    {
        waitingForObstacle = true;
        moveEnabled = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.1f);

        float heightDiff = target.position.y - transform.position.y;
        float required = Mathf.Clamp(heightDiff * 1.2f, jumpHeight, jumpHeight * 1.5f);

        yield return StartCoroutine(Jump(required, true));

        waitingForObstacle = false;
        moveEnabled = true;
    }
    #endregion

    #region Collider Switching
    private void UpdateCollider()
    {
        if (!isGrounded)
            SetCollider(jumpColliderOffset, jumpColliderSize);
        else if (currentState == STATE_RUNNING)
            SetCollider(runColliderOffset, runColliderSize);
        else
            ResetColliderToIdle();
    }

    private void ResetColliderToIdle() => SetCollider(idleColliderOffset, idleColliderSize);

    private void SetCollider(Vector2 offset, Vector2 size)
    {
        capsuleCollider.offset = offset;
        capsuleCollider.size = size;
    }
    #endregion

    #region Raycasts and Anim
    private void UpdateRaycasts()
    {
        Vector2 leftGround = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y);
        Vector2 rightGround = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y);

        leftInfoGround = Physics2D.Raycast(leftGround, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(rightGround, Vector2.down, groundRayLength, whatIsGround);

        Vector2 leftWall = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y + rayHeight);
        Vector2 rightWall = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y + rayHeight);

        leftInfoWall = Physics2D.Raycast(leftWall, Vector2.left, wallRayLength, whatIsGround);
        rightInfoWall = Physics2D.Raycast(rightWall, Vector2.right, wallRayLength, whatIsGround);

        upwardInfo = Physics2D.Raycast(transform.position, Vector2.up, upwardRayLength, whatIsGround);

        bool wasGrounded = isGrounded;
        isGrounded = leftInfoGround.collider || rightInfoGround.collider;

        if (!wasGrounded && isGrounded)
        {
            float dist = Vector2.Distance(transform.position, target.position);
            SetTargetState(dist <= walkDistanceThreshold ? STATE_WALKING : STATE_RUNNING);
        }

        if (DEBUGMODE)
        {
            Debug.DrawRay(leftWall, Vector2.left * wallRayLength, leftInfoWall.collider ? Color.green : Color.red);
            Debug.DrawRay(rightWall, Vector2.right * wallRayLength, rightInfoWall.collider ? Color.green : Color.red);
            Debug.DrawRay(transform.position, Vector2.up * upwardRayLength, upwardInfo.collider ? Color.green : Color.red);
        }
    }

    private void UpdateAnimation()
    {
        if (!isGrounded)
            currentState = STATE_JUMPING;
        else if (Mathf.Abs(rb.linearVelocity.x) < 0.1f)
            currentState = STATE_IDLE;
        else
            currentState = targetState;

        animator.SetInteger("state", currentState);
    }

    private void SetTargetState(int newState)
    {
        if (targetState != newState)
            targetState = newState;
    }

    private void ApplyGravityModifiers()
    {
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.8f - 1f) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !isGrounded)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.2f - 1f) * Time.deltaTime;
    }
    #endregion

    #region Gizmos
    void OnDrawGizmos()
    {
        if (!DEBUGMODE) return;

        if (target == null)
        {
            FindTargetByTag();
        }

        if (target)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
            Gizmos.DrawWireSphere(target.position, walkDistanceThreshold);

            Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
            Gizmos.DrawWireSphere(target.position, idleDistanceThreshold);
        }

        Vector2 center = (Vector2)transform.position + Vector2.up * upwardRayLength;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, upwardRayCircleRadius);

        Vector2 leftGround = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y);
        Vector2 rightGround = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y);

        Gizmos.color = leftInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(leftGround, leftGround + Vector2.down * groundRayLength);

        Gizmos.color = rightInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(rightGround, rightGround + Vector2.down * groundRayLength);

        Vector2 leftWall = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y + rayHeight);
        Vector2 rightWall = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y + rayHeight);

        Gizmos.color = new Color(0f, 1f, 1f, 0.5f);
        Gizmos.DrawLine(leftWall, leftWall + Vector2.left * wallRayLength);
        Gizmos.DrawLine(rightWall, rightWall + Vector2.right * wallRayLength);

        if (target)
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
    #endregion
}
