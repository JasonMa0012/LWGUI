// Copyright (c) Jason Ma

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LWGUI.PerformanceMonitor;

namespace LWGUI
{
    public static class ToolbarHelper
    {
        #region Toolbar Buttons

        internal enum CopyMaterialValueMask
        {
            Float       = 1 << 0,
            Vector      = 1 << 1,
            Texture     = 1 << 2,
            Keyword     = 1 << 3,
            RenderQueue = 1 << 4,
            Number      = Float | Vector,
            All         = (1 << 5) - 1,
        }

        public const uint CopyMaterialValueMaskAll = (uint)CopyMaterialValueMask.All;

        private static GUIContent[] _pasteMaterialMenus = new[]
        {
            new GUIContent("Paste Number Values"),
            new GUIContent("Paste Texture Values"),
            new GUIContent("Paste Keywords"),
            new GUIContent("Paste RenderQueue"),
        };

        private static uint[] _pasteMaterialMenuValueMasks = new[]
        {
            (uint)CopyMaterialValueMask.Number,
            (uint)CopyMaterialValueMask.Texture,
            (uint)CopyMaterialValueMask.Keyword,
            (uint)CopyMaterialValueMask.RenderQueue,
        };


        private const string _iconCopyGUID       = "9cdef444d18d2ce4abb6bbc4fed4d109";
        private const string _iconPasteGUID      = "8e7a78d02e4c3574998524a0842a8ccb";
        private const string _iconSelectGUID     = "6f44e40b24300974eb607293e4224ecc";
        private const string _iconExpandGUID     = "2382450e7f4ddb94c9180d6634c41378";
        private const string _iconCollapseGUID   = "929b6e5dfacc42b429d715a3e1ca2b57";
        private const string _iconStatsGUID      = "88909414120107547a673b8fcddc5236";
        private const string _iconVisibilityGUID = "9576e23a695b35d49a9fc55c9a948b4f";

        private const string _iconCopyTooltip       = "Copy Material Properties";
        private const string _iconPasteTooltip      = "Paste Material Properties\n\nRight-click to paste values by type.";
        private const string _iconSelectTooltip     = "Select the Material Asset\n\nUsed to jump from a Runtime Material Instance to a Material Asset.";
        private const string _iconExpandTooltip     = "Expand All Groups";
        private const string _iconCollapseTooltip   = "Collapse All Groups";
        private const string _iconStatsTooltip      = "Display Shader Performance Stats";
        private const string _iconVisibilityTooltip = "Display Mode";

        private static GUIContent _guiContentCopyCache;
        private static GUIContent _guiContentPasteCache;
        private static GUIContent _guiContentSelectCache;
        private static GUIContent _guiContentCheckoutCache;
        private static GUIContent _guiContentExpandCache;
        private static GUIContent _guiContentCollapseCache;
        private static GUIContent _guiContentStatsCache;
        private static GUIContent _guiContentVisibilityCache;

