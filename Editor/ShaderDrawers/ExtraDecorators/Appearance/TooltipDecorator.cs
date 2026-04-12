// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Tooltip, describes the details of the property. (Default: property.name and property default value)
	/// You can also use "#Text" in DisplayName to add Tooltip that supports Multi-Language.
	/// 
	/// tooltip: a single-line string to display, support up to 4 ','. (Default: Newline)
	/// </summary>
	[LwguiDrawerCategory("Appearance")]
	public class TooltipDecorator : SubDrawer
	{
		private string _tooltip;


		#region

		public TooltipDecorator() : this(string.Empty) { }

		public TooltipDecorator(string tooltip) { this._tooltip = tooltip; }

		public TooltipDecorator(string s1, string s2) : this(s1 + ", " + s2) { }

		public TooltipDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }

		public TooltipDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }

		public TooltipDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }

		#endregion


		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.tooltipMessages += _tooltip + "\n";
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
}
