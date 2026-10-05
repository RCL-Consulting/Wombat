# F04 An assessor works their inbox

**Who and why:** a consultant sees what waits for them, rates it or declines it with a reason, returns a reflection and
records the discussion, often on a phone between patients (Step A.7.2). Nothing emails an assessor when a request
arrives (T320): until the nudge mails after five days untouched (Step 3.32), Home and the inbox are the only notice. An
unseen request stalls a registrar (Steps 3.29–3.33). This is the other half of flow 03: a registrar's request, filed
there, waits here.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked W in BRIEF.md § 8 for the Assessor's Home and the inbox: the structure is expected to change, so round 1 asks for 2–3 structural variations there. **The activity page is fidelity:** flow 03 designed it (see "What flow 04 may change of flow 03's page" in § 1). |
| Mode | **Structure first** (BRIEF.md § 2.3, as § 11 corrected it). Round 1: 2–3 structural variations as wireframes of Home, the inbox and the way between them and the activity page, each with its reasoning and its step counts, plus the assessor's activity page drawn once in flow 03's vocabulary. Round 2, after the pick: fidelity with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | 2–3 in round 1 (Home and the inbox); the chosen one in round 2 |
| People | Assessor only: Dr Mohammed Patel and Dr Fatima Khumalo. Committee member and Assessor: Dr Thandi Zulu (the chair), Dr David Naidoo and Dr Sarah Botha, who switch views |
| Runbook steps | 17, pasted verbatim in § 8 |
| Pages (`coverage.md` templates) | `/`, `/activities/inbox`, `/activities/{ActivityId:int}`; the endpoint `/dashboard/switch/{role}` (a two-role consultant switches to the Assessor view). `/placeholder/{Feature}` is gone (T335 deleted the page and its template), and with it Recent activities (Step 3.51). Checked at `53d9322d`. |
| Components drawn at runtime | `Components/Pages/Dashboards/AssessorDashboard.razor` (in `DashboardFrame`, `DashboardCard`); `Pages/Activities/ActivityInbox.razor` (`StatePanel`, `DataTable`); `Pages/Activities/ActivityView.razor` with `Shared/Activities/ActivityStatus.razor`, `ActivityAbout.razor`, `ActivityForm.razor` (and `RungRow.razor`), `ActivityWorkflowActions.razor`, `RefusalSummary.razor`, `ActivityHistory.razor`; `ActivityRowNames.cs`, `ActivityEpaLabel.razor`, `EncounterDateLabel.razor` |
| Held | None: no group-1 task changes these pages |
| Depends on | Flow 01, built (`b347e11c`): the shell, the acting role and its switch, Home's frame, `DashboardFrame`. Flow 03, built (`725237ee`): the activity page's vocabulary (the status card "Who has it now", About, the sections open, filled and locked, the rung row, the action bar, the note panel, the result, the refusal summary, the history and its fold). The design system is version 10, re-synced to flow 03 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 04`. Open each screenshot. Leave out anything in
   `design/upload/crop-first/`, or crop its link first. Add flow 03's chosen assessor boards (§ 4) by hand; the script
   does not stage them.
2. **Start a new canvas** from the main app's Design page, on the Wombat design system v10
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not flow 01's, 02's or 03's canvas. **Done:**
   https://claude.ai/artifact/8JnYLZp6CTR1Mg38a5v7DX (2026-09-30; `README.md` in the record folder).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Put the returned `/_blob/` URL in § 1 and in
   `design/flows/04-assessor-inbox/round-1-ask.txt`, where they read `<MARK-URL>`.
4. **Send § 1 and § 8 as ONE message** (`design/flows/04-assessor-inbox/round-1-ask.txt`), with the key screenshots in
   § 4. The canvas draws round 1 from the first message and does not wait for a second (§ 11, flow 02). Read
   `project/canvas.json`'s version before and after: a send can draw nothing.
5. **Pick one structure,** say why in a sentence, and answer § 6's questions one sentence each. Ask for round 2.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/03-trainee-files-activity/round-2-review.md`
   did: one reviewer on the code, one on the cast; the reviewer on the code checks the ask too (§ 11, flow 03). Put the
   decisions to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/04-assessor-inbox/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 04 — See what waits for me, and rate it, decline it or send it back, on any screen

This is flow 04 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account) and 03 (a registrar files an activity, and the activity page) are designed and built; these pages sit inside
  the shell. Structure first, then fidelity: round 1 is 2–3 structural variations as wireframes of the assessor's Home
  and the inbox, and I pick one before round 2. The activity page is flow 03's: extend it, do not redesign it. The
  runbook steps follow below in this same message. The brand mark is at /_blob/89ddc910c53255a32bcbb84e2820bc45: use it, never a drawn disc.

GOAL: A consultant sees what waits for them, oldest first, and how long each has waited; opens it; rates it, declines
  it with a reason, returns a reflection for more work or records the discussion; and is taken on to the next one. Often
  on a phone between patients. They never see a request that does not name them. Nothing emails an assessor when a
  request arrives, and the nudge mails only after five days untouched, so Home and the inbox are the notice: their
  counts and their order must agree, and must be right.
  What is wrong today (the attached screenshots show each; all are on the shell as built, 2026-09-27 to 09-30):
  - Home and the inbox disagree on order: Home's "Waiting for your rating" lists oldest first, the inbox newest first,
    so the overdue request is first on Home and last in the inbox.
  - The inbox is a seven-column table that scrolls sideways at 390 px (Type, Subject, EPA, Encounter date, State,
    Updated, Open), where My activities stacks its rows. Its "Updated" is a server time with no zone, not how long the
    item has waited, and the inbox never says overdue; only Home does.
  - Home's rows name the instrument and the registrar only ("Mini-CEX (Paediatrics) — Anele Dlamini"): two requests
    from one registrar read the same, to the eye and to a screen reader. No row says its EPA, its date or its age.
    "Overdue" replaces the state's badge, so an overdue row no longer says whether it is to rate or to review.
  - The card is called "Waiting for your rating" but holds portfolio reviews and reflections to discuss too. A count of
    0 still shows a "0" badge.
  - Recent decisions lists ten and has no page behind it: the eleventh decision is out of reach. Its rows carry no
    date. (Recent activities, the stub that was to hold them, was dropped by flow 01.)
  - A two-role consultant's Home opens as the committee view, which says nothing of the request waiting on her as an
    assessor, even overdue; she must know to switch.
  - After Complete, Decline, Return or Record discussion the page says the result ("Completed.") and stops: nothing
    offers the next waiting item, or says how many are left.
  - On a phone the assessor scrolls past the status card, About and the whole Request before reaching the rating.
  - The rating is a select ("Select…"), where the read-only rung row beside it shows the six rungs and their
    descriptors; the rater sees no descriptor while choosing.
  - The two note panels do not match: Decline's reads "Decline this request", "Decline with this note" (red) and "Keep
    the request"; Return's reads "Return: add a note", "Return with this note" (blue) and "Do not return it". The
    note's required mark is not red like the form's. Discard changes is greyed with no reason given.
  - Moves are named by their workflow keys in title case: "Complete", "Decline", "Record Discussion", "Return".
  - The Request section reads the assessor's own name with his email ("David Naidoo (naidoo@kgk.wombat.local)").
  - "Activity unavailable" offers "Go to My activities", a page an assessor's menu does not have.
  - The inbox's subtitle says "rate, review or record", its empty state "rate, discuss or review".

