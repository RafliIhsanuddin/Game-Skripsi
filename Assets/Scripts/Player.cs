using UnityEngine;

public class Player : MonoBehaviour
{
    // THESE ARE MUST NEED VARIABLES INSIDE YOUR PLAYER SCRIPT!!
    public float startingHealth;
    [HideInInspector]
    public float Health;

    void Update()
    {
        Vector3 pos = Input.mousePosition;
        pos = Camera.main.ScreenToWorldPoint(pos);
        pos.z = 0;
        transform.position = pos;
    }
}
