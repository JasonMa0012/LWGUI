// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Display a Help URL button on the right side of the Group.
	/// Clicking the button opens the URL in the default browser.
	/// "https://" is automatically prepended.
	/// Each comma-separated parameter is joined with '/' to form the full URL path.
	/// 
	/// url: the document URL without "https://", each ',' = '/'. (Default: none)
	/// e.g. [HelpURL(github.com, JasonMa0012, LWGUI)] => https://github.com/JasonMa0012/LWGUI
	/// </summary>
	[LwguiDrawerCategory("Appearance")]
	public class HelpURLDecorator : SubDrawer
	{
		private string _url;

		private static string Join(params string[] segments) => string.Join("/", segments);

		#region

		public HelpURLDecorator(string url) { this._url = url; }
		public HelpURLDecorator(string s1, string s2) : this(Join(s1, s2)) { }
		public HelpURLDecorator(string s1, string s2, string s3) : this(Join(s1, s2, s3)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4) : this(Join(s1, s2, s3, s4)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5) : this(Join(s1, s2, s3, s4, s5)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6) : this(Join(s1, s2, s3, s4, s5, s6)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7) : this(Join(s1, s2, s3, s4, s5, s6, s7)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8, string s9) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8, s9)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8, string s9, string s10) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8, string s9, string s10, string s11) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8, string s9, string s10, string s11, string s12) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8, string s9, string s10, string s11, string s12, string s13) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8, string s9, string s10, string s11, string s12, string s13, string s14) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8, string s9, string s10, string s11, string s12, string s13, string s14, string s15) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15)) { }
		public HelpURLDecorator(string s1, string s2, string s3, string s4, string s5, string s6, string s7, string s8, string s9, string s10, string s11, string s12, string s13, string s14, string s15, string s16) : this(Join(s1, s2, s3, s4, s5, s6, s7, s8, s9, s10, s11, s12, s13, s14, s15, s16)) { }

		#endregion


		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.helpURL = "https://" + _url.Replace(" ", "");
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
}
