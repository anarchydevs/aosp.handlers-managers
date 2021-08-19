﻿using System;
using System.Collections.Generic;
using System.Linq;
using AOSharp.Character;
using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using Character.State;

using static CombatHandler.Generic.PerkCondtionProcessors;

namespace CombatHandler.Generic
{
    public class GenericCombatHandler : AOSharp.Core.Combat.CombatHandler
    {
        private const float PostZonePetCheckBuffer = 5;
        public int EvadeCycleTimeoutSeconds = 180;

        private double _lastPetSyncTime = Time.NormalTime;
        protected double _lastZonedTime = Time.NormalTime;
        protected double _lastCombatTime = double.MinValue;

        private string pluginDir = "";

        protected Settings settings;


        public static readonly List<string> AoeRootSnareSpamTargets = new List<string>() { "Flaming Vengeance", "Hand of the Colonel" };

        protected static HashSet<string> debuffTargetsToIgnore = new HashSet<string>
        {
                    "Awakened Xan",
                    "Altar of Torture",
                    "Altar of Purification",
                    "Calan-Cur",
                    "Spirit of Judgement",
                    "Wandering Spirit",
                    "Altar of Torture",
                    "Altar of Purification",
                    "Unicorn Coordinator Magnum Blaine",
                    "Xan Spirit",
                    "Watchful Spirit",
                    "Amesha Vizaresh",
                    "Guardian Spirit of Purification",
                    "Tibor 'Rocketman' Nagy",
                    "One Who Obeys Precepts",
                    "The Retainer Of Ergo",
                    "Green Tower",
                    "Blue Tower",
                    "Alien Cocoon",
                    "Outzone Supplier",
                    "Hollow Island Weed",
                    "Sheila Marlene",
                    "Unicorn Advance Sentry",
                    "Unicorn Technician",
                    "Basic Tools Merchant",
                    "Container Supplier",
                    "Basic Quality Pharmacist",
                    "Basic Quality Armorer",
                    "Basic Quality Weaponsdealer",
                    "Tailor",
                    "Unicorn Commander Rufus",
                    "Ergo, Inferno Guardian of Shadows",
                    "Unicorn Trooper",
                    "Unicorn Squadleader",
                    "Rookie Alien Hunter",
                    "Unicorn Service Tower Alpha",
                    "Unicorn Service Tower Delta",
                    "Unicorn Service Tower Gamma",
                    "Sean Powell",
                    "Xan Spirit",
                    "Unicorn Guard",
                    "Essence Fragment",
                    "Scalding Flames",
                    "Guide",
                    "Guard",
                    "Awakened Xan"
        };


