# ActivityForm

The activity form (`Components/Shared/Activities/ActivityForm.razor`): an activity type's schema rendered for filing on Log an activity, on the activity's page, and in the builder's live preview. Each section of the schema is a card of its own, one deep, in the schema's order (E6: the College's seeds are never reordered), and what a section is depends on who reads it. Flow 03 (T342, 2026-09-29, 725237ee) redesigned it; until then every section was a fieldset in one form, and a section the reader could not write showed disabled inputs. `ActivityDetail` is the same form read-only.

## What the consumer provides

```razor
<ActivityForm SchemaJson="…" DataJson="@_dataJson" SubjectUserId="…" CreditRulesJson="…" WbaToolKey="…"
              EditableFieldKeys="_editable" FiledOn="@FilingLateness.Today()" ProgrammeStartsOn="@_programmeStartsOn"
              ReaderIsSubject="true" RefusedFieldKeys="_refusedFieldKeys" RefusalMessage="@_refusal"
              RefusalId="refusal-summary" LockedOwnerName="@_handOffName" HandOffFieldKey="…"
              HandOffNameChanged="…" DataJsonChanged="…" />
```

- The schema, the working data and, on an existing activity, the stored data (`StoredDataJson`: a person is read out only from a stored value, T102), its credit rules and instrument (which EPAs the picker offers, T108, T122), and the fields this reader may write (`EditableFieldKeys`, from the server; null restricts nothing).
- **The flow 03 parameters:** `LockedOwnerName` (who fills a locked section: the name the page's hand-off field holds), `LockedSectionsClosed` (a closed record: "was to fill this in"), `ReaderIsSubject` ("your programme" or "the trainee's programme"), `Rungs` (the ladder, for RungRow), `SectionAttributions` (a filled section's "Filled in by …"), `HandOffFieldKey` and `HandOffNameChanged` (the form reports the name its hand-off picker now holds, so the button, the check line and the owner line name that person at once, saved or not), `SectionHeadingLevel` (2; the builder's preview passes 4, under its own h3).
- **A refusal:** `RefusedFieldKeys` (the schema keys the server named), `RefusalMessage` (its words, cut one field a line by `FieldRefusals.Split`) and `RefusalId` (the summary each refused field names: `refusal-summary`; see RefusalSummary).
- `FiledOn` and `ProgrammeStartsOn`, where what is typed can still be the filing: the late warning and the programme hint (below).

## A section, by its reader

Each is `section.detail-card.form-section` named by its `h2` (`aria-labelledby`, id `sec-<key>`), never a fieldset and never a card in a card.

- **Open**: the reader may write at least one of its fields. A head row (`.form-section-head`: the h2, and "You fill this in" in muted 0.9rem when the form holds a section of another kind), then a `.form-grid` of `.form-group`s. A field the reader may not write, among ones they may, is text: a `dl.form-group` with its label over its value.
- **Filled**: they may write none of it, and it holds a value. Its head may say who filled it in, "Filled in by Anele Dlamini, 2026-09-30"; its fields are a `dl.details-list.details-list--stacked`, each label over its value in words (an option's label, a person's name, an EPA's "Code — Title", running text keeping its lines, `.form-readout-value`), a field with no value "Not filled in." muted, and a scale field its RungRow.
- **Locked** (`.form-section--locked`, C14): they may write none of it, and it holds nothing yet. Not an empty state: a dashed `input-border` frame, no shadow, on the page's ground; a head row on `header-bg` (`.form-section-lockhead`, 12px by 24px, a dashed rule under it) holding the h2 and the owner line with a 16px `lock`, in body text at weight 600; a body (`.form-section-lockbody`, 16px by 24px) holding the pending RungRow of any scale field in it, then "Not filled in yet." (`.form-section-empty`, muted). No inputs.
  - **The owner line** comes from the section's own `editable_by` (`subject|creator` when unset): a `field:` owner is the person that field names, "Fatima Khumalo fills this in", or while nobody is named the field by its label, "The assessor you name fills this in"; a `role:` owner is the role, "A coordinator fills this in". The author's own section has no owner line, and reads "Not filled in.".
  - **Closed** (declined, cancelled): "Fatima Khumalo was to fill this in.", "The assessor was to fill this in.", "A coordinator was to fill this in.", over "Not filled in.".

## A field

- **The input's id is its key with `-in`** (`ActivityFieldIds.Input`: `observed_on-in`), so a refusal summary's link lands on the control. A multi-choice group's fieldset, a read-out's row, a locked field's `dl` and a locked section's refusal line carry it too, with `tabindex="-1"`, so the link can give each the focus.
- **The required mark:** the label ends in `<span class="required-mark" aria-hidden="true"> *</span>` (`danger-color`), and the control carries `aria-required="true"`. One line above the first section says what it means: "* marks a field you must fill in." A required group keeps its legend's visually hidden "required".
- **Help under the control:** `p.field-help` (muted, 0.9rem), id `<key>-help`, named first in the control's `aria-describedby`; then its message (`<key>-msg`), any notice, then the summary.
- **A choice, an EPA, a person** is a native `select` ("Select…" first), each option's full text; under an EPA or person select its chosen option is printed in full (`p.field-readback`, `aria-hidden`: the select already says it), so nothing is cut off at 390px (T323).
- **A scale** the reader may write keeps its `select` of the ladder's labels (radios are flow 04's); one they may not write is its RungRow.
- **A refused field** takes the invalid marks (the `danger-color` border and stripe, `.input-validation-error`, `aria-invalid="true"`) and shows its own part of the refusal under it (`.validation-message`, id `<key>-msg`): "The date cannot be after today (2026-09-30)." A read-out the refusal names says it under its value, and a locked section's field on a line of its own in the body. **A changed value drops its marks and its message at once**, and the form's hints come back; the summary stays until the page's next action.

## The encounter date's hints

Only on a type whose credit rules can credit (`EncounterDatePolicy.CanCredit`), and only once a date is entered (the date input's change, not each keystroke) or at once on a form that opens with one (File it again, a draft). Both sit in the date's always-present `role="status"` region (`<key>-filing-notice`).

- **Late:** `.field-warning`: "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after the encounter is recorded as late." Never a refusal (D15).
- **Before the programme:** a predicted refusal (C9), so the field's own `.validation-message` and the invalid marks: "This date is before your programme started (2026-01-15), and will not be accepted." To anyone but the registrar, "the trainee's programme" (C12; `ProgrammeStartWording`). Not shown while the server's refusal of the date is on show, which would say it twice.

## Below 641px

A section pads 16px (a locked one's head and body 16px at the sides), every `.form-control` and `.form-select` is 44px, a multi-choice row is 44px with its label filling the row, as flow 02's Remember me does, and the rungs close to 4px apart.

## Rules (DESIGN.md § Form system, "The activity form")

- A field the reader may not write has no control: never a disabled input.
- Sections keep the schema's order and the College's labels.
- An option, a person or an EPA is shown by its words, never by what it stores.

## Contrast

Help, the owner line and "Not filled in yet." `muted-text`: 5.09:1 on the surface, 4.83:1 on the page; the lock head's words `text-color` on `header-bg`, 11.36:1; the locked frame `input-border`, 3.60:1 on the page; the required mark and a refusal `danger-color`, 5.95:1 on the surface.

## Known gaps

- A locked section's body is "Not filled in yet." / "Not filled in."; the boards had a sentence per state ("After you submit, David Naidoo rates the encounter on this ladder.", "Not filled in: the request was declined.") (T347).
- A file field is not wired: it shows "File uploads are represented in the schema, but storage wiring lands in a later pass." in its place (a seed holds a document as a link, D34).
