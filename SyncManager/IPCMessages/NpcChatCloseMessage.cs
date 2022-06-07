﻿using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SyncManager.IPCMessages
{
    [AoContract((int)IPCOpcode.NpcChatClose)]
    public class NpcChatCloseMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.NpcChatClose;

        [AoMember(0)]
        public Identity Target { get; set; }
    }
}