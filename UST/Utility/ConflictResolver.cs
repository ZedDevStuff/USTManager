using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
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
                                if(conflicts.ContainsKey(level.Key))
                                {
                                    conflicts.Add(level.Key, [ust]);
                                }
                                else
                                {
                                    conflicts[level.Key].Add(ust);
                                }
                                break;
                            }
                            else
                            {
                                List<CustomUST.Descriptor> parts = new();
                                foreach(CustomUST.Descriptor part in level.Value)
                                {
                                    parts.Add(new(part.Part, Path.Combine(ust.Path,part.Path))); 
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
                                        if(conflicts.ContainsKey($"global:{entry.Part}"))
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
                                        if(merged.Levels.ContainsKey("global"))
                                        {
                                            merged.Levels["global"].Add(new(entry.Part,Path.Combine(ust.Path,entry.Path)));
                                        }
                                        else merged.Levels.Add("global",[new(entry.Part,Path.Combine(ust.Path,entry.Path))]);
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
        public Dictionary<string,List<CustomUST>> Conflicts = new();
        public Dictionary<string,CustomUST> Merged = new();

        public Conflict(CustomUST original, Dictionary<string,List<CustomUST>> conflicts)
        {
            this.original = original;
            Conflicts = conflicts;
            foreach(var conflict in conflicts)
            {
                Merged.Add(conflict.Key, conflict.Value[0]);
            }
        }
        public bool Validate(out CustomUST UST)
        {
            if(Merged.Count == Conflicts.Count)
            {
                UST = original;
                return true;
            }
            else
            {
                UST = null;
                return false;
            }
        }
    }
}