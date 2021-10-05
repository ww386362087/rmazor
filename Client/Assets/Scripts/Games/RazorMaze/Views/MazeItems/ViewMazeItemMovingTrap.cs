﻿using System;
using System.Collections.Generic;
using DI.Extensions;
using Entities;
using GameHelpers;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using Ticker;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemMovingTrap : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemMovingTrap : ViewMazeItemMovingBase, IViewMazeItemMovingTrap, IUpdateTick
    {
        #region constants

        private const string SoundClipNameMoveTrap = "saw_working";
        
        #endregion
        
        #region nonpublic members
        
        private Vector2 m_Position;
        private bool m_Rotating;
        
        #endregion
        
        #region shapes

        protected override object[] Shapes => new object[] {m_Saw};

        private SpriteRenderer m_Saw;

        #endregion
        
        #region inject
        
        public ViewMazeItemMovingTrap(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            ICoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers)
            : base(
                _ViewSettings,
                _Model, 
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers) { }
        
        #endregion
        
        #region api
        
        public override object Clone() => new ViewMazeItemMovingTrap(
            ViewSettings,
            Model, 
            CoordinateConverter, 
            ContainersGetter,
            GameTicker,
            Transitioner,
            Managers);

        public override EProceedingStage ProceedingStage
        {
            get => base.ProceedingStage;
            set
            {
                base.ProceedingStage = value;
                if (value == EProceedingStage.Inactive)
                    StopRotation();
                else
                    StartRotation();
            }
        }

        public override void OnMoving(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            var precisePosition = Vector2.Lerp(
                _Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(precisePosition));
        }

        public override void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
        }

        public void UpdateTick()
        {
            if (!Initialized || !Activated)
                return;
            DoRotation();
        }
        
        #endregion

        #region nonpublic methods

        protected override void SetShape()
        {
            var go = Object;
            var saw = ContainersGetter.MazeItemsContainer.gameObject
                .GetOrAddComponentOnNewChild<SpriteRenderer>(
                    "Moving Trap", 
                    ref go,
                    CoordinateConverter.ToLocalMazeItemPosition(Props.Position));
            go.DestroyChildrenSafe();
            saw.sprite = PrefabUtilsEx.GetObject<Sprite>("views", "moving_trap");
            saw.color = DrawingUtils.ColorTrap;
            saw.sortingOrder = DrawingUtils.GetBlockSortingOrder(Props.Type);
            saw.enabled = false;
            var coll = go.AddComponent<CircleCollider2D>();
            coll.radius = 0.5f;

            go.transform.localScale = Vector3.one * CoordinateConverter.GetScale();
            
            Object = go;
            m_Saw = saw;

            base.SetShape();
        }

        private void DoRotation()
        {
            if (!m_Rotating)
                return;
            float rotSpeed = ViewSettings.MovingTrapRotationSpeed * Time.deltaTime; 
            Object.transform.Rotate(Vector3.forward * rotSpeed);
        }

        private void StartRotation()
        {
            Managers.Notify(_SM => _SM.PlayClip(
                SoundClipNameMoveTrap, true, _Tags: $"{GetHashCode()}"));
            m_Rotating = true;
        }

        private void StopRotation()
        {
            Managers.Notify(_SM => _SM.StopClip(
                SoundClipNameMoveTrap, _Tags: $"{GetHashCode()}"));
            m_Rotating = false;
        }

        protected override void Appear(bool _Appear)
        {
            base.Appear(_Appear);
            Coroutines.Run(Coroutines.WaitWhile(
                () => !Initialized,
                () =>
                {
                    var sets = new Dictionary<object[], Func<Color>>
                    {
                        {new object[] {m_Saw}, () => DrawingUtils.ColorLines}
                    };
                    Transitioner.DoAppearTransitionSimple(
                        _Appear,
                        GameTicker,
                        sets,
                        Props.Position);
                }));
        }

        #endregion
    }
}