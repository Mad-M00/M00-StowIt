# Items from other mods

Other mods' items work exactly the same way as vanilla items. The mod
reads the item list from the running game, so anything another mod
adds is already known.

Until you write rules for them, modded items either follow their item
group (if the other mod set one) or land safely in the Misc crate.

## Giving modded items their own crate

Find their names first:

```
stow search rifle
```

Then add a rule with what you found:

```
Guns = gunMod*, WeaponPack*
```

Save, `stow reload`, sign a crate `Guns`. From then on those items skip
Misc and go home.

If a modded item keeps landing in an unexpected crate, ask the mod why
with `stow what <that crate's sign>`. Usually a broad rule higher in the
file is catching it; move your rule above it.

Next: [Editing rules from inside the game](editing-in-game.md)
