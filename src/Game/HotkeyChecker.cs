using UnityEngine;

namespace M00StowIt.Game;

internal static class HotkeyChecker
{
	// The last key of the combo must be freshly pressed this frame while all
	// preceding keys are held.
	public static bool ComboJustPressed(int[] keyCodes)
	{
		if (keyCodes == null || keyCodes.Length == 0)
		{
			return false;
		}
		if (!UICamera.GetKeyDown((KeyCode)keyCodes[keyCodes.Length - 1]))
		{
			return false;
		}
		for (int i = 0; i < keyCodes.Length - 1; i++)
		{
			if (!UICamera.GetKey((KeyCode)keyCodes[i]))
			{
				return false;
			}
		}
		return true;
	}
}
