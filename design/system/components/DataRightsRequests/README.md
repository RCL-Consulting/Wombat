# DataRightsRequests

Data rights requests (`/admin/data-rights`, `Pages/Admin/DataRights/RequestsList.razor`; its words `DataRightsRequestWords.cs`, the shared ones `Components/Shared/DataRightsWords.cs` and `DataRightsLabels`; the read `ListDataRightsRequestsQuery`, `PagedDataRightsResult`): **the requests about personal data that are the reviewer's to decide, newest first.** Flow 14 (T387, 2026-10-30, e8eea1d2; boards R3-Q01 to Q13, K02; Q1, Q2, Q8 to Q11, Q23, M4 to M6, N4, N5, N13, N15, S1, S4, S12, S18, F1, F10; build review P2, P7) rebuilt it on flow 12's list column. Until then it printed types and statuses by their enum names, its filters offering every enum value (Objection and UnderReview among them), filtered with "Apply filters" in the header, built its own pager and printed an exception's message on a failure.

## Who reads it

Administrator and Coordinator (`[Authorize(Roles = "Administrator,Coordinator")]`; NavMenu: the Administrator's Platform group and the Coordinator's last link, `shield-check`). A Coordinator reads her institution's requests, the Administrator every institution's (Q1, Q23). A Coordinator set to no institution reads none (S4). It is a list, so it lights itself and has no trail.

## The page

- **The header**: h1 and tab "Data rights requests"; the subtitle says whose (S4): "Requests from people at Kgosi Kgari Teaching Hospital about their personal data: decide each, and carry an approved correction through.", "Requests from people at every institution about their personal data: decide each, and carry an approved correction through." for the Administrator, or "Requests about personal data: decide each, and carry an approved correction through." while the first read runs and for a Coordinator set to no institution. No header action.
- **The column**: `<div class="people-page data-rights-requests">` (flow 12's list column and its phone rules), `aria-busy` while the first read runs.
- **The filter** (FilterBar; S12, S18): `form.search-container[aria-label="Filter Data rights requests"]` (`novalidate`), on `.search-grid`: **Type** (`select#f-type`: "All types", then Access, Export, Rectification, Erasure; never Objection, Q8) and **Status** (`select#f-status`: "All statuses", then Submitted, Approved, Completed, Rejected, Withdrawn; Under review is gone), each by label (T324); `.filter-actions` **Show** (primary, the form's submit, so Enter is Show) and, only once a filter is applied, **Clear filters** (outline). Nothing changes until Show. The filter is not in the address.
- **The list**: `section.list-section`, its heading `h2#rq-h.list-section-title` "Requests (5)" (`tabindex="-1"`: Show, Clear filters and a page turn focus it).
  - A `DataTable` with `StackWide` and `Class="data-rights-table"` (DataTable), captioned for whose and what was asked (T280, S12): "Every institution's data rights requests, newest first", "Kgosi Kgari Teaching Hospital's data rights requests, newest first: Export, Submitted" ("Your institution's" when no name is read).
  - Columns: **Submitted** (the moment in SAST to the minute, "2026-10-30 19:22 SAST"); **Requester**, the row header (`th scope="row"`), a `span.person-cell`: the live name over the address, broken after its @ (F1, S1), or the pseudonym over "Erased 2026-10-30" (N5, N13); **Institution**, the Administrator's only, "None" for a request with none (Q9); **Type** by label; **Status**, a badge (`BadgeFor.DataRightsRequest`; Badge); **Request id** (`span.request-id`: the mono face at 0.875rem, `overflow-wrap: anywhere`, so a 36-character id breaks where it must and stays whole where it fits, review P10); an unlabelled actions column with one **Review** (outline, small), a link to `/admin/data-rights/{id}` (DataRightsRequest), named for its row (T239, N4): "Review the Export request from Anele Dlamini, 2026-10-30 19:19 SAST", to the second where two of one type from one person share the minute.
  - **PagerControls** only past one page (F10): twenty a page, `Label` "Requests, pages", `PageSizeLabel` "Requests per page".
- **No match is not empty** (DESIGN.md § List page; Q2, S12): filtered, `.detail-card--empty` "No requests match" over what was asked, "No Erasure request at Kgosi Kgari Teaching Hospital. Clear filters to see every request.", "No request is Submitted at …", "No Erasure request is Submitted at …" ("… at any institution." for the Administrator). Unfiltered and empty: "No requests" over "Nobody at Kgosi Kgari Teaching Hospital has made a data rights request yet." ("Nobody has made a data rights request yet." for the Administrator; "Your account is not set to an institution, so no request is yours to decide." for a Coordinator with none).

## Reads and focus (review P2, N15)

Show, Clear filters, a page turn and a change of size **answer in place**, as Users does: the form, the table and the pager stay, a field typed but not yet shown is kept, and only the first read and Try again draw the loading state. Show, Clear filters and a page turn then focus "Requests (n)"; a change of size leaves the focus on its select. A later read that answers first wins (a read counter drops the earlier one).

## States

- **Loading**: `p.loading-line[role="status"]` "Loading the data rights requests." over two skeleton cards.
- **Load failed**: "Could not load the data rights requests. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel, `refresh-cw`), which focuses the h1 once it reads (N15). The cause goes to the log (T329).

## Narrow widths

**The queue stacks from 1100px, not 900px** (Q23): beside the 250px sidebar its six columns (seven for the Administrator) leave its names, types and badges no room from 901 to 1100px, so app.css repeats the stack-wide rules for `.data-rights-requests` under `@media (max-width: 1100px)`, its row buttons 44px. Stacked, each request is a block: "Submitted: …" first, the requester (name over address, or pseudonym over "Erased …") with no rule under it (`.data-rights-table.clinic-table--stack-wide tbody th[scope="row"]`, review P7), then "Institution:", "Type:", "Status:", "Request id:" and Review. Below 641px the filter's selects and the pager's page-size select are 44px (`.search-container .form-select`, `.pager .form-select`: a system rule for every list since flow 14, M4), Show and Clear filters the row's width and 44px (`.filter-actions .btn`), and `.people-page`'s controls 44px. Measured on the replay at 390, 768 and 1280, and at 901 to 1100: nothing sideways.

## Contrast

No new colour token. The address, the erased line and a stacked cell's label in `muted-text`, 5.09:1 on the surface; the badges as Badge's.

## Known gaps

- **T390** (proposed): with a size that holds every row (50 or 100 for 50 requests) the pager is not drawn, so its select cannot bring the size back down (the rule `TotalCount > PageSize`, `RequestsList.razor:145`, as on four other lists).
- app.css's comment on `.data-rights-table tbody th[scope="row"]` says each of the three tables puts the row header first in its row; on the queue it is the second cell, after Submitted. The rule holds either way. Found re-syncing.
