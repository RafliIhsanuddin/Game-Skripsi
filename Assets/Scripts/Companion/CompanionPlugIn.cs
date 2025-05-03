using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;



[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Sprite))]

public class CompanionPlugIn : MonoBehaviour
{
    #region Variables
    [Header("Movement")]
    public float minSpeed = 10;                                                         // determine min speed
    public float maxSpeed = 14;                                                         // determine max speed
    public float minJumpHeight = 5f;                                                    // minimum jump height
    public float maxJumpHeight = 12f;                                                   // maximum jump height
    float speed;                                                                        // current speed

    [Header("Jump Tuning")]
    public float baseHorizontalForce = 2.5f;  // Base horizontal force
    private float horizontalForceMultiplier = 1f; // The actual multiplier variable
    [Range(0.1f, 50f)] public float minHorizontalForceMultiplier = 0.5f;
    [Range(0.1f, 50f)] public float maxHorizontalForceMultiplier = 2f;
    public float horizontalForceAdjustSpeed = 0.5f; // How quickly the force adjusts

    [Header("Jump Height Smoothing")]
    public float minRequiredJumpHeight = 5f;
    public float maxRequiredJumpHeight = 12f;
    public float heightSmoothingFactor = 0.2f;
    public float distanceWeight = 0.1f;
    public float heightWeight = 1.2f;

    [Header("DEBUGING")]
    public bool DEBUGMODE = false;
    public bool showJumpCalculations = false;
    public bool showJumpDebugInfo = false;

    private bool lastJumpWasRight;
    private Vector2 lastJumpVelocity;
    private Vector2 lastJumpStartPosition;
    private Vector2 lastJumpTargetPosition;
    private float lastJumpHorizontalForce;
    private float lastJumpVerticalForce;
    private float lastJumpCalculatedHeight;

    [Header("Booleans")]
    private bool movingRight = true;
    private bool canJump = true;
    private bool moveEnabled = true;
    private bool isGrounded = false;
    private bool isInFollowRange = false;
    private bool isJumping = false;

    [Header("Range")]
    public float followRange = 40;                                                      // follow distance
    public float wallRayLength = 30;                                                    // the length for ray casts that face the wall
    public float groundRayLength = 3.5f;                                                // the length for ray casts that face the ground
    public float longGroundRayLength = 14f;                                             // longer ground detection ray
    public float rayHeight = 22.5f;
    public float platformDetectionRange = 10f;                                          // range to detect platforms for jump calculation
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
    Animator anim;                                                                      // this animator
    Rigidbody2D rb;                                                                     // this rigidbody2d

    // Raycast variables
    RaycastHit2D leftInfoGround;
    RaycastHit2D rightInfoGround;
    RaycastHit2D leftInfoLongGround;
    RaycastHit2D rightInfoLongGround;
    RaycastHit2D targetRay;
    RaycastHit2D leftInfoUp;
    RaycastHit2D RightInfoUp;
    RaycastHit2D leftInfo;
    RaycastHit2D rightInfo;

    // Jump calculation variables
    [SerializeField] private Vector2 currentPlatformPosition;
    [SerializeField] private Vector2 targetPlatformPosition;
    [SerializeField] private float requiredJumpHeight;
    #endregion

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (right == null) right = this.transform.GetChild(0);
        if (left == null) left = this.transform.GetChild(1);

        canJump = true;
        rb.freezeRotation = true;
        anim.SetBool("IsDead", false);

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
        #region Nearest Target
        targets.RemoveAll(target => target == null);
        targets = GameObject.FindGameObjectsWithTag("Player").Distinct().Where(t => t != null).ToList();

        distDic.Clear();

        foreach (GameObject obj in targets)
        {
            if (obj == null) continue;

            float dist = Vector2.Distance(transform.position, obj.transform.position);

            if (!distDic.ContainsKey(dist))
            {
                distDic.Add(dist, obj);
            }
        }

        if (distDic.Count > 0)
        {
            List<float> distances = distDic.Keys.ToList();
            distances.Sort();
            nearestTarget = distDic[distances[0]];
            distance = (transform.position - nearestTarget.transform.position);
            targetPos = nearestTarget.transform.position;
        }
        #endregion

