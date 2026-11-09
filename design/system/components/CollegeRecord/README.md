# CollegeRecord

A College's record (`/admin/colleges/new`, `/admin/colleges/{Id}`, `Pages/Admin/Colleges/CollegeEdit.razor`; its words `CollegeWords.cs`, the commands' refusals `CollegeHolders.cs`; the read `GetCollegeByIdQuery`, `CollegeDto`; the commands `CreateCollegeCommand`, `UpdateCollegeCommand`, `DeactivateCollegeCommand`; refusals under fields through `Components/Shared/FieldRefusal.cs`; the result token `CatalogueLinks`): **one College's name, short code and description, and whether a new invitation may name it.** Flow 16 (T394, 2026-11-05, de4cc718; boards R3-B03 to B12, B17b, R2-B13, B14b, B15b; D56, Q7, Q9, Q15, S10, S11, S13, S15, N20, T7, T272, T324, T329; fix pass S2, S3, S5) rebuilt it. Until then it was "Edit college" for every College, offered Back to colleges, put a refusal at the top in a validation summary, printed an exception's text, sent an unknown id to Page not found, and deactivated at once on a filled danger "Deactivate" that asked nothing (T264).

## Who reads it

The Administrator alone (`[Authorize(Policy = "Administrator")]`). A College admin is Access denied here (Step 6.10): the College keeps what it owns on its tree, the Administrator keeps the College itself. Reached from Colleges' Edit or Create college; Colleges is lit (`NavOwners`), and the trail is Home › Colleges › the page's h1.

## The page

