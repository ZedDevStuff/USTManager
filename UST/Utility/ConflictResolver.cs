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
                            if(others.Levels.ContainsKey(level.Key))
                            {
                                if(!conflicts.ContainsKey(level.Key))
                                {
                                    conflicts.Add(level.Key, [ust]);
                                }
                                else
                                {
                                    conflicts[level.Key].Add(ust);
                                }
                            }
                            else
                            {
                                List<CustomUST.Descriptor> parts = new();
                                foreach(CustomUST.Descriptor part in level.Value)
                                {
                                    parts.Add(new(part.Part, Path.Combine(ust.Path, part.Path)));
                                }
                                merged.Levels.Add(level.Key, parts);
                            }
                        }
                    }
                    else
                    {
                        foreach(CustomUST other in USTs.Where(x => x != ust))
                        {
                            if(!other.Levels.ContainsKey("global")) continue;
                            else
                            {
                                foreach(var entry in level.Value)
                                {
                                    if(other.Levels["global"].Where(x => x.Part == entry.Part).Count() > 0)
                                    {
                                        if(!conflicts.ContainsKey($"global:{entry.Part}"))
                                        {
                                            conflicts.Add($"global:{entry.Part}", [ust]);
                                        }
                                        else
                                        {
                                            conflicts[$"global:{entry.Part}"].Add(ust);
                                        }
                                    }
                                    else
                                    {
                                        if(!merged.Levels.ContainsKey("global"))
                                        {
                                            merged.Levels.Add("global", [new(entry.Part, Path.Combine(ust.Path, entry.Path))]);
                                        }
                                        else merged.Levels["global"].Add(new(entry.Part, Path.Combine(ust.Path, entry.Path)));
                                    }
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
                            original.Levels["global"].AddRange(entry.Value.Levels["global"]);
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