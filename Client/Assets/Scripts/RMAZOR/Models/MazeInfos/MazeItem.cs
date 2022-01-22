﻿using System;
using System.Collections.Generic;
using Common.Entities;
using Newtonsoft.Json;
using UnityEngine;

namespace RMAZOR.Models.MazeInfos
{
    public enum EMazeItemType
    {
        Block,
        GravityBlock,
        ShredingerBlock,
        Portal,
        TrapReact,
        TrapIncreasing,
        TrapMoving,
        GravityTrap,
        Turret,
        GravityBlockFree,
        Springboard,
        MovingBlockFree
    }
    
    [Serializable]
    public class MazeItem
    {
        [JsonIgnore] [SerializeField] private EMazeItemType type = EMazeItemType.Block;
        [JsonIgnore] [SerializeField] private V2Int         position;
        [JsonIgnore] [SerializeField] private List<V2Int>   path       = new List<V2Int>();
        [JsonIgnore] [SerializeField] private List<V2Int>   directions = new List<V2Int>();
        [JsonIgnore] [SerializeField] private V2Int         pair;

        [JsonProperty(PropertyName = "T")]
        public EMazeItemType Type
        {
            get => type;
            set => type = value;
        }

        [JsonProperty(PropertyName = "P")]
        public V2Int Position
        {
            get => position;
            set => position = value;
        }

        [JsonProperty(PropertyName = "W")]
        public List<V2Int> Path
        {
            get => path;
            set => path = value;
        }

        [JsonProperty(PropertyName = "D2")]
        public List<V2Int> Directions
        {
            get => directions;
            set => directions = value;
        }

        [JsonProperty(PropertyName = "2")]
        public V2Int Pair
        {
            get => pair;
            set => pair = value;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Type.GetHashCode();
                hash = hash * 23 + Position.GetHashCode();
                hash = hash * 23 + Directions.GetHashCode();
                hash = hash * 23 + Pair.GetHashCode();
                foreach (var item in Path)
                    hash = hash * 23 + item.GetHashCode();
                return hash;
            }
        }
    }
}