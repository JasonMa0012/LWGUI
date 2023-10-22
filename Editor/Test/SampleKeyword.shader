Shader "Hidden"
{
	Properties
	{
		[Preset(_, LWGUI_ShaderPropertyPreset2)] _preset2 ("Preset", float) = 0

		[KeywordEnum(key1, key2)]
		_keywordEnum ("KeywordEnum", float) = 0
		
		[KWEnum(_, Name 1, _KWENUM_KEY1, Name 2, _KWENUM_KEY2)]
		_kwenum ("KWEnum", float) = 0
		
		[Toggle(_TOGGLE_KEYWORD)] _toggle1 ("Toggle", float) = 0

		[Main(g0, _GROUP_TOGGLE_KEYWORD)] _group_toggle1 ("Group Toggle", float) = 0

		[Main(g1, _, on, on)]
		[PassSwitch(Always)]
		_group ("Pass Switch Group", float) = 1
		[SubEnum(g1, Off, 0, On, 1)] _ZWrite ("ZWrite Mode", Float) = 1
		[SubToggle(g1, _SUBTOGGLE_KEYWORD)] _toggle ("Sub Toggle", float) = 0
		[SubKeywordEnum(g1, key1, key2)]
		_subKeywordEnum ("SubKeywordEnum", float) = 0
	}
	SubShader
	{

		Pass
		{
			Tags { "RenderType" = "Opaque" "LightMode" = "Always" }
			LOD 100
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			#pragma multi_compile _KWENUM_KEY1 _KWENUM_KEY2
			#pragma multi_compile _KEYWORDENUM_KEY1 _KEYWORDENUM_KEY2
			#pragma multi_compile _SUBKEYWORDENUM_KEY1 _SUBKEYWORDENUM_KEY2
			#pragma multi_compile _ _SUBTOGGLE_KEYWORD
			#pragma multi_compile _ _TOGGLE_KEYWORD
			#pragma multi_compile _ _GROUP_TOGGLE_KEYWORD

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


			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed4 col = 0;
				
				#if _KWENUM_KEY1
					col.x += 0;
				#elif _KWENUM_KEY2
					col.x += 0.25;
				#endif
				
				#if _KEYWORDENUM_KEY1
					col.y += 0;
				#elif _KEYWORDENUM_KEY2
					col.y += 0.25;
				#endif
				
				#if _SUBKEYWORDENUM_KEY1
					col.y += 0.0;
				#elif _SUBKEYWORDENUM_KEY2
					col.y += 0.25;
				#endif
				
				#if _SUBTOGGLE_KEYWORD
					col.z += 0.25;
				#endif
				
				#if _TOGGLE_KEYWORD
					col.z += 0.25;
				#endif

				#if _GROUP_TOGGLE_KEYWORD
					col.z += 0.25;
				#endif


				return frac(col);
			}
			ENDCG
		}
	}
	CustomEditor "LWGUI.LWGUI"
}
