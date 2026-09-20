# T056 — InstitutionalAdmin role-power audit (Option A, cluster-incremental)

Closes the "hard finding" from the 2026-05-24 Act 1 play-through: an `InstitutionalAdmin`
user could provision their own account but was locked out of every admin route except
`/admin/entrustment-decisions`. The scenario's premise — Prof Mbatha (as
`InstitutionalAdmin`) owns the rest of the institution setup after Phase 1.B — could not
be tested. Phases 1.C–1.F had to fall back to the bootstrap Administrator.

This task implements **Option A**: grant institution-scoped admin powers to
`InstitutionalAdmin` across the admin surface. Three pages remain Administrator-only
(scheduled jobs, the institutions list/create page, and data-rights requests); the rest
become accessible to a holder of the role, with handler-level filtering so they can only
see/edit data scoped to their institution.

## Scope and landing strategy

Per the 2026-05-24 session, **T056 is landing in clusters** rather than as one monolithic
commit. Each cluster:

- Adds scope guards to every query/command for a single feature group.
- Updates every Razor call site (page + picker users) for that group's queries.
- Swaps the page-level `[Authorize]` over to the combined
  `AdministratorOrInstitutionalAdmin` policy.
- Ships its own scope-guard tests.
- Builds clean + all existing tests pass.

Until the final cluster lands, pages whose feature group has not yet been guarded stay
on the original `[Authorize(Policy = "Administrator")]` to preserve safety.

### Cluster status

| Cluster | Scope | Status |
|---------|-------|--------|
| **T056.a** | Foundations + Institutions / Specialities / SubSpecialities | ✅ shipped (commit `41def8a`) |
| **T056.b** | EPAs + Curricula | ✅ shipped (commit `9e3bc0a`) |
| **T056.c** | ActivityTypes + Forms | ✅ shipped (commit `e1d3737`) |
| **T056.d** | Trainees + Assessors + Invitations + EntrustmentScales | ✅ shipped (commit `8ad0788`) |
| **T056.e** | Audit + SSO + NavMenu refresh + scenario-doc revert | ✅ shipped this session |

## Design decisions (locked in by user 2026-05-24)

### New authorization policy

A new combined policy `AdministratorOrInstitutionalAdmin` is added to
`AuthorizationPolicies.cs` alongside the existing per-role policies. It accepts callers
in **either** the `Administrator` or `InstitutionalAdmin` role. Page `[Authorize]`
attributes that gate "the admin surface" switch from the bare `"Administrator"` policy
to this combined one, cluster by cluster; handler code is always the second line of
defence.

### Page taxonomy

