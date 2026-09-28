# MyAccount

My account (`/account/profile`, `Profile.razor`), in the shell: the header, then three cards, Account, Your name and How you sign in, and Remove's dialog. Interactive, but it saves nothing in its circuit: the name form and Remove each post a real form to an endpoint, which issues the sign-in cookie again and sends the browser back with a code. Flow 02 (T339, 2026-09-28, f50dffb2) redesigned it; until then it was one form with the roles as a comma list and no way to see or remove an institutional sign-in.

## The page

- **`PageHeader`**: "My account", "Your name, your roles, and how you sign in." No trail: My account is the top bar's name, which is its current page.
- **After change password**, `?status=password-updated`: a success Alert "Password updated." under the header, in an `ActionResult`.
- **`div.my-account-grid`**: a third and two thirds (`minmax(0, 1fr) minmax(0, 2fr)`), 24px apart, one column to 900px. Account on the left; Your name over How you sign in in `div.my-account-column` on the right.
- **Each card** is `section.detail-card.my-account-card`, named by its `<h2>` (`aria-labelledby`), set at the h3 size, 1.1rem/600, so no heading level is skipped under the page's h1; 16px between its blocks. An Alert inside it has no margin of its own.

## Account

A `dl.details-list.details-list--stacked`: each term above its value, 12px between rows, the value wrapping anywhere so a 40-character address stays in the card.

- **Email**, as text.
- **Institution**, by name, only for an account that has one: never "none" or a dash (the platform Administrator and a College admin have no row).
- **Roles**, one per line (`ul.my-account-roles`), by label in the product's order: "Committee member", "Assessor", "Pending trainee".

## Your name

The page's one form, `form.my-account-form` posted to `/account/profile/submit` (`data-submit-once`): First name and Last name in a `.form-grid`, `required`, `maxlength="100"`, unmarked (no `*`); the result; then "Save name" in a `FormActions` row.

- **Saved:** a success Alert in the card above the action row, "Name saved." (also for an unchanged name).
- **Refused:** a danger Alert in the same place. A blank first or last name marks that field alone (`aria-invalid`, emptied, "Enter your first name.") and the card says "Your name was not saved. Enter your first name."; both blank marks both. "A first name or a last name can be at most 100 characters." and "Your name could not be saved. Try again." are named by both fields. The tab starts "Error: ".

## How you sign in

A `ul.signin-methods`, one `li.signin-method` a way in, hairlines between them: the icon (`key-round` or `building-2`, 20px, muted; hidden below 641px), the name over what it is (`.signin-method-text`: `<strong>` over `.signin-method-detail`, muted 0.9rem), and the action at the right.

- **Password**, "Your Wombat password.", with Change password (an outline link), only when a password signs the account in.
- **Each institutional sign-in** by the institution's configured name, "Institutional sign-in." (no date: Wombat stores none), with Remove, named "Remove your Kgosi Kgari Teaching Hospital sign-in", `aria-haspopup="dialog"`.
- **An account with no password** says, above the list, "You sign in through your institution. This account has no Wombat password, so there is no password to change here."
- **The last way in** (no password and one sign-in): Remove is disabled and described by its reason under it, `.signin-method-reason`, muted 0.875rem, right-aligned: "Remove: this is the only way you sign in." The server refuses it too.
- **Remove's results** sit in the card above the list: "Kgosi Kgari Teaching Hospital sign-in removed." (success), or "Your Kgosi Kgari Teaching Hospital sign-in was not removed. It is the only way you sign in to Wombat." / "… was not removed. Try again." (danger).

## Remove's dialog

A ConfirmDialog in its form mode, posted to `/account/external-logins/remove`:

- Title: "Remove your Kgosi Kgari Teaching Hospital sign-in?"
- Body: what remains and how to link it again: "You will still sign in with your email and password. You can link it again the next time you sign in through Kgosi Kgari Teaching Hospital." With another institution linked it names it too ("…, or through Marula Academic Hospital."), never "only" the password.
- When the account has a password: a PasswordField "Password" (its toggle "Show password", through `@onclick` here), help "Enter your password to confirm it is you."
- Cancel and the danger "Remove sign-in".
- **A wrong password, the network's throttle, or a lock the account already had** reopen the dialog as the page is drawn, its refusal first and focused, the field empty: "Your sign-in was not removed. Incorrect password." (which alone marks the field: `aria-invalid`, "Incorrect password."), "Your sign-in was not removed. Too many attempts from this network. Wait a few minutes and try again.", "Your sign-in was not removed. Your account is locked. Wait 15 minutes, then try again." The fifth wrong password locks the account and signs this browser out, to the sign-in page's notice.

## Focus

No result is `autofocus`ed. Once the circuit has drawn the loaded page, the focus moves once to the one result there is: the dialog opened again, else How you sign in's result, Your name's, or Password updated. A load that fails is StatePanel's danger Alert, "Could not load your account. Nothing has changed. Try again, or come back in a few minutes.", with Try again; a retry that loads moves the focus to the Account heading, unless there is a result to say.

## Below 641px (E7)

Every `.btn` and `.form-control` is 44px; the cards pad 16px; the name's Save name and each sign-in's action take the row; each sign-in stacks, its name over its action, its reason left-aligned.

## Rules (DESIGN.md § Account / auth page, § My account)

- Nothing that changes the sign-in cookie is saved in a circuit: the name and Remove post forms.
- The page names the institution from its own configuration, never from the address.
- Change password is its own page, static in the shell (Home › My account › Change password), not a card here.

## Known gaps

- The outcomes travel in the address (`?status=`, `?error=`, `&provider=`), so a reload shows the message again: T341.
- Remove's Cancel is a `type="button"` with the circuit's `@onclick`, so while the circuit is down Cancel does nothing (Escape still closes the dialog). See ConfirmDialog.
