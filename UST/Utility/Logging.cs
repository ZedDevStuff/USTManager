using UnityEngine;

namespace USTManager.Utility
{
    public static class Logging
    {
        public static void Log(object message, Color color = default)
        {
            if(color == default) color = Color.white;
            Debug.Log($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>[USTManager]</color> {message}");
        }
        public static void LogError(object message, Color color = default)
        {
            if(color == default) color = Color.red;
            Debug.LogError($"<color=#{ColorUtility.ToHtmlStringRGB(color)}>[USTManager]</color> {message}");
        }
    }
}