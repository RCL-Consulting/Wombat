# F05 A registrar reads where they stand

Each of the five registrars, every week, reads her targets, shortfalls, training year, standing and trajectory on Home
and My progress. It is her only warning before the committee judges her in F07.

Written 2026-09-26 for T332, following `design/BRIEF.md` § 2.4. Paths to screenshots are under `design/baseline/`
(gitignored). Claims about Claude Design use BRIEF § 2.0's source keys.

| | |
|---|---|
| People | Trainee: Dr Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa Mahlangu and Dr Sipho Ndlovu (1) (`README.md` § Cast) |
| Pages | `/` (Trainee dashboard, `Components/Pages/Dashboards/TraineeDashboard.razor`), `/portfolio/progress` (`Portfolio/MyProgress.razor`), `/activities/mine` (`Activities/MyActivities.razor`) (`coverage.md` § Pages) |
| Steps | 17: 2.39, 2.40, 3.6, 3.7, 3.48, 3.50, 4.40, 5.15, 5.26, 6.15, 6.19, 6.24, 6.27, 6.35, 6.37, 6.39, A.7.3 |
| Frequency and stakes | Weekly. Medium-high: a registrar short of a target learns it here or at her review. |
| Mode | **Wireframe first** for the Trainee dashboard and My progress (what leads each), then fidelity (BRIEF § 2.3, step 3). Charts must stay readable at 390 px. |
| Held | **Nothing.** T297 (group 1, landed in 7bf8ea7) changed the Trainee dashboard's Activity inbox card; all ten of its captures were re-captured on 2026-09-26 (§ 5.3; BRIEF § 10). My progress and My activities are not changed by any group-1 task. |

---

## 1. How to run this thread

1. Run F01 (the shell) first; this flow sits inside its frame (BRIEF § 2.3, step 2).
2. T297 has landed and the ten dashboard captures (§ 5.3) were re-taken on 2026-09-26, so all four screens can run.
3. Start a new thread. Attach the key screenshots in § 5.1, then paste § 2.1 as one message.
4. Pick a wireframe for screens 1 and 2. Then paste § 2.2 for fidelity.
5. Attach the state captures in § 5.2 as the chat asks for them.
6. Answer its clarifying questions in the chat, one decision per sentence: the chat travels in the handoff bundle
   [academy].
7. Export the chosen artboards to `design/flows/05-trainee-progress/` before moving on. There is no version history
   [start].

## 2. The ask

### 2.1 Paste this first, with the key screenshots attached

