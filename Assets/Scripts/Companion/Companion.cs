using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class Companion : MonoBehaviour
{
    #region Variables
    // Animation states
    private const int STATE_IDLE = 0;
    private const int STATE_WALKING = 1;
    private const int STATE_RUNNING = 2;
    private const int STATE_JUMPING = 3;

    [Header("Stats")]
    public float jumpHeight = 8f;                                                       // the height we can jump
    public float maxJumpHeight = 12f;                                                  // maximum height for upward jumps

    [Header("Movement Settings")]
    [SerializeField] private float walkDistanceThreshold = 5f;                         // Distance to switch to walking
    [SerializeField] private float idleDistanceThreshold = 1f;                         // Distance to switch to idle
    [SerializeField] private float runSpeed = 14f;                                     // Speed when running
    [SerializeField] private float walkSpeed = 7f;                                     // Speed when walking

    [Header("Jump Settings")]
    [SerializeField] private float upwardRayLength = 5f;                               // Length of upward raycast
    [SerializeField] private float heightDifferenceThreshold = 2f;                     // How much higher Kael needs to be to trigger jump
    [SerializeField] private float obstacleWaitTime = 1f;                              // Time to wait when obstacle is detected above
    [SerializeField] private float jumpCooldown = 0.5f;                                // Time between jumps

    [Header("Ground Detection")]
    [SerializeField] private float groundRayLength = 3.5f;                             // the length for ray casts that face the ground
    [SerializeField] private float longGroundRayLength = 14f;                          // the length for long ground ray casts

    [Header("Wall Detection")]
    [SerializeField] private float wallRayLength = 30f;                                // the length for ray casts that face the wall
    [SerializeField] private float rayHeight = 22.5f;

    [Header("DEBUGING")]
    public bool DEBUGMODE = false;

    [Header("Components")]
    [SerializeField] private Animator animator;                                        // Animator from another GameObject
    [SerializeField] private SpriteRenderer spriteRenderer;                            // SpriteRenderer from another GameObject
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

    private Rigidbody2D rb;
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
        HandleJumpLogic();
        UpdateAnimation();
        ApplyGravityModifiers();
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

        // Check for idle state
        if (distanceToPlayer <= idleDistanceThreshold && isGrounded && !waitingForObstacle)
        {
            if (!isIdle)
            {
                isIdle = true;
                moveEnabled = false;
                speed = 0;
            }
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
            speed = distanceToPlayer <= walkDistanceThreshold ? walkSpeed : runSpeed;
        }

        // Update direction based on target position
        if (nearestTarget != null)
        {
            movingRight = (nearestTarget.transform.position.x - transform.position.x) >= 0;
        }
    }

    private void UpdateRaycasts()
    {
        // Ground detection
        leftInfoGround = Physics2D.Raycast(left.position, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(right.position, Vector2.down, groundRayLength, whatIsGround);
        leftInfoLongGround = Physics2D.Raycast(new Vector2(left.position.x - 2, left.position.y), Vector2.down, longGroundRayLength, whatIsGround);
        rightInfoLongGround = Physics2D.Raycast(new Vector2(right.position.x + 2, right.position.y), Vector2.down, longGroundRayLength, whatIsGround);

        // Wall detection
        leftInfoUp = Physics2D.Raycast(new Vector2(left.position.x, left.position.y + rayHeight), Vector2.left, wallRayLength, whatIsGround);
        rightInfoUp = Physics2D.Raycast(new Vector2(right.position.x, right.position.y + rayHeight), Vector2.right, wallRayLength, whatIsGround);
        leftInfo = Physics2D.Raycast(left.position, Vector2.left, wallRayLength, whatIsGround);
        rightInfo = Physics2D.Raycast(right.position, Vector2.right, wallRayLength, whatIsGround);

        // Upward detection
        upwardInfo = Physics2D.Raycast(transform.position, Vector2.up, upwardRayLength, whatIsGround);

        // Update grounded state
        isGrounded = leftInfoGround.collider != null || rightInfoGround.collider != null;
    }

    private void HandleJumpLogic()
    {
        if (isIdle || waitingForObstacle || !canJump || !isGrounded) return;

        // Check if player is significantly above us
        bool playerIsAbove = nearestTarget.transform.position.y > transform.position.y + heightDifferenceThreshold;

        if (playerIsAbove)
        {
            // Check if there's an obstacle above
            if (upwardInfo.collider != null)
            {
                // Wait before attempting to jump over obstacle
                if (!waitingForObstacle)
                {
                    StartCoroutine(WaitUnderObstacle());
                }
            }
            else
            {
                // Calculate required jump height
                float heightDifference = nearestTarget.transform.position.y - transform.position.y;
                float requiredJumpHeight = Mathf.Clamp(heightDifference * 1.2f, jumpHeight, maxJumpHeight);

                // Perform upward jump
                StartCoroutine(Jump("Up", movingRight, requiredJumpHeight));
                isJumpingUpward = true;
            }
        }

        // Handle edge jumps
        if (!isJumpingUpward)
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

        // Handle wall jumps
        if (isGrounded)
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
        if (!isGrounded)
        {
            animator.SetInteger("state", STATE_JUMPING);
            isIdle = false;
        }
        else if (isIdle)
        {
            animator.SetInteger("state", STATE_IDLE);
        }
        else
        {
            animator.SetInteger("state", speed == walkSpeed ? STATE_WALKING : STATE_RUNNING);
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
            animator.SetInteger("state", STATE_IDLE);
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
        animator.SetInteger("state", STATE_IDLE);

        yield return new WaitForSeconds(obstacleWaitTime);

        // After waiting, check if we still need to jump
        if (nearestTarget.transform.position.y > transform.position.y + heightDifferenceThreshold &&
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
        if (!canJump) yield break;

        canJump = false;
        isGrounded = false;
        isIdle = false;
        animator.SetInteger("state", STATE_JUMPING);

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
        if (nearestTarget == null || left == null || right == null) return;

        // Draw distance thresholds
        Gizmos.color = new Color(1, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(nearestTarget.transform.position, walkDistanceThreshold);

        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawWireSphere(nearestTarget.transform.position, idleDistanceThreshold);

        // Draw all rays
        Gizmos.color = upwardInfo.collider ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * upwardRayLength);

        // Ground rays
        Gizmos.color = leftInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(left.position, left.position + Vector3.down * groundRayLength);

        Gizmos.color = rightInfoGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(right.position, right.position + Vector3.down * groundRayLength);

        // Long ground rays
        Vector3 longLeftPos = new Vector3(left.position.x - 2, left.position.y, left.position.z);
        Vector3 longRightPos = new Vector3(right.position.x + 2, right.position.y, right.position.z);

        Gizmos.color = leftInfoLongGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(longLeftPos, longLeftPos + Vector3.down * longGroundRayLength);

        Gizmos.color = rightInfoLongGround.collider ? Color.green : Color.red;
        Gizmos.DrawLine(longRightPos, longRightPos + Vector3.down * longGroundRayLength);

        // Wall rays
        Gizmos.color = leftInfo.collider ? Color.green : Color.red;
        Gizmos.DrawLine(left.position, left.position + Vector3.left * wallRayLength);

        Gizmos.color = rightInfo.collider ? Color.green : Color.red;
        Gizmos.DrawLine(right.position, right.position + Vector3.right * wallRayLength);

        // Upper wall rays
        Vector3 upperLeftPos = new Vector3(left.position.x, left.position.y + rayHeight, left.position.z);
        Vector3 upperRightPos = new Vector3(right.position.x, right.position.y + rayHeight, right.position.z);

        Gizmos.color = leftInfoUp.collider ? Color.green : Color.red;
        Gizmos.DrawLine(upperLeftPos, upperLeftPos + Vector3.left * wallRayLength);

        Gizmos.color = rightInfoUp.collider ? Color.green : Color.red;
        Gizmos.DrawLine(upperRightPos, upperRightPos + Vector3.right * wallRayLength);

        // Target line
        Gizmos.color = new Color(0, 1, 1, 0.5f);
        Gizmos.DrawLine(transform.position, nearestTarget.transform.position);
    }
    #endregion
}
