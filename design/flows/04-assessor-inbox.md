# F04 An assessor works their inbox

**Who and why:** a consultant sees what waits for them, rates it or declines it with a reason, returns a reflection and
records the discussion, often on a phone between patients (Step A.7.2). Nothing emails an assessor when a request
arrives (T320), so the dashboard and the inbox are the only notice. An unseen request stalls a registrar (Steps
3.29–3.33).

| | |
|---|---|
| Mode | **Wireframe first** for the Assessor dashboard and the Recent Activities decision. The activity page extends F03's design at fidelity. |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | 2–3 |
| People | Assessor: Dr Mohammed Patel and Dr Fatima Khumalo. Dr Thandi Zulu, Dr David Naidoo and Dr Sarah Botha also assess, and switch views (CommitteeMember + Assessor). |
| Runbook steps | 17, pasted verbatim in § 8 |
| Pages (`coverage.md` templates) | `/`, `/activities/inbox`, `/activities/{ActivityId:int}`, `/placeholder/{Feature}`; the endpoint `/dashboard/switch/{role}` (a two-role consultant switches to the Assessor view) |
| Components | `Components/Pages/Dashboards/AssessorDashboard.razor`, `Pages/Activities/ActivityInbox.razor`, `Pages/Activities/ActivityView.razor` with `Shared/Activities/ActivityForm.razor` and `ActivityWorkflowActions.razor` |
| Held | Nothing. T297's 12 images, the four Assessor Home state captures among them, were re-captured on 2026-09-26 (§ 4) |
| Depends on | F01 (the shell and the role switch) and F03 (the activity page's vocabulary) |

**How to run this thread** (BRIEF.md § 2.3):
1. Brief F01 and F03 first. Open a new Claude Design thread for this flow alone (one thread per flow), and attach
   F03's chosen artboards (`design/flows/03-trainee-files-activity/`) as the activity page to extend.
2. Paste § 1, and attach the key screenshots in § 4 with it.
3. Paste § 8, the runbook steps, as the next message.
4. Attach the state screenshots in § 4 as the chat asks for them. Every Assessor Home capture § 4 lists was
   re-captured after T297 (2026-09-26). Brief the dashboard from the words in § 1.
5. Pick one dashboard wireframe and one Recent Activities answer, then ask for fidelity.
6. Answer § 6's questions in the chat, one sentence per decision. Export the chosen artboards into
   `design/flows/04-assessor-inbox/`.

---

## 1. The ask (paste this first)

```text
FLOW 04 — See what waits for me, and rate it, decline it or send it back, on any screen

GOAL: A consultant sees what waits for them, rates it or declines it with a reason, returns a reflection for more
  work and records the discussion, often on a phone between patients. They never see a request that does not name
  them. Nothing emails an assessor when a request arrives, so this dashboard and the inbox are the only notice, and
  their counts must be right.
  What is wrong today:
  - The dashboard's "Pending requests" reads 0 beside a full inbox. This is being fixed (T297); do not copy the old
    dashboard's cards from any screenshot.
  - Recent Activities was a "Coming soon" stub; T335 deleted the placeholder page and its nav item, so there is no
    page behind the Recent decisions card yet.
  - The activity page does not name the registrar whose work it is; its summary shows Type, Encounter date, State
    and the form's Version number.
  - A Decline refused for a missing note closes its note panel. A Complete that succeeds says nothing, and the focus
    drops to the page body.
  - At 390 px the rating form is three cards deep with 193 px inputs, the feedback fields are in monospace, and the
    history table is cut off at its Actor column.
  - Inbox times ("Updated 2026-09-26 12:27") carry no time zone.

AUDIENCE: Assessor (a consultant); desktop 1280×800 and phone 390×844. Cast: Dr Mohammed Patel and Dr Fatima Khumalo
  hold Assessor only. Dr Thandi Zulu (committee chair), Dr David Naidoo and Dr Sarah Botha hold CommitteeMember and
  Assessor; their Home opens as the committee view, and they switch to the Assessor view.

THE ASSESSOR DASHBOARD, AS IT WILL BE AFTER THE FIX (T297; design from these words, not from screenshots):
  - Pending requests: how many items in the inbox he can act on now (his own portfolio left out), linking to the inbox.
  - Needing action: those items, oldest first; an item untouched for more than 7 days is flagged overdue.
  - Recent decisions: the activities he last moved that are now finished or have no move left (Completed, Declined,
    Discussed…), each with its state as a badge and named by instrument, registrar and EPA or date.
  - Empty: nothing waiting and no decisions yet.

SCREENS, in order:
  1. The Assessor dashboard, in the frame from flow 01: what waits (oldest first, overdue flagged), recent decisions,
     empty, and the view a two-role consultant reaches by switching from the committee view.
  2. The inbox (/activities/inbox): rows to rate, each naming the instrument, the registrar, the EPA, the encounter
     date, the state and how long it has waited, with an Open action named for its row; empty ("Inbox clear. There
     are no activities waiting for your action."); an item on a paused EPA ("PAED-012 — … (no longer in use)");
     loading; 390 px.
  3. The activity page for the assessor, extending flow 03's design:
     - to rate: the registrar's Request read-only; the rung picker (the six rungs by the College's labels 1, 2, 3a,
       3b, 4, 5, with the help "The supervision this activity actually required on this occasion - not a judgement of
       the trainee's worth."); the three feedback fields; Complete and Decline;
     - decline with its note panel open;
     - the refused decline ("Decline requires a note."), with the note panel still open;
     - awaiting discussion (a reflection): the reflection read-only, the Discussion field open, Record Discussion and
       Return (Return needs a note);
     - completed, read-only, with its result announced;
     - credited nothing: completed while the EPA was paused, with a warning that it counted towards no curriculum
       requirement and why;
     - unavailable: an activity that does not name him reads exactly as an id that does not exist ("Activity
       unavailable. The requested activity could not be loaded."), and says nothing of permission.
  4. Recent Activities: a real page (the full list behind the dashboard's Recent decisions card) or no nav item. See
     question 1.
  5. Completing on a phone at 390 px: from the inbox, open the request, rate 3b, write the three feedback fields,
     complete, and read the result, with nothing scrolling sideways.

STEPS: 2.36, 3.4, 3.5, 3.11, 3.13, 3.15, 3.17, 3.24, 3.26, 3.28, 3.33, 3.51, 5.25, 6.18, A.4.3, A.6.8, A.7.2. Pasted
  verbatim in the next message (Role / Route / Do / Expect). Where an Expect quotes today's wording, that is today's
  page, not a requirement on the new one. Steps 2.36, 3.24, 3.33, 3.51 and A.6.8 describe today's dashboard cards,
  which T297 replaces.

STATES TO SHOW:
  Dashboard: empty; one request waiting; several waiting, one overdue; recent decisions; after a switch from the
    committee view; narrow.
  Inbox: empty; two rows of different states (Requested, Awaiting review); a paused EPA; loading; narrow.
  Activity page: unavailable; to rate; decline note open; decline refused; awaiting discussion; completed (result
    announced); credited nothing; narrow.
  Recent Activities: the chosen answer.
  Data volumes: an inbox of 0, 2 and 25 rows; EPA names up to about 120 characters; a registrar's name of 30 characters.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T297 (being fixed): "Pending requests" counts what the inbox holds; the "Accepted, needing action" card becomes
    the actionable items, oldest first, flagged overdue; "Recent decisions" is the activities he last moved that are
    finished or have no move left.
  - T299: the result of Complete, Decline, Return or Record Discussion is announced and takes the focus. A refused
    Decline or Return keeps its note panel open with the note as typed.
  - T323: the rating form fits 390 px: one card deep, inputs at least about 290 px wide, the six rungs and the feedback
    fields in the width, Complete and Decline reachable without scrolling sideways.
  - T322: badge and alert colours reach 4.5:1 (the tokens are flow 01's).
  - T320: nothing emails an assessor when a request arrives, so the dashboard and inbox are the only notice. No copy
    may suggest otherwise.
  - T280: every link and row action is named by more than its instrument ("Open Mini-CEX for Anele Dlamini, PAED-001,
    16 Sep"), so no two share a name.
  - T325: every time is South African time with its zone.
  - T328: textareas and selects in the body font; badges keep their pill shape beside a wrapping link.
  - Observed, not filed: the activity page must say whose work it is (the registrar's name) near its heading.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Recent Activities: design it (which list, which columns, how far back) or drop the nav item?
  2. Does the Assessor need dashboard cards at all, or is the inbox the Assessor's home?
  3. How does a two-role consultant see both queues (committee and assessor) without switching back and forth?
  4. What marks an item overdue? The dashboard's threshold is 7 days untouched, and 7 untouched days is also what counts
     as stalled for the coordinator.

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

ASK:
  - 2–3 variations as wireframes first: the Assessor dashboard (or the inbox as home), the inbox, and Recent
    Activities or its absence, at 1280 and 390 px. After I pick one, full fidelity, with the activity page extending
    flow 03's chosen design (same sections, locked sections, action bar, note panel, result region and history).
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    breaks.
  - Flag edge cases: 25 rows waiting; every row overdue; a registrar with two requests to the same assessor on one day;
    a two-role consultant with work in both queues.
  - Run an accessibility review against WCAG 2.1 AA: row actions named per row, focus after each move, the note panel
    named, and the rung picker usable by keyboard.

ATTACHED: act-3/3.24-2-patel-inbox-two.png, states/activity-view--to-rate.png, states/activity-view--decline-note.png,
  states/activity-view--decline-refused.png, states/activity-view--awaiting-discussion.png,
  states/activity-view--credited-nothing.png, act-A/A.7.2-5-completed-390.png,
  states/activity-view--unavailable.png, states/activity-inbox--assessor.png; flow 03's chosen activity-page
  artboards.
```

---

## 2. The journey

| Step | Page | The assessor does | They must be able to see |
|---|---|---|---|
| 2.36 | `/account/login` → `/` | Dr Patel and Dr Khumalo sign in for the first time | the Assessor view with nothing waiting; a nav of Activity Inbox and Recent Activities |
| 3.4 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel looks for a request that does not name him, then opens it by its address | "Inbox clear"; "Activity unavailable", identical to an id that does not exist |
| 3.5 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Naidoo rates Dr Dlamini's Mini-CEX at rung 4 and completes it | the row names the registrar, EPA, date and state; only Entrustment and Feedback open; the six rungs; Completed, credited "1 item" |
| 3.11 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Khumalo declines, first with no note, then with a reason | the note panel; "Decline requires a note." with the panel still open; Declined, her note in the history; the request gone from her inbox |
| 3.13 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Botha completes the re-filed Mini-CEX at 3a | Completed, credited "1 item" |
| 3.15 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Botha returns a reflection with a note | Awaiting discussion; Record Discussion and Return; Return asks for a note; afterwards, nothing left for her to do |
| 3.17 | `/activities/inbox` → `/activities/{ActivityId:int}` | she records the discussion on the re-submitted reflection | Discussed, read-only; credited "—", with no "counted towards no requirement" banner (it credits nothing by design) |
| 3.24 | `/` → `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel goes from the dashboard to the inbox and completes a DOPS | two rows of different states; the DOPS Completed; the portfolio review still waiting |
| 3.26 | `/activities/inbox` → `/activities/{ActivityId:int}` | five consultants complete Dr Molefe's six | each inbox holds only that assessor's own requests |
| 3.28 | `/activities/inbox` → `/activities/{ActivityId:int}` | three complete Dr Dlamini's three | each Completed, credited "1 item" |
| 3.33 | `/` → `/dashboard/switch/{role}` → `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Zulu switches to her Assessor view and completes a stalled Mini-CEX | the switch; the waiting count that matches her inbox (after T297); the request's age |
| 3.51 | `/` → `/activities/inbox` → `/placeholder/{Feature}` | Dr Khumalo reads her dashboard, follows it to the inbox, then opens Recent Activities | the waiting count and her recent decisions; Recent Activities (question 1) |
| 5.25 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Naidoo rates a request filed on the registrar's last day | Completed, read-only to both |
| 6.18 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel completes a Mini-CEX whose EPA was paused | "(no longer in use)" on the row and the page; not refused; the warning that it counted towards no requirement, and why |
| A.4.3 | `/activities/inbox` → `/account/session-ended` → `/account/login` | Dr Khumalo's second browser is signed out after her password change | the session-ended notice (F02's page); back to her inbox after signing in |
| A.6.8 | `/account/login` → `/` | Dr Patel signs in again after being reactivated | his Assessor Home |
| A.7.2 | `/activities/inbox` → `/activities/{ActivityId:int}` | Dr Patel completes a Mini-CEX on his phone | the row's action named for it; the request read-only; the ladder and feedback in the width; Complete and Decline without sideways scrolling; Completed |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Home and the role dashboards,
§ Activities and § System pages.

