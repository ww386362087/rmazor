﻿using System;
using Common;
using Common.CameraProviders;
using Common.CameraProviders.Camera_Effects_Props;
using Common.Constants;
using Common.Exceptions;
using Common.Extensions;
using Common.Helpers;
using Common.Managers;
using Common.Ticker;
using Common.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RMAZOR.Camera_Providers
{
    public class CameraProviderRmazor : InitBase, ICameraProvider
    {
        #region constants

        private const string PrefabSetName = "camera_effects";
        
        #endregion
        
        #region nonpublic members

        protected bool      LevelCameraInitialized;
        protected Transform LevelCameraTr;
        protected Camera    LevelCamera;
        protected bool      FollowTransformIsNotNull;
        private   Transform m_FollowTr;
        
        private   FastDOF             m_DepthOfField;
        private   FastGlitch          m_FastGlitch;
        private   ChromaticAberration m_ChromaticAberration;
        private   ColorGrading        m_ColorGrading;
        private   Pixellate           m_Pixelate;
        private   FXAA                m_Fxaa;

        #endregion

        #region inject

        private   IPrefabSetManager PrefabSetManager { get; }
        protected IViewGameTicker   ViewGameTicker   { get; }

        protected CameraProviderRmazor(
            IPrefabSetManager _PrefabSetManager,
            IViewGameTicker   _ViewGameTicker)
        {
            PrefabSetManager = _PrefabSetManager;
            ViewGameTicker   = _ViewGameTicker;
        }

        #endregion

        #region api

        public Func<Bounds> GetMazeBounds     { protected get; set; }
        public Func<float>  GetConverterScale { protected get; set; }

        public Transform Follow
        {
            get => m_FollowTr;
            set
            {
                m_FollowTr = value;
                FollowTransformIsNotNull = true;
            }
        }
        public Camera    Camera => LevelCameraInitialized ? LevelCamera : Camera.main;

        public void SetEffectProps<T>(ECameraEffect _Effect, T _Args) where T : ICameraEffectProps
        {
            void ShowTypeError<T1>() where T1 : ICameraEffectProps
            {
                Dbg.LogError($"_Args must have type {typeof(T1)}," +
                             $" but has type {_Args.GetType()}");
            }
            switch (_Effect)
            {
                case ECameraEffect.DepthOfField:
                {
                    if (!(_Args is FastDofProps props))
                    {
                        ShowTypeError<FastDofProps>();
                        break;
                    }
                    if (props!.BlurAmount.HasValue)
                        m_DepthOfField.BlurAmount = props.BlurAmount.Value;
                }
                    break;
                case ECameraEffect.Glitch:
                {
                    if (!(_Args is FastGlitchProps props))
                    {
                        ShowTypeError<FastGlitchProps>();
                        break;
                    }
                    if (props!.ChromaticGlitch.HasValue) m_FastGlitch.ChromaticGlitch = props.ChromaticGlitch.Value;
                    if (props.FrameGlitch.HasValue)      m_FastGlitch.FrameGlitch     = props.FrameGlitch.Value;
                    if (props.PixelGlitch.HasValue)      m_FastGlitch.PixelGlitch     = props.PixelGlitch.Value;
                }
                    break;
                case ECameraEffect.ColorGrading:
                {
                    if (!(_Args is ColorGradingProps props))
                    {
                        ShowTypeError<ColorGradingProps>();
                        break;
                    }
                    if (props!.Color.HasValue)            m_ColorGrading.Color            = props.Color.Value;
                    if (props.Hue.HasValue)               m_ColorGrading.Hue              = props.Hue.Value;
                    if (props.Contrast.HasValue)          m_ColorGrading.Contrast         = props.Contrast.Value;
                    if (props.Brightness.HasValue)        m_ColorGrading.Brightness       = props.Brightness.Value;
                    if (props.Saturation.HasValue)        m_ColorGrading.Saturation       = props.Saturation.Value;
                    if (props.Exposure.HasValue)          m_ColorGrading.Exposure         = props.Exposure.Value;
                    if (props.Gamma.HasValue)             m_ColorGrading.Gamma            = props.Gamma.Value;
                    if (props.Sharpness.HasValue)         m_ColorGrading.Sharpness        = props.Sharpness.Value;
                    if (props.Blur.HasValue)              m_ColorGrading.Blur             = props.Blur.Value;
                    if (props.VignetteColor.HasValue)     m_ColorGrading.VignetteColor    = props.VignetteColor.Value;
                    if (props.VignetteSoftness.HasValue)  m_ColorGrading.VignetteSoftness = props.VignetteSoftness.Value;
                    if (props.VignetteAmount.HasValue)    m_ColorGrading.VignetteAmount   = props.VignetteAmount.Value;
                }
                    break;
                case ECameraEffect.ChromaticAberration:
                {
                    if (!(_Args is ChromaticAberrationProps props))
                    {
                        ShowTypeError<ChromaticAberrationProps>();
                        break;
                    }
                    if (props!.RedX.HasValue) m_ChromaticAberration.RedX  = props.RedX.Value;
                    if (props.RedY.HasValue)  m_ChromaticAberration.RedY  = props.RedY.Value;
                    if (props.BlueX.HasValue) m_ChromaticAberration.BlueX = props.BlueX.Value;
                    if (props.BlueY.HasValue) m_ChromaticAberration.BlueY = props.BlueY.Value;
                    if (props.FishEyeDistortion.HasValue)
                        m_ChromaticAberration.FishEyeDistortion = props.FishEyeDistortion.Value;
                }
                    break;
                case ECameraEffect.AntiAliasing:
                {
                    if (!(_Args is FxaaProps props))
                    {
                        ShowTypeError<FxaaProps>();
                        break;
                    }
                    if (props!.Sharpness.HasValue) m_Fxaa.Sharpness = props.Sharpness.Value;
                    if (props.Threshold.HasValue)  m_Fxaa.Sharpness = props.Threshold.Value;
                }
                    break;
                case ECameraEffect.Pixelate:
                {
                    if (!(_Args is PixelateProps props))
                    {
                        ShowTypeError<PixelateProps>();
                        break;
                    }
                    if (props!.Density.HasValue) m_Pixelate.Dencity = props.Density.Value;
                }
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Effect);
            }
        }

        public void AnimateEffectProps<T>(ECameraEffect _Effect, T _From, T _To, float _Duration) where T : ICameraEffectProps
        {
            void ShowTypeError<T1>() where T1 : ICameraEffectProps
            {
                Dbg.LogError($"_From and _To must have type {typeof(T1)}," +
                             $" but have types {_From.GetType()} and {_To.GetType()}");
            }
            switch (_Effect)
            {
                case ECameraEffect.DepthOfField:
                {
                    if (!(_From is FastDofProps from) || !(_To is FastDofProps to))
                    {
                        ShowTypeError<FastDofProps>();
                        break;
                    }
                    if (from.BlurAmount.HasValue && to.BlurAmount.HasValue)
                        AnimateEffectParameter(_V => m_DepthOfField.BlurAmount = _V, from.BlurAmount.Value, to.BlurAmount.Value, _Duration);
                }
                    break;
                case ECameraEffect.Glitch:
                {
                    if (!(_From is FastGlitchProps from) || !(_To is FastGlitchProps to))
                    {
                        ShowTypeError<FastGlitchProps>();
                        break;
                    }
                    if (from.ChromaticGlitch.HasValue && to.ChromaticGlitch.HasValue)
                        AnimateEffectParameter(_V => m_FastGlitch.ChromaticGlitch = _V, from.ChromaticGlitch.Value, to.ChromaticGlitch.Value, _Duration);
                    if (from.FrameGlitch.HasValue && to.FrameGlitch.HasValue)
                        AnimateEffectParameter(_V => m_FastGlitch.FrameGlitch = _V, from.FrameGlitch.Value, to.FrameGlitch.Value, _Duration);
                    if (from.PixelGlitch.HasValue && to.PixelGlitch.HasValue)
                        AnimateEffectParameter(_V => m_FastGlitch.PixelGlitch = _V, from.PixelGlitch.Value, to.PixelGlitch.Value, _Duration);
                }
                    break;
                case ECameraEffect.ColorGrading:
                {
                    if (!(_From is ColorGradingProps from) || !(_To is ColorGradingProps to))
                    {
                        ShowTypeError<ColorGradingProps>();
                        break;
                    }
                    if (from.Color.HasValue && to.Color.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Color = _V, from.Color.Value, to.Color.Value, _Duration);
                    if (from.Hue.HasValue && to.Hue.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Hue = _V, from.Hue.Value, to.Hue.Value, _Duration);
                    if (from.Contrast.HasValue && to.Contrast.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Contrast = _V, from.Contrast.Value, to.Contrast.Value, _Duration);
                    if (from.Brightness.HasValue && to.Brightness.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Brightness = _V, from.Brightness.Value, to.Brightness.Value, _Duration);
                    if (from.Saturation.HasValue && to.Saturation.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Saturation = _V, from.Saturation.Value, to.Saturation.Value, _Duration);
                    if (from.Exposure.HasValue && to.Exposure.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Exposure = _V, from.Exposure.Value, to.Exposure.Value, _Duration);
                    if (from.Gamma.HasValue && to.Gamma.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Gamma = _V, from.Gamma.Value, to.Gamma.Value, _Duration);
                    if (from.Sharpness.HasValue && to.Sharpness.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Sharpness = _V, from.Sharpness.Value, to.Sharpness.Value, _Duration);
                    if (from.Blur.HasValue && to.Blur.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.Blur = _V, from.Blur.Value, to.Blur.Value, _Duration);
                    if (from.VignetteColor.HasValue && to.VignetteColor.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.VignetteColor = _V, from.VignetteColor.Value, to.VignetteColor.Value, _Duration);
                    if (from.VignetteSoftness.HasValue && to.VignetteSoftness.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.VignetteSoftness = _V, from.VignetteSoftness.Value, to.VignetteSoftness.Value, _Duration);
                    if (from.VignetteAmount.HasValue && to.VignetteAmount.HasValue)
                        AnimateEffectParameter(_V => m_ColorGrading.VignetteAmount = _V, from.VignetteAmount.Value, to.VignetteAmount.Value, _Duration);
                }
                    break;
                case ECameraEffect.ChromaticAberration:
                {
                    if (!(_From is ChromaticAberrationProps from) || !(_To is ChromaticAberrationProps to))
                    {
                        ShowTypeError<ChromaticAberrationProps>();
                        break;
                    }
                    if (from.RedX.HasValue && to.RedX.HasValue)
                        AnimateEffectParameter(_V => m_ChromaticAberration.RedX = _V, from.RedX.Value, to.RedX.Value, _Duration);
                    if (from.RedY.HasValue && to.RedY.HasValue)
                        AnimateEffectParameter(_V => m_ChromaticAberration.RedY = _V, from.RedY.Value, to.RedY.Value, _Duration);
                    if (from.BlueX.HasValue && to.BlueX.HasValue)
                        AnimateEffectParameter(_V => m_ChromaticAberration.BlueX = _V, from.BlueX.Value, to.BlueX.Value, _Duration);
                    if (from.BlueY.HasValue && to.BlueY.HasValue)
                        AnimateEffectParameter(_V => m_ChromaticAberration.BlueY = _V, from.BlueY.Value, to.BlueY.Value, _Duration);
                    if (from.FishEyeDistortion.HasValue && to.FishEyeDistortion.HasValue)
                        AnimateEffectParameter(_V => m_ChromaticAberration.FishEyeDistortion = _V, from.FishEyeDistortion.Value, to.FishEyeDistortion.Value, _Duration);
                }
                    break;
                case ECameraEffect.AntiAliasing:
                {
                    if (!(_From is FxaaProps from) || !(_To is FxaaProps to))
                    {
                        ShowTypeError<FxaaProps>();
                        break;
                    }
                    if (from.Sharpness.HasValue && to.Sharpness.HasValue)
                        AnimateEffectParameter(_V => m_Fxaa.Sharpness = _V, from.Sharpness.Value, to.Sharpness.Value, _Duration);
                    if (from.Threshold.HasValue && to.Threshold.HasValue)
                        AnimateEffectParameter(_V => m_Fxaa.Threshold = _V, from.Threshold.Value, to.Threshold.Value, _Duration);
                }
                    break;
                case ECameraEffect.Pixelate:
                {
                    if (!(_From is PixelateProps from) || !(_To is PixelateProps to))
                    {
                        ShowTypeError<PixelateProps>();
                        break;
                    }
                    if (from.Density.HasValue && to.Density.HasValue)
                        AnimateEffectParameter(_V => m_Pixelate.Dencity = _V, from.Density.Value, to.Density.Value, _Duration);
                }
                    break;
                default:
                    throw new SwitchCaseNotImplementedException(_Effect);
            }
        }

        public bool IsEffectEnabled(ECameraEffect _Effect)
        {
            return _Effect switch
            {
                ECameraEffect.DepthOfField        => m_DepthOfField.enabled,
                ECameraEffect.Glitch              => m_FastGlitch.enabled,
                ECameraEffect.ColorGrading        => m_ColorGrading.enabled,
                ECameraEffect.ChromaticAberration => m_ChromaticAberration.enabled,
                ECameraEffect.AntiAliasing        => m_Fxaa.enabled,
                ECameraEffect.Pixelate            => m_Pixelate.enabled,
                _                                 => throw new SwitchCaseNotImplementedException(_Effect)
            };
        }

        public void EnableEffect(ECameraEffect _Effect, bool _Enabled)
        {
            switch (_Effect)
            {
                case ECameraEffect.DepthOfField:        m_DepthOfField.enabled        = _Enabled; break;
                case ECameraEffect.Glitch:              m_FastGlitch.enabled          = _Enabled; break;
                case ECameraEffect.ColorGrading:        m_ColorGrading.enabled        = _Enabled; break;
                case ECameraEffect.ChromaticAberration: m_ChromaticAberration.enabled = _Enabled; break;
                case ECameraEffect.AntiAliasing:        m_Fxaa.enabled                = _Enabled; break;
                case ECameraEffect.Pixelate:            m_Pixelate.enabled            = _Enabled; break;
                default:
                    throw new SwitchCaseNotImplementedException(_Effect);
            }
        }

        public void UpdateState()
        {
            if (SceneManager.GetActiveScene().name != SceneNames.Level)
            {
                LevelCameraInitialized = false;
                FollowTransformIsNotNull = false;
                return;
            }
            InitLevelCamera();
            InitLevelCameraEffectComponents();
        }

        #endregion

        #region nonpublic methods

        private void InitLevelCamera()
        {
            if (LevelCamera.IsNotNull())
                return;
            var obj = GameObject.Find("Level Camera");
            LevelCameraTr = obj.transform;
            LevelCamera = obj.GetComponent<Camera>();
            LevelCameraInitialized = true;
        }

        private void InitLevelCameraEffectComponents()
        {
            InitDepthOfField();
            InitGlitch();
            InitChromaticAberration();
            InitColorGrading();
            InitFxaa();
            InitPixelate();
        }

        private void InitDepthOfField()
        {
            m_DepthOfField = LevelCamera.GetCompItem<FastDOF>("fast_dof");
            m_DepthOfField.enabled = false;
            m_DepthOfField.BlurAmount = 0;
        }

        private void InitGlitch()
        {
            m_FastGlitch = LevelCamera.GetCompItem<FastGlitch>("fast_glitch");
            m_FastGlitch.enabled = false;
            m_FastGlitch.material = PrefabSetManager.InitObject<Material>(
                PrefabSetName, "fast_glitch_material");
        }

        private void InitChromaticAberration()
        {
            m_ChromaticAberration = LevelCamera.GetCompItem<ChromaticAberration>("chromatic_aberration");
            m_ChromaticAberration.enabled = false;
        }

        private void InitColorGrading()
        {
            m_ColorGrading = LevelCamera.GetCompItem<ColorGrading>("color_grading");
            m_ColorGrading.enabled = false;
        }

        private void InitFxaa()
        {
            m_Fxaa = LevelCamera.GetCompItem<FXAA>("fxaa");
            m_Fxaa.enabled = false;
        }

        private void InitPixelate()
        {
            m_Pixelate = LevelCamera.GetCompItem<Pixellate>("pixelate");
            m_Pixelate.enabled = false;
        }

        private void AnimateEffectParameter(
            UnityAction<float> _UpdateValue,
            float              _From,
            float              _To,
            float              _Duration)
        {
            Cor.Run(Cor.Lerp(
                ViewGameTicker,
                _Duration,
                _From,
                _To,
                _UpdateValue));
        }
        
        private void AnimateEffectParameter(
            UnityAction<int> _UpdateValue,
            int              _From,
            int              _To,
            float            _Duration)
        {
            Cor.Run(Cor.Lerp(
                ViewGameTicker,
                _Duration,
                _From,
                _To,
                _P =>
                {
                    _UpdateValue((int)_P);
                }));
        }
        
        private void AnimateEffectParameter(
            UnityAction<Color> _UpdateValue,
            Color              _From,
            Color              _To,
            float              _Duration)
        {
            Cor.Run(Cor.Lerp(
                ViewGameTicker,
                _Duration,
                _OnProgress: _P =>
                {
                    var val = Color.Lerp(_From, _To, _P);
                    _UpdateValue(val);
                }));
        }
        
        #endregion
    }
}