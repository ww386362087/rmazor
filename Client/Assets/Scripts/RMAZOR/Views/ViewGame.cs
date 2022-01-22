﻿using System.Collections.Generic;
using System.Linq;
using Common;
using Common.CameraProviders;
using Managers;
using RMAZOR.Models;
using RMAZOR.Models.ItemProceeders;
using RMAZOR.Views.Characters;
using RMAZOR.Views.Common;
using RMAZOR.Views.ContainerGetters;
using RMAZOR.Views.InputConfigurators;
using RMAZOR.Views.MazeItemGroups;
using RMAZOR.Views.Rotation;
using RMAZOR.Views.UI;
using UnityEngine.Events;

namespace RMAZOR.Views
{
    public interface IViewGame :
        IInit,
        IOnLevelStageChanged,
        ICharacterMoveStarted,
        ICharacterMoveContinued,
        ICharacterMoveFinished
    {
        ViewSettings                   Settings              { get; }
        IContainersGetter              ContainersGetter      { get; }
        IViewUI                        UI                    { get; }
        IViewLevelStageController      LevelStageController  { get; }
        IViewInputController           InputController       { get; }
        IViewInputCommandsProceeder    CommandsProceeder     { get; }
        IViewCharacter                 Character             { get; }
        IViewMazeCommon                Common                { get; }
        IViewMazeBackground            Background            { get; }
        IViewMazeRotation              MazeRotation          { get; }
        IViewMazePathItemsGroup        PathItemsGroup        { get; }
        IViewMazeMovingItemsGroup      MovingItemsGroup      { get; }
        IViewMazeTrapsReactItemsGroup  TrapsReactItemsGroup  { get; }
        IViewMazeTrapsIncItemsGroup    TrapsIncItemsGroup    { get; }
        IViewMazeTurretsGroup          TurretsGroup          { get; }
        IViewMazePortalsGroup          PortalsGroup          { get; }
        IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }
        IManagersGetter                Managers              { get; }
    }
    
    public class ViewGame : IViewGame
    {
        #region inject

        public ViewSettings                   Settings              { get; }
        public IContainersGetter              ContainersGetter      { get; }
        public IViewUI                        UI                    { get; }
        public IViewLevelStageController      LevelStageController  { get; }
        public IViewInputController           InputController       { get; }
        public IViewInputCommandsProceeder    CommandsProceeder     { get; }
        public IViewCharacter                 Character             { get; }
        public IViewMazeCommon                Common                { get; }
        public IViewMazeBackground            Background            { get; }
        public IViewMazeRotation              MazeRotation          { get; }
        public IViewMazePathItemsGroup        PathItemsGroup        { get; }
        public IViewMazeMovingItemsGroup      MovingItemsGroup      { get; }
        public IViewMazeTrapsReactItemsGroup  TrapsReactItemsGroup  { get; }
        public IViewMazeTrapsIncItemsGroup    TrapsIncItemsGroup    { get; }
        public IViewMazeTurretsGroup          TurretsGroup          { get; }
        public IViewMazePortalsGroup          PortalsGroup          { get; }
        public IViewMazeShredingerBlocksGroup ShredingerBlocksGroup { get; }
        public IViewMazeSpringboardItemsGroup SpringboardItemsGroup { get; }
        public IViewMazeGravityItemsGroup     GravityItemsGroup     { get; }
        public IManagersGetter                Managers              { get; }

        private IMazeCoordinateConverter CoordinateConverter { get; }
        private IColorProvider           ColorProvider       { get; }
        private ICameraProvider          CameraProvider      { get; }

        public ViewGame(
            ViewSettings                          _Settings,
            IContainersGetter                     _ContainersGetter,
            IViewUI                               _UI,
            IViewLevelStageController             _LevelStageController,
            IViewInputController                  _InputController,
            IViewInputCommandsProceeder           _CommandsProceeder,
            IViewCharacter                        _Character,
            IViewMazeCommon                       _Common,
            IViewMazeBackground                   _Background,
            IViewMazeRotation                     _MazeRotation,
            IViewMazePathItemsGroup               _PathItemsGroup,
            IViewMazeMovingItemsGroup             _MovingItemsGroup,
            IViewMazeTrapsReactItemsGroup         _TrapsReactItemsGroup,
            IViewMazeTrapsIncItemsGroup           _TrapsIncItemsGroup,
            IViewMazeTurretsGroup                 _TurretsGroup,
            IViewMazePortalsGroup                 _PortalsGroup,
            IViewMazeShredingerBlocksGroup        _ShredingerBlocksGroup,
            IViewMazeSpringboardItemsGroup        _SpringboardItemsGroup,
            IViewMazeGravityItemsGroup            _GravityItemsGroup,
            IManagersGetter                       _Managers, 
            IMazeCoordinateConverter              _CoordinateConverter,
            IColorProvider                        _ColorProvider,
            ICameraProvider                       _CameraProvider)
        {
            Settings               = _Settings;
            ContainersGetter       = _ContainersGetter;
            UI                     = _UI;
            InputController        = _InputController;
            LevelStageController   = _LevelStageController;
            CommandsProceeder      = _CommandsProceeder;
            Character              = _Character;
            Common                 = _Common;
            Background             = _Background;
            MazeRotation           = _MazeRotation;
            PathItemsGroup         = _PathItemsGroup;
            MovingItemsGroup       = _MovingItemsGroup;
            TrapsReactItemsGroup   = _TrapsReactItemsGroup;
            TrapsIncItemsGroup     = _TrapsIncItemsGroup;
            TurretsGroup           = _TurretsGroup;
            PortalsGroup           = _PortalsGroup;
            ShredingerBlocksGroup  = _ShredingerBlocksGroup;
            SpringboardItemsGroup  = _SpringboardItemsGroup;
            GravityItemsGroup      = _GravityItemsGroup;
            Managers               = _Managers;
            CoordinateConverter    = _CoordinateConverter;
            ColorProvider          = _ColorProvider;
            CameraProvider         = _CameraProvider;
        }
        
        #endregion

        #region api

        public bool              Initialized { get; private set; }
        public event UnityAction Initialize;
        
        public void Init()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Managers.DebugManager.Init();
#endif
            Managers.AudioManager.Init();
            Managers.AssetBundleManager.Init();
            ColorProvider.Init();
            CoordinateConverter.Init();
            LevelStageController.RegisterProceeders(GetInterfaceOfProceeders<IOnLevelStageChanged>());
            GetInterfaceOfProceeders<IInit>().ForEach(_InitObj => _InitObj.Init());
            LevelStageController.Init();
            Initialize?.Invoke();
            Initialized = true;
        }
        
        public void OnLevelStageChanged(LevelStageArgs _Args)
        {
            LevelStageController.OnLevelStageChanged(_Args);
        }
        
        public void OnCharacterMoveStarted(CharacterMovingStartedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveStarted>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveStarted(_Args);
        }

        public void OnCharacterMoveContinued(CharacterMovingContinuedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveContinued>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveContinued(_Args);
        }
        
        public void OnCharacterMoveFinished(CharacterMovingFinishedEventArgs _Args)
        {
            var proceeders = GetInterfaceOfProceeders<ICharacterMoveFinished>();
            foreach (var proceeder in proceeders)
                proceeder.OnCharacterMoveFinished(_Args);
        }

        #endregion
        
        #region nonpublic methods

        private List<T> GetInterfaceOfProceeders<T>() where T : class
        {
            var proceeders = new List<object>
                {
                    ContainersGetter,
                    Common,
                    UI,                         
                    InputController,
                    Character,
                    MazeRotation,
                    PathItemsGroup,
                    MovingItemsGroup,
                    TrapsReactItemsGroup,
                    TrapsIncItemsGroup,
                    TurretsGroup,
                    PortalsGroup,
                    ShredingerBlocksGroup,
                    SpringboardItemsGroup,
                    GravityItemsGroup,
                    Background,
                    CameraProvider,
                }.Where(_Proceeder => _Proceeder != null);
            return proceeders.Where(_Proceeder => _Proceeder is T).Cast<T>().ToList();
        }
        
        #endregion
    }
}