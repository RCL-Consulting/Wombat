# T014 — Seeding & first-run bootstrap

**Phase:** 7 — Quality & ship
**Depends on:** T002 (Identity), T004 (EPAs/Curricula), T018 (Activity engine), T020 (Activity type seeds)
**Blocks:** T015

## Goal

Ensure a fresh database becomes fully usable in one startup: roles seeded, first admin
account created, demo reference data present, activity types published. All operations
must be idempotent.

## Status: COMPLETE (implemented across T002, T018, T020, T024)

The full bootstrap pipeline was built incrementally. This task file closes the loop.

## What was implemented (and where)

### Startup pipeline — `Wombat.Web/Program.cs`

```csharp
await dbContext.Database.MigrateAsync();   // applies pending migrations
await roleSeeder.SeedAsync();              // creates the 9 Identity roles
await adminSeeder.SeedAsync();             // creates first admin from config
await dataSeeder.SeedAsync();              // reference data + activity types
```

Guarded by `--seed` flag: running `dotnet run --seed` exits after seeding,
allowing one-shot bootstrap without starting the web server.

### RoleSeeder — `Wombat.Infrastructure/Identity/RoleSeeder.cs`

Idempotent. Creates all 9 `WombatRoles` if they don't exist:
Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin,
Coordinator, CommitteeMember, Assessor, Trainee, PendingTrainee.

### AdminSeeder — `Wombat.Infrastructure/Identity/AdminSeeder.cs`

Idempotent. Creates the first Administrator user from:
- `Wombat:SeedAdminEmail`
- `Wombat:SeedAdminPassword`

Skips silently if either config value is blank (safe for production where the
admin was already created via invitation or SSO).

### DataSeeder — `Wombat.Infrastructure/Persistence/DataSeeder.cs`

Idempotent. Seeds on every startup, adds only what's missing:

| Data | Key check |
|------|-----------|
| Demo institution (`DEMO`) + speciality + sub-speciality | `ShortCode == "DEMO"` |
| O-R EntrustmentScale (1–5) | `Name == "O-R Scale"` |
| Demo EPA (`EPA-001`) | code + sub-speciality |
| Demo curriculum (`IM Core Curriculum v2026.1`) | name + version + sub-speciality |
| 20 procedure catalogue entries | `Key` |
| 10 activity types (Mini-CEX, DOPS, CbD, ACAT, STAR, Procedure Log, Research Output, Teaching Session, QI Project, Journal Club) | `Key` |

Activity types are loaded from JSON seed files under
`src/Wombat.Infrastructure/Activities/Seeds/{key}/{schema,workflow,credit}.json`,
which were written in T020.

### ScheduledJobDefinitions — auto-seeded by `ScheduledJobHost`

The background job host upserts `ScheduledJobDefinition` rows on first fire.
No separate seed step is needed.

## Configuration reference

Minimal `appsettings.json` / environment variables for a working first run:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=...;Database=wombat;Username=...;Password=..."
  },
  "Wombat": {
    "BaseUrl": "https://your-domain",
    "MsfRespondUrl": "https://your-domain/msf/respond",
    "SeedAdminEmail": "admin@your-org",
    "SeedAdminPassword": "ChangeMe!1"
  },
  "Email": { "SmtpHost": "...", "SmtpPort": 587, ... }
}
```

Never commit real credentials. Use `dotnet user-secrets` locally and
`EnvironmentFile=` in the systemd unit on the server (see T015).

## Verification

- [x] `dotnet build Wombat.sln -c Release` — clean
- [x] `dotnet test tests/Wombat.Architecture.Tests` — green (T013)
- [x] `dotnet test tests/Wombat.Application.Tests` — 122 passing
- [ ] Manual: run `dotnet run --project src/Wombat.Web -- --seed` against a fresh
      Postgres database. Confirm roles, admin, demo institution, and activity types
      are present. (Requires T015 environment; verified then.)
