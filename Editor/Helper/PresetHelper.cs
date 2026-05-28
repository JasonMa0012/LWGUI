// Copyright (c) Jason Ma
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	public class PresetHelper
	{
		private static Dictionary<string /*FileName*/, LwguiShaderPropertyPreset> _loadedPresets = new Dictionary<string, LwguiShaderPropertyPreset>();

		private static bool _isInitComplete;

		public static bool IsInitComplete { get { return _isInitComplete; } }

		public static void Init()
		{
			if (!_isInitComplete)
			{
				ForceInit();
			}
		}

		public static void ForceInit()
		{
			_loadedPresets.Clear();
			_isInitComplete = false;
			var GUIDs = AssetDatabase.FindAssets("t:" + typeof(LwguiShaderPropertyPreset));
			foreach (var GUID in GUIDs)
			{
				var preset = AssetDatabase.LoadAssetAtPath<LwguiShaderPropertyPreset>(AssetDatabase.GUIDToAssetPath(GUID));
				AddPreset(preset);
			}

			if (GUIDs.Length == 0)
				Debug.LogWarning("LWGUI: No ShaderPropertyPreset found! If you have created presets, try reimporting LWGUI.");

			_isInitComplete = true;
		}

		public static void AddPreset(LwguiShaderPropertyPreset preset)
		{
			if (!preset) return;
			if (!_loadedPresets.ContainsKey(preset.name))
			{
				_loadedPresets.Add(preset.name, preset);
			}
		}

		public static LwguiShaderPropertyPreset GetPresetAsset(string presetFileName)
		{
			if (string.IsNullOrEmpty(presetFileName))
				return null;
			
			if (!_loadedPresets.ContainsKey(presetFileName) || !_loadedPresets[presetFileName])
				ForceInit();

			if (!_loadedPresets.ContainsKey(presetFileName) || !_loadedPresets[presetFileName])
			{
				if (!BuildPipeline.isBuildingPlayer)
					Debug.LogError("LWGUI: Invalid ShaderPropertyPreset path: ‘" + presetFileName + "’ ! If the preset file exists, try reimporting LWGUI.");
				return null;
			}

			return _loadedPresets[presetFileName];
		}

		// For Developers: Call this function after creating a material,
		// This applies all active presets and may modify some other properties.
		// Usually called after the material is created, otherwise the material default value will not contain the results of Preset Drawers.
		// If you only want to apply Keywords without modifying other properties, call UnityEditorExtension.ApplyMaterialPropertyAndDecoratorDrawers()
		public static void ApplyPresetsInMaterial(Material material)
		{
			var props = MaterialEditor.GetMaterialProperties(new UnityEngine.Object[] { material });
			foreach (var prop in props)
			{
				var drawer = ReflectionHelper.GetPropertyDrawer(material.shader, prop, out _);

				// Apply active preset
				if (drawer is IPresetDrawer presetDrawer)
				{
					var activePreset = presetDrawer.GetActivePreset(prop, GetPresetAsset(presetDrawer.GetPresetFileName()));
					activePreset?.ApplyToDefaultMaterial(material);
				}
			}
			UnityEditorExtension.ApplyMaterialPropertyAndDecoratorDrawers(material);
		}
	}
}