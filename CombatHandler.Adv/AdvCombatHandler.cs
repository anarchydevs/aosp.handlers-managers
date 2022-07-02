﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Linq;
using System;
using AOSharp.Common.GameData.UI;
using AOSharp.Core.IPC;
using System.Threading.Tasks;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System.Threading;
using SmokeLounge.AOtomation.Messaging.Messages;
using System.Collections.Generic;
using AOSharp.Core.Inventory;
using CombatHandler.Generic;

namespace CombatHandler.Adventurer
{
    public class AdvCombatHandler : GenericCombatHandler
    {
        private static string PluginDirectory;

        private static int AdvHealPercentage;
        private static int AdvCompleteHealPercentage;

        private static Window _morphWindow;
        private static Window _healingWindow;

        private static View _morphView;
        private static View _healingView;

        private static double _ncuUpdateTime;

        public AdvCombatHandler(string pluginDir) : base(pluginDir)
        {
            IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));
            IPCChannel.RegisterCallback((int)IPCOpcode.RemainingNCU, OnRemainingNCUMessage);

            Config.CharSettings[Game.ClientInst].AdvHealPercentageChangedEvent += AdvHealPercentage_Changed;
            Config.CharSettings[Game.ClientInst].AdvCompleteHealPercentageChangedEvent += AdvCompleteHealPercentage_Changed;

            _settings.AddVariable("HealSelection", (int)HealSelection.None);

            _settings.AddVariable("DragonMorph", false);
            _settings.AddVariable("LeetMorph", false);
            _settings.AddVariable("SaberMorph", false);
            _settings.AddVariable("WolfMorph", false);

            _settings.AddVariable("ArmorBuff", false);

            _settings.AddVariable("CH", false);

            RegisterSettingsWindow("Adventurer Handler", "AdvSettingsView.xml");

            RegisterSettingsWindow("Healing", "AdvHealingView.xml");
            RegisterSettingsWindow("Morphs", "AdvMorphView.xml");

            //LE Procs
            RegisterPerkProcessor(PerkHash.LEProcAdventurerMacheteFlurry, LEProc);
            RegisterPerkProcessor(PerkHash.LEProcAdventurerCombustion, LEProc);

