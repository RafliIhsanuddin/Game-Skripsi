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
    [Range(0f, 1f)] public float alpha = 0f; // bisa diakses skrip lain
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

    private float timer;
    private float targetDuration;
    private bool fadingIn = true;
    private float holdTime;

    void Start()
    {
        ResetBlinkRandom();
    }

    void Update()
    {
        Color c = GetCurrentColor();

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
                    SetNextRandom(false); // set next fade-out
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
                    SetNextRandom(true); // set next fade-in
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

    // ==================== UI COLOR UTILS ====================

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
