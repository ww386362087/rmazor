﻿using GameHelpers;
using Managers.Advertising;
using Managers.Audio;
using Managers.IAP;
using Zenject;

namespace Managers
{
    public delegate void SoundManagerHandler        (IAudioManager Manager);
    public delegate void AdsManagerHandler          (IAdsManager Manager);
    public delegate void AnalyticsManagerHandler    (IAnalyticsManager Manager);
    public delegate void ShopManagerHandler         (IShopManager Manager);
    public delegate void LocalizationManagerHandler (ILocalizationManager Manager);
    public delegate void ScoreManagerHandler        (IScoreManager Manager);
    public delegate void HapticsManagerHandler      (IHapticsManager Manager);
    public delegate void PrefabSetManagerHandler    (IPrefabSetManager Manager);
    public delegate void AssetBundleManagerHandler  (IAssetBundleManager Manager);
    
    public interface IManagersGetter
    {
        IAudioManager        AudioManager        { get; }
        IAnalyticsManager    AnalyticsManager    { get; }
        IAdsManager          AdsManager          { get; } 
        IShopManager         ShopManager         { get; }
        ILocalizationManager LocalizationManager { get; }
        IScoreManager        ScoreManager        { get; }
        IHapticsManager      HapticsManager      { get; }
        IPrefabSetManager    PrefabSetManager    { get; }
        IAssetBundleManager  AssetBundleManager  { get; }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        IDebugManager        DebugManager { get; }
#endif
        void Notify(
            SoundManagerHandler        _OnSoundManager        = null,
            AnalyticsManagerHandler    _OnAnalyticsManager    = null,
            AdsManagerHandler          _OnAdsManager          = null,
            ShopManagerHandler         _OnShopManager         = null,
            LocalizationManagerHandler _OnLocalizationManager = null,
            ScoreManagerHandler        _OnScoreManager        = null,
            HapticsManagerHandler      _OnHapticsManager      = null,
            PrefabSetManagerHandler    _OnPrefabSetManager    = null,
            AssetBundleManagerHandler  _OnAssetBundleManager = null);
    }

    public class ManagersGetter : IManagersGetter
    {
        #region inject
        
        public IAudioManager        AudioManager        { get; }
        public IAnalyticsManager    AnalyticsManager    { get; }
        public IAdsManager          AdsManager          { get; } 
        public IShopManager         ShopManager         { get; }
        public ILocalizationManager LocalizationManager { get; }
        public IScoreManager        ScoreManager        { get; }
        public IHapticsManager      HapticsManager      { get; }
        public IPrefabSetManager    PrefabSetManager    { get; }
        public IAssetBundleManager  AssetBundleManager  { get; }

        public ManagersGetter(
            IAudioManager        _AudioManager,
            IAnalyticsManager    _AnalyticsManager,
            IAdsManager          _AdsManager,
            IShopManager         _ShopManager,
            ILocalizationManager _LocalizationManager,
            IScoreManager        _ScoreManager,
            IHapticsManager      _HapticsManager,
            IPrefabSetManager    _PrefabSetManager,
            IAssetBundleManager  _AssetBundleManager)
        {
            AudioManager        = _AudioManager;
            AnalyticsManager    = _AnalyticsManager;
            AdsManager          = _AdsManager;
            ShopManager         = _ShopManager;
            LocalizationManager = _LocalizationManager;
            ScoreManager        = _ScoreManager;
            HapticsManager      = _HapticsManager;
            PrefabSetManager    = _PrefabSetManager;
            AssetBundleManager  = _AssetBundleManager;
        }
        
        #endregion

        #region api
        
#if UNITY_EDITOR || DEVELOPMENT_BUILD

        [Inject] public IDebugManager DebugManager { get; }
#endif

        public void Notify(
            SoundManagerHandler           _OnSoundManager        = null,
            AnalyticsManagerHandler       _OnAnalyticsManager    = null,
            AdsManagerHandler             _OnAdsManager          = null,
            ShopManagerHandler            _OnShopManager         = null,
            LocalizationManagerHandler    _OnLocalizationManager = null,
            ScoreManagerHandler           _OnScoreManager        = null,
            HapticsManagerHandler         _OnHapticsManager      = null,
            PrefabSetManagerHandler       _OnPrefabSetManager    = null,
            AssetBundleManagerHandler     _OnAssetBundleManager  = null)
        {
            _OnSoundManager         ?.Invoke(AudioManager);
            _OnAnalyticsManager     ?.Invoke(AnalyticsManager);
            _OnAdsManager           ?.Invoke(AdsManager);
            _OnShopManager          ?.Invoke(ShopManager);
            _OnLocalizationManager  ?.Invoke(LocalizationManager);
            _OnScoreManager         ?.Invoke(ScoreManager);
            _OnHapticsManager       ?.Invoke(HapticsManager);
            _OnPrefabSetManager     ?.Invoke(PrefabSetManager);
            _OnAssetBundleManager   ?.Invoke(AssetBundleManager);
        }

        #endregion
    }
}