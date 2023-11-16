using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace USTManager
{
    public static class DummyMusicManager
    {
        public static void OnEnable()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager OnEnable");
            if(USTMusicManager.mixerGroup is null) USTMusicManager.mixerGroup = MusicManager.Instance.cleanTheme.outputAudioMixerGroup;
            /*if (this.fadeSpeed == 0f)
            {
                this.fadeSpeed = 1f;
            }
            this.allThemes = base.GetComponentsInChildren<AudioSource>();
            this.defaultVolume = this.volume;
            if (!this.off)
            {
                AudioSource[] array = this.allThemes;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].Play();
                }
                this.cleanTheme.volume = this.volume;
                this.targetTheme = this.cleanTheme;
            }
            else
            {
                this.targetTheme = base.GetComponent<AudioSource>();
            }
            if (MonoSingleton<AudioMixerController>.Instance.musicSound)
            {
                MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Unpaused").TransitionTo(0f);
            }*/
        }
        public static void Update()
        {
            /*if (!this.off && this.targetTheme.volume != this.volume)
            {
                foreach (AudioSource audioSource in this.allThemes)
                {
                    if (audioSource == this.targetTheme)
                    {
                        if (audioSource.volume > this.volume)
                        {
                            audioSource.volume = this.volume;
                        }
                        if (Time.timeScale == 0f)
                        {
                            audioSource.volume = this.volume;
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
                if (this.targetTheme.volume == this.volume)
                {
                    foreach (AudioSource audioSource2 in this.allThemes)
                    {
                        if (audioSource2 != this.targetTheme)
                        {
                            audioSource2.volume = 0f;
                        }
                    }
                }
            }
            if (this.filtering)
            {
                float num;
                MonoSingleton<AudioMixerController>.Instance.musicSound.GetFloat("highPassVolume", out num);
                num = Mathf.MoveTowards(num, 0f, 1200f * Time.unscaledDeltaTime);
                MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", num);
                if (num == 0f)
                {
                    this.filtering = false;
                }
            }
            if (this.volume == 0f || (this.off && this.targetTheme.volume > 0f))
            {
                AudioSource[] array = this.allThemes;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].volume -= Time.deltaTime / 5f * this.fadeSpeed;
                }
                if (this.targetTheme.volume <= 0f)
                {
                    array = this.allThemes;
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].volume = 0f;
                    }
                }
            }*/
        }
        public static void ForceStartMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager ForceStartMusic");
            /*this.forcedOff = false;
            this.StartMusic();*/
        }
        public static void StartMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager StartMusic");
            /*if (this.forcedOff)
            {
                return;
            }
            foreach (AudioSource audioSource in this.allThemes)
            {
                if (audioSource.clip != null)
                {
                    audioSource.Play();
                    if (this.off && audioSource.time != 0f)
                    {
                        audioSource.time = 0f;
                    }
                }
            }
            this.off = false;
            this.cleanTheme.volume = this.volume;
            this.targetTheme = this.cleanTheme;*/
        }
        public static void PlayBattleMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager PlayBattleMusic");
            /*if (!this.dontMatch && this.targetTheme != this.battleTheme)
            {
                this.battleTheme.time = this.cleanTheme.time;
            }
            if (this.targetTheme != this.bossTheme)
            {
                this.targetTheme = this.battleTheme;
            }
            this.requestedThemes += 1f;*/
        }
        public static void PlayCleanMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager PlayCleanMusic");
            /*this.requestedThemes -= 1f;
            if (this.requestedThemes <= 0f && !this.arenaMode)
            {
                this.requestedThemes = 0f;
                if (!this.dontMatch && this.targetTheme != this.cleanTheme)
                {
                    this.cleanTheme.time = this.battleTheme.time;
                }
                if (this.battleTheme.volume == this.volume)
                {
                    this.cleanTheme.time = this.battleTheme.time;
                }
                this.targetTheme = this.cleanTheme;
            }*/
        }
        public static void PlayBossMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager PlayBossMusic");
            /*Debug.Log("PlayBossMusic");
            if (this.targetTheme != this.bossTheme)
            {
                this.bossTheme.time = this.cleanTheme.time;
            }
            this.targetTheme = this.bossTheme;*/
        }
        public static void ArenaMusicStart()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager ArenaMusicStart");
            /*if (this.forcedOff)
            {
                return;
            }
            if (this.off)
            {
                foreach (AudioSource audioSource in this.allThemes)
                {
                    if (audioSource.clip != null)
                    {
                        audioSource.Play();
                        if (this.off && audioSource.time != 0f)
                        {
                            audioSource.time = 0f;
                        }
                    }
                }
                this.off = false;
                this.battleTheme.volume = this.volume;
                this.targetTheme = this.battleTheme;
            }
            if (!this.battleTheme.isPlaying)
            {
                AudioSource[] array = this.allThemes;
                for (int i = 0; i < array.Length; i++)
                {
                    array[i].Play();
                }
                this.battleTheme.volume = this.volume;
            }
            if (this.targetTheme != this.bossTheme)
            {
                this.targetTheme = this.battleTheme;
            }
            this.arenaMode = true;*/
        }
        public static void ArenaMusicEnd()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager ArenaMusicEnd");
            /*this.requestedThemes = 0f;
            this.targetTheme = this.cleanTheme;
            this.arenaMode = false;*/
        }
        public static void ForceStopMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager ForceStopMusic");
            /*this.forcedOff = true;
            this.StopMusic();*/
        }
        public static void StopMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager StopMusic");
            /*this.off = true;
            foreach (AudioSource audioSource in this.allThemes)
            {
                audioSource.volume = 0f;
                audioSource.Stop();
            }*/
        }
        public static void FilterMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager FilterMusic");
            /*MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", -80f);
            base.CancelInvoke("RemoveHighPass");
            MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Paused").TransitionTo(0f);
            this.filtering = true;*/
        }
        public static void UnfilterMusic()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager UnfilterMusic");
            /*this.filtering = false;
            MonoSingleton<AudioMixerController>.Instance.musicSound.FindSnapshot("Unpaused").TransitionTo(0.5f);
            base.Invoke("RemoveHighPass", 0.5f);*/
        }

        public static void RemoveHighPass()
        {
            Debug.Log("<color=#ff0000>DummyMusicManager RemoveHighPass");
            MonoSingleton<AudioMixerController>.Instance.musicSound.SetFloat("highPassVolume", -80f);
        }
        public static bool off;
        public static bool dontMatch;
        public static AudioSource battleTheme;
        public static AudioSource cleanTheme;
        public static AudioSource bossTheme;
        public static AudioSource targetTheme;
        public static AudioSource[] allThemes;
        public static float volume = 1f;
        public static float requestedThemes;
        public static bool arenaMode;
        public static float defaultVolume;
        public static float fadeSpeed;
        public static bool forcedOff;
        public static bool filtering;
    }
}
