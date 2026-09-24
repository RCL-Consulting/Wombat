---
id: T131
title: "EPA governance: the committee cannot route, schedule or chase entrustment decisions"
status: in_progress
priority: P2
owner: agent
depends_on: [T130]
created: 2026-09-20
started: 2026-09-24
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

## Update 2026-09-24 — EPA-stream survey

Four things the survey found missing from this brief. Observed at `431e69e` unless marked otherwise.

**(i) Annexure A's `currency` column is this task's, and this file never mentions it.** D39 left it here: "left for
[T131]" (`EPA-PROGRAMME.md` § 3E, D39). The catalogue carries it, deliberately undeserialised and guarded by name
(`PaediatricCatalogueQuotaSeedTests.cs:71`). It cannot be parsed as a cadence: EPA 9's value is "Required —
opportunity with adolescents with LTHCs" (`paediatric-epa-v11.1.json:520`).

Recommendation: seed an **explicit per-EPA decision-cadence key taken from Annexure B**, as D39 did with
`observationsPerSemester`. Annexure B's `entrustment_decision` column (`tasks/done/T098-data/annexure-b.json:7-26`)
gives EPAs 1, 2, 4, 5, 10 and 12 decided each semester, and 3, 6, 7, 8, 9, 11, 13, 14 and 15 annually. `:33` gives the
neonatal CCC for EPAs 4 and 5. Seeds stamp on create only, so an existing database needs a migration, or a rebuild
(W-007).

Decide with [T139] whether "expiry period if not practised" is enforced, so that a STAR lapses, or is informational.
Today a STAR's `ExpiresOn` is a free, optional date the chair types (`ReviewDetail.razor:196-197`), and nothing lapses
an entrustment for an EPA not practised. Recommendation: cadence only for now, with the expiry reading recorded as
deferred. What the column means is also a College question the survey proposed.

**(ii) D38 is the operator decision that gates the agenda design** (`EPA-PROGRAMME.md` § 3C, D38). Recommendation (a):
each staged decision or agenda line names at least one item from the frozen evidence snapshot. It costs less than it
looks. `EntrustmentEvidenceLink`, `PendingEntrustmentDecision.EvidenceLinksJson` and the materialisation at ratify
(`RatifyCommitteeDecision.cs:70`) already exist. The only UI caller passes `Array.Empty<…>()`
(`ReviewDetail.razor:427`), and the validator requires no link (`StagePendingEntrustmentDecision.cs:45-49`). The work
is a picker over the snapshot and a `NotEmpty` rule.

**(iii) The committee-structure question in Notes goes to the College as § 3F question 6**, added to
`EPA-PROGRAMME.md` on 2026-09-24. Build per-institution in the meantime. The survey recommends a national
decision-body tag on the EPA (neonatal for PAED-004 and 005) plus a per-institution mapping from tag to panel. Then a
"national" answer moves only the mapping. Inferred: one review per panel per trainee per period keeps chair and ratify
authorisation unchanged.

**(iv) Siblings, not scope.** These were filed from the same survey:

- [T165]: a committee decision can be taken by one person (panel size, attendance, the Administrator bypass).
- [T166]: the committee cannot see a trainee's standing against Annexure A's year targets or the exit rule.
- [T167]: the evidence snapshot names no EPA, and a STAR can be staged outside the trainee's curriculum.

D38's evidence links draw from the snapshot, so land [T167] and [T138] before the agenda, or with it.

**Resolved since filing:** the Root cause's "do not exist until [T130]". `AcademicPeriod` and `QuotaWindow` exist
(`Domain/Curricula/AcademicPeriod.cs`, `QuotaWindow.cs`), so `depends_on: [T130]` is satisfied.

---

## Design — 2026-09-24

**Basis.** Three proposals went to three judges (buildability, College fit, security/data), and all three judges picked **minimal**. This design is minimal plus the grafts the judges named, with fixes for the flaws all three proposals shared. It was read at `c4fa3be`, with T167 at `f78519c`. That branch already has master merged in (`662d3e4`), `StarCurriculum` has `.InForce()`, and ratify re-checks the curriculum before the first mutation.

### Decisions (rejected alternatives in *italics*)