| Page | State | Screenshot | What it shows |
|---|---|---|---|
| Home | Assessor, first sign-in | `states/home--assessor-empty.png` | 0 awaiting, "Nothing is awaiting your review.", "No decisions yet." (re-captured after T297) |
| Home | A request waiting | `states/home--assessor-pending.png` | Dr Zulu after the switch: 1 awaiting, the Mini-CEX badged Overdue (re-captured after T297) |
| Home | Recent decisions | `states/home--assessor-decisions.png` | Dr Khumalo: 1 awaiting; Completed in green, Declined in red (re-captured after T297) |
| Home | Narrow | `states/home--narrow-assessor.png` | Dr Patel at 390 px: 1 awaiting, Overdue; four decisions (re-captured after T297) |
| Inbox | Inbox clear | `states/activity-inbox--empty.png` | "Inbox clear. There are no activities waiting for your action." |
| Inbox | Requests | `states/activity-inbox--assessor.png` | Type, Subject, EPA, Encounter date, State, Updated, Open |
| Inbox | A paused EPA | `states/activity-inbox--paused-epa.png` | "(no longer in use)" on the EPA |
| Inbox | Loading | `states/activity-inbox--loading.png` | |
| Inbox | Narrow | `states/activity-inbox--narrow.png` | |
| Activity page | Unavailable | `states/activity-view--unavailable.png` | "Activity unavailable. …" |
| Activity page | To rate | `states/activity-view--to-rate.png` | Request read-only; Entrustment and Feedback open; Complete and Decline in the summary card |
| Activity page | Decline note open | `states/activity-view--decline-note.png` | |
| Activity page | Decline refused | `states/activity-view--decline-refused.png` | "Decline requires a note.", the panel closed (T299) |
| Activity page | Awaiting discussion | `states/activity-view--awaiting-discussion.png` | Record Discussion and Return |
| Activity page | Credited nothing | `states/activity-view--credited-nothing.png` | the warning, and "(no longer in use)" on the EPA |
| Recent Activities | Not built | none | T335 deleted the placeholder page (Page not found now); this flow designs the page (§ 6, question 1) |

