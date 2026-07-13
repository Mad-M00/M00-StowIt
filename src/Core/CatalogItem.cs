using System;
using System.Collections.Generic;

namespace M00StowIt.Core;

// A game item as the sorter sees it: internal name, player-visible display
// name, and the creative-menu groups it belongs to (inheritance already
// applied by the game).
public sealed class CatalogItem
{
	public CatalogItem(string internalName, string displayName, params string[] groups)
	{
		InternalName = internalName ?? throw new ArgumentNullException(nameof(internalName));
		NameLower = internalName.ToLowerInvariant();
		DisplayName = string.IsNullOrEmpty(displayName) ? internalName : displayName;
		var groupsLower = new List<string>();
		if (groups != null)
		{
			foreach (string group in groups)
			{
				if (!string.IsNullOrEmpty(group))
				{
					groupsLower.Add(group.Trim().ToLowerInvariant());
				}
			}
		}
		GroupsLower = groupsLower;
	}

	public string InternalName { get; }

	public string NameLower { get; }

	public string DisplayName { get; }

	public IReadOnlyList<string> GroupsLower { get; }

	public bool IsInGroup(string groupLower)
	{
		foreach (string group in GroupsLower)
		{
			if (group == groupLower)
			{
				return true;
			}
		}
		return false;
	}

	// "internalName  (Display Name)" for console output.
	public string ToDisplayString()
	{
		if (DisplayName.Equals(InternalName, StringComparison.OrdinalIgnoreCase))
		{
			return InternalName;
		}
		return $"{InternalName}  ({DisplayName})";
	}
}
