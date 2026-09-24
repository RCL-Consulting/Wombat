---
id: T107
title: "Activities pinned to a superseded schema version are permanently uncompletable, and the UI still offers the button"
status: queued
priority: P2
created: 2026-09-17
---
# T107 — Activities pinned to a superseded schema version are permanently uncompletable, and the UI still offers the button

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Status:** open
**Surfaced:** 2026-09-17, adversarial review of T103.
**Severity:** Medium — a small number of real activities on dev and production are stuck, and the product
offers their assessor an action that always fails.

## Symptom

A CPSA Mini-CEX created before T103 ran sits in `requested`, pinned to schema version 1. Its assessor opens
it and sees a **Complete** button. Pressing it produces
`overall_level: A value is required; strengths: A value is required; …` — the exact symptom T070 was filed
to fix, on exactly the activities that motivated fixing it.

## Why

Pinning is correct and deliberate: `ActivityService.GetPinnedVersion` resolves the `ActivityTypeVersion` an
activity was created against, so a schema change cannot retroactively invalidate work in progress. T103's
republish therefore reaches **new** activities only — its own task file says so.

The consequences that were not thought through:

1. **There is no re-pin path anywhere in the codebase.** `Activity.SchemaVersion` is assigned in exactly
   one place, `ActivityService.cs:58`, at creation. Nothing can move an activity onto a newer version, so
   a stranded activity stays stranded for life. Its only dispositions are `cancel` and `decline`.
2. **The action list does not know.** `ActivityWorkflowActions` renders whatever `WorkflowEvaluator` allows,
   and the evaluator only checks the actor rule — it has no view of whether the transition's
   `requires_fields` are satisfiable by anyone in the current UI. So the button is offered and cannot work.

## Options

1. **Suppress the impossible action.** When the actor's writable set is empty but the transition declares
   `requires_fields` that the stored data does not satisfy, do not offer it — explain instead. Cheapest,
   honest, fixes the user-visible half only.
2. **Allow a re-pin.** An explicit, audited "move to the current version" for an activity whose pinned
   version is superseded, guarded by re-validating the stored `DataJson` against the new schema. This is
   the real fix, and it needs care: re-pinning is exactly the operation that could invalidate in-flight
   work, which is why pinning exists.
3. **Migrate the handful that exist.** A one-off for dev and production, plus option 1 so it cannot recur.

Recommendation: **1 now** (it is small and stops the product lying), then decide 2 vs 3 with a count of how
many activities are actually affected in production.

## Verification

- An assessor on a stranded activity is told why they cannot complete it, rather than being offered a
  button that throws.
- After whichever remedy: a previously stranded CPSA Mini-CEX can be completed, or is cleanly cancelled,
  with an audit trail either way.
- A newly created activity is unaffected — it pins to the current version and completes normally.

## Related

Consequence of T103's pinning semantics; the stranded case is the one T070 does **not** reach. Related to
T106 item 9 (the trajectory does not filter by workflow state).
