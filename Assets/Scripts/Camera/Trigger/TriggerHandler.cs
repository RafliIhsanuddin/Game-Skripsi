using UnityEngine;
using System; // Include this for Action

public class TriggerHandler : MonoBehaviour
{
    public event Action OnPlayerEnter; // Event ketika pemain masuk
    public event Action OnPlayerExit; // Event ketika pemain keluar

    [SerializeField] private Collider2D playerCollider; // Collider pemain

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == playerCollider)
        {
            OnPlayerEnter?.Invoke(); // Panggil event ketika pemain masuk
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == playerCollider)
        {
            OnPlayerExit?.Invoke(); // Panggil event ketika pemain keluar
        }
    }
}
