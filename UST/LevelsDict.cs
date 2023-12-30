using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace USTManager.Data
{
    internal class LevelsDict
    {
        private static readonly Dictionary<string, LevelAudioDescriptor> AudioSources = new()
        {
            {"default", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },

            {"0-1", new("0-1 Clean","0-1","0-1", "EXTRA") },
            {"0-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"0-3", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"0-4", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"0-5", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
                   
            {"1-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"1-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"1-3", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"1-4", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
                
            {"2-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"2-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"2-3", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"2-4", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
                
            {"3-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"3-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
                    

            {"4-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"4-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"4-3", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"4-4", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
                   
            {"5-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"5-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"5-3", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"5-4", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
                   
            {"6-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"6-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },


            {"7-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"7-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"7-3", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"7-4", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            
            {"8-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"8-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"8-3", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"8-4", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
             
            {"9-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"9-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            

            {"0-S", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"1-S", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"2-S", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"4-S", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"5-S", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"7-S", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"8-S", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            
            {"P-1", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"P-2", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
            {"P-3", new("CleanTheme","BattleTheme","BossTheme", "EXTRA") },
        };
        public struct LevelAudioDescriptor
        {
            public string clean, battle, boss;
            public List<string> extras = new();
            public LevelAudioDescriptor(string clean, string battle, string boss)
            {
                this.clean = clean;
                this.battle = battle;
                this.boss = boss;
            }
            public LevelAudioDescriptor(string clean, string battle, string boss, List<string> extras)
            {
                this.clean = clean;
                this.battle = battle;
                this.boss = boss;
                this.extras = extras;
            }
            // using params
            public LevelAudioDescriptor(string clean, string battle, string boss, params string[] extras)
            {
                this.clean = clean;
                this.battle = battle;
                this.boss = boss;
                this.extras = new List<string>(extras);
            }
        }
    }
}
