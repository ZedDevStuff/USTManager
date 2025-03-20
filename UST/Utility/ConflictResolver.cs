using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using USTManager.Data;

namespace USTManager.Utility
{
    public static class ConflictResolver
    {
        public static Conflict Merge(IEnumerable<CustomUST> usts)
        {
            // Associate level names (or "global:soundeffect" keys) with USTs that override them.
            // A "conflict" manifests as a list with two or more elements.
            Dictionary<string, List<CustomUST>> sourceDict = [];

            foreach(var ust in usts)
            {
                foreach(var (levelName, tracks) in ust.Levels)
                {
                    if(levelName == "global")
                    {
                        // Global overrides conflict based on individual tracks
                        foreach(var trackName in tracks.Keys)
                        {
                            var globalKey = "global:" + trackName;
                            sourceDict.GetOrAdd(globalKey, []).Add(ust);
                        }
                    }
                    else
                    {
                        // Level overrides conflict based on the level as a whole
                        sourceDict.GetOrAdd(levelName, []).Add(ust);
                    }
                }
            }

            Dictionary<string, List<CustomUST>> conflicts = []; // The overrides with conflicts
            CustomUST merged = new() { IsMerged = true };       // The overrides without conflicts

            foreach(var (key, sources) in sourceDict)
            {
                if(sources.Count != 1)
                {
                    conflicts.Add(key, sources);
                    continue;
                }

                var ust = sources[0];
                if(key.StartsWith("global:"))
                {
                    var trackName = key.Substring(7);
                    var track = ust.Levels["global"][trackName];
                    merged.Levels.GetOrAdd("global", []).Add(trackName, track);
                }
                else
                {
                    merged.Levels.Add(key, ust.Levels[key]);
                }
                Logging.Log($"Merged {key} from {ust.Name}");
            }

            Debug.WriteLine($"{conflicts.Count} Conflicts");
            return new Conflict(merged, conflicts);
        }
    }

    public sealed class Conflict
    {
        [JsonProperty("UST")]
        private readonly CustomUST mergedUST;
        [JsonProperty("Conflicts")]
        private readonly Dictionary<string, List<CustomUST>> conflicts;
        [JsonProperty("Resolutions")]
        private readonly Dictionary<string, CustomUST> resolutions;
        [JsonProperty("Validated")]
        public bool Validated = false;

        [JsonConstructor]
        private Conflict()
        {

        }
        [JsonIgnore]
        public IReadOnlyDictionary<string, List<CustomUST>> Conflicts => conflicts;

        public int ConflictCount => conflicts.Count;
        public int SolvedCount => resolutions.Count;

        public Conflict(CustomUST ust, Dictionary<string, List<CustomUST>> conflictingKeys)
        {
            mergedUST = ust;
            conflicts = conflictingKeys;
            resolutions = [];
        }

        public void Resolve(string key, CustomUST UST)
        {
            if(conflicts.ContainsKey(key) && conflicts[key].Contains(UST))
            {
                Logging.Log($"Resolved conflict for {key} with {UST.Name}");
                resolutions[key] = UST;
            }
        }

        public bool Validate(out CustomUST UST)
        {
            if(Validated)
            {
                UST = mergedUST;
                return true;
            }
            if(ConflictCount > SolvedCount)
            {
                Logging.Log("Conflicts not solved");
                UST = null;
                return false;
            }

            Logging.Log("No remaining conflicts");

            foreach(var (key, chosenUST) in resolutions)
            {
                if(key.StartsWith("global:"))
                {
                    var trackName = key.Substring(7);
                    var track = chosenUST.Levels["global"][trackName];
                    mergedUST.Levels.GetOrAdd("global", []).Add(trackName, track);
                }
                else
                {
                    mergedUST.Levels.Add(key, chosenUST.Levels[key]);
                }
            }
            Validated = true;
            UST = mergedUST;
            return true;
        }
        /// <summary>
        /// Will fail and return null if the conflict isn't resolved
        /// </summary>
        /// <returns></returns>
        public CustomUST GetMerged()
        {
            if(Validated) return mergedUST;
            return null;
        }
    }
}