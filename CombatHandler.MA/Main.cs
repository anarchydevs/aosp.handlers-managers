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
                Chat.WriteLine("MA Combat Handler Loaded!");
                CombatHandler.Set(new MACombatHandler());
            }
            catch(Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
