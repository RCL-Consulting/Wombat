---
id: T196
title: Reactivating an EPA does not re-credit what was filed while it was inactive, and a rebuild while inactive drops earned credit
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-25
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

- [x] Reactivation re-credits the affected completions. Test.
- [x] A rebuild keeps credit earned while the EPA was active. Test.

## Related

T158, T122 (D20, D21).

**Decision, 2026-09-25:** D48 adopted as a default (EPA-PROGRAMME § 3D). Build to it.

---

## As built — 2026-09-25 (`e6d388b`, D48)

- **A pause's history.** `Epa.DeactivatedOn` records when an EPA was deactivated (migration `T196_EpaDeactivatedOn`).
- **Deactivation pauses credit.** A completion credits only if its EPA was in force when it completed
  (`CurriculumItemsInForce`, `Epa.InForceAt`).
- **Rebuilds.** A rebuild uses today's curriculum, and judges only whether an EPA was active as of each completion.
- **Reactivation** re-credits what was completed during the pause (`ResumedEpaCredit`), for any admin who can reactivate,
  and says how many activities now count.
- **The gate** stays an instrument check. The picker offers only EPAs in force, and a stored EPA that is not in force
  reads "(no longer in use)".
- **EpaEdit.** Unticking Active asks first. Deactivate is not shown on an inactive EPA. The focus lands on the status
  region after Deactivate.

Browser on dev (scripted Chrome, master `d6b2796`; `pg_dump` first, at `recovery/pre-g1-migrations.dump`):
- **Save with Active unticked (PAED-006).** Save asks first, and Cancel saves nothing. After confirming, the page reads
  "EPA saved and deactivated." and Deactivate is gone.
- **The Deactivate button (PAED-003).** The focus lands on `.action-result`.
- **Draft 38 (PAED-006, saved while it was active).** The picker no longer offered PAED-006. The draft read "(no longer
  in use)". It was submitted and completed with credit 0, and the PAED-006 target card left My progress.
- **Reactivation.** The page read "EPA reactivated. 1 activity completed against it while it was inactive now counts
  towards progress." Item 7 went to 2 of 2, with keys 10 and 38.
- **instadmin (DEMO-LOC-01).** Deactivated and reactivated.
- **Not run:** `/admin/curriculum-progress`, which is Administrator-only. It is covered by `RebuildCurriculumProgressTests`
  and `EpaActivePeriodPostgresTests`.

**Found:** a plain Save drops the focus to BODY, filed as [T234]. **Filed from the review:** [T230] (races), [T231]
("(no longer in use)" on lists).
