---
id: T130
title: "The annual quota: a trainee cannot see what is expected of them this year"
status: done
priority: P1
owner: agent
model: opus
depends_on: []
created: 2026-09-20
started: 2026-09-23
completed: 2026-09-23
---

# T130 — The progress page reads "1 / 24" against a lifetime total nobody published

**Severity:** High. `knowledge/EPA-PROGRAMME.md` § 2 calls it *"the most visible gap in the
product"*, and it is the one question a trainee actually opens the page to answer.
**Surfaced:** 2026-09-20. The work is [T098] phase 3 and has been planned since 2026-09-19 —
but it lived as three paragraphs inside T098 and as Wave 4 of the programme document, never as
a task. § 2 says so in as many words: *"Phase 3 has no task file of its own. Write one before
starting."* This is that file.

## Symptom

Annexure A publishes a per-EPA frequency — so many encounters per semester or per year. The
product does not track against it. `PaediatricCatalogueSeeder.cs:356-361` multiplies the annual
quota by four programme years, so the progress page shows a trainee **"1 / 24"**: a lifetime
total against a number the College never published, on a page whose only real question is
*what is expected of me this year.*

Nothing buckets completions by period, because until [T119] there was no trustworthy date to
bucket on.

## Root cause

There is no period concept. `CurriculumItemProgress` counts completions for all time. The
per-EPA `currency` string that would distinguish *"Essential — each semester (six months)"*
from *"… annually (12 months)"* is carried in `paediatric-epa-v11.1.json` on every EPA and is
**deserialized by nothing**: `EpaSeed` (`PaediatricCatalogueSeeder.cs:397-405`) declares no
`currency` property. Same omission as `wbaTools` before [T122] was filed.

## What to build

Blast radius, already enumerated in § 2 — treat this as the starting map, not as gospel:

- A period column on `CurriculumItemProgress`, plus a new unique index on
  `CurriculumItemProgressConfiguration.cs:16`. **Hand-written migration with its `.Designer.cs`
  and a snapshot update** — see CLAUDE.md; without the Designer file `MigrateAsync()` skips it
  silently.
- A period resolver.
- `CreditApplier.cs:74-107` — credit into the right bucket.
- `RebuildCurriculumProgressCommand.cs:30-32` — replay into buckets. It was rewritten under
  [T119] and still has no production caller; check its atomicity before giving it one.
- Eight progress readers and two UI sites.
- Seed `PeriodKind` **explicitly** from the `currency` string rather than parsing it out of
  prose at read time. Add the property to `EpaSeed` and re-seed.

Nothing is live, so re-seeding and a destructive migration are both available. Prefer them to a
backfill that has to guess.

## Decisions this needs first

**From the College — ANSWERED 2026-09-20, this task is no longer blocked:**

- **D13** — the academic year runs **January to November**, boundary **in June**, national rather than
  per-institution. Recorded as Jan–Jun / Jul–Nov; **confirm whether June itself is semester 1 or 2**
  before bucketing anything. A June encounter is the only thing that moves if that reading is wrong.
- **D14** — a registrar starting mid-year is **exempt for the partial period** and starts counting at
  the next boundary. Say so on the progress page rather than showing a target they cannot meet.
- **D7** — an unrated instrument counts toward the frequency **not at all** (`counts_for: []`), so the
  quota counts rated encounters only. This narrows what this task has to bucket.
- **D8** — MSF does **not** consume the 55, so the quota and the MSF fan-out never interact here.

**From the maintainer, decidable today** — all three have a recommendation in § 3C:

- **D17** — `GetStage` (a 365-day block from `ProgrammeStartDate`) and the period (a fixed
  calendar window) disagree by construction. *Recommended: two named concepts, `Stage` for the
  Y1–Y4 curve and `Period` for the quota, with the difference documented.*
- **D18** — does `RequiredCount` become per-period, or gain a sibling? *Recommended: redefine
  it and delete the multiplication that produced "24".*
- **D19** — does phase 3 use `CurriculumItem.WindowMonths`? *Recommended: leave it, and say so
  here.* It is per-item currency, a different question from the quota period, and it is read by
  nothing in the credit path today — an implementer will find a period-shaped column already
  present and assume currency is enforced. **It is not.**

