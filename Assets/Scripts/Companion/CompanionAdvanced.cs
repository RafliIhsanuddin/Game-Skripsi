using UnityEngine;
using Pathfinding;
using System.Collections;

public class CompanionAdvanced : MonoBehaviour
{
    [Header("Pathfinding")]
    public Transform target;
    public float pathUpdateSeconds = 1f;

    [Header("Physics")]
    public float speed = 200f;
    public float nextWayPointDistance = 3f;
    public float jumpNodeHeightRequierment = 0.8f;
    public float jumpModifier = 0.3f;
    public float jumpCheckOffset = 0.1f;

    [Header("Custom Behaviour")]
    public bool followEnabled = true;
    public bool jumpEnabled = true;
    public bool directionLookEnabled = true;

    [Header("Layers")]
    public LayerMask groundLayer;
    public LayerMask platformLayer;

    private Path path;
    private int currentWaypoint = 0;
    private bool isGrounded = false;
    private bool isJumping = false; // Status untuk memeriksa apakah sudah lompat

    private Seeker seeker;
    private Rigidbody2D rb;
    private Collider2D col;

    private Coroutine followCoroutine;

    private void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        if (target == null)
        {
            Debug.LogError("Target tidak ditentukan!");
            return;
        }

        StartCoroutine(UpdatePathRoutine());
    }

    private IEnumerator UpdatePathRoutine()
    {
        while (followEnabled)
        {
            if (seeker.IsDone())
            {
                Debug.Log("Mulai mencari path dari " + rb.position + " ke " + target.position);
                seeker.StartPath(rb.position, target.position, OnPathComplete);
            }
            else
            {
                Debug.LogWarning("Seeker sedang sibuk dengan path sebelumnya.");
            }

            yield return new WaitForSeconds(pathUpdateSeconds);
        }
    }

    private void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            Debug.Log("Path ditemukan dengan " + p.vectorPath.Count + " waypoints");
            path = p;
            currentWaypoint = 0;

            if (followCoroutine != null)
            {
                StopCoroutine(followCoroutine);
            }

            followCoroutine = StartCoroutine(FollowPathRoutine());
        }
        else
        {
            Debug.LogWarning("Terjadi kesalahan dalam pencarian path: " + p.errorLog);
        }
    }

    private IEnumerator FollowPathRoutine()
    {
        while (path != null && currentWaypoint < path.vectorPath.Count)
        {
            PathFollow();
            yield return new WaitForFixedUpdate();
        }
    }

    private void PathFollow()
    {
        if (path == null || path.vectorPath.Count == 0)
        {
            Debug.LogWarning("Path is null atau tidak memiliki waypoint.");
            return;
        }

        if (currentWaypoint >= path.vectorPath.Count)
        {
            Debug.LogWarning("currentWaypoint exceeds path count.");
            return;
        }

        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, col.bounds.extents.y + jumpCheckOffset, groundLayer);

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;

        if (direction.magnitude < 0.1f)
        {
            Debug.Log("Waypoint terlalu dekat, lanjut ke waypoint berikutnya.");
            currentWaypoint++;
            return;
        }

        Vector2 force = direction * speed * Time.fixedDeltaTime;

        // Logika lompat
        if (jumpEnabled && isGrounded && !isJumping) // Cek apakah sudah lompat
        {
            float nodeHeightDiff = path.vectorPath[currentWaypoint].y - transform.position.y;

            if (nodeHeightDiff > jumpNodeHeightRequierment)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.up, nodeHeightDiff + 0.5f, platformLayer);
                if (hit.collider != null)
                {
                    Debug.Log("Melompat karena perbedaan tinggi: " + nodeHeightDiff);
                    rb.AddForce(Vector2.up * speed * jumpModifier);
                    isJumping = true; // Tandai sudah melompat
                }
                else
                {
                    Debug.Log("Tidak bisa melompat karena tidak ada platform di atas.");
                }
            }
        }

        rb.AddForce(force);

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWayPointDistance)
        {
            Debug.Log("Waypoint tercapai, lanjut ke berikutnya.");
            currentWaypoint++;
        }

        if (directionLookEnabled)
        {
            if (direction.x > 0.05f)
                transform.localScale = new Vector3(1, 1, 1);
            else if (direction.x < -0.05f)
                transform.localScale = new Vector3(-1, 1, 1);
        }

        // Reset isJumping jika sudah mendarat
        if (isGrounded && isJumping)
        {
            isJumping = false; // Reset status lompat setelah mendarat
        }
    }






}