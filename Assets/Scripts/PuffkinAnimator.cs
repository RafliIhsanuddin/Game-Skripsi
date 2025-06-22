using UnityEngine;
using Yarn.Unity;

public class PuffkinAnimator : MonoBehaviour
{
    public Animator animator;
    private SpriteRenderer sprite;

    private void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

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

    [YarnCommand("FadePuffkin")]
    public void FadingPuffkin()
    {
        var color = sprite.color;
        color.a = 0.3f;
        sprite.color = color;
    }
}
