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

        public static void HandleAudioSource(string level, AudioSource source)
        {
            if(!IsEnabled) return;

            if(source?.clip == null) return;

            // don't do anything if the track is already patched
            if(source.clip.name.Contains("[UST]")) return;

            // before trying to use any "handlers", check for an exact match
            if(CustomUST.ContainsKey(source.clip.name))
            {
                source.clip = CustomUST[source.clip.name];
                return;
            }

            level = level.Replace("Level ", "");
            string key = level switch
            {
                "0-5" => source.clip.name switch
                {
                    "Cerberus A" => "0-5:boss1",
                    "Cerberus B" => "0-5:boss2",
                    _ => null
                },
                "1-1" => HeartOfTheSunrise(source.clip.name),
                "1-2" => TheBurningWorld(source.clip.name, source.name == "CleanTheme"),
                "1-4" => source.clip.name switch
                {
                    "V2 Intro" => "1-4:intro",
                    "V2 1-4" => "1-4:boss",
                    _ => null
                },
                "2-4" => source.clip.name switch
                {
                    "Minos Corpse A" => "2-4:boss1",
                    "Minos Corpse B" => "2-4:boss2",
                    _ => null
                },
                "3-1" => source.clip.name switch
                {
                    "3-1 Guts Clean" => "3-1:clean1",
                    "3-1 Glory Clean" => "3-1:clean2",
                    "3-1 Guts" => "3-1:battle1",
                    "3-1 Glory" => "3-1:battle2",
                    _ => null,
                },
                "3-2" => source.name switch
                {
                    "Music 1" => "3-2:intro1", // Flesh Ambiance
                    "Music 2" => "3-2:intro2", // Gabriel 3-2 Intro
                    "Music 3" => "3-2:boss", // Gabriel 3-2
                    _ => null,
                },
                "4-3" => AShotInTheDark(source.clip.name),
                "4-4" => source.clip.name switch
                {
                    "V2 4-4" => "4-4:boss",
                    _ => null,
                },
                "5-2" => source.clip.name switch
                {
                    "Ferryman A" => "5-2:boss1",
                    "Ferryman B" => "5-2:boss2",
                    "Ferryman C" => "5-2:boss3",
                    _ => null
                },
                "5-3" => source.name switch
                {
                    // 5-3 Clean, 5-3 Aftermath Intro, 5-3 Aftermath Clean
                    "CleanTheme" => source.clip.name.Contains("Aftermath") ? "5-3:clean2" : "5-3:clean1",
                    // 5-3, 5-3 Aftermath Intro, 5-3 Aftermath
                    "BattleTheme" or "BossTheme" => source.clip.name.Contains("Aftermath") ? "5-3:battle2" : "5-3:battle1",
                    _ => null,
                },
                "5-4" => source.name switch
                {
                    "Music 1" => "5-4:boss1", // Leviathan A
                    "Music 2" => "5-4:boss2", // Leviathan B
                    _ => null
                },
                "6-1" => source.name switch
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
                },
                "6-2" => source.name switch
                {
                    "BossMusic" => "6-2:boss", // Gabriel 6-2
                    _ => null
                },
                "7-1" => source.name switch
                {
                    "CleanTheme" => "7-1:clean", // 7-1 Clean
                    "BattleTheme" => "7-1:battle", // 7-1
                    "MinotaurPhase1Music" => "7-1:boss1", // Minotaur A
                    "MinotaurPhase2Music" => "7-2:boss2", // Minotaur B
                    // Wind / Windloop
                    // IntroMusic / 7-1 Intro
                    // MinotaurIntroMusic / MinotaurIntro
                    _ => null,
                },
                "7-2" => source.name switch
                {
                    // 7-2 Intro Clean, 7-2 Clean
                    "CleanTheme" => source.clip.name.Contains("Intro") ? "7-2:clean1" : "7-2:clean2",
                    // 7-2 Intro Battle, 7-2
                    "BattleTheme" or "BossTheme" => source.clip.name.Contains("Intro") ? "7-2:battle1" : "7-2:battle2",
                    _ => null,
                },
                "7-3" => source.name switch
                {
                    // 7-3 Intro Clean, 7-3 Clean
                    "CleanTheme" => source.clip.name.Contains("Intro") ? "7-3:clean1" : "7-3:clean2",
                    // 7-3 Intro Clean, 7-3
                    "BattleTheme" or "BossTheme" => source.clip.name.Contains("Intro") ? "7-3:battle1" : "7-3:battle2",
                    // Wind / WindLoopHaunted
                    // Drone3 / MollyCorrupted4
                    _ => null,
                },
                "7-4" => null, // not currently supported
                "P-1" => source.clip.name switch
                {
                    // Chaos / Flesh Prison
                    "Flesh Prison" => "P-1:boss1",

                    // IntroMusic / Minos Prime Intro
                    // Music 3 / Minos Prime
                    "Minos Prime Intro" or "Minos Prime" => "P-1:boss2",

                    // Sourire / Sourire d'avril
                    _ => null,
                },
                "P-2" => null, // not currently supported

                _ => source.name switch
                {
                    "CleanTheme" => level + ":clean",
                    "BossTheme" when CustomUST.ContainsKey(level + ":boss") => level + ":boss",
                    "BattleTheme" or "BossTheme" => level + ":battle",
                    _ => null,
                },
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

        public static bool HandleMusicChanger(string level, MusicChanger changer)
        {
            if(!IsEnabled) return true;

            string cleanName = changer.clean?.name ?? "NULL";
            string battleName = changer.battle?.name ?? "NULL";
            string bossName = changer.boss?.name ?? "NULL";
            if(cleanName.Contains("[UST]") || battleName.Contains("[UST]") || bossName.Contains("[UST]"))
            {
                return false;
            }

            level = level.Replace("Level ", "");
            HandleChangerClip(level, "clean", ref changer.clean);
            HandleChangerClip(level, "battle", ref changer.battle);
            HandleChangerClip(level, "boss", ref changer.boss);

            return true;
        }

        private static void HandleChangerClip(string level, string flavor, ref AudioClip clip)
        {
            if(clip == null) return;

            string key = level switch
            {
                "1-1" => HeartOfTheSunrise(clip.name),
                "1-2" => TheBurningWorld(clip.name, flavor == "clean"),
                "4-3" => AShotInTheDark(clip.name),
                _ => null,
            };

            if(key != null && CustomUST.ContainsKey(key))
            {
                clip = CustomUST[key];
            }
            else if(level == "1-1" && flavor == "clean" && CustomUST.ContainsKey("1-1:clean"))
            {
                // "A Thousand Greetings" and "A Shattered Illusion (clean)" used to both be 1-1:clean
                clip = CustomUST["1-1:clean"];
            }
        }

        // These functions are used by both the AudioSource patch and the MusicChanger patch.
        // For these levels, the AudioSource logic handles the music at the start of the level,
        // while the MusicChangerPatch is needed to correctly override music mid-level.
        //
        // Ideally, each level would be handled by exactly one of the two patches,
        // but top minds have thus far been unable to solve this problem.

        private static string HeartOfTheSunrise(string clipName) => clipName switch
        {
            "A Thousand Greetings" => "1-1:clean1",
            "1-1 Clean" => "1-1:clean2",
            "1-1" => "1-1:battle",
            _ => null,
        };

        private static string TheBurningWorld(string clipName, bool clean) => clipName switch
        {
            "A Thousand Greetings" => clean ? "1-2:clean0" : "1-2:battle0",
            "1-2 Dark Clean" => "1-2:clean1",
            "1-2 Noise Clean" => "1-2:clean2",
            "1-2 Dark Battle" => "1-2:battle1",
            "1-2 Noise Battle" => "1-2:battle2",
            _ => null,
        };

        private static string AShotInTheDark(string clipName) => clipName switch
        {
            "4-3 Phase 1 Clean" => "4-3:clean1",
            "4-3 Phase 2 Clean" => "4-3:clean2",
            "4-3 Phase 3 Clean" => "4-3:clean3", // doesn't actually exist: when you clear the final arena, the game plays 4-3 Phase 1 Clean
            "4-3 Phase 1" => "4-3:battle1",
            "4-3 Phase 2" => "4-3:battle2",
            "4-3 Phase 3" => "4-3:battle3",
            _ => null,
        };
    }
}
