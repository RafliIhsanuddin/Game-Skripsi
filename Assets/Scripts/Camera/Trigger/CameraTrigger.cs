using Cinemachine;
using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Collider2D triggerCollider; // Collider yang menjadi trigger
    [SerializeField] private Collider2D playerCollider;  // Collider milik pemain

    [SerializeField] private float targetOrthoSize = 16f;
    [SerializeField] private Vector3 targetTrackedObjectOffset = new Vector3(0, 14, 0);
    [SerializeField] private float transitionDuration = 1f; // Durasi transisi dalam detik

    private bool isTransitioning = false;
    private float initialOrthoSize;
    private Vector3 initialTrackedObjectOffset;
    private float transitionTime;

    private void Start()
    {
        if (virtualCamera != null)
        {
            initialOrthoSize = virtualCamera.m_Lens.OrthographicSize;
            var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer != null)
            {
                initialTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
            }
        }
    }

    private void Update()
    {
        if (isTransitioning && virtualCamera != null)
        {
            transitionTime += Time.deltaTime / transitionDuration;
            transitionTime = Mathf.Clamp01(transitionTime);

            // Lerp untuk ukuran ortografik
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(initialOrthoSize, targetOrthoSize, transitionTime);

            // Lerp untuk tracked object offset
            var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer != null)
            {
                framingTransposer.m_TrackedObjectOffset = Vector3.Lerp(initialTrackedObjectOffset, targetTrackedObjectOffset, transitionTime);
            }

            // Hentikan transisi jika selesai
            if (transitionTime >= 1f)
            {
                isTransitioning = false;
                Debug.Log("Transition completed.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Pastikan collider yang menabrak adalah collider pemain dan triggerCollider adalah collider dari objek ini
        if (other == playerCollider && triggerCollider != null)
        {
            if (virtualCamera != null)
            {
                isTransitioning = true;
                transitionTime = 0f;

                Debug.Log($"Starting transition to Orthographic Size: {targetOrthoSize} and Tracked Object Offset: {targetTrackedObjectOffset}");
            }
            else
            {
                Debug.LogWarning("Virtual Camera is not assigned in the inspector!");
            }
        }
    }
}
