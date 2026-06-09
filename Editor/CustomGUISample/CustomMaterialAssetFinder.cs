// Copyright (c) Jason Ma
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace LWGUI
{
	public static class CustomMaterialAssetFinder
	{
		public static Material FindMaterialAssetInRendererByMaterialInstance(Renderer renderer, Material materialInstance)
		{
			Material materialAsset = null;

			// Find the material asset by name
			// if (materialAsset == null)
			// {
			// 	var name = materialInstance.name.Replace(" (Instance)", "");
			// 	var guids = AssetDatabase.FindAssets("t:Material " + name, new[] { "Assets" }).Select(guid =>
			// 	{
			// 		var assetPath = AssetDatabase.GUIDToAssetPath(guid);
			// 		if (string.IsNullOrEmpty(assetPath) || !assetPath.EndsWith(".mat"))
			// 			return null;
			// 		else
			// 			return guid;
			// 	}).ToArray();
			//
			// 	if (guids != null && guids.Length > 0)
			// 	{
			// 		var matPath = AssetDatabase.GUIDToAssetPath(guids[0]);
			// 		Selection.activeObject = AssetDatabase.LoadAssetAtPath<Material>(matPath);
			// 		
			// 		if (guids.Length > 1)
			// 		{
			// 			Debug.LogWarning($"LWGUI: Multiple materials with the same name were found, and the first one was selected: { matPath }");
			// 		}
			// 	}
			// }
			
			return materialAsset;
		}
		
		[InitializeOnLoadMethod]
		private static void RegisterEvent()
		{
			// Register Event
			// Helper.onFindMaterialAssetInRendererByMaterialInstance = FindMaterialAssetInRendererByMaterialInstance;
		}
	}
}