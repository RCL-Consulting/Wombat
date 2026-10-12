# F07 The committee sits: the chair stages STARs, records with a quorum, and ratifies

**Who and why:** a registrar's review is the one moment the programme's evidence becomes a decision. The panel's chair
reads the registrar's frozen evidence and standing, stages STARs (Statements of Awarded Responsibility), defers what the
panel will not decide, records the committee's decision with a quorum, and ratifies, which issues the STARs. Members
follow on their own screens without the chair's controls; an administrator reads and cannot ratify. A STAR is
"immutable once issued" (`execution/knowledge/DOMAIN.md:78`): this is the highest-stakes page in the product, and today
it is one page 9,192 px tall before Start and 14,834 px with thirteen decisions staged, with every result and refusal
shown at its top.

**Built 2026-10-12 (T361, step 6),** from round 3 (`07-review-sitting/round-3-check.md`): design A, "the agenda is the
page". **There is no deferral** (D53, the operator's, EPA-PROGRAMME § 3D): every agenda line the chair does not stage is
recorded as "Not decided at this sitting" when the review is ratified, with the decision's one optional sentence about
the EPAs not decided. §§ 1–7 below are the brief as round 1 was sent it, and describe the page before the build,
deferral included; § 8 is re-synced to the runbook as built. The build's own record is `07-review-sitting/build-*.md`.

Restated 2026-10-11 for T361 (the brief of 2026-09-26, T332, predates flows 01–06). Paths to screenshots are under
`design/baseline/` (gitignored). Code is cited at `bc9a6c4b`; `W/` is `src/Wombat.Web/`, `A/` is
`src/Wombat.Application/`, `D/` is `src/Wombat.Domain/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **W** in BRIEF.md § 8: the review page is expected to change a lot, so round 1 asks for 2–3 structural variations. |
| Mode | **Structure first** (BRIEF.md § 2.3, as § 11 corrected it). Round 1: 2–3 structural variations as wireframes of the review page through a whole sitting (before Start, staging, recording, ratifying) and of what each adds or drops, each with its reasoning and its step counts. Round 2, after the pick: fidelity with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. The old brief's "three parts, a wireframe each" is replaced by this. |
| Viewports | Desktop 1280×800 and phone 390×844 |
| Variations | 2–3 in round 1; the chosen one in round 2 |
| People | CommitteeMember: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha (each also an Assessor), Dr John van Rensburg (external, CommitteeMember only). InstitutionalAdmin: Prof Nolwazi Mbatha, who reads a decided review (Step 4.26). The registrars reviewed: Dr Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa Mahlangu and Dr Sipho Ndlovu (1) (`README.md` § Cast) |
| Runbook steps | 26 when round 1 was sent (25 since the build: 4.21, the deferral, is gone, D53), pasted verbatim in § 8: 4.14–4.32 and 4.50 (Act 4), 5.3–5.6 (Act 5), A.7.6 and A.7.13 (appendix). Forty steps touch the review page in all (`coverage.md`); the others are flow 08's (scheduling, panels) and flow 09's (appeals, revocation). |
| Pages (`coverage.md` templates) | `/committee/reviews/{ReviewId:int}` (`W/Components/Pages/CommitteeDecisions/ReviewDetail.razor`, 2,162 lines), `/activities/{ActivityId:int}` (read-only, opened from the snapshot; flow 03's page), `/committee/reviews` (`ReviewsSchedule.razor`, the list, to open one; flow 08's), `/committee/panels` (`PanelsList.razor`, who sits; flow 08's). The registrar page `/programme/trainees/{id}` (flow 06's, `ProgrammeTraineeDetail.razor:640–665`) and Decisions due (flow 08's, `DecisionsDue.razor:152`, `:160`) also link a review. Checked at `bc9a6c4b`. |
| Menus (`NavItems.cs`) | Committee reviews (`:100`) is in every staff menu that reaches a review: CommitteeMember (`:169`: Home, Programme trainees, Committee reviews, Decision panels); SpecialityAdmin and SubSpecialityAdmin (`:166`, `:168`); Coordinator (`:175`); InstitutionalAdmin and Administrator under "Reviews" (`:154`, `:148`). The review page's owning list is Committee reviews for all six (`W/Navigation/NavOwners.cs:105–114`; DESIGN.md owner table `:599`), so its trail reads Home › Committee reviews › Committee review. DESIGN.md's nav table (`:529–535`) matches. |
| Components drawn at runtime | `PageHeader`, `ActionResult`, `Alert` (the sampling card is its only `Dismissible` caller), `FormField`, `FormActions`, `EditForm` with `InputSelect`/`InputDate`/`InputTextArea`, `EncounterDateLabel`; flow 05's `EntrustmentStandingPanel` (`Shared/`, 332 lines), `MsfCoveragePanel` and `TrajectoryChart` (two fixed drawings, 900 and 322 px, its table always visible). The agenda, the staging form, the staged list and the snapshot are inline in `ReviewDetail.razor`, helped by `EvidenceSnapshotGroups.cs` and `CommitteeAgendaText.cs`. Not used: `ConfirmDialog`, `StatePanel`, `Skeleton`, `DataTable`. Queries: `GetCommitteeReviewByIdQuery` (with the agenda), `ListPendingEntrustmentDecisionsForReviewQuery`, `GetSamplingConcentrationWarningsQuery`, `GetEntrustmentStandingForTraineeQuery`, `GetMsfCoverageForTraineeQuery`, `GetEpaTrajectoryForTraineeQuery`; commands `Start…`, `RecordCommitteeDecision…`, `RatifyCommitteeDecision…`, `CloseFormativeReview…`, `DeferAgendaLine…`, `ReinstateAgendaLine…`, `StagePendingEntrustmentDecision…`, `RemovePendingEntrustmentDecision…` (`A/Features/CommitteeDecisions/`, `A/Features/EntrustmentDecisions/`) |
| Held | None. Every step capture § 4 names was re-taken by the T358 replay on 2026-10-05, on the shell as built. Most `states/review-detail--*` captures are from 2026-09-26 and show the old frame: brief from their page body only (§ 3). |
| Depends on | Flow 01, built (`b347e11c`): the shell, the trail, `ActionResult`, `ConfirmDialog` (designed, unused on this page), access denied and not found. Flow 03, built (`725237ee`): the activity page opened from the snapshot. Flow 04, built (`06aa51d7`): rows named per row, counts as words. Flow 05, built (`b020c942`): the standing panel, the trajectory charts and their table, ISO dates (D1), an EPA's name as its own link, `PageFocus` for in-page links. Flow 06, built (`85d5a508`): the committee Home, Programme trainees and the staff registrar page that links her reviews. The design system is **version 17**, re-synced to flow 06 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 07`. Open each screenshot. Leave out anything in
   `design/upload/crop-first/`, or crop its link first (none of this flow's captures shows one; checked 2026-10-11). The script keeps back
   `act-4/4.27-1-zulu-ratified-stars-issued.png` for the word "issued" in its name: it shows no link, so move it back.
   Two captures are 18,013 px tall, past Chromium's 16,384 px, and repeat the header near their foot (§ 4).
2. **Start a new canvas** from the main app's Design page, on the Wombat design system v17
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier flow's canvas. Flow 07's canvas: https://claude.ai/artifact/9hVRTiLRBUugygR7bHB1CS (created
   2026-10-11).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Put the returned `/_blob/` URL in § 1 and in
   `design/flows/07-review-sitting/round-1-ask.txt`, where they read `/_blob/33198ce497bf7fb3aeac9b59047b615b`.
4. **Send § 1 and § 8 as ONE message** (`design/flows/07-review-sitting/round-1-ask.txt`), with the key screenshots in
   § 4. The canvas draws round 1 from the first message and does not wait for a second (§ 11, flow 02). Read
   `project/canvas.json`'s version before and after: a send can draw nothing. Read round 1's notes for a line saying it
   never received the attachments (§ 11, flow 06); re-attach and name the folder before round 2 if so.
