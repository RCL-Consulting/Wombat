# T025 — Admin audit log

**Phase:** 6 — Cross-cutting operations
**Depends on:** T005, T006
**Blocks:** T016, T026

## Goal

An append-only record of every consequential action taken in the system: who did what, to whom, when, with what inputs, from where. Not a log file — a queryable table that satisfies "show me everything that happened to trainee X in 2026" in under a second. This is the evidence base for disputes, regulator inspections, and security reviews.

## What counts as auditable

Not every event. Reads are not audited (too noisy, wrong signal). What is audited:

- Every **command** that mutates state (via a MediatR pipeline behaviour — zero per-handler wiring).
- Every authentication event (login, logout, failed login, password change, role change, account lock/unlock).
- Every permission change (role assignment, scope change).
- Every committee decision action (start, record, ratify, appeal, resolve).
- Every MSF campaign state transition.
- Every data-export request (portfolio PDF, admin CSV export).
- Every data-subject-rights action (T026): export, deletion, rectification.
- Every scheduled-job "run now" manual dispatch (T024).
- Every activity type publish (T019) — which admin changed which schema, before/after hash.

## What to do

1. **Domain** in `Wombat.Domain/Audit/`:
   - `AuditEntry` — Id (`Ulid` for time-sortable keys), OccurredAt (UTC), ActorUserId?, ActorDisplay (denormalized at write time; users may be renamed later), ActorIpAddress?, ActorUserAgent?, Category (enum: `Command`, `Authentication`, `Permission`, `Committee`, `Msf`, `Export`, `DataRights`, `Job`, `ActivityType`), Action (string — e.g. `RecordCommitteeDecisionCommand`), SubjectType? (string — e.g. `CommitteeReview`), SubjectId? (`Guid`), InstitutionId?, SpecialityId?, SummaryJson (`jsonb` — small, redacted payload describing the change), Success (bool), ErrorMessage?.
   - `AuditEntry` has no domain methods and no behaviour. It is a record.
2. **Pipeline behaviour** in `Wombat.Application/Audit/AuditPipelineBehavior.cs`:
   - Implements `IPipelineBehavior<TRequest, TResponse>`.
   - Runs before and after the handler.
   - On success: writes an `AuditEntry` with `Success = true` and a redacted snapshot of the request.
   - On exception: writes an entry with `Success = false` + `ErrorMessage`, then rethrows.
   - Only applies to commands (`IRequest<T>` where `T : ICommandResult` or a marker interface `IAuditedCommand`). Queries skip the behaviour.
3. **Redaction**:
   - A `[Redact]` attribute marks properties on commands that must be scrubbed before writing to `SummaryJson`. Passwords, tokens, raw email bodies, file contents.
   - The `AuditPayloadSerializer` applies `[Redact]` before serialization. Redacted fields are replaced with `"[REDACTED]"`.
4. **Auth audit hooks**: wire `AuditEntry` writes into the Identity events. Failed logins must always be recorded, even when the user does not exist (record the attempted username without leaking whether the user exists in the error message).
5. **Manual audit API** for features that do not go through MediatR (rare): `IAuditWriter.Write(AuditEntry entry, CancellationToken ct)`.
6. **Append-only enforcement**:
   - The `AuditEntry` table has no `UPDATE` or `DELETE` permission for the app's database user. Grant only `INSERT` and `SELECT` via an explicit migration `GRANT`. This is belt-and-braces: domain code has no mutation method anyway, but a defence-in-depth posture against a future careless migration.
   - The migration that creates `AuditEntry` also creates a PostgreSQL trigger that raises an exception on `UPDATE` or `DELETE`. Yes, it is redundant. Yes, do it.
7. **Indexes**:
   - B-tree on `(OccurredAt DESC)` for time-range queries.
   - B-tree on `(ActorUserId, OccurredAt DESC)` for "what did user X do".
   - B-tree on `(SubjectType, SubjectId, OccurredAt DESC)` for "what happened to object Y".
   - GIN on `SummaryJson` for ad-hoc field searches (rare but occasionally needed by admins).
8. **Admin UI** under `Wombat.Web/Components/Pages/Admin/Audit/`:
   - `AuditList.razor` — filterable by actor, category, action, subject type, date range, success. Default shows last 24 hours. Server-side pagination (audit tables get large fast).
   - `AuditDetail.razor` — one entry, full payload, raw JSON view for developers (behind a role check).
9. **CQRS**:
   - `ListAuditEntriesQuery`, `GetAuditEntryByIdQuery`.
   - No commands — the table is write-only from the app side, read-only from the UI.
10. **Retention**:
    - Audit rows are **not** deleted on a schedule. They are moved to a separate `AuditEntryArchive` table after 2 years by the `AuditLogRetentionJob` (T024), then to compressed cold storage after 7 years. Define the cold-storage path in `INFRASTRUCTURE.md`.
    - Regulators sometimes require specific retention windows; surface the retention policy in `CUSTOMIZATION.md` so institutions can adjust.

## Files created

- `src/Wombat.Domain/Audit/AuditEntry.cs`
- `src/Wombat.Domain/Audit/AuditCategory.cs`
- `src/Wombat.Application/Audit/AuditPipelineBehavior.cs`
- `src/Wombat.Application/Audit/IAuditWriter.cs`
- `src/Wombat.Application/Audit/RedactAttribute.cs`
- `src/Wombat.Application/Audit/AuditPayloadSerializer.cs`
- `src/Wombat.Infrastructure/Audit/AuditWriter.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/Audit/AuditEntryConfiguration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*AuditLog.cs` (includes trigger)
- `src/Wombat.Web/Components/Pages/Admin/Audit/**`
- `tests/Wombat.Application.Tests/Audit/**`

## Verification

- [ ] `dotnet build` clean.
- [ ] `dotnet test` — pipeline behaviour test confirms every command routes through the auditor; redaction test confirms `[Redact]` scrubs marked fields.
- [ ] Manual: perform several actions (login, create activity, submit, ratify committee decision). Open `AuditList`, confirm each shows. Open one entry, confirm the payload matches the command.
- [ ] Try to `UPDATE` an audit row via a direct psql session using the app's DB user — confirm the trigger throws.
- [ ] Failed login attempts appear in the audit with `Success = false` and no leak of whether the user exists.
- [ ] Large-volume test: 50,000 synthetic audit rows written, list queries stay <200ms with the indexes.

## Notes & gotchas

- `Ulid` not `Guid`: audit queries are almost always time-ordered; Ulids sort naturally by time and eliminate the "order by OccurredAt, then by Id to break ties" dance.
- The audit table is the biggest write-path hotspot in the system. Keep `SummaryJson` small (<2KB per entry typical). Large payloads go to a side-channel (`AuditEntryBlob` keyed by entry id) so the main table stays lean.
- **Do not** audit queries. Teams always want to "just for a week" and it never gets turned off. The resulting volume kills the feature. If a specific high-sensitivity read genuinely needs an audit, add an explicit `IAuditWriter.Write` call for that one case.
- The audit table is a tempting target for data-subject deletion requests (T026). Do not allow it. The audit of an action persists even if the subject is deleted — this is both legally permissible (legitimate interest / legal obligation) and operationally necessary. Document in `CUSTOMIZATION.md` and `T026`.
- Do not put PII in `ActorIpAddress` storage longer than you must. Truncate to /24 (IPv4) or /48 (IPv6) after 90 days, or configure per-institution.
- The pipeline behaviour must be the **outermost** behaviour — it sees the final result and any exception from inner behaviours. Register it first in the MediatR pipeline.
