using HarmonyLib;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.Animations;

namespace USTManager.Patches
{
    public class AudioSourcePatches
    {
        [HarmonyPatch(typeof(AudioSource), "Play", []), HarmonyPrefix]
        public static bool Play(AudioSource __instance)
        {
            //Debug.Log($"Playing {__instance.clip.name} in {SceneHelper.CurrentScene}");
            if (!Manager.IsEnabled) return true;
            return Manager.HandleAudio(SceneHelper.CurrentScene, __instance, null);
            /*if(Manager.IsDebug)
            {
                if(__instance.clip != null)
                {
                    GameObject obj = GameObject.Instantiate(Plugin.DebugTextPrefab, __instance.transform.position, Quaternion.identity);
                    obj.transform.localScale /= 2;
                    obj.GetComponent<TMP_Text>().text = __instance.clip.name;
                    var constraint = obj.GetComponent<RotationConstraint>();
                    constraint.AddSource(new ConstraintSource(){sourceTransform = CameraController.Instance.transform, weight = 1});
                    constraint.constraintActive = true;
                    Plugin.RunCoroutine(DestroyAfter(obj, 2f));
                }
            }*/
        }

        [HarmonyPatch(typeof(AudioSource), "clip", MethodType.Setter), HarmonyPrefix]
        public static bool set_clip(AudioSource __instance, AudioClip value)
        {
            if(!Manager.IsEnabled) return true;
            return Manager.HandleAudio(SceneHelper.CurrentScene, __instance, value);
            //Debug.Log(__instance.name + ": set_clip: " + value??"null");
        }
        public static AudioClip NullClip = null;
        [HarmonyPatch(typeof(GameObject), "SetActive"), HarmonyPrefix]
        public static bool SetActive(GameObject __instance, bool value)
        {
            if(!Manager.IsEnabled) return true;
            if(__instance.TryGetComponent<AudioSource>(out AudioSource source))
            {
                return Manager.HandleAudio(SceneHelper.CurrentScene, source, NullClip);
            }
            return true;
        }

        public static IEnumerator DestroyAfter(GameObject obj, float time)
        {
            yield return new WaitForSeconds(time);
            GameObject.Destroy(obj);
        }
    }
}