| Page route                              | Before                          | After                                  | Cluster | Notes |
|-----------------------------------------|---------------------------------|----------------------------------------|---------|-------|
| `/admin/institutions`                   | Administrator                   | **Administrator only (unchanged)**     | T056.a  | Listing/creating institutions is a global act |
| `/admin/institutions/{id}` (edit)       | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.a  | InstitutionalAdmin can only edit their own; handler returns 404 otherwise |
| `/admin/specialities`                   | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.a  | List filtered to caller's institution |
| `/admin/institutions/{id}/specialities` | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.a  | List filtered by handler |
| `/admin/institutions/{id}/specialities/{id|new}` | Administrator          | AdministratorOrInstitutionalAdmin      | T056.a  | Create only under caller's institution; edit-own |
| `/admin/specialities/{id}/sub-specialities` | Administrator              | AdministratorOrInstitutionalAdmin      | T056.a  | Filtered by speciality scope |
| `/admin/specialities/{id}/sub-specialities/{id|new}` | Administrator     | AdministratorOrInstitutionalAdmin      | T056.a  | Edit-own scope |
| `/admin/epas`                           | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.b  | List filtered to Global + caller's institution |
| `/admin/epas/{id|new}` (edit)           | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.b  | Create with scope inside caller's institution; edit-own |
| `/admin/curricula`                      | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.b  | List filtered to Global + caller's institution |
| `/admin/curricula/{id}` (edit)          | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.b  | Edit-own |
| `/admin/curricula/{id}/items`           | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.b  | Items inherit curriculum scope |
| `/admin/activity-types`                 | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.c  | List filtered to Global + caller's institution |
| `/admin/activity-types/{id|new}`        | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.c  | Create with scope inside caller's institution; edit-own |
| `/admin/forms`                          | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.c  | Filtered to caller's institution |
| `/admin/forms/{id}`                     | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.c  | Edit-own |
| `/admin/invitations`                    | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.d  | List + issue scoped to caller's institution; `Administrator` target role still requires the caller to be Administrator |
| `/admin/trainees`                       | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.d  | Pending trainees filtered to institution |
| `/admin/trainees/{id}`                  | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.d  | Edit-own scope |
| `/admin/assessors`                      | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.d  | Filtered |
| `/admin/assessors/{id}`                 | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.d  | Edit-own scope |
| `/admin/entrustment-scales`             | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.d  | List filtered to Global + caller's institution; Global readable, edit blocked at handler |
| `/admin/entrustment-scales/{id|new}`    | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.d  | Create only with scope inside caller's institution; edit-own |
| `/admin/audit`                          | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.e  | List filtered to caller's institution (Actor.InstitutionId join) |
| `/admin/audit/{id}`                     | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.e  | 404 if out-of-scope |
| `/admin/sso/group-mappings`             | Administrator                   | AdministratorOrInstitutionalAdmin      | T056.e  | List + create scoped to caller's institution |
| `/admin/jobs`, `/admin/jobs/runs`       | Administrator                   | **Administrator only (unchanged)**     | —       | System infrastructure |
| `/admin/data-rights`                    | Administrator,Coordinator       | **unchanged**                          | —       | Cross-institution feature; not in T056 scope |

`/admin/entrustment-decisions` is already inclusive (`Administrator,InstitutionalAdmin,
SpecialityAdmin,SubSpecialityAdmin`) and stays that way.

### Scope-guard pattern

Two layers of defence:

1. **Page-level `[Authorize(Policy = "AdministratorOrInstitutionalAdmin")]`.** Gates the
   route at all. A `Trainee` or `Coordinator` still gets 403 from this layer.

2. **Handler-level scope filter / rejection.** Every list/get/create/update/delete
   reachable from a scoped page accepts a `ClaimsPrincipal` (added to the
   query/command record signature) and:
   - **Lists**: if caller `IsAdministrator()` → no filter. Else (InstitutionalAdmin) → add
     a `Where(... .InstitutionId == caller.GetInstitutionId())` clause, plus `OR
     scope == Global` for entities that have a Global option.
   - **Get-by-id**: if caller `IsAdministrator()` → return as-is. Else, after fetch,
     verify `entity.InstitutionId == caller.GetInstitutionId()` (or Global). If not,
     return `null` (the page already treats null as "not found"). Treating it as 404
     rather than 403 avoids leaking that a different-institution id exists.
   - **Create**: assert the requested scope/institution is within the caller's
     institution. If not, throw `UnauthorizedAccessException`.
   - **Update/Delete**: load the entity, check scope, throw if out-of-scope.

A helper extension `ClaimsPrincipal.CanAccessInstitution(int)` returns true iff the
caller is Administrator or holds `InstitutionalAdmin` for that institution. Handlers use
it on every entry path.

### Architecture tests

Per user choice (pragmatic option): no reflection-based enforcement that every handler
calls a scope helper. Instead, the per-handler unit tests assert the actual behavior
(`InstitutionalAdmin_from_other_institution_sees_nothing` and
`InstitutionalAdmin_from_other_institution_cannot_update`). This keeps the test count
honest and discoverable.

### NavMenu

The InstitutionalAdmin `AuthorizeView` block in `NavMenu.razor` will be expanded in
**T056.e** (final cluster) to the full set of accessible routes. T056.a leaves the
existing 3 placeholders in place — adding nav items pointing at half-guarded pages would
be misleading.

## T056.a — Shipped this session

**Foundations:**
- New `AdministratorOrInstitutionalAdmin` policy in
  `src/Wombat.Infrastructure/Identity/AuthorizationPolicies.cs`.