```
FLOW 05 — Know where I stand before the committee judges me

GOAL: A registrar opens Wombat once a week and must learn, in under a minute:
- this semester's observation targets and how many she has met;
- this academic year's targets;
- her training year, which sets the minimum level each encounter is judged against;
- which EPAs are short, by how many, and by when;
- her entrustment standing: the STAR level the committee gave each EPA, against her training year's target and the
  College's exit level;
- her rating trajectory per EPA.
She must also read correctly three catalogue events: an EPA the College has paused, an item her own institution added,
and her counts after she is moved to a newer curriculum version. This page is her only warning before the committee
sits.

What is wrong today:
- The training year is shown only on My progress, never on the dashboard.
- My progress is one long page: at 1280 px it is 4,500–5,000 px tall, at 390 px about 11,200 px. Its order is fixed:
  the "This period" card, ten "Each semester" cards, five "Once a year" cards, the standing table, and the trajectory
  charts last. Every card repeats two sentences about multi-source feedback (MSF).
- At 390 px each trajectory chart is a 600×200 drawing scaled to about 260 px, so its axis labels are 5–6 px tall.
- The dashboard's "Upcoming deadlines" card reads a field no instrument has, so it is always empty.
- On the dashboard a status badge stretches into a tall pill when the link beside it wraps. Its links are named only
  by the instrument, so "Mini-CEX (Paediatrics)" appears twice with nothing to tell them apart.

AUDIENCE: Trainee. The five paediatric registrars at Kgosi Kgari Teaching Hospital (KGK), on the College's (CPSA)
curriculum 11.1: Dr Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa
Mahlangu and Dr Sipho Ndlovu (1). Doctors reading between patients, often on a phone. Desktop 1280×800 and phone
390×844.

THREE DIFFERENT "PER"S THE DESIGN MUST KEEP APART:
- Observation targets, per window (Annexure B): ten EPAs have a target per semester (PAED-001–005, 010 and 012 at 3;
  006 at 2; 007 and 015 at 1), and five have one per academic year (008, 009, 011, 013, 014). Semester 1 is January to
  June; semester 2 is July to November, and December counts into it.
- Decision cadence: when the committee decides the EPA. Each semester for PAED-001, 002, 004, 005, 010 and 012; each
  academic year for the rest. So PAED-003 has a per-semester observation target but is decided once a year.
- Level targets, per training year (Annexure A): the minimum level for her training year (for example 3a, 3b, 4, 5 in
  years 1–4), and the exit level (5 or 4).
Figures are always "n of m" for a named window ("1 of 3 this semester", "0 of 1 in 2026"). Never a lifetime total and
never a percentage. "Training year N" and "Semester 2, 2026" / "2026 academic year" are different things, and the bare
word "year" is never used beside both.

WHAT THE PAGES CARRY TODAY (real figures from Step 3.7, Dr Dlamini, training year 3):
- "This period": "Semester 2, 2026 · July to November"; "0 of 10 EPAs met this semester"; "0 of 5 EPAs met in 2026";
  "Training year 3 — it sets the minimum level each encounter is judged against"; "0 of 15 EPAs covered by a released
  campaign that closed this semester. MSF is tracked on its own and counts towards no target."
- An EPA card, PAED-001: "1 of 3 this semester", a bar, "2 more by 30 November 2026. At the minimum level when observed:
  1 of 1. Last encounter date: 2026-09-16.", "Target: 3 per semester (6 a year). Minimum now 4.", "Semester 1, 2026: 0
  of 3, 3 short", then two MSF lines, one per semester. Yearly cards add "Your training year changed on 14 Jan 2026".
- "Entrustment against Annexure A" (Step 4.40, Dr Molefe): "You are in training year 4 on 26 September 2026."; "2 at or
  above · 1 below · 12 with no decision, of 15 EPAs"; the exit rule "2 of 15 EPAs at their exit level by STAR decision
  (level 5: 2 of 9 · level 4: 0 of 6)", marked "For information only"; then a six-column table: EPA, year N target,
  STAR decision, against target (a badge: At or above, Below, No decision, Not comparable), exit level (Reached or Not
  yet), latest rating.
- "Rating trajectory": one chart per EPA rated by a named assessor, on the six-rung ladder 1, 2, 3a, 3b, 4, 5, headed
  by the EPA and "3 observations from 3 distinct assessors". MSF is never plotted. Each chart has a table (date,
  level, assessor) that only a screen reader can reach.
- The dashboard (Step 3.50): "Curriculum targets" (the period, "1 / 10" semester targets met, "0 / 5" yearly, and the
  five EPAs furthest short, largest shortfall first), "Activity inbox", "Recent activities" (each with a state badge),
  "Upcoming deadlines", "My authorisations" (the only way in to her STARs: the nav has no link), and "Actions" (Log an
  activity, Request an assessment).
- Data volumes: none (first day, all zeros); typical (15 EPAs, 3 trajectories, 6 activities); heavy (17 EPA cards after
  a version move with a local item; 15 STAR decisions at graduation).

MODE: Wireframe first for screens 1 and 2 — decide what leads each page and what the dashboard holds that My progress
does not — then full fidelity for the variation I pick.

SCREENS, in order:
1. / — the Trainee dashboard: the first day (all zeros), a typical week, and 390 px. Cards: Curriculum targets (with
   the training year), the Activity inbox, Recent activities, My authorisations, Actions, and a decision on MSF (the
   dashboard shows none today) and on Upcoming deadlines (remove it, or give it a real source).
2. /portfolio/progress — My progress: first visit, counting, with MSF coverage, with entrustment standing, exit rule
   met, a paused EPA, a local (institution) item, before and after a curriculum rebuild, loading, load error, 390 px.
3. The rating trajectory: one EPA's chart, at 1280 and 390 px, and its accessible table shown as a visible
   alternative. The same component draws the committee's review page (Flow 07), so design it once.
4. /activities/mine — My activities filtered to MSF records (credited "—") and to a paused EPA (marked "(no longer in
   use)", credited None).

STEPS: 2.39, 2.40, 3.6, 3.7, 3.48, 3.50, 4.40, 5.15, 5.26, 6.15, 6.19, 6.24, 6.27, 6.35, 6.37, 6.39 and A.7.3, pasted
verbatim at the end of this message (Role / Route / Do / Expect). In them, D is the day the story is replayed and J the
latest 15 January on or before D; the story's dates are narrative.

STATES TO SHOW:
- Dashboard: first day; typical; 390 px; loading; nothing assigned yet ("No curriculum assigned yet").
- My progress: first visit (all zeros); one encounter counted; target met, with MSF coverage; standing with STARs; exit
  rule met ("15 of 15 … Every EPA is at its exit level."); a paused EPA; an institution's own item; after a version
  move, before and after the rebuild; loading (skeleton under the header); load error (a plain sentence, the header
  kept); 390 px.
- My progress states no replay reaches, which you must still draw: "No curriculum items assigned yet"; "Your programme
  starts on …"; "You started part-way through a period" (the target is waived for that window: show the count and the
  date targets start, never a fraction or a bar); "No EPA on your curriculum is in use"; an activity counted towards
  the number of observations but not towards the level (a different scale); the December notice ("The 2026 academic
  year ended on 30 November. …").
- My activities: MSF rows; a paused EPA's row.

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
- T306: "Training year N" on the dashboard's Curriculum targets card, worded as on My progress.
- T323: no chart text below 11 px at 390 px, or the chart's table is visible under it at phone width.
- T298: the Upcoming deadlines card is removed, or it shows a real date: the current window's end for an unmet
  target, a committee review scheduled for her, or a STAR nearing expiry.
- T328: badges keep their pill shape beside a wrapping link; controls use the body font.
- T280: every dashboard link has a unique name (the instrument plus the EPA or the encounter date).
- T297: the dashboard's Activity inbox card lists exactly what /activities/inbox lists for her. A declined request
  shows on Recent activities with its badge, not in the inbox card.
- T312: an EPA that is no longer in use keeps its place where it is a record, headed "PAED-012 — … (no longer in use)".
  Design the mark for a card heading as well as a chart heading.
- T322: every badge and alert meets 4.5:1 text contrast. The standing badges already put body text on a tint; follow
  them.
- T329: the header renders from the first moment; a skeleton shows while the page loads; a load error is a sentence
  under the header with no empty state beneath it.

QUESTIONS THE DESIGN MUST ANSWER:
1. What leads My progress: this semester's shortfall, the standing per EPA, or the trajectory?
2. How are per-semester observation targets, per-year decisions and per-training-year levels shown together without
   confusing them?
3. What does the dashboard hold that My progress does not, and the other way round? Does MSF appear on the dashboard?
4. How are a paused EPA, an institution's own item (for example KGK-001, "not in the exit rule") and an item from a
   newer curriculum version (PAED-016 on 11.2) marked?

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
or HTML with no script. Every chart needs a text or table equivalent.

ASK: 2–3 variations. Wireframes first for screens 1 and 2, then full fidelity. Name every design-system component you
use and mark anything else NEW. Say which DESIGN.md rule a variation breaks. Flag edge cases and draw the none, typical
and heavy data volumes. Review the result for accessibility against WCAG 2.1 AA.

ATTACHED: states/my-progress--first.png, act-3/3.7-1-dlamini-progress.png, states/my-progress--standing.png,
states/my-progress--paused-epa.png, states/my-progress--after-rebuild.png, states/my-progress--narrow.png,
states/review-detail--narrow.png, act-2/2.39-2-molefe-progress.png, and the post-T297
states/home--trainee-first.png, states/home--trainee.png and states/home--narrow-trainee.png.

RUNBOOK STEPS, VERBATIM:

--- from execution/knowledge/scenario-paediatrics/act-2-onboarding.md ---

### Step 2.39 — Dr Molefe, final-year registrar
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

### Step 2.40 — The other registrars' training years
Role: Trainee — Dr Dlamini, Dr du Plessis, Dr Mahlangu and Dr Ndlovu
Route: /account/login → / → /portfolio/progress
Do: Each signs in and opens My progress.
Expect: Each dashboard has Molefe's shape and figures. Training year reads 3 for Dlamini, 2 for du Plessis and 1 for
  Mahlangu and Ndlovu. Their semester and yearly targets apply now: a start on 15 January is on time for both (D42).

--- from execution/knowledge/scenario-paediatrics/act-3-operations.md ---

### Step 3.6 — Dr Dlamini sees the completed Mini-CEX in My Activities
Role: Trainee — Dr Anele Dlamini
Route: /activities/mine → /activities/{ActivityId:int}
Do: Open My activities, then the Mini-CEX.
Expect: Needs you is gone. Under All activities the row reads Who has it now "Done", State Completed, Credit "1 item".
  The page is read-only. Its status card, badged Completed, reads "Done. David Naidoo completed it on …" and "Rated 4.
  Credited 1 item to PAED-001.", with Open My progress, a link. Entrustment shows rung 4 chosen, and Feedback Dr
  Naidoo's three texts, "Filled in by David Naidoo, `D`". No move is offered. The history holds Create, Submit and
  Complete, with Dr Naidoo as the actor of the Complete.

### Step 3.7 — Dr Dlamini's progress counts the Mini-CEX against this semester's target
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

### Step 3.48 — Dr Molefe's record and progress show her evidence and the feedback
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

### Step 3.50 — Dr Dlamini's dashboard
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

--- from execution/knowledge/scenario-paediatrics/act-4-annual-review.md ---

### Step 4.40 — Dr Molefe reads her progress against Annexure A
Role: Trainee — Dr Lerato Molefe
Route: /portfolio/progress
Do: Open My progress and read "Entrustment against Annexure A".
Expect: It says "You are in training year 4 on `D`." It then reads "2 at or above · 1 below · 12 with no decision, of
  15 EPAs". The table shows:
  - PAED-001 and 012 at `5`, each "At or above" the target of `5`, with its exit level Reached;
  - PAED-010 at `4`, "Below" the target of `5`, with its issue and expiry dates.
  The exit rule says it gates nothing. The page reads the same table as the committee's (T166).

--- from execution/knowledge/scenario-paediatrics/act-5-graduation.md ---

### Step 5.15 — Dr Molefe leaves My progress open
Role: Trainee — Dr Lerato Molefe
Route: /account/login → /account/login/submit → / → /portfolio/progress
Do: In her own browser, sign in and open My progress. Leave the tab open through Step 5.19.
Expect: The page shows her programme as still running:
  - "This period", and each EPA's target for it, with progress bars;
  - on each EPA's card, the period before it;
  - "Entrustment against Annexure A", with the exit rule met as in Step 5.6;
  - her rating trajectories.
  Nothing says that her programme has ended.

### Step 5.26 — Dr du Plessis sees it counted
Role: Trainee — Dr Pieter du Plessis
Route: /portfolio/progress
Do: Open My progress and read PAED-002's card for this period.
Expect: This period's count for PAED-002 includes the `D−1` Mini-CEX, shown as the latest encounter. Record the count
  as n.

--- from execution/knowledge/scenario-paediatrics/act-6-catalogue.md ---

### Step 6.15 — Dr Dlamini reads her progress before the pause
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress
Do: Read the page. Note these, for Steps 6.19 and 6.24:
  - PAED-012's count this semester (`n`);
  - its count at the minimum level when observed;
  - the rung it names as the minimum now;
  - the "Semester targets" line's count of EPAs (`m`).
Expect: PAED-012 is among the "Each semester" cards, with a target of 3 per semester. PAED-006's card carries the title
  corrected in Step 6.14. The "This period" card reads "Semester targets: … of `m` EPAs met this semester".

### Step 6.19 — Dr Dlamini during the pause
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

### Step 6.24 — Dr Dlamini's credit is counted
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

### Step 6.27 — Dr Dlamini is measured against KGK-001
Role: Trainee — Dr Anele Dlamini
Route: /portfolio/progress → /activities/new → /activities/new?type=direct_observation_cpsa
Do: Read her progress. Then start a Direct Observation (Paediatrics) and open its EPA picker. Leave without saving.
Expect: A KGK-001 card appears under "Once a year", reading 0 of 1 for the current year, and the yearly targets line
  counts one more EPA. Its line for the year before reads "0 of 1, 1 short": a target is read live, into periods that
  have already closed, as the items page warned. The Direct Observation picker offers ten EPAs, KGK-001 among them.

### Step 6.35 — Dr Ndlovu notes his figures before he moves
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page, and note each card's count for its current period.
Expect: Sixteen cards: the 15 national EPAs of 11.1, and KGK-001. PAED-016 is not among them.

### Step 6.37 — Dr Ndlovu on 11.2, before progress is rebuilt
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress → /activities/new → /activities/new?type=mini_cex_cpsa
Do: Read the page. Then start a Mini-CEX (Paediatrics) and open its EPA picker. Leave without saving.
Expect:
  - **The cards.** Seventeen: 11.2's 16 national items (PAED-016 once a year, and PAED-011 at 2 per academic year,
    among them) and KGK-001.
  - **The counts.** His encounters before the move are counted in 11.1's tallies, which this page no longer reads. Until
    Step 6.38, each card shows only what 11.2 holds for him, which is nothing yet.
  - **The picker.** The Mini-CEX picker offers ten EPAs, PAED-016 among them.

### Step 6.39 — Dr Ndlovu's evidence counts on 11.2
Role: Trainee — Dr Sipho Ndlovu
Route: /portfolio/progress
Do: Read the page.
Expect: Each of the 15 EPAs carried over from 11.1 reads the count he noted in Step 6.35 for its current period.
  PAED-016 and KGK-001 read 0. PAED-011's card measures its count against a target of 2 per academic year.

--- from execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md ---

### Step A.7.3 — Dr Dlamini on her phone
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My progress, My activities and My data rights from the menu.
Expect: The dashboard's cards stack. On My progress, each EPA's figures and trajectory fit the width. My activities'
  rows stack, each cell but the link and the state labelled by its column (T342), and the table of "Your requests"
  scrolls inside its container; each row's link or action stays reachable.
```

