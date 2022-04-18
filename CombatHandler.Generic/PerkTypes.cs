﻿using AOSharp.Common.GameData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CombatHandler.Generic
{
    class PerkTypes
    {
        public static PerkType GetPerkType(PerkHash perkHash)
        {
            if(!BY_HASH.ContainsKey(perkHash))
            {
                return PerkType.UNKNOWN;
            }

            return BY_HASH[perkHash];
        }

        private static Dictionary<PerkHash, PerkType> BY_HASH = new Dictionary<PerkHash, PerkType>()
        {
            {PerkHash.AccelerateDecayingQuarks , PerkType.TARGETED_DAMAGE},
            {PerkHash.AccessNotumSource , PerkType.NANO_HEAL},
            {PerkHash.AnnihilateNotumMolecules , PerkType.TARGETED_DAMAGE},
            {PerkHash.Antitrust , PerkType.TARGETED_DAMAGE},
            {PerkHash.ArmorPiercingShot , PerkType.TARGETED_DAMAGE},
            {PerkHash.ArouseAnger , PerkType.TARGETED_DAMAGE},
            {PerkHash.Assassinate , PerkType.TARGETED_DAMAGE},
            {PerkHash.AssumeTarget , PerkType.TARGETED_DAMAGE},
            {PerkHash.Atrophy , PerkType.TARGETED_DAMAGE},
            {PerkHash.Avalanche , PerkType.TARGETED_DAMAGE},
            {PerkHash.Awakening , PerkType.HEAL},
            {PerkHash.BalanceOfYinAndYang ,PerkType.HEAL},
            {PerkHash.BattlegroupHeal1 , PerkType.CUSTOM},
            {PerkHash.BattlegroupHeal2 , PerkType.CUSTOM},
            {PerkHash.BattlegroupHeal3 , PerkType.CUSTOM},
            {PerkHash.BattlegroupHeal4 , PerkType.CUSTOM},
            {PerkHash.Bearhug , PerkType.TARGETED_DAMAGE},
            {PerkHash.Beckoning , PerkType.TARGETED_DAMAGE},
            {PerkHash.BigSmash , PerkType.TARGETED_DAMAGE},
            {PerkHash.BioCocoon , PerkType.CUSTOM},
            {PerkHash.BioRegrowth , PerkType.HEAL},
            {PerkHash.BioRejuvenation , PerkType.HEAL},
            {PerkHash.BioShield , PerkType.SELF_BUFF},
            {PerkHash.BladeOfNight , PerkType.TARGETED_DAMAGE},
            {PerkHash.BladeWhirlwind , PerkType.TARGETED_DAMAGE},
            {PerkHash.BlastNano , PerkType.TARGETED_DAMAGE},
            {PerkHash.BleedingWounds , PerkType.TARGETED_DAMAGE},
            {PerkHash.BlessingOfLife , PerkType.HEAL},
            {PerkHash.BlindedByDelights , PerkType.TARGETED_DAMAGE},
            {PerkHash.BlindsideBlow , PerkType.TARGETED_DAMAGE},
            {PerkHash.Bloodletting , PerkType.TARGETED_DAMAGE},
            {PerkHash.Bluntness , PerkType.TARGETED_DAMAGE},
            {PerkHash.Blur , PerkType.TARGETED_DAMAGE},
            {PerkHash.BodyTackle , PerkType.TARGETED_DAMAGE},
            {PerkHash.Bore , PerkType.TARGETED_DAMAGE},
            {PerkHash.BotConfinement , PerkType.TARGETED_DAMAGE},
            {PerkHash.BreachDefenses , PerkType.TARGETED_DAMAGE},
            {PerkHash.Break , PerkType.TARGETED_DAMAGE},
            {PerkHash.BringThePain , PerkType.TARGETED_DAMAGE},
            {PerkHash.CalledShot , PerkType.TARGETED_DAMAGE},
            {PerkHash.CaptureEssence , PerkType.DISABLED},
            {PerkHash.CaptureSpirit , PerkType.DISABLED},
            {PerkHash.CaptureVigor , PerkType.DISABLED},
            {PerkHash.CaptureVitality , PerkType.DISABLED},
            {PerkHash.CauseOfAnger , PerkType.TARGETED_DAMAGE},
            {PerkHash.ChannelRage , PerkType.PET_BUFF},
            {PerkHash.ChaosRitual , PerkType.TARGETED_DAMAGE},
            {PerkHash.ChaoticAssumption , PerkType.TARGETED_DAMAGE},
            {PerkHash.ChaoticEnergy , PerkType.PET_BUFF},
            {PerkHash.ChaoticModulation , PerkType.DISABLED},
            {PerkHash.Charge , PerkType.DISABLED},
            {PerkHash.ChemicalBlindness , PerkType.TARGETED_DAMAGE},
            {PerkHash.ChiConductor , PerkType.TARGETED_DAMAGE},
            {PerkHash.Clearshot , PerkType.TARGETED_DAMAGE},
            {PerkHash.Clearsight , PerkType.TARGETED_DAMAGE},
            {PerkHash.Cleave , PerkType.TARGETED_DAMAGE},
            {PerkHash.Clipfever , PerkType.TARGETED_DAMAGE},
            {PerkHash.CloseCall , PerkType.HEAL},
            {PerkHash.Collapser , PerkType.TARGETED_DAMAGE},
            {PerkHash.Combust , PerkType.TARGETED_DAMAGE},
            {PerkHash.ConcussiveShot , PerkType.TARGETED_DAMAGE},
            {PerkHash.Confinement , PerkType.TARGETED_DAMAGE},
            {PerkHash.ConfoundWithRules , PerkType.TARGETED_DAMAGE},
            {PerkHash.ConsumeTheSoul , PerkType.TARGETED_DAMAGE},
            {PerkHash.ContainedBurst , PerkType.TARGETED_DAMAGE},
            {PerkHash.ControlledChance , PerkType.TARGETED_DAMAGE},
            {PerkHash.ConvulsiveTremor , PerkType.TARGETED_DAMAGE},
            {PerkHash.Crave , PerkType.TARGETED_DAMAGE},
            {PerkHash.Cripple , PerkType.TARGETED_DAMAGE},
            {PerkHash.CrushBone , PerkType.TARGETED_DAMAGE},
            {PerkHash.Cure1 , PerkType.HEAL},
            {PerkHash.Cure2 , PerkType.HEAL},
            {PerkHash.CuringTouch , PerkType.HEAL},
            {PerkHash.DanceOfFools , PerkType.DISABLED},
            {PerkHash.DazzleWithLights , PerkType.TARGETED_DAMAGE},
            {PerkHash.Deadeye , PerkType.TARGETED_DAMAGE},
            {PerkHash.DeathStrike , PerkType.TARGETED_DAMAGE},
            {PerkHash.Deconstruction , PerkType.TARGETED_DAMAGE},
            {PerkHash.DeepCuts , PerkType.TARGETED_DAMAGE},
            {PerkHash.Derivate , PerkType.TARGETED_DAMAGE},
            {PerkHash.DetonateStoneworks , PerkType.TARGETED_DAMAGE},
            {PerkHash.DevastatingBlow , PerkType.TARGETED_DAMAGE},
            {PerkHash.DevotionalArmor , PerkType.SELF_BUFF},
            {PerkHash.Devour , PerkType.TARGETED_DAMAGE},
            {PerkHash.DevourEssence , PerkType.DISABLED},
            {PerkHash.DevourVigor , PerkType.DISABLED},
            {PerkHash.DevourVitality , PerkType.DISABLED},
            {PerkHash.Diffuse , PerkType.TARGETED_DAMAGE},
            {PerkHash.DimensionalFist , PerkType.TARGETED_DAMAGE},
            {PerkHash.DisableNaturalHealing , PerkType.TARGETED_DAMAGE},
            {PerkHash.Disorientate , PerkType.TARGETED_DAMAGE},
            {PerkHash.DizzyingHeights , PerkType.TARGETED_DAMAGE},
            {PerkHash.DodgeTheBlame , PerkType.SELF_BUFF},
            {PerkHash.DoomTouch , PerkType.TARGETED_DAMAGE},
            {PerkHash.DoubleShot , PerkType.TARGETED_DAMAGE},
            {PerkHash.DoubleStab , PerkType.DISABLED},
            {PerkHash.Dragonfire , PerkType.TARGETED_DAMAGE},
            {PerkHash.DrawBlood , PerkType.TARGETED_DAMAGE},
            {PerkHash.EasyShot , PerkType.TARGETED_DAMAGE},
            {PerkHash.EatBullets , PerkType.TARGETED_DAMAGE},
            {PerkHash.ECM1 , PerkType.CLEANSE},
            {PerkHash.ECM2 , PerkType.CLEANSE},
            {PerkHash.ElementaryTeleportation , PerkType.DISABLED},
            {PerkHash.EncaseInStone , PerkType.TARGETED_DAMAGE},
            {PerkHash.Energize , PerkType.DAMAGE_BUFF},
            {PerkHash.EnhancedHeal , PerkType.HEAL},
            {PerkHash.Escape , PerkType.DISABLED},
            {PerkHash.EtherealTouch , PerkType.TARGETED_DAMAGE},
            {PerkHash.EvasiveStance , PerkType.CUSTOM},
            {PerkHash.ExplorationTeleportation , PerkType.DISABLED},
            {PerkHash.Exultation , PerkType.TARGETED_DAMAGE},
            {PerkHash.FadeAnger , PerkType.TARGETED_DAMAGE},
            {PerkHash.FadeArmor , PerkType.TARGETED_DAMAGE},
            {PerkHash.Feel , PerkType.TARGETED_DAMAGE},
            {PerkHash.FieldBandage , PerkType.HEAL},
            {PerkHash.FindTheFlaw , PerkType.TARGETED_DAMAGE},
            {PerkHash.FireFrenzy , PerkType.TARGETED_DAMAGE},
            {PerkHash.Flay , PerkType.TARGETED_DAMAGE},
            {PerkHash.FleshQuiver , PerkType.TARGETED_DAMAGE},
            {PerkHash.FlimFocus , PerkType.SELF_BUFF},
            {PerkHash.FlurryOfCuts , PerkType.TARGETED_DAMAGE},
            {PerkHash.FollowupSmash , PerkType.TARGETED_DAMAGE},
            {PerkHash.ForceOpponent , PerkType.TARGETED_DAMAGE},
            {PerkHash.FreakShield , PerkType.SELF_BUFF},
            {PerkHash.FullFrontal , PerkType.TARGETED_DAMAGE},
            {PerkHash.Fuzz , PerkType.TARGETED_DAMAGE},
            {PerkHash.Gore , PerkType.DISABLED},
            {PerkHash.Governance , PerkType.DISABLED},
            {PerkHash.Grasp , PerkType.TARGETED_DAMAGE},
            {PerkHash.GreatPurge , PerkType.CLEANSE},
            {PerkHash.GripOfColossus , PerkType.TARGETED_DAMAGE},
            {PerkHash.GroinKick , PerkType.TARGETED_DAMAGE},
            {PerkHash.Guardian , PerkType.TARGETED_DAMAGE},
            {PerkHash.Guesstimate , PerkType.TARGETED_DAMAGE},
            {PerkHash.GuttingBlow , PerkType.TARGETED_DAMAGE},
            {PerkHash.HaleAndHearty , PerkType.CLEANSE},
            {PerkHash.HammerAndAnvil , PerkType.TARGETED_DAMAGE},
            {PerkHash.HarmonizeBodyAndMind , PerkType.HEAL},
            {PerkHash.Hatred , PerkType.TARGETED_DAMAGE},
            {PerkHash.Headbutt , PerkType.TARGETED_DAMAGE},
            {PerkHash.Heal , PerkType.HEAL},
            {PerkHash.Hecatomb , PerkType.DISABLED},
            {PerkHash.Highway , PerkType.TARGETED_DAMAGE},
            {PerkHash.HonoringTheAncients , PerkType.TARGETED_DAMAGE},
            {PerkHash.HostileTakeover , PerkType.TARGETED_DAMAGE},
            {PerkHash.IgnitionFlare , PerkType.TARGETED_DAMAGE},
            {PerkHash.Impale , PerkType.DISABLED},
            {PerkHash.Implode , PerkType.TARGETED_DAMAGE},
            {PerkHash.Incapacitate , PerkType.TARGETED_DAMAGE},
            {PerkHash.InitialStrike , PerkType.TARGETED_DAMAGE},
            {PerkHash.Insight , PerkType.HEAL},
            {PerkHash.InstallExplosiveDevice , PerkType.TARGETED_DAMAGE},
            {PerkHash.InstallNotumDepletionDevice , PerkType.TARGETED_DAMAGE},
            {PerkHash.InsuranceClaim , PerkType.DISABLED},
            {PerkHash.JarringBurst , PerkType.TARGETED_DAMAGE},
            {PerkHash.KaMon , PerkType.TARGETED_DAMAGE},
            {PerkHash.KenFi , PerkType.TARGETED_DAMAGE},
            {PerkHash.KenSi , PerkType.TARGETED_DAMAGE},
            {PerkHash.KnowledgeEnhancer , PerkType.TARGETED_DAMAGE},
            {PerkHash.Lacerate , PerkType.DISABLED},
            {PerkHash.LaserPaintTarget , PerkType.TARGETED_DAMAGE},
            {PerkHash.LayOnHands , PerkType.HEAL},
            {PerkHash.Leadership , PerkType.DISABLED},
            {PerkHash.LegShot , PerkType.OTHER},
            {PerkHash.LEProcAdventurerAesirAbsorption , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerBasicDressing , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerCharringBlow , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerCombustion , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerFerociousHits , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerHealingHerbs , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerMacheteFlurry , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerMacheteSlice , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerRestoreVigor , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerSelfPreservation , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerSkinProtection , PerkType.LE_PROC},
            {PerkHash.LEProcAdventurerSoothingHerbs , PerkType.LE_PROC},
            {PerkHash.LEProcAgentBrokenAnkle , PerkType.LE_PROC},
            {PerkHash.LEProcAgentCellKiller , PerkType.LE_PROC},
            {PerkHash.LEProcAgentDisableCuffs , PerkType.LE_PROC},
            {PerkHash.LEProcAgentGrimReaper , PerkType.LE_PROC},
            {PerkHash.LEProcAgentImprovedFocus , PerkType.LE_PROC},
            {PerkHash.LEProcAgentIntenseMetabolism , PerkType.LE_PROC},
            {PerkHash.LEProcAgentLaserAim , PerkType.LE_PROC},
            {PerkHash.LEProcAgentMinorNanobotEnhance , PerkType.LE_PROC},
            {PerkHash.LEProcAgentPlasteelPiercingRounds , PerkType.LE_PROC},

            {PerkHash.LEProcAgentNanoEnhancedTargeting , PerkType.LE_PROC},
            {PerkHash.LEProcAgentNoEscape , PerkType.LE_PROC},
            {PerkHash.LEProcAgentNotumChargedRounds , PerkType.LE_PROC},

            {PerkHash.LEProcBureaucratDeflation , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratFormsInTriplicate , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratInflationAdjustment , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratLostPaperwork , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratMobilityEmbargo , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratNextWindowOver , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratPapercut , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratPleaseHold , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratSocialServices , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratTaxAudit , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratWaitInThatQueue , PerkType.LE_PROC},
            {PerkHash.LEProcBureaucratWrongWindow , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorAnatomicBlight , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorAnesthetic , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorAntiseptic , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorAstringent , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorBloodTransfusion , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorDangerousCulture , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorHealingCare , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorInflammation , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorMassiveVitaePlan , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorMuscleMemory , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorPathogen , PerkType.LE_PROC},
            {PerkHash.LEProcDoctorRestrictiveBandaging , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerAirOfHatred , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerBustKneecaps , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerIgnorePain , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerInspireIre , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerInspireRage , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerRagingBlow , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerShieldOfTheOgre , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerShrugOffHits , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerTearLigaments , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerVileRage , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerViolationBuffer , PerkType.LE_PROC},
            {PerkHash.LEProcEnforcerVortexOfHate , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerAssaultForceRelief , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerCongenialEncasement , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerCushionBlows , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerDestructiveSignal , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerDestructiveTheorem , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerDroneExplosives , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerDroneMissiles , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerEndureBarrage , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerEnergyTransfer , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerPersonalProtection , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerReactiveArmor , PerkType.LE_PROC},
            {PerkHash.LEProcEngineerSplinterPreservation , PerkType.LE_PROC},
            {PerkHash.LEProcFixerBackyardBandages , PerkType.LE_PROC},
            {PerkHash.LEProcFixerBendingTheRules , PerkType.LE_PROC},
            {PerkHash.LEProcFixerBootlegRemedies , PerkType.LE_PROC},
            {PerkHash.LEProcFixerContaminatedBullets , PerkType.LE_PROC},
            {PerkHash.LEProcFixerDirtyTricks , PerkType.LE_PROC},
            {PerkHash.LEProcFixerEscapeTheSystem , PerkType.LE_PROC},
            {PerkHash.LEProcFixerFightingChance , PerkType.LE_PROC},
            {PerkHash.LEProcFixerFishInABarrel , PerkType.LE_PROC},
            {PerkHash.LEProcFixerIntenseMetabolism , PerkType.LE_PROC},
            {PerkHash.LEProcFixerSlipThemAMickey , PerkType.LE_PROC},
            {PerkHash.LEProcFixerUndergroundSutures , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperAmbientPurification , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperBenevolentBarrier , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperEschewTheFaithless , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperFaithfulReconstruction , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperHonorRestored , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperIgnoreTheUnrepentant , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperPureStrike , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperRighteousSmite , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperRighteousStrike , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperSubjugation , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperSymbioticBypass , PerkType.LE_PROC},
            {PerkHash.LEProcKeeperVirtuousReaper , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistAbsoluteFist , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistAttackLigaments , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistDebilitatingStrike , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistDisruptKi , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistHealingMeditation , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistMedicinalRemedy , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistSelfReconstruction , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistSmashingFist , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistStingingFist , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistStrengthenKi , PerkType.LE_PROC},
            {PerkHash.LEProcMartialArtistStrengthenSpirit , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistAnticipatedEvasion , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistDiffuseRage , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistEconomicNanobotUse , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistEgoStrike , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistMindWail , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistNanobotContingentArrest , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistRegainFocus , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistSowDespair , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistSowDoubt , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistSuppressFury , PerkType.LE_PROC},
            {PerkHash.LEProcMetaPhysicistThoughtfulMeans , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianAcceleratedReality , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianCircularLogic , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianHarvestEnergy , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianIncreaseMomentum , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianLayeredAmnesty , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianLoopingService , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianOptimizedLibrary , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianPoweredNanoFortress , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianSourceTap , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianThermalReprieve , PerkType.LE_PROC},
            {PerkHash.LEProcNanoTechnicianUnstableLibrary , PerkType.LE_PROC},
            {PerkHash.LEProcShadeBlackenedLegacy , PerkType.LE_PROC},
            {PerkHash.LEProcShadeBlackheart , PerkType.LE_PROC},
            {PerkHash.LEProcShadeConcealedSurprise , PerkType.LE_PROC},
            {PerkHash.LEProcShadeDeviousSpirit , PerkType.LE_PROC},
            {PerkHash.LEProcShadeDrainEssence , PerkType.LE_PROC},
            {PerkHash.LEProcShadeElusiveSpirit , PerkType.LE_PROC},
            {PerkHash.LEProcShadeMisdirection , PerkType.LE_PROC},
            {PerkHash.LEProcShadeSapLife , PerkType.LE_PROC},
            {PerkHash.LEProcShadeShadowedGift , PerkType.LE_PROC},
            {PerkHash.LEProcShadeSiphonBeing , PerkType.LE_PROC},
            {PerkHash.LEProcShadeToxicConfusion , PerkType.LE_PROC},
            {PerkHash.LEProcShadeTwistedCaress , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierConcussiveShot , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierDeepSixInitiative , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierEmergencyBandages , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierFuriousAmmunition , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierFuseBodyArmor , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierGearAssaultAbsorption , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierGrazeJugularVein , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierOnTheDouble , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierReconditioned , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierShootArtery , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierSuccessfulTargeting , PerkType.LE_PROC},
            {PerkHash.LEProcSoldierTargetAcquired , PerkType.LE_PROC},
            {PerkHash.LEProcTraderAccumulatedInterest , PerkType.LE_PROC},
            {PerkHash.LEProcTraderDebtCollection , PerkType.LE_PROC},
            {PerkHash.LEProcTraderDepleteAssets , PerkType.LE_PROC},
            {PerkHash.LEProcTraderEscrow , PerkType.LE_PROC},
            {PerkHash.LEProcTraderExchangeProduct , PerkType.LE_PROC},
            {PerkHash.LEProcTraderPaymentPlan , PerkType.LE_PROC},
            {PerkHash.LEProcTraderRebate , PerkType.LE_PROC},
            {PerkHash.LEProcTraderRefinanceLoans , PerkType.LE_PROC},
            {PerkHash.LEProcTraderRigidLiquidation , PerkType.LE_PROC},
            {PerkHash.LEProcTraderUnexpectedBonus , PerkType.LE_PROC},
            {PerkHash.LEProcTraderUnforgivenDebts , PerkType.LE_PROC},
            {PerkHash.LEProcTraderUnopenedLetter , PerkType.LE_PROC},
            {PerkHash.Lifeblood , PerkType.HEAL},
            {PerkHash.LightBullet , PerkType.TARGETED_DAMAGE},
            {PerkHash.LightKiller , PerkType.TARGETED_DAMAGE},
            {PerkHash.Limber , PerkType.DISABLED},
            {PerkHash.MaliciousProhibition , PerkType.TARGETED_DAMAGE},
            {PerkHash.MarkOfSufferance , PerkType.HEAL},
            {PerkHash.MarkOfTheUnclean , PerkType.TARGETED_DAMAGE},
            {PerkHash.MarkOfTheUnhallowed , PerkType.TARGETED_DAMAGE},
            {PerkHash.MarkOfVengeance , PerkType.TARGETED_DAMAGE},
            {PerkHash.Medallion , PerkType.TARGETED_DAMAGE},
            {PerkHash.MemoryScrabble , PerkType.TARGETED_DAMAGE},
            {PerkHash.Mistreatment , PerkType.TARGETED_DAMAGE},
            {PerkHash.MongoRage , PerkType.TARGETED_DAMAGE},
            {PerkHash.Moonmist , PerkType.TARGETED_DAMAGE},
            {PerkHash.MuzzleOverload , PerkType.TARGETED_DAMAGE},
            {PerkHash.MyOwnFortress , PerkType.TARGETED_DAMAGE},
            {PerkHash.NanoFeast , PerkType.TARGETED_DAMAGE},
            {PerkHash.NanoHeal , PerkType.NANO_HEAL},
            {PerkHash.NanoShakes , PerkType.TARGETED_DAMAGE},
            {PerkHash.NanoTransmission , PerkType.DISABLED},
            {PerkHash.NapalmSpray , PerkType.TARGETED_DAMAGE},
            {PerkHash.NCUBooster , PerkType.SELF_BUFF},
            {PerkHash.NeutroniumSlug , PerkType.TARGETED_DAMAGE},
            {PerkHash.NightKiller , PerkType.TARGETED_DAMAGE},
            {PerkHash.NocturnalStrike , PerkType.TARGETED_DAMAGE},
            {PerkHash.NotumOverflow , PerkType.TARGETED_DAMAGE},
            {PerkHash.NotumShield , PerkType.NANO_HEAL},
            {PerkHash.Numb , PerkType.TARGETED_DAMAGE},
            {PerkHash.Obliterate , PerkType.TARGETED_DAMAGE},
            {PerkHash.Opening , PerkType.TARGETED_DAMAGE},
            {PerkHash.OpportunityKnocks , PerkType.TARGETED_DAMAGE},
            {PerkHash.OptimizeBotProtocol , PerkType.PET_BUFF},
            {PerkHash.Overrule , PerkType.TARGETED_DAMAGE},
            {PerkHash.OverwhelmingMight , PerkType.TARGETED_DAMAGE},
            {PerkHash.PainLance , PerkType.TARGETED_DAMAGE},
            {PerkHash.PeelLayers , PerkType.TARGETED_DAMAGE},
            {PerkHash.Pen , PerkType.TARGETED_DAMAGE},
            {PerkHash.Perforate , PerkType.DISABLED},
            {PerkHash.PinpointStrike , PerkType.TARGETED_DAMAGE},
            {PerkHash.PointBlank , PerkType.TARGETED_DAMAGE},
            {PerkHash.PoisonSprinkle , PerkType.TARGETED_DAMAGE},
            {PerkHash.Popshot , PerkType.TARGETED_DAMAGE},
            {PerkHash.PowerBlast , PerkType.TARGETED_DAMAGE},
            {PerkHash.PowerBolt , PerkType.TARGETED_DAMAGE},
            {PerkHash.PowerCombo , PerkType.TARGETED_DAMAGE},
            {PerkHash.PowerOfLight , PerkType.TARGETED_DAMAGE},
            {PerkHash.PowerShock , PerkType.TARGETED_DAMAGE},
            {PerkHash.PowerVolley , PerkType.TARGETED_DAMAGE},
            {PerkHash.ProgramOverload , PerkType.TARGETED_DAMAGE},
            {PerkHash.Pulverize , PerkType.TARGETED_DAMAGE},
            {PerkHash.Puppeteer , PerkType.PET_BUFF},
            {PerkHash.Purge1 , PerkType.CLEANSE},
            {PerkHash.Purge2 , PerkType.CLEANSE},
            {PerkHash.Purify , PerkType.CLEANSE},
            {PerkHash.PurpleHeart , PerkType.DISABLED},
            {PerkHash.QuarkContainmentField , PerkType.TARGETED_DAMAGE},
            {PerkHash.QuickBash , PerkType.TARGETED_DAMAGE},
            {PerkHash.QuickCut , PerkType.TARGETED_DAMAGE},
            {PerkHash.QuickShot , PerkType.CUSTOM},
            {PerkHash.ReapLife , PerkType.TARGETED_DAMAGE},
            {PerkHash.Recalibrate , PerkType.TARGETED_DAMAGE},
            {PerkHash.ReconstructDNA , PerkType.HEAL},
            {PerkHash.Reconstruction , PerkType.PET_HEAL},
            {PerkHash.RedDawn , PerkType.HEAL},
            {PerkHash.RedDusk , PerkType.TARGETED_DAMAGE},
            {PerkHash.RedeemLastWish , PerkType.SELF_BUFF},
            {PerkHash.RegainNano , PerkType.NANO_HEAL},
            {PerkHash.ReinforceSlugs , PerkType.TARGETED_DAMAGE},
            {PerkHash.Reject , PerkType.TARGETED_DAMAGE},
            {PerkHash.Removal1 , PerkType.CLEANSE},
            {PerkHash.Removal2 , PerkType.CLEANSE},
            {PerkHash.RepairPet , PerkType.PET_HEAL},
            {PerkHash.RibbonFlesh , PerkType.TARGETED_DAMAGE},
            {PerkHash.RighteousFury , PerkType.TARGETED_DAMAGE},
            {PerkHash.RighteousWrath , PerkType.TARGETED_DAMAGE},
            {PerkHash.RitualOfBlood , PerkType.DISABLED},
            {PerkHash.RitualOfDevotion , PerkType.DISABLED},
            {PerkHash.RitualOfSpirit , PerkType.DISABLED},
            {PerkHash.RitualOfZeal , PerkType.DISABLED},
            {PerkHash.SabotageQuarkField , PerkType.TARGETED_DAMAGE},
            {PerkHash.Sacrifice , PerkType.DISABLED},
            {PerkHash.SealWounds , PerkType.TARGETED_DAMAGE},
            {PerkHash.SeismicSmash , PerkType.TARGETED_DAMAGE},
            {PerkHash.SeppukuSlash , PerkType.TARGETED_DAMAGE},
            {PerkHash.ShadowBullet , PerkType.TARGETED_DAMAGE},
            {PerkHash.ShadowKiller , PerkType.TARGETED_DAMAGE},
            {PerkHash.ShadowStab , PerkType.DISABLED},
            {PerkHash.ShutdownRemoval , PerkType.CLEANSE},
            {PerkHash.SilentPlague , PerkType.TARGETED_DAMAGE},
            {PerkHash.SiphonBox , PerkType.DISABLED},
            {PerkHash.SkillDrainRemoval , PerkType.CLEANSE},
            {PerkHash.SliceAndDice , PerkType.TARGETED_DAMAGE},
            {PerkHash.SnipeShot1 , PerkType.TARGETED_DAMAGE},
            {PerkHash.SnipeShot2 , PerkType.TARGETED_DAMAGE},
            {PerkHash.SoftenUp , PerkType.TARGETED_DAMAGE},
            {PerkHash.SolidSlug , PerkType.TARGETED_DAMAGE},
            {PerkHash.SpectatorWrath , PerkType.TARGETED_DAMAGE},
            {PerkHash.Sphere , PerkType.TARGETED_DAMAGE},
            {PerkHash.SpiritDissolution , PerkType.TARGETED_DAMAGE},
            {PerkHash.SpiritOfBlessing , PerkType.TARGETED_DAMAGE},
            {PerkHash.SpiritOfPurity , PerkType.TARGETED_DAMAGE},
            {PerkHash.Stab , PerkType.TARGETED_DAMAGE},
            {PerkHash.StoneFist , PerkType.TARGETED_DAMAGE},
            {PerkHash.Stoneworks , PerkType.TARGETED_DAMAGE},
            {PerkHash.StopNotumFlow , PerkType.TARGETED_DAMAGE},
            {PerkHash.StripNano , PerkType.TARGETED_DAMAGE},
            {PerkHash.Succumb , PerkType.TARGETED_DAMAGE},
            {PerkHash.Supernova , PerkType.TARGETED_DAMAGE},
            {PerkHash.SuppressivePrimer , PerkType.TARGETED_DAMAGE},
            {PerkHash.SupressiveHorde , PerkType.DAMAGE_BUFF},
            {PerkHash.Survival , PerkType.HEAL},
            {PerkHash.Sword , PerkType.TARGETED_DAMAGE},
            {PerkHash.Symbiosis , PerkType.TARGETED_DAMAGE},
            {PerkHash.TackyHack , PerkType.TARGETED_DAMAGE},
            {PerkHash.TaintWounds , PerkType.TARGETED_DAMAGE},
            {PerkHash.TapNotumSource , PerkType.NANO_HEAL},
            {PerkHash.TapVitae , PerkType.TARGETED_DAMAGE},
            {PerkHash.Taunt , PerkType.TARGETED_DAMAGE},
            {PerkHash.TauntBox , PerkType.DISABLED},
            {PerkHash.TeamHaleAndHearty , PerkType.CLEANSE},
            {PerkHash.TeamHeal , PerkType.HEAL},
            {PerkHash.TheDirector , PerkType.DISABLED},
            {PerkHash.ThermalDetonation , PerkType.TARGETED_DAMAGE},
            {PerkHash.ThermalPrimer , PerkType.TARGETED_DAMAGE},
            {PerkHash.TheShot , PerkType.TARGETED_DAMAGE},
            {PerkHash.Tick , PerkType.TARGETED_DAMAGE},
            {PerkHash.ToxicShock , PerkType.TARGETED_DAMAGE},
            {PerkHash.Tracer , PerkType.TARGETED_DAMAGE},
            {PerkHash.Tranquilizer , PerkType.TARGETED_DAMAGE},
            {PerkHash.Transfix , PerkType.TARGETED_DAMAGE},
            {PerkHash.TreatmentTransfer , PerkType.DISABLED},
            {PerkHash.TremorHand , PerkType.TARGETED_DAMAGE},
            {PerkHash.TriangulateTarget , PerkType.TARGETED_DAMAGE},
            {PerkHash.TriggerHappy , PerkType.TARGETED_DAMAGE},
            {PerkHash.TrollForm , PerkType.OTHER},
            {PerkHash.UnhallowedFury , PerkType.TARGETED_DAMAGE},
            {PerkHash.UnhallowedWrath , PerkType.TARGETED_DAMAGE},
            {PerkHash.UnsealedBlight , PerkType.DISABLED},
            {PerkHash.UnsealedContagion , PerkType.DISABLED},
            {PerkHash.UnsealedPestilence , PerkType.DISABLED},
            {PerkHash.Utilize , PerkType.TARGETED_DAMAGE},
            {PerkHash.Vaccinate1 , PerkType.CLEANSE},
            {PerkHash.Vaccinate2 , PerkType.CLEANSE},
            {PerkHash.Violence , PerkType.TARGETED_DAMAGE},
            {PerkHash.ViralCombination , PerkType.TARGETED_DAMAGE},
            {PerkHash.ViralWipe , PerkType.TARGETED_DAMAGE},
            {PerkHash.VitalShock , PerkType.TARGETED_DAMAGE},
            {PerkHash.WeaponBash , PerkType.TARGETED_DAMAGE},
            {PerkHash.WitOfTheAtrox , PerkType.CUSTOM},
            {PerkHash.ZapNano , PerkType.TARGETED_DAMAGE},
        };
    }

    enum PerkType
    {
        SELF_BUFF, HEAL, NANO_HEAL, TARGETED_DAMAGE, DAMAGE_BUFF, GENERIC, LE_PROC, CUSTOM, PET_BUFF, DISABLED, CLEANSE, PET_HEAL, UNKNOWN, OTHER
    }
}
