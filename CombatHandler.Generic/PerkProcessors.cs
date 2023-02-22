﻿using AOSharp.Common.GameData;
using AOSharp.Common.Unmanaged.Imports;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using static CombatHandler.Generic.GenericCombatHandler;
using static System.Collections.Specialized.BitVector32;

namespace CombatHandler.Generic
{
    public class PerkCondtionProcessors
    {
        private static double _timer = 0f;

        public static GenericPerkConditionProcessor GetPerkConditionProcessor(PerkAction perkAction)
        {
            PerkHash perkHash = perkAction.Hash;
            PerkType perkType = PerkTypes.GetPerkType(perkHash);

            switch (perkType)
            {
                //Chat.WriteLine("Attempt to register custom perk processor without defintion. Perk name: " + perkAction.Name);
                case PerkType.Custom:
                    if (!CustomProcessor.ContainsKey(perkHash)) { return null; }
                    return CustomProcessor[perkHash];
                case PerkType.SelfBuff:
                    return BuffPerk;
                case PerkType.SelfHeal:
                    return SelfHeal;
                case PerkType.SelfNano:
                    return SelfNano;
                case PerkType.TeamHeal:
                    return TeamHeal;
                case PerkType.TeamNano:
                    return TeamNano;
                case PerkType.TargetDamage:
                    return TargetedDamagePerk;
                case PerkType.DamageBuff:
                    return DamageBuffPerk;
                case PerkType.CombatBuff:
                    return CombatBuffPerk;
                case PerkType.PetBuff:
                case PerkType.PetHeal:
                case PerkType.LEProc:
                case PerkType.Cleanse:
                    return CleansePerk;
                case PerkType.Disabled:
                case PerkType.Unknown:
                    return null;
                default:
                    //Chat.WriteLine("Attempt to register unknown perk type for perk name: " + perkAction.Name);
                    return null;
            }
        }

        private static Dictionary<PerkHash, GenericPerkConditionProcessor> CustomProcessor = new Dictionary<PerkHash, GenericPerkConditionProcessor>()
        {
            {PerkHash.InstallExplosiveDevice, InstallExplosiveDevice },
            {PerkHash.InstallNotumDepletionDevice, InstallNotumDepletionDevice },
        };

        private static bool InstallExplosiveDevice(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            return ShouldInstallPrimedDevice(fightingTarget, RelevantEffects.ThermalPrimerBuff);
        }

        private static bool InstallNotumDepletionDevice(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            return ShouldInstallPrimedDevice(fightingTarget, RelevantEffects.SuppressivePrimerBuff);
        }

        private static bool ShouldInstallPrimedDevice(SimpleChar fightingTarget, int primerBuffId)
        {
            if (fightingTarget == null) { return false; }

            if (fightingTarget.Buffs.Find(primerBuffId, out Buff primerBuff))
                if (primerBuff.RemainingTime > 10) //Only install device if it will trigger before primer expires
                {
                    return true;
                }

            return false;
        }


        public static bool DamageBuffPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            return true;
        }

        public static bool BattleGroupHealPerk1(PerkAction perkAction)
        {
            return perkAction.IsAvailable;
        }

        public static bool BattleGroupHealPerk2(PerkAction perkAction)
        {
            PerkAction.Find("Battlegroup Heal 1", out PerkAction _bgHeal1Team);

            if (_bgHeal1Team?.IsAvailable == false && _bgHeal1Team?.IsExecuting == false)
                return perkAction.IsAvailable;

            return false;
        }

        public static bool BattleGroupHealPerk3(PerkAction perkAction)
        {
            PerkAction.Find("Battlegroup Heal 1", out PerkAction _bgHeal1Team);
            PerkAction.Find("Battlegroup Heal 2", out PerkAction _bgHeal2Team);

            if (_bgHeal1Team?.IsAvailable == false && _bgHeal1Team?.IsExecuting == false && _bgHeal2Team?.IsAvailable == false && _bgHeal2Team?.IsExecuting == false)
                return perkAction.IsAvailable;

            return false;
        }

