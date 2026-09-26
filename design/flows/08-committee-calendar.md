# F08 Panels, what is due, and the schedule

The institution's programme staff form the review panel and see which panel decides each EPA. They read what the
committee must decide this period, and schedule each registrar's annual, formative or pre-graduation review from it.
They must be stopped from scheduling a duplicate. A missed decision is a missed milestone for a registrar.

Written 2026-09-26 for T332, following `design/BRIEF.md` § 2.4. Paths to screenshots are under `design/baseline/`
(gitignored). Claims about Claude Design use BRIEF § 2.0's source keys.

| | |
|---|---|
| People | InstitutionalAdmin: Prof Nolwazi Mbatha. SpecialityAdmin: Dr Refilwe Mokoena. SubSpecialityAdmin: Dr Kabelo Sithole. Coordinator: Mr Pieter Smit. CommitteeMembers Dr David Naidoo and Dr Sarah Botha read the panels and the schedule; scheduling and Decisions Due are refused to them (Step 4.5) (`README.md` § Cast) |
| Pages | `/committee/panels` (`CommitteeDecisions/PanelsList.razor`), `/committee/panels/new` and `/committee/panels/{PanelId:int}` (`PanelEdit.razor`), `/committee/decisions-due` (`DecisionsDue.razor`), `/committee/reviews` (`ReviewsSchedule.razor`), `/committee/reviews/{ReviewId:int}` before its sitting (`ReviewDetail.razor`), `/access-denied` (`coverage.md` § Pages) |
| Steps | 30: 2.20–2.25, 2.32, 4.1–4.13, 4.33, 4.34, 4.37, 4.49, 5.1, 5.2, 5.29, A.1.10, A.7.5, A.7.8 |
| Frequency and stakes | Each semester. Weekly for the Coordinator around a sitting. Medium-high: a registrar whose EPA is not decided in its window shows as Missed (Step 4.2). |
| Mode | **Straight to fidelity** for the panels and the schedule: their List and Form shapes hold (BRIEF § 4, recommendation). **Wireframe first** for Decisions Due, a summary plus a long list in a scroll region. |
| Held | **Nothing held.** No group-1 task changes these pages. Two of the steps' Home captures, `act-2/2.32-1-smit-home.png` and `act-4/4.3-1-mokoena-dashboard-nav.png`, are held by T297, but they are F06's dashboards and are not used here (BRIEF § 10). |

---

## 1. How to run this thread

1. Run F01 first (BRIEF § 2.3, step 2).
2. Start a new thread. Attach the key screenshots in § 5.1, then paste § 2.1 as one message.
3. The panels and the schedule go straight to fidelity. For Decisions Due, pick a wireframe first. Record in the chat,
   in one sentence, whether Decisions Due and the schedule become one page or stay two. Then paste § 2.2.
4. Attach the state captures in § 5.2 as the chat asks for them.
5. Export the chosen artboards to `design/flows/08-committee-calendar/` before moving on. There is no version history
   [start].

## 2. The ask

### 2.1 Paste this first, with the key screenshots attached

