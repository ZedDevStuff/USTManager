using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using USTManager.Data;
using USTManager.Utility;
using Newtonsoft.Json;

namespace USTManager
{
    public static class Manager
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
                            if(clips.ContainsKey(desc.Part)) continue;
                            clips.Add(desc.Part, d.First().Value);
                            Logging.Log($"Adding clip {d.First().Value.name}");
                        }
                        else
                        {
                            if(clips.ContainsKey($"{level.Key}:{desc.Part}")) continue;
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
                            if(clips.ContainsKey(desc.Part)) continue;
                            clips.Add(desc.Part, clip);
                            Logging.Log($"Adding clip {clip.name}");
                        }
                        else
                        {
                            if(clips.ContainsKey($"{level.Key}:{desc.Part}")) continue;
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
                    "Cerberus A" => "boss1",
                    "Cerberus B" => "boss2",
                    _ => null
                },
                "1-1" => HeartOfTheSunrise(source.clip.name),
                "1-2" => TheBurningWorld(source.clip.name, source.name == "CleanTheme"),
                "1-4" => source.clip.name switch
                {
                    "V2 Intro" => "intro",
                    "V2 1-4" => "boss",
                    _ => null
                },
                "2-4" => source.clip.name switch
                {
                    "Minos Corpse A" => "boss1",
                    "Minos Corpse B" => "boss2",
                    _ => null
                },
                "3-1" => source.clip.name switch
                {
                    "3-1 Guts Clean" => "clean1",
                    "3-1 Glory Clean" => "clean2",
                    "3-1 Guts" => "battle1",
                    "3-1 Glory" => "battle2",
                    _ => null,
                },
                "3-2" => source.name switch
                {
                    "Music 1" => "intro1", // Flesh Ambiance
                    "Music 2" => "intro2", // Gabriel 3-2 Intro
                    "Music 3" => "boss", // Gabriel 3-2
                    _ => null,
                },
                "4-3" => AShotInTheDark(source.clip.name),
                "4-4" => source.clip.name switch
                {
                    "V2 4-4" => "boss",
                    _ => null,
                },
                "5-2" => source.clip.name switch
                {
                    "Ferryman A" => "boss1",
                    "Ferryman B" => "boss2",
                    "Ferryman C" => "boss3",
                    _ => null
                },
                "5-3" => source.name switch
                {
                    // 5-3 Clean, 5-3 Aftermath Intro, 5-3 Aftermath Clean
                    "CleanTheme" => source.clip.name.Contains("Aftermath") ? "clean2" : "clean1",
                    // 5-3, 5-3 Aftermath Intro, 5-3 Aftermath
                    "BattleTheme" or "BossTheme" => source.clip.name.Contains("Aftermath") ? "battle2" : "battle1",
                    _ => null,
                },
                "5-4" => source.name switch
                {
                    "Music 1" => "boss1", // Leviathan A
                    "Music 2" => "boss2", // Leviathan B
                    _ => null
                },
                "6-1" => source.name switch
                {
                    // CleanTheme / 6-1 Clean
                    // CleanTheme / Deep Drone 1
                    "CleanTheme" => source.clip.name.Contains("6-1") ? "clean" : null,

                    // BattleTheme / 6-1
                    // BattleTheme / Deep Drone 3B
                    // BossTheme / 6-1
                    // BossTheme / Deep Drone 3
                    "BattleTheme" or "BossTheme" => source.clip.name.Contains("6-1") ? "battle" : null,

                    // ClimaxMusic / 6-1 Hall of Sacreligious Remains
                    "ClimaxMusic" => "boss",

                    _ => null,
                },
                "6-2" => source.name switch
                {
                    "BossMusic" => "boss", // Gabriel 6-2
                    _ => null
                },
                "7-1" => source.name switch
                {
                    "CleanTheme" => "clean", // 7-1 Clean
                    "BattleTheme" => "battle", // 7-1
                    "MinotaurPhase1Music" => "boss1", // Minotaur A
                    "MinotaurPhase2Music" => "boss2", // Minotaur B
                    // Wind / Windloop
                    // IntroMusic / 7-1 Intro
                    // MinotaurIntroMusic / MinotaurIntro
                    _ => null,
                },
                "7-2" => source.name switch
                {
                    // 7-2 Intro Clean, 7-2 Clean
                    "CleanTheme" => source.clip.name.Contains("Intro") ? "clean1" : "clean2",
                    // 7-2 Intro Battle, 7-2
                    "BattleTheme" or "BossTheme" => source.clip.name.Contains("Intro") ? "battle1" : "battle2",
                    _ => null,
                },
                "7-3" => source.name switch
                {
                    // 7-3 Intro Clean, 7-3 Clean
                    "CleanTheme" => source.clip.name.Contains("Intro") ? "clean1" : "clean2",
                    // 7-3 Intro Clean, 7-3
                    "BattleTheme" or "BossTheme" => source.clip.name.Contains("Intro") ? "battle1" : "battle2",
                    // Wind / WindLoopHaunted
                    // Drone3 / MollyCorrupted4
                    _ => null,
                },
                "7-4" => null, // not currently supported
                "P-1" => source.clip.name switch
                {
                    // Chaos / Flesh Prison
                    "Flesh Prison" => "boss1",

                    // IntroMusic / Minos Prime Intro
                    // Music 3 / Minos Prime
                    "Minos Prime Intro" or "Minos Prime" => "boss2",

                    // Sourire / Sourire d'avril
                    _ => null,
                },
                "P-2" => null, // not currently supported

                _ => source.name switch
                {
                    "CleanTheme" => "clean",
                    "BossTheme" when CustomUST.ContainsKey(level + ":boss") => "boss",
                    "BattleTheme" or "BossTheme" => "battle",
                    _ => null,
                },
            };
            if(key != null) key = $"{level}:{key}";

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
            if(key != null) key = $"{level}:{key}";

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
            "A Thousand Greetings" => "clean1",
            "1-1 Clean" => "clean2",
            "1-1" => "battle",
            _ => null,
        };

        private static string TheBurningWorld(string clipName, bool clean) => clipName switch
        {
            "A Thousand Greetings" => clean ? "clean0" : "battle0",
            "1-2 Dark Clean" => "clean1",
            "1-2 Noise Clean" => "clean2",
            "1-2 Dark Battle" => "battle1",
            "1-2 Noise Battle" => "battle2",
            _ => null,
        };

        private static string AShotInTheDark(string clipName) => clipName switch
        {
            "4-3 Phase 1 Clean" => "clean1",
            "4-3 Phase 2 Clean" => "clean2",
            "4-3 Phase 3 Clean" => "clean3", // doesn't actually exist: when you clear the final arena, the game plays 4-3 Phase 1 Clean
            "4-3 Phase 1" => "battle1",
            "4-3 Phase 2" => "battle2",
            "4-3 Phase 3" => "battle3",
            _ => null,
        };
    }
}