5. **Pick one structure,** say why in a sentence, and answer § 6's questions one sentence each; questions 10–12 are the
   operator's. A question whose answer rests on an open task names it, and the task lands before the build (§ 11, flow
   05). Ask for round 2.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/06-programme-oversight/round-2-review.md`
   did: one reviewer on the code, one on the cast, one on the design system; the reviewer on the code checks the ask
   too (§ 11, flow 03). Put the decisions to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/07-review-sitting/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 07 — The committee sits: stage STARs, defer, record with a quorum, ratify

This is flow 07 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands) and
  06 (watching the programme) are designed and built; this page sits inside the shell. Structure first, then fidelity:
  round 1 is 2–3 structural variations as wireframes of one registrar's review through a whole sitting, and I pick one
  before round 2. The runbook steps follow below in this same message. The brand mark is at /_blob/33198ce497bf7fb3aeac9b59047b615b: use it, never
  a drawn disc.

GOAL: The panel's chair runs one registrar's review, start to finish, in one sitting, with the panel watching:
  - she reads the evidence frozen at Start, the registrar's standing against the College's levels (Annexure A), MSF
    coverage and each EPA's rating trajectory, and opens any piece of evidence and comes back to where she was;
  - she stages STARs: an EPA, the level the registrar is entrusted at, the issue date, an optional expiry, a rationale,
    and the snapshot items it rests on;
  - she defers each other EPA due at this sitting, with a reason, and may reinstate it;
  - she records the committee's decision: a progression category, a rationale, conditions, and who is present (the
    chair and at least one other member);
  - she ratifies, which issues every staged STAR at once; nothing on the review can change afterwards.
  Members follow the same review on their own screens with none of the chair's controls; an administrator reads it and
  cannot ratify. A formative check-in is read and closed, with no decision.
  What is wrong today (the attached screenshots show each; all on the shell as built, 2026-10-05):
  - It is one long page with no shape for a sitting: 9,192 px at 1280 before Start, about 11,000 in progress, 14,834
    with thirteen decisions staged; 18,013 at 390. Its cards, in order: a sampling-warnings alert; Review (with Start,
    Close review or Ratify) beside Decision (the record form); Entrustment against Annexure A; Multi-source feedback by
    EPA; Agenda (Stage and Defer per line); Pending entrustment decisions (the staging form, then the staged list)
    beside the Evidence snapshot; Rating trajectory by EPA (a chart per EPA); Appeals.
  - The record form sits at the top, beside the Review card, about 3,000 px above the agenda where the lines it waits
    on are staged and deferred. Choosing Stage on an agenda line sends the focus to the staging form's EPA select, below
    the agenda; the evidence to tick is in the same form, the snapshot to read is beside it.
  - Every action's result, and every refusal, shows at the very top of the page. A success takes the focus there, about
    6,000 px from the staging form; a refusal leaves the focus where it was, out of sight of its message.
  - Nothing asks for confirmation: not Start (which freezes the evidence), Remove, Record (which fixes what is staged),
    Ratify (which issues STARs), nor Close review.
  - Whether a quorum is present is known only from the refusal after pressing Record.
  - While the review loads, the header shows over a blank body; a review that cannot be found shows a red alert under
    "Committee review", with no body.
  - The h1 reads "Committee review" and the subtitle "Evidence bundle, decision record, ratification, and appeals in one
    place."; neither names the registrar or the period. Headings go h1 then h3, with no h2. Five of the nine cards
    (Review, Decision, Entrustment against Annexure A, Pending, Appeals) are not named regions.
  - The sampling warnings' dismiss button is a bare "×", unnamed, and the card is the only dismissible alert in Wombat.
  - A piece of evidence opened from the snapshot (an activity page, read-only) has no way back but the browser's Back,
    and the review reloads from its top.
  - A formative check-in's Type still reads "Annual progression review" (T309).
  - "Issued on" defaults to the UTC date, a day behind between 22:00 and 24:00 UTC, South African midnight to 02:00
    (T325).

AUDIENCE:
  - CommitteeMember: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha: senior paediatric consultants who also
    assess, so their sessions open as Committee member with "Switch to Assessor". Dr John van Rensburg: external
    examiner from another university, Committee member only. A sitting is a meeting: the chair drives the page, often
    on a laptop to a projector; members open the same review on their own screens while it is in progress.
  - InstitutionalAdmin: Prof Nolwazi Mbatha (head of department), who reads only.
  - Kgosi Kgari Teaching Hospital (KGK), Paediatrics, on the College's (CPSA) curriculum 11.1, fifteen EPAs (PAED-001
    to PAED-015), each rated on the six rungs of the CPSA Paediatric Entrustment Scale v11.1 (1, 2, 3a, 3b, 4, 5). The
    registrars reviewed: Dr Lerato Molefe (training year 4; an annual review in Act 4, a pre-graduation review in Act
    5), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa Mahlangu and Dr Sipho Ndlovu (1; also a formative
    check-in). Desktop 1280×800 and phone 390×844.

THE RULES THE PAGE CARRIES TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Who is offered what. Start: a panel member who may sit now, a Coordinator of the panel's institution, or an
    Administrator. Everything else is the chair's alone, never an administrator's: the staging form, Remove, the
    agenda's Stage, Defer and Reinstate, the decision form, Record decision, Ratify and Close review. Others read
    "Only the panel's chair, Thandi Zulu, can stage entrustment decisions, defer agenda lines and record the committee's
    decision." (in progress), "Only the panel's chair, Thandi Zulu, can ratify the committee's decision." (decided; it
    can also name removing a decision that no longer fits, and deferring a line that blocks ratifying), and "Only the
    panel's chair, Thandi Zulu, can close this review." (a formative check-in). Controls a reader may not use are not
    drawn, never drawn disabled.
  - The agenda: one line per EPA due in this window ("15 EPAs for 2026 S2, 12 still to stage or defer"), each with its
    window (2026 S2 for six semester EPAs, 2026 for nine annual ones), a state badge and its detail, the evidence count,
    and the chair's action. States: Due ("Must be decided at this sitting, or deferred with a reason."), Due by year
    end, Partial period, As opportunity allows, Staged ("A decision is staged below."), Deferred ("Reason: …", with
    Reinstate), Decided ("STAR #n."), Not decided ("The review was ratified without deciding it."), Decided elsewhere
    ("Another sitting has decided it in this window, so it need not be decided here. A decision staged on it decides it
    again."). An EPA whose STAR was revoked mid-review: "PAED-0xx is no longer decided in its window: the STAR that
    decided it has been revoked. It is not on this agenda; stage a decision on it to decide it at this sitting."
  - Staging: EPA (with help "The EPAs on the trainee's curriculum."); Authorised level, the EPA's rungs with "Rungs of
    CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on."; Issued on (today); Expires on
    (optional); Rationale; "Evidence it rests on", ticks over the snapshot's lines with that EPA's own lines first.
    Stage pending decision stays disabled with "Name at least one item of the evidence snapshot." until one is ticked.
    Hints that refuse nothing: "Only one item is named. The College's rule is that an entrustment decision is never
    taken from a single form."; "None of the named items is about PAED-002."; "None of the named items was finished
    when the review started (each shows the state it was in), so the decision would rest on no completed assessment."
    Each staged decision lists its EPA, level, "issued 2026-10-05 · expires 2026-10-25" and "Rests on 2 items of the
    snapshot: …", with Remove ("Remove the staged decision on PAED-012").
  - Deferring: "Defer PAED-002", a reason box with "Kept on the agenda line. The trainee sees it on their reviews page
    once the review is ratified."
  - Recording: Category, opening on "Select a category…": Satisfactory Progress; Satisfactory with Observations;
    Inadequate Progress — Additional Training; Inadequate Progress — Repeat; Release from Training; Outcome Deferred;
    Graduate (programme complete). Rationale; Conditions; Present, one tick per member who may sit, the chair's ticked
    and locked, with "Tick every panel member at this sitting. A committee decision needs at least two panel members
    present: the chair and at least one other." Record decision is disabled until every Due line is staged or deferred:
    "Record decision: PAED-001, PAED-002, … and PAED-015 must be decided at this sitting. Stage a decision on each, or
    defer it with a reason." Enabled, it reads beside "Recording fixes the entrustment decisions staged below and the
    agenda's deferrals: they cannot be changed afterwards." A recorded decision shows its category as a heading, the
    rationale, "Conditions: …" and "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg
    (external)"; the staged list then reads "Fixed when the committee's decision was recorded: ratifying issues exactly
    these. Only a decision that no longer fits the trainee's curriculum can still be removed."
  - Ratifying: offered to the chair once Decided; blocked, disabled with "Ratify: …", while the quorum recorded falls
    short or a line blocks it. After it: "Nothing is pending: ratifying the review issued what was staged as STARs, and
    the agenda names each one." or "Nothing was staged when the review was ratified, so ratifying it issued no STAR."
    It sends no mail (T320): promise nothing to the registrar.
  - Refusals, each with its exact words; a refusal keeps everything typed: "An expiry date must be after the issue
    date."; "An entrustment decision on PAED-001 is already staged at this review. Remove it to stage another."; "Say
    why the committee is deferring the decision."; "A committee decision needs at least two panel members present: the
    chair and at least one other."; "This review decides the trainee's progression, so its decision records a
    progression category. Choose one."
  - Results: "Review started.", "Pending entrustment decision staged.", "Pending entrustment decision removed.",
    "PAED-002 deferred.", "PAED-002 reinstated: it is due at this sitting again.", "Decision recorded.", "Decision
    ratified.", "Formative review closed."
  - Notes the page may carry: "Sampling concentration warnings" (per EPA "1 rating · 1 assessor · 1 source" and
    "Fewer than three distinct assessors across this EPA's evidence.", with "Not counted: 2 records in the review window
    with no rating by a named assessor …"); "Sampling figures are incomplete" (danger); "The chair rated all of this
    evidence" (D46, refuses nothing); the sitting-order warning ("PAED-004 and PAED-005 are decided by {panel} and not
    yet decided for 2026 S2. A progression decision recorded now is taken without them."); the chair cannot act ("The
    panel's chair, Thandi Zulu, cannot take the chair's actions now: … A panel administrator must seat a chair who
    can."); "This review's trainee does not train at the panel's institution, so the panel cannot act on it."; an MSF
    campaign released after Start ("Starting now leaves out N feedback campaigns").
  - Variants: a pre-graduation review, whose standing adds the exit rule ("2 of 15 EPAs at their exit level by STAR
    decision (level 5: 2 of 9 · level 4: 0 of 6)", "Not yet: …", "For information only. Recording a Graduate decision
    or completing the programme does not check the exit rule."); an entrustment-only review (no category; "This review
    decides entrustment only. Its decision is the entrustment decisions staged below, and it records no progression
    category: …"; recorded as "Entrustment decisions only"); a formative check-in (Mode "Formative (interim check-in, no
    binding decision)"; Review, Evidence snapshot, standing, MSF and trajectory only; Close review; state Closed).
  - Data volumes: none (a snapshot of 0 items, "No evidence has been frozen yet …"; "No rating by a named assessor to
    chart in this review's window."); typical (Dr Molefe in Act 4: 8 snapshot lines, 3 staged, 9 deferred, 3 optional);
    heavy (her pre-graduation review: thirteen decisions staged on 15 agenda lines, a snapshot of three years).

WHAT FLOW 07 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01: the trail, the acting role, access denied, not found, the reconnect dialog);
    flow 05's EntrustmentStandingPanel and TrajectoryChart (their content, words, two drawings and table: this flow
    places them, and may say which EPA's chart shows where); flow 06's Homes and registrar page; flow 08's Committee
    reviews list, scheduling and Decision panels; flow 09's Appeals card body; the rules above (who may act, the
    quorum, what each refusal means, what Record fixes and Ratify issues).
  May, for this page: its whole structure (one page, steps or tabs within the route, a workspace per EPA), the order
    and grouping of its parts, its h1 and subtitle, card titles, where each result and refusal shows and where focus
    goes, confirmations, the staging, deferral and decision forms' layout, the agenda's and snapshot's presentation, a
    sticky or fixed region for the sitting's state, and a way back from a piece of evidence (which touches flow 03's
    activity page: mark it PROPOSED). Wording may change; the runbook's Expects change with it.
  Say which of these each variation changes, and why.

SCREENS, in order (today's single page; a variation may split it into steps, tabs or panes within its route):
  1. Before the sitting (Scheduled): Start offered; the standing, MSF, a 15-line agenda with no actions, the snapshot
     not yet frozen, the trajectories, the sampling warnings.
  2. In progress, the chair: the frozen snapshot; the agenda with Stage and Defer; the staging form and staged list;
     deferral; the decision form with Present; Record disabled with its reason, then enabled.
  3. Opening a piece of evidence from the snapshot (an activity, read-only; flow 03's page) and coming back to the same
     place.
  4. In progress, a member: everything the chair sees, none of her controls, and the sentence saying so.
  5. Decided: the chair (Ratify, with its confirmation) and an administrator (no Ratify).
  6. Ratified: STARs issued, the agenda's final states, the standing updated; and ratified with no STAR.
  7. The variants: pre-graduation with the exit rule (and Graduate recorded and ratified); entrustment-only; formative
     in progress and closed.
  8. Loading, not found ("The committee review could not be found among the reviews you can view."), 390 px.

STEPS: 4.14–4.32, 4.50, 5.3–5.6, A.7.6 and A.7.13. Pasted verbatim below (Role / Route / Do / Expect). In them, D is
  the day the story is replayed, <P> the semester holding D (for example 2026 S2) and <Y> its year; J is the latest
  15 January on or before D. Where an Expect quotes today's wording or layout, that is today's page, not a requirement
  on the new one.

STATES TO SHOW:
  The chair: scheduled; in progress; Stage chosen (focus in the form); one evidence item named (the hint); staged;
    expiry refused; duplicate refused; defer open; deferral with no reason; reinstated; every Due line settled (ready to
    record); no quorum; no category; decided (Ratify offered); Ratify blocked; ratified with STARs; ratified with no
    STAR; a staged decision with no evidence about its EPA; thirteen staged; Graduate recorded; Graduate ratified (every
    EPA at its exit level); formative in progress and closed; entrustment-only; an agenda line no longer decided (a STAR
    revoked mid-review); a line decided by another panel; the sitting-order warning; the chair can no longer act.
  A member: during the sitting; after recording. An administrator: decided, no Ratify.
  Each confirmation (whatever the variation confirms), its cancel, and the result after it with focus on it.
  Loading (the header and a skeleton, no action offered until the review has loaded); load error (a sentence and Try
    again, never an exception's text); not found; 390 px through the whole sitting.

REQUIREMENTS FROM KNOWN DEFECTS:
  - A5 (BRIEF.md § 6): every action reports its outcome where the focus goes and where the chair is working; a refusal
    shows beside the control that caused it, and keeps what was typed. Irreversible acts confirm and name their target.
  - T329: the header renders from the first moment, a skeleton while the review loads, a failed read as an alert with
    Try again; no action offered before the review has loaded.
  - T280: every card a named region; the dismiss button named ("Dismiss", not "×"), or no dismiss at all.
  - T273: the sampling list and the staged decisions mark an EPA no longer in force "(no longer in use)"; the frozen
    snapshot's headings stay as they were (decided 2026-09-25).
  - T325: "Issued on" defaults to today on the South African calendar; every date is ISO (flow 05's D1).
  - T309: a formative review reads "Formative check-in" wherever its type shows.
  - T260: a note about who may act (the chair cannot act; only the chair may …) is re-read after every action and
    never left stale; the chair's controls are not left offered after a refusal that changed what she may do.
  - T324: a missing rationale or issue date is refused in Wombat's words, never "The Rationale field is required.".
  - T320: ratifying sends no mail; the confirmation promises nothing to the registrar.
  - T356: a scrolled table never paints over the fixed top bar (seen on this page at Step 4.15).
  - T190 (done for the tab's case): the h1 and the tab name the registrar and the period, or the brief says why not.
  - Content (BRIEF.md § 6): sentence case; people by name as stored; states, types and categories by their labels; no
    system words; ISO dates; the rungs as the scale writes them (3a, 3b).
  - Done since the old brief, keep them: the trajectory is a card per EPA, drawn at 900 or 322 px, never scaled, its
    table always visible (T323's chart item, flow 05); the standing panel's EPA names link to their charts (T355);
    badge contrast (T322); the chair's controls hidden from everyone else (T213); the chair-cannot-act note (T256).

QUESTIONS THE DESIGN MUST ANSWER:
  1. One page, steps (read, stage, record, ratify), tabs, or a workspace per EPA? How does the chair keep her place
     across 15 agenda lines and 13 staged decisions, and how does a member keep up with where the chair is?
  2. What stays on screen while the chair stages one EPA: that EPA's standing and trajectory, its snapshot lines, the
     agenda, or the decision's fields alone?
  3. Where does the decision form belong, given it waits on every Due line? How does the page show who is present and
     whether the quorum is met before Record is pressed?
  4. Which acts confirm, and how: Start (freezes the evidence), Remove, Record (fixes what is staged and deferred),
     Ratify (issues STARs, no undo, no administrator bypass), Close review? What does each confirmation name?
  5. Where does each result and each refusal show, and where does the focus go after Stage, Defer, Reinstate, Remove,
     Record and Ratify?
  6. The way back from a piece of evidence: a return link on the activity page (flow 03's page: PROPOSED), the
     evidence opened in place, or something else?
  7. The page's title: does the h1 name the registrar and the period ("Lerato Molefe · 2026 S2")? What does the
     subtitle say?
  8. The notes (sampling warnings, incomplete figures, chair-rated, sitting order, chair cannot act): where does each
     sit, and is any dismissible?
  9. At 390 px: what does the chair do on a phone (A.7.6 reads, it does not stage), and what does the page show first?
  10. (The operator's) T329, T273, T325, T309, T260 and T324 touch only this page: fold them into the build, or land
     any first as its own task (flow 05's T304)?
  11. (The operator's) T260's open choice: refuse Start while the chair cannot act, or keep offering it?
  12. (The operator's) T314's "Export portfolio" for the registrar under review: in this flow, or flow 13's?

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
  Since flow 03 also: ActivityStatus, ActivityAbout, ActivityForm, ActivityWorkflowActions, SubmitCheck,
  RefusalSummary, ActivityHistory, NeedsYouList, InstrumentPicker, and DataTable's stacked rows (Stack). Since flow 04:
  the rung picker, WaitingList, WayOn, the Overdue badge and the other-role line. Since flow 05: the progress index
  (an EPA per row, its name its own link), the figure over its caption, EntrustmentStandingPanel's stacked rows, and
  TrajectoryChart's two drawings (wide and 322 px) with its visible table.
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
Also: the page has exactly one h1, and its headings are in order. A confirmation is flow 01's native <dialog>
  ConfirmDialog. Flow 05's TrajectoryChart and EntrustmentStandingPanel are placed, not redrawn.

ASK:
  - Round 1: 2–3 STRUCTURAL variations as wireframes of the review page through a whole sitting (screens 1–8), each
    covering its parts and their order (or its steps, tabs or panes), what stays on screen while the chair stages and
    while she records, where each result, refusal and confirmation shows, and the way back from evidence. Give each its
    reasoning and its step count per journey, at 1280 and 390 px: the chair's Act 4 sitting for Dr Molefe from opening
    the review to Ratify (Steps 4.15–4.27: start, open one piece of evidence and return, stage three, defer nine,
    record with four present, ratify); her pre-graduation sitting (5.3–5.6: thirteen staged); a member following it
    (4.22); the administrator reading it (4.26); the formative check-in (4.50). No colour or type choices yet. Stop and
    wait for my pick.
  - Round 2: the chosen structure at full fidelity on the design system's tokens, every screen and state above, at
    1280 and 390 px, the chair's and a member's view of each, every confirmation, and where the focus lands after each
    action.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (DESIGN.md § Committee agenda; § The chair's controls; § Entrustment-only reviews; § Detail page and its
    details-grid; § Page-level patterns and its title rule; the owner table's row for a committee review).
  - Mark PROPOSED anything not built: a confirmation, a return link on the activity page, a new route or step.
  - Flag edge cases: a snapshot of nothing; thirteen staged; 15 lines all Due; a decision resting on one item or on no
    item about its EPA; an EPA no longer in force; the chair cannot act; the trainee at another institution; a member
    who opens the review mid-sitting; a panel of two (the chair and one member); the 120-character EPA title ("PAED-015 —
    Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and research").
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every card a named region, every row
    action named for its row, badges read as words, the chart's table, focus after every action and every
    confirmation; targets ≥ 24 px (44 at 390). Step A.7.13 walks this page with a screen reader.

ATTACHED: act-4/4.15-1-zulu-molefe-review-before-start.png, act-4/4.16-1-zulu-review-started.png,
  act-4/4.17-1-zulu-minicex-from-snapshot.png, act-4/4.19-1-zulu-three-staged.png,
  act-4/4.23-1-zulu-quorum-refused.png, act-4/4.21-2-zulu-ready-to-record.png,
  act-4/4.22-1-naidoo-sitting-no-controls.png, act-4/4.26-1-mbatha-decided-no-ratify.png,
  act-4/4.27-1-zulu-ratified-stars-issued.png, act-5/5.3-2-zulu-review-full.png,
  act-5/5.4-4-zulu-thirteen-staged-full.png, act-4/4.50-1-zulu-formative-started.png, act-A/A.7.6-3-review-390.png,
  act-A/A.7.13-1-review-7.png; flow 05's boards R3-T-committee and R3-P-standing, each at 1280 and 390, with
  flow05-r3.css.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 4.14 | `/committee/panels` | Dr Botha | who sits and which EPAs the panel decides; PAED-004 and 005 fall back to it (flow 08's page) |
| 4.15 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | before Start: Start offered, no decision yet, the standing (0 of 15), MSF coverage, a 15-line agenda (12 Due, 3 optional) with no actions, no snapshot yet, three trajectories, the sampling warnings; 9,192 px |
| 4.16 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | "Review started." at the top; the snapshot frozen and grouped; Stage and Defer per line; the decision form with Present; Record disabled with its reason; nothing asked her to confirm the freeze |
| 4.17 | `…/{ReviewId:int}` → `/activities/{ActivityId:int}` → `…/{ReviewId:int}` | Dr Zulu | her own Mini-CEX read-only, then Back by the browser to the top of the review; Stage puts the focus in the EPA select below the agenda; the ladder; the "one item" hint; the result at the top |
| 4.18 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | the expiry refusal at the top of the page, the form below keeping what she typed |
| 4.19 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | three staged; Remove without confirmation; the caption counting down |
| 4.20 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | the duplicate refusal at the top |
| 4.21 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | "Defer PAED-002" under the table; the empty reason refused in the form; Reinstate; nine deferred; Record enabled, about 3,000 px up |
| 4.22 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Naidoo | everything the chair sees, no controls, the sentence saying only the chair acts |
| 4.23 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | the quorum refusal at the top; nothing before pressing said the quorum was short |
| 4.24 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | the category refusal |
| 4.25 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | "Decision recorded." with no confirmation; the Present line; the staged decisions fixed; Ratify offered |
| 4.26 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Prof Mbatha | the decided review; no Ratify; "Only the panel's chair, Thandi Zulu, can ratify the committee's decision." |
| 4.27 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | "Decision ratified." with no confirmation; STARs #1–#3; the agenda's final states; the standing updated |
| 4.28–4.32 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | four more sittings: a decision on items not about its EPA (a hint); no STAR; Additional Training; Outcome Deferred, a declined request as evidence |
| 4.50 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | a formative check-in started and closed; Type still "Annual progression review" (T309) |
| 5.3 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu | the pre-graduation review: three years of evidence; the exit rule, which gates nothing |
| 5.4 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | thirteen decisions staged one after another; 14,834 px |
| 5.5–5.6 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | Graduate recorded and ratified; "15 of 15 … Every EPA is at its exit level." |
| A.7.6 | `/` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Zulu at 390 px | the cards stacked, 18,013 px; tables scrolling in their boxes; each chart its own card at 322 px |
| A.7.13 | `/committee/reviews/{ReviewId:int}` | Dr Zulu, screen reader | h1 then h3s; four of nine cards named; the "×" dismiss unnamed |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Committee. The step captures were
re-taken by the T358 replay on 2026-10-05 on the shell as built. The `states/review-detail--*` captures are from
2026-09-26, on the old frame (a flat Title Case sidebar, an email in the top bar), except `--scheduled` and `--narrow`
(2026-10-04): brief from their page body, not the frame. Where a state has a step capture, brief from that.

| State | Screenshot | Reached at |
|---|---|---|
| Scheduled, the chair | `act-4/4.15-1-zulu-molefe-review-before-start.png` (`states/review-detail--scheduled.png`) | Step 4.15 |
| In progress, the chair | `act-4/4.16-1-zulu-review-started.png` (`states/review-detail--in-progress.png`) | Step 4.16 |
| Evidence opened from the snapshot | `act-4/4.17-1-zulu-minicex-from-snapshot.png` | Step 4.17 |
| One item named; staged | `act-4/4.17-2-zulu-paed001-staged.png` (`states/review-detail--single-item.png`, `--staged.png`) | Step 4.17 |
| Expiry refused | `act-4/4.18-1-zulu-expiry-refused.png` (`states/review-detail--expiry-refused.png`) | Step 4.18 |
| Three staged | `act-4/4.19-1-zulu-three-staged.png` | Step 4.19 |
| Second decision refused | `act-4/4.20-1-zulu-duplicate-refused.png` (`states/review-detail--duplicate-refused.png`) | Step 4.20 |
| Deferral form, no reason | `act-4/4.21-1-zulu-defer-form.png` (`states/review-detail--defer-empty.png`) | Step 4.21 |
| Ready to record | `act-4/4.21-2-zulu-ready-to-record.png` (`states/review-detail--ready-to-record.png`) | Step 4.21 |
| A member during the sitting | `act-4/4.22-1-naidoo-sitting-no-controls.png` (`states/review-detail--member.png`) | Step 4.22 |
| No quorum | `act-4/4.23-1-zulu-quorum-refused.png` (`states/review-detail--quorum-refused.png`) | Step 4.23 |
| No category | `act-4/4.24-1-zulu-category-refused.png` (`states/review-detail--category-refused.png`) | Step 4.24 |
| Decided, the chair | `act-4/4.25-1-zulu-decision-recorded.png` (`states/review-detail--decided.png`) | Step 4.25 |
| Decided, an administrator | `act-4/4.26-1-mbatha-decided-no-ratify.png` (`states/review-detail--decided-admin.png`) | Step 4.26 |
| Ratified, STARs issued | `act-4/4.27-1-zulu-ratified-stars-issued.png` (`states/review-detail--ratified.png`) | Step 4.27 |
| No item about the EPA | `act-5/5.4-1-zulu-stage-paed002-hint.png`, `act-4/4.28-1-zulu-dlamini-staged-and-deferred.png` (`states/review-detail--none-about-epa.png`) | Step 4.28 |
| Ratified, no STAR | `act-4/4.30-1-zulu-duplessis-ratified-no-star.png` (`states/review-detail--ratified-no-star.png`) | Step 4.30 |
| Formative, in progress and closed | `act-4/4.50-1-zulu-formative-started.png`, `4.50-2-naidoo-formative-in-progress.png`, `4.50-3-zulu-formative-closed.png` (`states/review-detail--formative-in-progress.png`, `--formative-closed.png`) | Step 4.50 |
| Pre-graduation, in progress | `act-5/5.3-1-zulu-review-started.png`, `5.3-2-zulu-review-full.png` (`states/review-detail--pre-graduation.png`) | Step 5.3 |
| Thirteen staged | `act-5/5.4-4-zulu-thirteen-staged-full.png` (14,834 px) | Step 5.4 |
| Graduate recorded | `act-5/5.5-2-zulu-graduate-decided.png`, `5.5-3-naidoo-no-ratify.png` (`states/review-detail--graduate.png`) | Step 5.5 |
| Every EPA at its exit level | `act-5/5.6-2-zulu-exit-rule-met.png`, `5.6-3-zulu-ratified-full.png` (`states/review-detail--graduate-ratified.png`) | Step 5.6 |
| 390 px | `act-A/A.7.6-3-review-390.png` (`states/review-detail--narrow.png`), both 390×18,013 | Step A.7.6 |
| Populated, for the screen reader | `act-A/A.7.13-1-review-7.png` | Step A.7.13 |
| The chair can no longer act | `states/review-detail--chair-cannot-act.png` (old frame) | Scratch (post-act4) |
| Agenda: no longer decided | `states/review-detail--no-longer-decided.png` (old frame) | Scratch (post-act4) |
| Ratify blocked | `states/review-detail--ratify-blocked.png` (old frame) | Scratch (post-act4) |
| Entrustment-only review | `states/review-detail--entrustment-only.png` (old frame) | Scratch (post-act4) |
| Decided by another panel | `states/review-detail--decided-elsewhere.png` (old frame) | The same scratch run |
| Loading | `states/review-detail--loading.png` (old frame) | A held read: the header and blank space (T329) |
| Not found | `states/review-detail--not-found.png` (old frame) | Typed `/committee/reviews/999999` |

Not captured, so describe them in words (`states.md`):
- "The chair rated all of this evidence" (D46 warns and refuses nothing).
- "Sampling figures are incomplete" (the danger alert, when a source cannot be counted).
- The trainee now at another institution ("This review's trainee does not train at the panel's institution, …").
- "Starting now leaves out N feedback campaigns" (an MSF campaign released after Start).
- The sitting-order warning (another panel decides some EPAs and has not yet).
- Not played at all (`coverage.md` § Flows and states not played): the Administrator on a review (no ratify or appeal
  bypass, D46), Defer after recording, and Remove after recording when a decision no longer fits.
- Any confirmation: none exists today.

**Belongs to other flows:** flow 08: scheduling, the scheduled review with no Start (`--scheduled-no-start`), the
formative review as scheduled (`--formative`), the Committee reviews list and Decision panels. Flow 09: the Appeals
card's states (`--appeal-*`, `--remit-refused`, `--remitted`; `--appeal-upheld` no longer exists, D51). Flow 14: a review
withdrawn by an erasure (`--withdrawn`). Flow 03: the activity page's body.

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-10-11; none shows a
link or a password. Most are full pages of 9,000–15,000 px, which the canvas will see scaled: say so in the message.

**Key screenshots (attach these first, with § 1):**
1. `act-4/4.15-1-zulu-molefe-review-before-start.png`: the whole page before Start (9,192 px): the sampling warnings,
   Review with Start beside an empty Decision card, the standing, MSF, a 15-line agenda, no snapshot, the charts
2. `act-4/4.16-1-zulu-review-started.png`: just started: "Review started." at the top, the decision form live, Stage and
   Defer on each line, the staging form open, the snapshot frozen (11,033 px)
3. `act-4/4.17-1-zulu-minicex-from-snapshot.png`: a piece of evidence opened from the snapshot, read-only, no way back
4. `act-4/4.19-1-zulu-three-staged.png`: three decisions staged, the agenda's Staged badges
5. `act-4/4.23-1-zulu-quorum-refused.png`: the quorum refusal at the top, far from the filled decision form
6. `act-4/4.21-2-zulu-ready-to-record.png`: every Due line settled, Record enabled
7. `act-4/4.22-1-naidoo-sitting-no-controls.png`: a member's view: no Stage, Defer or decision form
8. `act-4/4.26-1-mbatha-decided-no-ratify.png`: an administrator on a decided review: "Only the panel's chair, Thandi
   Zulu, can ratify the committee's decision."
9. `act-4/4.27-1-zulu-ratified-stars-issued.png`: ratified: STARs issued, the agenda's Decided badges
10. `act-5/5.3-2-zulu-review-full.png`: the pre-graduation review: the standing with the exit rule
11. `act-5/5.4-4-zulu-thirteen-staged-full.png`: thirteen staged (14,834 px)
12. `act-4/4.50-1-zulu-formative-started.png`: a formative check-in in progress, Close review offered
13. `act-A/A.7.6-3-review-390.png`: the review at 390 px. It is 18,013 px tall, past Chromium's 16,384 px limit, so the
    header repeats near its foot: an artefact of the capture, not of the page (§ 11, flow 05)
14. `act-A/A.7.13-1-review-7.png`: a populated review at 1280, the headings and the "×" dismiss (Step A.7.13)

**States (attach as the chat asks):** every row of § 3 not already attached above, flagging the old-frame ones, and
`act-5/5.3-1-zulu-review-started.png` and `act-5/5.6-2-zulu-exit-rule-met.png` (each the 800 px top of a review, legible at full size).

**Also, for what the page places:** flow 05's round 3 boards `R3-T-committee-1280.dc.html` and `-390` (the trajectory
over a review window) and `R3-P-standing-1280.dc.html` and `-390` (the standing panel), from
`design/flows/05-trainee-progress/round-3/project/`, with `flow05-r3.css`. Stage them by hand in
`design/upload/flow05-boards/`; the staging script does not stage them.

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `bc9a6c4b`.

| Task | What it means for the design | Evidence |
|---|---|---|
| **A5** (BRIEF § 6; no task) | `ActionResult` is at the top of the page (`ReviewDetail.razor:16–26`); a success focuses it (`ExecuteAsync` `:1568–1589`; Stage `:1369`, Remove `:1521`, Defer `:1848`, Reinstate `:1884`), about 6,000 px from the staging form at 1280; a refusal leaves the focus where it was, its alert out of view. An empty deferral is the one refusal shown in its form (`:1815`). Each outcome shows, and takes the focus, where the chair is working. | `act-4/4.23-1-zulu-quorum-refused.png` |
| **Confirmations** (no task; T264's item here was the Remove name, met by T239) | The page has no `ConfirmDialog` (grep at `bc9a6c4b`): Start, Remove, Record, Ratify and Close review act on one press. | Steps 4.16, 4.19, 4.25, 4.27, 4.50 |
| **T329** (P2) | No loading flag, `Skeleton` or `StatePanel`; the body is gated on `_review is not null` (`:113`). Not found is `RefusalText` in the top alert (`A/Features/CommitteeDecisions/CommitteeDecisionAuthorization.cs:557`), under the h1, with no body. | `states/review-detail--loading.png`, `--not-found.png` |
| **T280** (P3) | `Alert.razor:8` renders the dismiss as "×" with no name; the sampling card is its only `Dismissible` caller. Review, Decision, Entrustment against Annexure A, Pending and Appeals are not named regions; MSF (`:1554`), Agenda (`agenda-heading`), the Evidence snapshot and the Trajectory section (`trajectory-section-h`, `:707–726`) are. | `act-A/A.7.13-1-review-7.png` |
| **T273** (P3) | The sampling list and the staged lines print a bare "Code — Title" for an EPA no longer in force; DESIGN.md:858–867 lists them as the surfaces not yet marked. The snapshot's headings stay unmarked (adopted 2026-09-25). | — |
| **T325** (P3) | "Issued on" defaults to `DateOnly.FromDateTime(DateTime.UtcNow)` (`:2152`). | Step 4.17 |
| **T309** (P3) | A formative check-in's Type is `ReviewTypeLabel` (`A/…/CommitteeDecisionWording.cs:120–126`), with no formative label: "Annual progression review". | `act-4/4.50-1-zulu-formative-started.png` |
| **T260** (P3) | The chair's controls stay offered after a refusal (no re-read after a refusal), and Start is offered while `ChairCannotAct`; whether to refuse Start then is undecided (question 11). | `states/review-detail--chair-cannot-act.png` |
| **T324** (P3) | The decision form's and the staging form's Rationale, and Issued on, are `[Required]` with no message (inferred: "The Rationale field is required."). | — |
| **T320** (P2) | Ratifying issues STARs and sends no mail. A confirmation must not promise that the registrar is told. | Step 4.27 |
| **T356** (P2) | A scrolled table paints over the fixed top bar, covering Sign out; found on this page at Step 4.15. | T356's file |
| **T190** (done) | The tab reads "Committee review · Wombat" (`:7`); the h1 "Committee review", the subtitle "Evidence bundle, decision record, ratification, and appeals in one place." (`:9`). Naming the registrar and the period was left to this flow. Headings go h1, h3, h4: no h2. | `act-A/A.7.13-1-review-7.png` |
| **T314** (P3) | No "Export portfolio" for the registrar under review; the list's Trainee column links nowhere (`ReviewsSchedule.razor`). Question 12. | — |
| Layout (observed, not filed) | `details-grid` is `1fr 2fr` (`app.css:2159–2163`): Review in the narrow column beside Decision, Pending beside the Evidence snapshot. The decision form (`:207–346`) is above the standing, MSF and agenda (`:1542–2070`), so Record sits about 3,000 px above the lines it waits on. Choosing Stage sets the staging form's EPA and focuses it (`:1745–1762`), below the agenda. DESIGN.md:2050–2133 (§ Committee agenda) places the agenda "directly above" Pending so the chair "reads what is due and stages it in one place". | `act-4/4.16-1-zulu-review-started.png` |
| The way back (observed, not filed) | The activity page opened from the snapshot has no return link or return URL; its owner table has no row for a committee reader (`NavOwners.cs:94–103`), so (inferred) the trail is Home › the activity; `WayOn`'s "Back to …" shows only after a move (`ActivityView.razor:91–95`, `:795`), which a reader never makes. Only the browser's Back returns, to the review's top (inferred). | `act-4/4.17-1-zulu-minicex-from-snapshot.png` |
| Quorum (observed) | Present lists only members who may sit, the chair ticked and locked; the rule is in the help (`:1248–1250`) and known short only from the refusal. Ratify is blocked with "Ratify: …" when the recorded quorum falls short (`D/CommitteeDecisions/CommitteeReview.cs:515–536`). | Step 4.23 |

**Dropped from the old brief (fixed since):** the trajectory charts' 5–6 px labels and the screen-reader-only table
(T323's chart item, flow 05: two fixed drawings, the table always visible); badge contrast (T322); the tab's Title Case
(T190); the stale line numbers (`ReviewDetail.razor` is 2,162 lines, Issued on at `:2152`).

## 6. Questions the design must answer

1. **One page, steps, tabs, or a workspace per EPA?** How the chair keeps her place across 15 lines and 13 staged
   decisions (`act-5/5.4-4-zulu-thirteen-staged-full.png`), and how a member keeps up.
2. **What stays on screen while the chair stages:** the EPA's standing and trajectory, its snapshot lines, the agenda.
3. **The decision form's place,** and how the page shows who is present and whether the quorum is met before Record
   (D46: the chair and at least one other).
4. **Confirmations:** which of Start, Remove, Record, Ratify and Close review confirm, and what each names. Ratifying
   issues STARs, "immutable once issued" (`DOMAIN.md:78`), with no undo and no administrator bypass (D46).
5. **Results and refusals:** where each shows and where the focus goes (A5).
6. **The way back from evidence:** a return link on the activity page (flow 03's page: PROPOSED), the evidence opened
   in place, or something else.
7. **The title:** an h1 that names the registrar and the period, and a subtitle that says something.
8. **The notes:** where each sits, and whether any is dismissible (T280).
9. **390 px:** what the chair does on a phone, and what shows first.
10. **(The operator's) T329, T273, T325, T309, T260 and T324:** folded into the build, or any landed first as its own
    task?
11. **(The operator's) T260:** refuse Start while the chair cannot act, or keep offering it?
12. **(The operator's) T314:** "Export portfolio" for the registrar under review in this flow, or in flow 13?

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps span Acts 4, 5 and the appendix; § 9): every Expect
  must hold, the refusals' exact words included. **Rewrite the steps that change:** a confirmation adds a press to
  4.16, 4.19, 4.25, 4.27, 4.29–4.32, 4.50, 5.5 and 5.6 (and to flow 09's and flow 14's steps on this page, if they act
  through it); a layout of steps or tabs changes the Route and Do lines of 4.15–4.32, 5.3–5.6 and A.7.6; a new title
  changes every Expect that quotes "Committee review"; search the whole runbook (BRIEF.md § 9 item 7). A new page or
  route needs a step (`Scenario/ScenarioRunbookTests`). A new step takes a letter when its number is taken (§ 11, flow
  06).
- **Checks the design adds:** 4.15 with a held read, the header and a skeleton and no Start until loaded (T329); 4.17,
  the way back lands where she left; 4.18 and 4.23, each refusal beside its control, with the focus on it; 4.17 and
  4.19, each result where the chair works; 4.25 and 4.27, Record and Ratify confirm and name what they fix or issue;
  4.50, "Formative check-in" (T309); A.7.6, nothing scrolls sideways at 390, and the page checked at 768 as well;
  A.7.13, one h1, headings in order, every card a named region, no "×" (T280).
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include:
  - `CommitteeDecisions/ReviewDetail*Tests` (Agenda, AppealForm, ChairControls, EvidenceEmptyState, EvidenceGrouping,
    EvidencePicker, MsfOutsideSnapshot, Quorum, SamplingSentence), `EntrustmentStandingPanelTests`,
    `EntrustmentOnlyReviewPageTests`, `CommitteeReviewStateLabelTests`, `WithdrawnReviewPageTests`;
  - `Charts/TrajectoryChartTests` and `TrajectoryWordsTests`;
  - `Accessibility/ActionFocusTests`, `RowNamesTests`, `AccessibleNamesTests`, `IdReferencesTests`;
  - `Design/NarrowLayoutTests`, `DefinedClassTests`, `AlertRoleTests`, `ContrastTests`, `FieldGroupTests`,
    `RowActionMarkupTests`; `Hosting/PageTitleTests`; `Navigation/ActiveNavItemTests`;
  - `Epas/EpaNoLongerInUseMarkTests` (T273); `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones on scratch copies, as `states.md` says) and the steps' own
  captures, keeping their names; `check_baseline_paths.py` at missing 0. A page taller than about 16,384 px is captured
  in parts and stitched (§ 11, flow 05). Compare them with the chosen artboards.
