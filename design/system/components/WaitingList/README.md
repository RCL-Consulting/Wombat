# WaitingList

Waiting for you (`Components/Shared/Activities/WaitingList.razor`): what waits on the reader as someone else's assessor, supervisor or reviewer, each row its link, its state's badge with "Overdue" beside it, and how long it has waited. It is NeedsYouList's row with an overdue edge, drawn by the same classes, on the Assessor's Home, in the activity page's way on (WayOn), and, as a table of the same rows and words, in the Activity inbox. Flow 04 (T350, 2026-09-30, 06aa51d7; C6, R1, R2) made it; until then the Assessor's Home listed "Waiting for your rating", ten rows each its type and its trainee, with "Overdue" in place of the state's badge.

## What the consumer provides

`<WaitingList Items="@summary.Waiting.Items.Take(5).ToList()" Names="LinkNames" />`

- `Items` (required): `ListWaitingForYouQuery`'s rows (`WaitingForYouDto.Items`), oldest first by `UpdatedOn`: every activity with a move that leads on which the reader may make now by an arm that is not the author's, less the reader's own work. Home, the inbox, the way on and the other-role line all read it through one reader (`WaitingForYou.ReadAsync`), so none can disagree with another (T297).
- `Names`: the links' accessible names over the whole page (`AssessorRowNames.For`), where the page lists these rows among more: Home passes its first five of the inbox's rows. Null: named among these rows (`ActivityRowNames.Waiting`).
- `WithSince`: each row says since when it has waited, "Waiting 8 days, since 2026-09-22 08:06 SAST." (the way on, which shows one row with no Waiting column beside it).
- `LinksOutOfTabOrder`: the rows' links take `tabindex="-1"` (the way on, whose "Open the next" opens the same activity and is the next Tab after the result; C10 d).
- `WithNominee` (flow 06, T358, E6): **the staff reading**. Each row says whom it waits with, "With Mohammed Patel" (`WaitingWords.With`), on a line of its own (`p.needs-you-why`) above flow 04's "Waiting 8 days", which is unchanged: Home's Waiting for assessors card (AssessorsWaitingCard) and the registrar page's section.
- `RowAction` (flow 06): what a row offers after its why-lines, inside the row: the registrar page's Send a reminder (ReminderAction). Null: none.

Without `WithNominee` or `RowAction` every row is byte for byte flow 04's. The staff reading's rows are `ListWaitingForAssessorsQuery`'s: requests in the reader's programme waiting for one named person, the reader's own left out (WaitingForAssessors).

## Markup

`ul.needs-you.stack-list` of `li.needs-you-row`, as NeedsYouList's (a hairline frame, 8px by 16px, `radius-md`), with no emphasis class: a row that is overdue adds `.needs-you-row--overdue`, which puts `warning-color` on its 4px left edge. In each row:

- **The link** (`ActivityLink` with `FromSubject`): the activity's name, "Type · EPA · date" ("Mini-CEX (Paediatrics) · PAED-004 · 2026-09-30"), then under it (`span.activity-link-to`) whose work it is, "from Nomsa Mahlangu" (`ActivityListWords.FromLine`), in place of the nominee, who here is the reader. A visually hidden ", " stands between the lines. A long name wraps inside the row at 390px (`overflow-wrap: anywhere`) rather than push the badges off it.
- **The badges** (`span.needs-you-badges`, 4px apart, wrapping): the state's badge (`BadgeFor.ActivityState(key, isFinished: false)`: "Requested", "Awaiting review", "Awaiting discussion"), then, when overdue, `span.badge.badge-overdue` "Overdue" **beside** it, never in its place.
- **The wait** (`p.needs-you-why`): "Waiting less than a day", "Waiting 1 day", "Waiting 8 days" (`WaitingWords.Waited`); with `WithSince`, "Waiting 8 days, since 2026-09-22 08:06 SAST." Whole days, rounded down, from `UpdatedOn`, the one clock (note 10).

**Overdue** is a wait of at least `AssessorDueDays` (7) × 24 hours. The rule line over a list says so, its number read from the setting: "Oldest first. Overdue once it has waited 7 days." (`p.needs-you-rule`, `WaitingWords.RuleLine`).

**Two links that would read the same** add to their accessible name, never their visible words: the state's label, then ", waiting since 2026-09-22 08:06 SAST" (", decided … SAST" on a decided row), then "(1 of 2)", one naming over every assessor list on the page (`ActivityRowNames.Waiting`, `AssessorRowNames`; C11). A tie-breaker that says the same of every alike row is skipped.

