---
id: T347
title: Flow 03's built words differ from the boards in a few places (type nouns, locked-section sentences, the greyed move's reason, picker counts, per-type date labels, help lines)
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-29
---

# T347 — Flow 03's built words differ from the boards in a few places (type nouns, locked-section sentences, the greyed move's reason, picker counts, per-type date labels, help lines)

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low: wording; the build's words are coherent and the runbook quotes them.
**Surfaced:** 2026-09-29, flow 03's build review (T342, `design/flows/03-trainee-files-activity/build-review.md`), filed under the agreed stopping line.

## Symptom

Flow 03's built pages differ from the accepted boards in a few words: About's Credit and the done body build a noun from the type name ("a portfolio and logbook review credits nothing" where the board says "a portfolio review"); a locked section's body is "Not filled in yet."/"Not filled in." where the boards have pre-filing sentences ("After you submit, David Naidoo rates the encounter on this ladder."), closed reasons ("Not filled in: the request was declined.") and "It holds: …"; the greyed move's reason ("which you cannot fill in here" against "which only the assessor fills in"); the picker's "7 types" against "(7)"; About's date row always "Encounter" where the boards say "Case or incident", "Delivered", "Period"; the boards' help lines under the EPA and assessor pickers.

## Root cause

The build generalised the boards' type-specific words (T342; build review D3, D4, D8, D9).

## What to build

Decide per item: take the board's words (a short noun per type, the locked sentences from the section's fields and state, the date label from the date field's label) or keep the build's and record it. Update the runbook Expects that quote them.

## Verification

- [ ] Each item decided and, where changed, pinned by a test and quoted by the runbook — checked by the Web suite and `check_verbatim_steps.py`

## Related

T342 (flow 03), `design/flows/03-trainee-files-activity/build-review.md`.

## Notes

Observed by the T342 build review (code read at `t342`, 2026-09-29); not yet replayed.
