// Copyright (c) Jason Ma

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Draw an image preview.
	/// display name: The path of the image file relative to the Unity project, such as: "Assets/test.png", "Doc/test.png", "../test.png"
	/// 
	/// group: parent group name (Default: none)
	/// Target Property Type: Any
	/// </summary>
	[LwguiDrawerCategory("Texture")]
	public class ImageDrawer : SubDrawer
	{
		public ImageDrawer() { }

		public ImageDrawer(string group)
		{
			this.group = group;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			var inputPath = (inProp.displayName ?? string.Empty).Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
			string imageAbsPath = null;

			try
			{
				// If the display path already starts with Assets or Packages (project-relative), resolve from project root
				if (inputPath.StartsWith("Assets") || inputPath.StartsWith("Packages"))
				{
					imageAbsPath = IOHelper.GetAbsPath(inputPath);
				}
				// If it's an absolute path, use it directly
				else if (Path.IsPathRooted(inputPath))
				{
					imageAbsPath = inputPath;
				}
				else
				{
					// Try resolving relative to the shader file location
					var shaderAssetPath = AssetDatabase.GetAssetPath(inShader);
					if (!string.IsNullOrEmpty(shaderAssetPath))
					{
						var shaderFullPath = IOHelper.GetAbsPath(shaderAssetPath);
						var shaderDir = Path.GetDirectoryName(shaderFullPath);
						if (!string.IsNullOrEmpty(shaderDir))
						{
							// Combine and normalize to resolve ../ segments
							var candidate = Path.GetFullPath(Path.Combine(shaderDir, inputPath));
							if (File.Exists(candidate))
							{
								imageAbsPath = candidate;
							}
						}
					}
					// Fallback: try project-root relative resolution
					if (imageAbsPath == null)
					{
						var candidate2 = IOHelper.GetAbsPath(inputPath);
						if (File.Exists(candidate2)) imageAbsPath = candidate2;
					}
				}

				if (!string.IsNullOrEmpty(imageAbsPath) && File.Exists(imageAbsPath))
				{
					var fileData = File.ReadAllBytes(imageAbsPath);
					Texture2D texture = new Texture2D(2, 2);

					// LoadImage will auto-resize the texture dimensions
					if (texture.LoadImage(fileData))
					{
						inoutPropertyStaticData.image = texture;
					}
					else
					{
						Debug.LogError($"LWGUI: Failed to load image data into texture: { imageAbsPath }");
					}
				}
				else
				{
					Debug.LogError($"LWGUI: Image path not found: { inputPath }");
				}
			}
			catch (Exception ex)
			{
				Debug.LogError($"LWGUI: Exception while resolving image path '{inputPath}': {ex.Message}");
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var image = metaDatas.GetPropStaticData(prop).image;
			if (image)
			{
				var scaledheight = Mathf.Max(0, image.height / (image.width / Helper.GetCurrentPropertyLayoutWidth()));
				var rect = EditorGUILayout.GetControlRect(true, scaledheight);
				rect = RevertableHelper.IndentRect(EditorGUI.IndentedRect(rect));
				EditorGUI.DrawPreviewTexture(rect, image);

				if (GUI.enabled)
					prop.textureValue = null;
			}
		}
	}
}
