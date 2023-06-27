using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LWGUI
{
	/// <summary>
	/// Helpers for drawing Unreal Style Revertable Shader GUI 
	/// </summary>
	public class RevertableHelper
	{
		public static readonly float revertButtonWidth = 15f;
		public static          float fieldWidth;
		public static          float labelWidth;

		private static Dictionary<Material /*Material*/, Dictionary<string /*Prop Name*/, MaterialProperty /*Prop*/>>
			_defaultProps = new Dictionary<Material, Dictionary<string, MaterialProperty>>();

		private static Dictionary<Shader, DateTime> _lastShaderModifiedTime = new Dictionary<Shader, DateTime>();
		private static Dictionary<Material, Shader> _lastShaders            = new Dictionary<Material, Shader>();
		private static bool                         _forceInit;


		#region Init

		private static void CheckProperty(Material material, MaterialProperty prop)
		{
			if (!(_defaultProps.ContainsKey(material) && _defaultProps[material].ContainsKey(prop.name)))
			{
				Debug.LogWarning("Uninitialized Shader:" + material.name + "or Prop:" + prop.name);
				LWGUI.ForceInit();
			}
		}

		public static void ForceInit() { _forceInit = true; }

		/// <summary>
		/// Detect Shader changes to know when to initialize
		/// </summary>
		public static bool InitAndHasShaderModified(Shader shader, Material material, MaterialProperty[] props)
		{
			var shaderPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + AssetDatabase.GetAssetPath(shader);
			Debug.Assert(File.Exists(shaderPath), "Unable to find Shader: " + shader.name + " in " + shaderPath + "!");

			var currTime = (new FileInfo(shaderPath)).LastWriteTime;

			// check for init
			if (_forceInit
			 || !_lastShaderModifiedTime.ContainsKey(shader)
			 || _lastShaderModifiedTime[shader] != currTime
			 || !_defaultProps.ContainsKey(material)
			 || !_lastShaders.ContainsKey(material)
			 || _lastShaders[material] != shader
			   )
			{
				if (_lastShaderModifiedTime.ContainsKey(shader) && _lastShaderModifiedTime[shader] != currTime)
					AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(shader));

				_forceInit = false;
				_lastShaders[material] = shader;
				_lastShaderModifiedTime[shader] = currTime;
			}
			else
				return false;

			// Get and cache new props
			var defaultMaterial = new Material(shader);
			PresetHelper.ApplyPresetValue(props, defaultMaterial);
			var newProps = MaterialEditor.GetMaterialProperties(new[] { defaultMaterial });
			Debug.Assert(newProps.Length == props.Length);

			_defaultProps[material] = new Dictionary<string, MaterialProperty>();
			foreach (var prop in newProps)
			{
				_defaultProps[material][prop.name] = prop;
			}

			return true;
		}

		#endregion


		#region GUI Setting

		public static Rect GetRevertButtonRect(MaterialProperty prop, Rect rect, bool isCallInDrawer = false)
		{
			// TODO: use Reflection
			float defaultHeightWithoutDrawers = EditorGUIUtility.singleLineHeight;
			return GetRevertButtonRect(defaultHeightWithoutDrawers, rect, isCallInDrawer);
		}

		public static Rect GetRevertButtonRect(float propHeight, Rect rect, bool isCallInDrawer = false)
		{
			if (isCallInDrawer) rect.xMax += revertButtonWidth;
			var revertButtonRect = new Rect(rect.xMax - revertButtonWidth + 2f,
											rect.yMax - propHeight * 0.5f - revertButtonWidth * 0.5f,
											revertButtonWidth - 2f,
											revertButtonWidth - 3f);
			return revertButtonRect;
		}

		public static void SetRevertableGUIWidths()
		{
			EditorGUIUtility.fieldWidth = RevertableHelper.fieldWidth;
			EditorGUIUtility.labelWidth = RevertableHelper.labelWidth;
		}

		#endregion


		#region Property Handle

		public static void SetPropertyToDefault(MaterialProperty defaultProp, MaterialProperty prop)
		{
			prop.vectorValue = defaultProp.vectorValue;
			prop.colorValue = defaultProp.colorValue;
			prop.floatValue = defaultProp.floatValue;
			prop.textureValue = defaultProp.textureValue;
#if UNITY_2021_1_OR_NEWER
			prop.intValue = defaultProp.intValue;
#endif
		}

		public static void SetPropertyToDefault(Material material, MaterialProperty prop)
		{
			CheckProperty(material, prop);
			var defaultProp = _defaultProps[material][prop.name];
			SetPropertyToDefault(defaultProp, prop);
		}

		public static MaterialProperty GetDefaultProperty(Material material, MaterialProperty prop)
		{
			CheckProperty(material, prop);
			return _defaultProps[material][prop.name];
		}

		public static string GetPropertyDefaultValueText(Material material, MaterialProperty prop)
		{
			var defaultProp = GetDefaultProperty(material, prop);
			string defaultText = String.Empty;
			switch (defaultProp.type)
			{
				case MaterialProperty.PropType.Color:
					defaultText += defaultProp.colorValue;
					break;
				case MaterialProperty.PropType.Float:
				case MaterialProperty.PropType.Range:
					defaultText += defaultProp.floatValue;
					break;
#if UNITY_2021_1_OR_NEWER
				case MaterialProperty.PropType.Int:
					defaultText += defaultProp.intValue;
					break;
#endif
				case MaterialProperty.PropType.Texture:
					defaultText += defaultProp.textureValue != null ? defaultProp.textureValue.name : "None";
					break;
				case MaterialProperty.PropType.Vector:
					defaultText += defaultProp.vectorValue;
					break;
			}
			return defaultText;
		}

		public static bool IsDefaultProperty(Material material, MaterialProperty prop)
		{
			CheckProperty(material, prop);
			return Helper.PropertyValueEquals(prop, _defaultProps[material][prop.name]);
		}

		#endregion


		#region Draw revert button

		public static bool DrawRevertableProperty(Rect position, MaterialProperty prop, MaterialEditor materialEditor)
		{
			var material = materialEditor.target as Material;
			CheckProperty(material, prop);
			var defaultProp = _defaultProps[material][prop.name];
			Rect rect = position;
			if (Helper.PropertyValueEquals(prop, defaultProp) && !prop.hasMixedValue)
				return false;
			if (DrawRevertButton(rect))
			{
				AddPropertyShouldRevert(prop.targets, prop.name);
				SetPropertyToDefault(defaultProp, prop);
				// refresh keywords
				MaterialEditor.ApplyMaterialPropertyDrawers(materialEditor.targets);
				return true;
			}
			return false;
		}

		private static readonly Texture _icon = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("e7bc1130858d984488bca32b8512ca96"));

		public static bool DrawRevertButton(Rect rect)
		{
			if (_icon == null) Debug.LogError("RevertIcon.png + meta is missing!");
			GUI.DrawTexture(rect, _icon);
			var e = Event.current;
			if (e.type == UnityEngine.EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				e.Use();
				return true;
			}
			return false;
		}

		#endregion


		#region Call drawers to do revert and refresh keywords

		private static Dictionary<Object, List<string>> _shouldRevertPropsPool;

		public static void AddPropertyShouldRevert(Object[] materials, string propName)
		{
			if (_shouldRevertPropsPool == null)
				_shouldRevertPropsPool = new Dictionary<Object, List<string>>();
			foreach (var material in materials)
			{
				if (_shouldRevertPropsPool.ContainsKey(material))
				{
					if (!_shouldRevertPropsPool[material].Contains(propName))
						_shouldRevertPropsPool[material].Add(propName);
				}
				else
				{
					_shouldRevertPropsPool.Add(material, new List<string> { propName });
				}
			}
		}

		public static void RemovePropertyShouldRevert(Object[] materials, string propName)
		{
			if (_shouldRevertPropsPool == null) return;
			foreach (var material in materials)
			{
				if (_shouldRevertPropsPool.ContainsKey(material))
				{
					if (_shouldRevertPropsPool[material].Contains(propName))
						_shouldRevertPropsPool[material].Remove(propName);
				}
			}
		}

		public static bool IsPropertyShouldRevert(Object material, string propName)
		{
			if (_shouldRevertPropsPool == null) return false;
			if (_shouldRevertPropsPool.ContainsKey(material))
			{
				return _shouldRevertPropsPool[material].Contains(propName);
			}
			else
			{
				return false;
			}
		}

		#endregion
	}
}