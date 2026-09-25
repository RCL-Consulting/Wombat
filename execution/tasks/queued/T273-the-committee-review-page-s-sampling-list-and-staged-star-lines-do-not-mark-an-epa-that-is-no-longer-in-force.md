---
id: T273
title: The committee review page's sampling list and staged STAR lines do not mark an EPA that is no longer in force
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
---

# T273 — The committee review page's sampling list and staged STAR lines do not mark an EPA that is no longer in force

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-25, the T255 review.

## Symptom and what to build

- **The sampling concentration list** (`ReviewDetail`). `EpaSamplingConcentrationDto` has no in-force flag. Add
  `EpaInForce` from the EPA rows the query already loads, and render through `EpaLabel`.
- **Staged STAR lines.** `PendingEntrustmentDecisionDto` has no flag. The line already says "No longer fits"; the mark
  makes it consistent.
- **Evidence snapshot headings** are frozen when the review starts, and the mark means "no longer in use now".
  **Decision (adopted 2026-09-25):** a frozen record is not re-marked. The snapshot shows what was true when it was
  taken. The live parts of the page carry the mark.

## Verification

- [ ] The sampling list and staged lines mark a deactivated EPA. bUnit.

## Related

T255, T231, D48.
