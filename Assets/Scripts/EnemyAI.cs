using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    public int damage = 10; 

    private float attackTimer = 0f;
    private bool isPlayerInRange = false;

    void Awake()
    {
        player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > attackRange)
            {
                ChasePlayer();
                isPlayerInRange = false;
            }
            else
            {
                isPlayerInRange = true;
            }

            if (isPlayerInRange && attackTimer <= 0f)
            {
                AttackPlayer();
                attackTimer = attackCooldown;
            }

            // Decrease attack cooldown timer
            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }
        }
    }

    void ChasePlayer()
    {
        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
    }

    void AttackPlayer()
    {
        PlayerHP playerHP = player.GetComponent<PlayerHP>();
        if (playerHP != null)
        {
            playerHP.TakeDamage(damage);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
