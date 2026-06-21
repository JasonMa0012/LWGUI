// Copyright (c) Jason Ma

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Draw a Texture property in single line with a extra property
	/// 
	/// group: parent group name (Default: none)
	/// extraPropName: extra property name (Default: none)
	/// Target Property Type: Texture
	/// Extra Property Type: Color, Vector
	/// Target Property Type: Texture2D
	/// </summary>
	[LwguiDrawerCategory("Texture")]
	public class TexDrawer : SubDrawer
	{
		private string        _extraPropName = String.Empty;
		private ChannelDrawer _channelDrawer = new ChannelDrawer();

		public TexDrawer() { }

		public TexDrawer(string group) : this(group, String.Empty) { }

		public TexDrawer(string group, string extraPropName)
		{
			this.group = group;
			this._extraPropName = extraPropName;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight; }

		public override bool IsMatchPropType(ShaderPropertyType propType) { return propType == ShaderPropertyType.Texture; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			inoutPropertyStaticData.AddExtraProperty(_extraPropName);
			inoutPropertyStaticData.labelWidth = Mathf.Max(inoutPropertyStaticData.labelWidth,
				EditorStyles.label.CalcSize(new GUIContent(inoutPropertyStaticData.displayName)).x + 32f);
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			var defaultExtraProp = inoutPerMaterialData.GetPropDynamicData(_extraPropName)?.defaultProperty;
			if (defaultExtraProp != null)
			{
				var text = string.Empty;
				if (defaultExtraProp.GetPropertyType() == ShaderPropertyType.Vector)
					text = ChannelDrawer.GetChannelName(defaultExtraProp);
				else
					text = RevertableHelper.GetPropertyDefaultValueText(defaultExtraProp);

				inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription =
					RevertableHelper.GetPropertyDefaultValueText(inDefaultProp) + ", " + text;
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position;

			MaterialProperty extraProp = metaDatas.GetProperty(_extraPropName);
			if (extraProp != null)
			{
				var i = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				
				var extraRect = MaterialEditor.GetRectAfterLabelWidth(rect);
				extraRect.xMin += 2f;

				if (extraProp.GetPropertyType() == ShaderPropertyType.Vector)
					_channelDrawer.OnGUI(extraRect, extraProp, GUIContent.none, editor);
				else
					editor.LwguiShaderProperty(extraRect, extraProp, GUIContent.none);

				EditorGUI.indentLevel = i;
			}

			editor.TexturePropertyMiniThumbnail(rect, prop, label.text, label.tooltip);

			EditorGUI.showMixedValue = false;
		}
	}
}

