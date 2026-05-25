// Copyright (c) Jason Ma

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace LWGUI
{
    public interface IBaseDrawer
    {
        public void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData) {}

        public void GetDefaultValueDescription(Shader inShader, MaterialProperty inProp, MaterialProperty inDefaultProp, PerShaderData inPerShaderData, PerMaterialData inoutPerMaterialData) {}
		
        public void GetCustomContextMenus(GenericMenu menu, Rect rect, MaterialProperty prop, LWGUIMetaDatas metaDatas) {}
    }
    
    public interface IPresetDrawer
    {
        public string GetPresetFileName();
		
        public LwguiShaderPropertyPreset.Preset GetActivePreset(MaterialProperty inProp, LwguiShaderPropertyPreset lwguiShaderPropertyPreset) =>
            lwguiShaderPropertyPreset?.TryGetPreset(inProp?.floatValue ?? -1);
    }

    public partial class PropertyStaticData
    {
        // Image
        public Texture2D image;
		
        // Button
        public List<string> buttonDisplayNames = new();
        public List<string> buttonCommands = new();
        public List<float> buttonDisplayNameWidths = new();

        // Label
        public float labelWidth = 0f;

        // HelpURLDecorator
        public string helpURL = string.Empty;

        // You can add more data that is determined during the initialization of the Drawer as a cache here,
        // thereby avoiding the need to calculate it every frame in OnGUI().
        // >>>>>>>>>>>>>>>>>>>>>>>> Add new data here <<<<<<<<<<<<<<<<<<<<<<<
    }
}