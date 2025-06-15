using UnityEngine;
using System.Collections;

public class JumpModifierCompanion : MonoBehaviour
{
    public enum ModificationType { SetAbsolute, AddRelative, Multiply }

    [Header("Jump Height Modification")]
    public ModificationType modificationType = ModificationType.SetAbsolute;
    public float value = 10f;
    public bool permanentChange = true;

    [Header("Upward Ray Length Modification")]
    public bool modifyUpwardRayLength = false;
    public ModificationType upwardRayModificationType = ModificationType.SetAbsolute;
    public float upwardRayValue = 5f;
    public bool permanentUpwardRayChange = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Companion companion = other.GetComponent<Companion>();
        if (companion != null)
        {
            // Modify jump height
            float originalHeight = companion.jumpHeight;
            ModifyValue(ref companion.jumpHeight, modificationType, value);
            Debug.Log("Companion's jump height changed to: " + companion.jumpHeight);

            // Modify upward ray length if enabled
            if (modifyUpwardRayLength)
            {
                float originalRayLength = companion.upwardRayLength;
                ModifyValue(ref companion.upwardRayLength, upwardRayModificationType, upwardRayValue);
                Debug.Log("Companion's upward ray length changed to: " + companion.upwardRayLength);
            }

            if (!permanentChange || (modifyUpwardRayLength && !permanentUpwardRayChange))
            {
                StartCoroutine(TemporaryChange(companion, originalHeight,
                    modifyUpwardRayLength ? companion.upwardRayLength : float.NaN));
            }
        }
    }

    private void ModifyValue(ref float targetValue, ModificationType type, float modificationValue)
    {
        switch (type)
        {
            case ModificationType.SetAbsolute:
                targetValue = modificationValue;
                break;
            case ModificationType.AddRelative:
                targetValue += modificationValue;
                break;
            case ModificationType.Multiply:
                targetValue *= modificationValue;
                break;
        }
    }

    IEnumerator TemporaryChange(Companion companion, float originalHeight, float originalRayLength)
    {
        yield return new WaitWhile(() => GetComponent<Collider2D>().bounds.Contains(companion.transform.position));

        if (!permanentChange)
        {
            companion.jumpHeight = originalHeight;
            Debug.Log("Companion's jump height restored to: " + originalHeight);
        }

        if (modifyUpwardRayLength && !permanentUpwardRayChange && !float.IsNaN(originalRayLength))
        {
            companion.upwardRayLength = originalRayLength;
            Debug.Log("Companion's upward ray length restored to: " + originalRayLength);
        }
    }
}
