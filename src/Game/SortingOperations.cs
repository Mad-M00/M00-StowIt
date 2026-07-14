using System;
using System.Collections.Generic;
using M00StowIt.Core;
using UnityEngine;

namespace M00StowIt.Game;

// Drives a full sort operation: request container locks, and when the game
// confirms them, execute the stash/restock and release the locks. Category
// routing follows the pass plan from StashPassPlanner (Core).
internal sealed class SortingOperations
{
	private readonly PlayerUiContext ui;
	private readonly OperationTracker operations;
	private readonly ContainerScanner scanner;
	private readonly ActionRepeatTracker repeats;
	private readonly ISorterLog log;
	private readonly Func<StowItSettings> getSettings;
	private readonly Func<AliasConfiguration> getAliases;
	private readonly Func<CategoryRuleResolver> getResolver;
	private readonly Func<GameItemCatalog> getCatalog;
	private readonly Action prepareAliasRules;

	public SortingOperations(
		PlayerUiContext ui,
		OperationTracker operations,
		ContainerScanner scanner,
		ActionRepeatTracker repeats,
		ISorterLog log,
		Func<StowItSettings> getSettings,
		Func<AliasConfiguration> getAliases,
		Func<CategoryRuleResolver> getResolver,
		Func<GameItemCatalog> getCatalog,
		Action prepareAliasRules)
	{
		this.ui = ui;
		this.operations = operations;
		this.scanner = scanner;
		this.repeats = repeats;
		this.log = log;
		this.getSettings = getSettings;
		this.getAliases = getAliases;
		this.getResolver = getResolver;
		this.getCatalog = getCatalog;
		this.prepareAliasRules = prepareAliasRules;
	}

	public void RequestSort()
	{
		if (operations.InProgress)
		{
			return;
		}
		prepareAliasRules();
		TEFeatureStorage[] containers = scanner.FindNearby();
		if (containers.Length != 0)
		{
			operations.Current = SortOperation.Sort;
			LockManager.Instance.LockRequestLocal(containers, null, 0);
		}
	}

	public void RequestRestock()
	{
		if (operations.InProgress)
		{
			return;
		}
		TEFeatureStorage[] containers = scanner.FindNearby();
		if (containers.Length != 0)
		{
			operations.Current = SortOperation.Restock;
			LockManager.Instance.LockRequestLocal(containers, null, 0);
		}
	}

	// Called from the LockManager.LockResponse patch once the game has locked
	// (or failed to lock) the requested containers.
	public void HandleLockResponse(bool success, ReadOnlySpan<ILockTarget> targets)
	{
		if (!operations.InProgress)
		{
			return;
		}
		try
		{
			if (!success)
			{
				operations.Current = SortOperation.None;
			}
			else if (operations.Current == SortOperation.Sort)
			{
				ExecuteSort(targets);
			}
			else
			{
				ExecuteRestock(targets);
			}
		}
		catch (Exception e)
		{
			log.Warning(e.Message);
			log.Warning(e.StackTrace);
			try
			{
				LockManager.Instance.UnlockRequestLocal();
			}
			catch
			{
			}
			operations.Current = SortOperation.None;
		}
	}

	public void HandleUnlockResponse()
	{
		operations.Current = SortOperation.None;
	}

	private void ExecuteSort(ReadOnlySpan<ILockTarget> containers)
	{
		StowItSettings settings = getSettings();
		if (settings.Routing == RoutingMode.Category)
		{
			StashByCategory(containers, settings);
		}
		else
		{
			XUiM_LootContainer.EItemMoveKind moveKind = MoveKindFor(SortOperation.Sort);
			foreach (ILockTarget target in containers)
			{
				var crate = (TEFeatureStorage)target;
				XUiM_LootContainer.StashItems(ui.BackpackWindow, ui.Backpack, crate, 0,
					ui.Controls.LockedSlots, moveKind, ui.Controls.MoveStartBottomRight);
				crate.SetModified();
			}
		}
		LockManager.Instance.UnlockRequestLocal();
	}

