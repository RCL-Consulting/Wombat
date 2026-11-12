# AuditLog

The audit log (`/admin/audit`, `Pages/Admin/Audit/AuditList.razor`; its words `AuditWords.cs`; its addresses `AuditLinks.cs`, `AuditLogAddress`; the read `ListAuditEntriesQuery`, `AuditLogRowDto`, `AuditInstitutionFilter`, `AuditActorKind`; the names through `DataRightsPeople.NamesOfAsync`; a failure's stored words `Wombat.Application/Audit/AuditFailureText.cs`): **every consequential action in a fixed window, newest first, each row saying when, who, what and how it ended, with its Detail.** Flow 18 (T400, 2026-11-12, `34b5077e`; boards R3-L01 to L13, R2-L14, L15; T4, T21, T101, T277, K8, K10, K15, K16, M3, N2, Q15, C1, P1) rebuilt it. Until then it showed the category as an enum's name, a user id for the person, its times in UTC through `ToLocalTime()`, a window that slid on every page turn, and a failed command's exception text, which it also stored.

## Who reads it

The Administrator (every row; the institutions' and those with none) and the Institutional admin (the rows of their own institution; T101, not widened). NavMenu's Audit log (the Administrator's Platform group, the Institutional admin's Access and audit). An audit entry lights it (AuditEntry).

## The page

- **The header**: h1 and tab "Audit log"; the subtitle by role, "Every consequential action across the institutions, and those with no institution, newest first." or "Every consequential action at Kgosi Kgari Teaching Hospital, newest first."; no header action.
- **The column**: `<div class="people-page audit-page">`, `aria-busy` while it reads.
- **The filter** (Users' card pattern without the card; FilterBar): `form.search-container.people-filter`, `role="search"`, named "Filter the audit log", on `.search-grid`: **Category** by label ("All categories", then Command, Authentication, Permission, Committee, MSF, Export, Data rights, Job, Activity type; never the enum's name, K15); **Action** (part of a name, with the help "Part of a name: "Msf" finds the MSF commands."); **Person's user id**; **Subject type**; **From (SAST)** and **To (SAST)**, `datetime-local` on the South African clock (To's help "Empty: up to when you press Show."); **Result** (All results, Success only, Failures only); and, for the Administrator alone, **Institution** (All institutions, No institution, then each by name). Then `.filter-actions`: **Show** (primary, `aria-disabled` until the first read is in, R2-L14) and **Clear filters** (outline) once a filter is applied.
- **The window is fixed and said** (T21, T277): on arrival the last 24 hours to the moment the page opened; after Show, From to To, or to the moment Show was pressed. That moment rides in the address (`opened=` or `shown=`, beside `category=`, `action=`, `person=`, `subject=`, `from=`, `to=`, `result=`, `institution=`, `page=`, `size=`), so a page turn and the trail back from an entry read between the same two moments.
- **The list section** (`section.list-section`, named by its `h2.list-section-title` "Actions", `tabindex="-1"`): **the count line** (`p[tabindex="-1"]`, "166 actions.", "1 action.", "No actions match these filters."), which takes the focus after Show and **is no live region** (N2); then the **window line** (`p.muted.text-sm`, a standing notice): "Read from 2026-11-10 18:58 SAST to 20:24 SAST, when you pressed Show. Turning a page keeps this window." ("… : the last 24 hours, when you opened the page. …"; "… when you opened the page. …" for a From with the address opened; "Read from ‹From› SAST to ‹To› SAST. …" with a To typed).
- **The list**: a `DataTable` with `StackWide`, captioned "Actions, newest first", **no row header**:
  - **When**: the day and the hour to the second, each a `span.nowrap`, so a time breaks only between them.
  - **Person**: the name read now (an erased person's pseudonym, Q15, M3), "MSF respondent (no account)" or "No signed-in person" (K8); for the Administrator a `.person-cell` with the row's institution under it, "No institution" for none.
  - **Category** by label ("Other" for a value the map does not know).
  - **Action** (`.col-wrap`), the command's name, with **no share** of the table (`width: auto`; its 7rem floor and its break kept for a narrow table).
  - **Result**: the badge, Success `badge-completed` or Failed `badge-declined`, and a failure's stored words under it (`span.text-sm`): a refusal's own words, or "The action failed." (K10, P1: the exception's text goes to the log, never to the row). **While a row shows them, Result asks for 20% of the table.**
  - **Detail** (`.col-fit--wide`, its header visually hidden): an outline small link named "Detail: ‹action› at ‹YYYY-MM-DD hh:mm:ss› SAST", " by ‹person›" added where two rows share action and second (`RowNames`).
- **The pager** (PagerControls): drawn whenever a row is, 50 a page, in its own order.
- **The focus**: Show focuses the count line; a page turn focuses "Actions"; Clear filters and "Clear the filters" focus Category; arriving back by an entry's trail (`entry=`) focuses that entry's Detail (R3-L13).
- **Nothing matching** (K16): a `.detail-card--empty` "No actions match" over "Nothing matches these filters between these times. Widen From and To, or clear the filters.", with **Clear the filters** (primary, as R3-L11 draws it and Run history has it) while a filter is applied.

## States

- **Loading**: under "Actions", `p.loading-line[role="status"]` "Loading the audit log." over a skeleton card; the filter stands, Show `aria-disabled`.
- **Load failed**: "Could not load the audit log. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), which focuses the h1 once it reads.

## Narrow widths

**At 1150px and below the list stacks** (C1: `.audit-page` in the seven selector lists of app.css's flow 16 shared 1150px query), a readability choice, not an overflow fix (S8); below 945px the stack also keeps the table in its container. Stacked, each action a block of labelled lines, "When:", "Person:", "Category:", "Action:", "Result:", then Detail, 44px. Below 641px the filter's controls and Show are 44px, Show the row's width. Action had a 14% share until the build's sweep: at 1280 it held 144px and broke "UpdateCurrentUserProfileCommand" over three lines beside When's and Person's empty bands.

## Contrast

No new colour token. The window line, the stacked labels and the institution under a name `muted-text`, 5.09:1 on the surface; the badges are the five tints (Badge).

## Known gaps

- **Not filed** (T400's replay): in a window with failures, Result's 20% share leaves "TransitionActivityCommand" over two lines beside a 33px band at 1280.
- **Found re-syncing and fixed**: the no-match card's Clear the filters was built as an outline button, where R3-L11 draws a primary (as Run history's is).
