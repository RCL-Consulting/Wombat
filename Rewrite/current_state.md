# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T026 — Data subject rights (POPIA/GDPR self-service)** (next task on the critical path after T025) — **Model: Opus**

T025 is now implemented. Next session: start `Tasks/T026-data-subject-rights.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → ~~T012~~ → ~~T023~~ → ~~T024~~ → ~~T025~~ → T026 → T027 → T013 → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

T025 completed:
- Domain layer:
  - `AuditEntry` entity (Guid v7 PK, append-only via `AuditEntry.Create()` factory, no mutation methods)
  - `AuditEntryArchive` entity (same schema + `ArchivedAt`, no trigger)
  - `AuditCategory` enum (Command, Authentication, Permission, Committee, Msf, Export, DataRights, Job, ActivityType)
- Application layer:
  - `IAuditWriter` interface — single `WriteAsync` method
  - `IAuditContextProvider` interface — UserId, UserDisplay, IpAddress, UserAgent
  - `IAuditedCommand` marker interface — opt-in for non-`*Command` named requests
  - `RedactAttribute` — marks command properties for scrubbing before SummaryJson serialisation
  - `AuditPayloadSerializer` — reflects over command properties, replaces `[Redact]` values with "[REDACTED]", serialises to camelCase JSON
  - `AuditPipelineBehavior<TRequest, TResponse>` — outermost MediatR behaviour; audits requests named `*Command` or implementing `IAuditedCommand`; skips queries
  - Queries: `ListAuditEntriesQuery` (filterable by category/action/actor/subject/date/success, server-side pagination), `GetAuditEntryByIdQuery`
  - DTOs: `AuditEntryDto`, `PagedAuditResult`
  - Registered `AuditPipelineBehavior` as first (outermost) behaviour in Application DI
- Infrastructure:
  - `AuditWriter` — scoped service; adds entry and calls `SaveChangesAsync`
  - `HttpAuditContextProvider` — reads current user from `IHttpContextAccessor`, truncates IP (/24 IPv4, /48 IPv6)
  - `AuditEntryConfiguration`, `AuditEntryArchiveConfiguration` — EF fluent config with jsonb column, GIN index on SummaryJson
  - `AuditLogRetentionJob` — real implementation: archives entries older than 2 years in batches of 1000 into `AuditEntryArchives`, runs daily at 03:00 UTC
  - Migration `20260414120000_AuditLog` with Designer + snapshot updated; includes PostgreSQL trigger `audit_entries_immutable` that raises exception on UPDATE/DELETE
  - DbSets added to `ApplicationDbContext`
  - `IAuditWriter` and `IAuditContextProvider` registered in Infrastructure DI
- Web (Program.cs):
  - Login handler: writes `Login`/`LoginFailed` audit entries; failed login doesn't leak user existence
  - Logout handler: writes `Logout` audit entry with user ID
  - `TruncateLoginIp` helper
- Web (Blazor):
  - `AuditList.razor` at `/admin/audit` — filterable by category/action/actor/subject/date/success; server-side pagination; 50-entry pages
  - `AuditDetail.razor` at `/admin/audit/{Id}` — full entry detail; raw JSON visible to Administrators only
  - NavMenu: "Audit Log" link under Administrator section
- Tests: 12 new tests
  - 4 `AuditPipelineBehaviorTests` (command audit, query skip, failure capture, IAuditedCommand marker)
  - 4 `AuditPayloadSerializerTests` (plain command, single redact, multiple redact, empty command)
  - 4 `ListAuditEntriesQueryHandlerTests` (default 24h, category filter, success filter, pagination)
- Verification:
  - `dotnet build Wombat.sln -c Release` — clean (0 warnings, 0 errors)
  - `dotnet test Application.Tests` — 86 passed (was 74)
  - `dotnet test Web.Tests` — 33 passed
  - `dotnet test Domain.Tests` — 17 passed
- Verification caveats:
  - Append-only trigger not tested end-to-end — requires running PostgreSQL
  - GRANT (REVOKE UPDATE, DELETE) not applied in migration — must be done manually post-deployment (documented in migration comment)
  - Audit browsing UI not tested against running app
