// Copyright (c) Jason Ma
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	public class ReflectionHelper
	{
		private static Assembly UnityEditor_Assembly = Assembly.GetAssembly(typeof(Editor));
		
		#region MaterialPropertyHandler
		private static Type         MaterialPropertyHandler_Type                    = UnityEditor_Assembly.GetType("UnityEditor.MaterialPropertyHandler");
		private static MethodInfo   MaterialPropertyHandler_GetHandler_Method       = MaterialPropertyHandler_Type.GetMethod("GetHandler", BindingFlags.Static | BindingFlags.NonPublic);
		private static PropertyInfo MaterialPropertyHandler_PropertyDrawer_Property = MaterialPropertyHandler_Type.GetProperty("propertyDrawer");
		private static FieldInfo    MaterialPropertyHandler_DecoratorDrawers_Field  = MaterialPropertyHandler_Type.GetField("m_DecoratorDrawers", BindingFlags.NonPublic | BindingFlags.Instance);

		public static MaterialPropertyDrawer GetPropertyDrawer(Shader shader, MaterialProperty prop, out List<MaterialPropertyDrawer> decoratorDrawers)
		{
			decoratorDrawers = new List<MaterialPropertyDrawer>();
			var handler = MaterialPropertyHandler_GetHandler_Method.Invoke(null, new System.Object[] { shader, prop.name });
			if (handler != null && handler.GetType() == MaterialPropertyHandler_Type)
			{
				decoratorDrawers = MaterialPropertyHandler_DecoratorDrawers_Field.GetValue(handler) as List<MaterialPropertyDrawer>;
				return MaterialPropertyHandler_PropertyDrawer_Property.GetValue(handler, null) as MaterialPropertyDrawer;
			}
			return null;
		}

		public static MaterialPropertyDrawer GetPropertyDrawer(Shader shader, MaterialProperty prop)
		{
			List<MaterialPropertyDrawer> decoratorDrawers;
			return GetPropertyDrawer(shader, prop, out decoratorDrawers);
		}
		#endregion


		#region MaterialEditor
		private static Type			MaterialEditor_Type									= typeof(MaterialEditor);
		private static MethodInfo   MaterialEditor_DoPowerRangeProperty_Method			= MaterialEditor_Type.GetMethod("DoPowerRangeProperty", BindingFlags.Static | BindingFlags.NonPublic);
		private static MethodInfo   MaterialEditor_DefaultShaderPropertyInternal_Method = MaterialEditor_Type.GetMethod("DefaultShaderPropertyInternal", BindingFlags.NonPublic | BindingFlags.Instance, null,
			new []{typeof(Rect), typeof(MaterialProperty), typeof(GUIContent)}, null);
#if !UNITY_2019_2_OR_NEWER
		private static FieldInfo	MaterialEditor_CustomShaderGUI_Field				= MaterialEditor_Type.GetField("m_CustomShaderGUI", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

		public static ShaderGUI GetCustomShaderGUI(MaterialEditor editor)
		{
#if !UNITY_2019_2_OR_NEWER
			return MaterialEditor_CustomShaderGUI_Field.GetValue(editor) as ShaderGUI;
#else
			return editor.customShaderGUI;
#endif
		}

		public static float DoPowerRangeProperty(Rect position, MaterialProperty prop, GUIContent label, float power)
		{
			return (float)MaterialEditor_DoPowerRangeProperty_Method.Invoke(null, new System.Object[] { position, prop, label, power });
		}

		public static void DefaultShaderPropertyInternal(MaterialEditor editor, Rect position, MaterialProperty prop, GUIContent label)
		{
			MaterialEditor_DefaultShaderPropertyInternal_Method.Invoke(editor, new System.Object[] { position, prop, label });
		}

		#endregion


		#region EditorUtility
		private static Type			EditorUtility_Type = typeof(EditorUtility);
		private static MethodInfo   EditorUtility_DisplayCustomMenuWithSeparators_Method = EditorUtility_Type.GetMethod("DisplayCustomMenuWithSeparators", BindingFlags.NonPublic | BindingFlags.Static, null,
			new []{typeof(Rect), typeof(string[]), typeof(bool[]), typeof(bool[]), typeof(int[]), typeof(EditorUtility.SelectMenuItemFunction), typeof(object), typeof(bool)}, null);

		public static void DisplayCustomMenuWithSeparators(
			Rect                                 position,
			string[]                             options,
			bool[]                               enabled,
			bool[]                               separator,
			int[]                                selected,
			EditorUtility.SelectMenuItemFunction callback,
			object                               userData = null,
			bool                                 showHotkey = false)
		{
			EditorUtility_DisplayCustomMenuWithSeparators_Method.Invoke(null, new System.Object[] { position, options, enabled, separator, selected, callback, userData, showHotkey });
		}
		#endregion


		#region EditorGUI

		private static Type         EditorGUI_Type            = typeof(EditorGUI);
		private static PropertyInfo EditorGUI_Indent_Property = EditorGUI_Type.GetProperty("indent", BindingFlags.NonPublic | BindingFlags.Static);

		public static float EditorGUI_Indent { get { return (float)EditorGUI_Indent_Property.GetValue(null, null); } }

		#endregion

		#region MaterialEnumDrawer
		// UnityEditor.MaterialEnumDrawer(string enumName)
		private static System.Type[] _types;

		public static System.Type[] GetAllTypes()
		{
			if (_types == null)
			{
				_types = ((IEnumerable<Assembly>)AppDomain.CurrentDomain.GetAssemblies())
						 .SelectMany<Assembly, System.Type>((Func<Assembly, IEnumerable<System.Type>>)
															(assembly =>
															{
																if (assembly == null)
																	return (IEnumerable<System.Type>)(new System.Type[0]);
																try
																{
																	return (IEnumerable<System.Type>)assembly.GetTypes();
																}
																catch (ReflectionTypeLoadException ex)
																{
																	Debug.LogError(ex);
																	return (IEnumerable<System.Type>)(new System.Type[0]);
																}
															})).ToArray<System.Type>();
			}
			return _types;
		}
		#endregion


		#region Ramp
		private static Type       EditorWindow_Type             = typeof(EditorWindow);
		private static MethodInfo EditorWindow_ShowModal_Method = EditorWindow_Type.GetMethod("ShowModal", BindingFlags.NonPublic | BindingFlags.Instance);

		// UnityEditor.EditorWindow.ShowModal
		// Other windows will not be accessible and any script recompilation will not happen until this window is closed
		// https://docs.unity3d.com/ScriptReference/EditorWindow.ShowModal.html
		public static void ShowModal(EditorWindow window)
		{
			#if UNITY_2019_3_OR_NEWER
				window.ShowModal();
			#else
				EditorWindow_ShowModal_Method.Invoke(window, null);
			#endif
		}
		#endregion


		#region MaterialProperty.PropertyData
#if UNITY_2022_1_OR_NEWER
		private static Type			MaterialProperty_Type					= typeof(MaterialProperty);
		private static Type			PropertyData_Type						= MaterialProperty_Type.GetNestedType("PropertyData", BindingFlags.NonPublic);
		// MergeStack(out bool lockedInChildren, out bool lockedByAncestor, out bool overriden)
		private static MethodInfo   PropertyData_MergeStack_Method			= PropertyData_Type.GetMethod("MergeStack", BindingFlags.Static | BindingFlags.NonPublic);
		// HandleApplyRevert(GenericMenu menu, bool singleEditing, UnityEngine.Object[] targets)
		private static MethodInfo   PropertyData_HandleApplyRevert_Method   = PropertyData_Type.GetMethod("HandleApplyRevert", BindingFlags.Static | BindingFlags.NonPublic);

		public static void HandleApplyRevert(GenericMenu menu, MaterialProperty prop)
		{
			System.Object[] parameters = new System.Object[3];
			ReflectionHelper.PropertyData_MergeStack_Method.Invoke(null, parameters);
			bool lockedInChildren = (bool)parameters[0];
			bool lockedByAncestor = (bool)parameters[1];
			bool overriden = (bool)parameters[2];
			bool singleEditing = prop.targets.Length == 1;

			if (overriden)
			{
				ReflectionHelper.PropertyData_HandleApplyRevert_Method.Invoke(null, new System.Object[]{menu, singleEditing, prop.targets});
				menu.AddSeparator("");
			}
		}
#endif
		#endregion
	}
}