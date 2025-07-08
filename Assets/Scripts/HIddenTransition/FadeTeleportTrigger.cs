using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FadeTeleportTrigger : MonoBehaviour
{
    [SerializeField] private GameObject popupUI;
    [SerializeField] private Transform teleportTarget;
    [SerializeField] private GameObject player;
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeInTime = 1f;
    [SerializeField] private float fadeOutTime = 1f;
    [SerializeField] private float delayBeforeFadeOut = 1f;
    [SerializeField] private GameObject colliderToEnable;
    [SerializeField] private List<GameObject> objectsToToggle; // List of GameObjects to toggle

    private bool isPlayerInTrigger = false;
    private bool hasTeleported = false;
    private Movement playerMovement; // Reference to Movement component

    private void Start()
    {
        popupUI.SetActive(false);

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
        }

        if (colliderToEnable != null)
        {
            colliderToEnable.SetActive(false);
        }

        // Get reference to Movement component
        if (player != null)
        {
            playerMovement = player.GetComponent<Movement>();
        }
    }

    private void Update()
    {
        if (isPlayerInTrigger && !hasTeleported && Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(FadeTeleportSequence());
        }
    }

    private IEnumerator FadeTeleportSequence()
    {
        // Disable vignette and overlay at the start of fade
        if (playerMovement != null)
        {
            playerMovement.vignetteEnabled = false;
            playerMovement.overlayEnabled = false;
        }

        // Deactivate objects in the list
        SetObjectsActive(false);

        // Fade In to full black
        yield return StartCoroutine(Fade(0f, 1f, fadeInTime));

        if (player != null && teleportTarget != null)
        {
            // Disable player
            player.SetActive(false);
            Debug.Log("Player disabled");

            // Move player
            player.transform.position = teleportTarget.position;
            Debug.Log("Player moved to target position: " + teleportTarget.position);

            // Wait one frame to ensure position is updated
            yield return null;

            // Enable player
            player.SetActive(true);
            Debug.Log("Player re-enabled");
        }

        hasTeleported = true;

        if (colliderToEnable != null)
        {
            colliderToEnable.SetActive(true);
        }

        // Delay before starting fade out
        yield return new WaitForSeconds(delayBeforeFadeOut);

        // Fade Out to transparent
        yield return StartCoroutine(Fade(1f, 0f, fadeOutTime));

        // Enable vignette and overlay after fade completes
        if (playerMovement != null)
        {
            playerMovement.vignetteEnabled = true;
            playerMovement.overlayEnabled = true;
        }

        // Reactivate objects in the list
        SetObjectsActive(true);

        popupUI.SetActive(false);
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetImageAlpha(alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetImageAlpha(endAlpha);
    }

    private void SetImageAlpha(float alpha)
    {
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;

            // Raycast active only when screen is fully black
            fadeImage.raycastTarget = alpha >= 0.95f;
        }
    }

    private void SetObjectsActive(bool active)
    {
        foreach (GameObject obj in objectsToToggle)
        {
            if (obj != null)
            {
                obj.SetActive(active);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            isPlayerInTrigger = true;
            popupUI.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            isPlayerInTrigger = false;
            popupUI.SetActive(false);
        }
    }
}