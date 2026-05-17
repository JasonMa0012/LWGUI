﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using LWGUI.Runtime.LwguiGradient;

namespace LWGUI.LwguiGradientEditor
{
    public class LwguiGradientEditor
    {
        private class CurveSelectionInfo
        {
            public List<AnimationCurve> selectedAnimationCurves;
            public Vector4 selectedVectorValue = Vector4.negativeInfinity;
            public float selectedFloatValue = float.NegativeInfinity;
            public float selectedTime = float.NegativeInfinity;
            public float selectedKeysAverageTime = 0;
            public uint selectedChannel = 0;
            public bool isOnlyColorKeySelected;
            public LwguiMergedColorCurves mergedCurves;
            
            public bool hasMixedTime;
            public bool hasMixedFloatValue; // Selected multiple keys with different values in a same Curve
            public bool hasMixedColorValue; // Selected multiple color keys with different values in the RGB Curves
            public List<bool> hasMixedChannelValue = Enumerable.Repeat(false, (int)LwguiGradient.Channel.Num).ToList();

            public CurveSelectionInfo(CurveEditor curveEditor)
            {
                selectedAnimationCurves = new List<AnimationCurve>();
                for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
                    selectedAnimationCurves.Add(new AnimationCurve());
                
                if (curveEditor?.selectedCurves is { Count: > 0 })
                {
	                int selectedKeyCount = 0;
                    foreach (var curveSelection in curveEditor.selectedCurves.Where(selection => selection != null))
                    {
                        var channelID = curveSelection.curveID;
                        var key = curveEditor.GetKeyframeFromSelection(curveSelection);
                        selectedAnimationCurves[channelID].AddKey(key);
                        selectedKeysAverageTime += key.time;
                        selectedKeyCount++;
                        selectedChannel |= 1u << channelID;

                        if (selectedTime != float.NegativeInfinity && selectedTime != key.time)
                            hasMixedTime = true;
                        selectedTime = key.time;
                        
                        if (selectedVectorValue[channelID] != Vector4.negativeInfinity[channelID] && selectedVectorValue[channelID] != key.value)
                            hasMixedChannelValue[channelID] = true;
                        selectedVectorValue[channelID] = key.value;

                        if (selectedFloatValue != float.NegativeInfinity && selectedFloatValue != key.value)
                            hasMixedFloatValue = true;
                        selectedFloatValue = key.value;
                    }

                    selectedKeysAverageTime /= selectedKeyCount;

                    for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
                    {
                        if (selectedVectorValue[c] == Vector4.negativeInfinity[c])
                            selectedVectorValue[c] = 0;
                    }

                    if (selectedFloatValue == float.NegativeInfinity)
                        selectedFloatValue = 0;

                    mergedCurves = new LwguiMergedColorCurves(selectedAnimationCurves);

                    var noAlphaKeySelected = mergedCurves.curves[(int)LwguiGradient.Channel.Alpha].Count == 0;
                    hasMixedColorValue      = noAlphaKeySelected && hasMixedChannelValue.   Where((_, c) => c < (int)LwguiGradient.Channel.Alpha).Any(b => b);
                    isOnlyColorKeySelected  = noAlphaKeySelected && selectedAnimationCurves.Where((_, c) => c < (int)LwguiGradient.Channel.Alpha).All(curve => curve.length == mergedCurves.curves[(int)LwguiGradient.Channel.Red].Count);
                }
                else
                {
                    mergedCurves = new LwguiMergedColorCurves();
                }
            }

            public bool HasSelectedChannel(int channelIndex)
            {
	            return (selectedChannel & 1u << channelIndex) != 0;
            }
        }
        
        #region UI Layout

        private bool _useSignalLineEditField    => _position.width > 1150;
        
        private float _gradientEditorHeisht     => _useSignalLineEditField ? 160 : 160 + _editFieldHeight;
        private float _margin                   => 8;
        private float _editFieldHeight          => EditorGUIUtility.singleLineHeight; // 18
        private float _editFieldMarginsHeight   => _margin * (_useSignalLineEditField ? 2 : 3);
        private float _gradientAndSwatchHeight  => _gradientEditorHeisht - _margin - _editFieldMarginsHeight - _editFieldHeight - _secondEditFieldRect.height;
        private float _swatchHeisht             => _gradientAndSwatchHeight * 0.15f;
        private float _alphaGradientHeisht      => _rgbGradientHeisht * 0.5f;
        private float _rgbGradientHeisht        => (_gradientAndSwatchHeight - _swatchHeisht * 2) * 0.66666f;
        
        private Rect _gradientEditorRect        => new Rect(0, 0, _position.width, _gradientEditorHeisht);
        private Rect _editFieldRect             => new Rect(_curveEditor.leftmargin, _margin, _position.width - _curveEditor.leftmargin - _curveEditor.rightmargin, _editFieldHeight);
        private Rect _secondEditFieldRect       => _useSignalLineEditField ? Rect.zero : new Rect(_curveEditor.leftmargin, _margin * 2 + _editFieldHeight, _position.width - _curveEditor.leftmargin - _curveEditor.rightmargin, _editFieldHeight);
        private Rect _alphaSwatchesRect         => new Rect(_curveEditor.leftmargin, _editFieldMarginsHeight + _editFieldHeight + _secondEditFieldRect.height, _position.width - _curveEditor.leftmargin - _curveEditor.rightmargin, _swatchHeisht);
        private Rect _rgbSwatchesRect           => new Rect(_curveEditor.leftmargin, _editFieldMarginsHeight + _editFieldHeight + _secondEditFieldRect.height + _swatchHeisht + _alphaGradientHeisht + _rgbGradientHeisht, _position.width - _curveEditor.leftmargin - _curveEditor.rightmargin, _swatchHeisht);
        private Rect _alphaGradientRect         => new Rect(_curveEditor.leftmargin, _editFieldMarginsHeight + _editFieldHeight + _secondEditFieldRect.height + _swatchHeisht, _position.width - _curveEditor.leftmargin - _curveEditor.rightmargin, _alphaGradientHeisht);
        private Rect _rgbGradientRect           => new Rect(_curveEditor.leftmargin, _editFieldMarginsHeight + _editFieldHeight + _secondEditFieldRect.height + _swatchHeisht + _alphaGradientHeisht, _position.width - _curveEditor.leftmargin - _curveEditor.rightmargin, _rgbGradientHeisht);
        
