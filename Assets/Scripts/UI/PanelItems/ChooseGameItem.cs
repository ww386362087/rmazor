﻿using Extensions;
using Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PanelItems
{
    public class ChooseGameItem : MonoBehaviour
    {
        public Button button;
        public Image icon;
        public TextMeshProUGUI comingSoonLabel;
    
        public void Init(ChooseGameItemProps _Props)
        {
            icon.sprite = GetLogo(_Props.GameId);
            button.interactable = !_Props.IsComingSoon;
            button.SetOnClick(() =>
            {
                SoundManager.Instance.PlayMenuButtonClick();
                _Props.Click?.Invoke();
            });
            comingSoonLabel.enabled = _Props.IsComingSoon;
        }
        
        private Sprite GetLogo(int _GameId)
        {
            return PrefabInitializer.GetObject<Sprite>("game_logos", $"game_logo_{_GameId}");
        }
    }

    public class ChooseGameItemProps
    {
        public int GameId { get; }
        public bool IsComingSoon { get; }
        public bool IsVisible { get; }
        public UnityEngine.Events.UnityAction Click { get; set; }

        public ChooseGameItemProps(
            int _GameId,
            bool _IsComingSoon,
            bool _IsVisible)
        {
            GameId = _GameId;
            IsComingSoon = _IsComingSoon;
            IsVisible = _IsVisible;
        }
    }
}