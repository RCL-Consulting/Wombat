# F07 The committee sits: the chair stages STARs, defers, records with a quorum, and ratifies

The panel's chair reads a registrar's evidence and standing, stages STARs (Statements of Awarded Responsibility),
defers what the panel will not decide, records the decision with a quorum, and ratifies. Members follow without the
chair's controls. STARs are "immutable once issued" (`execution/knowledge/DOMAIN.md:78`); this is the highest-stakes page
in the product.

Written 2026-09-26 for T332, following `design/BRIEF.md` § 2.4. Paths to screenshots are under `design/baseline/`
(gitignored). Claims about Claude Design use BRIEF § 2.0's source keys.

| | |
|---|---|
| People | CommitteeMember: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha. InstitutionalAdmin: Prof Nolwazi Mbatha, who reads a decided review (Step 4.26). The registrars under review: Dr Molefe, Dr Dlamini, Dr du Plessis, Dr Mahlangu, Dr Ndlovu (`README.md` § Cast) |
| Pages | `/committee/reviews/{ReviewId:int}` (`CommitteeDecisions/ReviewDetail.razor`, 2,093 lines), `/activities/{ActivityId:int}` (read-only, from the snapshot), `/committee/reviews` (the list, to open one), `/committee/panels` (who sits) (`coverage.md` § Pages) |
| Steps | 26: 4.14–4.32, 4.50, 5.3–5.6, A.7.6, A.7.13. Forty steps touch the review page in all (`coverage.md:80`); the others are F08's (scheduling) and F09's (appeals). |
| Frequency and stakes | Each semester or academic year for each registrar, by the EPA's decision cadence (`act-1-setup.md:382-396`). The highest stakes in the product: a STAR is a medico-legal record (`DOMAIN.md:78`). |
| Mode | **Wireframe first, in three parts:** before the sitting; staging; record and ratify. Then fidelity. This is the densest page: 38 state captures (`design/baseline/states/review-detail--*`, from `states.md` § Committee). |
| Held | **Nothing held.** No group-1 task changes this flow's states. T307 (group 1) changed the page's Appeals card; it landed in d03732d and those states (`review-detail--appeal-*`) were re-captured on 2026-09-26. They are F09's: do not use them here (BRIEF § 10). |

---

## 1. How to run this thread

1. Run F01 first (BRIEF § 2.3, step 2). F05 designs the trajectory chart; reuse its answer here rather than designing a
   second chart.
2. Start a new thread. Attach the key screenshots in § 5.1, then paste § 2.1 as one message.
3. Pick a wireframe for each of the three parts. Decide in the chat, one sentence each:
   - one page or steps;
   - what stays on screen while staging;
   - how Ratify is confirmed.
   Then paste § 2.2 for fidelity.
4. Attach the state captures in § 5.2 as the chat asks for them.
5. Ask for an accessibility review [start], with BRIEF § 6 A1 as the check. This page is walked by screen reader in Step
   A.7.13.
6. Export the chosen artboards to `design/flows/07-review-sitting/` before moving on. There is no version history
   [start].

## 2. The ask

### 2.1 Paste this first, with the key screenshots attached

