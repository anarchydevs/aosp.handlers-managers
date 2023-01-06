﻿using System;
using System.Collections.Generic;
using System.IO;
using AOSharp.Core;
using AOSharp.Core.UI;
using System.Xml;
using Newtonsoft.Json;

namespace CombatHandler.Generic
{
    public class Config
    {
        public Dictionary<int, CharacterSettings> CharSettings { get; set; }

        protected string _path;

        #region Json

        [JsonIgnore]
        public int IPCChannel => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].IPCChannel : 0;
        [JsonIgnore]
        public int HealthDrainPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].HealthDrainPercentage : 90;
        [JsonIgnore]
        public int HealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].HealPercentage : 90;
        [JsonIgnore]
        public int CompleteHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].CompleteHealPercentage : 20;
        [JsonIgnore]
        public int SingleTauntDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].SingleTauntDelay : 1;
        [JsonIgnore]
        public int MongoTauntDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].MongoTauntDelay : 1;
        [JsonIgnore]
        public int CycleAbsorbsDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].CycleAbsorbsDelay : 1;
        [JsonIgnore]
        public int CycleChallengerDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].CycleChallengerDelay : 1;
        [JsonIgnore]
        public int CycleRageDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].CycleRageDelay : 1;
        [JsonIgnore]
        public int CycleXpPerksDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].CycleXpPerksDelay : 1;
        [JsonIgnore]
        public int BioCocoonPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].BioCocoonPercentage : 65;
        [JsonIgnore]
        public int NTNanoAegisPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].NanoAegisPercentage : 70;
        [JsonIgnore]
        public int NTNullitySpherePercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].NullitySpherePercentage : 35;
        [JsonIgnore]
        public int NTIzgimmersWealthPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].IzgimmersWealthPercentage : 25;

        #endregion

        public static Config Load(string path)
        {
            Config config;

            try
            {
                config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));

                config._path = path;
            }
            catch
            {
                Chat.WriteLine($"No config file found.");
                Chat.WriteLine($"Using default settings");

                config = new Config
                {
                    CharSettings = new Dictionary<int, CharacterSettings>()
                    {
                        { Game.ClientInst, new CharacterSettings() }
                    }
                };

                config._path = path;

                config.Save();
            }

            return config;
        }

        public void Save()
        {
            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Generic"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\Generic");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Generic"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Generic");

            if (!Directory.Exists($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Generic\\{Game.ClientInst}"))
                Directory.CreateDirectory($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\AOSharp\\Generic\\{Game.ClientInst}");

            File.WriteAllText(_path, JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
        }
    }

    public class CharacterSettings
    {
        #region Breaking out Auto-Properties

        public event EventHandler<int> IPCChannelChangedEvent;
        private int _ipcChannel = 0;

        public int IPCChannel {
            get
            {
                return _ipcChannel;
            }
            set
            {
                if (_ipcChannel != value)
                {
                    _ipcChannel = value;
                    IPCChannelChangedEvent?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<int> HealthDrainPercentageChangedEvent;
        private int _healthDrainPercentage = 90;
        public int HealthDrainPercentage
        {
            get
            {
                return _healthDrainPercentage;
            }
            set
            {
                if (_healthDrainPercentage != value)
                {
                    _healthDrainPercentage = value;
                    HealthDrainPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> HealPercentageChangedEvent;
        private int _healPercentage = 90;
        public int HealPercentage
        {
            get
            {
                return _healPercentage;
            }
            set
            {
                if (_healPercentage != value)
                {
                    _healPercentage = value;
                    HealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> CompleteHealPercentageChangedEvent;
        private int _completeHealPercentage = 20;
        public int CompleteHealPercentage
        {
            get
            {
                return _completeHealPercentage;
            }
            set
            {
                if (_completeHealPercentage != value)
                {
                    _completeHealPercentage = value;
                    CompleteHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }

        public event EventHandler<int> MongoTauntDelayChangedEvent;
        private int _mongoTauntDelay = 1;
        public int MongoTauntDelay
        {
            get
            {
                return _mongoTauntDelay;
            }
            set
            {
                if (_mongoTauntDelay != value)
                {
                    _mongoTauntDelay = value;
                    MongoTauntDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> SingleTauntDelayChangedEvent;
        private int _SingleTauntDelay = 1;
        public int SingleTauntDelay
        {
            get
            {
                return _SingleTauntDelay;
            }
            set
            {
                if (_SingleTauntDelay != value)
                {
                    _SingleTauntDelay = value;
                    SingleTauntDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> CycleAbsorbsDelayChangedEvent;
        private int _cycleAbsorbsDelay = 1;
        public int CycleAbsorbsDelay
        {
            get
            {
                return _cycleAbsorbsDelay;
            }
            set
            {
                if (_cycleAbsorbsDelay != value)
                {
                    _cycleAbsorbsDelay = value;
                    CycleAbsorbsDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> CycleChallengerDelayChangedEvent;
        private int _cycleChallengerDelay = 1;
        public int CycleChallengerDelay
        {
            get
            {
                return _cycleChallengerDelay;
            }
            set
            {
                if (_cycleChallengerDelay != value)
                {
                    _cycleChallengerDelay = value;
                    CycleChallengerDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> CycleRageDelayChangedEvent;
        private int _cycleRageDelay = 1;
        public int CycleRageDelay
        {
            get
            {
                return _cycleRageDelay;
            }
            set
            {
                if (_cycleRageDelay != value)
                {
                    _cycleRageDelay = value;
                    CycleRageDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> CycleXpPerksDelayChangedEvent;
        private int _cycleXpPerksDelay = 1;
        public int CycleXpPerksDelay
        {
            get
            {
                return _cycleXpPerksDelay;
            }
            set
            {
                if (_cycleXpPerksDelay != value)
                {
                    _cycleXpPerksDelay = value;
                    CycleXpPerksDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> BioCocoonPercentageChangedEvent;
        private int _bioCocoonPercentage = 65;
        public int BioCocoonPercentage
        {
            get
            {
                return _bioCocoonPercentage;
            }
            set
            {
                if (_bioCocoonPercentage != value)
                {
                    _bioCocoonPercentage = value;
                    BioCocoonPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> NanoAegisPercentageChangedEvent;
        private int _nanoAegisPercentage = 70;
        public int NanoAegisPercentage
        {
            get
            {
                return _nanoAegisPercentage;
            }
            set
            {
                if (_nanoAegisPercentage != value)
                {
                    _nanoAegisPercentage = value;
                    NanoAegisPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> NullitySpherePercentageChangedEvent;
        private int _nullitySpherePercentage = 35;
        public int NullitySpherePercentage
        {
            get
            {
                return _nullitySpherePercentage;
            }
            set
            {
                if (_nullitySpherePercentage != value)
                {
                    _nullitySpherePercentage = value;
                    NullitySpherePercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> IzgimmersWealthPercentageChangedEvent;
        private int _izgimmersWealthPercentage = 35;
        public int IzgimmersWealthPercentage
        {
            get
            {
                return _izgimmersWealthPercentage;
            }
            set
            {
                if (_izgimmersWealthPercentage != value)
                {
                    _izgimmersWealthPercentage = value;
                    IzgimmersWealthPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }

        #endregion
    }
}

