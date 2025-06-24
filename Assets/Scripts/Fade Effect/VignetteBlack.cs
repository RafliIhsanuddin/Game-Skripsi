using UnityEngine;
using UnityEngine.UI;

public class VignetteBlack : MonoBehaviour
{
    public enum FadeMode { FadeInOnly, FadeOutOnly, BlinkFixed, BlinkRandom }
    public enum TargetType { Image, RawImage }

    [Header("Pengaturan Target UI")]
    public TargetType targetType;
    public Image image;
    public RawImage rawImage;

    [Header("Pengaturan Umum")]
    [Range(0f, 1f)] public float alpha = 0f;
    public FadeMode fadeMode;

    // ==================== FADE IN ONLY ====================
    [Header("Pengaturan Fade In Only")]
    public float fadeInDuration = 0.5f;

    // ==================== FADE OUT ONLY ===================
    [Header("Pengaturan Fade Out Only")]
    public float fadeOutDuration = 0.5f;

    // ==================== BLINK FIXED =====================
    [Header("Pengaturan Blink Fixed")]
    public float fixedFadeInDuration = 0.5f;
    public float fixedFadeOutDuration = 0.5f;
    public float fixedHoldInDuration = 0.2f;
    public float fixedHoldOutDuration = 0.2f;

    // ==================== BLINK RANDOM ====================
    [Header("Pengaturan Blink Random")]
    public float minFadeInDuration = 0.2f;
    public float maxFadeInDuration = 1.0f;
    public float minFadeOutDuration = 0.2f;
    public float maxFadeOutDuration = 1.0f;
    public float minHoldInDuration = 0.1f;
    public float maxHoldInDuration = 1.0f;
    public float minHoldOutDuration = 0.1f;
    public float maxHoldOutDuration = 1.0f;

    // ==================== IDLE FADE OUT ===================
    [Header("Pengaturan Idle Fade Out")]
    public float idleFadeOutDuration = 0.5f;
    private bool isPlayerIdle = false;
    private float preIdleAlpha = 0f;
    private bool needsRestore = false;

    [Header("Sync with Overlay")]
    public OverlayWithAnimationPicture overlayScript; // Reference to overlay script

    private float timer;
    private float targetDuration;
    private bool fadingIn = true;
    private float holdTime;

    public void SetPlayerIdle(bool idle)
    {
        if (isPlayerIdle != idle)
        {
            if (idle)
            {
                preIdleAlpha = alpha;
                needsRestore = true;
            }
            else
            {
                if (needsRestore)
                {
                    alpha = preIdleAlpha;
                    needsRestore = false;
                }
            }
            isPlayerIdle = idle;
        }
    }

    void Start()
    {
        ResetBlinkRandom();
    }

    void Update()
    {
        Color c = GetCurrentColor();

        // Check overlay alpha before allowing Vignette to activate
        if (overlayScript != null)
        {
            string alphaString = overlayScript.alpha.ToString("G9"); // must be exactly "0"
            if (alphaString != "0")
            {
                // Force alpha to 0 and skip all behaviors
                alpha = 0f;
                c.a = alpha;
                ApplyColor(c);
                return;
            }
        }

        // Handle idle fade out
        if (isPlayerIdle && alpha > 0f)
        {
            alpha = Mathf.MoveTowards(alpha, 0f, Time.deltaTime / idleFadeOutDuration);
            c.a = alpha;
            ApplyColor(c);
            return;
        }

        // Continue with normal fade modes if not idle
        switch (fadeMode)
        {
            case FadeMode.FadeInOnly:
                FadeInOnly();
                break;
            case FadeMode.FadeOutOnly:
                FadeOutOnly();
                break;
            case FadeMode.BlinkFixed:
                BlinkFixed();
                break;
            case FadeMode.BlinkRandom:
                BlinkRandom();
                break;
        }

        c.a = alpha;
        ApplyColor(c);
    }

    // ==================== MODE IMPLEMENTATIONS ====================

    void FadeInOnly()
    {
        alpha += Time.deltaTime / fadeInDuration;
        alpha = Mathf.Clamp01(alpha);
    }

    void FadeOutOnly()
    {
        alpha -= Time.deltaTime / fadeOutDuration;
        alpha = Mathf.Clamp01(alpha);
    }

    void BlinkFixed()
    {
        timer += Time.deltaTime;

        if (fadingIn)
        {
            alpha += Time.deltaTime / fixedFadeInDuration;
            if (alpha >= 1f)
            {
                alpha = 1f;
                if (timer >= fixedFadeInDuration + fixedHoldInDuration)
                {
                    timer = 0f;
                    fadingIn = false;
                }
            }
        }
        else
        {
            alpha -= Time.deltaTime / fixedFadeOutDuration;
            if (alpha <= 0f)
            {
                alpha = 0f;
                if (timer >= fixedFadeOutDuration + fixedHoldOutDuration)
                {
                    timer = 0f;
                    fadingIn = true;
                }
            }
        }
    }

    void BlinkRandom()
    {
        timer += Time.deltaTime;

        if (fadingIn)
        {
            alpha += Time.deltaTime / targetDuration;
            if (alpha >= 1f)
            {
                alpha = 1f;
                if (timer >= targetDuration + holdTime)
                {
                    timer = 0f;
                    fadingIn = false;
                    SetNextRandom(false);
                }
            }
        }
        else
        {
            alpha -= Time.deltaTime / targetDuration;
            if (alpha <= 0f)
            {
                alpha = 0f;
                if (timer >= targetDuration + holdTime)
                {
                    timer = 0f;
                    fadingIn = true;
                    SetNextRandom(true);
                }
            }
        }
    }

    // ==================== RANDOM HELPERS ====================

    void ResetBlinkRandom() => SetNextRandom(true);

    void SetNextRandom(bool nextIsFadeIn)
    {
        if (nextIsFadeIn)
        {
            targetDuration = Random.Range(minFadeInDuration, maxFadeInDuration);
            holdTime = Random.Range(minHoldInDuration, maxHoldInDuration);
        }
        else
        {
            targetDuration = Random.Range(minFadeOutDuration, maxFadeOutDuration);
            holdTime = Random.Range(minHoldOutDuration, maxHoldOutDuration);
        }
    }

    // ==================== UTILITY ====================

    Color GetCurrentColor()
    {
        if (targetType == TargetType.Image && image != null) return image.color;
        if (targetType == TargetType.RawImage && rawImage != null) return rawImage.color;
        return Color.white;
    }

    void ApplyColor(Color color)
    {
        if (targetType == TargetType.Image && image != null) image.color = color;
        if (targetType == TargetType.RawImage && rawImage != null) rawImage.color = color;
    }
}
