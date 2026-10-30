# F08 Panels, what is due, and the schedule

**Who and why:** the people who run the programme form the review panels, read what the committee must decide this
period, and schedule each registrar's review from it. A decision missed in its window is a missed milestone for a
registrar (Step 4.2), and a second binding review for a period is refused only after Create (Step 4.8). Four pages
carry it today: Decision panels, a panel's own page, Decisions due, and Committee reviews with its scheduling form.
None has been redesigned since 2026-09-26; flows 01–07 changed only their titles, their menus and (flow 07, D53) the
words for a line left undecided.

Restated 2026-10-12 for T364 (the brief of 2026-09-26, T332, predates flows 01–07). Paths to screenshots are under
`design/baseline/` (gitignored). Code is cited at `b62997a5`; `W/` is `src/Wombat.Web/Components/Pages/CommitteeDecisions/`,
`A/` is `src/Wombat.Application/Features/CommitteeDecisions/`, `D/` is `src/Wombat.Domain/CommitteeDecisions/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **F/W** in BRIEF.md § 8: the panels' list and form are expected to keep their shape (F), Decisions due and the schedule may change a lot (W). |
| Mode | **Structure first** (BRIEF.md § 2.3, as § 11 corrected it). Round 1: 2–3 structural variations of the calendar (Decisions due, the schedule and its form: one page or two, where the form lives, how one registrar is read), and ONE proposal for the panels (list, form, the panel's own page) to confirm or correct. Round 2, after the pick: fidelity with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768 (§ 11, flow 06) |
| Variations | 2–3 for the calendar in round 1, one for the panels; the chosen one in round 2 |
| People | InstitutionalAdmin: Prof Nolwazi Mbatha (head of department). SpecialityAdmin: Dr Refilwe Mokoena (programme director). SubSpecialityAdmin: Dr Kabelo Sithole (sub-speciality lead). Coordinator: Mr Pieter Smit (programme administrator). CommitteeMembers: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha, Dr John van Rensburg (external), who read the panels and their panels' reviews and are refused Decisions due (Step 4.5). The registrars: Dr Lerato Molefe (training year 4), Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa Mahlangu and Dr Sipho Ndlovu (1) (`README.md` § Cast) |
| Runbook steps | 30, pasted verbatim in § 8: 2.20–2.25 and 2.32 (Act 2), 4.1–4.13, 4.33, 4.34, 4.37 and 4.49 (Act 4), 5.1, 5.2 and 5.29 (Act 5), A.1.10, A.7.5 and A.7.8 (appendix) |
| Pages (`coverage.md` templates) | `/committee/panels` (`W/PanelsList.razor`, 295 lines), `/committee/panels/new` and `/committee/panels/{PanelId:int}` (`W/PanelEdit.razor`, 702), `/committee/decisions-due` (`W/DecisionsDue.razor`, 376, with `DecisionsDueText.cs`), `/committee/reviews` (`W/ReviewsSchedule.razor`, 548: the list and the scheduling form, inline), `/committee/reviews/{ReviewId:int}` before its sitting (flow 07's page; this flow only arrives there), `/access-denied` (flow 01's). Checked at `b62997a5`. |
| Menus (`W/../../Navigation/NavItems.cs`) | Decisions due, Committee reviews and Decision panels: grouped under "Reviews" for the Administrator and InstitutionalAdmin (`:146`, `:153`); flat for the Speciality and SubSpecialityAdmin, beside Programme trainees, Waiting for assessors and Entrustment decisions (`:165–168`); the Coordinator's menu has all three (`:175`, Decision panels since flow 06, `85d5a508`); a CommitteeMember's has Committee reviews and Decision panels, not Decisions due (`:169`). A list lights itself and draws no trail; a panel's page is owned by Decision panels for those who keep panels (`NavOwners.cs:63`, `:116`); a review by Committee reviews (`:106–113`). The Coordinator may read the panels list but not open a panel (access denied, Step 2.32). |
| Components drawn at runtime | `PageHeader`, `StatePanel` (no page passes `OnRetry`), `Skeleton`, `DataTable` (panels and reviews lists: no caption, no `Stack`, no `scope`), raw `table.clinic-table` (Decisions due's two tables), `PagerControls` (Decisions due only), `FormField`, `FormActions`, `FieldHelp`, `InFlight` (panel form only), `ActionResult` (panel form only), `Alert`, `BadgeFor`, native `<select multiple>` (Members, External members). Not used: `ConfirmDialog`, flow 01's not-found panel, `ActionResult` on the schedule. Queries: `ListDecisionPanels`, `GetCommitteeRouting`, `GetDecisionPanelFormOptions`, `GetDecisionPanelById`, `ListPanelMemberCandidates`, `GetEntrustmentDecisionsDue`, `ListReviewsForPanel`, `PreviewCommitteeAgenda`, `GetCommitteeReviewsAccess`; commands `CreateDecisionPanel`, `UpdateDecisionPanel`, `SetDecisionPanelBody`, `ScheduleCommitteeReview` (all `A/`) |
| Held | None. Every step capture is from the T361 replay (2026-10-12), on the shell as built. The `states/panels-list--*` and `states/panel-edit--*` captures are from 2026-09-26 on the old frame; the step captures stand in for most (§ 3). |
| Depends on | Flow 01, built (`b347e11c`): the shell, the trail, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again, access denied and not found. Flow 04 (`06aa51d7`): row actions named per row, counts as words, the pager's ends aria-disabled. Flow 05 (`b020c942`): ISO dates (D1), stacked rows below 641 px. Flow 06 (`85d5a508`): the committee and coordinator Homes, Programme trainees, the Coordinator's Decision panels link. Flow 07 (`44de180b`): the review page these pages open, D53's Not decided, the formative label on the list. The design system is **version 19**, re-synced to flow 07 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 08`. Open each screenshot. Leave out anything in
   `design/upload/crop-first/`, or crop its link first (none of this flow's captures shows a link or token; checked
   2026-10-12). Do not stage `states/panel-edit--one-member.png` (a broken stitch: its top 1,094 px have no shell) or
   `states/panels-list--narrow.png` (375 px, and it shows scratch rows of a curriculum 11.2).
2. **Start a new canvas** from the main app's Design page, on the Wombat design system v19
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier flow's canvas. Flow 08's canvas, "Wombat 08": https://claude.ai/artifact/Bvi2pBNN4WBWc4HqwnRRtQ (created 2026-10-12 by
   Claude Code from the Design type, the design system installed and the mark copied in).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Put the returned `/_blob/` URL in § 1 and in
   `design/flows/08-committee-calendar/round-1-ask.txt`, where they read `/_blob/d8e89f2bacfcd9a2e79781ca6f135313`.
4. **Send § 1 and § 8 as ONE message** (`design/flows/08-committee-calendar/round-1-ask.txt`), with the key screenshots
   in § 4. Read `project/canvas.json`'s version before and after: a send can draw nothing. Read round 1's notes for a
   line saying it never received the attachments (§ 11, flow 06); re-attach and name the folder before round 2 if so.
