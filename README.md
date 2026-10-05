![License](https://img.shields.io/github/license/RCL-Consulting/Wombat)
![GitHub last commit](https://img.shields.io/github/last-commit/RCL-Consulting/Wombat)

# Wombat — Work-Based Assessment Tool

**Wombat** tracks the training of medical specialists under competency-based medical education, built around
**Entrustable Professional Activities (EPAs)**. It holds each registrar's portfolio of workplace-based assessments,
counts them against the curriculum's targets, and supports the committee that decides each registrar's entrustment,
across colleges, institutions, specialities and sub-specialities.

It is deployed but **not yet in service**: every database holds scenario data only, replayed from the paediatrics
runbook (`execution/knowledge/scenario-paediatrics/`). There are no real trainees or clinical records anywhere.

## What it does

- **Activities, defined as data.** An institution or a College adds its own activity types (Mini-CEX, DOPS, CBD,
  portfolio reviews, reflections, procedure logs, research outputs …) in a visual builder. Each type carries its form
  schema, its workflow state machine and its curriculum credit rules as `jsonb`, interpreted at runtime by generic
  validators, a workflow evaluator and a credit applier. Publishing bumps a type's version; existing activities stay
  pinned to theirs.
- **Curricula and progress.** A College authors its catalogue (specialities, EPAs, entrustment scales, versioned
  curricula); an institution adopts it and may add its own items. Each registrar sees her targets for this semester
  and this academic year, her shortfalls, her rating trajectory per EPA and her standing against the College's
  training-year levels.
- **Assessment and review.** Registrars request assessments from named assessors, who rate them from an inbox. The
  committee sits on a schedule, reviews the evidence, and issues, renews or revokes entrustment decisions (STARs), with
  appeals.
- **Multi-source feedback**, run as campaigns with anonymous respondents.
- **Portfolio export** as PDF, with a verification page for third parties.
- **Data rights:** access, correction and erasure requests, decided by staff and audited.

### Roles

Administrator, CollegeAdmin, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator, CommitteeMember,
Assessor, Trainee and PendingTrainee. A person may hold several and acts as one at a time. Onboarding is by
invitation or institutional single sign-on (OIDC); there is no open registration.

## Technology

| | |
|---|---|
| Runtime | .NET 10, ASP.NET Core |
| UI | Blazor Interactive Server, with a custom design system (`src/Wombat.Web/wwwroot/app.css`); no CSS framework |
| Architecture | Clean Architecture with CQRS via MediatR 12; layer boundaries enforced by architecture tests |
| Data | PostgreSQL with EF Core 10 (Npgsql); migrations applied at startup |
| Identity | ASP.NET Core Identity; OIDC single sign-on per institution |
| PDF | QuestPDF |
| Hosting | Ubuntu 26.04 LTS on Linode, behind Caddy, run by systemd (`deploy/`) |

## Repository layout

```
src/
  Wombat.Domain/          entities, value objects, the activity DSL parsers
  Wombat.Application/     MediatR commands, queries, handlers, DTOs, validators
  Wombat.Infrastructure/  EF Core, Identity, email, the activity runtime, reporting, seeds
  Wombat.Api/             thin REST endpoints for webhooks and integration
  Wombat.Web/             the Blazor app
tests/                    Domain, Application, Infrastructure, Architecture, Integration and Web (bUnit) suites
deploy/                   server provisioning, deploy and verification scripts
design/                   the GUI redesign: brief, per-flow design records, the design system
execution/                project state, task register, architecture and domain documentation
```

Start with `execution/architecture/ARCHITECTURE.md` (how it is built), `execution/architecture/CUSTOMIZATION.md` (the
activity platform), `execution/knowledge/DOMAIN.md` (what EPAs, WBAs and STARs mean) and `CLAUDE.md` (conventions).

## Running it locally

Prerequisites: the .NET 10 SDK and a PostgreSQL server.

```bash
git clone https://github.com/RCL-Consulting/Wombat.git
cd Wombat

# The connection string lives in user secrets, never in a committed file
dotnet user-secrets --project src/Wombat.Web set "ConnectionStrings:DefaultConnection" \
  "Host=localhost;Database=wombat;Username=wombat;Password=<your local password>"

dotnet build Wombat.sln -c Release
dotnet run --project src/Wombat.Web/Wombat.Web.csproj      # http://localhost:5080
```

The database is migrated and seeded with a demo catalogue on first start. In the Development environment a set of
dev accounts is seeded as well (`src/Wombat.Infrastructure/Identity/DevUserSeeder.cs`); outside Development nothing of
the kind is created, and production startup fails if its connection string is missing.

## Tests

```bash
dotnet test tests/Wombat.Domain.Tests/Wombat.Domain.Tests.csproj
dotnet test tests/Wombat.Application.Tests/Wombat.Application.Tests.csproj
dotnet test tests/Wombat.Infrastructure.Tests/Wombat.Infrastructure.Tests.csproj
dotnet test tests/Wombat.Architecture.Tests/Wombat.Architecture.Tests.csproj
dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj
dotnet test tests/Wombat.Integration.Tests/Wombat.Integration.Tests.csproj   # needs PostgreSQL (WOMBAT_TEST_CONNECTION)
```

Do not pass `--no-build`: the solution build and the per-project tools write to different output paths, so it can run
a stale assembly (`CLAUDE.md` § Testing).

## Licence

Wombat is licensed under the [GNU Affero General Public License v3.0](LICENSE). Dependencies are limited to
GPLv3-compatible licences.
