# TraineeProfile

A trainee's programme (`/admin/trainees/edit?id=`, `Pages/Admin/Trainees/TraineeProfileEdit.razor`; its words `TraineeProfileWords.cs`, the refusals' `AdmissionText.cs`; the reads `GetTraineeProfileById.cs`, `GetTakeBackForecast.cs`; the ends `CompleteTraineeProfile.cs`, `DeactivateTraineeProfile.cs`, each returning `ProgrammeEndResult(TakenBack)`): **the programme's details, and where it ends**. Flow 13 (T379, 2026-10-27, 5cc10c66; boards R3-P01 to P25; T2, Q13, Q21, Q24, M2, F5, F8, F10, N6, N13, N14, S3, S4, S19, A5; build review B2, P4) rebuilt it in flow 12's column of sections. Until then it was "Trainee profile", ended a programme with two buttons that asked nothing and named no day, refused a day after the dialog, printed its read's exception (T329) and offered Back to trainees.

## Who reads it

Administrator and Institutional admin (`AdministratorOrInstitutionalAdmin`), each in their scope; an id outside it is the Unavailable page, as an unknown one is (S4). Someone who also holds Trainee reads nothing (T278, T290): the header and one standing info alert, `#trainee-profile-trainee-note`, asked before any read. Reached from Trainees' Edit (a running programme) or View (an ended one) (Trainees); the trail is Home › Trainees › ‹name›'s programme.

## The page

