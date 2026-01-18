Shader "Unlit/Path"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _RoadColor ("Road Color", Color) = (0,0,0,1)
        _SeparatorColor ("Separator Color", Color) = (1,1,1,1)

        [HideInInspector] _Alpha ("Alpha", Range(0, 1)) = 1
        _RoadThickness ("Road Thickness", Range(0, 1)) = 0.75
        _SeparatorThickness("Separator Thickness", Range(0, 1)) = 0.25
        _SeparatorDashLength("Seperator Dash Length", float) = 0.75
        _SeparatorDashCycle("Seperator Dash Cycle", Range(0, 1)) = 0.75
    }
    SubShader
    { 
        Tags { "RenderType"="Transparent" }
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };
        
            UNITY_INSTANCING_BUFFER_START(Props);
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _OutlineColor);
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _RoadColor);
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _SeparatorColor);
            UNITY_DEFINE_INSTANCED_PROP(fixed, _Alpha);
            UNITY_DEFINE_INSTANCED_PROP(fixed, _RoadThickness);
            UNITY_DEFINE_INSTANCED_PROP(fixed, _SeparatorThickness);
            UNITY_DEFINE_INSTANCED_PROP(fixed, _SeparatorDashLength);
            UNITY_DEFINE_INSTANCED_PROP(fixed, _SeparatorDashCycle);
            UNITY_INSTANCING_BUFFER_END(Props);

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed laneX = abs(i.uv.x - 0.5) * 2;
                fixed laneY = (1 + i.uv.y * _SeparatorDashLength - (_Time.x * 3 % 1)) % 1;

                fixed prog = laneY * 2 - 1;
                prog = prog > 0 ? prog : 0;
                fixed4 col = laneX < _RoadThickness ? _RoadColor + (_SeparatorColor - _RoadColor) * prog * prog :
                    _OutlineColor;

                col.a *= _Alpha;

                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