1. **Cadence is an explicit key on each curriculum item, taken from Annexure B (D39).**
   - `CurriculumItem.DecisionCadence` reuses `QuotaPeriod` and is nullable everywhere. Null means the item has no published cadence: it is never due but can still be decided.
   - `AcademicYear` is the enum's zero value, so no seed record, DTO, select binding or clone may fill it in by default.
   - *Rejected: parsing `currency`; a new enum, because that would be a second calendar concept.*
2. **Decision bodies come from a national list.**
   - A `DecisionBodies(Key, Name)` table holds one row: key `neonatal`, name "Neonatal team Clinical Competency Committee". It is seeded from a catalogue key, `decisionBodyVocabulary`.
   - Items and panels point to it with a foreign key.
   - *Rejected: a free-text slug; data-first's route table, which blocks every stage until routes are configured, while the brief requires a fallback.*
3. **Routing is one predicate, and the agenda line records the result.**
   - A panel is *eligible* for a trainee when it is at the trainee's institution and is either institution-wide or covers the trainee's speciality. This folds in T194 item 2.
   - `DecisionRouting.RoutesTo(item, panel, trainee, bodyPanels)`: an item tagged with a body goes to an eligible panel carrying that body, a speciality match before an institution-wide one. It goes to eligible default panels only when no such panel exists.
   - Once a line exists, the stage check asks only whether this review has a line for the EPA. The predicate runs again only when a chair adds a line.
   - *Rejected: checking routing live at stage or ratify, which would strand a review already started when a panel is re-tagged.*
4. **The review stores its own period.**
   - It gets `AcademicYear` and `Semester`, set from a period select that fills in the evidence window. The window stays editable.
   - *Rejected: deriving the period from `ReviewPeriodTo`, which `AcademicPeriod`'s remarks forbid; making `ScheduledOn` nullable.*
5. **Annual EPAs are not tied to one semester.**
   - An annual EPA appears on every binding sitting in its year until it is decided. Until the year's last sitting it reads "Due by year end", is optional, and never counts as missed.
   - A *closing* line is a semester-cadence line, or an annual line at the semester-2 sitting.
   - *Rejected: nine red lines at every semester-1 sitting; pinning annual EPAs to semester 2, which the College never states.*
6. **A closing line must be staged, or deferred with a reason, before the review can be ratified.** Ratify is shown disabled with the outstanding lines named (the T107 pattern). "Missed" is computed, never stored.
7. **No job writes committee records.** *Rejected: a job that creates reviews under a system principal, a nudge job, a 31-day grace period.*
8. **D38(a) as adopted:** each decision names at least one item from this review's frozen snapshot, about any EPA. *Rejected as a rule: requiring evidence about the same EPA. It is shown as a hint instead.*
9. **The D14/D42 partial-period exemption is not reused for decisions.** A partial-period window keeps its line, labelled "Partial period". The line is optional and never counts as missed.
10. **Expiry stays informational** (T139, College question 10). `EntrustmentDecisionExpiryJob` and the free-text `ExpiresOn` do not change.

### Data model and migrations

