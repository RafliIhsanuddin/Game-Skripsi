using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public float horizontalInput;
    public float verticalInput;
    public bool alwaysRunActive = false; // When true, player always runs

    // Wall Slide variables
    public bool isWallSliding;
    public float wallSlideSpeed = 2f;
    [SerializeField] private Transform WallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;
    private float lastWallJumpTime = 0f;
    private float wallJumpCooldown = 0.2f;
    private float lastWallJumpDirection = 1f; // Track last wall jump direction

    // Wall Jump variables
    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(15f, 15f);
    private bool isWallJumping;
    private float wallJumpDirection;
    private bool wallOnRight;

    // Wall Dash variables
    private bool isWallDashing;
    private float wallDashDirection;

    [Header("Wall Jump Force")]
    [SerializeField] public float wallJumpHorizontalForce = 15f;
    [SerializeField] public float wallJumpVerticalForce = 15f;

    // Input disable variables
    private bool inputsDisabled = false;
    private float inputDisableTimer = 0f;
    private float inputDisableDuration = 0.1f;

    public float speed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 5f;
    [SerializeField] private Vector2 groundCheckStartOffset = new Vector2(0f, -0.5f);
    [SerializeField] private float groundCheckRayLength = 1.1f;
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    private bool isGrounded;
    private Rigidbody2D rb;

    public Ghost ghost;

    private float groundTimeBuffer = 0.1f;
    private float timeSinceGrounded = 0f;

    // Dash variables
    [SerializeField] private float horizontalDashSpeed = 10f;
    [SerializeField] private float verticalDashSpeed = 7f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.01f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private bool hasAirDashed = false; // Track if player has used their air dash

    // Jump
    private float jumpDebounceTime = 0.2f;
    private float lastJumpTime = -1f;
    private bool isJumping = false;

    // Reference for flipping character
    private bool isFacingRight = true;

    [SerializeField]
    public Animator PlayerAnimationController;

    public float bodyHeightOffset = 1.0f;

    [SerializeField] private CapsuleCollider2D capsuleCollider;

    [SerializeField] ParticleSystem dust;

    // Sound GameObjects
    [SerializeField] private GameObject walkSound;
    [SerializeField] private GameObject runSound;
    [SerializeField] private GameObject jumpSoundPrefab;
    [SerializeField] private GameObject dashSoundPrefab;

    private Vector2 idleOffset = new Vector2(0.0956296921f, 0.16445756f);
    private Vector2 idleSize = new Vector2(2.33820915f, 15.3902521f);
    private Vector2 walkOffset = new Vector2(0.0295305252f, 0.371431112f);
    private Vector2 walkSize = new Vector2(2.15312004f, 14.9763098f);
    private Vector2 runOffset = new Vector2(0.0295305252f, 0.454950929f);
    private Vector2 runSize = new Vector2(2.15312004f, 14.8092728f);
    private Vector2 jumpOffset = new Vector2(0.0435304642f, 2.14305782f);
    private Vector2 jumpSize = new Vector2(1.83390617f, 10.8472614f);

    private bool isOnPlatform = false;
    private Vector2 platformVelocity = Vector2.zero;

    private bool stopRight;
    private bool stopLeft;

    private bool canJump = true;
    private bool canDash = true;

    public struct PlayerState
    {
        public Vector3 position;
        public bool isGrounded;
    }

    public List<PlayerState> positionHistory = new List<PlayerState>();
    public int historyLimit = 100;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        UpdateCollider(idleOffset, idleSize);

        walkSound.SetActive(false);
        runSound.SetActive(false);
    }

    void Update()
    {
        if (inputsDisabled)
        {
            inputDisableTimer -= Time.deltaTime;
            if (inputDisableTimer <= 0f)
            {
                inputsDisabled = false;
            }
        }

        PlayerState state = new PlayerState
        {
            position = transform.position,
            isGrounded = this.isGrounded
        };

        positionHistory.Insert(0, state);
        if (positionHistory.Count > historyLimit)
            positionHistory.RemoveAt(positionHistory.Count - 1);

        horizontalInput = inputsDisabled ? 0f : Input.GetAxisRaw("Horizontal");
        verticalInput = inputsDisabled ? 0f : Input.GetAxisRaw("Vertical");

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (stopRight && horizontalInput > 0) horizontalInput = 0;
        if (stopLeft && horizontalInput < 0) horizontalInput = 0;

        GroundCheck();
        WallSlide();

        if (isWallSliding && Input.GetKeyDown(KeyCode.Space))
        {
            WallJump();
        }
        else if (isWallSliding && Input.GetKeyDown(KeyCode.LeftShift) && canDash && dashCooldownTimer <= 0)
        {
            WallDash();
        }

        if (!isGrounded && !isWallSliding && !isDashing && !isWallDashing)
        {
            StopMovementSounds();
        }

        if (!isGrounded && !isWallSliding && !isDashing && !isWallDashing && !isJumping)
        {
            if (rb.linearVelocity.y > 0.1f)
            {
                isJumping = true;
                PlayerAnimationController.SetInteger("state", 3);
                UpdateCollider(jumpOffset, jumpSize);
            }
        }

        if (isWallSliding)
        {
            PlayerAnimationController.SetInteger("state", 5);
            StopMovementSounds();
            isJumping = false;
            hasAirDashed = false;
        }

        if (!isDashing && !isWallDashing)
        {
            if (isGrounded)
            {
                isJumping = false;
                isWallJumping = false;
                isWallDashing = false;
                hasAirDashed = false;

                if (Mathf.Abs(horizontalInput) > 0)
                {
                    ghost.makeGhost = false;
                    bool isRunning = alwaysRunActive || Input.GetKey(KeyCode.C);
                    PlayerAnimationController.SetInteger("state", isRunning ? 2 : 1);
                    UpdateCollider(isRunning ? runOffset : walkOffset, isRunning ? runSize : walkSize);

                    if (isRunning)
                    {
                        runSound.SetActive(true);
                        walkSound.SetActive(false);
                    }
                    else
                    {
                        runSound.SetActive(false);
                        walkSound.SetActive(true);
                    }

                    if (canJump && Input.GetKeyDown(KeyCode.Space))
                    {
                        Jump();
                        PlayDustEffect();
                    }
                }
                else
                {
                    ghost.makeGhost = false;
                    PlayerAnimationController.SetInteger("state", 0);
                    UpdateCollider(idleOffset, idleSize);
                    StopMovementSounds();

                    if (canJump && Input.GetKeyDown(KeyCode.Space))
                    {
                        Jump();
                        PlayDustEffect();
                    }
                }
            }

            if (!isWallSliding && !isWallJumping && !isWallDashing)
            {
                float movementSpeed = (alwaysRunActive || Input.GetKey(KeyCode.C)) ? runSpeed : speed;
                Vector2 movement = new Vector2(horizontalInput * movementSpeed, rb.linearVelocity.y);

                if (isOnPlatform)
                    rb.linearVelocity = movement + platformVelocity;
                else
                    rb.linearVelocity = movement;
            }

            bool canAirDash = !hasAirDashed;
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && dashCooldownTimer <= 0 && !inputsDisabled && (isGrounded || canAirDash))
            {
                if (!isGrounded)
                {
                    hasAirDashed = true;
                }

                PlayDustEffect();
                ghost.makeGhost = true;
                Dash(horizontalInput, verticalInput);
            }

            if (!isWallSliding && !isWallJumping && !isWallDashing)
            {
                if (horizontalInput > 0 && !isFacingRight)
                    Flip();
                else if (horizontalInput < 0 && isFacingRight)
                    Flip();
            }
        }
    }

    private void WallSlide()
    {
        bool wallDetected = IsWalled();

        if (wallDetected && !isGrounded && horizontalInput != 0)
        {
            isWallSliding = true;
            isWallJumping = false;
            isWallDashing = false;

            wallOnRight = Physics2D.Raycast(WallCheck.position, Vector2.right, 0.2f, wallLayer);
            bool wallOnLeft = Physics2D.Raycast(WallCheck.position, Vector2.left, 0.2f, wallLayer);

            if (wallOnRight && isFacingRight)
            {
                Flip();
            }
            else if (wallOnLeft && !isFacingRight)
            {
                Flip();
            }

            float currentYVelocity = Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, currentYVelocity);

            wallJumpDirection = wallOnRight ? -1f : 1f;
            wallDashDirection = wallJumpDirection;

            if (ghost != null)
            {
                ghost.makeGhost = false;
            }
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if (!canJump) return;

        inputsDisabled = true;
        inputDisableTimer = inputDisableDuration;

        isWallJumping = true;
        isWallSliding = false;
        lastWallJumpTime = Time.time;
        lastWallJumpDirection = wallJumpDirection;

        Vector2 jumpDirection = new Vector2(wallJumpDirection, 1f).normalized;
        Vector2 force = new Vector2(
            wallJumpHorizontalForce * jumpDirection.x * 1.5f,
            wallJumpVerticalForce * jumpDirection.y
        );

        rb.linearVelocity = force;

        if ((wallJumpDirection > 0f && !isFacingRight) || (wallJumpDirection < 0f && isFacingRight))
        {
            Flip();
        }

        if (jumpSoundPrefab != null)
        {
            Instantiate(jumpSoundPrefab, transform.position, Quaternion.identity);
        }
    }

    private void WallDash()
    {
        inputsDisabled = true;
        inputDisableTimer = inputDisableDuration;

        isWallDashing = true;
        isWallSliding = false;
        dashCooldownTimer = dashCooldown;

        Vector2 dashDirection = new Vector2(wallDashDirection, 0.7f).normalized;

        PlayerAnimationController.SetInteger("state", 4);
        rb.linearVelocity = new Vector2(dashDirection.x * horizontalDashSpeed, dashDirection.y * verticalDashSpeed);
        ghost.makeGhost = true;

        if (dashSoundPrefab != null)
        {
            Instantiate(dashSoundPrefab, transform.position, Quaternion.identity);
        }

        StartCoroutine(EndWallDash());
    }

    private IEnumerator EndWallDash()
    {
        yield return new WaitForSeconds(dashDuration);
        isWallDashing = false;
        ghost.makeGhost = false;

        if (!isGrounded)
        {
            PlayerAnimationController.SetInteger("state", 3);
        }
        else
        {
            PlayerAnimationController.SetInteger("state", 0);
        }
    }

    public void SetPlatformVelocity(Vector2 velocity, bool onPlatform)
    {
        platformVelocity = velocity;
        isOnPlatform = onPlatform;
    }

    private void StopMovementSounds()
    {
        walkSound.SetActive(false);
        runSound.SetActive(false);
    }

    private void GroundCheck()
    {
        Vector3 start = transform.position + (Vector3)groundCheckStartOffset;
        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, groundCheckRayLength, groundLayer | platformLayer);

        if (hit.collider != null)
        {
            timeSinceGrounded = Time.time;
            isGrounded = true;
            isWallJumping = false;
            isWallDashing = false;
        }
        else if (Time.time - timeSinceGrounded > groundTimeBuffer)
        {
            isGrounded = false;
        }
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(WallCheck.position, wallCheckRadius, wallLayer);
    }

    private void Jump()
    {
        if (Time.time - lastJumpTime > jumpDebounceTime && canJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            isJumping = true;
            lastJumpTime = Time.time;

            PlayerAnimationController.SetInteger("state", 3);
            UpdateCollider(jumpOffset, jumpSize);

            if (jumpSoundPrefab != null)
            {
                GameObject jumpSoundInstance = Instantiate(jumpSoundPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    private IEnumerator EndJumpAnimation()
    {
        while (!isGrounded)
        {
            yield return null;
        }

        if (isGrounded)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                bool isRunning = Input.GetKey(KeyCode.C);
                PlayerAnimationController.SetInteger("state", isRunning ? 2 : 1);
                UpdateCollider(isRunning ? runOffset : walkOffset, isRunning ? runSize : walkSize);
            }
            else
            {
                PlayerAnimationController.SetInteger("state", 0);
                UpdateCollider(idleOffset, idleSize);
            }
        }
    }

    private void Dash(float horizontalInput, float verticalInput)
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        Vector2 dashDirection;

        if (isWallJumping || (Time.time - lastWallJumpTime < 0.3f))
        {
            dashDirection = new Vector2(lastWallJumpDirection, 0.7f).normalized;
        }
        else
        {
            dashDirection = new Vector2(horizontalInput, verticalInput).normalized;
        }

        PlayerAnimationController.SetInteger("state", 4);
        rb.linearVelocity = new Vector2(dashDirection.x * horizontalDashSpeed, dashDirection.y * verticalDashSpeed);

        if (dashSoundPrefab != null)
        {
            Instantiate(dashSoundPrefab, transform.position, Quaternion.identity);
        }

        StartCoroutine(EndDash());
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
        ghost.makeGhost = false;

        if (!isGrounded)
        {
            PlayerAnimationController.SetInteger("state", 3);
        }
        else
        {
            PlayerAnimationController.SetInteger("state", 0);
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        if (dust != null)
        {
            Vector3 dustScale = dust.transform.localScale;
            dustScale.x *= -1;
            dust.transform.localScale = dustScale;
        }
    }

    private void UpdateCollider(Vector2 offset, Vector2 size)
    {
        if (capsuleCollider != null)
        {
            capsuleCollider.offset = offset;
            capsuleCollider.size = size;
        }
    }

    private void PlayDustEffect()
    {
        if (dust != null && !dust.isPlaying)
        {
            dust.Play();
        }
    }

    private void StopDustEffect()
    {
        if (dust != null && dust.isPlaying)
        {
            dust.Stop();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 start = transform.position + (Vector3)groundCheckStartOffset;
        Gizmos.DrawLine(start, start + Vector3.down * groundCheckRayLength);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(WallCheck.position, wallCheckRadius);
    }

    public void SetStopRight(bool value)
    {
        stopRight = value;
        stopLeft = !value;
        PlayerAnimationController.SetInteger("state", 0);
    }

    public void ResetMovement()
    {
        stopRight = false;
        stopLeft = false;
    }

    public void DisableJump()
    {
        canJump = false;
    }

    public void EnableJump()
    {
        canJump = true;
    }

    public void DisableDash()
    {
        canDash = false;
    }

    public void EnableDash()
    {
        canDash = true;
    }

}
