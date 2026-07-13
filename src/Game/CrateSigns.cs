using System.Collections.Generic;
using M00StowIt.Core;

namespace M00StowIt.Game;

internal static class CrateSigns
{
	// Category labels from the sign on the container's parent block; empty
	// when the crate is unsigned. Aliases allow a multi-line sign to spell
	// one label across lines.
	public static IReadOnlyList<string> ReadLabels(TEFeatureStorage container, AliasConfiguration aliases)
	{
		ITileEntitySignable sign = container.Parent?.GetFeature<ITileEntitySignable>();
		return SignLabelParser.Parse(
			sign?.GetAuthoredText()?.Text,
			segment => aliases != null && aliases.TryGetCanonicalLabel(segment, out _));
	}

	public static bool ContainsLabel(IReadOnlyList<string> labels, string labelLower)
	{
		for (int i = 0; i < labels.Count; i++)
		{
			if (labels[i] == labelLower)
			{
				return true;
			}
		}
		return false;
	}
}
