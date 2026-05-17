// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Control the show or hide of a single or a group of properties based on multiple conditions.
	/// 
	/// logicalOperator: And | Or (Default: And).
	/// propNameOrKeyword: Target Property Name or Keyword used for comparison. If no matching property is found, it falls back to checking material keywords (enabled = 1, disabled = 0).
	/// compareFunction: Less (L) | Equal (E) | LessEqual (LEqual / LE) | Greater (G) | NotEqual (NEqual / NE) | GreaterEqual (GEqual / GE).
	/// value: Target Property Value used for comparison.
	/// </summary>
	[LwguiDrawerCategory("Condition")]
	[LwguiDrawerParameterEnum("logicalOperator", LogicalOperator.And)]
	[LwguiDrawerParameterString("propNameOrKeyword")]
	[LwguiDrawerParameterEnum("compareFunction", CompareFunction.Equal)]
	[LwguiDrawerParameterFloat("value", 0f)]
	public class ShowIfDecorator : SubDrawer
	{
		public enum LogicalOperator
		{
			And,
			Or
		}

		public class ShowIfData
		{
			public LogicalOperator logicalOperator             = LogicalOperator.And;
			public string          targetPropertyNameOrKeyword = string.Empty;
			public CompareFunction compareFunction             = CompareFunction.Equal;
			public float           value                       = 0;
		}

		public ShowIfData showIfData = new();
		
		private static readonly Dictionary<string, string> _compareFunctionLUT = new()
		{
			{ "Less", "Less" },
			{ "L", "Less" },
			{ "Equal", "Equal" },
			{ "E", "Equal" },
			{ "LessEqual", "LessEqual" },
			{ "LEqual", "LessEqual" },
			{ "LE", "LessEqual" },
			{ "Greater", "Greater" },
			{ "G", "Greater" },
			{ "NotEqual", "NotEqual" },
			{ "NEqual", "NotEqual" },
			{ "NE", "NotEqual" },
			{ "GreaterEqual", "GreaterEqual" },
			{ "GEqual", "GreaterEqual" },
			{ "GE", "GreaterEqual" },
		};

		public static CompareFunction ParseCompareFunction(string compareFunction)
		{
			if (!_compareFunctionLUT.TryGetValue(compareFunction, out var compareFunctionName)
				|| !Enum.IsDefined(typeof(CompareFunction), compareFunctionName))
			{
				Debug.LogError("LWGUI: Invalid compareFunction: '"
							 + compareFunction
							 + "', Must be one of the following: Less (L) | Equal (E) | LessEqual (LEqual / LE) | Greater (G) | NotEqual (NEqual / NE) | GreaterEqual (GEqual / GE).");
				return CompareFunction.Equal;
			}

			return (CompareFunction)Enum.Parse(typeof(CompareFunction), compareFunctionName);
		}

		public ShowIfDecorator(string propNameOrKeyword, string compareFunction, float value) : this("And", propNameOrKeyword, compareFunction, value) { }

		public ShowIfDecorator(string logicalOperator, string propNameOrKeyword, string compareFunction, float value)
		{
			showIfData.logicalOperator = logicalOperator.ToLower() == "or" ? LogicalOperator.Or : LogicalOperator.And;
			showIfData.targetPropertyNameOrKeyword = propNameOrKeyword;
			showIfData.compareFunction = ParseCompareFunction(compareFunction);
			showIfData.value = value;
		}

		private static void Compare(ShowIfData showIfData, float targetValue, ref bool result)
		{
			bool compareResult;

			switch (showIfData.compareFunction)
			{
				case CompareFunction.Less:
					compareResult = targetValue < showIfData.value;
					break;
				case CompareFunction.LessEqual:
					compareResult = targetValue <= showIfData.value;
					break;
				case CompareFunction.Greater:
					compareResult = targetValue > showIfData.value;
					break;
				case CompareFunction.NotEqual:
					compareResult = targetValue != showIfData.value;
					break;
				case CompareFunction.GreaterEqual:
					compareResult = targetValue >= showIfData.value;
					break;
				default:
					compareResult = targetValue == showIfData.value;
					break;
			}

			switch (showIfData.logicalOperator)
			{
				case LogicalOperator.And:
					result &= compareResult;
					break;
				case LogicalOperator.Or:
					result |= compareResult;
					break;
			}
		}

		public static bool GetShowIfResultToFilterDrawerApplying(MaterialProperty prop)
		{
			var material = prop.targets[0] as Material;
			var showIfDatas = new List<ShowIfData>();
			{
				var drawer = ReflectionHelper.GetPropertyDrawer(material.shader, prop, out var decoratorDrawers);
				if (decoratorDrawers != null && decoratorDrawers.Count > 0)
				{
					foreach (ShowIfDecorator showIfDecorator in decoratorDrawers.Where(drawer => drawer is ShowIfDecorator))
					{
						showIfDatas.Add(showIfDecorator.showIfData);
					}
				}
				else
				{
					return true;
				}
			}

			return GetShowIfResultFromMaterial(showIfDatas, material);
		}
		
		public static float GetTargetValue(ShowIfData showIfData, Material material)
		{
			if (material.HasProperty(showIfData.targetPropertyNameOrKeyword))
				return material.GetFloat(showIfData.targetPropertyNameOrKeyword);
			return material.IsKeywordEnabled(showIfData.targetPropertyNameOrKeyword) ? 1f : 0f;
		}

		public static bool GetShowIfResultFromMaterial(List<ShowIfData> showIfDatas, Material material)
		{
			bool result = true;
			foreach (var showIfData in showIfDatas)
			{
				var targetValue = GetTargetValue(showIfData, material);
				Compare(showIfData, targetValue, ref result);
			}

			return result;
		}
		
		public static void GetShowIfResult(PropertyStaticData propStaticData, PropertyDynamicData propDynamicData, PerMaterialData perMaterialData)
		{
			foreach (var showIfData in propStaticData.showIfDatas)
			{
				float targetValue;
				if (perMaterialData.propDynamicDatas.TryGetValue(showIfData.targetPropertyNameOrKeyword, out var targetPropDynamicData))
					targetValue = targetPropDynamicData.property.floatValue;
				else
					targetValue = perMaterialData.material.IsKeywordEnabled(showIfData.targetPropertyNameOrKeyword) ? 1f : 0f;
				Compare(showIfData, targetValue, ref propDynamicData.isShowing);
			}
		}

		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.showIfDatas.Add(showIfData);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}
}
