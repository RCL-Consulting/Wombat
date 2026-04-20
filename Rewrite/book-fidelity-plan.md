# Book-fidelity plan

> **Superseded by `Rewrite/practical-plan.md`.** This document prioritised academic fidelity to the EPA textbook. The hospital is running a training programme, not doing training research — the forward plan is pragmatic and defensibility-oriented. Kept here for reference and because `EPA Book/critique.md` cites its reasoning. Do not execute tasks from this document.

Roadmap for closing the gaps identified in `EPA Book/evaluation.md`. Each phase lists candidate tasks; individual task files under `Rewrite/Tasks/T0xx-*.md` will be written once a phase is agreed. This document is the shared view; it is append-only — if scope changes, add a new phase rather than mutating an agreed one.

Companion docs:
- `EPA Book/evaluation.md` — the 92-requirement scorecard this plan responds to
- `EPA Book/critique.md` — peer-reviewed challenges to book prescriptions; drives several sequencing and scope decisions below
- `Rewrite/PLAN.md` — the master T001–T027 plan (complete)
- `Rewrite/current_state.md` — live session handoff

## Goal

Raise Wombat's fidelity to the EPA textbook (Ten Cate et al.) from its current post-rewrite baseline to a state where the platform demonstrably implements the book's central constructs — entrustment as a prospective decision, programmatic assessment, and defensible grounded trust — not just the surface vocabulary.

Non-goal: re-architect the schema-driven activity platform. It already serves the "four sources of evidence" well (ch. 17). Closing the gaps is additive, not corrective.

## Guiding principles

1. **Hard-code what carries legal weight, schema-drive everything else.** Entrustment decisions, A RICH, and calibration events join `CommitteeDecision` and `MsfCampaign` as hard-coded aggregates. Routine assessment instruments stay schema-driven.
2. **Resolve the STAR collision early.** The seeded `star_reflection` activity type uses an acronym the book reserves for a completely different artefact (Statement of Awarded Responsibility). Clear it up before building the real thing, not after.
3. **Read models, not live aggregates, drive analytics.** Trajectory/cohort views use projection tables rebuilt from activity/decision history; they do not mutate domain aggregates.
4. **Every new command goes through `AuditPipelineBehavior`.** Entrustment issues/revocations, calibration outcomes, and decision dissent are all audit-bearing.
5. **Prefer extending seeded schemas over forking them.** For A RICH and prospective/retrospective ratings, amend the seeded mini-CEX / DOPS / CbD schemas; don't ship parallel variants.
6. **Stage-gate on the build.** Architecture tests, domain tests, application tests, and bUnit smoke tests stay green at every commit — same rules as T001–T027.
7. **Data quality before analytics.** Rater leniency and ceiling effects (Kogan 2009; Pelgrim 2011) are well-documented in direct-observation WBA. Analytics built on uncalibrated assessor data mislead committees. Faculty-development work therefore sequences *before* the trajectory/cohort views — see `EPA Book/critique.md` §2–§3 for the supporting literature.
8. **Default to opt-in for trainee-facing structure.** A RICH capture, self-assessment pairing, and prospective/retrospective rating all risk aggravating assessment burden (Boyd 2018; Watling & Ginsburg 2019). New structured fields ship as institution-level toggles, off by default. See `EPA Book/critique.md` §8.
9. **Scale anchors are not universal.** The 5-level entrustment-supervision scale is one construct among several (Rekman 2016); scale performance depends on construct alignment (Crossley 2011). `EntrustmentScale` and `EntrustmentLevel` stay fully editable; admin documentation should cite the Crossley argument.

## Effort legend

- **S** — afternoon (≤½ day): schema or DTO change, small command, one migration
- **M** — 1–2 days: new aggregate + commands + UI + migration + tests
- **L** — 3–5 days: new aggregate with cross-cutting workflow impact, PDF, multiple pages
- **XL** — week+: new subsystem (e.g., analytics stack, calibration workflow)

## Phases at a glance

| Phase | Theme | Book ch. | Effort | Sequence |
|---|---|---|---|---|
| 1 | Fidelity quick wins (schemas only) | 4, 10, 19; Sargeant R2C2 | S×3 | Ships first |
| 2 | STAR / Entrustment Decision aggregate | 7, 10, 18 | L | Blocks analytics |
| 3 | Faculty-development layer | 17, 19, 23 | M–L | **Before analytics — data-quality prerequisite** |
| 4 | Committee & analytics enhancements | 6, 21 | L–XL | After P3 (rating diagnostics inform trajectory views) |
| 5 | Lower-priority fidelity items | 1, 4, 10, 11, 16, 18; plus patient-outcome connector | S–M each | Any time |