```
FLOW 07 — The committee sits: stage STARs, defer, record with a quorum, ratify

GOAL: The panel's chair runs one registrar's review.
- She reads the frozen evidence, the registrar's standing against the College's levels, MSF coverage and rating
  trajectories.
- She stages STARs: an EPA, the level the registrar is entrusted at, the issue date, an optional expiry, a rationale,
  and the evidence it rests on.
- She defers each other EPA that must be decided at this sitting, with a reason.
- She records the committee's decision: a progression category, a rationale, conditions, and who was present. At least
  two members must be present, the chair and one other.
- She ratifies. That issues every staged STAR at once, and nothing on the review can change afterwards.
Other members follow the same page without the chair's controls. An administrator reads it and cannot ratify.

What is wrong today:
- It is one very long page with no structure for the sitting: 7,100 px tall before Start, 9,100 px with three staged
  decisions and 12,700 px with thirteen, at 1280 px. At 390 px it is 12,900 px.
- The Decision form (Record decision) sits at the top, beside the Review card. The Agenda (Stage and Defer per line)
  is below the standing and MSF tables. The staging form is below the Agenda, in a narrow column beside the evidence.
- Every action's result appears at the very top of the page and takes the focus there, away from where the chair is
  working.
- Ratify, Remove and Close review ask for no confirmation.
- While the review loads, the page shows its header and blank space.
- At 390 px the trajectory charts' labels are about 5 px tall.
- The heading reads "Committee review" and never names the registrar.

AUDIENCE:
- CommitteeMember: Dr Thandi Zulu (chair, senior paediatric consultant); members Dr David Naidoo and Dr Sarah Botha;
  Dr John van Rensburg (external examiner).
- InstitutionalAdmin: Prof Nolwazi Mbatha (head of department), who reads only.
- The registrars reviewed: Dr Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr
  Nomsa Mahlangu and Dr Sipho Ndlovu (1).
- A sitting is a meeting: the chair drives the page, and members open the same review on their own screens while it
  is in progress. Desktop 1280×800 and phone 390×844.

THE PAGE'S PARTS TODAY, top to bottom:
- Result alert: "Pending entrustment decision staged."
- "Sampling concentration warnings", a dismissible warning: EPAs whose evidence comes from fewer than three distinct
  assessors, with "1 rating · 1 assessor · 1 source", and a note on records not counted (MSF has no named assessor).
- "Review": Trainee, Panel, Sits for (2026 S2), Evidence window (2026-01-01 to 2026-12-31), Type (Annual progression
  review, Pre-graduation review, Entrustment-only review), Mode (Summative or Formative), State.
- "Decision", beside Review: Category, Rationale, Conditions, Present (a checkbox per panel member, the chair's ticked
  and locked), and Record decision with its reason when disabled. Once recorded, it shows the category as a heading,
  the rationale, "Conditions: …" and "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg
  (external)".
- "Entrustment against Annexure A", full width: the training year; "2 at or above · 1 below · 12 with no decision, of
  15 EPAs"; the exit rule "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)", "For
  information only"; then a table per EPA (year target, STAR decision, against target, exit level, latest rating).
- "Multi-source feedback by EPA", full width: an EPA-by-semester table, each cell "Covered" or "None released".
- "Agenda", full width: its caption counts "15 EPAs for 2026 S2, 12 still to stage or defer". Its table has EPA,
  Window, State, Evidence (n items) and, for the chair only, Action.
  - A state is Due ("Must be decided at this sitting, or deferred with a reason."), As opportunity allows (optional),
    Staged, Deferred (with its reason and Reinstate), Decided ("STAR #n."), Not decided, or Decided elsewhere.
  - Warnings may name an EPA "no longer decided in its window" (its STAR was revoked mid-review), or EPAs decided by
    another panel.
- "Pending entrustment decisions": the staging form. EPA; Authorised level (the EPA's ladder: 1, 2, 3a, 3b, 4, 5 under
  "CPSA Paediatric Entrustment Scale v11.1"); Issued on (today by default); Expires on (optional); Rationale; and "Evidence
  it rests on", ticks over the snapshot's lines with that EPA's own lines first. Below the form is the list of staged
  decisions, each "PAED-010 … 4 · issued 2026-09-26 · expires 2026-10-16" with "Rests on 2 items of the snapshot: …"
  and a Remove.
- "Evidence snapshot", beside the staging form: frozen at Start, grouped by EPA then by instrument. Each line links to
  its activity and shows its rating, encounter date and state. A feedback campaign sits under "Not about a single EPA".
- "Rating trajectory by EPA": a small SVG chart per EPA on the six rungs.
- "Appeals" (designed in Flow 09).

RULES THE DESIGN MUST SHOW:
- Only the chair is offered: the staging form, Remove, the agenda's Stage, Defer and Reinstate, the decision form,
  Record decision, Ratify and Close review. Everyone else reads four agenda columns and a sentence: "Only the panel's
  chair, Thandi Zulu, can stage entrustment decisions, defer agenda lines and record the committee's decision." No
  administrator can bypass this.
- Record decision is disabled, with its reason naming the lines, until every Due line is staged or deferred. Recording
  "fixes the entrustment decisions staged below and the agenda's deferrals: they cannot be changed afterwards."
- A progression review must record a category: Satisfactory Progress, Satisfactory with Observations, Inadequate
  Progress — Additional Training, Inadequate Progress — Repeat, Release from Training, Outcome Deferred, or Graduate
  (programme complete). An entrustment-only review records none.
- Refusals, each with its exact words:
  - "An expiry date must be after the issue date."
  - "An entrustment decision on PAED-001 is already staged at this review. Remove it to stage another."
  - "Say why the committee is deferring the decision."
  - "A committee decision needs at least two panel members present: the chair and at least one other."
  - "This review decides the trainee's progression, so its decision records a progression category. Choose one."
  A refusal keeps everything typed.
- Hints that refuse nothing: "Only one item is named. The College's rule is that an entrustment decision is never
  taken from a single form." and "None of the named items is about PAED-002."
- A formative check-in has no Decision, Agenda, Pending or Appeals card. The chair closes it ("Close review").

DATA VOLUMES: none (a snapshot of 0 items; "No rating by a named assessor to chart for this trainee"); typical (8
snapshot lines, 3 staged, 9 deferred, 3 optional); heavy (a pre-graduation review with 13 staged decisions, and 15
agenda lines).

MODE: Wireframe first, in three parts, then full fidelity for the variations I pick.

SCREENS, in order:
1. Part 1, before the sitting: a scheduled review (Start offered) and one in progress, with the registrar's evidence
   (the sampling warnings and the snapshot), standing, trajectory and MSF. Include opening one piece of evidence from
   the snapshot (an activity, read-only) and coming back without losing her place.
2. Part 2, staging:
   - stage a STAR (EPA, level, issue date, expiry, rationale, evidence);
   - the refusals (expiry, duplicate);
   - defer with a reason, and the empty reason refused;
   - Reinstate;
   - ready to record;
   - a decision resting on a single item;
   - thirteen staged lines.
3. Part 3, record and ratify:
   - record a category with a quorum, and the quorum and category refusals;
   - decided, as the chair (Ratify offered) and as an administrator (no Ratify);
   - ratified with STARs issued, and ratified with no STAR;
   - a staged decision about an EPA with no evidence of its own.
4. The member's view (no chair controls), and the variants: formative in progress and closed, pre-graduation (exit
   rule), and Graduate decided and ratified.
5. Loading, not found, 390 px.

STEPS: 4.14–4.32, 4.50, 5.3–5.6, A.7.6 and A.7.13, pasted verbatim at the end of this message (Role / Route / Do /
Expect). In them, D is the day the story is replayed, <P> the semester holding D (for example 2026 S2) and <Y> its
year; J is the latest 15 January on or before D.

STATES TO SHOW: scheduled; in progress; one evidence item named; staged; expiry refused; duplicate refused; deferral
with no reason; ready to record; a member during the sitting; no quorum; no category; decided (chair); decided (an
administrator); ratified with STARs; a staged decision with no evidence about its EPA; ratified with no STAR; formative
in progress; formative closed; pre-graduation; Graduate recorded; Graduate ratified (every EPA at its exit level); the
chair can no longer act; an agenda line no longer decided; Ratify blocked; an entrustment-only review; decided by
another panel; loading; not found ("The committee review could not be found among the reviews you can view."); 390 px.

REQUIREMENTS FROM KNOWN DEFECTS (each is testable):
- T329: the header renders from the first moment and a skeleton shows while the review loads. No action is offered
  before it has loaded.
- T323: trajectory text is at least 11 px at 390 px, or the chart's table is visible under it.
- T273: the sampling list and the staged lines mark an EPA that is no longer in force, "(no longer in use)". The
  snapshot's headings stay as they were when frozen.
- T325: "Issued on" defaults to today on the South African calendar.
- T260: a warning about who can no longer sit is never left on screen after it stops being true.
- T280: the warnings' dismiss button has a name ("Dismiss", not "×"). Every card is a named region.
- T322: badge text meets 4.5:1 contrast.
- T309: a formative review reads "Formative check-in", never "Annual progression review".
- A5 (from BRIEF § 6): every action reports its outcome where the focus goes. A refused action keeps what was typed.
  Irreversible actions confirm and name their target.

QUESTIONS THE DESIGN MUST ANSWER:
1. One long page, or steps (evidence, stage, record, ratify)? How does the chair keep her place across 13 staged
   lines?
2. What stays on screen while the chair stages: the registrar's standing and shortfalls, or the evidence list?
3. How does the page show who is present and whether the quorum is met, before the chair presses Record?
4. How are irreversible acts (Record, which fixes what is staged; Ratify, which issues STARs) marked and confirmed?

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
Also: the page has exactly one h1. The trajectory chart is the hand-drawn SVG TrajectoryChart shared with My progress
(Flow 05); it needs a text or table equivalent. A confirmation is the native <dialog> ConfirmDialog.

ASK: 2–3 variations for each part. Wireframes first, then full fidelity. Name every design-system component you use
and mark anything else NEW. Say which DESIGN.md rule a variation breaks. Flag edge cases and draw the none, typical
and heavy data volumes. Review the result for accessibility against WCAG 2.1 AA: headings in order, every card a named
region, every row action named for its row.

ATTACHED: states/review-detail--scheduled.png, act-4/4.17-1-zulu-minicex-from-snapshot.png,
states/review-detail--staged.png, states/review-detail--ready-to-record.png, states/review-detail--quorum-refused.png,
states/review-detail--ratified.png, act-5/5.3-2-zulu-standing-exit-rule.png, act-5/5.4-4-zulu-thirteen-staged-full.png,
states/review-detail--member.png, act-A/A.7.13-1-zulu-review7-top.png, states/review-detail--narrow.png

RUNBOOK STEPS, VERBATIM:

--- from execution/knowledge/scenario-paediatrics/act-4-annual-review.md ---

### Step 4.14 — Dr Botha checks who sits and who decides
Role: CommitteeMember — Dr Sarah Botha
Route: /committee/panels
Do: Open Decision panels.
Expect:
  - **The list:** `Paed Annual Review Panel` with Scope Speciality, Decides for General panel and Members 4. There is no
    New panel button and no Edit column: she manages no panel (T239, T256).
  - **Who decides each EPA:** curriculum 11.1's EPAs are decided by `Paed Annual Review Panel`. PAED-004 and 005 fall
    back to it, because no panel covering the programme sits as the neonatal CCC.

### Step 4.15 — Dr Zulu reads Dr Molefe's review before starting it
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews and Dr Molefe's review. Read every card before starting it.
Expect:
  - **Start review** is offered to her.
  - **Decision card:** "No decision has been recorded yet."
  - **Entrustment against Annexure A** reads training year 4 on `D`, and "0 at or above · 0 below · 15 with no decision,
    of 15 EPAs". The exit rule reads "0 of 15 EPAs at their exit level by STAR decision", names all fifteen as "Not
    yet", and says it is for information only (T166).
  - **Multi-source feedback by EPA** marks Covered, in the semester Act 3's campaign closed in, each EPA that campaign
    covered. The other cells read "None released" or "None released yet" (T168).
  - **Agenda** lists the 15 EPAs:
    - window `<P>` for the six semester EPAs and `<Y>` for the nine annual ones;
    - twelve Due, each with "Must be decided at this sitting, or deferred with a reason.";
    - three As opportunity allows;
    - Evidence 0 items each;
    - no Action column, because the review has not started.
  - **Evidence snapshot:** "No evidence has been frozen yet …"
  - **Rating trajectory by EPA** charts PAED-001, PAED-010 and PAED-012 on the ladder's rungs.
  - **Sampling concentration warnings,** above the cards, name PAED-010 (one rating) and PAED-012 (two), each with
    "Fewer than three distinct assessors across this EPA's evidence." PAED-001, rated by three assessors on three
    instruments, is not named. The card says her two MSF records are not counted, because no named assessor rated
    them (T135, D44).
  - **Pending entrustment decisions:** "No pending entrustment decisions have been staged for this review."
  - **Appeals:** "No appeal has been lodged against this review."

### Step 4.16 — Dr Zulu starts the review
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start the review.
Expect: "Review started." The state reads In progress.
  - **Evidence snapshot:** frozen "when the review started", grouped by EPA and then by instrument. Every state is
    listed, and each line links to its activity and shows its rating, encounter date and state. The MSF campaign's
    report sits under "Not about a single EPA", and its per-EPA records are listed under each EPA.
  - **Agenda:** its caption reads "15 EPAs for `<P>`, 12 still to stage or defer". Each line counts its snapshot items
    and offers Stage and Defer.
  - **Decision card:** a form with these fields:
    - Category, opening on "Select a category…";
    - Rationale;
    - Conditions;
    - Present, listing Thandi Zulu (chair) ticked and locked, David Naidoo, Sarah Botha and John van Rensburg
      (external).
  - **Record decision** is disabled. The line under it reads "Record decision: PAED-001, PAED-002, … and PAED-015 must
    be decided at this sitting. Stage a decision on each, or defer it with a reason." It names the twelve closing EPAs.
  - **The staging form** appears under Pending entrustment decisions.

### Step 4.17 — Dr Zulu stages PAED-001 at level 5
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int} → /activities/{ActivityId:int} → /committee/reviews/{ReviewId:int}
Do: Open Dr Zulu's own PAED-001 Mini-CEX from the snapshot to re-read it, then come back to the review. Choose Stage on
  PAED-001's agenda line and fill in:
  - Authorised level `5`;
  - Issued on `D`, as filled;
  - no expiry;
  - the rationale "Leads resuscitation and emergency care without prompting; ready for unsupervised practice."
  Tick the Mini-CEX, then Dr Naidoo's CBD, and stage the pending decision.
Expect:
  - **The snapshot line** opens the Mini-CEX, Completed and read-only, rated 5.
  - **Choosing Stage** puts PAED-001 in the EPA select and moves the focus there.
  - **The level select** offers the rungs 1, 2, 3a, 3b, 4 and 5 under "CPSA Paediatric Entrustment Scale v11.1", with
    the help "Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on." (T076, T167)
  - **The evidence picker** lists PAED-001's three lines first. Stage pending decision stays disabled with "Name at
    least one item of the evidence snapshot." until a line is ticked (D38).
  - **With one line ticked,** the picker says "Only one item is named. The College's rule is that an entrustment
    decision is never taken from a single form." It still allows the stage.
  - **After staging:** "Pending entrustment decision staged." PAED-001's line reads Staged, "A decision is staged
    below.", and its Action cell reads "Staged below". The caption counts 11 still to stage or defer. The pending list
    shows PAED-001 at `5`, issued `D`, and "Rests on 2 items of the snapshot: …" naming both.

### Step 4.18 — An expiry before the issue date is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start staging PAED-010: choose its Stage, then level `4`, Issued on `D`, Expires on `D−1`, the rationale "Leads the
  ward round safely; one observation so far, to be confirmed at her final review." and both PAED-010 lines (the Direct
  Observation and the MSF record). Stage it.
Expect: Refused with "An expiry date must be after the issue date." Nothing is staged, the form keeps what was typed,
  and PAED-010 still reads Due.

### Step 4.19 — Dr Zulu stages PAED-010 with an expiry, and PAED-012
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Correct PAED-010's Expires on to `D+20` and stage it. Then stage PAED-012 at `5` with no expiry, the rationale
  "Breaks difficult news clearly and kindly; consistent across settings." and its two WBA lines. Remove the PAED-012
  decision, then stage it again the same way.
Expect: Both are staged. The pending list shows:
  - PAED-010 at `4`, "issued `D` · expires `D+20`";
  - PAED-012 at `5`.
  The caption counts 9 still to stage or defer.
  - **Remove** is named "Remove the staged decision on PAED-012". It answers "Pending entrustment decision removed.",
    PAED-012's line reads Due again, with Stage and Defer, and the caption counts 10.
  - **Staged again,** it reads Staged, and the caption counts 9.

### Step 4.20 — A second decision on PAED-001 is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: In the staging form, choose PAED-001 from the EPA select, then fill in level `4`, a rationale and one line. Stage
  it.
Expect: Refused with "An entrustment decision on PAED-001 is already staged at this review. Remove it to stage
  another." The pending list still holds exactly one PAED-001 decision, at `5`.

### Step 4.21 — Dr Zulu defers the other nine closing lines
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Defer on PAED-002 and submit an empty reason. Then give the reason "To be decided at her final review." and
  defer it. Reinstate PAED-002, then defer it again with the same reason. Defer the other eight lines still marked Due
  the same way.
Expect:
  - **The deferral form:** a group titled "Defer PAED-002" opens under the table with the focus in its reason box. Its
    help reads "Kept on the agenda line. The trainee sees it on their reviews page once the review is ratified."
  - **An empty reason** is refused in the form with "Say why the committee is deferring the decision." (T212).
  - **Deferring:** "PAED-002 deferred." The line reads Deferred with "Reason: To be decided at her final review." and
    offers Reinstate.
  - **Reinstating:** "PAED-002 reinstated: it is due at this sitting again." Stage and Defer return.
  - **After the last deferral:** the caption reads "15 EPAs for `<P>`" with nothing left to stage or defer. PAED-008,
    009 and 013 still read As opportunity allows. Record decision is enabled, beside "Recording fixes the entrustment
    decisions staged below and the agenda's deferrals: they cannot be changed afterwards."

### Step 4.22 — Dr Naidoo reads the sitting without the chair's controls
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Molefe's review while it is in progress.
Expect: He sees everything the chair sees:
  - the snapshot;
  - the agenda with its Staged and Deferred badges and reasons;
  - the three staged decisions and their evidence;
  - the standing, MSF coverage, trajectory and sampling cards.
  He is offered none of the chair's controls (T213):
  - the agenda has four columns, with no Action column;
  - there is no staging form, no Remove and no decision form.
  The Review card says "Only the panel's chair, Thandi Zulu, can stage entrustment decisions, defer agenda lines and
  record the committee's decision." The Decision card says "No decision has been recorded yet."

### Step 4.23 — Recording without a second member present is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose the category Satisfactory Progress and write a rationale. Tick nobody besides herself under Present, and
  record the decision.
Expect: Refused with "A committee decision needs at least two panel members present: the chair and at least one
  other." (T165, D46). The review stays In progress, with nothing recorded.

### Step 4.24 — Recording without a category is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Tick David Naidoo, Sarah Botha and John van Rensburg as present. Set Category back to "Select a category…" and
  record.
Expect: Refused with "This review decides the trainee's progression, so its decision records a progression category.
  Choose one." (T131 slice 5). Nothing is recorded.

### Step 4.25 — Dr Zulu records Dr Molefe's decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Satisfactory Progress, with the rationale "On track for completion; three EPAs entrusted, the rest to be
  decided at her final review." Keep all four present, and record the decision.
Expect: "Decision recorded." The state reads Decided.
  - **Decision card:** headed "Satisfactory Progress", with the rationale and a "Present:" line. It opens with "Thandi
    Zulu (chair)", ends with "John van Rensburg (external)", and names David Naidoo and Sarah Botha between them.
  - **Fixed:** the pending card now says the decisions were "Fixed when the committee's decision was recorded: ratifying
    issues exactly these." No Remove is offered.
  - **Agenda:** no Action column.
  - **Ratify** is offered, enabled.
  - **No chair-rated warning:** five assessors rated her lines, so "The chair rated all of this evidence" is not shown
    (D46 warns of that case and refuses nothing).

### Step 4.26 — Prof Mbatha reads the decided review, and is not offered Ratify
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Between the recording and the ratifying, open Dr Molefe's review from Committee reviews.
Expect: The list reads Decided, Satisfactory Progress, for her review. The review shows the decision with its
  "Present:" line and the three staged decisions under "Fixed when the committee's decision was recorded …". It offers
  no Ratify, no Remove and no Action column. The Review card says "Only the panel's chair, Thandi Zulu, can ratify the
  committee's decision.": only the chair ratifies, with no administrator's bypass (D46).

### Step 4.27 — Dr Zulu ratifies, and the STARs are issued
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Ratify the decision.
Expect: "Decision ratified." The state reads Ratified, and Ratify is gone.
  - **Agenda:**
    - PAED-001, 010 and 012 read Decided, each with "STAR #n.";
    - the nine deferred lines keep their reasons;
    - PAED-008, 009 and 013 read Not decided, "The review was ratified without deciding it.".
  - **Pending:** "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
    one." (T212)
  - **Standing:** "2 at or above · 1 below · 12 with no decision, of 15 EPAs". PAED-001 and 012 at `5` meet year 4's
    target of `5`, and PAED-010 at `4` is below it. The exit rule reads "2 of 15 EPAs at their exit level by STAR
    decision".
  - **What ratifying locks:** nothing on the review can now be staged, deferred, removed or recorded.

### Step 4.28 — Dr Zulu stages two decisions for Dr Dlamini
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Dlamini's review and start it. Stage, each on two finished PAED-001 lines (Dr Naidoo's Mini-CEX and Dr
  Botha's CCA):
  - PAED-001 at `4`, with the rationale "Manages the acutely ill child with the senior available on request.";
  - PAED-002 at `3b`, with the rationale "Manages common presentations with decisions checked by the senior on call."
  Defer the other ten closing lines with the reason "Not at a decision point this year."
Expect:
  - **Start:** "Review started." Her snapshot freezes her four Act 3 WBAs: three under PAED-001 and one under PAED-004.
  - **Staging PAED-002:** she has no PAED-002 line, so the picker says "None of the named items is about PAED-002." It
    still stages, since D38 lets any line of the snapshot ground a decision.
  - **The agenda:** PAED-001 and 002 read Staged, ten lines Deferred, and PAED-008, 009 and 013 As opportunity allows.
  - **Record decision** becomes enabled once the tenth line is deferred.

### Step 4.29 — Dr Zulu records and ratifies Dr Dlamini's decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Satisfactory Progress with a rationale, tick David Naidoo and Sarah Botha as present, and record. Then
  ratify.
Expect:
  - **Recorded:** "Decision recorded.", then "Decision ratified.". The state reads Ratified.
  - **Present:** a "Present:" line naming Thandi Zulu (chair), David Naidoo and Sarah Botha.
  - **The agenda:** PAED-001 and 002 read Decided with their STAR numbers, and PAED-008, 009 and 013 Not decided.
  - **Standing:** "1 at or above · 1 below · 13 with no decision, of 15 EPAs". PAED-001 at `4` meets year 3's target of
    `4`, and PAED-002 at `3b` is below it.

### Step 4.30 — Dr du Plessis's sitting: progress with an observation, no STAR
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr du Plessis's review and start it. Stage nothing, and defer all twelve closing lines with the reason "Not at
  a decision point this year." Record Satisfactory with Observations:
  - the rationale "Teaching logs are sound; no observed clinical assessment completed yet.";
  - the condition "At least two Mini-CEX before the next sitting.";
  - David Naidoo present.
  Then ratify.
Expect:
  - **The snapshot** holds his two teaching sessions logged this year, his CBD still Requested, his cancelled DOPS and
    his portfolio review still Awaiting review, each with its state (T138). The session logged for `J−1y−56d` is outside
    the window and not in it.
  - **Rating trajectory:** "No rating by a named assessor to chart for this trainee."
  - **Recorded:** the decision reads "Satisfactory with Observations", with "Conditions: At least two Mini-CEX before
    the next sitting." and "Present: Thandi Zulu (chair), David Naidoo".
  - **Ratified:** the state reads Ratified, and the pending card says "Nothing was staged when the review was ratified,
    so ratifying it issued no STAR."

### Step 4.31 — Dr Mahlangu's sitting: additional training
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review and start it, and read her snapshot. Defer all twelve closing lines with the reason "Not
  enough observed evidence yet this year." Record Inadequate Progress — Additional Training:
  - the rationale "One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six
    months.";
  - Sarah Botha and John van Rensburg present.
  Then ratify.
Expect:
  - **The snapshot** lists her DOPS under PAED-002 and her Mini-CEX under PAED-004, both Completed at 3a.
  - **Sampling concentration warnings** name PAED-002 and PAED-004, one rating each.
  - **Ratified:** the state reads Ratified.
  - **Present:** "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)".

### Step 4.32 — Dr Ndlovu's sitting: the outcome deferred
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Ndlovu's review and start it, and read his snapshot. Defer all twelve closing lines with the reason "Too
  early in the programme to decide." Record Outcome Deferred:
  - the rationale "One completed Mini-CEX and one reflection in his first semester; to be judged at the next
    sitting.";
  - Sarah Botha present.
  Then ratify.
Expect:
  - **Evidence snapshot:** his three Act 3 activities, each with its state: the Mini-CEX Dr Khumalo declined and the
    one Dr Botha completed at 3a, both under PAED-002, and the reflective exercise under PAED-001, discussed. A declined
    request is evidence too (T138).
  - **Trajectory:** PAED-002 alone, from Dr Botha's rating.
  - **Sampling:** the warnings card names PAED-002, one rating from one assessor.
  - **Ratified:** the review reads Ratified, with "Outcome Deferred".

### Step 4.50 — Dr Zulu starts and closes the check-in
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start the formative review, read it, and close it.
Expect:
  - **Started:** "Review started." The snapshot freezes the same three activities as his annual review, and the standing
    and MSF coverage cards follow it. Close review is offered to her alone.
  - **Closed:** "Formative review closed." The state reads Closed, and nothing is offered.
  - **Other readers** are told "Only the panel's chair, Thandi Zulu, can close this review." while it is in progress.

--- from execution/knowledge/scenario-paediatrics/act-5-graduation.md ---

### Step 5.3 — Dr Zulu starts the review and reads where Dr Molefe stands
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Molefe's pre-graduation review from Committee reviews and start it. Read the evidence snapshot, the agenda
  and "Entrustment against Annexure A".
Expect: The page says "Review started." and the review reads In progress.
  - The snapshot freezes her evidence observed since `J−3y`, grouped by EPA, including the released MSF
    campaign.
  - The standing reads training year 4, and "2 at or above · 1 below · 12 with no decision, of 15 EPAs". Its exit rule
    reads "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)", followed by "Not
    yet: …", which names every EPA but PAED-001 and PAED-012.
  - Under the rule: "For information only. Recording a Graduate decision or completing the programme does not check the
    exit rule."
  - Record decision is shown disabled, and names the closing agenda lines that are still to be staged or deferred.
  - Sampling warnings may name EPAs rated by fewer than three assessors. They are warnings and refuse nothing.

### Step 5.4 — Dr Zulu stages a STAR at its exit level on every EPA not yet there
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: For each EPA the standing names under "Not yet", stage a decision at its exit level. That is thirteen EPAs, every
  one but PAED-001 and PAED-012, which Act 4 decided at their exit level:
  - rung `5` on PAED-002 to 007 and PAED-010;
  - rung `4` on PAED-008, 009, 011, 013, 014 and 015.
  Use Stage on the EPA's agenda line. For PAED-010, which Act 4 decided in this window below its exit level, choose it
  in the staging form's EPA list instead. Issue each decision on `D` with no expiry, write a rationale,
  and tick the snapshot lines it rests on, choosing that EPA's own lines where it has some. Defer nothing.
Expect: The level list offers only the six rungs of the CPSA Paediatric Entrustment Scale v11.1 (`1`, `2`, `3a`, `3b`,
  `4`, `5`). Stage pending decision stays disabled until a line is ticked. A decision that rests on no line about its
  own EPA, or on a single line, gets a hint but is not refused. After each staging:
  - the page says "Pending entrustment decision staged.";
  - the EPA's line reads Staged, and an EPA that was not on the agenda gains a chair's line;
  - the decision is listed with its level, its issue date and the lines it rests on.
  Once every closing line is staged, Record decision is enabled.

### Step 5.5 — The panel records its decision: Graduate
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: Record the committee's decision:
  - category: Graduate (programme complete);
  - rationale: every EPA is entrusted at its exit level, and the panel recommends completion;
  - conditions: none;
  - present: Dr Zulu, Dr Naidoo, Dr Botha and Dr van Rensburg.
Expect: The category list opens with nothing chosen and offers Graduate (programme complete) among the progression
  outcomes. Dr Zulu is ticked as present and cannot be unticked. The review then reads Decided (T250). The decision is
  headed "Graduate (programme complete)" and names the four members present. The page says that the staged decisions
  are now fixed and that ratifying issues exactly these. The staging form is gone, and only Dr Zulu is offered Ratify.

### Step 5.6 — Dr Zulu ratifies
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: Ratify the decision.
Expect: The page says "Decision ratified." and the review reads Ratified. Every staged decision is issued as a STAR in
  the same action:
  - each agenda line reads Decided;
  - the list of pending decisions is empty;
  - Act 4's active STAR on PAED-010, which was re-decided today, is superseded.
  The standing's exit rule reads "15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of
  6). Every EPA is at its exit level." Its year line reads "15 at or above · 0 below · 0 with no decision, of 15 EPAs".

--- from execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md ---

### Step A.7.6 — Dr Zulu on her phone
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: At 390 px, open Committee reviews and Dr Molefe's final review.
Expect: The review's cards stack. The evidence tables scroll inside their containers, and each EPA's trajectory chart
  fits the width (T166).

### Step A.7.13 — Dr Zulu hears a populated review with a screen reader
Role: CommitteeMember — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: With NVDA (or Narrator) on, open Dr Molefe's final review. Walk it by headings, then by tables, then by regions.
Expect: The page heading is announced on arrival. Each card is a heading ("Review", "Decision", "Evidence snapshot",
  "Rating trajectory by EPA", "Appeals", "Entrustment against Annexure A", "Multi-source feedback by EPA", "Agenda"),
  and each EPA group within a card is a heading below it.
  - Each trajectory chart is announced by its EPA, and a table follows it that a screen reader reads, with one row per
    observation (T085).
  - Evidence tables announce their column headers, and every row action is named for its row (T239).
  - Every badge is read as its words.
  - No control is unnamed.
```

