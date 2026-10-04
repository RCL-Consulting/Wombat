# DashboardCard

A titled `.detail-card` for Home's role dashboards, with an optional Lucide icon, count badge, link, stripe and column span, laid out in the shared three-track `.dashboard-grid` inside a DashboardFrame that draws the loading and the load-error states.

## What the consumer provides

```razor
<DashboardFrame IsLoading="IsLoading" Failed="Failed" OnRetry="LoadAsync">
  <DashboardCard Title="Waiting for you" Icon="inbox" HeadingId="card-waiting" Span="2" IsLoading="IsLoading"
                 Emphasis="true" Warning="@(Summary?.Waiting.OverdueCount > 0)"
                 BadgeWords="@(Summary is { Waiting.Count: > 0 } counted ? WaitingWords.Count(counted.Waiting) : null)">
    @if (Summary is { } summary) { … }
  </DashboardCard>
</DashboardFrame>
```

- `Title` (required): an `h2.dashboard-card-title` under Home's h1, with the icon at 18px before it.
- `Icon`: a Lucide name from the Icons group.
- `Count`: a number shown after the title as a `badge-submitted` ("Needs you 2"); not while loading.
- `CountWords` (flow 03, T342, A16): the count in words, which a screen reader hears in place of the figure: the badge is then `aria-hidden` and a visually hidden ", 2 items" follows it. The Trainee's Needs you passes it (`ActivityListWords.ItemCount`). Null reads the figure.
- `BadgeWords` (flow 04, T350, note 13): the count in words **as** the badge, to the eye and a screen reader alike, "2 waiting, 1 overdue" (`badge-submitted`). It wins over `Count`. Null or empty shows no badge, so a card with nothing in it carries no "0". Not while loading.
- `HeadingId` (flow 04): the title's id; the card is then a `section` named by it (`aria-labelledby`), so a screen reader lists it as a region: the Assessor's `#card-waiting` and `#card-decisions`; the Trainee's `#card-targets`, `#card-decisions` and `#card-stars` (flow 05). Null: a plain `div` card.
- `Href`: the whole card becomes an `<a>` with `.detail-card--interactive` (it lifts 2px on hover); not while loading. Since flow 05 no dashboard passes it: the Trainee's Curriculum targets card was the last linked card, and Your targets, which replaced it, holds links of its own (T280: a card is never one link around other links).
- `Emphasis` / `Warning`: the 4px `secondary-color` or `warning-color` stripe, one per card: with both, the card carries `.detail-card--warning` in place of `--emphasis`, never both classes (flow 04, nit T15).
- `Span`: 2 or 3 (`.dashboard-span-2`, `-3`).
- `IsLoading`: the title over a `.dashboard-card-skeleton` (three skeleton lines, `aria-hidden`), and not the content.
- The body, in `.dashboard-card-body`, composed from: `.list-row` (a row of a list: its label left, a badge or a date right), `.dashboard-metric` (a `.dashboard-metric-value`, 2rem/700 `primary-color`, over a `.dashboard-metric-label`), `.dashboard-metric-row` (figures side by side), `.progress-row` (`.progress-row-head`: label left, "n of m" right; a `.progress-bar` and its `.progress-bar-fill`, `.is-complete` in `success-color`; a `.progress-row-meta` line), **`.progress-row--link`** (flow 05: the row is its link, `a.progress-row-link`, weight 600, with the bar, at most 16rem, and the `.progress-row-meta` line under the name, never beside it; ruled between rows), `.progress-group-label` (an `h3` over such a list, muted 0.9rem/600: "Furthest short"), `.targets-line` and `.targets-note` (a card's opening line and its notices), `.stars-summary` (My authorisations' summary line), `.decided-note` (one line under a decided row, in `text-color` 0.9rem, the row's full width), `.status-dot` rows, and a `.dashboard-card-footer` for a link button ("Open Activity inbox", "Open My progress", "Open My activities"); a card's empty words are `p.card-empty` (muted). A card may also hold a component's rows: the Trainee's Needs you draws `NeedsYouList`, the Assessor's Waiting for you `WaitingList`. Below 641px a footer's button is 44px.

## DashboardFrame (Components/Shared/DashboardFrame.razor)

Every dashboard that reads draws its cards inside one, in an `ActionResult`:

- **Loading:** the grid, `aria-busy="true"`, and every card's title at once over its skeleton. Nothing is offered until the read returns. A visually hidden status says "Loading your Home." (flow 04, C11): `p[role="status"]`, on the page from the first render and empty once the read has answered.
- **Failed:** one `danger` Alert with fixed words and an `.alert-row`: "**Could not load your Home.** Nothing has changed. Try again, or come back in a few minutes.", and Try again (`.btn-outline .btn-sm`, refresh-cw), which reads again. **No cards**: a card drawn empty would say there is nothing, and a skeleton that it is still coming. The failure's own text goes to the log. Once Try again answers, the region takes the focus. Below 641px Try again is 44px.
- **Loaded:** the grid and its cards.

## The grid (DESIGN.md § Dashboard layout grid)

- `.dashboard-grid`: three `minmax(0, 1fr)` tracks from 1100px, two from 901px, one at 900px and below, gap `space-lg`, `align-items: start`. At 1280px the content column is 974px, so three tracks of about 309px. A card spanning three takes the whole row at every width; spans reset to one track at 900px and below.
- A card's own content never widens its track (`min-width: 0`): a wide table scrolls inside it.

## Rules (DESIGN.md § Dashboard page)

- Home's header is the page's only one: "Home", "{acting role} · Semester N, YYYY", and one action for the Trainee ("Log an activity") and the Institutional admin ("Invite a person"). Dashboards add no header.
- **Home with no role** (a graduate, or an account whose last role was removed): no subtitle, no header action and no dashboard, but one `Span="3"` `Emphasis` card in a `.dashboard-grid`. A graduate holding the `trainee_record` claim gets "Your training record" (`book`): "You hold no role at the moment. Your training record is kept, read-only: your progress in each period, and your portfolio to export.", with "Open My progress →" (`/portfolio/progress`, `.btn-sm .btn-outline`). Anyone else gets "No role assigned" (`info`): "Your account holds no role at the moment, so there is nothing to show you here. An administrator can give you one.", with no link. It is the graduate's landing page, which flow 13 designs.
- Every link on a dashboard opens a page that admits the dashboard's role.
- A figure is a count against a target for a named window: "1 of 3 this semester", "1 of 2 in 2026". Never a lifetime total or a mean percentage. The Trainee's two figures are My progress's (flow 05, R3; `ProgressWords.SemesterFigure`, `YearFigure`): "1 of 10" over "EPAs met this semester", "0 of 6" over "EPAs met in 2026". Until flow 05 they read "2 / 5" over "semester targets met".
- Every `.progress-bar` has `role="progressbar"`, `aria-valuemin`, `aria-valuemax`, `aria-valuenow` and an `aria-label` that states the figure in words.
- Inline `style` only for a per-instance value (a bar's width); a row is `.list-row`, a row of buttons `.actions-cell`, figures side by side `.dashboard-metric-row`.
- The cards themselves belong to later flows (S20); flow 01 dropped only the duplicates (the Assessor's three ways to one inbox, the Administrator's Maintenance card, the Trainee's Actions card). Flow 04 redrew the Assessor's Home and flow 05 the Trainee's; every other role's cards are as they were.
- **The Assessor's Home is two cards** (flow 04, T350, R1), each the Activity inbox's own read, so a card and the section it opens cannot disagree (T297):
  - **"Waiting for you"** (`inbox`, `#card-waiting`, spanning two): the inbox's first five waiting rows (`WaitingList`), the count as the title's badge in words ("2 waiting, 1 overdue"; none when nothing waits), the rule line "Oldest first. Overdue once it has waited 7 days.", past five "20 more wait in the Activity inbox." (`p.waiting-more`), its foot "Open Activity inbox"; the warning stripe in place of the emphasis one when any row is overdue; empty, "Nothing is waiting for you.". See WaitingList.
  - **"Recent decisions"** (`history`, `#card-decisions`): the five newest decisions (`ListDecidedByYouQuery`'s first page), `ul.decided-list` of `li.decided-row` (a hairline between rows): the link by its full name with "from <registrar>" (`ActivityLink`, `FromSubject`) on the left, and on the right `span.decided-meta`, the decision's badge (green when the pinned workflow is finished, red when declined) over its South African day (`span.decided-date`, muted 0.85rem); below 641px the badge and the day sit on one line under the link. Its foot "All your decisions" (`/activities/inbox#decided-h`); empty, "No decisions yet." with no foot.
  Until flow 04 the first card was "Waiting for your rating", ten rows each its type and its trainee, with "Overdue" in place of the state's badge, and "Open inbox →"; the second listed type and trainee only, under `check`.
- **Another role's Home** may carry the other-role line above its dashboard (OtherRoleLine, flow 04), for someone who holds Assessor and acts in another role, when work waits.
- **The Trainee's Needs you** (flow 03, T342, E8) replaced the Trainee's "Activity inbox" card, which listed the inbox's rows (since T342 the inbox leaves out the author's own work, so a registrar's is always empty): exactly My activities' Needs you, the same `NeedsYouList` rows in the same words, read by the same code (`ListNeedsYouQuery`). It lists the first five and counts them all, its count a badge read as words ("2 items"); empty, "Nothing needs you. Requests you have filed are in My activities."; its foot "Open My activities" (`/activities/mine`), a `.btn-sm .btn-outline` since flow 05 (build review A2: the sibling footers' 44px at phone width), Home's one way to My activities. No `Href`: its rows are links.
- **The Trainee's Home is four cards** (flow 05, T355, R1; Q2, Q3), in this order, one read behind DashboardFrame, nothing moving the focus on load:
  - **"Your targets"** (`target`, `#card-targets`, spanning two, the emphasis stripe): the training year (`p.targets-line`, "Training year 3 — it sets the minimum level each encounter is judged against."; none before the programme starts or once it has ended), the two figures, the card's notices (`p.targets-note`: no curriculum, ended, not started, no EPA in use, every target met, started part-way), then **"Furthest short"** (`h3.progress-group-label`): the five items whose current window holds a target not yet met, largest shortfall first, then by code, each a `li.progress-row.progress-row--link` whose `a.progress-row-link` ("PAED-003 — Providing intensive care to children") opens its EPA's page, with its bar (`aria-label` "PAED-003: 0 of 3 this semester") and "0 of 3 this semester · 3 more by 2026-11-30" under it. Its foot "Open My progress" (`/portfolio/progress`).
  - **"Needs you"**, as above.
  - **"Recent decisions"** (`history`, `#card-decisions`, spanning two; B1, E6): what someone else decided on her own requests, newest first, five, no foot; flow 04's `decided-row`s (the link with its "to <nominee>" line, the decision's badge and its South African day), then one line by kind (`p.decided-note`, `CountLineWords.ForDecision`): the count it made ("PAED-001: 3 of 3 this semester, met."), "Credits nothing.", "Its credit to PAED-012 waits while the EPA is paused.", or, for a declined request alone, "File it again, to someone else" (a link named for its row, "File it again, to someone else: Mini-CEX (Paediatrics) · PAED-002 · 2026-09-13"; E5). Empty: `p.card-empty` (`TraineeHomeWords.NoDecisions`).
  - **"My authorisations"** (`award`, `#card-stars`): the standing's summary in the panel's words (`p.stars-summary`, `StandingWords.YearLine`: "1 at or above · 0 below · 15 with no decision, of 16 EPAs"), then each STAR below its training year's level or expiring within 30 days, a `decided-row` whose link opens its EPA's page with a `decided-note` ("4, below training year 4's level of 5 · expires 2026-10-23"); with no STAR, "No STAR yet. When the committee issues one, it shows here against training year 3's level." ("No STAR yet." once the programme has ended). Its foot "Open My authorisations". The card is no longer a link around its links (T280).
  Until flow 05 the Trainee's Home was Curriculum targets (a linked card, "2 / 5" "semester targets met"), Needs you, Recent activities ("All activities →"), Upcoming deadlines and a linked My authorisations; Recent activities and Upcoming deadlines are retired (Q3; T298): a date is shown on the row it belongs to.

## Contrast

Text pairs pass. A progress fill on its `hover-bg` track: `secondary-color` 4.61:1, a complete bar's `success-color` 5.58:1 (2.73:1 until T335).

## Known gaps

- The loading status is filled at the first render and in the prerendered HTML, so a screen reader may not announce it; DashboardFrame's comment says its words are announced when they come (T352).
- Home reads every waiting row and every decision the Assessor ever made to show five of each (T351).

- `Href` has no consumer since flow 05 but is still built: a card with it wraps its body in an `<a>`, so a link inside it would nest one anchor in another (T280's rule forbids the shape), and its `<a>` carries an inline `style="text-decoration:none;color:inherit;"` (DashboardCard.razor), where a class such as `.detail-card--interactive` should carry it. Do not take either as the pattern.
- The Administrator's System health rows for the email queue and the nightly job are stubs that always show `warn`, labelled with task ids "(T012)" and "(T024)" (T327).