Task IDs T028–T045 follow original numbering. T046 (R2C2 feedback) and T047 (patient-outcome connector) were added after critique review; their IDs are non-sequential within their phases.

---

## Phase 1 — Fidelity quick wins

Cheap schema edits that materially improve book fidelity without domain-model changes. No new aggregates, no new migrations for tables (only data migrations for seed refresh).

### T028 — Prospective / retrospective entrustment pair (opt-in)

**Gap closed:** book §19 — every observation should record *both* the supervision actually provided (retrospective) and the supervision level recommended going forward (prospective). Today Wombat captures one.

**Scope:**
- Add a new schema field type `entrustment_pair` to `FormSchema` (two anchored scales with shared anchors: "supervision provided" / "supervision recommended")
- Parser, validator, generic renderer updates
- Update seeded `mini_cex`, `dops`, `cbd`, `acat` schemas to make `entrustment_pair` available but keep single-rating as the default at institution onboarding. Toggle on the `Institution` record.
- `CreditApplier` gains a configuration knob: which rating feeds curriculum progress. Default: retrospective when pair is enabled; single rating otherwise.
- Migration: data-only (bump seeded activity type versions; existing activities stay pinned to their old schema)

**Rationale for opt-in default:** the pair is theoretically clean but two correlated ratings may be treated by assessors as the same field, adding friction without signal (`EPA Book/critique.md` §8). Ship the capability; let programmes adopt deliberately.

**Files touched (illustrative):** `Domain/Activities/FormSchema.cs`, `Infrastructure/Activities/SchemaValidator.cs`, `Infrastructure/Activities/CreditApplier.cs`, `Web/Components/Shared/ActivityFormRenderer.razor`, `Infrastructure/Seeding/ActivityTypeSeeder.cs`.

**Effort:** S–M (probably 1 day; the renderer change is the fiddly part).

### T029 — A RICH trustworthiness attributes (opt-in)

**Gap closed:** book §4, §10 — Agency, Reliability, Integrity, Capability, Humility as structured, assessor-visible fields, not buried in free-form narrative.

**Scope:**
- Add a schema field type `arich_rubric` (five short Likerts + optional narrative per attribute)
- Add a "Trustworthiness" section to seeded mini-CEX / CbD schemas; **off by default at institution level, toggleable by admin**
- Surface A RICH rollups on the trainee dashboard and the committee review evidence panel *only when the institution has enabled the section* — otherwise the rollup widget is hidden
- No new aggregate; data lives in `Activity.Data` jsonb like every other field

**Rationale for opt-in default:** structured rubric capture may produce worse data than a well-designed narrative prompt where the framework is not part of assessor training. Aligns with assessment-burden concerns (Boyd 2018; Watling & Ginsburg 2019). See `EPA Book/critique.md` §8.

**Files touched:** `Domain/Activities/FormSchema.cs`, new renderer component, seed updates, dashboard widget.

**Effort:** S (½–1 day).

### T046 — R2C2-aligned feedback workflow on seeded observation types

**Gap closed:** book §19 treats feedback as content; evidence (Sargeant 2008, 2015; Eva 2012) shows feedback reception is relational. See `EPA Book/critique.md` §4.

**Scope:**
- Extend the seeded workflow for `mini_cex`, `dops`, `cbd`, `acat` to add two new states after the current terminal "completed" state: `awaiting_reaction` (trainee acknowledges and reacts) and `coaching_scheduled` (optional — assessor and trainee agree a coaching conversation)
- Transitions: `acknowledge_feedback` (trainee only), `schedule_coaching` (assessor or trainee), `close_without_coaching` (either)
- New schema field type `feedback_reaction` — short structured reaction (emotional tenor + confidence + one-line narrative) captured at `awaiting_reaction` entry. Stored in `Activity.Data` jsonb.
- Institution-level toggle to opt into the R2C2 extension — off by default for migration compatibility
- Generic renderer handles both the terminal-on-complete and the reaction-extended variants based on the workflow

**Why this is Phase 1 not Phase 4:** it's a schema and workflow change to seeded types. No new aggregate, no new table. R2C2 is specifically about relationship, reaction, content, coaching — the first two are workflow states, the third is the existing feedback narrative, the fourth is an activity that can be logged separately.

**Files touched:** seeded workflow JSON, new field type in `FormSchema`, new Blazor component for `feedback_reaction` rendering, trainee dashboard widget for unacknowledged feedback.

