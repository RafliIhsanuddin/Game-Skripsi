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

    private float platformScanTimer = 0f;
    private float platformScanInterval = 0.2f;
    private List<Platform> allPlatforms = new List<Platform>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ScanPlatforms(); // inisialisasi awal
    }

    void Update()
    {
        // Timer pemindaian platform setiap 0.2 detik
        platformScanTimer += Time.deltaTime;
        if (platformScanTimer >= platformScanInterval)
        {
            ScanPlatforms();
            platformScanTimer = 0f;
        }

        UpdateStatus();

        // Cek apakah platform berubah → reset jalur lompat
        if (HasPlatformChanged())
        {
            Debug.Log("Platform Companion atau Player berubah → reset jalur lompat");
            jumpPath.Clear();
            isJumping = false;
        }

        // Logika tidak follow jika player di udara
        if (IsAirborne(player.position) && !IsAirborne(transform.position))
        {
            Debug.Log("Player di udara → companion tidak follow.");
            return;
        }

        if (IsAirborne(transform.position) && !IsAirborne(player.position))
        {
            Debug.Log("Companion di udara → tetap follow player.");
        }

        // Jika di platform sama atau sama-sama di ground, langsung follow
        if (IsSamePlatform())
        {
            EnableRigidbodyIfDisabled("FollowPlayer()");
            FollowPlayer();
            return;
        }

        // Jika Companion di udara atau platform null → aktifkan Rigidbody
        if (IsAirborne(transform.position) || currentPlatform == null || playerPlatform == null)
        {
            EnableRigidbodyIfDisabled("Di udara atau platform null, enable Rigidbody");
        }

        // Jika tidak sedang melompat dan belum punya path → hitung path
        if (!isJumping && jumpPath.Count == 0)
        {
            CalculateJumpPath();
        }

        if (jumpPath.Count > 0)
        {
            ExecuteJumpPath();
        }
    }

    void ScanPlatforms()
    {
        allPlatforms = new List<Platform>(FindObjectsByType<Platform>(FindObjectsSortMode.None));
        Debug.Log($"[ScanPlatforms] Ditemukan {allPlatforms.Count} platform.");
        foreach (var p in allPlatforms)
        {
            Debug.Log($"→ Platform ID: {p.platformID}");
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
        jumpPath.Clear();

        int fromID = currentPlatform != null ? currentPlatform.platformID : -1;
        int toID = playerPlatform != null ? playerPlatform.platformID : -1;

        Debug.Log($"[CalculateJumpPath] Dari platform {fromID} ke {toID}");

        // Companion di ground → naik ke platform
        if (currentPlatform == null && isGrounded && playerPlatform != null)
        {
            var ascendingPath = allPlatforms
                .Where(p => p.platformID >= toID)
                .OrderBy(p => p.platformID)
                .ToList();

            foreach (var p in ascendingPath)
            {
                jumpPath.Add(p);
                Debug.Log($"→ Tambah platform {p.platformID} (naik)");
            }
        }
        // Companion di platform → turun ke ground
        else if (playerPlatform == null && isPlayerGrounded && currentPlatform != null)
        {
            var descendingPath = allPlatforms
                .Where(p => p.platformID <= fromID)
                .OrderByDescending(p => p.platformID)
                .ToList();

            foreach (var p in descendingPath)
            {
                jumpPath.Add(p);
                Debug.Log($"→ Tambah platform {p.platformID} (turun)");
            }

            jumpPath.Add(null); // ground
            Debug.Log("→ Tambah ground sebagai target akhir");
        }
        // Companion dan Player di platform berbeda → cari rute dari current ke target berdasarkan ID
        else if (currentPlatform != null && playerPlatform != null && currentPlatform != playerPlatform)
        {
            int startID = currentPlatform.platformID;
            int endID = playerPlatform.platformID;

            var orderedPath = allPlatforms
                .Where(p => (startID < endID && p.platformID > startID && p.platformID <= endID)
                         || (startID > endID && p.platformID < startID && p.platformID >= endID))
                .OrderBy(p => startID < endID ? p.platformID : -p.platformID)
                .ToList();

            foreach (var p in orderedPath)
            {
                jumpPath.Add(p);
                Debug.Log($"→ Tambah platform {p.platformID} (antar platform)");
            }

            jumpPath.Add(playerPlatform);
            Debug.Log($"→ Tambah platform tujuan {playerPlatform.platformID}");
        }

        jumpIndex = 0;
        isJumping = true;

        if (jumpPath.Count == 0)
        {
            Debug.Log("→ Jalur lompat kosong");
        }
    }


    bool CanReachDirectly(Transform from, Transform to)
    {
        float maxHorizontal = 5f; // atur sesuai kemampuan lompat Companion
        float maxVertical = 3f;

        float dx = Mathf.Abs(to.position.x - from.position.x);
        float dy = to.position.y - from.position.y;

        return dx <= maxHorizontal && dy <= maxVertical;
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

    float DistanceBetween(Transform a, Transform b)
    {
        return Vector2.Distance(a.position, b.position);
    }

    void EnableRigidbodyIfDisabled(string reason)
    {
        if (!rb.simulated)
        {
            rb.simulated = true;
            Debug.Log($"[Rigidbody] ENABLED ({reason})");
        }
    }

    bool HasPlatformChanged()
    {
        return currentPlatform != GetPlatformUnder(transform.position) || playerPlatform != GetPlatformUnder(player.position);
    }
}
