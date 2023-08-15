﻿using AOSharp.Core;
using AOSharp.Core.UI;
using System;

namespace CombatHandler.Trader
{
    public class Main : AOPluginEntry
    {
        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Trader Combat Handler Loaded!");
                Chat.WriteLine("/handler for settings.");
                AOSharp.Core.Combat.CombatHandler.Set(new TraderCombatHandler(pluginDir));
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
