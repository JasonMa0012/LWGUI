// Copyright (c) Jason Ma
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
    public static class GUIStyles
    {
        // Tips: Use properties to fix null reference errors
        private static GUIStyle _title;
        private static GUIStyle _iconButton;
        private static GUIStyle _foldout;
        private static GUIStyle _helpbox;
        private static GUIStyle _rampSelectButton;
        private static GUIStyle _objectFieldButton;
        private static GUIStyle _toolbarSearchTextFieldPopup;
        private static GUIStyle _label_monospace;


        public static GUIStyle title => _title ?? new GUIStyle(EditorStyles.boldLabel)
        {
            alignment = TextAnchor.LowerLeft,
            border =
            {
                bottom = 2
            }
        };

        public static GUIStyle iconButton => _iconButton ?? new GUIStyle(EditorStyles.iconButton) { fixedHeight = 0, fixedWidth = 0 };

        public static GUIStyle foldout => _foldout ?? new GUIStyle(EditorStyles.miniButton)
        {
            contentOffset = new Vector2(22, 0),
            fixedHeight = 27,
            alignment = TextAnchor.MiddleLeft,
            font = EditorStyles.boldLabel.font,
            fontSize = EditorStyles.boldLabel.fontSize + 1
        };

        public static GUIStyle helpbox => _helpbox ?? new GUIStyle(EditorStyles.helpBox) { fontSize = 12 };

        public static GUIStyle rampSelectButton => _rampSelectButton ?? new GUIStyle(EditorStyles.miniButton)
        {
            fixedHeight = 0,
            stretchHeight = true,
            alignment = TextAnchor.MiddleLeft
        };

        public static GUIStyle objectFieldButton => _objectFieldButton ?? new GUIStyle("ObjectFieldButton")
        {
            fixedHeight = 0,
            stretchHeight = true
        };

        public static GUIStyle toolbarSearchTextFieldPopup
        {
            get
            {
                if (_toolbarSearchTextFieldPopup == null)
                {
                    string toolbarSeachTextFieldPopupStr = "ToolbarSeachTextFieldPopup";
                    {
                        // ToolbarSeachTextFieldPopup has renamed at Unity 2021.3.28+
#if !UNITY_2022_3_OR_NEWER
						string[] versionParts = Application.unityVersion.Split('.');
						int majorVersion = int.Parse(versionParts[0]);
						int minorVersion = int.Parse(versionParts[1]);
						Match patchVersionMatch = Regex.Match(versionParts[2], @"\d+");
						int patchVersion = int.Parse(patchVersionMatch.Value);
						if (majorVersion >= 2021 && minorVersion >= 3 && patchVersion >= 28)
#endif
                        {
                            toolbarSeachTextFieldPopupStr = "ToolbarSearchTextFieldPopup";
                        }
                    }
                    _toolbarSearchTextFieldPopup = new GUIStyle(toolbarSeachTextFieldPopupStr);
                }
                return _toolbarSearchTextFieldPopup;
            }
        }

        public static GUIStyle label_monospace => _label_monospace ?? new GUIStyle(EditorStyles.label) { font = EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf") as Font };
    }
}