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
    public float maxJumpHeight = 12f;

    [Header("Movement Settings")]
    [SerializeField] private float walkDistanceThreshold = 5f;
    [SerializeField] private float idleDistanceThreshold = 1f;
    [SerializeField] private float runSpeed = 14f;
    [SerializeField] private float walkSpeed = 7f;
    [SerializeField] private float centerAlignSpeed = 20f; // Faster speed for centering
    [SerializeField] private float movementSmoothing = 0.05f;
    [SerializeField] private float centerPositionTolerance = 0.1f; // How close we need to be to center

    [Header("State Transition Durations")]
    [SerializeField] private float idleToWalkDuration = 0.1f; // Faster transitions
    [SerializeField] private float walkToRunDuration = 0.2f;
    [SerializeField] private float runToWalkDuration = 0.1f;
    [SerializeField] private float walkToIdleDuration = 0.1f;

    [Header("Jump Settings")]
    [SerializeField] private float upwardRayLength = 5f;
    [SerializeField] private float obstacleWaitTime = 0.5f; // Shorter wait time
    [SerializeField] private float jumpCooldown = 0.3f; // Shorter cooldown
    [SerializeField] private float groundCheckDelayAfterJump = 0.2f;
    [SerializeField] private float upwardRayCircleRadius = 0.5f;

    [Header("Ground Detection")]
    [SerializeField] private float groundRayLength = 3.5f;
    [SerializeField] private float longGroundRayLength = 14f;
    [SerializeField] private float groundRayHorizontalOffset = 0.5f;
    [SerializeField] private float longGroundRayHorizontalOffset = 1f;

    [Header("Wall Detection")]
    [SerializeField] private float wallRayLength = 30f;
    [SerializeField] private float rayHeight = 22.5f;

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

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private GameObject playerTarget;
    private Transform left;
    private Transform right;

    // Raycast hits
    private RaycastHit2D leftInfoGround;
    private RaycastHit2D rightInfoGround;
    private RaycastHit2D leftInfoLongGround;
    private RaycastHit2D rightInfoLongGround;
    private RaycastHit2D leftInfoUp;
    private RaycastHit2D rightInfoUp;
    private RaycastHit2D leftInfo;
    private RaycastHit2D rightInfo;
    private RaycastHit2D upwardInfo;
    #endregion

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        
        

        // Find player by tag (more reliable than name)
        playerTarget = GameObject.FindGameObjectWithTag("Player");
        if (playerTarget == null)
        {
            Debug.LogError("Could not find player GameObject with tag 'Player'!");
            enabled = false;
            return;
        }

        // Validate required components
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

    #region Core Logic
    private void CheckForPlayerAbove()
    {
        // Calculate position of debug circle at end of upward ray
        Vector2 circleCenter = (Vector2)transform.position + Vector2.up * upwardRayLength;
        bool playerInCircle = Vector2.Distance(circleCenter, playerTarget.transform.position) <= upwardRayCircleRadius;

        // If player is above us and we're not already centering/jumping
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

        // If we need to center beneath the player
        if (needsToCenter)
        {
            // Check if we're centered enough (X position only)
            if (Mathf.Abs(transform.position.x - playerTarget.transform.position.x) < centerPositionTolerance)
            {
                needsToCenter = false;
                // Immediately jump if there's an obstacle above
                if (upwardInfo.collider != null)
                {
                    StartCoroutine(PerformCenteredJump());
                }
                return;
            }

            // Move quickly to center position
            speed = centerAlignSpeed;
            SetTargetState(STATE_RUNNING);

            // Update direction
            bool newDirection = (playerTarget.transform.position.x - transform.position.x) >= 0;
            if (newDirection != movingRight)
            {
                movingRight = newDirection;
                spriteRenderer.flipX = !movingRight;
            }
            return;
        }

        // Normal movement states when not centering
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

        // Only update direction when not idle
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
        // Calculate raycast positions with offsets
        Vector2 leftGroundPos = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y);
        Vector2 rightGroundPos = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y);
        Vector2 leftLongGroundPos = new Vector2(transform.position.x - longGroundRayHorizontalOffset, transform.position.y);
        Vector2 rightLongGroundPos = new Vector2(transform.position.x + longGroundRayHorizontalOffset, transform.position.y);

        // Ground detection
        leftInfoGround = Physics2D.Raycast(leftGroundPos, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(rightGroundPos, Vector2.down, groundRayLength, whatIsGround);
        leftInfoLongGround = Physics2D.Raycast(leftLongGroundPos, Vector2.down, longGroundRayLength, whatIsGround);
        rightInfoLongGround = Physics2D.Raycast(rightLongGroundPos, Vector2.down, longGroundRayLength, whatIsGround);

        // Wall detection
        Vector2 leftWallPos = new Vector2(transform.position.x, transform.position.y + rayHeight);
        Vector2 rightWallPos = new Vector2(transform.position.x, transform.position.y + rayHeight);
        leftInfoUp = Physics2D.Raycast(leftWallPos, Vector2.left, wallRayLength, whatIsGround);
        rightInfoUp = Physics2D.Raycast(rightWallPos, Vector2.right, wallRayLength, whatIsGround);
        leftInfo = Physics2D.Raycast(leftGroundPos, Vector2.left, wallRayLength, whatIsGround);
        rightInfo = Physics2D.Raycast(rightGroundPos, Vector2.right, wallRayLength, whatIsGround);

        // Upward detection
        upwardInfo = Physics2D.Raycast(transform.position, Vector2.up, upwardRayLength, whatIsGround);

        // Update grounded state
        isGrounded = leftInfoGround.collider != null || rightInfoGround.collider != null;
    }

    private void HandleJumpLogic()
    {
        if (isIdle || waitingForObstacle || !canJump || !isGrounded) return;

        // Handle edge jumps (only if not jumping upward and not centering)
        if (!isJumpingUpward && !needsToCenter)
        {
            if (leftInfoGround.collider == false && rightInfoGround.collider == true && leftInfoLongGround.collider == false)
            {
                if (leftInfo.collider == false) StartCoroutine(Jump("Large", false, jumpHeight));
            }
            else if (leftInfoGround.collider == false && rightInfoGround.collider == true && leftInfoLongGround.collider == true)
            {
                StartCoroutine(Jump("Small", false, jumpHeight / 2));
            }
            else if (leftInfoGround.collider == true && rightInfoGround.collider == false && rightInfoLongGround.collider == false)
            {
                if (rightInfo.collider == false) StartCoroutine(Jump("Large", true, jumpHeight));
            }
            else if (leftInfoGround.collider == true && rightInfoGround.collider == false && rightInfoLongGround.collider == true)
            {
                StartCoroutine(Jump("Small", true, jumpHeight / 2));
            }
        }

        // Handle wall jumps (only if not jumping upward and not centering)
        if (isGrounded && !isJumpingUpward && !needsToCenter)
        {
            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false && !movingRight)
                StartCoroutine(Jump("Large", movingRight, jumpHeight));

            if (rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && rightInfoUp.collider == false && movingRight)
                StartCoroutine(Jump("Large", movingRight, jumpHeight));
        }
    }

    private void HandleMovement()
    {
        if (!moveEnabled) return;

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
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Stop horizontal movement

        // Wait a brief moment to ensure we're properly centered
        yield return new WaitForSeconds(0.1f);

        // Calculate required jump height based on player position
        float heightDifference = playerTarget.transform.position.y - transform.position.y;
        float requiredJumpHeight = Mathf.Clamp(heightDifference * 1.2f, jumpHeight, maxJumpHeight);

        // Perform the jump
        yield return StartCoroutine(Jump("Up", movingRight, requiredJumpHeight));

        waitingForObstacle = false;
        moveEnabled = true;
    }

    IEnumerator Jump(string size, bool dirRight, float jumpForce)
    {
        if (!canJump || isIdle) yield break;

        canJump = false;
        isGrounded = false;
        isIdle = false;
        currentState = STATE_JUMPING;
        targetState = STATE_JUMPING;

        Vector2 jumpVelocity = Vector2.zero;

        switch (size)
        {
            case "Large":
                jumpVelocity = new Vector2(dirRight ? jumpForce / 2 : -jumpForce / 2, jumpForce);
                break;
            case "Small":
                jumpVelocity = new Vector2(dirRight ? jumpForce / 2 : -jumpForce / 2, jumpForce / 2);
                break;
            case "Up":
                jumpVelocity = new Vector2(0, jumpForce);
                isJumpingUpward = true;
                break;
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

        // Draw distance thresholds
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(playerTarget.transform.position, walkDistanceThreshold);

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(playerTarget.transform.position, idleDistanceThreshold);

        // Draw all rays
        Gizmos.color = upwardInfo.collider ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * upwardRayLength);

        // Draw debug circle at end of upward ray
        Vector2 circleCenter = (Vector2)transform.position + Vector2.up * upwardRayLength;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(circleCenter, upwardRayCircleRadius);

        // Calculate ray positions for visualization
        Vector2 leftGroundPos = new Vector2(transform.position.x - groundRayHorizontalOffset, transform.position.y);
        Vector2 rightGroundPos = new Vector2(transform.position.x + groundRayHorizontalOffset, transform.position.y);
        Vector2 leftLongGroundPos = new Vector2(transform.position.x - longGroundRayHorizontalOffset, transform.position.y);
        Vector2 rightLongGroundPos = new Vector2(transform.position.x + longGroundRayHorizontalOffset, transform.position.y);

        // Ground rays
        Gizmos.color = leftInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(leftGroundPos, leftGroundPos + Vector2.down * groundRayLength);

        Gizmos.color = rightInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(rightGroundPos, rightGroundPos + Vector2.down * groundRayLength);

        // Long ground rays
        Gizmos.color = leftInfoLongGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(leftLongGroundPos, leftLongGroundPos + Vector2.down * longGroundRayLength);

        Gizmos.color = rightInfoLongGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(rightLongGroundPos, rightLongGroundPos + Vector2.down * longGroundRayLength);

        // Wall rays
        Gizmos.color = leftInfo.collider ? Color.green : Color.red;
        Gizmos.DrawLine(leftGroundPos, leftGroundPos + Vector2.left * wallRayLength);

        Gizmos.color = rightInfo.collider ? Color.green : Color.red;
        Gizmos.DrawLine(rightGroundPos, rightGroundPos + Vector2.right * wallRayLength);

        // Upper wall rays
        Vector2 leftWallPos = new Vector2(transform.position.x, transform.position.y + rayHeight);
        Vector2 rightWallPos = new Vector2(transform.position.x, transform.position.y + rayHeight);

        Gizmos.color = leftInfoUp.collider ? Color.green : Color.red;
        Gizmos.DrawLine(leftWallPos, leftWallPos + Vector2.left * wallRayLength);

        Gizmos.color = rightInfoUp.collider ? Color.green : Color.red;
        Gizmos.DrawLine(rightWallPos, rightWallPos + Vector2.right * wallRayLength);

        // Target line
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawLine(transform.position, playerTarget.transform.position);
    }
    #endregion
}
