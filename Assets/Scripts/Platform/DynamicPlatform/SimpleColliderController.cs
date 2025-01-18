using UnityEngine;

public class SimpleColliderController : MonoBehaviour
{
    public Collider2D mainCollider; // Collider utama yang akan diaktifkan/dinonaktifkan
    public Collider2D triggerCollider; // Collider untuk mendeteksi keberadaan Player

    void Start()
    {
        // Pastikan collider utama aktif di awal
        mainCollider.enabled = true;
        //Debug.Log("Main Collider aktif pada awal permainan.");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("OnTriggerEnter2D dipanggil.");
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Trigger mendeteksi Player! Menonaktifkan main collider.");
            mainCollider.enabled = false;
        }
        else
        {
            //Debug.Log("Trigger mendeteksi objek lain: " + other.tag);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("OnTriggerExit2D dipanggil.");
        if (other.CompareTag("Player"))
        {
            //Debug.Log("Player keluar dari trigger! Mengaktifkan kembali main collider.");
            mainCollider.enabled = true;
        }
        else
        {
            //Debug.Log("Objek dengan tag " + other.tag + " keluar dari trigger.");
        }
    }
}