**Effort:** M (2 days — workflow edits are careful work).

### Phase 1 exit criteria

- `entrustment_pair` field type is available; seeded observation types can opt-in per institution (T028)
- `arich_rubric` field type is available; A RICH section is available on seeded types as institution-level opt-in (T029)
- Seeded observation types support an R2C2-aligned reaction-and-coaching extension, off by default (T046)
- No new aggregates, no new tables
- Build + tests green

---

## Phase 2 — STAR / Entrustment Decision aggregate

The load-bearing gap. Three tasks, sequenced.

### T030 — Resolve `star_reflection` naming collision

**Scope:** rename the seeded activity type key `star_reflection` to `situation_task_action_reflection` (or `star_reflection_note` — bikeshed in task file). Rename the type label. Add a data migration that rewrites the `Key` column for existing activity types and any references in `ActivityPermissionRule`. Update the title of existing `Activity` rows' display fields only if cached.

**Why first:** the real STAR aggregate in T031 should own the `STAR` term. Doing the rename after T031 ships is twice the work because references will have multiplied.

**Effort:** S.

### T031 — `EntrustmentDecision` aggregate

**Scope:**

Domain (`Wombat.Domain/EntrustmentDecisions/`):
- `EntrustmentDecision` — aggregate root. Id (GUID v7), TraineeUserId, EpaId, AuthorisedLevel (references `EntrustmentLevel`), IssuedOn (`DateOnly`), ExpiresOn (`DateOnly?`), IssuedByCommitteeReviewId (FK), IssuedByChairUserId, Rationale, Status (`Active`, `Expired`, `Revoked`, `Superseded`), RevokedOn?, RevokedByUserId?, RevocationReason?
- `EntrustmentEvidenceLink` — child. Pointers (by id) to the activities and MSF campaigns that grounded the decision. Snapshot-style; same pattern as `CommitteeEvidence` in T022.
- Domain methods: `static Issue(...)`, `Revoke(reason, actor)`, `MarkExpired()`, `Supersede(byNewDecisionId)`. No direct setters.

Application:
- Commands: `IssueEntrustmentDecisionCommand`, `RevokeEntrustmentDecisionCommand`, `SupersedeEntrustmentDecisionCommand` (issued implicitly when a new decision for same trainee+EPA+level is created while one is Active)
- Queries: `GetActiveDecisionsForTrainee`, `GetDecisionHistoryForEpa`, `ListExpiringDecisions` (for the expiration job)

Infrastructure:
- EF config + hand-written migration + Designer file
- Background job `EntrustmentDecisionExpiryJob` (daily) — flips `ExpiresOn` < today to `Status=Expired`, fires email to trainee + coordinator

**Why hard-coded not schema-driven:** matches the reasoning in `Rewrite/Tasks/T022-committee-decisions.md` — regulatory weight, immutability requirements, appeal pathway, revocation as a first-class event.

**Committee integration:** `RecordCommitteeDecisionCommand` (existing, T022) gains an optional list of `entrustmentDecisions[]` to issue as part of ratification. When the committee ratifies, the decisions issue atomically.

**Effort:** L.

### T032 — STAR certificate PDF + trainee dashboard

**Scope:**
- `IEntrustmentCertificateService` + `EntrustmentCertificatePdfService` (QuestPDF), parallel to `PortfolioPdfService` from T023
- Certificate content: trainee name, EPA title + description, authorised level with scale anchor, issue date, expiry, issuing panel, chair signature block, evidence summary, integrity hash
- Trainee dashboard gains an "Authorisations" panel: one card per Active `EntrustmentDecision` for that trainee, with "Download certificate" link
- Admin page for viewing and revoking (InstitutionalAdmin+)

**Effort:** M.

### Phase 2 exit criteria

- `star_reflection` has a non-colliding key
- An `EntrustmentDecision` can be issued by a ratified `CommitteeReview`, revoked with audit, and expires automatically
- Trainee can download a PDF STAR certificate
- Architecture tests verify `EntrustmentDecision` is not referenced from Web components except through DTOs

---

## Phase 3 — Faculty-development layer

**Moved ahead of analytics** on the strength of `EPA Book/critique.md` §2–§3 (Kogan 2009; Pelgrim 2011; Govaerts 2007, 2011). Rater leniency and supervisor-assessor role conflict mean rating data is structurally compromised until faculty calibration is addressed. Analytics (Phase 4) inherit the quality of this phase's work.

Three independently-shippable tasks.

### T036 — Assessor training status

