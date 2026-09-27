---
id: T317
title: Account page leftovers from the T295 replay: a refused registration drops the names typed, GET /account/logout answers 405, the remembered dashboard outlives sign-out, and the admin reset form enables at 8 characters
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T317 — Account page leftovers from the T295 replay: a refused registration drops the names typed, GET /account/logout answers 405, the remembered dashboard outlives sign-out, and the admin reset form enables at 8 characters

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Low. Each is friction or wording. None reveals or changes data: the dashboard cookie can only show the next person a view of a role they hold themselves (Home.razor:84-86 checks that the role is held).
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-2.9a, F-2.8b, F-A.4.7a, F-2.34a, F-A.4.5a, F-A.4.5b).

## Symptom

- **Registration** (Step 2.9; design/baseline/act-2/2.9-1-patel-too-short.png, 2.9-2-patel-mismatch.png): after PasswordTooShort, and again after ConfirmationMismatch, the form returns with First name and Last name empty, so each retry means retyping them (F-2.9a).
- **Sign-out** (Steps 2.8 and A.4.7): typing /account/logout while signed in shows Chrome's "This page isn't working … HTTP ERROR 405" (F-2.8b). /account/logout-confirm works when its address is typed (design/baseline/states/logout-confirm--default.png), but no page links to it (F-A.4.7a).
- **Dashboard view** (Step 2.34; design/baseline/act-2/2.34-4-naidoo-inherits-view.png): Zulu switched to her Assessor view and signed out. Naidoo then signed in on the same browser and landed on "Viewing as Assessor", which was Zulu's choice (F-2.34a).
- **Admin reset** (Step A.4.5; design/baseline/states/user-detail--reset-refused.png): Reset password enables at 8 characters, so a 10-character password is refused only after the press. The refusal reads "Passwords must be at least 12 characters.; Passwords must have at least one non alphanumeric character.; …" (F-A.4.5a, F-A.4.5b). Change password gives one sentence per rule (Step A.4.2).

## Root cause

- **Registration:** Program.cs:290-339 answers every refusal with LocalRedirect(RegisterOutcome.Url(token, codes)), which carries only the token and the codes (RegisterOutcome.cs:89-104). Register.razor:50-57 renders the name inputs with no value. T285 moved refusals to codes to stop text injection, but nothing there decided that the names must be lost.
- **Sign-out:** Program.cs:602 maps only POST /account/logout, so a GET matches no endpoint and gets 405. Logout.razor (/account/logout-confirm) is referenced by no component, because NavMenu.razor:55 and MainLayout.razor:13 post straight to /account/logout. Change password met the same 405 and now maps a GET that redirects (Program.cs:588, T265 review).
- **Dashboard:** /dashboard/switch/{role} (Program.cs:650-665) writes wombat_preferred_dashboard_role for 30 days and ties it to no account. The sign-out endpoint (Program.cs:602-626) leaves it in place, and Home.razor:81-89 honours it for whoever signs in next.
- **Reset:** UserDetail.razor:121 (minlength="8") and :127 (enabled at Length >= 8), and ResetUserPasswordCommandValidator (ResetUserPasswordCommand.cs:29, MinimumLength(8)), disagree with Identity's RequiredLength = 12 plus complexity (DependencyInjection.cs:56-61). UserAdministrationService.ResetPasswordAsync throws string.Join("; ", Identity's descriptions) (UserAdministrationService.cs:334), and the page prints that message.

## What to build

- **Registration.** A refusal brings the form back with First name and Last name as typed; the passwords are never carried back. The names must not go in the address, because they are personal data and the address is kept in history and in the proxy's access log. **Recommendation:** on a refusal, the endpoint writes the names to a short-lived cookie (minutes; HttpOnly, SameSite=Strict, Secure, path /account/register), protected with Data Protection and bound to the token. The page fills the fields from it when the token matches, and a successful registration deletes it. The address keeps carrying codes only, as T285 made it.
- **Sign-out.** Map GET /account/logout to a redirect to /account/logout-confirm, as Program.cs:588 does for change password. A GET never signs anyone out. The confirm page then has a caller, so keep it.
- **Dashboard view.** Tie the remembered view to the account that chose it. **Recommendation:** the cookie's value names the account as well as the role, protected with Data Protection. Home ignores a cookie written for another account, and the sign-out endpoint deletes it. Zulu still lands on her Assessor view when she signs in again (Step 2.34's Expect), and Naidoo lands on his own default.
- **Reset form.** Take the input's minlength and the button's rule from Identity's RequiredLength (inject IOptions<IdentityOptions>, as Register.razor does). The validator keeps NotEmpty and MaximumLength and drops its own length rule, so Identity is the only judge. The refusal travels as Identity's codes, in a typed exception like InvitationRefusedException's ErrorCodes, and the page gives one sentence per rule through PasswordRuleMessages, joined by spaces, as change password does. T286's note 'Identity descriptions in exceptions' names this same throw, and this change closes that note for the reset.

