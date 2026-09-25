# Wombat

Work-Based Assessment Tool for medical specialists. Built around the EPA
(Entrustable Professional Activities) approach to competency-based medical
education. The application tracks trainee portfolios, workplace-based assessments,
curriculum progress, and committee decisions across institutions.

**The rewrite is complete.** T001–T027 landed in June 2026 (`execution/knowledge/PLAN.md` § Status)
and the application has been deployed since 2026-06-19. Work since has been ordinary
product work — the activity platform, the EPA v11.1 catalogue, security hardening — tracked
as task files, not as a migration.

**Everything the project knows about itself lives under `execution/`**, the `rcl-harness`
workspace:

| | |
|---|---|
| `STATE.md`, `HANDOFF.md` | Live state, bounded and enforced. **Imported into every session** by the block at the end of this file; a SessionStart hook injects the active task. You do not need to go looking for state. |
| `tasks/{queued,in_progress,blocked,done}/` | The register, one file per task. |
| `architecture/` | How the system is built: `ARCHITECTURE.md`, `DESIGN.md`, `CUSTOMIZATION.md`, `INFRASTRUCTURE.md`. |
| `knowledge/` | What the project knows: `DOMAIN.md`, `EPA-PROGRAMME.md`, `HANDOVER.md`, `WORKFLOW.md`, `PLAN.md`, the scenario runbooks, the closed plans. |
| `DECISIONS.md` | Process decisions (`W-nnn`). Product decisions are `D1`–`D42` in `knowledge/EPA-PROGRAMME.md`. |
| `log/` | Trimmed overflow, and the 3,574-line pre-harness handoff archive. History; do not read it for state. |

These docs were in `Rewrite/` until 2026-09-20, then briefly `Programme/`, before landing in
the harness's standard layout. Old paths in `log/` and in completed task files are not
rewritten — they were correct when written.

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
| Design system | Custom CSS in `app.css` per `execution/architecture/DESIGN.md`; no Bootstrap, no MudBlazor, no Radzen, no jQuery |
| ORM | EF Core 10 with PostgreSQL (`Npgsql`); migrations applied at startup |
| Database | PostgreSQL; `jsonb` columns for activity schema/workflow/credit/data. **Never compare a stored `jsonb` value against serializer output as a raw string** — Postgres discards the submitted bytes and re-renders its own text (keys reordered, separators inserted), so nothing stored is ever byte-equal to what was written. Compare canonical-to-canonical: parse both sides and re-serialise. |
| MediatR | v12.x maximum — **do not upgrade to paid v13** |
| Clinical dates | `DateOnly` for calendar dates; `DateTime` only for timestamps and audit events |
| PDF generation | QuestPDF (portfolio export in T023) |
| Auth | ASP.NET Core Identity; 9 roles (see below); admin-controlled onboarding via invitations; institutional SSO via OIDC (T027) |
| Icons | Inline SVGs from Lucide via a shared `Icon.razor` component. **Bootstrap Icons font is not loaded** — `<i class="bi bi-*">` renders nothing |
| Security | CSP with nonce-backed `script-src`; `X-Content-Type-Options: nosniff`; sign-in throttled on failed password checks per client address (`SignInThrottle`, T156); `X-Forwarded-*` believed from loopback only |
| Dependency licensing | GPLv3-compatible additions only |
| Platform | Windows development; **Ubuntu 26.04 LTS** deployment target (the plan specified 24.04; the live box is 26.04) |

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
│   ├── Wombat.Infrastructure.Tests/ ← seed integrity, EF-backed services, DSL round-trips
│   ├── Wombat.Architecture.Tests/ ← enforces layer boundaries
│   ├── Wombat.Integration.Tests/
│   └── Wombat.Web.Tests/          ← bUnit smoke tests (added in T010)
├── execution/                     ← THE WORKSPACE. Bounded by rcl-harness; STATE+HANDOFF imported below
│   ├── STATE.md                   ← where the project is (≤60 lines, enforced)
│   ├── HANDOFF.md                 ← what the last session left (≤80 lines, enforced)
│   ├── DECISIONS.md               ← process decisions (W-nnn) and what was rejected
│   ├── DASHBOARD.md               ← generated by `harness.py status`; never hand-edit
│   ├── tasks/{queued,in_progress,blocked,done}/   ← the register, one file per task
│   ├── architecture/
│   │   ├── ARCHITECTURE.md        ← Clean Architecture / CQRS conventions
│   │   ├── DESIGN.md              ← canonical UI/design-system contract
│   │   ├── CUSTOMIZATION.md       ← the Activity platform (jsonb schema + workflow + credit)
│   │   └── INFRASTRUCTURE.md      ← Linode deployment target
│   ├── knowledge/
│   │   ├── DOMAIN.md              ← what EPAs, WBAs, STAR, roles mean
│   │   ├── EPA-PROGRAMME.md       ← the CPSA v11.1 programme; product decisions D1–D42
│   │   ├── HANDOVER.md            ← running the live service (T016 deliverable)
│   │   ├── WORKFLOW.md            ← git branching; session protocol superseded by the harness
│   │   ├── PLAN.md                ← the original rewrite plan; T001–T027, complete
│   │   ├── scenario-*.md          ← the replay runbooks that produce the test corpus
│   │   └── *-plan.md              ← closed post-rewrite plans, kept as record
│   └── log/                       ← trimmed overflow + the pre-harness handoff archive
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
it always requires explicit manual assignment — and an Administrator account can neither be linked to nor sign in
through SSO (T149). SSO sign-in also refuses a deactivated account (an admin's lock or an erasure, not a brute-force
lockout) and an account outside the provider's institution; linking reads everything from the external cookie, checks
the password with lockout, and is rate-limited. Moving a user to another institution drops their external logins.
SSO writes an email only when the provider verifies it (`SsoProviderOptions.EmailVerifiedClaim`, default
`email_verified`), never onto another account's address, provisions an account only from a verified email that no
account holds, and matches an account for linking only by a verified email (T155).

