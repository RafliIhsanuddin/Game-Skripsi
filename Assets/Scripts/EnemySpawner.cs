using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;

    // Update is called once per frame
    void Update()
    {
        if (enemy != null)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Instantiate(enemy);
            }
        }
    }
}
