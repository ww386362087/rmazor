﻿#if UNITY_ANDROID
using Common.Constants;
using Common.Entities;
using Common.Helpers;
using Common.Managers.PlatformGameServices.GameServiceAuth;
using Common.Network;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace Common.Managers.PlatformGameServices.Leaderboards
{
    public class LeaderboardProviderGooglePlayGames : LeaderboardProviderBase
    {
        #region nonpublic members

        #endregion

        #region inject
        
        public LeaderboardProviderGooglePlayGames(
            CommonGameSettings                _Settings,
            ILocalizationManager              _LocalizationManager,
            IGameClient                       _GameClient,
            IPlatformGameServiceAuthenticator _Authenticator) 
            : base(
                _Settings,
                _LocalizationManager, 
                _GameClient, 
                _Authenticator) { }

        #endregion

        #region api

        #endregion

        #region nonpublic methods

        private ScoresEntity GetScoreAndroid(ushort _Key)
        {
            var scoreEntity = new ScoresEntity();
            PlayGamesPlatform.Instance.LoadScores(
                GetScoreId(_Key),
                LeaderboardStart.PlayerCentered,
                1,
                LeaderboardCollection.Public,
                LeaderboardTimeSpan.AllTime,
                _Data =>
                {
                    if (_Data.Valid)
                    {
                        if (_Data.Status == ResponseStatus.Success)
                        {
                            if (_Data.PlayerScore != null)
                            {
                                scoreEntity.Value.Add(_Key, _Data.PlayerScore.value);
                                scoreEntity.Result = EEntityResult.Success;
                            }
                            else
                            {
                                Dbg.LogWarning("Remote score data PlayerScore is null");
                                scoreEntity = GetScoreCached(_Key, scoreEntity);
                            }
                        }
                        else
                        {
                            Dbg.LogWarning($"Remote score data status: {_Data.Status}");
                            scoreEntity = GetScoreCached(_Key, scoreEntity);
                        }
                    }
                    else
                    {
                        Dbg.LogWarning("Remote score data is not valid.");
                        scoreEntity = GetScoreCached(_Key, scoreEntity);
                    }
                });
            return scoreEntity;
        }
        
        private void SetScoreAndroid(ushort _Key, long _Value)
        {
            if (!Authenticator.IsAuthenticated)
            {
                Dbg.LogWarning($"{nameof(SetScoreAndroid)}: User is not authenticated to ");
                return;
            }
            Dbg.Log(nameof(SetScoreAndroid));
            PlayGamesPlatform.Instance.ReportScore(
                _Value,
                GetScoreId(_Key),
                _Success =>
                {
                    if (!_Success)
                        Dbg.LogWarning("Failed to post leaderboard score");
                    else Dbg.Log($"Successfully put score {_Value} to leaderboard {DataFieldIds.GetDataFieldName(_Key)}");
                });
        }
        
        private void ShowLeaderboardAndroid(ushort _Key)
        {
            string id = GetScoreId(_Key);
            PlayGamesPlatform.Instance.ShowLeaderboardUI(id);
        }
        
        #endregion
    }
}
#endif