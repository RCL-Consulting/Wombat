# MyCommitteeReviews

My committee reviews (`/committee/my-reviews`, `Pages/CommitteeDecisions/MyReviews.razor`; its words in `MyReviewWords.cs`): **the trainee's own list of committee reviews**, newest first, each opening her review's own page (MyReview). Flow 09 (T366, 2026-10-14, 49e7cf5e; boards R3-1; round 3 F1, M1, S8; T212, T329; build review D11) made it the list alone. Until then View opened a "Review detail" card under the list, on the same page, holding the decisions, the appeal form and the agenda; the window sat beside the period after a "·"; State was words, not a badge; ties on one day had no second order; and an appeal lodged there was not shown after the lodge (T308).

## Who reads it

The Trainee (`[Authorize(Roles = "Trainee,Administrator")]`): the list holds only the caller's own reviews (`ListReviewsForTraineeQuery`), so it admits an Administrator too but reads them nothing that is not their own. The Trainee's menu offers it (NavMenu); it owns her review's page.

## The page

- **The header**: "My committee reviews", the subtitle "Ratified decisions, appeal status, and final outcomes."; no action.
- **The list**: a hand-written `.clinic-table.clinic-table--stack.clinic-table--stack-wide` (S8; DataTable), captioned "Your committee reviews, newest first", `scope="col"` on its headers. **Newest first**: by scheduled date, then review number, both descending (F1), so one day's ties read the later review first.

| Column | What |
|---|---|
| Panel | the panel's name, "Paed Annual Review Panel" |
| Period | **the row's header** (`th scope="row"`, D11): "2026 S2", its evidence window under it, flow 08's `.period-window` (muted 0.9rem), "2026-01-01 to 2026-12-31", each date unbroken (T212) |
| Type | "Annual progression", "Pre-graduation", "Entrustment only", or "Formative check-in" for a formative review (`CommitteeDecisionWording.ReviewTypeShortLabel`) |
| State | the review page's badge (`ReviewSitting.StateBadge`; Badge): Ratified and Closed green, Under appeal amber |
| Decision | the category ("Satisfactory Progress", "Inadequate Progress — Additional Training", "Satisfactory with Observations"), "Entrustment decisions only", "Pending" before one is recorded, a check-in's "No binding decision", a withdrawn review's "None: the review was withdrawn" (`CommitteeDecisionWording.OutcomeLabel`) |
| (Actions) | **View** (`a.btn.btn-outline.btn-sm`) to `/committee/my-reviews/{id}` (`ReviewLinks.MyReview`), named by the review, "View the 2026 S2 review before Paed Annual Review Panel" (`ReviewRowNames`, T239), the type added where two rows share a name (", Pre-graduation", M1); the visible word is inside the name |

## States (T329)

- **Loading**: the header at once, an always-present visually hidden `role="status"` saying "Loading your committee reviews.", three row skeletons (3rem), no View until the read is in.
- **Load failed**: "Could not load your committee reviews. Nothing has changed. Try again, or come back in a few minutes." with Try again, which answers into the h1; never the exception's text, the failure to the log.
- **Empty**: "No decisions yet", "Ratified committee outcomes will appear here."

## Narrow widths

**Stacked at 900px and below** (both stack classes, S8): each row a block, its period and window heading the labelled cells (DataTable). No sideways scroll at 1280, 768 or 390 (the replay's 4.41, 4.51).

## Contrast

No new colour token. Body text 12.63:1 and the window (`muted-text`) 5.09:1 on the surface; the row header 11.36:1 on `header-bg`; View 4.86:1; the badges as Badge's.

## Known gaps

- None the replay met (4.41, 4.43, 4.48, 4.51).
- **Found re-syncing:** View stays the small button (28px) at every width. The 44px rules for stacked rows' actions are scoped to flow 08's wrapper (`.committee-calendar`) and flow 09's other two (`.my-authorisations`, `.entrustment-list`), and none reaches this list, whose page draws no wrapper of its own.
