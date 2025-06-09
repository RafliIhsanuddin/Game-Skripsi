using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightController : MonoBehaviour
{
    [SerializeField] private Light2D targetLight; // Komponen Light2D yang akan diubah
    [SerializeField] private float leftIntensity = 0.75f; // Intensitas target untuk area left
    [SerializeField] private float rightIntensity = 0.12f; // Intensitas target untuk area right
    [SerializeField] private float intensityChangeSpeed = 1f; // Kecepatan perubahan intensitas
    [SerializeField] private TriggerHandler leftTrigger; // Referensi ke trigger left
    [SerializeField] private TriggerHandler rightTrigger; // Referensi ke trigger right

    private float currentTargetIntensity; // Intensitas target saat ini
    private bool isLerping = false; // Apakah sedang transisi intensitas

    private void Start()
    {
        // Sambungkan event ke metode
        if (leftTrigger != null)
        {
            leftTrigger.OnPlayerEnter += () => SetTargetIntensity(leftIntensity, true);
        }
        if (rightTrigger != null)
        {
            rightTrigger.OnPlayerEnter += () => SetTargetIntensity(rightIntensity, true);
        }
    }

    private void Update()
    {
        // Jika sedang melakukan transisi, ubah intensitas secara perlahan
        if (isLerping && targetLight != null)
        {
            targetLight.intensity = Mathf.MoveTowards(targetLight.intensity, currentTargetIntensity, intensityChangeSpeed * Time.deltaTime);

            // Jika intensitas sudah mencapai target, hentikan transisi
            if (Mathf.Approximately(targetLight.intensity, currentTargetIntensity))
            {
                isLerping = false;
            }
        }
    }

    private void SetTargetIntensity(float targetIntensity, bool instant)
    {
        currentTargetIntensity = targetIntensity;

        if (instant && targetLight != null)
        {
            // Ubah langsung ke target intensitas
            targetLight.intensity = targetIntensity;
            isLerping = false;
        }
        else
        {
            // Mulai transisi intensitas
            isLerping = true;
        }
    }
}
