using BepInEx;
using UnityEngine.SceneManagement;
using Steamworks;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using UnityEngine;
using HarmonyLib;
using ULTRAKILL;
using GameConsole;

namespace USTManager
{
    [BepInPlugin("zed.uk.ustmanager", "USTManager", "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private Harmony console;
        public static string UKPath;
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            UKPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            Harmony.CreateAndPatchAll(typeof(MusicManagerPatches));
            GameConsole.Console.Instance.RegisterCommand(new USTToggle());
        }
    }
}
