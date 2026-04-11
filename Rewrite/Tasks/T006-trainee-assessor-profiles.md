# T006 — Trainee, Assessor, and Admin profile data

**Phase:** 1 — Domain & data
**Depends on:** T005
**Blocks:** T007, T011

## Goal

Extend the identity user with role-specific profile data: a trainee's programme and start date, an assessor's qualifications and available forms, admin-side enrolment of trainees into curricula.

## What to do

1. In `Wombat.Domain/Identity/`:
   - `TraineeProfile` — Id, UserId, CurriculumId, ProgrammeStartDate (`DateOnly`), ExpectedCompletionDate (`DateOnly`), IsActive.
   - `AssessorProfile` — Id, UserId, Qualifications (text), InstitutionId, SpecialityId?, SubSpecialityId?.
   - `AdminProfile` is not needed — admins are just users with roles.
2. EF configurations. `TraineeProfile` has a unique index on `UserId` (one active profile per user; historical profiles need a new design, defer).
3. CQRS:
   - `AdmitTraineeCommand` — take a `PendingTrainee` user, add a `TraineeProfile` with the chosen curriculum, promote the Identity role to `Trainee`. This is the gate that turns a `PendingTrainee` into a real trainee.
   - `UpdateTraineeProfileCommand`
   - `DeactivateTraineeProfileCommand` (for graduations / withdrawals)
   - `CreateOrUpdateAssessorProfileCommand`
   - Queries: `GetTraineeProfileById`, `ListTraineesForSpeciality`, `GetAssessorProfileById`, `ListAssessorsForSpeciality`.
4. Blazor pages:
   - `Admin/Trainees/PendingTraineesList.razor` — list `PendingTrainee` users and offer "Admit to curriculum".
   - `Admin/Trainees/TraineeProfileEdit.razor`
   - `Admin/Assessors/AssessorsList.razor`
   - `Admin/Assessors/AssessorProfileEdit.razor`
   - `Account/Profile.razor` — self-service edit for the logged-in user.
5. EF migration: `Profiles`. Include `Designer.cs`.
6. Wire the role promotion on `AdmitTraineeCommand`: remove the `PendingTrainee` role, add the `Trainee` role, refresh their claims on next sign-in (or force a sign-out and sign-in if easier).

## Files created

- `src/Wombat.Domain/Identity/{TraineeProfile,AssessorProfile}.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/{TraineeProfile,AssessorProfile}Configuration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*Profiles.cs`
- `src/Wombat.Application/Features/Trainees/**`
- `src/Wombat.Application/Features/Assessors/**`
- `src/Wombat.Web/Components/Pages/Admin/{Trainees,Assessors}/**`
- `src/Wombat.Web/Components/Pages/Account/Profile.razor`

## Verification

- [ ] `dotnet build` clean.
- [ ] Unit tests: admitting a trainee changes their role and creates a profile; attempting to admit a non-pending user fails cleanly.
- [ ] Manual: register a new trainee via invitation, log in as admin, admit them to a curriculum, log back in as the trainee, confirm they now see the `Trainee` dashboard stub.
- [ ] Expected completion date is derived from `ProgrammeStartDate + Curriculum.MaxWindowMonths` (or similar) and can be overridden.

## Notes & gotchas

- A trainee belongs to exactly one curriculum at a time. If they transfer, deactivate the old profile and create a new one — do not mutate in place.
- Assessor profiles are looser: one per user is fine for now. If the app ever needs assessors spanning multiple institutions, revisit.
- Do **not** carry claims across a role change without re-issuing them. Force re-auth after `AdmitTraineeCommand` if simpler.
