using UnityEngine;
using UnityEditor;

namespace LWGUI.CustomGUISample
{
	public static class CustomFooter
	{
		public static void DoCustomFooter(LWGUI lwgui)
		{
			// Draw your custom gui...
			
			// Debug.Log(lwgui.shader);
		}
		
		[InitializeOnLoadMethod]
		private static void RegisterEvent()
		{
			LWGUI.onDrawCustomFooter += DoCustomFooter;
		}
	}
}