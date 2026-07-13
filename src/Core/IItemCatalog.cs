using System.Collections.Generic;

namespace M00StowIt.Core;

// Read access to the game's item universe. The game adapter builds this from
// ItemClass.list once a world is loaded; tests provide an in-memory fake.
public interface IItemCatalog
{
	IReadOnlyList<CatalogItem> Items { get; }

	// Canonical-case group names, for player-facing listings (stow groups).
	IReadOnlyCollection<string> GroupNames { get; }

	bool IsKnownGroup(string groupLower);

	CatalogItem FindByInternalName(string nameLower);

	// Null when no item carries this display name; a display name can be
	// shared by several items.
	IReadOnlyList<CatalogItem> FindByDisplayName(string displayLower);
}
