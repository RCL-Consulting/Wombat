# Architecture

> **⚠ Amended by `CUSTOMIZATION.md`.** The ClinicAssist layout is still the baseline, but Wombat adds a schema-driven Activity platform that ClinicAssist does not have. Do not copy ClinicAssist on anything that touches the activity model — that is the deliberate departure. See `CUSTOMIZATION.md` and the "Activity platform" section at the end of this document.

The rewrite follows the ClinicAssist.NET layout almost exactly. When in doubt, open the reference project and copy the shape. This document captures the non-negotiables and the decisions that don't fall out naturally from "do what ClinicAssist does".

## Solution layout

```
Wombat.sln
├── src/
│   ├── Wombat.Domain/          <-- entities, value objects, domain services, no deps except BCL
│   ├── Wombat.Application/     <-- CQRS (MediatR), DTOs, validators, interfaces
│   ├── Wombat.Infrastructure/  <-- EF Core, Identity, email, external services
│   ├── Wombat.Api/             <-- minimal ASP.NET Core API for webhooks/integrations only
│   └── Wombat.Web/             <-- Blazor Interactive Server app; the main UI
├── tests/
│   ├── Wombat.Domain.Tests/
│   ├── Wombat.Application.Tests/
│   ├── Wombat.Architecture.Tests/
│   └── Wombat.Integration.Tests/
├── Directory.Build.props
├── Directory.Packages.props
├── .editorconfig
└── .gitignore
```

Copy ClinicAssist's `Directory.Build.props` and `Directory.Packages.props` verbatim, change the assembly prefix to `Wombat.`, then diverge only if something forces it.

## Layer rules

These are enforced by `Wombat.Architecture.Tests` (see T013). They are not suggestions.

- `Wombat.Domain` depends on **nothing** except the BCL and optionally a tiny `Ardalis.GuardClauses` style helper. No EF, no MediatR, no ASP.NET.
- `Wombat.Application` depends on `Wombat.Domain` and MediatR/FluentValidation only. It defines interfaces (e.g. `IApplicationDbContext`, `IEmailSender`) that Infrastructure implements.
- `Wombat.Infrastructure` depends on `Wombat.Application` and `Wombat.Domain`. It owns EF Core, Identity, MailKit, QuestPDF (if kept), etc.
- `Wombat.Api` depends on `Wombat.Application` and `Wombat.Infrastructure`. No Domain-specific logic.
- `Wombat.Web` depends on `Wombat.Application` and `Wombat.Infrastructure`. No direct Domain manipulation from Blazor components — go through MediatR.

Arch test summary:

- Domain → {BCL}
- Application → {Domain}
- Infrastructure → {Application, Domain}
- Web → {Application, Infrastructure}
- Api → {Application, Infrastructure}
- Web ⊄ Domain (may not reference Domain types directly in components, only via DTOs)
- Application handlers may not depend on EF types (they go through `IApplicationDbContext`)

## CQRS

- MediatR v12 (max). Do not upgrade to paid v13.
- Commands end in `Command`; Queries end in `Query`. Handlers end in `Handler` and live next to their command in a feature folder.
- Feature folders under `Wombat.Application/Features/{FeatureName}/{Commands|Queries}/{CommandName}/`. Mirror ClinicAssist exactly.
- Validators use FluentValidation and live in the same folder as their command.
- Handlers return DTOs, not entities.
- A command has one handler. A query has one handler. If you find yourself wanting two handlers for one command, split the command.

## Blazor

- **Interactive Server** render mode. Not WebAssembly. Not Auto. Same SignalR circuit model as ClinicAssist.
- Pages inject `IScopedSender`, not `ISender`. This is a Wombat/ClinicAssist convention to avoid the scoped-lifetime footgun inside Blazor circuits. Copy the implementation from ClinicAssist verbatim.
- Components live under `Wombat.Web/Components/`. Layout + shared under `Components/Layout/`. Feature components under `Components/Pages/{Feature}/`.
- No Bootstrap Icons font — ClinicAssist learned this the hard way. Use inline SVG or a different icon set.
- No jQuery. If a component needs interactivity, do it with Blazor or a small JS interop module.
- No global CSS dumping ground. Component-scoped CSS files or a single `app.css` with a clear section header per feature.

