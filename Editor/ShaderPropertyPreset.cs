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
			public string       propertyName;
			public PropertyType propertyType;

// #if UNITY_2021_1_OR_NEWER
// 			public int	   intValue;
// #endif
			public float   floatValue;
			public Color   colorValue;
			public Vector4 vectorValue;
			public Texture textureValue;

			private int propertyNameID = -1;

			public void Apply(Material material)
			{
				if (propertyNameID == -1 || !material.HasProperty(propertyNameID))
					propertyNameID = Shader.PropertyToID(propertyName);
				if (!material.HasProperty(propertyNameID))
				{
					var propertyNameLower = propertyName.ToLower();
					switch (propertyNameLower)
					{
						case "renderqueue":
							material.renderQueue = (int)floatValue;
							return;
						default:
							Debug.LogWarning("Unable to find Preset Property: " + propertyName + " in Material: " + material + "!");
							return;
					}
				}
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
			// [ContextMenuItem("Load Values Form Selected Material", "LoadValuesFormSelectedMaterial")]
			public List<PropertyValue> propertyValues;
			
			public void LoadValuesFormSelectedMaterial()
			{
				if (Selection.activeObject == null || !(Selection.activeObject is Material))
				{
					Debug.LogError("Please lock the Preset Window and select a Material in the Project Window!");
					return;
				}
				var material = Selection.activeObject as Material;
				var props = MaterialEditor.GetMaterialProperties(new[] { material });
				foreach (var propertyValue in this.propertyValues)
				{
					var prop = Array.Find(props, property => property.name == propertyValue.propertyName);
					propertyValue.CopyFromMaterialProperty(prop);
				}
			}
		}

		public List<Preset> presets;

		// private void Awake()
		// {
		// 	Debug.Log($"{this.name} Awake");
		// }
		//
		// private void OnDestroy()
		// {
		// 	Debug.Log($"{this.name} OnDestroy");
		// }
		//
		// private void OnDisable()
		// {
		// 	Debug.Log($"{this.name} OnDisable");
		// }
		//
		// private void Reset()
		// {
		// 	Debug.Log($"{this.name} Reset");
		// }
		
		private void OnValidate()
		{
			// Debug.Log($"{this.name} OnValidate");
			PresetHelper.ForceInit();
			RevertableHelper.ForceInit();
		}
		
		private void OnEnable()
		{
			// Debug.Log($"{this.name} OnEnable");
			// manually added when a preset is manually created
			if (PresetHelper.IsInitComplete)
				PresetHelper.AddPreset(this);
		}

		public void Apply(Material material, int presetIndex)
		{
			Apply(new []{material}, presetIndex);
		}
		
		public void Apply(Material[] materials, int presetIndex)
		{
			if (this.presets.Count == 0) return;
			
			int index = (int)Mathf.Min(this.presets.Count - 1, Mathf.Max(0, presetIndex));
			foreach (var propertyValue in this.presets[index].propertyValues)
			{
				foreach (var material in materials)
				{
					propertyValue.Apply(material);
				}
			}
		}

		public Preset GetPreset(string presetName)
		{
			return presets.Find((inPreset => inPreset.presetName == presetName));
		}

		public Preset GetPreset(MaterialProperty property)
		{
			if (property.floatValue < presets.Count)
				return presets[(int)property.floatValue];
			else
			{
				Debug.LogError("Preset Property: " + property.name + " Index Out Of Range!");
				return null;
			}
		}
	}
}