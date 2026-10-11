---
id: T107
title: "Activities pinned to a superseded schema version are permanently uncompletable, and the UI still offers the button"
status: done
priority: P2
created: 2026-09-17
started: 2026-09-24
completed: 2026-09-24
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

## Update 2026-09-24 — EPA-stream survey

**D33 part 1 stands as decided unless the operator overrules it.** `EPA-PROGRAMME.md` § 3D says "the recommendation
stands unless overruled". Suppress the impossible action: render it **disabled, with a reason naming the fields**, not
hidden. Restated after [T105]: offer a transition only if every field that `ValidateForTransition`
(`ActivityService.cs:1105-1124`) would flag against the stored data is writable by this actor in the current state.
That covers the transition's `validation` scope (`all`, `owned` or `draft`) plus its `requires_fields`. The test is
"writable by this actor", not "already filled". The [T122] tool gate and the [T102] nominee gate stay out of this
predicate.

**D33 part 2, the re-pin: recommendation (b), defer.** It is filed as [T171] (P3, pre-launch readiness). Option 3 is
moot: production has no activities ([T157]), and W-006 emptied dev.

---

## As built — 2026-09-24

- **D33 part 1, as decided.** `ActivityService.GetDetailAsync` runs each allowed transition's own validation (T105's
  `all`/`owned`/`draft` scope plus `requires_fields`) against the **stored** data under the **pinned** version. It then
  subtracts the fields this actor may write in the current state. It also drops a field whose `show_if` (its own or
  its section's) reads a field the actor can write, because the actor's move could hide it. If anything is left, the
  action is listed but unavailable (`ActivityActionDto.UnavailableReason`): "Needs A, B and C, which you cannot fill in
  here." When the activity is pinned to an older version it adds "This activity was filed on version N of the form;
  the current version is M." The server's refusal is unchanged, and the tool and nominee gates are not part of the
  predicate.
- `ActivityWorkflowActions` renders the action as a disabled button with no handler. `aria-describedby` points at the
  visible reason in a `.workflow-action-reasons` list, documented in DESIGN.md § Form system. `ActivityView` shows the
  form read-only when no action is available, and refuses a stale request with a message instead of silently. A note
  panel closes when its action stops being available.
- **Verification evidence.**
  - *Told why, instead of a button that throws:* browser on dev, activity 23. It is a trainee-created `msf_cpsa` draft,
    the one real stranded shape in today's data. As the Administrator, **Record** is disabled, its `aria-describedby`
    points at "Record: Needs EPA, MSF campaign, Feedback window closed and Questionnaires returned, which you cannot
    fill in here.", and it has no version sentence (v1 is current). The pre-T070 shape exists on no dev version: every
    `mini_cex_cpsa` version lets the assessor write the ratings. Pinning activity 20 to v1 by a recorded UPDATE, then
    restoring it, correctly left Complete and Decline **enabled**, a negative control. The old-version case is covered
    by `UnavailableActionTests`, which author the old shape.
  - *A new activity is unaffected:* browser, activity 22 (v2). As the assessor, Complete and Decline are enabled with
    no reason. `UnavailableActionTests` also walks `mini_cex_cpsa` from draft through complete.
  - *After the remedy, a stranded activity can be completed or cleanly cancelled:* the re-pin is deferred (D33 part 2,
    [T171]). `cancel` and `decline` stay available on a stranded activity (tested), which is the clean disposal.
- Tests: Application +8, Web +8. The mutants were caught, among them the predicate, the subtraction, the validation
  scope, the rendering, the page guard, and the hide-by-`show_if` filter.
- **Found here for [T162]:** a trainee-created `msf_cpsa` draft is stuck for good. Nobody can write its evidence fields
  (the `draft` state defaults to `subject|creator`, the sections to Coordinator or Administrator), and its workflow has
  no cancel. The dev database holds one: activity 23.

