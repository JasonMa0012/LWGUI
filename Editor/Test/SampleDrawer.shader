Shader "Hidden"
{
    Properties
    {
        // use Header on builtin attribute
        [Header(Header)][NoScaleOffset]
        _MainTex ("Color Map", 2D) = "white" { }
        [HDR] _Color ("Color", Color) = (1, 1, 1, 1)
        [Ramp]_Ramp ("Ramp", 2D) = "white" { }
        
        // use Title on LWGUI attribute
        [Title(_, Title)]
        [Tex(_, _mColor2)] _tex ("tex color", 2D) = "white" { }
        
        [Title(_, Title on Group)]
        // Create a folding group with name "g1"
        [Main(g1)] _group ("Group", float) = 1
        [Sub(g1)]  _float ("float", float) = 2
        
        [KWEnum(g1, name1, key1, name2, key2, name3, key3)]
        _enum ("enum", float) = 0
        
        // Display when the keyword ("group name + keyword") is activated
        [Sub(g1key1)] _enumFloat1 ("enumFloat1", float) = 0
        [Sub(g1key2)] _enumFloat2 ("enumFloat2", float) = 0
        [Sub(g1key3)] _enumFloat3 ("enumFloat3", float) = 0
        [Sub(g1key3)] _enumFloat4_range ("enumFloat4_range", Range(0, 1)) = 0
        
        [Tex(g1)][Normal] _normal ("normal", 2D) = "bump" { }
        [Sub(g1)][HDR] _hdr ("hdr", Color) = (1, 1, 1, 1)
        [Title(g1, Sample Title)]
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
        
        [Header(Header on Group)]
        // Create a drop-down menu that opens by default, without toggle
        [Main(g2, _KEYWORD, on, off)] _group2 ("group2 without toggle", float) = 1
        [Sub(g2)] _float2 ("float2", float) = 2
        [Ramp(g2)] _Ramp2 ("Ramp2", 2D) = "white" { }

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
