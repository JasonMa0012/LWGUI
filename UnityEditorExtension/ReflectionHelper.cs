// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace LWGUI
{
    public static class ReflectionHelper
    {
        #region MaterialPropertyHandler

        private static readonly Type MaterialPropertyHandler_Type = Assembly.GetAssembly(typeof(Editor)).GetType("UnityEditor.MaterialPropertyHandler");
        private static readonly PropertyInfo MaterialPropertyHandler_PropertyDrawer_Property = MaterialPropertyHandler_Type.GetProperty("propertyDrawer");
        private static readonly FieldInfo MaterialPropertyHandler_DecoratorDrawers_Field = MaterialPropertyHandler_Type.GetField("m_DecoratorDrawers", BindingFlags.NonPublic | BindingFlags.Instance);

        public static MaterialPropertyDrawer GetPropertyDrawer(Shader shader, MaterialProperty prop, out List<MaterialPropertyDrawer> decoratorDrawers)
        {
            decoratorDrawers = new List<MaterialPropertyDrawer>();
            var handler = MaterialPropertyHandler.GetHandler(shader, prop.name);
            if (handler != null)
            {
                decoratorDrawers = MaterialPropertyHandler_DecoratorDrawers_Field.GetValue(handler) as List<MaterialPropertyDrawer>;
                return MaterialPropertyHandler_PropertyDrawer_Property.GetValue(handler, null) as MaterialPropertyDrawer;
            }

            return null;
        }

        public static MaterialPropertyDrawer GetPropertyDrawer(Shader shader, MaterialProperty prop)
        {
            return GetPropertyDrawer(shader, prop, out _);
        }

        public static void InvalidatePropertyCache(Shader shader)
        {
            MaterialPropertyHandler.InvalidatePropertyCache(shader);
        }


        #endregion


        #region MaterialEditor

        private static readonly Type MaterialEditor_Type = typeof(MaterialEditor);
        private static readonly PropertyInfo MaterialEditor_RendererForAnimationMode_Property = MaterialEditor_Type.GetProperty("rendererForAnimationMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly MethodInfo MaterialEditor_TexturePropertyBody_Method = MaterialEditor_Type.GetMethod("TexturePropertyBody", BindingFlags.NonPublic | BindingFlags.Instance);
        
        /// <summary>
        /// Override Vector/Range behavior to match its width with other GUIs
        /// </summary>
        public static void LwguiShaderProperty(this MaterialEditor editor, Rect position, MaterialProperty prop, GUIContent label, int labelIndent = 0)
        {
            editor.BeginAnimatedCheck(position, prop);
            EditorGUI.indentLevel += labelIndent;
            using (new EditorGUI.DisabledScope((prop.flags & MaterialProperty.PropFlags.PerRendererData) != 0))
                editor.LwguiShaderPropertyInternal(position, prop, label);
            EditorGUI.indentLevel -= labelIndent;
            editor.EndAnimatedCheck();
        }

        /// <summary>
        /// Override Vector/Range behavior to match its width with other GUIs
        /// </summary>
        public static void LwguiShaderPropertyInternal(this MaterialEditor editor, Rect position, MaterialProperty prop, GUIContent label)
        {
            MaterialPropertyHandler handler = MaterialPropertyHandler.GetHandler(((Material) editor.target).shader, prop.name);
            if (handler != null)
            {
                handler.OnGUI(ref position, prop, label.text != null ? label : new GUIContent(prop.displayName), editor);
                if (handler.propertyDrawer != null)
                    return;
            }
            editor.LwguiDefaultShaderPropertyInternal(position, prop, label);
        }

        /// <summary>
        /// Override Vector/Range behavior to match its width with other GUIs
        /// </summary>
        public static void LwguiDefaultShaderPropertyInternal(this MaterialEditor editor, Rect position, MaterialProperty prop, GUIContent label)
        {
            switch (prop.type)
            {
                case MaterialProperty.PropType.Vector:
                    VectorPropertyInternal(position, prop, label);
                    break;
                case MaterialProperty.PropType.Range:
                    DoPowerRangeProperty(position, prop, label);
                    break;
                default:
                    editor.DefaultShaderPropertyInternal(position, prop, label);
                    break;
            }
        }

        public static Vector4 VectorPropertyInternal(in Rect position, in MaterialProperty prop, in GUIContent label)
        {
            MaterialEditor.BeginProperty(position, prop);
            EditorGUI.BeginChangeCheck();
            // float labelWidth = EditorGUIUtility.labelWidth;
            // EditorGUIUtility.labelWidth = 0.0f;
            Vector4 vector4 = EditorGUI.Vector4Field(position, label, prop.vectorValue);
            // EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck())
                prop.vectorValue = vector4;
            MaterialEditor.EndProperty();
            return prop.vectorValue;
        }
        
        public static float DoPowerRangeProperty(Rect position, MaterialProperty prop, GUIContent label, float power = 1.0f)
        {
            MaterialEditor.BeginProperty(position, prop);
            EditorGUI.BeginChangeCheck();
            // float labelWidth = EditorGUIUtility.labelWidth;
            // EditorGUIUtility.labelWidth = 0.0f;
            bool flag = (double) prop.rangeLimits.x > (double) prop.rangeLimits.y;
            float sliderValue = Mathf.Clamp(prop.floatValue, flag ? prop.rangeLimits.y : prop.rangeLimits.x, flag ? prop.rangeLimits.x : prop.rangeLimits.y);
            float num = EditorGUI.PowerSlider(position, label, sliderValue, prop.rangeLimits.x, prop.rangeLimits.y, power);
            // EditorGUIUtility.labelWidth = labelWidth;
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = num;
            MaterialEditor.EndProperty();
            return prop.floatValue;
        }

        public static List<Renderer> GetMeshRenderersByMaterialEditor(this MaterialEditor materialEditor)
        {
            var outRenderers = new List<Renderer>();

            // MaterialEditor.ShouldEditorBeHidden()
            PropertyEditor property = materialEditor.propertyViewer as PropertyEditor;
            if (property)
            {
                GameObject gameObject = property.tracker.activeEditors[0].target as GameObject;
                if (gameObject)
                {
                    outRenderers.AddRange(gameObject.GetComponents<Renderer>());
                }
            }

            return outRenderers;
        }

        public static Renderer GetRendererForAnimationMode(this MaterialEditor materialEditor)
        {
            return MaterialEditor_RendererForAnimationMode_Property.GetValue(materialEditor, null) as Renderer;
        }

        public static Texture TexturePropertyBody(this MaterialEditor materialEditor, Rect position, MaterialProperty prop)
        {
            return MaterialEditor_TexturePropertyBody_Method.Invoke(materialEditor, new object[] { position, prop }) as Texture;
        }

        #endregion
        
        #region EditorUtility

        public static void DisplayCustomMenuWithSeparators(Rect position, string[] options, bool[] enabled, bool[] separator, int[] selected, EditorUtility.SelectMenuItemFunction callback, object userData = null, bool showHotkey = false)
        {
            EditorUtility.DisplayCustomMenuWithSeparators(position, options, enabled, separator, selected, callback, userData, showHotkey);
        }

        #endregion

        #region EditorGUI

        public static float EditorGUI_Indent => EditorGUI.indentLevel;

        private static readonly PropertyInfo EditorGUI_indent_Property = typeof(EditorGUI).GetProperty("indent", BindingFlags.Static | BindingFlags.NonPublic);
        public static float EditorGUI_indent => (float)EditorGUI_indent_Property.GetValue(null);

        #endregion
        
        #region EditorGUIUtility

        public static float EditorGUIUtility_contextWidth => EditorGUIUtility.contextWidth;

        #endregion
        
        #region EditorGUILayout

        public static float EditorGUILayout_kLabelFloatMinW => EditorGUILayout.kLabelFloatMinW;

        #endregion

        #region MaterialEnumDrawer

        // UnityEditor.MaterialEnumDrawer(string enumName)
        private static Type[] _types;

        public static Type[] GetAllTypes()
        {
            if (_types == null)
            {
                _types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(assembly =>
                    {
                        if (assembly == null)
                            return Type.EmptyTypes;
                        try
                        {
                            return assembly.GetTypes();
                        }
                        catch (ReflectionTypeLoadException ex)
                        {
                            Debug.LogError(ex);
                            return Type.EmptyTypes;
                        }
                    }).ToArray();
            }

            return _types;
        }

        #endregion
        
        #region MaterialProperty.PropertyData

#if UNITY_2022_1_OR_NEWER
        private static readonly Type MaterialProperty_Type = typeof(MaterialProperty);
        private static readonly Type PropertyData_Type = MaterialProperty_Type.GetNestedType("PropertyData", BindingFlags.NonPublic);
        private static readonly MethodInfo PropertyData_MergeStack_Method = PropertyData_Type.GetMethod("MergeStack", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly MethodInfo PropertyData_HandleApplyRevert_Method = PropertyData_Type.GetMethod("HandleApplyRevert", BindingFlags.Static | BindingFlags.NonPublic);

        public static void HandleApplyRevert(GenericMenu menu, MaterialProperty prop)
        {
            var parameters = new object[3];
            PropertyData_MergeStack_Method.Invoke(null, parameters);
            var overriden = (bool)parameters[2];
            var singleEditing = prop.targets.Length == 1;

            if (overriden)
            {
                PropertyData_HandleApplyRevert_Method.Invoke(null, new object[] { menu, singleEditing, prop.targets });
                menu.AddSeparator("");
            }
        }
#endif

        #endregion

        #region Animation

        public static bool MaterialAnimationUtility_OverridePropertyColor(MaterialProperty materialProp, Renderer target, out Color color)
        {
            return MaterialAnimationUtility.OverridePropertyColor(materialProp, target, out color);
        }

        #endregion
        
        #region GUI

        private static readonly MethodInfo gui_Button_Method = typeof(GUI).GetMethod("Button", BindingFlags.Static | BindingFlags.NonPublic);

        public static bool GUI_Button(Rect position, int id, GUIContent content, GUIStyle style)
        {
            return (bool)gui_Button_Method.Invoke(null, new object[] { position, id, content, style });
        }

        #endregion

        #region GradientEditor

        private static readonly FieldInfo k_MaxNumKeys_Field = typeof(GradientEditor).GetField("k_MaxNumKeys", BindingFlags.Static | BindingFlags.NonPublic);
        public static readonly int maxGradientKeyCount = (int)k_MaxNumKeys_Field.GetValue(null);


        private static readonly FieldInfo m_SelectedSwatch_Field = typeof(GradientEditor).GetField("m_SelectedSwatch", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static GradientEditor.Swatch GetSelectedSwatch(this GradientEditor gradientEditor)
        {
            return m_SelectedSwatch_Field.GetValue(gradientEditor) as GradientEditor.Swatch;
        }

        internal static void SetSelectedSwatch(this GradientEditor gradientEditor, GradientEditor.Swatch swatch)
        {
            m_SelectedSwatch_Field.SetValue(gradientEditor, swatch);
        }


        private static readonly FieldInfo m_RGBSwatches_Field = typeof(GradientEditor).GetField("m_RGBSwatches", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static List<GradientEditor.Swatch> GetRGBdSwatches(this GradientEditor gradientEditor)
        {
            return m_RGBSwatches_Field.GetValue(gradientEditor) as List<GradientEditor.Swatch>;
        }


        private static readonly FieldInfo m_AlphaSwatches_Field = typeof(GradientEditor).GetField("m_AlphaSwatches", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static List<GradientEditor.Swatch> GetAlphaSwatches(this GradientEditor gradientEditor)
        {
            return m_AlphaSwatches_Field.GetValue(gradientEditor) as List<GradientEditor.Swatch>;
        }


        private static object s_Styles_Value;
        private static readonly FieldInfo s_Styles_Field = typeof(GradientEditor).GetField("s_Styles", BindingFlags.Static | BindingFlags.NonPublic);
        public static void GradientEditor_SetStyles()
        {
            s_Styles_Value ??= Activator.CreateInstance(typeof(GradientEditor).GetNestedType("Styles", BindingFlags.NonPublic));
            s_Styles_Field.SetValue(null, s_Styles_Value);
        }


        private static readonly MethodInfo ShowSwatchArray_Method = typeof(GradientEditor)
            .GetMethod("ShowSwatchArray", BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(Rect), typeof(List<GradientEditor.Swatch>), typeof(bool) }, null);

        internal static void ShowSwatchArray(this GradientEditor gradientEditor, Rect position, List<GradientEditor.Swatch> swatches, bool isAlpha)
        {
            ShowSwatchArray_Method.Invoke(gradientEditor, new object[] { position, swatches, isAlpha });
        }


        private static readonly MethodInfo GetTime_Method = typeof(GradientEditor).GetMethod("GetTime", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static float GetTime(this GradientEditor gradientEditor, float actualTime)
        {
            return (float)GetTime_Method.Invoke(gradientEditor, new object[] { actualTime });
        }

        public static void PopupWindowWithoutFocus_Hide()
        {
            PopupWindowWithoutFocus.Hide();
        }
        
        #endregion

        #region CurveEditor

        private static readonly FieldInfo m_CurveEditor_Field = typeof(CurveEditorWindow).GetField("m_CurveEditor", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static CurveEditor GetCurveEditor(this CurveEditorWindow curveEditorWindow)
        {
            return m_CurveEditor_Field.GetValue(curveEditorWindow) as CurveEditor;
        }


        private static readonly MethodInfo AddKeyAtTime_Method = typeof(CurveEditor).GetMethod("AddKeyAtTime", BindingFlags.Instance | BindingFlags.NonPublic);
        internal static CurveSelection AddKeyAtTime(this CurveEditor curveEditor, CurveWrapper cw, float time)
        {
            return AddKeyAtTime_Method.Invoke(curveEditor, new object[] { cw, time }) as CurveSelection;
        }

        #endregion


        #region Type Lookup

        private static Dictionary<string, Type> _typeCache = new Dictionary<string, Type>();

        /// <summary>
        /// Get a Type by its name, searching all loaded assemblies.
        /// Supports both full type name (Namespace.ClassName) and simple type name.
        /// </summary>
        public static Type GetTypeByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                return null;

            if (_typeCache.TryGetValue(typeName, out var cachedType))
                return cachedType;

            // Try to get type directly
            var type = Type.GetType(typeName);
            if (type != null)
            {
                _typeCache[typeName] = type;
                return type;
            }

            // Search in all loaded assemblies
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    _typeCache[typeName] = type;
                    return type;
                }

                // Try to find by simple name (without namespace)
                foreach (var t in assembly.GetTypes())
                {
                    if (t.Name == typeName || t.FullName == typeName)
                    {
                        _typeCache[typeName] = t;
                        return t;
                    }
                }
            }

            _typeCache[typeName] = null;
            return null;
        }

        #endregion
    }
}