- **Browser check** at 1280, 768 and 390 px as the chair, a member, the external member and the InstitutionalAdmin.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as flow 07's build left it (T361, 2026-10-12); § 2 records what each step showed before it, and
round 1 was sent the steps as they stood on 2026-10-11, after flow 06's build. Step 4.21, the deferral, is removed with
deferral itself (D53), so 25 steps remain; the numbers after it are kept. `D` is the replay day.

```text
Step 4.14 — Dr Botha checks who sits and who decides (act-4-annual-review.md:327)
Role: CommitteeMember — Dr Sarah Botha
Route: /committee/panels
Do: Open Decision panels.
Expect:
  - **The list:** `Paed Annual Review Panel` with Scope Speciality, Decides for General panel and Members 4. There is no
    New panel button and no Edit column: she manages no panel (T239, T256).
  - **Who decides each EPA:** curriculum 11.1's EPAs are decided by `Paed Annual Review Panel`. PAED-004 and 005 fall
    back to it, because no panel covering the programme sits as the neonatal CCC.

Step 4.15 — Dr Zulu reads Dr Molefe's review before starting it (act-4-annual-review.md:342)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews and Dr Molefe's review. Read every section before starting it.
Expect: The page is one column, the agenda its spine (T361, round 3):
  - **The header:** the h1 and the tab read "Lerato Molefe · `<P>`"; the subtitle "Annual progression review · Paed
    Annual Review Panel · Training year 4", with the link "Lerato Molefe's progress" to her registrar page under it
    (round 3 item 19). There is no Review card and no Pending card.
  - **The chair's sitting bar,** under the title: Scheduled, "15 EPAs on the agenda", "Summative · evidence
    `<Y>`-01-01 to `<Y>`-12-31", and Start review… (round 3 item 28).
  - **Decision:** "No decision has been recorded yet.", under the agenda.
  - **Entrustment against Annexure A** reads training year 4 on `D`, and "0 at or above · 0 below · 15 with no decision,
    of 15 EPAs". The exit rule reads "0 of 15 EPAs at their exit level by STAR decision", names all fifteen as "Not
    yet", and says it is for information only (T166). The names of PAED-001, PAED-010 and PAED-012 link to their charts
    on the page (T355).
  - **Multi-source feedback by EPA** marks Covered, in the semester Act 3's campaign closed in, each EPA that campaign
    covered. The other cells read "None released" or "None released yet" (T168).
  - **Agenda,** captioned "15 EPAs for `<P>`", lists the 15 EPAs, each its code over its title:
    - window `<P>` for PAED-001, 002, 004, 005, 010 and 012 and `<Y>` for the other nine (round 3 item 24);
    - twelve "Due at this sitting", with no sentence under the badge (D53);
    - three "As opportunity allows", each "Decided as opportunity allows: optional.";
    - Evidence 0 items each;
    - no Action column, because the review has not started.
  - **Evidence snapshot:** "No evidence has been frozen yet. Starting the review captures the current bundle."
  - **Rating trajectory by EPA** charts PAED-001, PAED-010 and PAED-012 on the ladder's rungs, each over the review's
    window and with no Today. Each chart's line names the trainee, the window and the assessors, PAED-001's reading
    "Lerato Molefe · 3 ratings in the review window, `<Y>`-01-01 to `<Y>`-12-31, from Thandi Zulu, David Naidoo and
    Mohammed Patel. 2 at the minimum, 1 below.", and the table of its ratings is under it (T355, T323).
  - **Before the committee decides,** the first section under the bar, not dismissible (T280): its "Sampling
    concentration warnings" read "PAED-010 · 1 rating · 1 assessor · 1 source. Fewer than three distinct assessors
    across this EPA's evidence." and "PAED-012 · 2 ratings · 2 assessors · 2 sources. Fewer than three distinct
    assessors across this EPA's evidence." (round 3 item 31). PAED-001, rated by three assessors on three instruments, is
    not named. "Not counted: 2 records in the review window with no rating by a named assessor …" says her two MSF
    records are not counted (T135, D44).
  - **Appeals:** "No appeal has been lodged against this review."

Step 4.16 — Dr Zulu starts the review (act-4-annual-review.md:398)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Start review… in the sitting bar, read the dialog, and start the review.
Expect:
  - **The dialog** is titled "Start Lerato Molefe's review for `<P>`?" and reads "Starting freezes the evidence: the 9
    items observed from `<Y>`-01-01 to `<Y>`-12-31 (8 activities and the MSF campaign report) become this review's
    snapshot, each in the state it is in now. Anything finished later is not added." (round 3 item 11). No sentence
    about a campaign left out: the campaign is released (T173). The focus is on its safe button, "Not yet"; the confirm
    is "Start review".
  - **Started:** "Review started." atop the agenda, with the focus there. The bar reads In progress, "None staged",
    "Present: the chair · one more member needed" and Go to the decision.
  - **Agenda:** its caption reads "15 EPAs for `<P>` · none staged". Each line counts its snapshot items and offers
    Stage only (D53).
  - **Evidence snapshot:** "Frozen when the review started, `D` · 9 items", an h3 per EPA ("PAED-001 — …"), its lines
    by instrument A–Z (round 3 item 30). Every state is listed, and each line links to its activity and shows its
    assessor, rating, encounter date and state. The MSF campaign's report sits under "Not about a single EPA", and its
    per-EPA records are listed under each EPA.
  - **Decision,** under the agenda: a form with these fields:
    - Category, opening on "Select a category…";
    - Rationale;
    - "About the EPAs not decided (optional)", with the help "The registrar sees this once the review is ratified.";
    - Conditions;
    - Present, listing Thandi Zulu (chair) ticked and locked, David Naidoo, Sarah Botha and John van Rensburg
      (external).
  - **Record decision…** is unavailable (aria-disabled, not natively disabled, round 3 item 16), with the quorum line
    beside it: "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and
    at least one other." It names no EPA: no line holds Record back (D53).
  - **Start** is also offered to a member who may sit, the Coordinator and the Administrator, never while the chair
    cannot act (E2).

Step 4.17 — Dr Zulu stages PAED-001 at level 5 (act-4-annual-review.md:445)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int} → /activities/{ActivityId:int} → /committee/reviews/{ReviewId:int}
Do: Open Dr Zulu's own PAED-001 Mini-CEX from the evidence snapshot to re-read it, then come back to the review by its
  return link. Choose Stage on PAED-001's agenda line and fill in:
  - Authorised level `5`;
  - Issued on `D`, as filled;
  - no expiry;
  - the rationale "Leads resuscitation and emergency care without prompting; ready for unsupervised practice."
  Tick the Mini-CEX, then Dr Naidoo's CBD, and stage the pending decision.
Expect:
  - **The snapshot line** "Mini-CEX (Paediatrics) #12" opens the Mini-CEX at
    `/activities/{ActivityId}?from=review-{ReviewId}&at=snap-{itemId}`, read-only: its status card, badged Completed,
    reads "Done. You completed it on …" and "Rated 5. Credited 1 item to PAED-001.", and no move is offered (T342). Its
    only back link is "Back to Lerato Molefe · `<P>`" (round 3 item 9), which returns to the review at that snapshot
    line (`#snap-{itemId}`).
  - **Choosing Stage** opens PAED-001's line in place, under its row, headed "Stage a decision on PAED-001", with the
    focus on Authorised level. Its standing reads "Year 4 target 5 · no STAR yet · 3 ratings in the review window · MSF:
    None released yet" (round 3 item 26), with the link "Rating chart for PAED-001".
  - **The level select** offers the rungs 1, 2, 3a, 3b, 4 and 5 under "CPSA Paediatric Entrustment Scale v11.1", with
    the help "Rungs of CPSA Paediatric Entrustment Scale v11.1, the ladder this EPA is assessed on." (T076, T167)
  - **"Evidence it rests on"** lists PAED-001's three lines first, by instrument A–Z: Case-Based Discussion
    (Paediatrics) #13, DOPS (Paediatrics) #14 and Mini-CEX (Paediatrics) #12, each with a tick and Read; the other
    lines sit behind "Show the other N snapshot lines". Read shows an activity in place of the form, and "Back to
    PAED-001" restores the form as typed (round 3 item 8). Stage pending decision stays unavailable with "Name at least
    one item of the evidence snapshot." beside it until a line is ticked (D38, round 3 item 16).
  - **With one line ticked,** the picker says "Only one item is named. The College's rule is that an entrustment
    decision is never taken from a single form." It still allows the stage.
  - **After staging:** "Pending entrustment decision staged." under PAED-001's line, with the focus there. The line reads
    Staged, "Level 5 · issued `D` · no expiry." and "Rests on 2 items of the snapshot: Mini-CEX (Paediatrics) #12;
    Case-Based Discussion (Paediatrics) #13." (round 3 item 30), with Remove… in its Action cell. The caption and the
    bar read "1 staged".

