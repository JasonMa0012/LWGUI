// Copyright (c) Jason Ma
// LWGUI - Amplify Shader Editor Extension Support
// This file provides core functionality for ASE integration without depending on ASE itself

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.Rendering;

namespace LWGUI
{
	#region Enums

	/// <summary>
	/// Parameter types for LWGUI Drawer/Decorator parameters
	/// </summary>
	public enum LwguiParameterType
	{
		/// <summary>Float number</summary>
		Float,
		/// <summary>String text</summary>
		String,
		/// <summary>Selection from predefined options</summary>
		Enum,
		/// <summary>Boolean value (True/False)</summary>
		Bool,
		/// <summary>Integer number</summary>
		Int,
		/// <summary>Shader keyword (uppercase, alphanumeric and underscore only)</summary>
		Keyword,
		/// <summary>Reference to another shader property name</summary>
		PropertyName,
		/// <summary>Reference to a pass LightMode tag</summary>
		PassName
	}

	#endregion

	#region Data Classes

	/// <summary>
	/// Represents a named parameter value for stable data migration between constructors
	/// </summary>
	[Serializable]
	public class NamedParameterValue
	{
		public string name;
		public string value;
	}

	/// <summary>
	/// Represents a single LWGUI Drawer or Decorator attribute with its parameters
	/// Can be serialized by Unity and used by any editor integration
	/// </summary>
	[Serializable]
	public class LwguiAttributeData
	{
		[SerializeField]
		private string _drawerTypeName;

		[SerializeField]
		private List<string> _parameters = new List<string>();

		[SerializeField]
		private bool _isDecorator;

		/// <summary>
		/// Full signature string of the selected constructor for stable identification
		/// </summary>
		[SerializeField]
		private string _constructorSignature = "";

		/// <summary>
		/// Named parameter values for stable data migration
		/// </summary>
		[SerializeField]
		private List<NamedParameterValue> _namedParameters = new List<NamedParameterValue>();

		/// <summary>
		/// Per-parameter validation error state: key = parameter index, value = true if error
		/// Not serialized by Unity (Dictionary is not serializable), rebuilt on demand.
		/// </summary>
		private Dictionary<int, bool> _validationErrors = new Dictionary<int, bool>();

		public string drawerTypeName
		{
			get => _drawerTypeName;
			set => _drawerTypeName = value;
		}

		public List<string> parameters => _parameters;

		public bool isDecorator
		{
			get => _isDecorator;
			set => _isDecorator = value;
		}

		/// <summary>
		/// Full signature string of the selected constructor
		/// </summary>
		public string constructorSignature
		{
			get => _constructorSignature;
			set => _constructorSignature = value ?? "";
		}

		/// <summary>
		/// Gets or sets the named parameter values
		/// </summary>
		public List<NamedParameterValue> namedParameters
		{
			get => _namedParameters;
			set => _namedParameters = value ?? new List<NamedParameterValue>();
		}

		/// <summary>
		/// Gets the validation errors dictionary
		/// </summary>
		public Dictionary<int, bool> validationErrors => _validationErrors;

		/// <summary>
		/// Clears all validation errors
		/// </summary>
		public void ClearValidationErrors()
		{
			_validationErrors.Clear();
		}

		/// <summary>
		/// Sets validation error state for a parameter
		/// </summary>
		public void SetValidationError(int paramIndex, bool hasError)
		{
			_validationErrors[paramIndex] = hasError;
		}

		/// <summary>
		/// Checks if a parameter has validation error
		/// </summary>
		public bool HasValidationError(int paramIndex)
		{
			return _validationErrors.ContainsKey(paramIndex) && _validationErrors[paramIndex];
		}

		/// <summary>
		/// Validates and repairs the data after deserialization.
		/// Ensures constructor signature resolves to a valid constructor.
		/// </summary>
		public void ValidateAndRepair()
		{
			if (string.IsNullOrEmpty(_drawerTypeName))
				return;

			var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(_drawerTypeName);
			if (drawerInfo == null)
				return;

			var constructor = drawerInfo.GetConstructorBySignature(_constructorSignature);
			if (constructor == null)
				constructor = drawerInfo.GetMainConstructor();

			if (constructor != null)
			{
				_constructorSignature = constructor.GetFullSignature();
				SyncParameters(constructor);
			}
		}

