// Copyright (c) Jason Ma
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	public delegate void LWGUICustomGUIEvent(LWGUI lwgui);

	public class LWGUI : ShaderGUI
	{
		public MaterialProperty[] props;
		public MaterialEditor     materialEditor;
		public Material           material;
		public Shader             shader;
		public PerShaderData      perShaderData;
		public PerFrameData       perFrameData;

		public static LWGUICustomGUIEvent onDrawCustomHeader;
		public static LWGUICustomGUIEvent onDrawCustomFooter;

		/// <summary>
		/// Called when switch to a new Material Window, each window has a LWGUI instance
		/// </summary>
		public LWGUI() { }

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
		{
			this.props = props;
			this.materialEditor = materialEditor;
			this.material = materialEditor.target as Material;
			this.shader = this.material.shader;
			this.perShaderData = MetaDataHelper.BuildPerShaderData(shader, props);
			this.perFrameData = MetaDataHelper.BuildPerFrameData(shader, material, props);


			// Custom Header
			if (onDrawCustomHeader != null)
				onDrawCustomHeader(this);


			// Toolbar
			bool enabled = GUI.enabled;
			GUI.enabled = true;
			var toolBarRect = EditorGUILayout.GetControlRect();
			toolBarRect.xMin = 2;

			Helper.DrawToolbarButtons(ref toolBarRect, this);

			Helper.DrawSearchField(toolBarRect, this);

			GUILayoutUtility.GetRect(0, 0); // Space(0)
			GUI.enabled = enabled;
			Helper.DrawSplitLine();


			// Properties
			{
				// move fields left to make rect for Revert Button
				materialEditor.SetDefaultGUIWidths();
				RevertableHelper.InitRevertableGUIWidths();

				// start drawing properties
				foreach (var prop in props)
				{
					var propStaticData = perShaderData.propertyDatas[prop.name];
					var propDynamicData = perFrameData.propertyDatas[prop.name];

					// Visibility
					{
						if (!MetaDataHelper.GetPropertyVisibility(prop, material, this))
							continue;

						if (propStaticData.parent != null
							&& (!MetaDataHelper.GetParentPropertyVisibility(propStaticData.parent, material, this)
								|| !MetaDataHelper.GetParentPropertyVisibility(propStaticData.parent.parent, material, this)))
							continue;
					}

					// Indent
					var indentLevel = EditorGUI.indentLevel;
					if (propStaticData.isAdvancedHeader)
						EditorGUI.indentLevel++;
					if (propStaticData.parent != null)
					{
						EditorGUI.indentLevel++;
						if (propStaticData.parent.parent != null)
							EditorGUI.indentLevel++;
					}

					// Advanced Header
					if (propStaticData.isAdvancedHeader && !propStaticData.isAdvancedHeaderProperty)
					{
						DrawAdvancedHeader(propStaticData, prop);

						if (!propStaticData.isExpanding)
						{
							RevertableHelper.SetRevertableGUIWidths();
							EditorGUI.indentLevel = indentLevel;
							continue;
						}
					}

					DrawProperty(prop);

					RevertableHelper.SetRevertableGUIWidths();
					EditorGUI.indentLevel = indentLevel;
				}

				materialEditor.SetDefaultGUIWidths();
			}


			EditorGUILayout.Space();
			Helper.DrawSplitLine();
			EditorGUILayout.Space();


			// Render settings
#if UNITY_2019_4_OR_NEWER
			if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
#endif
			{
				materialEditor.RenderQueueField();
			}
			materialEditor.EnableInstancingField();
			materialEditor.LightmapEmissionProperty();
			materialEditor.DoubleSidedGIField();


			// Custom Footer
			if (onDrawCustomFooter != null)
				onDrawCustomFooter(this);


			// LOGO
			EditorGUILayout.Space();
			Helper.DrawLogo();
		}

		private void DrawAdvancedHeader(PropertyStaticData propStaticData, MaterialProperty prop)
		{
			var rect = EditorGUILayout.GetControlRect();
			var revertButtonRect = RevertableHelper.SplitRevertButtonRect(ref rect);
			var label = string.IsNullOrEmpty(propStaticData.advancedHeaderString) ? "Advanced" : propStaticData.advancedHeaderString;
			propStaticData.isExpanding = EditorGUI.Foldout(rect, propStaticData.isExpanding, label);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
				propStaticData.isExpanding = !propStaticData.isExpanding;
			RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, this, true);
			Helper.DoPropertyContextMenus(rect, prop, this);
		}

		private void DrawProperty(MaterialProperty prop)
		{
			var propStaticData = perShaderData.propertyDatas[prop.name];
			var propDynamicData = perFrameData.propertyDatas[prop.name];

			Helper.DrawHelpbox(propStaticData, propDynamicData);

			var label = new GUIContent(propStaticData.displayName, MetaDataHelper.GetPropertyTooltip(propStaticData, propDynamicData));
			var height = materialEditor.GetPropertyHeight(prop, label.text);
			var rect = EditorGUILayout.GetControlRect(true, height);

			var revertButtonRect = RevertableHelper.SplitRevertButtonRect(ref rect);

			var enabled = GUI.enabled;
			if (propStaticData.isReadOnly) GUI.enabled = false;
			Helper.BeginProperty(rect, prop, this);
			Helper.DoPropertyContextMenus(rect, prop, this);
			RevertableHelper.FixGUIWidthMismatch(prop.type, materialEditor);
			if (propStaticData.isAdvancedHeaderProperty)
				propStaticData.isExpanding = EditorGUI.Foldout(rect, propStaticData.isExpanding, string.Empty);
			RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, this, propStaticData.isMain || propStaticData.isAdvancedHeaderProperty);
			materialEditor.ShaderProperty(rect, prop, label);
			Helper.EndProperty(this, prop);
			GUI.enabled = enabled;
		}
	}
} //namespace LWGUI