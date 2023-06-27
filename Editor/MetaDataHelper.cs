using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Provide Metadata for drawing
	/// </summary>
	public class MetaDataHelper
	{
		#region Meta Data Container

		private static Dictionary<Shader, Dictionary<string /*MainProp*/, List<string /*SubProp*/>>> _mainSubDic       = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*GroupName*/, string /*MainProp*/>>     _mainGroupNameDic = new Dictionary<Shader, Dictionary<string, string>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/, string /*GroupName*/>>     _propParentDic    = new Dictionary<Shader, Dictionary<string, string>>();

		private static Dictionary<Shader, Dictionary<string /*PropName*/, List<string /*ExtraPropName*/>>>        _extraPropDic = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/, List<string /*Tooltip*/>>>              _tooltipDic = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/, List<string /*DefaultValue*/>>>         _defaultDic = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/, List<string /*Helpbox*/>>>              _helpboxDic = new Dictionary<Shader, Dictionary<string, List<string>>>();
		private static Dictionary<Shader, Dictionary<string /*PropName*/, List<ShaderPropertyPreset /*Preset*/>>> _presetDic = new Dictionary<Shader, Dictionary<string, List<ShaderPropertyPreset>>>();

		public static void ClearCaches(Shader shader)
		{
			if (_mainSubDic.ContainsKey(shader)) _mainSubDic[shader].Clear();
			if (_mainGroupNameDic.ContainsKey(shader)) _mainGroupNameDic[shader].Clear();
			if (_propParentDic.ContainsKey(shader)) _propParentDic[shader].Clear();

			if (_extraPropDic.ContainsKey(shader)) _extraPropDic[shader].Clear();
			if (_tooltipDic.ContainsKey(shader)) _tooltipDic[shader].Clear();
			if (_defaultDic.ContainsKey(shader)) _defaultDic[shader].Clear();
			if (_helpboxDic.ContainsKey(shader)) _helpboxDic[shader].Clear();
			if (_presetDic.ContainsKey(shader)) _presetDic[shader].Clear();
		}

		#endregion


		#region Main - Sub

		public static void RegisterMainProp(Shader shader, MaterialProperty prop, string group)
		{
			if (_mainSubDic.ContainsKey(shader))
			{
				if (!_mainSubDic[shader].ContainsKey(prop.name))
				{
					_mainSubDic[shader].Add(prop.name, new List<string>());
				}
			}
			else
			{
				_mainSubDic.Add(shader, new Dictionary<string, List<string>>());
				_mainSubDic[shader].Add(prop.name, new List<string>());
			}

			if (_mainGroupNameDic.ContainsKey(shader))
			{
				if (!_mainGroupNameDic[shader].ContainsKey(group))
				{
					_mainGroupNameDic[shader].Add(group, prop.name);
				}
			}
			else
			{
				_mainGroupNameDic.Add(shader, new Dictionary<string, string>());
				_mainGroupNameDic[shader].Add(group, prop.name);
			}
		}

		public static void RegisterSubProp(Shader shader, MaterialProperty prop, string group, MaterialProperty[] extraProps = null)
		{
			if (!string.IsNullOrEmpty(group) && group != "_")
			{
				// add to _mainSubDic
				if (_mainGroupNameDic.ContainsKey(shader))
				{
					var groupName = _mainGroupNameDic[shader].Keys.First((s => group.Contains(s)));
					if (!string.IsNullOrEmpty(groupName))
					{
						var mainPropName = _mainGroupNameDic[shader][groupName];
						if (_mainSubDic[shader].ContainsKey(mainPropName))
						{
							if (!_mainSubDic[shader][mainPropName].Contains(prop.name))
								_mainSubDic[shader][mainPropName].Add(prop.name);
						}
						else
							Debug.LogError("Unregistered Main Property:" + mainPropName);

						// add to _propParentDic
						if (!_propParentDic.ContainsKey(shader))
							_propParentDic.Add(shader, new Dictionary<string, string>());
						if (!_propParentDic[shader].ContainsKey(prop.name))
							_propParentDic[shader].Add(prop.name, groupName);
					}
					else
						Debug.LogError("Unregistered Main Group Name: " + group);
				}
				else
					Debug.LogError("Unregistered Shader: " + shader.name);
			}

			// add to _extraPropDic
			if (extraProps != null)
			{
				if (!_extraPropDic.ContainsKey(shader))
					_extraPropDic.Add(shader, new Dictionary<string, List<string>>());
				if (!_extraPropDic[shader].ContainsKey(prop.name))
					_extraPropDic[shader].Add(prop.name, new List<string>());
				foreach (var extraProp in extraProps)
				{
					if (extraProp != null)
					{
						if (!_extraPropDic[shader][prop.name].Contains(extraProp.name))
							_extraPropDic[shader][prop.name].Add(extraProp.name);
					}
				}
			}
		}

		public static bool IsSubProperty(Shader shader, MaterialProperty prop)
		{
			var isSubProp = false;
			if (_propParentDic.ContainsKey(shader) && _propParentDic[shader].ContainsKey(prop.name))
				isSubProp = true;
			return isSubProp;
		}

		#endregion


		private static void RegisterProperty<T>(Shader shader, MaterialProperty prop, T value, Dictionary<Shader, Dictionary<string, List<T>>> dst)
		{
			if (!dst.ContainsKey(shader))
				dst.Add(shader, new Dictionary<string, List<T>>());
			if (!dst[shader].ContainsKey(prop.name))
				dst[shader].Add(prop.name, new List<T>());
			dst[shader][prop.name].Add(value);
		}

		private static string GetPropertyString(Shader shader, MaterialProperty prop, Dictionary<Shader, Dictionary<string, List<string>>> src, out int lineCount)
		{
			var str = string.Empty;
			lineCount = 0;
			if (src.ContainsKey(shader) && src[shader].ContainsKey(prop.name))
			{
				for (int i = 0; i < src[shader][prop.name].Count; i++)
				{
					if (i > 0) str += "\n";
					str += src[shader][prop.name][i];
					lineCount++;
				}
			}
			return str;
		}

		public static void ReregisterAllPropertyMetaData(Shader shader, Material material, MaterialProperty[] props)
		{
			foreach (var prop in props)
			{
				List<MaterialPropertyDrawer> decoratorDrawers;
				var drawer = ReflectionHelper.GetPropertyDrawer(shader, prop, out decoratorDrawers);
				if (decoratorDrawers != null && decoratorDrawers.Count > 0)
				{
					foreach (var decoratorDrawer in decoratorDrawers)
					{
						if (decoratorDrawer is IBaseDrawer)
							(decoratorDrawer as IBaseDrawer).InitMetaData(shader, material, prop, props);
					}
				}
				if (drawer != null)
				{
					if (drawer is IBaseDrawer)
						(drawer as IBaseDrawer).InitMetaData(shader, material, prop, props);
				}
				DisplayNameToTooltipAndHelpbox(shader, prop);
			}
		}


		#region Tooltip

		public static void RegisterPropertyDefaultValueText(Shader shader, MaterialProperty prop, string text)
		{
			RegisterProperty<string>(shader, prop, text, _defaultDic);
		}

		public static void RegisterPropertyTooltip(Shader shader, MaterialProperty prop, string text)
		{
			RegisterProperty<string>(shader, prop, text, _tooltipDic);
		}

		private static string GetPropertyDefaultValueText(Shader shader, Material material, MaterialProperty prop)
		{
			int lineCount;
			var defaultText = GetPropertyString(shader, prop, _defaultDic, out lineCount);
			if (string.IsNullOrEmpty(defaultText))
				// TODO: use Reflection - handle builtin Toggle / Enum
				defaultText = RevertableHelper.GetPropertyDefaultValueText(material, prop);

			return defaultText;
		}

		public static string GetPropertyTooltip(Shader shader, Material material, MaterialProperty prop)
		{
			int lineCount;
			var str = GetPropertyString(shader, prop, _tooltipDic, out lineCount);
			if (!string.IsNullOrEmpty(str))
				str += "\n\n";
			str += "Name: " + prop.name + "\n";
			str += "Default: " + GetPropertyDefaultValueText(shader, material, prop);
			return str;
		}

		#endregion


		#region Helpbox

		public static void RegisterPropertyHelpbox(Shader shader, MaterialProperty prop, string text)
		{
			RegisterProperty(shader, prop, text, _helpboxDic);
		}

		public static string GetPropertyHelpbox(Shader shader, MaterialProperty prop, out int lineCount)
		{
			return GetPropertyString(shader, prop, _helpboxDic, out lineCount);
		}

		#endregion


		#region Display Name

		private static readonly string _tooltipString = "#";
		private static readonly string _helpboxString = "%";

		public static string GetPropertyDisplayName(Shader shader, MaterialProperty prop)
		{
			var tooltipIndex = prop.displayName.IndexOf(_tooltipString, StringComparison.Ordinal);
			var helpboxIndex = prop.displayName.IndexOf(_helpboxString, StringComparison.Ordinal);
			var minIndex = tooltipIndex == -1 ? helpboxIndex : tooltipIndex;
			if (tooltipIndex != -1 && helpboxIndex != -1)
				minIndex = Mathf.Min(minIndex, helpboxIndex);
			if (minIndex == -1)
				return prop.displayName;
			else if (minIndex == 0)
				return string.Empty;
			else
				return prop.displayName.Substring(0, minIndex);
		}

		public static void DisplayNameToTooltipAndHelpbox(Shader shader, MaterialProperty prop)
		{
			var tooltips = prop.displayName.Split(new String[] { _tooltipString }, StringSplitOptions.None);
			if (tooltips.Length > 1)
			{
				for (int i = 1; i <= tooltips.Length - 1; i++)
				{
					var str = tooltips[i];
					var helpboxIndex = tooltips[i].IndexOf(_helpboxString, StringComparison.Ordinal);
					if (helpboxIndex == 0)
						str = "\n";
					else if (helpboxIndex > 0)
						str = tooltips[i].Substring(0, helpboxIndex);
					RegisterPropertyTooltip(shader, prop, str);
				}
			}
			var helpboxes = prop.displayName.Split(new String[] { _helpboxString }, StringSplitOptions.None);
			if (helpboxes.Length > 1)
			{
				for (int i = 1; i <= helpboxes.Length - 1; i++)
				{
					var str = helpboxes[i];
					var tooltipIndex = helpboxes[i].IndexOf(_tooltipString, StringComparison.Ordinal);
					if (tooltipIndex == 0)
						str = "\n";
					else if (tooltipIndex > 0)
						str = tooltips[i].Substring(0, tooltipIndex);
					RegisterPropertyHelpbox(shader, prop, str);
				}
			}
		}

		#endregion


		#region Preset

		public static void RegisterPropertyPreset(Shader shader, MaterialProperty prop, ShaderPropertyPreset shaderPropertyPreset)
		{
			RegisterProperty<ShaderPropertyPreset>(shader, prop, shaderPropertyPreset, _presetDic);
		}

		public static Dictionary<MaterialProperty, ShaderPropertyPreset> GetAllPropertyPreset(Shader shader, MaterialProperty[] props)
		{
			var result = new Dictionary<MaterialProperty, ShaderPropertyPreset>();

			var presetProps = props.Where((property =>
											  _presetDic.ContainsKey(shader) && _presetDic[shader].ContainsKey(property.name)));
			foreach (var presetProp in presetProps)
			{
				result.Add(presetProp, _presetDic[shader][presetProp.name][0]);
			}
			return result;
		}

		#endregion


		public static Dictionary<string, bool> SearchProperties(Shader shader, Material material, MaterialProperty[] props, string searchingText, SearchMode searchMode)
		{
			var result = new Dictionary<string, bool>();
			var isDefaultProps = new Dictionary<string, bool>();

			if (searchMode == SearchMode.Modified)
			{
				foreach (var prop in props)
				{
					isDefaultProps.Add(prop.name, RevertableHelper.IsDefaultProperty(material, prop));
				}
			}

			if (string.IsNullOrEmpty(searchingText) && searchMode == SearchMode.All)
			{
				foreach (var prop in props)
				{
					result.Add(prop.name, true);
				}
			}
			else
			{
				foreach (var prop in props)
				{
					bool contains = true;

					// filter props
					if (searchMode == SearchMode.Modified)
					{
						contains = !isDefaultProps[prop.name];
						if (!contains && _extraPropDic.ContainsKey(shader) && _extraPropDic[shader].ContainsKey(prop.name))
						{
							foreach (var extraPropName in _extraPropDic[shader][prop.name])
							{
								contains = !isDefaultProps[extraPropName];
								if (contains) break;
							}
						}
					}

					// whole word match search
					var displayName = GetPropertyDisplayName(shader, prop).ToLower();
					var name = prop.name.ToLower();
					searchingText = searchingText.ToLower();

					var keywords = searchingText.Split(' ', ',', ';', '|', '*', '&'); // Some possible separators

					foreach (var keyword in keywords)
					{
						var isMatch = false;
						isMatch |= displayName.Contains(keyword);
						isMatch |= name.Contains(keyword);
						contains &= isMatch;
					}

					result.Add(prop.name, contains);
				}

				// when a SubProp display, MainProp will also display
				if (_mainSubDic.ContainsKey(shader))
				{
					foreach (var prop in props)
					{
						if (_mainSubDic[shader].ContainsKey(prop.name))
						{
							// foreach sub prop in main
							foreach (var subPropName in _mainSubDic[shader][prop.name])
							{
								if (result.ContainsKey(subPropName))
								{
									if (result[subPropName])
									{
										result[prop.name] = true;
										break;
									}
								}
							}
						}
					}
				}
			}

			return result;
		}
	}
}