Not captured, so describe them in words:
- **The dashboard after T297.** The four Assessor Home state captures above, and the step captures of Home in § 4,
  were re-taken after T297 landed (2026-09-26) and show its cards (BRIEF.md § 10). Design from § 1's description.
- **"Accepted, needing action" with a row.** No CPSA workflow has a state named `accepted`, so the replay only ever
  shows the card empty (`coverage.md` § Flows and states not played; Step 3.51). T297 replaces the card with the
  actionable items, oldest first, flagged overdue past `DashboardThresholds.AssessorDueDays`, 7 days
  (`src/Wombat.Application/Common/Options/DashboardThresholds.cs:7`).
- **A completed activity from the assessor's side, announced.** Today nothing is announced (T299). The completed state
  itself is `states/activity-view--completed.png` (the registrar's view) and `act-3/3.5-3-completed.png` (his).

## 4. Attach

Paths are relative to `design/baseline/`. Every one below was checked with `ls` on 2026-09-26.

**Key screenshots (attach these first, with § 1):**
1. `act-3/3.24-2-patel-inbox-two.png`
2. `states/activity-view--to-rate.png`
3. `states/activity-view--decline-note.png`
4. `states/activity-view--decline-refused.png`
5. `states/activity-view--awaiting-discussion.png`
6. `states/activity-view--credited-nothing.png`
7. `act-A/A.7.2-5-completed-390.png`
8. `states/activity-view--unavailable.png`
9. `states/activity-inbox--assessor.png`

Step 3.51 no longer captures the Recent Activities stub: T335 deleted the placeholder page, so its address is Page
not found now, and this flow designs the page (§ 6, question 1).

Also attach **F03's chosen activity-page artboards** from `design/flows/03-trainee-files-activity/`.

**States (attach as the chat asks):** `states/activity-inbox--empty.png`, `states/activity-inbox--paused-epa.png`,
`states/activity-inbox--loading.png`, `states/activity-inbox--narrow.png`; and, for the completed state,
`act-3/3.5-3-completed.png` and `act-3/3.24-3-dops-completed.png`.

**Re-captured after T297 landed (2026-09-26); attach as the chat asks for the dashboard:**
`act-3/3.24-1-patel-home.png` (two waiting, oldest first), `act-3/3.33-2-zulu-home-assessor-view.png` (one waiting,
badged Overdue, after the switch), `act-3/3.51-1-khumalo-home.png` (one waiting, and Recent decisions in green and
red) and `act-A/A.6.8-1-patel-signed-in.png` (the overdue portfolio review). Also re-captured on 2026-09-26, and
re-taken on 2026-09-27 by the T335 replay at each step's own moment (BRIEF § 10), so none is held:
- `act-2/2.36-1-patel-home.png`, `act-2/2.36-2-khumalo-home.png`, `act-2/2.9-3-patel-home.png`,
  `act-2/2.10-4-khumalo-home.png` (the empty dashboard: "Nothing is waiting for your rating.")
- `states/home--assessor-empty.png`, `states/home--assessor-pending.png`, `states/home--assessor-decisions.png`,
  `states/home--narrow-assessor.png` (§ 3)

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists.

| Task | What it means for the design | Evidence (under `design/baseline/`) |
|---|---|---|
| **T297** (group 1, landed in 7bf8ea7) | The dashboard's cards are redefined (§ 1): a count that matches the inbox, the actionable items oldest first with an overdue flag, and recent decisions as "finished or no move left". Brief the dashboard from those words; re-capture before checking the design against Home. | § 4's re-captured Home captures (Steps 3.24, 3.33, 3.51, A.6.8, re-checked 2026-09-26) and the four Home state captures, re-captured the same day |
| **T299** | The same result region, note panel and focus rules as F03, from the assessor's side: Complete, Decline, Return and Record Discussion. | `states/activity-view--decline-refused.png`, `act-A/A.7.1-4-submitted-requested.png` |
| **T323** | The rating form at 390 px: one card deep, full-width inputs, the history table reflowed. | `act-A/A.7.2-4-activity-390.png` (the form, its inputs 208 px), `states/activity-view--narrow.png` |
| **T322** | Badges in the inbox and on Recent decisions from F01's tokens. | `act-A/A.7.14-7-review-7-badges.png` (re-taken after T335's tokens: the badges now pass) |
| **T320** | The dashboard and inbox are the only notice. Their counts, their order and the overdue flag are the product's whole reminder system, apart from the daily nudge email (Step 3.32). | T320's symptom |
| **T280** | Row actions and dashboard links carry the registrar and the EPA or date. Today Recent decisions names links "<instrument> for <trainee>", so several rows can share a name. | T280's notes |
| **T325** | "Updated" and every history time in SAST, with the zone. | `states/activity-inbox--assessor.png` |
| **T328** | Body font in the feedback fields; badges that keep their shape beside a wrapping link. | `act-A/A.7.2-5-completed-390.png`, `states/home--narrow-assessor.png` (both re-taken after T335: the feedback is in the body font, and the badges keep their pill) |
| **T329** | The inbox's loading state from F01's shared states. | `states/activity-inbox--loading.png` |
| Observed, not filed | The activity page never names the registrar: its summary lists Type, Encounter date, State and Version (`ActivityView.razor:73–79`). The inbox's subtitle reads "Activities that the current user can act on." | `states/activity-view--to-rate.png`, `states/activity-inbox--assessor.png` |

