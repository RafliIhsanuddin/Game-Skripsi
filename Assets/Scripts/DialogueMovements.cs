using UnityEngine;

public class DialogueMovements : MonoBehaviour
{
    public Movement movement;
    public Animator animator;
    public Rigidbody2D rb;
    public AudioSource audio;

    private void Start()
    {
        movement = gameObject.GetComponent<Movement>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public void OnDialogue()
    {
        movement.isMovementLocked = true;
        rb.linearVelocity = Vector3.zero;
        audio.enabled = false;
        animator.SetInteger("state", 0);
    }

    public void OffDialogue()
    {
        movement.isMovementLocked = false;
        audio.enabled = true;
    }
}
