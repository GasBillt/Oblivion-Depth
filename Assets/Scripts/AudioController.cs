using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SoundController : MonoBehaviour
{
    public bool isAnimationOnProgress = true;
    public AudioSource exceptionSound; 
    private List<AudioSource> allAudioSources = new List<AudioSource>();

    void Update()
    {
        if (isAnimationOnProgress)
        {
            AudioSource[] audioSources = FindObjectsByType<AudioSource>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource != exceptionSound)
                {
                    if (audioSource.isPlaying)
                    {
                        audioSource.Pause();
                        if (!allAudioSources.Contains(audioSource))
                        {
                            allAudioSources.Add(audioSource);
                        }
                    }
                }
                else
                {
                    if (!audioSource.isPlaying)
                    {
                        audioSource.Play();
                    }
                }
            }
        }
        else
        {
            foreach (AudioSource audioSource in allAudioSources)
            {
                if (audioSource != null && !audioSource.isPlaying)
                {
                    audioSource.UnPause();
                }
            }
            allAudioSources.Clear();
        }
    }
}