		public LwguiConstructorInfo GetCurrentConstructor()
		{
			if (string.IsNullOrEmpty(_drawerTypeName))
				return null;

			var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(_drawerTypeName);
			if (drawerInfo == null)
				return null;

			if (string.IsNullOrEmpty(_constructorSignature))
				ValidateAndRepair();

			return drawerInfo.GetConstructorBySignature(_constructorSignature)
				?? drawerInfo.GetMainConstructor();
		}

		public void SwitchConstructor(string newSignature)
		{
			if (string.IsNullOrEmpty(_drawerTypeName))
				return;

			var drawerInfo = LwguiDrawerDiscovery.GetDrawerInfo(_drawerTypeName);
			if (drawerInfo == null)
				return;

			var newConstructor = drawerInfo.GetConstructorBySignature(newSignature);

			if (newConstructor == null)
				return;

			// Build new named parameters list, preserving values for matching names
			var newNamedParams = new List<NamedParameterValue>();

			foreach (var param in newConstructor.parameters)
			{
				// Try to find existing value with same name
				var existingParam = _namedParameters.FirstOrDefault(np => np.name == param.name);
				string value = existingParam?.value ?? "";

				newNamedParams.Add(new NamedParameterValue
				{
					name = param.name,
					value = value
				});
			}

			_namedParameters = newNamedParams;
			_constructorSignature = newSignature;

			// Sync to indexed parameters
			SyncParameters(newConstructor);
		}

		/// <summary>
		/// Sets a parameter value by name
		/// </summary>
		public void SetParameterValue(string paramName, string value)
		{
			value = SanitizeParameterValue(value);

			var param = _namedParameters.FirstOrDefault(np => np.name == paramName);
			if (param != null)
			{
				param.value = value;
			}
			else
			{
				_namedParameters.Add(new NamedParameterValue { name = paramName, value = value });
			}

			// Sync to indexed parameters
			var constructor = GetCurrentConstructor();
			SyncParameters(constructor);
		}

		public static string SanitizeParameterValue(string value)
		{
			if (string.IsNullOrEmpty(value))
				return value;

			for (int i = 0; i < value.Length; i++)
			{
				if (!IsAllowedSanitizedChar(value[i]))
				{
					var sb = new StringBuilder(value.Length);
					sb.Append(value, 0, i);
					for (; i < value.Length; i++)
					{
						char c = value[i];
						if (IsAllowedSanitizedChar(c))
							sb.Append(c);
					}
					return sb.ToString();
				}
			}

			return value;
		}

		private static bool IsAllowedSanitizedChar(char c)
		{
			return c is >= 'A' and <= 'Z' 
				or >= 'a' and <= 'z' 
				or >= '0' and <= '9' 
				or '_' 
				or '.' 
				or ' ' 
				or '(' 
				or ')' 
				or '-';
		}

		/// <summary>
		/// Gets a parameter value by name
		/// </summary>
		public string GetParameterValue(string paramName)
		{
			var param = _namedParameters.FirstOrDefault(np => np.name == paramName);
			return param?.value ?? "";
		}

		/// <summary>
		/// Syncs named parameters with indexed parameters list
		/// </summary>
		private void SyncParameters(LwguiConstructorInfo constructor)
		{
			if (constructor == null)
				return;

			_parameters.Clear();
			for (int i = 0; i < constructor.parameters.Count; i++)
			{
				var paramName = constructor.parameters[i].name;
				var namedParam = _namedParameters.FirstOrDefault(np => np.name == paramName);
				_parameters.Add(namedParam?.value ?? "");
			}
		}

