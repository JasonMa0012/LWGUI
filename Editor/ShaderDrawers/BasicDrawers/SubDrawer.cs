// Copyright (c) Jason Ma

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Draw a property with default style in the folding group
	/// 
	/// group: parent group name (Default: none)
	/// Target Property Type: Any
	/// </summary>
	[LwguiDrawerCategory("Base", -90)]
	[LwguiDrawerParameterString("group", "", "Empty")]
	public class SubDrawer : MaterialPropertyDrawer, IBaseDrawer
	{
		public string         group = String.Empty;
		public LWGUIMetaDatas metaDatas;

		public SubDrawer() { }

		public SubDrawer(string group)
		{
			this.group = group;
		}

		public virtual bool IsMatchPropType(ShaderPropertyType propType) { return true; }

		protected virtual float GetVisibleHeight(MaterialProperty prop)
		{
			var height = MaterialEditor.GetDefaultPropertyHeight(prop);
			if (prop.GetPropertyType() == ShaderPropertyType.Vector)
				return string.IsNullOrEmpty(prop.displayName) ? EditorGUIUtility.singleLineHeight : EditorGUIUtility.singleLineHeight * 2;
			return height;
		}

		public virtual void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.groupName = group;
			PerShaderData.DecodeMetaDataFromDisplayName(inProp, inoutPropertyStaticData);
		}

		public virtual void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData) { }

		public virtual void GetCustomContextMenus(GenericMenu menu, Rect rect, MaterialProperty prop, LWGUIMetaDatas metaDatas)	{ }

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			metaDatas = Helper.GetLWGUIMetadatas(editor);

			if (IsMatchPropType(prop.GetPropertyType()))
			{
				DrawProp(position, prop, label, editor);
			}
			else
			{
				Debug.LogWarning("LWGUI: Property:'" + prop.name + "' Type:'" + prop.GetPropertyType() + "' mismatch!");
				editor.DefaultShaderProperty(position, prop, label.text);
			}
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return GetVisibleHeight(prop);
		}

		// Draws a custom style property
		public virtual void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			editor.LwguiDefaultShaderPropertyInternal(position, prop, label);
		}
	}
}

