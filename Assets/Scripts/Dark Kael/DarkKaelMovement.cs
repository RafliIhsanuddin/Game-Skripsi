using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class DarkKaelMovement : MonoBehaviour
{
    #region Variables
    // Animation states
    private const int STATE_IDLE = 0;
    private const int STATE_WALKING = 1;
    private const int STATE_RUNNING = 2;
    private const int STATE_JUMPING = 3;
    private const int STATE_ATTACKING = 4;

    [Header("Stats")]
    public float jumpHeight = 10f;
    public float attackRange = 3f;
    public float attackCooldown = 2f;

    [Header("Movement Settings")]
    [SerializeField] private float walkDistanceThreshold = 5f;
    [SerializeField] private float idleDistanceThreshold = 2f;
    [SerializeField] private float runSpeed = 16f;
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float movementSmoothing = 0.05f;
    [SerializeField] private float playerTrackingAggression = 0.8f; // How aggressively he tracks the player (0-1)
    [SerializeField] private bool enableIdleState = true;
    [SerializeField] private bool enableWalkingState = true;

    [Header("Jump Settings")]
    [SerializeField] private float jumpCooldown = 0.4f;
    [SerializeField] private float groundCheckDelayAfterJump = 0.2f;
    [SerializeField] private float jumpPredictionDistance = 2f; // How far ahead to predict jumps

    [Header("Ground Detection")]
    [SerializeField] private float groundRayLength = 3.5f;
    [SerializeField] private float groundRayHorizontalOffset = 0.5f;

    [Header("Wall Detection")]
    [SerializeField] private float wallRayLength = 2f;
    [SerializeField] private float rayHeight = 1.5f;

    [Header("DEBUGING")]
    public bool DEBUGMODE = false;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlayer;

    // Internal variables
    private float speed;
    private bool movingRight = true;
    private bool canJump = true;
    private bool canAttack = true;
    private bool moveEnabled = true;
    private bool isGrounded = false;
    private bool isIdle = false;
    private bool isAttacking = false;
    private int currentState = STATE_IDLE;
    private int targetState = STATE_IDLE;
    private Vector2 currentVelocity = Vector2.zero;
    private float lastAttackTime = 0f;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Transform playerTarget;

    // Raycast hits
    private RaycastHit2D leftInfoGround;
    private RaycastHit2D rightInfoGround;
    private RaycastHit2D leftInfoWall;
    private RaycastHit2D rightInfoWall;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) playerTarget = player.transform;

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
        HandleCombat();
        HandleMovementState();
        HandleJumpLogic();
        UpdateAnimation();
        ApplyGravityModifiers();
    }

    void FixedUpdate()
    {
        if (!isGrounded || isAttacking) return;
        HandleMovement();
    }

    #region Combat
    private void HandleCombat()
    {
        if (!canAttack || Time.time < lastAttackTime + attackCooldown) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
        bool playerInRange = distanceToPlayer <= attackRange;
        bool facingPlayer = (movingRight && playerTarget.position.x > transform.position.x) ||
                          (!movingRight && playerTarget.position.x < transform.position.x);

        if (playerInRange && facingPlayer)
        {
            StartCoroutine(PerformAttack());
        }
    }

    IEnumerator PerformAttack()
    {
        isAttacking = true;
        canAttack = false;
        moveEnabled = false;
        SetTargetState(STATE_ATTACKING);
        lastAttackTime = Time.time;

        // Trigger attack animation and logic
        animator.SetTrigger("Attack");

        // Wait for attack animation to complete (adjust time as needed)
        yield return new WaitForSeconds(0.8f);

        isAttacking = false;
        moveEnabled = true;
        canAttack = true;
    }
    #endregion

    #region Movement
    private void HandleMovementState()
    {
        if (isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);

        if (enableIdleState && distanceToPlayer <= idleDistanceThreshold)
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

            float newSpeed = enableWalkingState && distanceToPlayer <= walkDistanceThreshold ? walkSpeed : runSpeed;
            if (newSpeed != speed)
            {
                speed = newSpeed;
                SetTargetState(enableWalkingState && newSpeed == walkSpeed ? STATE_WALKING : STATE_RUNNING);
            }
        }

        // More aggressive tracking of player position
        float directionThreshold = playerTrackingAggression * distanceToPlayer;
        bool newDirection = (playerTarget.position.x - transform.position.x) >= 0;

        if (Mathf.Abs(playerTarget.position.x - transform.position.x) > directionThreshold)
        {
            if (newDirection != movingRight)
            {
                movingRight = newDirection;
                spriteRenderer.flipX = !movingRight;
            }
        }
    }

    private void HandleMovement()
    {
        if (!moveEnabled || isAttacking) return;

        Vector2 directionToPlayer = (playerTarget.position - transform.position).normalized;
        float horizontalMovement = directionToPlayer.x * speed;

        // Add some prediction to movement based on player velocity
        Rigidbody2D playerRB = playerTarget.GetComponent<Rigidbody2D>();
        if (playerRB != null)
        {
            float prediction = playerRB.linearVelocity.x * Time.deltaTime * jumpPredictionDistance;
            horizontalMovement = Mathf.Clamp(horizontalMovement + prediction, -speed, speed);
        }

        Vector2 targetVelocity = new Vector2(horizontalMovement, rb.linearVelocity.y);
        rb.linearVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, movementSmoothing);
    }
    #endregion

    #region Jumping
    private void HandleJumpLogic()
    {
        if (isIdle || !canJump || !isGrounded || isAttacking) return;

        // Jump when player is above and we're close enough
        bool playerAbove = playerTarget.position.y > transform.position.y + 2f;
        float horizontalDistance = Mathf.Abs(playerTarget.position.x - transform.position.x);

        if (playerAbove && horizontalDistance < 3f)
        {
            StartCoroutine(Jump(jumpHeight * 1.2f));
            return;
        }

        // Jump over gaps
        if (leftInfoGround.collider == false && rightInfoGround.collider == true)
        {
            if (leftInfoWall.collider == false) StartCoroutine(Jump(jumpHeight));
        }
        else if (leftInfoGround.collider == true && rightInfoGround.collider == false)
        {
            if (rightInfoWall.collider == false) StartCoroutine(Jump(jumpHeight));
        }

        // Jump over walls
        if (isGrounded)
        {
            if (leftInfoWall.collider == true && leftInfoWall.distance <= wallRayLength / 1.5f && !movingRight)
                StartCoroutine(Jump(jumpHeight));

            if (rightInfoWall.collider == true && rightInfoWall.distance <= wallRayLength / 1.5f && movingRight)
                StartCoroutine(Jump(jumpHeight));
        }
    }

    IEnumerator Jump(float jumpForce)
    {
        if (!canJump) yield break;

        canJump = false;
        isGrounded = false;
        isIdle = false;
        currentState = STATE_JUMPING;
        targetState = STATE_JUMPING;

        Vector2 jumpVelocity = new Vector2(movingRight ? jumpForce / 2 : -jumpForce / 2, jumpForce);
        rb.linearVelocity = jumpVelocity;

        yield return new WaitForSeconds(jumpCooldown);
        canJump = true;
    }
    #endregion

    #region Utility
    private void UpdateRaycasts()
    {
        Vector2 leftGroundPos = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y);
        Vector2 rightGroundPos = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y);

        leftInfoGround = Physics2D.Raycast(leftGroundPos, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(rightGroundPos, Vector2.down, groundRayLength, whatIsGround);

        Vector2 leftWallPos = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y + rayHeight);
        Vector2 rightWallPos = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y + rayHeight);

        leftInfoWall = Physics2D.Raycast(leftWallPos, Vector2.left, wallRayLength, whatIsGround);
        rightInfoWall = Physics2D.Raycast(rightWallPos, Vector2.right, wallRayLength, whatIsGround);

        bool wasGrounded = isGrounded;
        isGrounded = leftInfoGround.collider != null || rightInfoGround.collider != null;

        if (!wasGrounded && isGrounded)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTarget.position);
                SetTargetState(distanceToPlayer <= walkDistanceThreshold ? STATE_WALKING : STATE_RUNNING);
            }
            else
            {
                SetTargetState(STATE_IDLE);
            }
        }

        Debug.DrawRay(leftWallPos, Vector2.left * wallRayLength, leftInfoWall.collider ? Color.green : Color.red);
        Debug.DrawRay(rightWallPos, Vector2.right * wallRayLength, rightInfoWall.collider ? Color.green : Color.red);
    }

    private void UpdateAnimation()
    {
        if (isAttacking)
        {
            currentState = STATE_ATTACKING;
        }
        else if (!isGrounded)
        {
            currentState = STATE_JUMPING;
        }
        else if (Mathf.Abs(rb.linearVelocity.x) < 0.1f)
        {
            currentState = STATE_IDLE;
        }
        else
        {
            currentState = targetState;
        }

        animator.SetInteger("state", currentState);
    }

    private void SetTargetState(int newState)
    {
        if (targetState == newState) return;
        targetState = newState;
    }

    private void ApplyGravityModifiers()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.8f - 1) * Time.deltaTime;
        }
        else if (rb.linearVelocity.y > 0 && !isGrounded)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.2f - 1) * Time.deltaTime;
        }
    }
    #endregion

    #region Debugging
    void OnDrawGizmos()
    {
        if (!DEBUGMODE) return;

        if (playerTarget != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f);
            Gizmos.DrawWireSphere(playerTarget.position, walkDistanceThreshold);
            Gizmos.DrawWireSphere(playerTarget.position, attackRange);

            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawWireSphere(playerTarget.position, idleDistanceThreshold);
        }

        Vector2 leftGroundPos = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y);
        Vector2 rightGroundPos = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y);

        Gizmos.color = leftInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(leftGroundPos, leftGroundPos + Vector2.down * groundRayLength);

        Gizmos.color = rightInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(rightGroundPos, rightGroundPos + Vector2.down * groundRayLength);

        Vector2 leftWallPos = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y + rayHeight);
        Vector2 rightWallPos = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y + rayHeight);

        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawLine(leftWallPos, leftWallPos + Vector2.left * wallRayLength);
        Gizmos.DrawLine(rightWallPos, rightWallPos + Vector2.right * wallRayLength);

        if (playerTarget != null)
        {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawLine(transform.position, playerTarget.position);
        }
    }
    #endregion
}