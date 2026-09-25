---
id: T196
title: Reactivating an EPA does not re-credit what was filed while it was inactive, and a rebuild while inactive drops earned credit
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
---

# T196 — Reactivating an EPA does not re-credit what was filed while it was inactive, and a rebuild while inactive drops earned credit

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low while EPAs are rarely deactivated. The outcome is still wrong: credit depends on when a rebuild ran.
**Surfaced:** 2026-09-24, the T158 review (out-of-scope items 1, 3, 4 and 6).

## Symptom

- Completions stamped 0 while an EPA was inactive are not credited when it is reactivated. Only an Administrator
  rebuild does that, and the College and Institutional admins who can edit an EPA cannot run one.
- A rebuild while an EPA is inactive drops credit earned while it was active. Fixing that needs `Epa.DeactivatedOn`,
  which is a migration.
- The tool gate does not refuse an EPA that is not in force at the author's hand-on, and a stored option that is not in
  force is not labelled "(no longer in use)". This would turn the gate into a curriculum-membership check, which D20
  and D21 have not decided; it needs a decision.
- EpaEdit: unticking Active and pressing Save deactivates without a confirmation, and Deactivate still shows on an
  inactive EPA.

## What to build

Decide the hand-on question first. Then add `DeactivatedOn`, make the rebuild time-aware, and re-credit on
reactivation, for an admin who can reactivate. Fix the two EpaEdit gaps.

## Verification

- [ ] Reactivation re-credits the affected completions. Test.
- [ ] A rebuild keeps credit earned while the EPA was active. Test.

## Related

T158, T122 (D20, D21).

**Decision, 2026-09-25:** D48 adopted as a default (EPA-PROGRAMME § 3D). Build to it.
