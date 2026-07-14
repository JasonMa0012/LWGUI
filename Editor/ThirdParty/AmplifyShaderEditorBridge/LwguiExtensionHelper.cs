// Copyright (c) Jason Ma
// LWGUI - Amplify Shader Editor Extension Helper
// Provides UI drawing and attribute generation for the LWGUI bridge.
// This file is only compiled when LWGUI_ASE_INTEGRATION is defined.

#if LWGUI_ASE_INTEGRATION

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using AmplifyShaderEditor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Helper class for LWGUI integration in ASE
	/// Provides UI drawing methods that call into LWGUI core functionality
	/// </summary>
	public static class LwguiExtensionHelper
	{
		private static GUIContent s_helpIconContent;
		private static GUIStyle _lwguiPopupStyle;
		private static bool _lastProSkin;

		/// <summary>
		/// Returns a text color for placeholder/default values that works on the current editor skin.
		/// </summary>
		private static Color GetPlaceholderContentColor()
		{
			return EditorGUIUtility.isProSkin
				? new Color(0.65f, 0.65f, 0.65f, 1f)
				: new Color(0.35f, 0.35f, 0.35f, 0.5f);
		}

		/// <summary>
		/// Returns a popup style suitable for the current editor skin.
		/// The ASE InspectorPopdropdownStyle is based on PopupCurveDropdown and uses a light text
		/// color that is invisible in light themes, so we override the text colors in that case.
		/// </summary>
		private static GUIStyle GetLwguiPopupStyle()
		{
			bool isProSkin = EditorGUIUtility.isProSkin;
			if (_lwguiPopupStyle == null || _lastProSkin != isProSkin)
			{
				GUIStyle baseStyle = UIUtils.InspectorPopdropdownStyle ?? EditorStyles.popup;
				_lwguiPopupStyle = new GUIStyle(baseStyle);
				// ASE's InspectorPopdropdownStyle is based on PopupCurveDropdown, whose text is drawn
				// slightly higher than EditorGUILayout.LabelField. Shift it down to align both sides.
				_lwguiPopupStyle.contentOffset = new Vector2(0, 2);
				// The dropdown arrow is part of the background image and unaffected by contentOffset.
				// Shift the background down by 3 pixels so the arrow aligns with the text.
				RectOffset overflow = _lwguiPopupStyle.overflow;
				overflow.top -= 3;
				overflow.bottom += 3;
				_lwguiPopupStyle.overflow = overflow;

				if (!isProSkin)
					_lwguiPopupStyle.normal.textColor = EditorStyles.popup.normal.textColor;

				_lastProSkin = isProSkin;
			}

			return _lwguiPopupStyle;
		}

		#region Public API - Called from PropertyNode

		public static void DrawLwguiDrawerSection(
			LwguiAttributeData drawerData,
			ref bool foldoutState,
			Action onChanged,
			PropertyNode propertyNode = null)
		{
			// Ensure data is validated before drawing
			drawerData.ValidateAndRepair();

			DrawFoldoutSection("LWGUI Drawer Attribute", "https://github.com/JasonMa0012/LWGUI#extra-drawers", ref foldoutState, () =>
			{
				string displayName = string.IsNullOrEmpty(drawerData.drawerTypeName) ? "None" : drawerData.drawerTypeName;
				GUIStyle popupStyle = GetLwguiPopupStyle();

				EditorGUILayout.BeginHorizontal();
				{
					EditorGUILayout.LabelField("Type", GUILayout.Width(EditorGUIUtility.labelWidth - 4));

					if (GUILayout.Button(displayName, popupStyle, GUILayout.ExpandWidth(true)))
					{
						ShowDrawerOnlySelectionMenu(drawerData, onChanged, GetPropertyShaderType(propertyNode));
					}
				}
				EditorGUILayout.EndHorizontal();

				if (!string.IsNullOrEmpty(drawerData.drawerTypeName))
				{
					// Show constructor selection if multiple constructors available
					var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(drawerData.drawerTypeName);
					if (drawerInfo != null && drawerInfo.constructors.Count > 1)
					{
						EditorGUILayout.BeginHorizontal();
						{
							EditorGUILayout.LabelField("Constructor", GUILayout.Width(EditorGUIUtility.labelWidth - 4));

							var constructorInfo = drawerData.GetCurrentConstructor();
							int currentIndex = drawerInfo.constructors.IndexOf(constructorInfo) + 1;
							int totalCount = drawerInfo.constructors.Count;
							string constructorDisplayName = $"({currentIndex} / {totalCount})";

							if (GUILayout.Button(constructorDisplayName, popupStyle, GUILayout.ExpandWidth(true)))
							{
								ShowConstructorSelectionMenu(drawerData, drawerInfo, onChanged);
							}
						}
						EditorGUILayout.EndHorizontal();
					}

					DrawParametersForAttribute(drawerData, onChanged, propertyNode);
				}
			}, menu => PopulateDrawerContextMenu(menu, drawerData, onChanged));
		}

		public static void DrawLwguiDecoratorsSection(
			List<LwguiAttributeData> decorators,
			ref bool foldoutState,
			Action onChanged,
			PropertyNode propertyNode,
			ref ReorderableList reorderableList)
		{
			var list = reorderableList;
			if (list == null || list.list != decorators)
			{
				list = new ReorderableList(decorators, typeof(LwguiAttributeData), true, false, true, true)
				{
					headerHeight = 0,
					footerHeight = 0,
					showDefaultBackground = true,

					elementHeightCallback = index =>
				{
					if (index < 0 || index >= decorators.Count)
						return EditorGUIUtility.singleLineHeight + 4;

					var attr = decorators[index];
					// Ensure data is validated
					attr.ValidateAndRepair();

					// Base height for Type label + dropdown
					float baseHeight = EditorGUIUtility.singleLineHeight + 4;

					if (!string.IsNullOrEmpty(attr.drawerTypeName))
					{
						var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(attr.drawerTypeName);

						// Add height for Constructor label + dropdown if multiple constructors
						if (drawerInfo != null && drawerInfo.constructors.Count > 1)
						{
							baseHeight += EditorGUIUtility.singleLineHeight + 4;
						}

						// Add height for parameters
						var constructorInfo = attr.GetCurrentConstructor();
						if (constructorInfo != null && constructorInfo.parameters.Count > 0)
							baseHeight += constructorInfo.parameters.Count * (EditorGUIUtility.singleLineHeight + 2);
					}

					return baseHeight;
				},

					drawElementCallback = (rect, index, isActive, isFocused) =>
					{
						if (index < 0 || index >= decorators.Count)
							return;

						DrawDecoratorElementUI(rect, decorators[index], index, isActive, isFocused, onChanged, propertyNode);
					},

					onReorderCallback = _ => onChanged?.Invoke(),

					onAddCallback = _ =>
					{
						decorators.Add(new LwguiAttributeData());
						onChanged?.Invoke();
					},

					onRemoveCallback = list =>
					{
						if (list.index >= 0 && list.index < decorators.Count)
						{
							decorators.RemoveAt(list.index);
							if (list.index >= decorators.Count && decorators.Count > 0)
								list.index = decorators.Count - 1;
							onChanged?.Invoke();
						}
					}
				};
				reorderableList = list;
			}

			DrawFoldoutSection("LWGUI Decorator Attributes", "https://github.com/JasonMa0012/LWGUI#extra-decorators", ref foldoutState, () =>
			{
				float totalHeight = list.GetHeight() + EditorGUIUtility.singleLineHeight;
				Rect listRect = EditorGUILayout.GetControlRect(false, totalHeight);
				list.DoList(listRect);
			}, menu => PopulateDecoratorsContextMenu(menu, decorators, onChanged));
		}

		public static List<string> CollectExistingParameterValues(PropertyNode currentNode, string paramName, string paramType)
		{
			var values = new HashSet<string>();

			if (currentNode?.ContainerGraph?.AllNodes == null)
				return values.ToList();

			foreach (var node in currentNode.ContainerGraph.AllNodes)
			{
				if (!(node is PropertyNode propertyNode)) continue;

				var nodeData = LwguiPropertyNodeExtension.GetData(propertyNode);

				if (nodeData.drawer != null && !string.IsNullOrEmpty(nodeData.drawer.drawerTypeName))
					CollectValuesFromAttribute(nodeData.drawer, paramName, paramType, values);

				if (nodeData.decorators != null)
				{
					foreach (var decorator in nodeData.decorators)
					{
						if (!string.IsNullOrEmpty(decorator.drawerTypeName))
							CollectValuesFromAttribute(decorator, paramName, paramType, values);
					}
				}
			}

			return values.OrderBy(v => v).ToList();
		}

		public static string GenerateAttributesString(LwguiAttributeData drawer, List<LwguiAttributeData> decorators)
		{
			var result = new StringBuilder();

			if (drawer != null && !string.IsNullOrEmpty(drawer.drawerTypeName))
			{
				string attr = drawer.GenerateAttributeString();
				if (!string.IsNullOrEmpty(attr)) result.Append(attr);
			}

			if (decorators != null)
			{
				foreach (var d in decorators)
				{
					if (!string.IsNullOrEmpty(d.drawerTypeName))
					{
						string attr = d.GenerateAttributeString();
						if (!string.IsNullOrEmpty(attr)) result.Append(attr);
					}
				}
			}

			return result.ToString();
		}

		private static ShaderPropertyType? GetPropertyShaderType(PropertyNode propertyNode)
		{
			if (propertyNode == null) return null;

			var nodeType = propertyNode.GetType();

			if (nodeType.Name == "RangedFloatNode")
			{
				var floatModeField = nodeType.GetField("m_floatMode", BindingFlags.Instance | BindingFlags.NonPublic);
				if (floatModeField != null && !(bool)floatModeField.GetValue(propertyNode))
					return ShaderPropertyType.Range;
				return ShaderPropertyType.Float;
			}

			if (nodeType.Name == "IntNode")
			{
				var intModeField = nodeType.GetField("m_intMode", BindingFlags.Instance | BindingFlags.NonPublic);
				if (intModeField != null && !(bool)intModeField.GetValue(propertyNode))
					return ShaderPropertyType.Range;
				return ShaderPropertyType.Int;
			}

			if (nodeType.Name == "ColorNode") return ShaderPropertyType.Color;
			if (nodeType.Name == "Vector4Node" || nodeType.Name == "Vector3Node" || nodeType.Name == "Vector2Node")
				return ShaderPropertyType.Vector;
			if (nodeType.Name == "TexturePropertyNode" || nodeType.Name == "TextureArrayNode")
				return ShaderPropertyType.Texture;

			return null;
		}

		#endregion

		#region Private Implementation - Parameter Drawing

		private static void DrawParametersForAttribute(LwguiAttributeData attributeData, Action onChanged, PropertyNode propertyNode = null)
		{
			var constructorInfo = attributeData.GetCurrentConstructor();
			if (constructorInfo == null || constructorInfo.parameters.Count == 0) return;

			for (int i = 0; i < constructorInfo.parameters.Count; i++)
			{
				var param = constructorInfo.parameters[i];

				while (attributeData.parameters.Count <= i)
					attributeData.parameters.Add("");

				Rect totalRect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight);
				DrawSingleParameterUI(totalRect, attributeData, i, param, propertyNode, onChanged);
			}
		}

		private static void DrawSingleParameterUI(
			Rect totalRect,
			LwguiAttributeData attributeData,
			int paramIndex,
			LwguiParameterInfo param,
			PropertyNode propertyNode,
			Action onChanged)
		{
			bool hasError = attributeData.HasValidationError(paramIndex);

			switch (param.parameterType)
			{
				case LwguiParameterType.Bool:
					DrawBoolParameter(totalRect, attributeData, paramIndex, param, onChanged, propertyNode, hasError);
					break;
				case LwguiParameterType.Enum:
					DrawEnumParameter(totalRect, attributeData, paramIndex, param, onChanged, propertyNode, hasError);
					break;
				case LwguiParameterType.Float:
					DrawFloatParameter(totalRect, attributeData, paramIndex, param, onChanged, propertyNode, hasError);
					break;
				case LwguiParameterType.Int:
					DrawIntParameter(totalRect, attributeData, paramIndex, param, onChanged, propertyNode, hasError);
					break;
				case LwguiParameterType.PropertyName:
					DrawPropertyNameParameter(totalRect, attributeData, paramIndex, param, propertyNode, onChanged, hasError);
					break;
				case LwguiParameterType.PassName:
					DrawPassNameParameter(totalRect, attributeData, paramIndex, param, propertyNode, onChanged, hasError);
					break;
				case LwguiParameterType.Keyword:
				case LwguiParameterType.String:
				default:
					DrawStringParameter(totalRect, attributeData, paramIndex, param, propertyNode, onChanged, hasError);
					break;
			}
		}

		#region Parameter Type Specific UI

		private static void DrawBoolParameter(Rect totalRect, LwguiAttributeData attributeData, int paramIndex, LwguiParameterInfo param, Action onChanged, PropertyNode propertyNode, bool hasError)
		{
			Rect labelRect = new Rect(totalRect.x, totalRect.y, EditorGUIUtility.labelWidth - 4, totalRect.height);
			Rect fieldRect = new Rect(totalRect.x + EditorGUIUtility.labelWidth, totalRect.y, totalRect.width - EditorGUIUtility.labelWidth, totalRect.height);

			EditorGUI.LabelField(labelRect, param.name);

			string currentValue = paramIndex < attributeData.parameters.Count ? attributeData.parameters[paramIndex] : "";
			bool currentBool = ParseBoolValue(currentValue);

			Color originalColor = GUI.color;
			if (hasError)
				GUI.color = Color.red;

			EditorGUI.BeginChangeCheck();
			bool newBool = EditorGUI.Toggle(fieldRect, currentBool);
			if (EditorGUI.EndChangeCheck())
			{
				while (attributeData.parameters.Count <= paramIndex)
					attributeData.parameters.Add("");
				attributeData.parameters[paramIndex] = newBool ? "true" : "false";
				attributeData.SetParameterValue(param.name, attributeData.parameters[paramIndex]);
				ValidateAndShowErrors(attributeData, paramIndex, param, propertyNode);
				onChanged?.Invoke();
			}

			if (hasError)
				GUI.color = originalColor;
		}

		private static void DrawEnumParameter(Rect totalRect, LwguiAttributeData attributeData, int paramIndex, LwguiParameterInfo param, Action onChanged, PropertyNode propertyNode, bool hasError)
		{
			Rect labelRect = new Rect(totalRect.x, totalRect.y, EditorGUIUtility.labelWidth - 4, totalRect.height);
			Rect fieldRect = new Rect(totalRect.x + EditorGUIUtility.labelWidth, totalRect.y, totalRect.width - EditorGUIUtility.labelWidth, totalRect.height);

			EditorGUI.LabelField(labelRect, param.name);

			string currentValue = paramIndex < attributeData.parameters.Count ? attributeData.parameters[paramIndex] : "";
			bool isEmpty = string.IsNullOrEmpty(currentValue);

			// Get options from parameter info
			var options = param.enumOptions?.ToArray() ?? new string[0];
			if (options.Length == 0)
			{
				DrawStringParameter(totalRect, attributeData, paramIndex, param, propertyNode, onChanged, hasError);
				return;
			}

			int currentIndex = Array.IndexOf(options, currentValue);
			if (currentIndex < 0)
			{
				// Use default value if available
				if (isEmpty && param.hasDefaultValue && !string.IsNullOrEmpty(param.defaultValue))
				{
					currentIndex = Array.IndexOf(options, param.defaultValue);
				}
				if (currentIndex < 0) currentIndex = 0;
			}

			Color originalColor = GUI.color;
			if (hasError)
				GUI.color = Color.red;

			EditorGUI.BeginChangeCheck();
			int newIndex = EditorGUI.Popup(fieldRect, currentIndex, options);
			if (EditorGUI.EndChangeCheck())
			{
				while (attributeData.parameters.Count <= paramIndex)
					attributeData.parameters.Add("");
				attributeData.parameters[paramIndex] = options[newIndex];
				attributeData.SetParameterValue(param.name, options[newIndex]);
				ValidateAndShowErrors(attributeData, paramIndex, param, propertyNode);
				onChanged?.Invoke();
			}

			if (hasError)
				GUI.color = originalColor;
		}

		private static void DrawFloatParameter(Rect totalRect, LwguiAttributeData attributeData, int paramIndex, LwguiParameterInfo param, Action onChanged, PropertyNode propertyNode, bool hasError)
		{
			DrawNumericParameter(totalRect, attributeData, paramIndex, param, propertyNode, onChanged, hasError,
				(value, isEmpty, hasDefault, defaultValue) =>
				{
					float result = 0f;
					if (!float.TryParse(value, out result) && isEmpty && hasDefault)
						float.TryParse(defaultValue, out result);
					return result;
				},
				(rect, val, isEmpty, errorColor) =>
				{
					Color originalColor = GUI.color;
					Color originalContentColor = GUI.contentColor;
					if (errorColor != Color.white)
						GUI.color = errorColor;
					else if (isEmpty)
						GUI.contentColor = GetPlaceholderContentColor();

					float result = EditorGUI.FloatField(rect, val);
					GUI.color = originalColor;
					GUI.contentColor = originalContentColor;
					return result;
				},
				val => val.ToString());
		}

		private static void DrawIntParameter(Rect totalRect, LwguiAttributeData attributeData, int paramIndex, LwguiParameterInfo param, Action onChanged, PropertyNode propertyNode, bool hasError)
		{
			DrawNumericParameter(totalRect, attributeData, paramIndex, param, propertyNode, onChanged, hasError,
				(value, isEmpty, hasDefault, defaultValue) =>
				{
					int result = 0;
					if (!int.TryParse(value, out result) && isEmpty && hasDefault)
						int.TryParse(defaultValue, out result);
					return result;
				},
				(rect, val, isEmpty, errorColor) =>
				{
					Color originalColor = GUI.color;
					Color originalContentColor = GUI.contentColor;
					if (errorColor != Color.white)
						GUI.color = errorColor;
					else if (isEmpty)
						GUI.contentColor = GetPlaceholderContentColor();

					int result = EditorGUI.IntField(rect, val);
					GUI.color = originalColor;
					GUI.contentColor = originalContentColor;
					return result;
				},
				val => val.ToString());
		}

		private static void DrawNumericParameter<T>(Rect totalRect, LwguiAttributeData attributeData, int paramIndex,
			LwguiParameterInfo param, PropertyNode propertyNode, Action onChanged, bool hasError,
			Func<string, bool, bool, string, T> parseValue,
			Func<Rect, T, bool, Color, T> drawField,
			Func<T, string> valueToString)
		{
			Rect labelRect = new Rect(totalRect.x, totalRect.y, EditorGUIUtility.labelWidth - 4, totalRect.height);
			Rect fieldRect = new Rect(totalRect.x + EditorGUIUtility.labelWidth, totalRect.y, totalRect.width - EditorGUIUtility.labelWidth, totalRect.height);

			EditorGUI.LabelField(labelRect, param.name);

			string currentValue = paramIndex < attributeData.parameters.Count ? attributeData.parameters[paramIndex] : "";
			bool isEmpty = string.IsNullOrEmpty(currentValue);

			T currentValueTyped = parseValue(currentValue, isEmpty, param.hasDefaultValue, param.defaultValue);

			Color errorColor = hasError ? Color.red : Color.white;
			EditorGUI.BeginChangeCheck();
			T newValue = drawField(fieldRect, currentValueTyped, isEmpty, errorColor);

			if (EditorGUI.EndChangeCheck())
			{
				while (attributeData.parameters.Count <= paramIndex)
					attributeData.parameters.Add("");
				attributeData.parameters[paramIndex] = valueToString(newValue);
				attributeData.SetParameterValue(param.name, attributeData.parameters[paramIndex]);
				ValidateAndShowErrors(attributeData, paramIndex, param, propertyNode);
				onChanged?.Invoke();
			}
		}

		private static void DrawPropertyNameParameter(Rect totalRect, LwguiAttributeData attributeData, int paramIndex, LwguiParameterInfo param, PropertyNode propertyNode, Action onChanged, bool hasError)
		{
			Rect labelRect = new Rect(totalRect.x, totalRect.y, EditorGUIUtility.labelWidth - 4, totalRect.height);
			Rect fieldRect = new Rect(totalRect.x + EditorGUIUtility.labelWidth, totalRect.y, totalRect.width - EditorGUIUtility.labelWidth, totalRect.height);

			EditorGUI.LabelField(labelRect, param.name);

			var propertyNames = new List<string>();
			if (propertyNode?.ContainerGraph?.AllNodes != null)
			{
				foreach (var node in propertyNode.ContainerGraph.AllNodes)
				{
					if (node is PropertyNode propNode && !string.IsNullOrEmpty(propNode.PropertyName))
						propertyNames.Add(propNode.PropertyName);
				}
			}

			string currentValue = paramIndex < attributeData.parameters.Count ? attributeData.parameters[paramIndex] : "";

			Color originalColor = GUI.color;
			if (hasError)
				GUI.color = Color.red;

			int currentIndex = propertyNames.IndexOf(currentValue);

			if (propertyNames.Count > 0 && currentIndex >= 0)
			{
				EditorGUI.BeginChangeCheck();
				int newIndex = EditorGUI.Popup(fieldRect, currentIndex, propertyNames.ToArray());
				if (EditorGUI.EndChangeCheck())
				{
					while (attributeData.parameters.Count <= paramIndex)
						attributeData.parameters.Add("");
					attributeData.parameters[paramIndex] = propertyNames[newIndex];
					attributeData.SetParameterValue(param.name, propertyNames[newIndex]);
					ValidateAndShowErrors(attributeData, paramIndex, param, propertyNode);
					onChanged?.Invoke();
				}
			}
			else
			{
				EditorGUI.BeginChangeCheck();
				string newValue = EditorGUI.TextField(fieldRect, currentValue);
				if (EditorGUI.EndChangeCheck())
				{
					while (attributeData.parameters.Count <= paramIndex)
						attributeData.parameters.Add("");
					attributeData.parameters[paramIndex] = newValue;
					attributeData.SetParameterValue(param.name, newValue);
					ValidateAndShowErrors(attributeData, paramIndex, param, propertyNode);
					onChanged?.Invoke();
				}
			}

			if (hasError)
				GUI.color = originalColor;
		}

		private static void DrawPassNameParameter(Rect totalRect, LwguiAttributeData attributeData, int paramIndex, LwguiParameterInfo param, PropertyNode propertyNode, Action onChanged, bool hasError)
		{
			Rect labelRect = new Rect(totalRect.x, totalRect.y, EditorGUIUtility.labelWidth - 4, totalRect.height);
			Rect fieldRect = new Rect(totalRect.x + EditorGUIUtility.labelWidth, totalRect.y, totalRect.width - EditorGUIUtility.labelWidth, totalRect.height);

			EditorGUI.LabelField(labelRect, param.name);

			var passNames = GetPassNamesFromShader(propertyNode);

			string currentValue = paramIndex < attributeData.parameters.Count ? attributeData.parameters[paramIndex] : "";

			Color originalColor = GUI.color;
			if (hasError)
				GUI.color = Color.red;

			var displayOptions = new List<string> { "Empty" };
			displayOptions.AddRange(passNames);
			int currentIndex = passNames.IndexOf(currentValue);
			int popupIndex = currentIndex >= 0 ? currentIndex + 1 : 0;

			EditorGUI.BeginChangeCheck();
			int newPopupIndex = EditorGUI.Popup(fieldRect, popupIndex, displayOptions.ToArray());
			if (EditorGUI.EndChangeCheck())
			{
				while (attributeData.parameters.Count <= paramIndex)
					attributeData.parameters.Add("");
				string newValue = newPopupIndex > 0 ? passNames[newPopupIndex - 1] : "";
				attributeData.parameters[paramIndex] = newValue;
				attributeData.SetParameterValue(param.name, newValue);
				ValidateAndShowErrors(attributeData, paramIndex, param, propertyNode);
				onChanged?.Invoke();
			}

			if (hasError)
				GUI.color = originalColor;
		}

		private static void DrawStringParameter(Rect totalRect, LwguiAttributeData attributeData, int paramIndex, LwguiParameterInfo param, PropertyNode propertyNode, Action onChanged, bool hasError)
		{
			const float dropdownWidth = 17;

			Rect labelRect = new Rect(totalRect.x, totalRect.y, EditorGUIUtility.labelWidth - 4, totalRect.height);
			Rect fieldRect = new Rect(totalRect.x + EditorGUIUtility.labelWidth, totalRect.y, totalRect.width - EditorGUIUtility.labelWidth, totalRect.height);
			Rect dropdownRect = new Rect(totalRect.x + totalRect.width - dropdownWidth, totalRect.y, dropdownWidth, totalRect.height);

			EditorGUI.LabelField(labelRect, param.name);

			// Dropdown button overlaps the right edge of the text field.
			// Handle its click before the TextField so the button takes click priority in the overlapping area.
			GUIStyle dropdownStyle = UIUtils.InspectorPopdropdownFallback ?? GUI.skin.button;
			bool dropdownClicked = GUI.Button(dropdownRect, GUIContent.none, dropdownStyle);

			string currentValue = paramIndex < attributeData.parameters.Count ? attributeData.parameters[paramIndex] : "";
			bool isEmpty = string.IsNullOrEmpty(currentValue);

			Color originalColor = GUI.color;
			Color originalContentColor = GUI.contentColor;
			if (hasError)
			{
				GUI.color = Color.red;
			}
			else if (isEmpty)
			{
				// Use contentColor so only the text is tinted, not the TextField background.
				GUI.contentColor = GetPlaceholderContentColor();
			}

			EditorGUI.BeginChangeCheck();
			string displayValue = isEmpty ? param.GetDefaultDisplayText() : currentValue;
			string newValue = EditorGUI.TextField(fieldRect, displayValue);

			GUI.color = originalColor;
			GUI.contentColor = originalContentColor;

			if (EditorGUI.EndChangeCheck())
			{
				while (attributeData.parameters.Count <= paramIndex)
					attributeData.parameters.Add("");
				attributeData.parameters[paramIndex] = newValue;
				attributeData.SetParameterValue(param.name, newValue);
				ValidateAndShowErrors(attributeData, paramIndex, param, propertyNode);
				onChanged?.Invoke();
			}

			// Re-draw dropdown indicator on top of the TextField for correct visual layering
			if (Event.current.type == EventType.Repaint)
				dropdownStyle.Draw(dropdownRect, GUIContent.none, false, false, false, false);

			if (dropdownClicked)
			{
				GUIUtility.keyboardControl = 0;
				EditorGUI.FocusTextInControl(null);
				GUI.FocusControl(null);

				if (propertyNode != null)
				{
					var existingValues = CollectExistingParameterValues(propertyNode, param.name, param.parameterType.ToString());
					if (existingValues.Count > 0)
						ShowParameterValueMenu(existingValues, attributeData, paramIndex, param.name, onChanged);
				}
			}
		}

		#endregion

		#region Validation

		private static void ValidateAndShowErrors(LwguiAttributeData attributeData, int paramIndex, LwguiParameterInfo param, PropertyNode propertyNode)
		{
			if (paramIndex >= attributeData.parameters.Count) return;

			string value = attributeData.parameters[paramIndex];
			bool wasError = attributeData.HasValidationError(paramIndex);

			attributeData.SetValidationError(paramIndex, false);

			if (string.IsNullOrEmpty(value)) return;

			var context = CreateValidationContext(propertyNode);
			var result = LwguiParameterValidator.Validate(value, param, context);

			if (!result.isValid)
			{
				attributeData.SetValidationError(paramIndex, true);
				if (!wasError)
					Debug.LogError($"[LWGUI] Validation error in {attributeData.drawerTypeName}.{param.name}: {result.errorMessage}");
			}
		}

		private static ValidationContext CreateValidationContext(PropertyNode propertyNode)
		{
			var context = new ValidationContext();

			if (propertyNode?.ContainerGraph?.AllNodes != null)
			{
				// Collect property names from all PropertyNodes
				foreach (var node in propertyNode.ContainerGraph.AllNodes)
				{
					if (node is PropertyNode propNode && !string.IsNullOrEmpty(propNode.PropertyName))
					{
						context.existingPropertyNames.Add(propNode.PropertyName);
					}
				}
			}

			// TODO: Get pass names from shader
			// This requires access to the compiled shader or template data

			return context;
		}

		#endregion

		#region Helpers

		private static bool ParseBoolValue(string value)
		{
			if (string.IsNullOrEmpty(value)) return false;
			string lower = value.ToLower();
			return lower == "true" || lower == "on" || lower == "1";
		}

		private static List<string> GetPassNamesFromShader(PropertyNode propertyNode)
		{
			var passNames = new List<string>();

			if (propertyNode?.ContainerGraph?.CurrentMasterNode is TemplateMultiPassMasterNode templateMasterNode)
			{
				var template = templateMasterNode.CurrentTemplate;
				if (template != null)
				{
					// Get all subshaders and their passes
					foreach (var subShader in template.SubShaders)
					{
						foreach (var pass in subShader.Passes)
						{
							// Try to get LightMode from pass tags
							string lightMode = GetLightModeFromPass(pass);
							if (!string.IsNullOrEmpty(lightMode) && !passNames.Contains(lightMode))
							{
								passNames.Add(lightMode);
							}
						}
					}
				}
			}

			return passNames;
		}

		private static string GetLightModeFromPass(TemplatePass pass)
		{
			// Try to get LightMode from pass modules tag data
			if (pass.Modules?.TagData != null && pass.Modules.TagData.DataCheck == TemplateDataCheck.Valid)
			{
				foreach (var tag in pass.Modules.TagData.Tags)
				{
					if (tag.Name.Equals("LightMode", StringComparison.OrdinalIgnoreCase))
					{
						return tag.Value;
					}
				}
			}

			// Fallback: try to extract from pass name
			if (pass.PassNameContainer != null && pass.PassNameContainer.IsValid)
			{
				return pass.PassNameContainer.Data;
			}

			return null;
		}

		#endregion

		#endregion

		#region Private Implementation - Decorator UI

		private static bool DrawDecoratorPopupButton(Rect rect, string text, GUIStyle style)
		{
			Color originalBackgroundColor = GUI.backgroundColor;
			try
			{
				// Darken the dropdown background (and its arrow) in light theme for better contrast.
				if (!EditorGUIUtility.isProSkin)
					GUI.backgroundColor = new Color(0.5f, 0.5f, 0.5f);

				return GUI.Button(rect, text, style);
			}
			finally
			{
				GUI.backgroundColor = originalBackgroundColor;
			}
		}

		private static void DrawDecoratorElementUI(Rect rect, LwguiAttributeData decoratorData, int index, bool isActive, bool isFocused, Action onChanged, PropertyNode propertyNode = null)
		{
			decoratorData.ValidateAndRepair();

			if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
			{
				GenericMenu menu = new GenericMenu();
				PopulateDecoratorContextMenu(menu, decoratorData, onChanged);
				menu.ShowAsContext();
				Event.current.Use();
			}

			GUIStyle popupStyle = GetLwguiPopupStyle();
			float labelWidth = EditorGUIUtility.labelWidth - 4;
			float fieldX = rect.x + labelWidth;
			float fieldWidth = rect.width - labelWidth;

			Rect typeLabelRect = new Rect(rect.x, rect.y + 2, labelWidth, EditorGUIUtility.singleLineHeight);
			Rect typeRect = new Rect(fieldX, rect.y + 2, fieldWidth, EditorGUIUtility.singleLineHeight);

			EditorGUI.LabelField(typeLabelRect, "Type");

			string displayName = string.IsNullOrEmpty(decoratorData.drawerTypeName) ? "Select Decorator..." : decoratorData.drawerTypeName;
			if (DrawDecoratorPopupButton(typeRect, displayName, popupStyle))
				ShowDecoratorSelectionMenu(decoratorData, onChanged);

			if (!string.IsNullOrEmpty(decoratorData.drawerTypeName))
			{
				var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(decoratorData.drawerTypeName);
				if (drawerInfo != null && drawerInfo.constructors.Count > 1)
				{
					Rect constructorLabelRect = new Rect(rect.x, rect.y + typeRect.height + 4, labelWidth, EditorGUIUtility.singleLineHeight);
					Rect constructorRect = new Rect(fieldX, rect.y + typeRect.height + 4, fieldWidth, EditorGUIUtility.singleLineHeight);

					EditorGUI.LabelField(constructorLabelRect, "Constructor");

					var constructorInfo = decoratorData.GetCurrentConstructor();
					int currentIndex = drawerInfo.constructors.IndexOf(constructorInfo) + 1;
					int totalCount = drawerInfo.constructors.Count;
					string constructorDisplayName = $"({currentIndex} / {totalCount})";

					if (DrawDecoratorPopupButton(constructorRect, constructorDisplayName, popupStyle))
					{
						ShowConstructorSelectionMenu(decoratorData, drawerInfo, onChanged);
					}

					DrawDecoratorParameters(rect, decoratorData, onChanged, typeRect.height + constructorRect.height + 8, propertyNode);
				}
				else
				{
					DrawDecoratorParameters(rect, decoratorData, onChanged, typeRect.height + 4, propertyNode);
				}
			}
		}

		private static void DrawDecoratorParameters(Rect rect, LwguiAttributeData decoratorData, Action onChanged, float yOffset, PropertyNode propertyNode)
		{
			var constructorInfo = decoratorData.GetCurrentConstructor();
			if (constructorInfo == null || constructorInfo.parameters.Count == 0) return;

			float lineHeight = EditorGUIUtility.singleLineHeight;
			float spacing = 2;

			for (int i = 0; i < constructorInfo.parameters.Count; i++)
			{
				var param = constructorInfo.parameters[i];
				while (decoratorData.parameters.Count <= i)
					decoratorData.parameters.Add("");

				Rect paramRect = new Rect(rect.x, rect.y + yOffset + i * (lineHeight + spacing), rect.width, lineHeight);
				DrawSingleParameterUI(paramRect, decoratorData, i, param, propertyNode, onChanged);
			}
		}

		#endregion

		#region Private Implementation - Menus

		private static void ShowDrawerOnlySelectionMenu(LwguiAttributeData drawerData, Action onChanged, ShaderPropertyType? propType = null)
		{
			var drawers = LwguiDrawerDiscovery.GetDrawers();
			if (propType.HasValue)
				drawers = drawers.Where(d => d.IsSupportedPropertyType(propType.Value)).ToList();
			ShowTypeSelectionMenu(drawerData, drawers, true, onChanged);
		}

		private static void ShowDecoratorSelectionMenu(LwguiAttributeData decoratorData, Action onChanged)
		{
			ShowTypeSelectionMenu(decoratorData, LwguiDrawerDiscovery.GetDecorators(), false, onChanged);
		}

		private static void ShowTypeSelectionMenu(LwguiAttributeData data, List<LwguiDrawerInfo> types, bool includeNoneOption, Action onChanged)
		{
			GenericMenu menu = new GenericMenu();

			if (includeNoneOption)
			{
				menu.AddItem(new GUIContent("None"), string.IsNullOrEmpty(data.drawerTypeName), () =>
			{
				data.drawerTypeName = string.Empty;
				data.parameters.Clear();
				data.namedParameters.Clear();
				data.constructorSignature = "";
				onChanged?.Invoke();
			});
				menu.AddSeparator("");
			}

			foreach (var categoryGroup in types.GroupBy(d => d.categoryPath ?? "Uncategorized").OrderBy(g => g.Key))
			{
				string prefix = string.IsNullOrEmpty(categoryGroup.Key) || categoryGroup.Key == "Uncategorized" ? "" : $"{categoryGroup.Key}/";

				foreach (var type in categoryGroup.OrderBy(d => d.order).ThenBy(d => d.displayName))
				{
					// If type has multiple constructors, show submenu
					if (type.constructors.Count > 1)
					{
						string menuPath = prefix + type.displayName;

						// Add submenu items for each constructor (all are already valid)
						for (int i = 0; i < type.constructors.Count; i++)
						{
							var constructor = type.constructors[i];
							string constructorPath = $"{menuPath}/{i + 1}. {constructor.displayName}";
							bool isSelected = data.drawerTypeName == type.typeName && data.constructorSignature == constructor.GetFullSignature();

							string capturedTypeName = type.typeName;
							string capturedSignature = constructor.GetFullSignature();
							bool capturedIsDecorator = !includeNoneOption;

							menu.AddItem(new GUIContent(constructorPath), isSelected, () =>
							{
								data.drawerTypeName = capturedTypeName;
								data.constructorSignature = capturedSignature;
								data.isDecorator = capturedIsDecorator;

								var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(capturedTypeName);
								var constructorInfo = drawerInfo?.GetConstructorBySignature(capturedSignature);
								if (constructorInfo != null)
								{
									data.namedParameters = constructorInfo.parameters.Select(p => new NamedParameterValue
									{
										name = p.name,
										value = ""
									}).ToList();
									data.parameters.Clear();
									foreach (var p in constructorInfo.parameters)
										data.parameters.Add("");
								}

								onChanged?.Invoke();
							});
						}
					}
					else
					{
						// Single constructor - direct selection
					string menuPath = prefix + type.displayName;
					bool isSelected = data.drawerTypeName == type.typeName;

					// Capture variables for closure
					string capturedTypeName = type.typeName;
					bool capturedIsDecorator = !includeNoneOption;

					menu.AddItem(new GUIContent(menuPath), isSelected, () =>
					{
						data.drawerTypeName = capturedTypeName;
						data.isDecorator = capturedIsDecorator;

						var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(capturedTypeName);
						var constructorInfo = drawerInfo?.GetMainConstructor();
						if (constructorInfo != null)
						{
							data.constructorSignature = constructorInfo.GetFullSignature();
							data.namedParameters = constructorInfo.parameters.Select(p => new NamedParameterValue
							{
								name = p.name,
								value = ""
							}).ToList();
							data.parameters.Clear();
							foreach (var p in constructorInfo.parameters)
								data.parameters.Add("");
						}

						onChanged?.Invoke();
					});
					}
				}
			}

			menu.ShowAsContext();
		}

		private static void ShowConstructorSelectionMenu(LwguiAttributeData data, LwguiDrawerInfo drawerInfo, Action onChanged)
		{
			GenericMenu menu = new GenericMenu();

			for (int i = 0; i < drawerInfo.constructors.Count; i++)
			{
				var constructor = drawerInfo.constructors[i];
				bool isSelected = data.constructorSignature == constructor.GetFullSignature();
				string capturedSignature = constructor.GetFullSignature();

				menu.AddItem(new GUIContent($"{i + 1}. {constructor.displayName}"), isSelected, () =>
				{
					data.SwitchConstructor(capturedSignature);
					onChanged?.Invoke();
				});
			}

			menu.ShowAsContext();
		}

		private static void ShowParameterValueMenu(List<string> values, LwguiAttributeData data, int paramIndex, string paramName, Action onChanged)
		{
			GenericMenu menu = new GenericMenu();
			string currentValue = paramIndex < data.parameters.Count ? data.parameters[paramIndex] : "";

			foreach (var value in values)
			{
				string captured = value;
				bool isSelected = currentValue == captured;

				menu.AddItem(new GUIContent(captured), isSelected, () =>
				{
					GUIUtility.keyboardControl = 0;
					EditorGUI.FocusTextInControl(null);
					GUI.FocusControl(null);

					while (data.parameters.Count <= paramIndex)
						data.parameters.Add("");
					data.parameters[paramIndex] = captured;
					data.SetParameterValue(paramName, captured);
					onChanged?.Invoke();
				});
			}

			menu.ShowAsContext();
		}

		#endregion

		#region Private Helpers

		private static void DrawFoldoutSection(string title, string helpUrl, ref bool foldoutState, Action contentAction, Action<GenericMenu> populateContextMenu = null)
		{
			Color cachedColor = GUI.color;
			GUI.color = new Color(cachedColor.r, cachedColor.g, cachedColor.b, 0.5f);
			EditorGUILayout.BeginHorizontal(UIUtils.MenuItemToolbarStyle);
			GUI.color = cachedColor;

			bool value = GUILayout.Toggle(foldoutState, title, UIUtils.MenuItemToggleStyle);
			if (Event.current.button == Constants.FoldoutMouseId)
				foldoutState = value;

			GUILayout.FlexibleSpace();
			DrawHelpButton(helpUrl);

			EditorGUILayout.EndHorizontal();

			Rect headerRect = GUILayoutUtility.GetLastRect();
			headerRect = new Rect(0, headerRect.y, EditorGUIUtility.currentViewWidth, headerRect.height);

			if (populateContextMenu != null
				&& Event.current.type == EventType.ContextClick
				&& headerRect.Contains(Event.current.mousePosition))
			{
				GenericMenu menu = new GenericMenu();
				populateContextMenu(menu);
				menu.ShowAsContext();
				Event.current.Use();
			}

			if (!foldoutState)
				return;

			cachedColor = GUI.color;
			GUI.color = new Color(cachedColor.r, cachedColor.g, cachedColor.b, EditorGUIUtility.isProSkin ? 0.5f : 0.25f);
			EditorGUILayout.BeginVertical(UIUtils.MenuItemBackgroundStyle);
			GUI.color = cachedColor;

			EditorGUI.indentLevel++;

			contentAction?.Invoke();

			EditorGUI.indentLevel--;
			EditorGUILayout.Separator();
			EditorGUILayout.EndVertical();

			if (populateContextMenu != null)
			{
				Rect contentEndRect = GUILayoutUtility.GetLastRect();
				Rect contentRect = new Rect(0, headerRect.yMax, EditorGUIUtility.currentViewWidth, contentEndRect.yMax - headerRect.yMax);

				if (Event.current.type == EventType.ContextClick
					&& contentRect.Contains(Event.current.mousePosition))
				{
					GenericMenu menu = new GenericMenu();
					populateContextMenu(menu);
					menu.ShowAsContext();
					Event.current.Use();
				}
			}
		}

		private static void DrawHelpButton(string url)
		{
			if (s_helpIconContent == null)
			{
				s_helpIconContent = EditorGUIUtility.IconContent("_Help");
			}

			if (GUILayout.Button(s_helpIconContent, EditorStyles.iconButton, GUILayout.Width(20), GUILayout.Height(18)))
			{
				Help.BrowseURL(url);
			}
		}

		private static void CollectValuesFromAttribute(LwguiAttributeData attrData, string paramName, string paramType, HashSet<string> values)
		{
			var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(attrData.drawerTypeName);
			if (drawerInfo == null) return;

			// Check if this parameter name exists in any constructor of this drawer
			if (!drawerInfo.allParameterNames.Contains(paramName, StringComparer.OrdinalIgnoreCase))
				return;

			// Get the currently selected constructor using signature hash
			var constructorInfo = attrData.GetCurrentConstructor();
			if (constructorInfo == null) return;

			// Find the parameter in the current constructor
			for (int i = 0; i < constructorInfo.parameters.Count; i++)
			{
				var param = constructorInfo.parameters[i];

				if (!string.Equals(param.name, paramName, StringComparison.OrdinalIgnoreCase)) continue;

				// Compare parameter types
				string currentParamType = param.parameterType.ToString();
				if (!string.Equals(currentParamType, paramType, StringComparison.OrdinalIgnoreCase)) continue;

				if (i < attrData.parameters.Count)
				{
					string value = attrData.parameters[i];
					if (string.IsNullOrEmpty(value)) continue;
					if (LwguiDrawerDiscovery.IsDefaultValue(value, param)) continue;
					values.Add(value);
				}
			}
		}

		#endregion

		#region Copy/Paste Operations

		private static LwguiAttributeData _copiedAttribute;
		private static List<LwguiAttributeData> _copiedAttributes;

		private static LwguiAttributeData DeepCopy(LwguiAttributeData source)
		{
			return JsonUtility.FromJson<LwguiAttributeData>(JsonUtility.ToJson(source));
		}

		private static void CopyAttribute(LwguiAttributeData data)
		{
			_copiedAttribute = DeepCopy(data);
		}

		private static void CopyAttributes(List<LwguiAttributeData> attributes)
		{
			_copiedAttributes = new List<LwguiAttributeData>(attributes.Count);
			foreach (var attr in attributes)
				_copiedAttributes.Add(DeepCopy(attr));
		}

		private static void PasteAttributeToExisting(LwguiAttributeData target, Action onChanged)
		{
			JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(_copiedAttribute), target);
			target.ValidateAndRepair();
			target.ClearValidationErrors();
			onChanged?.Invoke();
		}

		private static void PasteAttributesToList(List<LwguiAttributeData> targetList, Action onChanged)
		{
			targetList.Clear();
			foreach (var attr in _copiedAttributes)
			{
				attr.ValidateAndRepair();
				targetList.Add(DeepCopy(attr));
			}
			onChanged?.Invoke();
		}

		private static void PopulateDrawerContextMenu(GenericMenu menu, LwguiAttributeData drawerData, Action onChanged)
		{
			menu.AddItem(new GUIContent("Copy Drawer"), false, () => CopyAttribute(drawerData));
			if (_copiedAttribute != null)
				menu.AddItem(new GUIContent("Paste Drawer"), false, () => PasteAttributeToExisting(drawerData, onChanged));
			else
				menu.AddDisabledItem(new GUIContent("Paste Drawer"));
		}

		private static void PopulateDecoratorContextMenu(GenericMenu menu, LwguiAttributeData decoratorData, Action onChanged)
		{
			menu.AddItem(new GUIContent("Copy Decorator"), false, () => CopyAttribute(decoratorData));
			if (_copiedAttribute != null)
				menu.AddItem(new GUIContent("Paste Decorator"), false, () => PasteAttributeToExisting(decoratorData, onChanged));
			else
				menu.AddDisabledItem(new GUIContent("Paste Decorator"));
		}

		private static void PopulateDecoratorsContextMenu(GenericMenu menu, List<LwguiAttributeData> decorators, Action onChanged)
		{
			menu.AddItem(new GUIContent("Copy All Decorators"), false, () => CopyAttributes(decorators));
			if (_copiedAttributes != null && _copiedAttributes.Count > 0)
				menu.AddItem(new GUIContent("Paste All Decorators"), false, () => PasteAttributesToList(decorators, onChanged));
			else
				menu.AddDisabledItem(new GUIContent("Paste All Decorators"));
		}

		#endregion
	}
}

#endif // LWGUI_ASE_INTEGRATION