		/// <summary>
		/// Generates the attribute string for shader code
		/// </summary>
		public string GenerateAttributeString()
		{
			if (string.IsNullOrEmpty(_drawerTypeName))
				return string.Empty;

			string attrName = _drawerTypeName;
			if (attrName.EndsWith("Drawer"))
				attrName = attrName.Substring(0, attrName.Length - 6);
			else if (attrName.EndsWith("Decorator"))
				attrName = attrName.Substring(0, attrName.Length - 9);

			var constructorInfo = GetCurrentConstructor();

			if (constructorInfo == null)
				return $"[{attrName}]";

			var processedParams = new List<string>();
			for (int i = 0; i < constructorInfo.parameters.Count; i++)
			{
				string paramValue = i < _parameters.Count ? _parameters[i] : "";
				var paramInfo = constructorInfo.parameters[i];

				if (string.IsNullOrEmpty(paramValue))
				{
					if (paramInfo != null && paramInfo.hasDefaultValue && !string.IsNullOrEmpty(paramInfo.defaultValue))
						paramValue = paramInfo.defaultValue;
					else
						paramValue = GetTypeDefaultValue(paramInfo?.parameterType ?? LwguiParameterType.String);
				}

				processedParams.Add(FormatParameterValue(paramValue, paramInfo?.parameterType ?? LwguiParameterType.String));
			}

			// Remove trailing optional parameters that match defaults
			while (processedParams.Count > 0)
			{
				int lastIndex = processedParams.Count - 1;
				var paramInfo = constructorInfo.parameters.ElementAtOrDefault(lastIndex);
				if (paramInfo != null && paramInfo.isOptional)
				{
					string defaultValue;
					if (paramInfo.hasDefaultValue && !string.IsNullOrEmpty(paramInfo.defaultValue))
						defaultValue = FormatParameterValue(paramInfo.defaultValue, paramInfo.parameterType);
					else
						defaultValue = FormatParameterValue(GetTypeDefaultValue(paramInfo.parameterType), paramInfo.parameterType);
					
					if (processedParams[lastIndex] == defaultValue)
					{
						processedParams.RemoveAt(lastIndex);
						continue;
					}
				}
				break;
			}

			if (processedParams.Count == 0)
				return $"[{attrName}]";

			return $"[{attrName}({string.Join(", ", processedParams)})]";
		}

		private static string GetTypeDefaultValue(LwguiParameterType type)
		{
			switch (type)
			{
				case LwguiParameterType.Float:
				case LwguiParameterType.Int:
					return "0";
				case LwguiParameterType.Bool:
					return "false";
				default:
					return "_";
			}
		}

		private static string FormatParameterValue(string value, LwguiParameterType type)
		{
			if (string.IsNullOrEmpty(value))
				return "_";
			return value;
		}
	}

	/// <summary>
	/// Information about a LWGUI Drawer/Decorator parameter
	/// </summary>
	public class LwguiParameterInfo
	{
		public string name;
		public LwguiParameterType parameterType;
		public List<string> enumOptions;
		public string defaultValue;
		public bool hasDefaultValue;
		public string displayText;
		public bool isOptional;

		public LwguiParameterInfo(string name, LwguiParameterType type, string defaultValue = null, string displayText = null, bool isOptional = false)
		{
			this.name = name;
			this.parameterType = type;
			this.defaultValue = defaultValue;
			this.displayText = displayText;
			this.isOptional = isOptional;
			this.hasDefaultValue = defaultValue != null;
			this.enumOptions = new List<string>();
		}

		public string GetDefaultDisplayText()
		{
			if (!string.IsNullOrEmpty(displayText))
				return displayText;
			
			if (!hasDefaultValue || string.IsNullOrEmpty(defaultValue))
			{
				switch (parameterType)
				{
					case LwguiParameterType.Float:
					case LwguiParameterType.Int:
						return "0";
					case LwguiParameterType.Bool:
						return "False";
					default:
						return "Empty";
				}
			}
			
			return defaultValue;
		}
	}

	/// <summary>
	/// Information about a specific constructor of a Drawer/Decorator
	/// </summary>
	public class LwguiConstructorInfo
	{
		/// <summary>Constructor index (0 = main constructor with most parameters)</summary>
		public int index;
		/// <summary>Display name for the constructor</summary>
		public string displayName;
		/// <summary>Parameters for this constructor</summary>
		public List<LwguiParameterInfo> parameters;
		/// <summary>Number of parameters</summary>
		public int parameterCount => parameters?.Count ?? 0;

		public LwguiConstructorInfo()
		{
			parameters = new List<LwguiParameterInfo>();
		}

		/// <summary>
		/// Generates a display name based on parameter signatures
		/// </summary>
		public string GenerateDisplayName()
		{
			if (parameters.Count == 0)
				return "()";
			
			var paramSignatures = parameters.Select(p =>
			{
				string typeName = p.parameterType.ToString().ToLower();
				return $"{typeName} {p.name}";
			});
			
			return $"({string.Join(", ", paramSignatures)})";
		}