## Verification

Rewritten on completion. The first two items as originally written assumed the period came from `currency`, and
decision D39 (below, and `knowledge/EPA-PROGRAMME.md` § 3E) reversed that. Each item says what was checked.

- [x] **A trainee's progress page shows the published per-EPA target for the current period, not a lifetime multiple.**
      Browser-verified on dev, 2026-09-23, against **Annexure B**. Annexure A publishes only "N per annum", so it cannot
      check a per-semester figure. PAED-001 reads "1 of 3 this semester", then "2 of 3" after a live completion
      (Annexure B: 3 per semester). PAED-006 reads "1 of 2 this semester" (2). PAED-011 reads "1 of 1 in 2026", met
      (1 per annum). PAED-003 reads "0 of 3 this semester" although its `currency` says "annually": that is D39,
      visible. The summary reads "0 of 10 EPAs met this semester · 1 of 5 met in 2026". Screenshots are in the session
      scratchpad (`evidence/`).
- [x] **The catalogue carries an explicit per-semester figure, and the window and target are seeded from it, not from
      `currency`.** `PaediatricCatalogueQuotaSeedTests` (Infrastructure.Tests) runs the seeder and checks all 15 items
      against a hard-coded Annexure B table: 25 per semester, 55 annualised. It names EPAs 3, 6 and 7 as per-semester.
      It checks that the migration's frozen table equals the catalogue. A guard fails on any catalogue key that is
      neither deserialized nor allow-listed, and that guard has a self-test. `currency` is allow-listed for [T131].
- [x] **Credit lands in the semester containing `ObservedOn`, not `CreatedOn` or the transition time.**
      `CreditApplierTests.ApplyAsync_CreditsTheSemesterContainingObservedOn_NotTheFilingDateOrTheTransitionTime`:
      encounter 30 June, filed 1 July, start not on a boundary; exactly one row, at (2026, 1). On dev, activity 8
      (observed 30 June) sits in semester 1 and activity 9 (1 July) in semester 2.
- [x] **The migration applies on a fresh database, with its `.Designer.cs` and an updated snapshot.** The migration was
      generated by `dotnet ef` (no `--no-build`), and `has-pending-model-changes` reports no changes.
      `AcademicPeriodQuotaPostgresTests` runs `MigrateAsync` on a fresh Postgres schema: all 40 migrations apply,
      including T130, with the new index, three CHECKs, and no column defaults. The same test migrates a populated
      pre-T130 schema, and the one-off correction rewrites exactly the seeded rows. The migration was also applied to
      the dev database at boot.
- [x] **A rebuild reproduces the same bucket counts as incremental crediting.**
      `RebuildCurriculumProgressTests.Rebuild_ReproducesTheSemesterTalliesTheIncrementalPathProduced` uses a fresh
      context per completion, an absolute oracle, and a filing order unlike the encounter order. The same holds on
      Postgres (`AcademicPeriodQuotaPostgresTests`) and in the browser: the psql tallies before and after the manual
      rebuild on `/admin/curriculum-progress` are byte-identical (6 activities, 4 semester tallies, 0 removed). **One
      qualification:** an activity whose encounter date moved after it was credited stays in its old semester until a
      rebuild moves it. That is pinned by a test, and it is the price of never counting one activity twice.
- [x] **Full suite green**, `dotnet test` per project, no `--no-build`: Domain **126**, Application **613**,
      Infrastructure **293**, Architecture **23**, Web **140**, **1195 total** (1040 before). Integration: the 9 new
      Postgres tests pass. The one pre-existing MSF flow test fails at fixture setup on the untouched baseline too
      (filed as [T140]).

## As built (2026-09-23)

**Decisions.** D17–D19 were closed as recommended in § 3C. D39–D42 are new and recorded in § 3E:
- D39: the window comes from Annexure B's per-semester figure, not `currency`.
- D40: S1 is January–June and S2 July–December; June belongs to S1 and December folds into S2.
- D41: storage is per semester, and D14 is applied when progress is read.
- D42: the provisional "part-way through" tolerance. A semester target applies if the programme started within the
  semester's first month; a yearly target is waived only for a start on or after 1 July.
