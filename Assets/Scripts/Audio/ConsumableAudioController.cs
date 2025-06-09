using UnityEngine;

public class ConsumableAudioController : MonoBehaviour
{
    /// <value>
    /// Property <c>_audioSource</c> represents the audio source of the game object.
    /// </value>
    private AudioSource _audioSource;

    /// <summary>
    /// Get the audio source of the game object.
    /// </summary>
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Destroy the game object when the sound is done playing,
    /// unless the game is paused.
    /// </summary>
    void Update()
    {
        if (_audioSource.isPlaying) return;

        Destroy(gameObject);
    }
}
