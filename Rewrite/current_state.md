# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T015 — Linode deployment** (next on the critical path after T014) — **Model: Sonnet**

T013 and T014 are both complete. Next session: start `Tasks/T015-linode-deployment.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → ~~T012~~ → ~~T023~~ → ~~T024~~ → ~~T025~~ → ~~T026~~ → ~~T027~~ → ~~T013~~ → ~~T014~~ → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

### T013 — Architecture tests (commit `99a22ca`)

- `NoValidatorAttribute` added to `Wombat.Application.Common`
- 19 commands annotated with `[NoValidator]` (all with XML comments)
- 5 test files, 19 tests, all green:
  - `LayerTests.cs` (9 tests) — layer boundary enforcement
  - `NamingTests.cs` (4 tests) — CQRS naming + validator coverage
  - `DomainInvariantTests.cs` (1 test) — sealed domain classes
  - `RegistrationTests.cs` (2 tests) — MediatR handler DI registration
  - `ModelConfigurationTests.cs` (2 tests) — EF config coverage
- Architecture.Tests.csproj gained `<PackageReference Include="MediatR" />`
- All other test suites unaffected (Application 122, Web 33, Domain 17)

### T014 — Seeding & first-run bootstrap (no new code)

- Implementation was completed progressively across T002, T018, T020, T024
- `RoleSeeder`, `AdminSeeder`, `DataSeeder` all wired and idempotent
- `Program.cs` runs migrate → seed on every startup; `--seed` flag exits after seeding
- Task file created at `Tasks/T014-seeding-bootstrap.md` documenting the full picture
- Manual verification deferred to T015 (requires a running Postgres target)

## T015 prerequisites

Before starting T015, read:
- `Rewrite/INFRASTRUCTURE.md` — Linode layout, systemd unit, Caddy config
- `Rewrite/Tasks/T015-linode-deployment.md` (if it exists; create from INFRASTRUCTURE.md if not)
- `src/Wombat.Web/appsettings.json` — config keys to fill in on the server
