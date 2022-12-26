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

namespace CombatHandler.Trader
{
    public class TraderCombatHandler : GenericCombatHandler
    {
        private static string PluginDirectory;

        private static int TraderHealPercentage;
        private static int TraderHealthDrainPercentage;

        private static Window _buffWindow;
        private static Window _debuffWindow;
        private static Window _healingWindow;
        private static Window _procWindow;

        private View _buffView;
        private View _debuffView;
        private View _healingView;
        private View _procView;

        private static SimpleChar _drainTarget;

        private static double _drainTick;
        private static double _ncuUpdateTime;

        private static bool _purpleReady = false;

        public TraderCombatHandler(string pluginDir) : base(pluginDir)
        {
            IPCChannel.RegisterCallback((int)IPCOpcode.RemainingNCU, OnRemainingNCUMessage);

            Config.CharSettings[Game.ClientInst].TraderHealPercentageChangedEvent += TraderHealPercentage_Changed;
            Config.CharSettings[Game.ClientInst].TraderHealthDrainPercentageChangedEvent += TraderHealthDrainPercentage_Changed;

            _settings.AddVariable("Buffing", true);
            _settings.AddVariable("Composites", true);

            _settings.AddVariable("DamageDrain", true);
            _settings.AddVariable("HealthDrain", false);

            _settings.AddVariable("EvadesSelection", (int)EvadesSelection.None);

            //LE Proc
            _settings.AddVariable("ProcType1Selection", (int)ProcType1Selection.DebtCollection);
            _settings.AddVariable("ProcType2Selection", (int)ProcType2Selection.UnopenedLetter);

            _settings.AddVariable("AAODrain", true);
            _settings.AddVariable("AADDrain", true);

            _settings.AddVariable("MyEnemy", true);

            _settings.AddVariable("ACDrains", true);

            _settings.AddVariable("GTH", true);

            _settings.AddVariable("NanoHealTeam", false);

            _settings.AddVariable("LegShot", false);
            _settings.AddVariable("PerkSelection", (int)PerkSelection.Sacrifice);
            _settings.AddVariable("HealSelection", (int)HealSelection.None);
            _settings.AddVariable("DepriveSelection", (int)DepriveSelection.Target);
            _settings.AddVariable("RansackSelection", (int)RansackSelection.Target);
            _settings.AddVariable("NanoDrainSelection", (int)NanoDrainSelection.None);
            _settings.AddVariable("ACDrainSelection", (int)ACDrainSelection.None);
            _settings.AddVariable("DamageDrainSelection", (int)DamageDrainSelection.None);
            _settings.AddVariable("AADDrainSelection", (int)AADDrainSelection.None);
            _settings.AddVariable("AAODrainSelection", (int)AAODrainSelection.None);

            RegisterSettingsWindow("Trader Handler", "TraderSettingsView.xml");

            //LE Proc
            RegisterPerkProcessor(PerkHash.LEProcTraderDebtCollection, DebtCollection, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderAccumulatedInterest, AccumulatedInterest, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderExchangeProduct, ExchangeProduct, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderUnforgivenDebts, UnforgivenDebts, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderUnexpectedBonus, UnexpectedBonus, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderRebate, Rebate, CombatActionPriority.Low);

            RegisterPerkProcessor(PerkHash.LEProcTraderUnopenedLetter, UnopenedLetter, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderRigidLiquidation, RigidLiquidation, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderDepleteAssets, DepleteAssets, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderEscrow, Escrow, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderRefinanceLoans, RefinanceLoans, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcTraderPaymentPlan, PaymentPlan, CombatActionPriority.Low);

            //Perks
            RegisterPerkProcessor(PerkHash.LegShot, LegShot);
            RegisterPerkProcessor(PerkHash.Sacrifice, Sacrifice);
            RegisterPerkProcessor(PerkHash.PurpleHeart, PurpleHeart);

            //Heals
            RegisterSpellProcessor(RelevantNanos.Heal, Healing);
            RegisterSpellProcessor(RelevantNanos.TeamHeal, Healing);
            RegisterSpellProcessor(RelevantNanos.HealthDrain, Healing);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DrainHeal).OrderByStackingOrder(), LEHeal);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDrain_LineA).OrderByStackingOrder(), RKNanoDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SLNanopointDrain).OrderByStackingOrder(), SLNanoDrain);

            //Buffs
            RegisterSpellProcessor(RelevantNanos.ImprovedQuantumUncertanity, ImprovedQuantumUncertanity);
            RegisterSpellProcessor(RelevantNanos.UnstoppableKiller, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.UmbralWranglerPremium, GenericBuff);

            //Team Buffs
            RegisterSpellProcessor(RelevantNanos.QuantumUncertanity, Evades);

            //Team Nano heal (Rouse Outfit nanoline)
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoPointHeals).OrderByStackingOrder(), NanoHeal);

            //Debuffs
            RegisterSpellProcessor(RelevantNanos.GrandThefts, GrandTheftHumidity, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.MyEnemiesEnemyIsMyFriend, MyEnemy);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderAADDrain).OrderByStackingOrder(), AADDrain, CombatActionPriority.Medium);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderAAODrain).OrderByStackingOrder(), AAODrain, CombatActionPriority.Medium);
            RegisterSpellProcessor(RelevantNanos.DivestDamage, DamageDrain, CombatActionPriority.Medium);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoResistanceDebuff_LineA).OrderByStackingOrder(), ACDrain, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderSkillTransferTargetDebuff_Deprive).OrderByStackingOrder(), DepriveDrain, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderSkillTransferTargetDebuff_Ransack).OrderByStackingOrder(), RansackDrain, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderDebuffACNanos).OrderByStackingOrder(), ACDrain, CombatActionPriority.Low);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderACTransferTargetDebuff_Draw).OrderByStackingOrder(), ACDrain, CombatActionPriority.Low);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderACTransferTargetDebuff_Siphon).OrderByStackingOrder(), ACDrain, CombatActionPriority.Low);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DebuffNanoACHeavy).OrderByStackingOrder(), ACDrain, CombatActionPriority.Low);

            PluginDirectory = pluginDir;

            TraderHealPercentage = Config.CharSettings[Game.ClientInst].TraderHealPercentage;
            TraderHealthDrainPercentage = Config.CharSettings[Game.ClientInst].TraderHealthDrainPercentage;
        }
        public Window[] _windows => new Window[] { _healingWindow, _buffWindow, _debuffWindow, _procWindow };

        #region Callbacks

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

        #endregion

        #region Handles

        private void HandleBuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_buffView)) { return; }

                _buffView = View.CreateFromXml(PluginDirectory + "\\UI\\TraderBuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Buffs", XmlViewName = "TraderBuffsView" }, _buffView);
            }
            else if (_buffWindow == null || (_buffWindow != null && !_buffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_buffWindow, PluginDir, new WindowOptions() { Name = "Buffs", XmlViewName = "TraderBuffsView" }, _buffView, out var container);
                _buffWindow = container;
            }
        }

        private void HandleDebuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_debuffView)) { return; }

                _debuffView = View.CreateFromXml(PluginDirectory + "\\UI\\TraderDebuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Debuffs", XmlViewName = "TraderDebuffsView" }, _debuffView);
            }
            else if (_debuffWindow == null || (_debuffWindow != null && !_debuffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_debuffWindow, PluginDir, new WindowOptions() { Name = "Debuffs", XmlViewName = "TraderDebuffsView" }, _debuffView, out var container);
                _debuffWindow = container;
            }
        }

        private void HandleHealingViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                //Cannot stop Multi-Tabs. Easy fix would be correct naming of views to reference against WindowOptions - options.Name
                _healingView = View.CreateFromXml(PluginDirectory + "\\UI\\TraderHealingView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Healing", XmlViewName = "TraderHealingView" }, _healingView);

                window.FindView("HealPercentageBox", out TextInputView healInput);
                window.FindView("HealthDrainPercentageBox", out TextInputView healthDrainInput);

                if (healInput != null)
                {
                    healInput.Text = $"{TraderHealPercentage}";
                }
                if (healthDrainInput != null)
                {
                    healthDrainInput.Text = $"{TraderHealthDrainPercentage}";
                }
            }
            else if (_healingWindow == null || (_healingWindow != null && !_healingWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_healingWindow, PluginDir, new WindowOptions() { Name = "Healing", XmlViewName = "TraderHealingView" }, _healingView, out var container);
                _healingWindow = container;

                container.FindView("HealPercentageBox", out TextInputView healInput);
                container.FindView("HealthDrainPercentageBox", out TextInputView healthDrainInput);

                if (healInput != null)
                {
                    healInput.Text = $"{TraderHealPercentage}";
                }
                if (healthDrainInput != null)
                {
                    healthDrainInput.Text = $"{TraderHealthDrainPercentage}";
                }
            }
        }
        private void HandleProcViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_procView)) { return; }

                _procView = View.CreateFromXml(PluginDirectory + "\\UI\\TraderProcsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Procs", XmlViewName = "TraderProcsView" }, _procView);
            }
            else if (_procWindow == null || (_procWindow != null && !_procWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_procWindow, PluginDir, new WindowOptions() { Name = "Procs", XmlViewName = "TraderProcsView" }, _procView, out var container);
                _procWindow = container;
            }
        }

        #endregion
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
                window.FindView("HealPercentageBox", out TextInputView healInput);
                window.FindView("HealthDrainPercentageBox", out TextInputView healthDrainInput);

                if (healInput != null && !string.IsNullOrEmpty(healInput.Text))
                    if (int.TryParse(healInput.Text, out int healValue))
                        if (Config.CharSettings[Game.ClientInst].TraderHealPercentage != healValue)
                            Config.CharSettings[Game.ClientInst].TraderHealPercentage = healValue;

                if (healthDrainInput != null && !string.IsNullOrEmpty(healthDrainInput.Text))
                    if (int.TryParse(healthDrainInput.Text, out int heallthDrainValue))
                        if (Config.CharSettings[Game.ClientInst].TraderHealthDrainPercentage != heallthDrainValue)
                            Config.CharSettings[Game.ClientInst].TraderHealthDrainPercentage = heallthDrainValue;
            }

            if ((RansackSelection.OS == (RansackSelection)_settings["RansackSelection"].AsInt32()
                || DepriveSelection.OS == (DepriveSelection)_settings["DepriveSelection"].AsInt32()
                || DamageDrainSelection.OS == (DamageDrainSelection)_settings["DamageDrainSelection"].AsInt32()
                || ACDrainSelection.OS == (ACDrainSelection)_settings["ACDrainSelection"].AsInt32()
                || AAODrainSelection.OS == (AAODrainSelection)_settings["AAODrainSelection"].AsInt32()
                || AADDrainSelection.OS == (AADDrainSelection)_settings["AADDrainSelection"].AsInt32())
                && Time.NormalTime > _drainTick + 1)
            {
                _drainTarget = DynelManager.NPCs
                    .Where(c => !debuffOSTargetsToIgnore.Contains(c.Name)
                        && c.IsInLineOfSight
                        && !c.Buffs.Contains(NanoLine.Mezz) && !c.Buffs.Contains(NanoLine.AOEMezz)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f)
                    .OrderBy(c => c.DistanceFrom(DynelManager.LocalPlayer))
                    .FirstOrDefault();

                _drainTick = Time.NormalTime;
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
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

                if (SettingsController.settingsWindow.FindView("ProcsView", out Button procView))
                {
                    procView.Tag = SettingsController.settingsWindow;
                    procView.Clicked = HandleProcViewClick;
                }
            }
        }

        #region LE Procs

        private bool AccumulatedInterest(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.AccumulatedInterest != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool DebtCollection(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.DebtCollection != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool ExchangeProduct(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.ExchangeProduct != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool Rebate(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.Rebate != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool UnexpectedBonus(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.UnexpectedBonus != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool UnforgivenDebts(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.UnforgivenDebts != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool DepleteAssets(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.DepleteAssets != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool Escrow(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.Escrow != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool RefinanceLoans(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.RefinanceLoans != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool RigidLiquidation(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.RigidLiquidation != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool UnopenedLetter(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.UnopenedLetter != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool PaymentPlan(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.PaymentPlan != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Perks

        private bool LegShot(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("LegShot")) { return false; }

            return CyclePerks(perk, fightingTarget, ref actionTarget);
        }

        private bool Sacrifice(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (PerkSelection.Sacrifice != (PerkSelection)_settings["PerkSelection"].AsInt32()) { return false; }

            if (fightingTarget == null) { return false; }

            return CyclePerks(perk, fightingTarget, ref actionTarget);
        }

        private bool PurpleHeart(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (PerkSelection.PurpleHeart != (PerkSelection)_settings["PerkSelection"].AsInt32()) { return false; }

            if (fightingTarget == null) { return false; }

            return CyclePerks(perk, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Healing

        private bool LEHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return FindMemberWithHealthBelow(60, spell, ref actionTarget);
        }

        private bool Healing(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (TraderHealPercentage == 0) { return false; }

            if (HealSelection.SingleTeam == (HealSelection)_settings["HealSelection"].AsInt32())
                return FindMemberWithHealthBelow(TraderHealPercentage, spell, ref actionTarget);

            if (HealSelection.SingleOS == (HealSelection)_settings["HealSelection"].AsInt32())
                return FindPlayerWithHealthBelow(TraderHealPercentage, spell, ref actionTarget);

            if (HealSelection.Team == (HealSelection)_settings["HealSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.IsInTeam())
                {
                    List<SimpleChar> dyingTeamMember = DynelManager.Characters
                        .Where(c => Team.Members
                            .Where(m => m.TeamIndex == Team.Members.FirstOrDefault(n => n.Identity == DynelManager.LocalPlayer.Identity).TeamIndex)
                                .Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                        .Where(c => c.HealthPercent <= 85 && c.HealthPercent >= 50)
                        .ToList();

                    if (dyingTeamMember.Count >= 4) { return false; }
                }

                return FindMemberWithHealthBelow(TraderHealPercentage, spell, ref actionTarget);
            }

            if (fightingTarget == null || TraderHealthDrainPercentage == 0 || !IsSettingEnabled("HealthDrain")) { return false; }

            if (DynelManager.LocalPlayer.HealthPercent <= TraderHealthDrainPercentage)
            {
                if (SpellChecksOther(spell, spell.Nanoline, fightingTarget))
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = fightingTarget;
                    return true;
                }
            }

            return false;
        }
        private bool NanoHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("NanoHealTeam")) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Buffs

        //protected bool UmbralWrangle(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        //{
        //    if (!IsSettingEnabled("UmbralWrangle")) { return false; }

        //    return Buff(spell, NanoLine.TraderTeamSkillWranglerBuff, fightingTarget, ref actionTarget);
        //}

        protected bool Evades(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || IsInsideInnerSanctum()) { return false; }

            if (EvadesSelection.Team == (EvadesSelection)_settings["EvadesSelection"].AsInt32())
                return GenericTeamBuff(spell, fightingTarget, ref actionTarget);

            if (EvadesSelection.None == (EvadesSelection)_settings["EvadesSelection"].AsInt32()) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }
        protected bool ImprovedQuantumUncertanity(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || IsInsideInnerSanctum()) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Debuffs

        private bool SLNanoDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (NanoDrainSelection.Shadowlands != (NanoDrainSelection)_settings["NanoDrainSelection"].AsInt32()
                || fightingTarget?.MaxHealth < 1000000) { return false; }

            return TargetDebuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool RKNanoDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (NanoDrainSelection.RubiKa != (NanoDrainSelection)_settings["NanoDrainSelection"].AsInt32()
                || fightingTarget?.MaxHealth < 1000000) { return false; }

            return TargetDebuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool MyEnemy(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget?.MaxHealth < 1000000) { return false; }

            return ToggledTargetDebuff("MyEnemy", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool GrandTheftHumidity(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget?.MaxHealth < 1000000) { return false; }

            return ToggledTargetDebuff("GTH", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool RansackDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (RansackSelection.Target == (RansackSelection)_settings["RansackSelection"].AsInt32())
                return TargetDebuff(spell, NanoLine.TraderSkillTransferTargetDebuff_Ransack, fightingTarget, ref actionTarget);

            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || _drainTarget == null) { return false; }

            if (RansackSelection.OS == (RansackSelection)_settings["RansackSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.TraderSkillTransferCasterBuff_Ransack, out Buff buff))
                {
                    if (spell.StackingOrder <= buff.StackingOrder)
                    {
                        if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU)) { return false; }

                        if (buff.RemainingTime > 40) { return false; }

                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = _drainTarget;
                        return true;
                    }

                    return false;
                }

                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU) { return false; }

                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = _drainTarget;
                return true;
            }

            return false;
        }

        private bool DepriveDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DepriveSelection.Target == (DepriveSelection)_settings["DepriveSelection"].AsInt32())
                return TargetDebuff(spell, NanoLine.TraderSkillTransferTargetDebuff_Deprive, fightingTarget, ref actionTarget);

            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || _drainTarget == null) { return false; }

            if (DepriveSelection.OS == (DepriveSelection)_settings["DepriveSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.TraderSkillTransferCasterBuff_Deprive, out Buff buff))
                {
                    if (spell.StackingOrder <= buff.StackingOrder)
                    {
                        if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU)) { return false; }

                        if (buff.RemainingTime > 40) { return false; }

                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = _drainTarget;
                        return true;
                    }

                    return false;
                }

                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU) { return false; }

                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = _drainTarget;
                return true;
            }

            return false;
        }

        private bool DamageDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DamageDrainSelection.Target == (DamageDrainSelection)_settings["DamageDrainSelection"].AsInt32())
                return TargetDebuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);

            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || _drainTarget == null) { return false; }

            if (DamageDrainSelection.OS == (DamageDrainSelection)_settings["DamageDrainSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Buffs.Find(spell.Nanoline, out Buff buff))
                {
                    if (spell.StackingOrder <= buff.StackingOrder)
                    {
                        if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU)) { return false; }

                        if (buff.RemainingTime > 40) { return false; }

                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = _drainTarget;
                        return true;
                    }

                    return false;
                }

                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU) { return false; }

                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = _drainTarget;
                return true;
            }

            return false;
        }

        private bool AAODrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AAODrainSelection.Target == (AAODrainSelection)_settings["AAODrainSelection"].AsInt32())
                return TargetDebuff(spell, NanoLine.TraderNanoTheft1, fightingTarget, ref actionTarget);

            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || _drainTarget == null) { return false; }

            if (AAODrainSelection.OS == (AAODrainSelection)_settings["AAODrainSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.TraderNanoTheft1, out Buff buff))
                {
                    if (spell.StackingOrder <= buff.StackingOrder)
                    {
                        if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU)) { return false; }

                        if (buff.RemainingTime > 40) { return false; }

                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = _drainTarget;
                        return true;
                    }

                    return false;
                }

                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU) { return false; }

                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = _drainTarget;
                return true;
            }

            return false;
        }

        private bool AADDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AADDrainSelection.Target == (AADDrainSelection)_settings["AADDrainSelection"].AsInt32())
                return TargetDebuff(spell, NanoLine.TraderNanoTheft2, fightingTarget, ref actionTarget);

            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || _drainTarget == null) { return false; }

            if (AADDrainSelection.OS == (AADDrainSelection)_settings["AADDrainSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.TraderNanoTheft2, out Buff buff))
                {
                    if (spell.StackingOrder <= buff.StackingOrder)
                    {
                        if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU)) { return false; }

                        if (buff.RemainingTime > 40) { return false; }

                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = _drainTarget;
                        return true;
                    }

                    return false;
                }

                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU) { return false; }

                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = _drainTarget;
                return true;
            }

            return false;
        }

        private bool ACDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ACDrainSelection.Target == (ACDrainSelection)_settings["ACDrainSelection"].AsInt32())
                return TargetDebuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);

            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || _drainTarget == null) { return false; }

            if (ACDrainSelection.OS == (ACDrainSelection)_settings["ACDrainSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Buffs.Find(spell.Nanoline, out Buff buff))
                {
                    if (spell.StackingOrder <= buff.StackingOrder)
                    {
                        if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU)) { return false; }

                        if (buff.RemainingTime > 40) { return false; }

                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = _drainTarget;
                        return true;
                    }

                    return false;
                }

                if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU) { return false; }

                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = _drainTarget;
                return true;
            }

            return false;
        }

        #endregion

        #region Misc

        private static class RelevantNanos
        {
            public const int QuantumUncertanity = 30745;
            public const int ImprovedQuantumUncertanity = 270808;
            public const int UnstoppableKiller = 275846;
            public const int DivestDamage = 273407;
            public const int UmbralWranglerPremium = 235291;
            public const int MyEnemiesEnemyIsMyFriend = 270714;
            public static int[] GrandThefts = { 269842, 280050 };
            public static int[] HealthDrain = { 270357, 77195, 76478, 76475, 76487, 76481,
                76484, 76491, 76494, 76499, 76571, 76503, 76651, 76614, 76656,
                76653, 76679, 76681, 76684, 76686, 76691, 76688, 76717, 76715,
                76720, 76722, 76724, 76727, 76729, 76732, 76742};
            public static int[] Heal = { 273410, 252155, 121496, 121500, 121501, 121499,
                121502, 121495, 121492, 121506, 121494, 121493, 121504, 121498, 121503,
                76653, 76679, 76681, 76684, 76686, 76691, 76688, 76717, 76715,
                121497, 121505};
            public static int[] TeamHeal = { 118245, 118230, 118232, 118231, 118235, 118233,
                118234, 118238, 118236, 118237, 118241, 118239, 118240, 118243, 118244,
                118242, 43374};
        }

        public enum PerkSelection
        {
            Sacrifice, PurpleHeart
        }
        public enum HealSelection
        {
            None, SingleTeam, SingleOS, Team
        }
        public enum NanoDrainSelection
        {
            None, RubiKa, Shadowlands
        }
        public enum AADDrainSelection
        {
            None, Target, OS
        }
        public enum AAODrainSelection
        {
            None, Target, OS
        }
        public enum ACDrainSelection
        {
            None, Target, OS
        }
        public enum DamageDrainSelection
        {
            None, Target, OS
        }
        public enum RansackSelection
        {
            None, Target, OS
        }
        public enum DepriveSelection
        {
            None, Target, OS
        }

        public enum ProcType1Selection
        {
            DebtCollection, AccumulatedInterest, ExchangeProduct, UnforgivenDebts, UnexpectedBonus, Rebate
        }

        public enum EvadesSelection
        {
            None, Self, Team
        }

        public enum ProcType2Selection
        {
            UnopenedLetter, RigidLiquidation, DepleteAssets, Escrow, RefinanceLoans, PaymentPlan
        }
        public static void TraderHealPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].TraderHealPercentage = e;
            TraderHealPercentage = e;
            Config.Save();
        }
        public static void TraderHealthDrainPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].TraderHealthDrainPercentage = e;
            TraderHealthDrainPercentage = e;
            Config.Save();
        }

        #endregion
    }
}
