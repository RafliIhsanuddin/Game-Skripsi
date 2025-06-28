using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDarkKaelModifier : MonoBehaviour
{
    public enum RevertOption { ChangePermanently, RevertOnExit }

    [Header("Wall Ray Length Settings")]
    [SerializeField] private bool modifyWallRayLength = false;
    [SerializeField] private float newWallRayLength = 30f;
    [SerializeField] private RevertOption wallRayLengthOption = RevertOption.RevertOnExit;
    private float originalWallRayLength;

    // --- CONTOH UNTUK LAINNYA ---
    [Header("Ray Height Settings")]
    [SerializeField] private bool modifyRayHeight = false;
    [SerializeField] private float newRayHeight = 22.5f;
    [SerializeField] private RevertOption rayHeightOption = RevertOption.RevertOnExit;
    private float originalRayHeight;

    [Header("Jump Height Settings")]
    [SerializeField] private bool modifyJumpHeight = false;
    [SerializeField] private float newJumpHeight = 12f;
    [SerializeField] private RevertOption jumpHeightOption = RevertOption.RevertOnExit;
    private float originalJumpHeight;

    [Header("Upward Ray Length Settings")]
    [SerializeField] private bool modifyUpwardRayLength = false;
    [SerializeField] private float newUpwardRayLength = 6f;
    [SerializeField] private RevertOption upwardRayLengthOption = RevertOption.RevertOnExit;
    private float originalUpwardRayLength;

    [Header("GameObject Enable List")]
    [SerializeField] private bool useEnableList = false;
    [SerializeField] private List<GameObject> enableObjects;
    [SerializeField] private RevertOption enableListOption = RevertOption.RevertOnExit;

    [Header("GameObject Disable List")]
    [SerializeField] private bool useDisableList = false;
    [SerializeField] private List<GameObject> disableObjects;
    [SerializeField] private RevertOption disableListOption = RevertOption.RevertOnExit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.name == "Dark Kael")
        {
            Debug.Log("Dark Kael entered the trigger zone.");

            Companion companion = other.GetComponent<Companion>();
            if (companion != null)
            {
                if (modifyWallRayLength)
                {
                    originalWallRayLength = companion.wallRayLength;
                    companion.wallRayLength = newWallRayLength;
                    Debug.Log("Modified Wall Ray Length to " + newWallRayLength);
                }

                if (modifyRayHeight)
                {
                    originalRayHeight = companion.rayHeight;
                    companion.rayHeight = newRayHeight;
                    Debug.Log("Modified Ray Height to " + newRayHeight);
                }

                if (modifyJumpHeight)
                {
                    originalJumpHeight = companion.jumpHeight;
                    companion.jumpHeight = newJumpHeight;
                    Debug.Log("Modified Jump Height to " + newJumpHeight);
                }

                if (modifyUpwardRayLength)
                {
                    originalUpwardRayLength = companion.upwardRayLength;
                    companion.upwardRayLength = newUpwardRayLength;
                    Debug.Log("Modified Upward Ray Length to " + newUpwardRayLength);
                }
            }

            if (useEnableList)
            {
                foreach (GameObject obj in enableObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log("Enabled GameObject: " + obj.name);
                    }
                }
            }

            if (useDisableList)
            {
                foreach (GameObject obj in disableObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log("Disabled GameObject: " + obj.name);
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.name == "Dark Kael")
        {
            Debug.Log("Dark Kael exited the trigger zone.");

            Companion companion = other.GetComponent<Companion>();
            if (companion != null)
            {
                if (modifyWallRayLength && wallRayLengthOption == RevertOption.RevertOnExit)
                {
                    companion.wallRayLength = originalWallRayLength;
                    Debug.Log("Reverted Wall Ray Length to " + originalWallRayLength);
                }

                if (modifyRayHeight && rayHeightOption == RevertOption.RevertOnExit)
                {
                    companion.rayHeight = originalRayHeight;
                    Debug.Log("Reverted Ray Height to " + originalRayHeight);
                }

                if (modifyJumpHeight && jumpHeightOption == RevertOption.RevertOnExit)
                {
                    companion.jumpHeight = originalJumpHeight;
                    Debug.Log("Reverted Jump Height to " + originalJumpHeight);
                }

                if (modifyUpwardRayLength && upwardRayLengthOption == RevertOption.RevertOnExit)
                {
                    companion.upwardRayLength = originalUpwardRayLength;
                    Debug.Log("Reverted Upward Ray Length to " + originalUpwardRayLength);
                }
            }

            if (useEnableList && enableListOption == RevertOption.RevertOnExit)
            {
                foreach (GameObject obj in enableObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log("Disabled GameObject: " + obj.name + " (on exit)");
                    }
                }
            }

            if (useDisableList && disableListOption == RevertOption.RevertOnExit)
            {
                foreach (GameObject obj in disableObjects)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log("Enabled GameObject: " + obj.name + " (on exit)");
                    }
                }
            }
        }
    }
}
