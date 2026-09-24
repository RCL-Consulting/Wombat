---
id: T167
title: The committee's evidence snapshot names no EPA, tool, rating or encounter date, and a STAR can be staged on an EPA outside the trainee's curriculum
status: done
priority: P2
owner: agent
depends_on: [T137]
created: 2026-09-24
completed: 2026-09-24
---

# T167 — The committee's evidence snapshot cannot say which EPA a line is about

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. The snapshot is what a committee sitting reads before it decides. Today every line says only "Mini-CEX
(Paediatrics) #14, State: completed, created …". To learn which EPA a line is about, or what it was rated, the panel
must open each activity. The STAR the chair then stages is not held to the trainee's own curriculum.
**Surfaced:** 2026-09-24, the EPA-stream survey, gap "the committee's evidence view". [T131] covers only an agenda with a
state per line. [T137]'s `Activity.EpaId` stamp would make the grouping a join, but T137 is scoped to the trainee's own
list.

**Depends on [T137]:** the EPA on each line should come from the stamped `Activity.EpaId`, not a fourth re-parse of
`DataJson`.

## Symptom

Observed at `431e69e`:

- `StartCommitteeReview.cs:94-100`: each activity's evidence line is `SourceLabel = "{type name} #{id}"` and
  `Summary = "State: …; created …; updated …"`. It carries no EPA, no instrument, no rating and no encounter date.
  The window is taken on `ObservedOn` (`:65-73`), but the line prints `CreatedOn`.
- `ReviewDetail.razor:233-247` renders the snapshot as one flat list. It is not grouped by EPA or by information source,
  and the page loads no quota position (`:351-357`).
- The STAR EPA picker is `ListEpasForSubSpecialityQuery(authState.User)` (`ReviewDetail.razor:357`;
  `Features/Epas/GetEpas.cs:66-80`). It lists the EPAs in the chair's scope, not the trainee's curriculum.
- `StagePendingEntrustmentDecision.cs:84-85` checks only that the EPA exists. The level is looked up by id alone
  (`:86-87`). Not verified: whether anything else checks it is a rung of the item's pinned ladder.

## What to build

1. **Self-describing lines.** Each activity line records the EPA (code and title, from `Activity.EpaId`), the
   instrument (`WbaToolKey`, shown by name), the rating as a rung label on the pinned ladder when the type is rated,
   the encounter date (with [T161]'s undated marker), and the state. Store these on the frozen `CommitteeEvidence` row,
   so the snapshot stays a record of what the committee saw even if the activity later changes. That needs a
   migration; read it after generation.
2. **Grouped by EPA** on the review page, with rated, unrated and MSF evidence under each EPA, and the EPA's quota
   position for the review period from [T130]'s reader. Keep lines in every state, labelled. [T135] recommends the
   snapshot keep showing all states, and [T138] removes the MSF campaigns that should not be there.
3. **STAR staging bound to the trainee's curriculum.** The picker offers only EPAs on the trainee's curriculum (scoped
   on `OwningInstitutionId`). `StagePendingEntrustmentDecision` refuses any other EPA, and any level that is not a rung
   of the item's pinned ladder. It refuses before any mutation (the audit trap).

## Verification

- [x] A snapshot line for a rated activity names its EPA code, instrument, rung and encounter date. Application test on
      `StartCommitteeReview`.
- [x] The review page groups the snapshot by EPA. bUnit test, and in the browser.
- [x] Staging a STAR on an EPA outside the trainee's curriculum is refused and writes nothing; the picker does not offer
      it. Application test, and in the browser.
- [x] Staging with a level off the item's ladder is refused. Application test.
- [x] Browser, on dev: a review whose window holds a CCA and a released three-EPA MSF shows them under the right EPAs.
- [x] Full suite green, no `--no-build`.

## Related

[T137] (the `EpaId` stamp; dependency), [T138] (MSF campaigns in the snapshot), [T131] (the agenda; D38's evidence links
will draw from this snapshot), [T135] (the state decision), [T144] (classification by `WbaToolKey`), [T161], [T165],
[T166], [T109].

## Note from T137, 2026-09-24

`Activity.EpaId` is now stamped from the schema's `evidence_epa_field` (create, transition, MSF release), and the
migration backfilled existing rows. It records the EPA the activity is evidence **for**. It does **not** say the EPA is
on the trainee's curriculum. Staging a STAR must still check the trainee's curriculum (scoped on `OwningInstitutionId`),
not the presence of an `EpaId`.

---

## As built — 2026-09-24

- **Snapshot lines.** Each activity line of `CommitteeEvidenceItems` records its EPA (the stamped `Activity.EpaId`,
  with code and title), its instrument (`WbaToolKey`, by the College's name), and whether the pinned version is rated.
  It also records the rating, as a rung label read through T135's profile; the encounter date, with the undated marker;
  and the source state.
  - Migration `20260924153716_T167_CommitteeEvidenceNamesEachLine` adds nullable columns, with no backfill.
  - `ReviewDetail` groups the lines: one card per EPA in code order, grouped by instrument. Campaign reports go under
    "Not about a single EPA", and pre-T167 rows under "Frozen before lines named their EPA".
- **`StarCurriculum`.**
  - The allowed EPAs are the in-force items of the trainee's preferred profile's curriculum: the national core plus
    their own institution's local items. The allowed levels are the item's pinned ladder.
  - Stage, issue and ratify each refuse anything outside this before any write; ratify re-checks every staged decision.
  - The EPA picker and the level list use the same predicate.

**Browser on dev (scripted Chrome, `5ae1141`):**
- On review 1, the STAR EPA picker lists exactly PAED-001..015 (not the demo EPA-001). After choosing PAED-001, the
  levels are CPSA v11.1's 1, 2, 3a, 3b, 4, 5.
- Review 1's old lines render under "Frozen before lines named their EPA".
- **Not seen in a browser: a new review's grouped snapshot.** No seeded dev account can schedule a review since T182,
  and the verifier may not use the admin credential. The grouping is covered by `ReviewDetailEvidenceGroupingTests`
  (bUnit) and `CommitteeEvidenceSnapshotPostgresTests`. The browser check moves to [T204], which seeds a dev
  Coordinator.

