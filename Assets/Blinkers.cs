using UnityEngine;
using Yarn.Unity;

public class Blinkers : MonoBehaviour
{
    public GameObject player; 
    public float appearDistance = 10f;
    private Movement movement;
    public DialogueRunner dialogueRunner;
    private ParticleSystem particles;

    private void Start()
    {
        movement = player.GetComponent<Movement>();

        // Get the Particle System on this GameObject
        particles = GetComponent<ParticleSystem>();

        if (particles == null)
        {
            Debug.LogWarning("No Particle System found on this Blinker.");
        }

        // Make sure particles are not playing at start
        particles.Stop();
    }

    private void Update()
    {
        if (player == null || particles == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);

        if (distance <= appearDistance)
        {
            if (!particles.isPlaying)
            {
                particles.Play();
            }
        }
        else
        {
            if (particles.isPlaying)
            {
                particles.Stop();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (gameObject.name == "Double Jump Blinker")
            {
                movement.doubleJumpEnabled = true;
                dialogueRunner.StartDialogue("enable_double_jump");
                gameObject.SetActive(false);
            }

            if (gameObject.name == "Wall Jump Blinker")
            {
                movement.wallDashDoubleJumpEnabled = true;
                movement.wallJumpDoubleJumpEnabled = true;
                dialogueRunner.StartDialogue("enable_wall_jump");
                gameObject.SetActive(false);
            }
        }
    }
}
