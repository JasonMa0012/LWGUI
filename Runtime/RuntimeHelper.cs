// Copyright (c) Jason Ma

namespace LWGUI.Runtime
{
	public static class RuntimeHelper
	{
		public static bool IsBitEnabled(int intValue, int bitIndex) => (intValue & 1U << bitIndex) > 0;
		
		public static int SetBitEnabled(int intValue, int bitIndex, bool enabled)
			=> enabled ? intValue | (int)(1U << bitIndex) : intValue & ~(int)(1U << bitIndex);
	}
}