        public GenericCombatHandler(string pluginDir)
        {
            this.pluginDir = pluginDir;
            Game.TeleportEnded += TeleportEnded;
            settings = new Settings("CombatHandler");

            RegisterPerkProcessors();
            RegisterPerkProcessor(PerkHash.Limber, Limber, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.DanceOfFools, DanceOfFools, CombatActionPriority.High);

            RegisterSpellProcessor(RelevantNanos.FountainOfLife, FountainOfLife);

            RegisterItemProcessor(RelevantItems.ReflectGraft, RelevantItems.ReflectGraft, ReflectGraft);

            RegisterItemProcessor(RelevantItems.FlurryOfBlowsLow, RelevantItems.FlurryOfBlowsHigh, DamageItem);

            RegisterItemProcessor(RelevantItems.StrengthOfTheImmortal, RelevantItems.StrengthOfTheImmortal, DamageItem);
            RegisterItemProcessor(RelevantItems.MightOfTheRevenant, RelevantItems.MightOfTheRevenant, DamageItem);
            RegisterItemProcessor(RelevantItems.BarrowStrength, RelevantItems.BarrowStrength, DamageItem);

            RegisterItemProcessor(RelevantItems.DreadlochEnduranceBooster, RelevantItems.DreadlochEnduranceBooster, EnduranceBooster, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.DreadlochEnduranceBoosterNanomageEdition, RelevantItems.DreadlochEnduranceBoosterNanomageEdition, EnduranceBooster, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.WitheredFlesh, RelevantItems.WitheredFlesh, WithFlesh, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.DesecratedFlesh, RelevantItems.DesecratedFlesh, DescFlesh, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.AssaultClassTank, RelevantItems.AssaultClassTank, AssaultClass, CombatActionPriority.High);

            RegisterItemProcessor(RelevantItems.MeteoriteSpikes, RelevantItems.MeteoriteSpikes, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.LavaCapsule, RelevantItems.LavaCapsule, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.HSR1, RelevantItems.HSR2, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.KizzermoleGumboil, RelevantItems.KizzermoleGumboil, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.SteamingHotCupOfEnhancedCoffee, RelevantItems.SteamingHotCupOfEnhancedCoffee, Coffee);
            RegisterItemProcessor(RelevantItems.GnuffsEternalRiftCrystal, RelevantItems.GnuffsEternalRiftCrystal, DamageItem);

            RegisterItemProcessor(RelevantItems.UponAWaveOfSummerLow, RelevantItems.UponAWaveOfSummerHigh, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.BlessedWithThunderLow, RelevantItems.BlessedWithThunderHigh, TargetedDamageItem);
            RegisterItemProcessor(RelevantItems.FlowerOfLifeLow, RelevantItems.FlowerOfLifeHigh, FlowerOfLife);

            RegisterItemProcessor(RelevantItems.HealthAndNanoStimLow, RelevantItems.HealthAndNanoStimHigh, HealthAndNanoStim, CombatActionPriority.High);
            RegisterItemProcessor(RelevantItems.RezCan, RelevantItems.RezCan, UseRezCan);
            RegisterItemProcessor(RelevantItems.ExperienceStim, RelevantItems.ExperienceStim, ExperienceStim);

            RegisterItemProcessor(RelevantItems.PremSitKit, RelevantItems.PremSitKit, SitKit);
            RegisterItemProcessor(RelevantItems.SitKit1, RelevantItems.SitKit100, SitKit);
            RegisterItemProcessor(RelevantItems.SitKit100, RelevantItems.SitKit200, SitKit);
            RegisterItemProcessor(RelevantItems.SitKit200, RelevantItems.SitKit300, SitKit);

            RegisterItemProcessor(RelevantItems.FreeStim1, RelevantItems.FreeStim50, UseFreeStim);
            RegisterItemProcessor(RelevantItems.FreeStim50, RelevantItems.FreeStim100, UseFreeStim);
            RegisterItemProcessor(RelevantItems.FreeStim100, RelevantItems.FreeStim200, UseFreeStim);
            RegisterItemProcessor(RelevantItems.FreeStim200, RelevantItems.FreeStim200, UseFreeStim);
            RegisterItemProcessor(RelevantItems.FreeStim200, RelevantItems.FreeStim300, UseFreeStim);


            RegisterItemProcessor(RelevantItems.AmmoBoxArrows, RelevantItems.AmmoBoxArrows, AmmoBoxArrows);
            RegisterItemProcessor(RelevantItems.AmmoBoxBullets, RelevantItems.AmmoBoxBullets, AmmoBoxBullets);
            RegisterItemProcessor(RelevantItems.AmmoBoxEnergy, RelevantItems.AmmoBoxEnergy, AmmoBoxEnergy);
            RegisterItemProcessor(RelevantItems.AmmoBoxShotgun, RelevantItems.AmmoBoxShotgun, AmmoBoxShotgun);

            RegisterSpellProcessor(RelevantNanos.CompositeNano, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeAttribute, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeUtility, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeMartialProwess, GenericBuff);

            if (CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Blunt1H ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Blunt1HAndEdged1H ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Blunt1HAndEnergy ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Blunt1HAndPiercing ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Blunt2H ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Energy ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Edged1H ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Edged1HAndPiercing ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Edged2H ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Blunt1HAndEdged1H ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Piercing ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Fists)
            {
                //We are melee
                RegisterSpellProcessor(RelevantNanos.CompositeMartial, GenericBuffExcludeInnerSanctum);
                RegisterSpellProcessor(RelevantNanos.CompositeMelee, GenericBuff);
                RegisterSpellProcessor(RelevantNanos.CompositePhysicalSpecial, GenericBuff);
            }


            if (CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Pistol ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.PistolAndAssaultRifle ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.PistolAndShotgun ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Smg ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Bow ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Shotgun ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.PistolAndShotgun ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.HeavyWeapons ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Grenade ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.AssaultRifle ||
                CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == CharacterWieldedWeapon.Rifle)
            {
                //We are ranged
                RegisterSpellProcessor(RelevantNanos.CompositeRanged, GenericBuff);
                RegisterSpellProcessor(RelevantNanos.CompositeRangedSpecial, GenericBuff);
            }

            switch (DynelManager.LocalPlayer.Breed)
            {
                case Breed.Solitus:
                    break;
                case Breed.Opifex:
                    break;
                case Breed.Nanomage:
                    break;
                case Breed.Atrox:
                    break;
            }

            Game.TeleportEnded += OnZoned;
        }

        //.Where(c => spell.StackingOrder > c.Buffs.FirstOrDefault(x => x.Nanoline == spell.Nanoline).StackingOrder)

        protected void RegisterSettingsWindow(string settingsName, string xmlName)
        {
            SettingsController.RegisterSettingsWindow(settingsName, pluginDir + "\\UI\\" + xmlName, settings);
        }

        private void OnZoned(object s, EventArgs e)
        {
            _lastZonedTime = Time.NormalTime;
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            if (DynelManager.LocalPlayer.IsAttacking || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) > 0)
            {
                _lastCombatTime = Time.NormalTime;
            }
        }


        #region Perks

        private PerkConditionProcessor ToPerkConditionProcessor(GenericPerkConditionProcessor genericPerkConditionProcessor)
        {
            return (PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget) => genericPerkConditionProcessor(perkAction, fightingTarget, ref actionTarget);
        }

