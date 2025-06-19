using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    public bool isMovementLocked = false; // Digunakan untuk mengunci input saat dialog
    public bool dashLocked = true;
    public bool dashEnabled = true; // New boolean to toggle dash ability

    public float horizontalInput;
    public float verticalInput;
    public bool alwaysRunActive = false; // When true, player always runs

    // Wall Slide variables
    public bool isWallSliding;
    public float wallSlideSpeed = 2f;
    [SerializeField] private LayerMask wallLayer;

    [Header("Wall Detection")]
    [SerializeField] private float wallCheckRightDistance = 0.2f;
    [SerializeField] private float wallCheckLeftDistance = 0.2f;
    private float lastWallJumpTime = 0f;
    private float wallJumpCooldown = 0.2f;
    private float lastWallJumpDirection = 1f; // Track last wall jump direction

    // Wall Jump variables
    [Header("Wall Jump")]
    public Vector2 wallJumpForce = new Vector2(15f, 15f);
    private bool isWallJumping;
    private float wallJumpDirection;
    public bool wallOnRight;
    public bool wallOnLeft;

    // Wall Dash variables
    private bool isWallDashing;
    private float wallDashDirection;
    private bool isWallDashLocked = false; // New variable to track wall dash direction lock
    private bool hasWallDashedInAir = false; // Track if player has wall dashed in air

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

    // Double Jump variables
    [Header("Double Jump")]
    public bool doubleJumpEnabled = true; // Toggle for double jump functionality
    private bool canDoubleJump = false; // Track if player can perform a double jump
    private bool hasDoubleJumped = false; // Track if player has used their double jump

    // Wall Jump Double Jump variables
    [Header("Wall Jump Double Jump")]
    public bool wallJumpDoubleJumpEnabled = true; // Toggle for wall jump double jump functionality
    private bool canWallJumpDoubleJump = false; // Track if player can perform a double jump after wall jump
    private bool hasWallJumpDoubleJumped = false; // Track if player has used their wall jump double jump
    private bool hasWallJumped = false; // New flag to track if we've done a wall jump

    // Wall Dash Double Jump variables
    [Header("Wall Dash Double Jump")]
    public bool wallDashDoubleJumpEnabled = true; // Toggle for wall dash double jump functionality
    private bool canWallDashDoubleJump = false; // Track if player can perform a double jump after wall dash
    private bool hasWallDashDoubleJumped = false; // Track if player has used their wall dash double jump
    private bool hasUsedFirstJumpAfterWallDash = false; // Track if first jump after wall dash was used

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
    public Vector2 runOffset = new Vector2(0.0295305252f, 0.454950929f);
    public Vector2 runSize = new Vector2(2.15312004f, 14.8092728f);
    [SerializeField] public Vector2 jumpOffset = new Vector2(0.361042f, 2.143058f);
    [SerializeField] public Vector2 jumpSize = new Vector2(8.456917f, 10.84726f);

    private bool isOnPlatform = false;
    private Vector2 platformVelocity = Vector2.zero;

    private bool stopRight;
    private bool stopLeft;

    private bool canJump = true;
    public bool canDash = true;

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

        canDash = false;
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

        horizontalInput = (inputsDisabled || isMovementLocked) ? 0f : Input.GetAxisRaw("Horizontal");
        verticalInput = (inputsDisabled || isMovementLocked) ? 0f : Input.GetAxisRaw("Vertical");

        if (isMovementLocked)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            UpdateCollider(idleOffset, idleSize);
            PlayerAnimationController.SetInteger("state", 0);
            StopMovementSounds();
            return;
        }

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
        else if (isWallSliding && Input.GetKeyDown(KeyCode.LeftShift) && canDash && dashCooldownTimer <= 0 && dashEnabled) // Added dashEnabled check
        {
            WallDash();
        }

        if (!isGrounded && !isWallSliding && !isDashing && !isWallDashing)
        {
            StopMovementSounds();
        }

        // Automatically set to jump state when in air
        if (!isGrounded && !isWallSliding && !isDashing && !isWallDashing)
        {
            PlayerAnimationController.SetInteger("state", 3); // Set to jump state
            UpdateCollider(jumpOffset, jumpSize);
            isJumping = true;
        }

        if (isWallSliding)
        {
            PlayerAnimationController.SetInteger("state", 7);
            StopMovementSounds();
            isJumping = false;
            hasAirDashed = false;
            hasWallDashedInAir = false; // Reset wall dash tracking when wall sliding
            isWallDashLocked = false; // Reset lock when wall sliding
            ResetWallDashDoubleJump(); // Reset wall dash double jump when wall sliding
        }

        if (!isDashing && !isWallDashing)
        {
            if (isGrounded)
            {
                isJumping = false;
                isWallJumping = false;
                hasWallJumped = false; // Reset wall jump tracking when grounded
                isWallDashing = false;
                hasAirDashed = false;
                hasWallDashedInAir = false; // Reset wall dash tracking when grounded
                isWallDashLocked = false; // Reset lock when grounded
                canDoubleJump = true; // Reset double jump when grounded
                hasDoubleJumped = false; // Reset double jump tracking when grounded
                canWallJumpDoubleJump = false; // Reset wall jump double jump when grounded
                hasWallJumpDoubleJumped = false; // Reset wall jump double jump tracking when grounded
                ResetWallDashDoubleJump(); // Reset wall dash double jump when grounded

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

            // Handle ground double jump when in air and not wall sliding
            if (!isGrounded && !isWallSliding && !isWallJumping && !isWallDashing && doubleJumpEnabled)
            {
                if (canDoubleJump && !hasDoubleJumped && Input.GetKeyDown(KeyCode.Space))
                {
                    DoubleJump();
                }
            }

            // Handle wall jump double jump when in air after wall jump (regardless of dash state)
            if (!isGrounded && !isWallSliding && wallJumpDoubleJumpEnabled && hasWallJumped && !IsWalled())
            {
                if (canWallJumpDoubleJump && !hasWallJumpDoubleJumped && Input.GetKeyDown(KeyCode.Space))
                {
                    WallJumpDoubleJump();
                }
            }

            // Handle first jump after wall dash
            if (!isGrounded && !isWallSliding && wallDashDoubleJumpEnabled && hasWallDashedInAir && !IsWalled())
            {
                if (canWallDashDoubleJump && !hasUsedFirstJumpAfterWallDash && Input.GetKeyDown(KeyCode.Space))
                {
                    FirstJumpAfterWallDash();
                }
                // Handle second jump (double jump) after wall dash
                else if (hasUsedFirstJumpAfterWallDash && !hasWallDashDoubleJumped && Input.GetKeyDown(KeyCode.Space))
                {
                    WallDashDoubleJump();
                }
            }

            if (!isWallSliding && !isWallJumping && !isWallDashing)
            {
                float movementSpeed = (alwaysRunActive || Input.GetKey(KeyCode.C)) ? runSpeed : speed;
                Vector2 movement = new Vector2(horizontalInput * movementSpeed, rb.linearVelocity.y);

                // Apply movement only if not in wall dash locked state
                if (!isWallDashLocked)
                {
                    if (isOnPlatform)
                        rb.linearVelocity = movement + platformVelocity;
                    else
                        rb.linearVelocity = movement;
                }
            }

            bool canAirDash = !hasAirDashed && !hasWallDashedInAir; // Modified condition to include wall dash check
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && dashCooldownTimer <= 0 && !inputsDisabled && (isGrounded || canAirDash) && dashEnabled) // Added dashEnabled check
            {
                if (!isGrounded)
                {
                    hasAirDashed = true;
                }

                PlayDustEffect();
                ghost.makeGhost = true;
                Dash(horizontalInput, verticalInput);
            }

            if (!isWallSliding && !isWallJumping && !isWallDashing && !isWallDashLocked)
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
            canDoubleJump = true; // Reset double jump when wall sliding
            hasDoubleJumped = false; // Reset double jump tracking when wall sliding
            canWallJumpDoubleJump = false; // Reset wall jump double jump when wall sliding
            hasWallJumpDoubleJumped = false; // Reset wall jump double jump tracking when wall sliding
            ResetWallDashDoubleJump(); // Reset wall dash double jump when wall sliding

            wallOnRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckRightDistance, wallLayer);
            wallOnLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckLeftDistance, wallLayer);

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
        hasWallJumped = true; // Mark that we've done a wall jump
        isWallSliding = false;
        lastWallJumpTime = Time.time;
        lastWallJumpDirection = wallJumpDirection;

        // Enable wall jump double jump
        if (wallJumpDoubleJumpEnabled)
        {
            canWallJumpDoubleJump = true;
            hasWallJumpDoubleJumped = false;
        }

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

        PlayerAnimationController.SetTrigger("Jump");
        PlayerAnimationController.SetInteger("state", 3);

        if (jumpSoundPrefab != null)
        {
            Instantiate(jumpSoundPrefab, transform.position, Quaternion.identity);
        }
    }

    private void WallJumpDoubleJump()
    {
        Debug.Log("WallJump double jump performed");

        // Reset vertical velocity before applying jump force for consistent height
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        hasWallJumpDoubleJumped = true;
        canWallJumpDoubleJump = false;

        PlayerAnimationController.SetTrigger("Jump");
        PlayerAnimationController.SetInteger("state", 3);
        UpdateCollider(jumpOffset, jumpSize);

        if (jumpSoundPrefab != null)
        {
            GameObject jumpSoundInstance = Instantiate(jumpSoundPrefab, transform.position, Quaternion.identity);
        }

        PlayDustEffect();
    }

    private void FirstJumpAfterWallDash()
    {
        Debug.Log("First jump after wall dash performed");

        // Regular jump behavior
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        hasUsedFirstJumpAfterWallDash = true;

        PlayerAnimationController.SetTrigger("Jump");
        PlayerAnimationController.SetInteger("state", 3);
        UpdateCollider(jumpOffset, jumpSize);

        if (jumpSoundPrefab != null)
        {
            GameObject jumpSoundInstance = Instantiate(jumpSoundPrefab, transform.position, Quaternion.identity);
        }

        PlayDustEffect();
    }

    private void WallDashDoubleJump()
    {
        Debug.Log("WallDash double jump performed");

        // Reset vertical velocity before applying jump force for consistent height
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

        hasWallDashDoubleJumped = true;
        canWallDashDoubleJump = false;

        // After using wall dash double jump, disable dashing
        canDash = false;

        PlayerAnimationController.SetTrigger("Jump");
        PlayerAnimationController.SetInteger("state", 3);
        UpdateCollider(jumpOffset, jumpSize);

        if (jumpSoundPrefab != null)
        {
            GameObject jumpSoundInstance = Instantiate(jumpSoundPrefab, transform.position, Quaternion.identity);
        }

        PlayDustEffect();
    }

    private void WallDash()
    {
        inputsDisabled = true;
        inputDisableTimer = inputDisableDuration;

        isWallDashing = true;
        isWallSliding = false;
        dashCooldownTimer = dashCooldown;
        hasWallDashedInAir = true; // Mark that we've used a wall dash in air

        // Enable wall dash double jump after wall dash
        if (wallDashDoubleJumpEnabled)
        {
            canWallDashDoubleJump = true;
            hasWallDashDoubleJumped = false;
            hasUsedFirstJumpAfterWallDash = false;
        }

        Vector2 dashDirection = new Vector2(wallDashDirection, 0.7f).normalized;

        PlayerAnimationController.SetInteger("state", 4);
        rb.linearVelocity = new Vector2(dashDirection.x * horizontalDashSpeed, dashDirection.y * verticalDashSpeed);
        ghost.makeGhost = true;

        StopMovementSounds(); // Stop movement sounds during dash
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

        // Set wall dash locked state
        isWallDashLocked = true;

        if (!isGrounded)
        {
            PlayerAnimationController.SetInteger("state", 3); // Return to jump state after dash
        }
        else
        {
            PlayerAnimationController.SetInteger("state", 0);
            isWallDashLocked = false;
        }
    }

    private void Jump()
    {
        if (Time.time - lastJumpTime > jumpDebounceTime && canJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            isJumping = true;
            lastJumpTime = Time.time;

            // Enable double jump after first jump
            if (doubleJumpEnabled)
            {
                canDoubleJump = true;
            }

            PlayerAnimationController.SetTrigger("Jump");
            PlayerAnimationController.SetInteger("state", 3);
            UpdateCollider(jumpOffset, jumpSize);

            if (jumpSoundPrefab != null)
            {
                GameObject jumpSoundInstance = Instantiate(jumpSoundPrefab, transform.position, Quaternion.identity);
            }
        }
    }

    private void DoubleJump()
    {
        if (canDoubleJump && !hasDoubleJumped)
        {
            // Reset vertical velocity before applying jump force for consistent height
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

            hasDoubleJumped = true;
            canDoubleJump = false;

            PlayerAnimationController.SetTrigger("Jump");
            PlayerAnimationController.SetInteger("state", 3);
            UpdateCollider(jumpOffset, jumpSize);

            if (jumpSoundPrefab != null)
            {
                GameObject jumpSoundInstance = Instantiate(jumpSoundPrefab, transform.position, Quaternion.identity);
            }

            PlayDustEffect();
        }
    }

    private void ResetWallDashDoubleJump()
    {
        canWallDashDoubleJump = false;
        hasWallDashDoubleJumped = false;
        hasUsedFirstJumpAfterWallDash = false;
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
            hasWallJumped = false; // Reset wall jump tracking when grounded
            isWallDashing = false;
            isWallDashLocked = false; // Reset lock when grounded
            hasWallDashedInAir = false; // Reset wall dash tracking when grounded
            canDoubleJump = true; // Reset double jump when grounded
            hasDoubleJumped = false; // Reset double jump tracking when grounded
            canWallJumpDoubleJump = false; // Reset wall jump double jump when grounded
            hasWallJumpDoubleJumped = false; // Reset wall jump double jump tracking when grounded
            ResetWallDashDoubleJump(); // Reset wall dash double jump when grounded
            canDash = true; // Re-enable dash when grounded
        }
        else if (Time.time - timeSinceGrounded > groundTimeBuffer)
        {
            isGrounded = false;
        }
    }

    private bool IsWalled()
    {
        wallOnRight = Physics2D.Raycast(transform.position, Vector2.right, wallCheckRightDistance, wallLayer);
        wallOnLeft = Physics2D.Raycast(transform.position, Vector2.left, wallCheckLeftDistance, wallLayer);
        return wallOnRight || wallOnLeft;
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

        StopMovementSounds(); // Stop movement sounds during dash
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
            PlayerAnimationController.SetInteger("state", 3); // Return to jump state after dash
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
        // Ground check debug
        Gizmos.color = Color.red;
        Vector3 start = transform.position + (Vector3)groundCheckStartOffset;
        Gizmos.DrawLine(start, start + Vector3.down * groundCheckRayLength);

        // Wall check debug
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.right * wallCheckRightDistance);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.left * wallCheckLeftDistance);
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
        Debug.Log("kong");
        dashEnabled = true;
    }


}
