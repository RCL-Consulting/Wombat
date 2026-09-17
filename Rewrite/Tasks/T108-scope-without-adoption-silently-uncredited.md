# T108 — A trainee can record an assessment against an EPA their institution has not adopted, and it is silently uncredited

**Status:** shipped 2026-09-17 (both deliverables; see Resolution)
**Surfaced:** 2026-09-17, browser-verifying T070 end to end.
**Severity:** High — the assessment completes, the data is stored, the assessor is thanked, and **nothing
counts**. There is no error, no warning, and nothing on any screen says so.

## What happened

Verifying T070, a paediatric trainee created a CPSA Mini-CEX against `PAED-001 — Providing paediatric
emergency care to children` (EPA id 17), the assessor rated it at the top rung and completed it. The
activity reached `completed`, `DataJson` holds the assessor's rating and feedback — and
`CurriculumItemProgresses` has **no row**.

```
TraineeProfile(dlamini).CurriculumId = 2   (FCPaed(SA) Part 1, v2026.1)   AdoptionId = 1
Epa 17 (CPSA PAED-001) -> CurriculumItem 17 -> CurriculumId = 3   (Paediatric EPA Curriculum v11.1)
InstitutionCurriculumAdoptions: exactly one row — institution 2 -> curriculum 2
```

`CreditApplier` credits only the adopted curriculum version (T091, by design and correctly). The EPA the
trainee picked is not in their curriculum, so no item matched and no credit was applied.

## Root cause — two gates, and only one was opened

**Speciality scope** and **curriculum adoption** are independent:

- T099 gave the paediatric cohort a Speciality-3 scope so the CPSA tools and CPSA EPAs became *visible and
  selectable*. That worked.
- Nobody adopted the CPSA curriculum for KGK, and no trainee is pinned to it. So everything selectable from
  the new catalogue is **uncreditable for these trainees**.

The EPA picker now offers **30 EPAs** — both catalogues interleaved, `PAED-001` through `PAED-015` twice
with different titles and no visual distinction. Fifteen of them silently produce no credit. A registrar
has no way to tell which fifteen.

**This is a consequence of T099.** Before it, the new catalogue was invisible, which was a different and
more honest failure. Making it visible without adoption is worse: it looks like it works.

## Fix — the picker should not offer what cannot be credited

1. **Filter the EPA options to the subject's adopted curriculum.** `ActivityReferenceDataService` scopes
   EPA options by institution/speciality; it should additionally scope by the subject's active adoption
   when the activity has a subject. This is the real fix and it is not specific to paediatrics — any
   institution mid-adoption has the same hole.
2. **Or warn at submit** when the chosen EPA resolves to no curriculum item for the subject. Weaker: it
   still lets the encounter be recorded uncredited, but at least the trainee knows.
3. **Or adopt the CPSA curriculum and re-pin the cohort** — that is T104, and it carries the 5-rung/6-rung
   re-point hazard, so it is not a quick unblock.

Recommendation: **1**, as a defect in its own right. Then T104 on its own merits.

## Wider point worth keeping

`CreditApplier` returning "no matching curriculum item" is indistinguishable, from every surface, from
"credit applied". Whatever else is decided, a completed activity that credited **nothing** should be
visible somewhere — on the activity, in the audit entry, or on the trainee's progress page. Silent
non-credit is how a registrar reaches the end of a year believing 55 encounters were logged.

## Verification

- A trainee whose institution has adopted only curriculum 2 is not offered curriculum 3's EPAs.
- An encounter that credits nothing says so, somewhere a human will see it.
- After adoption (T104), the same flow produces a `CurriculumItemProgress` row.

## Evidence

Dev database, activity id 11, created and completed through the browser on 2026-09-17. Left in place as
evidence; `CurriculumItemProgresses` has no row referencing it.

## Related

Consequence of T099. Blocks meaningful use of the v11.1 catalogue as much as T070 did. Interacts with T104
(adoption + rating remap) and T098 phase 3 (per-year quota, which counts the rows this never creates).

---

## Resolution — 2026-09-17

Both halves shipped. The picker no longer offers what cannot be credited, and a completion that
credited nothing now says so.

### 1. The picker mirrors the credit engine, not the adoption table

`IActivityReferenceDataService.GetEpaOptionsAsync` gained an `EpaOptionScope? scope` parameter
(`SubjectUserId`, `NarrowToCreditable`, `CurrentValue`). When it is supplied and resolves, the
claims filter is **replaced** by a mirror of `CreditApplier.ResolveCurriculumItemsAsync`:

```
offer EPA e  iff  e.IsActive
              AND ∃ ci : ci.EpaId == e.Id
                       ∧ ci.CurriculumId == P(subject).CurriculumId
                       ∧ (ci.OwningInstitutionId IS NULL ∨ ci.OwningInstitutionId == P(subject).InstitutionId)
```

where `P(subject)` is the profile `CreditApplier` itself picks — active first, then latest
`ProgrammeStartDate`, and **not** filtered on `IsActive` (a graduated trainee still credits).

