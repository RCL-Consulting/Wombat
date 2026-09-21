---
id: T137
title: A campaign's per-EPA evidence rows are indistinguishable on the trainee's own activity list
status: queued
priority: P2
owner: agent
model: opus
depends_on: []
created: 2026-09-21
---

# T137 — Eight identical rows, one per EPA, and nothing on the page says which is which

**Severity:** Medium. Nothing is wrong in the data; the page simply cannot express it. It is the
trainee's own portfolio surface, and a campaign covering eight EPAs makes it unreadable in one act.
**Surfaced:** 2026-09-21, browser-verifying [T121]. The task file predicted "the clutter is cosmetic";
the completeness critic sharpened that correctly to "the rows are not merely numerous, they are
unidentifiable", and the browser run confirmed it.

## Symptom

`/activities/mine` after releasing one MSF campaign covering three EPAs, verbatim:

```
Type                                  State     Updated
Multi-Source Feedback (Paediatrics)   recorded  2026-09-21 08:44
Multi-Source Feedback (Paediatrics)   recorded  2026-09-21 08:44
Multi-Source Feedback (Paediatrics)   recorded  2026-09-21 08:44
```

The only way to tell them apart is to open each one. An Annexure A campaign covers up to fifteen.

## Root cause

Not MSF-specific, and not `DisplayFieldsRule.None`: **no type's display fields are rendered on that page
at all.** `ActivitySummaryDto` carries Id, ActivityTypeId, Key, Name, SubjectUserId, CurrentState,
CreatedOn, UpdatedOn (`ListActivitiesBySubjectQuery.cs:44-52`) and `MyActivities.razor:24-33` renders
Type / State / Updated. There is no EPA and no encounter date in the projection.

`Activity.EpaId` exists as a column (`Activity.cs:15`) and is **written by nothing** — grep finds it
assigned nowhere, read only by `ActivityService.Map`. So the row's EPA lives only inside `DataJson`,
which is not SQL-projectable.

## What to build

Two candidate shapes; the second is the durable one.

1. **Render what is already there.** Add `ObservedOn` to `ActivitySummaryDto` and show it. Helps every
   type (T119 made `ObservedOn` the real date and the list still shows the audit clock), but does not
   separate siblings that share an encounter date.
2. **Populate `Activity.EpaId` at write time**, exactly as T119 populates `ObservedOn`: stamp it from
   the `epa`-typed field the pinned schema declares, on create, draft update and transition. Then the
   list can project and join it, and so can everything else that currently re-parses `DataJson`
   (`GetSamplingConcentrationWarnings.TryParseRating`, `GetEpaTrajectoryForTraineeQuery.TryParseObservation`).
   Needs a pointer at the schema root — `epa_field`, the third after `observation_date_field` and
   `rated_level_field` — with **both** a Parse and a Serialize half, or it is dropped silently at publish.

A grouped display ("Multi-Source Feedback — 3 EPAs, campaign #1") is a third option and is cosmetic
only; it does not fix the projection that every other reader also lacks.

## Verification

- [ ] A trainee whose released campaign covered three EPAs can tell the three rows apart without
      opening them — checked in the browser
- [ ] Whatever is added to `ActivitySummaryDto` is populated for every seeded type, not only MSF —
      checked by a test over the seed corpus
- [ ] If `Activity.EpaId` is populated: a DSL property added for it survives Parse+Serialize — checked
      by a named `SeedRoundTripTests` fixture, per CLAUDE.md's Serialize-half trap
- [ ] Full suite green — `dotnet test` per project, no `--no-build`

## Related

[T121] created the rows that make this visible. [T118] findings 6 and 7 are the neighbouring picker
complaints. [T119] is the precedent for stamping a column from a schema-declared pointer.

## Notes

- **Observed:** `Activity.EpaId` and `Activity.CurriculumItemId` are both dead columns today.
- **Observed:** the information is one click away on `ActivityView`, which renders every field; this is
  a list-projection gap, not a data loss.
