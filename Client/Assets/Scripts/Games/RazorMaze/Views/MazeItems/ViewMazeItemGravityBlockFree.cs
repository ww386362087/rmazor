﻿using DI.Extensions;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Models.ItemProceeders;
using Games.RazorMaze.Views.Common;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.Helpers;
using Games.RazorMaze.Views.Utils;
using Shapes;
using Ticker;
using UnityEngine;

namespace Games.RazorMaze.Views.MazeItems
{
    public interface IViewMazeItemGravityBlockFree : IViewMazeItemMovingBlock { }
    
    public class ViewMazeItemGravityBlockFree : ViewMazeItemMovingBase, IViewMazeItemGravityBlockFree 
    {
        #region shapes

        protected override string ObjectName => "Gravity Block Free";
        protected Rectangle m_Shape;
        
        #endregion
        
        #region inject
        
        public ViewMazeItemGravityBlockFree(
            ViewSettings _ViewSettings,
            IModelGame _Model,
            IMazeCoordinateConverter _CoordinateConverter,
            IContainersGetter _ContainersGetter,
            IGameTicker _GameTicker,
            IViewAppearTransitioner _Transitioner,
            IManagersGetter _Managers,
            IColorProvider _ColorProvider)
            : base(
                _ViewSettings,
                _Model,
                _CoordinateConverter,
                _ContainersGetter,
                _GameTicker,
                _Transitioner,
                _Managers,
                _ColorProvider) { }
        
        #endregion
        
        #region api
        
        public override Component[] Shapes => new Component[] {m_Shape};
        public override object Clone() => new ViewMazeItemGravityBlockFree(
            ViewSettings,
            Model,
            CoordinateConverter, 
            ContainersGetter, 
            GameTicker,
            Transitioner,
            Managers,
            ColorProvider);
        
        public override void OnMoving(MazeItemMoveEventArgs _Args)
        {
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            var pos = Vector2.Lerp(_Args.From.ToVector2(), _Args.To.ToVector2(), _Args.Progress);
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(pos));
        }

        public override void OnMoveFinished(MazeItemMoveEventArgs _Args)
        {
            base.OnMoveFinished(_Args);
            if (ProceedingStage != EProceedingStage.ActiveAndWorking)
                return;
            SetLocalPosition(CoordinateConverter.ToLocalMazeItemPosition(_Args.To));
            Managers.Notify();
        }

        #endregion
        
        #region nonpublic methods

        protected override void InitShape()
        {
            var sh = Object.AddComponentOnNewChild<Rectangle>("Block", out _);
            sh.Type = Rectangle.RectangleType.RoundedBorder;
            sh.Color = ColorProvider.GetColor(ColorIds.Main);
            sh.SortingOrder = SortingOrders.GetBlockSortingOrder(Props.Type);
            m_Shape = sh;
        }

        protected override void UpdateShape()
        {
            base.UpdateShape();
            m_Shape.Width = m_Shape.Height = CoordinateConverter.Scale * 0.9f;
            m_Shape.Thickness = ViewSettings.LineWidth * CoordinateConverter.Scale;
            m_Shape.CornerRadius = ViewSettings.CornerRadius * CoordinateConverter.Scale;
        }

        protected override void InitWallBlockMovingPaths() { }

        protected override void OnColorChanged(int _ColorId, Color _Color)
        {
            if (_ColorId == ColorIds.Main)
                m_Shape.Color = _Color;
        }

        #endregion

    }
}