Step 4.18 — An expiry before the issue date is refused (act-4-annual-review.md:498)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start staging PAED-010: choose its Stage, then level `4`, Issued on `D`, Expires on `D−1`, the rationale "Leads the
  ward round safely; one observation so far, to be confirmed at her final review." and both PAED-010 lines (the Direct
  Observation and the MSF record). Stage it.
Expect: PAED-010's open line lists its two lines first: the Direct Observation [#n] (Thandi Zulu, rated 5) and the MSF
  record #22 ("Not recorded"). Refused before anything is sent: "Not staged. An expiry date must be after the issue
  date." at the top of the line's form, with the focus there, and Expires on marked with "An expiry date must be after
  the issue date." (round 3 item 15). Nothing is staged, the form keeps what was typed, PAED-010 still reads Due at this
  sitting, and the bar still reads "1 staged".

Step 4.19 — Dr Zulu stages PAED-010 with an expiry, and PAED-012 (act-4-annual-review.md:516)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Correct PAED-010's Expires on to `D+20` and stage it. Then stage PAED-012 at `5` with no expiry, the rationale
  "Breaks difficult news clearly and kindly; consistent across settings." and its two WBA lines. Remove the PAED-012
  decision, confirming in its dialog, then stage it again the same way.
Expect: Both are staged, each with "Pending entrustment decision staged." under its line:
  - PAED-010 reads "Level 4 · issued `D` · expires `D+20`.";
  - PAED-012 reads "Level 5 · issued `D` · no expiry." on Mini-CEX (Paediatrics) #15 and Case-Based Discussion
    (Paediatrics) #16 (its Evidence 3 items).
  The bar reads "3 staged".
  - **Remove…** (an outline button, round 3 item 17) is named "Remove the staged decision on PAED-012". It opens a
    dialog titled "Remove the staged decision on PAED-012?", reading "Its level 5, its rationale and the 2 snapshot
    items it rests on are discarded. PAED-012 is due at this sitting again.", with the focus on its safe button "Keep
    it" (round 3 item 22), which would close it and change nothing. "Remove the decision" answers "Pending entrustment
    decision removed." under the line, PAED-012's line reads Due at this sitting with Stage, and the bar reads "2
    staged".
  - **Staged again,** it reads Staged, and the bar reads "3 staged".

