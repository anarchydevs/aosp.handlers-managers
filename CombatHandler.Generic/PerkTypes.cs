﻿using AOSharp.Common.GameData;
using System.Collections.Generic;

namespace CombatHandler.Generic
{
    class PerkTypes
    {
        public static PerkType GetPerkType(PerkHash perkHash)
        {
            if (!HashMap.ContainsKey(perkHash))
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
            {PerkHash.BattlegroupHeal1 , PerkType.Disabled},
            {PerkHash.BattlegroupHeal2 , PerkType.Disabled},
            {PerkHash.BattlegroupHeal3 , PerkType.Disabled},
            {PerkHash.BattlegroupHeal4 , PerkType.Disabled},
            {PerkHash.Bearhug , PerkType.TargetDamage},
            {PerkHash.Beckoning , PerkType.TargetDamage},
            {PerkHash.BigSmash , PerkType.TargetDamage},
            {PerkHash.BioShield , PerkType.CombatBuff},
            {PerkHash.BioCocoon , PerkType.Disabled},
            {PerkHash.BioRejuvenation , PerkType.TeamHeal},
            {PerkHash.BioRegrowth , PerkType.Disabled},
            {PerkHash.BladeOfNight , PerkType.TargetDamage},
            {PerkHash.BladeWhirlwind , PerkType.CombatBuff},
            {PerkHash.BlastNano , PerkType.TargetDamage},
            {PerkHash.BleedingWounds , PerkType.TargetDamage},
            {PerkHash.BlessingOfLife , PerkType.TeamHeal},
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
            {PerkHash.ChannelRage , PerkType.Pet_Buff},
            {PerkHash.ChaosRitual , PerkType.TargetDamage},
            {PerkHash.ChaoticAssumption , PerkType.TargetDamage},
            {PerkHash.ChaoticEnergy , PerkType.Disabled},
            {PerkHash.ChaoticModulation , PerkType.Disabled},
            {PerkHash.Charge , PerkType.Disabled},
            {PerkHash.ChemicalBlindness , PerkType.TargetDamage},
            {PerkHash.ChiConductor , PerkType.TargetDamage},
            {PerkHash.Clearshot , PerkType.TargetDamage},
            {PerkHash.Clearsight , PerkType.TargetDamage},
            {PerkHash.Cleave , PerkType.TargetDamage},
            {PerkHash.Clipfever , PerkType.Disabled},
            {PerkHash.CloseCall , PerkType.Disabled},
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
            {PerkHash.CrushBone , PerkType.Disabled},
            {PerkHash.Cure1 , PerkType.AAO_Dots_Cleanse}, // aao debuffs and dots
            {PerkHash.Cure2 , PerkType.AAO_Dots_Cleanse}, // aao debuffs and dots
            {PerkHash.CuringTouch , PerkType.SelfHeal},
            {PerkHash.DanceOfFools , PerkType.Disabled},
            {PerkHash.DazzleWithLights , PerkType.TargetDamage},
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
            {PerkHash.DodgeTheBlame , PerkType.CombatBuff},
            {PerkHash.DoomTouch , PerkType.TargetDamage},
            {PerkHash.DoubleShot , PerkType.TargetDamage},
            {PerkHash.DoubleStab , PerkType.Disabled},
            {PerkHash.Dragonfire , PerkType.TargetDamage},
            {PerkHash.DrawBlood , PerkType.TargetDamage},
            {PerkHash.EasyShot , PerkType.TargetDamage},
            {PerkHash.EatBullets , PerkType.TargetDamage},
            {PerkHash.ECM1 , PerkType.NanoShutdown_TraderDebuff_Cleanse}, //Nano shutdown, trader debuffs
            {PerkHash.ECM2 , PerkType.NanoShutdown_TraderDebuff_Cleanse}, //Nano shutdown, trader debuffs
            {PerkHash.ElementaryTeleportation , PerkType.Disabled},
            {PerkHash.EncaseInStone , PerkType.Disabled},
            {PerkHash.Energize , PerkType.CombatBuff},
            {PerkHash.EnhancedHeal , PerkType.SelfHeal},
            {PerkHash.Escape , PerkType.Disabled},
            {PerkHash.EtherealTouch , PerkType.TargetDamage},
            {PerkHash.EvasiveStance , PerkType.Disabled},
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
            {PerkHash.FreakShield , PerkType.CombatBuff},
            {PerkHash.FullFrontal , PerkType.TargetDamage},
            {PerkHash.Fuzz , PerkType.TargetDamage},
            {PerkHash.Gore , PerkType.Disabled},
            {PerkHash.Governance , PerkType.Disabled},
            {PerkHash.Grasp , PerkType.TargetDamage},
            {PerkHash.GreatPurge , PerkType.Root_Snare_Cleanse}, //Root/Snare
            {PerkHash.GripOfColossus , PerkType.TargetDamage},
            {PerkHash.GroinKick , PerkType.TargetDamage},
            {PerkHash.Guardian , PerkType.CombatBuff},
            {PerkHash.Guesstimate , PerkType.TargetDamage},
            {PerkHash.GuttingBlow , PerkType.TargetDamage},
            {PerkHash.HaleAndHearty , PerkType.AOO_Trader_DOts_Init_Cleanse}, //AAO debuffs, Tradeder debuffs, Dots, Init debuffs
            {PerkHash.HammerAndAnvil , PerkType.DamageBuff},
            {PerkHash.HarmonizeBodyAndMind , PerkType.SelfHeal},
            {PerkHash.Hatred , PerkType.TargetDamage},
            {PerkHash.Headbutt , PerkType.TargetDamage},
            {PerkHash.Heal , PerkType.SelfHeal},
            {PerkHash.Hecatomb , PerkType.Disabled},
            {PerkHash.Highway , PerkType.CombatBuff},
            {PerkHash.HonoringTheAncients , PerkType.TargetDamage},
            {PerkHash.HostileTakeover , PerkType.TargetDamage},
            {PerkHash.IgnitionFlare , PerkType.TargetDamage},
            {PerkHash.Impale , PerkType.Disabled},
            {PerkHash.Implode , PerkType.TargetDamage},
            {PerkHash.Incapacitate , PerkType.TargetDamage},
            {PerkHash.InitialStrike , PerkType.TargetDamage},
            {PerkHash.Insight , PerkType.CombatBuff},
            {PerkHash.InstallExplosiveDevice , PerkType.Custom},
            {PerkHash.InstallNotumDepletionDevice , PerkType.Custom},
            {PerkHash.InsuranceClaim , PerkType.Disabled},
            {PerkHash.JarringBurst , PerkType.TargetDamage},
            {PerkHash.KaMon , PerkType.TargetDamage},
            {PerkHash.KenFi , PerkType.Combat_PetBuff},
            {PerkHash.KenSi , PerkType.TargetDamage},
            {PerkHash.KnowledgeEnhancer , PerkType.TargetDamage},
            {PerkHash.Lacerate , PerkType.Disabled},
            {PerkHash.LaserPaintTarget , PerkType.TargetDamage},
            {PerkHash.LayOnHands , PerkType.SelfHeal},
            {PerkHash.Leadership , PerkType.Disabled},
            {PerkHash.LegShot , PerkType.Disabled},
            {PerkHash.LEProcAdventurerAesirAbsorption , PerkType.Disabled},
            {PerkHash.LEProcAdventurerBasicDressing , PerkType.Disabled},
            {PerkHash.LEProcAdventurerCharringBlow , PerkType.Disabled},
            {PerkHash.LEProcAdventurerCombustion , PerkType.Disabled},
            {PerkHash.LEProcAdventurerFerociousHits , PerkType.Disabled},
            {PerkHash.LEProcAdventurerHealingHerbs , PerkType.Disabled},
            {PerkHash.LEProcAdventurerMacheteFlurry , PerkType.Disabled},
            {PerkHash.LEProcAdventurerMacheteSlice , PerkType.Disabled},
            {PerkHash.LEProcAdventurerRestoreVigor , PerkType.Disabled},
            {PerkHash.LEProcAdventurerSelfPreservation , PerkType.Disabled},
            {PerkHash.LEProcAdventurerSkinProtection , PerkType.Disabled},
            {PerkHash.LEProcAdventurerSoothingHerbs , PerkType.Disabled},
            {PerkHash.LEProcAgentBrokenAnkle , PerkType.Disabled},
            {PerkHash.LEProcAgentCellKiller , PerkType.Disabled},
            {PerkHash.LEProcAgentDisableCuffs , PerkType.Disabled},
            {PerkHash.LEProcAgentGrimReaper , PerkType.Disabled},
            {PerkHash.LEProcAgentImprovedFocus , PerkType.Disabled},
            {PerkHash.LEProcAgentIntenseMetabolism , PerkType.Disabled},
            {PerkHash.LEProcAgentLaserAim , PerkType.Disabled},
            {PerkHash.LEProcAgentMinorNanobotEnhance , PerkType.Disabled},
            {PerkHash.LEProcAgentPlasteelPiercingRounds , PerkType.Disabled},
            {PerkHash.LEProcAgentNanoEnhancedTargeting , PerkType.Disabled},
            {PerkHash.LEProcAgentNoEscape , PerkType.Disabled},
            {PerkHash.LEProcAgentNotumChargedRounds , PerkType.Disabled},
            {PerkHash.LEProcBureaucratDeflation , PerkType.Disabled},
            {PerkHash.LEProcBureaucratFormsInTriplicate , PerkType.Disabled},
            {PerkHash.LEProcBureaucratInflationAdjustment , PerkType.Disabled},
            {PerkHash.LEProcBureaucratLostPaperwork , PerkType.Disabled},
            {PerkHash.LEProcBureaucratMobilityEmbargo , PerkType.Disabled},
            {PerkHash.LEProcBureaucratNextWindowOver , PerkType.Disabled},
            {PerkHash.LEProcBureaucratPapercut , PerkType.Disabled},
            {PerkHash.LEProcBureaucratPleaseHold , PerkType.Disabled},
            {PerkHash.LEProcBureaucratSocialServices , PerkType.Disabled},
            {PerkHash.LEProcBureaucratTaxAudit , PerkType.Disabled},
            {PerkHash.LEProcBureaucratWaitInThatQueue , PerkType.Disabled},
            {PerkHash.LEProcBureaucratWrongWindow , PerkType.Disabled},
            {PerkHash.LEProcDoctorAnatomicBlight , PerkType.Disabled},
            {PerkHash.LEProcDoctorAnesthetic , PerkType.Disabled},
            {PerkHash.LEProcDoctorAntiseptic , PerkType.Disabled},
            {PerkHash.LEProcDoctorAstringent , PerkType.Disabled},
            {PerkHash.LEProcDoctorBloodTransfusion , PerkType.Disabled},
            {PerkHash.LEProcDoctorDangerousCulture , PerkType.Disabled},
            {PerkHash.LEProcDoctorHealingCare , PerkType.Disabled},
            {PerkHash.LEProcDoctorInflammation , PerkType.Disabled},
            {PerkHash.LEProcDoctorMassiveVitaePlan , PerkType.Disabled},
            {PerkHash.LEProcDoctorMuscleMemory , PerkType.Disabled},
            {PerkHash.LEProcDoctorPathogen , PerkType.Disabled},
            {PerkHash.LEProcDoctorRestrictiveBandaging , PerkType.Disabled},
            {PerkHash.LEProcEnforcerAirOfHatred , PerkType.Disabled},
            {PerkHash.LEProcEnforcerBustKneecaps , PerkType.Disabled},
            {PerkHash.LEProcEnforcerIgnorePain , PerkType.Disabled},
            {PerkHash.LEProcEnforcerInspireIre , PerkType.Disabled},
            {PerkHash.LEProcEnforcerInspireRage , PerkType.Disabled},
            {PerkHash.LEProcEnforcerRagingBlow , PerkType.Disabled},
            {PerkHash.LEProcEnforcerShieldOfTheOgre , PerkType.Disabled},
            {PerkHash.LEProcEnforcerShrugOffHits , PerkType.Disabled},
            {PerkHash.LEProcEnforcerTearLigaments , PerkType.Disabled},
            {PerkHash.LEProcEnforcerVileRage , PerkType.Disabled},
            {PerkHash.LEProcEnforcerViolationBuffer , PerkType.Disabled},
            {PerkHash.LEProcEnforcerVortexOfHate , PerkType.Disabled},
            {PerkHash.LEProcEngineerAssaultForceRelief , PerkType.Disabled},
            {PerkHash.LEProcEngineerCongenialEncasement , PerkType.Disabled},
            {PerkHash.LEProcEngineerCushionBlows , PerkType.Disabled},
            {PerkHash.LEProcEngineerDestructiveSignal , PerkType.Disabled},
            {PerkHash.LEProcEngineerDestructiveTheorem , PerkType.Disabled},
            {PerkHash.LEProcEngineerDroneExplosives , PerkType.Disabled},
            {PerkHash.LEProcEngineerDroneMissiles , PerkType.Disabled},
            {PerkHash.LEProcEngineerEndureBarrage , PerkType.Disabled},
            {PerkHash.LEProcEngineerEnergyTransfer , PerkType.Disabled},
            {PerkHash.LEProcEngineerPersonalProtection , PerkType.Disabled},
            {PerkHash.LEProcEngineerReactiveArmor , PerkType.Disabled},
            {PerkHash.LEProcEngineerSplinterPreservation , PerkType.Disabled},
            {PerkHash.LEProcFixerBackyardBandages , PerkType.Disabled},
            {PerkHash.LEProcFixerBendingTheRules , PerkType.Disabled},
            {PerkHash.LEProcFixerBootlegRemedies , PerkType.Disabled},
            {PerkHash.LEProcFixerContaminatedBullets , PerkType.Disabled},
            {PerkHash.LEProcFixerDirtyTricks , PerkType.Disabled},
            {PerkHash.LEProcFixerEscapeTheSystem , PerkType.Disabled},
            {PerkHash.LEProcFixerFightingChance , PerkType.Disabled},
            {PerkHash.LEProcFixerFishInABarrel , PerkType.Disabled},
            {PerkHash.LEProcFixerIntenseMetabolism , PerkType.Disabled},
            {PerkHash.LEProcFixerSlipThemAMickey , PerkType.Disabled},
            {PerkHash.LEProcFixerUndergroundSutures , PerkType.Disabled},
            {PerkHash.LEProcFixerLucksCalamity , PerkType.Disabled},
            {PerkHash.LEProcKeeperAmbientPurification , PerkType.Disabled},
            {PerkHash.LEProcKeeperBenevolentBarrier , PerkType.Disabled},
            {PerkHash.LEProcKeeperEschewTheFaithless , PerkType.Disabled},
            {PerkHash.LEProcKeeperFaithfulReconstruction , PerkType.Disabled},
            {PerkHash.LEProcKeeperHonorRestored , PerkType.Disabled},
            {PerkHash.LEProcKeeperIgnoreTheUnrepentant , PerkType.Disabled},
            {PerkHash.LEProcKeeperPureStrike , PerkType.Disabled},
            {PerkHash.LEProcKeeperRighteousSmite , PerkType.Disabled},
            {PerkHash.LEProcKeeperRighteousStrike , PerkType.Disabled},
            {PerkHash.LEProcKeeperSubjugation , PerkType.Disabled},
            {PerkHash.LEProcKeeperSymbioticBypass , PerkType.Disabled},
            {PerkHash.LEProcKeeperVirtuousReaper , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistAbsoluteFist , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistAttackLigaments , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistDebilitatingStrike , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistDisruptKi , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistHealingMeditation , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistMedicinalRemedy , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistSelfReconstruction , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistSmashingFist , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistStingingFist , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistStrengthenKi , PerkType.Disabled},
            {PerkHash.LEProcMartialArtistStrengthenSpirit , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistAnticipatedEvasion , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistDiffuseRage , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistEconomicNanobotUse , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistEgoStrike , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistMindWail , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistNanobotContingentArrest , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistRegainFocus , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistSowDespair , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistSowDoubt , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistSuppressFury , PerkType.Disabled},
            {PerkHash.LEProcMetaPhysicistThoughtfulMeans , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianAcceleratedReality , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianCircularLogic , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianHarvestEnergy , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianIncreaseMomentum , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianLayeredAmnesty , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianLoopingService , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianOptimizedLibrary , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianPoweredNanoFortress , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianSourceTap , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianThermalReprieve , PerkType.Disabled},
            {PerkHash.LEProcNanoTechnicianUnstableLibrary , PerkType.Disabled},
            {PerkHash.LEProcShadeBlackenedLegacy , PerkType.Disabled},
            {PerkHash.LEProcShadeBlackheart , PerkType.Disabled},
            {PerkHash.LEProcShadeConcealedSurprise , PerkType.Disabled},
            {PerkHash.LEProcShadeDeviousSpirit , PerkType.Disabled},
            {PerkHash.LEProcShadeDrainEssence , PerkType.Disabled},
            {PerkHash.LEProcShadeElusiveSpirit , PerkType.Disabled},
            {PerkHash.LEProcShadeMisdirection , PerkType.Disabled},
            {PerkHash.LEProcShadeSapLife , PerkType.Disabled},
            {PerkHash.LEProcShadeShadowedGift , PerkType.Disabled},
            {PerkHash.LEProcShadeSiphonBeing , PerkType.Disabled},
            {PerkHash.LEProcShadeToxicConfusion , PerkType.Disabled},
            {PerkHash.LEProcShadeTwistedCaress , PerkType.Disabled},
            {PerkHash.LEProcSoldierConcussiveShot , PerkType.Disabled},
            {PerkHash.LEProcSoldierDeepSixInitiative , PerkType.Disabled},
            {PerkHash.LEProcSoldierEmergencyBandages , PerkType.Disabled},
            {PerkHash.LEProcSoldierFuriousAmmunition , PerkType.Disabled},
            {PerkHash.LEProcSoldierFuseBodyArmor , PerkType.Disabled},
            {PerkHash.LEProcSoldierGearAssaultAbsorption , PerkType.Disabled},
            {PerkHash.LEProcSoldierGrazeJugularVein , PerkType.Disabled},
            {PerkHash.LEProcSoldierOnTheDouble , PerkType.Disabled},
            {PerkHash.LEProcSoldierReconditioned , PerkType.Disabled},
            {PerkHash.LEProcSoldierShootArtery , PerkType.Disabled},
            {PerkHash.LEProcSoldierSuccessfulTargeting , PerkType.Disabled},
            {PerkHash.LEProcSoldierTargetAcquired , PerkType.Disabled},
            {PerkHash.LEProcTraderAccumulatedInterest , PerkType.Disabled},
            {PerkHash.LEProcTraderDebtCollection , PerkType.Disabled},
            {PerkHash.LEProcTraderDepleteAssets , PerkType.Disabled},
            {PerkHash.LEProcTraderEscrow , PerkType.Disabled},
            {PerkHash.LEProcTraderExchangeProduct , PerkType.Disabled},
            {PerkHash.LEProcTraderPaymentPlan , PerkType.Disabled},
            {PerkHash.LEProcTraderRebate , PerkType.Disabled},
            {PerkHash.LEProcTraderRefinanceLoans , PerkType.Disabled},
            {PerkHash.LEProcTraderRigidLiquidation , PerkType.Disabled},
            {PerkHash.LEProcTraderUnexpectedBonus , PerkType.Disabled},
            {PerkHash.LEProcTraderUnforgivenDebts , PerkType.Disabled},
            {PerkHash.LEProcTraderUnopenedLetter , PerkType.Disabled},
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
            {PerkHash.Moonmist , PerkType.CombatBuff},
            {PerkHash.MuzzleOverload , PerkType.TargetDamage},
            {PerkHash.MyOwnFortress , PerkType.CombatBuff},
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
            {PerkHash.OptimizeBotProtocol , PerkType.Combat_PetBuff},
            {PerkHash.Overrule , PerkType.CombatBuff},
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
            {PerkHash.PowerCombo , PerkType.DamageBuff},
            {PerkHash.PowerOfLight , PerkType.TargetDamage},
            {PerkHash.PowerShock , PerkType.TargetDamage},
            {PerkHash.PowerVolley , PerkType.TargetDamage},
            {PerkHash.ProgramOverload , PerkType.CombatBuff},
            {PerkHash.Pulverize , PerkType.TargetDamage},
            {PerkHash.Puppeteer , PerkType.Combat_PetBuff},
            {PerkHash.Purge1 , PerkType.Root_Snare_Cleanse}, //Root/Snare
            {PerkHash.Purge2 , PerkType.Root_Snare_Cleanse}, //Root/Snare
            {PerkHash.Purify , PerkType.TargetDamage},
            {PerkHash.PurpleHeart , PerkType.Disabled},
            {PerkHash.QuarkContainmentField , PerkType.TargetDamage},
            {PerkHash.QuickBash , PerkType.TargetDamage},
            {PerkHash.QuickCut , PerkType.TargetDamage},
            {PerkHash.QuickShot , PerkType.TargetDamage},
            {PerkHash.ReapLife , PerkType.TargetDamage},
            {PerkHash.Recalibrate , PerkType.TargetDamage},
            {PerkHash.ReconstructDNA , PerkType.SelfHeal},
            {PerkHash.Reconstruction , PerkType.Pet_Heal},
            {PerkHash.RedDawn , PerkType.SelfHeal},
            {PerkHash.RedDusk , PerkType.TargetDamage},
            {PerkHash.RedeemLastWish , PerkType.SelfBuff},
            {PerkHash.RegainNano , PerkType.SelfNano},
            {PerkHash.ReinforceSlugs , PerkType.CombatBuff},
            {PerkHash.Reject , PerkType.TargetDamage},
            {PerkHash.Removal1 , PerkType.Snare_Cleanse}, //Snare
            {PerkHash.Removal2 , PerkType.Root_Cleanse}, //Root
            {PerkHash.RepairPet , PerkType.Pet_Heal},
            {PerkHash.RibbonFlesh , PerkType.TargetDamage},
            {PerkHash.RighteousFury , PerkType.TargetDamage},
            {PerkHash.RighteousWrath , PerkType.TargetDamage},
            {PerkHash.RitualOfBlood , PerkType.Disabled},
            {PerkHash.RitualOfDevotion , PerkType.Disabled},
            {PerkHash.RitualOfSpirit , PerkType.Disabled},
            {PerkHash.RitualOfZeal , PerkType.Disabled},
            {PerkHash.SabotageQuarkField , PerkType.TargetDamage},
            {PerkHash.Sacrifice , PerkType.Disabled},
            {PerkHash.SealWounds , PerkType.Dot_Cleanse}, //Dots
            {PerkHash.SeismicSmash , PerkType.TargetDamage},
            {PerkHash.SeppukuSlash , PerkType.TargetDamage},
            {PerkHash.ShadowBullet , PerkType.TargetDamage},
            {PerkHash.ShadowKiller , PerkType.TargetDamage},
            {PerkHash.ShadowStab , PerkType.Disabled},
            {PerkHash.ShutdownRemoval , PerkType.NanoShutdown_Cleanse}, //Nano shutdown
            {PerkHash.SilentPlague , PerkType.TargetDamage},
            {PerkHash.SiphonBox , PerkType.Disabled},
            {PerkHash.SkillDrainRemoval , PerkType.Trader_Debuff_Cleanse}, //Trader debuffs
            {PerkHash.SliceAndDice , PerkType.TargetDamage},
            {PerkHash.SnipeShot1 , PerkType.TargetDamage},
            {PerkHash.SnipeShot2 , PerkType.TargetDamage},
            {PerkHash.SoftenUp , PerkType.TargetDamage},
            {PerkHash.SolidSlug , PerkType.TargetDamage},
            {PerkHash.SpectatorWrath , PerkType.TargetDamage},
            {PerkHash.Sphere , PerkType.Disabled},
            {PerkHash.SpiritDissolution , PerkType.TargetDamage},
            {PerkHash.SpiritOfBlessing , PerkType.TeamHeal},
            {PerkHash.SpiritOfPurity , PerkType.TeamNano},
            {PerkHash.Stab , PerkType.TargetDamage},
            {PerkHash.StoneFist , PerkType.TargetDamage},
            {PerkHash.Stoneworks , PerkType.TargetDamage},
            {PerkHash.StopNotumFlow , PerkType.TargetDamage},
            {PerkHash.StripNano , PerkType.TargetDamage},
            {PerkHash.Succumb , PerkType.TargetDamage},
            {PerkHash.Supernova , PerkType.TargetDamage},
            {PerkHash.SuppressivePrimer , PerkType.TargetDamage},
            {PerkHash.SupressiveHorde , PerkType.DamageBuff},
            {PerkHash.Survival , PerkType.TeamHeal},
            {PerkHash.Sword , PerkType.TargetDamage},
            {PerkHash.Symbiosis , PerkType.TargetDamage},
            {PerkHash.TackyHack , PerkType.CombatBuff   },
            {PerkHash.TaintWounds , PerkType.TargetDamage},
            {PerkHash.TapNotumSource , PerkType.SelfNano},
            {PerkHash.TapVitae , PerkType.TargetDamage},
            {PerkHash.Taunt , PerkType.TargetDamage},
            {PerkHash.TauntBox , PerkType.Disabled},
            {PerkHash.TeamHaleAndHearty , PerkType.AOO_Trader_DOts_Init_Cleanse}, //AAO debuffs, Tradeder debuffs, Dots, Init debuffs
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
            {PerkHash.Vaccinate1 , PerkType.Trader_Debuff_Cleanse}, //Trader debuff
            {PerkHash.Vaccinate2 , PerkType.Trader_Debuff_Cleanse}, //Trader debuff
            {PerkHash.Violence , PerkType.TargetDamage},
            {PerkHash.ViralCombination , PerkType.TargetDamage},
            {PerkHash.ViralWipe , PerkType.Dot_Cleanse}, //Dots
            {PerkHash.VitalShock , PerkType.TargetDamage},
            {PerkHash.WeaponBash , PerkType.TargetDamage},
            {PerkHash.WitOfTheAtrox , PerkType.Disabled},
            {PerkHash.ZapNano , PerkType.TargetDamage},

        };
    }

    enum PerkType
    {
        Generic, SelfBuff, SelfHeal, SelfNano, TeamHeal, TeamNano, TargetDamage, DamageBuff, CombatBuff,
        Pet_Buff, Pet_Heal, LEProc, Cleanse, Custom, Disabled, Unknown, Dot_Cleanse, NanoShutdown_TraderDebuff_Cleanse, Trader_Debuff_Cleanse,
        AAO_Dots_Cleanse, Root_Snare_Cleanse, AOO_Trader_DOts_Init_Cleanse, Snare_Cleanse, Root_Cleanse, NanoShutdown_Cleanse, Combat_PetBuff
    }
}
