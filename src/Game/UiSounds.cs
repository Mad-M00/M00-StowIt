namespace M00StowIt.Game;

internal static class UiSounds
{
	public static void PlayClick()
	{
		LocalPlayerUI.mPrimaryUI.mCursorController.PlayPagingSound();
	}
}
