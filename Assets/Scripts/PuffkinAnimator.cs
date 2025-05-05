using UnityEngine;
using Yarn.Unity;

public class PuffkinAnimator : MonoBehaviour
{
    public Animator animator;

    [YarnCommand("set_anim")]
    public void SetAnimation(string animTrigger)
    {
        if (animator == null)
        {
            Debug.LogWarning($"{gameObject.name} has no animator set.");
            return;
        }

        animator.SetTrigger(animTrigger);
    }
}
