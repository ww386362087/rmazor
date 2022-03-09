﻿using System.Runtime.CompilerServices;
using Common.CameraProviders;
using Common.Constants;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using RMAZOR.Views.Common.ViewMazeBackgroundPropertySets;
using RMAZOR.Views.Utils;
using UnityEngine;
using Common.Providers;

namespace RMAZOR.Views.Common.ViewMazeBackgroundTextureProviders
{
    public enum EBackgroundCircleCenterPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }
    
    public interface IViewMazeBackgroundCirclesTextureProvider 
        : IViewMazeBackgroundTextureProvider
    {
        void SetProperties(CirclesTextureSetItem _Item);
    }
    
    public class ViewMazeBackgroundCirclesTextureProvider : 
        ViewMazeBackgroundTextureProviderBase,
        IViewMazeBackgroundCirclesTextureProvider
    {
        #region nonpublic members
        
        private static readonly int
            RadiusId     = Shader.PropertyToID("_Radius"),
            CenterXId    = Shader.PropertyToID("_CenterX"),
            CenterYId    = Shader.PropertyToID("_CenterY"),
            WavesCountId = Shader.PropertyToID("_WavesCount"),
            AmplitudeId  = Shader.PropertyToID("_Amplitude"); 

        #endregion

        #region inject

        public ViewMazeBackgroundCirclesTextureProvider(
            IPrefabSetManager _PrefabSetManager,
            IContainersGetter _ContainersGetter,
            ICameraProvider   _CameraProvider,
            IViewGameTicker   _Ticker,
            IColorProvider    _ColorProvider) 
            : base(
                _PrefabSetManager,
                _ContainersGetter,
                _CameraProvider,
                _Ticker,
                _ColorProvider) { }

        #endregion

        #region api

        protected override int    SortingOrder      => SortingOrders.BackgroundTexture;
        protected override string TexturePrefabName => "circles_texture";

        public void SetProperties(CirclesTextureSetItem _Item)
        {
            Material.SetFloat(RadiusId, _Item.radius);
            Material.SetInt(WavesCountId, _Item.wavesCount);
            Material.SetFloat(AmplitudeId, _Item.amplitude);
            float centerX, centerY;
            switch (_Item.center)
            {
                case EBackgroundCircleCenterPosition.TopLeft:
                    (centerX, centerY) = (0f, 1f);
                    break;
                case EBackgroundCircleCenterPosition.TopRight:
                    (centerX, centerY) = (1f, 1f);
                    break;
                case EBackgroundCircleCenterPosition.BottomLeft:
                    (centerX, centerY) = (0f, 0f);
                    break;
                case EBackgroundCircleCenterPosition.BottomRight:
                    (centerX, centerY) = (1f, 0f);
                    break;
                case EBackgroundCircleCenterPosition.Center:
                    var container = ContainersGetter.GetContainer(ContainerNames.MazeHolder);
                    var worldPos = container.transform.position;
                    var screenPos = CameraProvider.MainCamera.WorldToViewportPoint(worldPos);
                    (centerX, centerY) = (screenPos.x, screenPos.y);
                    break;
                default:
                    throw new SwitchExpressionException(_Item.center);
            }
            Material.SetFloat(CenterXId, centerX);
            Material.SetFloat(CenterYId, centerY);
        }

        #endregion
    }
}