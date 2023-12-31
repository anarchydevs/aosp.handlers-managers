﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using CombatHandler.Generic;
using System.Linq;

namespace CombatHandler.Keeper
{
    public class KeeperCombatHandler : GenericCombatHandler
    {
        private static string PluginDirectory;

        private static bool ToggleBuffing = false;
        private static bool ToggleComposites = false;
        private static bool ToggleRez = false;

        private static Window _buffWindow;
        private static Window _procWindow;
        private static Window _itemWindow;
        private static Window _perkWindow;

        private static View _buffView;
        private static View _procView;
        private static View _itemView;
        private static View _perkView;

        private static double _ncuUpdateTime;

        public KeeperCombatHandler(string pluginDir) : base(pluginDir)
        {
            IPCChannel.RegisterCallback((int)IPCOpcode.RemainingNCU, OnRemainingNCUMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalBuffing, OnGlobalBuffingMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalComposites, OnGlobalCompositesMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalRez, OnGlobalRezMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.ClearBuffs, OnClearBuffs);
            IPCChannel.RegisterCallback((int)IPCOpcode.Disband, OnDisband);

            Config.CharSettings[DynelManager.LocalPlayer.Name].BioCocoonPercentageChangedEvent += BioCocoonPercentage_Changed;
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
            Config.CharSettings[DynelManager.LocalPlayer.Name].BodyDevAbsorbsItemPercentageChangedEvent += BodyDevAbsorbsItemPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].StrengthAbsorbsItemPercentageChangedEvent += StrengthAbsorbsItemPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].BioRegrowthPercentageChangedEvent += BioRegrowthPercentage_Changed;
            Config.CharSettings[DynelManager.LocalPlayer.Name].CycleBioRegrowthPerkDelayChangedEvent += CycleBioRegrowthPerkDelay_Changed;

            _settings.AddVariable("Buffing", true);
            _settings.AddVariable("Composites", true);

            _settings.AddVariable("GlobalBuffing", true);
            _settings.AddVariable("GlobalComposites", true);
            _settings.AddVariable("GlobalRez", true);

            _settings.AddVariable("EncaseInStone", false);

            _settings.AddVariable("AAOBuffs", true);

            _settings.AddVariable("SharpObjects", false);
            _settings.AddVariable("Grenades", false);

            _settings.AddVariable("ScorpioTauntTool", false);

            _settings.AddVariable("StimTargetSelection", (int)StimTargetSelection.None);

            _settings.AddVariable("Kits", true);

            _settings.AddVariable("RecastAntiFear", false);
            _settings.AddVariable("SLMap", false);

            //LE Proc
            _settings.AddVariable("ProcType1Selection", (int)ProcType1Selection.RighteousSmite);
            _settings.AddVariable("ProcType2Selection", (int)ProcType2Selection.HonorRestored);

            //Auras
            _settings.AddVariable("AuraSet1Selection", (int)AuraSet1Selection.Heal);
            _settings.AddVariable("AuraSet2Selection", (int)AuraSet2Selection.Damage);
            _settings.AddVariable("AuraSet3Selection", (int)AuraSet3Selection.AAO);
            _settings.AddVariable("AuraSet4Selection", (int)AuraSet4Selection.Sanc);

            RegisterSettingsWindow("Keeper Handler", "KeeperSettingsView.xml");

            RegisterPerkProcessors();

