﻿using AOSharp.Common.GameData;
using AOSharp.Core.UI;
using AOSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData.UI;

namespace AutoLoot
{
    public static class SettingsController
    {
        private static List<AOSharp.Core.Settings> settingsToSave = new List<AOSharp.Core.Settings>();
        public static Dictionary<string, string> settingsWindows = new Dictionary<string, string>();
        private static bool IsCommandRegistered;

        public static Window settingsWindow;
        public static View settingsView;

        public static void RegisterCharacters(AOSharp.Core.Settings settings)
        {
            RegisterChatCommandIfNotRegistered();
            settingsToSave.Add(settings);
        }

        public static void RegisterSettingsWindow(string settingsName, string settingsWindowPath, AOSharp.Core.Settings settings)
        {
            RegisterChatCommandIfNotRegistered();
            settingsWindows[settingsName] = settingsWindowPath;
            settingsToSave.Add(settings);
        }

        public static void RegisterSettings(AOSharp.Core.Settings settings)
        {
            RegisterChatCommandIfNotRegistered();
            settingsToSave.Add(settings);
        }

        public static void CleanUp()
        {
            settingsToSave.ForEach(settings => settings.Save());
        }

        private static void RegisterChatCommandIfNotRegistered()
        {
            if (!IsCommandRegistered)
            {
                Chat.RegisterCommand("helper", (string command, string[] param, ChatWindow chatWindow) =>
                {
                    try
                    {
                        settingsWindow = Window.Create(new Rect(50, 50, 300, 300), "MultiboxHelper", "Settings", WindowStyle.Default, WindowFlags.AutoScale);

                        foreach (string settingsName in settingsWindows.Keys)
                        {
                            AppendSettingsTab(settingsName, settingsWindow);

                        }
                    }
                    catch (Exception e)
                    {
                        Chat.WriteLine(e);
                    }
                });

                IsCommandRegistered = true;
            }
        }

        public static void AppendSettingsTab(String settingsName, Window testWindow)
        {
            String settingsWindowXmlPath = settingsWindows[settingsName];
            settingsView = View.CreateFromXml(settingsWindowXmlPath);
            if (settingsView != null)
            {
                testWindow.AppendTab(settingsName, settingsView);
                testWindow.Show(true);
            }
            else
            {
                Chat.WriteLine("Failed to load settings schema from " + settingsWindowXmlPath);
            }
        }
    }
}