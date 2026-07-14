// Copyright (c) Jason Ma

using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	[CustomEditor(typeof(LwguiRampAtlas), true)]
	public class LwguiRampAtlasEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			ForceExpandAllListElements();

			DrawDefaultInspector();

			EditorGUILayout.Space();

			var rampAtlas = (LwguiRampAtlas)target;

			if (rampAtlas.TotalRowCount > rampAtlas.rampAtlasHeight)
			{
				EditorGUILayout.HelpBox(
					$"Atlas height ({rampAtlas.rampAtlasHeight}) is less than total row count ({rampAtlas.TotalRowCount}). " +
					"Please increase the height before saving.",
					MessageType.Warning);
			}

			using (new EditorGUI.DisabledScope(!AssetDatabase.Contains(rampAtlas)))
			{
				if (GUILayout.Button("Save Ramp Atlas Texture", GUILayout.Height(30)))
				{
					rampAtlas.SaveTextureWithCheckout();
				}
			}
		}

		private void ForceExpandAllListElements()
		{
			var iter = serializedObject.GetIterator();
			bool enterChildren = true;
			while (iter.NextVisible(enterChildren))
			{
				enterChildren = true;
				if (!iter.isArray || iter.propertyType != SerializedPropertyType.Generic || !iter.isExpanded)
					continue;

				for (int i = 0; i < iter.arraySize; i++)
					iter.GetArrayElementAtIndex(i).isExpanded = true;

				enterChildren = false;
			}
		}
	}
}
