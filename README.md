# liblouis-jsonify-tables

A small app serving to map LibLouis tables to JSON suitable for processing in GUI apps.

## Usage

```
lljt <tables folder> <output JSON file name>
```

The tables folder is a LibLouis `tables/` directory. Sub-tables meant to be pulled in with `include`
rather than selected on their own (`.dis`, `.cti`, `.uti`) are skipped, and a table is written out only
if it has both a display name and at least one language — the minimum needed to offer it in a picker.
Output is ordered by file name so a regenerated file diffs cleanly against the previous one.

## What it reads

LibLouis tables declare metadata in the table header — the run of comment and empty lines before the
first translation rule — as `#+key: value` (queryable) and `#-key: value` (informative) lines. Two
rules from the [LibLouis manual](https://liblouis.io/documentation/liblouis.html) govern how they are
read:

* metadata after the table header is not metadata, so parsing stops at the first translation rule;
* **the same key may appear multiple times in a table**, which is why `Languages` and `TableTypes` are
  arrays. `he-IL.utb` (Israeli braille) declares Hebrew, Arabic and English; `ancient-languages-us.utb`
  declares thirty-six languages; the Swedish and Elfdalian 8-dot tables declare both a `computer` and a
  `literary` type.

Keys are matched exactly rather than by substring — the display names of `ancient-languages-us.utb` and
`ancient-languages-borger.utb` contain the word "language", which a substring match mistakes for the
language field.

## Output

```json
{
  "FileName": "he-IL.utb",
  "DisplayName": "Israeli braille",
  "IndexName": "Hebrew, modern",
  "Languages": [ "he", "ar", "en" ],
  "Region": "*-IL",
  "TableTypes": [ "literary" ],
  "ContractionType": "no",
  "Grade": "1",
  "DotsMode": 6,
  "Direction": "forward",
  "System": null,
  "Variant": null,
  "Version": null,
  "Locale": null,
  "UnicodeRange": null
}
```

Every field but `FileName` is optional in a table header, so most come out `null` (or an empty array)
for most tables.

* `FileName` — the table file name, from the file itself.
* `DisplayName` — from `#-display-name`. Required for a table to be written out at all.
* `IndexName` — from `#-index-name`. The sort-friendly "Language, qualifiers" form, e.g.
  _English, U.S., computer, 8-dot_.
* `Languages` — an array, from every `#+language`. These are RFC 4647 extended language ranges, so
  entries such as `akk-Latn` and `*-fonipa` occur; match them, don't compare them as strings.
* `Region` — from `#+region`. Also an extended language range, e.g. `en-US` or `*-IL`.
* `TableTypes` — an array, from every `#+type`: `literary`, `computer`, `math`.
* `ContractionType` — from `#+contraction`: `no`, `partial` or `full`.
* `Grade` — from `#+grade`. A **string**, not a number: alongside 0, 1, 2 and 3, LibLouis ships tables
  graded 1.2, 1.3, 1.4 and 1.5.
* `DotsMode` — from `#+dots`: 6 or 8, or 0 when the table declares none.
* `Direction` — from `#+direction`: `forward`, `backward` or `both`.
* `System` — from `#+system`. The Braille code the table implements, e.g. `ueb`, `ebae`, `ddp`.
* `Variant` — from `#+variant`. Distinguishes otherwise-alike tables, e.g. `detailed`, `compact`.
* `Version` — from `#+version`. The edition of the Braille standard, usually a year.
* `Locale` — from `#+locale`. The conventions assumed, where these differ from the language.
* `UnicodeRange` — from `#+unicode-range`: `ucs2` or `ucs4`.

## Consumers

The JSON is consumed by [SharpLouis](https://github.com/accessmind/sharp-louis), whose
`TableCollection` and `TranslationTable` types mirror this schema.

## License

Copyright © 2024–2026 [André Polykanine](https://github.com/Menelion),
[AccessMind LLC.](https://accessmind.io/), and contributors.
Licensed under the Apache License, Version 2.0. See [LICENSE.md](LICENSE.md).
