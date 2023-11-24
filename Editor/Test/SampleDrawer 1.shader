Shader "Hidden"
{
	Properties
	{
		[ShowIf(_enum, Equal, 1)]
		[Title(ShowIf Main Samples)]
		[Main(GroupName)] _group ("Group", float) = 0
		[Sub(GroupName)] _float ("Float", float) = 0
		[Sub(GroupName)] _Tex ("Tex", 2D) = "white" { }


		[Main(Group1, _KEYWORD, on)] _group1 ("Group - Default Open", float) = 1
		[Preset(Group1, LWGUI_ShaderPropertyPreset)] _preset ("Preset Sample", float) = 0
		[Preset(Group1, LWGUI_ShaderPropertyPreset1)] _preset1 ("Preset Sample 1", float) = 0
		[Sub(Group1)] _float1 ("Sub Float", float) = 0
		[Sub(Group1)] _vector1 ("Sub Vector", vector) = (1, 1, 1, 1)
		[Sub(Group1)] [HDR] _color1 ("Sub HDR Color", color) = (0.7, 0.7, 1, 1)

		[SubTitle(Group1, Conditional Display Samples       Enum)]
		[KWEnum(Group1, Name 1, _KEY1, Name 2, _KEY2, Name 3, _KEY3)] _enum ("KWEnum", float) = 0
		[Sub(Group1)][ShowIf(_enum, Equal, 0)] _key1_Float1 ("Key1 Float", float) = 0
		[Sub(Group1)][ShowIf(_enum, Equal, 1)] _key2_Float2 ("Key2 Float", float) = 0
		[SubIntRange(Group1)][ShowIf(_enum, Equal, 2)] _key3_Int_Range ("Key3 Int Range", Range(0, 10)) = 0
		[ShowIf(_enum, Equal, 0)][ShowIf(Or, _enum, Equal, 2)]
		[SubPowerSlider(Group1, 3)] _key13_PowerSlider ("Key1 or Key3 Power Slider", Range(0, 1)) = 0


		[Main(Group2, _, off, off)] _group2 ("Group - Without Toggle", float) = 0
		[Sub(Group2)] _float3 ("Float 2", float) = 0
		[Advanced][Sub(Group2)] _Advancedfloat0 ("Advanced Float 0", float) = 0
		[Advanced][Sub(Group2)] _Advancedfloat1 ("Advanced Float 1", float) = 0
		[Advanced(Advanced Header Test)][Sub(Group2)] _Advancedfloat3 ("Advanced Float 3", float) = 0
		[Advanced][Sub(Group2)] _Advancedfloat4 ("Advanced Float 4", float) = 0
		[AdvancedHeaderProperty][Tex(Group2, _AdvancedColor7)] _AdvancedTex0 ("Advanced Header Property Test", 2D) = "white" { }
		[Advanced][HideInInspector] _AdvancedColor7 ("Advanced Color 7", Color) = (1, 1, 1, 1)
		[Advanced][Tex(Group2, _AdvancedColor0)] _AdvancedTex1 ("Advanced Tex 1", 2D) = "white" { }
		[Advanced][HideInInspector] _AdvancedColor0 ("Advanced Color 0", Color) = (1, 1, 1, 1)

		[Title(Channel Samples)]
		[Channel] _textureChannelMask ("Texture Channel Mask (Default G)", Vector) = (0, 1, 0, 0)


		[Title(Metadata Samples)]
		[Tooltip(Test multiline Tooltip, a single line supports up to 4 commas)]
		[Tooltip()]
		[Tooltip(Line 3)]
		[Tooltip(Line 4)]
		_float_tooltip ("Float with Tooltips##这是中文Tooltip#これは日本語Tooltipです", float) = 1
		[Helpbox(Test multiline Helpbox)]
		[Helpbox(Line2)]
		[Helpbox(Line3)]
		_float_helpbox ("Float with Helpbox%这是中文Helpbox%これは日本語Helpboxです", float) = 1


		[Main(Group3, _, on)] _group3 ("Group - Tex and Color Samples", float) = 0
		[Advanced][Tex(Group3)] _tex_single_line ("Tex Single Line", 2D) = "white" { }
		[Advanced][Tex(Group3, _color)] _tex_color ("Tex with Color", 2D) = "white" { }
		[Advanced][HideInInspector] _color (" ", Color) = (1, 0, 0, 1)
		[Advanced][Tex(Group3, _textureChannelMask1)] _tex_channel ("Tex with Channel", 2D) = "white" { }
		[Advanced][HideInInspector] _textureChannelMask1 (" ", Vector) = (0, 0, 0, 1)

		[Advanced][Color(Group3, _mColor1, _mColor2, _mColor3)] _mColor ("Multi Color", Color) = (1, 1, 1, 1)
		[Advanced][HideInInspector] _mColor1 (" ", Color) = (1, 0, 0, 1)
		[Advanced][HideInInspector] _mColor2 (" ", Color) = (0, 1, 0, 1)
		[Advanced][HideInInspector] [HDR] _mColor3 (" ", Color) = (0, 0, 1, 1)


		[Title(Ramp Samples)]
		[Ramp(_, RampMap, Assets.Art, 512)] _Ramp ("Ramp Map", 2D) = "white" { }


		[Title(MinMaxSlider Samples)]
		[MinMaxSlider(_rangeStart, _rangeEnd)] _minMaxSlider ("Min Max Slider (0 - 1)", Range(0.0, 1.0)) = 1.0
		_rangeStart ("Range Start", Range(0.0, 0.5)) = 0.0
		[PowerSlider(10)] _rangeEnd ("Range End PowerSlider", Range(0.5, 1.0)) = 1.0
	}
	
	HLSLINCLUDE
	
	
	
	ENDHLSL
	
	SubShader
	{
		
		Pass { }
	}
	CustomEditor "LWGUI.LWGUI"
}
