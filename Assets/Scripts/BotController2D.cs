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
public class BotController2D : MonoBehaviour
{
    #region Variables
    [Header("Stats")]
    public float health;                                                                // current health
    public float startingHealth = 100;                                                  // starting health
    public float minSpeed = 10;                                                         // determine min speed
    public float maxSpeed = 14;                                                         // determine max speed
    public float jumpHeight = 8;                                                        // the height we can jump
    public float damage = 10;                                                           // how much damage you do per hit
    float speed;                                                                        // current speed
    float gravity;
    [Header("UI")]
    public TMP_Text txtHealth;
    public Slider healthBar;

    [Header("Attacking")]
    public bool isRanged = false;                                                       // false = meele | true = ranged
    float timeToAttack;
    public float startTimeToAttack = 3;                                                 // start time of the delay in-between attacks

    [Header("Projectile")]
    Transform ProjectileSpawnRight;                                                     // transform for projectile spawning on the right side
    Transform ProjectileSpawnLeft;                                                      // transform for projectile spawning on the left side
    public Rigidbody2D projectile;                                                      // projectile you wish to fire at the player
    public float projectileOffset = 40;                                                 // projectile offset, is needed otherwise projectile flies wierd

    [Header("DEBUGING")]
    public bool DEBUGMODE = false;
    [Header("Booleans")]
    private bool movingRight = true;
    private bool jumpRight = true;
    private bool canJump = true;
    private bool moveEnabled = true;
    private bool isGrounded = false;
    private bool isInFollowRange = false;
    private bool isInAttackRange = false;
    private bool flee = false;
    private bool canAttack = false;
    private bool hasTargets = false;
    [Header("Range")]
    public float followRange = 40;                                                      // follow distance
    public float attackRange = 10;                                                      // attack distance
    public float fleeRange = 30;                                                        // fleeing distance
    public float wallRayLength = 30;                                                    // the length for ray casts that face the wall
    public float groundRayLength = 3.5f;                                                // the length for ray casts that face the ground
    public float rayHeight = 22.5f;
    public LayerMask whatIsGround;
    [Header("Transforms")]
    public List<GameObject> targets;                                                    // list for all targets that could be the nearest player
    Dictionary<float, GameObject> distDic = new Dictionary<float, GameObject>();        // used for getting the nearest player
    GameObject nearestTarget;                                                           // nearest player
    Vector2 targetPos;                                                                  // nearest players position
    public Transform left;                                                              // transform for ray casts
    public Transform right;                                                             // transform for ray casts
    //Vector3 velocity;                                                                   
    Vector3 lastPos;                                                                    // last postion of this transform
    Vector2 distance;                                                                   // distance from this to nearest target
    Animator anim;                                                                      // this animator
    Rigidbody2D rb;                                                                     // this rigidbody2d

    RaycastHit2D leftInfoGround;
    RaycastHit2D rightInfoGround;
    RaycastHit2D leftInfoLongGround;
    RaycastHit2D rightInfoLongGround;
    RaycastHit2D targetRay;
    RaycastHit2D leftInfoUp;
    RaycastHit2D RightInfoUp;
    RaycastHit2D leftInfo;
    RaycastHit2D rightInfo;
    #endregion