Step 4.20 — A second decision on PAED-001 is refused (act-4-annual-review.md:543)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: In "Stage a decision on an EPA not on this agenda", at the agenda's foot, choose PAED-001 from the EPA select, then
  fill in level `4`, a rationale and one line, DOPS (Paediatrics) #14 (rated 4, Mohammed Patel). Stage it.
Expect: Refused: "Not staged. An entrustment decision on PAED-001 is already staged at this review. Remove it to stage
  another." at the top of that form, with the focus there. PAED-001's line still holds exactly one decision, "Level 5
  · issued `D` · no expiry.", and the bar still reads "3 staged".

Step 4.22 — Dr Naidoo reads the sitting without the chair's controls (act-4-annual-review.md:561)
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Molefe's review while it is in progress, and press Refresh in its sitting bar.
Expect: Dr Naidoo sees everything the chair sees:
  - the snapshot;
  - the agenda with its Staged and "Due at this sitting" badges, and each staged decision and its evidence in its line;
  - the standing, MSF coverage, trajectory and "Before the committee decides" sections.
  Dr Naidoo is offered none of the chair's controls (T213):
  - the agenda has four columns, with no Action column;
  - there is no Stage, no Remove…, no foot form and no decision form.
  The sitting bar is a reader's, static under the title (E8): In progress, "3 staged", the sentence "Only the panel's
  chair, Thandi Zulu, can stage entrustment decisions and record the committee's decision." (D53), and "As of hh:mm
  SAST" with Refresh. Refresh reads the review again in place, keeping the scroll and the focus, and says what changed:
  with nothing changed, "As of hh:mm SAST. No change." (round 3 item 21). Decision reads "No decision has been
  recorded yet."

