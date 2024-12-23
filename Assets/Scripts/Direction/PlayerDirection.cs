using UnityEngine;

public class PlayerDirection : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rb;
    public float horizontalSpeed; // Variabel publik untuk menyimpan kecepatan horizontal


    void Start()
    {
        // Mendapatkan komponen Rigidbody2D dari player
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Rigidbody2D tidak ditemukan pada GameObject!");
        }
    }

    void Update()
    {
        UpdateHorizontalSpeed();
        DetectDirection();
    }
    private void UpdateHorizontalSpeed()
    {
        if (rb != null)
        {
            horizontalSpeed = rb.linearVelocity.x; // Memperbarui nilai kecepatan horizontal
        }
    }

    private void DetectDirection()
    {
        if (rb != null)
        {
            // Mengecek apakah nilai linearVelocity pada sumbu X bernilai positif atau negatif
            if (rb.linearVelocity.x > 0)
            {
                Debug.Log("Player bergerak ke kanan.");
                Debug.Log(rb.linearVelocity.x);
            }
            else if (rb.linearVelocity.x < 0)
            {
                Debug.Log("Player bergerak ke kiri.");
                Debug.Log(rb.linearVelocity.x);
            }
            else
            {
                Debug.Log("Player diam atau tidak bergerak secara horizontal.");
            }
        }
    }
}
