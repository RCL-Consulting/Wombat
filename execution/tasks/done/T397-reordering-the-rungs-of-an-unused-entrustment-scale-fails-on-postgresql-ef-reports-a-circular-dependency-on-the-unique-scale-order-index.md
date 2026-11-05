---
id: T397
title: Reordering the rungs of an unused entrustment scale fails on PostgreSQL: EF reports a circular dependency on the unique (scale, order) index
status: done
priority: P2
owner: agent
depends_on: []
created: 2026-11-05
promoted: 2026-11-05
started: 2026-11-05
completed: 2026-11-05
---

# T397 — Reordering the rungs of an unused entrustment scale fails on PostgreSQL

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Flow 16's rung editor offers Up and Down only on a scale nothing uses (Q16, T395), and every such
move failed on the real database, so the feature could not work; nothing is live, and no runbook step reorders.
**Surfaced:** 2026-11-05, by T394's build fix pass, writing the PostgreSQL tests the build review's S-8 asked for. The
operator said "Fix T397 now".

## Why

Found 2026-11-05 by flow 16's fix pass (T394, adding the PostgreSQL tests the build review's S-8 asked for). A save
that swaps two rungs' `Order` on a scale nothing uses (the only case flow 16's Up and Down are offered, Q16/T395) makes
EF Core order the two updates against the unique (scale, order) index and give up with a circular dependency; the
in-memory provider has no such index, so every unit test passes. Reported as already on master (inferred from the fix
pass's report; needs confirmation by a PostgreSQL test on master). It blocks flow 16's rung editor in real use; no runbook
step reorders a rung, so the replay would not show it. The usual fix: write the moved rungs in two steps (to free
orders, then to their places) in one transaction, or make the index deferrable.

## Symptom

Saving a scale whose rungs change places (Low at 1 and High at 3 swapped) throws `InvalidOperationException: Unable to
save changes because a circular dependency was detected in the data to be saved: 'EntrustmentLevel [Modified] <- …'`
before anything reaches the database. `CatalogueReadsPostgresTests.ASwapOfTwoRungs_OnAScaleNothingUses_IsSaved_OnPostgres`
shows it on the handler as it was.

## Root cause

`EntrustmentLevels` has a unique index on (ScaleId, Order). EF Core sorts a save's updates so that no single statement
breaks a unique index; a swap has no such order, so EF gives up. The in-memory provider holds no index, so every unit test
passed. A chain (a removal freeing an order) has an order and saved; only a cycle failed.

## What to build

Built on `t394` (flow 16's branch, before its squash), as the operator asked:
- `UpdateEntrustmentScaleCommandHandler`: a save that moves a stored rung first parks each moved rung at the negative of
  its new order and deletes the removed rungs, saves, then writes the rest; both saves in one ambient `TransactionScope`,
  so a refusal of the second undoes the first.
- A refusal inside that transaction aborts it, so the name race (`EntrustmentScaleNameRace`, code N-7) can no longer read
  the database to learn which index refused it: it now reads the violated index's name from the provider exception's
  `Data["ConstraintName"]` (`PostgresErrors.ConstraintName`, naming no provider type) and reads back only when no name
  is given.

## Verification

- [x] A swap on a scale nothing uses saves on PostgreSQL — `ASwapOfTwoRungs_OnAScaleNothingUses_IsSaved_OnPostgres`
  fails on the old handler (circular dependency) and passes on the fix.
- [x] A swap whose second write is refused leaves no rung moved and the name unchanged —
  `ASwapWhoseNewNameARacingSaveTakes_IsRefusedUnderName_AndUndoesTheParkedRungs` fails on the old handler and passes.
- [x] The existing name race still refuses under Name — `AScalesNameTakenByARacingSave_IsRefusedUnderName_InTheCheckWords`.
- [x] The six suites green on `t394` with the fix — 836/4411/1035/52/4675/530, build 0 warnings; the fix is `5663b192`
  (on `t394`, reaching master with flow 16's squash).

## Related

T394 (flow 16), T395 (the reorder guard), T396 (the guards' race); `EntrustmentLevelConfiguration`.

## Notes

- Observed: Npgsql fills `Data["ConstraintName"]` (the rename test passes only because the race no longer reads inside
  the aborted transaction).
- Inferred, not checked: the defect is also on master (the editor's Up and Down predate flow 16); it ships fixed with
  flow 16's squash.
- 2026-11-05: the PostgreSQL rename test also showed that a refusal inside the transaction aborts it, so the name race's
  read-back failed with 25P02; fixed in the same commit (the index name read from the exception). The Web suite's
  `SpecialityPageTests` "no 99" check failed under load from bUnit's `blazor:elementReference` guids; it strips every
  guid now.
