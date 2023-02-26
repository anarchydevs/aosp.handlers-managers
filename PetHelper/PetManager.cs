﻿using System;
using System.Linq;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using PetManager.IPCMessages;
using System.Runtime.InteropServices;
using AOSharp.Common.GameData.UI;

namespace PetManager
{
    public class PetManager : AOPluginEntry
    {
        private static IPCChannel IPCChannel;

        public static Config Config { get; private set; }

        public static string PluginDirectory;

        public static Window _infoWindow;

        public static View _infoView;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        protected Settings _settings;

        public static string PluginDir;

        public override void Run(string pluginDir)
        {

            Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\PetManager\\{Game.ClientInst}\\Config.json");
            IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));

            IPCChannel.RegisterCallback((int)IPCOpcode.PetWait, OnPetWait);
            IPCChannel.RegisterCallback((int)IPCOpcode.PetFollow, OnPetFollow);
            IPCChannel.RegisterCallback((int)IPCOpcode.PetWarp, OnPetWarp);

            PluginDir = pluginDir;

            _settings = new Settings("PetManager");

            Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed; ;

            RegisterSettingsWindow("Pet Manager", "PetManagerSettingWindow.xml");

            Game.OnUpdate += OnUpdate;

            Chat.RegisterCommand("petwait", PetWaitCommand);
            Chat.RegisterCommand("petwarp", PetWarpCommand);
            Chat.RegisterCommand("petfollow", PetFollowCommand);

            Chat.WriteLine("PetManager Loaded!");
            Chat.WriteLine("/PetManager for settings.");

            PluginDirectory = pluginDir;
        }

        public Window[] _windows => new Window[] { _infoWindow };

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));
            Config.Save();
        }

        private void InfoView(object s, ButtonBase button)
        {
            _infoWindow = Window.CreateFromXml("Info", PluginDirectory + "\\UI\\PetManagerInfoView.xml",
                windowSize: new Rect(0, 0, 140, 300),
                windowStyle: WindowStyle.Default,
                windowFlags: WindowFlags.AutoScale | WindowFlags.NoFade);

            _infoWindow.Show(true);
        }

        protected void RegisterSettingsWindow(string settingsName, string xmlName)
        {
            SettingsController.RegisterSettingsWindow(settingsName, PluginDir + "\\UI\\" + xmlName, _settings);
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);
                
                    if (channelInput != null && !string.IsNullOrEmpty(channelInput.Text))
                    {
                        if (int.TryParse(channelInput.Text, out int channelValue)
                            && Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                        {
                            Config.CharSettings[Game.ClientInst].IPCChannel = channelValue;
                        }
                    }

                if (SettingsController.settingsWindow.FindView("PetManagerInfoView", out Button infoView))
                {
                    infoView.Tag = SettingsController.settingsWindow;
                    infoView.Clicked = InfoView; 
                }

                //wait
                if (SettingsController.settingsWindow.FindView("PetWait", out Button PetWait))
                { 
                    PetWait.Tag = SettingsController.settingsWindow;
                    PetWait.Clicked = PetWaitClicked;
                }

                //follow
                if (SettingsController.settingsWindow.FindView("PetFollow", out Button PetFollow))
                {
                    PetFollow.Tag = SettingsController.settingsWindow;
                    PetFollow.Clicked = PetFollowClicked;
                }

                //warp
                if (SettingsController.settingsWindow.FindView("PetWarp", out Button PetWarp))
                {
                    PetWarp.Tag = SettingsController.settingsWindow;
                    PetWarp.Clicked = PetWarpClicked;
                }
            }
        }

        //wait
        private void PetWaitClicked(object s, ButtonBase button)
        {
            PetWaitCommand(null, null, null);
        }

        private static void PetWaitCommand(string command, string[] param, ChatWindow chatWindow)
        {
            IPCChannel.Broadcast(new PetWaitMessage());
            OnPetWait(0, null);
        }

        private static void OnPetWait(int sender, IPCMessage msg)
        {
            if (DynelManager.LocalPlayer.Pets.Length > 0)
            {
                foreach (Pet pet in DynelManager.LocalPlayer.Pets)
                {
                    pet.Wait();
                }
            }
        }

        //warp
        private void PetWarpClicked(object s, ButtonBase button)
        {
            PetWarpCommand(null, null, null);
        }

        private static void PetWarpCommand(string command, string[] param, ChatWindow chatWindow)
        {
            IPCChannel.Broadcast(new PetWarpMessage());
            OnPetWarp(0, null);
        }

        private static void OnPetWarp(int sender, IPCMessage msg)
        {
            if (DynelManager.LocalPlayer.Pets.Length > 0)
            {
                Spell warp = Spell.List.FirstOrDefault(x => RelevantNanos.Warps.Contains(x.Id));
                if (warp != null)
                {
                    warp.Cast(DynelManager.LocalPlayer, false);
                }
            }
        }

        //follow
        private void PetFollowClicked(object s, ButtonBase button)
        {
            PetFollowCommand(null, null, null);   
        }

        private void PetFollowCommand(string command, string[] param, ChatWindow chatWindow)
        {
            IPCChannel.Broadcast(new PetFollowMessage());
            OnPetFollow(0, null);
        }

        private static void OnPetFollow(int sender, IPCMessage msg)
        {
            if (DynelManager.LocalPlayer.Pets.Length > 0)
            {
                foreach (Pet pet in DynelManager.LocalPlayer.Pets)
                {
                    pet.Follow();
                }
            }
        }

        private static class RelevantNanos
        {
            public static readonly int[] Warps = {
                209488
            };
        }
    }
}
