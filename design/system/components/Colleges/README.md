# Colleges

Colleges (`/admin/colleges`, `Pages/Admin/Colleges/CollegesList.razor`; its words `CollegeWords.cs`; the read `GetCollegesListQuery`, `CollegeDto`, in `Wombat.Application/Features/Colleges/GetColleges.cs`; the addresses `Components/Shared/CatalogueLinks.cs`): **the national Colleges, each a row with its way to its record and its specialities.** Flow 16 (T394, 2026-11-05, de4cc718, with a2878763, 7d4c4490 and f7cb1903: the column widths at 1280; boards R3-B01, B02 (and B02-1024, B02-1025), B11b, B16, B17a, R2-B14a, B15a; S15, N2, Q20, T239, T329) rebuilt it on flow 12's list column. Until then it was subtitled "National governing bodies (CMSA constituent colleges) that own the EPAs and curricula institutions adopt.", captioned "Colleges", headed each row by a plain cell, printed Created as the server's local time with the hour, offered Create college before the read had answered, and printed its read's exception text.

## Who reads it

The Administrator alone (`[Authorize(Policy = "Administrator")]`; NavMenu's Colleges, `graduation-cap`, in the Administrator's Organisations group beside Institutions). A College admin is Access denied here; their Specialities is the College's tree. It is a list, so it lights itself and has no trail; a College's record and its tree light it (`NavOwners`: `CollegeEdit` and the tree's pages under Colleges for the Administrator).

## The page

- **The header**: h1 and tab "Colleges"; the subtitle "The national Colleges. Each College's admin keeps its specialities, EPAs and curricula; you keep the Colleges themselves." (S15). **Create college** (primary, `plus`, to `/admin/colleges/new`) **only once the read has answered** (`_colleges is not null`): not while it loads, not after a failed read. From 1024px it stays beside the h1, the subtitle wrapping under it (Q20; PageHeader).
- **The column**: `<div class="people-page colleges-page">` (flow 12's list column and its phone rules). It carries no `aria-busy`.
- **The list**: a `DataTable` with `StackWide`, captioned "Every College", in the read's order (by name: College of Neurologists of South Africa, College of Paediatricians of South Africa, Demo College after Step 6.1).
  - Columns: **Name**, the row header (`th scope="row"`, `.col-wrap`): the College's name; **Short code** (`.col-fit`): "CPSA", "DEMO-C"; **Description** (`.col-wrap`, its header `.col-wrap--wide`), as the College wrote it, empty where it has none; **Status** (`.col-fit`), "Active" or "Inactive" in words (`CollegeWords.Status`); **Created** (`.col-fit`), the South African day of its creation in ISO form, "2026-11-05" (`ProgrammeCalendar.DateOf`, N2); an unlabelled actions column (`.col-actions`, its header the `.visually-hidden` "Actions").
  - **Two outline links a row** (`.btn-outline.btn-sm` in `div.actions-cell`), each named for its College, whose name is unique (T239): **Edit**, "Edit College of Paediatricians of South Africa", to its record (CollegeRecord); **Specialities**, "Specialities of College of Paediatricians of South Africa", to the College's tree (`CatalogueLinks.CollegeTree`). No list edits a row in place.
- **Empty** (StatePanel): "No colleges yet" over "Create the first college to start defining national specialities, EPAs and curricula.", Create college in the header. The seed makes it unreachable (`CollegeWords.EmptyTitle`'s remark).

## Reads and focus

The page reads once. Try again focuses the h1 once it has read (`PageFocus.FocusHeadingAsync`). A create, a save or a deactivation happens on the record, which stays on the record; the list is read again when it is next opened.

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the Colleges." over one skeleton card; no Create college (R2-B14a).
- **Load failed**: "Could not load the Colleges. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), no empty state under it and no Create college (R2-B15a). The cause goes to the log (T329).

## Narrow widths

**At 1024px and below the list stacks** (app.css's "Flow 16: the catalogue, shared" 1024px query, `.colleges-page .clinic-table--stack-wide`, written once with the EPA list and the College's tree): each College is a block, its name first, then "Short code:", "Description:", "Status:", "Created:", then Edit and Specialities, 44px. **From 1151px the text columns take the room** (a2878763, 7d4c4490): Name 28% (`.colleges-page .clinic-table th.col-wrap:not(.col-wrap--wide)`), Description 27% (`th.col-wrap--wide`), while Short code, Status and Created are `.col-fit`, as narrow as their content and on one line at every width (7d4c4490 and f7cb1903: "DEMO-C" had broken after its hyphen, and "2026-11-05" after its first hyphen). At 1280 the replay measured the table at its container's 972px, a name on two lines at most in 272px, a description on two to four in 262px, Short code 106px, Status 75px and Created 106px, each on one line. Below 641px the header's Create college takes the row, 44px (the system's header rule), and `.people-page`'s controls are 44px. Swept at 900 to 1100px in 25px steps on the build (`sweep.md`) and the replay: the table never wider than its container, nothing sideways.

## Contrast

No new colour token. A stacked cell's label in `muted-text`, 5.09:1 on the surface; the outline links 4.86:1.

## Known gaps

- **Between 1025px and 1150px the text columns lose their widths**: the Name 28% and Description 27% rules sit inside the `@media (min-width: 1151px)` block, which f7cb1903 opened above them for Curricula, while their own comment says "from 1025px" and the list is a table from 1025px; there Name and Description fall back to `.col-wrap`'s 14% and `.col-wrap--wide`'s 20%. Inferred from app.css; no capture measures it (the sweep stops at 1100px and found no overflow, only the squeeze).
- **Colleges with five rows** (R3-B16, the heavy list) were not swept: the build's sweep needed two more Colleges than the scenario holds (`sweep.md`).
- The column has no `aria-busy` while the first read runs, as the EPA list's and Curricula's have.
