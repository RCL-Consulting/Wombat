---
id: T098
title: "Adopt the Paediatric EPA catalogue v11.1"
status: done
priority: P3
---
# T098 — Adopt the Paediatric EPA catalogue v11.1

## Status: IN PROGRESS — **phases 1 and 2a are DONE and live** (catalogue seeded 2026-09-16, four CPSA WBA tools bound to the six-rung ladder). Phases 2c-4 open.

> The 'no catalogue data loaded / Phase 1 blocked' wording that stood here until 2026-09-17 was stale by a
> day. The scale-pinning decision it referred to was resolved by the operator confirming nothing is live;
> the hazard itself survives as **T109** and is now the most serious open defect in the credit path.
> Reachability and seed-propagation gaps found since: **T099** (nobody scoped to the new speciality),
> **T103** (seed edits inert on an existing DB — shipped), **T108** (uncreditable EPAs offered — shipped).

## Source

`EPA version 11.1.docx` — **College of Paediatricians of South Africa**, Work-Based Assessment
Committee (Chair: KL Naidoo, UKZN), September 2026. Fifteen EPAs in five domains.

The document was extracted and parsed; the machine-readable results are banked alongside this
task so the next session does not have to repeat it:

- `T098-data/annexure-a.json` — the 15 EPAs from Annexure A: number, title, domain, descriptor
  count, observation frequency, **Y1–Y4 target levels**, WBA tool set, currency/status.
- `T098-data/epa-detail.json` — per-EPA description, the full text of all **78 descriptors**,
  and the assessment/frequency prose.

Extraction integrity checks that passed: per-EPA descriptor counts match Annexure A's own column,
and the total (78) matches the figure the document states independently.

> **⚠ The document's cover reads "DRAFT — FOR DISCUSSION. Content is subject to revision."**
> The operator described the EPAs as finalised. Confirm which is true before seeding this as the
> authoritative national catalogue — T091 made the catalogue College-owned and institution-adopted,
> so publishing it is a national act, not a local one.

## What v11.1 requires

**Fifteen EPAs, five domains:** AEC (1–3), CN (4–5), CDP (6–9), LPS (10–11), ECP (12–15).

**A six-rung entrustment scale in four bands: 1, 2, 3a, 3b, 4, 5.** Only level 3 is split, and the
document is explicit about why: *"the gap between 'check everything with me' and 'call me if you're
worried' is the whole of a registrar's third year, and a single number hides it."*

**A per-EPA target level for each training year.** Two curves:

| Curve | Y1 | Y2 | Y3 | Y4 | EPAs |
|---|---|---|---|---|---|
| Acute / continuous | 3a | 3b | 4 | 5 | 1, 2, 3, 4, 5, 6, 7, 10, 12 |
| Episodic / annual | 2 | 3a | 3b | 4 | 8, 9, 11, 13, 14, 15 |

Level 1 is never a target.

**Observation frequency** from "One per annum" to "Six per annum" — **55 observed encounters per
registrar per year** (~5/month over an 11-month academic year, ~220 across the four-year programme).

**21 entrustment decisions per registrar per year** — 6 EPAs decided each semester (12) plus 9
decided annually. Every decision is taken by the **Clinical Competency Committee**; for EPAs 4 and 5
it is the **neonatal team CCC**. Never by a single assessor and never from a single form.

**Fourteen WBA tools:** MSF (all 15 EPAs), CBD (12), Mini-CEX (9), DOPS (8), Direct observation (8),
CCA (4), Reflective exercise (4), RCA (3), Clinical audit (3), Chart-stimulated recall (2), Case note
review (1), Directly observed clinical examination (1), Learner feedback (1), Portfolio and logbook
review (1).

## What Wombat already has

Established by reading the code, then adversarially re-checked. These need **data, not code**:

- **The six-rung scale is storable today.** `EntrustmentLevel` is `(int Order, string Label)` with
  `Label` free text to 200 chars, a unique index on `(ScaleId, Order)`, and **no maximum level
  count in any validator** — they require only ≥2 levels, orders contiguous from 1, and distinct
  labels. Map `Order 1..6` to labels `1, 2, 3a, 3b, 4, 5`.
  `src/Wombat.Domain/Epas/EntrustmentLevel.cs:3-12`
