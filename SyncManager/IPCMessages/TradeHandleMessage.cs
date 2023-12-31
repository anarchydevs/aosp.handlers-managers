﻿using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace SyncManager.IPCMessages
{
    [AoContract((int)IPCOpcode.Trade)]
    public class TradeHandleMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Trade;

        [AoMember(0)]
        public int Unknown1 { get; set; }
        [AoMember(1)]
        public TradeAction Action { get; set; }
        [AoMember(2)]
        public Identity Target { get; set; }
        [AoMember(3)]
        public Identity Container { get; set; }
    }
}
