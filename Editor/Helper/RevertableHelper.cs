// Copyright (c) Jason Ma

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Helpers for drawing Unreal Style Revertable Shader GUI 
	/// </summary>
	public class RevertableHelper
	{
		public static readonly float revertButtonWidth = 15f;


		#region GUI Setting

		public static Rect IndentRect(Rect rect)
		{
			rect.xMax -= revertButtonWidth;
			return rect;
		}

		public static Rect SplitRevertButtonRect(ref Rect rect, bool isCallInDrawer = false)
		{
			float defaultHeightWithoutDrawers = EditorGUIUtility.singleLineHeight;
			var revertButtonRect = GetRevertButtonRect(defaultHeightWithoutDrawers, rect, isCallInDrawer);
			rect = IndentRect(rect);
			return revertButtonRect;
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

		#endregion


		#region Property Handle

		public static void SetPropertyToDefault(MaterialProperty defaultProp, MaterialProperty prop)
		{
			prop.vectorValue = defaultProp.vectorValue;
			prop.colorValue = defaultProp.colorValue;
			prop.floatValue = defaultProp.floatValue;
			prop.textureValue = defaultProp.textureValue;
			prop.intValue = defaultProp.intValue;
		}

		public static string GetPropertyDefaultValueText(MaterialProperty defaultProp)
		{
			string defaultText = String.Empty;
			switch (defaultProp.GetPropertyType())
			{
				case ShaderPropertyType.Color:
					defaultText = defaultProp.colorValue.ToString();
					break;
				case ShaderPropertyType.Float:
				case ShaderPropertyType.Range:
					defaultText = defaultProp.floatValue.ToString();
					break;
				case ShaderPropertyType.Int:
					defaultText = defaultProp.intValue.ToString();
					break;
				case ShaderPropertyType.Texture:
					defaultText = defaultProp.textureValue != null ? defaultProp.textureValue.name : "None";
					break;
				case ShaderPropertyType.Vector:
					defaultText = defaultProp.vectorValue.ToString();
					break;
			}
			return defaultText;
		}

		#endregion


		#region Draw revert button

		public static bool DrawRevertableProperty(Rect position, MaterialProperty prop, LWGUIMetaDatas metaDatas, bool isHeader = false)
		{
			var (propStaticData, propDynamicData) = metaDatas.GetPropDatas(prop);

			bool hasModified = prop.hasMixedValue
							|| propDynamicData.hasModified
							|| (isHeader && propDynamicData.hasChildrenModified);

			if (!hasModified)
				return false;

			Rect rect = position;
			if (DrawRevertButton(rect))
			{
				GUI.changed = true;
				EditorGUI.FocusTextInControl(string.Empty);
				DoRevertProperty(prop, metaDatas);

				if (isHeader)
				{
					foreach (var childStaticData in propStaticData.children)
					{
						DoRevertProperty(metaDatas.GetProperty(childStaticData.name), metaDatas);
						foreach (var childChildStaticData in childStaticData.children)
							DoRevertProperty(metaDatas.GetProperty(childChildStaticData.name), metaDatas);
					}
				}

				return true;
			}
			return false;
		}

		private static void DoRevertProperty(MaterialProperty prop, LWGUIMetaDatas metaDatas)
		{
			var propDynamicData = metaDatas.GetPropDynamicData(prop.name);
			propDynamicData.hasRevertChanged = true;
			SetPropertyToDefault(propDynamicData.defaultProperty, prop);
			foreach (var extraPropName in metaDatas.GetPropStaticData(prop.name).extraPropNames)
			{
				var extraPropDynamicData = metaDatas.GetPropDynamicData(extraPropName);
				extraPropDynamicData.hasRevertChanged = true;
				SetPropertyToDefault(extraPropDynamicData.defaultProperty, extraPropDynamicData.property);
			}
		}

		private static Texture _iconCache;
		private static Texture _icon => _iconCache = _iconCache ?? AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("e7bc1130858d984488bca32b8512ca96"));

		public static bool DrawRevertButton(Rect rect)
		{
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
	}
}