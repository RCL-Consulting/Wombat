# T373 build review: code, data, scope, authorization

Diff `69a7df20..4a58fbe7` (`src`, `tests`), branch `t373`, read against `build-lanes.md`, `build-contracts.md`, `round-2-review.md`
(M1–M11, S1–S24, F1–F10) and the R3 spec's build rules. Read-only; no suite was run. Each claim below was read in the code.

**Result: 0 must-fix, 4 should-fix, 8 nits.** Everything the brief named holds, except as B1–B4 say (checked list at the end).

## Should-fix

### B1. The register form is no longer bound to the invitation it shows (two tabs register the other invitation)

- **Where:** `src/Wombat.Web/Program.cs:393–424` (the submit reads `state.Token` from the cookie), `Register.razor` (the form carries no
  invitation identifier; `RegisterPageTests` asserts `input[name=Token]` is absent), `RegisterCookie.Open` (`/start` overwrites the
  one cookie).
- **What is wrong:** until T373 the token rode in the form, so a form always registered the invitation it displayed. Now the invitation is
  whatever the cookie holds at submit time, and the cookie is one per browser.
- **Failure scenario:** a person (or an administrator walking a replay through several invitees in one browser) opens the link for
  `x@…` in tab 1 (form shows `x@…`, fills first name, last name, password), opens the link for `y@…` in tab 2 (cookie now holds Y's
  token), returns to tab 1 and presses Register. The antiforgery token is valid, no one is signed in, so the endpoint accepts Y's
  invitation with the names and password typed for X: Y's account exists under X's details, Y's invitation is Used, and tab 1
  signs in as Y. This is the "stale tab swaps identities" hazard M4 closed for a signed-in session, reopened for a second link.
