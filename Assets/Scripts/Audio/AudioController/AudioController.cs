using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private Transform player; // Transform dari player
    [SerializeField] private List<AudioSource> audioSources; // Daftar AudioSource
    [SerializeField] private float maxDistance = 20f; // Jarak maksimum untuk suara
    [SerializeField] private float blendDistance = 10f; // Jarak di mana suara maksimal

    void Update()
    {
        foreach (var audioSource in audioSources)
        {
            if (audioSource != null)
            {
                // Hitung jarak antara player dan AudioSource
                float distance = Vector3.Distance(player.position, audioSource.transform.position);

                if (distance > maxDistance)
                {
                    // Jika di luar jarak maksimum, matikan AudioSource
                    if (audioSource.isActiveAndEnabled)
                        audioSource.gameObject.SetActive(false);
                }
                else
                {
                    // Jika dalam jangkauan, aktifkan AudioSource dan atur volume
                    if (!audioSource.gameObject.activeSelf)
                        audioSource.gameObject.SetActive(true);

                    float volume = Mathf.Clamp01(1 - ((distance - blendDistance) / (maxDistance - blendDistance)));
                    audioSource.volume = volume;
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        // Tambahkan visualisasi untuk jarak maksimum di Scene
        if (audioSources != null)
        {
            Gizmos.color = Color.red;
            foreach (var audioSource in audioSources)
            {
                if (audioSource != null)
                {
                    Gizmos.DrawWireSphere(audioSource.transform.position, maxDistance); // Jarak maksimum
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawWireSphere(audioSource.transform.position, blendDistance); // Jarak untuk volume maksimum
                }
            }
        }
    }
}
