# F01 The frame everyone shares: navigation, the Home frame, role switching and the system states

**Who and why:** every role and every signed-out visitor meets this frame on every page. It settles the navigation,
the role switch, the failure screens and the tokens that F02–F18 sit inside, so it is briefed first (BRIEF.md § 2.3
step 2).

| | |
|---|---|
| Mode | **Wireframe first** for the shell, the nav per role and the dashboard grid, which are structure (BRIEF.md § 2.3). Then fidelity on the chosen frame, ending with the token sheet that every later flow uses. |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | 2–3 |
| People | All 10 roles (`src/Wombat.Domain/Identity/WombatRoles.cs`), a former trainee with no role, and signed-out visitors |
| Runbook steps | 21, pasted verbatim in § 8 |
| Pages (`coverage.md` templates) | `/`, `/access-denied`, `/not-found`, `/Error`, `/placeholder/{Feature}`; the endpoint `/dashboard/switch/{role}`; `Components/Layout/MainLayout.razor`, `NavMenu.razor`, `ReconnectModal.razor`; `#blazor-error-ui` (`MainLayout.razor:30`) |
| Held | Nothing. T297's 3 images were re-captured on 2026-09-26 (§ 4) |

**How to run this thread** (BRIEF.md § 2.3):
1. Set up the design system first (BRIEF.md § 2.3 step 1). Then open a new Claude Design thread for this flow alone.
2. Paste § 1, and attach the key screenshots in § 4 with it.
3. Paste § 8, the runbook steps, as the next message.
4. Attach the state screenshots in § 4 as the chat asks for them.
5. Pick one wireframe variation, then ask for fidelity and the token sheet.
6. Answer § 6's questions in the chat, one sentence per decision. Export the chosen artboards into
   `design/flows/01-shell/` before moving on.

---

## 1. The ask (paste this first)

