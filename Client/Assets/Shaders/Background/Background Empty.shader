﻿Shader "RMAZOR/Background/Empty"
{
    Properties
    {
        _Color1 ("Color 1", Color) = (0,0,0,1)
        _Color2 ("Color 2", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }
        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma target 2.0

            #include "../Common.cginc"

            v2f vert(appdata v)
            {
                return vert_default(v);
            }

           
            fixed4 frag(v2f f_i) : SV_Target
            {
                return fixed4(0,0,0,0);
            }
            
            ENDCG
        }
    }
}