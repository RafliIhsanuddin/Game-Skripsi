using UnityEngine;

public class DialogueMovements : MonoBehaviour
{
    public Movement movement;

    private void Start()
    {
        movement = gameObject.GetComponent<Movement>();
    }

    public void OnDialogue()
    {
        movement.enabled = false;
    }

    public void OffDialogue()
    {
        movement.enabled = true;
    }
}