**Scope:** extend `AssessorProfile` with `TrainingCompletedOn`, `CalibrationAttestedOn`, `LastCoachedOn`. Surface on the admin assessor-list view. Block acceptance of high-stakes assessment requests from untrained assessors (configurable per institution).

**Effort:** S.

### T038 — Assessor bias analytics

**Scope:** projection `AssessorRatingDistribution` — per assessor, distribution of ratings given (shape, extremity, mean vs peers within same speciality). Admin-only view. Not automated remediation — purely diagnostic.

Promoted ahead of T037 because it gives the data needed to target calibration sessions where they matter most.

**Effort:** M. Depends on T028 (needs the rating field — opt-in scope included). Meaningful signal requires enough institutions to have T028 enabled; otherwise the view operates on the single-rating default.

### T037 — Calibration event aggregate

**Scope:** new hard-coded aggregate `CalibrationSession` — chair selects a sample observation, panel of assessors rates it blind, consensus is recorded. Differences between individual and consensus ratings feed the `AssessorRatingDistribution` projection (T038) as a distinct evidence source.

Activity type cannot model this because it requires simultaneous blind multi-rater entry with reveal + consensus — a workflow the generic state machine does not express cleanly.

**Effort:** L. Can be deferred; T036 + T038 alone materially close §7 faculty-development gaps from the book.

### Phase 3 exit criteria

- Institutions can see who is calibrated and who isn't (T036)
- Rating-distribution diagnostics exist per assessor (T038)
- Optional: calibration sessions are recorded as first-class events (T037)
- Architecture tests stay green; no new Web→Domain references

---

## Phase 4 — Committee & analytics enhancements

Read-only views over existing domain state + new projection tables. Runs *after* Phase 3 so trajectory charts draw on data from assessors whose rating tendencies are known.

### T033 — Committee decision dissent + excellence capture

**Scope:** extend `CommitteeDecision` (T022) to capture:
- Structured dissenting opinion (member id, opinion, recorded timestamp) as a child collection
- An `Excellence` flag on the decision category enum — orthogonal to pass/fail, for programmes wanting to recognise above-expected performance (book §21)
- A "formative-only review" mode on `CommitteeReview` — allows a review to close without recording a binding decision (interim meetings, book §21)

**Effort:** M. Schema change to existing aggregate; handle with care for migration.

### T034 — Sampling adequacy dashboard

**Scope:** new projection `TraineeEpaSamplingView` — for each trainee+EPA:
- Count by source (direct obs / conversation / longitudinal / product)
- Count by distinct assessor
- Count by context tag (if the activity schema carries one)
- **Diversity flags, not just counts.** Per `EPA Book/critique.md` §6 (Moonen-van Loon 2013), absolute thresholds are not empirically fixed. The view surfaces: "single-rater concentration" (one assessor contributed > N% of observations), "context-narrow" (fewer than M distinct contexts), "single-source" (all evidence from one of the four sources).
- Configurable per EPA's CurriculumItem; defaults flag rather than block.

Rendered as a panel on `ReviewDetail.razor` (committee view) and on trainee's own EPA progress view.

**Effort:** M.

### T035 — Trajectory & cohort views

**Scope:**
- Per-trainee per-EPA trajectory: time-series chart of entrustment ratings over time (uses whichever rating is configured — single, prospective, or retrospective). Line chart with observation markers.
- Cohort comparison: anonymised percentile band overlay — "where this trainee sits relative to peers in the same SubSpeciality at the same point in training"
- "Stuck on EPA" flag: no new observation + level unchanged for ≥ N months (configurable per CurriculumItem)
- **Data-quality provenance on each chart.** Where the underlying ratings come from assessors with flagged distributions (T038), the chart shows a badge. Consumers should not read trajectory as uncontextualised truth.
- Chart rendering: prefer server-side SVG (Blazor-renderable) over a JS charting library — keeps the design-system contract intact and avoids adding a new dependency

**Effort:** L. Largest user-visible change in Phase 4.

### Phase 4 exit criteria

- Committee can see, for any trainee: sampling diversity, trajectory, cohort position — with assessor-data-quality context visible
- Dissent, excellence, and formative-only interim reviews are first-class in `CommitteeDecision`
- No new third-party charting dependencies added

---

## Phase 5 — Lower-priority fidelity items

Shippable piecemeal; each is self-contained. No ordering dependencies within the phase.