        protected void RegisterPerkProcessors()
        {
            PerkAction.List.ForEach(perkAction => RegisterPerkAction(perkAction));
        }

        private void RegisterPerkAction(PerkAction perkAction)
        {
            GenericPerkConditionProcessor perkConditionProcessor = PerkCondtionProcessors.GetPerkConditionProcessor(perkAction);

            if (perkConditionProcessor != null)
            {
                RegisterPerkProcessor(perkAction.Hash, ToPerkConditionProcessor(perkConditionProcessor));
            }
        }

        protected bool LEProc(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perk.Name)
                {
                    return false;
                }
            }
            return true;
        }

        private bool Limber(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!DynelManager.LocalPlayer.IsAttacking)
                return false;

            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
                return false;

            if (DynelManager.LocalPlayer.Buffs.Find(RelevantNanos.DanceOfFools, out Buff dof) && dof.RemainingTime > 12.5f)
                return false;

            // stop cycling if we haven't fought anything for over 10 minutes
            if (Time.NormalTime - _lastCombatTime > EvadeCycleTimeoutSeconds)
                return false;

            return true;
        }

        private bool DanceOfFools(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!DynelManager.LocalPlayer.IsAttacking)
                return false;

            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
                return false;

            if (!DynelManager.LocalPlayer.Buffs.Find(RelevantNanos.Limber, out Buff limber) || limber.RemainingTime > 12.5f)
                return false;

            // stop cycling if we haven't fought anything for over 10 minutes
            if (Time.NormalTime - _lastCombatTime > EvadeCycleTimeoutSeconds)
                return false;

            return true;
        }

        #endregion

        #region Instanced Logic

        protected bool HealOverTimeBuff(string toggleName, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return ToggledBuff(toggleName, spell, fightingTarget, ref actionTarget);
        }

        protected bool BuffInitDoc(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || !CanCast(spell))
            {
                return false;
            }

            if (SpellChecksPlayer(spell))
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => SpellChecksOther(spell, c))
                    .Where(c => c.Profession == Profession.Doctor || c.Profession == Profession.NanoTechnician)
                    .FirstOrDefault();

                if (teamMemberWithoutBuff != null)
                {
                    actionTarget.Target = teamMemberWithoutBuff;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        protected bool BuffInitEngi(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || !CanCast(spell))
            {
                return false;
            }

            if (SpellChecksPlayer(spell))
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => SpellChecksOther(spell, c))
                    .Where(c => c.Profession != Profession.Doctor && c.Profession != Profession.NanoTechnician)
                    .Where(c => CharacterState.GetWeaponType(c.Identity) == CharacterWeaponType.RANGED)
                    .FirstOrDefault();

                if (teamMemberWithoutBuff != null)
                {
                    actionTarget.Target = teamMemberWithoutBuff;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }
        protected bool BuffInitSol(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || !CanCast(spell))
            {
                return false;
            }

            if (SpellChecksPlayer(spell))
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => SpellChecksOther(spell, c))
                    .Where(c => c.Profession != Profession.Doctor && c.Profession != Profession.NanoTechnician)
                    .FirstOrDefault();

                if (teamMemberWithoutBuff != null)
                {
                    actionTarget.Target = teamMemberWithoutBuff;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        private bool FountainOfLife(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= 30)
            {
                actiontarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => c.IsAlive)
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent < 30)
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actiontarget.Target = dyingTeamMember;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Logic

        protected bool GenericBuffExcludeInnerSanctum(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsInsideInnerSanctum())
            {
                return false;
            }
            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        protected bool CombatBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || !CanCast(spell))
            {
                return false;
            }

            if (SpellChecksPlayer(spell))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        protected bool ToggledDebuffTarget(string settingName, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Check if we are fighting and if debuffing is enabled
            if (fightingTarget == null || !IsSettingEnabled(settingName))
            {
                return false;
            }

            if (SpellChecksOther(spell, fightingTarget))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = fightingTarget;
                return true;
            }

            return false;
            //return !fightingTarget.Buffs.Any(buff => ShouldRefreshBuff(spell, buff));
        }

        protected bool ToggledDebuffOthersInCombat(string toggleName, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled(toggleName))
            {
                return false;
            }

            SimpleChar debuffTarget = DynelManager.Characters
                .Where(c => !debuffTargetsToIgnore.Contains(c.Name)) //Is not a quest target etc
                .Where(c => !c.IsPlayer).Where(c => !c.IsPet) //Is not player or a pet
                .Where(c => c.IsAttacking) //Is in combat
                .Where(c => !c.Buffs.Contains(301844)) // doesn't have ubt in ncu
                .Where(c => c.IsValid).Where(c => c.IsInLineOfSight).Where(c => c.IsInAttackRange()) //Is in range for debuff (we assume weapon range == debuff range)
                .Where(c => SpellChecksOther(spell, c)) //Needs debuff refreshed
                .FirstOrDefault();

            if (debuffTarget != null)
            {
                actionTarget = (debuffTarget, true);
                return true;
            }

            return false;
        }

        protected virtual bool TargetedDamageItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = true;
            return DamageItem(item, fightingTarget, ref actionTarget);
        }

        protected virtual bool ReflectGraft(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return !DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)) && !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.ReflectShield);
        }

        protected virtual bool DamageItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return !DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)) && fightingTarget != null && fightingTarget.IsInAttackRange();
        }

        protected bool ToggledBuff(string settingName, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled(settingName))
            {
                return false;
            }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        protected bool TeamBuffExcludeInnerSanctum(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if(IsInsideInnerSanctum())
            {
                return false;
            }

            return GenericBuff(spell, fightingTarget, ref actionTarget);
        }

        private bool BuffSpecialAttack(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget, Func<SpecialAttacks, bool> specialAttackCheck)
        {
            if (fightingTarget != null || !CanCast(spell))
            {
                return false;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => SpellChecksOther(spell, c))
                    .Where(c => specialAttackCheck == null || specialAttackCheck.Invoke(CharacterState.GetSpecialAttacks(c.Identity)))
                    .FirstOrDefault();

                if (teamMemberWithoutBuff != null)
                {
                    actionTarget.Target = teamMemberWithoutBuff;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }
            else
            {
                if (SpellChecksPlayer(spell))
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = DynelManager.LocalPlayer;
                    return true;
                }
            }

            return false;
        }

        protected bool TeamBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || !CanCast(spell) || spell.Name.Contains("Veteran"))
            {
                return false;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => SpellChecksOther(spell, c))
                    .FirstOrDefault();

                if (teamMemberWithoutBuff != null)
                {
                    actionTarget.Target = teamMemberWithoutBuff;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        protected bool GenericBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || !CanCast(spell) || spell.Name.Contains("Veteran"))
            {
                return false;
            }

            if (SpellChecksPlayer(spell))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        protected bool BuffAttackType(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget, CharacterWeaponType supportedWeaponType)
        {
            if (fightingTarget != null || !CanCast(spell))
            {
                return false;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => SpellChecksOther(spell, c))
                    .Where(c => supportedWeaponType == CharacterWeaponType.UNAVAILABLE || CharacterState.GetWeaponType(c.Identity) == supportedWeaponType)
                    .FirstOrDefault();

                if (teamMemberWithoutBuff != null)
                {
                    actionTarget.Target = teamMemberWithoutBuff;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }
            else
            {
                if (SpellChecksPlayer(spell) && CharacterState.GetWeaponType(DynelManager.LocalPlayer.Identity) == supportedWeaponType)
                {
                    actionTarget.Target = DynelManager.LocalPlayer;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        protected bool BuffWeaponType(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget, CharacterWieldedWeapon supportedWeaponType1, CharacterWieldedWeapon supportedWeaponType2, CharacterWieldedWeapon supportedWeaponType3, CharacterWieldedWeapon supportedWeaponType4)
        {
            if (fightingTarget != null || !CanCast(spell))
            {
                return false;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => SpellChecksOther(spell, c))
                    .Where(c => CharacterState.GetWieldedWeaponOther(c) == supportedWeaponType1 || CharacterState.GetWieldedWeaponOther(c) == supportedWeaponType2 || CharacterState.GetWieldedWeaponOther(c) == supportedWeaponType3 || CharacterState.GetWieldedWeaponOther(c) == supportedWeaponType4)
                    .FirstOrDefault();

                if (teamMemberWithoutBuff != null)
                {
                    actionTarget.Target = teamMemberWithoutBuff;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }
            else
            {
                if (SpellChecksPlayer(spell) && (CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == supportedWeaponType1 || CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == supportedWeaponType2 || CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == supportedWeaponType3 || CharacterState.GetWieldedWeapon(DynelManager.LocalPlayer) == supportedWeaponType4))
                {
                    actionTarget.Target = DynelManager.LocalPlayer;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        protected bool RangedBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffAttackType(spell, fightingTarget, ref actionTarget, CharacterWeaponType.RANGED);
        }

        protected bool MeleeBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffAttackType(spell, fightingTarget, ref actionTarget, CharacterWeaponType.MELEE);
        }

        protected bool PistolBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Pistol, CharacterWieldedWeapon.PistolAndAssaultRifle, CharacterWieldedWeapon.PistolAndShotgun, CharacterWieldedWeapon.Bandaid);
        }

        protected bool BurstBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return BuffSpecialAttack(spell, fightingTarget, ref actionTarget, specialAttacks => specialAttacks.HasBurst);
        }
        #endregion

        #region Items

        private bool UseRezCan(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            actiontarget.ShouldSetTarget = false;

            return !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid);
        }

        private bool UseFreeStim(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            actiontarget.ShouldSetTarget = true;

            return (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root) || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Snare)) && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item));

        }

        protected bool TauntTool(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || !IsSettingEnabled("UseTauntTool"))
            {
                return false;
            }

            if (TauntTools.CanUseTauntTool())
            {
                actionTarget.Target = fightingTarget;
                actionTarget.ShouldSetTarget = true;
            }

            return false;
        }

        private bool SitKit(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment))
                return false;

            if (DynelManager.LocalPlayer.HealthPercent > 65 && DynelManager.LocalPlayer.NanoPercent > 65)
                return false;

            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = true;
            return true;
        }


        private bool ExperienceStim(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid))
                return false;

            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = false;
            return true;

        }

        private bool FlowerOfLife(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (fightingtarget == null)
                return false;

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)))
                return false;

            int approximateHealing = item.QualityLevel * 10;

            return DynelManager.LocalPlayer.MissingHealth > approximateHealing;
        }

        private bool HealthAndNanoStim(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)) || DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) >= 8)
                return false;

            if (DynelManager.LocalPlayer.Buffs.Contains(280470) || DynelManager.LocalPlayer.Buffs.Contains(258231))
                return false;

            actiontarget.ShouldSetTarget = true;
            actiontarget.Target = DynelManager.LocalPlayer;


            return DynelManager.LocalPlayer.HealthPercent < 80 || DynelManager.LocalPlayer.NanoPercent < 80;
        }

        private bool AmmoBoxBullets(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            Item bulletsammo = Inventory.Items
                .Where(c => c.Name.Contains("Bullets"))
                .Where(c => !c.Name.Contains("Crate"))
                .FirstOrDefault();

            actiontarget.ShouldSetTarget = false;

            return bulletsammo == null && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing);
        }

        private bool AmmoBoxEnergy(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            Item energyammo = Inventory.Items
                .Where(c => c.Name.Contains("Energy"))
                .Where(c => !c.Name.Contains("Crate"))
                .FirstOrDefault();

            actiontarget.ShouldSetTarget = false;

            return energyammo == null && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing);
        }

        private bool AmmoBoxShotgun(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            Item shotgunammo = Inventory.Items
                .Where(c => c.Name.Contains("Shotgun"))
                .Where(c => !c.Name.Contains("Crate"))
                .FirstOrDefault();

            actiontarget.ShouldSetTarget = false;

            return shotgunammo == null && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing);
        }

        private bool AmmoBoxArrows(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            Item arrowammo = Inventory.Items
                .Where(c => c.Name.Contains("Arrows"))
                .Where(c => !c.Name.Contains("Crate"))
                .FirstOrDefault();

            actiontarget.ShouldSetTarget = false;

            return arrowammo == null && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing);
        }

        private bool EnduranceBooster(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (Inventory.Find(305476, 305476, out Item absorbdesflesh))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BodyDevelopment))
                {
                    return false;
                }
            }
            if (Inventory.Find(204698, 204698, out Item absorbwithflesh))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BodyDevelopment))
                {
                    return false;
                }
            }
            if (Inventory.Find(156576, 156576, out Item absorbassaultclass))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.DuckExp))
                {
                    return false;
                }
            }

            // don't use if skill is locked (we will add this dynamically later)
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength))
                return false;

            // don't use if we're above 40%
            if (DynelManager.LocalPlayer.HealthPercent >= 65)
                return false;

            // don't use if nothing is fighting us
            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
                return false;

            // don't use if we have another major absorb running
            // we could check remaining absorb stat to be slightly more effective
            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon))
                return false;

            return true;
        }

        private bool AssaultClass(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            // don't use if skill is locked (we will add this dynamically later)
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.DuckExp))
                return false;

            // don't use if we're above 40%
            if (DynelManager.LocalPlayer.HealthPercent >= 65)
                return false;

            // don't use if nothing is fighting us
            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
                return false;

            // don't use if we have another major absorb running
            // we could check remaining absorb stat to be slightly more effective
            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon))
                return false;

            return true;
        }

        private bool DescFlesh(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (Inventory.Find(267168, 267168, out Item enduranceabsorbenf))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength))
                {
                    return false;
                }
            }
            if (Inventory.Find(267167, 267167, out Item enduranceabsorbnanomage))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength))
                {
                    return false;
                }
            }
            if (Inventory.Find(156576, 156576, out Item absorbassaultclass))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.DuckExp))
                {
                    return false;
                }
            }

            // don't use if skill is locked (we will add this dynamically later)
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BodyDevelopment))
                return false;

            // don't use if we're above 40%
            if (DynelManager.LocalPlayer.HealthPercent >= 65)
                return false;

            // don't use if nothing is fighting us
            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
                return false;

            // don't use if we have another major absorb running
            // we could check remaining absorb stat to be slightly more effective
            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon))
                return false;

            return true;
        }

        private bool WithFlesh(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            Inventory.Find(305476, 305476, out Item absorbdesflesh);

            if (Inventory.Find(267168, 267168, out Item enduranceabsorbenf))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength))
                {
                    return false;
                }
            }
            if (Inventory.Find(267167, 267167, out Item enduranceabsorbnanomage))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength))
                {
                    return false;
                }
            }
            if (Inventory.Find(156576, 156576, out Item absorbassaultclass))
            {
                if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.DuckExp))
                {
                    return false;
                }
            }

            // don't use if skill is locked (we will add this dynamically later)
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BodyDevelopment))
                return false;

            // don't use if we're above 40%
            if (DynelManager.LocalPlayer.HealthPercent >= 65)
                return false;

            // don't use if nothing is fighting us
            if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0)
                return false;

            // don't use if we have another major absorb running
            // we could check remaining absorb stat to be slightly more effective
            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon))
                return false;

            if (absorbdesflesh != null)
                return false;

            return true;
        }

        protected virtual bool Coffee(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!DynelManager.LocalPlayer.Buffs.Contains(NanoLine.FoodandDrinkBuffs))
                return DamageItem(item, fightingTarget, ref actionTarget);

            return false;
        }
        #endregion

        #region Pets

        protected bool NoShellPetSpawner(PetType petType, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanSpawnPets(petType))
            {
                return false;
            }

            actionTarget.ShouldSetTarget = false;
            return true;
        }

        protected bool PetSpawner(Dictionary<int, PetSpellData> petData, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!petData.ContainsKey(spell.Identity.Instance))
            {
                return false;
            }

            PetType petType = petData[spell.Identity.Instance].PetType;

            //Ignore spell if we already have the shell in our inventory
            if (Inventory.Find(petData[spell.Identity.Instance].ShellId, out Item shell))
            {
                return false;
            }

            return NoShellPetSpawner(petType, spell, fightingTarget, ref actionTarget);
        }

        protected virtual bool PetSpawnerItem(Dictionary<int, PetSpellData> petData, Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("SpawnPets"))
                return false;

            if (!CanLookupPetsAfterZone())
                return false;

            if (!petData.Values.Any(x => (x.ShellId == item.LowId || x.ShellId == item.HighId) && !DynelManager.LocalPlayer.Pets.Any(p => p.Type == x.PetType)))
                return false;

            actionTarget.ShouldSetTarget = false;
            return true;
        }

        protected bool CanSpawnPets(PetType petType)
        {
            if (!IsSettingEnabled("SpawnPets") || !CanLookupPetsAfterZone() || PetAlreadySpawned(petType))
            {
                return false;
            }

            return true;
        }

        private bool PetAlreadySpawned(PetType petType)
        {
            return DynelManager.LocalPlayer.Pets.Any(x => (x.Type == PetType.Unknown || x.Type == petType));
        }

        protected Pet FindPetThat(Func<Pet, bool> Filter)
        {
            return DynelManager.LocalPlayer.Pets
                .Where(pet => pet.Character != null && pet.Character.Buffs != null)
                .Where(pet => pet.Type == PetType.Support || pet.Type == PetType.Attack || pet.Type == PetType.Heal)
                .Where(pet => Filter.Invoke(pet))
                .FirstOrDefault();
        }

        protected Pet FindPetNeedsHeal(int percent)
        {
            return DynelManager.LocalPlayer.Pets
                .Where(pet => pet.Character != null && pet.Character.Buffs != null)
                .Where(pet => pet.Type == PetType.Support || pet.Type == PetType.Attack || pet.Type == PetType.Heal)
                .Where(pet => pet.Character.HealthPercent <= percent)
                .FirstOrDefault();
        }

        protected bool CanLookupPetsAfterZone()
        {
            return Time.NormalTime > _lastZonedTime + PostZonePetCheckBuffer;
        }

        protected bool PetTargetBuff(NanoLine buffNanoLine, PetType petType, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("BuffPets") || !CanLookupPetsAfterZone())
            {
                return false;
            }

            Pet petToBuff = FindPetThat(pet => pet.Type == petType && !pet.Character.Buffs.Contains(buffNanoLine));

            if (petToBuff != null)
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = petToBuff.Character;
                return true;
            }

            return false;
        }

        protected void SynchronizePetCombatStateWithOwner()
        {
            if (CanLookupPetsAfterZone() && Time.NormalTime - _lastPetSyncTime > 1)
            {
                SynchronizePetCombatState(FindPetThat(pet => pet.Type == PetType.Attack));
                SynchronizePetCombatState(FindPetThat(pet => pet.Type == PetType.Support));

                _lastPetSyncTime = Time.NormalTime;
            }
        }

        private void SynchronizePetCombatState(Pet pet)
        {
            if (pet != null)
            {
                if (DynelManager.LocalPlayer.IsAttacking)
                {
                    if (!pet.Character.IsAttacking)
                    {
                        pet.Attack(DynelManager.LocalPlayer.FightingTarget.Identity);
                    }
                }
                else
                {
                    if (pet.Character.IsAttacking)
                    {
                        pet.Follow();
                    }
                }
            }
        }

        #endregion

        #region Checks

        protected bool HasBuffNanoLine(NanoLine nanoLine, SimpleChar target)
        {
            return target.Buffs.Contains(nanoLine);
        }


        protected bool IsAoeRootSnareSpamTarget(SimpleChar target)
        {
            return AoeRootSnareSpamTargets.Contains(target.Name);
        }

        protected bool CheckNotKeeperBeforeCast(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanCast(spell))
            {
                return false;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar teamMemberWithoutBuff = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => SpellChecksOther(spell, c))
                    .Where(c => c.Profession != Profession.Keeper)
                    .FirstOrDefault();

                if (teamMemberWithoutBuff != null)
                {
                    actionTarget.Target = teamMemberWithoutBuff;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }

            return false;
        }

        protected bool SomeoneNeedsHealing()
        {
            if (DynelManager.LocalPlayer.HealthPercent <= 85)
            {
                return true;
            }

            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => c.IsAlive)
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent <= 85)
                    .OrderBy(c => c.Profession == Profession.Doctor)
                    .OrderBy(c => c.Profession == Profession.Enforcer)
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    return true;
                }
            }

            return false;
        }

        protected bool FindMemberWithHealthBelow(int healthPercentTreshold, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= healthPercentTreshold)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar dyingTeamMember = DynelManager.Characters
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.HealthPercent <= healthPercentTreshold)
                    .OrderBy(c => c.Profession == Profession.Doctor)
                    .OrderBy(c => c.Profession == Profession.Enforcer)
                    .FirstOrDefault();

                if (dyingTeamMember != null)
                {
                    actionTarget.Target = dyingTeamMember;
                    actionTarget.ShouldSetTarget = true;
                    return true;
                }
            }
            return false;
        }

        protected bool FindPlayerWithHealthBelow(int healthPercentTreshold, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            // Prioritize keeping ourself alive
            if (DynelManager.LocalPlayer.HealthPercent <= healthPercentTreshold)
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            // Try to keep our teammates alive if we're in a team
            SimpleChar dyingTeamMember = DynelManager.Characters
                .Where(c => c.IsPlayer)
                .Where(c => c.HealthPercent <= healthPercentTreshold)
                .Where(c => c.DistanceFrom(DynelManager.LocalPlayer) < 30f)
                .OrderBy(c => c.Profession == Profession.Doctor)
                .OrderBy(c => c.Profession == Profession.Enforcer)
                .FirstOrDefault();

            if (dyingTeamMember != null)
            {
                actionTarget.Target = dyingTeamMember;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            return false;
        }

        protected bool HasNano(int nanoId)
        {
            return Spell.Find(nanoId, out Spell spell);
        }

        protected bool SpellChecksOther(Spell spell, SimpleChar fightingTarget)
        {
            if (fightingTarget.IsPlayer && !CharacterState.IsCharacterRegistered(fightingTarget.Identity))
                return false;

            if (fightingTarget.IsPlayer && !HasNCU(spell, fightingTarget))
                return false;

            if (fightingTarget.Buffs.Find(spell.Nanoline, out Buff buff))
            {
                //Don't cast if weaker than existing
                if (spell.StackingOrder < buff.StackingOrder)
                {
                    return false;
                }

                //Don't cast if greater than 10% time remaining
                if (spell.Nanoline == buff.Nanoline && buff.RemainingTime / buff.TotalTime > 0.1)
                {
                    return false;
                }

                //if (fightingTarget.Buffs.Contains(buff))
                //{
                //    return false;
                //}
            }

            return true;
        }

        protected bool SpellChecksPlayer(Spell spell)
        {
            if (DynelManager.LocalPlayer.Buffs.Find(spell.Nanoline, out Buff buff))
            {
                //Don't cast if weaker than existing
                if (spell.StackingOrder < buff.StackingOrder)
                {
                    return false;
                }

                //Don't cast if greater than 10% time remaining
                if (spell.Nanoline == buff.Nanoline && buff.RemainingTime / buff.TotalTime > 0.1)
                {
                    return false;
                }

                if (DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU))
                {
                    return false;
                }

                //if (DynelManager.LocalPlayer.Buffs.Contains(buff))
                //{
                //    return false;
                //}
            }

            if (DynelManager.LocalPlayer.RemainingNCU < spell.NCU)
            {
                return false;
            }

            return true;
        }

        protected bool CanCast(Spell spell)
        {
            return spell.Cost < DynelManager.LocalPlayer.Nano;
        }

        protected bool IsSettingEnabled(string settingName)
        {
            return settings[settingName].AsBool();
        }

        protected bool HasNCU(Spell spell, SimpleChar target)
        {
            return CharacterState.GetRemainingNCU(target.Identity) > spell.NCU;
        }

        private void TeleportEnded(object sender, EventArgs e)
        {
            _lastCombatTime = double.MinValue;
        }

        protected void CancelHostileAuras(int[] auras)
        {
            if (Time.NormalTime - _lastCombatTime > 5)
            {
                CancelBuffs(auras);
            }
        }


        protected static void CancelBuffs(int[] buffsToCancel)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs)
            {
                if (buffsToCancel.Contains(buff.Identity.Instance))
                    buff.Remove();
            }
        }

        protected bool IsInsideInnerSanctum()
        {
            return DynelManager.LocalPlayer.Buffs.Any(buff => buff.Identity.Instance == RelevantNanos.InnerSanctumDebuff);
        }

        #endregion

        #region Misc

        // This will eventually be done dynamically but for now I will implement
        // it statically so we can have it functional
        private Stat GetSkillLockStat(Item item)
        {
            switch (item.HighId)
            {
                case RelevantItems.ReflectGraft:
                    return Stat.SpaceTime;
                case RelevantItems.UponAWaveOfSummerLow:
                case RelevantItems.UponAWaveOfSummerHigh:
                    return Stat.Riposte;
                case RelevantItems.FlowerOfLifeLow:
                case RelevantItems.FlowerOfLifeHigh:
                case RelevantItems.BlessedWithThunderLow:
                case RelevantItems.BlessedWithThunderHigh:
                    return Stat.MartialArts;
                case RelevantItems.HealthAndNanoStimLow:
                case RelevantItems.HealthAndNanoStimHigh:
                    return Stat.FirstAid;
                case RelevantItems.FlurryOfBlowsLow:
                case RelevantItems.FlurryOfBlowsHigh:
                    return Stat.AggDef;
                case RelevantItems.StrengthOfTheImmortal:
                case RelevantItems.MightOfTheRevenant:
                case RelevantItems.BarrowStrength:
                    return Stat.Strength;
                case RelevantItems.MeteoriteSpikes:
                case RelevantItems.LavaCapsule:
                case RelevantItems.KizzermoleGumboil:
                    return Stat.SharpObject;
                case RelevantItems.SteamingHotCupOfEnhancedCoffee:
                    return Stat.RunSpeed;
                case RelevantItems.GnuffsEternalRiftCrystal:
                    return Stat.MapNavigation;
                case RelevantItems.HSR1:
                case RelevantItems.HSR2:
                    return Stat.Grenade;
                default:
                    throw new Exception($"No skill lock stat defined for item id {item.HighId}");
            }
        }

        private static class RelevantItems
        {
            public const int ReflectGraft = 95225;
            public const int FlurryOfBlowsLow = 85907;
            public const int FlurryOfBlowsHigh = 85908;
            public const int StrengthOfTheImmortal = 305478;
            public const int MightOfTheRevenant = 206013;
            public const int BarrowStrength = 204653;
            public const int LavaCapsule = 245990;
            public const int WitheredFlesh = 204698;
            public const int DesecratedFlesh = 305476;
            public const int AssaultClassTank = 156576;
            public const int HSR1 = 164780;
            public const int HSR2 = 164781;
            public const int KizzermoleGumboil = 245323;
            public const int SteamingHotCupOfEnhancedCoffee = 157296;
            public const int DreadlochEnduranceBooster = 267168;
            public const int DreadlochEnduranceBoosterNanomageEdition = 267167;
            public const int MeteoriteSpikes = 244204;
            public const int FlowerOfLifeLow = 70614;
            public const int FlowerOfLifeHigh = 204326;
            public const int UponAWaveOfSummerLow = 205405;
            public const int UponAWaveOfSummerHigh = 205406;
            public const int BlessedWithThunderLow = 70612;
            public const int BlessedWithThunderHigh = 204327;
            public const int GnuffsEternalRiftCrystal = 303179;
            public const int HealthAndNanoStimLow = 291043;
            public const int HealthAndNanoStimHigh = 291044;
            public const int RezCan = 301070;
            public const int ExperienceStim = 288769;
            public const int PremSitKit = 297274;
            public const int SitKit1 = 291082;
            public const int SitKit100 = 291083;
            public const int SitKit200 = 291084;
            public const int SitKit300 = 293296;
            public const int FreeStim1 = 204103;
            public const int FreeStim50 = 204104;
            public const int FreeStim100 = 204105;
            public const int FreeStim200 = 204106;
            public const int FreeStim300 = 204107;
            public const int AmmoBoxEnergy = 303138;
            public const int AmmoBoxShotgun = 303141;
            public const int AmmoBoxBullets = 303137;
            public const int AmmoBoxArrows = 303136;
        };

        private static class RelevantNanos
        {
            public const int FountainOfLife = 302907;
            public const int DanceOfFools = 210159;
            public const int Limber = 210158;
            public const int CompositeAttribute = 223372;
            public const int CompositeNano = 223380;
            public const int CompositeUtility = 287046;
            public const int CompositeMartialProwess = 302158;
            public const int CompositeMartial = 302158;
            public const int CompositeMelee = 223360;
            public const int CompositePhysicalSpecial = 215264;
            public const int CompositeRanged = 223348;
            public const int CompositeRangedSpecial = 223364;
            public const int InnerSanctumDebuff = 206387;
        }

        public class PetSpellData
        {
            public int ShellId;
            public int ShellId2;
            public PetType PetType;

            public PetSpellData(int shellId, PetType petType)
            {
                ShellId = shellId;
                PetType = petType;
            }
            public PetSpellData(int shellId, int shellId2, PetType petType)
            {
                ShellId = shellId;
                ShellId2 = shellId2;
                PetType = petType;
            }
        }
        #endregion
    }
}