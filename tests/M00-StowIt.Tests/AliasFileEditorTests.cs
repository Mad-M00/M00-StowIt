using System;
using System.Linq;
using M00StowIt.Core;
using Xunit;

namespace M00StowIt.Tests;

// "stow alias" edits CrateLabels.txt while the game runs. The file is also
// hand-edited and carries meaning in its layout: comments explain decisions
// and LINE ORDER IS PRIORITY. So the editor must change exactly the one thing
// asked for and leave everything else byte-identical.
public class AliasFileEditorTests
{
	private static readonly string[] SampleFile =
	{
		"# comment stays",
		"",
		"WarnUnknownGroups = 1",
		"Cans = foodCan*, -foodCanShamSchematic",
		"Mod Armor, Mod Armour = modArmor*, modRadiationReady",
		"Ammo = Ammo"
	};

	[Fact]
	public void Setting_an_existing_label_replaces_tokens_but_keeps_the_line_position()
	{
		var result = AliasFileEditor.SetTokens(SampleFile, "cans", new[] { "drinkCan*" });
		Assert.True(result.Changed);
		Assert.Equal("Cans = drinkCan*", result.Lines[3]); // same line = same priority
		Assert.Equal(SampleFile.Length, result.Lines.Count);
	}

	[Fact]
	public void Setting_a_new_label_appends_it_at_the_end_of_the_file()
	{
		var result = AliasFileEditor.SetTokens(SampleFile, "Breakfast", new[] { "foodEgg", "foodHoney" });
		Assert.Equal("Breakfast = foodEgg, foodHoney", result.Lines[result.Lines.Count - 1]);
	}

	[Fact]
	public void Adding_tokens_appends_them_without_duplicates()
	{
		var result = AliasFileEditor.AddTokens(SampleFile, "ammo", new[] { "thrown*", "AMMO" });
		Assert.Equal("Ammo = Ammo, thrown*", result.Lines[5]);
	}

	[Fact]
	public void Adding_to_a_missing_label_creates_it()
	{
		var result = AliasFileEditor.AddTokens(SampleFile, "Breakfast", new[] { "foodEgg" });
		Assert.True(result.Changed);
		Assert.Contains("Breakfast = foodEgg", result.Lines);
	}

	[Fact]
	public void Removing_a_token_keeps_the_others()
	{
		var result = AliasFileEditor.RemoveTokens(SampleFile, "cans", new[] { "-foodCanShamSchematic" });
		Assert.Equal("Cans = foodCan*", result.Lines[3]);
	}

	[Fact]
	public void Removing_the_last_token_removes_the_whole_line()
	{
		var result = AliasFileEditor.RemoveTokens(SampleFile, "ammo", new[] { "ammo" });
		Assert.True(result.Changed);
		Assert.DoesNotContain(result.Lines, line => line.StartsWith("Ammo ="));
	}

	[Fact]
	public void Removing_a_token_that_is_not_there_changes_nothing()
	{
		var result = AliasFileEditor.RemoveTokens(SampleFile, "cans", new[] { "noSuchToken" });
		Assert.False(result.Changed);
		Assert.Equal(SampleFile, result.Lines);
	}

	// "Mod Armor, Mod Armour = ..." are synonyms sharing one rule. Deleting
	// one spelling must not take the other down with it.
	[Fact]
	public void Deleting_a_label_that_shares_a_line_keeps_its_partners()
	{
		var result = AliasFileEditor.DeleteLabel(SampleFile, "mod armour");
		Assert.Equal("Mod Armor = modArmor*, modRadiationReady", result.Lines[4]);
	}

	[Fact]
	public void Deleting_the_only_label_on_a_line_removes_the_line()
	{
		var result = AliasFileEditor.DeleteLabel(SampleFile, "cans");
		Assert.DoesNotContain(result.Lines, line => line.StartsWith("Cans"));
		Assert.Equal(SampleFile.Length - 1, result.Lines.Count);
	}

	[Fact]
	public void Deleting_a_missing_label_changes_nothing()
	{
		var result = AliasFileEditor.DeleteLabel(SampleFile, "no such label");
		Assert.False(result.Changed);
	}

	[Fact]
	public void Comments_settings_and_unrelated_lines_are_never_touched()
	{
		var result = AliasFileEditor.SetTokens(SampleFile, "cans", new[] { "x" });
		Assert.Equal("# comment stays", result.Lines[0]);
		Assert.Equal("", result.Lines[1]);
		Assert.Equal("WarnUnknownGroups = 1", result.Lines[2]);
		Assert.Equal("Ammo = Ammo", result.Lines[5]);
	}

	[Fact]
	public void Label_matching_ignores_case()
	{
		var result = AliasFileEditor.SetTokens(SampleFile, "CANS", new[] { "x" });
		Assert.Equal("Cans = x", result.Lines[3]);
	}
}
