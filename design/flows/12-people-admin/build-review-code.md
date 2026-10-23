# T376 build review: code, data, scope, authorization

Diff `a95e1daa..c4592070` (`src`), branch `t376`, read against `build-lanes.md`, `build-contracts.md`, `round-2-review.md`
(M1–M10, F1–F11), `round-1-assessment.md` § 10 and the R3 Spec boards. Read-only; no suite was run. Each claim was read in the code.

**Result: 0 must-fix, 3 should-fix, 6 nits.** The brief's list holds except as B1–B3 say (checked list at the end).

## Should-fix

### B1. "Requests waiting" is not limited to the person's institution

- **Where:** `Wombat.Application/Features/Users/Queries/GetRequestsWaitingForUser/GetRequestsWaitingForUserQuery.cs:75–76`
  (`AwaitingNominees.LoadForAsync(_dbContext, _dbContext.Set<Activity>(), person.UserId, …)`), `Activities/Services/AwaitingNominees.cs:63–83`.
- **What is wrong:** the query is scope-checked on the person, but the activities are every activity in the database whose data holds the
  person's id. Nothing compares `Activity.InstitutionId` (nullable, stamped) with the caller's scope.
- **Failure scenario:** an Assessor is moved from institution A to B (a supported admin action). Requests from trainees at A that name her
  still wait on her. B's Institutional admin opens Lock out… on her page and the dialog lists A's trainee names, activity types and EPA
  codes (`GetDisplayNamesAsync` is not institution-bound). Count and names cross the tenant line.
- **Fix:** for a non-Administrator caller add `.Where(a => a.InstitutionId == person.InstitutionId)` to the activities passed in (the
  nudge keeps its own unrestricted call). Test: a moved assessor, caller at the new institution, expects `None`.

### B2. Add a role loses a concurrent scope change, leaving a role whose need the scope no longer meets

- **Where:** `AddRoleToUserCommand.cs:76–79` (user and scope read), `:101` (`ResultingScope` over that read), `:103–110` (the write).
- **What is wrong:** the held scope is read before the transaction opens and `UpdateScopeAsync` replaces the person's whole scope with
  held ∪ added. `AddRoleAsync` re-finds the user, so nothing detects that the scope moved in between.
- **Failure scenario:** admins A and B both open Add a role for the same person. A adds Assessor with Paediatrics / Neonatology; B adds
  Coordinator with Surgery. Both read an empty scope; A commits {Paed, Neo}; B then writes {Surgery}. She holds Assessor with no
  sub-speciality, the very state M2 and F1 exist to prevent, and no request can name her.
- **Fix:** inside the `InOneTransactionAsync` lambda re-read the user (`GetByIdAsync`) and recompute `ResultingScope` from that;
  better, take a row lock or compare the account's concurrency stamp. Test: scope changed between the read and the write.

### B3. A mapping's speciality and sub-speciality are still accepted unchecked

- **Where:** `Sso/CreateSsoGroupMapping.cs:129–137` (written as given; no check after the role and provider rules above it).
- **What is wrong:** unlike Add a role (`AddRoleToUserCommand.cs:136–156`: adoption, sub-speciality under a held speciality) a mapping may
  name any speciality id, a sub-speciality of another speciality, a speciality the institution has not adopted, or none for an Assessor or
  Sub-speciality admin (roles that need a scope). SSO provisioning then grants that scope at sign-in.
- **Failure scenario:** a crafted request (the page offers only valid pairs) from an Institutional admin maps a group to Assessor with
  Surgery / Cardiology at an institution that adopted neither; members sign in holding a scope no gate reads, or an Assessor with none.
- **Fix:** reuse `AdoptedDisciplines.LoadAsync` and the `RoleTakesSubSpeciality` / `RoleNeedsSpeciality` rules, refusing with a typed
  `SsoMappingRefusedException` under the Role or Speciality field. Test per rule.

## Nits

- **B4.** `UpdateInstitutionCommandHandler.cs:59–63` maps every `DbUpdateException` to `Duplicate` on the short code; a concurrency or
  connection fault would read "already exists". Refuse as duplicate only for the unique-index violation (`PostgresException` 23505) and rethrow the rest.