## 6. Questions the design must answer

1. **Recent Activities: design it, or drop the nav item?** If designed: which list (everything he has moved, or
   everything naming him), which columns, and how far back? BRIEF.md § 7 B7 places it here. Its contents are inferred:
   no intent document exists.
2. **Does the dashboard need its own cards, or is the inbox the Assessor's home?** An Assessor-only consultant's
   nav is two items (Step 2.36). F01 decides the Home frame; this decides what fills it for an assessor.
3. **How does a two-role consultant see both queues without switching?** Dr Zulu's Home opens as the committee view,
   and she must switch to see her waiting request (Step 3.33). F01's question 3 decides where the switch lives.
4. **What marks an item overdue?** The dashboard's threshold is 7 days untouched
   (`DashboardThresholds.AssessorDueDays`), the same 7 days after which a request counts as stalled for the coordinator (`act-3-operations.md:60`). Is overdue
   a badge, an order, a count, or all three?

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **T297 has landed** (7bf8ea7). It rewrote the Expect lines of Steps 3.24, 3.33, 3.51 and A.6.8 (and others outside
  this flow), re-pasted in § 8. Replay against those.
- **Replay the 17 steps** on a fresh database, playing the acts up to each one: 2.36; Act 3's 3.4–3.33 and 3.51; 5.25;
  6.18; and the appendix's A.4.3, A.6.8 and A.7.2. Every Expect must hold. Steps 3.4, 3.11 and 3.51 quote the page's
  words: update them in the same task if the wording changes (BRIEF.md § 9 item 7).
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`). They
  must include:
  - `Dashboards/` (`AssessorDashboardSubjectNameTests`, `DashboardCardTests`, `DashboardPriorityTests`);
  - `Navigation/DashboardLinkAuthorizationTests`, and `NavMenuAuthorizationTests` if Recent Activities is built or
    dropped (`PlaceholderPage.Headings` must change with it);
  - `Activities/ActivityViewAssessorSurfaceTests`, `ActivityWorkflowActionsTests` and `ActivityViewCreditSignalTests`;
  - `Accessibility/ActionFocusTests` and `RowNamesTests`; `Design/RowActionMarkupTests`, `NarrowLayoutTests` and
    `DefinedClassTests`;
  - `Scenario/`.
- **Re-capture** the 16 states in § 3 and the steps' own captures. Compare them
  with the chosen artboards.
- **Browser check** at 1280 and 390 px as Dr Patel (Assessor only) and as Dr Zulu (two roles): each dashboard figure
  matches the page it opens.

## 8. The runbook steps, verbatim (paste this second)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as it is today. The Expects of 3.24, 3.33, 3.51 and A.6.8 were re-pasted on 2026-09-26, after
T297 rewrote them; 2.36 refers to Step 2.34's empty dashboard, which T297 also changed. `D` is the replay day (`act-3-operations.md` § The clock in this act).

```text
Step 2.36 — Dr Patel and Dr Khumalo, assessors (act-2-onboarding.md:623)
Role: Assessor — Dr Mohammed Patel and Dr Fatima Khumalo
Route: /account/login → /
Do: Each signs in and reads the dashboard and nav.
Expect: Each sees "Assessor · Semester N, YYYY" on Home, and the empty Assessor dashboard of Step 2.34. The sidebar
  reads "Acting as Assessor" with no switch, over Home and Activity inbox, then My data rights.

