# FormField

One labelled field of a form: a `<label for>`, the caller's control, its help text and its validation message, laid out as a `.form-group` in the form system's `.form-container` / `.form-grid` / `.form-actions` frame.

## What the consumer provides

```razor
<FormField Label="Title" InputId="epa-title" Required="true" FullWidth="true" HelpText="…">
  <InputText id="epa-title" class="form-control" @bind-Value="_model.Title"
             aria-describedby="@FieldHelp.DescribedBy("epa-title", helpText)" />
  <ValidationMessage For="() => _model.Title" />
</FormField>
```

- `Label` and `InputId` (required): the label's text and the control's id.
- The control, in the slot: `.form-control` for inputs and textareas, `.form-select` for selects, a `.form-check` with a `.form-check-input` for one checkbox.
- `Required`: a visual `*` (hidden from screen readers) plus a visually hidden "required". **Not on flow 02's account forms** (sign in, link, change password, My account's name, the Remove dialog; T339, R3-Spec § Accessibility): every field on them is required, so a mark on each tells nothing; the input's own `required` exposes it to assistive technology, and the browser's check names a blank field. Elsewhere the mark stands.
- `HelpText`: rendered as `small.page-subtitle` with id `{InputId}-help`. The control is the caller's, so the caller names the help with `aria-describedby="@FieldHelp.DescribedBy(…)"`, help first, then any warning or refusal region.
- `FullWidth`: spans the grid row. `ValidationContent`: a region under the field (the encounter date's warning).

`FormActions` wraps the submit row: `<FormActions>` renders `div.form-actions` (right-aligned, a top rule, gap 0.75rem): Cancel (`.btn-outline`) then the submit (`.btn-primary`).

## The form system (DESIGN.md § Form system)

- Every form is inside a `.form-container` (`surface-color`, `radius-xl`, `shadow-raised`, padding `space-xl`) and ends in a `.form-actions` row.
- A control is 38px: `.form-control`, `.form-select` and `.search-input` are a 24px line (the body's 1.5) between 0.375rem of padding and a 1px `input-border` edge, `radius-md`; `.form-select` draws its own chevron in `text-color`. Every control takes the body's font (T328).
- Fields sit in `.form-grid`: auto-fit columns of `minmax(min(250px, 100%), 1fr)` (`.form-grid--wide`: 350px), gap `space-lg`. Never a bare `minmax(250px, 1fr)`.
- Help goes in `HelpText`, never as a bare `<small>` in the slot, which no screen reader reads.
- A group of checkboxes or radios is a `fieldset.form-group` with a `<legend>` (reading as a label), its help a `p.page-subtitle` the fieldset names, its checkboxes in a `.check-grid`, each with a unique id. A rating scale is a `.scale-choices` list, lowest point first. Not a FormField.
- A field the caller may read but not change is text, not a control: a `dl.form-group` with the value and a `.muted .text-sm` line saying who sets it ("Set by a global administrator.").
- An option is shown by its label, never the key it stores.
- **An activity form is not built from FormField** (flow 03, T342): ActivityForm renders the schema itself, a card a section, with its own help under the control (`p.field-help`, muted 0.9rem, not `small.page-subtitle`), its required mark as `span.required-mark` (`danger-color`) with `aria-required` on the control, and the chosen option of an EPA or person select printed in full under it (`.field-readback`). A field the reader may not write has no control at all.
- **A choice of one among a few kinds, each needing a sentence, is radios, not a select** (flow 08, T364; round 3 S1, S6): the schedule page's Review type is a `fieldset.form-group` holding a `.scale-choices` list of `.form-check.review-type-choice` rows, each a radio named by its label alone ("Pre-graduation review"), its help under the label as a `small.page-subtitle.review-type-help` (`{id}-help`, set in from the radio's edge) that the radio names with `aria-describedby` (build review D7). The default is checked as the page opens, so the form never refuses a missing choice (F4). A bare radio group (New panel's Scope, a seat) is a `fieldset.form-group.choice-row`: no border, margin or padding, its radios in a `.check-grid` (ScheduleReview, SeatPicker).
- **A value fixed at creation is shown in a form's place** (flow 08, Q8, `.readonly-value`, weight 600): a panel's Decides for for a speciality admin, New panel's one offered Scope ("Speciality", with why), the chair's row in the seat picker ("Chair · chosen above").
- **Who sits on a panel is chosen by person** (flow 08, Q5): the seat picker, one radio group a person, never a `<select multiple>` (SeatPicker). A group refused as a whole is `.choice-group.is-invalid`: the field's 4px danger stripe with no shift of its padding, a 4px `CanvasText` border in a contrast theme, its message at its top, which the group names.
- **Dates are typed in the browser's date input** (flow 08, F3; `InputDate`), and every date shown is ISO: no text field asks for an ISO date, and none refuses its format. Two that belong together (an evidence window's From and To) sit side by side while they fit, `.field-pair` (auto-fit 12rem tracks).
- **On flow 08's pages** (`.committee-calendar`), below 641px every `.form-check`, `.form-control` and `.form-select` is 44px, and the form's actions are 44px and the row's width.
- **A counter is a second `.field-help`** (flow 09, T366, S9; DESIGN.md § Form system): "153 of 4,000 characters" (`p.field-help`), named after the help in the control's `aria-describedby` (`appeal-reason-help appeal-reason-count`, then the refusal's id when refused; the register's box, which has no help, its refusal then its counter). It counts in the browser as the person types (`js/review-appeal.js`'s `count`, attached once a box), so no keystroke is a round trip, and the page renders the same words from what it holds. **The control carries no `maxlength`**, which would cut a paste silently: the counter shows the excess and the action refuses it under the field in Wombat's words, "Give your reason in 4,000 characters or fewer." (the lodge block, MyReview), "Give the reason in 1,000 characters or fewer." (the register's revocation reason, D2, R3; StarRegister). Flow 10 (T369) counts the MSF report's Narrative the same way, "0 of 4,000 characters", refused over it with "Write the narrative in 4,000 characters or fewer." (MsfReport); the respondent's comment, on a static page with no counter, drops its `maxlength` too, the handler refusing an over-long one in words (build review D4; MsfRespond).
- **The required mark on the MSF forms** (flow 10, T369, F6): New MSF campaign and the campaign page's add form open with "* marks a field you must fill in." (`p.field-help`), and every control a person must fill carries the mark (`span.required-mark`, `aria-hidden`) and `aria-required="true"`; a control that always holds a value (Respondent group, a select with no placeholder) carries none. A fieldset's legend adds a hidden " required" (the Evidence group). The respondent's questionnaire says "Questions marked * must be answered." and marks an optional comment "(optional)" (`span.respond-optional`) (MsfRespond). Their forms are `novalidate`: the browser stays quiet and the page refuses in its own words (T324), a refused create summarised at the head of the form (RefusalSummary; NewMsfCampaign).
- **A select at its longest option's width** (flow 10, `.msf-narrow-field`): the MSF report's Supervision level, not the column's width (the column's below 641px).
- **A rating of five points is a column of 44px bordered rows** (flow 10, `.respond-form .scale-choices`): each `.form-check` 2.75rem tall with a border, the chosen one tinted `info-bg` with a `secondary-color` edge by `:has(:checked)` alone (a static page with no script), a 2px `CanvasText` frame in forced colours (MsfRespond).
- **A choice of sentences is radios, not a select** (flow 09, T366, T323): an option that is a sentence, "Remitted: the appeal body replaces the decision", is cut by a closed select at 390 (measured: 322px of text in 262px), so the appeal's Outcome is two radios in flow 08's `.choice-group` (`.is-invalid` when refused, the first radio taking the focus), one a line (`.check-grid.resolve-outcomes`), **neither chosen at first** (T307), under a `fieldset`'s legend "Outcome" with its help as `small.page-subtitle` (ReviewAppeals). Flow 08's Review type keeps its default checked; an appeal's outcome has no default.
- **A required textarea outside FormField** (flow 09, the lodge block): the label ends in `<span aria-hidden="true">*</span><span class="visually-hidden">required</span>`, as FormField writes it, and the control carries `aria-required="true"`; a refused one adds `.input-validation-error`, `aria-invalid` and `p.validation-message` under the help.
- A password is a `PasswordField` (its own component: the label, the input and a Show toggle beside it), not a FormField; a new password lists the six rules under it (PasswordRules). The old `.password-wrapper` and `PasswordToggleButton` are gone (T339).

