// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Create an Advanced Block using the current Property as the Header.
	/// </summary>
	[LwguiDrawerCategory("Structure")]
	public class AdvancedHeaderPropertyDecorator : SubDrawer
	{
		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.isAdvanced = true;
			inoutPropertyStaticData.isAdvancedHeader = true;
			inoutPropertyStaticData.isAdvancedHeaderProperty = true;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
}
