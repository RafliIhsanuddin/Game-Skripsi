using UnityEngine;

public class ParallaxWithBounds : MonoBehaviour
{
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float parallaxMultiplier = 0.5f;

    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    private Vector3 lastCameraPosition;
    private float startX;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;
        startX = transform.position.x;
    }

    private void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Hitung posisi calon gerakan parallax
        float targetX = transform.position.x + deltaMovement.x * parallaxMultiplier;

        // Batas kiri dan kanan dalam world space
        float leftBound = leftLimit.position.x;
        float rightBound = rightLimit.position.x;

        // Clamp posisi agar tidak melewati batas
        targetX = Mathf.Clamp(targetX, leftBound, rightBound);

        // Terapkan posisi baru
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);

        lastCameraPosition = cameraTransform.position;
    }
}
