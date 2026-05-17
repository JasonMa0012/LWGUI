// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Similar to HideInInspector(), the difference is that Hidden() can be unhidden through the Display Mode button.
	/// </summary>
	[LwguiDrawerCategory("Appearance")]
	public class HiddenDecorator : SubDrawer
	{
		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.isHidden = true;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
}
