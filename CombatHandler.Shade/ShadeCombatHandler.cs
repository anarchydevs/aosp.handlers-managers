﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using CombatHandler.Generic;
using System.Collections.Generic;
using System.Linq;

namespace CombatHandler.Shade
{
    public class ShadeCombatHandler : GenericCombatHandler
    {
        private static string PluginDirectory;

        private const int MissingHealthAbortCombatPercentage = 30;

        private static bool ToggleBuffing = false;
        private static bool ToggleComposites = false;
        private static bool ToggleRez = false;

        private static bool _shadeSiphon;

        private static Window _buffWindow;
        private static Window _debuffWindow;
        private static Window _procWindow;
        private static Window _itemWindow;
        private static Window _perkWindow;

        private static View _buffView;
        private static View _debuffView;
        private static View _procView;
        private static View _itemView;
        private static View _perkView;

        private static double _ncuUpdateTime;

        public ShadeCombatHandler(string pluginDir) : base(pluginDir)
        {
            IPCChannel.RegisterCallback((int)IPCOpcode.RemainingNCU, OnRemainingNCUMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalBuffing, OnGlobalBuffingMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalComposites, OnGlobalCompositesMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.GlobalRez, OnGlobalRezMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.ClearBuffs, OnClearBuffs);
            IPCChannel.RegisterCallback((int)IPCOpcode.Disband, OnDisband);

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

            _settings.AddVariable("Buffing", true);
            _settings.AddVariable("Composites", true);

            _settings.AddVariable("GlobalBuffing", true);
            _settings.AddVariable("GlobalComposites", true);
            _settings.AddVariable("GlobalRez", true);

            _settings.AddVariable("SharpObjects", true);
            _settings.AddVariable("Grenades", true);

            _settings.AddVariable("StimTargetSelection", (int)StimTargetSelection.Self);

            _settings.AddVariable("Kits", true);

            _settings.AddVariable("ProcType1Selection", (int)ProcType1Selection.BlackenedLegacy);
            _settings.AddVariable("ProcType2Selection", (int)ProcType2Selection.Blackheart);

            _settings.AddVariable("Runspeed", false);
            _settings.AddVariable("RunspeedTeam", false);
            _settings.AddVariable("SLMap", false);

            _settings.AddVariable("InitDebuffProc", false);
            _settings.AddVariable("DamageProc", false);
            _settings.AddVariable("DOTProc", false);
            _settings.AddVariable("StunProc", false);

            _settings.AddVariable("HealthDrain", false);
            _settings.AddVariable("SpiritSiphon", false);

            RegisterSettingsWindow("Shade Handler", "ShadeSettingsView.xml");

            //Perks
            RelevantPerks.SpiritPhylactery.ForEach(p => RegisterPerkProcessor(p, SpiritPhylacteryPerk));
            RelevantPerks.TotemicRites.ForEach(p => RegisterPerkProcessor(p, TotemicRitesPerk));
            RelevantPerks.PiercingMastery.ForEach(p => RegisterPerkProcessor(p, PiercingMasteryPerk));
            RegisterPerkProcessor(PerkHash.EvasiveStance, EvasiveStance, CombatActionPriority.High);

            //Spells
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.EmergencySneak).OrderByStackingOrder(), SmokeBombNano, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NemesisNanoPrograms).OrderByStackingOrder(), ShadesCaress, CombatActionPriority.High);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.HealthDrain).OrderByStackingOrder(), HealthDrain);
            RegisterSpellProcessor(RelevantNanos.SpiritDrain, SpiritSiphon);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AgilityBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ConcealmentBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.FastAttackBuffs).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MultiwieldBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MartialArtsBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ShadePiercingBuff).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SneakAttackBuffs).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.WeaponEffectAdd_On2).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.AADBuffs).OrderByStackingOrder(),
                (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                            => NonCombatBuff(spell, ref actionTarget, fightingTarget, null));

            RegisterSpellProcessor(RelevantNanos.ShadeDmgProc,(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                    => NonCombatBuff(spell, ref actionTarget, fightingTarget, "DamageProc"));
            RegisterSpellProcessor(RelevantNanos.ShadeStunProc, (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                    => NonCombatBuff(spell, ref actionTarget, fightingTarget, "StunProc"));
            RegisterSpellProcessor(RelevantNanos.ShadeInitDebuffProc, (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                    => NonCombatBuff(spell, ref actionTarget, fightingTarget, "InitDebuffProc"));
            RegisterSpellProcessor(RelevantNanos.ShadeDOTProc, (Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
                    => NonCombatBuff(spell, ref actionTarget, fightingTarget, "DOTProc"));

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.RunspeedBuffs).OrderByStackingOrder(), FasterThanYourShadow);

            //Items
            RegisterItemProcessor(RelevantItems.Tattoo, RelevantItems.Tattoo, TattooItem, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.Sappo, RelevantItems.Sappo, Sappo);

            //LE Proc
            RegisterPerkProcessor(PerkHash.LEProcShadeBlackenedLegacy, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeSiphonBeing, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeShadowedGift, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeDrainEssence, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeElusiveSpirit, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeToxicConfusion, LEProc1, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeSapLife, LEProc1, CombatActionPriority.Low);

            RegisterPerkProcessor(PerkHash.LEProcShadeBlackheart, LEProc2, CombatActionPriority.Low); ;
            RegisterPerkProcessor(PerkHash.LEProcShadeTwistedCaress, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeConcealedSurprise, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeMisdirection, LEProc2, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcShadeDeviousSpirit, LEProc2, CombatActionPriority.Low);


            PluginDirectory = pluginDir;

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
        }

        public Window[] _windows => new Window[] { _buffWindow, _debuffWindow, _procWindow, _itemWindow, _perkWindow };

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
                //Cannot re-use the view, as crashes client. I don't know why.
                //Cannot stop Multi-Tabs. Easy fix would be correct naming of views to reference against WindowOptions - options.Name
                _buffView = View.CreateFromXml(PluginDirectory + "\\UI\\ShadeBuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Buffs", XmlViewName = "ShadeBuffsView" }, _buffView);
            }
            else if (_buffWindow == null || (_buffWindow != null && !_buffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_buffWindow, PluginDir, new WindowOptions() { Name = "Buffs", XmlViewName = "ShadeBuffsView" }, _buffView, out var container);
                _buffWindow = container;
            }
        }

        private void HandleDebuffViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                //Cannot stop Multi-Tabs. Easy fix would be correct naming of views to reference against WindowOptions - options.Name
                _debuffView = View.CreateFromXml(PluginDirectory + "\\UI\\ShadeDebuffsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Debuffs", XmlViewName = "ShadeDebuffsView" }, _debuffView);
            }
            else if (_debuffWindow == null || (_debuffWindow != null && !_debuffWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_debuffWindow, PluginDir, new WindowOptions() { Name = "Debuffs", XmlViewName = "ShadeDebuffsView" }, _debuffView, out var container);
                _debuffWindow = container;
            }
        }
        private void HandlePerkViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                if (window.Views.Contains(_perkView)) { return; }

                _perkView = View.CreateFromXml(PluginDirectory + "\\UI\\ShadePerksView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Perks", XmlViewName = "ShadePerksView" }, _perkView);

                window.FindView("SphereDelayBox", out TextInputView sphereInput);
                window.FindView("WitDelayBox", out TextInputView witOfTheAtroxInput);
                window.FindView("SelfHealPercentageBox", out TextInputView selfHealInput);
                window.FindView("SelfNanoPercentageBox", out TextInputView selfNanoInput);
                window.FindView("TeamHealPercentageBox", out TextInputView teamHealInput);
                window.FindView("TeamNanoPercentageBox", out TextInputView teamNanoInput);

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
            }
            else if (_perkWindow == null || (_perkWindow != null && !_perkWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_perkWindow, PluginDir, new WindowOptions() { Name = "Perks", XmlViewName = "ShadePerksView" }, _perkView, out var container);
                _perkWindow = container;

                container.FindView("SphereDelayBox", out TextInputView sphereInput);
                container.FindView("WitDelayBox", out TextInputView witOfTheAtroxInput);
                container.FindView("SelfHealPercentageBox", out TextInputView selfHealInput);
                container.FindView("SelfNanoPercentageBox", out TextInputView selfNanoInput);
                container.FindView("TeamHealPercentageBox", out TextInputView teamHealInput);
                container.FindView("TeamNanoPercentageBox", out TextInputView teamNanoInput);

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
            }
        }
        private void HandleItemViewClick(object s, ButtonBase button)
        {
            Window window = _windows.Where(c => c != null && c.IsValid).FirstOrDefault();
            if (window != null)
            {
                //Cannot re-use the view, as crashes client. I don't know why.
                if (window.Views.Contains(_itemView)) { return; }

                _itemView = View.CreateFromXml(PluginDirectory + "\\UI\\ShadeItemsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Items", XmlViewName = "ShadeItemsView" }, _itemView);

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
                SettingsController.CreateSettingsTab(_itemWindow, PluginDir, new WindowOptions() { Name = "Items", XmlViewName = "ShadeItemsView" }, _itemView, out var container);
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
                //Cannot re-use the view, as crashes client. I don't know why.

                if (window.Views.Contains(_procView)) { return; }

                _procView = View.CreateFromXml(PluginDirectory + "\\UI\\ShadeProcsView.xml");
                SettingsController.AppendSettingsTab(window, new WindowOptions() { Name = "Procs", XmlViewName = "ShadeProcsView" }, _procView);
            }
            else if (_procWindow == null || (_procWindow != null && !_procWindow.IsValid))
            {
                SettingsController.CreateSettingsTab(_procWindow, PluginDir, new WindowOptions() { Name = "Procs", XmlViewName = "ShadeProcsView" }, _procView, out var container);
                _procWindow = container;
            }
        }

        #endregion

        protected override void OnUpdate(float deltaTime)
        {
            if (Game.IsZoning || Time.NormalTime < _lastZonedTime + 2.3)
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

            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
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

            if (_settings["InitDebuffProc"].AsBool() && _settings["DamageProc"].AsBool())
            {
                _settings["InitDebuffProc"] = false;
                _settings["DamageProc"] = false;

                Chat.WriteLine("Only activate one Proc option.");
            }
            if (_settings["InitDebuffProc"].AsBool() && _settings["DOTProc"].AsBool())
            {
                _settings["InitDebuffProc"] = false;
                _settings["DOTProc"] = false;

                Chat.WriteLine("Only activate one Proc option.");
            }
            if (_settings["InitDebuffProc"].AsBool() && _settings["StunProc"].AsBool())
            {
                _settings["InitDebuffProc"] = false;
                _settings["StunProc"] = false;

                Chat.WriteLine("Only activate one Proc option.");
            }
            if (_settings["DamageProc"].AsBool() && _settings["StunProc"].AsBool())
            {
                _settings["DamageProc"] = false;
                _settings["StunProc"] = false;

                Chat.WriteLine("Only activate one Proc option.");
            }
            if (_settings["DamageProc"].AsBool() && _settings["DOTProc"].AsBool())
            {
                _settings["DamageProc"] = false;
                _settings["DOTProc"] = false;

                Chat.WriteLine("Only activate one Proc option.");
            }
            if (_settings["StunProc"].AsBool() && _settings["DOTProc"].AsBool())
            {
                _settings["StunProc"] = false;
                _settings["DOTProc"] = false;

                Chat.WriteLine("Only activate one Proc option.");
            }

            if (!IsSettingEnabled("Runspeed") && !IsSettingEnabled("RunspeedTeam"))
            {
                CancelBuffs(RelevantNanos.FasterThanYourShadow);
            }
            if (!IsSettingEnabled("InitDebuffProc"))
            {
                CancelBuffs(RelevantNanos.ShadeInitDebuffProc);
            }
            if (!IsSettingEnabled("DamageProc"))
            {
                CancelBuffs(RelevantNanos.ShadeDmgProc);
            }
            if (!IsSettingEnabled("DOTProc"))
            {
                CancelBuffs(RelevantNanos.ShadeDOTProc);
            }
            if (!IsSettingEnabled("StunProc"))
            {
                CancelBuffs(RelevantNanos.ShadeStunProc);
            }
        }

        #region Items

        private bool Sappo(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingtarget == null) { return false; }

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.MartialArts)) { return false; }

            return true;
        }


        private bool TattooItem(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            // don't use if BM is locked (we will add this dynamically later)
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BiologicalMetamorphosis)) { return false; }

            // don't use if we're above 40%
            if (DynelManager.LocalPlayer.HealthPercent > 40) { return false; }

            // don't use if nothing is fighting us
            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            // don't use if we have another major absorb (example: nanomage booster) running
            // we could check remaining absorb stat to be slightly more effective
            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon)) { return false; }

            // don't use if our fighting target has caress running
            if (fightingtarget.Buffs.Contains(275242)) { return false; }

            return true;
        }

        #endregion

        #region Perks
        private bool PiercingMasteryPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            // Don't PM if there are TR/SP chains in progress
            if (_actionQueue.Any(x => x.CombatAction is PerkAction action && (RelevantPerks.TotemicRites.Contains(action.Hash) || RelevantPerks.SpiritPhylactery.Contains(action.Hash)))) { return false; }

            if (!(PerkAction.Find(PerkHash.Stab, out PerkAction stab) && PerkAction.Find(PerkHash.DoubleStab, out PerkAction doubleStab)))
                return true;

            if (perkAction.Hash == PerkHash.Perforate)
            {
                if (_actionQueue.Any(x => x.CombatAction is PerkAction action && (action == stab || action == doubleStab))) { return false; }
            }

            if (!(PerkAction.Find(PerkHash.Stab, out PerkAction perforate) && PerkAction.Find(PerkHash.DoubleStab, out PerkAction lacerate))) { return true; }

            if (perkAction.Hash == PerkHash.Impale)
            {
                if (_actionQueue.Any(x => x.CombatAction is PerkAction action && (action == stab || action == doubleStab || action == perforate || action == lacerate))) { return false; }
            }

            return true;
        }

        private bool SpiritPhylacteryPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            //Don't SP if there are TR/PM chains in progress
            if (_actionQueue.Any(x => x.CombatAction is PerkAction action && (RelevantPerks.TotemicRites.Contains(action.Hash) || RelevantPerks.PiercingMastery.Contains(action.Hash)))) { return false; }

            return true;
        }

        private bool TotemicRitesPerk(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            //Don't TR if there are SP/PM chains in progress
            if (_actionQueue.Any(x => x.CombatAction is PerkAction action && (RelevantPerks.SpiritPhylactery.Contains(action.Hash) || RelevantPerks.PiercingMastery.Contains(action.Hash)))) { return false; }

            return true;
        }

        #endregion

        #region Procs

        #endregion

        #region Buffs

        private bool FasterThanYourShadow(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsInsideInnerSanctum()) { return false; }

            if (IsSettingEnabled("RunspeedTeam"))
            {
                if (DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.RK_RUN_BUFFS))
                {
                    CancelBuffs(RelevantNanos.RK_RUN_BUFFS);
                }

                return FTYSTeamBuff(spell, fightingTarget, ref actionTarget);
            }

            if (IsSettingEnabled("Runspeed"))
            {
                if (DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.RK_RUN_BUFFS))
                {
                    CancelBuffs(RelevantNanos.RK_RUN_BUFFS);
                }

                return NonComabtTeamBuff(spell, fightingTarget, ref actionTarget);
            }

            return false;
        }
        private bool FTYSTeamBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Team.IsInTeam)
            {
                SimpleChar target = DynelManager.Players
                    .Where(c => c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0
                        & SpellChecksOther(spell, spell.Nanoline, c))
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = target;
                    return true;
                }
            }

            if (fightingTarget == null) { return false; }

            return NonComabtTeamBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool SmokeBombNano(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            actionTarget.ShouldSetTarget = false;

            if (DynelManager.LocalPlayer.HealthPercent <= MissingHealthAbortCombatPercentage) { return true; }

            return false;
        }

        private bool SpiritSiphon(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("SpiritSiphon")) { return false; }

            if (fightingTarget == null && _shadeSiphon)
            {
                _shadeSiphon = false;
            }

            if (!DynelManager.LocalPlayer.IsAttacking) { return false; }

            if (DynelManager.LocalPlayer.Nano < spell.Cost) { return false; }

            if (fightingTarget != null && fightingTarget.HealthPercent <= 21)
            {
                if (!_shadeSiphon)
                {
                    _shadeSiphon = true;
                    spell.Cast(fightingTarget);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Debuffs

        private bool ShadesCaress(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (!DynelManager.LocalPlayer.IsAttacking || fightingTarget == null
                 || !CanCast(spell)) { return false; }

            if (fightingTarget.HealthPercent < 5) { return false; }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                List<SimpleChar> teamMembersLowHp = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent <= 50)
                    .ToList();

                if (teamMembersLowHp.Count >= 3)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = fightingTarget;
                    return true;
                }
            }

            if (DynelManager.LocalPlayer.HealthPercent <= 80 && fightingTarget.HealthPercent > 5) { return true; }

            return false;
        }
        private bool HealthDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            if (fightingTarget.Buffs.Contains(273390)) { return false; }

            if (DynelManager.LocalPlayer.NanoPercent > 80) { return true; }

            // Otherwise save it for if our health starts to drop
            if (DynelManager.LocalPlayer.HealthPercent >= 85) { return false; }

            return ToggledTargetDebuff("HealthDrain", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }


        #endregion

        #region Misc

        private class RelevantItems
        {
            public const int Sappo = 267525;
            public const int Tattoo = 269511;
        }

        private class RelevantNanos
        {
            public const int ShadesCaress = 266300;
            public const int CompositeAttribute = 223372;
            public const int CompositeNano = 223380;
            public const int CompositeMelee = 223360;
            public const int CompositeMeleeSpec = 215264;
            public const int SpiritDrain = 297342;
            public static readonly int[] FasterThanYourShadow = { 272371 };
            public static readonly int[] EVASION_BUFFS = { 275844, 29247, 28903, 28878, 28872, 218070, 218068, 218066,
            218064, 218062, 218060, 272371, 270808, 30745, 302188, 29272, 270802, 28603, 223125, 223131, 223129, 215718,
            223127, 272416, 272415, 272414, 272413, 272412};
            public static readonly int[] RK_RUN_BUFFS = { 93132, 93126, 93127, 93128, 93129, 93130, 93131, 93125 };
            public static readonly int[] ShadeDmgProc = { 224167, 224165, 224163, 210371, 210369, 210367, 210365, 210363, 210361, 210359, 210357, 210355, 210353 };
            public static readonly int[] ShadeStunProc = { 224171, 224169, 210380, 210378, 210376 };
            public static readonly int[] ShadeInitDebuffProc = { 224177, 210407, 210401 };
            public static readonly int[] ShadeDOTProc = { 224161, 224159, 210395, 210393, 210391, 210389, 210387 };
        }

        private class RelevantPerks
        {
            public static readonly List<PerkHash> TotemicRites = new List<PerkHash>
            {
                PerkHash.RitualOfDevotion,
                PerkHash.DevourVigor,
                PerkHash.RitualOfZeal,
                PerkHash.DevourEssence,
                PerkHash.RitualOfSpirit,
                PerkHash.DevourVitality,
                PerkHash.RitualOfBlood
            };

            public static readonly List<PerkHash> PiercingMastery = new List<PerkHash>
            {
                PerkHash.Stab,
                PerkHash.DoubleStab,
                PerkHash.Perforate,
                PerkHash.Lacerate,
                PerkHash.Impale,
                PerkHash.Gore,
                PerkHash.Hecatomb
            };

            public static readonly List<PerkHash> SpiritPhylactery = new List<PerkHash>
            {
                PerkHash.CaptureVigor,
                PerkHash.UnsealedBlight,
                PerkHash.CaptureEssence,
                PerkHash.UnsealedPestilence,
                PerkHash.CaptureSpirit,
                PerkHash.UnsealedContagion,
                PerkHash.CaptureVitality
            };
        }

        public enum ProcType1Selection
        {
            BlackenedLegacy = 1111706695,
            SiphonBeing = 1397768775,
            ShadowedGift = 1396787014,
            DrainEssence = 1146242387,
            ElusiveSpirit = 1163219794,
            ToxicConfusion = 1330529877,
            SapLife = 1397771337
        }

        public enum ProcType2Selection
        {
            Blackheart = 1129007173,
            TwistedCaress = 1347179342,
            ConcealedSurprise = 1213354838,
            Misdirection = 1396984146,
            DeviousSpirit = 1146508112
        }

        #endregion
    }
}