- **An Administrator can build that scale unaided** through the T054 CRUD at
  `/admin/entrustment-scales`. `Order` is never typed — it is derived from row position — so they
  type only the labels. `Components/Pages/Admin/EntrustmentScales/EntrustmentScaleEdit.razor:59-87`
- **The Y1–Y4 curve is already expressible and already enforced.**
  `CurriculumItem.MinimumLevelByStageJson` is a training-year → level map; the credit engine gates
  on it (T073) and both trainee surfaces render it. Values validate 1..20, so a sixth rung is legal
  today. `src/Wombat.Domain/Curricula/CurriculumItem.cs:27-38`
- **Formative observation vs summative committee decision is already structurally enforced** (T031),
  matching v11.1's separation of observation from the CCC decision.
- **`EntrustmentDecision` / `PendingEntrustmentDecision` are already per-EPA**, wired to
  `CommitteeReview`, `EntrustmentLevel`, evidence links and an expiry job. Much of the machinery for
  "21 CCC decisions per registrar per year" exists.

## 🚨 Decide this BEFORE any v11.1 data is entered

**Nothing binds a stored entrustment level to a scale.** `CurriculumItem.MinimumLevelOrder`,
`MinimumLevelByStageJson` values, activity `DataJson` scale values and `MsfResponseAnswer.ScaleValue`
all persist a **bare integer** with no `ScaleId` and no version pin.

Insert 3a/3b — making six rungs — and every stored `4` and `5` silently changes meaning: today's
`4 = "Independent"` becomes v11.1's `4 = "3b, trainee decides"`. Issued STARs are safe because
`EntrustmentDecision.AuthorisedLevelId` is a foreign key; **curriculum minimums and every historical
WBA rating are not.**

This is cheap to handle before the data lands and expensive afterwards. Options, in preference order:

1. Pin the ordinal to a scale (add `ScaleId` to `CurriculumItem`, and record the scale on the
   activity value or its schema version) and migrate existing rows explicitly.
2. Create v11.1 as a **new, separate scale** and leave the existing "O-R Scale" untouched, accepting
   that old and new programmes use different scales and nothing cross-compares them.
3. Accept the reinterpretation, but only after confirming production holds no historical ratings
   that matter. **Do not choose this without checking.**

`src/Wombat.Domain/Curricula/CurriculumItem.cs:8-22` (no `ScaleId`);
`src/Wombat.Infrastructure/Persistence/DataSeeder.cs:140-144`

## Gap list

| # | Gap | Kind | Effort |
|---|---|---|---|
| 1 | No time dimension in the credit engine at all. `CreditApplier` reads no date; one `CurriculumItemProgress` row per (item, trainee), DB-enforced unique for life. "Six per annum, resetting each year" cannot be represented even as data. | schema + engine | **large** |
| 2 | No encounter date exists. Mini-CEX/CBD/DOPS/ACAT schemas carry no date field — only `Activity.CreatedOn`, an audit timestamp. A December encounter signed in January buckets into the wrong training year. | data + code | medium |
| 3 | The 78 descriptors have no home — no sub-EPA, milestone or descriptor entity exists (none of the 49 DbSets). | schema | large |
| 4 | No field for the five practice domains. `EpaCategory` is a dead two-value enum read by nothing. | schema | small |
| 5 | 8 of the 14 WBA tools have no seeded activity type: CCA, RCA, Clinical audit, Chart-stimulated recall, Case note review, Directly observed clinical examination, Learner feedback, Portfolio and logbook review. | data + code | medium |
| 6 | MSF carries no EPA reference and has **no credit path whatsoever** — yet v11.1 requires MSF on all 15 EPAs. | schema | medium |
| 7 | No per-EPA panel routing, so the neonatal CCC for EPAs 4–5 cannot be expressed. No semester or review-cadence concept. | schema | medium |
| 8 | Zero of the 15 v11.1 titles match the existing catalogue exactly; 4 are absent outright; Wombat holds EPAs v11.1 does not contain, which must be **retired**, not merely supplemented. | data | medium |

