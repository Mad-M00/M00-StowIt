M00 StowIt - How to use it and how to change it
====================================================

WHAT THIS MOD DOES

You put a Writable Storage Crate down, you write a word on its sign
(like "Ammo" or "Food"), and when you press LeftAlt + X the mod takes
everything out of your backpack and puts each item into the right crate.

That's it. Sort once, never again.


GETTING STARTED IN THE GAME

1. Craft some Writable Storage Crates and place them near each other.
2. Look at a crate, press E to open its sign, and type a category name.
   For example: Ammo
3. Make one crate with the sign "Misc". Anything that has no home
   goes in there, so nothing gets lost.
4. Stand near your crates and press LeftAlt + X. Watch your bags empty.

You don't need to be exact with the signs. "Mod Tools", "Mods \ Tools"
and "MOD-TOOLS" all mean the same thing to the mod. Big or small
letters make no difference either.

These category names are ready to use right now:

  Drinks, Cans, Cooking, Food, Buffs, Medical, Crafting, Resources,
  Farming, Building, Decor, Electrical, Ammo, Motor, Robotics, Books,
  Parts, Tools, Armour, Weapons, Clothing, Mods, Mod Tools,
  Mod Weapons, Mod Armor, Misc

When you get rich later, you can add extra crates like "Ammo 9mm" or
"Ammo Shotgun". They start working the moment you place them, and the
plain "Ammo" crate keeps whatever doesn't have its own crate yet.


THE FILE YOU CAN EDIT

All the sorting rules live in one text file, in the same folder as
this README:

  CrateLabels.txt

To open it: right click the file, pick "Open with", then "Notepad".
It's a normal text file. You can't damage your game by editing it.
Worst case, the mod ignores a bad line and writes a note in the game
log about it.

Tip: before your first edit, copy the file somewhere safe. If things
get confusing, put the copy back and you're good again.


HOW A RULE LOOKS

Each rule is one line, like this:

  Cans = foodCan*

Read it as: "a crate with the sign Cans receives every item whose
name starts with foodCan".

The left side is what you write on the sign. The right side says
which items belong there. You can list more than one thing, with
commas between them:

  Cooking = foodEgg, foodHoney, foodCrop*

Three kinds of things can go on the right side:

  1. A group name, like Medical or Food/Cooking. That means "every
     item the game puts in that group". Type "stow groups" in the game
     console to see them all.

  2. An item name, like foodCanChili. Type "stow search can" in the
     console to find the real names of items.

  3. A pattern with a star, like foodCan*. The star means "anything".
     So foodCan* matches foodCanChili, foodCanBeef and so on.

One more trick: a minus sign in front means "but not this one".

  Cans = foodCan*, -foodCanShamSchematic

That says: all the cans, but not the schematic (that one is a book,
so it belongs with Books).


LINES HIGHER UP WIN

If two crates could both take an item, the rule that sits higher in
the file wins. That's why "Ammo 9mm" is written above "Ammo". Keep
the picky rules near the top and the catch-all rules below them.


CHECKING YOUR WORK

Open the game console with F1 and try these:

  stow what Cans        shows exactly which items the Cans crate takes
  stow search shotgun   finds item names with "shotgun" in them
  stow groups           lists every group name you can use
  stow reload           loads your edits without restarting the game

So the routine is: edit the file, save it, press F1, type "stow reload",
then "stow what YourLabel" to see if it does what you wanted.

If you typed something the mod doesn't recognise, it tells you in the
game log and skips it. Nothing breaks.


OTHER LANGUAGES

Crate signs work in every language the game ships with. Write "Dosen",
"Munición", "弾薬" or "Патроны" on a sign and it behaves like the
English crate. Each language has its own file next to this one, named
with the usual language codes: CrateLabels.de.txt for German,
CrateLabels.fr.txt for French, CrateLabels.ja.txt for Japanese
and so on. Don't want a language? Delete its file. Every line inside
looks like this:

  Dosen = @Cans

The @ sign means "exactly the same as that label". You can use it in
your own rules too, for example to give a crate a second name.

The catch-all "Misc" crate is the one exception: its name is set in
StowItConfig.xml (FallbackCrateName). Change it there if
you want it in your language.


EDITING WITHOUT LEAVING THE GAME

You don't have to open Notepad at all if you don't want to. The same
edits can be done from the game console (press F1):

  stow alias Breakfast = foodEgg, foodHoney     make a new label
  stow alias add Breakfast = foodBaconAndEggs   put more items on it
  stow alias remove Breakfast = foodHoney       take an item off it
  stow alias delete Breakfast                   remove the label completely
  stow alias list                               show every label you have

Each change is saved into CrateLabels.txt straight away and starts
working immediately. The mod also keeps a copy of the file as it was
before your change, named CrateLabels.txt.bak, in case you want
to go back.

One thing to know: a brand new label made this way lands at the bottom
of the file, which means it has the lowest priority. If you need it to
win over another crate, open the file and move its line up.


A SMALL EXAMPLE FROM START TO END

Say you want a crate only for eggs and honey. Open the file, pick a
free spot above the Food line, and add:

  Breakfast = foodEgg, foodHoney

Save the file. In the game press F1, type "stow reload", then write
"Breakfast" on a crate sign. Done. Eggs and honey now skip the Food
crate and land in your new one.

Have fun, and happy hoarding.


IF SOMETHING DOES NOT WORK

Answers to the usual questions (mod not loading, items landing in
the wrong crate, the missing 0_TFP_Harmony folder and more) are in
the online FAQ:

  https://github.com/Mad-M00/M00-StowIt/blob/main/docs/user/faq.md

The full player guide lives one folder up from there.


CREDITS

This mod was inspired by the work of two other modders:

  Westwud, who created the QuickStack mod
  Walber, who created Walber-AutoSorter

Thank you both for the inspiration.
