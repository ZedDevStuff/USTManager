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
                Levels = new Dictionary<string, List<CustomUST.Descriptor>>()
                {
                    {"levels", new()},
                    {"global", new()}
                }
            };
            Debug.WriteLine($"Merging {USTs.Length} USTs");
            for(int i = 0;i < USTs.Length;i++)
            {
                foreach(var level in USTs[i].Levels)
                {
                    if(level.Key != "levels" && level.Key != "global") continue;
                    if(level.Key == "global")
                    {
                        foreach(var descriptor in level.Value)
                        {
                            if(merged.Levels["global"].Where(x => x.Part == descriptor.Part).Count() > 0)
                            {
                                if(!conflicts.ContainsKey($"global:{descriptor.Part}")) conflicts.Add($"global:{descriptor.Part}", new() {USTs[i]});
                                else conflicts[$"global:{descriptor.Part}"].Add(USTs[i]);
                            }
                            else
                            {
                                merged.Levels["global"].Add(descriptor);
                            }
                        }
                    }
                    else
                    {
                        if(merged.Levels.ContainsKey(level.Key))
                        {
                            if(!conflicts.ContainsKey(level.Key)) conflicts.Add(level.Key, new() {USTs[i]});
                            else conflicts[level.Key].Add(USTs[i]);
                        }
                        else
                        {
                            merged.Levels.Add(level.Key, new());
                            foreach(var descriptor in level.Value)
                            {
                                string fullPath = Path.Combine(USTs[i].Path, descriptor.Path);
                                Debug.WriteLine(fullPath);
                                merged.Levels[level.Key].Add(new(descriptor.Part, fullPath));
                            }
                        }
                    }
                }
            }
            Debug.WriteLine(conflicts.Count);
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
                Debug.WriteLine("Validated");
                UST = original;
                return true;
            }
            else
            {
                Debug.WriteLine("Not Validated");
                UST = null;
                return false;
            }
        }
    }
}