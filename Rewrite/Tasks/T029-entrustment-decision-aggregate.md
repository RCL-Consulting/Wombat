# T029 — `EntrustmentDecision` aggregate

**Plan:** `Rewrite/practical-plan.md` — Block 1, core task
**Depends on:** T022 (committee decisions), T028 (`star_reflection` rename)
**Blocks:** T030 (STAR certificate PDF + trainee authorisations panel)
**Model:** Opus

## Goal

Introduce the formal Statement of Awarded Responsibility (STAR): a per-trainee,
per-EPA summative entrustment decision that records *what* the trainee is
authorised to do, *at what supervision level*, *from when*, *until when*, *on
what evidence*, and *by whose authority*. This is the regulatory record Wombat
currently lacks — committee decisions (T022) record progression outcomes; they
do not record authorisation artefacts.

## Why hard-coded (not an activity type)

Same reasoning as T022:

- Immutable after issue. Edits are not possible; only supersession or revocation.
- Regulatory weight. Produced by a named chair on behalf of a named committee review.
- Evidence-linked. Must reference the activities and MSF campaigns that
  grounded the decision, snapshot-style so later activity edits cannot rewrite
  history.
- Not customisable. An institution does not get to redesign the shape of a STAR.

Letting an admin reshape this in the activity builder would undermine all of it.

## What to do

### 1. Domain — `src/Wombat.Domain/EntrustmentDecisions/`

Create a new subdirectory parallel to `CommitteeDecisions/`. Files:

- **`EntrustmentDecision.cs`** — aggregate root. Private parameterless ctor
  (EF), static factory `Issue(...)`, no public setters. Properties:
  - `int Id`
  - `string TraineeUserId`
  - `int EpaId`
  - `int AuthorisedLevelId` (FK to `EntrustmentLevel`)
  - `DateOnly IssuedOn`
  - `DateOnly? ExpiresOn`
  - `int IssuedByCommitteeReviewId` (FK to `CommitteeReview`)
  - `string IssuedByChairUserId`
  - `string Rationale`
  - `EntrustmentDecisionStatus Status` (enum — `Active`, `Expired`, `Revoked`, `Superseded`)
  - `DateTime? RevokedOn`
  - `string? RevokedByUserId`
  - `string? RevocationReason`
  - `int? SupersededByDecisionId` (nullable self-reference)
  - `ICollection<EntrustmentEvidenceLink> EvidenceLinks` (private setter)
  - Nav: `Epa`, `AuthorisedLevel`, `IssuedByCommitteeReview`

  Domain methods:
  - `static Issue(traineeUserId, epaId, authorisedLevelId, issuedOn, expiresOn, committeeReviewId, chairUserId, rationale, evidenceLinks, utcNow)` — validates
    all required fields non-empty/positive, `ExpiresOn` (if supplied) >
    `IssuedOn`, rationale non-empty. Returns a new instance with
    `Status=Active`.
  - `Revoke(reason, actorUserId, utcNow)` — only callable when `Status=Active`.
    Sets `Status=Revoked`, captures `RevokedOn`/`RevokedByUserId`/`RevocationReason`.
  - `MarkExpired(utcNow)` — only callable when `Status=Active` and
    `ExpiresOn.HasValue && ExpiresOn.Value < DateOnly.FromDateTime(utcNow)`.
    Sets `Status=Expired`.
  - `SupersedeBy(newDecisionId)` — only callable when `Status=Active`. Sets
    `Status=Superseded`, records pointer.
  - `Amend(...)` — throws `InvalidOperationException("Entrustment decisions are immutable. Revoke and reissue instead.")` (mirrors `CommitteeDecision.Amend`).

- **`EntrustmentDecisionStatus.cs`** — enum (`Active=1`, `Expired=2`, `Revoked=3`, `Superseded=4`).

- **`EntrustmentEvidenceLink.cs`** — child. Mirrors `CommitteeEvidence`:
  - `int Id`
  - `int DecisionId`
  - `EntrustmentEvidenceSourceType SourceType`
  - `int? ActivityId`, `int? MsfCampaignId`, `int? CommitteeReviewId` (only one set per row)
  - `string SourceLabel`
  - `string Summary`
  - `DateTime? SourceRecordedOn`
  - Nav: `Decision`

- **`EntrustmentEvidenceSourceType.cs`** — enum (`Activity=1`, `MsfCampaign=2`, `CommitteeReview=3`). Reuse naming from `CommitteeEvidenceSourceType` but keep the enum types separate — they describe different aggregates.

### 2. Application — `src/Wombat.Application/Features/EntrustmentDecisions/`

Feature folder parallel to `Features/CommitteeDecisions/`. Files:

- **Commands**
  - `IssueEntrustmentDecisionCommand` — issue a single decision. Parameters match
    the domain `Issue` call plus `ClaimsPrincipal`. Authorisation: chair of the
    referenced committee review's panel. (Reuse
    `CommitteeDecisionAuthorization.DemandChairAccess` — it already resolves
    chair membership from a panel.) On issue, if a prior `Active` decision
    exists for the same `(TraineeUserId, EpaId)`, call `SupersedeBy` on it
    atomically.
  - `RevokeEntrustmentDecisionCommand` — revoke an active decision.
    Authorisation: InstitutionalAdmin for the trainee's institution, or the
    chair of the issuing committee review's panel. Requires a `Reason`.
