﻿using AOSharp.Character;
using AOSharp.Common.GameData;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using CombatHandler.Generic.IPCMessages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Character.State
{
    public class CharacterState : AOPluginEntry
    {
        private static Dictionary<Identity, int> RemainingNCU = new Dictionary<Identity, int>();
        private static Dictionary<Identity, CharacterWeaponType> WeaponType = new Dictionary<Identity, CharacterWeaponType>();
        private static Dictionary<Identity, SpecialAttacks> SupportedSpecialAttacks = new Dictionary<Identity, SpecialAttacks>();

        private static IPCChannel ReportingIPCChannel;

        private static double _lastUpdateTime = 0;
        public override void Run(string pluginDir)
        {
            ReportingIPCChannel = new IPCChannel(112);

            ReportingIPCChannel.RegisterCallback((int)IPCOpcode.CharacterState, OnCharacterStateMessage);
            ReportingIPCChannel.RegisterCallback((int)IPCOpcode.CharacterSpecials, OnCharacterSpecialsMessage);
            ReportingIPCChannel.RegisterCallback((int)IPCOpcode.Disband, OnDisband);
            ReportingIPCChannel.RegisterCallback((int)IPCOpcode.UseGrid, OnUseGrid);
            ReportingIPCChannel.RegisterCallback((int)IPCOpcode.UseFGrid, OnUseFGrid);
            Game.OnUpdate += ReportCharacterState;
            new TeamCommands().RegisterCommands();
        }

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        public static void BroadcastDisband()
        {
            ReportingIPCChannel.Broadcast(new DisbandMessage());
        }
        public static void BroadcastUseGrid()
        {
            ReportingIPCChannel.Broadcast(new UseGrid());
        }

        public static void BroadcastUseFGrid()
        {
            ReportingIPCChannel.Broadcast(new UseFGrid());
        }

        public static int GetRemainingNCU(Identity target)
        {
            return CharacterState.RemainingNCU.ContainsKey(target) ? CharacterState.RemainingNCU[target] : 0;
        }

        public static CharacterWieldedWeapon GetWieldedWeapon(LocalPlayer local)
        {
            if (local.GetStat(Stat.EquippedWeapons) == 1028)
                return CharacterWieldedWeapon.Pistol;
            if (local.GetStat(Stat.EquippedWeapons) == 8196)
                return CharacterWieldedWeapon.Shotgun;
            if (local.GetStat(Stat.EquippedWeapons) == 3076)
                return CharacterWieldedWeapon.PistolAndAssaultRifle;
            if (local.GetStat(Stat.EquippedWeapons) == 4100)
                return CharacterWieldedWeapon.Rifle;
            if (local.GetStat(Stat.EquippedWeapons) == 2052)
                return CharacterWieldedWeapon.AssaultRifle;
            if (local.GetStat(Stat.EquippedWeapons) == 20)
                return CharacterWieldedWeapon.Smg;
            if (local.GetStat(Stat.EquippedWeapons) == 12)
                return CharacterWieldedWeapon.Bow;
            if (local.GetStat(Stat.EquippedWeapons) == 1)
                return CharacterWieldedWeapon.Fists;
            if (local.GetStat(Stat.EquippedWeapons) == 258)
                return CharacterWieldedWeapon.Blunt2H;
            if (local.GetStat(Stat.EquippedWeapons) == 66)
                return CharacterWieldedWeapon.Blunt1H;
            if (local.GetStat(Stat.EquippedWeapons) == 130)
                return CharacterWieldedWeapon.Edged2H;
            if (local.GetStat(Stat.EquippedWeapons) == 34)
                return CharacterWieldedWeapon.Edged1H;

            //if (local.GetStat(Stat.EquippedWeapons) == )
            //    return CharacterWieldedWeapon.Grenade;
            //if (local.GetStat(Stat.EquippedWeapons) == )
            //    return CharacterWieldedWeapon.HeavyWeapons;
            //if (local.GetStat(Stat.EquippedWeapons) == )
            //    return CharacterWieldedWeapon.Piercing;

            return CharacterWieldedWeapon.Invalid;
        }
        public static CharacterWieldedWeapon GetWieldedWeaponOther(SimpleChar local)
        {
            if (local.GetStat(Stat.EquippedWeapons) == 1028)
                return CharacterWieldedWeapon.Pistol;
            if (local.GetStat(Stat.EquippedWeapons) == 8196)
                return CharacterWieldedWeapon.Shotgun;
            if (local.GetStat(Stat.EquippedWeapons) == 3076)
                return CharacterWieldedWeapon.PistolAndAssaultRifle;
            if (local.GetStat(Stat.EquippedWeapons) == 4100)
                return CharacterWieldedWeapon.Rifle;
            if (local.GetStat(Stat.EquippedWeapons) == 2052)
                return CharacterWieldedWeapon.AssaultRifle;
            if (local.GetStat(Stat.EquippedWeapons) == 20)
                return CharacterWieldedWeapon.Smg;
            if (local.GetStat(Stat.EquippedWeapons) == 12)
                return CharacterWieldedWeapon.Bow;
            if (local.GetStat(Stat.EquippedWeapons) == 1)
                return CharacterWieldedWeapon.Fists;
            if (local.GetStat(Stat.EquippedWeapons) == 258)
                return CharacterWieldedWeapon.Blunt2H;
            if (local.GetStat(Stat.EquippedWeapons) == 66)
                return CharacterWieldedWeapon.Blunt1H;
            if (local.GetStat(Stat.EquippedWeapons) == 130)
                return CharacterWieldedWeapon.Edged2H;
            if (local.GetStat(Stat.EquippedWeapons) == 34)
                return CharacterWieldedWeapon.Edged1H;

            //if (local.GetStat(Stat.EquippedWeapons) == )
            //    return CharacterWieldedWeapon.Grenade;
            //if (local.GetStat(Stat.EquippedWeapons) == )
            //    return CharacterWieldedWeapon.HeavyWeapons;
            //if (local.GetStat(Stat.EquippedWeapons) == )
            //    return CharacterWieldedWeapon.Piercing;

            return CharacterWieldedWeapon.Invalid;
        }

        public static CharacterWeaponType GetWeaponType(Identity target)
        {
            return CharacterState.WeaponType.ContainsKey(target) ? CharacterState.WeaponType[target] : CharacterWeaponType.UNAVAILABLE;
        }

        public static Identity[] GetRegisteredCharacters()
        {
            return CharacterState.RemainingNCU.Keys.ToArray();
        }

        public static bool IsCharacterRegistered(Identity target)
        {
            return CharacterState.RemainingNCU.ContainsKey(target);
        }        

        public static SpecialAttacks GetSpecialAttacks(Identity target)
        {
            return CharacterState.SupportedSpecialAttacks.ContainsKey(target) ? CharacterState.SupportedSpecialAttacks[target] : new SpecialAttacks();
        }

        public static void OnCharacterSpecialsMessage(int sender, IPCMessage msg)
        {
            try
            {
                if (Game.IsZoning)
                    return;

                CharacterSpecialsMessage specialsMessage = (CharacterSpecialsMessage)msg;
                SupportedSpecialAttacks[specialsMessage.Character] = SpecialAttacks.FromMessage(specialsMessage);
            }
            catch (Exception e)
            {
                Chat.WriteLine(e);
            }
        }

        private static void OnCharacterStateMessage(int sender, IPCMessage msg)
        {
            try
            {
                if (Game.IsZoning)
                    return;

                CharacterStateMessage stateMessage = (CharacterStateMessage)msg;
                RemainingNCU[stateMessage.Character] = stateMessage.RemainingNCU;
                WeaponType[stateMessage.Character] = stateMessage.WeaponType;
            } catch(Exception e)
            {
                Chat.WriteLine(e);
            }
        }

        private static void OnDisband(int sender, IPCMessage msg)
        {
            Team.Leave();
        }

        private static void OnUseGrid(int sender, IPCMessage msg)
        {
            Item GridCan = null;

            Inventory.Find(288822, out GridCan);

            GridCan.Use();
        }

        private static void OnUseFGrid(int sender, IPCMessage msg)
        {
            Item FGridCan = null;

            Inventory.Find(296805, out FGridCan);

            FGridCan.Use();
        }

        private static void ReportCharacterState(object sender, float deltaTime)
        {
            if (Time.NormalTime - _lastUpdateTime > 1)
            {
                CharacterStateMessage stateMessage = CharacterStateMessage.ForLocalPlayer();
                CharacterSpecialsMessage specialsMessage = CharacterSpecialsMessage.ForLocalPlayer();

                ReportingIPCChannel.Broadcast(stateMessage);
                ReportingIPCChannel.Broadcast(specialsMessage);

                OnCharacterSpecialsMessage(0, specialsMessage);
                OnCharacterStateMessage(0, stateMessage);

                _lastUpdateTime = Time.NormalTime;
            }
        }
    }
}