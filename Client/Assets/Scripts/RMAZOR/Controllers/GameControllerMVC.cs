﻿using System.Diagnostics;
using Common;
using Common.Helpers;
using Common.Utils;
using RMAZOR.Models;
using RMAZOR.Views;
using UnityEngine;
using Zenject;

namespace RMAZOR.Controllers
{
    public interface IGameController : IInit
    {
        IModelGame Model { get; }
        IViewGame  View  { get; }
    }
    
    // ReSharper disable once InconsistentNaming
    public sealed class GameControllerMVC : MonoBehInitBase, IGameController
    {
        #region factory
        
        public static IGameController CreateInstance()
        {
            var go = CommonUtils.FindOrCreateGameObject("Game Manager", out bool wasFound);
            var instance = wasFound ? go.GetComponent<GameControllerMVC>() 
                : go.AddComponent<GameControllerMVC>();
            return instance;
        }

        #endregion

        #region inject

        private IRemotePropertiesRmazor RemoteProperties   { get; set; }
        public  IModelGame              Model              { get; private set; }
        public  IViewGame               View               { get; private set; }

        [Inject]
        private void Inject(
            IRemotePropertiesRmazor _RemoteProperties,
            IModelGame              _Model,
            IViewGame               _View)
        {
            RemoteProperties   = _RemoteProperties;
            Model              = _Model;
            View               = _View;
        }

        #endregion
        
        #region api
        
        public override void Init()
        {
            InitDebugging();
            bool modelInitialized = false;
            bool viewInitialized = false;
            Model.Initialize += () => modelInitialized = true;
            View.Initialize += () => viewInitialized = true;
            
            Model.Init();

            Model.PathItemsProceeder.AllPathsProceededEvent += View.Character.OnAllPathProceed;
            Model.PathItemsProceeder.AllPathsProceededEvent += View.LevelStageController.OnAllPathProceed;
            Model.PathItemsProceeder.PathProceedEvent       += View.PathItemsGroup.OnPathProceed;
            Model.MazeRotation.RotationStarted              += View.MazeRotation.OnRotationStarted;
            Model.MazeRotation.RotationFinished             += View.MazeRotation.OnRotationFinished;
            Model.MazeRotation.RotationFinished             += View.TouchProceeder.OnRotationFinished;
            
            var mItemProcs = Model.ModelItemsProceedersSet;
            var vItemGrps = View.MazeItemsGroupSet;
            
            mItemProcs.GravityItemsProceeder.MazeItemMoveStarted   += vItemGrps.GravityItemsGroup.OnMazeItemMoveStarted;
            mItemProcs.GravityItemsProceeder.MazeItemMoveStarted   += View.UI.GameControls.OnMazeItemMoveStarted;
            mItemProcs.GravityItemsProceeder.MazeItemMoveContinued += vItemGrps.GravityItemsGroup.OnMazeItemMoveContinued;
            mItemProcs.GravityItemsProceeder.MazeItemMoveFinished  += vItemGrps.GravityItemsGroup.OnMazeItemMoveFinished;
            mItemProcs.GravityItemsProceeder.MazeItemMoveFinished  += View.UI.GameControls.OnMazeItemMoveFinished;
            
            mItemProcs.TrapsMovingProceeder.MazeItemMoveStarted    += vItemGrps.MovingItemsGroup.OnMazeItemMoveStarted;
            mItemProcs.TrapsMovingProceeder.MazeItemMoveStarted    += View.UI.GameControls.OnMazeItemMoveStarted;
            mItemProcs.TrapsMovingProceeder.MazeItemMoveContinued  += vItemGrps.MovingItemsGroup.OnMazeItemMoveContinued;
            mItemProcs.TrapsMovingProceeder.MazeItemMoveFinished   += vItemGrps.MovingItemsGroup.OnMazeItemMoveFinished;
            mItemProcs.TrapsMovingProceeder.MazeItemMoveFinished   += View.UI.GameControls.OnMazeItemMoveFinished;
            
            mItemProcs.TrapsReactProceeder.TrapReactStageChanged           += vItemGrps.TrapsReactItemsGroup.OnMazeTrapReactStageChanged;
            mItemProcs.TrapsIncreasingProceeder.TrapIncreasingStageChanged += vItemGrps.TrapsIncItemsGroup.OnMazeTrapIncreasingStageChanged;
            mItemProcs.TurretsProceeder.TurretShoot                        += vItemGrps.TurretsGroup.OnTurretShoot;
            mItemProcs.PortalsProceeder.PortalEvent                        += vItemGrps.PortalsGroup.OnPortalEvent;
            mItemProcs.ShredingerBlocksProceeder.ShredingerBlockEvent      += vItemGrps.ShredingerBlocksGroup.OnShredingerBlockEvent;
            mItemProcs.SpringboardProceeder.SpringboardEvent               += vItemGrps.SpringboardsGroup.OnSpringboardEvent;

            mItemProcs.HammersProceeder.HammerShot += vItemGrps.HammersGroup.OnHammerShot;
            mItemProcs.SpearsProceeder.SpearAppear += vItemGrps.SpearsGroup.OnSpearAppear;
            mItemProcs.SpearsProceeder.SpearShot   += vItemGrps.SpearsGroup.OnSpearShot;
            mItemProcs.DiodesProceeder.DiodeBlock  += vItemGrps.DiodesGroup.OnDiodeBlock;
            mItemProcs.DiodesProceeder.DiodePass   += vItemGrps.DiodesGroup.OnDiodePass;

            Model.Character.CharacterMoveStarted   += View.OnCharacterMoveStarted;
            Model.Character.CharacterMoveContinued += View.OnCharacterMoveContinued;
            Model.Character.CharacterMoveFinished  += View.OnCharacterMoveFinished;
            Model.LevelStaging.LevelStageChanged   += View.OnLevelStageChanged;
            
            View.CommandsProceeder.Command         += Model.InputScheduler.AddCommand;
            View.MazeRotation.RotationFinished     += Model.MazeRotation.OnRotationFinished;
            
            View.Init();
            Cor.Run(Cor.WaitWhile(
                () => !modelInitialized || !viewInitialized,
                () =>
                {
                    base.Init();
                }));
        }

        #endregion

        #region nonpublic members

        private void InitDebugging()
        {
            if (!RemoteProperties.DebugEnabled && !Application.isEditor)
                return;
            if (SRDebug.Instance == null)
                SRDebug.Init();
            SRLauncher.Init(
                    Model.Settings, 
                    View.Settings, 
                    Model.LevelStaging,
                    View.Managers, 
                    View.CommandsProceeder);
            View.Managers.DebugManager.Init();
        }

        #endregion
    }
}