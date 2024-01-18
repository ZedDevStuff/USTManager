using HarmonyLib;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Animations;
using USTManager.Utility;

namespace USTManager.Patches
{
    public class AudioSourcePatches
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

        [HarmonyPatch(typeof(AudioSource), "Play", [typeof(double)]), HarmonyPrefix]
        public static void Play(AudioSource __instance, double delay)
        {
            if(!Manager.IsEnabled) return;
            if(Manager.IsDebug)
            {
                Logging.Log($"Playing {__instance.clip.name} in {SceneHelper.CurrentScene}");
                if(__instance.clip != null)
                {
                    if(__instance.name.Contains("Theme") || __instance.name.Contains("Music"))
                    {
                        // TODO: Add text to screen instead of world
                    }
                    else
                    {
                        GameObject obj = GameObject.Instantiate(Plugin.DebugTextPrefab, __instance.transform.position, Quaternion.identity);
                        obj.transform.localScale /= 2;
                        obj.GetComponent<TMP_Text>().text = __instance.clip.name;
                        var constraint = obj.GetComponent<RotationConstraint>();
                        constraint.AddSource(new ConstraintSource() { sourceTransform = CameraController.Instance.transform, weight = 1 });
                        constraint.constraintActive = true;
                        Plugin.RunCoroutine(DestroyAfter(obj, 2f));
                    }

                }
            }
            Manager.HandleAudio(SceneHelper.CurrentScene, __instance, null);
        }
        [HarmonyPatch(typeof(AudioSource), "Play", []), HarmonyPrefix]
        public static void Play(AudioSource __instance)
        {
            if(!Manager.IsEnabled) return;
            if(Manager.IsDebug)
            {
                Logging.Log($"Playing {__instance.clip.name} in {SceneHelper.CurrentScene}");
                if(__instance.clip != null)
                {
                    if(__instance.name.Contains("Theme") || __instance.name.Contains("Music"))
                    {
                        // TODO: Add text to screen instead of world
                    }
                    else
                    {
                        GameObject obj = GameObject.Instantiate(Plugin.DebugTextPrefab, __instance.transform.position, Quaternion.identity);
                        obj.transform.localScale /= 2;
                        obj.GetComponent<TMP_Text>().text = __instance.clip.name;
                        var constraint = obj.GetComponent<RotationConstraint>();
                        constraint.AddSource(new ConstraintSource() { sourceTransform = CameraController.Instance.transform, weight = 1 });
                        constraint.constraintActive = true;
                        Plugin.RunCoroutine(DestroyAfter(obj, 2f));
                    }

                }
            }
            Manager.HandleAudio(SceneHelper.CurrentScene, __instance, null);
        }

        [HarmonyPatch(typeof(AudioSource), "clip", MethodType.Setter), HarmonyPrefix]
        public static void set_clip(AudioSource __instance, AudioClip value)
        {
            if(!Manager.IsEnabled) return true;
            Manager.HandleAudio(SceneHelper.CurrentScene, __instance, value);
            //Logging.Log(__instance.name + ": set_clip: " + value??"null");
        }
        [HarmonyPatch(typeof(GameObject), "SetActive"), HarmonyPrefix]
        public static void SetActive(GameObject __instance, bool value)
        {
            if(!Manager.IsEnabled) return;
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
                Manager.HandleAudio(SceneHelper.CurrentScene, source, null);
            }
        }
        [HarmonyPatch(typeof(Crossfade), "Awake"), HarmonyPrefix]
        public static void Awake(Crossfade __instance)
        {
            if(!Manager.IsEnabled) return;
            if(__instance.TryGetComponent<AudioSource>(out AudioSource source))
            {
                if(Manager.IsDebug) Logging.Log($"Crossfade: {source.clip.name} in {SceneHelper.CurrentScene}");
                Manager.HandleAudio(SceneHelper.CurrentScene, source, null);
                if(source.playOnAwake) source.Play();
            }
        }
        [HarmonyPatch(typeof(MusicChanger), "Change"), HarmonyPrefix]
        public static bool Change(MusicChanger __instance)
        {
            if(!Manager.IsEnabled) return true;
            return Manager.HandleAudio(SceneHelper.CurrentScene, __instance);
        }
        public static void HandleInstantiate(GameObject obj)
        {
            Logging.Log($"Instantiating {obj.name} in {SceneHelper.CurrentScene}");
            if(!Manager.IsEnabled) return;
            AudioSource source = obj.GetComponentInChildren<AudioSource>();
            if(source != null)
            {
                Logging.Log($"Instantiating {source.clip.name} in {SceneHelper.CurrentScene}");
                Manager.HandleAudio(SceneHelper.CurrentScene, source, null);
            }
        }

        public static IEnumerator DestroyAfter(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            GameObject.Destroy(obj);
        }
    }
}
