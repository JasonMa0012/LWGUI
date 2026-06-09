// Copyright (c) Jason Ma
using UnityEngine;
using UnityEditor;

namespace LWGUI.CustomGUISample
{
	public static class CustomHeader
	{
		public static void DoCustomHeader(LWGUI lwgui)
		{
			// Draw your custom gui...
			
			// Debug.Log(lwgui.shader);
		}
		
		[InitializeOnLoadMethod]
		private static void RegisterEvent()
		{
			// Register Event
			// LWGUI.onDrawCustomHeader += DoCustomHeader;
		}
	}
}