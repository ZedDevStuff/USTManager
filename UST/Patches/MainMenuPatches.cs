using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using ULTRAKILL;
using USTManager.Utility;
using System.Linq;

namespace USTManager.Patches
{
    public static class MainMenuPatches
    {
        [HarmonyPatch(typeof(HudOpenEffect), "Awake"), HarmonyPostfix]
        public static void SetActive(HudOpenEffect __instance)
        {
            if(__instance.name != "Audio Options") return;
            if(__instance.transform.GetChild(1).transform.Find("MenuEntry(Clone)") == null)
            {
                RectTransform parent = __instance.transform.GetChild(1).GetComponent<RectTransform>();
                GameObject obj = GameObject.Instantiate(Plugin.MenuEntryPrefab, parent);
                RectTransform button = obj.transform.GetChild(1).GetComponent<RectTransform>();
                button.offsetMax = new Vector2(380, 20);
                button.offsetMin = new Vector2(220, -20);
                Button btn = button.GetComponent<Button>();
                btn.onClick.AddListener(() => Plugin.OpenMenu(btn.transform.parent.parent.parent));
                Logging.Log("Added USTManager button to main menu");
            }
        }
    }
}