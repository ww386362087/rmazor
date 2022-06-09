﻿using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Managers.Advertising;
using Common.Managers.Analytics;
using Common.Managers.IAP;
using Common.Managers.Notifications;
using Common.Managers.Scores;
using Common.Network;
using Common.Settings;
using Common.Ticker;
using RMAZOR;
using RMAZOR.Helpers;
using RMAZOR.Managers;
using RMAZOR.Settings;
using UnityEngine;
using Zenject;

namespace Mono_Installers
{
    public class GlobalMonoInstaller : MonoInstaller
    {
        public GameObject         cameraProvider;
        public GameObject         companyLogo;
        public CommonGameSettings commonGameSettings;
        public ModelSettings      modelSettings;
        public ViewSettings       viewSettings;
        
        public override void InstallBindings()
        {
            Container.BindInstance(new RemoteProperties());
            Container.Bind<CommonGameSettings>()  .FromScriptableObject(commonGameSettings) .AsSingle();
            Container.Bind<ModelSettings>()       .FromScriptableObject(modelSettings)      .AsSingle();
            Container.Bind<ViewSettings>()        .FromScriptableObject(viewSettings)       .AsSingle();
            Container.Bind<ICameraProvider>()     .FromComponentInNewPrefab(cameraProvider) .AsSingle();
            Container.Bind<CompanyLogo>()         .FromComponentInNewPrefab(companyLogo)    .AsSingle();

            #region settings

            Container.Bind<ISettingsGetter>()     .To<SettingsGetter>()                     .AsSingle();
            Container.Bind<ISoundSetting>()       .To<SoundSetting>()                       .AsSingle();
            Container.Bind<IMusicSetting>()       .To<MusicSetting>()                       .AsSingle();
            Container.Bind<INotificationSetting>().To<NotificationsSetting>()               .AsSingle();
            Container.Bind<IHapticsSetting>()     .To<HapticsSetting>()                     .AsSingle();
            Container.Bind<ILanguageSetting>()    .To<LanguageSetting>()                    .AsSingle();
            Container.Bind<IDebugSetting>()       .To<DebugSetting>()                       .AsSingle();
            
            #endregion

            #region managers

            
            Container.Bind<IRemotePropertiesInfoProvider>().To<RemotePropertiesInfoProvider>().AsSingle();
            Container.Bind<IUnityAnalyticsProvider>()  .To<UnityAnalyticsProvider>()        .AsSingle();
            Container.Bind<IFirebaseAnalyticsProvider>().To<FirebaseAnalyticsProvider>()    .AsSingle();
            Container.Bind<IGameClient>()              .To<GameClient>()                    .AsSingle();

            if (Application.isEditor)
            {
                Container.Bind<IRemoteConfigManager>() .To<RemoteConfigManagerFake>()       .AsSingle();
                Container.Bind<IAnalyticsManager>()    .To<AnalyticsManagerFake>()          .AsSingle();
                Container.Bind<IScoreManager>()        .To<ScoreManagerFake>()              .AsSingle();
                Container.Bind<IShopManager>()         .To<ShopManagerFake>()               .AsSingle();
                Container.Bind<IRemoteSavedGameProvider>().To<FakeRemoteSavedGameProvider>().AsSingle();
                Container.Bind<IPermissionsRequester>().To<FakePermissionsRequester>()      .AsSingle();
                Container.Bind<INotificationsManager>().To<NotificationsManagerFake>()      .AsSingle();
            }
            else
            {
                Container.Bind<IRemoteConfigManager>() .To<RemoteConfigManager>()           .AsSingle();
                Container.Bind<IRemoteConfigProvider>().To<FirebaseRemoteConfigProvider>()  .AsSingle();
#if UNITY_ANDROID
                Container.Bind<IAnalyticsManager>()    .To<AnalyticsManager>()              .AsSingle();
                Container.Bind<IPermissionsRequester>().To<FakePermissionsRequester>()      .AsSingle();
                Container.Bind<IScoreManager>()        .To<AndroidScoreManager>()           .AsSingle();
                Container.Bind<IShopManager>()         .To<AndroidUnityIAPShopManager>()    .AsSingle();
                Container.Bind<IRemoteSavedGameProvider>().To<FakeRemoteSavedGameProvider>().AsSingle();
                Container.Bind<INotificationsManager>().To<NotificationsManagerUnity>()     .AsSingle();
#elif UNITY_IOS || UNITY_IPHONE
                Container.Bind<IAnalyticsManager>()   .To<AnalyticsManager>()               .AsSingle();
                Container.Bind<IScoreManager>()       .To<IosScoreManager>()                .AsSingle();
                Container.Bind<IShopManager>()        .To<AppleUnityIAPShopManager>()       .AsSingle();
                Container.Bind<IRemoteSavedGameProvider>().To<FakeRemoteSavedGameProvider>().AsSingle();
                Container.Bind<IPermissionsRequester>().To<IosPermissionsRequester>()        .AsSingle();
                Container.Bind<INotificationsManager>().To<NotificationsManagerUnity>()     .AsSingle();
#endif
            }

            Container.Bind<ILocalizationManager>()      .To<LeanLocalizationManager>()      .AsSingle();
            
#if NICE_VIBRATIONS_3_9
            Container.Bind<IHapticsManager>()           .To<HapticsManagerNiceVibrations_3_9>().AsSingle();
#else
            Container.Bind<IHapticsManager>()           .To<HapticsManagerNiceVibrations_4_1>().AsSingle();
#endif
            Container.Bind<IAdsManager>()               .To<AdsManager>()    .AsSingle();
            Container.Bind<IPrefabSetManager>()         .To<PrefabSetManager>()             .AsSingle();
            Container.Bind<IAssetBundleManager>()       .To<AssetBundleManager>()           .AsSingle();
            // Container.Bind<IAssetBundleManager>()       .To<AssetBundleManagerFake>()           .AsSingle();

            #endregion
            
            Container.Bind<ICommonTicker>()             .To<CommonTicker>()                 .AsSingle();
            Container.Bind<IViewGameTicker>()           .To<ViewGameTicker>()               .AsSingle();
            Container.Bind<IModelGameTicker>()          .To<ModelGameTicker>()              .AsSingle();
            Container.Bind<IUITicker>()                 .To<UITicker>()                     .AsSingle();
            Container.Bind<ILevelsLoader>()             .To<LevelsLoader>()                 .AsSingle();
            Container.Bind<IMazeInfoValidator>()        .To<MazeInfoValidator>()            .AsSingle();

            Container.Bind<IAdMobAdsProvider>()         .To<AdMobAdsProvider>()             .AsSingle();
            Container.Bind<IAdMobInterstitialAd>()      .To<AdMobInterstitialAd>()          .AsSingle();
            Container.Bind<IAdMobRewardedAd>()          .To<AdMobRewardedAd>()              .AsSingle();
            
#if UNITY_ADS_API
            Container.Bind<IUnityAdsProvider>()         .To<UnityAdsProvider>()             .AsSingle();
            Container.Bind<IUnityAdsInterstitialAd>()   .To<UnityAdsInterstitialAd>()       .AsSingle();
            Container.Bind<IUnityAdsRewardedAd>()       .To<UnityAdsRewardedAd>()           .AsSingle();
#endif

            Container.Bind<IAppodealAdsProvider>()      .To<AppodealAdsProvider>()          .AsSingle();
            Container.Bind<IAppodealInterstitialAd>()   .To<AppodealInterstitialAd>()       .AsSingle();
            Container.Bind<IAppodealRewardedAd>()       .To<AppodealRewardedAd>()           .AsSingle();

            Container.Bind<IFontProvider>().To<DefaultFontProvider>().AsSingle();
        }
    }
}