# Wombat

Work-Based Assessment Tool for medical specialists. Built around the EPA
(Entrustable Professional Activities) approach to competency-based medical
education. The application tracks trainee portfolios, workplace-based assessments,
curriculum progress, and committee decisions across institutions.

**This is a rewrite in progress.** The canonical plan lives in `Rewrite/`. Start
every session by reading `Rewrite/current_state.md` — it names the active task,
the last verified commit, and any blockers.

## Project overview

A multi-project ASP.NET Core / Blazor solution backed by PostgreSQL. The primary
user-facing runtime is the Blazor Interactive Server app (`Wombat.Web`). A companion
REST API (`Wombat.Api`) exposes the same Application and Infrastructure layers and
is reserved for webhooks and integration endpoints.

The distinguishing architectural feature is a **schema-driven Activity platform**:
institutions can add new activity types (assessments, reflections, procedure logs,
research outputs, QI projects, etc.) through a visual builder without developer
involvement. Activity types carry their form schema, workflow state machine, and
credit rules as `jsonb` columns, interpreted at runtime by generic validators,
evaluators, and renderers.

## Key technical choices

| Concern | Decision |
|---------|----------|
| Architecture | Clean Architecture with CQRS via MediatR; dependency direction enforced by architecture tests |
| UI | Blazor Interactive Server; use `IScopedSender` (not `ISender`) in interactive components |
| Design system | Custom CSS in `app.css` per `Rewrite/DESIGN.md`; no Bootstrap, no MudBlazor, no Radzen, no jQuery |
| ORM | EF Core 10 with PostgreSQL (`Npgsql`); migrations applied at startup |
| Database | PostgreSQL; `jsonb` columns for activity schema/workflow/credit/data |
| MediatR | v12.x maximum — **do not upgrade to paid v13** |
| Clinical dates | `DateOnly` for calendar dates; `DateTime` only for timestamps and audit events |
| PDF generation | QuestPDF (portfolio export in T023) |
| Auth | ASP.NET Core Identity; 9 roles (see below); admin-controlled onboarding via invitations; institutional SSO via OIDC (T027) |
| Icons | Inline SVGs from Lucide via a shared `Icon.razor` component. **Bootstrap Icons font is not loaded** — `<i class="bi bi-*">` renders nothing |
| Security | CSP with nonce-backed `script-src`; `X-Content-Type-Options: nosniff`; rate-limited login |
| Dependency licensing | GPLv3-compatible additions only |
| Platform | Windows development; Ubuntu 24.04 LTS deployment target |

## Repository layout

```
Wombat/
├── src/
│   ├── Wombat.Domain/             ← entities, value objects, DSL parsers, no framework deps
│   │   └── Activities/            ← ActivityType, Activity, schema/workflow/credit DSLs
│   ├── Wombat.Application/        ← MediatR commands, queries, handlers, DTOs, validators
│   │   └── Features/              ← feature folders: Activities, Dashboards, Institutions, …
│   ├── Wombat.Infrastructure/     ← EF Core, Identity, email, external services
│   │   ├── Persistence/           ← DbContext, configurations, migrations
│   │   ├── Identity/              ← WombatIdentityUser, ExternalLoginHandler, SsoGroupMapper
│   │   ├── Activities/            ← SchemaValidator, WorkflowEvaluator, CreditApplier
│   │   └── Reporting/             ← PortfolioPdfService, QuestPDF section components
│   ├── Wombat.Api/                ← thin REST controllers (webhooks, integration)
│   └── Wombat.Web/                ← Blazor Interactive Server app
│       ├── Components/Pages/      ← Admin/*, Activities/*, Account/*, Portfolio/*
│       ├── Components/Layout/     ← MainLayout, NavMenu
│       ├── Components/Shared/     ← Icon, PageHeader, DataTable, FormField, …
│       └── wwwroot/               ← app.css (design system), icons/
├── tests/
│   ├── Wombat.Domain.Tests/
│   ├── Wombat.Application.Tests/
│   ├── Wombat.Architecture.Tests/ ← enforces layer boundaries
│   ├── Wombat.Integration.Tests/
│   └── Wombat.Web.Tests/          ← bUnit smoke tests (added in T010)
├── Rewrite/                       ← the rewrite plan (READ THIS FIRST)
│   ├── current_state.md           ← live handoff — read before every session
│   ├── PLAN.md                    ← master task list with progress checkboxes
│   ├── DOMAIN.md                  ← what EPAs, WBAs, STAR, roles mean
│   ├── ARCHITECTURE.md            ← Clean Architecture / CQRS conventions
│   ├── DESIGN.md                  ← canonical UI/design-system contract
│   ├── CUSTOMIZATION.md           ← the Activity platform (jsonb schema + workflow + credit)
│   ├── WORKFLOW.md                ← git branching, session handoff protocol
│   ├── INFRASTRUCTURE.md          ← Linode deployment target
│   └── Tasks/T0xx-*.md            ← individual task files
├── ClinicAssist.NET_ref_DO_NOT_COMMIT/   ← reference architecture (read-only, not committed)
├── Wombat_ref_old_DO_NOT_COMMIT/         ← old Wombat source (read-only, not committed)
├── Directory.Build.props
├── Directory.Packages.props
├── .editorconfig
└── Wombat.sln
```

