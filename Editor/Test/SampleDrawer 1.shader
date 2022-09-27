Shader "Hidden"
{
    Properties
    {
        [Title(Main Samples)]
        [Main(GroupName)]
        _group ("Group", float) = 0
        [Sub(GroupName)] _float ("Float", float) = 0
        [Sub(GroupName)] _Tex ("Tex", 2D) = "white" {}


        [Main(Group1, _KEYWORD, on)] _group1 ("Group - Default Open", float) = 1
        [Sub(Group1)] _float1 ("Sub Float", float) = 0
        [Sub(Group1)] _vector1 ("Sub Vector", vector) = (1, 1, 1, 1)
        [Sub(Group1)] [HDR] _color1 ("Sub HDR Color", color) = (0.7, 0.7, 1, 1)

        [Title(Group1, Conditional Display Samples       Enum)]
        [KWEnum(Group1, Name 1, _KEY1, Name 2, _KEY2, Name 3, _KEY3)]
        _enum ("KWEnum", float) = 0

        // Display when the keyword ("group name + keyword") is activated
        [Sub(Group1_KEY1)] _key1_Float1 ("Key1 Float", float) = 0
        [Sub(Group1_KEY2)] _key2_Float2 ("Key2 Float", float) = 0
        [Sub(Group1_KEY3)] _key3_Float3_Range ("Key3 Float Range", Range(0, 1)) = 0
        [SubPowerSlider(Group1_KEY3, 10)] _key3_Float4_PowerSlider ("Key3 Power Slider", Range(0, 1)) = 0

        [Title(Group1, Conditional Display Samples       Toggle)]
        [SubToggle(Group1, _TOGGLE_KEYWORD)] _toggle ("SubToggle", float) = 0
        [Tex(Group1_TOGGLE_KEYWORD)][Normal] _normal ("Normal Keyword", 2D) = "bump" { }
        [Sub(Group1_TOGGLE_KEYWORD)] _float2 ("Float Keyword", float) = 0


        [Main(Group2, _, off, off)] _group2 ("Group - Without Toggle", float) = 0
        [Sub(Group2)] _float3 ("Float 2", float) = 0

        
        [Space]
        [Title(Channel Samples)]
        [Channel] _textureChannelMask("Texture Channel Mask (Default G)", Vector) = (0,1,0,0)
        
        [Title(Metadata Samples)]
        [Tooltip(Test multiline Tooltip, a single line supports up to 4 commas)]
        [Tooltip()]
        [Tooltip(Line 3)]
        [Tooltip()]
        [Tooltip(Line 5)]
        _float_tooltip ("Float with Tooltips", float) = 1
        [Helpbox(Test multiline Helpbox)]
        [Helpbox(Line2)]
        [Helpbox(Line3)]
        _float_helpbox ("Float with Helpbox", float) = 1

        
        [Space]
        [Main(Group3, _, on)] _group3 ("Group - Tex and Color Samples", float) = 0
        [Tex(Group3, _color)] _tex_color ("Tex with Color", 2D) = "white" { }
        [HideInInspector] _color (" ", Color) = (1, 0, 0, 1)
        [Tex(Group3, _float4)] _tex_float ("Tex with Float", 2D) = "white" { }
        [HideInInspector] _float4 (" ", float) = 0
        [Tex(Group3, _range)] _tex_range ("Tex with Range", 2D) = "white" { }
        [HideInInspector] _range (" ", Range(0,1)) = 0
        [Tex(Group3, _textureChannelMask1)] _tex_channel ("Tex with Channel", 2D) = "white" { }
        [HideInInspector] _textureChannelMask1(" ", Vector) = (0,0,0,1)

        // Display up to 4 colors in a single line (Unity 2019.2+)
        [Color(Group3, _mColor1, _mColor2, _mColor3)]
        _mColor ("Multi Color", Color) = (1, 1, 1, 1)
        [HideInInspector] _mColor1 (" ", Color) = (1, 0, 0, 1)
        [HideInInspector] _mColor2 (" ", Color) = (0, 1, 0, 1)
        [HideInInspector] [HDR] _mColor3 (" ", Color) = (0, 0, 1, 1)

        [Space]
        [Title(Ramp Samples)]
        [Ramp] _Ramp ("Ramp Map", 2D) = "white" { }


        [Space]
        [Title(MinMaxSlider Samples)]
        [MinMaxSlider(_rangeStart, _rangeEnd)] _minMaxSlider("Min Max Slider (0 - 1)", Range(0.0, 1.0)) = 1.0
        _rangeStart("Range Start", Range(0.0, 0.5)) = 0.0
        [PowerSlider(10)] _rangeEnd("Range End PowerSlider", Range(0.5, 1.0)) = 1.0
    }
    
    HLSLINCLUDE
    
    
    
    ENDHLSL
    
    SubShader
    {
        
        Pass
        {
                

        }
    }
    CustomEditor "LWGUI.LWGUI"
}
