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

        // This is only used because its faster than a 20+ if chain
        private static Dictionary<string, Action<AudioSource, AudioClip>> Handlers = new()
        {
            {"0-1",(source, clip) => DefaultHandler("0-1", source)},
            {"0-2",(source, clip) => DefaultHandler("0-2", source)},
            {"0-3",(source, clip) => DefaultHandler("0-3", source)},
            {"0-4",(source, clip) => DefaultHandler("0-4", source)},
            {"0-5",Handle0_5},
            {"1-3",(source, clip) => DefaultHandler("1-3", source)},
            {"1-4",Handle1_4},
            {"2-1",(source, clip) => DefaultHandler("2-1", source)},
            {"2-2",(source, clip) => DefaultHandler("2-2", source)},
            {"2-3",(source, clip) => DefaultHandler("2-3", source)},
            {"2-4",Handle2_4},
            {"3-1",Handle3_1},
            {"3-2",Handle3_2},
            {"4-1",(source, clip) => DefaultHandler("4-1", source)},
            {"4-2",(source, clip) => DefaultHandler("4-2", source)},
            {"4-4",Handle4_4},
            {"5-1",(source, clip) => DefaultHandler("5-1", source)},
            {"5-2",Handle5_2},
            {"5-3",Handle5_3},
            {"5-4",Handle5_4},
            {"6-1",Handle6_1},
            {"6-2",Handle6_2},
            {"7-1",Handle7_1},
            {"7-2",Handle7_2},
            {"7-3",Handle7_3},
            {"7-4",Handle7_4},
            {"P-1",HandleP_1},
            {"P-2",HandleP_2},
        };
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
            if(Handlers.ContainsKey(actualLevel)) Handlers[actualLevel](source, clip);
            return;
        }
        public static void DefaultHandler(string level, AudioSource source)
        {
            if(source.name == "CleanTheme")
            {
                if(!CustomUST.ContainsKey(level + ":clean")) return;
                else source.clip = CustomUST[level + ":clean"];
            }
            else if(source.name == "BattleTheme")
            {
                if(!CustomUST.ContainsKey(level + ":battle")) return;
                else source.clip = CustomUST[level + ":battle"];
            }
            else if(source.name == "BossTheme")
            {
                if(!CustomUST.ContainsKey(level + ":boss"))
                {
                    if(!CustomUST.ContainsKey(level + ":battle")) return;
                    else source.clip = CustomUST[level + ":battle"];
                }
                else source.clip = CustomUST[level + ":boss"];
            }
        }
        public static void Handle0_5(AudioSource source, AudioClip clip)
        {
            if(source.clip.name == "Cerberus A")
            {
                if(!CustomUST.ContainsKey("0-5:boss1")) return;
                source.clip = CustomUST["0-5:boss1"];
            }
            if(source.clip.name == "Cerberus B")
            {
                if(!CustomUST.ContainsKey("0-5:boss2")) return;
                source.clip = CustomUST["0-5:boss2"];
            }
        }
        public static void Handle1_4(AudioSource source, AudioClip clip)
        {
            if(source.clip.name == "V2 Intro")
            {
                if(!CustomUST.ContainsKey("1-4:intro")) return;
                source.clip = CustomUST["1-4:intro"];
            }
            else if(source.clip.name == "V2 1-4")
            {
                if(!CustomUST.ContainsKey("1-4:boss")) return;
                source.clip = CustomUST["1-4:boss"];
            }
        }
        public static void Handle2_4(AudioSource source, AudioClip clip)
        {
            if(source.clip.name == "Minos Corpse A")
            {
                if(!CustomUST.ContainsKey("2-4:boss1")) return;
                source.clip = CustomUST["2-4:boss1"];
                source.pitch = 1f;
            }
            else if(source.clip.name == "Minos Corpse B")
            {
                if(!CustomUST.ContainsKey("2-4:boss2")) return;
                source.clip = CustomUST["2-4:boss2"];
                source.pitch = 1f;
            }
        }
        public static void Handle3_1(AudioSource source, AudioClip clip)
        {
            if(source.name == "CleanTheme")
            {
                if(source.clip.name == "3-1 Guts Clean")
                {
                    if(!CustomUST.ContainsKey("3-1:clean1")) return;
                    else source.clip = CustomUST["3-1:clean1"];
                }
                else if(source.clip.name == "3-1 Glory Clean")
                {
                    if(!CustomUST.ContainsKey("3-1:clean2")) return;
                    else source.clip = CustomUST["3-1:clean2"];
                }
            }
            else if(source.name == "BattleTheme" || source.name == "BossTheme")
            {
                if(source.clip.name == "3-1 Guts")
                {
                    if(!CustomUST.ContainsKey("3-1:battle1")) return;
                    else source.clip = CustomUST["3-1:battle1"];
                }
                else if(source.clip.name == "3-1 Glory")
                {
                    if(!CustomUST.ContainsKey("3-1:battle2")) return;
                    else source.clip = CustomUST["3-1:battle2"];
                }
            }
        }
        public static void Handle3_2(AudioSource source, AudioClip clip)
        {
            // Music 1 / Flesh Ambiance
            // Music 2 / Gabriel 3-2 Intro
            // Music 3 / Gabriel 3-2
            if(source.name == "Music 3")
            {
                if(!CustomUST.ContainsKey("3-2:boss")) return;
                source.clip = CustomUST["3-2:boss"];
            }
        }
        public static void Handle4_4(AudioSource source, AudioClip clip)
        {
            if(source.clip.name.Contains("V2 4-4"))
            {
                if(!CustomUST.ContainsKey("4-4:boss")) return;
                source.clip = CustomUST["4-4:boss"];
            }
        }
        public static void Handle5_2(AudioSource source, AudioClip clip)
        {
            if(source.clip.name == "Ferryman A")
            {
                if(!CustomUST.ContainsKey("5-2:boss1")) return;
                source.clip = CustomUST["5-2:boss1"];
            }
            else if(source.clip.name == "Ferryman B")
            {
                if(!CustomUST.ContainsKey("5-2:boss2")) return;
                source.clip = CustomUST["5-2:boss2"];
            }
            else if(source.clip.name == "Ferryman C")
            {
                if(!CustomUST.ContainsKey("5-2:boss3")) return;
                source.clip = CustomUST["5-2:boss3"];
            }
        }
        public static void Handle5_3(AudioSource source, AudioClip clip)
        {
            if(source.name == "CleanTheme")
            {
                if(source.clip.name.Contains("Aftermath"))
                {
                    // 5-3 Aftermath Intro
                    // 5-3 Aftermath Clean
                    if(!CustomUST.ContainsKey("5-3:clean2")) return;
                    source.clip = CustomUST["5-3:clean2"];
                }
                else
                {
                    // 5-3 Clean
                    if(!CustomUST.ContainsKey("5-3:clean1")) return;
                    source.clip = CustomUST["5-3:clean1"];
                }
            }
            else if(source.name == "BattleTheme" || source.name == "BossTheme")
            {
                if(source.clip.name.Contains("Aftermath"))
                {
                    // 5-3 Aftermath Intro
                    // 5-3 Aftermath
                    if(!CustomUST.ContainsKey("5-3:battle2")) return;
                    source.clip = CustomUST["5-3:battle2"];
                }
                else
                {
                    // 5-3
                    if(!CustomUST.ContainsKey("5-3:battle1")) return;
                    source.clip = CustomUST["5-3:battle1"];
                }
            }
        }
        public static void Handle5_4(AudioSource source, AudioClip clip)
        {
            if(source.name == "Music 1")
            {
                // Leviathan A
                if(!CustomUST.ContainsKey("5-4:boss1")) return;
                source.clip = CustomUST["5-4:boss1"];
            }
            if(source.name == "Music 2")
            {
                // Leviathan B
                if(!CustomUST.ContainsKey("5-4:boss2")) return;
                source.clip = CustomUST["5-4:boss2"];
            }
        }
        public static void Handle6_1(AudioSource source, AudioClip clip)
        {
            if(source.name == "CleanTheme")
            {
                // CleanTheme / 6-1 Clean
                if(source.clip.name.Contains("6-1"))
                {
                    if(!CustomUST.ContainsKey("6-1:clean")) return;
                    source.clip = CustomUST["6-1:clean"];
                }
                // CleanTheme / Deep Drone 1
            }
            else if(source.name == "BattleTheme")
            {
                // BattleTheme / 6-1
                if(source.clip.name.Contains("6-1"))
                {
                    if(!CustomUST.ContainsKey("6-1:battle")) return;
                    source.clip = CustomUST["6-1:battle"];
                }
                // BattleTheme / Deep Drone 3B
            }
            else if(source.name == "BossTheme")
            {
                if(source.clip.name.Contains("6-1"))
                {
                    // BossTheme / 6-1
                    if(!CustomUST.ContainsKey("6-1:battle")) return;
                    source.clip = CustomUST["6-1:battle"];
                }
                // BossTheme / Deep Drone 3
            }
            else if(source.name == "ClimaxMusic")
            {
                // ClimaxMusic / 6-1 Hall of Sacreligious Remains
                if(!CustomUST.ContainsKey("6-1:boss")) return;
                source.clip = CustomUST["6-1:boss"];
            }
        }
        public static void Handle6_2(AudioSource source, AudioClip clip)
        {
            if(source.name == "BossMusic")
            {
                // BossMusic / Gabriel 6-2
                if(!CustomUST.ContainsKey("6-2:boss")) return;
                source.clip = CustomUST["6-2:boss"];
            }
        }
        public static void Handle7_1(AudioSource source, AudioClip clip)
        {
            // Wind / WindLoop
            // IntroMusic / 7-1 Intro
            // MinotaurIntroMusic / MinotaurIntro
            if(source.name == "CleanTheme")
            {
                // 7-1 Clean
                if(!CustomUST.ContainsKey("7-1:clean")) return;
                source.clip = CustomUST["7-1:clean"];
            }
            else if(source.name == "BattleTheme")
            {
                // 7-1
                if(!CustomUST.ContainsKey("7-1:battle")) return;
                source.clip = CustomUST["7-1:battle"];
            }
            else if(source.clip.name == "Minotaur A")
            {
                // MinotaurPhase1Music / Minotaur A
                if(!CustomUST.ContainsKey("7-1:boss1")) return;
                source.clip = CustomUST["7-1:boss1"];
            }
            else if(source.clip.name == "Minotaur B")
            {
                // MinotaurPhase2Music / Minotaur B
                if(!CustomUST.ContainsKey("7-1:boss2")) return;
                source.clip = CustomUST["7-1:boss2"];
                source.pitch = 1f;
            }
        }
        public static void Handle7_2(AudioSource source, AudioClip clip)
        {
            if(source.name == "CleanTheme")
            {
                if(source.clip.name.Contains("Intro"))
                {
                    // 7-2 Intro Clean
                    if(!CustomUST.ContainsKey("7-2:clean1")) return;
                    source.clip = CustomUST["7-2:clean1"];
                }
                else
                {
                    // 7-2 Clean
                    if(!CustomUST.ContainsKey("7-2:clean2")) return;
                    source.clip = CustomUST["7-2:clean2"];
                }
            }
            else if(source.name == "BattleTheme" || source.name == "BossTheme")
            {
                if(source.clip.name.Contains("Intro"))
                {
                    // 7-2 Intro Battle
                    if(!CustomUST.ContainsKey("7-2:battle1")) return;
                    source.clip = CustomUST["7-2:battle1"];
                }
                else
                {
                    // 7-2
                    if(!CustomUST.ContainsKey("7-2:battle2")) return;
                    source.clip = CustomUST["7-2:battle2"];
                }
            }
        }
        public static void Handle7_3(AudioSource source, AudioClip clip)
        {
            // Wind / WindLoopHaunted
            // Drone3 / MollyCorrupted4
            if(source.name == "CleanTheme")
            {
                if(source.clip.name.Contains("Intro"))
                {
                    // 7-3 Intro Clean
                    if(!CustomUST.ContainsKey("7-3:clean1")) return;
                    source.clip = CustomUST["7-3:clean1"];
                }
                else
                {
                    // 7-3 Clean
                    if(!CustomUST.ContainsKey("7-3:clean2")) return;
                    source.clip = CustomUST["7-3:clean2"];
                }
            }
            else if(source.name == "BattleTheme" || source.name == "BossTheme")
            {
                if(source.clip.name.Contains("Intro"))
                {
                    // 7-3 Intro Clean
                    if(!CustomUST.ContainsKey("7-3:battle1")) return;
                    source.clip = CustomUST["7-3:battle1"];
                }
                else
                {
                    // 7-3
                    if(!CustomUST.ContainsKey("7-3:battle2")) return;
                    source.clip = CustomUST["7-3:battle2"];
                }
            }
        }
        public static void Handle7_4(AudioSource source, AudioClip clip)
        {
            // not currently supported
            return;
        }
        public static void HandleP_1(AudioSource source, AudioClip clip)
        {
            // Sourire / Sourire d'avril
            if(source.name == "Chaos")
            {
                // Chaos / Flesh Prison
                if(!CustomUST.ContainsKey("P-1:boss1")) return;
                source.clip = CustomUST["P-1:boss1"];
            }
            if(source.clip.name.Contains("Minos"))
            {
                // IntroMusic / Minos Prime Intro
                // Music 3 / Minos Prime
                if(!CustomUST.ContainsKey("P-1:boss2")) return;
                source.clip = CustomUST["P-1:boss2"];
            }
        }
        public static void HandleP_2(AudioSource source, AudioClip clip)
        {
            // not currently supported
            return;
        }

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
