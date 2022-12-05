﻿using AOSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using AOSharp.Common.GameData;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using AOSharp.Core.IPC;
using AOSharp.Common.GameData.UI;
using System.Threading.Tasks;
using AOSharp.Common.Unmanaged.DataTypes;
using AOSharp.Common.Unmanaged.Imports;
using AOSharp.Common.Helpers;
using System.Runtime.InteropServices;
using System.Windows.Input;
using Newtonsoft.Json;
using System.Data;
using System.IO;
using AOSharp.Core.Movement;

namespace LootManager
{
    public class LootManager : AOPluginEntry
    {
        private double _lastCheckTime = Time.NormalTime;

        public static List<MultiListViewItem> MultiListViewItemList = new List<MultiListViewItem>();
        public static Dictionary<ItemModel, MultiListViewItem> PreItemList = new Dictionary<ItemModel, MultiListViewItem>();

        private static List<Vector3> _corpsePosList = new List<Vector3>();
        private static Vector3 _currentPos = Vector3.Zero;
        private static List<Identity> _corpseIdList = new List<Identity>();

        private static int MinQlValue;
        private static int MaxQlValue;
        private static int ItemIdValue;
        private static string ItemNameValue;

        public static List<Rule> Rules;

        protected Settings _settings;
        public static Settings _settingsItems;

        private static bool _init = false;
        private static bool _internalOpen = false;
        private static bool _weAreDoingThings = false;
        private static bool _currentlyLooting = false;

        private static bool Looting = true;
        private static bool Bags = false;

        private static double _nowTimer = Time.NormalTime;

        private static int _currentIgnore = 0;

        private Window _infoWindow;

        private static List<Item> _invItems = new List<Item>();

        private static List<string> _ignores = new List<string>();

        public static string PluginDir;

