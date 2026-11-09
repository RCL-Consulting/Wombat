# SpecialityRecord

A speciality's record, and Create speciality (`/admin/colleges/{CollegeId}/specialities/new`, `/admin/colleges/{CollegeId}/specialities/{Id}`, `Pages/Admin/Institutions/SpecialityEdit.razor`; its words `CollegeTreeWords.cs`, the fields' refusals `SpecialityFieldMessages` and `SpecialitySaveRefusal`; the read `GetCollegeTreeQuery`; the commands `CreateSpecialityCommand`, `UpdateSpecialityCommand`, `DeactivateSpecialityCommand`; refusals under fields through `Components/Shared/FieldRefusal.cs` and `FieldRefusalException`; the return tokens `CatalogueLinks`): **one speciality of a College, as a form, titled by its name.** Flow 16 (T394, 2026-11-05, de4cc718; boards R3-A03, A04, A09, A11, A17 and R2-A18b, A19b, A20b; Q7, Q9, Q15, D56, S3, S7, S11, S13, S15, M4, M8, N1, N2, N10, N20, T324; fix pass S2 to S5) rebuilt it. Until then it was "Edit speciality" or "Create speciality" subtitled "College: ‹name› (‹short code›)", with "Back to specialities" in the header, a red Deactivate that acted at once and asked nothing (T264), refusals in `ValidationMessage`s and the read's or save's exception text printed at the top.

## Who reads it

`[Authorize(Policy = "AdministratorOrCollegeAdmin")]`, each in their scope: the page reads the College's tree (`GetCollegeTreeQuery`), so another College's id, or an unknown one, reads as one that does not exist. Reached from the College's tree (CollegeTree): Create speciality in its header, a section's Edit. NavOwners lights the College admin's Specialities and the Administrator's Colleges (`UnderTheCollege`).

The trail puts the College and its tree between the owner and the record:

- **A College admin** (Anton Kruger): Home › Specialities › College of Paediatricians of South Africa › Paediatrics, the College's crumb opening the tree.
- **The Administrator** (devadmin): Home › Colleges › College of Neurologists of South Africa › Specialities › Create speciality, the College's crumb opening its record and Specialities the tree.

## The page