## Architecture

Domain -> Application -> Infrastructure: dependency direction is one-way.
Architecture tests in `Wombat.Architecture.Tests` fail the build if boundaries
are violated — do not work around them.

| Layer | Responsibility |
|-------|----------------|
| Domain | Entities, enums, value objects, DSL parsers; no framework dependencies |
| Application | Use-case commands/queries/handlers via MediatR; orchestration without EF coupling |
| Infrastructure | EF Core `ApplicationDbContext`, repositories, migrations, Identity, activity runtime services |
| Api | Thin MediatR-backed REST controllers for webhooks/integration |
| Web | Blazor pages/components; use `IScopedSender` for scoped MediatR dispatch |

Application handlers may not depend on EF types — they go through `IApplicationDbContext`.
Web components may not reference Domain types directly in `.razor` files — only via DTOs.

## Roles

Wombat has 9 roles, checked via ASP.NET Core Identity:

1. **Administrator** — global; sees everything.
2. **InstitutionalAdmin** — scoped to one institution.
3. **SpecialityAdmin** — scoped to one speciality within an institution.
4. **SubSpecialityAdmin** — scoped to one sub-speciality.
5. **Coordinator** — administrative staff supporting a programme.
6. **CommitteeMember** — sits on the annual review committee.
7. **Assessor** — workplace supervisor who assesses trainees.
8. **Trainee** — the learner working through a curriculum.
9. **PendingTrainee** — invited but not yet admitted by admin.

Users can hold multiple roles. Onboarding is admin-controlled via invitations or SSO
provisioning. SSO-provisioned users get roles from group-to-role mappings; if no groups
match, they land as PendingTrainee. The Administrator role **cannot** be assigned via SSO —
it always requires explicit manual assignment.

## Activity platform (the schema-driven pivot)

The core departure from ClinicAssist. Instead of one aggregate per assessment type,
Wombat has:

- `ActivityType` — aggregate holding `SchemaJson` (form), `WorkflowJson` (state machine),
  `CreditRulesJson` (curriculum credit rules), all as `jsonb`. Versioned; publishing
  bumps the version and existing activities stay pinned to theirs.
- `Activity` — aggregate with `DataJson` (jsonb) holding the user's submission data,
  plus a state driven by the workflow engine.

Runtime services in Infrastructure:
- `SchemaValidator` — validates `DataJson` against the form schema.
- `WorkflowEvaluator` — evaluates available transitions for a `ClaimsPrincipal`.
- `CreditApplier` — matches completed activities to curriculum items and applies credit.

Read `Rewrite/CUSTOMIZATION.md` for the full model.

## EF Core migrations

Migrations are applied at startup via `Database.MigrateAsync()`. When the `dotnet ef`
CLI cannot run (e.g. no connection string available), migrations must be written by hand.
A hand-written migration **must** include a `.Designer.cs` file alongside the `.cs` file.
Without the Designer file — which carries the `[Migration("...")]` and `[DbContext(...)]`
attributes — EF Core will not discover the migration and `MigrateAsync()` will silently
skip it.

