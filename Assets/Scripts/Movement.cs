using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class Movement : MonoBehaviour
{
    //public VisualEffect vfxRenderer;

    public float speed = 5f; // Kecepatan gerakan karakter
    public float runSpeed = 8f; // Kecepatan lari
    public float jumpForce = 5f; // Kekuatan lompatan
    [SerializeField] private Vector2 groundCheckStartOffset = new Vector2(0f, -0.5f);
    [SerializeField] private float groundCheckRayLength = 1.1f;
    public LayerMask groundLayer; // Layer untuk tanah
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




    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Mengambil komponen Rigidbody2D dari objek
        UpdateCollider(idleOffset, idleSize); // Set default collider untuk idle

        walkSound.SetActive(false);
        runSound.SetActive(false);
    }



    // Update is called once per frame
    void Update()
    {
        //vfxRenderer.SetVector3("ColliderPos", transform.position);
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Pengecekan apakah dash sedang dalam cooldown
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Batasi gerakan berdasarkan stopRight dan stopLeft
        if (stopRight && horizontalInput > 0)
        {
            horizontalInput = 0; // Hentikan gerakan ke kanan
        }

        if (stopLeft && horizontalInput < 0)
        {
            horizontalInput = 0; // Hentikan gerakan ke kiri
        }


        // Update collider during jump
        GroundCheck();

        // Jump animation logic
        if (!isGrounded && !isDashing)
        {
            PlayerAnimationController.SetInteger("state", 3);
            walkSound.SetActive(false);
            runSound.SetActive(false);
        }


        // Animasi dan collider berdasarkan gerakan horizontal
        if (isGrounded && !isDashing)
        {
            if (Mathf.Abs(horizontalInput) > 0)
            {
                ghost.makeGhost = false;
                // Berjalan atau berlari berdasarkan input tombol C
                PlayDustEffect();
                bool isRunning = Input.GetKey(KeyCode.C);
                PlayerAnimationController.SetInteger("state", isRunning ? 2 : 1);

                UpdateCollider(isRunning ? runOffset : walkOffset, isRunning ? runSize : walkSize);

                walkSound.SetActive(!isRunning);
                runSound.SetActive(isRunning);

                if (canJump && Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                    PlayDustEffect();
                    UpdateCollider(jumpOffset, jumpSize);
                }
            }
            else
            {
                ghost.makeGhost = false;
                // Idle
                PlayerAnimationController.SetInteger("state", 0);
                UpdateCollider(idleOffset, idleSize);

                walkSound.SetActive(false);
                runSound.SetActive(false);

                if (canJump && Input.GetKeyDown(KeyCode.Space))
                {
                    Jump();
                    PlayDustEffect();
                    UpdateCollider(jumpOffset, jumpSize);
                }
            }
        }

        // Gerakan horizontal
        if (!isDashing)
        {
            float movementSpeed = Input.GetKey(KeyCode.C) ? runSpeed : speed;
            Vector2 movement = new Vector2(horizontalInput * movementSpeed, rb.linearVelocity.y);

            if (isOnPlatform)
            {
                rb.linearVelocity = movement + platformVelocity;
            }
            else
            {
                rb.linearVelocity = movement;
            }

            // Dash
            if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && dashCooldownTimer <= 0)
            {
                PlayDustEffect();
                ghost.makeGhost = true;
                Dash(horizontalInput, verticalInput);
            }

            // Membalik arah karakter
            if (horizontalInput > 0 && !isFacingRight)
            {
                Flip();
            }
            else if (horizontalInput < 0 && isFacingRight)
            {
                Flip();
            }
        }
    }

    public void SetPlatformVelocity(Vector2 velocity, bool onPlatform)
    {
        platformVelocity = velocity;
        isOnPlatform = onPlatform;
    }

    private void GroundCheck()
    {
        Vector3 start = transform.position + (Vector3)groundCheckStartOffset;
        RaycastHit2D hit = Physics2D.Raycast(start, Vector2.down, groundCheckRayLength, groundLayer);

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

    private void Jump()
    {
        if (Time.time - lastJumpTime > jumpDebounceTime)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            isGrounded = false;
            lastJumpTime = Time.time;

            // Aktifkan animasi lompat
            PlayerAnimationController.SetInteger("state", 3);
            StartCoroutine(EndJumpAnimation());

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

    // Menggambar raycast di editor untuk visualisasi
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

    // Metode untuk mengaktifkan kembali kemampuan melompat
    public void EnableJump()
    {
        canJump = true;
    }

    public void DisableDash()
    {
        canDash = false;
    }

    // Metode untuk mengaktifkan kembali kemampuan dash
    public void EnableDash()
    {
        canDash = true;
    }

}
