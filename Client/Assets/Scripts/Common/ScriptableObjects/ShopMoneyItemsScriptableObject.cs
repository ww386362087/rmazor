﻿using System;
using Common.Helpers;
using Common.Helpers.Attributes;
using UnityEngine;

namespace Common.ScriptableObjects
{
    [CreateAssetMenu(fileName = "shop_money_items_set", menuName = "Configs and Sets/Shop Money Items Set")]
    public class ShopMoneyItemsScriptableObject : ScriptableObject
    {
        [Serializable]
        public class MoneyItem
        {
            public int    purchaseKey;
            public int    reward;
            public bool   watchingAds;
        }
        
        [Serializable]
        public class MoneyItemsSet : ReorderableArray<MoneyItem> { }

        [Header("Set"), Reorderable(paginate = true, pageSize = 10)]
        public MoneyItemsSet set;
    }
}