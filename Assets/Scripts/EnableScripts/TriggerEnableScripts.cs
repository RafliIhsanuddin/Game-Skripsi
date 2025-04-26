using UnityEngine;

public class TriggerEnableScripts : MonoBehaviour
{
    [Header("Drag Target GameObject")]
    [SerializeField] private GameObject targetObject; // GameObject yang ingin dikontrol
    [SerializeField] private bool enable = false; // Boolean untuk mengatur aktif/tidaknya semua script

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ketika trigger tersentuh, ubah semua skrip di targetObject
        SetScriptsActive(enable);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Jika Anda ingin reset atau melakukan sesuatu ketika keluar dari trigger
        // Anda bisa menambahkan logika di sini (opsional).
    }

    private void SetScriptsActive(bool isActive)
    {
        if (targetObject != null)
        {
            // Dapatkan semua script di GameObject target
            MonoBehaviour[] scripts = targetObject.GetComponents<MonoBehaviour>();

            foreach (var script in scripts)
            {
                script.enabled = isActive;
            }
        }
        else
        {
            Debug.LogWarning("Target GameObject belum diatur!");
        }
    }
}
