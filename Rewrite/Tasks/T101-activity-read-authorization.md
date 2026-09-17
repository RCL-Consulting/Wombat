# T101 — Any authenticated user can read any activity's full DataJson by id

**Status:** open
**Surfaced:** 2026-09-17, mapping the activity transition pipeline for T070.
**Severity:** High (confidentiality) — WBA data is clinical performance data about a named trainee.

## Symptom

`GET /activities/{id}` returns any activity in the database to any signed-in user, including another
trainee's. There is no institution, speciality, subject or assessor check anywhere on the read path.

## Root cause

- `src/Wombat.Application/Features/Activities/Queries/GetActivityById/GetActivityByIdQuery.cs:7-19` —
  the request record is `(int ActivityId)`. It carries **no `ClaimsPrincipal`** and the handler calls
  `IActivityService.GetAsync` with a bare id.
- `src/Wombat.Web/Components/Pages/Activities/ActivityView.razor` is gated by a bare `[Authorize]` —
  authenticated, no policy, no ownership check.

Contrast with the T056 convention documented in `CLAUDE.md`: handlers reachable from scoped pages take
`ClaimsPrincipal Principal`, lists filter on `principal.GetInstitutionId()`, and get-by-id calls
`principal.CanAccessInstitution(...)` returning **null (404, not 403)** when out of scope, so the existence
of another institution's ids is not leaked. The activity read path never adopted it.

## What "authorised to read" should mean

An activity is legitimately readable by, at least:

- the **subject** (`Activity.SubjectUserId`) and the **creator** (`CreatedByUserId`);
- the **bound assessor** — the value of whatever field the workflow's actor rules point at
  (`field:assessor_user_id` for the CPSA seeds). Note this is data, not a column, so the check has to read
  `DataJson` the way `WorkflowEvaluator.cs:84-107` does;
- **Coordinators, CommitteeMembers, SpecialityAdmin / SubSpecialityAdmin, InstitutionalAdmin** within the
  activity's scope, and a global **Administrator** everywhere.

Deciding that list is the substance of this task; the plumbing is mechanical.

## Note for whoever does T070 first

T070 adds a `ClaimsPrincipal` to `GetActivityByIdQuery` for a *different* purpose — computing which fields
the caller may edit. **That is not a read gate**, and the T070 plan adds an explicit code comment saying so.
Do not read the presence of the principal as evidence this task is done.

## Verification

Sign in as trainee A, request `/activities/{id}` for an activity belonging to trainee B in another
institution: expect a 404-equivalent (not found), not the activity. Repeat as the bound assessor (allowed),
as an unrelated assessor (denied), as a Coordinator in scope (allowed) and out of scope (denied).

## Related

Sits directly under the page [T070] rebuilds. Same family as [T102].