## Traps found during the analysis

Each of these is live today, independent of v11.1, and each would distort the result if hit mid-adoption.

1. **The trajectory chart silently discards any rating above 5.**
   `GetEpaTrajectoryForTrainee` drops the entire observation when `rating < 1 || rating > 5` — no
   error, no log. Every top-rung v11.1 observation would vanish from the progress charts. The same
   query recognises only four tool families, so **10 of v11.1's 14 tools would never chart at all**,
   whatever the rungs say.
   `Features/Activities/Queries/GetEpaTrajectoryForTrainee/GetEpaTrajectoryForTraineeQuery.cs:176-181, :48-55`
2. **Editing the seeded schema JSON is inert on any existing database.** `DataSeeder` skips activity
   types whose key already exists, so widening the hardcoded `"options": ["1".."5"]` arrays requires
   a re-publish per type or a data migration — and `SchemaValidator` *enforces* options membership,
   so until that happens a sixth rung is **rejected at submit** with a validation error. In-flight
   activities stay pinned to their schema version and keep rejecting it.
   `SchemaValidator.cs:160-163`; `DataSeeder.cs:248-251`
3. **`RebuildCurriculumProgress` is destructive and time-blind.** It deletes all
   `CurriculumItemProgress` rows, then re-credits — resolving the trainee's stage from
   `DateTime.UtcNow` rather than the encounter date, so a Y4 trainee's Y1 encounters are re-scored
   against the Y4 minimum. It also skips inactive profiles, so **rebuilding wipes graduated
   trainees' progress irrecoverably.**
   `RebuildCurriculumProgressCommand.cs:30-32, :59-65`; `CreditApplier.cs:176-188`
4. **The seeded WBA forms are not wired to the entrustment scale at all** — `scale_key: "or_scale"`
   matches no scale by Id or Name, and `ActivityForm.GetOptions` prefers the hardcoded literals over
   the catalogue.
5. **Every display site prints `"{Order}. {Label}"`**, so a six-rung scale renders as "3. 3a",
   "4. 3b", "6. 5" on the STAR certificate, the portfolio PDF, the committee picker and the trainee's
   authorisations page.
6. **Two divergent hardcoded tool taxonomies** exist (`SourceByActivityKey` for committee sampling
   warnings, `SourceByActivityFamily` for the trajectory) — four keys each, different value types,
   only one prefix-matches. Any tool-mix model has to reconcile both, not extend one.

## Suggested phasing

**Phase 1 — catalogue as data (low risk).**
Resolve the scale-pinning decision above. Create the six-rung College scale. Load the 15 EPAs with
codes, titles and domains; set each one's Y1–Y4 map from `T098-data/annexure-a.json`; retire the
non-v11.1 EPAs. Set the sub-speciality default scale.
*Outcome: the catalogue is faithful and visible. Quotas are not yet enforced.*

**Phase 2 — stop the UI lying (narrow code).**
Un-clamp the trajectory chart and widen its tool-family list. Thread the rung **label** through
`CurriculumItemDto` / `TraineeDashboardSummaryDto` and drop the ordinal prefix so the app says "3b"
rather than "4". Add a real `observed_on` field to the WBA schemas and honour it in the trajectory
and committee evidence windows. Re-publish the four seeded WBA types so the sixth rung is accepted.
*Outcome: a clinician reading the screen sees v11.1's language and all their data.*

**Phase 3 — make the quota real (the hard one).**
A period key on `CurriculumItemProgress`, a period resolver anchored on the encounter date, a
period-aware read model, and per-EPA tool-mix counters. Fix the rebuild's time-blindness and its
destruction of alumni records first — a per-year tally would be equally unrecoverable.
*Outcome: "55 encounters a year" is enforceable rather than decorative.*

