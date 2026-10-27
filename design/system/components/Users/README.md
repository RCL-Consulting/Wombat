# Users

Users (`/admin/users`, `Pages/Admin/Users/UsersList.razor`; its words in `UsersListWords.cs`, its addresses in `Shared/PeopleLinks.cs`, the scope's words in `Wombat.Application/Features/Users/PersonScopeWords.cs`): **everyone with an account in the reader's scope, one row a person**, a filter card over the list, each row's Manage opening the person's page (UserPage). Flow 12 (T376, 2026-10-23, e53d4ae7; boards R3-U01 to U14; S1, S2, S6, S7, S8, S9, S20, S21, F9, Q4, Q14, Q22, K25; check Left 2 and 3; build review P2) rebuilt it. Until then it listed a scope per role, role keys rather than labels, "Locked out" for a sign-in throttle as well as the administrative lock, a pager under any list, and a failed read's exception text (T329).

## Who reads it

Administrator and Institutional admin (`AdministratorOrInstitutionalAdmin`), each in their scope: the Institutional admin her institution's accounts, the Administrator every account, with an Institution column. Someone who also holds Trainee reads nobody (T278, T290): the page shows the header with no subtitle and one standing info alert, `#users-trainee-note`, "You hold the Trainee role, so you cannot view or change user accounts, including your own.", asked before any read (S8).

## The page

The header: "Users", "Everyone with an account at ‹institution›: the roles each holds, their scope, and whether they can sign in." (the Administrator's "Every account in Wombat: the institution, roles and scope of each, and whether they can sign in."; before the institution's name is read and on a failed read, "… at your institution …", S9), and **Invite a person** (`user-plus`, primary) once the list has loaded. Then the column, `.people-page` (its parts 24px apart, each with no margin of its own):

- **The filter** (S6, S21; FilterBar): `form.detail-card.people-filter`, `role="search"`, named "Filter people", its fields on `.search-grid`: **Name or email** (`type="search"`, help "A name in either order, part of one, or an address."), **Role** (Any role, then Institutional admin, Speciality admin, Sub-speciality admin, Coordinator, Committee member, Assessor, Pending trainee, Trainee, and No role; the Administrator's adds Administrator and College admin first; with No role chosen its help is "A completed or withdrawn registrar keeps an account and its records."), **Status** (Any status, Active, Locked out). Then `.filter-actions`: **Show** (`search`, primary) and **Clear** (outline), always shown. Nothing filters per keystroke.
- **The address is the filter**: Show navigates to `/admin/users?q=…&role=…&status=active|locked&page=…&size=…` (`PeopleLinks.UsersFiltered`), Clear to the bare route keeping a chosen page size; Home's role links (InstitutionalAdminHome) and a person's way back (`?back=`, UserPage) open the same filtered list. A role not offered to the reader reads as Any role.
- **People**: `section.list-section`, `h2#people-heading.list-section-title` (focusable by script) with `span.inv-count`: the number alone with no filter ("14"), else what matched, "1 of 14 match “zulu”", "2 of 14 hold no role", "1 of 14 are locked out" (`UsersListWords.HeadingCount`). Beside it an always-present `p.visually-hidden[role=status]` says it as a sentence ("2 of 14 people hold no role.", "No one matches “zzz”."). The heading takes the focus after Show, Clear and a page turn, and on arrival with a filter set; never on a plain first visit.
- **The table**: `DataTable` with `StackWide`, captioned "At ‹institution›, by last name: each person's roles, one scope for all of them, and whether they can sign in." (the Administrator's "Every institution, by last name: each person's institution, roles, scope and status."):

| Column | What |
|---|---|
| Name | **the row's header**: `.person-cell`, the name over the address, the address with `<wbr>` after its @ and `overflow-wrap: break-word` (no `.col-wrap` on the header, check Left 3) |
| Institution | the Administrator's only, `.col-wrap`; "Global (no institution)" for none |
| Roles | `ul.role-cell`, one label a line in `WombatRoleLabels.InOrder`'s order, never keys (Q14). None: `.no-role-cell` "No role", and for an ended trainee profile only, its `.field-help` "A completed or withdrawn registrar keeps an account and its records." (S2) |
| Scope | `.col-wrap`: **one scope per person** (Q4, S1; `PersonScopeWords`): the institution's name for an Institutional admin, the College's for a College admin, "—" with no institution, else "Paediatrics / Paediatrics", "Paediatrics", or "Any speciality" |
| Status | "Active", or **Locked out**, `span.badge.badge-declined.status-locked` with the `lock` icon (14px) and its word, the administrative lock only (K25), never a sign-in throttle |
| (Actions) | **Manage**, `.btn .btn-outline .btn-sm`, a link to `PeopleLinks.User(id, back: this page's address)`, named "Manage ‹name›" (the address added where two share a name, T239) |

- **The pager only when the matches run past one page** (F9, round-3-check Left 2: `MatchCount > PageSize`, 20 a page by default), so fourteen draw none. Its turn keeps the filter in the address and focuses the heading; a change of rows per page keeps the focus on its select.

## States (U11 to U14, T329)

- **No match**: the empty card, "No one matches “zzz”" (or "No one matches these filters") over "Nobody at ‹institution› has “zzz” in their name or address. Press Clear to see all 14 people." The form stays, keeping what was typed.
- **No account**: "No accounts yet" · "Nobody has an account here yet. Use Invite a person to add the first."
- **Loading**: `p.loading-line` "Loading the users." over two skeleton cards; the header has no action yet.
- **Load failed**: "Could not load the users. Nothing has changed. Try again, or come back in a few minutes." with Try again (StatePanel), never the exception's text; a Try again that loads focuses the heading.

## Narrow widths

The table stacks at 900px and below (`StackWide`): each person a block, the name over the address heading the labelled cells; Manage 44px at 900px and below. Below 641px every control on the page is 44px (`.people-page .btn`, `.form-control`, `.form-select`), the filter's Show and Clear one under the other at the card's width (`.filter-actions .btn`, `.people-page .people-filter .filter-actions`). Institution and Scope keep their words whole between 901 and 1100px: `.people-page .clinic-table th.col-wrap` floors them at 7rem (build review P2). Measured on the replay at 1280, 1000 and 390 (T376 step 8): no sideways scroll.

## Accessibility

The search form is a landmark named "Filter people"; the count is said by a status that is always on the page, its words set by the filter; the stacked table keeps its roles (`table`, `rowgroup`, `row`, `columnheader`, `rowheader`, `cell`); Manage is named by its person.

## Contrast

No new colour token. The count and the address (`muted-text`) 5.09:1 on the surface; the Locked out badge `danger-color` on `danger-bg`, as every declined badge (Badge).

## Known gaps

None found re-syncing.
