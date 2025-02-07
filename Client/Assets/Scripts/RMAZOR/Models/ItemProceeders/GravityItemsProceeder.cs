﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Extensions;
using Common.Utils;
using mazing.common.Runtime.Entities;
using mazing.common.Runtime.Exceptions;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.Utils;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Models.ProceedInfos;
// ReSharper disable ClassNeverInstantiated.Global

namespace RMAZOR.Models.ItemProceeders
{
    public interface IGravityItemsProceeder : IMovingItemsProceeder, ICharacterMoveStarted
    {
        void OnShredingerBlockEvent(ShredingerBlockArgs _Args);
        void OnKeyLockBlockEvent(KeyLockEventArgs       _Args);
        void OnMazeOrientationChanged();
    }
    
    public class GravityItemsProceeder : 
        MovingItemsProceederBase, 
        IGravityItemsProceeder, 
        IGetAllProceedInfos
    {
        #region constants

        
        #endregion

        #region inject
        
        private IPathItemsProceeder PathItemsProceeder { get; }

        private GravityItemsProceeder(
            ModelSettings       _Settings,
            IModelData          _Data,
            IModelCharacter     _Character,
            IModelGameTicker    _GameTicker,
            IPathItemsProceeder _PathItemsProceeder,
            IModelMazeRotation  _Rotation)
            : base(
                _Settings,
                _Data, 
                _Character, 
                _GameTicker, 
                _Rotation)
        {
            PathItemsProceeder = _PathItemsProceeder;
        }
        
        #endregion
        
        #region api

        protected override EMazeItemType[]              Types              => RmazorUtils.GravityItemTypes;
        public             Func<IMazeItemProceedInfo[]> GetAllProceedInfos { private get; set; }

        public override void OnLevelStageChanged(LevelStageArgs _Args)
        {
            base.OnLevelStageChanged(_Args);
            if (_Args.LevelStage == ELevelStage.ReadyToStart && Rotation.Orientation == EMazeOrientation.North)
                MoveMazeItemsGravity(Rotation.Orientation, Character.Position);
        }

        public void OnShredingerBlockEvent(ShredingerBlockArgs _Args)
        {
            MoveMazeItemsGravity(Rotation.Orientation, Character.Position);
        }

        public void OnKeyLockBlockEvent(KeyLockEventArgs _Args)
        {
            MoveMazeItemsGravity(Rotation.Orientation, Character.Position);
        }

        public void OnMazeOrientationChanged()
        {
            MoveMazeItemsGravity(Rotation.Orientation, Character.Position);
        }

        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            MoveMazeItemsGravity(Rotation.Orientation, _Args.To);
        }

        #endregion

        #region nonpublic methods

        private void MoveMazeItemsGravity(EMazeOrientation _Orientation, V2Int _CharacterPoint)
        {
            var dropDirection = RmazorUtils.GetDropDirection(_Orientation);
            var infos = ProceedInfos.Where(_Info => _Info.IsProceeding);
            var infosMovedDict = 
                infos.ToDictionary(_Info => _Info, _Info => false);
            TryMoveMazeItemsGravityCore(dropDirection, _CharacterPoint, infosMovedDict);
        }

        private void TryMoveMazeItemsGravityCore(
            V2Int                                  _DropDirection,
            V2Int                                  _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _InfosMoved)
        {
            var copyOfDict = _InfosMoved.ToArray();
            foreach (var kvp in copyOfDict)
            {
                if (kvp.Value)
                    continue;
                var info = kvp.Key;
                TryMoveBlock(info, _DropDirection, _CharacterPoint, _InfosMoved);
            }
            foreach (var kvp in copyOfDict)
                kvp.Key.NextPosition = -V2Int.Right;
        }

        private bool TryMoveBlock(IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _InfosMoved)
        {
            switch (_Info.Type)
            {
                case EMazeItemType.GravityBlock:
                    return MoveGravityBlock(_Info, _DropDirection, _CharacterPoint, _InfosMoved);
                case EMazeItemType.GravityBlockFree:
                    return MoveGravityBlockFree(_Info, _DropDirection, _CharacterPoint, _InfosMoved);
                case EMazeItemType.GravityTrap:
                    return MoveGravityTrap(_Info, _DropDirection, _CharacterPoint, _InfosMoved);
                default: throw new SwitchCaseNotImplementedException(_Info.Type);
            }
        }

