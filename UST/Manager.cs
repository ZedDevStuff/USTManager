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
using Newtonsoft.Json;

namespace USTManager
{
    public class Manager
    {
        public static bool CurrentLevelHandled { get; private set; } = false;
        private static Dictionary<string, AudioClip> CustomUST = new();
        public static List<CustomUST> AllUSTs = new();
        public static void CheckUSTs()
        {
            FileInfo[] files = Directory.GetFiles(Path.Combine(Plugin.UKPath, "USTs"), "*.ust", SearchOption.AllDirectories).Select(x => new FileInfo(x)).ToArray();
            AllUSTs.Clear();
            foreach(FileInfo file in files)
            {
                if(file.Name == "template.ust") continue;
                try
                {
                    CustomUST ust = JsonConvert.DeserializeObject<CustomUST>(File.ReadAllText(file.FullName));
                    if(ust != null)
                    {
                        ust.Path = file.Directory.FullName;
                        ust.Hash = Hash128.Compute(ust.Path).ToString();
                        ust.Levels = ust.Levels.Select(level =>
                        {
                            return new KeyValuePair<string, List<CustomUST.Descriptor>>(level.Key, level.Value.Select(desc =>
                            {
                                desc.Path = Path.Combine(ust.Path, desc.Path);
                                return desc;
                            }).ToList());
                        }).ToDictionary(x => x.Key, x => x.Value);
                        string iconPath = Path.Combine(file.Directory.FullName, "icon.png");
                        if(File.Exists(iconPath))
                        {
                            Texture2D icon = new Texture2D(100, 100);
                            if(icon.LoadImage(File.ReadAllBytes(iconPath)))
                            {
                                ust.Icon = Sprite.Create(icon, new Rect(0, 0, icon.width, icon.height), new Vector2(0.5f, 0.5f));
                            }
                        }
                        AllUSTs.Add(ust);
                    }
                }
                catch
                {
                    Logging.LogError($"Failed to load UST {file.Name}: Invalid JSON");
                }
            }
        }