### InstitutionalAdmin scope-aware powers (T056)

Pages gated `[Authorize(Policy = "AdministratorOrInstitutionalAdmin")]` accept both
`Administrator` and `InstitutionalAdmin` callers, but their handlers filter results /
reject commands by institution scope. An `InstitutionalAdmin` from institution A cannot
see or edit data scoped to institution B; a global `Administrator` sees everything.

Handlers reachable from such pages take `ClaimsPrincipal Principal` in their MediatR
request record. Lists use `principal.GetInstitutionId()` to filter; get-by-id calls
`principal.CanAccessInstitution(entityInstitutionId)` and returns null when out-of-scope
(404, not 403, to avoid leaking the existence of other-institution ids); commands throw
`UnauthorizedAccessException` on scope mismatch. Helper extensions live on
`ClaimsPrincipalExtensions` (`IsAdministrator()`, `IsInstitutionalAdmin()`,
`CanAccessInstitution(int)`).

T056 is landing cluster-incrementally — see `execution/tasks/done/T056-institutional-admin-role-power.md`
for which page groups have been migrated and which still require `Administrator`.

## Activity platform (the schema-driven pivot)

The core departure from ClinicAssist. Instead of one aggregate per assessment type,
Wombat has:

- `ActivityType` — aggregate holding `SchemaJson` (form), `WorkflowJson` (state machine),
  `CreditRulesJson` (curriculum credit rules), all as `jsonb`. Versioned; publishing
  bumps the version and existing activities stay pinned to theirs.
- `Activity` — aggregate with `DataJson` (jsonb) holding the user's submission data,
  plus a state driven by the workflow engine.

