# Frequently asked questions

## Nothing happens when I press LeftAlt + X

Check these in order:

1. Are you standing close enough? The default reach is 7 blocks in every
   direction. Stand in the middle of your sorting room.
2. Is a container open? Sorting refuses to run while you (or anyone) has
   a crate open, to keep multiplayer safe. Close it first.
3. Are the crates the right kind? Only Writable Storage Crates with
   signs are sorted into by category. Other containers only receive
   top-ups of items they already hold.
4. Is the mod loaded? Press F1 and type `stow`. If the console says
   unknown command, the mod did not load. Make sure EAC is off in the
   launcher and the folder is `Mods\M00-StowIt` with the DLL inside.
5. Is `0_TFP_Harmony` still there? See the next question.

## The mod does not load and 0_TFP_Harmony is missing

The game itself ships a folder called `0_TFP_Harmony` inside the
`Mods` folder of the game install. Every DLL mod needs it, this one
included. It often gets deleted by accident when people clear out old
mods, and then no DLL mod loads at all.

To get it back, let Steam repair the install:

1. In your Steam library, right-click **7 Days to Die** and pick
   **Properties**.
2. Open **Installed Files**.
3. Click **Verify integrity of game files**.

Steam re-downloads the missing vanilla files, `0_TFP_Harmony`
included. Do not put your own copy there or move it; it belongs to
the game.

One thing that is *not* a problem: if you keep your mods in
`%APPDATA%\7DaysToDie\Mods`, you will not see `0_TFP_Harmony` next to
them. It lives only in the game install's `Mods` folder, and the game
reads both folders. Missing from the install folder: broken. Missing
from the AppData folder: normal.

## An item went to a crate I did not expect

Press F1 and ask the mod why:

```
stow what Ammo
```

That lists everything the `Ammo` crate receives. The usual reasons:

- **The crate already held one.** Crates top up items they already
  contain before the category rules run. Take the strays out once and
  the routing takes over from then on.
- **Another rule matched first.** Rules higher in `CrateLabels.txt` win.
  Move your line up if it should beat a general rule.

## Where do new or unknown items go?

To the crate signed `Misc`. If you have no Misc crate, unmatched items
simply stay in your backpack. Nothing is ever destroyed or lost.

## Does it work with items from other mods?

Yes. The item list is read from the running game, so modded items sort
like vanilla ones. See [Items from other mods](modded-items.md) for how
to give them their own crates.

## Can I change the key bindings?

Yes — any keys you like. Not in the game's keyboard options (those
only list the game's own actions), but by editing `StowItConfig.xml`.
It takes a minute and works for any combination.
[Buttons and keys](controls.md) walks you through it.

## Does it touch my toolbelt?

No. Only the backpack is sorted. Your toolbelt, worn armor and equipped
items are never moved. Backpack slots you lock with the game's own lock
button are skipped too.

## Two crates with the same sign?

Works fine. The one closest to you fills first; when it is full, the
rest goes to the next one.

## Does it work in multiplayer and on dedicated servers?

Yes. The mod uses the game's own container locking, so two players
sorting into the same room cannot lose or duplicate items. Every player
needs the mod installed (it is a DLL mod, so it does not push itself
from the server).

## Can I use it with EAC (anti-cheat) on?

No. Like every code mod, it only loads with EAC turned off in the game
launcher. With EAC on, the game simply skips the mod; nothing breaks.

## Can I add it to an existing save?

Yes — no new game needed, and your containers can keep whatever names
they already have. StowIt never writes into your save files: it reads
crate signs only at the moment you press the sort key, and any sign
text it does not recognise is simply ignored (at most a yellow note in
the log). A crate without a recognised label just receives top-ups of
items it already holds. Label the crates you want sorted into whenever
it suits you — there is nothing to rename or empty first.

## Is it safe to uninstall?

Yes. Delete the mod folder and everything stays where it was sorted.
The mod never changes game files or your save; it only moves items
between containers, which is a normal game action.

## I edited CrateLabels.txt and broke something

You cannot really break it. Bad lines are skipped with a warning in the
log, and every in-game edit keeps a backup as `CrateLabels.txt.bak`. If
things get confusing, restore the backup or re-download the mod zip and
copy the original file back.

## Sorting feels different since I rearranged my base

Distance matters. Crates fill nearest-first, and the search box is
centered on you. If you always sort from the same doorway, the same
crates win the ties. Stand somewhere else and equal crates may fill in
a different order. The categories themselves never change.

## How do I see what is happening under the hood?

The game log shows one line per sort: how many containers were found and
how fast. Every crate label also logs what it resolved to the first time
it is seen. If you enjoy that level of detail, the developer docs in the
[docs folder](../README.md) go much deeper.
