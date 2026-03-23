// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Display a Helpbox on the property
	/// You can also use "%Text" in DisplayName to add Helpbox that supports Multi-Language.
	/// 
	/// message: a single-line string to display, support up to 4 ','. (Default: Newline)
	/// </summary>
	[DrawerCategory("Appearance")]
	public class HelpboxDecorator : TooltipDecorator
	{
		private string _message;


		#region

		public HelpboxDecorator() : this(string.Empty) { }

		public HelpboxDecorator(string message) { this._message = message; }

		public HelpboxDecorator(string s1, string s2) : this(s1 + ", " + s2) { }

		public HelpboxDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }

		public HelpboxDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }

		[ParameterDefault("s1", "", "Empty", true)]
		[ParameterDefault("s2", "", "Empty", true)]
		[ParameterDefault("s3", "", "Empty", true)]
		[ParameterDefault("s4", "", "Empty", true)]
		[ParameterDefault("s5", "", "Empty", true)]
		public HelpboxDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }

		#endregion


		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.helpboxMessages += _message + "\n";
		}
	}
}
