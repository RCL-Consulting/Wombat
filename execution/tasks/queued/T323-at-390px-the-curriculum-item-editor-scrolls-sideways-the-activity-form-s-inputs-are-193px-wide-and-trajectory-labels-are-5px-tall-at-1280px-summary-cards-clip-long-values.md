---
id: T323
title: At 390px the curriculum item editor scrolls sideways, the activity form's inputs are 193px wide and trajectory labels are 5px tall; at 1280px summary cards clip long values
status: queued
priority: P3
owner: agent
depends_on: []
created: 2026-09-26
---

# T323 — At 390px the curriculum item editor scrolls sideways, the activity form's inputs are 193px wide and trajectory labels are 5px tall; at 1280px summary cards clip long values

> **Compatibility is not a constraint** ([W-007]; CLAUDE.md § "Nothing is live"). There are no real users and no real
> data anywhere: dev and production hold scenario rows only. Do not design around existing rows, stored versions, pinned
> activities or old behaviour. Prefer the correct end state: re-author seeds, write destructive migrations, empty and
> re-seed a database. The scenario corpus is still the test corpus, so show the change working end to end.

**Severity:** Medium. Content a person needs is hidden or unreadable:
- the selected EPA and assessor on the activity form;
- a chart's levels and dates;
- the item editor's help and Save;
- the data-rights record's id, which DESIGN § Page shapes keeps there because it is the record.

Each value can still be reached another way (by scrolling, or by opening the select), which keeps this below High.
**Surfaced:** 2026-09-26, the T295 replay of the rewritten scenario runbook (findings F-A.7.10a, F-A.7.2b, F-A.7.6a, F-A.1.3c, F-A.4.5c).

## Symptom

- **Step A.7.10, 390px, `/admin/curricula/{id}/items`:** Edit on PAED-001 opens a 796px row inside the table's scroll container. The inputs are 370px, two to a row, the help text is cut off at the screen edge, and the container had to be scrolled 490px (`design/baseline/act-A/A.7.10-4-kruger-item-editor-open.png`, `A.7.10-5-kruger-item-editor-viewport.png`).
- **Step A.7.2, 390px, an activity:** the form sits three cards deep, so its inputs are 193px wide in a 343px column, and "PAED-002 - Managing…" and "Mohammed Patel (patel…" are cut off in their selects (`design/baseline/states/activity-view--narrow.png`).
- **Step A.7.6, 390px, review 7 and My progress:** each trajectory chart is scaled from 600px to about 260px, so its axis levels and dates render about 5-6px tall. The companion table is only for screen readers (`design/baseline/act-A/A.7.6-2-zulu-trajectory.png`).
- **Steps A.1.3 and A.4.5, 1280px:** on `/admin/data-rights/{id}` the Request card, about 315px wide, runs the requester's address, the user id and the reason past or into its edge ("dlamini@kgk.wombat.loc", "225e1615-c4fb-4852-b056"). The user page's Account summary clips "duplessis@kgk.wombat.l" (`design/baseline/states/data-rights-request--submitted.png`, `--approved.png`, `--erasure.png`, `user-detail--reset-refused.png`).

## Root cause

- **Item editor.** `CurriculumItemsEdit.razor:189-190` renders the editor as `<tr class="is-editing"><td colspan="9">` inside `.table-container` (`overflow-x: auto`, app.css:163-171). The row is as wide as the table's narrowest width (794px, DESIGN § Table system), and the file's own comment (:183-187) says Save is in view only if the table fits its container. Below about 1170px it does not.
- **Activity form.** `ActivityForm.razor:16` wraps the form in `.form-container` (1px border, `--space-xl` padding, app.css:401-407), and `:24` wraps each section in `fieldset.detail-card--compact`. Its hosts add a third card: `ActivityView.razor:83` (`section.detail-card`), `NewActivity.razor:30` (another `.form-container`), and the read-only `ActivityDetail`. Under 641px nothing narrows `.form-container`'s padding; the only phone rule is `.account-form-container--wide`, app.css:568.
- **Charts.** `TrajectoryChart.razor:9-12,82-83` draws a 600×200 viewBox with 11px labels (app.css:1401-1405). `.trajectory-chart` is `max-width: 100%; height: auto` (1361-1365), so at 260px the labels scale to about 5px. The data table (`:57`) is `.visually-hidden` at every width.
- **Card values.** `.details-list dd` (app.css:915-917) and `.detail-list dd` (933-935) set only `margin: 0`, so the `1fr` track is as wide as the longest unbreakable value, an email or a `<code>` GUID. Since `.details-grid > .detail-card` is `min-width: 0` (1095), the value spills past the card. `RequestDetail.razor:36-46` puts those values in the grid's narrow `1fr` column, and `UserDetail.razor:53-59` does the same.

## What to build

