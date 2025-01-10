using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AudioManager : MonoBehaviour
{


    [SerializeField]
    private AudioSource bgmAudioSource;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.loop = true; // Set the BGM to loop
        }
        PlayBGM();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    

    /// <summary>
    /// Plays the background music if it is not already playing.
    /// </summary>
    public void PlayBGM()
    {
        if (bgmAudioSource == null)
        {
            Debug.LogWarning("BGM AudioSource is not assigned.");
            return;
        }

        if (!bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Play();
        }
    }

    /// <summary>
    /// Instantiates the specified audio source prefab, plays it at the given location, 
    /// and tracks it in the list of played sounds.
    /// </summary>
    /// <param name="audioSourcePrefab">The prefab of the audio source.</param>
    /// <param name="location">The position to spawn the audio source.</param>
    /// <returns>The length of the audio clip, or 0 if an error occurs.</returns>
    
}
