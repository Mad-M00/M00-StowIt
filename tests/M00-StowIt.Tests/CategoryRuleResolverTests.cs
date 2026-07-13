using System.Collections.Generic;
using System.Linq;
using M00StowIt.Core;
using M00StowIt.Tests.Fakes;
using Xunit;

namespace M00StowIt.Tests;

// The resolver is the heart of the mod: it turns a crate label into a routing
// rule via a fixed cascade — game group, then item wildcard/exact name, then
// display name, else warn. These tests pin the cascade ORDER (group must beat
// item) and the exclusion semantics, because a change in either silently
// re-routes items in players' bases.
public class CategoryRuleResolverTests
{
	private static FakeItemCatalog Catalog() => new(
		new CatalogItem("foodCanBeef", "Can of Beef", "Food/Cooking"),
		new CatalogItem("foodCanChili", "Can of Chili"),
		new CatalogItem("foodCanShamSchematic", "Can of Sham Schematic"),
		new CatalogItem("foodShamSandwich", "Sham Sandwich", "Food/Cooking"),
		new CatalogItem("drinkJarBeer", "Beer", "Food/Cooking", "Medical"),
		new CatalogItem("medicalBandage", "Bandage", "Medical"),
		new CatalogItem("medical", "Item Sharing A Group Name", "Special Items"));

	private static CategoryRuleResolver Resolver(
		RecordingLog log = null,
		Dictionary<string, IReadOnlyList<string>> aliases = null,
		bool warnUnknownGroups = true)
	{
		var config = new AliasConfiguration(
			aliases ?? new Dictionary<string, IReadOnlyList<string>>(),
			warnUnknownGroups);
		return new CategoryRuleResolver(Catalog(), log ?? new RecordingLog(), config);
	}

	private static Dictionary<string, IReadOnlyList<string>> Alias(string label, params string[] tokens)
	{
		return new Dictionary<string, IReadOnlyList<string>> { [label] = tokens };
	}

	[Fact]
	public void Group_names_win_over_items_with_the_same_name()
	{
		// An item literally named "medical" exists; the group must still win,
		// otherwise every group-labelled crate breaks when a mod adds a
		// same-named item.
		CategoryRule rule = Resolver().Resolve("Medical");
		Assert.True(rule.HasGroupRules);
		Assert.False(rule.HasItemRules);
	}

	[Fact]
	public void Wildcard_tokens_resolve_to_item_rules()
	{
		CategoryRule rule = Resolver().Resolve("foodCan*");
		Assert.True(rule.HasItemRules);
		Assert.False(rule.HasGroupRules);
	}

	[Fact]
	public void Exclusions_remove_items_from_wildcard_matches()
	{
		// The real-world case this protects: "Cans = foodCan*" must be able to
		// keep the Sham schematic (a book) out of the cans crate.
		var resolver = Resolver(aliases: Alias("cans", "foodCan*", "-foodCanShamSchematic"));
		CategoryRule rule = resolver.Resolve("cans");
		var matched = resolver.EnumerateMatches(rule).Select(i => i.InternalName).ToList();
		Assert.Contains("foodCanBeef", matched);
		Assert.Contains("foodCanChili", matched);
		Assert.DoesNotContain("foodCanShamSchematic", matched);
	}

	[Fact]
	public void Display_names_resolve_when_no_group_or_internal_name_matches()
	{
		// A player can sign a crate with what they see in the UI.
		var resolver = Resolver();
		CategoryRule rule = resolver.Resolve("Sham Sandwich");
		Assert.Equal(new[] { "foodShamSandwich" },
			resolver.EnumerateMatches(rule).Select(i => i.InternalName));
	}

	[Fact]
	public void Unknown_tokens_warn_and_are_ignored()
	{
		var log = new RecordingLog();
		CategoryRule rule = Resolver(log).Resolve("Meels");
		Assert.True(rule.IsEmpty);
		Assert.Single(log.Warnings);
	}

