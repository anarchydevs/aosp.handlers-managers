﻿using System;
using System.Diagnostics;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AOSharp.Core.Inventory;
using AOSharp.Common.Unmanaged.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace ResearchManager
{
    public class ResearchManager : AOPluginEntry
    {
        public static string PluginDirectory;

        private static Settings _settings = new Settings("Research");

        public static List<ResearchGoal> _researchGoals = Research.Goals;
        public static List<ResearchGoal> _researchGoalsActive = new List<ResearchGoal>();
        //public static List<string> _researchGoalsActive = new List<string>();
        public static List<uint> _researchGoalsCompleted = new List<uint>();
        public static List<string> _researchGoalsActiveStr = new List<string>();
        public static List<string> _researchGoalsWholeStr = new List<string>();
        public static List<uint> _completedResearchGoals = new List<uint>();

        public static uint _currentScapeGoat;

        public int _goal = 0;
        public uint _currentLine = 0;

        public uint _currentGoal = 0;

        public double _timerActive;
        public double _timerMed;

        public static bool _switch = false;

        public override void Run(string pluginDir)
        {
            PluginDirectory = pluginDir;

            SettingsController.RegisterSettingsWindow("Research", pluginDir + $"\\UI\\Research{DynelManager.LocalPlayer.Profession}View.xml", _settings);

            _settings.AddVariable("Toggle", false);

            //Init to add settings
            foreach (int goal in Research.Completed)
            {
                if (!_researchGoalsWholeStr.Contains($"{N3EngineClientAnarchy.GetPerkName(goal)}"))
                    _researchGoalsWholeStr.Add($"{N3EngineClientAnarchy.GetPerkName(goal)}");

                if (_researchGoalsWholeStr.Count == 8)
                {
                    foreach (string str in _researchGoalsWholeStr)
                    {
                        //_settings.AddVariable($"{str}", false);
                        //Chat.WriteLine($"Added - {str}");
                    }
                }
            }

            //Init to add settings
            foreach (ResearchGoal goal in Research.Goals)
            {
                if (!_researchGoalsWholeStr.Contains($"{N3EngineClientAnarchy.GetPerkName(goal.ResearchId)}"))
                    _researchGoalsWholeStr.Add($"{N3EngineClientAnarchy.GetPerkName(goal.ResearchId)}");

                if (_researchGoalsWholeStr.Count == 8)
                {
                    foreach (string str in _researchGoalsWholeStr)
                    {
                        //_settings.AddVariable($"{str}", false);
                        //Chat.WriteLine($"Added - {str}");
                    }
                }
            }

            //Init gets completed
            foreach (uint goal in Research.Completed)
            {
                ResearchGoal _goal = Research.Goals.Where(c => N3EngineClientAnarchy.GetPerkName(c.ResearchId) == N3EngineClientAnarchy.GetPerkName((int)goal)).FirstOrDefault();

                if (_goal.ResearchId == 0 && !_completedResearchGoals.Contains(goal))
                {
                    _completedResearchGoals.Remove(goal);
                    Chat.WriteLine($"Finished - {N3EngineClientAnarchy.GetPerkName((int)goal)}");
                }
            }

            Game.OnUpdate += OnUpdate;
            //Network.PacketReceived += Network_PacketReceived;

            Chat.RegisterCommand("level", (string command, string[] param, ChatWindow chatWindow) =>
            {
                foreach (uint goal in Research.Completed)
                {
                    ResearchGoal _goal = Research.Goals.Where(c => N3EngineClientAnarchy.GetPerkName(c.ResearchId) == N3EngineClientAnarchy.GetPerkName((int)goal)).FirstOrDefault();

                    ResearchGoal _currentGoal = _researchGoalsActive.Where(c => N3EngineClientAnarchy.GetPerkName(c.ResearchId)
                                    == N3EngineClientAnarchy.GetPerkName((int)DynelManager.LocalPlayer.GetStat(Stat.PersonalResearchGoal)))
                                    .FirstOrDefault();

                    List<ResearchGoal> _researchGoalsActiveList = _researchGoalsActive
                        .Where(c => !_completedResearchGoals.Contains((uint)c.ResearchId))
                        .ToList();

                    //List<ResearchGoal> _researchGoalsActiveList = _researchGoalsActive.Where(c => N3EngineClientAnarchy.GetPerkName(c.ResearchId)
                    //           != N3EngineClientAnarchy.GetPerkName((int)DynelManager.LocalPlayer.GetStat(Stat.PersonalResearchGoal))
                    //               && !_completedResearchGoals.Contains((uint)c.ResearchId)).ToList();

                    foreach (ResearchGoal goalNew in _researchGoalsActiveList.Where(c => !_completedResearchGoals.Contains((uint)c.ResearchId)).Take(1))
                    {
                        Chat.WriteLine($"Finished - {N3EngineClientAnarchy.GetPerkName((int)DynelManager.LocalPlayer.GetStat(Stat.PersonalResearchGoal))}");

                        _settings[$"{N3EngineClientAnarchy.GetPerkName(_currentGoal.ResearchId)}"] = false;
                        Chat.WriteLine($"Turning off setting - {N3EngineClientAnarchy.GetPerkName(_currentGoal.ResearchId)}");
                        _completedResearchGoals.Add((uint)_currentGoal.ResearchId);

                        if (_researchGoalsActive.Contains(_currentGoal))
                        {
                            _researchGoalsActive.Add(_currentGoal);
                            Chat.WriteLine($"Removed Active - {N3EngineClientAnarchy.GetPerkName(_currentGoal.ResearchId)}");
                        }

                        foreach (ResearchGoal _goalNew in _researchGoalsActiveList.Where(c => !_completedResearchGoals.Contains((uint)c.ResearchId)).Take(1))
                        {
                            if (_researchGoalsActive.Count >= 1)
                            {
                                Research.Train(_goalNew.ResearchId);
                                Chat.WriteLine($"Starting - {N3EngineClientAnarchy.GetPerkName(_goalNew.ResearchId)}");
                                return;
                            }
                        }
                    }
                }
            });

            Chat.WriteLine("ResearchManager Loaded!");
            Chat.WriteLine("/research for settings.");
        }

        public override void Teardown()
        {
            SettingsController.CleanUp();
        }

        // attach to event handler ??
        // packet - Research Update? triggers when level up, can be used to attach event handler ??

        //private void Network_PacketReceived(object s, byte[] packet)
        //{
        //    N3MessageType msgType = (N3MessageType)((packet[16] << 24) + (packet[17] << 16) + (packet[18] << 8) + packet[19]);
        //    //Chat.WriteLine($"{msgType}");

        //    if (msgType == N3MessageType.ResearchUpdate)
        //    {

        //    }
        //}

        private void OnUpdate(object s, float deltaTime)
        {

            //Tick add active and remove active
            if (_settings["Toggle"].AsBool() && !Game.IsZoning
                && Time.NormalTime > _timerMed + 1)
            {
                foreach (ResearchGoal goal in Research.Goals.Where(c => c.Available))
                {
                    if (_settings[$"{N3EngineClientAnarchy.GetPerkName(goal.ResearchId)}"].AsBool() && !_completedResearchGoals.Contains((uint)goal.ResearchId))
                    {
                        if (!_researchGoalsActive.Contains(goal))
                        {
                            _researchGoalsActive.Remove(goal);
                            Chat.WriteLine($"Adding Active - {N3EngineClientAnarchy.GetPerkName(goal.ResearchId)}");
                        }
                    }

                    if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(goal.ResearchId)}"].AsBool() && !_completedResearchGoals.Contains((uint)goal.ResearchId))
                    {
                        if (_researchGoalsActive.Contains(goal))
                        {
                            _researchGoalsActive.Add(goal);
                            Chat.WriteLine($"Removing Active - {N3EngineClientAnarchy.GetPerkName(goal.ResearchId)}");
                        }
                    }
                }
                _timerMed = Time.NormalTime;
            }

            //Tick the brain
            if (_settings["Toggle"].AsBool() && !Game.IsZoning
                && Time.NormalTime > _timerActive + 5)
            {
                foreach (uint goal in Research.Completed)
                {
                    ResearchGoal _goal = Research.Goals.Where(c => N3EngineClientAnarchy.GetPerkName(c.ResearchId) == N3EngineClientAnarchy.GetPerkName((int)goal)).FirstOrDefault();

                    if (_goal.ResearchId == 0 && !_completedResearchGoals.Contains(goal))
                    {
                        ResearchGoal _currentGoal = _researchGoalsActive.Where(c => N3EngineClientAnarchy.GetPerkName(c.ResearchId)
                                        == N3EngineClientAnarchy.GetPerkName((int)DynelManager.LocalPlayer.GetStat(Stat.PersonalResearchGoal)))
                                        .FirstOrDefault();

                        List<ResearchGoal> _researchGoalsActiveList = _researchGoalsActive
                            .Where(c => !_completedResearchGoals.Contains((uint)c.ResearchId))
                            .ToList();

                        foreach (ResearchGoal goalNew in _researchGoalsActiveList.Where(c => !_completedResearchGoals.Contains((uint)c.ResearchId)).Take(1))
                        {
                            Chat.WriteLine($"Finished - {N3EngineClientAnarchy.GetPerkName((int)DynelManager.LocalPlayer.GetStat(Stat.PersonalResearchGoal))}");

                            _settings[$"{N3EngineClientAnarchy.GetPerkName(_currentGoal.ResearchId)}"] = false;
                            Chat.WriteLine($"Turning off setting - {N3EngineClientAnarchy.GetPerkName(_currentGoal.ResearchId)}");
                            _completedResearchGoals.Add((uint)_currentGoal.ResearchId);

                            if (_researchGoalsActive.Contains(_currentGoal))
                            {
                                _researchGoalsActive.Add(_currentGoal);
                                Chat.WriteLine($"Removed Active - {N3EngineClientAnarchy.GetPerkName(_currentGoal.ResearchId)}");
                            }

                            foreach (ResearchGoal _goalNew in _researchGoalsActiveList.Where(c => !_completedResearchGoals.Contains((uint)c.ResearchId)).Take(1))
                            {
                                if (_researchGoalsActive.Count >= 1)
                                {
                                    Research.Train(_goalNew.ResearchId);
                                    Chat.WriteLine($"Starting - {N3EngineClientAnarchy.GetPerkName(_goalNew.ResearchId)}");
                                    return;
                                }
                            }
                        }
                    }
                }

                _timerActive = Time.NormalTime;
            }
        }

        //public static void InitAddResearch()
        //{
        //    foreach (int _goal in Research.Completed)
        //    {
        //        string _perkName = N3EngineClientAnarchy.GetPerkName(_goal);

        //        if (_settings[$"{_perkName}"].AsBool())
        //        {
        //            if (!_researchGoalsActiveStr.Contains(_perkName))
        //                _researchGoalsActiveStr.Add(_perkName);
        //        }
        //    }
        //}

        //public static void InitRemoveResearch()
        //{
        //    //foreach (int _goal in Research.Completed)
        //    //{
        //    //    string _perkName = N3EngineClientAnarchy.GetPerkName(_goal);

        //    //    if (_settings[$"{_perkName}"].AsBool())
        //    //    {
        //    //        if (_researchGoalsActiveStr.Contains(_perkName))
        //    //            _researchGoalsActiveStr.Remove(_perkName);
        //    //    }
        //    //}

        //    //foreach (ResearchGoal _goal in Research.Goals.Where(c => c.Available))
        //    //{
        //    //    if (_settings[$"{N3EngineClientAnarchy.GetPerkName(_goal.ResearchId)}"].AsBool())
        //    //    {
        //    //        if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_goal.ResearchId)))
        //    //            _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_goal.ResearchId));
        //    //    }
        //    //}


        //    //if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(_researchGoals[0].ResearchId)}"].AsBool())
        //    //{
        //    //    if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_researchGoals[0].ResearchId)))
        //    //        _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_researchGoals[0].ResearchId));
        //    //}
        //    //if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(_researchGoals[1].ResearchId)}"].AsBool())
        //    //{
        //    //    if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_researchGoals[1].ResearchId)))
        //    //        _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_researchGoals[1].ResearchId));
        //    //}
        //    //if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(_researchGoals[2].ResearchId)}"].AsBool())
        //    //{
        //    //    if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_researchGoals[2].ResearchId)))
        //    //        _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_researchGoals[2].ResearchId));
        //    //}
        //    //if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(_researchGoals[3].ResearchId)}"].AsBool())
        //    //{
        //    //    if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_researchGoals[3].ResearchId)))
        //    //        _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_researchGoals[3].ResearchId));
        //    //}
        //    //if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(_researchGoals[4].ResearchId)}"].AsBool())
        //    //{
        //    //    if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_researchGoals[4].ResearchId)))
        //    //        _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_researchGoals[4].ResearchId));
        //    //}
        //    //if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(_researchGoals[5].ResearchId)}"].AsBool())
        //    //{
        //    //    if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_researchGoals[5].ResearchId)))
        //    //        _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_researchGoals[5].ResearchId));
        //    //}
        //    //if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(_researchGoals[6].ResearchId)}"].AsBool())
        //    //{
        //    //    if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_researchGoals[6].ResearchId)))
        //    //        _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_researchGoals[6].ResearchId));
        //    //}
        //    //if (!_settings[$"{N3EngineClientAnarchy.GetPerkName(_researchGoals[7].ResearchId)}"].AsBool())
        //    //{
        //    //    if (_researchGoalsActiveStr.Contains(N3EngineClientAnarchy.GetPerkName(_researchGoals[7].ResearchId)))
        //    //        _researchGoalsActiveStr.Remove(N3EngineClientAnarchy.GetPerkName(_researchGoals[7].ResearchId));
        //    //}
        //}
    }
}
