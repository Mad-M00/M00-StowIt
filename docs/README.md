# StowIt documentation

## For players

Short pages, one topic each, in reading order:

| Page | Topic |
|---|---|
| [user/getting-started.md](user/getting-started.md) | Your first sorting room |
| [user/controls.md](user/controls.md) | Buttons and keys |
| [user/sorting-modes.md](user/sorting-modes.md) | Category mode vs Vanilla mode |
| [user/restock.md](user/restock.md) | Refilling your backpack from crates |
| [user/slot-locking.md](user/slot-locking.md) | The game's locked slots are always respected |
| [user/sort-distance.md](user/sort-distance.md) | How far the mod reaches |
| [user/crate-labels.md](user/crate-labels.md) | Editing crate rules |
| [user/finding-item-names.md](user/finding-item-names.md) | Finding item names with the console |
| [user/modded-items.md](user/modded-items.md) | Items from other mods |
| [user/editing-in-game.md](user/editing-in-game.md) | Editing rules without leaving the game |
| [user/misc-crate.md](user/misc-crate.md) | The Misc catch-all crate |
| [user/languages.md](user/languages.md) | Signs in other languages |
| [user/faq.md](user/faq.md) | Frequently asked questions |

## For mod authors

These documents explain how the mod is built, written for 7 Days To Die
mod authors. Most game mods are one big static class full of Harmony
patches; this one is deliberately not, and each document explains one
part of why — with diagrams — so you can steal the ideas for your own
mods.

| Document | What it covers |
|---|---|
| [architecture.md](architecture.md) | The three layers, the dependency rule, and the one static you cannot avoid |
| [sorting-pipeline.md](sorting-pipeline.md) | End to end: keypress → container locks → routing passes → items moved |
| [label-resolution.md](label-resolution.md) | How the text on a crate sign becomes a routing rule |
| [testing.md](testing.md) | 97 unit tests that run without the game installed, and how that is possible |

Suggested reading order: architecture first, then sorting-pipeline, then
the other two in any order.
