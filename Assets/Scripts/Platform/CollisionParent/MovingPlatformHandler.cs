using UnityEngine;
using System.Collections; // Required for IEnumerator

public class MovingPlatformHandler : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private Transform path1; // Titik kiri (Path 1)
    [SerializeField] private Transform path2; // Titik kanan (Path 2)
    [SerializeField] private float speed = 2f; // Kecepatan gerak

    private Transform target; // Titik tujuan saat ini
    private Rigidbody2D platformRb; // Rigidbody2D platform

    void Start()
    {
        if (path1 == null || path2 == null)
        {
            Debug.LogError("Path1 dan Path2 harus diatur di Inspector.");
            return;
        }

        // Memulai dengan bergerak ke path1 terlebih dahulu
        target = path1;

        // Pastikan platform memiliki Rigidbody2D
        if (!TryGetComponent<Rigidbody2D>(out platformRb))
        {
            Debug.LogError("Platform harus memiliki komponen Rigidbody2D.");
        }
    }

    void FixedUpdate()
    {
        if (path1 == null || path2 == null) return;

        // Gerakkan platform ke target
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Periksa jika sudah sampai di target
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // Ganti target ke titik berikutnya
            target = target == path1 ? path2 : path1;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);

            // Jika platform memiliki Rigidbody2D, kirimkan kecepatan ke karakter
            if (platformRb != null)
            {
                var movementScript = collision.gameObject.GetComponent<Movement>();
                if (movementScript != null)
                {
                    movementScript.SetPlatformVelocity(platformRb.linearVelocity, true);
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);

            var movementScript = collision.gameObject.GetComponent<Movement>();
            if (movementScript != null)
            {
                movementScript.SetPlatformVelocity(Vector2.zero, false);
            }
        }
    }
}
