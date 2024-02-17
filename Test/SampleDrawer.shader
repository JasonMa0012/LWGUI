Shader "Hidden"
{
	Properties
	{
		// use Header on builtin attribute
		[Header(Header)][NoScaleOffset]
		_MainTex ("Color Map", 2D) = "white" { }
		[HDR] _Color ("Color", Color) = (1, 1, 1, 1)
		[Ramp] _Ramp ("Ramp", 2D) = "white" { }
		
		// use Title on LWGUI attribute
		[Title(Title)]
		[Tex(_, _mColor2)] _tex ("tex color", 2D) = "white" { }
		
		[Title(Title on Group)]
		// Create a folding group with name "g1"
		[Main(g1)] _group ("Group", float) = 0
		[Sub(g1)]  _float ("float", float) = 2
		
		[SubEnum(g1, name1, 2, name2, 1, name3, 0)]
		_enum ("enum", float) = 0
		[Tex(g1)][Normal] _normal ("normal", 2D) = "bump" { }
		[Sub(g1)][HDR] _hdr ("hdr", Color) = (1, 1, 1, 1)
		[SubTitle(g1, Sample SubTitle)]
		[SubToggle(g1, _)] _toggle ("toggle", float) = 0
		[SubToggle(g1, _KEYWORD)] _toggle_keyword ("toggle_keyword", float) = 0
		[Sub(g1_KEYWORD)]  _float_keyword ("float_keyword", float) = 0
		[SubPowerSlider(g1, 2)] _powerSlider ("powerSlider", Range(0, 100)) = 0

		// Display up to 4 colors in a single line
		[Color(g1, _mColor1, _mColor2, _mColor3)]
		_mColor ("multicolor", Color) = (1, 1, 1, 1)
		[HideInInspector] _mColor1 (" ", Color) = (1, 0, 0, 1)
		[HideInInspector] _mColor2 (" ", Color) = (0, 1, 0, 1)
		[HideInInspector] [HDR] _mColor3 (" ", Color) = (0, 0, 1, 1)

		// Create a drop-down menu that opens by default, without toggle
		[Main(g2, _KEYWORD, on, off)] _group2 ("group2 without toggle", float) = 1
		[Sub(g2)] _float2 ("float2", float) = 2
		[Ramp(g2)] _Ramp2 ("Ramp2", 2D) = "white" { }
		[Tooltip(Test Tooltip)]
		[Helpbox(Test Helpbox)]
		[Sub(g2)] _float_tooltip_helpbox ("float tooltip helpbox#这是中文Tooltip%これは日本語Helpboxです", float) = 0

		
		[Main(Preset, _, on, off)] _PresetGroup ("Preset Samples", float) = 0
		[Preset(Preset, LWGUI_BlendModePreset)] _BlendMode ("Blend Mode Preset", float) = 0
		[SubEnum(Preset, UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 2
		[SubEnum(Preset, UnityEngine.Rendering.BlendMode)] _SrcBlend ("SrcBlend", Float) = 1
		[SubEnum(Preset, UnityEngine.Rendering.BlendMode)] _DstBlend ("DstBlend", Float) = 0
		[SubToggle(Preset)] _ZWrite ("ZWrite ", Float) = 1
		[SubEnum(Preset, UnityEngine.Rendering.CompareFunction)] _ZTest ("ZTest", Float) = 4 // 4 is LEqual
		[SubEnum(Preset, RGBA, 15, RGB, 14)] _ColorMask ("ColorMask", Float) = 15 // 15 is RGBA (binary 1111)

	}
	
	HLSLINCLUDE
	
	
	
	ENDHLSL
	
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		LOD 100
		Cull [_Cull]
		ZWrite [_ZWrite]
		Blend [_SrcBlend] [_DstBlend]
		ColorMask [_ColorMask]

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

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
			float4 _Color;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = _Color;
				return col;
			}
			ENDCG
		}
	}
	CustomEditor "LWGUI.LWGUI"
}