**The fix text above, option 1, is wrong on one point and was not followed.** It says to scope by the
subject's *active adoption*. `CreditApplier` never reads `InstitutionCurriculumAdoptions`,
`Curriculum.IsActive` or `TraineeProfile.AdoptionId` — it reads `TraineeProfile.CurriculumId`, which
is pinned at admission. Scoping the picker by adoption would hide creditable EPAs from every trainee
pinned to a superseded version, and from every trainee whose institution has no adoption row at all
(dev `TraineeProfiles` id 1 today: institution 1, `AdoptionId` NULL, no adoption row, credits fine).
The predicate joins `CurriculumItems`, which is also what keeps T091 institution-local extras — real,
creditable, outside the College's published core — in the list.

Replaced rather than intersected with the claims filter, deliberately: the curriculum-item join
already implies the right discipline, and the viewer on the detail page is routinely the assessor, a
different person in a different sub-speciality from the subject. Keeping both could only over-hide.

Narrowing applies only when all three gates hold, and otherwise falls through to today's behaviour:

1. the field is an `epa` field, and
2. its key is one the pinned version's credit rules actually read (`curriculum_item_match.epa_field`),
   which is what keeps a reflective note (epa field, empty `counts_for`) and an MSF form (epa field,
   fixed `curriculum_item_id`) untouched, and
3. the subject resolves to a `TraineeProfile` whose curriculum has items.

No subject (the builder preview), no profile (a PendingTrainee — `/activities/new` is open to them),
or a curriculum with no items all fall back rather than empty the picker. **An empty picker on a
required field is unsubmittable, which is a worse failure than the one this task is about.**

A stored value is always unioned back into the options, on the read-only render as much as the
editable one. An `epa` field is a `<select>`: dropping a stored value does not narrow a choice, it
renders "Select…" and erases recorded evidence. Activity 11 (`epa_id` 17) is exactly that case.

Wiring: `ActivityForm` gained `SubjectUserId` and `CreditRulesJson`; `ActivityDto` now carries the
pinned version's `CreditRulesJson` alongside `SchemaJson`/`WorkflowJson`. `ActivityView` passes
`_activity.SubjectUserId` (**not** the principal), `NewActivity` passes the current user, and the
builder preview passes neither. The option-reload key is now schema + subject + credit rules, not the
schema alone.

### 2. A completion that credits nothing says so

`ActivityTransition.CreditedItemCount` (`int?`, migration
`20260917151601_T108_TransitionCreditStamp`, Designer + snapshot included) is stamped by
`ActivityService.TransitionAsync` on the transition that caused the credit:

- `null` — credit was never evaluated. Non-terminal, or a pinned version with an empty `counts_for`.
  The six seeded no-credit types (journal_club, procedure_log, qi_project, reflective_note,
  research_output, teaching_session) stay null for ever and can never trip the warning. The
  `counts_for` gate is checked **before** `ApplyAsync` is called, which is what makes that guarantee.
- `0` — evaluated and matched nothing. The signal.
- `N` — that many `CurriculumItemProgress` rows credited.

`ActivityView` renders an `<Alert Kind="warning">` above the details grid — outside the
editable/read-only branch, because a completed activity always takes the read-only branch — plus a
"Credit" column in the history table ("—" / "None" / "N items"). No new CSS, no new component.

`RebuildCurriculumProgress` deliberately does **not** stamp: re-applying already-credited work
returns zero rows by design, and stamping there would flag every correctly-credited activity.

### Known limits, deliberately left

- **Existing rows stay `null`**, including activity 11. The outcome genuinely was not recorded for
  them and inventing one retrospectively would be a guess. A backfill, or a rebuild-that-stamps, is a
  separate decision — do not read the absent warning on activity 11 as the fix failing.
- **`0` conflates "no matching curriculum item" with "the subject has no `TraineeProfile`".** Both are
  honestly uncredited, so the copy names the likely cause and points at an administrator rather than
  asserting one. Splitting them needs `ICreditApplier` to return an outcome enum; not worth it yet.
- **The banner is per-activity.** A registrar still has to open each one to see the pattern. The
  surface that actually closes "fifty-five encounters logged, nothing counted" is a column on
  `MyActivities` — `ActivitySummaryDto` + `ListActivitiesBySubjectQuery` correlating the last
  transition. Cheap now that the data exists; filed as follow-up.
- **The narrowed list can still be a cross-catalogue mix, and that is the T104 hazard.** A KGK trainee
  opening `mini_cex_cpsa` is now offered the fifteen FCPaed EPAs that DO credit — but that type binds
  `overall_level` to the six-rung CPSA scale while curriculum 2's items carry five-rung minima.
  `CreditApplier.MeetsMinimumLevel` would compare a six-rung rating against a five-rung minimum. T108's
  symptom is gone (thirty options down to the creditable set, duplicate PAED-001..015 titles gone) but
  the rung-semantics defect underneath belongs to **T104** and is not fixed here.
- **`ListActivityTypesQuery` is unchanged.** Offering an activity type whose credited EPA set is empty
  for the subject is the honest thing to suppress one level up; that is the same predicate applied to
  the type list and is the natural companion to T099's scope/adoption split.

