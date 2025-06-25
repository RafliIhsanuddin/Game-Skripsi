using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneSceneLoader : MonoBehaviour
{
    public void OnEnable()
    {
        SceneManager.LoadScene("Final Forest");
    }
}
