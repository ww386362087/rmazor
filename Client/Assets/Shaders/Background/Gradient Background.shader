﻿Shader "RMAZOR/Background/Solid" {
    Properties {
        _Color1 ("Tint", Color) = (1,1,1,1)
        _Color2 ("Tint", Color) = (1,1,1,1)
        [MaterialToggle] _Gradient("Gradient", Float) = 0
        _Gc1("Gradient Coefficient 1", Range(0, 1)) = 0.5
        _Gc2("Gradient Coefficient 2", Range(0, 1)) = 0.5
    }
    SubShader {
        Tags {
            "Queue"       = "Transparent"
            "RenderType"  = "Transparent"
            "PreviewType" = "Plane"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #include "../Common.cginc"
            #include "Parallax.cginc"

            fixed4 _Color1, _Color2;
            fixed _Gc1, _Gc2;

			v2f vert(appdata v) {
                return vert_default(v);
            }
            
            fixed3 linear_to_srgb(fixed3 color) {
			    float v = 1.0/2.2;
			    return pow(color, float3(v,v,v));
			}

            float gradient_noise(in float2 uv) {
                const float3 magic = float3(0.06711056, 0.00583715, 52.9829189);
                return frac(magic.z * frac(dot(uv, magic.xy)));
            }

            float dist(float2 p0, float2 pf) {
			    return sqrt((pf.x-p0.x)*(pf.x-p0.x)+(pf.y-p0.y)*(pf.y-p0.y));
			}

            fixed4 frag(v2f i) : SV_Target
            {
                float d = dist(0.5,i.uv * _Gc2)*_Gc1*1.5;
                fixed4 main_col = lerp(_Color1, _Color2, d);
                main_col.rgb *= main_col.a;
                fixed4 parallax_col = parallax_color(i, 1.0, _Color1, _Color2);
                return main_col + parallax_col;
            }
            ENDCG
        }
    }
}