- Helpers in `ClaimsPrincipalExtensions`: `IsAdministrator()`,
  `IsInstitutionalAdmin()`, `CanAccessInstitution(int)`.

**Institutions / Specialities / SubSpecialities handlers — fully guarded:**
- 14 handlers updated (8 queries + 6 commands across the three entity types).
- Every query record carries `ClaimsPrincipal Principal`; lists filter by institution
  when the caller is not Administrator, get-by-id returns null for out-of-scope ids.
- Every command throws `UnauthorizedAccessException` when the target entity is outside
  the caller's institution. `CreateInstitution` and `DeactivateInstitution` are
  restricted to Administrator outright.

**Razor pages updated:**
- 6 pages in the Institutions feature now inject `AuthenticationStateProvider` and pass
  `authState.User` to every query/command (`InstitutionEdit`, `InstitutionsList`,
  `SpecialityEdit`, `SpecialitiesList`, `SubSpecialityEdit`, `SubSpecialitiesList`).
- 8 picker-using pages outside the Institutions feature were also updated to pass
  `authState.User` to `GetInstitutionsListQuery` / `GetSpecialitiesListQuery` /
  `GetSubSpecialitiesListQuery` since those records' signatures changed
  (`ActivityTypeEdit`, `Sso/GroupMappings`, `Curricula/CurriculaList`,
  `Curricula/CurriculumEdit`, `Invitations/InvitationsList`, `Epas/EpasList`,
  `Epas/EpaEdit`, `Forms/FormEdit`, `Assessors/AssessorProfileEdit`,
  `Trainees/PendingTraineesList`). Their `[Authorize]` attributes remain
  Administrator-only; that's reverted as their feature cluster lands.
- Page `[Authorize]` swap to `AdministratorOrInstitutionalAdmin`: only the 6
  Institutions/Speciality/SubSpec pages (plus the InstitutionsList stays Admin-only by
  design).

**Tests:**
- New `TestHelpers/TestPrincipals.cs` — synthetic Administrator + InstitutionalAdmin
  principals for handler-level scope tests.
- New `InstitutionalAdminScopeTests.cs` — 8 tests covering list-filter, get-by-id null,
  speciality-scope rejection, and update-rejection paths.
- Application test count: 174 → 183 (+8 new scope tests + 1 added in
  `CreateInstitutionCommandHandlerTests`).
- Domain 45 / Architecture 19 / Web 38 unchanged.

## Out of scope for T056.a (deferred to subsequent clusters)

- **EPAs, Curricula, ActivityTypes, Forms, Trainees, Assessors, Invitations,
  EntrustmentScales, Audit, SSO** — handlers not yet principal-aware; pages remain
  `[Authorize(Policy = "Administrator")]`.
- **NavMenu expansion** — deferred to T056.e per design.
- **Browser play-through of Phases 1.B onward as Mbatha** — meaningful once T056.b–d
  ship.
- **Documentation revert of Phase 1.B intent in scenario doc** — same.

## Out of scope (deferred follow-ups, all clusters)

- **The dashboard `/placeholder/users` route is left in place** as a follow-up for
  whoever builds the unified users page. T056 only swaps NavMenu links over to the real
  surfaces; the unbuilt page is a separate concern.

- **Audit entry scoping** (T056.e) will use the Actor's institution. Audit entries
  about entities scoped to a different institution but performed by the caller's user
  are visible; audit entries about the caller's institution but performed by a
  different institution's user are NOT visible. This matches the Actor-tracking model
  used today and keeps the query simple; a refined model (entity-scoped audit) is a
  separate task.

- **Cross-institution Administrator commands.** Administrators can still affect any
  institution. We do not add a "viewing as Administrator at institution X" mode.

- **Data Rights** stays at its current `Administrator,Coordinator` gate. Extending it
  to InstitutionalAdmin is a separate task — the request model is currently not
  institution-scoped at the row level.

- **Architecture-test enforcement** of scope-guard usage. Per user choice, the unit
  tests carry that responsibility.
