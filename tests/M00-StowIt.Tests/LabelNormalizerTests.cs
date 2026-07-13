using M00StowIt.Core;
using Xunit;

namespace M00StowIt.Tests;

// Players write labels on signs from memory: "Mod Tools", "Mods \ Tools",
// "MOD-TOOLS", or split across sign lines. The normalizer maps all of those
// to one canonical form so the alias still resolves. Contract: punctuation
// and casing never matter, plural/singular never matters, digit runs join
// across punctuation ("7.62" is "762").
public class LabelNormalizerTests
{
	[Theory]
	[InlineData("Mods \\ Tools")]
	[InlineData("Mod Tools")]
	[InlineData("MOD-TOOLS")]
	[InlineData("mod _ tools")]
	[InlineData("Mod / Tool")]
	[InlineData("Mod\nTools")] // multi-line sign
	public void Punctuation_case_line_breaks_and_plurals_do_not_matter(string variant)
	{
		Assert.Equal("mod tool", LabelNormalizer.Normalize(variant));
	}

	[Fact]
	public void Plural_and_singular_labels_normalize_identically()
	{
		Assert.Equal(LabelNormalizer.Normalize("Cans"), LabelNormalizer.Normalize("Can"));
		Assert.Equal(LabelNormalizer.Normalize("Drinks"), LabelNormalizer.Normalize("Drink"));
	}

	[Fact]
	public void Digit_runs_join_across_punctuation()
	{
		Assert.Equal("ammo 762", LabelNormalizer.Normalize("Ammo 7.62"));
		Assert.Equal("ammo 762", LabelNormalizer.Normalize("Ammo 762"));
		Assert.Equal("ammo 44", LabelNormalizer.Normalize("Ammo .44"));
	}

	[Fact]
	public void Words_ending_in_double_s_keep_their_s()
	{
		Assert.Equal("glass", LabelNormalizer.Normalize("Glass"));
	}

	[Fact]
	public void Mixed_alphanumeric_words_are_not_depluralized_or_split()
	{
		Assert.Equal("ammo 9mm", LabelNormalizer.Normalize("Ammo 9mm"));
	}
}