- **The header** (S11, T7): h1, tab and last crumb the College's name, "College of Paediatricians of South Africa" (its tab "College of Paediatricians of South Africa · Wombat"); "Create college" for a new one; "College" while the record is read and when its read failed; "College unavailable" for an id it cannot open. The subtitle: "CPSA · Active" (`CollegeWords.RecordSubtitle`, S15; "CNSA · Inactive" once deactivated), or "A national College keeps its own specialities, EPAs and curricula. It starts active." on a create; none while it reads, after a failed read and on Unavailable. No header action and no Back to colleges: the trail is the way back (Q9).
- **The column**: `<div class="user-page college-page">` (flow 12's column of sections and its phone rules), `aria-busy` while it reads. `.college-page .form-container` is one column of full-width fields, at most 48rem (app.css "Flow 16: Colleges and the scales").
- **The result region** (`ActionResult`) first, then `.form-container` holding an `EditForm` (`novalidate`): `p.field-help` "* marks a field you must fill in." (`RequiredMark.Line`), then `.form-grid`:
  - **Name \*** (`input#college-name`, `aria-required`, full width), **Short code \*** (`#college-short-code`), **Description** (`textarea#college-description`, four rows).
  - **Status** on an existing College only (D56; fix pass S5, one pattern with the speciality's and the sub-speciality's): `fieldset.form-group.full-width` whose legend is "Status", the lone **Active** box (`#college-active`) with its own label, so its name is "Active", described by its help `p.page-subtitle#college-active-help`. Stored active, the help says what an inactive College stops: "An inactive College cannot be named by a new invitation. Nothing else reads it: its specialities, EPAs and curricula, and every institution that has adopted one of its curricula, carry on as they are." Stored inactive, the help gives way to "Inactive. A new invitation cannot name this College. Tick Active and save to reactivate it." (R3-B11a). There is no Reactivate button.
- **The actions** (`FormActions`): **Cancel** (outline, to Colleges); **Deactivate…** (outline, `#college-deactivate`, `aria-haspopup="dialog"`) only on a College stored active; **Save** (primary, `#college-save`), "Create college" on a create, "Saving…" and "Creating…" while it runs. Save is `aria-disabled` and not natively disabled during its own save (T234: it holds the focus, and a second press sends nothing), and natively disabled while a deactivation runs; Deactivate… is natively disabled while either runs. **Save carries `aria-haspopup="dialog"` while it would deactivate** (Active unticked on a College stored active, N20).

## Refusals under their field (S10, T324)

Save checks the form first, in its order, each under its own field and each once: "Enter a name.", "A name is at most 200 characters.", "Enter a short code.", "A short code is at most 32 characters.", "A description is at most 1,000 characters." (`CollegeWords`, the same sentences as `CollegeHolders`, so the page and the commands say the same). Nothing is sent. A command's own refusal of one field arrives as a `FieldRefusalException` (`FieldRefusal.Of`) and lands the same way, **naming the holder**: "CPSA is already the short code of College of Paediatricians of South Africa. Choose another." under Short code (Step 6.1), "College of Paediatricians of South Africa is already a College's name. Choose another." under Name (the name first when both are taken). Each is a `div.validation-message[role="alert"]` with the id `‹input›-refusal` (`college-short-code-refusal`), named by the control's `aria-describedby`, the control marked `aria-invalid` and `.invalid` (the form's own message store), the first refused field focused, what was typed kept. No `ValidationSummary`. Any other failure is fixed words in the result region, focused, the cause logged (T272, T329): "The College could not be saved. Nothing has changed. Try again, or come back in a few minutes." (R3-B12), "The College could not be created. Nothing has changed. Try again, or come back in a few minutes." on a create.

## Deactivate (D56, Q15, N20)

Deactivate…, and Save with Active unticked on a College stored active, open **one** `ConfirmDialog` (action mode, `DangerAction`, so it opens on Cancel), named for the College: "Deactivate College of Neurologists of South Africa?" over "A new invitation will no longer be able to name it. That is all it changes: its specialities, EPAs and curricula stay as they are, every institution that has adopted one of its curricula carries on, and everyone already invited keeps their invitation." and "You can reactivate it on this page."; **Cancel** · **Deactivate** (the filled danger button). No count is read into it (Q15). Cancel or Escape returns the focus to the control that asked: Deactivate… after Deactivate…, **Save after Save** (fix pass S2; `ShowAsync(returnFocusId)`, DESIGN.md § Button system); a Save it interrupted leaves Active unticked, unsaved, and Save asks again (R3-B10). Confirmed from Deactivate…, it sends the deactivation alone (`DeactivateCollegeCommand`); confirmed from Save, it saves every edit with it.

## Results (S13, T234)

One result at a time, in the `ActionResult` at the column's top, focused once the action has answered and any dialog has closed, an `Alert` whose words are not bolded:

- "College saved." (R3-B08): Save stays on the record.
- "College of Neurologists of South Africa deactivated. A new invitation can no longer name it." (R3-B11a): the subtitle reads "CNSA · Inactive", the help the inactive line, and Deactivate… is gone.
- "College of Neurologists of South Africa reactivated. A new invitation can name it again.": Active ticked on a College stored inactive, and Save.
- **A create leaves for its new College's record** by `?created` (`CatalogueLinks.CollegeCreated`, `/admin/colleges/3?created`): that page writes "College created." itself, focuses it, then replaces its address without the token, so a reload says nothing (R3-B06).
- A failed deactivation: "The College could not be deactivated. Nothing has changed. Try again, or come back in a few minutes.", focused once the dialog has closed (no board draws it).

## States

- **Loading**: h1 and crumb "College", `p.loading-line[role="status"]` "Loading this College." over one skeleton card (R2-B14b). A create reads nothing.
- **Load failed**: h1 "College", "Could not load this College. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), which focuses the h1 once it has read (R2-B15b). The cause goes to the log; a failed read is not Unavailable.
- **Unavailable** (Q7, R2-B13): an id the read does not return (`/admin/colleges/999999`), h1 and last crumb "College unavailable" in the page's own place, never Page not found; the empty card `.detail-card.detail-card--empty` holding `div.state-panel-copy` "This College does not exist, or you cannot open it." and `div.form-actions` with **Go to Colleges** (primary). No id shown (DESIGN.md § Alerts, one Unavailable markup).

## Narrow widths

Below 641px every control on the page is 44px (`.user-page`), the Active row a 44px target with a 20px box (app.css's flow 16 shared checkbox-row rule), and the actions stack, each the form's width, **Save on top, then Deactivate…, then Cancel** (`.user-page .form-actions`, `column-reverse`: the DOM keeps Cancel first in the tab order). The trail folds to one 44px "Colleges" link back (Breadcrumbs). The dialog's buttons stack, Deactivate on top of Cancel (ConfirmDialog). Captured at 390 (`college-edit--narrow`, Step 6.2): nothing sideways.

## Contrast

No new colour token. The required line, the Active help and the inactive line in `muted-text`, 5.09:1 on the surface; the outline buttons 4.86:1; the danger button only in the dialog.

## Known gaps

- **The empty result region takes a row**: there is no `.college-page .action-result:empty { display: none; }`, as the EPA's, the speciality's, the sub-speciality's and the curriculum's pages have, so `.user-page`'s 24px gap sits twice between the header and the form while nothing has been done (the 390 capture shows it). One rule in app.css's "Flow 16: Colleges and the scales".
- **The results bold nothing**: the College's tree, a speciality's and a sub-speciality's record bold their first sentence (`TreeResult`, BRIEF § 11's flow 11 rule); this page writes `_statusMessage` plain, so "College of Neurologists of South Africa deactivated." is not set off from "A new invitation can no longer name it.".
- **Deactivate… sends the flag alone and keeps the rest unsaved**: an edit typed before it (a new description) stays in the form, unsaved, under "… deactivated.", with nothing saying so; Save still sends it. Inferred from `DeactivateAsync`; no step reaches it.
