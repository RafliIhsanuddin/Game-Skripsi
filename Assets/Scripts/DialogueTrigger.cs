using UnityEngine;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    public LayerMask kael;
    public DialogueRunner dialogueRunner;
    public GameObject bubble;
    public PuffkinAnimator puffkinAnim;
    [SerializeField] private string yarnNode;

    [Header("Unique ID for this Puffkin")]
    [SerializeField] private string puffkinID = "corn";  // <- Set this in Inspector

    private string animCommandName => $"set_{puffkinID}_anim";

    void Update()
    {
        if (Physics2D.Raycast(transform.position, Vector2.left, 10f, kael))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                EnableOnlyThisPuffkin();

                dialogueRunner.StartDialogue(yarnNode);
                bubble.SetActive(false);
            }
        }
    }

    void EnableOnlyThisPuffkin()
    {
        foreach (var anim in FindObjectsOfType<PuffkinAnimator>())
            anim.enabled = false;

        if (puffkinAnim != null)
            puffkinAnim.enabled = true;
    }

    private void OnEnable()
    {
        if (dialogueRunner != null && puffkinAnim != null)
        {
            dialogueRunner.AddCommandHandler<string>(animCommandName, puffkinAnim.SetAnimation);
        }
    }

    /*private void OnDisable()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.RemoveCommandHandler(animCommandName);
        }
    }*/
}
