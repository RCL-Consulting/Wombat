# EntrustmentScales

Entrustment scales (`/admin/entrustment-scales`, `Pages/Admin/EntrustmentScales/EntrustmentScalesList.razor`; its words `EntrustmentScalesWords.cs`; the read `GetEntrustmentScalesListQuery`; the delete `DeleteEntrustmentScaleCommand`, refused by `EntrustmentScaleDeleteRefusedException`): **the rating ladders every assessment form and committee review rates on, platform-wide.** Flow 15 (T392, 2026-11-02, c931d56f; boards R2-E01, E02, E05, R3-E03, E04; Q13, S7, S10, S19, T326; build review P6) made it say to a reader what the ladders are and who sets them, where it had invited an Institutional admin to "define" them (T326), and gave it a loading line and fixed failure words. Scales are global, not a College's: any curriculum item, sub-speciality or form may use any of them. The Administrator's page is as built.

## Who reads it

Administrator and Institutional admin (`AdministratorOrInstitutionalAdmin`; NavMenu's Entrustment scales, `gauge`). Only an Administrator creates, edits or deletes a scale; the Institutional admin reads. It is a list, so it lights itself and has no trail.

## The page

- **The header**: h1 and tab "Entrustment scales". The subtitle is the reader's, **"The rating ladders that assessment forms and committee reviews use. A Wombat administrator sets them; they are shown here for reference."** (Q13, T326), or the Administrator's, "Define and maintain the rating ladders used by assessment forms and committee reviews.". **Create scale** (primary, `plus`) for the Administrator only, once the read is in.
- **The column**: `<div class="people-page scales-page">` (flow 12's list column and its phone rules), the result region (`ActionResult`) first.
- **The list**: a `DataTable` with `StackWide`, captioned "Entrustment scales, by name": **Name**, the row header (`th scope="row"`, `.col-wrap`); **Description** (`.col-wrap.col-wrap--wide`); **Levels**, the count alone (S10), in tabular figures so they line up down the column (`.scales-page .clinic-table td[data-label="Levels"]`); and, **for the Administrator only**, an actions column (`.col-actions`, its header the `.visually-hidden` "Actions"): **Edit** (outline, small) and **Delete** (danger, small), each named by the scale, whose name is unique ("Edit CPSA Paediatric Entrustment Scale v11.1"). A reader is shown no actions column (T239: a column no row offers an action in is not rendered), and no link or button in the page body.
- **Empty**: "No scales yet" over "A Wombat administrator has not set up an entrustment scale yet." (a reader; the seed makes it unreachable) or "Create the first entrustment scale to back assessment ratings and entrustment decisions." (the Administrator).

## Results (the Administrator's)

Delete sends at once (no dialog: T264's open half). Done, "Entrustment scale deleted." and the list read again; refused, the handler's own sentence for the person who pressed, "This entrustment scale is the default scale of the sub-speciality "Adult Neurology" (Neurology), so it cannot be deleted. Change that sub-speciality's default entrustment scale to another scale, or to no default, first." (`RefusalText.Of` of `EntrustmentScaleDeleteRefusedException` only, build review P6); any other failure, "The scale could not be deleted. Nothing has changed. Try again, or come back in a few minutes.", the cause logged (T329). The result takes the focus.

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the entrustment scales." over a skeleton card (S19, R3-E03). No action is offered until the read is in.
- **Load failed**: "Could not load the entrustment scales. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`; S7, R3-E04), which focuses the h1 once it reads.

## Narrow widths

The list stacks from 900px (`StackWide`): each scale a block, its name first, then "Description:", "Levels:" and, for the Administrator, Edit and Delete. Below 641px `.people-page`'s controls are 44px. Measured on the replay at 390 (Step 6.9): nothing sideways.

## Contrast

No new colour token. A stacked cell's label in `muted-text`, 5.09:1 on the surface.

## Known gaps

- **T264**: the Administrator's Delete is a filled danger button that deletes with no dialog, against DESIGN.md § Button system (a destructive action is an outline trigger that opens a `ConfirmDialog`). Step 6.8's gap F-6.8a.
- **T393** (proposed): the scale's edit page, `EntrustmentScaleEdit.razor`, still assigns `exception.Message` (build review P6's leftover).
