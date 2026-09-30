# ConfirmDialog

A native modal `<dialog>` that asks before a consequential action: a title that is the question, a body that says what the action does and what it cannot undo, then the safe button (Cancel, or what it keeps) and the action. Two modes: the **action mode**, whose confirm runs in the circuit, and the **form mode** (flow 02, T339, C7), a real form the browser posts to an endpoint.

## What the consumer provides

`<ConfirmDialog @ref="_confirm" Title="…" Body="…" ConfirmLabel="…" DangerAction="true" OnConfirm="…" OnCancel="…" />`, one per page, opened with `ShowAsync()` (it calls `wwwroot/js/dialog.js`, so it opens on interactive pages only).

- `Title`: the question, naming its object. "Deactivate this EPA?", "Remove this item?", "Remove your Kgosi Kgari Teaching Hospital sign-in?", "Cancel this request?"
- `Body`: what happens, what is kept, and what cannot be undone. "… The progress it has earned is kept. … You can reactivate it on this page."
- `ConfirmLabel`: the verb, never "OK" or "Yes": "Deactivate", "Remove item", "Remove sign-in".
- `CancelLabel` (flow 03, T342, A5): the safe button's words, "Cancel" by default. Where the action is itself a cancel, "Cancel" beside "Cancel request" would say two things, so the safe button says what it keeps: "Keep the request", "Keep the draft".
- `DangerAction`: `true` renders the confirm button `.btn-danger`, else `.btn-primary`.
- **The form mode** adds `FormAction` (where the form posts; set, the dialog is a form and `OnConfirm` is not called), `HiddenFields` (by name: what the endpoint acts on, such as a provider's key), `FormContent` (the fields and messages between the body and the buttons: a PasswordField, a refusal) and `OnClosed` (the dialog has closed, by Cancel, Escape or the caller).

## Markup

Both modes: `dialog.dialog-scrim.dialog-card` (max width 28rem, 448px; `radius-xl`, `shadow-dialog`, no padding; 100% less 2rem on a phone), named by its heading and described by its body (`aria-labelledby`, `aria-describedby`). The backdrop is `.dialog-scrim::backdrop`, `scrim`.

- **The action mode** holds a `.form-container` with an `h2`, a `p` and a `.form-actions` row (FormActions): the safe button (`.btn-outline`, `CancelLabel`), then the action.
- **The form mode** adds `.dialog-card--form` and holds `form.dialog-form[method=post][data-submit-once]`: the antiforgery token, the hidden fields, the `h2` (1.25rem), the `p`, the caller's content, then `div.dialog-actions`. `.dialog-form` is a flex column padded 32px (`space-xl`) with 16px between its blocks; an Alert in it has no margin of its own. `.dialog-actions` has the top rule every action row has (a 1px `border-color` hairline, 16px above the buttons), Cancel (`type="button"`) then the `type="submit"` action at the right, 12px apart. Below 641px the form pads 24px and the buttons stack full width, the action first.

## Focus

- The dialog is opened modal, so while it is open nothing outside it takes the focus, Escape closes it, and the browser returns the focus to the control that opened it.
- **The form mode** gives the focus on open to the first `autofocus` inside: the caller's refusal, else its password field, else Cancel, which carries `autofocus` itself. On close `OnClosed` lets the caller put the focus back, for a dialog the page opened itself as it loaded (a refusal reopening it), which has no opener.
- **The action mode:** while `OnConfirm` runs the confirm button keeps the focus with `aria-disabled="true"` and Cancel is disabled; once the dialog has closed, the action's result takes the focus in an `ActionResult` region (see Alert).
- `data-submit-once` (`wombat.js`): a second press of the submit while the post is on its way sends nothing. Cancel is not a submit, so it never marks the form sent.

## Rules (DESIGN.md § Button system; § Account / auth page, ConfirmDialog's form mode)

- A destructive action always goes through a ConfirmDialog, and only its footer carries `.btn-danger`.
- A destructive row action on a list uses one dialog for the page, whose body names the row it was opened from.
- **Use the form mode when the action must change the sign-in cookie** (a security stamp, a claim): only an HTTP request can issue the cookie again. My account's Remove of an institutional sign-in is the first.
- **Cancelling an activity** (flow 03) is the action mode with `DangerAction`: "Cancel this draft?" · "A cancelled draft cannot be reopened, and it credits nothing." · Keep the draft · Cancel draft; "Cancel this request?" · "It leaves David Naidoo's Activity inbox. A cancelled request cannot be reopened, and it credits nothing." · Keep the request · Cancel request. The focus starts on the keep button and goes back to the opener on Keep (the status card's Cancel request…, or the bar's Cancel this draft…); once cancelled, to the page's result, "Cancelled." (ActivityWorkflowActions).
- A caller that opens the dialog from a press opens it once the render naming its object is drawn (`OnAfterRenderAsync`), not in the press's own handler, or `showModal` runs on the dialog as it was.

## Known gaps

- **The form mode's Cancel needs the circuit.** R3-Spec drew it as a `formmethod="dialog"` submit, which the browser closes the dialog with on its own; the build's Cancel is a `type="button"` with `@onclick`, kept for `OnClosed`, so while the circuit is down Cancel does nothing. Escape still closes the dialog (the browser's).
- The action mode opens only in a circuit (`dialog.js` over JS interop).
