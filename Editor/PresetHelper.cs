using System.Collections.Generic;
using System.Linq;
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
				// Debug.Log(preset.name);
			}
		}

		public static ShaderPropertyPreset GetPresetFile(string presetFileName)
		{
			if (!_loadedPresets.ContainsKey(presetFileName) || !_loadedPresets[presetFileName])
				ForceInit();

			if (!_loadedPresets.ContainsKey(presetFileName) || !_loadedPresets[presetFileName])
			{
				Debug.LogError("Invalid ShaderPropertyPreset: ‘" + presetFileName + "’ !");
				return null;
			}

			return _loadedPresets[presetFileName];
		}

		public static void ApplyPresetValue(MaterialProperty[] props, Material material)
		{
			Init();
			List<MaterialPropertyDrawer> _drawers = new List<MaterialPropertyDrawer>();
			List<MaterialProperty> _properties = new List<MaterialProperty>();
			// Update Value
			foreach (var prop in props)
			{
				var drawer = ReflectionHelper.GetPropertyDrawer(material.shader, prop);
				if (drawer != null)
				{
					_drawers.Add(drawer);
					_properties.Add(prop);
					if (drawer is PresetDrawer)
					{
						var preset = GetPresetFile((drawer as PresetDrawer).presetFileName);
						if (preset)
							preset.Apply(material, (int)prop.floatValue);
					}
				}
			}

			// Update Keyword
			for (int i = 0; i < _properties.Count; i++)
			{
				_drawers[i].Apply(_properties[i]);
			}
		}

		private enum PresetOperation
		{
			Add    = 0,
			Update = 1,
			Remove = 2
		}
		
		private class MenuItemData
		{
			public PresetOperation                    operation;
			public ShaderPropertyPreset               presetFile;
			public ShaderPropertyPreset.Preset        preset;
			public ShaderPropertyPreset.PropertyValue propertyValue;
		}

		public static void DrawAddPropertyToPresetMenu(Rect rect, Shader shader, MaterialProperty prop, MaterialProperty[] props)
		{
			if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
			{
				// Get Menu Content
				var propDisplayName = MetaDataHelper.GetPropertyDisplayName(shader, prop);
				var propPresetDic = MetaDataHelper.GetAllPropertyPreset(shader, props);
				if (propPresetDic.Count == 0) return;

				// Create Menus
				var menuItemDatas = new Dictionary<GUIContent, MenuItemData>(); 
				GUIContent[] menus = propPresetDic.SelectMany(((keyValuePair, i) =>
				{
					if (prop.name == keyValuePair.Key.name) return new List<GUIContent>();

					var preset = keyValuePair.Value.GetPreset(keyValuePair.Key);
					var propertyValue = preset.propertyValues.Find((value => value.propertyName == prop.name));
					if (propertyValue == null)
					{
						var content = new GUIContent("Add '" + propDisplayName + "' to '" + preset.presetName + "'");
						menuItemDatas.Add(content, new MenuItemData(){operation = PresetOperation.Add, presetFile = keyValuePair.Value, preset = preset, propertyValue = propertyValue});
						return new List<GUIContent>() { content };
					}
					else
					{
						var contentUpdate = new GUIContent("Update '" + propDisplayName + "' in '" + preset.presetName + "'");
						menuItemDatas.Add(contentUpdate, new MenuItemData(){operation = PresetOperation.Update, presetFile = keyValuePair.Value, preset = preset, propertyValue = propertyValue});
						var contentRemove = new GUIContent("Remove '" + propDisplayName + "' from '" + preset.presetName + "'");
						menuItemDatas.Add(contentRemove, new MenuItemData(){operation = PresetOperation.Remove, presetFile = keyValuePair.Value, preset = preset, propertyValue = propertyValue});
						return new List<GUIContent>() { contentUpdate, contentRemove };
					}
				})).ToArray();

				EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0),
												// Call Click Event
												menus, -1, (data, options, selected) =>
												{
													var menu = menus[selected];
													var operation = menuItemDatas[menu].operation;
													var preset = menuItemDatas[menu].preset;
													var propertyValue = menuItemDatas[menu].propertyValue;
													if (operation == PresetOperation.Add)
													{
														propertyValue = new ShaderPropertyPreset.PropertyValue();
														propertyValue.CopyFromMaterialProperty(prop);
														preset.propertyValues.Add(propertyValue);
													}
													else if (operation == PresetOperation.Update)
													{
														propertyValue.CopyFromMaterialProperty(prop);
													}
													else
													{
														preset.propertyValues.Remove(propertyValue);
													}
													EditorUtility.SetDirty(menuItemDatas[menu].presetFile);
													RevertableHelper.ForceInit();
												}, null);
			}
		}
	}
}