        private bool MoveGravityBlock(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _GravityItemsMovedDict)
        {
            if (!_GravityItemsMovedDict.ContainsKey(_Info))
                return false;
            if (_GravityItemsMovedDict[_Info])
                return true;
            GravityBlockValidPositionDefinitionCycle(
                _Info,
                _DropDirection,
                _CharacterPoint,
                true,
                false,
                out var to,
                out var gravityBlockItemInfo);
            // если для гравитационного блок/ловушка, на который наткнулась ловушка еще определено конечное положение,
            // пытаемся его определить и если получается, двигаем текущий блок
            if (gravityBlockItemInfo != null)
            {
                if (TryMoveBlock(gravityBlockItemInfo, _DropDirection, _CharacterPoint, _GravityItemsMovedDict))
                {
                    GravityBlockValidPositionDefinitionCycle(
                        _Info,
                        _DropDirection,
                        _CharacterPoint,
                        true,
                        true,
                        out to,
                        out gravityBlockItemInfo);
                }
                else
                    return false;
            }
            _Info.ProceedingStage = ModelCommonData.GravityItemStageDrop;
            ProceedCoroutine(_Info, MoveMazeItemGravityCoroutine(_Info, to));
            _Info.NextPosition = to;
            _GravityItemsMovedDict[_Info] = true;
            return true;
        }
        
        private bool MoveGravityBlockFree(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _GravityItemsMovedDict)
        {
            if (!_GravityItemsMovedDict.ContainsKey(_Info))
                return false;
            if (_GravityItemsMovedDict[_Info])
                return true;
            GravityBlockValidPositionDefinitionCycle(
                _Info,
                _DropDirection,
                _CharacterPoint,
                false,
                false,
                out var to,
                out var gravityBlockItemInfo);
            // если для гравитационного блок/ловушка, на который наткнулась ловушка еще определено конечное положение,
            // пытаемся его определить и если получается, двигаем текущий блок
            if (gravityBlockItemInfo != null)
            {
                if (TryMoveBlock(gravityBlockItemInfo, _DropDirection, _CharacterPoint, _GravityItemsMovedDict))
                {
                    GravityBlockValidPositionDefinitionCycle(
                        _Info,
                        _DropDirection,
                        _CharacterPoint,
                        false,
                        true,
                        out to,
                        out gravityBlockItemInfo);
                }
                else
                    return false;
            }
            _Info.ProceedingStage = ModelCommonData.GravityItemStageDrop;
            ProceedCoroutine(_Info, MoveMazeItemGravityCoroutine(_Info, to));
            _Info.NextPosition = to;
            _GravityItemsMovedDict[_Info] = true;
            return true;
        }
        
        private bool MoveGravityTrap(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int _CharacterPoint,
            Dictionary<IMazeItemProceedInfo, bool> _GravityItemsMovedDict)
        {
            if (!_GravityItemsMovedDict.ContainsKey(_Info))
                return false;
            if (_GravityItemsMovedDict[_Info])
                return true;
            var to = _Info.CurrentPosition;
            IMazeItemProceedInfo gravityBlockItemInfo;
            var infos = GetAllProceedInfos();
            while (IsValidPositionForMove(to + _DropDirection, infos, false, out gravityBlockItemInfo))
                to += _DropDirection;
            // если для гравитационного блок/ловушка, на который наткнулась ловушка еще определено конечное положение,
            // пытаемся его определить и если получается, двигаем текущий блок
            if (gravityBlockItemInfo != null)
            {
                if (TryMoveBlock(gravityBlockItemInfo, _DropDirection, _CharacterPoint, _GravityItemsMovedDict))
                {
                    while (IsValidPositionForMove(to + _DropDirection, infos, true, out gravityBlockItemInfo))
                        to += _DropDirection;
                }
                else
                    return false;
            }
            _Info.ProceedingStage = ModelCommonData.GravityItemStageDrop;
            ProceedCoroutine(_Info, MoveMazeItemGravityCoroutine(_Info, to));
            _Info.NextPosition = to;
            _GravityItemsMovedDict[_Info] = true;
            return true;
        }
        
        private void GravityBlockValidPositionDefinitionCycle(
            IMazeItemProceedInfo _Info,
            V2Int _DropDirection,
            V2Int? _CharacterPoint,
            bool _CheckMazeItemPath,
            bool _CheckNextPos,
            out V2Int _To,
            out IMazeItemProceedInfo _GravityBlockItemInfo)
        {
            bool doMove = false;
            var pos = _Info.CurrentPosition;
            var path = _Info.Path;
            int currPathIdx = path.IndexOf(_Info.CurrentPosition);
            V2Int? altPos = null;
            var infos = GetAllProceedInfos();
            while (IsValidPositionForMove(pos + _DropDirection, infos, _CheckNextPos, out _GravityBlockItemInfo))
            {
                pos += _DropDirection;
                // если новая позиция блока совпадает с позицией персонажа, записываем ее в отдельную переменную
                if (pos == _CharacterPoint)
                {
                    altPos = pos - _DropDirection;
                    break;
                }
                if (!_CheckMazeItemPath)
                    continue;
                // если новая позиция блока не находится на узле пути, проверяем следующую позицию
                if (_Info.Path.All(_Pos1 => pos != _Pos1))
                    continue;
                // если текущая позиция блока не находится на узле пути, а новая - находится, то движение разрешено  
                
                if (currPathIdx == -1)
                {
                    doMove = true;
                    break;
                }
                // если текущая позиция блока находится на узле пути, и новая тоде находится на узле пути,
                // но они не являются близжайшими, проверяем следующую позицию
                if (Math.Abs(path.IndexOf(pos) - path.IndexOf(_Info.CurrentPosition)) > 1)
                    continue;
                doMove = true;
                break;
            }
            // если текущая позиция находится на одном из близжайших участков пути, разрешаем движение
            if (currPathIdx != -1 && _CheckMazeItemPath)
            {
                if (currPathIdx - 1 >= 0 && PathContainsItem(path[currPathIdx - 1], path[currPathIdx], pos)
                || currPathIdx + 1 < path.Count && PathContainsItem(path[currPathIdx], path[currPathIdx + 1], pos))
                    doMove = true;
            }
            else
                doMove = true;
            
            if (altPos.HasValue)
                pos = altPos.Value;
            _To = doMove ? pos : _Info.CurrentPosition;
        }