Runtime services in Infrastructure:
- `SchemaValidator` — validates `DataJson` against the form schema. A transition declares how much of it counts
  (`validation`: `all`, the default; `owned`, the mover's writable fields; `draft`, formats only) (T105), so a
  schema's `required` flags are honest and `requires_fields` only ever adds. A create that is itself the filing (no
  move out of the initial state that leads on is the author's: a type born `requested` or terminal) checks the author's
  required fields as `owned` would (T127).
- `WorkflowEvaluator` — evaluates available transitions for a `ClaimsPrincipal`.
- `ActorRuleMatcher` — the one implementation of the actor grammar (`subject`, `creator`, `role:`,
  `scope:`, `field:`, combined with `|` and `+`). Shared by transition authorization and field-write
  authorization so the two cannot drift.
- `FieldPermissionEvaluator` — resolves which fields an actor may write, as the conjunction of the
  workflow state's `editable_by` and the field's (falling back to its section's). Default is
  `subject|creator`, which reproduces pre-T070 behaviour, so a type declaring nothing is unchanged.
- `CreditApplier` — matches completed activities to curriculum items and applies credit. Which items an activity
  would credit is `CreditTargetResolver`, the one implementation shared with the tool gate below. It reads only items
  in force (`CurriculumItemsInForce`, T158): an item whose EPA is deactivated is not offered and is on no progress page,
  and its credit is paused, not cancelled (D48, T196). Credit and the rebuild judge each completion at its own moment
  against `Epa.DeactivatedOn`; reactivating an EPA credits what was completed during the pause. Change the flag only
  through `Epa.Deactivate`/`Reactivate`, under `IEpaCreditLock.HoldForChangeAsync` taken before the EPA is read and
  committed with the save, reading the deactivation's moment only once it is held (T230; `EpaPauseWritePathTests`
  enforces the hold). Every picker, credit and progress reader applies it; the tool gate, the
  curriculum editor and scale-reference checks do not. An encounter observed after the trainee's last day
  (`TraineeProfile.EndedOn`, `IsAfterEnd`) credits nothing on that profile, live and in every replay (T281); it is
  kept, not refused at filing. Recording an end takes back credit already given after it, in the same save, by
  replaying that trainee against the unsaved end (`ProgrammeEndCredit`, `CurriculumProgressReplay`,
  `CreditSubject.PendingEnd`). Record an end only through `TraineeProfile.Complete`/`Deactivate`, under
  `ITraineeCreditLock.HoldForEndAsync` taken before the profile is read and committed with the save; the live
  completion and an EPA's reactivation hold the trainee with `HoldForCreditAsync` (`ProgrammeEndWritePathTests`
  enforces the end's hold).
- `ToolPermissionGate` — the write-path half of the EPA→tool allow-list (T122, D20): refuses an activity whose
  instrument (`ActivityType.WbaToolKey`) the matched curriculum item's `PermittedToolsJson` does not name, judged per
  credit directive: at create; on a change of target wherever credit can still follow; and for an unchanged target only
  when the author, before anyone else has acted, hands it on while still able to write its field. Never at credit, so a
  refusal never lands on someone who cannot act on it. The EPA picker narrows with the
  same predicate (`ToolPermission.Evaluate`). Null key or null list is unrestricted (D21). A type that credits
  nothing is judged by the EPA it is evidence for (`evidence_epa_field`), at the same moments (D45, T154).
- `NomineeGate` — a nominee field (every `user` field, plus any field a `field:` rule names) may name only someone
  `NomineeDirectory` lists: an active holder of the field's `role` (default `Assessor`) at the activity's stamped
  institution, never the subject, with no Administrator bypass (T102). A changed value is judged on every write; an
  unchanged one only at the author's hand-on, by the same predicate the tool gate uses (`UnchangedFieldsHandedOn`).
  The picker is the same query. `ActorFieldRules` (Domain) is the one walker of declared actor rules; save and publish
  refuse duplicate keys, a `field:` rule naming a non-user field, and options on a user field
  (`ActorFieldRules.EnsurePublishable`).
- `EncounterDateGate` — the encounter date (the pinned schema's `observation_date_field`) may not be after today on the
  South African calendar, for every type. A type whose pinned credit rules can credit (`EncounterDatePolicy.CanCredit`,
  meaning a non-empty `counts_for`) is also held to the subject's `ProgrammeStartDate`; with no profile, only the future
  check applies. A refusal is a field error on the date (T160). A changed date is judged on every write, and an unchanged
  one only at the author's hand-on (`UnchangedFieldsHandedOn`). The system-written MSF path gets the future check only.
  A filing more than `EncounterDatePolicy.LateFilingDays` (14, D15) after the encounter is **never refused**. On a type
  that can credit, the form warns while the date is typed, and the filing's history row records
  `ActivityTransition.DaysAfterEncounter`. A research output, journal club or reflective exercise gets neither. Only the
  first filing records it: a re-submission after a supervisor's `return` is not a filing (`Workflow.LeftInitialStateLeadingOn`
  is the shared test). "Today" comes from `ActivityService`'s `TimeProvider`, so a test can pin 22:30 UTC, which is
  already tomorrow in South Africa.

### Editing a seed folder

`DataSeeder` and `PaediatricCatalogueSeeder` **skip any activity-type key that already exists**, so editing
`Activities/Seeds/<key>/*.json` does nothing to a database that already holds that type.
`ActivityTypeSeedRefresher` (T103) closes that at startup: it canonicalises the on-disk seed and the stored
version through the DSL parsers, compares, and publishes a new version when they differ — provided the type
is still seed-owned, has no draft in flight, and its newest version was published by the seeder.

Three things follow:
- **A new seed folder must also be registered in `ActivityTypeSeedCatalogue`**, or the refresher will not
  see it.
- **Parse is not enough — a DSL property must be emitted in `Serialize` too.** `ActivityType.SaveDraft`
  round-trips Parse+Serialize, so a property with no `Serialize` half is dropped at publish with no error:
  the JSON parses, the builder shows the setting, the feature silently dies. `SeedRoundTripTests` guards
  this; add to it when you add a property.
- **In-flight activities stay pinned to their old version** and are not unblocked by a republish.
- `Wombat__RefreshSeededActivityTypes=false` disables the republish while still logging what differs.
- **`WbaToolKey` is not in a seed folder and is never refreshed.** It is declared on the type's
  `ActivityTypeSeedCatalogue` entry (required, so a new seed must say which instrument it is), written on create, and
  stamped on existing databases once by the T122 migration. The same holds for the catalogue's per-EPA tool lists,
  and for `SystemManaged` (T162: `msf_cpsa` is written only by the MSF release, so it is kept off the type picker and
  refused at a person's create; stamped once by the T162 migration).
- **A seeded curriculum item's values are written when a seeder creates the item, and on no later boot.** That is the
  tool list, the target and the scale pin for the v11.1 items (`PaediatricCatalogueSeeder`), and the scale pin for the
  demo IM Core item (`DataSeeder`). So no boot changes what a stored minimum means (T174), and an administrator's
  "Not pinned" survives a restart. The same holds for the Paediatrics sub-speciality's default entrustment scale:
  `PaediatricCatalogueSeeder` sets it only when it creates the sub-speciality, so an administrator's choice survives a
  restart too (T187). On existing databases the tool lists came from the T122 migration, the targets from T130's, and
  the demo item's pin from T174's; the v11.1 pins were stamped by the boots before T174, and the Paediatrics default by
  the boots before T187. A later change to any of these values, or to `WbaToolKey`, reaches an existing database only
  through a new migration. The seeders log a warning where the stored value differs.
- **`PaediatricCatalogueSeeder` finds its College, speciality, sub-speciality, v11.1 ladder, EPAs and curriculum by
  their `SeedKey` column (T221), and `DataSeeder` its Demo Institution, Demo College, discipline, O-R Scale, EPA-001 and
  IM Core curriculum by theirs (T229), never by a name or short code an administrator can edit; each creates its rows
  only on a database that holds none of them, in one save, and a row missing by its key is warned about and skipped,
  never created again.** So a new
  catalogue EPA, or a new `catalogueVersion` (which changes the ladder's and the curriculum's keys), reaches an existing
  database only through a migration, like every other seeded value; until then each boot announces it as missing.

Read `execution/architecture/CUSTOMIZATION.md` for the full model.

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

The canonical design contract is `execution/architecture/DESIGN.md`. Key rules:

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

# Infrastructure tests — seed integrity, activity runtime services, DSL round-trips.
# Do NOT skip this one: it is the second-largest suite and it guards the seed corpus.
dotnet test tests/Wombat.Infrastructure.Tests/Wombat.Infrastructure.Tests.csproj

# Architecture boundary tests
dotnet test tests/Wombat.Architecture.Tests/Wombat.Architecture.Tests.csproj

# bUnit UI smoke tests (after T010)
dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj
```

Always run architecture tests after adding any project reference — they guard layer
boundaries. Integration tests need a running PostgreSQL, not Docker: they read `WOMBAT_TEST_CONNECTION`, else
Wombat.Web's user secrets, and each test works in its own throwaway `it_<guid>` schema (ARCHITECTURE.md § Testing).

**Do not pass `--no-build` to `dotnet test` or `dotnet ef` in this repo.** `dotnet build Wombat.sln` and
the per-project tools resolve **different output paths**, so `--no-build` will happily load a stale
assembly. Measured 2026-09-20, and the direction is the opposite of what you would guess:

| Command | Resolves | Writes to |
|---|---|---|
| `dotnet build Wombat.sln -c Release` | `Platform=Any CPU` — the `.sln` maps every project's `Release\|x64` to `Release\|Any CPU` | `bin/Release/net10.0/` |
| `dotnet publish`/`test`/`ef` on a **`.csproj`** | `Platform=x64` (MSBuild's own default here; nothing in the repo sets it) | `bin/x64/Release/net10.0/` |

Both trees exist side by side and drift apart independently.

For `dotnet test` that means a green suite for code that
does not compile the behaviour you just wrote — it reports passes for tests your change should have
broken. For `dotnet ef migrations add` it is worse and quieter: the stale model matches the snapshot, so
EF finds no changes and writes an **empty migration** that compiles, ships, and does nothing. Always let
the tool do its own build, and always read a generated migration before trusting it.

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

- OS: **Ubuntu 26.04 LTS** on Linode. The plan specified 24.04; the box was built on 26.04,
  where `aspnetcore-runtime-10.0`, `postgresql` (18.x) and `caddy` all come from the **distro
  repos** — no Microsoft or Cloudsmith APT repo is needed. **Do not pin PostgreSQL 16:** a
  `pg_dump` 18 backup will not restore into a 16 cluster.
- Runtime: .NET 10
- Reverse proxy: Caddy (TLS + port forwarding)
- Database: PostgreSQL (local for Phase 1; managed later if justified)
- Process manager: systemd
- See `execution/architecture/INFRASTRUCTURE.md` for the full server layout.

### 🚨 Nothing is live — compatibility is not a constraint

`wombat.rcl.co.za` is **deployed and reachable, not in service**. It and the local dev database hold
**scenario-execution data only** — rows produced by replaying the `execution/knowledge/scenario-*.md` runbooks. There
are no real trainees, no real assessors, and no real clinical records anywhere.

**Therefore backward compatibility is not a design constraint.** Prefer the correct end state over the
migration-safe one:

- Destructive migrations, dropped columns, re-seeding from scratch and re-authoring seed JSON are all
  available. A backfill that would have to guess should be replaced by regenerating the data.
- Do not spend design effort on day-one lockout matrices, "who stops being able to work", or
  union-rather-than-conjunction hedges whose only purpose is preserving existing rows.
- Do not discount a defect's severity because live exposure is limited. There is no live exposure;
  severity is about whether the design is right.
- This licenses *changing* the scenario data, not skipping verification against it. It remains the
  test corpus, and a change still has to be shown working end to end.

Every open task file carries this rule as a preamble under its title, from `execution/tasks/_template.md` (W-007).
Delete this section, and that preamble from the template and every open task, the day Wombat takes on real users —
every decision above flips.

## Task management

Task state lives in `execution/`, managed by the harness — see the **Execution workspace**
section at the end of this file for the commands. `execution/STATE.md` and
`execution/HANDOFF.md` are imported into every session, and the SessionStart hook injects the
active task, so the reading path is already in front of you. Add `execution/architecture/DESIGN.md` if the
task touches any Razor file.

Work only on the active task. Move it with `harness.py task start|done|block` rather than
editing the file by hand — the lane, the `status` field and the mtime must stay in step, and
the mtime is what the Stop hook reads. If unexpected work surfaces, file a new task with
`harness.py task new` rather than mutating the current one; the register is append-only so
git history stays useful.

**Before finishing:** update `execution/HANDOFF.md` and run `harness.py lint`. The Stop hook
refuses to end a session that changed task state without writing a handoff.

**Commit after every completed task.** Do not accumulate multiple tasks in a single
uncommitted working tree.

## Reference folders

Both reference trees were deleted from the worktree on 2026-09-20. Nothing was lost —
each has a live source elsewhere, verified before the delete:

- **ClinicAssist.NET** — the reference architecture. The live working copy is at
  `C:\Users\Renier\ClinicAssist.NET` (remote
  `ssh://renier@rcl.co.za:10648/home/renier/ClinicAssist.NET`), and it is newer than the
  vendored snapshot ever was. Open it there when unsure about structure.
- **The old Wombat source** — it is in *this* repo's own history. Commit `55a92c6`, the
  parent of `c843421 "Scaffold new Wombat solution"`, holds the full old tree; all 556
  files were hash-compared against the deleted copy and matched.

  ```bash
  git show 55a92c6:Wombat.Web/Views/EPAs/EPA.cshtml    # one file
  git worktree add ../wombat-old 55a92c6               # the whole tree
  ```

  It is also at `github.com/reniercloete/Wombat`.


## Execution workspace

Live state, tasks and decisions are in `execution/`, managed by the `rcl-harness` script
at `bin/harness.py`. It is wired in through this project's own `.claude/settings.json`,
not a plugin: a SessionStart hook injects the state below before the first turn, and a
Stop hook refuses to end a session that changed task state without writing a handoff.

- `execution/STATE.md` — where the project is. Imported below, so it is always in context.
- `execution/HANDOFF.md` — what the last session left. Imported below.
- `execution/tasks/{queued,in_progress,blocked,done}/` — the work queue. The SessionStart
  hook injects the active task; you do not need to read the whole tree.
- `execution/DECISIONS.md` — what was decided and why, including what was rejected.
- `execution/DASHBOARD.md` — generated by `harness.py status`. Never hand-edit it.

Working rules:

- Continue a task in `in_progress/` unless HANDOFF says otherwise; else take the
  highest-priority unblocked task from `queued/`. Move it with the `task` command below.
- A task is done when every item in its **`## Verification`** section is backed by evidence:
  a test result, a file, a commit, a browser check. "Code exists" is not done. (67 of this
  repo's task files use `## Verification`; none uses the harness template's `done_when`
  frontmatter, so that field is optional here and `lint` does not ask for it.)
- Record a decision when reversing it would cost real work. Say what was rejected.
- Before the session ends, update `execution/HANDOFF.md`. The Stop hook blocks otherwise.
- Label statements as observed, inferred, or needs-confirmation when the difference matters.
- Keep project-specific material in this workspace. Moving anything outward is a deliberate,
  reviewed step, not a side effect of a session.

### Driving the harness

Run these as `python <harness>/bin/harness.py <command>` from the project root. **Prefer them
to editing `execution/` by hand** — they keep the lane, the status field and the file mtime
in step, and the mtime is what the Stop hook reads. The user should not have to name a command
or a task id: read the lanes, work out which task they mean, and run it.

| Command | Does |
|---|---|
| `task start <id>` | queued/blocked → in_progress; rewrites the status field |
| `task done <id>` | → done; stamps the completion date |
| `task block <id> --reason "..."` | → blocked; records what it waits on |
| `task new "<title>" [--priority P2] [--model sonnet]` | new task in queued/ from `tasks/_template.md` |
| `status` | regenerates `execution/DASHBOARD.md` (never hand-edit it) |
| `lint [--strict]` | bounds + consistency; exit 1 on a violation. Run it before finishing |
| `trim [--dry-run]` | moves overflow out of STATE/HANDOFF into `log/` |
| `context [--plain]` | the SessionStart bundle; the hook runs this, you rarely need to |

`<id>` matches a task id (`T-005`) or a filename prefix. `task start` refuses a fourth
concurrent task. Every command is a silent no-op with exit 0 outside a workspace.

@execution/STATE.md
@execution/HANDOFF.md