        private Rect _curveEditorRect           => new Rect(0, _gradientEditorHeisht, _position.width, _position.height - _gradientEditorHeisht);

        
        private static readonly GUIContent[] s_XYZWLabels = {EditorGUIUtility.TextContent("R"), EditorGUIUtility.TextContent("G"), EditorGUIUtility.TextContent("B"), EditorGUIUtility.TextContent("A")};


        private static GUIStyle _style_PopupCurveEditorBackground;
        public static GUIStyle style_PopupCurveEditorBackground => _style_PopupCurveEditorBackground ??= "PopupCurveEditorBackground";
        
        #endregion

        #region Math

        private const float TOLERANCE = 0.00001f;

        private static bool Equal(float a, float b) => Math.Abs(a - b) < TOLERANCE;

        private static bool Equal(GradientEditor.Swatch a, GradientEditor.Swatch b) => 
            a == b 
            || (a != null && b != null && a.m_Time == b.m_Time && a.m_Value == b.m_Value && a.m_IsAlpha == b.m_IsAlpha);

        #endregion

        #region Fields

        #region Inputs

        private Rect _position;
        internal LwguiGradient lwguiGradient;
        internal ColorSpace colorSpace;
        internal LwguiGradient.ChannelMask viewChannelMask;
        internal LwguiGradient.GradientTimeRange gradientTimeRange;
        private LwguiGradientWindow.ChangeGradientCallback _onChange;
        
        #endregion

        private GradientEditor _gradientEditor;
        private CurveEditor _curveEditor;
        private bool _viewSettingschanged;
        private bool _curveEditorContextMenuChanged;
        private bool _changed;
        private bool _lastChanged;

        #region Gradient Editor

        private static readonly string[] timeRangeMenuNames = new string[] { "0-1", "0-24", "0-2400" };
        private static readonly List<LwguiGradient.GradientTimeRange> timeRangeMenuValues = new () { LwguiGradient.GradientTimeRange.One, LwguiGradient.GradientTimeRange.TwentyFour, LwguiGradient.GradientTimeRange.TwentyFourHundred };

        private GradientEditor.Swatch _selectedGradientKey
        {
            get => _gradientEditor.GetSelectedSwatch();
            set => _gradientEditor.SetSelectedSwatch(value);
        }

        private GradientEditor.Swatch _lastSelectedGradientKey;
        private GradientEditor.Swatch _deletedGradientKey;

        private List<GradientEditor.Swatch> _gradientRGBSwatches => _gradientEditor.GetRGBdSwatches();
        private List<GradientEditor.Swatch> _gradientAlphaSwatches => _gradientEditor.GetAlphaSwatches();
        private int _lastGradientRGBSwatchesCount;
        private int _lastGradientAlphaSwatchesCount;
        
        private float _lastEditingTime = float.NegativeInfinity;
        
        private static bool _isAddGradientKeyFailure;

        #endregion
        

        #region Curve Editor

        private List<CurveSelection> _selectedCurves 
        {
            get => _curveEditor.selectedCurves;
            set => _curveEditor.selectedCurves = value;
        }

        private List<Keyframe> _deletedCurveKeys;

        private bool _shouldSyncSelectionFromCurveToGradient;
        
        #endregion

        #endregion

        #region Gradient Editor

        private void InitGradientEditor(bool force = false)
        {
            if (_gradientEditor != null && !force) return;

            var lastSelectedGradientKey = _gradientEditor != null && _selectedGradientKey != null ? new GradientEditor.Swatch(_selectedGradientKey.m_Time, _selectedGradientKey.m_Value, _selectedGradientKey.m_IsAlpha) : null;

            var lwguiMergedCurves = new LwguiMergedColorCurves(lwguiGradient.rawCurves);
            _gradientEditor ??= new GradientEditor();
            _gradientEditor.Init(lwguiMergedCurves.ToGradient(ReflectionHelper.maxGradientKeyCount), 1024, colorSpace == ColorSpace.Linear, colorSpace);
            
            // When Curve has only one key, Gradient Editor will automatically add a key
            {
                void FixAutoAddedGradientKey(List<GradientEditor.Swatch> swatches, LwguiGradient.Channel channel)
                {
                    if (swatches.Count == 2 && lwguiMergedCurves.curves[(int)channel].Count == 1)
                    {
                        swatches.Clear();
                        var curveKey = lwguiMergedCurves.curves[(int)channel][0];
                        swatches.Add(channel == LwguiGradient.Channel.Alpha
                        ? new GradientEditor.Swatch(curveKey.time, new Color(curveKey.value, curveKey.value, curveKey.value, curveKey.value), true)
                        : new GradientEditor.Swatch(curveKey.time, new Color(
                            lwguiMergedCurves.curves[(int)LwguiGradient.Channel.Red][0].value, 
                            lwguiMergedCurves.curves[(int)LwguiGradient.Channel.Green][0].value, 
                            lwguiMergedCurves.curves[(int)LwguiGradient.Channel.Blue][0].value,
                            1), false));
                    }
                }

                FixAutoAddedGradientKey(_gradientRGBSwatches, LwguiGradient.Channel.Red);
                FixAutoAddedGradientKey(_gradientAlphaSwatches, LwguiGradient.Channel.Alpha);
            }
            
            // Keep selected key
            if (lastSelectedGradientKey != null)
            {
                _selectedGradientKey = (lastSelectedGradientKey.m_IsAlpha ? _gradientAlphaSwatches : _gradientRGBSwatches)
                    .Find(swatch => Equal(swatch.m_Time, lastSelectedGradientKey.m_Time));
            }
            else
            {
                _selectedGradientKey = null;
            }
        }

