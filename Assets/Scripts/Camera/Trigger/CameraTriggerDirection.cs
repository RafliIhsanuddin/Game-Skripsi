using Cinemachine;
using UnityEngine;

public class CameraTriggerDirection : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private Collider2D playerCollider; // Collider milik pemain

    // Pengaturan zoom untuk gerakan ke kanan
    [SerializeField] private float targetOrthoSizeRight = 18f;
    [SerializeField] private Vector3 targetTrackedObjectOffsetRight = new Vector3(0, 12, 0);
    [SerializeField] private float transitionDurationRight = 1f;
    [SerializeField] private float targetOrthoSizeLeft = 14f;
    [SerializeField] private Vector3 targetTrackedObjectOffsetLeft = new Vector3(0, 16, 0);
    [SerializeField] private float transitionDurationLeft = 1f;

    [SerializeField] private float horizontalSpeedThreshold = 0.1f; // Ambang batas kecepatan horizontal

    private Rigidbody2D playerRigidbody;
    private float initialOrthoSize;
    private Vector3 initialTrackedObjectOffset;
    private float transitionTime;
    private float currentTransitionDuration;
    private float targetOrthoSize;
    private Vector3 targetTrackedObjectOffset;
    private bool isTransitioning;

    private void Start()
    {
        // Pastikan Cinemachine Virtual Camera dan Rigidbody2D pemain valid
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is not assigned!");
            return;
        }

        if (playerCollider != null)
        {
            playerRigidbody = playerCollider.GetComponent<Rigidbody2D>();
            if (playerRigidbody == null)
            {
                Debug.LogError("Rigidbody2D not found on the playerCollider!");
            }
        }

        // Ambil nilai awal kamera
        initialOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            initialTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
        }
    }

    private void Update()
    {
        if (isTransitioning)
        {
            // Lanjutkan transisi kamera
            transitionTime += Time.deltaTime / currentTransitionDuration;
            transitionTime = Mathf.Clamp01(transitionTime);

            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(initialOrthoSize, targetOrthoSize, transitionTime);

            var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer != null)
            {
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(initialTrackedObjectOffset, targetTrackedObjectOffset, transitionTime);
            }

            if (transitionTime >= 1f)
            {
                isTransitioning = false;
                Debug.Log("Transition completed.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pastikan trigger hanya aktif jika collider pemain masuk
        if (other == playerCollider && triggerCollider != null)
        {
            if (virtualCamera != null && playerRigidbody != null)
            {
                float horizontalSpeed = playerRigidbody.linearVelocity.x; // Ambil kecepatan horizontal dari Rigidbody2D

                // Tentukan arah gerakan dan mulai transisi jika ada perubahan
                if (horizontalSpeed > horizontalSpeedThreshold)
                {
                    StartTransition(targetOrthoSizeRight, targetTrackedObjectOffsetRight, transitionDurationRight);
                    Debug.Log("Player bergerak ke kanan. Mulai transisi ke kanan.");
                }
                else if (horizontalSpeed < -horizontalSpeedThreshold)
                {
                    StartTransition(targetOrthoSizeLeft, targetTrackedObjectOffsetLeft, transitionDurationLeft);
                    Debug.Log("Player bergerak ke kiri. Mulai transisi ke kiri.");
                }
            }
        }
    }

    private void StartTransition(float newTargetOrthoSize, Vector3 newTargetTrackedObjectOffset, float newTransitionDuration)
    {
        // Set nilai untuk transisi baru
        targetOrthoSize = newTargetOrthoSize;
        targetTrackedObjectOffset = newTargetTrackedObjectOffset;
        currentTransitionDuration = newTransitionDuration;

        // Set nilai awal untuk transisi
        initialOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            initialTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
        }

        transitionTime = 0f;
        isTransitioning = true;
    }
}
