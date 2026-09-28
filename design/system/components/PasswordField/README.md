# PasswordField

A password field: its label, its input, and a Show toggle standing 8px to the right of the input, never inside it. The toggle works with no circuit (driven by `wombat.js` on a static page) and in one (`@onclick`), and is hidden where neither can drive it, so there is never a dead button. Flow 02 (T339, 2026-09-28) built it; it replaced `PasswordToggleButton`.

## What the consumer provides

```razor
<PasswordField Label="Confirm new password" Id="confirm-password" Name="ConfirmPassword"
               Autocomplete="new-password" Required="true" Invalid="@refused"
               DescribedBy="@(refused ? "confirm-password-error change-password-error" : null)">
  @if (refused) { <div class="validation-message" id="confirm-password-error">The password confirmation does not match.</div> }
</PasswordField>
```

- `Label` and `Id` (required): the label's text and the input's id.
- `Name`, for a form the browser posts; `Autocomplete`: `current-password` (sign in, link, the Remove dialog, change password's current) or `new-password` (register, change password's new and confirm, the administrator's reset), so a password manager fills or offers the right one.
- `Required`, `Autofocus`, `Invalid` (`aria-invalid="true"`), `DescribedBy` (the ids the input names, space-separated).
- `Value`/`ValueChanged`, only on an interactive page that binds the field (the administrator's reset card). A field nothing binds takes no input handler, so a password typed in a circuit does not cross it keystroke by keystroke.
- The child content follows the input inside the group: the field's `.validation-message`, its help (`small.page-subtitle`), `PasswordRules`.

## Markup

```html
<div class="form-group">
  <label for="login-password">Password</label>
  <div class="password-field">
    <input id="login-password" name="Password" class="form-control" type="password" autocomplete="current-password" required>
    <button type="button" class="btn btn-outline password-toggle" aria-controls="login-password" aria-pressed="false"
            aria-label="Show password" data-password-toggle hidden>
      <svg class="icon password-toggle-eye">…eye…</svg>
      <svg class="icon password-toggle-eye-off">…eye-off…</svg>
      Show
    </button>
  </div>
  …child content…
</div>
```

## The toggle

- **A `.btn.btn-outline.password-toggle`**, as tall as the field: 38px, 44px below 641px; 0.75rem each side. `.password-field` is a flex row, 8px (`space-sm`) between the input and the toggle, so the input's focus ring sits in the gap and a typed password never runs under the button.
- **Its words are always "Show"**, with the `eye` icon. Its name is "Show" and the field's label lower-cased, exactly: "Show password", "Show confirm new password" (`aria-label`). It carries `aria-controls` (the input) and `aria-pressed`.
- **Pressed**, it keeps the word "Show" (E9): the `secondary-color` fill with `on-fill` words and the `eye-off` icon in place of the eye say it is on, and `aria-pressed="true"` says so to a reader. The stylesheet swaps the icons from `aria-pressed`, so whatever drives the toggle changes only `aria-pressed` and the input's `type`.
- **With no circuit** (every signed-out page, change password, and an interactive page's prerender) it is rendered `hidden` with `data-password-toggle`. `wombat.js` shows it as the page loads and after an enhanced navigation, and drives every toggle with one delegated click listener (no inline handler: the CSP forbids one). With script blocked the page has a plain password field and no button. `[hidden] { display: none !important }` in app.css makes that hold: `.btn`'s `inline-flex` would otherwise beat the browser's own `[hidden]`.
- **In a circuit** (My account's Remove dialog, the administrator's reset card) it toggles through `@onclick`.
- **A shown password is never posted as text or left on screen:** as its form is submitted, and when the page comes back from the back-forward cache, `wombat.js` turns every controlled field back to `type="password"` and `aria-pressed="false"`.

## Rules (DESIGN.md § Form system; § Account / auth page)

- Every password field is a `PasswordField`. The old `.password-wrapper` and `.password-toggle-btn` (a 28px muted word inside the field's end, which toggled over JS interop and so did nothing on a static page) are gone.
- On flow 02's account forms every field is required and none is marked `*` (DESIGN.md § Accessibility).
- A refused field names its own message and the refusal; a new-password field names the rules list until a refusal is shown (see PasswordRules).

## Contrast

At rest, `secondary-color` words and edge on the surface, 4.86:1. Pressed, `on-fill` on `secondary-color`, 4.86:1. The focus ring on the surface, 4.86:1.