        private void OnGradientEditorGUI()
        {
            OnGradientEditFieldGUI();
            
            EditorGUI.DrawPreviewTexture(_alphaGradientRect, lwguiGradient.GetPreviewRampTexture(1024, 1, colorSpace, LwguiGradient.ChannelMask.Alpha & viewChannelMask));
            EditorGUI.DrawPreviewTexture(_rgbGradientRect, lwguiGradient.GetPreviewRampTexture(1024, 1, colorSpace, LwguiGradient.ChannelMask.RGB & viewChannelMask));

            // Swatch Array
            {
                PrepareSyncSelectionFromGradientToCurve();
                EditorGUI.BeginChangeCheck();
                ShowGradientSwatchArray(_alphaSwatchesRect, _gradientAlphaSwatches, LwguiGradient.ChannelMask.Alpha);
                ShowGradientSwatchArray(_rgbSwatchesRect, _gradientRGBSwatches, LwguiGradient.ChannelMask.RGB);
                if (EditorGUI.EndChangeCheck())
                {
                    _changed = true;
                    EditorGUI.EndEditingActiveTextField();
                    ApplyGradientChangesToCurve();
                }

                SyncSelectionFromGradientToCurveWithoutChanges();
            }
        }

        private void OnGradientEditFieldGUI()
        {
            // Prevent interrupting mouse drag event
            if (Event.current.type is EventType.MouseDrag or EventType.MouseDown or EventType.MouseMove or EventType.MouseUp
                && (!_editFieldRect.Contains(Event.current.mousePosition)
                    && !_secondEditFieldRect.Contains(Event.current.mousePosition)))
                return;
            
            float addKeyButtonWidth         = 62;
            float space                     = 20;
            float vectorValueWidth          = 270;
            float locationWidth             = 70;
            float locationTextWidth         = 35;
            float alphaOrColorTextWidth     = 40;
            float colorSpaceTextWidth       = 87;
            float colorSpaceToggleWidth     = 16;
            float channelsTextWidth         = 60;
            float channelsMenuWidth         = 85;
            float timeRangeTextWidth        = 75;
            float timeRangeMenuWidth        = 70;
            
            Rect rect = _editFieldRect;
            
            // Add Key button (left side)
            {
                var btnRect = new Rect(_editFieldRect.x, _editFieldRect.y, addKeyButtonWidth, _editFieldRect.height);
                if (GUI.Button(btnRect, "Add Key"))
                {
                    PopupWindow.Show(btnRect, new AddKeyPopupContent(this, gradientTimeRange));
                }
            }
            
            // Color Space
            {
                rect.x = rect.xMax - colorSpaceToggleWidth - colorSpaceTextWidth;
                rect.width = colorSpaceTextWidth + colorSpaceToggleWidth;
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = colorSpaceTextWidth;
                EditorGUI.BeginChangeCheck();
                colorSpace = EditorGUI.Toggle(rect, "sRGB Preview", colorSpace == ColorSpace.Gamma) ? ColorSpace.Gamma : ColorSpace.Linear;
                if (EditorGUI.EndChangeCheck())
                {
                    _viewSettingschanged = true;
                    InitGradientEditor(true);
                }
                EditorGUIUtility.labelWidth = labelWidth;
            }

            // View Channel Mask
            {
                rect.x -= space + channelsMenuWidth + channelsTextWidth;
                rect.width = channelsMenuWidth + channelsTextWidth;
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = channelsTextWidth;
                EditorGUI.BeginChangeCheck();
                viewChannelMask = (LwguiGradient.ChannelMask)EditorGUI.EnumFlagsField(rect, "Channels", viewChannelMask);
                if (EditorGUI.EndChangeCheck())
                {
                    // TODO: Close the pop-up window immediately after modification to avoid inconsistent with the displayed value and the actual value.
                    // The reason is unknown, the old version does not have this problem.
                    ReflectionHelper.PopupWindowWithoutFocus_Hide();
                    
                    _viewSettingschanged = true;
                    InitGradientEditor(true);
                    InitCurveEditor(true);
                }
                EditorGUIUtility.labelWidth = labelWidth;
            }

            // Gradient Time Range
            {
                rect.x -= space + timeRangeMenuWidth + timeRangeTextWidth;
                rect.width = timeRangeMenuWidth + timeRangeTextWidth;
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = timeRangeTextWidth;
                EditorGUI.BeginChangeCheck();
                
                gradientTimeRange = timeRangeMenuValues[EditorGUI.Popup(rect, "Time Range", timeRangeMenuValues.IndexOf(gradientTimeRange), timeRangeMenuNames)];
                if (EditorGUI.EndChangeCheck())
                {
                    _viewSettingschanged = true;
                    InitGradientEditor(true);
                    InitCurveEditor(true);
                }
                EditorGUIUtility.labelWidth = labelWidth;
            }

            // Key edit field (GradientEditor.OnGUI())
            if (_selectedCurves is { Count: > 0 })
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                var showMixedValue = EditorGUI.showMixedValue;
                var selectionInfo = new CurveSelectionInfo(_curveEditor);
                
                // Time
                {
                    if (_useSignalLineEditField)
                    {
                        rect.x -= space + locationTextWidth + locationWidth;
                    }
                    else
                    {
                        rect = _secondEditFieldRect;
                        rect.x = rect.xMax - (locationTextWidth + locationWidth);
                    }
                    rect.width = locationTextWidth + locationWidth;
                    EditorGUIUtility.labelWidth = locationTextWidth;
                    EditorGUI.showMixedValue = selectionInfo.hasMixedTime;
                    
                    EditorGUI.BeginChangeCheck();
                    var newTime = EditorGUI.FloatField(rect, "Time", selectionInfo.selectedTime * (int)gradientTimeRange) / (int)gradientTimeRange;
                    // When two keys have the same time, they will be merged, so avoid modifying the time in real time and only apply the changes at the end of the change
                    var hasChange = EditorGUI.EndChangeCheck();
                    
                    if (hasChange)
	                    _lastEditingTime = newTime;
                    
                    if (_lastEditingTime != selectionInfo.selectedTime
                        && _lastEditingTime != float.NegativeInfinity
                        // End editing text
                        && (EditorGUI.IsEditingTextField() && Event.current.keyCode is KeyCode.Return or KeyCode.KeypadEnter
                            // Mouse drag
                            || !EditorGUI.IsEditingTextField() && hasChange))
                    {
                        var clampedNewTime = Mathf.Clamp01(_lastEditingTime);
                        var offsetedNewKeyTime = clampedNewTime;
                        if (HandleNewKeyTimeConflicts(ref offsetedNewKeyTime, clampedNewTime, selectionInfo))
                        {
	                        _changed = true;
			                _lastEditingTime = offsetedNewKeyTime;
							_curveEditor.SetSelectedKeyPositions(offsetedNewKeyTime, 0, true, false);
	                        InitGradientEditor(true);
	                        SyncSelectionFromCurveToGradient(true);
                        }
                    }
                }
                
                // Vector Value
                {
                    rect.x -= space + vectorValueWidth;
                    rect.width = vectorValueWidth;
                    // EditorGUI.VectorField()
                    {
                        int channelCount = (int)LwguiGradient.Channel.Num;
                        float w = (rect.width - (channelCount - 1) * EditorGUI.kSpacingSubLabel) / channelCount;
                        Rect nr = new Rect(rect) {width = w};
                        int l = EditorGUI.indentLevel;
                        EditorGUI.indentLevel = 0;
                        for (int c = 0; c < channelCount; c++)
                        {
                            EditorGUIUtility.labelWidth = EditorGUI.GetLabelWidth(s_XYZWLabels[c], 0);
                            EditorGUI.showMixedValue = selectionInfo.hasMixedChannelValue[c];
                            EditorGUI.BeginChangeCheck();
                            var newValue = EditorGUI.FloatField(nr, s_XYZWLabels[c], selectionInfo.selectedVectorValue[c]);
                            if (EditorGUI.EndChangeCheck())
                            {
                                _changed = true;
                                // Apply a curve's modification
                                foreach (var selection in _selectedCurves.Where(selection => selection.curveID == c))
                                {
                                    var cw = _curveEditor.animationCurves[selection.curveID];
                                    var key = cw.curve.keys[selection.key];
                                    key.value = newValue;
                                    cw.MoveKey(selection.key, ref key);
                                    cw.changed = true;
                                    AnimationUtility.UpdateTangentsFromMode(cw.curve);
                                }
                                _curveEditor.InvalidateSelectionBounds();
                                InitGradientEditor(true);
                                SyncSelectionFromCurveToGradient(true);
                            }
                            nr.x += w + EditorGUI.kSpacingSubLabel;
                        }
                        EditorGUI.indentLevel = l;
                    }
                }
                
                // Alpha or Color field
                {
                    rect = new Rect(_editFieldRect.x, rect.y, rect.x - _editFieldRect.x - space, rect.height);
                    EditorGUIUtility.labelWidth = alphaOrColorTextWidth;
                    EditorGUI.BeginChangeCheck();
                    if (selectionInfo.isOnlyColorKeySelected)
                    {
                        EditorGUI.showMixedValue = selectionInfo.hasMixedColorValue;
                        var newColorValue = new Vector4(selectionInfo.selectedVectorValue.x, selectionInfo.selectedVectorValue.y, selectionInfo.selectedVectorValue.z, 1);
                        newColorValue = EditorGUI.ColorField(rect, new GUIContent("Value"), newColorValue, true, false, colorSpace == ColorSpace.Linear);
                        newColorValue.w = selectionInfo.selectedVectorValue.w;
                        if (EditorGUI.EndChangeCheck())
                        {
                            _changed = true;
                            // Use EventCommandNames.ColorPickerChanged event to avoid ShowSwatchArray() Errors
                            Event.current.Use();
                            // Apply RGB curve's modification
                            foreach (var selection in _selectedCurves.Where(selection => selection.curveID < (int)LwguiGradient.Channel.Alpha))
                            {
                                var channelID = selection.curveID;
                                var cw = _curveEditor.animationCurves[channelID];
                                var key = cw.curve.keys[selection.key];
                                key.value = newColorValue[channelID];
                                cw.MoveKey(selection.key, ref key);
                                cw.changed = true;
                                AnimationUtility.UpdateTangentsFromMode(cw.curve);
                            }
                            _curveEditor.InvalidateSelectionBounds();
                            InitGradientEditor(true);
                        }
                    }
                    else
                    {
                        EditorGUI.showMixedValue = selectionInfo.hasMixedFloatValue;
                        var newValue = EditorGUI.IntSlider(rect, "Value", (int)(selectionInfo.selectedFloatValue * 255), 0, 255) / 255f;
                        if (EditorGUI.EndChangeCheck())
                        {
                            _changed = true;
                            _curveEditor.SetSelectedKeyPositions(0, newValue, false, true);
                            InitGradientEditor(true);
                        }
                    }
                }
                
                EditorGUIUtility.labelWidth = labelWidth;
                EditorGUI.showMixedValue = showMixedValue;
            }
        }

