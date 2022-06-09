﻿using System.Collections.Generic;
using System.Linq;
using Common.Entities;
using Common.Network.DataFieldFilters;
using Common.Utils;
using Newtonsoft.Json;
using UnityEngine.Events;

namespace RMAZOR.Managers
{
    public class RemoteConfigPropertyInfo
    {
        #region nonpublic members
        
        [JsonIgnore] private readonly GameDataFieldFilter m_Filter;

        #endregion

        #region ctor
        
        public RemoteConfigPropertyInfo(
            string              _Key,
            System.Type         _Type,
            GameDataFieldFilter _Filter,
            UnityAction<object> _SetProperty,
            bool                _IsJson = false)
        {
            Key              = _Key;
            Type             = _Type;
            IsJson           = _IsJson;
            m_Filter         = _Filter;
            SetPropertyValue = _SetProperty;
        }

        #endregion

        #region api
        
        [JsonProperty] public string      Key    { get; }
        [JsonProperty] public System.Type Type   { get; }
        [JsonProperty] public bool        IsJson { get; }

        
        [JsonIgnore] public UnityAction<object> SetPropertyValue { get; }

        [JsonIgnore]
        public Entity<object> GetCachedValueEntity
        {
            get
            {
                var entity = new Entity<object>();
                m_Filter.Filter(_Fields =>
                {
                    var field = GetField(_Fields, Key);
                    if (field?.GetValue() == null)
                    {
                        entity.Result = EEntityResult.Fail;
                        return;
                    }

                    entity.Value = field.GetValue();
                    entity.Result = EEntityResult.Success;
                });
                return entity;
            }
        }

        public void SetCachedValue(object _Value)
        {
            m_Filter.Filter(_Fields =>
            {
                var field = GetField(_Fields, Key);
                if (field == null)
                    return;
                field.SetValue(_Value).Save(true);
            });
        }

        #endregion

        #region nonpublic methods
        
        private static GameDataField GetField(IEnumerable<GameDataField> _Fields, string _FieldName)
        {
            ushort id = (ushort)CommonUtils.StringToHash(_FieldName);
            return _Fields.FirstOrDefault(_F => _F.FieldId == id);
        }

        #endregion
    }
}