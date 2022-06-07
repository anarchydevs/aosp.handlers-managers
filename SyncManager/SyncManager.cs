﻿using System;
using System.Linq;
using System.Diagnostics;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AOSharp.Core.Inventory;
using AOSharp.Common.GameData.UI;
using SyncManager.IPCMessages;

namespace SyncManager
{
    public class SyncManager : AOPluginEntry
    {
        private static IPCChannel IPCChannel;

        public static Config Config { get; private set; }

        private static Identity useDynel;
        private static Identity useOnDynel;
        private static Identity useItem;
        public static string PluginDirectory;

        public static bool _openBags = false;

        private static double _useTimer;

        public static Window infoWindow;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static Settings _settings = new Settings("SyncManager");

        private static Settings _info = new Settings("Info");

        private static Item _bagItem;

        private bool IsActiveWindow => GetForegroundWindow() == Process.GetCurrentProcess().MainWindowHandle;

        public override void Run(string pluginDir)
        {
            PluginDirectory = pluginDir;

            Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\SyncManager\\{Game.ClientInst}\\Config.json");

            IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));

            _settings.AddVariable("SyncMove", false);
            _settings.AddVariable("SyncBags", false);
            _settings.AddVariable("SyncUse", true);
            _settings.AddVariable("SyncChat", false);
            _settings.AddVariable("SyncTrade", false);

            IPCChannel.RegisterCallback((int)IPCOpcode.Move, OnMoveMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Jump, OnJumpMessage);

            //IPCChannel.RegisterCallback((int)IPCOpcode.Trade, OnTradeMessage);

            IPCChannel.RegisterCallback((int)IPCOpcode.Use, OnUseMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.UseItem, OnUseItemMessage);

            IPCChannel.RegisterCallback((int)IPCOpcode.NpcChatOpen, OnNpcChatOpenMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.NpcChatClose, OnNpcChatCloseMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.NpcChatAnswer, OnNpcChatAnswerMessage);

            SettingsController.RegisterSettingsWindow("Sync Manager", pluginDir + "\\UI\\SyncManagerSettingWindow.xml", _settings);

            SettingsController.RegisterSettingsWindow("Info", pluginDir + "\\UI\\SyncManagerInfoView.xml", _info);

            Chat.RegisterCommand("syncmove", SyncSwitch);
            Chat.RegisterCommand("syncbags", SyncBagsSwitch);
            Chat.RegisterCommand("syncuse", SyncUseSwitch);
            Chat.RegisterCommand("syncchat", SyncChatSwitch);
            Chat.RegisterCommand("synctrade", SyncTradeSwitch);

            Game.OnUpdate += OnUpdate;
            Network.N3MessageSent += Network_N3MessageSent;
            Game.TeleportEnded += OnZoned;
            //Team.TeamRequest = Team_TeamRequest;


