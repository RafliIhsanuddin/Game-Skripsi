using UnityEngine;

public class MorphController : MonoBehaviour
{
    public Transform player; // Transform dari player
    public Transform morph;  // Transform dari objek morph
    public Animator animator; // Animator yang memiliki Blend Trees
    [SerializeField] public float maxDistance = 10f; // Jarak maksimum untuk pengaruh blend (dari 0 ke 1)
    [SerializeField] public float blendDistance = 5f; // Jarak di mana Blend menjadi 1

    private float blendValue = 0f; // Nilai blend

    void Update()
    {
        // Hitung jarak antara player dan morph
        float distance = Vector3.Distance(player.position, morph.position);

        // Normalisasi jarak ke rentang 0 - 1
        if (distance <= blendDistance)
        {
            blendValue = 1f; // Jika jarak <= blendDistance, Blend langsung 1
        }
        else
        {
            // Jika jarak > blendDistance, hitung blendValue berdasarkan normalisasi
            blendValue = Mathf.Clamp01(1 - ((distance - blendDistance) / (maxDistance - blendDistance)));
        }

        // Set parameter blend pada Animator
        animator.SetFloat("Blend", blendValue);
    }

    void OnDrawGizmos()
    {
        // Tambahkan visualisasi untuk jarak maksimum di Scene
        if (morph != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(morph.position, maxDistance); // Jarak maksimum
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(morph.position, blendDistance); // Jarak untuk Blend = 1
        }
    }
}
