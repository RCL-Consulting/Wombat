# Act 3 play-through — findings (2026-05-29, Opus)

Played from the `after-act-2-replay` snapshot, dev server :5080, Playwright UI driving all
personas. Phases 3.A–3.C + 3.H played end-to-end; 3.D–3.G + 3.I deferred. Canonical finding
record for the session.

> **Correction discipline:** several mid-session notes were artifacts of a **crashed Blazor
> circuit** (the T067/T068 bugs) and have been removed. Everything below was observed on a live
> circuit after both fixes, and cross-checked against the DB.

## Two HIGH blockers found + fixed + committed (branch `fix/T067-activity-builder-addfield-crash`)

### T067 (`2b732cf`) — builder crashes on "Add field"
`/admin/activity-types/{id}` → Form tab → first **Add field** click (or any section
Edit/Up/Down/Delete) throws `ArgumentOutOfRangeException` at `ActivityTypeEdit.razor:511` and
terminates the circuit. Blazor loop-variable capture: `@for (var sectionIndex …)` shares one
variable across all `@onclick` lambdas; deferred click runs with `sectionIndex == Sections.Count`.
Deterministic on a fresh type, zero edits. **Blocked building any multi-field schema** — the Act 3
prerequisite. Fix: per-iteration `capturedSectionIndex`/`capturedFieldIndex`. Verified.

### T068 (`6281eae`) — no trainee can create any activity
Trainee → `/activities/new` → select a published type → circuit crash
(`GetActivityTypeEditorQuery` throws "could not be found" at `NewActivity.razor:70`). The trainee
create page reuses the **admin builder** query whose `CanReadAsync` only allowed Administrator /
scope-matched InstitutionalAdmin. **No trainee could create any activity** — the whole Act 3 loop
was blocked. Fix: any authenticated user may read a *published, active* activity type's schema
(form template; drafts/inactive stay admin-only). `CreateDraftAsync` already gates on "published".
Verified.

Neither was catchable by earlier replays — they only used the default single-`title` schema.

## Pre-Act-3 schema build (admin, post-T067)

Mini-CEX (`mini_cex_paed`, id 11) — full schema built via the visual builder, **published v2**:
3 sections (Encounter details / Clinical performance ratings / Feedback), 12 fields (epa_id EPA,
assessor_user_id User, setting Choice[5], patient_age_months Number, presenting_complaint LongText;
6 Scale ratings; narrative LongText). Workflow JSON (draft→submitted→rated→completed + recall;
accept/complete actor `field:assessor_user_id`) and Credit JSON (counts_for epa_field=epa_id,
minimum_level_field=overall_level, amount 1) both saved + verified byte-for-byte on reload.
Snapshots: `act3-schema-built`.

- **F-A3-doc-1 (doc, LOW):** Scenario Step 1.11.b says "all 13 fields" — the spec lists 12 (5+6+1).
- **F-A3-build-1 (cosmetic, LOW):** Workflow + Credit tabs have no "Format JSON" button the
  scenario (1.11.c/d) references; the textarea accepts pasted JSON and Save normalises it.

## Phase 3.A — Dr Dlamini submits a Mini-CEX ✓

- **3.1 ✓** Trainee `/activities/new` type picker shows exactly the 10 Paed types (Demo IM types
  correctly scoped out). Selecting Mini-CEX renders the full 3-section / 12-field schema form,
  no crash (post-T068).
- **3.2 ✓** Filled all 12 fields; Save draft → "Draft created.", activity 1 persisted (Draft,
  subject = Dlamini). Data round-trips in the detail view.
- **3.3 ✓** Submit → state draft→**submitted**; button set becomes **[Recall]** only. (In Draft the
  detail page shows only **Submit** — the workflow widget filters transitions by from-state
  correctly. The earlier "Recall shows in Draft" note was a crashed-circuit artifact and is wrong.)
- **F-A3-3 (widget gap, HIGH → T069):** the runtime `ActivityForm` renderer has **no dedicated
  controls for EPA / User / Scale field types** — all fall through to raw inputs (EPA → number,
  User → text, Scale → number; only Choice is a real `<select>`). So a trainee must type a raw EPA
  **id** and a raw assessor **GUID** by hand. The schema-driven forms aren't usable by real
  trainees until this lands. (`ActivityForm.razor` switch groups Number/Epa/Likert/Scale/Rating →
  number input, Text/User/Signature/ProcedureRef → text input.) Task file `T069`.
  - *Because there's no EPA picker, the earlier "Demo EPAs leak into the picker" note can't apply —
    removed. A future picker's EPA lookup must be speciality-scoped (T069 verification note).*

## Phase 3.B — Dr Naidoo rates and completes ✓ (+ actor-DSL verified)

- **Actor-DSL field binding ✓ (key engine test):** As **Patel** (an Assessor but NOT the named
  `assessor_user_id`), `/activities/1` (submitted) renders with **zero action buttons** — no Accept.
  As **Naidoo** (the named assessor) the same page shows **[Accept]**. `field:assessor_user_id`
  actor resolution works both directions.
