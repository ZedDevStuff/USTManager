using BepInEx;
using System.IO;
using UnityEngine;
using HarmonyLib;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using Newtonsoft.Json;
using BepInEx.Configuration;

namespace USTManager;

[BepInPlugin("dev.zeddevstuff.ustmanager", "USTManager", "2.0.0")]
public class Plugin : BaseUnityPlugin
{
    public static string UKPath = "", USTDir = "", LastUSTs = "";
    public ConfigEntry<bool>? UpdateBasedSwappingEnabled;
    public static GameObject? MenuEntryPrefab, SelectionScreenPrefab, SelectionScreenEntryPrefab, ConflictEntryPrefab, ConflictResolutionScreenPrefab, ToastPrefab;
    private void Awake()
    {
        UpdateBasedSwappingEnabled = Config.Bind(
            "Experimental",
            "UpdateBasedSwappingEnabled",
            false,
            "Enable this to use Update based swapping on top of method patching. This should allow most sounds to be swapped. This method will probably hurt performance.");
        //Harmony.CreateAndPatchAll(typeof(AudioSourcePatches));
        //Harmony.CreateAndPatchAll(typeof(MainMenuPatches));
        UKPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
        LastUSTs = Path.Combine(UKPath, "USTs", "lastUSTs.json");
        if(File.Exists(LastUSTs))
        {
            
        }
        DirectoryInfo ustDir = new(Path.Combine(UKPath, "USTs"));
        USTDir = ustDir.FullName;
        if(!ustDir.Exists) ustDir.Create();

        Harmony.CreateAndPatchAll(typeof()

    }

}
