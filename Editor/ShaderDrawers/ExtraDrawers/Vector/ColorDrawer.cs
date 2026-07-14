// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Display up to 4 colors in a single line
	///
	/// group: parent group name (Default: none)
	/// color2-4: extra color property name
	/// Target Property Type: Color
	/// </summary>
	[LwguiDrawerCategory("Vector")]
	public class ColorDrawer : SubDrawer
	{
		private string[]        _colorStrings = new string[3];
		private static readonly float _minColorWidth = 50f;

		public ColorDrawer(string group, string color2) : this(group, color2, String.Empty, String.Empty) { }

		public ColorDrawer(string group, string color2, string color3) : this(group, color2, color3, String.Empty) { }

		public ColorDrawer(string group, string color2, string color3, string color4)
		{
			this.group = group;
			this._colorStrings[0] = color2;
			this._colorStrings[1] = color3;
			this._colorStrings[2] = color4;
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) { return propType == ShaderPropertyType.Color; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			foreach (var colorPropName in _colorStrings)
			{
				inoutPropertyStaticData.AddExtraProperty(colorPropName);
			}
		}

		private int GetColorCount(MaterialProperty prop, LWGUIMetaDatas inMetaDatas)
		{
			if (inMetaDatas == null)
				return 1;

			int count = 1;
			foreach (var colorPropName in _colorStrings)
			{
				if (string.IsNullOrEmpty(colorPropName))
					continue;
				var p = inMetaDatas.GetProperty(colorPropName);
				if (p != null && IsMatchPropType(p.GetPropertyType()))
					count++;
			}
			return count;
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			var lwgui = editor != null ? Helper.GetLWGUI(editor) : null;
			var count = GetColorCount(prop, lwgui?.metaDatas);
			return count >= 3 ? EditorGUIUtility.singleLineHeight * 2 : EditorGUIUtility.singleLineHeight;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var cProps = new Stack<MaterialProperty>();
			for (int i = 0; i < 4; i++)
			{
				if (i == 0)
				{
					cProps.Push(prop);
					continue;
				}

				var p = metaDatas.GetProperty(_colorStrings[i - 1]);
				if (p != null && IsMatchPropType(p.GetPropertyType()))
					cProps.Push(p);
			}

			var count = cProps.Count;
			var colorArray = cProps.ToArray();

			int controlId = GUIUtility.GetControlID(FocusType.Keyboard, position);
			var fieldRect = EditorGUI.PrefixLabel(position, controlId, label);

			var spacing = 2f;
			// Similar to BitMaskDrawer: when each color is too narrow (< _minColorWidth), move all of them to the second row.
			var needSecondRow = count >= 3 && (fieldRect.width - spacing * (count - 1)) / count < _minColorWidth;
			if (needSecondRow)
				fieldRect = new Rect(position.x + ReflectionHelper.EditorGUI_indent,
					                 position.y + EditorGUIUtility.singleLineHeight,
					                 position.width - ReflectionHelper.EditorGUI_indent,
					                 EditorGUIUtility.singleLineHeight);
			else
				fieldRect.height = EditorGUIUtility.singleLineHeight;

			var colorWidth = (fieldRect.width - spacing * (count - 1)) / count;

			var indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;

			for (int i = 0; i < count; i++)
			{
				EditorGUI.BeginChangeCheck();
				var cProp = colorArray[i];
				EditorGUI.showMixedValue = cProp.hasMixedValue;
				var r = new Rect(fieldRect);
				r.xMin = fieldRect.xMin + i * (colorWidth + spacing);
				r.width = colorWidth;

				var src = cProp.colorValue;
				var isHdr = (colorArray[i].GetPropertyFlags() & ShaderPropertyFlags.HDR) != ShaderPropertyFlags.None;
				var dst = EditorGUI.ColorField(r, GUIContent.none, src, true, true, isHdr);
				if (Helper.EndChangeCheck(metaDatas, cProp))
				{
					cProp.colorValue = dst;
				}
			}

			EditorGUI.indentLevel = indentLevel;
			EditorGUI.showMixedValue = false;
		}
	}
}