            Chat.WriteLine("SyncManager Loaded!");
            Chat.WriteLine("/sync for settings.");
        }

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        //private void OnZoned(object s, EventArgs e)
        //{

        //}

        private void OnZoned(object s, EventArgs e)
        {
            if (_settings["SyncBags"].AsBool())
            {
                Task.Factory.StartNew(
                    async () =>
                    {
                        await Task.Delay(100);

                        List<Item> bags = Inventory.Items
                            .Where(c => c.UniqueIdentity.Type == IdentityType.Container)
                            .ToList();

                        foreach (Item bag in bags)
                        {
                            bag.Use();
                            bag.Use();
                        }
                    });
            }
        }


        private void Network_N3MessageSent(object s, N3Message n3Msg)
        {
            if (!IsActiveCharacter() || n3Msg.Identity != DynelManager.LocalPlayer.Identity) { return; }

            if (n3Msg.N3MessageType == N3MessageType.CharDCMove)
            {
                CharDCMoveMessage charDCMoveMsg = (CharDCMoveMessage)n3Msg;

                if (charDCMoveMsg.MoveType == MovementAction.JumpStart && !_settings["SyncMove"].AsBool())
                {
                    IPCChannel.Broadcast(new JumpMessage()
                    {
                        MoveType = charDCMoveMsg.MoveType,
                        PlayfieldId = Playfield.Identity.Instance,
                    });
                }
                else
                {
                    if (!_settings["SyncMove"].AsBool()) { return; }

                    IPCChannel.Broadcast(new MoveMessage()
                    {
                        MoveType = charDCMoveMsg.MoveType,
                        PlayfieldId = Playfield.Identity.Instance,
                        Position = charDCMoveMsg.Position,
                        Rotation = charDCMoveMsg.Heading
                    });
                }
            }
            //else if (n3Msg.N3MessageType == N3MessageType.Trade)
            //{
            //    if (!_settings["SyncTrade"].AsBool()) { return; }

            //    TradeMessage charTradeIpcMsg = (TradeMessage)n3Msg;

            //    if (charTradeIpcMsg.Action == TradeAction.Confirm)
            //    {
            //        IPCChannel.Broadcast(new TradeHandleMessage()
            //        {
            //            Unknown1 = charTradeIpcMsg.Unknown1,
            //            Action = charTradeIpcMsg.Action,
            //            Target = charTradeIpcMsg.Target,
            //            Container = charTradeIpcMsg.Container,
            //        });
            //    }

            //    if (charTradeIpcMsg.Action == TradeAction.Accept)
            //    {
            //        IPCChannel.Broadcast(new TradeHandleMessage()
            //        {
            //            Unknown1 = charTradeIpcMsg.Unknown1,
            //            Action = charTradeIpcMsg.Action,
            //            Target = charTradeIpcMsg.Target,
            //            Container = charTradeIpcMsg.Container,
            //        });
            //    }
            //}
            else if (n3Msg.N3MessageType == N3MessageType.CharacterAction)
            {
                if (!_settings["SyncMove"].AsBool()) { return; }

                CharacterActionMessage charActionMsg = (CharacterActionMessage)n3Msg;

                if (charActionMsg.Action != CharacterActionType.StandUp) { return; }

                IPCChannel.Broadcast(new MoveMessage()
                {
                    MoveType = MovementAction.LeaveSit,
                    PlayfieldId = Playfield.Identity.Instance,
                    Position = DynelManager.LocalPlayer.Position,
                    Rotation = DynelManager.LocalPlayer.Rotation
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.GenericCmd)
            {
                GenericCmdMessage genericCmdMsg = (GenericCmdMessage)n3Msg;

                Dynel target = DynelManager.AllDynels.FirstOrDefault(x => x.Identity == genericCmdMsg.Target);

                if (genericCmdMsg.Action == GenericCmdAction.Use && genericCmdMsg.Target.Type == IdentityType.Terminal)
                {
                    IPCChannel.Broadcast(new UseMessage()
                    {
                        Target = genericCmdMsg.Target,
                        PfId = Playfield.ModelIdentity.Instance
                    });
                }
                else if (genericCmdMsg.Action == GenericCmdAction.Use && _settings["SyncUse"].AsBool())
                {
                    if (Inventory.Find(genericCmdMsg.Target, out Item item) && item.UniqueIdentity == Identity.None && !IsOther(item))
                    {
                        IPCChannel.Broadcast(new UsableMessage()
                        {
                            ItemLowId = item.LowId,
                            ItemHighId = item.HighId,
                        });
                    }
                    else
                    {
                        foreach (Backpack bag in Inventory.Backpacks)
                        {
                            _bagItem = bag.Items
                                .Where(c => c.Slot.Instance == genericCmdMsg.Target.Instance)
                                .FirstOrDefault();

                            if (_bagItem != null)
                            {
                                IPCChannel.Broadcast(new UsableMessage()
                                {
                                    ItemLowId = _bagItem.LowId,
                                    ItemHighId = _bagItem.HighId
                                });
                            }
                        }
                    }
                }
                else if (genericCmdMsg.Action == GenericCmdAction.UseItemOnItem)
                {
                    if (Inventory.Find(genericCmdMsg.Source, out Item item))
                    {
                        IPCChannel.Broadcast(new UsableMessage()
                        {
                            ItemLowId = item.LowId,
                            ItemHighId = item.HighId,
                            Target = genericCmdMsg.Target
                        });
                    }
                    else 
                    {
                        foreach (Backpack bag in Inventory.Backpacks)
                        {
                            _bagItem = bag.Items
                                .Where(c => c.Slot.Instance == genericCmdMsg.Source.Instance)
                                .FirstOrDefault();

                            if (_bagItem != null)
                            {
                                IPCChannel.Broadcast(new UsableMessage()
                                {
                                    ItemLowId = _bagItem.LowId,
                                    ItemHighId = _bagItem.HighId,
                                    Target = genericCmdMsg.Target
                                });
                            }
                        }
                    }
                }
            }
            else if (n3Msg.N3MessageType == N3MessageType.KnubotOpenChatWindow)
            {
                if (!_settings["SyncChat"].AsBool()) { return; }

                KnuBotOpenChatWindowMessage n3OpenChatMessage = (KnuBotOpenChatWindowMessage)n3Msg;
                IPCChannel.Broadcast(new NpcChatOpenMessage()
                {
                    Target = n3OpenChatMessage.Target
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.KnubotCloseChatWindow)
            {
                if (!_settings["SyncChat"].AsBool()) { return; }

                KnuBotCloseChatWindowMessage n3CloseChatMessage = (KnuBotCloseChatWindowMessage)n3Msg;
                IPCChannel.Broadcast(new NpcChatCloseMessage()
                {
                    Target = n3CloseChatMessage.Target
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.KnubotAnswer)
            {
                if (!_settings["SyncChat"].AsBool()) { return; }

                KnuBotAnswerMessage n3AnswerMsg = (KnuBotAnswerMessage)n3Msg;
                IPCChannel.Broadcast(new NpcChatAnswerMessage()
                {
                    Target = n3AnswerMsg.Target,
                    Answer = n3AnswerMsg.Answer
                });
            }
        }


        private void OnUpdate(object s, float deltaTime)
        {
            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView textinput1);

                if (textinput1 != null && textinput1.Text != String.Empty)
                {
                    if (int.TryParse(textinput1.Text, out int channelValue))
                    {
                        if (Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                        {
                            IPCChannel.SetChannelId(Convert.ToByte(channelValue));
                            Config.CharSettings[Game.ClientInst].IPCChannel = Convert.ToByte(channelValue);
                            SettingsController.SyncManagerChannel = channelValue.ToString();
                            Config.Save();
                        }
                    }
                }

                if (SettingsController.settingsWindow.FindView("SyncManagerInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = InfoView;
                }
            }

            if (SettingsController.SyncManagerChannel == String.Empty)
            {
                SettingsController.SyncManagerChannel = Config.IPCChannel.ToString();
            }

            if (!_openBags && _settings["SyncBags"].AsBool())
            {
                List<Item> bags = Inventory.Items
                    .Where(c => c.UniqueIdentity.Type == IdentityType.Container)
                    .ToList();

                foreach (Item bag in bags)
                {
                    bag.Use();
                    bag.Use();
                }

                _openBags = true;
            }

            if (Time.NormalTime > _useTimer + 0.1)
            {
                if (!IsActiveWindow)
                {
                    ListenerUseSync();
                }
                _useTimer = Time.NormalTime;
            }
        }
        private void OnJumpMessage(int sender, IPCMessage msg)
        {
            if (IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            JumpMessage jumpMsg = (JumpMessage)msg;

            if (Playfield.Identity.Instance != jumpMsg.PlayfieldId)
                return;

            MovementController.Instance.SetMovement(jumpMsg.MoveType);
        }

        private void OnMoveMessage(int sender, IPCMessage msg)
        {
            if (IsActiveWindow)
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

        //private void OnTradeMessage(int sender, IPCMessage msg)
        //{
        //    if (Game.IsZoning)
        //        return;

        //    TradeHandleMessage charTradeIpcMsg = (TradeHandleMessage)msg;
        //    TradeMessage charTradeMsg = new TradeMessage()
        //    {
        //        Unknown1 = charTradeIpcMsg.Unknown1,
        //        Action = charTradeIpcMsg.Action,
        //        Target = charTradeIpcMsg.Target,
        //        Container = charTradeIpcMsg.Container,
        //    };
        //    Network.Send(charTradeMsg);
        //}

        private void OnStopAttackMessage(int sender, IPCMessage msg)
        {
            if (IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            DynelManager.LocalPlayer.StopAttack();
        }

        private void OnUseItemMessage(int sender, IPCMessage msg)
        {
            if (!_settings["SyncUse"].AsBool() || IsActiveWindow || Game.IsZoning) { return; }

            UsableMessage usableMsg = (UsableMessage)msg;

            if (usableMsg.ItemLowId == 291043 || usableMsg.ItemLowId == 291043 || usableMsg.ItemLowId == 204103 || usableMsg.ItemLowId == 204104 ||
                usableMsg.ItemLowId == 204105 || usableMsg.ItemLowId == 204106 || usableMsg.ItemLowId == 204107 || usableMsg.ItemHighId == 204107 ||
                usableMsg.ItemLowId == 303138 || usableMsg.ItemLowId == 303141 || usableMsg.ItemLowId == 303137 || usableMsg.ItemHighId == 303136)
                return;

            if (usableMsg.ItemLowId == 226308 || usableMsg.ItemLowId == 226290 || usableMsg.ItemLowId == 226291 || usableMsg.ItemLowId == 226307 || usableMsg.ItemLowId == 226288)
            {
                Item NoviRings = Inventory.Items
                    .Where(c => c.Name.Contains("Pure Novictum Ring"))
                    .FirstOrDefault();

                if (NoviRings != null)
                {
                    useItem = new Identity(IdentityType.Inventory, NoviRings.Slot.Instance);
                    useOnDynel = usableMsg.Target;
                    usableMsg.Target = Identity.None;
                }
                else
                {
                    foreach (Backpack bag in Inventory.Backpacks)
                    {
                        _bagItem = bag.Items
                            .Where(c => c.Name.Contains("Pure Novictum Ring"))
                            .FirstOrDefault();

                        if (_bagItem != null)
                        {
                            useItem = _bagItem.Slot;
                            useOnDynel = usableMsg.Target;
                            usableMsg.Target = Identity.None;
                        }
                    }
                }
            }
            if (usableMsg.ItemLowId == 226188 || usableMsg.ItemLowId == 226189 || usableMsg.ItemLowId == 226190 || usableMsg.ItemLowId == 226191 || usableMsg.ItemLowId == 226192)
            {
                Item RimyRings = Inventory.Items
                .Where(c => c.Name.Contains("Rimy Ring for"))
                .FirstOrDefault();

                if (RimyRings != null)
                {
                    useItem = new Identity(IdentityType.Inventory, RimyRings.Slot.Instance);
                    useOnDynel = usableMsg.Target;
                    usableMsg.Target = Identity.None;
                }
                else
                {
                    foreach (Backpack bag in Inventory.Backpacks)
                    {
                        _bagItem = bag.Items
                            .Where(c => c.Name.Contains("Rimy Ring for"))
                            .FirstOrDefault();

                        if (_bagItem != null)
                        {
                            useItem = _bagItem.Slot;
                            useOnDynel = usableMsg.Target;
                            usableMsg.Target = Identity.None;
                        }
                    }
                }
            }
            if (usableMsg.ItemLowId == 226065 || usableMsg.ItemLowId == 226066 || usableMsg.ItemLowId == 226067 || usableMsg.ItemLowId == 226068 || usableMsg.ItemLowId == 226069)
            {
                Item AchromRings = Inventory.Items
                .Where(c => c.Name.Contains("Achromic Ring for"))
                .FirstOrDefault();

                if (AchromRings != null)
                {
                    useItem = new Identity(IdentityType.Inventory, AchromRings.Slot.Instance);
                    useOnDynel = usableMsg.Target;
                    usableMsg.Target = Identity.None;
                }
                else
                {
                    foreach (Backpack bag in Inventory.Backpacks)
                    {
                        _bagItem = bag.Items
                            .Where(c => c.Name.Contains("Achromic Ring for"))
                            .FirstOrDefault();

                        if (_bagItem != null)
                        {
                            useItem = _bagItem.Slot;
                            useOnDynel = usableMsg.Target;
                            usableMsg.Target = Identity.None;
                        }
                    }
                }
            }
            if (usableMsg.ItemLowId == 226287 || usableMsg.ItemLowId == 226293 || usableMsg.ItemLowId == 226294 || usableMsg.ItemLowId == 226295 || usableMsg.ItemLowId == 226306)
            {
                Item SangRings = Inventory.Items
                .Where(c => c.Name.Contains("Sanguine Ring for"))
                .FirstOrDefault();

                if (SangRings != null)
                {
                    useItem = new Identity(IdentityType.Inventory, SangRings.Slot.Instance);
                    useOnDynel = usableMsg.Target;
                    usableMsg.Target = Identity.None;
                }
                else
                {
                    foreach (Backpack bag in Inventory.Backpacks)
                    {
                        _bagItem = bag.Items
                            .Where(c => c.Name.Contains("Sanguine Ring for"))
                            .FirstOrDefault();

                        if (_bagItem != null)
                        {
                            useItem = _bagItem.Slot;
                            useOnDynel = usableMsg.Target;
                            usableMsg.Target = Identity.None;
                        }
                    }
                }
            }
            if (usableMsg.ItemLowId == 226125 || usableMsg.ItemLowId == 226127 || usableMsg.ItemLowId == 226126 || usableMsg.ItemLowId == 226023 || usableMsg.ItemLowId == 226005)
            {
                Item CaligRings = Inventory.Items
                .Where(c => c.Name.Contains("Caliginous Ring"))
                .FirstOrDefault();

                if (CaligRings != null)
                {
                    useItem = new Identity(IdentityType.Inventory, CaligRings.Slot.Instance);
                    useOnDynel = usableMsg.Target;
                    usableMsg.Target = Identity.None;
                }
                else
                {
                    foreach (Backpack bag in Inventory.Backpacks)
                    {
                        _bagItem = bag.Items
                            .Where(c => c.Name.Contains("Caliginous Ring"))
                            .FirstOrDefault();

                        if (_bagItem != null)
                        {
                            useItem = _bagItem.Slot;
                            useOnDynel = usableMsg.Target;
                            usableMsg.Target = Identity.None;
                        }
                    }
                }
            }
            else
            {
                if (usableMsg.Target == Identity.None)
                {
                    if (Inventory.Find(usableMsg.ItemLowId, usableMsg.ItemHighId, out Item item))
                    {
                        Network.Send(new GenericCmdMessage()
                        {
                            Unknown = 1,
                            Action = GenericCmdAction.Use,
                            User = DynelManager.LocalPlayer.Identity,
                            Target = new Identity(IdentityType.Inventory, item.Slot.Instance)
                        });
                    }
                    else
                    {
                        foreach (Backpack bag in Inventory.Backpacks)
                        {
                            _bagItem = bag.Items
                                .Where(c => c.HighId == usableMsg.ItemHighId)
                                .FirstOrDefault();

                            if (_bagItem != null)
                            {
                                Network.Send(new GenericCmdMessage()
                                {
                                    Unknown = 1,
                                    Action = GenericCmdAction.Use,
                                    User = DynelManager.LocalPlayer.Identity,
                                    Target = _bagItem.Slot
                                });
                            }
                        }
                    }
                }
                else
                {
                    if (Inventory.Find(usableMsg.ItemLowId, usableMsg.ItemHighId, out Item item))
                    {
                        useItem = new Identity(IdentityType.Inventory, item.Slot.Instance);
                        useOnDynel = usableMsg.Target;
                        usableMsg.Target = Identity.None;
                    }
                    else
                    {
                        foreach (Backpack bag in Inventory.Backpacks)
                        {
                            _bagItem = bag.Items
                                .Where(c => c.HighId == usableMsg.ItemHighId)
                                .FirstOrDefault();

                            if (_bagItem != null)
                            {
                                Network.Send(new GenericCmdMessage()
                                {
                                    Unknown = 1,
                                    Action = GenericCmdAction.UseItemOnItem,
                                    User = DynelManager.LocalPlayer.Identity,
                                    Target = usableMsg.Target,
                                    Source = _bagItem.Slot
                                });
                            }
                        }
                    }
                }
            }
        }

        private void OnUseMessage(int sender, IPCMessage msg)
        {

            if (IsActiveWindow || Game.IsZoning) { return; }

            UseMessage useMsg = (UseMessage)msg;
            //DynelManager.GetDynel<SimpleItem>(useMsg.Target)?.Use();
            if (useMsg.PfId != Playfield.ModelIdentity.Instance) { return; }

            useDynel = useMsg.Target;
        }

        private void OnNpcChatOpenMessage(int sender, IPCMessage msg)
        {
            NpcChatOpenMessage message = (NpcChatOpenMessage)msg;
            NpcDialog.Open(message.Target);
        }

        private void OnNpcChatCloseMessage(int sender, IPCMessage msg)
        {
            NpcChatCloseMessage message = (NpcChatCloseMessage)msg;
            KnuBotCloseChatWindowMessage closeChatMessage = new KnuBotCloseChatWindowMessage()
            {
                Unknown1 = 2,
                Unknown2 = 0,
                Unknown3 = 0,
                Target = message.Target
            };
            Network.Send(closeChatMessage);
        }

        private void OnNpcChatAnswerMessage(int sender, IPCMessage msg)
        {
            NpcChatAnswerMessage message = (NpcChatAnswerMessage)msg;
            NpcDialog.SelectAnswer(message.Target, message.Answer);
        }

        private void SyncUseSwitch(string command, string[] param, ChatWindow chatWindow)
        {
            if (param.Length == 0)
            {
                _settings["SyncUse"] = !_settings["SyncUse"].AsBool();
                Chat.WriteLine($"Sync use : {_settings["SyncUse"].AsBool()}");
            }
        }

        private void SyncChatSwitch(string command, string[] param, ChatWindow chatWindow)
        {
            if (param.Length == 0)
            {
                _settings["SyncChat"] = !_settings["SyncChat"].AsBool();
                Chat.WriteLine($"Sync chat : {_settings["SyncChat"].AsBool()}");
            }
        }

        private void SyncTradeSwitch(string command, string[] param, ChatWindow chatWindow)
        {
            if (param.Length == 0)
            {
                _settings["SyncTrade"] = !_settings["SyncTrade"].AsBool();
                Chat.WriteLine($"Sync trading : {_settings["SyncTrade"].AsBool()}");
            }
        }

        private void SyncSwitch(string command, string[] param, ChatWindow chatWindow)
        {
            if (param.Length == 0)
            {
                _settings["SyncMove"] = !_settings["SyncMove"].AsBool();
                Chat.WriteLine($"Sync move : {_settings["SyncMove"].AsBool()}");
            }
        }
        private void SyncBagsSwitch(string command, string[] param, ChatWindow chatWindow)
        {
            if (param.Length == 0)
            {
                _settings["SyncBags"] = !_settings["SyncBags"].AsBool();
                Chat.WriteLine($"Sync bags : {_settings["SyncBags"].AsBool()}");
            }
        }

        private void ListenerUseSync()
        {
            Vector3 playerPos = DynelManager.LocalPlayer.Position;

            // delayed dynel use
            if (useDynel != Identity.None)
            {
                Dynel usedynel = DynelManager.AllDynels.FirstOrDefault(x => x.Identity == useDynel);

                if (usedynel != null && Vector3.Distance(playerPos, usedynel.Position) < 8 &&
                    usedynel.Name != "Rubi-Ka Banking Service Terminal" && usedynel.Name != "Mail Terminal") //Add more
                {
                    DynelManager.GetDynel<SimpleItem>(useDynel)?.Use();
                    useDynel = Identity.None;
                }
            }
            // delayed itemonitem dynel use
            if (useOnDynel != Identity.None)
            {
                Dynel _useOnDynel = DynelManager.AllDynels.FirstOrDefault(x => x.Identity == useOnDynel);
                if (_useOnDynel != null && Vector3.Distance(playerPos, _useOnDynel.Position) < 8)
                {
                    Network.Send(new GenericCmdMessage()
                    {
                        Unknown = 1,
                        Action = GenericCmdAction.UseItemOnItem,
                        Temp1 = 0,
                        Temp4 = 0,
                        Identity = DynelManager.LocalPlayer.Identity,
                        User = DynelManager.LocalPlayer.Identity,
                        Target = useOnDynel,
                        Source = useItem

                    });
                    useOnDynel = Identity.None;
                    useItem = Identity.None;
                }
            }
        }
        private void InfoView(object s, ButtonBase button)
        {
            infoWindow = Window.CreateFromXml("Info", PluginDirectory + "\\UI\\SyncManagerInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            infoWindow.Show(true);
        }

        private bool IsActiveCharacter()
        {
            return IsActiveWindow;
        }

        public static bool IsOther(Item item)
        {
            return item.LowId == 305476 || item.LowId == 204698 || item.LowId == 156576 || item.LowId == 267168 || item.LowId == 267167
                || item.Name.Contains("Health");
        }
    }
}