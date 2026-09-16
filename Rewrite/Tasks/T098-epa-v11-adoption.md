# T098 — Adopt the Paediatric EPA catalogue v11.1

## Status: NOT STARTED — gap analysis complete (2026-09-16), no code or data written

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