        /// <summary>
        /// If the new time of the key is too close to the existing key, Curve Editor will discard one of them.
        /// To avoid this, Key conflicts need to be detected and fixed
        /// </summary>
        private bool HandleNewKeyTimeConflicts(ref float outOffsetedNewKeyTime, float newKeyTime, CurveSelectionInfo selectionInfo)
        {
	        const float MinimumInterval = 0.0001f;
	        const float NearbyKeyThreshold = MinimumInterval * 10;
	        
	        // Collect nearby keys
	        var nearbyKeyTimes = new List<float>();
	        for (int c = 0; c < _curveEditor.animationCurves.Length; c++)
	        {
		        if (!selectionInfo.HasSelectedChannel(c))
			        continue;
		                        
		        foreach (var key in _curveEditor.animationCurves[c].curve.keys)
		        {
			        if (Mathf.Abs(key.time - newKeyTime) < NearbyKeyThreshold
			            && !nearbyKeyTimes.Contains(key.time))
				        nearbyKeyTimes.Add(key.time);
		        }
	        }

	        // Offset New Time to avoid conflicts with existing Keys
	        if (nearbyKeyTimes.Count > 0)
	        {
		        Debug.LogWarning($"LWGUI Gradient Editor: { nearbyKeyTimes.Count } keys that are too close are detected near the new time, " +
		                         $"and LWGUI will automatically fix the conflicts of the times. " +
		                         $"\n But too many nearby keys may still cause some problems, please be careful to use.");
		        
		        bool isNewTimeOnTheLeft = newKeyTime < selectionInfo.selectedKeysAverageTime;
		        float offsetDirection = isNewTimeOnTheLeft ? 1 : -1;
		        
		        nearbyKeyTimes.Sort();
		        if (!isNewTimeOnTheLeft)
			        nearbyKeyTimes.Reverse();

		        for (int i = 0; i < nearbyKeyTimes.Count; i++)
		        {
			        var currentNearbyKeyTime = nearbyKeyTimes[i];
			        if ((newKeyTime - currentNearbyKeyTime) * offsetDirection > MinimumInterval)
				        continue;

			        var lastNearbyKeyTime = nearbyKeyTimes[Mathf.Max(0, i - 1)];
			                        
			        // No conflict, no offset required
			        if (Mathf.Abs(outOffsetedNewKeyTime - currentNearbyKeyTime) >= MinimumInterval
			            && Mathf.Abs(outOffsetedNewKeyTime - lastNearbyKeyTime) >= MinimumInterval)
			        {
				        break;
			        }
			        // Has conflict, offset to the selectedKeysAverageTime
			        else
			        {
				        if (Mathf.Abs(outOffsetedNewKeyTime - lastNearbyKeyTime) < MinimumInterval)
				        {
					        outOffsetedNewKeyTime = lastNearbyKeyTime + MinimumInterval * offsetDirection;
				        }
				        if (Mathf.Abs(outOffsetedNewKeyTime - currentNearbyKeyTime) < MinimumInterval)
				        {
					        outOffsetedNewKeyTime = currentNearbyKeyTime + MinimumInterval * offsetDirection;
				        }
			        }
		        }

		        var offsetedNewKeyTime = outOffsetedNewKeyTime;
		        if (nearbyKeyTimes.Any(time => Mathf.Abs(offsetedNewKeyTime - time) < MinimumInterval))
		        {
			        EditorUtility.DisplayDialog("LWGUI Gradient Editor",
				        "Time conflicts of Keys were detected! \nPlease avoid the time when inputting too close to existing Keys!", "OK");
			        return false;
		        }
	        }
			return true;
        }

