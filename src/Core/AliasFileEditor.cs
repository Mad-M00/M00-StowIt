using System;
using System.Collections.Generic;

namespace M00StowIt.Core;

// Applies "stow alias" edits to the CrateLabels.txt lines. The file is also
// hand-edited and its layout carries meaning (comments document decisions,
// line order is priority), so edits touch exactly the one mapping asked for:
// replacing tokens keeps the line in place, and labels sharing a line survive
// the deletion of a partner.
public static class AliasFileEditor
{
	public sealed class Result
	{
		internal Result(IReadOnlyList<string> lines, bool changed, string message)
		{
			Lines = lines;
			Changed = changed;
			Message = message;
		}

		public IReadOnlyList<string> Lines { get; }

		public bool Changed { get; }

		public string Message { get; }
	}

	public static Result SetTokens(IReadOnlyList<string> lines, string label, IReadOnlyList<string> tokens)
	{
		if (tokens.Count == 0)
		{
			return new Result(lines, false, "No items given");
		}
		string joinedTokens = string.Join(", ", tokens);
		int index = FindDefiningLine(lines, label, out _, out _);
		var updated = new List<string>(lines);
		if (index < 0)
		{
			updated.Add($"{label.Trim()} = {joinedTokens}");
			return new Result(updated, true,
				$"Added new label '{label.Trim()}' at the end of the file (lowest priority; move the line up in CrateLabels.txt to raise it)");
		}
		ParseMappingLine(lines[index], out string lhs, out _);
		updated[index] = $"{lhs} = {joinedTokens}";
		return new Result(updated, true, $"Replaced the items of '{lhs}'");
	}

	public static Result AddTokens(IReadOnlyList<string> lines, string label, IReadOnlyList<string> tokens)
	{
		int index = FindDefiningLine(lines, label, out _, out _);
		if (index < 0)
		{
			return SetTokens(lines, label, tokens);
		}
		ParseMappingLine(lines[index], out string lhs, out string rhs);
		List<string> existing = SplitList(rhs);
		int added = 0;
		foreach (string token in tokens)
		{
			if (!ContainsIgnoreCase(existing, token))
			{
				existing.Add(token);
				added++;
			}
		}
		if (added == 0)
		{
			return new Result(lines, false, $"'{lhs}' already has all of those items");
		}
		var updated = new List<string>(lines)
		{
			[index] = $"{lhs} = {string.Join(", ", existing)}"
		};
		return new Result(updated, true, $"Added {added} item(s) to '{lhs}'");
	}

	public static Result RemoveTokens(IReadOnlyList<string> lines, string label, IReadOnlyList<string> tokens)
	{
		int index = FindDefiningLine(lines, label, out _, out _);
		if (index < 0)
		{
			return new Result(lines, false, $"No label '{label.Trim()}' in the file");
		}
		ParseMappingLine(lines[index], out string lhs, out string rhs);
		List<string> existing = SplitList(rhs);
		int removed = existing.RemoveAll(entry => ContainsIgnoreCase(tokens, entry));
		if (removed == 0)
		{
			return new Result(lines, false, $"None of those items are on the '{lhs}' line");
		}
		var updated = new List<string>(lines);
		if (existing.Count == 0)
		{
			updated.RemoveAt(index);
			return new Result(updated, true, $"Removed the last item; label '{lhs}' is gone entirely");
		}
		updated[index] = $"{lhs} = {string.Join(", ", existing)}";
		return new Result(updated, true, $"Removed {removed} item(s) from '{lhs}'");
	}

	public static Result DeleteLabel(IReadOnlyList<string> lines, string label)
	{
		int index = FindDefiningLine(lines, label, out List<string> lhsLabels, out string rhs);
		if (index < 0)
		{
			return new Result(lines, false, $"No label '{label.Trim()}' in the file");
		}
		var updated = new List<string>(lines);
		if (lhsLabels.Count == 1)
		{
			updated.RemoveAt(index);
			return new Result(updated, true, $"Deleted label '{lhsLabels[0]}'");
		}
		string labelLower = label.Trim().ToLowerInvariant();
		List<string> remaining = lhsLabels.FindAll(l => l.ToLowerInvariant() != labelLower);
		updated[index] = $"{string.Join(", ", remaining)} = {rhs}";
		return new Result(updated, true,
			$"Deleted label '{label.Trim()}'; its partner label(s) [{string.Join(", ", remaining)}] keep the rule");
	}

	private static int FindDefiningLine(
		IReadOnlyList<string> lines, string label, out List<string> lhsLabels, out string rhs)
	{
		string labelLower = label.Trim().ToLowerInvariant();
		for (int i = 0; i < lines.Count; i++)
		{
			if (!ParseMappingLine(lines[i], out string lhs, out rhs))
			{
				continue;
			}
			lhsLabels = SplitList(lhs);
			foreach (string candidate in lhsLabels)
			{
				if (candidate.ToLowerInvariant() == labelLower)
				{
					return i;
				}
			}
		}
		lhsLabels = null;
		rhs = null;
		return -1;
	}

	private static bool ParseMappingLine(string rawLine, out string lhs, out string rhs)
	{
		lhs = null;
		rhs = null;
		string line = rawLine?.Trim() ?? string.Empty;
		if (line.Length == 0 || line[0] == '#')
		{
			return false;
		}
		int equalsIndex = line.IndexOf('=');
		if (equalsIndex <= 0)
		{
			return false;
		}
		lhs = line.Substring(0, equalsIndex).Trim();
		rhs = line.Substring(equalsIndex + 1).Trim();
		return lhs.Length > 0 && !lhs.Equals("WarnUnknownGroups", StringComparison.OrdinalIgnoreCase);
	}

	private static List<string> SplitList(string text)
	{
		var entries = new List<string>();
		foreach (string part in text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
		{
			string entry = part.Trim();
			if (entry.Length > 0)
			{
				entries.Add(entry);
			}
		}
		return entries;
	}

	private static bool ContainsIgnoreCase(IReadOnlyList<string> entries, string value)
	{
		foreach (string entry in entries)
		{
			if (entry.Trim().Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
		}
		return false;
	}
}
