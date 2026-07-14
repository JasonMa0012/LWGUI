// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Draw a R/G/B/A drop menu:
	/// 	R = (1, 0, 0, 0)
	/// 	G = (0, 1, 0, 0)
	/// 	B = (0, 0, 1, 0)
	/// 	A = (0, 0, 0, 1)
	/// 	RGB Average = (1f / 3f, 1f / 3f, 1f / 3f, 0)
	/// 	RGB Luminance = (0.2126f, 0.7152f, 0.0722f, 0)
	///		None = (0, 0, 0, 0)
	/// 
	/// group: parent group name (Default: none)
	/// Target Property Type: Vector, used to dot() with Texture Sample Value
	/// </summary>
	[LwguiDrawerCategory("Vector")]
	public class ChannelDrawer : SubDrawer
	{
		private static GUIContent[] _names = new[]
		{
			new GUIContent("R"),
			new GUIContent("G"),
			new GUIContent("B"),
			new GUIContent("A"),
			new GUIContent("RGB Average"),
			new GUIContent("RGB Luminance"),
			new GUIContent("None")
		};
		private static int[] _intValues = new int[] { 0, 1, 2, 3, 4, 5, 6 };
		private static Vector4[] _vector4Values = new[]
		{
			new Vector4(1, 0, 0, 0),
			new Vector4(0, 1, 0, 0),
			new Vector4(0, 0, 1, 0),
			new Vector4(0, 0, 0, 1),
			new Vector4(1f / 3f, 1f / 3f, 1f / 3f, 0),
			new Vector4(0.2126f, 0.7152f, 0.0722f, 0),
			new Vector4(0, 0, 0, 0)
		};

		public ChannelDrawer() { }

		public ChannelDrawer(string group)
		{
			this.group = group;
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) { return propType == ShaderPropertyType.Vector; }

		protected override float GetVisibleHeight(MaterialProperty prop) => EditorGUIUtility.singleLineHeight;

		private static int GetChannelIndex(MaterialProperty prop)
		{
			int index = -1;
			for (int i = 0; i < _vector4Values.Length; i++)
			{
				if (prop.vectorValue == _vector4Values[i])
					index = i;
			}
			if (index == -1)
			{
				Debug.LogError("LWGUI: Channel Property: " + prop.name + " invalid vector found, reset to A");
				prop.vectorValue = _vector4Values[3];
				index = 3;
			}
			return index;
		}

		public static string GetChannelName(MaterialProperty prop)
		{
			return _names[GetChannelIndex(prop)].text;
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription = GetChannelName(inDefaultProp);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();

			EditorGUI.showMixedValue = prop.hasMixedValue;
			var index = GetChannelIndex(prop);
			int num = EditorGUI.IntPopup(position, label, index, _names, _intValues);
			EditorGUI.showMixedValue = false;
			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				prop.vectorValue = _vector4Values[num];
			}
		}
	}
}
