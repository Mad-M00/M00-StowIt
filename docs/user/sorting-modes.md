# Sorting modes

The mod has two sorting modes. You pick one in `StowItConfig.xml`.

## Category mode (the default)

Items are routed by the words on your crate signs. The order is always:

1. **Top up first.** Every crate first receives more of what it
   already holds, so your stacks stay together.
2. **Specific crates next.** A crate like `Ammo 9mm` takes its items
   before a general crate like `Ammo` gets a turn.
3. **General crates after that.** Crates signed with a broad category,
   like `Food` or `Ammo`, take what matches them.
4. **Misc last.** Whatever still has no home goes to the crate signed
   `Misc`.

If two crates have the same sign, the one closest to you fills up
first, then the overflow moves to the next one.

## Vanilla mode

Items only go into crates that
already contain that item type. Signs are ignored. Pressing the key
twice within two seconds also creates new stacks instead of only
topping up existing ones.

Use this if you already have a hand-sorted base and only want the
"deposit matching items" convenience.

## Switching modes

Open `StowItConfig.xml` in Notepad and find this line:

```xml
<SortingMode>Category</SortingMode>
```

Change `Category` to `Vanilla` (or back). Save, then press F1 in the
game and type `stow reload`. No restart needed.

Next: [Restock](restock.md)
