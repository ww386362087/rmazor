﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Entities;
using Common.Helpers;
using Common.Network;
using Common.Network.DataFieldFilters;
using Common.Utils;
using Newtonsoft.Json;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;

namespace RMAZOR.Managers
{
    public interface IRemotePropertiesInfoProvider : IInit
    {
        GameDataFieldFilter      GetFilter();
        List<RemoteConfigPropertyInfo> GetInfos();
    }
    
    public class RemotePropertiesInfoProvider : InitBase, IRemotePropertiesInfoProvider 
    {
        private IGameClient        GameClient         { get; }
        private CommonGameSettings CommonGameSettings { get; }
        private ModelSettings      ModelSettings      { get; }
        private ViewSettings       ViewSettings       { get; }
        private RemoteProperties   RemoteProperties   { get; }

        public RemotePropertiesInfoProvider(
            IGameClient        _GameClient,
            CommonGameSettings _CommonGameSettings,
            ModelSettings      _ModelSettings,
            ViewSettings       _ViewSettings,
            RemoteProperties   _RemoteProperties)
        {
            GameClient         = _GameClient;
            CommonGameSettings = _CommonGameSettings;
            ModelSettings      = _ModelSettings;
            ViewSettings       = _ViewSettings;
            RemoteProperties   = _RemoteProperties;
        }

        public GameDataFieldFilter GetFilter()
        {
            var fieldIds = new []
            {
                CommonUtils.StringToHash("ads_first_level_to_show_ads"),
                CommonUtils.StringToHash("ads_providers_info"),
                CommonUtils.StringToHash("ads_show_ad_every_level"),
                CommonUtils.StringToHash("ads_show_rewarded_instead_of_interstitial_on_unpause"),
                CommonUtils.StringToHash("character_speed"),
                CommonUtils.StringToHash("additional_color_props_set"),
                CommonUtils.StringToHash("background_texture_triangles2_props_set"),
                CommonUtils.StringToHash("first_level_to_rate_game"),
                CommonUtils.StringToHash("main_color_props_set"),
                CommonUtils.StringToHash("maze_item_transition_coefficient"),
                CommonUtils.StringToHash("maze_item_transition_time"),
                CommonUtils.StringToHash("money_item_coast"),
                CommonUtils.StringToHash("pay_to_continue_money_count"),
                CommonUtils.StringToHash("mazeitems_gravityblock_speed"),
                CommonUtils.StringToHash("mazeitems_movingtrap_speed"),
                CommonUtils.StringToHash("test_device_ids"),
            }.Select(_Id => (ushort) _Id)
                .ToArray();
            return new GameDataFieldFilter(
                    GameClient, 
                    GameClientUtils.AccountId, 
                    CommonGameSettings.gameId,
                    fieldIds) 
                {OnlyLocal = true};
        }

        public List<RemoteConfigPropertyInfo> GetInfos()
        {
            var filter = GetFilter();
            var infos = new List<RemoteConfigPropertyInfo>
            {
                new RemoteConfigPropertyInfo(
                    "ads_first_level_to_show_ads",  
                    typeof(int), 
                    filter,
                    _Value => CommonGameSettings.firstLevelToShowAds = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(
                    "ads_providers_info",    
                    typeof(string), 
                    filter,
                    _Value => CommonGameSettings.adsProviders = Convert.ToString(_Value)),
                new RemoteConfigPropertyInfo(
                    "ads_show_ad_every_level",  
                    typeof(int), 
                    filter,
                    _Value => CommonGameSettings.showAdsEveryLevel = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(
                    "ads_show_rewarded_on_level_pass",
                    typeof(bool),
                    filter,
                    _Value => CommonGameSettings.showRewardedOnLevelPass = Convert.ToBoolean(_Value)),
                new RemoteConfigPropertyInfo(
                    "character_speed", 
                    typeof(float),
                    filter,
                    _Value => ModelSettings.characterSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(
                    "additional_color_props_set", 
                    typeof(string), 
                    filter,
                    _Value =>
                    {
                        var converter = new ColorJsonConverter();
                        RemoteProperties.BackAndFrontColorsSet = JsonConvert.DeserializeObject<IList<AdditionalColorsProps>>(
                            Convert.ToString(_Value), converter);
                    },
                    true),
                new RemoteConfigPropertyInfo(
                    "background_texture_triangles2_props_set",
                    typeof(string),
                    filter,
                    _Value =>
                    {
                        RemoteProperties.Tria2TextureSet = 
                            JsonConvert.DeserializeObject<IList<Triangles2TextureProps>>(Convert.ToString(_Value));
                    },
                    true),
                new RemoteConfigPropertyInfo(
                    "first_level_to_rate_game",
                    typeof(int),
                    filter,
                    _Value => ViewSettings.firstLevelToRateGame = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(
                    "main_color_props_set", 
                    typeof(string), 
                    filter,
                    _Value =>
                    {
                        var converter = new ColorJsonConverter();
                        RemoteProperties.MainColorsSet = JsonConvert.DeserializeObject<IList<MainColorsProps>>(
                            Convert.ToString(_Value), converter);  
                    },
                    true),
                new RemoteConfigPropertyInfo(
                    "maze_item_transition_coefficient",
                    typeof(float), 
                    filter,
                    _Value => ViewSettings.mazeItemTransitionDelayCoefficient = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(
                    "maze_item_transition_time",   
                    typeof(float),
                    filter,
                    _Value => ViewSettings.mazeItemTransitionTime = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(
                    "money_item_coast",  
                    typeof(long), 
                    filter,
                    _Value => CommonGameSettings.moneyItemCoast = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(
                    "pay_to_continue_money_count", 
                    typeof(long), 
                    filter,
                    _Value => CommonGameSettings.payToContinueMoneyCount = Convert.ToInt32(_Value)),
                new RemoteConfigPropertyInfo(
                    "mazeitems_gravityblock_speed", 
                    typeof(float),
                    filter,
                    _Value => ModelSettings.gravityBlockSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(
                    "mazeitems_movingtrap_speed", 
                    typeof(float),
                    filter,
                    _Value => ModelSettings.movingItemsSpeed = Convert.ToSingle(_Value)),
                new RemoteConfigPropertyInfo(
                    "test_device_ids", 
                    typeof(string),
                    filter, 
                    _Value => { },
                    true),
            };
            return infos;
        }
    }
}