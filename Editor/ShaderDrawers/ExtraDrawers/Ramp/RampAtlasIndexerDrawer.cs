// Copyright (c) Jason Ma

using LWGUI.LwguiGradientEditor;
using LWGUI.Runtime.LwguiGradient;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Visually similar to Ramp(), but RampAtlasIndexer() must be used together with RampAtlas().
	/// The actual stored value is the index of the current Ramp in the Ramp Atlas SO, used for sampling the Ramp Atlas Texture in the Shader.
	///
	/// group: parent group name.
	/// rampAtlasPropName: RampAtlas() property name.
	/// defaultRampName: default ramp name. (Default: Ramp)
	/// colorSpace: default ramp color space. (sRGB/Linear) (Default: sRGB)
	/// viewChannelMask: editable channels. (Default: RGBA)
	/// timeRange: the abscissa display range (1/24/2400), is used to optimize the editing experience when the abscissa is time of day. (Default: 1)
	/// Target Property Type: Float
	/// </summary>
	[LwguiDrawerCategory("Ramp")]
	[LwguiDrawerParameterString("group", "")]
	[LwguiDrawerParameterString("rampAtlasPropName", "")]
	[LwguiDrawerParameterString("defaultRampName", "Ramp")]
	[LwguiDrawerParameterEnum("colorSpace", 0, "sRGB", "Linear")]
	[LwguiDrawerParameterEnum("viewChannelMask", 0, "RGBA", "RGB", "R", "G", "B", "A")]
	[LwguiDrawerParameterEnum("timeRange", 0, "1", "24", "2400")]
	public class RampAtlasIndexerDrawer : RampDrawer
	{
		public string rampAtlasPropName = string.Empty;
		public string defaultRampName = "Ramp";

		private MaterialProperty _rampAtlasProp;
		public MaterialProperty rampAtlasProp
		{
			get
			{
				if (_rampAtlasProp == null)
				{
					var prop = metaDatas?.GetProperty(rampAtlasPropName);
					if (prop != null && prop.GetPropertyType() == ShaderPropertyType.Texture)
						_rampAtlasProp = prop;
				}
				return _rampAtlasProp;
			}
			set => _rampAtlasProp = value;
		}

		private LwguiRampAtlas _rampAtlasSO;
		public LwguiRampAtlas rampAtlasSO
		{
			get => _rampAtlasSO ??= LwguiRampAtlas.LoadRampAtlasSO(rampAtlasProp?.textureValue);
			set => _rampAtlasSO = value;
		}

		private IRamp _currentRamp;
		private bool _rampAtlasSOHasMixedValue;
		private bool _isOutOfRange;

		private static readonly float _previewRowHeight = EditorGUIUtility.singleLineHeight;
		private static readonly float _previewRowSpacing = 2f;

		private int currentRowCount => rampAtlasSO?.RowCountPerRamp ?? 1;

		protected override float rampPreviewHeight => currentRowCount * _previewRowHeight + Mathf.Max(0, currentRowCount - 1) * _previewRowSpacing;
		
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName) : this(group, rampAtlasPropName, "Ramp") {}
		
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName) : this(group, rampAtlasPropName, defaultRampName, "sRGB") {}
		
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace) : this(group, rampAtlasPropName, defaultRampName, colorSpace, "RGBA") {}
		
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace, string viewChannelMask) : this(group, rampAtlasPropName, defaultRampName, colorSpace, viewChannelMask, 1) {}
			
		public RampAtlasIndexerDrawer(string group, string rampAtlasPropName, string defaultRampName, string colorSpace, string viewChannelMask, float timeRange)
		{
			this.group = group;
			this.rampAtlasPropName = rampAtlasPropName;
			this.defaultRampName = defaultRampName;
			this.colorSpace = colorSpace.ToLower() == "linear" ? ColorSpace.Linear : ColorSpace.Gamma;
			this.viewChannelMask = LwguiGradient.ChannelMask.None;
			{
				viewChannelMask = viewChannelMask.ToLower();
				for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
				{
					if (viewChannelMask.Contains(LwguiGradient.channelNames[c]))
						this.viewChannelMask |= LwguiGradient.ChannelIndexToMask(c);
				}
			}
			this.timeRange = LwguiGradient.GradientTimeRange.One;
			{
				if ((int)timeRange == (int)LwguiGradient.GradientTimeRange.TwentyFour)
					this.timeRange = LwguiGradient.GradientTimeRange.TwentyFour;
				else if ((int)timeRange == (int)LwguiGradient.GradientTimeRange.TwentyFourHundred)
					this.timeRange = LwguiGradient.GradientTimeRange.TwentyFourHundred;
			}
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) => propType is ShaderPropertyType.Float or ShaderPropertyType.Int;

		protected override void OnRampPropUpdate(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			if (doRegisterUndo)
				OnGradientEditorChange(null);
		}

		protected override void OnGradientEditorChange(LwguiGradient gradient)
		{
			rampAtlasSO?.UpdateTexturePixels();
		}

		protected override void OnEditRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			OnGradientEditorChange(null);
		}

		protected override void OnSaveRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			rampAtlasSO?.SaveTexture(checkoutAndForceWrite:true);
			rampAtlasSO?.SaveRampAtlasSO();
		}

		protected override LwguiGradient GetLwguiGradient(MaterialProperty prop, out bool isDirty)
		{
			isDirty = false;
			if (rampAtlasSO && (int)prop.GetNumericValue() < rampAtlasSO.RampCount)
			{
				_currentRamp = rampAtlasSO.GetRamp((int)prop.GetNumericValue());
				if (_currentRamp == null)
					return null;

				isDirty = EditorUtility.IsDirty(rampAtlasSO);
				colorSpace = _currentRamp.ColorSpace;
				viewChannelMask = _currentRamp.ChannelMask;
				timeRange = _currentRamp.TimeRange;
				return _currentRamp.Gradient;
			}
			else
				return null;
		}

		protected override void CloneRampMap(MaterialProperty prop, MaterialEditor editor, LwguiGradient gradient)
		{
			// Create or Clone a Ramp
			if (rampAtlasSO)
			{
				bool shouldCreateRamp = gradient == null || _currentRamp == null;
				var newIndex = rampAtlasSO.RampCount;
				var newRamp = rampAtlasSO.AddRamp(shouldCreateRamp ? null : _currentRamp);

				if (shouldCreateRamp)
				{
					newRamp.Name = defaultRampName;
					newRamp.ColorSpace = colorSpace;
					newRamp.ChannelMask = viewChannelMask;
					newRamp.TimeRange = timeRange;
				}

				prop.SetNumericValue(newIndex);

				if (rampAtlasSO.TotalRowCount > rampAtlasSO.rampAtlasHeight)
					rampAtlasSO.rampAtlasHeight *= 2;

				rampAtlasSO.UpdateTexturePixels();
			}
			// Create a Ramp Atlas SO
			else
			{
				var newRampAtlasSO = LwguiRampAtlas.CreateRampAtlasSO(rampAtlasProp, metaDatas);
				if (newRampAtlasSO)
				{
					rampAtlasSO = newRampAtlasSO;
					rampAtlasProp.textureValue = rampAtlasSO.rampAtlasTexture;
				}
			}
		}

		protected void SwitchRamp(MaterialProperty prop, int index)
		{
			prop.SetNumericValue(index);
			LWGUI.OnValidate(metaDatas);
		}

		protected override LwguiGradient DiscardRampMap(MaterialProperty prop, LwguiGradient gradient)
		{
			rampAtlasSO?.DiscardChanges();
			_currentRamp = rampAtlasSO?.GetRamp((int)prop.GetNumericValue());
			return _currentRamp?.Gradient;
		}

		// Selector button is drawn together with preview in DrawRampObjectField
		protected override void DrawRampSelector(Rect selectButtonRect, MaterialProperty prop, LwguiGradient gradient) { }

		/// <summary>
		/// Draw rampAtlasSO Fallback Field
		/// </summary>
		protected override void DrawRampObjectField(Rect rampFieldRect, MaterialProperty prop, LwguiGradient gradient)
		{
			if (!rampAtlasSO || _rampAtlasSOHasMixedValue)
			{
				EditorGUI.BeginChangeCheck();
				var newRampAtlasSO = EditorGUI.ObjectField(rampFieldRect, rampAtlasSO, typeof(LwguiRampAtlas), false) as LwguiRampAtlas;
				if (EditorGUI.EndChangeCheck())
				{
					rampAtlasSO = newRampAtlasSO;
					metaDatas.GetProperty(rampAtlasPropName).textureValue = rampAtlasSO?.rampAtlasTexture;
				}
			}
		}

		/// <summary>
		/// Draw Ramp Preview And Selector
		/// </summary>
		protected override void DrawPreviewTextureOverride(Rect previewRect, Rect fieldRect, MaterialProperty prop, LwguiGradient gradient)
		{
			if (!rampAtlasSO || _rampAtlasSOHasMixedValue)
				return;

			var previewWidth = previewRect.width;
			var selectorButtonWidth = EditorGUIUtility.singleLineHeight;

			if (_currentRamp == null || _isOutOfRange || EditorGUI.showMixedValue)
			{
				// Draw Invalid Ramp Fallback
				// Draw border
				GUI.Box(fieldRect, "None", EditorStyles.objectField);

				// Draw button
				var buttonRect = new Rect(previewRect.xMax - 1, fieldRect.y + 1, selectorButtonWidth - 0, rampPreviewHeight - 2);
				if (GUI.Button(buttonRect, GUIContent.none, GUIStyles.objectFieldButton))
				{
					RampSelectorWindow.ShowWindow(prop, rampAtlasSO, SwitchRamp);
				}
			}
			else
			{
				var gradients = _currentRamp.GetGradients();
				var gradientCount = gradients?.Length ?? 0;
				if (gradientCount == 0) return;

				// Draw gradient previews (clickable to edit)
				for (int i = 0; i < gradientCount; i++)
				{
					if (gradients[i] == null) continue;

					var rowY = fieldRect.y + i * (_previewRowHeight + _previewRowSpacing);
					var rowRect = new Rect(fieldRect.x, rowY, previewWidth, _previewRowHeight);

					// Draw border
					GUI.Box(rowRect, GUIContent.none, EditorStyles.objectField);

					// Draw gradient with alpha channel (clickable to edit)
					var innerRect = new Rect(rowRect.x + 1, rowRect.y + 1, rowRect.width - 2, rowRect.height - 2);
					LwguiGradientEditorHelper.GradientPreviewField(innerRect, gradients[i], colorSpace, viewChannelMask, timeRange, OnGradientEditorChange);
				}

				// Draw selector button (ObjectField picker style, stretches to cover all preview rows)
				{
					// Draw border
					var selectButtonRect = new Rect(previewRect.xMax - 2, fieldRect.y, selectorButtonWidth + 2, rampPreviewHeight);
					GUI.Box(selectButtonRect, GUIContent.none, EditorStyles.objectField);

					// Draw button
					var innerRect = new Rect(selectButtonRect.x + 0, selectButtonRect.y + 1, selectButtonRect.width - 1, selectButtonRect.height - 2);
					if (GUI.Button(innerRect, GUIContent.none, GUIStyles.objectFieldButton))
					{
						RampSelectorWindow.ShowWindow(prop, rampAtlasSO, SwitchRamp);
					}
				}
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			rampAtlasProp = metaDatas.GetProperty(rampAtlasPropName);
			if (rampAtlasProp == null || rampAtlasProp.GetPropertyType() != ShaderPropertyType.Texture)
			{
				Helper.DrawShaderPropertyWithErrorLabel(position, prop, label, editor, "Invalid rampAtlasPropName");
				Debug.LogError($"LWGUI: Property { prop.name } has invalid rampAtlasPropName: { rampAtlasPropName }");
				return;
			}

			// Add Info to label
			var currentIndex = (int)prop.GetNumericValue();
			label.tooltip += $"\nCurrent Value: {currentIndex}";
			if (rampAtlasSO)
			{
				_isOutOfRange = currentIndex >= rampAtlasSO.RampCount;
				var info = _isOutOfRange ? "OUT OF RANGE!" : rampAtlasSO.GetRamp(currentIndex)?.Name ?? "NULL";
				label.text += $" ({ currentIndex }: { info } - { rampAtlasSO.RampCount })";
			}
			else
			{
				_isOutOfRange = false;
				label.text += $" ({ currentIndex } - NULL)";
			}

			var labelWidth = EditorStyles.label.CalcSize(label).x;
			var indent = ReflectionHelper.EditorGUI_indent;
			if (labelWidth > EditorGUIUtility.labelWidth - indent)
				EditorGUIUtility.labelWidth = labelWidth + indent;

			// Handle Mixed Value
			_rampAtlasSOHasMixedValue = rampAtlasProp.hasMixedValue;
			var showMixedValue = EditorGUI.showMixedValue;
			EditorGUI.showMixedValue = prop.hasMixedValue || _rampAtlasSOHasMixedValue;
			
			base.DrawProp(position, prop, label, editor);
			
			// Clear
			_rampAtlasProp = null;
			_rampAtlasSO = null;
			_currentRamp = null;
			EditorGUI.showMixedValue = showMixedValue;
		}
	}
}