## Identity

- ASP.NET Core Identity, scaffolded, with a custom `WombatUser : IdentityUser` (copy the relevant bits of Wombat.Data/WombatUser.cs into Domain, minus EF attributes).
- Roles are seeded in `Wombat.Infrastructure/Identity/RoleSeeder.cs` from the nine roles defined in DOMAIN.md.
- Claim policies defined in `Wombat.Infrastructure/Identity/AuthorizationPolicies.cs` and registered in `Program.cs`. Mirror ClinicAssist's pattern.
- Registration is **invitation-only**. Self-registration is disabled. A user arrives via an invitation token, which pre-populates the form and ties them to a role and scope.

## Database

- PostgreSQL. Npgsql provider.
- EF Core 10 (same version as ClinicAssist; do not diverge).
- `ApplicationDbContext` implements `IApplicationDbContext` (the Application interface), so handlers depend on the interface.
- Migrations live in `Wombat.Infrastructure/Persistence/Migrations/`. **Always include the `Designer.cs` file** — EF Core needs it for `dotnet ef database update`. This is a recurring footgun in ClinicAssist; same rule here.
- Use `DateOnly` for calendar dates, `DateTime` (UTC) only for timestamps. Register the `DateOnlyConverter` in the DbContext.
- Seed data goes through `HasData(...)` in entity configurations for reference tables (roles, default institutions, scales), and through a `DataSeeder` service for bootstrap data that needs to run once at startup.

## Email

Port the good parts of the current Wombat email setup:

- `IEmailSender` interface in Application.
- `MailKitEmailSender` implementation in Infrastructure.
- An in-process channel queue (`System.Threading.Channels.Channel<EmailMessage>`) and a hosted `EmailWorker : BackgroundService` that drains it. Keeps web requests snappy.
- Templates stored as Razor files under `Wombat.Infrastructure/Email/Templates/` and rendered with `Microsoft.Extensions.RazorTemplating` or a hand-rolled renderer — whichever ClinicAssist uses.
- SMTP settings come from `appsettings.json` / environment variables, never from the database (simpler to audit, simpler to rotate).

## PDFs (if kept)

The current Wombat does not appear to generate PDFs heavily. Decide per task whether to pull in QuestPDF for STAR reflection exports and assessment summaries. Defer until there is an actual need — do not add it speculatively.

## Logging & diagnostics

- Serilog, configured in `Program.cs` of `Wombat.Web` and `Wombat.Api`. Same configuration as ClinicAssist.
- Sinks: Console (always), File (rolling daily, in `/var/log/wombat/` on the server), and optionally Seq if a Seq instance is reachable.
- Correlation IDs on every request.
- No PII in logs. User IDs yes, email addresses no.

## Configuration

- `appsettings.json` for defaults.
- `appsettings.Production.json` for production overrides (committed, no secrets).
- Environment variables for secrets (connection strings, SMTP credentials). Loaded via the default ASP.NET Core configuration pipeline.
- Systemd unit on the server sets the environment variables via an `EnvironmentFile=`.

## Testing

- xUnit everywhere.
- Domain tests: fast, no I/O.
- Application tests: handlers tested with an in-memory DbContext (Npgsql in-memory-compatible provider or a test container).
- Architecture tests: NetArchTest or a hand-rolled equivalent — mirror ClinicAssist.
- Integration tests: `WebApplicationFactory` with a disposable Postgres via Testcontainers. Only for the few critical flows (registration, assessment complete).

## Conventions

- Null-reference types enabled everywhere.
- Treat warnings as errors in CI builds; allow warnings in `dotnet watch` for flow.
- No `var` for primitive types; `var` allowed elsewhere. (Not a hill worth dying on; match ClinicAssist.)
- File-scoped namespaces.
- `sealed` on concrete classes by default.
- No `Task.Run` to fake async. No `async void` outside event handlers.

