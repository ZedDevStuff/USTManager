using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

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
        [JsonProperty("levels", Order = 3)]
        public Dictionary<string, Dictionary<string, string>> Levels = [];

        [JsonIgnore] public string Path;
        [JsonIgnore] public bool IsMerged = false;
        [JsonIgnore] public Color UserColor = Color.white;
        [JsonIgnore] public Sprite Icon = null;
        [JsonIgnore] public string Hash = "";

        public CustomUST() { }
        public CustomUST(string name, string author, string description)
        {
            Name = name;
            Author = author;
            Description = description;
        }

        public static string GetTemplateJson()
        {
            Dictionary<string, string> standard = TemplateLevel("clean", "battle");
            Dictionary<string, string> standardTwo = TemplateLevel("clean1", "clean2", "battle1", "battle2");

            CustomUST ust = new("Name", "Author", "Description")
            {
                Levels = {
                    {
                        "comments", new()
                        {
                            { "Comment", "This is an example UST file. It will get regenerated each time you launch the game with USTManager installed. Omit any unused parts." }
                        }
                    },
                    {
                        "global", new()
                        {
                            { "audioClipName", "relative/path/to/audio" }
                        }
                    },


                    // Prelude
                    { "0-0", standard },
                    { "0-1", standard },
                    { "0-2", standard },
                    { "0-3", standard },
                    { "0-4", standard },
                    { "0-5", TemplateLevel("boss1", "boss2") },

                    // Act I
                    { "1-1", TemplateLevel("clean1", "clean2", "battle") },
                    { "1-2", TemplateLevel("clean0", "clean1", "clean2", "battle0", "battle1", "battle2") },
                    { "1-3", TemplateLevel("clean", "battle", "boss") },
                    { "1-4", TemplateLevel("piano", "intro", "boss") },
                    { "2-1", standard },
                    { "2-2", standard },
                    { "2-3", standard },
                    { "2-4", TemplateLevel("boss1", "boss2") },
                    { "3-1", standardTwo },
                    { "3-2", TemplateLevel("intro1", "intro2", "boss") },

                    // Act II
                    { "4-1", standard },
                    { "4-2", standard },
                    { "4-3", TemplateLevel("clean1", "clean2", "clean3", "battle1", "battle2", "battle3") },
                    { "4-4", TemplateLevel("drone", "bassline", "intro", "boss", "outro") },
                    { "5-1", standard },
                    { "5-2", TemplateLevel("boss1", "boss2", "boss3") },
                    { "5-3", standardTwo },
                    { "5-4", TemplateLevel("boss1", "boss2") },
                    { "6-1", TemplateLevel("clean", "battle", "boss") },
                    { "6-2", TemplateLevel("boss") },

                    // Act III
                    { "7-1", TemplateLevel("clean", "battle", "boss1", "boss2") },
                    { "7-2", standardTwo },
                    { "7-3", standardTwo },
                    { "7-4", TemplateLevel("outside1", "outside2", "outside3", "outside4", "outside5", "outside6",
                                           "inside1", "inside2", "inside3",
                                           "escape1", "escape2", "escape3") },

                    // Prime Sanctums
                    { "P-1", TemplateLevel("intro1", "intro2", "intro3", "intro4", "boss1", "speech", "boss2") },
                    { "P-2", TemplateLevel("intro", "weezer", "clean", "battle", "boss1", "speech", "boss2") },

                    // TODO: Fraud
                    // TODO: Treachery
                    // TODO: P-3
                    // TODO: Secret levels?
                }
            };
            return JsonConvert.SerializeObject(ust, Formatting.Indented);
        }

        private static Dictionary<string, string> TemplateLevel(params string[] tracks)
        {
            Dictionary<string, string> level = [];
            foreach(string track in tracks)
            {
                level[track] = "relative/path/to/audio";
            }
            return level;
        }

        public override bool Equals(object obj)
        {
            if(obj is not CustomUST ust) return false;

            if(ust.Name != Name) return false;
            if(ust.Author != Author) return false;
            if(ust.Description != Description) return false;

            if(ust.Levels.Count != Levels.Count) return false;
            foreach(string level in ust.Levels.Keys)
            {
                if(!Levels.ContainsKey(level)) return false;
                Dictionary<string, string>
                    myParts = Levels[level],
                    ustParts = ust.Levels[level];

                if(ustParts.Count != myParts.Count) return false;
                foreach(string part in ustParts.Keys)
                {
                    if(!myParts.ContainsKey(part)) return false;
                    if(ustParts[part] != myParts[part]) return false;
                }
            }

            return true;
        }
        public override int GetHashCode() => throw new NotImplementedException();
    }
}