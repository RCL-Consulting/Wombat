# F05 A registrar reads where they stand

**Who and why:** each of the five registrars, every week and often on a phone between patients (Step A.7.3), reads her
targets for this semester and this year, her shortfalls and by when, her training year, her entrustment standing
against Annexure A and her rating trajectory per EPA. It is her only warning before the committee judges her (flow 07),
and the other half of flows 03 and 04: what she files there and her consultants rate there is counted here.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked W in BRIEF.md § 8 for the Trainee's Home and My progress: the structure is expected to change, so round 1 asks for 2–3 structural variations there. **My activities is fidelity:** flow 03 designed it (see "What flow 05 may change of what flows 03 and 04 built" in § 1). |
| Mode | **Structure first** (BRIEF.md § 2.3, as § 11 corrected it). Round 1: 2–3 structural variations as wireframes of Home and My progress and the way between them, the activity page and My activities, each with its reasoning and its step counts, plus the trajectory drawn once at both widths. Round 2, after the pick: fidelity with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | 2–3 in round 1 (Home and My progress); the chosen one in round 2 |
| People | Trainee: Dr Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa Mahlangu and Dr Sipho Ndlovu (1) (`README.md` § Cast). Dr Molefe graduates in Act 5 and Dr du Plessis withdraws; their ended records are flow 13's |
| Runbook steps | 17, pasted verbatim in § 8 |
| Pages (`coverage.md` templates) | `/` (the Trainee's Home), `/portfolio/progress` (My progress), `/activities/mine` (My activities). The steps also pass through `/activities/{ActivityId:int}` (Step 3.6, flow 03's page and its "Open My progress"), `/activities/new` (the pickers of Steps 6.19, 6.27 and 6.37, flow 03's) and `/account/data-rights` (A.7.3, flow 14's). The Trainee's menu (`NavItems.cs:158`): Home, Log an activity, My activities, MSF reports, My committee reviews, Export portfolio; then My progress and My data rights under the rule as personal links (`NavItems.cs:185–190`). My authorisations (`/portfolio/authorisations`) has no menu item; Home's card is its only way in (BRIEF § 7 B8). Checked at `f913dda3`. |
| Components drawn at runtime | `Components/Pages/Dashboards/TraineeDashboard.razor` (in `DashboardFrame`, `DashboardCard`, with `Shared/Activities/NeedsYouList.razor`); `Pages/Portfolio/MyProgress.razor` (`StatePanel`, `Alert`, `Shared/EntrustmentStandingPanel.razor`, `Shared/TrajectoryChart.razor`, `EpaLabel.razor`, `MsfCoverageText.cs`); `Pages/Activities/MyActivities.razor` (`NeedsYouList`, `DataTable` with `Stack`, `ActivityLink`, `PagerControls`, `ActivityListWords`, `CreditOutcome`); `Shared/Activities/ActivityStatus.razor` (the completed card's "Open My progress", `ActivityPageModel.cs:375–376`) |
| Held | None: no group-1 task changes these pages |
| Depends on | Flow 01, built (`b347e11c`): the shell, Home's frame ("Home", "Trainee · Semester 2, 2026", the header action "Log an activity"), `DashboardFrame`. Flow 03, built (`725237ee`): My activities, Needs you and Home's Needs you card, the activity page and its status card. Flow 04, built (`06aa51d7`): the list and dashboard vocabulary (rows named per row, the count as words, the pager). The design system is version 13, re-synced to flow 04 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 05`. Open each screenshot. Leave out anything in
   `design/upload/crop-first/`, or crop its link first. Flow 03's trainee-facing boards (§ 4) are not staged by the
   script.
2. **Start a new canvas** from the main app's Design page, on the Wombat design system v13
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not flow 01's, 02's, 03's or 04's canvas.
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Put the returned `/_blob/` URL in § 1 and in
   `design/flows/05-trainee-progress/round-1-ask.txt`, where they read `<MARK-URL>`.
4. **Send § 1 and § 8 as ONE message** (`design/flows/05-trainee-progress/round-1-ask.txt`), with the key screenshots
   in § 4. The canvas draws round 1 from the first message and does not wait for a second (§ 11, flow 02). Read
   `project/canvas.json`'s version before and after: a send can draw nothing.
5. **Pick one structure,** say why in a sentence, and answer § 6's questions one sentence each; questions 8 and 9 are
   the operator's. Ask for round 2.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/04-assessor-inbox/round-2-review.md`
   did: one reviewer on the code, one on the cast; the reviewer on the code checks the ask too (§ 11, flow 03). Put the
   decisions to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/05-trainee-progress/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 05 — Know where I stand this week, before the committee judges me, on any screen

This is flow 05 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity; My activities; the activity page) and 04 (an assessor's inbox) are
  designed and built; these pages sit inside the shell. Structure first, then fidelity: round 1 is 2–3 structural
  variations as wireframes of the registrar's Home and My progress, and I pick one before round 2. My activities and
  the activity page are flow 03's: extend them, do not redesign them. The runbook steps follow below in this same
  message. The brand mark is at /_blob/5e32abd7ee425f64c0db739c82470df3: use it, never a drawn disc.

GOAL: A registrar opens Wombat once a week and learns, in under a minute and on a phone if need be:
  - this semester's observation targets and how many she has met; this academic year's;
  - her training year, which sets the minimum level each encounter is judged against;
  - which EPAs are short, by how many, and by when;
  - her entrustment standing: the STAR level the committee gave each EPA, against her training year's target and the
    College's exit level;
  - her rating trajectory per EPA;
  and from any of these, the evidence behind it. She must also read correctly three catalogue events: an EPA the
  College has paused, an item her own institution added, and her counts after she is moved to a newer curriculum
  version. Nothing else warns her before the committee sits.
  What is wrong today (the attached screenshots show each; all on the shell as built, 2026-10-03, unless marked):
  - Home has no training year; only My progress says it.
  - Home and My progress say the same figure two ways: Home "1 / 10" and "semester targets met", My progress "1 of 10
    EPAs met this semester". Home's "Semester 2, 2026 · July to November" repeats the header's "Trainee · Semester 2,
    2026".
  - Home's Curriculum targets card is one link: the whole card, its five shortfall rows included, opens My progress
    and reads as one long link name. No row opens its own EPA. My authorisations is a link card with a second link
    inside it.
  - Home has two ways to My activities (Needs you's "Open My activities", a text link, and Recent activities' "All
    activities →", a button). Recent activities names its rows by instrument only ("Mini-CEX (Paediatrics)" three
    times at 390 px), orders them by when they were created where My activities orders by encounter, and holds nothing
    My activities does not.
  - Upcoming deadlines can never fill: it reads a field no instrument has, so it always says "No deadlines in the next
    14 days." My authorisations is a sentence and a button, the only way in to her STARs (the menu has none).
  - At 390 px Home's shortfall counts wrap to three lines ("0 of 3 / this / semester") beside the EPA titles.
  - My progress is one long page: 4,983 px tall at 1280 and 11,157 px at 390 for Dr Dlamini. Its order is fixed: "This
    period", ten "Each semester" cards, five or more "Once a year" cards, the 15-row Annexure A table, then the
    trajectories last. One EPA is in three places (its card, its standing row, its chart) about 2,000 px apart, and
    nothing links them.
  - Every card repeats two MSF sentences ("MSF in Semester 2, 2026: no released campaign covering this EPA has closed
    yet. MSF in Semester 1, 2026: …"); every yearly card repeats "Your training year changed on 14 Jan 2026, so
    encounters before then were judged against the previous year's minimum."
  - Three date forms side by side: "30 November 2026" on a card, "2026-09-23" for the last encounter, "3 Oct 2026" for a
    STAR and for the training-year change.
  - The trajectory charts are 600×200 drawings with 11 px labels, scaled into their card: at 390 px (308 px wide) and
    at 1280 in the narrow column of their two-column grid the rungs and dates are about 5 px tall. The x-axis names only
    the first and last date, so a one-point chart reads "2026-10-01 … 2026-10-01". Each chart's table (date, rating,
    evidence type) is for screen readers only. The heading counts "distinct assessors", whom the table does not name.
  - The Annexure A table has six columns and scrolls inside its box at 390 px; its rating links are 21 px tall.
  - KGK-001, the institution's own EPA, is marked so only in the standing table; its card says nothing.
  - After a move to a new curriculum version every card reads 0 and nothing says why, until an administrator's rebuild.
  - My progress's load error prints the database driver's text ("An exception has been raised that is likely due to a
    transient failure.") with no Try again (pre-shell capture, the page unchanged since); My activities says a sentence
    and offers Try again.
  - A completed activity's "Open My progress" opens the top of the page, not the EPA it credited, and its status card
    does not say the count it made ("1 of 3 this semester").

AUDIENCE: Trainee. The five paediatric registrars at Kgosi Kgari Teaching Hospital (KGK), on the College's (CPSA)
  curriculum 11.1: Dr Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa
  Mahlangu and Dr Sipho Ndlovu (1). Doctors reading between patients, often on a phone. Their menu is Home, Log an
  activity, My activities, MSF reports, My committee reviews and Export portfolio, then My progress and My data rights
  under the rule. Desktop 1280×800 and phone 390×844.

THREE DIFFERENT "PER"S THE DESIGN MUST KEEP APART:
  - Observation targets, per window (Annexure B): ten EPAs have a target per semester (PAED-001–005, 010 and 012 at 3;
    006 at 2; 007 and 015 at 1), and five per academic year (008, 009, 011, 013, 014). Semester 1 is January to June;
    semester 2 is July to November, and December counts into it.
  - Decision cadence: when the committee decides the EPA. Each semester for PAED-001, 002, 004, 005, 010 and 012; each
    academic year for the rest. So PAED-003 has a per-semester observation target but is decided once a year.
  - Level targets, per training year (Annexure A): the minimum level for her training year (for example 3a, 3b, 4, 5 in
    years 1–4), and the exit level (5 or 4).
  Figures are always "n of m" for a named window ("1 of 3 this semester", "0 of 1 in 2026"). Never a lifetime total,
  never a percentage, never "n / m". "Training year N" and "Semester 2, 2026" / "2026 academic year" are different
  things, and the bare word "year" is never used beside both.

WHAT THE PAGES CARRY TODAY (real figures; they are the data to draw with):
  - Home (Step 3.50, Dr Dlamini): Curriculum targets "Semester 2, 2026 · July to November", "1 / 10" semester and
    "0 / 5" yearly targets met (2026), then the five EPAs furthest short, largest shortfall first (PAED-002, 003, 005,
    010, 012, each "0 of 3 this semester", with a bar); Needs you ("Nothing needs you. Requests you have filed are in My
    activities.", Open My activities; flow 03's); Recent activities (five, each with its state's badge); Upcoming
    deadlines; My authorisations.
  - My progress, "This period" (Step 3.7): "Semester 2, 2026 · July to November"; "0 of 10 EPAs met this semester";
    "0 of 5 EPAs met in 2026"; Training year "3 — it sets the minimum level each encounter is judged against";
    multi-source feedback "0 of 15 EPAs covered by a released campaign that closed this semester. MSF is tracked on its
    own and counts towards no target."
  - An EPA card, PAED-001: "1 of 3 this semester", a bar, "2 more by 30 November 2026. At the minimum level when
    observed: 1 of 1. Last encounter date: 2026-09-23.", "Target: 3 per semester (6 a year). Minimum now 4.",
    "Semester 1, 2026: 0 of 3, 3 short", two MSF lines. Met: "Target met for Semester 2, 2026." and a green bar.
  - "Entrustment against Annexure A" (Step 4.40, Dr Molefe): "You are in training year 4 on 3 October 2026."; "2 at or
    above · 1 below · 12 with no decision, of 15 EPAs"; the exit rule "2 of 15 EPAs at their exit level by STAR
    decision (level 5: … · level 4: …)", "For information only. …"; a table: EPA, Year 4 target, STAR decision ("5,
    Issued 3 Oct 2026"; "4, Issued 3 Oct 2026, expires 23 Oct 2026"), Against target (a badge: At or above, Below, No
    decision), Exit level (Reached, Not yet), Latest rating ("4", "Encounter 2026-09-24", "Direct observation; below
    the target"). The committee's review page shows the same table (flow 07).
  - "Rating trajectory": one chart per EPA rated by a named assessor, on the six-rung ladder 1, 2, 3a, 3b, 4, 5, headed
    by the EPA and "3 observations from 3 distinct assessors". MSF is never plotted (D36).
  - My activities (flow 03's): Needs you when anything does; then "All activities (N)", each row "Mini-CEX (Paediatrics)
    · PAED-001 · 2026-09-23", "to David Naidoo", Who has it now "Done", State Completed, Credit "1 item" ("—" for MSF,
    "None" on a paused EPA, with "PAED-012 (no longer in use)" under the name); a pager.
  - Data volumes: none (first day, all zeros, Step 2.39); typical (Dr Dlamini: 15 EPAs, four trajectories, six
    activities); heavy (17 cards after a version move with a local item, Step 6.37; 15 STARs, every one at its exit
    level, Step 5.15; an EPA title of about 120 characters, "PAED-015 — Teaching and applying evidence-based care
    responsibly and ethically in clinical decision-making and research").

SCREENS, in order (today's names; a variation may merge, split or rename them):
  1. Home for the Trainee (/, "Trainee · Semester 2, 2026", header action "Log an activity"): what she must do or owes
     this week; her training year; the way into each EPA, My progress, My activities and her STARs. The first day; a
     typical week; 390 px.
  2. My progress (/portfolio/progress; its nav item "My progress"): this period; each EPA's count, shortfall and
     deadline, minimum and last encounter; the standing against Annexure A and the exit rule; the trajectories; MSF
     coverage. Every state below, at 1280 and 390.
  3. One EPA, wherever the variation puts it (a section, an expanding row, or a page of its own): its count in this
     window and the last, its minimum now, its STAR against the year's target and the exit level, its trajectory, and
     its evidence (the activities that counted, linking to them).
  4. The rating trajectory, drawn once at 1280 and 390 px with its table visible where the chart cannot be read. The
     same component draws the committee's review page (flow 07), so design it once.
  5. The way in from an activity: the completed status card's "Open My progress" (flow 03's card) and where it lands.
  6. My activities filtered by nothing (it has no filter): its MSF rows (credited "—") and a paused EPA's row, checked
     against the new progress pages, in flow 03's design.

WHAT FLOW 05 MAY CHANGE OF WHAT FLOWS 03 AND 04 BUILT, AND WHAT IT MAY NOT:
  May not (flows 01, 03 and 04 decided them): the shell and Home's frame ("Home", the role and semester under it, the
    one header action, no h1 of a dashboard's own); My activities' shape (Needs you first when anything does, then All
    activities a page at a time, newest encounter first; its four columns; rows stacked below 641 px with Who has it
    now and Credit labelled; each row's link named "Type · EPA · date" with "to <assessor>" under it; its load error
    sentence and Try again); Needs you's rows and words (NeedsYouList), on My activities and on Home alike; the
    activity page and its status card's parts, words and order; "(no longer in use)" as the paused mark; the other-role
    line (an Assessor holder's, not the Trainee's).
  May, for the registrar (flow 03 left these to flow 05): every other card on the Trainee's Home (Curriculum targets,
    Recent activities, Upcoming deadlines, My authorisations), their order, and where Needs you sits among them; Home's
    second way to My activities (Needs you's text link beside Recent activities' button); where "Open My progress"
    lands (the EPA it credited, not the top of the page) and whether the completed card says its count ("Rated 4.
    Credited 1 item to PAED-001: 1 of 3 this semester."); what My activities' Credit cell links to. Say which of these
    each variation changes, and why.

STEPS: 2.39, 2.40, 3.6, 3.7, 3.48, 3.50, 4.40, 5.15, 5.26, 6.15, 6.19, 6.24, 6.27, 6.35, 6.37, 6.39, A.7.3. Pasted
  verbatim below (Role / Route / Do / Expect). In them, D is the day the story is replayed and J the latest 15 January
  on or before D. Where an Expect quotes today's wording, that is today's page, not a requirement on the new one.

STATES TO SHOW:
  Home (Trainee): first day (all zeros); typical; a request returned to her (Needs you with a row); no curriculum
    assigned yet ("No curriculum assigned yet. …"); no EPA in use; the programme not started ("Your programme starts
    on …"); started part-way through a period; loading; load error; narrow.
  My progress: first visit (all zeros); one encounter counted; target met, with MSF coverage; standing with STARs, one
    below its target and expiring; exit rule met ("15 of 15 … Every EPA is at its exit level."); a paused EPA (no card;
    the trajectory headed "(no longer in use)"); an institution's own item (KGK-001); after a version move (PAED-016,
    PAED-011 at 2 a year) and, if question 9 keeps it, before the rebuild; loading (the header, then a skeleton); load
    error (a sentence under the header, Try again, nothing empty under it); narrow.
  My progress states no replay reaches, which you must still draw: "No curriculum items assigned yet"; "Your programme
    starts on …"; "You started part-way through a period" (the target is waived for that window: show the count and
    the window targets start with, never a fraction or a bar); "No EPA on your curriculum is in use"; an encounter
    counted towards the number but not the level (rated on a different scale, drawn hollow on the chart); December
    ("The 2026 academic year ended on 30 November. Encounters observed in December still count towards semester 2,
    2026 …").
  One EPA: no evidence yet; one encounter; target met; below its STAR target; paused; the institution's own.
  The trajectory: one point; three points from three assessors; a point off the ladder; a paused EPA's heading.
  My activities: MSF rows; a paused EPA's row.
  Data volumes: as above, none / typical / heavy.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T306: "Training year N" on Home, worded as on My progress.
  - T298: Upcoming deadlines is removed, or shows a real date: this window's end for an unmet target ("2 more by 30
    November 2026"), a committee review scheduled for her, or a STAR nearing expiry (PAED-010's expires 23 Oct 2026).
    A card that can never fill is not acceptable.
  - T323: no chart text below 11 px at 390 px, or the chart's table is visible under it at phone width; nothing
    scrolls sideways, the Annexure A table included.
  - T328: every target ≥ 24 px (the standing table's rating links are 21 px today), 44 px on a phone; badges keep their
    pill; controls in the body font.
  - T280: every link on Home is named by more than its instrument ("Mini-CEX (Paediatrics) · PAED-001 · 2026-09-23",
    flow 03's name), so no two share a name; a card is not one link around other links.
  - T329 and T272: My progress's header shows from the first moment, a skeleton while it loads, and its load error is a
    sentence under the header with Try again and nothing empty drawn beneath it, never the exception's text, as My
    activities does since flow 03. Each section that loads apart (the standing, MSF coverage) fails apart, in a
    sentence.
  - T346: the completed status card's count ("1 of 3 this semester"), here or on My progress where "Open My progress"
    lands; flow 03 left it to this flow.
  - T304: a registrar moved to a new curriculum version is not left reading zeros with no word (question 9).
  - T312 and T255: an EPA no longer in use keeps its place where it is a record, headed "PAED-012 — … (no longer in
    use)". Design the mark for a card heading as well as a chart heading.
  - T325: every "today" and every date is South African; one date form for people to read.
  - D8, D9 (the College): MSF counts towards no target, and a campaign covers an EPA, never is "about" it; keep
    MsfCoverageText's words, however they are placed.
  - Content (BRIEF.md § 6): sentence case; states and types by their labels; people by name as stored; no system words.
  - Done since the old brief, keep them: badges keep their pill beside a wrapping link (T335); badge and alert
    contrast (T322); Needs you replaces the Activity inbox card, the same rows as My activities' (T342); the Actions
    card is gone, its job is the header's "Log an activity" (T335); My activities' rows stack and are named per row
    (T342); a graduate's Home is "Your training record" with Open My progress, not "No role assigned" (T335; the rest of
    her record is flow 13's).

QUESTIONS THE DESIGN MUST ANSWER:
  1. What leads My progress: this period's shortfall, the standing per EPA, or the trajectory? And is an EPA one place
     (its count, its STAR, its chart and its evidence together) or three sections as today?
  2. What does Home hold that My progress does not, and the other way round? Home's Curriculum targets repeats "This
     period" in other words. Does MSF appear on Home? Does Recent activities stay, now that Needs you and My activities
     hold every row?
  3. Upcoming deadlines: remove it, or which real source (T298)?
  4. How are per-semester observation targets, per-year decision cadence and per-training-year levels shown together
     without confusing them?
  5. The trajectory: at 390 px, a visible table, a taller chart, or both? Does the chart draw the training year's
     minimum and the exit level as lines? What does a one-point chart show?
  6. How are a paused EPA, an institution's own item (KGK-001) and an item only a newer version holds (PAED-016 on
     11.2) marked, on a card, a row and a chart heading?
  7. From a completed activity, where does "Open My progress" land, and does the status card say its count?
  8. (The operator's) My progress is a personal link under the rule, with My data rights (flow 01's rule), though it is
     the registrar's weekly page; and My authorisations has no menu item. Move either into the Trainee's group?
  9. (The operator's) After a move to a new version, draw a notice that her counts are being rebuilt, or treat the
     rebuilt state as the only one because T304 replays credit in the same save?

CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5; restated after the flow 01 pilot, 2026-09-27)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  the error page, /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side
  behaviour beyond a small script module; no live validation; the phone menu is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence (Source Sans 3 and Fraunces, OFL, ship).
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token (the design system's tokens.json) or a NEW token with its value; prefer the spacing scale
  xs 4, sm 8, md 16, lg 24, xl 32, 2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
The shell is designed and built (flow 01): design the page body inside it, not a new frame. The sidebar shows the
  acting role's navigation (one role at a time; access is the union of the roles held), grouped above eight links, with
  My progress and My data rights under a rule; the top bar holds the person's name and Sign out; below 641 px a phone
  bar with a CSS-only menu. A new page names its owning list (its nav item lights, its breadcrumb follows it) and its
  title "<Page> · Wombat", the same words as its h1 and its nav label, in sentence case.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, the trail from the
  owning list, an optional icon, header actions), DataTable (.clinic-table in .table-container, PagerControls),
  FormField / FormActions (.form-container, .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid,
  loading and load error), StatePanel (loading / empty / error), Skeleton, Alert (success / info / warning / danger,
  text on the tint), ActionResult, ConfirmDialog (native <dialog>), badges (five tints), Icon, TrajectoryChart
  (hand-drawn SVG, no chart library), ReferenceBlock.
  Since flow 03 also: ActivityStatus, ActivityAbout, ActivityForm (sections open, filled, locked), RungRow,
  ActivityWorkflowActions (the action bar, the note panel), SubmitCheck, RefusalSummary, ActivityHistory, NeedsYouList,
  InstrumentPicker, and DataTable's stacked rows (Stack). Since flow 04: the rung picker, WaitingList, WayOn, the
  Overdue badge and the other-role line.
Already designed by flow 01, reuse them: the active nav item, the acting-role switch and its result alert, access
  denied, not found, the error page, the reconnect dialog and the in-app error bar. Still to design where a flow meets
  them: field validation (invalid border plus a stripe, not colour alone; message under the field; the summary) and
  session ended.
Content: sentence case everywhere; people by name as stored (no titles); states and types by label; times in South
  African time with the zone shown; an out-of-scope record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (the sidebar's white ring included); targets
  ≥ 24 px, 44 px on a phone; focus moves to an action's result; every page works at 390 px with no sideways scroll;
  reduced motion honoured.
Viewports: 1280×800 and 390×844.
Also: TrajectoryChart is hand-drawn SVG with no chart library. You may change the chart's form, but it must stay SVG
or HTML with no script. Every chart needs a text or table equivalent. EntrustmentStandingPanel draws the same table on
the committee's review page (flow 07): change it once, for both.

ASK:
  - Round 1: 2–3 STRUCTURAL variations as wireframes of Home (the Trainee's) and My progress, and of one EPA wherever
    each puts it. Each covers the pages and their order (or what replaces them), the steps of each journey (the weekly
    look from sign-in; from a completed Mini-CEX to its EPA's count; read her standing against Annexure A before a
    review; the same on a phone; find what a paused EPA, KGK-001 and a version move did to her figures), and where each
    figure, mark and failure shows. Give each its reasoning and its step count per journey, at 1280 and 390 px. No
    colour or type choices yet. Stop and wait for my pick.
  - In the same round, draw the trajectory ONCE at 1280 and 390 with its table, and say what each variation does with
    flow 03's pieces under "What flow 05 may change".
  - Round 2: the chosen structure at full fidelity on the design system's tokens, every screen and state above, at
    1280 and 390 px.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (DESIGN.md § Dashboard page; § Page-level patterns; the progress figures rule, "a count against a target
    for a named window"; "My progress once the programme has ended", which is flow 13's and must still hold).
  - Flag edge cases: all zeros; every target met; a 120-character EPA title; 17 cards; 15 STARs; a one-point and an
    off-scale trajectory; December; a start part-way through a period; a registrar who also acts as Assessor.
  - Run an accessibility review against WCAG 2.1 AA: every link named for its EPA or activity, figures read as words
    ("1 of 3 this semester", never a bare bar), each chart's equivalent reachable, targets ≥ 24 px, focus after Try
    again and on arriving at an EPA from "Open My progress".

ATTACHED: act-3/3.50-1-dlamini-home.png, act-A/A.7.3-1-home-390.png, act-2/2.39-1-molefe-home.png,
  act-3/3.7-1-dlamini-progress.png, act-A/A.7.3-3-progress-390.png, act-4/4.40-1-molefe-progress-annexure-a.png,
  act-6/6.19-1-dlamini-progress.png, act-6/6.27-1-dlamini-progress.png, act-6/6.37-1-ndlovu-progress.png,
  states/my-progress--load-error.png, act-A/A.7.3-4-activities-390.png, act-6/6.19-2-dlamini-my-activities.png;
  flow 03's boards: R3-M-home-card (Home's Needs you card), R3-C-Mine (My activities' component board) and
  R3-A-completed (the completed status card with "Open My progress"), at 1280 and 390.
```

---

## 2. The journey

| Step | Page | The registrar does | She must be able to see |
|---|---|---|---|
| 2.39 | `/account/login` → `/` → `/portfolio/progress` | Dr Molefe signs in for the first time after admission | 0 of 10 and 0 of 5, the shortfalls, her training year 4 (on My progress only today, T306); her menu |
| 2.40 | `/account/login` → `/` → `/portfolio/progress` | the other four do the same | the same shape; training years 3, 2, 1 and 1; no "started part-way" line (a 15 January start is on time, D42) |
| 3.6 | `/activities/mine` → `/activities/{ActivityId:int}` | Dr Dlamini finds her completed Mini-CEX and opens it | Done, Completed, "1 item"; the read-only page, "Rated 4. Credited 1 item to PAED-001.", Open My progress (question 7) |
| 3.7 | `/portfolio/progress` | she reads what it counted | PAED-001 "1 of 3 this semester", what is still owed and by when, the minimum now, last semester's shortfall, a one-point trajectory |
| 3.48 | `/activities/mine` → `/portfolio/progress` | Dr Molefe reads her evidence and progress | six WBAs and two MSF rows credited "—" (D8); PAED-001's target met; MSF 2 of 15; no MSF point on a chart (D36) |
| 3.50 | `/` | Dr Dlamini reads Home | 1 of 10 and 0 of 5, the five furthest short, Needs you, Recent activities (named by instrument), the empty deadlines card, My authorisations |
| 4.40 | `/portfolio/progress` | Dr Molefe reads her standing after her review | "Entrustment against Annexure A": 2 at or above, 1 below (expiring 23 Oct), 12 with no decision; the exit rule, which gates nothing |
| 5.15 | `/account/login` → `/account/login/submit` → `/` → `/portfolio/progress` | near the end, she leaves My progress open | every EPA at its exit level, the programme still running; nothing says it has ended |
| 5.26 | `/portfolio/progress` | Dr du Plessis reads PAED-002 | yesterday's Mini-CEX counted, as the last encounter |
| 6.15 | `/portfolio/progress` | Dr Dlamini, before a College pause | PAED-012's count and minimum; "1 of 10" |
| 6.19 | `/portfolio/progress` → `/activities/mine` → `/activities/new` | during the pause | no PAED-012 card and "1 of 9"; its trajectory headed "(no longer in use)"; My activities' row marked, credited None; the picker (flow 03) offers eight EPAs |
| 6.24 | `/portfolio/progress` → `/activities/mine` | after the pause | the card back with the new count, the marks gone, credited "1 item" (D48) |
| 6.27 | `/portfolio/progress` → `/activities/new` | Dr Dlamini, after KGK adds its own EPA | KGK-001 under "Once a year" at 0 of 1, the year before "1 short"; only the standing table says it is the institution's own |
| 6.35 | `/portfolio/progress` | Dr Ndlovu, before he is moved | sixteen cards on 11.1 |
| 6.37 | `/portfolio/progress` → `/activities/new` | moved to 11.2 | seventeen cards, every one 0, nothing saying why (T304; question 9) |
| 6.39 | `/portfolio/progress` | after the rebuild | his counts back; PAED-016 and KGK-001 at 0; PAED-011 against 2 a year |
| A.7.3 | `/` → `/portfolio/progress` → `/activities/mine` → `/account/data-rights` | Dr Dlamini on her phone | Home's cards stacked; every EPA's figures and chart in the width; My activities stacked; no sideways scroll |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Home and the role dashboards,
§ Portfolio and § Activities. **The Home and My activities states are on the shell as built** (the T342 states,
2026-09-30). **The `my-progress--*` states are pre-shell** (the T295 sweep, 2026-09-26: the old nav, the email in the
top bar), but the page body has not changed since; each has a new-shell step capture from the T350 replay (2026-10-03),
named beside it, which is the one to brief from.

| Page | State | Screenshot | What it shows |
|---|---|---|---|
| Home | First day | `act-2/2.39-1-molefe-home.png` (`states/home--trainee-first.png`) | "0 / 10", "0 / 5", PAED-001 to 005 "0 of 3 this semester"; no training year; "No activities yet."; the deadlines and authorisations cards |
| Home | Typical | `act-3/3.50-1-dlamini-home.png` (`states/home--trainee.png`) | "1 / 10"; five furthest short; Recent activities, "Mini-CEX (Paediatrics)" twice |
| Home | Work returned to her | `states/home--trainee-returned.png` | Needs you with a row (flow 03's card) |
| Home | Narrow | `act-A/A.7.3-1-home-390.png` (`states/home--narrow-trainee.png`) | the counts wrapping to three lines; "Mini-CEX (Paediatrics)" three times; badges keep their pill |
| My progress | First visit | `act-2/2.39-2-molefe-progress.png` (`states/my-progress--first.png`) | "0 of 10 EPAs met this semester", Training year "4 — it sets …" |
| My progress | One encounter | `act-3/3.7-1-dlamini-progress.png` (`states/my-progress--counting.png`) | the whole page, 4,983 px: This period, the cards, the standing table, one chart last |
| My progress | Target met, MSF | `act-3/3.48-2-molefe-progress.png` (`states/my-progress--msf.png`) | PAED-001 "3 of 3", a green bar; MSF "2 of 15" |
| My progress | Standing | `act-4/4.40-1-molefe-progress-annexure-a.png` (`states/my-progress--standing.png`) | the six-column table: 5 At or above, 4 Below expiring 23 Oct 2026, No decision |
| My progress | Exit rule met | `act-5/5.15-1-molefe-progress-running.png` (`states/my-progress--exit-met.png`) | "15 of 15 … Every EPA is at its exit level." |
| My progress | A paused EPA | `act-6/6.19-1-dlamini-progress.png` (`states/my-progress--paused-epa.png`) | no PAED-012 card; its chart headed "(no longer in use)" in the narrow column, labels unreadable |
| My progress | Institution's own item | `act-6/6.27-1-dlamini-progress.png` (`states/my-progress--local-item.png`) | KGK-001 leads "Once a year", its card unmarked |
| My progress | Moved, before the rebuild | `act-6/6.37-1-ndlovu-progress.png` (`states/my-progress--before-rebuild.png`) | every card 0, "0 of 7 EPAs met in 2026", nothing says why |
| My progress | Moved, after the rebuild | `act-6/6.39-1-ndlovu-progress.png` (`states/my-progress--after-rebuild.png`) | PAED-002 "1 of 3" again; seventeen cards |
| My progress | Loading | `states/my-progress--loading.png` (pre-shell) | the header, four skeleton bars |
| My progress | Load error | `states/my-progress--load-error.png` (pre-shell) | "An exception has been raised that is likely due to a transient failure." and nothing else |
| My progress | Narrow | `act-A/A.7.3-3-progress-390.png` (`states/my-progress--narrow.png`) | 11,157 px; four charts at the foot with ~5 px labels |
| My activities | MSF rows | `act-3/3.48-1-molefe-my-activities.png` (`states/my-activities--msf.png`) | two "Multi-Source Feedback (Paediatrics)" rows, Recorded, credit "—" |
| My activities | A paused EPA | `act-6/6.19-2-dlamini-my-activities.png` (`states/my-activities--paused-epa.png`) | "PAED-012 (no longer in use)" under the name, Credit None |
| My activities | Narrow | `act-A/A.7.3-4-activities-390.png` (`states/my-activities--narrow.png`) | stacked rows, Who has it now and Credit labelled |

Not captured, so describe them in words (`states.md`; BRIEF § 10):
- **Upcoming deadlines with a row.** Nothing can fill it (T298).
- **"No curriculum assigned yet" on Home and "No curriculum items assigned yet" on My progress.** Admission sets the
  role and the profile together, so no cast member meets them (`TraineeDashboard.razor:31`, `MyProgress.razor:26`).
- **"Your programme starts on …" and "You started part-way through a period."** Every start is 15 January, on or before
  `D` (D42) (`TraineeDashboard.razor:63–72`, `MyProgress.razor:94–130`).
- **"No EPA on your curriculum is in use"** (`TraineeDashboard.razor:42`, `MyProgress.razor:31`).
- **An encounter counted across scales,** drawn hollow on the chart (`TrajectoryChart.razor:42`); every rated
  instrument uses the v11.1 ladder.
- **`my-progress--december`,** the December notice (`MyProgress.razor:84–90`), shown only when `D` is in December.
- **Home's loading and load error for the Trainee.** `DashboardFrame` draws them (flow 01's), not captured for this role.

**Belongs to other flows:** the ended-programme states (`my-progress--completed`, `--withdrawn`, `--narrow-former`,
`home--trainee-ended`, `act-5/5.21-1-molefe-home-graduate.png`) are flow 13's; the pending trainee's "Awaiting
admission" card is flow 11's; the activity page and Needs you are flow 03's.

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-10-03; all but the
load error are new-shell captures from the T350 replay.

**Key screenshots (attach these first, with § 1):**
1. `act-3/3.50-1-dlamini-home.png`: Home, "1 / 10" and "0 / 5", no training year; Recent activities "Mini-CEX
   (Paediatrics)" twice; the empty deadlines card; My authorisations as a sentence
2. `act-A/A.7.3-1-home-390.png`: Home at 390 px, the counts wrapping to three lines, three rows reading "Mini-CEX
   (Paediatrics)"
3. `act-2/2.39-1-molefe-home.png`: the first day, all zeros
4. `act-3/3.7-1-dlamini-progress.png`: My progress whole, 4,983 px: the cards' repeated MSF and training-year lines, the
   15-row standing table, one small chart last
5. `act-A/A.7.3-3-progress-390.png`: the same at 390 px, 11,157 px, four charts at the foot with labels about 5 px
6. `act-4/4.40-1-molefe-progress-annexure-a.png`: the standing table, At or above, Below (expiring 23 Oct 2026), No
   decision; "3 Oct 2026" beside "2026-09-24"
7. `act-6/6.19-1-dlamini-progress.png`: no PAED-012 card; its chart headed "(no longer in use)"; a one-point chart
   reading "2026-10-01" at both ends
8. `act-6/6.27-1-dlamini-progress.png`: KGK-001 leads "Once a year", nothing on its card says it is KGK's
9. `act-6/6.37-1-ndlovu-progress.png`: after the move, every card 0 and no word why
10. `states/my-progress--load-error.png`: the driver's text, no Try again (pre-shell; the page is unchanged since)
11. `act-A/A.7.3-4-activities-390.png`: My activities stacked at 390 px, flow 03's
12. `act-6/6.19-2-dlamini-my-activities.png`: My activities, the paused EPA's row credited None

Also attach **flow 03's trainee-facing boards**, from `design/flows/03-trainee-files-activity/round-3/project/`:
`R3-M-home-card-1280.dc.html` and `R3-M-home-card-390.dc.html` (Home's Needs you card), `R3-C-Mine.dc.html` (My
activities' component board, every state), and `R3-A-completed-1280.dc.html` and `R3-A-completed-390.dc.html` (the
completed status card with "Open My progress" and the count T346 carries), with `R3-C-Activity.dc.html`, the
component board the two completed wrappers import (the home-card wrappers import `R3-C-Mine`). Add `flow03.css` and
`flow03-r3.css` from the same folder, which the boards load. They are staged by hand in `design/upload/flow03-boards/`. The staging script does not stage them. Flow 04's boards hold one trainee-facing
state, `R3-I-registrar-only` (the Activity inbox as a registrar who types its address sees it), which is not a flow 05
page: leave it out.

**States (attach as the chat asks):** every row of § 3 not already attached above, and the step captures
`act-2/2.39-2-molefe-progress.png`, `act-3/3.48-1-molefe-my-activities.png`, `act-3/3.48-2-molefe-progress.png`,
`act-5/5.15-1-molefe-progress-running.png`, `act-6/6.39-1-ndlovu-progress.png`, `act-3/3.6-2-completed-view.png` (the
completed page and its Open My progress) and `states/my-progress--loading.png`.

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `f913dda3`.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T306** (P3; BRIEF B4) | Home's Curriculum targets has no training year (`TraineeDashboard.razor:46–62`); My progress says it (`MyProgress.razor:153–159`). Step 2.39's Gap, still. | `act-3/3.50-1-dlamini-home.png`, `act-2/2.39-2-molefe-progress.png` |
| **T298** (P3; BRIEF B13) | Upcoming deadlines scans each activity's data for a `due_date` key no instrument has (`GetTraineeDashboardSummaryQuery.cs:121–172`), so it always reads "No deadlines in the next 14 days." (`TraineeDashboard.razor:143–163`). Remove it or give it a real source; a real one exists: PAED-010's STAR expires 23 Oct 2026 (Step 4.40). | `act-3/3.50-1-dlamini-home.png` |
| **T323** (P3; BRIEF A3) | `TrajectoryChart` draws a 600×200 viewBox (`TrajectoryChart.razor:82–83`) with 11 px labels (`app.css:3458–3462`), scaled to its card: 308 px at 390 (A.7.3's Actual), and the narrow column of `.details-grid`'s `1fr 2fr` at 1280 (`MyProgress.razor:211`, `app.css:2159–2163`). Its table is `.visually-hidden` (`TrajectoryChart.razor:57`). The x-axis labels only the first and last date (`:51–52`). The standing table scrolls in its box at 390 (500 px in 358, A.7.3). | `act-A/A.7.3-3-progress-390.png`, `act-6/6.19-1-dlamini-progress.png` |
| **T328** (P3; BRIEF A4) | The standing table's rating links are 21 px tall (`EntrustmentStandingPanel.razor:144`; A.7.3's Gap, F-A.7.3a). Badges keep their pill now (T335). | `act-A/A.7.3-3-progress-390.png` |
| **T280** (P3; BRIEF A14) | Recent activities names each link by its type alone (`TraineeDashboard.razor:130`). Needs you's rows are named (NeedsYouList, T342). The Curriculum targets and My authorisations cards are each one link around their content (`DashboardCard.razor:24`), and My authorisations has a second link inside the first (`TraineeDashboard.razor:165–169`). | `act-A/A.7.3-1-home-390.png` |
| **T329, T272** (P2, P3) | My progress keeps its header and shows a skeleton, but its load error is the exception's text (`MyProgress.razor:267`), as are the standing's and MSF's (`:284`, `:363`), and its `StatePanel` has no Try again (`:14`). My activities shows a sentence and Try again since flow 03 (`MyActivities.razor:21`, `:84`). | `states/my-progress--load-error.png` |
| **T346** (P3) | The completed card reads "Rated 4. Credited 1 item to PAED-001." without the window's count; the activity page makes no progress read. Its task lets flow 05 carry the count. "Open My progress" opens the page's top (`ActivityPageModel.cs:375–376`). | `act-3/3.6-2-completed-view.png` |
| **T304** (P2, backend) | Moving a registrar to 11.2 replays none of his credit: every card reads 0 until the Administrator's rebuild, and nothing says why (Step 6.37's Gap, F-6.37a). T304's plan replays credit in the same save (question 9). | `act-6/6.37-1-ndlovu-progress.png` |
| **T312, T255** (P2; done) | A running trainee's paused EPA loses its card (T158) and keeps its chart, headed "(no longer in use)" (`MyProgress.razor:216`, `EpaLabel`). After T312 an ended record keeps a paused EPA's card with the mark (flow 13's). Design the mark for both headings. | `act-6/6.19-1-dlamini-progress.png` |
| **T325** (P3) | Upcoming deadlines takes "today" from UTC (`GetTraineeDashboardSummaryQuery.cs:121–122`). Three date forms: `QuotaText.LongDate` "30 November 2026" and `ShortDate` "3 Oct 2026" (`QuotaProgress.cs:214`, `:217`), and `EncounterDate.Label` "2026-09-23" (`MyProgress.razor:474`, `:485`, `:520`). | `act-4/4.40-1-molefe-progress-annexure-a.png` |
| **T349** item 9 (P3) | Home's Needs you footer is a text link "Open My activities" (`TraineeDashboard.razor:112`), Recent activities' a `.btn-sm .btn-outline` "All activities →" (`:138`), to the same page; T349 leaves it to flow 05. | `act-3/3.50-1-dlamini-home.png` |
| **T311** (P2; flow 13) | Since T335 a graduate's Home is "Your training record" with Open My progress (`Home.razor:62–70`), not "No role assigned"; her STARs and her export are still out of reach. Flow 05 must not contradict that card. | `act-5/5.21-1-molefe-home-graduate.png` |
| **T343** (P3, backend) | My activities reads every row to serve one page; no design change, but a design that adds a filter or a per-EPA list of evidence reads through the same query. | T343's symptom |
| Observed, not filed | Home says "1 / 10" where My progress says "1 of 10 EPAs met this semester" (`TraineeDashboard.razor:51`, `:58`; `MyProgress.razor:306`): the progress-figures rule (DESIGN.md § Dashboard layout grid, T130) says "n of m". Home's shortfall rows are not links (`:79–88`). Recent activities is the five newest by creation (`GetTraineeDashboardSummaryQuery.cs:83–87`), My activities newest by encounter. Every card repeats `MsfCoverageText.CardLine`'s two sentences (`MyProgress.razor:531–533`; by design, DESIGN.md: keep the words, D8, D9) and every yearly card the training-year change (`:518–521`). KGK-001 is marked only in the standing table (`EntrustmentStandingPanel.razor`), not on its card (`MyProgress.razor:444`, `:450`). The chart's table has Date, Rating and Evidence type (`TrajectoryChart.razor:61–63`), though the heading counts assessors. My progress is a personal link under the rule (`NavItems.cs:185–190`); My authorisations is in no menu (`NavItems.cs:158`). | `act-3/3.7-1-dlamini-progress.png`, `act-6/6.27-1-dlamini-progress.png` |

**Dropped from the old brief (fixed since):** a badge stretching beside a wrapping link (T335's `.list-row`; observed
on `act-A/A.7.3-1-home-390.png`); the Activity inbox card and its links named by instrument (T342 replaced it with
Needs you, named per row); the Actions card (T335); badge contrast (T322, landed with T335's tokens); the graduate's "No
role assigned" (T335; the rest of T311 stays with flow 13).

## 6. Questions the design must answer

1. **What leads My progress, and is an EPA one place?** Today's order is fixed (`MyProgress.razor:132–228`): This
   period, the cards, the standing, then the trajectory about 4,700 px down at 1280 (`act-3/3.7-1-dlamini-progress.png`).
   One EPA's count, STAR and chart sit in three sections. A section per EPA, an expanding row, or a page of its own (a
   new route, which would name My progress as its owning list)?
2. **What does Home hold that My progress does not?** Home's Curriculum targets repeats This period in other words;
   Recent activities repeats My activities; Needs you is flow 03's and stays. Does MSF coverage belong on Home?
3. **Upcoming deadlines** (T298): remove it, or source it from this window's end for an unmet target, a committee review
   scheduled for her, or a STAR nearing expiry.
4. **The three "per"s** shown together without confusion: per-semester observation targets (Annexure B, D39),
   per-semester or per-year decision cadence (`act-1-setup.md`), per-training-year levels (Annexure A, D17).
5. **The trajectory** at 390 px: a visible table, a taller chart, or both; lines for the year's minimum and the exit
   level; a one-point chart.
6. **The marks:** a paused EPA, "(no longer in use)" (T255, D48); an institution's own item, "The institution's own
   EPA; not in the exit rule" (Step 6.27); an item only a newer version holds (PAED-016 on 11.2, Step 6.37).
7. **From an activity to its EPA:** where "Open My progress" lands, and whether the completed card says "1 of 3 this
   semester" (T346).
8. **Where My progress and My authorisations sit in the menu.** My progress is a personal link under the rule (flow
   01's rule, `NavItems.cs:185–190`, pinned by `NavMenuAuthorizationTests` through DESIGN.md § The NavMenu); My
   authorisations has none (B8). Moving either changes flow 01's shell. Operator's call.
9. **A registrar moved to a new version.** Draw a "your counts are being rebuilt" notice, or design the rebuilt state as
   the only one because T304 (P2, queued) replays credit in the same save? It depends on whether T304 lands before
   flow 05's build. Operator's call.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the 17 steps** on a fresh database, playing the acts up to each one (`README.md` § How to play): 2.39 and
  2.40; Act 3's 3.6, 3.7, 3.48 and 3.50; 4.40; 5.15 and 5.26; Act 6's 6.15, 6.19, 6.24, 6.27, 6.35, 6.37 and 6.39; and
  the appendix's A.7.3. Every Expect must hold. Steps 2.39, 3.7, 3.48, 3.50, 4.40 and 6.27 quote the pages' words:
  update them in the same task if the wording changes (BRIEF.md § 9 item 7), with every other step that quotes My
  progress or the standing table (A.7.4, A.7.6 and flow 07's review steps among them).
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`). They
  must include:
  - `Progress/QuotaProgressRenderingTests` and `Charts/TrajectoryChartTests`;
  - `Dashboards/` and `Navigation/DashboardLinkAuthorizationTests`, and `NavMenuAuthorizationTests` if question 8
    moves a link;
  - `Accessibility/RowNamesTests` and `ActionFocusTests`; `Design/NarrowLayoutTests`, `DefinedClassTests` and
    `BadgeForStatusTableTests`;
  - every file in `Activities/` that pins My activities or the completed status card;
  - `Scenario/`.
- **Re-capture** the states in § 3 (the `my-progress--*` ones on the new shell at last) and the steps' own captures,
  keeping their names. Compare them with the chosen artboards.
- **Browser check** at 1280 and 390 px as each of the five registrars: Home's figures match My progress's, in the same
  words; every chart's dates and rungs are at least 11 px, or its table shows; nothing scrolls sideways; every link on
  Home has its own name.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as it is today, after flow 04 (re-synced 2026-10-03). `D` is the replay day and `J` the latest 15
January on or before it.

```text
Step 2.39 — Dr Molefe, final-year registrar (act-2-onboarding.md:684)
Role: Trainee — Dr Lerato Molefe
Route: /account/login → / → /portfolio/progress
Do: Sign in, read the dashboard and nav, and open My progress.
Expect: Curriculum targets names the current semester and reads "0 / 10" semester targets met and "0 / 5" yearly targets
  met. The largest shortfalls are PAED-001 to PAED-005, each "0 of 3 this semester", and there is no "started part-way"
  line (D42). The other cards read: Needs you, "Nothing needs you. Requests you have filed are in My activities.", with
  Open My activities (T342); "No activities yet."; "No deadlines in the next 14 days."; and My authorisations. The
  header offers "Log an activity" (T335). The sidebar reads "Acting as Trainee" over Home, Log an activity, My
  activities, MSF reports, My committee reviews and Export portfolio, then My progress and My data rights under the
  rule. My progress reads "0 of 10 EPAs met this semester", "0 of 5 EPAs met in <year>" and Training year "4 — it sets
  the minimum level each encounter is judged against".

Step 2.40 — The other registrars' training years (act-2-onboarding.md:705)
Role: Trainee — Dr Dlamini, Dr du Plessis, Dr Mahlangu and Dr Ndlovu
Route: /account/login → / → /portfolio/progress
Do: Each signs in and opens My progress.
Expect: Each dashboard has Molefe's shape and figures. Training year reads 3 for Dlamini, 2 for du Plessis and 1 for
  Mahlangu and Ndlovu. Their semester and yearly targets apply now: a start on 15 January is on time for both (D42).

Step 3.6 — Dr Dlamini sees the completed Mini-CEX in My activities (act-3-operations.md:210)
Role: Trainee — Dr Anele Dlamini
Route: /activities/mine → /activities/{ActivityId:int}
Do: Open My activities, then the Mini-CEX.
Expect: Needs you is gone. Under All activities the row reads Who has it now "Done", State Completed, Credit "1 item".
  The page is read-only. Its status card, badged Completed, reads "Done. David Naidoo completed it on …" and "Rated 4.
  Credited 1 item to PAED-001.", with Open My progress, a link. Entrustment shows rung 4 chosen, and Feedback Dr
  Naidoo's three texts, "Filled in by David Naidoo, `D`". No move is offered. The history holds Create, Submit and
  Complete, with Dr Naidoo as the actor of the Complete.

Step 3.7 — Dr Dlamini's progress counts the Mini-CEX against this semester's target (act-3-operations.md:226)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress
Do: Open My progress.
Expect: The "This period" card reads:
  - "Semester 2, 2026 · July to November";
  - semester targets "0 of 10 EPAs met this semester" and yearly targets "0 of 5 EPAs met in 2026";
  - training year 3;
  - multi-source feedback "0 of 15 EPAs covered…".
  The PAED-001 card reads "1 of 3 this semester", with a bar, then "2 more by 30 November 2026. At the minimum level when
  observed: 1 of 1. Last encounter date: `D−10`." Below that come "Target: 3 per semester (6 a year). Minimum now 4."
  and "Semester 1, 2026: 0 of 3, 3 short". The Entrustment section shows no STAR yet. The trajectory charts PAED-001:
  1 observation from 1 distinct assessor.

Step 3.48 — Dr Molefe's record and progress show her evidence and the feedback (act-3-operations.md:1080)
Role: Trainee — Dr Lerato Molefe
Route: /activities/mine → /portfolio/progress
Do: Open My activities, then My progress.
Expect: My activities holds her six Completed WBAs and two Multi-Source Feedback (Paediatrics) rows. The MSF rows are
  PAED-010 and PAED-012, encounter date `D` (the day the campaign closed), Recorded, credit "—" (D8). My progress reads:
  - semester targets "1 of 10 EPAs met this semester", and training year 4;
  - multi-source feedback "2 of 15 EPAs covered by a released campaign that closed this semester. MSF is tracked on its
    own and counts towards no target.";
  - PAED-001: "3 of 3 this semester", "Target met for Semester 2, 2026. At the minimum level when observed: 2 of 3.",
    minimum now 5;
  - PAED-012 "2 of 3" and PAED-010 "1 of 3", each with "MSF in Semester 2, 2026: covered by a released campaign that
    closed on …".
  The trajectory charts PAED-001 (3 observations from 3 distinct assessors), PAED-010 and PAED-012, and plots no MSF
  point (D36).

Step 3.50 — Dr Dlamini's dashboard (act-3-operations.md:1124)
Role: Trainee — Dr Anele Dlamini
Route: /
Do: Open the dashboard and read each card.
Expect:
  - Curriculum targets, "Semester 2, 2026 · July to November": "1 / 10" semester targets met and "0 / 5" yearly targets
    met (2026). Below them are the five EPAs furthest short, largest shortfall first: PAED-002, PAED-003, PAED-005,
    PAED-010 and PAED-012, each "0 of 3 this semester".
  - Needs you: "Nothing needs you. Requests you have filed are in My activities.", with Open My activities (T342).
  - Recent activities: her four WBAs, each Completed, in green.
  - Upcoming deadlines: "No deadlines in the next 14 days."
  - My authorisations links to its page, and the header offers "Log an activity" (T335: the Actions card is gone).

Step 4.40 — Dr Molefe reads her progress against Annexure A (act-4-annual-review.md:860)
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/progress
Do: Open My progress and read "Entrustment against Annexure A".
Expect: It says "You are in training year 4 on `D`." It then reads "2 at or above · 1 below · 12 with no decision, of
  15 EPAs". The table shows:
  - PAED-001 and 012 at `5`, each "At or above" the target of `5`, with its exit level Reached;
  - PAED-010 at `4`, "Below" the target of `5`, with its issue and expiry dates.
  The exit rule says it gates nothing. The page reads the same table as the committee's (T166).

Step 5.15 — Dr Molefe leaves My progress open (act-5-graduation.md:384)
Role: Trainee — Dr Lerato Molefe
Route: /account/login → /account/login/submit → / → /portfolio/progress
Do: In her own browser, sign in and open My progress. Leave the tab open through Step 5.19.
Expect: The page shows her programme as still running:
  - "This period", and each EPA's target for it, with progress bars;
  - on each EPA's card, the period before it;
  - "Entrustment against Annexure A", with the exit rule met as in Step 5.6;
  - her rating trajectories.
  Nothing says that her programme has ended.

Step 5.26 — Dr du Plessis sees it counted (act-5-graduation.md:628)
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress
Do: Open My progress and read PAED-002's card for this period.
Expect: This period's count for PAED-002 includes the `D−1` Mini-CEX, shown as the latest encounter. Record the count
  as n.

Step 6.15 — Dr Dlamini reads her progress before the pause (act-6-catalogue.md:366)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress
Do: Read the page. Note these, for Steps 6.19 and 6.24:
  - PAED-012's count this semester (`n`);
  - its count at the minimum level when observed;
  - the rung it names as the minimum now;
  - the "Semester targets" line's count of EPAs (`m`).
Expect: PAED-012 is among the "Each semester" cards, with a target of 3 per semester. PAED-006's card carries the title
  corrected in Step 6.14. The "This period" card reads "Semester targets: … of `m` EPAs met this semester".

Step 6.19 — Dr Dlamini during the pause (act-6-catalogue.md:461)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/mine → /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read her progress and her activities. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without
  saving.
Expect:
  - **Progress.** No PAED-012 card, and the semester targets line counts `m − 1` EPAs. The rating trajectory for
    PAED-012 still charts every rating on it, the one from Step 6.18 included, under the heading "PAED-012 — … (no
    longer in use)" (T255).
  - **My activities.** The Mini-CEX carries "PAED-012 (no longer in use)" under its name, and its Credit reads None.
  - **The picker.** It offers eight EPAs, without PAED-012.

Step 6.24 — Dr Dlamini's credit is counted (act-6-catalogue.md:551)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/mine
Do: Read her progress and her activities.
Expect:
  - PAED-012's card is back, with `n + 1` this semester, and one more at the minimum level when observed than in Step
    6.15. The semester targets line counts `m` EPAs again.
  - The trajectory heading and My activities no longer carry "(no longer in use)", and the Mini-CEX's Credit reads
    1 item.
  - The credit PAED-012 had earned before the pause was kept, not cancelled. Dr Dlamini holds none from Acts 3-4
    (`n` is 0 in Step 6.15), so this is checked on Dr Molefe's PAED-012 progress from Act 3: its row is unchanged
    through the pause (by SQL, or on her record in Step 6.40).

Step 6.27 — Dr Dlamini is measured against KGK-001 (act-6-catalogue.md:631)
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/new → /activities/new?type=direct_observation_cpsa
Do: Read her progress. Then start a Direct Observation (Paediatrics) and open its EPA picker. Leave without saving.
Expect: A KGK-001 card appears under "Once a year", reading 0 of 1 for the current year, and the yearly targets line
  counts one more EPA. Its line for the year before reads "0 of 1, 1 short": a target is read live, into periods that
  have already closed, as the items page warned. The Direct Observation picker offers ten EPAs, KGK-001 among them.

Step 6.35 — Dr Ndlovu notes his figures before he moves (act-6-catalogue.md:796)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page, and note each card's count for its current period.
Expect: Sixteen cards: the 15 national EPAs of 11.1, and KGK-001. PAED-016 is not among them.

Step 6.37 — Dr Ndlovu on 11.2, before progress is rebuilt (act-6-catalogue.md:823)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress → /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read the page. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect:
  - **The cards.** Seventeen: 11.2's 16 national items (PAED-016 once a year, and PAED-011 at 2 per academic year,
    among them) and KGK-001.
  - **The counts.** His encounters before the move are counted in 11.1's tallies, which this page no longer reads. Until
    Step 6.38, each card shows only what 11.2 holds for him, which is nothing yet.
  - **The picker.** The Mini-CEX picker offers ten EPAs, PAED-016 among them.

Step 6.39 — Dr Ndlovu's evidence counts on 11.2 (act-6-catalogue.md:869)
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page.
Expect: Each of the 15 EPAs carried over from 11.1 reads the count he noted in Step 6.35 for its current period.
  PAED-016 and KGK-001 read 0. PAED-011's card measures its count against a target of 2 per academic year.

Step A.7.3 — Dr Dlamini on her phone (appendix-cross-cutting.md:1079)
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My progress, My activities and My data rights from the menu.
Expect: The dashboard's cards stack. On My progress, each EPA's figures and trajectory fit the width. My activities'
  rows stack, each cell but the link and the state labelled by its column (T342), and the table of "Your requests"
  scrolls inside its container; each row's link or action stays reachable.
```
