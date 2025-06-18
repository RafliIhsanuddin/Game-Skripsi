using UnityEngine;
using Yarn.Unity;

public class WalkAway : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private GameObject sprite;
    private Vector3 targetPosition;

    private void Start()
    {
        gameObject.SetActive(true);
    }

    [YarnCommand("WalkAway")]
    public void WalkingAway()
    {
        sprite.transform.Rotate(new Vector3(0, 180, 0));
        targetPosition = transform.position + Vector3.right * 10f;
        animator.SetInteger("state", 1);
        StartCoroutine(WalkToTarget());
    }

    private System.Collections.IEnumerator WalkToTarget()
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
