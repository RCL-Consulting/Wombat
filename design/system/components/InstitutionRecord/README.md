# InstitutionRecord

The Institutional admin's own institution (`/admin/institutions/{Id}`, `Pages/Admin/Institutions/InstitutionEdit.razor`, her branch; the command's refusals `InstitutionUpdateRefusedException`): its name, short code and contact email, which she keeps, and its status, which she reads. Flow 12 (T376, 2026-10-23, e53d4ae7; boards R3-I01 to I08; S9, S12, S22, M8; T291 item 7, T302, T324) rebuilt her view. The Administrator's view of the same route (Edit institution, Back to institutions, the Active box and Deactivate) is flow 18's, kept as built; `/admin/institutions/new` is now a page of its own, the Administrator's alone (`InstitutionCreate.razor`, S12), so hers is Access denied.

## The page

- **The header**: h1 and tab the institution's name ("Institution" before the read and on a failed one, S9); "Your institution's name, short code and contact email."; no header action. **The trail** is Home › ‹name›: her institution has no owner in the menu, so it is reached from Home's Your institution card (InstitutionalAdminHome).
- **The column**: `.record-page`, a flex column 24px apart, at most 52rem: the result region (hidden while empty), then the form.
- **The form**: `form.form-container`, `novalidate`, named by the institution's name; "* marks a field you must fill in."; on `.form-grid`, **Name \***, **Short code \*** and **Contact email** (`type="email"`); then **Status read out** (M8): `dl.form-group`, `dd.readonly-value` "Active" or "Inactive" over `dd.field-help` "Set by a Wombat administrator."; then `FormActions`: **Cancel** (outline, to Home) and **Save changes** (primary; `aria-disabled` while it runs).

## Refusals and the result

- **No `ValidationSummary`** (S22): the page's own checks and the command's typed refusals (by `Field`, never by its text) sit under their field as `p.validation-message`, the field marked (`aria-invalid`, `.input-validation-error`), described by it and focused, the first one when several fail: "Type a short code. The record was not saved." (I03); "Enter a valid email address, for example name@example.org." (I04, what was typed kept); "An institution with the same name or short code already exists." under Short code or Name (I08).
- **Saved** (I07): the result above the form takes the focus: "**Kgosi Kgari Teaching Hospital saved.** Its contact email is now hod.paediatrics@kgk.wombat.local." (`<wbr>` after the @), or "… It has no contact email now."
- **Load failed**: StatePanel's Alert, "Could not load the institution. Nothing has changed. Try again, or come back in a few minutes." with Try again, which focuses Name. An id outside her scope is Page not found.

## Narrow widths

Below 641px every control is 44px and the actions stack, Save changes on top of Cancel, both the form's width (`.record-page .form-actions`, `column-reverse`). Measured on the replay at 1280 and 390 (T376 step 8): no sideways scroll.

## Contrast

No new colour token.

## Known gaps

- **The race rule:** Deactivate is natively `disabled` while a save runs, and Save while a deactivation runs (`InstitutionEdit.razor`; DESIGN.md § Button system); the pressed one keeps the focus with `aria-disabled`.
- **The Administrator's New institution** (`/admin/institutions/new`, `InstitutionCreate.razor`) is not under `.record-page`: its actions do not stack primary-first at 390, its controls are not 44px, and its refusals are plain `ValidationMessage`s with no `aria-describedby` and no focus (T378).