	[Fact]
	public void Unknown_token_warnings_can_be_silenced()
	{
		var log = new RecordingLog();
		Resolver(log, warnUnknownGroups: false).Resolve("Meels");
		Assert.Empty(log.Warnings);
	}

	[Fact]
	public void Excluding_a_group_blocks_all_its_members_from_group_matches()
	{
		var resolver = Resolver(aliases: Alias("meds", "Medical", "-Food/Cooking"));
		CategoryRule rule = resolver.Resolve("meds");
		var matched = resolver.EnumerateMatches(rule).Select(i => i.InternalName).ToList();
		Assert.Contains("medicalBandage", matched);
		Assert.DoesNotContain("drinkJarBeer", matched); // in Medical AND excluded Food/Cooking
	}

	[Fact]
	public void Sign_labels_without_an_alias_resolve_literally()
	{
		CategoryRule rule = Resolver().Resolve("foodCanChili");
		Assert.True(rule.HasItemRules);
	}

	[Fact]
	public void Item_tier_matching_uses_only_explicit_item_rules()
	{
		// A crate labelled with a group must NOT participate in the
		// item-priority tier — that priority is reserved for explicit items.
		var resolver = Resolver();
		CategoryRule groupRule = resolver.Resolve("Medical");
		CatalogItem bandage = Catalog().FindByInternalName("medicalbandage");
		Assert.False(groupRule.Matches(bandage, MatchTier.Item));
		Assert.True(groupRule.Matches(bandage, MatchTier.Group));
	}

	[Fact]
	public void Resolved_rules_are_cached_per_label()
	{
		var resolver = Resolver();
		Assert.Same(resolver.Resolve("Medical"), resolver.Resolve("MEDICAL"));
	}

	[Fact]
	public void ResolveMany_skips_labels_that_resolve_to_nothing()
	{
		var rules = Resolver(warnUnknownGroups: false)
			.ResolveMany(new[] { "medical", "no-such-thing" });
		Assert.Single(rules);
	}

	[Fact]
	public void Rules_carry_the_priority_of_their_alias_definition()
	{
		var config = new AliasConfiguration(
			Alias("cans", "foodCan*"),
			warnUnknownGroups: true,
			priorityByLabel: new Dictionary<string, int> { ["cans"] = 3 });
		var resolver = new CategoryRuleResolver(Catalog(), new RecordingLog(), config);
		Assert.Equal(3, resolver.Resolve("cans").Priority);
	}

	[Fact]
	public void Literal_sign_labels_rank_after_all_alias_definitions()
	{
		CategoryRule rule = Resolver().Resolve("foodCanChili");
		Assert.Equal(int.MaxValue, rule.Priority);
	}

	// The fuzzy end-to-end promise: a sign saying "Mods \ Tools" (or any
	// normalized variant) resolves through the "Mod Tools" alias, including
	// its priority.
	[Fact]
	public void Label_variants_resolve_through_the_normalized_alias()
	{
		var config = new AliasConfiguration(
			Alias("mod tools", "foodCan*"),
			warnUnknownGroups: true,
			priorityByLabel: new Dictionary<string, int> { ["mod tools"] = 7 });
		var resolver = new CategoryRuleResolver(Catalog(), new RecordingLog(), config);
		CategoryRule rule = resolver.Resolve("Mods \\ Tools");
		Assert.True(rule.HasItemRules);
		Assert.Equal(7, rule.Priority);
	}

	// Label-to-label aliasing is how translations work: "Dosen = Cans" gives
	// a German sign the complete Cans rule, exclusions included.
	[Fact]
	public void Labels_can_reference_other_labels_and_inherit_their_full_rule()
	{
		var config = new AliasConfiguration(new Dictionary<string, IReadOnlyList<string>>
		{
			["cans"] = new[] { "foodCan*", "-foodCanShamSchematic" },
			["dosen"] = new[] { "Cans" }
		}, warnUnknownGroups: true);
		var resolver = new CategoryRuleResolver(Catalog(), new RecordingLog(), config);
		var matched = resolver.EnumerateMatches(resolver.Resolve("dosen"))
			.Select(i => i.InternalName).ToList();
		Assert.Contains("foodCanBeef", matched);
		Assert.DoesNotContain("foodCanShamSchematic", matched); // exclusion inherited
	}

