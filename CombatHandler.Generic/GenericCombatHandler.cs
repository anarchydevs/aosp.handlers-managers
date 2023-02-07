﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;
using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using static CombatHandler.Generic.PerkCondtionProcessors;
using static SmokeLounge.AOtomation.Messaging.Messages.N3Messages.FullCharacterMessage;

namespace CombatHandler.Generic
{
    public class GenericCombatHandler : AOSharp.Core.Combat.CombatHandler
    {
        private const float PostZonePetCheckBuffer = 5;
        public int EvadeCycleTimeoutSeconds = 180;

        private double _lastPetSyncTime = Time.NormalTime;
        protected double _lastZonedTime = Time.NormalTime;
        protected double _lastCombatTime = double.MinValue;

        public static int BioCocoonPercentage = 0;
        public static int SingleTauntDelay = 0;
        public static int MongoDelay = 0;
        public static int CycleXpPerksDelay = 0;
        public static int CycleSpherePerkDelay = 0;
        public static int CycleWitOfTheAtroxPerkDelay = 0;
        public static int CycleBioRegrowthPerkDelay = 0;
        public static int CycleChallengerDelay = 0;
        public static int CycleRageDelay = 0;
        public static int CycleAbsorbsDelay = 0;
        public static int HealPercentage = 0;
        public static int CompleteHealPercentage = 0;
        public static int HealthDrainPercentage = 0;
        public static int NanoAegisPercentage = 0;
        public static int NullitySpherePercentage = 0;
        public static int IzgimmersWealthPercentage = 0;

        public static int SelfHealPerkPercentage = 0;
        public static int SelfNanoPerkPercentage = 0;
        public static int TeamHealPerkPercentage = 0;
        public static int TeamNanoPerkPercentage = 0;

        public static int BioRegrowthPercentage = 0;

        public static int BattleGroupHeal1Percentage = 0;
        public static int BattleGroupHeal2Percentage = 0;
        public static int BattleGroupHeal3Percentage = 0;
        public static int BattleGroupHeal4Percentage = 0;

        public static int DuckAbsorbsItemPercentage = 0;
        public static int BodyDevAbsorbsItemPercentage = 0;
        public static int StrengthAbsorbsItemPercentage = 0;

        public static int StimHealthPercentage = 0;
        public static int StimNanoPercentage = 0;
        public static int KitHealthPercentage = 0;
        public static int KitNanoPercentage = 0;
        public static string StimTargetName = string.Empty;

        private double CycleXpPerks = 0;
        private double CycleSpherePerk = 0;
        private double CycleWitOfTheAtroxPerk = 0;
        private double CycleBioRegrowthPerk = 0;

        private static double _updateTick;

        private static Window _perkWindow;

        protected readonly string PluginDir;

        protected Settings _settings;

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public static bool IsActiveWindow => GetForegroundWindow() == Process.GetCurrentProcess().MainWindowHandle;

        protected static HashSet<string> debuffTargetsToIgnore = new HashSet<string>
        {
                    "Immortal Guardian",
                    "Mature Abyss Orchid",
                    "Abyss Orchid Sprout",
                    "Tower of Astodan",
                    "Spirit of Judgement",
                    "Guardian Spirit of Purification",
                    "Green Tower",
                    "Blue Tower",
                    "Alien Cocoon",
                    "Sheila Marlene",
                    "Rookie Alien Hunter",
                    "Sean Powell",
                    "Unicorn Guard",
                    "Essence Fragment",
                    "Awakened Xan",
                    "Fanatic",
                    "Harbinger of Pestilence",
                    "Pandemonium Idol"
        };

        protected static HashSet<string> debuffAreaTargetsToIgnore = new HashSet<string>
        {
                    "Immortal Guardian",
                    "Mature Abyss Orchid",
                    "Abyss Orchid Sprout",
                    "Tower of Astodan",
                    "Unicorn Commander Labbe",
                    "Calan-Cur",
                    "Spirit of Judgement",
                    "Wandering Spirit",
                    "Altar of Torture",
                    "Altar of Purification",
                    "Unicorn Coordinator Magnum Blaine",
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
                    "Awakened Xan",
                    "Fanatic",
                    "Peacekeeper Coursey",
                    "Harbinger of Pestilence",
                    "Pandemonium Idol",
                    "Laser Drone",
                    "Heatbeam",
                    "Thermal Detonator",
                    "Unstable Sentry Drone",
                    "Stasis Containment Field",
                    "Assault Drone",
                    "Scalding Flame",
                    "Automated Defense System",
                    "Medical Drone",
                    "Ju-Ju Doll",
                    "Temporal Vortex",
                    "Gateway to the Past",
                    "Gateway to the Present",
                    "Gateway to the Future",
                    "Masked Eleet",
                    "Dust Brigade Security Drone",
                    "Nanovoider",
                    "Punishment",
                    "Flaming Chaos",
                    "Flaming Punishment",
                    "Flaming Vengeance"
        };

        public static IPCChannel IPCChannel;
        public static Config Config { get; private set; }

        public GenericCombatHandler(string pluginDir)
        {
            Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Generic\\{Game.ClientInst}\\Config.json");
            IPCChannel = new IPCChannel(Convert.ToByte(Config.CharSettings[Game.ClientInst].IPCChannel));

            PluginDir = pluginDir;

            _settings = new Settings("CombatHandler");

            RegisterPerkProcessors();
            RegisterPerkProcessor(PerkHash.BioCocoon, BioCocoon);
            RegisterPerkProcessor(PerkHash.DazzleWithLights, Starfall, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.QuickShot, QuickShot, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.Sphere, Sphere, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.WitOfTheAtrox, WitOfTheAtrox, CombatActionPriority.High);
            RegisterPerkProcessor(PerkHash.BioRegrowth, BioRegrowth, CombatActionPriority.High);

            RegisterSpellProcessor(RelevantGenericNanos.FountainOfLife, FountainOfLife);
            RegisterItemProcessor(RelevantGenericItems.FlowerOfLifeLow, RelevantGenericItems.FlowerOfLifeHigh, FlowerOfLife);

            RegisterItemProcessor(RelevantGenericItems.ReflectGraft, RelevantGenericItems.ReflectGraft, ReflectGraft);

            RegisterItemProcessor(RelevantGenericItems.SteamingHotCupOfEnhancedCoffee, RelevantGenericItems.SteamingHotCupOfEnhancedCoffee, Coffee);

            RegisterItemProcessor(RelevantGenericItems.FlurryOfBlowsHigh, RelevantGenericItems.FlurryOfBlowsHigh, DamageItem);
            RegisterItemProcessor(RelevantGenericItems.FlurryOfBlowsLow, RelevantGenericItems.FlurryOfBlowsLow, DamageItem);

            RegisterItemProcessor(RelevantGenericItems.StrengthOfTheImmortal, RelevantGenericItems.StrengthOfTheImmortal, DamageItem);
            RegisterItemProcessor(RelevantGenericItems.MightOfTheRevenant, RelevantGenericItems.MightOfTheRevenant, DamageItem);
            RegisterItemProcessor(RelevantGenericItems.BarrowStrength, RelevantGenericItems.BarrowStrength, DamageItem);

            RegisterItemProcessor(RelevantGenericItems.GnuffsEternalRiftCrystal, RelevantGenericItems.GnuffsEternalRiftCrystal, DamageItem);
            RegisterItemProcessor(RelevantGenericItems.Drone, RelevantGenericItems.Drone, DamageItem);

            RegisterItemProcessor(RelevantGenericItems.WenWen, RelevantGenericItems.WenWen, DamageItem);

            RegisterItemProcessor(RelevantGenericItems.DreadlochEnduranceBooster, RelevantGenericItems.DreadlochEnduranceBooster, EnduranceBooster, CombatActionPriority.High);
            RegisterItemProcessor(RelevantGenericItems.DreadlochEnduranceBoosterNanomageEdition, RelevantGenericItems.DreadlochEnduranceBoosterNanomageEdition, EnduranceBooster, CombatActionPriority.High);
            RegisterItemProcessor(RelevantGenericItems.WitheredFlesh, RelevantGenericItems.WitheredFlesh, WithFlesh, CombatActionPriority.High);
            RegisterItemProcessor(RelevantGenericItems.DesecratedFlesh, RelevantGenericItems.DesecratedFlesh, DescFlesh, CombatActionPriority.High);
            RegisterItemProcessor(RelevantGenericItems.AssaultClassTank, RelevantGenericItems.AssaultClassTank, AssaultClass, CombatActionPriority.High);

            RegisterItemProcessor(RelevantGenericItems.MeteoriteSpikes, RelevantGenericItems.MeteoriteSpikes, SharpObjects);
            RegisterItemProcessor(RelevantGenericItems.TearOfOedipus, RelevantGenericItems.TearOfOedipus, SharpObjects);
            RegisterItemProcessor(RelevantGenericItems.LavaCapsule, RelevantGenericItems.LavaCapsule, SharpObjects);
            RegisterItemProcessor(RelevantGenericItems.KizzermoleGumboil, RelevantGenericItems.KizzermoleGumboil, SharpObjects);
            RegisterItemProcessor(RelevantGenericItems.FallenStar, RelevantGenericItems.FallenStar, SharpObjects);

            RegisterItemProcessor(RelevantGenericItems.HSRLow, RelevantGenericItems.HSRHigh, Grenades);

            RegisterItemProcessor(RelevantGenericItems.UponAWaveOfSummerLow, RelevantGenericItems.UponAWaveOfSummerHigh, TargetedDamageItem);
            RegisterItemProcessor(RelevantGenericItems.BlessedWithThunderLow, RelevantGenericItems.BlessedWithThunderHigh, TargetedDamageItem);

            RegisterItemProcessor(new int[] { RelevantGenericItems.RezCan1, RelevantGenericItems.RezCan2 }, RezCan);
            RegisterItemProcessor(new int[] { RelevantGenericItems.ExpCan1, RelevantGenericItems.ExpCan2 }, ExpCan);
            RegisterItemProcessor(new int[] { RelevantGenericItems.InsuranceCan1, RelevantGenericItems.InsuranceCan2 }, InsuranceCan);

            RegisterItemProcessor(new int[] { RelevantGenericItems.HealthAndNanoStim1, RelevantGenericItems.HealthAndNanoStim200, 
            RelevantGenericItems.HealthAndNanoStim400, }, HealthAndNanoStim, CombatActionPriority.High);

            RegisterItemProcessor(new int[] { RelevantGenericItems.PremSitKit, RelevantGenericItems.AreteSitKit, RelevantGenericItems.SitKit1,
            RelevantGenericItems.SitKit100, RelevantGenericItems.SitKit200, RelevantGenericItems.SitKit300, RelevantGenericItems.SitKit400 }, SitKit);

            RegisterItemProcessor(new int[] { RelevantGenericItems.DaTaunterLow, RelevantGenericItems.DaTaunterHigh }, TargetedDamageItem);

            RegisterItemProcessor(new int[] { RelevantGenericItems.FreeStim1, RelevantGenericItems.FreeStim50, RelevantGenericItems.FreeStim100,
            RelevantGenericItems.FreeStim200, RelevantGenericItems.FreeStim300 }, FreeStim);


            RegisterItemProcessor(RelevantGenericItems.AmmoBoxArrows, RelevantGenericItems.AmmoBoxArrows, AmmoBoxArrows);
            RegisterItemProcessor(RelevantGenericItems.AmmoBoxBullets, RelevantGenericItems.AmmoBoxBullets, AmmoBoxBullets);
            RegisterItemProcessor(RelevantGenericItems.AmmoBoxEnergy, RelevantGenericItems.AmmoBoxEnergy, AmmoBoxEnergy);
            RegisterItemProcessor(RelevantGenericItems.AmmoBoxShotgun, RelevantGenericItems.AmmoBoxShotgun, AmmoBoxShotgun);
            RegisterItemProcessor(RelevantGenericItems.AmmoBoxGrenade, RelevantGenericItems.AmmoBoxGrenade, AmmoBoxGrenade);

            RegisterSpellProcessor(RelevantGenericNanos.CompositeNano, CompositeBuff);
            RegisterSpellProcessor(RelevantGenericNanos.CompositeAttribute, CompositeBuff);
            RegisterSpellProcessor(RelevantGenericNanos.CompositeUtility, CompositeBuff);
            RegisterSpellProcessor(RelevantGenericNanos.CompositeMartialProwess, CompositeBuff);

            RegisterSpellProcessor(RelevantGenericNanos.InsightIntoSL, CompositeBuff);

            if (GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Melee))
            {
                //We are melee
                RegisterSpellProcessor(RelevantGenericNanos.CompositeMartial, CompositeBuffExcludeInnerSanctum);
                RegisterSpellProcessor(RelevantGenericNanos.CompositeMelee, CompositeBuff);
                RegisterSpellProcessor(RelevantGenericNanos.CompositePhysicalSpecial, CompositeBuff);
            }


