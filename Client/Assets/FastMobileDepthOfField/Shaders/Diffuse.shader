﻿Shader "RufatShaderlab/DOF/Diffuse" 
{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
	}
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 100

		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma vertex vert  
			#pragma fragment frag 
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_fwdbase
			#pragma multi_compile_instancing
			#pragma multi_compile_fog

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"

			UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
			half4 _MainTex_ST;
			half _Focus;
			half _Aperture;
			half4 _Color;
			half4 _LightColor0;
			half4 _SpecColor;

			struct appdata
			{
				half4 vertex : POSITION;
				half3 normal : NORMAL;
				half2 uv : TEXCOORD0;
#ifdef LIGHTMAP_ON
				half2 luv : TEXCOORD1;
#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f {
				half4 pos : SV_POSITION;
				half3 uv : TEXTCOORD0;
#ifdef LIGHTMAP_ON
				half3 fogCoord : TEXCOORD1;
#else
				half3 ref : TEXTCOORD1;
				half4 fogCoord : TEXTCOORD2;
				SHADOW_COORDS(3)
#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_OUTPUT(v2f, o);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				o.uv.xy = TRANSFORM_TEX(v.uv,_MainTex);
				o.pos = UnityObjectToClipPos(v.vertex);
				half a = -UnityObjectToViewPos(v.vertex).z;
				o.uv.z = saturate(abs((1.0h - clamp(max(a / _Focus, _Focus / a), 0.0h, 20.0h)) * _Aperture));
#ifdef LIGHTMAP_ON
				o.fogCoord.yz = v.luv * unity_LightmapST.xy + unity_LightmapST.zw;
#else
				half3 normal = UnityObjectToWorldNormal(v.normal);
				o.fogCoord.yzw = ShadeSH9(half4(normal, 1.0h));
				o.ref = _LightColor0.rgb * max(0.0h, dot(normal, _WorldSpaceLightPos0.xyz));
				TRANSFER_SHADOW(o);
#endif
				UNITY_TRANSFER_FOG(o, o.pos);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 color = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_MainTex, i.uv.xy);
#ifdef LIGHTMAP_ON
				color.rgb *= DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, i.fogCoord.yz)) * _Color.rgb;
#else
				color.rgb *= (i.ref.xyz * SHADOW_ATTENUATION(i) + i.fogCoord.yzw) * _Color.rgb;
#endif
				UNITY_APPLY_FOG(i.fogCoord.x, color);
				return fixed4(color.rgb, i.uv.z);
			}
			ENDCG
		}

		Pass
		{
			Tags { "LightMode" = "ShadowCaster" }

			ZWrite On ZTest LEqual Cull Off

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_shadowcaster
			#include "UnityCG.cginc"

			struct v2f {
				V2F_SHADOW_CASTER;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			v2f vert(appdata_base v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				SHADOW_CASTER_FRAGMENT(i)
			}
			ENDCG
		}
	}
}