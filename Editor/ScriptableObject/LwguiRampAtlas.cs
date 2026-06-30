// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LWGUI.LwguiGradientEditor;
using LWGUI.Runtime.LwguiGradient;
using UnityEngine;
using UnityEditor;


namespace LWGUI
{
	public interface IRamp
	{
		string                          Name        { get; set; }
		ColorSpace                      ColorSpace  { get; set; }
		LwguiGradient.ChannelMask       ChannelMask { get; set; }
		LwguiGradient.GradientTimeRange TimeRange   { get; set; }
		
		/// <summary>
		/// Ramp contains at least one Gradient.
		/// You can add more custom Gradients by overriding the virtual functions in LwguiRampAtlas.
		/// </summary>
		LwguiGradient                   Gradient    { get; set; }

		/// <summary>
		/// Get All Gradients.
		/// </summary>
		LwguiGradient[] GetGradients();
		
		/// <summary>
		/// Fill pixels for this Ramp into the output array.
		/// </summary>
		/// <param name="outputPixels">The output pixel array to fill</param>
		/// <param name="currentIndex">Current index in the output array, will be updated after filling</param>
		/// <param name="width">Width of each row in pixels</param>
		void GetPixelsForAtlas(ref Color[] outputPixels, ref int currentIndex, int width);

		/// <summary>
		/// Get preview textures for this Ramp in Ramp Selector Window.
		/// Returns multiple textures if the Ramp contains multiple gradients.
		/// </summary>
		/// <param name="width">Width of the preview texture</param>
		/// <returns>Array of preview textures</returns>
		Texture2D[] GetPreviewTexturesForRampSelector(int width);
		
		/// <summary>
		/// Copy properties from another Ramp.
		/// </summary>
		/// <param name="source">The source Ramp to copy from</param>
		void CopyFrom(IRamp source);
	}

	[Serializable]
	public class Ramp : IRamp
	{
		public string                          name        = "New Ramp";
		public ColorSpace                      colorSpace  = ColorSpace.Gamma;
		public LwguiGradient.ChannelMask       channelMask = LwguiGradient.ChannelMask.All;
		public LwguiGradient.GradientTimeRange timeRange   = LwguiGradient.GradientTimeRange.One;
		public LwguiGradient                   gradient    = LwguiGradient.white;

		// IRamp interface implementation using explicit properties that wrap the fields
		string                          IRamp.Name        { get => name;        set => name = value; }
		ColorSpace                      IRamp.ColorSpace  { get => colorSpace;  set => colorSpace = value; }
		LwguiGradient.ChannelMask       IRamp.ChannelMask { get => channelMask; set => channelMask = value; }
		LwguiGradient.GradientTimeRange IRamp.TimeRange   { get => timeRange;   set => timeRange = value; }
		LwguiGradient                   IRamp.Gradient    { get => gradient;    set => gradient = value; }

		/// <summary>
		/// The number of rows this Ramp type occupies in the Ramp Atlas Texture.
		/// This is a static value bound to the type. Derived classes can hide this with 'new static'.
		/// </summary>
		public static int RowCount => 1;

		public virtual LwguiGradient[] GetGradients() => new[] { gradient };
		
		public virtual void GetPixelsForAtlas(ref Color[] outputPixels, ref int currentIndex, int width)
		{
			var gradients = GetGradients();
			for (int i = 0; i < gradients.Length; i++)
			{
				gradients[i]?.GetPixels(ref outputPixels, ref currentIndex, width, 1, channelMask);
			}
		}

		public virtual Texture2D[] GetPreviewTexturesForRampSelector(int width)
		{
			var gradients = GetGradients();
			var textures = new Texture2D[gradients.Length];
			for (int i = 0; i < gradients.Length; i++)
			{
				textures[i] = gradients[i]?.GetPreviewRampTexture(width, 1, colorSpace, channelMask);
			}
			return textures;
		}

