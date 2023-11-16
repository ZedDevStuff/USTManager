using Logic;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace USTManager
{
    public class USTMusicManager : MonoBehaviour
    {
        public static bool isEnabled = true;
        public static bool off = false;
        public static AudioMixerGroup mixerGroup;
        public float targetVolume = 1f, volume = 1f;

        public List<AudioSource> themes = new List<AudioSource>();

        public void FadeAudio(AudioSource source)
        {
            
        }

        void Update()
        {
            // Fade audio
            /*if (!off && targetVolume != volume)
            {
                foreach (AudioSource audioSource in themes)
                {
                    if (audioSource == this.targetTheme)
                    {
                        if (audioSource.volume > volume)
                        {
                            audioSource.volume = volume;
                        }
                        if (Time.timeScale == 0f)
                        {
                            audioSource.volume = volume;
                        }
                        else
                        {
                            audioSource.volume = Mathf.MoveTowards(audioSource.volume, this.volume, this.fadeSpeed * Time.deltaTime);
                        }
                    }
                    else if (Time.timeScale == 0f)
                    {
                        audioSource.volume = 0f;
                    }
                    else
                    {
                        audioSource.volume = Mathf.MoveTowards(audioSource.volume, 0f, this.fadeSpeed * Time.deltaTime);
                    }
                }
            }*/
        }
    }
}
