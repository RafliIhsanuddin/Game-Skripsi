using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    void Update()
    {
        // Check for L every frame
        if (Input.GetKeyDown(KeyCode.L))
        {
            Debug.Log("l pressed");
            SceneManager.LoadScene("Chase Fail");
        }
    }

    //Back to Chase Scene
    public void BackToChase()
    {
        SceneManager.LoadScene("Desert Final Scene");
    }

    //Quit Game
    public void QuitGame()
    {
        Application.Quit();
    }
}
