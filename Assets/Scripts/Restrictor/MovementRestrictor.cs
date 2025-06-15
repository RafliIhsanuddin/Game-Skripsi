using UnityEngine;



[RequireComponent(typeof(Collider2D))]
public class MovementRestrictor : MonoBehaviour
{
    public enum RestrictionType
    {
        LockLeft,
        LockRight,
        LockAll,
        NoRestriction // New option added
    }

    [Header("Movement Restrictions")]
    [SerializeField] private RestrictionType restrictionType = RestrictionType.LockLeft;
    [SerializeField] private bool affectOnlyCompanion = true;

    [Header("Teleport Settings")]
    [SerializeField] private float teleportDistance = 5f;
    [SerializeField] private Color gizmoColor = Color.cyan;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null) return;

        // Check if we should process Kael's entry for teleportation
        if (other.gameObject.name == "Kael FBF")
        {
            HandleKaelEntry();
        }

        // Original companion movement restriction logic
        if (affectOnlyCompanion && other.gameObject.name != "Companion Dolly") return;

        Companion companion = other.GetComponent<Companion>();
        if (companion == null) return;

        ApplyRestriction(companion);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other == null) return;

        // Original companion movement restriction logic
        if (affectOnlyCompanion && other.gameObject.name != "Companion Dolly") return;

        Companion companion = other.GetComponent<Companion>();
        if (companion == null) return;

        ApplyRestriction(companion);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null) return;

        // Original companion movement restriction logic
        if (affectOnlyCompanion && other.gameObject.name != "Companion Dolly") return;

        Companion companion = other.GetComponent<Companion>();
        if (companion == null) return;

        // Only reset if we were actually restricting movement
        if (restrictionType != RestrictionType.NoRestriction)
        {
            companion.ResetMovementRestriction();
        }
    }

    private void ApplyRestriction(Companion companion)
    {
        if (companion == null || restrictionType == RestrictionType.NoRestriction) return;

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
                // NoRestriction case doesn't need handling as we return early
        }
    }

    private void HandleKaelEntry()
    {
        // Find the companion in the scene
        GameObject companionObj = GameObject.Find("Companion Dolly");
        if (companionObj == null) return;

        // Calculate distance from this collider to companion
        float distance = Vector2.Distance(transform.position, companionObj.transform.position);

        // Teleport if beyond threshold distance
        if (distance > teleportDistance)
        {
            companionObj.transform.position = transform.position;
            Debug.Log($"Companion teleported to restriction zone (distance: {distance})");
        }
    }

    private void OnDrawGizmos()
    {
        // Draw teleport distance visualization
        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, teleportDistance);
    }
}
