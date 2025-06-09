using UnityEngine;

public class Platform : MonoBehaviour
{
    public int platformID;

    public Vector2 GetClosestEdge(Vector2 fromPosition, Vector2 targetPosition)
    {
        Bounds bounds = GetComponent<Collider2D>().bounds;
        float edgeX = (targetPosition.x > bounds.center.x) ? bounds.max.x : bounds.min.x;
        return new Vector2(edgeX, bounds.max.y);
    }
}
