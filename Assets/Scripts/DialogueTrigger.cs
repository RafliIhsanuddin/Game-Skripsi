using UnityEngine;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    public LayerMask kael;
    public DialogueRunner dialogueRunner;
    public GameObject bubble;
    public PuffkinAnimator puffkinAnim;
    [SerializeField] private string yarnNode;

    // Update is called once per frame
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
            dialogueRunner.AddCommandHandler<string>("set_anim", puffkinAnim.SetAnimation);
        }
    }

    private void OnDisable()
    {
        if (dialogueRunner != null)
        {
            dialogueRunner.RemoveCommandHandler("set_anim");
        }
    }
}
