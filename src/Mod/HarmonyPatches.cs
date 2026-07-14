using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using M00StowIt.Game;
using UnityEngine;

namespace M00StowIt.Mod;

// Thin Harmony entry points; behaviour lives in the Game/Core layers behind
// StowItMod.Runtime. Patch method parameter names must match the game's
// method signatures — Harmony binds them by name.
internal class HarmonyPatches
{
	private static ModRuntime Runtime => StowItMod.Runtime;

	[HarmonyPatch(typeof(XUiC_BackpackWindow), "Init")]
	private class BackpackWindow_Init
	{
		public static void Postfix(XUiC_BackpackWindow __instance)
		{
			try
			{
				Runtime.Ui.Capture(__instance);
				XUiController button = Runtime.Ui.Controls.GetChildById("btnStow");
				if (button != null)
				{
					button.OnPress += delegate
					{
						Runtime.Stash.RequestSort();
					};
				}
				button = Runtime.Ui.Controls.GetChildById("btnRestock");
				if (button != null)
				{
					button.OnPress += delegate
					{
						Runtime.Stash.RequestRestock();
					};
				}
				Runtime.PrepareAliasRules();
			}
			catch (Exception e)
			{
				LogException(e);
			}
		}
	}

	[HarmonyPatch(typeof(XUiC_BackpackWindow), "GetBindingValueInternal")]
	private class BackpackWindow_GetBindingValueInternal
	{
		public static void Postfix(ref bool __result, XUiC_BackpackWindow __instance, ref string value, string bindingName)
		{
			try
			{
				if (!__result && bindingName == "notlootingorvehiclestorage")
				{
					value = (!__instance.TryGetMoveDestinationInventory(out var _)).ToString();
					__result = true;
				}
			}
			catch (Exception e)
			{
				LogException(e);
			}
		}
	}

	[HarmonyPatch(typeof(EntityPlayerLocal), "Update")]
	private class EntityPlayerLocal_Update
	{
		public static void Postfix(EntityPlayerLocal __instance)
		{
			try
			{
				Runtime.KeyGuard.Update(__instance.playerInput);
				if (HotkeyChecker.ComboJustPressed(Runtime.Settings.SortKeyCodes))
				{
					Runtime.Stash.RequestSort();
					UiSounds.PlayClick();
				}
				else if (HotkeyChecker.ComboJustPressed(Runtime.Settings.RestockKeyCodes))
				{
					Runtime.Stash.RequestRestock();
					UiSounds.PlayClick();
				}
			}
			catch (Exception e)
			{
				LogException(e);
			}
		}
	}

	// Belt and braces for the editor selection tool specifically: skip its
	// key handling while a StowIt modifier is held, mirroring the Ctrl guard
	// the method already has. A Harmony prefix runs before the method body
	// regardless of patch ordering, so this holds even if another mod reads
	// the selection actions in ways the KeyGuard cannot reach.
	[HarmonyPatch(typeof(BlockToolSelection), "CheckKeys")]
	private class BlockToolSelection_CheckKeys
	{
		public static bool Prefix()
		{
			return !Runtime.KeyGuard.Active;
		}
	}

	[HarmonyPatch(typeof(LockManager), "LockResponse")]
	private class LockManager_LockResponse
	{
		public static void Postfix(bool _success, string _errorMsg, ReadOnlySpan<ILockTarget> _targets, ILockContext _context, ushort _channel)
		{
			Runtime.Stash.HandleLockResponse(_success, _targets);
		}
	}

	[HarmonyPatch(typeof(LockManager), "UnlockResponse")]
	private class LockManager_UnlockResponse
	{
		public static void Postfix(bool _success, string _errorMsg, bool _isForceUnlocked)
		{
			Runtime.Stash.HandleUnlockResponse();
		}
	}

	// Suppress the container-locked UI popup while a sort operation holds the locks.
	[HarmonyPatch(typeof(TEFeatureStorage), "OnLockedLocal")]
	private class TEFeatureStorage_OnLockedLocal
	{
		public static bool Prefix(bool _success, ILockContext _context, ushort _channel)
		{
			return !Runtime.Operations.InProgress;
		}
	}

	[HarmonyPatch(typeof(GUIWindowManager), "CloseAllOpenModalWindows", new Type[]
	{
		typeof(GUIWindow),
		typeof(bool)
	})]
	private class GUIWindowManager_CloseAllOpenModalWindows
	{
		public static bool Prefix(GUIWindow _exceptWindow, bool _fromEsc)
		{
			return !Runtime.Operations.InProgress;
		}
	}

	// Release world-bound references (UI controllers, item catalog) when the
	// player leaves the world, so the static runtime does not pin the dead
	// world's objects in memory or reuse stale item data in the next world.
	[HarmonyPatch(typeof(GameManager), "SaveAndCleanupWorld")]
	private class GameManager_SaveAndCleanupWorld
	{
		public static void Postfix()
		{
			try
			{
				Runtime.HandleWorldUnloaded();
			}
			catch (Exception e)
			{
				LogException(e);
			}
		}
	}

	// Vanilla caps a lock request at 5 targets; raise the cap so every nearby
	// container can be locked in one operation.
	[HarmonyPatch(typeof(LockManager), "LockRequestServer")]
	private class LockManager_LockRequestServer
	{
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
		{
			foreach (CodeInstruction instruction in instructions)
			{
				if (instruction.opcode == OpCodes.Ldc_I4_5)
				{
					yield return new CodeInstruction(OpCodes.Ldc_I4, int.MaxValue);
				}
				else
				{
					yield return instruction;
				}
			}
		}
	}

	private static void LogException(Exception e)
	{
		Runtime.Log.Warning(e.Message);
		Runtime.Log.Warning(e.StackTrace);
	}
}