```text
FLOW 01 — Know where I am, what I may do and which role I am acting in, and carry on when something fails

GOAL: Wherever a person is in Wombat, they know where they are, what they may do, and which role they are acting in,
  and they can switch role. When something fails, is refused, is not found or is not built, they are told plainly and
  can go on. This flow designs the frame every other flow sits in, and the token set every later flow uses.
  What is wrong today (the attached screenshots show each):
  - The nav is a flat list of up to 20 links (the Administrator's), and five of them open "Coming soon" stubs.
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
  CollegeAdmin: Specialities, EPAs, Curricula
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

SCREENS, in order:
  1. The shell at 1280 px: the navigation (sidebar or other), the top row and the content column. Show it with the
     Administrator's nav (the longest) and with the Assessor's (a short one).
  2. The shell at 390 px: the navigation folded, and open. The toggle must be CSS-only, because signed-out pages are
     static.
  3. The Home frame: the page header, the role-switch line for a two-role user ("Viewing as Committee member · Switch
     to Assessor"), and the dashboard grid that every role's dashboard sits in. The dashboards' own cards belong to
     later flows; show placeholder cards spanning one, two and three columns.
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
  Home frame: a two-role user; the same user after switching; loading (skeleton); load error (alert, with no empty
    state under it).
  Access denied: signed in, signed out, narrow. Page not found: signed in, signed out, narrow.
  Error: signed in with its request id, signed out (today it redirects to sign-in: design the page instead), narrow.
  Coming soon: each surviving placeholder; an unknown /placeholder/ address (Page not found, 404); narrow.
  Data volumes: the nav at 6, 9, 10, 18 and 20 links; a long name in the top row; a role label that wraps.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T322: contrast is fixed in the tokens. Text reaches 4.5:1; control boundaries and the focus ring reach 3:1
    (WCAG 1.4.3, 1.4.11). These fail today: success text on its tint 2.55, warning 2.42, danger 3.57; validation text
    on white 3.82; white on the danger button 3.82 and on the success button 2.87; the focus ring on the page 2.99;
    the input border 1.49. The navigation needs its own focus-ring token that reaches 3:1 on both ends of its
    gradient.
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
  - Observed, not filed: nav items render underlined; the navigation's gradient stops short of a full-page capture's
    height; Sign out appears twice; Home greets by email, not name.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Grouped navigation, or a flat list? The Administrator sees 20 links and the InstitutionalAdmin 18. Whatever the
     grouping, every link opens a page that admits the role, and no two links share a label.
  2. For each "Coming soon" item (Recent Activities, Stalled Activities, Programme Trainees, STAR Review Queue,
     System): will a later flow design it, or is the nav item dropped? The shell shows the outcome.
  3. Where does the role switch live: on Home only (today), or in the shell, so it is visible on every page?
  4. One Sign out: where?
  5. Does an activity page, reached from several lists (inbox, My Activities, a dashboard, a review), light a nav item?
  6. Should the error page and the sign-out confirmation page be reachable by any link? Today they are reached only by
     typing their address.
  7. Body typeface: keep the Segoe UI stack (only Windows has Segoe UI), or name a self-hosted GPLv3-compatible face?
     Dark mode and reduced motion: in or out? Neither exists today.

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

ASK:
  - 2–3 variations, as wireframes first: the shell at 1280 and 390 px, the Administrator's and the Assessor's nav, and
    the Home frame with the switch line. After I pick one, full fidelity for every screen above, then the token sheet.
  - Name every design-system component you use, and mark anything else NEW.
  - Say which DESIGN.md rule a variation breaks. The shell's 250 px sidebar, sticky top row, 641 px breakpoint and
    16 px gutter are rules; so are the nav's order, grouping and labels.
  - Flag edge cases: a 20-link nav on an 800 px-tall screen, a long name or email in the top row, a role label that
    wraps, a two-role user whose second role has a long name.
  - Run an accessibility review against WCAG 2.1 AA, and give a contrast figure for every token pair on the sheet.

ATTACHED: states/shell--nav-open.png, states/shell--nav-folded.png, states/home--committee-member.png,
  states/access-denied--signed-in.png, act-A/A.6.3-4-back-to-institutions-denied.png, states/error--signed-in.png,
  states/placeholder--stalled-activities.png, states/shell--reconnect-retrying.png, states/shell--error-banner.png,
  states/home--loading.png, act-A/A.7.14-2-profile-saved-success-alert.png, act-A/A.7.14-6-reviews-badges.png,
  act-1/1.5-2-kruger-invited.png (cropped: it is here only for the underlined nav), states/home--administrator.png
  (the 20-link nav).
```

---

## 2. The journey

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
| Coming soon | Recent Activities | `states/placeholder--recent-activities.png` | Assessor |
| Coming soon | Stalled Activities | `states/placeholder--stalled-activities.png` | Coordinator |
| Coming soon | Programme Trainees | `states/placeholder--programme-trainees.png` | CommitteeMember |
| Coming soon | STAR Review Queue | `states/placeholder--star-review-queue.png` | SpecialityAdmin |
| Coming soon | System | `states/placeholder--system.png` | Administrator |
| Coming soon | Unknown feature | `states/placeholder--unknown-feature.png` | Page not found, 404 |
| Coming soon | Narrow | `states/placeholder--narrow.png` | |

Not captured, so describe them in words:
- **The error page signed out.** It redirects to sign-in today (`act-A/A.5.8-2-error-signed-out.png` shows the
  redirect). T321 wants the designed page for both.
- **The error page through a real failure.** No exception handler is configured, so a failure reaches the developer
  page on dev or a bare 500 (`states.md` § States no local replay reaches).
- **The reconnect dialog's "rejected" state.** It reloads the page at once (the same section).
- **Session ended.** It is the sign-in page's state and belongs to F02 (`states.md` § Shell and framework).
- **A former trainee's frame.** Her Home today is "No role assigned" (`states/home--no-role.png`, T311). Her dashboard
  is F13's (BRIEF.md § 7 B1), but her nav (My Progress only) is this flow's.

