namespace M00StowIt.Core;

public enum RoutingMode
{
	Vanilla,
	Category
}

public readonly struct GridRadius
{
	public GridRadius(int x, int y, int z)
	{
		X = x;
		Y = y;
		Z = z;
	}

	public int X { get; }

	public int Y { get; }

	public int Z { get; }
}

// All user-tunable settings. Key codes are Unity KeyCode values kept as ints
// so this type stays free of engine dependencies; the game layer casts them.
// Slot locking is deliberately NOT here: the game owns that feature, and the
// mod simply honours whatever slots the player locked with the vanilla UI.
public sealed class StowItSettings
{
	public RoutingMode Routing { get; internal set; } = RoutingMode.Category;

	public string FallbackCrateName { get; internal set; } = "Misc";

	public int[] SortKeyCodes { get; internal set; }

	public int[] RestockKeyCodes { get; internal set; }

	public GridRadius SortRadius { get; internal set; } = new(7, 7, 7);

	public static StowItSettings Defaults()
	{
		return new StowItSettings
		{
			SortKeyCodes = new[] { 308, 120 },   // LeftAlt + X
			RestockKeyCodes = new[] { 308, 122 } // LeftAlt + Z
		};
	}
}
