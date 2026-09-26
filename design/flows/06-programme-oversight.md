# F06 Watching the programme: who is behind, and what has stalled

The committee, the programme director, the sub-speciality lead and the coordinator look each week at who is behind
this period's targets and which requests have stalled. It is the programme's early warning; today three of its nav
items are "Coming soon" stubs and its stalled list misses most of what has stalled.

Written 2026-09-26 for T332, following `design/BRIEF.md` § 2.4. Paths to screenshots are under `design/baseline/`
(gitignored). Claims about Claude Design use BRIEF § 2.0's source keys.

| | |
|---|---|
| People | CommitteeMember: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha (each also an Assessor), Dr John van Rensburg (external). SpecialityAdmin: Dr Refilwe Mokoena. SubSpecialityAdmin: Dr Kabelo Sithole. Coordinator: Mr Pieter Smit (`README.md` § Cast) |
| Pages | `/` (`Dashboards/CommitteeMemberDashboard.razor`, `SpecialityAdminDashboard.razor`, `SubSpecialityAdminDashboard.razor`, `CoordinatorDashboard.razor`), `/placeholder/{Feature}` (`Placeholder/PlaceholderPage.razor`), `/activities/inbox`, `/committee/panels`, and the endpoint `/dashboard/switch/{role}` (`coverage.md` § Pages, § Endpoints) |
| Steps | 18: 2.33, 2.35, 2.37, 2.38, 3.30, 3.31, 3.32, 3.52, 3.53, 3.54, A.2.8, A.5.10, A.5.11, A.5.12, A.7.5, A.7.6, A.7.7, A.7.8. Steps 3.32 and A.2.8 are the Administrator running the reminder jobs on `/admin/jobs` (F18's page), seen here for what they chase. |
| Frequency and stakes | Weekly. Medium-high: this is where a registrar falling behind, or a request nobody has answered, is caught before the committee sits. |
| Mode | **Wireframe first.** Four role dashboards and three of the five "Coming soon" pages live here; decide them before fidelity (BRIEF § 2.3, step 3; § 7 B7). |
| Held | **Nothing.** T297 (group 1, landed in 7bf8ea7) changed the Coordinator's, SpecialityAdmin's and SubSpecialityAdmin's cards. 23 of their 24 captures were re-captured on 2026-09-26, and one (`3.53-2`) no longer has a step (§ 5.3; BRIEF § 10). The CommitteeMember's dashboard and the placeholders were never held. |

---

## 1. How to run this thread

1. Run F01 (the shell and the nav) first. Whether each "Coming soon" item keeps its nav slot is decided here, but the
   nav's grouping is F01's (BRIEF § 2.3, step 2).
2. T297 has landed and the captures in § 5.3 were re-taken on 2026-09-26, so every screen can be briefed.
3. Start a new thread. Attach the key screenshots in § 5.1, then paste § 2.1 as one message.
4. Pick a wireframe. Record in the chat, one sentence each, whether each placeholder is designed or dropped and whether
   triage is in scope. Then paste § 2.2 for fidelity.
5. Attach the state captures in § 5.2 as the chat asks for them.
6. Export the chosen artboards to `design/flows/06-programme-oversight/` before moving on. There is no version history
   [start].

## 2. The ask

### 2.1 Paste this first, with the key screenshots attached

```
FLOW 06 — Watching the programme: who is behind, and what has stalled

GOAL: Programme staff must, once a week:
- see each registrar against this period's targets, and which EPAs the programme as a whole is short on;
- find the requests that have waited too long for an assessor, and chase them;
- reach the queues they own in one step.
What is wrong today:
- The Coordinator's "Stalled requests" card lists only work waiting in one literal state, so it misses every
  workplace-based assessment waiting for its assessor. Its rows link nowhere. No page lets him send a reminder or
  reassign a request; only a nightly job mails the assessor.
- The programme director's "Pending reviews" tile reads "1 activities in review". Its "Review queue →" link opens her
  own inbox, which reads "Inbox clear", because she assesses nobody.
- The committee's external member sees "No trainees have targets this period." though he may read every registrar at
  the hospital.
- On the committee's "Targets this period", registrars with equal figures are ordered by an internal id.
- Neither committee card holds a link.
- Three nav items open a "Coming soon" stub: Programme Trainees (CommitteeMember, SpecialityAdmin, SubSpecialityAdmin),
  Stalled Activities (Coordinator) and STAR Review Queue (SpecialityAdmin, SubSpecialityAdmin).

AUDIENCE:
- CommitteeMember: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha (senior paediatric consultants who also
  assess), and Dr John van Rensburg (external examiner from another university).
- SpecialityAdmin: Dr Refilwe Mokoena (programme director). SubSpecialityAdmin: Dr Kabelo Sithole (sub-speciality
  training lead). They run the programme, form panels and schedule reviews.
- Coordinator: Mr Pieter Smit (programme administrator, not a clinician). He runs feedback campaigns, chases stalled
  work and schedules reviews.
- Kgosi Kgari Teaching Hospital, Paediatrics, five registrars. Desktop 1280×800 and phone 390×844.

WHAT THE DASHBOARDS CARRY TODAY:
- Home's header: "Welcome, zulu@kgk.wombat.local" (it greets by email today) and "Viewing as CommitteeMember". A person
  with two roles also reads "You also act as Assessor. Switch view: Assessor" (Dr Zulu, Dr Naidoo, Dr Botha).
- CommitteeMember:
  - "Targets this period": "Semester 2, 2026 · July to November", then each registrar "semester 1/10 · yearly 0/5",
    fewest met first.
  - "Targets met by EPA": each of 15 EPAs with its target "(3 per semester)", "2 of 5 met" and a bar.
- SpecialityAdmin and SubSpecialityAdmin:
  - "Pending reviews": a count and "Review queue →".
  - "Trainees in programme": "5 active / 0 inactive".
  - "Curriculum coverage — Semester 2, 2026": the same per-EPA list as the committee's "Targets met by EPA", under
    another title.
- Coordinator:
  - "Stalled requests": a warning card when it has rows, each "type — trainee · the day it last moved". A request is
    stalled after 7 untouched days.
  - "Invitations nearing expiry": three days ahead.
  - "Quick action": Start an MSF campaign.
- Figures are always "n of m" for a named window, never a percentage.

WHAT CHANGES WITH THE FIX NOW LANDING (T297), which the design must assume:
- "Stalled requests" lists every activity waiting for a reviewer and untouched for 7 days that the Coordinator may read,
  oldest first. Each row names the trainee and links to the activity.
- "Pending reviews" counts the programme's backlog waiting for a reviewer, worded "1 activity awaiting review" (with a
  singular). It no longer links to the inbox: it links to a page that lists those rows, or has no link until one
  exists.

NAVS TODAY (each page a nav item opens must admit that role, and no two items share a label):
- CommitteeMember: Programme Trainees, Decision Panels, Committee Reviews. Holders of Assessor also get Activity Inbox
  and Recent Activities.
- SpecialityAdmin and SubSpecialityAdmin: Programme Trainees, Decision Panels, Committee Reviews, STAR Review Queue,
  Decisions Due.
- Coordinator: Data Rights Requests, MSF Campaigns, Committee Reviews, Decisions Due, Stalled Activities. Decision
  Panels admits him, but his nav has no link to it.

WHAT EXISTS TO BUILD ON (and what does not):
- Programme Trainees must admit CommitteeMember, SpecialityAdmin and SubSpecialityAdmin. The existing trainee list
  (/admin/trainees) admits only the Administrator and the InstitutionalAdmin, so it cannot simply be linked.
- None of these roles can open a registrar's own progress page. The committee reads one registrar's standing, MSF
  coverage and rating trajectory only inside a review (Flow 07). A roster row that opens one registrar would need a new
  staff page.
- STAR Review Queue: the existing "Entrustment decisions" list (/admin/entrustment-decisions, designed in Flow 09)
  already admits both speciality admins. It lists every STAR with its status (Active, Expired, Revoked, Superseded),
  filters and a Revoke action. Their nav has no link to it today.
- Triage: a nightly job already emails the named assessor about anything untouched for 5 days. "Send a reminder now"
  and "Reassign to another assessor" are not built. A design that includes them becomes a build task; show them as
  proposals, clearly marked.

MODE: Wireframes first for all six screens. Fidelity after I pick.

SCREENS, in order:
1. / — the CommitteeMember dashboard: nobody to show, with figures (the Targets card), and 390 px. Include the switch
   line.
2. / — the SpecialityAdmin and SubSpecialityAdmin dashboards: with figures, and 390 px.
3. / — the Coordinator dashboard: nothing waiting, stalled requests (linked rows), invitations nearing expiry, and
   390 px.
4. Programme Trainees: a programme roster, each registrar with training year, this period's semester and yearly
   targets met, and shortfalls. Or no nav item.
5. Stalled Activities: the stalled-request list, with triage (remind, reassign) as a proposal. Or no nav item.
6. STAR Review Queue: renamed "Entrustment decisions" and pointed at the existing list, or dropped.

STEPS: 2.33, 2.35, 2.37, 2.38, 3.30, 3.31, 3.32, 3.52, 3.53, 3.54, A.2.8, A.5.10, A.5.11, A.5.12, A.7.5, A.7.6,
A.7.7 and A.7.8, pasted verbatim at the end of this message (Role / Route / Do / Expect). Steps 3.30, 3.53 and 3.54
describe the cards before T297; design to T297's outcome above.

STATES TO SHOW:
- Each dashboard: empty, typical, heavy (16 EPAs with an institution's own item; many stalled rows), loading, load
  error, 390 px.
- Programme Trainees and Stalled Activities, if designed: empty, typical, a filter with no match ("no match" is not
  "empty"), loading, load error, 390 px.
- A person with two roles, before and after "Switch view".
- The external committee member: after T290 he sees the institution's current registrars. The empty card remains
  only for an institution with no current registrars.

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
- T297: "Stalled requests" includes every request waiting for its assessor, and each row is a link naming the trainee.
  The speciality admins' count reads "N activity/activities awaiting review", with a singular. "Review queue →" is
  removed or re-pointed at a page that lists the rows it counts.
- T298: registrars tied on the Targets card are ordered by surname, then first name. Fewest met stays first.
- T290: a committee member with no sub-speciality (the external member) sees the institution's current registrars.
- T328: badges keep their pill shape in the dashboards' rows. A scrollable summary region shows the design's focus
  ring, not the browser's.
- T322: badge and alert text meets 4.5:1 contrast. The warning card's text (warning text on its tint) is 2.42:1 today.
- Every link opens a page that admits the role. No page is reached only by typing its address: the Coordinator's
  Decision Panels, and the speciality admins' Entrustment decisions.

QUESTIONS THE DESIGN MUST ANSWER:
1. Programme Trainees, Stalled Activities and STAR Review Queue: design each, or drop its nav item? Each must admit
   every role whose nav offers it.
2. Is stalled-work triage (send a reminder, reassign a request) in scope? It is not built; a design that includes it
   becomes a task.
3. Where does the SpecialityAdmin's "N awaiting review" lead, once "Review queue" no longer opens her empty inbox?
4. "STAR" now means Statement of Awarded Responsibility, the committee's formal entrustment; older text used it for a
   reflection. Should the queue be renamed "Entrustment decisions"?
Also say: should the committee's "Targets met by EPA" and the admins' "Curriculum coverage — <semester>" (one list)
carry one title?

CONSTRAINTS:
WOMBAT CONSTRAINTS (from design/BRIEF.md § 5)
Stack: Blazor Server (.NET 10), Razor components. Signed-in pages are interactive: every click is a server round trip
  over SignalR, so prefer explicit actions and flag any per-keystroke behaviour (typeahead, drag, live filtering).
Static pages: sign-in, register, forgot-password, link account, access denied and not found for a signed-out visitor,
  /msf/respond and /portfolio/verify are plain server-rendered HTML with form posts. No client-side behaviour beyond a
  ≤10-line script module; no live validation; the phone nav toggle is CSS-only.
Security policy (CSP): fonts, scripts, styles and images from this site only (images may be data: URIs). No Google
  Fonts, no CDN, no Tailwind CDN, no jQuery, no inline scripts or onclick attributes, no third-party calls or avatars.
  A new typeface must be a self-hosted woff2 with a GPLv3-compatible licence.
No CSS framework: no Bootstrap, Tailwind, MudBlazor or Radzen classes. Every class is defined in app.css. Name every
  colour as an existing token or a NEW token with its value; spacing on the scale xs 4, sm 8, md 16, lg 24, xl 32,
  2xl 48 px.
Icons: Lucide line icons only, named by their Lucide name.
Components (compose from these; mark anything else NEW): PageHeader (the page's one h1, subtitle, primary action),
  Breadcrumbs, DataTable (.clinic-table in .table-container, PagerControls), FormField / FormActions (.form-container,
  .form-grid, .form-actions), DashboardCard (.detail-card in .dashboard-grid), StatePanel (loading / empty / error),
  Skeleton, Alert (success / info / warning / danger), ActionResult, ConfirmDialog (native <dialog>), badges (five tints),
  Icon, TrajectoryChart (hand-drawn SVG, no chart library).
Design these framework states explicitly (no source file shows them): the active nav item; field validation (invalid
  border plus a stripe, not colour alone; message under the field; the summary); the reconnect dialog (rejoining,
  retrying, failed, paused, resume-failed); the in-app error bar; access denied; not found; session ended.
Content: people by name, states and types by label, times in South African time with the zone shown; an out-of-scope
  record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (sidebar included); targets ≥ 24 px; focus
  moves to an action's result; every page works at 390 px with no sideways scroll.
Viewports: 1280×800 and 390×844.

ASK: 2–3 variations. Wireframes first, then full fidelity. Name every design-system component you use and mark
anything else NEW. Say which DESIGN.md rule a variation breaks. Where a screen proposes an action that is not built
(triage) or a page that does not exist (Programme Trainees, a staff view of one registrar), label it PROPOSED. Flag
edge cases and draw the none, typical and heavy data volumes. Review the result for accessibility against WCAG 2.1 AA.

ATTACHED: states/home--committee-member-figures.png, act-3/3.52-1-zulu-committee-dashboard.png,
states/placeholder--programme-trainees.png, states/placeholder--star-review-queue.png,
act-A/A.7.8-4-sithole-trainees-stub.png, act-A/A.5.10-2-stalled-activities-stub.png,
act-A/A.7.5-3-decisions-due-summary-scrolled.png, and the Coordinator, SpecialityAdmin and SubSpecialityAdmin
dashboards re-captured after T297 (§ 5.3).

RUNBOOK STEPS, VERBATIM:

--- from execution/knowledge/scenario-paediatrics/act-2-onboarding.md ---

### Step 2.33 — Dr Zulu's first view: committee member
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: /account/login → /
Do: Sign in and read the dashboard and nav.
Expect: The page reads "Viewing as CommitteeMember" and "You also act as Assessor. Switch view: Assessor". Targets this
  period names the current semester and its months. It lists the five registrars, each at "semester 0/10 · yearly 0/5",
  and nobody is exempt: a 15 January start counts from the boundary (D42). Targets met by EPA lists PAED-001 to PAED-015,
  each "0 of 5 met" (T130's count, never a percentage). The nav reads Activity Inbox, Recent Activities, Programme
  Trainees, Decision Panels and Committee Reviews, each once (T178).

### Step 2.35 — Dr Naidoo and Dr Botha
Role: CommitteeMember + Assessor — Dr David Naidoo and Dr Sarah Botha
Route: /account/login → / → /committee/panels
Do: Each signs in, reads the dashboard and opens Decision Panels.
Expect: Each sees what Zulu saw in Step 2.33. Decision Panels lists the panel they sit on, with no New panel and no
  Edit.

### Step 2.37 — Dr van Rensburg, external committee member
Role: CommitteeMember — Dr John van Rensburg
Route: /account/login → / → /committee/panels
Do: Sign in, read the dashboard and open Decision Panels.
Expect: He sees "Viewing as CommitteeMember" with no Switch view line. Targets this period names the current semester
  and reads "No trainees have targets this period.", and Targets met by EPA reads "No curriculum targets for these
  trainees.": the card lists the trainees of the member's own sub-specialities at his institution, and he holds none.
  The nav reads Programme Trainees, Decision Panels and Committee Reviews. Decision Panels lists the panel he sits on as
  external member, with no New panel and no Edit.

### Step 2.38 — Dr Mokoena's and Dr Sithole's dashboards
Role: SpecialityAdmin and SubSpecialityAdmin — Dr Refilwe Mokoena and Dr Kabelo Sithole
Route: /
Do: Each opens Home, now that the registrars are admitted.
Expect: Each sees "Viewing as SpecialityAdmin" or "Viewing as SubSpecialityAdmin". Pending reviews reads 0 "activities
  in review" with Review queue. Trainees in programme reads 5 active / 0 inactive. Curriculum coverage names the current
  semester and lists PAED-001 to PAED-015 at "0 of 5 met". The nav reads Programme Trainees, Decision Panels, Committee
  Reviews, STAR Review Queue and Decisions Due.

--- from execution/knowledge/scenario-paediatrics/act-3-operations.md ---

### Step 3.30 — Mr Smit's dashboard shows the stalled requests
Role: Coordinator — Mr Pieter Smit
Route: /
Do: Once Dr Mahlangu's Mini-CEX and Dr du Plessis's portfolio review have waited more than seven days, open the
  dashboard.
Expect: "Stalled requests" is marked as a warning and lists both, oldest first, each by type, trainee and the day it
  last moved:
  - Portfolio and Logbook Review (Paediatrics) — Pieter du Plessis
  - Mini-CEX (Paediatrics) — Nomsa Mahlangu
  Neither row links anywhere. "Invitations nearing expiry" reads "No invitations expiring soon." The Quick action card
  offers "Start an MSF campaign".

### Step 3.31 — Mr Smit opens Stalled Activities
Role: Coordinator — Mr Pieter Smit
Route: /placeholder/{Feature}
Do: Choose Stalled Activities in the nav (`/placeholder/stalled-activities`). Then open a mistyped address,
  `/placeholder/stalled-work`.
Expect: Stalled Activities reads "This page is not built yet." with a "Coming soon" card. There is no page from which to
  chase a stalled request: no reminder and no reassignment. The mistyped address is "Page not found" with status 404,
  not a "Coming soon" (T178).

### Step 3.32 — The daily nudge reminds the assessors
Role: Administrator — devadmin@wombat.local
Route: /admin/jobs
Do: Run `assessor-pending-nudge` now.
Expect: The job's last run updates. The application log holds two stub emails, "Activities awaiting your assessment".
  The log names no address (T282); each greets its assessor by first name:
  - "Hi Thandi", listing Mini-CEX (Paediatrics) from Nomsa Mahlangu, waiting 8 days;
  - "Hi Mohammed", listing Portfolio and Logbook Review (Paediatrics) from Pieter du Plessis, waiting 8 days.
  The run's summary line reads "assessors nudged 2 (activities 2)" and skips nobody. Nothing else has waited five days,
  so nobody else is nudged.

### Step 3.52 — Dr Zulu's committee dashboard, and Programme Trainees
Role: CommitteeMember — Dr Thandi Zulu
Route: /dashboard/switch/{role} → / → /placeholder/{Feature}
Do: Switch the view back to CommitteeMember and read the dashboard. Then choose Programme Trainees in the nav.
Expect:
  - Targets this period, "Semester 2, 2026 · July to November": KGK's five current trainees, fewest met first.
    Pieter du Plessis, Nomsa Mahlangu and Sipho Ndlovu read "semester 0/10 · yearly 0/5". Anele Dlamini and Lerato
    Molefe read "semester 1/10 · yearly 0/5".
  - Targets met by EPA: PAED-001 (3 per semester) reads "2 of 5 met", and every other EPA "0 of 5 met".
  - Neither card holds a link.
  - Programme Trainees is "Coming soon".

### Step 3.53 — Dr Mokoena's dashboard, its review queue, and STAR Review Queue
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /activities/inbox → /placeholder/{Feature}
Do: Read the dashboard and follow "Review queue". Then choose STAR Review Queue in the nav.
Expect:
  - Pending reviews: 1 activity in review, Dr du Plessis's portfolio review.
  - Trainees in programme: "5 active / 0 inactive".
  - Curriculum coverage — Semester 2, 2026: the 15 EPAs with their targets. PAED-001 reads "2 of 5 met", and the rest
    "0 of 5 met", the same figures as Dr Zulu's.
  - "Review queue" opens an inbox that reads "Inbox clear", because she acts on no one's activity.
  - STAR Review Queue is "Coming soon".

### Step 3.54 — Dr Sithole's dashboard
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /
Do: Read the dashboard.
Expect: The same three cards and figures as Dr Mokoena's: 1 activity in review, "5 active / 0 inactive", and PAED-001
  "2 of 5 met". His nav offers the same Programme Trainees and STAR Review Queue placeholders.

--- from execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md ---

### Step A.2.8 — devadmin runs the two reminders against aged work
Role: Administrator — devadmin
Route: /admin/jobs
Do: Age the draft by 15 days and the request by 6 (see the Note). Then run activity-draft-nudge and
  assessor-pending-nudge now, and read the log.
Expect: The log holds a stub mail "You have draft activities waiting" beginning "Hi Nomsa,", listing "Mini-CEX
  (Paediatrics) — draft for 15 days". It also holds "Activities awaiting your assessment", beginning "Hi Fatima,",
  which lists "Mini-CEX (Paediatrics) from Nomsa Mahlangu — waiting 6 days". Each job logs one line counting whom it
  reminded and whom it skipped, and why (T151, T240).

### Step A.5.10 — Mr Smit's Stalled Activities
Role: Coordinator — Mr Pieter Smit
Route: / → /placeholder/{Feature}
Do: Read the dashboard's "Stalled requests" card, then open Stalled Activities from the nav.
Expect: Whatever the card lists, no row links anywhere, so there is no page to act on a stalled request from (Step
  3.30). Stalled Activities is the "Coming soon" stub, headed "Stalled Activities".

### Step A.5.11 — Dr Botha's Programme Trainees
Role: CommitteeMember — Dr Sarah Botha
Route: /placeholder/{Feature}
Do: Open Programme Trainees from the nav.
Expect: The "Coming soon" stub, headed "Programme Trainees".

### Step A.5.12 — Dr Mokoena's Programme Trainees and STAR Review Queue
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /placeholder/{Feature} → /placeholder/{Feature}
Do: Open Programme Trainees, then STAR Review Queue, from the nav.
Expect: Both are the "Coming soon" stub, each under its own heading.

### Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions Due and the data-rights queue.
Expect: The campaign page keeps its gutter (T226), and its invitee table (counts by respondent group: the page never
  lists an address) scrolls within its card. Decisions Due's summary scrolls sideways, and a keyboard can scroll it, as
  a labelled, focusable region (DESIGN.md's decisions-due contract). The queue's filters stack above its table.

### Step A.7.6 — Dr Zulu on her phone
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: At 390 px, open Committee Reviews and Dr Molefe's final review.
Expect: The review's cards stack. The evidence tables scroll inside their containers, and each EPA's trajectory chart
  fits the width (T166).

### Step A.7.7 — Dr Mokoena on her phone
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
Do: At 390 px, open her dashboard, Decision Panels, Decisions Due and, by its address, Entrustment decisions.
Expect: The dashboard's coverage cards stack. The panels list and the decisions list scroll inside their containers
  (T226).

### Step A.7.8 — Dr Sithole on his phone
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: / → /committee/reviews → /committee/decisions-due → /placeholder/{Feature} → /placeholder/{Feature}
Do: At 390 px, open his dashboard, Committee Reviews and Decisions Due, then Programme Trainees and STAR Review Queue
  from the folded nav.
Expect: As for Dr Mokoena, scoped to his sub-speciality. His nav offers the same two "Coming soon" stubs as hers
  (A.5.12), and each fits the width.
```

### 2.2 After you pick a wireframe, paste this

```
Take variation <letter> to full fidelity at 1280×800 and 390×844, in the design system you hold. For each of the four
dashboards, draw empty, typical, heavy, loading, load error and 390 px. For each page I chose to keep (Programme
Trainees, Stalled Activities, Entrustment decisions), draw empty, typical, no match, loading, load error and 390 px.
Mark every PROPOSED action. For each artboard, list the components used and mark NEW ones. For each colour, name the
token, or give a NEW token and its value. Then check contrast (text 4.5:1; borders and the focus ring 3:1) and every
target (at least 24 px).
```

## 3. The journey

One line per step: what the person does, and what they must be able to see. The step's full text is in § 2.1.

| Step | Page template | Who | Does, and must see |
|---|---|---|---|
| 2.33 | `/account/login` → `/` | Dr Zulu | First view as CommitteeMember, with the switch line. Five registrars at "semester 0/10 · yearly 0/5"; every EPA "0 of 5 met". |
| 2.35 | `/account/login` → `/` → `/committee/panels` | Dr Naidoo, Dr Botha | The same dashboard. The panel they sit on, with no New panel and no Edit (the panel page is F08's). |
| 2.37 | `/account/login` → `/` → `/committee/panels` | Dr van Rensburg | The external member: "No trainees have targets this period." He should see the institution's registrars (T290). |
| 2.38 | `/` | Dr Mokoena, Dr Sithole | Pending reviews 0 with "Review queue", "5 active / 0 inactive", coverage "0 of 5 met" per EPA, and two placeholder items in the nav. |
| 3.30 | `/` | Mr Smit | Stalled requests lists both requests, oldest first, each a link to its activity naming the trainee, with the day it last moved (after T297, re-checked 2026-09-26). |
| 3.31 | `/placeholder/{Feature}` | Mr Smit | Stalled Activities is "Coming soon": there is nowhere to chase a request. A mistyped placeholder address is Page not found (404). |
| 3.32 | `/admin/jobs` | devadmin (F18) | The nightly reminder runs: "Hi Thandi" and "Hi Mohammed", each listing the request that has waited 8 days. This is the only chase today. |
| 3.52 | `/dashboard/switch/{role}` → `/` → `/placeholder/{Feature}` | Dr Zulu | Back to CommitteeMember. Registrars fewest met first: Ndlovu, Mahlangu and du Plessis at 0/10, then Dlamini and Molefe at 1/10. PAED-001 "2 of 5 met"; no link in either card. Programme Trainees is "Coming soon". |
| 3.53 | `/` → `/placeholder/{Feature}` | Dr Mokoena | "2 activities awaiting review" counts the requested CBD and the portfolio review, in a card with no link (after T297, re-checked 2026-09-26). STAR Review Queue is "Coming soon". |
| 3.54 | `/` | Dr Sithole | The same three cards and figures, and the same two stubs. |
| A.2.8 | `/admin/jobs` | devadmin (F18) | The draft reminder ("Hi Nomsa, … draft for 15 days") and the assessor reminder (6 days) run; each job logs whom it reminded and skipped. |
| A.5.10 | `/` → `/placeholder/{Feature}` | Mr Smit | The card's rows link nowhere, and Stalled Activities is the stub. |
| A.5.11 | `/placeholder/{Feature}` | Dr Botha | Programme Trainees is the stub. |
| A.5.12 | `/placeholder/{Feature}` → `/placeholder/{Feature}` | Dr Mokoena | Programme Trainees and STAR Review Queue are both stubs. |
| A.7.5 | `/` → `/msf/campaigns` → `/msf/campaigns/{CampaignId:int}` → `/committee/decisions-due` → `/admin/data-rights` | Mr Smit | At 390 px, Home's three cards stack. His other pages are F08's, F10's and F14's; Decisions Due's summary region shows the browser's focus ring (T328). |
| A.7.6 | `/` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | At 390 px, Home's two cards stack. The review page is F07's. |
| A.7.7 | `/` → `/committee/panels` → `/committee/decisions-due` → `/admin/entrustment-decisions` | Dr Mokoena | At 390 px, three cards stack. Each coverage row's "0 of 2 met" wraps to three lines in its narrow column (observed, not filed). She reaches Entrustment decisions only by typing its address. |
| A.7.8 | `/` → `/committee/reviews` → `/committee/decisions-due` → `/placeholder/{Feature}` → `/placeholder/{Feature}` | Dr Sithole | At 390 px, as Dr Mokoena's, with the two stubs fitting the width. |

## 4. States to design

From `execution/knowledge/scenario-paediatrics/states.md` § Home and the role dashboards and § System pages. Each file is
`design/baseline/states/<page>--<state>.png`.

| Page | State | Capture | Reached at |
|---|---|---|---|
| `/` | CommitteeMember, nobody to show | `home--committee-member-empty.png` | Step 2.37 (a defect: T290) |
| `/` | CommitteeMember with figures | `home--committee-member-figures.png` | Step 3.52 |
| `/` | SpecialityAdmin | `home--speciality-admin.png` (re-captured after T297) | Step 2.38 |
| `/` | SpecialityAdmin with figures | `home--speciality-admin-figures.png` (re-captured) | Step 3.53 |
| `/` | SubSpecialityAdmin | `home--sub-speciality-admin.png` (re-captured) | Step 3.54 |
| `/` | Coordinator, nothing waiting | `home--coordinator-empty.png` (re-captured) | Step 2.32 |
| `/` | Coordinator, stalled requests | `home--coordinator-stalled.png` (re-captured) | Step 3.30, after its ageing SQL |
| `/` | Coordinator, invitations nearing expiry | `home--coordinator-expiring.png` (re-captured) | Scratch database (`states.md`) |
| `/` | 390 px, CommitteeMember | `home--narrow-committee-member.png` | Step A.7.6 |
| `/` | 390 px, Coordinator | `home--narrow-coordinator.png` (re-captured) | Step A.7.5 |
| `/` | 390 px, SpecialityAdmin | `home--narrow-speciality-admin.png` (re-captured) | Step A.7.7 |
| `/` | 390 px, SubSpecialityAdmin | `home--narrow-sub-speciality-admin.png` (re-captured) | Step A.7.8 |
| `/placeholder/{Feature}` | Programme Trainees | `placeholder--programme-trainees.png` | Step A.5.11 |
| `/placeholder/{Feature}` | STAR Review Queue | `placeholder--star-review-queue.png` | Step A.5.12 |
| `/placeholder/{Feature}` | Stalled Activities | `placeholder--stalled-activities.png` | Step 3.31 |

**A correction to the planner's list.** It named the Coordinator's third state "STARs expiring". The capture
`home--coordinator-expiring.png` is **invitations** nearing expiry (`states.md` § Home; `CoordinatorDashboard.razor:30`,
"Invitations nearing expiry"). No dashboard card lists STARs nearing expiry today; the product's STAR expiry reminder is a
mail (`EntrustmentDecisionExpiringReminderEmail`, named in T298).

**Not captured, to be drawn from their description:**
- The pages that do not exist yet, if designed: Programme Trainees, Stalled Activities with triage, and a staff view of
  one registrar (B7).
- The "holds Trainee" notes, which no cast member can meet (`states.md:779-781`).

**Belongs to other flows:** the Assessor's view after a switch (`home--assessor-switched`) is F04's. The InstitutionalAdmin's
dashboard is F12's and the Administrator's F18's. Recent Activities and System, the other two placeholders, are F04's and
F18's.

## 5. Attach

Every path below is under `design/baseline/` and was checked with `ls` on 2026-09-26. Open each before you upload it
(BRIEF § 3.3). None of them is an invitation page.

### 5.1 Key screenshots (attach with the ask)

1. `states/home--committee-member-figures.png`: the CommitteeMember dashboard with figures.
2. `act-3/3.52-1-zulu-committee-dashboard.png`: the same, from the story.
3. `states/placeholder--programme-trainees.png`: the stub, as a committee member sees it.
4. `states/placeholder--star-review-queue.png`: the stub, as the programme director sees it.
5. `act-A/A.7.8-4-sithole-trainees-stub.png`: a stub at 390 px.
6. `act-A/A.5.10-2-stalled-activities-stub.png`: the Coordinator's stub.
7. `act-A/A.7.5-3-decisions-due-summary-scrolled.png`: a scrolled summary region with the browser's focus ring (T328).

### 5.2 State captures (add as the chat asks)

- Dashboard and placeholder states:
  - `states/home--committee-member-empty.png`
  - `states/home--narrow-committee-member.png`
  - `states/placeholder--stalled-activities.png`
- Step captures, not held:
  - `act-2/2.33-1-zulu-committee-home.png`
  - `act-2/2.37-1-vanrensburg-home.png`
  - `act-3/3.31-1-stalled-activities-placeholder.png`
  - `act-3/3.31-2-mistyped-placeholder-404.png`
  - `act-3/3.52-2-programme-trainees-placeholder.png`
  - `act-3/3.53-3-star-review-queue-placeholder.png`
  - `act-A/A.5.12-2-star-review-queue.png`
  - `act-A/A.7.8-5-sithole-star-queue-stub.png`
  - `act-A/A.7.7-4-mokoena-entrustment-decisions.png` (the list STAR Review Queue could become)

### 5.3 Re-captured after T297 (formerly held)

T297 changed these pages' cards (BRIEF § 10). Every capture below was re-taken on 2026-09-26 after it landed, each from
the end of its act rather than the step's own moment, so none is held; attach them with § 5.1. At Step 2.10 Dr
Mokoena's and Dr Sithole's Homes already count the five admitted registrars, and `home--coordinator-stalled` lists both
stalled requests, as Step 3.30 now does (an SQL stand-in for the mid-act moment, BRIEF § 10):

`states/home--speciality-admin.png`, `states/home--speciality-admin-figures.png`,
`states/home--sub-speciality-admin.png`, `states/home--coordinator-empty.png`, `states/home--coordinator-stalled.png`,
`states/home--coordinator-expiring.png`, `states/home--narrow-coordinator.png`,
`states/home--narrow-speciality-admin.png`, `states/home--narrow-sub-speciality-admin.png`,
`act-2/2.38-1-mokoena-home.png`,
`act-2/2.38-2-sithole-home.png`, `act-A/A.5.10-1-stalled-requests-card.png`, `act-A/A.7.5-1-smit-home.png`,
`act-A/A.7.7-1-mokoena-home.png`, `act-A/A.7.8-1-sithole-home.png`, `act-2/2.8-2-smit-home.png`,
`act-2/2.32-1-smit-home.png`, `act-2/2.10-6-mokoena-home.png`, `act-2/2.10-7-sithole-home.png`,
`act-4/4.3-1-mokoena-dashboard-nav.png`

Re-captured from a fresh replay of Act 3 (2026-09-26); attach them with § 5.1 too:
`act-3/3.30-1-smit-stalled-requests.png` (both stalled requests, oldest first, each a link naming the trainee),
`act-3/3.53-1-mokoena-dashboard.png` and `act-3/3.54-1-sithole-dashboard.png` ("2 activities awaiting review", no
link). Not re-captured, and not to be attached: `act-3/3.53-2-review-queue-inbox-clear.png`, since the Pending reviews
card no longer links to the inbox it shows, so no step reaches it.

## 6. Known problems this design must solve

| Task | Problem today | What it means for the design | Evidence |
|---|---|---|---|
| T297 (group 1, P2, landed in 7bf8ea7) | Before the fix, the cards selected activities by literal state keys. The Coordinator's reads `submitted` only (`GetCoordinatorDashboardSummaryQuery.cs:44`), so every CPSA WBA waiting in `requested` is missed, and its rows are text (`CoordinatorDashboard.razor:22`). The speciality admins' tiles read `submitted` or `in_review`, say "activities in review" with no singular, and link to `/activities/inbox`, which lists only what the caller can move. | Stalled requests: linked rows naming the trainee, oldest first, for everything waiting on a reviewer for 7 days. Pending reviews: a singular label, and a link only to a page that lists what it counts. | § 5.3 (all re-captured after the fix, 2026-09-26); Steps 3.30, 3.53, 3.54 |
| T298 (P3) | Tied registrars are ordered by user id, a GUID (`CurriculumCoverage.cs:241-244`): "Ndlovu, Mahlangu, du Plessis" at 0/10, then "Dlamini, Molefe" at 1/10. | Fewest met first, then surname and first name. At 2.33 all five read Dlamini, du Plessis, Mahlangu, Molefe, Ndlovu. | `states/home--committee-member-figures.png` |
| T290 (P3; the committee item is Medium) | The committee card lists only trainees in the member's sub-speciality claims (`GetCommitteeMemberDashboardSummaryQuery.cs:34,55`). The external member holds none, so his card is empty. | The card reads the institution's current registrars. Design "nobody to show" only for an institution with none. | `states/home--committee-member-empty.png`; Step 2.37 |
| B7 (BRIEF § 7) | Programme Trainees, Stalled Activities and STAR Review Queue open `PlaceholderPage.razor`'s "Coming soon" card. `PlaceholderPage.Headings` must list exactly the features the nav links there (`NavMenuAuthorizationTests.ThePlaceholderPage_KnowsExactlyTheFeaturesTheNavLinksToIt`). | Design each, or drop its nav item. A nav change is a DESIGN.md change: `NavMenuAuthorizationTests` parses DESIGN.md's nav table (DESIGN.md:192-205; BRIEF § 4). Their contents are inferred: no intent document exists for any of them (searched `execution/knowledge`, `execution/architecture`, `DECISIONS.md`). | § 5.1 |
| Not built | Triage (remind, reassign): "No page offers it. The dashboard's 'Stalled requests' rows link nowhere" (`coverage.md` § Flows and states not played). Today's only chase is `AssessorPendingNudgeJob`, nightly, after 5 days (Step 3.32 Note). | Mark triage PROPOSED. A chosen design becomes a new task (`harness.py task new`). Who may reassign a named assessor is undecided (inference): the nominee rules (`NomineeGate`, CLAUDE.md) say whom a field may name, not who may change it. | Steps 3.31, A.5.10 |
| Reached only by address (`coverage.md` § Reached only by address) | `/committee/panels` admits the Coordinator, but his nav has no Decision Panels. `/admin/entrustment-decisions` admits the speciality admins, who reach it only by typing it. | Give each a link, or record why not. STAR Review Queue → Entrustment decisions would settle the second. F08 asks the first too. | Steps 2.32, A.7.7 |
| T328 (P3; BRIEF A4) | Dashboard rows are inline `display:flex`, so badges stretch. A focusable region (`DecisionsDue.razor:79`) shows the browser's 1 px ring. | A `.list-row` pattern for dashboard rows. A `:focus-visible` ring on focusable regions. | `act-A/A.7.5-3-decisions-due-summary-scrolled.png` |
| T322 (P2; BRIEF A1) | Warning text on its tint is 2.42:1, and the Stalled requests card is a warning card. | Body text on the tint, the semantic colour as a stripe or border. | `act-A/A.7.14-6-reviews-badges.png` |
| Observed, not filed | One component (`EpaTargetCoverageList`, DESIGN.md:745-750) is titled "Targets met by EPA" on the committee dashboard (`CommitteeMemberDashboard.razor:32`) and "Curriculum coverage — <semester>" on the admins' (`SpecialityAdminDashboard.razor:28`). At 390 px its "0 of 2 met" wraps to three lines (Step A.7.7). | One title; a figure column that does not wrap at 390 px. | `states/home--narrow-speciality-admin.png` (held) |

## 7. Questions the design must answer

1. **Programme Trainees, Stalled Activities, STAR Review Queue: design each, or drop its nav item?**
   - Each must admit every role whose nav offers it.
   - `/admin/trainees` does not admit these roles: Administrator and InstitutionalAdmin only (`coverage.md:71`).
2. **Is stalled-work triage in scope?** That means sending a reminder or reassigning a request. It is not built, and a
   design that includes it becomes a task.
3. **Where does the SpecialityAdmin's "N awaiting review" lead,** once "Review queue" no longer opens her empty inbox?
   - Stalled Activities, if it lists the programme's backlog?
   - Programme Trainees?
   - Nowhere?
4. **Rename STAR Review Queue to "Entrustment decisions"?** "STAR" now means Statement of Awarded Responsibility
   (`DOMAIN.md` § Entrustment decision; T296). Older text used it for a written reflection (`DOMAIN.md` § STAR
   reflection).
   - The page already exists as `/admin/entrustment-decisions` (F09).
   - It admits both speciality admins (`coverage.md:57`).

## 8. Acceptance

Claude Code builds the chosen design as one task per new page, plus one for the dashboards (BRIEF § 9). It is done when
all of the following hold.

**Tests.**
- `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green. Never use `--no-build` (CLAUDE.md § Testing).
- The run includes the tests that pin these pages:
  - `Navigation/NavMenuAuthorizationTests`, including the placeholder check;
  - `Navigation/DashboardLinkAuthorizationTests`;
  - `Dashboards/*`;
  - `Design/NarrowLayoutTests` and `Design/DefinedClassTests`;
  - the scenario guard in `Scenario/`, which fails if a new page has no step.
- A nav change updates DESIGN.md's nav table in the same task.

**The replay.**
- Replay the flow's steps on a fresh database (`tools/scenario-replay.ps1`, BRIEF § 9):
  - 2.33, 2.35, 2.37 and 2.38 (Act 2);
  - 3.30, 3.31, 3.32, 3.52, 3.53 and 3.54 (Act 3);
  - A.2.8, A.5.10, A.5.11, A.5.12, A.7.5, A.7.6, A.7.7 and A.7.8 (appendix).
- Each act may instead start from its predecessor's snapshot, `recovery/scenario-post-act<N>.dump` (README § How to
  play, "Snapshots"). Every Expect must hold.
- **Rewrite the steps that change.** A page that replaces a placeholder rewrites Steps 3.31, 3.52, 3.53, A.5.10–A.5.12
  and A.7.8, whose Expects read "Coming soon". Add steps for any new action, such as triage. T294's guard fails on a
  page with no step.

**Checks the design adds.**
- Step 3.30: both stalled requests are listed, oldest first, and each opens its activity (T297).
- Step 3.53: the count reads "1 activity awaiting review" (or its post-T297 number), and its link, if any, lists what it
  counts (T297).
- Step 2.33: tied registrars read in surname order (T298).
- Step 2.37: Dr van Rensburg sees the five registrars (T290).
- Steps A.7.5–A.7.8 at 390 px:
  - no page scrolls sideways;
  - badges keep their pill (T328);
  - a focused scroll region shows the design's ring (T328).

**Re-capture.** Take the 15 state captures in § 4 and this flow's step captures again (`states.md` § How to capture).
Compare them with the chosen artboards.

**Browser check.** Check each of the four roles at 1280 px and at 390 px.
