// Copyright (c) Jason Ma

using System;
using LWGUI.Timeline;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
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
	[LwguiDrawerCategory("Base", -100)]
	[LwguiDrawerParameterString("group", "", "Empty")]
	[LwguiDrawerParameterKeyword("keyword", "", "Empty")]
	[LwguiDrawerParameterEnum("defaultFoldingState", 1, "on", "off")]
	[LwguiDrawerParameterEnum("defaultToggleDisplayed", 0, "on", "off")]
	[LwguiDrawerParameterString("presetFileName", "", "Empty")]
	public class MainDrawer : MaterialPropertyDrawer, IBaseDrawer, IPresetDrawer
	{
		protected LWGUIMetaDatas metaDatas;

		private static readonly float  _height = 28f;
		private static readonly float _helpURLButtonWidth = 18f;
		
		private static GUIContent _helpURLIconCache;
		private static GUIContent _helpURLIcon => _helpURLIconCache ??= new GUIContent(EditorGUIUtility.IconContent("_Help")) { tooltip = string.Empty };
		
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

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			metaDatas = Helper.GetLWGUIMetadatas(editor);
			var propStaticData = metaDatas.GetPropStaticData(prop);

			var showMixedValue = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = prop.hasMixedValue;
			EditorGUI.BeginChangeCheck();

			bool toggleResult = DrawFoldout(position, ref propStaticData.isExpanding, !Helper.Approximately(prop.floatValue, 0), _defaultToggleDisplayed, label, propStaticData.helpURL);

			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				prop.floatValue = toggleResult ? 1.0f : 0.0f;
				var keyword = Helper.GetKeywordName(_keyword, prop.name);
				Helper.SetShaderKeywordEnabled(editor.targets, keyword, toggleResult);
				PresetHelper.GetPresetAsset(_presetFileName)?.TryGetPreset(prop.floatValue)?.ApplyToEditingMaterial(editor, metaDatas.perMaterialData);
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
		
		public static bool DrawFoldout(Rect rect, ref bool isFolding, bool toggleValue, bool hasToggle, GUIContent label, string helpURL)
		{
			var toggleRect = new Rect(rect.x + 8f, rect.y + 7f, 13f, 13f);
			
			bool hasHelpURL = !string.IsNullOrEmpty(helpURL);
			var helpButtonRect = !hasHelpURL ? Rect.zero : new Rect(rect.xMax - _helpURLButtonWidth - 4f,
				rect.yMax - (EditorGUIUtility.singleLineHeight + _helpURLButtonWidth) * 0.5f - 4.5f,
				_helpURLButtonWidth,
				_helpURLButtonWidth);

			// Toggle Event
			if (hasToggle)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && toggleRect.Contains(Event.current.mousePosition))
				{
					toggleValue = !toggleValue;
					Event.current.Use();
					GUI.changed = true;
				}
			}
			
			// Help URL Event
			if (hasHelpURL)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && helpButtonRect.Contains(Event.current.mousePosition))
				{
					Application.OpenURL(helpURL);
					Event.current.Use();
				}
			}

			// Button
			{
				// Cancel Right Click
				if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
					Event.current.Use();

				var enabled = GUI.enabled;
				GUI.enabled = true;
				var guiColor = GUI.backgroundColor;
				GUI.backgroundColor = isFolding ? Color.white : new Color(0.85f, 0.85f, 0.85f);
				
				if (GUI.Button(rect, label, GUIStyles.foldout))
				{
					isFolding = !isFolding;
					GUI.changed = false;
				}
				
				GUI.backgroundColor = guiColor;
				
				// Help URL Icon
				GUI.Button(helpButtonRect, _helpURLIcon, GUIStyles.iconButton);
				
				GUI.enabled = enabled;
			}

			// Toggle Icon
			if (hasToggle)
			{
				EditorGUI.Toggle(toggleRect, string.Empty, toggleValue);
			}

			return toggleValue;
		}
	}
}

