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
    public float minSpeed = 10;                                                         // determine min speed
    public float maxSpeed = 14;                                                         // determine max speed
    public float jumpHeight = 8;                                                        // the height we can jump
    float speed;                                                                        // current speed
    float gravity;

    [Header("Movement Settings")]
    [SerializeField] private float walkDistanceThreshold = 5f;                          // Distance to switch to walking
    [SerializeField] private float idleDistanceThreshold = 1f;                          // Distance to switch to idle
    [SerializeField] private float runSpeed = 14f;                                     // Speed when moving (no distance threshold)
    [SerializeField] private float walkSpeed = 7f;                                      // Speed when walking

    [Header("DEBUGING")]
    public bool DEBUGMODE = false;
    [Header("Booleans")]
    private bool movingRight = true;
    private bool jumpRight = true;
    private bool canJump = true;
    private bool moveEnabled = true;
    private bool isGrounded = false;
    private bool isInFollowRange = true; // Always true now
    private bool isIdle = false; // New flag to track idle state

    [Header("Raycast Settings")]
    [Header("Ground Detection")]
    public float groundRayLength = 3.5f;                                                // the length for ray casts that face the ground
    public float longGroundRayLength = 14f;                                             // the length for long ground ray casts

    [Header("Wall Detection")]
    public float wallRayLength = 30;                                                    // the length for ray casts that face the wall
    public float rayHeight = 22.5f;

    public LayerMask whatIsGround;
    [Header("Transforms")]
    public List<GameObject> targets;                                                    // list for all targets that could be the nearest player
    Dictionary<float, GameObject> distDic = new Dictionary<float, GameObject>();        // used for getting the nearest player
    GameObject nearestTarget;                                                           // nearest player
    Vector2 targetPos;                                                                  // nearest players position
    public Transform left;                                                              // transform for ray casts
    public Transform right;                                                             // transform for ray casts
    Vector3 lastPos;                                                                    // last postion of this transform
    Vector2 distance;                                                                   // distance from this to nearest target

    [Header("External Components")]
    [SerializeField] private Animator animator;                                         // Animator from another GameObject
    [SerializeField] private SpriteRenderer spriteRenderer;                             // SpriteRenderer from another GameObject

    Rigidbody2D rb;                                                                     // this rigidbody2d

    // Raycast hits - now all public so they're visible in inspector
    [Header("Raycast Hits (Read Only)")]
    public RaycastHit2D leftInfoGround;
    public RaycastHit2D rightInfoGround;
    public RaycastHit2D leftInfoLongGround;
    public RaycastHit2D rightInfoLongGround;
    public RaycastHit2D targetRay;
    public RaycastHit2D leftInfoUp;
    public RaycastHit2D rightInfoUp;
    public RaycastHit2D leftInfo;
    public RaycastHit2D rightInfo;
    #endregion

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Remove the GetComponent fallbacks since we're requiring these to be set in the inspector
        if (animator == null)
        {
            Debug.LogError("Animator reference is missing! Please assign an Animator from another GameObject.");
            enabled = false;
            return;
        }

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer reference is missing! Please assign a SpriteRenderer from another GameObject.");
            enabled = false;
            return;
        }

        if (right == null) right = this.transform.GetChild(0);
        if (left == null) left = this.transform.GetChild(1);

        // Find Kael FBF by name instead of using Player tag
        nearestTarget = GameObject.Find("Kael FBF");
        if (nearestTarget == null)
        {
            Debug.LogError("Could not find GameObject named 'Kael FBF'!");
            enabled = false;
            return;
        }

        canJump = true;
        rb.freezeRotation = true;

        StartCoroutine(removeDupes());
        StartCoroutine(isStanding());
    }

    IEnumerator removeDupes()
    {
        yield return new WaitForSeconds(0.1f);
        distDic.Clear();
        StartCoroutine(removeDupes());
        yield return null;
    }

    void Update()
    {


        #region Companion
        // Don't need to search for targets anymore since we have a specific target
        if (nearestTarget == null)
        {
            // Try to find Kael FBF again if we lost reference
            nearestTarget = GameObject.Find("Kael FBF");
            if (nearestTarget == null)
            {
                Debug.LogError("Lost reference to 'Kael FBF'!");
                return;
            }
        }

        distance = (transform.position - nearestTarget.transform.position);
        #endregion

        #region Movement Behavior Based on Distance
        float distanceToPlayer = Vector2.Distance(transform.position, nearestTarget.transform.position);

        // Check for idle state first
        if (distanceToPlayer <= idleDistanceThreshold && isGrounded)
        {
            // Immediately set to idle state when in range and grounded
            if (!isIdle)
            {
                isIdle = true;
                moveEnabled = false;
                speed = 0;
                animator.SetInteger("state", STATE_IDLE);
            }
        }
        else
        {
            // Only change state if we were previously idle
            if (isIdle)
            {
                isIdle = false;
                moveEnabled = true;
            }

            // Set movement speed based on distance
            if (distanceToPlayer <= walkDistanceThreshold)
            {
                speed = walkSpeed;
            }
            else
            {
                speed = runSpeed;
            }
        }

        // Determine direction based on target position
        if (nearestTarget != null)
        {
            if (distance.x > 0)
            {
                movingRight = false;
            }
            else
            {
                movingRight = true;
            }
        }
        #endregion

        #region Ray casts
        leftInfoGround = Physics2D.Raycast(left.position, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(right.position, Vector2.down, groundRayLength, whatIsGround);

        leftInfoLongGround = Physics2D.Raycast(new Vector2(left.position.x - 2, left.position.y), Vector2.down, longGroundRayLength, whatIsGround);
        rightInfoLongGround = Physics2D.Raycast(new Vector2(right.position.x + 2, right.position.y), Vector2.down, longGroundRayLength, whatIsGround);

        targetRay = Physics2D.Raycast(transform.position, nearestTarget.transform.position, 1000f);

        leftInfoUp = Physics2D.Raycast(new Vector2(left.position.x, left.position.y + rayHeight), Vector2.left, wallRayLength, whatIsGround);
        rightInfoUp = Physics2D.Raycast(new Vector2(right.position.x, right.position.y + rayHeight), Vector2.right, wallRayLength, whatIsGround);

        leftInfo = Physics2D.Raycast(left.position, Vector2.left, wallRayLength, whatIsGround);
        rightInfo = Physics2D.Raycast(right.position, Vector2.right, wallRayLength, whatIsGround);
        #endregion

        #region Ground Raycasts
        // Only check for jumps if not idle
        if (!isIdle)
        {
            if (leftInfoGround.collider == false && rightInfoGround.collider == true && leftInfoLongGround.collider == false)
            {
                movingRight = true;
                isGrounded = true;
                if (leftInfo.collider == false && canJump == true) StartCoroutine(Jump("Large", false, 2));
            }
            else if (leftInfoGround.collider == false && rightInfoGround.collider == true && leftInfoLongGround.collider == true)
            {
                movingRight = false;
                StartCoroutine(Jump("Small", false, 2));
                isGrounded = true;
            }

            if (leftInfoGround.collider == true && rightInfoGround.collider == true) isGrounded = true;
            else isGrounded = false;

            if (leftInfoGround.collider == true && rightInfoGround.collider == false && rightInfoLongGround.collider == false)
            {
                movingRight = false;
                isGrounded = true;
                if (rightInfo.collider == false && canJump == true) StartCoroutine(Jump("Large", true, 2));
            }
            else if (leftInfoGround.collider == true && rightInfoGround.collider == false && rightInfoLongGround.collider == true)
            {
                movingRight = true;
                StartCoroutine(Jump("Small", true, 2));
                isGrounded = true;
            }
        }
        #endregion

        #region Wall Raycasts
        // Only check for wall jumps if not idle
        if (!isIdle && leftInfoGround.collider == true && rightInfoGround.collider == true)
        {
            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false && !movingRight)
                StartCoroutine(Jump("Large", movingRight, 2));

            if (leftInfo.collider == true)
                if (leftInfo.distance <= 0.1f) movingRight = true;

            if (rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && rightInfoUp.collider == false && movingRight)
                StartCoroutine(Jump("Large", movingRight, 2));

            if (rightInfo.collider == true)
                if (rightInfo.distance <= 0.1f) movingRight = false;

            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false &&
               rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && rightInfoUp.collider == false)
                return;
        }
        #endregion

        // Handle jumping/falling animation
        if (!isGrounded)
        {
            animator.SetInteger("state", STATE_JUMPING);
            // If we're in the air, we can't be idle
            isIdle = false;
        }
        // Only set to idle if we're grounded and in range
        else if (isIdle)
        {
            animator.SetInteger("state", STATE_IDLE);
        }

        if (rb.linearVelocity.y < 0) rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !isGrounded) rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2 - 1) * Time.deltaTime;
    }

    void FixedUpdate()
    {
        #region Movement
        if (!isGrounded)
        {
            // Jumping state is already set in Update
            return;
        }

        if (!isIdle) // Only move if not idle
        {
            if (movingRight && moveEnabled)
            {
                spriteRenderer.flipX = false;
                Vector2 direction = ((Vector2)Vector2.right).normalized;
                Vector2 force = direction * speed * Time.deltaTime;
                rb.MovePosition(rb.position + force);
                if (direction.x > 0) jumpRight = true;

                // Set animation state based on speed
                if (speed == walkSpeed)
                {
                    animator.SetInteger("state", STATE_WALKING);
                }
                else
                {
                    animator.SetInteger("state", STATE_RUNNING);
                }
            }
            else if (!movingRight && moveEnabled)
            {
                spriteRenderer.flipX = true;
                Vector2 direction = ((Vector2)Vector2.left).normalized;
                Vector2 force = direction * speed * Time.deltaTime;
                rb.MovePosition(rb.position + force);
                if (direction.x < 0) jumpRight = false;

                // Set animation state based on speed
                if (speed == walkSpeed)
                {
                    animator.SetInteger("state", STATE_WALKING);
                }
                else
                {
                    animator.SetInteger("state", STATE_RUNNING);
                }
            }
        }
        #endregion
    }

    #region Bug fixes
    IEnumerator isStanding()
    {
        lastPos = transform.position;
        yield return new WaitForSeconds(1);
        if (lastPos.x == transform.position.x && lastPos.y == transform.position.y && isGrounded)
        {
            animator.SetInteger("state", STATE_IDLE);
            // Only jump if not idle and not in idle range
            if (!isIdle) StartCoroutine(Jump("Small", movingRight, 2));
        }
        StartCoroutine(isStanding());
    }
    #endregion

    #region Jump
    IEnumerator Jump(string size, bool dirRight, float wait)
    {
        // Don't jump if we're idle
        if (isIdle) yield break;

        isGrounded = false;
        isIdle = false; // Can't be idle while jumping
        animator.SetInteger("state", STATE_JUMPING);
        speed += 2;
        movingRight = true;

        if (size == "Large")
        {
            rb.linearVelocity = new Vector2(0, jumpHeight);
            if (dirRight) rb.AddForce(Vector2.right * jumpHeight / 2, ForceMode2D.Impulse);
            else rb.AddForce(Vector2.left * jumpHeight / 2, ForceMode2D.Impulse);
        }
        else if (size == "Small")
        {
            rb.linearVelocity = new Vector2(0, jumpHeight / 2);
            if (dirRight) rb.AddForce(Vector2.right * jumpHeight / 2, ForceMode2D.Impulse);
            else rb.AddForce(Vector2.left * jumpHeight / 2, ForceMode2D.Impulse);
        }

        yield return new WaitForSeconds(wait);
        if (isGrounded) canJump = false;
        moveEnabled = true;
        StartCoroutine(resetJump());
        yield return null;
    }

    IEnumerator resetJump()
    {
        yield return new WaitForSeconds(1);
        canJump = true;
        yield return null;
    }
    #endregion

    #region DEBUGING
    void OnDrawGizmos()
    {
        // Always draw the distance thresholds in the editor
        if (nearestTarget != null)
        {
            // Walk threshold - yellow
            Gizmos.color = new Color(1, 1, 0, 0.3f); // Semi-transparent yellow
            Gizmos.DrawWireSphere(nearestTarget.transform.position, walkDistanceThreshold);

            // Idle threshold - green
            Gizmos.color = new Color(0, 1, 0, 0.3f); // Semi-transparent green
            Gizmos.DrawWireSphere(nearestTarget.transform.position, idleDistanceThreshold);
        }

        // Draw all rays in edit mode (gray) or with hit/miss colors in play mode
        if (left == null || right == null) return;

        // -------------------------------
        // GROUND RAYS VISUALIZATION
        // -------------------------------
        // Short ground rays (left and right)
        if (Application.isPlaying && DEBUGMODE)
        {
            Gizmos.color = leftInfoGround.collider ? Color.green : Color.red;
            Gizmos.DrawLine(left.position, left.position + Vector3.down * groundRayLength);

            Gizmos.color = rightInfoGround.collider ? Color.green : Color.red;
            Gizmos.DrawLine(right.position, right.position + Vector3.down * groundRayLength);

            // Long ground rays (offset left and right)
            Vector3 longLeftPos = new Vector3(left.position.x - 2, left.position.y, left.position.z);
            Vector3 longRightPos = new Vector3(right.position.x + 2, right.position.y, right.position.z);

            Gizmos.color = leftInfoLongGround.collider ? Color.green : Color.red;
            Gizmos.DrawLine(longLeftPos, longLeftPos + Vector3.down * longGroundRayLength);

            Gizmos.color = rightInfoLongGround.collider ? Color.green : Color.red;
            Gizmos.DrawLine(longRightPos, longRightPos + Vector3.down * longGroundRayLength);
        }
        else
        {
            // Draw all rays gray in edit mode or when not debugging
            Gizmos.color = Color.gray;
            Gizmos.DrawLine(left.position, left.position + Vector3.down * groundRayLength);
            Gizmos.DrawLine(right.position, right.position + Vector3.down * groundRayLength);

            Vector3 longLeftPos = new Vector3(left.position.x - 2, left.position.y, left.position.z);
            Vector3 longRightPos = new Vector3(right.position.x + 2, right.position.y, right.position.z);
            Gizmos.DrawLine(longLeftPos, longLeftPos + Vector3.down * longGroundRayLength);
            Gizmos.DrawLine(longRightPos, longRightPos + Vector3.down * longGroundRayLength);
        }

        // -------------------------------
        // WALL RAYS VISUALIZATION
        // -------------------------------
        if (Application.isPlaying && DEBUGMODE)
        {
            // Standard wall rays (left and right)
            Gizmos.color = leftInfo.collider ? Color.green : Color.red;
            Gizmos.DrawLine(left.position, left.position + Vector3.left * wallRayLength);

            Gizmos.color = rightInfo.collider ? Color.green : Color.red;
            Gizmos.DrawLine(right.position, right.position + Vector3.right * wallRayLength);

            // Upper wall rays (at rayHeight)
            Vector3 upperLeftPos = new Vector3(left.position.x, left.position.y + rayHeight, left.position.z);
            Vector3 upperRightPos = new Vector3(right.position.x, right.position.y + rayHeight, right.position.z);

            Gizmos.color = leftInfoUp.collider ? Color.green : Color.red;
            Gizmos.DrawLine(upperLeftPos, upperLeftPos + Vector3.left * wallRayLength);

            Gizmos.color = rightInfoUp.collider ? Color.green : Color.red;
            Gizmos.DrawLine(upperRightPos, upperRightPos + Vector3.right * wallRayLength);
        }
        else
        {
            // Draw all wall rays yellow in edit mode or when not debugging
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(left.position, left.position + Vector3.left * wallRayLength);
            Gizmos.DrawLine(right.position, right.position + Vector3.right * wallRayLength);

            Vector3 upperLeftPos = new Vector3(left.position.x, left.position.y + rayHeight, left.position.z);
            Vector3 upperRightPos = new Vector3(right.position.x, right.position.y + rayHeight, right.position.z);
            Gizmos.DrawLine(upperLeftPos, upperLeftPos + Vector3.left * wallRayLength);
            Gizmos.DrawLine(upperRightPos, upperRightPos + Vector3.right * wallRayLength);
        }

        // Visualize rayHeight with a horizontal line
        if (DEBUGMODE)
        {
            Gizmos.color = new Color(0.5f, 0, 0.5f, 0.3f); // Purple with transparency
            float rayHeightLineLength = 5f;
            Gizmos.DrawLine(
                new Vector3(left.position.x - rayHeightLineLength, left.position.y + rayHeight, left.position.z),
                new Vector3(left.position.x + rayHeightLineLength, left.position.y + rayHeight, left.position.z)
            );
        }

        // -------------------------------
        // TARGET LINE VISUALIZATION
        // -------------------------------
        if (nearestTarget != null && isInFollowRange)
        {
            Gizmos.color = new Color(0, 1, 1, 0.5f); // Cyan with transparency
            Gizmos.DrawLine(transform.position, nearestTarget.transform.position);
        }
    }
    #endregion
}
