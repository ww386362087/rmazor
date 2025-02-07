﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders.Camera_Effects_Props;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Network;
using mazing.common.Runtime.Network.DataFieldFilters;
using mazing.common.Runtime.Utils;
using Newtonsoft.Json;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using UnityEngine.Events;

namespace RMAZOR.Managers
{
    public interface IRemotePropertiesInfoProvider : IInit
    {
        List<RemoteConfigPropertyInfo> GetInfos();
    }
    
    public class RemotePropertiesInfoProvider : InitBase, IRemotePropertiesInfoProvider 
    {
        #region constants

        private const string IdFirstLevelToShowAds                 = "ads_first_level_to_show_ads";
        private const string IdAnimatePathFill                     = "animate_path_fill";
        private const string IdBackgroundTextureTriangles2PropsSet = "background_texture_triangles2_props_set";
        private const string IdColorGradingProps                   = "color_grading_props_1";
        private const string IdFirstLevelToRateGame                = "first_level_to_rate_game";
        private const string IdInAppNotificationList               = "inapp_notifications_list_v3";
        private const string IdInterstitialAdsRatio                = "interstitial_ads_ratio";
        private const string IdPayToContinueMoneyCount             = "pay_to_continue_money_count";
        private const string IdSkipButtonDelay                     = "skip_button_seconds";
        private const string IdTestDeviceIds                       = "test_device_ids";
        private const string IdMoneyItemsFillRate                  = "money_items_fill_rate";
        private const string IdBackgroundTextures                  = "background_textures_v2";
        private const string IdBetweenLevelAdShowIntervalInSeconds = "between_level_ad_show_interval_in_seconds";
        private const string IdShowOnlyRewardedAds                 = "show_only_rewarded_ads";
        private const string IdDrawAdditionalMazeNet               = "draw_additional_maze_net";
        private const string IdMazeItemBlockColorEqualsMainColor   = "maze_item_block_color_equals_main_color";
        private const string IdBetweenLevelsTransitionTextureName  = "between_levels_transition_texture_name";
        private const string IdSpecialOfferDurationInMinutes       = "special_offer_duration_in_minutes";
        private const string IdGameServerUrl                       = "game_server_url";
        private const string IdBundlesUrl                          = "bundles_url";

        #endregion
        
        #region inject
        
        private IGameClient             GameClient         { get; }
        private GlobalGameSettings      GlobalGameSettings { get; }
        private ModelSettings           ModelSettings      { get; }
        private ViewSettings            ViewSettings       { get; }
        private IRemotePropertiesRmazor RemoteProperties   { get; }

        public RemotePropertiesInfoProvider(
            IGameClient             _GameClient,
            GlobalGameSettings      _GlobalGameSettings,
            ModelSettings           _ModelSettings,
            ViewSettings            _ViewSettings,
            IRemotePropertiesRmazor _RemoteProperties)
        {
            GameClient         = _GameClient;
            GlobalGameSettings = _GlobalGameSettings;
            ModelSettings      = _ModelSettings;
            ViewSettings       = _ViewSettings;
            RemoteProperties   = _RemoteProperties;
        }

        #endregion

        #region api

        public List<RemoteConfigPropertyInfo> GetInfos()
        {
            var filter = GetFilter();
            return GetViewSettingsInfos(filter)
                .Concat(GetGlobalSettingsInfos(filter))
                .Concat(GetRemotePropertiesInfos(filter)) // FIXME исправить нерабочую десериализацию
                .Concat(new[]
                {
                    new RemoteConfigPropertyInfo(filter, typeof(string), IdTestDeviceIds,
                        _Value => Execute(
                            _Value, _V => { }),
                        true),
                }).ToList();
        }

        #endregion

        #region nonpublic methods

