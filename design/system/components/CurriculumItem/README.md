# CurriculumItem

A curriculum item's page of its own (`/admin/curricula/{Id}/items/new` and `/admin/curricula/{Id}/items/{ItemId}`, `Pages/Admin/Curricula/CurriculumItemPage.razor`; its model `CurriculumItemFormModel.cs`, its words `CurriculumItemsWords.cs`; the rung pickers `RungPicker.razor` and `StageMinimaEditor.razor`; the field refusal `Components/Shared/FieldRefusal.cs` and Application's `FieldRefusalException`, `CurriculumItemFieldMessages`; the commands `AddCurriculumItemCommand`, `UpdateCurriculumItemCommand`, the picker `ListCurriculumItemEpaOptionsQuery`): **add an item, edit one, or read a national one the caller may not change.** Flow 15 (T392, 2026-11-02, c931d56f; boards R3-B22, B24, R2-B04, B05, B09, B12, B15; Q14, Q19, F3, F4, F6, P2, P3, C2, M1, T3, T7, T125, N9, decision 5) made it: **no list edits a row in place** (DESIGN.md § Table system). Until then the form was T176's edit row inside the items table, which overflowed at 390 (T323), and an institution had no page to read a national item's window, weight and year minima.

## Who reads it

`[Authorize(Policy = "NationalCatalogueAccess")]`, from the items list (CurriculumItems): its add link, a row's Edit, or a row's View. One editor for every caller, the College's and the Administrator's too (T4); its words follow `CanEditCurriculum`. **What it draws is the query's**: an item the caller `CanEdit` is the form; one the caller reads and may not change (a national item read by an Institutional admin) is its **record** (P2); the add on a version the caller may not add to is not a form. The trail is Home › Curricula › Items of Paediatric EPA Curriculum (11.2) › the page, Curricula lit; an item's crumb is its EPA's code ("PAED-011"), the add's its h1.

## The page