            //Anti-Fear
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.KeeperFearImmunity).OrderByStackingOrder(), RecastAntiFear);
            //RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.KeeperFearImmunity).OrderByStackingOrder(),
            //    (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
            //        => NonCombatBuff(spell, ref actionTarget, fightingTarget, "RecastAntiFear"));

            //Taunt Tools
            RegisterItemProcessor(244655, 244655, TauntTool);

            //Buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Fortify).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine._2HEdgedBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Fury).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.KeeperDeflect_RiposteBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.FastAttackBuffs).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.KeeperEvade_Dodge_DuckBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.KeeperStr_Stam_AgiBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));

            //Auras
            RegisterSpellProcessor(RelevantNanos.HealAuras, HpAura);
            RegisterSpellProcessor(RelevantNanos.NanoAuras, NpAura);
            RegisterSpellProcessor(RelevantNanos.ReflectAuras, BarrierAura);
            RegisterSpellProcessor(RelevantNanos.AAOAuras, ImminenceAura);
            RegisterSpellProcessor(RelevantNanos.DerootAuras, EnervateAura);
            RegisterSpellProcessor(RelevantNanos.DamageAuras, VengeanceAura);
            RegisterSpellProcessor(RelevantNanos.SancAuras, SanctifierAura);
            RegisterSpellProcessor(RelevantNanos.ReaperAuras, ReaperAura);

            //Team Buffs
            RegisterSpellProcessor(RelevantNanos.PunisherOfTheWicked,
            (Spell buffSpell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                => NonComabtTeamBuff(buffSpell, fightingTarget, ref actionTarget, "AAOBuffs"));

            //LE Proc
            RegisterPerkProcessor(PerkHash.LEProcKeeperRighteousSmite, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperSymbioticBypass, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperVirtuousReaper, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperIgnoreTheUnrepentant, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperPureStrike, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperEschewTheFaithless, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperRighteousStrike, LEProc1, CombatActionPriority.Low);

            RegisterPerkProcessor(PerkHash.LEProcKeeperHonorRestored, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperAmbientPurification, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperBenevolentBarrier, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperSubjugation, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcKeeperFaithfulReconstruction, LEProc2, CombatActionPriority.Low);

            PluginDirectory = pluginDir;

            BioCocoonPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].BioCocoonPercentage;
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
            BodyDevAbsorbsItemPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].BodyDevAbsorbsItemPercentage;
            StrengthAbsorbsItemPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].StrengthAbsorbsItemPercentage;
            BioRegrowthPercentage = Config.CharSettings[DynelManager.LocalPlayer.Name].BioRegrowthPercentage;
            CycleBioRegrowthPerkDelay = Config.CharSettings[DynelManager.LocalPlayer.Name].CycleBioRegrowthPerkDelay;
        }
        public Window[] _windows => new Window[] { _buffWindow, _procWindow, _itemWindow, _perkWindow };

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
        private void HandleItemViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                if (window.Views.Contains(_itemView)) { return; }

                _itemView = View.CreateFromXml(PluginDirectory + "\\UI\\KeeperItemsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Items", XmlViewName = "KeeperItemsView" }, _itemView);

                window.FindView("StimTargetBox", out TextInputView stimTargetInput);
                window.FindView("StimHealthPercentageBox", out TextInputView stimHealthInput);
                window.FindView("StimNanoPercentageBox", out TextInputView stimNanoInput);
                window.FindView("KitHealthPercentageBox", out TextInputView kitHealthInput);
                window.FindView("KitNanoPercentageBox", out TextInputView kitNanoInput);
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
                if (bodyDevInput != null)
                    bodyDevInput.Text = $"{BodyDevAbsorbsItemPercentage}";
                if (strengthInput != null)
                    strengthInput.Text = $"{StrengthAbsorbsItemPercentage}";
            }
            else if (_itemWindow == null || (_itemWindow != null && !_itemWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_itemWindow, PluginDir, new WindowOptions() { Name = "Items", XmlViewName = "KeeperItemsView" }, _itemView, out var container);
                _itemWindow = container;

                container.FindView("StimTargetBox", out TextInputView stimTargetInput);
                container.FindView("StimHealthPercentageBox", out TextInputView stimHealthInput);
                container.FindView("StimNanoPercentageBox", out TextInputView stimNanoInput);
                container.FindView("KitHealthPercentageBox", out TextInputView kitHealthInput);
                container.FindView("KitNanoPercentageBox", out TextInputView kitNanoInput);
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
                if (window.Views.Contains(_procView)) { return; }

                _procView = View.CreateFromXml(PluginDirectory + "\\UI\\KeeperProcsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Procs", XmlViewName = "KeeperProcsView" }, _procView);
            }
            else if (_procWindow == null || (_procWindow != null && !_procWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_procWindow, PluginDir, new WindowOptions() { Name = "Procs", XmlViewName = "KeeperProcsView" }, _procView, out var container);
                _procWindow = container;
            }
        }
        private void HandlePerkViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                if (window.Views.Contains(_perkView)) { return; }

                _perkView = View.CreateFromXml(PluginDirectory + "\\UI\\KeeperPerksView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Perks", XmlViewName = "KeeperPerksView" }, _perkView);

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
                SettingsController.CreateSettingsTab(_perkWindow, PluginDir, new WindowOptions() { Name = "Perks", XmlViewName = "KeeperPerksView" }, _perkView, out var container);
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

        private void HandleBuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                if (window.Views.Contains(_buffView)) { return; }

                _buffView = View.CreateFromXml(PluginDirectory + "\\UI\\KeeperBuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Buffs", XmlViewName = "KeeperBuffsView" }, _buffView);
            }
            else if (_buffWindow == null || (_buffWindow != null && !_buffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_buffWindow, PluginDir, new WindowOptions() { Name = "Buffs", XmlViewName = "KeeperBuffsView" }, _buffView, out var container);
                _buffWindow = container;
            }
        }

        #endregion

        protected override void OnUpdate(float deltaTime)
        {
            if (Game.IsZoning || Time.NormalTime < _lastZonedTime + 1.7)
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
                window.FindView("BodyDevAbsorbsItemPercentageBox", out TextInputView bodyDevInput);
                window.FindView("StrengthAbsorbsItemPercentageBox", out TextInputView strengthInput);
                window.FindView("BioRegrowthPercentageBox", out TextInputView bioRegrowthPercentageInput);
                window.FindView("BioRegrowthDelayBox", out TextInputView bioRegrowthDelayInput);

                if (bioCocoonInput != null && !string.IsNullOrEmpty(bioCocoonInput.Text))
                    if (int.TryParse(bioCocoonInput.Text, out int bioCocoonValue))
                        if (Config.CharSettings[DynelManager.LocalPlayer.Name].BioCocoonPercentage != bioCocoonValue)
                            Config.CharSettings[DynelManager.LocalPlayer.Name].BioCocoonPercentage = bioCocoonValue;

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

                CancelAuras();
            }
        }

        #region Anti-Fear

        private bool RecastAntiFear(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsSettingEnabled("RecastAntiFear")) { return true; }

            return Buff(spell, spell.Nanoline, ref actionTarget);
        }

        protected bool Buff(Spell spell, NanoLine nanoline, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.FightingTarget != null || RelevantGenericNanos.ShrinkingGrowingflesh.Contains(spell.Id)) { return false; }

            if (SpellChecksPlayer(spell, nanoline))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        #endregion

        #region Buffs

        #endregion

        #region Auras


        private bool HpAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AuraSet1Selection.Heal != (AuraSet1Selection)_settings["AuraSet1Selection"].AsInt32()) { return false; }

            return NonCombatBuff(spell, ref actionTarget, fightingTarget);
        }

        private bool NpAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AuraSet1Selection.Nano != (AuraSet1Selection)_settings["AuraSet1Selection"].AsInt32()) { return false; }

            return NonCombatBuff(spell, ref actionTarget, fightingTarget);
        }

        private bool BarrierAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AuraSet3Selection.Reflect != (AuraSet3Selection)_settings["AuraSet3Selection"].AsInt32()) { return false; }

            return NonCombatBuff(spell, ref actionTarget, fightingTarget);
        }

        private bool ImminenceAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AuraSet3Selection.AAO != (AuraSet3Selection)_settings["AuraSet3Selection"].AsInt32()) { return false; }

            return NonCombatBuff(spell, ref actionTarget, fightingTarget);
        }

        private bool EnervateAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AuraSet2Selection.DeRoot != (AuraSet2Selection)_settings["AuraSet2Selection"].AsInt32()) { return false; }

            return NonCombatBuff(spell, ref actionTarget, fightingTarget);
        }

        private bool VengeanceAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AuraSet2Selection.Damage != (AuraSet2Selection)_settings["AuraSet2Selection"].AsInt32()) { return false; }

            return NonCombatBuff(spell, ref actionTarget, fightingTarget);
        }

        private bool SanctifierAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AuraSet4Selection.Sanc != (AuraSet4Selection)_settings["AuraSet4Selection"].AsInt32()) { return false; }

            return NonCombatBuff(spell, ref actionTarget, fightingTarget);
        }

        private bool ReaperAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (AuraSet4Selection.Reaper != (AuraSet4Selection)_settings["AuraSet4Selection"].AsInt32()) { return false; }

            return NonCombatBuff(spell, ref actionTarget, fightingTarget);
        }

        #endregion

        #region Team Buffs

        #endregion

        #region Misc

        private void CancelAuras()
        {
            if (AuraSet1Selection.Heal != (AuraSet1Selection)_settings["AuraSet1Selection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.HealAuras);
            }
            if (AuraSet1Selection.Nano != (AuraSet1Selection)_settings["AuraSet1Selection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.NanoAuras);
            }
            if (AuraSet2Selection.Damage != (AuraSet2Selection)_settings["AuraSet2Selection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.DamageAuras);
            }
            if (AuraSet2Selection.DeRoot != (AuraSet2Selection)_settings["AuraSet2Selection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.DerootAuras);
            }
            if (AuraSet3Selection.AAO != (AuraSet3Selection)_settings["AuraSet3Selection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.AAOAuras);
            }
            if (AuraSet3Selection.Reflect != (AuraSet3Selection)_settings["AuraSet3Selection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.ReflectAuras);
            }
            if (AuraSet4Selection.Sanc != (AuraSet4Selection)_settings["AuraSet4Selection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.SancAuras);
            }
            if (AuraSet4Selection.Reaper != (AuraSet4Selection)_settings["AuraSet4Selection"].AsInt32())
            {
                CancelBuffs(RelevantNanos.ReaperAuras);
            }
        }

        protected override bool ShouldUseSpecialAttack(SpecialAttack specialAttack)
        {
            return specialAttack != SpecialAttack.Dimach;
        }

        private static class RelevantNanos
        {
            public const int CourageOfTheJust = 279380;
            public const int CompositeAttribute = 223372;
            public const int CompositeNano = 223380;
            public const int CompositeUtility = 287046;
            public const int CompositePhysical = 215264;
            public const int CompositeMartialProwess = 302158;
            public const int CompositeMelee = 223360;
            public const int PunisherOfTheWicked = 301602;

            public static int[] HealAuras = new[] { 273362, 223024, 210536, 210528 };
            public static int[] NanoAuras = new[] { 224073, 210597, 210589 };

            public static readonly int[] ReflectAuras = Spell.GetSpellsForNanoline(NanoLine.KeeperAura_Absorb_Reflect_AMSBuff)
                .Where(s => s.Name.Contains("Barrier of")).OrderByStackingOrder().Select(s => s.Id).ToArray();
            public static readonly int[] DamageAuras = Spell.GetSpellsForNanoline(NanoLine.KeeperAura_Damage_SnareReductionBuff)
                .Where(s => s.Name.Contains("Vengeance")).OrderByStackingOrder().Select(s => s.Id).ToArray();
            public static readonly int[] AAOAuras = Spell.GetSpellsForNanoline(NanoLine.KeeperAura_Absorb_Reflect_AMSBuff)
                .Where(s => s.Name.Contains("Imminence of")).OrderByStackingOrder().Select(s => s.Id).ToArray();
            public static readonly int[] DerootAuras = Spell.GetSpellsForNanoline(NanoLine.KeeperAura_Damage_SnareReductionBuff)
                .Where(s => s.Name.Contains("Enervate")).OrderByStackingOrder().Select(s => s.Id).ToArray();
            public static readonly int[] ReaperAuras = Spell.GetSpellsForNanoline(NanoLine.KeeperProcBuff)
                .Where(s => s.Name.Contains("Reaper")).OrderByStackingOrder().Select(s => s.Id).ToArray();
            public static readonly int[] SancAuras = Spell.GetSpellsForNanoline(NanoLine.KeeperProcBuff)
                .Where(s => s.Name.Contains("Sanctifier")).OrderByStackingOrder().Select(s => s.Id).ToArray();
        }
        public enum AuraSet1Selection
        {
            Heal, Nano
        }
        public enum AuraSet2Selection
        {
            Damage, DeRoot
        }
        public enum AuraSet3Selection
        {
            AAO, Reflect
        }
        public enum AuraSet4Selection
        {
            Sanc, Reaper
        }

        public enum ProcType1Selection
        {
            RighteousSmite = 1179013701,
            SymbioticBypass = 138053716,
            VirtuousReaper = 1179144773,
            IgnoreTheUnrepentant = 1163154517,
            PureStrike = 1163088981,
            EschewTheFaithless = 1146770517,
            RighteousStrike = 1179931461
        }

        public enum ProcType2Selection
        {
            HonorRestored = 1129202757,
            AmbientPurification = 1178945877,
            BenevolentBarrier = 1380537172,
            Subjugation = 1380467279,
            FaithfulReconstruction = 1398232143
        }

        #endregion
    }
}