		public virtual void CopyFrom(IRamp source)
		{
			if (source == null) return;
			
			name = source.Name;
			colorSpace = source.ColorSpace;
			channelMask = source.ChannelMask;
			timeRange = source.TimeRange;
			gradient = new LwguiGradient(source.Gradient);

			var gradients = GetGradients();
			for (int i = 1; i < gradients.Length; i++)
			{
				if (gradients[i] == null)
					continue;
				
				gradients[i].DeepCopyFrom(source.Gradient);
			}
		}
	}

	[CreateAssetMenu(fileName = "LWGUI_RampAtlas.asset", menuName = "LWGUI/Ramp Atlas", order = 84)]
	public partial class LwguiRampAtlas : ScriptableObject
	{
		public const string RampAtlasSOExtensionName      = "asset";
		public const string RampAtlasTextureExtensionName = "tga";

		public int  rampAtlasWidth  = 256;
		public int  rampAtlasHeight = 4;
		public bool rampAtlasSRGB   = true;
		
		private string _rampAtlasSOPath      = string.Empty;
		private string _rampAtlasTexturePath = string.Empty;
		
		[SerializeField] private List<Ramp> _ramps = new List<Ramp>();
		
		[NonSerialized] public Texture2D rampAtlasTexture = null;

		#region Ramp Operate

		/// <summary>
		/// Access the list of Ramps as IRamp interface. This property uses covariance (IReadOnlyList),
		/// allowing derived classes to directly return their strongly-typed lists without type conversion.
		/// Override this property in derived classes to return a custom Ramp list.
		/// </summary>
		public virtual IReadOnlyList<IRamp> Ramps => _ramps ??= new List<Ramp>();

		public virtual int RowCountPerRamp => Ramp.RowCount;
		
		public int RampCount => Ramps.Count;

		public int TotalRowCount => RampCount * RowCountPerRamp;

		public virtual IRamp CreateRamp(IRamp srcRamp = null)
		{
			var newRamp = new Ramp();
			if (srcRamp != null)
				newRamp.CopyFrom(srcRamp);
			return newRamp;
		}

		public virtual IRamp AddRamp(IRamp srcRamp = null)
		{
			_ramps ??= new List<Ramp>();
			var newRamp = CreateRamp(srcRamp);
			_ramps.Add(newRamp as Ramp);
			return newRamp;
		}

		public virtual IRamp GetRamp(int index)
		{
			if (index < RampCount && index >= 0)
			{
				return Ramps[index];
			}
			return null;
		}

		public virtual void ClearRamps()
		{
			_ramps?.Clear();
		}

		public void CheckRampRowCount()
		{
			if (TotalRowCount > rampAtlasHeight)
				Debug.LogError($"LWGUI: Ramp Atlas does NOT have enough height ({rampAtlasHeight} < {TotalRowCount}):\n{_rampAtlasSOPath}");
		}

		#endregion

		public void InitData()
		{
			if (AssetDatabase.Contains(this))
			{
				_rampAtlasSOPath = AssetDatabase.GetAssetPath(this);
				_rampAtlasTexturePath = Path.ChangeExtension(_rampAtlasSOPath, RampAtlasTextureExtensionName);
			}
		}

		public bool LoadTexture()
		{
			if (!AssetDatabase.Contains(this))
				return false;

			// Try to load
			rampAtlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(_rampAtlasTexturePath);

			// Create
			if (!rampAtlasTexture
				|| rampAtlasTexture.width != rampAtlasWidth
				|| rampAtlasTexture.height != rampAtlasHeight
				|| rampAtlasTexture.isDataSRGB != rampAtlasSRGB
			   )
			{
				CreateRampAtlasTexture();
				rampAtlasTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(_rampAtlasTexturePath);
			}

			if (!rampAtlasTexture)
			{
				Debug.LogError($"LWGUI: Can NOT create a Ramp Atlas Texture at path: {_rampAtlasTexturePath}");
				return false;
			}

			CheckRampRowCount();

			return true;
		}

