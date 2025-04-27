using System.Collections.Generic;
using UnityEngine;

public class Companion : MonoBehaviour
{

    public Transform player;
    public float chaseSpeed = 2f;
    public float jumpForce = 2f;
    public LayerMask groundLayer;


    private Rigidbody2D rb;
    private bool isGrounded;
    private bool shouldJump;
    public float playerDetectionHeight = 6f; // Make the raycast distance public
    public float platformDetectionHeight = 3f;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, 1f, groundLayer);
        Debug.DrawRay(transform.position, Vector2.down * 1f, Color.green);

        float direction = Mathf.Sign(player.position.x - transform.position.x);

        Vector2 frontCheckOrigin = transform.position;
        Vector2 frontCheckDirection = new Vector2(direction, 0);
        RaycastHit2D groundInFront = Physics2D.Raycast(frontCheckOrigin, frontCheckDirection, 2f, groundLayer);
        Debug.DrawRay(frontCheckOrigin, frontCheckDirection * 2f, Color.red);

        Vector2 gapCheckOrigin = transform.position + new Vector3(direction, 0, 0);
        RaycastHit2D gapAhead = Physics2D.Raycast(gapCheckOrigin, Vector2.down, 2f, groundLayer);
        Debug.DrawRay(gapCheckOrigin, Vector2.down * 2f, Color.yellow);

        // Raycast untuk mendeteksi pemain di atas
        RaycastHit2D playerAbove = Physics2D.Raycast(transform.position, Vector2.up, playerDetectionHeight, 1 << player.gameObject.layer);
        Debug.DrawRay(transform.position, Vector2.up * playerDetectionHeight, Color.blue);  // Warna biru

        // Raycast untuk platform di atas
        RaycastHit2D platformAbove = Physics2D.Raycast(transform.position, Vector2.up, platformDetectionHeight, groundLayer);
        Debug.DrawRay(transform.position, Vector2.up * platformDetectionHeight, Color.magenta);


        // Menampilkan log apakah pemain terdeteksi di atas
        bool isPlayerAbove = playerAbove.collider != null;  // Cek apakah raycast mendeteksi player di atas
        Debug.Log("isPlayerAbove: " + isPlayerAbove);  // Menampilkan status pemain di atas

        if (platformAbove.collider != null)
        {
            Debug.Log("Platform di atas terdeteksi: " + platformAbove.collider.name);
        }
        else
        {
            Debug.Log("Tidak ada platform di atas.");
        }

        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocity.y);

            if (!groundInFront.collider && !gapAhead.collider)
            {
                shouldJump = true;
            }
            else if (isPlayerAbove && platformAbove.collider)
            {
                shouldJump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if (isGrounded && shouldJump) {
            shouldJump = false;
            Vector2 direction = (player.position - transform.position).normalized;

            Vector2 jumpDirection = direction * jumpForce;

            rb.AddForce(new Vector2(jumpDirection.x, jumpForce), ForceMode2D.Impulse);
        }
    }
}
