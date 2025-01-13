using Cinemachine;
using UnityEngine;
using System; // Include this for Action

public class CameraTriggerDirectionNoPost : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Camera noPostCamera; // Referensi ke No Post Camera
    [SerializeField] private TriggerHandler zoomInTrigger;
    [SerializeField] private TriggerHandler zoomOutTrigger;

    // Pengaturan zoom untuk gerakan ke kanan
    [SerializeField] private float targetOrthoSizeZoomIn = 18f;
    [SerializeField] private Vector3 targetTrackedObjectOffsetZoomIn = new Vector3(0, 12, 0);
    [SerializeField] private float transitionDurationZoomIn = 1f;

    // Pengaturan zoom out
    [SerializeField] private float targetOrthoSizeZoomOut = 14f;
    [SerializeField] private Vector3 targetTrackedObjectOffsetZoomOut = new Vector3(0, 16, 0);
    [SerializeField] private float transitionDurationZoomOut = 1f;

    private Rigidbody2D playerRigidbody;
    private float initialOrthoSize;
    private Vector3 initialTrackedObjectOffset;
    private float transitionTime;
    private float currentTransitionDuration;
    private float targetOrthoSize;
    private Vector3 targetTrackedObjectOffset;
    private bool isTransitioning;

    private CinemachineBrain noPostCameraBrain;

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera is not assigned!");
            return;
        }

        // Ambil nilai awal kamera
        initialOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            initialTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
        }

        // Daftarkan event dari trigger handler
        if (zoomInTrigger != null)
        {
            zoomInTrigger.OnPlayerEnter += () =>
            {
                StartTransition(targetOrthoSizeZoomIn, targetTrackedObjectOffsetZoomIn, transitionDurationZoomIn);
                Debug.Log("Player entered Zoom In trigger.");
            };
        }

        if (zoomOutTrigger != null)
        {
            zoomOutTrigger.OnPlayerEnter += () =>
            {
                StartTransition(targetOrthoSizeZoomOut, targetTrackedObjectOffsetZoomOut, transitionDurationZoomOut);
                Debug.Log("Player entered Zoom Out trigger.");
            };
        }

        // Tambahkan CinemachineBrain ke No Post Camera jika belum ada
        if (noPostCamera != null)
        {
            noPostCameraBrain = noPostCamera.GetComponent<CinemachineBrain>();
            if (noPostCameraBrain == null)
            {
                noPostCameraBrain = noPostCamera.gameObject.AddComponent<CinemachineBrain>();
            }
            noPostCameraBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
            noPostCamera.orthographicSize = initialOrthoSize;
        }
    }

    private void FixedUpdate()
    {
        if (isTransitioning)
        {
            transitionTime += Time.fixedDeltaTime / currentTransitionDuration;
            transitionTime = Mathf.Clamp01(transitionTime);

            float newOrthoSize = Mathf.Lerp(initialOrthoSize, targetOrthoSize, transitionTime);
            virtualCamera.m_Lens.OrthographicSize = newOrthoSize;

            if (noPostCamera != null)
            {
                noPostCamera.orthographicSize = newOrthoSize;
            }

            var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
            if (framingTransposer != null)
            {
                Vector3 newTrackedObjectOffset = Vector3.Lerp(initialTrackedObjectOffset, targetTrackedObjectOffset, transitionTime);
                framingTransposer.m_TrackedObjectOffset = newTrackedObjectOffset;

                if (noPostCamera != null)
                {
                    noPostCamera.transform.position = virtualCamera.transform.position;
                }
            }

            if (transitionTime >= 1f)
            {
                isTransitioning = false;
                Debug.Log("Transition completed.");
            }
        }
    }

    private void StartTransition(float newTargetOrthoSize, Vector3 newTargetTrackedObjectOffset, float newTransitionDuration)
    {
        targetOrthoSize = newTargetOrthoSize;
        targetTrackedObjectOffset = newTargetTrackedObjectOffset;
        currentTransitionDuration = newTransitionDuration;

        initialOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            initialTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
        }

        if (noPostCamera != null)
        {
            noPostCamera.orthographicSize = initialOrthoSize;
            noPostCamera.transform.position = virtualCamera.transform.position;
        }

        transitionTime = 0f;
        isTransitioning = true;
    }
}
