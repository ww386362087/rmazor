﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common.Constants;
using Common.Entities;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.MazeItems;
using RMAZOR.Views.MazeItems.Props;
using SpawnPools;
using UnityEngine;
using Zenject;

namespace RMAZOR.Views.Helpers.MazeItemsCreators
{
    public class MazeItemsCreatorProt : MazeItemsCreatorBase
    {
        #region inject
        
        [Inject]
        public MazeItemsCreatorProt(
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
        
        #region protected constructor
        
        protected MazeItemsCreatorProt() { }
        
        #endregion
        
        #region api

        public override void InitPathItems(MazeInfo _Info, SpawnPool<IViewMazeItemPath> _PathPool)
        {
            throw new NotImplementedException("This method is only for release");
        }

        public override void InitAndActivateBlockItems(MazeInfo _Info, Dictionary<EMazeItemType, SpawnPool<IViewMazeItem>> _BlockPools)
        {
            throw new NotImplementedException("This method is only for release");
        }
        
        #endregion
        
        #region nonpublic methods



        protected override void AddPathItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, PathItem _Item)
        {
            AddPathItemProt(
                _Items, 
                _Info.Size,
                new ViewMazeItemProps
                {
                    IsNode = true,
                    IsStartNode = !_Items.Any(),
                    Position = _Item.Position,
                    Blank = _Item.Blank
                });
        }

        protected override void AddMazeItem(ICollection<IViewMazeItem> _Items, MazeInfo _Info, MazeItem _Item)
        {
            var props = new ViewMazeItemProps
            {
                Type = _Item.Type,
                Position = _Item.Position,
                Path = _Item.Path,
                Directions = _Item.Directions,
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
            var go = GameObject.Find(ContainerNames.MazeItems);
            if (go == null)
                go = new GameObject(ContainerNames.MazeItems);
            tr.SetParent(go.transform);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();
            item.MazeSize = _MazeSize; 
            item.Init(_Props);
            _Items.Add(item);
        }
        
        protected virtual void AddMazeItemProt(
            ICollection<IViewMazeItem> _Items,
            V2Int _MazeSize,
            ViewMazeItemProps _Props)
        {
            if (_Props.Type == EMazeItemType.Block)
                return;
            AddMazeItemProtCore(_Items, _MazeSize, _Props);
        }

        protected void AddMazeItemProtCore(
            ICollection<IViewMazeItem> _Items,
            V2Int _MazeSize,
            ViewMazeItemProps _Props)
        {
            var tr = new GameObject("Maze Item").transform;
            var go = GameObject.Find(ContainerNames.MazeItems);
            if (go == null)
                go = new GameObject(ContainerNames.MazeItems);
            tr.SetParent(go.transform);
            var item = tr.gameObject.AddComponent<ViewMazeItemProt>();

            item.MazeSize = _MazeSize;
            item.Init(_Props);
            _Items.Add(item);
        }
        
        #endregion
    }
}