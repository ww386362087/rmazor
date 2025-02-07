﻿using System.Collections;
using Common;
using Common.Entities;
using Common.Utils;
using mazing.common.Runtime;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
using UnityEngine.Events;

namespace RMAZOR.Models.ItemProceeders
{

    public interface ITrapsMovingProceeder : IMovingItemsProceeder { }
    

    public class TrapsMovingProceeder : MovingItemsProceederBase, IUpdateTick, ITrapsMovingProceeder
    {
        #region nonpublic members

        protected override EMazeItemType[] Types => new[] {EMazeItemType.TrapMoving};

        #endregion

        #region inject

        private TrapsMovingProceeder(
            ModelSettings      _Settings,
            IModelData         _Data,
            IModelCharacter    _Character,
            IModelGameTicker   _GameTicker,
            IModelMazeRotation _Rotation)
            : base(
                _Settings,
                _Data,
                _Character, 
                _GameTicker, 
                _Rotation) { }
        
        #endregion
        
        #region api

        public void UpdateTick()
        {
            ProceedTrapsMoving();
        }
        
        #endregion

        #region nonpublic methods
        
        private void ProceedTrapsMoving()
        {
            for (int i = 0; i < ProceedInfos.Length; i++)
            {
                var info = ProceedInfos[i];
                if (!info.IsProceeding)
                    continue;
                if (info.ProceedingStage != ModelCommonData.StageIdle)
                    continue;
                info.PauseTimer += GameTicker.DeltaTime;
                if (info.PauseTimer < Settings.movingItemsPause)
                    continue;
                info.PauseTimer = 0;
                info.ProceedingStage = ModelCommonData.TrapMovingStageMoving;
                ProceedTrapMoving(info, () => info.ProceedingStage = ModelCommonData.StageIdle);
            }
        }
        
        private void ProceedTrapMoving(
            IMazeItemProceedInfo _Info, 
            UnityAction _OnFinish)
        {
            V2Int from = _Info.CurrentPosition;
            V2Int to;
            int idx = _Info.Path.IndexOf(_Info.CurrentPosition);
            var path = _Info.Path;
            switch (_Info.MoveByPathDirection)
            {
                case EMoveByPathDirection.Forward:
                    if (idx == path.Count - 1)
                    {
                        idx--;
                        _Info.MoveByPathDirection = EMoveByPathDirection.Backward;
                    }
                    else
                        idx++;
                    to = path[idx];
                    break;
                case EMoveByPathDirection.Backward:
                    if (idx == 0)
                    {
                        idx++;
                        _Info.MoveByPathDirection = EMoveByPathDirection.Forward;
                    }
                    else
                        idx--;
                    if (idx < 0)
                    {
                        Dbg.LogError("Index must be greater than zero");
                        return;
                    }
                    to = path[idx];
                    break;
                default: throw new SwitchCaseNotImplementedException(_Info.MoveByPathDirection);
            }
            var coroutine = MoveTrapMovingCoroutine(_Info, from, to, _OnFinish);
            ProceedCoroutine(_Info, coroutine);
        }
        
        private IEnumerator MoveTrapMovingCoroutine(
            IMazeItemProceedInfo _Info, 
            V2Int _From,
            V2Int _To,
            UnityAction _OnFinish)
        {
            _Info.IsMoving = true;
            _Info.CurrentPosition = _From;
            float movingItemsSpeed = Settings.movingItemsSpeed;
            InvokeMoveStarted(new MazeItemMoveEventArgs(
                _Info, _From, _To, movingItemsSpeed,0));
            float distance = V2Int.Distance(_From, _To);
            yield return Cor.Lerp(
                GameTicker,
                distance / movingItemsSpeed,
                _OnProgress: _P =>
                {
                    var precisePosition = V2Int.Lerp(_From, _To, _P);
                    _Info.CurrentPosition = V2Int.Round(precisePosition);
                    InvokeMoveContinued(new MazeItemMoveEventArgs(
                        _Info, _From, _To, movingItemsSpeed, _P));
                },
                 _OnFinish: () =>
                {
                    _Info.CurrentPosition = _To;
                    _Info.IsMoving = false;
                    _OnFinish?.Invoke();
                    InvokeMoveFinished(new MazeItemMoveEventArgs(
                        _Info, _From, _To, movingItemsSpeed,1f));
                });
        }
        
        #endregion
    }
}