**Phase 4 — governance.**
Per-EPA panel routing for the neonatal CCC, semester cadence, and an EPA agenda on reviews so the
"6 each semester + 9 annually" pattern can be scheduled and chased.

Phases 1–2 give a faithful catalogue. Phase 3 is what makes v11.1 a rule rather than a description.

## Verification

- All 15 EPAs present with correct code, title, domain and Y1–Y4 curve, checked against
  `T098-data/annexure-a.json` row by row.
- A Mini-CEX submitted at the top rung is accepted, credited, and **appears on the trajectory chart**.
- A trainee's progress page shows the target as `3b`, not `4`.
- The portfolio PDF and STAR certificate print rung labels, not `"{Order}. {Label}"`.
- No pre-existing curriculum minimum or historical rating changed meaning — verified by querying
  before and after.

## Open questions for the College

1. Is v11.1 final, or is the "DRAFT — FOR DISCUSSION" cover current?
2. Does the 55-encounters-per-year total **include** MSF, which has no credit path today?
3. Does a training "year" run on the academic calendar or on each trainee's own start date? Wombat
   currently computes stage as 365-day blocks from the trainee's start date, which will not align
   with an 11-month academic year.
4. Do the 78 descriptors need to be individually assessable/creditable, or are they narrative scope
   for the EPA? This decides whether gap 3 is a schema change or a documentation field.

---

## Progress log

### 2026-09-16 — pre-work: the two live defects fixed (no catalogue data yet)

Started on the traps rather than the catalogue, because trap 1 would otherwise have silently
hidden any six-rung data created later: an observation at the top rung would simply not appear,
with no error to investigate.

**Trajectory now accepts rungs above 5.** `GetEpaTrajectoryForTrainee` rejected any rating
outside 1..5 and `continue`d past the whole activity. Replaced with a `MaxPlausibleRung` sanity
bound (20) that still rejects a mis-mapped percentage or year but no longer encodes an assumption
about how many rungs a scale has.

**The chart axis grows to fit.** `TrajectoryChart.MaxRating` defaulted to a hard `5`, plotting
anything higher off the top. It is now `int?`; unset, it derives `Math.Max(5, points.Max(rating))`,
so a five-rung scale renders exactly as before and a six-rung scale renders correctly.

**Credit resolves the stage from the encounter date, not today.** `CreditApplier` called
`GetStage(DateOnly.FromDateTime(DateTime.UtcNow))`, so replaying history judged every past
encounter against the trainee's *current* year. Now resolved from the activity's `CreatedOn`
(with fallbacks to the earliest transition, then now, so an activity with no date set does not
resolve to year 0001 and a null stage). Live submissions are unaffected — `CreatedOn` is
effectively today — but `RebuildCurriculumProgress` no longer rewrites the meaning of
`MinimumLevelReachedCount`.

**Graduated trainees are no longer wiped by a rebuild.** The trainee lookup filtered on
`IsActive`, which `TraineeProfile.Complete()` clears on graduation. Since the rebuild deletes
every progress row before replaying, alumni came back with zero and could not be restored by
re-running it. The filter is gone; an active profile is still preferred when a user has more
than one.

**Trajectory tool families extended** for v11.1 (`cca`, `rca`, `case_note_review`,
`chart_stimulated_recall`, `direct_observation`, `observed_clinical_exam`) so those tools chart
as soon as the activity types exist.

> **A broader fix was tried and reverted.** Making an unrecognised activity type fall back to its
> own display name — rather than being excluded — looked like the right cure for the hard-coded
> list. It is not: the existing test `IgnoresUnratedActivityTypes` correctly asserts that a
> trainee-authored `reflective_note` must not appear on an **entrustment** trajectory, and the
> fallback would have put any type carrying a numeric field onto it. The allow-list stays. The
> real fix is a flag on `ActivityType` marking it as producing an entrustment rating, so
> admin-built tools chart without a code change — **added to the gap list below.**

