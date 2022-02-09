﻿// ReSharper disable UnusedType.Global
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Common.Constants;
using Common.Entities;
using Common.Network;
using Common.Ticker;
using SA.iOS.Foundation;
using SA.iOS.GameKit;
using UnityEngine.Events;

namespace Common.Managers.Scores
{
    public class IosScoreManager : ScoreManagerBase
    {
        #region nonpublic members

        private EEntityResult         m_FetchSavedGamesResult = EEntityResult.Pending;
        private List<ISN_GKSavedGame> m_FetchedSavedGames = new List<ISN_GKSavedGame>();

        #endregion

        #region inject

        public IosScoreManager(
            IGameClient          _GameClient,
            ILocalizationManager _LocalizationManager,
            ICommonTicker        _Ticker) 
            : base(_GameClient, _LocalizationManager, _Ticker) { }

        #endregion

        #region api

        public override void Init()
        {
            ISN_GKLocalPlayerListener.DidModifySavedGame.AddListener(DidModifySavedGame);
            ISN_GKLocalPlayerListener.HasConflictingSavedGames.AddListener(HasConflictingSavedGames);
            base.Init();
        }

        public override ScoresEntity GetScoreFromLeaderboard(ushort _Id, bool _FromCache)
        {
            var entity = base.GetScoreFromLeaderboard(_Id, _FromCache);
            return entity ?? GetScoreIos(_Id);
        }

        public override bool SetScoreToLeaderboard(ushort _Id, long _Value, bool _OnlyToCache)
        {
            if (!base.SetScoreToLeaderboard(_Id, _Value, _OnlyToCache))
                SetScoreIos(_Id, _Value);
            return true;
        }

        public override bool ShowLeaderboard(ushort _Id)
        {
            if (!base.ShowLeaderboard(_Id))
                return false;
            ShowLeaderboardIos(_Id);
            return true;
        }

        public override void DeleteSavedGame(string _FileName)
        {
            if (m_FetchSavedGamesResult != EEntityResult.Success
            || m_FetchedSavedGames.All(_G => _G == null || _G.Name != _FileName))
            {
                Dbg.LogWarning("Failed to delete saved game");
                return;
            }
            var savedGame = m_FetchedSavedGames.First(_G => _G.Name == _FileName);
            ISN_GKLocalPlayer.DeleteSavedGame(savedGame, _Result =>
            {
                if (_Result.IsSucceeded)
                    Dbg.Log($"Saved game with file name {_FileName} deleted.");
                else
                    Dbg.LogWarning($"Failed to delete saved game with file name {_FileName}: {_Result.Error}");
            });
        }

        public override Entity<object> GetSavedGameProgress(string _FileName, bool _FromCache)
        {
            if (_FromCache)
                return GetSavedGameProgressFromCache(_FileName);
            var entity = new Entity<object> {Result = EEntityResult.Pending};
            var savedGame = m_FetchedSavedGames.FirstOrDefault(_G => _G.Name == _FileName);
            if (savedGame == null)
            {
                Dbg.LogWarning($"Saved game with {_FileName} does not exist in fetched saved games.");
                entity.Result = EEntityResult.Fail;
                return entity;
            }
            savedGame.Load(_Result =>
            {
                if (_Result.IsSucceeded)
                {
                    entity.Value = FromByteArray<object>(_Result.BytesArrayData);
                    entity.Result = EEntityResult.Success;
                }
                else
                {
                    Dbg.LogWarning($"Failed to load data from fetched saved game with file name {_FileName}");
                    entity.Result = EEntityResult.Fail;
                }
            });
            return entity;
        }

        public override void SaveGameProgress<T>(T _Data, bool _OnlyToCache)
        {
            SaveGameProgressToCache(_Data);
            if (_OnlyToCache)
                return;
            var data = ToByteArray(_Data);
            ISN_GKLocalPlayer.SavedGame(
                _Data.FileName,
                data,
                _Result =>
                {
                    if (_Result.IsSucceeded) 
                    {
                        if (!m_FetchedSavedGames.Any())
                            FetchSavedGames();
                        Dbg.Log($"Saved game name: {_Result.SavedGame.Name}");
                        Dbg.Log($"Saved game device name: {_Result.SavedGame.DeviceName}");
                        Dbg.Log($"Saved game modification date: {_Result.SavedGame.ModificationDate}");
                    } 
                    else
                    {
                        Dbg.LogError("SavedGame is failed! With:" +
                                     $" {_Result.Error.Code} and description: {_Result.Error.Message}");
                    }
                });
        }

        #endregion
        
        #region nonpublic methods
        
        private static void DidModifySavedGame(ISN_GKSavedGameSaveResult _Result) {
            Dbg.Log($"DidModifySavedGame! Device name = {_Result.SavedGame.DeviceName} " +
                      $"| game name = {_Result.SavedGame.Name} | modification Date = " +
                      $"{_Result.SavedGame.ModificationDate.ToString(CultureInfo.InvariantCulture)}");
        }
        
        private static void HasConflictingSavedGames(ISN_GKSavedGameFetchResult _Result)
        {
            foreach (var game in _Result.SavedGames)
            {
                Dbg.Log($"HasConflictingSavedGames! Device name = {game.DeviceName} " +
                        $"| game name = {game.Name} | modification Date = " +
                        $"{game.ModificationDate.ToString(CultureInfo.InvariantCulture)}");
            }
            // var conflictedGameIds = _Result.SavedGames.Select(game => game.Id).ToList();
            // ISN_GKLocalPlayer.ResolveConflictingSavedGames(conflictedGameIds, null, null);
        }
        
