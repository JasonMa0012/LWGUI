Shader "Hidden"
{
    Properties
    {
        [KWEnum(_, Name 1, _KWENUM_KEY1, Name 2, _KWENUM_KEY2)]
        _kwenum ("KWEnum", float) = 0
        
        [KeywordEnum(key1, key2)]
        _enum ("KeywordEnum", float) = 0

        [SubToggle(_, _SUBTOGGLE_KEYWORD)] _toggle ("Sub Toggle", float) = 0
        [SubToggle(_, _TOGGLE_KEYWORD)] _toggle1 ("Toggle", float) = 0

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog
            #pragma multi_compile _KWENUM_KEY1 _KWENUM_KEY2
            #pragma multi_compile _ENUM_KEY1 _ENUM_KEY2
            #pragma multi_compile _ _SUBTOGGLE_KEYWORD
            #pragma multi_compile _ _TOGGLE_KEYWORD

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

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = 0;
                
                #if _KWENUM_KEY1
                    col.x = 0;
                #elif _KWENUM_KEY2
                    col.x = 1;
                #endif
                
                #if _ENUM_KEY1
                    col.y = 0;
                #elif _ENUM_KEY2
                    col.y = 1;
                #endif
                
                #if _SUBTOGGLE_KEYWORD
                    col.z = 0.5;
                #endif
                
                #if _TOGGLE_KEYWORD
                    col.z = 1;
                #endif
                
                return col;
            }
            ENDCG
        }
    }
    CustomEditor "LWGUI.LWGUI"
}
