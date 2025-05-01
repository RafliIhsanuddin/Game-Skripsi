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
    public float jumpHorizontalForce = 2.5f;                                            // reduced horizontal force when jumping
    float speed;                                                                        // current speed

    [Header("Jump Tuning")]
    public float baseHorizontalForce = 2.5f;  // Base horizontal force
    public float horizontalForceMultiplier = 1f; // Dynamic multiplier
    public float horizontalForceAdjustSpeed = 0.5f; // How quickly the force adjusts

    [Header("DEBUGING")]
    public bool DEBUGMODE = false;
    public bool showJumpCalculations = false;

    private bool lastJumpWasRight;
    private Vector2 lastJumpVelocity;
    private Vector2 lastJumpStartPosition;

    [Header("Booleans")]
    private bool movingRight = true;
    private bool canJump = true;
    private bool moveEnabled = true;
    private bool isGrounded = false;
    private bool isInFollowRange = false;

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
    private Vector2 currentPlatformPosition;
    private Vector2 targetPlatformPosition;
    private float requiredJumpHeight;
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
        targets.RemoveAll(target => target == null); // Clean up null targets first
        targets = GameObject.FindGameObjectsWithTag("Player").Distinct().Where(t => t != null).ToList();

        distDic.Clear(); // Clear the dictionary before adding new entries

        foreach (GameObject obj in targets)
        {
            if (obj == null) continue;

            float dist = Vector2.Distance(transform.position, obj.transform.position);

            // Add only if the distance isn't already a key
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
        }
        #endregion

        // Adjust horizontal force based on distance to target
        if (nearestTarget != null)
        {
            float targetDistance = Mathf.Abs(distance.x);

            // Calculate desired multiplier based on distance
            float desiredMultiplier = Mathf.Clamp(targetDistance / 5f, 0.5f, 2f);

            // Smoothly adjust the current multiplier
            horizontalForceMultiplier = Mathf.Lerp(
                horizontalForceMultiplier,
                desiredMultiplier,
                Time.deltaTime * horizontalForceAdjustSpeed);
        }

        #region Ray casts
        leftInfoGround = Physics2D.Raycast(left.position, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(right.position, Vector2.down, groundRayLength, whatIsGround);

        leftInfoLongGround = Physics2D.Raycast(new Vector2(left.position.x - 2, left.position.y), Vector2.down, longGroundRayLength, whatIsGround); // left long ground raycast
        rightInfoLongGround = Physics2D.Raycast(new Vector2(right.position.x + 2, right.position.y), Vector2.down, longGroundRayLength, whatIsGround); // right long ground raycast

        targetRay = Physics2D.Raycast(transform.position, nearestTarget.transform.position, followRange);

        leftInfoUp = Physics2D.Raycast(new Vector2(left.position.x, left.position.y + rayHeight), Vector2.left, wallRayLength, whatIsGround); // left up raycast
        RightInfoUp = Physics2D.Raycast(new Vector2(right.position.x, right.position.y + rayHeight), Vector2.right, wallRayLength, whatIsGround); // Right up raycast

        leftInfo = Physics2D.Raycast(left.position, Vector2.left, wallRayLength, whatIsGround); // left raycast
        rightInfo = Physics2D.Raycast(right.position, Vector2.right, wallRayLength, whatIsGround); // right raycast
        #endregion

        #region Platform Detection
        // Detect current platform position
        if (isGrounded)
        {
            RaycastHit2D groundHit = Physics2D.Raycast(transform.position, Vector2.down, groundRayLength * 2, whatIsGround);
            if (groundHit.collider != null)
            {
                currentPlatformPosition = groundHit.point;
            }
        }

        // Detect target platform position (where we want to land)
        Vector2 lookDirection = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D platformHit = Physics2D.Raycast(
            transform.position + new Vector3(movingRight ? 1 : -1, 0, 0) * platformDetectionRange * 0.5f,
            lookDirection,
            platformDetectionRange,
            whatIsGround);

        if (platformHit.collider != null)
        {
            targetPlatformPosition = platformHit.point;

            // Calculate required jump height based on platform positions
            float heightDifference = targetPlatformPosition.y - currentPlatformPosition.y;
            float distanceToPlatform = Mathf.Abs(targetPlatformPosition.x - currentPlatformPosition.x);

            // Calculate required jump height using physics formula: v = sqrt(2 * g * h)
            // We adjust based on height difference and distance
            requiredJumpHeight = Mathf.Clamp(
                Mathf.Abs(heightDifference) * 1.2f + distanceToPlatform * 0.1f, // Reduced distance factor
                minJumpHeight,
                maxJumpHeight);
        }
        else
        {
            // If no platform detected, use default jump height
            requiredJumpHeight = (minJumpHeight + maxJumpHeight) * 0.5f;
        }
        #endregion

        #region Ground Raycasts
        // if the left ground ray cast hits nothing and the right ground ray hits somthing then
        if (leftInfoGround.collider == false && rightInfoGround.collider == true && leftInfoLongGround.collider == false)
        {
            // we move right
            movingRight = true;
            // we are grounded
            isGrounded = true;
            // if the left wall ray hits nothing and we can jump then we jump left
            if (leftInfo.collider == false && canJump == true) StartCoroutine(Jump("Large", false, 2));
        }
        else if (leftInfoGround.collider == false && rightInfoGround.collider == true && leftInfoLongGround.collider == true)
        {
            // we move left
            movingRight = false;
            StartCoroutine(Jump("Small", false, 2));
            // we are grounded
            isGrounded = true;
        }

        // if the left ground ray cast hits somthing and the right ground ray hits somthing then we are grounded
        if (leftInfoGround.collider == true && rightInfoGround.collider == true) isGrounded = true;
        // else if no ground ray is colliding with earth then we aren't grounded
        else isGrounded = false;

        // if the left ground ray cast hits somthing and the right ground ray hits nothing then
        if (leftInfoGround.collider == true && rightInfoGround.collider == false && rightInfoLongGround.collider == false)
        {
            //we move left
            movingRight = false;
            // we are grounded
            isGrounded = true;
            // if the right wall ray hits nothing and we can jump then we jump right
            if (rightInfo.collider == false && canJump == true) StartCoroutine(Jump("Large", true, 2));
        }
        else if (leftInfoGround.collider == true && rightInfoGround.collider == false && rightInfoLongGround.collider == true)
        {
            // we move right
            movingRight = true;
            StartCoroutine(Jump("Small", true, 2));
            // we are grounded
            isGrounded = true;
        }
        #endregion

        #region Wall Raycasts
        // if the left ground ray cast hits somthing and the right ground ray hits somthing then
        if (leftInfoGround.collider == true && rightInfoGround.collider == true)
        {
            // if the left ray hits somthing and the distance is about 1 / 3 to him and the left up ray hits nothing and if we are currently moving left, we want to jump left on a ledge
            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false && !movingRight)
                StartCoroutine(Jump("Precise", movingRight, 2));

            // if left wall ray has collided then
            if (leftInfo.collider == true)
                // if left wall ray distance is close to wall we need to turn right
                if (leftInfo.distance <= 0.1f) movingRight = true;

            // if the right ray hits somthing and the distance is about 1 / 3 to him and the right up ray hits nothing and if we are currently moving right, we want to jump right on a ledge
            if (rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && RightInfoUp.collider == false && movingRight)
                StartCoroutine(Jump("Precise", movingRight, 2));

            // if right wall ray has collided then
            if (rightInfo.collider == true)
                // if right wall ray distance is close to wall we need to turn left
                if (rightInfo.distance <= 0.1f) movingRight = false;

            // if left and right ray hit somthing and there distance is 1/3 and right/left up hits nothing we do nothing, why? because were in a hole
            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false &&
            rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && RightInfoUp.collider == false)
                return;
        }
        #endregion

        #region Follow Range
        // if player is withing the follow range then
        if (distance.x < followRange && distance.y < followRange && distance.x > -followRange && distance.y > -followRange)
        {
            // we have someone to follow
            isInFollowRange = true;
            // increase speed
            speed += 2;
        }
        else
        {
            // decrease speed
            speed -= 2;
            // no one to follow
            isInFollowRange = false;
        }
        // clam our speeds form a min, max, 
        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);

        // if nearest target is to the left of us
        if (distance.x > 0)
        { // move left
            // if nearest target is in follow range then
            if (distance.x < followRange && distance.y < followRange)
            {
                movingRight = false;
            }
        }
        // if nearest target is to the right of us
        if (distance.x < 0)
        { // move right
            // if nearest target is in follow range then
            if (distance.x > -followRange && distance.y > -followRange)
            {
                movingRight = true;
            }
        }
        #endregion

        // smoothens falling and jumping with physics
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !isGrounded)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2 - 1) * Time.deltaTime;
    }

    void FixedUpdate()
    {
        #region Movement
        // let our animator know were connected to earth
        anim.SetBool("isGrounded", isGrounded);
        // if we are moving right and we are able to move then
        if (movingRight && moveEnabled && isGrounded)
        { // right
            // let our animator know we are able to move
            anim.SetBool("moveEnabled", true);
            // flip our sprite to the right side
            GetComponent<SpriteRenderer>().flipX = false;
            // set a direction vector to be: (0, 1);
            Vector2 direction = ((Vector2)Vector2.right).normalized;
            // create a force to move, this is the direction were facing times our speed times delta time
            Vector2 force = direction * speed * Time.deltaTime;
            // move right
            rb.MovePosition(rb.position + force);
        }
        else if (!movingRight && moveEnabled && isGrounded)
        { // left
            // let our animator know we are able to move
            anim.SetBool("moveEnabled", true);
            // flip our sprite to the left side
            GetComponent<SpriteRenderer>().flipX = true;
            // set a direction vector to be: (0, -1);
            Vector2 direction = ((Vector2)Vector2.left).normalized;
            // create a force to move, this is the direction were facing times our speed times delta time
            Vector2 force = direction * speed * Time.deltaTime;
            // move left
            rb.MovePosition(rb.position + force);
        }
        #endregion
    }

    #region Bug fixes
    IEnumerator isStanding()
    {
        // setting the last pos vector to the current transform postion
        lastPos = transform.position;
        // wait 0.1 seconds before proceeding
        yield return new WaitForSeconds(1);
        // if our last position is the same as it was 0.1 seconds ago then
        if (lastPos.x == transform.position.x && lastPos.y == transform.position.y)
        {
            // let our anim know we cant move
            anim.SetBool("moveEnabled", false);
            // we move left
            StartCoroutine(Jump("Small", movingRight, 2));
        }
        // else let our anim know we can move
        else anim.SetBool("moveEnabled", true);
        // re call this function to make it a loop with a 0.1 sec delay
        StartCoroutine(isStanding());
    }
    #endregion

    #region Jump
    IEnumerator Jump(string size, bool dirRight, float wait)
    {
        // Store jump direction for debugging
        lastJumpWasRight = dirRight;
        lastJumpStartPosition = transform.position;

        // since we jump we can't be grounded
        isGrounded = false;
        // tell our animator we jumped
        anim.SetTrigger("Jump");
        // increase speed
        speed += 2;
        // move right
        movingRight = true;

        // Calculate jump force based on jump type and platform detection
        float jumpForce = 0f;
        float horizontalForce = baseHorizontalForce * horizontalForceMultiplier;

        switch (size)
        {
            case "Large":
                jumpForce = maxJumpHeight;
                horizontalForce *= 1.2f; // Reduced multiplier
                break;
            case "Small":
                jumpForce = minJumpHeight;
                horizontalForce *= 0.5f; // Reduced multiplier
                break;
            case "Precise":
                // Use calculated jump height for precise jumps
                jumpForce = requiredJumpHeight;
                horizontalForce *= 0.8f; // Reduced force for precision
                break;
            default:
                jumpForce = (minJumpHeight + maxJumpHeight) * 0.5f;
                break;
        }

        // Calculate and store jump velocity for debugging
        lastJumpVelocity = new Vector2(
            dirRight ? horizontalForce : -horizontalForce,
            Mathf.Sqrt(2f * Mathf.Abs(Physics2D.gravity.y) * jumpForce));

        // Apply jump force (vertical)
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); // Reset vertical velocity
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        // Apply continuous horizontal force during jump
        float forceDuration = 0.2f; // Duration to apply horizontal force
        float timer = 0f;

        while (timer < forceDuration)
        {
            if (dirRight)
                rb.AddForce(Vector2.right * horizontalForce * (1 - timer / forceDuration), ForceMode2D.Force);
            else
                rb.AddForce(Vector2.left * horizontalForce * (1 - timer / forceDuration), ForceMode2D.Force);

            timer += Time.deltaTime;
            yield return null;
        }

        // wait wait seconds before continuing
        yield return new WaitForSeconds(wait);
        // if we are grounded then we can't jump
        if (isGrounded) canJump = false;
        // we set move enabled 
        moveEnabled = true;
        // call reset jump so we don't spam jump
        StartCoroutine(resetJump());
    }

    IEnumerator resetJump()
    {
        // wait 1 sec before proceeding
        yield return new WaitForSeconds(1);
        //set jump to true
        canJump = true;
        // exit function
        yield return null;
    }
    #endregion

    #region DEBUGING
    void OnDrawGizmos()
    {
        if (DEBUGMODE)
        {
            // Existing debug visuals
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

            // New debug visuals for jump calculations
            if (showJumpCalculations)
            {
                // Draw current platform position
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(currentPlatformPosition, 0.3f);

                // Draw target platform position
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(targetPlatformPosition, 0.3f);

                // Draw line between platforms
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(currentPlatformPosition, targetPlatformPosition);

                // Draw calculated jump trajectory
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

                        // Stop drawing if we hit the ground
                        if (point.y <= lastJumpStartPosition.y && i > 1)
                            break;
                    }
                }
            }
        }
    }
    #endregion
}
