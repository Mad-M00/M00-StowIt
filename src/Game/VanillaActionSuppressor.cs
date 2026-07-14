using System;
using System.Collections.Generic;
using InControl;
using M00StowIt.Core;
using UnityEngine;

namespace M00StowIt.Game;

// While the modifier prefix of a StowIt hotkey is held, the vanilla actions
// that collide with the combo's trigger keys are disabled so the game does
// not act on the same key press. With the debug menu on, Z is SelectionSet
// and X is SelectionRotate: without this, LeftAlt+Z plants the world
// editor's blue selection box at the crosshair. InControl checks Enabled at
// read time, so the disable takes effect for every consumer immediately.
// Each action's previous Enabled value is restored on release, so another
// mod toggling the same actions is not stomped.
internal sealed class VanillaActionSuppressor
{
	private readonly Func<StowItSettings> getSettings;
	private readonly List<(PlayerAction action, bool wasEnabled)> suppressed = new();

	public VanillaActionSuppressor(Func<StowItSettings> getSettings)
	{
		this.getSettings = getSettings;
	}

	public bool Active { get; private set; }

	// Called every frame from the EntityPlayerLocal.Update patch.
	public void Update(PlayerActionsLocal playerInput)
	{
		StowItSettings settings = getSettings();
		bool prefixHeld = PrefixHeld(settings.SortKeyCodes) || PrefixHeld(settings.RestockKeyCodes);
		if (prefixHeld == Active)
		{
			return;
		}
		if (prefixHeld)
		{
			Suppress(playerInput.SelectionSet);
			Suppress(playerInput.SelectionRotate);
		}
		else
		{
			RestoreAll();
		}
		Active = prefixHeld;
	}

	// Restores immediately, e.g. when the world unloads mid-hold.
	public void Reset()
	{
		RestoreAll();
		Active = false;
	}

	// Every key of the combo except the last; the last is the trigger. A
	// single-key combo has no prefix, so nothing is suppressed for it.
	private static bool PrefixHeld(int[] keyCodes)
	{
		if (keyCodes == null || keyCodes.Length < 2)
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

	private void Suppress(PlayerAction action)
	{
		if (action == null)
		{
			return;
		}
		suppressed.Add((action, action.Enabled));
		action.Enabled = false;
	}

	private void RestoreAll()
	{
		foreach ((PlayerAction action, bool wasEnabled) in suppressed)
		{
			action.Enabled = wasEnabled;
		}
		suppressed.Clear();
	}
}
