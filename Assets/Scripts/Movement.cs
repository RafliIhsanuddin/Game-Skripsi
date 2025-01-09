using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class Movement : MonoBehaviour
{
    public VisualEffect vfxRenderer;

    public float speed = 5f; // Kecepatan gerakan karakter
    public float runSpeed = 8f; // Kecepatan lari
    public float jumpForce = 5f; // Kekuatan lompatan
    public float groundCheckDistance = 1.1f; // Jarak pengecekan tanah menggunakan Raycast
    public LayerMask groundLayer; // Layer untuk tanah
    private bool isGrounded; // Status apakah karakter berada di tanah
    private Rigidbody2D rb; // Referensi ke komponen Rigidbody2D

    public Ghost ghost;

    private float groundTimeBuffer = 0.1f; // Toleransi waktu sebelum menganggap karakter di tanah
    private float timeSinceGrounded = 0f;

    // Dash variables
    [SerializeField] private float horizontalDashSpeed = 10f; // Kecepatan dash horizontal
    [SerializeField] private float verticalDashSpeed = 7f; // Kecepatan dash vertikal
    public float dashDuration = 0.2f; // Durasi dash
    public float dashCooldown = 1f; // Waktu cooldown dash
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

    private Vector2 idleOffset = new Vector2(0.0956296921f, 0.16445756f);
    private Vector2 idleSize = new Vector2(2.33820915f, 15.3902521f);

    private Vector2 walkOffset = new Vector2(0.0295305252f, 0.371431112f);
    private Vector2 walkSize = new Vector2(2.15312004f, 14.9763098f);

    private Vector2 runOffset = new Vector2(0.0295305252f, 0.454950929f);
    private Vector2 runSize = new Vector2(2.15312004f, 14.8092728f);

    private Vector2 jumpOffset = new Vector2(0.0435304642f, 2.14305782f);
    private Vector2 jumpSize = new Vector2(1.83390617f, 10.8472614f);

    /*private float scale = 0.2f; // Skala untuk collider

    private Vector2 idleOffset => new Vector2(0.0139846802f, 0.00200867653f) * scale;
    private Vector2 idleSize => new Vector2(2.33820915f, 15.3902521f) * scale;

    private Vector2 walkOffset => new Vector2(0.0139846802f, 0.0323162079f) * scale;
    private Vector2 walkSize => new Vector2(0.415477753f, 2.95295954f) * scale;

    private Vector2 runOffset => new Vector2(0.0139846802f, 0.0626237392f) * scale;
    private Vector2 runSize => new Vector2(0.415477753f, 2.89234447f) * scale;*/

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Mengambil komponen Rigidbody2D dari objek
        UpdateCollider(idleOffset, idleSize); // Set default collider untuk idle
    }

    // Update is called once per frame
    void Update()
    {
        vfxRenderer.SetVector3("ColliderPos", transform.position);
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Pengecekan apakah dash sedang dalam cooldown
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Pengecekan apakah karakter berada di tanah menggunakan Raycast 2D
        GroundCheck();

        // Update collider during jump
        if (!isGrounded)
        {
            // Update collider for jumping state
            UpdateCollider(jumpOffset, jumpSize);
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

                if (Input.GetKeyDown(KeyCode.Space))
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

                if (Input.GetKeyDown(KeyCode.Space))
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
            rb.linearVelocity = movement;

            // Dash
            if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
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

    private void GroundCheck()
    {
        Vector3 bodyCenter = transform.position + new Vector3(0, bodyHeightOffset, 0);
        RaycastHit2D hit = Physics2D.Raycast(bodyCenter, Vector2.down, groundCheckDistance, groundLayer);

        if (hit.collider != null)
        {
            timeSinceGrounded = Time.time; // Catat waktu terakhir kali menyentuh tanah
            isGrounded = true;
        }
        else if (Time.time - timeSinceGrounded > groundTimeBuffer)
        {
            isGrounded = false; // Hanya ubah isGrounded jika sudah melewati buffer
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
            if (Mathf.Abs(rb.linearVelocity.x) > 0)
            {
                // Jika bergerak horizontal, set animasi berjalan atau lari
                bool isRunning = Input.GetKey(KeyCode.C);
                PlayerAnimationController.SetInteger("state", isRunning ? 2 : 1);
                UpdateCollider(isRunning ? runOffset : walkOffset, isRunning ? runSize : walkSize);
            }
            else
            {
                // Jika diam, set animasi idle
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
        Vector3 bodyCenter = transform.position + new Vector3(0, bodyHeightOffset, 0);
        Gizmos.DrawLine(bodyCenter, bodyCenter + Vector3.down * groundCheckDistance);
    }
}
