﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpManager.IPCMessages
{
    public enum IPCOpcode
    {
        YalmOn = 2100,
        YalmUse = 2101,
        YalmOff = 2102,
        ClearBuffs = 2103
    }
}
