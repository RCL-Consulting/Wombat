# Practical plan — post-rewrite enhancements

The authoritative forward plan for Wombat. Supersedes `Rewrite/book-fidelity-plan.md`, which remains for reference only.

**Audience:** a hospital running a specialist training programme. They need a working WBA system that produces defensible evidence and regulator-ready paperwork. They are not doing training research.

## Non-goals

- Academic fidelity to the EPA textbook where the cost does not reduce programme or audit burden
- Psychometric instrumentation
- Research-grade bias, calibration, or outcome-linkage tools
- Opt-in toggles and configuration surfaces beyond what daily operations require

## What "done" means

- A trainee's portfolio answers every question an accreditor or medico-legal reviewer might ask about their progression
- A committee can deliberate and record a decision with the evidence in one place
- Administrators run the system without calling a developer
- No feature exists that most users will route around

Companion docs:
- `EPA Book/evaluation.md` — original 92-point fidelity scorecard (reference)
- `EPA Book/critique.md` — literature-backed reasons we are *not* building some book items (use when a programme director asks "why not X?")
- `Rewrite/PLAN.md` — the master T001–T027 plan (complete)
- `Rewrite/current_state.md` — live session handoff

---

## Block 1 — Defensible entrustment decisions (~2 weeks)

Without this block the hospital has no formal record that trainee X is authorised to do Y. Highest regulatory and medico-legal value.

### T028 — Rename `star_reflection` activity type

The seeded `star_reflection` key collides with STAR (Statement of Awarded Responsibility) — the formal authorisation artefact Block 1 introduces. Rename to `reflective_note`.

- Data migration on `ActivityType.Key` and display labels
- Update references in `ActivityPermissionRule`
- Existing activities stay intact; key rename only

**Effort:** ½ day. Blocks T029.

### T029 — `EntrustmentDecision` aggregate

The core artefact of this block.

Domain (`Wombat.Domain/EntrustmentDecisions/`):
- `EntrustmentDecision` — aggregate root. TraineeUserId, EpaId, AuthorisedLevel (references `EntrustmentLevel`), IssuedOn (`DateOnly`), ExpiresOn (`DateOnly?`), IssuedByCommitteeReviewId, IssuedByChairUserId, Rationale, Status (`Active`, `Expired`, `Revoked`, `Superseded`), RevokedOn?, RevokedByUserId?, RevocationReason?
- `EntrustmentEvidenceLink` — child. Id-pointers to the activities and MSF campaigns that grounded the decision. Snapshot-style, same pattern as `CommitteeEvidence` in T022.
- Domain methods: `static Issue(...)`, `Revoke(reason, actor)`, `MarkExpired()`, `Supersede(byNewDecisionId)`. No direct setters. Immutable after `Issue`.

Application (`Wombat.Application/Features/EntrustmentDecisions/`):
- Commands: `IssueEntrustmentDecisionCommand`, `RevokeEntrustmentDecisionCommand`
- Queries: `GetActiveDecisionsForTrainee`, `GetDecisionHistoryForEpa`, `ListExpiringDecisions`

Infrastructure:
- EF config, hand-written migration, `Designer.cs`
- `EntrustmentDecisionExpiryJob` — daily; flips `ExpiresOn < today` to `Status=Expired`, emails trainee + coordinator

Committee integration: `RecordCommitteeDecisionCommand` (T022) gains an optional `entrustmentDecisions[]` parameter. On ratification, decisions issue atomically.

**Effort:** L (5 days).

### T030 — STAR certificate PDF + trainee "My authorisations" panel

- `EntrustmentCertificatePdfService` (QuestPDF), parallel to `PortfolioPdfService` from T023
- Certificate content: trainee name, EPA title, authorised level with scale anchor, issue date, expiry, issuing panel, chair signature block, evidence summary, SHA256 integrity hash
- Trainee dashboard: "My authorisations" panel — one card per active decision with "Download certificate" link
- Admin page for viewing and revoking (InstitutionalAdmin+)

**Effort:** M (2 days).

### Block 1 exit criteria

- A committee can ratify a review and issue entrustment decisions in the same transaction
- A trainee can download a STAR certificate PDF for any active decision
- Revocations are audited and expire correctly

---

## Block 2 — Committee decision practical improvements (~3 days)

Small extensions to the existing T022 aggregates that real committees need.

### T031 — Formative-only committee review mode

- Add `IsFormative` boolean to `CommitteeReview`
- Formative reviews close without producing a binding `CommitteeDecision` — used for interim progress check-ins
- Small UI change on review scheduling; ratification step skipped for formative reviews

**Effort:** ½–1 day.

### T032 — Sampling-concentration warning on review detail

A single query, not a projection stack. For the trainee in question, per EPA:
- "One assessor contributed > 50% of ratings" — warn
- "All ratings from one of the four sources" — warn
- "Fewer than 3 distinct assessors across all EPA evidence" — warn

Rendered as a dismissible warning panel on `ReviewDetail.razor`. Committee acknowledges in their decision rationale if relevant.

No projection table, no new aggregate. One query, one component.

**Effort:** 1–2 days.

### Block 2 exit criteria

- Committees can run formative check-ins without producing binding decisions
- Reviews show sampling-concentration warnings where they apply
- No new projection tables

---

## Block 3 — Trainee-visible progress (~3 days)

What trainees, supervisors, and committees actually want to see on-screen.

### T033 — Per-trainee per-EPA trajectory chart

- Server-side SVG (no new JS dependency; matches the `Rewrite/DESIGN.md` contract)
- Single line, dot per observation, rating on Y axis, date on X axis
- Rendered on the trainee's EPA detail view and on `ReviewDetail.razor`
- No cohort comparison, no percentile bands, no "stuck on EPA" algorithm. Committees read the chart themselves.

