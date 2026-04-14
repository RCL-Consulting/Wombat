# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T027 — Institutional SSO (OIDC)** (next task on the critical path after T026) — **Model: Opus**

T026 is now implemented. Next session: start `Tasks/T027-institutional-sso.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → ~~T012~~ → ~~T023~~ → ~~T024~~ → ~~T025~~ → ~~T026~~ → T027 → T013 → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

T026 completed:
- Domain layer:
  - `DataRightsRequest` entity (Guid v7 PK, factory `Create()`, workflow methods: Review, Approve, Reject, Complete, Withdraw)
  - `DataRightsRectification` entity (child of request; records before/after values for corrections)
  - `DataRightsErasureRecord` entity (one per erased user; pseudonym, retention reasons as jsonb)
  - `DataRightsRequestType` enum (Access, Rectification, Export, Objection, Erasure)
  - `DataRightsRequestStatus` enum (Submitted, UnderReview, Approved, Rejected, Completed, Withdrawn)
- Application layer:
  - Commands: `SubmitDataRightsRequestCommand` (blocks erasure if active committee review), `WithdrawDataRightsRequestCommand` (owner-only), `ApproveDataRightsRequestCommand` (auto-executes erasure/access/objection on approval), `RejectDataRightsRequestCommand`, `ApplyRectificationCommand` (SpecialityAdmin+), `CompleteRectificationRequestCommand`, `UpdateObjectionFlagsCommand`
  - Queries: `ListDataRightsRequestsQuery` (paginated, filterable by type/status/requester), `GetDataRightsRequestByIdQuery` (owner or admin), `GetMyDataRightsRequestsQuery`, `GetObjectionFlagsQuery`, `DownloadAccessReportQuery`
  - Interfaces: `IErasureExecutor`, `IAccessReportBuilder`, `IObjectionFlagUpdater`, `IObjectionFlagReader`
  - DTOs: `DataRightsRequestDto`, `DataRightsRequestSummaryDto`, `DataRightsRectificationDto`, `DataRightsErasureRecordDto`, `PagedDataRightsResult`, `AccessExportResult`, `ObjectionFlagsDto`
  - PseudonymSalt added to `WombatOptions`
- Infrastructure:
  - `ErasureExecutor` — pseudonymises all UserId references across: Activity (subject/creator), ActivityTransition (actor), ActivityType (owner/staging), ActivityTypeVersion (publisher), CommitteeReview (trainee/started-by/ratified-by), CommitteeDecision (chair), CommitteeAppeal (lodged-by/resolved-by), DecisionPanelMember, MsfCampaign (subject/creator/reviewer), CurriculumItemProgress, PortfolioExport, TraineeProfile, AssessorProfile, Invitation (issued-by). Clears Identity user PII, disables login, removes roles and scope associations. Retains audit entries unchanged. Generates deterministic pseudonym via SHA-256(salt + userId).
  - `AccessReportBuilder` — builds ZIP with JSON data export + PDF portfolio summary. Covers profile, activities, committee reviews, MSF campaigns, curriculum progress, audit entries, portfolio exports.
  - `ObjectionFlagService` — reads/writes opt-out flags on WombatIdentityUser
  - EF configurations for DataRightsRequest, DataRightsRectification, DataRightsErasureRecord
  - OptOutOfOptionalProcessing, OptOutOfDigestEmails added to WombatIdentityUser
  - Migration `20260414134415_DataRights` with Designer + snapshot (auto-generated via `dotnet ef`)
  - DbSets added to ApplicationDbContext
  - All services registered in Infrastructure DI
- Web (Blazor):
  - `Profile/DataRights.razor` at `/account/data-rights` — user self-service: processing preferences (opt-out checkboxes), submit requests (type + reason), view own requests with withdraw/download actions
  - `Admin/DataRights/RequestsList.razor` at `/admin/data-rights` — filterable by type/status, paginated, links to detail
  - `Admin/DataRights/RequestDetail.razor` at `/admin/data-rights/{Id}` — full request detail, approve/reject with decision note
  - NavMenu: "Data Rights" link for all authenticated users + admin/coordinator sections
- Tests: 25 new tests (111 total, was 86)
  - 7 `DataRightsRequestTests` (domain entity workflow: create, approve, reject, complete, withdraw, invalid transitions)
  - 6 `DataRightsCommandHandlerTests` (submit, erasure block, withdraw own/other, reject as admin/non-admin)
  - 7 `DataRightsQueryHandlerTests` (my requests, list filter, pagination, get-by-id as owner/other/admin)
  - 5 `ErasurePseudonymTests` (deterministic, prefix, different salt/user, expected length)
  - 1 `ErasureCoverageTests` — **reflection-based test** that enumerates all Domain entities with UserId-like string properties and fails if ErasureExecutor doesn't handle them
- Verification:
  - `dotnet build Wombat.sln -c Release` — clean (0 warnings, 0 errors)
  - `dotnet test Application.Tests` — 111 passed (was 86)
  - `dotnet test Web.Tests` — 33 passed
  - `dotnet test Domain.Tests` — 17 passed
- Verification caveats:
  - Erasure not tested end-to-end against running PostgreSQL
  - The auto-generated migration also creates Committee tables (DecisionPanels, CommitteeReviews, etc.) that were in the snapshot but had empty migrations before — this is correct, they are needed for the first real deployment
  - Data rights UI not tested against running app
  - Access report download endpoint not yet wired (the query exists but no download page)
