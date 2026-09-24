---
id: T131
title: "EPA governance: the committee cannot route, schedule or chase entrustment decisions"
status: queued
priority: P2
owner: agent
depends_on: [T130]
created: 2026-09-20
---

# T131 — Entrustment decisions have no routing, no cadence, and no agenda

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium, and deliberately below [T130]. Governance describes how the College's
committee structure is meant to work; the quota describes what a trainee is measured against.
The second is visible to every user daily, the first to a committee twice a year.
**Surfaced:** 2026-09-20. This is [T098] phase 4 and Wave 5 of the programme document. Like
phase 3 it was planned and never filed — § 2 notes it *"also has no task file."*

## Symptom

The summative entrustment decision for an EPA is taken by a Clinical Competency Committee. The
framework says more than that, and none of it is expressible today:

- **Per-EPA panel routing.** EPAs 4 and 5 are neonatal and belong to the neonatal CCC. Every
  review currently goes to one undifferentiated panel.
- **Semester cadence.** Annexure B gives a decision rhythm — roughly six EPAs decided each
  semester and nine annually. Nothing schedules against it.
- **An EPA agenda on a review.** A committee sitting cannot be told which EPAs it is there to
  decide, so nothing can be chased when one is missed.

## Root cause

`CommitteeReview` models a sitting and its decisions, but carries no EPA agenda and no routing
rule. The concepts it would route and schedule against — periods, semesters — do not exist
until [T130] builds them. Phase 4 was correctly sequenced behind phase 3 and then left unfiled,
so the sequencing was invisible to the queue.

## What to build

Not yet designed to the level [T130] is. Before building, do the design pass:

1. **Routing** — how a panel is bound to a set of EPAs. Per-institution, since committee
   structure is a local arrangement even where the catalogue is national. Note that a
   curriculum row is shared across adopting institutions, so any read must scope on
   `OwningInstitutionId`.
2. **Cadence** — express "six each semester, nine annually" against [T130]'s period concept
   rather than inventing a second calendar.
3. **Agenda** — an EPA list on a review, with a state per line, so a missed decision is
   visible rather than merely absent.

Check **D38** before designing the agenda: it asks whether a committee decision must record
what it was grounded in. Page 4 of the source is quoted as unambiguous that it draws on the
standard assessment information sources, which points at an evidence link on each agenda line
rather than a bare outcome.

## Verification

- [ ] EPAs 4 and 5 route to a neonatal panel where one is configured, and to the default panel
      where none is — checked in the browser and by a handler test
- [ ] A review carries an EPA agenda whose lines have their own state — checked by a query test
- [ ] Cadence is expressed against [T130]'s period, with no second calendar introduced —
      checked by reading the code and by grep for a competing date concept
- [ ] An institution's committee configuration is invisible to another institution — checked by
      a scope test in the [T056] family
- [ ] Full suite green — `dotnet test` per project, no `--no-build`

## Related

[T098] phase 4; Wave 5 of `knowledge/EPA-PROGRAMME.md` § 4. Depends on [T130] for the period
concept. **D38** (must a decision record what it was grounded in) is open and shapes the agenda
line. [T056] is the institution-scope pattern this must follow.

## Notes

- **Needs confirmation:** whether committee structure is genuinely per-institution or whether
  the College mandates a national arrangement. The programme document does not say, and it is
  not in the RFI ([T129]) — it was not among the fourteen. Worth adding to a second message
  rather than assuming.
- **Observed:** this task is a design brief, not an implementation plan. Do not treat its
  three-item list as a blast radius the way [T130]'s is.
