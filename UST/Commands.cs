using GameConsole;
using System;
using System.Collections.Generic;
using System.Text;

namespace USTManager
{
    public class USTToggle : ICommand
    {
        public string Name
        {
            get
            {
                return "ust.toggle";
            }
        }

        public string Description
        {
            get
            {
                return "Toggle UST";
            }
        }
        public string Command
        {
            get
            {
                return "ust.toggle";
            }
        }
        public void Execute(GameConsole.Console con, string[] args)
        {
            USTMusicManager.isEnabled = !USTMusicManager.isEnabled;
            con.PrintLine("UST is now " + USTMusicManager.isEnabled);
        }
    }
}
