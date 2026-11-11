# ActivityTypes

Activity types (`/admin/activity-types`, `Pages/Admin/ActivityTypes/ActivityTypesList.razor`; its words `ActivityTypesListWords.cs`, and `ActivityTypeWords.cs` for what it shares with the builder; the read `ListActivityTypesAdminQuery`; the addresses `Components/Shared/CatalogueLinks.cs`): **every activity type the caller may read, each row saying whose it is, what is published, whether a draft waits, and whether the caller may change it.** Flow 17 (T398, 2026-11-10, 84bcffeb, with 277e4a78: the one-word columns fitted and the 1150px stack; boards R2-A01 to A12, R3-A09-nocreate, R3-A12, R3-A13, R3-C08-list; Q7, Q9, Q11, Q12, T239, T300, T326, T329) rebuilt it on flow 12's list column. Until then it showed Scope as an enum name and a version as "v0", searched on every keystroke, said "No activity types" for a search that matched nothing, and printed its read's exception text.

## Who reads it

The Administrator, the College admin and the Institutional admin (`[Authorize(Policy = "NationalCatalogueAccess")]`; NavMenu's Activity types in the Administrator's Catalogue group, the Institutional admin's Curriculum group and the College admin's one list). It is a list, so it lights itself and has no trail; the builder lights it.

## The page

- **The header**: h1 and tab "Activity types". The subtitle by role, never by count (Q12): "The forms registrars file and assessors complete." then an institution's "You can change the types you set; the College sets its instruments.", a College admin's "You can change the types College of Paediatricians of South Africa sets; the others you can read." (with no speciality yet, "… Scratch College has no speciality yet, so there are no types for you to change.", N12), or the Administrator's "You can change every type but the two that Wombat's releases write." **New activity type** (primary, `plus`, to `/admin/activity-types/new`) only once the read has answered and only where the caller has a scope to create in (`CanCreate`).
- **The column**: `<div class="people-page activity-types-page">`, `aria-busy` while it reads.
- **The search** (Q7, T326): `form.search-container[role="search"]` named "Search activity types", a `search-grid` with "Search by name or key" (`input#types-q`, type search) and **Show** (primary, `search`); **Clear** (outline) beside it once a search is applied. The search is applied on Show only, to name and key, case-insensitive, on the rows already read: no round trip and no filtering per keystroke.
- **The count line** above the table, `p#types-count[role="status"][tabindex="-1"]`: "23 activity types.", "12 of 22 activity types match “cpsa”.", each with its singular ("1 activity type.", "1 of 1 activity type matches “x”."). It takes the focus after Show; Clear gives it to the search box.
- **The list**: a `DataTable` with `StackWide`, captioned "Activity types you can read or change" ("Activity types matching “cpsa”" under a search).
  - **The order** (Q11): the caller's own types (those they may write) first, then the rest, each part by name, ordinal and ignoring case, so "Mini-CEX" and "Mini-CEX (Paediatrics)" are neighbours; the Administrator's by name alone.
  - Columns: **Name**, the row header, a `.person-cell`: the name over its key in `<code>`, a system-managed type's release line between them ("Written by the MSF release.", "Written by the learner-feedback release.", Q9); **Set by**, a `.person-cell`: the owner, the College by its short code ("CPSA", "DEMO-C"), the institution by its name, "The platform" for a Global type, over the scope in words ("Speciality · Paediatrics", "Sub-speciality · Paediatrics / Neonatology", "Institution", "Global"); **Published** "Version 1" or "Not published"; **Draft** "Saved 2026-11-10" (the South African day) or "None"; **Status** "Active" or "Inactive"; an unlabelled actions column.
  - **One link a row** in `col-actions col-actions--single`, as wide as its one button: **Edit** where the caller may write the type (`CanWrite`, the rule the commands refuse by; never on a system-managed type, T334), else **View**, both opening the builder, each named for its row (T239): "Edit KGK Teaching Session Log", "View Mini-CEX (Paediatrics)"; where two types share a name the key is added. No pager.
- **No match is not empty** (T326): the search kept above, a `.detail-card--empty#types-no-match[tabindex="-1"]`, its title `role="status"` 'No activity types match "zzz".' over "The search is kept above. Clear it to see all 23." ("… Clear it to see the one activity type." with one), and **Clear the search** (primary); the card takes the focus, and Clear the search puts it in the emptied search box.
- **The empty catalogue** (StatePanel): "No activity types" over "Create the first activity type to start the builder." with New activity type in the card as well as the header, for a caller who may create; "No activity type is open to you yet." otherwise. No search is drawn.

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the activity types." over one skeleton card; no New activity type (R2-A10).
- **Load failed**: "Could not load the activity types. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), no empty state and no New activity type (R2-A11, T329); the cause goes to the log. Try again focuses the h1 once it has read.

## Narrow widths

**At 1150px and below the list stacks** (app.css's "Flow 16: the catalogue, shared" 1150px query, `.activity-types-page .clinic-table--stack-wide`, with Curricula and a curriculum's items): each type a block, its name, release line and key first, then "Set by:", "Published:", "Draft:", "Status:", then Edit or View, 44px. Until 277e4a78 it stacked from 1024px; when Published, Draft and Status took `.col-fit--wide` (one line each from 1025px; "Not published" and "Saved 2026-11-10" had wrapped at 1280 with room beside them), the table needed 779px, 62px past its container at 1025px. As a table from 1151px, with 64px to spare there; at 1280 every cell's words on one line. Below 641px `.people-page`'s controls are 44px and the header's New activity type takes the row.

## Contrast

No new colour token. The key and the stacked labels `muted-text`, 5.09:1 on the surface; the outline links 4.86:1.

## Known gaps

- No page fault found re-syncing. The search lives in the page alone, not in the address: a reload, or a return from the builder by its trail, shows every row again.
