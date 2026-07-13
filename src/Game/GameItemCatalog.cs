using System;
using System.Collections.Generic;
using M00StowIt.Core;

namespace M00StowIt.Game;

// IItemCatalog over the game's ItemClass.list (items, blocks and modifiers,
// with group inheritance already applied by the game). Unavailable until a
// world is loaded; the runtime retries creation lazily.
internal sealed class GameItemCatalog : IItemCatalog
{
	private readonly List<CatalogItem> items;
	private readonly Dictionary<string, CatalogItem> byName;
	private readonly Dictionary<string, List<CatalogItem>> byDisplay;
	private readonly HashSet<string> groupsLower;
	private readonly SortedSet<string> groupNames;

	private GameItemCatalog(
		List<CatalogItem> items,
		Dictionary<string, CatalogItem> byName,
		Dictionary<string, List<CatalogItem>> byDisplay,
		HashSet<string> groupsLower,
		SortedSet<string> groupNames)
	{
		this.items = items;
		this.byName = byName;
		this.byDisplay = byDisplay;
		this.groupsLower = groupsLower;
		this.groupNames = groupNames;
	}

	public static GameItemCatalog TryCreate(ISorterLog log)
	{
		if (ItemClass.list == null)
		{
			return null;
		}
		var items = new List<CatalogItem>();
		var byName = new Dictionary<string, CatalogItem>(StringComparer.Ordinal);
		var byDisplay = new Dictionary<string, List<CatalogItem>>(StringComparer.Ordinal);
		var groupsLower = new HashSet<string>(StringComparer.Ordinal);
		var groupNames = new SortedSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (ItemClass itemClass in ItemClass.list)
		{
			string name = itemClass?.GetItemName();
			if (string.IsNullOrEmpty(name))
			{
				continue;
			}
			var item = new CatalogItem(name, DisplayNameFor(itemClass, name), itemClass.Groups);
			items.Add(item);
			byName[item.NameLower] = item;
			if (!item.DisplayName.Equals(name, StringComparison.OrdinalIgnoreCase))
			{
				string displayLower = item.DisplayName.Trim().ToLowerInvariant();
				if (!byDisplay.TryGetValue(displayLower, out var sameDisplay))
				{
					sameDisplay = new List<CatalogItem>();
					byDisplay[displayLower] = sameDisplay;
				}
				sameDisplay.Add(item);
			}
			if (itemClass.Groups != null)
			{
				foreach (string group in itemClass.Groups)
				{
					if (!string.IsNullOrEmpty(group))
					{
						groupsLower.Add(group.Trim().ToLowerInvariant());
						groupNames.Add(group.Trim());
					}
				}
			}
		}
		if (items.Count == 0)
		{
			return null;
		}
		log.Info($"Indexed {items.Count} items and {groupNames.Count} groups for category resolution");
		return new GameItemCatalog(items, byName, byDisplay, groupsLower, groupNames);
	}

	private static string DisplayNameFor(ItemClass itemClass, string internalName)
	{
		string display = itemClass.GetLocalizedItemName();
		if (string.IsNullOrEmpty(display))
		{
			display = Localization.Get(internalName);
		}
		return display;
	}

	public IReadOnlyList<CatalogItem> Items => items;

	public IReadOnlyCollection<string> GroupNames => groupNames;

	public bool IsKnownGroup(string groupLower) => groupsLower.Contains(groupLower);

	public CatalogItem FindByInternalName(string nameLower)
	{
		return byName.TryGetValue(nameLower, out var item) ? item : null;
	}

	public IReadOnlyList<CatalogItem> FindByDisplayName(string displayLower)
	{
		return byDisplay.TryGetValue(displayLower, out var sameDisplay) ? sameDisplay : null;
	}
}
