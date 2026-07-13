using System.Collections.Generic;
using System.Linq;
using M00StowIt.Core;
using Xunit;

namespace M00StowIt.Tests;

// The pass order IS the sorting behaviour players observe: crates first top up
// what they already hold, then item-labelled crates take their items, then
// group-labelled crates, and the fallback crate sweeps last. Reordering these
// passes re-routes items, so the order is pinned here.
public class StashPassPlannerTests
{
	private static CrateRoutingInfo Unsigned() => new(isFallback: false, hasItemRules: false, hasGroupRules: false);

	private static CrateRoutingInfo ItemCrate() => new(isFallback: false, hasItemRules: true, hasGroupRules: false);

	private static CrateRoutingInfo GroupCrate() => new(isFallback: false, hasItemRules: false, hasGroupRules: true);

	private static CrateRoutingInfo MixedCrate() => new(isFallback: false, hasItemRules: true, hasGroupRules: true);

	private static CrateRoutingInfo Fallback() => new(isFallback: true, hasItemRules: false, hasGroupRules: false);

	[Fact]
	public void Passes_run_most_specific_first_with_fallback_last()
	{
		var plan = StashPassPlanner.Plan(new[] { GroupCrate(), ItemCrate(), Fallback() });
		var kinds = plan.Select(p => p.Kind).ToList();
		int topUp = kinds.LastIndexOf(StashPassKind.TopUpExistingStacks);
		int item = kinds.IndexOf(StashPassKind.TakeItemRuleMatches);
		int group = kinds.IndexOf(StashPassKind.TakeGroupRuleMatches);
		int fallback = kinds.IndexOf(StashPassKind.TakeRemainderAsFallback);
		Assert.True(topUp < item, "top-up must run before item rules");
		Assert.True(item < group, "item rules must run before group rules");
		Assert.True(group < fallback, "fallback must sweep last");
	}

	[Fact]
	public void The_fallback_crate_only_receives_the_final_sweep()
	{
		var plan = StashPassPlanner.Plan(new[] { Fallback(), ItemCrate() });
		var fallbackPasses = plan.Where(p => p.CrateIndex == 0).ToList();
		Assert.Single(fallbackPasses);
		Assert.Equal(StashPassKind.TakeRemainderAsFallback, fallbackPasses[0].Kind);
	}

	[Fact]
	public void Unsigned_crates_only_top_up_their_existing_stacks()
	{
		var plan = StashPassPlanner.Plan(new[] { Unsigned() });
		Assert.Single(plan);
		Assert.Equal(StashPassKind.TopUpExistingStacks, plan[0].Kind);
	}

	[Fact]
	public void A_crate_with_item_and_group_rules_participates_in_both_tiers()
	{
		var plan = StashPassPlanner.Plan(new[] { MixedCrate() });
		var kinds = plan.Select(p => p.Kind).ToList();
		Assert.Contains(StashPassKind.TopUpExistingStacks, kinds);
		Assert.Contains(StashPassKind.TakeItemRuleMatches, kinds);
		Assert.Contains(StashPassKind.TakeGroupRuleMatches, kinds);
	}

	[Fact]
	public void Crate_order_is_preserved_within_a_pass()
	{
		var plan = StashPassPlanner.Plan(new List<CrateRoutingInfo> { ItemCrate(), ItemCrate(), ItemCrate() });
		var itemPassIndexes = plan
			.Where(p => p.Kind == StashPassKind.TakeItemRuleMatches)
			.Select(p => p.CrateIndex)
			.ToList();
		Assert.Equal(new[] { 0, 1, 2 }, itemPassIndexes);
	}

	// The early-game/late-game story: with only a generic "Mods" crate it
	// catches everything; once a "Mods \ Weapons" crate exists (defined
	// earlier in the alias file = lower priority number), it must drain first
	// even though both crates sit in the same item tier.
	[Fact]
	public void Within_the_item_tier_higher_priority_crates_drain_first()
	{
		var generic = new CrateRoutingInfo(isFallback: false, hasItemRules: true, hasGroupRules: false, priority: 9);
		var specific = new CrateRoutingInfo(isFallback: false, hasItemRules: true, hasGroupRules: false, priority: 2);
		var plan = StashPassPlanner.Plan(new[] { generic, specific });
		var itemPassIndexes = plan
			.Where(p => p.Kind == StashPassKind.TakeItemRuleMatches)
			.Select(p => p.CrateIndex)
			.ToList();
		Assert.Equal(new[] { 1, 0 }, itemPassIndexes);
	}

	[Fact]
	public void Within_the_group_tier_higher_priority_crates_drain_first()
	{
		var generic = new CrateRoutingInfo(isFallback: false, hasItemRules: false, hasGroupRules: true, priority: 9);
		var specific = new CrateRoutingInfo(isFallback: false, hasItemRules: false, hasGroupRules: true, priority: 2);
		var plan = StashPassPlanner.Plan(new[] { generic, specific });
		var groupPassIndexes = plan
			.Where(p => p.Kind == StashPassKind.TakeGroupRuleMatches)
			.Select(p => p.CrateIndex)
			.ToList();
		Assert.Equal(new[] { 1, 0 }, groupPassIndexes);
	}

	[Fact]
	public void Equal_priority_crates_keep_their_scan_order()
	{
		var plan = StashPassPlanner.Plan(new[]
		{
			new CrateRoutingInfo(isFallback: false, hasItemRules: true, hasGroupRules: false, priority: 5),
			new CrateRoutingInfo(isFallback: false, hasItemRules: true, hasGroupRules: false, priority: 5)
		});
		var itemPassIndexes = plan
			.Where(p => p.Kind == StashPassKind.TakeItemRuleMatches)
			.Select(p => p.CrateIndex)
			.ToList();
		Assert.Equal(new[] { 0, 1 }, itemPassIndexes);
	}
}
