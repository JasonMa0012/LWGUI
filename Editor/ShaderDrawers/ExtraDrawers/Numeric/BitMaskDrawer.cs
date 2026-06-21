// Copyright (c) Jason Ma

using System.Collections.Generic;
using System.Linq;
using LWGUI.Runtime;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Draw the Int value as a Bit Mask.
	/// Note:
	///		- Currently only 8 bits are supported.
	///
	/// Warning 1: If used to set Stencil, it will conflict with SRP Batcher!  
	///		(Reproduced in Unity 2022)  
	///		SRP Batcher does not correctly handle multiple materials with different Stencil Ref values,  
	///		mistakenly merging them into a single Batch and randomly selecting one material's Stencil Ref value for the entire Batch.  
	///		In theory, if different materials have different Stencil Ref values, they should not be merged into a single Batch due to differing Render States.  
	/// Solution:  
	///		- Force disable SRP Batcher by setting the Material Property Block  
	///		- Place materials with the same Stencil Ref value in a separate Render Queue to ensure the Batch's Render State is correct
	///
	/// Warning 2: Once in use, do not change the Target Property Type!
	///		The underlying type of Int Property is Float Property, and in Materials, Int and Integer are stored separately.  
	///		Once a Material is saved, the Property Type is determined.  
	///		If you change the Property Type at this point (such as switching between Int/Integer), some strange bugs may occur.  
	///		If you must change the Property Type, it is recommended to modify the Property Name as well or delete the saved Property in the material.
	/// 
	/// group: parent group name (Default: none)
	/// bitDescription 7-0: Description of each Bit. (Default: none)
	/// Target Property Type: Int/Integer
	/// </summary>
	[LwguiDrawerCategory("Numeric", 10)]
	[LwguiDrawerParameterString("group", "", "Empty")]
	[LwguiDrawerParameterString("bitDescription7", "", "Empty")]
	[LwguiDrawerParameterString("bitDescription6", "", "Empty")]
	[LwguiDrawerParameterString("bitDescription5", "", "Empty")]
	[LwguiDrawerParameterString("bitDescription4", "", "Empty")]
	[LwguiDrawerParameterString("bitDescription3", "", "Empty")]
	[LwguiDrawerParameterString("bitDescription2", "", "Empty")]
	[LwguiDrawerParameterString("bitDescription1", "", "Empty")]
	[LwguiDrawerParameterString("bitDescription0", "", "Empty")]
	public class BitMaskDrawer : SubDrawer
	{
		public int bitCount = 8;
		
		public float maxHeight = EditorGUIUtility.singleLineHeight * 2;

		public List<GUIContent> buttonLables = new ();
		
		public List<float> buttonWidths = new();
		
		public List<GUIStyle> buttonStyles = new();

		public float totalButtonWidth;
		
		private static readonly int _hint = "BitMask".GetHashCode();
		
		private static readonly float _minButtonWidth = 25;
		
		private static readonly float _buttonPadding = 1.0f;
		
		public BitMaskDrawer() : this(string.Empty, null) { }
		
		public BitMaskDrawer(string group) : this(group, null) { }
		
		public BitMaskDrawer(string group, string bitDescription7, string bitDescription6, string bitDescription5, string bitDescription4, string bitDescription3, string bitDescription2, string bitDescription1, string bitDescription0) 
			: this(group, new List<string>() { bitDescription0, bitDescription1, bitDescription2, bitDescription3, bitDescription4, bitDescription5, bitDescription6, bitDescription7 }) { }

		public BitMaskDrawer(string group, List<string> bitDescriptions)
		{
			this.group = group;

			bitCount = Mathf.Clamp(bitCount, 1, 16);

			for (int i = 0; i < bitCount; i++)
			{
				var description = bitDescriptions != null && bitDescriptions.Count > i ? bitDescriptions[i] : string.Empty;
				buttonLables.Add(new GUIContent(
					string.IsNullOrEmpty(description) ? i.ToString()			: i + "\n" + description));
				buttonWidths.Add(Mathf.Max(_minButtonWidth, EditorStyles.miniButton.CalcSize(buttonLables[i]).x));

				if (!string.IsNullOrEmpty(description))
					maxHeight = EditorGUIUtility.singleLineHeight * 3;
			}

			for (int i = 0; i < bitCount; i++)
			{
				if (i == 0)
					buttonStyles.Add(new GUIStyle(EditorStyles.miniButtonRight));
				else if (i == bitCount - 1)
					buttonStyles.Add(new GUIStyle(EditorStyles.miniButtonLeft));
				else
					buttonStyles.Add(new GUIStyle(EditorStyles.miniButton));
					
				buttonStyles[i].fixedHeight = maxHeight - EditorGUIUtility.singleLineHeight;
			}

			totalButtonWidth = buttonWidths.Sum();
		}
		
		public override bool IsMatchPropType(ShaderPropertyType propType) 
			=> propType is ShaderPropertyType.Float or ShaderPropertyType.Int;

		protected override float GetVisibleHeight(MaterialProperty prop)
		{
			return string.IsNullOrEmpty(prop.displayName) ? EditorGUIUtility.singleLineHeight : maxHeight;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			bool hasLabel = !string.IsNullOrEmpty(label.text);
			if (hasLabel)
				label.tooltip += $"\nCurrent Value: { prop.GetNumericValue() }";
			
			int controlId = GUIUtility.GetControlID(_hint, FocusType.Keyboard, position);
			var fieldRect = EditorGUI.PrefixLabel(position, controlId, label);
			
			var needSecondRow = hasLabel && totalButtonWidth > fieldRect.width;
			var scale = needSecondRow ? (position.width - ReflectionHelper.EditorGUI_indent) / totalButtonWidth : fieldRect.width / totalButtonWidth;
			
			if (needSecondRow)
				fieldRect = new Rect(position.x + ReflectionHelper.EditorGUI_indent, position.y + EditorGUIUtility.singleLineHeight, position.width - ReflectionHelper.EditorGUI_indent, maxHeight - EditorGUIUtility.singleLineHeight);

			fieldRect.xMin = fieldRect.xMax;
			
			for (int i = 0; i < bitCount; i++)
			{
				fieldRect.xMin = fieldRect.xMax - buttonWidths[i] * scale;
				var buttonLable = buttonLables[i];
				var active = RuntimeHelper.IsBitEnabled((int)prop.GetNumericValue(), i);
				var style = buttonStyles[i];
				var buttonRect = fieldRect;
				
				if (i > 0 && i < bitCount - 1)
				{
					buttonRect.xMin -= _buttonPadding;
					buttonRect.xMax += _buttonPadding * 2;
				}

				if (prop.hasMixedValue)
				{
					style.richText = true;
					// https://docs.unity3d.com/2021.3/Documentation/Manual/StyledText.html
					buttonLable = new GUIContent($"<b><i>{ buttonLable.text }</i></b>");
				}

				if (Helper.ToggleButton(buttonRect, buttonLable, active, style, _buttonPadding * 1.5f))
				{
					prop.SetNumericValue(RuntimeHelper.SetBitEnabled((int)prop.GetNumericValue(), i, !active));
				}

				fieldRect.xMax = fieldRect.xMin;
			}
		}
	}
}

