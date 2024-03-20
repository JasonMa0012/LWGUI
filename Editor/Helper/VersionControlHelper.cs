// Copyright (c) Jason Ma

using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace LWGUI
{
	public class VersionControlHelper
	{
		public static bool isVCEnabled { get { return Provider.enabled && Provider.isActive; } }

		public static bool Checkout(UnityEngine.Object obj)
		{
			return Checkout(new[] { obj });
		}

		public static bool Checkout(UnityEngine.Object[] objs)
		{
			List<string> pathes = new List<string>();
			foreach (var obj in objs)
			{
				if (AssetDatabase.Contains(obj))
					pathes.Add(AssetDatabase.GetAssetPath(obj));
			}
			return Checkout(pathes);
		}

		public static bool Checkout(string path)
		{
			return Checkout(new List<string>() { path });
		}

		public static bool Checkout(List<string> pathes)
		{
			if (!isVCEnabled) return true;

			AssetList assetList = new AssetList();
			foreach (string path in pathes)
			{
				Asset asset = Provider.GetAssetByPath(path);
				if (asset != null && !Provider.IsOpenForEdit(asset))
				{
					assetList.Add(asset);
				}
			}

			if (assetList.Count == 0) return true;

			StringBuilder sb = new StringBuilder();
			assetList.ForEach((asset => sb.AppendLine(asset.path)));
			Debug.Log($"LWGUI: {assetList.Count} assets to be checked out:\n{sb}");
			sb.Clear();

			var checkOutTask = Provider.Checkout(assetList, CheckoutMode.Both);
			checkOutTask.Wait();
			if (checkOutTask.success)
			{
				return true;
			}
			else
			{
				assetList.ForEach((asset =>
				{
					if (!Provider.IsOpenForEdit(asset))
						sb.AppendLine(asset.path);
				}));
				Debug.LogError($"LWGUI: {assetList.Count} asssets failed to be checked out!\n{sb}");
				return false;
			}
		}

		public static bool Add(string projectRelativedPath)
		{
			if (isVCEnabled)
			{
				var vcAsset = Provider.GetAssetByPath(projectRelativedPath);
				if (vcAsset != null)
				{
					var statusTask = Provider.Status(vcAsset);
					statusTask.Wait();
					if (Provider.AddIsValid(statusTask.assetList))
					{
						var addTask = Provider.Add(vcAsset, false);
						addTask.Wait();
						if (addTask.success)
						{
							return true;
						}
					}
				}

				Debug.LogError($"LWGUI: Failed to add {projectRelativedPath}!");
				return false;
			}

			return true;
		}
	}
}