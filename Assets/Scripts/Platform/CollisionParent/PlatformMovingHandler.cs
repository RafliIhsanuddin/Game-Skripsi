using UnityEngine;

public class PlatformMovingHandler : MonoBehaviour
{
    private Rigidbody2D platformRb;
    


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);

            // Jika platform memiliki Rigidbody2D, kirimkan kecepatan ke karakter
            if (TryGetComponent<Rigidbody2D>(out platformRb))
            {
                collision.gameObject.GetComponent<Movement>().SetPlatformVelocity(platformRb.linearVelocity, true);
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(null);
            collision.gameObject.GetComponent<Movement>().SetPlatformVelocity(Vector2.zero, false);
        }
    }
}
