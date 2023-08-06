using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LWGUI
{
	public class GroupStateHelper
	{
		// Used to Folding Group, key: group name, value: is folding
		private static Dictionary<Shader, Dictionary<string, bool>> _groups       = new Dictionary<Shader, Dictionary<string, bool>>();
		private static Dictionary<Shader, Dictionary<string, bool>> _cachedGroups = new Dictionary<Shader, Dictionary<string, bool>>();

		// Used to Conditional Display, key: keyword, value: is activated
		private static Dictionary<Shader, Dictionary<string, bool>> _keywords = new Dictionary<Shader, Dictionary<string, bool>>();

		// TODO: clear, reset to default, expand all, collapse all
		private static void InitPoolPerMaterial(Shader shader)
		{
			if (!_groups.ContainsKey(shader)) _groups.Add(shader, new Dictionary<string, bool>());
			if (!_cachedGroups.ContainsKey(shader)) _cachedGroups.Add(shader, new Dictionary<string, bool>());
			if (!_keywords.ContainsKey(shader)) _keywords.Add(shader, new Dictionary<string, bool>());
		}

		public static bool ContainsGroup(Shader shader, string group)
		{
			InitPoolPerMaterial(shader);
			return _groups[shader].ContainsKey(group);
		}

		public static void SetGroupFolding(Shader shader, string group, bool isFolding)
		{
			InitPoolPerMaterial(shader);
			_groups[shader][group] = isFolding;
		}

		public static bool GetGroupFolding(Shader shader, string group)
		{
			InitPoolPerMaterial(shader);
			Debug.Assert(_groups[shader].ContainsKey(group), "Unknown Group: " + group);
			return _groups[shader][group];
		}

		public static void SetAllGroupFoldingAndCache(Shader shader, bool isFolding)
		{
			InitPoolPerMaterial(shader);
			_cachedGroups[shader] = new Dictionary<string, bool>(_groups[shader]);
			foreach (var group in _groups[shader].Keys.ToArray())
			{
				_groups[shader][group] = isFolding;
			}
		}

		public static void RestoreCachedFoldingState(Shader shader)
		{
			InitPoolPerMaterial(shader);
			_groups[shader] = new Dictionary<string, bool>(_cachedGroups[shader]);
		}

		public static bool IsSubVisible(Shader shader, string group)
		{
			if (string.IsNullOrEmpty(group) || group == "_")
				return true;

			InitPoolPerMaterial(shader);

			// common sub
			if (_groups[shader].ContainsKey(group))
			{
				return !_groups[shader][group];
			}
			// existing suffix, may be based on the enum conditions sub
			else
			{
				foreach (var prefix in _groups[shader].Keys)
				{
					// prefix = group name
					if (group.Contains(prefix))
					{
						string suffix = group.Substring(prefix.Length, group.Length - prefix.Length).ToUpperInvariant();
						return _keywords[shader].Keys.Any((keyword =>
																keyword.Contains(suffix)      // fuzzy matching keyword and suffix
															 && _keywords[shader][keyword]  // keyword is enabled
															 && !_groups[shader][prefix])); // group is not folding
					}
				}
				return false;
			}
		}

		public static void SetKeywordConditionalDisplay(Shader shader, string keyword, bool isDisplay)
		{
			if (string.IsNullOrEmpty(keyword) || keyword == "_") return;
			InitPoolPerMaterial(shader);
			_keywords[shader][keyword] = isDisplay;
		}
	}
}