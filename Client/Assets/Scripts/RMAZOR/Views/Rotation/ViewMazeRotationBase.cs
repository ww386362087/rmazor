﻿using Common.Exceptions;
using RMAZOR.Models;
using UnityEngine.Events;

namespace RMAZOR.Views.Rotation
{
    public abstract class ViewMazeRotationBase : IViewMazeRotation
    {
        #region api

        public bool                                  Initialized { get; private set; }
        public event          UnityAction            Initialize;
        public abstract event UnityAction<float>     RotationContinued;
        public abstract event MazeOrientationHandler RotationFinished;
        
        public virtual void Init()
        {
            Initialize?.Invoke();
            Initialized = true;
        }

        public abstract void OnRotationStarted(MazeRotationEventArgs _Args);
        public abstract void OnRotationFinished(MazeRotationEventArgs _Args);
        public abstract void OnLevelStageChanged(LevelStageArgs _Args);


        #endregion
        
        #region nonpublic methods

        protected static float GetAngleByOrientation(MazeOrientation _Orientation)
        {
            switch (_Orientation)
            {
                case MazeOrientation.North: return 0;
                case MazeOrientation.East:  return 270;
                case MazeOrientation.South: return 180;
                case MazeOrientation.West:  return 90;
                default: throw new SwitchCaseNotImplementedException(_Orientation);
            }
        }
        
        #endregion

    }
}