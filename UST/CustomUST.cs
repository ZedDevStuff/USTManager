using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace USTManager.Data
{
    [Serializable]
    public class CustomUST
    {
        [JsonProperty(Order = 0)]
        public string Name { get; private set; }
        [JsonProperty(Order = 1)]
        public string Author { get; private set; }
        [JsonProperty(Order = 2)]
        public string Description { get; private set; }
        [JsonIgnore]
        public ReadOnlyDictionary<string, List<Descriptor>> Levels => new(_Levels);
        [JsonProperty("levels", Order = 3)]
        private Dictionary<string, List<Descriptor>> _Levels = new();
        [JsonIgnore]
        public string Path;

        public CustomUST(){}
        public CustomUST(string name, string author, string description)
        {
            Name = name;
            Author = author;
            Description = description;
        }

        public string GetTrack(string level, string part)
        {
            if (Levels.TryGetValue(level, out List<Descriptor> parts))
            {
                foreach (Descriptor d in parts)
                {
                    if (d.Part.Contains(part)) return d.Path;
                }
            }
            return null;
        }
        public string GetJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
        public static string GetTemplateJson()
        {
            CustomUST ust = new("Name","Author","Description");
            List<Descriptor> comments = new()
            {
                new("Comment","This is an example UST file. It will get regenerated each time you launch the game with USTManager installed. Omit any unused parts."),
            };

            ust._Levels.Add("global", new List<Descriptor>() { new("audioClipName","relative/path/to/audio") });
            
            // Act 1
            
            ust._Levels.Add("comments", comments);
            List<Descriptor> l0_1 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("0-1", l0_1);
            List<Descriptor> l0_2 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("0-2", l0_2);
            List<Descriptor> l0_3 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
                new("boss","relative/path/to/track")
            };
            ust._Levels.Add("0-3", l0_3);
            List<Descriptor> l0_4 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("0-4", l0_4);
            List<Descriptor> l0_5 = new()
            {
                new("boss1","relative/path/to/track"),
                new("boss2","relative/path/to/track"),
            };
            ust._Levels.Add("0-5", l0_5);
            List<Descriptor> l1_1 = new()
            {
                new("intro","relative/path/to/track"),
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("1-1", l1_1);
            List<Descriptor> l1_2 = new()
            {
                new("clean1","relative/path/to/track"),
                new("battle1","relative/path/to/track"),
                new("clean2","relative/path/to/track"),
                new("battle2","relative/path/to/track"),
            };
            ust._Levels.Add("1-2", l1_2);
            List<Descriptor> l1_3 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("1-3", l1_3);
            List<Descriptor> l1_4 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
                new("boss","relative/path/to/track"),
            };
            ust._Levels.Add("1-4", l1_4);
            List<Descriptor> l2_1 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("2-1", l2_1);
            List<Descriptor> l2_2 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("2-2", l2_2);
            List<Descriptor> l2_3 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("2-3", l2_3);
            List<Descriptor> l2_4 = new()
            {
                new("boss1","relative/path/to/track"),
                new("boss2","relative/path/to/track"),
            };
            ust._Levels.Add("2-4", l2_4);
            List<Descriptor> l3_1 = new()
            {
                new("clean1","relative/path/to/track"),
                new("battle1","relative/path/to/track"),
                new("clean2","relative/path/to/track"),
                new("battle2","relative/path/to/track"),
            };
            ust._Levels.Add("3-1", l3_1);
            List<Descriptor> l3_2 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("3-2", l3_2);
            
            // Act 2
            
            List<Descriptor> l4_1 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("4-1", l4_1);
            List<Descriptor> l4_2 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
                new("boss","relative/path/to/track"),
            };
            ust._Levels.Add("4-2", l4_2);
            List<Descriptor> l4_3 = new()
            {
                new("clean1","relative/path/to/track"),
                new("battle1","relative/path/to/track"),
                new("clean2","relative/path/to/track"),
                new("battle2","relative/path/to/track"),
                new("clean3","relative/path/to/track"),
                new("battle3","relative/path/to/track"),
            };
            ust._Levels.Add("4-3", l4_3);
            List<Descriptor> l4_4 = new()
            {
               new("boss","relative/path/to/track"),
            };
            ust._Levels.Add("4-4", l4_4);
            List<Descriptor> l5_1 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("5-1", l5_1);
            List<Descriptor> l5_2 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
                new("boss","relative/path/to/track"),
            };
            ust._Levels.Add("5-2", l5_2);
            List<Descriptor> l5_3 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("5-3", l5_3);
            List<Descriptor> l5_4 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("5-4", l5_4);
            List<Descriptor> l6_1 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("6-1", l6_1);
            List<Descriptor> l6_2 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("6-2", l6_2);
            
            // Act 3
            
            List<Descriptor> l7_1 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("7-1", l7_1);
            List<Descriptor> l7_2 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("7-2", l7_2);
            List<Descriptor> l7_3 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };
            ust._Levels.Add("7-3", l7_3);
            List<Descriptor> l7_4 = new()
            {
                new("clean","relative/path/to/track"),
                new("battle","relative/path/to/track"),
            };

            // Add last Act 3 levels when it comes out

            // P levels
            List<Descriptor> lP_1 = new()
            {
                new("boss1","relative/path/to/track"),
                new("boss2","relative/path/to/track"),
            };
            ust._Levels.Add("P-1", lP_1);

            return ust.GetJson();
        }
        public struct Descriptor
        {
            public string Part, Path;
            public Descriptor(string part, string path)
            {
                Part = part;
                Path = path;
            }
        }
    }
}