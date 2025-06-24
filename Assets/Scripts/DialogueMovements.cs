using UnityEngine;

public class DialogueMovements : MonoBehaviour
{
    public Movement movement;
    public Animator animator;
    public Rigidbody2D rb;
    public AudioSource runAudio;
    public AudioSource walkAudio;

    private void Start()
    {
        movement = gameObject.GetComponent<Movement>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public void OnDialogue()
    {
        movement.isMovementLocked = true;
        rb.linearVelocity = Vector2.zero;

        // Matikan audio saat dialog (run & walk)
        if (runAudio != null) runAudio.enabled = false;
        if (walkAudio != null) walkAudio.enabled = false;

        if (animator != null)
            animator.SetInteger("state", 0);
    }

    public void OffDialogue()
    {
        movement.isMovementLocked = false;

        // Nyalakan kembali audio
        if (runAudio != null) runAudio.enabled = true;
        if (walkAudio != null) walkAudio.enabled = true;
    }
}
