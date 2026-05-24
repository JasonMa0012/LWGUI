// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Similar to builtin PowerSlider()
	/// 
	/// group: parent group name (Default: none)
	/// power: power of slider (Default: 1)
	///	presetFileName: "Shader Property Preset" asset name, it rounds up the float to choose which Preset to use.
	///		You can create new Preset by
	///		"Right Click > Create > LWGUI > Shader Property Preset" in Project window,
	///		*any Preset in the entire project cannot have the same name*
	/// Target Property Type: Range
	/// </summary>
	[LwguiDrawerCategory("Numeric")]
	[LwguiDrawerParameterString("group", "")]
	[LwguiDrawerParameterFloat("power", 1f)]
	[LwguiDrawerParameterString("presetFileName", "")]
	public class SubPowerSliderDrawer : SubDrawer, IPresetDrawer
	{
		public string presetFileName;
		
		private float _power = 1;

		public SubPowerSliderDrawer(float power) : this("_", power) { }
		
		public SubPowerSliderDrawer(string group, float power) : this(group, power, string.Empty) { }

		public SubPowerSliderDrawer(string group, float power, string presetFileName)
		{
			this.group = group;
			this._power = Mathf.Clamp(power, 0, float.MaxValue);
			this.presetFileName = presetFileName;
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) { return propType == ShaderPropertyType.Range; }
		
		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			PresetDrawer.SetPresetAssetToStaticData(inoutPropertyStaticData, presetFileName);
		}

		public string GetPresetFileName() => presetFileName;

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position;
			var oldValue = prop.floatValue;
			ReflectionHelper.DoPowerRangeProperty(rect, prop, label, _power);
			if (prop.floatValue != oldValue)
			{
				PresetHelper.GetPresetAsset(presetFileName)?.TryGetPreset(prop.floatValue)?.ApplyToEditingMaterial(editor, metaDatas.perMaterialData);
			}
			EditorGUI.showMixedValue = false;
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
			{
				PresetDrawer.ApplyPresetWithoutPropertyChanges(presetFileName, prop);
			}
		}
	}
}