Step 3.4 — An assessor who was not named cannot open the request (act-3-operations.md:135)
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Look for Dr Dlamini's Mini-CEX in the inbox, then open its address directly, using the id from Step 3.1.
Expect: The inbox reads "Inbox clear", because no request names him yet. The activity's page reads "Activity
  unavailable", exactly as it does for an id that does not exist, so walking ids discloses nothing (T101).

Step 3.5 — Dr Naidoo rates the Mini-CEX and completes it (act-3-operations.md:146)
Role: Assessor — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Choose Switch to Assessor in the sidebar, then open Activity inbox from the menu and Dr Dlamini's Mini-CEX in it.
  Rate the supervision the encounter required at rung 4, write what was done well, the areas for development and the
  agreed plan, and complete it.
Expect: The inbox row names Anele Dlamini, PAED-001, `D−10` and Requested. On the page, only Entrustment and Feedback
  can be filled in, and Complete and Decline are offered. The rating offers the six rungs by the College's labels, 1, 2,
  3a, 3b, 4 and 5 (D32). After Complete, the state is Completed and the page is read-only. The history's Complete row
  reads Requested → Completed, by David Naidoo, credited "1 item". Year 3's minimum on PAED-001 is rung 4, so the
  encounter counts at the minimum.

Step 3.11 — Dr Khumalo declines the request, with a reason (act-3-operations.md:243)
Role: Assessor — Dr Fatima Khumalo
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Ndlovu's Mini-CEX. Press Decline and apply it with the note left empty. Then apply it with the note "I was
  not on the ward that day; Dr Botha observed this encounter. Please send it to her."
