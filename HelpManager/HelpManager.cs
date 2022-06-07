﻿using System;
using System.Linq;
using System.Diagnostics;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using HelpManager.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AOSharp.Core.Inventory;
using AOSharp.Common.GameData.UI;

namespace HelpManager
{
    public class HelpManager : AOPluginEntry
    {
        private static IPCChannel IPCChannel;

        public static Config Config { get; private set; }

        private static Pet healpet;

        public static string PluginDirectory;

        private static double _updateTick;
        private static double _sitUpdateTimer;
        private static double _sitPetUpdateTimer;
        private static double _sitPetUsedTimer;
        private static double _shapeUsedTimer;
        private static double _followTimer;
        private static double _assistTimer;
        private static double _morphPathingTimer;
        private static double _bellyPathingTimer;
        private static double _zixMorphTimer;

        public static bool Sitting = false;
        public static bool HealingPet = false;

        public static Window followWindow;
        public static Window assistWindow;
        public static Window infoWindow;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        private static Settings _settings = new Settings("HelpManager");

        private static Settings _assist = new Settings("Assist");
        private static Settings _follow = new Settings("Follow");
        private static Settings _info = new Settings("Info");

        List<Vector3> MorphBird = new List<Vector3>
        {
            new Vector3(75.5, 29.0, 58.6),
            new Vector3(37.3, 29.0, 59.0),
            new Vector3(35.6, 29.3, 30.5),
            new Vector3(37.3, 29.0, 59.0),
            new Vector3(75.5, 29.0, 58.6),
            new Vector3(75.5, 29.0, 58.6)
            //new Vector3(76.1, 29.0, 28.3)
        };

        List<Vector3> BellyPath = new List<Vector3>
        {
            new Vector3(143.1f, 90.0f, 108.2f),
            new Vector3(156.1f, 90.0f, 102.3f),
            new Vector3(178.0f, 90.0f, 97.6f)
        };

        List<Vector3> OutBellyPath = new List<Vector3>
        {
            new Vector3(214.8f, 100.6f, 126.5f),
            new Vector3(210.6f, 100.2f, 129.7f)
        };

        List<Vector3> MorphHorse = new List<Vector3>
        {
            new Vector3(128.4, 29.0, 59.6),
            new Vector3(161.9, 29.0, 59.5),
            new Vector3(163.9, 29.4, 29.6),
            new Vector3(161.9, 29.0, 59.5),
            new Vector3(128.4, 29.0, 59.6),
            new Vector3(128.4, 29.0, 59.6)
            //new Vector3(76.1, 29.0, 28.3)
        };

        private bool IsActiveWindow => GetForegroundWindow() == Process.GetCurrentProcess().MainWindowHandle;

        public override void Run(string pluginDir)
        {
            PluginDirectory = pluginDir;

            Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\HelpManager\\{Game.ClientInst}\\Config.json");

            IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));

            _follow.AddVariable("FollowSelection", (int)FollowSelection.None);
            _assist.AddVariable("AttackSelection", (int)AttackSelection.None);

            _settings.AddVariable("AutoSit", true);

            _settings.AddVariable("MorphPathing", false);
            _settings.AddVariable("BellyPathing", false);
            _settings.AddVariable("Db3Shapes", false);

            IPCChannel.RegisterCallback((int)IPCOpcode.Assist, OnAssistMessage);

            IPCChannel.RegisterCallback((int)IPCOpcode.Follow, OnFollowMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.NavFollow, OnNavFollowMessage);

            IPCChannel.RegisterCallback((int)IPCOpcode.YalmOn, OnYalmCast);
            IPCChannel.RegisterCallback((int)IPCOpcode.YalmUse, OnYalmUse);
            IPCChannel.RegisterCallback((int)IPCOpcode.YalmOff, OnYalmCancel);

            IPCChannel.RegisterCallback((int)IPCOpcode.ClearBuffs, OnClearBuffs);

            SettingsController.RegisterSettingsWindow("Help Manager", pluginDir + "\\UI\\HelpManagerSettingWindow.xml", _settings);

            SettingsController.RegisterSettingsWindow("Assist", pluginDir + "\\UI\\HelpManagerAssistView.xml", _assist);
            SettingsController.RegisterSettingsWindow("Follow", pluginDir + "\\UI\\HelpManagerFollowView.xml", _follow);
            SettingsController.RegisterSettingsWindow("Info", pluginDir + "\\UI\\HelpManagerInfoView.xml", _info);

            Chat.RegisterCommand("leadfollow", LeadFollowSwitch);
            Chat.RegisterCommand("autosit", AutoSitSwitch);

            Chat.RegisterCommand("yalm", YalmCommand);
            Chat.RegisterCommand("rebuff", Rebuff);

            //Chat.RegisterCommand("bags", (string command, string[] param, ChatWindow chatWindow) =>
            //{
            //    List<Item> bags = Inventory.Items
            //    .Where(c => c.UniqueIdentity.Type == IdentityType.Container)
            //    .ToList();

