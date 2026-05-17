// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Control whether the property can be edited based on multiple conditions.
	///
	/// logicalOperator: And | Or (Default: And).
	/// propNameOrKeyword: Target Property Name or Keyword used for comparison. If no matching property is found, it falls back to checking material keywords (enabled = 1, disabled = 0).
	/// compareFunction: Less (L) | Equal (E) | LessEqual (LEqual / LE) | Greater (G) | NotEqual (NEqual / NE) | GreaterEqual (GEqual / GE).
	/// value: Target Property Value used for comparison.
	/// </summary>
	[LwguiDrawerCategory("Condition")]
	[LwguiDrawerParameterEnum("logicalOperator", ShowIfDecorator.LogicalOperator.And)]
	[LwguiDrawerParameterString("propNameOrKeyword")]
	[LwguiDrawerParameterEnum("compareFunction", UnityEngine.Rendering.CompareFunction.Equal)]
	[LwguiDrawerParameterFloat("value", 0f)]
	public class ActiveIfDecorator : SubDrawer
	{
		public ShowIfDecorator.ShowIfData activeIfData = new();

		public ActiveIfDecorator(string propNameOrKeyword, string compareFunction, float value) : this("And", propNameOrKeyword, compareFunction, value) { }

		public ActiveIfDecorator(string logicalOperator, string propNameOrKeyword, string compareFunction, float value)
		{
			activeIfData.logicalOperator = logicalOperator.ToLower() == "or" ? ShowIfDecorator.LogicalOperator.Or : ShowIfDecorator.LogicalOperator.And;
			activeIfData.targetPropertyNameOrKeyword = propNameOrKeyword;
			activeIfData.compareFunction = ShowIfDecorator.ParseCompareFunction(compareFunction);
			activeIfData.value = value;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.activeIfDatas.Add(activeIfData);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
}
