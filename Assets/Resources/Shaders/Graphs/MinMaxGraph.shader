Shader "UI/Min Max Graph"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            float _Values[64];
            float _ValuesMin[64];
            float _ValuesMax[64];
            float2 _Resolution;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPos = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPos);

                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

                OUT.color = v.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;

                float colPos = IN.texcoord.x * 63;
                float yValSlope = _Values[ceil(colPos)] - _Values[floor(colPos)];
                float yVal = _Values[floor(colPos)] + (yValSlope) * (colPos % 1);
                float yMin = _ValuesMin[floor(colPos)] + (_ValuesMin[ceil(colPos)] - _ValuesMin[floor(colPos)]) * (colPos % 1);
                float yMax = _ValuesMax[floor(colPos)] + (_ValuesMax[ceil(colPos)] - _ValuesMax[floor(colPos)]) * (colPos % 1);

                float colPrevPos = (IN.texcoord.x - 1 / + _Resolution.x) * 63;
                float yPrevValSlope = _Values[ceil(colPrevPos)] - _Values[floor(colPrevPos)];
                float yPrevVal = _Values[floor(colPrevPos)] + (yPrevValSlope) * (colPrevPos % 1);
                
                if (IN.texcoord.y < yMax) color.a = IN.texcoord.y;
                else if (IN.texcoord.y < yMin) color.a = IN.texcoord.y * 0.5;
                else color.a = 0;

                float yLineCenter = (yVal + yPrevVal) / 2;
                float yLineHeight = max(abs(yVal - yPrevVal), 1.0 / _Resolution.y) / 2;
                
                if (abs(IN.texcoord.y - yLineCenter) < yLineHeight) color.a = 1;

                color.a *= IN.texcoord.x * IN.texcoord.x;
                if (IN.worldPos.y > _ScreenParams.y * 0.5 - 140) color.a *= _ScreenParams.y * 0.01 - 1.8 - IN.worldPos.y * 0.02;

                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip (color.a - 0.001);
                #endif

                return color;
            }
        ENDCG
        }
    }
}