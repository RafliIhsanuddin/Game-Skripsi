using UnityEngine;

public class MultiColliderController : MonoBehaviour
{
    public Collider2D[] mainColliders; // Array Collider utama yang akan diaktifkan/dinonaktifkan
    public Collider2D triggerCollider; // Collider untuk mendeteksi keberadaan Player

    void Start()
    {
        // Pastikan semua collider utama aktif di awal
        foreach (var collider in mainColliders)
        {
            collider.enabled = true;
        }
        Debug.Log("Semua Main Collider aktif pada awal permainan.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D dipanggil.");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Trigger mendeteksi Player! Menonaktifkan semua main collider.");
            foreach (var collider in mainColliders)
            {
                collider.enabled = false;
            }
        }
        else
        {
            Debug.Log("Trigger mendeteksi objek lain: " + other.tag);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit2D dipanggil.");
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player keluar dari trigger! Mengaktifkan kembali semua main collider.");
            foreach (var collider in mainColliders)
            {
                collider.enabled = true;
            }
        }
        else
        {
            Debug.Log("Objek dengan tag " + other.tag + " keluar dari trigger.");
        }
    }
}
