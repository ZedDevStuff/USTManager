using BepInEx;
using System.IO;
using UnityEngine;
using HarmonyLib;
using GameConsole;
using USTManager.Data;
using USTManager.Patches;
using USTManager.Commands;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using USTManager.Misc;
using USTManager.Utility;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace USTManager
{
    [BepInPlugin("zed.uk.ustmanager", "USTManager", "1.5.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static string UKPath, USTDir;
        public static GameObject MenuEntryPrefab, SelectionScreenPrefab, SelectionScreenEntryPrefab, ConflictEntryPrefab, ConflictResolutionScreenPrefab, ToastPrefab;
        private void Awake()
        {
            instance = this;
            Application.quitting += () => Manager.SaveUST();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
            Harmony.CreateAndPatchAll(typeof(AudioSourcePatches));
            Harmony.CreateAndPatchAll(typeof(MainMenuPatches));
            UKPath = new DirectoryInfo(Application.dataPath).Parent.FullName;
            string lastUSTs = Path.Combine(UKPath, "USTs","lastUSTs.json");
            if(File.Exists(lastUSTs))
            {
                try
                {
                    Dictionary<string, object>? data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(lastUSTs));
                    if(data != null)
                    {
                        USTSelectionScreen.InternalConfirm((data["Selected"] as JArray).ToObject<List<string>>(), (data["UST"] as JObject).ToObject<CustomUST>());
                    }
                }
                catch(System.Exception e)
                {
                    Debug.LogError(e);
                }
            }
            DirectoryInfo ustDir = new(Path.Combine(UKPath, "USTs"));
            USTDir = ustDir.FullName;
            if(!ustDir.Exists) ustDir.Create();

            AssetBundle bundle = AssetBundle.LoadFromMemory(Resources.Resource1.ust);
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
            ConflictEntryPrefab = bundle.LoadAsset<GameObject>("Conflict");
            ConflictResolutionScreenPrefab = bundle.LoadAsset<GameObject>("ConflictResolutionScreen");
            ToastPrefab = bundle.LoadAsset<GameObject>("Popup");
            ToastPrefab.AddComponent<Toast>();
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
            Directory.CreateDirectory(Path.Combine(UKPath, "USTs", "Template"));
            File.WriteAllText(Path.Combine(UKPath, "USTs", "Template", "template.ust"), CustomUST.GetTemplateJson());
            File.WriteAllText(Path.Combine(UKPath, "USTs", "Template", "readme.md"), StaticResources.Readme);
        }

        private void OnGUI()
        {
            if(!Manager.IsDebug) return;

            AudioSource[] sources = FindObjectsOfType<AudioSource>()
                .Where(s => s.isPlaying)
                .Where(s => Manager.IsExtendedDebug ? true : s.spatialBlend < 1.0)
                //.OrderByDescending(s => s.clip.length)
                .ToArray();

            void DrawColumn(System.Action<AudioSource> action)
            {
                using var _ = new GUILayout.VerticalScope();
                foreach(AudioSource s in sources)
                {
                    action(s);
                }
            }

            GUIStyle style = GUI.skin.label;
            style.fontSize = 24;

            using var _ = new GUILayout.HorizontalScope();

            style.normal.textColor = Color.green;
            DrawColumn(s => GUILayout.Label(s.name));

            GUILayout.Space(20);
            style.normal.textColor = Color.white;
            DrawColumn(s => GUILayout.Label(s.clip.name));

            GUILayout.Space(20);
            style.normal.textColor = Color.red;
            DrawColumn(s =>
            {
                using var _ = new GUILayout.HorizontalScope();
                GUILayout.Label(s.volume * 100 + "%");
                GUILayout.HorizontalSlider(s.time, 0, s.clip.length, GUILayout.Width(150));
            });
        }
    }
}