- **Queries**
  - `GetActiveDecisionsForTrainee(traineeUserId)` — all `Status=Active`
    decisions, ordered by EPA.
  - `GetDecisionHistoryForEpa(traineeUserId, epaId)` — full chronological
    history across all statuses. Used by T030's admin panel.
  - `ListExpiringDecisions(withinDays)` — `Active` decisions whose `ExpiresOn`
    falls within the window. Used by the expiry job and by coordinator
    dashboards in later blocks.
- **DTOs / mappings** — `EntrustmentDecisionDto`, `EntrustmentEvidenceLinkDto`,
  extension methods `ToDto(this EntrustmentDecision)`. Never return the entity
  to the Web layer.
- **Validators** — FluentValidation, one per command, living alongside.
- **Authorisation helper** —
  `Wombat.Application/Features/EntrustmentDecisions/EntrustmentDecisionAuthorization.cs`.
  Chair demand reuses panel-chair logic; admin demand reuses existing
  `IAuthorizationService` / role checks used elsewhere in the codebase
  (grep `IsInRole("InstitutionalAdmin")` for precedents).

### 3. Committee integration

`RecordCommitteeDecisionCommand` (T022) gains an optional
`EntrustmentDecisions` collection parameter. Shape (application DTO, not the
domain type):

```csharp
public sealed record PendingEntrustmentDecision(
    int EpaId,
    int AuthorisedLevelId,
    DateOnly IssuedOn,
    DateOnly? ExpiresOn,
    string Rationale,
    IReadOnlyCollection<PendingEvidenceLink> EvidenceLinks);
```

Atomicity rules:

- Pending decisions are **not** persisted when the decision is recorded. They
  attach to the review as staging state. (Simplest: a transient `jsonb` column
  on `CommitteeReview` or — cleaner — a new `PendingEntrustmentDecision` child
  table that is cleared on ratification.)
- On `RatifyCommitteeDecisionCommand`, the handler:
  1. Calls `review.Ratify(...)`.
  2. For each pending decision, calls
     `EntrustmentDecision.Issue(..., committeeReviewId: review.Id, chairUserId: principalUserId, ...)`.
  3. For each issued decision, supersedes any prior `Active` decision for the
     same `(TraineeUserId, EpaId)`.
  4. Persists everything in a single `SaveChangesAsync`.
- If ratification fails validation, nothing is issued.

Decide staging mechanism during implementation — prefer the child table because
it stays queryable and keeps the aggregate relational. Document the choice in
the task implementation note before committing.

### 4. Infrastructure

- **EF configuration** — new files in
  `src/Wombat.Infrastructure/Persistence/Configurations/EntrustmentDecisions/`:
  - `EntrustmentDecisionConfiguration.cs`
  - `EntrustmentEvidenceLinkConfiguration.cs`
  - If you add a pending-decisions staging table, its configuration too.

  Conventions (match existing configs):
  - Explicit table names (`EntrustmentDecisions`, `EntrustmentEvidenceLinks`).
  - Max-length strings: `Rationale` 4000, `RevocationReason` 1000,
    `SourceLabel` 200, `Summary` 2000.
  - Indexes: `(TraineeUserId, EpaId, Status)` composite for active-lookup; a
    separate index on `(Status, ExpiresOn)` for the expiry job.
  - FK behaviour: `Restrict` on `EntrustmentLevel`, `Restrict` on
    `CommitteeReview` — never cascade-delete an authorisation.

- **Migration** — hand-written under
  `src/Wombat.Infrastructure/Persistence/Migrations/`.
  Name: `<timestamp>_EntrustmentDecisions.cs`. Timestamp convention: follow
  prior migrations (`YYYYMMDDHHMMSS`). **Must include the `.Designer.cs` file**
  — see CLAUDE.md; without it `MigrateAsync` silently skips. Update
  `ApplicationDbContextModelSnapshot.cs` to reflect the new tables.

- **Daily expiry job** — `src/Wombat.Infrastructure/Scheduling/Jobs/`:
  - `EntrustmentDecisionExpiryJob.cs` implementing `IScheduledJob`.
    - `Key = "entrustment-decision-expiry"`
    - `CronExpression = "30 3 * * *"` (daily 03:30 UTC — off-peak, no collision
      with existing 08:00 nudges; verify none of the existing jobs in
      `Scheduling/Jobs/` already claim 03:30)
    - Fetches all `Status=Active` decisions with `ExpiresOn < today`,
      calls `MarkExpired`, persists.
    - Emails trainee + coordinator for each. Template:
      `Wombat.Application/Common/Email/Templates/EntrustmentDecisionExpiredEmail.cs`
      — structured like `MsfExpiryReminderEmail` (pure factory returning an
      `EmailMessage`).
    - Also emits a separate "expiring within 30 days" reminder pass in the
      same job run (query `ListExpiringDecisions(30)` and send a
      non-terminal reminder — one reminder per decision, tracked via a new
      `LastExpiryReminderSentOn` nullable column on
      `EntrustmentDecision` to avoid repeat spam).
  - Register in the scheduled-job DI bootstrap
    (`ScheduledJobServiceCollectionExtensions.cs`) alongside the others.