```
FLOW 08 — Panels, what is due, and the schedule

GOAL: The people who run the programme must be able to:
- form a review panel (a chair, members and external members) and see which panel decides each EPA;
- see what the committee must decide this period, per registrar and per EPA, and where each decision stands;
- schedule each registrar's review from that list: an annual progression review, a formative check-in (no binding
  decision), or a pre-graduation review;
- be stopped, before they press Create, from scheduling a second binding review for a period the registrar already
  has one for.

What is wrong today:
- The agenda preview gives no sign that the registrar already holds the period's binding review. The refusal comes only
  on Create review.
- A formative check-in is typed and shown as "Annual progression review", because the Review type select stays live
  when "Formative only" is ticked.
- A panel's own page never shows the panel's name, scope or speciality.
- While the panels list loads, it already says "There is no committee routing to show for your institution."
- The panel form is a narrow column. Members are chosen from native multi-selects ("Hold Ctrl (Windows) / Cmd (Mac) to
  select multiple") that cut names short.
- Decisions Due's summary scrolls sideways at 390 px, and its focus ring is the browser's thin default.
- The Coordinator may open Decision Panels, but his nav has no link to it.

AUDIENCE:
- InstitutionalAdmin: Prof Nolwazi Mbatha (head of department). She manages every panel at her institution.
- SpecialityAdmin: Dr Refilwe Mokoena (programme director). SubSpecialityAdmin: Dr Kabelo Sithole (sub-speciality
  lead). They manage their speciality's panels and schedule reviews.
- Coordinator: Mr Pieter Smit (programme administrator). He schedules reviews and reads the panels.
- CommitteeMembers Dr David Naidoo and Dr Sarah Botha. They read the panels and the reviews of the panels they sit on;
  they are not offered scheduling or Decisions Due.
- Kgosi Kgari Teaching Hospital, Paediatrics, five registrars. Desktop 1280×800 and phone 390×844.

WHAT THE PAGES CARRY TODAY:
- Decision panels (list):
  - A table: Name, Scope (Institution or Speciality), Decides for (General panel, or a College committee such as the
    "Neonatal team Clinical Competency Committee"), Members, and Edit for those who manage the panel.
  - New panel in the header.
  - A card, "Who decides each EPA": per curriculum, the EPAs and the panel that decides them. For example "PAED-004,
    PAED-005 → Paed Annual Review Panel. No panel covering this programme sits as the Neonatal team Clinical
    Competency Committee, so the general panel decides."
- Decision panel (form):
  - Panel name, Scope, Speciality, Decides for (the InstitutionalAdmin only; read-only for a speciality admin), Chair,
    Members, External members, and Save panel.
  - Choosing the chair drops her from Members, with the note "Thandi Zulu is the chair now, so is no longer selected
    under Members."
  - Refusals: "A panel needs at least two members: the chair and at least one other, so that no decision is one
    person's." And, while a review before the panel is open, a change of committee is refused: "Review #1 (2026 S2,
    scheduled) still sits before this panel. …"
  - Once saved, the page has a "Decides for" card (Save committee) and "Update members".
- Decisions due:
  - Filters: Period (the current semester by default), Status ("Outstanding: not yet decided" by default, "Every
    status", or each status), and EPA. An Administrator also picks an Institution.
  - The opening sentence names the period, how many trainees and where, and what Missed means.
  - "By EPA": a summary table, EPA × Due, Decided, Scheduled, Deferred, To schedule, Missed, Optional. It scrolls
    sideways at 390 px.
  - A count: "75 of 75 decisions due in 2026 S2 shown."
  - The list: Trainee, EPA, window ("2026 S2", or "2026" for an annual EPA), status (a badge plus a sentence), and an
    action. Twenty rows a page; 75 rows for five registrars.
  - Statuses: Not scheduled, Scheduled ("On the agenda of review #1, which is still open."), Decided ("STAR #1, issued
    at review #1."), Deferred, As opportunity allows, Due by year end, Missed, and "Revoked: re-decide".
  - Actions: Schedule, which opens the schedule form already filled with panel, trainee and period; and Open review.
- Committee reviews (the schedule):
  - Schedule review in the header, for those who may schedule.
  - The form: Panel; Trainee (disabled until a panel is chosen); Period ("2026 S2 · 1 Jul to 31 Dec 2026"); Review
    period from and to (filled from the period); Scheduled on; Review type (Annual progression review or
    Pre-graduation review, with Entrustment-only review in semester 1); and Formative only.
  - The agenda preview: "15 EPAs will be on the agenda for 2026 S2." It names the EPAs that must be decided, lists the
    optional ones, and says "Already decided in this window, so not on the agenda: …". For a formative review: "A
    formative review carries no agenda: it decides no EPA."
  - A list: Trainee, Period with its window, Type, Mode, State, Decision, and Open.
  - The duplicate refusal, in full: "Review #1 already puts this trainee before Paed Annual Review Panel for 2026 S2
    (scheduled, scheduled 2026-09-26). A trainee has one binding review for each period before the panels that decide
    the same EPAs, until it is ratified: open that review, or ratify it before scheduling another."
- The review page before its sitting: the Review card (trainee, panel, period, window, type, mode, state), Start review
  for those who may start it, or "Only the coordinators of the panel's institution, and those of its members who are
  active committee members there, can start this review." The sitting itself is Flow 07.

DATA VOLUMES: none (no panels; nothing due; no reviews); typical (1 panel with 4 members; 75 decisions due for 5
registrars; 5 reviews); heavy (two panels, one sitting as the neonatal committee; 8 reviews; revoked STARs to decide
again).

MODE: Full fidelity for screens 1, 2, 4 and 5, which keep today's List and Form shapes. Wireframe first for screen 3,
Decisions Due, and for the question whether it merges with screen 4.

SCREENS, in order:
1. /committee/panels — Decision panels: empty; the manager's list; a speciality admin's; a member's (no New panel, no
   Edit); the coordinator's; and "who decides each EPA".
2. /committee/panels/new and /committee/panels/{id} — a panel: new; the chair note; a panel of one refused; saved; as a
   speciality admin (Decides for read-only) and a sub-speciality admin (Speciality scope only); the committee change
   refused; a member who can no longer sit; not found.
3. /committee/decisions-due — Decisions Due: nothing due; an Administrator who must choose an institution; outstanding;
   no row matches the filters; a past semester with Missed rows; all scheduled; after the sitting (Decided, Deferred);
   a revoked STAR to decide again.
4. /committee/reviews — the schedule: empty for a scheduler and for a member; the form; the agenda preview; a duplicate
   refused; scheduled; filled from Decisions Due; formative; pre-graduation; ratified; the preview failed.
5. /committee/reviews/{id} before its sitting: scheduled with no Start for this viewer, and a formative review as
   scheduled.

STEPS: 2.20–2.25, 2.32, 4.1–4.13, 4.33, 4.34, 4.37, 4.49, 5.1, 5.2, 5.29, A.1.10, A.7.5 and A.7.8, pasted verbatim at
the end of this message (Role / Route / Do / Expect). In them, D is the day the story is replayed, <P> the semester
holding D (for example 2026 S2) and <Y> its year; J is the latest 15 January on or before D.

STATES TO SHOW: every state named in SCREENS, plus loading and 390 px for each page, and access denied for a role a
page refuses (the Coordinator on the new-panel form; a committee member on Decisions Due).

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
- T309: when the registrar already holds the period's binding review, the agenda preview says so first, in the create's
  own words, with an Open review link. Create review is shown disabled with that reason. A formative check-in is named
  "Formative check-in" everywhere, and the Review type select is hidden while "Formative only" is ticked.
- T259: Decisions Due names the panel the holding review sits before, not the panel the EPA routes to.
- T260: after a panel save, no stale "can no longer sit" warning remains. The panel's page names the panel, its scope
  and its speciality.
- T329: the panels list never says "no committee routing" while it is still loading: a skeleton shows instead.
- T328: a focusable scroll region (the By EPA summary) shows the design's focus ring.
- T264: identical row actions get per-row names. Pagers keep the focus on a page change.
- T280: the pager's page-size select has a name ("Per page" is a label, not loose text).

QUESTIONS THE DESIGN MUST ANSWER:
1. Should Decisions Due and the schedule be one page (what is due, and scheduling it in place) or two linked pages?
2. How does a scheduler see, before saving, that the registrar already has a binding review this period?
3. Should the Coordinator get a Decision Panels link? The page admits him today; his nav has none.

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

ASK: 2–3 variations. Full fidelity for the panels and the schedule; wireframes first for Decisions Due. Name every
design-system component you use and mark anything else NEW. Say which DESIGN.md rule a variation breaks. Flag edge
cases and draw the none, typical and heavy data volumes. Review the result for accessibility against WCAG 2.1 AA.

ATTACHED: states/panel-edit--chair-note.png, states/panels-list--manager.png, act-2/2.24-1-who-decides.png,
states/decisions-due--outstanding.png, states/decisions-due--missed.png, states/reviews-schedule--preview.png,
states/reviews-schedule--refused.png, states/reviews-schedule--prefilled.png,
act-A/A.7.5-3-decisions-due-summary-scrolled.png, states/panels-list--loading.png

RUNBOOK STEPS, VERBATIM:

--- from execution/knowledge/scenario-paediatrics/act-2-onboarding.md ---

### Step 2.20 — Decision panels before any exist
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Open Decision Panels.
Expect: The page reads "No decision panels" / "Create a panel before scheduling reviews." and offers New panel. "Who
  decides each EPA" names `Paediatric EPA Curriculum 11.1` and says against every EPA "No panel at this institution
  covers this programme."

### Step 2.21 — The new-panel form, and a panel of one refused
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new
Do: Name it "Paed Annual Review Panel", with scope Speciality, speciality Paediatrics and Decides for General panel.
  Select Zulu and Naidoo under Members, then choose Zulu as Chair. Unselect Naidoo, leaving the chair alone, and save.
Expect: Scope offers Institution and Speciality and starts on Speciality. Speciality offers Paediatrics alone ("The panel
  runs at your institution. Only the specialities it has adopted a curriculum in are listed."). Decides for offers
  General panel and the Neonatal team Clinical Competency Committee. Chair, Members and External members each list
  exactly Botha, Naidoo, van Rensburg and Zulu: active committee members at KGK who are not trainees (T165, T237).
  Choosing Zulu as chair shows "Thandi Zulu is the chair now, so is no longer selected under Members." (T257). The save
  is refused: "A panel needs at least two members: the chair and at least one other, so that no decision is one
  person's." (D46).

### Step 2.22 — Mbatha saves the panel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new → /committee/panels/{PanelId:int}
Do: Select Naidoo under Members and van Rensburg under External members, then save. Botha is left for Step 2.23.
Expect: The page reloads as the panel's own page. It has a "Decides for" card reading General panel with Save
  committee, and Update members shows Zulu as chair, Naidoo as a member and van Rensburg as external. The External
  members help text says external members sit with the chair on the appeal body and that cross-institution externals
  are not yet supported.

### Step 2.23 — Dr Mokoena adds Dr Botha to the panel
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/panels → /committee/panels/{PanelId:int} → /committee/panels
Do: Sign in, open Decision Panels and Edit the panel. Select Botha under Members as well, then save and go back to the
  panels.
Expect: The list offers her New panel and an Edit on this panel, a Speciality-scoped panel in her speciality at her
  institution (T194). Decides for is read-only: "General panel", with "Only an institutional administrator can change
  which College committee a panel sits as." The save reads "Panel members updated.", and the list's Members column
  reads 4.

### Step 2.24 — Who decides each EPA now
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Read the list and the routing card.
Expect: One row: Paed Annual Review Panel, Speciality, General panel, 4 members, with an Edit. PAED-004 and PAED-005 go to
  the panel with "No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the
  general panel decides." (T131). The other thirteen EPAs go to the panel.

### Step 2.25 — Dr Sithole looks at what he may create
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/panels → /committee/panels/new
Do: Sign in, open Decision Panels, press New panel and read the form. He leaves without saving.
Expect: The list offers him New panel and an Edit on the panel. The form's Scope offers Speciality only, because a
  sub-speciality administrator manages the Speciality-scoped panels of his sub-speciality's speciality (T194, T245).
  Speciality offers Paediatrics. There is no Decides for field. Nothing is created.

### Step 2.32 — Mr Smit, Coordinator
Role: Coordinator — Mr Pieter Smit
Route: /account/login → / → /admin/invitations → /access-denied → /committee/panels → /committee/panels/new → /access-denied
Do: Sign in and read the dashboard and nav. Type the invitations address. Type the Decision Panels address, then the
  new-panel address.
Expect: The dashboard reads "No stalled requests.", "No invitations expiring soon." and a Quick action, "Start an MSF
  campaign". The nav adds Data Rights Requests, MSF Campaigns, Committee Reviews, Decisions Due and Stalled Activities.
  It has no Invitations, and the invitations page shows Access denied (T178). It has no Decision Panels either, though
  that page admits him: it lists the panel with no New panel and no Edit column. The panel form shows Access denied.

--- from execution/knowledge/scenario-paediatrics/act-4-annual-review.md ---

### Step 4.1 — Mr Smit reads the decisions due
Role: Coordinator — Mr Pieter Smit
Route: / → /committee/decisions-due
Do: Open Decisions Due from the nav. Leave Period on `<P>` and Status on "Outstanding: not yet decided". Page through
  the list, then filter Status to "Not scheduled" and EPA to PAED-004.
Expect: The opening sentence names `<P>`, "5 trainees you oversee at Kgosi Kgari Teaching Hospital", and what Missed
  means. By EPA lists all 15 EPAs, each Due 5.
  - Twelve EPAs read To schedule 5.
  - PAED-008, 009 and 013 read Optional 5.
  The count reads "75 of 75 decisions due in `<P>` shown.", and the list pages 20 rows at a time.
  - Each semester EPA's row reads Not scheduled, window `<P>`, "No open review holds it, and `<P>` has not ended.".
  - An annual EPA's row reads the same with window `<Y>`.
  - The three optional EPAs read "As opportunity allows".
  Every Not scheduled row offers Schedule, named by trainee and EPA. The filters leave five PAED-004 rows, one per
  registrar; the By EPA card does not change. No Missed row exists.

### Step 4.2 — Prof Mbatha reads the same period
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /committee/decisions-due
Do: Open Decisions Due from the nav. Look for an Institution filter, then switch Period to the previous semester and
  back to `<P>`.
Expect: No Institution filter is offered, because she reads her own institution. The same five registrars and 75 rows
  as Step 4.1. For the previous semester, `<Y> S1`, the statuses follow that period's windows:
  - every registrar's six semester EPAs read Missed ("`<Y> S1` has ended with nothing decided, deferred or on an open
    review's agenda."). Mahlangu and Ndlovu started on 15 January, inside the semester's first month, so their windows
    count too (D42);
  - the annual EPAs read "Due by year end";
  - PAED-008, 009 and 013 read "As opportunity allows", since an optional EPA is never missed.

### Step 4.3 — Dr Mokoena reads what is due in her speciality
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/decisions-due
Do: Sign in, read her nav, and open Decisions Due.
Expect:
  - **Nav:** Programme Trainees, Decision Panels, Committee Reviews, STAR Review Queue and Decisions Due.
  - **The page:** the same five registrars, all in Paediatrics, and 75 rows, each Schedule link offered.
  - **Not in her nav or on her dashboard:** Entrustment Decisions.

### Step 4.4 — Dr Sithole reads what is due in his sub-speciality
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/decisions-due
Do: Sign in and open Decisions Due.
Expect: The same five registrars and 75 rows as Step 4.3: all five train in the Paediatrics sub-speciality he
  administers. His nav reads as Mokoena's.

### Step 4.5 — Dr Naidoo is not offered what is due
Role: CommitteeMember — Dr David Naidoo
Route: /committee/decisions-due → /access-denied
Do: Look for Decisions Due in the nav, then type its address.
Expect: His nav has Decision Panels and Committee Reviews but no Decisions Due. The typed address lands on the
  access-denied page: the page is for the roles that schedule reviews.

### Step 4.6 — Mr Smit schedules Dr Molefe's review
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee Reviews from the nav and choose Schedule review. Fill in:
  - Panel: `Paed Annual Review Panel`.
  - Trainee: Dr Molefe.
  - Period: `<P>`, and leave the evidence window as it fills.
  - Scheduled on: `D`.
  - Review type: Annual progression review.
  - Formative only: unticked.
  Read the agenda preview, then choose Create review.
Expect:
  - **Panel:** the one panel is offered. The Trainee select, empty until a panel is chosen, then lists the five
    registrars.
  - **Period:** labelled "`<P>` · 1 Jul to 31 Dec `<Y>`". Choosing it fills the window as `<Y>`-01-01 to `<Y>`-12-31.
  - **Review type:** only "Annual progression review" and "Pre-graduation review", with the help "The semester-2 sitting
    decides the trainee's progression for the year." (T131 slice 5).
  - **The preview** reads "15 EPAs will be on the agenda for `<P>`." It then says that PAED-001, 002, 003, 004, 005,
    006, 007, 010, 011, 012, 014 and 015 "must be decided at this sitting, or deferred with a reason, before it is
    ratified". It lists PAED-008, 009 and 013 as "As opportunity allows".
  - **After Create review:** the page opens the new review. Its Review card reads:
    - Trainee Lerato Molefe;
    - Panel Paed Annual Review Panel;
    - Sits for `<P>`;
    - Evidence window `<Y>`-01-01 to `<Y>`-12-31;
    - Type Annual progression review;
    - Mode Summative;
    - State Scheduled.
  - **Start review** is offered to Smit, since the coordinators of the panel's institution may start a review. He leaves
    it for the chair.
  - **Evidence snapshot:** "No evidence has been frozen yet. Starting the review captures the current bundle."

### Step 4.7 — Mr Smit schedules Dr Dlamini's and Dr du Plessis's reviews
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule the same kind of review for Dr Dlamini, then for Dr du Plessis: same panel, `<P>`, window as filled,
  Scheduled on `D`, Annual progression review.
Expect: Each opens as Scheduled with a 15-EPA preview like Step 4.6's. Committee Reviews lists three rows for `<P>`.
  Each row reads:
  - Period "`<P>` · `<Y>`-01-01 to `<Y>`-12-31";
  - Type Annual progression;
  - Mode Summative;
  - State Scheduled;
  - Decision Pending.
  Each row's Open is named by the review it opens.

### Step 4.8 — A second review of Dr Molefe for the same period is refused
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews
Do: Schedule another Annual progression review of Dr Molefe before the same panel for `<P>`.
Expect: Refused, and nothing is created. The refusal names the open review: "Review #N already puts this trainee before
  Paed Annual Review Panel for `<P>` (scheduled, scheduled `D`). A trainee has one binding review for each period …".
  The wording is a product decision (T131 slice 4). The list still holds three rows.

### Step 4.9 — Dr Mokoena schedules Dr Mahlangu's review from what is due
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: On Decisions Due, choose Schedule on Dr Mahlangu's PAED-001 row. Check the form it opens, set Scheduled on `D`, and
  create the review.
Expect:
  - **Decisions Due before she schedules:** Molefe's, Dlamini's and du Plessis's rows now read Scheduled, "On the agenda
    of review #N, which is still open.", each with Open review.
  - **The link opens the scheduling form already filled:** Panel `Paed Annual Review Panel`, Trainee Dr Mahlangu and
    Period `<P>`. The preview shows 15 EPAs. The query string fills only what the form offers her, so the link
    authorises nothing.
  - **The review opens as Scheduled,** and Start review is not offered to her. The Review card says: "Only the
    coordinators of the panel's institution, and those of its members who are active committee members there, can start
    this review." (T194)

### Step 4.10 — Dr Sithole schedules Dr Ndlovu's review from what is due
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: On Decisions Due, choose Schedule on one of Dr Ndlovu's rows. Set Scheduled on `D` and create the review.
Expect: As Step 4.9: the form is filled, the review opens as Scheduled, and Start is not offered. Decisions Due then
  reads Scheduled on all 75 rows, and By EPA reads Scheduled 5 and To schedule 0 for every EPA.

### Step 4.11 — Dr Naidoo lists the panel's reviews
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee Reviews, then open Dr Dlamini's review.
Expect:
  - **The list:** its subtitle reads "Open existing committee reviews.", and no Schedule review button is offered. It
    lists the five reviews of the panel he sits on, all Scheduled, with Decision Pending.
  - **Dlamini's review:** Start review is offered to him, as a member who may sit on the panel. He leaves it.

### Step 4.12 — Prof Mbatha checks the schedule
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/decisions-due
Do: Read Committee Reviews, then Decisions Due.
Expect: Committee Reviews offers Schedule review and lists all five reviews. Decisions Due reads Scheduled on all 75
  rows. Each row's "Open review" opens the review holding it, and no row offers Schedule.

### Step 4.13 — The panel cannot change what it decides while its reviews are open
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels → /committee/panels/{PanelId:int}
Do: Open the panel. In Decides for, choose the Neonatal team Clinical Competency Committee and save the committee.
Expect: Refused, and the panel stays a General panel. The refusal names the first open review: "Review #N (`<P>`,
  scheduled) still sits before this panel. What a review decides, and its agenda, follow the College committee the
  panel sat as when it was scheduled …" (T131 slice 5). The select's help text says the same rule before she tries.

### Step 4.33 — Mr Smit reads the ratified schedule
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Read Committee Reviews, then open Dr Molefe's review.
Expect: All five read Ratified. The Decision column reads:
  - Satisfactory Progress for Molefe and Dlamini;
  - Satisfactory with Observations for du Plessis;
  - Inadequate Progress — Additional Training for Mahlangu;
  - Outcome Deferred for Ndlovu.
  No state or category prints an enum name (T250). Molefe's review shows its decision, agenda and STAR numbers, and
  offers Smit no action.

### Step 4.34 — Prof Mbatha reads what is due after the sitting
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/decisions-due
Do: Read `<P>` with the default filter, then with "Every status".
Expect:
  - **Count, default filter:** "70 of 75 decisions due in `<P>` shown."
  - **Every status:**
    - Decided on five rows, each "STAR #n, issued at review #N.": Molefe's PAED-001, 010 and 012, and Dlamini's PAED-001
      and 002;
    - Deferred on 55 rows, "Deferred at review #N. A later sitting for `<P>` plans it again.", or `<Y>` for an annual
      EPA;
    - As opportunity allows on 15.
  - **By EPA:** PAED-001 reads Due 5, Decided 2 and Deferred 3.
  - **Actions:** a deferred row offers Open review and Schedule, since no open review holds its seat now.

### Step 4.37 — Dr Sithole reads the list and what must be decided again
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /admin/entrustment-decisions → /committee/decisions-due
Do: Type the entrustment decisions page's address. Filter Status to Revoked and apply, then to Active and apply.
  Download the revoked certificate. Then open Decisions Due with "Every status".
Expect:
  - **Filtered:** Revoked leaves Dlamini's PAED-002, and Active leaves four rows.
  - **The revoked certificate** reads Status Revoked, with "REVOKED", the date, who revoked it (Dr Mokoena, by name,
    T142) and the reason.
  - **Decisions Due:** Dlamini's PAED-002 reads "Revoked: re-decide". Its detail reads "STAR #n, issued for `<P>`, was
    revoked. Schedule a review to decide it again." It offers Schedule.
  - **By EPA:** PAED-002 reads Decided 0, Deferred 4 and To schedule 1.
  - **Count, default filter:** "71 of 75 decisions due in `<P>` shown."

### Step 4.49 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule a review of Dr Ndlovu before the panel for `<P>`, Scheduled on `D`, with Formative only ticked.
Expect:
  - **Accepted:** a formative review takes no binding seat for the period (T131 slice 4).
  - **The preview:** "A formative review carries no agenda: it decides no EPA."
  - **The review:** it opens with Mode "Formative (interim check-in, no binding decision)" and State Scheduled. It has
    no Decision, Agenda, Pending entrustment decisions or Appeals card.
  - **The list:** the review shows Mode Formative and Decision "—".

--- from execution/knowledge/scenario-paediatrics/act-5-graduation.md ---

### Step 5.1 — Mr Smit reads what Dr Molefe's committee still has to decide
Role: Coordinator — Mr Pieter Smit
Route: /committee/decisions-due
Do: Open Decisions Due for the period holding `D`, set the status filter to every status, and read Dr Molefe's rows.
Expect: There is one row for each EPA due for her in the period, and each row gives its status in words:
  - an EPA that an Act 4 STAR decided reads Decided, and names the STAR and the review that issued it;
  - an EPA that Act 4 deferred reads Deferred, and says that a later sitting plans it again;
  - the others read Not scheduled, Due by year end or As opportunity allows.
  Every row that is not yet decided offers Schedule. KGK's other registrars are listed too, and nobody outside KGK is.

### Step 5.2 — Mr Smit schedules Dr Molefe's pre-graduation review
Role: Coordinator — Mr Pieter Smit
Route: /committee/decisions-due → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Follow Schedule on one of Dr Molefe's outstanding rows. If Act 4 decided every EPA that is due, use Schedule review
  on Committee reviews instead. Then complete the form:
  - review type: Pre-graduation review;
  - evidence window: widened to start on her programme start, `J−3y`;
  - scheduled on: `D`;
  - Formative only: unticked.
  Create the review.
Expect: The form opens with the Paed Annual Review Panel, Dr Molefe and the period already filled in. The type offers
  Annual progression review and Pre-graduation review. Entrustment-only review is offered only in a semester-1 period.
  The agenda preview lists the EPAs that are due and names those Act 4 decided. In the same semester-2 period as Act 4:
  - it reads "12 EPAs will be on the agenda for …", with the period's label;
  - it names the nine EPAs Act 4 deferred as to be decided, or deferred with a reason, before the review is ratified;
  - it lists PAED-008, 009 and 013 as As opportunity allows;
  - it says "Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012."
  The new review reads Pre-graduation review, Summative and Scheduled, with the evidence window running from `J−3y` to
  the end of the period. Mr Smit is offered Start review, but leaves it to the chair.

### Step 5.29 — Neither ended programme can be put before the panel
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews
Do: Open Schedule review, choose the Paed Annual Review Panel and read the trainee list.
Expect: The list offers only Dr Dlamini, Dr Mahlangu and Dr Ndlovu. A graduate and a registrar who has withdrawn are not
  current trainees (T238). He closes the form without scheduling anything.

--- from execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md ---

### Step A.1.10 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Schedule a review of Dr Ndlovu before the Paed Annual Review Panel for the period holding `D`, Scheduled on `D`,
  with Formative only ticked. Note the review's id from its address.
Expect: The review opens with Mode "Formative (interim check-in, no binding decision)" and State Scheduled, as at Step
  4.49. It is open, so the erasure at A.1.12 has a review to end.

### Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions Due and the data-rights queue.
Expect: The campaign page keeps its gutter (T226), and its invitee table (counts by respondent group: the page never
  lists an address) scrolls within its card. Decisions Due's summary scrolls sideways, and a keyboard can scroll it, as
  a labelled, focusable region (DESIGN.md's decisions-due contract). The queue's filters stack above its table.

### Step A.7.8 — Dr Sithole on his phone
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: / → /committee/reviews → /committee/decisions-due → /placeholder/{Feature} → /placeholder/{Feature}
Do: At 390 px, open his dashboard, Committee Reviews and Decisions Due, then Programme Trainees and STAR Review Queue
  from the folded nav.
Expect: As for Dr Mokoena, scoped to his sub-speciality. His nav offers the same two "Coming soon" stubs as hers
  (A.5.12), and each fits the width.
```

