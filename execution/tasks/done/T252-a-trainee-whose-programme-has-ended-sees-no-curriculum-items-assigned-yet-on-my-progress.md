---
id: T252
title: A trainee whose programme has ended sees "No curriculum items assigned yet" on My progress
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
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

- [x] An ended trainee's My progress shows their periods with D49's wording. bUnit and a handler test.

## Related

T209, D49, T166.

---

## As built — 2026-09-25 (`e91a8a8`)

A trainee whose programme has ended sees My progress read-only, for the profile they ended on:
- a notice at the top saying the programme ended, and when;
- a "Your programme" card;
- periods newest first, with D49's marker;
- each card's MSF line and period failures, and no progress bars.

Home says the same. Handler and bUnit tests.

Browser on dev (scripted Chrome, master `f19417d`; `pg_dump -n public` first, at `recovery/pre-t258-migration.dump`): instadmin deactivated the dev trainee with a last day of 2026-08-20.
- **My progress** read "Your programme ended on 20 August 2026. This page is your record of it and is read-only…". The
  periods read "Semester 2, 2026 · no target (your programme ended part-way through) · 10 recorded" and "Semester 1,
  2026 · 2 of 3, 1 short". Every card had an MSF line, and there were 0 progress bars.
- **Home** read "…no target applies to you any more…".
- **With the end day nulled by SQL**, the page read "Wombat did not record the day it ended…".
- **Restored by SQL:** the page is byte-identical to the baseline.

**Found:** encounters after the last day still count. Filed as [T281], with a default adopted.
