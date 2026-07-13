using M00StowIt.Core;
using Xunit;

namespace M00StowIt.Tests;

// The top-up pass historically sent an item to ANY crate already holding a
// stack of it. That silently overrode the category rules: one stray bullet
// casing left in the Ammo crate would attract every future casing, even
// though casings belong to Crafting. This policy pins the fix.
public class TopUpPolicyTests
{
	[Fact]
	public void Items_routed_by_this_crate_still_top_up_here()
	{
		Assert.True(TopUpPolicy.ShouldTopUp(routedByThisCrate: true, routedByAnyCrate: true));
	}

	// The bullet-casing regression: routed items must only ever land in the
	// crates that route them (or the fallback), never via top-up elsewhere.
	[Fact]
	public void Items_routed_only_by_another_crate_never_top_up_here()
	{
		Assert.False(TopUpPolicy.ShouldTopUp(routedByThisCrate: false, routedByAnyCrate: true));
	}

	[Fact]
	public void Unrouted_items_keep_the_legacy_top_up_anywhere_behaviour()
	{
		Assert.True(TopUpPolicy.ShouldTopUp(routedByThisCrate: false, routedByAnyCrate: false));
	}
}
