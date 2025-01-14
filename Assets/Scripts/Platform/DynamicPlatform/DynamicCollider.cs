using UnityEngine;

public class DynamicCollider : MonoBehaviour
{
    public Collider2D mainCollider; // Collider utama
    public Collider2D triggerCollider; // Trigger untuk deteksi posisi
    public Transform player; // Referensi ke pemain

    void Update()
    {
        // Periksa posisi pemain relatif terhadap batu
        if (player.position.y > transform.position.y + 0.5f) // Jika pemain di atas batu
        {
            mainCollider.enabled = true; // Aktifkan collider
        }
        else // Jika pemain berada di samping atau di bawah batu
        {
            mainCollider.enabled = false; // Nonaktifkan collider
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player")) // Jika pemain berada di dalam trigger
        {
            if (player.position.y > transform.position.y + 0.5f)
            {
                mainCollider.enabled = true; // Aktifkan collider
            }
            else
            {
                mainCollider.enabled = false; // Nonaktifkan collider
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            mainCollider.enabled = false; // Pastikan collider nonaktif saat pemain keluar
        }
    }
}
