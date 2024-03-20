// Copyright (c) Jason Ma
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	public class PresetHelper
	{
		private static Dictionary<string /*FileName*/, ShaderPropertyPreset> _loadedPresets = new Dictionary<string, ShaderPropertyPreset>();

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
			var GUIDs = AssetDatabase.FindAssets("t:" + typeof(ShaderPropertyPreset));
			foreach (var GUID in GUIDs)
			{
				var preset = AssetDatabase.LoadAssetAtPath<ShaderPropertyPreset>(AssetDatabase.GUIDToAssetPath(GUID));
				AddPreset(preset);
			}
			_isInitComplete = true;
		}

		public static void AddPreset(ShaderPropertyPreset preset)
		{
			if (!preset) return;
			if (!_loadedPresets.ContainsKey(preset.name))
			{
				_loadedPresets.Add(preset.name, preset);
			}
		}

		public static ShaderPropertyPreset GetPresetFile(string presetFileName)
		{
			if (!_loadedPresets.ContainsKey(presetFileName) || !_loadedPresets[presetFileName])
				ForceInit();

			if (!_loadedPresets.ContainsKey(presetFileName) || !_loadedPresets[presetFileName])
			{
				Debug.LogError("LWGUI: Invalid ShaderPropertyPreset: ‘" + presetFileName + "’ !");
				return null;
			}

			return _loadedPresets[presetFileName];
		}

		// For Developers: Call this after a material has modified in code
		public static void ApplyPresetsInMaterial(Material material)
		{
			var props = MaterialEditor.GetMaterialProperties(new[] { material });
			foreach (var prop in props)
			{
				List<MaterialPropertyDrawer> decoratorDrawers;
				var drawer = ReflectionHelper.GetPropertyDrawer(material.shader, prop, out decoratorDrawers);

				// Apply active preset
				if (drawer != null && drawer is IBasePresetDrawer)
				{
					var activePreset = (drawer as IBasePresetDrawer).GetActivePreset(prop, PresetHelper.GetPresetFile((drawer as PresetDrawer).presetFileName));
					if (activePreset != null)
						activePreset.ApplyToDefaultMaterial(material);
				}

			}
			MaterialEditor.ApplyMaterialPropertyDrawers(material);
		}
	}
}