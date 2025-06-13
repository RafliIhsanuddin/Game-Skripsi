using UnityEngine;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    public LayerMask kaelLayer;
    public DialogueRunner dialogueRunner;
    public GameObject bubble;
    [SerializeField] public string yarnNode;

    void Update()
    {
        Debug.Log("is dialogue running?" + dialogueRunner.IsDialogueRunning);

        // Raycast from Puffkin toward Kael
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, 7f, kaelLayer);
        //Debug.DrawRay(transform.position, Vector2.left * 7f, Color.cyan);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                //Debug.Log($"[Puffkin] {gameObject.name} starting node: {yarnNode}");
                dialogueRunner.StartDialogue(yarnNode);
                bubble.SetActive(false);
            }
        }

    }
}

