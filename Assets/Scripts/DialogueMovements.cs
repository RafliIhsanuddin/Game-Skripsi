using UnityEngine;

public class DialogueMovements : MonoBehaviour
{
    public Movement movement;
    public Animator animator;
    public Rigidbody2D rb;

    private void Start()
    {
        movement = gameObject.GetComponent<Movement>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public void OnDialogue()
    {
        movement.enabled = false;
        rb.linearVelocity = Vector3.zero;
        animator.SetInteger("state", 0);
    }

    public void OffDialogue()
    {
        movement.enabled = true;
    }
}