| Task (provisional) | Gap | Effort |
|---|---|---|
| T039 — EPA entity extensions (core/elective flag, risk summary, framework mappings, expiresAfter) | §1 #1, #6, #10, #85 | S |
| T040 — Nested EPA parent link | §1 #5 | S |
| T041 — Stage-indexed supervision expectations on `CurriculumItem` | §5 #49 | S |
| T042 — Self-assessment pairing (trainee-rated + supervisor-rated on same activity) | §8 #72 | M |
| T043 — Training-phase transition aggregate (phase-to-phase readiness gate) | §5 #51 | M |
| T044 — EQual rubric tooling for EPA definition quality audit | §1 #9 | M |
| T045 — Accreditor-specific export templates (HPCSA / ACGME / ABMS shapes) | §9 #91 | M, per-accreditor |
| T047 — Patient-outcome data source for EPA evidence | `critique.md` §7 (Asch 2009; Bansal 2016) | L |

Each becomes a task file only when a programme requests it.

**T047 note:** generic external-data-source connector per institution, mapped into `Activity`-shaped evidence records under a new seeded `outcome_metric` activity type. The book under-weights outcome-linked evidence; the literature (Asch 2009; Bansal 2016; Sirovich 2014) supports it as a differentiator for a mature WBA platform. Not urgent — slot in when a programme brings a registry to connect.

**T042 caveat:** Self-assessment pairing can either support or aggravate imposter-syndrome dynamics (LaDonna 2018). Implementation framing matters; the task file should specify opt-in per trainee, comparison-view never default-on.

---

## Sequencing rationale

- **Phase 1 first** because it's low effort and creates the schema-level scaffolding (rating pair, A RICH, R2C2 workflow) that later phases rely on. All Phase 1 additions ship opt-in so rollout is under institution control.
- **Phase 2 next** because the `EntrustmentDecision` aggregate + STAR rename is the single biggest fidelity gap (`EPA Book/evaluation.md` §2). Nothing downstream makes sense without it, and the naming collision gets cheaper to resolve the sooner it happens.
- **Phase 3 before Phase 4** — the reversal from the original plan. Rater leniency, supervisor-assessor role conflict, and ceiling effects in direct observation are documented (`EPA Book/critique.md` §2–§3). Analytics built on uncalibrated rating data mislead committees. T036 + T038 give institutions the tools to see their rating-data quality before the trajectory/cohort views amplify whatever noise is in it. T037 (calibration event aggregate) can defer.
- **Phase 4 last among the main phases** because its value is maximised when the data behind the charts has known provenance.
- **Phase 5 opportunistic** — slot tasks in as real user demand surfaces. T047 (patient-outcome connector) is the most strategically differentiating of the Phase 5 items.

## Out of scope for this plan

- Real-time in-situ observation (mobile live form filling)
- Multi-language UI
- Native mobile app
- Knowledge/skills exam integration (separate integration work)
- Video capture in assessments
- The T019-b…T019-g builder UX follow-ups — already tracked in `current_state.md`, orthogonal to book fidelity

## Open questions before starting T028

1. **Which rating does `CreditApplier` honour by default** when `entrustment_pair` is enabled — retrospective (what supervision was actually needed) or prospective (what supervision is recommended next)? The book implies prospective for summative decisions, retrospective for day-to-day progress. Confirm.
2. **STAR rename target** — `situation_task_action_reflection`? `reflection_star`? `reflective_note`? Picked key must not be reserved by the real STAR aggregate in T031.
3. **Which calibration-state to require before unblocking T035 cohort charts?** Options: require institution-level assessor-training threshold (e.g., > 50% assessors have `TrainingCompletedOn`), require per-assessor diagnostic data (T038 has N+ ratings before including in cohort), or ship charts unconditionally with provenance badges. `EPA Book/critique.md` §3 argues for a floor.
4. **Reference verification** — the Phase 3 re-sequencing and T046/T047 additions rest on citations in `EPA Book/critique.md`. User has committed to verifying against PubMed before the relevant tasks start. Track which citations have been verified as each Phase begins.
5. **Model override for heavier tasks** — per `MEMORY.md`, confirm the Opus/Sonnet recommendation when each task becomes the active one.

**Resolved:**
- ~~A RICH default on/off per institution?~~ → **Off by default** (opt-in). Rationale: `EPA Book/critique.md` §8 (Boyd 2018; Watling & Ginsburg 2019) on assessment burden, plus guiding principle #8.

## Active task

None of these are active yet. The roadmap is a draft for review. When approved, the next step is to write `Rewrite/Tasks/T028-*.md` through the first block of approved Phase 1 tasks (T028, T029, T046), then update `current_state.md` to point at the first active one.
