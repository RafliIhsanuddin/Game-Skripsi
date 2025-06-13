using UnityEngine;
using Yarn.Unity;

public class PumpkinDialogue : MonoBehaviour
{
    public LayerMask kael;
    public DialogueRunner dialogueRunner;
    public GameObject bubble;
    public PuffkinAnimator puffkinAnim;
 
    void Update()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.left, 10f, kael);
        Debug.DrawRay(transform.position, Vector2.left * 10f, Color.cyan);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                //Debug.Log($"[Puffkin] {gameObject.name} starting node: {yarnNode}");
                dialogueRunner.StartDialogue("pumpkin");
                bubble.SetActive(false);
            }
        }
    }
}