		/// <summary>
		/// Gets the parameter signature string using full type + parameter names
		/// </summary>
		public string GetSignature()
		{
			if (parameters.Count == 0)
				return "";
			return string.Join(",", parameters.Select(p => p.name));
		}

		/// <summary>
		/// Full unique signature string including type info for stable cross-platform identification
		/// </summary>
		public string GetFullSignature()
		{
			if (parameters.Count == 0)
				return "";
			return string.Join(",", parameters.Select(p => p.parameterType.ToString().ToLower() + " " + p.name));
		}

		/// <summary>
		/// Checks if this constructor has a parameter with the given name and type
		/// </summary>
		public bool HasParameter(string name, LwguiParameterType type)
		{
			return parameters.Any(p => p.name == name && p.parameterType == type);
		}

		/// <summary>
		/// Gets the index of a parameter by name, or -1 if not found
		/// </summary>
		public int GetParameterIndex(string name)
		{
			return parameters.FindIndex(p => p.name == name);
		}
	}

	/// <summary>
	/// Information about a LWGUI Drawer/Decorator type
	/// </summary>
	public class LwguiDrawerInfo
	{
		public string typeName;
		public string displayName;
		public string description;
		public string categoryPath;
		public int order;
		public bool isDecorator;
		public Type drawerType;
		
		/// <summary>
		/// All available constructors for this drawer, ordered by parameter count (descending)
		/// Index 0 is always the constructor with most parameters
		/// </summary>
		public List<LwguiConstructorInfo> constructors;

		/// <summary>
		/// All unique parameter names across all constructors (for value collection)
		/// </summary>
		public List<string> allParameterNames;

		public LwguiDrawerInfo()
		{
			constructors = new List<LwguiConstructorInfo>();
			allParameterNames = new List<string>();
		}

		/// <summary>
		/// Gets a constructor by index (0 = main constructor with most parameters)
		/// </summary>
		public LwguiConstructorInfo GetConstructor(int index)
		{
			if (index < 0 || index >= constructors.Count)
				return constructors.FirstOrDefault();
			return constructors[index];
		}

		/// <summary>
		/// Gets a constructor by its full signature string
		/// </summary>
		public LwguiConstructorInfo GetConstructorBySignature(string signature)
		{
			if (string.IsNullOrEmpty(signature))
				return null;
			return constructors.FirstOrDefault(c => c.GetFullSignature() == signature);
		}

		/// <summary>
		/// Gets the main constructor (with most parameters)
		/// </summary>
		public LwguiConstructorInfo GetMainConstructor()
		{
			return constructors.FirstOrDefault();
		}

		public string GetMenuPath()
		{
			if (string.IsNullOrEmpty(categoryPath))
				return displayName;
			return $"{categoryPath}/{displayName}";
		}

		public bool IsSupportedPropertyType(ShaderPropertyType propType)
		{
			try
			{
				var instance = (SubDrawer)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(drawerType);
				return instance.IsMatchPropType(propType);
			}
			catch
			{
				return true;
			}
		}
	}

	#endregion

	#region Validation System

	/// <summary>
	/// Validation result containing error information
	/// </summary>
	public class ValidationResult
	{
		public bool isValid;
		public string errorMessage;
		public string parameterName;
		public int parameterIndex;

		public static ValidationResult Success() => new ValidationResult { isValid = true };
		public static ValidationResult Failed(string message, string paramName = null, int paramIndex = -1) => 
			new ValidationResult { isValid = false, errorMessage = message, parameterName = paramName, parameterIndex = paramIndex };
	}

	/// <summary>
	/// Context for parameter validation
	/// </summary>
	public class ValidationContext
	{
		/// <summary>Available shader property names from the saved shader</summary>
		public List<string> existingPropertyNames;
		/// <summary>Available pass LightMode names from the saved shader</summary>
		public List<string> existingPassNames;
		/// <summary>The shader being edited (if available)</summary>
		public UnityEngine.Object shaderAsset;

		public ValidationContext()
		{
			existingPropertyNames = new List<string>();
			existingPassNames = new List<string>();
		}
	}

