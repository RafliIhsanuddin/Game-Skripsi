using System.Collections.Generic;
using UnityEngine;

public class DialogueMovements : MonoBehaviour
{
    public Movement movement;
    public Animator animator;
    public Rigidbody2D rb;
    public AudioSource runAudio;
    public AudioSource walkAudio;

    [Header("Visual Effect Control")]
    public bool controlVisualEffect = true; // Toggle to enable/disable vignette & overlay logic

    [Header("Optional GameObjects to Disable During Dialogue")]
    public List<GameObject> objectsToDisableDuringDialogue;

    private void Start()
    {
        movement = gameObject.GetComponent<Movement>();
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    public void OnDialogue()
    {
        movement.isMovementLocked = true;
        rb.linearVelocity = Vector2.zero;

        // Matikan audio saat dialog
        if (runAudio != null) runAudio.enabled = false;
        if (walkAudio != null) walkAudio.enabled = false;

        if (animator != null)
            animator.SetInteger("state", 0);

        // Matikan vignette & overlay jika diizinkan
        if (controlVisualEffect)
        {
            movement.vignetteEnabled = false;
            movement.overlayEnabled = false;
        }

        // Nonaktifkan objek tambahan jika ada
        if (objectsToDisableDuringDialogue != null)
        {
            foreach (GameObject obj in objectsToDisableDuringDialogue)
            {
                if (obj != null) obj.SetActive(false);
            }
        }
    }

    public void OffDialogue()
    {
        movement.isMovementLocked = false;

        // Nyalakan audio kembali
        if (runAudio != null) runAudio.enabled = true;
        if (walkAudio != null) walkAudio.enabled = true;

        // Aktifkan kembali vignette & overlay jika diizinkan
        if (controlVisualEffect)
        {
            movement.vignetteEnabled = true;
            movement.overlayEnabled = true;
        }

        // Aktifkan kembali objek yang sebelumnya dinonaktifkan
        if (objectsToDisableDuringDialogue != null)
        {
            foreach (GameObject obj in objectsToDisableDuringDialogue)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }
}
