using Cinemachine;
using UnityEngine;

public class NoPostCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera; // Virtual Camera utama

    private CinemachineBrain noPostCameraBrain;
    private Camera noPostCamera;
    private float initialOrthoSize;

    private void Start()
    {
        if (virtualCamera == null)
        {
            Debug.LogError("Virtual Camera tidak diatur!");
            return;
        }

        // Ambil komponen Camera dari game object ini
        noPostCamera = GetComponent<Camera>();
        if (noPostCamera == null)
        {
            Debug.LogError("NoPostCamera membutuhkan komponen Camera!");
            return;
        }

        // Simpan ukuran ortografis awal kamera
        initialOrthoSize = virtualCamera.m_Lens.OrthographicSize;

        // Tambahkan CinemachineBrain jika belum ada
        noPostCameraBrain = GetComponent<CinemachineBrain>();
        if (noPostCameraBrain == null)
        {
            noPostCameraBrain = gameObject.AddComponent<CinemachineBrain>();
        }

        noPostCameraBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.FixedUpdate;
        noPostCamera.orthographicSize = initialOrthoSize;
    }

    private void FixedUpdate()
    {
        if (virtualCamera != null && noPostCamera != null)
        {
            // Sinkronisasi posisi kamera dengan virtualCamera
            transform.position = virtualCamera.transform.position;
            noPostCamera.orthographicSize = virtualCamera.m_Lens.OrthographicSize;
        }
    }
}
