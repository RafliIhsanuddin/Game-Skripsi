using UnityEngine;
using UnityEngine.SceneManagement;

public class GetOutHouse : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            SceneManager.LoadScene("Desert Final Scene");
        }
    }
}
