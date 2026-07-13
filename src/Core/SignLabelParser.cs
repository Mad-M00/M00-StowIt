using System;
using System.Collections.Generic;

namespace M00StowIt.Core;

// Splits crate sign text into category labels. Commas, semicolons and line
// breaks separate labels; slashes deliberately do not, so labels like
// "Building / Décor" and "Mods \ Tools" stay intact.
public static class SignLabelParser
{
	private static readonly char[] HardSeparators = { ',', ';' };
	private static readonly char[] LineBreaks = { '\n', '\r' };

	public static IReadOnlyList<string> Parse(string signText)
	{
		return Parse(signText, null);
	}

	// isKnownAliasLabel enables one fuzzy nicety: a label written across sign
	// lines ("Mod" / "Tools") stays one label when the whole segment is a
	// known alias; otherwise line breaks separate labels as they always did.
	public static IReadOnlyList<string> Parse(string signText, Predicate<string> isKnownAliasLabel)
	{
		if (string.IsNullOrEmpty(signText))
		{
			return Array.Empty<string>();
		}
		var labels = new List<string>();
		foreach (string segment in signText.Split(HardSeparators, StringSplitOptions.RemoveEmptyEntries))
		{
			string trimmed = segment.Trim();
			if (trimmed.Length == 0)
			{
				continue;
			}
			string segmentLower = trimmed.ToLowerInvariant();
			if (isKnownAliasLabel != null
				&& segmentLower.IndexOfAny(LineBreaks) >= 0
				&& isKnownAliasLabel(segmentLower))
			{
				labels.Add(segmentLower);
				continue;
			}
			foreach (string part in segmentLower.Split(LineBreaks, StringSplitOptions.RemoveEmptyEntries))
			{
				string label = part.Trim();
				if (label.Length > 0)
				{
					labels.Add(label);
				}
			}
		}
		return labels;
	}
}