- **The header** (S11, M8): h1 and tab the speciality's stored name, "Paediatrics", subtitled "Speciality of College of Paediatricians of South Africa"; "Create speciality", subtitled "A speciality of College of Neurologists of South Africa." (R3-A11); "Speciality" while a record is read and when its read failed (the create keeps "Create speciality"). No header action and no Back link (Q9): Cancel and the trail return to the tree.
- **The column**: `<div class="user-page speciality-page">` (flow 12's column at 60rem, its parts 24px apart, and its phone rules), `aria-busy` while it reads.
- **One result region** (S3): an `ActionResult` at the column's top, holding a deactivation's result or a failure that is no field's; it takes no row while empty (`.speciality-page .action-result:empty`).
- **The form**: `EditForm` as `form.form-container`, `novalidate` (T324: the page refuses in its own words). First `p.field-help` "* marks a field you must fill in." (M4, `RequiredMark.Line`), then:
  - **Name \*** (`input#speciality-name`, `aria-required`, `autocomplete="off"`);
  - **Description** (`textarea#speciality-description`, three rows);
  - **Status**, on a record only (fix pass S5; DESIGN.md's one Status pattern): `fieldset.form-group`, legend "Status", the lone box `#speciality-active` labelled **Active**, its help a `p.page-subtitle#speciality-active-help` named by the box's `aria-describedby`. Stored active (D56, S15): "An inactive speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on." Stored inactive: "Inactive. A new invitation cannot name it, and no institution can give anyone a role for it or any of its sub-specialities. Tick Active and save to reactivate it." The help follows what is stored, not the box.
- **The actions** (`FormActions`): **Cancel** (outline link, to the tree at the speciality's section, `…/specialities#speciality-2`; to the tree's top from Create speciality); **Deactivate…** (outline, `#speciality-deactivate`, `aria-haspopup="dialog"`) only on a speciality stored active; **Save** (primary submit, `#speciality-save`), "Create speciality" on the create, "Saving…" while its save runs. Save is never natively disabled while its own save runs (`aria-disabled`, T234; a second press sends nothing); it is natively disabled while a deactivation runs, and Deactivate… is natively disabled while a save runs. While Active is unticked on a speciality stored active, Save carries `aria-haspopup="dialog"`: it asks first (N20).

## Saving

- **Refusals under their field** (T324, N1): the page checks before it sends, and the command's `FieldRefusalException` or validator failure lands the same way (`FieldRefusal.Of`). Each is a `div.validation-message[role="alert"]` under its control, `#speciality-name-refusal`, named first by the control's `aria-describedby`, the control `aria-invalid` and marked; the first refused field in the form's order takes the focus; no summary, nothing at the top, what was typed kept. The words: "Enter a name.", "A name is at most 200 characters.", "A description is at most 2,000 characters.", and the command's "A speciality with the same name already exists for this college."
- **A save or a create returns to the tree** by its token (S13): `?saved=speciality:2` writes "Speciality saved." there, `?created=speciality:3` "Speciality Neurology created.", each focused (CollegeTree).
- **Any other failure** is the fixed sentence in the result region, a danger `Alert` with its first sentence bold, focused, the cause to the log (T272, T393): "**The speciality could not be saved.** Nothing has changed. Try again, or come back in a few minutes." ("**The speciality could not be created.** …"; "**The speciality could not be deactivated.** …").

## Deactivating (D56)

- **Two actions deactivate, and both ask first**: Deactivate…, and Save with Active unticked on a speciality stored active (N20). They open one `ConfirmDialog` (action mode, `DangerAction`), titled "Deactivate the speciality Neurology?" (R3-A17), its body two paragraphs: "A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it or any of its sub-specialities. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on." and "You can reactivate it on this page." No count is read (Q15). **Cancel** (focused when it opens) · **Deactivate** (danger).
- **Cancel or Escape returns the focus to the control that asked** (fix pass S2): Deactivate… after Deactivate…, Save after Save (`ConfirmDialog.ShowAsync(returnFocusId)`). Cancelling Save's question leaves Active unticked and unsaved; Save asks again.
- **Confirmed, the record stays** with its result, focused once the dialog has closed: "**Speciality Neurology deactivated.** A new invitation can no longer name it." Active is unticked, the help becomes the inactive line, and Deactivate… is gone. Confirming Save's question saves every edit on the form with the deactivation (`UpdateSpecialityCommand`); Deactivate…'s confirm sends `DeactivateSpecialityCommand` alone.
- **Reactivating asks nothing**: tick Active and Save; there is no Reactivate button. The save returns to the tree with "Speciality saved.".

## Unavailable

A College the caller cannot open, or that does not exist, is **College unavailable** on the create; on a record, that and a speciality the College does not hold are **Speciality unavailable** (Q7, N2, R2-A18b). DESIGN.md's one Unavailable markup: h1 and tab the heading, no subtitle, a `.detail-card.detail-card--empty` with `div.state-panel-copy` "This College does not exist, or you cannot open it." or "This speciality does not exist, or you cannot open it.", then one primary link, no icon: **Go to Specialities** (the College admin) or **Go to Colleges** (the Administrator). The trail ends at the h1: Home › Specialities › Speciality unavailable.

## States

- **Loading** (R2-A19b): `p.loading-line[role="status"]` "Loading this speciality." ("Loading this College." for the create) over one skeleton card.
- **Load failed** (R2-A20b; T329): "Could not load this speciality. Nothing has changed. Try again, or come back in a few minutes." ("Could not load this College. …" for the create) with Try again (StatePanel, `refresh-cw`), which focuses the h1 once it reads (S7). A failed read is not Unavailable.

## Narrow widths

The form is one column at the page's 60rem measure, every field the column's width. Below 641px (flow 12's `.user-page` rules) every button, input and textarea is 44px, and the actions stack with Save on top and Cancel last on screen (`column-reverse`; the DOM keeps Cancel first in the tab order), each the row's width; Active's row is a 44px target with a 20px box (flow 16's shared rule, `.user-page .form-check`); the trail folds to its parent; the dialog's buttons stack with Deactivate on top (ConfirmDialog). Captured at 390 at Step 6.11 (`speciality-edit--narrow`).

## Contrast

No new colour token. The required line and Status's help in `muted-text`, 5.09:1 on the form's surface; a refusal as FormField's; the danger button only in the dialog.

## Known gaps

- **Reactivating says only "Speciality saved."** on the tree (T394's states pass 1, unfiled): a deactivation has its own result on the record, a reactivation none; the College's record says "reactivated".
- **A duplicate name's refusal names no holder** (T394's states pass 1, unfiled): "A speciality with the same name already exists for this college." does not say which, and writes "college" where every other sentence writes "College".
- **Save with a new name and Active unticked keeps the old name in the h1, the tab, the trail and the dialog's title**, while the result names the new one ("Speciality ‹new name› deactivated."): the page sets its heading from the speciality it read, and a deactivating save stays on the page without reading it again (`Deactivated(saved.Name)` changes only the result). Inferred from the code; no step reaches it.