        private void ShowGradientSwatchArray(Rect rect, List<GradientEditor.Swatch> swatches, LwguiGradient.ChannelMask drawingChannelMask)
        {
            // GradientEditor.ShowSwatchArray()
            ReflectionHelper.GradientEditor_SetStyles();

            _isAddGradientKeyFailure = false;
            _gradientEditor.ShowSwatchArray(rect, 
                // Swatches are not displayed when ViewChannel != RGB to avoid modifying other channels
                (viewChannelMask & drawingChannelMask) != drawingChannelMask ? new List<GradientEditor.Swatch>() : swatches, 
                drawingChannelMask == LwguiGradient.ChannelMask.Alpha);
            
            // Since the maximum number of Gradient Keys is hard-coded in the engine, keys that exceed the limit can only be displayed and edited in the Curve Editor
            if (_isAddGradientKeyFailure)
            {
                _changed = true;
                _curveEditor.SelectNone();
                float mouseSwatchTime = _gradientEditor.GetTime((Event.current.mousePosition.x - rect.x) / rect.width);
                for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
                {
                    if (!LwguiGradient.IsChannelIndexInMask(c, drawingChannelMask))
                        continue;
                                
                    var curveSelection = _curveEditor.AddKeyAtTime(_curveEditor.animationCurves[c], mouseSwatchTime);
                    _curveEditor.AddSelection(curveSelection);
                }
                _curveEditor.InvalidateSelectionBounds();
                _selectedGradientKey = null;
            }
        }

