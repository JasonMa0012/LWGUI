// Copyright (c) 2022 Jason Ma

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LWGUI
{
	/// <summary>
	/// Create a Folding Group
	/// group：group name (Default: Property Name)
	/// keyword：keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
	/// default Folding State: "on" or "off" (Default: off)
	/// default Toggle Displayed: "on" or "off" (Default: on)
	/// Target Property Type: FLoat, express Toggle value
	/// </summary>
	internal class MainDrawer : MaterialPropertyDrawer
	{
		private bool   _isFolding;
		private string _group;
		private string _keyword;
		private bool   _defaultFoldingState;
		private bool   _defaultToggleDisplayed;

		private static readonly float _height = 28f;

		public MainDrawer() : this(String.Empty) { }

		public MainDrawer(string group) : this(group, String.Empty) { }

		public MainDrawer(string group, string keyword) : this(group, keyword, "off") { }

		public MainDrawer(string group, string keyword, string defaultFoldingState) : this(group, keyword, defaultFoldingState, "on") { }

		public MainDrawer(string group, string keyword, string defaultFoldingState, string defaultToggleDisplayed)
		{
			this._group = group;
			this._keyword = keyword;
			this._defaultFoldingState = defaultFoldingState == "on";
			this._defaultToggleDisplayed = defaultToggleDisplayed == "on";
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var lwgui = Helper.GetLWGUI(editor);
			var toggleValue = prop.floatValue > 0;

			if (lwgui.lwguiEventType == LwguiEventType.Init)
			{
				MetaDataHelper.RegisterMainProp(lwgui.shader, prop, _group);
				MetaDataHelper.RegisterPropertyDefaultValueText(lwgui.shader, prop, 
																RevertableHelper.GetDefaultProperty(lwgui.shader, prop).floatValue > 0 ? "On" : "Off");
			}

			string finalGroupName = (_group != String.Empty && _group != "_") ? _group : prop.name;
			bool isFirstFrame = !GroupStateHelper.ContainsGroup(editor.target, finalGroupName);
			_isFolding = isFirstFrame ? !_defaultFoldingState : GroupStateHelper.GetGroupFolding(editor.target, finalGroupName);

			EditorGUI.BeginChangeCheck();
			bool toggleResult = Helper.Foldout(position, ref _isFolding, toggleValue, _defaultToggleDisplayed, label);
			// EditorGUI.showMixedValue = false;

			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = toggleResult ? 1.0f : 0.0f;
				Helper.SetShaderKeyWord(editor.targets, Helper.GetKeyWord(_keyword, prop.name), toggleResult);
			}

			GroupStateHelper.SetGroupFolding(editor.target, finalGroupName, _isFolding);
		}

		// Call in custom shader gui
		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return _height;
		}

		// Call when creating new material 
		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && (prop.type == MaterialProperty.PropType.Float 
#if UNITY_2021_1_OR_NEWER
									 || prop.type == MaterialProperty.PropType.Int
#endif
										))
				Helper.SetShaderKeyWord(prop.targets, Helper.GetKeyWord(_keyword, prop.name), prop.floatValue > 0f);
		}
	}

	/// <summary>
	/// Draw a property with default style in the folding group
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// Target Property Type: Any
	/// </summary>
	internal class SubDrawer : MaterialPropertyDrawer
	{
		protected string             group = String.Empty;
		protected MaterialProperty   prop;
		protected MaterialProperty[] props;
		protected LWGUI              lwgui;
		protected Shader             shader;

		public SubDrawer() { }

		public SubDrawer(string group)
		{
			this.group = group;
		}
		
		protected virtual bool IsMatchPropType(MaterialProperty property) { return true; }
		
		protected virtual float GetVisibleHeight(MaterialProperty prop)
		{
			var height = MaterialEditor.GetDefaultPropertyHeight(prop);
			return prop.type == MaterialProperty.PropType.Vector ? EditorGUIUtility.singleLineHeight : height;
		}

		public virtual void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			MetaDataHelper.RegisterSubProp(shader, prop, group);
		}

		public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			this.prop = prop;
			lwgui = Helper.GetLWGUI(editor);
			props = lwgui.props;
			shader = lwgui.shader;

			if (lwgui.lwguiEventType == LwguiEventType.Init)
			{
				Init(position, prop, label, editor);
				return;
			}
			
			var rect = position;
			
			if (group != String.Empty && group != "_")
				EditorGUI.indentLevel++;
			
			if (GroupStateHelper.IsSubVisible(editor.target, group))
			{
				if (IsMatchPropType(prop))
				{
					RevertableHelper.SetRevertableGUIWidths();
					DrawProp(rect, prop, label, editor);
				}
				else
				{
					Debug.LogWarning($"Property:'{prop.name}' Type:'{prop.type}' mismatch!");
					editor.DefaultShaderProperty(rect, prop, label.text);
				}
			}

			if (group != String.Empty && group != "_")
				EditorGUI.indentLevel--;
		}

		public override float GetPropertyHeight(MaterialProperty prop, string label, MaterialEditor editor)
		{
			return GroupStateHelper.IsSubVisible(editor.target, group) ? GetVisibleHeight(prop) : 0;
		}

		// Draws a custom style property
		public virtual void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// Process some builtin types display misplaced
			switch (prop.type)
			{
				case MaterialProperty.PropType.Texture:
				case MaterialProperty.PropType.Range:
					editor.SetDefaultGUIWidths();
					break;
			}
			// TODO: use Reflection
			editor.DefaultShaderProperty(position, prop, label.text);
			GUI.Label(position, new GUIContent(String.Empty, label.tooltip));
		}
	}

	/// <summary>
	/// Similar to builtin Toggle()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// keyword：keyword used for toggle, "_" = ignore, none or "__" = Property Name +  "_ON", always Upper (Default: none)
	/// Target Property Type: FLoat
	/// </summary>
	internal class SubToggleDrawer : SubDrawer
	{
		private string _keyWord = String.Empty;
		
		public SubToggleDrawer() { }
		public SubToggleDrawer(string group) : this(group, String.Empty) { }

		public SubToggleDrawer(string group, string keyWord)
		{
			this.group = group;
			this._keyWord = keyWord;
		}
		
		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			base.Init(position, prop, label, editor);
			MetaDataHelper.RegisterPropertyDefaultValueText(shader, prop, 
															RevertableHelper.GetDefaultProperty(shader, prop).floatValue > 0 ? "On" : "Off");
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			EditorGUI.BeginChangeCheck();
			var rect = position;//EditorGUILayout.GetControlRect();
			var value = EditorGUI.Toggle(rect, label, prop.floatValue > 0.0f);
			string k = Helper.GetKeyWord(_keyWord, prop.name);
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = value ? 1.0f : 0.0f;
				Helper.SetShaderKeyWord(editor.targets, k, value);
			}

			GroupStateHelper.SetKeywordConditionalDisplay(editor.target, k, value);
			EditorGUI.showMixedValue = false;
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && IsMatchPropType(prop))
				Helper.SetShaderKeyWord(prop.targets, Helper.GetKeyWord(_keyWord, prop.name), prop.floatValue > 0f);
		}
	}

	/// <summary>
	/// Similar to builtin PowerSlider()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// power: power of slider (Default: 1)
	/// Target Property Type: Range
	/// </summary>
	internal class SubPowerSliderDrawer : SubDrawer
	{
		private float _power = 1;
		
		public SubPowerSliderDrawer(float power) : this("_", power) { }

		public SubPowerSliderDrawer(string group, float power)
		{
			this.group = group;
			this._power = Mathf.Clamp(power, 0, float.MaxValue);
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Range; }
		
		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			editor.SetDefaultGUIWidths();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position; //EditorGUILayout.GetControlRect();
			Helper.PowerSlider(prop, _power, rect, label);
			EditorGUI.showMixedValue = false;
		}
	}
	
	/// <summary>
	/// Similar to builtin Enum() / KeywordEnum()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// n(s): display name
	/// k(s): keyword
	/// v(s): value
	/// Target Property Type: FLoat, express current keyword index
	/// </summary>
	internal class KWEnumDrawer : SubDrawer
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

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		protected virtual string GetKeywordName(string propName, string name) { return (name).Replace(' ', '_').ToUpperInvariant(); }

		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			base.Init(position, prop, label, editor);
			var index = (int)RevertableHelper.GetDefaultProperty(shader, prop).floatValue;
			if (index < _names.Length && index >= 0)
				MetaDataHelper.RegisterPropertyDefaultValueText(shader, prop, _names[index].text);
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
        	
			var rect = position; //EditorGUILayout.GetControlRect();

			string[] keyWords = GetKeywords(prop);
			int index = Array.IndexOf(_values, prop.floatValue);
			if (index < 0)
			{
				index = 0;
				if (!prop.hasMixedValue)
				{
					Debug.LogError("Property: " + prop.displayName + " has unknown Enum Value: '" + prop.floatValue + "' !\n"
								 + "It will be set to: '" + _values[index] + "'!");
					prop.floatValue = _values[index];
					Helper.SetShaderKeyWord(editor.targets, keyWords, index);
				}
			}

			Helper.AdaptiveFieldWidth(EditorStyles.popup, _names[index], EditorStyles.popup.lineHeight);
			int newIndex = EditorGUI.Popup(rect, label, index, _names);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = _values[newIndex];
				Helper.SetShaderKeyWord(editor.targets, keyWords, newIndex);
			}
			
			// set keyword for conditional display
			for (int i = 0; i < keyWords.Length; i++)
			{
				GroupStateHelper.SetKeywordConditionalDisplay(editor.target, keyWords[i], newIndex == i);
			}
		}

		public override void Apply(MaterialProperty prop)
		{
			base.Apply(prop);
			if (!prop.hasMixedValue && IsMatchPropType(prop))
				Helper.SetShaderKeyWord(prop.targets, GetKeywords(prop), (int)prop.floatValue);
		}
	}

	internal class SubEnumDrawer : KWEnumDrawer
	{
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2)
			: base(group, new []{n1, n2}, null, new []{v1, v2}){ }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3)
			: base(group, new []{n1, n2, n3}, null, new []{v1, v2, v3}){ }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4)
			: base(group, new []{n1, n2, n3, n4}, null, new []{v1, v2, v3, v4}){ }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5)
			: base(group, new []{n1, n2, n3, n4, n5}, null, new []{v1, v2, v3, v4, v5}){ }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6)
			: base(group, new []{n1, n2, n3, n4, n5, n6}, null, new []{v1, v2, v3, v4, v5, v6}){ }
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7)
			: base(group, new []{n1, n2, n3, n4, n5, n6, n7}, null, new []{v1, v2, v3, v4, v5, v6, v7}){ }

		protected override string GetKeywordName(string propName, string name) { return "_"; }
	}

	internal class SubKeywordEnumDrawer : KWEnumDrawer
	{
		public SubKeywordEnumDrawer(string group, string kw1, string kw2)
			: base(group, new []{kw1, kw2}, new []{kw1, kw2}) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3)
			: base(group, new []{kw1, kw2, kw3}, new []{kw1, kw2, kw3}) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4)
			: base(group, new []{kw1, kw2, kw3, kw4}, new []{kw1, kw2, kw3, kw4}) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5)
			: base(group, new []{kw1, kw2, kw3, kw4, kw5}, new []{kw1, kw2, kw3, kw4, kw5}) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6)
			: base(group, new []{kw1, kw2, kw3, kw4, kw5, kw6}, new []{kw1, kw2, kw3, kw4, kw5, kw6}) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7)
			: base(group, new []{kw1, kw2, kw3, kw4, kw5, kw6, kw7}, new []{kw1, kw2, kw3, kw4, kw5, kw6, kw7}) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8)
			: base(group, new []{kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8}, new []{kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8}) { }
		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9)
			: base(group, new []{kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9}, new []{kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9}) { }
		protected override string GetKeywordName(string propName, string name) { return (propName + "_" + name).Replace(' ', '_').ToUpperInvariant(); }

	}

	/// <summary>
	/// Draw a Texture property in single line with a extra property
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// extraPropName: extra property name (Unity 2019.2+ only) (Default: none)
	/// Target Property Type: Texture
	/// Extra Property Type: Any, except Texture
	/// </summary>
	internal class TexDrawer : SubDrawer
	{
		private string        _extraPropName = String.Empty;
		private ChannelDrawer _channelDrawer = new ChannelDrawer("_");

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight; }

		public TexDrawer() { }

		public TexDrawer(string group) : this(group, String.Empty) { }

		public TexDrawer(string group, string extraPropName)
		{
			this.group = group;
			this._extraPropName = extraPropName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Texture; }

		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			MaterialProperty extraProp = LWGUI.FindProp(_extraPropName, props, true);
			MetaDataHelper.RegisterSubProp(shader, prop, group, extraProp == null ? null : new []{extraProp});
			if (extraProp != null)
			{
				var text = string.Empty;
				if (extraProp.type == MaterialProperty.PropType.Vector)
					text = ChannelDrawer.GetChannelName(extraProp);
				else
					text = RevertableHelper.GetPropertyDefaultValueText(shader, extraProp);
				
				MetaDataHelper.RegisterPropertyDefaultValueText(shader, prop, 
																RevertableHelper.GetPropertyDefaultValueText(shader, prop) + ", " + text);
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.showMixedValue = prop.hasMixedValue;
			var rect = position; //EditorGUILayout.GetControlRect();
			var texLabel = label.text;

			MaterialProperty extraProp = LWGUI.FindProp(_extraPropName, props, true);
			if (extraProp != null && extraProp.type != MaterialProperty.PropType.Texture)
			{
				var i = EditorGUI.indentLevel;
				Rect indentedRect, extraPropRect = new Rect(rect);
				switch (extraProp.type)
				{
#if UNITY_2021_1_OR_NEWER
					case MaterialProperty.PropType.Int:
#endif
					case MaterialProperty.PropType.Color:
					case MaterialProperty.PropType.Float:
					case MaterialProperty.PropType.Vector:
						texLabel = string.Empty;
						indentedRect = EditorGUI.IndentedRect(extraPropRect);
						RevertableHelper.SetRevertableGUIWidths();
						EditorGUIUtility.labelWidth -= (indentedRect.xMin - extraPropRect.xMin) + 30f;
						extraPropRect = indentedRect;
						extraPropRect.xMin += 30f;
						EditorGUI.indentLevel = 0;
						break;
					case MaterialProperty.PropType.Range:
						label.text = string.Empty;
						indentedRect = EditorGUI.IndentedRect(extraPropRect);
						editor.SetDefaultGUIWidths();
						EditorGUIUtility.fieldWidth += 1f;
						EditorGUIUtility.labelWidth = 0;
						EditorGUI.indentLevel = 0;
						extraPropRect = MaterialEditor.GetRectAfterLabelWidth(extraPropRect);
						extraPropRect.xMin += 2;
						break;
				}

				if (extraProp.type == MaterialProperty.PropType.Vector)
					_channelDrawer.DrawProp(extraPropRect, extraProp, label, editor);
				else
					editor.ShaderProperty(extraPropRect, extraProp, label);
				
				EditorGUI.indentLevel = i;

				var revertButtonRect = RevertableHelper.GetRevertButtonRect(extraProp, position, true);
				if (RevertableHelper.IsPropertyShouldRevert(editor.target, prop.name) ||
					RevertableHelper.DrawRevertableProperty(revertButtonRect, extraProp, editor, shader))
				{
					RevertableHelper.SetPropertyToDefault(shader, prop);
					RevertableHelper.SetPropertyToDefault(shader, extraProp);
					RevertableHelper.RemovePropertyShouldRevert(editor.targets, prop.name);
				}
			}
			
			editor.TexturePropertyMiniThumbnail(rect, prop, texLabel, label.tooltip);

			EditorGUI.showMixedValue = false;
		}
	}
	
	/// <summary>
	/// Display up to 4 colors in a single line
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// color2-4: extra color property name (Unity 2019.2+ only)
	/// Target Property Type: Color
	/// </summary>
	internal class ColorDrawer : SubDrawer
	{
		private string[] _colorStrings = new string[3];
		
		public ColorDrawer(string group, string color2) : this(group, color2, String.Empty, String.Empty) { }
		
		public ColorDrawer(string group, string color2, string color3) : this(group, color2, color3, String.Empty) { }
		
		public ColorDrawer(string group, string color2, string color3, string color4)
		{
			this.group = group;
			this._colorStrings[0] = color2;
			this._colorStrings[1] = color3;
			this._colorStrings[2] = color4;
		}
		
		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Color; }

		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var extraColorProps = new List<MaterialProperty>();
			foreach (var extraColorProp in _colorStrings)
			{
				var p = LWGUI.FindProp(extraColorProp, props);
				if (p != null && IsMatchPropType(p))
					extraColorProps.Add(p);
			}
			MetaDataHelper.RegisterSubProp(shader, prop, group, extraColorProps.ToArray());
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			Stack<MaterialProperty> cProps = new Stack<MaterialProperty>();
			for (int i = 0; i < 4; i++)
			{
				if (i == 0)
				{
					cProps.Push(prop);
					continue;
				}

				var p = LWGUI.FindProp(_colorStrings[i - 1], props);
				if (p != null && IsMatchPropType(p))
					cProps.Push(p);
			}

			int count = cProps.Count;
			var colorArray = cProps.ToArray();
			var rect = position; //EditorGUILayout.GetControlRect();

			EditorGUI.PrefixLabel(rect, label);

			for (int i = 0; i < count; i++)
			{
				var cProp = colorArray[i];
				EditorGUI.showMixedValue = cProp.hasMixedValue;
				Rect r = new Rect(rect);
				var interval = 13 * i * (-0.25f + EditorGUI.indentLevel * 1.25f);
				float w = EditorGUIUtility.fieldWidth * (0.8f + EditorGUI.indentLevel * 0.2f);
				r.xMin += r.width - w * (i + 1) + interval;
				r.xMax -= w * i - interval;

				EditorGUI.BeginChangeCheck();
				Color src, dst;
				src = cProp.colorValue;
				var isHdr = (prop.flags & MaterialProperty.PropFlags.HDR) != MaterialProperty.PropFlags.None;
				dst = EditorGUI.ColorField(r, GUIContent.none, src, true, true, isHdr);
				if (EditorGUI.EndChangeCheck())
				{
					cProp.colorValue = dst;
				}
			}

			var revertButtonRect = RevertableHelper.GetRevertButtonRect(prop, position, true);
			bool[] shouldRevert = new bool[count];
			shouldRevert[count - 1] = RevertableHelper.IsPropertyShouldRevert(editor.target, prop.name);
			for (int i = 0; i < shouldRevert.Length - 1; i++)
			{
				shouldRevert[i] = RevertableHelper.DrawRevertableProperty(revertButtonRect, colorArray[i], editor, shader);
			}

			if (shouldRevert.Contains(true))
			{
				if (shouldRevert[count - 1])
					RevertableHelper.RemovePropertyShouldRevert(editor.targets, prop.name);
				for (int i = 0; i < count; i++)
				{
					RevertableHelper.SetPropertyToDefault(shader, colorArray[i]);
				}
			}

			EditorGUI.showMixedValue = false;
		}
	}

	/// <summary>
	/// Draw a Ramp Map Editor (Defaulf Ramp Map Resolution: 512 * 2)
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// defaultFileName: default Ramp Map file name when create a new one (Default: RampMap)
	/// defaultWidth: default Ramp Width (Default: 512)
	/// Target Property Type: Texture2D
	/// </summary>
	internal class RampDrawer : SubDrawer
	{
		private string _defaultFileName;
		private float  _defaultWidth;
		private float  _defaultHeight = 2;
		private bool   _isDirty;

		// used to read/write Gradient value in code
		private RampHelper.GradientObject _gradientObject;
		// used to modify Gradient value for users
		private SerializedObject _serializedObject;
		
		private static readonly GUIContent _iconMixImage = EditorGUIUtility.IconContent("darkviewbackground");

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight * 2f; }

		public RampDrawer() : this(String.Empty) { }
		public RampDrawer(string group) : this(group, "RampMap") { }
		public RampDrawer(string group, string defaultFileName) : this(group, defaultFileName, 512) { }

		public RampDrawer(string group, string defaultFileName, float defaultWidth)
		{
			this.group = group;
			this._defaultFileName = defaultFileName;
			this._defaultWidth = Mathf.Max(2.0f, defaultWidth);
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Texture; }

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// TODO: cache these variables between different prop?
			_gradientObject = ScriptableObject.CreateInstance<RampHelper.GradientObject>();
			_gradientObject.gradient = RampHelper.GetGradientFromTexture(prop.textureValue, out _isDirty);
			_serializedObject = new SerializedObject(_gradientObject);

			// Draw Label
			var labelRect = new Rect(position);//EditorGUILayout.GetControlRect();
			labelRect.yMax -= position.height * 0.5f;
			EditorGUI.PrefixLabel(labelRect, label);

			// Ramp buttons Rect
			var labelWidth = EditorGUIUtility.labelWidth;
			var indentLevel = EditorGUI.indentLevel;
			EditorGUIUtility.labelWidth = 0;
			EditorGUI.indentLevel = 0;
			var buttonRect = new Rect(position);//EditorGUILayout.GetControlRect();
			buttonRect.yMin += position.height * 0.5f;
			buttonRect = MaterialEditor.GetRectAfterLabelWidth(buttonRect);
			if (buttonRect.width < 50f) return;
				
			// Draw Ramp Editor
			bool hasChange, doSave, doDiscard;
			Texture newUserTexture, newCreatedTexture;
			hasChange = RampHelper.RampEditor(prop, buttonRect, _serializedObject.FindProperty("gradient"), _isDirty,
											  _defaultFileName, (int)_defaultWidth, (int)_defaultHeight,
											  out newCreatedTexture, out doSave, out doDiscard);

			if (hasChange || doSave)
			{
				// TODO: undo support
				// Undo.RecordObject(_gradientObject, "Edit Gradient");
				_serializedObject.ApplyModifiedProperties();
				RampHelper.SetGradientToTexture(prop.textureValue, _gradientObject, doSave);
				// EditorUtility.SetDirty(_gradientObject);
			}

			// Texture object field
			var textureRect = MaterialEditor.GetRectAfterLabelWidth(labelRect);
			newUserTexture = (Texture)EditorGUI.ObjectField(textureRect, prop.textureValue, typeof(Texture2D), false);
			
			// When tex has changed, update vars
			if (newUserTexture != prop.textureValue || newCreatedTexture != null || doDiscard)
			{
				if (newUserTexture != prop.textureValue)
					prop.textureValue = newUserTexture;
				if (newCreatedTexture != null)
					prop.textureValue = newCreatedTexture;
				_gradientObject.gradient = RampHelper.GetGradientFromTexture(prop.textureValue, out _isDirty, doDiscard);
				_serializedObject.Update();
				if (doDiscard)
					RampHelper.SetGradientToTexture(prop.textureValue, _gradientObject, true);
			}

			// Preview texture override (larger preview, hides texture name)
			var previewRect = new Rect(textureRect.x + 1, textureRect.y + 1, textureRect.width - 19, textureRect.height - 2);
			if (prop.hasMixedValue)
			{
				EditorGUI.DrawPreviewTexture(previewRect, _iconMixImage.image);
				GUI.Label(new Rect(previewRect.x + previewRect.width * 0.5f - 10, previewRect.y, previewRect.width * 0.5f, previewRect.height), "―");
			}
			else if (prop.textureValue != null)
				EditorGUI.DrawPreviewTexture(previewRect, prop.textureValue);
			
			EditorGUIUtility.labelWidth = labelWidth;
			EditorGUI.indentLevel = indentLevel;
		}
	}

	/// <summary>
	/// Draw a min max slider (Unity 2019.2+ only)
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// minPropName: Output Min Property Name
	/// maxPropName: Output Max Property Name
	/// Target Property Type: Range, range limits express the MinMaxSlider value range
	/// Output Min/Max Property Type: Range, it's value is limited by it's range
	/// </summary>
	internal class MinMaxSliderDrawer : SubDrawer
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

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Range; }

		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var minProp = LWGUI.FindProp(_minPropName, props, true);
			var maxProp = LWGUI.FindProp(_maxPropName, props, true);
			MetaDataHelper.RegisterSubProp(shader, prop, group, new []{ minProp, maxProp });
			MetaDataHelper.RegisterPropertyDefaultValueText(shader, prop,
															RevertableHelper.GetDefaultProperty(shader, minProp).floatValue + " - " + 
															RevertableHelper.GetDefaultProperty(shader, maxProp).floatValue);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			// read min max
			MaterialProperty min = LWGUI.FindProp(_minPropName, props, true);
			MaterialProperty max = LWGUI.FindProp(_maxPropName, props, true);
			if (min == null || max == null)
			{
				Debug.LogError("MinMaxSliderDrawer: minProp: " + (min == null ? "null" : min.name) + " or maxProp: " + (max == null ? "null" : max.name) + " not found!");
				return;
			}
			float minf = min.floatValue;
			float maxf = max.floatValue;

			// define draw area
			Rect controlRect = position; //EditorGUILayout.GetControlRect(); // this is the full length rect area
			var w = EditorGUIUtility.labelWidth;
			EditorGUIUtility.labelWidth = 0;
			Rect inputRect = MaterialEditor.GetRectAfterLabelWidth(controlRect); // this is the remaining rect area after label's area
			EditorGUIUtility.labelWidth = w;

			// draw label
			EditorGUI.LabelField(controlRect, label);

			// draw min max slider
			Rect[] splittedRect = Helper.SplitRect(inputRect, 3);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = min.hasMixedValue;
			var newMinf = EditorGUI.FloatField(splittedRect[0], minf);
			if (EditorGUI.EndChangeCheck())
			{
				minf = Mathf.Clamp(newMinf, min.rangeLimits.x, min.rangeLimits.y);
				min.floatValue = minf;
			}
			
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = max.hasMixedValue;
			var newMaxf = EditorGUI.FloatField(splittedRect[2], maxf);
			if (EditorGUI.EndChangeCheck())
			{
				maxf = Mathf.Clamp(newMaxf, max.rangeLimits.x, max.rangeLimits.y);
				max.floatValue = maxf;
			}

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			if (splittedRect[1].width > 50f)
				EditorGUI.MinMaxSlider(splittedRect[1], ref minf, ref maxf, prop.rangeLimits.x, prop.rangeLimits.y);
			EditorGUI.showMixedValue = false;

			// write back min max if changed
			if (EditorGUI.EndChangeCheck())
			{
				min.floatValue = Mathf.Clamp(minf, min.rangeLimits.x, min.rangeLimits.y);
				max.floatValue = Mathf.Clamp(maxf, max.rangeLimits.x, max.rangeLimits.y);
			}

			var revertButtonRect = RevertableHelper.GetRevertButtonRect(prop, position, true);
			if (RevertableHelper.DrawRevertableProperty(revertButtonRect, min, editor, shader) ||
				RevertableHelper.DrawRevertableProperty(revertButtonRect, max, editor, shader))
			{
				RevertableHelper.SetPropertyToDefault(shader, min);
				RevertableHelper.SetPropertyToDefault(shader, max);
			}

		}
	}

	/// <summary>
	/// Draw a R/G/B/A drop menu
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// Target Property Type: Vector, used to dot() with Texture Sample Value 
	/// </summary>
	internal class ChannelDrawer : SubDrawer
	{
		private static GUIContent[] _names  = new[] { new GUIContent("R"), new GUIContent("G"), new GUIContent("B"), new GUIContent("A"),
			new GUIContent("RGB Average"), new GUIContent("RGB Luminance") };
		private static int[]     _intValues     = new int[] { 0, 1, 2, 3, 4, 5 };
		private static Vector4[] _vector4Values = new[]
		{
			new Vector4(1, 0, 0, 0),
			new Vector4(0, 1, 0, 0),
			new Vector4(0, 0, 1, 0),
			new Vector4(0, 0, 0, 1),
			new Vector4(1f / 3f, 1f / 3f, 1f / 3f, 0),
			new Vector4(0.2126f, 0.7152f, 0.0722f, 0)
		};

		public ChannelDrawer() { }
		public ChannelDrawer(string group)
		{
			this.group = group;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Vector; }

		private static int GetChannelIndex(MaterialProperty prop)
		{
			int index;
			if (prop.vectorValue == _vector4Values[0])
				index = 0;
			else if (prop.vectorValue == _vector4Values[1])
				index = 1;
			else if (prop.vectorValue == _vector4Values[2])
				index = 2;
			else if (prop.vectorValue == _vector4Values[3])
				index = 3;
			else if (prop.vectorValue == _vector4Values[4])
				index = 4;
			else if (prop.vectorValue == _vector4Values[5])
				index = 5;
			else
			{
				Debug.LogError($"Channel Property:{prop.name} invalid vector found, reset to A");
				prop.vectorValue = _vector4Values[3];
				index = 3;
			}
			return index;
		}

		public static string GetChannelName(MaterialProperty prop)
		{
			return _names[GetChannelIndex(prop)].text;
		}

		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			base.Init(position, prop, label, editor);
			MetaDataHelper.RegisterPropertyDefaultValueText(shader, prop, GetChannelName(RevertableHelper.GetDefaultProperty(shader, prop)));
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var rect = position; //EditorGUILayout.GetControlRect();
			var index = GetChannelIndex(prop);

			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
			int num = EditorGUI.IntPopup(rect, label, index, _names, _intValues);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.vectorValue = _vector4Values[num];
			}
		}
	}

	/// <summary>
	/// Popping a menu, you can select the Shader Property Preset, the Preset values will replaces the default values
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	///	presetFileName: "Shader Property Preset" asset name, you can create new Preset by
	///		"Right Click > Create > LWGUI > Shader Property Preset" in Project window,
	///		*any Preset in the entire project cannot have the same name*
	/// </summary>
	internal class PresetDrawer : SubDrawer
	{
		public string presetFileName;
		public PresetDrawer(string presetFileName) : this("_", presetFileName) {}
		public PresetDrawer(string group, string presetFileName)
		{
			this.group = group;
			this.presetFileName = presetFileName;
		}

		protected override bool IsMatchPropType(MaterialProperty property) { return property.type == MaterialProperty.PropType.Float; }

		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			base.Init(position, prop, label, editor);
			var preset = PresetHelper.GetPreset(presetFileName);
			if (preset == null) return;
			
			var presetNames = preset.presets.Select(((inPreset) => (inPreset.presetName))).ToArray();
			var index = (int)RevertableHelper.GetDefaultProperty(shader, prop).floatValue;
			if (index < presetNames.Length && index >= 0)
				MetaDataHelper.RegisterPropertyDefaultValueText(shader, prop, presetNames[index]);
			index = (int)prop.floatValue;
			if (index < presetNames.Length && index >= 0)
				MetaDataHelper.RegisterPropertyPreset(shader, prop, presetFileName, presetNames[index]);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			EditorGUI.BeginChangeCheck();
			EditorGUI.showMixedValue = prop.hasMixedValue;
        	
			var rect = position;

			int index = (int)Mathf.Max(0, prop.floatValue);
			var preset = PresetHelper.GetPreset(presetFileName);
			if (preset == null || preset.presets.Count == 0)
			{
				var c = GUI.color;
				GUI.color = Color.red;
				label.text += $"  (Invalid Preset File: {presetFileName})";
				EditorGUI.LabelField(rect, label);
				GUI.color = c;
				return;
			}
			
			var presetNames = preset.presets.Select(((inPreset) => new GUIContent(inPreset.presetName))).ToArray();
			Helper.AdaptiveFieldWidth(EditorStyles.popup, presetNames[index], EditorStyles.popup.lineHeight);
			int newIndex = EditorGUI.Popup(rect, label, index, presetNames);
			EditorGUI.showMixedValue = false;
			if (EditorGUI.EndChangeCheck())
			{
				prop.floatValue = newIndex;
				preset.Apply(prop.targets.Select((o => o as Material)).ToArray(), (int)prop.floatValue);
				RevertableHelper.ForceInit();
			}

			if (RevertableHelper.IsPropertyShouldRevert(prop.targets[0], prop.name))
			{
				preset.Apply(prop.targets.Select((o => o as Material)).ToArray(), (int)prop.floatValue);
				RevertableHelper.ForceInit();
				RevertableHelper.RemovePropertyShouldRevert(prop.targets, prop.name);
			}
		}
	}
	
	/// <summary>
	/// Similar to Header()
	/// group：father group name, support suffix keyword for conditional display (Default: none)
	/// header: string to display, "SpaceLine" or "_" = none (Default: none)
	/// tips: Modifying the Decorator parameters in Shader requires manually refreshing the GUI instance by throwing an exception
	/// </summary>
	internal class TitleDecorator : SubDrawer
	{
		private string _header;

		protected override float GetVisibleHeight(MaterialProperty prop) { return EditorGUIUtility.singleLineHeight + 6f; }

		public TitleDecorator(string header) : this("_", header) {}
		public TitleDecorator(string group, string header)
		{
			this.group = group;
			this._header = header == "SpaceLine" || header == "_" ? String.Empty : header;
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			position.y += 2;
			position = EditorGUI.IndentedRect(position);
			GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
			GUI.Label(position, _header, style);
		}
	}

	/// <summary>
	/// Tooltip, describes the details of the property. (Default: property.name and property default value)
	/// You can also use "#Text" in DisplayName to add Tooltip that supports Multi-Language.
	/// tooltip：a single-line string to display, support up to 4 ','. (Default: Newline)
	/// tips: Modifying Decorator parameters in Shader requires refreshing the cache by modifying the Property default value
	/// </summary>
	internal class TooltipDecorator : SubDrawer
	{
		private string _tooltip;

		public TooltipDecorator() { }
		
		public TooltipDecorator(string s1, string s2) : this(s1 + ", " + s2) { }
		
		public TooltipDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }
		
		public TooltipDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }
		
		public TooltipDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }
		
		public TooltipDecorator(string tooltip) { this._tooltip = tooltip; }

		protected override float GetVisibleHeight(MaterialProperty prop) { return 0; }
		
		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			MetaDataHelper.RegisterPropertyTooltip(shader, prop, _tooltip);
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor) { }
	}

	/// <summary>
	/// Display a Helpbox on the property
	/// You can also use "%Text" in DisplayName to add Helpbox that supports Multi-Language.
	/// message：a single-line string to display, support up to 4 ','. (Default: Newline)
	/// tips: Modifying Decorator parameters in Shader requires refreshing the cache by modifying the Property default value
	/// </summary>
	internal class HelpboxDecorator : TooltipDecorator
	{
		private string _message;

		public HelpboxDecorator() { }
		
		public HelpboxDecorator(string s1, string s2) : this(s1 + ", " + s2) { }
		
		public HelpboxDecorator(string s1, string s2, string s3) : this(s1 + ", " + s2 + ", " + s3) { }
		
		public HelpboxDecorator(string s1, string s2, string s3, string s4) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4) { }
		
		public HelpboxDecorator(string s1, string s2, string s3, string s4, string s5) : this(s1 + ", " + s2 + ", " + s3 + ", " + s4 + ", " + s5) { }

		public HelpboxDecorator(string message) { this._message = message; }

		public override void Init(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			MetaDataHelper.RegisterPropertyHelpbox(shader, prop, _message);
			
			// To resolve such errors:
			// ArgumentException: Getting control 26's position in a group with only 26 controls when doing repaint
			{
				// When the Drawer draws in the Repaint stage but does not draw in the Init stage, an error will occur.
				// It is necessary to ensure that the same number of GUIs are drawn in different stages
				EditorGUI.HelpBox(EditorGUILayout.GetControlRect(), "", MessageType.None);
			}
		}
	}

} //namespace LWGUI