# F01 The frame everyone shares: navigation, landing, role switching and the system states (the pilot)

**Who and why:** every role and every signed-out visitor meets this frame on every page. It settles the navigation, what
each role lands on, the role switch, the failure screens, and the tokens that F02–F20 sit inside.

**This flow is the pilot** (T335). It is taken from Claude Design through Razor to a green replay before any other flow
is briefed, and what it teaches corrects the brief (BRIEF.md § 2.3, § 11). The step-by-step is
`design/pilot/README.md`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4). The frame may change in structure, not only in look. Today's sidebar, top row, Home-only switch line and flat lists are what exists, not what must stay. |
| Mode | **Structure first.** Round 1: 2–3 structural variations as wireframes, each with its reasoning and its click counts. Round 2, after the pick: the chosen frame at fidelity with all its states, ending with the token sheet that every later flow uses. |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | 2–3 in round 1; the chosen one in round 2 |
| People | All 10 roles (`src/Wombat.Domain/Identity/WombatRoles.cs`), a former trainee with no role, and signed-out visitors |
| Runbook steps | 21, pasted verbatim in § 8 |
| Pages (`coverage.md` templates) | `/`, `/access-denied`, `/not-found`, `/Error` (and `/placeholder/{Feature}`, until T335 deleted it with the placeholder page); the endpoint `/dashboard/switch/{role}`; `Components/Layout/MainLayout.razor`, `NavMenu.razor`, `ReconnectModal.razor`; `#blazor-error-ui` (the error bar, in `MainLayout.razor`); `Components/Pages/Home.razor` and the frame of the nine `Components/Pages/Dashboards/*.razor` |
| Held | Nothing. T297's images were re-captured on 2026-09-26 (§ 4) |

**How to run this thread** (`design/pilot/README.md` steps B–E):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 01`.
   - Open each screenshot.
   - The one file in `design/upload/crop-first/` shows a registration link. Crop it first, or leave it out.
2. **Start the canvas** with the design system chosen in pilot step A (pilot step C). Paste § 1, and attach the key
   screenshots in § 4 with it.
3. **Paste § 8,** the runbook steps, as the next message.
4. **Review round 1's variations** against the checklist in pilot step D. Attach § 4's landing and state captures as
   the chat asks for them.
5. **Pick one,** and say why in a sentence (pilot step E). Then ask for round 2.
6. **Answer § 6's questions** in the chat, one sentence per decision.
7. **Export the chosen artboards** into `design/flows/01-shell/` before moving on.

---

## 1. The ask (paste this first)

> **The ask as sent (round 1).** § 1–§ 3 are what the canvas was given, and describe the product before the build
> ("Coming soon", "Back to home", a request id on a typed /Error). § 8 and the runbook hold what was built (T335).

```text
FLOW 01 — Know where I am, what I may do and which role I am acting in, reach my work from where I land, and carry on
  when something fails

This is the pilot of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). The structure is open. Structure first,
  then fidelity: round 1 is wireframes only, and I pick one before round 2.