Expect: Decline opens a transition note. Applied with the note empty, it is refused with "Decline requires a note."
  Applied with the note, the state is Declined. The page is read-only, with no action left. The history's Decline row
  shows her note, credited "—". Her inbox no longer lists the request.

Step 3.13 — Dr Botha completes Dr Ndlovu's Mini-CEX (act-3-operations.md:282)
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open Activity inbox from the menu and Dr Ndlovu's re-filed Mini-CEX
  in it. Rate it 3a with feedback and complete it. Then choose Switch to Committee member in the sidebar.
Expect: The Mini-CEX is Completed, credited "1 item". Year 1's minimum on PAED-002 is 3a, so it counts at the minimum.
  The declined request stays Declined and credits nothing. Each switch lands on Home with its one-time info alert, "You
  are now acting as Assessor." and then "You are now acting as Committee member." (Step 2.34), and the sidebar ends
  reading "Acting as Committee member".

Step 3.15 — Dr Botha returns the reflection for more detail (act-3-operations.md:320)
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open Dr Ndlovu's reflection from Activity inbox. Return it with the
  note "Please say what you would do differently at triage, and what you will read before your next take." Then choose
  Switch to Committee member in the sidebar.
Expect: The inbox lists the reflection as Awaiting discussion. The page offers Record Discussion and Return, and only the
  Discussion field is open. Return asks for a note. Once it is returned, the state is Draft and the page offers
  Dr Botha nothing. The history's Return row (Awaiting discussion → Draft) carries her note.