**Effort:** 2 days.

### T034 — EPA core/elective flag + stage-indexed supervision levels

- `Epa.Category` enum: `Core`, `Elective`. Admin edits on EPA form.
- `CurriculumItem.MinimumLevelByStage` optional column (jsonb, keyed by training year — e.g., `{"1": 2, "2": 3, "3": 4}`) overriding the flat `MinimumLevel` where present.
- Trainee progress view respects the stage-indexed minimum.

**Effort:** 1 day.

### Block 3 exit criteria

- Every EPA progress view shows a trajectory chart
- Programmes can mark EPAs as core/elective and set stage-indexed level expectations

---

## Block 4 — Audit-ready basics (~2–3 days)

The items an accreditor asks for which Wombat does not currently document.

### T035 — Assessor training status field

- Add `TrainingCompletedOn` (`DateOnly?`) to `AssessorProfile`
- Surfaced on the admin assessor-list view
- No blocking behaviour — just a visible record. Answers "do your assessors have training?" with a list.

**Effort:** ½ day.

### T036 — Accreditor-specific export template

One export template for the accreditor the hospital actually works with (HPCSA / CMSA / RCS / etc. — confirmed before starting).

- Variant of the T023 portfolio PDF with sections shaped to the accreditor's expected layout
- Admin page to trigger it for a trainee

**Effort:** 2 days, contingent on the accreditor's format spec being available.

### Block 4 exit criteria

- Every assessor record shows training status
- Programme director can export a portfolio in the accreditor's expected shape

---

## Sequencing

Blocks ship linearly. Each block is small enough to review, test, and ship before starting the next. Total estimate: **3–4 working weeks** for a single developer, including migration and review overhead.

| Block | Dependency | Estimate |
|---|---|---|
| 1 — Entrustment decisions | None (extends T022) | 2 weeks |
| 2 — Committee improvements | T022 exists (done) | 3 days |
| 3 — Trainee progress views | None | 3 days |
| 4 — Audit-ready basics | T023 portfolio PDF (done) | 2–3 days |

Blocks 2, 3, and 4 have no hard dependencies on Block 1 and could ship out of order if Block 1 hits a snag — but Block 1's regulatory value dwarfs the others, so it goes first unless a programme-specific pressure flips priority.

---

## Deferred indefinitely

Items considered and explicitly not planned. Kept here so "why didn't we build X?" has an answer.

| Item | Reason not built |
|---|---|
| Prospective / retrospective rating pair | Two correlated ratings; marginal signal; real friction. Pick one rating mode per institution. |
| A RICH rubric (Agency, Reliability, Integrity, Capability, Humility) | Needs assessor training to produce better data than a narrative prompt. Narrative already carries this. |
| R2C2 feedback workflow extension (reaction / coaching states) | Users skip through extra workflow states. One "trainee response" field in the existing workflow solves this if needed. |
| Calibration session aggregate | Calibration happens in faculty meetings, not in software. |
| Rating-distribution bias dashboard | Admins will not act on a diagnostic with no framed remedy — creates liability without a fix. |
| Cohort percentile views / peer comparison | Committees want "is this trainee ready", not peer ranking. |
| "Stuck on EPA" algorithmic flag | Visible in the trajectory chart; no need for an extra alert. |
| Nested EPAs (parent-child EPA links) | Over-modelled; rarely used in practice. Programmes do this with naming conventions. |
| Self-assessment vs. supervisor-assessment pairing | Risks imposter-syndrome dynamics; programmes have not asked for it. |
| Training-phase transition aggregate | A `CurriculumItem` per phase, combined with stage-indexed supervision levels (T034), covers this. |
| EQual rubric tooling for EPA definition audit | Academic audit; programmes can do this in a document review. |
| Patient-outcome registry connector | Research project; not a training-programme feature. |
| Structured dissent logging on `CommitteeDecision` | Committees do not dissent on paper. Rationale field carries what gets written down. |
| Excellence flag on decision category | No programme has asked for it. |
| Validity-framework documentation in UI | Nobody reads it. Belongs in user guide at most. |

## Known compromises (the defensibility answer)

When an accreditor or an external reviewer asks "why does your system not do X?", these are the pre-stated answers. Citations in `EPA Book/critique.md`.

- **One rating per observation, not a prospective/retrospective pair.** Chosen for assessor cognitive load vs. information gain.
- **Sampling warnings, not enforced thresholds.** The literature (Moonen-van Loon 2013) does not settle on fixed thresholds; context determines adequacy.
- **No structured trustworthiness (A RICH) capture.** Narrative feedback carries this where assessors have context; structured capture without training produces noise.
- **No calibration or rater-bias analytics.** Diagnostic data without a matched intervention plan invites liability without remedy.
- **No cohort-percentile comparison.** Decisions are individual; relative ranking is not the committee's question.

---

## Progress

| Task | Status | Commit |
|---|---|---|
| T028 — rename `star_reflection` → `reflective_note` | ✅ done | `dc506d1` |
| T029 — `EntrustmentDecision` aggregate | ✅ done | `91ff841` |
| T030 — STAR certificate PDF + trainee authorisations panel | ✅ done | `10f7e55` |
| T031 — Formative-only committee review mode | ✅ done | — |
| T032 — Sampling-concentration warning | active | — |
| T033 — Per-trainee trajectory chart | pending | — |
| T034 — EPA core/elective + stage-indexed supervision levels | pending | — |
| T035 — Assessor training status field | pending | — |
| T036 — Accreditor-specific export template | pending | — |

## Active task

**T032 — Sampling-concentration warning on review detail.** Model: Sonnet. Live state in `Rewrite/current_state.md`.

Commit after every completed task, per `Rewrite/WORKFLOW.md`.
