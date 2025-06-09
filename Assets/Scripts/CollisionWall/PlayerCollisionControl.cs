using UnityEngine;

public class PlayerCollisionControl : MonoBehaviour
{
    public bool StopRight; // Menentukan apakah karakter berhenti bergerak ke kanan atau kiri

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Movement playerMovement = collision.gameObject.GetComponent<Movement>();
            if (playerMovement != null)
            {
                playerMovement.SetStopRight(StopRight);
                playerMovement.DisableJump(); // Nonaktifkan kemampuan melompat
                playerMovement.DisableDash(); // Nonaktifkan kemampuan dash
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Movement playerMovement = collision.gameObject.GetComponent<Movement>();
            if (playerMovement != null)
            {
                playerMovement.ResetMovement(); // Kembalikan gerakan normal
                playerMovement.EnableJump();   // Kembalikan kemampuan melompat
                playerMovement.EnableDash();   // Kembalikan kemampuan dash
            }
        }
    }

}