## Verification

- [ ] Integration (AccountRefusalFlowTests): a registration refused for PasswordTooShort, and one refused for ConfirmationMismatch, each come back with both names filled and both password fields empty; the redirect address carries no name; a successful registration deletes the cookie.
- [ ] Integration: GET /account/logout while signed in redirects to /account/logout-confirm and leaves the session signed in; the POST still signs out and writes Logout.
- [x] Web tests (DashboardPriorityTests, or an integration test of /dashboard/switch and Home): a view remembered by one account is ignored for another account on the same cookie jar; sign-out deletes the cookie; the same account signing in again gets its own choice. — `b347e11c`: `Hosting/ActingRoleFlowTests`. The view is stored with the account (a column carried as a claim), not in a cookie, so another account on the same browser lands by precedence, and sign-out leaves nothing of the choice behind. The one-time result cookie is deleted at sign-out.
- [ ] bUnit (UserDetail, alongside UsersPagesSelfAndTraineeTests): Reset password stays disabled below Identity's RequiredLength, and a refused reset shows one sentence per broken rule, with no '; '.
- [ ] Browser checks against the runbook, with the Expects updated: Step 2.9 (the names are kept), Step 2.8 (a typed /account/logout lands on the confirm page), Step 2.34 (Naidoo lands on his own view), Step A.4.5 (enabled at 12 characters, one sentence per rule), and Step A.4.7's Note.

## As built in T335 (`b347e11c`, 2026-09-27)

- Only the remembered-view part is closed. It is F-2.34a: the acting role is stored with the account (T335, D1 as
  built). The browser check of Step 2.34 is part of T335's step G replay.
- Still open: the registration names being dropped, GET `/account/logout` answering 405, the admin reset form enabling
  at 8 characters, and the three Notes items.

## Related

T285, T265, T286 (its note 'Identity descriptions in exceptions' covers the same ResetPasswordAsync throw), T290 (the Users commands' refusals), T287 (its cookie-prefix item: the dashboard cookie is a third app cookie); DESIGN.md:1826; runbook Steps 2.8, 2.9, 2.34, A.4.5, A.4.7; F-2.9a, F-2.8b, F-A.4.7a, F-2.34a, F-A.4.5a, F-A.4.5b.

## Notes

- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26.** Three more account-page items, all wording or consistency; the severity is unchanged. (1) Signed out, /access-denied says 'Your current role does not allow access to this area. Return to the dashboard or sign in with a different account.' to a visitor who has no role, and offers only Back to home (AccessDenied.razor:6-13; design/baseline/states/access-denied--signed-out.png). To a visitor who is not signed in, it should say that the page needs signing in and link to /account/login. (2) 'Your password was changed. Please sign in with your new password.' shows as a red danger alert (states/login--password-changed.png). Login.razor:22-25 renders every SignInOutcome code as Kind="danger", but PasswordChanged (SignInOutcome.cs:50, :112; ChangePasswordOutcome.cs:86) reports what happened; it is not a refusal. Give each SignInOutcome code a kind (info for PasswordChanged), and keep the fields' aria-describedby for refusals only. (3) Only Login.razor:12-15 and MsfRespond.razor:37-40 carry the brand row. Register, ForgotPassword, LinkExternalLogin and Logout, all under AuthLayout, have none (states/register--token-missing.png, compared with msf-respond--not-recognised.png). Put the row in AuthLayout or a shared component so every auth page has it, and record that in DESIGN.md § Account / auth page. Verify with bUnit: signed out, Access denied links to sign-in; PasswordChanged renders a non-danger alert; each AuthLayout page renders .account-brand. The sweep's fourth item, the reconnect modal's resume-failed wording, is noted on T328 with the modal's CSS.