AUDIENCE: Assessor (a consultant); desktop 1280×800 and phone 390×844. Assessors rate between patients, often on a
  phone, often days after the encounter. Cast: Dr Mohammed Patel and Dr Fatima Khumalo hold Assessor only (their menu
  is Home and Activity inbox, then My data rights). Dr Thandi Zulu (committee chair), Dr David Naidoo and Dr Sarah Botha
  hold Committee member and Assessor; their Home opens as the view they last chose, and they switch in the sidebar.
  Registrars in the story: Anele Dlamini (year 3), Sipho Ndlovu and Nomsa Mahlangu (year 1), Pieter du Plessis (year 2),
  Lerato Molefe (year 4).

WHAT WAITS ON AN ASSESSOR (read from each activity's own workflow, never from a state's name):
  - What the inbox lists: every activity a move of which, one that leads on, the caller may make now and did not file
    themselves. For the CPSA instruments: a Mini-CEX, DOPS, CBD, CCA or direct observation Requested (Complete or
    Decline); a reflective exercise Awaiting discussion (Record Discussion, or Return with a note); a portfolio and
    logbook review Awaiting review (Sign Off, or Return). Home's count is the same rows.
  - Overdue: untouched for more than 7 days (the dashboard's threshold; the coordinator also sees a request as stalled
    after 7 days, and the nudge mails the assessor after 5).
  - A decision: an activity the caller moved last that is now finished or has no move left (Completed, Declined,
    Discussed, Signed off…).
  - An activity on a paused EPA reads "(no longer in use)" and is still completed; its credit waits (D48).

