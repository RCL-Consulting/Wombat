---
id: T363
title: The committee review page leaves gaps where it has no data: the graduation summary, a line's earlier STAR, a decided line's level and expiry, and who may start when the chair cannot
status: proposed
priority:
owner: agent
depends_on: []
created: 2026-10-12
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The page works; four things the boards drew have no data behind them, so the build left them out.
**Surfaced:** 2026-10-12, T361's build (flow 07); confirmed by the replay (`design/flows/07-review-sitting/build-review.md`).

## Why

Flow 07's round 3 drew four things the page cannot show yet, because no query returns them. The integrator held them
for a proposal after the replay, rather than add reads during the build.

## Symptom

On `/committee/reviews/{id}`:
- a ratified pre-graduation review shows the decision card, but no summary of what the graduation issued;
- a chair's line (one added from the foot form) does not show the trainee's earlier STAR on that EPA;
- a decided line does not show the level and expiry that were decided;
- when the chair cannot act, the bar says why but not who else may start the review.

## Root cause

Not yet known in detail: each needs a field the review's queries (`GetCommitteeReviewByIdQuery`, the agenda read) do
not return.

## What to build

Decide each with the operator (some may be dropped), then add the read and the words.

## Verification

- [ ] Each kept item is shown on the page, by bUnit and a browser check on the replay's data.

## Related

T361 (flow 07), `design/flows/07-review-sitting/round-3-check.md`, T362 (live following).

## Notes
