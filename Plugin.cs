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
using System.Linq;
using Newtonsoft.Json;
using USTManager.Data;
using USTManager.Utility;
using USTManager.Patches;
using USTManager.Commands;
using TMPro;

namespace USTManager
{
    [BepInPlugin("zed.uk.ustmanager", "USTManager", "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static string UKPath;
        public static GameObject DebugTextPrefab;
        private void Awake()
        {
            instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(AudioSourcePatches));
            UKPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            DirectoryInfo ustDir = new(Path.Combine(UKPath,"USTs"));
            if(!ustDir.Exists) ustDir.Create();
            FileInfo[] files = Directory.GetFiles(Path.Combine(UKPath,"USTs"), "*.ust", SearchOption.AllDirectories).Select(x => new FileInfo(x)).ToArray();
            foreach(FileInfo file in files)
            {
                if(file.Name == "template.ust") continue;
                CustomUST ust = JsonConvert.DeserializeObject<CustomUST>(File.ReadAllText(file.FullName));
                if(ust != null)
                {
                    ust.Path = file.Directory.FullName;
                    Logger.LogInfo($"Loaded UST {ust.Name} by {ust.Author}");
                    Manager.AllUSTs.Add(ust);
                }
            }
            AssetBundle bundle = AssetBundle.LoadFromMemory(Resources.Resource1.ust);
            DebugTextPrefab = bundle.LoadAsset<GameObject>("DebugText");
            Console.Instance.RegisterCommand(new USTToggle());
            Console.Instance.RegisterCommand(new USTDebug());
            if(Manager.AllUSTs.Count > 0)Manager.LoadUST(Manager.AllUSTs.First());
            CreateTemplate();
            
        }
        private static Plugin instance;
        public static void RunCoroutine(System.Collections.IEnumerator routine)
        {
            instance.StartCoroutine(routine);
        }
        public void CreateTemplate()
        {
            Directory.CreateDirectory(Path.Combine(UKPath,"USTs","Template"));
            File.WriteAllText(Path.Combine(UKPath,"USTs","Template","template.ust"), CustomUST.GetTemplateJson());
            File.Create(Path.Combine(UKPath,"USTs","Template","place your tracks here or in subfolders.txt"));
        }
    }
}
