using UnityEngine;
using UnityEngine.SceneManagement;

public class VisibilityChecker : MonoBehaviour
{
    public GameObject target; // Object to track
    public string sceneToLoad = "Chase Fail"; // Replace with your scene name
    public float outOfViewTime = 5f;

    private float timer = 0f;
    private Camera mainCamera;

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned!");
            enabled = false;
            return;
        }

        mainCamera = Camera.main;

        if (mainCamera == null)
        {
            Debug.LogError("No Main Camera found!");
            enabled = false;
        }
    }

    void Update()
    {
        if (IsVisibleToCamera())
        {
            timer = 0f;
        }
        else
        {
            timer += Time.deltaTime;

            if (timer >= outOfViewTime)
            {
                Debug.Log("Target out of view for " + outOfViewTime + " seconds. Loading scene...");
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    bool IsVisibleToCamera()
    {
        Vector3 viewportPos = mainCamera.WorldToViewportPoint(target.transform.position);
        bool isInside = viewportPos.z > 0 && viewportPos.x > 0 && viewportPos.x < 1 && viewportPos.y > 0 && viewportPos.y < 1;
        return isInside;
    }
}
