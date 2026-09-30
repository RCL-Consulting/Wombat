---
id: T348
title: A draft of a type whose move is Log still says until you submit it
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-30
---

# T348 — A draft of a type whose move is Log still says "until you submit it"

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Wording only; the teaching log (and any self-logged type) works.
**Surfaced:** 2026-09-30, T342 step 7's states capture (`states/new-activity--teaching-log.png`, row "A self-logged form").

## Symptom

On Log an activity for `kgk_teaching_log`, under the button **Log**, the help line reads "Save draft keeps it in My
activities. It is in nobody's inbox until you submit it." There is no Submit on that form. The same sentence
(`FilingWords.NobodysInboxSentence`) is the "Draft saved." result, the draft's status card body ("Finish the … and submit
it."), and the draft lines of My activities and Needs you (`ActivityListWords`).

## Root cause

`FilingWords.NobodysInboxSentence` and `ActivityPageModel`'s draft body are constants that assume the author's move is
Submit and hands the activity to someone. A type whose move is Log (no hand-off field) needs the move's own label.

## What to build

One wording source keyed on the author's move (`FilingWords.MoveLabel`, `HandOffFieldKey`): with a hand-off, today's
text; without, name the move ("Nothing happens to it until you log it." or similar, checked with the design system's
voice). Use it on every surface listed above so they cannot disagree.

## Verification

- [ ] A teaching-log draft reads the move's word on Log an activity, its page, My activities and Needs you — bUnit tests.
- [ ] A Mini-CEX draft is unchanged — the existing tests stay green.

## Related

T342 (flow 03), E4 (button labels), C2 (the draft result).
