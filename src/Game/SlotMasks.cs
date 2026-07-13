using System;

namespace M00StowIt.Game;

internal static class SlotMasks
{
	// True in the mask means StashItems ignores the slot: locked slots, empty
	// slots (checked live, because earlier passes empty slots as they run),
	// and slots the filter rejects. The filter receives the slot index so
	// callers can match against identities computed once per operation.
	public static PackedBoolArray BuildIgnoredMask(
		XUiController[] slots, PackedBoolArray locked, Func<int, bool> allowSlot)
	{
		var mask = new PackedBoolArray(slots.Length);
		for (int i = 0; i < slots.Length; i++)
		{
			bool ignore = locked != null && i < locked.Length && locked[i];
			if (!ignore)
			{
				ItemStack itemStack = ((XUiC_ItemStack)slots[i]).ItemStack;
				ignore = itemStack == null || itemStack.IsEmpty() || (allowSlot != null && !allowSlot(i));
			}
			mask[i] = ignore;
		}
		return mask;
	}
}