To create a Designer file: copy the most recent existing Designer, change the
`[Migration]` timestamp and partial class name, and update the `BuildTargetModel` body.
Also update `ApplicationDbContextModelSnapshot.cs` to match the final model state.

## Coding conventions

- `PascalCase` for types, methods, properties. `camelCase` for local variables and fields.
- `DateOnly` for all calendar-based dates (`AssessmentDate`, `AdmissionDate`, etc.).
- `DateTime` only for audit timestamps and system events.
- Never commit plaintext connection strings or credentials. Use `user-secrets` locally
  and environment variables in deployment.
- Admin-controlled onboarding only: registration requires a valid invitation token.
- Use `IScopedSender` (not `ISender`) in Blazor InteractiveServer components to avoid
  DbContext lifetime issues inside circuits.
- Feature folders under `Wombat.Application/Features/{FeatureName}/{Commands|Queries}/{Name}/`.
- Handlers return DTOs, not entities. One command = one handler.
- Validators use FluentValidation and live alongside their command.

## Design system non-negotiables

The canonical design contract is `Rewrite/DESIGN.md`. Key rules:

- **No Bootstrap classes.** No `class="table"`, no `btn-outline-primary`, no `col-md-*`.
  Wombat ships its own token-driven `app.css`.
- **No raw hex colors outside `:root`.** All colors route through CSS custom properties.
- **No `<i class="bi bi-*">`.** The Bootstrap Icons font is not loaded. Use the shared
  `Icon.razor` component backed by inline SVGs from Lucide.
- **No inline `<style>` blocks** in page-level `.razor` files. All styles live in `app.css`
  or in `.razor.css` isolation files for layout components only.
- **No MudBlazor, no Radzen, no jQuery.** Plain Blazor + custom CSS.

## Testing

```bash
# Domain tests
dotnet test tests/Wombat.Domain.Tests/Wombat.Domain.Tests.csproj

# Application tests (the primary test suite)
dotnet test tests/Wombat.Application.Tests/Wombat.Application.Tests.csproj

# Architecture boundary tests
dotnet test tests/Wombat.Architecture.Tests/Wombat.Architecture.Tests.csproj

# bUnit UI smoke tests (after T010)
dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj
```

Always run architecture tests after adding any project reference — they guard layer
boundaries. Integration tests require Docker for Testcontainers.

## Build and run

```bash
# Build the full solution
dotnet build Wombat.sln -c Release

# Run the Web host (requires ConnectionStrings:DefaultConnection via user-secrets or env)
dotnet run --project src/Wombat.Web/Wombat.Web.csproj

# Run the API host
dotnet run --project src/Wombat.Api/Wombat.Api.csproj
```

Local dev: `http://localhost:5080` (Web), `http://localhost:5090/health` (API).
Set `ConnectionStrings:DefaultConnection` via `dotnet user-secrets` for local
development. Production startup fails fast if it is missing.

## Deployment target

- OS: Ubuntu 24.04 LTS on Linode
- Runtime: .NET 10
- Reverse proxy: Caddy (TLS + port forwarding)
- Database: PostgreSQL (local for Phase 1; managed later if justified)
- Process manager: systemd
- See `Rewrite/INFRASTRUCTURE.md` for the full server layout.

## Task management

All task state lives in the `Rewrite/` folder. Before starting any work, read:

1. `Rewrite/current_state.md` — names the active task, blockers, last commit.
2. The task file it points to under `Rewrite/Tasks/`.
3. `Rewrite/DESIGN.md` — if the task touches any Razor file.

Work only on the active task. When done, update `current_state.md` and check the
box in `PLAN.md`. If unexpected work surfaces, add a new task file rather than
mutating the current one — the plan is append-only so git history stays useful.

**Commit after every completed task.** Do not accumulate multiple tasks in a single
uncommitted working tree.

## Reference folders

- `ClinicAssist.NET_ref_DO_NOT_COMMIT/` — the reference architecture. Treat as
  read-only. When unsure about structure, look there first. **Do not commit this
  folder** — it is in `.gitignore`.
- `Wombat_ref_old_DO_NOT_COMMIT/` — the old Wombat source. Read-only reference for
  porting domain logic. Also in `.gitignore`.
