# T093 — Build CollegeAdmin scoping end-to-end + invitation provisioning

**Status:** DONE (code + migration + tests; uncommitted) — built + live-verified 2026-06-14.
**Surfaced:** 2026-06-14, during the T092 fix, while wiring CollegeAdmin invitations (the "related finding"
in T092). Investigation revealed the gap was much larger than "no invitation option".

## Problem

T091 P1 added the `CollegeAdmin` **role**, the `CollegeId` **claim type**, the `AdministratorOrCollegeAdmin`
**policy**, and the `GetCollegeId()` / `CanAccessCollege()` **helpers** — but never wired the actual
**user → college association** or **claim emission**. Concretely, before this task:

- `WombatIdentityUser` had no `CollegeId`; there was no user-college scope table.
- `WombatUserClaimsPrincipalFactory` emitted InstitutionId / Speciality / SubSpeciality claims but **never a
  `CollegeId` claim**.
- The invitation surface had no `CollegeAdmin` role option and `Invitation` had no college column.

Net: **no user could actually be college-scoped** — `GetCollegeId()` returned null for everyone, so the
CollegeAdmin-only catalogue handlers (and the whole `AdministratorOrCollegeAdmin` surface) were dead for any
non-Administrator. The national catalogue could only be authored by the bootstrap Administrator.

## Changes (the whole chain)

- **Domain** — `Invitation.CollegeId` (`int?`) added; `Invitation.InstitutionId` made nullable (a
  CollegeAdmin invitation has no institution).
- **Infrastructure** — `WombatIdentityUser.CollegeId` (`int?`); `WombatUserClaimsPrincipalFactory` now emits
  the `CollegeId` claim when present; `InvitedUserProvisioner` sets `user.CollegeId` (signature now takes
  `int? institutionId, int? collegeId`). Migration **`T093_CollegeAdminProvisioning`** (CLI-generated +
  Designer + snapshot): `AspNetUsers.CollegeId`, `Invitations.CollegeId`, `Invitations.InstitutionId` →
  nullable. No FK constraints (consistent with the existing `SpecialityId` scalar columns).
- **Application** — `IInvitedUserProvisioner` signature updated; `AcceptInvitation` passes the college.
  `InvitationRules`: `CollegeAdmin` added to allowed roles; `ValidateScope` requires a college and forbids
  institution/speciality/sub-speciality for CollegeAdmin (and forbids a college for every other role);
  `ValidateScopeEntitiesAsync` verifies the college exists. `IssueInvitation`: command carries `int?
  InstitutionId, int? CollegeId`; **CollegeAdmin issuance is Administrator-only** (other roles still gated by
  the caller's institution claim); persists the college. `ListActiveInvitations` left-joins institution +
  college (CollegeAdmin invitations have no institution). `GetUserByIdQuery` pending-invitations DTO
  (`UserPendingInvitationDto`) `InstitutionId` → nullable, `InstitutionName` → `ScopeName` (institution **or**
  college name); the two revoke handlers (`RevokeInvitation`, `RevokePendingInvitationsForEmail`) are now
  institution-or-college scope-aware.
- **Web** — `InvitationsList.razor`: `CollegeAdmin` role offered to Administrators only; a **College picker**
  replaces the Institution picker when role = CollegeAdmin; active-invitations table gains a College column.
  `UserDetail.razor` pending-invitations column renamed Institution → "Institution / College".

## Tests

+5 in `InvitationValidatorTests` (CollegeAdmin with college accepted; without college / with institution /
issued-by-InstitutionalAdmin rejected; institution-role-with-college rejected). All suites green: Domain 50,
**Application 313**, Architecture 19, Web 43. Full solution builds clean in **Release** (0 warnings).

## Live verification (Playwright + DB)

Administrator issued a CollegeAdmin invitation for **Dr Kruger** scoped to **College of Paediatricians**;
Kruger registered (`kruger@cmsa.wombat.local` / pw in `pwd_DO_NOT_COMMIT.txt`) → landed on a CollegeAdmin
dashboard (nav: Specialities / EPAs / Curricula) → `/admin/epas` showed **exactly his college's 15 PAED
EPAs** (Demo College filtered out), proving the `CollegeId` claim flows through to the catalogue handlers.
DB-verified: `AspNetUsers.CollegeId = 2`, invitation `CollegeId = 2 / InstitutionId null / used`. Snapshot
**`t091-act2-with-collegeadmin`**.

## Notes / follow-ups

- SSO provisioning (`ExternalLoginHandler` / `SsoGroupMapper`) is a separate path and was **not** given a
  college mapping — CollegeAdmin via SSO is out of scope (and the Administrator role is already SSO-excluded).
- This unblocks `scenario-paediatrics.md` Step 1.3 (provision the CollegeAdmin, author the national catalogue
  as Kruger) without the bootstrap-Administrator workaround.
