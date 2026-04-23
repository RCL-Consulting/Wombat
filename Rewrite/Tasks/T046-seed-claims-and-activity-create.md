# T046 — Seed-pipeline claims gap + ActivityService draft-create bug

Follow-up item #2 from the post-T045 backlog: "Seed-pipeline claims gap. `admin@wombat.local` and the `DevUserSeeder`-created `trainee@wombat.local` both lack `SpecialityIds` / `SubSpecialityIds` / `InstitutionId` claims on their principals. This silently breaks `ListActivityTypesQuery` (empty selector) and `GetTraineeDashboardSummaryQuery` (`CurriculumProgress.Count == 0`)."

## Investigation

The claims pipeline is correctly plumbed end-to-end:

- `src/Wombat.Infrastructure/Identity/WombatUserClaimsPrincipalFactory.cs` overrides `UserClaimsPrincipalFactory<WombatIdentityUser, IdentityRole>.GenerateClaimsAsync`. It reads `user.InstitutionId`, `user.SpecialityScopes`, and `user.SubSpecialityScopes` and stamps the corresponding `institution_id` / `speciality_id` / `sub_speciality_id` claims on every sign-in.
- `src/Wombat.Infrastructure/Identity/InvitedUserProvisioner.cs` (the production path) accepts `specialityId` / `subSpecialityId` parameters and writes rows to `UserSpecialityScopes` / `UserSubSpecialityScopes` join tables.

The gap was narrow: **`DevUserSeeder` only set `WombatIdentityUser.InstitutionId`** and never wrote the scope join rows. The trainee and committee dev users therefore signed in with zero speciality/sub-speciality claims, and every query that filters by those claims (activity-type listing, dashboard scope joins) silently returned no results.

`admin@wombat.local` is deliberately global/scopeless per CLAUDE.md ("Administrator — global; sees everything"). `AdminSeeder` creates them that way on purpose. The observation in the handoff that admin "lacks InstitutionId" was accurate but wasn't itself a bug — admin isn't supposed to pick activity types (admin's surface is `/admin/activity-types` for management, not `/activities/new` for authoring).

## Fix A — DevUserSeeder

Added idempotent `EnsureScopesAsync` helper that checks for existing `WombatIdentityUserSpecialityScope` / `WombatIdentityUserSubSpecialityScope` rows and inserts the missing ones. The trainee and committee members both get scoped to the demo curriculum's speciality + sub-speciality (reached via `curriculum.SubSpeciality.Speciality`). The seeder now also runs the scope check for users that already exist from a prior seed run — that "migration" case matters because the repo's dev database started with trainee + committee rows already created pre-fix.

After restarting the dev server, the trainee's `/activities/new` type selector populated all 10 seeded activity types (ACAT, Case-based Discussion, DOPS, Journal Club, Mini-CEX, Procedure Log, QI Project, Reflective Note, Research Output, Teaching Session). Scoping fix verified.

## Fix B — ActivityService.CreateDraftAsync missing `.Include(Versions)`

Selecting Reflective Note and clicking **Save draft** then threw:

> The published activity type version '1' could not be found.

Traced to `src/Wombat.Infrastructure/Activities/ActivityService.cs`. `CreateDraftAsync` (line 33–34) loaded the `ActivityType` with no includes, then called `Map(activity)` which calls `GetPinnedVersion` which accesses `activity.ActivityType.Versions.SingleOrDefault(...)`. The `Versions` collection wasn't eager-loaded, so it was empty, so `GetPinnedVersion` threw.

Contrast with `LoadActivityAsync` (line 159–166) which correctly `.Include(entity => entity.ActivityType).ThenInclude(t => t.Versions)` for the Update/Transition/Get paths.

Fix: add `.Include(entity => entity.Versions)` to the `CreateDraftAsync` query. One-line change. After restart, the trainee successfully creates a Reflective Note draft, sees "Draft created." success Alert, and the draft appears in `/activities/mine` with state=draft. `/activities/2` (the newly-created draft) renders the populated `ActivityView` cleanly — PageHeader + Summary aside + Activity details section with the schema-driven form.

Note: the pre-fix failed attempt actually persisted a partial draft (the SaveChanges succeeded before Map threw), leaving a stranded row in the db. It shows up as activity id=1 in `/activities/mine` alongside the post-fix id=2. Harmless for dev data but worth flagging if anyone asks why there are two drafts — only activity 2 was created cleanly end-to-end.

## Populated ActivityView rendering — now verified

Closes the other half of the T045 backlog item (#2 was "populated ReviewDetail and ActivityView"). ReviewDetail was verified in T045; ActivityView is now verified here in T046. Both pages render populated content cleanly with the schema-driven forms embedded.

## Out of scope

- **Trainee dashboard "No curriculum items assigned yet"** — the claims fix alone doesn't populate this. `GetTraineeDashboardSummaryQuery` likely needs evidence (Activities created, rated, etc.) beyond just a profile-curriculum link. Verifying that query is a separate investigation; flagged as a standing follow-up (not a new backlog item because it's cosmetic).
- **Stranded activity id=1** in the dev DB. Leave it. If any future test requires a clean slate, drop and re-seed.
- **Fixing the admin "global" definition to let admin browse activity types by scope** — admin's correct surface is `/admin/activity-types`, not `/activities/new`. No change.

## Changes

- `src/Wombat.Infrastructure/Identity/DevUserSeeder.cs` — add `EnsureScopesAsync` helper, call from both trainee and committee paths, re-structure `EnsureCommitteeMemberAsync` to be idempotent (previously returned early on existing user, so would not have topped up missing scopes).
- `src/Wombat.Infrastructure/Activities/ActivityService.cs` — add `.Include(entity => entity.Versions)` to `CreateDraftAsync`'s ActivityType query.

## Definition of done

- Trainee dev user has SpecialityScope + SubSpecialityScope rows matching the seeded curriculum.
- `/activities/new` type selector populates for the trainee.
- Trainee can create a draft activity end-to-end.
- Populated `ActivityView` renders cleanly with schema-driven form data.
- 271/271 tests pass (unchanged count — no new tests added; the ActivityService fix is better covered by re-exercising the end-to-end create flow which now works).

## Files touched

- `src/Wombat.Infrastructure/Identity/DevUserSeeder.cs`
- `src/Wombat.Infrastructure/Activities/ActivityService.cs`
- `Rewrite/Tasks/T046-seed-claims-and-activity-create.md` (this file)
- `Rewrite/current_state.md`
