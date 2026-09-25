---
id: T194
title: Committee commands reveal which reviews and panels exist, and a panel's speciality is never checked against the trainee
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
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

- [x] Unknown and out-of-scope review and panel ids get the same refusal, before any state check. Handler tests.
- [x] A Speciality panel refuses a trainee of another speciality. Handler test.
- [x] A SpecialityAdmin is not offered the Institution scope. bUnit.

## Related

T182, T183, T113, T131.

---

## As built — 2026-09-25 (`225763e`)

- **Authorisation first.** Every committee command authorises before anything else, with one refusal for an unknown
  and an out-of-scope id.
- **No trainee on a panel.** `WorksOnPanel` and `HoldsSeat` refuse anyone who holds Trainee, before any other check.
  This closes a trainee resolving their own appeal from an External seat.
- **The panel form** offers each admin only the scopes they may create.
- **One filter** now drives the scheduling panel list and the trainee picker (`OfferableNamesAsync`), so an erased
  trainee's panel is not offered.
- **An InstitutionalAdmin seated on another institution's panel** may read what they may start.

Handler tests cover each command. bUnit covers the panel form.

Browser on dev (scripted Chrome, master `225763e`):
- **Unknown and out-of-scope ids.** Review 99999, and review 1 as an assessor made CommitteeMember, got the same
  sentence: "The committee review could not be found among the reviews you can view."
- **The Speciality scope.** The coordinator made SpecialityAdmin was offered only the Speciality scope, and created panel 3.
- **A trainee seated as External** lodged appeal #1 and got no resolve form; the chair did.
- **Not run:** the steps that need `coordinator.t113b`, which has no seeded credential. They are covered by handler
  tests.

**Filed:** [T237] (a trainee can still be seated), [T238] (erased profiles), and [T245] (a speciality admin's panel
form offers unadopted specialities).