### 2.2 After you pick a wireframe, paste this

```
Take variation <letter> of screens 1 and 2 to full fidelity at 1280×800 and 390×844, in the design system you hold.
Draw every state listed under STATES TO SHOW, including the ones no replay reaches. Draw the trajectory (screen 3) at
both widths with its visible table, and My activities (screen 4) filtered to MSF and to a paused EPA. For each
artboard, list the components used and mark NEW ones. For each colour, name the token, or give a NEW token and its
value. Then check contrast (text 4.5:1; borders and the focus ring 3:1) and every target (at least 24 px).
```

## 3. The journey

One line per step: what the person does, and what she must be able to see. The step's full text is in § 2.1.

| Step | Page template | Who | Does, and must see |
|---|---|---|---|
| 2.39 | `/account/login` → `/` → `/portfolio/progress` | Dr Molefe | First sign-in after admission. Sees 0/10 semester and 0/5 yearly targets, the five largest shortfalls, and her training year, 4. Today the year is on My progress only (T306). |
| 2.40 | `/account/login` → `/` → `/portfolio/progress` | Dr Dlamini, Dr du Plessis, Dr Mahlangu, Dr Ndlovu | The same shape. Training years 3, 2, 1 and 1. No "started part-way" line: a 15 January start is on the boundary (D42). |
| 3.6 | `/activities/mine` → `/activities/{ActivityId:int}` | Dr Dlamini | Finds her completed Mini-CEX, "Completed", credited "1 item", and opens it read-only. The activity page itself is F03's and F04's. |
| 3.7 | `/portfolio/progress` | Dr Dlamini | PAED-001 "1 of 3 this semester", what is still owed and by when, the minimum now, last semester's shortfall, and a trajectory of one point. |
| 3.48 | `/activities/mine` → `/portfolio/progress` | Dr Molefe | Six WBAs and two MSF rows credited "—" (MSF counts towards no target, D8). PAED-001's target met. MSF coverage 2 of 15. The trajectory plots no MSF point (D36). |
| 3.50 | `/` | Dr Dlamini | The dashboard: 1/10 and 0/5, the five EPAs furthest short, Recent activities, the empty deadlines card, My authorisations and Actions. |
| 4.40 | `/portfolio/progress` | Dr Molefe | After her review: "Entrustment against Annexure A", 2 at or above, 1 below, 12 with no decision; the exit rule, which gates nothing. |
| 5.15 | `/account/login` → `/account/login/submit` → `/` → `/portfolio/progress` | Dr Molefe | Near the end: the exit rule met, every EPA at its exit level, the programme still running. Nothing says it has ended. |
| 5.26 | `/portfolio/progress` | Dr du Plessis | PAED-002's count includes yesterday's Mini-CEX, shown as the latest encounter. |
| 6.15 | `/portfolio/progress` | Dr Dlamini | Before a College pause: PAED-012's count and the semester line, "1 of 10". |
| 6.19 | `/portfolio/progress` → `/activities/mine` → `/activities/new` | Dr Dlamini | During the pause: no PAED-012 card and "1 of 9". The trajectory is headed "(no longer in use)", and My activities marks the Mini-CEX and credits None. The picker (F03) offers eight EPAs. |
| 6.24 | `/portfolio/progress` → `/activities/mine` | Dr Dlamini | Restored: the card is back with the new count, the marks are gone, and it is credited "1 item" (D48). |
| 6.27 | `/portfolio/progress` → `/activities/new` | Dr Dlamini | KGK's own item, KGK-001, appears under "Once a year" at 0 of 1, with the year before "1 short". The standing table calls it "The institution's own EPA; not in the exit rule". |
| 6.35 | `/portfolio/progress` | Dr Ndlovu | Sixteen cards on 11.1, before he is moved. |
| 6.37 | `/portfolio/progress` → `/activities/new` | Dr Ndlovu | Moved to 11.2: seventeen cards, all at 0. Nothing says his earlier evidence is not yet counted (T304). |
| 6.39 | `/portfolio/progress` | Dr Ndlovu | After the rebuild: his counts are back, PAED-016 and KGK-001 read 0, and PAED-011 counts against 2 per academic year. |
| A.7.3 | `/` → `/portfolio/progress` → `/activities/mine` → `/account/data-rights` | Dr Dlamini | At 390 px: cards stack, the charts fit only by shrinking (labels ~5 px), tables scroll inside their containers, and each row action stays reachable. |