        private void ApplyGradientChangesToCurve()
        {
            var selectedKeyEqual    = _selectedGradientKey == _lastSelectedGradientKey || Equal(_selectedGradientKey, _lastSelectedGradientKey);
            var rgbKeyCountEqual    = _gradientRGBSwatches.Count == _lastGradientRGBSwatchesCount;
            var alphaKeyCountEqual  = _gradientAlphaSwatches.Count == _lastGradientAlphaSwatchesCount;
            var addRGBKey           = _gradientRGBSwatches.Count > _lastGradientRGBSwatchesCount;
            var addAlphaKey         = _gradientAlphaSwatches.Count > _lastGradientAlphaSwatchesCount;
            var delRGBKey           = _gradientRGBSwatches.Count < _lastGradientRGBSwatchesCount;
            var delAlphaKey         = _gradientAlphaSwatches.Count < _lastGradientAlphaSwatchesCount;
            
            if (selectedKeyEqual && rgbKeyCountEqual && alphaKeyCountEqual)
            {
                return;
            }
            
            // Change time or value
            if ((!selectedKeyEqual && rgbKeyCountEqual && alphaKeyCountEqual)
                // Del a key
                || (delRGBKey || delAlphaKey))
            {
                foreach (var curveSelection in _selectedCurves.Where(selection => LwguiGradient.IsChannelIndexInMask(selection.curveID, viewChannelMask)))
                {
                    var cw = _curveEditor.animationCurves[curveSelection.curveID];
                    var selectedKey = cw.curve.keys[curveSelection.key];
                    if (rgbKeyCountEqual && alphaKeyCountEqual)
                    {
                        var newKey = selectedKey;
                        
                        // Change a key time
                        if (_selectedGradientKey.m_Time != _lastSelectedGradientKey.m_Time)
                        {
                            newKey.time = _selectedGradientKey.m_Time;
                        }
                        // Change a key value
                        else if (_selectedGradientKey.m_Value != _lastSelectedGradientKey.m_Value)
                        {
                            newKey.value = _selectedGradientKey.m_IsAlpha ? _selectedGradientKey.m_Value.r : _selectedGradientKey.m_Value[curveSelection.curveID];
                        }
                        newKey = CheckNewKeyTime(cw, newKey, selectedKey.time);
                        curveSelection.key = cw.MoveKey(curveSelection.key, ref newKey);
                    }
                    else
                    {
                        // Mouse drag out of the swatch rect, save the key
                        if (_selectedGradientKey != null)
                        {
                            _deletedGradientKey = new GradientEditor.Swatch(_selectedGradientKey.m_Time, _selectedGradientKey.m_Value, _selectedGradientKey.m_IsAlpha);
                            _deletedCurveKeys ??= new List<Keyframe>(new Keyframe[(int)LwguiGradient.Channel.Num]);
                            _deletedCurveKeys[curveSelection.curveID] = selectedKey;
                        }
                        
                        // Del a key
                        cw.curve.RemoveKey(curveSelection.key);
                    }
                    AnimationUtility.UpdateTangentsFromMode(cw.curve);
                    cw.changed = true;
                }
                
                if (delRGBKey || delAlphaKey)
                    _curveEditor.SelectNone();
            }
            else
            {
                var curveSelections = new List<CurveSelection>();

                for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
                {
                    if (!LwguiGradient.IsChannelIndexInMask(c, viewChannelMask))
                        continue;
                    
                    // Add a RGB Key
                    if ((c < (int)LwguiGradient.Channel.Alpha && addRGBKey)
                        // Add an Alpha Key
                        || (c == (int)LwguiGradient.Channel.Alpha && addAlphaKey))
                    {
                        var cw = _curveEditor.animationCurves[c];
                        
                        // Mouse drag back to the swatch rect, restore the key
                        if (_deletedGradientKey != null && _deletedCurveKeys != null
                            && _selectedGradientKey?.m_Value == _deletedGradientKey?.m_Value 
                            && _selectedGradientKey?.m_IsAlpha == _deletedGradientKey?.m_IsAlpha)
                        {
                            var deletedKey = _deletedCurveKeys[c];
                            var newKey = deletedKey;
                            newKey.time = _selectedGradientKey.m_Time;
                            newKey = CheckNewKeyTime(cw, newKey, deletedKey.time);
                            var addedKeyIndex = cw.AddKey(newKey);
                            curveSelections.Add(new CurveSelection(c, addedKeyIndex, CurveSelection.SelectionType.Key));
                        }
                        // Add a new key
                        else
                        {
                            var curveSelection = _curveEditor.AddKeyAtTime(cw, _selectedGradientKey.m_Time);
                            curveSelections.Add(curveSelection);
                        }
                        
                        cw.selected = CurveWrapper.SelectionMode.Selected;
                        cw.changed = true;
                    }
                }

                _deletedGradientKey = null;
                _deletedCurveKeys = null;
                _curveEditor.SelectNone();
                curveSelections.ForEach(selection => _curveEditor.AddSelection(selection));
                InitGradientEditor(true);
            }
            
            _curveEditor.InvalidateSelectionBounds();
            InitCurveEditor(true);

            // Cannot overlap with the Time of an existing Key when adding or moving Keys
            Keyframe CheckNewKeyTime(CurveWrapper cw, Keyframe newKey, float oldKeyTime = 0)
            {
                try
                {
                    var sameTimeKey = cw.curve.keys.First(keyframe => keyframe.time == newKey.time);
                    if (newKey.time > oldKeyTime)
                        newKey.time += 0.00001f;
                    else
                        newKey.time -= 0.00001f;
                }
                catch (InvalidOperationException) { }

                return newKey;
            }
        }

        private void PrepareSyncSelectionFromGradientToCurve()
        {
            _lastSelectedGradientKey = _selectedGradientKey != null ? new GradientEditor.Swatch(_selectedGradientKey.m_Time, _selectedGradientKey.m_Value, _selectedGradientKey.m_IsAlpha) : null;
            _lastGradientRGBSwatchesCount = _gradientRGBSwatches.Count;
            _lastGradientAlphaSwatchesCount = _gradientAlphaSwatches.Count;
        }

