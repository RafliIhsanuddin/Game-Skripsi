using UnityEngine;

public class RandomAnimatorStart : MonoBehaviour
{
    private void Start()
    {
        // Generate satu nilai random untuk semua animator
        float randomOffset = Random.Range(0f, 1f);

        // Ambil semua komponen Animator di children
        Animator[] animators = GetComponentsInChildren<Animator>();

        foreach (Animator animator in animators)
        {
            if (animator.runtimeAnimatorController == null) continue;

            // Ambil informasi state aktif di layer 0
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            // Play animasi dengan offset random yang sama
            animator.Play(state.fullPathHash, 0, randomOffset);
        }
    }
}
