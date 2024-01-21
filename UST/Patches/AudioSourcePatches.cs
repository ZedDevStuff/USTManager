using HarmonyLib;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Animations;
using USTManager.Utility;

namespace USTManager.Patches
{
    public static class AudioSourcePatches
    {
        // I don't remember why I made this. it does work but i don't really see much of a point anymore.
        // I'll keep it here for now just in case i ever need it.
        /*[HarmonyPatch(typeof(StatsManager), "Awake"), HarmonyPrefix]
        public static void StatsManagerAwake(StatsManager __instance)
        {
            if(!Manager.IsEnabled) return;
            var data = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(AudioSource));
            foreach(AudioSource source in data)
            {
                if(Manager.IsDebug && source.clip != null && source.playOnAwake && !source.gameObject.activeInHierarchy) Logging.Log($"PlayOnAwake ({source.gameObject.name}): {source.clip.name}");
                if(source.clip != null)
                {
                    Manager.HandleAudio(SceneHelper.CurrentScene, source, null, null);
                }
            }
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AudioSource), "Play", [typeof(double)])]
        [HarmonyPatch(typeof(AudioSource), "Play", [])]
        public static void Play(AudioSource __instance)
        {
            Manager.HandleAudioSource(SceneHelper.CurrentScene, __instance);
        }

        /*
        [HarmonyPatch(typeof(AudioSource), "clip", MethodType.Setter), HarmonyPrefix]
        public static void set_clip(AudioSource __instance, AudioClip value)
        {
            if(!Manager.IsEnabled) return;
            Manager.HandleAudio(SceneHelper.CurrentScene, __instance, value);
            //Logging.Log(__instance.name + ": set_clip: " + value??"null");
        }
        */

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
                if(Manager.IsDebug) Logging.Log($"Crossfade: {source.clip.name} in {SceneHelper.CurrentScene}");
                Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
                if(source.playOnAwake) source.Play();
            }
        }
        [HarmonyPatch(typeof(MusicChanger), "Change"), HarmonyPrefix]
        public static bool Change(MusicChanger __instance)
        {
            return Manager.HandleMusicChanger(SceneHelper.CurrentScene, __instance);
        }
        public static void HandleInstantiate(GameObject obj)
        {
            Logging.Log($"Instantiating {obj.name} in {SceneHelper.CurrentScene}");
            AudioSource source = obj.GetComponentInChildren<AudioSource>();
            if(source != null)
            {
                Logging.Log($"Instantiating {source.clip.name} in {SceneHelper.CurrentScene}");
                Manager.HandleAudioSource(SceneHelper.CurrentScene, source);
            }
        }

        public static IEnumerator DestroyAfter(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            GameObject.Destroy(obj);
        }
    }
}
