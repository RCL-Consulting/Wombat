---
id: T230
title: A completion racing an EPA's deactivation or reactivation can keep or miss credit until the next rebuild
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-25
completed: 2026-09-25
---

# T230 — A completion racing an EPA's deactivation or reactivation can keep or miss credit until the next rebuild

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. The window is one request wide, and a rebuild corrects it.
**Surfaced:** 2026-09-25, the T196 review (finding 3). Documented in the `ResumedEpaCredit` remarks and CUSTOMIZATION.md,
not fixed.

## Symptom

- A completion that reads the EPA as inactive, and saves after a reactivation has read its candidates, stays uncredited
  until an Administrator rebuild.
- A completion that read the EPA as active just before a deactivation committed, but whose moment falls after
  `DeactivatedOn`, keeps live credit. A rebuild while the EPA is inactive removes it; reactivation restores it.

## What to build

Make the completion's save and the EPA's deactivation or reactivation conflict: for example, the credit path touches a
concurrency token the EPA's save also touches, or the credit reads the EPA under a lock. Then re-run the loser.

## Verification

- [x] Each race, driven from two contexts in a Postgres test, ends with the credit a rebuild would write.

## Related

T196, D48, T158.

---

## As built — 2026-09-25 (`6267a19`)

**One lock** (`EpaCreditLock`):
- An EPA change takes `FOR NO KEY UPDATE` on the EPA row, which still lets foreign-key checks through.
- A completion's credit takes `FOR SHARE` on each EPA it counts towards, and reads the clock after the lock.
- An architecture test fails if anything calls `Epa.Deactivate` or `Epa.Reactivate` without the lock.

**A wait past 30 seconds** (Npgsql's default) is refused with a readable message, and nothing is saved.

Postgres race tests drive each race from two contexts. Each ends with what a rebuild would write.

Browser on dev (scripted Chrome, master `dea2997`; `pg_dump` first, at `recovery/pre-g4-migration.dump`): a `psql` session held the locks.
- **A completion waits** (activity 44). It waited 14.5 s for a held `FOR NO KEY UPDATE`, then completed and credited
  when the lock was released.
- **A completion past 30 s** (activity 45) was refused: "An EPA this activity counts towards is being changed right now,
  so nothing was saved. Try again in a moment." It stayed Requested. The retry completed.
- **A deactivation waits.** It waited 15.9 s for a held `FOR SHARE`, and `DeactivatedOn` was read after the release.
  Activity 46, completed while PAED-001 was paused, credited nothing until the reactivation, which counted it.
- **A deactivation past 30 s** was refused: "Activities are still being completed against this EPA…". The EPA stayed
  active.

Every lock was rolled back, and no session is left in a transaction.
