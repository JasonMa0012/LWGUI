// Copyright (c) Jason Ma

using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace LWGUI
{
	// UnityEditor.MaterialEnumDrawer(string enumName)
	// enumName: like "UnityEngine.Rendering.BlendMode"
	[LwguiDrawerCategory("Enum")]
	public class SubEnumDrawer : KWEnumDrawer
	{
		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2)
			: base(group, new[] { n1, n2 }, null, new[] { v1, v2 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3)
			: base(group, new[] { n1, n2, n3 }, null, new[] { v1, v2, v3 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4)
			: base(group, new[] { n1, n2, n3, n4 }, null, new[] { v1, v2, v3, v4 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5)
			: base(group, new[] { n1, n2, n3, n4, n5 }, null, new[] { v1, v2, v3, v4, v5 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6)
			: base(group, new[] { n1, n2, n3, n4, n5, n6 }, null, new[] { v1, v2, v3, v4, v5, v6 }) { }

		public SubEnumDrawer(string group, string n1, float v1, string n2, float v2, string n3, float v3, string n4, float v4, string n5, float v5, string n6, float v6, string n7, float v7)
			: base(group, new[] { n1, n2, n3, n4, n5, n6, n7 }, null, new[] { v1, v2, v3, v4, v5, v6, v7 }) { }

		public SubEnumDrawer(string group, string enumName) : base(group, enumName)
		{
			var array = ReflectionHelper.GetAllTypes();
			try
			{
				Type enumType = array.FirstOrDefault(x => x.IsSubclassOf(typeof(Enum)) && (x.Name == enumName || x.FullName == enumName));
				string[] names = Enum.GetNames(enumType);
				Array valuesArray = Enum.GetValues(enumType);
				var values = new float[valuesArray.Length];
				for (int index = 0; index < valuesArray.Length; ++index)
					values[index] = (int)valuesArray.GetValue(index);
				Init(group, names, null, values);
			}
			catch (Exception ex)
			{
				Debug.LogWarningFormat("LWGUI: Failed to create SubEnum, enum {0} not found, {1}.", enumName, ex);
				throw;
			}
		}

		protected override string GetKeywordName(string propName, string name) { return "_"; }

		public override void Apply(MaterialProperty prop) { }
	}
}

