using UnityEngine;
using System.Collections;

public class SimpleMovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private bool moveHorizontal = true;
    [SerializeField] private bool moveVertical = false;
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private bool reverseDirection = false; // Tambahan boolean

    private Vector3 _startPosition;
    private Vector3 _targetOffset;
    private bool _movingForward = true;

    private Collider2D _platformCollider;

    private void Start()
    {
        _startPosition = transform.position;

        // Terapkan reverseDirection
        float horizontalOffset = moveHorizontal ? moveDistance : 0f;
        float verticalOffset = moveVertical ? moveDistance : 0f;

        if (reverseDirection)
        {
            horizontalOffset *= -1f;
            verticalOffset *= -1f;
        }

        _targetOffset = new Vector3(horizontalOffset, verticalOffset, 0f);

        _platformCollider = GetComponentInChildren<Collider2D>();
        if (_platformCollider == null)
        {
            Debug.LogError("Child Collider2D not found! Please add a child with a Collider2D.");
        }
    }

    private void FixedUpdate()
    {
        Vector3 targetPosition = _startPosition + (_movingForward ? _targetOffset : -_targetOffset);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.fixedDeltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            _movingForward = !_movingForward;
        }
    }

    private void OnEnable()
    {
        if (_platformCollider != null)
            _platformCollider.isTrigger = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.transform.SetParent(transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            StartCoroutine(DetachAfterDelay(collision.transform));
        }
    }

    private IEnumerator DetachAfterDelay(Transform target)
    {
        yield return null; // Tunggu 1 frame agar aman dari error parenting
        if (target != null && target.parent == transform)
        {
            target.SetParent(null);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            _startPosition = transform.position;

        Gizmos.color = Color.cyan;

        float horizontalOffset = moveHorizontal ? moveDistance : 0f;
        float verticalOffset = moveVertical ? moveDistance : 0f;

        if (reverseDirection)
        {
            horizontalOffset *= -1f;
            verticalOffset *= -1f;
        }

        Vector3 offset = new Vector3(horizontalOffset, verticalOffset, 0f);

        Gizmos.DrawLine(_startPosition, _startPosition + offset);
        Gizmos.DrawLine(_startPosition, _startPosition - offset);
        Gizmos.DrawWireSphere(_startPosition + offset, 0.1f);
        Gizmos.DrawWireSphere(_startPosition - offset, 0.1f);
    }
}
