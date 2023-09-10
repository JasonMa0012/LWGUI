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
			this.perFrameData = MetaDataHelper.BuildPerFrameData(shader, props);


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
					var propertyStaticData = perShaderData.propertyDatas[prop.name];
					var propertyDynamicData = perFrameData.propertyDatas[prop.name];

					// Visibility
					{
						if (// if HideInInspector
							(prop.flags & MaterialProperty.PropFlags.HideInInspector) != 0
							// if Search Filtered
							|| !propertyStaticData.isSearchDisplayed
							// if the Group is not Expanded
							|| (!propertyStaticData.isMain && propertyStaticData.parent != null && !propertyStaticData.parent.isExpanded)
							// if the Conditional Display Keyword is not active
							|| (!string.IsNullOrEmpty(propertyStaticData.conditionalDisplayKeyword)
								&& !material.shaderKeywords.Any((str => str == propertyStaticData.conditionalDisplayKeyword)))
						   )
						{
							continue;
						}
					}

					Helper.DrawHelpbox(propertyStaticData, propertyDynamicData);

					var label = new GUIContent(propertyStaticData.displayName, MetaDataHelper.GetPropertyTooltip(propertyStaticData, propertyDynamicData));
					var height = materialEditor.GetPropertyHeight(prop, label.text);
					var rect = EditorGUILayout.GetControlRect(true, height);
					var revertButtonRect = RevertableHelper.SplitRevertButtonRect(ref rect);

					// PresetHelper.DrawPropertyPresetMenu(rect, shader, prop, props);

					RevertableHelper.FixGUIWidthMismatch(prop.type, materialEditor);

					RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, this);
					materialEditor.ShaderProperty(rect, prop, label);

					RevertableHelper.SetRevertableGUIWidths();
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


		/// <summary>
		///   <para>Find shader properties.</para>
		/// </summary>
		/// <param name="propertyName">The name of the material property.</param>
		/// <param name="properties">The array of available material properties.</param>
		/// <param name="propertyIsMandatory">If true then this method will throw an exception if a property with propertyName was not found.</param>
		/// <returns>
		///   <para>The material property found, otherwise null.</para>
		/// </returns>
		public static MaterialProperty FindProp(string             propertyName,
												MaterialProperty[] properties,
												bool               propertyIsMandatory = false)
		{
			MaterialProperty outProperty = null;

			if (!string.IsNullOrEmpty(propertyName) && propertyName != "_")
				outProperty = FindProperty(propertyName, properties, propertyIsMandatory);
			else
				return null;

			return outProperty;
		}
	}
} //namespace LWGUI