Step 3.17 — Dr Botha records the discussion (act-3-operations.md:353)
Role: Assessor — Dr Sarah Botha
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, then open the re-submitted reflection from Activity inbox, write the
  discussion notes, and record the discussion. Then choose Switch to Committee member in the sidebar.
Expect: The state is Discussed, finished and read-only to both of them. The Record Discussion row is credited "—":
  the exercise credits nothing (D7), and no "counted towards no curriculum requirement" banner shows (T108). Dr Ndlovu's
  progress does not change.

Step 3.24 — Dr Patel completes Dr Mahlangu's DOPS (act-3-operations.md:449)
Role: Assessor — Dr Mohammed Patel
Route: / → /activities/inbox → /activities/{ActivityId:int}
Do: From the dashboard, open the inbox, then the DOPS. Rate it 3a with feedback, and complete it.
Expect: Home (Assessor view): "Waiting for your rating" is badged 2 and lists the same two rows as the inbox, oldest
  first: Pieter du Plessis's portfolio review (Awaiting review), then Nomsa Mahlangu's DOPS
  (Requested) (T297). The inbox holds those two rows. After Complete, the DOPS is Completed, credited "1 item". Year 1's
  minimum on PAED-002 is 3a, so it counts at the minimum. The portfolio review stays in his inbox, Home's card is then
  badged 1, and Recent decisions lists the DOPS as Completed.

Step 3.26 — The consultants complete Dr Molefe's six (act-3-operations.md:488)
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
Expect: Each inbox holds only that assessor's own requests. All six end Completed, each credited "1 item". Year 4's
  minimum is rung 5 on PAED-001, PAED-010 and PAED-012. So Dr Patel's rung 4 counts towards the target, but not at the
  minimum.

Step 3.28 — The consultants complete Dr Dlamini's three (act-3-operations.md:524)
Role: Assessor — Dr Thandi Zulu, Dr Sarah Botha and Dr Fatima Khumalo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: Each completes their own from Activity inbox with feedback, rating: Dr Zulu 3b on the CBD, Dr Botha 4 on the CCA,
  and Dr Khumalo 4 on the Mini-CEX. Dr Zulu and Dr Botha first choose Switch to Assessor in the sidebar, and after
  rating choose Switch to Committee member (Step 3.13).
Expect: All three end Completed, each credited "1 item". Dr Dlamini's PAED-001 now holds three encounters this semester.
  Two are at year 3's minimum of rung 4; the CBD at 3b counts towards the target only. Her PAED-004 holds one, at the
  minimum.

Step 3.33 — Dr Zulu switches to her assessor view and completes the stalled Mini-CEX (act-3-operations.md:611)
Role: Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Her sessions open acting as a Committee member; choose Switch to Assessor in the sidebar. From "Waiting for your
  rating", open Dr Mahlangu's Mini-CEX, rate it 3a with feedback, and complete it.
Expect: The sidebar first reads "Acting as Committee member", with "Switch to Assessor" under it, and Home "Committee
  member · Semester N, YYYY" under its heading. After the switch Home reads "You are now acting as Assessor." in an info
  alert under the header, and "Assessor · Semester N, YYYY"; the sidebar reads "Acting as Assessor" over Home and
  Activity inbox, and "Waiting for your rating" is badged 1: this Mini-CEX.
  The card lists it, "Mini-CEX (Paediatrics) — Nomsa Mahlangu", badged Overdue: it has waited past the
  assessor's seven days since Step 3.30 aged it. After Complete, the Mini-CEX is Completed, credited "1 item". Year 1's
  minimum on PAED-004 is 3a. Nothing of Dr Mahlangu's is stalled any more.

