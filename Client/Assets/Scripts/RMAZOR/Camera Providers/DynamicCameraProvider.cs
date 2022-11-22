﻿using Common.CameraProviders;
using Common.Entities;
using Common.Extensions;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;

namespace RMAZOR.Camera_Providers
{
    public interface IDynamicCameraProvider : ICameraProvider { }
    
    public class DynamicCameraProvider :
        CameraProviderBase,
        IDynamicCameraProvider,
        IFixedUpdateTick
    {
        #region constants

        private const float MaxFollowDistanceX  = 2f;
        private const float MaxFollowDistanceY  = 2f;
        private const float MaxMazeBorderIndent = 5f;

        #endregion
        
        #region nonpublic members

        protected override string CameraName => "Dynamic Camera";
        
        private Vector2? m_CameraPosition;
        private bool     m_EnableFollow;

            #endregion
        
        #region inject

        private ViewSettings    ViewSettings   { get; }

        protected DynamicCameraProvider(
            IPrefabSetManager _PrefabSetManager,
            ViewSettings      _ViewSettings,
            IViewGameTicker   _ViewGameTicker) 
            : base(_PrefabSetManager, _ViewGameTicker)
        {
            ViewSettings   = _ViewSettings;
        }

        #endregion

        #region api
        
        public override void Init()
        {
            if (Initialized)
                return;
            ViewGameTicker.Register(this);
            CommonDataRmazor.LastMazeSizeChanged += OnLastMazeSizeChanged;
            base.Init();
        }

        public void FixedUpdateTick()
        {
            CameraFollow();
        }

        #endregion

        #region nonpublic methods
        
        private void OnLastMazeSizeChanged(V2Int _Size)
        {
            m_EnableFollow = RmazorUtils.IsBigMaze(_Size);
        }

        private void CameraFollow()
        {
            if (!LevelCameraInitialized
                || !FollowTransformIsNotNull
                || !m_EnableFollow)
            {
                return;
            }
            var camPos = SetCameraPositionRaw();
            camPos = KeepCameraInCharacterRectangle(camPos);
            camPos = KeepCameraInMazeRectangle(camPos);
            m_CameraPosition = camPos;
            LevelCameraTr.SetPosXY(camPos);
        }

        private Vector2 SetCameraPositionRaw()
        {
            if (!m_CameraPosition.HasValue)
                m_CameraPosition = Follow.position;
            else
            {
                var newPos = Vector2.Lerp(
                    m_CameraPosition.Value, Follow.position, ViewSettings.cameraSpeed);
                m_CameraPosition = newPos;
            }
            return m_CameraPosition!.Value;
        }

        private Vector2 KeepCameraInCharacterRectangle(Vector2 _CameraPosition)
        {
            if (GetConverterScale == null)
                return _CameraPosition;
            float scale = GetConverterScale();
            var followPos = Follow.position;
            var camPos = _CameraPosition;
            float minX = followPos.x - MaxFollowDistanceX * scale;
            float maxX = followPos.x + MaxFollowDistanceX * scale;
            float minY = followPos.y - MaxFollowDistanceY * scale;
            float maxY = followPos.y + MaxFollowDistanceY * scale;
            camPos.x = MathUtils.Clamp(camPos.x, minX, maxX);
            camPos.y = MathUtils.Clamp(camPos.y, minY, maxY);
            return camPos;
        }

        private Vector2 KeepCameraInMazeRectangle(Vector2 _CameraPosition)
        {
            if (GetConverterScale == null)
                return _CameraPosition;
            float scale = GetConverterScale();
            var mazeBounds = GetMazeBounds();
            var camPos = _CameraPosition;
            float minX = mazeBounds.min.x + MaxMazeBorderIndent * scale;
            float maxX = mazeBounds.max.x - MaxMazeBorderIndent * scale;
            float minY = mazeBounds.min.y + MaxMazeBorderIndent * scale;
            float maxY = mazeBounds.max.y - MaxMazeBorderIndent * scale;
            camPos.x = MathUtils.Clamp(camPos.x, minX, maxX);
            camPos.y = MathUtils.Clamp(camPos.y, minY, maxY);
            return camPos;
        }

        #endregion
    }
}