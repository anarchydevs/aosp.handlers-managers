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
            if(!HashMap.ContainsKey(perkHash))
            {
                return PerkType.Unknown;
            }

            return HashMap[perkHash];
        }

        private static Dictionary<PerkHash, PerkType> HashMap = new Dictionary<PerkHash, PerkType>()
        {
            {PerkHash.AccelerateDecayingQuarks , PerkType.TargetDamage},
            {PerkHash.AccessNotumSource , PerkType.SelfNano},
            {PerkHash.AnnihilateNotumMolecules , PerkType.TargetDamage},
            {PerkHash.Antitrust , PerkType.TargetDamage},
            {PerkHash.ArmorPiercingShot , PerkType.TargetDamage},
            {PerkHash.ArouseAnger , PerkType.TargetDamage},
            {PerkHash.Assassinate , PerkType.TargetDamage},
            {PerkHash.AssumeTarget , PerkType.TargetDamage},
            {PerkHash.Atrophy , PerkType.TargetDamage},
            {PerkHash.Avalanche , PerkType.TargetDamage},
            {PerkHash.Awakening , PerkType.SelfHeal},
            {PerkHash.BalanceOfYinAndYang ,PerkType.SelfHeal},
            {PerkHash.BattlegroupHeal1 , PerkType.Custom},
            {PerkHash.BattlegroupHeal2 , PerkType.Custom},
            {PerkHash.BattlegroupHeal3 , PerkType.Custom},
            {PerkHash.BattlegroupHeal4 , PerkType.Custom},
            {PerkHash.Bearhug , PerkType.TargetDamage},
            {PerkHash.Beckoning , PerkType.TargetDamage},
            {PerkHash.BigSmash , PerkType.TargetDamage},
            {PerkHash.BioCocoon , PerkType.Disabled},
            {PerkHash.BioRegrowth , PerkType.Disabled},
            {PerkHash.BioShield , PerkType.SelfBuff},
            {PerkHash.BladeOfNight , PerkType.TargetDamage},
            {PerkHash.BladeWhirlwind , PerkType.TargetDamage},
            {PerkHash.BlastNano , PerkType.TargetDamage},
            {PerkHash.BleedingWounds , PerkType.TargetDamage},
            {PerkHash.BlessingOfLife , PerkType.SelfHeal},
            {PerkHash.BlindedByDelights , PerkType.TargetDamage},
            {PerkHash.BlindsideBlow , PerkType.TargetDamage},
            {PerkHash.Bloodletting , PerkType.TargetDamage},
            {PerkHash.Bluntness , PerkType.TargetDamage},
            {PerkHash.Blur , PerkType.TargetDamage},
            {PerkHash.BodyTackle , PerkType.TargetDamage},
            {PerkHash.Bore , PerkType.TargetDamage},
            {PerkHash.BotConfinement , PerkType.TargetDamage},
            {PerkHash.BreachDefenses , PerkType.TargetDamage},
            {PerkHash.Break , PerkType.TargetDamage},
            {PerkHash.BringThePain , PerkType.TargetDamage},
            {PerkHash.CalledShot , PerkType.TargetDamage},
            {PerkHash.CaptureEssence , PerkType.Disabled},
            {PerkHash.CaptureSpirit , PerkType.Disabled},
            {PerkHash.CaptureVigor , PerkType.Disabled},
            {PerkHash.CaptureVitality , PerkType.Disabled},
            {PerkHash.CauseOfAnger , PerkType.TargetDamage},
            {PerkHash.ChannelRage , PerkType.PetBuff},
            {PerkHash.ChaosRitual , PerkType.TargetDamage},
            {PerkHash.ChaoticAssumption , PerkType.TargetDamage},
            {PerkHash.ChaoticEnergy , PerkType.PetBuff},
            {PerkHash.ChaoticModulation , PerkType.Disabled},
            {PerkHash.Charge , PerkType.Disabled},
            {PerkHash.ChemicalBlindness , PerkType.TargetDamage},
            {PerkHash.ChiConductor , PerkType.TargetDamage},
            {PerkHash.Clearshot , PerkType.TargetDamage},
            {PerkHash.Clearsight , PerkType.TargetDamage},
            {PerkHash.Cleave , PerkType.TargetDamage},
            {PerkHash.Clipfever , PerkType.TargetDamage},
            {PerkHash.CloseCall , PerkType.SelfHeal},
            {PerkHash.Collapser , PerkType.TargetDamage},
            {PerkHash.Combust , PerkType.TargetDamage},
            {PerkHash.ConcussiveShot , PerkType.TargetDamage},
            {PerkHash.Confinement , PerkType.TargetDamage},
            {PerkHash.ConfoundWithRules , PerkType.TargetDamage},
            {PerkHash.ConsumeTheSoul , PerkType.TargetDamage},
            {PerkHash.ContainedBurst , PerkType.TargetDamage},
            {PerkHash.ControlledChance , PerkType.TargetDamage},
            {PerkHash.ConvulsiveTremor , PerkType.TargetDamage},
            {PerkHash.Crave , PerkType.TargetDamage},
            {PerkHash.Cripple , PerkType.TargetDamage},
            {PerkHash.CrushBone , PerkType.TargetDamage},
            {PerkHash.Cure1 , PerkType.SelfHeal},
            {PerkHash.Cure2 , PerkType.SelfHeal},
            {PerkHash.CuringTouch , PerkType.SelfHeal},
            {PerkHash.DanceOfFools , PerkType.Disabled},
            {PerkHash.DazzleWithLights , PerkType.Disabled},
            {PerkHash.Deadeye , PerkType.TargetDamage},
            {PerkHash.DeathStrike , PerkType.TargetDamage},
            {PerkHash.Deconstruction , PerkType.TargetDamage},
            {PerkHash.DeepCuts , PerkType.TargetDamage},
            {PerkHash.Derivate , PerkType.TargetDamage},
            {PerkHash.DetonateStoneworks , PerkType.TargetDamage},
            {PerkHash.DevastatingBlow , PerkType.TargetDamage},
            {PerkHash.DevotionalArmor , PerkType.SelfBuff},
            {PerkHash.Devour , PerkType.TargetDamage},
            {PerkHash.DevourEssence , PerkType.Disabled},
            {PerkHash.DevourVigor , PerkType.Disabled},
            {PerkHash.DevourVitality , PerkType.Disabled},
            {PerkHash.Diffuse , PerkType.TargetDamage},
            {PerkHash.DimensionalFist , PerkType.TargetDamage},
            {PerkHash.DisableNaturalHealing , PerkType.TargetDamage},
            {PerkHash.Disorientate , PerkType.TargetDamage},
            {PerkHash.DizzyingHeights , PerkType.TargetDamage},
            {PerkHash.DodgeTheBlame , PerkType.SelfBuff},
            {PerkHash.DoomTouch , PerkType.TargetDamage},
            {PerkHash.DoubleShot , PerkType.TargetDamage},
            {PerkHash.DoubleStab , PerkType.Disabled},
            {PerkHash.Dragonfire , PerkType.TargetDamage},
            {PerkHash.DrawBlood , PerkType.TargetDamage},
            {PerkHash.EasyShot , PerkType.TargetDamage},
            {PerkHash.EatBullets , PerkType.TargetDamage},
            {PerkHash.ECM1 , PerkType.Clease},
            {PerkHash.ECM2 , PerkType.Clease},
            {PerkHash.ElementaryTeleportation , PerkType.Disabled},
            {PerkHash.EncaseInStone , PerkType.TargetDamage},

            {PerkHash.Energize , PerkType.Disabled},
            //{PerkHash.Energize , PerkType.DamageBuff},

            {PerkHash.EnhancedHeal , PerkType.SelfHeal},
            {PerkHash.Escape , PerkType.Disabled},
            {PerkHash.EtherealTouch , PerkType.TargetDamage},
            {PerkHash.EvasiveStance , PerkType.Custom},
            {PerkHash.ExplorationTeleportation , PerkType.Disabled},
            {PerkHash.Exultation , PerkType.TargetDamage},
            {PerkHash.FadeAnger , PerkType.TargetDamage},
            {PerkHash.FadeArmor , PerkType.TargetDamage},
            {PerkHash.Feel , PerkType.TargetDamage},
            {PerkHash.FieldBandage , PerkType.SelfHeal},
            {PerkHash.FindTheFlaw , PerkType.TargetDamage},
            {PerkHash.FireFrenzy , PerkType.TargetDamage},
            {PerkHash.Flay , PerkType.TargetDamage},
            {PerkHash.FleshQuiver , PerkType.TargetDamage},
            {PerkHash.FlimFocus , PerkType.Disabled},
            {PerkHash.FlurryOfCuts , PerkType.TargetDamage},
            {PerkHash.FollowupSmash , PerkType.TargetDamage},
            {PerkHash.ForceOpponent , PerkType.TargetDamage},
            {PerkHash.FreakShield , PerkType.SelfBuff},
            {PerkHash.FullFrontal , PerkType.TargetDamage},
            {PerkHash.Fuzz , PerkType.TargetDamage},
            {PerkHash.Gore , PerkType.Disabled},
            {PerkHash.Governance , PerkType.Disabled},
            {PerkHash.Grasp , PerkType.TargetDamage},
            {PerkHash.GreatPurge , PerkType.Clease},
            {PerkHash.GripOfColossus , PerkType.TargetDamage},
            {PerkHash.GroinKick , PerkType.TargetDamage},
            {PerkHash.Guardian , PerkType.TargetDamage},
            {PerkHash.Guesstimate , PerkType.TargetDamage},
            {PerkHash.GuttingBlow , PerkType.TargetDamage},
            {PerkHash.HaleAndHearty , PerkType.Clease},
            {PerkHash.HammerAndAnvil , PerkType.TargetDamage},
            {PerkHash.HarmonizeBodyAndMind , PerkType.SelfHeal},
            {PerkHash.Hatred , PerkType.TargetDamage},
            {PerkHash.Headbutt , PerkType.TargetDamage},
            {PerkHash.Heal , PerkType.SelfHeal},
            {PerkHash.Hecatomb , PerkType.Disabled},
            {PerkHash.Highway , PerkType.TargetDamage},
            {PerkHash.HonoringTheAncients , PerkType.TargetDamage},
            {PerkHash.HostileTakeover , PerkType.TargetDamage},
            {PerkHash.IgnitionFlare , PerkType.TargetDamage},
            {PerkHash.Impale , PerkType.Disabled},
            {PerkHash.Implode , PerkType.TargetDamage},
            {PerkHash.Incapacitate , PerkType.TargetDamage},
            {PerkHash.InitialStrike , PerkType.TargetDamage},
            {PerkHash.Insight , PerkType.SelfHeal},
            {PerkHash.InstallExplosiveDevice , PerkType.TargetDamage},
            {PerkHash.InstallNotumDepletionDevice , PerkType.TargetDamage},
            {PerkHash.InsuranceClaim , PerkType.Disabled},
            {PerkHash.JarringBurst , PerkType.TargetDamage},
            {PerkHash.KaMon , PerkType.TargetDamage},
            {PerkHash.KenFi , PerkType.TargetDamage},
            {PerkHash.KenSi , PerkType.TargetDamage},
            {PerkHash.KnowledgeEnhancer , PerkType.TargetDamage},
            {PerkHash.Lacerate , PerkType.Disabled},
            {PerkHash.LaserPaintTarget , PerkType.TargetDamage},
            {PerkHash.LayOnHands , PerkType.SelfHeal},
            {PerkHash.Leadership , PerkType.Disabled},
            {PerkHash.LegShot , PerkType.Disabled},
            {PerkHash.LEProcAdventurerAesirAbsorption , PerkType.LEProc},
            {PerkHash.LEProcAdventurerBasicDressing , PerkType.LEProc},
            {PerkHash.LEProcAdventurerCharringBlow , PerkType.LEProc},
            {PerkHash.LEProcAdventurerCombustion , PerkType.LEProc},
            {PerkHash.LEProcAdventurerFerociousHits , PerkType.LEProc},
            {PerkHash.LEProcAdventurerHealingHerbs , PerkType.LEProc},
            {PerkHash.LEProcAdventurerMacheteFlurry , PerkType.LEProc},
            {PerkHash.LEProcAdventurerMacheteSlice , PerkType.LEProc},
            {PerkHash.LEProcAdventurerRestoreVigor , PerkType.LEProc},
            {PerkHash.LEProcAdventurerSelfPreservation , PerkType.LEProc},
            {PerkHash.LEProcAdventurerSkinProtection , PerkType.LEProc},
            {PerkHash.LEProcAdventurerSoothingHerbs , PerkType.LEProc},
            {PerkHash.LEProcAgentBrokenAnkle , PerkType.LEProc},
            {PerkHash.LEProcAgentCellKiller , PerkType.LEProc},
            {PerkHash.LEProcAgentDisableCuffs , PerkType.LEProc},
            {PerkHash.LEProcAgentGrimReaper , PerkType.LEProc},
            {PerkHash.LEProcAgentImprovedFocus , PerkType.LEProc},
            {PerkHash.LEProcAgentIntenseMetabolism , PerkType.LEProc},
            {PerkHash.LEProcAgentLaserAim , PerkType.LEProc},
            {PerkHash.LEProcAgentMinorNanobotEnhance , PerkType.LEProc},
            {PerkHash.LEProcAgentPlasteelPiercingRounds , PerkType.LEProc},

            {PerkHash.LEProcAgentNanoEnhancedTargeting , PerkType.LEProc},
            {PerkHash.LEProcAgentNoEscape , PerkType.LEProc},
            {PerkHash.LEProcAgentNotumChargedRounds , PerkType.LEProc},

            {PerkHash.LEProcBureaucratDeflation , PerkType.LEProc},
            {PerkHash.LEProcBureaucratFormsInTriplicate , PerkType.LEProc},
            {PerkHash.LEProcBureaucratInflationAdjustment , PerkType.LEProc},
            {PerkHash.LEProcBureaucratLostPaperwork , PerkType.LEProc},
            {PerkHash.LEProcBureaucratMobilityEmbargo , PerkType.LEProc},
            {PerkHash.LEProcBureaucratNextWindowOver , PerkType.LEProc},
            {PerkHash.LEProcBureaucratPapercut , PerkType.LEProc},
            {PerkHash.LEProcBureaucratPleaseHold , PerkType.LEProc},
            {PerkHash.LEProcBureaucratSocialServices , PerkType.LEProc},
            {PerkHash.LEProcBureaucratTaxAudit , PerkType.LEProc},
            {PerkHash.LEProcBureaucratWaitInThatQueue , PerkType.LEProc},
            {PerkHash.LEProcBureaucratWrongWindow , PerkType.LEProc},
            {PerkHash.LEProcDoctorAnatomicBlight , PerkType.LEProc},
            {PerkHash.LEProcDoctorAnesthetic , PerkType.LEProc},
            {PerkHash.LEProcDoctorAntiseptic , PerkType.LEProc},
            {PerkHash.LEProcDoctorAstringent , PerkType.LEProc},
            {PerkHash.LEProcDoctorBloodTransfusion , PerkType.LEProc},
            {PerkHash.LEProcDoctorDangerousCulture , PerkType.LEProc},
            {PerkHash.LEProcDoctorHealingCare , PerkType.LEProc},
            {PerkHash.LEProcDoctorInflammation , PerkType.LEProc},
            {PerkHash.LEProcDoctorMassiveVitaePlan , PerkType.LEProc},
            {PerkHash.LEProcDoctorMuscleMemory , PerkType.LEProc},
            {PerkHash.LEProcDoctorPathogen , PerkType.LEProc},
            {PerkHash.LEProcDoctorRestrictiveBandaging , PerkType.LEProc},
            {PerkHash.LEProcEnforcerAirOfHatred , PerkType.LEProc},
            {PerkHash.LEProcEnforcerBustKneecaps , PerkType.LEProc},
            {PerkHash.LEProcEnforcerIgnorePain , PerkType.LEProc},
            {PerkHash.LEProcEnforcerInspireIre , PerkType.LEProc},
            {PerkHash.LEProcEnforcerInspireRage , PerkType.LEProc},
            {PerkHash.LEProcEnforcerRagingBlow , PerkType.LEProc},
            {PerkHash.LEProcEnforcerShieldOfTheOgre , PerkType.LEProc},
            {PerkHash.LEProcEnforcerShrugOffHits , PerkType.LEProc},
            {PerkHash.LEProcEnforcerTearLigaments , PerkType.LEProc},
            {PerkHash.LEProcEnforcerVileRage , PerkType.LEProc},
            {PerkHash.LEProcEnforcerViolationBuffer , PerkType.LEProc},
            {PerkHash.LEProcEnforcerVortexOfHate , PerkType.LEProc},
            {PerkHash.LEProcEngineerAssaultForceRelief , PerkType.LEProc},
            {PerkHash.LEProcEngineerCongenialEncasement , PerkType.LEProc},
            {PerkHash.LEProcEngineerCushionBlows , PerkType.LEProc},
            {PerkHash.LEProcEngineerDestructiveSignal , PerkType.LEProc},
            {PerkHash.LEProcEngineerDestructiveTheorem , PerkType.LEProc},
            {PerkHash.LEProcEngineerDroneExplosives , PerkType.LEProc},
            {PerkHash.LEProcEngineerDroneMissiles , PerkType.LEProc},
            {PerkHash.LEProcEngineerEndureBarrage , PerkType.LEProc},
            {PerkHash.LEProcEngineerEnergyTransfer , PerkType.LEProc},
            {PerkHash.LEProcEngineerPersonalProtection , PerkType.LEProc},
            {PerkHash.LEProcEngineerReactiveArmor , PerkType.LEProc},
            {PerkHash.LEProcEngineerSplinterPreservation , PerkType.LEProc},
            {PerkHash.LEProcFixerBackyardBandages , PerkType.LEProc},
            {PerkHash.LEProcFixerBendingTheRules , PerkType.LEProc},
            {PerkHash.LEProcFixerBootlegRemedies , PerkType.LEProc},
            {PerkHash.LEProcFixerContaminatedBullets , PerkType.LEProc},
            {PerkHash.LEProcFixerDirtyTricks , PerkType.LEProc},
            {PerkHash.LEProcFixerEscapeTheSystem , PerkType.LEProc},
            {PerkHash.LEProcFixerFightingChance , PerkType.LEProc},
            {PerkHash.LEProcFixerFishInABarrel , PerkType.LEProc},
            {PerkHash.LEProcFixerIntenseMetabolism , PerkType.LEProc},
            {PerkHash.LEProcFixerSlipThemAMickey , PerkType.LEProc},
            {PerkHash.LEProcFixerUndergroundSutures , PerkType.LEProc},
            {PerkHash.LEProcFixerLucksCalamity , PerkType.LEProc},
            {PerkHash.LEProcKeeperAmbientPurification , PerkType.LEProc},
            {PerkHash.LEProcKeeperBenevolentBarrier , PerkType.LEProc},
            {PerkHash.LEProcKeeperEschewTheFaithless , PerkType.LEProc},
            {PerkHash.LEProcKeeperFaithfulReconstruction , PerkType.LEProc},
            {PerkHash.LEProcKeeperHonorRestored , PerkType.LEProc},
            {PerkHash.LEProcKeeperIgnoreTheUnrepentant , PerkType.LEProc},
            {PerkHash.LEProcKeeperPureStrike , PerkType.LEProc},
            {PerkHash.LEProcKeeperRighteousSmite , PerkType.LEProc},
            {PerkHash.LEProcKeeperRighteousStrike , PerkType.LEProc},
            {PerkHash.LEProcKeeperSubjugation , PerkType.LEProc},
            {PerkHash.LEProcKeeperSymbioticBypass , PerkType.LEProc},
            {PerkHash.LEProcKeeperVirtuousReaper , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistAbsoluteFist , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistAttackLigaments , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistDebilitatingStrike , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistDisruptKi , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistHealingMeditation , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistMedicinalRemedy , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistSelfReconstruction , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistSmashingFist , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistStingingFist , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistStrengthenKi , PerkType.LEProc},
            {PerkHash.LEProcMartialArtistStrengthenSpirit , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistAnticipatedEvasion , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistDiffuseRage , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistEconomicNanobotUse , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistEgoStrike , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistMindWail , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistNanobotContingentArrest , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistRegainFocus , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistSowDespair , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistSowDoubt , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistSuppressFury , PerkType.LEProc},
            {PerkHash.LEProcMetaPhysicistThoughtfulMeans , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianAcceleratedReality , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianCircularLogic , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianHarvestEnergy , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianIncreaseMomentum , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianLayeredAmnesty , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianLoopingService , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianOptimizedLibrary , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianPoweredNanoFortress , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianSourceTap , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianThermalReprieve , PerkType.LEProc},
            {PerkHash.LEProcNanoTechnicianUnstableLibrary , PerkType.LEProc},
            {PerkHash.LEProcShadeBlackenedLegacy , PerkType.LEProc},
            {PerkHash.LEProcShadeBlackheart , PerkType.LEProc},
            {PerkHash.LEProcShadeConcealedSurprise , PerkType.LEProc},
            {PerkHash.LEProcShadeDeviousSpirit , PerkType.LEProc},
            {PerkHash.LEProcShadeDrainEssence , PerkType.LEProc},
            {PerkHash.LEProcShadeElusiveSpirit , PerkType.LEProc},
            {PerkHash.LEProcShadeMisdirection , PerkType.LEProc},
            {PerkHash.LEProcShadeSapLife , PerkType.LEProc},
            {PerkHash.LEProcShadeShadowedGift , PerkType.LEProc},
            {PerkHash.LEProcShadeSiphonBeing , PerkType.LEProc},
            {PerkHash.LEProcShadeToxicConfusion , PerkType.LEProc},
            {PerkHash.LEProcShadeTwistedCaress , PerkType.LEProc},
            {PerkHash.LEProcSoldierConcussiveShot , PerkType.LEProc},
            {PerkHash.LEProcSoldierDeepSixInitiative , PerkType.LEProc},
            {PerkHash.LEProcSoldierEmergencyBandages , PerkType.LEProc},
            {PerkHash.LEProcSoldierFuriousAmmunition , PerkType.LEProc},
            {PerkHash.LEProcSoldierFuseBodyArmor , PerkType.LEProc},
            {PerkHash.LEProcSoldierGearAssaultAbsorption , PerkType.LEProc},
            {PerkHash.LEProcSoldierGrazeJugularVein , PerkType.LEProc},
            {PerkHash.LEProcSoldierOnTheDouble , PerkType.LEProc},
            {PerkHash.LEProcSoldierReconditioned , PerkType.LEProc},
            {PerkHash.LEProcSoldierShootArtery , PerkType.LEProc},
            {PerkHash.LEProcSoldierSuccessfulTargeting , PerkType.LEProc},
            {PerkHash.LEProcSoldierTargetAcquired , PerkType.LEProc},
            {PerkHash.LEProcTraderAccumulatedInterest , PerkType.LEProc},
            {PerkHash.LEProcTraderDebtCollection , PerkType.LEProc},
            {PerkHash.LEProcTraderDepleteAssets , PerkType.LEProc},
            {PerkHash.LEProcTraderEscrow , PerkType.LEProc},
            {PerkHash.LEProcTraderExchangeProduct , PerkType.LEProc},
            {PerkHash.LEProcTraderPaymentPlan , PerkType.LEProc},
            {PerkHash.LEProcTraderRebate , PerkType.LEProc},
            {PerkHash.LEProcTraderRefinanceLoans , PerkType.LEProc},
            {PerkHash.LEProcTraderRigidLiquidation , PerkType.LEProc},
            {PerkHash.LEProcTraderUnexpectedBonus , PerkType.LEProc},
            {PerkHash.LEProcTraderUnforgivenDebts , PerkType.LEProc},
            {PerkHash.LEProcTraderUnopenedLetter , PerkType.LEProc},
            {PerkHash.Lifeblood , PerkType.SelfHeal},
            {PerkHash.LightBullet , PerkType.TargetDamage},
            {PerkHash.LightKiller , PerkType.TargetDamage},
            {PerkHash.Limber , PerkType.Disabled},
            {PerkHash.MaliciousProhibition , PerkType.TargetDamage},
            {PerkHash.MarkOfSufferance , PerkType.SelfHeal},
            {PerkHash.MarkOfTheUnclean , PerkType.TargetDamage},
            {PerkHash.MarkOfTheUnhallowed , PerkType.TargetDamage},
            {PerkHash.MarkOfVengeance , PerkType.TargetDamage},
            {PerkHash.Medallion , PerkType.TargetDamage},
            {PerkHash.MemoryScrabble , PerkType.TargetDamage},
            {PerkHash.Mistreatment , PerkType.TargetDamage},
            {PerkHash.MongoRage , PerkType.TargetDamage},
            {PerkHash.Moonmist , PerkType.TargetDamage},
            {PerkHash.MuzzleOverload , PerkType.TargetDamage},
            {PerkHash.MyOwnFortress , PerkType.TargetDamage},
            {PerkHash.NanoFeast , PerkType.TargetDamage},
            {PerkHash.NanoHeal , PerkType.SelfNano},
            {PerkHash.NanoShakes , PerkType.TargetDamage},
            {PerkHash.NanoTransmission , PerkType.Disabled},
            {PerkHash.NapalmSpray , PerkType.TargetDamage},
            {PerkHash.NCUBooster , PerkType.SelfBuff},
            {PerkHash.NeutroniumSlug , PerkType.TargetDamage},
            {PerkHash.NightKiller , PerkType.TargetDamage},
            {PerkHash.NocturnalStrike , PerkType.TargetDamage},
            {PerkHash.NotumOverflow , PerkType.TargetDamage},
            {PerkHash.NotumShield , PerkType.Disabled},
            {PerkHash.Numb , PerkType.TargetDamage},
            {PerkHash.Obliterate , PerkType.TargetDamage},
            {PerkHash.Opening , PerkType.TargetDamage},
            {PerkHash.OpportunityKnocks , PerkType.TargetDamage},
            {PerkHash.OptimizeBotProtocol , PerkType.PetBuff},
            {PerkHash.Overrule , PerkType.TargetDamage},
            {PerkHash.OverwhelmingMight , PerkType.TargetDamage},
            {PerkHash.PainLance , PerkType.TargetDamage},
            {PerkHash.PeelLayers , PerkType.TargetDamage},
            {PerkHash.Pen , PerkType.TargetDamage},
            {PerkHash.Perforate , PerkType.Disabled},
            {PerkHash.PinpointStrike , PerkType.TargetDamage},
            {PerkHash.PointBlank , PerkType.TargetDamage},
            {PerkHash.PoisonSprinkle , PerkType.TargetDamage},
            {PerkHash.Popshot , PerkType.TargetDamage},
            {PerkHash.PowerBlast , PerkType.TargetDamage},
            {PerkHash.PowerBolt , PerkType.TargetDamage},
            {PerkHash.PowerCombo , PerkType.TargetDamage},
            {PerkHash.PowerOfLight , PerkType.TargetDamage},
            {PerkHash.PowerShock , PerkType.TargetDamage},
            {PerkHash.PowerVolley , PerkType.TargetDamage},
            {PerkHash.ProgramOverload , PerkType.TargetDamage},
            {PerkHash.Pulverize , PerkType.TargetDamage},
            {PerkHash.Puppeteer , PerkType.PetBuff},
            {PerkHash.Purge1 , PerkType.Clease},
            {PerkHash.Purge2 , PerkType.Clease},
            {PerkHash.Purify , PerkType.Clease},
            {PerkHash.PurpleHeart , PerkType.Disabled},
            {PerkHash.QuarkContainmentField , PerkType.TargetDamage},
            {PerkHash.QuickBash , PerkType.TargetDamage},
            {PerkHash.QuickCut , PerkType.TargetDamage},
            {PerkHash.QuickShot , PerkType.Custom},
            {PerkHash.ReapLife , PerkType.TargetDamage},
            {PerkHash.Recalibrate , PerkType.TargetDamage},
            {PerkHash.ReconstructDNA , PerkType.SelfHeal},
            {PerkHash.Reconstruction , PerkType.PetHeal},
            {PerkHash.RedDawn , PerkType.SelfHeal},
            {PerkHash.RedDusk , PerkType.TargetDamage},
            {PerkHash.RedeemLastWish , PerkType.SelfBuff},
            {PerkHash.RegainNano , PerkType.SelfNano},

            {PerkHash.ReinforceSlugs , PerkType.Disabled},
            //{PerkHash.ReinforceSlugs , PerkType.TargetDamage},

            {PerkHash.Reject , PerkType.TargetDamage},
            {PerkHash.Removal1 , PerkType.Clease},
            {PerkHash.Removal2 , PerkType.Clease},
            {PerkHash.RepairPet , PerkType.PetHeal},
            {PerkHash.RibbonFlesh , PerkType.TargetDamage},
            {PerkHash.RighteousFury , PerkType.TargetDamage},
            {PerkHash.RighteousWrath , PerkType.TargetDamage},
            {PerkHash.RitualOfBlood , PerkType.Disabled},
            {PerkHash.RitualOfDevotion , PerkType.Disabled},
            {PerkHash.RitualOfSpirit , PerkType.Disabled},
            {PerkHash.RitualOfZeal , PerkType.Disabled},
            {PerkHash.SabotageQuarkField , PerkType.TargetDamage},
            {PerkHash.Sacrifice , PerkType.Disabled},
            {PerkHash.SealWounds , PerkType.TargetDamage},
            {PerkHash.SeismicSmash , PerkType.TargetDamage},
            {PerkHash.SeppukuSlash , PerkType.TargetDamage},
            {PerkHash.ShadowBullet , PerkType.TargetDamage},
            {PerkHash.ShadowKiller , PerkType.TargetDamage},
            {PerkHash.ShadowStab , PerkType.Disabled},
            {PerkHash.ShutdownRemoval , PerkType.Clease},
            {PerkHash.SilentPlague , PerkType.TargetDamage},
            {PerkHash.SiphonBox , PerkType.Disabled},
            {PerkHash.SkillDrainRemoval , PerkType.Clease},
            {PerkHash.SliceAndDice , PerkType.TargetDamage},
            {PerkHash.SnipeShot1 , PerkType.TargetDamage},
            {PerkHash.SnipeShot2 , PerkType.TargetDamage},
            {PerkHash.SoftenUp , PerkType.TargetDamage},
            {PerkHash.SolidSlug , PerkType.TargetDamage},
            {PerkHash.SpectatorWrath , PerkType.TargetDamage},
            {PerkHash.Sphere , PerkType.TargetDamage},
            {PerkHash.SpiritDissolution , PerkType.TargetDamage},
            {PerkHash.SpiritOfBlessing , PerkType.TargetDamage},
            {PerkHash.SpiritOfPurity , PerkType.TargetDamage},
            {PerkHash.Stab , PerkType.TargetDamage},
            {PerkHash.StoneFist , PerkType.TargetDamage},
            {PerkHash.Stoneworks , PerkType.TargetDamage},
            {PerkHash.StopNotumFlow , PerkType.TargetDamage},
            {PerkHash.StripNano , PerkType.TargetDamage},
            {PerkHash.Succumb , PerkType.TargetDamage},
            {PerkHash.Supernova , PerkType.TargetDamage},
            {PerkHash.SuppressivePrimer , PerkType.TargetDamage},
            {PerkHash.SupressiveHorde , PerkType.DamageBuff},
            {PerkHash.Survival , PerkType.SelfHeal},
            {PerkHash.Sword , PerkType.TargetDamage},
            {PerkHash.Symbiosis , PerkType.TargetDamage},
            {PerkHash.TackyHack , PerkType.TargetDamage},
            {PerkHash.TaintWounds , PerkType.TargetDamage},
            {PerkHash.TapNotumSource , PerkType.SelfNano},
            {PerkHash.TapVitae , PerkType.TargetDamage},
            {PerkHash.Taunt , PerkType.TargetDamage},
            {PerkHash.TauntBox , PerkType.Disabled},
            {PerkHash.TeamHaleAndHearty , PerkType.Clease},
            {PerkHash.TeamHeal , PerkType.SelfHeal},
            {PerkHash.TheDirector , PerkType.Disabled},
            {PerkHash.ThermalDetonation , PerkType.TargetDamage},
            {PerkHash.ThermalPrimer , PerkType.TargetDamage},
            {PerkHash.TheShot , PerkType.TargetDamage},
            {PerkHash.Tick , PerkType.TargetDamage},
            {PerkHash.ToxicShock , PerkType.TargetDamage},
            {PerkHash.Tracer , PerkType.TargetDamage},
            {PerkHash.Tranquilizer , PerkType.TargetDamage},
            {PerkHash.Transfix , PerkType.TargetDamage},
            {PerkHash.TreatmentTransfer , PerkType.Disabled},
            {PerkHash.TremorHand , PerkType.TargetDamage},
            {PerkHash.TriangulateTarget , PerkType.TargetDamage},
            {PerkHash.TriggerHappy , PerkType.TargetDamage},
            {PerkHash.TrollForm , PerkType.Disabled},
            {PerkHash.UnhallowedFury , PerkType.TargetDamage},
            {PerkHash.UnhallowedWrath , PerkType.TargetDamage},
            {PerkHash.UnsealedBlight , PerkType.Disabled},
            {PerkHash.UnsealedContagion , PerkType.Disabled},
            {PerkHash.UnsealedPestilence , PerkType.Disabled},
            {PerkHash.Utilize , PerkType.TargetDamage},
            {PerkHash.Vaccinate1 , PerkType.Clease},
            {PerkHash.Vaccinate2 , PerkType.Clease},
            {PerkHash.Violence , PerkType.TargetDamage},
            {PerkHash.ViralCombination , PerkType.TargetDamage},
            {PerkHash.ViralWipe , PerkType.TargetDamage},
            {PerkHash.VitalShock , PerkType.TargetDamage},
            {PerkHash.WeaponBash , PerkType.TargetDamage},
            {PerkHash.WitOfTheAtrox , PerkType.Custom},
            {PerkHash.ZapNano , PerkType.TargetDamage},
        };
    }

    enum PerkType
    {
        Generic, SelfBuff, SelfHeal, SelfNano, TeamHeal, TeamNano, TargetDamage, DamageBuff, PetBuff, PetHeal, LEProc, Clease, Custom, Disabled, Unknown
    }
}
