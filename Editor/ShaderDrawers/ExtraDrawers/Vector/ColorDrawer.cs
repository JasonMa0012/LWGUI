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
	[DrawerCategory("Vector")]
	public class ColorDrawer : SubDrawer
	{
		private string[] _colorStrings = new string[3];

		public ColorDrawer(string group, string color2) : this(group, color2, String.Empty, String.Empty) { }

		public ColorDrawer(string group, string color2, string color3) : this(group, color2, color3, String.Empty) { }

		public ColorDrawer(string group, string color2, string color3, string color4)
		{
			this.group = group;
			this._colorStrings[0] = color2;
			this._colorStrings[1] = color3;
			this._colorStrings[2] = color4;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Color; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			foreach (var colorPropName in _colorStrings)
			{
				inoutPropertyStaticData.AddExtraProperty(colorPropName);
			}
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
				if (p != null && IsMatchPropType(p))
					cProps.Push(p);
			}

			var count = cProps.Count;
			var colorArray = cProps.ToArray();

			EditorGUI.PrefixLabel(position, label);

			for (int i = 0; i < count; i++)
			{
				EditorGUI.BeginChangeCheck();
				var cProp = colorArray[i];
				EditorGUI.showMixedValue = cProp.hasMixedValue;
				var r = new Rect(position);
				var interval = 13 * i * (-0.25f + EditorGUI.indentLevel * 1.25f);
				var w = EditorGUIUtility.fieldWidth * (0.8f + EditorGUI.indentLevel * 0.2f);
				r.xMin += r.width - w * (i + 1) + interval;
				r.xMax -= w * i - interval;

				var src = cProp.colorValue;
				var isHdr = (colorArray[i].GetPropertyFlags() & ShaderPropertyFlags.HDR) != ShaderPropertyFlags.None;
				var dst = EditorGUI.ColorField(r, GUIContent.none, src, true, true, isHdr);
				if (Helper.EndChangeCheck(metaDatas, cProp))
				{
					cProp.colorValue = dst;
				}
			}

			EditorGUI.showMixedValue = false;
		}
	}
}
