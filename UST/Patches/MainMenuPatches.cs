using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using ULTRAKILL;
using USTManager.Utility;
using System.Linq;
using GameConsole;
using USTManager.Commands;

namespace USTManager.Patches
{
    public static class MainMenuPatches
    {
        [HarmonyPatch(typeof(HudOpenEffect), "Awake"), HarmonyPostfix]
        public static void SetActive(HudOpenEffect __instance)
        {
            if(__instance.name != "Audio" || __instance.transform.parent == null) return;
            //Logging.Log(__instance.transform.GetPath());
            if(__instance.transform.GetChild(0).transform.name == "Container")
            {
                RectTransform parent = __instance.transform.GetChild(0).GetComponent<RectTransform>();
                GameObject obj = GameObject.Instantiate(Plugin.MenuEntryPrefab, parent);
                RectTransform button = obj.transform.Find("Button").GetComponent<RectTransform>();
                button.offsetMax = new Vector2(380, 20);
                button.offsetMin = new Vector2(220, -20);
                Button btn = button.GetComponent<Button>();
                btn.onClick.AddListener(() => Plugin.OpenMenu(btn.transform.parent.parent.parent));
                Logging.Log("Added USTManager button to main menu");
            }
        }
        [HarmonyPatch(typeof(Console), "Awake"), HarmonyPostfix]
        public static void Awake(Console __instance)
        {
            if(__instance != null && __instance == Console.Instance)
            {
                if(__instance.registeredCommandTypes.Contains(typeof(USTDebug))) return;
                __instance.RegisterCommand(new USTToggle());
                __instance.RegisterCommand(new USTDebug());
            }
        }
    }
    
}