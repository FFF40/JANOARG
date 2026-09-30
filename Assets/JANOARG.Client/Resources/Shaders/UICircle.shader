Shader "UI/Circle"
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
                // Per-instance circle params, packed by GraphicCircleGPU:
                // x = FillAmount, y = InsideRadius, z = Sides, w = Rotation (deg)
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
                // Aspect ratio (width / height) — used in fragment
                float  aspect        : TEXCOORD3;
                // Per-instance circle params, see appdata_t.texcoord1
                float4 circleParams  : TEXCOORD4;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // ---------------------------------------------------------------
            // Uniforms
            // ---------------------------------------------------------------
            sampler2D _MainTex;
            fixed4    _Color;
            fixed4    _TextureSampleAdd;
            float4    _ClipRect;
            float4    _MainTex_ST;

            // ---------------------------------------------------------------
            // Helpers
            // ---------------------------------------------------------------

            // Signed distance to a regular N-gon centred at origin, radius r.
            // p must already be in isotropic (aspect-corrected) space.
            float sdPolygon(float2 p, int n, float r)
            {
                float angle  = UNITY_TWO_PI / (float)n;
                float halfA  = angle * 0.5;
                // Snap to nearest face and measure distance to that edge
                float a      = atan2(p.x, p.y);           // angle from top
                float sector = floor((a + halfA) / angle) * angle;
                float2 dir   = float2(sin(sector), cos(sector));
                return dot(p, dir) - r * cos(halfA);
            }

            // ---------------------------------------------------------------
            // Vertex shader
            // ---------------------------------------------------------------
            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

                OUT.worldPosition = v.vertex;
                OUT.vertex        = UnityObjectToClipPos(v.vertex);
                OUT.texcoord      = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color         = v.color * _Color;

                // texcoord is [0,1]; remap to [-1,1]
                float2 uv = v.texcoord * 2.0 - 1.0;

                // We need aspect ratio to keep the circle round.
                // _ScreenParams gives pixel size but for UI we encode it via
                // the world position difference — Unity passes rect verts so
                // we derive it per-instance in the fragment instead.
                // Store the raw [-1,1] UV and handle aspect in fragment.
                OUT.localUV = uv;
                OUT.aspect  = 1.0; // placeholder; corrected in fragment via ddx/ddy
                OUT.circleParams = v.texcoord1;
                return OUT;
            }

            // ---------------------------------------------------------------
            // Fragment shader
            // ---------------------------------------------------------------
            fixed4 frag(v2f IN) : SV_Target
            {
                // --- Aspect correction using screen-space derivatives -------
                // ddx/ddy give us the pixel footprint in UV space, which lets
                // us recover the true aspect ratio of the rect at runtime,
                // regardless of canvas scale or resolution.
                float2 dUVdx = ddx(IN.localUV);
                float2 dUVdy = ddy(IN.localUV);
                // Width  in UV units per pixel in the x direction → actual px width
                float pxW = length(dUVdx);
                float pxH = length(dUVdy);
                // aspect = UV-width per pixel / UV-height per pixel
                // If aspect > 1 the rect is wider than tall; we squish x to make
                // a circle, matching GraphicCircle's ellipse behaviour.
                float aspect = (pxH > 0.0001) ? (pxW / pxH) : 1.0;

                // Corrected local coord: p.x is in "height units" so that
                // radius 1 means "touch the shorter edge" on both axes.
                // This matches GraphicCircle which uses separate radius.x/y.
                float2 p = float2(IN.localUV.x * aspect, IN.localUV.y);

                // Per-instance params, packed into the vertex stream so all
                // GraphicCircleGPU instances can share one Material and batch.
                float fillAmount   = IN.circleParams.x;
                float insideRadius = IN.circleParams.y;
                float sidesParam   = IN.circleParams.z;
                float rotation     = IN.circleParams.w;

                // --- Rotation ----------------------------------------------
                int   sides = max((int)round(sidesParam), 1);

                float rotRad = rotation * UNITY_PI / 180.0;
                // Add half of side angle to mimic pointy top angle of CPU GraphicCircle
                rotRad += UNITY_TWO_PI * 0.5 / sides;
                float sinR = sin(rotRad), cosR = cos(rotRad);
                p = float2(p.x * cosR - p.y * sinR,
                           p.x * sinR + p.y * cosR);

                // --- Radial distance / polygon distance --------------------
                float dist; // signed distance to outer shape edge (negative = inside)

                if (sides >= 3)
                    dist = sdPolygon(p, sides, 1.0);
                else
                    dist = length(p) - 1.0; // circle SDF

                // --- Anti-aliasing width (in the same UV space) ------------
                float aa = length(float2(dUVdx.x * aspect, dUVdy.y)); // ~1 pixel

                // --- Outer mask -------------------------------------------
                float outerAlpha = 1.0 - smoothstep(-aa, aa, dist);

                // --- Inner (hole) mask ------------------------------------
                float innerAlpha = 1.0;
                if (insideRadius > 0.001)
                {
                    float innerDist;
                    if (sides >= 3)
                        innerDist = sdPolygon(p, sides, insideRadius);
                    else
                        innerDist = length(p) - insideRadius;

                    innerAlpha = smoothstep(-aa, aa, innerDist);
                }

                // --- Fill amount (arc / partial fill) ---------------------
                // FillAmount = 1 → full shape. < 1 → cut clockwise from top.
                float fillAlpha = 1.0;
                if (fillAmount < 0.9999)
                {
                    // Angle from top, [0, 2π)
                    // Note: atan2(x,y) = angle from +Y axis
                    float angle = atan2(IN.localUV.x, IN.localUV.y); // [-π, π]
                    if (angle < 0.0) angle += UNITY_TWO_PI;           // [0, 2π)

                    float cutAngle = fillAmount * UNITY_TWO_PI;
                    // Soft edge at the fill boundary (half a pixel wide)
                    fillAlpha = 1.0 - smoothstep(cutAngle - aa * 0.5, cutAngle + aa * 0.5, angle);
                }

                // --- Sample tint / texture --------------------------------
                half4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                color.a *= outerAlpha * innerAlpha * fillAlpha;

                // --- Unity UI clipping ------------------------------------
                #ifdef UNITY_UI_CLIP_RECT
                color.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(color.a - 0.001);
                #endif

                return color;
            }
            ENDCG
        }
    }
}
