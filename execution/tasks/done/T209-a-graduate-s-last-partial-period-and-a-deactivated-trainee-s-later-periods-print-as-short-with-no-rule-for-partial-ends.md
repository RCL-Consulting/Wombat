---
id: T209
title: A graduate's last partial period, and a deactivated trainee's later periods, print as 'short' with no rule for partial ends
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
---

# T209 — A graduate's last partial period, and a deactivated trainee's later periods, print as 'short' with no rule for partial ends

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low until a trainee finishes. It is a policy gap: D14 covers late starts but not early ends.
**Surfaced:** 2026-09-24, the T169 review; split from T203 item 2.

## What to decide, then build

Whether a period that a programme's end (completion or deactivation) cuts short is held to its target, pro-rated, or
exempt. It sits with College question 4 (D42, late starters and part-way finishers). Recommendation: exempt a period
the programme ends in before its last month, mirroring D42's provisional start rule. Then apply it in
`QuotaProgressCalculator`, the progress page and T169's PDF section.

## Verification

- [x] The rule is recorded in EPA-PROGRAMME § 3, then applied, with tests.

## Related

T169, T130, D14, D42, § 3F question 4.

**Decision, 2026-09-25:** D49 adopted as a default (EPA-PROGRAMME § 3D). Build to it.

---

## As built — 2026-09-25 (`55d211f`, D49)

- **The end.** `TraineeProfile.DeactivatedOn` (migration `T209_TraineeProfileDeactivatedOn`) records a withdrawn
  trainee's last day, as `CompletedOn` does for a completion. The last day is never after today, and never before the
  start. Both end actions confirm first.
- **The rule (D49).** A period the programme ends in before its last month is exempt: "no target (the programme ended
  part-way through)". A period that ends in its last month keeps its target. Periods after the end are not listed.
- **Where it applies.** `QuotaProgressCalculator` applies the rule, and the progress page and T169's per-EPA PDF section
  print it.

Tests cover each boundary.

Browser on dev (scripted Chrome, master `ec58d2e`; `pg_dump` first, at `recovery/pre-g2-migrations.dump`): instadmin deactivated the dev trainee with a last day of 2026-08-20.
- A future date and a date before the start were refused, and a start after the end was refused.
- **The PDF:** every semester EPA read "Semester 2, 2026: no target (the programme ended part-way through)",
  semester 1 was unchanged, and nothing after S2 was listed.
- **Undone by SQL**, because no UI reactivates a profile.

**Found:** an ended trainee's My progress reads "No curriculum items assigned yet", so D49 shows only in the PDF. Filed
as [T252].
