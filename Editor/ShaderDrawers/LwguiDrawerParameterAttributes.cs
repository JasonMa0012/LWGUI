// Copyright (c) Jason Ma
// Provides type-safe parameter metadata for Drawer/Decorator constructors

using System;
using System.Globalization;

namespace LWGUI
{
    /// <summary>
    /// Base class for all LWGUI drawer parameter attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class LwguiDrawerParameterAttributeBase : Attribute
    {
        /// <summary>Parameter name</summary>
        public string parameterName { get; protected set; }
        
        /// <summary>Default value as string</summary>
        public string defaultValue { get; protected set; }
        
        /// <summary>Display text for UI</summary>
        public string displayText { get; protected set; }
        
        /// <summary>Parameter type</summary>
        public LwguiParameterType parameterType { get; protected set; }
        
        /// <summary>Enum options for Enum type parameters</summary>
        public string[] enumOptions { get; protected set; }
    }

    /// <summary>
    /// Float parameter attribute
    /// </summary>
    public class LwguiDrawerParameterFloatAttribute : LwguiDrawerParameterAttributeBase
    {
        public LwguiDrawerParameterFloatAttribute(string parameterName, float defaultValue = 0f, string displayText = null)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.Float;
            this.defaultValue = defaultValue.ToString(CultureInfo.InvariantCulture);
            this.displayText = displayText;
        }
    }

    /// <summary>
    /// Int parameter attribute
    /// </summary>
    public class LwguiDrawerParameterIntAttribute : LwguiDrawerParameterAttributeBase
    {
        public LwguiDrawerParameterIntAttribute(string parameterName, int defaultValue = 0, string displayText = null)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.Int;
            this.defaultValue = defaultValue.ToString();
            this.displayText = displayText;
        }
    }

    /// <summary>
    /// Bool parameter attribute
    /// </summary>
    public class LwguiDrawerParameterBoolAttribute : LwguiDrawerParameterAttributeBase
    {
        public LwguiDrawerParameterBoolAttribute(string parameterName, bool defaultValue = false, string displayText = null)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.Bool;
            this.defaultValue = defaultValue ? "true" : "false";
            this.displayText = displayText;
        }
    }

    /// <summary>
    /// String parameter attribute
    /// </summary>
    public class LwguiDrawerParameterStringAttribute : LwguiDrawerParameterAttributeBase
    {
        public LwguiDrawerParameterStringAttribute(string parameterName, string defaultValue = "", string displayText = null)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.String;
            this.defaultValue = defaultValue;
            this.displayText = displayText;
        }
    }

    /// <summary>
    /// Enum parameter attribute with string options
    /// </summary>
    public class LwguiDrawerParameterEnumAttribute : LwguiDrawerParameterAttributeBase
    {
        /// <summary>
        /// Creates an enum parameter with string options
        /// </summary>
        public LwguiDrawerParameterEnumAttribute(string parameterName, int defaultIndex, params string[] options)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.Enum;
            this.enumOptions = options;
            this.defaultValue = options != null && defaultIndex >= 0 && defaultIndex < options.Length 
                ? options[defaultIndex] 
                : null;
            this.displayText = null;
        }

        /// <summary>
        /// Creates an enum parameter from a C# enum type with default value
        /// </summary>
        public LwguiDrawerParameterEnumAttribute(string parameterName, object defaultEnumValue)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.Enum;
            
            if (defaultEnumValue != null)
            {
                Type enumType = defaultEnumValue.GetType();
                if (enumType.IsEnum)
                {
                    this.enumOptions = Enum.GetNames(enumType);
                    this.defaultValue = Enum.GetName(enumType, defaultEnumValue);
                }
            }
            
            this.displayText = null;
        }
    }

    /// <summary>
    /// Keyword parameter attribute (uppercase, alphanumeric and underscore only)
    /// </summary>
    public class LwguiDrawerParameterKeywordAttribute : LwguiDrawerParameterAttributeBase
    {
        public LwguiDrawerParameterKeywordAttribute(string parameterName, string defaultValue = "", string displayText = null)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.Keyword;
            this.defaultValue = defaultValue?.ToUpper();
            this.displayText = displayText;
        }
    }

    /// <summary>
    /// PropertyName parameter attribute (reference to another shader property)
    /// </summary>
    public class LwguiDrawerParameterPropertyNameAttribute : LwguiDrawerParameterAttributeBase
    {
        public LwguiDrawerParameterPropertyNameAttribute(string parameterName, string defaultValue = "", string displayText = null)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.PropertyName;
            this.defaultValue = defaultValue;
            this.displayText = displayText;
        }
    }

    /// <summary>
    /// PassName parameter attribute (reference to a pass LightMode)
    /// </summary>
    public class LwguiDrawerParameterPassNameAttribute : LwguiDrawerParameterAttributeBase
    {
        public LwguiDrawerParameterPassNameAttribute(string parameterName, string defaultValue = "", string displayText = null)
        {
            this.parameterName = parameterName;
            this.parameterType = LwguiParameterType.PassName;
            this.defaultValue = defaultValue;
            this.displayText = displayText;
        }
    }
}
