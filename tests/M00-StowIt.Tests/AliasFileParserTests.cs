using M00StowIt.Core;
using Xunit;

namespace M00StowIt.Tests;

// CrateLabels.txt is user-edited. These tests pin the file format so a
// refactor can't break existing player configs: '#' comments, "label = tokens"
// lines, multi-label left sides, '-' exclusions, and the WarnUnknownGroups
// setting masquerading as a line.
public class AliasFileParserTests
{
	private static AliasConfiguration Parse(params string[] lines)
	{
		return AliasFileParser.Parse(lines);
	}

	[Fact]
	public void Comments_and_blank_lines_are_ignored()
	{
		var config = Parse("# a comment", "", "   ", "Cans = foodCan*");
		Assert.Single(config.TokensByLabel);
	}

	[Fact]
	public void A_line_maps_a_label_to_its_tokens()
	{
		var config = Parse("Cans = foodCan*, -foodCanShamSchematic");
		Assert.Equal(new[] { "foodCan*", "-foodCanShamSchematic" }, config.TokensByLabel["cans"]);
	}

	[Fact]
	public void Multiple_labels_on_the_left_share_the_same_tokens()
	{
		var config = Parse("Armour, Armor = armor*");
		Assert.Equal(config.TokensByLabel["armour"], config.TokensByLabel["armor"]);
	}

	[Fact]
	public void Labels_are_stored_lowercased_for_case_insensitive_sign_matching()
	{
		var config = Parse("CANS = foodCan*");
		Assert.True(config.IsAliasLabel("cans"));
	}

	[Fact]
	public void Exclusion_tokens_keep_their_minus_prefix_for_later_resolution()
	{
		var config = Parse("Food = Food/Cooking, -toolCooking*");
		Assert.Contains("-toolCooking*", config.TokensByLabel["food"]);
	}

	[Fact]
	public void WarnUnknownGroups_is_a_setting_not_a_label()
	{
		var config = Parse("WarnUnknownGroups = 0", "Cans = foodCan*");
		Assert.False(config.WarnUnknownGroups);
		Assert.False(config.IsAliasLabel("warnunknowngroups"));
	}

	[Fact]
	public void Unknown_group_warnings_default_to_enabled()
	{
		Assert.True(Parse("Cans = foodCan*").WarnUnknownGroups);
	}

	[Fact]
	public void Lines_without_an_equals_sign_are_ignored()
	{
		var config = Parse("this is not a mapping", "Cans = foodCan*");
		Assert.Single(config.TokensByLabel);
	}

	[Fact]
	public void Later_definitions_replace_earlier_ones_for_the_same_label()
	{
		var config = Parse("Cans = foodCan*", "Cans = drinkCan*");
		Assert.Equal(new[] { "drinkCan*" }, config.TokensByLabel["cans"]);
	}

	[Fact]
	public void Duplicate_tokens_on_one_line_are_kept_once()
	{
		var config = Parse("Cans = foodCan*, foodCan*");
		Assert.Single(config.TokensByLabel["cans"]);
	}

	// Priority is how "Ammo 9mm" beats a generic "Ammo" crate when both exist:
	// the earlier a label is defined in the file, the earlier its crate drains
	// within a routing tier. Players order the file most-specific-first.
	[Fact]
	public void Labels_defined_earlier_in_the_file_win_priority_over_later_ones()
	{
		var config = Parse("Ammo 9mm = ammo9mm*", "Ammo = ammoArrow*");
		Assert.True(config.GetPriority("ammo 9mm") < config.GetPriority("ammo"));
	}

	[Fact]
	public void Labels_on_the_same_line_share_one_priority()
	{
		var config = Parse("Armour, Armor = armor*");
		Assert.Equal(config.GetPriority("armour"), config.GetPriority("armor"));
	}

	[Fact]
	public void Labels_not_in_the_file_have_the_lowest_priority()
	{
		var config = Parse("Ammo = ammoArrow*");
		Assert.True(config.GetPriority("ammo") < config.GetPriority("some ad-hoc sign"));
	}

	// A sign does not need to reproduce the alias label exactly: any variant
	// with the same normalized form ("Mod Tools", "Mod - Tools", ...) finds
	// the canonical alias.
	[Fact]
	public void Fuzzy_label_lookup_finds_the_canonical_alias()
	{
		var config = Parse("Mod Tools = modFuelTank*");
		Assert.True(config.TryGetCanonicalLabel("mods \\ tools", out string canonical));
		Assert.Equal("mod tools", canonical);
		Assert.True(config.TryGetCanonicalLabel("mod - tools", out _));
		Assert.False(config.TryGetCanonicalLabel("mод weapons", out _));
	}

	[Fact]
	public void Exact_label_matches_win_over_normalized_ones()
	{
		var config = Parse("Cans = foodCan*");
		Assert.True(config.TryGetCanonicalLabel("cans", out string canonical));
		Assert.Equal("cans", canonical);
	}
}
