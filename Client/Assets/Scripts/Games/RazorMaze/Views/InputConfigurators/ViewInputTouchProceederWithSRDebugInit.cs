﻿using System.Collections.Generic;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Lean.Touch;
using Ticker;
using UnityEngine.Events;
using Utils;

namespace Games.RazorMaze.Views.InputConfigurators
{
    public interface IViewInputTouchProceederWithSRDebugInit : IViewInputTouchProceeder
    {
        UnityAction OnSrDebugInitialized { get; set; }
    }
    
    public class ViewInputTouchProceederWithSRDebugInit : ViewInputTouchProceeder
    {
        private readonly List<bool> m_SRDebugCurrentSequence = new List<bool>();
        private readonly bool[] m_SRDebugInitCombination = 
        {
            true, true, true,
            false, false,
            true,
            false, false, false, false,
            true, true, true
        };
        private bool m_SrDebuggerInitialized;
        
        public ViewInputTouchProceederWithSRDebugInit(
            CommonGameSettings _CommonGameSettings,
            ViewSettings _ViewSettings, 
            IModelGame _Model, 
            IContainersGetter _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewGameTicker _GameTicker, 
            ICameraProvider _CameraProvider,
            IPrefabSetManager _PrefabSetManager) 
            : base(
                _CommonGameSettings, 
                _ViewSettings, 
                _Model, 
                _ContainersGetter, 
                _CommandsProceeder,
                _GameTicker,
                _CameraProvider,
                _PrefabSetManager) { }
        
        
        public UnityAction OnSrDebugInitialized { get; set; }

        protected override void OnTapCore(LeanFinger _Finger)
        {
            base.OnTapCore(_Finger);
            ProceedSRDebugInitialization(_Finger);
        }
        
        private void ProceedSRDebugInitialization(LeanFinger _Finger)
        {
            if (CommonGameSettings.SrDebuggerOn)
                return;
            if (m_SrDebuggerInitialized)
                return;
            bool val = _Finger.LastScreenPosition.y / GraphicUtils.ScreenSize.y > 0.5f;
            m_SRDebugCurrentSequence.Add(val);
            for (int i = 0; i < m_SRDebugCurrentSequence.Count; i++)
            {
                if (m_SRDebugCurrentSequence[i] == m_SRDebugInitCombination[i]) 
                    continue;
                m_SRDebugCurrentSequence.Clear();
                return;
            }
            if (m_SRDebugCurrentSequence.Count != m_SRDebugInitCombination.Length) 
                return;
            
            CommonUtils.InitSRDebugger();
            OnSrDebugInitialized?.Invoke();
            m_SrDebuggerInitialized = true;
        }
    }
}