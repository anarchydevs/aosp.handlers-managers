﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.UI.Options;
using CombatHandler.Generic;
using System;

namespace Desu
{
    public class NTCombatHandler : GenericCombatHandler
    {
        private Menu _menu;

        public NTCombatHandler()
        {
            //Perks
            RegisterPerkProcessor(PerkHash.HostileTakeover, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ChaoticAssumption, TargetedDamagePerk);
            RegisterPerkProcessor(PerkHash.ProgramOverload, DamagePerk);
            RegisterPerkProcessor(PerkHash.FlimFocus, DamagePerk);
            RegisterPerkProcessor(PerkHash.Utilize, TargetedDamagePerk);

            RegisterPerkProcessor(PerkHash.BreachDefenses, TargetedDamagePerk);

            //Spells
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NullitySphereNano).OrderByStackingOrder(), NullitySphere, CombatActionPriority.High);
            RegisterSpellProcessor(RelevantNanos.NanobotAegis, NanobotAegis);
            RegisterSpellProcessor(RelevantNanos.IzgimmersWealth, IzgimmersWealth);
            RegisterSpellProcessor(RelevantNanos.Garuk, SingleTargetNuke);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DOTNanotechnicianStrainA).OrderByStackingOrder(), AiDotNuke);
            RegisterSpellProcessor(RelevantNanos.IzgimmersUltimatum, SingleTargetNuke);

            RegisterSpellProcessor(RelevantNanos.CompositeAttribute, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.CompositeNano, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.NanobotShelter, GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Psy_IntBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoOverTime_LineA).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDamageMultiplierBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NPCostBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MatCreaBuff).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.MajorEvasionBuffs).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.Fortify).OrderByStackingOrder(), GenericBuff);
            RegisterSpellProcessor(RelevantNanos.NanobotShelter, GenericBuff);

            _menu = new Menu("CombatHandler.NT", "CombatHandler.NT");
            _menu.AddItem(new MenuBool("UseAIDot", "Use AI DoT", true));
            OptionPanel.AddMenu(_menu);
        }



        private bool NanobotAegis(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;
            return DynelManager.LocalPlayer.HealthPercent < 50 && !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.NullitySphereNano);
        }

        private bool NullitySphere(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;
            return DynelManager.LocalPlayer.HealthPercent < 50 && !DynelManager.LocalPlayer.Buffs.Contains(RelevantNanos.NanobotAegis);
        }

        private bool SingleTargetNuke(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null)
                return false;

            return true;
        }

        private bool IzgimmersWealth(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            actionTarget.ShouldSetTarget = false;

            if (fightingTarget == null)
                return false;

            if (DynelManager.LocalPlayer.MissingNano < 20000 && DynelManager.LocalPlayer.NanoPercent > 5)
                return false;

            return true;
        }

        private bool AiDotNuke(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!_menu.GetBool("UseAIDot"))
                return false;

            if (fightingTarget == null)
                return false;

            if (fightingTarget.Health < 80000)
                return false;

            if (fightingTarget.Buffs.Find(spell.Identity.Instance, out Buff buff) && buff.RemainingTime > 5)
                return false;

            return true;
        }

        private static class RelevantNanos
        {
            public const int NanobotAegis = 302074;
            public const int IzgimmersWealth = 275024;
            public const int IzgimmersUltimatum = 218168;
            public const int Garuk = 275692;

            //Buffs
            public static readonly int[] NanobotShelter = { 273388, 263265 };
            public static readonly int CompositeAttribute = 223372;
            public static readonly int CompositeNano = 223380;
        }
    }
}
