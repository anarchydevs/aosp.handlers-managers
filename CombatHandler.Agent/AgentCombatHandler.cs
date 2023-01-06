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
using System.Security.Cryptography;

namespace CombatHandler.Agent
{
    public class AgentCombatHandler : GenericCombatHandler
    {
        private static string PluginDirectory;

        private static bool ToggleBuffing = false;
        private static bool ToggleComposites = false;
        private static bool ToggleDebuffing = false;

        private double _lastSwitchedHealTime = 0;

        private static Window _buffWindow;
        private static Window _debuffWindow;
        private static Window _procWindow;
        private static Window _itemWindow;
        private static Window _falseProfWindow;
        private static Window _healingWindow;
        private static Window _perkWindow;

        private static View _buffView;
        private static View _debuffView;
        private static View _procView;
        private static View _itemView;
        private static View _falseProfView;
        private static View _healingView;
        private static View _perkView;

        private static double _ncuUpdateTime;

        public AgentCombatHandler(string pluginDir) : base(pluginDir)
        {
            IPCChannel.RegisterCallback((int)IPCOpcode.RemainingNCU, OnRemainingNCUMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalBuffing, OnGlobalBuffingMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalComposites, OnGlobalCompositesMessage);
            //IPCChannel.RegisterCallback((int)IPCOpcode.GlobalDebuffing, OnGlobalDebuffingMessage);

            Config.CharSettings[Game.ClientInst].HealPercentageChangedEvent += HealPercentage_Changed;
            Config.CharSettings[Game.ClientInst].CompleteHealPercentageChangedEvent += CompleteHealPercentage_Changed;

            _settings.AddVariable("Buffing", true);
            _settings.AddVariable("Composites", true);

            _settings.AddVariable("GlobalBuffing", true);
            _settings.AddVariable("GlobalComposites", true);
            //_settings.AddVariable("GlobalDebuffs", true);

            _settings.AddVariable("Kits", true);
            _settings.AddVariable("Stims", true);

            _settings.AddVariable("DOTA", false);
            _settings.AddVariable("EvasionDebuff", false);

            _settings.AddVariable("CritTeam", false);

            _settings.AddVariable("InitDebuffSelection", (int)InitDebuffSelection.None);

            _settings.AddVariable("ProcType1Selection", (int)ProcType1Selection.GrimReaper);
            _settings.AddVariable("ProcType2Selection", (int)ProcType2Selection.LaserAim);

            _settings.AddVariable("HealSelection", (int)HealSelection.None);

            _settings.AddVariable("CH", false);

            _settings.AddVariable("ProcSelection", (int)ProcSelection.Damage);

            _settings.AddVariable("Concentration", false);

            _settings.AddVariable("FalseProfSelection", (int)FalseProfSelection.None);

            RegisterSettingsWindow("Agent Handler", "AgentSettingsView.xml");

            //LE Procs
            RegisterPerkProcessor(PerkHash.LEProcAgentGrimReaper, GrimReaper, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentDisableCuffs, DisableCuffs, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentNoEscape, NoEscape, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentIntenseMetabolism, IntenseMetabolism, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentMinorNanobotEnhance, MinorNanobotEnhance, CombatActionPriority.Low);

            RegisterPerkProcessor(PerkHash.LEProcAgentNotumChargedRounds, NotumChargedRounds, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentLaserAim, LaserAim, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentNanoEnhancedTargeting, NanoEnhancedTargeting, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentPlasteelPiercingRounds, PlasteelPiercingRounds, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentCellKiller, CellKiller, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentImprovedFocus, ImprovedFocus, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcAgentBrokenAnkle, BrokenAnkle, CombatActionPriority.Low);

            //Buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AgilityBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AimedShotBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ConcealmentBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.CriticalIncreaseBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ExecutionerBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.RifleBuffs).OrderByStackingOrder(), Rifle);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AgentProcBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ConcentrationCriticalLine).OrderByStackingOrder(), Concentration, CombatActionPriority.Medium);
            RegisterSpellProcessor(RelevantNanos.DetauntProcs, DetauntProc);
            RegisterSpellProcessor(RelevantNanos.DOTProcs, DamageProc);

            RegisterSpellProcessor(RelevantNanos.Healing, Healing, CombatActionPriority.Medium);
            RegisterSpellProcessor(RelevantNanos.CompleteHealing, CompleteHealing, CombatActionPriority.High);

            //False Profs
            RegisterSpellProcessor(RelevantNanos.FalseProfDoc, FalseProfDoctor);
            RegisterSpellProcessor(RelevantNanos.FalseProfAdv, FalseProfAdventurer);
            RegisterSpellProcessor(RelevantNanos.FalseProfCrat, FalseProfBeauracrat);
            RegisterSpellProcessor(RelevantNanos.FalseProfEnf, FalseProfEnforcer);
            RegisterSpellProcessor(RelevantNanos.FalseProfEng, FalseProfEngineer);
            RegisterSpellProcessor(RelevantNanos.FalseProfFixer, FalseProfFixer);
            RegisterSpellProcessor(RelevantNanos.FalseProfMa, FalseProfMartialArtist);
            RegisterSpellProcessor(RelevantNanos.FalseProfMp, FalseProfMetaphysicist);
            RegisterSpellProcessor(RelevantNanos.FalseProfNt, FalseProfNanoTechnician);
            RegisterSpellProcessor(RelevantNanos.FalseProfSol, FalseProfSoldier);
            RegisterSpellProcessor(RelevantNanos.FalseProfTrader, FalseProfTrader);

            //Team Buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DamageBuffs_LineA).OrderByStackingOrder(), GenericTeamBuff);
            RegisterSpellProcessor(RelevantNanos.TeamCritBuffs, CritIncrease);

            //Debuffs
            RegisterSpellProcessor(RelevantNanos.InitDebuffs, InitDebuffs, CombatActionPriority.Medium);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DOTAgentStrainA).OrderByStackingOrder(), DOTA, CombatActionPriority.Low);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.EvasionDebuffs_Agent), EvasionDebuff, CombatActionPriority.Low);

            //False prof

            //Sol
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ReflectShield).Where(c => c.Name.Contains("Mirror")).OrderByStackingOrder(), AMS);

            PluginDirectory = pluginDir;

            HealPercentage = Config.CharSettings[Game.ClientInst].HealPercentage;
            CompleteHealPercentage = Config.CharSettings[Game.ClientInst].CompleteHealPercentage;
        }

        public Window[] _windows => new Window[] { _buffWindow, _debuffWindow, _healingWindow, _procWindow, _itemWindow, _perkWindow };

        #region Callbacks

        public static void OnRemainingNCUMessage(int sender, IPCMessage msg)
        {
            if (Game.IsZoning)
                return;

            RemainingNCUMessage ncuMessage = (RemainingNCUMessage)msg;
            SettingsController.RemainingNCU[ncuMessage.Character] = ncuMessage.RemainingNCU;
        }
        private void OnGlobalBuffingMessage(int sender, IPCMessage msg)
        {
            GlobalBuffingMessage buffMsg = (GlobalBuffingMessage)msg;

            if (DynelManager.LocalPlayer.Identity.Instance == sender) { return; }

            _settings[$"Buffing"] = buffMsg.Switch;
            _settings[$"GlobalBuffing"] = buffMsg.Switch;
        }
        private void OnGlobalCompositesMessage(int sender, IPCMessage msg)
        {
            GlobalCompositesMessage compMsg = (GlobalCompositesMessage)msg;

            if (DynelManager.LocalPlayer.Identity.Instance == sender) { return; }

            _settings[$"Composites"] = compMsg.Switch;
            _settings[$"GlobalComposites"] = compMsg.Switch;
        }

        //private void OnGlobalDebuffingMessage(int sender, IPCMessage msg)
        //{
        //    GlobalDebuffingMessage debuffMsg = (GlobalDebuffingMessage)msg;

        //    _settings[$"Debuffing"] = debuffMsg.Switch;
        //    _settings[$"Debuffing"] = debuffMsg.Switch;
        //}

        #endregion

        #region Handles
        private void HandleItemViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                if (window.Views.Contains(_itemView)) { return; }

                _itemView = View.CreateFromXml(PluginDirectory + "\\UI\\AgentItemsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Items", XmlViewName = "AgentItemsView" }, _itemView);
            }
            else if (_itemWindow == null || (_itemWindow != null && !_itemWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_itemWindow, PluginDir, new WindowOptions() { Name = "Items", XmlViewName = "AgentItemsView" }, _itemView, out var container);
                _itemWindow = container;
            }
        }

        private void HandlePerkViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                if (window.Views.Contains(_perkView)) { return; }

                _perkView = View.CreateFromXml(PluginDirectory + "\\UI\\AgentPerksView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Perks", XmlViewName = "AgentPerksView" }, _perkView);
            }
            else if (_perkWindow == null || (_perkWindow != null && !_perkWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_perkWindow, PluginDir, new WindowOptions() { Name = "Perks", XmlViewName = "AgentPerksView" }, _perkView, out var container);
                _perkWindow = container;
            }
        }

        private void HandleProcViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                if (window.Views.Contains(_procView)) { return; }

                _procView = View.CreateFromXml(PluginDirectory + "\\UI\\AgentProcsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Procs", XmlViewName = "AgentProcsView" }, _procView);
            }
            else if (_procWindow == null || (_procWindow != null && !_procWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_procWindow, PluginDir, new WindowOptions() { Name = "Procs", XmlViewName = "AgentProcsView" }, _procView, out var container);
                _procWindow = container;
            }
        }

        private void HandleFalseProfViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                if (window.Views.Contains(_falseProfView)) { return; }

                _falseProfView = View.CreateFromXml(PluginDirectory + "\\UI\\AgentFalseProfsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "False Professions", XmlViewName = "AgentFalseProfsView" }, _falseProfView);
            }
            else if (_falseProfWindow == null || (_falseProfWindow != null && !_falseProfWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_falseProfWindow, PluginDir, new WindowOptions() { Name = "False Professions", XmlViewName = "AgentFalseProfsView" }, _falseProfView, out var container);
                _falseProfWindow = container;
            }
        }

        private void HandleBuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                if (window.Views.Contains(_buffView)) { return; }

                _buffView = View.CreateFromXml(PluginDirectory + "\\UI\\AgentBuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Buffs", XmlViewName = "AgentBuffsView" }, _buffView);
            }
            else if (_buffWindow == null || (_buffWindow != null && !_buffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_buffWindow, PluginDir, new WindowOptions() { Name = "Buffs", XmlViewName = "AgentBuffsView" }, _buffView, out var container);
                _buffWindow = container;
            }
        }

        private void HandleDebuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                _debuffView = View.CreateFromXml(PluginDirectory + "\\UI\\AgentDebuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Debuffs", XmlViewName = "AgentDebuffsView" }, _debuffView);
            }
            else if (_debuffWindow == null || (_debuffWindow != null && !_debuffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_debuffWindow, PluginDir, new WindowOptions() { Name = "Debuffs", XmlViewName = "AgentDebuffsView" }, _debuffView, out var container);
                _debuffWindow = container;
            }
        }

        private void HandleHealingViewClick(object s, ButtonBase button)
        {

            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                if (window.Views.Contains(_healingView)) { return; }

                _healingView = View.CreateFromXml(PluginDirectory + "\\UI\\AgentHealingView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Healing", XmlViewName = "AgentHealingView" }, _healingView);

                window.FindView("HealPercentageBox", out TextInputView healInput);
                window.FindView("CompleteHealPercentageBox", out TextInputView completeHealInput);

                if (healInput != null)
                    healInput.Text = $"{HealPercentage}";

                if (completeHealInput != null)
                    completeHealInput.Text = $"{CompleteHealPercentage}";
            }
            else if (_healingWindow == null || (_healingWindow != null && !_healingWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_healingWindow, PluginDir, new WindowOptions() { Name = "Healing", XmlViewName = "AgentHealingView" }, _healingView, out var container);
                _healingWindow = container;

                container.FindView("HealPercentageBox", out TextInputView healInput);
                container.FindView("CompleteHealPercentageBox", out TextInputView completeHealInput);

                if (healInput != null)
                    healInput.Text = $"{HealPercentage}";

                if (completeHealInput != null)
                    completeHealInput.Text = $"{CompleteHealPercentage}";
            }
        }

        #endregion

        protected override void OnUpdate(float deltaTime)
        {
            if (Game.IsZoning)
                return;

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
                window.FindView("HealPercentageBox", out TextInputView healInput);
                window.FindView("CompleteHealPercentageBox", out TextInputView completeHealInput);

                if (healInput != null && !string.IsNullOrEmpty(healInput.Text))
                    if (int.TryParse(healInput.Text, out int healValue))
                        if (Config.CharSettings[Game.ClientInst].HealPercentage != healValue)
                            Config.CharSettings[Game.ClientInst].HealPercentage = healValue;

                if (completeHealInput != null && !string.IsNullOrEmpty(completeHealInput.Text))
                    if (int.TryParse(completeHealInput.Text, out int completeHealValue))
                        if (Config.CharSettings[Game.ClientInst].CompleteHealPercentage != completeHealValue)
                            Config.CharSettings[Game.ClientInst].CompleteHealPercentage = completeHealValue;
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                if (SettingsController.settingsWindow.FindView("ItemsView", out Button itemView))
                {
                    itemView.Tag = SettingsController.settingsWindow;
                    itemView.Clicked = HandleItemViewClick;
                }

                if (SettingsController.settingsWindow.FindView("PerksView", out Button perkView))
                {
                    perkView.Tag = SettingsController.settingsWindow;
                    perkView.Clicked = HandlePerkViewClick;
                }

                if (SettingsController.settingsWindow.FindView("HealingView", out Button healingView))
                {
                    healingView.Tag = SettingsController.settingsWindow;
                    healingView.Clicked = HandleHealingViewClick;
                }

                if (SettingsController.settingsWindow.FindView("BuffsView", out Button buffView))
                {
                    buffView.Tag = SettingsController.settingsWindow;
                    buffView.Clicked = HandleBuffViewClick;
                }

                if (SettingsController.settingsWindow.FindView("DebuffsView", out Button debuffView))
                {
                    debuffView.Tag = SettingsController.settingsWindow;
                    debuffView.Clicked = HandleDebuffViewClick;
                }

                if (SettingsController.settingsWindow.FindView("FalseProfsView", out Button falseProfView))
                {
                    falseProfView.Tag = SettingsController.settingsWindow;
                    falseProfView.Clicked = HandleFalseProfViewClick;
                }

                if (SettingsController.settingsWindow.FindView("ProcView", out Button procView))
                {
                    procView.Tag = SettingsController.settingsWindow;
                    procView.Clicked = HandleProcViewClick;
                }


                #region GlobalBuffing

                if (!_settings["GlobalBuffing"].AsBool() && ToggleBuffing)
                {
                    IPCChannel.Broadcast(new GlobalBuffingMessage()
                    {
                        Switch = false
                    });

                    ToggleBuffing = false;
                    _settings["Buffing"] = false;
                    _settings["GlobalBuffing"] = false;
                }

                if (_settings["GlobalBuffing"].AsBool() && !ToggleBuffing)
                {
                    IPCChannel.Broadcast(new GlobalBuffingMessage()
                    {
                        Switch = true
                    });

                    ToggleBuffing = true;
                    _settings["Buffing"] = true;
                    _settings["GlobalBuffing"] = true;
                }

                #endregion

                #region Global Composites

                if (!_settings["GlobalComposites"].AsBool() && ToggleComposites)
                {
                    IPCChannel.Broadcast(new GlobalCompositesMessage()
                    {
                        Switch = false
                    });

                    ToggleComposites = false;
                    _settings["Composites"] = false;
                    _settings["GlobalComposites"] = false;
                }
                if (_settings["GlobalComposites"].AsBool() && !ToggleComposites)
                {
                    IPCChannel.Broadcast(new GlobalCompositesMessage()
                    {
                        Switch = true
                    });

                    ToggleComposites = true;
                    _settings["Composites"] = true;
                    _settings["GlobalComposites"] = true;
                }

                #endregion

                #region Global Debuffing

                //if (!_settings["GlobalDebuffing"].AsBool() && ToggleDebuffing)
                //{
                //    IPCChannel.Broadcast(new GlobalDebuffingMessage()
                //    {

                //        Switch = false
                //    });

                //    ToggleDebuffing = false;
                //    _settings["GlobalDebuffing"] = false;
                //}
                //if (_settings["GlobalDebuffing"].AsBool() && !ToggleDebuffing)
                //{
                //    IPCChannel.Broadcast(new GlobalDebuffingMessage()
                //    {
                //        Switch = true
                //    });

                //    ToggleDebuffing = true;
                //    _settings["GlobalDebuffing"] = true;
                //}

                #endregion
            }

            if (CanLookupPetsAfterZone() && DynelManager.LocalPlayer.Pets.Count() >= 1)
            {
                SynchronizePetCombatStateWithOwner();
                AssignTargetToHealPet();
            }

            if (ProcSelection.Damage == (ProcSelection)_settings["ProcSelection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.DetauntProcs);
            }
            if (ProcSelection.DeTaunt == (ProcSelection)_settings["ProcSelection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.DOTProcs);
            }
        }

        #region LE Procs

        private bool DisableCuffs(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.DisableCuffs != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool GrimReaper(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.GrimReaper != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool IntenseMetabolism(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.IntenseMetabolism != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool MinorNanobotEnhance(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.MinorNanobotEnhance != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool NoEscape(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.NoEscape != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool BrokenAnkle(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.BrokenAnkle != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool CellKiller(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.CellKiller != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool ImprovedFocus(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.ImprovedFocus != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool LaserAim(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.LaserAim != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool NanoEnhancedTargeting(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.NanoEnhancedTargeting != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool NotumChargedRounds(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.NotumChargedRounds != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool PlasteelPiercingRounds(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.PlasteelPiercingRounds != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Healing

        private bool Healing(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || HealPercentage == 0) { return false; }

            if (HealSelection.SingleTeam == (HealSelection)_settings["HealSelection"].AsInt32())
                return FindMemberWithHealthBelow(HealPercentage, spell, ref actionTarget);

            if (HealSelection.SingleArea == (HealSelection)_settings["HealSelection"].AsInt32())
                return FindPlayerWithHealthBelow(HealPercentage, spell, ref actionTarget);

            return false;
        }

        private bool CompleteHealing(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing") || !IsSettingEnabled("CH") || CompleteHealPercentage == 0) { return false; }

            return FindMemberWithHealthBelow(CompleteHealPercentage, spell, ref actionTarget);
        }

        #endregion

        #region Buffs

        #region False Profs

        private bool FalseProfEnforcer(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Enforcer != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfEngineer(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Engineer != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfNanoTechnician(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.NanoTechnician != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfTrader(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Trader != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfBeauracrat(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Beauracrat != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfMartialArtist(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.MartialArtist != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfFixer(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Fixer != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfAdventurer(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Adventurer != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfDoctor(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Doctor != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool FalseProfSoldier(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Soldier != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }
        private bool FalseProfMetaphysicist(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (FalseProfSelection.Metaphysicist != (FalseProfSelection)_settings["FalseProfSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }


        #endregion


        private bool Concentration(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return ToggledTargetDebuff("Concentration", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool Rifle(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Buffs.Find(RelevantNanos.AssassinsAimedShot, out Buff AAS)) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Find(RelevantNanos.SteadyNerves, out Buff SN)) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool CritIncrease(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsSettingEnabled("CritTeam"))
                return GenericTeamBuff(spell, fightingTarget, ref actionTarget);

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool DetauntProc(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcSelection.DeTaunt != (ProcSelection)_settings["ProcSelection"].AsInt32()) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool DamageProc(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcSelection.Damage != (ProcSelection)_settings["ProcSelection"].AsInt32()) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Debuffs

        private bool InitDebuffs(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (InitDebuffSelection.Area == (InitDebuffSelection)_settings["InitDebuffSelection"].AsInt32())
                return AreaDebuff(spell, ref actionTarget);

            if (InitDebuffSelection.Target == (InitDebuffSelection)_settings["InitDebuffSelection"].AsInt32()
                && fightingTarget != null)
            {
                if (debuffTargetsToIgnore.Contains(fightingTarget.Name)) { return false; }

                return TargetDebuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
            }

            return false;
        }

        private bool DOTA(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return ToggledTargetDebuff("DOTA", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool EvasionDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return ToggledTargetDebuff("EvasionDebuff", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        #endregion

        #region False Profs
        private bool AMS(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!DynelManager.LocalPlayer.Buffs.Any(c => RelevantNanos.FalseProfSol.Contains(c.Id)) || !IsSettingEnabled("Buffing")) { return false; }

            if (fightingtarget == null || !CanCast(spell)) { return false; }

            if (DynelManager.LocalPlayer.HealthPercent <= 85) { return true; }

            return false;
        }

        #endregion

        #region Misc

        private void AssignTargetToHealPet()
        {
            if (Time.NormalTime - _lastSwitchedHealTime > 5)
            {
                SimpleChar dyingTarget = GetTargetToHeal();
                if (dyingTarget != null)
                {
                    Pet healPet = DynelManager.LocalPlayer.Pets.Where(pet => pet.Type == PetType.Heal).FirstOrDefault();
                    if (healPet != null)
                    {
                        healPet.Heal(dyingTarget.Identity);
                        _lastSwitchedHealTime = Time.NormalTime;
                    }
                }
            }
        }

        private SimpleChar GetTargetToHeal()
        {
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Players
                    .Where(c => c.IsAlive && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent < 90)
                    .OrderBy(c => c)
                    .ThenByDescending(c => c.HealthPercent)
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                    return dyingTeamMember;
            }

            if (DynelManager.LocalPlayer.HealthPercent < 90)
                return DynelManager.LocalPlayer;

            Pet dyingPet = DynelManager.LocalPlayer.Pets
                 .Where(pet => pet.Character.HealthPercent < 90
                    && (pet.Type == PetType.Attack || pet.Type == PetType.Social))
                 .OrderByDescending(pet => pet.Character.HealthPercent)
                 .FirstOrDefault();

            if (dyingPet != null)
                return dyingPet.Character;

            return null;
        }

        private static class RelevantNanos
        {
            public static int[] DetauntProcs = { 226437, 226435, 226433, 226431, 226429, 226427 };
            public static int[] FalseProfDoc = { 117210, 117221, 32033 };
            public static int[] FalseProfEng = { 117213, 117224, 32034 };
            public static int[] FalseProfSol = { 117216, 117227, 32038 };
            public static int[] FalseProfCrat = { 117209, 117220, 32032 };
            public static int[] FalseProfTrader = { 117211, 117222, 32040 };
            public static int[] FalseProfAdv = { 117214, 117225, 32030 };
            public static int[] FalseProfMp = { 117210, 117221, 32033 };
            public static int[] FalseProfFixer = { 117212, 117223, 32039 };
            public static int[] FalseProfEnf = { 117217, 117228, 32041 };
            public static int[] FalseProfMa = { 117215, 117226, 32035 };
            public static int[] FalseProfNt = { 117207, 117218, 32037 };
            public static int[] DOTProcs = { 226425, 226423, 226421, 226419, 226417, 226415, 226413, 226410 };
            public static int[] TeamCritBuffs = { 160791, 160789, 160787 };
            public static int AssassinsAimedShot = 275007;
            public static int SteadyNerves = 160795;
            public static int CompleteHealing = 28650;
            public static int TeamCH = 42409; //Add logic later
            public const int TiredLimbs = 99578;
            public static readonly Spell[] InitDebuffs = Spell.GetSpellsForNanoline(NanoLine.InitiativeDebuffs).OrderByStackingOrder().Where(spell => spell.Id != TiredLimbs).ToArray();
            public static int[] Healing = new[] { 223299, 223297, 223295, 223293, 223291, 223289, 223287, 223285, 223281, 43878, 43881, 43886, 43885,
                43887, 43890, 43884, 43808, 43888, 43889, 43883, 43811, 43809, 43810, 28645, 43816, 43817, 43825, 43815,
                43814, 43821, 43820, 28648, 43812, 43824, 43822, 43819, 43818, 43823, 28677, 43813, 43826, 43838, 43835,
                28672, 43836, 28676, 43827, 43834, 28681, 43837, 43833, 43830, 43828, 28654, 43831, 43829, 43832, 28665 };
        }
        public enum FalseProfSelection
        {
            None, Metaphysicist, Soldier, Enforcer, Engineer, Doctor, Fixer, Beauracrat, MartialArtist, NanoTechnician, Trader, Adventurer
        }
        public enum HealSelection
        {
            None, SingleTeam, SingleArea
        }
        public enum InitDebuffSelection
        {
            None, Target, Area
        }
        public enum ProcType1Selection
        {
            GrimReaper, DisableCuffs, NoEscape, IntenseMetabolism, MinorNanobotEnhance
        }

        public enum ProcType2Selection
        {
            NotumChargedRounds, LaserAim, NanoEnhancedTargeting, PlasteelPiercingRounds, CellKiller, ImprovedFocus, BrokenAnkle
        }
        public enum ProcSelection
        {
            None, Damage, DeTaunt
        }

        #endregion
    }
}
