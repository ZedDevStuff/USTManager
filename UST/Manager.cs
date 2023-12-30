using Logic;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;
using USTManager.Data;
using USTManager.Utility;

namespace USTManager
{
    public class Manager
    {
        public static bool CurrentLevelHandled { get; private set; } = false;
        private static Dictionary<string, AudioClip> CustomUST = new();
        public static List<CustomUST> AllUSTs = new();
        private static int Counter1 = 0, Counter2 = 0;
        public static AudioClip GetClip(string id)
        {
            if(CustomUST.TryGetValue(id, out AudioClip clip))
            {
                return clip;
            }
            return null;
        }
        public static AudioClip GetClip(string level, string part)
        {
            return GetClip($"{level}:{part}");
        }
        
        public static void LoadUST(CustomUST ust)
        {
            Dictionary<string, AudioClip> clips = new();
            string path = ust.Path;
            foreach(var level in ust.Levels)
            {
                foreach(var desc in level.Value)
                {
                    if(!File.Exists(Path.Combine(path,desc.Path))) continue;
                    var d = clips.Where(x => x.Value.name == "[UST] " + Path.GetFileNameWithoutExtension(desc.Path));
                    if(d.Count() > 0)
                    {
                        if(level.Key == "global")
                        {
                            clips.Add(desc.Part, d.First().Value);
                            Debug.Log($"[USTManager] Adding clip {d.First().Value.name}");
                        }
                        else
                        {
                            clips.Add($"{level.Key}:{desc.Part}", d.First().Value);
                            Debug.Log($"[USTManager] Adding clip {level.Key+":"+desc.Part}");
                        }
                        continue;
                    }
                    AudioClip clip = Loader.LoadClipFromPath(Path.Combine(path,desc.Path));
                    if(clip != null)
                    {
                        if(level.Key == "global")
                        {
                            clips.Add(desc.Part, clip);
                            Debug.Log($"[USTManager] Adding clip {clip.name}");
                        }
                        else
                        {
                            clips.Add($"{level.Key}:{desc.Part}", clip);
                            Debug.Log($"[USTManager] Adding clip {level.Key+":"+desc.Part}");
                        }
                    }
                }
            }
            if(clips.Count > 0)
            {
                CustomUST.Clear();
                CustomUST = clips;
            }
        }

        public static bool IsEnabled = true;
        public static bool IsDebug = false;
        private static Dictionary<string, Func<AudioSource,AudioClip,bool>> Handlers = new()
        {
            {"0-1",Handle0_1},
            {"0-2",Handle0_2},
            {"0-3",Handle0_3},
            {"0-4",Handle0_4},
            {"0-5",Handle0_5},
            {"1-1",Handle1_1},
            {"1-2",Handle1_2},
            {"1-3",Handle1_3},
            {"1-4",Handle1_4},
            {"2-1",Handle2_1},
            {"2-2",Handle2_2},
            {"2-3",Handle2_3},
            {"2-4",Handle2_4},
            {"3-1",Handle3_1},
            {"3-2",Handle3_2},
            {"4-1",Handle4_1},
            {"4-2",Handle4_2},
            {"4-3",Handle4_3},
            {"4-4",Handle4_4},
            {"5-1",Handle5_1},
            {"5-2",Handle5_2},
            {"5-3",Handle5_3},
            {"5-4",Handle5_4},
            {"6-1",Handle6_1},
            {"6-2",Handle6_2},
            {"7-1",Handle7_1},
            {"7-2",Handle7_2},
            {"7-3",Handle7_3},
            {"7-4",Handle7_4},
        };
        public static bool HandleAudio(string level, AudioSource source, AudioClip clip)
        {
            if(clip != null)
            {
                if(clip.name.Contains("[UST]")) return true;
                if(CustomUST.ContainsKey(clip.name)) source.clip = CustomUST[clip.name];
            }
            string actualLevel = level.Replace("Level ","");
            if(Handlers.ContainsKey(actualLevel)) return Handlers[actualLevel](source, clip);
            return true;
        }
        public static bool DefaultHandler(string level, AudioSource source, AudioClip clip)
        {
            if(source.name == "CleanTheme" && !source.clip.name.Contains("[UST]"))
            {
                if(!CustomUST.ContainsKey(level + ":clean")) return true;
                source.clip = CustomUST[level + ":clean"];
            }
            if(source.name == "BattleTheme" && !source.clip.name.Contains("[UST]"))
            {
                if(!CustomUST.ContainsKey(level + ":battle")) return true;
                source.clip = CustomUST[level + ":battle"];
            }
            if(source.name == "BossTheme" && !source.clip.name.Contains("[UST]"))
            {
                if(!CustomUST.ContainsKey(level + ":boss")) return true;
                source.clip = CustomUST[level + ":boss"];
            }
            return true;
        }
        public static bool Handle0_1(AudioSource source, AudioClip clip) => DefaultHandler("0-1", source, clip);
        public static bool Handle0_2(AudioSource source, AudioClip clip) => DefaultHandler("0-2", source, clip);
        public static bool Handle0_3(AudioSource source, AudioClip clip) => DefaultHandler("0-3", source, clip);
        public static bool Handle0_4(AudioSource source, AudioClip clip) => DefaultHandler("0-4", source, clip);
        public static bool Handle0_5(AudioSource source, AudioClip clip)
        {
            if(source == null || source.clip == null) return true;
            if(source.clip.name == "Cerberus A")
            {
                if(!CustomUST.ContainsKey("0-5:boss1")) return true;
                source.clip = CustomUST["0-5:boss1"];
            }
            if(source.clip.name == "Cerberus B")
            {
                if(!CustomUST.ContainsKey("0-5:boss2")) return true;
                source.clip = CustomUST["0-5:boss2"];
            }
            return true;
        }
        
        public static bool Handle1_1(AudioSource source, AudioClip clip)
        {
            if(clip	!= null)
            {
                if(clip.name == "1-1")
                {
                    if(!CustomUST.ContainsKey("1-1:battle")) return true;
                    else return false;
                }
                if(clip.name == "1-1 Clean")
                {
                    if(!CustomUST.ContainsKey("1-1:clean")) return true;
                    else return false;
                    
                }
            }
            if(source.name == "CleanTheme" && !source.clip.name.Contains("[UST]"))
            {
                if(Counter1 == 0)
                {
                    Counter1 = 1;
                    if(CustomUST.ContainsKey("1-1:intro"))
                    {
                        if(!CustomUST.ContainsKey("1-1:intro")) return true;
                        source.clip = CustomUST["1-1:intro"];
                    }
                    else
                    {
                        if(!CustomUST.ContainsKey("1-1:clean")) return true;
                        source.clip = CustomUST["1-1:clean"];
                    }
                }
                else
                {
                    if(!CustomUST.ContainsKey("1-1:clean")) return true;
                    source.clip = CustomUST["1-1:clean"];
                }
                
            }
            if(source.name == "BattleTheme" && !source.clip.name.Contains("[UST]"))
            {
                if(!CustomUST.ContainsKey("1-1:battle")) return true;
                source.clip = CustomUST["1-1:battle"];
            }
            return true;
        }
        public static bool Handle1_2(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle1_3(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle1_4(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle2_1(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle2_2(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle2_3(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle2_4(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle3_1(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle3_2(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle4_1(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle4_2(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle4_3(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle4_4(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle5_1(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle5_2(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle5_3(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle5_4(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle6_1(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle6_2(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle7_1(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle7_2(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle7_3(AudioSource source, AudioClip clip)
        {
            return true;
        }
        public static bool Handle7_4(AudioSource source, AudioClip clip)
        {
            return true;
        }
    }
}
