using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ScriptFinderDebugger : MonoBehaviour
{
    [Header("Masukkan nama skrip yang ingin dicari (case sensitive)")]
    [SerializeField] private string scriptNameToFind = "ParallaxWithBounds";

    private bool hasSearched = false;

    private void Update()
    {
        if (!hasSearched)
        {
            FindObjectsWithScript();
            hasSearched = true;
        }
    }

    private void FindObjectsWithScript()
    {
        MonoBehaviour[] allMonoBehaviours = FindObjectsOfType<MonoBehaviour>(true);
        List<GameObject> foundObjects = new List<GameObject>();

        foreach (var mb in allMonoBehaviours)
        {
            if (mb.GetType().Name == scriptNameToFind)
            {
                foundObjects.Add(mb.gameObject);
            }
        }

        if (foundObjects.Count == 0)
        {
            Debug.LogWarning($"Tidak ditemukan GameObject dengan skrip '{scriptNameToFind}'.");
        }
        else
        {
            Debug.Log($"Ditemukan {foundObjects.Count} GameObject dengan skrip '{scriptNameToFind}':");
            foreach (var obj in foundObjects)
            {
                Debug.Log($"- {obj.name}", obj); // klik log = ping GameObject
            }
        }
    }
}
