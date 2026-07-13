using System;
using System.Collections.Generic;

namespace M00StowIt.Core;

public enum StashPassKind
{
	// Fill stacks of items the crate already holds (may create new stacks of
	// those item types).
	TopUpExistingStacks,

	// The crate's explicit item rules take their items.
	TakeItemRuleMatches,

	// The crate's group rules take their items.
	TakeGroupRuleMatches,

	// The fallback crate sweeps everything that is still unrouted.
	TakeRemainderAsFallback
}

public readonly struct StashPass
{
	public StashPass(int crateIndex, StashPassKind kind)
	{
		CrateIndex = crateIndex;
		Kind = kind;
	}

	public int CrateIndex { get; }

	public StashPassKind Kind { get; }
}

// What the planner needs to know about one crate; derived from its resolved
// rules by the game layer.
public sealed class CrateRoutingInfo
{
	public CrateRoutingInfo(bool isFallback, bool hasItemRules, bool hasGroupRules, int priority = 0)
	{
		IsFallback = isFallback;
		HasItemRules = hasItemRules;
		HasGroupRules = hasGroupRules;
		Priority = priority;
	}

	public bool IsFallback { get; }

	public bool HasItemRules { get; }

	public bool HasGroupRules { get; }

	// Lower value = drains earlier within its tier (alias-file line order).
	public int Priority { get; }
}

// Produces the ordered pass list for one quick-stack operation. The order is
// the routing contract: top-up first, then item-rule crates (most specific),
// then group-rule crates, then the fallback sweep. Items physically move each
// pass, which is how earlier passes take priority over later ones. Within the
// item and group tiers, crates drain in alias-definition priority order, so
// "Ammo 9mm" (defined above "Ammo") empties before a generic crate.
public static class StashPassPlanner
{
	public static IReadOnlyList<StashPass> Plan(IReadOnlyList<CrateRoutingInfo> crates)
	{
		var plan = new List<StashPass>();
		for (int i = 0; i < crates.Count; i++)
		{
			if (!crates[i].IsFallback)
			{
				plan.Add(new StashPass(i, StashPassKind.TopUpExistingStacks));
			}
		}
		foreach (int i in ByPriority(crates, c => !c.IsFallback && c.HasItemRules))
		{
			plan.Add(new StashPass(i, StashPassKind.TakeItemRuleMatches));
		}
		foreach (int i in ByPriority(crates, c => !c.IsFallback && c.HasGroupRules))
		{
			plan.Add(new StashPass(i, StashPassKind.TakeGroupRuleMatches));
		}
		for (int i = 0; i < crates.Count; i++)
		{
			if (crates[i].IsFallback)
			{
				plan.Add(new StashPass(i, StashPassKind.TakeRemainderAsFallback));
			}
		}
		return plan;
	}

	private static List<int> ByPriority(IReadOnlyList<CrateRoutingInfo> crates, Func<CrateRoutingInfo, bool> include)
	{
		var indexes = new List<int>();
		for (int i = 0; i < crates.Count; i++)
		{
			if (include(crates[i]))
			{
				indexes.Add(i);
			}
		}
		indexes.Sort((a, b) =>
		{
			int byPriority = crates[a].Priority.CompareTo(crates[b].Priority);
			return byPriority != 0 ? byPriority : a.CompareTo(b);
		});
		return indexes;
	}
}
