// Copyright (c) Jason Ma

namespace LWGUI
{
	[LwguiDrawerCategory("Enum")]
	public class SubKeywordEnumDrawer : KWEnumDrawer
	{
		public SubKeywordEnumDrawer(string group, string kw1, string kw2)
			: base(group, new[] { kw1, kw2 }, new[] { kw1, kw2 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3)
			: base(group, new[] { kw1, kw2, kw3 }, new[] { kw1, kw2, kw3 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4)
			: base(group, new[] { kw1, kw2, kw3, kw4 }, new[] { kw1, kw2, kw3, kw4 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5 }, new[] { kw1, kw2, kw3, kw4, kw5 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8 }) { }

		public SubKeywordEnumDrawer(string group, string kw1, string kw2, string kw3, string kw4, string kw5, string kw6, string kw7, string kw8, string kw9)
			: base(group, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }, new[] { kw1, kw2, kw3, kw4, kw5, kw6, kw7, kw8, kw9 }) { }

		protected override string GetKeywordName(string propName, string name) { return (propName + "_" + name).Replace(' ', '_').ToUpperInvariant(); }
	}
}

