// Copyright (c) 2022 Jason Ma

using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace LWGUI
{
	public enum LwguiEventType
	{
		Init,
		Repaint
	}
	
	
    public class LWGUI : ShaderGUI
    {
		// Used for access to all props in Drawer
        public MaterialProperty[] props;
        public MaterialEditor     materialEditor;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            this.props = props;
            this.materialEditor = materialEditor;
			RevertableHelper.defaultMaterial = new Material((materialEditor.target as Material).shader);
			
			// LWGUI header
			var headerRect = EditorGUILayout.GetControlRect();
			headerRect.xMax -= RevertableHelper.revertButtonWidth;
			EditorGUI.TextField(headerRect, "", "Search", new GUIStyle("ToolbarSeachTextFieldPopup"));

            // base.OnGUI(materialEditor, props);
			{
				// move fields left to make rect for Revert Button
				materialEditor.SetDefaultGUIWidths();
				EditorGUIUtility.fieldWidth += RevertableHelper.revertButtonWidth;
				EditorGUIUtility.labelWidth -= RevertableHelper.revertButtonWidth;
				RevertableHelper.fieldWidth = EditorGUIUtility.fieldWidth;
				RevertableHelper.labelWidth = EditorGUIUtility.labelWidth;
				
				for (int index = 0; index < props.Length; ++index)
				{
					var prop = props[index];
					if ((prop.flags & MaterialProperty.PropFlags.HideInInspector) == 0)
					{
						var height = materialEditor.GetPropertyHeight(prop, prop.displayName);
						// ignored when in Folding Group
						if (height <= 0) continue;
						
						var rect = EditorGUILayout.GetControlRect(true, height, EditorStyles.layerMaskField);
						var revertButtonRect = RevertableHelper.GetRevertButtonRect(prop, rect);
						rect.xMax -= RevertableHelper.revertButtonWidth;

						// Process some builtin types display misplaced
						switch (prop.type)
						{
							case MaterialProperty.PropType.Texture:
							case MaterialProperty.PropType.Range:
								materialEditor.SetDefaultGUIWidths();
								break;
							default:
								RevertableHelper.SetRevertableGUIWidths();
								break;
						}
						
						RevertableHelper.RevertButton(revertButtonRect, prop, materialEditor);
						var label = new GUIContent(prop.displayName, "Property Name: " + prop.name);
						materialEditor.ShaderProperty(rect, prop, label);
					}
				}
				materialEditor.SetDefaultGUIWidths();

				EditorGUILayout.Space();
				EditorGUILayout.Space();
				if (SupportedRenderingFeatures.active.editableMaterialRenderQueue)
				{
					materialEditor.RenderQueueField();
				}
				materialEditor.EnableInstancingField();
				materialEditor.DoubleSidedGIField();
			}
			
			
			// LWGUI logo
			EditorGUILayout.Space();
			Helper.DrawLogo();
		}


		public static MaterialProperty FindProp(string propertyName, MaterialProperty[] properties, bool propertyIsMandatory = false)
        {
	        if (properties == null)
	        {
				Debug.LogWarning("Get other properties form Drawer is only support Unity 2019.2+!");
		        return null;
	        }
	        else
				return FindProperty(propertyName, properties, propertyIsMandatory);
        }
    }
	
	public class GroupStateHelper
    {
		// Used to Folding Group, key: group name, value: is folding
		private static Dictionary<Object, Dictionary<string, bool>> _groups = new Dictionary<Object, Dictionary<string, bool>>();
		
		// Used to Conditional Display, key: keyword, value: is activated
		private static Dictionary<Object, Dictionary<string, bool>> _keywords = new Dictionary<Object, Dictionary<string, bool>>();

		private static void InitPoolPerMaterial(Object material)
		{
			if (!_groups.ContainsKey(material)) _groups.Add(material, new Dictionary<string, bool>());
			if (!_keywords.ContainsKey(material)) _keywords.Add(material, new Dictionary<string, bool>());
		}

		public static bool ContainsGroup(Object material, string group)
		{
			InitPoolPerMaterial(material);
			return _groups[material].ContainsKey(group);
		}
		
		public static void SetGroupFolding(Object material, string group, bool isFolding)
		{
			InitPoolPerMaterial(material);
			if (_groups[material].ContainsKey(group))
				_groups[material][group] = isFolding;
			else 
				_groups[material].Add(group, isFolding);
		}
		
		public static bool GetGroupFolding(Object material, string group)
		{
			InitPoolPerMaterial(material);
			Debug.Assert(_groups[material].ContainsKey(group), "Unknown Group: " + group);
			return _groups[material][group];
		}
		
		public static bool IsSubVisible(Object material, string group)
		{
			if (group == "" || group == "_") return true;
			InitPoolPerMaterial(material);
			
			// common sub
			if (_groups[material].ContainsKey(group))
			{
				return !_groups[material][group];
			}
			// existing suffix, may be based on the enum conditions sub
			else
			{
				foreach (var prefix in _groups[material].Keys)
				{
					// prefix = group name, suffix = keyWord
					if (group.Contains(prefix))
					{
						string suffix = group.Substring(prefix.Length, group.Length - prefix.Length).ToUpperInvariant();
						if (_keywords[material].ContainsKey(suffix))
						{
							// visible when keyword is activated and group is not folding 
							return _keywords[material][suffix] && !_groups[material][prefix];
						}
					}
				}
				return false;
			}
		}

		public static void SetKeywordConditionalDisplay(Object material, string keyword, bool isDisplay)
		{
			if (keyword == "" || keyword == "_") return;
			InitPoolPerMaterial(material);

			if (_keywords[material].ContainsKey(keyword))
				_keywords[material][keyword] = isDisplay;
			else
				_keywords[material].Add(keyword, isDisplay);
		}
    }

	
	/// <summary>
	/// Helpers for drawing Unreal Style Revertable Shader GUI 
	/// </summary>
    public class RevertableHelper
	{
		public static readonly float    revertButtonWidth = 15f;
		public static          float    fieldWidth;
		public static          float    labelWidth;
		public static          Material defaultMaterial;

		public static Rect GetRevertButtonRect(MaterialProperty prop, Rect rect, bool isCallInDrawer = false)
		{
			float defaultHeightWithoutDrawers;
			switch (prop.type)
			{
				case MaterialProperty.PropType.Vector:
					defaultHeightWithoutDrawers = 18f;
					break;
				default:
					defaultHeightWithoutDrawers = MaterialEditor.GetDefaultPropertyHeight(prop); 
					break;
			}
			if (isCallInDrawer) rect.xMax += revertButtonWidth;
			var revertButtonRect = new Rect(rect.xMax - revertButtonWidth + 2f, 
											rect.yMax - defaultHeightWithoutDrawers * 0.5f - (revertButtonWidth - 3f) * 0.5f - 1f,
											revertButtonWidth - 2f, 
											revertButtonWidth - 3f);
			return revertButtonRect;
		}

		public static void SetRevertableGUIWidths()
		{
			EditorGUIUtility.fieldWidth = RevertableHelper.fieldWidth;
			EditorGUIUtility.labelWidth = RevertableHelper.labelWidth;
		}
		
		public static void SetPropertyToDefault(MaterialProperty prop)
		{
			switch (prop.type)
			{
				case MaterialProperty.PropType.Color:
					prop.colorValue = defaultMaterial.GetColor(prop.name);
					break;
				case MaterialProperty.PropType.Vector:
					prop.vectorValue = defaultMaterial.GetVector(prop.name);
					break;
				case MaterialProperty.PropType.Float:
				case MaterialProperty.PropType.Range:
					prop.floatValue = defaultMaterial.GetFloat(prop.name);
					break;
				case MaterialProperty.PropType.Texture: 
					prop.textureValue = defaultMaterial.GetTexture(prop.name);
					break;
#if UNITY_2021_1_OR_NEWER
				case MaterialProperty.PropType.Int:
					prop.intValue = defaultMaterial.GetInt(prop.name);
					break;
#endif
			}
		}
		

		// ======================= Draw revert button =======================
		public static bool RevertButton(Rect position, MaterialProperty prop, MaterialEditor materialEditor)
		{
			bool doRevert = false;
			Rect rect = position;
			switch (prop.type)
			{
				case MaterialProperty.PropType.Color:
					var color = defaultMaterial.GetColor(prop.name);
					if (color == prop.colorValue && !prop.hasMixedValue)
						break;
					if (DoRevertButton(rect))
					{
						AddProperty(prop.targets, prop.name);
						prop.colorValue = color;
						doRevert = true;
					}
					break;

				case MaterialProperty.PropType.Vector:
					var vector = defaultMaterial.GetVector(prop.name);
					if (vector == prop.vectorValue && !prop.hasMixedValue)
						break;
					if (DoRevertButton(rect))
					{
						AddProperty(prop.targets, prop.name);
						prop.vectorValue = vector;
						doRevert = true;
					}
					break;

				case MaterialProperty.PropType.Range:
				case MaterialProperty.PropType.Float:
					var value = defaultMaterial.GetFloat(prop.name);
					if (value == prop.floatValue && !prop.hasMixedValue)
						break;
					if (DoRevertButton(rect))
					{
						AddProperty(prop.targets, prop.name);
						prop.floatValue = value;
						doRevert = true;
						// refresh keywords
						MaterialEditor.ApplyMaterialPropertyDrawers(materialEditor.targets);
					}
					break;

				case MaterialProperty.PropType.Texture:
					// var tex = defaultMaterial.GetTexture(prop.name);
					// if (tex == prop.textureValue && !prop.hasMixedValue)
					// 	break;
					// if (DoRevertButton(rect))
					// {
					// 	AddProperty(prop.targets, prop.name);
					// 	prop.textureValue = tex;
					// }
					break;
			}
			return doRevert;
		}

		private static readonly Texture _icon = AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath("e7bc1130858d984488bca32b8512ca96"));
		
		private static bool DoRevertButton(Rect rect)
		{
			if (_icon == null) Debug.LogError("RevertIcon.png + meta is missing!");
			GUI.DrawTexture(rect, _icon);
			var e = Event.current;
			if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				e.Use();
				return true;
			}
			return false;
		}

		// ========== Call drawers to do revert and refresh keywords ========== 
		private static Dictionary<Object, List<string>> _needRevertPropsPool;

		public static void AddProperty(Object[] materials, string propName)
		{
			if (_needRevertPropsPool == null)
				_needRevertPropsPool = new Dictionary<Object, List<string>>();
			foreach (var material in materials)
			{
				if (_needRevertPropsPool.ContainsKey(material))
				{
					if (!_needRevertPropsPool[material].Contains(propName))
						_needRevertPropsPool[material].Add(propName);
				}
				else
				{
					_needRevertPropsPool.Add(material, new List<string> { propName });
				}
			}
		}

		public static void RemoveProperty(Object[] materials, string propName)
		{
			if (_needRevertPropsPool == null) return;
			foreach (var material in materials)
			{
				if (_needRevertPropsPool.ContainsKey(material))
				{
					if (_needRevertPropsPool[material].Contains(propName))
						_needRevertPropsPool[material].Remove(propName);
				}
			}
		}

		public static bool ContainsProperty(Object material, string propName)
		{
			if (_needRevertPropsPool == null) return false;
			if (_needRevertPropsPool.ContainsKey(material))
			{
				return _needRevertPropsPool[material].Contains(propName);
			}
			else
			{
				return false;
			}
		}
	}

	/// <summary>
	/// Misc Function
	/// </summary>
    public class Helper
    {
#region Engine Misc

		public static void ObsoleteWarning(string obsoleteStr, string newStr)
		{
			Debug.LogWarning("'"+obsoleteStr+"' is Obsolete! Please use '"+newStr+"'!");
		}
		
		public static string GetKeyWord(string keyWord, string propName)
		{
			string k;
			if (keyWord == "" || keyWord == "__")
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
			if (string.IsNullOrEmpty(keyWord)) return;
			
			foreach (Material m in materials)
			{
				// delete "_" keywords
				if(keyWord == "_")
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
			Debug.Assert(keyWords.Length >= 1 && index < keyWords.Length && index >= 0, "KeyWords: "+keyWords+" or Index: "+index+" Error! ");
			for (int i = 0; i < keyWords.Length; i++)
			{
				SetShaderKeyWord(materials, keyWords[i], index == i);
			}
		}


		/// <summary>
		/// make Drawer can get all current Material props by customShaderGUI
		/// Unity 2019.2+
		/// </summary>
		public static LWGUI GetLWGUI(MaterialEditor editor)
		{
			if (editor.customShaderGUI != null && editor.customShaderGUI is LWGUI)
			{
				LWGUI gui = editor.customShaderGUI as LWGUI;
				return gui;
			}
			else
			{
				Debug.LogWarning("Please add \"CustomEditor \"LWGUI.LWGUI\"\" to the end of your shader!");
				return null;
			}
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

        public static bool Foldout(Rect position, ref bool isFolding, bool toggleValue, bool hasToggle, string title)
        {
			var style = new GUIStyle("ShurikenModuleTitle");
			style.border = new RectOffset(15, 7, 4, 4);
			style.fixedHeight = 30;
			// Text
			style.font = new GUIStyle(EditorStyles.boldLabel).font;
			style.fontSize = (int)(style.fontSize * 1.5f);
			style.contentOffset = new Vector2(30f, -2f);

			var rect = position;//GUILayoutUtility.GetRect(position.width, 24f, style);

			GUI.backgroundColor = isFolding ? Color.white : new Color(0.85f, 0.85f, 0.85f);
			GUI.Box(rect, title, style);
			GUI.backgroundColor = Color.white;

            var toggleRect = new Rect(rect.x + 8f, rect.y + 7f, 13f, 13f);

            if (hasToggle)
				toggleValue = GUI.Toggle(toggleRect, toggleValue, "", new GUIStyle(EditorGUI.showMixedValue ? "ToggleMixed" : "Toggle"));

			var e = Event.current;
            if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
            {
                isFolding = !isFolding;
                e.Use();
            }
            return toggleValue;
        }

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
            if (sliderRect.width >= 50f)
                value = GUI.Slider(sliderRect, value, 0.0f, start, end, GUI.skin.horizontalSlider, !EditorGUI.showMixedValue ? GUI.skin.horizontalSliderThumb : (GUIStyle)"SliderMixed", true, controlId);

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
					if (Event.current.type == EventType.MouseDown)
						Application.OpenURL("https://github.com/JasonMa0012/LWGUI");
				}
				GUI.DrawTexture(logoRect, _logo);
				GUI.color = c;
				GUI.Label(logoRect, new GUIContent("", "LWGUI (Light Weight Shader GUI)\n\n"
													 + "A Lightweight, Flexible, Powerful Unity Shader GUI system.\n\n"
													 + "Copyright (c) Jason Ma"));
			}
		}
		
		
