# AuthLayout

The shell for signed-out and account pages: a centred `.account-form-container` card on a pale gradient, carrying the brand lockup, a title, the page's refusal and a plain HTML form that posts to an endpoint.

## What the consumer provides

A page with `@layout Layout.AuthLayout` whose body is the card:

- `div.account-form-container.shadow` (max `account-card-width` 30rem; `--wide` 44rem for the MSF questionnaire, with less padding at 640px and below).
- `div.account-brand`: `img.account-brand-mark` (the mark at 56px) and `span.account-brand-text` "Wombat" in `wordmark-large`, `primary-color`.
- An `h2` title in `account-title` (2rem): "Sign in".
- A refusal as `<Alert Kind="danger" Id="login-error">`, which every field names with `aria-describedby`.
- A `<form method="post" action="/account/login/submit">`: labelled inputs with their `autocomplete` tokens (`username`, `current-password`, `new-password`), a password in a `.password-wrapper` with `PasswordToggleButton` ("Show"/"Hide"), a `.form-check` for Remember me, and a `FormActions` row with the `.btn-primary` submit.
- Under it, a `.page-subtitle` line with the way out: "Forgotten your password? Reset it."
- Where an institution's SSO is configured: a `.sso-divider` ("or") and `.sso-providers` of `.btn-outline.sso-button` links, "Sign in with …". No provider is configured today.

## Rules (DESIGN.md § Account / auth page; § Anonymous static page)

- A signed-out visitor gets static server-rendered HTML with no circuit: links, form posts to minimal-API endpoints, and `wombat.js`. `@onclick`, `@bind` and JS interop do nothing there.
- The sign-in cookie is written only by an HTTP POST, so sign-in, register, link, sign-out and change password stay real `<form method="post">` elements.
- A refusal comes back as a code in `?error=` and the page chooses the sentence ("Your session has ended. Please sign in again."); a crafted link cannot put words on the page.
- `/msf/respond` is this shape widened, and is never interactive, signed in or not; its states each have their own `<PageTitle>`.

## Known gaps

- `PasswordToggleButton` works only in a circuit, so on the static sign-in, register and link pages it does nothing; and it centres on the label and input together, so it sits high.
- `.sso-divider span` names `var(--text-muted)`, which is undefined; the text falls back to `text-color`.
- The page's gradient (`rgb(52 152 219 / 0.12)` and two greys) is written outside :root.
- `.shadow` on the card overrides its own `account-card-shadow` with `shadow-utility`.
