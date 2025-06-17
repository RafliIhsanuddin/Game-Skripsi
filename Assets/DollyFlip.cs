using UnityEngine;
using Yarn.Unity;

public class DollyFlip : MonoBehaviour
{
    private Vector3 originalScale;
    private bool isFlipped = false;

    void Start()
    {
        originalScale = transform.localScale;
    }

    [YarnCommand("flip_dolly")]
    public void FlipDolly()
    {
        if (!isFlipped)
        {
            transform.localScale = new Vector3(-originalScale.x, originalScale.y, originalScale.z);
            isFlipped = true;
        }
    }

    [YarnCommand("unflip_dolly")]
    public void UnflipDolly()
    {
        if (isFlipped)
        {
            transform.localScale = originalScale;
            isFlipped = false;
        }
    }
}