## Invalid and warning states (DESIGN.md § Form system; § Alerts, validation, empty states)

- A form-level summary is `<ValidationSummary class="validation-summary-errors" />`, read as a danger alert: body text on `danger-bg`, the danger edge and 4px stripe, and the circle-alert icon; its `.validation-message` items take its colour and no icon of their own.
- A field's error is a `.validation-message` under it: `danger-color` words at 0.875rem with a 16px circle-alert icon in the same colour, hung in a 1.375rem left padding so a message that wraps keeps its edge.
- An invalid control is marked on the control too: Blazor's `.invalid` and `aria-invalid="true"`, or `.input-validation-error` plus `aria-invalid` on one the page marks by hand. It gets a `danger-color` border and, with it, a 5px stripe down its left (the 1px border and `inset 4px 0 0 var(--danger-color)`), so the state is not colour alone; a shadow, so the text does not move as the field turns invalid. In a Windows contrast theme the stripe is a 5px left border. A checkbox is not marked.
- A non-blocking warning is a `.field-warning`: `text-color` on `warning-bg` with a `warning-color` stripe, 0.85rem. "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after the encounter is recorded as late."
- A warning and a predicted refusal never show together; both live in one `role="status"` region the input names.
- **A field the server refused shows its own part of the refusal under it** (flow 03, T263, C8): the page hands the form the refusal's words, cut one field a line, and each field it names says its part as a `.validation-message` (`<key>-msg`), takes the invalid marks, and names its help, its message and then the RefusalSummary with `aria-describedby`. A changed value drops its marks and its message at once.

## Contrast

Every pair passes (T322, closed in flow 01): labels and help (`text-color`, `muted-text`); `.validation-message` 5.95:1 on white and 5.65:1 on the page; the summary's words 11.81:1 on `danger-bg`; `.field-warning` 11.89:1; a control's `input-border` edge 3.80:1 on white and 3.60:1 on the page (it was 1.49:1).
