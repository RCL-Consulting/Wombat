# SubSpecialityRecord

A sub-speciality's record, and Create sub-speciality (`/admin/specialities/{SpecialityId}/sub-specialities/new`, `/admin/specialities/{SpecialityId}/sub-specialities/{Id}`, `Pages/Admin/Institutions/SubSpecialityEdit.razor`; its words `CollegeTreeWords.cs`, the fields' refusals `SpecialityFieldMessages` and `SubSpecialitySaveRefusal`; the reads `GetSpecialityByIdQuery`, `GetCollegeTreeQuery` and `GetEntrustmentScalesListQuery`; the commands `CreateSubSpecialityCommand`, `UpdateSubSpecialityCommand`, `DeactivateSubSpecialityCommand`; refusals under fields through `Components/Shared/FieldRefusal.cs`; the return tokens `CatalogueLinks`): **one sub-speciality of a speciality, as a form, with the entrustment ladder its programmes default to.** Flow 16 (T394, 2026-11-05, de4cc718; boards R3-A05, A06, A08, A13a, A14a, A15, A16a and R2-A18c, A19b, A20b; Q1, Q7, Q9, Q15, D56, D57, S3, S7, S11, S13, S15, M4, M8, N1, N2, N20, T324; fix pass S2 to S5) rebuilt it. Until then it was "Edit sub-speciality" or "Create sub-speciality" subtitled "Speciality: ‹name›", with "Back to sub-specialities" in the header; the default ladder could be chosen only once the sub-speciality existed; Deactivate acted at once and asked nothing (T264); and the read's or save's exception text was printed at the top.

## Who reads it

`[Authorize(Policy = "AdministratorOrCollegeAdmin")]`, each in their scope: the page reads the speciality in the caller's scope (`GetSpecialityByIdQuery`), then its College's tree (`GetCollegeTreeQuery`) for the College, the speciality and the record, so another College's sub-speciality reads as one that does not exist. Reached from the College's tree (CollegeTree): a section's Create sub-speciality, a row's Edit. NavOwners lights the College admin's Specialities and the Administrator's Colleges (`UnderTheCollege`).

The trail puts the College and its tree between the owner and the record, the tree's crumb opening it at the speciality's section:

- **A College admin** (Anton Kruger): Home › Specialities › College of Paediatricians of South Africa › Paediatrics (R3-A05).
- **The Administrator** (devadmin): Home › Colleges › College of Neurologists of South Africa › Specialities › Adult Neurology (R3-A14a).

**The scales** come from `GetEntrustmentScalesListQuery`, every scale on the platform by name, whichever College's ladder it is: a College admin chooses from them as they read them on Entrustment scales (D57).

## The page