Four questions for the College are listed in § 3F; none blocks anything.

**What exists now.**
- `AcademicPeriod`, `QuotaWindow` (the one implementation of D14/D42), `QuotaPeriod` and `ProgrammeCalendar` in Domain.
- `CurriculumItemProgress` is keyed (item, trainee, `AcademicYear`, `Semester`). The period members are required, and
  the row gains `LastObservedOn` and an `xmin` token.
- `CurriculumItem.QuotaPeriod`, with `RequiredCount` a target per window.
- `CreditApplier` is split into `PlanAsync` (every read) and `Apply` (synchronous). `ActivityService` plans before
  `ApplyTransition`, so nothing awaits between the first mutation and the save, which closes the audit-trap window on
  the credit path.
- The rebuild is period-keyed, rolls back by reference, guards its save and its stamps, and refuses when a completion
  lands mid-rebuild.
- One read model (`QuotaProgressCalculator`, `TraineeQuotaProgressReader`, `CurriculumCoverageReader`) behind the
  progress page, the trainee dashboard and the committee, speciality and sub-speciality dashboards. They show counts,
  never means.
- `/admin/curriculum-progress`, and `CurriculumProgressBootstrapper`, which rebuilds at startup when the table is
  empty but completions have credited. Its audit row is attributed to `system:curriculum-progress-bootstrap`.

**Divergences from the plan above.**
1. The period source was `currency`; it is Annexure B's per-semester figure (D39).
2. The migration was generated by `dotnet ef`, not hand-written (CLAUDE.md: hand-write only when the CLI cannot run).
   It carries five marked hand-edits, including the removal of an `xmin` AddColumn, the same edit T121 needed.
3. The rebuild got a caller: two, in fact. The migration empties the table, so they are required, not optional.
4. Progress readers: five display readers needed quota semantics, not eight. The access report gained the period
   columns (schema version 2); erasure and the scale-delete guard needed nothing.

**Found and fixed on the way.**
- The staff dashboards had no institution filter, and a sub-speciality id is national. Once the committee card listed
  every trainee by name (instead of a few ids above an 80% threshold), that became a cross-institution disclosure. All
  three dashboards now keep to the caller's institution.
- `CommitteeCompletionPercent` is gone: a percentage of a period's target reads as behind in every period's first weeks.
- The undated-activity fallback now uses the South African date, the same calendar as "today".
- The rebuild's activity query is split.
- The D15 entry claimed [T119] enforced date bounds; it never did, and the entry is corrected.

**Review.** An adversarial review of the diff ran six dimensions with three refuters per finding: 28 findings, 22
survived, about 12 of them distinct, all fixed. The two majors were the institution scope and a D14 notice that told a
July starter no target applied while ten did. A design critique before building (four lenses, about 60 findings)
reshaped D42, `LastObservedOn`, the startup rebuild, the plan/apply split and the dashboards.

**Filed, not fixed.** [T139] (WindowMonths seeding and the one-year completion default), [T140] (the MSF integration
fixture), [T141] (no nav route to My progress), [T142] (raw user ids on activity pages), [T143] (the form keeps its
values after submit).

## Related

[T098] phase 3; Wave 4 of `knowledge/EPA-PROGRAMME.md` § 4. Depends on [T129] for D13/D14.
[T119] (`observed_on`) is **done** and is what made this startable. [T131] is phase 4 and
depends on the period concept this task creates. C1 closed the period-anchor question on
2026-09-19: the fixed national academic year, subdivided into two semesters.

## Notes

- **Observed:** `WindowMonths` exists, is validated, is admin-editable, is carried through
  clone, and is read by nothing in the credit path. Its only consumer is `AdmitTrainee.cs:126`.
- **Observed:** the § 2 blast-radius list was written 2026-09-19 and cites line numbers. [T119]
  has landed since. Re-check each site rather than trusting the line number.
- **Inferred:** "eight progress readers" is § 2's count, not one this task re-derived.
