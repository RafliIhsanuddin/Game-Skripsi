using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{

    public float speed = 5f; // Kecepatan gerakan karakter
    public float jumpForce = 5f; // Kekuatan lompatan
    public float groundCheckDistance = 1.1f; // Jarak pengecekan tanah menggunakan Raycast
    public LayerMask groundLayer; // Layer untuk tanah (untuk memfilter raycast)
    private bool isGrounded; // Status apakah karakter berada di tanah
    private Rigidbody2D rb; // Referensi ke komponen Rigidbody2D

    // Dash variables
    [SerializeField] private float horizontalDashSpeed = 10f; // Kecepatan dash horizontal
    [SerializeField] private float verticalDashSpeed = 7f; // Kecepatan dash vertical
    public float dashDuration = 0.2f; // Durasi dash
    public float dashCooldown = 1f; // Waktu cooldown dash
    private bool isDashing = false; // Status dash
    private float dashTime = 0f; // Timer dash
    private float dashCooldownTimer = 0f; // Timer cooldown dash

    // Reference for flipping character
    private bool isFacingRight = true; // Status apakah karakter menghadap kanan

    // Double jump variables
    [SerializeField] private bool canDoubleJump = true; // Status untuk mengaktifkan double jump
    private bool hasDoubleJumped = false; // Status apakah sudah double jump

    [SerializeField]
    public Animator PlayerAnimationController;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Mengambil komponen Rigidbody2D dari objek
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");

        // Pengecekan apakah dash sedang dalam cooldown
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Pengecekan apakah karakter berada di tanah menggunakan Raycast 2D
        GroundCheck();

        // Menghandle animasi idle atau walk berdasarkan input gerakan horizontal
        if (Mathf.Abs(horizontalInput) > 0 && isGrounded && !isDashing)
        {
            PlayerAnimationController.SetInteger("state", 1); // Animasi berjalan
        }
        else if (isGrounded && !isDashing)
        {
            PlayerAnimationController.SetInteger("state", 0); // Animasi idle
        }

        if (!isDashing)
        {
            Vector2 movement = new Vector2(horizontalInput * speed, rb.velocity.y);
            rb.velocity = movement;

            // Menghandle lompatan dan double jump
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isGrounded)
                {
                    Jump(); // Lompat pertama
                }
                else if (!hasDoubleJumped && canDoubleJump)
                {
                    DoubleJump(); // Double jump jika belum double jump
                }
            }

            // Memulai dash jika tombol Shift ditekan dan cooldown sudah selesai
            if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
            {
                StartDash(horizontalInput);
            }

            // Logika untuk membalikkan karakter
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
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        if (hit.collider != null)
        {
            isGrounded = true; // Karakter berada di tanah
            hasDoubleJumped = false; // Reset double jump setelah menyentuh tanah
        }
        else
        {
            isGrounded = false; // Karakter tidak berada di tanah
        }
    }

    private void Jump()
    {
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isGrounded = false; // Setelah melompat, karakter tidak berada di tanah
        PlayerAnimationController.SetInteger("state", 2); // Set animasi lompat
    }

    private void DoubleJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); // Reset kecepatan vertikal sebelum double jump
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // Lompatan kedua
        hasDoubleJumped = true; // Setelah double jump, set true

        PlayerAnimationController.SetInteger("state", 2); // Set animasi lompat
    }

    private void StartDash(float horizontalInput)
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown; // Mengatur cooldown

        rb.velocity = new Vector2(horizontalInput * horizontalDashSpeed, rb.velocity.y);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight; // Balik status arah
        Vector3 scale = transform.localScale; // Mengambil skala objek
        scale.x *= -1; // Balik skala di sumbu X
        transform.localScale = scale; // Terapkan skala baru
    }

    // Menggambar raycast di editor untuk visualisasia
    private void OnDrawGizmos()
    {
        // Set warna Gizmos
        Gizmos.color = Color.red;

        // Menggambar garis raycast ke bawah
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }

}
