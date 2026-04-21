# Domain — EPAs, Work-Based Assessment, and the Wombat Model

> **⚠ Amended by `CUSTOMIZATION.md`.** After drafting this document, a requirement surfaced that admins must be able to add new activity types (research outputs, teaching, QI, course attendance, anything) without developer involvement. That requirement replaces the "concrete aggregate per workflow" assumption in several sections below. Specifically, `Assessment` and `StarReflection` are no longer domain aggregates — they are seeded instances of `ActivityType` built on top of the generic activity platform. Read `CUSTOMIZATION.md` in full before executing any Phase 2 task. The EPA / Curriculum / CurriculumItem / Role / Invitation content below is still current.

This document exists because Wombat's current model was written while its author was still learning competency-based medical education. A few concepts are mis-named or structurally tangled. This document defines the intended model for the rewrite. It is the source of truth; if the code and this document disagree, fix the code.

## Concepts

### Entrustable Professional Activity (EPA)

An EPA is a unit of professional practice that a trainee can be **entrusted** to perform unsupervised once they are competent. Examples in medicine: "admit a patient to a general medical ward", "perform a lumbar puncture", "lead a family discussion about end-of-life care". An EPA is *not* a test, and it is *not* a skill in isolation; it is a discrete, observable, meaningful chunk of clinical work that integrates many skills and knowledge areas.

Each EPA has:

- A **title** and **description**.
- A **context** (setting, limitations — e.g. "in a teaching hospital, non-complex presentations").
- **Required knowledge, skills and attitudes** (prose).
- **Assessment methods** — which work-based assessment instruments are valid evidence for this EPA.
- An **entrustment scale** (usually 1–5, occasionally 1–4 or 1–6). The numbers mean, roughly:
  1. Observe only. Not ready to do it.
  2. Does with direct supervision at the elbow.
  3. Does with supervision in the room or nearby.
  4. Does independently; supervisor reviews afterwards.
  5. Does independently and can supervise others.

In the current Wombat model, EPA is directly linked to `SubSpeciality` and has a `Forms` collection. That is not *wrong*, but the scale is modelled as an `Option` stored on `EPACurriculum`, which is confusing. In the rewrite, scale is a property of the assessment instrument (form) and/or an override on the curriculum item, not a free-floating row.

### Curriculum

A curriculum is the programme that a trainee is working through. It belongs to a **SubSpeciality** (or more generally a **Programme** — a thin layer above SubSpeciality we may add later). A curriculum lists the EPAs that must be demonstrated and, for each EPA, the **requirement**: how many assessments, over what period, at what target entrustment level before sign-off.

**The current Wombat `EPACurriculum` entity is actually a curriculum *line item*, not a curriculum.** It has `EPAId`, `NumberOfMonths`, `EPAScaleId`, and nothing that groups lines into a programme. In the rewrite:

- `Curriculum` — aggregate root. Belongs to `SubSpeciality`. Has metadata (name, version, effective date, owning SpecialityAdmin).
- `CurriculumItem` — child of `Curriculum`. Contains `EpaId`, `RequiredCount`, `MinimumEntrustmentLevel`, `WindowMonths`, and `Weight` (optional). This is the thing currently called `EPACurriculum` in Wombat.

Rename with intent: `EPACurriculum` → `CurriculumItem`, and introduce the missing `Curriculum` aggregate.

### Work-Based Assessment (WBA)

A WBA is an observation of a trainee doing an EPA (or part of one), scored against a rubric. The current Wombat code uses:

- `AssessmentForm` — the rubric template (option criteria, which EPAs it applies to).
- `LoggedAssessment` — one observation. Trainee + Assessor + EPA + Form + Date + responses.
- `AssessmentRequest` — the workflow that gets a `LoggedAssessment` into existence (trainee asks, assessor accepts, assessor fills in).
- `AssessmentEvent` — a history record.

In the rewrite, this cleans up to:

- `AssessmentForm` — unchanged conceptually; belongs to Institution/Speciality/SubSpeciality; defines option criteria; references which EPAs are valid for this form.
- `Assessment` (was `LoggedAssessment`) — the aggregate. Exists in one of: `Requested`, `Accepted`, `Declined`, `Cancelled`, `Completed`. Has a collection of `CriterionResponse`. State transitions are methods on the aggregate, not service methods.
- The workflow aggregate absorbs `AssessmentRequest` and `AssessmentEvent`. The state machine lives in `Assessment` itself; events are produced as a by-product of state transitions and stored for audit.

### STAR reflection (legacy terminology)

Historically Wombat used STAR to mean a **written reflection** the trainee submits against an EPA, using the Situation-Task-Action-Result framing. After T028, that activity type was renamed to `reflective_note` and the STAR acronym was reclaimed for the formal entrustment artefact defined below. The reflective-note activity is still structured around the Situation-Task-Action-Result frame and follows the same `Draft` → `Submitted` → `Approved` / `Declined` lifecycle; it is just no longer called a STAR anywhere in code or UI.

