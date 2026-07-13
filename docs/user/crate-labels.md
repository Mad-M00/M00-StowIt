# Editing crate rules

All the sorting rules live in one text file in the mod folder:
`CrateLabels.txt`. Open it with Notepad. You cannot break your game
with it: a bad line is skipped, and the mod notes it in the log.

Every change applies live: save the file, press F1 in the game, type
`stow reload`.

## How a rule reads

```
Cans = foodCan*
```

Left side: what you write on the crate sign. Right side: what the
crate receives. Read it as "a crate signed Cans receives every item
whose name starts with foodCan". The star means "anything".

## A new crate for eggs and honey

Add one line:

```
Breakfast = foodEgg, foodHoney
```

Save, `stow reload`, write `Breakfast` on a crate. Done.

## Keep one thing out of a crate

A minus sign means "but not this one":

```
Cans = foodCan*, -foodCanShamSchematic
```

All the cans, but the schematic stays out (it is a book, so the Books
crate takes it).

## Give a crate a second name

The @ sign means "exactly the same as that label":

```
Loot = @Misc
Kitchen = @Cooking
```

## Rules higher up win

Early game, one `Ammo` crate takes everything. Later you want calibers
separated. Those rules are already in the file, in this order:

```
Ammo 9mm = ammo9mm*, ammoBundle9mm*
Ammo Shotgun = ammoShotgun*, ammoBundleShotgun*
Ammo = Ammo
```

The moment you place a crate signed `Ammo 9mm`, the 9mm rounds stop
going to the general `Ammo` crate, because its rule sits higher in the
file. When you add your own lines, remember: **put picky rules above
catch-all rules.**

Next: [Finding item names](finding-item-names.md)