5. **Pick one structure,** say why in a sentence, and answer § 6's questions one sentence each; the operator's are
   marked. A question whose answer rests on an open task names it, and the task lands before the build (§ 11, flow 05).
   Ask for round 2.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/07-review-sitting/round-2-review.md`
   did: one reviewer on the code (and the ask), one on the cast and the runbook, one on the design system. Put the
   decisions to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/08-committee-calendar/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 08 — Panels, what is due, and the schedule

This is flow 08 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme) and 07 (the review sitting) are designed and built; these pages sit inside the shell.
  Structure first, then fidelity: round 1 is 2–3 structural variations of the calendar (Decisions due, Committee
  reviews and scheduling) as wireframes, plus ONE proposal for the panels (the list, the form, a panel's own page) for
  me to confirm or correct. I pick before round 2. The runbook steps follow below in this same message. The brand mark
  is at /_blob/d8e89f2bacfcd9a2e79781ca6f135313: use it, never a drawn disc.

GOAL: The people who run the programme must be able to:
  - form a review panel (a name, a scope, a chair, members and external members, and the College committee it sits
    as, if any), see who sits on it, and see which panel decides each EPA of each programme;
  - read what the committee must decide in a semester, per registrar and per EPA, and where each decision stands
    (decided, on an open review's agenda, not decided at a sitting, revoked, missed, optional);
  - read one registrar's fifteen EPAs together (Step 5.1);
  - schedule a registrar's review from what is due: an annual progression review, a pre-graduation review, an
    entrustment-only review, or a formative check-in that decides nothing;
  - know, before pressing Create, that the registrar already holds the period's binding review, and open it.
  Committee members read the panels and the reviews of the panels they sit on; they never schedule, and Decisions due
  is not theirs.
  What is wrong today (the attached screenshots show each; all on the shell as built, 2026-10-12, except one):
  - A panel's own page is titled "Decision panel" and never shows the panel's name, scope, institution or speciality
    (2.22). Saving a new panel reloads the page with no word that it was created. "Back to panels" repeats the trail.
  - The panel form is one narrow card at 1280 (about 315 px). Chair, Members and External members cut names and
    emails short ("Thandi Zulu (zulu@kgk.wombat.loc"); Members and External members are native multi-selects ("Hold
    Ctrl (Windows) / Cmd (Mac) to select multiple.") that do not work by touch at 390.
  - A refused change of College committee shows a ten-line danger box inside the narrow card, and the select keeps the
    refused choice until a reload (4.13). After a save, "1 member of this panel can no longer sit on it" stays until a
    reload, and is never announced.
  - The panels list's Scope column prints "Institution" or "Speciality" and names no speciality or institution; while
    the list loads, "Who decides each EPA" already says "There is no committee routing to show for your institution."
    (the one old-frame capture; brief from its body).
  - Decisions due is 3,781 px for 75 rows at 1280 (10,276 with a hundred a page). It has no trainee filter, so reading
    Dr Molefe's fifteen rows means paging past Dr Dlamini's (5.1). Its filters apply on change, one round trip each.
  - Every row wanting a sitting offers Schedule, a Missed row in a semester that has ended included (4.2). A Not
    decided or revoked row can show Open review and Schedule stacked in a cramped Action column; "Open review" wraps.
  - At 390 the list's Status and Action columns are off-screen to the right, and By EPA's EPA names are cut as it
    scrolls (A.7.5). The schedule's list shows Trainee, Panel and a Period broken a word a line; Type, State, Decision
    and Open are off-screen (A.7.8).
  - The scheduling form opens above the list, toggled by Schedule review, which stays in the header while the form is
    open; the focus does not move into it. Dates show in the browser's format ("2026/01/01") where the list is ISO.
  - The agenda preview gives no sign that the registrar already holds the period's binding review. The refusal comes
    after Create, at the top of the page, unfocused, with "(scheduled, scheduled 2026-10-12)" doubled and no link to
    the review it names (4.8).
  - With Formative only ticked, Review type still shows and reads "Annual progression review", with the semester-2
    help (4.49); a pre-graduation review shows the same semester-2 help (5.2).
  - When the preview fails it prints the exception ("The agenda could not be previewed: An exception has been raised
    that is likely due to a transient failure."), and Create stays enabled. Create has no busy state.
  - Headings go h1 to h3 to h4 on every page; the panels and reviews tables have no caption; the pager's "Per page:"
    names nothing.

AUDIENCE:
  - InstitutionalAdmin: Prof Nolwazi Mbatha. She manages every panel at her institution and alone says which College
    committee a panel sits as.
  - SpecialityAdmin: Dr Refilwe Mokoena; SubSpecialityAdmin: Dr Kabelo Sithole. They manage their speciality's panels
    and schedule reviews.
  - Coordinator: Mr Pieter Smit. He schedules every review, weekly around a sitting, and reads the panels.
  - CommitteeMembers: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha (each also an Assessor), Dr John van
    Rensburg (external, CommitteeMember only). They read the panels and their panels' reviews.
  - Kgosi Kgari Teaching Hospital (KGK), Paediatrics, on the College's (CPSA) curriculum 11.1, fifteen EPAs (PAED-001 to
    PAED-015): six decided every semester (001, 002, 004, 005, 010, 012), nine once a year (008, 009 and 013 as
    opportunity allows); PAED-001 is "Providing paediatric emergency care to children"; PAED-004 and PAED-005 are the
    College's neonatal committee's (the "Neonatal team Clinical Competency Committee"). Five registrars: Dr Lerato Molefe (training year 4),
    Dr Anele Dlamini (3), Dr Pieter du Plessis (2), Dr Nomsa Mahlangu and Dr Sipho Ndlovu (1). Desktop 1280×800 and
    phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - Who is offered what. Decision panels: everyone above reads it; New panel only for someone who may create a panel
    somewhere (hidden, never disabled); Edit per row only on a panel the reader keeps. An InstitutionalAdmin keeps every
    panel at her institution; a Speciality admin the Speciality-scoped panels of her speciality; a Sub-speciality admin
    those of her sub-speciality's parent; a Coordinator and a member keep none. A panel's page: those who keep panels;
    the Coordinator and members are refused (access denied). Decisions due: the schedulers (Coordinator, the three
    admins, an Administrator), never a CommitteeMember. Scheduling: the same schedulers. Anyone holding Trainee as well
    is refused every change, with "You hold the Trainee role, so you cannot create or change a decision panel,
    including one that reviews you." (and the schedule's equivalent).
  - The panels list: Name, Scope ("Institution" or "Speciality"), Decides for (a College committee, or "General
    panel"), Members (a count), and Edit ("Edit Paed Annual Review Panel"). Empty: "No decision panels" / "Create a
    panel before scheduling reviews." (or "No decision panel runs at your institution yet." for a reader). Then "Who
    decides each EPA": "Which panel takes each EPA's entrustment decision for a trainee of each programme at Kgosi
    Kgari Teaching Hospital. An EPA the College gives to a named committee (EPAs 4 and 5 to the neonatal CCC) goes to a
    panel sitting as it, one covering the trainee's speciality before an institution-wide one, and to the general
    panels while none does." Per programme ("Paediatric EPA Curriculum 11.1"), a table of EPAs and Decided by, rows
    grouped by answer: "PAED-004, PAED-005 → Paed Annual Review Panel. No panel covering this programme sits as the
    Neonatal team Clinical Competency Committee, so the general panel decides."; with no panel, "No panel at this
    institution covers this programme."; an Administrator first chooses an institution ("Choose an institution to see
    which panel decides each EPA there.").
  - A panel's form. Creating: Panel name; Scope (Institution, Speciality; it starts on Speciality wherever one is
    offered); Institution (an Administrator, or Scope Institution); Speciality ("The panel runs at your institution.
    Only the specialities it has adopted a curriculum in are listed."); Decides for (the InstitutionalAdmin and an
    Administrator only; a speciality admin gets no field); Chair ("Choose a chair."); Members; External members
    ("External examiners, who sit with the chair on the panel's appeal body. Only active committee members at the
    panel's institution who are not trainees are listed: cross-institution externals are not yet supported."). Choosing
    a chair drops her from Members: "Thandi Zulu is the chair now, so is no longer selected under Members." (a live
    note). Candidates are the institution's active committee members who are not trainees, by surname. Name, scope,
    institution and speciality cannot change after creation; only the members and the committee can.
  - A saved panel: "Decides for" (a select and Save committee for the InstitutionalAdmin, with "A panel sitting as a
    College committee decides the EPAs the College gives that committee (the neonatal CCC decides EPAs 4 and 5), and
    nothing else. Where no panel sits as it, the general panels decide them. It cannot change while a review before the
    panel is scheduled, in progress or awaiting ratification."; for a speciality admin, read-only with "Only an
    institutional administrator can change which College committee a panel sits as."), and "Update members" with Save
    panel. Members may change while reviews are open; the committee may not.
  - Panel refusals: "A panel needs at least two members: the chair and at least one other, so that no decision is one
    person's."; "A panel member is listed more than once."; "Only an active committee member at the panel's
    institution who is not a trainee can sit on it."; "Your institution has adopted no curriculum in this speciality,
    so a panel for it would have no trainee to review."; "Paed Annual Review Panel already sits as the Neonatal team
    Clinical Competency Committee for this speciality. An institution has one such panel, and one per speciality:
    change that panel instead."; and, while a review is open, "Review #1 (2026 S2, scheduled) still sits before this
    panel. What a review decides, and its agenda, follow the College committee the panel sat as when it was scheduled,
    so the panel keeps what it sits as until its open reviews are ratified, or closed if formative." Results: "Panel
    members updated."; "The panel is a general panel." / "The panel sits as the Neonatal team Clinical Competency
    Committee.". The warning "1 member of this panel can no longer sit on it, so is not listed below: only an active
    committee member at its institution who is not a trainee can. Saving takes that member off the panel." Not found:
    "The decision panel could not be found." (an out-of-scope panel reads the same). A panel manager with nowhere to
    create one: "You cannot create a decision panel. A speciality or sub-speciality administrator creates the panels of
    the speciality administered, at the administrator's own institution, once that institution has adopted a
    curriculum in the speciality. An institutional administrator creates the institution's panels."
  - Decisions due. Filters: Institution (an Administrator only, "Select institution"; until then "Choose an
    institution" / "An administrator belongs to no institution, so the decisions due are shown for the one chosen
    above."); Period (three semesters back, the current, two ahead, as "2026 S2 · 1 Jul to 31 Dec 2026"); Status
    ("Outstanding: not yet decided" by default, "Every status", then each status); EPA ("Every EPA", "PAED-001 —
    Managing common paediatric emergencies" …). The opening: "The EPAs due for an entrustment decision in 2026 S2 for
    the 5 trainees you oversee at Kgosi Kgari Teaching Hospital, by each EPA's decision cadence: every semester, or
    once a year. Missed is read today: the EPA's window has ended with nothing decided or on an open review's agenda."
    "By EPA", a summary per EPA (Due, Decided, Scheduled, Not decided, To schedule, Missed, Optional), captioned "How
    many trainees stand where on each EPA due in 2026 S2. To schedule: not scheduled, due by year end, or revoked.
    Optional: a partial period, or decided as opportunity allows."; the filters do not narrow it. "75 of 75 decisions
    due in 2026 S2 shown." The list: Trainee, EPA, Window ("2026 S2", or "2026" for an annual EPA), Status (a badge
    and a sentence), Action; sorted by trainee then EPA; 20 a page (10, 20, 50, 100).
  - The statuses and their sentences: Not scheduled ("No open review holds it, and 2026 S2 has not ended."); Scheduled
    ("On the agenda of review #1, which is still open."); Decided ("STAR #1, issued at review #1." or "STAR #1 stands
    for 2026."), green; Not decided ("Not decided at review #4. A later sitting for 2026 S2 plans it again."), amber
    (D53); Revoked: re-decide ("STAR #5, issued for 2026 S2, was revoked. Schedule a review to decide it again."), red;
    Due by year end ("Decided once a year: due by the last sitting of 2026, and not missed before the year ends.");
    Partial period ("The trainee joined the window part-way through: optional, never missed."); As opportunity allows
    ("Decided as opportunity allows: optional, never missed."); Missed ("2026 S1 has ended with nothing decided or on
    an open review's agenda."), red. Where an open review already holds the period: " Review #2 is open for 2026 S2
    before Paed Annual Review Panel: decide it there, or ratify that review before scheduling another." Optional rows
    are never Missed. Only current registrars are listed (an ended, withdrawn or locked one is not).
  - The Action: "Open review" ("Open review #1 for Lerato Molefe, PAED-001") where a review holds the row; "Open review
    #2" where another open review holds the period; otherwise "Schedule" ("Schedule a review of Lerato Molefe for
    PAED-004 before Paed Annual Review Panel"), which opens the schedule form filled with panel, trainee and period; or
    "No panel you can schedule before decides it." Empty: "Nothing due for 2026 S2" / "No trainee you oversee at Kgosi
    Kgari Teaching Hospital owes an entrustment decision in 2026 S2."; filtered to nothing: "No decision matches the
    filters" / "Choose another status or EPA above."
  - Committee reviews: subtitle "Schedule progression reviews and open existing ones." (or "Open existing committee
    reviews." for a member); Schedule review in the header for a scheduler. The list, newest scheduled first, unpaged:
    Trainee, Panel, Period ("2026 S2 · 2026-01-01 to 2026-12-31"), Type ("Annual progression", "Pre-graduation",
    "Entrustment only", "Formative"), Mode ("Summative", "Formative"), State (Scheduled, In progress, Decided,
    Ratified, Under appeal, Closed, Withdrawn), Decision (a category's label, "Entrustment decisions only", "Pending",
    "No binding decision", "None: the review was withdrawn", or "—" for a formative review), and Open ("Open the 2026 S2
    review of Lerato Molefe before Paed Annual Review Panel"). Empty: "No reviews yet" / "The reviews you can open
    appear here once they are scheduled. Use Schedule review to put a trainee before a panel." (a member: "The reviews
    of the panels you sit on appear here once they are scheduled.").
  - The scheduling form: Panel (only panels with a trainee you can schedule); Trainee ("Choose a panel first. The list
    holds the trainees at its institution you can schedule."; current registrars only, Step 5.29); Period ("The
    semester the committee decides for. Choosing one fills the evidence window from the start of its academic year, so
    the annual EPAs a semester-2 sitting decides are judged on the whole year."); Review period from and to; Scheduled
    on (today); Review type (for a general panel in S2: Annual progression review, Pre-graduation review, "The
    semester-2 sitting decides the trainee's progression for the year."; in S1 also Entrustment-only review; before a
    College committee only Entrustment-only review, "… which decides entrustment only: a review before it records no
    progression category."); Formative only ("Formative review — closes without a binding decision and cannot issue
    entrustment decisions. Use for interim progress check-ins."); Create review. Created, the review's page opens
    (flow 07's, Scheduled).
  - The agenda preview, live as the form fills: "Choose a panel, a trainee and a period to see which EPAs the review
    will be there to decide."; then "15 EPAs will be on the agenda for 2026 S2." and "PAED-001, PAED-002, … and
    PAED-015 are due at this sitting.", the optional lines ("PAED-007 — … · As opportunity allows"), "PAED-004 and
    PAED-005 are decided by {panel}: schedule them separately." where another panel decides them, and "Already decided
    in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012." (5.2). Formative: "A formative review
    carries no agenda: it decides no EPA." Nothing-to-decide sentences: "This trainee follows no curriculum, so the
    review will have no agenda."; "Every EPA this panel decides that is due for 2026 S2 is already decided in its
    window, so the review will have nothing on its agenda."
  - Scheduling refusals: the duplicate, "Review #1 already puts this trainee before Paed Annual Review Panel for 2026
    S2 (scheduled, scheduled 2026-10-12). A trainee has one binding review for each period before the panels that
    decide the same EPAs, until it is ratified: open that review, or ratify it before scheduling another." (a formative
    review takes no seat and is never refused so); "A review can only be scheduled on a panel of your institution that
    covers the trainee's programme, for a trainee at that institution whose programme you oversee."; "A general
    panel's semester-2 sitting decides the trainee's progression for the year, so it is an annual progression or a
    pre-graduation review. An entrustment-only review sits in semester 1, or before a College committee such as the
    neonatal CCC."; "Only a trainee in a programme now can be put before a panel: …"; "Choose a trainee from the
    list."; "Choose the period the review sits for."
  - The review page before its sitting is flow 07's (its sitting bar reads Scheduled with Start review… for a
    Coordinator or a seated member, or "Only the coordinators of the panel's institution, and those of its members who
    are active committee members there, can start this review."). This flow only arrives there.
  - Data volumes: none (no panels; nothing due; no reviews); typical (one panel of four seats; 75 decisions due for five
    registrars; five reviews); heavy (two panels, one sitting as the neonatal committee; eight reviews and two formative
    check-ins; a revoked STAR to decide again; a hundred rows a page; a deleted user's pseudonym in the list, flow 14's).

WHAT FLOW 08 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01: the trail, the acting role, access denied, not found, the reconnect dialog); the
    menus' items (flow 06 settled them); flow 07's review page; the rules above (who may act, the seat rule, the
    quorum of two, what is Missed and what is optional, which types a panel and a semester allow).
  May, for these pages: their whole structure (Decisions due and the schedule as one page or two; the form inline, in
    a page of its own, or opened on a row; a trainee filter or a per-registrar view), the panels' form and how members
    are chosen, the panel's page title, the order and grouping of parts, titles and subtitles, where each result and
    refusal shows and where focus goes, confirmations, the lists' stacking at 390, and the words. Mark PROPOSED any
    rule change (refusing Schedule on an ended semester's row, renaming a panel, refusing a member change while a
    review is open): those are the operator's. The runbook's Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order:
  1. Decision panels: empty; the manager's list with "Who decides each EPA"; a speciality admin's; a member's and the
     Coordinator's (no New panel, no Edit); an Administrator choosing an institution; loading.
  2. A panel: new (the InstitutionalAdmin's, with Decides for; a sub-speciality admin's, Speciality only); the chair
     note; a panel of one refused; created (the result, and the page now naming the panel); members updated; read-only
     committee (a speciality admin); the committee change refused; a member who can no longer sit; nowhere to create;
     not found.
  3. Decisions due: an Administrator choosing an institution; outstanding (the opening, By EPA, the list); one
     registrar's rows; no row matches; a past semester with Missed rows; all scheduled; after the sitting (Decided,
     Not decided, optional); a revoked STAR to decide again; nothing due.
  4. Scheduling: the form, empty and filled from a Decisions due row; the agenda preview (15 EPAs; pre-graduation with
     "Already decided"); the period's binding review already held (the warning before Create); formative (no type, no
     agenda); the preview failed; created.
  5. Committee reviews: empty for a scheduler and for a member; scheduled; ratified (decisions by label); with a
     formative check-in; a member's list.
  6. 390 px for every page, and access denied (the Coordinator on a panel's page; a member on Decisions due).

STEPS: 2.20–2.25, 2.32, 4.1–4.13, 4.33, 4.34, 4.37, 4.49, 5.1, 5.2, 5.29, A.1.10, A.7.5 and A.7.8. Pasted verbatim
  below (Role / Route / Do / Expect). In them, D is the day the story is replayed, <P> the semester holding D (for
  example 2026 S2) and <Y> its year; J is the latest 15 January on or before D. Where an Expect quotes today's wording
  or layout, that is today's page, not a requirement on the new one.

STATES TO SHOW:
  Every state named in SCREENS; loading for every page (the header and a skeleton, no action offered until the read is
  in; "Who decides each EPA" never says there is no routing while it loads); a load error (a sentence and Try again,
  never an exception's text); each confirmation the design adds, its cancel, and the result after it with focus on
  it; 390 px for every page; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - A5 (BRIEF.md § 6): every action reports its outcome where the focus goes and where the person is working; a
    refusal shows beside the control that caused it and keeps what was typed; a refused select shows the saved value,
    or says it was not saved.
  - T309: when the registrar already holds the period's binding review, the preview says so before Create, in the
    create's own words, with a link to that review, and Create is not offered for it (or is refused with that reason
    beside it). A formative check-in shows no Review type, and reads "Formative check-in" wherever its type shows.
  - T260: a panel's page names the panel, its scope, its institution and its speciality; after a save, no stale "can no
    longer sit" warning; Decision panels says why New panel is missing for a manager who can create nowhere.
  - T259: a holding review's sentence names the panel that review sits before.
  - T329 and T338: every page renders its header from the first moment, then a skeleton; a failed read is an alert
    with Try again, never an exception's text; the preview's failure says the agenda could not be read, and why in
    words.
  - T324: a missing panel name, trainee or panel is refused in Wombat's words, never "The Name field is required.".
  - T272: no refusal or warning prints framework text.
  - T280: the pager's page-size select is named; every table has a caption; headings in order (one h1, then h2).
  - T264: every row action named for its row (done for the lists; keep it); the focus stays on the pager or moves to
    the list's heading on a page change.
  - Content (BRIEF.md § 6): sentence case; people by name as stored; states, types and categories by their labels;
    ISO dates (flow 05's D1); no doubled words ("scheduled, scheduled").
  - Done since the old brief, keep them: the Coordinator's Decision panels link (flow 06); the design's focus ring on
    By EPA's scroll region (T328); the formative label in the list (flow 07, T309 item 2); D53's Not decided.

QUESTIONS THE DESIGN MUST ANSWER:
  1. Decisions due and the schedule: one page (what is due, and scheduling it in place) or two linked pages? Where
     does the scheduling form live: inline above the list, a page of its own, or opened on a Decisions due row?
  2. How does a scheduler read one registrar's fifteen EPAs (5.1): a trainee filter, rows grouped by registrar, or a
     per-registrar view?
  3. How does a scheduler see, before Create, that the registrar already holds the period's binding review (4.8)?
  4. What does the list show at 390 px (A.7.5, A.7.8): stacked rows, fewer columns, or a different view?
  5. How are a panel's members chosen, at 1280 and by touch at 390, with whole names?
  6. What does a panel's own page show above its forms, and where do Decides for and the members sit?
  7. (The operator's) A Missed row in an ended semester offers Schedule for that semester. Keep it, refuse it, or
     schedule it for the current semester?
  8. (The operator's) A panel's name, scope and speciality cannot change after creation, and members may change while
     its reviews are open (only the committee may not). Keep both rules, or change either?
  9. (The operator's) One semester, two labels: the schedule's "2026 S2 · 1 Jul to 31 Dec 2026" and the registrar
     pages' "Semester 2, 2026 · July to November" (the College's nominal end, D40). Keep both, and say why where both
     appear, or align them?
  10. (The operator's) T309, T259, T260, T329, T338, T324, T272 and T280 touch only these pages: fold them into the
     build, or land any first as its own task?

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
  TrajectoryChart's two drawings with its visible table. Since flow 06: the committee and coordinator Homes, the
  registrar list with its filters. Since flow 07: ReviewSitting, SittingBar, AgendaTable (its container-query stack),
  AgendaLineForm, EvidenceReader, DecisionForm, ReviewReturnLink.
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
Also: each page has exactly one h1, and its headings are in order. A confirmation is flow 01's native <dialog>
  ConfirmDialog. Flow 07's review page is arrived at, not redrawn.

ASK:
  - Round 1: 2–3 STRUCTURAL variations of the calendar as wireframes (screens 3–5): what is due, how one registrar is
    read, where scheduling happens and how the seat warning shows before Create, and the lists at 390. Give each its
    reasoning and its step count per journey, at 1280 and 390 px: Mr Smit scheduling Dr Molefe from an empty schedule
    (4.1, 4.6, 4.7) and being stopped from a second review (4.8); Dr Mokoena scheduling Dr Mahlangu from what is due
    (4.9); Mr Smit reading Dr Molefe's fifteen rows and scheduling her pre-graduation review (5.1, 5.2); Prof Mbatha
    after the sitting (4.34); Mr Smit on his phone (A.7.5). Plus ONE proposal for the panels (screens 1–2: Prof
    Mbatha's 2.20–2.22 and 4.13), wireframed, for me to confirm or correct. No colour or type choices yet. Stop and
    wait for my pick.
  - Round 2: the chosen structure at full fidelity on the design system's tokens, every screen and state above, at
    1280 and 390 px, each role's view, every confirmation, and where the focus lands after each action.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ Table system; § Form system; § Pager; § Page-level patterns: List page, Detail page, Form page, and the
    title rule; the owner table's rows for a panel and a review; Decisions due's own section).
  - Mark PROPOSED anything not built: a confirmation, a new route, a rule change.
  - Flag edge cases: an Administrator who belongs to no institution; a panel of two (the chair and one member); a
    panel sitting as the neonatal committee; a registrar whose review is already held; a formative check-in on the same
    period; a registrar who has graduated or withdrawn (not offered); a deleted user's pseudonym in the list; the
    120-character EPA title ("PAED-015 — Teaching and applying evidence-based care responsibly and ethically in
    clinical decision-making and research").
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, badges read as words, the pager's select named, focus after every action, page change
    and confirmation; targets ≥ 24 px (44 at 390).

ATTACHED: act-2/2.21-1-chair-chosen.png, act-2/2.22-1-panel-saved.png, act-2/2.24-1-who-decides.png,
  act-4/4.13-1-mbatha-committee-change-refused.png, act-4/4.1-1-smit-decisions-due-outstanding.png,
  act-4/4.2-1-mbatha-previous-semester-missed.png, act-4/4.37-2-sithole-revoked-re-decide.png,
  act-4/4.6-1-smit-molefe-schedule-form.png, act-4/4.8-1-smit-second-review-refused.png,
  act-4/4.49-1-smit-formative-form.png, act-5/5.1-2-smit-molefe-rows-page1.png, act-5/5.2-1-smit-pre-graduation-form.png,
  act-A/A.7.5-4-decisions-due-390.png, act-A/A.7.8-3-reviews-390.png, states/panels-list--loading.png (old frame: read
  its body only). Several are full pages of 3,000–10,000 px, which you will see scaled. In every tall capture at 1280
  the sidebar's colour stops at 800 px: that is the capture, not the page.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 2.20 | `/committee/panels` | Prof Mbatha | no panels: "No decision panels", New panel; "who decides" says no panel covers the programme |
| 2.21 | `/committee/panels/new` | Prof Mbatha | the form's options, the chair note, a panel of one refused (D46); names cut short in a 315 px card |
| 2.22 | `/committee/panels/new` → `/committee/panels/{PanelId:int}` | Prof Mbatha | the panel saved: no word that it was created, and the page never names it (T260) |
| 2.23 | `/account/login` → `/` → `/committee/panels` → `/committee/panels/{PanelId:int}` → `/committee/panels` | Dr Mokoena | adds Dr Botha; Decides for read-only for her; "Panel members updated."; Members reads 4 |
| 2.24 | `/committee/panels` | Prof Mbatha | "who decides": PAED-004 and 005 fall back to the general panel with the College's reason; the other thirteen go to it |
| 2.25 | `/account/login` → `/` → `/committee/panels` → `/committee/panels/new` | Dr Sithole | Speciality scope only, no Decides for; "Select speciality" though one is offered; leaves without saving |
| 2.32 | `/account/login` → `/` → `/admin/invitations` → `/access-denied` → `/committee/panels` → `/committee/panels/new` → `/access-denied` | Mr Smit | Decision panels in his menu, lit (no New, no Edit); the new-panel form is access denied, which does not say which page (D4) |
| 4.1 | `/` → `/committee/decisions-due` | Mr Smit | outstanding: the opening, By EPA (12 To schedule, 3 Optional), "75 of 75", 20 a page, Schedule named per row; filtered to five PAED-004 rows; 3,781 px |
| 4.2 | `/` → `/committee/decisions-due` | Prof Mbatha | no Institution filter; the previous semester's Missed rows, each still offering Schedule; optional EPAs never missed; 10,276 px at a hundred a page |
| 4.3 | `/account/login` → `/` → `/committee/decisions-due` | Dr Mokoena | the same 75 rows |
| 4.4 | `/account/login` → `/` → `/committee/decisions-due` | Dr Sithole | the same, for his sub-speciality |
| 4.5 | `/committee/decisions-due` → `/access-denied` | Dr Naidoo | not in his menu; typed, access denied |
| 4.6 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | the form above the list, the period filling the window, dates in the browser's format, the preview (15 EPAs, 12 due); then the review, Scheduled, Start offered |
| 4.7 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | two more; three rows, each Open named for its review |
| 4.8 | `/committee/reviews` | Mr Smit | a second review of Dr Molefe: the preview warns of nothing; Create refused at the top, unfocused, "scheduled, scheduled", no link (T309) |
| 4.9 | `/committee/decisions-due` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Mokoena | three registrars Scheduled ("Open review" wrapping); Schedule on Dr Mahlangu's row opens the form filled; the review opens with no Start for her |
| 4.10 | `/committee/decisions-due` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Sithole | Dr Ndlovu the same way; all 75 rows Scheduled (9,556 px) |
| 4.11 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Naidoo | his panel's five reviews, no Schedule review; Start offered to him as a seated member |
| 4.12 | `/committee/reviews` → `/committee/decisions-due` | Prof Mbatha | all five scheduled; each row's Open review opens the review holding it |
| 4.13 | `/committee/panels` → `/committee/panels/{PanelId:int}` | Prof Mbatha | the committee change refused while reviews are open: a ten-line box in the narrow card; the select keeps the refused choice |
| 4.33 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | after the sitting: five Ratified, each Decision by its label (T250); the Period wraps mid-date |
| 4.34 | `/committee/decisions-due` | Prof Mbatha | 70 outstanding; Every status: Decided (STAR and review), Not decided (amber, D53), optional |
| 4.37 | `/admin/entrustment-decisions` → `/committee/decisions-due` | Dr Sithole | after a revocation (flow 09's list): "Revoked: re-decide" with Schedule; Not decided rows with Open review and Schedule stacked |
| 4.49 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | a formative check-in: Review type still shown ("Annual progression review", the semester-2 help); no agenda; the list reads "Formative"; the review "Formative check-in" (flow 07) |
| 5.1 | `/committee/decisions-due` | Mr Smit | Dr Molefe's fifteen rows, from row 16 of page 1 on: no trainee filter |
| 5.2 | `/committee/decisions-due` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | her pre-graduation review from a row: the form filled, the window widened to her start, the preview naming what is already decided; the semester-2 help |
| 5.29 | `/committee/reviews` | Mr Smit | only current registrars offered; a graduate and a withdrawn registrar are not |
| A.1.10 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Mr Smit | another formative check-in, for the data-rights appendix |
| A.7.5 | `/` → `/msf/campaigns` → `/msf/campaigns/{CampaignId:int}` → `/committee/decisions-due` → `/admin/data-rights` | Mr Smit at 390 | By EPA a focusable region with the design's ring, its EPA names cut as it scrolls; the list's Status and Action off-screen |
| A.7.8 | `/` → `/committee/reviews` → `/committee/decisions-due` → `/placeholder/{Feature}` → `/placeholder/{Feature}` | Dr Sithole at 390 | Committee reviews shows Trainee, Panel and a Period a word a line, the rest off-screen; a deleted user's pseudonym; Decisions due as above |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Committee. The step captures and
the `states/decisions-due--*`, `states/reviews-schedule--*` and `states/review-detail--*` captures are from the T361
replay (2026-10-12) on the shell as built; many are the same image as a step capture. The `states/panels-list--*` and
`states/panel-edit--*` captures are from 2026-09-26 on the old frame (a flat Title Case sidebar, an email in the top
bar): brief from their page body. Where a state has a step capture, brief from that.

| Page | State | Screenshot | Reached at |
|---|---|---|---|
| `/committee/panels` | No panels | `act-2/2.20-1-panels-empty.png` (`states/panels-list--empty.png`, old) | Step 2.20 |
| `/committee/panels` | The manager's list, who decides | `act-2/2.24-1-who-decides.png` (`states/panels-list--manager.png`, old) | Step 2.24 |
| `/committee/panels` | A speciality admin's list | `act-2/2.23-4-mokoena-panels-after.png` (`states/panels-list--speciality-admin.png`, old) | Step 2.23 |
| `/committee/panels` | A sub-speciality admin's list | `act-2/2.25-1-sithole-panels.png` | Step 2.25 |
| `/committee/panels` | A member's list | `act-4/4.14-1-botha-decision-panels.png` (`states/panels-list--member.png`, old) | Step 4.14 |
| `/committee/panels` | The Coordinator's list | `act-2/2.32-3-smit-panels.png` (`states/panels-list--coordinator.png`, old) | Step 2.32 |
| `/committee/panels` | Loading | `states/panels-list--loading.png` (old frame) | A held read: "no committee routing" too soon (T329) |
| `/committee/panels` | 390 px | `states/panels-list--narrow.png` (old frame, 375 px, scratch rows: do not attach) | Step A.7.7 |
| `/committee/panels/new` | The new-panel form | `states/panel-edit--new.png` (old frame; the only one with Decides for on a new form) | Step 2.21 |
| `/committee/panels/new` | Chair moved out of Members | `act-2/2.21-1-chair-chosen.png` (`states/panel-edit--chair-note.png`, old) | Step 2.21 |
| `/committee/panels/new` | A panel of one refused | `act-2/2.21-2-panel-of-one-refused.png` (`states/panel-edit--one-member.png` is a broken stitch: do not attach) | Step 2.21 |
| `/committee/panels/{PanelId:int}` | Saved | `act-2/2.22-1-panel-saved.png` (`states/panel-edit--saved.png`, old) | Step 2.22 |
| `/committee/panels/{PanelId:int}` | Read-only committee, members updated | `act-2/2.23-2-mokoena-panel-edit.png`, `act-2/2.23-3-botha-added.png` (`states/panel-edit--speciality-admin.png`, old) | Step 2.23 |
| `/committee/panels/new` | Speciality scope only | `act-2/2.25-2-sithole-new-panel.png` (`states/panel-edit--sub-speciality-admin.png`, old) | Step 2.25 |
| `/committee/panels/{PanelId:int}` | Committee change refused | `act-4/4.13-1-mbatha-committee-change-refused.png` (`states/panel-edit--body-refused.png`, old) | Step 4.13 |
| `/committee/panels/{PanelId:int}` | A member who can no longer sit | `states/panel-edit--unseatable.png` (old frame) | Scratch (post-appendix): Dr Botha locked out |
| `/committee/panels/{PanelId:int}` | Not found | `states/panel-edit--not-found.png` (old frame) | Typed `/committee/panels/999999` |
| `/committee/panels/{PanelId:int}` | 390 px | `states/panel-edit--narrow.png` (old frame, 375 px) | Step 2.22 |
| `/committee/panels/new` | Access denied | `act-2/2.32-4-new-panel-denied.png` | Step 2.32 |
| `/committee/decisions-due` | Nothing due | `states/decisions-due--nothing-due.png` | After Step 1.22, before any admission |
| `/committee/decisions-due` | An administrator, no institution | `states/decisions-due--choose-institution.png` | devadmin, any time after Act 1 |
| `/committee/decisions-due` | Outstanding | `act-4/4.1-1-smit-decisions-due-outstanding.png` (`states/decisions-due--outstanding.png`) | Step 4.1 |
| `/committee/decisions-due` | Filtered to one EPA | `act-4/4.1-2-smit-not-scheduled-paed004.png` | Step 4.1 |
| `/committee/decisions-due` | No row matches | `states/decisions-due--no-match.png` | Step 4.1, Status Missed |
| `/committee/decisions-due` | A past semester, missed | `act-4/4.2-1-mbatha-previous-semester-missed.png` (`states/decisions-due--missed.png`; 10,276 px) | Step 4.2 |
| `/committee/decisions-due` | Some scheduled | `act-4/4.9-1-mokoena-decisions-due-three-scheduled.png` | Step 4.9 |
| `/committee/decisions-due` | All scheduled | `act-4/4.10-4-sithole-all-scheduled.png` (`states/decisions-due--scheduled.png`) | Step 4.10 |
| `/committee/decisions-due` | After the sitting | `act-4/4.34-1-mbatha-decisions-due-every-status.png` (the 800 px top only); `states/decisions-due--after-sitting.png` (after Step 4.37: it includes the revoked row) | Step 4.34 |
| `/committee/decisions-due` | Revoked, to decide again | `act-4/4.37-2-sithole-revoked-re-decide.png` (`states/decisions-due--revoked.png`) | Step 4.37 |
| `/committee/decisions-due` | One registrar's rows | `act-5/5.1-1-smit-decisions-due-every-status.png`, `act-5/5.1-2-smit-molefe-rows-page1.png` | Step 5.1 |
| `/committee/decisions-due` | Loading | `states/decisions-due--loading.png` | A held read |
| `/committee/decisions-due` | Access denied, a member | `act-4/4.5-2-naidoo-decisions-due-denied.png` | Step 4.5 |
| `/committee/decisions-due` | 390 px | `act-A/A.7.5-4-decisions-due-390.png` (`states/decisions-due--narrow.png`), `act-A/A.7.8-4-decisions-due-390.png` | Steps A.7.5, A.7.8 |
| `/committee/reviews` | None yet, a scheduler | `states/reviews-schedule--empty-scheduler.png` | Step 4.1, before 4.6 |
| `/committee/reviews` | None yet, a member | `states/reviews-schedule--empty-member.png` | Step 4.5 |
| `/committee/reviews` | The form, no panel chosen | `states/reviews-schedule--form.png` | Step 4.6 |
| `/committee/reviews` | The agenda preview | `act-4/4.6-1-smit-molefe-schedule-form.png` (`states/reviews-schedule--preview.png`) | Step 4.6 |
| `/committee/reviews` | A second review refused | `act-4/4.8-1-smit-second-review-refused.png` (`states/reviews-schedule--refused.png`) | Step 4.8 |
| `/committee/reviews` | Scheduled | `act-4/4.7-1-smit-three-scheduled.png` (`states/reviews-schedule--scheduled.png`) | Step 4.7 |
| `/committee/reviews` | Filled from Decisions due | `act-4/4.9-2-mokoena-schedule-form-filled.png` (`states/reviews-schedule--prefilled.png`), `act-4/4.10-2-sithole-schedule-form-filled.png` | Steps 4.9, 4.10 |
| `/committee/reviews` | A member's list | `act-4/4.11-1-naidoo-five-scheduled.png` | Step 4.11 |
| `/committee/reviews` | Formative | `act-4/4.49-1-smit-formative-form.png` (`states/reviews-schedule--formative.png`), `act-4/4.49-3-smit-formative-scheduled-list.png` | Step 4.49 |
| `/committee/reviews` | Already decided in this window | `act-5/5.2-1-smit-pre-graduation-form.png` (`states/reviews-schedule--pre-graduation.png`) | Step 5.2 |
| `/committee/reviews` | Current registrars only | `act-5/5.29-1-smit-current-trainees-only.png` (`states/reviews-schedule--current-only.png`) | Step 5.29 |
| `/committee/reviews` | Ratified | `act-4/4.33-1-smit-five-ratified.png` (`states/reviews-schedule--ratified.png`) | Step 4.33 |
| `/committee/reviews` | Preview failed | `states/reviews-schedule--preview-failed.png` | A held read during Step 4.6 |
| `/committee/reviews` | Loading | `states/reviews-schedule--loading.png` | A held read |
| `/committee/reviews` | 390 px | `act-A/A.7.8-3-reviews-390.png` (`states/reviews-schedule--narrow.png`) | Step A.7.8 |
| `/committee/reviews/{ReviewId:int}` | Scheduled, Start offered | `act-4/4.6-2-smit-molefe-review-scheduled.png` | Step 4.6 |
| `/committee/reviews/{ReviewId:int}` | Scheduled, no Start | `act-4/4.9-3-mokoena-review-scheduled-no-start.png` (`states/review-detail--scheduled-no-start.png`) | Step 4.9 |
| `/committee/reviews/{ReviewId:int}` | Formative, scheduled | `act-4/4.49-2-smit-formative-scheduled.png` (`states/review-detail--formative.png`) | Step 4.49 |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- On the new shell: the panels list loading and at 390; a panel's page at 390, not found and with a member who can no
  longer sit; the InstitutionalAdmin's blank new-panel form. The old-frame captures above stand in.
- Anywhere: a panel's page loading; the Administrator with an institution chosen on Decisions due (D46); "You cannot
  create a decision panel" (T194; every manager in the cast can create); the "holds Trainee" notes on the panels and
  the schedule (T237, T256; no cast member holds Trainee beside a staff role); a second panel sitting as the neonatal
  committee (flow 07's scratch captures `states/review-detail--entrustment-only.png` and `--decided-elsewhere.png` show
  its review); field validation on either form; a result after a panel is created (there is none).
- Not built: the seat warning before Create and the hidden Review type (T309); draw them from § 1.

**Belongs to other flows:** flow 07: the review page itself. Flow 09: Entrustment decisions and the revocation (Step
4.37's first capture). Flow 14: the pseudonym after an erasure (`states/reviews-schedule--pseudonym.png`). Flow 06:
the Homes (`act-2/2.32-1-smit-home.png`, `act-4/4.3-1-mokoena-dashboard-nav.png`, `act-4/4.4-1-sithole-dashboard-nav.png`).

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-10-12; none shows a
link, a token or a password. Several are full pages of 3,000–10,000 px, which the canvas will see scaled: say so in the
message. In every tall capture at 1280 the sidebar's colour stops at 800 px: an artefact of the capture.

**Key screenshots (attach these first, with § 1):**
1. `act-2/2.21-1-chair-chosen.png`: the panel form, a 315 px card; the chair note; multi-selects cutting names
2. `act-2/2.22-1-panel-saved.png`: a saved panel's page that never names the panel (T260)
3. `act-2/2.24-1-who-decides.png`: the panels list and "Who decides each EPA"
4. `act-4/4.13-1-mbatha-committee-change-refused.png`: the refusal inside the narrow card; the select keeps the refused
   choice
5. `act-4/4.1-1-smit-decisions-due-outstanding.png`: Decisions due, outstanding (3,781 px)
6. `act-4/4.2-1-mbatha-previous-semester-missed.png`: Missed rows, each offering Schedule (10,276 px)
7. `act-4/4.37-2-sithole-revoked-re-decide.png`: Revoked and Not decided, Open review and Schedule stacked
8. `act-5/5.1-2-smit-molefe-rows-page1.png`: one registrar's rows start at row 16: no trainee filter
9. `act-4/4.6-1-smit-molefe-schedule-form.png`: the form and the agenda preview
10. `act-4/4.8-1-smit-second-review-refused.png`: the duplicate refused only after Create (T309)
11. `act-4/4.49-1-smit-formative-form.png`: Formative ticked, Review type still "Annual progression review" (T309)
12. `act-5/5.2-1-smit-pre-graduation-form.png`: pre-graduation; "Already decided"; the semester-2 help
13. `act-A/A.7.5-4-decisions-due-390.png`: Decisions due at 390: By EPA's ring; Status and Action off-screen
14. `act-A/A.7.8-3-reviews-390.png`: Committee reviews at 390: three columns visible, a pseudonym
15. `states/panels-list--loading.png`: "no committee routing" while loading (old frame: its body only)

**States (attach as the chat asks):** every row of § 3 not already attached above, flagging the old-frame ones.

**Do not attach:** `states/panel-edit--one-member.png` (a broken stitch) or `states/panels-list--narrow.png` (375 px,
scratch rows).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `b62997a5`.

| Task | What it means for the design | Evidence |
|---|---|---|
| **A5** (BRIEF § 6; no task) | The schedule's refusals are a danger Alert at the page's top, above the form (`W/ReviewsSchedule.razor:21–24`), with no focus move; Create has no busy state (`:94`). The panel form's refusals are in `ActionResult` but not focused (`W/PanelEdit.razor:16–19`); Save committee's refusal sits in its card and the select keeps the refused value (`:422–425`). A new panel's save reloads the page with no result (`:646`). Each outcome shows, and takes the focus, where the person works. | `act-4/4.8-1-smit-second-review-refused.png`, `act-4/4.13-1-mbatha-committee-change-refused.png` |
| **T309** (P3) | Item 1: the preview runs the scope check and the planner only (`A/PreviewCommitteeAgenda.cs:46–86`); the seat check is the create's (`A/ScheduleCommitteeReview.cs:124–130`, `:233–237`), so the warning comes after Create. Item 2 (the list's "Formative") is done (`44de180b`). Item 3: Review type stays shown while Formative only is ticked (`W/ReviewsSchedule.razor` Review type and `#review-formative`). The refusal doubles "scheduled" ("(scheduled, scheduled 2026-10-12)"). | `act-4/4.8-1…`, `act-4/4.49-1…` |
| **T260** (P3) | The panel's page is titled "Decision panel" (`W/PanelEdit.razor:10`); name, scope and speciality render only on a new panel (`:115–173`). The unseatable warning is not reset after a save (`:625–634`) and has no live role. Decision panels does not say why New panel is missing (`W/PanelsList.razor:12–15`). | `act-2/2.22-1-panel-saved.png`, `states/panel-edit--unseatable.png` |
| **T259** (P3) | The holding sentence names the panel the EPA routes to (`SchedulePanelName`, `W/DecisionsDueText.cs:80–84`), not the holding review's own panel. | Step 4.37 |
| **T329** (P2) | `_routingLoading` starts false (`W/PanelsList.razor:184`), so "There is no committee routing to show for your institution." shows while the list loads. | `states/panels-list--loading.png` |
| **T338** (P3) | No page here passes `StatePanel`'s `OnRetry`; a failed read prints `RefusalText`. The preview's failure prints the exception (`W/ReviewsSchedule.razor` `AgendaPreview`, "The agenda could not be previewed: …") and leaves Create live (T272 too). Decisions due hides an Administrator's failed institution list behind "Choose an institution" (`W/DecisionsDue.razor:63`, `:223–230`). | `states/reviews-schedule--preview-failed.png` |
| **T324** (P3) | Only Chair has a message (`W/PanelEdit.razor:687`); Panel name, Trainee and Panel print DataAnnotations' defaults (inferred: "The Name field is required."). | — |
| **T280** (P3) | `PagerControls`' "Per page:" is a loose span (`Shared/PagerControls.razor:30–31`); the panels and reviews tables have no caption and no `scope`; every page goes h1 → h3 → h4. | Step 4.1 |
| **T264** (P3) | Row actions are named per row (keep). The page does not move focus after a page change (`W/DecisionsDue.razor:271`). | Step 4.1 |
| Missed rows (observed, not filed) | Every row wanting a sitting offers Schedule (`W/DecisionsDue.razor:148–177`), a Missed row of an ended semester included: Schedule then opens the form on that past semester (question 7). | `act-4/4.2-1…` |
| One registrar (observed, not filed) | Decisions due has no trainee filter; rows sort by trainee then EPA, 20 a page (`A/GetEntrustmentDecisionsDue.cs:350–356`). | `act-5/5.1-2…` |
| 390 px (observed, not filed) | Decisions due's list is a raw table with no stacking (`W/DecisionsDue.razor`, `#due-items`); the reviews list a `DataTable` with no `Stack`. Both scroll sideways in their boxes. | `act-A/A.7.5-4…`, `act-A/A.7.8-3…` |
| The panel form (observed, not filed) | One `section.detail-card` in a `details-grid` (`1fr 2fr`); native multi-selects (`W/PanelEdit.razor`, Members and External members); options "{First} {Last} ({email})" (`:432–433`). `Design/MultiSelectOptionKeyTests` pins today's multi-select. | `act-2/2.21-1…` |
| The panels list (observed, not filed) | Scope prints the enum (`W/PanelsList.razor:51`); no speciality, no institution, even for an Administrator who sees every institution's panels. | `act-2/2.24-1…` |
| Panel rules (observed; question 8) | No command renames a panel or changes its scope; `UpdateDecisionPanel` changes members with no open-review check (`A/UpdateDecisionPanel.cs:63–84`); only `SetDecisionPanelBody` refuses while a review is open (`A/SetDecisionPanelBody.cs:172–220`). | Step 4.13 |
| Two labels (observed; question 9) | The schedule's "2026 S2 · 1 Jul to 31 Dec 2026" (`A/CommitteeReviewPeriods.cs:92–94`); the registrar pages' "Semester 2, 2026 · July to November" (D40, flow 05). | `act-4/4.6-1…` |
| The form's dates (observed) | `InputDate` renders in the browser's locale ("2026/01/01"); the list is ISO. | `act-4/4.6-1…` |
| The pre-graduation help (observed) | The type's help is by semester only, so a pre-graduation review reads "The semester-2 sitting decides the trainee's progression for the year." | `act-5/5.2-1…` |