## 4. States to design

From `execution/knowledge/scenario-paediatrics/states.md` § Home and the role dashboards, § Portfolio and § Activities.
Each file is `design/baseline/states/<page>--<state>.png`.

| Page | State | Capture | Reached at |
|---|---|---|---|
| `/` | Admitted, nothing filed | `home--trainee-first.png` (re-captured after T297) | Step 2.39 |
| `/` | With activity | `home--trainee.png` (re-captured after T297) | Step 3.50 |
| `/` | 390 px | `home--narrow-trainee.png` (re-captured after T297) | Step A.7.3 |
| `/portfolio/progress` | Admitted, nothing counted | `my-progress--first.png` | Step 2.39 |
| `/portfolio/progress` | One encounter counted | `my-progress--counting.png` | Step 3.7 |
| `/portfolio/progress` | Target met, with MSF coverage | `my-progress--msf.png` | Step 3.48 |
| `/portfolio/progress` | Standing with STARs | `my-progress--standing.png` | Step 4.40 |
| `/portfolio/progress` | Exit rule met | `my-progress--exit-met.png` | Step 5.15 |
| `/portfolio/progress` | A paused EPA | `my-progress--paused-epa.png` | Step 6.19 |
| `/portfolio/progress` | An institution's own item | `my-progress--local-item.png` | Step 6.27 |
| `/portfolio/progress` | New version, before the rebuild | `my-progress--before-rebuild.png` | Step 6.37 |
| `/portfolio/progress` | New version, after the rebuild | `my-progress--after-rebuild.png` | Step 6.39 |
| `/portfolio/progress` | Loading | `my-progress--loading.png` | A held read (`states.md` § Holding a read) |
| `/portfolio/progress` | Load error | `my-progress--load-error.png` | The same, held 35 s |
| `/portfolio/progress` | 390 px | `my-progress--narrow.png` | Step A.7.3 |
| `/activities/mine` | With MSF records | `my-activities--msf.png` | Step 3.48 |
| `/activities/mine` | A paused EPA | `my-activities--paused-epa.png` | Step 6.19 |

