// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Draw a min max slider
	/// 
	/// group: parent group name (Default: none)
	/// minPropName: Output Min Property Name
	/// maxPropName: Output Max Property Name
	/// Target Property Type: Range, range limits express the MinMaxSlider value range
	/// Output Min/Max Property Type: Range, it's value is limited by it's range
	/// </summary>
	[LwguiDrawerCategory("Numeric")]
	public class MinMaxSliderDrawer : SubDrawer
	{
		private string _minPropName;
		private string _maxPropName;

		public MinMaxSliderDrawer(string minPropName, string maxPropName) : this("_", minPropName, maxPropName) { }

		public MinMaxSliderDrawer(string group, string minPropName, string maxPropName)
		{
			this.group = group;
			this._minPropName = minPropName;
			this._maxPropName = maxPropName;
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) { return propType == ShaderPropertyType.Range; }

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			base.BuildStaticMetaData(inShader, inProp, inProps, inoutPropertyStaticData);
			inoutPropertyStaticData.AddExtraProperty(_minPropName);
			inoutPropertyStaticData.AddExtraProperty(_maxPropName);
		}

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			if (string.IsNullOrEmpty(_minPropName)
			 || string.IsNullOrEmpty(_maxPropName)
			 || !inoutPerMaterialData.propDynamicDatas.ContainsKey(_minPropName)
			 || !inoutPerMaterialData.propDynamicDatas.ContainsKey(_maxPropName)
			   )
			{
				Debug.LogError("LWGUI: " + inProp.name + " has no available min/max properties!");
				return;
			}

			inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription =
				inoutPerMaterialData.propDynamicDatas[_minPropName].defaultProperty.floatValue
			  + " - "
			  + inoutPerMaterialData.propDynamicDatas[_maxPropName].defaultProperty.floatValue;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// read min max
			MaterialProperty minProp = metaDatas.GetProperty(_minPropName);
			MaterialProperty maxProp = metaDatas.GetProperty(_maxPropName);
			if (minProp == null || maxProp == null)
			{
				Debug.LogError("LWGUI: MinMaxSliderDrawer: minProp: " + (minProp == null ? "null" : minProp.name) + " or maxProp: " + (maxProp == null ? "null" : maxProp.name) + " not found!");
				return;
			}
			float minf = minProp.floatValue;
			float maxf = maxProp.floatValue;

			// define draw area
			Rect controlRect = position; // this is the full length rect area
			Rect inputRect = MaterialEditor.GetRectAfterLabelWidth(controlRect); // this is the remaining rect area after label's area

			// draw label
			EditorGUI.PrefixLabel(controlRect, label);

			// draw min max slider
			var indentLevel = EditorGUI.indentLevel;
			EditorGUI.indentLevel = 0;
			Rect[] splittedRect = Helper.SplitRect(inputRect, 3);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = minProp.hasMixedValue;
			var newMinf = EditorGUI.FloatField(splittedRect[0], minf);
			if (Helper.EndChangeCheck(metaDatas, minProp))
			{
				minf = Mathf.Clamp(newMinf, minProp.rangeLimits.x, minProp.rangeLimits.y);
				minProp.floatValue = minf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = maxProp.hasMixedValue;
			var newMaxf = EditorGUI.FloatField(splittedRect[2], maxf);
			if (Helper.EndChangeCheck(metaDatas, maxProp))
			{
				maxf = Mathf.Clamp(newMaxf, maxProp.rangeLimits.x, maxProp.rangeLimits.y);
				maxProp.floatValue = maxf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			if (splittedRect[1].width > 50f)
				EditorGUI.MinMaxSlider(splittedRect[1], ref minf, ref maxf, prop.rangeLimits.x, prop.rangeLimits.y);
			EditorGUI.showMixedValue = false;

			// write back min max if changed
			if (EditorGUI.EndChangeCheck())
			{
				minProp.floatValue = Mathf.Clamp(minf, minProp.rangeLimits.x, minProp.rangeLimits.y);
				maxProp.floatValue = Mathf.Clamp(maxf, maxProp.rangeLimits.x, maxProp.rangeLimits.y);
			}

			EditorGUI.indentLevel = indentLevel;
		}
	}
}

