using System;
using System.Linq;
using System.Text.RegularExpressions;
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

		public static void AdaptiveFieldWidth(string str, GUIContent content, float extraWidth = 0)
		{
			AdaptiveFieldWidth(new GUIStyle(str), content, extraWidth);
		}

		public static void AdaptiveFieldWidth(GUIStyle style, GUIContent content, float extraWidth = 0)
		{
			var extraTextWidth = Mathf.Max(0, style.CalcSize(content).x + extraWidth - EditorGUIUtility.fieldWidth);
			EditorGUIUtility.labelWidth -= extraTextWidth;
			EditorGUIUtility.fieldWidth += extraTextWidth;
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

		public static GUIStyle guiStyles_IconButton = new GUIStyle("IconButton") { fixedHeight = 0, fixedWidth = 0};

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

		public static bool Foldout(Rect       position,
								   ref bool   isFolding,
								   bool       toggleValue,
								   bool       hasToggle,
								   GUIContent label)
		{
			var rect = position;

			// Background
			{
				var style = new GUIStyle("ShurikenModuleTitle");
				style.border = new RectOffset(15, 7, 4, 4);
				style.fixedHeight = 30;
				// Text
				style.font = new GUIStyle(EditorStyles.boldLabel).font;
				style.fontSize = (int)(style.fontSize * 1.5f);
				style.contentOffset = new Vector2(30f, -2f);

				var enabled = GUI.enabled;
				GUI.enabled = true;
				GUI.backgroundColor = isFolding ? Color.white : new Color(0.85f, 0.85f, 0.85f);
				GUI.Box(rect, label, style);
				GUI.backgroundColor = Color.white;
				GUI.enabled = enabled;
			}

			var toggleRect = new Rect(rect.x + 8f, rect.y + 7f, 13f, 13f);

			if (hasToggle)
			{
				EditorGUI.BeginChangeCheck();
				GUI.Toggle(toggleRect, EditorGUI.showMixedValue ? false : toggleValue, String.Empty,
						   new GUIStyle(EditorGUI.showMixedValue ? "ToggleMixed" : "Toggle"));
				if (EditorGUI.EndChangeCheck())
					toggleValue = !toggleValue;
			}

			// Background Click Event
			{
				var enabled = GUI.enabled;
				GUI.enabled = true;
				var e = Event.current;
				if (e.type == UnityEngine.EventType.MouseDown && rect.Contains(e.mousePosition))
				{
					isFolding = !isFolding;
					e.Use();
				}
				GUI.enabled = enabled;
			}

			return toggleValue;
		}

		// TODO: use Reflection
		public static void PowerSlider(MaterialProperty prop, float power, Rect position, GUIContent label)
		{
			int controlId = GUIUtility.GetControlID("EditorSliderKnob".GetHashCode(), FocusType.Passive, position);
			float left = prop.rangeLimits.x;
			float right = prop.rangeLimits.y;
			float start = left;
			float end = right;
			float value = prop.floatValue;
			float originValue = prop.floatValue;

			if ((double)power != 1.0)
			{
				start = Helper.PowPreserveSign(start, 1f / power);
				end = Helper.PowPreserveSign(end, 1f / power);
				value = Helper.PowPreserveSign(value, 1f / power);
			}

			EditorGUI.BeginChangeCheck();

			var labelWidth = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 0;

			var rectAfterLabel = EditorGUI.PrefixLabel(position, label);

			Rect sliderRect = MaterialEditor.GetFlexibleRectBetweenLabelAndField(position);
			sliderRect.xMin += 2;
			if (sliderRect.width >= 50f)
				// TODO: Slider Focus
				value = GUI.Slider(sliderRect, value, 0.0f, start, end, GUI.skin.horizontalSlider,
								   !EditorGUI.showMixedValue ? GUI.skin.horizontalSliderThumb : (GUIStyle)"SliderMixed",
								   true,
								   controlId);

			if ((double)power != 1.0)
				value = Helper.PowPreserveSign(value, power);

			position.xMin = Mathf.Max(rectAfterLabel.xMin, sliderRect.xMax - 10f);
			var floatRect = position;
			value = EditorGUI.FloatField(floatRect, value);

			if (value != originValue)
				prop.floatValue = Mathf.Clamp(value, Mathf.Min(left, right), Mathf.Max(left, right));

			EditorGUIUtility.labelWidth = labelWidth;
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

		public static void DrawHelpbox(Shader shader, MaterialProperty prop)
		{
			int lineCount;
			var helpboxStr = MetaDataHelper.GetPropertyHelpbox(shader, prop, out lineCount);
			if (!string.IsNullOrEmpty(helpboxStr))
			{
				var content =
					new GUIContent(helpboxStr, EditorGUIUtility.IconContent("console.infoicon").image as Texture2D);
				var style = EditorStyles.helpBox;
				var dpiScale = EditorGUIUtility.pixelsPerPoint;
				int fontSize = 12;
				style.fontSize = fontSize;

				var helpboxRect =
					EditorGUILayout.GetControlRect(true, style.CalcHeight(content, EditorGUIUtility.currentViewWidth));
				if (MetaDataHelper.IsSubProperty(shader, prop))
				{
					EditorGUI.indentLevel++;
					helpboxRect = EditorGUI.IndentedRect(helpboxRect);
					EditorGUI.indentLevel--;
				}
				helpboxRect.xMax -= RevertableHelper.revertButtonWidth;
				GUI.Label(helpboxRect, content, style);
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
		private static Material _copiedMaterial;

		private static Texture _iconCopy = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("9cdef444d18d2ce4abb6bbc4fed4d109"));
		private static Texture _iconPaste = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("8e7a78d02e4c3574998524a0842a8ccb"));
		private static Texture _iconSelect = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("6f44e40b24300974eb607293e4224ecc"));
		private static Texture _iconCheckout = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("72488141525eaa8499e65e52755cb6d0"));
		private static Texture _iconExpand = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("2382450e7f4ddb94c9180d6634c41378"));
		private static Texture _iconCollapse = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("929b6e5dfacc42b429d715a3e1ca2b57"));

		private static GUIContent _guiContentCopy = new GUIContent("", _iconCopy, "Copy Material Properties");
		private static GUIContent _guiContentPaste = new GUIContent("", _iconPaste, "Paste Material Properties\n\nRight-click to paste values by type.");
		private static GUIContent _guiContentSelect = new GUIContent("", _iconSelect, "Select the Material Asset\n\nUsed to jump from a Runtime Material Instance to a Material Asset.");
		private static GUIContent _guiContentChechout = new GUIContent("", _iconCheckout, "Checkout selected Material Assets");
		private static GUIContent _guiContentExpand = new GUIContent("", _iconExpand, "Expand All Groups");
		private static GUIContent _guiContentCollapse = new GUIContent("", _iconCollapse, "Collapse All Groups");

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
					switch (type)
					{
						case ShaderUtil.ShaderPropertyType.Color:
							if ((valueMask & (uint)CopyMaterialValueMask.Vector) != 0)
								material.SetColor(name, _copiedMaterial.GetColor(name));
							break;
						case ShaderUtil.ShaderPropertyType.Vector:
							if ((valueMask & (uint)CopyMaterialValueMask.Vector) != 0)
								material.SetVector(name, _copiedMaterial.GetVector(name));
							break;
						case ShaderUtil.ShaderPropertyType.TexEnv:
							if ((valueMask & (uint)CopyMaterialValueMask.Texture) != 0)
								material.SetTexture(name, _copiedMaterial.GetTexture(name));
							break;
						// Float
						default:
							if ((valueMask & (uint)CopyMaterialValueMask.Float) != 0)
								material.SetFloat(name, _copiedMaterial.GetFloat(name));
							break;
					}
				}
				if ((valueMask & (uint)CopyMaterialValueMask.Keyword) != 0)
					material.shaderKeywords = _copiedMaterial.shaderKeywords;
				if ((valueMask & (uint)CopyMaterialValueMask.RenderQueue) != 0)
					material.renderQueue = _copiedMaterial.renderQueue;
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
				GroupStateHelper.SetAllGroupFoldingAndCache(lwgui.shader, false);
			}

			// Collapse
			buttonRect.x += buttonRectOffset;
			toolBarRect.xMin += buttonRectOffset;
			if (GUI.Button(buttonRect, _guiContentCollapse, Helper.guiStyles_IconButton))
			{
				GroupStateHelper.SetAllGroupFoldingAndCache(lwgui.shader, true);
			}


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
		public static bool DrawSearchField(Rect rect, LWGUI lwgui, ref string searchingText, ref SearchMode searchMode)
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
			var toolbarSeachTextFieldPopup = new GUIStyle(toolbarSeachTextFieldPopupStr);

			bool isHasChanged = false;
			EditorGUI.BeginChangeCheck();

			var revertButtonRect = RevertableHelper.GetRevertButtonRect(EditorGUIUtility.singleLineHeight, rect);
			rect.xMax -= RevertableHelper.revertButtonWidth;
			// Get internal TextField ControlID
			int controlId = GUIUtility.GetControlID(s_TextFieldHash, FocusType.Keyboard, rect) + 1;

			// searching mode
			Rect modeRect = new Rect(rect);
			modeRect.width = 20f;
			if (Event.current.type == UnityEngine.EventType.MouseDown && modeRect.Contains(Event.current.mousePosition))
			{
				EditorUtility.DisplayCustomMenu(rect, _searchModeMenus, (int)searchMode,
												(data, options, selected) =>
												{
													if (lwgui.searchMode != (SearchMode)selected)
													{
														lwgui.searchMode = (SearchMode)selected;
														lwgui.updateSearchMode = true;
													}
												}, null);
				Event.current.Use();
			}

			// TODO: use Reflection -> controlId
			searchingText = EditorGUI.TextField(rect, String.Empty, searchingText, toolbarSeachTextFieldPopup);

			if (EditorGUI.EndChangeCheck())
				isHasChanged = true;

			// revert button
			if ((!string.IsNullOrEmpty(searchingText) || searchMode != SearchMode.Group)
			 && RevertableHelper.DrawRevertButton(revertButtonRect))
			{
				searchingText = string.Empty;
				searchMode = SearchMode.Group;
				isHasChanged = true;
				GUIUtility.keyboardControl = 0;
			}

			// display search mode
			if (GUIUtility.keyboardControl != controlId
			 && string.IsNullOrEmpty(searchingText)
			 && Event.current.type == UnityEngine.EventType.Repaint)
			{
				using (new EditorGUI.DisabledScope(true))
				{
#if UNITY_2019_2_OR_NEWER
					var disableTextRect = new Rect(rect.x, rect.y, rect.width,
												   toolbarSeachTextFieldPopup.fixedHeight > 0.0
													   ? toolbarSeachTextFieldPopup.fixedHeight
													   : rect.height);
#else
					var disableTextRect = rect;
					disableTextRect.yMin -= 3f;
#endif
					disableTextRect = toolbarSeachTextFieldPopup.padding.Remove(disableTextRect);
					int fontSize = EditorStyles.label.fontSize;
					EditorStyles.label.fontSize = toolbarSeachTextFieldPopup.fontSize;
					EditorStyles.label.Draw(disableTextRect, new GUIContent(searchMode.ToString()), false, false, false, false);
					EditorStyles.label.fontSize = fontSize;
				}
			}

			return isHasChanged;
		}

		#endregion
	}
}