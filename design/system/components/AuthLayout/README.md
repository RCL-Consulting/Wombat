# AuthLayout

The shell for the sign-in pages: a centred `.account-form-container` card on a pale gradient, carrying the brand lockup, a title, the page's refusal and a plain HTML form that posts to an endpoint; it is not flow 01's shell, and flow 02 redesigns it.

## What the consumer provides

A page with `@layout Layout.AuthLayout` (sign in, sign out, register, forgotten password, link account, and the MSF respondent's `/msf/respond`) whose body is the card:

- `div.account-form-container.shadow`: `surface-color`, a `border-color` edge, `radius-xl`, `shadow-raised`, padding 2.5rem, at most 30rem wide; `--wide` 44rem for the MSF questionnaire, with `space-lg` padding at 640px and below.
- `div.account-brand`, on the sign-in page and `/msf/respond` only: `img.account-brand-mark` (the mark, drawn at 56px) and `span.account-brand-text` "Wombat" in `wordmark-large` (Fraunces 500, 2.4rem, `primary-color`). Register, forgotten password, link account and sign out have no lockup: the card opens on its title (Register's `<h2>Complete registration</h2>`).
- An `h2` title in `account-title` (2rem/600): "Sign in". The tab is "Sign in · Wombat".
- A refusal as `<Alert Kind="danger" Id="login-error">`, which every field names with `aria-describedby`.
- A `<form method="post" action="/account/login/submit">`: labelled inputs with their `autocomplete` tokens (`username`, `current-password`, `new-password`), a password in a `.password-wrapper` with `PasswordToggleButton` ("Show"/"Hide", a 28px target inside the 38px field's right end), a `.form-check` for Remember me, and a `FormActions` row with the `.btn-primary` submit.
- Under it, a `.page-subtitle` line with the way out: "Forgotten your password? Reset it."
- Where an institution's SSO is configured: a `.sso-divider` ("or", `muted-text` at 0.875rem) and `.sso-providers` of `.btn-outline.sso-button` links, "Sign in with …". No provider is configured today.

The page is `div.auth-page-shell` (a radial `secondary-color` glow at 12% over a 135deg `background-color` to `header-bg` gradient, both from tokens) holding `main.auth-page-main`.

## Rules (DESIGN.md § Account / auth page; § Anonymous static page)

- A signed-out visitor gets static server-rendered HTML with no circuit: links, form posts to minimal-API endpoints, and `wombat.js`. `@onclick`, `@bind` and JS interop do nothing there.
- The sign-in cookie is written only by an HTTP POST, so sign-in, register, link, sign-out, change password and My account's name stay real `<form method="post">` elements.
- A refusal comes back as a code in `?error=` and the page chooses the sentence ("Your session has ended. Please sign in again."); a crafted link cannot put words on the page. After a refused post the tab starts "Error: ".
- `/msf/respond` is this shape widened, and is never interactive, signed in or not; its states each have their own `<PageTitle>`.
- The other signed-out pages (Access denied, Page not found, the error page, `/portfolio/verify`) use MainLayout's signed-out bar, not this layout.

## Known gaps

- `PasswordToggleButton` works only in a circuit, so on the static sign-in, register and link pages it does nothing.
- The `<img>` carries `width="44" height="44"`; the stylesheet draws it at 56px.
