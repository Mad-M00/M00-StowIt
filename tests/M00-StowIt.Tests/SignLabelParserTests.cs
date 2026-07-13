using M00StowIt.Core;
using Xunit;

namespace M00StowIt.Tests;

// A crate sign can carry several category labels. The separators are the
// contract players rely on when writing signs: commas/semicolons/new lines
// split, but slashes do NOT — "Building / Décor" and "Mods \ Tools" are single
// labels.
public class SignLabelParserTests
{
	[Fact]
	public void Labels_are_split_on_commas_semicolons_and_line_breaks()
	{
		var labels = SignLabelParser.Parse("Cans, Drinks; Food\nMedical\r\nAmmo");
		Assert.Equal(new[] { "cans", "drinks", "food", "medical", "ammo" }, labels);
	}

	[Fact]
	public void Slashes_and_backslashes_do_not_split_labels()
	{
		Assert.Equal(new[] { "building / décor" }, SignLabelParser.Parse("Building / Décor"));
		Assert.Equal(new[] { "mods \\ tools" }, SignLabelParser.Parse("Mods \\ Tools"));
	}

	[Fact]
	public void Labels_are_trimmed_and_lowercased()
	{
		Assert.Equal(new[] { "cans" }, SignLabelParser.Parse("  CANS  "));
	}

	[Fact]
	public void Blank_or_separator_only_signs_give_no_labels()
	{
		Assert.Empty(SignLabelParser.Parse(null));
		Assert.Empty(SignLabelParser.Parse(""));
		Assert.Empty(SignLabelParser.Parse("   "));
		Assert.Empty(SignLabelParser.Parse(",;,\n"));
	}

	// A player may write one label across two sign lines for readability
	// ("Mod" / "Tools"). If the whole segment is a known alias it stays ONE
	// label; otherwise line breaks still separate labels, so an old-style
	// sign listing "Medical" and "Ammo" on separate lines keeps working.
	[Fact]
	public void A_multi_line_segment_matching_a_known_alias_stays_one_label()
	{
		var labels = SignLabelParser.Parse("Mod\nTools",
			segment => LabelNormalizer.Normalize(segment) == "mod tool");
		Assert.Single(labels);
	}

	[Fact]
	public void Multi_line_segments_not_matching_an_alias_still_split_into_labels()
	{
		var labels = SignLabelParser.Parse("Medical\nAmmo", segment => false);
		Assert.Equal(new[] { "medical", "ammo" }, labels);
	}
}