### Tests

- `tests/Wombat.Infrastructure.Tests/Activities/EpaOptionCreditScopeTests.cs` — the predicate: adopted
  EPA offered, another curriculum's not, institution-local extra offered, another institution's local
  item not, no-profile/no-items/no-scope fall back, the assessor's own claims do not narrow the
  subject's set, a graduated profile still narrows, a stored value is offered back.
- `tests/Wombat.Web.Tests/Activities/EpaPickerScopeTests.cs` — only the credited field is narrowed, it
  narrows to the subject not the viewer, reflective-note and fixed-item types are never narrowed, the
  stored value reaches the service, a subject change reloads.
- `tests/Wombat.Application.Tests/Activities/CreditOutcomeSignalTests.cs` — zero-credit completion
  stamped 0, crediting completion stamped N, empty-`counts_for` type never stamped, non-terminal
  transition never stamped, no-profile subject stamped 0.
- `tests/Wombat.Web.Tests/Activities/ActivityViewCreditSignalTests.cs` — the banner fires on 0, not on
  N, and never on null.

Each was checked to fail against both the pre-fix behaviour and a naive implementation (strict
"hide anything uncreditable", and a stamp without the `counts_for` gate).

### Adversarial review — three fixes, and one finding that outgrew this task

Three reviewers attacked the diff (over-filtering, under-filtering, regression). **The narrowing predicate
held on every subject shape tried** — normal trainee, no profile, graduated/inactive, two profiles, an
assessor in another sub-speciality, the builder preview, institution-local extras, a non-`epa_id` field
key. The original T108 scenario is closed on the create path. Fixed after review:

1. **The anti-empty-picker guard did not guard (medium).** Emptiness was decided on the curriculum-item
   join, but the caller then applied `Where(epa => epa.IsActive)` — a filter `CreditApplier` never
   applies. `DeactivateEpaCommandHandler` does not check for referencing items, so retiring a superseded
   catalogue leaves items whose EPAs are all inactive: the guard saw a non-empty set, the caller filtered
   it to nothing, and a required `<select>` rendered with no options. The `IsActive` join now happens
   inside the resolution, so the guard is decided against the same predicate the caller applies.
   *Test:* `GetEpaOptions_WhenEveryCreditableEpaIsDeactivated_FallsBackInsteadOfEmptyingThePicker`,
   verified to fail against the pre-fix code.
2. **The picker narrowed on a field the engine never reads (low, but a real divergence).**
   `CreditApplier` tests `curriculum_item_id` first, then `curriculum_item_field`, and only reads
   `epa_field` when neither is set — and `CreditRulesParser` does not make them exclusive. A rule block
   naming both had its EPA field narrowed although the choice could not affect credit.
   *Tests:* `CreditRuleFieldsTests`, six cases.
3. **A `.razor` file reached into Domain (low, but a stated rule).** `ActivityForm` gained
   `@using Wombat.Domain.Activities.Credit` and called `CreditRulesParser.Parse` directly, which
   `CLAUDE.md` forbids and no architecture test catches. Moved to
   `Wombat.Application/Features/Activities/Services/CreditRuleFields.cs`, which also fixes (2) — the
   precedence rule now lives one layer from the engine it mirrors, not in a component. It fails open on
   any parse failure, including the `InvalidOperationException` that `System.Text.Json` raises for a
   wrong-typed primitive, which `CreditRulesParseException` does not cover.

**Build clean, 0 warnings. 698 tests green** — Domain 59, Application 363, Infrastructure 171,
Architecture 19, Web 86.

### Noted, not fixed here

- **Cross-institution disclosure on the read-only render.** Narrowing replaces the viewer's claims filter,
  so an assessor at institution A opening an activity whose subject is at institution B sees B's
  curriculum EPAs, including B's institution-local extras. Intersecting with the viewer's claims would
  over-hide for the legitimate cross-sub-speciality assessor, which is why it was replaced rather than
  intersected. The real fix is **T101** — nothing should be loading another institution's activity at all.
- **A `CreditedItemCount == 0` stamp can never be corrected.** The comment claiming
  `RebuildCurriculumProgress` must not stamp because re-applying returns zero is **wrong**: that handler
  deletes every progress row before replaying, so it re-credits properly. The consequence is that after an
  administrator fixes the curriculum and rebuilds, the warning banner stays on for ever. Making the
  rebuild stamp is the fix; it is a separate code path and is left as a follow-up.

### 🚨 T109 — the finding that outgrew this task

Narrowing converts T108's silent **no**-credit into silent **wrong** credit for one combination: a trainee
pinned to a five-rung curriculum filing a six-rung CPSA tool can now only pick a five-rung EPA, so
`MeetsMinimumLevel` compares a rating of `4` — CPSA's rung "3b", *still needing supervision* — against a
curriculum-2 minimum of `4`, meaning *"Independent"*, and credits it.

The mis-comparison was always reachable; T108 removed the alternative. It is the concrete instance of the
scale-pinning hazard T098 identified and left open. **Filed as T109, and it is now the most serious open
defect in the credit path** — wrong credit is worse than absent credit.