	private void StashByCategory(ReadOnlySpan<ILockTarget> containers, StowItSettings settings)
	{
		XUiController[] slots = ui.Backpack.GetItemStackControllers();
		PackedBoolArray lockedSlots = ui.Controls.LockedSlots;
		string fallbackLabel = string.IsNullOrEmpty(settings.FallbackCrateName)
			? null
			: settings.FallbackCrateName.Trim().ToLowerInvariant();
		CategoryRuleResolver resolver = getResolver();
		GameItemCatalog catalog = getCatalog();

		int count = containers.Length;
		var crates = new TEFeatureStorage[count];
		var crateRules = new IReadOnlyList<CategoryRule>[count];
		var routing = new CrateRoutingInfo[count];
		AliasConfiguration aliases = getAliases();
		for (int i = 0; i < count; i++)
		{
			crates[i] = (TEFeatureStorage)containers[i];
			IReadOnlyList<string> labels = CrateSigns.ReadLabels(crates[i], aliases);
			bool isFallback = fallbackLabel != null && CrateSigns.ContainsLabel(labels, fallbackLabel);
			crateRules[i] = (!isFallback && resolver != null)
				? resolver.ResolveMany(labels)
				: Array.Empty<CategoryRule>();
			bool hasItemRules = false;
			bool hasGroupRules = false;
			int priority = int.MaxValue;
			foreach (CategoryRule rule in crateRules[i])
			{
				hasItemRules |= rule.HasItemRules;
				hasGroupRules |= rule.HasGroupRules;
				priority = Math.Min(priority, rule.Priority);
			}
			routing[i] = new CrateRoutingInfo(isFallback, hasItemRules, hasGroupRules, priority);
		}

		var allRules = new List<CategoryRule>();
		foreach (IReadOnlyList<CategoryRule> rules in crateRules)
		{
			allRules.AddRange(rules);
		}

		// Identify every backpack slot once. Slot contents never change type
		// during the operation (StashItems only removes from the backpack),
		// so the per-pass mask only needs the live emptiness check.
		var slotItems = new CatalogItem[slots.Length];
		var slotRoutedByAnyCrate = new bool[slots.Length];
		for (int i = 0; i < slots.Length; i++)
		{
			slotItems[i] = Identify(((XUiC_ItemStack)slots[i]).ItemStack, catalog);
			slotRoutedByAnyCrate[i] = slotItems[i] != null && IsRouted(slotItems[i], allRules);
		}

		foreach (StashPass pass in StashPassPlanner.Plan(routing))
		{
			IReadOnlyList<CategoryRule> rules = crateRules[pass.CrateIndex];
			Func<int, bool> allowSlot = pass.Kind switch
			{
				StashPassKind.TopUpExistingStacks => i => slotItems[i] == null
					|| TopUpPolicy.ShouldTopUp(IsRouted(slotItems[i], rules), slotRoutedByAnyCrate[i]),
				StashPassKind.TakeItemRuleMatches => i => RulesMatch(slotItems[i], rules, MatchTier.Item),
				StashPassKind.TakeGroupRuleMatches => i => RulesMatch(slotItems[i], rules, MatchTier.Group),
				_ => null
			};
			XUiM_LootContainer.EItemMoveKind moveKind = pass.Kind == StashPassKind.TopUpExistingStacks
				? XUiM_LootContainer.EItemMoveKind.FillAndCreate
				: XUiM_LootContainer.EItemMoveKind.All;
			PackedBoolArray ignoredSlots = SlotMasks.BuildIgnoredMask(slots, lockedSlots, allowSlot);
			TEFeatureStorage crate = crates[pass.CrateIndex];
			XUiM_LootContainer.StashItems(ui.BackpackWindow, ui.Backpack, crate, 0,
				ignoredSlots, moveKind, ui.Controls.MoveStartBottomRight);
			crate.SetModified();
		}
	}

	private static bool RulesMatch(CatalogItem item, IReadOnlyList<CategoryRule> rules, MatchTier tier)
	{
		if (item == null)
		{
			return false;
		}
		foreach (CategoryRule rule in rules)
		{
			if (rule.Matches(item, tier))
			{
				return true;
			}
		}
		return false;
	}

