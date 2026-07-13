namespace M00StowIt.Core;

// Glob matching for sign labels and alias tokens: '*' matches any run of
// characters (including none), everything else is literal, case-insensitive.
public static class WildcardPattern
{
	public static bool IsPattern(string token)
	{
		return token != null && token.IndexOf('*') >= 0;
	}

	public static bool Matches(string text, string pattern)
	{
		if (text == null || pattern == null)
		{
			return false;
		}
		int t = 0;
		int p = 0;
		int lastStar = -1;
		int backtrack = 0;
		while (t < text.Length)
		{
			if (p < pattern.Length && char.ToLowerInvariant(pattern[p]) == char.ToLowerInvariant(text[t]))
			{
				t++;
				p++;
			}
			else if (p < pattern.Length && pattern[p] == '*')
			{
				lastStar = p++;
				backtrack = t;
			}
			else if (lastStar >= 0)
			{
				p = lastStar + 1;
				t = ++backtrack;
			}
			else
			{
				return false;
			}
		}
		while (p < pattern.Length && pattern[p] == '*')
		{
			p++;
		}
		return p == pattern.Length;
	}
}