**Not captured, to be drawn from their description** (`states.md:762-771`; BRIEF § 10):
- The dashboard's "Upcoming deadlines" with a row. Nothing can fill it today (T298).
- "No curriculum assigned yet" on Home and "No curriculum items assigned yet" on My progress. Admission sets the role
  and the profile together, so no cast member meets them.
- "Your programme starts on …" (every start is on or before `D`), and "You started part-way through a period" (every
  start is 15 January, the semester boundary, D42).
- "No EPA on your curriculum is in use" (every EPA of a curriculum paused at once).
- An activity counted across scales: "counted towards the required number of observations, but not towards the
  supervision level". Every rated instrument uses the v11.1 ladder.
- `my-progress--december`: the December notice, shown only when `D` is in December.

**Belongs to other flows:** the ended-programme states (`my-progress--completed`, `--withdrawn`, `--narrow-former`,
`home--trainee-ended`) are F13's; `home--trainee-returned` and the activity page are F03's.

## 5. Attach

Every path below is under `design/baseline/` and was checked with `ls` on 2026-09-26. Open each before you upload it
(BRIEF § 3.3). None of them is an invitation page, so none shows a registration link.

### 5.1 Key screenshots (attach with the ask)

1. `states/my-progress--first.png`: the whole page on the first day.
2. `act-3/3.7-1-dlamini-progress.png`: the whole page, counting (the figures quoted in § 2.1).
3. `states/my-progress--standing.png`: the Annexure A standing table.
4. `states/my-progress--paused-epa.png`: a paused EPA.
5. `states/my-progress--after-rebuild.png`: seventeen cards on 11.2.
6. `states/my-progress--narrow.png`: the page at 390 px.
7. `states/review-detail--narrow.png`: at its foot, three trajectory charts at 375 px, labels about 5 px.
8. `act-2/2.39-2-molefe-progress.png`: "Training year 4" on My progress.

