# AuthLayout

The shell for the signed-out account pages: **the auth card**, a 480px `.account-form-container` top-aligned on a pale gradient, holding the page's one `<h1>`, one message slot and a plain HTML form that posts to an endpoint. Flow 02 (T339, 2026-09-28, f50dffb2) redesigned it. It is not flow 01's shell: My account and change password are in the shell (MainLayout), not here.

## What the consumer provides

A page with `@layout Layout.AuthLayout` whose body is the card: sign in (SignInCard), sign out, link your institutional sign-in, forgotten password, register, and the MSF respondent's `/msf/respond` (the `--wide` card).

```html
<div class="auth-page-shell">
  <main class="auth-page-main">
    <div class="account-form-container">
      …the lockup and purpose line (sign in only)…
      <h1>Sign out</h1>
      …one message slot…
      <p>…</p>
      <form method="post" action="…"> <AntiforgeryToken /> …fields… <FormActions>…</FormActions> </form>
    </div>
  </main>
</div>
```

## The card (DESIGN.md § Account / auth page, E8)

- **`div.account-form-container`**: `surface-color`, a `border-color` edge, `radius-xl`, `shadow-raised`; 30rem (480px) wide at most (`width: min(100%, 30rem)`), padded 40px (`2.5rem`), 24px below 641px.
- **The rhythm:** the card and its form are flex columns with a 16px (`space-md`) gap, between the card's blocks and between the fields of its form; no block carries a margin of its own (the card zeroes its children's and its form's children's margins). An Alert in the slot sits in that rhythm too.
- **Top-aligned, not centred:** `.auth-page-main` puts the card 40px under the top (32px below 641px, with 16px sides), so a card that grows (a refusal, the institutions' buttons) no longer moves its heading.
- **The heading is the page's `<h1>`**, 2rem/600 at line height 1.25 (1.5rem below 641px). A page on the card has no PageHeader; until T339 the card's heading was an `<h2>`, so those pages had no h1.
- **Below 641px** (E7): every `.btn` and `.form-control` on the card is 44px; an action row stacks, each button the row's width and the primary on top (`column-reverse`); the Remember me row is 44px, its label filling it so the tick is pressed anywhere on it (6735fa98), the box 20px; and `.account-link` is a 44px target.
- **The words' classes:** `p.account-purpose`, the muted line under the lockup; `strong.account-email`, an email in the card's words at 600, breaking anywhere rather than widen the card; `ol.account-steps`, numbered steps 8px apart; `p.account-note`, a condition ruled off by a hairline, muted, 16px under it; `a.account-link`, a link on its own line.
- **One message slot**, under the h1 (or, on link, above Password inside the form): a refusal as a danger Alert that takes the focus itself (`tabindex="-1"` + `autofocus`) and is named by the fields; a notice as an info Alert, `role="status"`. SignInCard has the focus rule.
- **The `--wide` card** (`/msf/respond`, T205) keeps its own flow: block layout, 44rem, an `<h2>` a state at 2rem, and its margins. Flow 10 designs it.

The page is `div.auth-page-shell` (a radial `secondary-color` glow at 12% over a 135deg `background-color` to `header-bg` gradient, both from tokens) holding `main.auth-page-main`. No text sits on that ground outside the card: muted words on its blue corner would be 4.13:1.

## The pages

- **Sign in** (`/account/login`): SignInCard.
- **Sign out** (`/account/logout`, and `/account/logout-confirm`, where the error page's Sign out links). `<h1>Sign out</h1>`, "You are signed in as Nomsa Mahlangu (mahlangu@kgk.wombat.local). Signing out ends your session in this browser.", then Cancel (back to a local return address, else Home) and the primary Sign out with `log-out`. Neither GET signs anyone out: the form posts to `/account/logout/submit` with its own token. The shell's Sign out is one press and never comes here. Signed out, it sends the visitor to the sign-in page, which says "You have signed out."
- **Link your institutional sign-in** (`/account/link-external`). "A Wombat account with the email **…** already exists. Enter your current password to link your institutional sign-in.", Password, Cancel and "Link and sign in". A refusal stands in the slot above Password, takes the focus, and Password names it; a wrong password and a locked account read the same. When the sign-in in progress has gone: **Institutional sign-in expired** (h1 and tab), "Your institutional sign-in has expired. Start again from the sign-in page." and Back to sign in (`arrow-left`), no field and no alert.
- **Forgotten password** (`/account/forgot-password`). No reset email and no field: "A Wombat administrator can set a new password for you.", three `.account-steps`, and Back to sign in; where an institution signs people in, an `.account-note`: "If your institution signs you in, use its button on the sign-in page. Your institution resets that password, not Wombat."
- **Register** (`/account/register`): its h1 is "Complete registration", its passwords are `PasswordField`s with the six rules (PasswordRules) under the first. The rest of its body is flow 11's.

## Rules (DESIGN.md § Account / auth page; § Anonymous static page)

- A signed-out visitor gets static server-rendered HTML with no circuit: links, form posts to minimal-API endpoints, and `wombat.js` (the Show toggles). `@onclick`, `@bind` and JS interop do nothing there.
- The sign-in cookie is written only by an HTTP POST, so sign-in, register, link, sign-out, change password, My account's name and Remove stay real `<form method="post">` elements.
- A refusal comes back as a code in `?error=` and the page chooses the sentence; a crafted link cannot put words on the page. After a refusal the tab starts "Error: ". No sentence says "Please".
- Every field on these forms is required, and none is marked `*`.
- The other signed-out pages (Access denied, Page not found, the error page, `/portfolio/verify`) use MainLayout's signed-out bar, not this layout.

## Known gaps

- Register's page body other than its passwords (the name and email fields in `.mb-3` wrappers, its invitation states) is still pre-restructure; flow 11 designs it.
- `/msf/respond`'s `--wide` card is pre-restructure; flow 10 designs it.
- The account outcomes travel in the address (`?error=`, `?status=`), so a reload or a shared link shows the message again: T341.
- Wombat sends no password-reset email: T340.
