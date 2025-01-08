using UnityEngine;
using Yarn.Unity;

public class DialogueTrigger : MonoBehaviour
{
    public LayerMask kael;
    public DialogueRunner dialogueRunner;

    // Update is called once per frame
    void Update()
    {
        if (Physics2D.Raycast(transform.position, Vector2.left, 2f, kael))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                dialogueRunner.StartDialogue("Start");
            }
        }
    }

    // Visualize the raycast in the Scene view
    private void OnDrawGizmos()
    {
        // Set the color of the Gizmo
        Gizmos.color = Color.red;

        // Define the ray's direction and length
        Vector2 direction = Vector2.left;
        float rayLength = 2f;

        // Draw the raycast line
        Gizmos.DrawRay(transform.position, direction * rayLength);

        // Optional: Draw a sphere at the endpoint of the ray
        Gizmos.DrawSphere((Vector2)transform.position + direction * rayLength, 0.05f);
    }
}
