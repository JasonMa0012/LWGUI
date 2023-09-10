// Copyright (c) Jason Ma
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	public class ShaderModifyListener : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
		{
			foreach (var assetPath in importedAssets)
			{
				if (Path.GetExtension(assetPath).Equals(".shader", StringComparison.OrdinalIgnoreCase))
				{
					var shader = AssetDatabase.LoadAssetAtPath<Shader>(assetPath);
					MetaDataHelper.ForceRebuildPerShaderData(shader);
				}
			}
		}
	}
}