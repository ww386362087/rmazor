﻿using System;
using System.Collections;
using System.Collections.Generic;
using Common;
using Common.Extensions;
using Common.Utils;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models;
using RMAZOR.Views.Coordinate_Converters;
using UnityEngine;

namespace RMAZOR.Views.UI
{
    public class HandSwipeMovement : HandSwipeBase
    {
        #region types

        [Serializable]
        public class HandSpriteMovementTraceParams : HandSpriteTraceParamsBase
        {
            public PointPositions aMoveLeftPositions;
            public PointPositions aMoveRightPositions;
            public PointPositions aMoveDownPositions;
            public PointPositions aMoveUpPositions;
        }
        
        #endregion

        #region serialized fields

        [SerializeField] private LineRenderer                  trace;
        [SerializeField] private HandSpriteMovementTraceParams moveParams;

        #endregion

        #region nonpublic members
        
        protected override Dictionary<EDirection, float> HandAngles => 
            new Dictionary<EDirection, float>
            {
                {EDirection.Left, 0f},
                {EDirection.Right, 0f},
                {EDirection.Down, 90f},
                {EDirection.Up, 90f},
            };

        #endregion

        #region api

        public override void Init(
            IUnityTicker         _Ticker,
            ICameraProvider      _CameraProvider,
            ICoordinateConverter _CoordinateConverter,
            IColorProvider       _ColorProvider,
            Vector4              _Offsets)
        {
            base.Init(_Ticker, _CameraProvider, _CoordinateConverter, _ColorProvider, _Offsets);
            var uiCol = _ColorProvider.GetColor(ColorIds.UI);
            hand.color = uiCol;
            trace.startColor = trace.endColor = uiCol.SetA(0f);
            trace.widthMultiplier = 1f;
        }

        public void ShowMoveLeftPrompt()
        {
            Direction = EDirection.Left;
            ReadyToAnimate = true;
        }

        public void ShowMoveRightPrompt()
        {
            Direction = EDirection.Right;
            ReadyToAnimate = true;
        }

        public void ShowMoveUpPrompt()
        {
            Direction = EDirection.Up;
            ReadyToAnimate = true;
        }

        public void ShowMoveDownPrompt()
        {
            Direction = EDirection.Down;
            ReadyToAnimate = true;
        }

        public override void HidePrompt()
        {
            base.HidePrompt();
            trace.enabled = false;
        }
        
        public override void UpdateTick()
        {
            if (Direction.HasValue && ReadyToAnimate)
                AnimateHandAndTrace(Direction.Value, moveParams.aTimeEnd);
        }
        
        #endregion

        #region nonpublic methods
        
        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            base.OnColorChanged(_ColorId, _Color);
            if (_ColorId != ColorIds.UI)
                return;
            var oldCol = trace.startColor;
            var newCol = oldCol.SetR(_Color.r).SetG(_Color.g).SetB(_Color.b);
            trace.startColor = trace.endColor = newCol;
        }
        
        protected override IEnumerator AnimateTraceCoroutine(EDirection _Direction)
        {
            var a = _Direction switch
            {
                EDirection.Left  => moveParams.aMoveLeftPositions,
                EDirection.Right => moveParams.aMoveRightPositions,
                EDirection.Down  => moveParams.aMoveDownPositions,
                EDirection.Up    => moveParams.aMoveUpPositions,
                _                        => throw new SwitchCaseNotImplementedException(_Direction)
            };
            trace.enabled = false;
            trace.SetPosition(0, a.bPos);
            trace.SetPosition(1, a.posStart);
            if (TutorialFinished)
                yield break;
            yield return Cor.Lerp(
                Ticker,
                moveParams.aTimeStart,
                _OnProgress: _P =>
                {
                    hand.color = hand.color.SetA(_P);
                    trace.startColor = trace.endColor = trace.startColor.SetA(_P * 0.5f);
                },
                _BreakPredicate: () => ReadyToAnimate || TutorialFinished);
            if (TutorialFinished)
                yield break;
            trace.enabled = true;
            yield return Cor.Lerp(
                Ticker,
                moveParams.aTimeMiddle - moveParams.aTimeStart, 
                _OnProgress: _P =>
                {
                    var vec = Vector2.Lerp(a.posStart, a.posMiddle, _P);
                    trace.SetPosition(1, vec);
                },
                _BreakPredicate: () => ReadyToAnimate || TutorialFinished);
            if (TutorialFinished)
                yield break;
            var traceCol = trace.startColor;
            yield return Cor.Lerp(
                Ticker,
                moveParams.aTimeEnd - moveParams.aTimeMiddle, 
                _OnProgress: _P =>
                {
                    trace.SetPosition(1, Vector2.Lerp(a.posMiddle, a.posEnd, _P));
                    trace.startColor = trace.endColor = Color.Lerp(traceCol.SetA(0.5f), traceCol.SetA(0f), _P);
                },
                _BreakPredicate: () => ReadyToAnimate || TutorialFinished);
        }

        protected override IEnumerator AnimateHandPositionCoroutine(EDirection _Direction)
        {
            yield return AnimateHandPositionCoroutine(_Direction, moveParams);
        }

        #endregion
    }
}