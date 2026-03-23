// Copyright (c) Jason Ma

using System;

namespace LWGUI
{
    /// <summary>
    /// Specifies the category path for a LWGUI Drawer or Decorator.
    /// Used by external tools (e.g., ASE) to organize drawers in a hierarchical menu.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DrawerCategoryAttribute : Attribute
    {
        /// <summary>
        /// Category path using '/' as separator, e.g., "Texture", "Texture/Advanced"
        /// </summary>
        public string CategoryPath { get; }
        
        /// <summary>
        /// Optional display order for sorting within the same category. Lower values appear first.
        /// </summary>
        public int Order { get; }

        public DrawerCategoryAttribute(string categoryPath, int order = 0)
        {
            CategoryPath = categoryPath ?? string.Empty;
            Order = order;
        }
    }
}
