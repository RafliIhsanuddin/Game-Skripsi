using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneControlManager : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            QuitGame();
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ReloadCurrentScene();
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            LoadDesertScene();
        }
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // berhenti saat di editor
#else
        Application.Quit(); // keluar saat di build
#endif
    }

    void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }

    void LoadDesertScene()
    {
        SceneManager.LoadScene("Desert fbf");
    }
}
