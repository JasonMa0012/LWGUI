// Copyright (c) Jason Ma

using System;
using System.IO;
using LWGUI.LwguiGradientEditor;
using LWGUI.Runtime.LwguiGradient;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Draw an unreal style Ramp Map Editor (Default Ramp Map Resolution: 256 * 2)
	/// NEW: The new LwguiGradient type has both the Gradient and Curve editors, and can be used in C# scripts and runtime, and is intended to replace UnityEngine.Gradient
	/// 
	/// group: parent group name (Default: none)
	/// defaultFileName: default Ramp Map file name when create a new one (Default: RampMap)
	/// rootPath: the path where ramp is stored, replace '/' with '.' (for example: Assets.Art.Ramps). when selecting ramp, it will also be filtered according to the path (Default: Assets)
	/// colorSpace: switch sRGB / Linear in ramp texture import setting (Default: sRGB)
	/// defaultWidth: default Ramp Width. (Default: 256)
	/// viewChannelMask: editable channels. (Default: RGBA)
	/// timeRange: the abscissa display range (1/24/2400), is used to optimize the editing experience when the abscissa is time of day. (Default: 1)
	/// Target Property Type: Texture2D
	/// </summary>
	[LwguiDrawerCategory("Ramp", -10)]
	public class RampDrawer : SubDrawer
	{
		public static readonly string DefaultRootPath = "Assets";

		public string rootPath = "Assets";
		public string saveFilePanelTitle = "Create New Ramp Texture";
		public string defaultFileName = "RampMap";
		public string fileExtension = "png";
		public int defaultWidth = 256;
		public int defaultHeight = 2;
		public ColorSpace colorSpace = ColorSpace.Gamma;
		public LwguiGradient.ChannelMask viewChannelMask = LwguiGradient.ChannelMask.All;
		public LwguiGradient.GradientTimeRange timeRange = LwguiGradient.GradientTimeRange.One;
		public bool doRegisterUndo;

		private static readonly float _rampButtonsHeight = EditorGUIUtility.singleLineHeight;


		private static string _lastSaveDirectoryPrefKey = nameof(_lastSaveDirectoryPrefKey) + ": " + Application.dataPath;
		private static string _lastSaveDirectory
		{
			get => EditorPrefs.GetString(_lastSaveDirectoryPrefKey, string.Empty);
			set => EditorPrefs.SetString(_lastSaveDirectoryPrefKey, value);
		}

		protected virtual float rampPreviewHeight => EditorGUIUtility.singleLineHeight;

		protected override float GetVisibleHeight(MaterialProperty prop) { return rampPreviewHeight + _rampButtonsHeight; }

		public RampDrawer() : this(String.Empty) { }

		public RampDrawer(string group) : this(group, "RampMap") { }

		public RampDrawer(string group, string defaultFileName) : this(group, defaultFileName, DefaultRootPath, 256) { }

		public RampDrawer(string group, string defaultFileName, float defaultWidth) : this(group, defaultFileName, DefaultRootPath, defaultWidth) { }

		public RampDrawer(string group, string defaultFileName, string rootPath, float defaultWidth) : this(group, defaultFileName, rootPath, "sRGB", defaultWidth) { }
		
		public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, "RGBA") { }
		
		public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, string viewChannelMask) : this(group, defaultFileName, rootPath, colorSpace, defaultWidth, viewChannelMask, 1) { }

		public RampDrawer(string group, string defaultFileName, string rootPath, string colorSpace, float defaultWidth, string viewChannelMask, float timeRange)
		{
			if (!rootPath.StartsWith(DefaultRootPath) && !rootPath.StartsWith("Packages"))
			{
				Debug.LogError("LWGUI: Ramp Root Path: '" + rootPath + "' must start with 'Assets' or 'Packages'!");
				rootPath = DefaultRootPath;
			}
			this.group = group;
			this.defaultFileName = defaultFileName;
			this.rootPath = rootPath.Replace('.', '/');
			this.colorSpace = colorSpace.ToLower() == "linear" ? ColorSpace.Linear : ColorSpace.Gamma;
			this.defaultWidth = (int)Mathf.Max(2, defaultWidth);
			this.viewChannelMask = LwguiGradient.ChannelMask.None;
			{
				viewChannelMask = viewChannelMask.ToLower();
				for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
				{
					if (viewChannelMask.Contains(LwguiGradient.channelNames[c]))
						this.viewChannelMask |= LwguiGradient.ChannelIndexToMask(c);
				}

				if (this.viewChannelMask == (LwguiGradient.ChannelMask.RGB | LwguiGradient.ChannelMask.Alpha))
					this.viewChannelMask = LwguiGradient.ChannelMask.All;
			}
			this.timeRange = LwguiGradient.GradientTimeRange.One;
			{
				if ((int)timeRange == (int)LwguiGradient.GradientTimeRange.TwentyFour)
					this.timeRange = LwguiGradient.GradientTimeRange.TwentyFour;
				else if ((int)timeRange == (int)LwguiGradient.GradientTimeRange.TwentyFourHundred)
					this.timeRange = LwguiGradient.GradientTimeRange.TwentyFourHundred;
			}
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) { return propType == ShaderPropertyType.Texture; }

		protected virtual void OnRampPropUpdate(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }

		protected virtual void OnCreateNewRampMap(MaterialProperty prop) { }

		protected virtual void OnGradientEditorChange(LwguiGradient gradient) { }
		
		protected virtual void OnEditRampMap(MaterialProperty prop, LwguiGradient gradient) { }
		
		protected virtual void OnSaveRampMap(MaterialProperty prop, LwguiGradient gradient) { }
		
		protected virtual void OnDiscardRampMap(MaterialProperty prop, LwguiGradient gradient) { }
		
		protected virtual void OnSwitchRampMap(MaterialProperty prop, Texture2D newRampMap, int index) { }
		
		protected virtual LwguiGradient GetLwguiGradient(MaterialProperty prop, out bool isDirty)
		{
			return RampHelper.GetGradientFromTexture(prop.textureValue, out isDirty, false, doRegisterUndo);
		}

		protected virtual void EditWhenNoRampMap(MaterialProperty prop, MaterialEditor editor)
		{
			LwguiGradientWindow.CloseWindow();
			CreateNewRampMap(prop, editor);
			OnCreateNewRampMap(prop);
			LWGUI.OnValidate(metaDatas);
		}
		
		protected virtual void CreateNewRampMap(MaterialProperty prop, MaterialEditor editor)
		{
			CloneRampMap(prop, editor, null);
		}

		protected virtual void CloneRampMap(MaterialProperty prop, MaterialEditor editor, LwguiGradient gradient)
		{
			string createdFileRelativePath = string.Empty;

			string dialogDirectory = rootPath;
			string dialogFileName = defaultFileName;
			string absRootPath = IOHelper.GetAbsPath(rootPath);
			string lastSaveDirectory = _lastSaveDirectory;
			string absLastSaveDirectory = IOHelper.GetAbsPath(lastSaveDirectory);
			

			if (gradient != null && prop.textureValue != null)
			{
				var currentPath = AssetDatabase.GetAssetPath(prop.textureValue);
				if (!string.IsNullOrEmpty(currentPath) && currentPath.StartsWith(rootPath))
				{
					dialogDirectory = Path.GetDirectoryName(currentPath);
					var baseName = Path.GetFileNameWithoutExtension(currentPath);
					dialogFileName = IOHelper.GenerateUniqueFileName(dialogDirectory, baseName, fileExtension);
				}
			}
			else if (!string.IsNullOrEmpty(lastSaveDirectory) && lastSaveDirectory.StartsWith(rootPath) && Directory.Exists(absLastSaveDirectory))
			{
				dialogDirectory = lastSaveDirectory;
			}
			
			Directory.CreateDirectory(IOHelper.GetAbsPath(dialogDirectory));

			while (true)
			{
				var absPath = IOHelper.SaveFilePanel(saveFilePanelTitle, dialogDirectory, dialogFileName, fileExtension);
				if (absPath.StartsWith(absRootPath))
				{
					createdFileRelativePath = IOHelper.GetRelativePath(absPath);
					break;
				}
				else if (absPath != string.Empty)
				{
					var retry = EditorUtility.DisplayDialog("Invalid Path", $"Please select the subdirectory of '{absRootPath}'", "Retry", "Cancel");
					if (!retry) break;
				}
				else
				{
					break;
				}
			}

			if (!string.IsNullOrEmpty(createdFileRelativePath))
			{
				_lastSaveDirectory = Path.GetDirectoryName(createdFileRelativePath);

				var width = prop.textureValue != null ? prop.textureValue.width : defaultWidth;
				var height = prop.textureValue != null ? prop.textureValue.height : defaultHeight;
				RampHelper.CreateAndSaveNewGradientTexture(width, height, createdFileRelativePath, colorSpace == ColorSpace.Linear, gradient);
				prop.textureValue = AssetDatabase.LoadAssetAtPath<Texture2D>(createdFileRelativePath);
				EditorGUIUtility.PingObject(prop.textureValue);
			}
		}

		protected virtual void ChangeRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			RampHelper.SetGradientToTexture(prop.textureValue, gradient, false);
		}

		protected virtual void SaveRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			RampHelper.SetGradientToTexture(prop.textureValue, gradient, true);
		}
		
		protected virtual void SwitchRampMap(MaterialProperty prop, Texture2D newRampMap, int index)
		{
			prop.textureValue = newRampMap;
			OnSwitchRampMap(prop, newRampMap, index);
			LWGUI.OnValidate(metaDatas);
		}

		protected virtual LwguiGradient DiscardRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			// Tex > Gradient
			gradient = RampHelper.GetGradientFromTexture(prop.textureValue, out _, true);
			// GradientObject > Tex
			RampHelper.SetGradientToTexture(prop.textureValue, gradient, true);
			return gradient;
		}

		protected virtual void DrawRampSelector(Rect selectButtonRect, MaterialProperty prop, LwguiGradient gradient)
		{
			RampHelper.RampMapSelectorOverride(selectButtonRect, prop, rootPath, SwitchRampMap);
		}

		// Manual replace ramp map
		protected virtual void DrawRampObjectField(Rect rampFieldRect, MaterialProperty prop, LwguiGradient gradient)
		{
			EditorGUI.BeginChangeCheck();
			var newManualSelectedTexture = (Texture2D)EditorGUI.ObjectField(rampFieldRect, prop.textureValue, typeof(Texture2D), false);
			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				var texturePath = AssetDatabase.GetAssetPath(newManualSelectedTexture);
				if (newManualSelectedTexture && !texturePath.StartsWith(rootPath) && !texturePath.StartsWith("Packages/"))
					EditorUtility.DisplayDialog("Invalid Path", "Please select the subdirectory of '" + rootPath + "' or a Package asset", "OK");
				else
					SwitchRampMap(prop, newManualSelectedTexture, 0);
			}
		}

		protected virtual void DrawPreviewTextureOverride(Rect previewRect, Rect fieldRect, MaterialProperty prop, LwguiGradient gradient)
		{
			if (!prop.hasMixedValue && gradient != null)
			{
				LwguiGradientEditorHelper.DrawGradientWithSeparateAlphaChannel(previewRect, gradient, colorSpace, viewChannelMask);
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var indentLevel = EditorGUI.indentLevel;

			OnRampPropUpdate(position, prop, label, editor);

			LwguiGradient gradient = null;
			var isDirty = false;
			string gradientErrorMessage = null;

			try
			{
				gradient = GetLwguiGradient(prop, out isDirty);
			}
			catch (System.Exception e)
			{
				gradientErrorMessage = "Ramp data corrupted! Check Console for details.";
				Debug.LogError("LWGUI: Failed to parse gradient from '" + prop.displayName + "': " + e.Message);
			}

			if (gradient == null && gradientErrorMessage == null
				&& prop.GetPropertyType() == ShaderPropertyType.Texture && prop.textureValue != null)
			{
				gradientErrorMessage = "Ramp data missing or corrupted! Check Console for details.";
			}

			// Draw Label
			var labelRect = new Rect(position);
			{
				labelRect.height = rampPreviewHeight;
				EditorGUI.PrefixLabel(labelRect, label);
			}

			// Ramp buttons Rect
			var buttonRect = new Rect(position);
			{
				EditorGUI.indentLevel = 0;
				buttonRect.yMin = buttonRect.yMax - EditorGUIUtility.singleLineHeight;
				buttonRect = MaterialEditor.GetRectAfterLabelWidth(buttonRect);
				if (buttonRect.width < 50f) return;
			}

			if (gradientErrorMessage != null)
			{
				EditorGUI.HelpBox(buttonRect, gradientErrorMessage, MessageType.Error);
			}
			else
			{
				// Draw Ramp Editor
				RampHelper.RampEditor(buttonRect, ref gradient, colorSpace, viewChannelMask, timeRange, isDirty,  
					out bool hasGradientChanges, 
					out bool doEditWhenNoGradient, 
					out doRegisterUndo, 
					out bool doClone,
					out bool doCreate, 
					out bool doSaveGradient, 
					out bool doDiscardGradient,
					OnGradientEditorChange);
				
				// Edit When No Gradient
				if (doEditWhenNoGradient)
				{
					EditWhenNoRampMap(prop, editor);
				}

				// Clone
				if (doClone)
				{
					LwguiGradientWindow.CloseWindow();
					CloneRampMap(prop, editor, gradient);
					OnCreateNewRampMap(prop);
					LWGUI.OnValidate(metaDatas);
				}
				
				// Create
				if (doCreate)
				{
					LwguiGradientWindow.CloseWindow();
					CreateNewRampMap(prop, editor);
					OnCreateNewRampMap(prop);
					LWGUI.OnValidate(metaDatas);
				}

				// Change
				if (hasGradientChanges && gradient != null)
				{
					ChangeRampMap(prop, gradient);
					OnEditRampMap(prop, gradient);
				}
				
				// Save
				if (doSaveGradient && gradient != null)
				{
					SaveRampMap(prop, gradient);
					OnSaveRampMap(prop, gradient);
				}

				// Discard
				if (doDiscardGradient)
				{
					LwguiGradientWindow.CloseWindow();
					gradient = DiscardRampMap(prop, gradient);
					OnDiscardRampMap(prop, gradient);
				}
			}

			// Texture Object Field, handle switch texture event
			var rampFieldRect = MaterialEditor.GetRectAfterLabelWidth(labelRect);
			rampFieldRect.height = labelRect.height;
			var previewRect = new Rect(rampFieldRect.x + 0.5f, rampFieldRect.y + 0.5f, rampFieldRect.width - 18, rampFieldRect.height - 0.5f);
			{
				var selectButtonRect = new Rect(previewRect.xMax, rampFieldRect.y, rampFieldRect.width - previewRect.width, rampFieldRect.height);
				DrawRampSelector(selectButtonRect, prop, gradient);

				DrawRampObjectField(rampFieldRect, prop, gradient);
			}

			// Preview texture override (larger preview, hides texture name)
			DrawPreviewTextureOverride(previewRect, rampFieldRect, prop, gradient);

			EditorGUI.indentLevel = indentLevel;
		}
	}
}
