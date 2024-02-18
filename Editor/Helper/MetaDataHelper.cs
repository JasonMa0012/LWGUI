// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	public class LWGUIMetaDatas
	{
		public PerShaderData    perShaderData;
		public PerMaterialData  perMaterialData;
		public PerInspectorData perInspectorData;


		#region Get Prop Data
		public PropertyStaticData GetPropStaticData(string propName) => perShaderData?.GetPropStaticData(propName);

		public PropertyStaticData GetPropStaticData(MaterialProperty prop) => GetPropStaticData(prop.name);

		public PropertyDynamicData GetPropDynamicData(string propName) => perMaterialData?.GetPropDynamicData(propName);

		public PropertyDynamicData GetPropDynamicData(MaterialProperty prop) => GetPropDynamicData(prop.name);

		public PropertyInspectorData GetPropInspectorData(string propName) => perInspectorData?.GetPropInspectorData(propName);

		public PropertyInspectorData GetPropInspectorData(MaterialProperty prop) => GetPropInspectorData(prop.name);

		public MaterialProperty GetProperty(string propName) => GetPropDynamicData(propName)?.property;

		public MaterialProperty GetDefaultProperty(string propName) => GetPropDynamicData(propName)?.defualtProperty;
		#endregion

		#region Get Data Tuple
		// var (perShaderData, perMaterialData, perInspectorData) =
		public (PerShaderData, PerMaterialData, PerInspectorData) GetDatas() => (perShaderData, perMaterialData, perInspectorData);

		// var (propStaticData, propDynamicData, propInspectorData) =
		public (PropertyStaticData, PropertyDynamicData, PropertyInspectorData) GetPropDatas(MaterialProperty prop) =>
			(GetPropStaticData(prop), GetPropDynamicData(prop), GetPropInspectorData(prop));
		#endregion

		public MaterialProperty[] GetProps() => perMaterialData.props;

		public Material GetMaterial() => perMaterialData.material;

		public MaterialEditor GetMaterialEditor() => perInspectorData.materialEditor;
	}

	public class MetaDataHelper
	{
		/*
		 * The metadata cache is a structure with three nested dictionaries of Shader / Material / Inspector,
		 * which stores all the data required to render LWGUI by category:
		 * <Shader>
		 * -- PerShaderData
		 * -- <Material>
		 * ---- PerMaterialData
		 * ---- <Inspector>
		 * ------ PerInspectorData
		 */
		private class PerMaterialCache
		{
			public PerMaterialData                     perMaterialData           = null;
			public Dictionary<LWGUI, PerInspectorData> perInspectorDataCachesDic = new Dictionary<LWGUI, PerInspectorData>();
		}

		private class PerShaderCache
		{
			public PerShaderData                          perShaderData            = null;
			public Dictionary<Material, PerMaterialCache> perMaterialDataCachesDic = new Dictionary<Material, PerMaterialCache>();
		}

		private static Dictionary<Shader, PerShaderCache> _perShaderCachesDic = new Dictionary<Shader, PerShaderCache>();

		public static LWGUIMetaDatas BuildMetaDatas(Shader shader, Material material, MaterialEditor materialEditor, LWGUI lwgui, MaterialProperty[] props)
		{
			var outDatas = new LWGUIMetaDatas();

			// perShaderData
			if (!_perShaderCachesDic.ContainsKey(shader))
				_perShaderCachesDic.Add(shader, new PerShaderCache() { perShaderData = new PerShaderData(shader, props) });

			var perShaderCache = _perShaderCachesDic[shader];
			outDatas.perShaderData = perShaderCache.perShaderData;

			// perMaterialData
			if (!perShaderCache.perMaterialDataCachesDic.ContainsKey(material))
				perShaderCache.perMaterialDataCachesDic.Add(material, new PerMaterialCache() { perMaterialData = new PerMaterialData(material, props) });

			var perMaterialCache = perShaderCache.perMaterialDataCachesDic[material];
			outDatas.perMaterialData = perMaterialCache.perMaterialData;
			outDatas.perMaterialData.Update(shader, material, props, outDatas.perShaderData);

			// perInspectorData
			if (!perMaterialCache.perInspectorDataCachesDic.ContainsKey(lwgui))
				perMaterialCache.perInspectorDataCachesDic.Add(lwgui, new PerInspectorData(materialEditor, outDatas.perShaderData, outDatas.perMaterialData));

			outDatas.perInspectorData = perMaterialCache.perInspectorDataCachesDic[lwgui];
			outDatas.perInspectorData.Update(materialEditor);

			return outDatas;
		}

		public static void ReleaseShaderMetadataCache(Shader shader)
		{
			if (shader && _perShaderCachesDic.ContainsKey(shader))
				_perShaderCachesDic.Remove(shader);
		}

		public static void ReleaseMaterialMetadataCache(Material material)
		{
			if (material
			 && material.shader
			 && _perShaderCachesDic.ContainsKey(material.shader)
			 && _perShaderCachesDic[material.shader].perMaterialDataCachesDic.ContainsKey(material))
				_perShaderCachesDic[material.shader].perMaterialDataCachesDic.Remove(material);
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

		public static bool GetPropertyVisibility(MaterialProperty prop, Material material, LWGUIMetaDatas metaDatas)
		{
			bool result = true;

			var (propStaticData, propDynamicData, propInspectorData) = metaDatas.GetPropDatas(prop);
			var displayModeDynamicData = metaDatas.perInspectorData.displayModeDynamicData;
			var cachedModifiedProperties = metaDatas.perInspectorData.displayModeDynamicData.cachedModifiedProperties;
Debug.Log(cachedModifiedProperties?.Count);

			if ( // if HideInInspector
				Helper.IsPropertyHideInInspector(prop)
				// if Search Filtered
			 || !propInspectorData.isSearchMatched
				// if the Conditional Display Keyword is not active
			 || (!string.IsNullOrEmpty(propStaticData.conditionalDisplayKeyword)
			  && !material.shaderKeywords.Any((str => str == propStaticData.conditionalDisplayKeyword)))
			 || (!displayModeDynamicData.showAllHiddenProperties && propStaticData.isHidden)
				// if show modified only
			 || (cachedModifiedProperties != null && !(
					(displayModeDynamicData.showOnlyModifiedProperties && cachedModifiedProperties.ContainsKey(prop.name))
					// if show modified group only
					|| (displayModeDynamicData.showOnlyModifiedGroups && cachedModifiedProperties.ContainsKey(propStaticData.parent != null ? propStaticData.parent.name : prop.name))))
				// ShowIf() == false
			 || !propDynamicData.isShowing
			   )
			{
				result = false;
			}

			return result;
		}

		public static bool GetParentPropertyVisibility(PropertyStaticData parentPropStaticData, Material material, LWGUIMetaDatas metaDatas)
		{
			bool result = true;

			if (parentPropStaticData != null
			 && (!metaDatas.GetPropInspectorData(parentPropStaticData.name).isExpanding
			  || !MetaDataHelper.GetPropertyVisibility(metaDatas.GetProperty(parentPropStaticData.name), material, metaDatas)))
			{
				result = false;
			}

			return result;
		}
	}
}