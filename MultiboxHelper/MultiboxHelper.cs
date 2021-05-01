﻿using System;
using System.Diagnostics;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Core.UI.Options;
using AOSharp.Common;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using MultiboxHelper.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Runtime.InteropServices;

namespace MultiboxHelper
{
    public class MultiboxHelper : AOPluginEntry
    {
        public static string PluginDir;

        private Menu _menu;
        private IPCChannel IPCChannel;
        private StatusWindow _statusWindow;
        private double _lastUpdateTime = 0;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private bool IsActiveWindow => GetForegroundWindow() == Process.GetCurrentProcess().MainWindowHandle;

        public override void Run(string pluginDir)
        {
            PluginDir = pluginDir;
            _statusWindow = new StatusWindow();

            IPCChannel = new IPCChannel(111);
            IPCChannel.RegisterCallback((int)IPCOpcode.Move, OnMoveMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Target, OnTargetMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Attack, OnAttackMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.StopAttack, OnStopAttackMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Use, OnUseMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.CharStatus, OnCharStatusMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.CharLeft, OnCharLeftMessage);

            Chat.RegisterCommand("mb", MbCommand);

            _menu = new Menu("MultiboxHelper", "MultiboxHelper");
            _menu.AddItem(new MenuBool("SyncMove", "Sync Movement", true));
            _menu.AddItem(new MenuBool("SyncAttack", "Sync Attacks", true));
            _menu.AddItem(new MenuBool("SyncUse", "Sync Use", true));
            OptionPanel.AddMenu(_menu);

            Game.OnUpdate += OnUpdate;
            Network.N3MessageSent += Network_N3MessageSent;
            Chat.WriteLine("Multibox Helper Loaded!");
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Time.NormalTime > _lastUpdateTime + 0.5f)
            {
                IPCChannel.Broadcast(new CharStatusMessage
                {
                    Name = DynelManager.LocalPlayer.Name,
                    Health = DynelManager.LocalPlayer.Health,
                    MaxHealth = DynelManager.LocalPlayer.MaxHealth,
                    Nano = DynelManager.LocalPlayer.Nano,
                    MaxNano = DynelManager.LocalPlayer.MaxNano,
                });

                _lastUpdateTime = Time.NormalTime;
            }
        }

        private void Network_N3MessageSent(object s, N3Message n3Msg)
        {
            //Only the active window will issue commands
            if (!Team.IsInTeam || !IsActiveWindow)
                return;

            if (n3Msg.Identity != DynelManager.LocalPlayer.Identity)
                return;

            if (n3Msg.N3MessageType == N3MessageType.CharDCMove)
            {
                if (!_menu.GetBool("SyncMove"))
                    return;

                CharDCMoveMessage charDCMoveMsg = (CharDCMoveMessage)n3Msg;
                IPCChannel.Broadcast(new MoveMessage()
                {
                    MoveType = charDCMoveMsg.MoveType,
                    PlayfieldId = Playfield.Identity.Instance,
                    Position = charDCMoveMsg.Position,
                    Rotation = charDCMoveMsg.Heading
                });

            }
            else if (n3Msg.N3MessageType == N3MessageType.CharacterAction)
            {
                if (!_menu.GetBool("SyncMove"))
                    return;

                CharacterActionMessage charActionMsg = (CharacterActionMessage)n3Msg;

                if (charActionMsg.Action != CharacterActionType.StandUp)
                    return;

                IPCChannel.Broadcast(new MoveMessage()
                {
                    MoveType = MovementAction.LeaveSit,
                    PlayfieldId = Playfield.Identity.Instance,
                    Position = DynelManager.LocalPlayer.Position,
                    Rotation = DynelManager.LocalPlayer.Rotation
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.LookAt)
            {
                LookAtMessage lookAtMsg = (LookAtMessage)n3Msg;
                IPCChannel.Broadcast(new TargetMessage()
                {
                    Target = lookAtMsg.Target
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.Attack)
            {
                if (!_menu.GetBool("SyncAttack"))
                    return;

                AttackMessage attackMsg = (AttackMessage)n3Msg;
                IPCChannel.Broadcast(new AttackIPCMessage()
                {
                    Target = attackMsg.Target
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.StopFight)
            {
                if (!_menu.GetBool("SyncAttack"))
                    return;

                StopFightMessage lookAtMsg = (StopFightMessage)n3Msg;
                IPCChannel.Broadcast(new StopAttackIPCMessage());
            }
            else if (n3Msg.N3MessageType == N3MessageType.GenericCmd)
            {
                if (!_menu.GetBool("SyncUse"))
                    return;

                GenericCmdMessage genericCmdMsg = (GenericCmdMessage)n3Msg;
   
                if(genericCmdMsg.Action == GenericCmdAction.Use && genericCmdMsg.Target.Type == IdentityType.Terminal)
                {
                    IPCChannel.Broadcast(new UseMessage()
                    {
                        Target = genericCmdMsg.Target
                    });
                }
            }
        }

        private void OnMoveMessage(int sender, IPCMessage msg)
        {
            //Only followers will act on commands
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            MoveMessage moveMsg = (MoveMessage)msg;

            if (Playfield.Identity.Instance != moveMsg.PlayfieldId)
                return;

            DynelManager.LocalPlayer.Position = moveMsg.Position;
            DynelManager.LocalPlayer.Rotation = moveMsg.Rotation;
            MovementController.Instance.SetMovement(moveMsg.MoveType);
        }

        private void OnTargetMessage(int sender, IPCMessage msg)
        {
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            TargetMessage targetMsg = (TargetMessage)msg;
            Targeting.SetTarget(targetMsg.Target);
        }

        private void OnAttackMessage(int sender, IPCMessage msg)
        {
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            AttackIPCMessage attackMsg = (AttackIPCMessage)msg;
            DynelManager.LocalPlayer.Attack(attackMsg.Target);
        }

        private void OnStopAttackMessage(int sender, IPCMessage msg)
        {
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            DynelManager.LocalPlayer.StopAttack();
        }

        private void OnUseMessage(int sender, IPCMessage msg)
        {
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            UseMessage useMsg = (UseMessage)msg;
            DynelManager.GetDynel<SimpleItem>(useMsg.Target)?.Use();
        }

        private void OnCharStatusMessage(int sender, IPCMessage msg)
        {
            CharStatusMessage statusMsg = (CharStatusMessage)msg;

            _statusWindow.SetCharStatus(sender, new CharacterStatus
            {
                Name = statusMsg.Name,
                Health = statusMsg.Health,
                MaxHealth = statusMsg.MaxHealth,
                Nano = statusMsg.Nano,
                MaxNano = statusMsg.MaxNano
            });
        }

        private void OnCharLeftMessage(int sender, IPCMessage msg)
        {
            _statusWindow.RemoveChar(sender);
        }

        public override void Teardown()
        {
            IPCChannel.Broadcast(new CharLeftMessage());
        }

        private void PrintCommandUsage(ChatWindow chatWindow)
        {
            string help = "Usage:\nStatus - toggles status window";

            chatWindow.WriteLine(help, ChatColor.LightBlue);
        }

        private void MbCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                if (param.Length < 1)
                {
                    PrintCommandUsage(chatWindow);
                    return;
                }

                switch (param[0].ToLower())
                {
                    case "status":
                        _statusWindow.Open();
                        break;
                    default:
                        PrintCommandUsage(chatWindow);
                        break;
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}