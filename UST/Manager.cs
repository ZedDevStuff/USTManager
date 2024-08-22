using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using USTManager.Data;
using USTManager.Utility;
using Newtonsoft.Json;
using System;

namespace USTManager
{
    public static class Manager
    {
        public static bool IsEnabled = true;
        public static bool IsDebug = false;
        public static bool IsExtendedDebug = false;
        public static float DebugLifetime = 1f;

        private static Dictionary<string, AudioClip> CustomUST = new();
        public static List<CustomUST> AllUSTs = new();
        public static void CheckUSTs()
        {
            IEnumerable<FileInfo> files = Directory.GetFiles(Path.Combine(Plugin.UKPath, "USTs"), "*.ust", SearchOption.AllDirectories).Select(x => new FileInfo(x));
            files = files.Concat(GetUstsInPlugins());
            AllUSTs.Clear();
            LegacyUSTConverter.legacyUSTs.Clear();
            foreach(FileInfo file in files)
            {
                if(file.Name == "template.ust") continue;
                try
                {
                    CustomUST ust = JsonConvert.DeserializeObject<CustomUST>(File.ReadAllText(file.FullName));
                    if(ust == null) continue;

                    ust.Path = file.Directory.FullName;
                    foreach(Dictionary<string, string> level in ust.Levels.Values)
                    {
                        foreach(string track in level.Keys.ToArray())
                        {
                            level[track] = Path.Combine(ust.Path, level[track]);
                        }
                    }
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
                catch(Exception ex)
                {
                    if(LegacyUSTConverter.IsLegacyUST(file.FullName))
                    {
                        Logging.Log("Legacy UST detected", Color.yellow);
                    }
                    else
                    {
                        Logging.LogError($"Failed to load UST {file.Name}: Invalid JSON");
                        Logging.Log(ex, Color.red);
                    }
                }
            }
            if(LegacyUSTConverter.legacyUSTs.Count > 0)
            {
                LegacyUSTConverter.ConvertLegacyUSTs();
                USTSelectionScreen.Instance.Refresh();
            }
        }

        public static void UnloadUST()
        {
            CustomUST.Clear();
        }
        public static void LoadUST(CustomUST ust, bool fromSaved = false)
        {
            UnloadUST();
            if(ust == null) return;
            Dictionary<string, AudioClip> clips = new();
            string path = ust.Path;
            foreach(var level in ust.Levels)
            {
                foreach(var desc in level.Value)
                {
                    string trackPart = desc.Key;
                    string trackPath = desc.Value;
                    if(fromSaved && !File.Exists(trackPath)) continue;
                    else if(!fromSaved && !ust.IsMerged && !File.Exists(Path.Combine(path, trackPath))) continue;
                    var d = clips.Where(x => x.Value.name == "[UST] " + Path.GetFileNameWithoutExtension(trackPath));
                    if(d.Count() > 0)
                    {
                        if(level.Key == "global")
                        {
                            if(clips.ContainsKey(trackPart)) continue;
                            clips.Add(trackPart, d.First().Value);
                            Logging.Log($"Adding clip {d.First().Value.name}", Color.green);
                        }
                        else
                        {
                            if(clips.ContainsKey($"{level.Key}:{trackPart}")) continue;
                            clips.Add($"{level.Key}:{trackPart}", d.First().Value);
                            Logging.Log($"Adding clip {level.Key + ":" + trackPart}", Color.green);
                        }
                        continue;
                    }
                    AudioClip clip = Loader.LoadClipFromPath(trackPath);
                    if(clip != null)
                    {
                        if(level.Key == "global")
                        {
                            if(clips.ContainsKey(trackPart)) continue;
                            clips.Add(trackPart, clip);
                            Logging.Log($"Adding clip {clip.name}", Color.green);
                        }
                        else
                        {
                            if(clips.ContainsKey($"{level.Key}:{trackPart}")) continue;
                            clips.Add($"{level.Key}:{trackPart}", clip);
                            Logging.Log($"Adding clip {level.Key + ":" + trackPart}", Color.green);
                        }
                    }
                    else Logging.Log("Something went wrong with this clip", Color.red);
                }
            }
            if(clips.Count > 0)
            {
                CustomUST.Clear();
                CustomUST = clips;
            }
        }
        public static void SaveUST()
        {
            if(USTSelectionScreen.PersistentEntries.Count > 0 && USTSelectionScreen.CurrentUST != null)
            {
                USTSave data = new()
                {
                    Selected = USTSelectionScreen.PersistentEntries,
                    UST = USTSelectionScreen.CurrentUST
                };
                File.WriteAllText(Plugin.LastUSTs, JsonConvert.SerializeObject(data, Formatting.Indented));//.Replace(@"\\","/"));
            }
        }
        public static IEnumerable<FileInfo> GetUstsInPlugins()
        {
            DirectoryInfo plugins = new FileInfo(typeof(Plugin).Assembly.Location).Directory.Parent;
            if(plugins.Exists)
            {
                return plugins.GetFiles("*.ust", SearchOption.AllDirectories).Concat(plugins.GetFiles("*.ust.json", SearchOption.AllDirectories));
            }
            else
            {
                return Enumerable.Empty<FileInfo>();
            }
        }
        public class USTSave { public List<string> Selected; public CustomUST UST; }
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
                if(source.playOnAwake)
                {
                    source.Play();
                }
                return;
            }

