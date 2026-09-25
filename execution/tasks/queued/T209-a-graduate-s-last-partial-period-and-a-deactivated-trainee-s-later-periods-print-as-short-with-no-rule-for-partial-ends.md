---
id: T209
title: A graduate's last partial period, and a deactivated trainee's later periods, print as 'short' with no rule for partial ends
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
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

- [ ] The rule is recorded in EPA-PROGRAMME § 3, then applied, with tests.

## Related

T169, T130, D14, D42, § 3F question 4.

**Decision, 2026-09-25:** D49 adopted as a default (EPA-PROGRAMME § 3D). Build to it.