	// "Crafting = Chemicals, Resources" must keep meaning the Resources GROUP
	// even though a "Resources" crate label also exists. Bare tokens resolve
	// group-first; only "@Resources" forces the label.
	[Fact]
	public void Group_names_win_over_implicit_label_references()
	{
		var config = new AliasConfiguration(new Dictionary<string, IReadOnlyList<string>>
		{
			["medical"] = new[] { "foodCan*" },  // a crate label shadowing a group name
			["meds"] = new[] { "Medical" }
		}, warnUnknownGroups: true);
		var resolver = new CategoryRuleResolver(Catalog(), new RecordingLog(), config);
		CategoryRule rule = resolver.Resolve("meds");
		Assert.True(rule.HasGroupRules);
		Assert.False(rule.HasItemRules);
	}

	[Fact]
	public void At_prefixed_references_beat_group_names()
	{
		var config = new AliasConfiguration(new Dictionary<string, IReadOnlyList<string>>
		{
			["medical"] = new[] { "foodCan*" },
			["meds"] = new[] { "@Medical" }
		}, warnUnknownGroups: true);
		var resolver = new CategoryRuleResolver(Catalog(), new RecordingLog(), config);
		CategoryRule rule = resolver.Resolve("meds");
		Assert.True(rule.HasItemRules);
		Assert.False(rule.HasGroupRules);
	}

	[Fact]
	public void Circular_references_are_broken_with_a_warning()
	{
		var config = new AliasConfiguration(new Dictionary<string, IReadOnlyList<string>>
		{
			["a"] = new[] { "@b" },
			["b"] = new[] { "@a" }
		}, warnUnknownGroups: true);
		var log = new RecordingLog();
		var resolver = new CategoryRuleResolver(Catalog(), log, config);
		Assert.True(resolver.Resolve("a").IsEmpty);
		Assert.Contains(log.Warnings, w => w.Contains("circular"));
	}

	// A translated label keeps its OWN line position for priority; it does
	// not steal the priority of the label it references.
	[Fact]
	public void References_keep_their_own_line_priority()
	{
		var config = new AliasConfiguration(new Dictionary<string, IReadOnlyList<string>>
		{
			["cans"] = new[] { "foodCan*" },
			["dosen"] = new[] { "@Cans" }
		}, warnUnknownGroups: true,
			priorityByLabel: new Dictionary<string, int> { ["cans"] = 1, ["dosen"] = 9 });
		var resolver = new CategoryRuleResolver(Catalog(), new RecordingLog(), config);
		Assert.Equal(9, resolver.Resolve("dosen").Priority);
	}

	[Fact]
	public void Excluding_a_reference_removes_everything_it_matches()
	{
		var config = new AliasConfiguration(new Dictionary<string, IReadOnlyList<string>>
		{
			["schematics"] = new[] { "foodCanShamSchematic" },
			["cans"] = new[] { "foodCan*", "-@schematics" }
		}, warnUnknownGroups: true);
		var resolver = new CategoryRuleResolver(Catalog(), new RecordingLog(), config);
		var matched = resolver.EnumerateMatches(resolver.Resolve("cans"))
			.Select(i => i.InternalName).ToList();
		Assert.Contains("foodCanBeef", matched);
		Assert.DoesNotContain("foodCanShamSchematic", matched);
	}

	[Fact]
	public void An_at_reference_to_a_missing_label_warns_and_is_ignored()
	{
		var log = new RecordingLog();
		var config = new AliasConfiguration(
			Alias("x", "@NoSuchLabel"), warnUnknownGroups: true);
		var resolver = new CategoryRuleResolver(Catalog(), log, config);
		Assert.True(resolver.Resolve("x").IsEmpty);
		Assert.Single(log.Warnings);
	}
}
