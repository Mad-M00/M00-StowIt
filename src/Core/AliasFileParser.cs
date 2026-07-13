using System;
using System.Collections.Generic;

namespace M00StowIt.Core;

// CrateLabels.txt format: "label[, label...] = token[, token...]" per
// line, '#' comments, '-' prefixed tokens are exclusions, plus the special
// "WarnUnknownGroups = 0|1" setting. Tokens are kept raw here; resolution
// happens later when game item data is available.
public static class AliasFileParser
{
	private static readonly char[] ListSeparators = { ',', ';' };

	public static AliasConfiguration Parse(IEnumerable<string> lines)
	{
		var tokensByLabel = new Dictionary<string, IReadOnlyList<string>>(StringComparer.Ordinal);
		var priorityByLabel = new Dictionary<string, int>(StringComparer.Ordinal);
		int nextPriority = 0;
		bool warnUnknownGroups = true;
		foreach (string rawLine in lines)
		{
			string line = rawLine?.Trim() ?? string.Empty;
			if (line.Length == 0 || line[0] == '#')
			{
				continue;
			}
			int equalsIndex = line.IndexOf('=');
			if (equalsIndex <= 0)
			{
				continue;
			}
			string labelsPart = line.Substring(0, equalsIndex).Trim();
			string tokensPart = line.Substring(equalsIndex + 1).Trim();
			if (labelsPart.Length == 0)
			{
				continue;
			}
			if (labelsPart.Equals("WarnUnknownGroups", StringComparison.OrdinalIgnoreCase))
			{
				warnUnknownGroups = tokensPart == "1"
					|| tokensPart.Equals("true", StringComparison.OrdinalIgnoreCase);
				continue;
			}
			List<string> tokens = ParseList(tokensPart);
			if (tokens.Count == 0)
			{
				continue;
			}
			int linePriority = nextPriority++;
			foreach (string label in ParseList(labelsPart))
			{
				string key = label.ToLowerInvariant();
				tokensByLabel[key] = tokens;
				priorityByLabel[key] = linePriority;
			}
		}
		return new AliasConfiguration(tokensByLabel, warnUnknownGroups, priorityByLabel);
	}

	private static List<string> ParseList(string text)
	{
		var entries = new List<string>();
		foreach (string part in text.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries))
		{
			string entry = part.Trim();
			if (entry.Length > 0 && !entries.Contains(entry))
			{
				entries.Add(entry);
			}
		}
		return entries;
	}
}
