using UnityEngine;
using UnityEngine.UI;

public class OverlayWithAnimationPicture : MonoBehaviour
{
    [Header("References")]
    public Image blueOverlay;
    public GameObject animationGO;
    public VignetteBlack vignetteBlack;

    private float currentFadeOutTotal = 0f;

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
    private float extraIdleTime = 0f;
    private bool fadeInComplete = false;

    void Update()
    {
        if (!enableOverlayEffect) return;

        float vignetteAlpha = (vignetteBlack != null) ? vignetteBlack.alpha : 0f;

        // Block BlueOverlay fade-in if Vignette is not zero
        if (vignetteAlpha != 0f)
        {
            // Fade out BlueOverlay
            if (alpha > 0f) FadeOutSmooth();
            ApplyAlpha();
            return;
        }

        // Vignette is 0, BlueOverlay can fade-in
        if (IsIdle)
        {
            // Fade-in toward 1 if alpha < 1
            if (alpha < 1f)
            {
                alpha += Time.deltaTime / fadeInDuration;
                if (alpha >= 1f)
                {
                    alpha = 1f;
                    fadeInComplete = true;
                    idleTimeWhenFull = 0f;
                    extraIdleTime = 0f;
                    if (animationGO) animationGO.SetActive(true);
                    Debug.Log("BlueOverlay reached 1");
                }
                else
                {
                    //Debug.Log("BlueOverlay fading in toward 1");
                }
            }
            else
            {
                // Already full alpha
                idleTimeWhenFull += Time.deltaTime;
                if (idleTimeWhenFull > fadeInDuration)
                {
                    extraIdleTime += Time.deltaTime;
                    extraIdleTime = Mathf.Clamp(extraIdleTime, 0f, extraIdleMax);
                }
            }
        }
        else
        {
            // Fade-out toward 0 if moving
            if (alpha > 0f)
            {
                FadeOutSmooth();
                //Debug.Log("BlueOverlay fading out toward 0");
            }
        }

        ApplyAlpha();
    }

    void FadeOutSmooth()
    {
        if (animationGO) animationGO.SetActive(false);

        currentFadeOutTotal = fadeOutDuration + extraIdleTime + extraFadeOutDuration;
        alpha -= Time.deltaTime / currentFadeOutTotal;

        Debug.Log($"[FadeOut] Total fade out duration: {currentFadeOutTotal:F2} = base {fadeOutDuration:F2} + extraIdle {extraIdleTime:F2} + extraManual {extraFadeOutDuration:F2}");


        if (alpha <= 0f)
        {
            alpha = 0f;
            fadeInComplete = false;
            idleTimeWhenFull = 0f;
            extraIdleTime = 0f;
            extraFadeOutDuration = 0f;
            Debug.Log("BlueOverlay reached 0 and reset");
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
