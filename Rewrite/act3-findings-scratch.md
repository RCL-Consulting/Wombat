# Act 3 play-through — findings (2026-05-29, Opus)

Played from the `after-act-2-replay` snapshot, dev server :5080, Playwright UI driving all
personas. Phases 3.A–3.C + 3.H played end-to-end; 3.D–3.G + 3.I deferred. This is the canonical
finding record for the session; everything below is cross-checked against the DB.

> **Accuracy note:** several mid-session notes were artifacts of a **crashed Blazor circuit** (the
> T067/T068 bugs) or of UI steps that silently no-op'd (a wrong `<select>` value, a stale element
> ref). They have been removed. Every claim below was re-verified on a live circuit AND against the
> Postgres DB at the end of the session.

## Two HIGH blockers found + fixed + committed (branch `fix/T067-activity-builder-addfield-crash`)

### T067 (`2b732cf`) — builder crashes on "Add field"
`/admin/activity-types/{id}` → Form tab → first **Add field** click (or any section
Edit/Up/Down/Delete) throws `ArgumentOutOfRangeException` at `ActivityTypeEdit.razor:511` and
terminates the circuit. Blazor loop-variable capture: `@for (var sectionIndex …)` shares one
variable across all `@onclick` lambdas; the deferred click runs with `sectionIndex ==
Sections.Count`. Deterministic on a fresh type, zero edits. **Blocked building any multi-field
schema** — the Act 3 prerequisite. Fix: per-iteration `capturedSectionIndex`/`capturedFieldIndex`.
Verified (Add field now appends a field with no crash).

### T068 (`6281eae`) — no trainee can create any activity
Trainee → `/activities/new` → select a published type → circuit crash
(`GetActivityTypeEditorQuery` throws "could not be found" at `NewActivity.razor:70`). The trainee
create page reuses the **admin builder** query whose `CanReadAsync` only allowed Administrator /
scope-matched InstitutionalAdmin. **No trainee could create any activity** — the whole Act 3 loop
was blocked. Fix: any authenticated user may read a *published, active* activity type's schema
(form template; drafts/inactive stay admin-only). `CreateDraftAsync` already gates on "published".
Verified (form renders + draft creates).

Neither was catchable by earlier replays — they only used the default single-`title` schema.

## Pre-Act-3 schema build (admin, post-T067)

Mini-CEX (`mini_cex_paed`, id 11) — full schema built via the visual builder, **published v2**:
3 sections (Encounter details / Clinical performance ratings / Feedback), 12 fields (epa_id EPA,
assessor_user_id User, setting Choice[5], patient_age_months Number, presenting_complaint LongText;
6 Scale ratings; narrative LongText). Workflow JSON (draft→submitted→rated→completed + recall;
accept/complete actor `field:assessor_user_id`) and Credit JSON (counts_for epa_field=epa_id,
minimum_level_field=overall_level, amount 1) saved + verified byte-for-byte on reload. Snapshots:
`act3-schema-built`.

- **F-A3-doc-1 (doc, LOW):** Step 1.11.b says "all 13 fields" — the spec lists 12 (5+6+1).
- **F-A3-build-1 (cosmetic, LOW):** Workflow + Credit tabs have no "Format JSON" button the
  scenario (1.11.c/d) references; the textarea accepts pasted JSON and Save normalises it.

## Phase 3.A — Dr Dlamini submits a Mini-CEX ✓

- **3.1 ✓** Trainee `/activities/new` type picker shows exactly the 10 Paed types (Demo IM types
  correctly scoped out). Selecting Mini-CEX renders the full 3-section / 12-field form, no crash
  (post-T068).
- **3.2 ✓** Filled all 12 fields; Save draft → "Draft created.", activity 1 persisted (Draft,
  subject = Dlamini). NOTE: Save draft keeps the URL at `/activities/new` (does **not** flip to
  `/activities/{id}`); the activity must be reached via `/activities/{id}` or My Activities. Minor.
- **3.3 ✓** From the **detail page** (`/activities/1`) the trainee sees **Submit** only in Draft.
  Submit → state draft→**submitted**; button set becomes **[Recall]**. The workflow widget filters
  transitions by from-state correctly (no "Recall in Draft" — the earlier note was a crashed-circuit
  artifact and is wrong).
- **F-A3-3 (widget gap, HIGH → T069):** the runtime `ActivityForm` renderer has **no dedicated
  controls for EPA / User / Scale field types** — all fall through to raw inputs (EPA → number,
  User → text, Scale → number; only Choice is a real `<select>`). A trainee must type a raw EPA
  **id** and a raw assessor **GUID** by hand. The schema-driven forms aren't usable by real
  trainees until this lands. (`ActivityForm.razor` switch groups Number/Epa/Likert/Scale/Rating →
  number, Text/User/Signature/ProcedureRef → text.) Task file `T069`. (Because there's no EPA
  picker, the cross-institution-leak question can't be judged from the UI; a future picker's EPA
  lookup must be speciality-scoped — T069 note.)

## Phase 3.B — Dr Naidoo rates and completes ✓ (+ actor-DSL verified)

