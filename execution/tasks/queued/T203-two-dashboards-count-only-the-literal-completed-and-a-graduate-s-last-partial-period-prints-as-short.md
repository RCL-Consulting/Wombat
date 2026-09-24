---
id: T203
title: Two dashboards count only the literal 'completed', and a graduate's last partial period prints as 'short'
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# <id> — <one line that states the defect or the goal, not the solution>

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low.
**Surfaced:** 2026-09-24, the T169 review.

## Items

1. `GetAssessorDashboardSummaryQuery.cs:74` and `GetTraineeDashboardSummaryQuery.cs:74` count activities in the literal
   state `completed`, so reflective exercises (`discussed`), MSF (`recorded`) and logs (`logged`) never count. Use the
   pinned workflow's terminal states, as T169 and D44 do.
2. Once ended, a period prints "short" even for a graduate's last partial period, and for a deactivated trainee's periods
   after an end date Wombat does not record. D14 has a rule for late starts but none for partial ends. This needs a
   decision; it sits with College question 4 (D42).

## Verification

- [ ] The dashboards count terminal states. Tests.
- [ ] The partial-end rule is decided and recorded, then applied.

## Related

T169, T130, D14, D42.