        public static bool BattleGroupHealPerk4(PerkAction perkAction)
        {
            PerkAction.Find("Battlegroup Heal 1", out PerkAction _bgHeal1Team);
            PerkAction.Find("Battlegroup Heal 2", out PerkAction _bgHeal2Team);
            PerkAction.Find("Battlegroup Heal 3", out PerkAction _bgHeal3Team);

            if (_bgHeal1Team?.IsAvailable == false && _bgHeal1Team?.IsExecuting == false && _bgHeal2Team?.IsAvailable == false && _bgHeal2Team?.IsExecuting == false
                && _bgHeal3Team?.IsAvailable == false && _bgHeal3Team?.IsExecuting == false)
                return perkAction.IsAvailable;

            return false;
        }

        public static bool LeadershipPerk(PerkAction perkAction)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perkAction.Name) { return false; }
            }

            return perkAction.IsAvailable;
        }
        public static bool GovernancePerk(PerkAction perkAction)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perkAction.Name) { return false; }
            }

            if (PerkAction.Find("Leadership", out PerkAction _leadership))
            {
                if (_leadership?.IsAvailable == false && _leadership?.IsExecuting == false)
                    return perkAction.IsAvailable;
            }

            return false;
        }
        public static bool TheDirectorPerk(PerkAction perkAction)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perkAction.Name) { return false; }
            }

            if (PerkAction.Find("Leadership", out PerkAction _leadership) && PerkAction.Find("Governance", out PerkAction _governance))
            {
                if (_leadership?.IsAvailable == false && _leadership?.IsExecuting == false && _governance?.IsAvailable == false && _governance?.IsExecuting == false)
                    return perkAction.IsAvailable;
            }

            return false;
        }

        public static bool VolunteerPerk(PerkAction perkAction, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perkAction.Name) { return false; }
            }

            if (Time.NormalTime < _timer + 363f) { return false; }

            _timer = Time.NormalTime;
            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = true;
            return perkAction.IsAvailable;
        }

        public static bool BuffPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perkAction.Name) { return false; }
            }

            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = true;
            return true;
        }


        public static bool CombatBuffPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perkAction.Name) { return false; }
            }

            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = true;
            return true;
        }

        public static bool StarFallPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || (fightingTarget.MaxHealth < 1000000 && fightingTarget.HealthPercent < 5)) { return false; }

            if (PerkAction.Find(PerkHash.Combust, out PerkAction combust) && !combust.IsAvailable) { return false; }

            actionTarget.ShouldSetTarget = true;
            return DamagePerk(perkAction, fightingTarget, ref actionTarget);
        }
        public static bool QuickShotPerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || (fightingTarget.MaxHealth < 1000000 && fightingTarget.HealthPercent < 5)) { return false; }

            if (PerkAction.Find("Double Shot", out PerkAction doubleShot) && !doubleShot.IsAvailable) { return false; }

            actionTarget.ShouldSetTarget = true;
            return DamagePerk(perkAction, fightingTarget, ref actionTarget);
        }
        public static bool TargetedDamagePerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            actionTarget.ShouldSetTarget = true;
            return DamagePerk(perkAction, fightingTarget, ref actionTarget);
        }

        public static bool DamagePerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || (fightingTarget.MaxHealth < 1000000 && fightingTarget.HealthPercent < 5)) { return false; }

            if (perkAction.Name == "Unhallowed Wrath" || perkAction.Name == "Spectator Wrath" || perkAction.Name == "Righteous Wrath")
                if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Skill2hEdged)) { return false; }

            return true;
        }

        public static bool CleansePerk(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root)
            || !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Snare)
            || !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Mezz)
            || !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.DemotivationalSpeeches))
            { return false; }

            foreach (Buff buff in DynelManager.LocalPlayer.Buffs.AsEnumerable())
            {
                if (buff.Name == perkAction.Name) { return false; }
            }

            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = true;
            return true;
        }

        public delegate bool GenericPerkConditionProcessor(PerkAction perkAction, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget);

        private static class RelevantEffects
        {
            public const int ThermalPrimerBuff = 209835;
            public const int SuppressivePrimerBuff = 209834;
        }
    }
}
