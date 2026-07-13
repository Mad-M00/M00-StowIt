# How far it reaches

By default the mod searches a 15x15x15 block box centered on you. The
setting is in `StowItConfig.xml`:

```xml
<SortDistance>7 7 7</SortDistance>
```

The three numbers are the distance in blocks sideways (X), up and down
(Y), and forwards and backwards (Z). `7` means 7 blocks in each
direction from where you stand.

If your sorting room is bigger, raise the numbers. The limit is 127,
and bigger numbers do not slow anything down.

After changing the file: save, press F1 in the game, type `stow reload`.

Next: [Editing crate rules](crate-labels.md)