            if (GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(CharacterWieldedWeapon.Ranged))
            {
                //We are ranged
                RegisterSpellProcessor(RelevantGenericNanos.CompositeRanged, CompositeBuff);
                RegisterSpellProcessor(RelevantGenericNanos.CompositeRangedSpecial, CompositeBuff);
            }

            Game.TeleportEnded += OnZoned;
            Game.TeleportEnded += TeleportEnded;
            Team.TeamRequest += Team_TeamRequest;
            Config.CharSettings[Game.ClientInst].IPCChannelChangedEvent += IPCChannel_Changed;
            //Network.N3MessageSent += Network_N3MessageSent;

            Chat.RegisterCommand("reform", ReformCommand);
            Chat.RegisterCommand("form", FormCommand);
            Chat.RegisterCommand("convert", RaidCommand);
            Chat.RegisterCommand("disband", DisbandCommand);
        }

        public static Window[] _window => new Window[] { _perkWindow };

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            //Chat.WriteLine($"{SettingsController.GetRegisteredCharacters().Length}");

            //Chat.WriteLine($"{Config.CharSettings[Game.ClientInst].IPCChannel}");

            //if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.F1) && !_init
            //    && IsActiveWindow)
            //{
            //    _init = true;

            //    Config = Config.Load($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Generic\\{Game.ClientInst}\\Config.json");

            //    SettingsController.settingsWindow = Window.Create(new Rect(50, 50, 300, 300), "CombatHandler", "Settings", WindowStyle.Default, WindowFlags.AutoScale);

            //    if (SettingsController.settingsWindow != null && !SettingsController.settingsWindow.IsVisible)
            //    {
            //        foreach (string settingsName in SettingsController.settingsWindows.Keys.Where(x => x.Contains("Handler")))
            //        {
            //            SettingsController.AppendSettingsTab(settingsName, SettingsController.settingsWindow);

            //            SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);
            //            SettingsController.settingsWindow.FindView("EngiBioCocoonPercentageBox", out TextInputView engiBioCocoonInput);

            //            if (channelInput != null)
            //                channelInput.Text = $"{Config.CharSettings[Game.ClientInst].IPCChannel}";
            //            if (engiBioCocoonInput != null)
            //                engiBioCocoonInput.Text = $"{Config.CharSettings[Game.ClientInst].EngiBioCocoonPercentage}";
            //        }
            //    }

            //    _init = false;
            //}

            if (Time.NormalTime > _updateTick + 0.1f)
            {
                foreach (SimpleChar player in DynelManager.Characters
                    .Where(c => c.IsPlayer && DynelManager.LocalPlayer.DistanceFrom(c) < 40f))
                {
                    Network.Send(new CharacterActionMessage()
                    {
                        Action = CharacterActionType.InfoRequest,
                        Target = player.Identity
                    });
                }

                _updateTick = Time.NormalTime;
            }

            if (SettingsController.settingsWindow != null && SettingsController.settingsWindow.IsValid)
            {
                SettingsController.CleanUp();

                SettingsController.settingsWindow.FindView("ChannelBox", out TextInputView channelInput);

                if (channelInput != null && !string.IsNullOrEmpty(channelInput.Text))
                    if (int.TryParse(channelInput.Text, out int channelValue)
                        && Config.CharSettings[Game.ClientInst].IPCChannel != channelValue)
                        Config.CharSettings[Game.ClientInst].IPCChannel = channelValue;
            }

