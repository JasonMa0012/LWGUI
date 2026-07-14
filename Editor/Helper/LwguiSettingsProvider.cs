// Copyright (c) Jason Ma
// LWGUI - Project Settings Provider
// Provides a toggle in Project Settings > LWGUI to enable/disable ASE integration.

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	public static class LwguiSettingsProvider
	{
		private const string Symbol = "LWGUI_ASE_INTEGRATION";
		private const string SettingsPath = "Project/LWGUI";

		[SettingsProvider]
		static SettingsProvider CreateLwguiSettingsProvider()
		{
			var provider = new SettingsProvider( SettingsPath, SettingsScope.Project )
			{
				label = "LWGUI",
				keywords = new HashSet<string> { "LWGUI", "ASE", "Amplify", "Integration" },
				guiHandler = DrawGUI
			};
			return provider;
		}

		private static void DrawGUI( string searchContext )
		{
			var target = EditorUserBuildSettings.selectedBuildTargetGroup;
			PlayerSettings.GetScriptingDefineSymbolsForGroup( target, out string[] defines );
			var definesList = defines.ToList();

			bool isEnabled = definesList.Contains( Symbol );

			EditorGUILayout.Space();
			EditorGUILayout.LabelField( "Third-Party Integration", EditorStyles.boldLabel );

			if ( isEnabled )
			{
				EditorGUILayout.HelpBox(
					"ASE Integration is enabled. LWGUI Drawer/Decorator attributes " +
					"are available in Amplify Shader Editor property nodes.\n" +
					"Requires Amplify Shader Editor v1.9.9.10+.",
					MessageType.Info );
			}
			else
			{
				EditorGUILayout.HelpBox(
					"ASE Integration is disabled. Enable it to add LWGUI Drawer/Decorator " +
					"selection to Amplify Shader Editor property nodes.\n" +
					"Requires Amplify Shader Editor v1.9.9.10+.",
					MessageType.None );
			}

			EditorGUI.BeginChangeCheck();
			bool newValue = EditorGUILayout.Toggle(
				new GUIContent( "ASE Integration",
					"Enable Amplify Shader Editor integration.\n" +
					"Adds LWGUI Drawer/Decorator selection to ASE property nodes.\n" +
					"Requires Amplify Shader Editor v1.9.9.10+ to be installed.\n" +
					"Triggers a domain recompile when changed." ),
				isEnabled );

			if ( EditorGUI.EndChangeCheck() && newValue != isEnabled )
			{
				if ( newValue )
					definesList.Add( Symbol );
				else
					definesList.Remove( Symbol );

				PlayerSettings.SetScriptingDefineSymbolsForGroup( target, definesList.ToArray() );
			}
		}
	}
}
