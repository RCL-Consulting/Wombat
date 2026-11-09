# EntrustmentScale

A scale's page (`/admin/entrustment-scales/new`, `/admin/entrustment-scales/{Id}`, `Pages/Admin/EntrustmentScales/EntrustmentScaleEdit.razor`; its words `EntrustmentScaleWords.cs`, the field refusals `EntrustmentScaleBindings` and the save's refusals `EntrustmentScaleRefusalWords` in the Application layer; the read `GetEntrustmentScaleByIdQuery` with `EntrustmentScaleDto.InUse`, the commands `CreateEntrustmentScaleCommand` and `UpdateEntrustmentScaleCommand`, the typed refusal `EntrustmentScaleUpdateRefusedException`, "in use" read once by `EntrustmentScaleUses`): **one ladder, its details and its rungs, which the Administrator alone creates and changes.** Flow 16 (T394, 2026-11-05, de4cc718; boards R3-C02 to C05, C11 to C16, C20, R2-C17 to C19b; Q6, Q7, Q9, Q11, Q16, Q18, S3, S4, S5, S6, S11, S13, S14, T1, T7, T324, T393, T395, T397) rebuilt it. Until then it was "Edit entrustment scale" whatever the scale, with "Back to scales" in the header, two `detail-card`s headed by h3s, a filled red Remove on every rung of every scale (a scale in use included, so a reorder silently moved the minima of the items pinned to it, T395), its rungs named by their order alone ("Move level 3 up"), its read failure and its refusals in the exception's text (T393), and a reorder that failed on PostgreSQL (T397).

## Who reads it