            if (DynelManager.LocalPlayer.IsAttacking || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) > 0)
            {
                _lastCombatTime = Time.NormalTime;
            }
        }


        #region Perks

        public static bool SelfHeal(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            //if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            //if (DynelManager.LocalPlayer.HealthPercent <= SelfHealPerkPercentage)
            //    return GenericPerk(perk, DynelManager.LocalPlayer, ref actionTarget);

            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.HealthPercent <= SelfHealPerkPercentage)
                return CombatBuffPerk(perk, fightingTarget, ref actionTarget);

            return false;
        }

        public static bool SelfNano(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            //if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            //if (DynelManager.LocalPlayer.NanoPercent <= SelfNanoPerkPercentage)
            //    return GenericPerk(perk, DynelManager.LocalPlayer, ref actionTarget);

            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.NanoPercent <= SelfNanoPerkPercentage)
                return CombatBuffPerk(perk, fightingTarget, ref actionTarget);

            return false;
        }

        public static bool TeamHeal(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            //if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            if (Team.IsInTeam)
            {
                SimpleChar _person = DynelManager.Players
                    .Where(c => c.Health > 0
                        && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent <= TeamHealPerkPercentage)
                    .FirstOrDefault();

                if (_person != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = _person;
                    return true;
                }
            }

            if (DynelManager.LocalPlayer.HealthPercent <= TeamHealPerkPercentage)
                return CombatBuffPerk(perk, fightingTarget, ref actionTarget);

            return false;
        }

        public static bool TeamNano(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            //if (DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            if (Team.IsInTeam)
            {
                SimpleChar _person = DynelManager.Players
                    .Where(c => c.Health > 0
                        && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.NanoPercent <= TeamNanoPerkPercentage)
                    .FirstOrDefault();

                if (_person != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = _person;
                    return true;
                }
            }

            if (DynelManager.LocalPlayer.NanoPercent <= TeamNanoPerkPercentage)
                return CombatBuffPerk(perk, fightingTarget, ref actionTarget);

            return false;
        }

        protected bool LegShot(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || fightingTarget == null) { return false; }

            if (!IsSettingEnabled("LegShot")) { return false; }

            if (fightingTarget?.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower()) && c.RemainingTime > 3).Any() == true) { return false; }

            return DamagePerk(perk, fightingTarget, ref actionTarget);
        }

        protected bool ReinforceSlugs(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || fightingTarget == null) { return false; }

            //if (fightingTarget?.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower()) && c.RemainingTime > 3).Any() == true) { return false; }

            return DamagePerk(perk, fightingTarget, ref actionTarget);
        }

        protected bool Energize(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || fightingTarget == null) { return false; }

            if (fightingTarget?.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower()) && c.RemainingTime > 3).Any() == true) { return false; }

            return DamagePerk(perk, fightingTarget, ref actionTarget);
        }



        protected bool BioCocoon(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || !InCombat()
                || DynelManager.LocalPlayer.HealthPercent > BioCocoonPercentage
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon)) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower())).Any()) { return false; }

            return BuffPerk(perk, fightingTarget, ref actionTarget);
        }

        protected bool BattleGroupHeal1(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || !InCombat()) { return false; }

            if (Team.IsInTeam)
            {
                List<SimpleChar> dyingTeamMember = DynelManager.Players
                    .Where(c => c.Health > 0 && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent <= BattleGroupHeal1Percentage)
                    .ToList();

                if (dyingTeamMember.Count >= 1)
                    return BattleGroupHealPerk1(perk);
            }

            if (DynelManager.LocalPlayer.HealthPercent > BattleGroupHeal1Percentage) { return false; }

            return BattleGroupHealPerk1(perk);
        }

        protected bool BattleGroupHeal2(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || !InCombat()) { return false; }

            if (Team.IsInTeam)
            {
                List<SimpleChar> dyingTeamMember = DynelManager.Players
                    .Where(c => c.Health > 0 && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent <= BattleGroupHeal2Percentage)
                    .ToList();

                if (dyingTeamMember.Count >= 1)
                {
                    return BattleGroupHealPerk2(perk);
                }
            }

            if (DynelManager.LocalPlayer.HealthPercent > BattleGroupHeal2Percentage) { return false; }

            return BattleGroupHealPerk2(perk);
        }

        protected bool BattleGroupHeal3(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || !InCombat()) { return false; }

            if (Team.IsInTeam)
            {
                List<SimpleChar> dyingTeamMember = DynelManager.Players
                    .Where(c => c.Health > 0 && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent <= BattleGroupHeal3Percentage)
                    .ToList();

                if (dyingTeamMember.Count >= 1)
                {
                    return BattleGroupHealPerk3(perk);
                }
            }

            if (DynelManager.LocalPlayer.HealthPercent > BattleGroupHeal3Percentage) { return false; }

            return BattleGroupHealPerk3(perk);
        }

        protected bool BattleGroupHeal4(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || !InCombat()) { return false; }

            if (Team.IsInTeam)
            {
                List<SimpleChar> dyingTeamMember = DynelManager.Players
                    .Where(c => c.Health > 0 && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent <= BattleGroupHeal4Percentage)
                    .ToList();

                if (dyingTeamMember.Count >= 1)
                {
                    return BattleGroupHealPerk4(perk);
                }
            }

            if (DynelManager.LocalPlayer.HealthPercent > BattleGroupHeal4Percentage) { return false; }

            return BattleGroupHealPerk4(perk);
        }


        protected bool Leadership(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable) { return false; }

            if (Time.NormalTime > CycleXpPerks + CycleXpPerksDelay)
            {
                CycleXpPerks = Time.NormalTime;

                if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.ShortTermXPGain)) { return false; }

                if (DynelManager.NPCs.Any(c => c.FightingTarget != null && AttackingTeam(c)))
                    return LeadershipPerk(perk);
            }

            return false;
        }

        protected bool Governance(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.ShortTermXPGain)) { return false; }

            if (DynelManager.NPCs.Any(c => c.FightingTarget != null && AttackingTeam(c)))
                return GovernancePerk(perk);

            return false;
        }
        protected bool TheDirector(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.ShortTermXPGain)) { return false; }

            if (DynelManager.NPCs.Any(c => c.FightingTarget != null && AttackingTeam(c)))
                return TheDirectorPerk(perk);

            return false;
        }

        protected bool Volunteer(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower())).Any()) { return false; }

            return VolunteerPerk(perk, ref actionTarget);
        }

        protected bool CyclePerks(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || fightingTarget == null) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower())).Any()) { return false; }

            return BuffPerk(perk, fightingTarget, ref actionTarget);
        }

        protected bool LEProc(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower())).Any()) { return false; }

            return BuffPerk(perk, fightingTarget, ref actionTarget);
        }

        protected bool Limber(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Find(RelevantGenericNanos.DanceOfFools, out Buff dof) && dof.RemainingTime > 12.5f) { return false; }

            return CombatBuffPerk(perk, fightingTarget, ref actionTarget);
        }

        protected bool DanceOfFools(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Find(RelevantGenericNanos.DanceOfFools, out Buff dof) && dof.RemainingTime > 12.5f) { return false; }

            return CombatBuffPerk(perk, fightingTarget, ref actionTarget);
        }
        protected bool EvasiveStance(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable) { return false; }

            if (DynelManager.LocalPlayer.HealthPercent >= 75) { return false; }

            return CombatBuffPerk(perk, fightingTarget, ref actionTarget);
        }

        protected bool Moonmist(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || fightingTarget == null) { return false; }

            if (fightingTarget.HealthPercent < 90 && DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) < 2) { return false; }

            return CombatBuffPerk(perk, fightingTarget, ref actionTarget);
        }
        protected bool Starfall(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || fightingTarget == null) { return false; }

            return StarFallPerk(perk, fightingTarget, ref actionTarget);
        }
        protected bool QuickShot(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!perk.IsAvailable || fightingTarget == null) { return false; }

            return QuickShotPerk(perk, fightingTarget, ref actionTarget);
        }

        protected bool WitOfTheAtrox(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Time.NormalTime > CycleWitOfTheAtroxPerk + CycleWitOfTheAtroxPerkDelay)
            {
                CycleWitOfTheAtroxPerk = Time.NormalTime;

                if (DynelManager.LocalPlayer.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower())).Any()) { return false; }

                return CombatBuffPerk(perk, fightingTarget, ref actionTarget);
            }

            return false;
        }

        protected bool BioRegrowth(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Time.NormalTime > CycleBioRegrowthPerk + CycleBioRegrowthPerkDelay)
            {
                CycleBioRegrowthPerk = Time.NormalTime;

                if (!InCombat()) { return false; }

                SimpleChar dyingTeamMember = DynelManager.Players
                    .Where(c => c.Health > 0 && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent <= BioRegrowthPercentage)
                    .OrderBy(c => c.HealthPercent)
                    .OrderBy(c => c.Profession == Profession.Enforcer)
                    .OrderBy(c => c.Profession == Profession.Doctor)
                    .OrderBy(c => c.Profession == Profession.Soldier)
                    .FirstOrDefault();

                if (DynelManager.LocalPlayer.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower())).Any()
                    || dyingTeamMember == null) { return false; }

                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = dyingTeamMember;
                return true;
            }

            return false;
        }

        protected bool Sphere(PerkAction perk, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Time.NormalTime > CycleSpherePerk + CycleSpherePerkDelay)
            {
                CycleSpherePerk = Time.NormalTime;

                if (DynelManager.LocalPlayer.Buffs.Where(c => c.Name.ToLower().Contains(perk.Name.ToLower())).Any()) { return false; }

                return CombatBuffPerk(perk, fightingTarget, ref actionTarget);
            }

            return false;
        }

        #endregion

        #region Logic

        protected bool Pistol(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Team.IsInTeam)
                return TeamBuffExclusionWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Pistol);

            return BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Pistol);
        }

        //TODO: Add UI
        private bool FountainOfLife(Spell spell, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing")) { return false; }

            if (Team.IsInTeam)
            {
                SimpleChar teamMember = DynelManager.Players
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent <= 30 && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0)
                    .OrderBy(c => c.HealthPercent)
                    .OrderBy(c => c.Profession == Profession.Enforcer)
                    .OrderBy(c => c.Profession == Profession.Doctor)
                    .OrderBy(c => c.Profession == Profession.Soldier)
                    .FirstOrDefault();

                if (teamMember != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = teamMember;
                    return true;
                }

                return false;
            }


            if (DynelManager.LocalPlayer.HealthPercent <= 30)
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        #endregion

        #region Extensions

        #region Comps
        protected bool CompositeBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Composites") || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            if (SpellChecksPlayer(spell, spell.Nanoline))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        protected bool CompositeBuffExcludeInnerSanctum(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Composites") || IsInsideInnerSanctum()) { return false; }

            return GenericBuff(spell, ref actionTarget); ;
        }
        #endregion

        #region Buffs

        #region Combat

        protected bool GenericCombatTeamBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Team.IsInTeam)
                return CombatTeamBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);

            return CombatBuff(spell, spell.Nanoline, fightingTarget, ref actionTarget);
        }

        protected bool CombatBuff(Spell spell, NanoLine nanoline, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            if (SpellChecksPlayer(spell, nanoline))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        protected bool CombatTeamBuff(Spell spell, NanoLine nanoline, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            SimpleChar target = DynelManager.Players
                .Where(c => c.IsInLineOfSight
                        && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0
                        && SpellChecksOther(spell, spell.Nanoline, c))
                .FirstOrDefault();

            if (target != null)
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = target;
                return true;
            }

            return false;
        }
        #endregion

        #region Non Combat

        protected bool GlobalGenericBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return GenericBuff(spell, ref actionTarget);
        }

        protected bool GlobalGenericTeamBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return GenericTeamBuff(spell, ref actionTarget);
        }

        protected bool GenericBuff(Spell spell, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.FightingTarget != null || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            return Buff(spell, spell.Nanoline, ref actionTarget);
        }

        protected bool GenericTeamBuff(Spell spell, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            if (Team.IsInTeam)
                return TeamBuff(spell, spell.Nanoline, ref actionTarget);

            return Buff(spell, spell.Nanoline, ref actionTarget);
        }

        protected bool Buff(Spell spell, NanoLine nanoline, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.FightingTarget != null || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            if (SpellChecksPlayer(spell, nanoline))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        protected bool TeamBuff(Spell spell, NanoLine nanoline, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.FightingTarget != null || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            SimpleChar target = DynelManager.Players
                    .Where(c => c.IsInLineOfSight
                        && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0
                        && SpellChecksOther(spell, spell.Nanoline, c))
                    .FirstOrDefault();

            if (target != null)
            {
                if (spell.Nanoline == NanoLine.CriticalIncreaseBuff && target.Buffs.Any(c => RelevantGenericNanos.AAOTransfer.Contains(c.Id))) { return false; }

                if (spell.Nanoline == NanoLine.RunspeedBuffs && target.Buffs.Contains(NanoLine.MajorEvasionBuffs)) { return false; }

                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = target;
                return true;
            }

            return false;
        }

        protected bool GenericTeamBuffExclusion(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsInsideInnerSanctum() || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.MajorEvasionBuffs)) { return false; }

            if (Team.IsInTeam)
                return TeamBuff(spell, spell.Nanoline, ref actionTarget);

            return Buff(spell, spell.Nanoline, ref actionTarget);
        }

        protected bool BuffExclusion(Spell spell, NanoLine nanoline, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (IsInsideInnerSanctum() || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.MajorEvasionBuffs)) { return false; }

            if (DynelManager.LocalPlayer.FightingTarget != null || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            if (SpellChecksPlayer(spell, nanoline))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        #endregion

        #endregion

        #region Debuffs

        protected bool TargetDebuff(Spell spell, NanoLine nanoline, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null) { return false; }

            if (SpellChecksOther(spell, nanoline, fightingTarget))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = fightingTarget;
                return true;
            }

            return false;
        }
        protected bool ToggledTargetDebuff(string settingName, Spell spell, NanoLine nanoline, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || !IsSettingEnabled(settingName)) { return false; }

            if (SpellChecksOther(spell, nanoline, fightingTarget))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = fightingTarget;
                return true;
            }

            return false;
        }

        protected bool AreaDebuff(Spell spell, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanCast(spell) || !IsSettingEnabled("Buffing")) { return false; }

            SimpleChar target = DynelManager.NPCs
                    .Where(c => !debuffAreaTargetsToIgnore.Contains(c.Name)
                        && c.FightingTarget != null
                        && c.Health > 0
                        && !c.Buffs.Contains(301844)
                        && c.IsInLineOfSight
                        && !c.Buffs.Contains(NanoLine.Mezz) && !c.Buffs.Contains(NanoLine.AOEMezz)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && SpellChecksOther(spell, spell.Nanoline, c))
                    .OrderBy(c => c.DistanceFrom(DynelManager.LocalPlayer))
                    .FirstOrDefault();

            if (target != null)
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = target;
                return true;
            }

            return false;
        }

        protected bool ToggledOSDebuff(string toggleName, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled(toggleName) || !CanCast(spell) || !IsSettingEnabled("Buffing")) { return false; }

            SimpleChar target = DynelManager.NPCs
                    .Where(c => !debuffAreaTargetsToIgnore.Contains(c.Name)
                        && c.FightingTarget != null
                        && c.Health > 0
                        && !c.Buffs.Contains(301844)
                        && c.IsInLineOfSight
                        && !c.Buffs.Contains(NanoLine.Mezz) && !c.Buffs.Contains(NanoLine.AOEMezz)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && SpellChecksOther(spell, spell.Nanoline, c))
                    .OrderBy(c => c.DistanceFrom(DynelManager.LocalPlayer))
                    .FirstOrDefault();

            if (target != null)
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = target;
                return true;
            }

            return false;
        }

        #endregion

        #region NanoSkills Buff

        protected bool GenericNanoSkillsBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            if (Team.IsInTeam)
                return NanoSkillsTeamBuff(spell, fightingTarget, ref actionTarget);

            return NanoSkillsBuff(spell, fightingTarget, ref actionTarget);
        }

        protected bool NanoSkillsBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            if (SpellChecksNanoSkillsPlayer(spell, fightingTarget))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        protected bool NanoSkillsTeamBuff(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget != null || RelevantGenericNanos.IgnoreNanos.Contains(spell.Id)) { return false; }

            if (Team.IsInTeam)
            {
                SimpleChar target = DynelManager.Players
                    .Where(c => c.IsInLineOfSight
                        && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0
                        && SpellChecksNanoSkillsOther(spell, c))
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = target;
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region Weapon Type

        protected bool Ranged(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget) => BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Ranged);

        protected bool Melee(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget) => BuffWeaponType(spell, fightingTarget, ref actionTarget, CharacterWieldedWeapon.Melee);

        protected bool BuffWeaponType(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget, CharacterWieldedWeapon supportedWeaponType)
        {
            if (fightingTarget != null) { return false; }

            if (SpellChecksPlayer(spell, spell.Nanoline) && GetWieldedWeapons(DynelManager.LocalPlayer).HasFlag(supportedWeaponType))
            {
                actionTarget.Target = DynelManager.LocalPlayer;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            return false;
        }

        protected bool TeamBuffWeaponType(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget, CharacterWieldedWeapon supportedWeaponType)
        {
            if (Team.IsInTeam)
            {
                SimpleChar target = DynelManager.Players
                    .Where(c => c.IsInLineOfSight
                        && Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0
                        && SpellChecksOther(spell, spell.Nanoline, c)
                        && GetWieldedWeapons(c).HasFlag(supportedWeaponType))
                    .FirstOrDefault();

                if (target != null)
                {
                    if (target.Buffs.Contains(NanoLine.FixerSuppressorBuff) &&
                        (spell.Nanoline == NanoLine.FixerSuppressorBuff || spell.Nanoline == NanoLine.AssaultRifleBuffs)) { return false; }

                    if (target.Buffs.Contains(NanoLine.PistolBuff) &&
                        spell.Nanoline == NanoLine.PistolBuff) { return false; }

                    if (target.Buffs.Contains(NanoLine.AssaultRifleBuffs) &&
                        (spell.Nanoline == NanoLine.AssaultRifleBuffs || spell.Nanoline == NanoLine.GrenadeBuffs)) { return false; }

                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = target;
                    return true;
                }
            }

            return false;
        }

        protected bool TeamBuffExclusionWeaponType(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget, CharacterWieldedWeapon supportedWeaponType)
        {
            if (Team.IsInTeam)
            {
                SimpleChar target = DynelManager.Players
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.Profession != Profession.NanoTechnician
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0
                        && SpellChecksOther(spell, spell.Nanoline, c)
                        && GetWieldedWeapons(c).HasFlag(supportedWeaponType))
                    .FirstOrDefault();

                if (target != null)
                {
                    if (target.Buffs.Contains(NanoLine.FixerSuppressorBuff) &&
                        (spell.Nanoline == NanoLine.FixerSuppressorBuff || spell.Nanoline == NanoLine.AssaultRifleBuffs)) { return false; }

                    if (target.Buffs.Contains(NanoLine.PistolBuff) &&
                        spell.Nanoline == NanoLine.PistolBuff) { return false; }

                    if (target.Buffs.Contains(NanoLine.AssaultRifleBuffs) &&
                        (spell.Nanoline == NanoLine.AssaultRifleBuffs || spell.Nanoline == NanoLine.GrenadeBuffs)) { return false; }

                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = target;
                    return true;
                }
            }

            return false;
        }

        #endregion

        #endregion

        #region Items

        protected virtual bool SharpObjects(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("SharpObjects") || fightingTarget == null) { return false; }

            return TargetedDamageItem(item, fightingTarget, ref actionTarget);
        }
        protected virtual bool Grenades(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Grenades") || fightingTarget == null) { return false; }

            return TargetedDamageItem(item, fightingTarget, ref actionTarget);
        }

        protected virtual bool DamageItem(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            return !DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item)) && fightingTarget != null && fightingTarget.IsInAttackRange();
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

        private bool RezCan(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid)) { return false; }

            actiontarget.ShouldSetTarget = false;
            return true;
        }

        private bool FreeStim(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid) 
                || (!DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root) && !DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Snare)
                && !DynelManager.LocalPlayer.Buffs.Contains(258231))) { return false; }

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid)) { return false; }

            actiontarget.ShouldSetTarget = true;
            return true;

        }

        protected bool TauntTool(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (fightingTarget == null || !IsSettingEnabled("ScorpioTauntTool")
                || DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Psychology)) { return false; }

            actionTarget.Target = fightingTarget;
            actionTarget.ShouldSetTarget = true;
            return true;
        }

        private bool InsuranceCan(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid)
                || DynelManager.LocalPlayer.GetStat(Stat.UnsavedXP) == 0
                || DynelManager.LocalPlayer.Buffs.Contains(300727)) { return false; }

            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = false;
            return true;

        }

        private bool ExpCan(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid)) { return false; }

            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = false;
            return true;

        }

        private bool FlowerOfLife(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (fightingtarget == null || DynelManager.LocalPlayer.Cooldowns.ContainsKey(GetSkillLockStat(item))) { return false; }

            int approximateHealing = item.QualityLevel * 10;

            return DynelManager.LocalPlayer.MissingHealth > approximateHealing;
        }

        private bool SitKit(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Kits")) { return false; }

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Treatment)
                || (DynelManager.LocalPlayer.HealthPercent >= KitHealthPercentage && DynelManager.LocalPlayer.NanoPercent >= KitNanoPercentage)) { return false; }

            actionTarget.Target = DynelManager.LocalPlayer;
            actionTarget.ShouldSetTarget = true;
            return true;
        }

        private bool HealthAndNanoStim(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (StimTargetSelection.Target == (StimTargetSelection)_settings["StimTargetSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid)
                    || DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) >= 1
                    || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root) || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Snare)
                    || DynelManager.LocalPlayer.Buffs.Contains(280470) || DynelManager.LocalPlayer.Buffs.Contains(258231)) { return false; }

                SimpleChar player = DynelManager.Players
                    .Where(c => c.IsInLineOfSight
                        && (c.HealthPercent <= StimHealthPercentage || c.NanoPercent <= StimNanoPercentage)
                        && c.Name == StimTargetName
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 10f
                        && c.Health > 0)
                    .FirstOrDefault();

                if (player != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = player;
                    return true;
                }
            }

            if (StimTargetSelection.Team == (StimTargetSelection)_settings["StimTargetSelection"].AsInt32())
            {
                if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid)
                    || DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) >= 1
                    || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root) || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Snare)
                    || DynelManager.LocalPlayer.Buffs.Contains(280470) || DynelManager.LocalPlayer.Buffs.Contains(258231)) { return false; }

                SimpleChar teamMember = DynelManager.Players
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && (c.HealthPercent <= StimHealthPercentage || c.NanoPercent <= StimNanoPercentage)
                        && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 10f
                        && c.Health > 0)
                    .OrderBy(c => c.HealthPercent)
                    .OrderBy(c => c.Profession == Profession.Enforcer)
                    .OrderBy(c => c.Profession == Profession.Doctor)
                    .OrderBy(c => c.Profession == Profession.Soldier)
                    .FirstOrDefault();

                if (teamMember != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = teamMember;

                    return true;
                }

                return false;
            }

            if (StimTargetSelection.None == (StimTargetSelection)_settings["StimTargetSelection"].AsInt32()) { return false; }

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.FirstAid)
                || DynelManager.LocalPlayer.GetStat(Stat.TemporarySkillReduction) >= 1
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Root) || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.Snare)
                || DynelManager.LocalPlayer.Buffs.Contains(280470) || DynelManager.LocalPlayer.Buffs.Contains(258231)) { return false; }

            int targetHealing = item.UseModifiers
                    .Where(x => x is SpellData.Healing hx && hx.ApplyOn == SpellModifierTarget.Target)
                    .Cast<SpellData.Healing>()
                    .Sum(x => x.Average);

            if (DynelManager.LocalPlayer.Buffs.FirstOrDefault(c => c.Id == 275130 && c.RemainingTime >= 595f) == null
                && (DynelManager.LocalPlayer.MissingHealth >= targetHealing || DynelManager.LocalPlayer.MissingNano >= targetHealing))
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;

                return true;
            }

            return false;
        }

        private bool AmmoBoxBullets(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing)) { return false; }

            return !Inventory.Items
                .Where(c => c.Name == "Ammo: Box of Bullets")
                .Any();
        }

        private bool AmmoBoxEnergy(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing)) { return false; }

            return !Inventory.Items
                .Where(c => c.Name == "Ammo: Box of Energy Weapon Ammo")
                .Any();
        }

        private bool AmmoBoxShotgun(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing)) { return false; }

            return !Inventory.Items
                .Where(c => c.Name == "Ammo: Box of Shotgun Shells")
                .Any();
        }
        private bool AmmoBoxGrenade(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing)) { return false; }

            return !Inventory.Items
                .Where(c => c.Name == "Ammo: Box of Launcher Grenades")
                .Any();
        }

        private bool AmmoBoxArrows(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.WeaponSmithing)) { return false; }

            return !Inventory.Items
                .Where(c => c.Name == "Ammo: Box of Arrows")
                .Any();
        }

        private bool EnduranceBooster(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            //if (Inventory.Find(305476, 305476, out Item absorbdesflesh))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BodyDevelopment)) { return false; }
            //}
            //if (Inventory.Find(204698, 204698, out Item absorbwithflesh))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BodyDevelopment)) { return false; }
            //}
            //if (Inventory.Find(156576, 156576, out Item absorbassaultclass))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.DuckExp)) { return false; }
            //}

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength) 
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon)
                || Item.HasPendingUse
                || DynelManager.LocalPlayer.HealthPercent > StrengthAbsorbsItemPercentage
                || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            return item != null;
        }

        private bool AssaultClass(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.DuckExp) 
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon)
                || Item.HasPendingUse
                || DynelManager.LocalPlayer.HealthPercent > DuckAbsorbsItemPercentage
                || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            return item != null;
        }

        private bool DescFlesh(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            //if (Inventory.Find(267168, 267168, out Item enduranceabsorbenf))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength)) { return false; }
            //}
            //if (Inventory.Find(267167, 267167, out Item enduranceabsorbnanomage))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength)) { return false; }
            //}
            //if (Inventory.Find(156576, 156576, out Item absorbassaultclass))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.DuckExp)) { return false; }
            //}

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BodyDevelopment) 
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon)
                || Item.HasPendingUse
                || DynelManager.LocalPlayer.HealthPercent > BodyDevAbsorbsItemPercentage
                || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            return item != null;
        }

        private bool WithFlesh(Item item, SimpleChar fightingtarget, ref (SimpleChar Target, bool ShouldSetTarget) actiontarget)
        {
            //Inventory.Find(305476, 305476, out Item absorbdesflesh);

            //if (Inventory.Find(267168, 267168, out Item enduranceabsorbenf))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength)) { return false; }
            //}
            //if (Inventory.Find(267167, 267167, out Item enduranceabsorbnanomage))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.Strength)) { return false; }
            //}
            //if (Inventory.Find(156576, 156576, out Item absorbassaultclass))
            //{
            //    if (!DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.DuckExp)) { return false; }
            //}

            if (DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.BodyDevelopment) 
                || DynelManager.LocalPlayer.Buffs.Contains(NanoLine.BioCocoon)
                || Item.HasPendingUse
                || DynelManager.LocalPlayer.HealthPercent > BodyDevAbsorbsItemPercentage
                || DynelManager.LocalPlayer.GetStat(Stat.NumFightingOpponents) == 0) { return false; }

            return item != null;
        }

        protected virtual bool Coffee(Item item, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (DynelManager.LocalPlayer.Buffs.Contains(NanoLine.FoodandDrinkBuffs)) { return false; }

            return DamageItem(item, fightingTarget, ref actionTarget);
        }

        #endregion

        #region Pets

        protected bool NoShellPetSpawner(PetType petType, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanSpawnPets(petType)) { return false; }

            actionTarget.ShouldSetTarget = false;
            return true;
        }

        protected bool PetSpawner(Dictionary<int, PetSpellData> petData, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (Game.IsZoning) { return false; }

            if (DynelManager.LocalPlayer.Pets.Where(c => c.Type == petData[spell.Id].PetType || c.Type == PetType.Unknown).Count() >= 1) return false;

            if (!petData.ContainsKey(spell.Id)) { return false; }

            if (Inventory.Find(petData[spell.Id].ShellId, out Item shell)) 
            {
                if (!CanSpawnPets(petData[spell.Id].PetType)) { return false; }

                shell.Use();
            }

            return NoShellPetSpawner(petData[spell.Id].PetType, spell, fightingTarget, ref actionTarget);
        }

        protected bool CanSpawnPets(PetType petType)
        {
            if (!IsSettingEnabled("SpawnPets") || !CanLookupPetsAfterZone() || PetAlreadySpawned(petType)) { return false; }

            return true;
        }

        private bool PetAlreadySpawned(PetType petType)
        {
            return DynelManager.LocalPlayer.Pets.Any(c => (c.Type == PetType.Unknown || c.Type == petType));
        }

        protected bool CanLookupPetsAfterZone()
        {
            return Time.NormalTime > _lastZonedTime + PostZonePetCheckBuffer;
        }

        public bool PetCleanse(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanLookupPetsAfterZone()) { return false; }

            return DynelManager.LocalPlayer.Pets
                .Where(c => c.Character == null || c.Character.Buffs.Contains(NanoLine.Root) || c.Character.Buffs.Contains(NanoLine.Snare)
                    || c.Character.Buffs.Contains(NanoLine.Mezz)).Any();
        }

        protected bool PetTargetBuff(NanoLine buffNanoLine, PetType petType, Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("BuffPets") || !CanLookupPetsAfterZone()) { return false; }

            Pet target = DynelManager.LocalPlayer.Pets
                    .Where(c => c.Type == petType
                        && !c.Character.Buffs.Contains(buffNanoLine))
                    .FirstOrDefault();

            if (target != null && target.Character != null)
            {
                actionTarget.Target = target.Character;
                actionTarget.ShouldSetTarget = true;
                return true;
            }

            return false;
        }

        protected void SynchronizePetCombatStateWithOwner()
        {
            if (CanLookupPetsAfterZone() && Time.NormalTime - _lastPetSyncTime > 1)
            {
                foreach (Pet _pet in DynelManager.LocalPlayer.Pets.Where(c => c.Type == PetType.Attack || c.Type == PetType.Support))
                    SynchronizePetCombatState(_pet);

                _lastPetSyncTime = Time.NormalTime;
            }
        }

        private void SynchronizePetCombatState(Pet pet)
        {
            if (!DynelManager.LocalPlayer.IsAttacking && pet?.Character.IsAttacking == true)
                pet?.Follow();

            if (DynelManager.LocalPlayer.IsAttacking && DynelManager.LocalPlayer.FightingTarget != null)
            {
                if (pet?.Character.IsAttacking == false)
                    pet?.Attack(DynelManager.LocalPlayer.FightingTarget.Identity);

                if (pet?.Character.IsAttacking == true && pet?.Character.FightingTarget != null
                    && pet?.Character.FightingTarget.Identity != DynelManager.LocalPlayer.FightingTarget.Identity)
                        pet?.Attack(DynelManager.LocalPlayer.FightingTarget.Identity);
            }
        }

        #endregion

        #region Checks

        protected bool CheckNotProfsBeforeCast(Spell spell, SimpleChar fightingTarget, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!IsSettingEnabled("Buffing") || !CanCast(spell)) { return false; }

            if (Team.IsInTeam)
            {
                SimpleChar target = DynelManager.Players
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.Profession != Profession.Keeper && c.Profession != Profession.Engineer
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0
                        && SpellChecksOther(spell, spell.Nanoline, c))
                    .FirstOrDefault();

                if (target != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = target;
                    return true;
                }
            }

            return Buff(spell, spell.Nanoline, ref actionTarget); ;
        }

        protected bool FindMemberWithHealthBelow(int healthPercentThreshold, Spell spell, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanCast(spell)) { return false; }

            if (Team.IsInTeam)
            {
                SimpleChar teamMember = DynelManager.Players
                    .Where(c => Team.Members.Select(t => t.Identity.Instance).Contains(c.Identity.Instance)
                        && c.HealthPercent <= healthPercentThreshold && c.IsInLineOfSight
                        && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                        && c.Health > 0)
                    .OrderBy(c => c.HealthPercent)
                    .OrderBy(c => c.Profession == Profession.Enforcer)
                    .OrderBy(c => c.Profession == Profession.Doctor)
                    .OrderBy(c => c.Profession == Profession.Soldier)
                    .FirstOrDefault();

                if (teamMember != null)
                {
                    actionTarget.ShouldSetTarget = true;
                    actionTarget.Target = teamMember;
                    return true;
                }

                return false;
            }


            if (DynelManager.LocalPlayer.HealthPercent <= healthPercentThreshold)
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = DynelManager.LocalPlayer;
                return true;
            }

            return false;
        }

        protected bool FindPlayerWithHealthBelow(int healthPercentThreshold, Spell spell, ref (SimpleChar Target, bool ShouldSetTarget) actionTarget)
        {
            if (!CanCast(spell)) { return false; }

            SimpleChar player = DynelManager.Players
                .Where(c => c.HealthPercent <= healthPercentThreshold
                    && c.IsInLineOfSight
                    && c.DistanceFrom(DynelManager.LocalPlayer) < 30f
                    && c.Health > 0)
                .OrderBy(c => c.HealthPercent)
                .OrderBy(c => c.Profession == Profession.Enforcer)
                .OrderBy(c => c.Profession == Profession.Doctor)
                .OrderBy(c => c.Profession == Profession.Soldier)
                .FirstOrDefault();

            if (player != null)
            {
                actionTarget.ShouldSetTarget = true;
                actionTarget.Target = player;
                return true;
            }

            return false;
        }

        protected bool SpellChecksNanoSkillsPlayer(Spell spell, SimpleChar fightingTarget)
        {
            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || Playfield.ModelIdentity.Instance == 152) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Find(spell.Nanoline, out Buff buff))
            {
                if (spell.StackingOrder < buff.StackingOrder || DynelManager.LocalPlayer.RemainingNCU < spell.NCU) { return false; }

                return buff.RemainingTime < 10;
            }

            return false;
        }

        protected bool SpellChecksNanoSkillsOther(Spell spell, SimpleChar fightingTarget)
        {
            if (!IsSettingEnabled("Buffing")
                || !CanCast(spell)
                || Playfield.ModelIdentity.Instance == 152
                || !fightingTarget.IsInLineOfSight
                || (fightingTarget.IsPlayer && !SettingsController.IsCharacterRegistered(fightingTarget.Identity))) { return false; }

            if (fightingTarget.Buffs.Find(spell.Nanoline, out Buff buff))
            {
                if (spell.StackingOrder < buff.StackingOrder || (fightingTarget.IsPlayer && !HasNCU(spell, fightingTarget))) { return false; }

                return buff.RemainingTime < 10;
            }

            return false;
        }
        protected bool SpellChecksOther(Spell spell, NanoLine nanoline, SimpleChar fightingTarget)
        {
            if (!IsSettingEnabled("Buffing")
                || !CanCast(spell)
                || Playfield.ModelIdentity.Instance == 152
                || !fightingTarget.IsInLineOfSight
                || (fightingTarget.IsPlayer && !SettingsController.IsCharacterRegistered(fightingTarget.Identity))) { return false; }

            if (fightingTarget.Buffs.Find(nanoline, out Buff buff))
            {
                if (spell.StackingOrder < buff.StackingOrder || (fightingTarget.IsPlayer && !HasNCU(spell, fightingTarget))) { return false; }

                if (spell.NanoSchool != NanoSchool.Combat && spell.StackingOrder == buff.StackingOrder && buff.RemainingTime > 20f) { return false; }

                //Add here exceptions
                if ((spell.NanoSchool == NanoSchool.Combat || spell.Nanoline == NanoLine.EvasionDebuffs_Agent)
                    && spell.StackingOrder == buff.StackingOrder && buff.RemainingTime > 8f) { return false; }

                return true;
            }

            if (fightingTarget.IsPlayer && !HasNCU(spell, fightingTarget)) { return false; }

            return true;
        }

        protected bool SpellChecksPlayer(Spell spell, NanoLine nanoline)
        {
            if (!IsSettingEnabled("Buffing") || !CanCast(spell) || Playfield.ModelIdentity.Instance == 152) { return false; }

            if (RelevantGenericNanos.HpBuffs.Contains(spell.Id) && DynelManager.LocalPlayer.Buffs.Contains(NanoLine.DoctorHPBuffs)) { return false; }

            if (DynelManager.LocalPlayer.Buffs.Find(nanoline, out Buff buff))
            {
                if (spell.StackingOrder < buff.StackingOrder || DynelManager.LocalPlayer.RemainingNCU < Math.Abs(spell.NCU - buff.NCU)) { return false; }

                if (spell.StackingOrder == buff.StackingOrder && buff.RemainingTime > 20f) { return false; }

                return true;
            }

            return DynelManager.LocalPlayer.RemainingNCU >= spell.NCU;
        }

        protected bool CanCast(Spell spell)
        {
            return spell.Cost < DynelManager.LocalPlayer.Nano;
        }

        public static void CancelBuffs(int[] buffsToCancel)
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs)
            {
                if (buffsToCancel.Contains(buff.Id))
                    buff.Remove();
            }
        }

        protected bool IsSettingEnabled(string settingName)
        {
            return _settings[settingName].AsBool();
        }

        protected bool HasNCU(Spell spell, SimpleChar target)
        {
            return SettingsController.GetRemainingNCU(target.Identity) > spell.NCU;
        }

        private void TeleportEnded(object sender, EventArgs e)
        {
            _lastCombatTime = double.MinValue;
        }

        protected void CancelHostileAuras(int[] auras)
        {
            if (Time.NormalTime - _lastCombatTime > 5)
                CancelBuffs(auras);
        }

        protected bool IsInsideInnerSanctum()
        {
            return DynelManager.LocalPlayer.Buffs.Any(buff => buff.Id == RelevantGenericNanos.InnerSanctumDebuff);
        }

        public bool AttackingMob(SimpleChar mob)
        {
            if (Team.IsInTeam)
                return Team.Members.Any(c => c.Character?.FightingTarget?.Identity == c.Identity);

            return DynelManager.LocalPlayer.FightingTarget?.Identity == mob.Identity;
        }

        public bool AttackingTeam(SimpleChar mob)
        {
            if (mob.FightingTarget == null) { return false; }

            if (Team.IsInTeam)
                return Team.Members.Select(m => m.Name).Contains(mob.FightingTarget?.Name)
                        || (bool)mob.FightingTarget?.IsPet;

            return mob.FightingTarget?.Name == DynelManager.LocalPlayer.Name
                || (bool)mob.FightingTarget?.IsPet;
        }

        public static bool InCombat()
        {
            if (Team.IsInTeam)
            {
                return DynelManager.Characters
                    .Any(c => Team.Members.Select(m => m.Name).Contains(c.FightingTarget?.Name));
            }

            return DynelManager.Characters
                    .Any(c => c.FightingTarget?.Name == DynelManager.LocalPlayer.Name);
        }

        public static CharacterWieldedWeapon GetWieldedWeapons(SimpleChar local) => (CharacterWieldedWeapon)local.GetStat(Stat.EquippedWeapons);

        #endregion

        #region Misc

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

        //public static void Network_N3MessageSent(object s, N3Message n3Msg)
        //{
        //    if (!IsActiveWindow || n3Msg.Identity != DynelManager.LocalPlayer.Identity) { return; }

        //    //Chat.WriteLine($"{n3Msg.Identity != DynelManager.LocalPlayer.Identity}");

        //    if (n3Msg.N3MessageType == N3MessageType.LookAt)
        //    {
        //        LookAtMessage lookAtMsg = (LookAtMessage)n3Msg;
        //        IPCChannel.Broadcast(new TargetMessage()
        //        {
        //            Target = lookAtMsg.Target
        //        });
        //    }
        //    else if (n3Msg.N3MessageType == N3MessageType.Attack)
        //    {
        //        AttackMessage attackMsg = (AttackMessage)n3Msg;
        //        IPCChannel.Broadcast(new AttackIPCMessage()
        //        {
        //            Target = attackMsg.Target
        //        });
        //    }
        //    else if (n3Msg.N3MessageType == N3MessageType.StopFight)
        //    {
        //        StopFightMessage stopAttackMsg = (StopFightMessage)n3Msg;
        //        IPCChannel.Broadcast(new StopAttackIPCMessage());
        //    }
        //}

        //public static void OnStopAttackMessage(int sender, IPCMessage msg)
        //{
        //    if (IsActiveWindow)
        //        return;

        //    if (Game.IsZoning)
        //        return;

        //    DynelManager.LocalPlayer.StopAttack();
        //}

        //public static void OnTargetMessage(int sender, IPCMessage msg)
        //{
        //    if (IsActiveWindow)
        //        return;

        //    if (Game.IsZoning)
        //        return;

        //    TargetMessage targetMsg = (TargetMessage)msg;
        //    Targeting.SetTarget(targetMsg.Target);
        //}

        //public static void OnAttackMessage(int sender, IPCMessage msg)
        //{
        //    if (IsActiveWindow)
        //        return;

        //    if (Game.IsZoning)
        //        return;

        //    AttackIPCMessage attackMsg = (AttackIPCMessage)msg;
        //    Dynel targetDynel = DynelManager.GetDynel(attackMsg.Target);
        //    DynelManager.LocalPlayer.Attack(targetDynel, true);
        //}

        public static bool IsRaidEnabled(string[] param)
        {
            return param.Length > 0 && "raid".Equals(param[0]);
        }

        public static Identity[] GetRegisteredCharactersInvite()
        {
            Identity[] registeredCharacters = SettingsController.GetRegisteredCharacters();
            int firstTeamCount = registeredCharacters.Length > 6 ? 6 : registeredCharacters.Length;
            Identity[] firstTeamCharacters = new Identity[firstTeamCount];
            Array.Copy(registeredCharacters, firstTeamCharacters, firstTeamCount);
            return firstTeamCharacters;
        }

        public static Identity[] GetRemainingRegisteredCharacters()
        {
            Identity[] registeredCharacters = SettingsController.GetRegisteredCharacters();
            int characterCount = registeredCharacters.Length - 6;
            Identity[] remainingCharacters = new Identity[characterCount];
            if (characterCount > 0)
            {
                Array.Copy(registeredCharacters, 6, remainingCharacters, 0, characterCount);
            }
            return remainingCharacters;
        }

        public static void SendTeamInvite(Identity[] targets)
        {
            foreach (Identity target in targets)
            {
                if (target != DynelManager.LocalPlayer.Identity)
                    Team.Invite(target);
            }
        }

        public static void OnDisband(int sender, IPCMessage msg)
        {
            Team.Leave();
        }

        public static void DisbandCommand(string command, string[] param, ChatWindow chatWindow)
        {
            Team.Disband();
            IPCChannel.Broadcast(new DisbandMessage());
        }

        public static void RaidCommand(string command, string[] param, ChatWindow chatWindow)
        {
            if (Team.IsLeader)
                Team.ConvertToRaid();
            else
                Chat.WriteLine("Needs to be used from leader.");
        }

        public static void ReformCommand(string command, string[] param, ChatWindow chatWindow)
        {
            Team.Disband();
            IPCChannel.Broadcast(new DisbandMessage());
            Task.Factory.StartNew(
                async () =>
                {
                    await Task.Delay(1000);
                    FormCommand("form", param, chatWindow);
                });
        }

        public static void FormCommand(string command, string[] param, ChatWindow chatWindow)
        {
            if (!Team.IsInTeam)
            {
                SendTeamInvite(GetRegisteredCharactersInvite());

                if (IsRaidEnabled(param))
                {
                    Task.Factory.StartNew(
                        async () =>
                        {
                            await Task.Delay(100);
                            Team.ConvertToRaid();
                            await Task.Delay(1000);
                            SendTeamInvite(GetRemainingRegisteredCharacters());
                            await Task.Delay(100);
                        });
                }
            }
            else
            {
                Chat.WriteLine("Cannot form a team. Character already in team. Disband first.");
            }
        }

        [Flags]
        public enum CharacterWieldedWeapon
        {
            Fists = 0x0,              // 0x00000000000000000000b Fists / invalid
            MartialArts = 0x01,       // 0x00000000000000000001b martialarts / fists
            Melee = 0x02,             // 0x00000000000000000010b
            Ranged = 0x04,            // 0x00000000000000000100b
            Bow = 0x08,               // 0x00000000000000001000b
            Smg = 0x10,               // 0x00000000000000010000b
            Edged1H = 0x20,           // 0x00000000000000100000b
            Blunt1H = 0x40,           // 0x00000000000001000000b
            Edged2H = 0x80,           // 0x00000000000010000000b
            Blunt2H = 0x100,          // 0x00000000000100000000b
            Piercing = 0x200,         // 0x00000000001000000000b
            Pistol = 0x400,           // 0x00000000010000000000b
            AssaultRifle = 0x800,     // 0x00000000100000000000b
            Rifle = 0x1000,           // 0x00000001000000000000b
            Shotgun = 0x2000,         // 0x00000010000000000000b
            Grenade = 0x8000,         // 0x00000100000000000000b // 0x00001000000000000000b grenade / martial arts
            MeleeEnergy = 0x4000,     // 0x00001000000000000000b // 0x00000100000000000000b
            RangedEnergy = 0x10000,   // 0x00010000000000000000b
            Grenade2 = 0x20000,       // 0x00100000000000000000b
            HeavyWeapons = 0x40000,   // 0x01000000000000000000b
        }

        // This will eventually be done dynamically but for now I will implement
        // it statically so we can have it functional
        private Stat GetSkillLockStat(Item item)
        {
            switch (item.HighId)
            {
                case RelevantGenericItems.ReflectGraft:
                    return Stat.SpaceTime;
                case RelevantGenericItems.UponAWaveOfSummerLow:
                case RelevantGenericItems.UponAWaveOfSummerHigh:
                    return Stat.Riposte;
                case RelevantGenericItems.FlowerOfLifeLow:
                case RelevantGenericItems.FlowerOfLifeHigh:
                case RelevantGenericItems.BlessedWithThunderLow:
                case RelevantGenericItems.BlessedWithThunderHigh:
                    return Stat.MartialArts;
                case RelevantGenericItems.FlurryOfBlowsLow:
                case RelevantGenericItems.FlurryOfBlowsHigh:
                    return Stat.AggDef;
                case RelevantGenericItems.StrengthOfTheImmortal:
                case RelevantGenericItems.MightOfTheRevenant:
                case RelevantGenericItems.BarrowStrength:
                    return Stat.Strength;
                case RelevantGenericItems.MeteoriteSpikes:
                case RelevantGenericItems.LavaCapsule:
                case RelevantGenericItems.KizzermoleGumboil:
                case RelevantGenericItems.FallenStar:
                case RelevantGenericItems.TearOfOedipus:
                    return Stat.SharpObject;
                case RelevantGenericItems.SteamingHotCupOfEnhancedCoffee:
                    return Stat.RunSpeed;
                case RelevantGenericItems.GnuffsEternalRiftCrystal:
                    return Stat.MapNavigation;
                case RelevantGenericItems.Drone:
                    return Stat.MaterialCreation;
                case RelevantGenericItems.WenWen:
                    return Stat.RangedEnergy;
                case RelevantGenericItems.DaTaunterLow:
                case RelevantGenericItems.DaTaunterHigh:
                    return Stat.Psychology;
                case RelevantGenericItems.HSRLow:
                case RelevantGenericItems.HSRHigh:
                    return Stat.Grenade;
                default:
                    throw new Exception($"No skill lock stat defined for item id {item.HighId}");
            }
        }
        private void OnZoned(object s, EventArgs e)
        {
            _lastZonedTime = Time.NormalTime;
        }

        public static void Team_TeamRequest(object s, TeamRequestEventArgs e)
        {
            if (SettingsController.IsCharacterRegistered(e.Requester))
            {
                e.Accept();
            }
        }

        protected void RegisterSettingsWindow(string settingsName, string xmlName)
        {
            SettingsController.RegisterSettingsWindow(settingsName, PluginDir + "\\UI\\" + xmlName, _settings);
        }

        private static class RelevantGenericItems
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
            public const int HSRLow = 164780;
            public const int HSRHigh = 164781;
            public const int KizzermoleGumboil = 245323;
            public const int FallenStar = 244214;
            public const int TearOfOedipus = 244216;
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
            public const int Drone = 303188;
            public const int RezCan1 = 301070;
            public const int RezCan2 = 303390;
            public const int ExpCan1 = 288769;
            public const int ExpCan2 = 303376;
            public const int InsuranceCan1 = 300728;
            public const int InsuranceCan2 = 303389;
            public const int PremSitKit = 297274;
            public const int AreteSitKit = 292256;
            public const int SitKit1 = 291082;
            public const int SitKit100 = 291083;
            public const int SitKit200 = 291084;
            public const int SitKit300 = 293296;
            public const int SitKit400 = 293297;
            public const int FreeStim1 = 204103;
            public const int FreeStim50 = 204104;
            public const int FreeStim100 = 204105;
            public const int FreeStim200 = 204106;
            public const int FreeStim300 = 204107;
            public const int HealthAndNanoStim1 = 291043;
            public const int HealthAndNanoStim200 = 291044;
            public const int HealthAndNanoStim400 = 291045;
            public const int AmmoBoxEnergy = 303138;
            public const int AmmoBoxShotgun = 303141;
            public const int AmmoBoxBullets = 303137;
            public const int AmmoBoxGrenade = 303140;
            public const int AmmoBoxArrows = 303136;
            public const int DaTaunterLow = 158045;
            public const int DaTaunterHigh = 158046;
            public const int WenWen = 129656;
        };

        public static class RelevantGenericNanos
        {
            public static int[] HpBuffs = new[] { 95709, 28662, 95720, 95712, 95710, 95711, 28649, 95713, 28660, 95715, 95714, 95718, 95716, 95717, 95719, 42397 };
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
            public const int InsightIntoSL = 268610;
            public static int[] IgnoreNanos = new[] { 302535, 302534, 302544, 302542, 302540, 302538, 302532, 302530 };
            public static int[] AAOTransfer = new[] { 301524, 301520, 267263, 267265 };
            public static int[] KeeperStrStamAgiBuff = new[] { 211158, 211160, 211162, 273365 };

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
        public enum StimTargetSelection
        {
            None, Self, Team, Target
        }

        public static void IPCChannel_Changed(object s, int e)
        {
            IPCChannel.SetChannelId(Convert.ToByte(e));
            Config.Save();
        }
        public static void BioCocoonPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].BioCocoonPercentage = e;
            BioCocoonPercentage = e;
            Config.Save();
        }

        public static void SingleTauntDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].SingleTauntDelay = e;
            SingleTauntDelay = e;
            Config.Save();
        }

        public static void MongoDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].MongoDelay = e;
            MongoDelay = e;
            Config.Save();
        }
        public static void CycleAbsorbsDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].CycleAbsorbsDelay = e;
            CycleAbsorbsDelay = e;
            Config.Save();
        }
        public static void CycleChallengerDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].CycleChallengerDelay = e;
            CycleChallengerDelay = e;
            Config.Save();
        }
        public static void CycleRageDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].CycleRageDelay = e;
            CycleRageDelay = e;
            Config.Save();
        }
        public static void StimTargetName_Changed(object s, string e)
        {
            Config.CharSettings[Game.ClientInst].StimTargetName = e;
            StimTargetName = e;
            Config.Save();
        }
        public static void StimHealthPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].StimHealthPercentage = e;
            StimHealthPercentage = e;
            Config.Save();
        }
        public static void StimNanoPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].StimNanoPercentage = e;
            StimNanoPercentage = e;
            Config.Save();
        }
        public static void KitHealthPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].KitHealthPercentage = e;
            KitHealthPercentage = e;
            Config.Save();
        }
        public static void KitNanoPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].KitNanoPercentage = e;
            KitNanoPercentage = e;
            Config.Save();
        }
        public static void CycleXpPerksDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].CycleXpPerksDelay = e;
            CycleXpPerksDelay = e;
            Config.Save();
        }
        public static void HealPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].HealPercentage = e;
            HealPercentage = e;
            Config.Save();
        }
        public static void CompleteHealPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].CompleteHealPercentage = e;
            CompleteHealPercentage = e;
            Config.Save();
        }
        public static void HealthDrainPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].HealthDrainPercentage = e;
            HealthDrainPercentage = e;
            Config.Save();
        }
        public static void NanoAegisPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].NanoAegisPercentage = e;
            NanoAegisPercentage = e;
            Config.Save();
        }
        public static void NullitySpherePercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].NullitySpherePercentage = e;
            NullitySpherePercentage = e;
            Config.Save();
        }
        public static void IzgimmersWealthPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].IzgimmersWealthPercentage = e;
            IzgimmersWealthPercentage = e;
            Config.Save();
        }
        public static void CycleSpherePerkDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].CycleSpherePerkDelay = e;
            CycleSpherePerkDelay = e;
            Config.Save();
        }
        public static void CycleWitOfTheAtroxPerkDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].CycleWitOfTheAtroxPerkDelay = e;
            CycleWitOfTheAtroxPerkDelay = e;
            Config.Save();
        }
        public static void CycleBioRegrowthPerkDelay_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].CycleBioRegrowthPerkDelay = e;
            CycleBioRegrowthPerkDelay = e;
            Config.Save();
        }
        public static void BioRegrowthPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].BioRegrowthPercentage = e;
            BioRegrowthPercentage = e;
            Config.Save();
        }
        public static void SelfHealPerkPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].SelfHealPerkPercentage = e;
            SelfHealPerkPercentage = e;
            Config.Save();
        }
        public static void SelfNanoPerkPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].SelfNanoPerkPercentage = e;
            SelfNanoPerkPercentage = e;
            Config.Save();
        }
        public static void TeamHealPerkPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].TeamHealPerkPercentage = e;
            TeamHealPerkPercentage = e;
            Config.Save();
        }
        public static void TeamNanoPerkPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].TeamNanoPerkPercentage = e;
            TeamNanoPerkPercentage = e;
            Config.Save();
        }
        public static void BattleGroupHeal1Percentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].BattleGroupHeal1Percentage = e;
            BattleGroupHeal1Percentage = e;
            Config.Save();
        }
        public static void BattleGroupHeal2Percentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].BattleGroupHeal2Percentage = e;
            BattleGroupHeal2Percentage = e;
            Config.Save();
        }
        public static void BattleGroupHeal3Percentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].BattleGroupHeal3Percentage = e;
            BattleGroupHeal3Percentage = e;
            Config.Save();
        }
        public static void BattleGroupHeal4Percentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].BattleGroupHeal4Percentage = e;
            BattleGroupHeal4Percentage = e;
            Config.Save();
        }
        public static void DuckAbsorbsItemPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].DuckAbsorbsItemPercentage = e;
            DuckAbsorbsItemPercentage = e;
            Config.Save();
        }
        public static void BodyDevAbsorbsItemPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].BodyDevAbsorbsItemPercentage = e;
            BodyDevAbsorbsItemPercentage = e;
            Config.Save();
        }
        public static void StrengthAbsorbsItemPercentage_Changed(object s, int e)
        {
            Config.CharSettings[Game.ClientInst].StrengthAbsorbsItemPercentage = e;
            StrengthAbsorbsItemPercentage = e;
            Config.Save();
        }
        #endregion
    }
}