		public virtual Color[] GetPixels()
		{
			var pixels = Enumerable.Repeat(Color.white, rampAtlasWidth * rampAtlasHeight).ToArray();
			int currentIndex = 0;
			foreach (var ramp in Ramps)
			{
				ramp?.GetPixelsForAtlas(ref pixels, ref currentIndex, rampAtlasWidth);
			}

			return pixels;
		}

		public void CreateRampAtlasTexture()
		{
			var rampAtlas = new Texture2D(rampAtlasWidth, rampAtlasHeight, TextureFormat.RGBA32, false, !rampAtlasSRGB);
			rampAtlas.SetPixels(GetPixels());
			rampAtlas.wrapMode = TextureWrapMode.Clamp;
			rampAtlas.name = Path.GetFileName(_rampAtlasTexturePath);
			rampAtlas.Apply();

			SaveTexture(rampAtlas, checkoutAndForceWrite:true);

			AssetDatabase.ImportAsset(_rampAtlasTexturePath);
			RampHelper.SetRampTextureImporter(_rampAtlasTexturePath, true, !rampAtlasSRGB, EditorJsonUtility.ToJson(this));
		}

		public void SaveTexture(Texture2D rampAtlas = null, string targetRelativePath = null, bool checkoutAndForceWrite = false)
		{
			targetRelativePath ??= _rampAtlasTexturePath;
			rampAtlas ??= rampAtlasTexture;
			if (!rampAtlas || string.IsNullOrEmpty(targetRelativePath))
				return;
			
			CheckRampRowCount();

			var absPath = IOHelper.GetAbsPath(targetRelativePath);
			if (File.Exists(absPath))
			{
				var existRampTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(targetRelativePath);
				if (!VersionControlHelper.IsWriteable(existRampTexture))
				{
					if (checkoutAndForceWrite)
					{
						if (!VersionControlHelper.Checkout(targetRelativePath))
						{
							Debug.LogError($"LWGUI: Can NOT write the Ramp Atlas Texture to path: {absPath}");
							return;
						}
					}
					else
					{
						return;
					}
				}
			}

			try
			{
				File.WriteAllBytes(absPath, rampAtlas.EncodeToTGA());
				SaveTextureUserData(targetRelativePath);

				Debug.Log($"LWGUI: Saved the Ramp Atlas Texture at path: {absPath}");
			}
			catch (Exception e)
			{
				Debug.LogError(e);
			}
		}

		public void SaveTextureUserData(string targetRelativePath = null)
		{
			targetRelativePath ??= _rampAtlasTexturePath;
			if (!string.IsNullOrEmpty(targetRelativePath))
			{
				var importer = AssetImporter.GetAtPath(targetRelativePath);
				if (importer)
				{
					importer.userData = EditorJsonUtility.ToJson(this);
					importer.SaveAndReimport();
				}
			}
		}

		public void SaveRampAtlasSO()
		{
			AssetDatabase.SaveAssetIfDirty(this);
		}

		public void UpdateTexturePixels()
		{
			if (!rampAtlasTexture)
				return;

			LwguiGradientWindow.RegisterSerializedObjectUndo(this);
			rampAtlasTexture.Reinitialize(rampAtlasWidth, rampAtlasHeight);
			rampAtlasTexture.SetPixels(GetPixels());
			rampAtlasTexture.Apply();
		}

		public void DiscardChanges()
		{
			var importer = AssetImporter.GetAtPath(_rampAtlasTexturePath);
			if (!importer)
				return;

			EditorJsonUtility.FromJsonOverwrite(importer.userData, this);
			InitData();
			AssetDatabase.ImportAsset(_rampAtlasTexturePath, ImportAssetOptions.ForceUpdate);
			LoadTexture();
			EditorUtility.ClearDirty(this);
		}
		
		private void OnEnable()
		{
			InitData();
			LoadTexture();
		}

