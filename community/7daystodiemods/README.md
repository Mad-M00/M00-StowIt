# 7daystodiemods.com listing

Source text for the mod's page on [7daystodiemods.com](https://7daystodiemods.com).

| File | What it is |
|---|---|
| `title.txt` | The listing title and the short description shown under it |
| `homepage.html` | The full page body, in HTML — 7daystodiemods.com's editor takes HTML, not BBCode |
| `install.html` | Detailed install instructions, in HTML, for the listing's install section |
| `homepage.bbcode.txt` | The same page body in BBCode, for sites that want BBCode (e.g. Nexus) |
| `install.bbcode.txt` | The install instructions in BBCode, same purpose |
| `download-instructions.txt` | Plain-text install steps for the site's simple "Download Instructions" field (their site hosts the zip) |
| `changelog.bbcode.txt` | Changelog for the listing in BBCode, starting at v1.0 |
| `thumbnail-400x225.png` | The listing thumbnail, at the site's required 400x225 |
| `promo-art.png` | The full-size promo artwork the thumbnail is cut from |

Keep the HTML and BBCode pairs saying the same thing — edit both when
the copy changes.

The BBCode embeds the banner and sorting-room screenshot straight from
this repo's `main` branch via raw.githubusercontent.com, so keeping
those images fresh keeps the listing fresh.

When the mod's features change, update the copy here first, then paste
it over the live listing so the repo stays the source of truth.
