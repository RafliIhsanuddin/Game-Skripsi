using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{



    public bool alwaysRunActive = false; // When true, player always runs

    //WallSlide variables
    public bool isWallSliding;
    public float wallSlideSpeed = 2f;
    [SerializeField] private Transform WallCheck;
    [SerializeField] private LayerMask wallLayer;

    // Input disable variables
    private bool inputsDisabled = false;
    private float inputDisableTimer = 0f;
    private float inputDisableDuration = 0.1f;

    //WallJump variables
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    private float wallJumpingDuration = 0.4f;
    private Vector2 wallJumpingPower = new Vector2(5f, 10f); // Kekuatan lompatan saat wall jump

    public float speed = 5f; // Kecepatan gerakan karakter
    public float runSpeed = 8f; // Kecepatan lari
    public float jumpForce = 5f; // Kekuatan lompatan
    [SerializeField] private Vector2 groundCheckStartOffset = new Vector2(0f, -0.5f);
    [SerializeField] private float groundCheckRayLength = 1.1f;
    public LayerMask groundLayer; // Layer untuk tanah
    public LayerMask platformLayer;
    private bool isGrounded; // Status apakah karakter berada di tanah
    private Rigidbody2D rb; // Referensi ke komponen Rigidbody2D

    public Ghost ghost;

    private float groundTimeBuffer = 0.1f; // Toleransi waktu sebelum menganggap karakter di tanah
    private float timeSinceGrounded = 0f;

    // Dash variables
    [SerializeField] private float horizontalDashSpeed = 10f; // Kecepatan dash horizontal
    [SerializeField] private float verticalDashSpeed = 7f; // Kecepatan dash vertikal
    public float dashDuration = 0.1f; // Durasi dash
    public float dashCooldown = 0.01f; // Waktu cooldown dash
    private bool isDashing = false; // Status dash
    private float dashCooldownTimer = 0f; // Timer cooldown dash

    // Jump
    private float jumpDebounceTime = 0.2f;
    private float lastJumpTime = -1f;
    private bool isJumping = false; // Track if we're in a jump

    // Reference for flipping character
    private bool isFacingRight = true; // Status apakah karakter menghadap kanan

    [SerializeField]
    public Animator PlayerAnimationController;

    // Offset for ground check
    public float bodyHeightOffset = 1.0f;

    [SerializeField] private CapsuleCollider2D capsuleCollider;

    [SerializeField] ParticleSystem dust;

    // Sound GameObjects
    [SerializeField] private GameObject walkSound;
    [SerializeField] private GameObject runSound;

    [SerializeField] private GameObject jumpSoundPrefab; // Prefab untuk suara lompat
    [SerializeField] private GameObject dashSoundPrefab; // Prefab untuk suara dash

    private Vector2 idleOffset = new Vector2(0.0956296921f, 0.16445756f);
    private Vector2 idleSize = new Vector2(2.33820915f, 15.3902521f);

    private Vector2 walkOffset = new Vector2(0.0295305252f, 0.371431112f);
    private Vector2 walkSize = new Vector2(2.15312004f, 14.9763098f);

    private Vector2 runOffset = new Vector2(0.0295305252f, 0.454950929f);
    private Vector2 runSize = new Vector2(2.15312004f, 14.8092728f);

    private Vector2 jumpOffset = new Vector2(0.0435304642f, 2.14305782f);
    private Vector2 jumpSize = new Vector2(1.83390617f, 10.8472614f);

    private bool isOnPlatform = false; // Status apakah karakter berada di platform
    private Vector2 platformVelocity = Vector2.zero; // Kecepatan platform

    private bool stopRight; // Status apakah karakter tidak dapat bergerak ke kanan
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
        rb = GetComponent<Rigidbody2D>(); // Mengambil komponen Rigidbody2D dari objek
        UpdateCollider(idleOffset, idleSize); // Set default collider untuk idle

        walkSound.SetActive(false);
        runSound.SetActive(false);
    }

    void Update()
    {
        // Handle input disable timer
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

        float horizontalInput = inputsDisabled ? 0f : Input.GetAxisRaw("Horizontal");
        float verticalInput = inputsDisabled ? 0f : Input.GetAxisRaw("Vertical");

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;

        if (stopRight && horizontalInput > 0) horizontalInput = 0;
        if (stopLeft && horizontalInput < 0) horizontalInput = 0;

        GroundCheck();
        WallSlide();

        // Stop movement sounds if not grounded
        if (!isGrounded && !isWallSliding && !isDashing)
        {
            StopMovementSounds();
        }

        // Handle jump animation state
        if (!isGrounded && !isWallSliding && !isDashing && !isJumping)
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
        }

        if (!isDashing && !isWallSliding)
        {
            if (isGrounded)
            {
                isJumping = false;

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

            float movementSpeed = (alwaysRunActive || Input.GetKey(KeyCode.C)) ? runSpeed : speed;
            Vector2 movement = new Vector2(horizontalInput * movementSpeed, rb.linearVelocity.y);

            if (isOnPlatform)
                rb.linearVelocity = movement + platformVelocity;
            else
                rb.linearVelocity = movement;

            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && dashCooldownTimer <= 0 && !inputsDisabled)
            {
                PlayDustEffect();
                ghost.makeGhost = true;
                Dash(horizontalInput, verticalInput);
            }

            if (horizontalInput > 0 && !isFacingRight)
                Flip();
            else if (horizontalInput < 0 && isFacingRight)
                Flip();
        }

        Debug.Log("Jumping: " + isJumping + ", Wall Sliding: " + isWallSliding + ", Grounded: " + isGrounded + ", Dashing: " + isDashing);
    }

    private void WallSlide()
    {
        bool wallDetected = IsWalled();

        if (wallDetected && !isGrounded)
        {
            if (isDashing)
            {
                isDashing = false;
                StopCoroutine(EndDash()); // Hentikan dash jika menyentuh wall
            }

            if (!isWallSliding)
            {
                // Just started wall sliding - disable inputs
                inputsDisabled = true;
                inputDisableTimer = inputDisableDuration;
            }

            isWallSliding = true;

            // Batasi kecepatan jatuh saat wall slide
            rb.linearVelocity = new Vector2(0f, Mathf.Clamp(rb.linearVelocity.y, -wallSlideSpeed, float.MaxValue));

            // Matikan ghost saat sliding di dinding
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

    // ... [Rest of the code remains exactly the same as in your original version] ...
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
        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, groundCheckRayLength, groundLayer | platformLayer); // Gunakan OR untuk memeriksa kedua layer

        if (hit.collider != null)
        {
            timeSinceGrounded = Time.time;
            isGrounded = true;
        }
        else if (Time.time - timeSinceGrounded > groundTimeBuffer)
        {
            isGrounded = false;
        }
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(WallCheck.position, 0.2f, wallLayer);
    }

    private void Jump()
    {
        if (Time.time - lastJumpTime > jumpDebounceTime && canJump)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            isJumping = true;
            lastJumpTime = Time.time;

            // Set jump animation immediately
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
        // Tunggu hingga karakter benar-benar mendarat
        while (!isGrounded)
        {
            yield return null; // Tunggu satu frame
        }

        // Setelah mendarat, periksa status gerakan untuk kembali ke animasi yang sesuai
        if (isGrounded)
        {
            if (Mathf.Abs(rb.linearVelocity.x) > 0.1f) // Jika karakter bergerak horizontal
            {
                bool isRunning = Input.GetKey(KeyCode.C);
                PlayerAnimationController.SetInteger("state", isRunning ? 2 : 1);
                UpdateCollider(isRunning ? runOffset : walkOffset, isRunning ? runSize : walkSize);
            }
            else // Jika karakter diam
            {
                PlayerAnimationController.SetInteger("state", 0);
                UpdateCollider(idleOffset, idleSize);
            }
        }
    }

    private void Dash(float horizontalInput, float verticalInput)
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown; // Atur cooldown
        Vector2 dashDirection = new Vector2(horizontalInput, verticalInput).normalized;

        // Set animasi dash sesuai arah dash
        PlayerAnimationController.SetInteger("state", 4);

        // Terapkan kecepatan dash
        rb.linearVelocity = new Vector2(dashDirection.x * horizontalDashSpeed, dashDirection.y * verticalDashSpeed);

        if (dashSoundPrefab != null)
        {
            Instantiate(dashSoundPrefab, transform.position, Quaternion.identity);
        }

        // Cek apakah lompat saat dash
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        // Kembali ke kondisi normal setelah durasi dash
        StartCoroutine(EndDash());
    }

    private IEnumerator EndDash()
    {
        yield return new WaitForSeconds(dashDuration);
        isDashing = false; // Akhiri dash
        if (isGrounded)
        {
            PlayerAnimationController.SetInteger("state", 0); // Kembali ke idle jika di tanah
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight; // Balik status arah
        Vector3 scale = transform.localScale; // Mengambil skala objek
        scale.x *= -1; // Balik skala di sumbu X
        transform.localScale = scale; // Terapkan skala baru

        if (dust != null)
        {
            Vector3 dustScale = dust.transform.localScale;
            dustScale.x *= -1; // Balik sumbu X partikel
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
    }

    public void SetStopRight(bool value)
    {
        stopRight = value;
        stopLeft = !value; // Jika stopRight true, maka stopLeft false, dan sebaliknya

        // Set animasi idle saat terkena collider
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
