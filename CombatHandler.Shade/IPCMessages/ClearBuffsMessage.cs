﻿using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace CombatHandler.Shade
{
    [AoContract((int)IPCOpcode.ClearBuffs)]
    public class ClearBuffsMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.ClearBuffs;
    }
}
