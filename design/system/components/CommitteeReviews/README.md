# CommitteeReviews

Committee reviews (`/committee/reviews`, `Pages/CommitteeDecisions/ReviewsSchedule.razor`; its words in `ScheduleWords.cs`): **the list of committee reviews** the caller may open, newest scheduled first, each opening the review (ReviewSitting). Flow 08 (T364, 2026-10-13, 622c5ad1; boards R2-5; Q1; round 3 M8, F6, S7; C8, C10, C11; T338) made it the list alone. Until then the scheduling form opened above the list on the same page, the list had a Mode column, and a scheduled review's badge was blue here and grey on the review.

## Who reads it

Coordinator, Administrator, Institutional admin, both speciality admins and the Committee member (`[Authorize(Roles = …)]`); the list holds only the reviews the caller may open (`ListReviewsForPanelQuery`, T218). The menu offers it to each (NavMenu); it owns the review's page and the schedule page (ScheduleReview).

## The page

`div.committee-calendar`.

- **The header**: "Committee reviews", the subtitle "Schedule progression reviews and open existing ones." ("Open existing committee reviews." for someone who schedules none), and **Schedule a review** (`btn-primary`, `plus`), to `/committee/reviews/new`, in the page's own words (S7), only for a caller the scheduling command would admit at all (`GetCommitteeReviewsAccessQuery`, T216): not a Committee member, not someone who also holds Trainee; and only once that is read (C11).
- **The rule line** (F6), once above the list (`p.needs-you-rule`): "The College's semester 2 ends in November; its window runs to 31 December, so December's work counts."
- **The list**: a `.clinic-table--stack.clinic-table--stack-wide`, captioned "Committee reviews at Kgosi Kgari Teaching Hospital, newest scheduled first" (the institution only when every row is at one), of seven columns:

| Column | What |
|---|---|
| Trainee | the row header: the name, or an erased trainee's pseudonym as stored (`deleted_user_…`, flow 14) |
| Panel | the panel's name |
| Period | "2026 S2", its evidence window under it, `.period-window` (muted 0.9rem), "2026-01-01 to 2026-12-31", each date unbroken |
| Type | "Annual progression", "Pre-graduation", "Entrustment only", or **"Formative check-in"** for a formative review whatever its stored type (C8, T309 item 2); **no Mode column** |
| State | the review page's badge (`ReviewSitting.StateBadge`; Badge): **Scheduled grey**, In progress blue, Decided and Under appeal amber, Ratified and Closed (final) green, Withdrawn red (S7) |
| Decision | "Pending" until recorded, then the category ("Satisfactory Progress", "Inadequate Progress — Additional Training", "Outcome Deferred"), or "Entrustment decisions only"; a check-in's is "—", read as "None" (`aria-hidden` dash, `.visually-hidden` word); a withdrawn review's "None: the review was withdrawn", formative or not (T258) |
| (Actions) | Open (`btn-outline btn-sm`), named for the review it opens, "Open the 2026 S2 review of Lerato Molefe before Paed Annual Review Panel" (T239), with the type, the day it was scheduled and the review's number added where two names are alike |

## States (C11, T338)

- **Loading**: the header at once (no action yet), the status "Loading Committee reviews.", skeletons.
- **Load failed**: "Could not load Committee reviews. Nothing has changed. Try again, or come back in a few minutes." with Try again; never the exception's text.
- **Empty**: "No reviews yet", with "The reviews you can open appear here once they are scheduled. Use Schedule a review to put a trainee before a panel." for a scheduler, or "The reviews of the panels you sit on appear here once they are scheduled." for a Committee member; someone who holds Trainee reads "No reviews to show", the trainee note, and "Open My committee reviews".

## Narrow widths

**Stacked at 900px and below** (`.clinic-table--stack-wide`, M8; DataTable), each row its trainee over the labelled cells, Open 44px there; below 641px Open is the row's width and Schedule a review is 44px and the header's width.

## Contrast

No new colour token. Body text 12.63:1 and the window (`muted-text`) 5.09:1 on the surface; the row header 11.36:1 on `header-bg`; the rule line (`muted-text`) 4.83:1 on the page; Scheduled's words 11.36:1 on `header-bg`, its edge 3.42:1; Open 4.86:1.

## Known gaps

- None the replay met on this page. My committee reviews, the trainee's own list, is flow 09's, still pre-restructure.