**Tests:** +5 in `Wombat.Application.Tests`, +2 in `Wombat.Web.Tests` (319 and 45; 433 across the
four suites). Each fails against the pre-change code: rung 6 dropped, implausible rating drawn,
year-1 encounter judged at the year-3 minimum, graduated trainee credited nothing, axis capped at 5.

**Process note:** the first test run reported all-green against **stale binaries** because
`dotnet test --no-build` resolves `bin/x64/Release` while `dotnet build Wombat.sln` writes
`bin/Release`. Two genuine failures were hiding behind that. `CLAUDE.md` now says not to use
`--no-build` in this repo.

### Added to the gap list

| # | Gap | Kind | Effort |
|---|---|---|---|
| 9 | `ActivityType` has no flag marking it as producing an entrustment rating, so the trajectory and the committee sampling warnings both rely on hard-coded key lists. An institution that builds its own rated tool through the Activity builder is invisible to both — which defeats the platform's premise that new tools need no developer. | schema | small |

### Still not started

Everything in "Suggested phasing" below. No entrustment scale has been created, no EPA data
loaded, no catalogue retired. **The scale-pinning decision above is still open and still blocks
Phase 1.**

### 2026-09-16 — Phase 1: the catalogue is seeded

Unblocked by the operator confirming **nothing is live or released**, so no existing rating needed
preserving and the scale could be created outright rather than migrated around. The scale-pinning
hazard above is therefore no longer a *data* risk; it remains a design item (see "Still open").

**`Epa.Domain` added** (`string?`, max 200) with migration `20260916142354_T098_EpaDomain`,
generated by `dotnet ef` (which works in this repo — hand-writing was unnecessary). Deliberately
free text rather than an enum: domains are defined per College and differ between disciplines.

**New `PaediatricCatalogueSeeder`**, separate from `DataSeeder` because this is a real national
catalogue rather than demo content. Idempotent; runs at startup after the other seeders. It seeds:

- **College of Paediatricians of South Africa** (`CPSA`) → Speciality → SubSpeciality "Paediatrics".
- **CPSA Paediatric Entrustment Scale v11.1** — six rungs, `Order` 1-6, `Label` `1, 2, 3a, 3b, 4, 5`,
  each with the band description from the document. Set as the sub-speciality's default scale, so
  committee STAR pickers are constrained to this ladder (T076).
- **The 15 EPAs**, College-owned (`OwningInstitutionId = null`), codes `PAED-001..015`, each with
  its domain, description, and its descriptors carried in `RequiredKnowledgeSkills`.
- **Curriculum "Paediatric EPA Curriculum" v11.1** with 15 items carrying `MinimumLevelByStageJson`
  (the Y1-Y4 curve as scale orders) and `MinimumLevelOrder` (the year-4 target).

Catalogue data lives in `src/Wombat.Infrastructure/Persistence/Seeds/paediatric-epa-v11.1.json`,
generated from `T098-data/` and shipped as content.

**Verified against the dev database**, not just asserted:

- Migration applied; six levels seeded with the expected order/label pairs.
- All 15 EPAs present with domains; both curves stored correctly
  (`{"1":3,"2":4,"3":5,"4":6}` and `{"1":2,"2":3,"3":4,"4":5}`).
- All **78 descriptors** persisted.
- `RequiredCount` totals **220** — 55 per year × 4, matching Annexure B.
- **Second run creates no duplicates** (1 college, 1 scale, 6 levels, 1 curriculum).

**+11 catalogue integrity tests** (`PaediatricCatalogueSeedTests`, Application now **330**). They
assert the figures the document states *independently* of its per-EPA tables — 15 EPAs, 78
descriptors, 55 observations/year — plus that every curve is monotonic, never targets rung 1,
matches its year-4 target, and that every EPA specifies MSF. An editing slip in the seed file now
fails a test instead of reaching a portfolio.

> **The `--no-build` trap bit twice more during this work**, once via `dotnet run --no-build`, which
> silently ran a stale binary and reported "No migrations were applied" while the database was in
> fact untouched — briefly looking like the seeder had run when it had not. The `CLAUDE.md` note now
> covers `dotnet run` as well.

### Still open after Phase 1

