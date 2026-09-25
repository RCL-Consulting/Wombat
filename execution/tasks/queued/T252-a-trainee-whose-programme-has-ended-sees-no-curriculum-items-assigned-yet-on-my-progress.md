---
id: T252
title: A trainee whose programme has ended sees "No curriculum items assigned yet" on My progress
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T252 — A trainee whose programme has ended sees "No curriculum items assigned yet" on My progress

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low to Medium. A completed or withdrawn trainee loses sight of their record. D49's end rule shows only in
the portfolio PDF.
**Surfaced:** 2026-09-25, the G2 browser check (T209). The implementer disclosed it.

## Symptom

After the dev trainee was deactivated with a last day of 2026-08-20, `/portfolio/progress` read "No curriculum items
assigned yet. Once you are admitted…", because the page reads only active profiles. The PDF showed the periods correctly.

## What to build

Show an ended trainee their progress read-only: the profile they ended on, with D49's end marker ("no target: the
programme ended part-way through"). Say at the top that the programme ended, and on what day. Decide whether a
completed trainee who has lost the Trainee role still reaches the page. **Recommendation:** yes, read-only, as the PDF
does.

## Verification

- [ ] An ended trainee's My progress shows their periods with D49's wording. bUnit and a handler test.

## Related

T209, D49, T166.
