﻿using Common;
using Common.CameraProviders;
using Common.Helpers;
using Common.Managers;
using Common.Providers;
using UnityEngine;

namespace RMAZOR.Views.Common.FullscreenTextureProviders
{
    public abstract class FullscreenTransitionTextureProviderSimpleBase:
        FullscreenTransitionTextureProviderBase
    {
        #region nonpublic members

        protected static readonly int Color1Id          = Shader.PropertyToID("_Color1");
        protected static readonly int TransitionValueId = Shader.PropertyToID("_TransitionValue");
        
        #endregion

        #region inject

        protected FullscreenTransitionTextureProviderSimpleBase(
            IPrefabSetManager _PrefabSetManager, 
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IColorProvider    _ColorProvider) 
            : base(
                _PrefabSetManager,
                _ContainersGetter,
                _CameraProvider,
                _ColorProvider) { }

        #endregion
        
        #region api

        public override void SetTransitionValue(float _Value)
        {
            Material.SetFloat(TransitionValueId, _Value);
        }

        public override void Activate(bool _Active)
        {
            Renderer.enabled = _Active;
            Material.SetColor(Color1Id, CommonData.CompanyLogoBackgroundColor);
        }

        #endregion
    }
}