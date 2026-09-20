# T092 — InstitutionalAdmin cannot issue Assessor/Trainee invitations (empty speciality dropdown post-T091)

**Status:** DONE (code + tests; uncommitted) — fixed + live-verified 2026-06-14.
**Surfaced:** 2026-06-14, during the T091 fresh-DB scenario replay (`scenario-paediatrics.md` Act 2),
as Prof Mbatha (InstitutionalAdmin, KGK) on `/admin/invitations`.

## Fix shipped

`GetSpecialitiesListQueryHandler` / `GetSubSpecialitiesListQueryHandler` now have an **InstitutionalAdmin
branch** (gated on `IsInstitutionalAdmin()` so no other role's behaviour changes): when the caller has no
college claim but has an institution claim, the list is scoped to the national specialities /
sub-specialities the institution has an **active `InstitutionCurriculumAdoption`** for. So Mbatha's
invitation Speciality dropdown now shows exactly KGK's adopted discipline (Paediatrics / General
Paediatrics) and she can issue Assessor + Trainee invitations. Same change correctly fills the EPA /
curriculum / activity-type / panel / form scope pickers for an InstitutionalAdmin (previously all empty).
+4 tests in `InstitutionalAdminScopeTests`. **Live-verified:** Mbatha issued an Assessor invitation for
Patel scoped to Paediatrics / General Paediatrics.

The **related finding** below (CollegeAdmin provisioning via invitation) grew into its own task **T093**,
which is also DONE (CollegeAdmin scoping was entirely unwired in T091 P1 — built end-to-end).

## Problem

Under T091, `Speciality` was re-parented off `Institution` onto `College` — specialities are now
**nationally owned**. The invitations form was not updated to match, so an `InstitutionalAdmin`
cannot complete the invitations that require speciality scope.

On `/admin/invitations` as an InstitutionalAdmin:

1. The **Speciality dropdown is empty** (only "Select speciality"; no national speciality appears).
2. `Assessor` and `Trainee` invitations **require** speciality + sub-speciality scope
   (`InvitationRules.ValidateScope`, now actually enforced since T088 wired FluentValidation into
   the MediatR pipeline). Submitting fails with:
   *"The selected role requires speciality and sub-speciality scope."*
3. Net effect: **Mbatha cannot onboard her own clinical team** (assessors + registrars) — the
   entire Act-2 workflow the scenario prescribes for the InstitutionalAdmin. Coordinator /
   CommitteeMember invitations (speciality optional) still work.

The bootstrap **Administrator is unaffected** — its invitation form lists all national specialities
(Paediatrics, General Medicine) and issues Assessor/Trainee invitations fine. So this is strictly an
InstitutionalAdmin-scope regression.

## Root cause

`src/Wombat.Web/Components/Pages/Admin/Invitations/InvitationsList.razor:178` populates the dropdown via
`GetSpecialitiesListQuery(authState.User)`. Its handler:

`src/Wombat.Application/Features/Institutions/Queries/GetSpecialitiesList/GetSpecialitiesListQueryHandler.cs:22-30`

```csharp
if (!request.Principal.IsAdministrator())
{
    var scopedCollegeId = request.Principal.GetCollegeId();
    if (!scopedCollegeId.HasValue) return [];           // <-- InstitutionalAdmin lands here
    query = query.Where(e => e.CollegeId == scopedCollegeId.Value);
}
```

An `InstitutionalAdmin` is scoped to an **institution**, not a college, so `GetCollegeId()` is null and
the handler returns an empty list. (Pre-T091 specialities were institution-owned and this query keyed on
the institution claim, so the dropdown populated.)

## Fix options

The invitation form needs a speciality source that makes sense for an InstitutionalAdmin. Options, in
rough order of preference:

1. **Adoption-driven (most correct):** an InstitutionalAdmin should only invite into disciplines their
   institution has **adopted** a curriculum for. Surface the national specialities/sub-specialities behind
   the institution's active `InstitutionCurriculumAdoption` rows. This also dovetails with the admission
   gate (you can only admit trainees into adopted versions anyway).
2. **All national specialities:** branch `GetSpecialitiesListQuery` so an InstitutionalAdmin sees every
   active national speciality/sub-speciality (simplest; slightly over-broad — lets them invite into a
   discipline they have not adopted, which admission would then block downstream).
3. Keep the query college-scoped but resolve the InstitutionalAdmin's relevant college(s) from their
   institution's adoptions.

Whichever is chosen, also re-check `GetSubSpecialitiesListQuery` for the same InstitutionalAdmin hole
(the sub-speciality dropdown cascades off the speciality and likely has the same scoping).

## Related finding (smaller — can fold in or split)

The invitation **Role** dropdown exposes no `CollegeAdmin` option and there is **no College picker**, so
a CollegeAdmin cannot be provisioned via invitation (the T091 P1 role/claim/policy exist, but the
invitation surface was never wired for them). During the replay the national catalogue was authored by the
bootstrap Administrator instead (the scenario's documented Step-1.3 fallback). Add `CollegeAdmin` to the
role list + a College picker (enabled when role = CollegeAdmin), scoped to the Administrator.

## Acceptance

- As an InstitutionalAdmin with an active adoption, `/admin/invitations` lists the adopted
  speciality/sub-speciality, and Assessor + Trainee invitations issue successfully.
- An InstitutionalAdmin with no adoptions sees no invitable clinical disciplines (and the UI says so
  rather than silently failing validation).
- Administrator behaviour unchanged.
- (If folded in) Administrator can invite a `CollegeAdmin` scoped to a College.
- Re-run `scenario-paediatrics.md` Act 2 onboarding entirely as Mbatha (no Administrator workaround).

## Repro / validation state

Snapshots from the surfacing replay: `t091-act1-replay` (national catalogue + KGK adoption + Mini-CEX),
`t091-act2-molefe-admitted` (Molefe admitted via the Administrator-issued-invitation workaround, profile
pinned to AdoptionId 1). Restore either to reproduce.
