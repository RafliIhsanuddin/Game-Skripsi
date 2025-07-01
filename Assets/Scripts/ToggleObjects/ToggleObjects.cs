using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

public class ToggleObjects : MonoBehaviour
{
    [SerializeField] private List<GameObject> targetObjects = new List<GameObject>();

    [SerializeField] private KeyCode toggleKey = KeyCode.E;

    [SerializeField] private bool toggleToEnable = true; // Jika true, tombol akan mengaktifkan objek. Jika false, tombol akan menonaktifkan objek.

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if (toggleToEnable)
            {
                EnableAllObjects();
            }
            else
            {
                DisableAllObjects();
            }
        }
    }

    [YarnCommand("toggle")]
    public void ToggleAllObjects()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(!obj.activeSelf);
            }
        }
    }

    public void EnableAllObjects()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
    }

    public void DisableAllObjects()
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null)
            {
                obj.SetActive(false);
            }
        }
    }
}
