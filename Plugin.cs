using BepInEx;
using UnityEngine.SceneManagement;

namespace USTManager
{
    [BepInPlugin("zed.uk.ustmanager", "USTManager", "1.0.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }
    }
}
