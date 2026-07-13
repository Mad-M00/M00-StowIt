using System.Collections.Generic;
using System.Text;

namespace M00StowIt.Core;

// Canonicalises a crate label so spelling variants resolve to the same alias:
// case, punctuation and line breaks are dropped, adjacent digit runs join
// ("7.62" == "762"), and plain words lose a plural 's' ("Mods" == "Mod").
public static class LabelNormalizer
{
	public static string Normalize(string label)
	{
		if (string.IsNullOrEmpty(label))
		{
			return string.Empty;
		}
		List<string> words = SplitIntoAlphanumericWords(label);
		MergeAdjacentDigitRuns(words);
		for (int i = 0; i < words.Count; i++)
		{
			words[i] = Depluralize(words[i]);
		}
		return string.Join(" ", words);
	}

	private static List<string> SplitIntoAlphanumericWords(string label)
	{
		var words = new List<string>();
		var current = new StringBuilder();
		foreach (char c in label)
		{
			if (char.IsLetterOrDigit(c))
			{
				current.Append(char.ToLowerInvariant(c));
			}
			else if (current.Length > 0)
			{
				words.Add(current.ToString());
				current.Clear();
			}
		}
		if (current.Length > 0)
		{
			words.Add(current.ToString());
		}
		return words;
	}

	// "7", "62" (from "7.62") become "762" so calibre labels match however
	// the player punctuates them.
	private static void MergeAdjacentDigitRuns(List<string> words)
	{
		for (int i = words.Count - 1; i > 0; i--)
		{
			if (IsAllDigits(words[i]) && IsAllDigits(words[i - 1]))
			{
				words[i - 1] += words[i];
				words.RemoveAt(i);
			}
		}
	}

	private static string Depluralize(string word)
	{
		bool isPlainWord = word.Length > 3 && IsAllLetters(word);
		if (isPlainWord && word.EndsWith("s") && !word.EndsWith("ss"))
		{
			return word.Substring(0, word.Length - 1);
		}
		return word;
	}

	private static bool IsAllDigits(string word)
	{
		foreach (char c in word)
		{
			if (!char.IsDigit(c))
			{
				return false;
			}
		}
		return true;
	}

	private static bool IsAllLetters(string word)
	{
		foreach (char c in word)
		{
			if (!char.IsLetter(c))
			{
				return false;
			}
		}
		return true;
	}
}