		/// <summary>
		/// Explicitly save texture with P4 checkout. Only call from user-initiated actions (e.g. Save button).
		/// </summary>
		public void SaveTextureWithCheckout()
		{
			InitData();

			if (!LoadTexture())
				return;

			UpdateTexturePixels();
			SaveTexture(checkoutAndForceWrite: true);
		}

		#region Static
		
		public static Texture LoadRampAtlasTexture(LwguiRampAtlas rampAtlasSO)
		{
			if (!rampAtlasSO || !AssetDatabase.Contains(rampAtlasSO))
			{
				return null;
			}

			var soPath = Path.ChangeExtension(AssetDatabase.GetAssetPath(rampAtlasSO), RampAtlasTextureExtensionName);
			return AssetDatabase.LoadAssetAtPath<Texture>(soPath);
		}

		public static LwguiRampAtlas LoadRampAtlasSO(Texture texture)
		{
			if (!texture || !AssetDatabase.Contains(texture))
			{
				return null;
			}

			var soPath = Path.ChangeExtension(AssetDatabase.GetAssetPath(texture), RampAtlasSOExtensionName);
			return AssetDatabase.LoadAssetAtPath<LwguiRampAtlas>(soPath);
		}

		public static LwguiRampAtlas CreateRampAtlasSO(MaterialProperty rampAtlasProp, LWGUIMetaDatas metaDatas, Type rampAtlasType = null)
		{
			if (rampAtlasProp == null || metaDatas == null)
				return null;

			var shader = metaDatas.GetShader();

			// Get default ramps
			RampAtlasDrawer targetRampAtlasDrawer = null;
			List<(int defaultIndex, RampAtlasIndexerDrawer indexerDrawer)> defaultRampAtlasIndexerDrawers = new();
			// Unity Bug: The cache of MaterialPropertyHandler must be cleared first, otherwise the default value cannot be obtained correctly.
			ReflectionHelper.InvalidatePropertyCache(shader);
			for (int i = 0; i < metaDatas.perMaterialData.defaultPropertiesWithPresetOverride.Length; i++)
			{
				var prop = metaDatas.perMaterialData.defaultPropertiesWithPresetOverride[i];
				var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop);
				if (drawer == null)
					continue;

				if (drawer is RampAtlasDrawer rampAtlasDrawer && prop.name == rampAtlasProp.name)
					targetRampAtlasDrawer = rampAtlasDrawer;

				if (drawer is RampAtlasIndexerDrawer rampAtlasIndexerDrawer && rampAtlasIndexerDrawer.rampAtlasPropName == rampAtlasProp.name)
					defaultRampAtlasIndexerDrawers.Add(((int)prop.GetNumericValue(), rampAtlasIndexerDrawer));
			}

			if (targetRampAtlasDrawer == null)
			{
				Debug.LogError($"LWGUI: Can NOT find RampAtlasDrawer {rampAtlasProp.name} in Shader {shader}");
				return null;
			}

			// Init Ramp Atlas with custom type or default type
			rampAtlasType ??= typeof(LwguiRampAtlas);
			var newRampAtlasSO = ScriptableObject.CreateInstance(rampAtlasType) as LwguiRampAtlas;
			if (newRampAtlasSO == null)
			{
				Debug.LogError($"LWGUI: Failed to create RampAtlas of type '{rampAtlasType.Name}'");
				return null;
			}
			newRampAtlasSO.name = targetRampAtlasDrawer.defaultFileName;
			newRampAtlasSO.rampAtlasWidth = targetRampAtlasDrawer.defaultAtlasWidth;
			newRampAtlasSO.rampAtlasHeight = targetRampAtlasDrawer.defaultAtlasHeight;
			newRampAtlasSO.rampAtlasSRGB = targetRampAtlasDrawer.defaultAtlasSRGB;

