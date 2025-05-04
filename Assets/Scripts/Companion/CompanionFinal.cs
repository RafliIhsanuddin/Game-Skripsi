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
public class CompanionFinal : MonoBehaviour
{
    #region Variables
    [Header("Stats")]
    public float minSpeed = 10;                                                         // determine min speed
    public float maxSpeed = 14;                                                         // determine max speed
    public float jumpHeight = 8;                                                        // the height we can jump
    float speed;                                                                        // current speed
    float gravity;

    [Header("DEBUGING")]
    public bool DEBUGMODE = false;
    [Header("Booleans")]
    private bool movingRight = true;
    private bool jumpRight = true;
    private bool canJump = true;
    private bool moveEnabled = true;
    private bool isGrounded = false;
    private bool isInFollowRange = false;

    [Header("Raycast Settings")]
    [Header("Ground Detection")]
    public float groundRayLength = 3.5f;                                                // the length for ray casts that face the ground
    public float longGroundRayLength = 14f;                                             // the length for long ground ray casts

    [Header("Wall Detection")]
    public float wallRayLength = 30;                                                    // the length for ray casts that face the wall
    public float rayHeight = 22.5f;

    [Header("Ranges")]
    public float followRange = 40;                                                      // follow distance

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
    SpriteRenderer spriteRenderer;                                                      // this sprite renderer

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
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

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
        #region Nearest Enemy
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

        if (distDic.Count == 0)
        {
            return;
        }

        List<float> distances = distDic.Keys.ToList();
        distances.Sort();
        nearestTarget = distDic[distances[0]];
        distance = (transform.position - nearestTarget.transform.position);
        #endregion

        #region Ray casts
        leftInfoGround = Physics2D.Raycast(left.position, Vector2.down, groundRayLength, whatIsGround);
        rightInfoGround = Physics2D.Raycast(right.position, Vector2.down, groundRayLength, whatIsGround);

        leftInfoLongGround = Physics2D.Raycast(new Vector2(left.position.x - 2, left.position.y), Vector2.down, longGroundRayLength, whatIsGround);
        rightInfoLongGround = Physics2D.Raycast(new Vector2(right.position.x + 2, right.position.y), Vector2.down, longGroundRayLength, whatIsGround);

        targetRay = Physics2D.Raycast(transform.position, nearestTarget.transform.position, followRange);

        leftInfoUp = Physics2D.Raycast(new Vector2(left.position.x, left.position.y + rayHeight), Vector2.left, wallRayLength, whatIsGround);
        rightInfoUp = Physics2D.Raycast(new Vector2(right.position.x, right.position.y + rayHeight), Vector2.right, wallRayLength, whatIsGround);

        leftInfo = Physics2D.Raycast(left.position, Vector2.left, wallRayLength, whatIsGround);
        rightInfo = Physics2D.Raycast(right.position, Vector2.right, wallRayLength, whatIsGround);
        #endregion

        Vector2 range = new Vector2(followRange, followRange);
        #region Ground Raycasts
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
        #endregion

        #region Wall Raycasts
        if (leftInfoGround.collider == true && rightInfoGround.collider == true)
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

        #region Follow Range
        if (distance.x < followRange && distance.y < followRange && distance.x > -followRange && distance.y > -followRange)
        {
            isInFollowRange = true;
            speed += 2;
        }
        else
        {
            speed -= 2;
            isInFollowRange = false;
        }

        speed = Mathf.Clamp(speed, minSpeed, maxSpeed);

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

        if (rb.linearVelocity.y < 0) rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !isGrounded) rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2 - 1) * Time.deltaTime;
    }

    void FixedUpdate()
    {
        #region Movement
        anim.SetBool("isGrounded", isGrounded);
        if (movingRight && moveEnabled && isGrounded)
        {
            anim.SetBool("moveEnabled", true);
            spriteRenderer.flipX = false;
            Vector2 direction = ((Vector2)Vector2.right).normalized;
            Vector2 force = direction * speed * Time.deltaTime;
            rb.MovePosition(rb.position + force);
            if (direction.x > 0) jumpRight = true;
        }
        else if (!movingRight && moveEnabled && isGrounded)
        {
            anim.SetBool("moveEnabled", true);
            spriteRenderer.flipX = true;
            Vector2 direction = ((Vector2)Vector2.left).normalized;
            Vector2 force = direction * speed * Time.deltaTime;
            rb.MovePosition(rb.position + force);
            if (direction.x < 0) jumpRight = false;
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
            StartCoroutine(Jump("Small", movingRight, 2));
        }
        else anim.SetBool("moveEnabled", true);
        StartCoroutine(isStanding());
    }
    #endregion

    #region Die
    IEnumerator Die()
    {
        moveEnabled = false;
        canJump = false;
        speed = 0;
        jumpHeight = 0;
        anim.SetTrigger("Died");
        anim.SetBool("IsDead", true);
        Destroy(gameObject, 3);
        yield return null;
    }
    #endregion

    #region Jump
    IEnumerator Jump(string size, bool dirRight, float wait)
    {
        isGrounded = false;
        anim.SetTrigger("Jump");
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
        if (DEBUGMODE)
        {
            Vector2 dir = new Vector2(Mathf.Cos(rayHeight * Mathf.Deg2Rad), Mathf.Sin(rayHeight * Mathf.Deg2Rad)).normalized;
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

            if (rightInfoUp.collider == true)
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
        }
    }
    #endregion
}