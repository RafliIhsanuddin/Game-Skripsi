using UnityEngine;

public class BlinkingParticles : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public float blinkInterval = 0.5f; // seconds on/off

    private ParticleSystem.EmissionModule emission;
    private float timer = 0f;
    private bool isEmitting = true;

    void Start()
    {
        if (particleSystem == null)
            particleSystem = GetComponent<ParticleSystem>();

        emission = particleSystem.emission;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= blinkInterval)
        {
            isEmitting = !isEmitting;
            emission.enabled = isEmitting;
            timer = 0f;
        }
    }
}
