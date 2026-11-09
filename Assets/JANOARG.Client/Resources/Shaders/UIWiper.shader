Shader "UI/Wiper"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // --- Standard Unity UI boilerplate ---
        _StencilComp      ("Stencil Comparison", Float) = 8
        _Stencil          ("Stencil ID",          Float) = 0
        _StencilOp        ("Stencil Operation",   Float) = 0
        _StencilWriteMask ("Stencil Write Mask",  Float) = 255
        _StencilReadMask  ("Stencil Read Mask",   Float) = 255
        _ColorMask        ("Color Mask",          Float) = 15
        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Stencil
        {
            Ref       [_Stencil]
            Comp      [_StencilComp]
            Pass      [_StencilOp]
            ReadMask  [_StencilReadMask]
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
            #pragma vertex   vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            // ---------------------------------------------------------------
            // Structs
            // ---------------------------------------------------------------
            struct appdata_t
            {
                float4 vertex    : POSITION;
                float4 color     : COLOR;
                float2 texcoord  : TEXCOORD0;
                // x = angle, y = fill threshold
                float4 texcoord1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex        : SV_POSITION;
                fixed4 color         : COLOR;
                float2 texcoord      : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
                // Local-space UV in [-1,1] range, aspect-corrected
                float2 localUV       : TEXCOORD2;
                // Params passed from appdata_t.textcoord1
                float4 params        : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4    _Color;
            fixed4    _TextureSampleAdd;
            float4    _ClipRect;
            float4    _MainTex_ST;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = v.vertex;
                OUT.vertex        = UnityObjectToClipPos(v.vertex);
                OUT.texcoord      = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color         = v.color * _Color;
                OUT.localUV       = v.texcoord;
                OUT.params        = v.texcoord1;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float angle = IN.params.x;
                float threshold = IN.params.y;

                clip(
                    IN.localUV.y * sin(angle) - IN.localUV.x * cos(angle)
                    + threshold
                );
                
                return IN.color;
            }
            ENDCG
        }
    }
}
