﻿using AppodealAds.Unity.Api;
using Common.Helpers;
using Common.Ticker;
using UnityEngine.Events;

namespace Common.Managers.Advertising
{
    public abstract class AppodealAdBase : IAdBase, IUpdateTick
    {
        private const string DefaultPlacement = "default";
        
        protected abstract int AdType { get; }
        
        protected          bool        DoLoadAdWithDelay;
        protected volatile bool        m_DoInvokeOnShown;
        private            float       m_LoadAdDelayTimer;
        private            UnityAction m_OnShown;
        
        private CommonGameSettings Settings     { get; }
        private ICommonTicker      CommonTicker { get; }

        protected AppodealAdBase(CommonGameSettings _Settings, ICommonTicker _CommonTicker)
        {
            Settings = _Settings;
            CommonTicker = _CommonTicker;
        }
        
        public bool Ready => Appodeal.isLoaded(AdType)
                             && Appodeal.canShow(AdType, DefaultPlacement);
        
        public virtual void Init(string _AppId, string _UnitId)
        {
            CommonTicker.Register(this);
            Appodeal.setAutoCache(AdType, false);
            Appodeal.initialize(_AppId, AdType);
        }

        public void LoadAd()
        {
            Appodeal.cache(AdType);
        }

        public void ShowAd(UnityAction _OnShown)
        {
            if (Ready)
                Appodeal.show(AdType);
            else LoadAd();
        }

        public void UpdateTick()
        {
            if (m_DoInvokeOnShown)
            {
                m_OnShown?.Invoke();
                m_DoInvokeOnShown = false;
            }
            if (!DoLoadAdWithDelay)
                return;
            m_LoadAdDelayTimer += CommonTicker.DeltaTime;
            if (!(m_LoadAdDelayTimer > Settings.adsLoadDelay)) 
                return;
            LoadAd();
            m_LoadAdDelayTimer = 0f;
            DoLoadAdWithDelay = false;
        }
    }
}