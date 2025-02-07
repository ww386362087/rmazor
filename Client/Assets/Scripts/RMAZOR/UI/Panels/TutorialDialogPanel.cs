﻿using System.Collections;
using System.Globalization;
using System.Linq;
using System.Text;
using Common.Helpers;
using mazing.common.Runtime;
using mazing.common.Runtime.CameraProviders;
using mazing.common.Runtime.Constants;
using mazing.common.Runtime.Enums;
using mazing.common.Runtime.Extensions;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Providers;
using mazing.common.Runtime.Ticker;
using mazing.common.Runtime.UI;
using mazing.common.Runtime.Utils;
using RMAZOR.Managers;
using RMAZOR.Models;
using RMAZOR.Views.Common;
using RMAZOR.Views.Common.ViewLevelStageSwitchers;
using RMAZOR.Views.InputConfigurators;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;

namespace RMAZOR.UI.Panels
{
    public class TutorialDialogPanelInfo
    {
        public string TutorialName               { get; }
        public string TitleLocalizationKey       { get; }
        public string DescriptionLocalizationKey { get; }
        public string VideoClipAssetKey          { get; }
        
        public TutorialDialogPanelInfo(
            string _TutorialName,
            string _TitleLocalizationKey, 
            string _DescriptionLocalizationKey,
            string _VideoClipAssetKey)
        {
            TutorialName               = _TutorialName;
            TitleLocalizationKey       = _TitleLocalizationKey;
            DescriptionLocalizationKey = _DescriptionLocalizationKey;
            VideoClipAssetKey          = _VideoClipAssetKey;
        }
    }
    
    public interface ITutorialDialogPanel : IDialogPanel
    {
        UnityAction OnPanelCloseAction { get; set; }
        bool        IsVideoReady       { get; }
        void        SetPanelInfo(TutorialDialogPanelInfo _Info);
        void        PrepareVideo();
    }
    
    public class TutorialDialogPanel : DialogPanelBase, ITutorialDialogPanel
    {
        #region nonpublic members

        private TutorialDialogPanelInfo m_Info;
        private TextMeshProUGUI         m_Title;
        private TextMeshProUGUI         m_Description;
        private VideoPlayer             m_VideoPlayer;
        private Button                  m_ButtonClose;
        private Animator                m_Animator;
        private SimpleUiItem            m_PanelView;

        private IEnumerator m_PrintCoroutine;
        
        protected override string PrefabName => "tutorial_panel";

        #endregion

        #region inject

        private GlobalGameSettings      GlobalGameSettings { get; }
        private IContainersGetter       ContainersGetter   { get; }
        private IViewLevelStageSwitcher LevelStageSwitcher { get; }

        private TutorialDialogPanel(
            GlobalGameSettings          _GlobalGameSettings,
            IManagersGetter             _Managers,
            IUITicker                   _Ticker,
            ICameraProvider             _CameraProvider,
            IColorProvider              _ColorProvider,
            IViewTimePauser             _TimePauser,
            IContainersGetter           _ContainersGetter,
            IViewInputCommandsProceeder _CommandsProceeder,
            IViewLevelStageSwitcher     _LevelStageSwitcher) 
            : base(
                _Managers, 
                _Ticker,
                _CameraProvider,
                _ColorProvider,
                _TimePauser,
                _CommandsProceeder)
        {
            GlobalGameSettings = _GlobalGameSettings;
            ContainersGetter   = _ContainersGetter;
            LevelStageSwitcher = _LevelStageSwitcher;
        }

        #endregion

        #region api
        
        public override    int      DialogViewerId => DialogViewerIdsCommon.FullscreenCommon;
        public override    Animator Animator       => m_Animator;

        public UnityAction OnPanelCloseAction { get; set; }
        public bool        IsVideoReady       => m_VideoPlayer.IsNotNull() && m_VideoPlayer.isPrepared;

        public void SetPanelInfo(TutorialDialogPanelInfo _Info)
        {
            m_Info = _Info;
        }

        public void PrepareVideo()
        {
            if (m_VideoPlayer.IsNull())
                InitVideoPlayer();
#if UNITY_WEBGL
            m_VideoPlayer.url = GlobalGameSettings.urlOtherAssets
                                + "/videos/" 
                                + m_Info.VideoClipAssetKey + ".webm"; 
#else
            var clip = Managers.PrefabSetManager.GetObject<VideoClip>(
                "tutorial_clips", m_Info.VideoClipAssetKey);
            m_VideoPlayer.clip = clip;
#endif
            m_VideoPlayer.enabled = true;
            m_VideoPlayer.Prepare();
        }

        #endregion

        #region nonpublic methods
        
