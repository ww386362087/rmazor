﻿using System.Collections.Generic;
using System.Linq;
using Constants;
using Entities;
using Helpers;
using UnityEngine;
using Utils;

namespace Managers
{
    public class SoundManager : MonoBehaviour, ISingleton
    {
        #region singleton
    
        private static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (_instance is SoundManager ptm && !ptm.IsNull())
                    return _instance;
                var obj = new GameObject("Sound Manager");
                _instance = obj.AddComponent<SoundManager>();
                return _instance;
            }
        }
    
        #endregion
    
        #region factory

        public void PlayUiButtonClick()
        {
            PlayClip("ui_button_click", false);
        }
    
        #endregion
    
        #region private members
    
        private readonly Dictionary<GameObject,AudioSource> m_clipDictionary
            = new Dictionary<GameObject, AudioSource>();

        #endregion

        #region api
    
        public void SwitchSound(bool _IsOn)
        {
            SaveUtils.PutValue(SaveKey.SettingSoundOn, _IsOn);
        }

        public void PlayClip(string _Name, bool _Cycling, float? _Volume = null)
        {
            AudioClip clip = PrefabInitializer.GetObject<AudioClip>("sounds", _Name);
            PlayClipCore(clip, _Cycling, _Volume);
        }

        public void SwitchSoundInActualClips(bool _IsOn)
        {
            foreach (var clip in m_clipDictionary
                .Where(_Clip => _Clip.Key != null))
                clip.Value.volume = _IsOn ? 1 : 0;
        }

        public void StopPlayingClips()
        {
            foreach (var clip in m_clipDictionary.Values.ToArray())
                clip.Stop();
        }

        #endregion
        
        #region nonpublic methods

        private void PlayClipCore(AudioClip _Clip, bool _Cycling, float? _Volume = null)
        {
            GameObject go = new GameObject($"AudioClip_{_Clip.name}");
            AudioSource audioSource = go.AddComponent<AudioSource>();
            audioSource.clip = _Clip;
            audioSource.volume = (_Volume ?? 1f) * (SaveUtils.GetValue<bool>(SaveKey.SettingSoundOn) ? 1 : 0);
            audioSource.loop = _Cycling;
            m_clipDictionary.Add(go, audioSource);

            Coroutines.Run(Coroutines.WaitEndOfFrame(() =>
            {
                Coroutines.Run(Coroutines.WaitWhile(() =>
                {
                    m_clipDictionary.Remove(go);
                    Destroy(go);
                }, () => audioSource.isPlaying));
            }));
            audioSource.Play();
        }
        
        #endregion
    }
}