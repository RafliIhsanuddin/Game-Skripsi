using UnityEngine;

public class DialogueCanvasEnabler : MonoBehaviour
{
    public void EnablePortraits()
    {
        gameObject.SetActive(true);
    }

    public void DisablePortraits()
    {
        gameObject.SetActive(false);
    }
}