            //Spells
            RegisterSpellProcessor(RelevantNanos.HEALS, Healing, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.CompleteHealingLine).OrderByStackingOrder(), CompleteHealing, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TeamHealing).OrderByStackingOrder(), TeamHealing, CombatActionPriority.High);

            //Buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.General1HEdgedBuff).OrderByStackingOrder(), MeleeBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PistolBuff).OrderByStackingOrder(), RangedBuff);
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ArmorBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DamageShieldUpgrades).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DamageShields).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MultiwieldBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(RelevantNanos.ArmorBuffs, ArmorBuff);

            //Morphs
            RegisterSpellProcessor(RelevantNanos.DragonMorph, DragonMorph);
            RegisterSpellProcessor(RelevantNanos.LeetMorph, LeetMorph);
            RegisterSpellProcessor(RelevantNanos.WolfMorph, WolfMorph);
            RegisterSpellProcessor(RelevantNanos.SaberMorph, SaberMorph);

            RegisterSpellProcessor(RelevantNanos.DragonScales, DragonScales);
            RegisterSpellProcessor(RelevantNanos.LeetCrit, LeetCrit);
            RegisterSpellProcessor(RelevantNanos.WolfAgility, WolfAgility);
            RegisterSpellProcessor(RelevantNanos.SaberDamage, SaberDamage);

            PluginDirectory = pluginDir;

            //Items
            //RegisterItemProcessor(RelevantItems.TheWizdomOfHuzzum, RelevantItems.TheWizdomOfHuzzum, MartialArtsTeamHealAttack);

        }

        public Window[] _windows => new Window[] { _morphWindow, _healingWindow };

        public static void AdvHealPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].DocHealPercentage = e;
            //TODO: Change in config so it saves when needed to - interface name -> INotifyPropertyChanged
            Config.Save();
        }

        public static void AdvCompleteHealPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].DocCompleteHealPercentage = e;
            //TODO: Change in config so it saves when needed to - interface name -> INotifyPropertyChanged
            Config.Save();
        }
        public static void OnRemainingNCUMessage(int sender, IPCMessage msg)
        {
            try
            {
                if (Game.IsZoning)
                    return;

                RemainingNCUMessage ncuMessage = (RemainingNCUMessage)msg;
                SettingsController.RemainingNCU[ncuMessage.Character] = ncuMessage.RemainingNCU;
            }
            catch (Exception e)
            {
                Chat.WriteLine(e);
            }
        }

        private void HandleHealingViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                //Cannot stop Multi-Tabs. Easy fix would be correct naming of views to reference against WindowOptions - options.Name
                _healingView = View.CreateFromXml(PluginDirectory + "\\UI\\AdvHealingView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Healing", XmlViewName = "AdvHealingView" }, _healingView);

                window.FindView("HealPercentageBox", out TextInputView textinput1);
                window.FindView("CompleteHealPercentageBox", out TextInputView textinput2);

                if (textinput1 != null && string.IsNullOrEmpty(textinput1.Text))
                {
                    textinput1.Text = $"{AdvHealPercentage}";
                }

                if (textinput2 != null && string.IsNullOrEmpty(textinput2.Text))
                {
                    textinput2.Text = $"{AdvCompleteHealPercentage}";
                }
            }
            else if (_healingWindow == null || (_healingWindow != null && !_healingWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_healingWindow, PluginDir, new WindowOptions() { Name = "Healing", XmlViewName = "AdvHealingView" }, _healingView, out var container);
                _healingWindow = container;

                container.FindView("HealPercentageBox", out TextInputView textinput1);
                container.FindView("CompleteHealPercentageBox", out TextInputView textinput2);

                if (textinput1 != null && string.IsNullOrEmpty(textinput1.Text))
                {
                    textinput1.Text = $"{AdvHealPercentage}";
                }

                if (textinput2 != null && string.IsNullOrEmpty(textinput2.Text))
                {
                    textinput2.Text = $"{AdvCompleteHealPercentage}";
                }
            }
        }

        private void HandleMorphViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                //Cannot stop Multi-Tabs. Easy fix would be correct naming of views to reference against WindowOptions - options.Name
                _morphView = View.CreateFromXml(PluginDirectory + "\\UI\\AdvMorphView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Debuffs", XmlViewName = "AdvMorphView" }, _morphView);
            }
            else if (_morphWindow == null || (_morphWindow != null && !_morphWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_morphWindow, PluginDir, new WindowOptions() { Name = "Debuffs", XmlViewName = "AdvMorphView" }, _morphView, out var container);
                _morphWindow = container;
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (Time.NormalTime > _ncuUpdateTime + 0.5f)
            {
                RemainingNCUMessage ncuMessage = RemainingNCUMessage.ForLocalPlayer();

                IPCChannel.Broadcast(ncuMessage);

                OnRemainingNCUMessage(0, ncuMessage);

                _ncuUpdateTime = Time.NormalTime;
            }

            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                window.FindView("HealPercentageBox", out TextInputView textinput1);
                window.FindView("CompleteHealPercentageBox", out TextInputView textinput2);

                if (textinput1 != null && !string.IsNullOrEmpty(textinput1.Text))
                {
                    if (int.TryParse(textinput1.Text, out int healValue))
                    {
                        if (Config.CharSettings[Game.ClientInst].AdvHealPercentage != healValue)
                        {
                            Config.CharSettings[Game.ClientInst].AdvHealPercentage = healValue;
                            AdvHealPercentage = healValue;
                            Config.Save();
                        }
                    }
                }
                if (textinput2 != null && !string.IsNullOrEmpty(textinput2.Text))
                {
                    if (int.TryParse(textinput2.Text, out int completeHealValue))
                    {
                        if (Config.CharSettings[Game.ClientInst].AdvCompleteHealPercentage != completeHealValue)
                        {
                            Config.CharSettings[Game.ClientInst].AdvCompleteHealPercentage = completeHealValue;
                            AdvCompleteHealPercentage = completeHealValue;
                            Config.Save();
                        }
                    }
                }
            }

            if (_settings["DragonMorph"].AsBool() && _settings["LeetMorph"].AsBool())
            {
                _settings["DragonMorph"] = false;
                _settings["LeetMorph"] = false;

                Chat.WriteLine("Only activate one Morph option.");
            }
            if (_settings["DragonMorph"].AsBool() && _settings["SaberMorph"].AsBool())
            {
                _settings["DragonMorph"] = false;
                _settings["SaberMorph"] = false;

                Chat.WriteLine("Only activate one Morph option.");
            }
            if (_settings["DragonMorph"].AsBool() && _settings["WolfMorph"].AsBool())
            {
                _settings["DragonMorph"] = false;
                _settings["WolfMorph"] = false;

                Chat.WriteLine("Only activate one Morph option.");
            }
            if (_settings["SaberMorph"].AsBool() && _settings["LeetMorph"].AsBool())
            {
                _settings["SaberMorph"] = false;
                _settings["LeetMorph"] = false;

                Chat.WriteLine("Only activate one Morph option.");
            }
            if (_settings["SaberMorph"].AsBool() && _settings["WolfMorph"].AsBool())
            {
                _settings["SaberMorph"] = false;
                _settings["WolfMorph"] = false;

                Chat.WriteLine("Only activate one Morph option.");
            }
            if (_settings["LeetMorph"].AsBool() && _settings["WolfMorph"].AsBool())
            {
                _settings["LeetMorph"] = false;
                _settings["WolfMorph"] = false;

                Chat.WriteLine("Only activate one Morph option.");
            }

            if (!_settings["DragonMorph"].AsBool())
            {
                CancelBuffs(RelevantNanos.DragonMorph);
            }
            if (!_settings["LeetMorph"].AsBool())
            {
                CancelBuffs(RelevantNanos.LeetMorph);
            }
            if (!_settings["SaberMorph"].AsBool())
            {
                CancelBuffs(RelevantNanos.SaberMorph);
            }
            if (!_settings["WolfMorph"].AsBool())
            {
                CancelBuffs(RelevantNanos.WolfMorph);
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                if (SettingsController.settingsWindow.FindView("HealingView", out Button healingView))
                {
                    healingView.Tag = SettingsController.settingsWindow;
                    healingView.Clicked = HandleHealingViewClick;
                }

                if (SettingsController.settingsWindow.FindView("MorphView", out Button morphView))
                {
                    morphView.Tag = SettingsController.settingsWindow;
                    morphView.Clicked = HandleMorphViewClick;
                }
            }
        }

        private bool ArmorBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.DragonMorph)) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        #region Morphs

        private bool DragonMorph(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("DragonMorph")) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }
        private bool LeetMorph(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("LeetMorph")) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }
        private bool WolfMorph(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("WolfMorph")) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }
        private bool SaberMorph(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("SaberMorph")) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool WolfAgility(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("WolfMorph")) { return false; }

            if (!DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.WolfMorph)) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }
        private bool SaberDamage(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("SaberMorph")) { return false; }

            if (!DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.SaberMorph)) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }
        private bool LeetCrit(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("LeetMorph")) { return false; }

            if (!DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.LeetMorph)) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }
        private bool DragonScales(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("DragonMorph")) { return false; }

            if (!DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.DragonMorph)) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Healing

        private bool TeamHealing(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanCast(spell) 
                || AdvHealPercentage == 0
                || HealSelection.SingleTeam != (HealSelection)_settings["HealSelection"].AsInt32()) { return false; }

            return FindMemberWithHealthBelow(AdvHealPercentage, ref actionTarget);
        }

        private bool Healing(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanCast(spell) || AdvHealPercentage == 0) { return false; }

            if (HealSelection.SingleTeam == (HealSelection)_settings["HealSelection"].AsInt32())
            {
                return FindMemberWithHealthBelow(AdvHealPercentage, ref actionTarget);
            }
            else if (HealSelection.SingleOS == (HealSelection)_settings["HealSelection"].AsInt32())
            {
                return FindPlayerWithHealthBelow(AdvHealPercentage, ref actionTarget);
            }

            return false;
        }

        private bool CompleteHealing(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("CH") || !CanCast(spell)
                || AdvCompleteHealPercentage == 0) { return false; }

            return FindMemberWithHealthBelow(AdvCompleteHealPercentage, ref actionTarget);
        }

        #endregion

        #region Misc

        public enum HealSelection
        {
            None, SingleTeam, SingleOS
        }
        private static class RelevantNanos
        {
            public static int[] HEALS = new[] { 223167, 252008, 252006, 136674, 136673, 143908, 82059, 136675, 136676, 82060, 136677,
                136678, 136679, 136682, 82061, 136681, 136680, 136683, 136684, 136685, 82062, 136686, 136689, 82063, 136688, 136687,
                82064, 26695 };

            public static readonly int[] ArmorBuffs = { 74173, 74174, 74175 , 74176, 74177, 74178 };
            public static readonly int[] DragonMorph = { 217670, 25994 };
            public static readonly int[] LeetMorph = { 263278, 82834 };
            public static readonly int[] WolfMorph = { 275005, 85062 };
            public static readonly int[] SaberMorph = { 217680, 85070 };
            public static readonly int[] DragonScales = { 302217, 302214 };
            public static readonly int[] WolfAgility = { 302235, 302232 };
            public static readonly int[] LeetCrit = { 302229, 302226 };
            public static readonly int[] SaberDamage = { 302243, 302240 };

        }

        private static class RelevantItems
        {

        }

        #endregion
    }
}
