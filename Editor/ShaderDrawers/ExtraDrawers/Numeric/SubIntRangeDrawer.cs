// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Similar to builtin IntRange()
	/// 
	/// group: parent group name (Default: none)
	/// Target Property Type: Range
	/// </summary>
	[LwguiDrawerCategory("Numeric")]
	public class SubIntRangeDrawer : SubDrawer
	{
		public SubIntRangeDrawer(string group)
		{
			this.group = group;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.GetPropertyType() == ShaderPropertyType.Range; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			RevertableHelper.FixGUIWidthMismatch(prop.GetPropertyType(), editor);

			if (prop.GetPropertyType() != ShaderPropertyType.Range)
			{
				EditorGUI.LabelField(position, "IntRange used on a non-range property: " + prop.name, EditorStyles.helpBox);
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				EditorGUI.showMixedValue = prop.hasMixedValue;
				float labelWidth = EditorGUIUtility.labelWidth;
				EditorGUIUtility.labelWidth = 0.0f;
				int num = EditorGUI.IntSlider(position, label, (int)prop.floatValue, (int)prop.rangeLimits.x, (int)prop.rangeLimits.y);
				EditorGUI.showMixedValue = false;
				EditorGUIUtility.labelWidth = labelWidth;
				if (Helper.EndChangeCheck(metaDatas, prop))
				{
					prop.floatValue = num;
				}
			}
		}
	}
}

