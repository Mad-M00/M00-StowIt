namespace M00StowIt.Core;

// Which routing tier a match is evaluated for. Item rules are more specific
// than group rules and therefore run in an earlier stash pass.
public enum MatchTier
{
	Item,
	Group
}
