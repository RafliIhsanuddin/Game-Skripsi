using UnityEngine;

public class FlexibleMover : MonoBehaviour
{
    [SerializeField] private float moveDistance = 3f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Direction Options")]
    [SerializeField] private bool moveHorizontal = false;
    [SerializeField] private bool moveVertical = false;
    [SerializeField] private bool moveDiagonalLeft = false;
    [SerializeField] private bool moveDiagonalRight = false;
    [SerializeField] private bool reverseDirection = false;

    private Vector3 _startPos;
    private Vector3 _direction;

    private void Start()
    {
        _startPos = transform.position;
        _direction = GetDirection();
        if (reverseDirection) _direction *= -1f;
    }

    private void Update()
    {
        float pingPong = Mathf.PingPong(Time.time * moveSpeed, moveDistance);
        transform.position = _startPos + _direction * pingPong;
    }

    private Vector3 GetDirection()
    {
        if (moveHorizontal)
            return Vector3.right;
        if (moveVertical)
            return Vector3.up;
        if (moveDiagonalLeft)
            return new Vector3(-1f, 1f).normalized;
        if (moveDiagonalRight)
            return new Vector3(1f, 1f).normalized;
        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 startPos = transform.position;
        Vector3 direction = GetDirection();
        if (reverseDirection) direction *= -1f;

        Vector3 endPos = startPos + direction * moveDistance;

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(startPos, endPos);
        Gizmos.DrawWireSphere(endPos, 0.1f);
    }
}