            level = level.Replace("Level ", "");
            string? key = level switch
            {
                "0-5" => source.clip.name switch
                {
                    "Cerberus A" => "boss1",
                    "Cerberus B" => "boss2",
                    _ => null
                },
                "1-1" => HeartOfTheSunrise(source.clip.name),
                "1-2" => TheBurningWorld(source.clip.name, source.name == "CleanTheme"),
                "1-4" => source.name switch
                {
                    "Music - Clair de Lune" => "piano", // Clair_de_lune_(Claude_Debussy)_Suite_bergamasque (CREATIVE COMMONS)
                    "SlowMo" => "intro",                // V2 Intro
                    "Music - Versus" => "boss",         // V2 1-4
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
                    "Tanpura Drone" => "drone",
                    "V2 Intro" => source.pitch < 1 ? "bassline" : "intro",
                    "V2 4-4" => "boss",
                    "Versus2Outro" => "outro",
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
                "7-4" => source.clip.name switch
                {
                    "Centaur A-1" => "outside1", // Ground
                    "Centaur A-2" => "outside2", // Air
                    "Centaur A-3" => "outside3", // Front
                    "Centaur A-4" => "outside4", // House
                    "Centaur A-5" => "outside5", // SecurityBoss
                    "Centaur A-6" => "outside6", // Interlude

                    "Centaur B-1" => "inside1", // Alarm
                    "Centaur B-2" => "inside2", // BrainPath
                    "Centaur B-3" => "inside3", // BrainFight

                    "Centaur B-4" => "escape1", // Escape
                    "Centaur B-5" => "escape2", // EscapeFight
                    "Centaur B-6" => "escape3", // End

                    _ => null,
                },
                "P-1" => source.name switch
                {
                    "Sourire" => "intro1",    // Sourire d'avril
                    "Sourire 2" => "intro2",  // Sourire d'avril corrupted 2
                    "Sourire 3" => "intro3",  // Sourire d'avril corrupted 5
                    "Sourire 4" => "intro4",  // Sourire d'avril corrupted 6
                    "Chaos" => "boss1",       // Flesh Prison
                    "IntroMusic" => "speech", // Minos Prime Intro
                    "Music 3" => "boss2",     // Minos Prime
                    _ => null,
                },
                "P-2" => source.clip.name switch
                {
                    "Weihnachten Am Klavier Subtler" => "intro",
                    "Deep Drone 5B" => "weezer",
                    "P-2 Clean" => "clean",
                    "P-2" => "battle",
                    "Flesh panopticon" => "boss1",
                    "Sisyphus Prime Intro" => "speech",
                    "Sisyphus Prime" => "boss2",
                    _ => null,
                },

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
                if(pitchedTracks.Contains(key))
                {
                    source.pitch = 1f;
                }
            }
        }

        private static readonly HashSet<string> pitchedTracks = [
            "2-4:boss1",
            "2-4:boss2",
            "4-4:bassline",
            "7-1:boss2",
            // The simple approach doesn't work here: the game continuously varies this source's pitch between 0.95 and 1.0 in a triangle wave pattern.
            // Support for disabling this effect TBD - it could be desirable.
            //"P-2:intro",
        ];

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
