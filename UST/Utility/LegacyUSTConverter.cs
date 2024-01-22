using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using USTManager.Data;

namespace USTManager.Utility
{
    public class LegacyUSTConverter
    {
        public static List<LegacyUST> legacyUSTs = new();
        public static bool IsLegacyUST(string path)
        {
            string data = File.ReadAllText(path);
            try
            {
                LegacyUST obj = JsonConvert.DeserializeObject<LegacyUST>(data);
                obj.Path = path;
                legacyUSTs.Add(obj);
                return true;
            }
            catch(JsonException ex)
            {
                Logging.LogError(ex);
                return false;
            }
        }
        public static void ConvertLegacyUSTs()
        {
            foreach(LegacyUST ust in legacyUSTs)
            {
                CustomUST newUst = new(ust.Name, ust.Author, ust.Description);
                foreach(KeyValuePair<string, LegacyUSTDescriptor[]> level in ust.levels)
                {
                    Dictionary<string, string> newLevel = new();
                    foreach(LegacyUSTDescriptor descriptor in level.Value)
                    {
                        newLevel.Add(descriptor.Part, descriptor.Path);
                    }
                    newUst.Levels.Add(level.Key, newLevel);
                }
                string json = JsonConvert.SerializeObject(newUst, Formatting.Indented);
                File.WriteAllText(ust.Path, json);
            }
            legacyUSTs.Clear();
        }
        public struct LegacyUST
        {
            public string Name, Author, Description;
            [JsonIgnore] public string Path;
            public Dictionary<string, LegacyUSTDescriptor[]> levels;
        }
        public struct LegacyUSTDescriptor
        {
            public string Part, Path;
        };
    }
}