﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using CombatHandler.Generic;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CombatHandler.Engi
{
    class EngiCombatHandler : GenericCombatHandler
    {
        private const float DelayBetweenTrims = 1;
        private const float DelayBetweenDiverTrims = 305;
        private bool attackPetTrimmedAggressive = false;
        private double _lastTrimTime = 0;
        private double _recastBlinds = Time.NormalTime;
        private Dictionary<PetType, bool> petTrimmedAggDef = new Dictionary<PetType, bool>();
        private Dictionary<PetType, bool> petTrimmedHpDiv = new Dictionary<PetType, bool>();
        private Dictionary<PetType, bool> petTrimmedOffDiv = new Dictionary<PetType, bool>();
        private Dictionary<PetType, double> _lastPetTrimDivertOffTime = new Dictionary<PetType, double>()
        {
            { PetType.Attack, 0 },
            { PetType.Support, 0 }
        };
        private Dictionary<PetType, double> _lastPetTrimDivertHpTime = new Dictionary<PetType, double>()
        {
            { PetType.Attack, 0 },
            { PetType.Support, 0 }
        };

        public EngiCombatHandler(string pluginDir) : base(pluginDir)
        {
            settings.AddVariable("SpawnPets", true);
            settings.AddVariable("BuffPets", true);
            settings.AddVariable("HealPets", false);

            settings.AddVariable("DivertHpTrimmer", true);
            settings.AddVariable("DivertOffTrimmer", true);
            settings.AddVariable("TauntTrimmer", true);
            settings.AddVariable("AggDefTrimmer", true);

            settings.AddVariable("ShieldRipper", false);
            settings.AddVariable("SnareAura", false);

            settings.AddVariable("AuraShield", false);
            settings.AddVariable("AuraDamage", false);
            settings.AddVariable("AuraReflect", false);
            settings.AddVariable("AuraArmor", false);

            settings.AddVariable("Offperks", false);
            settings.AddVariable("Defperks", false);

            settings.AddVariable("SpamBlindAura", false);
            settings.AddVariable("SpamSnareAura", false);

            RegisterSettingsWindow("Engineer Handler", "EngineerSettingsView.xml");

            //LE Procs
            RegisterPerkProcessor(PerkHash.LEProcEngineerDestructiveTheorem, LEProc, CombatActionPriority.Low);
            RegisterPerkProcessor(PerkHash.LEProcEngineerDroneMissiles, LEProc, CombatActionPriority.Low);

            //Buffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PistolBuff).OrderByStackingOrder(), PistolMasteryBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.GrenadeBuffs).OrderByStackingOrder(), PistolGrenadeBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ShadowlandReflectBase).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.SpecialAttackAbsorberBase).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.EngineerSpecialAttackAbsorber).OrderByStackingOrder(), GenericBuff);

            if(Spell.Find(RelevantNanos.BoostedTendons, out Spell boostedTendons))
            {
                RegisterSpellProcessor(boostedTendons, GenericBuff);
            }

            RegisterSpellProcessor(RelevantNanos.DamageBuffLineA, TeamBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ArmorBuff).OrderByStackingOrder(), TeamBuff);
            RegisterSpellProcessor(RelevantNanos.Blinds, BlindAura);
            RegisterSpellProcessor(RelevantNanos.ShieldRippers, ShieldRipperAura);
            RegisterSpellProcessor(RelevantNanos.AuraArmor, AuraArmor);
            RegisterSpellProcessor(RelevantNanos.AuraDamage, AuraDamage);
            RegisterSpellProcessor(RelevantNanos.AuraReflect, AuraReflect);
            RegisterSpellProcessor(RelevantNanos.AuraShield, AuraShield);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.EngineerPetAOESnareBuff).OrderByStackingOrder(), SnareAura);
            RegisterSpellProcessor(RelevantNanos.IntrusiveAuraCancellation, AuraCancellation);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.InitiativeBuffs).OrderByStackingOrder(), InitBuff);

            //Pet Spawners
            RegisterSpellProcessor(PetsList.Pets.Where(x => x.Value.PetType == PetType.Attack).Select(x => x.Key).ToArray(), PetSpawner);
            RegisterSpellProcessor(PetsList.Pets.Where(x => x.Value.PetType == PetType.Support).Select(x => x.Key).ToArray(), PetSpawner);

            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.EngineerMiniaturization).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetShortTermDamageBuffs).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.PetDefensiveNanos).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MPPetInitiativeBuffs).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.EngineerMiniaturization).OrderByStackingOrder(), PetTargetBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.ArmorBuff).OrderByStackingOrder(), PetTargetBuff);

            RegisterSpellProcessor(RelevantNanos.PetHealing, PetHealing);
            RegisterSpellProcessor(RelevantNanos.PetHealingCH, PetHealingCH);

            RegisterSpellProcessor(RelevantNanos.ShieldOfObedientServant, ShieldOfTheObedientServant);
            RegisterSpellProcessor(RelevantNanos.MastersBidding, MastersBidding);
            RegisterSpellProcessor(RelevantNanos.DamageBuffLineA, PetDamage);

            RegisterPerkProcessor(PerkHash.ChaoticEnergy, ChaoticEnergyBox);
            RegisterPerkProcessor(PerkHash.SiphonBox, SiphonBox);
            RegisterPerkProcessor(PerkHash.TauntBox, TauntBox);

            ResetTrimmers();
            RegisterItemProcessor(RelevantTrimmers.PositiveAggressiveDefensive, RelevantTrimmers.PositiveAggressiveDefensive, PetAggDefTrimmer);

            RegisterItemProcessor(RelevantTrimmers.IncreaseAggressivenessLow, RelevantTrimmers.IncreaseAggressivenessLow, PetAggressiveTrimmer);
            RegisterItemProcessor(RelevantTrimmers.IncreaseAggressivenessHigh, RelevantTrimmers.IncreaseAggressivenessHigh, PetAggressiveTrimmer);

            RegisterItemProcessor(RelevantTrimmers.DivertEnergyToOffenseLow, RelevantTrimmers.DivertEnergyToOffenseLow, PetDivertOffTrimmer);
            RegisterItemProcessor(RelevantTrimmers.DivertEnergyToOffenseHigh, RelevantTrimmers.DivertEnergyToOffenseHigh, PetDivertOffTrimmer);

            RegisterItemProcessor(RelevantTrimmers.DivertEnergyToHitpointsLow, RelevantTrimmers.DivertEnergyToHitpointsLow, PetDivertHpTrimmer);
            RegisterItemProcessor(RelevantTrimmers.DivertEnergyToHitpointsHigh, RelevantTrimmers.DivertEnergyToHitpointsHigh, PetDivertHpTrimmer);

            //Pet Shells
            foreach (PetSpellData petData in PetsList.Pets.Values)
            {
                RegisterItemProcessor(petData.ShellId, petData.ShellId2, PetSpawnerItem);
            }

            Game.TeleportEnded += OnZoned;
        }

        private bool AuraCancellation(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if(fightingTarget != null) { return false; }

            Pet petWithSnareAura = FindPetThat(pet => HasBuffNanoLine(NanoLine.EngineerPetAOESnareBuff, pet.Character));

            if (petWithSnareAura != null)
            {
                actionTarget.Target = petWithSnareAura.Character;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            return false;
        }

        private bool SnareAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsSettingEnabled("SpamSnareAura") && ShouldSpamAoeSnare())
            {
                Pet petToCastOn = FindPetThat(pet => true);
                if (petToCastOn != null)
                {
                    actionTarget.Target = petToCastOn.Character;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            if (!IsSettingEnabled("SnareAura") || fightingTarget == null) { return false; }

            return PetTargetBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool ShouldSpamAoeSnare()
        {
            return DynelManager.NPCs
                .Where(c => c.Name == "Flaming Vengeance" ||
                    c.Name == "Hand of the Colonel")
                .Any();
        }

        protected bool PistolGrenadeBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Team.IsInTeam)
            {
                return TeamBuffNoNTWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Grenade) || TeamBuffNoNTWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Pistol);
            }
            else
                return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Grenade) || BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Pistol);
        }

        private bool InitBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffInitEngi(spell, fightingTarget, ref actionTarget);
        }

        private bool ShieldRipperAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("ShieldRipper") || fightingTarget == null) { return false; }

            if (IsSettingEnabled("SpamBlindAura")) { return false; }

            return !HasBuffNanoLine(NanoLine.EngineerDebuffAuras, DynelManager.LocalPlayer);
        }

        private bool AuraArmor(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("AuraArmor") && !IsSettingEnabled("AuraReflect") && !IsSettingEnabled("AuraShield") && !IsSettingEnabled("AuraDamage")) { return false; }

            if (IsSettingEnabled("AuraDamage") || IsSettingEnabled("AuraReflect") || IsSettingEnabled("AuraShield")) { return false; }

            //if (IsSettingEnabled("AuraArmor") && !HasBuffNanoLine(NanoLine.EngineerAuras, DynelManager.LocalPlayer)) { return false; }

            return !HasBuffNanoLine(NanoLine.EngineerAuras, DynelManager.LocalPlayer);
        }

        private bool AuraDamage(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("AuraArmor") && !IsSettingEnabled("AuraReflect") && !IsSettingEnabled("AuraShield") && !IsSettingEnabled("AuraDamage")) { return false; }

            if (IsSettingEnabled("AuraArmor") || IsSettingEnabled("AuraReflect") || IsSettingEnabled("AuraShield")) { return false; }

            //if (IsSettingEnabled("AuraDamage") && !HasBuffNanoLine(NanoLine.EngineerAuras, DynelManager.LocalPlayer)) { return false; }

            return !HasBuffNanoLine(NanoLine.EngineerAuras, DynelManager.LocalPlayer);
        }

        private bool AuraReflect(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("AuraArmor") && !IsSettingEnabled("AuraReflect") && !IsSettingEnabled("AuraShield") && !IsSettingEnabled("AuraDamage")) { return false; }

            if (IsSettingEnabled("AuraDamage") || IsSettingEnabled("AuraArmor") || IsSettingEnabled("AuraShield")) { return false; }

            //if (IsSettingEnabled("AuraReflect") && !HasBuffNanoLine(NanoLine.EngineerAuras, DynelManager.LocalPlayer)) { return false; }

            return !HasBuffNanoLine(NanoLine.EngineerAuras, DynelManager.LocalPlayer);
        }

        private bool AuraShield(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("AuraArmor") && !IsSettingEnabled("AuraReflect") && !IsSettingEnabled("AuraShield") && !IsSettingEnabled("AuraDamage")) { return false; }

            if (IsSettingEnabled("AuraDamage") || IsSettingEnabled("AuraReflect") || IsSettingEnabled("AuraArmor")) { return false; }

            //if (IsSettingEnabled("AuraShield") && !HasBuffNanoLine(NanoLine.EngineerAuras, DynelManager.LocalPlayer)) { return false; }

            return !HasBuffNanoLine(NanoLine.EngineerAuras, DynelManager.LocalPlayer);
        }

        private bool BlindAura(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsSettingEnabled("ShieldRipper") || fightingTarget == null) { return false; }

            if (IsSettingEnabled("SpamBlindAura"))
            {
                if (Time.NormalTime - _recastBlinds > 9)
                {
                    _recastBlinds = Time.NormalTime;
                    return true;
                }
            }

            return false;
        }

        private bool PetHealing(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("HealPets") || !CanLookupPetsAfterZone()) { return false; }

            Pet pettoheal = FindPetNeedsHeal(90);
            if (pettoheal != null)
            {
                actionTarget.Target = pettoheal.Character;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            return false;
        }

        private bool PetHealingCH(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("HealPets") || !CanLookupPetsAfterZone()) { return false; }

            Pet pettoheal = FindPetNeedsHeal(90);
            if (pettoheal != null)
            {
                actionTarget.Target = pettoheal.Character;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            return false;
        }

        private bool ChaoticEnergyBox(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Offperks") || !CanLookupPetsAfterZone()) { return false; }

            Pet petToPerk = FindPetsWithoutBuff(RelevantNanos.PerkChaoticEnergy);

            CancelBuffs(RelevantNanos.PerkTauntBox);
            CancelBuffs(RelevantNanos.PerkSiphonBox);

            if (petToPerk != null)
            {
                actionTarget.Target = petToPerk.Character;
                actionTarget.ShouldSetTarget = true;
                return true;
            }
            return false;
        }

        private bool SiphonBox(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Defperks") || !CanLookupPetsAfterZone()) { return false; }

            Pet petToPerk = FindPetsWithoutBuff(RelevantNanos.PerkSiphonBox);

            CancelBuffs(RelevantNanos.PerkChaoticEnergy);

            if (petToPerk != null)
            {
                if (petToPerk.Type == PetType.Attack) { return false; }

                actionTarget.Target = petToPerk.Character;
                actionTarget.ShouldSetTarget = true;
                return true;
            }
            return false;
        }

        private bool TauntBox(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Defperks") || !CanLookupPetsAfterZone()) { return false; }

            Pet petToPerk = FindPetsWithoutBuff(RelevantNanos.PerkTauntBox);

            CancelBuffs(RelevantNanos.PerkChaoticEnergy);

            if (petToPerk != null)
            {
                if (petToPerk.Type == PetType.Support) { return false; }

                actionTarget.Target = petToPerk.Character;
                actionTarget.ShouldSetTarget = true;
                return true;
            }
            return false;
        }

        protected bool PetDivertHpTrimmer(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!IsSettingEnabled("DivertHpTrimmer") || !CanLookupPetsAfterZone() || !CanTrim()) { return false; }

            Pet petToTrim = FindSupportPetThat(CanDivertHpTrim);

            if (petToTrim != null)
            {
                actiontarget.Target = petToTrim.Character;
                actiontarget.ShouldSetTarget = true;
                petTrimmedHpDiv[petToTrim.Type] = true;
                _lastPetTrimDivertHpTime[petToTrim.Type] = Time.NormalTime;
                _lastTrimTime = Time.NormalTime;
                return true;
            }
            return false;
        }


        protected bool PetDivertOffTrimmer(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!IsSettingEnabled("DivertOffTrimmer") || !CanLookupPetsAfterZone() || !CanTrim()) { return false; }

            Pet petToTrim;

            if (IsSettingEnabled("DivertHpTrimmer"))
            {
                petToTrim = FindAttackPetThat(CanDivertOffTrim);

                if (petToTrim != null)
                {
                    actiontarget.Target = petToTrim.Character;
                    actiontarget.ShouldSetTarget = true;
                    petTrimmedOffDiv[petToTrim.Type] = true;
                    _lastPetTrimDivertOffTime[petToTrim.Type] = Time.NormalTime;
                    _lastTrimTime = Time.NormalTime;
                    return true;
                }
            }
            else
            {
                petToTrim = FindPetThat(CanDivertOffTrim);

                if (petToTrim != null)
                {
                    actiontarget.Target = petToTrim.Character;
                    actiontarget.ShouldSetTarget = true;
                    petTrimmedOffDiv[petToTrim.Type] = true;
                    _lastPetTrimDivertOffTime[petToTrim.Type] = Time.NormalTime;
                    _lastTrimTime = Time.NormalTime;
                    return true;
                }
            }

            return false;
        }

        protected bool PetAggDefTrimmer(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!IsSettingEnabled("AggDefTrimmer") || !CanLookupPetsAfterZone() || !CanTrim()) { return false; }

            Pet petToTrim = FindPetThat(CanAggDefTrim);                
            if (petToTrim != null)
            {
                actiontarget.Target = petToTrim.Character;
                actiontarget.ShouldSetTarget = true;
                petTrimmedAggDef[petToTrim.Type] = true;
                _lastTrimTime = Time.NormalTime;
                return true;
            }
            return false;
        }

        protected bool PetAggressiveTrimmer(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (!IsSettingEnabled("TauntTrimmer") || !CanLookupPetsAfterZone() || !CanTrim()) { return false; }

            Pet petToTrim = FindPetThat(CanTauntTrim);

            if (petToTrim != null)
            {
                if (petToTrim.Type == PetType.Support) { return false; }

                actiontarget = (petToTrim.Character, true);
                attackPetTrimmedAggressive = true;
                _lastTrimTime = Time.NormalTime;
                return true;
            }
            return false;
        }

        protected bool PetSpawner(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (PetSpawner(PetsList.Pets, spell, fightingTarget, ref actionTarget))
            {
                ResetTrimmers();
                return true;
            }
            return false;
        }

        protected virtual bool PetSpawnerItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return PetSpawnerItem(PetsList.Pets, item, fightingTarget, ref actionTarget);
        }

        protected bool PetTargetBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return PetTargetBuff(spell.Nanoline, PetType.Attack, spell, fightingTarget, ref actionTarget)
                || PetTargetBuff(spell.Nanoline, PetType.Support, spell, fightingTarget, ref actionTarget);
        }

        protected bool ShieldOfTheObedientServant(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("BuffPets") || !CanLookupPetsAfterZone()) { return false; }

            return FindPetThat(pet => !HasBuffNanoLine(NanoLine.ShieldoftheObedientServant, pet.Character)) != null;
        }

        protected bool PetDamage(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("BuffPets") || !CanLookupPetsAfterZone()) { return false; }

            if (IsSettingEnabled("AuraDamage")) { return false; }

            Pet petToBuff = FindPetThat(pet => !pet.Character.Buffs.Contains(NanoLine.DamageBuffs_LineA)
                && (pet.Type == PetType.Attack || pet.Type == PetType.Support));

            if (petToBuff != null)
            {
                spell.Cast(petToBuff.Character, true);
            }

            return false;
        }

        protected bool MastersBidding(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("BuffPets") || !CanLookupPetsAfterZone()) { return false; }

            Pet petToBuff = FindPetThat(pet => !pet.Character.Buffs.Contains(NanoLine.SiphonBox683)
                && (pet.Type == PetType.Attack || pet.Type == PetType.Support));

            if (petToBuff != null)
            {
                spell.Cast(petToBuff.Character, true);
            }

            return false;
        }

        protected bool CanTrim()
        {
            return _lastTrimTime + DelayBetweenTrims < Time.NormalTime;
        }

        protected bool CanDivertOffTrim(Pet pet)
        {
            return _lastPetTrimDivertOffTime[pet.Type] + DelayBetweenDiverTrims < Time.NormalTime || !petTrimmedOffDiv[pet.Type];
        }

        protected bool CanDivertHpTrim(Pet pet)
        {
            return _lastPetTrimDivertHpTime[pet.Type] + DelayBetweenDiverTrims < Time.NormalTime || !petTrimmedHpDiv[pet.Type];
        }


        protected bool CanAggDefTrim(Pet pet)
        {
            return !petTrimmedAggDef[pet.Type];
        }

        protected bool CanTauntTrim(Pet pet)
        {
            return pet.Type == PetType.Attack && !attackPetTrimmedAggressive;
        }

        private bool CanPerkBox(Pet pet)
        {
            return !pet.Character.Buffs.Any(buff => buff.Nanoline == NanoLine.GadgeteerPetProcs);
        }

        private void ResetTrimmers()
        {
            attackPetTrimmedAggressive = false;
            petTrimmedOffDiv[PetType.Attack] = false;
            petTrimmedOffDiv[PetType.Support] = false;
            petTrimmedHpDiv[PetType.Attack] = false;
            petTrimmedHpDiv[PetType.Support] = false;
            petTrimmedAggDef[PetType.Attack] = false;
            petTrimmedAggDef[PetType.Support] = false;
        }

        private void OnZoned(object s, EventArgs e)
        {

            ResetTrimmers();
        }

        protected override void OnUpdate(float deltaTime)
        {
            SynchronizePetCombatStateWithOwner();

            base.OnUpdate(deltaTime);

            if (!IsSettingEnabled("AuraShield"))
            {
                CancelBuffs(RelevantNanos.AuraShield);
            }

            if (!IsSettingEnabled("AuraDamage"))
            {
                CancelBuffs(RelevantNanos.AuraDamage);
            }

            if (!IsSettingEnabled("AuraArmor"))
            {
                CancelBuffs(RelevantNanos.AuraArmor);
            }

            if (!IsSettingEnabled("AuraReflect"))
            {
                CancelBuffs(RelevantNanos.AuraReflect);
            }

            CancelBuffs(IsSettingEnabled("ShieldRipper") ? RelevantNanos.Blinds : RelevantNanos.ShieldRippers);
            CancelHostileAuras(RelevantNanos.Blinds);
            CancelHostileAuras(RelevantNanos.ShieldRippers);
        }

        protected bool ShouldCancelHostileAuras()
        {
            return Time.NormalTime - _lastCombatTime > 5;
        }

        private static class RelevantNanos
        {
            public static readonly int[] PerkTauntBox = { 229131, 229130, 229129, 229128, 229127, 229126 };
            public static readonly int[] PerkSiphonBox = { 229657, 229656, 229655, 229654 };
            public static readonly int[] PerkChaoticEnergy = { 227787 };
            public const int CompositeAttribute = 223372;
            public const int CompositeNano = 223380;
            public const int MastersBidding = 268171;
            public const int CompositeUtility = 287046;
            public const int CompositeRanged = 223348;
            public const int CompositeRangedSpec = 223364;
            public const int SympatheticReactiveCocoon = 154550;
            public const int IntrusiveAuraCancellation = 204372;
            public const int BoostedTendons = 269463;
            public static readonly Spell[] DamageBuffLineA = Spell.GetSpellsForNanoline(NanoLine.DamageBuffs_LineA).Where(spell => spell.Identity.Instance != RelevantNanos.BoostedTendons).OrderByStackingOrder().ToArray();
            public static readonly int[] ShieldRippers = { 154725, 154726, 154727, 154728 };
            public static readonly int[] Blinds = { 154715, 154716, 154717, 154718, 154719 };
            public static readonly int[] AuraShield = { 154550, 154551, 154552, 154553 };
            public static readonly int[] AuraDamage = { 154560, 154561 };
            public static readonly int[] AuraArmor = { 154562, 154563, 154564, 154565, 154566, 154567 };
            public static readonly int[] AuraReflect = { 154557, 154558, 154559 };
            public static readonly int[] PetHealing = { 116791, 116795, 116796, 116792, 116797, 116794, 116793 };
            public static readonly int PetHealingCH = 270351;
            public static readonly int[] ShieldOfObedientServant = { 270790, 202260 };
        }

        private static class RelevantTrimmers
        {
            public const int IncreaseAggressivenessLow = 154939;
            public const int IncreaseAggressivenessHigh = 154940;
            public const int DivertEnergyToOffenseLow = 88377;
            public const int DivertEnergyToOffenseHigh = 88378;
            public const int PositiveAggressiveDefensive = 88384;
            public const int DivertEnergyToHitpointsLow = 88381;
            public const int DivertEnergyToHitpointsHigh = 88382;
        }
    }
}
