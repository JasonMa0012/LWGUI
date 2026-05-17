// Copyright (c) Jason Ma

using System;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Similar to Header()
	/// 
	/// group: parent group name (Default: none)
	/// header: string to display, "SpaceLine" or "_" = none (Default: none)
	/// height: line height (Default: 24)
	/// </summary>
	[LwguiDrawerCategory("Appearance")]
	[LwguiDrawerParameterString("group", "_", "Empty")]
	[LwguiDrawerParameterString("header", "", "Empty")]
	[LwguiDrawerParameterFloat("height", 24f, "24")]
	public class TitleDecorator : SubDrawer
	{
		private string _header;
		private float  _height;

		public static readonly float DefaultHeight = EditorGUIUtility.singleLineHeight + 6f;

		protected override float GetVisibleHeight(MaterialProperty prop) { return _height; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData) { }

		public TitleDecorator(string header) : this("_", header, DefaultHeight) { }

		public TitleDecorator(string header, float height) : this("_", header, height) { }

		public TitleDecorator(string group, string header) : this(group, header, DefaultHeight) { }

		public TitleDecorator(string group, string header, float height)
		{
			this.group = group;
			this._header = header == "SpaceLine" || header == "_" ? String.Empty : header;
			this._height = height;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			position = EditorGUI.IndentedRect(position);
			GUI.Label(position, _header, GUIStyles.title);
		}
	}
}