- **Seeded WBA schemas still cap the scale at 5 options.** `SchemaValidator` enforces `options`
  membership, so a rating at rung 6 is rejected at submit. Fixing this needs a schema-version bump
  and a re-publish per type — `DataSeeder` skips activity types whose key already exists, so editing
  the seed JSON alone changes nothing on an existing database. **This is the next task.**
- The old scenario EPAs (`PAED-*` under the pre-existing demo discipline, different titles) still
  exist alongside the new national catalogue. They are a separate sub-speciality so there is no
  collision, but they should be retired.
- Scale pinning (`CurriculumItem.ScaleId`) — no longer urgent, still the right model.
- Phases 2-4 unchanged.

### 2026-09-16 — Phase 2a: paediatric WBA tools bound to the six-rung ladder

**Discovered while doing this: the assessor cannot enter a rating at all.** `DataPatchJson` has
**zero** occurrences in the entire Web and Api layer; `/activities/{id}` renders `ActivityForm` with
`ReadOnly="true"`, and the transition is dispatched with a null data patch, so `ActivityService`
leaves `DataJson` untouched. The only data an activity will ever hold is what the **creator** typed
at creation. `NewActivity.razor` also hard-codes `SubjectUserId` to the current user, so
"assessor creates it for the trainee" is not available either.

This is already filed as **T070** ("No assessor rating-edit / assessor-note surface", open since the
Act 3 play-through). It, not the schema options, is the real blocker on recording entrustment. The
seeds below are authored for the correct clinical model (trainee requests, assessor completes)
rather than contorted around a gap that T070 will close. **Until T070 ships, `complete` cannot be
satisfied through the product.**

**Four paediatric WBA tools seeded**, Speciality-scoped to Paediatrics, covering the rated tools
v11.1 leans on hardest: `mini_cex_cpsa` (9 EPAs), `cbd_cpsa` (12), `dops_cpsa` (8),
`direct_observation_cpsa` (8, and previously not seeded at all in any form).

How the six-rung binding actually works, established by reading the runtime rather than guessing:

- **`options` is omitted.** Declaring it does two bad things at once: it caps the validator at those
  exact strings (rejecting the sixth rung) *and* it overrides the College ladder in the picker, so
  the clinician sees bare numbers instead of 3a / 3b. `ActivityForm.GetOptions` gives a declared
  `options` array absolute precedence over the catalogue.
- **`scale_key` is the scale exact Name**, "CPSA Paediatric Entrustment Scale v11.1". It resolves
  against `EntrustmentScale.Id` (int-parsed) or the exact `Name`, so the seed stays declarative with
  no runtime id substitution. A wrong string raises no error; the field silently degrades to a
  plain number box. (The old seeds' "or_scale" matches nothing, which is why they survive purely on
  their hard-coded options.)
- **`validation: min 1, max 6`** is the only remaining server-side guard once `options` is gone.
- The rating field is named **`overall_level`**, which the EPA trajectory query already reads.

Three traps designed around, each of which fails silently rather than loudly:

1. **`submit` is declared first.** The New Activity page fires the *first* transition out of the
   initial state that the creator may take. With `cancel` first, pressing Submit would cancel the
   request outright.
2. **Only `completed` is terminal.** Credit fires on any transition into a terminal state, and an
   abandoned request still carries a filled-in `epa_id`, so terminal declined/cancelled states would
   count refused and withdrawn requests toward the trainee observation volume. The ten older seeds
   still have this flaw.
3. **Assessor fields are not `required: true`.** Every transition validates the whole schema in
   Submit mode, so a required assessor field would also block `decline` and `cancel`. They are gated
   by `requires_fields` on `complete` instead. (The older seeds' accept/decline/cancel are unusable
   for exactly this reason.)

**`observed_on` added**, a real encounter date that no existing WBA schema captures. Nothing reads it
yet; it exists so phase 3's per-year quota has a trustworthy date to bucket on instead of an audit
timestamp.

