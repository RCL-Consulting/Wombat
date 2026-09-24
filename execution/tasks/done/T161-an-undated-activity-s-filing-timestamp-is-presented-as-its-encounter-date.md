---
id: T161
title: An undated activity's filing timestamp is presented as its encounter date
status: done
priority: P3
owner: agent
depends_on: []
created: 2026-09-24
completed: 2026-09-24
---

# T161 — An undated activity's filing timestamp is shown as though it were the encounter date

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Every CPSA seed declares an encounter-date field, so this reaches only activity types without one. Among
the seeds that is `reflective_note` and `qi_project`, neither rated, plus any builder-made type. Where it applies, the
product shows an audit timestamp as if it were a clinical fact.
**Surfaced:** 2026-09-24, the EPA-stream survey. The decision was taken as D28 (`EPA-PROGRAMME.md` § 3D; [T119] D4, "yes,
as a follow-up behind T100"). [T100] shipped and the follow-up was never filed.

## Symptom

Observed at `431e69e`. When a type's schema has no `observation_date_field`, or the value is missing,
`ObservationDateResolver` falls back to the filing date and records `ObservedOnSource = CreatedOn`
(`ObservationDateResolver.cs:44`, `Activity.cs:51`). Nothing that displays the date reads that source:

- The trajectory: `GetEpaTrajectoryForTraineeQuery.cs:182-185`, in a comment: "NOT exposed here:
  activity.ObservedOnSource … deliberately left to a follow-up".
- The portfolio PDF: `ActivitiesSectionComponent.cs:78-82` prints `ObservedOn` unmarked, with the same deferral comment.
- The activity view shows no encounter date at all. `ActivityDtos.cs` carries no `ObservedOn` (grep).

Seeds with no date pointer (grep of `Seeds/*/schema.json`): `qi_project`, `reflective_note`.

## What to build

Carry `ObservedOnSource` beside `ObservedOn` wherever the date is shown, and mark an undated one: "date not recorded,
filed yyyy-mm-dd". The surfaces are the trajectory table and chart tooltip, the activity view (which should show the
encounter date in any case) and the portfolio PDF's activity lines. If [T137] lands first and adds `ObservedOn` to
`ActivitySummaryDto`, the activity list gets the same marker.

## Verification

- [x] An activity of a type with no date pointer shows "date not recorded" with its filing date on the activity view
      and in the PDF. bUnit test and a PDF section test.
- [x] A rated test type with no date pointer is marked the same way on the trajectory. The seeds cannot show this,
      because both undated seeds are unrated. Query test.
- [x] An activity with a declared date is unchanged on all three. Tests.
- [x] Browser: file a `reflective_note` on dev and read it back on the activity view.
- [x] Full suite green, no `--no-build`.

## Related

D28, [T119] D4, [T100], [T137] (the list DTO), [T169] (the PDF's other gaps).

---

## As built — 2026-09-24

One helper (`EncounterDate.Label`, shared with T137's lists) marks an undated activity's date wherever it is shown as
an encounter date:
- the trajectory's tooltip and screen-reader table, on My progress and on the review page;
- ActivityView's summary;
- the portfolio PDF.

The wording follows T137, "(filed; no encounter date)". [T197] reconsiders it (for a draft it is the created day) and
covers the unmarked "Last encounter", which needs a migration.

Browser on dev, 2026-09-24 (master `b0cb335`, the batch-C check): 
- Reflective Note 31 reads "Encounter date: 2026-09-24 (filed; no encounter date)".
- The exported portfolio PDF (13 pages) prints "2026-09-24 (filed; no encounter date)" on activity 31's line.
- The trajectory marking cannot be shown with the seeds, because both undated seeds are unrated. The page tests cover it.
