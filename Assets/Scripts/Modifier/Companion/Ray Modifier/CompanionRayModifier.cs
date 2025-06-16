using UnityEngine;

public class CompanionRayModifier : MonoBehaviour
{
    [Header("Wall Ray Length Settings")]
    [SerializeField] private bool modifyWallRayLength = true;
    [SerializeField] private float newWallRayLength = 30f;
    [SerializeField] private bool revertWallRayOnExit = false;
    private float originalWallRayLength;

    [Header("Ray Height Settings")]
    [SerializeField] private bool modifyRayHeight = true;
    [SerializeField] private float newRayHeight = 22.5f;
    [SerializeField] private bool revertRayHeightOnExit = false;
    private float originalRayHeight;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Companion Dolly")
        {
            Companion companion = other.GetComponent<Companion>();
            if (companion != null)
            {
                // Handle wall ray length
                if (modifyWallRayLength)
                {
                    originalWallRayLength = companion.wallRayLength;
                    companion.wallRayLength = newWallRayLength;
                }

                // Handle ray height
                if (modifyRayHeight)
                {
                    originalRayHeight = companion.rayHeight;
                    companion.rayHeight = newRayHeight;
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "Companion Dolly")
        {
            Companion companion = other.GetComponent<Companion>();
            if (companion != null)
            {
                // Revert wall ray length if needed
                if (modifyWallRayLength && revertWallRayOnExit)
                {
                    companion.wallRayLength = originalWallRayLength;
                }

                // Revert ray height if needed
                if (modifyRayHeight && revertRayHeightOnExit)
                {
                    companion.rayHeight = originalRayHeight;
                }
            }
        }
    }
}
