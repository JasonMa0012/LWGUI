// Copyright (c) Jason Ma

using System;

namespace LWGUI
{
    /// <summary>
    /// Specifies the default value for a parameter in a LWGUI Drawer or Decorator constructor.
    /// Used by external tools (e.g., ASE) to display default values in the UI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = true)]
    public class ParameterDefaultAttribute : Attribute
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        public string ParameterName { get; }
        
        /// <summary>
        /// Default value as string representation
        /// </summary>
        public string DefaultValue { get; }
        
        /// <summary>
        /// Display text for the default value (e.g., "Empty", "On", "Off")
        /// If null, DefaultValue will be used
        /// </summary>
        public string DisplayText { get; }
        
        /// <summary>
        /// Whether this parameter is optional (can be omitted in generated code)
        /// </summary>
        public bool IsOptional { get; }

        /// <summary>
        /// Creates a ParameterDefaultAttribute
        /// </summary>
        /// <param name="parameterName">Name of the parameter</param>
        /// <param name="defaultValue">Default value as string</param>
        /// <param name="displayText">Display text for UI (optional, uses DefaultValue if not set)</param>
        /// <param name="isOptional">Whether this parameter can be omitted (optional, default false)</param>
        public ParameterDefaultAttribute(string parameterName, string defaultValue, string displayText = null, bool isOptional = true)
        {
            ParameterName = parameterName ?? string.Empty;
            DefaultValue = defaultValue ?? string.Empty;
            DisplayText = displayText;
            IsOptional = isOptional;
        }
    }
}
