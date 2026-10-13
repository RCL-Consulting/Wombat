# DecisionsDue

Decisions due (`/committee/decisions-due`, `Pages/CommitteeDecisions/DecisionsDue.razor`; every sentence, address and action in `DecisionsDueText.cs`): **what the committee must decide, per registrar and per EPA, and where each decision stands**, so a scheduler can put each one before a panel. Flow 08 (T364, 2026-10-13, 622c5ad1, with 1ec7a352 from its replay; boards R2-3; round 3 F1, F2, F6, M2–M5, S4, S7; C2, C3, C9, C11; T259, T264, T280, T329, T338) rebuilt it. Until then it was a filter form acting on change, a counts table always open above the list, a Scheduled badge in blue, and rows offering Schedule beside Open review.

## Who reads it

Coordinator, Administrator, Institutional admin and both speciality admins (`[Authorize(Roles = …)]`), the roles that schedule reviews; not the Committee member. The menu offers it to each (NavMenu). The page lists, per current trainee the caller oversees at their institution, each EPA due for an entrustment decision in a period, so every Schedule it offers is one the scheduling handler accepts. A caller who also holds Trainee is shown nobody's decisions (T185): the empty card.

## The page, in order

`div.committee-calendar`.

- **The header**: "Decisions due", the subtitle "What the committee must decide, per registrar and per EPA, and where each decision stands." (no "each semester": the annual EPAs are decided once a year, S4).
- **The opening** (`p#due-intro.muted`): "The EPAs due for an entrustment decision in 2026 S2 for the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda." An Administrator reads "An administrator belongs to no institution, so the decisions due are shown for the one chosen above."
- **The filters, applied with Show** (FilterBar; F2): `form.search-container[method=get][aria-label="Filter Decisions due"]`: Institution (an Administrator's only, "Select institution"), **Period** (the six `CommitteeReviewPeriods.Around` offers, "2026 S2 · 1 Jul to 31 Dec 2026", the current one by default, with its help `small.field-help` "The College's semester 2 ends in November; its window runs to 31 December, so December's work counts.", F6), **Trainee** ("Every trainee", then the current trainees by surname, Q2), **Status** ("Outstanding: not yet decided" by default, "Every status", then each status: Missed, Not scheduled, Revoked: re-decide, Due by year end, Not decided, Scheduled, Partial period, As opportunity allows, Decided) and **EPA** ("Every EPA", then "PAED-001 — Providing paediatric emergency care to children" …); Show, and Clear filters once Trainee, Status or EPA is set. The address carries them (`?institution=&period=2026-1&trainee=&status=&epa=`): Institution and Period choose what is read, the other three narrow it, and Clear filters keeps the first two.
- **By EPA** (`section.list-section`, `h2.list-section-title` "By EPA", then "The filters above do not narrow these counts." `.muted`): `.rung-legend`'s fold, **shut by default** (M3), its summary "Counts for the 15 EPAs due in 2026 S2" with the `chevron-right` marker and Show / Hide, 44px at every width. Inside it a focusable scroll region (`.table-container[tabindex=0][role=region]`, "By EPA, 2026 S2, scrollable"; `.rung-legend > .table-container`, set in 12px from the fold's frame) holds an eight-column stack-wide table: EPA (the row header), Due, Decided, Scheduled, Not decided, To schedule, Missed and Optional, captioned "How many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked. Optional: a partial period, or decided as opportunity allows."
- **Decisions** (`section.list-section`): the heading `h2#due-list-heading` (`tabindex="-1"`) says the page while there is more than one, "Decisions, page 2 of 4" (`span.list-section-page`, muted, weight 400), and **takes the focus after Show and after a page turn** (C9, T264). Under it the count, `p#due-count.list-section-count` (`role="status"`, muted 0.9rem): "75 of 75 decisions due in 2026 S2 shown.", "15 of 75 decisions due in 2026 S2 shown: Lerato Molefe, every status.", "5 of 75 … shown: Not scheduled, PAED-004.".
- **The trainee line** (`.trainee-line`, C2), above the list while one trainee is filtered: a raised card of one line, its sentence then its one link (`btn-outline btn-sm`), one line for each panel the trainee's rows name for Schedule, read by the scheduling handler's own seat rule:
  - held: "Review #1 (annual progression) holds 2026 S2 before Paed Annual Review Panel, scheduled on 2026-10-13." with "Open review #1" (named "Open review #1 of Lerato Molefe");
  - free: "No open review holds 2026 S2." with "Schedule a review" (named "Schedule a review of Lerato Molefe for 2026 S2 before Paed Annual Review Panel"); beside a second panel's line each names its panel ("No open review before Neonatal CCC holds 2026 S2." with "Schedule a review before Neonatal CCC");
  - each open formative check-in: "Formative check-in #6 is open: it takes no seat." with "Open check-in #6" (named "Open check-in #6 of Sipho Ndlovu").
