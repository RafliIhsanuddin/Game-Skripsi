using System.Collections.Generic;
using UnityEngine;

public class ToggleObjectsSimple : MonoBehaviour
{
    [Header("GameObject yang akan di-enable saat trigger")]
    [SerializeField] private List<GameObject> objectsToEnable = new List<GameObject>();

    [Header("GameObject yang akan di-disable saat trigger")]
    [SerializeField] private List<GameObject> objectsToDisable = new List<GameObject>();

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name != "Kael FBF") return;

        // Enable objek
        if (objectsToEnable != null && objectsToEnable.Count > 0)
        {
            foreach (GameObject obj in objectsToEnable)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }

        // Disable objek
        if (objectsToDisable != null && objectsToDisable.Count > 0)
        {
            foreach (GameObject obj in objectsToDisable)
            {
                if (obj != null)
                    obj.SetActive(false);
            }
        }
    }
}
