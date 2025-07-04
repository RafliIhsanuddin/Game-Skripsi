using UnityEngine;
using Yarn.Unity;

public class Snowy : MonoBehaviour
{
    [SerializeField] private DialogueRunner dialogueRunner;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            dialogueRunner.StartDialogue("snowy");
        }
    }
}
