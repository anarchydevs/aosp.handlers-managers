﻿using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using CombatHandler.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CombatHandler.Bureaucrat
{
    public class CratCombatHandler : GenericCombatHandler
    {
        private static string PluginDirectory;

        private double _lastTrimTime = 0;
        private const float DelayBetweenTrims = 1;
        private const float DelayBetweenDiverTrims = 305;

        private bool attackPetTrimmedAggressive = false;

        private static Window _buffWindow;
        private static Window _debuffWindow;
        private static Window _petWindow;
        private static Window _calmingWindow;
        private static Window _procWindow;

        private static View _buffView;
        private static View _debuffView;
        private static View _calmView;
        private static View _petView;
        private static View _procView;

        private Dictionary<PetType, bool> petTrimmedAggDef = new Dictionary<PetType, bool>();
        private Dictionary<PetType, bool> petTrimmedOffDiv = new Dictionary<PetType, bool>();

        private Dictionary<PetType, double> _lastPetTrimDivertOffTime = new Dictionary<PetType, double>()
        {
            { PetType.Attack, 0 },
            { PetType.Support, 0 }
        };

        private static double _ncuUpdateTime;

        public CratCombatHandler(string pluginDir) : base(pluginDir)
        {
            IPCChannel.RegisterCallback((int)IPCOpcode.RemainingNCU, OnRemainingNCUMessage);

            Game.TeleportEnded += OnZoned;

            _settings.AddVariable("Buffing", true);
            _settings.AddVariable("Composites", true);

            _settings.AddVariable("BuffingAuraSelection", (int)BuffingAuraSelection.AAOAAD);
            _settings.AddVariable("DebuffingAuraSelection", (int)DebuffingAuraSelection.None);

            _settings.AddVariable("CalmingSelection", (int)CalmingSelection.SL);
            _settings.AddVariable("ModeSelection", (int)ModeSelection.None);

            _settings.AddVariable("ProcType1Selection", (int)ProcType1Selection.FormsinTriplicate);
            _settings.AddVariable("ProcType2Selection", (int)ProcType2Selection.WrongWindow);

            _settings.AddVariable("NanoDeltaTeam", false);

            _settings.AddVariable("SyncPets", true);
            _settings.AddVariable("SpawnPets", true);
            _settings.AddVariable("BuffPets", true);
            _settings.AddVariable("WarpPets", true);

            _settings.AddVariable("MastersBidding", false);

            _settings.AddVariable("InitDebuffSelection", (int)InitDebuffSelection.None);

            _settings.AddVariable("DivertTrimmer", false);
            _settings.AddVariable("TauntTrimmer", false);
            _settings.AddVariable("AggDefTrimmer", false);

            _settings.AddVariable("Nukes", false);
            _settings.AddVariable("AOERoot", false);

            _settings.AddVariable("Calm12Man", false);
            //_settings.AddVariable("CalmSector7", false);

            RegisterSettingsWindow("Bureaucrat Handler", "BureaucratSettingsView.xml");

            //LE Procs
            RegisterPerkProcessor(PerkHash.LEProcBureaucratPleaseHold, PleaseHold, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratFormsInTriplicate, FormsinTriplicate, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratSocialServices, SocialServices, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratNextWindowOver, NextWindowOver, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratWaitInThatQueue, WaitInThatQueue, CombatActionPriority.Low);

            RegisterPerkProcessor(PerkHash.LEProcBureaucratMobilityEmbargo, MobilityEmbargo, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratWrongWindow, WrongWindow, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratTaxAudit, TaxAudit, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratLostPaperwork, LostPaperwork, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratDeflation, Deflation, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratInflationAdjustment, InflationAdjustment, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcBureaucratPapercut, Papercut, CombatActionPriority.Low);

            //Debuffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.InitiativeDebuffs).OrderByStackingOrder(), InitDebuffs, CombatActionPriority.Medium);
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.InitiativeDebuffs).OrderByStackingOrder(), CratDebuffOthersInCombat, CombatActionPriority.Medium);
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.InitiativeDebuffs).OrderByStackingOrder(), MalaiseTargetDebuff, CombatActionPriority.Medium);
            RegisterSpellProcessor(RelevantNanos.GeneralRadACDebuff, InitDebuffs);
            RegisterSpellProcessor(RelevantNanos.GeneralProjACDebuff, InitDebuffs);
            RegisterSpellProcessor(RelevantNanos.PuissantVoidInertia, AOERoot, CombatActionPriority.High);

            RegisterSpellProcessor(RelevantNanos.ShadowlandsCalms, SLCalmDebuff, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.AoECalms, AoECalmDebuff, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.RkCalms, RKCalmDebuff, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.LastMinNegotiations, Calm12Man, CombatActionPriority.High);
            //RegisterSpellProcessor(RelevantNanos.RkCalms, CalmSector7, CombatActionPriority.High);

            //Debuff Aura
            RegisterSpellProcessor(RelevantNanos.NanoPointsDebuffAuras, DebuffNanoDrainAura);
            RegisterSpellProcessor(RelevantNanos.NanoResDebuffAuras, DebuffNanoResistAura);
            RegisterSpellProcessor(RelevantNanos.CritDebuffAuras, DebuffCritAura);

            //Buffs
            RegisterSpellProcessor(RelevantNanos.PetWarp, PetWarp);
            RegisterSpellProcessor(RelevantNanos.SingleTargetNukes, SingleTargetNuke, CombatActionPriority.Low);
            RegisterSpellProcessor(RelevantNanos.WorkplaceDepression, WorkplaceDepressionTargetDebuff, CombatActionPriority.Low);
            RegisterSpellProcessor(RelevantNanos.PistolBuffsSelf, PistolSelfBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Psy_IntBuff).OrderByStackingOrder(), GenericBuff);

            //Buff Aura
            RegisterSpellProcessor(RelevantNanos.AadBuffAuras, BuffAAOAADAura);
            RegisterSpellProcessor(RelevantNanos.CritBuffAuras, BuffCritAura);
            RegisterSpellProcessor(RelevantNanos.NanoResBuffAuras, BuffNanoResistAura);

            //Team Buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDeltaBuffs).OrderByStackingOrder(), NanoDelta);
            RegisterSpellProcessor(RelevantNanos.PistolBuffs, PistolMasteryBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.CriticalDecreaseBuff).OrderByStackingOrder(), TeamBuff);

            //Pet Buffs
            if (Spell.Find(RelevantNanos.CorporateStrategy, out Spell spell))
            {
                RegisterSpellProcessor(RelevantNanos.CorporateStrategy, CorporateStrategy);
            }
            else
            {
                RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetShortTermDamageBuffs).OrderByStackingOrder(), PetTargetBuff);
            }

            RegisterSpellProcessor(RelevantNanos.PetCleanse, PetCleanse);
            RegisterSpellProcessor(RelevantNanos.MastersBidding, MastersBidding);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetDamageOverTimeResistNanos).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetDefensiveNanos).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetTauntBuff).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(RelevantNanos.DroidDamageMatrix, DroidMatrixBuff);
            RegisterSpellProcessor(RelevantNanos.DroidPressureMatrix, DroidMatrixBuff);

            //Pet Spawners
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SupportPets).OrderByStackingOrder(), CarloSpawner);
            RegisterSpellProcessor(PetsList.Pets.Where(x => x.Value.PetType == PetType.Attack).Select(x => x.Key).ToArray(), RobotSpawner);

            //Pet Shells
            foreach (PetSpellData petData in PetsList.Pets.Values)
            {
                RegisterItemProcessor(petData.ShellId, petData.ShellId2, RobotSpawnerItem);
            }

            //Pet Trimmers
            ResetTrimmers();
            RegisterItemProcessor(RelevantTrimmers.PositiveAggressiveDefensive, RelevantTrimmers.PositiveAggressiveDefensive, PetAggDefTrimmer);
            RegisterItemProcessor(RelevantTrimmers.IncreaseAggressivenessHigh, RelevantTrimmers.IncreaseAggressivenessHigh, PetAggressiveTrimmer);
            RegisterItemProcessor(RelevantTrimmers.DivertEnergyToOffense, RelevantTrimmers.DivertEnergyToOffense, PetDivertOffTrimmer);

            //Pet Perks
            RegisterPerkProcessor(PerkHash.Puppeteer, Puppeteer);

            PluginDirectory = pluginDir;
        }

        public Window[] _windows => new Window[] { _calmingWindow, _buffWindow, _petWindow, _procWindow, _debuffWindow };

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

        private void HandlePetViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_petView)) { return; }

                _petView = View.CreateFromXml(PluginDirectory + "\\UI\\BureaucratPetsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Pets", XmlViewName = "BureaucratPetsView" }, _petView);
            }
            else if (_petWindow == null || (_petWindow != null && !_petWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_petWindow, PluginDir, new WindowOptions() { Name = "Pets", XmlViewName = "BureaucratPetsView" }, _petView, out var container);
                _petWindow = container;
            }
        }

        private void HanndleBuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_buffView)) { return; }

                _buffView = View.CreateFromXml(PluginDirectory + "\\UI\\BureaucratBuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Buffs", XmlViewName = "BureaucratBuffsView" }, _buffView);
            }
            else if (_buffWindow == null || (_buffWindow != null && !_buffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_buffWindow, PluginDir, new WindowOptions() { Name = "Buffs", XmlViewName = "BureaucratBuffsView" }, _buffView, out var container);
                _buffWindow = container;
            }
        }

        private void HandleProcViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_procView)) { return; }

                _procView = View.CreateFromXml(PluginDirectory + "\\UI\\BureaucratProcsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Procs", XmlViewName = "BureaucratProcsView" }, _procView);
            }
            else if (_procWindow == null || (_procWindow != null && !_procWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_procWindow, PluginDir, new WindowOptions() { Name = "Procs", XmlViewName = "BureaucratProcsView" }, _procView, out var container);
                _procWindow = container;
            }
        }

        private void HandleDebuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_debuffView)) { return; }

                _debuffView = View.CreateFromXml(PluginDirectory + "\\UI\\BureaucratDebuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Debuffs", XmlViewName = "BureaucratDebuffsView" }, _debuffView);
            }
            else if (_debuffWindow == null || (_debuffWindow != null && !_debuffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_debuffWindow, PluginDir, new WindowOptions() { Name = "Debuffs", XmlViewName = "BureaucratDebuffsView" }, _debuffView, out var container);
                _debuffWindow = container;
            }
        }

        private void HandleCalmingViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_calmView)) { return; }

                _calmView = View.CreateFromXml(PluginDirectory + "\\UI\\BureaucratCalmingView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Calming", XmlViewName = "BureaucratCalmingView" }, _calmView);
            }
            else if (_calmingWindow == null || (_calmingWindow != null && !_calmingWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_calmingWindow, PluginDir, new WindowOptions() { Name = "Calming", XmlViewName = "BureaucratCalmingView" }, _calmView, out var container);
                _calmingWindow = container;
            }
        }

        private void OnZoned(object s, EventArgs e)
        {
            ResetTrimmers();
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

            if (IsSettingEnabled("SyncPets"))
                SynchronizePetCombatStateWithOwner();

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                if (SettingsController.settingsWindow.FindView("CalmingView", out Button calmView))
                {
                    calmView.Tag = SettingsController.settingsWindow;
                    calmView.Clicked = HandleCalmingViewClick;
                }

                if (SettingsController.settingsWindow.FindView("ProcsView", out Button procView))
                {
                    procView.Tag = SettingsController.settingsWindow;
                    procView.Clicked = HandleProcViewClick;
                }

                if (SettingsController.settingsWindow.FindView("PetsView", out Button petView))
                {
                    petView.Tag = SettingsController.settingsWindow;
                    petView.Clicked = HandlePetViewClick;
                }

                if (SettingsController.settingsWindow.FindView("BuffsView", out Button buffView))
                {
                    buffView.Tag = SettingsController.settingsWindow;
                    buffView.Clicked = HanndleBuffViewClick;
                }

                if (SettingsController.settingsWindow.FindView("DebuffsView", out Button debuffView))
                {
                    debuffView.Tag = SettingsController.settingsWindow;
                    debuffView.Clicked = HandleDebuffViewClick;
                }
            }

            HandleCancelDebuffAuras();
            HandleCancelBuffAuras();
        }

        #region Perks


        private bool FormsinTriplicate(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.FormsinTriplicate != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool NextWindowOver(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.NextWindowOver != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool PleaseHold(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.PleaseHold != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool SocialServices(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.SocialServices != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool WaitInThatQueue(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.WaitInThatQueue != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool Deflation(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.Deflation != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool InflationAdjustment(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.InflationAdjustment != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool LostPaperwork(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.LostPaperwork != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool MobilityEmbargo(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.MobilityEmbargo != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool Papercut(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.Papercut != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool TaxAudit(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.TaxAudit != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool WrongWindow(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.WrongWindow != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }


        #endregion

        #region Buffs

        protected bool PetWarp(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("WarpPets") || !CanCast(spell) || !CanLookupPetsAfterZone()) { return false; }

            foreach (Pet pet in DynelManager.LocalPlayer.Pets)
            {
                if (pet.Character == null)
                    return true;
            }

            return true;
        }

        private bool NanoDelta(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Team.IsInTeam)
            {
                if (IsSettingEnabled("NanoDeltaTeam"))
                {
                    return CheckNotProfsBeforeCast(spell, fightingTarget, ref actionTarget);
                }

                return GenericBuff(spell, fightingTarget, ref actionTarget);
            }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        public bool PetCleanse(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanLookupPetsAfterZone()) { return false; }

            return DynelManager.LocalPlayer.Pets
                .Where(c => c.Character == null || c.Character.Buffs.Contains(NanoLine.Root) || c.Character.Buffs.Contains(NanoLine.Snare)
                    || c.Character.Buffs.Contains(NanoLine.Mezz)).Any();
        }

        protected bool MastersBidding(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("BuffPets") || !CanLookupPetsAfterZone() || !IsSettingEnabled("MastersBidding")) { return false; }

            foreach (Pet pet in DynelManager.LocalPlayer.Pets)
            {
                if (pet.Character == null) continue;

                if (!pet.Character.Buffs.Contains(NanoLine.SiphonBox683)
                    && pet.Type == PetType.Attack)
                {
                    if (spell.IsReady)
                        spell.Cast(pet.Character, true);

                    //Not working for some reason

                    //actionTarget.Target = pet.Character;
                    //actionTarget.ShouldSetTarget = true;

                    //return true;
                }
            }

            return false;
        }

        public bool Puppeteer(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("BuffPets") || !CanLookupPetsAfterZone() || fightingTarget == null) { return false; }

            foreach (Pet pet in DynelManager.LocalPlayer.Pets)
            {
                if (pet.Character == null) continue;

                if (CanPerkPuppeteer(pet))
                {
                    actionTarget.Target = pet.Character;

                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }
        private bool BuffCritAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (BuffingAuraSelection.Crit != (BuffingAuraSelection)_settings["BuffingAuraSelection"].AsInt32()) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool BuffNanoResistAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (BuffingAuraSelection.NanoResist != (BuffingAuraSelection)_settings["BuffingAuraSelection"].AsInt32()) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool BuffAAOAADAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (BuffingAuraSelection.AAOAAD != (BuffingAuraSelection)_settings["BuffingAuraSelection"].AsInt32()) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool DebuffCritAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DebuffingAuraSelection.Crit != (DebuffingAuraSelection)_settings["DebuffingAuraSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool DebuffNanoResistAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DebuffingAuraSelection.NanoResist != (DebuffingAuraSelection)_settings["DebuffingAuraSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool DebuffNanoDrainAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DebuffingAuraSelection.MaxNano != (DebuffingAuraSelection)_settings["DebuffingAuraSelection"].AsInt32()) { return false; }

            return CombatBuff(spell, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Calms

        //private bool CalmSector7(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        //{
        //    if (!IsSettingEnabled("Buffing")) { return false; }

        //    if (!CanCast(spell)) { return false; }

        //    if (!IsSettingEnabled("CalmSector7")) { return false; }

        //    SimpleChar target = DynelManager.NPCs
        //        .Where(c => !debuffOSTargetsToIgnore.Contains(c.Name)) //Is not a quest target etc
        //        .Where(c => c.IsInLineOfSight)
        //        .Where(c => c.Name == "Kyr'Ozch Guardian")
        //        .Where(c => c.DistanceFrom(DynelManager.LocalPlayer) < 30f) //Is in range for debuff (we assume weapon range == debuff range)
        //        .Where(c => !c.Buffs.Contains(NanoLine.Mezz))
        //        .Where(c => c.MaxHealth < 1000000)
        //        .FirstOrDefault();

        //    if (target != null)
        //    {
        //        actionTarget.Target = target;
        //        actionTarget.ShouldSetTarget = true;
        //        return true;
        //    }

        //    return false;
        //}

        private bool RKCalmDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (CalmingSelection.RK != (CalmingSelection)_settings["CalmingSelection"].AsInt32()
                || !CanCast(spell) || ModeSelection.None == (ModeSelection)_settings["ModeSelection"].AsInt32()) { return false; }

            if (ModeSelection.All == (ModeSelection)_settings["ModeSelection"].AsInt32())
            {
                SimpleChar target = DynelManager.NPCs
                    .Where(c => !debuffOSTargetsToIgnore.Contains(c.Name)
                        && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && !c.Buffs.Contains(NanoLine.Mezz)
                        && c.MaxHealth < 1000000)
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.Target = target;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            if (ModeSelection.Adds == (ModeSelection)_settings["ModeSelection"].AsInt32())
            {
                SimpleChar target = DynelManager.NPCs
                    .Where(c => !debuffOSTargetsToIgnore.Contains(c.Name)
                        && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && !c.Buffs.Contains(NanoLine.Mezz)
                        && c.MaxHealth < 1000000
                        && c.FightingTarget != null
                        && IsAttackingUs(c))
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.Target = target;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        private bool SLCalmDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (CalmingSelection.SL != (CalmingSelection)_settings["CalmingSelection"].AsInt32()
                || !CanCast(spell) || ModeSelection.None == (ModeSelection)_settings["ModeSelection"].AsInt32()) { return false; }

            if (ModeSelection.All == (ModeSelection)_settings["ModeSelection"].AsInt32())
            {
                SimpleChar target = DynelManager.NPCs
                    .Where(c => !debuffOSTargetsToIgnore.Contains(c.Name)
                        && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && !c.Buffs.Contains(NanoLine.Mezz)
                        && c.MaxHealth < 1000000)
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.Target = target;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            if (ModeSelection.Adds == (ModeSelection)_settings["ModeSelection"].AsInt32())
            {
                SimpleChar target = DynelManager.NPCs
                    .Where(c => !debuffOSTargetsToIgnore.Contains(c.Name)
                        && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && !c.Buffs.Contains(NanoLine.Mezz)
                        && c.MaxHealth < 1000000
                        && c.FightingTarget != null
                        && IsAttackingUs(c))
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.Target = target;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        private bool AoECalmDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (CalmingSelection.AOE != (CalmingSelection)_settings["CalmingSelection"].AsInt32()
                || !CanCast(spell) || ModeSelection.None == (ModeSelection)_settings["ModeSelection"].AsInt32()) { return false; }

            if (ModeSelection.All == (ModeSelection)_settings["ModeSelection"].AsInt32())
            {
                SimpleChar target = DynelManager.NPCs
                    .Where(c => !debuffOSTargetsToIgnore.Contains(c.Name)
                        && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && !c.Buffs.Contains(NanoLine.Mezz)
                        && c.MaxHealth < 1000000)
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.Target = target;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            if (ModeSelection.Adds == (ModeSelection)_settings["ModeSelection"].AsInt32())
            {
                SimpleChar target = DynelManager.NPCs
                    .Where(c => !debuffOSTargetsToIgnore.Contains(c.Name)
                        && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && !c.Buffs.Contains(NanoLine.Mezz)
                        && c.MaxHealth < 1000000
                        && c.FightingTarget != null
                        && IsAttackingUs(c))
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.Target = target;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        private bool Calm12Man(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (!IsSettingEnabled("Calm12Man") || !CanCast(spell)) { return false; }

            List<SimpleChar> targets = DynelManager.NPCs
                .Where(c => c.IsAlive 
                    && c.DistanceFrom(DynelManager.LocalPlayer) < 20f
                    && (c.Name == "Right Hand of Madness" || c.Name == "Deranged Xan")
                    && (!c.Buffs.Contains(267535) || !c.Buffs.Contains(267536)))
                .ToList();

            if (targets.Count >= 1)
            {
                actionTarget.Target = targets.FirstOrDefault();
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            return false;
        }

        #endregion

        #region Debuffs

        private bool SingleTargetNuke(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || !CanCast(spell) || !IsSettingEnabled("Nukes")) { return false; }

            if (Spell.Find(273631, out Spell workplace))
            {
                if (!fightingTarget.Buffs.Contains(273632) && !fightingTarget.Buffs.Contains(301842) &&
                    ((fightingTarget.HealthPercent >= 40 && fightingTarget.MaxHealth < 1000000)
                    || fightingTarget.MaxHealth > 1000000)) { return false; }
            }

            return true;
        }

        private bool WorkplaceDepressionTargetDebuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || !CanCast(spell) || !IsSettingEnabled("Nukes")
                || fightingTarget.Buffs.Contains(273632) || fightingTarget.Buffs.Contains(301842)
                || (fightingTarget.HealthPercent < 40 && fightingTarget.MaxHealth < 1000000)) { return false; }

            return true;
        }

        private bool InitDebuffs(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing") || !CanCast(spell)) { return false; }

            if (spell.Nanoline == NanoLine.GeneralRadiationACDebuff || spell.Nanoline == NanoLine.GeneralProjectileACDebuff)
            {
                if (fightingTarget != null)
                {
                    return DebuffTarget(spell, spell.Nanoline, fightingTarget, ref actionTarget);
                }

                return false;
            }

            if (InitDebuffSelection.OS == (InitDebuffSelection)_settings["InitDebuffSelection"].AsInt32())
            {
                //This is optimal USE THIS
                foreach (SimpleChar _mob in DynelManager.NPCs)
                {
                    if (debuffOSTargetsToIgnore.Contains(_mob.Name)
                        || _mob.FightingTarget == null || _mob.Buffs.Contains(301844) || !_mob.IsInLineOfSight
                        || _mob.Buffs.Contains(NanoLine.Mezz) || _mob.Buffs.Contains(NanoLine.AOEMezz)
                        || _mob.DistanceFrom(DynelManager.LocalPlayer) >= 30f) 
                            continue;

                    if (SpellChecksOther(spell, spell.Nanoline, _mob))
                    {
                        actionTarget.Target = _mob;
                        actionTarget.ShouldSetTarget = true;
                        return true;
                    }
                }

                return false;
            }

            if (InitDebuffSelection.Target == (InitDebuffSelection)_settings["InitDebuffSelection"].AsInt32()
                && fightingTarget != null)
            {
                if (debuffTargetsToIgnore.Contains(fightingTarget.Name)) { return false; }

                return DebuffTarget(spell, spell.Nanoline, fightingTarget, ref actionTarget);
            }

            return false;
        }

        private bool RootLogic(SimpleChar target, Spell spell)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (target.Buffs.Contains(NanoLine.Root))
            {
                if (target.Buffs.FirstOrDefault(c => c.Nanoline == NanoLine.Root).RemainingTime < 30f)
                    return true;
            }

            if (!target.Buffs.Contains(NanoLine.Root))
                return true;

            return false;
        }

        private bool AOERoot(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")
                || !IsSettingEnabled("AOERoot") || !CanCast(spell)) { return false; }

            if (DynelManager.Characters
                .Where(c => c.DistanceFrom(DynelManager.LocalPlayer) < 50f
                    && (c.Name == "Flaming Vengeance"
                    || c.Name == "Hand of the Colonel"))
                .Any())
            {
                actionTarget.Target = DynelManager.Characters
                    .Where(c => c.DistanceFrom(DynelManager.LocalPlayer) < 50f
                        && (c.Name == "Flaming Vengeance"
                        || c.Name == "Hand of the Colonel"))
                    .OrderByDescending(c => c.DistanceFrom(DynelManager.LocalPlayer))
                    .FirstOrDefault();

                if (actionTarget.Target != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Logic

        private bool CanPerkPuppeteer(Pet pet)
        {
            return pet.Type == PetType.Attack;
        }

        protected bool FindSpellNanoLineFallbackToId(Spell spell, Buff[] buffs, out Buff buff)
        {
            if (buffs.Find(spell.Nanoline, out Buff buffFromNanoLine))
            {
                buff = buffFromNanoLine;
                return true;
            }
            int spellId = spell.Id;
            if (RelevantNanos.PetNanoToBuff.ContainsKey(spellId))
            {
                int buffId = RelevantNanos.PetNanoToBuff[spellId];
                if (buffs.Find(buffId, out Buff buffFromId))
                {
                    buff = buffFromId;
                    return true;
                }
            }

            buff = null;
            return false;
        }

        protected virtual bool RobotSpawnerItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return PetSpawnerItem(PetsList.Pets, item, fightingTarget, ref actionTarget);
        }

        protected bool CarloSpawner(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return NoShellPetSpawner(PetType.Support, spell, fightingTarget, ref actionTarget);
        }

        protected bool RobotSpawner(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return PetSpawner(PetsList.Pets, spell, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Pets

        private bool RobotNeedsBuff(Spell spell, Pet pet)
        {
            if (pet.Type != PetType.Attack) { return false; }

            if (FindSpellNanoLineFallbackToId(spell, pet.Character.Buffs, out Buff buff))
            {
                //Don't cast if weaker than existing
                if (spell.StackingOrder < buff.StackingOrder) { return false; }

                //Don't cast if greater than 10% time remaining
                if (buff.RemainingTime / buff.TotalTime > 0.1) { return false; }
            }

            return true;
        }

        private bool CorporateStrategy(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return PetTargetBuff(NanoLine.PetShortTermDamageBuffs, PetType.Attack, spell, fightingTarget, ref actionTarget);
        }

        private bool PetTargetBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return PetTargetBuff(spell.Nanoline, PetType.Attack, spell, fightingTarget, ref actionTarget);
        }

        protected bool PetDivertOffTrimmer(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("DivertTrimmer") || !CanLookupPetsAfterZone() || !CanTrim()) { return false; }

            foreach (Pet pet in DynelManager.LocalPlayer.Pets)
            {
                if (pet.Character == null) continue;

                if (pet.Type == PetType.Attack
                        && CanDivertOffTrim(pet))
                {
                    actionTarget.Target = pet.Character;

                    actionTarget.ShouldSetTarget = true;
                    petTrimmedOffDiv[PetType.Attack] = true;
                    _lastPetTrimDivertOffTime[PetType.Attack] = Time.NormalTime;
                    _lastTrimTime = Time.NormalTime;
                    return true;
                }

                if (pet.Type == PetType.Support
                        && CanDivertOffTrim(pet))
                {
                    actionTarget.Target = pet.Character;

                    actionTarget.ShouldSetTarget = true;
                    petTrimmedOffDiv[PetType.Support] = true;
                    _lastPetTrimDivertOffTime[PetType.Support] = Time.NormalTime;
                    _lastTrimTime = Time.NormalTime;
                    return true;
                }
            }

            return false;
        }

        protected bool PetAggDefTrimmer(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("AggDefTrimmer") || !CanLookupPetsAfterZone() || !CanTrim()) { return false; }

            foreach (Pet pet in DynelManager.LocalPlayer.Pets)
            {
                if (pet.Character == null) continue;

                if (pet.Type == PetType.Attack
                        && CanAggDefTrim(pet))
                {
                    actionTarget.Target = pet.Character;

                    actionTarget.ShouldSetTarget = true;
                    petTrimmedAggDef[PetType.Attack] = true;
                    _lastTrimTime = Time.NormalTime;
                    return true;
                }

                if (pet.Type == PetType.Support
                    && CanAggDefTrim(pet))
                {
                    actionTarget.Target = pet.Character;

                    actionTarget.ShouldSetTarget = true;
                    petTrimmedAggDef[PetType.Support] = true;
                    _lastTrimTime = Time.NormalTime;
                    return true;
                }
            }

            return false;
        }

        protected bool PetAggressiveTrimmer(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("TauntTrimmer") || !CanLookupPetsAfterZone() || !CanTrim()) { return false; }

            foreach (Pet pet in DynelManager.LocalPlayer.Pets)
            {
                if (pet.Character == null) continue;

                if (pet.Type == PetType.Attack
                        && CanTauntTrim(pet))
                {
                    actionTarget.Target = pet.Character;

                    actionTarget.ShouldSetTarget = true;
                    attackPetTrimmedAggressive = true;
                    _lastTrimTime = Time.NormalTime;
                    return true;
                }
            }

            return false;
        }

        protected bool DroidMatrixBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("BuffPets") || !CanLookupPetsAfterZone()) { return false; }

            if (DynelManager.LocalPlayer.Pets
                .Where(c => RobotNeedsBuff(spell, c))
                .Any())
            {
                actionTarget.Target = DynelManager.LocalPlayer.Pets
                    .Where(c => RobotNeedsBuff(spell, c))
                    .FirstOrDefault().Character;

                if (actionTarget.Target != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Misc

        public enum InitDebuffSelection
        {
            None, Target, OS
        }

        private static class RelevantNanos
        {
            public const int WorkplaceDepression = 273631;
            public const int DroidDamageMatrix = 267916;
            public const int DroidPressureMatrix = 302247;
            public const int CorporateStrategy = 267611;
            public const int LastMinNegotiations = 267535;
            public const int SkilledGunSlinger = 263251;
            public const int GreaterGunSlinger = 263250;
            public const int MastersBidding = 268171;
            public const int PuissantVoidInertia = 224129;
            public const int PetWarp = 209488;

            public static readonly int[] PistolBuffsSelf = { 263250, 263251 };
            public static readonly Spell[] PistolBuffs = Spell.GetSpellsForNanoline(NanoLine.PistolBuff).OrderByStackingOrder().Where(spell => spell.Id != GreaterGunSlinger && spell.Id != SkilledGunSlinger).ToArray();
            public static readonly int[] SingleTargetNukes = { 273307, WorkplaceDepression, 270250, 78400, 30082, 78394, 78395, 82000, 78396, 78397, 30091, 78399, 81996, 30083, 81997, 30068, 81998, 78398, 81999, 29618 };
            public static readonly int[] AoeRoots = { 224129, 224127, 224125, 224123, 224121, 224119, 82166, 82164, 82163, 82161, 82160, 82159, 82158, 82157, 82156 };
            public static readonly int[] AoeRootDebuffs = { 82137, 244634, 244633, 244630, 244631, 244632, 82138, 82139, 244629, 82140, 82141, 82142, 82143, 82144, 82145 };
            public static readonly int[] AadBuffAuras = { 270783, 155807, 155806, 155805, 155809, 155808 };
            public static readonly int[] CritBuffAuras = { 157503, 157499 };
            public static readonly int[] NanoResBuffAuras = { 157504, 157500, 157501, 157502 };
            public static readonly int[] NanoPointsDebuffAuras = { 275826, 157524, 157534, 157533, 157532, 157531 };
            public static readonly int[] CritDebuffAuras = { 157530, 157529, 157528 };
            public static readonly int[] NanoResDebuffAuras = { 157527, 157526, 157525, 157535 };
            public static readonly int[] GeneralRadACDebuff = { 302143, 302142 };
            public static readonly int[] GeneralProjACDebuff = { 302150, 302152 };
            public static readonly int[] PetCleanse = { 269870, 269869 };

            public static readonly int[] ShadowlandsCalms = { 224143, 224141, 224139, 224149, 224147, 224145,
            224137, 224135, 224133, 224131, 219020 };
            public static readonly int[] RkCalms = { 155577, 100428, 100429, 100430, 100431, 100432,
            30093, 30056, 30065 };
            public static readonly int[] AoECalms = { 100422, 100424, 100426 };

            public static Dictionary<int, int> PetNanoToBuff = new Dictionary<int, int>
            {
                {DroidDamageMatrix, 285696},
                {DroidPressureMatrix, 302246},
                {CorporateStrategy, 285695}
            };

        }

        private static class RelevantTrimmers
        {
            public const int IncreaseAggressivenessLow = 154940;
            public const int IncreaseAggressivenessHigh = 154940;
            public const int DivertEnergyToOffense = 88378;
            public const int PositiveAggressiveDefensive = 88384;
        }

        public enum ProcType1Selection
        {
            PleaseHold, FormsinTriplicate, SocialServices, NextWindowOver, WaitInThatQueue
        }

        public enum ProcType2Selection
        {
            MobilityEmbargo, WrongWindow, TaxAudit, LostPaperwork, Deflation, InflationAdjustment, Papercut
        }

        public enum BuffingAuraSelection
        {
            AAOAAD, Crit, NanoResist
        }
        public enum DebuffingAuraSelection
        {
            None, NanoResist, Crit, MaxNano
        }
        public enum CalmingSelection
        {
            SL, RK, AOE
        }
        public enum ModeSelection
        {
            None, All, Adds
        }

        protected bool CanTrim()
        {
            return _lastTrimTime + DelayBetweenTrims < Time.NormalTime;
        }
        protected bool CanDivertOffTrim(Pet pet)
        {
            return _lastPetTrimDivertOffTime[pet.Type] + DelayBetweenDiverTrims < Time.NormalTime;
        }

        protected bool CanAggDefTrim(Pet pet)
        {
            return !petTrimmedAggDef[pet.Type];
        }

        protected bool CanTauntTrim(Pet pet)
        {
            return pet.Type == PetType.Attack && !attackPetTrimmedAggressive;
        }

        private void ResetTrimmers()
        {
            attackPetTrimmedAggressive = false;
            petTrimmedOffDiv[PetType.Attack] = false;
            petTrimmedOffDiv[PetType.Support] = false;
            petTrimmedAggDef[PetType.Attack] = false;
            petTrimmedAggDef[PetType.Support] = false;
        }

        private void HandleCancelBuffAuras()
        {
            if (BuffingAuraSelection.AAOAAD != (BuffingAuraSelection)_settings["BuffingAuraSelection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.AadBuffAuras);
            }
            if (BuffingAuraSelection.Crit != (BuffingAuraSelection)_settings["BuffingAuraSelection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.CritBuffAuras);
            }
            if (BuffingAuraSelection.NanoResist != (BuffingAuraSelection)_settings["BuffingAuraSelection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.NanoResBuffAuras);
            }
        }

        private void HandleCancelDebuffAuras()
        {
            CancelHostileAuras(RelevantNanos.CritDebuffAuras);
            CancelHostileAuras(RelevantNanos.NanoPointsDebuffAuras);
            CancelHostileAuras(RelevantNanos.NanoResDebuffAuras);

            if (DebuffingAuraSelection.Crit != (DebuffingAuraSelection)_settings["DebuffingAuraSelection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.CritDebuffAuras);
            }
            if (DebuffingAuraSelection.MaxNano != (DebuffingAuraSelection)_settings["DebuffingAuraSelection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.NanoPointsDebuffAuras);
            }
            if (DebuffingAuraSelection.NanoResist != (DebuffingAuraSelection)_settings["DebuffingAuraSelection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.NanoResDebuffAuras);
            }
        }

        #endregion
    }
}
