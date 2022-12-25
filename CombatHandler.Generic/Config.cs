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
        public int DocHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].DocHealPercentage : 90;
        [JsonIgnore]
        public int DocCompleteHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].DocCompleteHealPercentage : 20;
        [JsonIgnore]
        public int TraderHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].TraderHealPercentage : 90;
        [JsonIgnore]
        public int TraderHealthDrainPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].TraderHealthDrainPercentage : 90;
        [JsonIgnore]
        public int AgentHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].AgentHealPercentage : 90;
        [JsonIgnore]
        public int AgentCompleteHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].AgentCompleteHealPercentage : 20;
        [JsonIgnore]
        public int MAHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].MAHealPercentage : 90;
        [JsonIgnore]
        public int AdvHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].AdvHealPercentage : 90;
        [JsonIgnore]
        public int AdvCompleteHealPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].AdvCompleteHealPercentage : 20;
        [JsonIgnore]
        public int EnfTauntDelaySingle => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].EnfTauntDelaySingle : 1;
        [JsonIgnore]
        public int EnfTauntDelayArea => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].EnfTauntDelayArea : 1;
        [JsonIgnore]
        public int EnfCycleAbsorbsDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].EnfCycleAbsorbsDelay : 1;
        [JsonIgnore]
        public int EnfCycleChallengerDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].EnfCycleChallengerDelay : 1;
        [JsonIgnore]
        public int EnfCycleRageDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].EnfCycleRageDelay : 1;
        [JsonIgnore]
        public int CratCycleLeadershipDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].CratCycleLeadershipDelay : 1;
        [JsonIgnore]
        public int CratCycleGovernanceDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].CratCycleGovernanceDelay : 1;
        [JsonIgnore]
        public int CratCycleTheDirectorDelay => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].CratCycleTheDirectorDelay : 1;
        [JsonIgnore]
        public int SolTauntDelaySingle => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].SolTauntDelaySingle : 1;
        [JsonIgnore]
        public int EngiBioCocoonPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].EngiBioCocoonPercentage : 65;
        [JsonIgnore]
        public int NTNanoAegisPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].NTNanoAegisPercentage : 70;
        [JsonIgnore]
        public int NTNullitySpherePercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].NTNullitySpherePercentage : 35;
        [JsonIgnore]
        public int NTIzgimmersWealthPercentage => CharSettings != null && CharSettings.ContainsKey(Game.ClientInst) ? CharSettings[Game.ClientInst].NTIzgimmersWealthPercentage : 25;

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

        //Breaking out auto-property
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
        public event EventHandler<int> TraderHealPercentageChangedEvent;
        private int _traderHealPercentage = 90;
        public int TraderHealPercentage
        {
            get
            {
                return _traderHealPercentage;
            }
            set
            {
                if (_traderHealPercentage != value)
                {
                    _traderHealPercentage = value;
                    TraderHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> TraderHealthDrainPercentageChangedEvent;
        private int _traderHealthDrainPercentage = 90;
        public int TraderHealthDrainPercentage
        {
            get
            {
                return _traderHealthDrainPercentage;
            }
            set
            {
                if (_traderHealthDrainPercentage != value)
                {
                    _traderHealthDrainPercentage = value;
                    TraderHealthDrainPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> MAHealPercentageChangedEvent;
        private int _maHealPercentage = 90;
        public int MAHealPercentage
        {
            get
            {
                return _maHealPercentage;
            }
            set
            {
                if (_maHealPercentage != value)
                {
                    _maHealPercentage = value;
                    MAHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> DocHealPercentageChangedEvent;
        private int _docHealPercentage = 90;
        public int DocHealPercentage
        {
            get
            {
                return _docHealPercentage;
            }
            set
            {
                if (_docHealPercentage != value)
                {
                    _docHealPercentage = value;
                    DocHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> DocCompleteHealPercentageChangedEvent;
        private int _docCompleteHealPercentage = 20;
        public int DocCompleteHealPercentage
        {
            get
            {
                return _docCompleteHealPercentage;
            }
            set
            {
                if (_docCompleteHealPercentage != value)
                {
                    _docCompleteHealPercentage = value;
                    DocCompleteHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> AgentHealPercentageChangedEvent;
        private int _agentHealPercentage = 90;
        public int AgentHealPercentage
        {
            get
            {
                return _agentHealPercentage;
            }
            set
            {
                if (_agentHealPercentage != value)
                {
                    _agentHealPercentage = value;
                    AgentHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> AgentCompleteHealPercentageChangedEvent;
        private int _agentCompleteHealPercentage = 20;
        public int AgentCompleteHealPercentage
        {
            get
            {
                return _agentCompleteHealPercentage;
            }
            set
            {
                if (_agentCompleteHealPercentage != value)
                {
                    _agentCompleteHealPercentage = value;
                    AgentCompleteHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> AdvHealPercentageChangedEvent;
        private int _advHealPercentage = 90;
        public int AdvHealPercentage
        {
            get
            {
                return _advHealPercentage;
            }
            set
            {
                if (_advHealPercentage != value)
                {
                    _advHealPercentage = value;
                    AdvHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> AdvCompleteHealPercentageChangedEvent;
        private int _advCompleteHealPercentage = 20;
        public int AdvCompleteHealPercentage
        {
            get
            {
                return _advCompleteHealPercentage;
            }
            set
            {
                if (_advCompleteHealPercentage != value)
                {
                    _advCompleteHealPercentage = value;
                    AdvCompleteHealPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> EnfTauntDelayAreaChangedEvent;
        private int _enfTauntDelayArea = 1;
        public int EnfTauntDelayArea
        {
            get
            {
                return _enfTauntDelayArea;
            }
            set
            {
                if (_enfTauntDelayArea != value)
                {
                    _enfTauntDelayArea = value;
                    EnfTauntDelayAreaChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> EnfTauntDelaySingleChangedEvent;
        private int _enfTauntDelaySingle = 1;
        public int EnfTauntDelaySingle
        {
            get
            {
                return _enfTauntDelaySingle;
            }
            set
            {
                if (_enfTauntDelaySingle != value)
                {
                    _enfTauntDelaySingle = value;
                    EnfTauntDelaySingleChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> EnfCycleAbsorbsDelayChangedEvent;
        private int _enfCycleAbsorbsDelay = 1;
        public int EnfCycleAbsorbsDelay
        {
            get
            {
                return _enfCycleAbsorbsDelay;
            }
            set
            {
                if (_enfCycleAbsorbsDelay != value)
                {
                    _enfCycleAbsorbsDelay = value;
                    EnfCycleAbsorbsDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> EnfCycleChallengerDelayChangedEvent;
        private int _enfCycleChallengerDelay = 1;
        public int EnfCycleChallengerDelay
        {
            get
            {
                return _enfCycleChallengerDelay;
            }
            set
            {
                if (_enfCycleChallengerDelay != value)
                {
                    _enfCycleChallengerDelay = value;
                    EnfCycleChallengerDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> EnfCycleRageDelayChangedEvent;
        private int _enfCycleRageDelay = 1;
        public int EnfCycleRageDelay
        {
            get
            {
                return _enfCycleRageDelay;
            }
            set
            {
                if (_enfCycleRageDelay != value)
                {
                    _enfCycleRageDelay = value;
                    EnfCycleRageDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> CratCycleLeadershipDelayChangedEvent;
        private int _cratCycleLeadershipDelay = 1;
        public int CratCycleLeadershipDelay
        {
            get
            {
                return _cratCycleLeadershipDelay;
            }
            set
            {
                if (_cratCycleLeadershipDelay != value)
                {
                    _cratCycleLeadershipDelay = value;
                    CratCycleLeadershipDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> CratCycleGovernanceDelayChangedEvent;
        private int _cratCycleGovernanceDelay = 1;
        public int CratCycleGovernanceDelay
        {
            get
            {
                return _cratCycleGovernanceDelay;
            }
            set
            {
                if (_cratCycleGovernanceDelay != value)
                {
                    _cratCycleGovernanceDelay = value;
                    CratCycleGovernanceDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> CratCycleTheDirectorDelayChangedEvent;
        private int _cratCycleTheDirector = 1;
        public int CratCycleTheDirectorDelay
        {
            get
            {
                return _cratCycleTheDirector;
            }
            set
            {
                if (_cratCycleTheDirector != value)
                {
                    _cratCycleTheDirector = value;
                    CratCycleTheDirectorDelayChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> SolTauntDelaySingleChangedEvent;
        private int _solTauntDelaySingle = 1;
        public int SolTauntDelaySingle
        {
            get
            {
                return _solTauntDelaySingle;
            }
            set
            {
                if (_solTauntDelaySingle != value)
                {
                    _solTauntDelaySingle = value;
                    SolTauntDelaySingleChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> EngiBioCocoonPercentageChangedEvent;
        private int _engiBioCocoonPercentage = 65;
        public int EngiBioCocoonPercentage
        {
            get
            {
                return _engiBioCocoonPercentage;
            }
            set
            {
                if (_engiBioCocoonPercentage != value)
                {
                    _engiBioCocoonPercentage = value;
                    EngiBioCocoonPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> NTNanoAegisPercentageChangedEvent;
        private int _ntNanoAegisPercentage = 70;
        public int NTNanoAegisPercentage
        {
            get
            {
                return _ntNanoAegisPercentage;
            }
            set
            {
                if (_ntNanoAegisPercentage != value)
                {
                    _ntNanoAegisPercentage = value;
                    NTNanoAegisPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> NTNullitySpherePercentageChangedEvent;
        private int _ntNullitySpherePercentage = 35;
        public int NTNullitySpherePercentage
        {
            get
            {
                return _ntNullitySpherePercentage;
            }
            set
            {
                if (_ntNullitySpherePercentage != value)
                {
                    _ntNullitySpherePercentage = value;
                    NTNullitySpherePercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> NTIzgimmersWealthPercentageChangedEvent;
        private int _ntIzgimmersWealthPercentage = 35;
        public int NTIzgimmersWealthPercentage
        {
            get
            {
                return _ntIzgimmersWealthPercentage;
            }
            set
            {
                if (_ntIzgimmersWealthPercentage != value)
                {
                    _ntIzgimmersWealthPercentage = value;
                    NTIzgimmersWealthPercentageChangedEvent?.Invoke(this, value);
                }
            }
        }
        public event EventHandler<int> NTCycleAbsorbsDelayChangedEvent;
        private int _ntCycleAbsorbsDelay = 15;
        public int NTCycleAbsorbsDelay
        {
            get
            {
                return _ntCycleAbsorbsDelay;
            }
            set
            {
                if (_ntCycleAbsorbsDelay != value)
                {
                    _ntCycleAbsorbsDelay = value;
                    NTCycleAbsorbsDelayChangedEvent?.Invoke(this, value);
                }
            }
        }

        #endregion
    }
}

