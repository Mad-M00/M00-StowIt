namespace M00StowIt.Core;

// Decides whether the top-up pass may move an item into a crate. "Routed"
// means some nearby crate's rules explicitly claim the item (either tier).
// Routed items only top up crates that route them, so category rules always
// win over "the crate already had one"; unrouted items top up anywhere, as
// they always did.
public static class TopUpPolicy
{
	public static bool ShouldTopUp(bool routedByThisCrate, bool routedByAnyCrate)
	{
		return routedByThisCrate || !routedByAnyCrate;
	}
}