- **Slice 1: `T131_EvidenceProvenance`**
  - Add `EntrustmentEvidenceLinks.CommitteeEvidenceId int NULL`, with no foreign key (T167's snapshot rule).
  - Make the pending-decision index on `(ReviewId, EpaId)` unique.
  - Replace `EvidenceLinksJson` with `EvidenceItemIdsJson`, which holds ids only. Delete existing pending rows (W-007).
- **Slice 2: `T131_DecisionCadenceAndBodies`**
  - Create `DecisionBodies`. Add to `CurriculumItems`: `DecisionCadence int NULL`, `DecisionBodyKey varchar(32) NULL` (foreign key, restrict) and `DecisionIsOpportunistic bool NOT NULL DEFAULT false`.
  - New catalogue keys, from `annexure-b.json`:
    - `decisionCadence`: semester for EPAs 1, 2, 4, 5, 10 and 12; annual for the other nine.
    - `decisionBody: "neonatal"` on EPAs 4 and 5.
    - `decisionOpportunistic: true` on EPAs 8, 9 and 13. This follows Annexure A's "as rotation allows", "opportunity with adolescents" and "as opportunity allows". An opportunistic line is never a closing line.
  - The seeder writes these on create and logs a warning when a stored value differs. The migration stamps the 15 national items by EPA code from its own permanent copy of the values, as T122 did.
  - `Curriculum.CloneAsNewVersion` copies all three fields, with a "carried deliberately" comment. `UpdateCurriculumItemCommand` requires all three, with null allowed.
  - `currency` stays unread; its guard-test note is re-pointed at College question 10 and T139.
- **Slice 3: `T131_PanelBodies`**
  - Add `DecisionPanels.DecisionBodyKey` (foreign key, restrict).
  - Add a unique index on `(InstitutionId, SpecialityId, DecisionBodyKey)`, NULLS NOT DISTINCT, where `DecisionBodyKey IS NOT NULL`.
- **Slice 4: `T131_ReviewAgenda`**
  - Add `CommitteeReviews.AcademicYear` and `Semester`, both NOT NULL.
  - Add a partial unique index on `(TraineeUserId, PanelId, AcademicYear, Semester)` for binding reviews in the Scheduled, InProgress or Decided states. The schedule handler checks this first and names the existing review. A remediation or post-appeal sitting is still possible after ratify.
  - New table `CommitteeAgendaLines`:
    - Columns: `Id`; `ReviewId` (cascade delete); `CurriculumItemId`; `EpaId`; `EpaCode` and `EpaTitle`, frozen at creation; `Origin` (Cadence or Chair); `WindowYear`; `WindowSemester?` (null means a year window); `IsClosing`; `IsPartialPeriod`; `State` (Due, Deferred, Decided or NotDecided); `DeferralReason?`, marked `[Redact]`; `EntrustmentDecisionId?`.
    - Unique on `(ReviewId, EpaId)`.
    - Check constraints: semester is 1 or 2; Decided if and only if a decision id is set; Deferred if and only if a reason is set.
  - The table has no user-id column; the audit row records who deferred. `ErasureExecutor` therefore does not change, and a test asserts that.
  - "Staged" is not stored. It is derived from the pending row, which is now unique per EPA.

### Handlers and queries

- **Stage.** The command takes `EvidenceItemIds`, which must be non-empty and distinct. Checks run in this order, all before any mutation:
  1. Authorise first, giving one refusal for both unknown and out-of-scope ids (T194 item 1).
  2. The chair check.
  3. `DemandTraineeAtPanelInstitutionAsync`.
  4. `StarCurriculum.DemandAsync`.
  5. The review has a line for this EPA, or `RoutesTo` passes and a Chair line is added.
  6. Every id is in `review.EvidenceItems` and is not a `SupervisorReport` row, because `EntrustmentEvidenceLink` cannot hold one. The picker leaves those rows out too.
  7. No other pending decision exists for this EPA.

  `PendingEntrustmentDecision.Stage` also refuses an empty id list.
- **Ratify.**
  - Checks before any mutation: `DemandStagedAsync`, D38 on each pending decision, the closing-line rule, and T165's quorum check once T165 lands.
  - Then a single `SaveChanges`. This is needed because `IApplicationDbContext` has no transaction, and the audit pipeline commits whatever was saved before an exception.
    - Load the prior active STARs first.
    - Build the evidence links on the server from the frozen snapshot rows (label, summary, `CommitteeEvidenceId`).
    - Supersede through a navigation property, `SupersedeBy(EntrustmentDecision)`.
    - Move the lines: staged becomes Decided; a Due line that is not closing becomes NotDecided; Deferred stays Deferred.
  - `EntrustmentDecision.Issue` refuses an empty link list.
- **Delete `IssueEntrustmentDecisionCommand`.** No UI or API calls it; only three test files use it.
- **`AgendaPlanner.PlanAsync(review)`**
  - It takes the trainee's in-force items that have a cadence and `RoutesTo` this panel, using `StarCurriculum`'s predicate, scoped on `OwningInstitutionId`.
  - The window comes from `QuotaWindow.For(cadence, …, programmeStart)` for the review's period. An item that already has a Decided line in that window is skipped.
  - Three callers use it, so picker = gate:
    - the schedule preview;
    - `ScheduleCommitteeReview`, for binding reviews only;
    - `StartCommitteeReview`, which only adds lines and then freezes them together with the snapshot.
- **`DeferAgendaLine` and `ReinstateAgendaLine`.** Chair only, with the trainee at the panel's institution, both checked before any mutation.
- **Setting a panel's body.** Only an InstitutionalAdmin or Administrator may do it. This closes a hijack: today a SpecialityAdmin can create a Speciality panel for any speciality and could claim the neonatal tag. An unknown panel and an out-of-scope panel get the same refusal.
- **Reads**
  - `GetCommitteeReviewById` gains an `Agenda`, read through `DemandReviewAccess`. A trainee sees line states only after ratify.
  - `GetCommitteeRoutingQuery(InstitutionId, Principal)` returns who decides each EPA, or "no neonatal panel: routed to general". An Administrator must name the institution; anyone else gets their own institution regardless. Out of scope returns null (a 404).
  - `GetEntrustmentDecisionsDueQuery(Year, Semester, InstitutionId?, Principal)`:
    - Trainees come from `TraineeScopeResolver`, filtered by `IsAdministeredOrCoordinatedBy`, with the same institution pinning and no CommitteeMember arm.
    - Statuses: Decided, Scheduled, Due by year end, Deferred, Partial period, Missed, Not scheduled.
    - **Decided means a STAR issued in the window that is still Active or Superseded.** A revoked one reads "Revoked: re-decide".
    - "Today" comes from `ProgrammeCalendar.DateOf`.

### UI (DESIGN.md patterns)

- **ReviewDetail**
  - A new Agenda `detail-card` above the pending decisions. Its `.clinic-table` shows EPA, window (`2026 S1` or `2026`), a state badge, the evidence count, and an action (Stage, Defer or Reinstate).
  - Badges, to be recorded in DESIGN.md:

    | State | Badge |
    |---|---|
    | Due, Due by year end, Partial period | `badge-draft` (told apart by label) |
    | Staged | `badge-submitted` |
    | Decided | `badge-completed` |
    | Deferred | `badge-accepted` |
    | Not decided | `badge-declined` |

  - The evidence picker is a `<fieldset>` `.check-grid` over T167's grouped snapshot, with the line's EPA group first.
  - Stage is disabled until an item is ticked, with `.workflow-action-reasons` reading "Name at least one item of the evidence snapshot". A hint appears when only one form is named ("never from a single form"), or when no named item is about this EPA.
  - Lines routed to another panel for this period are shown read-only, for example "PAED-004: Neonatal CCC, not yet decided". The progression form warns while one is still undecided (sitting order).
- **PanelEdit:** a "Decides for" select, shown only to InstitutionalAdmin and Administrator.
- **PanelsList:** a "Who decides each EPA" card.
- **ReviewsSchedule:** a period select that fills in the dates, and an agenda preview, for example "PAED-004 and 005 are decided by the Neonatal CCC: schedule separately".
- **New `/committee/decisions-due` page (list pattern):**
  - filters for period, status and EPA;
  - a summary per EPA;
  - a table of outstanding items, with a "Schedule" link that pre-fills the schedule form;
  - `StatePanel` for loading, error and empty, plus a NavMenu entry.
- **Curriculum item editor:** fields for cadence, body and the opportunistic flag. National items stay read-only to institutions.

### Entrustment-only reviews (slice 5)

- Add a third review type, `CommitteeReviewType.EntrustmentOnly`. A review on a panel with a decision body is always this type; a general semester-1 sitting may also be.
- It ratifies without a progression category. `Ratify` branches on the type, and the enum's "no engine logic" remark is rewritten.
- A remitted appeal on an entrustment-only review records no category.
- *Rejected: letting a neonatal CCC write a progression outcome, a role the College never gives it.*

### Siblings

- **T167:** a hard prerequisite; merge it first. Afterwards, re-run the T182, T183 and T158 tests, because T167 rewrote Stage and Issue.
- **T165:** its checks go in ratify's pre-mutation block, with one `DemandChairAccess`. Whichever task lands second rebases.
- **T166:** its standing becomes columns on the agenda card; pre-graduation lines follow it.
- **T194:** item 2 is folded into slice 3, and item 1's single refusal applies on every path this task touches.
- **T139:** unchanged.

### Build slices

Every slice: the full suite per project with no `--no-build`, a mutation check on each new gate, and a browser check on dev.

1. **D38 and ratify integrity** (S–M; after T167). Verification:
   - The validator refuses an empty id list.
   - An id from another review, and a SupervisorReport row, are refused and nothing is written.
   - The stored link matches its frozen row.
   - Staging the same EPA twice is refused: a handler test plus a PostgreSQL index test.
   - Ratify with an unlinked pending decision is refused; the review stays Decided and no STAR is issued (an audit-trap test).
   - `git grep IssueEntrustmentDecisionCommand` finds nothing.
   - Browser: stage with one item, ratify, and the certificate lists that item.
2. **Cadence and bodies** (S–M). Verification:
   - All 15 items have an explicit cadence, and 6×2 + 9 = 21.
   - Bodies appear on EPAs 4 and 5 only.
   - A cloned curriculum version keeps all three fields.
   - The first boot stamps the values; the second changes nothing.
3. **Routing** (M). Verification:
   - Handler tests: EPAs 4 and 5 route to the neonatal panel where one is configured and to the default panel where none is, never to another institution's or another speciality's panel.
   - A SpecialityAdmin cannot set a body.
   - Scope: institution B's admin gets null for A's routing, and an attempt to set a body on A's panel writes nothing.
   - Institutions A and B can each own a neonatal panel.
4. **Agenda** (L). Verification:
   - Query test on the line states: Due, Staged, Decided (with the STAR id), Deferred with a reason, and NotDecided.
   - An annual line at a semester-1 sitting does not block ratify; a closing line does.
   - A December review is covered.
   - PostgreSQL tests for the constraints.
   - Grep `Features/CommitteeDecisions` for `new DateOnly(`, `AddMonths(` and month literals: no new hits.
   - Browser, with a neonatal panel at the dev institution: the general review's agenda leaves out EPAs 4 and 5, and staging PAED-004 there is refused.
   - Update Act 4 of the runbook.
5. **Entrustment-only reviews** (S–M). Verification:
   - A review on a body panel ratifies with no category.
   - A general AnnualProgression review still needs one.
   - A remitted-appeal test.
6. **Decisions-due page** (M). Verification:
   - A test covering the full status matrix, including a revoked STAR.
   - Institution B's query ignores `InstitutionId = A`.
   - An Administrator must name an institution.
   - bUnit tests for the page's states.

### Open questions (each has a default, so none blocks the build)

**Operator**
- **O1. D38 minimum:** one named item or two, given page 4's "never from a single form"? *Default: one, with the hint.*
- **O2. Unstaged closing lines:** should they block ratify? *Default: yes; deferring with a reason is the way out.*
- **O3. Late starters:** do they owe decisions in a partial period? *Default: the partial-period line is optional.* Ask alongside College question 4.
- **O4. Routed bodies:** do they record no progression category? *Default: yes.*
- **O5. Exit level:** is an EPA already at exit level re-decided every semester? *Default: yes; T166 shows where the trainee stands.*
- **O6. Deferral reason:** can the trainee see it after ratify? *Default: yes.*
- **O7. Opportunistic EPAs (8, 9, 13):** are they never closing? *Default: yes.*
- **O8. Sitting order:** when the general sitting comes before the neonatal one, warn or refuse? *Default: warn.*

**College**
- **Question 6 (committee structure):** a national answer changes only the panel-to-body mapping. It could also seed panels and turn the fallback into an error.
- **Question 10 (currency):** the cadence does not depend on it; expiry stays informational.
- **New:** must a decision cover all four of page 8's information sources? *Default: show coverage read-only.* The candidate's self-reflection (T170) is not in the snapshot yet.

### Risks

- `ReviewDetail.razor` is also edited by T165, T166 and T167, so merges need sequencing.
- The agenda is frozen at Start: routing or curriculum changes made afterwards only reach the next review.
- A trainee who moves institution strands their open review, which T182 refuses to act on; an Administrator closes it.
- Snapshots taken before T167 have no `EpaId`; re-seed (W-007).
