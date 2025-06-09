using Cinemachine;
using UnityEngine;

public class NoPostCameraFollow : MonoBehaviour
{
    [SerializeField] private Camera noPostCamera; // Referensi ke No Post Camera
    private CinemachineVirtualCamera virtualCamera;

    private void Awake()
    {
        // Ambil referensi Virtual Camera dari game object ini
        virtualCamera = GetComponent<CinemachineVirtualCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("Cinemachine Virtual Camera is not attached to this GameObject!");
        }

        if (noPostCamera == null)
        {
            Debug.LogError("No Post Camera is not assigned!");
        }
    }

    private void Update()
    {
        // Sinkronkan ukuran orthographic jika kedua kamera ada
        if (virtualCamera != null && noPostCamera != null)
        {
            noPostCamera.orthographicSize = virtualCamera.m_Lens.OrthographicSize;
        }
    }
}
