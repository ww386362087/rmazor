﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Common;
using Common.Helpers;

namespace RMAZOR.Views.MazeItems.ViewMazeItemPath
{
    public interface IViewMazeItemPathExtraBordersSet : IInit, ICloneable
    {
        IList<IViewMazeItemPathExtraBorders> GetSet();
    }
    
    public class ViewMazeItemPathExtraBordersSet : InitBase, IViewMazeItemPathExtraBordersSet
    {
        #region nonpublic members

        private readonly IList<IViewMazeItemPathExtraBorders> m_Set = new List<IViewMazeItemPathExtraBorders>();

        #endregion
        
        #region inject

        private ViewSettings                   ViewSettings  { get; }
        private IViewMazeItemPathExtraBorders1 ExtraBorders1 { get; }
        private IViewMazeItemPathExtraBorders2 ExtraBorders2 { get; }
        private IViewMazeItemPathExtraBorders3 ExtraBorders3 { get; }
        private IViewMazeItemPathExtraBorders4 ExtraBorders4 { get; }
        private IViewMazeItemPathExtraBorders5 ExtraBorders5 { get; }

        private ViewMazeItemPathExtraBordersSet(
            ViewSettings                   _ViewSettings,
            IViewMazeItemPathExtraBorders1 _ExtraBorders1,
            IViewMazeItemPathExtraBorders2 _ExtraBorders2,
            IViewMazeItemPathExtraBorders3 _ExtraBorders3,
            IViewMazeItemPathExtraBorders4 _ExtraBorders4, 
            IViewMazeItemPathExtraBorders5 _ExtraBorders5)
        {
            ViewSettings      = _ViewSettings;
            ExtraBorders1     = _ExtraBorders1;
            ExtraBorders2     = _ExtraBorders2;
            ExtraBorders3     = _ExtraBorders3;
            ExtraBorders4     = _ExtraBorders4;
            ExtraBorders5     = _ExtraBorders5;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            var extraBordersInUse = ViewSettings.extraBordersIndices
                .Split(',')
                .Select(_S => Convert.ToInt32(_S))
                .ToList();
            m_Set.Clear();
            foreach (var borders in extraBordersInUse.Select(_Idx =>
                (IViewMazeItemPathExtraBorders) (_Idx switch
            {
                1 => ExtraBorders1,
                2 => ExtraBorders2,
                3 => ExtraBorders3,
                4 => ExtraBorders4,
                5 => ExtraBorders5,
                _ => throw new SwitchExpressionException(_Idx)
            })))
            {
                m_Set.Add(borders);
            }
            base.Init();
        }

        public IList<IViewMazeItemPathExtraBorders> GetSet()
        {
            if (!Initialized)
                Init();
            return m_Set;
        }

        public object Clone() =>
            new ViewMazeItemPathExtraBordersSet(
                ViewSettings,
                ExtraBorders1    .Clone() as IViewMazeItemPathExtraBorders1,
                ExtraBorders2    .Clone() as IViewMazeItemPathExtraBorders2,
                ExtraBorders3    .Clone() as IViewMazeItemPathExtraBorders3,
                ExtraBorders4    .Clone() as IViewMazeItemPathExtraBorders4,
                ExtraBorders5    .Clone() as IViewMazeItemPathExtraBorders5);

        #endregion
        

    }
}