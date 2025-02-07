﻿using Common.Managers.Advertising;
using mazing.common.Runtime.Managers;
using mazing.common.Runtime.Ticker;
using UnityEngine.Events;

namespace RMAZOR.UI.PanelItems.Shop_Panel_Items
{
    public class ShopMoneyItem : ShopItemBase
    {
        public override void Init(
            IUITicker            _UITicker,
            IAudioManager        _AudioManager,
            ILocalizationManager _LocalizationManager,
            IAdsManager          _AdsManager,
            UnityAction          _Click,
            ViewShopItemInfo     _Info)
        {
            base.Init(_UITicker, _AudioManager, _LocalizationManager, _AdsManager, _Click, _Info);
            if (_Info.Reward > 0)
                title.text = _Info.Reward.ToString();
        }
    }
}