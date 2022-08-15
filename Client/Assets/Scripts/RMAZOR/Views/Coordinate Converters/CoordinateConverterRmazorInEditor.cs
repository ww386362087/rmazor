﻿using System;
using Common.CameraProviders;
using Common.Entities;
using UnityEngine;

namespace RMAZOR.Views.Coordinate_Converters
{
    public interface ICoordinateConverterRmazorInEditor : ICoordinateConverterRmazorBase
    {
        void SetMazeSize(V2Int _Size);
    }
    
    [Serializable]
    public class CoordinateConverterRmazorInEditor :
        CoordinateConverterRmazor, 
        ICoordinateConverterRmazorInEditor
    {
        #region inject

        private CoordinateConverterRmazorInEditor(
            ViewSettings    _ViewSettings,
            ICameraProvider _CameraProvider)
            : base(_ViewSettings, _CameraProvider) { }

        #endregion

        #region factory method

        public static CoordinateConverterRmazorInEditor Create(
            ViewSettings    _ViewSettings,
            ICameraProvider _CameraProvider,
            bool            _Debug)
        {
            return new CoordinateConverterRmazorInEditor(_ViewSettings, _CameraProvider) {IsDebug = _Debug};
        }

        #endregion

        #region api

        public void SetMazeSize(V2Int _Size)
        {
            MazeSizeForPositioning = _Size;
            MazeSizeForScale = _Size;
            MazeDataWasSet = true;
            CheckForErrors();
            SetScale();
        }

        public override Vector2 ToLocalMazeItemPosition(Vector2 _Point)
        {
            return ToLocalMazePosition(_Point + Vector2.right * .5f);
        }
        
        public override Vector2 ToLocalCharacterPosition(Vector2 _Point)
        {
            return ToLocalMazePosition(_Point + Vector2.one * .5f);
        }

        #endregion

        #region nonpublic menhods

        protected override Vector2 ToLocalMazePosition(Vector2 _Point)
        {
            return ScaleValue * (_Point - MazeSizeForPositioning * 0.5f);
        }

        #endregion
        
    }
}