### 5.2 State captures (add as the chat asks)

- My progress:
  - `states/my-progress--counting.png`
  - `states/my-progress--msf.png`
  - `states/my-progress--exit-met.png`
  - `states/my-progress--local-item.png`
  - `states/my-progress--before-rebuild.png`
  - `states/my-progress--loading.png`
  - `states/my-progress--load-error.png`
- My activities:
  - `states/my-activities--msf.png`
  - `states/my-activities--paused-epa.png`
- Step captures, not held, useful for detail:
  - `act-3/3.48-2-molefe-progress.png`
  - `act-4/4.40-1-molefe-progress-annexure-a.png`
  - `act-5/5.15-1-molefe-progress-running.png`
  - `act-6/6.19-1-dlamini-progress.png`
  - `act-6/6.19-2-dlamini-my-activities.png`
  - `act-6/6.27-1-dlamini-progress.png`
  - `act-6/6.37-1-ndlovu-progress.png`
  - `act-A/A.7.3-3-progress-390.png`
  - `act-A/A.7.3-4-activities-390.png`
  - `act-3/3.6-1-my-activities-completed.png`

### 5.3 Re-captured after T297 (formerly held)

T297 changed the Trainee dashboard's Activity inbox card (BRIEF § 10). Every capture below was re-taken on 2026-09-26
after it landed, and again on 2026-09-27 by the T335 replay, so none is held; attach the three state captures:

