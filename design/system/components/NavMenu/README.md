# NavMenu

The sidebar: the Wombat lockup on a darkened brand row, then one link per page the signed-in user's roles offer, each with a Lucide icon, ending in Logout.

## What the consumer provides

Nothing: MainLayout renders it, and the links come from the user's roles. Each page is declared once in `NavMenu.razor`'s `Sections`, with one label and one icon, and a user holding several roles sees each role's links in turn, a page an earlier role offered not repeated.

## Structure (DESIGN.md § The NavMenu)

- `div.top-row` (black at 40% over the gradient, min height 4.25rem) holding `a.navbar-brand`: the 40px mark (`img.navbar-brand-mark`, `/brand/wombat-mark.svg`) and `span.navbar-brand-text` "Wombat" in the `wordmark` style, white.
- `input.navbar-toggler`: a CSS-only checkbox that shows and hides the nav below 641px. No JavaScript.
- `div.nav-scrollable > nav > div.nav-item > a.nav-link`: 3rem tall, 0.9rem, rgb(215 215 215), `radius-4`, the icon at 1.25rem. Active (`NavLink`'s `active` class): white on white at 37%. Hover: white on white at 10%. From 641px the list scrolls within the sidebar.
- Logout: a `<form action="/account/logout" method="post">` with an antiforgery token, its button `.nav-link.nav-logout-button`.

## Who sees what

| Role | Links |
|---|---|
| Signed out | Home, Sign in |
| Everyone signed in | Home, My Account, Data Rights, …, Logout |
| Trainee | Activities, My Activities, MSF Reports, My Committee Reviews, My Progress, Export Portfolio |
| PendingTrainee | Activities, My Activities |
| Assessor | Activity Inbox, Recent Activities |
| Coordinator | Data Rights Requests, MSF Campaigns, Committee Reviews, Decisions Due, Stalled Activities |
| CommitteeMember | Programme Trainees, Decision Panels, Committee Reviews |
| SpecialityAdmin, SubSpecialityAdmin | Programme Trainees, Decision Panels, Committee Reviews, STAR Review Queue, Decisions Due |
| CollegeAdmin | Specialities, EPAs, Curricula, Activity Types |
| InstitutionalAdmin | Curriculum Adoptions, EPAs, Curricula, Activity Types, Entrustment Scales, Trainees, Assessors, Invitations, Users, SSO Mappings, Audit Log, Decision Panels, Committee Reviews, Decisions Due |
| Administrator | Colleges, EPAs, Curricula, Institutions, Invitations, Users, Activity Types, Entrustment Scales, Scheduled Jobs, SSO Mappings, Audit Log, Decision Panels, Committee Reviews, Decisions Due, Data Rights Requests, System |

## Rules

- Every link opens a page that admits the role offering it, and no two links share a label ("My Committee Reviews" beside "Committee Reviews"). `NavMenuAuthorizationTests` parses the table above, so a nav change is a DESIGN.md change.
- Icons come from `Icon`; never an icon font.
- Nav labels are Title Case page names.

## Contrast

Item text 9.78:1 and up on the gradient; white on the active background 4.51:1 at the top, 5.14:1 at the violet end. Nav links take the page's `focus-ring` (4.46:1 and 5.14:1 on the gradient's ends).

## Known gaps (design/BRIEF.md § 4, § 5.5)

- The list is long and flat: 20 links for an Administrator, 18 for an InstitutionalAdmin, a multi-role user the union; no grouping.
- Five links open "Coming soon" placeholders (Recent Activities, Stalled Activities, Programme Trainees, STAR Review Queue, System).
- Links render underlined: `.nav-link` never resets `text-decoration`.
- The nav's colours (215 grey, white at 37% and 10%, black at 40%) are raw values in NavMenu.razor.css, outside :root.
- The restructure (W-008) redesigns the navigation first.