**Keys are `<family>_cpsa`.** The suffix matters (the trajectory matches a family by exact key or by
a `<family>_` prefix, so a `cpsa_` prefix would never chart), and `_cpsa` rather than `_paed` matters
because earlier scenario play-throughs left institution-scoped `mini_cex_paed` and `dops_paed` types
behind. `ActivityType.Key` is globally unique and the seeder **skips** existing keys, so the first
attempt here silently seeded only two of the four tools. A key collision is a no-op, not an error.

**+36 seed tests** (`CpsaWbaSeedTests`, 9 theories over 4 tools; Infrastructure now **50**) asserting
rung 6 validates, 3a/3b validate, 0 and 7 are rejected, `options` is empty, `scale_key` matches the
seeded scale name exactly, `submit` is first, only `completed` is terminal, and assessor fields are
gated by `requires_fields`. The pre-existing `AllSeedJsonFiles_ParseCleanly` picks the new folders up
automatically.

**Verified on the dev database:** all four seeded at Version 1, Scope = Speciality, ScopeId =
Paediatrics; `scale_key` resolves to the six-rung scale.

### Still open after Phase 2a

- **T070 blocks end-to-end rating entry**: an assessor-editable form on the activity view plus a
  `DataPatchJson` on the transition. This is now the critical path, not a polish item.
- A paediatric trainee needs a `UserSpecialityScope` row for Paediatrics **and a fresh sign-in**
  before these tools appear: the scope filter reads claims baked into the auth cookie.
- Ten v11.1 tools remain unseeded (CCA, RCA, clinical audit, chart-stimulated recall, case note
  review, directly observed clinical exam, learner feedback, portfolio/logbook review, plus MSF and
  reflective exercise in paediatric form).
- The rung picker renders "Order. Label", so rung 3a shows as "3. 3a". Cosmetic, in
  `ActivityReferenceDataService`.
- The old scenario `*_paed` activity types and `PAED-*` EPAs still sit alongside the new catalogue.

### 2026-09-17 — phase 2a follow-ups verified and filed as their own tasks

The "Still open after Phase 2a" list above was checked against the source and the dev database. It was
accurate but incomplete, and one item was materially wrong. Each surviving item now has its own task file,
per the append-only rule.

| Was listed as | Now |
|---|---|
| "T070 blocks end-to-end rating entry" | **Correct, and larger than filed.** T070 rewritten with verified ground truth + an 11-step plan. `requires_fields` cannot be used to derive the assessor's editable set — it widens the full Submit validation pass and is never read by `WorkflowEvaluator`. The model is an `editable_by` actor rule on section/field/state. |
| "a paediatric trainee needs a `UserSpecialityScope` row … and a fresh sign-in" | **→ T099.** Understated: `UserSpecialityScopes` holds **zero** rows for Speciality 3, so the entire catalogue is invisible to every non-admin user, not just to one test trainee. |
| "Seeded WBA schemas still cap the scale at 5 options … next task" | **→ T103.** The general problem is that *any* seed JSON edit is inert on an existing DB (`PaediatricCatalogueSeeder.cs:113` skips existing keys). Needs a canonicalising `ActivityTypeSeedRefresher`, not a per-type re-publish. |
| "Ten v11.1 tools remain unseeded" | **Confirmed**, and the annexure union is exactly 14 tools with 4 seeded. Two of the ten are not plain activity-type seeds: **MSF is a first-class aggregate** (`Wombat.Domain/MultiSourceFeedback/`) with no activity-type key, and the seeded generic `reflective_note` is Speciality-scoped to the demo General Medicine speciality, so it is not the v11.1 reflective exercise. Decide those two separately. |
| "The rung picker renders 'Order. Label' … Cosmetic" | **→ T100.** Six sites concatenate; a further eight print the bare ordinal with no label; one PDF site prints the raw `DataJson` integer with no scale mapping at all. |
| "The old scenario `*_paed` types and `PAED-*` EPAs still sit alongside" | **→ T104.** Only **four** `*_paed` types exist, not ten. The codes do not collide (unique index is `(SubSpecialityId, Code)`). Nothing in `src` seeds this data, every relevant FK is `RESTRICT`, and deactivating an EPA does not hide it. This is a hand-run live migration carrying the 5-rung/6-rung re-point hazard — **do it last**. |