The column is `<div class="user-page curriculum-item-page">` (flow 12's 60rem column of sections, and its phone rules), `aria-busy` while it reads.

- **The header** (F3): the add is "Add item" (the College's and the Administrator's, subtitled "On Paediatric EPA Curriculum (11.2). It applies to every institution that has adopted it.") or "Add your institution's own item" (subtitled "On Paediatric EPA Curriculum (11.1). It applies only to Kgosi Kgari Teaching Hospital's trainees."). An edit is "Edit PAED-011 — Managing population health challenges", subtitled as the national add, or for an own item "Your institution's own item on Paediatric EPA Curriculum (11.2)". The tab is the h1's words.
- **The form** (`EditForm.form-container`, `.form-grid.form-grid--wide`), field for field the items page's old one with its rules, each `FormField` with its help:
  - **EPA** *(required, full width)*: a select offering exactly what the command accepts (T195, T222): the national EPAs of the sub-speciality no item holds for the same trainees, and for an own item the institution's own EPAs; on an edit, the item's own EPA and what it could move to (M6). A paused EPA's option says " (inactive)". A stored EPA it cannot name is said under it before Save: "PAED-011 is not one of the EPAs this item can name, so none is selected. Choose one before saving."
  - **Target** (a number), **Per** ("Semester (January to June, July to November)", "Academic year (January to November)"), **Decision cadence** ("No published cadence", "Each semester", "Each academic year"), **Decided by** ("The trainee's general panel", or a College body such as the Neonatal team Clinical Competency Committee), **Opportunistic** (a checkbox, "Decided as rotation or opportunity allows", disabled while there is no cadence).
  - **Entrustment scale** ("Not pinned", or a ladder), with the reason it was suggested while it still is: "Suggested because every item on this curriculum is pinned to it." (or "because it is the default scale of Paediatrics"); changing it clears the minima picked on the old ladder (T125).
  - **Minimum level** *(required)*: `RungPicker`, a select of the ladder's rungs ("Choose a rung", "1", "2", "3a", "3b", "4", "5"), or a number 1 to 20 when unpinned.
  - **Minimum by training year** (decision 5): `StageMinimaEditor`'s fieldset, a row a year ("Year 1" and its rung picker, Remove), "Add year 1", or "No year has its own minimum."
  - **Completion window (months)**, **Weight**, and **Tools** (a fieldset, its help, then a `.check-grid` of checkboxes, one an instrument; none ticked means any).
  - **The actions** (`FormActions`): Cancel first in the DOM (to the list with no token from the add, P6; with `?back=` from an item, R3-B11), the status `span#item-minima-status.muted[role="status"]`, then the primary "Add item" or "Save item". **The submit is never natively disabled**: until every minimum is a rung it is `aria-disabled` and described by the status, which says why: "Adding is off until every minimum is a rung on CPSA Paediatric Entrustment Scale v11.1." ("Saving is off …"; after a change of ladder, "The scale is now ‹scale›, so the minima picked on the previous ladder have been cleared: …"). A press then sends nothing; so does a second press while one is sent (T234).
- **Sent**: an add returns to the list by `?added=‹id›` and an edit by `?saved=‹id›`, where the list writes "Curriculum item added." or "Curriculum item saved." and focuses it (`CatalogueLinks`, F1, M6).
- **Refusals under their field** (F4, M1; `FieldRefusal`): Target and Completion window are checked before anything is sent, in the commands' own words ("Enter a target of 1 or more.", "Enter a completion window of 1 month or more."); the command's own refusal of the EPA, Minimum level, Decided by or Opportunistic lands under that control too. Each is `div.validation-message[role="alert"]` (`id="‹input›-refusal"`) under the control and above its help, the control `aria-invalid` (Blazor's `.invalid` through the form's `EditContext`) and described by the refusal first, then its help (flow 14's N17); the control takes the focus; what was typed is kept. **A refusal of the EPA reads the picker again** (T222 review): the likeliest cause is a page out of date (an EPA added in another tab), so a choice no longer offered moves to the first one that is (the add) or back to the item's stored EPA (an edit); an add left with nothing to add gives way to the empty state, the refusal leading it, focused: "This curriculum already contains the selected EPA." over "Every EPA your institution could add is already on this curriculum: …" with Go to Items of Paediatric EPA Curriculum (11.2).
- **Failures**: a version superseded since the page was read says the command's sentence at the form's top, focused (C2): "Your institution has superseded this version, so no item is added to it. Nothing has changed." Anything else is fixed words there, logged (T329): "The item could not be added. Nothing has changed. Try again, or come back in a few minutes." ("… could not be saved. …").

### A national item's record (P2)

An item the caller reads and may not change, reached by the list's View: titled by its EPA ("PAED-001 — Providing paediatric emergency care to children"), subtitled "A national item on Paediatric EPA Curriculum (11.1). The College sets it, so it is read only here." One `section.detail-card.user-section` holding a `dl.detail-list`: **EPA** (with " (inactive: not in force)" muted when paused), **Target and decision** ("3 per semester" over "Decided each semester"), **Minimum level** (the rung over its ladder, or "Not pinned"), **Minimum by training year** (an unmarked `ul.list-unstyled`, "Year 1: 3a" a line, or "No year has its own minimum."), **Completion window** ("12 months"), **Weight** ("—" for none) and **Tools**. No form and no Save; its one action, in `.form-actions`, is the outline "Back to Items of Paediatric EPA Curriculum (11.1)", carrying `?back=`, so the list focuses that row's View.

### Unavailable (Q19, R2-B15, P3)

Each in the route's own place, a `.detail-card--empty` with one primary link, the trail straight under Curricula:

- **Curriculum unavailable**: "This curriculum does not exist, or you cannot open it." with Go to Curricula.
- **Item unavailable** (an item the curriculum does not hold for the caller, or an add the command would refuse the caller): "This curriculum item does not exist, or you cannot change it." with "Go to Items of Paediatric EPA Curriculum (11.2)".
- **Cannot add an item** (`/items/new` on a version the caller's institution has superseded, F6, P3), its trail through the items list: "Your institution has superseded this version, so no item is added to it. Its own items stay for the registrars still on it." with Go to Items of ….

## States

- **Loading**: h1 "Curriculum item", `p.loading-line[role="status"]` "Loading this curriculum item." over a skeleton card.
- **Load failed**: "Could not load this curriculum item. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), which focuses the h1 once it reads; the cause goes to the log.

## Narrow widths

The form is one field a row below 641px (`.form-grid`). `.user-page` makes every button, input and select 44px and stacks the form's actions with the primary on top (Cancel first in the DOM, so it stays first in the tab order); app.css's "Flow 15: a curriculum's items" makes the item page's checkbox rows (Opportunistic and each tool) 44px rows with a 20px box (N9). The record's Back is the row's width. Measured on the replay at 390 (A.7.10: each control 44px, each tool's label 44px, Save item on top of Cancel; 390 in 390) and 1280.

## Contrast

No new colour token. The help, the scale's reason, the status and the record's second lines in `muted-text` (5.09:1 on the surface); a refusal's words and icon as FormField's `danger-color`.

## Known gaps

- None found re-syncing. The page has no result of its own by design: a save leaves it for the list, which says what was done (F1); a refused save stays here with its refusal.