- **Fix:** put a non-secret binding in the form, not the token: a hidden field `invitation` = first 16 hex chars of SHA-256(token) (or a
  random nonce kept in the cookie and echoed). In the submit, if it differs from the cookie's, accept nothing, leave the cookie, and
  redirect to the page (it will show the cookie's invitation, so the person sees the other address). Test: open link X, load form, open
  link Y, post X's form: no account, Y still unused.

### B2. Admission is not atomic: profile, scope and promotion commit separately, and the stale-tab refusal then lies

- **Where:** `src/Wombat.Application/Features/Trainees/AdmitTrainee.cs:166` (`SaveChangesAsync` of the profile), `:172`
  (`UpdateScopeAsync`), `:180` (`PromotePendingTraineeAsync`); `UserAdministrationService.cs:413–423` (`RemoveFromRoleAsync`
  commits, then `AddToRoleAsync` commits).
- **What is wrong:** the handler's comment promises "nothing that can fail runs after the mutation", but the last two calls can fail
  (Identity concurrency, a database fault). F10 made accept atomic; the admission, the other half of onboarding, is not.
- **Failure scenario:** the profile is saved, then `AddToRoleAsync(Trainee)` fails after `RemoveFromRoleAsync(PendingTrainee)` committed.
  The registrar now holds no role and has an active profile. A retry from the same page is refused `AlreadyAdmitted` (the active-profile
  check comes before the role check) and the page says "already has an active trainee profile … Open Trainees", which is true but
  leaves her unable to sign in as anything. The register of Waiting no longer lists her (no PendingTrainee). Nothing in the UI
  repairs it.
- **Fix:** run profile save, scope update and promotion inside the same `InOneTransactionAsync` (add it to `IUserAdministrationService`
  or reuse the provisioner's), or promote first inside a transaction that also saves the profile. Keep the mail after the commit.
  Test with a promotion that throws after the remove: no profile, role unchanged.

### B3. Two readers of an invitation's address and last day skipped S4 and F7

- **Where:** `src/Wombat.Application/Features/Users/Queries/GetUserById/GetUserByIdQuery.cs:58` (`today = DateOnly.FromDateTime(DateTime.UtcNow)`)
  and `:62` (`entity.Email == user.Email`); `…/RevokePendingInvitationsForEmail/RevokePendingInvitationsForEmailCommand.cs:43`
  (`entity.Email == normalisedEmail`).
- **What is wrong:** build-contracts § 0 says every comparison of an invitation's address with another goes through
  `InvitationAddress.ToAddress` / `NormaliseAddress`, and the last day is judged on the South African calendar.
- **Failure scenario (F7):** a link whose last day is D works until 22:00 UTC on D. From 22:00 to 24:00 UTC the user page's "pending
  invitations" still lists it (UTC date is still D), while Register, the list and the resend call it expired. **(S4):** an invitation
  stored as `Molefe@kgk…` and an account `molefe@kgk…` (an SSO-created account) are not matched: the user page shows no pending
  invitation and "Revoke pending invitations" revokes nothing, returning 0.
- **Fix:** `ProgrammeCalendar.DateOf(utcNow)` (with the handler's `TimeProvider`) and `.ToAddress(user.Email)` in both; a case-variant
  test and a 22:30 UTC test each.

### B4. One unusual address makes the whole "Expired, not used" read throw, which fails the invitations page

- **Where:** `src/Wombat.Application/Features/Invitations/ListExpiredUnusedInvitations.cs:128` (`newestByAddress[address]`), built from
  `:106–114`.
- **What is wrong:** `addresses` are normalised in C# (`Trim().ToLowerInvariant()`), the second query matches on SQL `lower("Email")`,
  and the dictionary is keyed by the C# normalisation of what SQL returned. Where Postgres `lower()` (database locale) and
  `ToLowerInvariant` disagree on a character (non-ASCII local parts, `İ`, some locales), the row's own invitation is absent from
  `toTheseAddresses`, so the indexer throws `KeyNotFoundException`. `EmailAddress()` validation permits non-ASCII.
- **Failure scenario:** an expired invitation to `İlker@…` exists; Invitations shows "The invitations could not be read" for every
  administrator of that institution until it ages out (30 days), and the active list is lost with it (`ReadAsync` reads both).
- **Fix:** `newestByAddress.GetValueOrDefault(address, row.Id) == row.Id`, or query the rows' own ids with the address match done in
  memory. The same `ToLower()` vs `NormaliseAddress` seam makes `ToAddress` miss silently (Q10 does not fire) for such addresses;
  say so in `InvitationAddress`'s remarks.

## Nits

- **B5. `FindByEmailAsync` and the used-invitation read are called once per row.** `ListExpiredUnusedInvitations.cs:117–123` (two queries
  per address through `GetRolesAsync`) and `ListPendingTrainees.cs` (one `UsedInvitationAsync` per registrar). Fine at "the waiting few";
  batch if either list can grow.
- **B6. An in-scope holder with no name reads "open  under Users".** `InviteAPerson.razor:252` matches `HolderName: { }` against `""`
  (an erased or nameless account trims to ""). Match `HolderName: { Length: > 0 }` and fall back to the address only.
- **B7. `/account/register/start` stores any token length.** `RegisterCookie.Open` has no bound; a token over ~3 KB makes a cookie the
  browser drops, and the person sees "Invitation link incomplete". Legitimate tokens are 43 characters; treat a token over, say, 256 as
  missing (clear the cookie).
- **B8. The sign-out endpoint now reads the form for anonymous callers.** `Program.cs:831–836`: `ReadFormAsync` on a malformed
  urlencoded body throws `InvalidDataException` (500) where it used to redirect. Wrap in try/catch and treat as no `returnTo`.
- **B9. `GetAdmissionResult` says "admitted" for any active profile of the caller's institution, however old.** `?admitted=<id>` from a
  bookmark or a typed id shows the admission sentence and takes the focus. Not a leak (in scope, as the list), but misleading; require the
  profile's `AdmittedOn` to be today or yesterday (SAST), else say nothing.
- **B10. The submit rewrites the timed-out cookie on every blind post**, extending its 30 minutes each time (`Program.cs:397–399`); harmless
  but the G16 card then outlives the window. Keep the original `Until` when the state is already `TimedOut`.
- **B11. Tests that stop one assertion short.** `AccountRefusalFlowTests.TheMailsLink_KeepsTheTokenInTheCookie…` does not assert
  `Cache-Control: no-store` on the 303 (S2's requirement); `SignOut_IgnoresAnyOtherReturnTo` (integration) lacks the case and
  trailing-slash variants the unit test has (`/ACCOUNT/REGISTER`, `/account/register/`), which is fine for the pure function but not
  the endpoint's form read (multiple `returnTo` values join with a comma, correctly refused, untested).
- **B12. `GetAdmissionDates` takes any curriculum id** (`GetAdmissionDates.cs`): it answers the derived completion of a curriculum the
  registrar is not offered. The catalogue is national, so nothing private; for symmetry with F1 it could return null dates for an id
  `AdmissionCurricula.Offered` does not list.

## What was checked and holds

- **M3.** `InvitationIssueRefusedException` derives from `InvalidOperationException`; Q10/Q12 messages are the constant
  `AddressInUse`; holder id/name and the other invitation's issue time and last day are set only when `InCallerScope`
  (`IssueInvitation.cs:188–232`). `AuditPipelineBehavior` writes `ex.Message` and `Serialize(request)` (the caller's own typed
  address); the test drives the real pipeline over another institution's account and invitation and asserts nothing of either is in
  the row. All checks precede the first staged write (T201). Scope refusals, resend and revoke refusals are typed with fixed words; the
  page shows only `IsRefusal` messages and logs the rest.
- **M4.** `Program.cs:385` refuses a signed-in caller before the antiforgery check, the cookie and the command; the integration test posts
  under a signed-in cookie and asserts no account, invitation unused, session kept, no `Set-Cookie`.
- **F5.** `SignOutOutcome.LandingFor` is an ordinal equality with `/account/register`; anything else (case, slash, query, `//host`,
  absolute URL, comma-joined duplicates) lands on `SignedOutUrl`. No open redirect.
- **F10.** Provisioner and accept share one scoped `ApplicationDbContext` (`DependencyInjection.cs:50`; Identity stores use the same
  instance), no retrying execution strategy is configured, so `BeginTransactionAsync` is legal. Profile-less path: refusals are thrown
  before the first write. Rollback then `ChangeTracker.Clear()` leaves the pipeline's failure row (written afterwards through the same
  context, outside the transaction) as the only commit; the integration test injects a fault after `ProvisionAsync` and asserts no
  user and `UsedOn` null. The in-memory provider runs the work unwrapped (unit tests only).
- **S1/S2.** Cookie `wombat_register`: Data Protection under a purpose string, HttpOnly, SameSite=Lax, `Path=/account/register`
  (covers start and submit), `Secure = Request.IsHttps`, `MaxAge` = the remainder of 30 minutes from when the link was opened (a refusal
  keeps `Until`), `IsEssential`; holds token, codes, the two names (cut at 256), timed-out flag; never a password. Every redirect in the
  register flow is the bare `/account/register`. `/start` answers 303 with `Cache-Control: no-store`, deletes the cookie on a missing
  token. Forged, other-purpose and lapsed values read as no cookie. Antiforgery: `.DisableAntiforgery()` plus `IsRequestValidAsync` is
  equivalent (same cookie token and form token, same validator); the manual check runs after the signed-in redirect so a stale tab is
  redirected, not 400ed, and before anything is accepted.
- **F1.** `AdmissionCurricula.Offered` is the one rule for page and command; a forged curriculum id is `CurriculumNotOffered` before
  anything is staged; the scope is rewritten only for a registrar with no invited sub-speciality.
- **F3.** `IssuedAsRole` is written from the principal (never the page); resend (`ExecuteUpdate`) leaves it; Home shows "Invited by" only
  for `InstitutionalAdmin`, an issuer read now and not deactivated, on `IssuedOn`'s SAST date. Migration, designer and snapshot agree
  (one nullable `varchar(64)`).
- **F4/S6/S8/M09.** Derived completion refused only when left empty and passed; deactivated refused in words; `NotPending` is one
  reason for unknown, not waiting and out-of-scope (message names nobody); the active-profile check is after the scope check; `?admitted=`
  is re-read with `CanAccessInstitution`; out of scope returns null.
- **F7.** `LinkExpiresOn`, `HasPassedLastDay`, issue, active list, expired list, preview, accept, resend and prefill all use the SAST date
  from an injectable clock; 21:59/22:00 UTC tests exist. Only B3's two readers were missed.
- **S4.** `ToAddress` is used by Q10, the accept's sweep, the used-invitation reads and Expired's leaving rule; `FindByEmailAsync` reads
  `NormalizedEmail`. Misses: B3, B4's seam.
- **Mail.** The admission mail goes through `AccountNotices` after the profile save and the promotion (so a refused admission mails
  nobody, a failed send is logged, a deactivated account is skipped); both mails HTML-encode every interpolated name. The invitation
  mail no longer carries the role key in words or tags.
- **M5.** Both pages clear the link at the start of issue, resend, revoke-open, "Invite another address", "Invite again", the header
  action and on `LocationChanged`; tests per action. **M1.** All three new/changed pages ask the T290 note before any read; the handlers
  also demand it. **Scope.** Every new query/command calls `DemandUserAdministration`/`DemandTraineeAdministration` first, filters by
  `InstitutionId == own` (Expired) or `IsInScope`/`CanAccessInstitution` (Prefill, Candidate, Result, Waiting); a College invitation is
  never in an Institutional admin's scope. **Layers.** No Domain entity in a `.razor`; handlers use `IApplicationDbContext`; the
  provisioner's transaction is in Infrastructure behind an Application interface. **Exception text** never reaches a page: unexpected
  faults are logged and shown as fixed sentences on all four new/changed pages and Register.
