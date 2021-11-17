﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Ticker
{
    public class TickerManager : MonoBehaviour
    {
        #region nonpublic members

        private bool m_Paused;
        private float m_Delta;
        private float m_FixedDelta;
        private readonly List<IUpdateTick> m_UpdateInfoDict = new List<IUpdateTick>();
        private readonly List<IFixedUpdateTick> m_FixedUpdateInfoDict = new List<IFixedUpdateTick>();
        private readonly List<ILateUpdateTick> m_LateUpdateInfoDict = new List<ILateUpdateTick>();
        private readonly List<IDrawGizmosTick> m_DrawGizmosInfoDict = new List<IDrawGizmosTick>();
        
        #endregion

        #region api

        public event UnityAction Paused;
        public event UnityAction UnPaused;
        public float             Time           { get; private set; }
        public float             DeltaTime      => UnityEngine.Time.deltaTime;
        public float             FixedTime      { get; private set; }
        public float             FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;

        public bool Pause
        {
            get => m_Paused;
            set
            {
                m_Paused = value;
                if (value)
                    Paused?.Invoke();
                else 
                    UnPaused?.Invoke();
            }
        }

        public void Reset()
        {
            Pause = false;
            Time = 0f;
            m_Delta = 0f;
        }
        
        public void RegisterObject(object _Object)
        {
            if (_Object == null)
                return;
            RegisterUpdateMethods(_Object);
        }

        public void UnregisterObject(object _Object)
        {
            switch (_Object)
            {
                case null:
                    return;
                case IUpdateTick onUpdateObj:
                    m_UpdateInfoDict.Remove(onUpdateObj);
                    break;
                case IFixedUpdateTick onFixedUpdateObj:
                    m_FixedUpdateInfoDict.Remove(onFixedUpdateObj);
                    break;
                case ILateUpdateTick onLateUpdateObj:
                    m_LateUpdateInfoDict.Remove(onLateUpdateObj);
                    break;
                case IDrawGizmosTick onDrawGizmosObj:
                    m_DrawGizmosInfoDict.Remove(onDrawGizmosObj);
                    break;
            }
        }

        public void Clear()
        {
            m_UpdateInfoDict.Clear();
            m_DrawGizmosInfoDict.Clear();
            m_FixedUpdateInfoDict.Clear();
            m_DrawGizmosInfoDict.Clear();
        }

        #endregion

        #region engine methods

        private void Update()
        {
            if (Pause)
            {
                m_Delta += UnityEngine.Time.deltaTime;
                return;
            }
            Time = UnityEngine.Time.time - m_Delta;
            if (Pause)
                return;
            for (int i = 0; i < m_UpdateInfoDict.Count; i++)
                m_UpdateInfoDict[i]?.UpdateTick();
        }

        private void FixedUpdate()
        {
            if (Pause)
            {
                m_FixedDelta += UnityEngine.Time.fixedTime;
                return;
            }
            FixedTime = UnityEngine.Time.fixedTime - m_FixedDelta;
            for (int i = 0; i < m_FixedUpdateInfoDict.Count; i++)
                m_FixedUpdateInfoDict[i]?.FixedUpdateTick();
        }

        private void LateUpdate()
        {
            if (Pause)
                return;
            for (int i = 0; i < m_LateUpdateInfoDict.Count; i++)
                m_LateUpdateInfoDict[i]?.LateUpdateTick();
        }
        
        private void OnDrawGizmos()
        {
            if (Pause)
                return;
            if (!m_DrawGizmosInfoDict.Any())
                return;
            foreach (var obj in m_DrawGizmosInfoDict)
                obj?.DrawGizmosTick();
        }

        #endregion

        #region nonpublic methods
        
        private void RegisterUpdateMethods(object _Object)
        {
            if (_Object is IUpdateTick onUpdateObj)
                m_UpdateInfoDict.Add(onUpdateObj);
            if (_Object is IFixedUpdateTick onFixedUpdateObj)
                m_FixedUpdateInfoDict.Add(onFixedUpdateObj);
            if (_Object is ILateUpdateTick onLateUpdateObj)
                m_LateUpdateInfoDict.Add(onLateUpdateObj);
            if (_Object is IDrawGizmosTick onDrawGizmosObj)
                m_DrawGizmosInfoDict.Add(onDrawGizmosObj);
            RemoveNullObjects();
        }

        private void RemoveNullObjects()
        {
            m_UpdateInfoDict.RemoveAll(_Obj => _Obj == null);
            m_FixedUpdateInfoDict.RemoveAll(_Obj => _Obj == null);
            m_LateUpdateInfoDict.RemoveAll(_Obj => _Obj == null);
            m_DrawGizmosInfoDict.RemoveAll(_Obj => _Obj == null);
        }

        #endregion
    }
}