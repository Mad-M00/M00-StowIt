using System.Collections.Generic;

namespace M00StowIt.Core;

// A crate label resolved into concrete matching sets. Immutable once built;
// created only by CategoryRuleResolver.
public sealed class CategoryRule
{
	private readonly HashSet<string> groups;
	private readonly HashSet<string> includeItems;
	private readonly HashSet<string> excludeItems;

	internal CategoryRule(
		string label,
		HashSet<string> groupsLower,
		HashSet<string> includeItemsLower,
		HashSet<string> excludeItemsLower,
		int priority)
	{
		Label = label;
		groups = groupsLower;
		includeItems = includeItemsLower;
		excludeItems = excludeItemsLower;
		includeItems.ExceptWith(excludeItems);
		Priority = priority;
	}

	public string Label { get; }

	// Lower value = the crate drains earlier within its routing tier; comes
	// from the label's line position in CrateLabels.txt.
	public int Priority { get; }

	internal IReadOnlyCollection<string> IncludeItemsView => includeItems;

	// Merges this rule's matchers into another rule being built - the
	// mechanism behind label-to-label references like "Dosen = @Cans".
	internal void CopyMatchersInto(
		HashSet<string> targetGroups, HashSet<string> targetIncludes, HashSet<string> targetExcludes)
	{
		targetGroups.UnionWith(groups);
		targetIncludes.UnionWith(includeItems);
		targetExcludes.UnionWith(excludeItems);
	}

	public IReadOnlyCollection<string> Groups => groups;

	public bool HasItemRules => includeItems.Count > 0;

	public bool HasGroupRules => groups.Count > 0;

	public bool IsEmpty => includeItems.Count == 0 && groups.Count == 0;

	public bool Matches(CatalogItem item, MatchTier tier)
	{
		if (item == null)
		{
			return false;
		}
		if (tier == MatchTier.Item)
		{
			return includeItems.Contains(item.NameLower);
		}
		if (!HasGroupRules || excludeItems.Contains(item.NameLower))
		{
			return false;
		}
		foreach (string group in item.GroupsLower)
		{
			if (groups.Contains(group))
			{
				return true;
			}
		}
		return false;
	}

	public string Describe()
	{
		if (IsEmpty)
		{
			return "no matches (crate will only receive stack top-ups)";
		}
		var parts = new List<string>();
		if (HasGroupRules)
		{
			parts.Add($"group(s) [{string.Join(", ", groups)}]");
		}
		if (HasItemRules)
		{
			parts.Add($"{includeItems.Count} item(s)");
		}
		if (excludeItems.Count > 0)
		{
			parts.Add($"{excludeItems.Count} exclusion(s)");
		}
		return string.Join(", ", parts);
	}
}
