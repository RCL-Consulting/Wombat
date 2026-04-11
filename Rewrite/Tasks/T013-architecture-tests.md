# T013 — Architecture tests

**Phase:** 4 — Quality & ship
**Depends on:** T007 (earliest point the domain exists to test)
**Blocks:** T016

## Goal

Encode the layer rules from ARCHITECTURE.md as executable tests. Prevent regressions where Web reaches into Domain directly, or Infrastructure leaks into Application.

## What to do

1. In `tests/Wombat.Architecture.Tests/`, add xUnit tests using `NetArchTest.Rules` (or the helper ClinicAssist uses). Mirror ClinicAssist's test file structure.
2. Tests to implement:
   - `Domain_should_not_depend_on_EF` — no references to `Microsoft.EntityFrameworkCore`.
   - `Domain_should_not_depend_on_MediatR`.
   - `Domain_should_not_depend_on_AspNetCore`.
   - `Application_should_not_depend_on_EF_types` — may reference `IApplicationDbContext` only, not DbContext itself.
   - `Application_should_not_depend_on_AspNetCore` (except `ClaimsPrincipal` in extensions, which is fine).
   - `Infrastructure_should_not_be_referenced_by_Application`.
   - `Web_should_not_reference_EF_directly` — Web talks through Application.
   - `Api_should_not_reference_Domain_entities_directly` — Api exposes DTOs, never entities.
   - `CommandHandlers_should_end_with_Handler`.
   - `Commands_should_end_with_Command` and `Queries_should_end_with_Query`.
   - `Every_command_should_have_a_validator` — using reflection; skip command classes marked with a `[NoValidator]` attribute.
   - `Every_aggregate_root_should_be_sealed`.
   - `No_public_setters_on_domain_entities` — force mutations through methods. (Allow private setters, internal setters for EF, and init-only properties.)
3. Add a test that lists all MediatR handlers and confirms each one is registered in DI at boot. This catches "forgot to register" regressions. May need a small test-only `ServiceCollection` harness.
4. Add a test that confirms the DbContext's `OnModelCreating` runs all `IEntityTypeConfiguration` implementations in the Infrastructure assembly. Otherwise, adding a new entity config without wiring it up silently drops the mapping.
5. Make these tests run as part of `dotnet test`, so CI catches regressions on every push.

## Files created

- `tests/Wombat.Architecture.Tests/LayerTests.cs`
- `tests/Wombat.Architecture.Tests/NamingTests.cs`
- `tests/Wombat.Architecture.Tests/DomainInvariantTests.cs`
- `tests/Wombat.Architecture.Tests/RegistrationTests.cs`
- `tests/Wombat.Architecture.Tests/ModelConfigurationTests.cs`

## Verification

- [ ] `dotnet test tests/Wombat.Architecture.Tests` — all green.
- [ ] Deliberately break a rule (e.g. add `using Microsoft.EntityFrameworkCore;` to a Domain file) and confirm the test fails.
- [ ] Run against the full solution after each Phase 1–3 task to catch drift.

## Notes & gotchas

- Architecture tests are a safety net, not a substitute for review. They catch the common drift patterns; they will not catch a semantically wrong implementation.
- The "every command has a validator" test is the one most likely to annoy during development. Allow an opt-out attribute, but require an XML comment explaining why.
- NetArchTest has a learning curve. Budget time on the first test; subsequent tests are fast to write.
- Run these tests in parallel with domain tests in CI — they're fast and they catch the worst classes of bug.
