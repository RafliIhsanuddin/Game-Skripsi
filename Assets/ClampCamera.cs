using UnityEngine;

public class ClampCamera : MonoBehaviour
{
    [Header("Target to Follow")]
    public Transform target;  // The player or object to follow

    [Header("X Axis Limits")]
    public float minX = -10f;
    public float maxX = 10f;

    [Header("Smooth Follow")]
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 newPos = transform.position;

        // Smooth follow target on X axis
        newPos.x = Mathf.Lerp(transform.position.x, target.position.x, smoothSpeed * Time.deltaTime);

        // Clamp X
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);

        transform.position = newPos;
    }
}
