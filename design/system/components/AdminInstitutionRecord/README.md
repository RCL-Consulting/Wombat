# AdminInstitutionRecord

Create institution and the Administrator's record of an institution (`/admin/institutions/new`, `Pages/Admin/Institutions/InstitutionCreate.razor`; `/admin/institutions/{Id}`, `InstitutionEdit.razor`, the Administrator's branch; its words `InstitutionWords.cs`; the read `GetInstitutionByIdQuery`; the commands `CreateInstitutionCommand`, `UpdateInstitutionCommand`, `DeactivateInstitutionCommand`, `ReactivateInstitutionCommand`; a field-tied refusal `InstitutionUpdateRefusedException`; the addresses `CatalogueLinks.NewInstitution`, `Institution(id)`, `InstitutionCreated(id)`): **an institution's name, short code and contact email, and, apart from them, its status with the one change it allows.** Flow 18 (T400, 2026-11-11, `34b5077e`; boards R2-I17 to I21, R3-I08 to I14, I22; Q5, Q17, T10, T11, T12, T23, T378, K13, K14, K17, M1, N2, N4, S6) rebuilt both. Until then the Administrator's record was "Edit institution" with Back to institutions, an Active box that Save turned into a deactivation, and the exception's text on a failure; the create page was not under `.record-page` (its actions did not stack at 390, its refusals were plain `ValidationMessage`s with no focus, T378). The same route's other reader, an Institutional admin keeping their own institution, is InstitutionRecord (flow 12).

## Who reads it

The Administrator (`/new` is `[Authorize(Policy = "Administrator")]`, so an Institutional admin's is Access denied; the record admits both roles, each in their scope). Reached from Institutions (its header's Create institution, a row's Edit), which both light. **The trail is the way back**: Home › Institutions › Kgosi Kgari Teaching Hospital, Home › Institutions › Create institution; neither page has a Back.

## Create institution

- **The header**: h1 and tab "Create institution"; "A new institution is active from the moment it is saved, so its people can be invited."
- **The column**: `.record-page`, the result region (only a failure, T23: "**Could not save the institution.** Nothing has changed. Try again, or come back in a few minutes."), then `form.form-container`, `novalidate`.
- **The form**: "* marks a field you must fill in."; on `.form-grid`, **Name \*** and **Short code \***, each `aria-required="true"` with a visually hidden " required" after its mark, **never the native `required` and no `maxlength`** (S6, K14: the browser would refuse first in its own words, and a long paste would be cut without a word), and **Contact email** (`type="email"`); each field's help under it, "Up to 200 characters.", "Up to 32 characters.", "Optional."; then **Cancel** (outline, to Institutions) and **Save** ("Saving…" while it runs, `aria-disabled`).
- **Refusals under their fields** (T378): the page checks before anything is sent ("Type the institution's name. The record was not saved.", "Type a short code. The record was not saved.", "Keep the name to 200 characters. The record was not saved.", "Keep the short code to 32 characters. The record was not saved.", "Enter a valid email address, for example name@example.org."); each is a `p.validation-message` under its field, first in its `aria-describedby` before the help, the field marked (`aria-invalid`, `.input-validation-error`), the first refused field focused. A name or short code another institution holds is the command's field-tied refusal, asked before anything is staged, "An institution with the same name or short code already exists." under Short code or Name, naming no institution (R3-I09).
- **Saved**, the page lands on the record (`CatalogueLinks.InstitutionCreated(id)`, `/admin/institutions/7?created`), which reads it in the caller's scope, says "Kosmos Children's Hospital created." in its result region, focuses it and replaces its address without the token (K13), so a reload says nothing again.

## The Administrator's record

- **The header**: h1, tab and last crumb the name ("Institution" before the read and on a failed one); the subtitle the short code and the stored state, "KGK · Active", "DEMO · Inactive" (N2); no header action, no Back.
- **The column**: `.record-page`: the result region, the form, then the Status section.
- **The form**: as Create's, filled; Cancel (to Institutions) and **Save**, which **never asks**: there is no Active box. Saved, "Kgosi Kgari Teaching Hospital saved. Its contact email is now hod.paediatrics@kgk.wombat.local." (`<wbr>` after the @) or "… It has no contact email now." (flow 12's words, T11).
- **Status, a section of its own** (`section.user-section`, `h2` "Status", N4): the stored state on its badge (Active `badge-completed`, Inactive `badge-withdrawn`) and its sentence, "A new invitation can name it." or "No new invitation can name it. Its people, their records and the invitations already sent are kept."; then **the one trigger the stored state allows**, never the other (the picker rule): **Deactivate…** on an active institution, **Reactivate…** on an inactive one, each `.btn-outline` with `aria-haspopup="dialog"`.
- **The two questions** (ConfirmDialog's action mode, M1; each opens on Cancel; Cancel and Escape return to the trigger): "Deactivate Kgosi Kgari Teaching Hospital?" · "No new invitation can be issued for it until it is reactivated. Its people, their records and the invitations already sent are kept. You can reactivate it on this page." · Cancel and the danger **Deactivate**; "Reactivate Demo Institution?" · "A new invitation can name it again. Its people and their records are as they were." · Cancel and the primary **Reactivate** (`DangerAction=false`). The confirm sends its own command, and the result takes the focus: "Kgosi Kgari Teaching Hospital deactivated. A new invitation can no longer name it.", "Demo Institution reactivated. A new invitation can name it again." Typing not yet saved stays in the fields when the status changes (K17).
- **Failures** are T23's fixed sentences, the first sentence bold: "**Could not save the institution.** …", "**Could not deactivate the institution.** …", "**Could not reactivate the institution.** …", each "Nothing has changed. Try again, or come back in a few minutes."; the cause goes to the log, never the exception's text.
- **The race rule** (T234): the Status trigger is natively `disabled` while a save runs, and Save while a status change runs; the pressed one keeps the focus with `aria-disabled`.

## States

- **Loading**: `p.loading-line[role="status"]` over a skeleton card.
- **Load failed**: StatePanel's Alert, "Could not load the institution. Nothing has changed. Try again, or come back in a few minutes." with Try again, which focuses the h1 once it reads.
- **Institution unavailable** (Q17, T10; R2-I19, I20): an unknown id, or one outside the caller's scope, reads "Institution unavailable" (h1 and last crumb) over a `.detail-card--empty`, "This institution does not exist, or you cannot open it.", and one primary link: **Go to Institutions** for the Administrator, **Go to Home** for an Institutional admin, whose trail is Home › Institution unavailable (no Institutions crumb). Never which cause, and no id shown. Until T400 it was Page not found.

## Narrow widths

`.record-page`'s rules: below 641px every control is 44px and the form's actions stack, Save on top of Cancel, both the form's width; the Status trigger is 44px (`.record-page .btn`). A dialog's buttons stack at 390 (ConfirmDialog).

## Contrast

No new colour token. The help `muted-text`, 5.09:1 on the surface; a refusal `danger-color` 5.95:1.

## Known gaps

- No page fault found re-syncing. The Status section's trigger sits in an `.actions-cell` under its sentence, not in `.form-actions`: it is no form's action.
