﻿using System.Collections.Generic;
using System.Linq;
using Entities;
using Games.RazorMaze.Models;
using Games.RazorMaze.Views.ContainerGetters;
using Games.RazorMaze.Views.MazeItems;
using SpawnPools;
using UnityEngine;
using Zenject;

namespace Games.RazorMaze.Views.Helpers
{
    public class MazeItemsCreatorProt : MazeItemsCreatorBase
    {
        #region inject
        
        [Inject]
        public MazeItemsCreatorProt(
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter, 
            IViewMazeItemPath _ItemPath,
            IViewMazeItemGravityBlock _GravityBlock,
            IViewMazeItemMovingTrap _MovingTrap,
            IViewMazeItemShredingerBlock _ShredingerBlock,
            IViewMazeItemTurret _Turret,
            IViewMazeItemSpringboard _Springboard,
            IViewMazeItemPortal _Portal,
            IViewMazeItemGravityTrap _GravityTrap,
            IViewMazeItemTrapReact _TrapReact,
            IViewMazeItemTrapIncreasing _TrapIncreasing) : base(
            _ContainersGetter, 
            _CoordinateConverter, 
            _ItemPath,
            _GravityBlock, 
            _MovingTrap,
            _ShredingerBlock,
            _Turret, 
            _Springboard,
            _Portal,
            _GravityTrap,
            _TrapReact,
            _TrapIncreasing) { }
        
        #endregion
        
        protected MazeItemsCreatorProt(
            IContainersGetter _ContainersGetter,
            ICoordinateConverter _CoordinateConverter) : base(_ContainersGetter, _CoordinateConverter)
        { }

        public override void InitMazeItems(
            MazeInfo _Info,
            SpawnPool<IViewMazeItemPath> _PathPool,
            Dictionary<EMazeItemType,
                SpawnPool<IViewMazeItem>> _BlockPools)
        {
            throw new System.NotImplementedException("This method is only for release");
        }

        protected override void AddPathItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, V2Int _Position)
        {
            AddPathItemProt(
                _Items, 
                _Info.Size,
                new ViewMazeItemProps
                {
                    IsNode = true,
                    IsStartNode = !_Items.Any(),
                    Position = _Position
                });
        }

        protected override void AddMazeItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, MazeItem _Item)
        {
            var props = new ViewMazeItemProps
            {
                Type = _Item.Type,
                Position = _Item.Position,
                Path = _Item.Path,
                Directions = new List<V2Int>{_Item.Direction},
                Pair = _Item.Pair,
                IsNode = false,
                IsStartNode = false
            };
            AddMazeItemProt(_Items, _Info.Size, props);
        }
        
        private void AddPathItemProt(
            ICollection<IViewMazeItem> _Items,
            V2Int _MazeSize,
            ViewMazeItemProps _Props) 
        {
            var tr = new GameObject("Path Item").transform;
            tr.SetParent(ContainersGetter.MazeItemsContainer);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();
            item.MazeSize = _MazeSize; 
            item.Init(_Props);
            _Items.Add(item);
        }
        
        private void AddMazeItemProt(
            ICollection<IViewMazeItem> _Items,
            V2Int _MazeSize,
            ViewMazeItemProps _Props)
        {
            if (_Props.Type == EMazeItemType.Block || this is MazeItemsCreatorInEditor)
                return;
            
            var tr = new GameObject("Maze Item").transform;
            tr.SetParent(ContainersGetter.MazeItemsContainer);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();

            item.MazeSize = _MazeSize;
            item.Init(_Props);
            _Items.Add(item);
        }
    }
}