- **The header** (the title rule, T2): h1, tab and crumb "Lerato Molefe's programme" (`PortfolioWords.ProgrammeOf`); "Trainee profile" while it loads and when its read failed; "Profile unavailable" for an id it cannot open. Subtitle "Update curriculum and completion details for this trainee." (running) or "The record of a programme that has ended." No Back: the trail returns. **Its one action**: "Export Lerato Molefe's portfolio" (`ExportPortfolioAction`, outline, `download`) where the export would not be refused (`MayExport`) and the person is not erased (ExportPortfolio, PageHeader).
- **The column**: `.user-page` (flow 12's: a flex column of `section.detail-card.user-section`s 24px apart, each an h2):
  - **The summary** (N6, F10): `dl.detail-list` named "Trainee summary" (`aria-label`; no visible heading, no Name row): **Status** "Active" (a plain word), or the green **Completed** badge and the day ("Completed 2026-10-27"), or the grey **Inactive** badge (`badge-withdrawn`) and "Left the programme: 2026-10-25" (Badge); **Email**, `<wbr>` after the @.
  - **Profile details** (`h2#profile-details-heading`). Running: a form (`OnSubmit`, `novalidate`, P4), on `.form-grid`, **Curriculum \*** (the versions the command accepts, T304) and **Programme start date \*** (both plain controls with `aria-required`) and **Expected completion date** (help "Leave this empty to derive it from the curriculum window."); `.form-actions` **Cancel** (outline, to Trainees) and **Save profile** (primary). Ended: "This programme has ended, so its record is archived and cannot be changed." over a `dl.detail-list` of Curriculum, Programme start date and Expected completion date as `dd.readonly-value`s (T305).
  - **End this programme** (running only; `h2#end-heading`, no `tabindex`, N14; the section itself, no form element, A2): the last ratified committee decision, read and gating nothing (Q13): "The last committee decision: Satisfactory Progress, Annual progression review, ratified 2026-10-27."; then **Last day in the programme \*** (`input#programme-end-date`, `type="date"`, `aria-required`), its help "The graduation day, or the last day of a registrar who leaves without completing. It cannot be after today, and it cannot be changed afterwards. The period it falls in holds no target unless the day is in that period's last month, and encounters observed after it count towards nothing in this programme, even those already counted."; then flow 12's role rows (`ul.role-rows`), each the note and its outline trigger (Button): "A graduation: archives the profile, removes the Trainee role and sends Anele Dlamini a graduation email." · **Mark complete…** (`#end-mark-complete`); "A withdrawal: archives the profile and removes the Trainee role. No email is sent." · **Deactivate…** (`#end-deactivate`). Each trigger `aria-haspopup="dialog"`, `aria-describedby` its note, `aria-disabled` while an end runs.
  - **The account** (ended only, F5; `h2#account-heading`): "Lerato Molefe holds no role now, and can still sign in to read the record, export the portfolio and download the STAR certificates." ("… read the record and export the portfolio." with no STAR ever issued; "Pieter du Plessis now holds Assessor. They can still read the record and export the portfolio." with a role), then `p.account-open` **Open Lerato Molefe's user page** (UserPage). A locked account has the link alone; an erased person, whose record is read under the pseudonym, has no account section.

## The end (Q21, Q24, M2, S3, S19)

1. **The press checks the day first**, by the rule both commands hold it to (`TraineeProfile.EndDayRefusal`), on the South African today read at the press (B2): none typed, "Choose the last day."; before the start, "The last day cannot be before the programme start date (2023-01-15)."; after today, "The last day cannot be after today (2026-10-27).". Refused, **no dialog opens**: the refusal is a `div.validation-message[role="alert"]#programme-end-refusal` under the field, named first by its `aria-describedby`, the input `aria-invalid`, the focus in it, the typed day kept.
2. **Otherwise the dialog opens** with what the end would take back for that day, read as it opens (`GetTakeBackForecastQuery`, the commands' own count): Mark complete's confirms with the primary, Deactivate's with the filled danger button (ConfirmDialog, "Flow 13's two").
3. **The result** sits at the top of the column, above the summary, in an `ActionResult` that takes the focus, its first sentence bold, the page now the ended record: "**Trainee marked complete.** The Trainee role has been removed and a graduation email sent. 1 activity observed after that day no longer counts." / "**Trainee profile deactivated.** The last day in the programme is recorded as 2026-10-25, and the Trainee role has been removed." (the count only when above zero, from the command's `TakenBack`).
4. **The command's own refusal** at confirm shows under the field as the press-time one does (P07); **any other failure** is fixed words at the section's top, focused: "**The programme could not be ended just now.** Nothing has changed. Try again, or come back in a few minutes."

## Saving the details (P4)

The page's own checks sit under their field, `p.validation-message`, the field `aria-invalid` and described by it, the first focused; a changed field clears its refusal: "Choose a curriculum.", "Enter the programme start." (Admit's words). No `ValidationSummary`. The command's own refusal (a validation, scope or state refusal) is the section's result above the form, in its words, none bold, focused. Saved, the result is above the form in its section, focused, the command's message with its first sentence bold (a move, or a change of start, says what its replay checked, T304). Failed: "The profile could not be saved just now. Nothing has changed. Try again, or come back in a few minutes."

## States

- **Loading**: `p.loading-line[role="status"]` "Loading this trainee profile." over three skeleton cards, `aria-busy` on the column.
- **Load failed**: "Could not load this trainee profile. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), which focuses the h1.
- **Profile unavailable** (S4): the empty card "This trainee profile does not exist, or you cannot open it." and **Go to Trainees** (`btn-primary`, `arrow-right`).

## Narrow widths

Below 641px flow 12's shared rules hold: every control on `.user-page` 44px; a role row stacks, its note over the trigger, the trigger the row's width; Profile details' actions stack with Save profile on top of Cancel (`column-reverse`, Cancel first in the tab order); the dialog's buttons stack with the action on top. The account's link is a 44px target (`.user-page .account-open a`; 1.5rem from 641px). The header's Export action takes the row. Measured on the replay (wombat_scenario_t379, Nolwazi Mbatha: Anele Dlamini running, Lerato Molefe completed, Pieter du Plessis withdrawn) at 1280 and 390: no sideways scroll, every control 44px at 390.

## Contrast

No new colour token. The notes and help `muted-text`, 5.09:1 on the surface; Inactive's badge as Badge's neutral.

## Known gaps

- **Found re-syncing and fixed** (`73baa556`): Save profile's failure now bolds its first sentence, "**The profile could not be saved just now.** Nothing has changed. Try again, or come back in a few minutes.", as the end's failure does (`TraineeProfileWords.SaveFailedLead`, `SaveFailedRest`). Not drawn on the boards.
- One row of the states pass is uncaptured, `trainee-profile-edit--end-failed` (it needs a forced failure).
- Both dialogs are in the DOM, closed, on an ended record too; nothing opens them there.
