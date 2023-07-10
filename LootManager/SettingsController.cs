﻿using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;

namespace LootManager
{
    public static class SettingsController
    {
        private static readonly List<Settings> settingsToSave = new List<Settings>();
        private static readonly Dictionary<string, string> settingsWindows = new Dictionary<string, string>();
        private static bool isCommandRegistered;

        public static MultiListView searchList;

        public static string NameValue = string.Empty;
        public static int ItemIdValue = 0;
        public static int MinQlValue = 0;
        public static int MaxQlValue = 0;
        public static int QtyValue = 0;

        public static Window settingsWindow;

        public static void RegisterCharacters(Settings settings)
        {
            RegisterChatCommandIfNotRegistered();
            settingsToSave.Add(settings);
        }

        public static void RegisterSettingsWindow(string settingsName, string settingsWindowPath, Settings settings)
        {
            RegisterChatCommandIfNotRegistered();
            settingsWindows[settingsName] = settingsWindowPath;
            settingsToSave.Add(settings);
        }

        public static void RegisterSettings(Settings settings)
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
            if (!isCommandRegistered)
            {
                Chat.RegisterCommand("lootmanager", (string command, string[] param, ChatWindow chatWindow) =>
                {
                    try
                    {
                        settingsWindow = Window.Create(new Rect(50, 50, 300, 300), "Loot Manager", "Settings", WindowStyle.Default, WindowFlags.AutoScale);

                        if (settingsWindow != null && !settingsWindow.IsVisible)
                        {
                            AppendSettingsTab("Loot Manager", settingsWindow);

                            if (settingsWindow.FindView("ScrollListRoot", out MultiListView mlv) &&
                                settingsWindow.FindView("tivminql", out TextInputView tivminql) &&
                                settingsWindow.FindView("tivmaxql", out TextInputView tivmaxql))
                            {
                                tivminql.Text = "1";
                                tivmaxql.Text = "500";
                                mlv.DeleteAllChildren();
                                int iEntry = 0;
                                foreach (Rule r in LootManager.Rules)
                                {
                                    View entry = View.CreateFromXml(LootManager.PluginDir + "\\UI\\ItemEntry.xml");
                                    entry.FindChild("ItemName", out TextView tx);

                                    string scope = r.Global ? "Global" : "Local";
                                    tx.Text = (iEntry + 1).ToString() + " - " + scope + " - [" + r.Lql.PadLeft(3, ' ') + "-" + r.Hql.PadLeft(3, ' ') + "  ] - " + r.Name;

                                    mlv.AddChild(entry, false);
                                    iEntry++;
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Chat.WriteLine(e);
                    }
                });

                isCommandRegistered = true;
            }
        }

        public static void AppendSettingsTab(string settingsName, Window testWindow)
        {
            if (settingsWindows.TryGetValue(settingsName, out string settingsWindowXmlPath))
            {
                View settingsView = View.CreateFromXml(settingsWindowXmlPath);

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

        private static void SaveSettings()
        {
            settingsToSave.ForEach(settings => settings.Save());
        }
    }
}