			if (defaultRampAtlasIndexerDrawers.Count > 0)
			{
				defaultRampAtlasIndexerDrawers.Sort(((x, y) => x.defaultIndex.CompareTo(y.defaultIndex)));

				// Set Ramps Count
				var maxIndex = defaultRampAtlasIndexerDrawers.Max((tuple => tuple.defaultIndex));
				for (int i = 0; i < maxIndex + 1; i++)
				{
					newRampAtlasSO.AddRamp();
					if (newRampAtlasSO.TotalRowCount > newRampAtlasSO.rampAtlasHeight)
						newRampAtlasSO.rampAtlasHeight *= 2;
				}

				// Set Ramps Default Value
				for (int i = 0; i < defaultRampAtlasIndexerDrawers.Count; i++)
				{
					var defaultRampAtlasIndexerDrawer = defaultRampAtlasIndexerDrawers[i];
					var ramp = newRampAtlasSO.GetRamp(defaultRampAtlasIndexerDrawer.defaultIndex);
					var drawer = defaultRampAtlasIndexerDrawer.indexerDrawer;
					ramp.Name = drawer.defaultRampName;
					ramp.ColorSpace = drawer.colorSpace;
					ramp.ChannelMask = drawer.viewChannelMask;
					ramp.TimeRange = drawer.timeRange;
				}
			}

			return SaveRampAtlasSOToAsset(newRampAtlasSO, targetRampAtlasDrawer.rootPath, targetRampAtlasDrawer.defaultFileName);
		}

		public static LwguiRampAtlas CloneRampAtlasSO(LwguiRampAtlas rampAtlasSO, Type targetType = null)
		{
			if (!rampAtlasSO)
				return null;

			var rootPath = Path.GetDirectoryName(rampAtlasSO._rampAtlasSOPath);
			var defaultFileName = Path.GetFileName(rampAtlasSO._rampAtlasSOPath);

			LwguiRampAtlas newRampAtlasSO;
			
			// If target type is specified and different from source type, use ConvertToType
			if (targetType != null && targetType != rampAtlasSO.GetType())
			{
				// Use ConvertToType for type conversion
				newRampAtlasSO = ConvertToType(rampAtlasSO, targetType, false);
				return newRampAtlasSO;
			}
			else
			{
				// Same type, use Instantiate for direct clone
				newRampAtlasSO = Instantiate(rampAtlasSO);
			}

			if (SaveRampAtlasSOToAsset(newRampAtlasSO, rootPath, defaultFileName))
			{
				newRampAtlasSO.InitData();
				newRampAtlasSO.LoadTexture();
				return newRampAtlasSO;
			}

			return null;
		}

		public static LwguiRampAtlas SaveRampAtlasSOToAsset(LwguiRampAtlas rampAtlasSO, string rootPath, string defaultFileName)
		{
			if (!rampAtlasSO)
				return null;

			// Save Ramp Atlas
			string createdFileRelativePath = string.Empty;
			while (true)
			{
				var absPath = IOHelper.SaveFilePanel("Create a Ramp Atlas SO", rootPath, defaultFileName, "asset");
				if (absPath.StartsWith(IOHelper.ProjectPath))
				{
					createdFileRelativePath = IOHelper.GetRelativePath(absPath);
					break;
				}
				else if (absPath != string.Empty)
				{
					var retry = EditorUtility.DisplayDialog("Invalid Path", $"Please select the subdirectory of '{IOHelper.ProjectPath}'", "Retry", "Cancel");
					if (!retry) break;
				}
				else
				{
					break;
				}
			}

			if (!string.IsNullOrEmpty(createdFileRelativePath))
			{
				AssetDatabase.CreateAsset(rampAtlasSO, createdFileRelativePath);
				rampAtlasSO.InitData();
				rampAtlasSO.LoadTexture();
				return rampAtlasSO;
			}

			return null;
		}
		#endregion

		#region Context Menu

