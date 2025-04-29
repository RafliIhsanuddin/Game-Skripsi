using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CompanionAI : MonoBehaviour
{
    public Transform player;
    public LayerMask groundLayer;
    public LayerMask platformLayer;
    public float moveSpeed = 3f;
    public float jumpHeight = 1.5f;
    public float arriveThreshold = 0.1f;
    public float groundCheckDistance = 0.2f;
    public Vector2 groundCheckOffset;

    private Rigidbody2D rb;

    private Platform currentPlatform;
    private Platform playerPlatform;
    private bool isGrounded;
    private bool isPlayerGrounded;
    private bool isJumping;

    [SerializeField] private List<Platform> jumpPath = new List<Platform>();
    private int jumpIndex = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        UpdateStatus();

        if (IsAirborne(player.position) && !IsAirborne(transform.position))
        {
            Debug.Log("Player di udara → companion tidak follow.");
            return;
        }

        if (IsAirborne(transform.position) && !IsAirborne(player.position))
        {
            Debug.Log("Companion di udara → tetap follow player.");
        }

        if (IsSamePlatform())
        {
            EnableRigidbodyIfDisabled("FollowPlayer()");
            FollowPlayer();
            return;
        }

        if (!isJumping && jumpPath.Count == 0)
        {
            CalculateJumpPath();
        }

        if (jumpPath.Count > 0)
        {
            ExecuteJumpPath();
        }
    }

    void UpdateStatus()
    {
        isGrounded = IsOnGround(transform.position);
        isPlayerGrounded = IsOnGround(player.position);

        currentPlatform = GetPlatformUnder(transform.position);
        playerPlatform = GetPlatformUnder(player.position);

        string companionStatus = isGrounded ? "di ground" : currentPlatform != null ? $"di platform {currentPlatform.platformID}" : "di udara";
        string playerStatus = isPlayerGrounded ? "di ground" : playerPlatform != null ? $"di platform {playerPlatform.platformID}" : "di udara";
        Debug.Log($"Companion: {companionStatus} | Player: {playerStatus}");
    }

    bool IsOnGround(Vector2 pos)
    {
        Vector2 origin = pos + groundCheckOffset;
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.green, 0.1f);
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    bool IsAirborne(Vector2 pos)
    {
        return !IsOnGround(pos) && GetPlatformUnder(pos) == null;
    }

    Platform GetPlatformUnder(Vector2 pos)
    {
        RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.down, 2f, platformLayer);
        if (hit.collider != null)
        {
            Debug.DrawRay(pos, Vector2.down * 2f, Color.green);
            return hit.collider.GetComponent<Platform>();
        }
        else
        {
            Debug.DrawRay(pos, Vector2.down * 2f, Color.red);
        }
        return null;
    }

    bool IsSamePlatform()
    {
        if (isGrounded && isPlayerGrounded) return true;
        if (currentPlatform != null && playerPlatform != null)
            return currentPlatform.platformID == playerPlatform.platformID;
        return false;
    }

    void FollowPlayer()
    {
        Vector2 target = new Vector2(player.position.x, transform.position.y);
        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
    }

    void CalculateJumpPath()
    {
        List<Platform> all = new List<Platform>(FindObjectsByType<Platform>(FindObjectsSortMode.None));
        all.Sort((a, b) => a.platformID.CompareTo(b.platformID));

        int fromID = currentPlatform != null ? currentPlatform.platformID : -1;
        int toID = playerPlatform != null ? playerPlatform.platformID : -1;

        jumpPath.Clear();

        Debug.Log($"Menyusun jalur lompat dari platform {fromID} ke platform {toID}");

        Debug.Log("Platform yang ditemukan:");
        foreach (var p in all)
        {
            Debug.Log($"Platform ID: {p.platformID}");
        }

        if (fromID < toID)
        {
            foreach (var p in all)
            {
                if (p.platformID > fromID && p.platformID <= toID)
                {
                    jumpPath.Add(p);
                    Debug.Log($"Menambahkan platform {p.platformID} ke jalur lompat");
                }
            }
        }
        else if (fromID > toID)
        {
            for (int i = all.Count - 1; i >= 0; i--)
            {
                if (all[i].platformID < fromID && all[i].platformID >= toID)
                {
                    jumpPath.Add(all[i]);
                    Debug.Log($"Menambahkan platform {all[i].platformID} ke jalur lompat");
                }
            }
        }

        if (playerPlatform == null && isPlayerGrounded)
        {
            jumpPath.Add(null);
            Debug.Log("Menambahkan ground sebagai target terakhir");
        }

        jumpIndex = 0;
        isJumping = true;

        Debug.Log("Menyusun jalur lompat:");
        if (jumpPath.Count == 0)
        {
            Debug.Log("Jalur lompat kosong, tidak ada platform yang bisa dijangkau");
        }
        else
        {
            foreach (var p in jumpPath)
            {
                Debug.Log("→ " + (p != null ? $"Platform {p.platformID}" : "Ground"));
            }
        }
    }

    void ExecuteJumpPath()
    {
        if (jumpIndex >= jumpPath.Count)
        {
            isJumping = false;
            jumpPath.Clear();
            EnableRigidbodyIfDisabled("Selesai semua lompatan");
            return;
        }

        Platform targetPlatform = jumpPath[jumpIndex];

        Vector2 target = targetPlatform != null
            ? new Vector2(targetPlatform.transform.position.x, targetPlatform.transform.position.y + jumpHeight)
            : new Vector2(player.position.x, transform.position.y);

        DisableRigidbodyIfEnabled($"Mulai lompat ke {(targetPlatform != null ? $"platform {targetPlatform.platformID}" : "ground")}");

        transform.position = Vector2.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, target) < arriveThreshold)
        {
            jumpIndex++;
            EnableRigidbodyIfDisabled($"Tiba di {(targetPlatform != null ? $"platform {targetPlatform.platformID}" : "ground")}");
            StartCoroutine(WaitUntilLandedThenContinue());
        }
    }

    System.Collections.IEnumerator WaitUntilLandedThenContinue()
    {
        yield return new WaitUntil(() => IsOnGround(transform.position) || GetPlatformUnder(transform.position) != null);
        yield return new WaitForSeconds(0.05f);
        DisableRigidbodyIfEnabled("Siap lompat ke platform berikutnya");
    }

    void DisableRigidbodyIfEnabled(string reason)
    {
        if (rb.simulated)
        {
            rb.simulated = false;
            Debug.Log($"[Rigidbody] DISABLED ({reason})");
        }
    }

    void EnableRigidbodyIfDisabled(string reason)
    {
        if (!rb.simulated)
        {
            rb.simulated = true;
            Debug.Log($"[Rigidbody] ENABLED ({reason})");
        }
    }
}
