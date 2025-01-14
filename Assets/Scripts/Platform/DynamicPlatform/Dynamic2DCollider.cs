using UnityEngine;

public class Dynamic2DCollider : MonoBehaviour
{
    public Collider2D mainCollider; // Collider utama
    public Transform player; // Transform pemain

    void Update()
    {
        // Periksa posisi pemain relatif terhadap main collider
        Vector2 playerPosition = player.position;
        Bounds bounds = mainCollider.bounds;

        float colliderTop = bounds.max.y; // Bagian atas collider
        float colliderBottom = bounds.min.y; // Bagian bawah collider
        float colliderLeft = bounds.min.x; // Bagian kiri collider
        float colliderRight = bounds.max.x; // Bagian kanan collider

        if (playerPosition.y > colliderTop) // Pemain di atas collider
        {
            mainCollider.enabled = true; // Aktifkan collider
        }
        else if (playerPosition.y < colliderBottom || // Pemain di bawah collider
                 (playerPosition.x > colliderRight || playerPosition.x < colliderLeft)) // Pemain di samping collider
        {
            mainCollider.enabled = false; // Nonaktifkan collider
        }
    }
}