### 5. Blazor surfaces (minimal — T030 carries the real UI)

T030 is where trainee-facing cards and certificates land. T029 only needs:

- Committee chair flow: `ReviewDetail.razor` gets an "Entrustment decisions to
  issue on ratification" panel — add, edit, remove pending decisions. One row
  per pending decision showing EPA, level, issue date, expiry, evidence link
  count. Pending decisions live only until ratification.
- Nothing else. No trainee panel, no certificate, no admin list — all deferred
  to T030.

Keep UI restrained. All styling via `app.css` tokens; no Bootstrap classes, no
`<i class="bi bi-*">`, no inline `<style>` blocks (per CLAUDE.md /
`Rewrite/DESIGN.md`).

### 6. Tests

- **`tests/Wombat.Domain.Tests/EntrustmentDecisions/`**
  - `EntrustmentDecisionTests.Issue_*`: validation rules (null/empty/zero
    parameters throw; ExpiresOn <= IssuedOn throws; happy path returns
    `Status=Active`).
  - `EntrustmentDecisionTests.Revoke_*`: only `Active` can revoke; `Reason`
    required; status transitions; revoking twice throws.
  - `EntrustmentDecisionTests.MarkExpired_*`: only `Active` with past
    `ExpiresOn`; null `ExpiresOn` cannot expire.
  - `EntrustmentDecisionTests.SupersedeBy_*`: only `Active` can be superseded;
    captures pointer.
  - `EntrustmentDecisionTests.Amend_Throws`.

- **`tests/Wombat.Application.Tests/Features/EntrustmentDecisions/`**
  - Handler tests for `IssueEntrustmentDecisionCommand`: chair-only
    authorisation; auto-supersession of prior active; rejects issuing when the
    review is not ratified (or not `InProgress`/`Decided` per the chosen
    integration point — document the decision); evidence links persist.
  - `RevokeEntrustmentDecisionCommand`: admin or issuing-chair only; rationale
    required; non-active rejects.
  - Query handlers return DTOs, include evidence links, filter by status
    correctly.
  - Ratification integration: a `RatifyCommitteeDecisionCommand` call with
    pending decisions issues them atomically; failure during issue rolls
    everything back (no partial state).

- **`tests/Wombat.Architecture.Tests/`** — should remain green. If the new
  project references violate layer boundaries the test will fail. Do not
  suppress.

- **`tests/Wombat.Web.Tests/`** — add one bUnit smoke test for the pending
  decisions panel on `ReviewDetail.razor` (renders without error given a
  ratified and a scheduled review).

No integration tests required (Docker-gated per CLAUDE.md).

### 7. Documentation

- Update `Rewrite/DOMAIN.md` — add the STAR / entrustment decision section.
  Define the term "Statement of Awarded Responsibility" and cross-link to
  committee decisions.
- Update `Rewrite/ARCHITECTURE.md` only if a new dependency pattern emerges
  (e.g. the atomic issue-during-ratification integration). Otherwise leave
  alone.
- `Rewrite/practical-plan.md` progress table: mark T029 done on commit.
- `Rewrite/current_state.md`: advance to T030, model recommendation Opus
  (QuestPDF work continues to benefit from Opus's larger-context planning).

## Validation

Before declaring done:

```bash
dotnet build Wombat.sln -c Release           # zero errors, zero warnings
dotnet test                                   # all Domain, Application, Architecture, Web tests green
```

Manual smoke (Web host):

- Log in as a panel chair. On a scheduled committee review, start it, record a
  decision, add two pending entrustment decisions (different EPAs), ratify.
  Confirm the two `EntrustmentDecision` rows exist in the DB with
  `Status=Active` and correct evidence links.
- Revoke one of them as InstitutionalAdmin. Confirm `Status=Revoked`,
  `RevokedOn/By/Reason` populated.
- Issue a third decision for the same `(TraineeUserId, EpaId)` as an existing
  active one. Confirm the prior becomes `Superseded` with
  `SupersededByDecisionId` set.
- Simulate the expiry job (temporarily lower the cron or invoke directly):
  confirm decisions with past `ExpiresOn` flip to `Expired` and an email is
  queued.

## Exit criteria

- Build + tests green
- Aggregate, EF config, migration (with Designer), and model-snapshot update
  land in one commit
- Ratification of a committee review can issue entrustment decisions in a
  single transaction
- Daily expiry job registered and exercised in tests
- Auto-supersession on re-issue works
- Revocation captures full audit tuple
- Commit created, `current_state.md` advanced to T030

## Estimated effort

L — 5 days. Largest items: migration with Designer, ratification integration,
and the transactional guarantees on issuance.

## Non-goals (deferred to T030)

- Certificate PDF
- Trainee "My authorisations" dashboard panel
- Admin list / revoke UI

Keep T029 a domain+application+migration task. UI is minimal: only enough for a
chair to stage pending decisions on a review.
