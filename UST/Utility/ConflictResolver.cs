using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using USTManager.Data;

namespace USTManager.Utility
{
    public class ConflictResolver
    {
        public static Conflict Merge(params CustomUST[] USTs)
        {
            Dictionary<string, List<CustomUST>> conflicts = new();
            CustomUST merged = new CustomUST()
            {
                IsMerged = true,
                Levels = new()
            };
            foreach(CustomUST ust in USTs)
            {
                foreach(var level in ust.Levels)
                {
                    if(level.Key != "global")
                    {
                        foreach(CustomUST others in USTs.Where(x => x != ust))
                        {
                            if(!others.Levels.ContainsKey(level.Key)) continue;
                            if(merged.Levels.ContainsKey(level.Key)) continue;
                            List<CustomUST> queryResult = USTs.Where(x => x.Levels.ContainsKey(level.Key)).ToList();
                            if(queryResult.Count == 1)
                            {
                                merged.Levels.Add(level.Key, level.Value);
                            }
                            else if(queryResult.Count != 0)
                            {
                                if(conflicts.ContainsKey(level.Key)) continue;
                                else conflicts.Add(level.Key, queryResult);
                            }
                        }
                    }
                    else
                    {
                        foreach(CustomUST other in USTs.Where(x => x != ust))
                        {
                            if(!other.Levels.ContainsKey("global")) continue;
                            if(!merged.Levels.ContainsKey("global")) merged.Levels.Add("global", []);

                            foreach(var entry in level.Value)
                            {
                                if(merged.Levels.ContainsKey("global"))
                                {
                                    if(merged.Levels["global"].ContainsKey(entry.Key)) continue;
                                }
                                var queryResult = USTs.Where(x => 
                                {
                                    if(x.Levels.ContainsKey("global"))
                                    {
                                        if(x.Levels["global"].ContainsKey(entry.Key)) return true;
                                        else return false;
                                    }
                                    else return false;
                                }).ToList();
                                if(queryResult.Count == 1)
                                {
                                    merged.Levels["global"].Add(entry.Key, entry.Value);
                                }
                                else if(queryResult.Count != 0)
                                {
                                    if(conflicts.ContainsKey($"global:{entry.Key}")) continue;
                                    else conflicts.Add($"global:{entry.Key}", queryResult);
                                }
                            }
                        }
                    }
                }
            }
            Debug.WriteLine($"{conflicts.Count} Conflicts");
            Conflict conflict = new Conflict(merged, conflicts);
            return conflict;
        }
    }
    public class Conflict
    {
        private CustomUST original;
        public Dictionary<string, List<CustomUST>> Conflicts = new();
        private Dictionary<string, CustomUST> Merged = new();
        public int ConflictCount => Conflicts.Count;
        public int SolvedCount => Merged.Count;

        public Conflict(CustomUST original, Dictionary<string, List<CustomUST>> conflicts)
        {
            this.original = original;
            Conflicts = conflicts;
        }
        public void Resolve(string key, CustomUST UST)
        {
            if(Conflicts.ContainsKey(key))
            {
                if(Conflicts[key].Contains(UST))
                {
                    Logging.Log($"Resolved conflict for {key} with {UST.Name}");
                    if(Merged.ContainsKey(key)) Merged[key] = UST;
                    else Merged.Add(key, UST);

                }
            }
        }
        public bool Validate(out CustomUST UST)
        {
            if(ConflictCount == 0 || SolvedCount == ConflictCount)
            {
                Logging.Log("No conflicts");
                foreach(var entry in Merged)
                {
                    if(entry.Key.FastStartsWith("global:"))
                    {
                        if(original.Levels.ContainsKey("global"))
                        {
                            foreach(var kv in entry.Value.Levels["global"])
                            {
                                original.Levels["global"].Add(kv.Key, kv.Value);
                            }
                        }
                        else original.Levels.Add("global", entry.Value.Levels["global"]);
                    }
                    else original.Levels[entry.Key] = entry.Value.Levels[entry.Key];
                }
                UST = original;
                return true;
            }
            else
            {
                Logging.Log("Conflicts not solved");
                UST = null;
                return false;
            }
        }
    }
}