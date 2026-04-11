# T005 — Invitation flow

**Phase:** 1 — Domain & data
**Depends on:** T002, T003
**Blocks:** T006, T011

## Goal

Implement the invitation-only registration flow end-to-end: an admin issues an invitation, an email is queued, the recipient clicks the link, registers, and lands as the right role with the right scope claims.

## What to do

1. In `Wombat.Domain/Invitations/`:
   - `Invitation` — Id, Email, TokenHash, TargetRole, InstitutionId, SpecialityId?, SubSpecialityId?, IssuedByUserId, IssuedOn (UTC), ExpiresOn (`DateOnly`), UsedOn (UTC?), RevokedOn (UTC?). Never store the raw token; hash it on write, compare hashes on read.
2. `InvitationTokenService` in `Wombat.Application/Common/Security/` — generates cryptographically random tokens (`RandomNumberGenerator`), hashes them with SHA-256, verifies candidates against the hash.
3. CQRS in `Wombat.Application/Features/Invitations/`:
   - `IssueInvitationCommand` — inputs: email, target role, scope. Creates the row, returns the raw token (one time only, to be emailed).
   - `RevokeInvitationCommand`
   - `AcceptInvitationCommand` — inputs: token, password, first name, last name. Creates the `WombatIdentityUser`, assigns target role, writes scope claims, marks the invitation used.
   - `GetInvitationPreview` (query) — given a raw token, return the email and target role so the registration form can pre-populate.
   - `ListActiveInvitations` query for admin pages.
4. Email template: `InvitationEmail.cshtml` (or Razor template) — includes the registration URL with the token in the query string. This is a "safe" use of a URL token because the token is one-time and bound to the email recipient. Make the token expire in 14 days by default.
5. Blazor pages:
   - `Admin/Invitations/InvitationsList.razor` — list + issue + revoke
   - `Account/Register.razor?token=...` — reads token, validates via `GetInvitationPreview`, shows the registration form, calls `AcceptInvitationCommand` on submit, signs the user in, redirects based on role.
6. Hook the invitation email send to the email queue from T012 — but since T012 hasn't shipped yet, for this task implement a stub `IEmailSender` that writes to a log. The real sender will plug in later.
7. EF migration: `Invitations`. Include `Designer.cs`.

## Files created

- `src/Wombat.Domain/Invitations/Invitation.cs`
- `src/Wombat.Infrastructure/Persistence/Configurations/InvitationConfiguration.cs`
- `src/Wombat.Infrastructure/Persistence/Migrations/*Invitations.cs`
- `src/Wombat.Application/Common/Security/InvitationTokenService.cs`
- `src/Wombat.Application/Features/Invitations/**`
- `src/Wombat.Infrastructure/Email/LoggingEmailSender.cs` (stub)
- `src/Wombat.Web/Components/Pages/Admin/Invitations/**`
- `src/Wombat.Web/Components/Pages/Account/Register.razor`

## Verification

- [x] `dotnet build` clean.
- [x] Unit tests: token generation produces unique values; hash-and-verify round-trips; expired invitations are rejected; revoked invitations are rejected; used invitations are rejected.
- [x] Manual: issue an invitation as the seeded admin, copy the token from the log, navigate to `/Account/Register?token=...`, register with a new email, confirm the user exists in `AspNetUsers` with the right role, log out and back in.
- [x] The invitation row is marked used and cannot be re-used.

## Session notes

- 2026-04-11: Implemented the invitation aggregate, EF configuration, `Invitations` migration, token hashing/verification service, CQRS handlers and DTOs, a logging `IEmailSender` stub, admin invitation UI, registration page, and basic login/logout pages.
- 2026-04-11: Added startup migration application so the seeded data path no longer fails on a fresh database, and moved login/register/logout cookie issuance to real POST endpoints after interactive Blazor cookie writes failed with `Headers are read-only, response has already started.`
- 2026-04-11: Added invitation-focused application tests covering token uniqueness, hash verification, and rejection of expired/revoked/used invitations.
- 2026-04-11: Manual walkthrough passed: admin issued an invitation, invited user registered successfully, invited user could log back in, and reusing the same invitation link was rejected.

## Notes & gotchas

- **Never** put the raw token in the database. Hash it. The raw token only exists in the email the user receives and the in-memory return of `IssueInvitationCommand`.
- Timing attack note: when comparing tokens, use a constant-time compare. Helper lives in `CryptographicOperations.FixedTimeEquals`.
- If two invitations exist for the same email, accepting one should not invalidate the other. Invitations are independent.
- Trainees accepted via invitation land as `PendingTrainee`, not `Trainee`. They become `Trainee` when a SpecialityAdmin adds them to a curriculum (part of T006 / T011).
- URLs with tokens leak via referrer headers. In the registration page, immediately remove the token from the URL on client load (e.g. `replaceState`) so it doesn't survive navigation.
