using UnityEngine;

public class FadeAndDestroyOnTrigger : MonoBehaviour
{
    [SerializeField] private GameObject targetObject; // GameObject yang punya SpriteRenderer
    [SerializeField] private float fadeDuration = 1f; // Durasi transisi alpha dari 1 ke 0

    private SpriteRenderer spriteRenderer;
    private bool isFading = false;
    private float fadeTimer = 0f;
    private float initialAlpha = 1f;

    void Start()
    {
        if (targetObject != null)
        {
            spriteRenderer = targetObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogError("SpriteRenderer tidak ditemukan di GameObject target.");
            }
            else
            {
                initialAlpha = spriteRenderer.color.a;
            }
        }
        else
        {
            Debug.LogError("Target Object belum di-assign.");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Masuk trigger dengan: " + other.gameObject.name);

        if (other.gameObject.name == "Kael FBF" && !isFading && spriteRenderer != null)
        {
            isFading = true;
            fadeTimer = 0f;
        }
    }

    void Update()
    {
        if (isFading && spriteRenderer != null)
        {
            fadeTimer += Time.deltaTime;
            float t = Mathf.Clamp01(fadeTimer / fadeDuration);
            float alpha = Mathf.Lerp(initialAlpha, 0f, t);

            Color newColor = spriteRenderer.color;
            newColor.a = alpha;
            spriteRenderer.color = newColor;

            if (t >= 1f)
            {
                // Nonaktifkan targetObject dan gameObject ini
                targetObject.SetActive(false);
                gameObject.SetActive(false);
                isFading = false;
            }
        }
    }
}
