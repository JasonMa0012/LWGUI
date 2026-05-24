// Copyright (c) Jason Ma

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Draw a "Ramp Atlas Scriptable Object" selector and texture preview.
	/// The Ramp Atlas SO is responsible for storing multiple ramps and generating the corresponding Ramp Atlas Texture.
	/// Use it together with RampAtlasIndexer() to sample specific ramps in Shader using Index, similar to UE's Curve Atlas.
	/// Note: Currently, the material only saves Texture reference and Int value,
	///		  if you manually modify the Ramp Atlas, the references will not update automatically!
	/// 
	/// group: parent group name (Default: none)
	/// defaultFileName: the default file name when creating a Ramp Atlas SO (Default: RampAtlas)
	/// rootPath: the default directory when creating a Ramp Atlas SO, replace '/' with '.' (for example: Assets.Art.RampAtlas). (Default: Assets)
	/// colorSpace: the Color Space of Ramp Atlas Texture. (sRGB/Linear) (Default: sRGB)
	/// defaultWidth: default Ramp Atlas Texture width (Default: 256)
	/// defaultHeight: default Ramp Atlas Texture height (Default: 4)
	/// showAtlasPreview: Draw the preview of Ramp Atlas below (True/False) (Default: True)
	/// rampAtlasTypeName: custom RampAtlas type name for user-defined RampAtlas classes (Default: LwguiRampAtlas)
	/// Target Property Type: Texture2D
	/// </summary>
	[LwguiDrawerCategory("Ramp")]
	public class RampAtlasDrawer : SubDrawer
	{
		public string rootPath = "Assets";
		public string defaultFileName = "RampAtlas";
		public bool defaultAtlasSRGB = true;
		public int defaultAtlasWidth = 256;
		public int defaultAtlasHeight = 2;
		public bool showAtlasPreview = true;
		public Type rampAtlasType = typeof(LwguiRampAtlas);
		
		protected LwguiRampAtlas _rampAtlasSO;
		
		public RampAtlasDrawer() : this(string.Empty) { }
		
		public RampAtlasDrawer(string group) : this(group, "RampAtlas") { }
		
		public RampAtlasDrawer(string group, string defaultFileName) : this(group, defaultFileName, "Assets") { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath) : this(group, defaultFileName, rootPath, "sRGB") { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace) : this(group, defaultFileName, rootPath, colorSpace, 256) { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, 4) { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, float defaultHeight) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, defaultHeight, "true") { }
		
		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, float defaultHeight, string showAtlasPreview) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, defaultHeight, showAtlasPreview, "") { }

		public RampAtlasDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, float defaultHeight, string showAtlasPreview, string rampAtlasTypeName)
		{
			if (!rootPath.StartsWith(this.rootPath) && !rootPath.StartsWith("Packages"))
			{
				Debug.LogError("LWGUI: Ramp Atlas Root Path: '" + rootPath + "' must start with 'Assets' or 'Packages'!");
				rootPath = this.rootPath;
			}
			this.group = group;
			this.defaultFileName = defaultFileName;
			this.rootPath = rootPath.Replace('.', '/');
			this.defaultAtlasSRGB = colorSpace.ToLower() == "srgb";
			this.defaultAtlasWidth = (int)Mathf.Max(2, defaultWidth);
			this.defaultAtlasHeight = (int)Mathf.Max(2, defaultHeight);
			this.showAtlasPreview = Helper.StringToBool(showAtlasPreview);
			
			// Resolve custom RampAtlas type
			if (!string.IsNullOrEmpty(rampAtlasTypeName))
			{
				var customType = ReflectionHelper.GetTypeByName(rampAtlasTypeName);
				if (customType != null && typeof(LwguiRampAtlas).IsAssignableFrom(customType))
				{
					rampAtlasType = customType;
				}
				else
				{
					Debug.LogError($"LWGUI: RampAtlas type '{rampAtlasTypeName}' not found or not derived from LwguiRampAtlas!");
				}
			}
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) => propType == ShaderPropertyType.Texture;

		protected override float GetVisibleHeight(MaterialProperty prop) => 
			EditorGUIUtility.singleLineHeight + 
			(prop.textureValue && showAtlasPreview ? MaterialEditor.GetDefaultPropertyHeight(prop) + 2.0f : 0);

		public override void GetCustomContextMenus(GenericMenu menu, Rect rect, MaterialProperty prop, LWGUIMetaDatas metaDatas)
		{
			menu.AddSeparator("");
			menu.AddItem(new GUIContent("Create Ramp Atlas"), false, () =>
			{
				_rampAtlasSO = LwguiRampAtlas.CreateRampAtlasSO(prop, metaDatas, rampAtlasType);
				if (_rampAtlasSO)
				{
					prop.textureValue = _rampAtlasSO.rampAtlasTexture;
					LWGUI.OnValidate(metaDatas);
				}
			});

			if (_rampAtlasSO)
			{
				menu.AddItem(new GUIContent("Clone Ramp Atlas"), false, () =>
				{
					var newRampAtlasSO = LwguiRampAtlas.CloneRampAtlasSO(_rampAtlasSO, rampAtlasType);
					if (newRampAtlasSO)
					{
						_rampAtlasSO = newRampAtlasSO;
						prop.textureValue = _rampAtlasSO.rampAtlasTexture;
						LWGUI.OnValidate(metaDatas);
					}
				});
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.PrefixLabel(position, label);
			
			var indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			
			var fieldRect = MaterialEditor.GetRectAfterLabelWidth(position);
			var rampAtlasSORect = new Rect(fieldRect.x, fieldRect.y, fieldRect.width, EditorGUIUtility.singleLineHeight);
			var rampAtlasTextureRect = new Rect(fieldRect.x, rampAtlasSORect.yMax + 2.0f, fieldRect.width, MaterialEditor.GetDefaultPropertyHeight(prop));

			_rampAtlasSO = LwguiRampAtlas.LoadRampAtlasSO(prop.textureValue);
			
			// Disable ObjectField's Context Menu
			var e = Event.current;
			if (e?.type == EventType.MouseDown && Event.current.button == 1
			    && (rampAtlasSORect.Contains(e.mousePosition)))
			{
				e.Use();
			}
			
			EditorGUI.BeginChangeCheck();
			_rampAtlasSO = (LwguiRampAtlas)EditorGUI.ObjectField(rampAtlasSORect, GUIContent.none, _rampAtlasSO, rampAtlasType, false);
			if (EditorGUI.EndChangeCheck())
			{
				prop.textureValue = LwguiRampAtlas.LoadRampAtlasTexture(_rampAtlasSO);

				if (_rampAtlasSO && !prop.textureValue)
				{
					Debug.LogError($"LWGUI: Can NOT load the Ramp Atlas Texture from: { _rampAtlasSO.name }");
				}
			}

			if (showAtlasPreview && prop.textureValue && !prop.hasMixedValue)
			{
				var filter = prop.textureValue.filterMode;
				prop.textureValue.filterMode = FilterMode.Point;
				EditorGUI.DrawPreviewTexture(rampAtlasTextureRect, prop.textureValue);
				prop.textureValue.filterMode = filter;
			}

			EditorGUI.indentLevel = indentLevel;
		}
	}
}