- **The header** (S11, M8): h1 and tab the sub-speciality's stored name, "Paediatrics", subtitled "Sub-speciality of Paediatrics" (the subtitle tells it from the speciality of the same name, whose tab reads the same); "Create sub-speciality", subtitled "A sub-speciality of Neurology, in College of Neurologists of South Africa." (R3-A06, A13a); "Sub-speciality" while a record is read and when its read failed (the create keeps "Create sub-speciality"). No header action and no Back link (Q9).
- **The column**: `<div class="user-page sub-speciality-page">`, `aria-busy` while it reads.
- **One result region** (S3): an `ActionResult` at the column's top, a deactivation's result or a failure that is no field's; no row while empty (`.sub-speciality-page .action-result:empty`).
- **The form**: `form.form-container`, `novalidate` (T324). First `p.field-help` "* marks a field you must fill in." (M4), then:
  - **Name \*** (`input#sub-speciality-name`, `aria-required`, `autocomplete="off"`);
  - **Description** (`textarea#sub-speciality-description`, three rows);
  - **Default entrustment scale** (`select#sub-speciality-scale`), **on the create as on the record** (Q1): its first option "No default — offer every scale", then every scale by name ("CNSA Neurology Entrustment Scale", "CPSA Paediatric Entrustment Scale v11.1", "O-R Scale" after Step 6.5). Its help, `small.page-subtitle#sub-speciality-scale-help`, named by the select: "Committee STARs for this programme's trainees are limited to this scale's levels."
  - **Status**, on a record only (DESIGN.md's one Status pattern): `fieldset.form-group`, legend "Status", the box `#sub-speciality-active` labelled **Active**, its help `p.page-subtitle#sub-speciality-active-help` named by the box. Stored active (D56, S15): "An inactive sub-speciality cannot be named by a new invitation, and an institution can no longer give anyone a role for it. Nothing else reads it: its EPAs and curricula, and the registrars on them, carry on." Stored inactive (R3-A16a): "Inactive. A new invitation cannot name it, and no institution can give anyone a role for it. Tick Active and save to reactivate it."
- **The actions** (`FormActions`): **Cancel** (outline link, to the tree at the speciality's section, `…/specialities#speciality-3`, from the create and the record alike); **Deactivate…** (outline, `#sub-speciality-deactivate`, `aria-haspopup="dialog"`) only on a sub-speciality stored active; **Save** (primary submit, `#sub-speciality-save`), "Create sub-speciality" on the create, "Saving…" while its save runs. As on a speciality's record: Save is `aria-disabled`, never natively disabled, while its own save runs (T234), and natively disabled while a deactivation runs; Deactivate… is natively disabled while a save runs; Save carries `aria-haspopup="dialog"` while Active is unticked on a sub-speciality stored active (N20).

## Saving

- **Refusals under their field** (T324, N1), checked before sending and from the command alike: `div.validation-message[role="alert"]` (`#sub-speciality-name-refusal`), named first by the control (before the scale's help, where it has one), the control `aria-invalid`, the first refused field focused, no summary, what was typed kept: "Enter a name.", "A name is at most 200 characters.", "A description is at most 2,000 characters.", and the command's "A sub-speciality with the same name already exists for this speciality."
- **A save or a create returns to the tree** by its token (S13): "Sub-speciality saved." (`?saved=sub:3`), "Sub-speciality Neonatology created." (`?created=sub:4`), focused there (CollegeTree).
- **Any other failure**: a danger `Alert` in the result region, first sentence bold, focused, the cause logged: "**The sub-speciality could not be saved.** Nothing has changed. Try again, or come back in a few minutes." ("… could not be created." / "… could not be deactivated.").

## Deactivating (D56)

Deactivate…, and Save with Active unticked on a sub-speciality stored active, open one `ConfirmDialog` (action mode, `DangerAction`; R3-A15): "Deactivate the sub-speciality Adult Neurology?", then "A new invitation will no longer be able to name it, and an institution can no longer give anyone a role for it. That is all it changes: its EPAs and curricula, and the registrars already on them, carry on." and "You can reactivate it on this page."; no count (Q15). **Cancel** (focused on open) · **Deactivate** (danger). Cancel or Escape returns the focus to the control that asked (Save or Deactivate…); a cancelled Save leaves Active unticked, and Save asks again. Confirmed, the record stays (R3-A16a): "**Sub-speciality Adult Neurology deactivated.** A new invitation can no longer name it.", focused; Active unticked, the inactive help, no Deactivate…. Save's confirm saves every edit with it, the default scale included. Reactivating is the box and Save, asking nothing; it returns to the tree with "Sub-speciality saved.".

## Unavailable

A speciality the caller cannot open, or that does not exist, is **Speciality unavailable** on the create; on a record, that and a sub-speciality its speciality does not hold are **Sub-speciality unavailable** (Q7, N2, R2-A18c). The one Unavailable markup (CollegeTree, SpecialityRecord): "This speciality does not exist, or you cannot open it." or "This sub-speciality does not exist, or you cannot open it." with **Go to Specialities** (the College admin) or **Go to Colleges** (the Administrator); the trail ends at the h1.

## States

- **Loading** (R2-A19b): `p.loading-line[role="status"]` "Loading this sub-speciality." ("Loading this speciality." for the create) over one skeleton card.
- **Load failed** (R2-A20b): "Could not load this sub-speciality. Nothing has changed. Try again, or come back in a few minutes." ("Could not load this speciality. …" for the create) with Try again, which focuses the h1 once it reads (S7).

## Narrow widths

One column at 60rem. Below 641px every control is 44px, the select and the textarea among them; the actions stack with Save on top (`column-reverse`, Cancel first in the tab order), each the row's width; Active's row is a 44px target; the trail folds to its parent; the dialog's buttons stack. Captured at 390 at Step 6.12 (`sub-speciality-edit--narrow`).

## Contrast

No new colour token. The required line and both helps in `muted-text`, 5.09:1 on the form's surface.

## Known gaps

- **Reactivating says only "Sub-speciality saved."** on the tree (T394's states pass 1, unfiled).
- **A scale removed between the page's read and the save** is no field's refusal: the command throws a plain `InvalidOperationException` ("Entrustment scale ‹id› was not found."), so the page says "The sub-speciality could not be saved. …" and the select keeps the gone scale. Inferred from the code (`CreateSubSpecialityCommandHandler`, `UpdateSubSpecialityCommandHandler`); no step reaches it.
- **Save with a new name and Active unticked keeps the old name in the h1, the tab, the trail and the dialog's title**, the result naming the new one, as on a speciality's record. Inferred from the code.
- A duplicate name's refusal names no holder, as a speciality's.