The original collapse of `STARApplication` / `STARApplicationForm` / `STARItem` / `STARResponse` now lands inside the activity platform (schema-driven form + workflow) rather than as a dedicated aggregate. See `CUSTOMIZATION.md` and the `reflective_note` seed directory for the current shape.

### Entrustment decision (STAR — Statement of Awarded Responsibility)

After the reflective-note rename in T028, **STAR** in Wombat now refers to the **Statement of Awarded Responsibility**: the formal, per-trainee-per-EPA authorisation record that a committee issues on the back of a ratified review. Introduced in T029.

- `EntrustmentDecision` — aggregate root. Immutable after issue. Properties: `TraineeUserId`, `EpaId`, `AuthorisedLevelId` (references `EntrustmentLevel` — the 1–5 scale), `IssuedOn` (`DateOnly`), `ExpiresOn?` (`DateOnly?`), `IssuedByCommitteeReviewId`, `IssuedByChairUserId`, `Rationale`, `Status` (`Active`, `Expired`, `Revoked`, `Superseded`), revocation tuple (`RevokedOn`, `RevokedByUserId`, `RevocationReason`), `SupersededByDecisionId?`.
- `EntrustmentEvidenceLink` — child. Snapshot-style pointer to the activities, MSF campaigns, or committee reviews that grounded the decision. Parallel to `CommitteeEvidence` on `CommitteeReview`.
- Domain methods: `static Issue(...)`, `Revoke(reason, actor, utcNow)`, `MarkExpired(utcNow)`, `SupersedeBy(newDecisionId)`. `Amend()` throws — mirrors `CommitteeDecision`.
- `PendingEntrustmentDecision` — chair-staging state attached to a `CommitteeReview`. Holds the intended decision (EPA, level, issue/expiry, rationale, evidence links as jsonb) until the review is ratified. On ratification, each pending row is materialised atomically into an `EntrustmentDecision` with `IssuedByCommitteeReviewId` pointing at the parent review. Pending rows are cleared as part of the same transaction.

**Why this is hard-coded.** Entrustment decisions carry regulatory and medico-legal weight: immutable once issued, appealable through committee machinery, defensible to a regulator, produced by a named chair on behalf of a named panel. They are not an activity type — an admin must not be able to reshape them in the builder. Same reasoning as `CommitteeDecision`.

**Auto-supersession.** When a new `Active` decision is issued for the same `(TraineeUserId, EpaId)` pair, any prior `Active` decision for that pair is automatically marked `Superseded` with `SupersededByDecisionId` pointing at the new row. Only one `Active` decision per `(trainee, EPA)` exists at a time.

**Expiry.** A daily background job (`EntrustmentDecisionExpiryJob`, 03:30 UTC) flips `Active` decisions past `ExpiresOn` to `Expired`, emails the trainee, and sends a 30-day expiry-reminder pass tracked via `LastExpiryReminderSentOn` to avoid repeat spam.

The PDF certificate ("STAR certificate") is produced by T030.

## Role hierarchy

Wombat currently declares nine roles. In the rewrite, keep all nine but document what each one actually does. These become both Identity roles and claim policies.

| Role | Scope | Can do |
|---|---|---|
| **Administrator** | Global | Everything. Single superuser, seeded on first run. |
| **InstitutionalAdmin** | One institution | Manage users within their institution; approve Specialities and SubSpecialities for that institution. |
| **SpecialityAdmin** | One speciality within an institution | Define curricula, approve assessment forms, approve STAR reflections for the speciality. |
| **SubSpecialityAdmin** | One sub-speciality | Like SpecialityAdmin, scoped one level down. |
| **Coordinator** | An institution or speciality | Operational role: invite users, reassign assessments, nudge stalled requests. No curriculum editing. |
| **CommitteeMember** | A speciality | Read-only oversight plus final sign-off on trainee progression. Cannot edit forms or curricula. |
| **Assessor** | Their own assessments | Accept/decline/complete assessment requests addressed to them. |
| **Trainee** | Their own record | Request assessments, submit STAR reflections, view their own progress. |
| **PendingTrainee** | None | Transitional role between accepting an invitation and being admitted to a programme. Can log in but can only view onboarding screens. |

Role membership is **not** hierarchical — an InstitutionalAdmin is not automatically a SpecialityAdmin. If someone needs both, assign both. Policies use claim-based checks (`HasRole("SpecialityAdmin")` + institution claim + speciality claim) rather than role-hierarchy tricks.

Each user carries claims for their scope:

- `institution_id` — one per user (users belong to one institution at a time).
- `speciality_id` — zero or more (SpecialityAdmins and Trainees).
- `sub_speciality_id` — zero or more.

