using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KaelMovement : MonoBehaviour
{
    public float speed = 5f; // Kecepatan gerakan karakter
    private Rigidbody2D rb; // Referensi ke komponen Rigidbody2D
    private bool isFacingRight = true; // Status apakah karakter menghadap kanan

    [SerializeField]
    public Animator kaelAnimationController; // Referensi ke Animator untuk mengatur animasi Kael

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // Mengambil komponen Rigidbody2D dari objek
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal"); // Mengambil input gerakan horizontal

        // Mengatur gerakan karakter hanya ke kiri dan kanan
        Vector2 movement = new Vector2(horizontalInput * speed, rb.linearVelocity.y);
        rb.linearVelocity = movement;

        // Menghandle animasi idle atau walk berdasarkan input gerakan horizontal
        if (Mathf.Abs(horizontalInput) > 0)
        {
            kaelAnimationController.SetInteger("state", 1); // Animasi berjalan
        }
        else
        {
            kaelAnimationController.SetInteger("state", 0); // Animasi idle
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

    // Fungsi untuk membalikkan arah karakter
    private void Flip()
    {
        isFacingRight = !isFacingRight; // Membalik status arah
        Vector3 scale = transform.localScale; // Mengambil skala objek
        scale.x *= -1; // Membalik skala di sumbu X
        transform.localScale = scale; // Menerapkan skala baru
    }
}
