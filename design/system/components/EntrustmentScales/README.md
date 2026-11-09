# EntrustmentScales

Entrustment scales (`/admin/entrustment-scales`, `Pages/Admin/EntrustmentScales/EntrustmentScalesList.razor`; its words `EntrustmentScalesWords.cs`; the read `GetEntrustmentScalesListQuery`; the delete `DeleteEntrustmentScaleCommand`, refused by `EntrustmentScaleDeleteRefusedException` with its `Holders`): **the rating ladders every assessment form and committee review rates on, platform-wide.** Flow 15 (T392, 2026-11-02, c931d56f; boards R2-E01, E02, E05, R3-E03, E04; Q13, S7, S10, S19, T326; build review P6) made it say to a reader what the ladders are and who sets them, where it had invited an Institutional admin to "define" them (T326), and gave it a loading line and fixed failure words. Flow 16 (T394, 2026-11-05, de4cc718, with f7cb1903: the one-word Levels column fitted from 1025px; boards R2-C01, C06, C07, C21a, C21b, R3-C08 to C10; D57, Q11, S6, N11) admitted the College admin as a reader, made Delete an outline trigger that asks first (T264's open half on this page; until then a filled red Delete deleted on the press), and linked the refusal to the sub-specialities that hold the scale. Scales are global, not a College's: any curriculum item, sub-speciality or form may use any of them. A scale's own page is EntrustmentScale.

## Who reads it

Administrator, College admin and Institutional admin (`[Authorize(Policy = "NationalCatalogueAccess")]`; NavMenu's Entrustment scales, `gauge`: in the Administrator's Catalogue group, the College admin's one ungrouped list (D57) and the Institutional admin's Curriculum group). Only an Administrator creates, edits or deletes a scale; the College admin and the Institutional admin read it, so the College can read the ladder its own sub-specialities default to (D57). It is a list, so it lights itself and has no trail.

## The page

- **The header**: h1 and tab "Entrustment scales". The subtitle is the reader's, **"The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference."** (Q13, T326; the College admin's too), or the Administrator's, "Define and maintain the rating ladders used by assessment forms and committee reviews.". **Create scale** (primary, `plus`) for the Administrator only, once the read is in.
- **The column**: `<div class="people-page scales-page">` (flow 12's list column and its phone rules), the result region (`ActionResult`) first.
- **The list**: a `DataTable` with `StackWide`, captioned "Entrustment scales, by name": **Name**, the row header (`th scope="row"`, `.col-wrap`); **Description** (`.col-wrap.col-wrap--wide`); **Levels**, the count alone (S10), `.col-fit--wide`, in tabular figures so they line up down the column (`.scales-page .clinic-table td[data-label="Levels"]`); and, **for the Administrator only**, an actions column (`.col-actions`, its header the `.visually-hidden` "Actions"): **Edit** (outline, small, to the scale's page) and **Delete…** (outline, small, `#scale-delete-{id}`, `aria-haspopup="dialog"`; N11), each named by the scale, whose name is unique ("Edit CNSA Neurology Entrustment Scale", "Delete CNSA Neurology Entrustment Scale"). A reader is shown no actions column (T239: a column no row offers an action in is not rendered), and no link or button in the page body.
- **Empty**: "No scales yet" over "A Wombat administrator has not set up an entrustment scale yet." (a reader; the seed makes it unreachable) or "Create the first entrustment scale to back assessment ratings and entrustment decisions." (the Administrator).

## Delete… (the Administrator's)

- **The dialog** (`ConfirmDialog`, action mode, `DangerAction`; Q11, R2-C07): "Delete CNSA Neurology Entrustment Scale?", then "It is removed from every list. This cannot be undone." and "A scale that anything uses is not deleted: you will be told what uses it, and nothing changes."; **Cancel** (first, focused on open) and **Delete scale** (danger). Cancel or Escape returns the focus to the row's Delete…. While a delete runs, a second Delete… opens nothing.
- **Deleted** (R3-C10): "Entrustment scale deleted.", the list read again.
- **Refused** (R3-C08): the handler's own sentence for the first use it finds (in its order: an MSF question, a pinned curriculum item, an item on an ended programme's record, scored progress, a sub-speciality's default, a published form's binding, an entrustment decision), as a `p` in a danger Alert, word for word: "This entrustment scale is the default scale of the sub-speciality "Adult Neurology" (Neurology), so it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to no default, first." When the use is a sub-speciality's default, `p.scales-refusal-links` under it links each sub-speciality that holds the scale to its page (S6): an outline small **Open Adult Neurology**, named "Open Adult Neurology, the sub-speciality" (the speciality added in brackets where two share a name, `RowNames.Distinct`), three at most, then "and 2 more.". The list is not read again.
- **Any other failure** (T329): "The scale could not be deleted. Nothing has changed. Try again, or come back in a few minutes.", the cause logged.
- The result takes the focus once the dialog has closed (`ActionResult.FocusAfterRender`).

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the entrustment scales." over a skeleton card (S19, R3-E03). No action is offered until the read is in.
- **Load failed**: "Could not load the entrustment scales. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`; S7, R3-E04), which focuses the h1 once it reads.

## Narrow widths

**From 1025px the text takes the room** (f7cb1903, the states pass 2's finding that Levels took 634px beside a seven-line description): Levels is `.col-fit--wide` (no wrap, 1%), Name 28% and Description 45% (`.scales-page .clinic-table th.col-wrap`, app.css's `min-width: 1025px` block under "Flow 15: EPAs"). **At 900px and below the list stacks** (`StackWide`, the system's rule; not the catalogue's 1024px stack): each scale a block, its name first, then "Description:", "Levels:" and, for the Administrator, Edit and Delete…, 44px. Below 641px `.people-page`'s controls are 44px, the dialog's buttons stack with Delete scale on top (ConfirmDialog), and the refusal's links are 44px (`.scales-page .scales-refusal-links .btn`). Measured on the replay at 390 (Step 6.9) and swept 390 to 1440 after f7cb1903: nothing sideways.

## Contrast

No new colour token. A stacked cell's label in `muted-text`, 5.09:1 on the surface; the outline triggers 4.86:1; the danger button only in the dialog, `on-fill` 5.95:1.

## Known gaps

- **T264** (queued): this page's half no longer occurs (Delete… asks first, Step 6.8); the task's other pages and pagers are open.
- The refusal names only the first use the handler finds; the holder links are drawn only for a sub-speciality's default (`EntrustmentScaleUse.IsSubSpecialityDefault`). A scale pinned to items, for instance, is refused with no way to them.
