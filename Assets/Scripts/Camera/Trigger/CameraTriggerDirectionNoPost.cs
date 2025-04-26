using Cinemachine;
using UnityEngine;
using System; // Include this for Action

public class CameraTriggerDirectionNoPost : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private Camera noPostCamera; // Referensi ke No Post Camera
    [SerializeField] private TriggerHandler leftTrigger; // Left Trigger untuk Zoom In
    [SerializeField] private TriggerHandler rightTrigger; // Right Trigger untuk Zoom Out

    // Pengaturan zoom untuk gerakan ke kiri (Left Trigger)
    [SerializeField] private float targetOrthoSizeLeftTrigger = 18f;
    [SerializeField] private Vector3 targetTrackedObjectOffsetLeftTrigger = new Vector3(0, 12, 0);
    [SerializeField] private float transitionDurationLeftTrigger = 1f;

    // Pengaturan zoom untuk gerakan ke kanan (Right Trigger)
    [SerializeField] private float targetOrthoSizeRightTrigger = 14f;
    [SerializeField] private Vector3 targetTrackedObjectOffsetRightTrigger = new Vector3(0, 16, 0);
    [SerializeField] private float transitionDurationRightTrigger = 1f;

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

        initialOrthoSize = virtualCamera.m_Lens.OrthographicSize;
        var framingTransposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        if (framingTransposer != null)
        {
            initialTrackedObjectOffset = framingTransposer.m_TrackedObjectOffset;
        }

        if (leftTrigger != null)
        {
            leftTrigger.OnPlayerEnter += () =>
            {
                TriggerTransition(targetOrthoSizeLeftTrigger, targetTrackedObjectOffsetLeftTrigger, transitionDurationLeftTrigger);
                Debug.Log("Player triggered Left Trigger for Zoom In.");
            };
        }

        if (rightTrigger != null)
        {
            rightTrigger.OnPlayerEnter += () =>
            {
                TriggerTransition(targetOrthoSizeRightTrigger, targetTrackedObjectOffsetRightTrigger, transitionDurationRightTrigger);
                Debug.Log("Player triggered Right Trigger for Zoom Out.");
            };
        }

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
            // Update waktu transisi
            transitionTime += Time.fixedDeltaTime / currentTransitionDuration;
            transitionTime = Mathf.Clamp01(transitionTime);

            // Lerp untuk OrthographicSize
            float newOrthoSize = Mathf.Lerp(initialOrthoSize, targetOrthoSize, transitionTime);
            virtualCamera.m_Lens.OrthographicSize = newOrthoSize;

            if (noPostCamera != null)
            {
                noPostCamera.orthographicSize = newOrthoSize;
            }

            // Lerp untuk TrackedObjectOffset
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

            // Akhiri transisi jika selesai
            if (transitionTime >= 1f)
            {
                isTransitioning = false;
                Debug.Log("Transition completed.");
            }
        }
    }

    private void TriggerTransition(float newTargetOrthoSize, Vector3 newTargetTrackedObjectOffset, float newTransitionDuration)
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
