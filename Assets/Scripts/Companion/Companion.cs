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

    [Header("State Transition Durations")]
    [SerializeField] private float idleToWalkDuration = 0.2f;
    [SerializeField] private float walkToRunDuration = 0.3f;
    [SerializeField] private float runToWalkDuration = 0.2f;
    [SerializeField] private float walkToIdleDuration = 0.15f;

    [Header("Jump Settings")]
    [SerializeField] private float upwardRayLength = 5f;
    [SerializeField] private float obstacleWaitTime = 1f;
    [SerializeField] private float jumpCooldown = 0.5f;
    [SerializeField] private float groundCheckDelayAfterJump = 0.2f;
    [SerializeField] private float upwardRayCircleRadius = 0.5f; // New variable for debug circle radius

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
    private int currentState = STATE_IDLE;
    private float stateTransitionTimer = 0f;
    private int targetState = STATE_IDLE;
    private bool wasGroundedLastFrame = false;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private GameObject nearestTarget;
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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();

        // Get child transforms for raycasting
        if (right == null) right = transform.GetChild(0);
        if (left == null) left = transform.GetChild(1);

        // Find Kael FBF by name
        nearestTarget = GameObject.Find("Kael FBF");
        if (nearestTarget == null)
        {
            Debug.LogError("Could not find GameObject named 'Kael FBF'!");
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
        StartCoroutine(CheckIfStanding());
    }

    void Update()
    {
        if (nearestTarget == null) return;

        HandleMovementState();
        UpdateRaycasts();

        // Only handle jump logic if we're not in idle state
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
        if (!isGrounded || waitingForObstacle || isIdle) return;

        HandleMovement();
    }

    #region Core Logic
    private void HandleMovementState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, nearestTarget.transform.position);

        // Check for idle state - this now has highest priority
        if (distanceToPlayer <= idleDistanceThreshold && isGrounded && !waitingForObstacle)
        {
            if (!isIdle)
            {
                isIdle = true;
                moveEnabled = false;
                speed = 0;
                SetTargetState(STATE_IDLE);
                // Cancel any jump in progress
                StopAllCoroutines();
                canJump = true;
                isJumpingUpward = false;
            }
            return; // Exit early if we're in idle state
        }
        else
        {
            // Exit idle state if conditions change
            if (isIdle)
            {
                isIdle = false;
                moveEnabled = true;
            }

            // Set movement speed based on distance
            float newSpeed = distanceToPlayer <= walkDistanceThreshold ? walkSpeed : runSpeed;
            if (newSpeed != speed)
            {
                speed = newSpeed;
                SetTargetState(newSpeed == walkSpeed ? STATE_WALKING : STATE_RUNNING);
            }
        }

        // Update direction based on target position
        if (nearestTarget != null)
        {
            movingRight = (nearestTarget.transform.position.x - transform.position.x) >= 0;
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

        // Calculate position of debug circle at end of upward ray
        Vector2 circleCenter = (Vector2)transform.position + Vector2.up * upwardRayLength;

        // Check if the debug circle overlaps with the player
        bool playerInCircle = Vector2.Distance(circleCenter, nearestTarget.transform.position) <= upwardRayCircleRadius;

        // Check if there's an obstacle above AND the debug circle overlaps with the player
        if (upwardInfo.collider != null && playerInCircle)
        {
            // Wait before attempting to jump over obstacle
            if (!waitingForObstacle)
            {
                StartCoroutine(WaitUnderObstacle());
            }
        }
        // Handle edge jumps (only if not jumping upward)
        else if (!isJumpingUpward)
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

        // Handle wall jumps (only if not jumping upward)
        if (isGrounded && !isJumpingUpward)
        {
            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false && !movingRight)
                StartCoroutine(Jump("Large", movingRight, jumpHeight));

            if (rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && rightInfoUp.collider == false && movingRight)
                StartCoroutine(Jump("Large", movingRight, jumpHeight));
        }
    }

    private void HandleMovement()
    {
        if (movingRight)
        {
            spriteRenderer.flipX = false;
            Vector2 force = Vector2.right * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + force);
        }
        else
        {
            spriteRenderer.flipX = true;
            Vector2 force = Vector2.left * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + force);
        }
    }

    private void UpdateAnimation()
    {
        // Jumping has highest priority, unless we're in idle state
        if (!isGrounded && !isIdle)
        {
            currentState = STATE_JUMPING;
            animator.SetInteger("state", STATE_JUMPING);
            return;
        }

        // Apply the current state (which may be transitioning)
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

        // Set appropriate transition time based on current and target states
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
            // Default transition (immediate)
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
    IEnumerator CheckIfStanding()
    {
        Vector3 lastPos = transform.position;
        yield return new WaitForSeconds(1);

        if (Vector3.Distance(lastPos, transform.position) < 0.1f && isGrounded && !waitingForObstacle)
        {
            SetTargetState(STATE_IDLE);
            if (!isIdle) StartCoroutine(Jump("Small", movingRight, jumpHeight / 2));
        }

        StartCoroutine(CheckIfStanding());
    }

    IEnumerator WaitUnderObstacle()
    {
        waitingForObstacle = true;
        isIdle = true;
        moveEnabled = false;
        speed = 0;
        SetTargetState(STATE_IDLE);

        yield return new WaitForSeconds(obstacleWaitTime);

        // After waiting, check if we still need to jump
        Vector2 circleCenter = (Vector2)transform.position + Vector2.up * upwardRayLength;
        bool playerInCircle = Vector2.Distance(circleCenter, nearestTarget.transform.position) <= upwardRayCircleRadius;

        if (upwardInfo.collider != null && playerInCircle &&
            isGrounded && canJump)
        {
            float heightDifference = nearestTarget.transform.position.y - transform.position.y;
            float requiredJumpHeight = Mathf.Clamp(heightDifference * 1.2f, jumpHeight, maxJumpHeight);
            StartCoroutine(Jump("Up", movingRight, requiredJumpHeight));
        }

        waitingForObstacle = false;
        isIdle = false;
        moveEnabled = true;
    }

    IEnumerator Jump(string size, bool dirRight, float jumpForce)
    {
        if (!canJump || isIdle) yield break; // Don't jump if we're in idle state

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
                jumpVelocity = new Vector2(0, jumpForce); // Straight up
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
        if (nearestTarget == null) return;

        // Draw distance thresholds
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(nearestTarget.transform.position, walkDistanceThreshold);

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(nearestTarget.transform.position, idleDistanceThreshold);

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
        Gizmos.DrawLine(transform.position, nearestTarget.transform.position);
    }
    #endregion
}
