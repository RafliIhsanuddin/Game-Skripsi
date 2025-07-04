using UnityEngine;

public class PromptTriggers : MonoBehaviour
{
    public GameObject jumpPrompt;
    public GameObject doubleJumpPrompt;
    public GameObject dashPrompt;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (gameObject.name == "Jump")
            {
                jumpPrompt.SetActive(true);
            }

            if (gameObject.name == "DoubleJump")
            {
                doubleJumpPrompt.SetActive(true);
            }

            if (gameObject.name == "Dash")
            {
                dashPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (gameObject.name == "Jump")
            {
                jumpPrompt.SetActive(false);
            }

            if (gameObject.name == "DoubleJump")
            {
                doubleJumpPrompt.SetActive(false);
            }

            if (gameObject.name == "Dash")
            {
                dashPrompt.SetActive(false);
            }
        }
    }
}
