using GameConsole;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using USTManager.Utility;

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
            Logging.Log("UST is now " + Manager.IsEnabled);
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
            if(args.Length > 0)
            {
                if(bool.TryParse(args[0].ToLower(), out bool result))
                {
                    if(args.Length > 1)
                    {
                        if(args[1].ToLower() == "extended")
                        {
                            Manager.IsDebug = result;
                            Manager.IsExtendedDebug = result ? true : false;
                            Logging.Log("UST Debug is now " + (Manager.IsDebug ? "enabled" : "disabled") + (Manager.IsExtendedDebug ? " in extended mode" : ""));
                        }
                    }
                    else
                    {
                        Manager.IsDebug = result;
                        Manager.IsExtendedDebug = result ? false : Manager.IsExtendedDebug;
                        Logging.Log("UST Debug is now " + (Manager.IsDebug ? "enabled" : "disabled")); 
                    }
                }
                else Logging.Log($"Incorrect argument \"{args[0]}\"");
            }
            else Logging.Log($"Not enough arguments");
        }
    }
}
