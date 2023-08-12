﻿using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace SyncManager.IPCMessages
{
    [AoContract((int)IPCOpcode.UseItem)]
    public class UsableMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.UseItem;

        [AoMember(0)]
        public int ItemLowId { get; set; }

        [AoMember(1)]
        public int ItemHighId { get; set; }

        [AoMember(2)]
        public Identity Target { get; set; }
    }
}
