using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChangerOnCollision : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "Akhir";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Kael FBF")
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
