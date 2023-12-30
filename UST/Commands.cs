using GameConsole;
using System;
using System.Collections.Generic;
using System.Text;

namespace USTManager.Commands
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
            Manager.IsEnabled = !Manager.IsEnabled;
            con.PrintLine("UST is now " + Manager.IsEnabled);
        }
    }
    public class USTDebug : ICommand
    {
        public string Name
        {
            get
            {
                return "ust.debug";
            }
        }

        public string Description
        {
            get
            {
                return "Toggle UST Debug";
            }
        }
        public string Command
        {
            get
            {
                return "ust.debug";
            }
        }
        public void Execute(GameConsole.Console con, string[] args)
        {
            Manager.IsDebug = !Manager.IsDebug;
            con.PrintLine("UST Debug is now " + Manager.IsDebug);
        }
    }
}