#endregion
	}

	// Ramp

	public class RampHelper
	{
		
		[Serializable]
		public class GradientObject : ScriptableObject
		{
			[SerializeField] public Gradient gradient = new Gradient();
		}

		private static readonly GUIContent _iconAdd     = new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Plus").image, "Add"),
										   _iconEdit    = new GUIContent(EditorGUIUtility.IconContent("editicon.sml").image, "Edit"),
										   _iconDiscard = new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Refresh").image, "Discard"),
										   _iconSave    = new GUIContent(EditorGUIUtility.IconContent("SaveActive").image, "Save");

		private static readonly GUIStyle _styleEdit = new GUIStyle("button");

		public static bool RampEditor(
			MaterialProperty   prop,
			Rect               buttonRect,
			SerializedProperty gradientProperty,
			bool               isDirty,
			string             defaultFileName,
			int                defaultWidth,
			int                defaultHeight,
			out Texture        newTexture,
			out bool           doSave,
			out bool           doDiscard)
		{
			newTexture = null;
			var hasChange = false;
			var isNeedCreate = false;
			var singleButtonWidth = buttonRect.width * 0.25f;
			var editRect = new Rect(buttonRect.x + singleButtonWidth * 0, buttonRect.y, singleButtonWidth, buttonRect.height);
			var saveRect = new Rect(buttonRect.x + singleButtonWidth * 1, buttonRect.y, singleButtonWidth, buttonRect.height);
			var addRect = new Rect(buttonRect.x + singleButtonWidth * 2, buttonRect.y, singleButtonWidth, buttonRect.height);
			var discardRect = new Rect(buttonRect.x + singleButtonWidth * 3, buttonRect.y, singleButtonWidth, buttonRect.height);

			// if the current edited texture is null, create new one
			var currEvent = Event.current;
			if (prop.textureValue == null && currEvent.type == EventType.MouseDown && editRect.Contains(currEvent.mousePosition))
			{
				isNeedCreate = true;
				currEvent.Use();
			}
			
			// Gradient Editor
			var gradientPropertyRect = new Rect(editRect.x + 2, editRect.y + 2, editRect.width - 2, editRect.height - 2);
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(gradientPropertyRect, gradientProperty, new GUIContent(""));
			if (EditorGUI.EndChangeCheck()) hasChange = true;
			
			// Edit Icon override
			if (currEvent.type == EventType.Repaint)
			{
				var isHover = editRect.Contains(currEvent.mousePosition);
				_styleEdit.Draw(editRect, _iconEdit, isHover, false, false, false);
			}
				
			// Create Ramp Texture
            if (GUI.Button(addRect, _iconAdd) || isNeedCreate)
            {
                var path = EditorUtility.SaveFilePanel("Create New Ramp Texture", lastSavePath, defaultFileName, "png");
                if (path.Contains(projectPath))
                {
                    lastSavePath = Path.GetDirectoryName(path);

                    //Create texture and save PNG
                    var saveUnityPath = path.Replace(projectPath, "");
                    CreateAndSaveNewGradientTexture(defaultWidth, defaultHeight, saveUnityPath);

                    //Load created texture
					newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(saveUnityPath);
				}
				else if(!string.IsNullOrEmpty(path))
				{
					Debug.LogError("Invalid Path: "+path+"\n"
								 + "Please make sure you chosen Unity Project Relative Path");
				}
            }

			var color = GUI.color;
			if (isDirty) GUI.color = Color.yellow;
			doSave = GUI.Button(saveRect, _iconSave);
			GUI.color = color;
			
			doDiscard = GUI.Button(discardRect, _iconDiscard);
			
			return hasChange;
		}
		
		
		public static readonly string projectPath = Application.dataPath.Substring(0, Application.dataPath.Length - 6);
		public static string lastSavePath
		{
			get { return EditorPrefs.GetString("LWGUI_GradientSavePath_" + Application.version, Application.dataPath); }
			set
			{
				if (value.Contains(projectPath))
					EditorPrefs.SetString("LWGUI_GradientSavePath_" + Application.version, value);
			}
		}

		
		public static Gradient GetGradientFromTexture(Texture texture, out bool isDirty, bool doReimporte = false)
		{
			isDirty = false;
			if (texture == null) return null;
			
			var assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
			if (assetImporter != null && assetImporter.userData.Contains("LWGUI"))
			{
				GradientObject savedGradientObject, editingGradientObject;
				isDirty = DecodeGradientFromJSON(assetImporter.userData, out savedGradientObject, out editingGradientObject);
				return doReimporte ? savedGradientObject.gradient : editingGradientObject.gradient;
			}
			else
			{
				Debug.LogError("Can not find texture: "+texture.name+" or it's userData on disk! \n"
							 + "If you are moving or copying the Ramp Map, make sure your .meta file is not lost!");
				return null;
			}
		}

		public static void SetGradientToTexture(Texture texture, GradientObject gradientObject, bool doSaveToDisk = false)
		{
			if (texture == null || gradientObject.gradient == null) return;

			var texture2D = (Texture2D)texture;
			// Save to texture
			var path = AssetDatabase.GetAssetPath(texture);
			var pixels = GetPixelsFromGradient(gradientObject.gradient, texture.width, texture.height);
			texture2D.SetPixels(pixels);
			texture2D.Apply(true, false);
			
			// Save gradient JSON to userData
			var assetImporter = AssetImporter.GetAtPath(path);
			GradientObject savedGradientObject, editingGradientObject;
			DecodeGradientFromJSON(assetImporter.userData, out savedGradientObject, out editingGradientObject);
			assetImporter.userData = EncodeGradientToJSON(doSaveToDisk ? gradientObject : savedGradientObject, gradientObject);
				
			// Save texture to disk
			if (doSaveToDisk)
			{
				var systemPath = projectPath + path;
				File.WriteAllBytes(systemPath, texture2D.EncodeToPNG());
				assetImporter.SaveAndReimport();
			}
		}

		private static string EncodeGradientToJSON(GradientObject savedGradientObject, GradientObject editingGradientObject)
		{
			string savedJSON = " ", editingJSON = " ";
			if (savedGradientObject != null)
				savedJSON = EditorJsonUtility.ToJson(savedGradientObject);
			if (editingGradientObject != null)
				editingJSON = EditorJsonUtility.ToJson(editingGradientObject);

			return savedJSON + "#" + editingJSON;
		}

		private static bool DecodeGradientFromJSON(string json, out GradientObject savedGradientObject, out GradientObject editingGradientObject)
		{
			var subJSONs = json.Split('#');
			savedGradientObject = ScriptableObject.CreateInstance<GradientObject>();
			if (subJSONs[0] != " ")
				EditorJsonUtility.FromJsonOverwrite(subJSONs[0], savedGradientObject);
			editingGradientObject = ScriptableObject.CreateInstance<GradientObject>();
			if (subJSONs[1] != " ")
				EditorJsonUtility.FromJsonOverwrite(subJSONs[1], editingGradientObject);
			return subJSONs[0] != subJSONs[1];
		}
		

		public static bool CreateAndSaveNewGradientTexture(int width, int height, string unityPath)
		{
			var gradientObject = ScriptableObject.CreateInstance<GradientObject>();
			gradientObject.gradient = new Gradient();
			gradientObject.gradient.colorKeys = new[] { new GradientColorKey(Color.gray, 0.0f), new GradientColorKey(Color.white, 1.0f) };
			gradientObject.gradient.alphaKeys = new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) };

			var ramp = CreateGradientTexture(gradientObject.gradient, width, height);
			var png = ramp.EncodeToPNG();
			Object.DestroyImmediate(ramp);

			var systemPath = projectPath + unityPath;
			File.WriteAllBytes(systemPath, png);

			AssetDatabase.ImportAsset(unityPath);
			var textureImporter = AssetImporter.GetAtPath(unityPath) as TextureImporter;
			textureImporter.wrapMode = TextureWrapMode.Clamp;
			textureImporter.isReadable = true;
			textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
			textureImporter.alphaSource = TextureImporterAlphaSource.FromInput;
			
			//Gradient data embedded in userData
			textureImporter.userData = EncodeGradientToJSON(gradientObject, gradientObject);
			textureImporter.SaveAndReimport();

			return true;
		}
		
		private static Texture2D CreateGradientTexture(Gradient gradient, int width, int height)
		{
			var ramp = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
			var colors = GetPixelsFromGradient(gradient, width, height);
			ramp.SetPixels(colors);
			ramp.Apply(true);
			return ramp;
		}

		private static Color[] GetPixelsFromGradient(Gradient gradient, int width, int height)
		{
			var pixels = new Color[width * height];
			for (var x = 0; x < width; x++)
			{
				var delta = x / (float)width;
				if (delta < 0) delta = 0;
				if (delta > 1) delta = 1;
				var col = gradient.Evaluate(delta);
				for (int i = 0; i < height; i++)
				{
					pixels[x + i * width] = col;
				}
			}
			return pixels;
		}
	}



} //namespace LWGUI
