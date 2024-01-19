# LWGUI (Light Weight Shader GUI)

[中文](https://github.com/JasonMa0012/LWGUI/blob/dev/README_CN.md) | [English](https://github.com/JasonMa0012/LWGUI)

[![](https://dcbadge.vercel.app/api/server/WwBYGXqPEh)](https://discord.gg/WwBYGXqPEh)

一个轻量, 灵活, 强大的**Unity Shader GUI**系统.

已经过诸多大型商业项目的验证, 使用简洁的Material Property Drawer语法实现功能强大的Shader GUI, 节省大量开发时间, 易于使用和扩展, 有效提升美术人员的使用体验.

![809c4a1c-ce80-48b1-b415-7e8d4bea716e](README_CN.assets/809c4a1c-ce80-48b1-b415-7e8d4bea716e-16616214059841.png)

![LWGUI](README_CN.assets/LWGUI.png)



| ![image-20220926025611208](./README_CN.assets/image-20220926025611208.png) | ![image-20230821205439889](./README_CN.assets/image-20230821205439889.png) |
| ------------------------------------------------------------ | ------------------------------------------------------------ |
| 搜索栏亦可筛选已修改的属性                                   | 右键以按类型粘贴属性值                                       |



| With your sponsorship, I will update more actively. | 有你的赞助我会更加积极地更新                                 |
| --------------------------------------------------- | ------------------------------------------------------------ |
| [paypal.me/JasonMa0012](paypal.me/JasonMa0012)      | ![723ddce6-fb86-48ff-9683-a12cf6cff7a0](./README_CN.assets/723ddce6-fb86-48ff-9683-a12cf6cff7a0.jpg) |


- [Installation](#installation)
- [Usage](#usage)
  * [Getting Started](#getting-started)
  * [LWGUI Drawers](#lwgui-drawers)
    + [Main & Sub](#main--sub)
    + [SubToggle](#subtoggle)
    + [SubPowerSlider](#subpowerslider)
    + [SubIntRange](#subintrange)
    + [MinMaxSlider](#minmaxslider)
    + [KWEnum](#kwenum)
    + [SubEnum & SubKeywordEnum](#subenum--subkeywordenum)
    + [Tex & Color](#tex--color)
    + [Channel](#channel)
    + [Ramp](#ramp)
    + [Preset](#preset)
      - [Create Preset File](#create-preset-file)
      - [Edit Preset](#edit-preset)
  * [LWGUI Decorator](#lwgui-decorator)
    + [Title & SubTitle](#title--subtitle)
    + [Tooltip & Helpbox](#tooltip--helpbox)
    + [PassSwitch](#passswitch)
    + [Advanced & AdvancedHeaderProperty](#advanced--advancedheaderproperty)
    + [Hidden](#hidden)
    + [ShowIf](#showif)
  * [Unity Builtin Drawers](#unity-builtin-drawers)
    + [Space](#space)
    + [Header](#header)
    + [Enum](#enum)
    + [IntRange](#intrange)
    + [KeywordEnum](#keywordenum)
    + [PowerSlider](#powerslider)
    + [Toggle](#toggle)
  * [Tips](#tips)
- [Custom Shader GUI](#custom-shader-gui)
  * [Custom Header and Footer](#custom-header-and-footer)
  * [Custom Drawer](#custom-drawer)
- [TODO](#todo)
- [Contribution](#contribution)


## Installation

1. 确保你的Unity版本兼容LWGUI: **Unity 2017.4+**
2. 打开已有工程
3. （可能需要全局代理）`Window > Package Manager > Add > Add package from git URL` 输入`https://github.com/JasonMa0012/LWGUI.git`

   - 你也可以选择手动从Github下载Zip，然后从`Package Manager > Add package from disk`添加Local Package
   - **对于Unity 2017, 请直接将Zip解压到Assets目录**

## Usage

### Getting Started

1. 新建一个Shader或使用现有的Shader
2. 在代码编辑器中打开Shader
3. 在Shader最底部, 最后一个大括号之前, 添加行:`CustomEditor "LWGUI.LWGUI"`
4. 完成! 开始使用以下功能强大的Drawer轻松绘制你的ShaderGUI吧
   - MaterialPropertyDrawer是一种类似C# Attribute的语法, 在MaterialProperty前加上Drawer可以更改绘制方式, 更多信息可以查看官方文档:`https://docs.unity3d.com/ScriptReference/MaterialPropertyDrawer.html`
   - 每个Property只能有一个Drawer
   - 每个Property可以有多个Decorator


### LWGUI Drawers

#### Main & Sub

```c#
/// Create a Folding Group
/// group：group name (Default: Property Name)
/// keyword：keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
/// default Folding State: "on" or "off" (Default: off)
/// default Toggle Displayed: "on" or "off" (Default: on)
/// Target Property Type: FLoat, express Toggle value
public MainDrawer() : this(String.Empty) { }
public MainDrawer(string group) : this(group, String.Empty) { }
public MainDrawer(string group, string keyword) : this(group, keyword, "off") { }
public MainDrawer(string group, string keyword, string defaultFoldingState) : this(group, keyword, defaultFoldingState, "on") { }
public MainDrawer(string group, string keyword, string defaultFoldingState, string defaultToggleDisplayed)

```

```c#
/// Draw a property with default style in the folding group
/// group：father group name, support suffix keyword for conditional display (Default: none)
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

![image-20220828003026556](README_CN.assets/image-20220828003026556.png)

Then change values:

![image-20220828003129588](README_CN.assets/image-20220828003129588.png)

#### SubToggle

```c#
/// Similar to builtin Toggle()
/// group：father group name, support suffix keyword for conditional display (Default: none)
/// keyword：keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
/// Target Property Type: FLoat
public SubToggleDrawer() { }
public SubToggleDrawer(string group) : this(group, String.Empty) { }
public SubToggleDrawer(string group, string keyWord)

```



#### SubPowerSlider

```c#
/// Similar to builtin PowerSlider()
/// group：father group name, support suffix keyword for conditional display (Default: none)
/// power: power of slider (Default: 1)
/// Target Property Type: Range
public SubPowerSliderDrawer(float power) : this("_", power) { }
public SubPowerSliderDrawer(string group, float power)
```


#### SubIntRange

```c#
/// Similar to builtin IntRange()
/// group：father group name, support suffix keyword for conditional display (Default: none)
/// Target Property Type: Range
public SubIntRangeDrawer(string group)

```



#### MinMaxSlider

```c#
/// Draw a min max slider 
/// group：father group name, support suffix keyword for conditional display (Default: none)
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

![image-20220828003810353](README_CN.assets/image-20220828003810353.png)



#### KWEnum

```c#
/// <summary>
/// Similar to builtin Enum() / KeywordEnum()
/// group：father group name, support suffix keyword for conditional display (Default: none)
/// n(s): display name
/// k(s): keyword
/// v(s): value
/// Target Property Type: FLoat, express current keyword index
/// </summary>
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



#### Tex & Color

```c#
/// Draw a Texture property in single line with a extra property
/// group：father group name, support suffix keyword for conditional display (Default: none)
/// extraPropName: extra property name  (Default: none)
/// Target Property Type: Texture
/// Extra Property Type: Color, Vector
public TexDrawer() { }
public TexDrawer(string group) : this(group, String.Empty) { }
public TexDrawer(string group, string extraPropName)

```

```c#
/// Display up to 4 colors in a single line
/// group：father group name, support suffix keyword for conditional display (Default: none)
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

// Display up to 4 colors in a single line (Unity 2019.2+)
[Color(Group3, _mColor1, _mColor2, _mColor3)]
_mColor ("Multi Color", Color) = (1, 1, 1, 1)
[HideInInspector] _mColor1 (" ", Color) = (1, 0, 0, 1)
[HideInInspector] _mColor2 (" ", Color) = (0, 1, 0, 1)
[HideInInspector] [HDR] _mColor3 (" ", Color) = (0, 0, 1, 1)

```

Result:

![image-20220828003507825](README_CN.assets/image-20220828003507825.png)

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
/// group：father group name, support suffix keyword for conditional display (Default: none)
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



![image-20220822010511978](README_CN.assets/image-20220822010511978.png)

#### Ramp

```c#
/// Draw a Ramp Map Editor (Defaulf Ramp Map Resolution: 512 * 2)
/// group：father group name, support suffix keyword for conditional display (Default: none)
/// defaultFileName: default Ramp Map file name when create a new one (Default: RampMap)
/// rootPath: the path where ramp is stored, replace '/' with '.' (for example: Assets.Art.Ramps). when selecting ramp, it will also be filtered according to the path (Default: Assets)
/// defaultWidth: default Ramp Width (Default: 512)
/// Target Property Type: Texture2D
public RampDrawer() : this(String.Empty) { }
public RampDrawer(string group) : this(group, "RampMap") { }
public RampDrawer(string group, string defaultFileName) : this(group, defaultFileName, DefaultRootPath, 512) { }
public RampDrawer(string group, string defaultFileName, float defaultWidth) : this(group, defaultFileName, DefaultRootPath, defaultWidth) { }
public RampDrawer(string group, string defaultFileName, string rootPath, float defaultWidth)

```

Example:

```c#
[Ramp(_, RampMap, Assets.Art, 512)] _Ramp ("Ramp Map", 2D) = "white" { }

```

Result:

![image-20230625185730363](./README_CN.assets/image-20230625185730363.png)

Ramp编辑器:

![image-20220821234658509](README_CN.assets/image-20220821234658509.png)

你**必须手动保存编辑结果**, 如果有未保存的修改, Save按钮将显示黄色.

**在你移动或者复制RampMap的时候, 切记要连同.meta文件一起移动, 否则将无法再次编辑!**



#### Preset

```c#
/// Popping a menu, you can select the Shader Property Preset, the Preset values will replaces the default values
/// group：father group name, support suffix keyword for conditional display (Default: none)
///	presetFileName: "Shader Property Preset" asset name, you can create new Preset by
///		"Right Click > Create > LWGUI > Shader Property Preset" in Project window,
///		*any Preset in the entire project cannot have the same name*
public PresetDrawer(string presetFileName) : this("_", presetFileName) {}
public PresetDrawer(string group, string presetFileName)

```

Example:

~~~c#
[Title(Preset Samples)]
[Preset(LWGUI_BlendModePreset)] _BlendMode ("Blend Mode Preset", float) = 0 
[Enum(UnityEngine.Rendering.CullMode)]_Cull("Cull", Float) = 2
[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("SrcBlend", Float) = 1
[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("DstBlend", Float) = 0
[Toggle(_)]_ZWrite("ZWrite ", Float) = 1
[Enum(UnityEngine.Rendering.CompareFunction)]_ZTest("ZTest", Float) = 4 // 4 is LEqual
[Enum(RGBA,15,RGB,14)]_ColorMask("ColorMask", Float) = 15 // 15 is RGBA (binary 1111)
    
``````
    
Cull [_Cull]
ZWrite [_ZWrite]
Blend [_SrcBlend] [_DstBlend]
ColorMask [_ColorMask]
~~~

Result:

选择的预设内的属性值将成为默认值

**RenderQueue**是个特殊属性, 需要手动在预设中添加

![image-20221122231655378](README_CN.assets/image-20221122231655378.png)![image-20221122231816714](README_CN.assets/image-20221122231816714.png)

##### Create Preset File

![image-20221122232307362](README_CN.assets/image-20221122232307362.png)

##### Edit Preset

![image-20221122232354623](README_CN.assets/image-20221122232354623.png)![image-20221122232415972](README_CN.assets/image-20221122232415972.png)![image-20221122232425194](README_CN.assets/image-20221122232425194.png)



### LWGUI Decorator

#### Title & SubTitle

```c#
/// Similar to Header()
/// group：father group name, support suffix keyword for conditional display (Default: none)
/// header: string to display, "SpaceLine" or "_" = none (Default: none)
/// height: line height (Default: 22)
public TitleDecorator(string header) : this("_", header, DefaultHeight) {}
public TitleDecorator(string header, float  height) : this("_", header, height) {}
public TitleDecorator(string group,  string header) : this(group, header, DefaultHeight) {}
public TitleDecorator(string group, string header, float height)


/// Similar to Title()
/// group：father group name, support suffix keyword for conditional display (Default: none)
/// header: string to display, "SpaceLine" or "_" = none (Default: none)
/// height: line height (Default: 22)
public SubTitleDecorator(string group,  string header) : base(group, header, DefaultHeight) {}
public SubTitleDecorator(string group, string header, float height) : base(group, header, height) {}

```

#### Tooltip & Helpbox

```c#
/// Tooltip, describes the details of the property. (Default: property.name and property default value)
/// You can also use "#Text" in DisplayName to add Tooltip that supports Multi-Language.
/// tooltip：a single-line string to display, support up to 4 ','. (Default: Newline)
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
/// message：a single-line string to display, support up to 4 ','. (Default: Newline)
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

![image-20221231221240686](README_CN.assets/image-20221231221240686.png)

![image-20221231221254101](README_CN.assets/image-20221231221254101.png)

Tips:

- Tooltip可能在Editor运行时消失, 这是Unity本身的特性 (或者是bug)



#### PassSwitch

```c#
/// Cooperate with Toggle to switch certain Passes
/// lightModeName(s): Light Mode in Shader Pass (https://docs.unity3d.com/2017.4/Documentation/Manual/SL-PassTags.html)
public PassSwitchDecorator(string   lightModeName1) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2, string lightModeName3) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4, string lightModeName5) 
public PassSwitchDecorator(string   lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4, string lightModeName5, string lightModeName6) 

```



#### Advanced & AdvancedHeaderProperty

```c#
/// 将当前Property折叠到一个Advanced Block中, 指定Header String可以创建新的Advanced Block, 所有使用了Advanced()的Property会被折叠到最近的Advanced Block中.
/// headerString: Advanced Block的标题. 默认: "Advanced"
public AdvancedDecorator() : this(string.Empty) { }
public AdvancedDecorator(string headerString)
```

```c#
/// 以当前Property作为Header创建一个Advanced Block
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

![image-20231007163044176](./README_CN.assets/image-20231007163044176.png)

Tips:

- LWGUI使用树状数据结构存储Group和Advanced Block及其子级的关系, 理论上可以存储无限多级父子关系, 但**目前LWGUI仅手动处理3层父子关系, 也就是说你可以将Advanced Block放在Group内, 而不能将Group放在Advanced Block内.**

#### Hidden

```c#
/// 类似于HideInInspector(), 区别在于Hidden()可以通过Display Mode按钮取消隐藏.
public HiddenDecorator()
```



#### ReadOnly

```c#
/// 将属性设为只读.
public ReadOnlyDecorator()
```



#### ShowIf

```c#
/// 可以根据多个条件控制单个或者一组属性的显示 / 隐藏.
/// logicalOperator: And | Or (Default: And).
/// propName: Target Property Name used for comparison.
/// compareFunction: Less (L) | Equal (E) | LessEqual (LEqual / LE) | Greater (G) | NotEqual (NEqual / NE) | GreaterEqual (GEqual / GE).
/// value: Target Property Value used for comparison.
public ShowIfDecorator(string propName, string comparisonMethod, float value) : this("And", propName, comparisonMethod, value) { }
public ShowIfDecorator(string logicalOperator, string propName, string compareFunction, float value)
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

![image-20231023010137495](./README_CN.assets/image-20231023010137495.png)

![image-20231023010153213](./README_CN.assets/image-20231023010153213.png)

![image-20231023010204399](./README_CN.assets/image-20231023010204399.png)



### Unity Builtin Drawers

#### Space

```c#
MaterialSpaceDecorator(float height)
```

#### Header

```c#
MaterialHeaderDecorator(string header)
```

#### Enum

```c#
MaterialEnumDrawer(string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7)
```



#### IntRange

```c#
MaterialIntRangeDrawer()
```



#### KeywordEnum

```c#
MaterialKeywordEnumDrawer(string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9)
```



#### PowerSlider

```c#
MaterialPowerSliderDrawer(float power)
```



#### Toggle

```c#
MaterialToggleUIDrawer(string keyword)
```



## Custom Shader GUI

### Custom Header and Footer

![image-20230821211652918](./README_CN.assets/image-20230821211652918.png)

Custom Header和Footer可以让你无需修改LWGUI插件的代码即可在ShaderGUI的顶部或底部添加自定义的模块.

1. 根据你要添加自定义GUI的位置复制以下脚本到你的项目某个Editor文件夹中:
   - 顶部: Packages/com.jasonma.lwgui/Editor/CustomGUISample/CustomHeader.cs
   - 底部: Packages/com.jasonma.lwgui/Editor/CustomGUISample/CustomFooter.cs
2. 修改文件名和类名
3. 在DoCustomHeader() / DoCustomFooter()中添加你自定义的GUI代码
4. 建议查看lwgui对象的定义以获取你所需要的数据

### Custom Drawer

TODO

## TODO

- [ ] 支持ShaderGraph or ASE
- [x] **per material保存折叠组打开状态**
- [x] 支持Unreal风格的Revertable GUI
- [x] **支持HelpBox**
- [ ] 支持改文字格式
- [x] **支持Tooltip, 显示默认值和自定义内容**
- [x] **支持右上角菜单全部展开或折叠**
- [x] 支持Pass开关
- [ ] 支持Curve
- [x] **支持搜索框**
- [x] **支持仅显示已修改项**
- [x] 支持预设管理器
- [x] 支持自适应枚举宽度
- [x] 支持2017
  - [x] 反射引擎私有函数
  - [x] 复制属性菜单




## Contribution

1. 使用不同Unity版本创建多个空工程
2. 拉取repo
3. 使用符号链接将此repo放到所有工程的Assets或Packages目录内
4. 在`ShaderDrawer.cs`内继承`SubDrawer`开始开发你的自定义Drawer
5. 检查功能在不同Unity版本是否正常
6. Pull requests