Step 3.51 — Dr Khumalo's dashboard, and Recent Activities (act-3-operations.md:945)
Role: Assessor — Dr Fatima Khumalo
Route: / → /activities/inbox
Do: Read the dashboard, then follow "Open inbox →". Then read the menu.
Expect:
  - Waiting for your rating, badged 1: "Case-Based Discussion (Paediatrics) — Pieter du Plessis", Requested (T297,
    T335). The inbox lists it as Requested.
  - Recent decisions, the activities she moved last, newest first: Mini-CEX (Paediatrics) — Anele Dlamini, Completed,
    in green; Case-Based Discussion (Paediatrics) — Lerato Molefe, Completed; Mini-CEX (Paediatrics) — Sipho Ndlovu,
    Declined, in red.
  - The inbox lights Activity inbox in the menu: Home and Activity inbox, then My data rights. There is no Recent
    activities: the flow 01 pick dropped it, with its placeholder.

Step 5.25 — Dr Naidoo rates it (act-5-graduation.md:593)
Role: Assessor — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int} → /dashboard/switch/{role} → /
Do: He has acted as a Committee member since Step 4.5: choose Switch to Assessor in the sidebar, then open Dr du
  Plessis's Mini-CEX from Activity inbox and rate it `3b`, PAED-002's minimum in training year 2. Write what was done
  well, what to develop and the agreed plan, and complete it. Then choose Switch to Committee member in the sidebar.
Expect: The activity reads Completed and is read-only to both of them. It credits PAED-002 in the semester that holds
  `D−1` (checked in Step 5.26), and My activities reads it credited "1 item". Nobody is emailed.

Step 6.18 — Dr Patel completes the Mini-CEX during the pause (act-6-catalogue.md:460)
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: Open Dr Dlamini's Mini-CEX from the inbox. Rate it at the rung that Dr Dlamini's PAED-012 card named as the minimum
  now (Step 6.15). Write the three feedback fields, and complete it.
Expect:
  - The inbox row and the activity's EPA both read "PAED-012 — Communicating with and counselling patients, caregivers
    and healthcare teams (no longer in use)".
  - The completion is not refused (D48). The activity reads Completed and is read-only.
  - The activity page warns that it counted towards no curriculum requirement, and names as one cause an EPA that was
    not in use at the time. In its history, the completion's Credit reads None.

Step A.4.3 — Dr Khumalo's other session ends (appendix-cross-cutting.md:569)
Role: Assessor — Dr Fatima Khumalo (the second browser)
Route: /activities/inbox → /account/session-ended → /account/login → /activities/inbox
Do: Go back to the second browser and wait up to a minute. Sign in with the old password, then with the new one.
Expect: The tab leaves for the sign-in page by a full page load, which says "Your session has ended. Sign in again."
  (T279), an information notice. The old password is refused, and "Invalid email or password." takes the notice's
  place. The new one brings her back to her Activity inbox: the return address survives the refusal.

Step A.6.8 — Dr Patel signs in again (appendix-cross-cutting.md:937)
Role: Assessor — Dr Mohammed Patel
Route: /account/login → /
Do: Sign in with his own password.
Expect: He lands on his Assessor dashboard. "Waiting for your rating" is badged 1 and lists Dr du Plessis's Portfolio
  and Logbook Review, badged Overdue, as his inbox lists it (T297, T335). Dr Dlamini's
  assessor list names him again (checked at A.7.1).

Step A.7.2 — Dr Patel completes it on his phone (appendix-cross-cutting.md:1024)
Role: Assessor — Dr Mohammed Patel
Route: /activities/inbox → /activities/{ActivityId:int}
Do: At 390 px, open Dr Dlamini's Mini-CEX from Activity inbox. Rate the supervision at `3b`, write the three
  feedback fields, and complete it.
Expect: The inbox lists it with an action named for it. On the activity, her request is read-only to him, and the
  six-rung ladder and the feedback fields fit the width. Complete and Decline are reachable without scrolling
  sideways. Once completed, it reads Completed and is read-only to both of them. Nobody is emailed (Step 3.3).
```