- **Actor-DSL field binding ✓ (key engine test):** as **Patel** (an Assessor but NOT the named
  `assessor_user_id`), `/activities/1` (submitted) renders with **zero action buttons** (no Accept);
  as **Naidoo** (the named assessor) the same page shows **[Accept]**. `field:assessor_user_id`
  actor resolution works both directions.
- **3.4 ✓** Naidoo → Accept → state submitted→**rated**; button becomes **[Complete]**.
- **F-A3-4 (functional gap, MEDIUM → T070):** in **Rated** state the detail page shows the data as
  **read-only** (DOM: 0 editable, 12 disabled fields) with **no assessor-note field**. Scenario
  Step 3.5 ("assessor adjusts Communication 4→3, adds a note, then Completes") **cannot be
  performed**. Task file `T070`.
- **3.5 ✓** Complete → state rated→**completed** (terminal, no buttons), no crash. All four
  transitions persisted: create / submit / accept / complete.

## Phase 3.C — Credit verification — engine WORKS, with two real findings

Two Mini-CEX activities were driven to completion for Dlamini against PAED-001 (CurriculumItem id 2,
`MinimumLevelOrder = 4`, RequiredCount 30); credit rule has `minimum_level_field = overall_level`:

- **Activity 1 — overall_level 3 (below min 4):** completed cleanly; **`CurriculumItemProgresses`
  got ZERO rows** — no credit at all.
- **Activity 2 — overall_level 4 (meets min 4):** completed; **one progress row written** (verified
  in DB): `CurriculumItemId=2, TraineeUserId=Dlamini, CountsSoFar=1, MinimumLevelReachedCount=1,
  LastActivityId=2, CreditedActivityKeysJson=["2:complete"]`. **→ The credit engine works
  end-to-end through the real UI. Headline Act 3 goal MET at the data layer.**

**F-A3-5 (credit semantics, HIGH → T071, domain decision):** with `minimum_level_field` set,
`CreditApplier` treats the level as an **all-or-nothing gate** — a below-min completion credits
**nothing**, not even the volume count (`if (!minimumLevelReached && MinimumLevelField present)
continue;` at `CreditApplier.cs`). Because of that `continue`, whenever a row *is* written
`minimumLevelReached` is necessarily true, so `MinimumLevelReachedCount` can never differ from
`CountsSoFar` — **the two-counter model is dead.** The scenario (Step 3.6) expected volume to accrue
regardless of level ("1 of 30" with the level below target). Decide (A) count volume always + gate
only the level-reached counter [matches scenario, revives the 2nd counter], or (B) keep the gate +
fix the scenario. Recommend A on EPA/CBME grounds. (Seeded PAED-001 `MinimumLevelOrder=4`, so
level-3 is genuinely one short — the scenario's Step 3.6 narrative is consistent with option A.)

**F-A3-6 (progress page does not surface credit, HIGH → T072):** even with the verified credit row
present (activity 2, item PAED-001, counts=1), the trainee's **`/portfolio/progress` page shows no
PAED-001 entry** ("PAED-001" not found in the page text; h1 "My progress"). So the credit is
persisted but the trainee-facing progress surface doesn't render it. (My earlier "dashboard shows 1
of 30" claim was an artifact of a crashed circuit and was never true — corrected.) Needs
investigation: the progress query/projection vs the `CurriculumItemProgresses` row. Task file `T072`.

## Phase 3.H — Audit log ✓

As bootstrap admin, `/admin/audit` renders 50 rows, **no error banner, no JsonException** (T045
`[PRINCIPAL]` substitution holds through the new lifecycle writes); Activity events present
alongside the schema-build and Act 2 events.

## End-of-session DB state (snapshot `act3-minicex-credited`)

- Activities: 1 = completed (level 3, no credit), 2 = completed (level 4, credited), 3 = **submitted**
  (a stray Mini-CEX left mid-flow by a botched batch — harmless; can be recalled/deleted or ignored).
- `CurriculumItemProgresses`: 1 row (PAED-001, Dlamini, counts 1, minReached 1, lastAct 2).

## Deferred to a fresh session
3.D procedure-log stage-minimum gating (most valuable distinct remaining test), 3.E DOPS
(in-training assessor), 3.F MSF (time-based workflow + notifications), 3.G stalled-assessment triage,
3.I full dashboard sweep. Each of 3.D–3.F needs that type's full schema built first via the
(now-fixed) builder (~15 builder interactions each).

## Follow-up tasks raised
- **T067** ✅ shipped (builder Add-field crash).
- **T068** ✅ shipped (trainee activity-create read scope).
- **T069** (open, HIGH) — `ActivityForm` runtime pickers for EPA / User / Scale.
- **T070** (open, MEDIUM) — assessor rating-edit + note surface in Rated state.
- **T071** (open, HIGH, domain decision) — credit `minimum_level_field` volume-vs-gate semantics.
- **T072** (open, HIGH) — `/portfolio/progress` does not render an existing `CurriculumItemProgress`
  row; credit is invisible to the trainee even when persisted.
- Doc fixes: 1.11.b "13"→12 fields; 1.11.c/d "Format JSON" button; Step 3.6 reconcile with T071.
