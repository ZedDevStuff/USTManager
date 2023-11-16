using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using HarmonyLib;
using UnityEngine;
using ULTRAKILL;

namespace USTManager
{
    public class MusicManagerPatches
    {
        [HarmonyPatch(typeof(MusicManager),"OnEnable",MethodType.Normal), HarmonyPrefix]
        public static bool OnEnable()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.OnEnable();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "Update"), HarmonyPrefix]
        public static bool Update()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.Update();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "ForceStartMusic"), HarmonyPrefix]
        public static bool ForceStartMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.ForceStartMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "StartMusic"), HarmonyPrefix]
        public static bool StartMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.StartMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "PlayBattleMusic"), HarmonyPrefix]
        public static bool PlayBattleMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.PlayBattleMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "PlayCleanMusic"), HarmonyPrefix]
        public static bool PlayCleanMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.PlayCleanMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "PlayBossMusic"), HarmonyPrefix]
        public static bool PlayBossMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.PlayBossMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "ArenaMusicStart"), HarmonyPrefix]
        public static bool ArenaMusicStart()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.ArenaMusicStart();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "ArenaMusicEnd"), HarmonyPrefix]
        public static bool ArenaMusicEnd()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.ArenaMusicEnd();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "ForceStopMusic"), HarmonyPrefix]
        public static bool ForceStopMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.ForceStopMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "StopMusic"), HarmonyPrefix]
        public static bool StopMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.StopMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "FilterMusic"), HarmonyPrefix]
        public static bool FilterMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.FilterMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "UnfilterMusic"), HarmonyPrefix]
        public static bool UnfilterMusic()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.UnfilterMusic();
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "RemoveHighPass"), HarmonyPrefix]
        public static bool RemoveHighPass()
        {
            if (USTMusicManager.isEnabled) return false;
            DummyMusicManager.RemoveHighPass();
            return true;
        }

        [HarmonyPatch(typeof(MusicManager), "off", MethodType.Setter), HarmonyPrefix]
        public static bool off()
        {
            Debug.Log("off");
            return true;
        }
        [HarmonyPatch(typeof(MusicManager), "volume", MethodType.Setter), HarmonyPrefix]
        public static bool volume()
        {
            Debug.Log("volume");
            return true;
        }
    }
}
