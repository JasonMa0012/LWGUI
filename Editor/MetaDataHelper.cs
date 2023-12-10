// Copyright (c) Jason Ma
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	public enum SearchMode
	{
		Auto     = 0, // Search by group first, and search by property when there are no results
		Property = 1, // Search by property
		Group    = 2, // Search by group
		Num      = 3
	}

	public enum LogicalOperator
	{
		And,
		Or
	}

	public struct DisplayModeData
	{
		public bool showAllAdvancedProperties;
		public bool showAllHiddenProperties;
		public bool showOnlyModifiedProperties;

		public int advancedCount;
		public int hiddenCount;

		public bool IsDefaultDisplayMode() { return !(showAllAdvancedProperties || showAllHiddenProperties || showOnlyModifiedProperties); }
	}

	public class ShowIfData
	{
		public LogicalOperator logicalOperator    = LogicalOperator.And;
		public string          targetPropertyName = string.Empty;
		public CompareFunction compareFunction    = CompareFunction.Equal;
		public float           value              = 0;
	}

	/// <summary>
	/// All static metadata for a Property, determined after the Shader is compiled.
	/// </summary>
	public class PropertyStaticData
	{
		public string name        = string.Empty;
		public string displayName = string.Empty; // Decoded displayName (Helpbox and Tooltip are encoded in displayName)

		// Structure
		public string                   groupName                = string.Empty; // [Group(groupName)] / [Sub(groupName)] / [Advanced(groupName)]
		public bool                     isMain                   = false;        // [Group]
		public bool                     isAdvanced               = false;        // [Advanced]
		public bool                     isAdvancedHeader         = false;        // the first [Advanced] in the same group
		public bool                     isAdvancedHeaderProperty = false;
		public string                   advancedHeaderString     = string.Empty;
		public PropertyStaticData       parent                   = null;
		public List<PropertyStaticData> children                 = new List<PropertyStaticData>();

		// Visibility
		public string           conditionalDisplayKeyword = string.Empty;           // [Group(groupName_conditionalDisplayKeyword)]
		public bool             isSearchMatched           = true;                   // Draws when the search match is successful
		public bool             isExpanding               = false;                  // Draws when the group is expanding
		public bool             isHidden                  = false;                  // [Hidden]
		public List<ShowIfData> showIfDatas               = new List<ShowIfData>(); // [ShowIf()]

		// Metadata
		public List<string>         extraPropNames          = new List<string>();	// Other Props that have been associated
		public string               helpboxMessages         = string.Empty;
		public string               tooltipMessages         = string.Empty;
		public ShaderPropertyPreset propertyPresetAsset     = null;					// The Referenced Preset Asset

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
		public DisplayModeData                        displayModeData    = new DisplayModeData();


		public void BuildPropertyStaticData(Shader shader, MaterialProperty[] props)
		{
			// Get Property Static Data
			foreach (var prop in props)
			{
				var propStaticData = new PropertyStaticData(){ name = prop.name };
				propertyDatas[prop.name] = propStaticData;

				List<MaterialPropertyDrawer> decoratorDrawers;
				var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out decoratorDrawers);
				if (decoratorDrawers != null && decoratorDrawers.Count > 0)
				{
					foreach (var decoratorDrawer in decoratorDrawers)
					{
						if (decoratorDrawer is IBaseDrawer)
							(decoratorDrawer as IBaseDrawer).BuildStaticMetaData(shader, prop, props, propStaticData);
					}
				}
				if (drawer != null)
				{
					if (drawer is IBaseDrawer)
						(drawer as IBaseDrawer).BuildStaticMetaData(shader, prop, props, propStaticData);
				}

				DecodeMetaDataFromDisplayName(prop, propStaticData);
			}

			// Check Data
			foreach (var prop in props)
			{
				var propStaticData = propertyDatas[prop.name];
				propStaticData.extraPropNames.RemoveAll((extraPropName =>
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

			// Build Display Mode Data
			{
				PropertyStaticData lastPropData = null;
				PropertyStaticData lastHeaderPropData = null;
				for (int i = 0; i < props.Length; i++)
				{
					var prop = props[i];
					var propStaticData = propertyDatas[prop.name];

					// Counting
					if (propStaticData.isHidden
					 || (propStaticData.parent != null
					  && (propStaticData.parent.isHidden
					   || (propStaticData.parent.parent != null && propStaticData.parent.parent.isHidden))))
						displayModeData.hiddenCount++;
					if (propStaticData.isAdvanced
					 || (propStaticData.parent != null
					  && (propStaticData.parent.isAdvanced
					   || (propStaticData.parent.parent != null && propStaticData.parent.parent.isAdvanced))))
						displayModeData.advancedCount++;

					// Build Advanced Structure
					if (propStaticData.isAdvanced)
					{
						// If it is the first prop in a Advanced Block, set to Header
						if (lastPropData == null
						 || !lastPropData.isAdvanced
						 || propStaticData.isAdvancedHeaderProperty
						 || (!string.IsNullOrEmpty(propStaticData.advancedHeaderString)
							&& propStaticData.advancedHeaderString != lastPropData.advancedHeaderString))
						{
							propStaticData.isAdvancedHeader = true;
							lastHeaderPropData = propStaticData;
						}
						// Else set to child
						else
						{
							propStaticData.parent = lastHeaderPropData;
							lastHeaderPropData.children.Add(propStaticData);
						}

					}

					lastPropData = propStaticData;
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
				propStaticData.helpboxMessages = propStaticData.helpboxMessages.Substring(0, propStaticData.helpboxMessages.Length - 1);

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
				propertyData.Value.isSearchMatched = isSearchStringEmpty
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
					if (!propertyDatas.Any((propertyData => propertyData.Value.isSearchMatched && propertyData.Value.isMain)))
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
						if (propertyData.Value.isMain && propertyData.Value.children.Any((childPropertyData => childPropertyData.isSearchMatched)))
							propertyData.Value.isSearchMatched = true;
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
								childPropertyData.isSearchMatched = propertyData.Value.isSearchMatched;
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

		public void ToggleShowAllAdvancedProperties()
		{
			foreach (var propertyStaticDataPair in propertyDatas)
			{
				if (propertyStaticDataPair.Value.isAdvancedHeader)
					propertyStaticDataPair.Value.isExpanding = displayModeData.showAllAdvancedProperties;
			}
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
		public bool             hasModified             = false;        // Are properties modified in the material?
		public bool             hasChildrenModified     = false;        // Are Children properties modified in the material?
		public bool             hasRevertChanged        = false;        // Used to call property EndChangeCheck()
		public bool             isShowing               = true;			// ShowIf() result

	}

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

	/// <summary>
	/// Each frame of each material may have different metadata.
	/// </summary>
	public class PerFrameData
	{
		public Dictionary<string, PropertyDynamicData> propertyDatas    = new Dictionary<string, PropertyDynamicData>();
		public List<PersetDynamicData>                 activePresets    = new List<PersetDynamicData>();

		public int modifiedCount = 0;

		public void BuildPerFrameData(Shader shader, Material material, MaterialProperty[] props, PerShaderData perShaderData)
		{
			// Get active presets
			foreach (var prop in props)
			{
				List<MaterialPropertyDrawer> decoratorDrawers;
				var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out decoratorDrawers);

				// Get Presets
				if (drawer != null)
				{
					if (drawer is IBasePresetDrawer)
					{
						var activePreset = (drawer as IBasePresetDrawer).GetActivePreset(prop, perShaderData.propertyDatas[prop.name].propertyPresetAsset);
						if (activePreset != null)
						{
							activePresets.Add(new PersetDynamicData(activePreset, prop));
						}
					}
				}
			}

			// Apply presets to default material
			{
				var defaultMaterial =
#if UNITY_2022_1_OR_NEWER
					material.parent ? UnityEngine.Object.Instantiate(material.parent) :
#endif
					new Material(shader);

				foreach (var activePreset in activePresets)
					activePreset.preset.ApplyToDefaultMaterial(defaultMaterial);

				var defaultProperties = MaterialEditor.GetMaterialProperties(new[] { defaultMaterial });
				Debug.Assert(defaultProperties.Length == props.Length);

				for (int i = 0; i < props.Length; i++)
				{
					Debug.Assert(props[i].name == defaultProperties[i].name);
					Debug.Assert(!propertyDatas.ContainsKey(props[i].name));

					var hasModified = !Helper.PropertyValueEquals(props[i], defaultProperties[i]);
					if (hasModified) modifiedCount++;
					propertyDatas.Add(props[i].name, new PropertyDynamicData()
					{
						property = props[i],
						defualtProperty = defaultProperties[i],
						hasModified = hasModified
					});
				}

				// Extra Prop hasModified
				for (int i = 0; i < props.Length; i++)
				{
					foreach (var extraPropName in perShaderData.propertyDatas[props[i].name].extraPropNames)
					{
						if (propertyDatas[extraPropName].hasModified)
							propertyDatas[props[i].name].hasChildrenModified = true;
					}
				}
			}

			foreach (var prop in props)
			{
				var propStaticData = perShaderData.propertyDatas[prop.name];
				var propDynamicData = propertyDatas[prop.name];

				// Override parent hasModified
				if (propDynamicData.hasModified)
				{
					var parentPropData = propStaticData.parent;
					if (parentPropData != null)
					{
						propertyDatas[parentPropData.name].hasChildrenModified = true;
						if (parentPropData.parent != null)
							propertyDatas[parentPropData.parent.name].hasChildrenModified = true;
					}
				}

				// Get default value descriptions
				List<MaterialPropertyDrawer> decoratorDrawers;
				var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out decoratorDrawers);
				{
					if (decoratorDrawers != null && decoratorDrawers.Count > 0)
					{
						foreach (var decoratorDrawer in decoratorDrawers)
						{
							if (decoratorDrawer is IBaseDrawer)
								(decoratorDrawer as IBaseDrawer).GetDefaultValueDescription(shader, prop, propDynamicData.defualtProperty, perShaderData, this);
						}
					}
					if (drawer != null)
					{
						if (drawer is IBaseDrawer)
							(drawer as IBaseDrawer).GetDefaultValueDescription(shader, prop, propDynamicData.defualtProperty, perShaderData, this);
					}
					if (string.IsNullOrEmpty(propDynamicData.defaultValueDescription))
						propDynamicData.defaultValueDescription =
							RevertableHelper.GetPropertyDefaultValueText(propDynamicData.defualtProperty);
				}

				// Get ShowIf() results
				foreach (var showIfData in propStaticData.showIfDatas)
				{
					var propCurrentValue = propertyDatas[showIfData.targetPropertyName].property.floatValue;
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

		public MaterialProperty GetProperty(string propName)
		{
			if (!string.IsNullOrEmpty(propName) && propertyDatas.ContainsKey(propName))
				return propertyDatas[propName].property;
			else
				return null;
		}

		public MaterialProperty GetDefaultProperty(string propName)
		{
			if (!string.IsNullOrEmpty(propName) && propertyDatas.ContainsKey(propName))
				return propertyDatas[propName].defualtProperty;
			else
				return null;
		}

		public bool EndChangeCheck(string propName = null)
		{
			var result = EditorGUI.EndChangeCheck();
			if (!string.IsNullOrEmpty(propName))
			{
				result |= propertyDatas[propName].hasRevertChanged;
				propertyDatas[propName].hasRevertChanged = false;
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

		public static PerFrameData BuildPerFrameData(Shader shader, Material material, MaterialProperty[] props)
		{
			var perFrameData = new PerFrameData();
			perFrameData.BuildPerFrameData(shader, material, props, _shaderDataDic[shader]);
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

		public static bool GetPropertyVisibility(MaterialProperty prop, Material material, LWGUI lwgui)
		{
			bool result = true;

			var propertyStaticData = lwgui.perShaderData.propertyDatas[prop.name];
			var propertyDynamicData = lwgui.perFrameData.propertyDatas[prop.name];
			var displayModeData = lwgui.perShaderData.displayModeData;

			if ( // if HideInInspector
				Helper.IsPropertyHideInInspector(prop)
				// if Search Filtered
			 	|| !propertyStaticData.isSearchMatched
				// if the Conditional Display Keyword is not active
			 	|| (!string.IsNullOrEmpty(propertyStaticData.conditionalDisplayKeyword) && !material.shaderKeywords.Any((str => str == propertyStaticData.conditionalDisplayKeyword)))
				|| (!displayModeData.showAllHiddenProperties && propertyStaticData.isHidden)
				// if show modified only
				|| (displayModeData.showOnlyModifiedProperties && !(propertyDynamicData.hasModified || propertyDynamicData.hasChildrenModified))
				// ShowIf() == false
				|| !propertyDynamicData.isShowing
			   )
			{
				result = false;
			}

			return result;
		}

		public static bool GetParentPropertyVisibility(PropertyStaticData parentPropStaticData, Material material, LWGUI lwgui)
		{
			bool result = true;

			if (parentPropStaticData != null
				&& (!lwgui.perShaderData.propertyDatas[parentPropStaticData.name].isExpanding
					|| !MetaDataHelper.GetPropertyVisibility(lwgui.perFrameData.propertyDatas[parentPropStaticData.name].property, material, lwgui)))
			{
				result = false;
			}

			return result;
		}
	}
}