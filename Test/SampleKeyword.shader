// Copyright (c) Jason Ma
Shader "Hidden"
{
	Properties
	{
		[Preset(_, LWGUI_ShaderPropertyPreset2)] _preset2 ("Preset", float) = 0

		[KeywordEnum(key1, key2)] _keywordEnum ("KeywordEnum", float) = 0
		
		[KWEnum(_, Name 1, _KWENUM_KEY1, Name 2, _KWENUM_KEY2)] _kwenum ("KWEnum", float) = 0
		[ShowIf(_KWENUM_KEY1, E, 1)] _float1 ("Show If - Name 1", float) = 0
		[ActiveIf(_KWENUM_KEY2, E, 1)] _float2 ("Active If - Name 2", float) = 0
		
		[Toggle(_TOGGLE_KEYWORD)] _toggle1 ("Toggle", float) = 0
		
		[SubPowerSlider(_, 1, LWGUI_ShaderPropertyPreset_Keywords)] _subPowerSlider ("SubPowerSlider with Preset", Range(0, 1)) = 0

		[Main(g0, _GROUP_TOGGLE_KEYWORD)] _group_toggle1 ("Group Toggle", float) = 0

		[SubEnum(g0, Off, 0, On, 1)] _ZWrite ("ZWrite Mode", Float) = 1
		[SubToggle(g0, _SUBTOGGLE_KEYWORD)] _toggle ("Sub Toggle", float) = 0
		[SubKeywordEnum(g0, key1, key2)] _subKeywordEnum ("SubKeywordEnum", float) = 0
	}
	SubShader
	{
		Pass
		{
			Tags { "RenderType" = "Opaque" }
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
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
				fixed4 col = 1;
				col.a = 1;
				
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
				
				col.rgb = frac(col.rgb);

				return col;
			}
			ENDCG
		}
	}
	CustomEditor "LWGUI.LWGUI"
}
