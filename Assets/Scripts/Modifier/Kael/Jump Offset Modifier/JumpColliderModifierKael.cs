using UnityEngine;

public class JumpColliderModifierKael : MonoBehaviour
{
    [SerializeField] private Vector2 newJumpOffset = new Vector2(0.361042f, 2.143058f);
    [SerializeField] private Vector2 newJumpSize = new Vector2(8.456917f, 10.84726f);

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Kael FBF")
        {
            Movement movement = other.GetComponent<Movement>();
            if (movement != null)
            {
                movement.jumpOffset = newJumpOffset;
                movement.jumpSize = newJumpSize;
            }
        }
    }
}
