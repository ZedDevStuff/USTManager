using HarmonyLib;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Animations;
using USTManager.Utility;
using System;
using System.Linq;
using GameConsole.pcon;
using System.Collections.Generic;
using USTManager.Preprocessor;

namespace USTManager.Patches
{
    public static class AudioSourcePatches
    {
        [HarmonyPatch(typeof(StatsManager), "Awake"), HarmonyPostfix]
        public static void StatsManagerAwake(StatsManager __instance)
        {
            if(!Manager.IsEnabled) return;
            /*List<AudioClip> clips = UnityEngine.Resources.FindObjectsOfTypeAll<AudioClip>().ToList();
            clips.ForEach(x => Extension.Registry.BrandClip(x));*/
            List<AudioSource> sources = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(GameObject)).Where(x => (x as GameObject).GetComponentInChildren<AudioSource>()).SelectMany(x => (x as GameObject).GetComponentsInChildren<AudioSource>()).ToList();
            foreach(BasePreprocessor preprocessor in Registry.Preprocessors)
            {
                preprocessor.Apply(sources);
            }
            foreach(AudioSource source in sources)
            {
                if(source.clip != null)
                {
                    if(source.gameObject.GetComponent<USTTarget>() != null) source.gameObject.AddComponent<USTTarget>();
                }
            }
        }

        // I have no idea if that is the cracking issue cause so it'll remain commented out until i find out 
        /*[HarmonyPatch(typeof(USTTarget), "Awake")]
        [HarmonyPrefix]
        public static void USTTargetAwake(USTTarget __instance)
        {
            if(!Manager.IsEnabled) return;
            AudioSource[] sources = __instance.GetComponentsInChildren<AudioSource>();
            foreach(AudioSource source in sources)
            {
                if(source != null && source.clip != null)
                {
                    Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                }
            }
        }*/

        [HarmonyPatch(typeof(AudioSource), "Play", [typeof(double)])]
        [HarmonyPatch(typeof(AudioSource), "Play", [])]
        [HarmonyPrefix]
        public static void Play(AudioSource __instance)
        {
            Manager.HandleAudioSource(SceneHelper.CurrentScene, __instance);
        }

        [HarmonyPatch(typeof(GameObject), "SetActive"), HarmonyPrefix]
        public static void SetActive(GameObject __instance, bool value)
        {
            if(SceneHelper.CurrentScene == "Level 5-4" && __instance.name == "Music")
            {
                foreach(Transform child in __instance.transform)
                {
                    child.gameObject.SetActive(false);
                    child.gameObject.SetActive(true);
                }
            }
            else if(__instance.TryGetComponent<AudioSource>(out AudioSource source))
            {
                Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
            }
        }
        [HarmonyPatch(typeof(Crossfade), "Awake"), HarmonyPrefix]
        public static void Awake(Crossfade __instance)
        {
            if(__instance.TryGetComponent<AudioSource>(out AudioSource source))
            {
                Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                if(source.playOnAwake) source.Play();
            }
        }
        [HarmonyPatch(typeof(MusicChanger), "Change"), HarmonyPrefix]
        public static bool Change(MusicChanger __instance)
        {
            return Manager.HandleMusicChanger(SceneHelper.CurrentScene, __instance);
        }

        /*public static void HandleInstantiate(GameObject obj)
        {
            Logging.Log($"Instantiating {obj.name} in {SceneHelper.CurrentScene}");
            AudioSource source = obj.GetComponentInChildren<AudioSource>();
            if(source != null)
            {
                Logging.Log($"Instantiating {source.clip.name} in {SceneHelper.CurrentScene}");
                Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
            }
        }*/

        public static IEnumerator DestroyAfter(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            GameObject.Destroy(obj);
        }
    }
}
