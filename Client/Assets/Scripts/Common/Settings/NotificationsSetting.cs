﻿using Common.Entities;
using UnityEngine.Events;

namespace Common.Settings
{
    public interface INotificationSetting : ISetting<bool> { }
    
    public class NotificationsSetting : SettingBase<bool>, INotificationSetting
    {
        public override UnityAction<bool> OnValueSet   { get; set; }
        public override SaveKey<bool>     Key          => SaveKeysCommon.SettingNotificationsOn;
        public override string            TitleKey     => "Notifications";
        public override ESettingLocation  Location     => ESettingLocation.MiniButtons;
        public override ESettingType      Type         => ESettingType.OnOff;
        public override string            SpriteOnKey  { get; }
        public override string            SpriteOffKey { get; }
    }
}