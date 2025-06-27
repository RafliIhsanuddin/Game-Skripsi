using UnityEngine;

public class ParralaxRaf : MonoBehaviour
{


    public Camera cam;
    public Transform subject;

    Vector2 startPosition;
    float startZ;

    Vector2 Travel => (Vector2)cam.transform.position - startPosition;

    float DistanceFromSubject => transform.position.z - subject.position.z;

    float ClippingPlane => (cam.transform.position.z + (DistanceFromSubject > 0 ? cam.farClipPlane : cam.nearClipPlane));

    float ParallaxFactor => Mathf.Abs(DistanceFromSubject) / ClippingPlane;

    void Start()
    {
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    void Update()
    {
        Vector2 newPos = startPosition + Travel * ParallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }
}