        protected override void LoadPanelCore(RectTransform _Container, ClosePanelAction _OnClose)
        {
            base.LoadPanelCore(_Container, _OnClose);
            m_PanelView.Init(Ticker, Managers.AudioManager, Managers.LocalizationManager);
        }
        
        protected override void OnDialogStartAppearing()
        {
            m_VideoPlayer.Play();
            TimePauser.PauseTimeInGame();
            var font =  Managers.LocalizationManager.GetFont(ETextType.MenuUI_H1);
            m_Title.font = m_Description.font = font;
            m_Title.text = m_Description.text = string.Empty;
            base.OnDialogStartAppearing();
        }

        protected override void OnDialogAppeared()
        {
            base.OnDialogAppeared();
            var locMan = Managers.LocalizationManager;
            string titleTextLocalized = locMan.GetTranslation(m_Info.TitleLocalizationKey)
                .FirstCharToUpper(CultureInfo.CurrentUICulture);
            string descriptionTextLocalized = locMan.GetTranslation(m_Info.DescriptionLocalizationKey)
                .FirstCharToUpper(CultureInfo.CurrentUICulture);
            string tutorialWordLocalized = locMan.GetTranslation("tutorial")
                .FirstCharToUpper(CultureInfo.CurrentUICulture);
            int currentTutorialNumber = GetNumberOfFinishedTutorials() + 1;
            titleTextLocalized = $"{tutorialWordLocalized} #{currentTutorialNumber}: {titleTextLocalized}";
            m_PrintCoroutine = PrintTutorialTextCoroutine(titleTextLocalized, descriptionTextLocalized);
            Cor.Run(m_PrintCoroutine);
        }

        protected override void OnDialogDisappeared()
        {
            Cor.Stop(m_PrintCoroutine);
            m_Title.text          = string.Empty;
            m_Description.text    = string.Empty;
            m_VideoPlayer.Stop();
            m_VideoPlayer.clip    = null;
            m_VideoPlayer.enabled = false;
            TimePauser.UnpauseTimeInGame();
            base.OnDialogDisappeared();
        }

        protected override void GetPrefabContentObjects(GameObject _Go)
        {
            m_PanelView   = _Go.GetCompItem<SimpleUiItem>("panel");
            m_Title       = _Go.GetCompItem<TextMeshProUGUI>("title");
            m_Description = _Go.GetCompItem<TextMeshProUGUI>("description");
            m_ButtonClose = _Go.GetCompItem<Button>("button_close");
            m_Animator    = _Go.GetCompItem<Animator>("animator");
        }

        protected override void LocalizeTextObjectsOnLoad() { }

        protected override void SubscribeButtonEvents()
        {
            m_ButtonClose.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnCloseButtonClick()
        {
            OnClose(OnPanelCloseAction);
        }

        private void InitVideoPlayer()
        {
            var vpParent = ContainersGetter.GetContainer(ContainerNamesCommon.Common);
            var vpGo = Managers.PrefabSetManager.InitPrefab(
                vpParent, "tutorials", "tutorial_video_player");
            vpGo.transform.SetLocalPosXY(Vector2.left * 1e3f);
            m_VideoPlayer = vpGo.GetCompItem<VideoPlayer>("video_player");
            m_VideoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        }

        private IEnumerator PrintTutorialTextCoroutine(string _Title, string _Description)
        {
            var titleCharArray = _Title.ToCharArray();
            var sb = new StringBuilder(titleCharArray.Length);
            float printTime = _Title.Length * 0.05f;
            yield return Cor.Lerp(Ticker, printTime, _OnProgress: _P =>
            {
                sb.Clear();
                int textLength = Mathf.RoundToInt(titleCharArray.Length * _P);
                for (int i = 0; i < textLength; i++)
                    sb.Append(titleCharArray[i]);
                m_Title.text = sb.ToString();
            });
            var descriptionCharArray = _Description.ToCharArray();
            printTime = _Description.Length * 0.05f;
            yield return Cor.Lerp(Ticker, printTime, _OnProgress: _P =>
            {
                sb.Clear();
                int textLength = Mathf.RoundToInt(descriptionCharArray.Length * _P);
                for (int i = 0; i < textLength; i++)
                    sb.Append(descriptionCharArray[i]);
                m_Description.text = sb.ToString();
            });
        }

        private static int GetNumberOfFinishedTutorials()
        {
            return SaveKeysRmazor.GetAllTutorialNames()
                .Except(new [] {"main_menu"})
                .Select(SaveKeysRmazor.IsTutorialFinished)
                .Select(SaveUtils.GetValue)
                .Count(_TutorialFinished => _TutorialFinished);
        }
        
        
        #endregion
    }
}