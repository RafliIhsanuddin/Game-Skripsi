using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class Companion : MonoBehaviour
{
    #region Variables
    // Animation states
    private const int STATE_IDLE = 0;
    private const int STATE_WALKING = 1;
    private const int STATE_RUNNING = 2;
    private const int STATE_JUMPING = 3;

    [Header("Stats")]
    public float jumpHeight = 8f;

    [Header("Movement Settings")]
    [SerializeField] private float walkDistanceThreshold = 5f;
    [SerializeField] private float idleDistanceThreshold = 1f;
    [SerializeField] private float runSpeed = 14f;
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float centerAlignSpeed = 20f;
    [SerializeField] private float movementSmoothing = 0.05f;
    [SerializeField] private float centerPositionTolerance = 0.1f;

    [Header("State Transition Durations")]
    [SerializeField] private float idleToWalkDuration = 0.1f;
    [SerializeField] private float walkToRunDuration = 0.2f;
    [SerializeField] private float runToWalkDuration = 0.1f;
    [SerializeField] private float walkToIdleDuration = 0.1f;

    [Header("Jump Settings")]
    [SerializeField] public float upwardRayLength = 5f;
    [SerializeField] private float obstacleWaitTime = 0.5f;
    [SerializeField] private float jumpCooldown = 0.3f;
    [SerializeField] private float groundCheckDelayAfterJump = 0.2f;
    [SerializeField] private float upwardRayCircleRadius = 0.5f;

    [Header("Ground Detection")]
    [SerializeField] private float groundRayLength = 3.5f;
    [SerializeField] private float groundRayHorizontalOffset = 0.5f;

    [Header("Wall Detection")]
    [SerializeField] private float wallRayLength = 30f;
    [SerializeField] private float rayHeight = 22.5f; // Currently unused in logic

    [Header("DEBUGING")]
    public bool DEBUGMODE = false;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LayerMask whatIsGround;

    // Internal variables
    private float speed;
    private bool movingRight = true;
    private bool canJump = true;
    private bool moveEnabled = true;
    private bool isGrounded = false;
    private bool isIdle = false;
    private bool waitingForObstacle = false;
    private bool isJumpingUpward = false;
    private bool needsToCenter = false;
    private int currentState = STATE_IDLE;
    private float stateTransitionTimer = 0f;
    private int targetState = STATE_IDLE;
    private Vector2 currentVelocity = Vector2.zero;
    private bool movementRestrictedLeft = false;
    private bool movementRestrictedRight = false;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private GameObject playerTarget;

    // Raycast hits
    private RaycastHit2D leftInfoGround;
    private RaycastHit2D rightInfoGround;
    private RaycastHit2D leftInfo;
    private RaycastHit2D rightInfo;
    private RaycastHit2D upwardInfo;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        playerTarget = GameObject.FindGameObjectWithTag("Player");
        if (playerTarget == null)
        {
            Debug.LogError("Could not find player GameObject with tag 'Player'!");
            enabled = false;
            return;
        }

        if (animator == null || spriteRenderer == null)
        {
            Debug.LogError("Animator or SpriteRenderer reference is missing!");
            enabled = false;
            return;
        }

        rb.freezeRotation = true;
    }

    void Update()
    {
        if (playerTarget == null) return;

        UpdateRaycasts();
        CheckForPlayerAbove();
        HandleMovementState();

        if (!isIdle)
        {
            HandleJumpLogic();
        }

        UpdateAnimation();
        ApplyGravityModifiers();
        HandleStateTransitions();
    }

    void FixedUpdate()
    {
        if (!isGrounded || waitingForObstacle) return;

        HandleMovement();
    }

    #region Public Methods
    public void RestrictMovement(bool left, bool right)
    {
        movementRestrictedLeft = left;
        movementRestrictedRight = right;

        // If currently moving in a restricted direction, stop
        if ((movingRight && movementRestrictedRight) || (!movingRight && movementRestrictedLeft))
        {
            isIdle = true;
            moveEnabled = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            SetTargetState(STATE_IDLE);
        }
    }

    public void ResetMovementRestriction()
    {
        movementRestrictedLeft = false;
        movementRestrictedRight = false;

        // Re-enable movement if we were forced idle by restrictions
        if (isIdle && Vector2.Distance(transform.position, playerTarget.transform.position) > idleDistanceThreshold)
        {
            isIdle = false;
            moveEnabled = true;
        }
    }
    #endregion

    #region Core Logic
    private void CheckForPlayerAbove()
    {
        Vector2 circleCenter = (Vector2)transform.position + Vector2.up * upwardRayLength;
        bool playerInCircle = Vector2.Distance(circleCenter, playerTarget.transform.position) <= upwardRayCircleRadius;

        if (playerInCircle && !needsToCenter && !waitingForObstacle && isGrounded)
        {
            needsToCenter = true;
            moveEnabled = true;
            isIdle = false;
        }
    }

    private void HandleMovementState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.transform.position);

        if (needsToCenter)
        {
            if (Mathf.Abs(transform.position.x - playerTarget.transform.position.x) < centerPositionTolerance)
            {
                needsToCenter = false;
                if (upwardInfo.collider != null)
                {
                    StartCoroutine(PerformCenteredJump());
                }
                return;
            }

            speed = centerAlignSpeed;
            SetTargetState(STATE_RUNNING);

            bool newDirection = (playerTarget.transform.position.x - transform.position.x) >= 0;
            if (newDirection != movingRight)
            {
                movingRight = newDirection;
                spriteRenderer.flipX = !movingRight;
            }
            return;
        }

        if (distanceToPlayer <= idleDistanceThreshold && isGrounded && !waitingForObstacle)
        {
            if (!isIdle)
            {
                isIdle = true;
                moveEnabled = false;
                speed = 0;
                SetTargetState(STATE_IDLE);
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }
        else
        {
            if (isIdle)
            {
                isIdle = false;
                moveEnabled = true;
            }

            float newSpeed = distanceToPlayer <= walkDistanceThreshold ? walkSpeed : runSpeed;
            if (newSpeed != speed)
            {
                speed = newSpeed;
                SetTargetState(newSpeed == walkSpeed ? STATE_WALKING : STATE_RUNNING);
            }
        }

        if (!isIdle && playerTarget != null)
        {
            bool newDirection = (playerTarget.transform.position.x - transform.position.x) >= 0;
            if (newDirection != movingRight)
            {
                movingRight = newDirection;
                spriteRenderer.flipX = !movingRight;
            }
        }
    }

    private void UpdateRaycasts()
    {
        Vector2 leftGroundPos = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y);
        Vector2 rightGroundPos = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y);

        leftInfoGround = Physics2D.Raycast(leftGroundPos, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(rightGroundPos, Vector2.down, groundRayLength, whatIsGround);

        leftInfo = Physics2D.Raycast(leftGroundPos, Vector2.left, wallRayLength, whatIsGround);
        rightInfo = Physics2D.Raycast(rightGroundPos, Vector2.right, wallRayLength, whatIsGround);

        upwardInfo = Physics2D.Raycast(transform.position, Vector2.up, upwardRayLength, whatIsGround);

        isGrounded = leftInfoGround.collider != null || rightInfoGround.collider != null;

        // Debug visualization for wall rays
        Debug.DrawRay(leftGroundPos, Vector2.left * wallRayLength, leftInfo.collider ? Color.green : Color.red);
        Debug.DrawRay(rightGroundPos, Vector2.right * wallRayLength, rightInfo.collider ? Color.green : Color.red);

        // Debug visualization for rayHeight (though not used in logic)
        Debug.DrawRay(transform.position, Vector2.up * rayHeight, Color.blue);
    }

    private void HandleJumpLogic()
    {
        if (isIdle || waitingForObstacle || !canJump || !isGrounded) return;

        if (!isJumpingUpward && !needsToCenter)
        {
            if (leftInfoGround.collider == false && rightInfoGround.collider == true)
            {
                if (leftInfo.collider == false) StartCoroutine(Jump("Edge", false, jumpHeight));
            }
            else if (leftInfoGround.collider == true && rightInfoGround.collider == false)
            {
                if (rightInfo.collider == false) StartCoroutine(Jump("Edge", true, jumpHeight));
            }

            if (isGrounded && !isJumpingUpward && !needsToCenter)
            {
                if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && !movingRight)
                    StartCoroutine(Jump("Wall", movingRight, jumpHeight));

                if (rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && movingRight)
                    StartCoroutine(Jump("Wall", movingRight, jumpHeight));
            }
        }
    }

    private void HandleMovement()
    {
        if (!moveEnabled) return;

        // Check if movement in current direction is restricted
        if ((movingRight && movementRestrictedRight) || (!movingRight && movementRestrictedLeft))
        {
            isIdle = true;
            moveEnabled = false;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            SetTargetState(STATE_IDLE);
            return;
        }

        Vector2 targetVelocity = movingRight ?
            new Vector2(speed, rb.linearVelocity.y) :
            new Vector2(-speed, rb.linearVelocity.y);

        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothing);
    }

    private void UpdateAnimation()
    {
        if (!isGrounded && !isIdle)
        {
            currentState = STATE_JUMPING;
            animator.SetInteger("state", STATE_JUMPING);
            return;
        }

        animator.SetInteger("state", currentState);
    }

    private void HandleStateTransitions()
    {
        if (currentState == targetState || !isGrounded) return;

        stateTransitionTimer -= Time.deltaTime;

        if (stateTransitionTimer <= 0f)
        {
            currentState = targetState;
        }
    }

    private void SetTargetState(int newState)
    {
        if (targetState == newState) return;

        targetState = newState;

        if (currentState == STATE_IDLE && targetState == STATE_WALKING)
        {
            stateTransitionTimer = idleToWalkDuration;
        }
        else if (currentState == STATE_WALKING && targetState == STATE_RUNNING)
        {
            stateTransitionTimer = walkToRunDuration;
        }
        else if (currentState == STATE_RUNNING && targetState == STATE_WALKING)
        {
            stateTransitionTimer = runToWalkDuration;
        }
        else if (currentState == STATE_WALKING && targetState == STATE_IDLE)
        {
            stateTransitionTimer = walkToIdleDuration;
        }
        else
        {
            stateTransitionTimer = 0f;
            currentState = targetState;
        }
    }

    private void ApplyGravityModifiers()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isGrounded)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2f - 1) * Time.deltaTime;
        }
    }
    #endregion

    #region Coroutines
    IEnumerator PerformCenteredJump()
    {
        waitingForObstacle = true;
        moveEnabled = false;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        yield return new WaitForSeconds(0.1f);

        float heightDifference = playerTarget.transform.position.y - transform.position.y;
        float requiredJumpHeight = Mathf.Clamp(heightDifference * 1.2f, jumpHeight, jumpHeight * 1.5f);

        yield return StartCoroutine(Jump("Up", movingRight, requiredJumpHeight));

        waitingForObstacle = false;
        moveEnabled = true;
    }

    IEnumerator Jump(string type, bool dirRight, float jumpForce)
    {
        if (!canJump || isIdle) yield break;

        canJump = false;
        isGrounded = false;
        isIdle = false;
        currentState = STATE_JUMPING;
        targetState = STATE_JUMPING;

        Vector2 jumpVelocity = new Vector2(dirRight ? jumpForce / 2 : -jumpForce / 2, jumpForce);

        if (type == "Up")
        {
            jumpVelocity = new Vector2(0, jumpForce);
            isJumpingUpward = true;
        }

        rb.linearVelocity = jumpVelocity;

        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
        isJumpingUpward = false;
    }
    #endregion

    #region Debugging
    void OnDrawGizmos()
    {
        if (!DEBUGMODE) return;
        if (playerTarget == null) return;

        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(playerTarget.transform.position, walkDistanceThreshold);

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(playerTarget.transform.position, idleDistanceThreshold);

        Gizmos.color = upwardInfo.collider ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * upwardRayLength);

        Vector2 circleCenter = (Vector2)transform.position + Vector2.up * upwardRayLength;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(circleCenter, upwardRayCircleRadius);

        Vector2 leftGroundPos = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y);
        Vector2 rightGroundPos = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y);

        Gizmos.color = leftInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(leftGroundPos, leftGroundPos + Vector2.down * groundRayLength);

        Gizmos.color = rightInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(rightGroundPos, rightGroundPos + Vector2.down * groundRayLength);

        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawLine(transform.position, playerTarget.transform.position);
    }
    #endregion
}
