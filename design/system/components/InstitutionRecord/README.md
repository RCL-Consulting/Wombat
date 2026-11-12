# InstitutionRecord

An Institutional admin's own institution (`/admin/institutions/{Id}`, `Pages/Admin/Institutions/InstitutionEdit.razor`, their branch; the command's refusals `InstitutionUpdateRefusedException`): its name, short code and contact email, which they keep, and its status, which they read. Flow 12 (T376, 2026-10-23, e53d4ae7; boards R3-I01 to I08; S9, S12, S22, M8; T291 item 7, T302, T324) rebuilt this view. The Administrator's view of the same route, and `/admin/institutions/new` (a page of its own, the Administrator's alone, `InstitutionCreate.razor`, S12, so an Institutional admin's is Access denied), were rebuilt by flow 18 (T400): AdminInstitutionRecord. Flow 18 changed this view in one place: an id outside their scope is Institution unavailable (Q17), no longer Page not found.

## The page

- **The header**: h1 and tab the institution's name ("Institution" before the read and on a failed one, S9); "Your institution's name, short code and contact email."; no header action. **The trail** is Home › ‹name›: the institution has no owner in their menu, so it is reached from Home's Your institution card (InstitutionalAdminHome).
- **The column**: `.record-page`, a flex column 24px apart, at most 52rem: the result region (hidden while empty), then the form.
- **The form**: `form.form-container`, `novalidate`, named by the institution's name; "* marks a field you must fill in."; on `.form-grid`, **Name \***, **Short code \*** (each `aria-required="true"`, never the native `required`) and **Contact email** (`type="email"`), with no help under them (the help lines are the Administrator's view's); then **Status read out** (M8): `dl.form-group`, `dd.readonly-value` "Active" or "Inactive" over `dd.field-help` "Set by a Wombat administrator."; then `FormActions`: **Cancel** (outline, to Home) and **Save changes** (primary; `aria-disabled` while it runs).

## Refusals and the result

- **No `ValidationSummary`** (S22): the page's own checks and the command's typed refusals (by `Field`, never by its text) sit under their field as `p.validation-message`, the field marked (`aria-invalid`, `.input-validation-error`), described by it and focused, the first one when several fail: "Type a short code. The record was not saved." (I03); "Enter a valid email address, for example name@example.org." (I04, what was typed kept); "An institution with the same name or short code already exists." under Short code or Name (I08).
- **Saved** (I07): the result above the form takes the focus: "**Kgosi Kgari Teaching Hospital saved.** Its contact email is now hod.paediatrics@kgk.wombat.local." (`<wbr>` after the @), or "… It has no contact email now."
- **Load failed**: StatePanel's Alert, "Could not load the institution. Nothing has changed. Try again, or come back in a few minutes." with Try again, which focuses the h1 once it reads (Name until flow 18, S3).
- **Institution unavailable** (flow 18, Q17, T10): an id outside their scope, or one that does not exist, reads "Institution unavailable", "This institution does not exist, or you cannot open it." and **Go to Home**; the trail is Home › Institution unavailable, with no Institutions crumb (DESIGN.md § null-read words, 1.23). Never which cause.

## Narrow widths

Below 641px every control is 44px and the actions stack, Save changes on top of Cancel, both the form's width (`.record-page .form-actions`, `column-reverse`). Measured on the replay at 1280 and 390 (T376 step 8): no sideways scroll.

## Contrast

No new colour token.

## Known gaps

- None found re-syncing. Until flow 18 this record listed two: the race rule between Save and Deactivate (now the Administrator's view's, AdminInstitutionRecord) and the create page outside `.record-page` (T378: flow 18 put it there, its refusals under their fields and focused).
