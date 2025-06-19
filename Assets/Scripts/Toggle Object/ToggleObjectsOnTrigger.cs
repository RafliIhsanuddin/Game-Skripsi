using System.Collections.Generic;
using UnityEngine;

public class ToggleObjectsOnTrigger : MonoBehaviour
{
    [Header("Daftar GameObject yang ingin diubah")]
    [SerializeField] private List<GameObject> objectsToToggle = new List<GameObject>();

    [Header("Opsi Aksi")]
    [SerializeField] private bool enableOnTrigger = true; // Jika true, akan enable objek. Jika false, akan disable.

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Kael FBF")
        {
            foreach (GameObject obj in objectsToToggle)
            {
                if (obj != null)
                    obj.SetActive(enableOnTrigger);
            }
        }
    }
}
