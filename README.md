# LWGUI (Light Weight Shader GUI)

[中文](https://github.com/JasonMa0012/LWGUI/blob/dev/README_CN.md) | [English](https://github.com/JasonMa0012/LWGUI)

[![](https://dcbadge.vercel.app/api/server/WwBYGXqPEh)](https://discord.gg/WwBYGXqPEh)

A lightweight, flexible, and powerful **Unity Shader GUI** system built to maximize material inspector productivity.

LWGUI has been proven in many large-scale commercial projects:  
with concise Material Property Drawer syntax, you can build complex inspectors quickly while benefiting from a modular Drawer/Decorator extension architecture, robust MetaData lifecycle caching, a complete Ramp/Preset/Toolbar toolchain, and Timeline integration.   

It significantly shortens iteration cycles while improving collaboration between artists and technical artists.

![809c4a1c-ce80-48b1-b415-7e8d4bea716e](assets~/809c4a1c-ce80-48b1-b415-7e8d4bea716e-16616214059841.png)

![LWGUI](assets~/LWGUI.png)

| ![](assets~/Pasted%20image%2020260714225931.png)                                                                                                                                    | ![](assets~/Pasted%20image%2020260714230034.png)                  |
| ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------- |
| **NEW: Now supports visual editing of LWGUI Attributes in Amplify Shader Editor! <br/> You need to install ASE v1.9.9.10+ and enable `Project Settings > LWGUI > ASE Integration`** | The new `SampleAmplifyShader` can help you get started quickly!   |
| ![image-20240716183800118](./assets~/image-20240716183800118.png)                                                                                                                   | ![](assets~/Pasted%20image%2020250522183200.png)                  |
| A more powerful Gradient editor than UE, with support for both Shader and C#                                                                                                        | Use Ramp Atlas to include multiple Ramps in one Texture           |
| ![image-20250314160119094](./assets~/image-20250314160119094.png)                                                                                                                   | ![image-20220926025611208](./assets~/image-20220926025611208.png) |
| When recording material parameter animations in Timeline, automatically capture changes to Toggle's Keywords to enable switching material Keywords at runtime.                      | Feature-rich toolbar                                              |

| With your sponsorship, I will update more actively. | 有你的赞助我会更加积极地更新                                                              |
| --------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| [paypal.me/JasonMa0012](paypal.me/JasonMa0012)         | ![723ddce6-fb86-48ff-9683-a12cf6cff7a0](./assets~/723ddce6-fb86-48ff-9683-a12cf6cff7a0.jpg) |

<!--ts-->
* [LWGUI (Light Weight Shader GUI)](#lwgui-light-weight-shader-gui)
   * [Installation](#installation)
   * [Getting Started](#getting-started)
      * [NEW: Using in Amplify Shader Editor](#new-using-in-amplify-shader-editor)
      * [Using in a Code Editor](#using-in-a-code-editor)
   * [Basic Drawers](#basic-drawers)
      * [Main &amp; Sub](#main--sub)
   * [Extra Drawers](#extra-drawers)
      * [Enum](#enum)
         * [KWEnum](#kwenum)
         * [SubEnum &amp; SubKeywordEnum](#subenum--subkeywordenum)
         * [Preset](#preset)
            * [Create Preset File](#create-preset-file)
            * [Edit Preset](#edit-preset)
      * [Numeric](#numeric)
         * [BitMask](#bitmask)
         * [MinMaxSlider](#minmaxslider)
         * [SubIntRange](#subintrange)
         * [SubPowerSlider](#subpowerslider)
         * [SubToggle](#subtoggle)
      * [Ramp](#ramp)
         * [Ramp](#ramp-1)
            * [ShaderLab](#shaderlab)
            * [C#](#c)
            * [Gradient Editor](#gradient-editor)
         * [RampAtlas](#rampatlas)
            * [Ramp Atlas Scriptable Object](#ramp-atlas-scriptable-object)
         * [RampAtlasIndexer](#rampatlasindexer)
      * [Texture](#texture)
         * [Image](#image)
         * [Tex](#tex)
      * [Vector](#vector)
         * [Channel](#channel)
         * [Color](#color)
      * [Other](#other)
         * [Button](#button)
   * [Extra Decorators](#extra-decorators)
      * [Appearance](#appearance)
         * [Title](#title)
         * [Tooltip &amp; Helpbox](#tooltip--helpbox)
         * [Hidden](#hidden)
         * [HelpURL](#helpurl)
         * [ReadOnly](#readonly)
      * [Condition](#condition)
         * [ActiveIf](#activeif)
         * [ShowIf](#showif)
         * [PassSwitch](#passswitch)
      * [Structure](#structure)
         * [Advanced &amp; AdvancedHeaderProperty](#advanced--advancedheaderproperty)
   * [LWGUI Timeline Tracks](#lwgui-timeline-tracks)
      * [MaterialKeywordToggleTrack](#materialkeywordtoggletrack)
   * [Unity Builtin Drawers](#unity-builtin-drawers)
      * [Space](#space)
      * [Header](#header)
      * [Enum](#enum-1)
      * [IntRange](#intrange)
      * [KeywordEnum](#keywordenum)
      * [PowerSlider](#powerslider)
      * [Toggle](#toggle)
   * [FAQs](#faqs)
      * [Problems Occurred After Modifying the Material in the Code](#problems-occurred-after-modifying-the-material-in-the-code)
      * [Problems Occurred After Creating the Material in the Code](#problems-occurred-after-creating-the-material-in-the-code)
   * [Custom Shader GUI](#custom-shader-gui)
      * [Custom Header and Footer](#custom-header-and-footer)
      * [Custom Drawer](#custom-drawer)
   * [Contribution](#contribution)
   * [Development Guide](#development-guide)
      * [Data Structure (Shader &gt; Material &gt; Inspector)](#data-structure-shader--material--inspector)
      * [Lifecycle](#lifecycle)
<!--te-->

## Installation

1. Make sure your environment is compatible with LWGUI

   - LWGUI <1.17: **Unity 2017.4+**
   - LWGUI >=1.17: **Unity 2021.3+**

     - **Recommended minimum version: Unity 2022.2+, lower versions can be used but may have bugs**
2. Open your project
3. `Window > Package Manager > Add > Add package from git URL` , enter: `https://github.com/JasonMa0012/LWGUI.git`

   - You can also choose to manually download the Zip from Github，then: `Package Manager > Add package from disk`
   - For Unity 2017, please extract the Zip directly to the Assets directory

## Getting Started

### NEW: Using in Amplify Shader Editor

1. Install ASE `v1.9.9.10 +`  
2. Enable: `Project Settings > LWGUI > ASE Integration` (At this point, ASE's default Custom Editor will be replaced with `LWGUI.LWGUI`)  
3. Create or open an Amplify Shader, create or select a Parameter node, and now you can edit LWGUI Drawers and Decorators:
![](assets~/Pasted%20image%2020260714231511.png)
- Each Parameter node can only have one Drawer, but can have multiple Decorators  
- Detailed descriptions of all Drawers and Decorators can be found below  
- You need to manually select the constructor, a constructor with fewer parameters means some parameters use default values  
- Don't forget to adjust the order of parameters! Incorrect order can cause layout issues  
- You can view the generated code to help solve problems

### Using in a Code Editor

1. Create a newer or use the existing Shader
2. Open the Shader in the code editor
3. At the bottom of the Shader, before the last large bracket, add line:`CustomEditor "LWGUI.LWGUI"`
4. Completed! Start using the following powerful Drawer to easily draw your Shader GUI

   - MaterialPropertyDrawer is C#-like attribute syntax, it can be used in front of shader properties to change the drawing method, more information can be found in the [official documentation](https://docs.unity3d.com/ScriptReference/MaterialPropertyDrawer.html)
   - You can refer to the example Shaders in the Test directory
   - ***Please note: Each Property can only have one Drawer, but can have multiple Decorators***

## Basic Drawers

### Main & Sub

```c#
/// Create a Folding Group
/// 
/// group: group name (Default: Property Name)
/// keyword: keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
/// default Folding State: "on" or "off" (Default: off)
/// default Toggle Displayed: "on" or "off" (Default: on)
/// preset File Name: "Shader Property Preset" asset name, see Preset() for detail (Default: none)
/// Target Property Type: Float, express Toggle value
public MainDrawer() : this(String.Empty) { }
public MainDrawer(string group) : this(group, String.Empty) { }
public MainDrawer(string group, string keyword) : this(group, keyword, "off") { }
public MainDrawer(string group, string keyword, string defaultFoldingState) : this(group, keyword, defaultFoldingState, "on") { }
public MainDrawer(string group, string keyword, string defaultFoldingState, string defaultToggleDisplayed) : this(group, keyword, defaultFoldingState, defaultToggleDisplayed, String.Empty) { }
public MainDrawer(string group, string keyword, string defaultFoldingState, string defaultToggleDisplayed, string presetFileName)

```

```c#
/// Draw a property with default style in the folding group
/// 
/// group: parent group name (Default: none)
/// Target Property Type: Any
public SubDrawer() { }
public SubDrawer(string group)

```

Example:

```c#
[Title(Main Samples)]
[Main(GroupName)]
_group ("Group", float) = 0
[Sub(GroupName)] _float ("Float", float) = 0


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

```

Default result:

![image-20220828003026556](assets~/image-20220828003026556.png)

Then change values:

![image-20220828003129588](assets~/image-20220828003129588.png)

## Extra Drawers

### Enum

#### KWEnum

```c#
/// Similar to builtin Enum() / KeywordEnum()
/// 
/// group: parent group name (Default: none)
/// n(s): display name
/// k(s): keyword
/// v(s): value
/// Target Property Type: Float, express current keyword index
public KWEnumDrawer(string n1, string k1)
public KWEnumDrawer(string n1, string k1, string n2, string k2)
public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3)
public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
  
public KWEnumDrawer(string group, string n1, string k1)
public KWEnumDrawer(string group, string n1, string k1, string n2, string k2)
public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3)
public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
```

#### SubEnum & SubKeywordEnum

```c#
// enumName: like "UnityEngine.Rendering.BlendMode"
public SubEnumDrawer(string group, string enumName) : base(group, enumName)

public SubEnumDrawer(string group, string n1, float v1, string n2, float v2)
public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3)
public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4)
public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5)
public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6)
public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7)


public SubKeywordEnumDrawer(string group, string kw1, string kw2)
public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3)
public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4)
public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5)
public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6)
public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7)
public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8)
public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9)

```

#### Preset

```c#
/// Popping a menu, you can select the Shader Property Preset, the Preset values will replaces the default values
/// 
/// group: parent group name (Default: none)
///	presetFileName: "Shader Property Preset" asset name, you can create new Preset by
///		"Right Click > Create > LWGUI > Shader Property Preset" in Project window,
///		*any Preset in the entire project cannot have the same name*
/// Target Property Type: Float, express current keyword index
public PresetDrawer(string presetFileName) : this("_", presetFileName) {}
public PresetDrawer(string group, string presetFileName)

```

Example:

```c#
[Title(Preset Samples)]
[Preset(LWGUI_BlendModePreset)] _BlendMode ("Blend Mode Preset", float) = 0 
[Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 2
[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1
[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0
[Toggle(_)]_ZWrite("ZWrite ", Float) = 1
[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4 // 4 is LEqual
[Enum(RGBA,15,RGB,14)]_ColorMask("ColorMask", Float) = 15 // 15 is RGBA (binary 1111)

......
  
Cull [_Cull]
ZWrite [_ZWrite]
Blend [_SrcBlend] [_DstBlend]
ColorMask [_ColorMask]
```

Result:

The Property Value in the selected Preset will be the default value:

![image-20221122231655378](assets~/image-20221122231655378.png)![image-20221122231816714](assets~/image-20221122231816714.png)

##### Create Preset File

![image-20221122232307362](assets~/image-20221122232307362.png)

##### Edit Preset

![image-20221122232354623](assets~/image-20221122232354623.png)![image-20221122232415972](assets~/image-20221122232415972.png)![image-20221122232425194](assets~/image-20221122232425194.png)

### Numeric

#### BitMask

```C#
/// Draw the Int value as a Bit Mask.
/// Note:
///    - Currently only 8 bits are supported.
///
/// Warning 1: If used to set Stencil, it will conflict with SRP Batcher!  
///		(Reproduced in Unity 2022)  
///		SRP Batcher does not correctly handle multiple materials with different Stencil Ref values,  
///		mistakenly merging them into a single Batch and randomly selecting one material's Stencil Ref value for the entire Batch.  
///		In theory, if different materials have different Stencil Ref values, they should not be merged into a single Batch due to differing Render States.  
/// Solution:  
///		- Force disable SRP Batcher by setting the Material Property Block  
///		- Place materials with the same Stencil Ref value in a separate Render Queue to ensure the Batch's Render State is correct
///
/// Warning 2: Once in use, do not change the Target Property Type!
///		The underlying type of Int Property is Float Property, and in Materials, Int and Integer are stored separately.  
///		Once a Material is saved, the Property Type is determined.  
///		If you change the Property Type at this point (such as switching between Int/Integer), some strange bugs may occur.  
///		If you must change the Property Type, it is recommended to modify the Property Name as well or delete the saved Property in the material.
/// group: parent group name (Default: none)
/// bitDescription 7-0: Description of each Bit. (Default: none)
/// Target Property Type: Int
public BitMaskDrawer() : this(string.Empty, null) { }
public BitMaskDrawer(string group) : this(group, null) { }
public BitMaskDrawer(string group, string bitDescription7, string bitDescription6, string bitDescription5, string bitDescription4, string bitDescription3, string bitDescription2, string bitDescription1, string bitDescription0) 
    : this(group, new List<string>() { bitDescription0, bitDescription1, bitDescription2, bitDescription3, bitDescription4, bitDescription5, bitDescription6, bitDescription7 }) { }

```

Example:

```C#
[BitMask(Preset)] _Stencil ("Stencil", Integer) = 0  
[BitMask(Preset, Left, Bit6, Bit5, Bit4, Description, Bit2, Bit1, Right)] _StencilWithDescription ("Stencil With Description", Integer) = 0
```

Result:
![](assets~/Pasted%20image%2020250321174432.png)

> [!CAUTION]
> If used to set Stencil, it will conflict with SRP Batcher!
> (Reproduced in Unity 2022)
>
> SRP Batcher does not correctly handle multiple materials with different Stencil Ref values,
> mistakenly merging them into a single Batch and randomly selecting one material's Stencil Ref value for the entire Batch.
> In theory, if different materials have different Stencil Ref values, they should not be merged into a single Batch due to differing Render States.
>
> Solution:
>
> - Force disable SRP Batcher by setting the Material Property Block
> - Place materials with the same Stencil Ref value in a separate Render Queue to ensure the Batch's Render State is correct

#### MinMaxSlider

```c#
/// Draw a min max slider
/// 
/// group: parent group name (Default: none)
/// minPropName: Output Min Property Name
/// maxPropName: Output Max Property Name
/// Target Property Type: Range, range limits express the MinMaxSlider value range
/// Output Min/Max Property Type: Range, it's value is limited by it's range
public MinMaxSliderDrawer(string minPropName, string maxPropName) : this("_", minPropName, maxPropName) { }
public MinMaxSliderDrawer(string group, string minPropName, string maxPropName)

```

Example:

```c#
[Title(MinMaxSlider Samples)]
[MinMaxSlider(_rangeStart, _rangeEnd)] _minMaxSlider("Min Max Slider (0 - 1)", Range(0.0, 1.0)) = 1.0
/*[HideInInspector]*/_rangeStart("Range Start", Range(0.0, 0.5)) = 0.0
/*[HideInInspector]*/[PowerSlider(10)] _rangeEnd("Range End PowerSlider", Range(0.5, 1.0)) = 1.0

```

Result:

![image-20220828003810353](assets~/image-20220828003810353.png)

#### SubIntRange

```c#
/// Similar to builtin IntRange()
/// 
/// group: parent group name (Default: none)
/// Target Property Type: Range
public SubIntRangeDrawer(string group)

```

#### SubPowerSlider

```c#
/// Similar to builtin PowerSlider()
/// 
/// group: parent group name (Default: none)
/// power: power of slider (Default: 1)
/// presetFileName: "Shader Property Preset" asset name, it rounds up the float to choose which Preset to use.  
///    You can create new Preset by  
///    "Right Click > Create > LWGUI > Shader Property Preset" in Project window,  
///    *any Preset in the entire project cannot have the same name*
/// Target Property Type: Range
public SubPowerSliderDrawer(float power) : this("_", power) { }  
public SubPowerSliderDrawer(string group, float power) : this(group, power, string.Empty) { }  
public SubPowerSliderDrawer(string group, float power, string presetFileName)
```

#### SubToggle

```c#
/// Similar to builtin Toggle()
/// 
/// group: parent group name (Default: none)
/// keyword: keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
/// preset File Name: "Shader Property Preset" asset name, see Preset() for detail (Default: none)
/// Target Property Type: Float
public SubToggleDrawer() { }
public SubToggleDrawer(string group) : this(group, String.Empty, String.Empty) { }
public SubToggleDrawer(string group, string keyWord) : this(group, keyWord, String.Empty) { }
public SubToggleDrawer(string group, string keyWord, string presetFileName)
```

### Ramp

#### Ramp

##### ShaderLab

```c#
/// Draw an unreal style Ramp Map Editor (Default Ramp Map Resolution: 512 * 2)
/// NEW: The new LwguiGradient type has both the Gradient and Curve editors, and can be used in C# scripts and runtime, and is intended to replace UnityEngine.Gradient
///
/// group: parent group name (Default: none)
/// defaultFileName: default Ramp Map file name when create a new one (Default: RampMap)
/// rootPath: the path where ramp is stored, replace '/' with '.' (for example: Assets.Art.Ramps). when selecting ramp, it will also be filtered according to the path (Default: Assets)
/// colorSpace: switch sRGB / Linear in ramp texture import setting (Default: sRGB)
/// defaultWidth: default Ramp Width (Default: 256)
/// viewChannelMask: editable channels. (Default: RGBA)
/// timeRange: the abscissa display range (1/24/2400), is used to optimize the editing experience when the abscissa is time of day. (Default: 1)
/// Target Property Type: Texture2D
public RampDrawer() : this(String.Empty) { }
public RampDrawer(string group) : this(group, "RampMap") { }
public RampDrawer(string group, string defaultFileName) : this(group, defaultFileName, DefaultRootPath, 256) { }
public RampDrawer(string group, string defaultFileName, float defaultWidth) : this(group, defaultFileName, DefaultRootPath, defaultWidth) { }
public RampDrawer(string group, string defaultFileName, string rootPath, float defaultWidth) : this(group, defaultFileName, rootPath, "sRGB", defaultWidth) { }
public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, "RGBA") { }
public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, string viewChannelMask) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, viewChannelMask, 1) { }
public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, string viewChannelMask, float timeRange)
```

Example:

```c#
[Ramp(_, RampMap, Assets.Art, 512)] _Ramp ("Ramp Map", 2D) = "white" { }
```

Result:

![image-20230625185730363](./assets~/image-20230625185730363.png)

You **must manually Save the edit results**, if there are unsaved changes, the Save button will display yellow.

**When you move or copy the Ramp Map, remember to move together with the .meta file, otherwise you will not be able to edit it again!**

##### C#

Example:

```c#
public class Test : MonoBehaviour
{
    public LwguiGradient lwguiGradientSrgb = new LwguiGradient();

    [LwguiGradientUsage(ColorSpace.Linear, LwguiGradient.ChannelMask.RGB, LwguiGradient.GradientTimeRange.TwentyFourHundred)]
    public LwguiGradient lwguiGradientLinear = new LwguiGradient();
}
```

Result:

![image-20240717104144821](./assets~/image-20240717104144821.png)![image-20240717104206365](./assets~/image-20240717104206365.png)

Default display settings can be set using the LwguiGradientUsage() Attribute.

##### Gradient Editor

The new LWGUI Gradient Editor integrates with Unity's built-in [Gradient Editor](https://docs.unity3d.com/Manual/EditingValueProperties.html) and [Curve Editor](https://docs.unity3d.com/Manual/EditingCurves.html), enabling more powerful features than UE's Gradient Editor.

![image-20241126110012922](./assets~/image-20241126110012922.png)

| Editor                | Description                                                                                                                                                                                                                                                                                                                                                                                                                 |
| --------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Time Range            | The display range of the horizontal axis, selectable from 0-1 / 0-24 / 0-2400, is very useful when the horizontal axis is time. Note that only the display is affected, the actual value stored in the horizontal axis is always 0-1.                                                                                                                                                                                       |
| Channels              | The channels displayed. Can be displayed individually.                                                                                                                                                                                                                                                                                                                                                                      |
| sRGB Preview          | It should be checked when the value of Gradient is a color to preview the correct color, otherwise it doesn't need to be checked. Only affects the display, Gradient and Ramp Map are always stored as Linear.                                                                                                                                                                                                              |
| Value / R / G / B / A | Used to edit the Value of the selected Key, you can edit the Value of multiple Keys at the same time.                                                                                                                                                                                                                                                                                                                       |
| Time                  | Used to edit the Time of the selected Key, you can edit the Time of multiple Keys at the same time. If you enter a number manually, you must**press enter** to end the editing.                                                                                                                                                                                                                                       |
| Gradient Editor       | This is similar to Unity's built-in[Gradient Editor](https://docs.unity3d.com/Manual/EditingValueProperties.html), but the Alpha channels are separated into black and white. ``Note that **adding keys from the Gradient Editor is limited to a maximum of 8 keys**, adding keys from the Curve Editor is **unlimited**. Exceeding the key limit will not affect preview or usage.                         |
| Curve Editor          | Similar to Unity's built-in Curve Editor, it displays the XY 0-1 range by default, and you can use the scroll wheel to zoom or move the display range.``As you can see in the image below, the context menu has a number of functions for controlling the shape of the curve, and you can consult the [Unity documentation](https://docs.unity3d.com/Manual/EditingCurves.html) to get the most out of these functions. |
| Presets               | You can save the current LWGUI Gradient as a preset and apply it anytime. These presets are common between different engine versions on the local computer, but are not saved to the project.                                                                                                                                                                                                                               |

![image-20241126105823397](./assets~/image-20241126105823397.png)![image-20241126112320151](./assets~/image-20241126112320151.png)

> [!NOTE]
> **Known issues:**
>
> - Preview images below Unity 2022 have no difference between sRGB/Linear color spaces
> - Ctrl + Z results may be slightly different from expected when the editor frame rate is too low

#### RampAtlas

```c#
/// Draw a "Ramp Atlas Scriptable Object" selector and texture preview.  
/// The Ramp Atlas SO is responsible for storing multiple ramps and generating the corresponding Ramp Atlas Texture.  
/// Use it together with RampAtlasIndexer() to sample specific ramps in Shader using Index, similar to UE's Curve Atlas.  
/// Note: Currently, the material only saves Texture reference and Int value,  
///      if you manually modify the Ramp Atlas, the references will not update automatically!
///  
/// group: parent group name (Default: none)
/// defaultFileName: the default file name when creating a Ramp Atlas SO (Default: RampAtlas)
/// rootPath: the default directory when creating a Ramp Atlas SO, replace '/' with '.' (for example: Assets.Art.RampAtlas). (Default: Assets)
/// colorSpace: the Color Space of Ramp Atlas Texture. (sRGB/Linear) (Default: sRGB)
/// defaultWidth: default Ramp Atlas Texture width (Default: 256)
/// defaultHeight: default Ramp Atlas Texture height (Default: 4)
/// showAtlasPreview: Draw the preview of Ramp Atlas below (True/False) (Default: True)
/// rampAtlasTypeName: custom RampAtlas type name for user-defined RampAtlas classes (Default: LwguiRampAtlas)
/// Target Property Type: Texture2D
public RampAtlasDrawer() : this(string.Empty) { }  
public RampAtlasDrawer(string group) : this(group, "RampAtlas") { }  
public RampAtlasDrawer(string group, string defaultFileName) : this(group, defaultFileName, "Assets") { }  
public RampAtlasDrawer(string group, string defaultFileName, string rootPath) : this(group, defaultFileName, rootPath, "sRGB") { }  
public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace) : this(group, defaultFileName, rootPath, colorSpace, 256) { } 
public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, 4) { }  
public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, float defaultHeight) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, defaultHeight, "true") { }  
public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, float defaultHeight, string showAtlasPreview) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, defaultHeight, showAtlasPreview, "") { }  
public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, float defaultHeight, string showAtlasPreview, string rampAtlasTypeName)
```

Example:

```c#
[RampAtlas(g2)] _RampAtlas ("Ramp Atlas", 2D) = "white" { }  
[Space]  
[RampAtlasIndexer(g2, _RampAtlas, Default Ramp)] _RampAtlasIndex0 ("Indexer", float) = 0  
[RampAtlasIndexer(g2, _RampAtlas, Default Ramp)] _RampAtlasIndex1 ("Indexer", float) = 1  
[RampAtlasIndexer(g2, _RampAtlas, Green, Linear, GA, 24)] _RampAtlasIndex2 ("Indexer Linear/Green/24", float) = 3
```

Result:
![](assets~/Pasted%20image%2020250522183200.png)

Shaderlab:

```c#
sampler2D _RampAtlas;  
float4 _RampAtlas_TexelSize;  
int _RampAtlasIndex0;

......

float2 rampUV = float2(i.uv.x, _RampAtlas_TexelSize.y * (_RampAtlasIndex0 + 0.5f));  
fixed4 color = tex2D(_RampAtlas, saturate(rampUV));
```

##### Ramp Atlas Scriptable Object

Ramp Atlas SO is responsible for storing and generating Ramp Atlas Texture:
![](assets~/Pasted%20image%2020250523120309.png)
When loading an SO or modifying a Ramp on a material, a Ramp Atlas Texture will be automatically created at the same path as the SO, with the file extension `.tga`.
After manually modifying the SO, you need to click `Save Texture Toggle` to generate the Texture.

You can create an SO in the following ways:

- Right-click in the Project panel: `Create > LWGUI > Ramp Atlas`
- Right-click on a material property using RampAtlas(): `Create Ramp Atlas` or `Clone Ramp Atlas`
  - An SO created this way will include default values for all Ramps in the current material.

You can click the add button of RampAtlasIndexer() to add a new Ramp to the SO.

The context menu in the upper right corner has a one-click color space conversion feature.

> [!CAUTION]
> Currently, the material only saves Texture references and Int values. If you manually modify the number and order of Ramps in the Ramp Atlas SO, the selected Ramps in the material may be disrupted!
>
> Suggestions:
>
> - Limit the usage scope of a single Ramp Atlas
> - Only add Ramps
> - Do not modify the Ramp order

#### RampAtlasIndexer

```c#
/// Visually similar to Ramp(), but RampAtlasIndexer() must be used together with RampAtlas().  
/// The actual stored value is the index of the current Ramp in the Ramp Atlas SO, used for sampling the Ramp Atlas Texture in the Shader.
///  
/// group: parent group name.  
/// rampAtlasPropName: RampAtlas() property name.  
/// defaultRampName: default ramp name. (Default: Ramp)  
/// colorSpace: default ramp color space. (sRGB/Linear) (Default: sRGB)  
/// viewChannelMask: editable channels. (Default: RGBA)  
/// timeRange: the abscissa display range (1/24/2400), is used to optimize the editing experience when the abscissa is time of day. (Default: 1)  
/// Target Property Type: Float
public RampAtlasIndexerDrawer(string group, string rampAtlasPropName) : this(group, rampAtlasPropName, "Ramp") {}  
public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName) : this(group, rampAtlasPropName, defaultRampName, "sRGB") {}  
public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace) : this(group, rampAtlasPropName, defaultRampName, colorSpace, "RGBA") {}  
public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace, string viewChannelMask) : this(group, rampAtlasPropName, defaultRampName, colorSpace, viewChannelMask, 1) {}  
public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace, string viewChannelMask, float timeRange)  
```

See details for usage: RampAtlas()

### Texture

#### Image

```c#
/// Draw an image preview.
/// display name: The path of the image file relative to the Unity project, such as: "assets~/test.png", "Doc/test.png", "../test.png"
/// 
/// group: parent group name (Default: none)
/// Target Property Type: Any
public ImageDrawer() { }
public ImageDrawer(string group)
```

Result:

![image-20240416142736663](./assets~/image-20240416142736663.png)

#### Tex

```c#
/// Draw a Texture property in single line with a extra property
/// 
/// group: parent group name (Default: none)
/// extraPropName: extra property name (Default: none)
/// Target Property Type: Texture
/// Extra Property Type: Color, Vector
/// Target Property Type: Texture2D
public TexDrawer() { }
public TexDrawer(string group) : this(group, String.Empty) { }
public TexDrawer(string group, string extraPropName)

```

Example:

```c#
[Main(Group3, _, on)] _group3 ("Group - Tex and Color Samples", float) = 0
[Tex(Group3, _color)] _tex_color ("Tex with Color", 2D) = "white" { }
[HideInInspector] _color (" ", Color) = (1, 0, 0, 1)
[Tex(Group3, _textureChannelMask1)] _tex_channel ("Tex with Channel", 2D) = "white" { }
[HideInInspector] _textureChannelMask1(" ", Vector) = (0,0,0,1)

// Display up to 4 colors in a single line
[Color(Group3, _mColor1, _mColor2, _mColor3)]
_mColor ("Multi Color", Color) = (1, 1, 1, 1)
[HideInInspector] _mColor1 (" ", Color) = (1, 0, 0, 1)
[HideInInspector] _mColor2 (" ", Color) = (0, 1, 0, 1)
[HideInInspector] [HDR] _mColor3 (" ", Color) = (0, 0, 1, 1)

```

Result:

![image-20220828003507825](assets~/image-20220828003507825.png)

### Vector

#### Channel

```c#
/// Draw a R/G/B/A drop menu:
/// 	R = (1, 0, 0, 0)
/// 	G = (0, 1, 0, 0)
/// 	B = (0, 0, 1, 0)
/// 	A = (0, 0, 0, 1)
/// 	RGB Average = (1f / 3f, 1f / 3f, 1f / 3f, 0)
/// 	RGB Luminance = (0.2126f, 0.7152f, 0.0722f, 0)
///		None = (0, 0, 0, 0)
/// 
/// group: parent group name (Default: none)
/// Target Property Type: Vector, used to dot() with Texture Sample Value
public ChannelDrawer() { }
public ChannelDrawer(string group)
```

Example:

```c#
[Title(_, Channel Samples)]
[Channel(_)]_textureChannelMask("Texture Channel Mask (Default G)", Vector) = (0,1,0,0)

......

float selectedChannelValue = dot(tex2D(_Tex, uv), _textureChannelMask);
```

![image-20220822010511978](assets~/image-20220822010511978.png)

#### Color

```c#
/// Display up to 4 colors in a single line
/// 
/// group: parent group name (Default: none)
/// color2-4: extra color property name
/// Target Property Type: Color
public ColorDrawer(string group, string color2) : this(group, color2, String.Empty, String.Empty) { }
public ColorDrawer(string group, string color2, string color3) : this(group, color2, color3, String.Empty) { }
public ColorDrawer(string group, string color2, string color3, string color4)

```

Example:

```c#
[Main(Group3, _, on)] _group3 ("Group - Tex and Color Samples", float) = 0
[Tex(Group3, _color)] _tex_color ("Tex with Color", 2D) = "white" { }
[HideInInspector] _color (" ", Color) = (1, 0, 0, 1)
[Tex(Group3, _textureChannelMask1)] _tex_channel ("Tex with Channel", 2D) = "white" { }
[HideInInspector] _textureChannelMask1(" ", Vector) = (0,0,0,1)

// Display up to 4 colors in a single line
[Color(Group3, _mColor1, _mColor2, _mColor3)]
_mColor ("Multi Color", Color) = (1, 1, 1, 1)
[HideInInspector] _mColor1 (" ", Color) = (1, 0, 0, 1)
[HideInInspector] _mColor2 (" ", Color) = (0, 1, 0, 1)
[HideInInspector] [HDR] _mColor3 (" ", Color) = (0, 0, 1, 1)

```

Result:

![image-20220828003507825](assets~/image-20220828003507825.png)

### Other

#### Button

```c#
/// Draw one or more Buttons within the same row, using the Display Name to control the appearance and behavior of the buttons
/// 
/// Declaring a set of Button Name and Button Command in Display Name generates a Button, separated by '@':
/// ButtonName0@ButtonCommand0@ButtonName1@ButtonCommand1
/// 
/// Button Name can be any other string, the format of Button Command is:
/// TYPE:Argument
/// 
/// The following TYPEs are currently supported:
/// - URL: Open the URL, Argument is the URL
/// - C#: Call the public static C# function, Argument is NameSpace.Class.Method(arg0, arg1, ...),
///		for target function signatures, see: LWGUI.ButtonDrawer.TestMethod().
///
/// The full example:
/// [Button(_)] _button0 ("URL Button@URL:https://github.com/JasonMa0012/LWGUI@C#:LWGUI.ButtonDrawer.TestMethod(1234, abcd)", Float) = 0
/// 
/// group: parent group name (Default: none)
/// Target Property Type: Any
public ButtonDrawer() { }
public ButtonDrawer(string group)
```

Example:

```c#
[Title(Button Samples)]
[Button(_)] _button0 ("URL Button@URL:https://github.com/JasonMa0012/LWGUI@C# Button@C#:LWGUI.ButtonDrawer.TestMethod(1234, abcd)", Float) = 0

```

![image-20241127180711449](./assets~/image-20241127180711449.png)

## Extra Decorators

### Appearance

#### Title

```c#
/// <summary>
/// Similar to Header()
/// 
/// group: parent group name (Default: none)
/// header: string to display, "SpaceLine" or "_" = none (Default: none)
/// height: line height (Default: 22)
public TitleDecorator(string header) : this("_", header, DefaultHeight) {}
public TitleDecorator(string header, float  height) : this("_", header, height) {}
public TitleDecorator(string group,  string header) : this(group, header, DefaultHeight) {}
public TitleDecorator(string group, string header, float height)


/// Similar to Title()
/// 
/// group: parent group name (Default: none)
/// header: string to display, "SpaceLine" or "_" = none (Default: none)
/// height: line height (Default: 22)
public SubTitleDecorator(string group,  string header) : base(group, header, DefaultHeight) {}
public SubTitleDecorator(string group, string header, float height) : base(group, header, height) {}

```

#### Tooltip & Helpbox

```c#
/// Tooltip, describes the details of the property. (Default: property.name and property default value)
/// You can also use "#Text" in DisplayName to add Tooltip that supports Multi-Language.
/// 
/// tooltip: a single-line string to display, support up to 4 ','. (Default: Newline)
public TooltipDecorator() : this(string.Empty) {}
public TooltipDecorator(string tooltip) { this._tooltip = tooltip; }
public TooltipDecorator(string s1, string s2) : this(s1 + ", " + s2) { }
public TooltipDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }
public TooltipDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }
public TooltipDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }


```

```c#
/// Display a Helpbox on the property
/// You can also use "%Text" in DisplayName to add Helpbox that supports Multi-Language.
/// 
/// message: a single-line string to display, support up to 4 ','. (Default: Newline)
public HelpboxDecorator() : this(string.Empty) {}
public HelpboxDecorator(string message) { this._message = message; }
public HelpboxDecorator(string s1, string s2) : this(s1 + ", " + s2) { }
public HelpboxDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }
public HelpboxDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }
public HelpboxDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }


```

Example:

```c#
[Title(Metadata Samples)]
[Tooltip(Test multiline Tooltip, a single line supports up to 4 commas)]
[Tooltip()]
[Tooltip(Line 3)]
[Tooltip(Line 4)]
_float_tooltip ("Float with Tooltips#这是中文Tooltip#これは日本語Tooltipです", float) = 1
[Helpbox(Test multiline Helpbox)]
[Helpbox(Line2)]
[Helpbox(Line3)]
_float_helpbox ("Float with Helpbox%这是中文Helpbox%これは日本語Helpboxです", float) = 1

```

![image-20221231221240686](assets~/image-20221231221240686.png)

![image-20221231221254101](assets~/image-20221231221254101.png)

Tips:

- Tooltip may disappear when the Editor is running. This is a feature of Unity itself (or a bug)

#### Hidden

```c#
/// Similar to HideInInspector(), the difference is that Hidden() can be unhidden through the Display Mode button.
public HiddenDecorator()
```

#### HelpURL

```c#
/// Display a Help URL button on the right side of the Group.
/// Clicking the button opens the URL in the default browser.
/// "https://" is automatically prepended.
/// Each comma-separated parameter is joined with '/' to form the full URL path.
/// 
/// url: the document URL without "https://", each ',' = '/'. (Default: none)
/// e.g. [HelpURL(github.com, JasonMa0012, LWGUI)] => https://github.com/JasonMa0012/LWGUI
public HelpURLDecorator(string url)
public HelpURLDecorator(string s1, string s2)
...
public HelpURLDecorator(string s1, string s2, ..., string s16)
```

Example:

```c#
[HelpURL(github.com, JasonMa0012, LWGUI)]
[Main(GroupName)] _group ("Group", float) = 0

[HelpURL(github.com, JasonMa0012, LWGUI, blob, 1.x, README.md)]
[Main(GroupName2, _, on, off)] _group2 ("Group2", float) = 0
```

#### ReadOnly

```c#
/// Set the property to read-only.
public ReadOnlyDecorator()
```

### Condition

#### ActiveIf

```c#
/// Control whether a single property or a group can be edited based on multiple conditions.
/// 
/// logicalOperator: And | Or (Default: And).
/// propNameOrKeyword: Target Property Name or Keyword used for comparison. If no matching property is found, it falls back to checking material keywords (enabled = 1, disabled = 0).
/// compareFunction: Less (L) | Equal (E) | LessEqual (LEqual / LE) | Greater (G) | NotEqual (NEqual / NE) | GreaterEqual (GEqual / GE).
/// value: Target Property Value used for comparison.
/// 
/// When the condition is false, the property is read-only.
public ActiveIfDecorator(string propNameOrKeyword, string comparisonMethod, float value) : this("And", propNameOrKeyword, comparisonMethod, value) { }
public ActiveIfDecorator(string logicalOperator, string propNameOrKeyword, string compareFunction, float value)
```

Example:

```c#
[Main(GroupName)] _group ("Group", float) = 0
[Sub(GroupName)][KWEnum(Key 1, _KEY1, key 2, _KEY2)] _enum ("KWEnum", float) = 0
[Sub(GroupName)][ActiveIf(_enum, Equal, 0)] _float0 ("Editable only when key 1", float) = 0
[Sub(GroupName)][ActiveIf(_enum, E, 1)] _float1 ("Editable only when key 2", float) = 0
[Sub(GroupName)][ActiveIf(Or, _enum, E, 0)][ActiveIf(Or, _enum, G, 0)] _float2 ("Editable when key >= 0", float) = 0
```

#### ShowIf

```c#
/// Control the show or hide of a single or a group of properties based on multiple conditions.
///
/// logicalOperator: And | Or (Default: And).
/// propNameOrKeyword: Target Property Name or Keyword used for comparison. If no matching property is found, it falls back to checking material keywords (enabled = 1, disabled = 0).
/// compareFunction: Less (L) | Equal (E) | LessEqual (LEqual / LE) | Greater (G) | NotEqual (NEqual / NE) | GreaterEqual (GEqual / GE).
/// value: Target Property Value used for comparison.
public ShowIfDecorator(string propNameOrKeyword, string comparisonMethod, float value) : this("And", propNameOrKeyword, comparisonMethod, value) { }
public ShowIfDecorator(string logicalOperator, string propNameOrKeyword, string compareFunction, float value)
```

Example:

```c#
[ShowIf(_enum, Equal, 1)]
[Title(ShowIf Main Samples)]
[Main(GroupName)] _group ("Group", float) = 0
[Sub(GroupName)] _float ("Float", float) = 0
[Sub(GroupName)] _Tex ("Tex", 2D) = "white" { }

...

[SubTitle(Group1, Conditional Display Samples       Enum)]
[KWEnum(Group1, Name 1, _KEY1, Name 2, _KEY2, Name 3, _KEY3)] _enum ("KWEnum", float) = 0
[Sub(Group1)][ShowIf(_enum, Equal, 0)] _key1_Float1 ("Key1 Float", float) = 0
[Sub(Group1)][ShowIf(_enum, Equal, 1)] _key2_Float2 ("Key2 Float", float) = 0
[SubIntRange(Group1)][ShowIf(_enum, Equal, 2)] _key3_Int_Range ("Key3 Int Range", Range(0, 10)) = 0
[ShowIf(_enum, Equal, 0)][ShowIf(Or, _enum, Equal, 2)]
[SubPowerSlider(Group1, 3)] _key13_PowerSlider ("Key1 or Key3 Power Slider", Range(0, 1)) = 0

```

![image-20231023010137495](./assets~/image-20231023010137495.png)

![image-20231023010153213](./assets~/image-20231023010153213.png)

![image-20231023010204399](./assets~/image-20231023010204399.png)

#### PassSwitch

```c#
/// Cooperate with Toggle to switch certain Passes.
/// 
/// lightModeName(s): Light Mode in Shader Pass (https://docs.unity3d.com/2017.4/Documentation/Manual/SL-PassTags.html)
public PassSwitchDecorator(string   lightModeName1) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2, string lightModeName3) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4, string lightModeName5) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4, string lightModeName5, string lightModeName6) 

```

### Structure

#### Advanced & AdvancedHeaderProperty

```c#
/// Collapse the current Property into an Advanced Block.
/// Specify the Header String to create a new Advanced Block.
/// All Properties using Advanced() will be collapsed into the nearest Advanced Block.
/// 
/// headerString: The title of the Advanced Block. Default: "Advanced"
public AdvancedDecorator() : this(string.Empty) { }
public AdvancedDecorator(string headerString)
```

```c#
/// Create an Advanced Block using the current Property as the Header.
public AdvancedHeaderPropertyDecorator()
```

Example:

```c#
[Main(Group2, _, off, off)] _group2 ("Group - Without Toggle", float) = 0
[Sub(Group2)] _float3 ("Float 2", float) = 0
[Advanced][Sub(Group2)] _Advancedfloat0 ("Advanced Float 0", float) = 0
[Advanced][Sub(Group2)] _Advancedfloat1 ("Advanced Float 1", float) = 0
[Advanced(Advanced Header Test)][Sub(Group2)] _Advancedfloat3 ("Advanced Float 3", float) = 0
[Advanced][Sub(Group2)] _Advancedfloat4 ("Advanced Float 4", float) = 0
[AdvancedHeaderProperty][Tex(Group2, _Advancedfloat7)] _AdvancedTex0 ("Advanced Header Property Test", 2D) = "white" { }
[Advanced][HideInInspector] _Advancedfloat7 ("Advanced Float 7", float) = 0
[Advanced][Tex(Group2, _AdvancedRange0)] _AdvancedTex1 ("Advanced Tex 1", 2D) = "white" { }
[Advanced][HideInInspector] _AdvancedRange0 ("Advanced Range 0", Range(0, 1)) = 0

```

![image-20231007163044176](./assets~/image-20231007163044176.png)

Tips:

- LWGUI uses a tree data structure to store the relationship between Group, Advanced Block and their children. In theory, it can store unlimited multi-level parent-child relationships, but **currently LWGUI only manually handles 3-level parent-child relationships, which means you can put an Advanced Block in a Group, but a Group cannot be placed in an Advanced Block.**

## LWGUI Timeline Tracks

### MaterialKeywordToggleTrack

When recording material parameter animation, Keyword changes are automatically captured and the track is added to the Timeline Asset. The Keyword state is set according to the float value during runtime.

Supports Toggle-type Drawer with Keyword.

## Unity Builtin Drawers

### Space

```c#
MaterialSpaceDecorator(float height)
```

### Header

```c#
MaterialHeaderDecorator(string header)
```

### Enum

```c#
MaterialEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7)
```

### IntRange

```c#
MaterialIntRangeDrawer()
```

### KeywordEnum

```c#
MaterialKeywordEnumDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9)
```

### PowerSlider

```c#
MaterialPowerSliderDrawer(float power)
```

### Toggle

```c#
MaterialToggleUIDrawer(string keyword)
```

## FAQs

### Problems Occurred After Modifying the Material in the Code

After modifying material properties in the code, the Drawer logic ***does not*** run, potentially losing some data (e.g., Keywords).
You need to manually call `LWGUI.UnityEditorExtension.ApplyMaterialPropertyAndDecoratorDrawers()` to set up this part of the data (it will actually call `MaterialPropertyDrawer.Apply()`).

### Problems Occurred After Creating the Material in the Code

When creating materials in code, some Drawer logic may ***not*** run, and default values might not meet expectations.
You need to manually call `LWGUI.PresetHelper.ApplyPresetsInMaterial()` to ensure default values are correct.

## Custom Shader GUI

### Custom Header and Footer

![image-20230821211652918](./assets~/image-20230821211652918.png)

1. Custom Headers and Footers enable you to integrate bespoke modules at the top or bottom of the ShaderGUI without altering LWGUI plugin code.
2. Depending on the desired location for the custom GUI, duplicate the following script into an Editor folder within your project:

   - Top: Packages/com.jasonma.lwgui/Editor/CustomGUISample/CustomHeader.cs
   - Bottom: Packages/com.jasonma.lwgui/Editor/CustomGUISample/CustomFooter.cs
3. Modify the file name and class name accordingly.
4. Implement your custom GUI code within the DoCustomHeader() / DoCustomFooter() functions.
5. It is advisable to examine the lwgui object definition to obtain any requisite data.

### Custom Drawer

You can build new Drawer or Decorator behaviors by inheriting `SubDrawer` (Decorators in this project also follow the `SubDrawer` pipeline). The following best practices make customization easier to maintain:

1. **Define clear responsibilities first**
   - Drawers should focus on value input and visual interaction.
   - Decorators should focus on structure, display control, or visual enhancement.
   - Avoid mixing too many unrelated responsibilities into one drawer.
2. **Start from a minimal working loop**
   - Implement a minimal useful version first, then iterate.
   - Prefer referencing similar implementations in `Editor/ShaderDrawers/ExtraDrawers/` and `Editor/ShaderDrawers/ExtraDecorators/`.
3. **Keep logic idempotent and side effects low**
   - `OnGUI` can be called frequently, so avoid repeated allocations and unnecessary writes.
   - Only write back to materials or trigger linkage when values truly change.
4. **Use the correct cache scope**
   - Put inspector-temporary state in `PerInspectorData`.
   - Put material-related state in `PerMaterialData`.
   - Put cross-material shader parse results in `PerShaderData`.
5. **Reuse existing Helpers whenever possible**
   - For context menu, Ramp, Preset, or Toolbar requirements, integrate with existing utilities in `Editor/Helper/` instead of reimplementing.
6. **Validate real production scenarios**
   - At minimum, verify behavior under multi-material editing, Undo/Redo, asset reimport, and script recompilation.
   - Before submission, run regression checks with assets in `Test/`.

## Contribution

1. Create multiple empty projects using different versions of Unity
2. Pull this repo
3. Use symbolic links to place this repo in the Assets or Packages directory of all projects
4. Inherit the `Subdrawer` in ` shadeerdrawer.cs` to start developing your custom Drawer
5. Check whether the functionality works in different Unity versions
6. Pull requests

## Development Guide

### Data Structure (Shader &gt; Material &gt; Inspector)

The core purpose of `Editor/MetaData/` is to "avoid state interference and reduce redundant calculations" by isolating caches through different scopes.

```
LWGUIMetaDatas
├─ PerShaderData (Shader scope, static and immutable)
│  ├─ propStaticDatas: Dictionary&lt;property name, PropertyStaticData&gt;
│  │  ├─ Static property information (group name, Drawer, display mode, parent-child relationship)
│  │  └─ Only rebuilt when Shader is recompiled
│  ├─ displayModeData: Display mode settings (advanced properties, hidden properties, modified properties only)
│  ├─ searchMode/searchString: Search state
│  └─ defaultMaterial: Default material (for comparing modifications)
│
├─ PerMaterialData (Material scope, material-related)
│  ├─ propDynamicDatas: Dictionary&lt;property name, PropertyDynamicData&gt;
│  │  ├─ Current property values, default values, modification flags, ShowIf/ActiveIf results
│  │  └─ forceInit flag controls whether to rebuild
│  ├─ activePresetDatas: List of active Presets
│  ├─ modifiedCount: Number of modified properties
│  ├─ cachedModifiedProperties: Cache when showing only modified properties
│  └─ shaderPerfDatas: Performance statistics data
│
└─ PerInspectorData (Inspector scope, window-specific)
   └─ materialEditor: Current MaterialEditor reference
```

### Lifecycle
- **PerShaderData**: Created when Shader is first accessed, released when Shader is reimported (`ReleaseShaderMetadataCache`)
- **PerMaterialData**: Created when material is first accessed, released when material is modified/closed/shader is changed (`ReleaseMaterialMetadataCache`)
- **PerInspectorData**: Created independently for each LWGUI instance (Inspector window)
- **Forced Update**: Trigger complete PerMaterialData rebuild after modifying material via code
  - Call `PresetHelper.ApplyPresetsInMaterial` after modifying Preset values