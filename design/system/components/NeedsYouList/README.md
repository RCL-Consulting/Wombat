# NeedsYouList

Needs you (`Components/Shared/Activities/NeedsYouList.razor`): the registrar's own drafts and the work returned to them, each its link, its state's badge and why it is here. The same component, the same read (`ListNeedsYouQuery`) and the same words on Home's Needs you card and at the head of My activities, so the two can never say different things of one row (C11; T297's rule). Flow 03 (T342, 2026-09-29, 725237ee; E8) made it; until then the registrar's Home showed an "Activity inbox" card that listed the inbox's rows. `ActivityLink`, the row's link, is described here too.

## What the consumer provides

`<NeedsYouList Items="_needsYou" Names="_names" />`

- `Items` (required): `ListNeedsYouQuery`'s rows, most recently updated first. Home passes the first five (`NeedsYouListed`) and counts them all.
- `Names`: the links' accessible names over the whole page (`ActivityRowNames.Links`), where the page lists these rows beside others; My activities passes them so a row in both lists reads the same in each. Null: named among these rows.

## Markup

`ul.needs-you.stack-list` (8px apart, no bullets) of `li.needs-you-row.detail-card--emphasis`: a hairline frame with the emphasis stripe (4px `secondary-color`), `radius-md`, 8px by 16px, wrapping:

- **The link** (`ActivityLink`, `a.activity-link`, weight 600, `flex: 1 1 16rem`): the activity's name, "Type · EPA · date" ("Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20"; "· no date" when none is set, "· no EPA yet · no date yet" when neither is; E9), and on a second line (`span.activity-link-to`, muted 0.9rem, weight 400) whom it goes to or is talked over with: "to David Naidoo" for work rated by its nominee, "with Sarah Botha" for work discussed or reviewed with them (the type's shape). No second line with no nominee, or when the name already ends with them: two activities sharing type, EPA and date each add their nominee to the name (E7), "… · 2026-09-10 · Sarah Botha". A visually hidden ", " stands between the lines, so the link reads "Mini-CEX (Paediatrics) · PAED-001 · 2026-09-20, to David Naidoo". Where two links on the page would still read the same, an `aria-label` adds the state, then "(1 of 2)" (T280). **On an assessor's list** (`FromSubject`, flow 04) the second line is whose work it is, "from Anele Dlamini" (`ActivityListWords.FromLine`), in place of the nominee, who there is the reader; and `OutOfTabOrder` takes the link out of the tab order (`tabindex="-1"`: the way on's row, beside "Open the next"). See WaitingList.
- **The badge**, the state's label: every row here still has a move that leads on.
- **Why it is here** (`p.needs-you-why`, the row's full width): "Returned to you by Sarah Botha on 2026-09-30. Change it and submit again." or "Not submitted yet. It is in nobody's inbox until you submit it." (`ActivityListWords.NeedsYouWhy`).

## On My activities

A `section.detail-card.list-section` before All activities, only when it holds a row: `h2.list-section-title` "Needs you" (focusable, `tabindex="-1"`) with its count as a badge to the eye and words to a screen reader (`aria-hidden` on the badge, then a visually hidden ", 2 items"; A16); the rule line (`p.needs-you-rule`, muted 0.9rem): "Your drafts, and work returned to you. A request you can still cancel is with its assessor, so it is not here: it is under All activities."; then the list. My activities' header is "My activities", "Everything you have filed or logged, newest encounter first.", with Log an activity; All activities is a stacked DataTable (see DataTable).

## On Home

The Trainee's DashboardCard "Needs you" (`inbox` icon), its count a badge read as words (`CountWords`, "2 items"); empty: "Nothing needs you. Requests you have filed are in My activities."; its foot "Open My activities" (`/activities/mine`). See DashboardCard.

## Rules (DESIGN.md § Page-level patterns, "List page"; § Dashboard page)

- **A draft is not private** (C2): it is "in nobody's inbox", never "nobody can see it".
- A request the registrar can still cancel is not here: it is with its assessor.
- A declined request is not here: nothing more can happen to it (T297).
- A registrar's Activity inbox is empty by design (its query leaves out the author's own work): it says so and sends them here, "Nothing here is yours to act on." · "This inbox holds work that assessors rate, discuss or review. Your drafts and work returned to you are under My activities, in Needs you.", with Open My activities.

## Contrast

The link `link-color` 6.70:1 on the surface; its second line and the rule `muted-text` 5.09:1; the stripe `secondary-color` 4.86:1.

## Known gaps

- A draft of a type whose move is Log still says "until you submit it" (T348).
- My activities reads every row to serve one page (E7's names need them all); paging and the collision keys belong in the query (T343). An assessor may see an activity's name without the nominee its registrar sees (T345).
