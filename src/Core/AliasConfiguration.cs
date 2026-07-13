using System;
using System.Collections.Generic;

namespace M00StowIt.Core;

// Parsed contents of CrateLabels.txt: crate labels (lowercased) mapped to
// their raw, unresolved tokens.
public sealed class AliasConfiguration
{
	public static readonly AliasConfiguration Empty = new(
		new Dictionary<string, IReadOnlyList<string>>(), warnUnknownGroups: true);

	private readonly IReadOnlyDictionary<string, int> priorityByLabel;
	private readonly Dictionary<string, string> canonicalByNormalized;

	public AliasConfiguration(
		IReadOnlyDictionary<string, IReadOnlyList<string>> tokensByLabel,
		bool warnUnknownGroups,
		IReadOnlyDictionary<string, int> priorityByLabel = null)
	{
		TokensByLabel = tokensByLabel ?? throw new ArgumentNullException(nameof(tokensByLabel));
		WarnUnknownGroups = warnUnknownGroups;
		this.priorityByLabel = priorityByLabel;
		canonicalByNormalized = BuildNormalizedIndex(tokensByLabel);
	}

	// When two alias labels normalize to the same form, the higher-priority
	// (earlier-defined) one wins the normalized slot.
	private Dictionary<string, string> BuildNormalizedIndex(
		IReadOnlyDictionary<string, IReadOnlyList<string>> tokensByLabel)
	{
		var index = new Dictionary<string, string>(StringComparer.Ordinal);
		foreach (string label in tokensByLabel.Keys)
		{
			string normalized = LabelNormalizer.Normalize(label);
			if (normalized.Length == 0)
			{
				continue;
			}
			if (!index.TryGetValue(normalized, out string existing)
				|| GetPriority(label) < GetPriority(existing)
				|| (GetPriority(label) == GetPriority(existing) && string.CompareOrdinal(label, existing) < 0))
			{
				index[normalized] = label;
			}
		}
		return index;
	}

	public IReadOnlyDictionary<string, IReadOnlyList<string>> TokensByLabel { get; }

	public bool WarnUnknownGroups { get; }

	public int LabelCount => TokensByLabel.Count;

	public bool IsAliasLabel(string labelLower)
	{
		return TokensByLabel.ContainsKey(labelLower);
	}

	// Fuzzy lookup: exact label first, then by normalized form, so a sign
	// saying "Mods \ Tools" or "Mod-Tools" finds the "Mod Tools" alias.
	public bool TryGetCanonicalLabel(string label, out string canonicalLabel)
	{
		string key = label.Trim().ToLowerInvariant();
		if (TokensByLabel.ContainsKey(key))
		{
			canonicalLabel = key;
			return true;
		}
		string normalized = LabelNormalizer.Normalize(key);
		if (normalized.Length > 0 && canonicalByNormalized.TryGetValue(normalized, out canonicalLabel))
		{
			return true;
		}
		canonicalLabel = null;
		return false;
	}

	// Lower value = drains earlier within a routing tier. Labels defined
	// earlier in CrateLabels.txt outrank later ones; labels not defined
	// at all (ad-hoc sign text) rank last.
	public int GetPriority(string labelLower)
	{
		if (priorityByLabel != null && priorityByLabel.TryGetValue(labelLower, out int priority))
		{
			return priority;
		}
		return int.MaxValue;
	}
}
