// Copyright (c) 2022 Jason Ma
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

namespace LWGUI
{
	[CreateAssetMenu(fileName = "LWGUI_ShaderPropertyPreset.asset", menuName = "LWGUI/Shader Property Preset")]
	public class ShaderPropertyPreset : ScriptableObject
	{
		public enum PropertyType
		{
			Color,
			Vector,
			Float,
			Range,
			Texture,
		}

		[Serializable]
		public class PropertyValue
		{
			public PropertyValue(MaterialProperty prop)
			{
				CopyFromMaterialProperty(prop);
			}

			public string       propertyName;
			public PropertyType propertyType;
			public float        floatValue;
			public Color        colorValue;
			public Vector4      vectorValue;
			public Texture      textureValue;

			private int propertyNameID = -1;

			public void Apply(Material material, bool isDefaultMaterial, PerFrameData perFrameData = null)
			{
				if (propertyNameID == -1 || !material.HasProperty(propertyNameID))
					propertyNameID = Shader.PropertyToID(propertyName);
				if (!material.HasProperty(propertyNameID))
				{
					// Legacy
					var propertyNameLower = propertyName.ToLower();
					switch (propertyNameLower)
					{
						case "renderqueue":
							material.renderQueue = (int)floatValue;
							return;
						default:
							// Debug.LogWarning("Unable to find Preset Property: " + propertyName + " in Material: " + material + "!");
							return;
					}
				}


				// Must be modified MaterialProperty directly for the material editing in ShaderGUI.
				// For the Material in background, just use Material.SetXXX().
				var isPropertyOtherMaterials = !isDefaultMaterial && perFrameData == null;
				if (isPropertyOtherMaterials || isDefaultMaterial)
				{
					switch (propertyType)
					{
						case PropertyType.Color:
							material.SetColor(propertyNameID, colorValue);
							break;
						case PropertyType.Vector:
							material.SetVector(propertyNameID, vectorValue);
							break;
						case PropertyType.Float:
						case PropertyType.Range:
							material.SetFloat(propertyNameID, floatValue);
							break;
						case PropertyType.Texture:
							material.SetTexture(propertyNameID, textureValue);
							break;
					}

					if (isPropertyOtherMaterials)
						MaterialEditor.ApplyMaterialPropertyDrawers(material);
				}
				else
				// is Property Primary Material
				{
					var propDynamicData = perFrameData.propertyDatas[propertyName];
					var prop = propDynamicData.property;
					switch (propertyType)
					{
						case PropertyType.Color:
							prop.colorValue = colorValue;
							break;
						case PropertyType.Vector:
							prop.vectorValue = vectorValue;
							break;
						case PropertyType.Float:
						case PropertyType.Range:
							prop.floatValue = floatValue;
							break;
						case PropertyType.Texture:
							prop.textureValue = textureValue;
							break;
					}

					propDynamicData.hasRevertChanged = true;
				}
			}

			public void CopyFromMaterialProperty(MaterialProperty prop)
			{
				propertyName = prop.name;
				switch (prop.type)
				{
					case MaterialProperty.PropType.Color:
						propertyType = PropertyType.Color;
						colorValue = prop.colorValue;
						break;
					case MaterialProperty.PropType.Vector:
						propertyType = PropertyType.Vector;
						vectorValue = prop.vectorValue;
						break;
#if UNITY_2021_1_OR_NEWER
					case MaterialProperty.PropType.Int:   
#endif
					case MaterialProperty.PropType.Float:   
						propertyType = PropertyType.Float;
						floatValue = prop.floatValue;
						break;
					case MaterialProperty.PropType.Range:   
						propertyType = PropertyType.Range;
						floatValue = prop.floatValue;
						break;
					case MaterialProperty.PropType.Texture: 
						propertyType = PropertyType.Texture;
						textureValue = prop.textureValue;
						break;
				}
			}

			public void OnValidate()
			{
				propertyNameID = -1;
			}
		}

		[Serializable]
		public class Preset
		{
			public string              presetName;
			public List<PropertyValue> propertyValues   = new List<PropertyValue>();
			public List<string>        enabledKeywords  = new List<string>();
			public List<string>        disabledKeywords = new List<string>();
			public int                 renderQueue      = -1;


			public void ApplyToDefaultMaterial(Material material)
			{
				foreach (var propertyValue in propertyValues)
					propertyValue.Apply(material, true);
				foreach (var enabledKeyword in enabledKeywords)
					material.EnableKeyword(enabledKeyword);
				foreach (var disabledKeyword in disabledKeywords)
					material.DisableKeyword(disabledKeyword);
				if (renderQueue >= 0)
					material.renderQueue = renderQueue;
			}

			public void ApplyToEditingMaterial(UnityEngine.Object[] materials, PerFrameData perFrameData)
			{
				for (int i = 0; i < materials.Length; i++)
				{
					var material = materials[i] as Material;
					foreach (var propertyValue in propertyValues)
						propertyValue.Apply(material, false, i == 0 ? perFrameData : null);
					foreach (var enabledKeyword in enabledKeywords)
						material.EnableKeyword(enabledKeyword);
					foreach (var disabledKeyword in disabledKeywords)
						material.DisableKeyword(disabledKeyword);
					if (renderQueue >= 0)
						material.renderQueue = renderQueue;
				}
			}

			public void ApplyKeywordsToMaterials(UnityEngine.Object[] materials)
			{
				for (int i = 0; i < materials.Length; i++)
				{
					var material = materials[i] as Material;
					foreach (var enabledKeyword in enabledKeywords)
						material.EnableKeyword(enabledKeyword);
					foreach (var disabledKeyword in disabledKeywords)
						material.DisableKeyword(disabledKeyword);
				}
			}

			public PropertyValue GetPropertyValue(string propName)
			{
				PropertyValue result = null;
				if (propertyValues != null)
				{
					foreach (var propertyValue in propertyValues)
					{
						if (propertyValue.propertyName == propName)
						{
							result = propertyValue;
							break;
						}
					}
				}
				return result;
			}

			public void AddOrUpdate(MaterialProperty prop)
			{
				var propertyValue = GetPropertyValue(prop.name);
				if (propertyValue != null)
					propertyValue.CopyFromMaterialProperty(prop);
				else
					propertyValues.Add(new PropertyValue(prop));
			}

			public void AddOrUpdateIncludeExtraProperties(LWGUI lwgui, MaterialProperty prop)
			{
				AddOrUpdate(prop);
				foreach (var extraPropName in lwgui.perShaderData.propertyDatas[prop.name].extraPropNames)
				{
					AddOrUpdate(lwgui.perFrameData.propertyDatas[extraPropName].property);
				}
			}

			public void Remove(string propName)
			{
				var propertyValue = GetPropertyValue(propName);
				if (propertyValue != null)
					propertyValues.Remove(propertyValue);
			}

			public void RemoveIncludeExtraProperties(LWGUI lwgui, string propName)
			{
				Remove(propName);
				foreach (var extraPropName in lwgui.perShaderData.propertyDatas[propName].extraPropNames)
				{
					Remove(lwgui.perFrameData.propertyDatas[extraPropName].property.name);
				}
			}
		}


		public List<Preset> presets;

		
		private void OnValidate()
		{
			PresetHelper.ForceInit();
		}
		
		private void OnEnable()
		{
			if (PresetHelper.IsInitComplete)
				PresetHelper.AddPreset(this);
		}
	}
}