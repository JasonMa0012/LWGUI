// Copyright (c) Jason Ma

using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	/// <summary>
	/// Draw one or more Buttons within the same row, using the Display Name to control the appearance and behavior of the buttons
	/// 
	/// Declaring a set of Button Name and Button Command in Display Name generates a Button, separated by '@':
	/// ButtonName0@ButtonCommand0@ButtonName1@ButtonCommand1
	/// 
	/// Button Name can be any other string, the format of Button Command is:
	/// TYPE:Argument
	/// 
	/// The following TYPEs are currently supported:
	/// - URL: Open the URL, Argument is the URL
	/// - C#: Call the public static C# function, Argument is NameSpace.Class.Method(arg0, arg1, ...),
	///		for target function signatures, see: LWGUI.ButtonDrawer.TestMethod().
	///
	/// The full example:
	/// [Button(_)] _button0 ("URL Button@URL:https://github.com/JasonMa0012/LWGUI@C#:LWGUI.ButtonDrawer.TestMethod(1234, abcd)", Float) = 0
	/// 
	/// group: parent group name (Default: none)
	/// Target Property Type: Any
	/// </summary>
	[LwguiDrawerCategory("Other")]
	public class ButtonDrawer : SubDrawer
	{
		private const string _urlPrefix = "URL:";
		private const string _csPrefix = "C#:";
		private const string _separator = "@";
		
		public ButtonDrawer() { }
			
		public ButtonDrawer(string group)
		{
			this.group = group;
		}

		protected override float GetVisibleHeight(MaterialProperty prop) => 24;

		public override void BuildStaticMetaData(Shader inShader, MaterialProperty inProp, MaterialProperty[] inProps, PropertyStaticData inoutPropertyStaticData)
		{
			inoutPropertyStaticData.groupName = group;

			// Display Name: ButtonName@URL:XXX@ButtonName@CS:NameSpace.Class.Method(arg0, arg1, ...)@...
			var buttonNameAndCommands = inProp.displayName.Split(_separator);
			if (buttonNameAndCommands != null && buttonNameAndCommands.Length > 0 && buttonNameAndCommands.Length % 2 == 0)
			{
				for (int i = 0; i < buttonNameAndCommands.Length; i++)
				{
					if (i % 2 == 0)
					{
						inoutPropertyStaticData.buttonDisplayNames.Add(buttonNameAndCommands[i]);
						inoutPropertyStaticData.buttonDisplayNameWidths.Add(EditorStyles.label.CalcSize(new GUIContent(buttonNameAndCommands[i])).x);
					}
					else
					{
						inoutPropertyStaticData.buttonCommands.Add(buttonNameAndCommands[i]);
					}
				}
			}
			else
			{
				Debug.LogError($"LWGUI: ButtonDrawer with invalid Display Name Commands: { buttonNameAndCommands } ! prop: { inProp.name }");
			}
		}

		public override void DrawProp(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
		{
			var buttonDisplayNames = metaDatas.GetPropStaticData(prop).buttonDisplayNames;
			var buttonDisplayNameWidths = metaDatas.GetPropStaticData(prop).buttonDisplayNameWidths;
			var buttonCommands = metaDatas.GetPropStaticData(prop).buttonCommands;
			if (buttonDisplayNames == null || buttonCommands == null || buttonDisplayNames.Count == 0 || buttonCommands.Count == 0 
			    || buttonDisplayNames.Count != buttonCommands.Count)
			{
				return;
			}

			var enbaled = GUI.enabled;
			GUI.enabled = true;
			
			position = EditorGUI.IndentedRect(position);
			var rect = new Rect(position.x, position.y, 0, position.height);
			var spaceWidth = (position.width - buttonDisplayNameWidths.Sum()) / buttonDisplayNames.Count;

			for (int i = 0; i < buttonDisplayNames.Count; i++)
			{
				var displayName = buttonDisplayNames[i];
				var displayNameRelativeWidth = buttonDisplayNameWidths[i];
				var command = buttonCommands[i];
				rect.xMax = rect.xMin + displayNameRelativeWidth + spaceWidth;

				if (GUI.Button(rect, new GUIContent(displayName, command)))
				{
					if (command.StartsWith(_urlPrefix))
					{
						Application.OpenURL(command.Substring(_urlPrefix.Length, command.Length - _urlPrefix.Length));
					}
					else if (command.StartsWith(_csPrefix))
					{
						var csCommand = command.Substring(_csPrefix.Length, command.Length - _csPrefix.Length);
						
						// Get method name and args
						string className = null, methodName = null;
						string[] args = null;
						{
							var lastPointIndex = csCommand.LastIndexOf('.');
							if (lastPointIndex != -1)
							{
								className = csCommand.Substring(0, lastPointIndex);
								var leftBracketIndex = csCommand.IndexOf('(');
								if (leftBracketIndex != -1)
								{
									methodName = csCommand.Substring(lastPointIndex + 1, leftBracketIndex - lastPointIndex - 1);
									args = csCommand.Substring(leftBracketIndex + 1, csCommand.Length - leftBracketIndex - 2)
										?.Split(',').Select(s => s.TrimStart()).ToArray();
								}
							}
						}

						// Find and call method
						if (!string.IsNullOrEmpty(className) && !string.IsNullOrEmpty(methodName) && args != null)
						{
							Type type = ReflectionHelper.GetAllTypes().FirstOrDefault((type1 => type1.Name == className || type1.FullName == className));
							if (type != null)
							{
								var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
								if (methodInfo != null)
								{
									methodInfo.Invoke(null, new object[]{ prop, editor, metaDatas, args });
								}
								else
								{
									Debug.LogError($"LWGUI: Method {methodName} not found in {className}");
								}
							}
							else
							{
								Debug.LogError($"LWGUI: Class {className} not found");
							}
						}
						else
						{
							Debug.LogError($"LWGUI: Invalid C# command: {csCommand}");
						}
					}
					else
					{
						Debug.LogError($"LWGUI: Unknown command type: {command}");
					}
				}

				rect.xMin = rect.xMax;
			}

			GUI.enabled = enbaled;
		}

		public static void TestMethod(MaterialProperty prop, MaterialEditor editor, LWGUIMetaDatas metaDatas, string[] args)
		{
			Debug.Log($"LWGUI: ButtonDrawer.TestMethod({prop}, {editor}, {metaDatas}, {args})");
			
			foreach (var arg in args)
			{
				Debug.Log(arg);
			}
		}
	}
}
