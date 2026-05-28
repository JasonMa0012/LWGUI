// Copyright (c) Jason Ma

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Popping a menu, you can select the Shader Property Preset, the Preset values will replaces the default values
	/// 
	/// group: parent group name (Default: none)
	///	presetFileName: "Shader Property Preset" asset name, you can create new Preset by
	///		"Right Click > Create > LWGUI > Shader Property Preset" in Project window,
	///		*any Preset in the entire project cannot have the same name*
	/// Target Property Type: Float, express current keyword index
	/// </summary>
	[LwguiDrawerCategory("Enum", 10)]
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

		// Apply Keywords and Passes in presets without modifying other property values
		// Used to call in MaterialPropertyDrawer.Apply()
		public static void ApplyPresetWithoutPropertyChanges(string presetFileName, MaterialProperty prop)
		{
			var presetFile = PresetHelper.GetPresetAsset(presetFileName);
			if (presetFile && ShowIfDecorator.GetShowIfResultToFilterDrawerApplying(prop))
			{
				presetFile.TryGetPreset(prop.floatValue)?.ApplyKeywordsAndPassesToMaterials(prop.targets);
			}
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) { return propType == ShaderPropertyType.Float; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			SetPresetAssetToStaticData(inoutPropertyStaticData, presetFileName);
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			var propertyPreset = inPerShaderData.propStaticDatas[inProp.name].propertyPresetAsset;
			if (propertyPreset)
				inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription = propertyPreset.TryGetPreset(inDefaultProp.floatValue)?.presetName;
		}

		public string GetPresetFileName() => presetFileName;

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			int index = (int)Mathf.Max(0, prop.floatValue);
			var presetFile = PresetHelper.GetPresetAsset(presetFileName);
			if (!presetFile || presetFile.GetPresetCount() == 0)
			{
				Helper.DrawShaderPropertyWithErrorLabel(position, prop, label, editor, $"Invalid Preset File: {presetFileName}. Try reimporting LWGUI.");
				return;
			}

			if (index < presetFile.GetPresetCount())
			{
				var presetNames = presetFile.GetPresets().Select((inPreset) => new GUIContent(inPreset.presetName)).ToArray();
				if (EditorGUI.showMixedValue)
					index = -1;
				int newIndex = EditorGUI.Popup(position, label, index, presetNames);
				if (Helper.EndChangeCheck(metaDatas, prop))
				{
					prop.floatValue = newIndex;
					presetFile.TryGetPreset(newIndex)?.ApplyToEditingMaterial(editor, metaDatas.perMaterialData);
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
}

