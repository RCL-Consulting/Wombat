# SignInCard

The sign-in page (`/account/login`, `Login.razor`) on the auth card: the lockup, the purpose line, the page's one `<h1>`, one message slot, the institutions' buttons above the form, the form, and "Forgotten your password?". Static HTML with a real form post: a visitor who has not signed in has no circuit. Flow 02 (T339, 2026-09-28, f50dffb2) redesigned it; AuthLayout holds the card's frame and the other auth pages.

## The order, top to bottom

1. **The lockup** (`div.account-brand`): the mark as `<img alt="" width="56" height="56">` and "Wombat" in `wordmark-large` (Fraunces 500, 2.4rem, `primary-color`). The sign-in page and `/msf/respond` alone carry it.
2. **The purpose line** (`p.account-purpose`, muted): "Work-based assessment for specialist training."
3. **`<h1>Sign in</h1>`**, 2rem (1.5rem below 641px). Until T339 it was an `<h2>`, so the page had no h1.
4. **One message slot**, empty, or one Alert (below).
5. **The institutions' buttons** (`div.sso-providers`), only where an institution's sign-in is configured: one full-width 44px `a.btn.btn-outline.sso-button` each, `building-2` and "Sign in with Kgosi Kgari Teaching Hospital" at its left, a long name wrapping inside the button. Then `div.sso-divider`: "or sign in with your email and password" (muted, between two hairlines of at least 24px), saying what the form is for.
6. **The form**, posted to `/account/login/submit` with its antiforgery token: Email (`autocomplete="username"`), Password (a `PasswordField`, `current-password`), Remember me (a `.form-check`), and the `Sign in` submit in a `FormActions` row.
7. **`a.account-link` "Forgotten your password?"**, on its own line under the form (a 24px target, 44px below 641px).

The card's blocks are 16px apart (see AuthLayout). No field is marked `*`: every field is required.

## The slot: a refusal or a notice

What sent the browser here travels as a code in `?error=`, and the page chooses the words (`SignInOutcome.Describe`), so a crafted link cannot put words on Wombat's page.

- **A refusal** is `Alert Kind="danger"` (`role="alert"`), `Id="login-error"`: "Invalid email or password." (with institutions configured: "Invalid email or password. If your institution signs you in, use its sign-in button below."), "Enter your email and your password.", "Too many failed sign-in attempts from this network. Wait a few minutes and try again.", the institutional sign-in's refusals, and "Sign-in could not be completed. Try again." for a code the page does not know. **A wrong password, an address no account has, an account that signs in only through its institution and a locked account all read the same** (T156, T287), so the page says nothing about which addresses have accounts or are locked. The tab reads "Error: Sign in · Wombat".
- **A notice** is `Alert Kind="info" Role="status"`, `Id="login-notice"`: nothing on this page was refused. Four codes: "Your session has ended. Sign in again." (`SessionEnded`), "Your password was changed. Sign in with your new password." (`PasswordChanged`), "You have signed out." (`SignedOut`), and "Your current password was entered incorrectly too many times, so your account is locked for 15 minutes and you have been signed out. Wait 15 minutes, then sign in again." (`LockedSignedOut`; the span is Identity's configured lockout, never a number written in the page). Until T339 the session's end was a danger alert.

## The focus rule (C6)

An alert already on the page as it loads is not reliably announced, so the page decides who reads it:

- a refusal takes the focus itself (`tabindex="-1"` + `autofocus`), and both fields name it (`aria-describedby`);
- a notice with no institution's button leaves the focus on Email, which names it;
- a notice beside the institutions' buttons takes the focus itself, so it is read and the next Tab reaches the first institution;
- with nothing to say, Email takes the focus when there is no button, and nothing does when there are.

**A signed-in visitor is sent Home** (E4): the notices are for someone who has just stopped being signed in.

## Rules (DESIGN.md § Account / auth page)

- The institutions stand **above** the form (Q1): someone whose institution signs them in should not read past a form that is not theirs.
- The button's name is the institution's configured `DisplayName`, never a key.
- No sentence says "Please" (C5).
- Forgotten password is a link, never a form: Wombat sends no reset email (T340 would add one).

## Contrast

The purpose line and the divider's words, `muted-text` on the surface, 5.09:1; an institution's button, `secondary-color` on the surface, 4.86:1; the alerts' words 11.81:1 (danger) and 11.84:1 (info) on their tints. No text sits on the auth ground outside the card.