Step 4.23 — Recording without a second member present is refused (act-4-annual-review.md:586)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Go to the decision in the sitting bar. Choose the category Satisfactory Progress and write a rationale. Tick
  nobody besides Dr Zulu under Present, and press Record decision….
Expect: Go to the decision moves the focus to the Decision heading. With only the chair ticked, Record decision… cannot
  be pressed to any effect: it is unavailable (aria-disabled, round 3 item 16), and the quorum line beside it reads
  "Only the chair is ticked. A committee decision needs at least two panel members present: the chair and at least one
  other." (T165, D46). Pressing it moves the focus to that line, opens no dialog and sends nothing, so the audit log
  gains no row (Step 4.38). The review stays In progress, with nothing recorded; the server's refusal stays only as
  the backstop.

Step 4.24 — Recording without a category is refused (act-4-annual-review.md:604)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Tick David Naidoo, Sarah Botha and John van Rensburg as present. Set Category back to "Select a category…" and
  press Record decision….
Expect: The quorum line reads "4 present: the chair and 3 others. The quorum is met." and the bar "4 present · quorum
  met"; Record decision… is available, with the hint "Recording fixes the 3 staged decisions, and records the 12 EPAs
  not staged as not decided." beside it. Pressed, it is refused before any dialog (T324): "Not recorded. This review
  decides the trainee's progression, so its decision records a progression category. Choose one." at the top of the
  Decision section, with the focus there, and Category marked "Choose a progression category." (T131 slice 5). Nothing
  is sent and nothing is recorded.

