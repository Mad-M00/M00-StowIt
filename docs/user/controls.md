# Buttons and keys

## The buttons

Two new buttons sit above your backpack window:

![The two StowIt buttons above the backpack grid](images/backpack-buttons.png)

- The **sort** button empties your backpack into nearby crates.
- The **restock** button refills your carried stacks from nearby crates.

## The keys

| Keys | What happens |
|---|---|
| LeftAlt + X | Sort your backpack into nearby crates |
| LeftAlt + Z | Restock your backpack from nearby crates |

Slots you lock with the game's own lock button are always skipped.

## Changing the keys

Open `StowItConfig.xml` in Notepad. Hotkeys are key codes, one per
key, separated by spaces — every listed key must be held, and the
last one triggers the action. Pick any codes from the
[tables below](#key-code-reference). For example, to sort with
LeftControl + S instead:

```xml
<SortButtons>306 115</SortButtons>
```

306 is LeftControl and 115 is S. Three-key combos work too:
`306 308 120` is LeftControl + LeftAlt + X. Save, then press F1 in
the game and type `stow reload`.

## When a key clashes with the game

You can change either side. If a StowIt hotkey overlaps a key you
use for something else — say you toggle crouch with LeftAlt — pick
different StowIt keys as shown above, or move the game's own action
to another key in the game's **Options, Controls**. Both routes work;
use whichever keeps your muscle memory intact.

## Key code reference

Every key you can use, with its code. The same list ships as a
comment at the bottom of `StowItConfig.xml`.

### Modifiers

| Key | Code | Key | Code |
|---|---|---|---|
| LeftShift | 304 | RightShift | 303 |
| LeftControl | 306 | RightControl | 305 |
| LeftAlt | 308 | RightAlt | 307 |
| CapsLock | 301 | Numlock | 300 |
| ScrollLock | 302 | | |

### Letters

| Key | Code | Key | Code | Key | Code | Key | Code |
|---|---|---|---|---|---|---|---|
| A | 97 | H | 104 | O | 111 | V | 118 |
| B | 98 | I | 105 | P | 112 | W | 119 |
| C | 99 | J | 106 | Q | 113 | X | 120 |
| D | 100 | K | 107 | R | 114 | Y | 121 |
| E | 101 | L | 108 | S | 115 | Z | 122 |
| F | 102 | M | 109 | T | 116 | | |
| G | 103 | N | 110 | U | 117 | | |

### Number row

| Key | Code | Key | Code | Key | Code | Key | Code | Key | Code |
|---|---|---|---|---|---|---|---|---|---|
| 0 | 48 | 2 | 50 | 4 | 52 | 6 | 54 | 8 | 56 |
| 1 | 49 | 3 | 51 | 5 | 53 | 7 | 55 | 9 | 57 |

### Function keys

| Key | Code | Key | Code | Key | Code | Key | Code |
|---|---|---|---|---|---|---|---|
| F1 | 282 | F4 | 285 | F7 | 288 | F10 | 291 |
| F2 | 283 | F5 | 286 | F8 | 289 | F11 | 292 |
| F3 | 284 | F6 | 287 | F9 | 290 | F12 | 293 |

### Arrows and navigation

| Key | Code | Key | Code |
|---|---|---|---|
| UpArrow | 273 | Insert | 277 |
| DownArrow | 274 | Delete | 127 |
| LeftArrow | 276 | Home | 278 |
| RightArrow | 275 | End | 279 |
| PageUp | 280 | PageDown | 281 |

### Punctuation

| Key | Code | Key | Code |
|---|---|---|---|
| Comma `,` | 44 | Semicolon `;` | 59 |
| Minus `-` | 45 | Equals `=` | 61 |
| Period `.` | 46 | LeftBracket `[` | 91 |
| Slash `/` | 47 | RightBracket `]` | 93 |
| Backslash `\` | 92 | BackQuote `` ` `` | 96 |

### Keypad

| Key | Code | Key | Code |
|---|---|---|---|
| Keypad0 | 256 | KeypadPeriod | 266 |
| Keypad1 | 257 | KeypadDivide | 267 |
| Keypad2 | 258 | KeypadMultiply | 268 |
| Keypad3 | 259 | KeypadMinus | 269 |
| Keypad4 | 260 | KeypadPlus | 270 |
| Keypad5 | 261 | KeypadEnter | 271 |
| Keypad6 | 262 | KeypadEquals | 272 |
| Keypad7 | 263 | Keypad8 | 264 |
| Keypad9 | 265 | | |

### Everything else

| Key | Code | Key | Code |
|---|---|---|---|
| Space | 32 | Escape | 27 |
| Tab | 9 | Pause | 19 |
| Return (Enter) | 13 | Clear | 12 |
| Backspace | 8 | None | 0 |

### Mouse buttons

| Button | Code |
|---|---|
| Mouse0 (left) | 323 |
| Mouse1 (right) | 324 |
| Mouse2 (middle) | 325 |
| Mouse3 | 326 |
| Mouse4 | 327 |
| Mouse5 | 328 |
| Mouse6 | 329 |

Next: [Sorting modes](sorting-modes.md)
