﻿using System.Collections.Generic;
using System.Linq;
using mazing.common.Runtime;
using mazing.common.Runtime.Helpers;
using mazing.common.Runtime.Managers;
using RMAZOR.Helpers;
using RMAZOR.Views.Common.FullscreenTextureProviders;
using UnityEngine;

namespace RMAZOR.Views.Common
{
    public interface IBackgroundTextureProviderSet : IInit
    {
        IFullscreenTextureProvider        GetProvider(string _Name);
        IList<IFullscreenTextureProvider> GetSet();
    }

    public class BackgroundTextureProviderSetImpl : InitBase, IBackgroundTextureProviderSet
    {
        #region nonpublic members

        private readonly Dictionary<string, IFullscreenTextureProvider> m_TextureProvidersDict
            = new Dictionary<string, IFullscreenTextureProvider>();

        #endregion

        #region inject
        
        private IFullscreenTextureProviderCustom     TextureProviderCustom     { get; }
        private IFullscreenTextureProviderTriangles2 TextureProviderTriangles2 { get; }
        private IFullscreenTextureProviderSynthwave  TextureProviderSynthwave  { get; }
        private IFullscreenTextureProviderEmpty      TextureProviderEmpty      { get; }
        private IPrefabSetManager                    PrefabSetManager          { get; }

        public BackgroundTextureProviderSetImpl(
            IFullscreenTextureProviderCustom     _TextureProviderCustom,
            IFullscreenTextureProviderTriangles2 _TextureProviderTriangles2,
            IFullscreenTextureProviderSynthwave  _TextureProviderSynthwave,
            IFullscreenTextureProviderEmpty      _TextureProviderEmpty,
            IPrefabSetManager                    _PrefabSetManager)
        {
            TextureProviderCustom     = _TextureProviderCustom;
            TextureProviderTriangles2 = _TextureProviderTriangles2;
            TextureProviderSynthwave  = _TextureProviderSynthwave;
            TextureProviderEmpty      = _TextureProviderEmpty;
            PrefabSetManager          = _PrefabSetManager;
        }

        #endregion

        #region api

        public override void Init()
        {
            if (Initialized)
                return;
            InitTextureProviders();
            base.Init();
        }

        public IFullscreenTextureProvider GetProvider(string _Name)
        {
            string name = m_TextureProvidersDict.ContainsKey(_Name) ? _Name : "solid";
            return m_TextureProvidersDict[name];
        }

        public IList<IFullscreenTextureProvider> GetSet()
        {
            return m_TextureProvidersDict.Values.ToList();
        }

        #endregion

        #region nonpublic members

        private void InitTextureProviders()
        {
            var setRawScrObj = PrefabSetManager.GetObject<MainBackgroundMaterialInfoSetScriptableObject>(
                "background", "main_background_set", EPrefabSource.Bundle);
            foreach (var setItem in setRawScrObj.set)
            {
                if (m_TextureProvidersDict.ContainsKey(setItem.name))
                    continue;
                IFullscreenTextureProvider provider = setItem.name switch
                {
                    "triangles_2" => TextureProviderTriangles2,
                    "empty"       => TextureProviderEmpty,
                    "synthwave"   => TextureProviderSynthwave,
                    _             => (IFullscreenTextureProvider) TextureProviderCustom.Clone()
                };
                var material = Object.Instantiate(setItem.material);
                provider.SetMaterial(material);
                provider.Init();
                m_TextureProvidersDict.Add(setItem.name, provider);
            }
        }
        
        #endregion
    }
}