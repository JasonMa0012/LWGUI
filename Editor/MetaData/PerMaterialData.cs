// Copyright (c) Jason Ma
// Per Shader > Per Material > Per Inspector

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

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
	///
	/// Some of which may need to be updated every frame, such as the default value.
	/// (Because there are many other ways to modify the default value externally)
	/// </summary>
	public class PerMaterialData
	{
		public Dictionary<string, PropertyDynamicData> propDynamicDatas = new Dictionary<string, PropertyDynamicData>();
		public MaterialProperty[]                      props            = null;
		public Material                                material         = null;
		public List<PersetDynamicData>                 activePresets    = new List<PersetDynamicData>();
		public int                                     modifiedCount    = 0;

		public PerMaterialData(Material material, MaterialProperty[] props)
		{
			this.props = props;
			this.material = material;
		}

		public void Update(Shader shader, Material material, MaterialProperty[] props, PerShaderData perShaderData)
		{
			// Get active presets
			activePresets.Clear();
			foreach (var prop in props)
			{
				var activePreset = (perShaderData.propStaticDatas[prop.name].drawer as IBasePresetDrawer)
					?.GetActivePreset(prop, perShaderData.propStaticDatas[prop.name].propertyPresetAsset);
				if (activePreset != null)
					activePresets.Add(new PersetDynamicData(activePreset, prop));
			}

			{
				// Apply presets to default material
				var defaultMaterial =
#if UNITY_2022_1_OR_NEWER
					material.parent
						? UnityEngine.Object.Instantiate(material.parent)
						:
#endif
						new Material(shader);

				foreach (var activePreset in activePresets)
					activePreset.preset.ApplyToDefaultMaterial(defaultMaterial);

				var defaultProperties = MaterialEditor.GetMaterialProperties(new[] { defaultMaterial });
				Debug.Assert(defaultProperties.Length == props.Length);

				// Init propDynamicDatas
				propDynamicDatas.Clear();
				modifiedCount = 0;
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

				foreach (var prop in props)
				{
					var propStaticData = perShaderData.propStaticDatas[prop.name];
					var propDynamicData = propDynamicDatas[prop.name];

					// Extra Prop hasModified
					foreach (var extraPropName in propStaticData.extraPropNames)
						propDynamicData.hasModified |= propDynamicDatas[extraPropName].hasModified;

					// Override parent hasModified
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

			foreach (var prop in props)
			{
				var propStaticData = perShaderData.propStaticDatas[prop.name];
				var propDynamicData = propDynamicDatas[prop.name];

				// Get default value descriptions
				(propStaticData.drawer as IBaseDrawer)?.GetDefaultValueDescription(shader, prop, propDynamicData.defualtProperty, perShaderData, this);
				propStaticData.decoratorDrawers?.ForEach(propertyDrawer =>
															 (propertyDrawer as IBaseDrawer)?.GetDefaultValueDescription(shader, prop, propDynamicData.defualtProperty, perShaderData, this));
				if (string.IsNullOrEmpty(propDynamicData.defaultValueDescription))
					propDynamicData.defaultValueDescription = RevertableHelper.GetPropertyDefaultValueText(propDynamicData.defualtProperty);

				// Get ShowIf() results
				foreach (var showIfData in propStaticData.showIfDatas)
				{
					var propCurrentValue = propDynamicDatas[showIfData.targetPropertyName].property.floatValue;
					bool compareResult;

					switch (showIfData.compareFunction)
					{
						case CompareFunction.Less:
							compareResult = propCurrentValue < showIfData.value;
							break;
						case CompareFunction.LessEqual:
							compareResult = propCurrentValue <= showIfData.value;
							break;
						case CompareFunction.Greater:
							compareResult = propCurrentValue > showIfData.value;
							break;
						case CompareFunction.NotEqual:
							compareResult = propCurrentValue != showIfData.value;
							break;
						case CompareFunction.GreaterEqual:
							compareResult = propCurrentValue >= showIfData.value;
							break;
						default:
							compareResult = propCurrentValue == showIfData.value;
							break;
					}

					switch (showIfData.logicalOperator)
					{
						case LogicalOperator.And:
							propDynamicData.isShowing &= compareResult;
							break;
						case LogicalOperator.Or:
							propDynamicData.isShowing |= compareResult;
							break;
					}
				}
			}
		}

		public bool EndChangeCheck(string propName = null)
		{
			var result = EditorGUI.EndChangeCheck();
			if (!string.IsNullOrEmpty(propName))
			{
				result |= propDynamicDatas[propName].hasRevertChanged;
				propDynamicDatas[propName].hasRevertChanged = false;
			}
			return result;
		}

		public PropertyDynamicData GetPropDynamicData(string propName)
		{
			propDynamicDatas.TryGetValue(propName, out var propDynamicData);
			return propDynamicData;
		}
	}
}