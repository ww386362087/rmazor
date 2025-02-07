﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common;
using Common.Constants;
using Common.Extensions;
using Common.Helpers;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using RMAZOR.Views.MazeItems;
using UnityEngine;
using UnityEngine.Events;

namespace RMAZOR.Views.Common
{
    public interface IMazeShaker : IInit, IOnLevelStageChanged
    {
        bool        ShakeMaze { get; set; }
        IEnumerator HitMazeCoroutine(CharacterMovingFinishedEventArgs _Args);
        
        IEnumerator ShakeMazeCoroutine(float _Duration, float _Amplitude);
        void OnCharacterDeathAnimation(
            Vector2                    _DeathPosition,
            IEnumerable<IViewMazeItem> _MazeItems,
            UnityAction                _OnFinish);
    }
    
    public class MazeShaker : InitBase, IMazeShaker, IUpdateTick
    {
        #region nonpublic members

        private Transform m_MazeContainer;
        private Vector3   m_StartPosition;
        private bool      m_ShakeMaze;

        #endregion
        
        #region inject

        private IContainersGetter    ContainersGetter    { get; }
        private ICoordinateConverter CoordinateConverter { get; }
        private IViewGameTicker      GameTicker          { get; }

        private MazeShaker(
            IContainersGetter    _ContainersGetter,
            ICoordinateConverter _CoordinateConverter,
            IViewGameTicker      _GameTicker)
        {
            ContainersGetter    = _ContainersGetter;
            CoordinateConverter = _CoordinateConverter;
            GameTicker          = _GameTicker;
        }

        #endregion

        #region api

        public bool ShakeMaze
        {
            get => m_ShakeMaze;
            set
            {
                m_ShakeMaze = value;
                if (!value)
                    m_MazeContainer.position = m_StartPosition;
            }
        }

        public override void Init()
        {
            m_MazeContainer = ContainersGetter.GetContainer(ContainerNamesMazor.Maze);
            GameTicker.Register(this);
            base.Init();
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            if (_Args.LevelStage == ELevelStage.Loaded)
                m_StartPosition = CoordinateConverter.GetMazeBounds().center;
        }

        public IEnumerator HitMazeCoroutine(CharacterMovingFinishedEventArgs _Args)
        {
            const float amplitude = 1f;
            var dir = RmazorUtils.GetDirectionVector(_Args.Direction, EMazeOrientation.North);
            const float duration = 0.2f;
            yield return Cor.Lerp(
                GameTicker,
                duration,
                _OnProgress: _P =>
                {
                    float distance = _P < 0.5f ? _P * amplitude : (1f - _P) * amplitude;
                    var res = (Vector2)m_StartPosition + distance * dir;
                    m_MazeContainer.position = res;
                },
                _OnFinish: () => m_MazeContainer.position = m_StartPosition);
        }

        public IEnumerator ShakeMazeCoroutine(float _Duration, float _Amplitude)
        {
            yield return Cor.Lerp(
                GameTicker,
                _Duration,
                1f,
                0f,
                _Progress =>
                {
                    float amplitude = _Amplitude * _Progress;
                    Vector2 res;
                    res.x = m_StartPosition.x + amplitude * Mathf.Sin(GameTicker.Time * 200f);
                    res.y = m_StartPosition.y + amplitude * Mathf.Cos(GameTicker.Time * 100f);
                    m_MazeContainer.position = res;
                },
                () => m_MazeContainer.position = m_StartPosition);
        }
        
        public void UpdateTick()
        {
            if (!Initialized)
                return;
            if (!m_ShakeMaze)
                return;
            float amplitude = 0.1f;
            Vector2 res;
            res.x = m_StartPosition.x + amplitude * Mathf.Sin(GameTicker.Time * 200f);
            res.y = m_StartPosition.y + amplitude * Mathf.Cos(GameTicker.Time * 100f);
            m_MazeContainer.position = res;
        }
        
         public void OnCharacterDeathAnimation(
             Vector2                    _DeathPosition,
             IEnumerable<IViewMazeItem> _MazeItems,
             UnityAction                _OnFinish)
        {
            const float scaleCoeff = 0.4f;
            const float maxDistance = 10f;
            const float transitionTime = 0.5f;
            var shapes = _MazeItems
                .SelectMany(_Item => _Item.Renderers.Where(_Shape => _Shape.IsNotNull()))
                .Distinct()
                .ToList();
            var finished = shapes
                .ToDictionary(_Shape => _Shape, _Shape => false);
            foreach (var shape in shapes)
            {
                float dist = Vector2.Distance(_DeathPosition, shape.transform.position);
                if (dist > maxDistance * CoordinateConverter.Scale)
                {
                    finished[shape] = true;
                    continue;
                }
                var shapeTr = shape.transform;
                var startLocalScale = shapeTr.localScale;
                var startRotation = shapeTr.localRotation;
                float angleSign = 1f;
                Cor.Run(Cor.Delay(
                Mathf.Max(0f, 2f * (dist - 1) * 0.01f),
                GameTicker,
                () =>
                {
                    Cor.Run(Cor.Lerp(
                    GameTicker,
                    transitionTime,
                    _OnProgress: _P =>
                    {
                        var scale = startLocalScale * (1f + _P * scaleCoeff);
                        angleSign = -angleSign;
                        var angles = startRotation.eulerAngles + angleSign * Vector3.forward * _P * 10f;
                        shapeTr.localScale = scale;
                        shapeTr.localRotation = Quaternion.Euler(angles);
                    },
                    _OnFinish: () =>
                    {
                        shapeTr.localScale = startLocalScale;
                        shapeTr.localRotation = startRotation;
                        finished[shape] = true;
                    },
                    _ProgressFormula: _P => _P < 0.5f ? 2f * _P : 2f * (1f - _P)));
                }));
            }
            Cor.Run(Cor.WaitWhile(
            () => finished.Values.Any(_F => !_F),
            () => _OnFinish?.Invoke()));
        }

        #endregion
    }
}