**Dropped from the old brief (fixed since):** the Coordinator's missing Decision panels link (flow 06, `85d5a508`;
`NavItems.cs:175`); the browser's thin focus ring on By EPA (the global ring, T328); the schedule list's formative label
(flow 07, `44de180b`); Entrustment decisions missing from the speciality admins' menus (flow 06); the pager's buttons
disabling themselves at either end (T350: aria-disabled).

## 6. Questions the design must answer

1. **One page or two:** Decisions due and the schedule, and where the scheduling form lives.
2. **One registrar's rows** (5.1): a trainee filter, grouped rows, or a per-registrar view.
3. **The held seat before Create** (4.8; T309).
4. **The lists at 390** (A.7.5, A.7.8).
5. **Choosing members** with whole names, at 1280 and by touch at 390.
6. **A panel's own page:** what it shows above its forms (T260), and where Decides for and the members sit.
7. **(The operator's) Missed rows of an ended semester:** keep Schedule, refuse it, or schedule for the current semester?
8. **(The operator's) The panel's rules:** a name and scope fixed at creation; members changeable while reviews are open.
9. **(The operator's) One semester, two labels** (D40): keep both and explain, or align?
10. **(The operator's) T309, T259, T260, T329, T338, T324, T272, T280:** folded into the build, or any landed first?

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps span Acts 2, 4, 5 and the appendix; § 9): every
  Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** 4.8 (the warning before
  Create); 4.49 and A.1.10 (no Review type when formative); 2.22 (the panel's page names it; a result after create);
  4.13 (the refusal where the committee is chosen); 5.1 (one registrar's rows); every Route line through scheduling if
  the form moves (4.6, 4.7, 4.9, 4.10, 5.2); A.7.5 and A.7.8 (the lists at 390); search the whole runbook (BRIEF.md § 9
  item 7) for the words that change. A new page or route needs a step (`Scenario/ScenarioRunbookTests`). A new step
  takes a letter when its number is taken (§ 11, flow 06).
- **Checks the design adds:** 2.20 and 4.1 with a held read, the header and a skeleton, never "no committee routing"
  (T329); 2.22, the panel named (T260); 4.8, the warning before Create with a link to review #1 (T309); 4.13, the
  refusal beside the select and the saved value shown; 4.49, no Review type (T309); 4.37, the holding review's own panel
  (T259); A.7.5 and A.7.8, nothing scrolls sideways at 390, every page checked at 768 as well; one h1 and headings in
  order on every page; every table captioned; the pager's select named (T280).
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include:
  - `CommitteeDecisions/DecisionsDuePageTests`; `PanelDecisionBodyTests`, `PanelEdit*Tests`, `PanelPagesTraineeFirstTests`;
    `ReviewsSchedule*Tests` (AgendaPreview, ListScope, PanelList, Prefill, TraineeFirst, TraineePicker);
    `CommitteeReviewStateLabelTests`;
  - `Navigation/NavMenuAuthorizationTests`, `ActiveNavItemTests`; `Hosting/PageTitleTests`;
  - `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`, `IdReferencesTests`;
  - `Design/*`, including `MultiSelectOptionKeyTests` (which changes with the members' control), `NarrowLayoutTests`,
    `DefinedClassTests`, `ContrastTests`; the scenario guard in `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones on scratch copies, as `states.md` says) and the steps' own
  captures, keeping their names; `check_baseline_paths.py` at missing 0. Compare them with the chosen artboards.
- **Browser check** at 1280, 768 and 390 px as the InstitutionalAdmin, the SpecialityAdmin, the SubSpecialityAdmin, the
  Coordinator and a CommitteeMember.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as flow 07's build left it (T361, 2026-10-12). `D` is the replay day.

```text
Step 2.20 — Decision panels before any exist
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Open Decision panels.
Expect: While the page reads, the header shows at once over skeletons: no New panel yet, and Who decides each EPA draws
  skeletons, never a sentence saying there is no routing (T329). Then the page reads "No decision panels" / "Create a
  panel before scheduling reviews." and offers New panel in the header. "Who decides each EPA" names `Paediatric EPA
  Curriculum 11.1` in one row, "PAED-001 to PAED-015", against "No panel at this institution covers this programme."

Step 2.21 — The new-panel form, and a panel of one refused
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new
Do: Name it "Paed Annual Review Panel", with Scope Speciality, Speciality Paediatrics and Decides for General panel.
  Under Who sits on it, seat Zulu and Naidoo as Member, then choose Zulu as Chair. Set Naidoo to Not seated, leaving the
  chair alone, and press Create panel.
Expect: The page reads "New panel": "A panel takes entrustment and progression decisions for the trainees it covers. Its
  name, scope, institution and speciality cannot change once it is created."
  - Scope offers Institution and Speciality and starts on Speciality. Speciality offers Paediatrics alone ("The panel
    runs at your institution. Only the specialities it has adopted a curriculum in are listed."). Decides for offers
    General panel and the Neonatal team Clinical Competency Committee.
  - Chair, and the seat picker under it (one row a person: Not seated, Member or External member), each list exactly
    Botha, Naidoo, van Rensburg and Zulu, by surname, each with the whole address (van Rensburg's on
    `sun.wombat.local`): active committee members at KGK who are not trainees (T165, T237).
  - Choosing Zulu as chair says "Thandi Zulu is the chair now, so is no longer seated as a member." (T257), and her row
    reads "Chair · chosen above".
  - Create panel is refused and nothing is created: "The panel was not created. Everything you chose is kept below.",
    with "Seats: A panel needs at least two members: the chair and at least one other, so that no decision is one
    person's." (D46). The summary takes the focus, and the seats are marked with the same sentence.

Step 2.22 — Mbatha saves the panel
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels/new → /committee/panels/{PanelId:int}
Do: Seat Naidoo as Member and van Rensburg as External member, then press Create panel. Botha is left Not seated, for
  Dr Mokoena in Step 2.23.
Expect: The panel's own page opens, and names the panel: its h1 and tab read "Paed Annual Review Panel", its subtitle
  "Speciality panel · Paediatrics · Kgosi Kgari Teaching Hospital" (T260). "Paed Annual Review Panel was created. Its
  reviews can now be scheduled on Committee reviews." takes the focus.
  - **About this panel:** Scope "Speciality: Paediatrics", Institution Kgosi Kgari Teaching Hospital, Open reviews
    None, and "Its name, scope, institution and speciality cannot change once it is created. Its members can, even
    while its reviews are open."
  - **Who sits on it:** Chair Zulu; Naidoo Member, van Rensburg External member, Botha Not seated; "3 seats: the chair,
    1 member and 1 external member." The externals' help says external members sit with the chair on the panel's
    appeal body, and that cross-institution externals are not yet supported.
  - **Decides for:** General panel, with Save committee and the rule under it.
  There is no "Back to panels".

Step 2.23 — Dr Mokoena adds Dr Botha to the panel
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/panels → /committee/panels/{PanelId:int} → /committee/panels
Do: Sign in, open Decision panels and Edit the panel. Seat Botha as Member, press Save panel, then go back to Decision
  panels from the menu.
Expect: The list offers her New panel and an Edit on this panel, a Speciality-scoped panel in her speciality at her
  institution (T194). On the panel's page Decides for is read-only: "General panel", with "Only an institutional
  administrator can change which College committee a panel sits as." Save panel reads "Panel members updated. Thandi
  Zulu chairs, with 2 members and 1 external member.", with the focus on it. Back on the list, Members names the four,
  "4: Thandi Zulu (chair), …, John van Rensburg (external)", with Sarah Botha, then David Naidoo, between (the members
  in the picker's surname order).

Step 2.24 — Who decides each EPA now
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels
Do: Read the list and Who decides each EPA.
Expect: One row: Paed Annual Review Panel, Scope "Speciality · Paediatrics", Decides for General panel, and Members
  naming the four (the chair first, van Rensburg marked external), with an Edit. Who decides each EPA has two rows:
  "PAED-001 to PAED-003, PAED-006 to PAED-015" go to Paed Annual Review Panel, and "PAED-004, PAED-005" to "Paed Annual
  Review Panel. No panel covering this programme sits as the Neonatal team Clinical Competency Committee, so the
  general panel decides." (T131).

Step 2.25 — Dr Sithole looks at what he may create
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/panels → /committee/panels/new → /committee/panels
Do: Sign in, open Decision panels, press New panel and read the form. Then Cancel without creating.
Expect: The list offers him New panel and an Edit on the panel. The form's Scope reads "Speciality" as text, with "A
  sub-speciality administrator creates Speciality-scoped panels for the speciality of the sub-speciality.": a
  sub-speciality administrator manages the Speciality-scoped panels of his sub-speciality's speciality (T194, T245).
  Speciality offers Paediatrics. There is no Decides for field. Cancel returns to Decision panels; nothing is created.

Step 2.32 — Mr Smit, Coordinator
Role: Coordinator — Mr Pieter Smit
Route: /account/login → / → /admin/invitations → /access-denied → /committee/panels → /committee/panels/new → /access-denied
Do: Sign in and read the dashboard and nav. Type the invitations address. Open Decision panels from the menu, then type
  the new-panel address.
Expect: Home reads "Coordinator · Semester N, YYYY", with the header action "Start an MSF campaign"
  (`/msf/campaigns/new`), where a Quick action card was (T358, Q6). Its cards, in order:
  - "Waiting for assessors", with no badge: "Nothing is waiting for an assessor.", with Open Waiting for assessors;
  - "Nothing filed in 30 days", its rule line "Current registrars with nothing filed (a draft is not filed) in the last
    30 days. A registrar admitted less than 30 days ago is not listed.", then "Every current registrar has filed
    something in the last 30 days.", with Open in Programme trainees: the registrars were admitted today, and a
    registrar admitted less than 30 days ago is not listed (E5);
  - "Invitations nearing expiry": "No invitations expiring soon."
  The sidebar reads "Acting as Coordinator" over Home, Programme trainees, Waiting for assessors, Decisions due, MSF
  campaigns, Committee reviews, Decision panels and Data rights requests, one flat list (E2), then My data rights. It
  has no Invitations, and the invitations page reads "You cannot open this page", "Your role (Coordinator) does not open
  this page." and "If you need it for your work, ask your institution's Wombat administrator.", with Go to Home (T178,
  T335). Decision panels lists the panel (Scope "Speciality · Paediatrics", its four members by name) with no New
  panel and no Edit column, and Decision panels is lit there (T358, C10). The panel form reads "You cannot open this
  page" in the same words: the Coordinator reads panels, and edits none (D4).

Step 4.1 — Mr Smit reads the decisions due
Role: Coordinator — Mr Pieter Smit
Route: / → /committee/decisions-due
Do: Open Decisions due from the menu. Leave Period on `<P>` and Status on "Outstanding: not yet decided", and open By
  EPA. Page through the list with Next. Then set Status to "Not scheduled" and EPA to PAED-004, and press Show.
Expect: The opening sentence names `<P>`, "5 trainees you oversee at Kgosi Kgari Teaching Hospital", and what Missed
  means. Period's help reads "The College's semester 2 ends in November; its window runs to 31 December, so December's
  work counts." By EPA ("Counts for the 15 EPAs due in `<P>`") lists all 15 EPAs, each Due 5.
  - Twelve EPAs read To schedule 5.
  - PAED-008, 009 and 013 read Optional 5.
  The count reads "75 of 75 decisions due in `<P>` shown.", and the list pages 20 rows at a time.
  - Each semester EPA's row (PAED-001, 002, 004, 005, 010, 012) reads Not scheduled, window `<P>`, "No open review
    holds it, and `<P>` has not ended.".
  - Each annual EPA's row (PAED-003, 006, 007, 011, 014, 015) reads the same with window `<Y>`: "… and `<Y>` has not
    ended.".
  - The three optional EPAs read "As opportunity allows".
  Every Not scheduled row offers one action, Schedule, named by trainee, EPA and panel ("Schedule a review of … for
  PAED-001 before Paed Annual Review Panel"). After Next the focus is on the heading, which reads "Decisions, page 2 of
  4" (T264). The filters leave five PAED-004 rows, one per registrar ("5 of 75 decisions due in `<P>` shown: Not
  scheduled, PAED-004."), with Clear filters offered; By EPA does not change. No Missed row exists.

Step 4.2 — Prof Mbatha reads the same period
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /committee/decisions-due → /committee/reviews/new → /committee/decisions-due
Do: Open Decisions due from the menu. Look for an Institution filter. Switch Period to the previous semester, `<Y>` S1,
  and press Show, then open By EPA. Press Schedule on one Missed row and read the form it opens, then Cancel without
  creating. Back on Decisions due, switch Period to `<P>` and press Show.
Expect: No Institution filter is offered, because she reads her own institution. The same five registrars and 75 rows
  as Step 4.1. For `<Y>` S1 the statuses follow that period's windows:
  - every registrar's six semester EPAs read Missed ("`<Y>` S1 has ended with nothing decided or on an open review's
    agenda.", T361). Mahlangu and Ndlovu started on 15 January, inside the semester's first month, so their windows
    count too (D42);
  - the annual EPAs read "Due by year end", so PAED-003 and 006 are never Missed;
  - PAED-008, 009 and 013 read "As opportunity allows", since an optional EPA is never missed;
  - By EPA reads Missed 5 on the six semester EPAs.
  A Missed row's Schedule opens Schedule a review filled with the panel, that registrar and the row's own period (F1),
  "`<Y>` S1 · 1 Jan to 30 Jun `<Y>`", the window `<Y>`-01-01 to `<Y>`-06-30. Period's help begins "`<Y>` S1 has ended. A
  review can still sit for it: sittings are often held after the period ends." Review type offers Annual progression
  review (chosen), Pre-graduation review and Entrustment-only review, then Formative check-in. The Agenda reads `<Y>`
  S1's plan as the planner gives it: "PAED-001, PAED-002, PAED-004, PAED-005, PAED-010 and PAED-012 are due at this
  sitting.", the six annual EPAs "Due by year end" and PAED-008, 009 and 013 "As opportunity allows". Cancel returns to
  Decisions due on `<Y>` S1. Nothing is created.

Step 4.3 — Dr Mokoena reads what is due in her speciality
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /account/login → / → /committee/decisions-due
Do: Sign in, read her menu, and open Decisions due.
Expect:
  - **Menu:** Home, Programme trainees, Waiting for assessors, Decisions due, Committee reviews, Decision panels and
    Entrustment decisions, then My data rights (T358, R2-Menus). Decisions due is lit on its page.
  - **Home's cards:** Waiting for assessors, Registrars and Targets by EPA, read for Paediatrics. Waiting for assessors
    holds Dr du Plessis's CBD and portfolio review, both still open from Act 3, the review Overdue; Registrars the five,
    badged "5 registrars".
  - **The page:** the same five registrars, all in Paediatrics, and 75 rows, each with one action, Schedule.
  - **Entrustment decisions** is in the menu (Step A.7.7 opens it from there); no card on Home names it.

Step 4.4 — Dr Sithole reads what is due in his sub-speciality
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /account/login → / → /committee/decisions-due
Do: Sign in and open Decisions due.
Expect: The same five registrars and 75 rows as Step 4.3: all five train in the Paediatrics sub-speciality Dr Sithole
  administers. Dr Sithole's menu and Home's cards read as Dr Mokoena's.

Step 4.5 — Dr Naidoo is not offered what is due
Role: CommitteeMember — Dr David Naidoo
Route: /dashboard/switch/{role} → / → /committee/decisions-due → /access-denied
Do: He still acts as Assessor, his choice from Step 3.5: choose Switch to Committee member in the sidebar. Look for
  Decisions due in the menu, then type its address.
Expect: His menu has Committee reviews and Decision panels but no Decisions due. The typed address lands on the
  access-denied page, and nothing in his menu is lit there: the page is for the roles that schedule reviews.

Step 4.6 — Mr Smit schedules Dr Molefe's review
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews from the menu and press Schedule a review. Fill in:
  - Panel: `Paed Annual Review Panel`.
  - Trainee: Dr Molefe.
  - Period: `<P>`, and leave the evidence window as it fills.
  - Scheduled on: `D`, as it starts.
  - Review type: Annual progression review, as it starts.
  Read the Agenda, then press Create review.
Expect:
  - **The page:** Schedule a review is a page of its own, under the trail Home › Committee reviews › Schedule a review
    (T364, Q1).
  - **Panel:** the one panel is offered. Trainee is disabled until a panel is chosen ("Choose a panel first. The list
    holds the trainees at its institution you can schedule."), then lists the five registrars, each with the training
    year ("Lerato Molefe · Training year 4").
  - **Period:** labelled "`<P>` · 1 Jul to 31 Dec `<Y>`". Choosing it fills the window as `<Y>`-01-01 to `<Y>`-12-31
    and adds "The College's semester 2 ends in November; its window runs to 31 December, so December's work counts."
  - **Review type:** radios, "Annual progression review" (chosen) and "Pre-graduation review", with the help "The
    semester-2 sitting decides the trainee's progression for the year." (T131 slice 5), then Formative check-in (T309
    item 3).
  - **The Agenda** reads "15 EPAs will be on the agenda for `<P>`." It then says "PAED-001, PAED-002, PAED-003,
    PAED-004, PAED-005, PAED-006, PAED-007, PAED-010, PAED-011, PAED-012, PAED-014 and PAED-015 are due at this
    sitting." (T361, round 3 item 29: no deferral, D53). It lists PAED-008, 009 and 013 as "As opportunity allows".
  - **After Create review:** the page opens the new review (T361):
    - the h1 and the tab read "Lerato Molefe · `<P>`" ("… · Wombat" in the tab);
    - the subtitle reads "Annual progression review · Paed Annual Review Panel · Training year 4";
    - the sitting bar, under the title, reads Scheduled, "15 EPAs on the agenda" and "Summative · evidence
      `<Y>`-01-01 to `<Y>`-12-31".
  - **Start review…** is offered to Smit in the sitting bar, since the coordinators of the panel's institution may start
    a review (E2). He leaves it for the chair.
  - **Evidence snapshot:** "No evidence has been frozen yet. Starting the review captures the current bundle."

Step 4.7 — Mr Smit schedules Dr Dlamini's and Dr du Plessis's reviews
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int}
Do: From Schedule a review, schedule the same kind of review for Dr Dlamini, then for Dr du Plessis: same panel, `<P>`,
  window as filled, Scheduled on `D`, Annual progression review.
Expect: Each opens as Scheduled with a 15-EPA preview like Step 4.6's. Committee reviews, captioned "Committee reviews
  at Kgosi Kgari Teaching Hospital, newest scheduled first" under "The College's semester 2 ends in November; …",
  lists three rows for `<P>`. Each row reads:
  - Period "`<P>`" over its window, "`<Y>`-01-01 to `<Y>`-12-31";
  - Type Annual progression;
  - State Scheduled, a grey badge, as on Decisions due;
  - Decision Pending.
  There is no Mode column (C8). Each row's Open is named by the review it opens.

Step 4.8 — A second review of Dr Molefe for the same period is refused
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new
Do: Press Schedule a review and choose the panel, Dr Molefe and `<P>`, as for another Annual progression review. Read
  the Agenda before Create.
Expect: Before Create, the Agenda says the seat is held (T309 item 1): "Already held. Review #N already puts Lerato
  Molefe before Paed Annual Review Panel for `<P>` (scheduled on `D`). A trainee has one binding review for each period
  …", then "A formative check-in takes no seat: choose Formative check-in under Review type to schedule one." Create
  review is not offered: "Create review is not offered: review #N already holds `<P>` for Lerato Molefe.", beside Cancel
  and Open review #N. The wording is a product decision (T131 slice 4; T364, C7). Nothing is created; the list still
  holds three rows.

Step 4.9 — Dr Mokoena schedules Dr Mahlangu's review from what is due
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /committee/decisions-due → /committee/reviews/new → /committee/reviews/{ReviewId:int}
Do: On Decisions due, set Trainee to Dr Molefe, press Show and read the line above the list. Then set Trainee to Dr
  Mahlangu, press Show, and press Schedule a review on the line (or Schedule on her PAED-001 row). Check the form it
  opens, set Scheduled on `D`, and create the review.
Expect:
  - **Decisions due before she schedules:** Molefe's, Dlamini's and du Plessis's rows read Scheduled, "On the agenda of
    review #N before Paed Annual Review Panel, which is still open." (T259), each with Open review #N.
  - **The trainee line** (C2): with Dr Molefe filtered, "Review #N (annual progression) holds `<P>` before Paed Annual
    Review Panel, scheduled on `D`." with Open review #N; with Dr Mahlangu, "No open review holds `<P>`." with Schedule
    a review.
  - **The link opens the schedule page already filled:** Panel `Paed Annual Review Panel`, Trainee Dr Mahlangu and
    Period `<P>`. The preview shows 15 EPAs. The query string fills only what the form offers her, so the link
    authorises nothing.
  - **The review opens as Scheduled,** and Start review… is not offered to her. Her sitting bar, static under the title,
    says: "Only the coordinators of the panel's institution, and those of its members who are active committee members
    there, can start this review." (T194, E2), with "As of hh:mm SAST" and Refresh.

Step 4.10 — Dr Sithole schedules Dr Ndlovu's review from what is due
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /committee/decisions-due → /committee/reviews/new → /committee/reviews/{ReviewId:int} → /committee/decisions-due
Do: On Decisions due, press Schedule on one of Dr Ndlovu's rows. Set Scheduled on `D` and create the review. Then go
  back to Decisions due and open By EPA.
Expect: As Step 4.9: the form is filled, the review opens as Scheduled, and Start review… is not offered: the sitting
  bar says who can start it. Decisions due then reads Scheduled on all 75 rows, and By EPA reads Scheduled 5 and To
  schedule 0 for every EPA.

Step 4.11 — Dr Naidoo lists the panel's reviews
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews, then open Dr Dlamini's review.
Expect:
  - **The list:** its subtitle reads "Open existing committee reviews.", and no Schedule a review is offered. It
    lists the five reviews of the panel he sits on, all Scheduled, with Decision Pending.
  - **Dlamini's review:** Start review… is offered to him in the sitting bar, as a member who may sit on the panel (E2).
    He leaves it.

Step 4.12 — Prof Mbatha checks the schedule
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/reviews → /committee/decisions-due
Do: Read Committee reviews, then Decisions due. Set Trainee to Dr Ndlovu and press Show.
Expect: Committee reviews offers Schedule a review and lists all five reviews. Decisions due reads Scheduled on all 75
  rows, each with Open review #N, which opens the review holding it; no row offers Schedule. With Dr Ndlovu filtered,
  the line reads "Review #N (annual progression) holds `<P>` before Paed Annual Review Panel, scheduled on `D`." with
  Open review #N.

Step 4.13 — The panel cannot change what it decides while its reviews are open
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/panels → /committee/panels/{PanelId:int}
Do: Open the panel from Decision panels and read Decides for.
Expect: Decides for reads "General panel", with no select and no Save committee: the lock is said first (F5), "It
  cannot change now. Review #N (`<P>`, scheduled) still sits before this panel. What a review decides, and its agenda,
  follow the College committee the panel sat as when it was scheduled …" (T131 slice 5), with Open review #N, the
  first review still open before the panel. About this panel's Open reviews names the same review, linked, "and 4
  more".

Step 4.33 — Mr Smit reads the ratified schedule
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Read Committee reviews, then open Dr Molefe's review.
Expect: All five read Ratified, Type Annual progression; there is no Mode column (C8). The Decision column reads:
  - Satisfactory Progress for Molefe and Dlamini;
  - Satisfactory with Observations for du Plessis;
  - Inadequate Progress — Additional Training for Mahlangu;
  - Outcome Deferred for Ndlovu.
  No state or category prints an enum name (T250). Molefe's review shows its decision, agenda and STAR numbers, and
  offers Smit no Ratify or Start, only the header's "Export Lerato Molefe's portfolio" (T379, Step 5.12).

Step 4.34 — Prof Mbatha reads what is due after the sitting
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /committee/decisions-due
Do: Read `<P>` with the default filter. Then set Status to "Every status", press Show, and open By EPA.
Expect:
  - **Count, default filter:** "70 of 75 decisions due in `<P>` shown."
  - **Every status:**
    - Decided on five rows, each "STAR #n, issued at review #N.": Molefe's PAED-001, 010 and 012, and Dlamini's PAED-001
      and 002;
    - Not decided on 55 rows, its badge amber, "Not decided at review #N. A later sitting for `<P>` plans it again.",
      or `<Y>` for an annual EPA (T361, D53, E3: each was due at that sitting);
    - As opportunity allows on 15: an optional EPA left unstaged is recorded as not decided at its review, but its
      window `<Y>` has not ended, so it still reads "Decided as opportunity allows: optional, never missed." (E3).
  - **By EPA:** PAED-001 reads Due 5, Decided 2 and Not decided 3.
  - **Actions:** each row has one (C3). A Not decided row offers only Schedule, since no open review holds its seat now;
    its review is a link in its sentence. A Decided row offers none: "Decided: nothing to schedule".

Step 4.37 — Dr Sithole reads the list and what must be decided again
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /admin/entrustment-decisions → /committee/decisions-due
Do: Open Entrustment decisions from the menu. Set Status to Revoked and press Show, then to Active and press Show.
  Download the revoked certificate. Then open Decisions due, read the count, set Status to "Every status", press Show,
  and open By EPA.
Expect:
  - **The menu:** Entrustment decisions is lit on its page, with no trail (T358, R2-Menus m5).
  - **Filtered:** Revoked leaves Dlamini's PAED-002, with its line "Revoked `D` at hh:mm SAST by Refilwe Mokoena: …";
    Active leaves four rows.
  - **The revoked certificate** reads Status Revoked, with "REVOKED", "Revoked on `D` by Refilwe Mokoena" (the
    South African day, ISO, T366 G6; who revoked it by name, T142, T324) and the reason.
  - **Decisions due:** Dlamini's PAED-002 reads "Revoked: re-decide". Its detail reads "STAR #n, issued for `<P>`, was
    revoked. Schedule a review to decide it again." Its one action is Schedule (C3).
  - **By EPA:** PAED-002 reads Decided 0, Not decided 4 and To schedule 1.
  - **Count, default filter:** "71 of 75 decisions due in `<P>` shown."

Step 4.49 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int} → /committee/decisions-due
Do: Press Schedule a review and schedule a review of Dr Ndlovu before the panel for `<P>`, Scheduled on `D`, with
  Review type Formative check-in; create it. Then open Decisions due, set Trainee to Dr Ndlovu and press Show.
Expect:
  - **Accepted:** a formative review takes no binding seat for the period (T131 slice 4). Formative check-in's help
    reads "Closes without a binding decision and cannot issue entrustment decisions. Use for interim progress
    check-ins. It takes no seat for the period." (T309 item 3).
  - **The preview:** "A formative review carries no agenda: it decides no EPA."
  - **The review:** its h1 reads "Sipho Ndlovu · `<P>`" and its subtitle "Formative check-in · Paed Annual Review
    Panel · Training year 1" (T309 item 2). The sitting bar reads Scheduled and "Formative: an interim check-in, no
    binding decision · evidence `<Y>`-01-01 to `<Y>`-12-31" (round 3 item 28), with Start review… for Smit. It has no
    Decision, Agenda or Appeals section: only the evidence snapshot, the standing, MSF coverage and the trajectories.
  - **The list:** the review's Type reads "Formative check-in" and its Decision "—", read as "None"; there is no Mode
    column (C8).
  - **Decisions due's line:** "No open review holds `<P>`." with Schedule a review, and "Formative check-in #N is open:
    it takes no seat." with Open check-in #N.

Step 5.1 — Mr Smit reads what Dr Molefe's committee still has to decide
Role: Coordinator — Mr Pieter Smit
Route: /committee/decisions-due
Do: Open Decisions due for the period holding `D`. Set Trainee to Dr Molefe and Status to "Every status", and press
  Show. Then set Trainee back to "Every trainee" and press Show.
Expect: The line above the list reads "No open review holds `<P>`." with Schedule a review: Act 4's review of her is
  ratified. There is one row for each EPA due for her in the period ("15 of 75 decisions due in `<P>` shown: Lerato
  Molefe, every status."), and each row gives its status in words:
  - an EPA that an Act 4 STAR decided reads Decided, and names the STAR and the review that issued it;
  - an EPA due at Act 4's sitting and left unstaged there reads Not decided (amber), "Not decided at review #N. A later
    sitting for … plans it again." (T361, D53, E3);
  - the others read Not scheduled, Due by year end or As opportunity allows: PAED-008, 009 and 013, recorded as not
    decided at Act 4 but optional, read As opportunity allows while their window `<Y>` is open (E3).
  Every row that is not yet decided offers Schedule. While the filter is set only her rows are listed; with Every
  trainee, KGK's other registrars are listed too, and nobody outside KGK is.

Step 5.2 — Mr Smit schedules Dr Molefe's pre-graduation review
Role: Coordinator — Mr Pieter Smit
Route: /committee/decisions-due → /committee/reviews/new → /committee/reviews/{ReviewId:int}
Do: Set Trainee to Dr Molefe again and press Show (Step 5.1 ended on Every trainee, where no trainee line shows). Press
  Schedule a review on Dr Molefe's line (or Schedule on one of her outstanding rows). Then complete the form:
  - review type: Pre-graduation review;
  - evidence window: From widened to her programme start, `J−3y`;
  - scheduled on: `D`.
  Create the review.
Expect: The form opens with the Paed Annual Review Panel, Dr Molefe and the period already filled in. Review type offers
  Annual progression review (chosen) and Pre-graduation review, then Formative check-in; Entrustment-only review is
  offered only in a semester-1 period. Pre-graduation review's help reads "The final review before the programme ends:
  it decides the trainee's progression, and whether they may graduate." (C1). The agenda preview lists the EPAs that
  are due and names those Act 4 decided. In the same semester-2 period as Act 4:
  - it reads "12 EPAs will be on the agenda for …", with the period's label;
  - it names the nine EPAs Act 4 did not decide that are due here: "PAED-002, PAED-003, PAED-004, PAED-005, PAED-006,
    PAED-007, PAED-011, PAED-014 and PAED-015 are due at this sitting." (T361, round 3 item 29);
  - it lists PAED-008, 009 and 013 as As opportunity allows;
  - it says "Already decided in this window, so not on the agenda: PAED-001, PAED-010 and PAED-012."
  The new review's h1 reads "Lerato Molefe · `<P>`", its subtitle "Pre-graduation review · Paed Annual Review Panel ·
  Training year 4", and its sitting bar Scheduled, "12 EPAs on the agenda" and "Summative · evidence `J−3y` to
  `<Y>`-12-31" (ISO dates). Mr Smit is offered Start review… in the bar, but leaves it to the chair.

Step 5.29 — Neither ended programme can be put before the panel
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews
Do: Press Schedule a review, choose the Paed Annual Review Panel and read the Trainee list. Then Cancel.
Expect: The list offers only Dr Dlamini, Dr Mahlangu and Dr Ndlovu, each with the training year. A graduate and a
  registrar who has withdrawn are not current trainees (T238). Cancel returns to Committee reviews; nothing is
  scheduled.

Step A.1.10 — Mr Smit schedules a formative check-in for Dr Ndlovu
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int} → /committee/decisions-due
Do: Press Schedule a review and schedule a review of Dr Ndlovu before the Paed Annual Review Panel for the period
  holding `D`, Scheduled on `D`, with Review type Formative check-in. Note the review's id from its address. Then open Decisions
  due, set Trainee to Dr Ndlovu and press Show.
Expect: The review opens as at Step 4.49: the subtitle "Formative check-in · Paed Annual Review Panel · Training year
  1", and the sitting bar Scheduled with "Formative: an interim check-in, no binding decision · evidence …" (T361). It
  is open, so the erasure at A.1.12 has a review to end. Decisions due's line reads "Formative check-in #N is open: it
  takes no seat." with Open check-in #N, as at Step 4.49.

Step A.7.5 — Mr Smit on his phone
Role: Coordinator — Mr Pieter Smit
Route: / → /msf/campaigns → /msf/campaigns/{CampaignId:int} → /committee/decisions-due → /admin/data-rights
Do: At 390 px, open his dashboard, Dr Molefe's released campaign, Decisions due and the data-rights queue. On Decisions
  due, set Status to "Every status", press Show, and open By EPA.
Expect: Home's header action, Start an MSF campaign, is a row of its own below the header's rule, 44px tall and the page's
  width, and the cards stack in one column, each row's link a 44px block (T358, R2-Home k6). MSF campaigns stacks its rows
  (T323): Pieter du Plessis's first, then Dr Molefe's, each a block with every field shown under its column's name, each
  action a 44px block; nothing scrolls sideways. The campaign page keeps its gutter (T226), and its invitee table (counts by
  respondent group: the page never lists an address) fits its card. Decisions due's filters stack, with Show and Clear
  filters each 44px tall; By EPA's fold summary is 44px, and opened it reads one block per EPA; each decision is a block, its
  one action a 44px button the block's width; the pager's buttons are 44px (T364). The data-rights queue (T387, R3-Q13):
  Type and Status stack, each select 44px tall, with Show 44px under them; no Clear filters at the defaults and no
  Apply filters in the header (T328); "Requests (5)" over the five KGK rows, stacked (the queue stacks from 1100px),
  each a block with every field under its column's name and its Review 44px; no pager; nothing scrolls sideways.

Step A.7.8 — Dr Sithole on his phone
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: / → /committee/reviews → /committee/decisions-due
Do: At 390 px, open his dashboard, then Committee reviews and Decisions due from the menu.
Expect: As for Dr Mokoena, scoped to Dr Sithole's sub-speciality: Home stacks the same three cards. The folded bar reads
  "Acting as" over "Sub-speciality admin", in two lines: the bar grows, and the role is never cut. Dr Sithole's menu
  reads as Dr Mokoena's (A.5.12), each row 44px. Committee reviews: Schedule a review is a 44px row of its own under the
  header, and each review a block (the trainee, then Panel, Period over its window, Type, State, Decision, and Open
  44px). Decisions due reads as at Step A.7.5.
```
