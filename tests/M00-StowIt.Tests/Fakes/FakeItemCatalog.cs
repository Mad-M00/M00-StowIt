using System;
using System.Collections.Generic;
using M00StowIt.Core;

namespace M00StowIt.Tests.Fakes;

// In-memory stand-in for the game's item list. Mirrors GameItemCatalog's
// indexing rules: display names index only when they differ from the internal
// name, and the known-group set is derived from the items themselves.
internal sealed class FakeItemCatalog : IItemCatalog
{
	private readonly List<CatalogItem> items = new();
	private readonly Dictionary<string, CatalogItem> byName = new(StringComparer.Ordinal);
	private readonly Dictionary<string, List<CatalogItem>> byDisplay = new(StringComparer.Ordinal);
	private readonly HashSet<string> groupsLower = new(StringComparer.Ordinal);
	private readonly SortedSet<string> groupNames = new(StringComparer.OrdinalIgnoreCase);

	public FakeItemCatalog(params CatalogItem[] catalogItems)
	{
		foreach (CatalogItem item in catalogItems)
		{
			items.Add(item);
			byName[item.NameLower] = item;
			if (!item.DisplayName.Equals(item.InternalName, StringComparison.OrdinalIgnoreCase))
			{
				string displayLower = item.DisplayName.Trim().ToLowerInvariant();
				if (!byDisplay.TryGetValue(displayLower, out var list))
				{
					list = new List<CatalogItem>();
					byDisplay[displayLower] = list;
				}
				list.Add(item);
			}
			foreach (string group in item.GroupsLower)
			{
				groupsLower.Add(group);
				groupNames.Add(group);
			}
		}
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
		return byDisplay.TryGetValue(displayLower, out var list) ? list : null;
	}
}
