// Copyright (c) Jason Ma

using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	/// <summary>
	/// Similar to builtin Enum() / KeywordEnum()
	/// 
	/// group: parent group name (Default: none)
	/// n(s): display name
	/// k(s): keyword
	/// v(s): value
	/// Target Property Type: Float, express current keyword index
	/// </summary>
	[LwguiDrawerCategory("Enum")]
	public class KWEnumDrawer : SubDrawer
	{
		private GUIContent[] _names;
		private string[]     _keyWords;
		private float[]      _values;


		#region

		public KWEnumDrawer(string n1, string k1)
			: this("_", new string[1] { n1 }, new string[1] { k1 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2)
			: this("_", new string[2] { n1, n2 }, new string[2] { k1, k2 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3)
			: this("_", new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
			: this("_", new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }) { }

		public KWEnumDrawer(string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
			: this("_", new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }) { }

		public KWEnumDrawer(string group, string n1, string k1)
			: this(group, new string[1] { n1 }, new string[1] { k1 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2)
			: this(group, new string[2] { n1, n2 }, new string[2] { k1, k2 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3)
			: this(group, new string[3] { n1, n2, n3 }, new string[3] { k1, k2, k3 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4)
			: this(group, new string[4] { n1, n2, n3, n4 }, new string[4] { k1, k2, k3, k4 }) { }

		public KWEnumDrawer(string group, string n1, string k1, string n2, string k2, string n3, string k3, string n4, string k4, string n5, string k5)
			: this(group, new string[5] { n1, n2, n3, n4, n5 }, new string[5] { k1, k2, k3, k4, k5 }) { }

		#endregion


		public KWEnumDrawer(string group, string[] names, string[] keyWords = null, float[] values = null)
		{
			Init(group, names, keyWords, values);
		}

		protected void Init(string group, string[] names, string[] keyWords, float[] values)
		{
			this.group = group;

			this._names = new GUIContent[names.Length];
			for (int index = 0; index < names.Length; ++index)
				this._names[index] = new GUIContent(names[index]);

			if (keyWords == null)
			{
				keyWords = new string[names.Length];
				for (int i = 0; i < names.Length; i++)
					keyWords[i] = String.Empty;
			}
			this._keyWords = keyWords;

			if (values == null)
			{
				values = new float[names.Length];
				for (int index = 0; index < names.Length; ++index)
					values[index] = index;
			}
			this._values = values;
		}

		public override bool IsMatchPropType(ShaderPropertyType propType) { return propType == ShaderPropertyType.Float; }

		protected virtual string GetKeywordName(string propName, string name) { return (name).Replace(' ', '_').ToUpperInvariant(); }

		public override void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData)
		{
			var index = Array.IndexOf(_values, (int)inDefaultProp.floatValue);
			if (index < _names.Length && index >= 0)
				inoutPerMaterialData.propDynamicDatas[inProp.name].defaultValueDescription = _names[index].text;
		}

		private string[] GetKeywords(MaterialProperty property)
		{
			string[] keyWords = new string[_keyWords.Length];
			for (int i = 0; i < keyWords.Length; i++)
				keyWords[i] = GetKeywordName(property.name, _keyWords[i]);
			return keyWords;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;

			string[] keyWords = GetKeywords(prop);
			int index = Array.IndexOf(_values, prop.floatValue);
			if (index < 0)
			{
				Debug.LogError("LWGUI: Property: " + prop.name + " has unknown Enum Value: '" + prop.floatValue + "' !\n");
				return;
			}

			int newIndex = EditorGUI.Popup(position, label, index, _names);
			EditorGUI.showMixedValue = false;
			if (Helper.EndChangeCheck(metaDatas, prop))
			{
				prop.floatValue = _values[newIndex];
				Helper.SelectShaderKeyword(editor.targets, keyWords, newIndex);
			}
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && VersionControlHelper.IsWriteable(prop.targets))
			{
				Helper.SelectShaderKeyword(prop.targets, GetKeywords(prop), (int)prop.floatValue);
			}
		}
	}
}