        private void SyncSelectionFromGradientToCurveWithoutChanges()
        {
            // Only detect when switching selected Key without modifying it
            if (!_gradientEditorRect.Contains(Event.current.mousePosition) 
                // || Event.current.type != EventType.MouseDown
                || _changed
                || _lastChanged
                || Equal(_lastSelectedGradientKey, _selectedGradientKey))
                return;
            
            if (_selectedGradientKey == null)
            {
                _curveEditor.SelectNone();
                return;
            }
            
            // Get selected gradient key index
            var selectedGradientKeyIndexes = Enumerable.Repeat(-1, (int)LwguiGradient.Channel.Num).ToArray();

            if (_selectedGradientKey.m_IsAlpha)
            {
                selectedGradientKeyIndexes[(int)LwguiGradient.Channel.Alpha] = _gradientAlphaSwatches.FindIndex(swatch => swatch == _selectedGradientKey);
            }
            else
            {
                for (int c = 0; c < (int)LwguiGradient.Channel.Num - 1; c++)
                {
                    selectedGradientKeyIndexes[c] = _gradientRGBSwatches.FindIndex(swatch => swatch == _selectedGradientKey);
                }
            }
            
            // Get curve key index
            _curveEditor.SelectNone();
            var lwguiMergedCurves = new LwguiMergedColorCurves(lwguiGradient.rawCurves);
            for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
            {
                if (selectedGradientKeyIndexes[c] < 0)
                    continue;

                var curveKeyIndex = lwguiMergedCurves.curves[c][selectedGradientKeyIndexes[c]].index;
                _selectedCurves.Add(new CurveSelection(c, curveKeyIndex));
            }
            _curveEditor.InvalidateSelectionBounds();
        }

        #endregion

        #region Curve Editor
        
        private void InitCurveEditor(bool force = false)
        {
            if (_curveEditor != null && !force)
                return;

            var firstOpenWindow = _curveEditor == null;
            _curveEditor = CurveEditorWindow.instance.GetCurveEditor();

            var cws = new CurveWrapper[(int)LwguiGradient.Channel.Num];
            for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
            {
                var curve = lwguiGradient.rawCurves[c];
                var cw = new CurveWrapper();
                cw.id = c;
                if (LwguiGradient.IsChannelIndexInMask(c, viewChannelMask))
                {
                    cw.color = LwguiGradient.channelColors[c];
                    cw.renderer = new NormalCurveRenderer(curve);
                    cw.renderer.SetWrap(curve.preWrapMode, curve.postWrapMode);
                }
                else
                {
                    cw.renderer = new NormalCurveRenderer(new AnimationCurve());
                }
                cws[c] = cw;
            }

            _curveEditor.animationCurves = cws;
            _curveEditor.curvesUpdated = () =>
            {
	            _curveEditorContextMenuChanged = true;
            };
            
            SyncCurveEditorRect();

            if (firstOpenWindow)
            {
                _curveEditor.Frame(new Bounds(new Vector2(0.5f, 0.5f), Vector2.one), true, true);
                _curveEditor.SelectNone();
            }
        }

        private void OnCurveEditorGUI()
        {
            GUI.Label(_curveEditorRect, GUIContent.none, style_PopupCurveEditorBackground);
            EditorGUI.DrawRect(new Rect(_curveEditorRect.x, _curveEditorRect.y, _curveEditorRect.width, 1), new Color(0.5f, 0.5f, 0.5f, 0.5f));

            PrepareSyncSelectionFromCurveToGradient();
            EditorGUI.BeginChangeCheck();
            _curveEditor.OnGUI();
            bool curveEditorChanged = EditorGUI.EndChangeCheck() || _curveEditorContextMenuChanged;
            _changed |= curveEditorChanged;
            if (curveEditorChanged)
            {
				InitGradientEditor(true);
				foreach (var cw in _curveEditor.animationCurves)
				{
					cw.changed = false;
				}
				_curveEditorContextMenuChanged = false;
            }
            SyncSelectionFromCurveToGradient();
        }

        private void SyncCurveEditorRect()
        {
            _curveEditor.rect = _curveEditorRect;
        }

        private void PrepareSyncSelectionFromCurveToGradient()
        {
            // var eventType = Event.current.GetTypeForControl(GUIUtility.GetControlID(897560, FocusType.Passive)); // CurveEditor.SelectPoints()
            var eventType = Event.current.type;

            if (_curveEditorRect.Contains(Event.current.mousePosition)
                && eventType is EventType.MouseDown or EventType.MouseDrag)
                _shouldSyncSelectionFromCurveToGradient = true;
        }

        private void SyncSelectionFromCurveToGradient(bool force = false)
        {
            if (!_shouldSyncSelectionFromCurveToGradient && !force)
                return;

            _shouldSyncSelectionFromCurveToGradient = false;
            _selectedGradientKey = null;

            var selectedGradientKeys = new List<GradientEditor.Swatch>();
            var mergedCurves = new CurveSelectionInfo(_curveEditor).mergedCurves;

            FindSelectedGradientKey((int)LwguiGradient.Channel.Red, _gradientRGBSwatches);
            FindSelectedGradientKey((int)LwguiGradient.Channel.Alpha, _gradientAlphaSwatches);

            // Sync selection to Gradient Editor only when single selection
            if (selectedGradientKeys.Count == 1)
                _selectedGradientKey = selectedGradientKeys[0];
            return;

            void FindSelectedGradientKey(int channel, List<GradientEditor.Swatch> list)
            {
                foreach (var lwguiKeyframe in mergedCurves.curves[channel])
                {
                    if (selectedGradientKeys.Count > 1) return;
                    
                    var key = list.Find(swatch => Equal(swatch.m_Time, lwguiKeyframe.time));
                    if (key != null)
                        selectedGradientKeys.Add(key);
                }
            }
        }

        #endregion

        #region Events

