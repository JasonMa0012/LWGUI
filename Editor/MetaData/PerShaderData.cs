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
	public class DisplayModeStaticData
	{
		public int advancedCount;
		public int hiddenCount;
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
		public string                           conditionalDisplayKeyword = string.Empty;                           // [Group(groupName_conditionalDisplayKeyword)]
		public bool                             isHidden                  = false;                                  // [Hidden]
		public bool                             isReadOnly                = false;                                  // [ReadOnly]
		public List<ShowIfDecorator.ShowIfData> showIfDatas               = new List<ShowIfDecorator.ShowIfData>(); // [ShowIf()]

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
		public DisplayModeStaticData                  displayModeStaticData = new DisplayModeStaticData();
		public List<string>                           favoriteproperties    = new List<string>();

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
						displayModeStaticData.hiddenCount++;
					if (propStaticData.isAdvanced
					 || (propStaticData.parent != null
					  && (propStaticData.parent.isAdvanced
					   || (propStaticData.parent.parent != null && propStaticData.parent.parent.isAdvanced))))
						displayModeStaticData.advancedCount++;

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

		public PropertyStaticData GetPropStaticData(string propName)
		{
			propStaticDatas.TryGetValue(propName, out var propStaticData);
			return propStaticData;
		}
	}
}