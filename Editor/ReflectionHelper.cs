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
#if !UNITY_2019_2_OR_NEWER
		private static Type      MaterialEditor_Type                  = UnityEditor_Assembly.GetType("UnityEditor.MaterialEditor");
		private static FieldInfo MaterialEditor_CustomShaderGUI_Field = MaterialEditor_Type.GetField("m_CustomShaderGUI", BindingFlags.NonPublic | BindingFlags.Instance);
#endif

		public static ShaderGUI GetCustomShaderGUI(MaterialEditor editor)
		{
#if !UNITY_2019_2_OR_NEWER
			return MaterialEditor_CustomShaderGUI_Field.GetValue(editor) as ShaderGUI;
#else
			return editor.customShaderGUI;
#endif
		}
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
		private static Type       EditorWindow_Type             = UnityEditor_Assembly.GetType("UnityEditor.EditorWindow");
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
	}
}