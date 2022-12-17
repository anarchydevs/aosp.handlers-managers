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
using System.Reflection;

namespace CombatHandler.Soldier
{
    public class SoldCombathandler : GenericCombatHandler
    {
        private static string PluginDirectory;

        private static Window _buffWindow;
        private static Window _tauntWindow;
        private static Window _procWindow;

        private static View _buffView;
        private static View _tauntView;
        private static View _procView;

        private static double _singleTauntTick;
        private static double _singleTaunt;
        private static double _ncuUpdateTime;

        private static int SolTauntDelaySingle;

        public SoldCombathandler(string pluginDir) : base(pluginDir)
        {
            IPCChannel.RegisterCallback((int)IPCOpcode.RemainingNCU, OnRemainingNCUMessage);

            Config.CharSettings[Game.ClientInst].SolTauntDelaySingleChangedEvent += SolTauntDelaySingle_Changed;

            _settings.AddVariable("Buffing", true);
            _settings.AddVariable("Composites", true);

            //LE Proc
            _settings.AddVariable("ProcType1Selection", (int)ProcType1Selection.FuriousAmmunition);
            _settings.AddVariable("ProcType2Selection", (int)ProcType2Selection.GrazeJugularVein);

            _settings.AddVariable("ReflectSelection", (int)ReflectSelection.Shadowlands);
            _settings.AddVariable("AAOSelection", (int)AAOSelection.Self);
            _settings.AddVariable("InitBuffSelection", (int)InitBuffSelection.Self);
            _settings.AddVariable("RiotControlSelection", (int)RiotControlSelection.Self);
            _settings.AddVariable("CompHeavyArtSelection", (int)CompHeavyArtSelection.None);
            _settings.AddVariable("SingleTauntsSelection", (int)SingleTauntsSelection.None);

            _settings.AddVariable("NotumGrenades", false);

            _settings.AddVariable("LegShot", false);

            RegisterSettingsWindow("Soldier Handler", "SoldierSettingsView.xml");

            //LE Proc
            RegisterPerkProcessor(PerkHash.LEProcSoldierFuriousAmmunition, FuriousAmmunition, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierTargetAcquired, TargetAcquired, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierReconditioned, Reconditioned, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierConcussiveShot, ConcussiveShot, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierEmergencyBandages, EmergencyBandages, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierSuccessfulTargeting, SuccessfulTargeting, CombatActionPriority.Low);

            RegisterPerkProcessor(PerkHash.LEProcSoldierFuseBodyArmor, FuseBodyArmor, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierOnTheDouble, OnTheDouble, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierGrazeJugularVein, GrazeJugularVein, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierGearAssaultAbsorption, GearAssaultAbsorption, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierDeepSixInitiative, DeepSixInitiative, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcSoldierShootArtery, ShootArtery, CombatActionPriority.Low);

            //Perks
            RegisterPerkProcessor(PerkHash.LegShot, LegShot);

            //Spells
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ReflectShield).Where(c => c.Name.Contains("Mirror")).OrderByStackingOrder(), AugmentedMirrorShieldMKV);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ReflectShield).Where(c => !c.Name.Contains("Mirror")).OrderByStackingOrder(), RKReflects);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ShadowlandReflectBase).OrderByStackingOrder(), SLReflects);
            RegisterSpellProcessor(RelevantNanos.SolDrainHeal, SolDrainHeal);
            RegisterSpellProcessor(RelevantNanos.TauntBuffs, SingleTargetTaunt, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.Phalanx, PhalanxBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.HPBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SiphonBox683).OrderByStackingOrder(), NotumGrenades);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MajorEvasionBuffs).OrderByStackingOrder(), GenericBuffExcludeInnerSanctum);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SoldierFullAutoBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TotalFocus).OrderByStackingOrder(), GenericBuff);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SoldierShotgunBuff).OrderByStackingOrder(), ShotgunBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.HeavyWeaponsBuffs).OrderByStackingOrder(), HeavyWeaponBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ArmorBuff).OrderByStackingOrder(), GenericBuff);

            RegisterSpellProcessor(RelevantNanos.ArBuffs, ARBuff);
            RegisterSpellProcessor(RelevantNanos.HeavyComp, HeavyCompBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SoldierDamageBase).OrderByStackingOrder(), GenericBuff);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AAOBuffs).OrderByStackingOrder(), AAOBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PistolBuff).OrderByStackingOrder(), Pistol);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.BurstBuff).OrderByStackingOrder(), RiotControlBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.InitiativeBuffs).OrderByStackingOrder(), InitBuff);

            // Needs work for 2nd tanking Abmouth and Ayjous
            //if (TauntTools.CanTauntTool())
            //{
            //    Item tauntTool = TauntTools.GetBestTauntTool();
            //    RegisterItemProcessor(tauntTool.LowId, tauntTool.HighId, TauntTool);
            //}

            PluginDirectory = pluginDir;

            SolTauntDelaySingle = Config.CharSettings[Game.ClientInst].SolTauntDelaySingle;
        }
        public Window[] _windows => new Window[] { _buffWindow, _tauntWindow, _procWindow };

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

                _buffView = View.CreateFromXml(PluginDirectory + "\\UI\\SoldierBuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Buffs", XmlViewName = "SoldierBuffsView" }, _buffView);
            }
            else if (_buffWindow == null || (_buffWindow != null && !_buffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_buffWindow, PluginDir, new WindowOptions() { Name = "Buffs", XmlViewName = "SoldierBuffsView" }, _buffView, out var container);
                _buffWindow = container;
            }
        }

        private void HandleTauntViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_tauntView)) { return; }

                _tauntView = View.CreateFromXml(PluginDirectory + "\\UI\\SoldierTauntsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Taunts", XmlViewName = "SoldierTauntsView" }, _tauntView);

                window.FindView("DelaySingleBox", out TextInputView singleInput);

                if (singleInput != null)
                {
                    singleInput.Text = $"{SolTauntDelaySingle}";
                }
            }
            else if (_tauntWindow == null || (_tauntWindow != null && !_tauntWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_tauntWindow, PluginDir, new WindowOptions() { Name = "Taunts", XmlViewName = "SoldierTauntsView" }, _tauntView, out var container);
                _tauntWindow = container;

                container.FindView("DelaySingleBox", out TextInputView singleInput);

                if (singleInput != null)
                {
                    singleInput.Text = $"{SolTauntDelaySingle}";
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

                _procView = View.CreateFromXml(PluginDirectory + "\\UI\\SoldierProcsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Procs", XmlViewName = "SoldierProcsView" }, _procView);
            }
            else if (_procWindow == null || (_procWindow != null && !_procWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_procWindow, PluginDir, new WindowOptions() { Name = "Procs", XmlViewName = "SoldierProcsView" }, _procView, out var container);
                _procWindow = container;
            }
        }

        #endregion

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                window.FindView("DelaySingleBox", out TextInputView singleInput);

                if (singleInput != null && !string.IsNullOrEmpty(singleInput.Text))
                {
                    if (int.TryParse(singleInput.Text, out int singleValue))
                    {
                        if (Config.CharSettings[Game.ClientInst].SolTauntDelaySingle != singleValue)
                        {
                            Config.CharSettings[Game.ClientInst].SolTauntDelaySingle = singleValue;
                        }
                    }
                }
            }

            if (Time.NormalTime > _ncuUpdateTime + 0.5f)
            {
                RemainingNCUMessage ncuMessage = RemainingNCUMessage.ForLocalPlayer();

                IPCChannel.Broadcast(ncuMessage);

                OnRemainingNCUMessage(0, ncuMessage);

                _ncuUpdateTime = Time.NormalTime;
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                if (SettingsController.settingsWindow.FindView("BuffsView", out Button buffView))
                {
                    buffView.Tag = SettingsController.settingsWindow;
                    buffView.Clicked = HandleBuffViewClick;
                }

                if (SettingsController.settingsWindow.FindView("ProcsView", out Button procView))
                {
                    procView.Tag = SettingsController.settingsWindow;
                    procView.Clicked = HandleProcViewClick;
                }

                if (SettingsController.settingsWindow.FindView("TauntsView", out Button tauntView))
                {
                    tauntView.Tag = SettingsController.settingsWindow;
                    tauntView.Clicked = HandleTauntViewClick;
                }
            }
        }

        #region LE Procs

        private bool ConcussiveShot(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.ConcussiveShot != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool EmergencyBandages(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.EmergencyBandages != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool FuriousAmmunition(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.FuriousAmmunition != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool Reconditioned(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.Reconditioned != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool SuccessfulTargeting(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.SuccessfulTargeting != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool TargetAcquired(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType1Selection.TargetAcquired != (ProcType1Selection)_settings["ProcType1Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool AmbientPurification(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.FuseBodyArmor != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool FuseBodyArmor(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.FuseBodyArmor != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool GearAssaultAbsorption(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.GearAssaultAbsorption != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool DeepSixInitiative(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.DeepSixInitiative != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool GrazeJugularVein(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.GrazeJugularVein != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }
        private bool OnTheDouble(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.OnTheDouble != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        private bool ShootArtery(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ProcType2Selection.ShootArtery != (ProcType2Selection)_settings["ProcType2Selection"].AsInt32()) { return false; }

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Perks
        private bool LegShot(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("LegShot")) { return false; }

            return CyclePerks(perk, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Buffs

        private bool AAOBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AAOSelection.Team == (AAOSelection)_settings["AAOSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.IsInTeam())
                {
                    SimpleChar target = DynelManager.Players
                        .Where(c => c.IsInLineOfSight
                            && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                            && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                            && c.Health > 0
                            && !c.Buffs.Contains(NanoLine.AAOBuffs) && !c.Buffs.Contains(NanoLine.AdventurerMorphBuff)
                            && SpellChecksOther(spell, spell.Nanoline, c))
                        .FirstOrDefault();

                    if (target != null)
                    {
                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = target;
                        return true;
                    }
                }
            }

            if (AAOSelection.Self != (AAOSelection)_settings["AAOSelection"].AsInt32()) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool RiotControlBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (RiotControlSelection.Team == (RiotControlSelection)_settings["RiotControlSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.IsInTeam())
                {
                    SimpleChar target = DynelManager.Players
                        .Where(c => c.IsInLineOfSight
                            && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                            && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                            && c.Health > 0
                            && c.SpecialAttacks.Contains(SpecialAttack.Burst)
                            && SpellChecksOther(spell, spell.Nanoline, c))
                        .FirstOrDefault();

                    if (target != null)
                    {
                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = target;
                        return true;
                    }
                }
            }

            if (RiotControlSelection.None == (RiotControlSelection)_settings["RiotControlSelection"].AsInt32()) { return false; }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        protected bool Inits(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Team.IsInTeam)
                return TeamBuffExclusionWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Ranged);

            if (!GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Ranged)
                || DynelManager.LocalPlayer.Profession == Profession.Doctor || DynelManager.LocalPlayer.Profession == Profession.NanoTechnician) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool InitBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (InitBuffSelection.Team == (InitBuffSelection)_settings["InitBuffSelection"].AsInt32())
            {
                if (Team.IsInTeam)
                {
                    SimpleChar target = DynelManager.Players
                        .Where(c => c.IsInLineOfSight
                            && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                            && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                            && c.Health > 0
                            && c.Profession != Profession.NanoTechnician
                            && SpellChecksOther(spell, spell.Nanoline, c))
                        .FirstOrDefault();

                    if (target != null)
                    {
                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = target;
                        return true;
                    }
                }
            }

            if (InitBuffSelection.None == (InitBuffSelection)_settings["InitBuffSelection"].AsInt32()) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool HeavyCompBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (CompHeavyArtSelection.Team == (CompHeavyArtSelection)_settings["CompHeavyArtSelection"].AsInt32())
            {
                if (Team.IsInTeam)
                {
                    SimpleChar target = DynelManager.Players
                        .Where(c => c.IsInLineOfSight
                            && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                            && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                            && c.Health > 0
                            && !c.Buffs.Contains(NanoLine.FixerSuppressorBuff) && !c.Buffs.Contains(NanoLine.AssaultRifleBuffs)
                            && HeavyCompWeaponChecks(c)
                            && SpellChecksOther(spell, spell.Nanoline, c))
                        .FirstOrDefault();

                    if (target != null)
                    {
                        if (target.Identity == DynelManager.LocalPlayer.Identity
                            && GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.AssaultRifle)) { return false; }

                        actionTarget.ShouldSetTarget = true;
                        actionTarget.Target = target;
                        return true;
                    }
                }
            }

            if (CompHeavyArtSelection.None == (CompHeavyArtSelection)_settings["CompHeavyArtSelection"].AsInt32()) { return false; }

            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Smg)
                                || BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Shotgun)
                                || BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Grenade);
        }

        private bool HeavyWeaponBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.HeavyWeapons);
        }

        private bool ShotgunBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Shotgun);
        }

        private bool ARBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.AssaultRifle);
        }

        private bool RKReflects(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ReflectSelection.RubiKa == (ReflectSelection)_settings["ReflectSelection"].AsInt32())
                return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);

            if (ReflectSelection.RubiKaTeam == (ReflectSelection)_settings["ReflectSelection"].AsInt32())
                return GenericBuff(spell, fightingTarget, ref actionTarget);

            return false;
        }

        private bool PhalanxBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Buffs.Contains(162357)) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool SLReflects(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (ReflectSelection.Shadowlands != (ReflectSelection)_settings["ReflectSelection"].AsInt32()) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool NotumGrenades(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("NotumGrenades")) { return false; }

            return Buff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool SingleTargetTaunt(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (SingleTauntsSelection.OS == (SingleTauntsSelection)_settings["SingleTauntsSelection"].AsInt32()
                && Time.NormalTime > _singleTaunt + SolTauntDelaySingle)
            {
                SimpleChar mob = DynelManager.NPCs
                    .Where(c => c.IsAttacking && c.FightingTarget != null
                        && c.IsInLineOfSight
                        && !debuffOSTargetsToIgnore.Contains(c.Name)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && !FightingMe(c)
                        && AttackingTeam(c))
                    .OrderBy(c => c.MaxHealth)
                    .FirstOrDefault();

                if (mob != null)
                {
                    _singleTaunt = Time.NormalTime;
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = mob;
                    return true;
                }
            }

            if (SingleTauntsSelection.Target == (SingleTauntsSelection)_settings["SingleTauntsSelection"].AsInt32()
                && Time.NormalTime > _singleTaunt + SolTauntDelaySingle)
            {
                if (fightingTarget != null)
                {
                    _singleTaunt = Time.NormalTime;
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = fightingTarget;
                    return true;
                }
            }

            return false;
        }


        private bool SolDrainHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (DynelManager.LocalPlayer.FightingTarget == null || !CanCast(spell)) { return false; }

            if (DynelManager.LocalPlayer.HealthPercent <= 40) { return true; }

            return false;
        }

        private bool AugmentedMirrorShieldMKV(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (fightingtarget == null || !CanCast(spell)) { return false; }

            if (DynelManager.LocalPlayer.HealthPercent <= 85) { return true; }

            return false;
        }

        #endregion

        #region Misc

        private bool HeavyCompWeaponChecks(SimpleChar _target)
        {
            return GetWieldedWeapons(_target).HasFlag(CharacterWieldedWeapon.AssaultRifle)
                                || GetWieldedWeapons(_target).HasFlag(CharacterWieldedWeapon.Smg)
                                || GetWieldedWeapons(_target).HasFlag(CharacterWieldedWeapon.Shotgun)
                                || (GetWieldedWeapons(_target).HasFlag(CharacterWieldedWeapon.Grenade) && _target.Profession != Profession.Engineer);
        }

        public enum ReflectSelection
        {
            RubiKa, RubiKaTeam, Shadowlands
        }
        public enum RiotControlSelection
        {
            None, Self, Team
        }
        public enum AAOSelection
        {
            Self, Team
        }
        public enum InitBuffSelection
        {
            None, Self, Team
        }
        public enum CompHeavyArtSelection
        {
            None, Self, Team
        }
        public enum SingleTauntsSelection
        {
            None, Target, OS
        }

        public enum ProcType1Selection
        {
            FuriousAmmunition, TargetAcquired, Reconditioned, ConcussiveShot, EmergencyBandages, SuccessfulTargeting
        }

        public enum ProcType2Selection
        {
            FuseBodyArmor, OnTheDouble, GrazeJugularVein, GearAssaultAbsorption, DeepSixInitiative, ShootArtery
        }

        private static class RelevantNanos
        {
            public static readonly int[] SolDrainHeal = { 29241, 301897 };
            public static readonly int[] TauntBuffs = { 223209, 223207, 223205, 223203, 223201, 29242, 100207,
            29218, 100205, 100206, 100208, 29228};
            public const int HeavyComp = 269482;
            public const int Phalanx = 29245;
            public static readonly int[] ArBuffs = { 275027, 203119, 29220, 203121 };
        }

        private static class RelevantItems
        {
            public const int DreadlochEnduranceBooster = 267168;
            public const int DreadlochEnduranceBoosterNanomageEdition = 267167;
        }

        public static void SolTauntDelaySingle_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].SolTauntDelaySingle = e;
            SolTauntDelaySingle = e;
            Config.Save();
        }

        #endregion
    }
}
