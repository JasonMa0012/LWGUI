using System;
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
			Debug.Assert(keyWords.Length >= 1 && index < keyWords.Length && index >= 0, "KeyWords Length: " + keyWords.Length + " or Index: " + index + " Error! ");
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


		#region Draw GUI for Drawer

		// TODO: use Reflection
		// copy and edit of https://github.com/GucioDevs/SimpleMinMaxSlider/blob/master/Assets/SimpleMinMaxSlider/Scripts/Editor/MinMaxSliderDrawer.cs
		public static Rect[] SplitRect(Rect rectToSplit, int n)
		{
			Rect[] rects = new Rect[n];

			for (int i = 0; i < n; i++)
			{
				rects[i] = new Rect(rectToSplit.position.x + (i * rectToSplit.width / n), rectToSplit.position.y, rectToSplit.width / n, rectToSplit.height);
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

		public static bool Foldout(Rect position, ref bool isFolding, bool toggleValue, bool hasToggle, GUIContent label)
		{
			var style = new GUIStyle("ShurikenModuleTitle");
			style.border = new RectOffset(15, 7, 4, 4);
			style.fixedHeight = 30;
			// Text
			style.font = new GUIStyle(EditorStyles.boldLabel).font;
			style.fontSize = (int)(style.fontSize * 1.5f);
			style.contentOffset = new Vector2(30f, -2f);

			var rect = position; //GUILayoutUtility.GetRect(position.width, 24f, style);

			GUI.backgroundColor = isFolding ? Color.white : new Color(0.85f, 0.85f, 0.85f);
			GUI.Box(rect, label, style);
			GUI.backgroundColor = Color.white;

			var toggleRect = new Rect(rect.x + 8f, rect.y + 7f, 13f, 13f);

			if (hasToggle)
			{
				EditorGUI.BeginChangeCheck();
				GUI.Toggle(toggleRect, EditorGUI.showMixedValue ? false : toggleValue, String.Empty, new GUIStyle(EditorGUI.showMixedValue ? "ToggleMixed" : "Toggle"));
				if (EditorGUI.EndChangeCheck())
					toggleValue = !toggleValue;
			}

			var e = Event.current;
			if (e.type == UnityEngine.EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				isFolding = !isFolding;
				e.Use();
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
				value = GUI.Slider(sliderRect, value, 0.0f, start, end, GUI.skin.horizontalSlider, !EditorGUI.showMixedValue ? GUI.skin.horizontalSliderThumb : (GUIStyle)"SliderMixed", true,
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

		public static readonly float helpboxSingleLineHeight = 12.5f;

		public static void DrawHelpbox(Shader shader, MaterialProperty prop)
		{
			int lineCount;
			var helpboxStr = MetaDataHelper.GetPropertyHelpbox(shader, prop, out lineCount);
			if (!string.IsNullOrEmpty(helpboxStr))
			{
				var content = new GUIContent(helpboxStr, EditorGUIUtility.IconContent("console.infoicon").image as Texture2D);
				var style = EditorStyles.helpBox;
				var dpiScale = EditorGUIUtility.pixelsPerPoint;
				int fontSize = 12;
				style.fontSize = fontSize;
				
				var helpboxRect = EditorGUILayout.GetControlRect(true, style.CalcHeight(content, EditorGUIUtility.currentViewWidth));
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

		public static void DrawLogo()
		{
			var logoRect = EditorGUILayout.GetControlRect(false, _logo.height);
			var w = logoRect.width;
			logoRect.xMin += w * 0.5f - _logo.width * 0.5f;
			logoRect.xMax -= w * 0.5f - _logo.width * 0.5f;

			if (EditorGUIUtility.currentViewWidth >= logoRect.width)
			{
				var c = GUI.color;
				GUI.color = new Color(c.r, c.g, c.b, 0.4f);
				if (logoRect.Contains(Event.current.mousePosition))
				{
					GUI.color = new Color(c.r, c.g, c.b, 0.8f);
					if (Event.current.type == UnityEngine.EventType.MouseDown)
						Application.OpenURL("https://github.com/JasonMa0012/LWGUI");
				}
				GUI.DrawTexture(logoRect, _logo);
				GUI.color = c;
				GUI.Label(logoRect, new GUIContent(String.Empty, "LWGUI (Light Weight Shader GUI)\n\n"
															   + "A Lightweight, Flexible, Powerful Unity Shader GUI system.\n\n"
															   + "Copyright (c) Jason Ma"));
			}
		}

		private static readonly int s_TextFieldHash = "EditorTextField".GetHashCode();
		private static readonly GUIContent[] _searchModeMenus = new[]
		{
			new GUIContent(SearchMode.All.ToString()),
			new GUIContent(SearchMode.Modified.ToString())
		};

		/// <returns>is has changed?</returns>
		public static bool DrawSearchField(ref string searchingText, ref SearchMode searchMode, LWGUI lwgui)
		{
			var toolbarSeachTextFieldPopup = new GUIStyle("ToolbarSeachTextFieldPopup");

			bool isHasChanged = false;
			EditorGUI.BeginChangeCheck();

			var rect = EditorGUILayout.GetControlRect();
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
			if ((!string.IsNullOrEmpty(searchingText) || searchMode != SearchMode.All) && RevertableHelper.DrawRevertButton(revertButtonRect))
			{
				searchingText = string.Empty;
				searchMode = SearchMode.All;
				isHasChanged = true;
				GUIUtility.keyboardControl = 0;
			}

			// display search mode
			if (GUIUtility.keyboardControl != controlId && string.IsNullOrEmpty(searchingText) && Event.current.type == UnityEngine.EventType.Repaint)
			{
				using (new EditorGUI.DisabledScope(true))
				{
					Rect position1 = toolbarSeachTextFieldPopup.padding.Remove(new Rect(rect.x, rect.y, rect.width,
																						toolbarSeachTextFieldPopup.fixedHeight > 0.0 ? toolbarSeachTextFieldPopup.fixedHeight : rect.height));
					int fontSize = EditorStyles.label.fontSize;
					EditorStyles.label.fontSize = toolbarSeachTextFieldPopup.fontSize;
					EditorStyles.label.Draw(position1, new GUIContent(searchMode.ToString()), false, false, false, false);
					EditorStyles.label.fontSize = fontSize;
				}
			}

			return isHasChanged;
		}

		#endregion
	}
}