using UnityEngine;

public class MoveLeftRight : MonoBehaviour
{
    [SerializeField] private Transform path1; // Titik kiri (Path 1)
    [SerializeField] private Transform path2; // Titik kanan (Path 2)
    [SerializeField] private float speed = 2f; // Kecepatan gerak

    private Transform target; // Titik tujuan saat ini

    void Start()
    {
        if (path1 == null || path2 == null)
        {
            Debug.LogError("Path1 dan Path2 harus diatur di Inspector.");
            return;
        }

        // Memulai dengan bergerak ke path1 terlebih dahulu
        target = path1;
    }

    void Update()
    {
        if (path1 == null || path2 == null) return;

        // Gerakkan objek ke target
        transform.position = Vector3.MoveTowards(transform.position, target.position, speed * Time.deltaTime);

        // Periksa jika sudah sampai di target
        if (Vector3.Distance(transform.position, target.position) < 0.1f)
        {
            // Ganti target ke titik berikutnya
            target = target == path1 ? path2 : path1;
        }
    }
}
