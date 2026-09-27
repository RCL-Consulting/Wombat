# ConfirmDialog

A native modal `<dialog>` that asks before a consequential action: a title that is the question, a body that says what the action does and what it cannot undo, then Cancel and the action.

## What the consumer provides

`<ConfirmDialog @ref="_confirm" Title="…" Body="…" ConfirmLabel="…" DangerAction="true" OnConfirm="…" OnCancel="…" />`, one per page, opened with `ShowAsync()` (it calls `wwwroot/js/dialog.js`, so it works on interactive pages only).

- `Title`: the question, naming its object. "Deactivate this EPA?", "Remove this item?", "Rebuild curriculum progress?"
- `Body`: what happens, what is kept, and what cannot be undone. "… The progress it has earned is kept. … You can reactivate it on this page."
- `ConfirmLabel`: the verb, never "OK" or "Yes": "Deactivate", "Remove item", "Rebuild progress".
- `DangerAction`: `true` renders the confirm button `.btn-danger`, else `.btn-primary`.

## Markup

`dialog.dialog-scrim.dialog-card` (max width 28rem, `radius-xl`, `shadow-dialog`, no padding, 100% less 2rem on a phone) holds a `.form-container` with an `h2`, a `p` and a `.form-actions` row (FormActions): Cancel (`.btn-outline`), then the action. The backdrop is `.dialog-scrim::backdrop`, `scrim`.

## Rules (DESIGN.md § Button system)

- A destructive action always goes through a ConfirmDialog, and only its footer carries `.btn-danger`.
- A destructive row action on a list uses one dialog for the page, whose body names the row it was opened from.
- While `OnConfirm` runs the confirm button keeps the focus with `aria-disabled="true"`, and Cancel is disabled.
- Once the dialog has closed, the action's result takes the focus in an `ActionResult` region (see Alert); while a modal is open nothing outside it can take the focus.

## Known gaps

The dialog has no accessible name (no `aria-labelledby` on its `h2`). The `btn-danger` label passes since T335 (5.95:1).
