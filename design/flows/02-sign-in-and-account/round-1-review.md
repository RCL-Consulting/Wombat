# Flow 02 · Round 1 — the review

Read 2026-09-28 from the canvas's boards (`round-1/`, version `1790577632-b23c`: 30 boards and the index). Round 1 was
drawn from § 1 alone. § 8's steps then went as a check, which added `Steps.dc.html` and three corrections to `Main`.

## What round 1 proposes

One structure. The seven pages stay; the sign-out confirmation moves onto the anonymous card. Changes that matter:

- **One message slot** on the sign-in page, under the h1: a refusal (danger, `role="alert"`, takes the focus, tab
  "Error: …") or a notice (info, `role="status"`, the focus stays on Email, which names it). Session ended, password
  changed, and a NEW "You have signed out." are notices.
- **A static password toggle** beside the input, not inside it: hidden until `wombat.js` shows and wires it
  (`data-password-toggle`); named "Show <field label>", `aria-pressed`, `aria-controls`. `PasswordToggleButton` retires.
- **Change password becomes static in the shell** (`[ExcludeFromInteractiveRouting]`, as `/Error` is): one column, the
  five rules shown before typing, the refusal in the card above its actions, success redirected to My account
  (`?status=password-updated`).
- **My account: three cards.** Account (email as text, institution, roles by label one per line), Your name (the only
  form; "Save name", "Name saved."), How you sign in (the password with Change password; each institutional sign-in
  with Remove). `NameMissing` splits into first and last.
- **The rules as plain sentences** in `PasswordRuleMessages`, shared with F11's register page and F12's reset card.
- **Sign out:** the top bar stays one press. GET `/account/logout` and `/account/logout-confirm` show the confirmation
  page (T317), which names who is signed in.
- **Journey step counts** are unchanged, except forgot password (B: 5 not 6) and a typed GET `/account/logout` (1 press
  where today is a dead 405).

## Checked against the code (b347e11c)

- **Holds:** the returnUrl already survives a refused sign-in (`Program.cs`: every refusal is
  `SignInOutcome.Url(code, request.ReturnUrl)`); the round's "correction" makes explicit what is already so. The top
  bar's Sign out is a form post. The 38-word `EmailNotVerifiedMessage` is real (`ExternalLoginHandler.cs:77`).
- **Missed:** the error page's Sign out is a **link to `/account/logout-confirm`** (`MainLayout.razor:66–75`), because a
  form there would carry the failed pass's token. So the confirmation page is not reached only by typing, and
  `/account/logout-confirm` must stay a page that draws its own form. Round 2 must not make it a redirect to anything
  but a page with a form.
- **Not built, so Q4's "yes" is backend work:** `UserProfileDto` has no institution; nothing lists or removes an
  account's external logins (only `ErasureExecutor` calls `GetLoginsAsync`/`RemoveLoginAsync`). That is T286's half:
  a query, a Remove command with its audit row and the last-way-in guard, and the ConfirmDialog.
- **Q2 option B is not built** (no reset token endpoint; `PasswordResetEmail` is never sent).
- **T287's recommendation** (one refusal for a locked account) relies on the lockout being explained by email, which
  flow 19 would design and nothing sends today.

## For the operator

Q1 (where the institution buttons sit), Q2 (forgot password), Q3 (confirm every sign-out), Q4 with T286's scope, and
T287's wording. Recorded below once decided.

## Decisions (the operator, 2026-09-28)

- **Q1: the institution buttons sit above the form** (option A), with the divider "or sign in with your email and
  password". No provider configured: the plain form.
- **Q2: plain words, an administrator resets** (option A). The emailed-link reset (option B) is filed as T340.
- **Q3: the top bar's Sign out stays one press** (option A, adopted as recommended; the operator may overrule). GET
  `/account/logout` and `/account/logout-confirm` both draw the confirmation page with its own form: the error page's
  Sign out links to the latter.
- **Q4: yes, and T286's half is built in this flow:** the institution on My account; "How you sign in" lists the
  password and each institutional sign-in, with Remove (audited, the last way in guarded, a ConfirmDialog).
- **T287: a locked account is told "Invalid email or password.",** as a wrong password and an unknown address are.
  The lockout email is flow 19's; until then the locked person is told nothing on the page.