        protected override bool IsAuthenticatedInPlatformGameService()
        {
            return ISN_GKLocalPlayer.LocalPlayer.Authenticated;
        }

        protected override void AuthenticatePlatformGameService(UnityAction _OnFinish)
        {
            AuthenticateIos(_OnFinish);
        }

        private void AuthenticateIos(UnityAction _OnFinish)
        {
            ISN_GKLocalPlayer.SetAuthenticateHandler(_Result =>
            {
                if (_Result.IsSucceeded)
                {
                    var player = ISN_GKLocalPlayer.LocalPlayer;
                    var sb = new StringBuilder();
                    sb.AppendLine($"player game id: {player.GamePlayerId}");
                    sb.AppendLine($"player team id: {player.TeamPlayerId}");
                    sb.AppendLine($"player Alias: {player.Alias}");
                    sb.AppendLine($"player DisplayName: {player.DisplayName}");
                    sb.AppendLine($"player Authenticated: {player.Authenticated}");
                    sb.AppendLine($"player Underage: {player.Underage}");
                    Dbg.Log(sb.ToString());

                    player.GenerateIdentityVerificationSignatureWithCompletionHandler(_SignatureResult =>
                    {
                        if(_SignatureResult.IsSucceeded)
                        {
                            sb.Clear();
                            sb.AppendLine($"signatureResult.PublicKeyUrl: {_SignatureResult.PublicKeyUrl}");
                            sb.AppendLine($"signatureResult.Timestamp: {_SignatureResult.Timestamp}");
                            sb.AppendLine($"signatureResult.Salt.Length: {_SignatureResult.Salt.Length}");
                            sb.AppendLine($"signatureResult.Signature.Length: {_SignatureResult.Signature.Length}");
                            Dbg.Log(sb.ToString());
                            FetchSavedGames();
                        } 
                        else
                        {
                            m_FetchSavedGamesResult = EEntityResult.Fail;
                            Dbg.LogError($"IdentityVerificationSignature has failed: {_SignatureResult.Error.FullMessage}");
                        }
                        _OnFinish?.Invoke();
                    });
                }
                else 
                {
                    Dbg.LogError(AuthMessage(false, 
                        $"Error with code: {_Result.Error.Code} and description: {_Result.Error.Message}"));
                }
            });
        }

        private void FetchSavedGames()
        {
            ISN_GKLocalPlayer.FetchSavedGames(_Result => 
            {
                if (_Result.IsSucceeded) 
                {
                    m_FetchedSavedGames = _Result.SavedGames;
                    m_FetchSavedGamesResult = EEntityResult.Success;
                    Dbg.Log($"Loaded {_Result.SavedGames.Count} saved games");
                    var sb = new StringBuilder();
                    foreach (var game in _Result.SavedGames)
                    {
                        sb.Clear();
                        sb.AppendLine($"saved game name: {game.Name}");
                        sb.AppendLine($"saved game DeviceName: {game.DeviceName}");
                        sb.AppendLine($"saved game ModificationDate: {game.ModificationDate}");
                        Dbg.Log(sb.ToString());
                    }
                }
                else
                {
                    m_FetchSavedGamesResult = EEntityResult.Fail;
                    Dbg.LogError("Fetching saved games is failed! With:" +
                                 $" {_Result.Error.Code} and description: {_Result.Error.Message}");
                }
            });
        }
                
        private void SetScoreIos(ushort _Id, long _Value)
        {
            if (!IsAuthenticatedInPlatformGameService())
            {
                Dbg.LogWarning("User is not authenticated to Game Center");
                return;
            }
            var scoreReporter = new ISN_GKScore(GetScoreKey(_Id))
            {
                Value = _Value
            };
            scoreReporter.Report(_Result =>
            {
                if (_Result.IsSucceeded) 
                    Dbg.Log("Score Report Success");
                else
                {
                    Dbg.LogError("Score Report failed! Code: " + 
                                 _Result.Error.Code + " Message: " + _Result.Error.Message);
                }
            });
        }
        
        private ScoresEntity GetScoreIos(ushort _Id)
        {
            var scoreEntity = new ScoresEntity{Result = EEntityResult.Pending};
            var leaderboardRequest = new ISN_GKLeaderboard
            {
                Identifier = GetScoreKey(_Id),
                PlayerScope = ISN_GKLeaderboardPlayerScope.Global,
                TimeScope = ISN_GKLeaderboardTimeScope.AllTime,
                Range = new ISN_NSRange(1, 25)
            };
            leaderboardRequest.LoadScores(_Result => 
            {
                if (_Result.IsSucceeded) 
                {
                    Dbg.Log($"Score Load Succeed: {DataFieldIds.GetDataFieldName(_Id)}: {leaderboardRequest.LocalPlayerScore.Value}");
                    scoreEntity.Value.Add(_Id, leaderboardRequest.LocalPlayerScore.Value);
                    scoreEntity.Result = EEntityResult.Success;
                } 
                else
                {
                    scoreEntity = GetScoreCached(_Id, scoreEntity);
                    Dbg.LogWarning("Score Load failed! Code: " + _Result.Error.Code + " Message: " + _Result.Error.Message);
                }
            });
            return scoreEntity;
        }
        
        private void ShowLeaderboardIos(ushort _Id)
        {
            var viewController = new ISN_GKGameCenterViewController
            {
                ViewState = ISN_GKGameCenterViewControllerState.Leaderboards,
                LeaderboardIdentifier = GetScoreKey(_Id)
            };
            viewController.Show();
        }
        
        #endregion
    }
}