        if (nearestTarget != null)
        {
            float targetDistance = Mathf.Abs(distance.x);
            float desiredMultiplier = Mathf.Clamp(
                targetDistance / 5f,
                minHorizontalForceMultiplier,
                maxHorizontalForceMultiplier);

            horizontalForceMultiplier = Mathf.Lerp(
                horizontalForceMultiplier,
                desiredMultiplier,
                Time.deltaTime * horizontalForceAdjustSpeed);
        }

        #region Ray casts
        leftInfoGround = Physics2D.Raycast(left.position, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(right.position, Vector2.down, groundRayLength, whatIsGround);

        leftInfoLongGround = Physics2D.Raycast(new Vector2(left.position.x - 2, left.position.y), Vector2.down, longGroundRayLength, whatIsGround);
        rightInfoLongGround = Physics2D.Raycast(new Vector2(right.position.x + 2, right.position.y), Vector2.down, longGroundRayLength, whatIsGround);

        targetRay = Physics2D.Raycast(transform.position, nearestTarget.transform.position, followRange);

        leftInfoUp = Physics2D.Raycast(new Vector2(left.position.x, left.position.y + rayHeight), Vector2.left, wallRayLength, whatIsGround);
        RightInfoUp = Physics2D.Raycast(new Vector2(right.position.x, right.position.y + rayHeight), Vector2.right, wallRayLength, whatIsGround);

        leftInfo = Physics2D.Raycast(left.position, Vector2.left, wallRayLength, whatIsGround);
        rightInfo = Physics2D.Raycast(right.position, Vector2.right, wallRayLength, whatIsGround);
        #endregion

        #region Platform Detection
        if (isGrounded && !isJumping)
        {
            RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, groundRayLength * 2, whatIsGround);
            if (groundHit.collider != null)
            {
                currentPlatformPosition = groundHit.point;
            }
        }

        Vector2 lookDirection = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D platformHit = Physics2D.Raycast(
            transform.position + new Vector3(movingRight ? 1 : -1, 0, 0) * platformDetectionRange * 0.5f,
            lookDirection,
            platformDetectionRange,
            whatIsGround);

        if (platformHit.collider != null)
        {
            targetPlatformPosition = platformHit.point;
            float heightDifference = targetPlatformPosition.y - currentPlatformPosition.y;
            float distanceToPlatform = Mathf.Abs(targetPlatformPosition.x - currentPlatformPosition.x);

            requiredJumpHeight = Mathf.Clamp(
                Mathf.Abs(heightDifference) * heightWeight + distanceToPlatform * distanceWeight,
                minRequiredJumpHeight,
                maxRequiredJumpHeight);
        }
        else
        {
            requiredJumpHeight = (minRequiredJumpHeight + maxRequiredJumpHeight) * 0.5f;
        }
        #endregion

        #region Ground Raycasts
        if (leftInfoGround.collider == false && rightInfoGround.collider == true && leftInfoLongGround.collider == false)
        {
            movingRight = true;
            isGrounded = true;
            if (leftInfo.collider == false && canJump == true && !isJumping) StartCoroutine(Jump("Large", false, 2));
        }
        else if (leftInfoGround.collider == false && rightInfoGround.collider == true && leftInfoLongGround.collider == true)
        {
            movingRight = false;
            if (!isJumping) StartCoroutine(Jump("Small", false, 2));
            isGrounded = true;
        }

        if (leftInfoGround.collider == true && rightInfoGround.collider == true)
        {
            isGrounded = true;
            isJumping = false;
        }
        else isGrounded = false;

        if (leftInfoGround.collider == true && rightInfoGround.collider == false && rightInfoLongGround.collider == false)
        {
            movingRight = false;
            isGrounded = true;
            if (rightInfo.collider == false && canJump == true && !isJumping) StartCoroutine(Jump("Large", true, 2));
        }
        else if (leftInfoGround.collider == true && rightInfoGround.collider == false && rightInfoLongGround.collider == true)
        {
            movingRight = true;
            if (!isJumping) StartCoroutine(Jump("Small", true, 2));
            isGrounded = true;
        }
        #endregion

        #region Wall Raycasts
        if (leftInfoGround.collider == true && rightInfoGround.collider == true)
        {
            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false && !movingRight && !isJumping)
                StartCoroutine(Jump("Precise", movingRight, 2));

            if (leftInfo.collider == true && leftInfo.distance <= 0.1f) movingRight = true;

            if (rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && RightInfoUp.collider == false && movingRight && !isJumping)
                StartCoroutine(Jump("Precise", movingRight, 2));

