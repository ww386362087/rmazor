﻿using System.Linq;
using Common;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Models.ItemProceeders.Additional;
using RMAZOR.Models.ProceedInfos;
using RMAZOR.Views;

namespace RMAZOR.Models
{
    public interface IModelItemsProceedersSet :
        IInit,
        IGetAllProceedInfos,
        IOnLevelStageChanged,
        ICharacterMoveStarted, 
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        ITrapsMovingProceeder      TrapsMovingProceeder      { get; }
        IGravityItemsProceeder     GravityItemsProceeder     { get; }
        ITrapsReactProceeder       TrapsReactProceeder       { get; }
        ITrapsIncreasingProceeder  TrapsIncreasingProceeder  { get; }
        ITurretsProceeder          TurretsProceeder          { get; }
        IPortalsProceeder          PortalsProceeder          { get; }
        IShredingerBlocksProceeder ShredingerBlocksProceeder { get; }
        ISpringboardProceeder      SpringboardProceeder      { get; }
        IHammersProceeder          HammersProceeder          { get; }
        ISpearsProceeder           SpearsProceeder           { get; }
        IDiodesProceeder           DiodesProceeder           { get; }
        IKeyLockMazeItemsProceeder KeyLockMazeItemsProceeder { get; }
        
        IItemsProceeder[] GetProceeders();
    }
    
    public class ModelItemsProceedersSet : InitBase, IModelItemsProceedersSet
    {
        #region nonpublic members

        private IItemsProceeder[] m_ProceedersCached;

        #endregion

        #region inject
        
        public ITrapsMovingProceeder      TrapsMovingProceeder      { get; }
        public IGravityItemsProceeder     GravityItemsProceeder     { get; }
        public ITrapsReactProceeder       TrapsReactProceeder       { get; }
        public ITrapsIncreasingProceeder  TrapsIncreasingProceeder  { get; }
        public ITurretsProceeder          TurretsProceeder          { get; }
        public IPortalsProceeder          PortalsProceeder          { get; }
        public IShredingerBlocksProceeder ShredingerBlocksProceeder { get; }
        public ISpringboardProceeder      SpringboardProceeder      { get; }
        public IHammersProceeder          HammersProceeder          { get; }
        public ISpearsProceeder           SpearsProceeder           { get; }
        public IDiodesProceeder           DiodesProceeder           { get; }
        public IKeyLockMazeItemsProceeder KeyLockMazeItemsProceeder { get; }

        private ModelItemsProceedersSet(
            ITrapsMovingProceeder      _TrapsMovingProceeder,
            IGravityItemsProceeder     _GravityItemsProceeder,
            ITrapsReactProceeder       _TrapsReactProceeder,
            ITrapsIncreasingProceeder  _TrapsIncreasingProceeder,
            ITurretsProceeder          _TurretsProceeder,
            IPortalsProceeder          _PortalsProceeder,
            IShredingerBlocksProceeder _ShredingerBlocksProceeder,
            ISpringboardProceeder      _SpringboardProceeder,
            IHammersProceeder          _HammersProceeder,
            ISpearsProceeder           _SpearsProceeder,
            IDiodesProceeder           _DiodesProceeder, 
            IKeyLockMazeItemsProceeder _KeyLockMazeItemsProceeder)
        {
            TrapsMovingProceeder      = _TrapsMovingProceeder;
            GravityItemsProceeder     = _GravityItemsProceeder;
            TrapsReactProceeder       = _TrapsReactProceeder;
            TrapsIncreasingProceeder  = _TrapsIncreasingProceeder;
            TurretsProceeder          = _TurretsProceeder;
            PortalsProceeder          = _PortalsProceeder;
            ShredingerBlocksProceeder = _ShredingerBlocksProceeder;
            SpringboardProceeder      = _SpringboardProceeder;
            HammersProceeder          = _HammersProceeder;
            SpearsProceeder           = _SpearsProceeder;
            DiodesProceeder           = _DiodesProceeder;
            KeyLockMazeItemsProceeder = _KeyLockMazeItemsProceeder;
        }

        #endregion

        #region api

        public System.Func<IMazeItemProceedInfo[]> GetAllProceedInfos
        {
            set
            {
                foreach (var item in GetInterfaceOfProceeders<IGetAllProceedInfos>()
                    .Where(_Item => _Item != null))
                {
                    item.GetAllProceedInfos = value;
                }
            }
        }

        public override void Init()
        {
            foreach (var item in GetInterfaceOfProceeders<IInit>().Where(_Item => _Item != null))
                item.Init();
            base.Init();
        }

        public IItemsProceeder[] GetProceeders()
        {
            if (m_ProceedersCached != null)
                return m_ProceedersCached;
            m_ProceedersCached = new IItemsProceeder[]
            {
                TrapsMovingProceeder,
                GravityItemsProceeder,
                TrapsReactProceeder,
                TrapsIncreasingProceeder,
                TurretsProceeder,
                PortalsProceeder,
                ShredingerBlocksProceeder,
                SpringboardProceeder,
                HammersProceeder,
                SpearsProceeder,
                DiodesProceeder,
                KeyLockMazeItemsProceeder
            };
            return m_ProceedersCached;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            var groups = GetProceeders();
            foreach (var g in groups)
                g.OnLevelStageChanged(_Args);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveStarted>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveStarted(_Args);
        }

        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveContinued(_Args);
        }

        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveFinished>();
            foreach (var proceeder in proceeders)
                proceeder?.OnCharacterMoveFinished(_Args);
        }

        #endregion

        #region nonpublic methods
        
        private T[] GetInterfaceOfProceeders<T>() where T : class
        {
            return System.Array.ConvertAll(GetProceeders(), _Item => _Item as T);
        }

        #endregion
    }
}