        public void Init(Rect position, 
            LwguiGradient gradient, 
            ColorSpace colorSpace = ColorSpace.Gamma, 
            LwguiGradient.ChannelMask viewChannelMask = LwguiGradient.ChannelMask.All, 
            LwguiGradient.GradientTimeRange timeRange = LwguiGradient.GradientTimeRange.One, 
            LwguiGradientWindow.ChangeGradientCallback onChange = null)
        {
            Clear();
            
            this._position = position;
            this.lwguiGradient = gradient;
            this.colorSpace = colorSpace;
            this.viewChannelMask = viewChannelMask;
            this.gradientTimeRange = timeRange;
            this._onChange = onChange;
        }

        public void OnGUI(Rect position)
        {
            if (lwguiGradient == null) 
                return;
            
            // Debug.Log(JsonUtility.ToJson(lwguiGradient));
            
            this._position = position;

            InitGradientEditor();
            InitCurveEditor();
            
            // Gradient Editor
            OnGradientEditorGUI();
            
            // Curve Editor
            SyncCurveEditorRect();
            OnCurveEditorGUI();

            _lastChanged = _changed;
            _changed = false;
            
            if (_lastChanged)
            {
                if (!EditorApplication.isPlaying)
                    UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                
                GUI.changed = true;
                // _onChange?.Invoke(lwguiGradient);
            }

            if (_viewSettingschanged)
            {
                _viewSettingschanged = false;
                GUI.changed = true;
            }
        }

        public void AddKeyAtTime(float normalizedTime, LwguiGradient.ChannelMask channelMask)
        {
            if (lwguiGradient == null || _curveEditor == null) return;

            _curveEditor.SelectNone();
            for (int c = 0; c < (int)LwguiGradient.Channel.Num; c++)
            {
                if (!LwguiGradient.IsChannelIndexInMask(c, channelMask)) continue;

                var cw = _curveEditor.animationCurves[c];

                bool keyExists = cw.curve.keys.Any(k => Equal(k.time, normalizedTime));
                if (keyExists) continue;

                var curveSelection = _curveEditor.AddKeyAtTime(cw, normalizedTime);
                cw.changed = true;
                _curveEditor.AddSelection(curveSelection);
            }
            _curveEditor.InvalidateSelectionBounds();
            _changed = true;
            InitGradientEditor(true);
            SyncSelectionFromCurveToGradient(true);
        }

        public void Clear()
        {
            lwguiGradient = null;
            _gradientEditor = null;
            _curveEditor?.OnDisable();
            _curveEditor = null;
            _lastChanged = false;
            _lastEditingTime = float.NegativeInfinity;
        }

        public static void CheckAddGradientKeyFailureLog(string logString, string stackTrace, LogType type)
        {
            if (type == LogType.Warning
                && logString == "Max " + ReflectionHelper.maxGradientKeyCount + " color keys and " + ReflectionHelper.maxGradientKeyCount + " alpha keys are allowed in a gradient.")
            {
                _isAddGradientKeyFailure = true;
            }
        }
        
        #endregion
    }

    public class AddKeyPopupContent : PopupWindowContent
    {
        private const string PrefKeyR = "LwguiGradientEditor.AddKey.R";
        private const string PrefKeyG = "LwguiGradientEditor.AddKey.G";
        private const string PrefKeyB = "LwguiGradientEditor.AddKey.B";
        private const string PrefKeyA = "LwguiGradientEditor.AddKey.A";

        private float _time;
        private bool _r, _g, _b, _a;
        private readonly LwguiGradientEditor _editor;
        private readonly LwguiGradient.GradientTimeRange _timeRange;

        public AddKeyPopupContent(LwguiGradientEditor editor, LwguiGradient.GradientTimeRange timeRange)
        {
            _editor = editor;
            _timeRange = timeRange;
            _r = EditorPrefs.GetBool(PrefKeyR, true);
            _g = EditorPrefs.GetBool(PrefKeyG, true);
            _b = EditorPrefs.GetBool(PrefKeyB, true);
            _a = EditorPrefs.GetBool(PrefKeyA, false);
        }

        public override Vector2 GetWindowSize() => new Vector2(230, 68);

        public override void OnGUI(Rect rect)
        {
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 80;

            bool timeFieldEnterPressed = Event.current.type == EventType.KeyDown
                && Event.current.keyCode is KeyCode.Return or KeyCode.KeypadEnter;
            _time = EditorGUILayout.FloatField("Time", _time);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Channel");
            _r = DrawChannelToggle("R", _r, PrefKeyR);
            _g = DrawChannelToggle("G", _g, PrefKeyG);
            _b = DrawChannelToggle("B", _b, PrefKeyB);
            _a = DrawChannelToggle("A", _a, PrefKeyA);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Add") || timeFieldEnterPressed)
            {
                TryAddKey();
            }
            EditorGUIUtility.labelWidth = labelWidth;
        }

        private void TryAddKey()
        {
            float normalizedTime = Mathf.Clamp01(_time / (int)_timeRange);

            LwguiGradient.ChannelMask mask = 0;
            if (_r) mask |= LwguiGradient.ChannelMask.Red;
            if (_g) mask |= LwguiGradient.ChannelMask.Green;
            if (_b) mask |= LwguiGradient.ChannelMask.Blue;
            if (_a) mask |= LwguiGradient.ChannelMask.Alpha;

            if (mask != 0)
                _editor.AddKeyAtTime(normalizedTime, mask);

            editorWindow.Close();
        }

        private static bool DrawChannelToggle(string label, bool value, string prefKey)
        {
            EditorGUI.BeginChangeCheck();
            value = GUILayout.Toggle(value, label);
            if (EditorGUI.EndChangeCheck())
                EditorPrefs.SetBool(prefKey, value);
            return value;
        }
    }
}