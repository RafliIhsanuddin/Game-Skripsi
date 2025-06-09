using UnityEngine;

public class DynamicPlatformEffector : MonoBehaviour
{
    public PlatformEffector2D platformEffector; // Referensi ke Platform Effector
    public Collider2D triggerCollider; // Trigger untuk deteksi posisi pemain
    public Transform player; // Transform pemain untuk deteksi posisi

    void Update()
    {
        // Periksa posisi pemain relatif terhadap batu
        if (player.position.y > transform.position.y) // Pemain di atas batu
        {
            platformEffector.surfaceArc = 180; // Collider aktif untuk bagian atas
        }
        else if (player.position.y <= transform.position.y) // Pemain di bawah batu
        {
            platformEffector.surfaceArc = 0; // Collider nonaktif dari bawah
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (player.position.x < transform.position.x - 0.5f || player.position.x > transform.position.x + 0.5f) // Jika pemain di samping
            {
                platformEffector.surfaceArc = 0; // Collider nonaktif
            }
        }
    }
}
