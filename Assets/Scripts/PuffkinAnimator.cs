using UnityEngine;
using Yarn.Unity;

public class PuffkinAnimator : MonoBehaviour
{
    public Animator animator;

    [YarnCommand("HappyPuffkin")]
    public void SetAnimation()
    {
        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name} has no animator set.");
            return;
        }

        animator.SetTrigger("Happy");
    }
}
