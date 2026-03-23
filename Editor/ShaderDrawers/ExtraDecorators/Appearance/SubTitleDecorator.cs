// Copyright (c) Jason Ma

namespace LWGUI
{
	/// <summary>
	/// Similar to Title()
	/// 
	/// group: parent group name (Default: none)
	/// header: string to display, "SpaceLine" or "_" = none (Default: none)
	/// height: line height (Default: 22)
	/// </summary>
	[DrawerCategory("Appearance")]
	public class SubTitleDecorator : TitleDecorator
	{
		public SubTitleDecorator(string group, string header) : base(group, header, DefaultHeight) { }

		public SubTitleDecorator(string group, string header, float height) : base(group, header, height) { }
	}
}