Also filed from the same pass: **T101** (activity read authorization), **T102** (trainee can self-assign as
assessor and self-award credit), **T105** (every transition validates in full Submit mode, so a half-filled
draft cannot be cancelled) and **T106** (ten smaller verified activity-platform gaps).

**Phase 3 is two changes, not one.** `observed_on` is captured by all four new schemas but is read by
**nothing** — `CreditApplier.cs:199` and `GetEpaTrajectoryForTraineeQuery.cs:141` both still date
observations from `Activity.CreatedOn`. The bucketing date has to be wired before per-year buckets mean
anything. Blast radius of the period key itself is now enumerated in T106 item 10 and in the session handoff:
a new column plus a new unique index on `CurriculumItemProgressConfiguration.cs:16` (hand-written migration
**with** its `.Designer.cs` and a snapshot update), `CreditApplier.cs:74-107`,
`RebuildCurriculumProgressCommand.cs:30-32`, eight progress readers and two UI sites.

**Also worth knowing before phase 3:** `CurriculumItem.WindowMonths` already exists, is validated and is
editable in the admin UI — and the credit engine never reads it. Its only consumer is `AdmitTrainee.cs:126`.
Do not assume a currency window is already enforced.

---

## DECISION — 2026-09-19 — the period anchor for phase 3 is the **fixed academic year**

Open question 3 (`:193-194`) is closed. "Six per annum" means a **fixed national academic year**
(Annexure B: *"about 5 a month across an eleven-month academic year"*), **subdivided into two
semesters** (*"25 of the 55 fall in each semester … the remaining 5 are scheduled throughout the year
as opportunities arise"*).

Chosen over trainee-anchored 365-day blocks, which would have been free — `TraineeProfile.GetStage`
(`:66-75`) already computes exactly that and `CreditApplier` already uses it to select a stage minimum.
The academic year was chosen because it is what the College document says and because a shared boundary
is what makes a departmental "how are we doing this semester" view computable at all; trainee-anchored
periods give no two registrars the same boundary.

### What this decision obliges

1. **`GetStage` and the period resolver must be reconciled.** They now disagree by construction: stage
   is a 365-day block from `ProgrammeStartDate`, the period is a fixed calendar window. One of them
   moves, or they stay separate concepts with separate names and the difference is documented. Decide
   this *explicitly* — silently having two notions of "year" is how the lossy `RequiredCount` happened.
2. **Mid-year starters get a short first period.** A registrar starting in August does not get eleven
   months before the boundary. Needs a rule: pro-rata the quota, exempt the partial period, or carry
   forward. Not decided.
3. **The academic year's actual boundaries are not in the repo.** Annexure B says eleven months; it does
   not say which. Needs the month, and probably needs to be configurable per College rather than a
   constant.
4. **Semesters are required, not optional** — 7 of 15 EPAs are decided per semester. Their `currency`
   string (`"Essential — each semester (six months)"` vs `"… annually (12 months)"`) is present in
   `paediatric-epa-v11.1.json` on every EPA but is deserialized by nothing: `EpaSeed`
   (`PaediatricCatalogueSeeder.cs:397-405`) declares no `currency` property. A `PeriodKind` should be
   seeded explicitly rather than parsed out of prose.
5. **`CurriculumItem.WindowMonths` already exists**, is admin-editable and validated, and is read by
   nothing in the credit path (T106:88-94). An implementer will find a period-shaped column already
   there. Say whether phase 3 uses it or leaves it.

### Still open on phase 3 after this decision

The column set and types on `CurriculumItemProgress` and `CurriculumItem`; the migration story for the
four existing progress rows; the back-compat rule for the eight existing progress readers; and whether
`RequiredCount` becomes per-period or gains a sibling. **And `observed_on` must be wired first** — it is
the bucketing date, it is captured by all four CPSA schemas, and it is read by no C# code today.
