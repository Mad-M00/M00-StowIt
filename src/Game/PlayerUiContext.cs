namespace M00StowIt.Game;

// The backpack UI controllers the sorter operates on, captured when the game
// initialises the backpack window.
internal sealed class PlayerUiContext
{
	public XUiC_BackpackWindow BackpackWindow { get; private set; }

	public XUiC_Backpack Backpack { get; private set; }

	public XUiC_ContainerStandardControls Controls { get; private set; }

	public bool IsReady => BackpackWindow != null && Backpack != null && Controls != null;

	public void Capture(XUiC_BackpackWindow window)
	{
		BackpackWindow = window;
		Controls = window.GetChildByType<XUiC_ContainerStandardControls>();
		Backpack = window.backpackGrid;
	}

	// Called on world unload so this static-rooted context does not keep the
	// destroyed UI graph alive in memory.
	public void Clear()
	{
		BackpackWindow = null;
		Backpack = null;
		Controls = null;
	}
}