            //    Chat.WriteLine($"{bags.Count()}");

            //    foreach (Item bag in bags)
            //    {
            //        bag.Use();
            //        bag.Use();
            //    }
            //});

            Chat.RegisterCommand("doc", DocTarget);


            Game.OnUpdate += OnUpdate;
            //Game.TeleportEnded += OnZoned;


            Chat.WriteLine("HelpManager Loaded!");
            Chat.WriteLine("/helper for settings.");
        }

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        private void OnZoned(object s, EventArgs e)
        {

        }


        private void OnUpdate(object s, float deltaTime)
        {
            if (assistWindow != null && assistWindow.IsValid)
            {
                assistWindow.FindView("FollowNamedCharacter", out TextInputView textinput1);
                assistWindow.FindView("FollowNamedIdentity", out TextInputView textinput2);
                assistWindow.FindView("NavFollowDistanceBox", out TextInputView textinput3);
                assistWindow.FindView("AssistNamedCharacter", out TextInputView textinput4);

                if (textinput1 != null && textinput1.Text != String.Empty)
                {
                    if (Config.CharSettings[Game.ClientInst].FollowPlayer != textinput1.Text)
                    {
                        Config.CharSettings[Game.ClientInst].FollowPlayer = textinput1.Text;
                        SettingsController.HelpManagerFollowPlayer = textinput1.Text;
                        Config.Save();
                    }
                }

                if (textinput2 != null && textinput2.Text != String.Empty)
                {
                    if (Config.CharSettings[Game.ClientInst].NavFollowPlayer != textinput2.Text)
                    {
                        Config.CharSettings[Game.ClientInst].NavFollowPlayer = textinput2.Text;
                        SettingsController.HelpManagerNavFollowPlayer = textinput2.Text;
                        Config.Save();
                    }
                }

                if (textinput3 != null && textinput3.Text != String.Empty)
                {
                    if (int.TryParse(textinput3.Text, out int rangeValue))
                    {
                        if (Config.CharSettings[Game.ClientInst].NavFollowDistance != rangeValue)
                        {
                            Config.CharSettings[Game.ClientInst].NavFollowDistance = rangeValue;
                            SettingsController.HelpManagerNavFollowDistance = rangeValue;
                            Config.Save();
                        }
                    }
                }
                if (textinput4 != null && textinput4.Text != String.Empty)
                {
                    if (Config.CharSettings[Game.ClientInst].AssistPlayer != textinput4.Text)
                    {
                        Config.CharSettings[Game.ClientInst].AssistPlayer = textinput4.Text;
                        SettingsController.HelpManagerAssistPlayer = textinput4.Text;
                        Config.Save();
                    }
                }
            }

            if (followWindow != null && followWindow.IsValid)
            {
                followWindow.FindView("FollowNamedCharacter", out TextInputView textinput1);
                followWindow.FindView("FollowNamedIdentity", out TextInputView textinput2);
                followWindow.FindView("NavFollowDistanceBox", out TextInputView textinput3);
                followWindow.FindView("AssistNamedCharacter", out TextInputView textinput4);

                if (textinput1 != null && textinput1.Text != String.Empty)
                {
                    if (Config.CharSettings[Game.ClientInst].FollowPlayer != textinput1.Text)
                    {
                        Config.CharSettings[Game.ClientInst].FollowPlayer = textinput1.Text;
                        SettingsController.HelpManagerFollowPlayer = textinput1.Text;
                        Config.Save();
                    }
                }

                if (textinput2 != null && textinput2.Text != String.Empty)
                {
                    if (Config.CharSettings[Game.ClientInst].NavFollowPlayer != textinput2.Text)
                    {
                        Config.CharSettings[Game.ClientInst].NavFollowPlayer = textinput2.Text;
                        SettingsController.HelpManagerNavFollowPlayer = textinput2.Text;
                        Config.Save();
                    }
                }

                if (textinput3 != null && textinput3.Text != String.Empty)
                {
                    if (int.TryParse(textinput3.Text, out int rangeValue))
                    {
                        if (Config.CharSettings[Game.ClientInst].NavFollowDistance != rangeValue)
                        {
                            Config.CharSettings[Game.ClientInst].NavFollowDistance = rangeValue;
                            SettingsController.HelpManagerNavFollowDistance = rangeValue;
                            Config.Save();
                        }
                    }
                }

                if (textinput4 != null && textinput4.Text != String.Empty)
                {
                    if (Config.CharSettings[Game.ClientInst].AssistPlayer != textinput4.Text)
                    {
                        Config.CharSettings[Game.ClientInst].AssistPlayer = textinput4.Text;
                        SettingsController.HelpManagerAssistPlayer = textinput4.Text;
                        Config.Save();
                    }
                }
            }

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
                            SettingsController.HelpManagerChannel = channelValue.ToString();
                            Config.Save();
                        }
                    }
                }

                if (SettingsController.settingsWindow.FindView("HelpManagerInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = InfoView;
                }

                if (SettingsController.settingsWindow.FindView("HelpManagerFollowView", out Button followView))
                {
                    followView.Tag = SettingsController.settingsWindow;
                    followView.Clicked = FollowView;
                }

                if (SettingsController.settingsWindow.FindView("HelpManagerAssistView", out Button assistView))
                {
                    assistView.Tag = SettingsController.settingsWindow;
                    assistView.Clicked = AssistView;
                }
            }

            if (SettingsController.HelpManagerChannel == String.Empty)
            {
                SettingsController.HelpManagerChannel = Config.IPCChannel.ToString();
            }

            if (SettingsController.HelpManagerAssistPlayer == String.Empty)
            {
                SettingsController.HelpManagerAssistPlayer = Config.AssistPlayer;
            }

            if (SettingsController.HelpManagerFollowPlayer == String.Empty)
            {
                SettingsController.HelpManagerFollowPlayer = Config.FollowPlayer;
            }

            if (SettingsController.HelpManagerNavFollowPlayer == String.Empty)
            {
                SettingsController.HelpManagerNavFollowPlayer = Config.NavFollowPlayer;
            }

            if (SettingsController.HelpManagerNavFollowDistance != Config.NavFollowDistance)
            {
                SettingsController.HelpManagerNavFollowDistance = Config.NavFollowDistance;
            }

            if (Time.NormalTime > _updateTick + 8f)
            {
                List<SimpleChar> PlayersInRange = DynelManager.Characters
                    .Where(x => x.IsPlayer)
                    .Where(x => DynelManager.LocalPlayer.DistanceFrom(x) < 30f)
                    .ToList();

                foreach (SimpleChar player in PlayersInRange)
                {
                    Network.Send(new CharacterActionMessage()
                    {
                        Action = CharacterActionType.InfoRequest,
                        Target = player.Identity

                    });
                }

                _updateTick = Time.NormalTime;
            }

            if (_settings["BellyPathing"].AsBool() && Time.NormalTime > _bellyPathingTimer + 1)
            {
                Dynel Pustule = DynelManager.AllDynels
                    .Where(x => x.Identity.Type == IdentityType.Terminal && DynelManager.LocalPlayer.DistanceFrom(x) < 7f
                        && x.Name == "Glowing Pustule")
                    .FirstOrDefault();

                if (Pustule != null)
                {
                    Pustule.Use();
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(132.0f, 90.0f, 117.0f)) < 2f
                    && !MovementController.Instance.IsNavigating)
                {
                    MovementController.Instance.SetPath(BellyPath);
                }

                if (DynelManager.LocalPlayer.Position.DistanceFrom(new Vector3(217.0f, 94.0f, 148.0f)) < 2f
                    && !MovementController.Instance.IsNavigating)
                {
                    MovementController.Instance.SetPath(OutBellyPath);
                }

                _bellyPathingTimer = Time.NormalTime;
            }

            if (_settings["MorphPathing"].AsBool() && Time.NormalTime > _morphPathingTimer + 2)
            {
                if (!MovementController.Instance.IsNavigating && DynelManager.LocalPlayer.Buffs.Contains(281109))
                {
                    Vector3 curr = DynelManager.LocalPlayer.Position;

                    MovementController.Instance.SetPath(MorphBird);
                    MovementController.Instance.AppendDestination(curr);
                }

                if (!MovementController.Instance.IsNavigating && DynelManager.LocalPlayer.Buffs.Contains(281108))
                {
                    Vector3 curr = DynelManager.LocalPlayer.Position;

                    MovementController.Instance.SetPath(MorphHorse);
                    MovementController.Instance.AppendDestination(curr);
                }

                _morphPathingTimer = Time.NormalTime;
            }

            if (_settings["Db3Shapes"].AsBool() && Time.NormalTime > _shapeUsedTimer + 0.5)
            {
                Dynel shape = DynelManager.AllDynels
                    .Where(x => x.Identity.Type == IdentityType.Terminal && DynelManager.LocalPlayer.DistanceFrom(x) < 5f
                        && (x.Name == "Triangle of Nano Power" || x.Name == "Cylinder of Speed"
                    || x.Name == "Torus of Aim" || x.Name == "Square of Attack Power"))
                    .FirstOrDefault();

                if (shape != null)
                {
                    shape.Use();
                }

                _shapeUsedTimer = Time.NormalTime;
            }


            if (Time.NormalTime > _sitUpdateTimer + 0.5)
            {
                ListenerSit();

                _sitUpdateTimer = Time.NormalTime;
            }

            if (Time.NormalTime > _zixMorphTimer + 3)
            {
                if (DynelManager.LocalPlayer.Buffs.Contains(288532) || DynelManager.LocalPlayer.Buffs.Contains(302212))
                {
                    CancelBuffs(RelevantNanos.ZixMorph);
                }

                _zixMorphTimer = Time.NormalTime;
            }

            if (Time.NormalTime > _sitPetUpdateTimer + 2)
            {
                if (DynelManager.LocalPlayer.Profession == Profession.Metaphysicist)
                    ListenerPetSit();

                _sitPetUpdateTimer = Time.NormalTime;
            }

            if (AttackSelection.Assist == (AttackSelection)_assist["AttackSelection"].AsInt32()
                && Time.NormalTime > _assistTimer + 1)
            {
                //if (settings["SyncAttack"].AsBool())
                //{
                //    settings["SyncAttack"] = false;
                //    settings["AssistPlayer"] = false;
                //    Chat.WriteLine($"Can only have one form of sync attack active at once.");
                //}

                SimpleChar identity = DynelManager.Characters
                    .Where(c => SettingsController.HelpManagerAssistPlayer != String.Empty)
                    .Where(c => c.IsAlive)
                    .Where(x => !x.Flags.HasFlag(CharacterFlags.Pet))
                    .Where(c => c.Name == SettingsController.HelpManagerAssistPlayer)
                    .FirstOrDefault();

                if (identity == null) { return; }

                if (identity != null && identity.FightingTarget == null &&
                    DynelManager.LocalPlayer.FightingTarget != null)
                {
                    DynelManager.LocalPlayer.StopAttack();

                    _assistTimer = Time.NormalTime;
                }

                if (identity != null && identity.FightingTarget != null &&
                    (DynelManager.LocalPlayer.FightingTarget == null ||
                    (DynelManager.LocalPlayer.FightingTarget != null && DynelManager.LocalPlayer.FightingTarget.Identity != identity.FightingTarget.Identity)))
                {
                    DynelManager.LocalPlayer.Attack(identity.FightingTarget);

                    IPCChannel.Broadcast(new AssistMessage()
                    {
                        Target = identity.Identity
                    });
                    _assistTimer = Time.NormalTime;
                }
            }

            if (FollowSelection.LeadFollow == (FollowSelection)_follow["FollowSelection"].AsInt32()
                && Time.NormalTime > _followTimer + 1)
            {
                IPCChannel.Broadcast(new FollowMessage()
                {
                    Target = DynelManager.LocalPlayer.Identity
                });
                _followTimer = Time.NormalTime;
            }

            if (FollowSelection.NavFollow == (FollowSelection)_follow["FollowSelection"].AsInt32()
                && Time.NormalTime > _followTimer + 1)
            {
                Dynel identity = DynelManager.AllDynels
                    .Where(x => !x.Flags.HasFlag(CharacterFlags.Pet))
                    .Where(x => SettingsController.HelpManagerNavFollowPlayer != String.Empty)
                    .Where(x => x.Name == SettingsController.HelpManagerNavFollowPlayer)
                    .FirstOrDefault();

                if (identity != null)
                {
                    if (DynelManager.LocalPlayer.DistanceFrom(identity) <= Config.CharSettings[Game.ClientInst].NavFollowDistance)
                        MovementController.Instance.Halt();

                    if (DynelManager.LocalPlayer.DistanceFrom(identity) > Config.CharSettings[Game.ClientInst].NavFollowDistance)
                        MovementController.Instance.SetDestination(identity.Position);

                    IPCChannel.Broadcast(new NavFollowMessage()
                    {
                        Target = identity.Identity
                    });
                    _followTimer = Time.NormalTime;
                }
            }

            if (FollowSelection.OSFollow == (FollowSelection)_follow["FollowSelection"].AsInt32()
                && Time.NormalTime > _followTimer + 1)
            {
                if (SettingsController.HelpManagerFollowPlayer != String.Empty)
                {
                    Dynel identity = DynelManager.AllDynels
                        .Where(x => !x.Flags.HasFlag(CharacterFlags.Pet))
                        .Where(x => SettingsController.HelpManagerFollowPlayer != String.Empty)
                        .Where(x => x.Name == SettingsController.HelpManagerFollowPlayer)
                        .FirstOrDefault();

                    if (identity != null)
                    {
                        if (identity.Identity != DynelManager.LocalPlayer.Identity)
                            OSFollow(identity);

                        IPCChannel.Broadcast(new FollowMessage()
                        {
                            Target = identity.Identity // change this to the new target with selection param
                        });

                        _followTimer = Time.NormalTime;
                    }
                }
            }
        }
        private void OSFollow(Dynel dynel)
        {
            FollowTargetMessage n3Msg = new FollowTargetMessage()
            {
                Target = dynel.Identity,
                Unknown1 = 0,
                Unknown2 = 0,
                Unknown3 = 0,
                Unknown4 = 0,
                Unknown5 = 0,
                Unknown6 = 0,
                Unknown7 = 0
            };
            Network.Send(n3Msg);
            MovementController.Instance.SetMovement(MovementAction.Update);
        }



        private void OnClearBuffs(int sender, IPCMessage msg)
        {
            CancelAllBuffs();
        }
        private void OnAssistMessage(int sender, IPCMessage msg)
        {
            AssistMessage assistMessage = (AssistMessage)msg;

            Dynel targetDynel = DynelManager.GetDynel(assistMessage.Target);

            if (targetDynel != null && DynelManager.LocalPlayer.FightingTarget == null)
            {
                DynelManager.LocalPlayer.Attack(targetDynel);
            }
        }

        private void OnFollowMessage(int sender, IPCMessage msg)
        {
            FollowMessage followMessage = (FollowMessage)msg;
            FollowTargetMessage n3Msg = new FollowTargetMessage()
            {
                Target = followMessage.Target,
                Unknown1 = 0,
                Unknown2 = 0,
                Unknown3 = 0,
                Unknown4 = 0,
                Unknown5 = 0,
                Unknown6 = 0,
                Unknown7 = 0
            };
            Network.Send(n3Msg);
            MovementController.Instance.SetMovement(MovementAction.Update);
        }

        private void OnNavFollowMessage(int sender, IPCMessage msg)
        {

            NavFollowMessage followMessage = (NavFollowMessage)msg;

            Dynel targetDynel = DynelManager.GetDynel(followMessage.Target);

            if (targetDynel != null)
            {
                if (DynelManager.LocalPlayer.DistanceFrom(targetDynel) <= 15f)
                    MovementController.Instance.Halt();

                if (DynelManager.LocalPlayer.DistanceFrom(targetDynel) > 15f)
                    MovementController.Instance.SetDestination(targetDynel.Position);
                _followTimer = Time.NormalTime;
            }
            else
            {
                Chat.WriteLine($"Cannot find {targetDynel.Name}. Make sure to type captial first letter.");
                _settings["NavFollow"] = false;
                return;
            }
        }


        private void OnYalmCast(int sender, IPCMessage msg)
        {
            YalmOnMessage yalmMsg = (YalmOnMessage)msg;

            Spell yalm = Spell.List.FirstOrDefault(x => x.Id == yalmMsg.Spell);

            Spell yalm2 = Spell.List.FirstOrDefault(x => RelevantNanos.Yalms.Contains(x.Id));

            if (yalm != null)
            {
                yalm.Cast(false);
            }
            else if (yalm2 != null)
            {
                yalm2.Cast(false);
            }
            else
            {
                Item yalm3 = Inventory.Items.Where(x => x.Name.Contains("Yalm") || x.Name.Contains("Ganimedes")).Where(x => x.Slot.Type == IdentityType.Inventory).FirstOrDefault();

                if (yalm3 != null)
                    yalm3.Equip(EquipSlot.Weap_Hud1);
            }
        }

        private void OnYalmUse(int sender, IPCMessage msg)
        {
            YalmUseMessage yalmMsg = (YalmUseMessage)msg;

            Item yalm = Inventory.Items.FirstOrDefault(x => x.HighId == yalmMsg.Item);

            Item yalm2 = Inventory.Items.Where(x => x.Name.Contains("Yalm") || x.Name.Contains("Ganimedes")).Where(x => x.Slot.Type == IdentityType.Inventory).FirstOrDefault();

            if (yalm != null)
            {
                yalm.Equip(EquipSlot.Weap_Hud1);
            }
            else if (yalm2 != null)
            {
                yalm2.Equip(EquipSlot.Weap_Hud1);
            }
            else
            {
                Spell yalm3 = Spell.List.FirstOrDefault(x => RelevantNanos.Yalms.Contains(x.Id));

                if (yalm3 != null)
                    yalm3.Cast(false);
            }
        }

        private void OnYalmCancel(int sender, IPCMessage msg)
        {
            if (Inventory.Items.Where(x => x.Name.Contains("Yalm")).Where(x => x.Slot.Type == IdentityType.WeaponPage).Any())
            {
                Item yalm = Inventory.Items.Where(x => x.Name.Contains("Yalm")).Where(x => x.Slot.Type == IdentityType.WeaponPage).FirstOrDefault();

                if (yalm != null)
                    yalm.MoveToInventory();
            }
            else
                CancelBuffs(RelevantNanos.Yalms);
        }

        private void LeadFollowSwitch(string command, string[] param, ChatWindow chatWindow)
        {
            if (param.Length == 0)
            {
                _settings["Follow"] = !_settings["Follow"].AsBool();
                Chat.WriteLine($"Lead follow : {_settings["Follow"].AsBool()}");
            }
        }


        private void AutoSitSwitch(string command, string[] param, ChatWindow chatWindow)
        {
            if (param.Length == 0)
            {
                _settings["AutoSit"] = !_settings["AutoSit"].AsBool();
                Chat.WriteLine($"Auto sit : {_settings["AutoSit"].AsBool()}");
            }
        }

        private void YalmCommand(string command, string[] param, ChatWindow chatWindow)
        {
            if (DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.Yalms))
            {
                CancelBuffs(RelevantNanos.Yalms);
                IPCChannel.Broadcast(new YalmOffMessage());
            }
            else if (Inventory.Items.Where(x => x.Name.Contains("Yalm") || x.Name.Contains("Ganimedes")).Where(x => x.Slot.Type == IdentityType.WeaponPage).Any())
            {
                Item yalm = Inventory.Items.Where(x => x.Name.Contains("Yalm") || x.Name.Contains("Ganimedes")).Where(x => x.Slot.Type == IdentityType.WeaponPage).FirstOrDefault();

                if (yalm != null)
                {
                    yalm.MoveToInventory();

                    IPCChannel.Broadcast(new YalmOffMessage());
                }
            }
            else if (Inventory.Items.Where(x => x.Name.Contains("Yalm") || x.Name.Contains("Ganimedes")).Where(x => x.Slot.Type == IdentityType.Inventory).Any())
            {
                Item yalm = Inventory.Items.Where(x => x.Name.Contains("Yalm") || x.Name.Contains("Ganimedes")).Where(x => x.Slot.Type == IdentityType.Inventory).FirstOrDefault();

                if (yalm != null)
                {
                    yalm.Equip(EquipSlot.Weap_Hud1);

                    IPCChannel.Broadcast(new YalmUseMessage()
                    {
                        Item = yalm.HighId
                    });
                }
            }
            else 
            {
                Spell yalmbuff = Spell.List.FirstOrDefault(x => RelevantNanos.Yalms.Contains(x.Id));

                if (yalmbuff != null)
                {
                    yalmbuff.Cast(false);

                    IPCChannel.Broadcast(new YalmOnMessage()
                    {
                        Spell = yalmbuff.Id
                    });
                }
            }
        }

        private void DocTarget(string command, string[] param, ChatWindow chatWindow)
        {
            SimpleChar doctor = DynelManager.Characters
                .Where(c => c.IsAlive)
                .Where(c => c.Profession == Profession.Doctor)
                .Where(c => c.IsPlayer)
                .Where(c => !Team.Members.Contains(c.Identity))
                .Where(c => c.DistanceFrom(DynelManager.LocalPlayer) < 30f)
                .FirstOrDefault();

            if (doctor != null)
                Targeting.SetTarget(doctor);
        }

        private void Rebuff(string command, string[] param, ChatWindow chatWindow)
        {
            CancelAllBuffs();
            IPCChannel.Broadcast(new ClearBuffsMessage());
        }

        //private void HelpCommand(string command, string[] param, ChatWindow chatWindow)
        //{
        //    string help = "For team commands;\n" +
        //                    "\n" +
        //                    "/form and /form raid\n" +
        //                    "\n" +
        //                    "/disband\n" +
        //                    "\n" +
        //                    "/convert to convert to raid (must be done from leader)\n" +
        //                    "\n" +
        //                    "\n" +
        //                    "For shortcuts to /aosharp settings;\n" +
        //                    "\n" +
        //                    "/syncchat syncs chat from current player to all\n" +
        //                    "\n" +
        //                    "/synctrade syncs trade from current player to all\n" +
        //                    "\n" +
        //                    "/syncuse for syncing items from current player to all\n" +
        //                    "\n" +
        //                    "/sync for syncing trade from current player to all\n" +
        //                    "\n" +
        //                    "/autosit auto sits to use kits\n" +
        //                    "\n" +
        //                    "/allfollow name then /allfollow to toggle\n" +
        //                    "\n" +
        //                    "/yalm all will use yalm then /yalm to toggle\n" +
        //                    "\n" +
        //                    "/rebuff to clear buffs\n" +
        //                    "\n" +
        //                    "/navfollow name then /navfollow to toggle\n" +
        //                    "(Follow the npc or player using waypoints)\n" +
        //                    "\n" +
        //                    "/assistplayer name then /assistplayer to toggle\n" +
        //                    "(This is implemented to avoid KSing)\n" +
        //                    "\n" +
        //                    "Add clear to the end of each of these to clear the name\n" +
        //                    "\n" +
        //                    "\n" +
        //                    "For IPC Channel;\n" +
        //                    "\n" +
        //                    "/mbchannel # or /mbchannelall #\n" +
        //                    $"Currently: {Config.IPCChannel}";

        //    Chat.WriteLine(help, ChatColor.LightBlue);
        //}

        private void ListenerPetSit()
        {
            healpet = DynelManager.LocalPlayer.Pets.Where(x => x.Type == PetType.Heal).FirstOrDefault();

            Item kit = Inventory.Items.Where(x => RelevantItems.Kits.Contains(x.LowId)).FirstOrDefault();

            if (healpet == null || kit == null) { return; }

            if (_settings["AutoSit"].AsBool())
            {
                if (CanUseSitKit() && Time.NormalTime > _sitPetUsedTimer + 16
                    && DynelManager.LocalPlayer.DistanceFrom(healpet.Character) < 10f && healpet.Character.IsInLineOfSight)
                {
                    if (healpet.Character.Nano == 10) { return; }

                    if (healpet.Character.Nano / PetMaxNanoPool() * 100 > 55) { return; }

                    MovementController.Instance.SetMovement(MovementAction.SwitchToSit);

                    if (DynelManager.LocalPlayer.MovementState == MovementState.Sit)
                    {
                        kit.Use(healpet.Character, true);
                        Task.Factory.StartNew(
                            async () =>
                            {
                                await Task.Delay(100);
                                MovementController.Instance.SetMovement(MovementAction.LeaveSit);
                            });
                        _sitPetUsedTimer = Time.NormalTime;
                    }
                }
            }
        }

        private void ListenerSit()
        {
            Spell spell = Spell.List.FirstOrDefault(x => x.IsReady);

            Item kit = Inventory.Items.Where(x => RelevantItems.Kits.Contains(x.LowId)).FirstOrDefault();

            if (kit == null) { return; }

            if (spell != null && _settings["AutoSit"].AsBool())
            {
                if (!DynelManager.LocalPlayer.Buffs.Contains(280488) && CanUseSitKit())
                {
                    if (spell != null && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment) && Sitting == false
                        && DynelManager.LocalPlayer.MovementState != MovementState.Sit)
                    {
                        if (DynelManager.LocalPlayer.NanoPercent < 66 || DynelManager.LocalPlayer.HealthPercent < 66)
                        {
                            Task.Factory.StartNew(
                               async () =>
                               {
                                   Sitting = true;
                                   await Task.Delay(400);
                                   MovementController.Instance.SetMovement(MovementAction.SwitchToSit);
                                   await Task.Delay(800);
                                   MovementController.Instance.SetMovement(MovementAction.LeaveSit);
                                   await Task.Delay(200);
                                   Sitting = false;
                               });
                        }
                    }
                }
            }
        }

        public static void CancelAllBuffs()
        {
            //foreach (Buff buff in DynelManager.LocalPlayer.Buffs.Where(x => !RelevantNanos.DontRemoveNanos.Contains(x.Identity.Instance)))
            //{
            //    buff.Remove();
            //}

            foreach (Buff buff in DynelManager.LocalPlayer.Buffs
                .Where(x => !x.Name.Contains("Valid Pass")
                && x.Nanoline != NanoLine.BioMetBuff && x.Nanoline != NanoLine.MatCreaBuff
                && x.Nanoline != NanoLine.MatLocBuff && x.Nanoline != NanoLine.MatMetBuff
                && x.Nanoline != NanoLine.PsyModBuff && x.Nanoline != NanoLine.SenseImpBuff
                && x.Nanoline != NanoLine.TraderTeamSkillWranglerBuff
                && x.Nanoline != NanoLine.FixerNCUBuff))
            {
                buff.Remove();
            }
        }

        public static void CancelBuffs(int[] buffsToCancel)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs)
            {
                if (buffsToCancel.Contains(buff.Id))
                    buff.Remove();
            }
        }

        private void AssistView(object s, ButtonBase button)
        {
            if (followWindow != null && followWindow.IsValid)
            {
                if (!followWindow.Views.Contains(SettingsController.assistView))
                {
                    SettingsController.assistView = View.CreateFromXml(PluginDirectory + "\\UI\\HelpManagerAssistView.xml");

                    if (SettingsController.HelpManagerAssistPlayer != String.Empty)
                    {
                        SettingsController.assistView.FindChild("AssistNamedCharacter", out TextInputView textinput);

                        if (textinput != null)
                            textinput.Text = SettingsController.HelpManagerAssistPlayer;
                    }

                    followWindow.AppendTab("Assist", SettingsController.assistView);
                }
            }
            else
            {
                assistWindow = Window.CreateFromXml("Assist", PluginDirectory + "\\UI\\HelpManagerAssistView.xml",
                        windowSize: new Rect(0, 0, 220, 345),
                        windowStyle: WindowStyle.Default,
                        windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

                if (SettingsController.HelpManagerAssistPlayer != String.Empty)
                {
                    assistWindow.FindView("AssistNamedCharacter", out TextInputView textinput);

                    if (textinput != null)
                        textinput.Text = SettingsController.HelpManagerAssistPlayer;
                }

                assistWindow.Show(true);
            }
        }

        private void FollowView(object s, ButtonBase button)
        {
            if (assistWindow != null && assistWindow.IsValid)
            {
                if (!assistWindow.Views.Contains(SettingsController.followView))
                {
                    SettingsController.followView = View.CreateFromXml(PluginDirectory + "\\UI\\HelpManagerFollowView.xml");

                    if (SettingsController.HelpManagerFollowPlayer != String.Empty)
                    {
                        SettingsController.followView.FindChild("FollowNamedCharacter", out TextInputView textinput);

                        if (textinput != null)
                            textinput.Text = SettingsController.HelpManagerFollowPlayer;
                    }

                    if (SettingsController.HelpManagerNavFollowPlayer != String.Empty)
                    {
                        SettingsController.followView.FindChild("FollowNamedIdentity", out TextInputView textinput);

                        if (textinput != null)
                            textinput.Text = SettingsController.HelpManagerNavFollowPlayer;
                    }

                    if (SettingsController.HelpManagerNavFollowDistance.ToString() != String.Empty)
                    {
                        SettingsController.followView.FindChild("NavFollowDistanceBox", out TextInputView textinput);

                        if (textinput != null)
                            textinput.Text = SettingsController.HelpManagerNavFollowDistance.ToString();
                    }

                    assistWindow.AppendTab("Follow", SettingsController.followView);
                }
            }
            else
            {
                followWindow = Window.CreateFromXml("Follow", PluginDirectory + "\\UI\\HelpManagerFollowView.xml",
                        windowSize: new Rect(0, 0, 220, 345),
                        windowStyle: WindowStyle.Default,
                        windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

                if (SettingsController.HelpManagerFollowPlayer != String.Empty)
                {
                    followWindow.FindView("FollowNamedCharacter", out TextInputView textinput);

                    if (textinput != null)
                        textinput.Text = SettingsController.HelpManagerFollowPlayer;
                }

                if (SettingsController.HelpManagerNavFollowPlayer != String.Empty)
                {
                    followWindow.FindView("FollowNamedIdentity", out TextInputView textinput);

                    if (textinput != null)
                        textinput.Text = SettingsController.HelpManagerNavFollowPlayer;
                }

                if (SettingsController.HelpManagerNavFollowDistance.ToString() != String.Empty)
                {
                    followWindow.FindView("NavFollowDistanceBox", out TextInputView textinput);

                    if (textinput != null)
                        textinput.Text = SettingsController.HelpManagerNavFollowDistance.ToString();
                }

                followWindow.Show(true);
            }
        }

        private void InfoView(object s, ButtonBase button)
        {
            infoWindow = Window.CreateFromXml("Info", PluginDirectory + "\\UI\\HelpManagerInfoView.xml",
                windowSize: new Rect(0, 0, 440, 510),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            infoWindow.Show(true);
        }

        private bool BeingAttacked()
        {
            if (Team.IsInTeam)
            {
                return DynelManager.Characters
                    .Any(c => c.FightingTarget != null
                        && Team.Members.Select(m => m.Name).Contains(c.FightingTarget.Name));
            }

            return DynelManager.Characters
                    .Any(c => c.FightingTarget != null
                        && c.FightingTarget.Name == DynelManager.LocalPlayer.Name);
        }

        private bool CanUseSitKit()
        {
            List<Item> sitKits = Inventory.FindAll("Health and Nano Recharger").Where(c => c.LowId != 297274).ToList();

            if (Inventory.Find(297274, out Item premSitKit))
            {
                if (DynelManager.LocalPlayer.IsAlive && !BeingAttacked() && DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0
                    && !Team.IsInCombat() && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning) { return true; }
            }

            if (!sitKits.Any()) { return false; }

            if (DynelManager.LocalPlayer.IsAlive && !BeingAttacked() && DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0
                    && !Team.IsInCombat() && DynelManager.LocalPlayer.FightingTarget == null
                    && !DynelManager.LocalPlayer.IsMoving && !Game.IsZoning)
            {
                foreach (Item sitKit in sitKits.OrderBy(x => x.QualityLevel))
                {
                    int skillReq = (sitKit.QualityLevel > 200 ? (sitKit.QualityLevel % 200 * 3) + 1501 : (int)(sitKit.QualityLevel * 7.5f));

                    if (DynelManager.LocalPlayer.GetStat(Stat.FirstAid) >= skillReq || DynelManager.LocalPlayer.GetStat(Stat.Treatment) >= skillReq)
                        return true;
                }
            }

            return false;
        }

        private float PetMaxNanoPool()
        {
            if (healpet.Character.Level == 215)
                return 5803;
            else if (healpet.Character.Level == 192)
                return 13310;
            else if (healpet.Character.Level == 169)
                return 11231;
            else if (healpet.Character.Level == 146)
                return 9153;
            else if (healpet.Character.Level == 123)
                return 7169;
            else if (healpet.Character.Level == 99)
                return 5327;
            else if (healpet.Character.Level == 77)
                return 3807;
            else if (healpet.Character.Level == 55)
                return 2404;
            else if (healpet.Character.Level == 33)
                return 1234;
            else if (healpet.Character.Level == 14)
                return 414;

            return 0;
        }

        public enum FollowSelection
        {
            None, LeadFollow, OSFollow, NavFollow
        }

        public enum AttackSelection
        {
            None, Assist
        }

        private static class RelevantNanos
        {
            public static readonly int[] ZixMorph = { 288532, 302212 };
            public static readonly int[] Yalms = {
                290473, 281569, 301672, 270984, 270991, 273468, 288795, 270993, 270995, 270986, 270982,
                296034, 296669, 304437, 270884, 270941, 270836, 287285, 288816, 270943, 270939, 270945,
                270711, 270731, 270645, 284061, 288802, 270764, 277426, 288799, 270738, 270779, 293619,
                294781, 301669, 301700, 301670, 120499, 82835
            };
            //public static readonly int[] DontRemoveNanos = {};
        }

        private static class RelevantItems
        {
            public static readonly int[] Kits = {
                297274, 293296, 291084, 291083, 291082
            };
        }
    }
}