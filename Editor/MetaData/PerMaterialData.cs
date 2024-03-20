// Copyright (c) Jason Ma
// Per Shader > Per Material > Per Inspector

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	public class PersetDynamicData
	{
		public ShaderPropertyPreset.Preset preset;
		public MaterialProperty            property;

		public PersetDynamicData(ShaderPropertyPreset.Preset preset, MaterialProperty property)
		{
			this.preset = preset;
			this.property = property;
		}
	}

	public class PropertyDynamicData
	{
		public MaterialProperty property;
		public MaterialProperty defualtProperty; // Default values may be overridden by Preset

		public string defaultValueDescription = string.Empty; // Description of the default values used in Tooltip
		public bool   hasModified             = false;        // Are properties modified in the material?
		public bool   hasChildrenModified     = false;        // Are Children properties modified in the material?
		public bool   hasRevertChanged        = false;        // Used to call property EndChangeCheck()
		public bool   isShowing               = true;         // ShowIf() result
	}

	/// <summary>
	/// Contains Metadata that may be different for each Material.
	/// </summary>
	public class PerMaterialData
	{
		public Dictionary<string, PropertyDynamicData> propDynamicDatas         = new Dictionary<string, PropertyDynamicData>();
		public MaterialProperty[]                      props                    = null;
		public Material                                material                 = null;
		public List<PersetDynamicData>                 activePresetDatas        = new List<PersetDynamicData>();
		public int                                     modifiedCount            = 0;
		public Dictionary<string, bool>                cachedModifiedProperties = null;
		public bool                                    forceInit              = true;

		public PerMaterialData(Shader shader, Material material, MaterialProperty[] props, PerShaderData perShaderData)
		{
			Init(shader, material, props, perShaderData);
		}

		public void Init(Shader shader, Material material, MaterialProperty[] props, PerShaderData perShaderData)
		{
			// Reset Datas
			this.props = props;
			this.material = material;
			activePresetDatas.Clear();
			propDynamicDatas.Clear();
			modifiedCount = 0;

			// Get active presets
			foreach (var prop in props)
			{
				var activePreset = perShaderData.propStaticDatas[prop.name].presetDrawer?.GetActivePreset(prop, perShaderData.propStaticDatas[prop.name].propertyPresetAsset);
				if (activePreset != null) activePresetDatas.Add(new PersetDynamicData(activePreset, prop));
			}

			{
				// Apply presets to default material
				var defaultMaterial = UnityEngine.Object.Instantiate(
#if UNITY_2022_1_OR_NEWER
																	 material.parent
																		 ? material.parent
																		 :
#endif
																		 perShaderData.defaultMaterial
																	);

				foreach (var activePresetData in activePresetDatas)
					activePresetData.preset.ApplyToDefaultMaterial(defaultMaterial);

				var defaultProperties = MaterialEditor.GetMaterialProperties(new[] { defaultMaterial });
				Debug.Assert(defaultProperties.Length == props.Length);

				// Init propDynamicDatas
				for (int i = 0; i < props.Length; i++)
				{
					Debug.Assert(props[i].name == defaultProperties[i].name);
					Debug.Assert(!propDynamicDatas.ContainsKey(props[i].name));

					var hasModified = !Helper.PropertyValueEquals(props[i], defaultProperties[i]);
					if (hasModified) modifiedCount++;
					propDynamicDatas.Add(props[i].name, new PropertyDynamicData()
					{
						property = props[i],
						defualtProperty = defaultProperties[i],
						hasModified = hasModified
					});
				}

				// Collect modification
				foreach (var prop in props)
				{
					var propStaticData = perShaderData.propStaticDatas[prop.name];
					var propDynamicData = propDynamicDatas[prop.name];

					// Extra Prop hasModified
					foreach (var extraPropName in propStaticData.extraPropNames)
						propDynamicData.hasModified |= propDynamicDatas[extraPropName].hasModified;

					// Override parent hasChildrenModified
					if (propDynamicData.hasModified)
					{
						var parentPropData = propStaticData.parent;
						if (parentPropData != null)
						{
							propDynamicDatas[parentPropData.name].hasChildrenModified = true;
							if (parentPropData.parent != null)
								propDynamicDatas[parentPropData.parent.name].hasChildrenModified = true;
						}
					}
				}
			}

			// Store Show Modified Props Only Cache
			{
				if (perShaderData.displayModeData.showOnlyModifiedGroups || perShaderData.displayModeData.showOnlyModifiedProperties)
				{
					if (cachedModifiedProperties == null)
					{
						cachedModifiedProperties = new Dictionary<string, bool>();
						foreach (var propDynamicDataKWPair in propDynamicDatas)
						{
							if (propDynamicDataKWPair.Value.hasModified || propDynamicDataKWPair.Value.hasChildrenModified)
								cachedModifiedProperties.Add(propDynamicDataKWPair.Key, true);
						}
					}
				}
				else
					cachedModifiedProperties = null;
			}

			foreach (var prop in props)
			{
				var propStaticData = perShaderData.propStaticDatas[prop.name];
				var propDynamicData = propDynamicDatas[prop.name];

				// Get default value descriptions
				propStaticData.baseDrawers?.ForEach(propertyDrawer => propertyDrawer.GetDefaultValueDescription(shader, prop, propDynamicData.defualtProperty, perShaderData, this));
				if (string.IsNullOrEmpty(propDynamicData.defaultValueDescription))
					propDynamicData.defaultValueDescription = RevertableHelper.GetPropertyDefaultValueText(propDynamicData.defualtProperty);

				// Get ShowIf() results
				ShowIfDecorator.GetShowIfResult(propStaticData, propDynamicData, this);
			}

			forceInit = false;
		}

		public void Update(Shader shader, Material material, MaterialProperty[] props, PerShaderData perShaderData)
		{
			if (forceInit)
			{
				Init(shader, material, props, perShaderData);
				return;
			}

			foreach (var prop in props)
			{
				propDynamicDatas[prop.name].property = prop;
			}
		}

		public bool EndChangeCheck(string propName = null)
		{
			if (!string.IsNullOrEmpty(propName))
			{
				GUI.changed |= propDynamicDatas[propName].hasRevertChanged;
				propDynamicDatas[propName].hasRevertChanged = false;
			}
			return EditorGUI.EndChangeCheck();
		}

		public PropertyDynamicData GetPropDynamicData(string propName)
		{
			propDynamicDatas.TryGetValue(propName, out var propDynamicData);
			return propDynamicData;
		}
	}
}