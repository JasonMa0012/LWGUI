// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Collapse the current Property into an Advanced Block.
	/// Specify the Header String to create a new Advanced Block.
	/// All Properties using Advanced() will be collapsed into the nearest Advanced Block.
	/// 
	/// headerString: The title of the Advanced Block. Default: "Advanced"
	/// </summary>
	[LwguiDrawerCategory("Structure")]
	public class AdvancedDecorator : SubDrawer
	{
		private string headerString;

		public AdvancedDecorator() : this(string.Empty) { }

		public AdvancedDecorator(string headerString)
		{
			this.headerString = headerString;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.isAdvanced = true;
			inoutPropertyStaticData.advancedHeaderString = headerString;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
}
