﻿using System;
using System.Collections;
using Ticker;
using UnityEngine;
using UnityEngine.Events;

namespace Utils
{
    public static partial class Coroutines
    {
        public static IEnumerator Lerp(
            long _From,
            long _To,
            float _Time,
            UnityAction<long> _OnProgress,
            ITicker _Ticker,
            UnityAction<bool, long> _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_OnProgress == null)
                yield break;

            float currTime = _Ticker.Time;
            long progress = _From;
            bool breaked = false;
            while (_Ticker.Time < currTime + _Time)
            {
                if (_OnBreak != null && _OnBreak())
                {
                    breaked = true;
                    break;
                }
                if (_Ticker.Pause)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                
                float timeCoeff = 1 - (currTime + _Time - _Ticker.Time) / _Time;
                progress = MathUtils.Lerp(_From, _To, timeCoeff);
                _OnProgress(progress);
                yield return new WaitForEndOfFrame();
            }
            if (_OnBreak != null && _OnBreak())
                breaked = true;
            if (!breaked)
                _OnProgress(_To);
            _OnFinish?.Invoke(breaked, breaked ? progress : _To);
        }
        
        public static IEnumerator Lerp(
            float _From,
            float _To,
            float _Time,
            UnityAction<float> _OnProgress,
            ITicker _Ticker,
            UnityAction<bool, float> _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_OnProgress == null)
                yield break;

            float currTime = _Ticker.Time;
            float progress = _From;
            bool breaked = false;
            
            while (_Ticker.Time < currTime + _Time)
            {
                if (_OnBreak != null && _OnBreak())
                {
                    breaked = true;
                    break;
                }
                if (_Ticker.Pause)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                
                float timeCoeff = 1 - (currTime + _Time - _Ticker.Time) / _Time;
                progress = Mathf.Lerp(_From, _To, timeCoeff);
                _OnProgress(progress);
                yield return new WaitForEndOfFrame();
            }
            if (_OnBreak != null && _OnBreak())
                breaked = true;
            if (!breaked)
                _OnProgress(_To);
            _OnFinish?.Invoke(breaked, breaked ? progress : _To);
        }

        public static IEnumerator Lerp(
            int _From,
            int _To,
            float _Time,
            UnityAction<int> _OnProgress,
            ITicker _Ticker,
            UnityAction<bool, int> _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_OnProgress == null)
                yield break;
            float currTime = _Ticker.Time;
            int progress = _From;
            bool breaked = false;
            while (_Ticker.Time < currTime + _Time)
            {
                if (_OnBreak != null && _OnBreak())
                {
                    breaked = true;
                    break;
                }
                if (_Ticker.Pause)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                float timeCoeff = 1 - (currTime + _Time - _Ticker.Time) / _Time;
                progress = Mathf.RoundToInt(Mathf.Lerp(_From, _To, timeCoeff));
                _OnProgress(progress);
                yield return new WaitForEndOfFrame();
            }
            if (_OnBreak != null && _OnBreak())
                breaked = true;
            if (!breaked)
                _OnProgress(_To);
            _OnFinish?.Invoke(breaked, breaked ? progress : _To);
        }
        
        public static IEnumerator Lerp(
            Color _From,
            Color _To,
            float _Time,
            UnityAction<Color> _OnProgress,
            ITicker _Ticker,
            UnityAction<bool, Color> _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_OnProgress == null)
                yield break;
            float currTime = _Ticker.Time;
            Color progress = _From;
            bool breaked = false;
            while (_Ticker.Time < currTime + _Time)
            {
                if (_OnBreak != null && _OnBreak())
                {
                    breaked = true;
                    break;
                }
                
                if (_Ticker.Pause)
                {
                    yield return new WaitForEndOfFrame();
                    continue;
                }
                
                float timeCoeff = 1 - (currTime + _Time - _Ticker.Time) / _Time;
                float r = Mathf.Lerp(_From.r, _To.r, timeCoeff);
                float g = Mathf.Lerp(_From.g, _To.g, timeCoeff);
                float b = Mathf.Lerp(_From.b, _To.b, timeCoeff);
                float a = Mathf.Lerp(_From.a, _To.a, timeCoeff);
                progress = new Color(r, g, b, a);
                _OnProgress(progress);
                yield return new WaitForEndOfFrame();
            }
            if (_OnBreak != null && _OnBreak())
                breaked = true;
            if (!breaked)
                _OnProgress(_To);
            _OnFinish?.Invoke(breaked, breaked ? progress : _To);
        }

        public static IEnumerator Lerp(
            Vector2 _From,
            Vector2 _To,
            float _Time,
            UnityAction<Vector2> _OnProgress,
            ITicker _Ticker,
            UnityAction<bool, Vector2> _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_OnProgress == null)
                yield break;
            float currTime = _Ticker.Time;
            bool breaked = false;
            Vector2 progress = _From;
            while (_Ticker.Time < currTime + _Time)
            {
                if (_OnBreak != null && _OnBreak())
                {
                    breaked = true;
                    break;
                }
                
                float timeCoeff = 1 - (currTime + _Time - _Ticker.Time) / _Time;
                progress = Vector2.Lerp(_From, _To, timeCoeff);
                _OnProgress(progress);
                yield return new WaitForEndOfFrame();
            }
            if (_OnBreak != null && _OnBreak())
                breaked = true;
            if (!breaked)
                _OnProgress(_To);
            _OnFinish?.Invoke(breaked, breaked ? progress : _To);
        }
        
        public static IEnumerator Lerp(
            Vector3 _From,
            Vector3 _To,
            float _Time,
            UnityAction<Vector3> _OnProgress,
            ITicker _Ticker,
            UnityAction<bool, Vector3> _OnFinish = null,
            Func<bool> _OnBreak = null)
        {
            if (_OnProgress == null)
                yield break;
            bool breaked = false;
            Vector3 progress = _From;
            float currTime = _Ticker.Time;
            while (_Ticker.Time < currTime + _Time)
            {
                if (_OnBreak != null && _OnBreak())
                {
                    breaked = true;
                    break;
                }
                float timeCoeff = 1 - (currTime + _Time - _Ticker.Time) / _Time;
                progress = Vector3.Lerp(_From, _To, timeCoeff);
                _OnProgress(progress);
                yield return new WaitForEndOfFrame();
            }
            if (_OnBreak != null && _OnBreak())
                breaked = true;
            if (!breaked)
                _OnProgress(_To);
            _OnFinish?.Invoke(breaked, breaked ? progress : _To);
        }
    }
}