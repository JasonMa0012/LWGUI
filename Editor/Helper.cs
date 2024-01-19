// Copyright (c) Jason Ma
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LWGUI
{
	/// <summary>
	/// Misc Function
	/// </summary>
	public class Helper
	{
		#region Engine Misc

		public static void ObsoleteWarning(string obsoleteStr, string newStr)
		{
			Debug.LogWarning("'" + obsoleteStr + "' is Obsolete! Please use '" + newStr + "'!");
		}

		public static bool PropertyValueEquals(MaterialProperty prop1, MaterialProperty prop2)
		{
			if (prop1.textureValue == prop2.textureValue
			 && prop1.vectorValue == prop2.vectorValue
			 && prop1.colorValue == prop2.colorValue
			 && prop1.floatValue == prop2.floatValue
#if UNITY_2021_1_OR_NEWER
			 && prop1.intValue == prop2.intValue
#endif
			   )
				return true;
			else
				return false;
		}

		public static bool IsPropertyHideInInspector(MaterialProperty prop)
		{
			return (prop.flags & MaterialProperty.PropFlags.HideInInspector) != 0;
		}

		public static string GetKeyWord(string keyWord, string propName)
		{
			string k;
			if (string.IsNullOrEmpty(keyWord) || keyWord == "__")
			{
				k = propName.ToUpperInvariant() + "_ON";
			}
			else
			{
				k = keyWord.ToUpperInvariant();
			}
			return k;
		}

		public static void SetShaderKeyWord(Object[] materials, string keyWord, bool isEnable)
		{
			if (string.IsNullOrEmpty(keyWord) || string.IsNullOrEmpty(keyWord)) return;

			foreach (Material m in materials)
			{
				// delete "_" keywords
				if (keyWord == "_")
				{
					if (m.IsKeywordEnabled(keyWord))
					{
						m.DisableKeyword(keyWord);
					}
					continue;
				}

				if (m.IsKeywordEnabled(keyWord))
				{
					if (!isEnable) m.DisableKeyword(keyWord);
				}
				else
				{
					if (isEnable) m.EnableKeyword(keyWord);
				}
			}
		}

		public static void SetShaderKeyWord(Object[] materials, string[] keyWords, int index)
		{
			Debug.Assert(keyWords.Length >= 1 && index < keyWords.Length && index >= 0,
						 "KeyWords Length: " + keyWords.Length + " or Index: " + index + " Error! ");
			for (int i = 0; i < keyWords.Length; i++)
			{
				SetShaderKeyWord(materials, keyWords[i], index == i);
			}
		}

		public static void SetShaderPassEnabled(Object[] materials, string[] lightModeNames, bool enabled)
		{
			if (lightModeNames.Length == 0) return;

			foreach (Material material in materials)
			{
				for (int i = 0; i < lightModeNames.Length; i++)
				{
					material.SetShaderPassEnabled(lightModeNames[i], enabled);
				}
			}
		}

		/// <summary>
		/// make Drawer can get all current Material props by customShaderGUI
		/// Unity 2019.2+
		/// </summary>
		public static LWGUI GetLWGUI(MaterialEditor editor)
		{
			var customShaderGUI = ReflectionHelper.GetCustomShaderGUI(editor);
			if (customShaderGUI != null && customShaderGUI is LWGUI)
			{
				LWGUI gui = customShaderGUI as LWGUI;
				return gui;
			}
			else
			{
				Debug.LogWarning("Please add \"CustomEditor \"LWGUI.LWGUI\"\" to the end of your shader!");
				return null;
			}
		}

		public static void AdaptiveFieldWidth(GUIStyle style, GUIContent content)
		{
			var extraTextWidth = Mathf.Max(0, style.CalcSize(content).x - (EditorGUIUtility.fieldWidth - RevertableHelper.revertButtonWidth));
			EditorGUIUtility.labelWidth -= extraTextWidth;
			EditorGUIUtility.fieldWidth += extraTextWidth;
		}

		public static void BeginProperty(Rect rect, MaterialProperty property, LWGUI lwgui)
		{
#if UNITY_2022_1_OR_NEWER
			MaterialEditor.BeginProperty(rect, property);
			foreach (var extraPropName in lwgui.perShaderData.propertyDatas[property.name].extraPropNames)
				MaterialEditor.BeginProperty(rect, lwgui.perFrameData.propertyDatas[extraPropName].property);
#endif
		}

		public static void EndProperty(LWGUI lwgui, MaterialProperty property)
		{
#if UNITY_2022_1_OR_NEWER
			MaterialEditor.EndProperty();
			foreach (var extraPropName in lwgui.perShaderData.propertyDatas[property.name].extraPropNames)
				MaterialEditor.EndProperty();
#endif
		}

		public static bool EndChangeCheck(LWGUI lwgui, MaterialProperty property)
		{
			return lwgui.perFrameData.EndChangeCheck(property.name);
		}

		#endregion


		#region Math

		public static float PowPreserveSign(float f, float p)
		{
			float num = Mathf.Pow(Mathf.Abs(f), p);
			if ((double)f < 0.0)
				return -num;
			return num;
		}

		#endregion


		#region GUI Styles

		// Tips: Use properties to fix null reference errors

		private static GUIStyle _guiStyles_IconButton;
		public static  GUIStyle guiStyles_IconButton
		{
			get
			{
				if (_guiStyles_IconButton == null)
				{
					_guiStyles_IconButton = new GUIStyle(
#if UNITY_2021_2_OR_NEWER
														 EditorStyles.iconButton
#else
						"iconButton"
#endif
														) { fixedHeight = 0, fixedWidth = 0 };
				}
				return _guiStyles_IconButton;
			}
		}

		private static GUIStyle _guiStyle_Foldout;

		public static GUIStyle guiStyle_Foldout
		{
			get
			{
				if (_guiStyle_Foldout == null)
				{
					_guiStyle_Foldout =
						new GUIStyle(EditorStyles.miniButton)
						{
							contentOffset = new Vector2(22, 0),
							fixedHeight = 27,
							alignment = TextAnchor.MiddleLeft,
							font = EditorStyles.boldLabel.font,
							fontSize = EditorStyles.boldLabel.fontSize
				#if UNITY_2019_4_OR_NEWER
									 + 1,
				#endif
						};
				}
				return _guiStyle_Foldout;
			}
		}

		private static GUIStyle _guiStyle_Helpbox;

		public static GUIStyle guiStyle_Helpbox
		{
			get
			{
				if (_guiStyle_Helpbox == null)
				{
					_guiStyle_Helpbox = new GUIStyle(EditorStyles.helpBox) { fontSize = 12 };
				}
				return _guiStyle_Helpbox;
			}
		}

		private static GUIStyle _guiStyles_ToolbarSearchTextFieldPopup;
		public static GUIStyle guiStyles_ToolbarSearchTextFieldPopup
		{
			get
			{
				if (_guiStyles_ToolbarSearchTextFieldPopup == null)
				{
					string toolbarSeachTextFieldPopupStr = "ToolbarSeachTextFieldPopup";
					{
						// ToolbarSeachTextFieldPopup has renamed at Unity 2021.3.28+
	#if !UNITY_2022_3_OR_NEWER
						string[] versionParts = Application.unityVersion.Split('.');
						int majorVersion = int.Parse(versionParts[0]);
						int minorVersion = int.Parse(versionParts[1]);
						Match patchVersionMatch = Regex.Match(versionParts[2], @"\d+");
						int patchVersion = int.Parse(patchVersionMatch.Value);
						if (majorVersion >= 2021 && minorVersion >= 3 && patchVersion >= 28)
	#endif
						{
							toolbarSeachTextFieldPopupStr = "ToolbarSearchTextFieldPopup";
						}
					}
					_guiStyles_ToolbarSearchTextFieldPopup = new GUIStyle(toolbarSeachTextFieldPopupStr);
				}
				return _guiStyles_ToolbarSearchTextFieldPopup;
			}
		}


		#endregion

		#region Draw GUI for Drawer

		// TODO: use Reflection
		// copy and edit of https://github.com/GucioDevs/SimpleMinMaxSlider/blob/master/Assets/SimpleMinMaxSlider/Scripts/Editor/MinMaxSliderDrawer.cs
		public static Rect[] SplitRect(Rect rectToSplit, int n)
		{
			Rect[] rects = new Rect[n];

			for (int i = 0; i < n; i++)
			{
				rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y,
									rectToSplit.width / n, rectToSplit.height);
			}

			int padding = (int)rects[0].width - 50; // use 50, enough to show 0.xx (2 digits)
			int space = 5;

			rects[0].width -= padding + space;
			rects[2].width -= padding + space;

			rects[1].x -= padding;
			rects[1].width += padding * 2;

			rects[2].x += padding + space;

			return rects;
		}


		public static bool DrawFoldout(Rect rect, ref bool isFolding, bool toggleValue, bool hasToggle, GUIContent label)
		{
			var toggleRect = new Rect(rect.x + 8f, rect.y + 7f, 13f, 13f);

			// Toggle Event
			if (hasToggle)
			{
				if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && toggleRect.Contains(Event.current.mousePosition))
				{
					toggleValue = !toggleValue;
					Event.current.Use();
					GUI.changed = true;
				}
			}

			// Button
			{
				// Cancel Right Click
				if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
					Event.current.Use();

				var enabled = GUI.enabled;
				GUI.enabled = true;
				var guiColor = GUI.backgroundColor;
				GUI.backgroundColor = isFolding ? Color.white : new Color(0.85f, 0.85f, 0.85f);
				if (GUI.Button(rect, label, guiStyle_Foldout))
				{
					isFolding = !isFolding;
					GUI.changed = false;
				}
				GUI.backgroundColor = guiColor;
				GUI.enabled = enabled;
			}

			// Toggle Icon
			if (hasToggle)
			{
				EditorGUI.Toggle(toggleRect, string.Empty, toggleValue);
			}

			return toggleValue;
		}

		#endregion


		#region Draw GUI for Material

		public static void DrawSplitLine()
		{
			var rect = EditorGUILayout.GetControlRect(true, 1);
			rect.x = 0;
			rect.width = EditorGUIUtility.currentViewWidth;
			EditorGUI.DrawRect(rect, new Color(0, 0, 0, 0.45f));
		}

		private static readonly Texture2D _helpboxIcon     = EditorGUIUtility.IconContent("console.infoicon").image as Texture2D;

		public static void DrawHelpbox(PropertyStaticData propertyStaticData, PropertyDynamicData propertyDynamicData)
		{
			var helpboxStr = propertyStaticData.helpboxMessages;
			if (!string.IsNullOrEmpty(helpboxStr))
			{
				var content = new GUIContent(helpboxStr, _helpboxIcon);
				var textWidth = EditorGUIUtility.currentViewWidth - ReflectionHelper.EditorGUI_Indent - RevertableHelper.revertButtonWidth
							  - _helpboxIcon.width - guiStyle_Helpbox.margin.horizontal + 0.5f;
				var textHeight = guiStyle_Helpbox.CalcHeight(new GUIContent(helpboxStr), textWidth);
				var helpboxRect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(true, textHeight));
				helpboxRect.xMax -= RevertableHelper.revertButtonWidth;
				GUI.Label(helpboxRect, content, guiStyle_Helpbox);
				// EditorGUI.HelpBox(helpboxRect, helpboxStr, MessageType.Info);
			}
		}


		private static Texture _logo = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("26b9d845eb7b1a747bf04dc84e5bcc2c"));
		private static GUIContent _logoGuiContent = new GUIContent(string.Empty, _logo,
																   "LWGUI (Light Weight Shader GUI)\n\n"
																	+ "A Lightweight, Flexible, Powerful Unity Shader GUI system.\n\n"
																	+ "Copyright (c) Jason Ma");

		public static void DrawLogo()
		{
			var logoRect = EditorGUILayout.GetControlRect(false, _logo.height);
			var w = logoRect.width;
			logoRect.xMin += w * 0.5f - _logo.width * 0.5f;
			logoRect.xMax -= w * 0.5f - _logo.width * 0.5f;

			if (EditorGUIUtility.currentViewWidth >= logoRect.width && GUI.Button(logoRect, _logoGuiContent, guiStyles_IconButton))
			{
				Application.OpenURL("https://github.com/JasonMa0012/LWGUI");
			}
		}
		#endregion


		#region Toolbar Buttons
		private static Material     _copiedMaterial;
		private static List<string> _copiedProps = new List<string>();

		private static Texture _iconCopy = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("9cdef444d18d2ce4abb6bbc4fed4d109"));
		private static Texture _iconPaste = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("8e7a78d02e4c3574998524a0842a8ccb"));
		private static Texture _iconSelect = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("6f44e40b24300974eb607293e4224ecc"));
		private static Texture _iconCheckout = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("72488141525eaa8499e65e52755cb6d0"));
		private static Texture _iconExpand = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("2382450e7f4ddb94c9180d6634c41378"));
		private static Texture _iconCollapse = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("929b6e5dfacc42b429d715a3e1ca2b57"));
		private static Texture _iconVisibility = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("9576e23a695b35d49a9fc55c9a948b4f"));

		private static GUIContent _guiContentCopy = new GUIContent("", _iconCopy, "Copy Material Properties");
		private static GUIContent _guiContentPaste = new GUIContent("", _iconPaste, "Paste Material Properties\n\nRight-click to paste values by type.");
		private static GUIContent _guiContentSelect = new GUIContent("", _iconSelect, "Select the Material Asset\n\nUsed to jump from a Runtime Material Instance to a Material Asset.");
		private static GUIContent _guiContentChechout = new GUIContent("", _iconCheckout, "Checkout selected Material Assets");
		private static GUIContent _guiContentExpand = new GUIContent("", _iconExpand, "Expand All Groups");
		private static GUIContent _guiContentCollapse = new GUIContent("", _iconCollapse, "Collapse All Groups");
		private static GUIContent _guiContentVisibility = new GUIContent("", _iconVisibility, "Display Mode");

		private static string[] _materialInstanceNameEnd = new[] { "_Instantiated", " (Instance)" };

		private enum CopyMaterialValueMask
		{
			Float       = 1 << 0,
			Vector      = 1 << 1,
			Texture     = 1 << 2,
			Keyword     = 1 << 3,
			RenderQueue = 1 << 4,
			Number      = Float | Vector,
			All         = (1 << 5) - 1,
		}

		private static GUIContent[] _pasteMaterialMenus = new[]
		{
			new GUIContent("Paste Number Values"),
			new GUIContent("Paste Texture Values"),
			new GUIContent("Paste Keywords"),
			new GUIContent("Paste RenderQueue"),
		};

		private static uint[] _pasteMaterialMenuValueMasks = new[]
		{
			(uint)CopyMaterialValueMask.Number,
			(uint)CopyMaterialValueMask.Texture,
			(uint)CopyMaterialValueMask.Keyword,
			(uint)CopyMaterialValueMask.RenderQueue,
		};

		private static void DoPasteMaterialProperties(LWGUI lwgui , uint valueMask)
		{
			if (!_copiedMaterial)
			{
				Debug.LogError("Please copy Material Properties first!");
				return;
			}
			foreach (Material material in lwgui.materialEditor.targets)
			{
				if (!VersionControlHelper.Checkout(material))
				{
					Debug.LogError("Material: '" + lwgui.material.name + "' unable to write!");
					return;
				}

				Undo.RecordObject(material, "Paste Material Properties");
				for (int i = 0; i < ShaderUtil.GetPropertyCount(_copiedMaterial.shader); i++)
				{
					var name = ShaderUtil.GetPropertyName(_copiedMaterial.shader, i);
					var type = ShaderUtil.GetPropertyType(_copiedMaterial.shader, i);
					PastePropertyValueToMaterial(material, name, name, type, valueMask);
				}
				if ((valueMask & (uint)CopyMaterialValueMask.Keyword) != 0)
					material.shaderKeywords = _copiedMaterial.shaderKeywords;
				if ((valueMask & (uint)CopyMaterialValueMask.RenderQueue) != 0)
					material.renderQueue = _copiedMaterial.renderQueue;
			}
		}

		private static void PastePropertyValueToMaterial(Material material, string srcName, string dstName)
		{
			for (int i = 0; i < ShaderUtil.GetPropertyCount(_copiedMaterial.shader); i++)
			{
				var name = ShaderUtil.GetPropertyName(_copiedMaterial.shader, i);
				if (name == srcName)
				{
					var type = ShaderUtil.GetPropertyType(_copiedMaterial.shader, i);
					PastePropertyValueToMaterial(material, srcName, dstName, type);
					return;
				}
			}
		}

		private static void PastePropertyValueToMaterial(Material material, string srcName, string dstName, ShaderUtil.ShaderPropertyType type, uint valueMask = (uint)CopyMaterialValueMask.All)
		{
			switch (type)
			{
				case ShaderUtil.ShaderPropertyType.Color:
					if ((valueMask & (uint)CopyMaterialValueMask.Vector) != 0)
						material.SetColor(dstName, _copiedMaterial.GetColor(srcName));
					break;
				case ShaderUtil.ShaderPropertyType.Vector:
					if ((valueMask & (uint)CopyMaterialValueMask.Vector) != 0)
						material.SetVector(dstName, _copiedMaterial.GetVector(srcName));
					break;
				case ShaderUtil.ShaderPropertyType.TexEnv:
					if ((valueMask & (uint)CopyMaterialValueMask.Texture) != 0)
						material.SetTexture(dstName, _copiedMaterial.GetTexture(srcName));
					break;
				// Float
				default:
					if ((valueMask & (uint)CopyMaterialValueMask.Float) != 0)
						material.SetFloat(dstName, _copiedMaterial.GetFloat(srcName));
					break;
			}
		}

		public static void DrawToolbarButtons(ref Rect toolBarRect, LWGUI lwgui)
		{
			// Copy
			var buttonRectOffset = toolBarRect.height + 2;
			var buttonRect = new Rect(toolBarRect.x, toolBarRect.y, toolBarRect.height, toolBarRect.height);
			toolBarRect.xMin += buttonRectOffset;
			if (GUI.Button(buttonRect, _guiContentCopy, Helper.guiStyles_IconButton))
			{
				_copiedMaterial = UnityEngine.Object.Instantiate(lwgui.material);
			}

			// Paste
			buttonRect.x += buttonRectOffset;
			toolBarRect.xMin += buttonRectOffset;
			// Right Click
			if (Event.current.type == EventType.MouseDown
			 && Event.current.button == 1
			 && buttonRect.Contains(Event.current.mousePosition))
			{
				EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), _pasteMaterialMenus, -1,
												(data, options, selected) =>
												{
													DoPasteMaterialProperties(lwgui, _pasteMaterialMenuValueMasks[selected]);
												}, null);
				Event.current.Use();
			}
			// Left Click
			if (GUI.Button(buttonRect, _guiContentPaste, Helper.guiStyles_IconButton))
			{
				DoPasteMaterialProperties(lwgui, (uint)CopyMaterialValueMask.All);
			}

			// Select Material Asset, jump from a Runtime Material Instance to a Material Asset
			buttonRect.x += buttonRectOffset;
			toolBarRect.xMin += buttonRectOffset;
			if (GUI.Button(buttonRect, _guiContentSelect, Helper.guiStyles_IconButton))
			{
				if (AssetDatabase.Contains(lwgui.material))
				{
					Selection.activeObject = lwgui.material;
				}
				else
				{
					// Get Material Asset name
					var name = lwgui.material.name;
					foreach (var nameEnd in _materialInstanceNameEnd)
					{
						if (name.EndsWith(nameEnd))
						{
							name = name.Substring(0, name.Length - nameEnd.Length);
							break;
						}
					}

					// Get path
					var guids = AssetDatabase.FindAssets("t:Material " + name);
					var paths = guids.Select(((guid, i) =>
					{
						var filePath = AssetDatabase.GUIDToAssetPath(guid);
						var fileName = System.IO.Path.GetFileNameWithoutExtension(filePath);
						return (fileName == name && filePath.EndsWith(".mat")) ? filePath : null;
					})).Where((s => !string.IsNullOrEmpty(s))).ToArray();

					// Select Asset
					if (paths.Length == 0)
					{
						Debug.LogError("Can not find Material Assets with name: " + name);
					}
					else if (paths.Length > 1)
					{
						var str = string.Empty;
						foreach (string path in paths)
						{
							str += "\n" + path;
						}
						Debug.LogWarning("Multiple Material Assets with the same name have been found, select only the first one:" + str);
						Selection.activeObject = AssetDatabase.LoadAssetAtPath<Material>(paths[0]);
					}
					else
					{
						Selection.activeObject = AssetDatabase.LoadAssetAtPath<Material>(paths[0]);
					}
				}
			}

			// Checkout
			buttonRect.x += buttonRectOffset;
			toolBarRect.xMin += buttonRectOffset;
			if (GUI.Button(buttonRect, _guiContentChechout, Helper.guiStyles_IconButton))
			{
				foreach (var material in lwgui.materialEditor.targets)
				{
					VersionControlHelper.Checkout(material);
				}
			}

			// Expand
			buttonRect.x += buttonRectOffset;
			toolBarRect.xMin += buttonRectOffset;
			if (GUI.Button(buttonRect, _guiContentExpand, Helper.guiStyles_IconButton))
			{
				foreach (var propertyStaticDataPair in lwgui.perShaderData.propertyDatas)
				{
					if (propertyStaticDataPair.Value.isMain || propertyStaticDataPair.Value.isAdvancedHeader)
						propertyStaticDataPair.Value.isExpanding = true;
				}
			}

			// Collapse
			buttonRect.x += buttonRectOffset;
			toolBarRect.xMin += buttonRectOffset;
			if (GUI.Button(buttonRect, _guiContentCollapse, Helper.guiStyles_IconButton))
			{
				foreach (var propertyStaticDataPair in lwgui.perShaderData.propertyDatas)
				{
					if (propertyStaticDataPair.Value.isMain || propertyStaticDataPair.Value.isAdvancedHeader)
						propertyStaticDataPair.Value.isExpanding = false;
				}
			}

			// Display Mode
			buttonRect.x += buttonRectOffset;
			toolBarRect.xMin += buttonRectOffset;
			var color = GUI.color;
			if (!lwgui.perShaderData.displayModeData.IsDefaultDisplayMode())
				GUI.color = Color.yellow;
			if (GUI.Button(buttonRect, _guiContentVisibility, Helper.guiStyles_IconButton))
			{
				string[] displayModeMenus = new[]
				{
					"Show All Advanced	(" + lwgui.perShaderData.displayModeData.advancedCount + " of " + lwgui.perShaderData.propertyDatas.Count + ")",
					"Show All Hidden	(" + lwgui.perShaderData.displayModeData.hiddenCount + " of " + lwgui.perShaderData.propertyDatas.Count + ")",
					"Show Only Modified	(" + lwgui.perFrameData.modifiedCount + " of " + lwgui.perShaderData.propertyDatas.Count + ")",
				};
				bool[] enabled = new[] { true, true, true };
				bool[] separator = new bool[3];
				int[] selected = new[]
				{
					lwgui.perShaderData.displayModeData.showAllAdvancedProperties	? 0 : -1,
					lwgui.perShaderData.displayModeData.showAllHiddenProperties		? 1 : -1,
					lwgui.perShaderData.displayModeData.showOnlyModifiedProperties	? 2 : -1,
				};
				ReflectionHelper.DisplayCustomMenuWithSeparators(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0),
															  displayModeMenus, enabled, separator, selected,
															  (data, options, selectedIndex) =>
															  {
																  switch (selectedIndex)
																  {
																	  case 0:
																		  lwgui.perShaderData.displayModeData.showAllAdvancedProperties = !lwgui.perShaderData.displayModeData.showAllAdvancedProperties;
																		  lwgui.perShaderData.ToggleShowAllAdvancedProperties();
																		  break;
																	  case 1:
																		  lwgui.perShaderData.displayModeData.showAllHiddenProperties = !lwgui.perShaderData.displayModeData.showAllHiddenProperties;
																		  break;
																	  case 2:
																		  lwgui.perShaderData.displayModeData.showOnlyModifiedProperties = !lwgui.perShaderData.displayModeData.showOnlyModifiedProperties;
																		  break;
																  }
															  });
			}
			GUI.color = color;

			toolBarRect.xMin += 2;
		}

		#endregion


		#region Search Field
		private static readonly int s_TextFieldHash = "EditorTextField".GetHashCode();
		private static readonly GUIContent[] _searchModeMenus =
			(new GUIContent[(int)SearchMode.Num]).Select(((guiContent, i) =>
			{
				if (i == (int)SearchMode.Num)
					return null;

				return new GUIContent(((SearchMode)i).ToString());
			})).ToArray();


		/// <returns>is has changed?</returns>
		public static bool DrawSearchField(Rect rect, LWGUI lwgui)
		{
			bool hasChanged = false;
			EditorGUI.BeginChangeCheck();

			var revertButtonRect = RevertableHelper.SplitRevertButtonRect(ref rect);

			// Get internal TextField ControlID
			int controlId = GUIUtility.GetControlID(s_TextFieldHash, FocusType.Keyboard, rect) + 1;

			// searching mode
			Rect modeRect = new Rect(rect);
			modeRect.width = 20f;
			if (Event.current.type == UnityEngine.EventType.MouseDown && modeRect.Contains(Event.current.mousePosition))
			{
				EditorUtility.DisplayCustomMenu(rect, _searchModeMenus, (int)lwgui.perShaderData.searchMode,
												(data, options, selected) =>
												{
													lwgui.perShaderData.searchMode = (SearchMode)selected;
													hasChanged = true;
												}, null);
				Event.current.Use();
			}

			lwgui.perShaderData.searchString = EditorGUI.TextField(rect, String.Empty, lwgui.perShaderData.searchString, guiStyles_ToolbarSearchTextFieldPopup);

			if (EditorGUI.EndChangeCheck())
				hasChanged = true;

			// revert button
			if (!string.IsNullOrEmpty(lwgui.perShaderData.searchString)
			 && RevertableHelper.DrawRevertButton(revertButtonRect))
			{
				lwgui.perShaderData.searchString = string.Empty;
				hasChanged = true;
				GUIUtility.keyboardControl = 0;
			}

			// display search mode
			if (GUIUtility.keyboardControl != controlId
			 && string.IsNullOrEmpty(lwgui.perShaderData.searchString)
			 && Event.current.type == UnityEngine.EventType.Repaint)
			{
				using (new EditorGUI.DisabledScope(true))
				{
#if UNITY_2019_2_OR_NEWER
					var disableTextRect = new Rect(rect.x, rect.y, rect.width,
												   guiStyles_ToolbarSearchTextFieldPopup.fixedHeight > 0.0
													   ? guiStyles_ToolbarSearchTextFieldPopup.fixedHeight
													   : rect.height);
#else
					var disableTextRect = rect;
					disableTextRect.yMin -= 3f;
#endif
					disableTextRect = guiStyles_ToolbarSearchTextFieldPopup.padding.Remove(disableTextRect);
					int fontSize = EditorStyles.label.fontSize;
					EditorStyles.label.fontSize = guiStyles_ToolbarSearchTextFieldPopup.fontSize;
					EditorStyles.label.Draw(disableTextRect, new GUIContent(lwgui.perShaderData.searchMode.ToString()), false, false, false, false);
					EditorStyles.label.fontSize = fontSize;
				}
			}

			if (hasChanged) lwgui.perShaderData.UpdateSearchFilter();

			return hasChanged;
		}

		#endregion


		#region Context Menu

		private static void EditPresetEvent(string mode, ShaderPropertyPreset presetAsset, ShaderPropertyPreset.Preset activePreset, MaterialProperty prop, LWGUI lwgui)
		{
			if (!VersionControlHelper.Checkout(presetAsset))
			{
				Debug.LogError("Can not edit the preset: " + presetAsset);
				return;
			}
			switch (mode)
			{
				case "Add":
				case "Update":
					activePreset.AddOrUpdateIncludeExtraProperties(lwgui, prop);
					break;
				case "Remove":
					activePreset.RemoveIncludeExtraProperties(lwgui, prop.name);
					break;
			}
			EditorUtility.SetDirty(presetAsset);
		}

		public static void DoPropertyContextMenus(Rect rect, MaterialProperty prop, LWGUI lwgui)
		{
			if (Event.current.type != EventType.ContextClick || !rect.Contains(Event.current.mousePosition)) return;

			Event.current.Use();
			var propStaticData = lwgui.perShaderData.propertyDatas[prop.name];
			var menus = new GenericMenu();

			// 2022+ Material Varant Menus
#if UNITY_2022_1_OR_NEWER
			ReflectionHelper.HandleApplyRevert(menus, prop);
#endif

			// Copy
			menus.AddItem(new GUIContent("Copy"), false, () =>
			{
				_copiedMaterial = UnityEngine.Object.Instantiate(lwgui.material);
				_copiedProps.Clear();
				_copiedProps.Add(prop.name);
				foreach (var extraPropName in propStaticData.extraPropNames)
				{
					_copiedProps.Add(extraPropName);
				}

				// Copy Children
				foreach (var childPropStaticData in propStaticData.children)
				{
					_copiedProps.Add(childPropStaticData.name);
					foreach (var extraPropName in childPropStaticData.extraPropNames)
					{
						_copiedProps.Add(extraPropName);
					}

					foreach (var childChildPropStaticData in childPropStaticData.children)
					{
						_copiedProps.Add(childChildPropStaticData.name);
						foreach (var extraPropName in childChildPropStaticData.extraPropNames)
						{
							_copiedProps.Add(extraPropName);
						}
					}

				}
			});

			// Paste
			GenericMenu.MenuFunction pasteAction = () =>
			{
				foreach (Material material in prop.targets)
				{
					if (!VersionControlHelper.Checkout(material))
					{
						Debug.LogError("Material: '" + lwgui.material.name + "' unable to write!");
						return;
					}

					Undo.RecordObject(material, "Paste Material Properties");

					var index = 0;

					PastePropertyValueToMaterial(material, _copiedProps[index++], prop.name);
					foreach (var extraPropName in propStaticData.extraPropNames)
					{
						if (index == _copiedProps.Count) break;
						PastePropertyValueToMaterial(material, _copiedProps[index++], extraPropName);
					}

					// Paste Children
					foreach (var childPropStaticData in propStaticData.children)
					{
						if (index == _copiedProps.Count) break;
						PastePropertyValueToMaterial(material, _copiedProps[index++], childPropStaticData.name);
						foreach (var extraPropName in childPropStaticData.extraPropNames)
						{
							if (index == _copiedProps.Count) break;
							PastePropertyValueToMaterial(material, _copiedProps[index++], extraPropName);
						}

						foreach (var childChildPropStaticData in childPropStaticData.children)
						{
							if (index == _copiedProps.Count) break;
							PastePropertyValueToMaterial(material, _copiedProps[index++], childChildPropStaticData.name);
							foreach (var extraPropName in childChildPropStaticData.extraPropNames)
							{
								if (index == _copiedProps.Count) break;
								PastePropertyValueToMaterial(material, _copiedProps[index++], extraPropName);
							}
						}
					}
				}
			};

			if (_copiedMaterial != null && _copiedProps.Count > 0 && GUI.enabled)
				menus.AddItem(new GUIContent("Paste"), false, pasteAction);
			else
				menus.AddDisabledItem(new GUIContent("Paste"));

			menus.AddSeparator("");

			// Copy Display Name
			menus.AddItem(new GUIContent("Copy Display Name"), false, () =>
			{
				EditorGUIUtility.systemCopyBuffer = propStaticData.displayName;
			});

			// Copy Property Names
			menus.AddItem(new GUIContent("Copy Property Names"), false, () =>
			{
				EditorGUIUtility.systemCopyBuffer = prop.name;
				foreach (var extraPropName in propStaticData.extraPropNames)
				{
					EditorGUIUtility.systemCopyBuffer += ", " + extraPropName;
				}
			});

			// menus.AddSeparator("");
			//
			// // Add to Favorites
			// menus.AddItem(new GUIContent("Add to Favorites"), false, () =>
			// {
			// });
			//
			// // Remove from Favorites
			// menus.AddItem(new GUIContent("Remove from Favorites"), false, () =>
			// {
			// });

			// Preset
			if (GUI.enabled)
			{
				menus.AddSeparator("");
				foreach (var activePresetData in lwgui.perFrameData.activePresets)
				{
					if (activePresetData.property == prop) continue;

					var activePreset = activePresetData.preset;
					var presetAsset = lwgui.perShaderData.propertyDatas[activePresetData.property.name].propertyPresetAsset;
					var presetPropDisplayName = lwgui.perShaderData.propertyDatas[activePresetData.property.name].displayName;

					if (activePreset.GetPropertyValue(prop.name) != null)
					{
						menus.AddItem(new GUIContent("Update to Preset/" + presetPropDisplayName + "/" + activePreset.presetName), false, () => EditPresetEvent("Update", presetAsset, activePreset, prop, lwgui));
						menus.AddItem(new GUIContent("Remove from Preset/" + presetPropDisplayName + "/" + activePreset.presetName), false, () => EditPresetEvent("Remove", presetAsset, activePreset, prop, lwgui));
					}
					else
					{
						menus.AddItem(new GUIContent("Add to Preset/" + presetPropDisplayName + "/" + activePreset.presetName), false, () => EditPresetEvent("Add", presetAsset, activePreset, prop, lwgui));
					}
				}
			}

			menus.ShowAsContext();
		}
		#endregion

	}
}