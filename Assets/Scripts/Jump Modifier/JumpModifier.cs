using UnityEngine;

public class JumpModifier : MonoBehaviour
{
    public enum ModificationType { SetAbsolute, AddRelative, Multiply }

    public ModificationType modificationType = ModificationType.SetAbsolute;
    public float value = 10f;
    public bool permanentChange = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        CompanionFinal companion = other.GetComponent<CompanionFinal>();
        if (companion != null)
        {
            switch (modificationType)
            {
                case ModificationType.SetAbsolute:
                    companion.jumpHeight = value;
                    break;
                case ModificationType.AddRelative:
                    companion.jumpHeight += value;
                    break;
                case ModificationType.Multiply:
                    companion.jumpHeight *= value;
                    break;
            }

            Debug.Log("Companion's jump height changed to: " + companion.jumpHeight);

            if (!permanentChange)
            {
                // Store original value and restore on exit
                StartCoroutine(TemporaryChange(companion));
            }
        }
    }

    System.Collections.IEnumerator TemporaryChange(CompanionFinal companion)
    {
        float originalHeight = companion.jumpHeight;
        yield return new WaitWhile(() => GetComponent<Collider2D>().bounds.Contains(companion.transform.position));
        companion.jumpHeight = originalHeight;
        Debug.Log("Companion's jump height restored to: " + originalHeight);
    }
}
