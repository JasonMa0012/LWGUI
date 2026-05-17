// Copyright (c) Jason Ma

using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Cooperate with Toggle to switch certain Passes.
	/// 
	/// lightModeName(s): Light Mode in Shader Pass (https://docs.unity3d.com/2017.4/Documentation/Manual/SL-PassTags.html)
	/// </summary>
	[LwguiDrawerCategory("Condition")]
	[LwguiDrawerParameterPassName("lightModeName1", "", "Empty")]
	[LwguiDrawerParameterPassName("lightModeName2", "", "Empty")]
	[LwguiDrawerParameterPassName("lightModeName3", "", "Empty")]
	[LwguiDrawerParameterPassName("lightModeName4", "", "Empty")]
	[LwguiDrawerParameterPassName("lightModeName5", "", "Empty")]
	[LwguiDrawerParameterPassName("lightModeName6", "", "Empty")]
	public class PassSwitchDecorator : SubDrawer
	{
		private string[] _lightModeNames;


		#region

		public PassSwitchDecorator(string lightModeName1)
			: this(new[] { lightModeName1 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2)
			: this(new[] { lightModeName1, lightModeName2 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2, string lightModeName3)
			: this(new[] { lightModeName1, lightModeName2, lightModeName3 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4)
			: this(new[] { lightModeName1, lightModeName2, lightModeName3, lightModeName4 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4, string lightModeName5)
			: this(new[] { lightModeName1, lightModeName2, lightModeName3, lightModeName4, lightModeName5 }) { }

		public PassSwitchDecorator(string lightModeName1, string lightModeName2, string lightModeName3, string lightModeName4, string lightModeName5, string lightModeName6)
			: this(new[] { lightModeName1, lightModeName2, lightModeName3, lightModeName4, lightModeName5, lightModeName6 }) { }

		public PassSwitchDecorator(string[] lightModeNames) { _lightModeNames = lightModeNames.Select((s => s.ToUpper())).ToArray(); }

		#endregion


		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override bool IsMatchPropType(ShaderPropertyType propType)
		{
			return propType == ShaderPropertyType.Float
				|| propType == ShaderPropertyType.Int;
		}

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData) { }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
				Helper.SetShaderPassEnabled(prop.targets, _lightModeNames, prop.floatValue > 0);
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
			{
				if (ShowIfDecorator.GetShowIfResultToFilterDrawerApplying(prop))
					Helper.SetShaderPassEnabled(prop.targets, _lightModeNames, prop.floatValue > 0);
			}
		}
	}
}
