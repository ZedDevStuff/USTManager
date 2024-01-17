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
using USTManager.Patches;
using USTManager.Commands;
using TMPro;
using UnityEngine.UI;
using USTManager.Misc;

namespace USTManager
{
    [BepInPlugin("zed.uk.ustmanager", "USTManager", "1.2.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static string UKPath, USTDir;
        public static GameObject DebugTextPrefab, MenuEntryPrefab, SelectionScreenPrefab, SelectionScreenEntryPrefab;
        private void Awake()
        {
            instance = this;
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(AudioSourcePatches));
            Harmony.CreateAndPatchAll(typeof(MainMenuPatches));
            
            UKPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            DirectoryInfo ustDir = new(Path.Combine(UKPath,"USTs"));
            USTDir = ustDir.FullName;
            if(!ustDir.Exists) ustDir.Create();
            
            
            AssetBundle bundle = AssetBundle.LoadFromMemory(Resources.Resource1.ust);
            DebugTextPrefab = bundle.LoadAsset<GameObject>("DebugText");
            MenuEntryPrefab = bundle.LoadAsset<GameObject>("MenuEntry");
            MenuEntryPrefab.AddComponent<HudOpenEffect>();
            SelectionScreenEntryPrefab = bundle.LoadAsset<GameObject>("USTEntry");
            USTEntry entry = SelectionScreenEntryPrefab.AddComponent<USTEntry>();
            entry.Image = SelectionScreenEntryPrefab.transform.GetChild(0).GetComponent<Image>();
            entry.Name = SelectionScreenEntryPrefab.transform.GetChild(1).GetComponent<TMP_Text>();
            entry.Author = SelectionScreenEntryPrefab.transform.GetChild(2).GetComponent<TMP_Text>();
            entry.IconButton = SelectionScreenEntryPrefab.transform.GetChild(0).GetComponent<Button>();
            entry.Status = SelectionScreenEntryPrefab.transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
            SelectionScreenPrefab = bundle.LoadAsset<GameObject>("OptionMenu");
            SelectionScreenPrefab.AddComponent<USTSelectionScreen>();
            Console.Instance.RegisterCommand(new USTToggle());
            Console.Instance.RegisterCommand(new USTDebug());
            CreateTemplate();
            
        }
        public static USTSelectionScreen Screen;
        public static void OpenMenu(Transform transform)
        {
            if(Screen == null)
            {
                Screen = GameObject.Instantiate(SelectionScreenPrefab, transform).GetComponent<USTSelectionScreen>();
                Screen.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            }
            else
            {
                Screen.gameObject.SetActive(true);
            }
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
            File.WriteAllText(Path.Combine(UKPath,"USTs","Template","readme.md"),StaticResources.Readme);
        }
        
    }
}
