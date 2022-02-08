﻿using Common.Entities;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface IMusicSetting : ISetting<bool> { }
    
    public class MusicSetting : SettingBase<bool>, IMusicSetting
    {
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeysCommon.SettingMusicOn;
        public override string            TitleKey     => "Music";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  => "setting_music_on";
        public override string            SpriteOffKey => "setting_music_off";
    }
}