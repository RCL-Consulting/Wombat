# InstitutionalAdminHome

The Institutional admin's Home (`Pages/Dashboards/InstitutionalAdminDashboard.razor`; its words in `InstitutionalAdminHomeWords.cs`, its read `GetInstitutionalAdminDashboardSummaryQuery`): **her institution's people, what it has adopted, and its record**, each card's footer its way on. Flow 12 (T376, 2026-10-23, e53d4ae7; boards R3-H01 to H05; Q3, Q7, Q8, S18, S19, S23, M7, K24; T291 items 4 and 8) rebuilt it. Until then it counted the College's whole catalogue rather than what she had adopted, showed role keys, had a Quick links card (Entrustment decisions moved to her menu, NavMenu), and a `settings` card for her institution.

## The page

Home's header as every role's ("Home", "Institutional admin · Semester 2, 2026"), with **Invite a person** as its action (flow 01). Then:

- **The waiting line, only while someone waits** (Q8): above the cards, an info Alert with no role (standing content, not a live region, S23), `#home-waiting-line`, an `.alert-row`: "**5 registrars are waiting to be admitted.** Nothing can be filed about them until then." ("1 registrar is …") and **Open Trainees** (`arrow-right`). Flow 06's oversight line is the precedent.
- **The cards** in `.dashboard-grid`, each a DashboardCard named by its h2 (`HeadingId`):
  - **People** (`users`, `#home-card-people`): each role held at her institution as **one link** (S19), `a.dashboard-metric` in `.dashboard-metric-row`, the count (`.dashboard-metric-value`) and its label (`.dashboard-metric-label`, underlined in `link-color`, `link-hover` on hover), the count inside the link's name, to Users filtered by that role (`PeopleLinks.UsersFiltered(role:)`). The footer: the note on the left, `.dashboard-card-footer .field-help`, the distinct accounts, those with no role included ("14 accounts.", "1 account: yours."), and **Open Users**.
  - **Curriculum** (`book`, `#home-card-curriculum`): the adopted specialities and sub-specialities as two counts ("speciality adopted", "sub-specialities adopted"), each adoption's line ("Paediatrics / Paediatrics, from the CPSA's Paediatric EPA Curriculum (11.2).") or "Nothing is adopted yet. Adopt the College's curriculum to set up a programme.", and **Open Curriculum adoptions**.
  - **Your institution** (`building-2`, `#home-card-institution`): `dl.detail-list.home-institution-list` of Name, Short code, Contact email (`<wbr>` after its @; "None recorded") and Status ("Active" over "Set by a Wombat administrator."), and **Open ‹institution›** to her record (InstitutionRecord). The list wraps inside the card (M7): `.dashboard-grid .home-institution-list` is `minmax(0, max-content) minmax(0, 1fr)`, its `dd` breaking anywhere; the Trainee's lists keep `.detail-list`'s own columns (build review P8).
- **No Quick links** (Q7): each card's footer is its way on.
- One read for the page (DashboardFrame): its loading and its failure, "**Could not load your Home.** Nothing has changed. …", are Home's.

## Narrow widths

The grid goes to one column at 900px and below. Below 641px the metric links are 44px (`.dashboard-grid a.dashboard-metric`), the footers wrap and their links are 44px. Measured on the replay at 1280, 1000 and 390 (T376 step 8): no sideways scroll; the metric labels wrap onto a second line at 1000 ("Sub-speciality admin").

## Contrast

No new colour token. The metric labels `link-color` on the surface; the footer's note `muted-text` 5.09:1.
