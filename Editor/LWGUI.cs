// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// when LwguiEventType.Init:		get all metadata from drawer
	/// when LwguiEventType.Repaint:	LWGUI decides how to draw each prop according to metadata
	public enum LwguiEventType
	{
		Init,
		Repaint
	}
	
	public enum SearchMode
	{
		All,
		Modified
	}

	public class LWGUI : ShaderGUI
	{
		public        MaterialProperty[]                                props;
		public        MaterialEditor                                    materialEditor;
		public        Material                                          material;
		public        Dictionary<string /*PropName*/, bool /*Display*/> searchResult;
		public        string                                            searchingText     = String.Empty;
		public        string                                            lastSearchingText = String.Empty;
		public        SearchMode                                        searchMode        = SearchMode.All;
		public        SearchMode                                        lastSearchMode    = SearchMode.All;
		public        bool                                              updateSearchMode  = false;
		public        LwguiEventType                                    lwguiEventType    = LwguiEventType.Init;
		public        Shader                                            shader;

		private static bool                                              _forceInit = false;
		
		/// <summary>
		/// Called when switch to a new Material Window, each window has a LWGUI instance
		/// </summary>
		public LWGUI() { }

		public static void ForceInit() { _forceInit = true; }

		public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
		{
			this.props = props;
			this.materialEditor = materialEditor;
			this.material = materialEditor.target as Material;
			this.shader = this.material.shader;
			this.lwguiEventType = RevertableHelper.InitAndHasShaderModified(shader, material, props) || _forceInit
				? LwguiEventType.Init
				: LwguiEventType.Repaint;

			// reset caches and metadata
			if (lwguiEventType == LwguiEventType.Init)
			{
				MetaDataHelper.ClearCaches(shader);
				searchResult = null;
				lastSearchingText = searchingText = string.Empty;
				lastSearchMode = searchMode = SearchMode.All;
				updateSearchMode = false;
				_forceInit = false;
				MetaDataHelper.ReregisterAllPropertyMetaData(shader, material, props);
			}

			// draw with metadata
			{
				// Search Field
				if (searchResult == null)
					searchResult = MetaDataHelper.SearchProperties(shader, material, props, String.Empty, searchMode);

				if (Helper.DrawSearchField(ref searchingText, ref searchMode, this) || updateSearchMode)
				{
					// change anything to expand all group
					if ((string.IsNullOrEmpty(lastSearchingText) && lastSearchMode == SearchMode.All)) // last == init 
						GroupStateHelper.SetAllGroupFoldingAndCache(materialEditor.target, false);
					// restore to the cached state
					else if ((string.IsNullOrEmpty(searchingText) && searchMode == SearchMode.All)) // now == init
						GroupStateHelper.RestoreCachedFoldingState(materialEditor.target);

					searchResult = MetaDataHelper.SearchProperties(shader, material, props, searchingText, searchMode);
					lastSearchingText = searchingText;
					lastSearchMode = searchMode;
					updateSearchMode = false;
				}

				// move fields left to make rect for Revert Button
				materialEditor.SetDefaultGUIWidths();
				EditorGUIUtility.fieldWidth += RevertableHelper.revertButtonWidth;
				EditorGUIUtility.labelWidth -= RevertableHelper.revertButtonWidth;
				RevertableHelper.fieldWidth = EditorGUIUtility.fieldWidth;
				RevertableHelper.labelWidth = EditorGUIUtility.labelWidth;

				// start drawing properties
				foreach (var prop in props)
				{
					// force init when missing prop
					if (!searchResult.ContainsKey(prop.name))
					{
						_forceInit = true;
						return;
					}

					// ignored hidden prop
					if ((prop.flags & MaterialProperty.PropFlags.HideInInspector) != 0 || !searchResult[prop.name])
						continue;

					var height = materialEditor.GetPropertyHeight(prop, MetaDataHelper.GetPropertyDisplayName(shader, prop));

					// ignored when in Folding Group
					if (height <= 0) continue;

					Helper.DrawHelpbox(shader, prop);

					// get rect
					var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
					var revertButtonRect = RevertableHelper.GetRevertButtonRect(prop, rect);
					rect.xMax -= RevertableHelper.revertButtonWidth;

					PresetHelper.DrawAddPropertyToPresetMenu(rect, shader, prop, props);

					// fix some builtin types display misplaced
					switch (prop.type)
					{
						case MaterialProperty.PropType.Texture:
						case MaterialProperty.PropType.Range:
							materialEditor.SetDefaultGUIWidths();
							break;
						default:
							RevertableHelper.SetRevertableGUIWidths();
							break;
					}

					RevertableHelper.DrawRevertableProperty(revertButtonRect, prop, materialEditor);
					var label = new GUIContent(MetaDataHelper.GetPropertyDisplayName(shader, prop), MetaDataHelper.GetPropertyTooltip(shader, material, prop));
					materialEditor.ShaderProperty(rect, prop, label);
				}
			}

			materialEditor.SetDefaultGUIWidths();

			EditorGUILayout.Space();
			EditorGUILayout.Space();
#if UNITY_2019_4_OR_NEWER
			if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
#endif
			materialEditor.RenderQueueField();
			materialEditor.EnableInstancingField();
			materialEditor.LightmapEmissionProperty();
			materialEditor.DoubleSidedGIField();

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
		public static MaterialProperty FindProp(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory = false)
		{
			MaterialProperty outProperty= null;
			if (properties == null)
			{
				Debug.LogWarning("Get other properties form Drawer is only support Unity 2019.2+!");
				return null;
			}
			
			if (!string.IsNullOrEmpty(propertyName) && propertyName != "_")
				outProperty = FindProperty(propertyName, properties, propertyIsMandatory);
			else
				return null;
			
			if (outProperty == null)
				ForceInit();
			
			return outProperty;
		}
	}
} //namespace LWGUI