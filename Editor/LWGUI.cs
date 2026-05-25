// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	public delegate void LWGUICustomGUIEvent(LWGUI lwgui);
	public delegate void LWGUIToolbarExtensionEvent(LWGUI lwgui, ref Rect toolBarRect);

	public class LWGUI : ShaderGUI
	{
		public LWGUIMetaDatas metaDatas;
		public bool hasChange;

		public static LWGUICustomGUIEvent onDrawCustomHeader;
		public static LWGUICustomGUIEvent onDrawCustomFooter;
		public static LWGUIToolbarExtensionEvent onDrawToolbarLeft;
		public static LWGUIToolbarExtensionEvent onDrawToolbarRight;

		/// <summary>
		/// Called when switch to a new Material Window, each window has a LWGUI instance
		/// </summary>
		public LWGUI() { }

		/// <summary>
		/// Called every frame when the content is updated, such as the mouse moving in the material editor
		/// </summary>
		public override void OnGUI(MaterialEditor editor, MaterialProperty[] props)
		{
			//-----------------------------------------------------------------------------
			// Init Datas
			var material = editor.target as Material;
			var shader = material.shader;
			if (hasChange)
			{
				OnValidate(editor.targets);
				hasChange = false;
			}
			this.metaDatas = MetaDataHelper.BuildMetaDatas(shader, material, editor, this, props);


			//-----------------------------------------------------------------------------
			// Header
			if (onDrawCustomHeader != null)
				onDrawCustomHeader(this);

			// Toolbar
			{
				bool enabled = GUI.enabled;
				GUI.enabled = true;
				var toolBarRect = EditorGUILayout.GetControlRect();
				toolBarRect.xMin = 2;

				onDrawToolbarLeft?.Invoke(this, ref toolBarRect);
				ToolbarHelper.DrawToolbarButtons(ref toolBarRect, metaDatas);
				onDrawToolbarRight?.Invoke(this, ref toolBarRect);
				ToolbarHelper.DrawSearchField(toolBarRect, metaDatas);

				GUILayoutUtility.GetRect(0, 0); // Space(0)
				GUI.enabled = enabled;
				Helper.DrawSplitLine();

				ToolbarHelper.DrawShaderPerformanceStats(metaDatas);
			}
			

			//-----------------------------------------------------------------------------
			// Draw Properties
			{
				Helper.SetAdjustableGUIWidths(metaDatas.perShaderData.displayModeData.labelWidthPercentage);

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
							EditorGUI.indentLevel = indentLevel;
							continue;
						}
					}

					if (propStaticData.labelWidth > 0)
					{
						var indent = ReflectionHelper.EditorGUI_indent;
						if (propStaticData.labelWidth > EditorGUIUtility.labelWidth - indent)
							EditorGUIUtility.labelWidth = propStaticData.labelWidth + indent;
					}

					DrawProperty(prop);

					EditorGUI.indentLevel = indentLevel;
					Helper.SetAdjustableGUIWidths(metaDatas.perShaderData.displayModeData.labelWidthPercentage);
				}
			}


			//-----------------------------------------------------------------------------
			// Footer
			EditorGUILayout.Space();
			Helper.DrawSplitLine();
			EditorGUILayout.Space();

			// Render settings
			if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
				editor.RenderQueueField();
			editor.EnableInstancingField();
			editor.LightmapEmissionProperty();
			editor.DoubleSidedGIField();

			// Custom Footer
			if (onDrawCustomFooter != null)
				onDrawCustomFooter(this);

			// LOGO
			EditorGUILayout.Space();
			Helper.DrawLogo();
		}

		private void DrawAdvancedHeader(PropertyStaticData propStaticData, MaterialProperty prop)
		{
			EditorGUILayout.Space(3);
			
			var rect = EditorGUILayout.GetControlRect();
			var revertButtonRect = RevertableHelper.SplitRevertButtonRect(ref rect);
			var label = string.IsNullOrEmpty(propStaticData.advancedHeaderString) ? "Advanced" : propStaticData.advancedHeaderString;
			
			propStaticData.isExpanding = EditorGUI.Foldout(rect, propStaticData.isExpanding, label, EditorStyles.foldoutHeader);
			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
				propStaticData.isExpanding = !propStaticData.isExpanding;
			
			RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, metaDatas, true);
			ContextMenuHelper.DoPropertyContextMenus(rect, prop, metaDatas);
		}

		private void DrawProperty(MaterialProperty prop)
		{
			var (propStaticData, propDynamicData) = metaDatas.GetPropDatas(prop);
			var materialEditor = metaDatas.GetMaterialEditor();
			
			if (propStaticData.isAdvancedHeaderProperty)
				EditorGUILayout.Space(3);

			Helper.DrawHelpbox(propStaticData, propDynamicData);

			var label = new GUIContent(propStaticData.displayName, MetaDataHelper.GetPropertyTooltip(propStaticData, propDynamicData));
			var height = metaDatas.perInspectorData.materialEditor.GetPropertyHeight(prop, label.text);
			var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
			var revertButtonRect = RevertableHelper.SplitRevertButtonRect(ref rect);

			var enabled = GUI.enabled;
			if (propStaticData.isReadOnly || !propDynamicData.isActive) 
				GUI.enabled = false;
			Helper.BeginProperty(rect, prop, metaDatas);
			ContextMenuHelper.DoPropertyContextMenus(rect, prop, metaDatas);
			
			if (propStaticData.isAdvancedHeaderProperty)
				propStaticData.isExpanding = EditorGUI.Foldout(rect, propStaticData.isExpanding, string.Empty);
			
			RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, metaDatas, propStaticData.isMain || propStaticData.isAdvancedHeaderProperty);
			materialEditor.LwguiShaderProperty(rect, prop, label);

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

		public static void OnValidate(Object[] materials)
		{
			VersionControlHelper.Checkout(materials);
			UnityEditorExtension.ApplyMaterialPropertyAndDecoratorDrawers(materials);
			MetaDataHelper.ForceUpdateMaterialsMetadataCache(materials);
		}
		
		// Called after edit in code
		public static void OnValidate(LWGUIMetaDatas metaDatas)
		{
			OnValidate(metaDatas?.GetMaterialEditor()?.targets);
		}
		
		public override void ValidateMaterial(Material material)
		{
			// Debug.Log($"ValidateMaterial {material.name}, {metaDatas}, {Event.current?.type}");

			// Validate a Faked Material when select/edit a Material
			if (metaDatas == null && (Event.current == null || Event.current.type == EventType.Layout))
			{
				// Skip to avoid lag when editing large amounts of materials
			}
			// Undo/Edit in Timeline (EventType.Repaint)
			// Note: When modifying the material in Timeline in Unity 2022, this function cannot correctly obtain the modified value.
			else if (metaDatas == null)
			{
				MetaDataHelper.ForceUpdateMaterialMetadataCache(material);
			}
			// Edit
			else
			{
				if (!hasChange) hasChange = true;
			}
		}
	}
}