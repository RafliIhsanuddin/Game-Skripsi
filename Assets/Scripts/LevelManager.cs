using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public GameObject pauseMenu;

    void Update()
    {
        // Check for Esc for pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
        }
    }

    //Back to Chase Scene
    public void BackToChase()
    {
        SceneManager.LoadScene("Desert Final Scene");
    }

    //Resumes Game
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    //Quit Game
    public void QuitGame()
    {
        Application.Quit();
    }
}