		public void ConvertColorSpace(ColorSpace targetColorSpace)
		{
			foreach (var ramp in Ramps)
			{
				if (ramp.ColorSpace != targetColorSpace)
				{
					ramp.ColorSpace = targetColorSpace;
					foreach (var gradient in ramp.GetGradients())
					{
						gradient?.ConvertColorSpaceWithoutCopy(
							targetColorSpace != ColorSpace.Gamma
								? ColorSpace.Linear
								: ColorSpace.Gamma);
					}
				}
			}

			rampAtlasSRGB = targetColorSpace == ColorSpace.Gamma;
			RampHelper.SetRampTextureImporter(_rampAtlasTexturePath, true, !rampAtlasSRGB, EditorJsonUtility.ToJson(this));
			UpdateTexturePixels();
			SaveTexture(checkoutAndForceWrite:true);
		}

		[ContextMenu("Convert Gamma To Linear")]
		public void ConvertGammaToLinear()
		{
			ConvertColorSpace(ColorSpace.Linear);
		}

		[ContextMenu("Convert Linear To Gamma")]
		public void ConvertLinearToGamma()
		{
			ConvertColorSpace(ColorSpace.Gamma);
		}

		#endregion
		
		
		#region Conversion Utilities
		
		/// <summary>
		/// Convert an existing LwguiRampAtlas asset to a custom derived type.
		/// The new asset will be created at the same location with a suffix.
		/// For custom Ramp types with additional Gradients, the extra Gradients will be copied from the default Gradient.
		/// </summary>
		public static LwguiRampAtlas ConvertToType(LwguiRampAtlas source, Type targetType, bool saveToAsset = true, string suffix = "_Converted")
		{
			if (source == null)
			{
				Debug.LogError("LWGUI: Source RampAtlas is null");
				return null;
			}
			
			if (targetType == null || !typeof(LwguiRampAtlas).IsAssignableFrom(targetType))
			{
				Debug.LogError($"LWGUI: Target type must be derived from LwguiRampAtlas");
				return null;
			}
			
			// Create new instance of target type
			var newAtlas = ScriptableObject.CreateInstance(targetType) as LwguiRampAtlas;
			if (newAtlas == null)
			{
				Debug.LogError($"LWGUI: Failed to create instance of type '{targetType.Name}'");
				return null;
			}
			
			// Copy basic properties
			newAtlas.rampAtlasWidth = source.rampAtlasWidth;
			newAtlas.rampAtlasHeight = source.rampAtlasHeight;
			newAtlas.rampAtlasSRGB = source.rampAtlasSRGB;
			
			// Convert each Ramp using CopyFrom interface
			foreach (var sourceRamp in source.Ramps)
			{
				if (sourceRamp == null) continue;
				
				var newRamp = newAtlas.AddRamp();
				newRamp?.CopyFrom(sourceRamp);
			}
			
			// Adjust height if needed
			while (newAtlas.TotalRowCount > newAtlas.rampAtlasHeight)
			{
				newAtlas.rampAtlasHeight *= 2;
			}

			if (!saveToAsset)
				return newAtlas;
			
			// Save as new asset
			var sourcePath = AssetDatabase.GetAssetPath(source);
			if (string.IsNullOrEmpty(sourcePath))
			{
				Debug.LogWarning("LWGUI: Source RampAtlas is not a saved asset");
				return newAtlas;
			}
			
			var directory = Path.GetDirectoryName(sourcePath);
			var fileName = Path.GetFileNameWithoutExtension(sourcePath);
			var newPath = Path.Combine(directory, fileName + suffix + "." + RampAtlasSOExtensionName);
			newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);
			
			var result = SaveRampAtlasSOToAsset(newAtlas, directory, Path.GetFileNameWithoutExtension(newPath));
			
			if (result != null)
				Debug.Log($"LWGUI: Successfully converted RampAtlas to '{targetType.Name}' at: {newPath}");
			else			
				Debug.LogError($"LWGUI: Conversion of RampAtlas to '{targetType.Name}' failed at: {newPath}");
			
			return result;
		}
		
		#endregion
	}
}
