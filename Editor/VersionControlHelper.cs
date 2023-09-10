// Copyright (c) Jason Ma
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
			if (AssetDatabase.Contains(obj))
			{
				var path = AssetDatabase.GetAssetPath(obj);
				return Checkout(path);
			}
			else
			{
				return true;
			}
		}

		public static bool Checkout(string projectRelativedPath)
		{
			if (isVCEnabled)
			{
				var vcAsset = Provider.GetAssetByPath(projectRelativedPath);
				if (vcAsset != null)
				{
					var statusTask = Provider.Status(vcAsset);
					statusTask.Wait();
					if (Provider.CheckoutIsValid(statusTask.assetList))
					{
						var checkOutTask = Provider.Checkout(vcAsset, CheckoutMode.Both);
						checkOutTask.Wait();
						if (checkOutTask.success)
						{
							return true;
						}
					}
					else if (Provider.IsOpenForEdit(vcAsset))
					{
						return true;
					}
				}
				
				Debug.LogError("Checkout '" + projectRelativedPath + "' failure!");
				return false;
			}

			return true;
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
				
				Debug.LogError("Add '" + projectRelativedPath + "' failure!");
				return false;
			}

			return true;
		}
	}
}