        private static GUIContent _guiContentCopy       => _guiContentCopyCache = _guiContentCopyCache ?? new GUIContent("", AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(_iconCopyGUID)), _iconCopyTooltip);
        private static GUIContent _guiContentPaste      => _guiContentPasteCache = _guiContentPasteCache ?? new GUIContent("", AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(_iconPasteGUID)), _iconPasteTooltip);
        private static GUIContent _guiContentSelect     => _guiContentSelectCache = _guiContentSelectCache ?? new GUIContent("", AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(_iconSelectGUID)), _iconSelectTooltip);
        private static GUIContent _guiContentExpand     => _guiContentExpandCache = _guiContentExpandCache ?? new GUIContent("", AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(_iconExpandGUID)), _iconExpandTooltip);
        private static GUIContent _guiContentCollapse   => _guiContentCollapseCache = _guiContentCollapseCache ?? new GUIContent("", AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(_iconCollapseGUID)), _iconCollapseTooltip);
        private static GUIContent _guiContentStats      => _guiContentStatsCache = _guiContentStatsCache ?? new GUIContent("", AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(_iconStatsGUID)), _iconStatsTooltip);
        private static GUIContent _guiContentVisibility => _guiContentVisibilityCache = _guiContentVisibilityCache ?? new GUIContent("", AssetDatabase.LoadAssetAtPath<Texture>(AssetDatabase.GUIDToAssetPath(_iconVisibilityGUID)), _iconVisibilityTooltip);


        public static void DrawToolbarButtons(ref Rect toolBarRect, LWGUIMetaDatas metaDatas)
        {
            var (perShaderData, perMaterialData, perInspectorData) = metaDatas.GetDatas();
            var shader = metaDatas.GetShader();

            //----------------------------------------------------------------------------------------------------------------
            // Copy
            var buttonRectOffset = toolBarRect.height + 2;
            var buttonRect = new Rect(toolBarRect.x, toolBarRect.y, toolBarRect.height, toolBarRect.height);
            toolBarRect.xMin += buttonRectOffset;
            if (GUI.Button(buttonRect, _guiContentCopy, GUIStyles.iconButton))
            {
                ContextMenuHelper.CopyMaterial(metaDatas.GetMaterial());
            }

            //----------------------------------------------------------------------------------------------------------------
            // Paste
            buttonRect.x += buttonRectOffset;
            toolBarRect.xMin += buttonRectOffset;
            // Right Click
            if (Event.current.type == EventType.MouseDown
                && Event.current.button == 1
                && buttonRect.Contains(Event.current.mousePosition))
            {
                EditorUtility.DisplayCustomMenu(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, 0, 0), _pasteMaterialMenus, -1,
                    (data, options, selected) => { ContextMenuHelper.PastePropertiesToMaterials(metaDatas, _pasteMaterialMenuValueMasks[selected]); }, null);
                Event.current.Use();
            }
            // Left Click
            if (GUI.Button(buttonRect, _guiContentPaste, GUIStyles.iconButton))
            {
                ContextMenuHelper.PastePropertiesToMaterials(metaDatas, (uint)CopyMaterialValueMask.All);
            }

            //----------------------------------------------------------------------------------------------------------------
            // Select Material Asset, jump from a Runtime Material Instance to a Material Asset
            buttonRect.x += buttonRectOffset;
            toolBarRect.xMin += buttonRectOffset;
            if (GUI.Button(buttonRect, _guiContentSelect, GUIStyles.iconButton))
            {
                var material = metaDatas.GetMaterial();

                if (AssetDatabase.Contains(material))
                {
                    Selection.activeObject = material;
                }
                else
                {
                    if (FindMaterialAssetByMaterialInstance(material, metaDatas, out var materialAsset))
                    {
                        Selection.activeObject = materialAsset;
                    }
                }
            }


            //----------------------------------------------------------------------------------------------------------------
            // Expand
            buttonRect.x += buttonRectOffset;
            toolBarRect.xMin += buttonRectOffset;
            if (GUI.Button(buttonRect, _guiContentExpand, GUIStyles.iconButton))
            {
                foreach (var propStaticDataKVPair in perShaderData.propStaticDatas)
                {
                    if (propStaticDataKVPair.Value.isMain || propStaticDataKVPair.Value.isAdvancedHeader)
                        propStaticDataKVPair.Value.isExpanding = true;
                }
            }

            //----------------------------------------------------------------------------------------------------------------
            // Collapse
            buttonRect.x += buttonRectOffset;
            toolBarRect.xMin += buttonRectOffset;
            if (GUI.Button(buttonRect, _guiContentCollapse, GUIStyles.iconButton))
            {
                foreach (var propStaticDataKVPair in perShaderData.propStaticDatas)
                {
                    if (propStaticDataKVPair.Value.isMain || propStaticDataKVPair.Value.isAdvancedHeader)
                        propStaticDataKVPair.Value.isExpanding = false;
                }
            }

            //----------------------------------------------------------------------------------------------------------------
            // Shader Perf Stats
            buttonRect.x += buttonRectOffset;
            toolBarRect.xMin += buttonRectOffset;
            {
                var color = GUI.color;
                if (IsDisplayShaderPerfStatsEnabled(metaDatas.GetShaderUID()))
                    GUI.color = Color.yellow;

                if (GUI.Button(buttonRect, _guiContentStats, GUIStyles.iconButton))
                    SwitchDisplayShaderPerfStatsEnabled(shader, metaDatas.GetShaderUID());

                GUI.color = color;
            }

            //----------------------------------------------------------------------------------------------------------------
            // Display Mode
            buttonRect.x += buttonRectOffset;
            toolBarRect.xMin += buttonRectOffset;
            {
                var color = GUI.color;
                var displayModeData = perShaderData.displayModeData;
                if (!displayModeData.IsDefaultDisplayMode())
                    GUI.color = Color.yellow;
                if (GUI.Button(buttonRect, _guiContentVisibility, GUIStyles.iconButton))
                {
                    var menu = new GenericMenu();
                    menu.AddItem(new GUIContent($"Show All Advanced Properties\t\t({displayModeData.advancedCount} - {perShaderData.propStaticDatas.Count})"),            displayModeData.showAllAdvancedProperties,   OnSwitchDisplayMode, 0);
                    menu.AddItem(new GUIContent($"Show All Hidden Properties\t\t({displayModeData.hiddenCount} - {perShaderData.propStaticDatas.Count})"),                displayModeData.showAllHiddenProperties,     OnSwitchDisplayMode, 1);
                    menu.AddItem(new GUIContent($"Show Only Modified Properties\t\t({perMaterialData.modifiedCount} - {perShaderData.propStaticDatas.Count})"),           displayModeData.showOnlyModifiedProperties,  OnSwitchDisplayMode, 2);
                    menu.AddItem(new GUIContent($"Show Only Modified Properties by Group\t({perMaterialData.modifiedCount} - {perShaderData.propStaticDatas.Count})"),    displayModeData.showOnlyModifiedGroups,      OnSwitchDisplayMode, 3);
                    
                    menu.AddSeparator("");

                    // Label Width: 30%-70%
                    for (int i = 0; i <= 8; i++)
                    {
                        float pct = 0.30f + i * 0.05f;
                        string label = Mathf.Approximately(pct, Helper.DefaultLabelWidthPercentage) ? $"Label Width/{pct * 100:00}% (Default)" : $"Label Width/{pct * 100:00}%";
                        menu.AddItem(new GUIContent(label), Mathf.Approximately(displayModeData.labelWidthPercentage, pct), () =>
                        {
                            displayModeData.labelWidthPercentage = pct;
                            Helper.SetLabelWidthPercentage(metaDatas.GetShaderUID(), pct);
                            MetaDataHelper.ForceUpdateAllMaterialsMetadataCache(shader);
                        });
                    }

                    menu.ShowAsContext();
                    
                    void OnSwitchDisplayMode(object userData)
                    {
                        switch ((int)userData)
                        {
                            case 0: // Show All Advanced Properties
                                displayModeData.showAllAdvancedProperties = !displayModeData.showAllAdvancedProperties;
                                perShaderData.ToggleShowAllAdvancedProperties();
                                break;
                            case 1: // Show All Hidden Properties
                                displayModeData.showAllHiddenProperties = !displayModeData.showAllHiddenProperties;
                                break;
                            case 2: // Show Only Modified Properties
                                displayModeData.showOnlyModifiedProperties = !displayModeData.showOnlyModifiedProperties;
                                if (displayModeData.showOnlyModifiedProperties) displayModeData.showOnlyModifiedGroups = false;
                                MetaDataHelper.ForceUpdateAllMaterialsMetadataCache(shader);
                                break;
                            case 3: // Show Only Modified Groups
                                displayModeData.showOnlyModifiedGroups = !displayModeData.showOnlyModifiedGroups;
                                if (displayModeData.showOnlyModifiedGroups) displayModeData.showOnlyModifiedProperties = false;
                                MetaDataHelper.ForceUpdateAllMaterialsMetadataCache(shader);
                                break;
                        }
                    }
                }
                GUI.color = color;
            }

            toolBarRect.xMin += 2;
        }

        public static Func<Renderer, Material, Material> onFindMaterialAssetInRendererByMaterialInstance;

        public static bool FindMaterialAsset(LWGUIMetaDatas metaDatas, out Material materialAsset)
        {
            return FindMaterialAssetByMaterialInstance(metaDatas.GetMaterial(), metaDatas, out materialAsset);
        }

        private static bool FindMaterialAssetByMaterialInstance(Material material, LWGUIMetaDatas metaDatas, out Material materialAsset)
        {
            materialAsset = null;

            var renderers = metaDatas.perInspectorData.materialEditor.GetMeshRenderersByMaterialEditor();
            foreach (var renderer in renderers)
            {
                if (onFindMaterialAssetInRendererByMaterialInstance != null)
                {
                    materialAsset = onFindMaterialAssetInRendererByMaterialInstance(renderer, material);
                }

                // Look for renderer.sharedMaterials as a fallback, if the runtime has modified the sharedMaterials will not work
                if (materialAsset == null)
                {
                    int index = renderer.materials.ToList().FindIndex(materialInstance => materialInstance == material);
                    if (index >= 0 && index < renderer.sharedMaterials.Length)
                    {
                        materialAsset = renderer.sharedMaterials[index];
                    }
                }

                if (materialAsset != null && AssetDatabase.Contains(materialAsset))
                    return true;
            }

            Debug.LogError("LWGUI: Can not find the Material Assets of: " + material.name);

            return false;
        }

        #endregion


        #region Search Field

        private static readonly int s_TextFieldHash = "EditorTextField".GetHashCode();

        private static readonly GUIContent[] _searchModeMenus = Enumerable.Range(0, (int)SearchMode.Num - 1).Select(i =>
            new GUIContent(((SearchMode)i).ToString())).ToArray();

        /// <returns>is has changed?</returns>
        public static bool DrawSearchField(Rect rect, LWGUIMetaDatas metaDatas)
        {
            var (perShaderData, perMaterialData, perInspectorData) = metaDatas.GetDatas();

            bool hasChanged = false;
            EditorGUI.BeginChangeCheck();

            var revertButtonRect = RevertableHelper.SplitRevertButtonRect(ref rect);

            // Get internal TextField ControlID
            int controlId = GUIUtility.GetControlID(s_TextFieldHash, FocusType.Keyboard, rect) + 1;

            // searching mode
            Rect modeRect = new Rect(rect);
            modeRect.width = 20f;
            if (Event.current.type == UnityEngine.EventType.MouseDown && modeRect.Contains(Event.current.mousePosition))
            {
                EditorUtility.DisplayCustomMenu(rect, _searchModeMenus, (int)perShaderData.searchMode,
                    (data, options, selected) =>
                    {
                        perShaderData.searchMode = (SearchMode)selected;
                        hasChanged = true;
                    }, null);
                Event.current.Use();
            }

            perShaderData.searchString = EditorGUI.TextField(rect, String.Empty, perShaderData.searchString, GUIStyles.toolbarSearchTextFieldPopup);

            if (EditorGUI.EndChangeCheck())
                hasChanged = true;

            // revert button
            if (!string.IsNullOrEmpty(perShaderData.searchString)
                && RevertableHelper.DrawRevertButton(revertButtonRect))
            {
                perShaderData.searchString = string.Empty;
                hasChanged = true;
                GUIUtility.keyboardControl = 0;
            }

            // display search mode
            if (GUIUtility.keyboardControl != controlId
                && string.IsNullOrEmpty(perShaderData.searchString)
                && Event.current.type == UnityEngine.EventType.Repaint)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    var disableTextRect = new Rect(rect.x, rect.y, rect.width,
                        GUIStyles.toolbarSearchTextFieldPopup.fixedHeight > 0.0
                            ? GUIStyles.toolbarSearchTextFieldPopup.fixedHeight
                            : rect.height);
                    disableTextRect = GUIStyles.toolbarSearchTextFieldPopup.padding.Remove(disableTextRect);
                    int fontSize = EditorStyles.label.fontSize;
                    EditorStyles.label.fontSize = GUIStyles.toolbarSearchTextFieldPopup.fontSize;
                    EditorStyles.label.Draw(disableTextRect, new GUIContent(perShaderData.searchMode.ToString()), false, false, false, false);
                    EditorStyles.label.fontSize = fontSize;
                }
            }

            if (hasChanged) perShaderData.UpdateSearchFilter();

            return hasChanged;
        }

        #endregion

        #region Shader Perf Stats

        #region Keyword Overrides

        private static string GetShowKeywordOverridesPreferenceKey(string shaderUID) => $"LWGUI/{shaderUID}/ShowKeywordOverrides";

        private static string GetKeywordOverridePreferenceKey(string shaderUID, string keyword) => $"LWGUI/{shaderUID}/KeywordOverride/{keyword}/IsOverride";

        private static string GetKeywordEnabledPreferenceKey(string shaderUID, string keyword) => $"LWGUI/{shaderUID}/KeywordOverride/{keyword}/IsEnabled";

        private static bool IsShowKeywordOverridesEnabled(string shaderUID) => EditorPrefs.GetBool(GetShowKeywordOverridesPreferenceKey(shaderUID), false);

        public static bool IsUserKeywordOverride(string shaderUID, string keyword) => EditorPrefs.HasKey(GetKeywordOverridePreferenceKey(shaderUID, keyword));

        public static bool IsUserKeywordEnabled(string shaderUID, string keyword) => EditorPrefs.GetBool(GetKeywordEnabledPreferenceKey(shaderUID, keyword), false);

        private static void SetShowKeywordOverridesEnabled(string shaderUID, bool enabled)
        {
            if (enabled)
                EditorPrefs.SetBool(GetShowKeywordOverridesPreferenceKey(shaderUID), true);
            else
                EditorPrefs.DeleteKey(GetShowKeywordOverridesPreferenceKey(shaderUID));
        }

        private static void SetUserKeywordOverride(Shader shader, string shaderUID, string keyword, bool isOverride)
        {
            var overrideKey = GetKeywordOverridePreferenceKey(shaderUID, keyword);

            if (isOverride)
                EditorPrefs.SetBool(overrideKey, true);
            else
                EditorPrefs.DeleteKey(overrideKey);

            MetaDataHelper.ForceUpdateAllMaterialsMetadataCache(shader);
        }

        private static void SetUserKeywordEnabled(Shader shader, string shaderUID, string keyword, bool isEnabled)
        {
            EditorPrefs.SetBool(GetKeywordEnabledPreferenceKey(shaderUID, keyword), isEnabled);
            MetaDataHelper.ForceUpdateAllMaterialsMetadataCache(shader);
        }

        private static void DrawKeywordOverridesList(LWGUIMetaDatas metaDatas)
        {
            var shader = metaDatas.GetShader();
            var shaderUID = metaDatas.GetShaderUID();
            if (!shader) return;

            var showKeywordOverrides = IsShowKeywordOverridesEnabled(shaderUID);

            EditorGUI.indentLevel++;
            var newShowKeywordOverrides = EditorGUILayout.Foldout(showKeywordOverrides, "Keyword Overrides");
            if (newShowKeywordOverrides != showKeywordOverrides)
                SetShowKeywordOverridesEnabled(shaderUID, newShowKeywordOverrides);
            if (newShowKeywordOverrides)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Enabled");
                EditorGUILayout.LabelField("    Override");
                EditorGUILayout.EndHorizontal();

                var activeKeywords = metaDatas.perMaterialData.activeKeywords;
                var allKeywords = shader.keywordSpace.keywords.Select(k => k.name).ToList();
                foreach (var keyword in allKeywords)
                {
                    var rect = EditorGUILayout.BeginHorizontal();
                    
                    // Context Menu
                    if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Copy Keyword"), false, () =>
                        {
                            EditorGUIUtility.systemCopyBuffer = keyword;
                        });
                        menu.ShowAsContext();
                    }

                    bool currentOverride = IsUserKeywordOverride(shaderUID, keyword);
                    bool currentEnabled = currentOverride ? IsUserKeywordEnabled(shaderUID, keyword) : activeKeywords.Contains(keyword);

                    using (new EditorGUI.DisabledGroupScope(!currentOverride))
                    {
                        var newEnabled = EditorGUILayout.ToggleLeft(keyword, currentEnabled);
                        if (newEnabled != currentEnabled)
                            SetUserKeywordEnabled(shader, shaderUID, keyword, newEnabled);
                    }

                    bool newOverride = EditorGUILayout.Toggle(string.Empty, currentOverride);
                    if (newOverride != currentOverride)
                        SetUserKeywordOverride(shader, shaderUID, keyword, newOverride);

                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUI.indentLevel--;
        }

        #endregion

        #region Compiler Settings

        private static string GetShowCompilerSettingsPreferenceKey(string shaderUID) => $"LWGUI/{shaderUID}/ShowCompilerSettings";
        private static bool IsShowCompilerSettingsEnabled(string shaderUID) => EditorPrefs.GetBool(GetShowCompilerSettingsPreferenceKey(shaderUID), false);
        private static void SetShowCompilerSettingsEnabled(string shaderUID, bool enabled)
        {
            if (enabled)
                EditorPrefs.SetBool(GetShowCompilerSettingsPreferenceKey(shaderUID), true);
            else
                EditorPrefs.DeleteKey(GetShowCompilerSettingsPreferenceKey(shaderUID));
        }

        private static void DrawCompilerSettings(LWGUIMetaDatas metaDatas)
        {
            var shaderUID = metaDatas.GetShaderUID();
            var showCompilerSettings = IsShowCompilerSettingsEnabled(shaderUID);

            EditorGUI.indentLevel++;
            var newShowCompilerSettings = EditorGUILayout.Foldout(showCompilerSettings, "Compiler Settings");
            if (newShowCompilerSettings != showCompilerSettings)
                SetShowCompilerSettingsEnabled(shaderUID, newShowCompilerSettings);
            if (newShowCompilerSettings)
            {
                if (GUILayout.Button("Install FXC (Windows SDK)", GUILayout.ExpandWidth(false)))
                {
                    Application.OpenURL("https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/");
                }
                if (GUILayout.Button("Install Mali Offline Compiler", GUILayout.ExpandWidth(false)))
                {
                    Application.OpenURL("https://developer.arm.com/documentation/101863/8-8/Using-Mali-Offline-Compiler/Install-Mali-Offline-Compiler");
                }
            }
            EditorGUI.indentLevel--;
        }

        #endregion
        private static string GetDisplayShaderPerfStatsPreferenceKey(string shaderUID) => $"LWGUI/{shaderUID}/DisplayShaderPerformanceStats";

        public static bool IsDisplayShaderPerfStatsEnabled(string shaderUID) => EditorPrefs.HasKey(GetDisplayShaderPerfStatsPreferenceKey(shaderUID));

        public static void SetDisplayShaderPerfStatsEnabled(Shader shader, string shaderUID, bool enabled)
        {
            if (enabled)
                EditorPrefs.SetBool(GetDisplayShaderPerfStatsPreferenceKey(shaderUID), true);
            else
                EditorPrefs.DeleteKey(GetDisplayShaderPerfStatsPreferenceKey(shaderUID));
            MetaDataHelper.ForceUpdateAllMaterialsMetadataCache(shader);
        }

        public static void SwitchDisplayShaderPerfStatsEnabled(Shader shader, string shaderUID)
        {
            var key = GetDisplayShaderPerfStatsPreferenceKey(shaderUID);
            if (EditorPrefs.HasKey(key))
                EditorPrefs.DeleteKey(key);
            else
                EditorPrefs.SetBool(key, true);
            MetaDataHelper.ForceUpdateAllMaterialsMetadataCache(shader);
        }

        public static void DrawShaderPerformanceStats(LWGUIMetaDatas metaDatas)
        {
            if (!IsDisplayShaderPerfStatsEnabled(metaDatas.GetShaderUID()) || metaDatas.perMaterialData.shaderPerfDatas == null)
                return;

            var fieldWidth = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = 0;

            var compiler = ShaderPerfMonitor.GetActiveCompiler();
            if (compiler != null)
                EditorGUILayout.LabelField($"Shader Performance Stats (Compiler: {compiler?.compilerName ?? "NULL"}, API: {compiler.api}, Target: {compiler.target})", GUIStyles.title);
            else
                EditorGUILayout.LabelField($"Shader Performance Stats (Compiler: NULL)", GUIStyles.title);

            if (compiler != null)
            {
                DrawCompilerSettings(metaDatas);
                DrawKeywordOverridesList(metaDatas);
                compiler.DrawShaderPerformanceStatsHeader(metaDatas);

                var lastPassName = string.Empty;
                foreach (var shaderPerfData in metaDatas.perMaterialData.shaderPerfDatas)
                {
                    if (lastPassName == string.Empty)
                        lastPassName = shaderPerfData.passName;

                    if (lastPassName != shaderPerfData.passName)
                    {
                        lastPassName = shaderPerfData.passName;
                        EditorGUILayout.Space();
                    }

                    compiler.DrawShaderPerformanceStatsLine(metaDatas, shaderPerfData);
                }
                compiler.DrawShaderPerformanceStatsFooter(metaDatas);
            }
            else
            {
                DrawCompilerSettings(metaDatas);
                EditorGUILayout.HelpBox("No shader compiler is available. Please install FXC (Windows SDK) or Mali Offline Compiler to view shader performance stats.", MessageType.Warning);
            }

            EditorGUIUtility.fieldWidth = fieldWidth;
            EditorGUILayout.Space();
            Helper.DrawSplitLine();
        }


        public static void DrawShaderPerformanceStatsLineButtons(ShaderPerfData shaderPerfData)
        {
            if (GUILayout.Button("Find", GUILayout.MaxWidth(40)))
                EditorUtility.RevealInFinder(shaderPerfData.compiledShaderPath);
            // if (GUILayout.Button("Open", GUILayout.MaxWidth(40)))
            // 	IOHelper.OpenFile(shaderPerfData.compiledShaderPath);
        }

        #endregion
    }
}