- **The list**: a `.clinic-table--stack` (stacked below 641px) of five columns: EPA (the row header, "PAED-001 — Providing paediatric emergency care to children"), Trainee (left out while one trainee is filtered), Window ("2026 S2", or "2026" for an annual EPA), Status, and a visually hidden "Actions". Rows by trainee surname, then EPA code. Captioned "Decisions due in 2026 S2, by trainee then EPA"; "Decisions due in 2026 S2: Not scheduled, PAED-004"; "Lerato Molefe's decisions due in 2026 S2, every status, by EPA". `PagerControls` under it, 20 rows to start, its select labelled "Decisions per page" and the pager named "Decisions, pages".
- **The status cell** (`.status-cell`): the badge over a sentence (0.9rem) saying what it means, the review it names a link where the caller may open it (C3), its link padded to a 24px target (`.status-cell p a`, 1ec7a352; it was 19px):

| Status | Badge (Badge) | Its sentence |
|---|---|---|
| Decided | `badge-completed` | "STAR #12, issued at review #1." |
| Scheduled | `badge-draft`, grey (S7) | "On the agenda of review #1 before Paed Annual Review Panel, which is still open." (T259) |
| Not decided | `badge-accepted` | "Not decided at review #30. A later sitting for 2026 S2 plans it again." |
| Revoked: re-decide | `badge-declined` | "STAR #14, issued for 2026 S1, was revoked. Schedule a review to decide it again." |
| Due by year end | `badge-draft` | "Decided once a year: due by the last sitting of 2026, and not missed before the year ends." |
| Not scheduled | `badge-draft` | "No open review holds it, and 2026 S2 has not ended." |
| Missed | `badge-declined` | "2026 S1 has ended with nothing decided or on an open review's agenda." |
| Partial period / As opportunity allows | `badge-draft` | "The trainee joined the window part-way through: optional, never missed." / "Decided as opportunity allows: optional, never missed." |

  A row whose seat an open review already holds adds "Review #1 is open for 2026 S2 before Paed Annual Review Panel: decide it there, or ratify that review before scheduling another."
- **One action a row** (C3; DataTable): Decided, "Decided: nothing to schedule" (`.muted`); Scheduled, "Open review #1" (named "Open review #1 for Lerato Molefe, PAED-001"); a row whose seat an open review holds, "Open review #1" for that review, never Schedule, which would be refused; any other row **Schedule**, to the schedule page filled with the panel the EPA routes to, the trainee, **the row's own period, an ended one included** (F1), and this page's address to come back to (`ReviewLinks.Schedule`), named "Schedule a review of Anele Dlamini for PAED-001 before Paed Annual Review Panel" (", for 2026 S1" once the period has ended). Where none can be offered the cell says why: "No panel you can schedule before decides it.", "Review #1 holds it: nothing to schedule". There is no deferral (D53).

## States (C11, T329, T338)

- **Loading**: the header, the form and both section headings at once, skeletons under them (`aria-busy`), a status line "Loading Decisions due.", and no Schedule or Open review.
- **Load failed**: "Could not load Decisions due. Nothing has changed. Try again, or come back in a few minutes." with Try again, whose answer focuses the Decisions heading; a handler's own refusal still shows in its words (S4).
- **No match**: the count, then the card "No decision matches these filters.", what was asked ("Missed only, every trainee, every EPA."), and Clear filters.
- **Nothing due**: "Nothing due for 2026 S2" · "No trainee you oversee at Kgosi Kgari Teaching Hospital owes an entrustment decision in 2026 S2.", the form still standing so another period can be read.
- **An Administrator before Show**: `#due-choose-institution`, "Choose an institution" · "Choose an institution above, then Show, to read what its committees must decide."; a failed institution list, "Could not load the institutions to choose from. …" with Try again.

## Narrow widths

Below 641px the list stacks (each row's EPA over its labelled cells), every form control and every row action is 44px and the row's width, and the trainee line's link is a 44px row of its own. By EPA stacks at 900px and below.

## Contrast

No new colour token. The status sentence 12.63:1 and its link 6.70:1 on the surface; the count and the page in the heading (`muted-text`) 4.83:1 on the page; the trainee line's words 12.63:1 on the surface; the badges as Badge's (Scheduled now the neutral's, words 11.36:1, edge 3.42:1).

## Known gaps

- **T365**: an S1 sitting's agenda labels an EPA decided as opportunity allows "Due by year end" (the schedule page's Agenda, ScheduleReview; the replay's 4.2), where this page reads "As opportunity allows" for the same period.
