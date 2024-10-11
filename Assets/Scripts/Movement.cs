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
    public float dashSpeed = 10f; // Kecepatan dash
    public float dashDuration = 0.2f; // Durasi dash
    public float dashCooldown = 1f; // Waktu cooldown dash
    private bool isDashing = false; // Status dash
    private float dashTime = 0f; // Timer dash
    private float dashCooldownTimer = 0f; // Timer cooldown dash

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Mengambil komponen Rigidbody2D dari objek
    }

    // Update is called once per frame
    void Update()
    {
        // Mengambil input dari tombol A/D atau arah kiri/kanan
        float horizontalInput = Input.GetAxis("Horizontal");

        // Pengecekan apakah dash sedang dalam cooldown
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // Menghitung gerakan karakter di sumbu X
        Vector2 movement = new Vector2(horizontalInput * speed, rb.velocity.y);

        // Pengecekan apakah karakter berada di tanah menggunakan Raycast 2D
        GroundCheck();

        // Jika karakter sedang tidak dash, gunakan gerakan normal
        if (!isDashing)
        {
            rb.velocity = movement;

            // Menghandle lompatan jika karakter berada di tanah
            if (isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
            }

            // Memulai dash jika tombol Shift ditekan dan cooldown sudah selesai
            if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0)
            {
                StartDash(horizontalInput);
            }
        }
        else
        {
            // Jika sedang dash, lanjutkan gerakan dash
            Dash(horizontalInput);
        }
    }

    private void GroundCheck()
    {
        // Membuat Raycast dari posisi karakter ke bawah
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        if (hit.collider != null)
        {
            isGrounded = true; // Karakter berada di tanah
        }
        else
        {
            isGrounded = false; // Karakter tidak berada di tanah
        }

        Debug.Log("Grounded: " + isGrounded);
    }

    private void Jump()
    {
        // Mengaplikasikan gaya lompat pada Rigidbody2D
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        isGrounded = false; // Setelah melompat, karakter tidak berada di tanah
    }

    private void StartDash(float horizontalInput)
    {
        isDashing = true;
        dashTime = dashDuration;
        dashCooldownTimer = dashCooldown;

        // Mengatur kecepatan dash sesuai arah input horizontal (ke kiri atau ke kanan)
        rb.velocity = new Vector2(horizontalInput * dashSpeed, rb.velocity.y);
    }

    private void Dash(float horizontalInput)
    {
        // Mengurangi timer dash
        dashTime -= Time.deltaTime;

        // Jika dash selesai, kembali ke keadaan normal
        if (dashTime <= 0)
        {
            isDashing = false;
        }
    }

    // Menggambar raycast di editor untuk visualisasi
    private void OnDrawGizmos()
    {
        // Set warna Gizmos
        Gizmos.color = Color.red;

        // Menggambar garis raycast ke bawah
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }

}