## 4. Attach

Paths are relative to `design/baseline/`. Every one below was checked with `ls` on 2026-09-26.

**Key screenshots (attach these first, with § 1):**
1. `states/shell--nav-open.png`
2. `states/shell--nav-folded.png`
3. `states/home--committee-member.png`
4. `states/access-denied--signed-in.png`
5. `act-A/A.6.3-4-back-to-institutions-denied.png`
6. `states/error--signed-in.png`
7. `states/placeholder--stalled-activities.png`
8. `states/shell--reconnect-retrying.png`
9. `states/shell--error-banner.png`
10. `states/home--loading.png`
11. `act-A/A.7.14-2-profile-saved-success-alert.png`
12. `act-A/A.7.14-6-reviews-badges.png`
13. `act-1/1.5-2-kruger-invited.png`. It is here only to show the underlined nav. **Crop out its registration link
    first** (BRIEF.md § 3.3), or leave it out.
14. `states/home--administrator.png`: the Administrator's 20-link nav (optional; Administrator homes are not held).

**States (attach as the chat asks):**
- `states/shell--reconnect-rejoining.png`, `states/shell--reconnect-failed.png`, `states/shell--reconnect-paused.png`,
  `states/shell--reconnect-resume-failed.png`
- `states/home--load-error.png`
- `states/access-denied--signed-out.png`, `states/access-denied--narrow.png`
- `states/not-found--signed-in.png`, `states/not-found--signed-out.png`, `states/not-found--narrow.png`
- `states/error--narrow.png`
- `states/placeholder--recent-activities.png`, `states/placeholder--programme-trainees.png`,
  `states/placeholder--star-review-queue.png`, `states/placeholder--system.png`,
  `states/placeholder--unknown-feature.png`, `states/placeholder--narrow.png`
- `act-A/A.5.8-2-error-signed-out.png` (the signed-out redirect)

**Re-captured after T297 landed (2026-09-26), so no longer held; attach as the chat asks.** Each is from the end of its
act, not the step's own moment (BRIEF § 10):
- `states/home--assessor-switched.png` and `act-2/2.34-1-zulu-assessor-view.png` (Step 2.34: the empty Assessor view
  after the switch)
- `act-A/A.5.3-1-forged-switch-trainee-view.png` (Step A.5.3: still "Viewing as Trainee")

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists.

