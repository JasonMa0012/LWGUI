// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Set the property to read-only.
	/// </summary>
	[LwguiDrawerCategory("Appearance")]
	public class ReadOnlyDecorator : SubDrawer
	{
		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.isReadOnly = true;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
}