Step 4.25 — Dr Zulu records Dr Molefe's decision (act-4-annual-review.md:623)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Satisfactory Progress, with the rationale "On track for completion; three EPAs entrusted, the rest to be
  decided at her final review." Write "To be decided at her final review." in About the EPAs not decided. Keep all
  four present, press Record decision…, and confirm.
Expect:
  - **The dialog** is titled "Record Satisfactory Progress for Lerato Molefe?" and reads "Fixes 3 staged decisions:
    PAED-001 at 5, PAED-010 at 4 (expires `D+20`), PAED-012 at 5.", "Records 12 EPAs as not decided: PAED-002,
    PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-008, PAED-009, PAED-011, PAED-013, PAED-014 and PAED-015."
    (E1), "Present: Thandi Zulu (chair), David Naidoo, Sarah Botha, John van Rensburg (external)." and "Afterwards only
    a decision that no longer fits the trainee's curriculum can be removed." The focus is on its safe button, "Not yet"
    (round 3 item 22); the confirm is "Record decision".
  - **Recorded:** "Decision recorded." in the Decision section, which now sits above the agenda, with the focus there.
    The bar reads Decided, "Satisfactory Progress · 3 decisions to issue" and Ratify….
  - **The decision:** headed "Satisfactory Progress", with the rationale, "About the EPAs not decided: To be decided at
    her final review." and a "Present:" line. It opens with "Thandi Zulu (chair)", ends with "John van Rensburg
    (external)", and names David Naidoo and Sarah Botha between them.
  - **Agenda:** "Fixed when the committee's decision was recorded: ratifying issues exactly these." above it. The three
    staged lines keep their decisions, with no Remove…; the other twelve, due and optional alike, read "Not decided at
    this sitting" with "To be decided at her final review." (E1). No Action column.
  - **No chair-rated warning:** five assessors rated her lines, so "The chair rated all of this evidence" is not shown
    (D46 warns of that case and refuses nothing).

Step 4.26 — Prof Mbatha reads the decided review, and is not offered Ratify (act-4-annual-review.md:661)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Between the recording and the ratifying, open Dr Molefe's review from Committee reviews.
Expect: The list reads Decided, Satisfactory Progress, for Dr Molefe's review. The review shows the decision with its
  "Present:" line above the agenda, and the three staged decisions in their lines under "Fixed when the committee's
  decision was recorded …". It offers no Ratify…, no Remove… and no Action column, and the subtitle has no "Lerato
  Molefe's progress": that link is for the committee member, the coordinator and the speciality and sub-speciality
  admins (round 3 item 19). The sitting bar is a reader's, static: Decided, "Satisfactory Progress · 3 decisions to
  issue", "Only the panel's chair, Thandi Zulu, can ratify the committee's decision.", and "As of hh:mm SAST" with
  Refresh. Only the chair ratifies, with no administrator's bypass (D46).

Step 4.27 — Dr Zulu ratifies, and the STARs are issued (act-4-annual-review.md:680)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Ratify… in the sitting bar, read the dialog, and confirm.
Expect:
  - **The dialog** is titled "Ratify and issue 3 STARs to Lerato Molefe?". It lists "PAED-001 at 5, issued `D`",
    "PAED-010 at 4, issued `D`, expires `D+20`" and "PAED-012 at 5, issued `D`", then "PAED-002 and 11 others were not
    decided at this sitting." (E1) and "Ratifying cannot be undone: nothing on this review can change afterwards." It
    says nothing about mail (T320). The focus is on "Not yet"; the confirm is "Ratify and issue 3 STARs".
  - **Ratified:** "Decision ratified." under the bar, with the focus there. The bar reads Ratified and "Satisfactory
    Progress · 3 STARs issued", and Ratify… is gone.
  - **Agenda:** "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
    one." above it (T212).
    - PAED-001, 010 and 012 read Decided, each with "STAR #n.";
    - the other twelve, due and optional alike, read "Not decided at this sitting" with "To be decided at her final
      review." (E1).
  - **Standing:** "2 at or above · 1 below · 12 with no decision, of 15 EPAs". PAED-001 and 012 at `5` meet year 4's
    target of `5`, and PAED-010 at `4` is below it. The exit rule reads "2 of 15 EPAs at their exit level by STAR
    decision".
  - **What ratifying locks:** nothing on the review can now be staged, removed or recorded.

Step 4.28 — Dr Zulu stages two decisions for Dr Dlamini (act-4-annual-review.md:714)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Dlamini's review and start it through its dialog. Stage, each on two finished PAED-001 lines (Dr Naidoo's
  Mini-CEX and Dr Botha's CCA):
  - PAED-001 at `4`, with the rationale "Manages the acutely ill child with the senior available on request.";
  - PAED-002 at `3b`, with the rationale "Manages common presentations with decisions checked by the senior on call."
  Defer nothing: there is no deferral (D53).
Expect:
  - **Start:** the dialog counts "the 4 items observed from `<Y>`-01-01 to `<Y>`-12-31 (4 activities)"; then "Review
    started." Dr Dlamini's snapshot freezes her four Act 3 WBAs: three under PAED-001 and one under PAED-004.
  - **Staging PAED-002:** Dr Dlamini has no PAED-002 line, so PAED-002's open line lists none of its own first, and the
    picker says "None of the named items is about PAED-002." It still stages, since D38 lets any line of the snapshot
    ground a decision; the staged line repeats the hint.
  - **The agenda:** PAED-001 and 002 read Staged; the other ten closing lines stay "Due at this sitting" and PAED-008,
    009 and 013 "As opportunity allows"; the bar reads "2 staged".
  - **Record decision…** is held back only by the quorum: it becomes available once a second member is ticked (Step
    4.29).

Step 4.29 — Dr Zulu records and ratifies Dr Dlamini's decision (act-4-annual-review.md:741)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Choose Satisfactory Progress with a rationale, write "Not at a decision point this year." in About the EPAs not
  decided, tick David Naidoo and Sarah Botha as present, and record, confirming in the dialog. Then ratify, confirming in
  its dialog.
Expect:
  - **The Record dialog** reads "Fixes 2 staged decisions: PAED-001 at 4, PAED-002 at 3b." and "Records 13 EPAs as not
    decided: PAED-003, PAED-004, PAED-005, PAED-006, PAED-007, PAED-008, PAED-009, PAED-010, PAED-011, PAED-012,
    PAED-013, PAED-014 and PAED-015." (E1).
  - **Recorded:** "Decision recorded.", then, after the Ratify dialog ("Ratify and issue 2 STARs to Anele Dlamini?",
    "PAED-003 and 12 others were not decided at this sitting."), "Decision ratified.". The state reads Ratified.
  - **Present:** a "Present:" line naming Thandi Zulu (chair), David Naidoo and Sarah Botha.
  - **The agenda:** PAED-001 and 002 read Decided with their STAR numbers; the other thirteen read "Not decided at this
    sitting" with "Not at a decision point this year."
  - **Standing:** "1 at or above · 1 below · 13 with no decision, of 15 EPAs". PAED-001 at `4` meets year 3's target of
    `4`, and PAED-002 at `3b` is below it.

Step 4.30 — Dr du Plessis's sitting: progress with an observation, no STAR (act-4-annual-review.md:767)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr du Plessis's review and start it through its dialog. Stage nothing. Record Satisfactory with Observations,
  confirming in the dialog:
  - the rationale "Teaching logs are sound; no observed clinical assessment completed yet.";
  - About the EPAs not decided: "Not at a decision point this year.";
  - the condition "At least two Mini-CEX before the next sitting.";
  - David Naidoo present.
  Then ratify, confirming in its dialog.
Expect:
  - **The snapshot** holds his two teaching sessions logged this year, his CBD still Requested, his cancelled DOPS and
    his portfolio review still Awaiting review, each with its state (T138). The session logged for `J−1y−56d` is outside
    the window and not in it.
  - **Rating trajectory:** "No rating by a named assessor to chart in this review's window."
  - **Record decision…** is available once David Naidoo is ticked, with nothing staged (D53): its dialog reads "Fixes no
    staged decision." and "Records 15 EPAs as not decided: PAED-001, PAED-002, … and PAED-015."
  - **Recorded:** the decision reads "Satisfactory with Observations", with "About the EPAs not decided: Not at a
    decision point this year.", "Conditions: At least two Mini-CEX before the next sitting." and "Present: Thandi Zulu
    (chair), David Naidoo".
  - **Ratified:** the Ratify dialog is titled "Ratify Pieter du Plessis's review?", reads "PAED-001 and 14 others were
    not decided at this sitting." and confirms with "Ratify the review". The state reads Ratified, the bar
    "Satisfactory with Observations · no STAR issued", and the agenda says "Nothing was staged when the review was
    ratified, so ratifying it issued no STAR." All fifteen lines read "Not decided at this sitting" with "Not at a
    decision point this year."

Step 4.31 — Dr Mahlangu's sitting: additional training (act-4-annual-review.md:805)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review and start it through its dialog, and read the snapshot. Stage nothing. Record Inadequate
  Progress — Additional Training, confirming in the dialog:
  - the rationale "One DOPS in the year and a Mini-CEX that stalled; a supervised support plan and re-review in six
    months.";
  - About the EPAs not decided: "Not enough observed evidence yet this year.";
  - Sarah Botha and John van Rensburg present.
  Then ratify, confirming in its dialog.
Expect:
  - **The snapshot** lists Dr Mahlangu's DOPS under PAED-002 and Mini-CEX under PAED-004, both Completed at 3a.
  - **Sampling concentration warnings** name PAED-002 and PAED-004, one rating each.
  - **Ratified:** the state reads Ratified. All fifteen lines read "Not decided at this sitting" with "Not enough
    observed evidence yet this year." (D53, E1).
  - **Present:** "Present: Thandi Zulu (chair), Sarah Botha, John van Rensburg (external)".