These claims are populated at login from the user's profile data, not stored loose in Identity. Same approach as ClinicAssist's clinician/clinic claim pattern.

## Invitation flow

The current Wombat has a `RegistrationInvitation` entity. Keep the flow but simplify the state:

1. An Admin or Coordinator issues an invitation for a specific email, assigning target role, institution, speciality, sub-speciality.
2. An invitation row is created with a random token and an expiry (default 14 days).
3. An email is sent containing a registration link with the token in the query string.
4. The recipient clicks the link. If the token is valid and unused, they land on a registration form pre-populated from the invitation.
5. On successful registration, the new user is assigned the invitation's target role, scope claims are written, the invitation is marked used, and they are redirected to their role-appropriate landing page.
6. Trainees land as `PendingTrainee` until a SpecialityAdmin admits them to a curriculum, at which point they are promoted to `Trainee`.

Invitations can be revoked (set `RevokedOn`) and re-issued (a new row with a new token; old row stays for audit).

## Corrections the rewrite must make

These are the misunderstandings to actively fix, not just preserve:

1. **"Curriculum" is not a single EPA requirement.** A curriculum is the programme; a CurriculumItem is one row in that programme. Rename and restructure.
2. **Entrustment scale belongs to the form, not to the curriculum item.** A CurriculumItem can override the minimum required level, but the scale definition lives with the assessment instrument.
3. **STAR reflections are not applications.** Rename.
4. **The assessment "workflow" is a state machine, not a separate Event table.** Model it as methods on the aggregate, generate audit records as a by-product.
5. **Roles are not hierarchical.** No implicit inheritance. Check roles explicitly.
6. **PendingTrainee is not a normal role.** It is a waiting-room role; the only pages it can hit are onboarding.
7. **`DateTime` for calendar-ish things.** Use `DateOnly` for assessment dates, expiry dates, curriculum effective dates. Keep `DateTime` (UTC) for timestamps only.
8. **AutoMapper is gone.** CQRS handlers project directly with LINQ `Select(...)`. Faster, simpler, less magic. Same as ClinicAssist.
9. **Repository pattern is gone.** Handlers use `DbContext` directly. The CQRS handler *is* the repository abstraction.
10. **MVC controllers are gone.** Blazor Interactive Server pages call MediatR directly via `IScopedSender`. A small API project exists for webhooks/integrations only.

## Addendum — the Activity platform

The post-evaluation pivot introduces a generic `Activity` aggregate that replaces several concrete aggregates earlier in this document. Full details in `CUSTOMIZATION.md`. Short version for anyone skimming:

- `ActivityType` — admin-defined type (Mini-CEX, DOPS, STAR Reflection, Research Output, Teaching Session, QI Project, Journal Club, Course Attendance, …). Stores a form schema, a workflow, and credit rules — all as jsonb.
- `Activity` — one instance. Has `Data` (jsonb, shaped by the schema), `CurrentState`, `SubjectUserId`, and optional links to EPA / CurriculumItem.
- `ActivityTransition` — audit row per state change; holds a snapshot of `Data` at the moment of transition.
- `ActivityPermissionRule` — who can fire which transition on which activity type.

The concrete "Assessment" aggregate described earlier in this document is replaced by activity types `mini_cex`, `dops`, `cbd`, `acat` seeded in T020. The "StarReflection" aggregate is replaced by the activity type `reflective_note` (renamed from `star_reflection` in T028 to free the STAR acronym for the formal entrustment artefact). The domain method `Assessment.Complete(...)` becomes a transition named `complete` in the activity type's workflow JSON, executed by the generic `TransitionActivityCommand` handler.

What is **not** replaced: identity, roles, scope claims, institution hierarchy, curriculum structure, invitation flow, profile data. Those remain concrete aggregates. Also not replaced: MSF (anonymity requirement), committee decisions (legal weight), audit log (append-only requirement). See the "What stays in code" section of `CUSTOMIZATION.md`.

This is a deliberate split. The platform is code; the content is data. Trying to make *everything* data turns Wombat into a clinical-flavoured Airtable, which is neither useful nor maintainable. Trying to make *everything* code loses the no-code intent. The line between the two must be respected in both directions.

## Terms glossary (for future sessions)

- **EPA** — Entrustable Professional Activity.
- **WBA** — Work-Based Assessment. An observation of a trainee doing an EPA, scored on a form.
- **Curriculum** — The training programme; a collection of EPAs with requirements.
- **CurriculumItem** — One requirement row within a curriculum.
- **STAR** — Situation-Task-Action-Result. A structured reflection framework.
- **Entrustment level** — How much supervision is needed; 1 (observe only) to 5 (can supervise others).
- **Form** — Assessment form / rubric.
- **CriterionResponse** — One answer on a filled-in form.