	/// <summary>
	/// Static class for parameter validation
	/// </summary>
	public static class LwguiParameterValidator
	{
		/// <summary>
		/// Validates a single parameter value
		/// </summary>
		public static ValidationResult Validate(string value, LwguiParameterInfo paramInfo, ValidationContext context)
		{
			if (string.IsNullOrEmpty(value))
				return ValidationResult.Success(); // Empty values use defaults

			switch (paramInfo.parameterType)
			{
				case LwguiParameterType.Float:
					return ValidateFloat(value, paramInfo);
				case LwguiParameterType.Int:
					return ValidateInt(value, paramInfo);
				case LwguiParameterType.Bool:
					return ValidateBool(value, paramInfo);
				case LwguiParameterType.Keyword:
					return ValidateKeyword(value, paramInfo);
				case LwguiParameterType.Enum:
					return ValidateEnum(value, paramInfo);
				case LwguiParameterType.PropertyName:
					return ValidatePropertyName(value, paramInfo, context);
				case LwguiParameterType.PassName:
					return ValidatePassName(value, paramInfo, context);
				default:
					return ValidationResult.Success();
			}
		}

		private static ValidationResult ValidateFloat(string value, LwguiParameterInfo paramInfo)
		{
			if (!float.TryParse(value, out _))
				return ValidationResult.Failed($"'{value}' is not a valid float number", paramInfo.name);
			return ValidationResult.Success();
		}

		private static ValidationResult ValidateInt(string value, LwguiParameterInfo paramInfo)
		{
			if (!int.TryParse(value, out _))
				return ValidationResult.Failed($"'{value}' is not a valid integer", paramInfo.name);
			return ValidationResult.Success();
		}

		private static ValidationResult ValidateBool(string value, LwguiParameterInfo paramInfo)
		{
			string lowerValue = value.ToLower();
			string[] validValues = { "true", "false", "on", "off", "1", "0" };
			
			if (!validValues.Contains(lowerValue))
				return ValidationResult.Failed($"'{value}' is not a valid boolean. Use True, False, On, Off, 1, or 0", paramInfo.name);
			
			return ValidationResult.Success();
		}

		private static ValidationResult ValidateKeyword(string value, LwguiParameterInfo paramInfo)
		{
			// Check if value contains only uppercase letters, numbers, and underscores
			if (string.IsNullOrEmpty(value))
				return ValidationResult.Success();

			// Keywords should be uppercase
			if (value != value.ToUpper())
				return ValidationResult.Failed($"Keyword '{value}' must be all uppercase", paramInfo.name);

			// Check valid characters
			foreach (char c in value)
			{
				if (!char.IsUpper(c) && !char.IsDigit(c) && c != '_')
					return ValidationResult.Failed($"Keyword '{value}' contains invalid character '{c}'. Only uppercase letters, numbers, and underscores are allowed", paramInfo.name);
			}

			return ValidationResult.Success();
		}

		private static ValidationResult ValidateEnum(string value, LwguiParameterInfo paramInfo)
		{
			if (paramInfo.enumOptions == null || paramInfo.enumOptions.Count == 0)
				return ValidationResult.Success();

			if (!paramInfo.enumOptions.Contains(value))
				return ValidationResult.Failed($"'{value}' is not a valid option. Valid options: {string.Join(", ", paramInfo.enumOptions)}", paramInfo.name);

			return ValidationResult.Success();
		}

		private static ValidationResult ValidatePropertyName(string value, LwguiParameterInfo paramInfo, ValidationContext context)
		{
			if (context?.existingPropertyNames == null || context.existingPropertyNames.Count == 0)
				return ValidationResult.Success(); // No context available, skip validation

			if (!context.existingPropertyNames.Contains(value))
				return ValidationResult.Failed($"Property '{value}' does not exist in the shader", paramInfo.name);

			return ValidationResult.Success();
		}

		private static ValidationResult ValidatePassName(string value, LwguiParameterInfo paramInfo, ValidationContext context)
		{
			if (context?.existingPassNames == null || context.existingPassNames.Count == 0)
				return ValidationResult.Success(); // No context available, skip validation

			if (!context.existingPassNames.Contains(value))
				return ValidationResult.Failed($"Pass LightMode '{value}' does not exist in the shader", paramInfo.name);

			return ValidationResult.Success();
		}
	}

	#endregion

	#region Drawer Discovery

