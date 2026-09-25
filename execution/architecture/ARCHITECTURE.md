# Architecture

> **⚠ Amended by `CUSTOMIZATION.md`.** The ClinicAssist layout is still the baseline, but Wombat adds a schema-driven Activity platform that ClinicAssist does not have. Do not copy ClinicAssist on anything that touches the activity model — that is the deliberate departure. See `CUSTOMIZATION.md` and the "Activity platform" section at the end of this document.

The rewrite follows the ClinicAssist.NET layout almost exactly. When in doubt, open the reference project and copy the shape — it is no longer vendored into this repo, so open the live working copy at `C:\Users\Renier\ClinicAssist.NET`. This document captures the non-negotiables and the decisions that don't fall out naturally from "do what ClinicAssist does".

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
- An address no page claims: `Program.cs` reruns a browser's 404 as `/not-found` (`Navigation/NotFoundPages.UseWombatNotFoundPage`, T233), still answered 404, so a signed-in user gets `Pages/NotFound.razor` in the main layout instead of the browser's own error page. Only a GET that accepts `text/html`, for an address whose last segment has no dot. Any other status (the circuit's 401, a 400, a 405), any other request (a script's `fetch`, a form's post, a HEAD, which every page answers with 405) and a missing static file stay bare, and a response that already has a body is never rerun: the MSF respondent page's refusals keep their text because it sets its status after its body is written. A visitor who has not signed in is sent to sign in first, by the fallback policy. In a circuit the router renders the same page through its `Found` fragment, so the layout is the same.

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
- An in-process channel queue (`System.Threading.Channels.Channel<QueuedEmail>`) and a hosted `EmailWorker : BackgroundService` that drains it. Keeps web requests snappy.
- **A send is a hand-off, not a delivery** (T251). `QueuedEmailSender` never fails for want of a mail server, so a request
  cannot say whether its mail arrived. A mail that needs to know carries an `EmailMessage.DeliveryKey`, and the worker
  reports its outcome (sent on attempt N, or dropped after three attempts, or still queued as the host stops) to every
  `IEmailDeliveryObserver` on a scope of its own. A mail cut off mid-send by the host stopping is not reported: it may
  have arrived (T251 review). The key is never a tag and never logged, since it names the respondent's invitation.
  An MSF link's key names its invitation and link (`MsfInvitation.DeliveryKey`), and
  `MsfLinkDeliveryRecorder` writes the outcome onto the invitation in one conditioned `UPDATE`, which may land before the
  request that sent the mail has stored the link, so it names the link it is about and touches no column the request
  writes. Account invitations, nudges and digests ask for nothing yet.
- Templates stored as Razor files under `Wombat.Infrastructure/Email/Templates/` and rendered with `Microsoft.Extensions.RazorTemplating` or a hand-rolled renderer — whichever ClinicAssist uses.
- SMTP settings come from `appsettings.json` / environment variables, never from the database (simpler to audit, simpler to rotate).

## PDFs (if kept)

The current Wombat does not appear to generate PDFs heavily. Decide per task whether to pull in QuestPDF for STAR reflection exports and assessment summaries. Defer until there is an actual need — do not add it speculatively.

## Logging & diagnostics

- Serilog, configured in `Program.cs` of `Wombat.Web` and `Wombat.Api`. Same configuration as ClinicAssist.
- Sinks: Console (always), File (rolling daily, in `/var/log/wombat/` on the server), and optionally Seq if a Seq instance is reachable.
- Correlation IDs on every request.
- No PII in logs. User IDs yes, email addresses no.
- **A mail is logged by its tags and a reference, never by an address (T282).** `QueuedEmailSender` queues each mail as a
  `QueuedEmail` under a reference drawn at random (`EmailLog.NewReference`, 12 hex digits), and the enqueue line and
  every line the worker writes about it (sent on attempt N, retrying, dropped, cut off or abandoned at a stop, outcome
  not recorded) carry the same reference and the mail's tags. The reference follows one mail, not one person: one
  derived from the address, even a keyed hash, would join an MSF respondent's invitation and reminders to the mail the
  same person gets as a user, and name them as surely as the address. The subject is not logged either (an MSF mail's
  names the trainee). An exception about a mail is logged through `EmailLog.Redact`, which renders it as thrown, inner
  exceptions and stack included, less every form of the mail's addresses: a mail server's refusal usually quotes the
  address, and MailKit makes that reply the message. "Every form" is each address as given, with its domain in Unicode
  and in the ASCII form MailKit sends to a server without UTF-8, and its local part where that stands alone (sendmail's
  `<nurse-a>... User unknown`), if it has at least `EmailLog.ShortestRedactedLocalPart` (3) characters. A test sends
  through the real `MailKitEmailSender` to a loopback server that refuses the recipient, so the redaction is proved on
  MailKit's own `SmtpCommandException`. `MailKitEmailSender` logs nothing. `LoggingEmailSender`, the
  no-SMTP fallback, prints the subject and text but not the address. The SSO path logs a failed Identity call by its
  error codes, never their descriptions, which quote the value refused ("Email 'x' is already taken").
- **Enforced by `Wombat.Architecture.Tests/NoAddressInLogsTests` (T282).** It reads the compiled IL of the five `src`
  assemblies, so Razor components, async state machines, lambdas and `[LoggerMessage]` code are covered. Its log calls
  are `LoggerExtensions.Log*`/`BeginScope` and `LoggerMessage.Define*`, whose templates it reads; a call to a
  `[LoggerMessage]` method, or to the delegate a `Define*` returned, whose template it reads where it is declared; and a
  direct `ILogger.Log`. It refuses:
  - a template that names an address placeholder (`{To}`, `{Email}`, `{RespondentEmail}`, `{UserName}`, which Wombat
    sets to the address) or quotes an address, in each arm of a template chosen by a conditional;
  - anything a call's arguments evaluate, from the logger on, so the exception and event id as well as the values, that
    reads an address-named property, field, parameter or local, calls a method whose name says it returns one
    (`GetEmailAsync`, `GetUserName`), reads `IIdentity.Name` (the user name, which is the address) or looks up an
    address claim (`ClaimTypes.Name`, `Email`, `Upn`, `email`, `preferred_username`, `upn`, `unique_name`), or passes
    an address literal or a whole object with an address property;
  - any call it cannot read: a template that is not a constant (an interpolated string, a `static readonly` field), values
    in an array built before the call, or a direct `ILogger.Log`.

  A value computed out of sight, in another method or before an `await` inside the call, is not seen; the template check
  and review cover that. `LogAddressSpecimens` holds one bad call for each check, and seven clean ones it must pass:
  counts and a date (`{NoEmailCount}`, a `DateOnly to`, a count per reason named `skippedRecipients`), a mail's tags and
  subject, an exception, a claim type logged as a value, a template chosen between two clean ones, and a
  `[LoggerMessage]` method and a `Define`'s delegate given a reference.

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
- bUnit waits in `Wombat.Web.Tests` (T244). bUnit's `WaitForAssertion` and `WaitForState` give up after one second by default. A test whose fakes answer with completed tasks renders on its own thread, so its waits pass at their first check and need no more. A test that releases a task the page is awaiting (a held command, a gated load) waits for a render that comes from a thread-pool thread, and on a busy machine that has taken over a second. Such a test derives from `WombatTestContext` (namespace `Wombat.Web.Tests.TestSupport`) and passes its `AsyncWorkTimeout` (5 s) to each wait that follows the release, and to no other wait. First check that the page awaits the task: a longer timeout would also pass a render that is late because the page started work it never awaited. To find the waits that need it, set `TestContextBase.DefaultWaitTimeout` and `AsyncWorkTimeout` to 1 ms and run the suite several times with the CPU busy (20 spinning shells on a 16-core machine): a wait that fails is one that waits for a thread-pool render. An idle run is not enough: on an idle machine the hop often beats even 1 ms, and the wait after the stale first load in `NomineePickerTests` passed all five idle runs but failed five of six busy ones.
- Integration tests (`Wombat.Integration.Tests`): a real PostgreSQL server, not Testcontainers (T241 removed the unused `Testcontainers.PostgreSql` and `Respawn` references, and with them the `SSH.NET` security pin). Every class reaches the server through `TestSupport/TestDatabase`, which reads the connection string once, from `WOMBAT_TEST_CONNECTION` or else `Wombat.Web`'s user secrets (`ConnectionStrings:DefaultConnection`). Each test works in a new `it_<guid>` schema (`TestDatabase.NewSchemaName`), the only schema on its search path (`TestDatabase.SchemaConnectionString`). A class holds a `TestSchemas`, which registers each schema before creating it and drops every one with `DROP SCHEMA … CASCADE` from `DisposeAsync`, and in most classes also from the test's `finally`. Most classes test migrations and Postgres-only behaviour; three host the app with `WebApplicationFactory` and create and drop their schema through `TestDatabase` themselves, because the host must outlive a test's `finally`: `MsfRespondEndpointFlowTests`, a schema per test, and the classes on the `MsfRespondPageFlowTests.WebHost` fixture, `MsfRespondPageFlowTests` and `Hosting/NotFoundPageFlowTests` (T233). xUnit gives each of those two classes its own copy of the fixture, and each copy works in one schema for the whole class, so the tests of one class share it. That host maps the static-asset endpoints a publish does (`ReloadStaticAssetsAtRuntime=false`): a build's manifest also maps a development `{**path:file}` GET fallback the server does not have, whose route matches every address before its constraint is checked, so it answers a POST to an unknown address with 405 and a GET of a POST-only address with 404.
- Connections in those tests (T241). The admin connection, which creates and drops schemas and reads `pg_stat_activity` (`OpenAdminConnectionAsync`), is unpooled. xUnit runs the classes in parallel in one process, and a pooled connection that a test disposes stays open on the server, idle, until Npgsql prunes it minutes later; beside the tests' open connections and other processes on the same dev server, those can pass `max_connections` (53300). Measured on a private PostgreSQL 18: 32 connections at peak and 22 on average with the old pooled admin connections, 16 and 6 without them. A schema's connections (`SchemaConnectionString`, `OpenSchemaConnectionAsync`) are pooled instead, one pool for each string handed out (an `applicationName` makes another), and `TestDatabase.DropSchemaAsync`, which `TestSchemas` and the web-host fixtures call, clears the schema's pools, so no connection outlives its schema. Unpooled, every EF Core save or query was a new connection, and on Windows each one closed holds a local TCP port for about two minutes. Against a server on the same machine, such as the `postgres:18` container below, a run used up Windows' 16,384 ports and tests failed with "Only one usage of each socket address". Measured on 18.6 in Docker with 264 tests on 2026-09-25: unpooled, 14,956 connections, 9,037 ports left in TIME_WAIT and 3 tests failed, with at most 17 connections open at once (5.3 on average); pooled, 723 connections and none failed, with at most 20 open (8.7 on average). `TestDatabaseTests` fails if any other type reads `WOMBAT_TEST_CONNECTION`, the user-secrets id or the local default, sets `NpgsqlConnectionStringBuilder.Pooling`, or builds an `NpgsqlConnectionStringBuilder` at all: a string derived from one `TestDatabase` handed out is a pool it cannot clear. It reads IL, so it also sees async state machines and lambdas. It also checks that a disposed admin connection leaves `pg_stat_activity`, that a schema connection is reused and then closed when its schema is dropped, and that only a name `NewSchemaName` could have made is ever created or dropped.
- Which PostgreSQL version (T243). Production runs 18.x; the dev server that the user secrets name was 16.10 on 2026-09-25. The two differ in at least one error code. A delete refused by an `ON DELETE RESTRICT` foreign key, which is what EF Core's `DeleteBehavior.Restrict` creates, is `23001` (`restrict_violation`) on 18 and `23503` on 16 and 17 (all three measured). A missing referenced row and a `NO ACTION` key are `23503` on every version. So never test or catch a foreign-key refusal by one code. Code asks `PostgresErrors.IsForeignKeyViolation` (Application, `Common/Persistence`), which reads the whole `InnerException` chain, so a refusal a handler has already translated (`CreateDecisionPanel` throws an `InvalidOperationException` carrying the `DbUpdateException` it translates) is still recognised above it. A test asserts `SqlState.Should().BeOneOf(PostgresErrors.ForeignKeyViolationStates)`. `ForeignKeyErrorCodeTests` (Architecture) enforces the rule for `src`: it fails when any type in Domain, Application, Infrastructure, Api or Web other than `PostgresErrors` loads the string `23503` or `23001` or declares it as a constant, which also catches Npgsql's `PostgresErrorCodes.ForeignKeyViolation`, since a `const string` is compiled into the caller as its literal. `ForeignKeySqlStatePostgresTests` pins each code against the server's own `server_version_num`. A green run on dev therefore says nothing about 18. To run the suite on production's version, start a container and point the suite at it: `docker run -d --name pg18 -e POSTGRES_PASSWORD=<pw> -p 55418:5432 postgres:18`, `createdb` a database, and set `WOMBAT_TEST_CONNECTION=Host=127.0.0.1;Port=55418;Database=<db>;Username=postgres;Password=<pw>`. `deploy/verify/drift-check.sh` prints production's version.
- Catalog queries in those tests (T227). Every class shares the one server and drops its schemas in parallel with the others, so a catalog query can reach another test's schema while that schema is being dropped. `pg_get_indexdef`, `pg_get_constraintdef` and `pg_get_expr` lock the table they render. Run on a table that is being dropped, they wait for the drop, then fail with XX000 "could not open relation with OID" when it commits. The view columns built on them count too: `pg_indexes.indexdef` and `information_schema.columns.column_default`. So filter on plain catalog columns, and use these functions and columns only in the select list, never in the WHERE clause. A subquery that renders them in its own select list is safe only while Postgres can push the outer filter into it. It can for a UNION (`information_schema.check_constraints`) or a DISTINCT. It cannot below a window function or a LIMIT, so there the subquery renders rows from every schema before the outer filter sees them. `TestSupport/Catalog` asks the common questions this way (a table or column exists, a column's type, an index's names, whether it is unique, the unique indexes with their definitions): each names its table by OID through `to_regclass`, which looks up one name on the schema's search path and takes no lock. Use it before writing a catalog query of your own. `TestDatabaseTests` runs every helper while another schema's drop is held open under a lock timeout.

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
