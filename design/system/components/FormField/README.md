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

- Every form is inside a `.form-container` (`surface-color`, `radius-xl`, `shadow-raised`, padding `space-xl`) and ends in a `.form-actions` row.
- A control is 38px: `.form-control`, `.form-select` and `.search-input` are a 24px line (the body's 1.5) between 0.375rem of padding and a 1px `input-border` edge, `radius-md`; `.form-select` draws its own chevron in `text-color`. Every control takes the body's font (T328).
- Fields sit in `.form-grid`: auto-fit columns of `minmax(min(250px, 100%), 1fr)` (`.form-grid--wide`: 350px), gap `space-lg`. Never a bare `minmax(250px, 1fr)`.
- Help goes in `HelpText`, never as a bare `<small>` in the slot, which no screen reader reads.
- A group of checkboxes or radios is a `fieldset.form-group` with a `<legend>` (reading as a label), its help a `p.page-subtitle` the fieldset names, its checkboxes in a `.check-grid`, each with a unique id. A rating scale is a `.scale-choices` list, lowest point first. Not a FormField.
- A field the caller may read but not change is text, not a control: a `dl.form-group` with the value and a `.muted .text-sm` line saying who sets it ("Set by a global administrator.").
- An option is shown by its label, never the key it stores.
- Sensitive inputs sit in a `.password-wrapper` with `PasswordToggleButton`.

## Invalid and warning states (DESIGN.md § Form system; § Alerts, validation, empty states)

- A form-level summary is `<ValidationSummary class="validation-summary-errors" />`, read as a danger alert: body text on `danger-bg`, the danger edge and 4px stripe, and the circle-alert icon; its `.validation-message` items take its colour and no icon of their own.
- A field's error is a `.validation-message` under it: `danger-color` words at 0.875rem with a 16px circle-alert icon in the same colour, hung in a 1.375rem left padding so a message that wraps keeps its edge.
- An invalid control is marked on the control too: Blazor's `.invalid` and `aria-invalid="true"`, or `.input-validation-error` plus `aria-invalid` on one the page marks by hand. It gets a `danger-color` border and, with it, a 5px stripe down its left (the 1px border and `inset 4px 0 0 var(--danger-color)`), so the state is not colour alone; a shadow, so the text does not move as the field turns invalid. In a Windows contrast theme the stripe is a 5px left border. A checkbox is not marked.
- A non-blocking warning is a `.field-warning`: `text-color` on `warning-bg` with a `warning-color` stripe, 0.85rem. "This encounter was 20 days ago. It can still be filed, but a filing more than 14 days after the encounter is recorded as late."
- A warning and a predicted refusal never show together; both live in one `role="status"` region the input names.

## Contrast

Every pair passes (T322, closed in flow 01): labels and help (`text-color`, `muted-text`); `.validation-message` 5.95:1 on white and 5.65:1 on the page; the summary's words 11.81:1 on `danger-bg`; `.field-warning` 11.89:1; a control's `input-border` edge 3.80:1 on white and 3.60:1 on the page (it was 1.49:1).