### 2.2 After you pick a wireframe for each part, paste this

```
Take variation <letter> of Part 1, <letter> of Part 2 and <letter> of Part 3 to full fidelity at 1280×800 and
390×844, in the design system you hold. Draw every state under STATES TO SHOW, the chair's and a member's view of each,
and the confirmation for Record and for Ratify. Show where the focus lands after Stage, Defer, Remove, Record and Ratify.
For each artboard, list the components used and mark NEW ones. For each colour, name the token, or give a NEW token and
its value. Then check contrast (text 4.5:1; borders and the focus ring 3:1) and every target (at least 24 px).
```

## 3. The journey

One line per step: what the person does, and what they must be able to see. The step's full text is in § 2.1.

| Step | Page template | Who | Does, and must see |
|---|---|---|---|
| 4.14 | `/committee/panels` | Dr Botha | Who sits on the panel and which EPAs it decides; PAED-004 and 005 fall back to it (the panel page is F08's). |
| 4.15 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | Before Start: Start offered; no decision yet; the standing (0 of 15), MSF coverage, a 15-line agenda (12 Due, 3 optional) with no Action column, the snapshot not yet frozen, three trajectories, and sampling warnings. |
| 4.16 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Starts it: "Review started.", the snapshot frozen and grouped, Stage and Defer on each line, the decision form with Present, and Record disabled with its reason. |
| 4.17 | `/committee/reviews/{ReviewId:int}` → `/activities/{ActivityId:int}` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | Opens her own Mini-CEX from the snapshot, comes back, and stages PAED-001 at 5 on two items. She sees the ladder, the "one item" hint, and the line reading Staged. |
| 4.18 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | An expiry before the issue date is refused; everything typed is kept. |
| 4.19 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Stages PAED-010 with an expiry and PAED-012, then removes and re-stages PAED-012. The caption counts down. Remove asks for no confirmation. |
| 4.20 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | A second decision on PAED-001 is refused. |
| 4.21 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Defers nine lines with a reason (an empty one is refused), reinstates one, and ends with Record enabled beside its "cannot be changed afterwards" sentence. |
| 4.22 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Naidoo | Follows the sitting: everything the chair sees, no controls, and a sentence saying only the chair acts. |
| 4.23 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Records with only herself present: refused for want of a quorum. |
| 4.24 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Records with no category: refused. |
| 4.25 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Records Satisfactory Progress with four present. The decision card shows the Present line; staged decisions are fixed; Ratify is offered. |
| 4.26 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Prof Mbatha | Reads the decided review; no Ratify, no Remove; "Only the panel's chair … can ratify" (D46). |
| 4.27 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Ratifies: STARs #1–#3 issued, the agenda reads Decided / Deferred / Not decided, and the standing updates. No confirmation is asked, and no mail is sent (T320). |
| 4.28 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | Dr Dlamini's review: stages PAED-002 on items not about it (a hint, not a refusal), and defers ten. |
| 4.29 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Records and ratifies with three present; the standing reads 1 at or above, 1 below. |
| 4.30 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | Dr du Plessis: stages nothing, records "Satisfactory with Observations" with a condition, and ratifies with no STAR. |
| 4.31 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | Dr Mahlangu: "Inadequate Progress — Additional Training", the external member present. |
| 4.32 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | Dr Ndlovu: the snapshot holds a declined request as evidence; "Outcome Deferred". |
| 4.50 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | A formative check-in: starts, reads, and closes it. Others see "Only the panel's chair … can close this review." |
| 5.3 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | The pre-graduation review: the widened snapshot and the exit rule "2 of 15 … Not yet: …", which gates nothing. |
| 5.4 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Stages thirteen STARs at their exit levels, one after another. |
| 5.5 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Records "Graduate (programme complete)" with four present. |
| 5.6 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Ratifies: STARs #6–#18 issued; "15 of 15 … Every EPA is at its exit level." |
| A.7.6 | `/` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | At 390 px: 17 cards stack, tables scroll in their containers, and the chart labels shrink to about 6 px (T323). |
| A.7.13 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | With a screen reader: headings, tables, regions. Today only 3 of 9 cards are named regions, and the dismiss button's name is "×" (T280). |

## 4. States to design

From `execution/knowledge/scenario-paediatrics/states.md` § Committee. Each file is
`design/baseline/states/review-detail--<state>.png`.

| State | Capture | Reached at |
|---|---|---|
| Scheduled, the chair | `review-detail--scheduled.png` | Step 4.15, with the sampling warnings |
| In progress, the chair | `review-detail--in-progress.png` | Step 4.16: the staging form; Record disabled with its reason |
| One item named | `review-detail--single-item.png` | Step 4.17 |
| Staged | `review-detail--staged.png` | Step 4.17, after staging |
| Expiry refused | `review-detail--expiry-refused.png` | Step 4.18 |
| Second decision refused | `review-detail--duplicate-refused.png` | Step 4.20 |
| Deferral form, no reason | `review-detail--defer-empty.png` | Step 4.21 |
| Every closing line settled | `review-detail--ready-to-record.png` | Step 4.21, after the last deferral |
| A member during the sitting | `review-detail--member.png` | Step 4.22 |
| No quorum | `review-detail--quorum-refused.png` | Step 4.23 |
| No category | `review-detail--category-refused.png` | Step 4.24 |
| Decided, the chair | `review-detail--decided.png` | Step 4.25: Ratify offered |
| Decided, an administrator | `review-detail--decided-admin.png` | Step 4.26 |
| Ratified, STARs issued | `review-detail--ratified.png` | Step 4.27 |
| No item about the EPA | `review-detail--none-about-epa.png` | Step 4.28 |
| Ratified, no STAR | `review-detail--ratified-no-star.png` | Step 4.30 |
| Formative, in progress | `review-detail--formative-in-progress.png` | Step 4.50 |
| Formative, closed | `review-detail--formative-closed.png` | Step 4.50 |
| Pre-graduation, in progress | `review-detail--pre-graduation.png` | Step 5.3 |
| Graduate recorded | `review-detail--graduate.png` | Step 5.5 |
| Every EPA at its exit level | `review-detail--graduate-ratified.png` | Step 5.6 |
| The chair can no longer act | `review-detail--chair-cannot-act.png` | Scratch (post-act4): Dr Zulu locked out; Dr Naidoo opens the review |
| Agenda: no longer decided | `review-detail--no-longer-decided.png` | Scratch (post-act4): a STAR revoked mid-review |
| Ratify blocked | `review-detail--ratify-blocked.png` | Scratch (post-act4) |
| Entrustment-only review | `review-detail--entrustment-only.png` | Scratch (post-act4): a second panel sitting as the neonatal committee |
| Decided by another panel | `review-detail--decided-elsewhere.png` | The same scratch run (named only in `states.md`'s prose; BRIEF § 10) |
| Loading | `review-detail--loading.png` | A held read: the header and blank space (T329) |
| Not found | `review-detail--not-found.png` | Typed `/committee/reviews/999999` |
| 390 px | `review-detail--narrow.png` | Step A.7.6 |

**Not captured, to be drawn from their description** (`states.md:775-778`):
- "The chair rated all of this evidence" (D46 warns and refuses nothing).
- "Sampling figures are incomplete".
- The note that the trainee is now at another institution.
- "Starting now leaves out N feedback campaigns".
- Not played at all (`coverage.md` § Flows and states not played): the Administrator on a review (no ratify or appeal
  bypass, D46), and Defer after recording. The scratch captures above cover a neonatal panel and "Decided elsewhere".

**Belongs to other flows:**
- F08: the scheduled review with no Start (`--scheduled-no-start`) and the formative review as scheduled
  (`--formative`).
- F09: the appeal card's states (`--appeal-*`, `--remit-refused`, `--remitted`), re-captured after T307 (2026-09-26);
  `--appeal-upheld` no longer exists (D51).
- F14: a review withdrawn by an erasure (`--withdrawn`).

## 5. Attach

Every path below is under `design/baseline/` and was checked with `ls` on 2026-09-26. Open each before you upload it
(BRIEF § 3.3). None of them is an invitation page.

### 5.1 Key screenshots (attach with the ask)

1. `states/review-detail--scheduled.png`: the whole page before Start (7,111 px tall).
2. `act-4/4.17-1-zulu-minicex-from-snapshot.png`: a piece of evidence opened from the snapshot.
3. `states/review-detail--staged.png`: one decision staged (9,126 px tall).
4. `states/review-detail--ready-to-record.png`: every line settled, Record enabled.
5. `states/review-detail--quorum-refused.png`: the quorum refusal.
6. `states/review-detail--ratified.png`: ratified, STARs issued.
7. `act-5/5.3-2-zulu-standing-exit-rule.png`: the standing and the exit rule.
8. `act-5/5.4-4-zulu-thirteen-staged-full.png`: thirteen staged decisions (12,711 px tall).
9. `states/review-detail--member.png`: a member's view.
10. `act-A/A.7.13-1-zulu-review7-top.png`: the top of a populated review, with the "×" dismiss button.
11. `states/review-detail--narrow.png`: 390 px (12,935 px tall).

### 5.2 State captures (add as the chat asks)

- Review page states:
  - `states/review-detail--in-progress.png`
  - `states/review-detail--single-item.png`
  - `states/review-detail--expiry-refused.png`
  - `states/review-detail--duplicate-refused.png`
  - `states/review-detail--defer-empty.png`
  - `states/review-detail--category-refused.png`
  - `states/review-detail--decided.png`
  - `states/review-detail--decided-admin.png`
  - `states/review-detail--none-about-epa.png`
  - `states/review-detail--ratified-no-star.png`
  - `states/review-detail--formative-in-progress.png`
  - `states/review-detail--formative-closed.png`
  - `states/review-detail--pre-graduation.png`
  - `states/review-detail--graduate.png`
  - `states/review-detail--graduate-ratified.png`
  - `states/review-detail--chair-cannot-act.png`
  - `states/review-detail--no-longer-decided.png`
  - `states/review-detail--ratify-blocked.png`
  - `states/review-detail--entrustment-only.png`
  - `states/review-detail--decided-elsewhere.png`
  - `states/review-detail--loading.png`
  - `states/review-detail--not-found.png`
- Evidence for the known problems:
  - `act-A/A.7.6-2-zulu-trajectory.png` (the charts at 390 px)
  - `act-A/A.7.14-6-reviews-badges.png` (badge contrast)
- Step captures for detail:
  - `act-4/4.16-1-zulu-review-started.png`
  - `act-4/4.19-1-zulu-three-staged.png`
  - `act-4/4.21-1-zulu-ready-to-record.png`
  - `act-4/4.25-1-zulu-decision-recorded.png`
  - `act-4/4.27-1-zulu-ratified-stars-issued.png`
  - `act-4/4.30-1-zulu-duplessis-ratified-no-star.png`
  - `act-5/5.4-1-zulu-stage-paed002-hint.png`
  - `act-5/5.5-1-zulu-graduate-form.png`
  - `act-5/5.6-2-zulu-exit-rule-met.png`
  - `act-4/4.14-1-botha-decision-panels.png`

### 5.3 Held

None.

## 6. Known problems this design must solve

| Task | Problem today | What it means for the design | Evidence |
|---|---|---|---|
| T329 (P2; BRIEF A6) | `ReviewDetail.razor:113` renders the body only once the review is read. `LoadAsync` keeps no loading flag, and the page has no `StatePanel`. | The header renders from the first moment, with a skeleton for the body. A failed first read shows its alert there. No action is offered before the review has loaded. | `states/review-detail--loading.png` |
| T323 (P3; BRIEF A3) | Each trajectory is a 600×200 viewBox scaled to about 260 px, so its labels are 5–6 px. The table beside it is for screen readers only. | Text at least 11 px at 390 px, or the table visible under 641 px. Use F05's answer; it is one component. | `act-A/A.7.6-2-zulu-trajectory.png` |
| T273 (P3) | The sampling list and the staged lines print a bare "Code — Title" for an EPA no longer in force (DESIGN.md:448-450). | Mark both through `EpaLabel`, "(no longer in use)". A staged line also says "No longer fits". Decision adopted 2026-09-25: the frozen snapshot's headings are not re-marked. | — |
| T325 (P3; BRIEF A9) | "Issued on" defaults to the UTC date (`ReviewDetail.razor:2083`), a day behind between 22:00 and 24:00 UTC. | The default is today on the South African calendar. Any printed time carries its zone. | Step 4.17 ("Issued on filled 2026-09-26") |
| T260 (P3) | After a panel save, the panel page keeps a stale "can no longer sit" warning (F08's page). | This page's seat notes ("The chair can no longer act", T256) are re-read after every action and never left stale (inference: the same class of note). | `states/review-detail--chair-cannot-act.png` |
| T280 (P3; BRIEF A14) | The sampling warnings' dismiss button is `<button …>×</button>` with no name. Only 3 of the 9 cards are named regions (Step A.7.13). | "Dismiss" as the accessible name. Every card `aria-labelledby` its heading. | `act-A/A.7.13-1-zulu-review7-top.png` |
| T322 (P2; BRIEF A1) | Badge text on its tint fails 4.5:1 (for example success 2.55:1). | Body text on the tint, as `.badge-standing-*` does already. | `act-A/A.7.14-6-reviews-badges.png` |
| T309 (P3) | A formative check-in carries Type "Annual progression review" (Steps 4.49, A.1.10). | Every surface reads "Formative check-in" (short: "Formative"). | `states/review-detail--formative-in-progress.png`, `states/review-detail--formative-closed.png` |
| BRIEF A5; T264 (P3) | The result region (`ReviewDetail.razor:16-26`) is at the top of the page and takes the focus after each action. So each stage sends the chair about 6,000 px up from the staging form, at 1280 px (inference from the code and `states/review-detail--staged.png`). Remove, Ratify and Close review ask for no confirmation: the page has no `ConfirmDialog` (Steps 4.19, 4.27, 4.50). | Each action's outcome appears, and takes the focus, where the chair is working. Ratify and Record, which cannot be undone, confirm and name what they fix or issue. Remove confirms and names the staged decision. | Steps 4.19, 4.27, 4.50 |
| T320 (P2, backend) | Ratifying issues STARs and sends no mail (Step 4.27, F-4.27a). | Do not promise "the registrar will be told" in the confirmation until T320 decides. | Step 4.27 |
| T190 (P3; BRIEF A14) | The h1 is "Committee review" and the tab title "Committee Review" (`ReviewDetail.razor:7,9`). Neither names the registrar. The headings go h1 then h3, with no h2 (Step A.7.13 Note; cosmetic, T085). | One title pattern that names the registrar and the period. Headings in order. | `act-A/A.7.13-1-zulu-review7-top.png` |
| Layout (observed, not filed) | The Decision form (Record) sits at the top beside the Review card. The Agenda, where lines are staged and deferred, starts about 3,000 px lower, above the staging form (DESIGN.md:1231-1233: the Agenda "directly above the 'Pending entrustment decisions' card, so a chair reads what is due and stages it in one place"). | Keep the agenda and the staging together, and decide where Record belongs (questions 1 and 3). | `states/review-detail--staged.png` |

## 7. Questions the design must answer

1. **One long page, or steps (evidence, stage, record, ratify)?** How does the chair keep her place across 13 staged
   lines (Step 5.4, `act-5/5.4-4-zulu-thirteen-staged-full.png`)?
2. **What stays on screen while the chair stages?** The registrar's standing and shortfalls (the Annexure A table), or
   the evidence list (the snapshot and its ticks, D38)?
3. **How does the page show who is present, and whether the quorum is met, before the chair presses Record?** At least
   two present, the chair among them (D46). Today it is known only from the refusal (Step 4.23).
4. **How are irreversible acts marked and confirmed?**
   - Recording fixes the staged decisions and deferrals.
   - Ratifying issues STARs, "immutable once issued" (`DOMAIN.md:78`); there is no undo, and no Administrator bypass
     (D46).

## 8. Acceptance

Claude Code builds the chosen design as one task, or one per part if the page becomes steps (BRIEF § 9). It is done
when all of the following hold.

**Tests.**
- `dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj` is green. Never use `--no-build` (CLAUDE.md § Testing).
- The run includes the tests that pin this page:
  - `CommitteeDecisions/ReviewDetail*Tests` (Agenda, ChairControls, EvidenceEmptyState, EvidenceGrouping,
    EvidencePicker, MsfOutsideSnapshot, Quorum, SamplingSentence);
  - `CommitteeDecisions/EntrustmentStandingPanelTests`, `EntrustmentOnlyReviewPageTests`,
    `CommitteeReviewStateLabelTests` and `WithdrawnReviewPageTests`;
  - `Charts/TrajectoryChartTests`;
  - `Accessibility/ActionFocusTests` and `RowNamesTests`;
  - `Design/*`;
  - the scenario guard in `Scenario/`.

**The replay.**
- Replay the flow's steps on a fresh database (`tools/scenario-replay.ps1`, BRIEF § 9):
  - 4.14–4.32 and 4.50 (Act 4);
  - 5.3–5.6 (Act 5);
  - A.7.6 and A.7.13 (appendix).
- Each act may instead start from `recovery/scenario-post-act3.dump` or `-act4.dump` (README § How to play,
  "Snapshots"). Every Expect must hold, the refusals' exact words included.
- **Update the Expects the design changes.** A confirmation dialog adds a step to 4.19, 4.25, 4.27, 4.29–4.32, 4.50,
  5.5 and 5.6. A layout of steps changes the Route lines. The steps quote the screen (BRIEF § 9, item 7).
- Replay the scratch states in § 4 (`states.md` § Three ways a state is reached).

**Checks the design adds.**
- Step 4.15, reached from the nav with a held read: the header and a skeleton show, and no Start is offered until the
  review has loaded (T329).
- Step 4.17: after staging, the focus is on the result, and the result is where the chair was working.
- Step 4.27: Ratify asks for confirmation and names what it issues.
- Step A.7.6 at 390 px: trajectory text is at least 11 px, or the table is visible (T323). No page scrolls sideways.
- Step A.7.13: every card is a named region, the dismiss button is named, and the headings are in order (T280).

**Re-capture.** Take the 29 state captures in § 4 and this flow's step captures again (`states.md` § How to capture).
Compare them with the chosen artboards.

**Browser check.** Check as the chair, a member and the InstitutionalAdmin, at 1280 px and at 390 px.
