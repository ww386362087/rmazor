﻿using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.MazeInfos;
using RMAZOR.Views.Common;
using RMAZOR.Views.MazeItems;

namespace RMAZOR.Views.MazeItemGroups
{
    public interface IViewMazeSpringboardItemsGroup : IViewMazeItemGroup
    {
        void OnSpringboardEvent(SpringboardEventArgs _Args);
    }
    
    public sealed class ViewMazeSpringboardItemsGroup : ViewMazeItemsGroupBase, IViewMazeSpringboardItemsGroup
    {
        public ViewMazeSpringboardItemsGroup(IViewMazeCommon _Common) : base(_Common) { }
        
        public override EMazeItemType[] Types => new[] {EMazeItemType.Springboard};
        
        public void OnSpringboardEvent(SpringboardEventArgs _Args)
        {
            var item = Common.GetItem<IViewMazeItemSpringboard>(_Args.Info);
            item.MakeJump(_Args);
        }

    }
}