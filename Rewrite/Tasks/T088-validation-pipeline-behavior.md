# T088 — Wire FluentValidation into the MediatR pipeline

**Status:** ✅ Done (2026-06-07)
**Found by:** clean v2 replay (Act 4 / T087 investigation).

## Problem

~75 FluentValidation validators live beside their commands and were registered
(`AddValidatorsFromAssembly`), but **no `IPipelineBehavior` ever invoked them** — only
`AuditPipelineBehavior` was in the pipeline. So command validators never ran; domain guards were the
only enforcement. This is what let the malformed Remitted appeal request reach the domain layer in
the replay (see T087 / F-4F-1).

## Fix

- New `src/Wombat.Application/Common/Behaviours/ValidationBehavior.cs` — runs all registered validators
  for a request before its handler; throws `FluentValidation.ValidationException` on failure. Validators
  run **sequentially** (not `Task.WhenAll`) because a validator may query the shared scoped
  `ApplicationDbContext`, which EF Core forbids concurrently.
- Registered in `DependencyInjection.cs` **after** `AuditPipelineBehavior` (so audit stays outermost and
  records validation failures) and **before** the handler (so invalid commands are rejected pre-handler).

## Tests

`tests/Wombat.Application.Tests/Common/ValidationBehaviorTests.cs` (uses the real
`ResolveAppealCommandValidator`): invalid command → `ValidationException`, handler not called; valid
command → handler called. Application 278→**280**.

## Verification

- Full solution build clean; Domain 50, Application 280, Architecture 19, Web 43 — all green
  (Integration not run — Docker). Note: Application handler tests construct handlers directly and bypass
  the pipeline, so they neither exercise nor are affected by the behavior.
- **Live smoke (validation active):** a valid `IssueInvitationCommand` succeeded (inline URL rendered);
  `/committee/reviews` rendered its 6 rows with no errors — confirms valid commands/queries are not
  falsely rejected at runtime. (Smoke-test invitation removed afterward.)

## Note

`ValidationException.Message` aggregates the failures; the Blazor pages already catch `Exception` and
surface `.Message`, so validation errors now display to the operator instead of falling through to a
domain exception.
