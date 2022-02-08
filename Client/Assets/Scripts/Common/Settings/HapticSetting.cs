﻿using Common.Entities;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface IHapticsSetting : ISetting<bool> { }
    
    public class HapticsSetting : SettingBase<bool>, IHapticsSetting
    {
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeysCommon.SettingHapticsOn;
        public override string            TitleKey     => "Haptics";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "setting_haptics_on";
        public override string            SpriteOffKey => "setting_haptics_off";
    }
}