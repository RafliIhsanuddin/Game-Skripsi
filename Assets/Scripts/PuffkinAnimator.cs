using UnityEngine;
using Yarn.Unity;

public class PuffkinAnimator : MonoBehaviour
{
    public Animator animator;

    public void SetAnimation(string animTrigger)
    {
        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name} has no animator set.");
            return;
        }

        Debug.Log($"TRIGGERING: {animTrigger} on {gameObject.name}");
        animator.SetTrigger(animTrigger);
    }
}
