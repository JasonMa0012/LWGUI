using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LWGUI
{
	public class GroupStateHelper
	{
		// Used to Folding Group, key: group name, value: is folding
		private static Dictionary<Object, Dictionary<string, bool>> _groups       = new Dictionary<Object, Dictionary<string, bool>>();
		private static Dictionary<Object, Dictionary<string, bool>> _cachedGroups = new Dictionary<Object, Dictionary<string, bool>>();

		// Used to Conditional Display, key: keyword, value: is activated
		private static Dictionary<Object, Dictionary<string, bool>> _keywords = new Dictionary<Object, Dictionary<string, bool>>();

		// TODO: clear, reset to default, expand all, collapse all
		private static void InitPoolPerMaterial(Object material)
		{
			if (!_groups.ContainsKey(material)) _groups.Add(material, new Dictionary<string, bool>());
			if (!_cachedGroups.ContainsKey(material)) _cachedGroups.Add(material, new Dictionary<string, bool>());
			if (!_keywords.ContainsKey(material)) _keywords.Add(material, new Dictionary<string, bool>());
		}

		public static bool ContainsGroup(Object material, string group)
		{
			InitPoolPerMaterial(material);
			return _groups[material].ContainsKey(group);
		}

		public static void SetGroupFolding(Object material, string group, bool isFolding)
		{
			InitPoolPerMaterial(material);
			_groups[material][group] = isFolding;
		}

		public static bool GetGroupFolding(Object material, string group)
		{
			InitPoolPerMaterial(material);
			Debug.Assert(_groups[material].ContainsKey(group), "Unknown Group: " + group);
			return _groups[material][group];
		}

		public static void SetAllGroupFoldingAndCache(Object material, bool isFolding)
		{
			InitPoolPerMaterial(material);
			_cachedGroups[material] = new Dictionary<string, bool>(_groups[material]);
			foreach (var group in _groups[material].Keys.ToArray())
			{
				_groups[material][group] = isFolding;
			}
		}

		public static void RestoreCachedFoldingState(Object material)
		{
			InitPoolPerMaterial(material);
			_groups[material] = new Dictionary<string, bool>(_cachedGroups[material]);
		}

		public static bool IsSubVisible(Object material, string group)
		{
			if (string.IsNullOrEmpty(group) || group == "_")
				return true;

			InitPoolPerMaterial(material);

			// common sub
			if (_groups[material].ContainsKey(group))
			{
				return !_groups[material][group];
			}
			// existing suffix, may be based on the enum conditions sub
			else
			{
				foreach (var prefix in _groups[material].Keys)
				{
					// prefix = group name
					if (group.Contains(prefix))
					{
						string suffix = group.Substring(prefix.Length, group.Length - prefix.Length).ToUpperInvariant();
						return _keywords[material].Keys.Any((keyword =>
																keyword.Contains(suffix)      // fuzzy matching keyword and suffix
															 && _keywords[material][keyword]  // keyword is enabled
															 && !_groups[material][prefix])); // group is not folding
					}
				}
				return false;
			}
		}

		public static void SetKeywordConditionalDisplay(Object material, string keyword, bool isDisplay)
		{
			if (string.IsNullOrEmpty(keyword) || keyword == "_") return;
			InitPoolPerMaterial(material);
			_keywords[material][keyword] = isDisplay;
		}
	}
}