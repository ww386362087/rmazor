﻿using System;
using System.Collections.Generic;
using Common;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Analytics;
using Common.Managers.PlatformGameServices;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Debugging;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.InputConfigurators;

namespace RMAZOR.DebugConsole
{
    public delegate void LogChangedHandler(string[] _Log);
    
    public delegate void CommandHandler(string[] _Args);

    public class DebugCommandArgs
    {
        public string         Command     { get; }
        public CommandHandler Handler     { get; }
        public string         Description { get; }

        public DebugCommandArgs(string _Command, CommandHandler _Handler, string _Description)
        {
            Command = _Command;
            Handler = _Handler;
            Description = _Description;
        }
    }
    
    public interface IDebugConsoleController
    {
        event LogChangedHandler              OnLogChanged;
        IViewInputCommandsProceeder          CommandsProceeder { get; }
        IAdsManager                          AdsManager        { get; }
        IScoreManager                        ScoreManager      { get; }
        IAudioManager                        AudioManager      { get; }
        IAnalyticsManager                    AnalyticsManager  { get; }
        IModelGame                           Model             { get; }
        IFpsCounter                          FpsCounter        { get; }
        Queue<string>                        Scrollback        { get; }
        string[]                             Log               { get; }
        Dictionary<string, DebugCommandArgs> Commands          { get; }
        List<string>                         CommandHistory    { get; }
        
        void Init(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IAdsManager                 _AdsManager, 
            IScoreManager               _ScoreManager,
            IAudioManager               _AudioManager,
            IAnalyticsManager           _AnalyticsManager,
            IFpsCounter                 _FpsCounter);
        void RegisterCommand(DebugCommandArgs _DebugCommandArgs);
        void RunCommandString(string          _CommandString);
        void AppendLogLine(string             _Line);
        void RaiseLogChangedEvent(string[]    _Args);
    }
    
    public class DebugConsoleController : IDebugConsoleController
    {
        #region constants

        private const int ScrollbackSize = 100;

        #endregion

        #region constructor

        public DebugConsoleController()
        {
            DebugConsoleCommands.Controller = this;
            DebugConsoleCommands.RegisterCommands();
        }

        #endregion

        #region api
        
        public event LogChangedHandler     OnLogChanged;
        public IModelGame                  Model             { get; private set; }
        public IViewInputCommandsProceeder CommandsProceeder { get; private set; }
        public IAdsManager                 AdsManager        { get; private set; }
        public IScoreManager               ScoreManager      { get; private set; }
        public IAudioManager               AudioManager      { get; private set; }
        public IAnalyticsManager           AnalyticsManager  { get; private set; }
        public IFpsCounter                 FpsCounter        { get; private set; }

        public string[]      Log        { get; private set; }
        public Queue<string> Scrollback { get; } = new Queue<string>(ScrollbackSize);
        
        public Dictionary<string, DebugCommandArgs> Commands { get; } = new Dictionary<string, DebugCommandArgs>();

        public List<string> CommandHistory { get; private set; }
        public void RaiseLogChangedEvent(string[] _Args)
        {
            OnLogChanged?.Invoke(_Args);
        }

        public void Init(
            IModelGame                  _Model,
            IViewInputCommandsProceeder _CommandsProceeder,
            IAdsManager                 _AdsManager,
            IScoreManager               _ScoreManager,
            IAudioManager               _AudioManager,
            IAnalyticsManager           _AnalyticsManager,
            IFpsCounter                 _FpsCounter)
        {
            Model             = _Model;
            CommandsProceeder = _CommandsProceeder;
            AdsManager        = _AdsManager;
            ScoreManager      = _ScoreManager;
            AudioManager      = _AudioManager;
            AnalyticsManager  = _AnalyticsManager;
            FpsCounter        = _FpsCounter;
            LoadCommandsHistory();
        }

        public void RegisterCommand(DebugCommandArgs _DebugCommandArgs)
        {
            Commands.Add(_DebugCommandArgs.Command, _DebugCommandArgs);
        }
        
        public void AppendLogLine(string _Line)
        {
            Dbg.Log(_Line);
            if (Scrollback.Count >= ScrollbackSize)
                Scrollback.Dequeue();
            Scrollback.Enqueue(_Line);
            Log = Scrollback.ToArray();
            OnLogChanged?.Invoke(Log);
        }
        
        public void RunCommandString(string _CommandString)
        {
            AppendLogLine("$ " + _CommandString);
            string[] commandSplit = ParseArguments(_CommandString);
            string[] args = new string[0];
            if (commandSplit.Length <= 0)
                return;
            if (commandSplit.Length >= 2)
            {
                int numArgs = commandSplit.Length - 1;
                args = new string[numArgs];
                Array.Copy(
                    commandSplit,
                    1, 
                    args,
                    0,
                    numArgs);
            }
            RunCommand(commandSplit[0].ToLower(), args);
            SaveCommandToHistory(_CommandString);
        }

        #endregion

        #region nonpublic methods
        
        private void RunCommand(string _Command, string[] _Args)
        {
            if (!Commands.TryGetValue(_Command, out var reg))
                AppendLogLine($"Unknown command '{_Command}', type 'help' for list.");
            else
            {
                if (reg.Handler == null)
                    AppendLogLine($"Unable to process command '{_Command}', handler was null.");
                else
                    reg.Handler(_Args);
            }
        }

        private static string[] ParseArguments(string _CommandString)
        {
            var parmChars = new LinkedList<char>(_CommandString.ToCharArray());
            bool inQuote = false;
            var node = parmChars.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.Value == '"')
                {
                    inQuote = !inQuote;
                    parmChars.Remove(node);
                }
                if (!inQuote && node.Value == ' ')
                {
                    node.Value = ' ';
                }
                node = next;
            }
            char[] parmCharsArr = new char[parmChars.Count];
            parmChars.CopyTo(parmCharsArr, 0);
            return new string(parmCharsArr).Split(
                new[] { ' ' },
                StringSplitOptions.RemoveEmptyEntries);
        }

        private void SaveCommandToHistory(string _Command)
        {
            CommandHistory.Add(_Command);
            SaveUtils.PutValue(SaveKeysMazor.DebugConsoleCommandsHistory, CommandHistory);
            
        }

        private void LoadCommandsHistory()
        {
            CommandHistory = SaveUtils.GetValue(SaveKeysMazor.DebugConsoleCommandsHistory) ?? new List<string>();
        }

        #endregion
    }
}