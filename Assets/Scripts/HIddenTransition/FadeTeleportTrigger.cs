using System.Collections;
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
    [SerializeField] private float delayBeforeFadeOut = 1f; // Delay setelah teleport sebelum fade out
    [SerializeField] private GameObject colliderToEnable;

    private bool isPlayerInTrigger = false;
    private bool hasTeleported = false;

    private void Start()
    {
        popupUI.SetActive(false);

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // Pastikan GameObject-nya aktif
            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
        }

        if (colliderToEnable != null)
        {
            colliderToEnable.SetActive(false);
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
        // Fade In ke hitam penuh
        yield return StartCoroutine(Fade(0f, 1f, fadeInTime));

        if (player != null && teleportTarget != null)
        {
            // Disable player
            player.SetActive(false);
            Debug.Log("Player dinonaktifkan");

            // Pindahkan player
            player.transform.position = teleportTarget.position;
            Debug.Log("Player dipindahkan ke posisi target: " + teleportTarget.position);

            // Tunggu 1 frame agar posisi benar-benar terupdate
            yield return null;

            // Enable player kembali
            player.SetActive(true);
            Debug.Log("Player diaktifkan kembali");
        }

        hasTeleported = true;

        if (colliderToEnable != null)
        {
            colliderToEnable.SetActive(true);
        }

        // Delay sebelum mulai fade out
        yield return new WaitForSeconds(delayBeforeFadeOut);

        // Fade Out ke transparan
        yield return StartCoroutine(Fade(1f, 0f, fadeOutTime));

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

            // Raycast aktif hanya saat layar hitam penuh
            fadeImage.raycastTarget = alpha >= 0.95f;
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
