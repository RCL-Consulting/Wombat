# Institutions

Institutions (`/admin/institutions`, `Pages/Admin/Institutions/InstitutionsList.razor`; its words `InstitutionWords.cs`; the read `GetInstitutionsListQuery`; the addresses `Components/Shared/CatalogueLinks.cs`, `Institutions`, `NewInstitution`, `Institution(id)`): **every institution, by name, each row saying how to reach it, whether it is active and when it was created.** Flow 18 (T400, 2026-11-11, `34b5077e`; boards R2-I01 to I06, R3-I07, I15, I16; Q5, T24, T239, T329, S3, S8, C1) rebuilt it on the people pages' column. Until then it showed a Created date in the server's local time, an Edit not named for its row and its read's exception text.

## Who reads it

The Administrator alone (`[Authorize(Policy = "Administrator")]`; NavMenu's Institutions in the Organisations group). It is a list, so it lights itself and has no trail; an institution's record and Create institution light it.

## The page

- **The header**: h1 and tab "Institutions"; the subtitle "The institutions that hold Wombat's people and records."; **Create institution** (primary, `plus`, to `/admin/institutions/new`) once the rows have loaded (R2-I04), so it is not offered beside a failure.
- **The column**: `<div class="people-page institutions-page">`.
- **The list**: a `DataTable` with `StackWide`, captioned "Institutions", **by name, no pager** (institutions are few; Q5).
  - Columns: **Name**, the row header (`th scope="row"`, `.col-wrap`); **Short code** (`.col-fit--wide`); **Contact email** (`.col-wrap`, the address through `InvitationText.Address`, so it breaks after its @; "None" muted where there is none); **Status**, a badge: Active `badge-completed`, Inactive `badge-withdrawn` (the neutral tint under its own name, `BadgeFor.ProgrammeOutcome`); **Created**, the South African day (T24, `SastTime.Day`); an unlabelled actions column.
  - **Edit** a row (`.btn-sm.btn-outline`), named for it, "Edit Kgosi Kgari Teaching Hospital" (`aria-label`, the visible word first, as `RowActionMarkupTests` holds every row action; T239).
- **Empty** (StatePanel's empty state, kept from master; no board draws it): "No institutions yet" over "Create the first institution to start configuring scope and onboarding."

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the institutions." over one skeleton card; no Create institution.
- **Load failed**: "Could not load the institutions. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), no Create institution (T329); the cause goes to the log. Try again focuses the h1 once it has read (S3).

## Narrow widths

**At 1150px and below the list stacks** (C1: `.institutions-page` in the seven selector lists of app.css's flow 16 shared 1150px query, with Curricula, a curriculum's items, the activity types, Scheduled jobs and the audit log), **a readability choice, not an overflow fix** (S8): above 945px the table never overflows, but at 1151px Contact email ran over three lines beside short cells. Stacked, each institution is a block, its name first, then "Short code:", "Contact email:", "Status:", "Created:", then Edit, 44px. Below 641px the header's Create institution takes the row.

## Contrast

No new colour token. "None" and the stacked labels `muted-text`, 5.09:1 on the surface; Inactive's neutral badge `text-color` 11.36:1 on `header-bg`, its `input-border` edge 3.42:1.

## Known gaps

- **Not filed** (T400's replay): a 200-character name, the most the record takes, runs over seven lines in the Name column at 1280; no share of the table suits every name length.
