﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using CombatHandler.Generic;
using System;
using System.Linq;

namespace CombatHandler.Enf
{
    class EnfCombatHandler : GenericCombatHandler
    {
        private static string PluginDirectory;

        private static bool ToggleBuffing = false;
        private static bool ToggleComposites = false;
        private static bool ToggleRez = false;

        private static Window _buffWindow;
        private static Window _tauntWindow;
        private static Window _procWindow;
        private static Window _itemWindow;
        private static Window _perkWindow;

        private static View _buffView;
        private static View _tauntView;
        private static View _procView;
        private static View _itemView;
        private static View _perkView;

        private static double _absorbs;
        private static double _challenger;
        private static double _rage;
        private static double _mongo;
        private static double _singleTaunt;

        private static double _ncuUpdateTime;

        public EnfCombatHandler(string pluginDir) : base(pluginDir)
        {
            IPCChannel.RegisterCallback((int)IPCOpcode.RemainingNCU, OnRemainingNCUMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalBuffing, OnGlobalBuffingMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalComposites, OnGlobalCompositesMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalRez, OnGlobalRezMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.ClearBuffs, OnClearBuffs);
            IPCChannel.RegisterCallback((int)IPCOpcode.Disband, OnDisband);

            Config.CharSettings[DynelManager.LocalPlayer.Name].BioCocoonPercentageChangedEvent += BioCocoonPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].SingleTauntDelayChangedEvent += SingleTauntDelay_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].MongoDelayChangedEvent += MongoDelay_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleAbsorbsDelayChangedEvent += CycleAbsorbsDelay_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleChallengerDelayChangedEvent += CycleChallengerDelay_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleRageDelayChangedEvent += CycleRageDelay_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].StimTargetNameChangedEvent += StimTargetName_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].StimHealthPercentageChangedEvent += StimHealthPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].StimNanoPercentageChangedEvent += StimNanoPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].KitHealthPercentageChangedEvent += KitHealthPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].KitNanoPercentageChangedEvent += KitNanoPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleSpherePerkDelayChangedEvent += CycleSpherePerkDelay_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleWitOfTheAtroxPerkDelayChangedEvent += CycleWitOfTheAtroxPerkDelay_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].SelfHealPerkPercentageChangedEvent += SelfHealPerkPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].SelfNanoPerkPercentageChangedEvent += SelfNanoPerkPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].TeamHealPerkPercentageChangedEvent += TeamHealPerkPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].TeamNanoPerkPercentageChangedEvent += TeamNanoPerkPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].DuckAbsorbsItemPercentageChangedEvent += DuckAbsorbsItemPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].BodyDevAbsorbsItemPercentageChangedEvent += BodyDevAbsorbsItemPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].StrengthAbsorbsItemPercentageChangedEvent += StrengthAbsorbsItemPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].BioRegrowthPercentageChangedEvent += BioRegrowthPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleBioRegrowthPerkDelayChangedEvent += CycleBioRegrowthPerkDelay_Changed;

            _settings.AddVariable("Buffing", true);
            _settings.AddVariable("Composites", true);

            _settings.AddVariable("GlobalBuffing", true);
            _settings.AddVariable("GlobalComposites", true);
            _settings.AddVariable("GlobalRez", true);

            _settings.AddVariable("Ost", false);

            _settings.AddVariable("SharpObjects", true);
            _settings.AddVariable("Grenades", true);

            _settings.AddVariable("StimTargetSelection", (int)StimTargetSelection.Self);

            _settings.AddVariable("Kits", true);

            _settings.AddVariable("ProcType1Selection", (int)ProcType1Selection.RagingBlow);
            _settings.AddVariable("ProcType2Selection", (int)ProcType2Selection.ViolationBuffer);

            _settings.AddVariable("SingleTauntsSelection", (int)SingleTauntsSelection.None);

            _settings.AddVariable("Mongo", true);
            _settings.AddVariable("CycleAbsorbs", false);
            _settings.AddVariable("SelfAbsorb", false);
            _settings.AddVariable("CycleChallenger", false);
            _settings.AddVariable("CycleRage", false);
            _settings.AddVariable("TauntProc", true);
            _settings.AddVariable("AbsorbACBuff", true);
            _settings.AddVariable("TargetedHpBuff", true);
            _settings.AddVariable("InitiativeBuffs", true);
            _settings.AddVariable("DamageShields", false);


            _settings.AddVariable("StrengthBuffSelection", (int)StrengthBuffSelection.None);

            _settings.AddVariable("TrollForm", false);
            _settings.AddVariable("EncaseInStone", false);
            _settings.AddVariable("DamagePerk", false);

            _settings.AddVariable("ScorpioTauntTool", false);

            RegisterSettingsWindow("Enforcer Handler", "EnforcerSettingsView.xml");

            //LE Procs
            RegisterPerkProcessor(PerkHash.LEProcEnforcerVortexOfHate, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerRagingBlow, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerShieldOfTheOgre, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerInspireRage, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerAirOfHatred, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerTearLigaments, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerVileRage, LEProc1, CombatActionPriority.Low);

            RegisterPerkProcessor(PerkHash.LEProcEnforcerViolationBuffer, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerInspireIre, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerShrugOffHits, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerBustKneecaps, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEnforcerIgnorePain, LEProc2, CombatActionPriority.Low);



            //Troll Form
            RegisterPerkProcessor(PerkHash.TrollForm, TrollForm);

            //Taunts
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MongoBuff).OrderByStackingOrder(), Mongo, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.SingleTargetTaunt, SingleTargetTaunt, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.EnforcerTauntProcs).OrderByStackingOrder(), TauntProc);

            //Buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.HPBuff).OrderByStackingOrder(), GlobalGenericBuff);
            RegisterSpellProcessor(RelevantNanos.FortifyBuffs, CycleAbsorbs, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Rage).OrderByStackingOrder(), CycleRage, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Challenger).OrderByStackingOrder(), CycleChallenger, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DamageShields).OrderByStackingOrder(), GlobalGenericBuff);

            //Weapon Buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.FastAttackBuffs).OrderByStackingOrder(), GlobalGenericBuff);
            RegisterSpellProcessor(RelevantNanos.Melee1HB, Melee1HBBuffWeapon);
            RegisterSpellProcessor(RelevantNanos.Melee1HE, Melee1HEBuffWeapon);
            RegisterSpellProcessor(RelevantNanos.Melee2HE, Melee2HEBuffWeapon);
            RegisterSpellProcessor(RelevantNanos.Melee2HB, Melee2HBBuffWeapon);
            RegisterSpellProcessor(RelevantNanos.MeleePierce, MeleePierceBuffWeapon);
            RegisterSpellProcessor(RelevantNanos.MeleeEnergy, MeleeEnergyBuffWeapon);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DamageChangeBuffs).OrderByStackingOrder(), DamageChange);

            //Team buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.InitiativeBuffs).OrderByStackingOrder(), InitiativeBuffs);
            RegisterSpellProcessor(RelevantNanos.TargetedDamageShields, DamageShields);
            RegisterSpellProcessor(RelevantNanos.TargetedHpBuff, TargetedHpBuff);
            RegisterSpellProcessor(RelevantNanos.AbsorbACBuff, AbsorbACBuff);
            RegisterSpellProcessor(RelevantNanos.ProdigiousStrength,
                (Spell buffSpell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                => GenericSelectionBuff(buffSpell, fightingTarget, ref actionTarget, "StrengthBuffSelection"));

            //Taunt Tools
            RegisterItemProcessor(244655, 244655, TauntTool);

            PluginDirectory = pluginDir;

            BioCocoonPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].BioCocoonPercentage;
            SingleTauntDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].SingleTauntDelay;
            MongoDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].MongoDelay;
            CycleAbsorbsDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].CycleAbsorbsDelay;
            CycleChallengerDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].CycleChallengerDelay;
            CycleRageDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].CycleRageDelay;
            StimTargetName = Config.CharSettings[DynelManager.LocalPlayer.Name].StimTargetName;
            StimHealthPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].StimHealthPercentage;
            StimNanoPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].StimNanoPercentage;
            KitHealthPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].KitHealthPercentage;
            KitNanoPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].KitNanoPercentage;
            CycleSpherePerkDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].CycleSpherePerkDelay;
            CycleWitOfTheAtroxPerkDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].CycleWitOfTheAtroxPerkDelay;
            SelfHealPerkPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].SelfHealPerkPercentage;
            SelfNanoPerkPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].SelfNanoPerkPercentage;
            TeamHealPerkPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].TeamHealPerkPercentage;
            TeamNanoPerkPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].TeamNanoPerkPercentage;
            DuckAbsorbsItemPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].DuckAbsorbsItemPercentage;
            BodyDevAbsorbsItemPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].BodyDevAbsorbsItemPercentage;
            StrengthAbsorbsItemPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].StrengthAbsorbsItemPercentage;
            BioRegrowthPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].BioRegrowthPercentage;
            CycleBioRegrowthPerkDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].CycleBioRegrowthPerkDelay;
        }

        public Window[] _windows => new Window[] { _buffWindow, _tauntWindow, _procWindow, _itemWindow, _perkWindow };

        #region Callbacks
        public static void OnRemainingNCUMessage(int sender, IPCMessage msg)
        {
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

        private void OnGlobalRezMessage(int sender, IPCMessage msg)
        {
            GlobalRezMessage rezMsg = (GlobalRezMessage)msg;

            if (DynelManager.LocalPlayer.Identity.Instance == sender) { return; }

            _settings[$"GlobalRez"] = rezMsg.Switch;
            _settings[$"GlobalRez"] = rezMsg.Switch;

        }

        #endregion

        #region Handles

        private void HandleBuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                _buffView = View.CreateFromXml(PluginDirectory + "\\UI\\EnforcerBuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Buffs", XmlViewName = "EnforcerBuffsView" }, _buffView);

                window.FindView("AbsorbsDelayBox", out TextInputView absorbsInput);
                window.FindView("ChallengerDelayBox", out TextInputView challengerInput);
                window.FindView("RageDelayBox", out TextInputView rageInput);

                if (absorbsInput != null)
                    absorbsInput.Text = $"{CycleAbsorbsDelay}";
                if (challengerInput != null)
                    challengerInput.Text = $"{CycleChallengerDelay}";
                if (rageInput != null)
                    rageInput.Text = $"{CycleRageDelay}";
            }
            else if (_buffWindow == null || (_buffWindow != null && !_buffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_buffWindow, PluginDir, new WindowOptions() { Name = "Buffs", XmlViewName = "EnforcerBuffsView" }, _buffView, out var container);
                _buffWindow = container;

                container.FindView("AbsorbsDelayBox", out TextInputView absorbsInput);
                container.FindView("ChallengerDelayBox", out TextInputView challengerInput);
                container.FindView("RageDelayBox", out TextInputView rageInput);

                if (absorbsInput != null)
                    absorbsInput.Text = $"{CycleAbsorbsDelay}";
                if (challengerInput != null)
                    challengerInput.Text = $"{CycleChallengerDelay}";
                if (rageInput != null)
                    rageInput.Text = $"{CycleRageDelay}";
            }
        }

        private void HandleTauntViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                _tauntView = View.CreateFromXml(PluginDirectory + "\\UI\\EnforcerTauntsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Taunts", XmlViewName = "EnforcerTauntsView" }, _tauntView);

                window.FindView("SingleTauntDelayBox", out TextInputView singleInput);
                window.FindView("MongoDelayBox", out TextInputView mongoInput);

                if (singleInput != null)
                    singleInput.Text = $"{SingleTauntDelay}";
                if (mongoInput != null)
                    mongoInput.Text = $"{MongoDelay}";
            }
            else if (_tauntWindow == null || (_tauntWindow != null && !_tauntWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_tauntWindow, PluginDir, new WindowOptions() { Name = "Taunts", XmlViewName = "EnforcerTauntsView" }, _tauntView, out var container);
                _tauntWindow = container;

                container.FindView("SingleTauntDelayBox", out TextInputView singleInput);
                container.FindView("MongoDelayBox", out TextInputView mongoInput);

                if (singleInput != null)
                    singleInput.Text = $"{SingleTauntDelay}";
                if (mongoInput != null)
                    mongoInput.Text = $"{MongoDelay}";
            }
        }
        private void HandleItemViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                if (window.Views.Contains(_itemView)) { return; }

                _itemView = View.CreateFromXml(PluginDirectory + "\\UI\\EnforcerItemsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Items", XmlViewName = "EnforcerItemsView" }, _itemView);

                window.FindView("StimTargetBox", out TextInputView stimTargetInput);
                window.FindView("StimHealthPercentageBox", out TextInputView stimHealthInput);
                window.FindView("StimNanoPercentageBox", out TextInputView stimNanoInput);
                window.FindView("KitHealthPercentageBox", out TextInputView kitHealthInput);
                window.FindView("KitNanoPercentageBox", out TextInputView kitNanoInput);
                window.FindView("DuckAbsorbsItemPercentageBox", out TextInputView duckInput);
                window.FindView("BodyDevAbsorbsItemPercentageBox", out TextInputView bodyDevInput);
                window.FindView("StrengthAbsorbsItemPercentageBox", out TextInputView strengthInput);

                if (stimTargetInput != null)
                    stimTargetInput.Text = $"{StimTargetName}";
                if (stimHealthInput != null)
                    stimHealthInput.Text = $"{StimHealthPercentage}";
                if (stimNanoInput != null)
                    stimNanoInput.Text = $"{StimNanoPercentage}";
                if (kitHealthInput != null)
                    kitHealthInput.Text = $"{KitHealthPercentage}";
                if (kitNanoInput != null)
                    kitNanoInput.Text = $"{KitNanoPercentage}";
                if (duckInput != null)
                    duckInput.Text = $"{DuckAbsorbsItemPercentage}";
                if (bodyDevInput != null)
                    bodyDevInput.Text = $"{BodyDevAbsorbsItemPercentage}";
                if (strengthInput != null)
                    strengthInput.Text = $"{StrengthAbsorbsItemPercentage}";
            }
            else if (_itemWindow == null || (_itemWindow != null && !_itemWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_itemWindow, PluginDir, new WindowOptions() { Name = "Items", XmlViewName = "EnforcerItemsView" }, _itemView, out var container);
                _itemWindow = container;

                container.FindView("StimTargetBox", out TextInputView stimTargetInput);
                container.FindView("StimHealthPercentageBox", out TextInputView stimHealthInput);
                container.FindView("StimNanoPercentageBox", out TextInputView stimNanoInput);
                container.FindView("KitHealthPercentageBox", out TextInputView kitHealthInput);
                container.FindView("KitNanoPercentageBox", out TextInputView kitNanoInput);
                container.FindView("DuckAbsorbsItemPercentageBox", out TextInputView duckInput);
                container.FindView("BodyDevAbsorbsItemPercentageBox", out TextInputView bodyDevInput);
                container.FindView("StrengthAbsorbsItemPercentageBox", out TextInputView strengthInput);

                if (stimTargetInput != null)
                    stimTargetInput.Text = $"{StimTargetName}";
                if (stimHealthInput != null)
                    stimHealthInput.Text = $"{StimHealthPercentage}";
                if (stimNanoInput != null)
                    stimNanoInput.Text = $"{StimNanoPercentage}";
                if (kitHealthInput != null)
                    kitHealthInput.Text = $"{KitHealthPercentage}";
                if (kitNanoInput != null)
                    kitNanoInput.Text = $"{KitNanoPercentage}";
                if (duckInput != null)
                    duckInput.Text = $"{DuckAbsorbsItemPercentage}";
                if (bodyDevInput != null)
                    bodyDevInput.Text = $"{BodyDevAbsorbsItemPercentage}";
                if (strengthInput != null)
                    strengthInput.Text = $"{StrengthAbsorbsItemPercentage}";
            }
        }

        private void HandleProcViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                if (window.Views.Contains(_procView)) { return; }

                _procView = View.CreateFromXml(PluginDirectory + "\\UI\\EnforcerProcsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Procs", XmlViewName = "EnforcerProcsView" }, _procView);
            }
            else if (_procWindow == null || (_procWindow != null && !_procWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_procWindow, PluginDir, new WindowOptions() { Name = "Procs", XmlViewName = "EnforcerProcsView" }, _procView, out var container);
                _procWindow = container;
            }
        }
        private void HandlePerkViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                if (window.Views.Contains(_perkView)) { return; }

                _perkView = View.CreateFromXml(PluginDirectory + "\\UI\\EnforcerPerksView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Perks", XmlViewName = "EnforcerPerksView" }, _perkView);

                window.FindView("BioCocoonPercentageBox", out TextInputView bioCocoonInput);
                window.FindView("SphereDelayBox", out TextInputView sphereInput);
                window.FindView("WitDelayBox", out TextInputView witOfTheAtroxInput);
                window.FindView("SelfHealPercentageBox", out TextInputView selfHealInput);
                window.FindView("SelfNanoPercentageBox", out TextInputView selfNanoInput);
                window.FindView("TeamHealPercentageBox", out TextInputView teamHealInput);
                window.FindView("TeamNanoPercentageBox", out TextInputView teamNanoInput);
                window.FindView("BioRegrowthPercentageBox", out TextInputView bioRegrowthPercentageInput);
                window.FindView("BioRegrowthDelayBox", out TextInputView bioRegrowthDelayInput);

                if (bioCocoonInput != null)
                    bioCocoonInput.Text = $"{BioCocoonPercentage}";
                if (sphereInput != null)
                    sphereInput.Text = $"{CycleSpherePerkDelay}";
                if (witOfTheAtroxInput != null)
                    witOfTheAtroxInput.Text = $"{CycleWitOfTheAtroxPerkDelay}";
                if (selfHealInput != null)
                    selfHealInput.Text = $"{SelfHealPerkPercentage}";
                if (selfNanoInput != null)
                    selfNanoInput.Text = $"{SelfNanoPerkPercentage}";
                if (teamHealInput != null)
                    teamHealInput.Text = $"{TeamHealPerkPercentage}";
                if (teamNanoInput != null)
                    teamNanoInput.Text = $"{TeamNanoPerkPercentage}";
                if (bioRegrowthPercentageInput != null)
                    bioRegrowthPercentageInput.Text = $"{BioRegrowthPercentage}";
                if (bioRegrowthDelayInput != null)
                    bioRegrowthDelayInput.Text = $"{CycleBioRegrowthPerkDelay}";
            }
            else if (_perkWindow == null || (_perkWindow != null && !_perkWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_perkWindow, PluginDir, new WindowOptions() { Name = "Perks", XmlViewName = "EnforcerPerksView" }, _perkView, out var container);
                _perkWindow = container;

                container.FindView("BioCocoonPercentageBox", out TextInputView bioCocoonInput);
                container.FindView("SphereDelayBox", out TextInputView sphereInput);
                container.FindView("WitDelayBox", out TextInputView witOfTheAtroxInput);
                container.FindView("SelfHealPercentageBox", out TextInputView selfHealInput);
                container.FindView("SelfNanoPercentageBox", out TextInputView selfNanoInput);
                container.FindView("TeamHealPercentageBox", out TextInputView teamHealInput);
                container.FindView("TeamNanoPercentageBox", out TextInputView teamNanoInput);
                container.FindView("BioRegrowthPercentageBox", out TextInputView bioRegrowthPercentageInput);
                container.FindView("BioRegrowthDelayBox", out TextInputView bioRegrowthDelayInput);

                if (bioCocoonInput != null)
                    bioCocoonInput.Text = $"{BioCocoonPercentage}";
                if (sphereInput != null)
                    sphereInput.Text = $"{CycleSpherePerkDelay}";
                if (witOfTheAtroxInput != null)
                    witOfTheAtroxInput.Text = $"{CycleWitOfTheAtroxPerkDelay}";
                if (selfHealInput != null)
                    selfHealInput.Text = $"{SelfHealPerkPercentage}";
                if (selfNanoInput != null)
                    selfNanoInput.Text = $"{SelfNanoPerkPercentage}";
                if (teamHealInput != null)
                    teamHealInput.Text = $"{TeamHealPerkPercentage}";
                if (teamNanoInput != null)
                    teamNanoInput.Text = $"{TeamNanoPerkPercentage}";
                if (bioRegrowthPercentageInput != null)
                    bioRegrowthPercentageInput.Text = $"{BioRegrowthPercentage}";
                if (bioRegrowthDelayInput != null)
                    bioRegrowthDelayInput.Text = $"{CycleBioRegrowthPerkDelay}";
            }
        }

        #endregion

        protected override void OnUpdate(float deltaTime)
        {
            if (Game.IsZoning || Time.NormalTime < _lastZonedTime + 0.5)
                return;

            base.OnUpdate(deltaTime);

            if (Time.NormalTime > _ncuUpdateTime + 1.0f)
            {
                RemainingNCUMessage ncuMessage = RemainingNCUMessage.ForLocalPlayer();

                IPCChannel.Broadcast(ncuMessage);

                OnRemainingNCUMessage(0, ncuMessage);

                _ncuUpdateTime = Time.NormalTime;
            }

            #region UI

            var window = SettingsController.FindValidWindow(_windows);

            if (window != null && window.IsValid)
            {
                window.FindView("SingleTauntDelayBox", out TextInputView singleInput);
                window.FindView("MongoDelayBox", out TextInputView mongoInput);
                window.FindView("AbsorbsDelayBox", out TextInputView absorbsInput);
                window.FindView("ChallengerDelayBox", out TextInputView challengerInput);
                window.FindView("RageDelayBox", out TextInputView rageInput);
                window.FindView("BioCocoonPercentageBox", out TextInputView bioCocoonInput);
                window.FindView("StimTargetBox", out TextInputView stimTargetInput);
                window.FindView("StimHealthPercentageBox", out TextInputView stimHealthInput);
                window.FindView("StimNanoPercentageBox", out TextInputView stimNanoInput);
                window.FindView("KitHealthPercentageBox", out TextInputView kitHealthInput);
                window.FindView("KitNanoPercentageBox", out TextInputView kitNanoInput);
                window.FindView("SphereDelayBox", out TextInputView sphereInput);
                window.FindView("WitDelayBox", out TextInputView witOfTheAtroxInput);
                window.FindView("SelfHealPercentageBox", out TextInputView selfHealInput);
                window.FindView("SelfNanoPercentageBox", out TextInputView selfNanoInput);
                window.FindView("TeamHealPercentageBox", out TextInputView teamHealInput);
                window.FindView("TeamNanoPercentageBox", out TextInputView teamNanoInput);
                window.FindView("DuckAbsorbsItemPercentageBox", out TextInputView duckInput);
                window.FindView("BodyDevAbsorbsItemPercentageBox", out TextInputView bodyDevInput);
                window.FindView("StrengthAbsorbsItemPercentageBox", out TextInputView strengthInput);
                window.FindView("BioRegrowthPercentageBox", out TextInputView bioRegrowthPercentageInput);
                window.FindView("BioRegrowthDelayBox", out TextInputView bioRegrowthDelayInput);

                if (bioCocoonInput != null && !string.IsNullOrEmpty(bioCocoonInput.Text))
                    if (int.TryParse(bioCocoonInput.Text, out int bioCocoonValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].BioCocoonPercentage != bioCocoonValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].BioCocoonPercentage = bioCocoonValue;

                if (singleInput != null && !string.IsNullOrEmpty(singleInput.Text))
                    if (int.TryParse(singleInput.Text, out int singleValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].SingleTauntDelay != singleValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].SingleTauntDelay = singleValue;

                if (mongoInput != null && !string.IsNullOrEmpty(mongoInput.Text))
                    if (int.TryParse(mongoInput.Text, out int mongoValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].MongoDelay != mongoValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].MongoDelay = mongoValue;

                if (absorbsInput != null && !string.IsNullOrEmpty(absorbsInput.Text))
                    if (int.TryParse(absorbsInput.Text, out int absorbsValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].CycleAbsorbsDelay != absorbsValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleAbsorbsDelay = absorbsValue;

                if (challengerInput != null && !string.IsNullOrEmpty(challengerInput.Text))
                    if (int.TryParse(challengerInput.Text, out int challengerValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].CycleChallengerDelay != challengerValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleChallengerDelay = challengerValue;

                if (rageInput != null && !string.IsNullOrEmpty(rageInput.Text))
                    if (int.TryParse(rageInput.Text, out int rageValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].CycleRageDelay != rageValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleRageDelay = rageValue;

                if (stimTargetInput != null)
                    if (Config.CharSettings[DynelManager.LocalPlayer.Name].StimTargetName != stimTargetInput.Text)
                        Config.CharSettings[DynelManager.LocalPlayer.Name].StimTargetName = stimTargetInput.Text;

                if (stimHealthInput != null && !string.IsNullOrEmpty(stimHealthInput.Text))
                    if (int.TryParse(stimHealthInput.Text, out int stimHealthValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].StimHealthPercentage != stimHealthValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].StimHealthPercentage = stimHealthValue;

                if (stimNanoInput != null && !string.IsNullOrEmpty(stimNanoInput.Text))
                    if (int.TryParse(stimNanoInput.Text, out int stimNanoValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].StimNanoPercentage != stimNanoValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].StimNanoPercentage = stimNanoValue;

                if (kitHealthInput != null && !string.IsNullOrEmpty(kitHealthInput.Text))
                    if (int.TryParse(kitHealthInput.Text, out int kitHealthValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].KitHealthPercentage != kitHealthValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].KitHealthPercentage = kitHealthValue;

                if (kitNanoInput != null && !string.IsNullOrEmpty(kitNanoInput.Text))
                    if (int.TryParse(kitNanoInput.Text, out int kitNanoValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].KitNanoPercentage != kitNanoValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].KitNanoPercentage = kitNanoValue;

                if (sphereInput != null && !string.IsNullOrEmpty(sphereInput.Text))
                    if (int.TryParse(sphereInput.Text, out int sphereValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].CycleSpherePerkDelay != sphereValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleSpherePerkDelay = sphereValue;

                if (witOfTheAtroxInput != null && !string.IsNullOrEmpty(witOfTheAtroxInput.Text))
                    if (int.TryParse(witOfTheAtroxInput.Text, out int witOfTheAtroxValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].CycleWitOfTheAtroxPerkDelay != witOfTheAtroxValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleWitOfTheAtroxPerkDelay = witOfTheAtroxValue;

                if (selfHealInput != null && !string.IsNullOrEmpty(selfHealInput.Text))
                    if (int.TryParse(selfHealInput.Text, out int selfHealValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].SelfHealPerkPercentage != selfHealValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].SelfHealPerkPercentage = selfHealValue;

                if (selfNanoInput != null && !string.IsNullOrEmpty(selfNanoInput.Text))
                    if (int.TryParse(selfNanoInput.Text, out int selfNanoValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].SelfNanoPerkPercentage != selfNanoValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].SelfNanoPerkPercentage = selfNanoValue;

                if (teamHealInput != null && !string.IsNullOrEmpty(teamHealInput.Text))
                    if (int.TryParse(teamHealInput.Text, out int teamHealValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].TeamHealPerkPercentage != teamHealValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].TeamHealPerkPercentage = teamHealValue;

                if (teamNanoInput != null && !string.IsNullOrEmpty(teamNanoInput.Text))
                    if (int.TryParse(teamNanoInput.Text, out int teamNanoValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].TeamNanoPerkPercentage != teamNanoValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].TeamNanoPerkPercentage = teamNanoValue;

                if (duckInput != null && !string.IsNullOrEmpty(duckInput.Text))
                    if (int.TryParse(duckInput.Text, out int duckValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].DuckAbsorbsItemPercentage != duckValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].DuckAbsorbsItemPercentage = duckValue;

                if (bodyDevInput != null && !string.IsNullOrEmpty(bodyDevInput.Text))
                    if (int.TryParse(bodyDevInput.Text, out int bodyDevValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].BodyDevAbsorbsItemPercentage != bodyDevValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].BodyDevAbsorbsItemPercentage = bodyDevValue;

                if (strengthInput != null && !string.IsNullOrEmpty(strengthInput.Text))
                    if (int.TryParse(strengthInput.Text, out int strengthValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].StrengthAbsorbsItemPercentage != strengthValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].StrengthAbsorbsItemPercentage = strengthValue;

                if (bioRegrowthPercentageInput != null && !string.IsNullOrEmpty(bioRegrowthPercentageInput.Text))
                    if (int.TryParse(bioRegrowthPercentageInput.Text, out int bioRegrowthPercentageValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].BioRegrowthPercentage != bioRegrowthPercentageValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].BioRegrowthPercentage = bioRegrowthPercentageValue;

                if (bioRegrowthDelayInput != null && !string.IsNullOrEmpty(bioRegrowthDelayInput.Text))
                    if (int.TryParse(bioRegrowthDelayInput.Text, out int bioRegrowthDelayValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].CycleBioRegrowthPerkDelay != bioRegrowthDelayValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleBioRegrowthPerkDelay = bioRegrowthDelayValue;
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

                if (SettingsController.settingsWindow.FindView("BuffsView", out Button buffView))
                {
                    buffView.Tag = SettingsController.settingsWindow;
                    buffView.Clicked = HandleBuffViewClick;
                }

                if (SettingsController.settingsWindow.FindView("TauntsView", out Button tauntView))
                {
                    tauntView.Tag = SettingsController.settingsWindow;
                    tauntView.Clicked = HandleTauntViewClick;
                }

                if (SettingsController.settingsWindow.FindView("ProcsView", out Button procView))
                {
                    procView.Tag = SettingsController.settingsWindow;
                    procView.Clicked = HandleProcViewClick;
                }

                #endregion

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

                #region Global Resurrection

                if (!_settings["GlobalRez"].AsBool() && ToggleRez)
                {
                    IPCChannel.Broadcast(new GlobalRezMessage()
                    {

                        Switch = false
                    });

                    ToggleRez = false;
                    _settings["GlobalRez"] = false;
                }
                if (_settings["GlobalRez"].AsBool() && !ToggleRez)
                {
                    IPCChannel.Broadcast(new GlobalRezMessage()
                    {
                        Switch = true
                    });

                    ToggleRez = true;
                    _settings["GlobalRez"] = true;
                }

                #endregion
            }
        }


        #region LE Procs

        protected bool LEProc1(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (perk.Hash != ((PerkHash)_settings["ProcType1Selection"].AsInt32()))
                return false;

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        protected bool LEProc2(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (perk.Hash != ((PerkHash)_settings["ProcType2Selection"].AsInt32()))
                return false;

            return LEProc(perk, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Taunts

        private bool SingleTargetTaunt(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing") || !CanCast(spell)) { return false; }

            if (SingleTauntsSelection.Area == (SingleTauntsSelection)_settings["SingleTauntsSelection"].AsInt32()
                && Time.NormalTime > _singleTaunt + SingleTauntDelay)
            {
                SimpleChar mob = DynelManager.NPCs
                    .Where(c => c.IsAttacking && c.FightingTarget != null
                        && c.FightingTarget?.Profession != Profession.Enforcer
                        && c.IsInLineOfSight
                        && !debuffAreaTargetsToIgnore.Contains(c.Name)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.FightingTarget?.Identity != DynelManager.LocalPlayer.Identity
                        && c.Name != "Alien Heavy Patroller"
                        && AttackingTeam(c))
                    .OrderBy(c => c.MaxHealth)
                    .FirstOrDefault();

                if (mob != null && DynelManager.LocalPlayer.HealthPercent >= 30)
                {
                    _singleTaunt = Time.NormalTime;
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = mob;
                    return true;
                }
                else if (fightingTarget != null && DynelManager.LocalPlayer.HealthPercent >= 30)
                {
                    _singleTaunt = Time.NormalTime;
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = fightingTarget;
                    return true;
                }
            }

            if (SingleTauntsSelection.Target == (SingleTauntsSelection)_settings["SingleTauntsSelection"].AsInt32()
                && Time.NormalTime > _singleTaunt + SingleTauntDelay)
            {
                if (fightingTarget != null
                    && DynelManager.LocalPlayer.HealthPercent >= 30)
                {
                    _singleTaunt = Time.NormalTime;
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = fightingTarget;
                    return true;
                }
            }

            return false;
        }

        private bool Mongo(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing") || !IsSettingEnabled("Mongo") || !CanCast(spell)) { return false; }

            if (DynelManager.NPCs.Any(c => c.Health > 0
                && c.Name == "Alien Heavy Patroller"
                && c.Position.DistanceFrom(DynelManager.LocalPlayer.Position) <= 20f)) { return false; }

            if (Time.NormalTime > _mongo + MongoDelay
            && fightingTarget != null && DynelManager.LocalPlayer.HealthPercent >= 30)
            {
                _mongo = Time.NormalTime;
                return true;
            }
            return false;
        }

        private bool TauntProc(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("TauntProc")) { return false; }

            return Buff(spell, spell.Nanoline, ref actionTarget);
        }

        #endregion

        #region Perks

        private bool TrollForm(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("TrollForm") || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            return CyclePerks(perk, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Buffs

        private bool CycleAbsorbs(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsSettingEnabled("SelfAbsorb"))
            {
                if (DynelManager.LocalPlayer.Buffs.Any(Buff => Buff.Id == RelevantNanos.BioCocoon)) { return false; }

                if (IsSettingEnabled("CycleAbsorbs") && Time.NormalTime > _absorbs + CycleAbsorbsDelay
                    && (fightingTarget != null || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) > 0))
                {
                    if (!IsSettingEnabled("Buffing") || !CanCast(spell)) { return false; }

                    _absorbs = Time.NormalTime;
                    return true;
                }

                return Buff(spell, spell.Nanoline, ref actionTarget);
            }

            return false;
        }

        private bool CycleRage(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsSettingEnabled("CycleRage") && Time.NormalTime > _rage + CycleRageDelay
                && (fightingTarget != null || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) > 0))
            {
                if (!IsSettingEnabled("Buffing") || !CanCast(spell)) { return false; }

                _rage = Time.NormalTime;
                return true;
            }

            if (!DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root) || !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Snare)) { return false; }

            return true;
        }

        private bool CycleChallenger(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsSettingEnabled("CycleChallenger") && Time.NormalTime > _challenger + CycleChallengerDelay
                && (fightingTarget != null || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) > 0))
            {
                if (!IsSettingEnabled("Buffing") || !CanCast(spell)) { return false; }

                _challenger = Time.NormalTime;
                return true;
            }

            return false;
        }

        #region Weapon Buffs

        private bool Melee1HBBuffWeapon(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Blunt1H);
        }

        private bool Melee1HEBuffWeapon(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Edged1H);
        }

        private bool Melee2HEBuffWeapon(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Edged2H);
        }

        private bool Melee2HBBuffWeapon(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Blunt2H);
        }

        private bool MeleePierceBuffWeapon(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Piercing);
        }

        private bool MeleeEnergyBuffWeapon(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.MeleeEnergy);
        }

        //private bool DamageChange(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        //{
        //    return Buff(spell, NanoLine.DamageChangeBuffs, ref actionTarget);
        //}

        private bool DamageChange(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if there is a fighting target or if the spell ID should be ignored
            if (DynelManager.LocalPlayer.FightingTarget != null || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id))
            {
                return false;
            }

            // Check if the "Buffing" setting is enabled and if the spell can be cast, and if the ModelIdentity.Instance is not 152
            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || Playfield.ModelIdentity.Instance == 152)
            {
                return false;
            }

            // Check if NanoLine.DamageChangeBuffs is in the player's buffs
            bool hasDamageChangeBuffs = DynelManager.LocalPlayer.Buffs.Contains(NanoLine.DamageChangeBuffs);

            // Check if there is enough remaining NCU to cast the spell
            if (DynelManager.LocalPlayer.RemainingNCU >= Math.Abs(spell.NCU))
            {
                // If NanoLine.DamageChangeBuffs is NOT in the player's buffs, cast the spell
                if (!hasDamageChangeBuffs)
                {
                    // Set the actionTarget to the local player and mark ShouldSetTarget as true
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = DynelManager.LocalPlayer;
                    return true;
                }
            }

            return false;
        }


        #endregion

        #endregion

        #region Team Buffs

        private bool InitiativeBuffs(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsSettingEnabled("InitiativeBuffs"))
                return TeamBuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Melee);

            return Buff(spell, spell.Nanoline, ref actionTarget);
        }

        private bool DamageShields(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("DamageShields")) { return false; }

            return GenericTeamBuff(spell, ref actionTarget);
        }

        private bool TargetedHpBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("TargetedHpBuff")) { return false; }

            return GenericTeamBuff(spell, ref actionTarget);
        }

        private bool AbsorbACBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("AbsorbACBuff")) { return false; }

            return GenericTeamBuff(spell, ref actionTarget);
        }

        #endregion

        #region Misc

        private static class RelevantNanos
        {
            public static readonly int[] SingleTargetTaunt = { 275014, 223123, 223121, 223119, 223117, 223115, 100209, 100210, 100212, 100211, 100213 };
            public static readonly int[] Melee1HB = { 202846, 202844, 202842, 29630, 202840, 29644 };
            public static readonly int[] Melee2HB = { 202856, 202854, 202852, 29630, 202850, 29644, 202848 };
            public static readonly int[] Melee1HE = { 202818, 202816, 202793, 202791, 202774, 202739, 202776 };
            public static readonly int[] Melee2HE = { 202838, 202836, 202834, 202832, 202830, 202828, 202826 };
            public static readonly int[] MeleePierce = { 202858, 202860, 202862, 202864, 202866, 202868, 202870 };
            public static readonly int[] MeleeEnergy = { 203215, 203207, 203209, 203211, 203213 };
            public static readonly int[] TargetedHpBuff = { 273629, 95708, 95700, 95701, 95702, 95704, 95706, 95707 };
            public static readonly int[] FortifyBuffs = { 273320, 270350, 117686, 117688, 117682, 117687, 117685, 117684, 117683, 117680, 117681 };
            public static readonly int[] AbsorbACBuff = { 270350, 117686, 117688, 117682, 117687, 117685, 117684, 117683, 117680, 117681 };
            public static readonly Spell[] TargetedDamageShields = Spell.GetSpellsForNanoline(NanoLine.DamageShields).OrderByStackingOrder().Where(spell => spell.Id != ICE_BURN).ToArray();
            public const int MONGO_KRAKEN = 273322;
            public const int MONGO_DEMOLISH = 270786;
            public const int FOCUSED_ANGER = 29641;
            public const int IMPROVED_ESSENCE_OF_BEHEMOTH = 273629;
            public const int CORUSCATING_SCREEN = 55751;
            public const int ICE_BURN = 269460;
            public const int ProdigiousStrength = 29652;
            public const int BioCocoon = 209802;
        }

        public enum ProcType1Selection
        {
            VortexofHate = 1112689735,
            RagingBlow = 1380532823,
            Shieldoftheogre = 1229867077,
            InspireRage = 1230197319,
            Airofhatred = 1413827655,
            TearLigaments = 1111577427,
            VileRage = 1230195026
        }

        public enum ProcType2Selection
        {
            ViolationBuffer = 1112886085,
            InspireIre = 1112754266,
            ShrugOffHits = 1447973709,
            BustKneecaps = 1480807238,
            IgnorePain = 1229410377
        }

        public enum SingleTauntsSelection
        {
            None, Target, Area
        }

        public enum StrengthBuffSelection
        {
            None, Self, Team
        }
        #endregion
    }
}