### 2.2 After you pick, paste this

```
Take the panels (screens 1–2) and the schedule (screens 4–5) in variation <letter>, and Decisions Due in wireframe
<letter>, to full fidelity at 1280×800 and 390×844, in the design system you hold. If we chose one page for Decisions
Due and scheduling, draw it with the form open on a row. Draw every state under SCREENS, plus loading, 390 px and access
denied. For each artboard, list the components used and mark NEW ones. For each colour, name the token, or give a NEW
token and its value. Then check contrast (text 4.5:1; borders and the focus ring 3:1) and every target (at least 24 px).
```

## 3. The journey

One line per step: what the person does, and what they must be able to see. The step's full text is in § 2.1.

| Step | Page template | Who | Does, and must see |
|---|---|---|---|
| 2.20 | `/committee/panels` | Prof Mbatha | No panels yet: "No decision panels", New panel, and "who decides" saying no panel covers the programme. |
| 2.21 | `/committee/panels/new` | Prof Mbatha | The form's options (Scope, Speciality, Decides for, Chair, Members, External members), the chair note, and a panel of one refused (D46). |
| 2.22 | `/committee/panels/new` → `/committee/panels/{PanelId:int}` | Prof Mbatha | Saves. The panel's page shows "Decides for" and "Update members", but never the panel's name (F-2.22a, T260). |
| 2.23 | `/account/login` → `/` → `/committee/panels` → `/committee/panels/{PanelId:int}` → `/committee/panels` | Dr Mokoena | Adds Dr Botha. Decides for is read-only for her; "Panel members updated."; Members reads 4. |
| 2.24 | `/committee/panels` | Prof Mbatha | One panel, and "who decides": PAED-004 and 005 fall back to the general panel with the College's reason; the other thirteen go to it. |
| 2.25 | `/account/login` → `/` → `/committee/panels` → `/committee/panels/new` | Dr Sithole | May create Speciality-scoped panels only, with no Decides for field. Leaves without saving. |
| 2.32 | `/account/login` → `/` → `/admin/invitations` → `/access-denied` → `/committee/panels` → `/committee/panels/new` → `/access-denied` | Mr Smit | His nav has no Decision Panels. Typed, the list opens (no New, no Edit); the new-panel form is access denied. |
| 4.1 | `/` → `/committee/decisions-due` | Mr Smit | Outstanding for the period: the opening sentence, By EPA (12 To schedule, 3 Optional), "75 of 75", 20 a page, Schedule named per row; filters to five PAED-004 rows. |
| 4.2 | `/` → `/committee/decisions-due` | Prof Mbatha | No Institution filter for her. The previous semester shows Missed rows; optional EPAs are never missed. |
| 4.3 | `/account/login` → `/` → `/committee/decisions-due` | Dr Mokoena | The same 75 rows. Entrustment Decisions is in neither her nav nor her dashboard. |
| 4.4 | `/account/login` → `/` → `/committee/decisions-due` | Dr Sithole | The same, for his sub-speciality. |
| 4.5 | `/committee/decisions-due` → `/access-denied` | Dr Naidoo | Not in his nav; typed, access denied. |
| 4.6 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | Schedules Dr Molefe: the form's options, the period filling the window, the agenda preview (15 EPAs, 12 to decide), then the review, Scheduled, Start offered to him. |
| 4.7 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | Schedules two more; the list reads three rows, each Open named for its review. |
| 4.8 | `/committee/reviews` | Mr Smit | A second review of Dr Molefe for the period: the preview shows no warning, and Create review is refused with the review it collides with (T309). |
| 4.9 | `/committee/decisions-due` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Mokoena | Decisions Due shows three registrars Scheduled. Schedule on Dr Mahlangu's row opens the form filled. The review opens Scheduled, with no Start for her. |
| 4.10 | `/committee/decisions-due` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Sithole | Schedules Dr Ndlovu the same way; then all 75 rows read Scheduled. |
| 4.11 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Naidoo | The five reviews of his panel, no Schedule review; Start is offered to him as a seated member. |
| 4.12 | `/committee/reviews` → `/committee/decisions-due` | Prof Mbatha | All five scheduled; every row's Open review opens the review holding it. |
| 4.13 | `/committee/panels` → `/committee/panels/{PanelId:int}` | Prof Mbatha | Changing what the panel decides is refused while its reviews are open. The select keeps the refused choice until a reload (observed in the step's Actual). |
| 4.33 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | After the sitting: all five Ratified, each Decision by its label, never an enum name (T250). |
| 4.34 | `/committee/decisions-due` | Prof Mbatha | After the sitting: 70 outstanding; Every status shows Decided (with STAR and review), Deferred and optional rows. |
| 4.37 | `/admin/entrustment-decisions` → `/committee/decisions-due` | Dr Sithole | After a revocation (the list is F09's): "Revoked: re-decide", with Schedule. |
| 4.49 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | A formative check-in: no agenda in the preview, and no Decision or Agenda card on the review. It is typed "Annual progression" (T309). |
| 5.1 | `/committee/decisions-due` | Mr Smit | Dr Molefe's fifteen rows in words: Decided, Deferred, As opportunity allows. |
| 5.2 | `/committee/decisions-due` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | Schedules her pre-graduation review from a row: the form filled, the window widened to her start, the preview naming what is already decided. |
| 5.29 | `/committee/reviews` | Mr Smit | Only current registrars are offered; a graduate and a withdrawn registrar are not. |
| A.1.10 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | Another formative check-in, for the data-rights appendix. |
| A.7.5 | `/` → `/msf/campaigns` → `/msf/campaigns/{CampaignId:int}` → `/committee/decisions-due` → `/admin/data-rights` | Mr Smit | At 390 px, Decisions Due's summary is a focusable region, 685 px wide in 291 px, with the browser's focus ring (T328). |
| A.7.8 | `/` → `/committee/reviews` → `/committee/decisions-due` → `/placeholder/{Feature}` → `/placeholder/{Feature}` | Dr Sithole | At 390 px, Committee Reviews' table (902 px) scrolls in its container; Decisions Due as above. |

## 4. States to design

From `execution/knowledge/scenario-paediatrics/states.md` § Committee. Each file is
`design/baseline/states/<page>--<state>.png`.

| Page | State | Capture | Reached at |
|---|---|---|---|
| `/committee/panels` | No panels | `panels-list--empty.png` | Step 2.20 |
| `/committee/panels` | The manager's list | `panels-list--manager.png` | Step 2.24, with the routing card |
| `/committee/panels` | A speciality admin's list | `panels-list--speciality-admin.png` | Step 2.23 |
| `/committee/panels` | A member's list | `panels-list--member.png` | Step 4.14 |
| `/committee/panels` | A coordinator's list | `panels-list--coordinator.png` | Step 2.32 |
| `/committee/panels` | Loading | `panels-list--loading.png` | A held read: "no committee routing" shown too soon (T329) |
| `/committee/panels` | 390 px | `panels-list--narrow.png` | Step A.7.7 |
| `/committee/panels/new` | The new-panel form | `panel-edit--new.png` | Step 2.21 |
| `/committee/panels/new` | Chair moved out of Members | `panel-edit--chair-note.png` | Step 2.21 |
| `/committee/panels/new` | A panel of one refused | `panel-edit--one-member.png` | Step 2.21 |
| `/committee/panels/{PanelId:int}` | Saved | `panel-edit--saved.png` | Step 2.22 |
| `/committee/panels/{PanelId:int}` | Read-only committee | `panel-edit--speciality-admin.png` | Step 2.23 |
| `/committee/panels/new` | Speciality scope only | `panel-edit--sub-speciality-admin.png` | Step 2.25 |
| `/committee/panels/{PanelId:int}` | Committee change refused | `panel-edit--body-refused.png` | Step 4.13 |
| `/committee/panels/{PanelId:int}` | A member who can no longer sit | `panel-edit--unseatable.png` | Scratch (post-appendix): Dr Botha locked out |
| `/committee/panels/{PanelId:int}` | Not found | `panel-edit--not-found.png` | Typed `/committee/panels/999999` |
| `/committee/panels/{PanelId:int}` | 390 px | `panel-edit--narrow.png` | Step 2.22 |
| `/committee/decisions-due` | Nothing due | `decisions-due--nothing-due.png` | After Step 1.22, before any admission |
| `/committee/decisions-due` | An administrator, no institution | `decisions-due--choose-institution.png` | devadmin, any time after Act 1 |
| `/committee/decisions-due` | Outstanding | `decisions-due--outstanding.png` | Step 4.1 |
| `/committee/decisions-due` | No row matches | `decisions-due--no-match.png` | Step 4.1, Status Missed |
| `/committee/decisions-due` | A past semester, missed | `decisions-due--missed.png` | Step 4.2 |
| `/committee/decisions-due` | All scheduled | `decisions-due--scheduled.png` | Step 4.10 |
| `/committee/decisions-due` | After the sitting | `decisions-due--after-sitting.png` | Step 4.34, Every status |
| `/committee/decisions-due` | Revoked, to decide again | `decisions-due--revoked.png` | Step 4.37 |
| `/committee/decisions-due` | Loading | `decisions-due--loading.png` | A held read |
| `/committee/decisions-due` | 390 px | `decisions-due--narrow.png` | Step A.7.5 |
| `/committee/reviews` | None yet, a scheduler | `reviews-schedule--empty-scheduler.png` | Step 4.1, before 4.6 |
| `/committee/reviews` | None yet, a member | `reviews-schedule--empty-member.png` | Step 4.5 |
| `/committee/reviews` | The form, no panel chosen | `reviews-schedule--form.png` | Step 4.6 |
| `/committee/reviews` | The agenda preview | `reviews-schedule--preview.png` | Step 4.6 |
| `/committee/reviews` | A second review refused | `reviews-schedule--refused.png` | Step 4.8 |
| `/committee/reviews` | Scheduled | `reviews-schedule--scheduled.png` | Step 4.7 |
| `/committee/reviews` | Filled from Decisions Due | `reviews-schedule--prefilled.png` | Step 4.9 |
| `/committee/reviews` | Formative preview | `reviews-schedule--formative.png` | Step 4.49 |
| `/committee/reviews` | Already decided in this window | `reviews-schedule--pre-graduation.png` | Step 5.2 |
| `/committee/reviews` | Ratified | `reviews-schedule--ratified.png` | Step 4.33 |
| `/committee/reviews` | Preview failed | `reviews-schedule--preview-failed.png` | A held read during Step 4.6 |
| `/committee/reviews` | Loading | `reviews-schedule--loading.png` | A held read |
| `/committee/reviews` | 390 px | `reviews-schedule--narrow.png` | Step A.7.8 |
| `/committee/reviews/{ReviewId:int}` | Scheduled, no Start | `review-detail--scheduled-no-start.png` | Step 4.9 |
| `/committee/reviews/{ReviewId:int}` | Formative, scheduled | `review-detail--formative.png` | Step 4.49 |

**Not captured, to be drawn from their description** (`states.md:779-781`; `coverage.md` § Flows and states not
played):
- The "holds Trainee" notes on the panel list and form and on Committee Reviews (T237, T256). No cast member holds
  Trainee beside a staff role.
- The panel form's "You cannot create a decision panel" card (T194). Every panel manager in the cast has a scope to
  create in.
- The Administrator on Decisions Due with an institution chosen (D46). The Administrator acts only in Acts 1 and 6 and
  the appendix.
- A second panel sitting as the neonatal committee, and the agenda's "Decided by another panel". These are captured on
  a scratch database for F07 (`review-detail--entrustment-only`, `--decided-elsewhere`).

**Also on disk, placed elsewhere:** `reviews-schedule--current-only.png` (Step 5.29, current trainees only) and
`reviews-schedule--pseudonym.png` (after an erasure, F14's). Attach the first if the chat asks about the trainee picker.

## 5. Attach

Every path below is under `design/baseline/` and was checked with `ls` on 2026-09-26. Open each before you upload it
(BRIEF § 3.3). None of them is an invitation page. `act-2/2.32-2-invitations-denied.png` shows the access-denied page,
not the invitations list.

### 5.1 Key screenshots (attach with the ask)

1. `states/panel-edit--chair-note.png`: the panel form, with the chair note and the multi-selects.
2. `states/panels-list--manager.png`: the list and "Who decides each EPA".
3. `act-2/2.24-1-who-decides.png`: the routing card from the story.
4. `states/decisions-due--outstanding.png`: Decisions Due, outstanding (3,971 px tall at 1280).
5. `states/decisions-due--missed.png`: a past semester with Missed rows.
6. `states/reviews-schedule--preview.png`: the form and the agenda preview.
7. `states/reviews-schedule--refused.png`: the duplicate refused after Create.
8. `states/reviews-schedule--prefilled.png`: the form filled from a Decisions Due row.
9. `act-A/A.7.5-3-decisions-due-summary-scrolled.png`: the summary region at 390 px, with the browser's focus ring.
10. `states/panels-list--loading.png`: "no committee routing" shown while loading.

### 5.2 State captures (add as the chat asks)

- Panels:
  - `states/panels-list--empty.png`
  - `states/panels-list--speciality-admin.png`
  - `states/panels-list--member.png`
  - `states/panels-list--coordinator.png`
  - `states/panels-list--narrow.png`
  - `states/panel-edit--new.png`
  - `states/panel-edit--one-member.png`
  - `states/panel-edit--saved.png`
  - `states/panel-edit--speciality-admin.png`
  - `states/panel-edit--sub-speciality-admin.png`
  - `states/panel-edit--body-refused.png`
  - `states/panel-edit--unseatable.png`
  - `states/panel-edit--not-found.png`
  - `states/panel-edit--narrow.png`
- Decisions Due:
  - `states/decisions-due--nothing-due.png`
  - `states/decisions-due--choose-institution.png`
  - `states/decisions-due--no-match.png`
  - `states/decisions-due--scheduled.png`
  - `states/decisions-due--after-sitting.png`
  - `states/decisions-due--revoked.png`
  - `states/decisions-due--loading.png`
  - `states/decisions-due--narrow.png`
- Schedule:
  - `states/reviews-schedule--empty-scheduler.png`
  - `states/reviews-schedule--empty-member.png`
  - `states/reviews-schedule--form.png`
  - `states/reviews-schedule--scheduled.png`
  - `states/reviews-schedule--formative.png`
  - `states/reviews-schedule--pre-graduation.png`
  - `states/reviews-schedule--ratified.png`
  - `states/reviews-schedule--preview-failed.png`
  - `states/reviews-schedule--loading.png`
  - `states/reviews-schedule--narrow.png`
  - `states/reviews-schedule--current-only.png`
- Review page:
  - `states/review-detail--scheduled-no-start.png`
  - `states/review-detail--formative.png`
- Step captures for detail:
  - `act-2/2.22-1-panel-saved.png`
  - `act-2/2.25-2-sithole-new-panel.png`
  - `act-2/2.32-3-smit-panels.png`
  - `act-2/2.32-4-new-panel-denied.png`
  - `act-4/4.5-1-naidoo-decisions-due-denied.png`
  - `act-4/4.8-1-smit-second-review-refused.png`
  - `act-4/4.13-1-mbatha-committee-change-refused.png`
  - `act-4/4.34-1-mbatha-decisions-due-every-status.png`
  - `act-4/4.37-1-sithole-revoked-re-decide.png`
  - `act-4/4.49-1-smit-formative-scheduled-list.png`
  - `act-5/5.2-1-smit-pre-graduation-form.png`
  - `act-A/A.7.8-2-sithole-reviews.png`
  - `act-A/A.7.8-3-sithole-decisions-due.png`

### 5.3 Held

None for this flow's pages. Do not use `act-2/2.32-1-smit-home.png` or `act-4/4.3-1-mokoena-dashboard-nav.png`: they are
F06's dashboards, held by T297.

## 6. Known problems this design must solve

| Task | Problem today | What it means for the design | Evidence |
|---|---|---|---|
| T309 (P3) | The preview runs only the scope check and the agenda planner (`PreviewCommitteeAgenda.cs`). The seat check is private to the create (`ScheduleCommitteeReview.cs:205-222`). The Review type select stays live while "Formative only" is ticked (`ReviewsSchedule.razor:74-90`). | The preview warns first, in the create's own words (`AlreadyScheduled`), with Open review. Create review is disabled with that reason (the T107 pattern). A formative review reads "Formative check-in" (short "Formative") on every surface, and the type select hides. | `states/reviews-schedule--refused.png`, `states/review-detail--formative.png` |
| T259 (P3) | A "Revoked: re-decide" row names the panel the EPA routes to (`SchedulePanelName`), not the panel the holding review sits before. | The status sentence names the holding review's own panel. | `states/decisions-due--revoked.png` |
| T260 (P3) | After a panel save, the "can no longer sit" warning stays until a reload. The panel's page never names the panel, its scope or its speciality (F-2.22a). The revocation form's button (on `/admin/entrustment-decisions`, F09's) enables only when the reason loses focus. | Re-read after save. The panel's page header names the panel. On F09's revocation form, the button enables as the reason is typed. | `states/panel-edit--unseatable.png`, `act-2/2.22-1-panel-saved.png` |
| T329 (P2; BRIEF A6) | `PanelsList.razor:184` starts `_routingLoading` false, so "There is no committee routing to show for your institution." shows while the list loads. | The routing card shows a skeleton from the first render. Every page here: header first, skeleton, then content or an alert. | `states/panels-list--loading.png` |
| T328 (P3; BRIEF A4) | The By EPA region (`DecisionsDue.razor:79`, `tabindex="0" role="region"`) has no `:focus-visible` rule. | The design's focus ring on a focusable region. | `act-A/A.7.5-3-decisions-due-summary-scrolled.png` |
| T264 (P3; BRIEF A5) | Pagers drop the focus: `PagerControls`' Next and Previous disable themselves at either end. Some row actions share a name. Decisions Due's Schedule and Open review links are already named per row (Steps 4.1, 4.12). | Keep the pager mounted and the focus on the pressed control or the list heading. Every row action is named for its row. | Step 4.1 |
| T280 (P3; BRIEF A14) | "Per page:" is loose text, not a label, so the page-size select has no name (F-4.1a). It is the shared pager, on every paged list. | The pager's select is labelled. | Step 4.1 |
| Reached only by address (`coverage.md:160`) | `/committee/panels` admits the Coordinator; his nav has no Decision Panels (DESIGN.md:199). | Give him the link, or record why not. A nav change is a DESIGN.md change (`NavMenuAuthorizationTests` parses the table). | `act-2/2.32-3-smit-panels.png` |
| Observed, not filed | The panel form is one narrow card at 1280 px. Members and External members are native multi-selects ("Hold Ctrl (Windows) / Cmd (Mac) to select multiple") that clip "Sarah Botha (botha@kgk.wombat.loc". | Choose members in a control that shows whole names and works by touch at 390 px. `Design/MultiSelectOptionKeyTests` pins today's multi-select. | `states/panel-edit--chair-note.png` |
| Observed, not filed | After a refused committee change, the Decides for select keeps the unsaved choice until a reload (Step 4.13's Actual). | After a refusal, show the saved value, or mark the field as not saved (BRIEF § 6 A5). | `states/panel-edit--body-refused.png` |
| Observed, not filed | One semester, two labels. The schedule form reads "2026 S2 · 1 Jul to 31 Dec 2026" (DESIGN.md:1412-1414). The registrar's pages read "Semester 2, 2026 · July to November", the College's nominal end (D40; F05). | Decide whether both labels stay, and say why they differ where both appear. | `states/reviews-schedule--preview.png` |

## 7. Questions the design must answer

1. **Should Decisions Due and the schedule be one page, or two linked pages?** One page would show what is due and
   schedule it in place. Today Schedule on a row opens `/committee/reviews?panel=…&trainee=…&period=…`, already filled
   (Step 4.9; DESIGN.md:1576-1610).
2. **How does a scheduler see, before saving, that the registrar already has a binding review this period?** Today the
   refusal comes only on Create (Step 4.8; T309).
3. **Should the Coordinator get a Decision Panels link?** The page admits him; his nav has none (`coverage.md` § Reached
   only by address).

## 8. Acceptance

Claude Code builds the chosen design as one task per page, or one for Decisions Due and the schedule if they merge
(BRIEF § 9). It is done when all of the following hold.

**Tests.**
- `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green. Never use `--no-build` (CLAUDE.md § Testing).
- The run includes the tests that pin these pages:
  - `CommitteeDecisions/DecisionsDuePageTests`;
  - `PanelDecisionBodyTests`, `PanelEdit*Tests` and `PanelPagesTraineeFirstTests`;
  - `ReviewsSchedule*Tests` (AgendaPreview, ListScope, PanelList, Prefill, TraineeFirst, TraineePicker);
  - `CommitteeReviewStateLabelTests`;
  - `Navigation/NavMenuAuthorizationTests`, if the Coordinator gains a link;
  - `Accessibility/RowNamesTests` and `Design/*`, including `MultiSelectOptionKeyTests`;
  - the scenario guard in `Scenario/`.

**The replay.**
- Replay the flow's steps on a fresh database (`tools/scenario-replay.ps1`, BRIEF § 9):
  - 2.20–2.25 and 2.32 (Act 2);
  - 4.1–4.13, 4.33, 4.34, 4.37 and 4.49 (Act 4);
  - 5.1, 5.2 and 5.29 (Act 5);
  - A.1.10, A.7.5 and A.7.8 (appendix).
- Each act may instead start from its predecessor's snapshot, `recovery/scenario-post-act<N>.dump` (README § How to
  play, "Snapshots"). Every Expect must hold, the refusals' exact words included.
- **Update the steps the design changes** (BRIEF § 9, item 7):
  - Step 4.8's preview now warns before Create.
  - Step 4.49's review reads "Formative check-in".
  - Step 2.32 changes if the Coordinator gains Decision Panels.
  - If Decisions Due and the schedule merge, the Route lines of 4.9, 4.10 and 5.2 change.

**Checks the design adds.**
- Step 4.8: the preview names review #1 and offers Open review, and Create review is disabled with that reason (T309).
- Step 4.49: the review and the list read "Formative check-in", and the form shows no Review type while "Formative only"
  is ticked (T309).
- Step 2.22: the saved panel's page names the panel, its scope and its speciality (T260).
- The panels list, loaded with a held read: a skeleton, never "no committee routing" (T329).
- Steps A.7.5 and A.7.8 at 390 px:
  - the By EPA region shows the design's focus ring (T328);
  - no page scrolls sideways;
  - the pager keeps the focus (T264) and its select is named (T280).

**Re-capture.** Take the 42 state captures in § 4 and this flow's step captures again (`states.md` § How to capture).
Compare them with the chosen artboards.

**Browser check.** Check as each of the five roles at 1280 px and at 390 px: the InstitutionalAdmin, the SpecialityAdmin,
the SubSpecialityAdmin, the Coordinator and a CommitteeMember.
