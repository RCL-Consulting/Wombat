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
- `Required`: a visual `*` (hidden from screen readers) plus a visually hidden "required".
- `HelpText`: rendered as `small.page-subtitle` with id `{InputId}-help`. The control is the caller's, so the caller names the help with `aria-describedby="@FieldHelp.DescribedBy(…)"`, help first, then any warning or refusal region.
- `FullWidth`: spans the grid row. `ValidationContent`: a region under the field (the encounter date's warning).

`FormActions` wraps the submit row: `<FormActions>` renders `div.form-actions` (right-aligned, a top rule, gap 0.75rem): Cancel (`.btn-outline`) then the submit (`.btn-primary`).

## The form system (DESIGN.md § Form system)

- Every form is inside a `.form-container` (`surface-color`, `radius-12`, `detail-card-shadow`, padding `space-xl`) and ends in a `.form-actions` row.
- Fields sit in `.form-grid`: auto-fit columns of `minmax(min(250px, 100%), 1fr)` (`.form-grid--wide`: 350px), gap `space-lg`. Never a bare `minmax(250px, 1fr)`.
- Help goes in `HelpText`, never as a bare `<small>` in the slot, which no screen reader reads.
- A group of checkboxes or radios is a `fieldset.form-group` with a `<legend>` (reading as a label), its help a `p.page-subtitle` the fieldset names, its checkboxes in a `.check-grid`, each with a unique id. A rating scale is a `.scale-choices` list, lowest point first. Not a FormField.
- A field the caller may read but not change is text, not a control: a `dl.form-group` with the value and a `.muted .text-sm` line saying who sets it ("Set by a global administrator.").
- An option is shown by its label, never the key it stores.
- Sensitive inputs sit in a `.password-wrapper` with `PasswordToggleButton`.

## Invalid and warning states (DESIGN.md § Form system; § Alerts, validation, empty states)

- A form-level summary is `<ValidationSummary class="validation-summary-errors" />`: a red panel of `.validation-message` items at the top.
- A field's error is a `.validation-message` under it (`danger-color`, 0.85rem).
- An invalid control is marked on the control too: Blazor's `.invalid` and `aria-invalid="true"`, or `.input-validation-error` plus `aria-invalid` on one the page marks by hand. It gets a `danger-color` border and a 4px left stripe (`invalid-stripe`), so the state is not colour alone; a checkbox is not marked.
- A non-blocking warning is a `.field-warning`: `text-color` on `warning-bg` with a `warning-color` stripe, 0.85rem. "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after the encounter is recorded as late."
- A warning and a predicted refusal never show together; both live in one `role="status"` region the input names.

## Contrast

Labels and help pass (`text-color`, `muted-text`). `.field-warning` passes (11.89:1). **Fail, kept as the source has it (T322):** `.validation-message` 3.82:1 on white, `.validation-summary-errors` 3.57:1 on `danger-bg`, and every control's `input-border` at 1.49:1 (3:1 needed).
