﻿using AOSharp.Common.GameData;
using AOSharp.Core;
using CombatHandler.Generic;
using System.Linq;
using System.Collections.Generic;
using AOSharp.Core.Inventory;
using AOSharp.Core.UI;
using System;

namespace Desu
{
    public class TraderCombatHandler : GenericCombatHandler
    {
        public TraderCombatHandler(string pluginDir) : base(pluginDir)
        {
            settings.AddVariable("UseDamageDrain", true);
            settings.AddVariable("UseAAODrain", true);
            settings.AddVariable("UseAADDrain", true);
            settings.AddVariable("UseMyEnemy", true);
            settings.AddVariable("UseRansackDrain", true);
            settings.AddVariable("UseDepriveDrain", true);
            settings.AddVariable("UseACDrains", true);
            settings.AddVariable("UseGTH", true);
            settings.AddVariable("VolunteerSelection", 0);
            RegisterSettingsWindow("Trader Handler", "TraderSettingsView.xml");

            //LE Proc
            RegisterPerkProcessor(PerkHash.LEProcTraderRigidLiquidation, LEProc);
            RegisterPerkProcessor(PerkHash.LEProcTraderDebtCollection, LEProc);

            RegisterPerkProcessor(PerkHash.PurpleHeart, PurpleHeart);
            RegisterPerkProcessor(PerkHash.Sacrifice, Sacrifice);

            //Self Buffs
            RegisterSpellProcessor(RelevantNanos.ImprovedQuantumUncertanity, GenericBuffExcludeInnerSanctum);
            RegisterSpellProcessor(RelevantNanos.UnstoppableKiller, GenericBuff);
            RegisterSpellProcessor(RelevantNanos.UmbralWranglerPremium, GenericBuff);

            //Team Buffs
            RegisterSpellProcessor(RelevantNanos.QuantumUncertanity, TeamBuffExcludeInnerSanctum);

            //Team Nano heal (Rouse Outfit nanoline)
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoPointHeals).OrderByStackingOrder(), TeamNanoHeal);

            //GTH/Your Enemy Drains
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.NanoDrain_LineB), GrandTheftHumidity);
            RegisterSpellProcessor(RelevantNanos.MyEnemiesEnemyIsMyFriend, MyEnemy);

            //AAO/AAD/Damage Drains
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderAADDrain).OrderByStackingOrder(), AADDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderAAODrain).OrderByStackingOrder(), AAODrain);
            RegisterSpellProcessor(RelevantNanos.DivestDamage, DamageDrain);

            //Deprive/Ransack Drains
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderSkillTransferTargetDebuff_Deprive), DepriveDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderSkillTransferTargetDebuff_Ransack), RansackDrain);

            //AC Drains/Debuffs
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderDebuffACNanos).OrderByStackingOrder(), TraderACDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderACTransferTargetDebuff_Draw).OrderByStackingOrder(), TraderACDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.TraderACTransferTargetDebuff_Siphon).OrderByStackingOrder(), TraderACDrain);
            RegisterSpellProcessor(Spell.GetSpellsForNanoline(NanoLine.DebuffNanoACHeavy).OrderByStackingOrder(), TraderACDrain);
        }

        private bool PurpleHeart(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSelected(VolunteerSelection.PURPLE_HEART))
            {
                return false;
            }
            return PerkCondtionProcessors.TeamHealPerk(perk, fightingTarget, ref actionTarget);
        }

        private bool Sacrifice(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSelected(VolunteerSelection.SACRIFICE))
            {
                return false;
            }
            return PerkCondtionProcessors.DamagePerk(perk, fightingTarget, ref actionTarget);
        }

        private bool IsSelected(VolunteerSelection requestedSelection)
        {
            return (VolunteerSelection)settings["VolunteerSelection"].AsInt32() == requestedSelection;
        }

        private static class RelevantNanos
        {
            public const int QuantumUncertanity = 30745;
            public const int ImprovedQuantumUncertanity = 270808;
            public const int UnstoppableKiller = 275846;
            public const int DivestDamage = 273407;
            public const int UmbralWranglerPremium = 235291;
            public const int MyEnemiesEnemyIsMyFriend = 270714;
            public static Dictionary<NanoLine, NanoLine> DebuffToDrainLine = new Dictionary<NanoLine, NanoLine>()
            {
                {NanoLine.TraderAADDrain, NanoLine.TraderNanoTheft2},
                {NanoLine.TraderAAODrain, NanoLine.TraderNanoTheft1},
                {NanoLine.NanoDrain_LineB, NanoLine.NanoOverTime_LineB}
            };
        }

        private bool MyEnemy(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("UseMyEnemy") || fightingTarget == null || fightingTarget.FightingTarget == DynelManager.LocalPlayer)
            {
                return false;
            }

            return true;
        }

        private bool GrandTheftHumidity(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("UseGTH") || fightingTarget == null)
            {
                return false;
            }

            return true;
        }

        private bool RansackDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if(!ToggledDebuff("UseRansackDrain", spell, NanoLine.TraderSkillTransferTargetDebuff_Ransack, fightingTarget, ref actionTarget))
            {
                return false;
            }

            if(DynelManager.LocalPlayer.Buffs.Find(NanoLine.TraderSkillTransferCasterBuff_Ransack, out Buff buff))
            {
                if(buff.RemainingTime > 5)
                {
                    return false;
                }
            }

            return true;
        }

        private bool DepriveDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!ToggledDebuff("UseDepriveDrain", spell, NanoLine.TraderSkillTransferTargetDebuff_Deprive, fightingTarget, ref actionTarget))
            {
                return false;
            }

            if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.TraderSkillTransferCasterBuff_Deprive, out Buff buff))
            {
                if (buff.RemainingTime > 5)
                {
                    return false;
                }
            }
            return true;
        }

        private bool DamageDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return ToggledDebuff("UseDamageDrain", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool AAODrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return ToggledDebuff("UseAAODrain", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

         private bool AADDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return ToggledDebuff("UseAADDrain", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool TeamNanoHeal(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            foreach(Buff buff in DynelManager.LocalPlayer.Buffs)
            {
                if(buff.Nanoline == NanoLine.NanoPointHeals)
                {
                    return false;
                }
            }

            // Cast when any team mate is lower than 30% of nano
            if (DynelManager.LocalPlayer.IsInTeam())
            {
                SimpleChar lowNanoTeamMember = DynelManager.Characters
                    .Where(c => c.Identity != DynelManager.LocalPlayer.Identity) //Do net perk on self
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance))
                    .Where(c => c.Identity != DynelManager.LocalPlayer.Identity)
                    .Where(c => c.NanoPercent <= 30)
                    .OrderByDescending(c => c.GetStat(Stat.NumFightingOpponents))
                    .FirstOrDefault();

                if (lowNanoTeamMember != null)
                {
                    actionTarget.Target = lowNanoTeamMember;
                    return true;
                }
            }

            return false;
        }

        private bool TraderACDrain(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return ToggledDebuff("UseACDrains", spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        private bool ToggledDebuff(string settingName, Spell spell, NanoLine spellNanoLine , SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled(settingName) ||  fightingTarget == null)
            {
                return false;
            }

            return !fightingTarget.Buffs
                .Where(buff => buff.Nanoline == spellNanoLine) //Same nanoline as the spell nanoline
                .Where(buff => buff.RemainingTime > 1) //Remaining time on buff > 1 second
                .Any(); ;
        }

        private enum VolunteerSelection
        {
            SACRIFICE, PURPLE_HEART
        }
    }
}
