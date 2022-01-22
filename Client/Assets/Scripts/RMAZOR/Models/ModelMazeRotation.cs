﻿using Common;
using Common.Exceptions;
using Common.Utils;
using RMAZOR.Views;
using UnityEngine.Events;

namespace RMAZOR.Models
{
    public enum EMazeRotateDirection {Clockwise, CounterClockwise}

    public class MazeRotationEventArgs
    {
        public EMazeRotateDirection Direction { get; }
        public MazeOrientation CurrentOrientation { get; }
        public MazeOrientation NextOrientation { get; }
        public bool Instantly { get; }

        public MazeRotationEventArgs(
            EMazeRotateDirection _Direction, 
            MazeOrientation _CurrentOrientation,
            MazeOrientation _NextOrientation,
            bool _Instantly)
        {
            Direction = _Direction;
            CurrentOrientation = _CurrentOrientation;
            NextOrientation = _NextOrientation;
            Instantly = _Instantly;
        }
    }
    
    public delegate void MazeOrientationHandler(MazeRotationEventArgs Args);
    
    public interface IModelMazeRotation : IInit, IOnLevelStageChanged
    {
        event MazeOrientationHandler RotationStarted;
        event MazeOrientationHandler RotationFinished;
        void StartRotation(EMazeRotateDirection _Direction, MazeOrientation? _NextOrientation = null);
        void OnRotationFinished(MazeRotationEventArgs _Args);
    }
    
    public class ModelMazeRotation : IModelMazeRotation 
    {

        private IModelData Data { get; }

        public ModelMazeRotation(IModelData _Data)
        {
            Data = _Data;
        }

        public event MazeOrientationHandler RotationStarted;
        public event MazeOrientationHandler RotationFinished;
        public bool                         Initialized { get; private set; }
        public event UnityAction            Initialize;
        
        public void Init()
        {
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public void StartRotation(
            EMazeRotateDirection _Direction, 
            MazeOrientation? _NextOrientation = null)
        {
            if (!Data.ProceedingControls)
                return;
            var currOrientation = Data.Orientation;
            Data.Orientation = _NextOrientation ?? GetNextOrientation(_Direction, currOrientation);
            var args = new MazeRotationEventArgs(
                _Direction, currOrientation, Data.Orientation, false);
            RotationStarted?.Invoke(args);
        }

        public void OnRotationFinished(MazeRotationEventArgs _Args)
        {
            RotationFinished?.Invoke(_Args);
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var stage = _Args.Stage;
            if (stage == ELevelStage.Unloaded)
            {
                Data.Orientation = MazeOrientation.North;
                var args = new MazeRotationEventArgs(
                    default, default, Data.Orientation, true);
                RotationStarted?.Invoke(args);
            }
            else if (_Args.Stage == ELevelStage.ReadyToStart && _Args.PreviousStage != ELevelStage.CharacterKilled)
            {
                if (Data.Orientation == MazeOrientation.North)
                    return;
                var rotDir = (int) Data.Orientation < 2
                    ? EMazeRotateDirection.CounterClockwise
                    : EMazeRotateDirection.Clockwise;
                var currOrient = Data.Orientation;
                Data.Orientation = MazeOrientation.North;
                var args = new MazeRotationEventArgs(
                    rotDir, currOrient, Data.Orientation, false);
                RotationStarted?.Invoke(args);
            }
        }
        
        private static MazeOrientation GetNextOrientation(
            EMazeRotateDirection _Direction,
            MazeOrientation _Orientation)
        {
            int orient = (int) _Orientation;
            switch (_Direction)
            {
                case EMazeRotateDirection.Clockwise:
                    orient = MathUtils.ClampInverse(orient + 1, 0, 3); break;
                case EMazeRotateDirection.CounterClockwise:
                    orient = MathUtils.ClampInverse(orient - 1, 0, 3); break;
                default: throw new SwitchCaseNotImplementedException(_Direction);
            }
            return (MazeOrientation) orient;
        }
    }
}