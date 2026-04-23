# T045 — Populated ReviewDetail + ActivityView verification

Follow-up item #2 from the post-T044 backlog: "Populated `ReviewDetail` and `ActivityView` browser verification. T039 and T041 both deferred the populated-data rendering check. Low-value unless a bug is actually suspected."

A bug was actually suspected. One was found.

## Findings

### ActivityView populated verification — blocked by seed-claims gap

Can't create any activity through the UI because `/activities/new`'s type selector is empty for every seeded user:

- `ListActivityTypesQuery` at `src/Wombat.Application/Features/Activities/Queries/ListActivityTypes/ListActivityTypesQuery.cs:36` filters by the current user's claims: `activityType.Scope == ActivityScope.Speciality && specialityIds.Contains(activityType.ScopeId ?? 0)`.
- The 10 seeded activity types are all Speciality-scoped at speciality id 1.
- Neither `admin@wombat.local` nor the `DevUserSeeder`-created `trainee@wombat.local` has a `SpecialityIds` claim stamped on their principal.
- Result: the selector renders with only "Select…" and no user can create an activity through the UI.

Same root cause as the existing `current_state.md` note about `GetTraineeDashboardSummaryQuery` returning `CurriculumProgress.Count == 0` for the seeded trainee. The issue is in the claims stamping at sign-in (or in the seeder not populating enough profile metadata), not in the rendering code.

**Verdict on ActivityView:** not a rendering regression. The T041 dual-error split is mechanical and the error-state path was verified at the time. Populated rendering remains unverified, but the gating is upstream of the view. Recorded as a separate finding (see handoff follow-up).

### ReviewDetail populated verification — blocker found, fixed, verified

Attempted to create a decision panel via `/committee/panels/new` as admin. The save threw a System.Text.Json error:

> A possible object cycle was detected. This can either be due to a cycle or if the object depth is larger than the maximum allowed depth of 64. Consider using ReferenceHandler.Preserve on JsonSerializerOptions to support cycles. Path: $.Claims.Subject.Claims.Subject.Claims.Subject…

**Root cause:** `AuditPipelineBehavior` writes a compact JSON summary of every MediatR command to `AuditEntry.SummaryJson` via `AuditPayloadSerializer`. Most committee commands (e.g. `CreateDecisionPanelCommand`, `ScheduleCommitteeReviewCommand`, `StartCommitteeReviewCommand`, `RecordCommitteeDecisionCommand`, etc.) carry an `Actor: ClaimsPrincipal` property for authorization checks. `ClaimsPrincipal.Claims[].Subject` is self-referencing, so reflecting over the command's properties into a dictionary and calling `JsonSerializer.Serialize` blew up on the first committee write path exercised.

This isn't a T041/T042 regression — the serializer has been there since the committee feature was built. It's also why populated `ReviewDetail` was deferred in T039: the seeding flow literally fails through the UI.

**Fix:** `AuditPayloadSerializer.Serialize` now checks whether a property type is `ClaimsPrincipal` / `ClaimsIdentity` and substitutes `"[PRINCIPAL]"` for the value. Actor identity is already captured on `AuditEntry` as `ActorUserId` + `ActorDisplay`, so the audit summary loses nothing.

Added regression test `AuditPayloadSerializerTests.Serialize_CommandWithClaimsPrincipal_ReplacesWithPrincipalMarker`.

**Verification after fix:**

1. Created panel "T045 Test Panel" (id=2) with the committee user as chair. Save succeeded, redirect to `/committee/panels/2` — form renders the Update-members shape as expected.
2. Scheduled a review against the panel (id=1) with the seeded trainee. Save succeeded, redirect to `/committee/reviews/1`.
3. Populated `/committee/reviews/1` rendered cleanly in state `Scheduled`: all 6 sections (Review summary, Decision form inactive, Pending entrustment decisions, Evidence snapshot empty, Rating trajectory empty, Appeals) render correctly. The T043 `details-list` utility applies correctly to the Review summary card with the dt/dd grid alignment.
4. Clicked **Start review**. Success Alert "Review started." rendered, state transitioned to `InProgress`, the Record decision button appeared on the Decision card, the Stage pending decision form appeared in the Pending entrustment decisions card. Confirms the transition pipeline works after the fix and conditional form rendering behaves correctly.

Full workflow (decision recording, ratification, appeals) not exercised — the rendering-under-load part of the verification is complete.

## Changes

- `src/Wombat.Application/Audit/AuditPayloadSerializer.cs` — add `ClaimsPrincipal` / `ClaimsIdentity` type check before `GetValue`, substitute the `[PRINCIPAL]` marker.
- `tests/Wombat.Application.Tests/Audit/AuditPayloadSerializerTests.cs` — add `Serialize_CommandWithClaimsPrincipal_ReplacesWithPrincipalMarker`.

## Out of scope

- Fixing the seed-claims gap that blocks `ListActivityTypesQuery`. Flagged as a follow-up.
- Full committee workflow (record decision → ratify → issue entrustment decisions). Rendering under each state is implied by the conditional-block structure already visible under `Scheduled` and `InProgress`.
- Any ActivityView populated verification.

## Definition of done

- Populated ReviewDetail rendering verified in browser under Scheduled and InProgress states.
- ActivityView populated verification: not attempted; upstream blocker documented.
- JSON-cycle bug in audit serialization fixed and regression-tested.
- Handoff backlog updated (item #2 closed, new follow-up added for the seed-claims gap).

## Files touched

- `src/Wombat.Application/Audit/AuditPayloadSerializer.cs`
- `tests/Wombat.Application.Tests/Audit/AuditPayloadSerializerTests.cs`
- `Rewrite/Tasks/T045-populated-rendering-verification.md` (this file)
- `Rewrite/current_state.md`
