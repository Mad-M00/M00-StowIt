using System.Collections.Generic;

namespace M00StowIt.Core;

// Detects a quick second press of the same action ("double-tap"), which the
// game layer escalates to a more aggressive item-move kind.
public sealed class ActionRepeatTracker
{
	private readonly float windowSeconds;
	private readonly Dictionary<int, float> lastActionTimes = new();

	public ActionRepeatTracker(float windowSeconds)
	{
		this.windowSeconds = windowSeconds;
	}

	public bool IsRepeat(int actionKey, float nowSeconds)
	{
		bool isRepeat = lastActionTimes.TryGetValue(actionKey, out float last)
			&& nowSeconds - last < windowSeconds;
		lastActionTimes[actionKey] = nowSeconds;
		return isRepeat;
	}
}
