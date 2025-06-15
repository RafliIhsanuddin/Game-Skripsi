using UnityEngine;



[RequireComponent(typeof(Collider2D))]
public class MovementRestrictor : MonoBehaviour
{
    public enum RestrictionType
    {
        LockLeft,
        LockRight,
        LockAll
    }

    [SerializeField] private RestrictionType restrictionType = RestrictionType.LockLeft;
    [SerializeField] private bool affectOnlyCompanion = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (affectOnlyCompanion && other.gameObject.name != "Companion Dolly") return;

        Companion companion = other.GetComponent<Companion>();
        if (companion == null) return;

        ApplyRestriction(companion);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (affectOnlyCompanion && other.gameObject.name != "Companion Dolly") return;

        Companion companion = other.GetComponent<Companion>();
        if (companion == null) return;

        ApplyRestriction(companion);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (affectOnlyCompanion && other.gameObject.name != "Companion Dolly") return;

        Companion companion = other.GetComponent<Companion>();
        if (companion == null) return;

        companion.ResetMovementRestriction();
    }

    private void ApplyRestriction(Companion companion)
    {
        switch (restrictionType)
        {
            case RestrictionType.LockLeft:
                companion.RestrictMovement(left: true, right: false);
                break;
            case RestrictionType.LockRight:
                companion.RestrictMovement(left: false, right: true);
                break;
            case RestrictionType.LockAll:
                companion.RestrictMovement(left: true, right: true);
                break;
        }
    }
}
