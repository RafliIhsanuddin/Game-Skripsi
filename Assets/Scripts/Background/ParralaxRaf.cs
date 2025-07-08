using UnityEngine;

public class ParralaxRaf : MonoBehaviour
{


    [Header("References")]
    [Tooltip("Main camera that will control the parallax effect.")]
    [SerializeField] private Camera cam;

    [Tooltip("The object that represents the subject (e.g., player).")]
    [SerializeField] private Transform subject;

    private Vector2 startPosition;
    private float startZ;

    private Vector2 Travel => (Vector2)cam.transform.position - startPosition;

    private float DistanceFromSubject => transform.position.z - subject.position.z;

    private float ClippingPlane =>
        cam.transform.position.z +
        (DistanceFromSubject > 0f ? cam.farClipPlane : cam.nearClipPlane);

    private float ParallaxFactor =>
        Mathf.Abs(DistanceFromSubject) / ClippingPlane;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
    }

    private void Start()
    {
        startPosition = transform.position;
        startZ = transform.position.z;
    }

    private void LateUpdate()
    {
        Vector2 newPos = startPosition + Travel * ParallaxFactor;
        transform.position = new Vector3(newPos.x, newPos.y, startZ);
    }
}
