# CollegeAdminHome

The College admin's Home (`Pages/Dashboards/CollegeAdminDashboard.razor`, drawn by Home for the acting role College admin; its words `CollegeTreeWords.HomeCardTitle` and `HomeCardBody` in `Pages/Admin/Institutions/CollegeTreeWords.cs`; the addresses `CatalogueLinks`): **one card that says what the College keeps, with its four ways in.** It reads nothing, so it has no query, no loading and no failure of its own. Flow 16 (T394, 2026-11-05, de4cc718; board R2-A01; Q12, Q17, D57) redrew it. Until then the card read "Your college's specialities, its EPAs and its curricula. Institutions adopt an active curriculum to train against it.", offered three links (no Activity types) in an `.actions-cell` inside the body, where no phone rule reached them (36px at 390, A.7.10-1), and its card was no named section.

## Who reads it

A College admin acting as one (`[Authorize(Roles = "CollegeAdmin")]`; Home's `case WombatRoles.CollegeAdmin`): Anton Kruger, the CPSA's. Home is the first item of their navigation, one ungrouped list: Home, Specialities, EPAs, Curricula, Activity types, Entrustment scales, then My data rights under a rule (`NavItems`).

## The page

Home's header as every role's (Home): "Home", "College admin · Semester 2, 2026" (`HomeFrame.Subtitle`), **no header action** (`HomeFrame.ActionFor` has none for the role: the College's work starts from a link). The other-role line draws nothing unless they also hold Assessor (OtherRoleLine). Then one `.dashboard-grid` holding one DashboardCard:

- **National catalogue** (`book`, `Emphasis`, `Span="2"`, `HeadingId="home-card-catalogue"`): a `section.detail-card.detail-card--emphasis.dashboard-span-2` named by its h2 `#home-card-catalogue`.
- Its body, `p.muted`: "Your College's specialities, EPAs, curricula and activity types. An institution can adopt a curriculum version once you open it." (Q12: what the College keeps, and that a version reaches an institution once the College opens it for adoption).
- **Its four links in the card's footer** (`.dashboard-card-footer`, Q17), each `a.btn.btn-outline`, in the order of the menu: **Specialities** (`/admin/specialities`, which sends to the College's tree, or says "No College on your account" to an account that names none), **EPAs**, **Curricula**, **Activity types** (`/admin/activity-types`). Not Entrustment scales: the College admin reads the scales and changes none (D57), so the menu offers them and Home does not.

No other card: no count, no Quick links.

## Narrow widths

The card spans two of the grid's three tracks from 1100px (at 1280 the third stays empty), the whole row of two from 901px, and the one column at 900px and below. The footer sits at the card's end (`justify-content: flex-end`) with a rule above it. Below 641px the footer's links are 44px (`.dashboard-card-footer .btn`, the system's phone rule) and wrap (`.dashboard-grid .dashboard-card-footer`), which is why the links moved there (R2-A01). Replayed at 390 (A.7.10): the four links 44px each, nothing sideways.

## Contrast

No new colour token. The body in `muted-text`, 5.09:1 on the surface; the emphasis stripe `secondary-color`; the outline links 4.86:1.

## Known gaps

- **The narrow state's capture is the old card**: `states.md`'s "Narrow: college admin" (`home--narrow-college-admin.png`, At Step A.7.10) in `design/baseline/states/` dates from 2026-09-27 and shows the three-link card and its old sentence; A.7.10's replay checked the new card at 390 but the capture was not re-taken. `home--college-admin.png` (1280) was re-taken on 2026-11-05.
- A College admin whose account names no College reads the same card, "Your College's …", and learns it only behind Specialities ("No College on your account") and EPAs. Inferred from the code; no step reaches it.
