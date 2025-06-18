using UnityEngine;
using Yarn.Unity;

public class DesertDialogueTrigger : MonoBehaviour
{
    public DialogueRunner dialogueRunner;

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            dialogueRunner.StartDialogue(gameObject.name);
        }
    }

}