| Task | What it means for the design | Evidence (under `design/baseline/`) |
|---|---|---|
| **T322** (P2, High) | The token sheet is where contrast is fixed, not each page. Every alert tint, badge tint, validation colour, button fill, input border and focus ring on the sheet carries its measured ratio: text 4.5:1, boundaries and focus 3:1. The navigation gets a focus-ring token of its own that reaches 3:1 on both ends of its gradient. Today's failing pairs are in § 1. | `act-A/A.7.14-2-profile-saved-success-alert.png`, `act-A/A.7.14-6-reviews-badges.png`; the measurements are from step A.7.14 (BRIEF.md § 6, A1) |
| **T321** | Design the error page for signed-in and signed-out visitors, with the request id as quotable text. Access denied is one page with one frame, whether it is reached by a typed address or an in-app link. | `act-A/A.6.3-4-back-to-institutions-denied.png`, `states/error--signed-in.png`, `act-A/A.5.8-2-error-signed-out.png` |
| **T330** | One message per reconnect state, and a button whose label matches the message. Keep the `components-reconnect-*` class names (`Layout/ReconnectModal.razor.css`). | `states/shell--reconnect-retrying.png`, `states/shell--reconnect-resume-failed.png` |
| **T331** | The active item is designed, and a rule says which item a sub-page lights. Never two at once. | `states/specialities-list--loading.png`, `states/campaign-report--loading.png` (both F16/F10 pages, shown here for the unlit nav) |
| **T328** | Controls in the body font; the design's focus ring on a focusable scroll region; badges that stay pills in a flex row; header actions of one height; a top-row account link at least 24 px tall. | `states/home--narrow-trainee.png` (the stretched badge; re-captured after T297, and still stretched), `act-A/A.7.5-3-decisions-due-summary-scrolled.png` |
| **T329** | The shared loading and load-error states: the header from the first render, a skeleton while the first read runs, an alert (not an alert above an empty state) when it fails, and no action before the record loads. The in-app error bar must not be the way a failed read ends. | `states/home--loading.png`, `states/home--load-error.png`, `states/shell--error-banner.png` |
| **T325** | Wherever the frame prints a time, it is South African time with its zone. | BRIEF.md § 6, A9 |
| **T324** | Home greets by name, and the switch line names roles by label ("Committee member"). | `states/home--committee-member.png` |
| **T190** | One page-title pattern for every tab. | T190's symptom: "Dashboard — Wombat", "Sign in - Wombat", and most pages with no suffix |
| **T317** | The switch choice is remembered per person, and does not carry over to the next person who signs in on the same browser. | `act-2/2.34-4-naidoo-inherits-view.png` |
| Observed, not filed | Nav items render underlined (`NavMenu.razor.css` sets `text-decoration: none` on the brand, line 38, but not on `.nav-link`). The navigation's gradient stops short of a full-page capture's height. Sign out appears twice (`MainLayout.razor:13–16` and the nav's Logout form). Home greets by email. | `act-1/1.5-2-kruger-invited.png`, `states/home--committee-member.png` |

## 6. Questions the design must answer

1. **Grouped navigation or a flat list?** The Administrator sees 20 links, the InstitutionalAdmin 18
   (DESIGN.md:192–205). Whatever the grouping, every link must open a page that admits the role, and no two links may
   share a label (`NavMenuAuthorizationTests` parses DESIGN.md's nav table, so a regrouping is a DESIGN.md change).
2. **The five "Coming soon" items** (`NavMenu.razor:108, 113, 114, 116, 132`; BRIEF.md § 7 B7). For each, either a
   later flow designs it or the nav item goes: Recent Activities (F04); Stalled Activities, Programme Trainees and STAR
   Review Queue (F06); System (F18). The shell shows the outcome. Their contents are inferred: no intent document
   exists.
3. **Where does the role switch live?** On Home only, as today, or in the shell, so the role is visible on every page?
4. **One Sign out, where?** The nav's Logout and the top row's Sign out duplicate each other. Both are form posts
   (DESIGN.md § The NavMenu), and either survives the choice.
5. **Does `/activities/{id}` light a nav item?** It is reached from the inbox, My Activities, the dashboards and a
   review. T331 leaves it unlit unless a rule is chosen.
6. **Should `/Error` and `/account/logout-confirm` be reachable by a link?** Today only a typed address reaches either
   (`coverage.md` § Reached only by address).
7. **Typeface, dark mode, reduced motion.** The body stack starts with Segoe UI (`app.css:50`), which only Windows has.
   Keep it, or name a self-hosted, GPLv3-compatible woff2 for `wwwroot/fonts`. Fraunces stays the wordmark's face
   (`--font-display`). Dark mode and reduced motion are both absent from `app.css` today: in or out?

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the 21 steps** on a fresh database (BRIEF.md § 9 gives the commands), in act order: 2.33, 2.34, 3.31, 3.33,
  3.52, then the appendix's A.5.1–A.5.13, A.6.3, A.7.3 and A.7.14. Every Expect must hold. If the design changes the
  wording a step quotes, update that Expect in the same task (BRIEF.md § 9 item 7). Steps 1.1 and 2.33 list the nav,
  and 2.33, 2.34, 3.33 and 3.52 quote the switch line.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`). They
  must include:
  - `Navigation/NavMenuAuthorizationTests`, which covers the nav table, the role unions and `PlaceholderPage.Headings`;
  - `Navigation/DashboardLinkAuthorizationTests`;
  - `Design/NarrowLayoutTests`, `DesignSystemSmokeTests`, `PageShapeSmokeTests`, `DefinedClassTests`, `AlertRoleTests`,
    `BadgeForStatusTableTests`, `InvalidFieldStyleTests` and `FieldGroupTests`;
  - T322's planned `Design/ContrastTests`, added in the same task;
  - `Hosting/AppAssetUrlTests`, if a font or script is added;
  - `Scenario/`.
- **Re-capture** the 27 states in § 3 and the steps' own captures into `design/baseline/`. Compare them with the chosen
  artboards.
- **Browser check** at 1280 and at 390 px for every row of DESIGN.md's nav table, and for a two-role user. Check the
  reconnect dialog by suspending the app (`states.md` § Shell and framework).

## 8. The runbook steps, verbatim (paste this second)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as it is today. Steps 2.34 and 3.33 describe the Assessor dashboard cards as T297 left them
(re-pasted on 2026-09-26 after it landed); F04 designs them.

```text
Step 2.33 — Dr Zulu's first view: committee member (act-2-onboarding.md:537)
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: /account/login → /
Do: Sign in and read the dashboard and nav.
Expect: The page reads "Viewing as CommitteeMember" and "You also act as Assessor. Switch view: Assessor". Targets this
  period names the current semester and its months. It lists the five registrars, each at "semester 0/10 · yearly 0/5",
  and nobody is exempt: a 15 January start counts from the boundary (D42). Targets met by EPA lists PAED-001 to PAED-015,
  each "0 of 5 met" (T130's count, never a percentage). The nav reads Activity Inbox, Recent Activities, Programme
  Trainees, Decision Panels and Committee Reviews, each once (T178).

Step 2.34 — Dr Zulu switches between her dashboards (act-2-onboarding.md:552)
Role: CommitteeMember + Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /account/logout → /account/login → / → /dashboard/switch/{role} → /
Do: Choose Switch view: Assessor. Sign out and in again. Then type `/dashboard/switch/Administrator`.
Expect: The page reads "Viewing as Assessor" and "You also act as CommitteeMember. Switch view: CommitteeMember". The
  Assessor dashboard reads 0 "assessments awaiting review" with Review inbox, "Nothing is awaiting your review." (T297),
  "No decisions yet." and "Open my inbox →". The nav is unchanged. After signing in again she lands on the Assessor view,
  because the choice is remembered. The Administrator address brings her back to "Viewing as CommitteeMember": a role she
  does not hold is never shown.

Step 3.33 — Dr Zulu switches to her assessor view and completes the stalled Mini-CEX (act-3-operations.md:580)
Role: Assessor — Dr Thandi Zulu
Route: / → /dashboard/switch/{role} → / → /activities/inbox → /activities/{ActivityId:int}
Do: Her dashboard opens as a CommitteeMember; switch the view to Assessor. From "Review inbox", open Dr Mahlangu's
  Mini-CEX, rate it 3a with feedback, and complete it.
Expect: Home first reads "Viewing as CommitteeMember" and "You also act as Assessor. Switch view: Assessor". After the
  switch it reads "Viewing as Assessor", and "Pending requests" counts 1 assessment awaiting review: this Mini-CEX.
  Awaiting your review lists it, "Mini-CEX (Paediatrics) — Nomsa Mahlangu", badged Overdue: it has waited past the
  assessor's seven days since Step 3.30 aged it. After Complete, the Mini-CEX is Completed, credited "1 item". Year 1's
  minimum on PAED-004 is 3a. Nothing of
  Dr Mahlangu's is stalled any more.

Step 3.52 — Dr Zulu's committee dashboard, and Programme Trainees (act-3-operations.md:908)
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

Step 3.31 — Mr Smit opens Stalled Activities (act-3-operations.md:550)
Role: Coordinator — Mr Pieter Smit
Route: /placeholder/{Feature}
Do: Choose Stalled Activities in the nav (`/placeholder/stalled-activities`). Then open a mistyped address,
  `/placeholder/stalled-work`.
Expect: Stalled Activities reads "This page is not built yet." with a "Coming soon" card. There is no page from which to
  chase a stalled request: no reminder and no reassignment. The mistyped address is "Page not found" with status 404,
  not a "Coming soon" (T178).

Step A.5.3 — A forged dashboard switch (appendix-cross-cutting.md:681)
Role: Trainee — Dr Anele Dlamini
Route: /dashboard/switch/{role} → /
Do: Open `/dashboard/switch/Administrator`.
Expect: She is back on her own dashboard, viewing it as a Trainee. A switch to a role she does not hold changes
  nothing, and no other view is offered to her.

Step A.5.1 — A registrar opens an administrator's pages (appendix-cross-cutting.md:659)
Role: Trainee — Dr Anele Dlamini
Route: /admin/users → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Users page, then that of Scheduled Jobs.
Expect: Each time, the "Access denied" page: she does not have permission to view the page, her role does not allow
  access to the area, and "Back to home" takes her home. Nothing of the page she asked for is shown.

Step A.5.2 — Prof Mbatha opens the Administrator's own pages (appendix-cross-cutting.md:671)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions → /access-denied → /admin/jobs → /access-denied
Do: Type the address of the Institutions list, then that of Scheduled Jobs.
Expect: Access denied for both. They are the Administrator's alone, and her nav offers neither.

Step A.6.3 — Prof Mbatha edits her own institution (appendix-cross-cutting.md:864)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /access-denied
Do: Open KGK's page by its address (no nav link leads there). Set its contact email to
  `hod.paediatrics@kgk.wombat.local` and save. Press Deactivate. Untick Active and save. Then press Back to
  institutions.
Expect: The contact email saves ("Institution saved."). Deactivate is refused: "Only global administrators may
  deactivate institutions." Unticking Active is refused as Deactivate is, and KGK stays Active. Back to institutions
  and Cancel lead to the Institutions list, which is the Administrator's alone, so she lands on Access denied
  (reported).

Step A.5.4 — Addresses that do not exist (appendix-cross-cutting.md:693)
Role: Trainee — Dr Anele Dlamini
Route: /not-found → /placeholder/{Feature} → /account/login → /not-found
Do: Type the following addresses:
  - `/portfolio/cv`;
  - `/placeholder/reports`;
  - then, signed out, `/portfolio/cv` again.
Expect: For each, the "Page not found" page: the page asked for does not exist or has moved, and "Back to home" is
  offered. The address stays as typed, and the status is 404 (the browser's network panel) (T233).
  - `/placeholder/reports` is Page not found too, not a "Coming soon" (T178).
  - Signed out, the unknown address first asks her to sign in, then shows Page not found.

Step A.5.5 — Another institution's records, by id (appendix-cross-cutting.md:712)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/institutions/{Id:int} → /not-found → /admin/users/{UserId}
Do: Open the Demo Institution's page by its id. Then open the page of the dev trainee `trainee@wombat.local` by that
  account's id.
Expect: The institution shows Page not found, not Access denied. The user shows "User unavailable" ("The user could not
  be found or is outside your scope."). Neither page confirms that the record exists (CLAUDE.md: 404, not 403).

Step A.5.6 — Another registrar's activity, by id (appendix-cross-cutting.md:728)
Role: Trainee — Dr Anele Dlamini
Route: /activities/{ActivityId:int}
Do: Open the address of Dr Mahlangu's submitted Mini-CEX from A.2.7.
Expect: "Activity unavailable": the requested activity could not be loaded. Nothing of it is shown, and nothing on the
  page speaks of permission.

Step A.5.7 — A data-rights request that is not his, by id (appendix-cross-cutting.md:739)
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights/{Id:guid}
Do: Open the address of Dr Mahlangu's request with its last digit changed.
Expect: The refusal "You are not authorized to access this data-rights request." and the empty state "Not found". An
  unknown id and another institution's request read the same, so the page does not tell them apart (T112).

Step A.5.8 — The error page (appendix-cross-cutting.md:754)
Role: Trainee — Dr Anele Dlamini
Route: /Error
Do: Type `/Error`.
Expect: "Something went wrong": an unexpected error interrupted the request. If it keeps happening, she is to contact
  her administrator, quoting the request ID the page gives. Signed out, `/Error` asks her to sign in first.

Step A.5.9 — Dr Patel's Recent Activities (appendix-cross-cutting.md:772)
Role: Assessor — Dr Mohammed Patel
Route: /placeholder/{Feature}
Do: Open Recent Activities from the nav.
Expect: A page headed "Recent Activities" says it is not built yet. Its "Coming soon" card says the menu links to it so
  that the role's navigation is complete. Nothing else is offered.

Step A.5.10 — Mr Smit's Stalled Activities (appendix-cross-cutting.md:783)
Role: Coordinator — Mr Pieter Smit
Route: / → /placeholder/{Feature}
Do: Read the dashboard's "Stalled requests" card, then open Stalled Activities from the nav.
Expect: Whatever the card lists, no row links anywhere, so there is no page to act on a stalled request from (Step
  3.30). Stalled Activities is the "Coming soon" stub, headed "Stalled Activities".

Step A.5.11 — Dr Botha's Programme Trainees (appendix-cross-cutting.md:799)
Role: CommitteeMember — Dr Sarah Botha
Route: /placeholder/{Feature}
Do: Open Programme Trainees from the nav.
Expect: The "Coming soon" stub, headed "Programme Trainees".

Step A.5.12 — Dr Mokoena's Programme Trainees and STAR Review Queue (appendix-cross-cutting.md:808)
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /placeholder/{Feature} → /placeholder/{Feature}
Do: Open Programme Trainees, then STAR Review Queue, from the nav.
Expect: Both are the "Coming soon" stub, each under its own heading.

Step A.5.13 — devadmin's System page (appendix-cross-cutting.md:818)
Role: Administrator — devadmin
Route: /placeholder/{Feature}
Do: Open System from the nav.
Expect: The "Coming soon" stub, headed "System".

Step A.7.3 — Dr Dlamini on her phone (appendix-cross-cutting.md:1062)
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My Progress, My Activities and Data Rights from the folded nav.
Expect: The dashboard's cards stack. On My Progress, each EPA's figures and trajectory fit the width. The tables of My
  Activities and "Your requests" scroll inside their containers, and each row's action stays reachable.

Step A.7.14 — Prof Mbatha's pages, checked for contrast (appendix-cross-cutting.md:1291)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /account/login → / → /account/profile → /account/change-password → /admin/users/{UserId} → /admin/activity-types/{ActivityTypeId:int} → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: With a contrast checker (axe, or the browser's accessibility audit), check each pair below:
  - the sign-in card;
  - the nav on its gradient;
  - muted text on the page background;
  - the status badges on a committee review opened from Committee Reviews (Dr Molefe's review 7; the list itself shows
    each state as plain text);
  - a success alert (save My Account unchanged: "Profile saved.") and a danger alert (Change password with a
    confirmation that differs, which checks no password and changes nothing);
  - white on the primary, danger (Lock out user on Dr Patel's page, not pressed) and success (Publish in the builder,
    not pressed; with no draft it is disabled, and a disabled control is exempt) buttons;
  - the focus ring on white and on the page background;
  - an input's border.
Expect: Every pair meets WCAG 2.1 AA: text 4.5:1, large text 3:1, and 3:1 for a control's boundary and the focus
  ring. Muted text passes on the page background since T086.
```
