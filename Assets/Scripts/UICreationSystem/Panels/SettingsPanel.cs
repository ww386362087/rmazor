﻿using UnityEngine;
using UICreationSystem.Factories;
using Utils;
using Settings;

namespace UICreationSystem.Panels
{
    public class SettingsPanel
    {
        private RectTransform m_Content;
        private RectTransform m_SettingsPanel;
        private IDialogViewer m_DialogViewer;

        private RectTransformLite SettingRectLite => new RectTransformLite
        {
            Anchor = UiAnchor.Create(0, 1, 0, 1),
            AnchoredPosition = new Vector2(213f, -54.6f),
            Pivot = Vector2.one * 0.5f,
            SizeDelta = new Vector2(406f, 87f)
        };
        
        public RectTransform CreatePanel(IDialogViewer _DialogViewer)
        {
            GameObject sp = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    _DialogViewer.DialogContainer,
                    RtrLites.FullFill),
                "main_menu", "settings_panel");

            m_Content = sp.GetComponentItem<RectTransform>("content");
            m_DialogViewer = _DialogViewer;
            m_SettingsPanel = sp.RTransform();
            InitSettingItems();
            return m_SettingsPanel;
        }
        
        private void InitSettingItems()
        {
            InitSettingItem(new SoundSetting());
            InitSettingItem(new LanguageSetting());
        }
        
        private void InitSettingItem(ISetting _Setting)
        {
            switch (_Setting.Type)
            {
                case SettingType.OnOff:
                    var itemOnOff = CreateOnOffSetting();
                    itemOnOff.Init((bool)_Setting.Get(), _Setting.Name, _IsOn =>
                    {
                        _Setting.Put(_IsOn);
                    });
                    break;
                case SettingType.InPanelSelector:
                    var itemSelector = CreateInPanelSelectorSetting();
                    itemSelector.Init(
                        m_SettingsPanel,
                        m_DialogViewer,
                        () => (string)_Setting.Get(),
                        _Setting.Name,
                        () => _Setting.Values,
                        _Value =>
                        {
                            itemSelector.setting.text = _Value;
                            _Setting.Put(_Value);
                        });
                    break;
                case SettingType.Slider:
                    var itemSlider = CreateSliderSetting();
                    bool wholeNumbers = _Setting.Get() is int;
                    if (wholeNumbers)
                        itemSlider.Init(_Setting.Name, (float)_Setting.Min, (float)_Setting.Max, (float)_Setting.Get());
                    else
                        itemSlider.Init(_Setting.Name, (int)_Setting.Min, (int)_Setting.Max, (int)_Setting.Get());
                    break;
            }
        }

        private SettingItemOnOff CreateOnOffSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingRectLite),
                "setting_items", "on_off_item");
            return obj.GetComponent<SettingItemOnOff>();
        }

        private SettingItemInPanelSelector CreateInPanelSelectorSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingRectLite),
                "setting_items", "in_panel_selector_item");
            return obj.GetComponent<SettingItemInPanelSelector>();
        }


        private SettingItemSlider CreateSliderSetting()
        {
            GameObject obj = PrefabInitializer.InitUiPrefab(
                UiFactory.UiRectTransform(
                    m_Content,
                    SettingRectLite),
                "setting_items", "slider_item");
            return obj.GetComponent<SettingItemSlider>();
        }
    }
}