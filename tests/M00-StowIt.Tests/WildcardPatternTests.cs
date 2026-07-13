using M00StowIt.Core;
using Xunit;

namespace M00StowIt.Tests;

// Sign labels and alias tokens promise glob semantics: '*' matches any run of
// characters, everything else is literal. These tests pin that promise so the
// matcher can never silently drift towards substring or regex behaviour.
public class WildcardPatternTests
{
	[Theory]
	[InlineData("foodCanBeef", "foodCan*", true)]
	[InlineData("foodXCanBeef", "foodCan*", false)]
	[InlineData("drink", "drink*", true)] // '*' may match an empty run
	[InlineData("gunMGT1AK47Parts", "*Parts", true)]
	[InlineData("resourceScrapIron", "*scrap*", true)]
	[InlineData("abc", "a*b*c", true)]
	[InlineData("axxbyyc", "a*b*c", true)]
	[InlineData("acb", "a*b*c", false)]
	public void Star_matches_any_run_of_characters(string text, string pattern, bool expected)
	{
		Assert.Equal(expected, WildcardPattern.Matches(text, pattern));
	}

	[Fact]
	public void Matching_is_case_insensitive()
	{
		Assert.True(WildcardPattern.Matches("FoodCanBeef", "foodcan*"));
		Assert.True(WildcardPattern.Matches("foodcanbeef", "FOODCAN*"));
	}

	[Fact]
	public void Without_a_star_the_pattern_must_match_exactly()
	{
		Assert.True(WildcardPattern.Matches("foodCanBeef", "foodcanbeef"));
		Assert.False(WildcardPattern.Matches("foodCanBeef", "foodcan"));
	}

	// Why the tree-seed tokens in CrateLabels.txt are exact names, not
	// "treePlanted*1m": a suffix pattern also matches larger sizes ending in
	// "1m" (41m, 21m...). Documented here so nobody "simplifies" the config.
	[Fact]
	public void Suffix_patterns_also_match_longer_runs_ending_the_same_way()
	{
		Assert.True(WildcardPattern.Matches("treePlantedOak41m", "treeplanted*1m"));
	}

	[Fact]
	public void IsPattern_is_true_only_when_a_star_is_present()
	{
		Assert.True(WildcardPattern.IsPattern("foodCan*"));
		Assert.False(WildcardPattern.IsPattern("foodCanBeef"));
	}
}
