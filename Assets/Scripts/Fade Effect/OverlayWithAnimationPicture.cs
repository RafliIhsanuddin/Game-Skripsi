using UnityEngine;
using UnityEngine.UI;

public class OverlayWithAnimationPicture : MonoBehaviour
{
    [Header("References")]
    public Image blueOverlay;
    public GameObject animationGO;
    public VignetteBlack vignetteBlack;

    [Header("Settings")]
    [Range(0f, 1f)] public float alpha = 0f;
    public bool enableOverlayEffect = true;

    [Header("Fade Durations")]
    public float fadeInDuration = 5f;
    public float fadeOutDuration = 5f;
    public float extraIdleMax = 5f;
    public float extraFadeOutDuration = 0f;

    public bool IsIdle { get; set; } = false;

    private float idleTimeWhenFull = 0f;
    private float totalExtraIdle = 0f;
    private bool fadeInComplete = false;

    void Update()
    {
        if (!enableOverlayEffect) return;

        HandleFade();
    }

    void HandleFade()
    {
        // 1) Jika VignetteBlack belum selesai fade (alpha != 0), tetap lanjutkan fade-out bila perlu
        if (vignetteBlack != null && vignetteBlack.alpha.ToString("G9") != "0")
        {
            // Player bergerak, lakukan fade out bila perlu
            if (!IsIdle && alpha > 0f)
            {
                FadeOutSmooth();
            }
            ApplyAlpha();
            return; // skip fade-in logic
        }

        // 2) Kalau Vignette sudah selesai (alpha == 0)
        if (IsIdle)
        {
            // Fade In
            if (!fadeInComplete)
            {
                alpha += Time.deltaTime / fadeInDuration;
                if (alpha >= 1f)
                {
                    alpha = 1f;
                    fadeInComplete = true;
                    idleTimeWhenFull = 0f;
                    totalExtraIdle = 0f;
                    animationGO?.SetActive(true);
                }
            }
            else
            {
                // Tambah extra idle time
                idleTimeWhenFull += Time.deltaTime;
                if (idleTimeWhenFull > fadeInDuration)
                {
                    totalExtraIdle += Time.deltaTime;
                    totalExtraIdle = Mathf.Clamp(totalExtraIdle, 0f, extraIdleMax);
                }
            }
        }
        else
        {
            // Fade Out bila player bergerak
            if (fadeInComplete || alpha > 0f) FadeOutSmooth();
        }

        ApplyAlpha();
    }

    void FadeOutSmooth()
    {
        if (animationGO != null) animationGO.SetActive(false); // Matikan anim
        float fadeOutTotal = fadeOutDuration + totalExtraIdle + extraFadeOutDuration;
        alpha -= Time.deltaTime / fadeOutTotal;

        if (alpha <= 0f)
        {
            alpha = 0f;
            fadeInComplete = false;
            totalExtraIdle = 0f;
            idleTimeWhenFull = 0f;
            extraFadeOutDuration = 0f;
        }
    }

    void ApplyAlpha()
    {
        alpha = Mathf.Clamp01(alpha);
        if (blueOverlay != null)
        {
            var c = blueOverlay.color;
            blueOverlay.color = new Color(c.r, c.g, c.b, alpha);
        }
    }
}