        private IEnumerator MoveMazeItemGravityCoroutine(
            IMazeItemProceedInfo _Info,
            V2Int _To)
        {
            var from = _Info.CurrentPosition;
            if (from == _To)
            {
                _Info.ProceedingStage = ModelCommonData.StageIdle;
                yield break;
            }
            float speed = Settings.gravityBlockSpeed;
            var busyPositions = _Info.BusyPositions;
            InvokeMoveStarted(new MazeItemMoveEventArgs(_Info, from, _To, speed, 0));
            var direction = (_To - from).Normalized;
            float distance = V2Int.Distance(from, _To);
            yield return Cor.Lerp(
                GameTicker,
                distance / speed,
                _OnProgress: _P =>
                {
                    var addict = direction * ((_P + 0.1f) * distance);
                    busyPositions.Clear();
                    busyPositions.Add(from + V2Int.Floor(addict));
                    if (busyPositions[0] != _To)
                        busyPositions.Add(from + V2Int.Ceil(addict));
                    InvokeMoveContinued(new MazeItemMoveEventArgs(
                        _Info, from, _To, speed, _P));
                },
                _OnFinishEx: (_Stopped, _Progress) =>
                {
                    var to = !_Stopped ? _To : _Info.BusyPositions[0];  
                    _Info.CurrentPosition = to;
                    _Info.ProceedingStage = ModelCommonData.StageIdle;
                    InvokeMoveFinished(new MazeItemMoveEventArgs(
                        _Info, from, to, speed, _Progress));
                    busyPositions.Clear();
                    busyPositions.Add(to);
                });
        }

        private bool IsValidPositionForMove(
            V2Int _Position,
            IMazeItemProceedInfo[] _Infos,
            bool _CheckNextPos,
            out IMazeItemProceedInfo _GravityBlockItemInfo)
        {
            bool isOnNode = PathItemsProceeder.PathProceeds.Keys.Any(_Pos => _Pos == _Position);
            var staticBlockItems = GetStaticBlockItems(GetAllProceedInfos());
            bool isOnStaticBlockItem = staticBlockItems.Any(_N =>
            {
                if (_N.CurrentPosition != _Position)
                    return false;
                if (_N.Type == EMazeItemType.ShredingerBlock
                    && _N.ProceedingStage == ModelCommonData.ShredingerStageClosed)
                    return true;
                if (_N.Type == EMazeItemType.Diode
                    && _N.Direction == -RmazorUtils.GetDirectionVector(EDirection.Down, Rotation.Orientation))
                {
                    return true;
                }
                if (_N.Type == EMazeItemType.KeyLock 
                    && _N.Direction == V2Int.Right 
                    && _N.ProceedingStage == ModelCommonData.KeyLockStage1)
                {
                    return true;
                }
                return _N.Type != EMazeItemType.ShredingerBlock
                       && _N.Type != EMazeItemType.KeyLock 
                       && _N.Type != EMazeItemType.Diode;
            });
            _GravityBlockItemInfo = null;
            for (int i = 0; i < _Infos.Length; i++)
            {
                var info = _Infos[i];
                if (!Types.ContainsAlt(info.Type))
                    continue;
                if (_Position != (_CheckNextPos ? info.NextPosition : info.CurrentPosition))
                    continue;
                _GravityBlockItemInfo = info;
                break;
            }
            bool result = isOnNode && !isOnStaticBlockItem && _GravityBlockItemInfo == null;
            return result;
        }
        
        private static IEnumerable<IMazeItemProceedInfo> GetStaticBlockItems(IEnumerable<IMazeItemProceedInfo> _Items)
        {
            var types = new[]
            {
                EMazeItemType.Block,
                EMazeItemType.Turret,
                EMazeItemType.TrapReact,
                EMazeItemType.TrapIncreasing,
                EMazeItemType.ShredingerBlock,
                EMazeItemType.Portal,
                EMazeItemType.Diode,
                EMazeItemType.KeyLock
            };
            return _Items.Where(_Item => types.Contains(_Item.Type));
        }

        #endregion
    }
}