	/// <summary>
	/// Discovers and caches LWGUI Drawer/Decorator types using reflection
	/// Does not depend on any external editor framework
	/// </summary>
	public static class LwguiDrawerDiscovery
	{
		private static List<LwguiDrawerInfo> _drawerCache;
		private static List<LwguiDrawerInfo> _decoratorCache;
		private static Dictionary<string, LwguiDrawerInfo> _typeLookup;
		private static bool _initialized;

		public static void Initialize()
		{
			if (_initialized)
				return;

			_drawerCache = new List<LwguiDrawerInfo>();
			_decoratorCache = new List<LwguiDrawerInfo>();

			try
			{
				var currentAssembly = Assembly.GetExecutingAssembly();
				var baseDrawerInterface = currentAssembly.GetType("LWGUI.IBaseDrawer");
				
				if (baseDrawerInterface == null)
					return;

				var drawerTypes = currentAssembly.GetTypes()
					.Where(t => baseDrawerInterface.IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
					.ToList();

				foreach (var type in drawerTypes)
				{
					var info = ParseDrawerType(type);
					if (info != null)
					{
						if (info.isDecorator)
							_decoratorCache.Add(info);
						else
							_drawerCache.Add(info);
					}
				}

				_drawerCache = _drawerCache
					.OrderBy(d => d.categoryPath ?? "")
					.ThenBy(d => d.order)
					.ThenBy(d => d.displayName)
					.ToList();
					
				_decoratorCache = _decoratorCache
					.OrderBy(d => d.categoryPath ?? "")
					.ThenBy(d => d.order)
					.ThenBy(d => d.displayName)
					.ToList();

				_typeLookup = new Dictionary<string, LwguiDrawerInfo>();
				foreach (var drawer in _drawerCache)
					_typeLookup[drawer.typeName] = drawer;
				foreach (var decorator in _decoratorCache)
					_typeLookup[decorator.typeName] = decorator;

				_initialized = true;
			}
			catch (Exception e)
			{
				Debug.LogError($"[LWGUI] Failed to initialize drawer discovery: {e.Message}");
			}
		}

		private static LwguiDrawerInfo ParseDrawerType(Type type)
		{
			var info = new LwguiDrawerInfo
			{
				typeName = type.Name,
				displayName = type.Name,
				drawerType = type,
				isDecorator = type.Name.EndsWith("Decorator")
			};

			ReadCategoryAttribute(type, info);
			
			// Read parameter attributes from class level (shared across all constructors)
			var classLevelParamDefaults = ReadClassLevelParameterDefaults(type);

			var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
			if (constructors.Length == 0)
				return info;

			// Order constructors by parameter count (descending) - main constructor first
			var orderedConstructors = constructors
				.OrderByDescending(c => c.GetParameters().Length)
				.ToList();

			// Collect all unique parameter names across all constructors
			var allParamNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

			for (int i = 0; i < orderedConstructors.Count; i++)
			{
				var constructor = orderedConstructors[i];
				var constructorInfo = ParseConstructor(constructor, classLevelParamDefaults, i);
				if (constructorInfo == null) continue; // Skip invalid constructors
				
				info.constructors.Add(constructorInfo);
				
				// Collect parameter names for value sharing
				foreach (var param in constructorInfo.parameters)
				{
					allParamNames.Add(param.name);
				}
			}

			info.allParameterNames = allParamNames.ToList();
			return info;
		}

		private static Dictionary<string, ParameterDefaultData> ReadClassLevelParameterDefaults(Type type)
		{
			var result = new Dictionary<string, ParameterDefaultData>(StringComparer.OrdinalIgnoreCase);

			try
			{
				// Read attributes from class level instead of constructors
				var attrs = type.GetCustomAttributes(false)
					.Where(a => a.GetType().IsSubclassOf(typeof(LwguiDrawerParameterAttributeBase)));

				foreach (var attr in attrs)
				{
					var data = ExtractParameterDefaultData(attr);
					if (data != null && !string.IsNullOrEmpty(data.parameterName))
					{
						result[data.parameterName] = data;
					}
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[LWGUI] Failed to read class-level parameter defaults: {e.Message}");
			}

			return result;
		}

		private static LwguiConstructorInfo ParseConstructor(ConstructorInfo constructor, Dictionary<string, ParameterDefaultData> classLevelDefaults, int index)
		{
			var parameters = constructor.GetParameters();

			// Skip constructors with unsupported parameter types (only string and float are valid for shader attributes)
			if (parameters.Any(p => p.ParameterType != typeof(string) && p.ParameterType != typeof(float)))
				return null;

			var constructorInfo = new LwguiConstructorInfo
			{
				index = index
			};

			foreach (var param in parameters)
			{
				// Try to get defaults from class-level attributes first
				var paramDefault = classLevelDefaults.GetValueOrDefault(param.Name);
				
				// Determine if parameter is optional (has default value in constructor)
				bool isOptional = param.HasDefaultValue;
				
				// Get default value: class attribute > constructor default > null
				string defaultValue = paramDefault?.defaultValue;
				if (defaultValue == null && param.HasDefaultValue)
				{
					defaultValue = FormatDefaultValue(param.DefaultValue);
				}

				var paramInfo = new LwguiParameterInfo(
					param.Name,
					paramDefault?.parameterType ?? LwguiParameterType.String,
					defaultValue,
					paramDefault?.displayText,
					isOptional
				);

				if (paramDefault?.enumOptions != null)
					paramInfo.enumOptions.AddRange(paramDefault.enumOptions);

				constructorInfo.parameters.Add(paramInfo);
			}

			// Generate display name after all parameters are parsed
			constructorInfo.displayName = constructorInfo.GenerateDisplayName();

			return constructorInfo;
		}

		private static ParameterDefaultData ExtractParameterDefaultData(object attr)
		{
			var data = new ParameterDefaultData();

			// All parameter attributes inherit from LwguiDrawerParameterAttributeBase
			if (attr is LwguiDrawerParameterAttributeBase baseAttr)
			{
				data.parameterName = baseAttr.parameterName;
				data.defaultValue = baseAttr.defaultValue;
				data.displayText = baseAttr.displayText;
				data.parameterType = baseAttr.parameterType;
				data.enumOptions = baseAttr.enumOptions?.ToList();
			}

			return data;
		}

		private class ParameterDefaultData
		{
			public string parameterName;
			public LwguiParameterType parameterType;
			public string defaultValue;
			public string displayText;
			public List<string> enumOptions;
		}

		private static string FormatDefaultValue(object value)
		{
			if (value == null) return null;
			if (value is string str) return str;
			if (value is bool b) return b ? "true" : "false";
			return value.ToString();
		}

		private static void ReadCategoryAttribute(Type type, LwguiDrawerInfo info)
		{
			try
			{
				if (type.GetCustomAttributes(false)
					.FirstOrDefault(attr => attr is LwguiDrawerCategoryAttribute) is LwguiDrawerCategoryAttribute categoryAttr)
				{
					info.categoryPath = categoryAttr.categoryPath;
					info.order = categoryAttr.order;
				}
			}
			catch (Exception e)
			{
				Debug.LogWarning($"[LWGUI] Failed to read category for {type.Name}: {e.Message}");
			}
		}

		public static List<LwguiDrawerInfo> GetDrawers()
		{
			Initialize();
			return _drawerCache ?? new List<LwguiDrawerInfo>();
		}

		public static List<LwguiDrawerInfo> GetDecorators()
		{
			Initialize();
			return _decoratorCache ?? new List<LwguiDrawerInfo>();
		}

		public static LwguiDrawerInfo GetDrawerInfo(string typeName)
		{
			Initialize();
			if (_typeLookup != null && !string.IsNullOrEmpty(typeName) && _typeLookup.TryGetValue(typeName, out var info))
				return info;
			return null;
		}

		/// <summary>
		/// Check if a value should be considered a default value (for filtering purposes)
		/// </summary>
		public static bool IsDefaultValue(string value, LwguiParameterInfo param)
		{
			if (string.IsNullOrEmpty(value)) return true;
			
			if (param.hasDefaultValue && value.Equals(param.defaultValue, StringComparison.OrdinalIgnoreCase))
				return true;
			
			if (value == "_" || value.Equals("Empty", StringComparison.OrdinalIgnoreCase))
				return true;
			
			switch (param.parameterType)
			{
				case LwguiParameterType.Float:
				case LwguiParameterType.Int:
					return value == "0";
				case LwguiParameterType.Bool:
					return value == "false" || value == "False";
				default:
					return false;
			}
		}
	}

	#endregion
}
