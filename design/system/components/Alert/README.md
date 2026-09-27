# Alert

A message block in one of four kinds (`success`, `info`, `warning`, `danger`) whose ARIA role follows its kind unless the caller names one; with `ActionResult` and `RefusalText`, it is how every page reports what an action did.

## What the consumer provides

`<Alert Kind="danger" Id="login-error" Role="…" Dismissible="true">…</Alert>`

- `Kind`: `success`, `info` (the default), `warning` or `danger`. It renders `div.alert.alert-{Kind}`.
- The content: the message in the product's words. A caught refusal goes through `RefusalText.Of(exception)`, never `exception.Message`, so a validator's refusal reads as its sentences, not as "Validation failed: -- Members: … Severity: Error".
- `Role`, only where the kind's default is wrong. Defaults: `danger` is `role="alert"` (read at once), `warning` and `success` are `role="status"`, `info` has none. A refusal shown as a warning takes `Role="alert"`; a warning or success that is standing page content, there on every visit, takes `Role=""`, which renders none.
- `Id`, when a field must name the alert with `aria-describedby`: an alert already on the page when it loads is not reliably announced, so the field that takes the focus names it (sign-in, and every field a refusal names).
- `Dismissible`: adds a `.btn .btn-outline .btn-xs` × beside the content in an `.actions-cell`.

## ActionResult

`<ActionResult @ref="_result">` wraps a page's result alerts in `div.action-result[tabindex=-1]`. The page calls `FocusAfterRender()` once the action has answered, done or refused, so the result is read and the focus never falls to the body; `FocusOnLoad` does the same for a result that arrives with a page load (change password, the export check). The region shows the `focus-ring` outline when focused.

## Rules (DESIGN.md § Alerts, validation, empty states; § Button system)

- Never hand-write `<div class="alert …">`: it skips the role default.
- An action that is done moves the focus to its result; a refused action leaves the focus where it was, with its refusal as a `danger` Alert.
- A refusal that arrives by URL travels as a code and the page chooses the sentence, so a crafted link cannot put words on Wombat's page.

## Contrast

`info`: `secondary-color` on `info-bg`, 4.55:1, passes. **Fail, kept as the source has it (T322):** `success` 2.55:1, `warning` 2.42:1, `danger` 3.57:1, each the semantic colour on its own tint. The fix T322 proposes is `text-color` on the tint (11.2:1 and up), with the semantic colour kept as the border.
