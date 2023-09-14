// Copyright (c) Jason Ma
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	public enum SearchMode
	{
		Auto     = 0, // Search by group first, and search by property when there are no results
		Property = 1, // Search by property
		Group    = 2, // Search by group
		Num      = 3
	}

	/// <summary>
	/// All static metadata for a Property, determined after the Shader is compiled.
	/// </summary>
	public class PropertyStaticData
	{
		public string displayName	= string.Empty; // Decoded displayName (Helpbox and Tooltip are encoded in displayName)

		// Structure
		public string                   groupName        = string.Empty; // [Group(groupName)] / [Sub(groupName)] / [Advanced(groupName)]
		public bool                     isMain           = false;        // [Group]
		public bool                     isAdvanced       = false;        // [Advanced]
		public bool                     isAdvancedHeader = false;        // the first [Advanced] in the same group
		public PropertyStaticData       parent           = null;
		public List<PropertyStaticData> children         = new List<PropertyStaticData>();

		// Visibility
		public string conditionalDisplayKeyword = string.Empty;	// [Group(groupName_conditionalDisplayKeyword)]
		public bool   isSearchDisplayed         = true;			// Draws when the search match is successful
		public bool   isExpanded                = false;		// Draws when the group has been expanded
		public bool   isHidden                  = false;		// [Hidden]

		// Metadata
		public List<string>         extraPropNames          = new List<string>();	// Other Props that have been associated
		public string               helpboxMessages         = string.Empty;
		public string               tooltipMessages         = string.Empty;
		public ShaderPropertyPreset propertyPreset          = null;					// The Referenced Preset Asset

		public void AddExtraProperty(string propName)
		{
			if (!extraPropNames.Contains(propName)) extraPropNames.Add(propName);
		}
	}

	/// <summary>
	/// Consistent metadata across different material instances of the same Shader.
	/// </summary>
	public class PerShaderData
	{
		public Dictionary<string, PropertyStaticData> propertyDatas      = new Dictionary<string, PropertyStaticData>();
		public SearchMode                             searchMode         = SearchMode.Auto;
		public string                                 searchString       = string.Empty;
		public List<string>                           favoriteproperties = new List<string>();


		public void BuildPropertyStaticData(Shader shader, MaterialProperty[] props)
		{
			// Get Property Static
			foreach (var prop in props)
			{
				var propertyStaticData = new PropertyStaticData();
				propertyDatas[prop.name] = propertyStaticData;

				List<MaterialPropertyDrawer> decoratorDrawers;
				var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out decoratorDrawers);
				if (decoratorDrawers != null && decoratorDrawers.Count > 0)
				{
					foreach (var decoratorDrawer in decoratorDrawers)
					{
						if (decoratorDrawer is IBaseDrawer)
							(decoratorDrawer as IBaseDrawer).BuildStaticMetaData(shader, prop, props, propertyStaticData);
					}
				}
				if (drawer != null)
				{
					if (drawer is IBaseDrawer)
						(drawer as IBaseDrawer).BuildStaticMetaData(shader, prop, props, propertyStaticData);
				}

				DecodeMetaDataFromDisplayName(prop, propertyStaticData);
			}

			// Check Data
			foreach (var prop in props)
			{
				var propertyStaticData = propertyDatas[prop.name];
				propertyStaticData.extraPropNames.RemoveAll((extraPropName =>
					string.IsNullOrEmpty(extraPropName) || !propertyDatas.ContainsKey(extraPropName)));
			}

			// Build Property Structure
			{
				var groupToMainPropertyDic = new Dictionary<string, MaterialProperty>();

				// Collection Groups
				foreach (var prop in props)
				{
					var propData = propertyDatas[prop.name];
					if (propData.isMain
						&& !string.IsNullOrEmpty(propData.groupName)
						&& !groupToMainPropertyDic.ContainsKey(propData.groupName))
						groupToMainPropertyDic.Add(propData.groupName, prop);
				}

				// Register SubProps
				foreach (var prop in props)
				{
					var propData = propertyDatas[prop.name];
					if (!propData.isMain
					 && !string.IsNullOrEmpty(propData.groupName))
					{
						foreach (var groupName in groupToMainPropertyDic.Keys)
						{
							if (propData.groupName.StartsWith(groupName))
							{
								// Update Structure
								var mainProp = groupToMainPropertyDic[groupName];
								propData.parent = propertyDatas[mainProp.name];
								propertyDatas[mainProp.name].children.Add(propData);

								// Split groupName and conditional display keyword
								if (propData.groupName.Length > groupName.Length)
								{
									propData.conditionalDisplayKeyword =
										propData.groupName.Substring(groupName.Length, propData.groupName.Length - groupName.Length).ToUpper();
									propData.groupName = groupName;
								}
								break;
							}
						}
					}
				}
			}
		}

		private static readonly string _tooltipSplitter = "#";
		private static readonly string _helpboxSplitter = "%";

		public void DecodeMetaDataFromDisplayName(MaterialProperty prop, PropertyStaticData propStaticData)
		{
			var tooltips = prop.displayName.Split(new String[] { _tooltipSplitter }, StringSplitOptions.None);
			if (tooltips.Length > 1)
			{
				for (int i = 1; i <= tooltips.Length - 1; i++)
				{
					var str = tooltips[i];
					var helpboxIndex = tooltips[i].IndexOf(_helpboxSplitter, StringComparison.Ordinal);
					if (helpboxIndex > 0)
						str = tooltips[i].Substring(0, helpboxIndex);
					propStaticData.tooltipMessages += str + "\n";
				}
			}

			var helpboxes = prop.displayName.Split(new String[] { _helpboxSplitter }, StringSplitOptions.None);
			if (helpboxes.Length > 1)
			{
				for (int i = 1; i <= helpboxes.Length - 1; i++)
				{
					var str = helpboxes[i];
					var tooltipIndex = helpboxes[i].IndexOf(_tooltipSplitter, StringComparison.Ordinal);
					if (tooltipIndex > 0)
						str = tooltips[i].Substring(0, tooltipIndex);
					propStaticData.helpboxMessages += str + "\n";
				}
			}

			if (propStaticData.helpboxMessages.EndsWith("\n"))
				propStaticData.helpboxMessages = propStaticData.helpboxMessages.Substring(0, propStaticData.helpboxMessages.Length - "\n".Length - 1);

			propStaticData.displayName = prop.displayName.Split(new String[] { _tooltipSplitter, _helpboxSplitter }, StringSplitOptions.None)[0];
		}

		public void UpdateSearchFilter()
		{
			var isSearchStringEmpty = string.IsNullOrEmpty(searchString);
			var searchStringLower = searchString.ToLower();
			var searchKeywords = searchStringLower.Split(' ', ',', ';', '|', '，', '；'); // Some possible separators

			// The First Search
			foreach (var propertyData in propertyDatas)
			{
				propertyData.Value.isSearchDisplayed = isSearchStringEmpty
					? true
					: IsWholeWordMatch(propertyData.Value.displayName, propertyData.Key, searchKeywords);
			}

			// Further adjust visibility
			if (!isSearchStringEmpty)
			{
				var searchModeTemp = searchMode;
				// Auto: search by group first, and search by property when there are no results
				if (searchModeTemp == SearchMode.Auto)
				{
					// if has no group
					if (!propertyDatas.Any((propertyData => propertyData.Value.isSearchDisplayed && propertyData.Value.isMain)))
						searchModeTemp = SearchMode.Property;
					else
						searchModeTemp = SearchMode.Group;
				}

				// search by property
				if (searchModeTemp == SearchMode.Property)
				{
					// when a SubProp is displayed, the MainProp is also displayed
					foreach (var propertyData in propertyDatas)
					{
						if (propertyData.Value.isMain && propertyData.Value.children.Any((childPropertyData => childPropertyData.isSearchDisplayed)))
							propertyData.Value.isSearchDisplayed = true;
					}
				}
				// search by group
				else if (searchModeTemp == SearchMode.Group)
				{
					// when search by group, all SubProps should display with MainProp
					foreach (var propertyData in propertyDatas)
					{
						if (propertyData.Value.isMain)
							foreach (var childPropertyData in propertyData.Value.children)
								childPropertyData.isSearchDisplayed = propertyData.Value.isSearchDisplayed;
					}
				}
			}
		}

		private static bool IsWholeWordMatch(string displayName, string propertyName, string[] searchingKeywords)
		{
			bool contains = true;
			displayName = displayName.ToLower();
			var name = propertyName.ToLower();

			foreach (var keyword in searchingKeywords)
			{
				var isMatch = false;
				isMatch |= displayName.Contains(keyword);
				isMatch |= name.Contains(keyword);
				contains &= isMatch;
			}
			return contains;
		}
	}

	/// <summary>
	/// Property metadata dynamically generated perframe
	/// </summary>
	public class PropertyDynamicData
	{
		public MaterialProperty property;
		public MaterialProperty defualtProperty;                        // Default values may be overridden by Preset
		public string           defaultValueDescription = string.Empty; // Description of the default values used in Tooltip
		public bool             changed                 = false;

	}

	/// <summary>
	/// Each frame of each material may have different metadata.
	/// </summary>
	public class PerFrameData
	{
		public Dictionary<string, PropertyDynamicData> propertyDatas  = new Dictionary<string, PropertyDynamicData>();
		public List<ShaderPropertyPreset.Preset>       activePresets  = new List<ShaderPropertyPreset.Preset>();

		public void BuildPerFrameData(Shader shader, MaterialProperty[] props, PerShaderData perShaderData)
		{
			// Get active presets
			foreach (var prop in props)
			{
				List<MaterialPropertyDrawer> decoratorDrawers;
				var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out decoratorDrawers);
				if (drawer != null)
				{
					if (drawer is IBasePresetDrawer)
					{
						var activePreset = (drawer as IBasePresetDrawer).GetActivePreset(prop, perShaderData.propertyDatas[prop.name].propertyPreset);
						if (activePreset != null)
							activePresets.Add(activePreset);
					}
				}
			}

			// Apply presets to default value
			{
				var defaultMaterial = new Material(shader);
				foreach (var activePreset in activePresets)
				{
					foreach (var propertyValue in activePreset.propertyValues)
						propertyValue.Apply(defaultMaterial);
				}

				var defaultProperties = MaterialEditor.GetMaterialProperties(new[] { defaultMaterial });
				Debug.Assert(defaultProperties.Length == props.Length);

				for (int i = 0; i < props.Length; i++)
				{
					Debug.Assert(props[i].name == defaultProperties[i].name);
					Debug.Assert(!propertyDatas.ContainsKey(props[i].name));

					propertyDatas.Add(props[i].name, new PropertyDynamicData()
					{
						property = props[i],
						defualtProperty = defaultProperties[i]
					});
				}
			}

			// Get default value descriptions
			foreach (var prop in props)
			{
				List<MaterialPropertyDrawer> decoratorDrawers;
				var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out decoratorDrawers);
				if (decoratorDrawers != null && decoratorDrawers.Count > 0)
				{
					foreach (var decoratorDrawer in decoratorDrawers)
					{
						if (decoratorDrawer is IBaseDrawer)
							(decoratorDrawer as IBaseDrawer).GetDefaultValueDescription(shader, prop, perShaderData, this);
					}
				}
				if (drawer != null)
				{
					if (drawer is IBaseDrawer)
						(drawer as IBaseDrawer).GetDefaultValueDescription(shader, prop, perShaderData, this);
				}
				if (string.IsNullOrEmpty(propertyDatas[prop.name].defaultValueDescription))
					propertyDatas[prop.name].defaultValueDescription = RevertableHelper.GetPropertyDefaultValueText(propertyDatas[prop.name].defualtProperty);
			}
		}

		public bool EndChangeCheck(string propName = null)
		{
			var result = EditorGUI.EndChangeCheck();
			if (!string.IsNullOrEmpty(propName))
			{
				result |= propertyDatas[propName].changed;
				propertyDatas[propName].changed = false;
			}
			return result;
		}
	}

	public class MetaDataHelper
	{
		private static Dictionary<Shader, PerShaderData> _shaderDataDic = new Dictionary<Shader, PerShaderData>();

		public static PerShaderData BuildPerShaderData(Shader shader, MaterialProperty[] props)
		{
			if (!_shaderDataDic.ContainsKey(shader))
			{
				var perShaderData = new PerShaderData();
				perShaderData.BuildPropertyStaticData(shader, props);
				_shaderDataDic.Add(shader, perShaderData);
			}
			return _shaderDataDic[shader];
		}

		public static void ForceRebuildPerShaderData(Shader shader)
		{
			if (shader && _shaderDataDic.ContainsKey(shader))
				_shaderDataDic.Remove(shader);
		}

		public static PerFrameData BuildPerFrameData(Shader shader, MaterialProperty[] props)
		{
			var perFrameData = new PerFrameData();
			perFrameData.BuildPerFrameData(shader, props, _shaderDataDic[shader]);
			return perFrameData;
		}

		public static string GetPropertyTooltip(PropertyStaticData propertyStaticData, PropertyDynamicData propertyDynamicData)
		{
			var str = propertyStaticData.tooltipMessages;
			if (!string.IsNullOrEmpty(str))
				str += "\n\n";
			str += "Property Name: " + propertyDynamicData.property.name + "\n";
			str += "Default Value: " + propertyDynamicData.defaultValueDescription;
			return str;
		}


		#region Meta Data Container

		private static Dictionary<Shader, Dictionary<string /*PropName*/, List<ShaderPropertyPreset /*Preset*/>>> _presetDic = new Dictionary<Shader, Dictionary<string, List<ShaderPropertyPreset>>>();

		public static void ClearCaches(Shader shader)
		{
			if (_presetDic.ContainsKey(shader)) _presetDic[shader].Clear();
		}

		#endregion


		#region Display Name

		private static readonly string _tooltipString = "#";
		private static readonly string _helpboxString = "%";

		public static string GetPropertyDisplayName(Shader shader, MaterialProperty prop)
		{
			var tooltipIndex = prop.displayName.IndexOf(_tooltipString, StringComparison.Ordinal);
			var helpboxIndex = prop.displayName.IndexOf(_helpboxString, StringComparison.Ordinal);
			var minIndex = tooltipIndex == -1 ? helpboxIndex : tooltipIndex;
			if (tooltipIndex != -1 && helpboxIndex != -1)
				minIndex = Mathf.Min(minIndex, helpboxIndex);
			if (minIndex == -1)
				return prop.displayName;
			else if (minIndex == 0)
				return string.Empty;
			else
				return prop.displayName.Substring(0, minIndex);
		}


		#endregion


		#region Preset


		public static Dictionary<MaterialProperty, ShaderPropertyPreset> GetAllPropertyPreset(Shader shader, MaterialProperty[] props)
		{
			var result = new Dictionary<MaterialProperty, ShaderPropertyPreset>();

			var presetProps = props.Where((property =>
											  _presetDic.ContainsKey(shader) && _presetDic[shader].ContainsKey(property.name)));
			foreach (var presetProp in presetProps)
			{
				result.Add(presetProp, _presetDic[shader][presetProp.name][0]);
			}
			return result;
		}

		#endregion


	}
}