	private static bool IsRouted(CatalogItem item, IReadOnlyList<CategoryRule> rules)
	{
		foreach (CategoryRule rule in rules)
		{
			if (rule.Matches(item, MatchTier.Item) || rule.Matches(item, MatchTier.Group))
			{
				return true;
			}
		}
		return false;
	}

	private static CatalogItem Identify(ItemStack stack, GameItemCatalog catalog)
	{
		if (stack == null || stack.IsEmpty() || catalog == null)
		{
			return null;
		}
		string nameLower = stack.itemValue?.ItemClass?.GetItemName()?.ToLowerInvariant();
		return nameLower == null ? null : catalog.FindByInternalName(nameLower);
	}

	private void ExecuteRestock(ReadOnlySpan<ILockTarget> containers)
	{
		bool createNewStacks =
			MoveKindFor(SortOperation.Restock) == XUiM_LootContainer.EItemMoveKind.FillAndCreate;
		IInventory playerInventory = LocalPlayerUI.GetUIForPrimaryPlayer().mXUi.PlayerInventory;
		int itemsMoved = 0;
		int cratesUsed = 0;
		foreach (ILockTarget target in containers)
		{
			int moved = RestockFromCrate((TEFeatureStorage)target, playerInventory, createNewStacks);
			if (moved > 0)
			{
				itemsMoved += moved;
				cratesUsed++;
			}
		}
		log.Info($"Restock ({(createNewStacks ? "fill and create" : "top-up only")}): " +
			$"moved {itemsMoved} item(s) from {cratesUsed} of {containers.Length} crate(s)");
		LockManager.Instance.UnlockRequestLocal();
	}

	// Reads the crate's stacks directly, mirroring the slot loop of
	// XUiM_LootContainer.StashItems. StashItems needs a UI grid as its source,
	// and the only grid bindable to a crate is the loot window's - but that
	// window is a live UI surface: binding it turns its slot views visible,
	// and with no container open the restore rebind is a no-op, leaving a
	// ghost grid on screen (issue #1). Changes propagate exactly as the
	// borrowed grid did: UpdateSlot + SetModified under the held lock.
	private int RestockFromCrate(TEFeatureStorage crate, IInventory playerInventory, bool createNewStacks)
	{
		ItemStack[] stacks = crate.items;
		PackedBoolArray slotLocks = crate.HasSlotLocksSupport ? crate.SlotLocks : null;
		bool startBottomRight = ui.Controls.MoveStartBottomRight;
		int itemsMoved = 0;
		int i = startBottomRight ? stacks.Length - 1 : 0;
		while (startBottomRight ? i >= 0 : i < stacks.Length)
		{
			bool slotLocked = slotLocks != null && i < slotLocks.Length && slotLocks[i];
			ItemStack stack = stacks[i];
			if (!slotLocked && stack != null && !stack.IsEmpty())
			{
				int countBefore = stack.count;
				playerInventory.TryStackItem(0, stack);
				// A successful AddItem hands the stack object to the player
				// inventory, so the slot is cleared with a fresh empty stack.
				bool movedWholeStack = stack.count == 0
					|| (createNewStacks && playerInventory.HasItem(stack.itemValue)
						&& playerInventory.AddItem(stack));
				if (movedWholeStack)
				{
					crate.UpdateSlot(i, ItemStack.Empty.Clone());
					itemsMoved += countBefore;
				}
				else if (stack.count != countBefore)
				{
					crate.UpdateSlot(i, stack);
					itemsMoved += countBefore - stack.count;
				}
			}
			i = startBottomRight ? i - 1 : i + 1;
		}
		if (itemsMoved > 0)
		{
			crate.SetModified();
		}
		return itemsMoved;
	}

	// A quick second use of the same action escalates from topping up existing
	// stacks to also creating new ones.
	private XUiM_LootContainer.EItemMoveKind MoveKindFor(SortOperation operation)
	{
		return repeats.IsRepeat((int)operation, Time.unscaledTime)
			? XUiM_LootContainer.EItemMoveKind.FillAndCreate
			: XUiM_LootContainer.EItemMoveKind.FillOnly;
	}
}
