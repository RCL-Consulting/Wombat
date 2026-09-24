---
id: T194
title: Committee commands reveal which reviews and panels exist, and a panel's speciality is never checked against the trainee
status: queued
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
---

# T194 — Committee commands reveal which reviews and panels exist, and a panel's speciality is never checked against the trainee

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. These are the committee-side remainders of the T113/T182 scope work.
**Surfaced:** 2026-09-24, the T182 review (findings 6 and 8 and a minor, confirmed in code, out of scope).

## Symptom

1. **Existence leak.** An unknown review id gets "could not be found", while an out-of-scope one gets an
   `UnauthorizedAccessException`. `UpdateDecisionPanel` does the same for panels. Stage, Remove and Issue check the
   formative flag and the review state before the chair check (`StagePendingEntrustmentDecision.cs:72,77`,
   `RemovePendingEntrustmentDecision.cs:45`, `IssueEntrustmentDecision.cs:72`). Issue compares the trainee id before
   authorising (`:77`).
2. **Speciality.** A Paediatrics panel accepts a Surgery trainee at the same institution. `MayAdministerPanel` does not
   check which speciality a SpecialityAdmin's or SubSpecialityAdmin's panel covers, and the panel form offers those
   admins the Institution scope, which is then refused.
3. **Minor.** An External member from another institution sees that panel in the scheduling page's panel list, with an
   empty trainee list.

## What to build

Authorise first, and give one refusal for an unknown id and an out-of-scope one. Check the panel's speciality scope
against the trainee's profile when scheduling. Offer each admin only the scopes they may create. Keep an External
member's panel off the scheduling list unless they may schedule on it.

## Verification

- [ ] Unknown and out-of-scope review and panel ids get the same refusal, before any state check. Handler tests.
- [ ] A Speciality panel refuses a trainee of another speciality. Handler test.
- [ ] A SpecialityAdmin is not offered the Institution scope. bUnit.

## Related

T182, T183, T113, T131.
