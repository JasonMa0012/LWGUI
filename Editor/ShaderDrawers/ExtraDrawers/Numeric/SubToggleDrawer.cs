// Copyright (c) Jason Ma

using System;
using LWGUI.Timeline;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Similar to builtin Toggle()
	/// 
	/// group: parent group name (Default: none)
	/// keyword: keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
	/// preset File Name: "Shader Property Preset" asset name, see Preset() for detail (Default: none)
	/// Target Property Type: Float
	/// </summary>
	[LwguiDrawerCategory("Numeric")]
	[LwguiDrawerParameterString("group", "", "Empty")]
	[LwguiDrawerParameterKeyword("keyWord", "", "Empty")]
	[LwguiDrawerParameterString("presetFileName", "", "Empty")]
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

		public override bool IsMatchPropType(ShaderPropertyType propType)
		{
			return propType == ShaderPropertyType.Float;
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
				PresetHelper.GetPresetAsset(_presetFileName)?.TryGetPreset(prop.floatValue)?.ApplyToEditingMaterial(editor, metaDatas.perMaterialData);
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
}

