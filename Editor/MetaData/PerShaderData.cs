// Copyright (c) Jason Ma
// Per Shader > Per Material > Per Inspector

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

	public class DisplayModeData
	{
		public bool showAllAdvancedProperties;
		public bool showAllHiddenProperties;
		public bool showOnlyModifiedProperties;
		public bool showOnlyModifiedGroups;

		public int advancedCount;
		public int hiddenCount;

		public bool IsDefaultDisplayMode() { return !(showAllAdvancedProperties || showAllHiddenProperties || showOnlyModifiedProperties || showOnlyModifiedGroups); }
	}

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
		public bool                             isSearchMatched           = true;                                   // Search filter result
		public bool                             isExpanding               = false;                                  // Children are displayed only when expanded
		public bool                             isReadOnly                = false;                                  // [ReadOnly]
		public bool                             isHidden                  = false;                                  // [Hidden]
		public List<ShowIfDecorator.ShowIfData> showIfDatas               = new List<ShowIfDecorator.ShowIfData>(); // [ShowIf()]
		public string                           conditionalDisplayKeyword = string.Empty;                           // [Group(groupName_conditionalDisplayKeyword)]

		// Drawers
		public IBasePresetDrawer presetDrawer = null;
		public List<IBaseDrawer> baseDrawers  = null;

		// Metadata
		public List<string>         extraPropNames      = new List<string>(); // Other Props that have been associated
		public string               helpboxMessages     = string.Empty;
		public string               tooltipMessages     = string.Empty;
		public ShaderPropertyPreset propertyPresetAsset = null; // The Referenced Preset Asset

		public void AddExtraProperty(string propName)
		{
			if (!extraPropNames.Contains(propName)) extraPropNames.Add(propName);
		}
	}

	/// <summary>
	/// All Shader static metadata can be determined after Shader is compiled and will not change.
	/// </summary>
	public class PerShaderData
	{
		public Dictionary<string, PropertyStaticData> propStaticDatas       = new Dictionary<string, PropertyStaticData>();
		public Shader                                 shader                = null;
		public DisplayModeData                  displayModeData = new DisplayModeData();
		public SearchMode                             searchMode            = SearchMode.Auto;
		public string                                 searchString          = string.Empty;
		// public List<string>                           favoriteproperties    = new List<string>();

		// UnityEngine.Object may be destroyed when loading new scene, so must manually check null reference
		private Material _defaultMaterial = null;

		public Material defaultMaterial
		{
			get
			{
				if (!_defaultMaterial && shader) _defaultMaterial = new Material(shader);
				return _defaultMaterial;
			}
		}

		public PerShaderData(Shader shader, MaterialProperty[] props)
		{
			this.shader = shader;

			// Get Property Static Data
			foreach (var prop in props)
			{
				var propStaticData = new PropertyStaticData() { name = prop.name };
				propStaticDatas[prop.name] = propStaticData;

				// Get Drawers and Build Drawer StaticMetaData
				{
					var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out var decoratorDrawers);

					if (drawer is IBasePresetDrawer)
						propStaticData.presetDrawer = drawer as IBasePresetDrawer;

					var baseDrawer = drawer as IBaseDrawer;
					if (baseDrawer != null)
					{
						propStaticData.baseDrawers = new List<IBaseDrawer>() { baseDrawer };
						baseDrawer.BuildStaticMetaData(shader, prop, props, propStaticData);
					}

					decoratorDrawers?.ForEach(decoratorDrawer =>
					{
						baseDrawer = decoratorDrawer as IBaseDrawer;
						if (baseDrawer != null)
						{
							if (propStaticData.baseDrawers == null)
								propStaticData.baseDrawers = new List<IBaseDrawer>() { baseDrawer };
							else
								propStaticData.baseDrawers.Add(baseDrawer);

							baseDrawer.BuildStaticMetaData(shader, prop, props, propStaticData);
						}
					});
				}

				DecodeMetaDataFromDisplayName(prop, propStaticData);
			}

			// Check Data
			foreach (var prop in props)
			{
				var propStaticData = propStaticDatas[prop.name];
				propStaticData.extraPropNames.RemoveAll((extraPropName =>
															string.IsNullOrEmpty(extraPropName) || !propStaticDatas.ContainsKey(extraPropName)));
			}

			// Build Property Structure
			{
				var groupToMainPropertyDic = new Dictionary<string, MaterialProperty>();

				// Collection Groups
				foreach (var prop in props)
				{
					var propData = propStaticDatas[prop.name];
					if (propData.isMain
					 && !string.IsNullOrEmpty(propData.groupName)
					 && !groupToMainPropertyDic.ContainsKey(propData.groupName))
						groupToMainPropertyDic.Add(propData.groupName, prop);
				}

				// Register SubProps
				foreach (var prop in props)
				{
					var propData = propStaticDatas[prop.name];
					if (!propData.isMain
					 && !string.IsNullOrEmpty(propData.groupName))
					{
						foreach (var groupName in groupToMainPropertyDic.Keys)
						{
							if (propData.groupName.StartsWith(groupName))
							{
								// Update Structure
								var mainProp = groupToMainPropertyDic[groupName];
								propData.parent = propStaticDatas[mainProp.name];
								propStaticDatas[mainProp.name].children.Add(propData);

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
					var propStaticData = propStaticDatas[prop.name];

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

		public PropertyStaticData GetPropStaticData(string propName)
		{
			propStaticDatas.TryGetValue(propName, out var propStaticData);
			return propStaticData;
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
			foreach (var propStaticDataKWPair in propStaticDatas)
			{
				propStaticDataKWPair.Value.isSearchMatched = isSearchStringEmpty
					? true
					: IsWholeWordMatch(propStaticDataKWPair.Value.displayName, propStaticDataKWPair.Value.name, searchKeywords);
			}

			// Further adjust visibility
			if (!isSearchStringEmpty)
			{
				var searchModeTemp = searchMode;
				// Auto: search by group first, and search by property when there are no results
				if (searchModeTemp == SearchMode.Auto)
				{
					// if has no group
					if (!propStaticDatas.Any((propStaticDataKWPair => propStaticDataKWPair.Value.isSearchMatched && propStaticDataKWPair.Value.isMain)))
						searchModeTemp = SearchMode.Property;
					else
						searchModeTemp = SearchMode.Group;
				}

				// search by property
				if (searchModeTemp == SearchMode.Property)
				{
					// when a SubProp is displayed, the MainProp is also displayed
					foreach (var propStaticDataKWPair in propStaticDatas)
					{
						var propStaticData = propStaticDataKWPair.Value;
						if (propStaticData.isMain
						 && propStaticData.children.Any((childPropStaticData => propStaticDatas[childPropStaticData.name].isSearchMatched)))
							propStaticDataKWPair.Value.isSearchMatched = true;
					}
				}
				// search by group
				else if (searchModeTemp == SearchMode.Group)
				{
					// when search by group, all SubProps should display with MainProp
					foreach (var propStaticDataKWPair in propStaticDatas)
					{
						var propStaticData = propStaticDataKWPair.Value;
						if (propStaticData.isMain)
							foreach (var childPropStaticData in propStaticData.children)
								propStaticDatas[childPropStaticData.name].isSearchMatched = propStaticData.isSearchMatched;
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
			foreach (var propStaticDataKWPair in propStaticDatas)
			{
				if (propStaticDataKWPair.Value.isAdvancedHeader)
					propStaticDataKWPair.Value.isExpanding = displayModeData.showAllAdvancedProperties;
			}
		}

	}
}