## Activity platform layer

The generic activity engine sits in its own sub-namespace inside Application and Infrastructure. Layout:

```
src/Wombat.Domain/Activities/
    ActivityType.cs                 (aggregate root)
    Activity.cs                     (aggregate root)
    ActivityTransition.cs
    ActivityPermissionRule.cs
    Schema/
        FormSchema.cs               (record hierarchy representing parsed schema)
        FormSection.cs
        FormField.cs
        FieldType.cs
    Workflow/
        Workflow.cs                 (parsed workflow)
        WorkflowState.cs
        WorkflowTransition.cs
    Credit/
        CreditRules.cs

src/Wombat.Application/Features/Activities/
    Commands/
        CreateActivity/
        TransitionActivity/
        UpdateActivityDraft/
    Queries/
        GetActivityById/
        ListActivitiesForSubject/
        ListActivitiesForActor/
        GetActivityTypeById/
    Services/
        IActivityService.cs
        ISchemaValidator.cs
        IWorkflowEvaluator.cs
        ICreditApplier.cs

src/Wombat.Infrastructure/Activities/
    ActivityService.cs
    SchemaValidator.cs
    WorkflowEvaluator.cs
    CreditApplier.cs
    Persistence/
        ActivityConfiguration.cs
        ActivityTypeConfiguration.cs
        ActivityTransitionConfiguration.cs
```

Key rules:

- **Never** add per-activity-type handlers. If a type needs something special, extend the schema language, not C#.
- The `Data` column on `Activity` is `jsonb`. Access only through `IActivityService`, which enforces schema validation on write.
- Strongly-typed projections for dashboards are allowed and encouraged; they read jsonb paths into DTOs inside query handlers. This is the only place strongly-typed shape meets untyped storage.
- Workflow rules are evaluated by `IWorkflowEvaluator` given an activity, the proposed transition, and the acting user's principal. The evaluator returns allow/deny with a reason. The Blazor UI also uses the evaluator to decide which buttons to render — same source of truth for UI and enforcement.
- Credit rules are applied by `ICreditApplier` after a transition reaches a terminal state. The applier walks the activity type's `CreditRules`, finds matching `CurriculumItem`s for the subject, and updates the computed progress view. Progress is a derived fact stored in a materialised view, not a mutable counter — rebuildable from activity history.

## Reporting

Because activity data is jsonb, ad hoc reports cannot be written as strongly-typed LINQ. Two patterns:

1. **Defined reports** — a developer writes a query handler that projects the jsonb into a typed DTO. Lives in `Wombat.Application/Features/Reports/`. Same pattern as any other query handler. These are the reports shown on dashboards and in exports.
2. **Ad hoc reports** — admins get a restricted query UI that lets them filter by activity type, subject, state, date range, and common fields. The UI translates to parameterised SQL under the hood. No free-form SQL for admins. This is Phase 6 extension work, not in the current task list; flagged here because it will come up.

## jsonb indexing strategy

- GIN index on `Activity.Data`.
- Expression index on `(Data->>'epa_id')` where EPA is a common filter field.
- Expression index on `(Data->>'assessor_user_id')` so "my inbox" queries are fast.
- Composite index on `(ActivityTypeId, CurrentState, SubjectUserId)` for dashboard queries.
- `pg_stat_statements` enabled in production to spot slow activity queries early.

## Testing the platform

Architecture tests (T013) verify:

- No code outside `Wombat.Domain/Activities/` and `Wombat.Infrastructure/Activities/` reads or writes the `Data` column directly.
- Every seeded activity type in T020 parses cleanly against the schema DSL and the workflow DSL.
- Round-trip tests: serialize each seeded schema to JSON, deserialize, re-serialize, confirm byte-identical. Catches silent schema drift.

Property tests for the workflow evaluator: given a random valid workflow and a random principal, the evaluator's decision must match a declarative oracle. Catches "one rule was silently more permissive than intended" bugs.