## On Home

The Assessor's DashboardCard "Waiting for you" (`inbox`, `#card-waiting`, spanning two): its count as the title's badge in words, "2 waiting, 1 overdue" (`WaitingWords.Count`, DashboardCard's `BadgeWords`), and no badge when nothing waits; the rule line; the first five rows; past five, "20 more wait in the Activity inbox." (`p.waiting-more`, muted 0.9rem); and its foot "Open Activity inbox". Its stripe is the emphasis one, or the warning one in its place when any row is overdue. Empty: "Nothing is waiting for you." (`p.card-empty`). Beside it, "Recent decisions" (DashboardCard).

## In the Activity inbox

`/activities/inbox`, headed "Activity inbox", "What waits for you to rate, review or discuss.", with no header action. Two `section.list-section`s:

- **Waiting for you** (`h2#waiting-h.list-section-title`, `tabindex="-1"`): the count badge ("1 waiting, 1 overdue"), the rule line, then every row as a stacked DataTable of **Activity, EPA, State, Waiting**. The Activity cell is the same `ActivityLink`; the EPA cell (`td.epa-cell`, at most 22rem, wrapping) is "PAED-015 — Teaching and applying evidence-based care …", with "(no longer in use)" muted beside a paused EPA; the State cell the same `.needs-you-badges`; the Waiting cell (`td.waited-cell`) "8 days" in bold over "since 2026-09-25 17:22 SAST" in muted 0.85rem (`WaitingWords.WaitedCell`, `Since`). Only EPA and Waiting carry `data-label`. Empty: a `.detail-card--empty`, "Inbox clear" · "Nothing is waiting for you."
- **Decided by you** (`h2#decided-h`, `tabindex="-1"`, the target of Home's "All your decisions"): a grey badge "3 decisions" (`WaitingWords.DecidedCount`), the rule line "Newest first. Everything you completed, declined, discussed or signed off.", then a stacked table of **Activity, Decision, Decided, Credit**: the link with "from …", the decision's badge (`BadgeFor.ActivityState(key, isFinished)`: green when the pinned workflow is finished, red when declined), the moment in SAST ("2026-10-03 19:13 SAST"), and `CreditOutcome.Label` ("1 item", "None", "—"). `ListDecidedByYouQuery`, newest first, 20 a page under `PagerControls`, labelled "Decided by you, pages"; a page turn puts the focus on the section's heading. Empty: "No decisions yet."
- **Loading:** each section's heading over skeletons, and a visually hidden status, always on the page, "Loading the Activity inbox.". **Load error:** StatePanel's alert, "Could not load the Activity inbox. Nothing has changed. Try again, or come back in a few minutes.", with Try again; its answer puts the focus on "Waiting for you".
- **A registrar's inbox** is empty by design and says so: "Nothing here is yours to act on." · "This inbox holds work that assessors rate, discuss or review. Your drafts and work returned to you are under My activities, in Needs you." with Open My activities (NeedsYouList).

Below 641px both tables stack, a block a row (DataTable's `Stack`); the page no longer scrolls sideways.

## Rules (DESIGN.md § Page-level patterns, "List page" R2; § Dashboard page R1; § Badges)

- One read, one order, one set of words, wherever a waiting row is drawn; the staff reading adds only "With <name>" (E6), in place of nothing.
- "Overdue" is said in words beside the state, never instead of it, and never by the stripe alone.
- Times are South African with the zone; never the server's clock.

## Contrast

The link `link-color` 6.70:1 on the surface; "from …", the wait's "since" line and the rule `muted-text` 5.09:1; the overdue stripe `warning-color` 5.77:1 on the surface; "Overdue" `text-color` on `warning-bg` 11.89:1, its edge 5.43:1.

## Known gaps

- **The clock restarts on the assessor's own save** (T351): the wait is measured from `UpdatedOn`, which Save draft sets, so an assessor who saves a half-rated request seven days old turns "Waiting 7 days, Overdue" into "Waiting less than a day" and drops it to the bottom; the status card still dates the move.
- Every reader reads the whole list: Home for five rows, the way on and the other-role line for a first row and a count (T351).
- A registrar with no directory entry reads "from <user id>", with no test deciding the words (T352).
- The inbox's loading status is filled at the first render, so a screen reader may not announce it (T352).
