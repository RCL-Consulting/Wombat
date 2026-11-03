# Epas

EPAs (`/admin/epas`, `Pages/Admin/Epas/EpasList.razor`; its words `EpaWords.cs`; the read `GetEpaListQuery`, `EpaListDto`, `EpaDto`, the rules `EpaReads` in `GetEpas.cs`): **the EPAs the caller reads, and whose each one is.** Flow 15 (T392, 2026-11-02, c931d56f, with 50ca6470: the title column's room and the stack to 1024px; boards R2-C01 to C03, R3-C09, C15 to C17; Q3, Q12, F8, M1, P5, S4, S7, S10, S14, S19) rebuilt it on flow 12's list column. Until then it offered Edit on every row, a national EPA's too, which `UpdateEpa` then refused (T291 item 1), marked no EPA as an institution's own, offered Create EPA with no discipline to put it in, and printed its read's exception.

## Who reads it

Administrator, College admin and Institutional admin (`NationalCatalogueAccess`; NavMenu's EPAs, `list-checks`). What each reads is the list's rule, the same one an EPA's page reads by (AdminEpaPage): an Institutional admin, the national EPAs of the disciplines her institution has actively adopted and its own; a College admin, the College's; the Administrator, every one. It is a list, so it lights itself and has no trail.

## The page

- **The header**: h1 and tab "EPAs"; the subtitle says whose (`EpaWords.ListSubtitle`): "The EPAs of the curricula Kgosi Kgari Teaching Hospital has adopted. The College's are read only; your institution's own are yours to change." (an Institutional admin, a reader of the College's), "Every College's EPAs, and every institution's own." (the Administrator), "Maintain the entrustable professional activities available to curricula and forms." (a College admin, as built). No subtitle while the first read runs. **Create EPA** (primary, `plus`, round-3-check decision 4) only where it can be completed (`EpaListDto.MayCreate`, Q12, S4): the Administrator, a College admin with a College, an Institutional admin only while her institution has actively adopted a discipline.
- **The column**: `<div class="people-page epas-page">` (flow 12's list column and its phone rules), `aria-busy` while the first read runs.
- **The list**: a `DataTable` with `StackWide`, captioned for whose (`EpaWords.Caption`): "EPAs your institution reads", "Every EPA", "Entrustable professional activities" (the College's).
  - Columns: **EPA**, the row header (`th scope="row"`, `.col-wrap.col-wrap--wide`): its label as a link to its page, "PAED-001 — Providing paediatric emergency care to children" (`EpaOptionLabel`, the em dash, S10), and under it, for an institution's own EPA, **the owner mark in words** in a `div.muted.text-sm` (Q3, F8: never a badge on the admin pages): "Your institution's own EPA", or "Kgosi Kgari Teaching Hospital's own EPA" to a caller whose reads span institutions (`EpaDto.NamesOwner`, the Administrator); **College**; **Sub-speciality** ("Paediatrics / Paediatrics"); **Status**, "Active" or "Inactive" in words; an unlabelled actions column (its header the `.visually-hidden` "Actions").
  - **Every row says something in its actions cell** (M1, T211): **Edit** (outline, small, to the EPA's page) only where the EPA may be changed (`EpaDto.MayChange`, `UpdateEpa`'s own rule, never the caller's roles: S4), named for its row (T239): "Edit KGK-001 — Running a paediatric outreach clinic at a district hospital", with whose it is, then its discipline, then its College where two would read alike (`RowNames.Distinct`); else `span.muted` **"Set by the College."**. The row's link reaches the EPA's page either way: its form, or a national EPA's record (AdminEpaPage).
  - **PagerControls** under the table only while the list runs past the chosen size (P5): twenty a page, `Label` "Pages of EPAs", `PageSizeLabel` "EPAs per page". The rows are read once and paged on the page, so a turn reads nothing.
- **Empty** (StatePanel): "No EPAs yet" over "Create the first EPA to start building curricula and assessment forms." where Create EPA is offered; where it is not (an institution that has adopted nothing, Q12, R2-C01), "Your institution has not adopted a curriculum yet, so there is no discipline to add an EPA to. Adopt one under Curricula." with **Go to Curricula** (primary) and no Create EPA.

## Reads and focus

A page turn focuses the h1, the page's one heading (P5); a change of size leaves the focus on its select. Try again focuses the h1 once it has read (S7).

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the EPAs." over two skeleton cards (S19).
- **Load failed**: "Could not load the EPAs. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`; S7, R3-C17), no empty state under it. The cause goes to the log (T329).

## Narrow widths

**From 1025px the EPA column takes 45%** (`.epas-page .clinic-table th.col-wrap--wide`, 50ca6470): at `.col-wrap--wide`'s 20% a title wrapped over three to six lines at 1280 while Status and the actions, both auto, left an empty band. **At 1024px and below the list stacks** (the rules written once for the items list and the EPA list, `.curriculum-items-page` and `.epas-page` `.clinic-table--stack-wide`, in app.css's "Flow 15: a curriculum's items" section): at 901px its five columns left the EPA 144px. Stacked, each EPA is a block: the label and its mark first (`overflow-wrap: anywhere`, so a 200-character title breaks inside the cell, R2-C19), then "College:", "Sub-speciality:", "Status:" and Edit or "Set by the College.", its Edit 44px. Below 641px `.people-page`'s controls are 44px, the pager's select among them (flow 14's system rule). Re-measured on a republished app after 50ca6470 (the replay and its states pass): nothing sideways.

## Contrast

No new colour token. The owner mark, "Set by the College." and a stacked cell's label in `muted-text`, 5.09:1 on the surface.

## Known gaps

- **T390** (proposed): with a size that holds every row the pager is not drawn, so its select cannot bring the size back down (`list.Epas.Count > _pageSize`; build review P5, left to T390).
- The College admin's subtitle and caption are the built words, which no board redraws.
- Three states were taken before the column fix and not re-taken: `epas-list--college`, `--institution`, `--inactive` (the layout only).
- A College admin set to no College reads the empty state's Institutional admin words ("Your institution has not adopted a curriculum yet, …"): the body is chosen by `MayCreate` alone. Inferred from the code; no step reaches it.
