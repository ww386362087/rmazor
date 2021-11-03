﻿#if UNITY_EDITOR
using DI.Extensions;
using Entities;
using GameHelpers;
using UnityEngine;
using Utils;

namespace Games.RazorMaze.Views.Debug
{
    public class ScreenViewDebug : MonoBehaviour
    {
        private static ScreenViewDebug _instance;
        public static ScreenViewDebug Instance =>
            _instance.IsNotNull() ? _instance : _instance = FindObjectOfType<ScreenViewDebug>(); 
        
        public MazeCoordinateConverter converter;

        private V2Int m_MazeSize;
        public V2Int MazeSize
        {
            set
            {
                var settings = PrefabUtilsEx.GetObject<ViewSettings>(
                    "model_settings", "view_settings");
                converter = new MazeCoordinateConverter(settings, null);
                converter.Init();
                converter.MazeSize = value;
            }
        }

        private void OnDrawGizmos()
        {
            if (converter == null)
                return;
            if (!converter.Initialized())
                return;
            var mazeBounds = converter.GetMazeBounds();
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(mazeBounds.center, mazeBounds.size);
            var offsets = converter.GetScreenOffsets();
            var scrBds = GraphicUtils.GetVisibleBounds();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(new Vector3(offsets.x, scrBds.min.y), new Vector3(offsets.x, scrBds.max.y));
            Gizmos.DrawLine(new Vector3(offsets.y, scrBds.min.y), new Vector3(offsets.y, scrBds.max.y));
            Gizmos.DrawLine(new Vector3(scrBds.min.x, offsets.z), new Vector3(scrBds.max.x, offsets.z));
            Gizmos.DrawLine(new Vector3(scrBds.min.x, offsets.w), new Vector3(scrBds.max.x, offsets.w));
        }
    }
}

#endif