No field, value or chart label is out of reach or unreadable at 390px, and no card clips its values at 1280px.
- **Item editor.** At every width the editor's fields, help and Save fit the table container's visible width. Either:
  - make `.table-container` an inline-size container and pin the editing cell's fieldset to it (`position: sticky; left: 0; width: 100cqi` minus the cell padding); or
  - under 641px, render the editor as a card directly after the table, keep the row marked `.is-editing`, and move the focus to the editor's legend.
  Measure at 390, 700, 1000 and 1280px, and record the result in DESIGN § Table system beside T176's and T198's numbers.
- **Activity form.** One card deep. `ActivityForm` does not add its own `.form-container` when its host is already a card (a parameter, or a `.detail-card > .form-container` reset), and under 641px its sections drop their card padding or become plain fieldsets. The target is inputs at least the column width minus one card's padding (about 290-300px at 390px), with the chosen EPA and assessor legible in their selects. This applies to the activity page, New activity and the read-only view.
- **Charts.** No chart text renders below 11px at 390px, and a phone reader can read every point's date and level. Either:
  - (recommended) under 641px, show the companion table visibly under each chart, since it already carries date, level and assessor, and drop the axis text from the scaled chart; or
  - keep the chart at its natural width inside a focusable scroll region, like the tables (T166).
  Record the choice in DESIGN.
- **Card values.** `.details-list dd` and `.detail-list dd` take `min-width: 0` and `overflow-wrap: anywhere`, and a `<code>` inside them wraps too, so a long email or GUID breaks inside its card at every width. On the data-rights request, consider giving the Request card the grid's wide column.

## Verification

- [ ] `Design/NarrowLayoutTests` gets new facts pinning: the `dd` wrap rule on both list classes; the chart rule under 641px (table visible, or scroll region); and the item editor's containment rule.
- [ ] bUnit: `ActivityView` and `NewActivity` render exactly one card between the page and the form's fields.
- [ ] Browser at 390px, runbook step A.7.10: open Edit on PAED-001 at `/admin/curricula/{id}/items`. No field, help or Save needs sideways scrolling. Cancel, and nothing is saved. Retake A.7.10-4/5.
- [ ] Browser at 390px, step A.7.2: on an activity, the EPA and assessor selects show their whole chosen text and the inputs measure at least 290px. Retake `states/activity-view--narrow.png`.
- [ ] Browser at 390px, step A.7.6: on review 7 and My progress, every trajectory's dates and levels are readable (text at least 11px, or the table visible). Retake A.7.6-2.
- [ ] Browser at 1280px, steps A.1.3 and A.4.5: the data-rights request (submitted, approved, erasure) and the user page show the whole address, user id and reason inside their cards. Retake those four state screenshots.

## Related

T176, T198, T226 (phone-width reflow), T166 (chart fits the width), T266 (builder columns by container width), DESIGN § Table system and § Layout grid, runbook steps A.7.2, A.7.6, A.7.10, A.1.3 and A.4.5.

## Notes

- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26: /admin/invitations at 1280px.** This widens the task by one more case of content out of reach at 1280px. The Active invitations table has ten columns (InvitationsList.razor:131-143) and sits in the grid's second card: `.details-grid` is 1fr 2fr (app.css:1086-1090), so the card is about 630px at 1280. The table scrolls sideways, so Delivery, Resend and Revoke are off-screen until it is scrolled. Delivery is a `.col-wrap` column (14%, min 4rem; app.css:228-235). It falls to about 60px and breaks mid-word ('delivere d.', 'invitatio n'), which makes a 'check the address' row about 800px tall (design/baseline/states/invitations-list--check-address.png, --not-delivered.png, both scrolled to the right edge; the story's invitations-list--being-sent.png shows no Delivery column at all). Give the list the whole row under the issue form, or otherwise make it fit, so that Delivery and the actions are visible without scrolling at 1280 and no word breaks. Measure at 1280 and 1000px, and record the numbers in DESIGN § Table system beside T176's and T198's. Retake those two states.
- **T295 replay, 2026-09-26 (sweep).** **T295 states sweep, 2026-09-26 (design/baseline/states/user-detail--pending-invitations.png).** This widens the card-values item on the user page. Besides the Account summary's email ('botha@kgk.wombat.loca', already listed), two more things. (1) The Pending invitations table (UserDetail.razor:165-192, five columns) sits in a narrow grid column and clips its Speciality column ('Special…'). (2) In the Roles list, the red Remove buttons run straight into the role names (UserDetail.razor:70-90), because `.stack-list` (app.css:946-950) styles the list only and gives an li no flex or gap. Give the Pending invitations card the grid's wide column or the whole row. Lay each role row out with T328's `.list-row` (the name, then the button, with a gap), which lands with that task. Verify at 1280px: the whole address, every pending-invitation column and a clear gap before each Remove. Retake the state.
- **T303 re-check, 2026-09-26 (F-2.28b).** On the user page each Roles row runs the role name into its note with no gap: "PendingTraineeSystem-managed", and since T303 "TraineeSystem-managed" on every admitted trainee's page, which is also the list item's accessible text. The same cause as this task's "Remove button touches the role name" item: `UserDetail.razor` sets two spans side by side in an unstyled `li`, and Razor drops the whitespace (`act-2/2.28-3-molefe-user.png`).
