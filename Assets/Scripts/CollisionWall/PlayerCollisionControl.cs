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
                playerMovement.ResetMovement(); // Kembalikan ke gerakan normal
            }
        }
    }

}
