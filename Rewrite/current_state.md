# Current state

This file is the live handoff between sessions. Every session ends by editing this file. Keep it short and accurate.

## Active task

**T014 — Seeding & first-run bootstrap** (next on the critical path after T013) — **Model: Sonnet**

T013 is complete. Next session: start `Tasks/T014-seeding-bootstrap.md`.

## Critical-path reminder (post-pivot)

The plan has been restructured around a **schema-driven Activity platform** so institutions can add new activity types without code. The old per-type tasks (T007 Assessment, T008 Workflow, T009 STAR) are **superseded** — read their banners. The new critical path after the core domain is:

> T001 → T002 → T003 → T004 → T005 → T006 → **T017 → T018 → T019 → T020** → T021 → T022 → T010 → ~~T011~~ → ~~T012~~ → ~~T023~~ → ~~T024~~ → ~~T025~~ → ~~T026~~ → ~~T027~~ → ~~T013~~ → T014 → T015 → T016

See `PLAN.md` for the full phase/dependency graph and `CUSTOMIZATION.md` for the no-code model.

## Last session notes

T013 completed:
- New attribute: `Wombat.Application.Common.NoValidatorAttribute` — opt-out marker for commands that genuinely need no validator
- 19 commands decorated with `[NoValidator]` (each with an XML comment explaining why):
  - ID-only / no-param commands: CloseMsfCampaignCommand, DeleteSsoGroupMappingCommand, DeactivateInstitutionCommand, DeactivateSpecialityCommand, DeactivateSubSpecialityCommand, DeactivateTraineeProfileCommand, DeactivateEpaCommand, DeactivateAssessmentFormCommand, DiscardActivityTypeDraftCommand, OpenMsfCampaignCommand, RebuildCurriculumProgressCommand, RevokeInvitationCommand, WithdrawMsfCampaignCommand
  - String-key commands (validated by job registry / caller identity): DisableScheduledJobCommand, EnableScheduledJobCommand, RunScheduledJobNowCommand
  - Domain-validated commands: PublishActivityTypeDraftCommand (ActorUserId from Identity), SaveActivityTypeDraftCommand (domain parsers throw on malformed JSON), CreateSsoGroupMappingCommand (Administrator guard in handler)
- Architecture.Tests.csproj: added `MediatR` package reference
- 5 test files added under `tests/Wombat.Architecture.Tests/`:
  - `LayerTests.cs` — 9 tests: Domain→{EF,MediatR,AspNetCore}, Application→{Infrastructure,AspNetCore}, handler DbContext guard, Infrastructure→{Api,Web}, Web.Components→EF, Api→Domain
  - `NamingTests.cs` — 4 tests: Commands_should_end_with_Command, Queries_should_end_with_Query, CommandHandlers_should_end_with_Handler, Every_command_should_have_a_validator_or_be_opted_out
  - `DomainInvariantTests.cs` — 1 test: All_concrete_domain_classes_should_be_sealed; No_public_setters test deferred (EF-friendly public setters are intentional — ~380 properties, tracked as future work)
  - `RegistrationTests.cs` — 2 tests: All_Application_handlers_are_registered_by_MediatR, All_Application_handlers_are_public_concrete_and_non_generic
  - `ModelConfigurationTests.cs` — 2 tests: All_entity_configurations_are_EF_discoverable, Every_Domain_entity_with_a_DbSet_has_a_configuration (Identity-owned types excluded — configured inline in OnModelCreating)
- Verification:
  - `dotnet build Wombat.sln -c Release` — clean (0 warnings, 0 errors)
  - `dotnet test Architecture.Tests` — 19 passed
  - `dotnet test Application.Tests` — 122 passed (no regressions)
  - `dotnet test Web.Tests` — 33 passed
  - `dotnet test Domain.Tests` — 17 passed