- **3.4 ✓** Naidoo → Accept → state submitted→**rated**; button becomes **[Complete]**.
- **F-A3-4 (functional gap, MEDIUM → T070):** in **Rated** state the detail page shows the data as
  **read-only** (DOM: 0 editable, 12 disabled fields) with **no assessor-note field**. Scenario
  Step 3.5 ("assessor adjusts Communication 4→3, adds a note, then Completes") **cannot be
  performed** — the assessor can only Accept→Complete, not modify ratings or attach a note.
  Task file `T070`.
- **3.5 ✓ (completion)** Complete → state rated→**completed** (terminal, no buttons), no crash. All
  four transitions persisted: create / submit / accept / complete.

## Phase 3.C — Credit verification — **IMPORTANT FINDING**

DB after Complete (activity 1 = completed, subject Dlamini, overall_level "3"):
**`CurriculumItemProgresses` has ZERO rows — no credit was written.**

This is **correct per the implementation but contradicts the scenario.** `CreditApplier` ran on the
terminal transition (confirmed: it's invoked from `ActivityService.TransitionAsync` when
`targetState.Terminal`). For PAED-001, `CurriculumItem.MinimumLevelOrder = 4`. The credit directive
has `minimum_level_field = overall_level`, and the activity's `overall_level = 3`. In
`CreditApplier.ApplyAsync`:

```
var minimumLevelReached = MeetsMinimumLevel(...);   // 3 >= 4 → false
if (!minimumLevelReached && !string.IsNullOrWhiteSpace(directive.MinimumLevelField))
    continue;                                        // skips the row entirely
...
progress.CountsSoFar += amount;                      // never reached
```

So when a `minimum_level_field` is present and the level is short, **nothing is credited at all** —
not even the volume count. Consequences:

- **F-A3-5 (credit semantics, HIGH → T071 candidate):** the scenario (Step 3.6) expected
  "PAED-001 … 1 of 30" with the level *below* the stage target — i.e. it assumes **volume
  (CountsSoFar) accrues regardless of level, while level-attainment (MinimumLevelReachedCount) is
  tracked separately.** The implementation instead makes `minimum_level_field` an **all-or-nothing
  gate**: below-min completions don't count toward the requirement at all. Also note that, given
  the `continue`, `MinimumLevelReachedCount` can never differ from `CountsSoFar` — the two-counter
  design is effectively dead. **Decision needed (domain):** (a) count volume always + gate only the
  level-reached counter [matches scenario + makes the second counter meaningful], or (b) keep
  gating and fix the scenario expectation. This is a real EPA-semantics call — flagged, not
  unilaterally "fixed".
- **F-A3-doc-2:** Step 3.6's narrative is internally consistent with option (a) but not with the
  current code. (Seeded PAED-001 `MinimumLevelOrder=4`, so level-3 is genuinely one short — the
  earlier note claiming the seed was 3 was wrong.)

### Credit WRITE path proven (level-4 confirmation) ✓
A second Mini-CEX (activity 2) was driven through the full lifecycle by Dlamini + Naidoo with
`overall_level = 4` (meets PAED-001 min 4). After Complete, `CurriculumItemProgresses` has exactly
one row: **item=2 (PAED-001), trainee=Dlamini, CountsSoFar=1, MinimumLevelReachedCount=1,
LastActivityId=2, CreditedActivityKeysJson=["2:complete"]**. Dlamini's `/portfolio/progress` shows
**"PAED-001 … 1 of 30"**. So:
- Level **≥** min → credit row written (counts + minReached both 1). ✓ headline Act 3 goal MET.
- Level **<** min (activity 1, level 3) → **no row at all** → confirms F-A3-5 (gate, not counter).
- The two counters are identical in every reachable case → confirms the two-counter model is dead.

## Phase 3.H — Audit log ✓
As bootstrap admin, `/admin/audit` renders 20 rows, **no error banner, no JsonException** (T045
`[PRINCIPAL]` substitution holds through the new lifecycle writes); Activity events present
alongside the schema-build and Act 2 events.

## Deferred to a fresh session
3.D procedure-log stage-minimum gating, 3.E DOPS (in-training assessor), 3.F MSF (time-based
workflow + notifications), 3.G stalled-assessment triage, 3.I full dashboard sweep. Each of
3.D–3.F needs that type's full schema built via the (now-fixed) builder first.

## Follow-up tasks raised
- **T067** ✅ shipped (builder Add-field crash).
- **T068** ✅ shipped (trainee activity-create read scope).
- **T069** (open, HIGH) — `ActivityForm` runtime pickers for EPA / User / Scale.
- **T070** (open, MEDIUM) — assessor rating-edit + note surface in Rated state.
- **T071** (open, HIGH — domain decision) — credit `minimum_level_field` semantics: volume-count
  vs all-or-nothing gate (the two-counter model is currently dead).
- Doc fixes: 1.11.b "13"→12 fields; 1.11.c/d "Format JSON" button; Step 3.6 reconcile with T071.