    void Start()
    {
        // randomize the playing style when it gets spawned
        int x = Random.Range(0, 2);
        if (x == 1) isRanged = false;
        else isRanged = true;
        if (isRanged)
        {
            startingHealth /= 2;
            healthBar.maxValue = startingHealth;
        }
        else
        {
            healthBar.maxValue = startingHealth;
        }

        // reset our health
        health = startingHealth;
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (projectile == null) Debug.LogError("You have no projectile set!");
        if (right == null) right = this.transform.GetChild(0);
        if (left == null) left = this.transform.GetChild(1);
        if (ProjectileSpawnRight == null) ProjectileSpawnRight = this.transform.GetChild(2);
        if (ProjectileSpawnLeft == null) ProjectileSpawnLeft = this.transform.GetChild(3);
        canJump = true;
        //rb.drag = 1.4f;
        rb.freezeRotation = true;


        // increase the time attack if we are ranged
        if (isRanged) startTimeToAttack += startTimeToAttack * 2;

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
        #region UI
        healthBar.value = health;
        txtHealth.text = health.ToString() + "/" + startingHealth.ToString();
        health = Mathf.Clamp(health, 0, startingHealth);
        if (health <= 0)
        {
            StartCoroutine(Die());
        }
        #endregion
        // looks for the nearest enemy 
        #region Nearest Enemy
        // In the Update() method, replace the nearest enemy section with:
        // Nearest Enemy
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

        if (distDic.Count == 0)
        {
            // No targets found, handle this case
            return;
        }

        List<float> distances = distDic.Keys.ToList();
        distances.Sort();
        nearestTarget = distDic[distances[0]];
        distance = (transform.position - nearestTarget.transform.position);
        #endregion

        #region Ray casts
        leftInfoGround = Physics2D.Raycast(left.position, Vector2.down, groundRayLength, whatIsGround); // left ground raycast
        rightInfoGround = Physics2D.Raycast(right.position, Vector2.down, groundRayLength, whatIsGround); // right ground raycast

        leftInfoLongGround = Physics2D.Raycast(new Vector2(left.position.x - 2, left.position.y), Vector2.down, groundRayLength * 4, whatIsGround); // left ground raycast
        rightInfoLongGround = Physics2D.Raycast(new Vector2(right.position.x + 2, right.position.y), Vector2.down, groundRayLength * 4, whatIsGround); // right ground raycast
        // List<Collider2D> shootingBoundry = Physics2D.OverlapBoxAll (transform.position, range, 0);

        targetRay = Physics2D.Raycast(transform.position, nearestTarget.transform.position, followRange);

        leftInfoUp = Physics2D.Raycast(new Vector2(left.position.x, left.position.y + rayHeight), Vector2.left, wallRayLength, whatIsGround); // left up raycast
        RightInfoUp = Physics2D.Raycast(new Vector2(right.position.x, right.position.y + rayHeight), Vector2.right, wallRayLength, whatIsGround); // Right up raycast

        leftInfo = Physics2D.Raycast(left.position, Vector2.left, wallRayLength, whatIsGround); // left raycast
        rightInfo = Physics2D.Raycast(right.position, Vector2.right, wallRayLength, whatIsGround); // right raycast

        #endregion
        Vector2 range = new Vector2(followRange, followRange);
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
            if (leftInfo.collider == true && leftInfo.distance <= wallRayLength / 1.5f && leftInfoUp.collider == false && !movingRight) StartCoroutine(Jump("Large", movingRight, 2));
            // if left wall ray has collided then
            if (leftInfo.collider == true)
                // if left wall ray distance is close to wall we need to turn right
                if (leftInfo.distance <= 0.1f) movingRight = true;


            // if the right ray hits somthing and the distance is about 1 / 3 to him and the right up ray hits nothing and if we are currently moving right, we want to jump right on a ledge
            if (rightInfo.collider == true && rightInfo.distance <= wallRayLength / 1.5f && RightInfoUp.collider == false && movingRight) StartCoroutine(Jump("Large", movingRight, 2));
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

        #region Attack States
        #region Meele
        // if nearest target is inside the attack range and we are not meele
        if (isInAttackRange && !isRanged)
        {
            // we need to take away time to attack so we don't spam attack, this is just for delay
            timeToAttack -= 0.1f;
            // if we are ready for an attack then
            if (timeToAttack <= 0)
            {
                // we can attack
                canAttack = true;
                // reset the attack delay
                timeToAttack = startTimeToAttack;
            }
            // if we can attack then
            if (canAttack)
            {
                if (nearestTarget.name == "Player (1)")
                    // we get the nearest targets script to takeaway health depending on set damage
                    nearestTarget.GetComponent<Player>().Health -= damage;
                // once we attack we cant attack no more
                canAttack = false;
            }
        }
        // if nearest target is within the attack range then
        if (distance.x < attackRange && distance.y * 1.5f < attackRange && distance.x > -attackRange && distance.y * 1.5f > -attackRange)
        {
            // nearest target is in attack range
            isInAttackRange = true;
            // if is ranged then we are able to move;
            if (isRanged) moveEnabled = true;
        }
        // else nearest target isn't inside the attack range then nothing is in attack range
        else isInAttackRange = false;
        #endregion
        // we calculate the targets position  so we can shoot a projectile into that direct
        Vector2 targetPos = new Vector2(nearestTarget.transform.position.x, nearestTarget.transform.position.y + projectileOffset).normalized;

        #region Ranged
        // if nearest target is inside the follow range and we are ranged and we are not in flee distance then
        if (isInFollowRange && isRanged && !flee)
        {
            // add a delay to attack
            timeToAttack -= 0.1f;
            // if delay is over
            if (timeToAttack <= 0)
            {
                // set attack to true
                canAttack = true;
                // reset delay
                timeToAttack = startTimeToAttack;
            }
            // if we can attack
            if (canAttack)
            {
                // we cant attack to avoid spam
                canAttack = false;
                // create rigid body 2d so we can controll it here.
                Rigidbody2D clone;

                // if we are currently moving right then spawn projectile on right side
                if (movingRight) clone = Instantiate(projectile, ProjectileSpawnRight.position, Quaternion.identity) as Rigidbody2D;
                // else spawn a projectile on the left side
                else clone = Instantiate(projectile, ProjectileSpawnLeft.position, Quaternion.identity) as Rigidbody2D;
            }
        }
        #endregion
        #endregion
        #region Flee
        // if nearest target enters the flee range then set flee to true
        if (distance.x < fleeRange && distance.y * 1.5f < fleeRange && distance.x > -fleeRange && distance.y * 1.5f > -fleeRange && isRanged) flee = true;
        // else the player doesnt bother us
        else flee = false;
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
                // if we are range and nearest target is in flee range then move right
                if (isRanged && flee) movingRight = true;
                // if we are ranged and nearest target is not in flee range then we move left
                if (isRanged && !flee) movingRight = false;
                // if we are not range then move left
                if (!isRanged) movingRight = false;
            }
        }
        // if nearest target is to the left of us
        if (distance.x < 0)
        { // move right
            // if nearest target is in follow range then
            if (distance.x > -followRange && distance.y > -followRange)
            {
                // if we are range and nearest target is in flee range then move left
                if (isRanged && flee) movingRight = false;
                // if we are ranged and nearest target is not in flee range then we move right
                if (isRanged && !flee) movingRight = true;
                // if we are not range then move right
                if (!isRanged) movingRight = true;
            }
        }
        #endregion

        // smoothens falling and jumping with physics
        if (rb.linearVelocity.y < 0) rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2.5f - 1) * Time.deltaTime;
        else if (rb.linearVelocity.y > 0 && !isGrounded) rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (2 - 1) * Time.deltaTime;

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
            // if nearest target is to the right, then we jump right
            if (direction.x > 0) jumpRight = true;
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
            // if nearest target is to the right, then we jump left
            if (direction.x < 0) jumpRight = false;
        }
        #endregion
    }
    #region Bug fixes
    // somtimes the AI doesnt move, like randomly stops moving, i dont know why, but i found out if the AIs ground is reset it fixes the issue, so a small jump fixes this issue just perfectly, it looks somewhat normal and adds more realism lets say
    // So in this case we can say a bug turned into a feature :)
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
    #region Die
    IEnumerator Die()
    {
        moveEnabled = false;
        canJump = false;
        speed = 0;
        jumpHeight = 0;
        timeToAttack = 999;
        canAttack = false;
        anim.SetTrigger("Died");
        anim.SetBool("IsDead", true);
        Destroy(gameObject, 3);
        yield return null;
    }
    #endregion
    #region Jump
    IEnumerator Jump(string size, bool dirRight, float wait)
    {
        // since we jump we cant be grounded
        isGrounded = false;
        // tell our animator we jumped
        anim.SetTrigger("Jump");
        // increase speed
        speed += 2;
        // move right
        movingRight = true;
        // add force up
        if (size == "Large")
        {
            //rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
            rb.linearVelocity = new Vector2(0, jumpHeight);
            // add force right
            if (dirRight) rb.AddForce(Vector2.right * jumpHeight / 2, ForceMode2D.Impulse);
            // add force right
            else rb.AddForce(Vector2.left * jumpHeight / 2, ForceMode2D.Impulse);
        }
        else if (size == "Small")
        {
            //rb.AddForce(Vector2.up * jumpHeight, ForceMode2D.Impulse);
            rb.linearVelocity = new Vector2(0, jumpHeight / 2);
            // add force right
            if (dirRight) rb.AddForce(Vector2.right * jumpHeight / 2, ForceMode2D.Impulse);
            // add force right
            else rb.AddForce(Vector2.left * jumpHeight / 2, ForceMode2D.Impulse);
        }

        // wait wait seconds before continueing
        yield return new WaitForSeconds(wait);
        // if we are grounded then we cant jump
        if (isGrounded) canJump = false;
        // we set move enabled 
        moveEnabled = true;
        // call reset jump so we don't spam jump
        StartCoroutine(resetJump());
        yield return null;
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
                Gizmos.DrawLine(new Vector2(right.position.x + 2, right.position.y), new Vector2(right.position.x + 2, right.position.y - groundRayLength * 4));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(new Vector2(right.position.x + 2, right.position.y), new Vector2(right.position.x + 2, right.position.y - groundRayLength * 4));
            }
            if (leftInfoLongGround.collider == true)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(new Vector2(left.position.x - 2, left.position.y), new Vector2(left.position.x - 2, left.position.y - groundRayLength * 4));
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(new Vector2(left.position.x - 2, left.position.y), new Vector2(left.position.x - 2, left.position.y - groundRayLength * 4));
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
            if (isInAttackRange)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, new Vector3(attackRange * 2, attackRange / 1.5f, 0));
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(attackRange * 2, attackRange / 1.5f, 0));
            }
            if (flee && isRanged)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(transform.position, new Vector3(fleeRange * 2, fleeRange / 1.5f, 0));
            }
            else if (!flee && isRanged)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireCube(transform.position, new Vector3(fleeRange * 2, fleeRange / 1.5f, 0));
            }
        }
    }
    #endregion
}