        public static void UnloadUST()
        {
            CustomUST.Clear();
        }
        public static void LoadUST(CustomUST ust)
        {
            UnloadUST();
            if(ust == null) return;
            Dictionary<string, AudioClip> clips = new();
            string path = ust.Path;
            foreach(var level in ust.Levels)
            {
                foreach(var desc in level.Value)
                {
                    if(!ust.IsMerged && !File.Exists(Path.Combine(path, desc.Path)))
                    {
                        Logging.Log("Skipping");
                        continue;
                    }
                    var d = clips.Where(x => x.Value.name == "[UST] " + Path.GetFileNameWithoutExtension(desc.Path));
                    if(d.Count() > 0)
                    {
                        if(level.Key == "global")
                        {
                            clips.Add(desc.Part, d.First().Value);
                            Logging.Log($"Adding clip {d.First().Value.name}");
                        }
                        else
                        {
                            clips.Add($"{level.Key}:{desc.Part}", d.First().Value);
                            Logging.Log($"Adding clip {level.Key + ":" + desc.Part}");
                        }
                        continue;
                    }
                    AudioClip clip = Loader.LoadClipFromPath(desc.Path);
                    if(clip != null)
                    {
                        if(level.Key == "global")
                        {
                            clips.Add(desc.Part, clip);
                            Logging.Log($"Adding clip {clip.name}");
                        }
                        else
                        {
                            clips.Add($"{level.Key}:{desc.Part}", clip);
                            Logging.Log($"Adding clip {level.Key + ":" + desc.Part}");
                        }
                    }
                    else Logging.Log("Something went wrong with this clip");
                }
            }
            if(clips.Count > 0)
            {
                CustomUST.Clear();
                CustomUST = clips;
            }
        }
        public static void DeleteUST(CustomUST ust)
        {
            if(ust == null) return;
            string path = ust.Path;
            if(Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        public static bool IsEnabled = true;
        public static bool IsDebug = false;

        public static void HandleAudio(string level, AudioSource source, AudioClip clip)
        {
            // clip can be null, but not source or source.clip
            if(source?.clip == null) return;

            // don't do anything if the track is already patched
            if(source.clip.name.Contains("[UST]")) return;
            if(clip != null && clip.name.Contains("[UST]")) return;

            // before trying to use any "handlers", check for an exact match
            if(CustomUST.ContainsKey(source.clip.name))
            {
                source.clip = CustomUST[source.clip.name];
                return;
            }

            string actualLevel = level.Replace("Level ", "");
            string key = actualLevel switch
            {
                "0-5" => Handle0_5(source, clip),
                "1-4" => Handle1_4(source, clip),
                "2-4" => Handle2_4(source, clip),
                "3-1" => Handle3_1(source, clip),
                "3-2" => Handle3_2(source, clip),
                "4-4" => Handle4_4(source, clip),
                "5-2" => Handle5_2(source, clip),
                "5-3" => Handle5_3(source, clip),
                "5-4" => Handle5_4(source, clip),
                "6-1" => Handle6_1(source, clip),
                "6-2" => Handle6_2(source, clip),
                "7-1" => Handle7_1(source, clip),
                "7-2" => Handle7_2(source, clip),
                "7-3" => Handle7_3(source, clip),
                "7-4" => Handle7_4(source, clip),
                "P-1" => HandleP_1(source, clip),
                "P-2" => HandleP_2(source, clip),
                _ => DefaultHandler(actualLevel, source),
            };

            if(key != null && CustomUST.ContainsKey(key))
            {
                source.clip = CustomUST[key];
                if(key is "2-4:boss1" or "2-4:boss2" or "7-1:boss2")
                {
                    source.pitch = 1f;
                }
            }
        }
        public static string DefaultHandler(string level, AudioSource source)
        {
            if(source.name == "CleanTheme")
            {
                return level + ":clean";
            }
            else if(source.name == "BattleTheme")
            {
                return level + ":battle";
            }
            else if(source.name == "BossTheme")
            {
                if(!CustomUST.ContainsKey(level + ":boss"))
                {
                    return level + ":battle";
                }
                return level + ":boss";
            }
            return null;
        }
        public static string Handle0_5(AudioSource source, AudioClip clip) => source.clip.name switch
        {
            "Cerberus A" => "0-5:boss1",
            "Cerberus B" => "0-5:boss2",
            _ => null
        };
        public static string Handle1_4(AudioSource source, AudioClip clip) => source.clip.name switch
        {
            "V2 Intro" => "1-4:intro",
            "V2 1-4" => "1-4:boss",
            _ => null
        };
        public static string Handle2_4(AudioSource source, AudioClip clip) => source.clip.name switch
        {
            "Minos Corpse A" => "2-4:boss1",
            "Minos Corpse B" => "2-4:boss2",
            _ => null
        };
        public static string Handle3_1(AudioSource source, AudioClip clip) => source.clip.name switch
        {
            "3-1 Guts Clean" => "3-1:clean1",
            "3-1 Glory Clean" => "3-1:clean2",
            "3-1 Guts" => "3-1:battle1",
            "3-1 Glory" => "3-1:battle2",
            _ => null,
        };
        public static string Handle3_2(AudioSource source, AudioClip clip) => source.name switch
        {
            "Music 1" => "3-2:intro1", // Flesh Ambiance
            "Music 2" => "3-2:intro2", // Gabriel 3-2 Intro
            "Music 3" => "3-2:boss", // Gabriel 3-2
            _ => null,
        };
        public static string Handle4_4(AudioSource source, AudioClip clip) => source.clip.name switch
        {
            "V2 4-4" => "4-4:boss",
            _ => null,
        };

        public static string Handle5_2(AudioSource source, AudioClip clip) => source.clip.name switch
        {
            "Ferryman A" => "5-2:boss1",
            "Ferryman B" => "5-2:boss2",
            "Ferryman C" => "5-2:boss3",
            _ => null
        };
        public static string Handle5_3(AudioSource source, AudioClip clip) => source.name switch
        {
            // 5-3 Clean, 5-3 Aftermath Intro, 5-3 Aftermath Clean
            "CleanTheme" => source.clip.name.Contains("Aftermath") ? "5-3:clean2" : "5-3:clean1",
            // 5-3, 5-3 Aftermath Intro, 5-3 Aftermath
            "BattleTheme" or "BossTheme" => source.clip.name.Contains("Aftermath") ? "5-3:battle2" : "5-3:battle1",
            _ => null,
        };
        public static string Handle5_4(AudioSource source, AudioClip clip) => source.name switch
        {
            "Music 1" => "5-4:boss1", // Leviathan A
            "Music 2" => "5-4:boss2", // Leviathan B
            _ => null
        };
        public static string Handle6_1(AudioSource source, AudioClip clip)
        {
            return source.name switch
            {
                // CleanTheme / 6-1 Clean
                // CleanTheme / Deep Drone 1
                "CleanTheme" => source.clip.name.Contains("6-1") ? "6-1:clean" : null,

                // BattleTheme / 6-1
                // BattleTheme / Deep Drone 3B
                // BossTheme / 6-1
                // BossTheme / Deep Drone 3
                "BattleTheme" or "BossTheme" => source.clip.name.Contains("6-1") ? "6-1:battle" : null,

                // ClimaxMusic / 6-1 Hall of Sacreligious Remains
                "ClimaxMusic" => "6-1:boss",

                _ => null,
            };
        }
        public static string Handle6_2(AudioSource source, AudioClip clip) => source.name switch
        {
            "BossMusic" => "6-2:boss", // Gabriel 6-2
            _ => null
        };
        public static string Handle7_1(AudioSource source, AudioClip clip) => source.name switch
        {
            "CleanTheme" => "7-1:clean", // 7-1 Clean
            "BattleTheme" => "7-1:battle", // 7-1
            "MinotaurPhase1Music" => "7-1:boss1", // Minotaur A
            "MinotaurPhase2Music" => "7-2:boss2", // Minotaur B
            // Wind / Windloop
            // IntroMusic / 7-1 Intro
            // MinotaurIntroMusic / MinotaurIntro
            _ => null,
        };
        public static string Handle7_2(AudioSource source, AudioClip clip) => source.name switch
        {
            // 7-2 Intro Clean, 7-2 Clean
            "CleanTheme" => source.clip.name.Contains("Intro") ? "7-2:clean1" : "7-2:clean2",
            // 7-2 Intro Battle, 7-2
            "BattleTheme" or "BossTheme" => source.clip.name.Contains("Intro") ? "7-2:battle1" : "7-2:battle2",
            _ => null,
        };
        public static string Handle7_3(AudioSource source, AudioClip clip) => source.name switch
        {
            // 7-3 Intro Clean, 7-3 Clean
            "CleanTheme" => source.clip.name.Contains("Intro") ? "7-3:clean1" : "7-3:clean2",
            // 7-3 Intro Clean, 7-3
            "BattleTheme" or "BossTheme" => source.clip.name.Contains("Intro") ? "7-3:battle1" : "7-3:battle2",
            // Wind / WindLoopHaunted
            // Drone3 / MollyCorrupted4
            _ => null,
        };
        // not currently supported
        public static string Handle7_4(AudioSource source, AudioClip clip) => null;
        public static string HandleP_1(AudioSource source, AudioClip clip) => source.clip.name switch
        {
            // Chaos / Flesh Prison
            "Flesh Prison" => "P-1:boss1",

            // IntroMusic / Minos Prime Intro
            // Music 3 / Minos Prime
            "Minos Prime Intro" or "Minos Prime" => "P-1:boss2",

            // Sourire / Sourire d'avril
            _ => null,
        };
        // not currently supported
        public static string HandleP_2(AudioSource source, AudioClip clip) => null;

        public static bool HandleAudio(string level, MusicChanger changer)
        {
            string actualLevel = level.Replace("Level ", "");
            return actualLevel switch
            {
                "1-1" => Handle1_1(changer),
                "1-2" => Handle1_2(changer),
                "4-3" => Handle4_3(changer),
                _ => true,
            };
        }

        public static bool Handle1_1(MusicChanger changer)
        {
            if(changer != null)
            {
                if((changer.battle != null && changer.battle.name.Contains("[UST]")) || (changer.clean != null && changer.clean.name.Contains("[UST]")) || (changer.boss != null && changer.boss.name.Contains("[UST]"))) return false;
                if(CustomUST.ContainsKey("1-1:clean"))
                {
                    changer.clean = CustomUST["1-1:clean"];
                }
                if(CustomUST.ContainsKey("1-1:battle"))
                {
                    changer.battle = CustomUST["1-1:battle"];
                }
                return true;
            }
            return true;
        }

        public static bool Handle1_2(MusicChanger changer)
        {
            if(changer != null)
            {
                if((changer.battle != null && changer.battle.name.Contains("[UST]")) || (changer.clean != null && changer.clean.name.Contains("[UST]")) || (changer.boss != null && changer.boss.name.Contains("[UST]"))) return false;
                if(changer.clean != null && changer.clean.name.Contains("Dark"))
                {
                    if(CustomUST.ContainsKey("1-2:clean1"))
                    {
                        changer.clean = CustomUST["1-2:clean1"];
                    }
                }
                if(changer.battle != null && changer.battle.name.Contains("Dark"))
                {
                    if(CustomUST.ContainsKey("1-2:battle1"))
                    {
                        changer.battle = CustomUST["1-2:battle1"];
                    }
                }
                if(changer.clean != null && changer.clean.name.Contains("Noise"))
                {
                    if(CustomUST.ContainsKey("1-2:clean2"))
                    {
                        changer.clean = CustomUST["1-2:clean2"];
                    }
                }
                if(changer.battle != null && changer.battle.name.Contains("Noise"))
                {
                    if(CustomUST.ContainsKey("1-2:battle2"))
                    {
                        changer.battle = CustomUST["1-2:battle2"];
                    }
                }
                return true;
            }
            return true;
        }

        public static bool Handle4_3(MusicChanger changer)
        {
            if(changer != null)
            {
                if((changer.battle != null && changer.battle.name.Contains("[UST]")) || (changer.clean != null && changer.clean.name.Contains("[UST]")) || (changer.boss != null && changer.boss.name.Contains("[UST]"))) return false;
                if(changer.clean != null && changer.clean.name.Contains("Phase 1"))
                {
                    if(!CustomUST.ContainsKey("4-3:clean1")) return true;
                    changer.clean = CustomUST["4-3:clean1"];
                }
                if(changer.battle != null && changer.battle.name.Contains("Phase 1"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle1")) return true;
                    changer.battle = CustomUST["4-3:battle1"];
                }
                if(changer.clean != null && changer.clean.name.Contains("Phase 2"))
                {
                    if(!CustomUST.ContainsKey("4-3:clean2")) return true;
                    changer.clean = CustomUST["4-3:clean2"];
                }
                if(changer.battle != null && changer.battle.name.Contains("Phase 2"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle2")) return true;
                    changer.battle = CustomUST["4-3:battle2"];
                }
                if(changer.clean != null && changer.clean.name.Contains("Phase 3"))
                {
                    if(!CustomUST.ContainsKey("4-3:clean3")) return true;
                    changer.clean = CustomUST["4-3:clean3"];
                }
                if(changer.battle != null && changer.battle.name.Contains("Phase 3"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle3")) return true;
                    changer.battle = CustomUST["4-3:battle3"];
                }
                return true;
            }
            /*if(source == null || source.clip == null) return true;
            if(source.name == "CleanTheme" && !source.clip.name.Contains("[UST]"))
            {
                if(source.clip.name.Contains("Phase 1"))
                {
                    if(!CustomUST.ContainsKey("4-3:clean1")) return true;
                    else source.clip = CustomUST["4-3:clean1"];
                }
                else if(source.clip.name.Contains("Phase 2"))
                {
                    if(!CustomUST.ContainsKey("4-3:clean2")) return true;
                    else source.clip = CustomUST["4-3:clean2"];
                }
                else if(source.clip.name.Contains("Phase 3"))
                {
                    if(!CustomUST.ContainsKey("4-3:clean3")) return true;
                    else source.clip = CustomUST["4-3:clean3"];
                }
            }
            else if(source.name == "BattleTheme" && !source.clip.name.Contains("[UST]"))
            {
                if(source.clip.name.Contains("Phase 1"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle1")) return true;
                    else source.clip = CustomUST["4-3:battle1"];
                }
                else if(source.clip.name.Contains("Phase 2"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle2")) return true;
                    else source.clip = CustomUST["4-3:battle2"];
                }
                else if(source.clip.name.Contains("Phase 3"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle3")) return true;
                    else source.clip = CustomUST["4-3:battle3"];
                }
            }
            else if(source.name == "BossTheme" && !source.clip.name.Contains("[UST]"))
            {
                if(source.clip.name.Contains("Phase 1"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle1")) return true;
                    else source.clip = CustomUST["4-3:battle1"];
                }
                else if(source.clip.name.Contains("Phase 2"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle2")) return true;
                    else source.clip = CustomUST["4-3:battle2"];
                }
                else if(source.clip.name.Contains("Phase 3"))
                {
                    if(!CustomUST.ContainsKey("4-3:battle3")) return true;
                    else source.clip = CustomUST["4-3:battle3"];
                }
            }*/
            return true;
        }
    }
}
