// Copyright (c) Jason Ma
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	public delegate void LWGUICustomGUIEvent(LWGUI lwgui);

	public class LWGUI : ShaderGUI
	{
		public LWGUIMetaDatas     metaDatas;

		public static LWGUICustomGUIEvent onDrawCustomHeader;
		public static LWGUICustomGUIEvent onDrawCustomFooter;

		/// <summary>
		/// Called when switch to a new Material Window, each window has a LWGUI instance
		/// </summary>
		public LWGUI() { }

		/// <summary>
		/// Called every frame when the content is updated, such as the mouse moving in the material editor
		/// </summary>
		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
		{
			//-----------------------------------------------------------------------------
			// Init Datas
			var material = materialEditor.target as Material;
			var shader = material.shader;
			this.metaDatas = MetaDataHelper.BuildMetaDatas(shader, material, materialEditor, this, props);


			//-----------------------------------------------------------------------------
			// Header
			if (onDrawCustomHeader != null)
				onDrawCustomHeader(this);

			// Toolbar
			bool enabled = GUI.enabled;
			GUI.enabled = true;
			var toolBarRect = EditorGUILayout.GetControlRect();
			toolBarRect.xMin = 2;

			Helper.DrawToolbarButtons(ref toolBarRect, metaDatas);

			Helper.DrawSearchField(toolBarRect, metaDatas);

			GUILayoutUtility.GetRect(0, 0); // Space(0)
			GUI.enabled = enabled;
			Helper.DrawSplitLine();


			//-----------------------------------------------------------------------------
			// Draw Properties
			{
				// move fields left to make rect for Revert Button
				materialEditor.SetDefaultGUIWidths();
				RevertableHelper.InitRevertableGUIWidths();

				// start drawing properties
				foreach (var prop in props)
				{
					var (propStaticData, propDynamicData) = metaDatas.GetPropDatas(prop);

					// Visibility
					{
						if (!MetaDataHelper.GetPropertyVisibility(prop, material, metaDatas))
							continue;

						if (propStaticData.parent != null
							&& (!MetaDataHelper.GetParentPropertyVisibility(propStaticData.parent, material, metaDatas)
								|| !MetaDataHelper.GetParentPropertyVisibility(propStaticData.parent.parent, material, metaDatas)))
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


			//-----------------------------------------------------------------------------
			// Footer
			EditorGUILayout.Space();
			Helper.DrawSplitLine();
			EditorGUILayout.Space();

			// Render settings
			if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
				materialEditor.RenderQueueField();
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
			RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, metaDatas, true);
			Helper.DoPropertyContextMenus(rect, prop, metaDatas);
		}

		private void DrawProperty(MaterialProperty prop)
		{
			var (propStaticData, propDynamicData) = metaDatas.GetPropDatas(prop);
			var materialEditor = metaDatas.GetMaterialEditor();

			Helper.DrawHelpbox(propStaticData, propDynamicData);

			var label = new GUIContent(propStaticData.displayName, MetaDataHelper.GetPropertyTooltip(propStaticData, propDynamicData));
			var height = metaDatas.perInspectorData.materialEditor.GetPropertyHeight(prop, label.text);
			var rect = EditorGUILayout.GetControlRect(true, height);

			var revertButtonRect = RevertableHelper.SplitRevertButtonRect(ref rect);

			var enabled = GUI.enabled;
			if (propStaticData.isReadOnly) GUI.enabled = false;
			Helper.BeginProperty(rect, prop, metaDatas);
			Helper.DoPropertyContextMenus(rect, prop, metaDatas);
			RevertableHelper.FixGUIWidthMismatch(prop.type, materialEditor);
			if (propStaticData.isAdvancedHeaderProperty)
				propStaticData.isExpanding = EditorGUI.Foldout(rect, propStaticData.isExpanding, string.Empty);
			RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, metaDatas, propStaticData.isMain || propStaticData.isAdvancedHeaderProperty);
			materialEditor.ShaderProperty(rect, prop, label);
			Helper.EndProperty(metaDatas, prop);
			GUI.enabled = enabled;
		}

		public override void OnClosed(Material material)
		{
			base.OnClosed(material);
			MetaDataHelper.ReleaseMaterialMetadataCache(material);
		}

		public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
		{
			base.AssignNewShaderToMaterial(material, oldShader, newShader);
			if (newShader != oldShader)
				MetaDataHelper.ReleaseMaterialMetadataCache(material);
		}

		// Called after editing the material
		public override void ValidateMaterial(Material material)
		{
			base.ValidateMaterial(material);
			metaDatas?.OnValidate();
		}
	}
} //namespace LWGUI