# Editing rules from inside the game

Everything you can do in `CrateLabels.txt` with Notepad, you can also
do from the game console (press F1):

```
stow alias Breakfast = foodEgg, foodHoney     make a new label
stow alias add Breakfast = foodBaconAndEggs   put more items on it
stow alias remove Breakfast = foodHoney       take an item off it
stow alias delete Breakfast                   remove the label completely
stow alias list                               show every label you have
```

The change is saved to `CrateLabels.txt` immediately and starts working
right away. A backup of the previous version is kept next to it as
`CrateLabels.txt.bak`, in case you want to go back.

One thing to know: a brand new label made this way lands at the bottom
of the file, which is the lowest priority. If it needs to win over
another crate, open the file and move its line up.

Next: [The Misc crate](misc-crate.md)