- **B5.** `GetInstitutionalAdminDashboardSummaryQuery.cs:53` pages with `PageSize: int.MaxValue` to count roles: every account of the
  institution plus its scope rows is materialised for a tile; a grouped count in SQL is cheaper. Unlike every other Users read it also asks no
  `DemandUserAdministration` (a Trainee-holding admin gets the counts); the page may gate it, the handler should.
- **B6.** `ListAssessors.cs:332–353`: `ListUsersInRoleAsync` reads every holder and the page is cut in memory. The page's reads are constant
  (no N+1), but it is not paged in SQL as `ListUsersPageAsync` is. Fine at this size.
- **B7.** `GetAdoptedDisciplinesForInstitutionQuery.cs:40–47` has no trainee-first (T278). It returns non-secret names for an institution the
  caller can access, so the harm is nil, but the rule is "every new handler".
- **B8.** `ListUsersPageAsync` (`UserAdministrationService.cs:264`) orders by the database's `lower()` collation, the in-memory default by
  ordinal: accented names may order differently between the real service and a test directory.
- **B9.** `AwaitingNominees.LoadForAsync` (`:75–78`) scans `DataJson` as text over every row in the pinned waiting states. It runs when the
  dialog opens, not per view, so acceptable.

## Checked and holding

- **AddRoleToUser (M2).** Order: `DemandUserAdministration` (trainee-first, `:71`), not the caller (`:72`), assignable role (`:73`; Trainee and
  system roles refused with the admission words), then the user (`DemandInScope`, `:76`), global-administrator guard, already-held,
  no-institution, adopted-only ids, resulting scope (F1 judged on held plus added, F2 narrowing). Every refusal precedes the write, so the
  failure audit row stages nothing. Role and scope go through `IdentityTransaction.RunAsync` (`:103`), which rolls back and clears the
  change tracker on a fault in the second write. An out-of-scope or missing id is the same `UnauthorizedAccessException` sentence for an
  Institutional admin (T290), never forbidden-versus-missing.
- **RemoveUserInstitutionalSignIn (F7).** Order at `RemoveUserInstitutionalSignInCommand.cs:44–60`: trainee-first, not own account, in scope,
  Administrator guard, then `RemoveInstitutionalSignInAsync`, whose last-way-in guard (`UserAdministrationService.cs:352`) runs before any
  mutation and detaches the staged deletions on a conflict; typed refusal.
- **GetRequestsWaitingForUser (F6).** Trainee-first and `DemandInScope` before any read; same sentence for out-of-scope and missing (except
  as B1). `AwaitingNominees` is the nudge's former rule moved verbatim (first `field:` arm, string value only, pinned workflow, same cutoff
  filter); the job now orders by `UpdatedOn` but the mail set is unchanged.
- **ResetUserPassword F3.** The refusal sits after the scope checks and before the write; no mutation.
- **Training-only save (M1).** `CreateOrUpdateAssessorProfile.cs:84–129`: trainee-first, not own, in scope, institution stamped from the person;
  no `UpdateScopeAsync`, no security-stamp change. The reads (`GetAssessorTraining`, owner query) fail closed to null.
- **F4 migration `20261023132146_T376_AssessorProfileTrainingOnly`.** Drops two FKs, two indexes, two columns; `Down` restores them; Designer and
  snapshot match (no navigation or index left). Nothing else reads `AssessorProfile.SpecialityId` / `SubSpecialityId` (searched `src`, `tests`).
- **ListUsersPageAsync.** Two counts, one page query with scope includes, one roles query for the page's ids: no N+1. The lock threshold equals
  `UserDeactivation`'s; Administrator and College-admin filters are refused to an Institutional admin (`ListUsersQuery.cs:164–167`), who is
  pinned to her institution (`:103–112`); an unknown role or status is refused.
- **SSO.** Delete: missing and out-of-scope read alike as not found; provider-gone is the Administrator's. Create keeps T288 (trainee-first),
  T333 (admission roles before the mappable list), the Administrator refusal and Q19.
- **UpdateInstitution.** Scope check first, typed refusals; the duplicate checks match the unique indexes (both case-sensitive).
- **Deleted code.** `RevokePendingInvitationsForEmail`, `GetAssessorProfileById`, `ListAssessorUsers`, `ListAssessorsForSpeciality`: no
  remaining reference in `src` or `tests` except a test asserting the first is gone. Flow 11's B3 on the first is moot.
- **Audit.** `AuditPipelineBehavior` is unchanged; the new refusals throw before any mutation, and their messages name no one.
