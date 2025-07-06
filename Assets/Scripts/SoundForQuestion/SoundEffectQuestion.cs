using UnityEngine;
using Yarn.Unity;

public class SoundEffectQuestion : MonoBehaviour
{
    [SerializeField] private AudioClip suaraBenar;
    [SerializeField] private AudioClip suaraSalah;

    private AudioSource audioSource;

    private void Awake()
    {
        // Tambahkan AudioSource secara otomatis jika belum ada
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    [YarnCommand("SuaraBenar")]
    public void PlayBenar()
    {
        if (suaraBenar != null)
        {
            audioSource.PlayOneShot(suaraBenar);
        }
        else
        {
            Debug.LogWarning("Suara Benar belum diisi di Inspector!");
        }
    }

    [YarnCommand("SuaraSalah")]
    public void PlaySalah()
    {
        if (suaraSalah != null)
        {
            audioSource.PlayOneShot(suaraSalah);
        }
        else
        {
            Debug.LogWarning("Suara Salah belum diisi di Inspector!");
        }
    }
}
