﻿using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace SyncManager.IPCMessages
{
    [AoContract((int)IPCOpcode.StopAttack)]
    public class StopAttackIPCMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.StopAttack;
    }
}