        public override void Run(string pluginDir)
        {
            try
            {
                _settings = new Settings("LootManager");
                PluginDir = pluginDir;

                Game.OnUpdate += OnUpdate;
                Inventory.ContainerOpened += OnContainerOpened;

                RegisterSettingsWindow("Loot Manager", "LootManagerSettingWindow.xml");

                LoadRules();

                Chat.WriteLine("Loot Manager loaded!");
                Chat.WriteLine("/lootmanager for settings.");
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        public override void Teardown()
        {
            SaveRules();
            SettingsController.CleanUp();
        }

        private bool ItemExists(Item item)
        {
            if (Inventory.Items.Contains(item)) { return true; }

            foreach (Backpack backpack in Inventory.Backpacks.Where(c => c.Name.Contains("loot")))
            {
                if (backpack.Items.Contains(item))
                    return true;
            }

            return false;
        }

        private static Backpack FindBagWithSpace()
        {
            foreach (Backpack backpack in Inventory.Backpacks.Where(c => c.Name.Contains("loot")))
            {
                if (backpack.Items.Count < 21)
                    return backpack;
            }

            return null;
        }

        private void OnContainerOpened(object sender, Container container)
        {
            if (container.Identity.Type != IdentityType.Corpse
                || !_internalOpen
                || !_weAreDoingThings) { return; }

            _currentlyLooting = true;

            foreach (Item item in container.Items)
            {
                if (Inventory.NumFreeSlots >= 1)
                {
                    if (CheckRules(item))
                        item.MoveToInventory();
                    else if (!_ignores.Contains(item.Name))
                        item.MoveToInventory();
                }
                else
                {
                    Backpack _bag = FindBagWithSpace();

                    if (_bag == null) { return; }

                    foreach (Item itemtomove in Inventory.Items.Where(c => c.Slot.Type == IdentityType.Inventory))
                    {
                        if (_invItems.Contains(itemtomove)) { continue; }

                        itemtomove.MoveToContainer(_bag);
                    }

                    if (CheckRules(item))
                        item.MoveToInventory();
                    else if (!_ignores.Contains(item.Name))
                        item.MoveToInventory();
                }
            }

            _corpsePosList.Add(_currentPos);
            _corpseIdList.Add(container.Identity);
            //Chat.WriteLine($"Adding bits");
            Item.Use(container.Identity);
            _currentlyLooting = false;
            _internalOpen = false;
            _weAreDoingThings = false;
        }

        private void OnUpdate(object sender, float deltaTime)
        {
            if (Looting)
            {
                //Stupid correction - for if we try looting and someone else is looting or we are moving and just get out of range before the tick...
                if (_internalOpen && _weAreDoingThings && Time.NormalTime > _nowTimer + 2f)
                {
                    if (_currentlyLooting) { return; }

                    //Chat.WriteLine($"Resetting");
                    //Sigh
                    _internalOpen = false;
                    _weAreDoingThings = false;
                }

                if (_weAreDoingThings) { return; }

                //Tidying up of the stupid ass logic
                foreach (Vector3 corpsePos in _corpsePosList)
                    if (DynelManager.Corpses.Where(c => c.Position == corpsePos).ToList().Count == 0)
                    {
                        _corpsePosList.Remove(corpsePos);
                        //Chat.WriteLine($"Removing vector3");
                        return;
                    }

                foreach (Identity corpseId in _corpseIdList)
                    if (DynelManager.Corpses.Where(c => c.Identity == corpseId).ToList().Count == 0)
                    {
                        _corpseIdList.Remove(corpseId);
                        //Chat.WriteLine($"Removing identity");
                        return;
                    }

                foreach (Corpse corpse in DynelManager.Corpses.Where(c => c.DistanceFrom(DynelManager.LocalPlayer) < 7
                    && !_corpsePosList.Contains(c.Position)
                    && !_corpseIdList.Contains(c.Identity)).Take(3))
                {
                    Corpse _corpse = DynelManager.Corpses.FirstOrDefault(c =>
                        c.Identity != corpse.Identity
                        && c.Position.DistanceFrom(corpse.Position) <= 1f);

                    if (_corpse != null || _weAreDoingThings) { continue; }

                    //Chat.WriteLine($"Opening");
                    //This is so we can open ourselves without the event auto closing
                    _internalOpen = true;
                    //Sigh
                    _weAreDoingThings = true;
                    _nowTimer = Time.NormalTime;
                    corpse.Open();

                    //This is so we can pass the vector to the event
                    _currentPos = corpse.Position;
                }
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                if (SettingsController.settingsWindow.FindView("chkOnOff", out Checkbox chkOnOff))
                {
                    chkOnOff.SetValue(Looting);
                    if (chkOnOff.Toggled == null)
                        chkOnOff.Toggled += chkOnOff_Toggled;
                }

                //if (SettingsController.settingsWindow.FindView("chkBags", out Checkbox chkBags))
                //{
                //    chkBags.SetValue(Bags);
                //    if (chkBags.Toggled == null)
                //        chkBags.Toggled += chkBags_Toggled;
                //}

                if (SettingsController.settingsWindow.FindView("buttonAdd", out Button addbut))
                {
                    if (addbut.Clicked == null)
                        addbut.Clicked += addButtonClicked;
                }

                if (SettingsController.settingsWindow.FindView("buttonDel", out Button rembut))
                {
                    if (rembut.Clicked == null)
                        rembut.Clicked += remButtonClicked;
                }

                if (SettingsController.settingsWindow.FindView("tivminql", out TextInputView tivminql))
                {
                    tivminql.Text = "1";
                }
                if (SettingsController.settingsWindow.FindView("tivmaxql", out TextInputView tivmaxql))
                {
                    tivmaxql.Text = "500";
                }
            }
        }

        private void chkBags_Toggled(object sender, bool e)
        {
            Checkbox chk = (Checkbox)sender;
            Bags = e;
        }

        private void chkOnOff_Toggled(object sender, bool e)
        {
            Checkbox chk = (Checkbox)sender;
            Looting = e;
        }

        private void addButtonClicked(object sender, ButtonBase e)
        {
            SettingsController.settingsWindow.FindView("ScrollListRoot", out MultiListView mlv);

            SettingsController.settingsWindow.FindView("tivName", out TextInputView tivname);
            SettingsController.settingsWindow.FindView("tivminql", out TextInputView tivminql);
            SettingsController.settingsWindow.FindView("tivmaxql", out TextInputView tivmaxql);

            SettingsController.settingsWindow.FindView("tvErr", out TextView txErr);

            if (tivname.Text.Trim() == "")
            {
                txErr.Text = "Can't add an empty name";
                return;
            }

            int minql = 0;
            int maxql = 0;
            try
            {
                minql = Convert.ToInt32(tivminql.Text);
                maxql = Convert.ToInt32(tivmaxql.Text);
            }
            catch
            {
                txErr.Text = "Quality entries must be numbers!";
                return;
            }

            if (minql > maxql)
            {
                txErr.Text = "Min Quality must be less or equal than the high quality!";
                return;
            }
            if (minql <= 0)
            {
                txErr.Text = "Min Quality must be least 1!";
                return;
            }
            if (maxql > 500)
            {
                txErr.Text = "Max Quality must be 500!";
                return;
            }


            SettingsController.settingsWindow.FindView("chkGlobal", out Checkbox chkGlobal);
            bool GlobalScope = chkGlobal.IsChecked;


            mlv.DeleteAllChildren();



            Rules.Add(new Rule(tivname.Text, tivminql.Text, tivmaxql.Text, GlobalScope));

            Rules = Rules.OrderBy(o => o.Name.ToUpper()).ToList();

            int iEntry = 0;
            foreach (Rule r in Rules)
            {
                View entry = View.CreateFromXml(PluginDir + "\\UI\\ItemEntry.xml");
                entry.FindChild("ItemName", out TextView tx);
                string globalscope = "";
                if (r.Global)
                    globalscope = "G";
                else
                    globalscope = "N";

                //entry.Tag = iEntry;
                tx.Text = (iEntry + 1).ToString() + " - " + globalscope + " - [" + r.Lql.PadLeft(3, ' ') + "-" + r.Hql.PadLeft(3, ' ') + "] - " + r.Name;

                mlv.AddChild(entry, false);
                iEntry++;
            }


            tivname.Text = "";
            tivminql.Text = "1";
            tivmaxql.Text = "500";
            txErr.Text = "";

        }

        private void remButtonClicked(object sender, ButtonBase e)
        {
            try
            {
                SettingsController.settingsWindow.FindView("ScrollListRoot", out MultiListView mlv);

                SettingsController.settingsWindow.FindView("tivindex", out TextInputView txIndex);

                SettingsController.settingsWindow.FindView("tvErr", out TextView txErr);

                if (txIndex.Text.Trim() == "")
                {
                    txErr.Text = "Cant remove an empty entry";
                    return;
                }

                int index = 0;

                try
                {
                    index = Convert.ToInt32(txIndex.Text) - 1;
                }
                catch
                {
                    txErr.Text = "Entry must be a number!";
                    return;
                }

                if (index < 0 || index >= Rules.Count)
                {
                    txErr.Text = "Invalid entry!";
                    return;
                }

                Rules.RemoveAt(index);

                mlv.DeleteAllChildren();
                //viewitems.Clear();

                int iEntry = 0;
                foreach (Rule r in Rules)
                {
                    View entry = View.CreateFromXml(PluginDir + "\\UI\\ItemEntry.xml");
                    entry.FindChild("ItemName", out TextView tx);

                    //entry.Tag = iEntry;

                    string scope = "";
                    if (r.Global)
                        scope = "G";
                    else
                        scope = "N";
                    tx.Text = (iEntry + 1).ToString() + " - " + scope + " - [" + r.Lql.PadLeft(3, ' ') + "-" + r.Hql.PadLeft(3, ' ') + "] - " + r.Name;


                    mlv.AddChild(entry, false);
                    iEntry++;
                }

                txErr.Text = "";
            }
            catch (Exception ex)
            {

                Chat.WriteLine(ex.Message);
            }
        }

        protected void RegisterSettingsWindow(string settingsName, string xmlName)
        {
            SettingsController.RegisterSettingsWindow(settingsName, PluginDir + "\\UI\\" + xmlName, _settings);
        }

        private void LoadRules()
        {
            Rules = new List<Rule>();

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LootManager"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LootManager");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager\\{DynelManager.LocalPlayer.Name}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager\\{DynelManager.LocalPlayer.Name}");

            string filename = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager\\Global.json";
            if (File.Exists(filename))
            {
                string rulesJson = File.ReadAllText(filename);
                Rules = JsonConvert.DeserializeObject<List<Rule>>(rulesJson);
                foreach (Rule r in Rules)
                    r.Global = true;
            }


            filename = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager\\{DynelManager.LocalPlayer.Name}\\Rules.json";
            if (File.Exists(filename))
            {
                List<Rule> scopedRules = new List<Rule>();
                string rulesJson = File.ReadAllText(filename);
                scopedRules = JsonConvert.DeserializeObject<List<Rule>>(rulesJson);
                foreach (Rule r in scopedRules)
                {
                    r.Global = false;
                    Rules.Add(r);
                }
            }
            Rules = Rules.OrderBy(o => o.Name.ToUpper()).ToList();
        }

        private void SaveRules()
        {
            List<Rule> GlobalRules = new List<Rule>();
            List<Rule> ScopeRules = new List<Rule>();

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LootManager"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\LootManager");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager\\{DynelManager.LocalPlayer.Name}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager\\{DynelManager.LocalPlayer.Name}");

            string filename = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager\\Global.json";

            GlobalRules = Rules.Where(o => o.Global == true).ToList();
            ScopeRules = Rules.Where(o => o.Global == false).ToList();

            string rulesJson = JsonConvert.SerializeObject(GlobalRules);
            File.WriteAllText(filename, rulesJson);

            filename = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\LootManager\\{DynelManager.LocalPlayer.Name}\\Rules.json";
            rulesJson = JsonConvert.SerializeObject(ScopeRules);
            File.WriteAllText(filename, rulesJson);
        }

        public bool CheckRules(Item item)
        {
            foreach (Rule rule in Rules)
            {
                if (
                    item.Name.ToUpper().Contains(rule.Name.ToUpper()) &&
                    item.QualityLevel >= Convert.ToInt32(rule.Lql) &&
                    item.QualityLevel <= Convert.ToInt32(rule.Hql)
                    )
                    return true;

            }
            return false;
        }

    }

    [StructLayout(LayoutKind.Explicit, Pack = 0)]
    public struct MemStruct
    {
        [FieldOffset(0x14)]
        public Identity Identity;

        [FieldOffset(0x9C)]
        public IntPtr Name;
    }

    public class RemoveItemModel
    {
        public MultiListView MultiListView;
        public MultiListViewItem MultiListViewItem;
        public View ViewSettings;
        public View ViewButton;
    }

    public class SettingsViewModel
    {
        public string Type;
        public MultiListView MultiListView;
        public Dictionary<ItemModel, MultiListViewItem> Dictionary;
    }

    public class ItemModel
    {
        public string ItemName;
        public int LowId;
        public int HighId;
        public int Ql;
    }
}