GOAL: Wherever a person is in Wombat, they know where they are, what they may do, and which role they are acting in,
  and they can switch role. From where they land, their most frequent job is a click or two away. When something
  fails, is refused, is not found or is not built, they are told plainly, where they are looking, and can go on. This
  flow designs the frame every other flow sits in: the navigation, the landing per role, the role switch, where
  outcomes and failures show, and the token set every later flow uses.
  What is wrong today (the attached screenshots show each):
  - The nav is a flat list of up to 20 links (the Administrator's), and five of them open "Coming soon" stubs.
  - Every role lands on the same Home, which shows one role's dashboard at a time. A two-role person switches it from
    a line on Home only, and the switch changes the dashboard but not the nav.
  - Some dashboards say what exists rather than what needs doing. The InstitutionalAdmin's counts users per role by
    their codes. Its "Quick links" repeat three nav links, and add Entrustment decisions, which her nav does not
    offer.
  - Sign out appears twice: the top row's button and the nav's Logout.
  - The nav lights no item on some pages, and its items render underlined.
  - Access denied, reached by an in-app link, renders inside a second copy of the layout: two sidebars, two top rows,
    two Sign out buttons.
  - The reconnect dialog shows two messages at once: "Rejoining the server..." above "Rejoin failed... trying again in
    5 seconds."
  - Alert, badge, validation and focus colours fail WCAG AA.
  - Home greets by email ("Welcome, zulu@kgk.wombat.local") and names the role by its code ("Viewing as
    CommitteeMember").

AUDIENCE: every role, and signed-out visitors; desktop 1280×800 and phone 390×844.
  Roles (10): Administrator, CollegeAdmin, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, Coordinator,
  CommitteeMember, Assessor, Trainee, PendingTrainee. A former trainee holds no role and keeps a read-only record.
  Personas for the frame:
  - devadmin, the platform operator (Administrator): the longest nav.
  - Prof Nolwazi Mbatha (InstitutionalAdmin): 18 links.
  - Dr Mohammed Patel (Assessor): a short nav.
  - Dr Thandi Zulu, the committee chair, holds CommitteeMember and Assessor and switches between them; so do
    Dr David Naidoo and Dr Sarah Botha.
  - Dr Anele Dlamini (Trainee), often on her phone.

NAV CONTENT PER ROLE (today's order; * = a "Coming soon" stub today):
  Signed out: Home, Sign in
  Everyone signed in: Home, My Account, Data Rights, <the role's links>, Logout
  Trainee: Activities, My Activities, MSF Reports, My Committee Reviews, My Progress, Export Portfolio
  PendingTrainee: Activities, My Activities
  Assessor: Activity Inbox, Recent Activities*
  Coordinator: Data Rights Requests, MSF Campaigns, Committee Reviews, Decisions Due, Stalled Activities*
  CommitteeMember: Programme Trainees*, Decision Panels, Committee Reviews
  SpecialityAdmin, SubSpecialityAdmin: Programme Trainees*, Decision Panels, Committee Reviews, STAR Review Queue*,
    Decisions Due
  CollegeAdmin: Specialities, EPAs, Curricula, Activity Types (T300)
  InstitutionalAdmin: Curriculum Adoptions, EPAs, Curricula, Activity Types, Entrustment Scales, Trainees, Assessors,
    Invitations, Users, SSO Mappings, Audit Log, Decision Panels, Committee Reviews, Decisions Due
  Administrator: Colleges, EPAs, Curricula, Institutions, Invitations, Users, Activity Types, Entrustment Scales,
    Scheduled Jobs, SSO Mappings, Audit Log, Decision Panels, Committee Reviews, Decisions Due, Data Rights Requests,
    System*
  Former trainee (no role): My Progress
  Someone with several roles sees the union, each page once. Every link must open a page that admits the role, and no
  two links may share a label; both rules are tested and survive any regrouping.
  Nav lengths to design for: Assessor 6 links, CommitteeMember + Assessor 9, Trainee 10, InstitutionalAdmin 18,
  Administrator 20 (counting Home, My Account, Data Rights and Logout).

LANDING TODAY (every role lands on Home, /, which shows one dashboard; its cards' contents belong to later flows):
  Administrator: System health, Users across institutions, Maintenance
  CollegeAdmin: National catalogue
  InstitutionalAdmin: Users (a count per role), Specialities & sub-specialities, Quick links
  SpecialityAdmin, SubSpecialityAdmin: Pending reviews, Trainees in programme, Curriculum coverage
  Coordinator: Stalled requests, Invitations nearing expiry, Quick action
  CommitteeMember: Targets this period, Targets met by EPA
  Assessor: Pending requests, Awaiting your review, Recent decisions, Actions
  Trainee: Curriculum targets, Activity inbox, Recent activities, Upcoming deadlines, My authorisations, Actions
  PendingTrainee: Awaiting admission, with "Review your account"
  Former trainee: "No role assigned"
  Someone with several roles lands on the first of theirs in this order: Administrator, CollegeAdmin,
  InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin, CommitteeMember, Coordinator, Assessor, Trainee,
  PendingTrainee. A remembered switch overrides it.

WHAT EACH ROLE COMES TO DO MOST (the frame must get them there from where they land):
  Trainee: file an observation (25 per semester) and follow it; read her progress.
  Assessor: rate what waits in the inbox.
  Coordinator: chase stalled work; run MSF campaigns; schedule reviews from what is due.
  CommitteeMember: sit a review; see who is behind this period.
  SpecialityAdmin, SubSpecialityAdmin: see who is behind; schedule reviews from what is due.
  InstitutionalAdmin: invite, admit and manage people; adopt the curriculum.
  CollegeAdmin: keep the national catalogue.
  Administrator: keep the platform (jobs, institutions, audit).
  Former trainee: read her record and export her portfolio.

ROUND 1 — STRUCTURAL VARIATIONS (wireframes: greyscale boxes and real labels, no colour or type choices yet).
  Give 2–3, each a different navigation model, and show for each:
  a. The navigation model: what holds the navigation (a sidebar, a top bar, a rail, a hub page or something else), how
     links are grouped, and each role's navigation written out. At least the Administrator's (20 links today), the
     InstitutionalAdmin's (18), the Trainee's (10) and the Assessor's (6).
  b. A person with several roles: Dr Zulu (CommitteeMember + Assessor). Her navigation, and how she sees which role she
     is acting in: on every page, or on Home only.
  c. The top bar: identity by name, one Sign out, and anything else it carries.
  d. Role switching: where it lives, and what changes when it happens (the landing only, or the navigation too).
  e. Landing per role: what each role sees first, and the clicks from landing to its most frequent job above, beside
     today's count. Today a Trainee is one click from a new observation ("Log an activity" on Home, or Activities in
     the nav), then chooses the type; an Assessor is two from a waiting request (Activity Inbox, then the row).
  f. Where outcomes and system states show: an action's result and a refusal, in view of the control that caused it;
     loading and load error; access denied; not found; the error page; the reconnect dialog; the in-app error bar.
  g. At 390 px: the navigation folded and open. The toggle is CSS-only.
  h. The reasoning: what the variation is best at, what it costs, which roles it serves worst, and which DESIGN.md rule
     and test it changes.
  Stop after round 1 and wait for my pick.

ROUND 2 — AFTER I PICK: SCREENS at full fidelity, in order:
  1. The shell at 1280 px: the navigation, the top bar and the content column, with the Administrator's navigation (the
     longest) and with the Assessor's (a short one).
  2. The shell at 390 px: the navigation folded, and open.
  3. The landing frame: the page header, the acting role and the way to switch it for a two-role user ("Viewing as
     Committee member · Switch to Assessor", or its replacement), and the grid every role's landing sits in. Show
     placeholder cards spanning one, two and three columns; the cards themselves belong to later flows.
  4. The failure pages: Access denied (signed in, signed out), Page not found (signed in, signed out), and the error
     page with a request id to quote (signed in, signed out).
  5. The "Coming soon" stub, only if any placeholder survives question 2 below.
  6. The framework overlays: the reconnect dialog in its five states (rejoining, retrying, failed, paused, resume
     failed); the in-circuit error bar ("An unhandled error has occurred. Reload"); and the shared loading (skeleton)
     and load-error (alert) states that every list and detail page uses.
  7. The token sheet: the palette with a contrast figure for each text/background and border/background pair; the type
     scale; spacing; radii; the focus ring on the page and on the navigation; badges in five tints; alerts in four
     kinds (success, info, warning, danger); buttons (primary, outline, danger, success, small).

STEPS: 2.33, 2.34, 3.33, 3.52, 3.31, A.5.3, A.5.1, A.5.2, A.6.3, A.5.4, A.5.5, A.5.6, A.5.7, A.5.8, A.5.9, A.5.10,
  A.5.11, A.5.12, A.5.13, A.7.3, A.7.14. Pasted verbatim in the next message (Role / Route / Do / Expect). Where an
  Expect quotes today's wording, that is today's page, not a requirement on the new one.

STATES TO SHOW:
  Shell: nav folded and nav open at 390 px; reconnect rejoining, retrying, failed, paused, resume failed; the in-app
    error bar.
  Landing frame: a two-role user; the same user after switching; loading (skeleton); load error (alert, with no empty
    state under it).
  Access denied: signed in, signed out, narrow. Page not found: signed in, signed out, narrow.
  Error: signed in with its request id, signed out (today it redirects to sign-in: design the page instead), narrow.
  Coming soon: each surviving placeholder; an unknown /placeholder/ address (Page not found, 404); narrow.
  Data volumes: the nav at 6, 9, 10, 18 and 20 links; a long name in the top row; a role label that wraps.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T322: contrast is fixed in the tokens. Text reaches 4.5:1; control boundaries and the focus ring reach 3:1
    (WCAG 1.4.3, 1.4.11). These fail today: success text on its tint 2.55, warning 2.42, danger 3.57; validation text
    on white 3.82; white on the danger button 3.82 and on the success button 2.87; the focus ring on the page 2.99;
    the input border 1.49. The navigation needs its own focus-ring token that reaches 3:1 on its background (both
    ends of today's gradient).
  - T321: an unhandled failure shows a designed error page with a request id, signed in or out. Today it is a bare 500;
    /Error loses its id once the page goes interactive; signed out, it redirects to sign-in. Access denied is drawn
    once, never inside a second layout.
  - T330: the reconnect dialog shows one message per state, and its text and its button agree (today resume-failed
    says "retry" above a button labelled "Resume").
  - T331: the nav marks where you are, and never two items at once. Today a CollegeAdmin's Specialities list and the
    Coordinator's MSF report and MSF coverage light nothing.
  - T328: form controls use the body font; a focusable scroll region shows the design's focus ring; badges keep their
    pill shape in a flex row; header actions share one height; the top row's account link is at least 24 px tall.
  - T329: a page's h1 and header render from the first render; a skeleton shows while its first read runs; an alert
    shows if the read fails, with no empty state beneath it; no action is offered on a record before it has loaded.
  - T325: every printed time is South African time with its zone ("2026-09-26 15:14 SAST").
  - T324: people by name, states and types by label, roles by label ("Committee member", not "CommitteeMember"). Ids
    stay only in the audit log and an audit entry, the data-rights list and a request, and /portfolio/verify.
  - T190: page titles follow one pattern.
  - T317: the remembered dashboard role does not outlive sign-out.
  - T311: a former trainee lands on something other than "No role assigned" (her landing's cards are flow 13's; her
    frame and navigation are this flow's).
  - Observed, not filed: nav items render underlined; the navigation's gradient stops short of a full-page capture's
    height; Sign out appears twice; Home greets by email, not name.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Grouped navigation, or a flat list, or another model? The Administrator sees 20 links and the InstitutionalAdmin
     18. Whatever the model, every link opens a page that admits the role, and no two links share a label.
  2. For each "Coming soon" item (Recent Activities, Stalled Activities, Programme Trainees, STAR Review Queue,
     System): will a later flow design it, or is the nav item dropped? The shell shows the outcome.
  3. Where does the role switch live: on Home only (today), or in the shell, so it is visible on every page? Does a
     switch change the navigation as well as the landing?
  4. One Sign out: where?
  5. Does an activity page, reached from several lists (inbox, My Activities, a dashboard, a review), light a nav item?
  6. Should the error page and the sign-out confirmation page be reachable by any link? Today they are reached only by
     typing their address.
  7. Body typeface: keep the Segoe UI stack (only Windows has Segoe UI), or name a self-hosted GPLv3-compatible face?
     Dark mode and reduced motion: in or out? Neither exists today.
  8. What does each role land on: one Home whose content depends on the role (today), or a landing per role, such as
     the Assessor's inbox or the Trainee's progress? What does a person with several roles land on?
  9. Where does an action's outcome show, and where a refusal, so that it is in view of the control that caused it?
     This is the frame's rule that every later flow follows.

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
  - Round 1: 2–3 structural variations as wireframes, each showing a–h above, with its reasoning and its click
    counts. No colour or type choices yet. Stop and wait for my pick.
  - Round 2: the chosen frame at full fidelity, every screen and state above, then the token sheet.
  - Name every design-system component you use, and mark anything else NEW.
  - Say which DESIGN.md rule each variation changes. Today the shell's 250 px sidebar, sticky top row, 641 px
    breakpoint and 16 px gutter are rules, and so are the nav's order, grouping and labels. All of these may change
    (W-008). Two rules may not: every nav link opens a page that admits the role, and no two links share a label
    (BRIEF.md § 4.4).
  - Flag edge cases: a 20-link nav on an 800 px-tall screen, a long name or email in the top bar, a role label that
    wraps, a two-role user whose second role has a long name.
  - Run an accessibility review against WCAG 2.1 AA, and give a contrast figure for every token pair on the sheet.

ATTACHED: states/shell--nav-open.png, states/shell--nav-folded.png, states/home--committee-member.png,
  states/access-denied--signed-in.png, states/error--signed-in.png, states/shell--reconnect-retrying.png,
  states/shell--error-banner.png, states/home--loading.png, act-A/A.7.14-3-success-alert.png,
  act-A/A.7.14-7-review-7-badges.png,
  act-1/1.5-2-kruger-invited.png (cropped: it is here only for the underlined nav), states/home--administrator.png
  (the 20-link nav). For round 1, the landing captures follow as you ask for them.
```

---

## 2. The journey

> **The ask as sent (round 1);** § 8 and the runbook hold what was built.

Each row is one runbook step, in the order above.

| Step | Page | The person does | They must be able to see |
|---|---|---|---|
| 2.33 | `/account/login` → `/` | Dr Zulu (CommitteeMember + Assessor) signs in and reads Home and the nav | which role she is viewing as; that she also acts as Assessor, with a way to switch; her nav, each link once |
| 2.34 | `/` → `/dashboard/switch/{role}` → `/` | switches to Assessor, signs out and in, then types a switch to Administrator | the view she is in after each move; the remembered choice after signing in again; a role she does not hold is never shown; the nav does not change with the view |
| 3.33 | `/` → `/dashboard/switch/{role}` → `/activities/inbox` | switches to Assessor to clear a stalled request | the switch line, then the Assessor view (its cards are F04's) |
| 3.52 | `/dashboard/switch/{role}` → `/` → `/placeholder/{Feature}` | switches back, then opens Programme Trainees | the committee view; Programme Trainees is "Coming soon" today (question 2) |
| 3.31 | `/placeholder/{Feature}` | Mr Smit (Coordinator) opens Stalled Activities, then mistypes `/placeholder/stalled-work` | "Coming soon" for the real item; Page not found (404) for the mistyped one |
| A.5.3 | `/dashboard/switch/{role}` → `/` | Dr Dlamini (Trainee) types `/dashboard/switch/Administrator` | her own Trainee view; no other view is offered |
| A.5.1 | `/admin/users`, `/admin/jobs` → `/access-denied` | a trainee types two administrators' addresses | Access denied: why, and Back to home; nothing of the page she asked for |
| A.5.2 | `/admin/institutions`, `/admin/jobs` → `/access-denied` | Prof Mbatha (InstitutionalAdmin) types the Administrator's pages | Access denied for both; her nav offers neither |
| A.6.3 | `/admin/institutions/{Id:int}` → `/access-denied` | Prof Mbatha presses Back to institutions on her own institution | Access denied, drawn once (today inside a second layout: T321) |
| A.5.4 | `/not-found`, `/placeholder/{Feature}`, `/account/login` | types unknown addresses, signed in and signed out | Page not found, the address kept as typed, Back to home; signed out, sign in first, then Page not found |
| A.5.5 | `/admin/institutions/{Id:int}` → `/not-found`; `/admin/users/{UserId}` | Prof Mbatha opens another institution's records by id | Not found, never Access denied (CLAUDE.md: 404, not 403) |
| A.5.6 | `/activities/{ActivityId:int}` | a trainee opens another registrar's activity | "Activity unavailable", and nothing about permission |
| A.5.7 | `/admin/data-rights/{Id:guid}` | Mr Smit opens a request that is not his | the refusal and a "Not found" empty state, the same for an unknown id |
| A.5.8 | `/Error` | types `/Error` | "Something went wrong" and a request id to quote; signed out, the same page (T321), not a sign-in form |
| A.5.9–A.5.13 | `/placeholder/{Feature}` | the Assessor, Coordinator, CommitteeMember, SpecialityAdmin and Administrator each open their "Coming soon" item | today the stub under its own heading; after question 2, a real page's frame or no nav item |
| A.7.3 | `/`, `/portfolio/progress`, `/activities/mine`, `/account/data-rights` | Dr Dlamini, at 390 px, moves through the folded nav | the nav folds and opens; cards stack; tables scroll inside their containers, with each row's action reachable |
| A.7.14 | sign-in, Home, My Account, Change password, a user, the builder, Committee Reviews, a review | Prof Mbatha checks contrast with axe | every pair meets WCAG 2.1 AA; the token sheet gives the figures |

## 3. States to design

> **The ask as sent (round 1);** § 8 and the runbook hold what was built.

Each screenshot is under `design/baseline/`. How to reach each is in
`execution/knowledge/scenario-paediatrics/states.md` (§ Shell and framework, § Home and the role dashboards, § System
pages).

| Page | State | Screenshot | What it shows today |
|---|---|---|---|
| Shell | Nav folded, 390 px | `states/shell--nav-folded.png` | the brand and a menu toggle in a top bar |
| Shell | Nav open, 390 px | `states/shell--nav-open.png` | the Trainee's 10 links pushing the page down |
| Shell | Reconnect: rejoining | `states/shell--reconnect-rejoining.png` | "Rejoining the server..." |
| Shell | Reconnect: retrying | `states/shell--reconnect-retrying.png` | two messages at once (T330) |
| Shell | Reconnect: failed | `states/shell--reconnect-failed.png` | "Failed to rejoin. Please retry or reload the page." with Retry |
| Shell | Reconnect: paused | `states/shell--reconnect-paused.png` | "The session has been paused by the server." with Resume |
| Shell | Reconnect: resume failed | `states/shell--reconnect-resume-failed.png` | "retry" above a Resume button (T330) |
| Shell | In-app error bar | `states/shell--error-banner.png` | "An unhandled error has occurred. Reload" in `lightyellow`, over New activity left with an empty type picker (T329) |
| Home | Two-role user | `states/home--committee-member.png` | "Welcome, zulu@…", "Viewing as CommitteeMember", "You also act as Assessor. Switch view: Assessor" |
| Home | After a switch | `states/home--assessor-switched.png` | "Viewing as Assessor", the CommitteeMember switch line, Pending requests 0, "Nothing is awaiting your review." (re-captured after T297, 2026-09-26) |
| Home | Loading | `states/home--loading.png` | the skeleton |
| Home | Load error | `states/home--load-error.png` | the danger alert with the raw timeout text |
| Access denied | Signed in | `states/access-denied--signed-in.png` | heading, reason, warning alert, Back to home |
| Access denied | Signed out | `states/access-denied--signed-out.png` | the static page, with Sign in in the top row |
| Access denied | Narrow | `states/access-denied--narrow.png` | |
| Page not found | Signed in | `states/not-found--signed-in.png` | |
| Page not found | Signed out | `states/not-found--signed-out.png` | |
| Page not found | Narrow | `states/not-found--narrow.png` | |
| Error | Signed in | `states/error--signed-in.png` | "Something went wrong", ending with no request id once interactive (T321) |
| Error | Narrow | `states/error--narrow.png` | |
| Coming soon | Recent Activities, Stalled Activities, Programme Trainees, STAR Review Queue, System | none | T335 deleted the placeholder page, so each address is Page not found now. Flow 04 designs Recent Activities, flow 06 the next three, and flow 18 System |
| Coming soon | Any `/placeholder/` address | `act-3/3.31-1-stalled-activities-404.png` | Page not found, 404 |

Not captured, so describe them in words:
- **The error page signed out.** It redirects to sign-in today (`act-A/A.5.8-2-error-signed-out.png` shows the
  redirect). T321 wants the designed page for both.
- **The error page through a real failure.** On dev the developer exception page answers a failure; outside
  Development the error page does (`ErrorPages`, T321; `states.md` § System pages, "After a failure").
- **The reconnect dialog's "rejected" state.** It reloads the page at once (the same section).
- **Session ended.** It is the sign-in page's state and belongs to F02 (`states.md` § Shell and framework).
- **A former trainee's frame.** Her Home today is "No role assigned" (`states/home--no-role.png`, T311). Her dashboard
  is F13's (BRIEF.md § 7 B1), but her nav (My Progress only) is this flow's.

## 4. Attach

Paths are relative to `design/baseline/`. Every one below was checked with `ls` on 2026-09-26, and the landing captures
by `design/tools/check_baseline_paths.py` on 2026-09-27. `pwsh design/tools/stage_upload.ps1 -Flow 01` stages them
all, keeping `act-1/1.5-2-kruger-invited.png` back in `design/upload/crop-first/` (BRIEF.md § 3.1).

**Key screenshots (attach these first, with § 1):**
1. `states/shell--nav-open.png`
2. `states/shell--nav-folded.png`
3. `states/home--committee-member.png`
4. `states/access-denied--signed-in.png`
5. `states/error--signed-in.png`
6. `states/shell--reconnect-retrying.png`
7. `states/shell--error-banner.png`
8. `states/home--loading.png`
9. `act-A/A.7.14-3-success-alert.png`
10. `act-A/A.7.14-7-review-7-badges.png`
11. `act-1/1.5-2-kruger-invited.png`. It is here only to show the underlined nav. **Crop out its registration link
    first** (BRIEF.md § 3.3), or leave it out.
12. `states/home--administrator.png`: the Administrator's 20-link nav (optional; Administrator homes are not held).

Access denied reached by an in-app link has no capture now: Back to institutions, the link that reached it, went with
T302, and this flow designs Access denied. Nor do the "Coming soon" stubs: T335 deleted the placeholder page, so
each address is Page not found now, and flows 04, 06 and 18 design the pages (§ 3).

**Landing per role (for round 1; attach as the chat asks).** Each is today's Home for one role:
- `states/home--trainee.png` (Trainee), `states/home--assessor-pending.png` (Assessor, with a request waiting)
- `states/home--coordinator-stalled.png` (Coordinator), `states/home--committee-member-figures.png` (CommitteeMember)
- `states/home--speciality-admin-figures.png` (SpecialityAdmin), `states/home--college-admin.png` (CollegeAdmin)
- `states/home--institutional-admin.png` (InstitutionalAdmin: users counted per role; Quick links that repeat three nav
  links and add one the nav lacks)
- `states/home--awaiting-admission.png` (PendingTrainee), `states/home--no-role.png` (former trainee, T311)
- `states/logout-confirm--default.png` (the sign-out confirmation, reached only by address; question 6)

**States (attach as the chat asks):**
- `states/shell--reconnect-rejoining.png`, `states/shell--reconnect-failed.png`, `states/shell--reconnect-paused.png`,
  `states/shell--reconnect-resume-failed.png`
- `states/home--load-error.png`
- `states/access-denied--signed-out.png`, `states/access-denied--narrow.png`
- `states/not-found--signed-in.png`, `states/not-found--signed-out.png`, `states/not-found--narrow.png`
- `states/error--narrow.png`
- `act-3/3.31-1-stalled-activities-404.png` (a `/placeholder/` address, Page not found since T335 deleted the page)
- `act-A/A.5.8-2-error-signed-out.png` (the signed-out redirect)

**Re-captured after T297 landed (2026-09-26), and re-taken on 2026-09-27 by the T335 replay, the step captures at
their own moments (BRIEF § 10); attach as the chat asks:**
- `states/home--assessor-switched.png` and `act-2/2.34-1-zulu-assessor-view.png` (Step 2.34: the empty Assessor view
  after the switch)
- `act-A/A.5.3-1-forged-switch-trainee-view.png` (Step A.5.3: still "Acting as Trainee")

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists.

| Task | What it means for the design | Evidence (under `design/baseline/`) |
|---|---|---|
| **T322** (P2, High) | The token sheet is where contrast is fixed, not each page. Every alert tint, badge tint, validation colour, button fill, input border and focus ring on the sheet carries its measured ratio: text 4.5:1, boundaries and focus 3:1. The navigation gets a focus-ring token of its own that reaches 3:1 on both ends of its gradient. Today's failing pairs are in § 1. | `act-A/A.7.14-3-success-alert.png`, `act-A/A.7.14-7-review-7-badges.png`, re-taken after T335's tokens landed T322 (every pair now passes); the failing measurements were step A.7.14's before T335 (BRIEF.md § 6, A1) |
| **T321** | Design the error page for signed-in and signed-out visitors, with the request id as quotable text. Access denied is one page with one frame, whether it is reached by a typed address or an in-app link. | `states/error--signed-in.png`, `act-A/A.5.8-2-error-signed-out.png`; Access denied reached by an in-app link has no capture now (Back to institutions, the link that reached it, went with T302) |
| **T330** | One message per reconnect state, and a button whose label matches the message. Keep the `components-reconnect-*` class names (`Layout/ReconnectModal.razor.css`). | `states/shell--reconnect-retrying.png`, `states/shell--reconnect-resume-failed.png` |
| **T331** | The active item is designed, and a rule says which item a sub-page lights. Never two at once. | `states/specialities-list--loading.png`, `states/campaign-report--loading.png` (both F16/F10 pages, shown here for the unlit nav) |
| **T328** | Controls in the body font; the design's focus ring on a focusable scroll region; badges that stay pills in a flex row; header actions of one height; a top-row account link at least 24 px tall. | `states/home--narrow-trainee.png` (the stretched badge; re-captured after T297, and still stretched), `act-A/A.7.5-4-decisions-due-390.png` (re-taken after T335: the design's ring) |
| **T329** | The shared loading and load-error states: the header from the first render, a skeleton while the first read runs, an alert (not an alert above an empty state) when it fails, and no action before the record loads. The in-app error bar must not be the way a failed read ends. | `states/home--loading.png`, `states/home--load-error.png`, `states/shell--error-banner.png` |
| **T325** | Wherever the frame prints a time, it is South African time with its zone. | BRIEF.md § 6, A9 |
| **T324** | Home greets by name, and the switch line names roles by label ("Committee member"). | `states/home--committee-member.png` |
| **T190** | One page-title pattern for every tab. | T190's symptom: "Dashboard — Wombat", "Sign in - Wombat", and most pages with no suffix |
| **T317** | The switch choice is remembered per person, and does not carry over to the next person who signs in on the same browser. | `act-2/2.34-3-naidoo-same-browser.png` (built in T335: Dr Naidoo, next on the same browser, lands as Committee member) |
| Observed, not filed | Nav items render underlined (`NavMenu.razor.css` sets `text-decoration: none` on the brand, line 38, but not on `.nav-link`). The navigation's gradient stops short of a full-page capture's height. Sign out appears twice (`MainLayout.razor:13–16` and the nav's Logout form). Home greets by email. | `act-1/1.5-2-kruger-invited.png`, `states/home--committee-member.png` |

## 6. Questions the design must answer

1. **Grouped navigation, a flat list, or another model?** The Administrator sees 20 links, the InstitutionalAdmin 18
   (DESIGN.md § The NavMenu, its table). Whatever the model, every link must open a page that admits the role, and no
   two links may share a label (`NavMenuAuthorizationTests` parses DESIGN.md's nav table, so a regrouping is a
   DESIGN.md change).
2. **The five "Coming soon" items** (`NavMenu.razor:108, 113, 114, 116, 132`; BRIEF.md § 7 B7). For each, either a
   later flow designs it or the nav item goes: Recent Activities (F04); Stalled Activities, Programme Trainees and STAR
   Review Queue (F06); System (F18). The shell shows the outcome. Their contents are inferred: no intent document
   exists.
3. **Where does the role switch live?** On Home only, as today, or in the shell, so the role is visible on every page?
   Does a switch change the navigation as well as the landing? Today it changes only the dashboard (Step 2.34).
4. **One Sign out, where?** The nav's Logout and the top row's Sign out duplicate each other. Both are form posts
   (DESIGN.md § The NavMenu), and either survives the choice.
5. **Does `/activities/{id}` light a nav item?** It is reached from the inbox, My Activities, the dashboards and a
   review. T331 leaves it unlit unless a rule is chosen.
6. **Should `/Error` and `/account/logout-confirm` be reachable by a link?** Today only a typed address reaches either
   (`coverage.md` § Reached only by address).
7. **Typeface, dark mode, reduced motion.** The body stack starts with Segoe UI (`app.css:50`), which only Windows has.
   Keep it, or name a self-hosted, GPLv3-compatible woff2 for `wwwroot/fonts`. Fraunces stays the wordmark's face
   (`--font-display`). Dark mode and reduced motion are both absent from `app.css` today: in or out?
8. **What does each role land on?** Today it is one Home whose dashboard depends on the role, chosen by
   `Navigation/DashboardPriority.cs` for a person with several. The alternative is a landing per role, such as the
   Assessor's inbox or the Trainee's progress. Either way, the most frequent job (§ 1) is stated in clicks from landing,
   beside today's count.
9. **Where does an outcome show, and where a refusal?** Today a result region takes the focus
   (`Accessibility/ActionFocusTests`). But a refusal can land at the head of a long page, out of view of the card that
   caused it (`act-4/4.46-1-zulu-remit-quorum-refused.png`, flow 09). The frame sets the rule that every later flow
   follows.

## 7. Acceptance

A flow is done when BRIEF.md § 9's checks hold. For this flow, the pilot, `design/pilot/README.md` steps F and G give
the commands.

- **`DESIGN.md` is amended in the same task** (W-008; BRIEF.md § 9 item 6). The sections rewritten are the ones the
  chosen frame changes, at least:
  - § Design tokens and § Typography (the token sheet);
  - § Layout grid (the shell) and § The NavMenu (its role table, which `NavMenuAuthorizationTests` parses);
  - § Dashboard layout grid and § Page-level patterns › Dashboard page (the landing frame);
  - § Alerts, validation, empty states, and § Accessibility (outcomes, refusals and the focus ring).

  Flow 01 is then added to the banner's "Redesigned so far" list, with its date and commit.
- **Replay on a fresh database, from Act 1.** Flow 01's steps run from Act 2 to the appendix, so the replay is the whole
  runbook in order, through A.7.14. The 21 steps are the acceptance: 2.33, 2.34, 3.31, 3.33, 3.52, then the appendix's
  A.5.1–A.5.13, A.6.3, A.7.3 and A.7.14. Every Expect must hold.
- **Every step that quotes the frame is checked.** A keyword search finds 59 steps whose Role, Route, Do or Expect lines
  mention the nav, the role switch, Sign out, a menu or "Coming soon" (BRIEF.md § 4.5); 15 of the 21 above are among
  them, and the other six (A.5.1, A.5.3, A.5.5–A.5.8) are checked as this flow's own. If the design changes wording a
  step quotes, update that Expect in the same task (BRIEF.md § 9 item 7). For example:
  - Steps 1.1 and 2.33 list the nav;
  - Steps 2.33, 2.34, 3.33 and 3.52 quote the switch, now the sidebar's role head;
  - Steps A.5.9–A.5.13 check that the menu offers nothing unbuilt.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`). They
  must include:
  - `Navigation/NavMenuAuthorizationTests`, which covers the nav table per acting role, the personal links, and that no
    placeholder page or link exists (`NoPlaceholderPage_Exists`);
  - `Navigation/DashboardLinkAuthorizationTests` and `Dashboards/*`;
  - `Design/NarrowLayoutTests` (`TheShell_HasASideGutter_AtPhoneWidth`), `DesignSystemSmokeTests`,
    `PageShapeSmokeTests`, `DefinedClassTests`, `AlertRoleTests`, `BadgeForStatusTableTests`, `InvalidFieldStyleTests`
    and `FieldGroupTests`;
  - T322's planned `Design/ContrastTests`, added in the same task;
  - `Accessibility/*`, which holds the invariants in BRIEF.md § 4.4;
  - `Hosting/AppHeadTests` (the page title, T190), and `Hosting/AppAssetUrlTests` if a font or script is added;
  - `Scenario/`.
- **Re-capture** the rows of states.md § Shell and framework, § Home and the role dashboards, and § System pages, the
  landing captures in § 4, and the steps' own captures into `design/baseline/`. Compare them with the chosen artboards.
- **Browser check** at 1280 and at 390 px for every row of DESIGN.md's nav table, and for a two-role user. Check the
  reconnect dialog by suspending the app (`states.md` § Shell and framework).

## 8. The runbook steps, verbatim (paste this second)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as it is today. Steps 2.34 and 3.33 describe the Assessor dashboard's card as T335 left it,
"Waiting for your rating"; F04 designs the cards.

```text
Step 2.33 — Dr Zulu's first view: committee member (act-2-onboarding.md:578)
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: /account/login → /
Do: Sign in and read the dashboard and nav.
Expect: Home reads "Committee member · Semester N, YYYY" under its heading. The sidebar reads "Acting as Committee
  member", with "Switch to Assessor" under it: she holds both, and the precedence opens her sessions as a Committee
  member. Targets this period names the current semester and its months. It lists the five registrars, each at "semester
  0/10 · yearly 0/5", and nobody is exempt: a 15 January start counts from the boundary (D42). Targets met by EPA lists
  PAED-001 to PAED-015, each "0 of 5 met" (T130's count, never a percentage). The menu is the Committee member's alone,
  never the union of her roles: Home, Committee reviews and Decision panels, then My data rights.

Step 2.34 — Dr Zulu switches between her dashboards (act-2-onboarding.md:595)
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /account/logout/submit → /account/login → / → /dashboard/switch/{role} → / → /dashboard/switch/{role} → /
Do: Choose Switch to Assessor in the sidebar, and reload the page. Sign out and in again. Then type
  `/dashboard/switch/Administrator`. Last, choose Switch to Committee member in the sidebar.
Expect: Under the header an info alert reads "You are now acting as Assessor." and takes the focus; on Home it offers no
  Switch back. The sidebar reads "Acting as Assessor", with "Switch to Committee member" under it, and the menu changes
  to the Assessor's: Home and Activity inbox, then My data rights. Home's subtitle reads "Assessor · Semester N, YYYY".
  The Assessor dashboard has two cards: "Waiting for you", with no badge, which reads "Nothing is waiting for you." with
  "Open Activity inbox" (T297, T335, T350), and Recent decisions, "No decisions yet.", with no "All your decisions".
  Nothing waits in her inbox, so the Committee member's Home shows no line about it (T350). The reload shows no alert:
  it is said once. Signing out lands on the sign-in page with "You have signed out." After signing in again she lands
  acting as Assessor, with no alert, because the choice is stored with her account, not in the browser (T317). The
  Administrator address writes nothing and says nothing: she is still acting as Assessor, since a role she does not hold
  is never shown or stored. Switch to Committee member brings back the Committee member's sidebar and menu, "Committee
  member · Semester N, YYYY" under Home's heading, and the alert "You are now acting as Committee member.", the
  role Step 3.33 starts from.

Step 3.33 — Dr Zulu switches to her assessor view and completes the stalled Mini-CEX (act-3-operations.md:814)
Role: Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Her sessions open acting as a Committee member; choose Switch to Assessor in the sidebar. From "Waiting for you",
  open Dr Mahlangu's Mini-CEX (the Activity inbox lists it too), rate it 3a with feedback, and complete it.
Expect: The sidebar first reads "Acting as Committee member", with "Switch to Assessor" under it, and Home "Committee
  member · Semester N, YYYY" under its heading. Under the header, above the committee cards, a warning line reads "1
  activity waits for you in the Activity inbox, and it is overdue: Mini-CEX (Paediatrics) · PAED-004 · `D−3`, from
  Nomsa Mahlangu, waiting 8 days.", with Open it (T350). After the switch Home reads "You are now acting as Assessor."
  in an info alert under the header, and "Assessor · Semester N, YYYY", with no such line; the sidebar reads "Acting
  as Assessor" over Home and Activity inbox, and "Waiting for you", in the warning stripe, is badged "1 waiting, 1
  overdue": this Mini-CEX, "Mini-CEX (Paediatrics) · PAED-004 · `D−3`", from Nomsa Mahlangu, Requested with Overdue
  beside it, "Waiting 8 days": it has waited past the assessor's seven days since Step 3.30 aged it. The inbox lists it
  with "8 days" over "since `D−8` … SAST"; the page's status card reads "Your move. Nomsa Mahlangu asked you on `D−8` …
  SAST.", the same moment. She rates it on the rung picker. After Complete the result reads "Completed. Nothing else
  waits for you.", with Go to Home, and Home then reads "Nothing is waiting for you." The Mini-CEX is Completed,
  credited "1 item". Year 1's minimum on PAED-004 is 3a. Nothing of Dr Mahlangu's is stalled any more.

Step 3.52 — Dr Zulu's committee dashboard, and Programme Trainees (act-3-operations.md:1194)
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → /
Do: Choose Switch to Committee member in the sidebar and read the dashboard and the menu.
Expect:
  - Home opens acting as Assessor, her choice from Step 3.33, kept with her account. After the switch an info alert
    reads "You are now acting as Committee member.", the sidebar "Acting as Committee member", and Home's subtitle
    "Committee member · Semester N, YYYY".
  - Targets this period, "Semester 2, 2026 · July to November": KGK's five current trainees, fewest met first.
    Pieter du Plessis, Nomsa Mahlangu and Sipho Ndlovu read "semester 0/10 · yearly 0/5". Anele Dlamini and Lerato
    Molefe read "semester 1/10 · yearly 0/5".
  - Targets met by EPA: PAED-001 (3 per semester) reads "2 of 5 met", and every other EPA "0 of 5 met".
  - Neither card holds a link.
  - Her menu is Home, Committee reviews and Decision panels, then My data rights. Programme trainees is flow 06's, and
    the menu offers no page before it is built.

Step 3.31 — Mr Smit opens Stalled Activities (act-3-operations.md:780)
Role: Coordinator — Mr Pieter Smit
Route: / → /not-found
Do: Look in the menu for a page of stalled requests. Then type the address the menu once linked,
  `/placeholder/stalled-activities`.
Expect: The menu offers none: Home, Decisions due, MSF campaigns, Committee reviews and Data rights requests, then My
  data rights. The nav links to no unbuilt page (DESIGN.md § The NavMenu); stalled work is flow 06's. Only Home's
  "Stalled requests" card lists it, and no page chases a stalled request: no reminder and no reassignment. The old
  address is "Page not found" with status 404: the placeholder page went with the stubs (T335, flow 01).

Step A.5.3 — A forged dashboard switch (appendix-cross-cutting.md:697)
Role: Trainee — Dr Anele Dlamini
Route: /dashboard/switch/{role} → /
Do: Open `/dashboard/switch/Administrator`.
Expect: She is back on her own dashboard, still acting as a Trainee. A switch to a role she does not hold changes
  nothing and says nothing (no "You are now acting as" alert), and the sidebar offers her no switch: she holds one role.

Step A.5.1 — A registrar opens an administrator's pages (appendix-cross-cutting.md:670)
Role: Trainee — Dr Anele Dlamini
Route: /admin/users → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Users page, then that of Scheduled jobs.
Expect: Each time, "You cannot open this page": "Your role (Trainee) does not open this page." and "If you need it for
  your work, ask your institution's Wombat administrator.", with Go to Home, which takes her home. Nothing of the page
  she asked for is shown or named, and no switch of role is offered (T335, D6).

Step A.5.2 — Prof Mbatha opens the Administrator's own pages (appendix-cross-cutting.md:684)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Institutions list, then that of Scheduled jobs.
Expect: For both, "You cannot open this page": "Your role (Institutional admin) does not open this page." and "If you
  need it for your work, ask the platform administrator.", with Go to Home. They are the Administrator's alone, her menu
  offers neither, and nothing in it is lit on the refusal.

Step A.6.3 — Prof Mbatha edits her own institution (appendix-cross-cutting.md:879)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /
Do: Open KGK's page by its address (no nav link leads there). Set its contact email to
  `hod.paediatrics@kgk.wombat.local` and save. Read Status, then look at Back to home and Cancel.
Expect: "Institution saved." takes the focus. Status reads "Active" as text, with "Set by a global administrator."
  beneath: there is no Active box and no Deactivate, because an institution's state is the Administrator's alone
  (T302). "Back to home" and Cancel both lead to `/`. SQL:
  `SELECT "Id","ContactEmail","IsActive" FROM "Institutions" WHERE "ShortCode"='KGK'` gives
  `2|hod.paediatrics@kgk.wombat.local|t`.

Step A.5.4 — Addresses that do not exist (appendix-cross-cutting.md:707)
Role: Trainee — Dr Anele Dlamini
Route: /not-found → /not-found → /account/login → /not-found
Do: Type the following addresses:
  - `/portfolio/cv`;
  - `/placeholder/recent-activities`, which the Assessor's menu once linked;
  - then, signed out, `/portfolio/cv` again.
Expect: For each, the "Page not found" page: "There is no page at this address." and "Check the address, or start
  again from Home.", with Go to Home; the page does not repeat the address (T335). The address stays as typed, and the
  status is 404 (the browser's network panel) (T233).
  - `/placeholder/recent-activities` is Page not found too: the placeholder page went with the stubs (T335, flow 01).
  - Signed out, the unknown address first asks her to sign in, then shows Page not found.

Step A.5.5 — Another institution's records, by id (appendix-cross-cutting.md:726)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found → /admin/users/{UserId}
Do: Open the Demo Institution's page by its id. Then open the page of the dev trainee `trainee@wombat.local` by that
  account's id.
Expect: The institution shows Page not found, not "You cannot open this page". The user shows "User unavailable" ("The
  user could not be found or is outside your scope."). Neither page confirms that the record exists (CLAUDE.md: 404, not
  403).

Step A.5.6 — Another registrar's activity, by id (appendix-cross-cutting.md:744)
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Open the address of Dr Mahlangu's submitted Mini-CEX from A.2.7.
Expect: The page is headed "Activity unavailable" and reads "This activity does not exist, or you cannot open it.", with
  Go to My activities: the same page an id that does not exist shows, so nothing of the activity is shown and nothing
  tells her it exists (T101, C7).

Step A.5.7 — A data-rights request that is not his, by id (appendix-cross-cutting.md:757)
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights/{Id:guid}
Do: Open the address of Dr Mahlangu's request with its last digit changed.
Expect: The refusal "You are not authorized to access this data-rights request." and the empty state "Not found". An
  unknown id and another institution's request read the same, so the page does not tell them apart (T112).

Step A.5.8 — The error page (appendix-cross-cutting.md:770)
Role: Trainee — Dr Anele Dlamini
Route: /Error
Do: Type `/Error`.
Expect: Typed, no request failed: "Nothing went wrong" and "This is Wombat's error page, opened directly. No request
  failed, so there is nothing to report.", with Go to Home, no reference and no Try again. Signed out it reads the same,
  with no sign-in first: the page is open to everyone, and static (T321, T335).

Step A.5.9 — Dr Patel's Recent Activities (appendix-cross-cutting.md:790)
Role: Assessor — Dr Mohammed Patel
Route: /
Do: Read the menu.
Expect: Home and Activity inbox, then My data rights. There is no Recent activities: the flow 01 pick dropped it, and
  the nav links to no page that is not built (DESIGN.md § The NavMenu).

Step A.5.10 — Mr Smit's Stalled Activities (appendix-cross-cutting.md:800)
Role: Coordinator — Mr Pieter Smit
Route: /
Do: Read the dashboard's "Stalled requests" card, then look for a stalled-work page in the menu.
Expect: The card lists Dr du Plessis's portfolio review, still awaiting review since Step 3.30 aged it, and its row
  links to the activity's page (T297). No page chases a stalled request: no reminder and no reassignment. The menu
  offers no stalled-work page: it is flow 06's, and the nav links to no page that is not built.

Step A.5.11 — Dr Botha's Programme Trainees (appendix-cross-cutting.md:815)
Role: CommitteeMember — Dr Sarah Botha
Route: /
Do: Read the menu.
Expect: "Acting as Committee member", with "Switch to Assessor" under it, over Home, Committee reviews and Decision
  panels, then My data rights. Programme trainees is flow 06's, and not offered before it is built.

Step A.5.12 — Dr Mokoena's Programme Trainees and STAR Review Queue (appendix-cross-cutting.md:825)
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /
Do: Read the menu.
Expect: Home, Decisions due, Committee reviews and Decision panels, then My data rights. Programme trainees is flow
  06's and the STAR review queue flow 09's; the menu offers neither before it is built.

Step A.5.13 — devadmin's System page (appendix-cross-cutting.md:835)
Role: Administrator — devadmin
Route: /
Do: Read the menu.
Expect: The grouped menu of Step 1.1, with no System item: the flow 01 pick dropped it, with its placeholder.

Step A.7.3 — Dr Dlamini on her phone (appendix-cross-cutting.md:1080)
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /portfolio/progress/{EpaId:int} → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My progress and one EPA's page from its index, My activities and My data
  rights from the menu.
Expect: The dashboard's cards stack, each Furthest short row's figure under its name. On My progress, This period, the
  index and the Entrustment panel stack: each EPA's link is a 44 px block, every cell but the link labelled by its
  column, and each Latest rating link in the panel is 44 px tall (T328); nothing scrolls sideways. The EPA's page draws
  its chart at 326 px, never scaled, with its table stacked under it (T355, T323). My activities' rows stack, each cell
  but the link and the state labelled by its column (T342), its Credit link 44 px tall, and the table of "Your requests"
  scrolls inside its container; each row's link or action stays reachable.

Step A.7.14 — Prof Mbatha's pages, checked for contrast (appendix-cross-cutting.md:1271)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/profile/submit → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient, and its current item (white on the .32 fill);
  - muted text on the page background;
  - the status badges on a committee review opened from Committee reviews (Dr Molefe's review 7; the list itself shows
    each state as plain text);
  - a success alert (save My account's name, from the name in the top bar, unchanged: "Name saved.") and a danger
    alert (Change password with a confirmation that differs, which checks no password and changes nothing);
  - a password field's Show toggle, pressed (white on the action blue, 4.86:1);
  - white on the primary, danger (Lock out user on Dr Patel's page, not pressed) and success (Publish in the builder,
    not pressed; with no draft it is disabled, and a disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.
```