        private IEnumerable<RemoteConfigPropertyInfo> GetViewSettingsInfos(GameDataFieldFilter _Filter)
        {
            return new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(_Filter, typeof(bool), IdAnimatePathFill,
                    _Value => Execute(
                        _Value, _V => ViewSettings.animatePathFill = ToBool(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdSkipButtonDelay,
                    _Value => Execute(
                        _Value, _V => ViewSettings.skipLevelSeconds = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdFirstLevelToRateGame,
                    _Value => Execute(
                        _Value, _V => ViewSettings.firstLevelToRateGame = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdBackgroundTextures,
                    _Value => Execute(
                        _Value, _V =>
                        {
                            ViewSettings.BackgroundTextures = 
                                JsonConvert.DeserializeObject<List<string>>(ToString(_V));
                        })),
                new RemoteConfigPropertyInfo(_Filter, typeof(bool), IdDrawAdditionalMazeNet,
                    _Value => Execute(
                        _Value, _V => ViewSettings.drawAdditionalMazeNet = ToBool(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(bool), IdMazeItemBlockColorEqualsMainColor,
                    _Value => Execute(
                        _Value, _V => ViewSettings.mazeItemBlockColorEqualsMainColor = ToBool(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdBetweenLevelsTransitionTextureName,
                    _Value => Execute(
                        _Value, _V => ViewSettings.betweenLevelsTransitionTextureName = ToString(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdSpecialOfferDurationInMinutes,
                    _Value => Execute(
                        _Value, _V => ViewSettings.specialOfferDurationInMinutes = ToFloat(_V))),
            };
        }

        private IEnumerable<RemoteConfigPropertyInfo> GetGlobalSettingsInfos(GameDataFieldFilter _Filter)
        {
            return new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(_Filter, typeof(int), IdFirstLevelToShowAds,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.firstLevelToShowAds = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(long), IdPayToContinueMoneyCount,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.payToContinueMoneyCount = ToInt(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdInterstitialAdsRatio,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.interstitialAdsRatio = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdMoneyItemsFillRate,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.moneyItemsRate = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(float), IdBetweenLevelAdShowIntervalInSeconds,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.betweenLevelAdShowIntervalInSeconds = ToFloat(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(bool), IdShowOnlyRewardedAds,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.showOnlyRewardedAds = ToBool(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdGameServerUrl,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.urlGameServer = ToString(_V))),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdBundlesUrl,
                    _Value => Execute(
                        _Value, _V => GlobalGameSettings.urlBundles = ToString(_V))),
            };
        }

        private IEnumerable<RemoteConfigPropertyInfo> GetRemotePropertiesInfos(GameDataFieldFilter _Filter)
        {
            return new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdInAppNotificationList,
                    _Value => Execute(
                            _Value, _V => RemoteProperties.Notifications =
                                JsonConvert.DeserializeObject<List<NotificationInfoEx>>(_V.ToString()))
                    ),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdBackgroundTextureTriangles2PropsSet,
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.Tria2TextureSet =
                            JsonConvert.DeserializeObject<List<Triangles2TextureProps>>(Convert.ToString(_V));
                    }),
                    true),
                new RemoteConfigPropertyInfo(_Filter, typeof(string), IdColorGradingProps,
                    _Value => Execute(
                        _Value, _V =>
                    {
                        RemoteProperties.ColorGradingProps =
                            JsonConvert.DeserializeObject<ColorGradingProps>(Convert.ToString(_V));
                    }), true),
            };
        }
        
        private GameDataFieldFilter GetFilter()
        {
            var fieldInfos = GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Static);
            var fieldIds = fieldInfos
                .Select(_Fi => _Fi.GetValue(this))
                .Where(_V => _V is string)
                .Cast<string>()
                .Select(MazorCommonUtils.StringToHash)
                .Select(_Id => (ushort) _Id)
                .ToArray();
            return new GameDataFieldFilter(
                    GameClient, 
                    GameClientUtils.AccountId, 
                    CommonData.GameId,
                    fieldIds) 
                {OnlyLocal = true};
        }
        
        private static void Execute(object _Value, UnityAction<object> _Action)
        {
            try
            {
                if (_Value == null)
                    return;
                _Action?.Invoke(_Value);
            }
            catch (Exception ex)
            {
                Dbg.LogError(ex);                
            }
        }

        private static float ToFloat(object _Value)
        {
            return Convert.ToSingle(_Value);
        }

        private static int ToInt(object _Value)
        {
            return Convert.ToInt32(_Value);
        }

        private static string ToString(object _Value)
        {
            return Convert.ToString(_Value);
        }

        private static bool ToBool(object _Value)
        {
            return Convert.ToBoolean(_Value);
        }
        
        #endregion
    }
}