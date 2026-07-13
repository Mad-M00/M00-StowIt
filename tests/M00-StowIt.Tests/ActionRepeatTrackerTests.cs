using M00StowIt.Core;
using Xunit;

namespace M00StowIt.Tests;

// Double-tapping Quick Stack escalates from "fill existing stacks" to "fill
// and create new stacks" (vanilla routing mode and restock). The tracker owns
// that double-tap window; the game layer maps repeat/non-repeat to move kinds.
public class ActionRepeatTrackerTests
{
	[Fact]
	public void A_second_action_within_the_window_is_a_repeat()
	{
		var tracker = new ActionRepeatTracker(windowSeconds: 2f);
		Assert.False(tracker.IsRepeat(actionKey: 1, nowSeconds: 10f));
		Assert.True(tracker.IsRepeat(actionKey: 1, nowSeconds: 11f));
	}

	[Fact]
	public void Actions_after_the_window_are_not_repeats()
	{
		var tracker = new ActionRepeatTracker(windowSeconds: 2f);
		tracker.IsRepeat(1, 10f);
		Assert.False(tracker.IsRepeat(1, 12.5f));
	}

	[Fact]
	public void Different_actions_are_tracked_independently()
	{
		var tracker = new ActionRepeatTracker(windowSeconds: 2f);
		tracker.IsRepeat(1, 10f);
		Assert.False(tracker.IsRepeat(2, 10.5f)); // restock right after stack is not a repeat
	}

	[Fact]
	public void The_first_action_is_never_a_repeat()
	{
		var tracker = new ActionRepeatTracker(windowSeconds: 2f);
		Assert.False(tracker.IsRepeat(1, 0.1f)); // even immediately after game start
	}
}
