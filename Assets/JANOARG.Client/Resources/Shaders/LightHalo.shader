Shader "JANOARG/Light Halo"
{
    Properties
    {
        [PerRendererData] _MainTex ("Halo Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Size ("Size", Float) = 1
        _NoCore ("No Core", Range(0,1)) = 0
        _CoreSize ("Core Size", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha One
        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            float _Size;
            float _NoCore;
            float _CoreSize;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // Billboard in view space so the halo always faces the camera,
                // regardless of the light's parent rotation/scale.
                float4 viewCenter = mul(UNITY_MATRIX_MV, float4(0, 0, 0, 1));
                viewCenter.xy += v.vertex.xy * _Size;
                o.vertex = mul(UNITY_MATRIX_P, viewCenter);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 tex = tex2D(_MainTex, i.uv);

                // Soft glow is always present. CoreSize adds a bright center
                // only when Bright Core is on; with it off this is pure glow.
                float2 p = i.uv * 2.0 - 1.0;
                float d = saturate(length(p));
                float soft = tex.a * 0.35;
                float coreMask = step(0.0001, _CoreSize) * (1.0 - smoothstep(_CoreSize, _CoreSize + 0.1, d));
                float coreAlpha = lerp(soft, tex.a, coreMask);

                // _NoCore = 1 when Bright Core is off.
                float alpha = lerp(coreAlpha, soft, saturate(_NoCore));

                return fixed4(tex.rgb * _Color.rgb, alpha * _Color.a);
            }
            ENDCG
        }
    }
}
