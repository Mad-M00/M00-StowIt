# Languages

Crate signs work in every language the game ships with. Write `Dosen`,
`Munición`, `弾薬` or `Патроны` on a sign and it behaves exactly like
the English crate.

Each language has its own file in the mod folder, named with the usual
language codes:

| File | Language |
|---|---|
| `CrateLabels.de.txt` | German |
| `CrateLabels.es.txt` | Spanish |
| `CrateLabels.fr.txt` | French |
| `CrateLabels.it.txt` | Italian |
| `CrateLabels.ja.txt` | Japanese |
| `CrateLabels.ko.txt` | Korean |
| `CrateLabels.pl.txt` | Polish |
| `CrateLabels.pt-BR.txt` | Brazilian Portuguese |
| `CrateLabels.ru.txt` | Russian |
| `CrateLabels.tr.txt` | Turkish |
| `CrateLabels.zh-CN.txt` | Simplified Chinese |
| `CrateLabels.zh-TW.txt` | Traditional Chinese |

All of them are active at the same time, so a multilingual server just
works.

- Don't want a language? Delete its file.
- Want to fix or add a translation? Every line inside is a simple rule
  like `Dosen = @Cans`, which means "same as the Cans crate".
- Your own rules in `CrateLabels.txt` always win if a name appears in
  both places.

Next: [Frequently asked questions](faq.md)
