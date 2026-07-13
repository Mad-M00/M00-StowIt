using M00StowIt.Core;

namespace M00StowIt.Game;

internal sealed class UnitySorterLog : ISorterLog
{
	public void Info(string message)
	{
		Log.Out("[StowIt] " + message);
	}

	public void Warning(string message)
	{
		Log.Warning("[StowIt] " + message);
	}
}