SCREENS, in order (today's names; a variation may merge, split or rename them):
  1. Home for the Assessor (/, "Assessor · Semester 2, 2026"): what waits, oldest first, overdue flagged; recent
     decisions; empty; and the Home a two-role consultant sees, in each view, when work waits in the other.
  2. The Activity inbox (/activities/inbox; its nav item "Activity inbox"): each row naming the instrument, the
     registrar, the EPA, the encounter date, the state and how long it has waited, with an Open action named for its
     row; empty; a paused EPA; loading; load error; 390 px.
  3. Recent decisions beyond the card: a page, a section of the inbox, or nothing (question 1).
  4. The activity page for the assessor, in flow 03's design:
     - to rate (a Mini-CEX): the registrar's Request read out, "Filled in by Anele Dlamini, 2026-09-30"; Entrustment and
       Feedback open, "You fill this in"; the six rungs by the College's labels 1, 2, 3a, 3b, 4, 5 with each rung's
       descriptor and the field's help ("The supervision this activity actually required on this occasion - not a
       judgement of the trainee's worth."); Complete, Discard changes and Decline;
     - decline with its note panel open; the refused decline ("Not declined." · "Reason for Sipho Ndlovu: Decline
       requires a note."), the panel still open with the note as typed; declined;
     - awaiting discussion (a reflection): the reflection read out, the Discussion open, Record Discussion and Return;
       return with its note panel open; returned ("With Sipho Ndlovu. Sarah Botha returned it on …");
     - awaiting review (a portfolio review, Dr Patel's): what the review asks, Sign Off and Return;
     - completed, with its result announced and the way on to the next waiting item;
     - completed on a paused EPA ("Its credit to PAED-012 waits while the EPA is paused.");
     - unavailable: an activity that does not name him reads exactly as an id that does not exist ("This activity does
       not exist, or you cannot open it."), and says nothing of permission;
     - loading and load error (flow 03's).
  5. Completing on a phone at 390 px: from Home or the inbox, open the request, rate 3b, write the three feedback
     fields, complete, read the result and go on to the next, with nothing scrolling sideways.

WHAT FLOW 04 MAY CHANGE OF FLOW 03'S PAGE, AND WHAT IT MAY NOT:
  May not (flow 03 decided them, and the registrar's view shares them): the page's order (the result, a refusal's
    summary, "Who has it now", then About beside the sections and the bar, then the history); the status card's parts
    and stripe rule (the reader's move is the action blue); the title "Type · EPA · date" and the subtitle naming the
    people ("Anele Dlamini's request to David Naidoo"); sections open, filled or locked, with "Filled in by" and "…
    fills this in"; the read-only rung row; the result's words and focus; the refusal summary; the note panel's place
    under the bar, its focus rules and that a refused move keeps it open; the history table, oldest first, times in
    SAST, folded into "All N moves" below 641 px; "Activity unavailable".
  May, for the assessor (flow 03 left these to flow 04): the rung picker the rater uses (radios in the rung row are
    flow 04's; today a select); the moves' own labels (flow 03 kept the workflow's title-case keys, "Record
    Discussion"; flow 04 may give moves labels of their own); the two note panels' words and weight; where About and the
    Request sit for the rater on a phone; the way on after a move (the next waiting item); "Activity unavailable"'s way
    on for someone with no My activities. Say which of these each variation changes, and why.

STEPS: 2.36, 3.4, 3.5, 3.11, 3.13, 3.15, 3.17, 3.24, 3.26, 3.28, 3.33, 3.51, 5.25, 6.18, A.4.3, A.6.8, A.7.2. Pasted
  verbatim below (Role / Route / Do / Expect). Where an Expect quotes today's wording, that is today's page, not a
  requirement on the new one.

STATES TO SHOW:
  Home (Assessor): empty; one request waiting; several waiting, one overdue; recent decisions; after a switch from the
    committee view; the committee view of a consultant with assessor work waiting; loading; load error; narrow.
  Activity inbox: empty; two rows of different states (Requested, Awaiting review); an overdue row; a paused EPA;
    loading; load error; narrow.
  Recent decisions: the chosen answer.
  Activity page (the assessor's view): to rate; decline note open; decline refused; declined; awaiting discussion;
    return note open; returned; discussed; awaiting review; completed (result announced, the way on); completed on a
    paused EPA; a move running ("Completing…"); unavailable; loading; load error; narrow.
  Data volumes: an inbox of 0, 2 and 25 rows; EPA names up to about 120 characters ("PAED-015 — Teaching and applying
    evidence-based care responsibly and ethically in clinical decision-making and research"); a registrar's name of 30
    characters; two requests from one registrar on one EPA and date; 12 recent decisions.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T297 (landed, keep it): Home's count is the inbox's rows, and its list is those rows; a decision is "finished or no
    move left". Home and the inbox must also agree on order.
  - T320: nothing emails an assessor when a request arrives (the nudge mails after five days). No copy may say or
    imply otherwise; the count, the order and the overdue flag are the reminder.
  - T323: every page fits 390 px with no sideways scroll: the inbox too (flow 03's stacked table, DataTable's Stack, is
    the pattern), and the rating (the six rungs abreast in the width, the feedback fields full width, the bar stacked).
  - T280: every link and row action is named by more than its instrument ("Mini-CEX (Paediatrics) · PAED-001 ·
    2026-09-20", flow 03's name), so no two share a name. Home's rows do not meet it today; the inbox's Open does.
  - T325: every time is South African time with its zone ("2026-09-30 10:27 SAST"); the inbox's Updated is not today.
  - T299 (done by flow 03, keep it): a move's result takes the focus; a refused Decline or Return keeps its note panel
    open with the note as typed.
  - T349, the assessor's side: the note's required mark red like the form's; a greyed Discard changes says why; one
    subtitle for the inbox.
  - Content (BRIEF.md § 6): states and types by their labels; people by name as stored, no email beside a name the
    reader knows; sentence case; no system words.
  - Done since the old brief, keep them: the activity page names the registrar (its subtitle and About); a refused
    Decline keeps its panel; Complete announces "Completed." and takes the focus; the feedback fields are in the body
    font; the history fits 390 px; the inbox's loading is a skeleton and its load error says nothing of the exception.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Recent decisions has ten rows and no page behind it. Does an assessor need everything they have decided (a page,
     or a second section of the inbox, with how far back), or is the card enough?
  2. Is the inbox the assessor's Home? An Assessor-only consultant's menu is Home and Activity inbox, and Home's one
     useful card is the inbox again. Keep both, merge them, or give Home something the inbox does not have?
  3. How does a two-role consultant learn of work waiting in the view she is not in (Dr Zulu opens as Committee member
     with an overdue request waiting)? A line on the other view's Home, a count on the switch, or nothing?
  4. What marks an item overdue, and where: a badge beside the state (not in its place), an order, a count, all three?
     Is 7 days the threshold the page should say?
  5. How does the rater choose a rung: the rung row as radios with each descriptor, a select, or cards? At 390 px the
     six rungs must fit abreast or wrap.
  6. After a move, what is the way on: back to the inbox, straight to the next waiting item, or a line naming how many
     are left?
  7. On a phone, does the rater see the Request first (as today), or the rating first with the Request folded?

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
  InstrumentPicker, and DataTable's stacked rows (Stack).
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

ASK:
  - Round 1: 2–3 STRUCTURAL variations as wireframes of Home (the Assessor's, and a two-role consultant's) and the
    Activity inbox, and of Recent decisions or its absence. Each covers the pages and their order (or what replaces
    them), the steps of each journey (rate a Mini-CEX from Home; decline a request with a reason; return a reflection,
    then record the discussion; rate on a phone; a two-role consultant finds and rates an overdue request), where each
    outcome, refusal and "what waits" shows, and the way on after a move. Give each its reasoning and its step count
    per journey, at 1280 and 390 px. No colour or type choices yet. Stop and wait for my pick.
  - In the same round, draw the assessor's activity page ONCE in flow 03's design (the attached boards), changing only
    what "What flow 04 may change" allows, and say what each change is.
  - Round 2: the chosen structure at full fidelity on the design system's tokens, every screen and state above, at
    1280 and 390 px.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (DESIGN.md § Dashboard page; § Page-level patterns "List page" and "Record page with a workflow"; § Form
    system "The activity form").
  - Flag edge cases: 25 rows waiting; every row overdue; two requests from one registrar on one EPA and date; a
    two-role consultant with work in both views; a request on a paused EPA; an assessor who is also a registrar.
  - Run an accessibility review against WCAG 2.1 AA: row actions named per row, the count read as words, focus after
    each move and after the way on, the note panel named, and the rung picker usable by keyboard.

ATTACHED: act-3/3.24-1-patel-home.png, act-3/3.24-2-patel-inbox-two.png, states/activity-inbox--paused-epa.png,
  act-A/A.7.2-3-inbox-390.png, states/home--narrow-assessor.png, act-3/3.33-1-zulu-home-committee-view.png,
  states/activity-view--to-rate.png, states/activity-view--decline-refused.png, states/activity-view--return-note.png,
  act-3/3.5-3-completed.png, act-A/A.7.2-4-activity-390.png, states/activity-view--unavailable.png; flow 03's chosen
  boards: R3-A-assessor and R3-A-assessor-note-open at 1280 and 390 (its only two boards of the assessor's view), and
  R3-C-Activity (the page's component board, every state; the other states are drawn for the registrar).
```

---

## 2. The journey

| Step | Page | The assessor does | They must be able to see |
|---|---|---|---|
| 2.36 | `/account/login` → `/` | Dr Patel and Dr Khumalo sign in for the first time | the Assessor view with nothing waiting; a menu of Home and Activity inbox, then My data rights |
| 3.4 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel looks for a request that does not name him, then opens it by its address | "Inbox clear"; "Activity unavailable", identical to an id that does not exist |
| 3.5 | `/dashboard/switch/{role}` → `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Naidoo switches, rates Dr Dlamini's Mini-CEX at rung 4 and completes it | the row names the registrar, EPA, date and state; "Your move."; only Entrustment and Feedback open; the six rungs; "Completed.", credited 1 item |
| 3.11 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Khumalo declines, first with no note, then with a reason | the note panel; "Not declined." with the panel still open; Declined, her reason quoted; the request gone from her inbox |
| 3.13 | `/dashboard/switch/{role}` → `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Botha completes the re-filed Mini-CEX at 3a, then switches back | Completed, credited 1 item; each switch's one-time alert |
| 3.15 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Botha returns a reflection with a note | Awaiting discussion; Record Discussion and Return; the note panel; "Returned to Sipho Ndlovu."; nothing left for her |
| 3.17 | `/activities/inbox` → `/activities/{ActivityId:int}` | she records the discussion on the re-submitted reflection | Discussed, read-only; credited "—", with no "counted towards no requirement" warning |
| 3.24 | `/` → `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel goes from Home to the inbox and completes a DOPS | Home's two rows, oldest first, as the inbox holds them; the DOPS Completed; the review still waiting; Recent decisions |
| 3.26 | `/activities/inbox` → `/activities/{ActivityId:int}` | five consultants complete Dr Molefe's six | each inbox holds only that assessor's own requests |
| 3.28 | `/activities/inbox` → `/activities/{ActivityId:int}` | three complete Dr Dlamini's three | each Completed, credited 1 item |
| 3.33 | `/` → `/dashboard/switch/{role}` → `/` → `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Zulu switches to her Assessor view and completes a stalled Mini-CEX | the committee view first (nothing of the request today); the switch; the count that matches her inbox; Overdue |
| 3.51 | `/` → `/activities/inbox` | Dr Khumalo reads Home, follows "Open inbox →", reads the menu | the waiting count and her recent decisions; no Recent activities in the menu (question 1) |
| 5.25 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Naidoo rates a request filed for the registrar's last day | Completed, read-only to both |
| 6.18 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel completes a Mini-CEX whose EPA was paused | "(no longer in use)" on the row and the page; not refused; its credit waits |
| A.4.3 | `/activities/inbox` → `/account/session-ended` → `/account/login` | Dr Khumalo's second browser is signed out after her password change | flow 02's notice; back to her inbox after signing in |
| A.6.8 | `/account/login` → `/` | Dr Patel signs in again after being reactivated | his Assessor Home, the overdue review listed as the inbox lists it |
| A.7.2 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel completes a Mini-CEX on his phone | the row's action named for it; the ladder and feedback in the width; the bar without sideways scrolling; "Completed." |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Home and the role dashboards and
§ Activities. **Every capture below is on the shell as built:** the Home states were re-taken by the T335 replay
(2026-09-27), the inbox and activity-page states by the T342 replay (2026-09-30), with flow 03's page.

| Page | State | Screenshot | What it shows |
|---|---|---|---|
| Home | Assessor, first sign-in | `states/home--assessor-empty.png` | "Waiting for your rating 0", "Nothing is waiting for your rating.", "No decisions yet." |
| Home | A request waiting | `states/home--assessor-pending.png` | Dr Zulu after the switch: 1 waiting, badged Overdue |
| Home | After a switch, nothing waiting | `states/home--assessor-switched.png` | "You are now acting as Assessor."; a "0" badge |
| Home | Recent decisions | `states/home--assessor-decisions.png` | Dr Khumalo: Completed in green, Declined in red |
| Home | Narrow | `states/home--narrow-assessor.png` | Dr Patel at 390 px: Overdue; two decisions that read the same, "Mini-CEX (Paediatrics) — Anele Dlamini" |
| Home | The committee view, assessor work waiting | `act-3/3.33-1-zulu-home-committee-view.png` | Dr Zulu as Committee member: nothing says her Mini-CEX waits |
| Activity inbox | Inbox clear | `states/activity-inbox--empty.png` | "Inbox clear" · "Nothing is waiting for you to rate, discuss or review." |
| Activity inbox | Requests | `states/activity-inbox--assessor.png` | Type, Subject, EPA, Encounter date, State, Updated (no zone), Open |
| Activity inbox | A paused EPA | `states/activity-inbox--paused-epa.png` | "(no longer in use)"; newest first, the week-old review last |
| Activity inbox | Loading | `states/activity-inbox--loading.png` | four skeleton rows |
| Activity inbox | Load error | `states/activity-inbox--load-error.png` | "Could not load the Activity inbox. …" with Try again |
| Activity inbox | Narrow | `states/activity-inbox--narrow.png` | the table cut off at Encounter date, scrolling sideways |
| Activity page | Unavailable | `states/activity-view--unavailable.png` | "This activity does not exist, or you cannot open it.", Go to My activities |
| Activity page | To rate | `states/activity-view--to-rate.png` | "Your move."; Request read out; the rung select; Complete, Discard changes (greyed), Decline |
| Activity page | Decline note open | `states/activity-view--decline-note.png` | "Decline this request", "Reason for Sipho Ndlovu" |
| Activity page | Decline refused | `states/activity-view--decline-refused.png` | "Not declined.", the panel open, the note marked |
| Activity page | Declined | `states/activity-view--declined.png` | the registrar's view: "Closed. Fatima Khumalo declined it on …", the reason quoted (the assessor's own is `act-3/3.11-3-declined.png`) |
| Activity page | Awaiting discussion | `states/activity-view--awaiting-discussion.png` | Record Discussion, Discard changes, Return |
| Activity page | Return note open | `states/activity-view--return-note.png` | "Return: add a note", "Note to Sipho Ndlovu", Do not return it |
| Activity page | Returned | `states/activity-view--returned.png` | the registrar's view after the return |
| Activity page | Discussed | `states/activity-view--discussed.png` | the registrar's view, finished (the assessor's is `act-3/3.17-1-discussed.png`) |
| Activity page | Awaiting review | `states/activity-view--awaiting-review.png` | the registrar's view: "With Mohammed Patel since …" (no capture shows the reviewer's side) |
| Activity page | Completed | `states/activity-view--completed.png` | "Done. David Naidoo completed it on …", "Rated 4. Credited 1 item to PAED-001." (the registrar's view) |
| Activity page | Credited nothing | `states/activity-view--credited-nothing.png` | Dr Patel: "Its credit to PAED-012 waits while the EPA is paused."; the paused EPA's warning under Credit |
| Activity page | Submitting | `states/activity-view--submitting.png` | a move running, the registrar's "Submitting…" (no capture of "Completing…") |
| Activity page | Loading | `states/activity-view--loading.png` | |
| Activity page | Load error | `states/activity-view--load-error.png` | |
| Activity page | Narrow | `states/activity-view--narrow.png` | Dr Patel at 390 px; the history folded |

Not captured, so describe them in words:
- **Recent decisions beyond ten.** The card lists ten (`GetAssessorDashboardSummaryQuery.RecentDecisionsListed`); no
  page lists more. Question 1.
- **The inbox with an overdue row marked.** The inbox never marks one today.
- **The way on after a move.** Nothing offers it today (`act-3/3.5-3-completed.png`).
- **A portfolio review signed off from the assessor's side.** The runbook leaves Dr du Plessis's review waiting to the
  end of Act 3; its Sign Off is not played in these steps.

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-09-30; all are on
the shell as built.

**Key screenshots (attach these first, with § 1):**
1. `act-3/3.24-1-patel-home.png`: Home, two waiting (the review first), no decisions yet; rows name type and registrar only
2. `act-3/3.24-2-patel-inbox-two.png`: the inbox, two rows, seven columns, Updated with no zone
3. `states/activity-inbox--paused-epa.png`: "(no longer in use)", and the order: newest first, the week-old review last
4. `act-A/A.7.2-3-inbox-390.png`: the inbox at 390 px, cut off at Encounter date and scrolling sideways
5. `states/home--narrow-assessor.png`: Home at 390 px: Overdue in place of the state; two decisions that read the same
6. `act-3/3.33-1-zulu-home-committee-view.png`: a two-role consultant's committee view, silent on her overdue request
7. `states/activity-view--to-rate.png`: the page to rate, the rung select, the email beside his own name, Discard greyed
8. `states/activity-view--decline-refused.png`: the refused decline, the panel kept open
9. `states/activity-view--return-note.png`: Return's note panel, worded and weighted unlike Decline's
10. `act-3/3.5-3-completed.png`: "Completed." and the done status card; no way on
11. `act-A/A.7.2-4-activity-390.png`: the page at 390 px: About and the Request before the rating
12. `states/activity-view--unavailable.png`: "Activity unavailable", with Go to My activities

Also attach **flow 03's chosen boards**, from `design/flows/03-trainee-files-activity/round-3/project/`:
`R3-A-assessor-1280.dc.html`, `R3-A-assessor-390.dc.html`, `R3-A-assessor-note-open-1280.dc.html` and
`R3-A-assessor-note-open-390.dc.html`, the only boards of the assessor's view (to rate; Decline's note open), and
`R3-C-Activity.dc.html`, the page's component board, which draws every state (the others for the registrar:
`refused-assessor` is her refused submit naming the assessor field, not the assessor's page). Add `flow03.css` and
`flow03-r3.css` from the same folder, which the boards load. The staging script does not stage any of them.

**States (attach as the chat asks):** every row of § 3 not already attached above, and the step captures
`act-3/3.33-2-zulu-home-assessor-view.png` (after the switch: one waiting, Overdue), `act-3/3.51-1-khumalo-home.png`
(one waiting; Recent decisions in green and red), `act-A/A.6.8-1-patel-signed-in.png` (the overdue review) and
`act-A/A.7.2-5-completed-390.png` (completed at 390 px, the rung row and the feedback read out).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `53d9322d`.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T320** | Nothing emails an assessor when a request arrives; the nudge mails after five days (Step 3.32). Home and the inbox are the notice, so their count, order and overdue flag must agree and be right, and no copy may promise mail. | T320's symptom; `DataRights.razor:164` still promises "a request to assess" |
| **T323** | The inbox is a plain `DataTable` (`ActivityInbox.razor:26`), where My activities passes `Stack="true"` (`MyActivities.razor:47`): at 390 px it scrolls sideways (A.7.2's Actual, `appendix-cross-cutting.md:1058`). The activity page itself now fits (A.7.2: no gap). | `act-A/A.7.2-3-inbox-390.png`, `states/activity-inbox--narrow.png` |
| **T280** | Home's rows are named "{type} for {subject}" (`AssessorDashboard.razor:30`, `:57`), so two decisions on one registrar share a name. The inbox's Open is named per row (`ActivityRowNames`, `ActivityInbox.razor:52`). | `states/home--narrow-assessor.png` |
| **T325** | The inbox's Updated is `UpdatedOn.ToLocalTime()` with no zone (`ActivityInbox.razor:48`). The activity page's times are SAST (flow 03). | `act-3/3.24-2-patel-inbox-two.png` |
| **T297** (landed, `7bf8ea7`) | Home's count and rows are the inbox's (`GetAssessorDashboardSummaryQuery.cs:68–75`). Its order is not: Home re-sorts oldest first (`:72`), the inbox keeps `ActivityWaiting`'s newest first (`ActivityWaiting.cs:103`). | `states/activity-inbox--paused-epa.png` beside `act-A/A.6.8-1-patel-signed-in.png`; A.7.2's Actual |
| **T349** (items 3, 4, 5) | The note's `*` is not `.required-mark`; Discard changes is `aria-disabled` with no reason; DESIGN.md:2649 gives the inbox's subtitle as "Work waiting for an assessor's rating or review.", the code "Work waiting for you to rate, review or record." (`ActivityInbox.razor:63`), and its empty state "rate, discuss or review" (`:68`). | `states/activity-view--decline-refused.png`, `states/activity-view--to-rate.png` |
| **T345** | An activity's name can differ by viewer: the assessor may not see the " · <nominee>" suffix the registrar sees on a collision. | T345's symptom |
| **T346** | The completed status card lacks the semester count ("1 of 3 this semester"); from the assessor's side, whether it should say so at all is question-worthy (rating bias). | `act-3/3.5-3-completed.png` |
| **T322** | Badge and alert colours from flow 01's tokens (done); the Overdue badge is `BadgeFor.State(BadgeState.Accepted)`, amber, in place of the state's (`AssessorDashboard.razor:33`). | `states/home--narrow-assessor.png` |
| Observed, not filed | The note panels' words are keyed on the literal "decline" (`ActivityWorkflowActions.razor:274`, `:277`, `:280`), so Return reads "Return: add a note" and "Do not return it". The rater's scale is a select (`ActivityForm.razor:210–220`, "the rung row with radios is flow 04's"). A person's option label adds the email (`NomineeDirectory.cs:100`), and the filled Request reads it back. "Activity unavailable" offers Go to My activities (`ActivityView.razor:58`). The committee dashboard shows nothing of the assessor's waiting work. The card's `Count` shows "0" (`states/home--assessor-switched.png`). Recent decisions' `DecidedOn` is read and not shown (`GetAssessorDashboardSummaryQuery.cs:162`). | `states/activity-view--return-note.png`, `states/activity-view--to-rate.png`, `states/activity-view--unavailable.png`, `act-3/3.33-1-zulu-home-committee-view.png` |

## 6. Questions the design must answer

1. **Recent decisions beyond ten.** The card lists ten (`RecentDecisionsListed`) and nothing lists more; flow 01 dropped
   Recent activities, the stub that was to. A page, a section of the inbox, or is the card enough? If a list: which
   (everything he moved last, or everything naming him), which columns, how far back.
2. **Is the inbox the assessor's Home?** An Assessor-only consultant's menu is Home and Activity inbox (Step 2.36), and
   Home's main card is the inbox again, with "Open inbox →" at its foot. Keep both, merge them, or give Home what the
   inbox lacks (decisions, a count of this semester's ratings)?
3. **A two-role consultant.** Dr Zulu's Home opens as the committee view, which says nothing of her overdue request
   (`act-3/3.33-1-zulu-home-committee-view.png`); she must switch (Step 3.33). The acting role chooses only what the
   frame shows (flow 01, W-010), so the answer is a line or a count, not a merged view.
4. **Overdue.** Home's threshold is 7 days untouched (`DashboardThresholds.AssessorDueDays`,
   `src/Wombat.Application/Common/Options/DashboardThresholds.cs:7`); the coordinator's stall is also 7, the nudge mails
   at 5 (`act-3-operations.md:702`). A badge beside the state, an order, a count, or all three; and in the inbox too?
5. **The rater's rung picker.** Radios in the rung row with each descriptor, as the read-only row shows them, or keep
   the select? The College's labels 1, 2, 3a, 3b, 4, 5 (D32); six abreast at 390 px, or wrapped.
6. **The way on.** After Complete, Decline, Return or Record Discussion: back to the inbox, to the next waiting item,
   or a line naming how many are left?
7. **Should the rater see the registrar's year minimum?** Step 3.5 notes year 3's minimum on PAED-001 is rung 4; the
   page does not show it to the assessor. Showing it may anchor the rating. Operator's call.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the 17 steps** on a fresh database, playing the acts up to each one (`README.md` § How to play): 2.36; Act
  3's 3.4–3.33 and 3.51; 5.25; 6.18; and the appendix's A.4.3, A.6.8 and A.7.2. Every Expect must hold. Steps 3.4,
  3.5, 3.11, 3.15, 3.33 and 3.51 quote the pages' words: update them in the same task if the wording changes (BRIEF.md
  § 9 item 7), and flow 03's quoted steps with them where they share a page.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`). They
  must include:
  - `Dashboards/` (`AssessorDashboardSubjectNameTests`, `DashboardCardTests`, `DashboardPriorityTests`);
  - `Navigation/DashboardLinkAuthorizationTests`, and `NavMenuAuthorizationTests` if a page is added to the menu;
  - every file in `Activities/`, notably `ActivityViewAssessorSurfaceTests`, `ActivityWorkflowActionsTests` and
    `ActivityViewCreditSignalTests`;
  - `Accessibility/ActionFocusTests` and `RowNamesTests`; `Design/RowActionMarkupTests`, `NarrowLayoutTests` and
    `DefinedClassTests`;
  - `Scenario/`.
- **Re-capture** the states in § 3 and the steps' own captures, keeping their names. Compare them with the chosen
  artboards.
- **Browser check** at 1280 and 390 px as Dr Patel (Assessor only) and as Dr Zulu (two roles): each Home figure
  matches the page it opens, in the same order.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as it is today, after flow 03 (re-synced 2026-09-30). `D` is the replay day
(`act-3-operations.md` § The clock in this act).

```text
Step 2.36 — Dr Patel and Dr Khumalo, assessors (act-2-onboarding.md:656)
Role: Assessor — Dr Mohammed Patel and Dr Fatima Khumalo
Route: /account/login → /
Do: Each signs in and reads the dashboard and nav.
Expect: Each sees "Assessor · Semester N, YYYY" on Home, and the empty Assessor dashboard of Step 2.34: "Waiting for
  you" with no badge and no "0", "Nothing is waiting for you.", "Open Activity inbox"; Recent decisions, "No decisions
  yet." (T350). The sidebar reads "Acting as Assessor" with no switch, over Home and Activity inbox, then My data
  rights.

Step 3.4 — An assessor who was not named cannot open the request (act-3-operations.md:158)
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Look for Dr Dlamini's Mini-CEX in the inbox, then open its address directly, using the id from Step 3.1.
Expect: The inbox's subtitle reads "What waits for you to rate, review or discuss." Its section Waiting for you reads
  "Inbox clear" and "Nothing is waiting for you.", because no request names him yet, and its section Decided by you
  reads "No decisions yet." (T350). The activity's page is headed "Activity unavailable" and reads "This activity does
  not exist, or you cannot open it.", with Go to Activity inbox, the list his acting role opens activities from (T350).
  It reads exactly so for an id that does not exist, so walking ids discloses nothing (T101, C7).

Step 3.5 — Dr Naidoo rates the Mini-CEX and completes it (act-3-operations.md:173)
Role: Assessor — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Choose Switch to Assessor in the sidebar, then open Activity inbox from the menu and Dr Dlamini's Mini-CEX in it.
  Rate the supervision the encounter required at rung 4, write what was done well, the areas for development and the
  agreed plan, and complete it.
Expect: The inbox's row is its own link, "Mini-CEX (Paediatrics) · PAED-001 · `D−10`" with "from Anele Dlamini" under
  it; the EPA cell "PAED-001 — Providing paediatric emergency care to children"; Requested; Waiting "Less than a day"
  over "since … SAST". The row has no Open button, and the table no Subject, Encounter date or Updated column (T350).
  On the page, the status card reads "Your move. Anele Dlamini asked you on …" and "Complete it, or decline it with a
  note Anele Dlamini will read." Request is read-only, "Filled in by Anele Dlamini, `D`", and its Assessor reads "David
  Naidoo", with no email; only Entrustment and Feedback can be filled in. The actions are Complete, Discard changes
  (greyed, "Nothing to discard yet." beside it) and Decline. The rating is the rung picker: six radios labelled by the
  College's rungs, 1, 2, 3a, 3b, 4 and 5 (D32), with "What each rung means" open under them; choosing 4 marks it
  "chosen" and puts its descriptor, "Unsupervised practice. …", under the row (T350). After Complete, the focused
  result reads "Completed. Nothing else waits for you.", with Go to Home after it; the status card reads "Done. You
  completed it on …" and "Rated 4. Credited 1 item to PAED-001.", and the page is read-only. The history's Complete row
  reads Requested → Completed, by David Naidoo, credited "1 item". Year 3's minimum on PAED-001 is rung 4, so the
  encounter counts at the minimum; nothing on the page says the minimum (Q7), so that is the tester's check.

Step 3.11 — Dr Khumalo declines the request, with a reason (act-3-operations.md:321)
Role: Assessor — Dr Fatima Khumalo
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Ndlovu's Mini-CEX. Press Decline and send it with the reason left empty. Then send it with the reason "I was
  not on the ward that day; Dr Botha observed this encounter. Please send it to her."
Expect: The status card reads "Your move. Sipho Ndlovu asked you on …". Decline opens the note panel under the actions,
  "Decline this request", and moves the focus into "Note for Sipho Ndlovu", marked required, with "Sipho Ndlovu reads
  it on the activity's page. It is kept with the activity's history." under it. Sent empty with Decline with this note,
  it is refused: the panel stays open, with the note as typed, and its summary reads "Not declined. It is still
  Requested." and "Note for Sipho Ndlovu: Decline requires a note."; the activity stays Requested (T350). Keep the
  request would close the panel and hand the focus back to Decline. Sent with the reason, the result reads "Declined.
  Nothing else waits for you.", with Go to Home after it. The status card, badged Declined, reads "Closed. You declined
  it on …", quotes her reason, and reads "It credits nothing, and nothing more can happen to it." The page is
  read-only, with no move left. The history's Decline row, Requested → Declined, is credited "—", with her note on a
  row of its own under it. Her inbox's Waiting for you reads "Inbox clear", and Decided by you ("1 decision") holds the
  request, Declined, decided "… SAST", Credit "—".

Step 3.13 — Dr Botha completes Dr Ndlovu's Mini-CEX (act-3-operations.md:392)
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open Activity inbox from the menu and Dr Ndlovu's re-filed Mini-CEX
  in it. Rate it 3a with feedback and complete it. Then choose Switch to Committee member in the sidebar.
Expect: The inbox lists the re-filed Mini-CEX alone under Waiting for you. She rates it on the rung picker, choosing
  3a. After Complete the result reads "Completed. Nothing else waits for you." (T350). The Mini-CEX is Completed,
  credited "1 item". Year 1's minimum on PAED-002 is 3a, so it counts at the minimum. The declined request stays
  Declined and credits nothing. Each switch lands on Home with its one-time info alert, "You
  are now acting as Assessor." and then "You are now acting as Committee member." (Step 2.34), and the sidebar ends
  reading "Acting as Committee member".

Step 3.15 — Dr Botha returns the reflection for more detail (act-3-operations.md:439)
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open Dr Ndlovu's reflection from Activity inbox. Return it with the
  note "Please say what you would do differently at triage, and what you will read before your next take." Then choose
  Switch to Committee member in the sidebar.
Expect: The inbox lists the reflection under Waiting for you, badged Awaiting discussion. The page offers Record
  discussion, Discard changes (greyed, "Nothing to discard yet." beside it) and Return, and only the Discussion field is
  open. Its status card reads "Your move. Sipho Ndlovu asked you on …" and "Record the discussion, or return it with a
  note Sipho Ndlovu will read." Return opens the note panel, "Return this reflection", with the focus in "Note for Sipho
  Ndlovu", marked required; Return with this note (primary) sends it, and Keep the reflection would close the panel
  (T350). Once it is returned, the result reads "Returned to Sipho Ndlovu. Nothing else waits for you.", with Go to Home after
  it; the status card reads "With Sipho Ndlovu. Sarah Botha returned it on …" and "It is in nobody's inbox until Sipho
  Ndlovu submits it.", quoting her note, and the page offers Dr Botha nothing. The history's Return row (Awaiting
  discussion → Draft) carries her note.

Step 3.17 — Dr Botha records the discussion (act-3-operations.md:496)
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open the re-submitted reflection from Activity inbox, write the
  discussion notes, and record the discussion. Then choose Switch to Committee member in the sidebar.
Expect: The result reads "Discussed. Nothing else waits for you.", with Go to Home after it (T350). The state is
  Discussed, finished and read-only to both of them. The Record discussion row is credited "—":
  the exercise credits nothing (D7), and no "counted towards no curriculum requirement" banner shows (T108). Dr Ndlovu's
  progress does not change.

Step 3.24 — Dr Patel completes Dr Mahlangu's DOPS (act-3-operations.md:622)
Role: Assessor — Dr Mohammed Patel
Route: / → /activities/inbox → /activities/{ActivityId:int}
Do: From the dashboard, open the inbox, then the DOPS. Rate it 3a with feedback, and complete it.
Expect: Home (Assessor view): "Waiting for you" is badged "2 waiting", with the rule line "Oldest first. Overdue once
  it has waited 7 days.", and lists the same two rows as the inbox, in the same order, oldest first (T297, T350):
  "Portfolio and Logbook Review (Paediatrics) · PAED-015 · `D−1`", from Pieter du Plessis, Awaiting review, "Waiting
  less than a day"; then "DOPS (Paediatrics) · PAED-002 · `D−8`", from Nomsa Mahlangu, Requested. The inbox's Waiting
  for you holds those two rows, in that order. He rates the DOPS 3a on the rung picker. After Complete the result
  reads "Completed. 1 more waits for you.", and under it the review's row, Open the next and Back to Activity inbox
  (T350). The DOPS is Completed, credited "1 item". Year 1's minimum on PAED-002 is 3a, so it counts at the minimum.
  The portfolio review stays in his inbox, Home's card is then badged "1 waiting", and Recent decisions lists "DOPS
  (Paediatrics) · PAED-002 · `D−8`", from Nomsa Mahlangu, Completed, dated `D`.

Step 3.26 — The consultants complete Dr Molefe's six (act-3-operations.md:675)
Role: Assessor — Dr Thandi Zulu, Dr David Naidoo, Dr Mohammed Patel, Dr Sarah Botha and Dr Fatima Khumalo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Each consultant opens their own requests from Activity inbox and completes them with feedback, rating:
  - Dr Zulu: rung 5 on the PAED-001 Mini-CEX and rung 5 on the PAED-010 observation;
  - Dr Naidoo: rung 5;
  - Dr Patel: rung 4;
  - Dr Botha: rung 5;
  - Dr Khumalo: rung 5.
  Dr Zulu and Dr Botha act as Committee members: each first chooses Switch to Assessor in the sidebar, and after rating
  chooses Switch to Committee member (Step 3.13). Dr Naidoo still acts as Assessor, his choice from Step 3.5.
Expect: Each inbox's Waiting for you holds only that assessor's own requests. Each rates on the rung picker. After each
  Complete the result says what is left: Dr Zulu reads "Completed. 1 more waits for you." after her first, with the
  second's row and Open the next, and "Completed. Nothing else waits for you." after her second; Dr Patel reads
  "Completed. 1 more waits for you.", with Dr du Plessis's portfolio review as the next row; Dr Khumalo reads the same,
  with Dr du Plessis's CBD (Step 3.21) still waiting; Dr Naidoo and Dr Botha read "Completed. Nothing else waits for
  you.", with Go to Home (T350). All six end Completed, each credited "1 item". Year 4's
  minimum is rung 5 on PAED-001, PAED-010 and PAED-012. So Dr Patel's rung 4 counts towards the target, but not at the
  minimum.

Step 3.28 — The consultants complete Dr Dlamini's three (act-3-operations.md:721)
Role: Assessor — Dr Thandi Zulu, Dr Sarah Botha and Dr Fatima Khumalo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Each completes their own from Activity inbox with feedback, rating: Dr Zulu 3b on the CBD, Dr Botha 4 on the CCA,
  and Dr Khumalo 4 on the Mini-CEX. Dr Zulu and Dr Botha first choose Switch to Assessor in the sidebar, and after
  rating choose Switch to Committee member (Step 3.13).
Expect: Each rates on the rung picker. Dr Khumalo, who also holds Dr du Plessis's CBD, reads "Completed. 1 more waits
  for you.", with the CBD's row as the next; Dr Zulu and Dr Botha read "Completed. Nothing else waits for you." (T350).
  All three end Completed, each credited "1 item". Dr Dlamini's PAED-001 now holds three encounters this semester.
  Two are at year 3's minimum of rung 4; the CBD at 3b counts towards the target only. Her PAED-004 holds one, at the
  minimum.

Step 3.33 — Dr Zulu switches to her assessor view and completes the stalled Mini-CEX (act-3-operations.md:851)
Role: Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Her sessions open acting as a Committee member; choose Switch to Assessor in the sidebar. From "Waiting for you",
  open Dr Mahlangu's Mini-CEX (the Activity inbox lists it too), rate it 3a with feedback, and complete it.
Expect: The sidebar first reads "Acting as Committee member", with "Switch to Assessor" under it, and Home "Committee
  member · Semester N, YYYY" under its heading. Under the header, above the committee cards (Registrars and Targets by
  EPA), a warning line reads "1 activity waits for you in the Activity inbox, and it is overdue: Mini-CEX (Paediatrics)
  · PAED-004 · `D−3`, from Nomsa Mahlangu, waiting 8 days.", with Open it (T350). After the switch Home reads "You are
  now acting as Assessor." in an info alert under the header, and "Assessor · Semester N, YYYY", with no such line; the
  sidebar reads "Acting as Assessor" over Home and Activity inbox, and "Waiting for you", in the warning stripe, is
  badged "1 waiting, 1 overdue": this Mini-CEX, "Mini-CEX (Paediatrics) · PAED-004 · `D−3`", from Nomsa Mahlangu,
  Requested with Overdue beside it, "Waiting 8 days": it has waited past the assessor's seven days since Step 3.30 aged
  it. The inbox lists it with "8 days" over "since `D−8` … SAST"; the page's status card reads "Your move. Nomsa
  Mahlangu asked you on `D−8` … SAST.", the same moment. Step 3.31's reminder has not moved it. Dr Zulu rates it on the
  rung picker. After Complete the result reads "Completed. Nothing else waits for you.", with Go to Home, and Home then
  reads "Nothing is waiting for you." The Mini-CEX is Completed, credited "1 item". Year 1's minimum on PAED-004 is 3a.
  Nothing of Dr Mahlangu's waits for an assessor any more: the Mini-CEX has left Waiting for assessors and Mr Smit's
  Home card.

Step 3.51 — Dr Khumalo's dashboard, and Recent Activities (act-3-operations.md:1214)
Role: Assessor — Dr Fatima Khumalo
Route: / → /activities/inbox
Do: Read the dashboard, then follow "Open Activity inbox". Then read the menu.
Expect:
  - Waiting for you, badged "1 waiting": "Case-Based Discussion (Paediatrics) · PAED-002 · `D−5`", from Pieter du
    Plessis, Requested, "Waiting less than a day" (T297, T335, T350). The inbox's Waiting for you lists it alone, as
    Requested, "Less than a day" over "since … SAST".
  - Recent decisions, the activities she moved last, newest first, each linked by its full name with its registrar,
    its decision's badge and its day: "Mini-CEX (Paediatrics) · PAED-004 · `D−2`", from Anele Dlamini, Completed, in
    green; "Case-Based Discussion (Paediatrics) · PAED-012 · `D−5`", from Lerato Molefe, Completed; "Mini-CEX
    (Paediatrics) · PAED-002 · `D−20`", from Sipho Ndlovu, Declined, in red; its foot "All your decisions". The inbox's
    Decided by you, "3 decisions", lists the same three, each with its moment (SAST) and its credit.
  - The inbox lights Activity inbox in the menu: Home and Activity inbox, then My data rights. There is no Recent
    activities: the flow 01 pick dropped it, with its placeholder.

Step 5.25 — Dr Naidoo rates it (act-5-graduation.md:594)
Role: Assessor — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: He has acted as a Committee member since Step 4.5: choose Switch to Assessor in the sidebar, then open Dr du
  Plessis's Mini-CEX from Activity inbox and rate it `3b`, PAED-002's minimum in training year 2. Write what was done
  well, what to develop and the agreed plan, and complete it. Then choose Switch to Committee member in the sidebar.
Expect: Before the switch, his Committee member's Home carries, under the header, the line "1 activity waits for you
  in the Activity inbox: Mini-CEX (Paediatrics) · PAED-002 · `D−1`, from Pieter du Plessis, waiting less than a day.",
  with Open it (T350). After it, Home's "Waiting for you" is badged "1 waiting", and the inbox lists the Mini-CEX as
  "Mini-CEX (Paediatrics) · PAED-002 · `D−1`", from Pieter du Plessis, Requested. He rates it on the rung picker. After
  Complete the result reads "Completed. Nothing else waits for you.", with Go to Home. The activity reads Completed and
  is read-only to both of them. It credits PAED-002 in the semester that holds `D−1` (checked in Step 5.26), and My
  activities reads it credited "1 item". Nobody is emailed.

Step 6.18 — Dr Patel completes the Mini-CEX during the pause (act-6-catalogue.md:419)
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Dlamini's Mini-CEX from the inbox. Rate it at the rung that Dr Dlamini's PAED-012 card named as the minimum
  now (Step 6.15). Write the three feedback fields, and complete it.
Expect:
  - The inbox's Waiting for you lists Dr du Plessis's portfolio review first, Overdue since Step 3.30's ageing, then
    "Mini-CEX (Paediatrics) · PAED-012 · `D`", from Anele Dlamini (T350). The row's EPA cell and the request's EPA both
    read "PAED-012 — Communicating with and counselling patients, caregivers and healthcare teams (no longer in use)".
  - He rates it on the rung picker. The completion is not refused (D48): "Completed. 1 more waits for you.", with the
    review's row, Open the next and Back to Activity inbox under it (T350). The activity is read-only.
  - The status card reads "Done. You completed it on …" and "Rated <the rung>. Its credit to PAED-012 waits while the
    EPA is paused." About's Credit reads None, with "This activity's EPA is paused: its credit waits." under it. In
    its history, the completion's Credit reads None.

Step A.4.3 — Dr Khumalo's other session ends (appendix-cross-cutting.md:581)
Role: Assessor — Dr Fatima Khumalo (the second browser)
Route: /activities/inbox → /account/session-ended → /account/login → /activities/inbox
Do: Go back to the second browser and wait up to a minute. Sign in with the old password, then with the new one.
Expect: The tab leaves for the sign-in page by a full page load, which says "Your session has ended. Sign in again."
  (T279), an information notice. The old password is refused, and "Invalid email or password." takes the notice's
  place. The new one brings her back to her Activity inbox: the return address survives the refusal.

Step A.6.8 — Dr Patel signs in again (appendix-cross-cutting.md:1023)
Role: Assessor — Dr Mohammed Patel
Route: /account/login → /
Do: Sign in with his own password.
Expect: He lands on his Assessor dashboard. "Waiting for you", in the warning stripe, is badged "1 waiting, 1 overdue"
  and lists Dr du Plessis's Portfolio and Logbook Review, from Pieter du Plessis, Awaiting review with Overdue beside
  it, "Waiting 8 days", as his inbox lists it, "8 days" over "since `D−8` … SAST" (T297, T335, T350). Recent decisions
  lists the three he decided on `D`. Dr Dlamini's assessor list names him again (checked at A.7.1).

Step A.7.2 — Dr Patel completes it on his phone (appendix-cross-cutting.md:1118)
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: At 390 px, open Dr Dlamini's Mini-CEX from Activity inbox. Rate the supervision at `3b`, write the three
  feedback fields, and complete it.
Expect: Home's "Waiting for you" is badged "2 waiting, 1 overdue". The inbox's rows stack, nothing scrolling sideways:
  the review first, then "Mini-CEX (Paediatrics) · PAED-002 · `D−20`", from Anele Dlamini, each row its own link named
  for it, with no Open button (T350). On the activity, her request is read-only to him and folded: "Request", "Filled in
  by Anele Dlamini, `D`" and Show, reading Hide while open. Entrustment comes first, the rung picker's six radios
  abreast, then the three feedback fields, all fitting the width. Complete, Discard changes and Decline stack, each 44
  px tall, reachable without scrolling sideways, and About sits under them. The history folds into "All N moves", the
  same moves as stacked blocks (A7). Once completed, the result reads "Completed. 1 more waits for you.", with the
  review's row, Open the next and Back to Activity inbox; the completed page folds nothing, and it is read-only to both
  of them. Nobody is emailed (Step 3.3).
```
