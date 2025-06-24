using UnityEngine;
using UnityEngine.UI;


public class FadeEffect : MonoBehaviour
{
    public enum FadeMode { FadeIn, FadeOut, Blink }
    public enum TargetType { Image, RawImage }

    [Header("Pengaturan Target UI")]
    public TargetType targetType;
    public Image image;
    public RawImage rawImage;

    [Header("Pengaturan Fade")]
    public FadeMode fadeMode = FadeMode.FadeIn;
    [Range(0f, 1f)] public float alpha = 0f;
    public float fadeSpeed = 1f;
    public float blinkSpeed = 2f;

    private float blinkTime;

    void Update()
    {
        // Ambil warna dari target UI
        Color c = GetCurrentColor();

        switch (fadeMode)
        {
            case FadeMode.FadeIn:
                alpha += fadeSpeed * Time.deltaTime;
                break;

            case FadeMode.FadeOut:
                alpha -= fadeSpeed * Time.deltaTime;
                break;

            case FadeMode.Blink:
                blinkTime += Time.deltaTime * blinkSpeed;
                alpha = (Mathf.Sin(blinkTime) + 1) / 2f;
                break;
        }

        alpha = Mathf.Clamp01(alpha); // tetap 0-1
        c.a = alpha;
        ApplyColor(c); // terapkan kembali ke target
    }

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
