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

	public class DisplayModeDynamicData
	{
		public bool                     showAllAdvancedProperties;
		public bool                     showAllHiddenProperties;
		public bool                     showOnlyModifiedProperties;
		public bool                     showOnlyModifiedGroups;
		public Dictionary<string, bool> cachedModifiedProperties;

		public bool IsDefaultDisplayMode() { return !(showAllAdvancedProperties || showAllHiddenProperties || showOnlyModifiedProperties || showOnlyModifiedGroups); }
	}

	public class PropertyInspectorData
	{
		public bool isSearchMatched = true;  // Draws when the searching matched
		public bool isExpanding     = false; // Draws when the group is expanding
	}

	/// <summary>
	/// Contains metadata that may be different for each Inspector
	/// </summary>
	public class PerInspectorData
	{
		public Dictionary<string /*propName*/, PropertyInspectorData> propInspectorDatas     = new Dictionary<string, PropertyInspectorData>();
		public MaterialEditor                                         materialEditor         = null;
		public DisplayModeDynamicData                                 displayModeDynamicData = new DisplayModeDynamicData();
		public SearchMode                                             searchMode             = SearchMode.Auto;
		public string                                                 searchString           = string.Empty;


		public PerInspectorData(MaterialProperty[] props, PerShaderData perShaderData, PerMaterialData perMaterialData)
		{
			foreach (var prop in props)
			{
				if (!propInspectorDatas.ContainsKey(prop.name))
					propInspectorDatas.Add(prop.name, new PropertyInspectorData());

				perShaderData.propStaticDatas[prop.name].baseDrawers?.ForEach(baseDrawer => baseDrawer.InitInspectorData(propInspectorDatas[prop.name]));
			}
		}

		public void Update(MaterialEditor materialEditor)
		{
			this.materialEditor = materialEditor;
		}

		public void UpdateSearchFilter(PerShaderData perShaderData)
		{
			var isSearchStringEmpty = string.IsNullOrEmpty(searchString);
			var searchStringLower = searchString.ToLower();
			var searchKeywords = searchStringLower.Split(' ', ',', ';', '|', '，', '；'); // Some possible separators
			var propStaticDatas = perShaderData.propStaticDatas;

			// The First Search
			foreach (var propInspectorDataKVPair in propInspectorDatas)
			{
				propInspectorDataKVPair.Value.isSearchMatched = isSearchStringEmpty
					? true
					: IsWholeWordMatch(propStaticDatas[propInspectorDataKVPair.Key].displayName, propInspectorDataKVPair.Key, searchKeywords);
			}

			// Further adjust visibility
			if (!isSearchStringEmpty)
			{
				var searchModeTemp = searchMode;
				// Auto: search by group first, and search by property when there are no results
				if (searchModeTemp == SearchMode.Auto)
				{
					// if has no group
					if (!propInspectorDatas.Any((propInspectorDataKVPair =>
													propInspectorDataKVPair.Value.isSearchMatched
												 && propStaticDatas[propInspectorDataKVPair.Key].isMain)))
						searchModeTemp = SearchMode.Property;
					else
						searchModeTemp = SearchMode.Group;
				}

				// search by property
				if (searchModeTemp == SearchMode.Property)
				{
					// when a SubProp is displayed, the MainProp is also displayed
					foreach (var propInspectorDataKVPair in propInspectorDatas)
					{
						var propStaticData = propStaticDatas[propInspectorDataKVPair.Key];
						if (propStaticData.isMain
						 && propStaticData.children.Any((childPropStaticData => propInspectorDatas[childPropStaticData.name].isSearchMatched)))
							propInspectorDataKVPair.Value.isSearchMatched = true;
					}
				}
				// search by group
				else if (searchModeTemp == SearchMode.Group)
				{
					// when search by group, all SubProps should display with MainProp
					foreach (var propInspectorDataKVPair in propInspectorDatas)
					{
						var propStaticData = propStaticDatas[propInspectorDataKVPair.Key];
						if (propStaticData.isMain)
							foreach (var childPropStaticData in propStaticData.children)
								propInspectorDatas[childPropStaticData.name].isSearchMatched = propInspectorDataKVPair.Value.isSearchMatched;
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

		public void ToggleShowAllAdvancedProperties(PerShaderData perShaderData)
		{
			foreach (var propStaticData in perShaderData.propStaticDatas.Values)
			{
				if (propStaticData.isAdvancedHeader)
					propInspectorDatas[propStaticData.name].isExpanding = displayModeDynamicData.showAllAdvancedProperties;
			}
		}

		public PropertyInspectorData GetPropInspectorData(string propName)
		{
			propInspectorDatas.TryGetValue(propName, out var propInspectorData);
			return propInspectorData;
		}
	}
}