            if (rightInfo.collider == true && rightInfo.distance <= 0.1f) movingRight = false;

            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false &&
            rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && RightInfoUp.collider == false)
                return;
        }
        #endregion

        #region Follow Range
        if (distance.x < followRange && distance.y < followRange && distance.x > -followRange && distance.y > -followRange)
        {
            isInFollowRange = true;
            speed = maxSpeed;
        }
        else
        {
            speed = minSpeed;
            isInFollowRange = false;
        }

        if (distance.x > 0)
        {
            if (distance.x < followRange && distance.y < followRange)
            {
                movingRight = false;
            }
        }
        if (distance.x < 0)
        {
            if (distance.x > -followRange && distance.y > -followRange)
            {
                movingRight = true;
            }
        }
        #endregion

        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !isGrounded)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2 - 1) * Time.deltaTime;
    }

    void FixedUpdate()
    {
        #region Movement
        anim.SetBool("isGrounded", isGrounded);

        if (moveEnabled && isGrounded)
        {
            anim.SetBool("moveEnabled", true);
            GetComponent<SpriteRenderer>().flipX = !movingRight;
            Vector2 direction = movingRight ? Vector2.right : Vector2.left;
            Vector2 force = direction * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + force);
        }
        #endregion
    }

    #region Bug fixes
    IEnumerator isStanding()
    {
        lastPos = transform.position;
        yield return new WaitForSeconds(1);
        if (lastPos.x == transform.position.x && lastPos.y == transform.position.y)
        {
            anim.SetBool("moveEnabled", false);
            if (!isJumping) StartCoroutine(Jump("Small", movingRight, 2));
        }
        else anim.SetBool("moveEnabled", true);
        StartCoroutine(isStanding());
    }
    #endregion

    #region Jump
    IEnumerator Jump(string size, bool dirRight, float wait)
    {
        lastJumpWasRight = dirRight;
        lastJumpStartPosition = transform.position;
        lastJumpTargetPosition = targetPlatformPosition;
        isJumping = true;

        isGrounded = false;
        anim.SetTrigger("Jump");

        float jumpForce = 0f;
        float horizontalForce = baseHorizontalForce * horizontalForceMultiplier;

        switch (size)
        {
            case "Large":
                jumpForce = maxJumpHeight;
                horizontalForce *= 1.2f;
                break;
            case "Small":
                jumpForce = minJumpHeight;
                horizontalForce *= 0.5f;
                break;
            case "Precise":
                jumpForce = requiredJumpHeight;
                horizontalForce *= 0.8f;
                break;
            default:
                jumpForce = (minJumpHeight + maxJumpHeight) * 0.5f;
                break;
        }

        // Store jump values for debugging
        lastJumpHorizontalForce = dirRight ? horizontalForce : -horizontalForce;
        lastJumpVerticalForce = jumpForce;
        lastJumpCalculatedHeight = requiredJumpHeight;
        lastJumpVelocity = new Vector2(lastJumpHorizontalForce, jumpForce);

        // Log jump information to console
        if (DEBUGMODE && showJumpDebugInfo)
        {
            Debug.Log($"=== JUMP DEBUG ===");
            Debug.Log($"Jump Type: {size}");
            Debug.Log($"Direction: {(dirRight ? "Right" : "Left")}");
            Debug.Log($"Horizontal Force: {lastJumpHorizontalForce:F2}");
            Debug.Log($"Vertical Force: {lastJumpVerticalForce:F2}");
            Debug.Log($"Jump Force Value: {jumpForce:F2}");
            Debug.Log($"Target Position: {lastJumpTargetPosition}");
            Debug.Log($"Calculated Height: {lastJumpCalculatedHeight:F2}");
            Debug.Log($"Start Position: {lastJumpStartPosition}");
            Debug.Log($"Platform Positions - Current: {currentPlatformPosition} | Target: {targetPlatformPosition}");
            Debug.Log($"Horizontal Force Multiplier: {horizontalForceMultiplier:F2}");
            Debug.Log($"=== END JUMP DEBUG ===");
        }

        // Apply forces
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        if (dirRight)
            rb.AddForce(Vector2.right * horizontalForce, ForceMode2D.Impulse);
        else
            rb.AddForce(Vector2.left * horizontalForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(wait);
        StartCoroutine(resetJump());
    }

    IEnumerator resetJump()
    {
        yield return new WaitForSeconds(1);
        canJump = true;
        isJumping = false;

        // Log landing information to console
        if (DEBUGMODE && showJumpDebugInfo)
        {
            Debug.Log($"Jump Completed:\n" +
                     $"Landed at: {transform.position}\n" +
                     $"Distance to target: {Vector2.Distance(transform.position, lastJumpTargetPosition):F2}");
        }

        yield return null;
    }
    #endregion

    #region DEBUGING
    void OnDrawGizmos()
    {
        if (DEBUGMODE)
        {
            if (rightInfoGround.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(right.position, new Vector2(right.position.x, right.position.y - groundRayLength));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(right.position, new Vector2(right.position.x, right.position.y - groundRayLength));
            }
            if (leftInfoGround.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(left.position, new Vector2(left.position.x, left.position.y - groundRayLength));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(left.position, new Vector2(left.position.x, left.position.y - groundRayLength));
            }
            if (rightInfoLongGround.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(new Vector2(right.position.x + 2, right.position.y), new Vector2(right.position.x + 2, right.position.y - longGroundRayLength));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(new Vector2(right.position.x + 2, right.position.y), new Vector2(right.position.x + 2, right.position.y - longGroundRayLength));
            }
            if (leftInfoLongGround.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(new Vector2(left.position.x - 2, left.position.y), new Vector2(left.position.x - 2, left.position.y - longGroundRayLength));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(new Vector2(left.position.x - 2, left.position.y), new Vector2(left.position.x - 2, left.position.y - longGroundRayLength));
            }

            if (leftInfo.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(left.position, new Vector2(left.position.x - wallRayLength, left.position.y));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(left.position, new Vector2(left.position.x - wallRayLength, left.position.y));
            }

            if (rightInfo.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(right.position, new Vector2(right.position.x + wallRayLength, left.position.y));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(right.position, new Vector2(right.position.x + wallRayLength, left.position.y));
            }

            if (RightInfoUp.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(new Vector2(right.position.x, right.position.y + rayHeight), new Vector2(right.position.x + wallRayLength, right.position.y + rayHeight));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine((new Vector2(right.position.x, right.position.y + rayHeight)), new Vector2(right.position.x + wallRayLength, right.position.y + rayHeight));
            }

            if (leftInfoUp.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine((new Vector2(left.position.x, left.position.y + rayHeight)), new Vector2(left.position.x + -wallRayLength, left.position.y + rayHeight));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine((new Vector2(left.position.x, left.position.y + rayHeight)), new Vector2(left.position.x + -wallRayLength, left.position.y + rayHeight));
            }

            if (isInFollowRange)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, nearestTarget.transform.position);
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, new Vector3(followRange * 2, followRange * 2, 0));
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(followRange * 2, followRange * 2, 0));
            }

            if (showJumpCalculations)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(currentPlatformPosition, 0.3f);

                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(targetPlatformPosition, 0.3f);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(currentPlatformPosition, targetPlatformPosition);

                if (isGrounded)
                {
                    int resolution = 20;
                    Vector2 prevPoint = lastJumpStartPosition;
                    for (int i = 1; i <= resolution; i++)
                    {
                        float time = i * 0.1f;
                        Vector2 point = lastJumpStartPosition +
                                        lastJumpVelocity * time +
                                        0.5f * Physics2D.gravity * time * time;

                        Gizmos.DrawLine(prevPoint, point);
                        prevPoint = point;

                        if (point.y <= lastJumpStartPosition.y && i > 1)
                            break;
                    }
                }
            }

            if (showJumpDebugInfo)
            {
                // Draw debug text for jump information
                Vector3 debugPos = transform.position + Vector3.up * 2f;
                /*string debugText = $"Jump Debug:\n" +
                                $"Start: {lastJumpStartPosition.ToString("F1")}\n" +
                                $"Target: {lastJumpTargetPosition.ToString("F1")}\n" +
                                $"Horizontal: {lastJumpHorizontalForce.ToString("F1")}\n" +
                                $"Vertical: {lastJumpVerticalForce.ToString("F1")}\n" +
                                $"Calc Height: {lastJumpCalculatedHeight.ToString("F1")}";*/

                /*UnityEditor.Handles.Label(debugPos, debugText, new GUIStyle()
                {
                    fontSize = 12,
                    normal = new GUIStyleState() { textColor = Color.white },
                    alignment = TextAnchor.MiddleCenter
                });*/
            }
        }
    }
    #endregion
}
