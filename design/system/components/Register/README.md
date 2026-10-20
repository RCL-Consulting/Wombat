# Register

Complete registration (`/account/register`, `Pages/Account/Register.razor`; its words in `RegisterText.cs` and `Security/RegisterOutcome.cs`, the invitation's in `InvitationRefusals.Describe`): **the auth card an invitation's link opens**, the form for the invited address, or one card saying why there is none. Flow 11 (T373, 2026-10-20, 69a17c70; boards R3-G01 to G18; round 3 Q6, Q11, Q13, F5, F7, F10, M4, S1, S2, S7, S15, S21; T265, T285, T315; build review B1, B7, B8, B10, P8) designed it. Until then the token stayed in the address bar (T315, the script that cleared it never ran on a static page), the name and email sat in `.mb-3` wrappers, a refusal was one Alert above the form, every unusable invitation read as an error over an empty form, and a signed-in visitor was offered the form.

## Who reads it

Anyone holding an invitation's link (`AllowAnonymous`). **Static for every visitor**, signed in or not (`[ExcludeFromInteractiveRouting]`): the form is an HTTP post and nothing on the page needs a circuit. The card is AuthLayout's 480px card, headed by **the lockup and the purpose line**, as sign in's (Q6; SignInCard): the mark at 56px, "Wombat", "Work-based assessment for specialist training.", then the page's one `<h1>`, which is also its tab's stem.

## The link and the cookie (S1, S2, S7)

The invitation mail's link opens `/account/register/start?token=…` (`RegisterOutcome.StartPath`, which `InvitationLinks.RegistrationUrl` writes). That GET keeps the token in **the register cookie** (`RegisterCookie`: Data Protection, `HttpOnly`, `SameSite=Lax`, `Path=/account/register`, 30 minutes from when the link was opened) and answers 303 to `/account/register` with no query, so no address bar, history entry or page source holds the token, and no script is needed (G17). The form posts to `/account/register/submit`, which reads the token from the cookie; a refusal writes its codes and the two names into the cookie (never a password) and redirects to the page with no query; success deletes the cookie. The page shows the codes once.

## The form (G01–G06)

- `h1` "Complete registration".
- **An info Alert, standing content, no role** (P8): "This invitation is for `span.account-email` kruger@cmsa.wombat.local, as College admin at College of Paediatricians of South Africa." (`InvitationScopeWords.RoleAndScope`: "as Trainee in Paediatrics / Paediatrics at Kgosi Kgari Teaching Hospital", "as Coordinator at Kgosi Kgari Teaching Hospital"; Q13), the address with `<wbr>` after its @.
- `form[method=post][data-submit-once]` with its antiforgery token and a hidden `Invitation` field, the invitation's fingerprint, never the token (B1):
  - **Email** (FormField): `readonly`, not `disabled`, with no `name`, `autocomplete="username"`; its help, "The address this invitation is for. It cannot be changed here."
  - **First name** (`given-name`) and **Last name** (`family-name`), FormFields, `required`, with **no `maxlength`** (S21): an over-long name is refused in words.
  - **Password** (`PasswordField`, `new-password`) with the six rules under it before anything is typed (PasswordRules), and **Confirm password**.
  - `FormActions`: **Register** (primary).
- `p.account-note`: "Not expecting this invitation? Ignore it: its link works until 2026-11-03." (F7).

No field is marked `*` (AuthLayout's rule for the auth card: every field is required).

## Refusals under their fields (S15; G04–G06)

AuthLayout's one message slot becomes **a message per field**: `div.validation-message` under the field (`register-…-message`), the field `aria-invalid` and described by it, and the focus on it (`autofocus`, on a static page): the first refused of First name, Last name, Password.
- A name: "Enter your first name.", "Enter your last name.", "Last name: enter 100 characters or fewer." (G06).
- **A broken password rule is Password's message** (G04): "The new password needs: At least 12 characters.", the field described by its message, then the rules' list; "Enter a password." for none.
- **A refused confirmation** (G05) empties both password fields, keeps the focus on Password, and says under Confirm password "The password confirmation does not match. Type the password again in both fields."; Password is described by that message, then the rules.
The names are kept; the passwords never are.

## The cards (G07–G16, G18)

When there is no form, the card says why in plain paragraphs, no Alert, each its own h1 and tab title; the first sentence is the refusal's own:

| h1 | Its words | Its actions |
|---|---|---|
| Invitation already used | "This invitation has already been used: an account was registered from it." "If it was yours, sign in with that account." | Sign in (`log-in`), Forgotten your password? |
| Invitation revoked | "This invitation has been revoked." "Ask the person who invited you for a new invitation." | none |
| Invitation link not recognised | "This invitation is invalid." "The link may be incomplete, or a newer invitation email may have replaced it. Open the link in the most recent invitation email, or ask the person who invited you for a new invitation." | none |
| Invitation expired | "This invitation has expired: its link worked until 2026-10-19." "Ask the person who invited you for a new invitation." (F7) | none |
| Account already exists | "A user with this email address already exists." "Sign in with that account." | Sign in, Forgotten your password? |
| Address not accepted | "This email address cannot be used for an account. Ask your administrator for an invitation to another address." | none |
| Invitation link incomplete | "The invitation token is missing." "Open the link from the invitation email again. If the email split it over two lines, copy all of it into the address bar." (a GET with neither cookie nor token) | none |
| Registration not completed | "Registration could not be completed. Nothing was saved." "The invitation is still open in this browser for 30 minutes from when you opened its link." (F10's one transaction makes it true) | Try again (outline, `refresh-cw`, back to the page, which reopens the form from the cookie) |
| Registered, but not signed in | "The invited user could not be loaded after registration." "Your account was created. Sign in with this email address and the password you chose." | Sign in |
| Invitation timed out | "This page held the invitation for 30 minutes, and that time has passed. Nothing was saved." "Try again: open the link in the invitation email once more." (a submit after the cookie's 30 minutes) | none |
| Another invitation opened | "Another invitation's link was opened in this browser after this form was shown, so nothing was registered." "The invitation open in this browser now is for ‹address›. Continue to register it, or open the link in your own invitation email again." (B1: two tabs, two links) | Continue (`arrow-right`) |
| Signed in as someone else | "You are signed in as Nolwazi Mbatha. This invitation is for ndlovu@kgk.wombat.local." "To register it, sign out first. Your own account is not changed." (Q11, M4) | Sign out and continue (`log-out`), Go to Home (`a.account-link`) |

**Signed in as someone else** (G18; F5): a signed-in visitor gets no form. Sign out and continue posts flow 02's `/account/logout/submit` with its own antiforgery token and a hidden `returnTo=/account/register`; the sign-out endpoint lands on Register only when `returnTo` is exactly that path (`SignOutOutcome.LandingFor`), never anywhere else, so it is no open redirect. The submit refuses a signed-in caller before anything is accepted, back to this card, replacing no session (M4).

## Narrow widths

AuthLayout's phone rules: below 641px every control and button on the card is 44px, an action row stacks, each button the row's width, and `.account-link` is 44px. Nothing scrolls sideways at 390 (states.md's Narrow).

## Contrast

No new colour token. As AuthLayout and SignInCard: the purpose line and the note `muted-text` 5.09:1, the notice's words 11.84:1 on `info-bg`, a field's refusal `danger-color` 5.95:1.