Step 4.32 — Dr Ndlovu's sitting: the outcome deferred (act-4-annual-review.md:830)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Ndlovu's review and start it through its dialog, and read the snapshot. Stage nothing. Record Outcome
  Deferred, the progression category (unrelated to D53's end of agenda deferral), confirming in the dialog:
  - the rationale "One completed Mini-CEX and one reflection in his first semester; to be judged at the next
    sitting.";
  - About the EPAs not decided: "Too early in the programme to decide.";
  - Sarah Botha present.
  Then ratify, confirming in its dialog.
Expect:
  - **The agenda, once ratified:** all fifteen lines read "Not decided at this sitting" with "Too early in the programme
    to decide."
  - **Evidence snapshot:** his three Act 3 activities, each with its state: the Mini-CEX Dr Khumalo declined and the
    one Dr Botha completed at 3a, both under PAED-002, and the reflective exercise under PAED-001, discussed. A declined
    request is evidence too (T138).
  - **Trajectory:** PAED-002 alone, from Dr Botha's rating.
  - **Sampling:** "Before the committee decides" names PAED-002, one rating from one assessor.
  - **Ratified:** the review reads Ratified, with "Outcome Deferred".

Step 4.50 — Dr Zulu starts and closes the check-in (act-4-annual-review.md:1224)
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Start the formative review through its dialog, read it, and close it through its dialog.
Expect:
  - **The type:** the subtitle reads "Formative check-in · Paed Annual Review Panel · Training year 1" (T309 item 2).
  - **Started:** "Review started." under the sitting bar, with the focus there. The bar reads In progress, "Formative: an
    interim check-in, no binding decision" and Close review…. The snapshot freezes the same three activities as Dr
    Ndlovu's annual review, and the standing and MSF coverage sections follow it. Close review… is offered to the chair
    alone.
  - **Closed:** the dialog is titled "Close Sipho Ndlovu's formative check-in?" and reads "It records no decision and
    issues no STAR. Nothing on it can change once it is closed.", with the focus on "Not yet"; its confirm is "Close
    review". Then "Formative review closed." with the focus on it; the bar reads Closed and "Formative check-in · no
    decision recorded", and nothing is offered.
  - **Other readers** (Dr Naidoo, Mr Smit) read "Only the panel's chair, Thandi Zulu, can close this review." in a
    static bar while it is in progress, with "As of hh:mm SAST" and Refresh, and no button to close it.

Step 5.3 — Dr Zulu starts the review and reads where Dr Molefe stands (act-5-graduation.md:126)
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Molefe's pre-graduation review from Committee reviews and start it through its dialog. Read the evidence
  snapshot, the agenda and "Entrustment against Annexure A".
Expect: Before Start, the h1 reads "Lerato Molefe · `<P>`" with "Pre-graduation review" in the subtitle, and the bar
  "Summative · evidence `J−3y` to `<Y>`-12-31" (round 3 item 28). The Start dialog counts "the [n] items observed from
  `J−3y` to `<Y>`-12-31 (…)". The page then says "Review started." atop the agenda and the review reads In progress.
  - Above the agenda's caption "12 EPAs for `<P>` · none staged", it says "Already decided in this window, so not on
    the agenda: PAED-001, PAED-010 and PAED-012." (round 3 item 25).
  - The snapshot freezes her evidence observed since `J−3y`, grouped by EPA, including the released MSF
    campaign.
  - The standing reads training year 4, and "2 at or above · 1 below · 12 with no decision, of 15 EPAs". Its exit rule
    reads "2 of 15 EPAs at their exit level by STAR decision (level 5: 2 of 9 · level 4: 0 of 6)", followed by "Not
    yet: …", which names every EPA but PAED-001 and PAED-012.
  - Under the rule: "For information only. Recording a Graduate decision or completing the programme does not check the
    exit rule."
  - Record decision… is held back only by the quorum (D53): unavailable, with the quorum line beside it, until a
    second member is ticked. It names no agenda line.
  - Sampling warnings may name EPAs rated by fewer than three assessors. They are warnings and refuse nothing.

Step 5.4 — Dr Zulu stages a STAR at its exit level on every EPA not yet there (act-5-graduation.md:166)
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: For each EPA the standing names under "Not yet", stage a decision at its exit level. That is thirteen EPAs, every
  one but PAED-001 and PAED-012, which Act 4 decided at their exit level:
  - rung `5` on PAED-002 to 007 and PAED-010;
  - rung `4` on PAED-008, 009, 011, 013, 014 and 015.
  Use Stage on each of the twelve agenda lines (PAED-002 to 009, 011, 013, 014 and 015), each resting on Default MSF #1
  alone, since none of them has a line of its own. For PAED-010, which Act 4 decided in this window below its exit
  level, use "Stage a decision on an EPA not on this agenda" at the agenda's foot, choosing it in its EPA list and
  ticking its own two lines. Issue each decision on `D` with no expiry and write a rationale.
Expect: The level list offers only the six rungs of the CPSA Paediatric Entrustment Scale v11.1 (`1`, `2`, `3a`, `3b`,
  `4`, `5`). Stage pending decision stays unavailable, with "Name at least one item of the evidence snapshot." beside
  it, until a line is ticked. A decision on Default MSF #1 alone gets both hints, "Only one item is named. The College's
  rule is that an entrustment decision is never taken from a single form." and "None of the named items is about
  PAED-002." (and so on), and is staged anyway. After each staging:
  - "Pending entrustment decision staged." sits under the EPA's line, with the focus there;
  - the line reads Staged, with its decision in words ("Level 5 · issued `D` · no expiry.") and "Rests on 1 item of
    the snapshot: Default MSF #1." with the hints;
  - PAED-010 gains a chair's line, reading Staged with "Added by the chair.", on its two lines [the Direct
    Observation #n and the MSF record #n].
  Once all thirteen are staged, the caption reads "13 EPAs for `<P>` · 13 staged" and "Already decided in this window,
  so not on the agenda: PAED-001 and PAED-012." (round 3 item 25). Record decision… is held back only by the quorum.

Step 5.5 — The panel records its decision: Graduate (act-5-graduation.md:210)
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: Choose Go to the decision and record the committee's decision, confirming in the dialog:
  - category: Graduate (programme complete);
  - rationale: every EPA is entrusted at its exit level, and the panel recommends completion;
  - About the EPAs not decided: nothing (every line is staged);
  - conditions: none;
  - present: Dr Zulu, Dr Naidoo, Dr Botha and Dr van Rensburg.
Expect: The category list opens with nothing chosen and offers Graduate (programme complete) among the progression
  outcomes. Dr Zulu is ticked as present and cannot be unticked. Beside the available Record decision…: "Recording fixes
  the 13 staged decisions." The dialog, "Record Graduate (programme complete) for Lerato Molefe?", reads "Fixes 13
  staged decisions: …" naming each EPA at its level, and records none as not decided: it has no "Records … as not
  decided" line (E1). The review then reads Decided (T250), with "Decision recorded." in the Decision section, now above
  the agenda. The decision is headed "Graduate (programme complete)" and names the four members present. The agenda
  says "Fixed when the committee's decision was recorded: ratifying issues exactly these." The foot form and every
  Remove… are gone, the bar reads "Graduate (programme complete) · 13 decisions to issue", and only Dr Zulu is offered
  Ratify…; Dr Naidoo's static bar reads "Only the panel's chair, Thandi Zulu, can ratify the committee's decision."

Step 5.6 — Dr Zulu ratifies (act-5-graduation.md:246)
Role: CommitteeMember — Dr Thandi Zulu (chair)
Route: /committee/reviews/{ReviewId:int}
Do: Choose Ratify… and confirm in the dialog.
Expect: The dialog, "Ratify and issue 13 STARs to Lerato Molefe?", lists the thirteen ("PAED-002 at 5, issued `D`" and
  so on) and names no EPA as not decided; its confirm is "Ratify and issue 13 STARs". The page says "Decision
  ratified." under the bar, and the review reads Ratified, "Graduate (programme complete) · 13 STARs issued". Every
  staged decision is issued as a STAR in the same action, STARs #6 to #18 (which EPA holds which number is read at the
  replay: [#n] each):
  - each agenda line reads Decided with its "STAR #n.";
  - the agenda says "Nothing is pending: ratifying the review issued what was staged as STARs, and the agenda names each
    one.";
  - Act 4's active STAR on PAED-010 [#2], re-decided today as [#18], is superseded.
  The standing's exit rule reads "15 of 15 EPAs at their exit level by STAR decision (level 5: 9 of 9 · level 4: 6 of
  6). Every EPA is at its exit level." Its year line reads "15 at or above · 0 below · 0 with no decision, of 15 EPAs".

Step A.7.6 — Dr Zulu on her phone (appendix-cross-cutting.md:1258)
Role: CommitteeMember — Dr Thandi Zulu
Route: / → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: At 390 px, open Committee reviews and Dr Molefe's final review.
Expect: Home's Registrars and Targets by EPA cards are one column: each registrar's name a 44px block with the training
  year and the figures under it, each EPA's name a 44px block with its cadence and its "n of 2" under it (T358, round 3
  item 21). The review's cards stack. The evidence tables scroll inside their containers. Each EPA's trajectory chart is
  a card of its own (its section is not a card around it), drawn at 322 px, never scaled, its rungs and months 12 px,
  and fits its named region, "Rating chart for PAED-001" and the like (E1), with its table stacked under it. Nothing
  scrolls sideways (T166, T355).
  Review #7, Dr Molefe's pre-graduation review, ratified (T361, round 3): the sitting bar under the title, static
  below 641 px; the decision above the agenda; the agenda as stacked lines (a container query on its container, below
  760 px), each keeping its table roles and each cell labelled by its column; "Lerato Molefe's progress" under the
  title a 44 px target (round 3 item 20; a line's "Rating chart for …" and the chart's "Back to …'s line", the other
  links item 20 sizes, are drawn only while a line is open, so the Note checks them with the review in progress);
  the charts at 322 px. Nothing scrolls sideways. Checked at 768 px as well: the agenda still stacks, and nothing
  scrolls sideways.

Step A.7.13 — Dr Zulu hears a populated review with a screen reader (appendix-cross-cutting.md:1427)
Role: CommitteeMember — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: With NVDA (or Narrator) on, open Dr Molefe's final review (review #7). Walk it by headings, then by tables, then by
  regions.
Expect: The h1, "Lerato Molefe · `<P>`", is announced on arrival. The sections are h2s, in the page's order: "The
  sitting" (visually hidden), "Before the committee decides", "Decision" (above the agenda, the review being ratified),
  "Agenda", "Entrustment against Annexure A", "Multi-source feedback by EPA", "Evidence snapshot", "Rating trajectory by
  EPA" and "Appeals" (T361, T280). Below them, h3s: "Sampling concentration warnings", the decision's "Graduate
  (programme complete)", each EPA group of the snapshot and "Not about a single EPA", and each EPA's chart.
  - Every section is a region named by its heading.
  - Each trajectory chart is announced by its EPA, and a table follows it that a screen reader reads, with one row per
    observation (T085).
  - The agenda and the evidence tables announce their column headers (stacked lines keep their table roles), and every
    row action is named for its row (T239).
  - Every badge is read as its words.
  - Nothing announces itself on arrival: on this page only a result, the decision form's quorum line and Refresh's
    status are live (round 3 item 21), and a ratified review shows none of them. Nothing is dismissible, and no
    control is unnamed (no "×", T280).
```

