# ActivityTypeOutline

The builder's Form tab (`ActivityTypeEdit.razor`'s `FormPanel`, `FormSettings`, `SectionEditor` and `FieldEditor`; the working model `BuilderModels.cs`; words `ActivityTypeEditWords.cs`; the preview is ActivityForm): **the form's settings, its sections and fields as an outline whose editor opens in place under its row, and the live preview beside it.** Flow 17 (T398, 2026-11-10, 84bcffeb; boards R3-C09 to C23, R2-B02; Q4, Q5, Q8, Q18, S2, S8, S13, S17, S27, S28, T19, T23, T133, T266, T324) rebuilt it. Until then the editor rail listed every field with Edit, Up, Down and Delete, and the one editor opened below the whole list; a type was an enum name ("LongText"), a setting a bare key, Delete removed at once without naming its row, and a narrow window showed the editor and the preview one above the other.

## The card

The tab's panel is the card, `div.form-container[role="tabpanel"]` (no `tabindex`: it holds controls), and the builder's size container (`builder`, T266). In it:

- **Show** (C2, Q5): a `fieldset.form-group.choice-group` with legend "Show" and two radios, "Edit the form" ("The form" to a reader) and "Preview". Where the card is under 46.5rem (up to a 1115px viewport) the two columns stack and the choice hides the unchosen one, by CSS alone (no round trip; switching tab and back sets it to the form again). From 1116px both columns stand side by side and the choice is hidden.
- **`.builder-two-col`**: the left column 320px or more, the preview 400px or more (1fr and 1.4fr).

## The left column

- **h2 Form settings** and "Which field carries the encounter date, which carries the entrustment rating, and which names the EPA." Three selects (a reader's a `dl`): **Encounter date field** ("None — this form has no single encounter date"), **Entrustment rating field** ("None — this form asserts no entrustment level"), **EPA field** ("None — this form is about no single EPA", with its help), each offering only the fields of its type, **by label with the key after**: "Date delivered (delivered_on)", "Supervision required for this encounter (overall_level)" (T19, T324). A setting at a field deleted since keeps its key until the save drops it (T133, Q18).
- **h2 Sections and fields**, a writer's **Add section** (outline, small), then `div.form-outline`, a column of sections 16px apart.
- **A section** is `section.outline-section[aria-labelledby]`: a 1px `border-color` box, `radius-lg`, on the surface. Its head, `div.outline-section-head` on `header-bg`, holds the title as an `h3` alone, its key as `p.field-row-meta > code`, then its buttons in `div.actions-cell`: a writer's **Edit**, **Up**, **Down**, **Delete**; a reader's **View**. Above 20 fields in the form each head also offers **Hide fields** / **Show fields** (`aria-controls` its list, no `aria-expanded`: the words say which way; every section starts open; T23, S17). Under the head, `ul.list-unstyled` of fields ("No fields in this section yet." in an empty one), then a writer's **Add field**, alone at the section's foot (not drawn while the section's fields are hidden).
- **A field** is `li.field-row`: its label (`p.field-row-label`, 600) over a meta line (`p.field-row-meta`, muted, 0.875rem): the type by label ("Text", "Long text", "EPA", "User · Names an Assessor", "Procedure reference"), then the key in `<code>`, then "Required" where it is: "Long text · strengths · Required". Then the row's buttons, as the section's.
- **Every button is named for its row** (T239): "Edit field Topic", "Move field Topic up", "Delete section Request", "Add field to section Teaching session", "View field EPA", "Hide fields of section Feedback".
- **Up on the first row and Down on the last are natively `disabled`**: a position, not an action withheld for a reason. A move keeps the focus on the moved button (or its twin at an end).
- **Delete asks nothing** (Q8): an outline button, since nothing is saved until Save draft. The focus goes to the next row's Edit, else Add field (for a section, the next section's Edit, else Add section).

## The editor in place (Q4, S13)

One at a time, directly under its row: **Edit** becomes **Done** (`aria-expanded`, `aria-controls` the editor; a reader's View becomes Close), Edit focuses the editor's first input, Done gives the focus back to Edit.

- **A field's** editor is an `li.field-editor-inplace` after its row (no `role`), its `h4` "Field Topic", "* marks a field you must fill in.", then a `form-grid`: **Field key*** (`font-mono`), **Label***, **Type*** (Text, Long text, Number, Date, Choice, Multi-choice, Scale, User, EPA, Likert, Procedure reference, File, Signature), **Help text**; a User field's **Names a** ("Assessor (default)", Institutional admin, Coordinator, Committee member, Trainee, by label; "Who this field may name: someone at the trainee's institution holding this role.") in place of **Options** (one a line; starred for a Choice or a Multi-choice; "value | Label" shows a label and stores the value) and **Catalogue key**; a Scale's **Entrustment scale** ("Select…", each scale by name: a `seed:` key reads by its scale's name, c9742a18); **Required**. A reader's is a `dl` of the same settings as text.
- **A section's** editor is a `div.field-editor-inplace[role="group"]` under its head, its `h4` "Section Teaching session", then **Section key*** and **Title***.
- **Add field** adds a row "New field" and opens its editor, Field key ("field_1") focused, so no Edit is pressed for a new field. **Add section** adds "Section 2" (by its place) holding one "New field", and opens the section's editor, Section key focused.
- **The stripes** (S2, S27): the open row and its editor share a 4px inset stripe in `secondary-color` (`.is-editing`); a refused row a 4px `danger-color` stripe (`.is-refused`) with its words under it (`p.validation-message`), so not colour alone. In a contrast theme the stripes are a 4px left border, `Highlight` for open and `CanvasText` for refused (S28).
- **A refusal at its place**: a field's words under its row while closed, under its Field key while open (the key `aria-invalid`, its words first in `aria-describedby`); a section's in its head (`#sec-N-refusal`), and its editor's Section key, where the summary's line lands, is `aria-invalid` and described by them.

## The right column

**h2 Live preview** over the shared ActivityForm (`SectionHeadingLevel="3"`, so its sections sit under the h2), fed the type's own EPAs (`EpaOptions`, S8: an institution's type previews the EPAs of the curricula it adopted, the College's its own, never another institution's) and changed as it is typed. Under a User field: "In the preview this offers nobody: it offers an active Assessor at the registrar's institution, never the registrar." A schema that does not parse shows ActivityForm's sentence in place of the form.

## Narrow widths

The columns go by the card, not the viewport (T266): stacked with the Show choice up to 1115px, side by side from 1116px. In a stacked column a field row's buttons stay on one line under a one-line label (the replay's sweep, 900 to 1150px); below 641px each row's buttons share the row (`flex: 1 1 auto`), 44px.

## Contrast

No new colour token. The head's text on `header-bg`: `text-color` 11.36:1, the key `muted-text` 4.57:1; a field's meta `muted-text` 5.09:1 on the surface; the stripes `secondary-color` 4.86:1 and `danger-color` 5.95:1 against the surface (non-text, 3:1 needed).

## Known gaps

- **A section's refusal marks only its Section key** (`#section-key`), not its Title, though either may be the cause the words name (the fix pass's S2; T398's states pass 2, unfiled).
- The editors use fixed ids (`field-key`, `section-key`, …), so only one may be open at a time; the page holds to that.