The Administrator only (`[Authorize(Policy = "Administrator")]`; every scale command refuses anyone else). NavOwners lights Entrustment scales (`gauge`, the Administrator's Catalogue group) for it, and the trail is Home › Entrustment scales › the page's h1 ("CNSA Neurology Entrustment Scale", "Create entrustment scale"). The College admin and the Institutional admin read the list only (EntrustmentScales, D57); this page sends them to Access denied.

## The page

- **The header**: h1 and tab are the scale's name (T1, S11), "Create entrustment scale" on a create, and "Entrustment scale" while it is read or when its read failed (T7). The subtitle is "A ladder shared by every College and institution." on a scale, "A ladder of at least two rungs, shared by every College and institution." on a create, and none while it reads, on a failure or when unavailable. No header action and no Back link (Q9): the trail is the way back.
- **The column**: `<div class="user-page scale-page">`, `aria-busy` while it reads; the result region (`ActionResult`) first, then one `EditForm` (`novalidate`, no `ValidationSummary`) of two `section.user-section`s, each named by its h2. The sections sit on the page's ground, not in a card.
- **Details** (`h2#scale-details-heading`): `p.field-help` "* marks a field you must fill in.", then a `.form-grid` of **Name \*** (`#scale-name`, full width, `aria-required`) and **Description** (`#scale-description`, a three-row textarea, full width).
- **Levels** (`h2#scale-levels-heading`): `p.field-help` "A rung is read by its label everywhere ("3a", "5"). Its order is its rank, lowest first, and everything that compares levels uses the order: on a ladder with a split rung, the label and the order differ. The order is shown only on this page." Then a `DataTable` with `StackWide`, its class `clinic-table--compact clinic-table--inputs`, captioned "Levels of CPSA Paediatric Entrustment Scale v11.1, lowest first" ("Levels of the new scale, lowest first" on a create). Every row is a set of inputs (DESIGN.md § Table system):
  - **Order**, the row's header (`th scope="row"`, `.col-fit`, S5), the rung's rank as a number;
  - **Label \*** (the header carries the star and a hidden "required"), an `input.form-control` named "Label of the rung in order 3";
  - **Description**, a three-row textarea (S4) named "Description of 3a" ("Description of the rung in order 2" while the label is empty);
  - **on a scale nothing uses** (`EntrustmentScaleDto.InUse` false, T395, Q16), an unlabelled actions column (`.col-actions`, its header the `.visually-hidden` "Actions"): **Up**, **Down** and **Remove**, each `btn btn-outline btn-sm`, named for the rung ("Move Direct supervision up", "Move the rung in order 2 down", "Remove Supervises others"). Up on the first rung and Down on the last are natively `disabled`, as R3-C04 draws them. Remove asks nothing (Q11): nothing is stored until Save, and a refused save offers the rung back (S14). Up, Down and Remove renumber the orders at once.
  - **On a scale anything uses** (the delete's seven uses: an MSF question, a pinned curriculum item, an item on an ended programme's record, scored progress, a sub-speciality's default, a published form's binding, an entrustment decision) there is no actions column (T239), and `p.field-help.scale-in-use-line` "Rungs can be reordered or removed only while nothing uses this scale." follows the table. Adding a rung at the top and relabelling one stay (P2).
- **Add level** (`.btn-outline` with `plus`, in `div.scale-levels-actions`) adds an empty rung at the end, with the next order. A create starts with two empty rungs.
- **The actions** (`FormActions`): **Cancel** (`.btn-outline`, a link to the list), then **Save** ("Saving…" while it runs), or **Create scale** ("Creating…") on a create. Save is never natively disabled (T234): while it runs it is `aria-disabled`, and a second press sends nothing.

### Refusals under their fields (T324)

The page checks before it sends, in the commands' own words, each refusal once under its control as `div.validation-message[role="alert"]` with the id `‹input›-refusal`, the control `aria-invalid` and described by it; no summary. Name: "Enter a name.", "A name is at most 200 characters."; Description (the scale's or a rung's): "A description is at most 2,000 characters."; a rung's Label: "Enter this rung's label.", "A label is at most 200 characters.", and a repeated label (ignoring case) under the later rung's Label, "“Indirect supervision” is already the label of another rung. Give each rung its own label." (R3-C03b). Fewer than two rungs is the one refusal no field holds: `div.validation-message#scale-levels-refusal` (`tabindex="-1"`) above the table, "A scale needs at least two rungs. Add another." (R3-C15c). The command adds two the page does not check: a taken name under Name, "An entrustment scale named 'O-R Scale' already exists." (R3-C14), and a reserved one, "A scale's name cannot be digits alone or start with "seed:", because an activity type's form would read that name as a scale's id or seed key."

### Results and the save's refusals (Q18)

One result region, a result and then the rename warning sharing it (S3):

- **Created** (`?created`, S13): the create goes to the new scale's page, which writes "Entrustment scale created." once and takes the token out of the address (a reload says nothing).
- **Saved**: "Entrustment scale saved.", the rungs as stored. A rename that left a published form's fields with no ladder (T253) adds a warning Alert after it, the command's sentence ("The form of the activity type … names this scale by its old name, …").
- **A save refused for its levels** is one danger Alert, in Q18's order, the handler asking each before it assigns anything (a taken name, under Name, is asked before all four):
  1. **Q6, an item needs a removed rung** (R3-C11): `p` "Not saved.", then `div.scale-put-back` with an outline small **Put rung “5” back** for each removed rung an item needs, then a `ul` of the needs, three named and the rest counted on the third: "PAED-001 on Paediatric EPA Curriculum (11.1) needs the rung “5” of this scale, which the new levels leave out. Change that item's minimum first." … "… Change that item's minimum first. and 6 more." Put back re-inserts the rung where it was, with its id (not a delete and an add), renumbers the rest and focuses its Label.
  2. **An ended programme's record keeps it** (T312, R3-C13), whole: "Not saved. PAED-001 on Paediatric EPA Curriculum (11.1) was on the record of a programme that has ended, which keeps the rung “5” of this scale as its minimum. A closed record keeps the targets it ended with, so the rung “5” cannot be removed." Nothing to put back.
  3. **An entrustment decision is on it** (built, T109): "One or more levels are referenced by entrustment decisions and cannot be removed."
  4. **Q16, the scale came into use since the page was read** (R3-C12): "Not saved. Rungs can be reordered or removed only while nothing uses this scale, and this scale is now in use. The page has been read again." The page reads the scale again, so the moves go and the in-use line appears; nothing is put back.
- **Any other failure** (T393): "The entrustment scale could not be saved. Nothing has changed. Try again, or come back in a few minutes." ("… could not be created. …" on a create), the cause logged. Never the exception's text.

## Reads and focus

The page reads once per id, not when only the query changes (so taking `?created` out reads nothing). A done save, a created scale's result and every refusal in the result region take the focus there (`ActionResult`). A field's refusal focuses the first refused control in the form's order: Name, Description, the Levels refusal, then each rung's Label and Description. Put back focuses the restored rung's Label. Try again focuses the h1 once it has read. `InUse` is the page's read; a save keeps it, so a scale that came into use since is told by Q16's refusal and the re-read, not before.

## States

- **Loading**: `p.loading-line[role="status"]` "Loading this entrustment scale." over a skeleton card, under the h1 "Entrustment scale" (R2-C18a).
- **Load failed**: "Could not load this entrustment scale. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`; R2-C18b), the cause logged.
- **Unavailable** (an unknown id, Q7, R2-C17): the h1 "Entrustment scale unavailable" and its last crumb, `.detail-card--empty` "This entrustment scale does not exist, or you cannot open it." with **Go to Entrustment scales** (primary).

## Narrow widths

The levels grid stacks from 900px down (`StackWide`, the system's rule): each rung a block, its order first, then "Label:" and "Description:" with their controls at the block's width (`.scale-page .clinic-table--inputs .form-control`, 900px query), then Up, Down and Remove. A row header carries no label, so the stacked block opens on the bare number. Above 900px the grid answers to its own width (`.table-container:has(> .clinic-table--inputs)`, container `inputs-grid`): below 44rem its buttons stack and its cells pad by `--space-xs`. The descriptions are textareas at least 4.5rem tall, resizable vertically. Below 641px `.user-page` makes every control 44px and stacks the form's actions with Save on top of Cancel, each the row's width (`column-reverse`: Cancel stays first in the tab order). Measured on the replay at 390 (states `entrustment-scale-edit--narrow`, Step 6.6): nothing sideways.

## Contrast

No new colour token. The required line, the levels' intro and the in-use line in `muted-text` (`field-help`), 5.09:1 on the surface; a refused control's danger stripe and border as the system's (T236).

## Known gaps

- **T396** (proposed): Q16's guard (and Q18's per-item checks) read whether the scale is in use and then save, with no lock between, so a pin committed between the read and the save lets a reorder through.
- Q6's refusal, the ended record's and Put back are reached only by that kind of race: Remove is offered only on a scale nothing uses, and Q6 needs an item pinned to it. The runbook lists Q6's refusal as unreachable from the UI; no step or state captures it.
- The decision refusal is the built T109 sentence: it does not open with "Not saved." as the other three do, and names no rung (build review pages, Words row 20, called it audit-only; the page's default branch shows it).
- Up on the first rung and Down on the last are natively `disabled`, as R3-C04 draws them, where the system's rule for an unavailable action is `aria-disabled` with its reason.
- **T393** (proposed) no longer occurs on this page: no exception's text reaches it (the operator's call to close).