`states/home--trainee-first.png`, `states/home--trainee.png`, `states/home--narrow-trainee.png`,
`act-A/A.7.3-1-home-390.png`, `act-2/2.39-1-molefe-home.png`,
`act-2/2.40-1-dlamini-home.png`, `act-2/2.40-3-duplessis-home.png`, `act-2/2.40-5-mahlangu-home.png`,
`act-2/2.40-7-ndlovu-home.png`

The T335 replay took the step captures at their own moments. `act-3/3.50-1-dlamini-home.png` comes from that fresh
replay (Step 3.50: the Activity inbox card reads "No pending items." beside an inbox reading "Inbox
clear").

## 6. Known problems this design must solve

| Task | Problem today | What it means for the design | Evidence |
|---|---|---|---|
| T306 (P3; BRIEF B4) | The training year is on My progress only. `TraineeDashboard.razor:26` (Curriculum targets) shows none. | The Curriculum targets card names "Training year N". Word it "training year N" (DESIGN.md:738, D17). | `act-2/2.39-2-molefe-progress.png`; the dashboard in `act-2/2.39-1-molefe-home.png` (re-captured after T297) |
| T323 (P3; BRIEF A3) | `TrajectoryChart.razor` draws a 600×200 viewBox with 11 px labels. At 260–293 px they render at 5–6 px. The companion table is `.visually-hidden` at every width. | Chart text is at least 11 px at 390 px, or the table is visible under each chart below 641 px (T323 recommends the table). Record the choice in DESIGN. | `states/review-detail--narrow.png` (its foot, at 375 px); Step A.7.3's Actual (charts 293 px wide in a 341 px column) |
| T298 (P3; BRIEF B13) | "Upcoming deadlines" scans each activity's data for a `due_date` key that no seeded or KGK type has (`GetTraineeDashboardSummaryQuery.cs:111-148`). It always reads "No deadlines in the next 14 days." | Remove the card, or give it one of these sources: the current window's end for an unmet target, a committee review scheduled for her, or a STAR nearing expiry. A card that can never fill is not acceptable. | `act-3/3.50-1-dlamini-home.png` (re-captured after T297) |
| T328 (P3; BRIEF A4) | Dashboard rows are inline `display:flex` (`TraineeDashboard.razor:101,125`), so a badge takes the row's height when the link wraps. Form controls use the browser's font. | A `.list-row` pattern: the badge keeps its pill (`align-self: center`, no wrap). Controls use the body font. | `states/home--narrow-trainee.png` (re-taken after T335: the badges keep their pill now) |
| T280 (P3; BRIEF A14) | Inbox, recent and deadline links are named only by the instrument: "Mini-CEX (Paediatrics)" twice on one card. | Each link carries the EPA or the encounter date, so its name is unique. | `act-3/3.50-1-dlamini-home.png` (re-captured after T297) |
| T297 (group 1, landed in 7bf8ea7) | Before the fix, the Activity inbox card listed "requested, accepted, declined or draft", so it showed a declined request, which has no move left, and missed work waiting in "submitted". | The card lists exactly what `/activities/inbox` lists for her. A declined request is on Recent activities with its badge. | BRIEF § 10; § 5.3, re-captured 2026-09-26 |
| T312 (P2, backend) | An ended record reads today's catalogue, so a paused EPA vanishes from a graduate's record. | After T312, an ended record keeps a paused EPA's card, headed "(no longer in use)" (`EpaLabel`, T255). A running trainee still loses the card (T158) but keeps the trajectory with the mark (Step 6.19). Design the mark for card headings and chart headings. The ended record itself is F13's. | `act-6/6.21-1-molefe-progress.png` (F13) |
| T304 (P2, backend) | Moving a registrar to 11.2 replays none of his credit. His cards read 0 until the Administrator's global rebuild, and nothing says why (Step 6.37, F-6.37a). | T304's plan replays credit in the same save as the move, so the before-rebuild zeros go away. Design the after-rebuild state as the normal one. Do not design a "counts await a rebuild" notice unless T304 is rejected (inference from T304's plan). | `states/my-progress--before-rebuild.png` |
| T329 (P2; BRIEF A6) | My progress already shows its header and a skeleton. The load error prints the driver's text, "An exception has been raised that is likely due to a transient failure." | Keep the header and skeleton. The error is one plain sentence under the header, with nothing empty drawn beneath it; the wording is T272's (queued). | `states/my-progress--loading.png`, `states/my-progress--load-error.png` |
| T322 (P2; BRIEF A1) | `.badge-completed` text is 2.55:1 on its tint. The dashboard's green "Completed" badges fail. | Badges put body text on the tint, as `.badge-standing-*` already does (DESIGN.md:880-900). | `act-A/A.7.14-7-review-7-badges.png` (re-taken after T335's tokens: the badges now pass) |
| Observed, not filed | Every quota card repeats two MSF sentences. This is by design: one line per semester, in `MsfCoverageText`'s words (DESIGN.md:940-947). | Change how it is shown if you like, but keep the words. A covering campaign is never "about" an EPA (D9), and MSF counts towards no target (D8). | `act-3/3.7-1-dlamini-progress.png` |
| Observed, not filed | One card prints "30 November 2026" and "2026-09-16" side by side. | Choose one date format for people to read. | Step 3.7's Actual |

## 7. Questions the design must answer

1. **What leads My progress:** this semester's shortfall, the standing per EPA, or the trajectory? Today the order is
   fixed, and the trajectory starts about 4,600 px down at 1280 px (`act-3/3.7-1-dlamini-progress.png`).
2. **How are the three "per"s shown together without confusion?**
   - Per-semester observation targets (Annexure B, D39).
   - Per-semester or per-year decision cadence (`act-1-setup.md:382-396`).
   - Per-training-year level targets (Annexure A; D17's two named concepts).
3. **What does the dashboard hold that My progress does not, and the other way round?**
   - The dashboard alone has the inbox, recent activities, the way in to My authorisations, and the Actions.
   - My progress alone has the standing and the trajectory.
   - Does MSF coverage belong on the dashboard?
   - Does My authorisations get a nav link (`coverage.md` § Reached only by address)?
4. **How is each of these marked?**
   - A paused EPA: "(no longer in use)" (T255, D48).
   - An institution's own item: KGK-001, "The institution's own EPA; not in the exit rule" (Step 6.27).
   - An item that exists only in a newer curriculum version: PAED-016 on 11.2 (Step 6.37).

## 8. Acceptance

Claude Code builds the chosen design as one task (BRIEF § 9). It is done when all of the following hold.

**Tests.**
- `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green. Never use `--no-build` (CLAUDE.md § Testing).
- The run includes the tests that pin these pages:
  - `Progress/QuotaProgressRenderingTests` and `Charts/TrajectoryChartTests`;
  - `Dashboards/*` and `Navigation/DashboardLinkAuthorizationTests`;
  - `Design/NarrowLayoutTests`, `Design/DefinedClassTests` and `Design/BadgeForStatusTableTests`;
  - the scenario guard in `Scenario/`.

**The replay.**
- Replay the flow's steps on a fresh database (`tools/scenario-replay.ps1 create` / `publish` / `start`, BRIEF § 9):
  - 2.39 and 2.40 (Act 2);
  - 3.6, 3.7, 3.48 and 3.50 (Act 3);
  - 4.40 (Act 4);
  - 5.15 and 5.26 (Act 5);
  - 6.15, 6.19, 6.24, 6.27, 6.35, 6.37 and 6.39 (Act 6);
  - A.7.3 (appendix).
- Each act may instead start from its predecessor's snapshot, `recovery/scenario-post-act<N>.dump` (README § How to
  play, "Snapshots"). Every Expect must hold.
- If the design changes on-screen wording, update the runbook's Expect lines in the same task. Steps 2.39, 3.7, 3.48
  and 3.50 quote the screen.

**Checks the design adds.**
- Step 2.39: Dr Molefe's Home reads "Training year 4" (T306).
- Step 3.50: Home shows no "Upcoming deadlines" card, or shows a row from the chosen source (T298).
- Steps A.7.3 and A.7.6 at 390 px:
  - every trajectory's dates and levels are at least 11 px, or its table is visible (T323);
  - no badge stretches (T328);
  - no page scrolls sideways.
- Every link on the Trainee dashboard has a unique accessible name (T280).

**Re-capture.** Take the 17 state captures in § 4 and this flow's step captures again into `design/baseline/`
(`states.md` § How to capture). Compare them with the chosen artboards.

**Browser check.** Check as each of the five registrars at 1280 px and at 390 px.
