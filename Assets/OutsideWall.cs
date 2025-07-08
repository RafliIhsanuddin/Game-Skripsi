using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class OutsideWall : MonoBehaviour
{
    public DialogueRunner dialogue;
    public GameObject cutsceneWall;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            if (gameObject.name == "BorderWall")
            {
                dialogue.StartDialogue("outside");
                cutsceneWall.SetActive(true);
            }

            if (gameObject.name == "CutsceneWall")
            {
                SceneManager.LoadScene("House Cutscene");
            }
        }
    }
}
