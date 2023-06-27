using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LWGUI
{
	public class RampHelper
	{
		#region RampEditor
		[Serializable]
		public class GradientObject : ScriptableObject
		{
			[SerializeField] public Gradient gradient = new Gradient();
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

		private static readonly GUIContent _iconAdd     = new GUIContent(EditorGUIUtility.IconContent("d_Toolbar Plus").image, "Add"),
										   _iconEdit    = new GUIContent(EditorGUIUtility.IconContent("editicon.sml").image, "Edit"),
										   _iconDiscard = new GUIContent(EditorGUIUtility.IconContent("d_TreeEditor.Refresh").image, "Discard"),
										   _iconSave    = new GUIContent(EditorGUIUtility.IconContent("SaveActive").image, "Save");

		private static readonly GUIStyle _styleEdit = new GUIStyle("button");

		public static bool RampEditor(
			Rect               buttonRect,
			MaterialProperty   prop,
			SerializedProperty serializedProperty,
			bool               isDirty,
			string             defaultFileName,
			string             rootPath,
			int                defaultWidth,
			int                defaultHeight,
			out Texture2D      newTexture,
			out bool           doSave,
			out bool           doDiscard)
		{
			newTexture = null;
			var hasChange = false;
			var shouldCreate = false;
			var singleButtonWidth = buttonRect.width * 0.25f;
			var editRect = new Rect(buttonRect.x + singleButtonWidth * 0, buttonRect.y, singleButtonWidth, buttonRect.height);
			var saveRect = new Rect(buttonRect.x + singleButtonWidth * 1, buttonRect.y, singleButtonWidth, buttonRect.height);
			var addRect = new Rect(buttonRect.x + singleButtonWidth * 2, buttonRect.y, singleButtonWidth, buttonRect.height);
			var discardRect = new Rect(buttonRect.x + singleButtonWidth * 3, buttonRect.y, singleButtonWidth, buttonRect.height);

			// Edit button event
			var currEvent = Event.current;
			if (currEvent.type == EventType.MouseDown && editRect.Contains(currEvent.mousePosition))
			{
				// if the current edited texture is null, create new one
				if (prop.textureValue == null)
				{
					shouldCreate = true;
					currEvent.Use();
				}
				else
				{
					// Undo.RecordObject(prop.textureValue, "Edit Gradient");
				}
			}

			// Gradient Editor
			var gradientPropertyRect = new Rect(editRect.x + 2, editRect.y + 2, editRect.width - 2, editRect.height - 2);
			EditorGUI.BeginChangeCheck();
			EditorGUI.PropertyField(gradientPropertyRect, serializedProperty, GUIContent.none);
			if (EditorGUI.EndChangeCheck())
			{
				hasChange = true;
			}

			// Edit button overlay
			if (currEvent.type == EventType.Repaint)
			{
				var isHover = editRect.Contains(currEvent.mousePosition);
				_styleEdit.Draw(editRect, _iconEdit, isHover, false, false, false);
			}
			
			// Create button
			if (GUI.Button(addRect, _iconAdd) || shouldCreate)
			{
				while (true)
				{
					var absPath = EditorUtility.SaveFilePanel("Create New Ramp Texture", rootPath, defaultFileName, "png");
					
					if (absPath.StartsWith(projectPath + rootPath))
					{
						//Create texture and save PNG
						var saveUnityPath = absPath.Replace(projectPath, String.Empty);
						CreateAndSaveNewGradientTexture(defaultWidth, defaultHeight, saveUnityPath);
						// VersionControlHelper.Add(saveUnityPath);
						//Load created texture
						newTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(saveUnityPath);
						break;
					}
					else if (absPath != String.Empty)
					{
						var retry = EditorUtility.DisplayDialog("Invalid Path", "Please select the subdirectory of '" + projectPath + rootPath + "'", "Retry", "Cancel");
						if (!retry) break;
					}
					else
					{
						break;
					}
				}
			}

			// Save button
			var color = GUI.color;
			if (isDirty) GUI.color = Color.yellow;
			doSave = GUI.Button(saveRect, _iconSave);
			GUI.color = color;

			// Discard button
			doDiscard = GUI.Button(discardRect, _iconDiscard);

			return hasChange;
		}

		public static bool HasGradient(AssetImporter assetImporter) { return assetImporter.userData.Contains("LWGUI");}
		
		public static Gradient GetGradientFromTexture(Texture texture, out bool isDirty, bool doReimport = false)
		{
			isDirty = false;
			if (texture == null) return null;

			var assetImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));
			if (assetImporter != null && HasGradient(assetImporter))
			{
				GradientObject savedGradientObject, editingGradientObject;
				isDirty = DecodeGradientFromJSON(assetImporter.userData, out savedGradientObject, out editingGradientObject);
				return doReimport ? savedGradientObject.gradient : editingGradientObject.gradient;
			}
			else
			{
				Debug.LogError("Can not find texture: "
							 + texture.name
							 + " or it's userData on disk! \n"
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
			texture2D.SetPixels32(pixels);
			texture2D.Apply();

			// Save gradient JSON to userData
			var assetImporter = AssetImporter.GetAtPath(path);
			GradientObject savedGradientObject, editingGradientObject;
			DecodeGradientFromJSON(assetImporter.userData, out savedGradientObject, out editingGradientObject);
			assetImporter.userData = EncodeGradientToJSON(doSaveToDisk ? gradientObject : savedGradientObject, gradientObject);

			// Save texture to disk
			if (doSaveToDisk)
			{
				var systemPath = projectPath + path;
				VersionControlHelper.Checkout(path);
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
			gradientObject.gradient.colorKeys = new[] { new GradientColorKey(Color.black, 0.0f), new GradientColorKey(Color.white, 1.0f) };
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
			textureImporter.mipmapEnabled = false;
			
			var platformTextureSettings = textureImporter.GetDefaultPlatformTextureSettings();
			platformTextureSettings.format = TextureImporterFormat.ARGB32;
			platformTextureSettings.textureCompression = TextureImporterCompression.Uncompressed;
			textureImporter.SetPlatformTextureSettings(platformTextureSettings);

			//Gradient data embedded in userData
			textureImporter.userData = EncodeGradientToJSON(gradientObject, gradientObject);
			textureImporter.SaveAndReimport();

			return true;
		}

		private static Texture2D CreateGradientTexture(Gradient gradient, int width, int height)
		{
			var ramp = new Texture2D(width, height, TextureFormat.RGBA32, true, true);
			var colors = GetPixelsFromGradient(gradient, width, height);
			ramp.SetPixels32(colors);
			ramp.Apply();
			return ramp;
		}

		private static Color32[] GetPixelsFromGradient(Gradient gradient, int width, int height)
		{
			var pixels = new Color32[width * height];
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
		#endregion


		#region RampSelector
		public delegate void SwitchRampMapEvent(Texture2D selectedRamp);

		public static void RampSelector(Rect rect, string rootPath, SwitchRampMapEvent switchRampMapEvent)
		{
			var e = Event.current;
			if (e.type == UnityEngine.EventType.MouseDown && rect.Contains(e.mousePosition))
			{
				e.Use();
				var textureGUIDs = AssetDatabase.FindAssets("t:Texture2D", new[] { rootPath });
				var rampMaps = textureGUIDs.Select((GUID) =>
				{
					var path = AssetDatabase.GUIDToAssetPath(GUID);
					var assetImporter = AssetImporter.GetAtPath(path);
					if (HasGradient(assetImporter))
					{
						return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
					}
					else
						return null;
				}).ToArray();
				RampSelectorWindow.ShowWindow(rect, rampMaps, switchRampMapEvent);
			}
		}
		#endregion
	}

	public class RampSelectorWindow : EditorWindow
	{
		private Texture2D[]                   _rampMaps;
		private Vector2                       _scrollPosition;
		private RampHelper.SwitchRampMapEvent _switchRampMapEvent;

		public static void ShowWindow(Rect rect, Texture2D[] rampMaps, RampHelper.SwitchRampMapEvent switchRampMapEvent)
		{
			RampSelectorWindow window = ScriptableObject.CreateInstance<RampSelectorWindow>();
			window.titleContent = new GUIContent("Ramp Selector");
			window.minSize = new Vector2(400, 500);
			window._rampMaps = rampMaps;
			window._switchRampMapEvent = switchRampMapEvent;
			window.ShowAuxWindow();
		}
		
		private void OnGUI()
		{
			EditorGUILayout.BeginVertical();
			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			foreach (Texture2D rampMap in _rampMaps)
			{
				EditorGUILayout.BeginHorizontal();
				if (rampMap != null)
				{
					var guiContent = new GUIContent(rampMap.name);
					var rect = EditorGUILayout.GetControlRect();
					var buttonWidth = Mathf.Min(300f, Mathf.Max(GUI.skin.button.CalcSize(guiContent).x, rect.width * 0.35f));
					var buttonRect = new Rect(rect.x + rect.width - buttonWidth, rect.y, buttonWidth, rect.height);
					var previewRect = new Rect(rect.x, rect.y, rect.width - buttonWidth - 3.0f, rect.height);
					if (GUI.Button(buttonRect, guiContent) && _switchRampMapEvent != null)
					{
						_switchRampMapEvent(rampMap);
						Close();
					}
					EditorGUI.DrawPreviewTexture(previewRect, rampMap);
				}
				EditorGUILayout.EndHorizontal();
			}
			
			EditorGUILayout.EndScrollView();
			EditorGUILayout.EndVertical();
		}
	}
}