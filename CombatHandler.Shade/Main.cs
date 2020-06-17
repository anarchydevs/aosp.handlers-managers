﻿using AOSharp.Core;
using AOSharp.Core.Combat;
using System;

namespace Desu
{
    public class Main : IAOPluginEntry
    {
        public void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Shade Combat Handler Loaded!");
                CombatHandler.Set(new ShadeCombatHandler());
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
