// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LWGUI.LwguiGradientEditor;
using LWGUI.Runtime;
using LWGUI.Runtime.LwguiGradient;
using LWGUI.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LWGUI
{
	#region Interfaces
	public interface IBaseDrawer
	{
		public void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData) {}

		public void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData) {}
		
		public void GetCustomContextMenus(GenericMenu menu, Rect rect, MaterialProperty prop, LWGUIMetaDatas metaDatas) {}
	}

	public interface IPresetDrawer
	{
		public string GetPresetFileName();
		
		public LwguiShaderPropertyPreset.Preset GetActivePreset(MaterialProperty inProp, LwguiShaderPropertyPreset lwguiShaderPropertyPreset);
	}
	#endregion

	#region Metadata
	public partial class PropertyStaticData
	{
		// Image
		public Texture2D image;
		
		// Button
		public List<string> buttonDisplayNames = new();
		public List<string> buttonCommands = new();
		public List<float> buttonDisplayNameWidths = new();

		// You can add more data that is determined during the initialization of the Drawer as a cache here,
		// thereby avoiding the need to calculate it every frame in OnGUI().
		// >>>>>>>>>>>>>>>>>>>>>>>> Add new data here <<<<<<<<<<<<<<<<<<<<<<<
	}
	#endregion

	#region Basic Drawers
	/// <summary>
	/// Create a Folding Group
	/// 
	/// group: group name (Default: Property Name)
	/// keyword: keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
	/// default Folding State: "on" or "off" (Default: off)
	/// default Toggle Displayed: "on" or "off" (Default: on)
	/// preset File Name: "Shader Property Preset" asset name, see Preset() for detail (Default: none)
	/// Target Property Type: Float, express Toggle value
	/// </summary>
	public class MainDrawer : MaterialPropertyDrawer, IBaseDrawer, IPresetDrawer
	{
		protected LWGUIMetaDatas metaDatas;

		private static readonly float  _height = 28f;
		
		private                 bool   _isFolding;
		private                 string _group;
		private                 string _keyword;
		private                 bool   _defaultFoldingState;
		private                 bool   _defaultToggleDisplayed;
		private                 string _presetFileName;

		public MainDrawer() : this(String.Empty) { }

		public MainDrawer(string group) : this(group, String.Empty) { }

		public MainDrawer(string group, string keyword) : this(group, keyword, "off") { }

		public MainDrawer(string group, string keyword, string defaultFoldingState) : this(group, keyword, defaultFoldingState, "on") { }
		
		public MainDrawer(string group, string keyword, string defaultFoldingState, string defaultToggleDisplayed) : this(group, keyword, defaultFoldingState, defaultToggleDisplayed, String.Empty) { }

		public MainDrawer(string group, string keyword, string defaultFoldingState, string defaultToggleDisplayed, string presetFileName)
		{
			this._group = group;
			this._keyword = keyword;
			this._defaultFoldingState = Helper.StringToBool(defaultFoldingState);
			this._defaultToggleDisplayed = Helper.StringToBool(defaultToggleDisplayed);
			this._presetFileName = presetFileName;
		}

		public virtual void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.groupName = _group;
			inoutPropertyStaticData.isMain = true;
			inoutPropertyStaticData.isExpanding = _defaultFoldingState;
			PerShaderData.DecodeMetaDataFromDisplayName(inProp, inoutPropertyStaticData);
			PresetDrawer.SetPresetAssetToStaticData(inoutPropertyStaticData, _presetFileName);
		}

		public virtual void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription = inDefaultProp.floatValue > 0 ? "On" : "Off";
		}

		public string GetPresetFileName() => _presetFileName;

		public LwguiShaderPropertyPreset.Preset GetActivePreset(MaterialProperty inProp, LwguiShaderPropertyPreset lwguiShaderPropertyPreset) =>
			PresetDrawer.GetActivePresetFromFloatProperty(inProp, lwguiShaderPropertyPreset);

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			metaDatas = Helper.GetLWGUIMetadatas(editor);

			var showMixedValue = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = prop.hasMixedValue;
			EditorGUI.BeginChangeCheck();

			bool toggleResult = Helper.DrawFoldout(position, ref metaDatas.GetPropStaticData(prop).isExpanding, !Helper.Approximately(prop.floatValue, 0), _defaultToggleDisplayed, label);

			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				prop.floatValue = toggleResult ? 1.0f : 0.0f;
				var keyword = Helper.GetKeywordName(_keyword, prop.name);
				Helper.SetShaderKeywordEnabled(editor.targets, keyword, toggleResult);
				PresetHelper.GetPresetAsset(_presetFileName)?.GetPreset(prop.floatValue)?.ApplyToEditingMaterial(editor, metaDatas.perMaterialData);
				TimelineHelper.SetKeywordToggleToTimeline(prop, editor, keyword);
			}
			EditorGUI.showMixedValue = showMixedValue;
		}

		// Call in custom shader gui
		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return _height;
		}

		// Call when create/edit/undo materials.
		// Used to set material settings such as Keywords that need to be kept synchronized with the value forever.
		// DO NOT modify other properties here!!! Otherwise, manually modified values will be overwritten.
		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
			{
				Helper.SetShaderKeywordEnabled(prop.targets, Helper.GetKeywordName(_keyword, prop.name), prop.floatValue > 0f);
				PresetDrawer.ApplyPresetWithoutPropertyChanges(_presetFileName, prop);
			}
		}
	}

	/// <summary>
	/// Draw a property with default style in the folding group
	/// 
	/// group: parent group name (Default: none)
	/// Target Property Type: Any
	/// </summary>
	public class SubDrawer : MaterialPropertyDrawer, IBaseDrawer
	{
		public string         group = String.Empty;
		public LWGUIMetaDatas metaDatas;

		public SubDrawer() { }

		public SubDrawer(string group)
		{
			this.group = group;
		}

		protected virtual bool IsMatchPropType(MaterialProperty property) { return true; }

		protected virtual float GetVisibleHeight(MaterialProperty prop)
		{
			var height = MaterialEditor.GetDefaultPropertyHeight(prop);
			return prop.GetPropertyType() == ShaderPropertyType.Vector ? EditorGUIUtility.singleLineHeight : height;
		}

		public virtual void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.groupName = group;
			PerShaderData.DecodeMetaDataFromDisplayName(inProp, inoutPropertyStaticData);
		}

		public virtual void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData) { }

		public virtual void GetCustomContextMenus(GenericMenu menu, Rect rect, MaterialProperty prop, LWGUIMetaDatas metaDatas)	{ }

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			metaDatas = Helper.GetLWGUIMetadatas(editor);

			if (IsMatchPropType(prop))
			{
				DrawProp(position, prop, label, editor);
			}
			else
			{
				Debug.LogWarning("LWGUI: Property:'" + prop.name + "' Type:'" + prop.GetPropertyType() + "' mismatch!");
				editor.DefaultShaderProperty(position, prop, label.text);
			}
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return GetVisibleHeight(prop);
		}

		// Draws a custom style property
		public virtual void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			RevertableHelper.FixGUIWidthMismatch(prop.GetPropertyType(), editor);
			editor.DefaultShaderPropertyInternal(position, prop, label);
		}
	}
	#endregion

	#region Extra Drawers

	#region Numeric
	/// <summary>
	/// Similar to builtin Toggle()
	/// 
	/// group: parent group name (Default: none)
	/// keyword: keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
	/// preset File Name: "Shader Property Preset" asset name, see Preset() for detail (Default: none)
	/// Target Property Type: Float
	/// </summary>
	public class SubToggleDrawer : SubDrawer, IPresetDrawer
	{
		private string _keyWord			= String.Empty;
		private string _presetFileName	= String.Empty;

		public SubToggleDrawer() { }

		public SubToggleDrawer(string group) : this(group, String.Empty, String.Empty) { }
		
		public SubToggleDrawer(string group, string keyWord) : this(group, keyWord, String.Empty) { }

		public SubToggleDrawer(string group, string keyWord, string presetFileName)
		{
			this.group = group;
			this._keyWord = keyWord;
			this._presetFileName = presetFileName;
		}

		protected override bool IsMatchPropType(MaterialProperty property)
		{
			return property.GetPropertyType() is ShaderPropertyType.Float;
		}

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			PresetDrawer.SetPresetAssetToStaticData(inoutPropertyStaticData, _presetFileName);
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription = inDefaultProp.floatValue > 0 ? "On" : "Off";
		}
		
		public string GetPresetFileName() => _presetFileName;
		
		public LwguiShaderPropertyPreset.Preset GetActivePreset(MaterialProperty inProp, LwguiShaderPropertyPreset lwguiShaderPropertyPreset) =>
			PresetDrawer.GetActivePresetFromFloatProperty(inProp, lwguiShaderPropertyPreset);

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var value = EditorGUI.Toggle(position, label, !Helper.Approximately(prop.floatValue, 0));
			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				prop.floatValue = value ? 1.0f : 0.0f;
				var keyword = Helper.GetKeywordName(_keyWord, prop.name);
				Helper.SetShaderKeywordEnabled(editor.targets, keyword, value);
				PresetHelper.GetPresetAsset(_presetFileName)?.GetPreset(prop.floatValue)?.ApplyToEditingMaterial(editor, metaDatas.perMaterialData);
				TimelineHelper.SetKeywordToggleToTimeline(prop, editor, keyword);
			}
			EditorGUI.showMixedValue = false;
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
			{
				Helper.SetShaderKeywordEnabled(prop.targets, Helper.GetKeywordName(_keyWord, prop.name), prop.floatValue > 0f);
				PresetDrawer.ApplyPresetWithoutPropertyChanges(_presetFileName, prop);
			}
		}
	}

	/// <summary>
	/// Similar to builtin PowerSlider()
	/// 
	/// group: parent group name (Default: none)
	/// power: power of slider (Default: 1)
	/// Target Property Type: Range
	/// </summary>
	public class SubPowerSliderDrawer : SubDrawer
	{
		private float _power = 1;

		public SubPowerSliderDrawer(float power) : this("_", power) { }

		public SubPowerSliderDrawer(string group, float power)
		{
			this.group = group;
			this._power = Mathf.Clamp(power, 0, float.MaxValue);
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Range; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			RevertableHelper.FixGUIWidthMismatch(prop.GetPropertyType(), editor);
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position;
			ReflectionHelper.DoPowerRangeProperty(rect, prop, label, _power);
			EditorGUI.showMixedValue = false;
		}
	}

	/// <summary>
	/// Similar to builtin IntRange()
	/// 
	/// group: parent group name (Default: none)
	/// Target Property Type: Range
	/// </summary>
	public class SubIntRangeDrawer : SubDrawer
	{
		public SubIntRangeDrawer(string group)
		{
			this.group = group;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Range; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			RevertableHelper.FixGUIWidthMismatch(prop.GetPropertyType(), editor);

			if (prop.GetPropertyType() != ShaderPropertyType.Range)
			{
				EditorGUI.LabelField(position, "IntRange used on a non-range property: " + prop.name, EditorStyles.helpBox);
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = prop.hasMixedValue;
				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 0.0f;
				int num = EditorGUI.IntSlider(position, label, (int)prop.floatValue, (int)prop.rangeLimits.x, (int)prop.rangeLimits.y);
				EditorGUI.showMixedValue = false;
				EditorGUIUtility.labelWidth = labelWidth;
				if (Helper.EndChangeCheck(metaDatas, prop))
				{
					prop.floatValue = num;
				}
			}
		}
	}

	/// <summary>
	/// Draw a min max slider
	/// 
	/// group: parent group name (Default: none)
	/// minPropName: Output Min Property Name
	/// maxPropName: Output Max Property Name
	/// Target Property Type: Range, range limits express the MinMaxSlider value range
	/// Output Min/Max Property Type: Range, it's value is limited by it's range
	/// </summary>
	public class MinMaxSliderDrawer : SubDrawer
	{
		private string _minPropName;
		private string _maxPropName;

		public MinMaxSliderDrawer(string minPropName, string maxPropName) : this("_", minPropName, maxPropName) { }

		public MinMaxSliderDrawer(string group, string minPropName, string maxPropName)
		{
			this.group = group;
			this._minPropName = minPropName;
			this._maxPropName = maxPropName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Range; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			inoutPropertyStaticData.AddExtraProperty(_minPropName);
			inoutPropertyStaticData.AddExtraProperty(_maxPropName);
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			if (string.IsNullOrEmpty(_minPropName)
			 || string.IsNullOrEmpty(_maxPropName)
			 || !inoutPerMaterialData.propDynamicDatas.ContainsKey(_minPropName)
			 || !inoutPerMaterialData.propDynamicDatas.ContainsKey(_maxPropName)
			   )
			{
				Debug.LogError("LWGUI: " + inProp.name + " has no available min/max properties!");
				return;
			}

			inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription =
				inoutPerMaterialData.propDynamicDatas[_minPropName].defualtProperty.floatValue
			  + " - "
			  + inoutPerMaterialData.propDynamicDatas[_maxPropName].defualtProperty.floatValue;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// read min max
			MaterialProperty minProp = metaDatas.GetProperty(_minPropName);
			MaterialProperty maxProp = metaDatas.GetProperty(_maxPropName);
			if (minProp == null || maxProp == null)
			{
				Debug.LogError("LWGUI: MinMaxSliderDrawer: minProp: " + (minProp == null ? "null" : minProp.name) + " or maxProp: " + (maxProp == null ? "null" : maxProp.name) + " not found!");
				return;
			}
			float minf = minProp.floatValue;
			float maxf = maxProp.floatValue;

			// define draw area
			Rect controlRect = position; // this is the full length rect area
			var w = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 0;
			Rect inputRect = MaterialEditor.GetRectAfterLabelWidth(controlRect); // this is the remaining rect area after label's area

			// draw label
			EditorGUI.PrefixLabel(controlRect, label);

			// draw min max slider
			var indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Rect[] splittedRect = Helper.SplitRect(inputRect, 3);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = minProp.hasMixedValue;
			var newMinf = EditorGUI.FloatField(splittedRect[0], minf);
			if (Helper.EndChangeCheck(metaDatas, minProp))
			{
				minf = Mathf.Clamp(newMinf, minProp.rangeLimits.x, minProp.rangeLimits.y);
				minProp.floatValue = minf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = maxProp.hasMixedValue;
			var newMaxf = EditorGUI.FloatField(splittedRect[2], maxf);
			if (Helper.EndChangeCheck(metaDatas, maxProp))
			{
				maxf = Mathf.Clamp(newMaxf, maxProp.rangeLimits.x, maxProp.rangeLimits.y);
				maxProp.floatValue = maxf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			if (splittedRect[1].width > 50f)
				EditorGUI.MinMaxSlider(splittedRect[1], ref minf, ref maxf, prop.rangeLimits.x, prop.rangeLimits.y);
			EditorGUI.showMixedValue = false;

			// write back min max if changed
			if (EditorGUI.EndChangeCheck())
			{
				minProp.floatValue = Mathf.Clamp(minf, minProp.rangeLimits.x, minProp.rangeLimits.y);
				maxProp.floatValue = Mathf.Clamp(maxf, maxProp.rangeLimits.x, maxProp.rangeLimits.y);
			}

			EditorGUI.indentLevel = indentLevel;
			EditorGUIUtility.labelWidth = w;
		}
	}

	/// <summary>
	/// Similar to builtin Enum() / KeywordEnum()
	/// 
	/// group: parent group name (Default: none)
	/// n(s): display name
	/// k(s): keyword
	/// v(s): value
	/// Target Property Type: Float, express current keyword index
	/// </summary>
	public class KWEnumDrawer : SubDrawer
	{
		private GUIContent[] _names;
		private string[]     _keyWords;
		private float[]      _values;


		#region

		public KWEnumDrawer(string n1, string k1)
			: this("_", new string[1] { n1 }, new string[1] { k1 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2)
			: this("_", new string[2] { n1, n2 }, new string[2] { k1, k2 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3)
			: this("_", new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
			: this("_", new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
			: this("_", new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }) { }

		public KWEnumDrawer(string group, string n1, string k1)
			: this(group, new string[1] { n1 }, new string[1] { k1 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2)
			: this(group, new string[2] { n1, n2 }, new string[2] { k1, k2 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3)
			: this(group, new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
			: this(group, new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
			: this(group, new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }) { }

		#endregion


		public KWEnumDrawer(string group, string[] names, string[] keyWords = null, float[] values = null)
		{
			Init(group, names, keyWords, values);
		}

		protected void Init(string group, string[] names, string[] keyWords, float[] values)
		{
			this.group = group;

			this._names = new GUIContent[names.Length];
			for (int index = 0; index < names.Length; ++index)
				this._names[index] = new GUIContent(names[index]);

			if (keyWords == null)
			{
				keyWords = new string[names.Length];
				for (int i = 0; i < names.Length; i++)
					keyWords[i] = String.Empty;
			}
			this._keyWords = keyWords;

			if (values == null)
			{
				values = new float[names.Length];
				for (int index = 0; index < names.Length; ++index)
					values[index] = index;
			}
			this._values = values;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() is ShaderPropertyType.Float; }

		protected virtual string GetKeywordName(string propName, string name) { return (name).Replace(' ', '_').ToUpperInvariant(); }

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			var index = Array.IndexOf(_values, (int)inDefaultProp.floatValue);
			if (index < _names.Length && index >= 0)
				inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription = _names[index].text;
		}

		private string[] GetKeywords(MaterialProperty property)
		{
			string[] keyWords = new string[_keyWords.Length];
			for (int i = 0; i < keyWords.Length; i++)
				keyWords[i] = GetKeywordName(property.name, _keyWords[i]);
			return keyWords;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			var rect = position;

			string[] keyWords = GetKeywords(prop);
			int index = Array.IndexOf(_values, prop.floatValue);
			if (index < 0)
			{
				Debug.LogError("LWGUI: Property: " + prop.name + " has unknown Enum Value: '" + prop.floatValue + "' !\n");
				return;
			}

			Helper.AdaptiveFieldWidth(EditorStyles.popup, _names[index]);
			int newIndex = EditorGUI.Popup(rect, label, index, _names);
			EditorGUI.showMixedValue = false;
			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				prop.floatValue = _values[newIndex];
				Helper.SelectShaderKeyword(editor.targets, keyWords, newIndex);
			}
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
			{
				Helper.SelectShaderKeyword(prop.targets, GetKeywords(prop), (int)prop.floatValue);
			}
		}
	}

	public class SubEnumDrawer : KWEnumDrawer
	{
		// UnityEditor.MaterialEnumDrawer(string enumName)
		// enumName: like "UnityEngine.Rendering.BlendMode"
		public SubEnumDrawer(string group, string enumName) : base(group, enumName)
		{
			var array = ReflectionHelper.GetAllTypes();
			try
			{
				Type enumType = array.FirstOrDefault(x => x.IsSubclassOf(typeof(Enum)) && (x.Name == enumName || x.FullName == enumName));
				string[] names = Enum.GetNames(enumType);
				Array valuesArray = Enum.GetValues(enumType);
				var values = new float[valuesArray.Length];
				for (int index = 0; index < valuesArray.Length; ++index)
					values[index] = (int)valuesArray.GetValue(index);
				Init(group, names, null, values);
			}
			catch (Exception ex)
			{
				Debug.LogWarningFormat("LWGUI: Failed to create SubEnum, enum {0} not found, {1}.", enumName, ex);
				throw;
			}
		}

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2)
			: base(group, new[] { n1, n2 }, null, new[] { v1, v2 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3)
			: base(group, new[] { n1, n2, n3 }, null, new[] { v1, v2, v3 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4)
			: base(group, new[] { n1, n2, n3, n4 }, null, new[] { v1, v2, v3, v4 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5)
			: base(group, new[] { n1, n2, n3, n4, n5 }, null, new[] { v1, v2, v3, v4, v5 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6)
			: base(group, new[] { n1, n2, n3, n4, n5, n6 }, null, new[] { v1, v2, v3, v4, v5, v6 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7)
			: base(group, new[] { n1, n2, n3, n4, n5, n6, n7 }, null, new[] { v1, v2, v3, v4, v5, v6, v7 }) { }

		protected override string GetKeywordName(string propName, string name) { return "_"; }

		public override void Apply(MaterialProperty prop) { }
	}

	public class SubKeywordEnumDrawer : KWEnumDrawer
	{
		public SubKeywordEnumDrawer(string group, string kw1, string kw2)
			: base(group, new[] { kw1, kw2 }, new[] { kw1, kw2 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3)
			: base(group, new[] { kw1, kw2, kw3 }, new[] { kw1, kw2, kw3 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4)
			: base(group, new[] { kw1, kw2, kw3, kw4 }, new[] { kw1, kw2, kw3, kw4 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5 }, new[] { kw1, kw2, kw3, kw4, kw5 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }) { }

		protected override string GetKeywordName(string propName, string name) { return (propName + "_" + name).Replace(' ', '_').ToUpperInvariant(); }
	}
	
	/// <summary>
	/// Popping a menu, you can select the Shader Property Preset, the Preset values will replaces the default values
	/// 
	/// group: parent group name (Default: none)
	///	presetFileName: "Shader Property Preset" asset name, you can create new Preset by
	///		"Right Click > Create > LWGUI > Shader Property Preset" in Project window,
	///		*any Preset in the entire project cannot have the same name*
	/// Target Property Type: Float, express current keyword index
	/// </summary>
	public class PresetDrawer : SubDrawer, IPresetDrawer
	{
		public string presetFileName;

		public PresetDrawer(string presetFileName) : this("_", presetFileName) { }

		public PresetDrawer(string group, string presetFileName)
		{
			this.group = group;
			this.presetFileName = presetFileName;
		}

		public static void SetPresetAssetToStaticData(PropertyStaticData inoutPropertyStaticData, string presetFileName)
		{
			inoutPropertyStaticData.propertyPresetAsset = PresetHelper.GetPresetAsset(presetFileName);
		}

		public static LwguiShaderPropertyPreset.Preset GetActivePresetFromFloatProperty(MaterialProperty inProp, LwguiShaderPropertyPreset lwguiShaderPropertyPreset)
		{
			LwguiShaderPropertyPreset.Preset preset = null;
			var index = (int)inProp.floatValue;
			if (lwguiShaderPropertyPreset && index >= 0 && index < lwguiShaderPropertyPreset.GetPresetCount())
			{
				preset = lwguiShaderPropertyPreset.GetPreset(index);
				preset.order = index != 0 ? lwguiShaderPropertyPreset.order : -1;
			}
			return preset;
		}

		// Apply Keywords and Passes in presets without modifying other property values
		// Used to call in MaterialPropertyDrawer.Apply()
		public static void ApplyPresetWithoutPropertyChanges(string presetFileName, MaterialProperty prop)
		{
			var presetFile = PresetHelper.GetPresetAsset(presetFileName);
			if (presetFile != null
			    && prop.floatValue < presetFile.GetPresetCount()
			    && ShowIfDecorator.GetShowIfResultToFilterDrawerApplying(prop)
			   )
			{
				presetFile.GetPreset(prop.floatValue)?.ApplyKeywordsAndPassesToMaterials(prop.targets);
			}
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Float; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			SetPresetAssetToStaticData(inoutPropertyStaticData, presetFileName);
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			var index = (int)inDefaultProp.floatValue;
			var propertyPreset = inPerShaderData.propStaticDatas[inProp.name].propertyPresetAsset;

			if (propertyPreset && index < propertyPreset.GetPresetCount() && index >= 0)
				inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription = propertyPreset.GetPreset(index).presetName;
		}

		public string GetPresetFileName() => presetFileName;

		public LwguiShaderPropertyPreset.Preset GetActivePreset(MaterialProperty inProp, LwguiShaderPropertyPreset lwguiShaderPropertyPreset) =>
			GetActivePresetFromFloatProperty(inProp, lwguiShaderPropertyPreset);

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			var rect = position;

			int index = (int)Mathf.Max(0, prop.floatValue);
			var presetFile = PresetHelper.GetPresetAsset(presetFileName);
			if (presetFile == null || presetFile.GetPresetCount() == 0)
			{
				Helper.DrawShaderPropertyWithErrorLabel(rect, prop, label, editor, $"Invalid Preset File: {presetFileName}");
				return;
			}

			if (index < presetFile.GetPresetCount())
			{
				var presetNames = presetFile.GetPresets().Select((inPreset) => new GUIContent(inPreset.presetName)).ToArray();
				if (EditorGUI.showMixedValue)
					index = -1;
				else
					Helper.AdaptiveFieldWidth(EditorStyles.popup, presetNames[index]);
				int newIndex = EditorGUI.Popup(rect, label, index, presetNames);
				if (Helper.EndChangeCheck(metaDatas, prop))
				{
					prop.floatValue = newIndex;
					presetFile.GetPreset(newIndex).ApplyToEditingMaterial(editor, metaDatas.perMaterialData);
				}
				EditorGUI.showMixedValue = false;
			}
			else
			{
				Helper.DrawShaderPropertyWithErrorLabel(position, prop, label, editor, $"Out of Index Range");
				Debug.LogError($"LWGUI: { prop.name } out of Preset index range!");
			}

		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
			{
				ApplyPresetWithoutPropertyChanges(presetFileName, prop);
			}
		}
	}

	/// <summary>
	/// Draw the Int value as a Bit Mask.
	/// Note:
	///		- Currently only 8 bits are supported.
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
	/// 
	/// group: parent group name (Default: none)
	/// bitDescription 7-0: Description of each Bit. (Default: none)
	/// Target Property Type: Int/Integer
	/// </summary>
	public class BitMaskDrawer : SubDrawer
	{
		public int bitCount = 8;
		
		public float maxHeight = EditorGUIUtility.singleLineHeight;

		public List<GUIContent> buttonLables = new ();
		
		public List<float> buttonWidths = new();
		
		public List<GUIStyle> buttonStyles = new();

		public float totalButtonWidth;
		
		private static readonly int _hint = "BitMask".GetHashCode();
		
		private static readonly float _minButtonWidth = 25;
		
		private static readonly float _buttonPadding = 1.0f;
		
		public BitMaskDrawer() : this(string.Empty, null) { }
		
		public BitMaskDrawer(string group) : this(group, null) { }
		
		public BitMaskDrawer(string group, string bitDescription7, string bitDescription6, string bitDescription5, string bitDescription4, string bitDescription3, string bitDescription2, string bitDescription1, string bitDescription0) 
			: this(group, new List<string>() { bitDescription0, bitDescription1, bitDescription2, bitDescription3, bitDescription4, bitDescription5, bitDescription6, bitDescription7 }) { }

		public BitMaskDrawer(string group, List<string> bitDescriptions)
		{
			this.group = group;

			bitCount = Mathf.Clamp(bitCount, 1, 16);

			for (int i = 0; i < bitCount; i++)
			{
				var description = bitDescriptions != null && bitDescriptions.Count > i ? bitDescriptions[i] : string.Empty;
				buttonLables.Add(new GUIContent(
					string.IsNullOrEmpty(description) ? i.ToString()			: i + "\n" + description));
				buttonWidths.Add(Mathf.Max(_minButtonWidth, EditorStyles.miniButton.CalcSize(buttonLables[i]).x));

				if (!string.IsNullOrEmpty(description))
					maxHeight = EditorGUIUtility.singleLineHeight * 2;
			}

			for (int i = 0; i < bitCount; i++)
			{
				if (i == 0)
					buttonStyles.Add(new GUIStyle(EditorStyles.miniButtonRight));
				else if (i == bitCount - 1)
					buttonStyles.Add(new GUIStyle(EditorStyles.miniButtonLeft));
				else
					buttonStyles.Add(new GUIStyle(EditorStyles.miniButton));
					
				buttonStyles[i].fixedHeight = maxHeight;
			}

			totalButtonWidth = buttonWidths.Sum();
		}
		
		protected override bool IsMatchPropType(MaterialProperty property) 
			=> property.GetPropertyType() is ShaderPropertyType.Float or ShaderPropertyType.Int;

		protected override float GetVisibleHeight(MaterialProperty prop) { return maxHeight; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			label.tooltip += $"\nCurrent Value: { prop.GetNumericValue() }";
			
			int controlId = GUIUtility.GetControlID(_hint, FocusType.Keyboard, position);
			var fieldRect = EditorGUI.PrefixLabel(position, controlId, label);
			
			if (position.width < totalButtonWidth) 
				return;

			fieldRect.xMin = fieldRect.xMax;
			
			for (int i = 0; i < bitCount; i++)
			{
				fieldRect.xMin = fieldRect.xMax - buttonWidths[i];
				var buttonLable = buttonLables[i];
				var active = RuntimeHelper.IsBitEnabled((int)prop.GetNumericValue(), i);
				var style = buttonStyles[i];
				var buttonRect = fieldRect;
				
				if (i > 0 && i < bitCount - 1)
				{
					buttonRect.xMin -= _buttonPadding;
					buttonRect.xMax += _buttonPadding * 2;
				}
				
				if (style.richText = prop.hasMixedValue)
				{
					// https://docs.unity3d.com/2021.3/Documentation/Manual/StyledText.html
					buttonLable = new GUIContent($"<b><i>{ buttonLable.text }</i></b>");
				}

				if (Helper.ToggleButton(buttonRect, buttonLable, active, style, _buttonPadding * 1.5f))
				{
					prop.SetNumericValue(RuntimeHelper.SetBitEnabled((int)prop.GetNumericValue(), i, !active));
				}

				fieldRect.xMax = fieldRect.xMin;
			}
		}
	}
	
	/// <summary>
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
	/// </summary>
	public class RampAtlasIndexerDrawer : RampDrawer
	{
		public string rampAtlasPropName = string.Empty;
		public string defaultRampName = "Ramp";

		private LwguiRampAtlas _rampAtlasSO;
		public LwguiRampAtlas rampAtlasSO
		{
			get
			{
				if (!_rampAtlasSO)
				{
					var rampAtlasProp = metaDatas.GetProperty(rampAtlasPropName);
					if (rampAtlasProp != null && rampAtlasProp.GetPropertyType() == ShaderPropertyType.Texture)
					{
						_rampAtlasSO = LwguiRampAtlas.LoadRampAtlasSO(rampAtlasProp.textureValue);
					}
				} 
				return _rampAtlasSO;
			}

			set => _rampAtlasSO = value;
		}

		private LwguiRampAtlas.Ramp _currentRamp;
		private bool _rampAtlasSOHasMixedValue;
		
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName) : this(group, rampAtlasPropName, "Ramp") {}
		
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName) : this(group, rampAtlasPropName, defaultRampName, "sRGB") {}
		
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace) : this(group, rampAtlasPropName, defaultRampName, colorSpace, "RGBA") {}
		
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace, string viewChannelMask) : this(group, rampAtlasPropName, defaultRampName, colorSpace, viewChannelMask, 1) {}
			
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace, string viewChannelMask, float timeRange)
		{
			this.group = group;
			this.rampAtlasPropName = rampAtlasPropName;
			this.defaultRampName = defaultRampName;
			this.colorSpace = colorSpace.ToLower() == "linear" ? ColorSpace.Linear : ColorSpace.Gamma;
			this.viewChannelMask = LwguiGradient.ChannelMask.None;
			{
				viewChannelMask = viewChannelMask.ToLower();
				for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
				{
					if (viewChannelMask.Contains(LwguiGradient.channelNames[c]))
						this.viewChannelMask |= LwguiGradient.ChannelIndexToMask(c);
				}
			}
			this.timeRange = LwguiGradient.GradientTimeRange.One;
			{
				if ((int)timeRange == (int)LwguiGradient.GradientTimeRange.TwentyFour)
					this.timeRange = LwguiGradient.GradientTimeRange.TwentyFour;
				else if ((int)timeRange == (int)LwguiGradient.GradientTimeRange.TwentyFourHundred)
					this.timeRange = LwguiGradient.GradientTimeRange.TwentyFourHundred;
			}
		}

		protected override bool IsMatchPropType(MaterialProperty property) => property.GetPropertyType() is ShaderPropertyType.Float or ShaderPropertyType.Int;

		protected override void OnRampPropUpdate(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			if (doRegisterUndo)
				OnGradientEditorChange(null);
		}

		protected override void OnGradientEditorChange(LwguiGradient gradient)
		{
			LwguiGradientWindow.RegisterSerializedObjectUndo(rampAtlasSO);
		}

		protected override void OnEditRampMap(MaterialProperty prop, LwguiGradient gradient)
		{	
			rampAtlasSO?.UpdateTexturePixels();
			OnGradientEditorChange(null);
		}

		protected override void OnSaveRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			rampAtlasSO?.SaveTexture(checkoutAndForceWrite:true);
			rampAtlasSO?.SaveRampAtlasSO();
		}

		protected override LwguiGradient GetLwguiGradient(MaterialProperty prop, out bool isDirty)
		{
			isDirty = false;
			if (rampAtlasSO && (int)prop.GetNumericValue() < rampAtlasSO.ramps.Count)
			{
				_currentRamp = rampAtlasSO.GetRamp((int)prop.GetNumericValue());
				if (_currentRamp == null)
					return null;
				
				isDirty = EditorUtility.IsDirty(rampAtlasSO);
				colorSpace = _currentRamp.colorSpace;
				viewChannelMask = _currentRamp.channelMask;
				timeRange = _currentRamp.timeRange;
				return _currentRamp.gradient;
			}
			else
				return null;
		}

		protected override void CreateNewRampMap(MaterialProperty prop, MaterialEditor editor)
		{
			// Create a Ramp
			if (rampAtlasSO)
			{
				var newIndex = rampAtlasSO.ramps.Count;
				
				rampAtlasSO.ramps.Add(new LwguiRampAtlas.Ramp()
				{
					name = defaultRampName,
					gradient = LwguiGradient.white,
					colorSpace = colorSpace,
					channelMask = viewChannelMask,
					timeRange = timeRange
				});
				
				prop.SetNumericValue(newIndex);
				if (newIndex >= rampAtlasSO.rampAtlasHeight)
					rampAtlasSO.rampAtlasHeight *= 2;
				rampAtlasSO.UpdateTexturePixels();
			}
			// Create a Ramp Atlas SO
			else
			{
				var rampAtlasProp = metaDatas.GetProperty(rampAtlasPropName);
				var newRampAtlasSO = LwguiRampAtlas.CreateRampAtlasSO(rampAtlasProp, metaDatas);
				if (newRampAtlasSO)
				{
					rampAtlasSO = newRampAtlasSO;
					rampAtlasProp.textureValue = rampAtlasSO?.rampAtlasTexture;
				}
			}
		}

		protected override void SwitchRampMap(MaterialProperty prop, Texture2D newRampMap, int index)
		{
			prop.SetNumericValue(index);
			OnSwitchRampMap(prop, newRampMap, index);
			LWGUI.OnValidate(metaDatas);
		}

		protected override LwguiGradient DiscardRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			rampAtlasSO?.DiscardChanges();
			_currentRamp = rampAtlasSO?.GetRamp((int)prop.GetNumericValue());
			return _currentRamp?.gradient;
		}

		protected override void DrawRampSelector(Rect selectButtonRect, MaterialProperty prop, LwguiGradient gradient)
		{
			if (!_rampAtlasSOHasMixedValue)
				RampHelper.RampIndexSelectorOverride(selectButtonRect, prop, rampAtlasSO, SwitchRampMap);
		}

		protected override void DrawRampObjectField(Rect rampFieldRect, MaterialProperty prop, LwguiGradient gradient)
		{
			if (rampAtlasSO && !_rampAtlasSOHasMixedValue)
				EditorGUI.ObjectField(rampFieldRect, gradient?.GetPreviewRampTexture(), typeof(Texture2D), false);
			else
			{
				EditorGUI.BeginChangeCheck();
				var newRampAtlasSO = EditorGUI.ObjectField(rampFieldRect, rampAtlasSO, typeof(LwguiRampAtlas), false) as LwguiRampAtlas;
				if (EditorGUI.EndChangeCheck())
				{
					rampAtlasSO = newRampAtlasSO;
					metaDatas.GetProperty(rampAtlasPropName).textureValue = rampAtlasSO?.rampAtlasTexture;
				}
			}
		}

		protected override void DrawPreviewTextureOverride(Rect previewRect, MaterialProperty prop, LwguiGradient gradient)
		{
			if (gradient != null && !EditorGUI.showMixedValue)
				LwguiGradientEditorHelper.DrawGradientWithSeparateAlphaChannel(previewRect, gradient, colorSpace, viewChannelMask);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var rampAtlasProp = metaDatas.GetProperty(rampAtlasPropName);
			if (rampAtlasProp == null || rampAtlasProp.GetPropertyType() != ShaderPropertyType.Texture)
			{
				Helper.DrawShaderPropertyWithErrorLabel(position, prop, label, editor, "Invalid rampAtlasPropName");
				Debug.LogError($"LWGUI: Property { prop.name } has invalid rampAtlasPropName: { rampAtlasPropName }");
				return;
			}

			// Add Info to label
			var currentIndex = (int)prop.GetNumericValue();
			label.tooltip += $"\nCurrent Value: {currentIndex}";
			if (rampAtlasSO)
			{
				var isOutOfRange = currentIndex >= rampAtlasSO.ramps.Count;
				var info = isOutOfRange ? "OUT OF RANGE!" : rampAtlasSO.ramps[currentIndex].name;
				label.text += $" ({ currentIndex }: { info } - { rampAtlasSO.ramps.Count })";
			}
			else
				label.text += $" ({ currentIndex } - NULL)";

			// Handle Mixed Value
			_rampAtlasSOHasMixedValue = rampAtlasProp.hasMixedValue;
			var showMixedValue = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = prop.hasMixedValue || _rampAtlasSOHasMixedValue;

			
			base.DrawProp(position, prop, label, editor);
			
			
			// Clear
			_rampAtlasSO = null;
			_currentRamp = null;
			EditorGUI.showMixedValue = showMixedValue;
		}
	}

	#endregion

	#region Texture
	/// <summary>
	/// Draw a Texture property in single line with a extra property
	/// 
	/// group: parent group name (Default: none)
	/// extraPropName: extra property name (Default: none)
	/// Target Property Type: Texture
	/// Extra Property Type: Color, Vector
	/// Target Property Type: Texture2D
	/// </summary>
	public class TexDrawer : SubDrawer
	{
		private string        _extraPropName = String.Empty;
		private ChannelDrawer _channelDrawer = new ChannelDrawer();

		public TexDrawer() { }

		public TexDrawer(string group) : this(group, String.Empty) { }

		public TexDrawer(string group, string extraPropName)
		{
			this.group = group;
			this._extraPropName = extraPropName;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight; }

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Texture; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			inoutPropertyStaticData.AddExtraProperty(_extraPropName);
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			var defaultExtraProp = inoutPerMaterialData.GetPropDynamicData(_extraPropName)?.defualtProperty;
			if (defaultExtraProp != null)
			{
				var text = string.Empty;
				if (defaultExtraProp.GetPropertyType() == ShaderPropertyType.Vector)
					text = ChannelDrawer.GetChannelName(defaultExtraProp);
				else
					text = RevertableHelper.GetPropertyDefaultValueText(defaultExtraProp);

				inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription =
					RevertableHelper.GetPropertyDefaultValueText(inDefaultProp) + ", " + text;
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position;

			MaterialProperty extraProp = metaDatas.GetProperty(_extraPropName);
			if (extraProp != null
			 // && (
				// 	extraProp.type == MaterialProperty.PropType.Color
				//  || extraProp.type == MaterialProperty.PropType.Vector
				// )
			)
			{
				var i = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;

				var extraRect = MaterialEditor.GetRightAlignedFieldRect(rect);
				extraRect.height = rect.height;

				if (extraProp.GetPropertyType() == ShaderPropertyType.Vector)
					_channelDrawer.OnGUI(extraRect, extraProp, GUIContent.none, editor);
				else
					editor.ShaderProperty(extraRect, extraProp, GUIContent.none);

				EditorGUI.indentLevel = i;
			}

			editor.TexturePropertyMiniThumbnail(rect, prop, label.text, label.tooltip);

			EditorGUI.showMixedValue = false;
		}
	}

	/// <summary>
	/// Draw an unreal style Ramp Map Editor (Default Ramp Map Resolution: 256 * 2)
	/// NEW: The new LwguiGradient type has both the Gradient and Curve editors, and can be used in C# scripts and runtime, and is intended to replace UnityEngine.Gradient
	/// 
	/// group: parent group name (Default: none)
	/// defaultFileName: default Ramp Map file name when create a new one (Default: RampMap)
	/// rootPath: the path where ramp is stored, replace '/' with '.' (for example: Assets.Art.Ramps). when selecting ramp, it will also be filtered according to the path (Default: Assets)
	/// colorSpace: switch sRGB / Linear in ramp texture import setting (Default: sRGB)
	/// defaultWidth: default Ramp Width. (Default: 256)
	/// viewChannelMask: editable channels. (Default: RGBA)
	/// timeRange: the abscissa display range (1/24/2400), is used to optimize the editing experience when the abscissa is time of day. (Default: 1)
	/// Target Property Type: Texture2D
	/// </summary>
	public class RampDrawer : SubDrawer
	{
		public static readonly string DefaultRootPath = "Assets";

		public string rootPath = "Assets";
		public string saveFilePanelTitle = "Create New Ramp Texture";
		public string defaultFileName = "RampMap";
		public string fileExtension = "png";
		public int defaultWidth = 256;
		public int defaultHeight = 2;
		public ColorSpace colorSpace = ColorSpace.Gamma;
		public LwguiGradient.ChannelMask viewChannelMask = LwguiGradient.ChannelMask.All;
		public LwguiGradient.GradientTimeRange timeRange = LwguiGradient.GradientTimeRange.One;
		public bool doRegisterUndo;

		private static readonly GUIContent _iconMixImage = EditorGUIUtility.IconContent("darkviewbackground");

		private static readonly float _rampPreviewHeight = EditorGUIUtility.singleLineHeight;
		private static readonly float _rampButtonsHeight = EditorGUIUtility.singleLineHeight;
		
		protected override float GetVisibleHeight(MaterialProperty prop) { return _rampPreviewHeight + _rampButtonsHeight; }

		public RampDrawer() : this(String.Empty) { }

		public RampDrawer(string group) : this(group, "RampMap") { }

		public RampDrawer(string group, string defaultFileName) : this(group, defaultFileName, DefaultRootPath, 256) { }

		public RampDrawer(string group, string defaultFileName, float defaultWidth) : this(group, defaultFileName, DefaultRootPath, defaultWidth) { }

		public RampDrawer(string group, string defaultFileName, string rootPath, float defaultWidth) : this(group, defaultFileName, rootPath, "sRGB", defaultWidth) { }
		
		public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, "RGBA") { }
		
		public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, string viewChannelMask) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, viewChannelMask, 1) { }

		public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, string viewChannelMask, float timeRange)
		{
			if (!rootPath.StartsWith(DefaultRootPath))
			{
				Debug.LogError("LWGUI: Ramp Root Path: '" + rootPath + "' must start with 'Assets'!");
				rootPath = DefaultRootPath;
			}
			this.group = group;
			this.defaultFileName = defaultFileName;
			this.rootPath = rootPath.Replace('.', '/');
			this.colorSpace = colorSpace.ToLower() == "linear" ? ColorSpace.Linear : ColorSpace.Gamma;
			this.defaultWidth = (int)Mathf.Max(2, defaultWidth);
			this.viewChannelMask = LwguiGradient.ChannelMask.None;
			{
				viewChannelMask = viewChannelMask.ToLower();
				for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
				{
					if (viewChannelMask.Contains(LwguiGradient.channelNames[c]))
						this.viewChannelMask |= LwguiGradient.ChannelIndexToMask(c);
				}

				if (this.viewChannelMask == (LwguiGradient.ChannelMask.RGB | LwguiGradient.ChannelMask.Alpha))
					this.viewChannelMask = LwguiGradient.ChannelMask.All;
			}
			this.timeRange = LwguiGradient.GradientTimeRange.One;
			{
				if ((int)timeRange == (int)LwguiGradient.GradientTimeRange.TwentyFour)
					this.timeRange = LwguiGradient.GradientTimeRange.TwentyFour;
				else if ((int)timeRange == (int)LwguiGradient.GradientTimeRange.TwentyFourHundred)
					this.timeRange = LwguiGradient.GradientTimeRange.TwentyFourHundred;
			}
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Texture; }

		protected virtual void OnRampPropUpdate(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }

		protected virtual void OnCreateNewRampMap(MaterialProperty prop) { }

		protected virtual void OnGradientEditorChange(LwguiGradient gradient) { }
		
		protected virtual void OnEditRampMap(MaterialProperty prop, LwguiGradient gradient) { }
		
		protected virtual void OnSaveRampMap(MaterialProperty prop, LwguiGradient gradient) { }
		
		protected virtual void OnDiscardRampMap(MaterialProperty prop, LwguiGradient gradient) { }
		
		protected virtual void OnSwitchRampMap(MaterialProperty prop, Texture2D newRampMap, int index) { }
		
		protected virtual LwguiGradient GetLwguiGradient(MaterialProperty prop, out bool isDirty)
		{
			return RampHelper.GetGradientFromTexture(prop.textureValue, out isDirty, false, doRegisterUndo);
		}

		protected virtual void EditWhenNoRampMap(MaterialProperty prop, MaterialEditor editor)
		{
			LwguiGradientWindow.CloseWindow();
			CreateNewRampMap(prop, editor);
			OnCreateNewRampMap(prop);
			LWGUI.OnValidate(metaDatas);
		}
		
		protected virtual void CreateNewRampMap(MaterialProperty prop, MaterialEditor editor)
		{
			string createdFileRelativePath = string.Empty;
			while (true)
			{
				if (!Directory.Exists(Helper.ProjectPath + rootPath))
					Directory.CreateDirectory(Helper.ProjectPath + rootPath);

				// TODO: Warning:
				// PropertiesGUI() is being called recursively. If you want to render the default gui for shader properties then call PropertiesDefaultGUI() instead
				var absPath = EditorUtility.SaveFilePanel(saveFilePanelTitle, rootPath, defaultFileName, fileExtension);
					
				if (absPath.StartsWith(Helper.ProjectPath + rootPath))
				{
					createdFileRelativePath = absPath.Replace(Helper.ProjectPath, string.Empty);
					break;
				}
				else if (absPath != string.Empty)
				{
					var retry = EditorUtility.DisplayDialog("Invalid Path", "Please select the subdirectory of '" + Helper.ProjectPath + rootPath + "'", "Retry", "Cancel");
					if (!retry) break;
				}
				else
				{
					break;
				}
			}

			if (!string.IsNullOrEmpty(createdFileRelativePath))
			{
				RampHelper.CreateAndSaveNewGradientTexture(defaultWidth, defaultHeight, createdFileRelativePath, colorSpace == ColorSpace.Linear);
				prop.textureValue = AssetDatabase.LoadAssetAtPath<Texture2D>(createdFileRelativePath);
			}
		}

		protected virtual void ChangeRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			RampHelper.SetGradientToTexture(prop.textureValue, gradient, false);
		}

		protected virtual void SaveRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			RampHelper.SetGradientToTexture(prop.textureValue, gradient, true);
		}
		
		protected virtual void SwitchRampMap(MaterialProperty prop, Texture2D newRampMap, int index)
		{
			prop.textureValue = newRampMap;
			OnSwitchRampMap(prop, newRampMap, index);
			LWGUI.OnValidate(metaDatas);
		}

		protected virtual LwguiGradient DiscardRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			// Tex > Gradient
			gradient = RampHelper.GetGradientFromTexture(prop.textureValue, out _, true);
			// GradientObject > Tex
			RampHelper.SetGradientToTexture(prop.textureValue, gradient, true);
			return gradient;
		}

		protected virtual void DrawRampSelector(Rect selectButtonRect, MaterialProperty prop, LwguiGradient gradient)
		{
			RampHelper.RampMapSelectorOverride(selectButtonRect, prop, rootPath, SwitchRampMap);
		}

		// Manual replace ramp map
		protected virtual void DrawRampObjectField(Rect rampFieldRect, MaterialProperty prop, LwguiGradient gradient)
		{
			EditorGUI.BeginChangeCheck();
			var newManualSelectedTexture = (Texture2D)EditorGUI.ObjectField(rampFieldRect, prop.textureValue, typeof(Texture2D), false);
			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				if (newManualSelectedTexture && !AssetDatabase.GetAssetPath(newManualSelectedTexture).StartsWith(rootPath))
					EditorUtility.DisplayDialog("Invalid Path", "Please select the subdirectory of '" + rootPath + "'", "OK");
				else
					SwitchRampMap(prop, newManualSelectedTexture, 0);
			}
		}

		protected virtual void DrawPreviewTextureOverride(Rect previewRect, MaterialProperty prop, LwguiGradient gradient)
		{
			if (!prop.hasMixedValue && gradient != null)
			{
				LwguiGradientEditorHelper.DrawGradientWithSeparateAlphaChannel(previewRect, gradient, colorSpace, viewChannelMask);
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var labelWidth = EditorGUIUtility.labelWidth;
			var indentLevel = EditorGUI.indentLevel;

			OnRampPropUpdate(position, prop, label, editor);

			var gradient = GetLwguiGradient(prop, out var isDirty);

			// Draw Label
			var labelRect = new Rect(position);
			{
				labelRect.height = _rampPreviewHeight;
				EditorGUI.PrefixLabel(labelRect, label);
			}

			// Ramp buttons Rect
			var buttonRect = new Rect(position);
			{
				EditorGUIUtility.labelWidth = 0;
				EditorGUI.indentLevel = 0;
				buttonRect.yMin = buttonRect.yMax - _rampPreviewHeight;
				buttonRect = MaterialEditor.GetRectAfterLabelWidth(buttonRect);
				if (buttonRect.width < 50f) return;
			}

			// Draw Ramp Editor
			RampHelper.RampEditor(buttonRect, ref gradient, colorSpace, viewChannelMask, timeRange, isDirty,  
				out bool hasGradientChanges, 
				out bool doEditWhenNoGradient, 
				out doRegisterUndo, 
				out bool doCreate, 
				out bool doSaveGradient, 
				out bool doDiscardGradient,
				OnGradientEditorChange);
			
			// Edit When No Gradient
			if (doEditWhenNoGradient)
			{
				EditWhenNoRampMap(prop, editor);
			}
			
			// Create
			if (doCreate)
			{
				LwguiGradientWindow.CloseWindow();
				CreateNewRampMap(prop, editor);
				OnCreateNewRampMap(prop);
				LWGUI.OnValidate(metaDatas);
			}

			// Change
			if (hasGradientChanges && gradient != null)
			{
				ChangeRampMap(prop, gradient);
				OnEditRampMap(prop, gradient);
			}
			
			// Save
			if (doSaveGradient && gradient != null)
			{
				SaveRampMap(prop, gradient);
				OnSaveRampMap(prop, gradient);
			}

			// Discard
			if (doDiscardGradient)
			{
				LwguiGradientWindow.CloseWindow();
				gradient = DiscardRampMap(prop, gradient);
				OnDiscardRampMap(prop, gradient);
			}

			// Texture Object Field, handle switch texture event
			var rampFieldRect = MaterialEditor.GetRectAfterLabelWidth(labelRect);
			var previewRect = new Rect(rampFieldRect.x + 0.5f, rampFieldRect.y + 0.5f, rampFieldRect.width - 18, rampFieldRect.height - 0.5f);
			{
				var selectButtonRect = new Rect(previewRect.xMax, rampFieldRect.y, rampFieldRect.width - previewRect.width, rampFieldRect.height);
				DrawRampSelector(selectButtonRect, prop, gradient);

				DrawRampObjectField(rampFieldRect, prop, gradient);
			}

			// Preview texture override (larger preview, hides texture name)
			DrawPreviewTextureOverride(previewRect, prop, gradient);

			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUI.indentLevel = indentLevel;
		}
	}

	/// <summary>
	/// Draw a "Ramp Atlas Scriptable Object" selector and texture preview.
	/// The Ramp Atlas SO is responsible for storing multiple ramps and generating the corresponding Ramp Atlas Texture.
	/// Use it together with RampAtlasIndexer() to sample specific ramps in Shader using Index, similar to UE's Curve Atlas.
	/// Note: Currently, the material only saves Texture reference and Int value,
	///		  if you manually modify the Ramp Atlas, the references will not update automatically!
	/// 
	/// group: parent group name (Default: none)
	/// defaultFileName: the default file name when creating a Ramp Atlas SO (Default: RampAtlas)
	/// rootPath: the default directory when creating a Ramp Atlas SO, replace '/' with '.' (for example: Assets.Art.RampAtlas). (Default: Assets)
	/// colorSpace: the Color Space of Ramp Atlas Texture. (sRGB/Linear) (Default: sRGB)
	/// defaultWidth: default Ramp Atlas Texture width (Default: 256)
	/// showAtlasPreview: Draw the preview of Ramp Atlas below (True/False) (Default: True)
	/// Target Property Type: Texture2D
	/// </summary>
	public class RampAtlasDrawer : SubDrawer
	{
		public string rootPath = "Assets";
		public string defaultFileName = "RampAtlas";
		public bool defaultAtlasSRGB = true;
		public int defaultAtlasWidth = 256;
		public int defaultAtlasHeight = 2;
		public bool showAtlasPreview = true;
		
		protected LwguiRampAtlas _rampAtlasSO;
		
		public RampAtlasDrawer() : this(string.Empty) { }
		
		public RampAtlasDrawer(string group) : this(group, "RampAtlas") { }
		
		public RampAtlasDrawer(string group, string defaultFileName) : this(group, defaultFileName, "Assets") { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath) : this(group, defaultFileName, rootPath, "sRGB") { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace) : this(group, defaultFileName, rootPath, colorSpace, 256) { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, 4) { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, float defaultHeight) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, defaultHeight, "true") { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, float defaultHeight, string showAtlasPreview)
		{
			if (!rootPath.StartsWith(this.rootPath))
			{
				Debug.LogError("LWGUI: Ramp Atlas Root Path: '" + rootPath + "' must start with 'Assets'!");
				rootPath = this.rootPath;
			}
			this.group = group;
			this.defaultFileName = defaultFileName;
			this.rootPath = rootPath.Replace('.', '/');
			this.defaultAtlasSRGB = colorSpace.ToLower() == "srgb";
			this.defaultAtlasWidth = (int)Mathf.Max(2, defaultWidth);
			this.defaultAtlasHeight = (int)Mathf.Max(2, defaultHeight);
			this.showAtlasPreview = Helper.StringToBool(showAtlasPreview);
		}

		protected override bool IsMatchPropType(MaterialProperty property) => property.GetPropertyType() == ShaderPropertyType.Texture;

		protected override float GetVisibleHeight(MaterialProperty prop) => 
			EditorGUIUtility.singleLineHeight + 
			(prop.textureValue && showAtlasPreview ? MaterialEditor.GetDefaultPropertyHeight(prop) + 2.0f : 0);

		public override void GetCustomContextMenus(GenericMenu menu, Rect rect, MaterialProperty prop, LWGUIMetaDatas metaDatas)
		{
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Create Ramp Atlas"), false, () =>
			{
				_rampAtlasSO = LwguiRampAtlas.CreateRampAtlasSO(prop, metaDatas);
				if (_rampAtlasSO)
				{
					prop.textureValue = _rampAtlasSO.rampAtlasTexture;
					LWGUI.OnValidate(metaDatas);
				}
			});

			if (_rampAtlasSO)
			{
				menu.AddItem(new GUIContent("Clone Ramp Atlas"), false, () =>
				{
					var newRampAtlasSO = LwguiRampAtlas.CloneRampAtlasSO(_rampAtlasSO);
					if (newRampAtlasSO)
					{
						_rampAtlasSO = newRampAtlasSO;
						prop.textureValue = _rampAtlasSO.rampAtlasTexture;
						LWGUI.OnValidate(metaDatas);
					}
				});
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.PrefixLabel(position, label);
			
			var labelWidth = EditorGUIUtility.labelWidth;
			var indentLevel = EditorGUI.indentLevel;
			EditorGUIUtility.labelWidth = 0;
			EditorGUI.indentLevel = 0;
			
			var fieldRect = MaterialEditor.GetRectAfterLabelWidth(position);
			var rampAtlasSORect = new Rect(fieldRect.x, fieldRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
			var rampAtlasTextureRect = new Rect(fieldRect.x, rampAtlasSORect.yMax + 2.0f, fieldRect.width, MaterialEditor.GetDefaultPropertyHeight(prop));

			_rampAtlasSO = LwguiRampAtlas.LoadRampAtlasSO(prop.textureValue);
			
			// Disable ObjectField's Context Menu
			var e = Event.current;
			if (e?.type == EventType.MouseDown && Event.current.button == 1
			    && (rampAtlasSORect.Contains(e.mousePosition)))
			{
				e.Use();
			}
			
			EditorGUI.BeginChangeCheck();
			_rampAtlasSO = (LwguiRampAtlas)EditorGUI.ObjectField(rampAtlasSORect, GUIContent.none, _rampAtlasSO, typeof(LwguiRampAtlas), false);
			if (EditorGUI.EndChangeCheck())
			{
				prop.textureValue = LwguiRampAtlas.LoadRampAtlasTexture(_rampAtlasSO);

				if (_rampAtlasSO && !prop.textureValue)
				{
					Debug.LogError($"LWGUI: Can NOT load the Ramp Atlas Texture from: { _rampAtlasSO.name }");
				}
			}

			if (showAtlasPreview && prop.textureValue && !prop.hasMixedValue)
			{
				var filter = prop.textureValue.filterMode;
				prop.textureValue.filterMode = FilterMode.Point;
				EditorGUI.DrawPreviewTexture(rampAtlasTextureRect, prop.textureValue);
				prop.textureValue.filterMode = filter;
			}
		}
	}

	/// <summary>
	/// Draw an image preview.
	/// display name: The path of the image file relative to the Unity project, such as: "Assets/test.png", "Doc/test.png", "../test.png"
	/// 
	/// group: parent group name (Default: none)
	/// Target Property Type: Any
	/// </summary>
	public class ImageDrawer : SubDrawer
	{
		public ImageDrawer() { }

		public ImageDrawer(string group)
		{
			this.group = group;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			var inputPath = (inProp.displayName ?? string.Empty).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			var projectRoot = Helper.ProjectPath;
			string imagePath = null;

			try
			{
				// If the display path already starts with Assets (project-relative), resolve from project root
				if (inputPath.StartsWith("Assets"))
				{
					imagePath = Path.Combine(projectRoot, inputPath);
				}
				// If it's an absolute path, use it directly
				else if (Path.IsPathRooted(inputPath))
				{
					imagePath = inputPath;
				}
				else
				{
					// Try resolving relative to the shader file location
					var shaderAssetPath = AssetDatabase.GetAssetPath(inShader);
					if (!string.IsNullOrEmpty(shaderAssetPath))
					{
						var shaderFullPath = Path.Combine(projectRoot, shaderAssetPath);
						var shaderDir = Path.GetDirectoryName(shaderFullPath);
						if (!string.IsNullOrEmpty(shaderDir))
						{
							// Combine and normalize to resolve ../ segments
							var candidate = Path.GetFullPath(Path.Combine(shaderDir, inputPath));
							if (File.Exists(candidate))
							{
								imagePath = candidate;
							}
						}
					}
					// Fallback: try project-root relative resolution
					if (imagePath == null)
					{
						var candidate2 = Path.Combine(projectRoot, inputPath);
						if (File.Exists(candidate2)) imagePath = candidate2;
					}
				}

				if (!string.IsNullOrEmpty(imagePath) && File.Exists(imagePath))
				{
					var fileData = File.ReadAllBytes(imagePath);
					Texture2D texture = new Texture2D(2, 2);

					// LoadImage will auto-resize the texture dimensions
					if (texture.LoadImage(fileData))
					{
						inoutPropertyStaticData.image = texture;
					}
					else
					{
						Debug.LogError($"LWGUI: Failed to load image data into texture: { imagePath }");
					}
				}
				else
				{
					Debug.LogError($"LWGUI: Image path not found: { inputPath }");
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"LWGUI: Exception while resolving image path '{inputPath}': {ex.Message}");
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var image = metaDatas.GetPropStaticData(prop).image;
			if (image)
			{
				var scaledheight = Mathf.Max(0, image.height / (image.width / Helper.GetCurrentPropertyLayoutWidth()));
				var rect = EditorGUILayout.GetControlRect(true, scaledheight);
				rect = RevertableHelper.IndentRect(EditorGUI.IndentedRect(rect));
				EditorGUI.DrawPreviewTexture(rect, image);

				if (GUI.enabled)
					prop.textureValue = null;
			}
		}
	}
	#endregion

	#region Vector
	/// <summary>
	/// Display up to 4 colors in a single line
	/// 
	/// group: parent group name (Default: none)
	/// color2-4: extra color property name
	/// Target Property Type: Color
	/// </summary>
	public class ColorDrawer : SubDrawer
	{
		private string[] _colorStrings = new string[3];

		public ColorDrawer(string group, string color2) : this(group, color2, String.Empty, String.Empty) { }

		public ColorDrawer(string group, string color2, string color3) : this(group, color2, color3, String.Empty) { }

		public ColorDrawer(string group, string color2, string color3, string color4)
		{
			this.group = group;
			this._colorStrings[0] = color2;
			this._colorStrings[1] = color3;
			this._colorStrings[2] = color4;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Color; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			foreach (var colorPropName in _colorStrings)
			{
				inoutPropertyStaticData.AddExtraProperty(colorPropName);
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var cProps = new Stack<MaterialProperty>();
			for (int i = 0; i < 4; i++)
			{
				if (i == 0)
				{
					cProps.Push(prop);
					continue;
				}

				var p = metaDatas.GetProperty(_colorStrings[i - 1]);
				if (p != null && IsMatchPropType(p))
					cProps.Push(p);
			}

			var count = cProps.Count;
			var colorArray = cProps.ToArray();

			EditorGUI.PrefixLabel(position, label);

			for (int i = 0; i < count; i++)
			{
				EditorGUI.BeginChangeCheck();
				var cProp = colorArray[i];
				EditorGUI.showMixedValue = cProp.hasMixedValue;
				var r = new Rect(position);
				var interval = 13 * i * (-0.25f + EditorGUI.indentLevel * 1.25f);
				var w = EditorGUIUtility.fieldWidth * (0.8f + EditorGUI.indentLevel * 0.2f);
				r.xMin += r.width - w * (i + 1) + interval;
				r.xMax -= w * i - interval;

				var src = cProp.colorValue;
				var isHdr = (colorArray[i].GetPropertyFlags() & ShaderPropertyFlags.HDR) != ShaderPropertyFlags.None;
				var dst = EditorGUI.ColorField(r, GUIContent.none, src, true, true, isHdr);
				if (Helper.EndChangeCheck(metaDatas, cProp))
				{
					cProp.colorValue = dst;
				}
			}

			EditorGUI.showMixedValue = false;
		}
	}

	/// <summary>
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
	/// </summary>
	public class ChannelDrawer : SubDrawer
	{
		private static GUIContent[] _names = new[]
		{
			new GUIContent("R"),
			new GUIContent("G"),
			new GUIContent("B"),
			new GUIContent("A"),
			new GUIContent("RGB Average"),
			new GUIContent("RGB Luminance"),
			new GUIContent("None")
		};
		private static int[] _intValues = new int[] { 0, 1, 2, 3, 4, 5, 6 };
		private static Vector4[] _vector4Values = new[]
		{
			new Vector4(1, 0, 0, 0),
			new Vector4(0, 1, 0, 0),
			new Vector4(0, 0, 1, 0),
			new Vector4(0, 0, 0, 1),
			new Vector4(1f / 3f, 1f / 3f, 1f / 3f, 0),
			new Vector4(0.2126f, 0.7152f, 0.0722f, 0),
			new Vector4(0, 0, 0, 0)
		};

		public ChannelDrawer() { }

		public ChannelDrawer(string group)
		{
			this.group = group;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Vector; }

		private static int GetChannelIndex(MaterialProperty prop)
		{
			int index = -1;
			for (int i = 0; i < _vector4Values.Length; i++)
			{
				if (prop.vectorValue == _vector4Values[i])
					index = i;
			}
			if (index == -1)
			{
				Debug.LogError("LWGUI: Channel Property: " + prop.name + " invalid vector found, reset to A");
				prop.vectorValue = _vector4Values[3];
				index = 3;
			}
			return index;
		}

		public static string GetChannelName(MaterialProperty prop)
		{
			return _names[GetChannelIndex(prop)].text;
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription = GetChannelName(inDefaultProp);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();

			EditorGUI.showMixedValue = prop.hasMixedValue;
			var index = GetChannelIndex(prop);
			int num = EditorGUI.IntPopup(position, label, index, _names, _intValues);
			EditorGUI.showMixedValue = false;
			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				prop.vectorValue = _vector4Values[num];
			}
		}
	}
	#endregion

	#region Other
	/// <summary>
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
	/// </summary>
	public class ButtonDrawer : SubDrawer
	{
		private const string _urlPrefix = "URL:";
		private const string _csPrefix = "C#:";
		private const string _separator = "@";
		
		public ButtonDrawer() { }
			
		public ButtonDrawer(string group)
		{
			this.group = group;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) => 24;

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.groupName = group;

			// Display Name: ButtonName@URL:XXX@ButtonName@CS:NameSpace.Class.Method(arg0, arg1, ...)@...
			var buttonNameAndCommands = inProp.displayName.Split(_separator);
			if (buttonNameAndCommands != null && buttonNameAndCommands.Length > 0 && buttonNameAndCommands.Length % 2 == 0)
			{
				for (int i = 0; i < buttonNameAndCommands.Length; i++)
				{
					if (i % 2 == 0)
					{
						inoutPropertyStaticData.buttonDisplayNames.Add(buttonNameAndCommands[i]);
						inoutPropertyStaticData.buttonDisplayNameWidths.Add(EditorStyles.label.CalcSize(new GUIContent(buttonNameAndCommands[i])).x);
					}
					else
					{
						inoutPropertyStaticData.buttonCommands.Add(buttonNameAndCommands[i]);
					}
				}
			}
			else
			{
				Debug.LogError($"LWGUI: ButtonDrawer with invalid Display Name Commands: { buttonNameAndCommands } ! prop: { inProp.name }");
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var buttonDisplayNames = metaDatas.GetPropStaticData(prop).buttonDisplayNames;
			var buttonDisplayNameWidths = metaDatas.GetPropStaticData(prop).buttonDisplayNameWidths;
			var buttonCommands = metaDatas.GetPropStaticData(prop).buttonCommands;
			if (buttonDisplayNames == null || buttonCommands == null || buttonDisplayNames.Count == 0 || buttonCommands.Count == 0 
			    || buttonDisplayNames.Count != buttonCommands.Count)
			{
				return;
			}

			var enbaled = GUI.enabled;
			GUI.enabled = true;
			
			position = EditorGUI.IndentedRect(position);
			var rect = new Rect(position.x, position.y, 0, position.height);
			var spaceWidth = (position.width - buttonDisplayNameWidths.Sum()) / buttonDisplayNames.Count;

			for (int i = 0; i < buttonDisplayNames.Count; i++)
			{
				var displayName = buttonDisplayNames[i];
				var displayNameRelativeWidth = buttonDisplayNameWidths[i];
				var command = buttonCommands[i];
				rect.xMax = rect.xMin + displayNameRelativeWidth + spaceWidth;

				if (GUI.Button(rect, new GUIContent(displayName, command)))
				{
					if (command.StartsWith(_urlPrefix))
					{
						Application.OpenURL(command.Substring(_urlPrefix.Length, command.Length - _urlPrefix.Length));
					}
					else if (command.StartsWith(_csPrefix))
					{
						var csCommand = command.Substring(_csPrefix.Length, command.Length - _csPrefix.Length);
						
						// Get method name and args
						string className = null, methodName = null;
						string[] args = null;
						{
							var lastPointIndex = csCommand.LastIndexOf('.');
							if (lastPointIndex != -1)
							{
								className = csCommand.Substring(0, lastPointIndex);
								var leftBracketIndex = csCommand.IndexOf('(');
								if (leftBracketIndex != -1)
								{
									methodName = csCommand.Substring(lastPointIndex + 1, leftBracketIndex - lastPointIndex - 1);
									args = csCommand.Substring(leftBracketIndex + 1, csCommand.Length - leftBracketIndex - 2)
										?.Split(',').Select(s => s.TrimStart()).ToArray();
								}
							}
						}

						// Find and call method
						if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(methodName) && args != null)
						{
							Type type = ReflectionHelper.GetAllTypes().FirstOrDefault((type1 => type1.Name == className || type1.FullName == className));
							if (type != null)
							{
								var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
								if (methodInfo != null)
								{
									methodInfo.Invoke(null, new object[]{ prop, editor, metaDatas, args });
								}
								else
								{
									Debug.LogError($"LWGUI: Method {methodName} not found in {className}");
								}
							}
							else
							{
								Debug.LogError($"LWGUI: Class {className} not found");
							}
						}
						else
						{
							Debug.LogError($"LWGUI: Invalid C# command: {csCommand}");
						}
					}
					else
					{
						Debug.LogError($"LWGUI: Unknown command type: {command}");
					}
				}

				rect.xMin = rect.xMax;
			}

			GUI.enabled = enbaled;
		}

		public static void TestMethod(MaterialProperty prop, MaterialEditor editor, LWGUIMetaDatas metaDatas, string[] args)
		{
			Debug.Log($"LWGUI: ButtonDrawer.TestMethod({prop}, {editor}, {metaDatas}, {args})");
			
			foreach (var arg in args)
			{
				Debug.Log(arg);
			}
		}
	}
	#endregion

	#endregion

	#region Extra Decorators

	#region Appearance
	/// <summary>
	/// Similar to Header()
	/// 
	/// group: parent group name (Default: none)
	/// header: string to display, "SpaceLine" or "_" = none (Default: none)
	/// height: line height (Default: 22)
	/// </summary>
	public class TitleDecorator : SubDrawer
	{
		private string _header;
		private float  _height;

		public static readonly float DefaultHeight = EditorGUIUtility.singleLineHeight + 6f;

		protected override float GetVisibleHeight(MaterialProperty prop) { return _height; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData) { }

		public TitleDecorator(string header) : this("_", header, DefaultHeight) { }

		public TitleDecorator(string header, float height) : this("_", header, height) { }

		public TitleDecorator(string group, string header) : this(group, header, DefaultHeight) { }

		public TitleDecorator(string group, string header, float height)
		{
			this.group = group;
			this._header = header == "SpaceLine" || header == "_" ? String.Empty : header;
			this._height = height;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			position = EditorGUI.IndentedRect(position);
			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			style.alignment = TextAnchor.LowerLeft;
			style.border.bottom = 2;
			GUI.Label(position, _header, style);
		}
	}

	/// <summary>
	/// Similar to Title()
	/// 
	/// group: parent group name (Default: none)
	/// header: string to display, "SpaceLine" or "_" = none (Default: none)
	/// height: line height (Default: 22)
	/// </summary>
	public class SubTitleDecorator : TitleDecorator
	{
		public SubTitleDecorator(string group, string header) : base(group, header, DefaultHeight) { }

		public SubTitleDecorator(string group, string header, float height) : base(group, header, height) { }
	}

	/// <summary>
	/// Tooltip, describes the details of the property. (Default: property.name and property default value)
	/// You can also use "#Text" in DisplayName to add Tooltip that supports Multi-Language.
	/// 
	/// tooltip: a single-line string to display, support up to 4 ','. (Default: Newline)
	/// </summary>
	public class TooltipDecorator : SubDrawer
	{
		private string _tooltip;


		#region

		public TooltipDecorator() : this(string.Empty) { }

		public TooltipDecorator(string tooltip) { this._tooltip = tooltip; }

		public TooltipDecorator(string s1, string s2) : this(s1 + ", " + s2) { }

		public TooltipDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }

		public TooltipDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }

		public TooltipDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }

		#endregion


		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.tooltipMessages += _tooltip + "\n";
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}

	/// <summary>
	/// Display a Helpbox on the property
	/// You can also use "%Text" in DisplayName to add Helpbox that supports Multi-Language.
	/// 
	/// message: a single-line string to display, support up to 4 ','. (Default: Newline)
	/// </summary>
	public class HelpboxDecorator : TooltipDecorator
	{
		private string _message;


		#region

		public HelpboxDecorator() : this(string.Empty) { }

		public HelpboxDecorator(string message) { this._message = message; }

		public HelpboxDecorator(string s1, string s2) : this(s1 + ", " + s2) { }

		public HelpboxDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }

		public HelpboxDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }

		public HelpboxDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }

		#endregion


		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.helpboxMessages += _message + "\n";
		}
	}
	
	/// <summary>
	/// Set the property to read-only.
	/// </summary>
	public class ReadOnlyDecorator : SubDrawer
	{
		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.isReadOnly = true;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
	#endregion

	#region Logic
	/// <summary>
	/// Cooperate with Toggle to switch certain Passes.
	/// 
	/// lightModeName(s): Light Mode in Shader Pass (https://docs.unity3d.com/2017.4/Documentation/Manual/SL-PassTags.html)
	/// </summary>
	public class PassSwitchDecorator : SubDrawer
	{
		private string[] _lightModeNames;


		#region

		public PassSwitchDecorator(string lightModeName1)
			: this(new[] { lightModeName1 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2)
			: this(new[] { lightModeName1, lightModeName2 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2, string lightModeName3)
			: this(new[] { lightModeName1, lightModeName2, lightModeName3 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4)
			: this(new[] { lightModeName1, lightModeName2, lightModeName3, lightModeName4 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4, string lightModeName5)
			: this(new[] { lightModeName1, lightModeName2, lightModeName3, lightModeName4, lightModeName5 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4, string lightModeName5, string lightModeName6)
			: this(new[] { lightModeName1, lightModeName2, lightModeName3, lightModeName4, lightModeName5, lightModeName6 }) { }

		public PassSwitchDecorator(string[] lightModeNames) { _lightModeNames = lightModeNames.Select((s => s.ToUpper())).ToArray(); }

		#endregion


		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		protected override bool IsMatchPropType(MaterialProperty property)
		{
			return property.GetPropertyType() == ShaderPropertyType.Float
				|| property.GetPropertyType() == ShaderPropertyType.Int;
		}

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData) { }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
				Helper.SetShaderPassEnabled(prop.targets, _lightModeNames, prop.floatValue > 0);
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
			{
				if (ShowIfDecorator.GetShowIfResultToFilterDrawerApplying(prop))
					Helper.SetShaderPassEnabled(prop.targets, _lightModeNames, prop.floatValue > 0);
			}
		}
	}
	#endregion

	#region Structure
	/// <summary>
	/// Collapse the current Property into an Advanced Block.
	/// Specify the Header String to create a new Advanced Block.
	/// All Properties using Advanced() will be collapsed into the nearest Advanced Block.
	/// 
	/// headerString: The title of the Advanced Block. Default: "Advanced"
	/// </summary>
	public class AdvancedDecorator : SubDrawer
	{
		private string headerString;

		public AdvancedDecorator() : this(string.Empty) { }

		public AdvancedDecorator(string headerString)
		{
			this.headerString = headerString;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.isAdvanced = true;
			inoutPropertyStaticData.advancedHeaderString = headerString;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}

	/// <summary>
	/// Create an Advanced Block using the current Property as the Header.
	/// </summary>
	public class AdvancedHeaderPropertyDecorator : SubDrawer
	{
		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.isAdvanced = true;
			inoutPropertyStaticData.isAdvancedHeader = true;
			inoutPropertyStaticData.isAdvancedHeaderProperty = true;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
	#endregion

	#region Condition Display
	/// <summary>
	/// Similar to HideInInspector(), the difference is that Hidden() can be unhidden through the Display Mode button.
	/// </summary>
	public class HiddenDecorator : SubDrawer
	{
		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.isHidden = true;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}

	/// <summary>
	/// Control the show or hide of a single or a group of properties based on multiple conditions.
	/// 
	/// logicalOperator: And | Or (Default: And).
	/// propName: Target Property Name used for comparison.
	/// compareFunction: Less (L) | Equal (E) | LessEqual (LEqual / LE) | Greater (G) | NotEqual (NEqual / NE) | GreaterEqual (GEqual / GE).
	/// value: Target Property Value used for comparison.
	/// </summary>
	public class ShowIfDecorator : SubDrawer
	{
		public enum LogicalOperator
		{
			And,
			Or
		}

		public class ShowIfData
		{
			public LogicalOperator logicalOperator    = LogicalOperator.And;
			public string          targetPropertyName = string.Empty;
			public CompareFunction compareFunction    = CompareFunction.Equal;
			public float           value              = 0;
		}

		public ShowIfData showIfData = new();
		
		private readonly Dictionary<string, string> _compareFunctionLUT = new()
		{
			{ "Less", "Less" },
			{ "L", "Less" },
			{ "Equal", "Equal" },
			{ "E", "Equal" },
			{ "LessEqual", "LessEqual" },
			{ "LEqual", "LessEqual" },
			{ "LE", "LessEqual" },
			{ "Greater", "Greater" },
			{ "G", "Greater" },
			{ "NotEqual", "NotEqual" },
			{ "NEqual", "NotEqual" },
			{ "NE", "NotEqual" },
			{ "GreaterEqual", "GreaterEqual" },
			{ "GEqual", "GreaterEqual" },
			{ "GE", "GreaterEqual" },
		};

		public ShowIfDecorator(string propName, string comparisonMethod, float value) : this("And", propName, comparisonMethod, value) { }

		public ShowIfDecorator(string logicalOperator, string propName, string compareFunction, float value)
		{
			showIfData.logicalOperator = logicalOperator.ToLower() == "or" ? LogicalOperator.Or : LogicalOperator.And;
			showIfData.targetPropertyName = propName;
			if (!_compareFunctionLUT.ContainsKey(compareFunction) || !Enum.IsDefined(typeof(CompareFunction), _compareFunctionLUT[compareFunction]))
				Debug.LogError("LWGUI: Invalid compareFunction: '"
							 + compareFunction
							 + "', Must be one of the following: Less (L) | Equal (E) | LessEqual (LEqual / LE) | Greater (G) | NotEqual (NEqual / NE) | GreaterEqual (GEqual / GE).");
			else
				showIfData.compareFunction = (CompareFunction)Enum.Parse(typeof(CompareFunction), _compareFunctionLUT[compareFunction]);
			showIfData.value = value;
		}

		private static void Compare(ShowIfData showIfData, float targetValue, ref bool result)
		{
			bool compareResult;

			switch (showIfData.compareFunction)
			{
				case CompareFunction.Less:
					compareResult = targetValue < showIfData.value;
					break;
				case CompareFunction.LessEqual:
					compareResult = targetValue <= showIfData.value;
					break;
				case CompareFunction.Greater:
					compareResult = targetValue > showIfData.value;
					break;
				case CompareFunction.NotEqual:
					compareResult = targetValue != showIfData.value;
					break;
				case CompareFunction.GreaterEqual:
					compareResult = targetValue >= showIfData.value;
					break;
				default:
					compareResult = targetValue == showIfData.value;
					break;
			}

			switch (showIfData.logicalOperator)
			{
				case LogicalOperator.And:
					result &= compareResult;
					break;
				case LogicalOperator.Or:
					result |= compareResult;
					break;
			}
		}

		public static bool GetShowIfResultToFilterDrawerApplying(MaterialProperty prop)
		{
			var material = prop.targets[0] as Material;
			var showIfDatas = new List<ShowIfData>();
			{
				var drawer = ReflectionHelper.GetPropertyDrawer(material.shader, prop, out var decoratorDrawers);
				if (decoratorDrawers != null && decoratorDrawers.Count > 0)
				{
					foreach (ShowIfDecorator showIfDecorator in decoratorDrawers.Where(drawer => drawer is ShowIfDecorator))
					{
						showIfDatas.Add(showIfDecorator.showIfData);
					}
				}
				else
				{
					return true;
				}
			}

			return GetShowIfResultFromMaterial(showIfDatas, material);
		}
		
		public static bool GetShowIfResultFromMaterial(List<ShowIfData> showIfDatas, Material material)
		{
			bool result = true;
			foreach (var showIfData in showIfDatas)
			{
				var targetValue = material.GetFloat(showIfData.targetPropertyName);
				Compare(showIfData, targetValue, ref result);
			}

			return result;
		}
		
		public static void GetShowIfResult(PropertyStaticData propStaticData, PropertyDynamicData propDynamicData, PerMaterialData perMaterialData)
		{
			foreach (var showIfData in propStaticData.showIfDatas)
			{
				var targetValue = perMaterialData.propDynamicDatas[showIfData.targetPropertyName].property.floatValue;
				Compare(showIfData, targetValue, ref propDynamicData.isShowing);
			}
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.showIfDatas.Add(showIfData);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
	#endregion

	#endregion

}