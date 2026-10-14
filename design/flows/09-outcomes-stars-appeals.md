# F09 What the committee issued: STARs, the register, a revocation and an appeal

**Who and why:** a committee's decision becomes two things a registrar keeps: her STARs (Statements of Awarded
Responsibility, each the committee's entrustment of an EPA at a level, with a certificate that leaves Wombat) and the
decision on her review, which she may appeal once. The panel's appeal body resolves the appeal, and that cannot be
undone. Programme staff keep the STAR register and revoke a STAR issued in error. Four pages carry it today: My
authorisations, My committee reviews, the Appeals section of the committee's review page, and Entrustment decisions.
My authorisations and the register are untouched since 2026-09-26 except their titles (flow 01); flow 07 rebuilt the
review page around them (the decision history, the Appeals section last, results beside controls) and changed three
lines of My committee reviews (D53's Not decided, the formative label).

Restated 2026-10-14 for T366 (the brief of 2026-09-26 predates flows 01–08). Paths to screenshots are under
`design/baseline/` (gitignored). Code is cited at `adfa3700`; `W/` is `src/Wombat.Web/Components/Pages/`, `CD/` is
`W/CommitteeDecisions/`, `A/` is `src/Wombat.Application/Features/`, `D/` is `src/Wombat.Domain/`, `Nav/` is
`src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **F/W** in BRIEF.md § 8: My authorisations and the STAR register are expected to keep their shape (F), the trainee's review view (its decision history, Conditions and Appeals block, T308, unbuilt) and the committee's Appeals section may change a lot (W). |
| Mode | **Structure first** (BRIEF.md § 2.3, as § 11 corrected it). Round 1: 2–3 structural variations of the appeal journey (the trainee's review view: where it lives, its decision history, her Appeals block and the lodge; the committee's Appeals section: the member's note, the resolve form, its quorum and its result), and ONE proposal for My authorisations (with "No longer in force", T319) and the register (with its revoke confirmation) to confirm or correct. Round 2, after the pick: fidelity with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768 (§ 11, flow 06) |
| Variations | 2–3 for the appeal journey in round 1, one for My authorisations and the register; the chosen one in round 2 |
| People | Trainees: Dr Lerato Molefe (training year 4; three STARs, later fifteen), Dr Anele Dlamini (3; one STAR revoked), Dr Nomsa Mahlangu (1; appeals Inadequate Progress), Dr Sipho Ndlovu (1; an annual review and a formative check-in). CommitteeMembers: Dr Thandi Zulu (chair), Dr David Naidoo (member), Dr Sarah Botha (member, present for the remit), Dr John van Rensburg (external). SpecialityAdmin: Dr Refilwe Mokoena (revokes). SubSpecialityAdmin: Dr Kabelo Sithole. InstitutionalAdmin: Prof Nolwazi Mbatha (`README.md` § Cast) |
| Runbook steps | 17, pasted verbatim in § 8: 4.35–4.37, 4.39, 4.41–4.48 and 4.51 (Act 4), 5.7 and 5.8 (Act 5), 6.22 (Act 6), A.7.7 (appendix) |
| Pages (`coverage.md` templates) | `/portfolio/authorisations` (`W/Portfolio/MyAuthorisations.razor`, 136 lines), `/committee/my-reviews` (`CD/MyReviews.razor`, 249: the list and, below it, the detail of the review chosen with View; the detail has no route of its own), `/committee/reviews/{ReviewId:int}` (`CD/ReviewDetail.razor`, 1594: its Appeals section, `:1367–1461`, and the decision history in `CD/Review/DecisionForm.razor:142–185`; the rest is flow 07's), `/admin/entrustment-decisions` (`W/Admin/EntrustmentDecisions/Index.razor`, 225), `/committee/decisions-due` (flow 08's; this flow only lands on its "Revoked: re-decide" line), `/access-denied` (flow 01's). Arrived at from `/` (the Homes' cards) and `/committee/reviews` (flow 08's list). Checked at `adfa3700`. |
| Menus (`Nav/NavItems.cs`) | My committee reviews is in the Trainee's flat menu (`:94`, `:179`). My authorisations has no item: it lights My progress, under the trail Home › My progress › My authorisations (`Nav/NavOwners.cs:159`), and is reached from the Trainee Home's My authorisations card (`W/Dashboards/TraineeDashboard.razor:161–193`) and an EPA's page (`W/Portfolio/EpaProgress.razor:146`). Entrustment decisions is in the Speciality and SubSpecialityAdmin's menus (`:165–168`, flow 06), lit with no trail; not in the Administrator's or the InstitutionalAdmin's, who reach it by the Home's quick link (`W/Dashboards/InstitutionalAdminDashboard.razor:53`); the Coordinator and a CommitteeMember are refused it. A review's page is owned by Committee reviews (`Nav/NavOwners.cs:106–114`); it does not admit a Trainee. |
| Components drawn at runtime | `PageHeader` (no trail on these pages except My authorisations' and the review page's), `StatePanel` (only the review page passes `OnRetry`), `Skeleton`, `DataTable` (My committee reviews only: no caption, no `Stack`, no `scope`), raw `table.clinic-table` (the register; the trainee's agenda), `section.detail-card` (a STAR's card; the trainee's detail; the inline revoke section), `dl.details-list` (a STAR's card), `FormField`, `FormActions`, `FieldHelp`, `InFlight` and `ActionResult` (the review page only), `Alert` (every result and refusal on the other three pages), `BadgeFor` (Expires in, the agenda's outcome, a STAR's status), `EpaLabel` ("(no longer in use)", T255), `RowNames`/`ReviewRowNames`. Not used: `ConfirmDialog` (no revoke, lodge or resolve confirmation), `PagerControls` (the register is unpaged). Queries: `GetActiveDecisionsForTrainee`, `ListReviewsForTrainee`, `GetCommitteeReviewById`, `ListEntrustmentDecisionsForAdmin`; commands `LodgeAppeal`, `ResolveAppeal`, `RevokeEntrustmentDecision`, `DownloadEntrustmentCertificate` (all `A/`) |
| Held | None. Every step capture is from the T364 replay (2026-10-13), on the shell as built; the `states/my-reviews--*` and `states/review-detail--*` captures from 2026-10-12. The `states/my-authorisations--*` (except `--loading`) and every `states/entrustment-decisions--*` capture are from 2026-09-26 on the old frame; the step captures stand in for them (§ 3). |
| Depends on | Flow 01, built (`b347e11c`): the shell, the trail, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again, access denied and not found. Flow 04 (`06aa51d7`): row actions named per row. Flow 05 (`b020c942`): ISO dates (D1), stacked rows below 641 px, My authorisations under My progress. Flow 06 (`85d5a508`): Entrustment decisions in the speciality admins' menus. Flow 07 (`44de180b`): the review page, its decision history, results beside controls, D53's Not decided, the formative label. Flow 08 (`622c5ad1`): Decisions due's "Revoked: re-decide" line with one action, Schedule. The design system is **version 20**, re-synced to flow 08 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `pwsh design/tools/stage_upload.ps1 -Flow 09`. Open each screenshot. Leave out anything in
   `design/upload/crop-first/` (none of this flow's captures shows a link or token; checked 2026-10-14). The script
   copies whole files and cannot crop, so crop the staged copies under `design/upload/baseline/` (never the baseline)
   to the Appeals section before sending: `act-4/4.44-1-naidoo-appeal-body-note.png` and
   `states/review-detail--appeal-dismissed.png` keep their bottom 300 px; `act-4/4.45-3-vanrensburg-remit-form-full.png`
   its bottom 720 px; `act-4/4.47-1-zulu-appeal-remitted.png` becomes two files, its top 1,000 px (the Closed bar and
   the decision history) and its bottom 300 px (the Appeals section). The trainee's pages (2,500–3,500 px) go whole.
2. **Start a new canvas** from the main app's Design page, on the Wombat design system v20
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier flow's canvas. Flow 09's canvas, "Wombat 09":
   its link goes here once it is created.
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, `asset_ids: ["16c4e619b7ea0971d0c28ed6509be7a8"]`). Put the returned `/_blob/` URL in § 1 and in
   `design/flows/09-outcomes-stars-appeals/round-1-ask.txt`, where they read `/_blob/MARK`.
4. **Send § 1 and § 8 as ONE message** (`design/flows/09-outcomes-stars-appeals/round-1-ask.txt`), with the key
   screenshots in § 4. Read `project/canvas.json`'s version before and after: a send can draw nothing. Read round 1's
   notes for a line saying it never received the attachments (§ 11, flow 06); re-attach and name the folder before
   round 2 if so.
5. **Pick one structure,** say why in a sentence, and answer § 6's questions one sentence each; the operator's are
   marked. A question whose answer rests on an open task names it, and the task lands before the build (§ 11, flow 05).
   Ask for round 2.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/07-review-sitting/round-2-review.md`
   did: one reviewer on the code (and the ask), one on the cast and the runbook, one on the design system. Put the
   decisions to the operator, then ask for one correction round.
7. **Record every round** in `design/flows/09-outcomes-stars-appeals/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 09 — What the committee issued: STARs, the register, a revocation and an appeal

This is flow 09 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting) and 08 (panels, what is due, the schedule) are designed and
  built; these pages sit inside the shell. Structure first, then fidelity: round 1 is 2–3 structural variations of the
  appeal journey (the registrar's review, her appeal, the committee's Appeals section) as wireframes, plus ONE proposal
  for My authorisations and the STAR register for me to confirm or correct. I pick before round 2. The runbook steps
  follow below in this same message. The brand mark is at /_blob/MARK: use it, never a drawn disc.

GOAL: A registrar must be able to:
    - read the STARs in force (each the committee's entrustment of an EPA at a level), download each certificate, and
      see when one expires;
    - see a STAR that is no longer in force (revoked or expired), when, and why, and still download its certificate;
    - read each committee review of hers: what was decided, its conditions, who was present, what the agenda decided
      EPA by EPA, and every earlier decision the appeal replaced;
    - appeal a ratified review, knowing who hears it and that the outcome is final, and then follow her appeal to its
      outcome.
  The panel's appeal body (the chair and the external members) must be able to resolve an appeal, Dismissed or
  Remitted, with a quorum for a replacement decision, knowing it cannot be undone; every other member reads who may.
  Programme staff must be able to read the STAR register, filter it, download a certificate, and revoke a STAR issued
  in error with a reason, knowing what the registrar is told.
  What is wrong today (the attached screenshots show each; all on the shell as built, 2026-10-13):
  - The registrar never sees her appeal, the decision it replaced, or any decision's Conditions: her review shows only
    the current decision (4.48). After she lodges, "Appeal lodged." shows at the page top and the detail reads Under
    appeal, but the list row above still reads Ratified (4.43).
  - Her review opens below the list with View: no address of its own, no focus move, no loading state. At 390 the
    list scrolls sideways in its box and cuts its words (4.41). Typing the committee's address for her review lands on
    "You cannot open this page", which does not point to My committee reviews.
  - The appeal form is one box, "Appeal reason", and Lodge appeal: it says nothing of who hears the appeal, that either
    outcome is final, or that the reason is read by every committee reader; there is no confirmation. Left blank it
    says "The Reason field is required.".
  - A revoked STAR simply vanishes from My authorisations (4.42). The register's revoke confirmation promises "The
    trainee is notified." and nothing is sent.
  - The revoke confirmation is not a dialog: Revoke, a red button in the row, opens a section below the table, out of
    view on a long list; Confirm revocation enables only when the reason box loses focus; the reason box is monospace;
    the result and any refusal show at the page top, unfocused (4.36).
  - The register names no revocation in its row (no date, no one, no reason), links no row to the review that issued
    it, prints statuses as code words ("Active", "Superseded"), says "No entrustment decisions" for both nothing issued
    and nothing matching, and at 390 scrolls inside its box (A.7.7). The revoked certificate names its reviser by a
    user id (4.37).
  - On the Appeals section the chosen Outcome is cut short once Remitted fills the row ("Remitted: the appeal body
    replaces the" at 1280, 4.46). The quorum refusal shows at the section's head, above a six-field form, far from
    Resolve appeal, and the Present ticks feed no live count as the sitting's Record does. Resolve appeal, which
    nothing reopens, has no confirmation. The appeal list reads "{date} {reason} (Open)", with no resolved date and no
    resolver.
  - My authorisations prints dates long-form ("2 July 2026") where every other page is ISO; "Expires in N days" counts
    on the UTC date; a failed read or download prints the exception's text.

AUDIENCE:
  - Trainees: Dr Lerato Molefe (training year 4; three STARs after Act 4, PAED-010 expiring in 20 days; fifteen at
    exit level after her pre-graduation review), Dr Anele Dlamini (3; PAED-002 revoked), Dr Nomsa Mahlangu (1;
    appeals "Inadequate Progress — Additional Training"; remitted to "Satisfactory with Observations"), Dr Sipho
    Ndlovu (1; an annual review, Outcome Deferred, and a formative check-in).
  - CommitteeMembers: Dr Thandi Zulu (chair), Dr David Naidoo, Dr Sarah Botha (each also an Assessor), Dr John van
    Rensburg (external, CommitteeMember only). The appeal body is the chair and the external member.
  - SpecialityAdmin: Dr Refilwe Mokoena, who revokes; SubSpecialityAdmin: Dr Kabelo Sithole; InstitutionalAdmin: Prof
    Nolwazi Mbatha. They read the register.
  - Kgosi Kgari Teaching Hospital (KGK), Paediatrics, on the College's (CPSA) curriculum 11.1, fifteen EPAs (PAED-001
    to PAED-015). Frequency: STARs after each sitting (a review each semester or year); appeals are rare. Stakes:
    high. Resolving an appeal cannot be undone; the certificate is the medico-legal record and leaves Wombat. Desktop
    1280×800 and phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - My authorisations (the Trainee only): "Your active Statements of Awarded Responsibility (STAR)."; one card per
    Active STAR, by EPA code: the EPA ("PAED-012 — Communicating with … (no longer in use)" when the College has
    paused it: a pause leaves a STAR standing), Authorised level ("5", "3b"), Issued, Expires ("No expiry", or the
    date with "Expires in 20 days" when 30 days or fewer remain), the committee's rationale, and Download certificate
    ("Download certificate for PAED-001"; "Generating…" while it runs). Empty: "No active authorisations yet" / "When
    a committee review ratifies a decision authorising you to perform an EPA, it will appear here. You can then
    download a STAR certificate for your records." A revoked, expired or superseded STAR is not listed. A graduate
    loses the page with the Trainee role (flow 13's).
  - A STAR's status: Active, Expired (past its expiry, by a nightly job), Revoked (by programme staff, with a reason),
    Superseded (a later STAR for the same EPA was ratified). Only an Active STAR can be revoked, expire or be
    superseded. Its certificate can be downloaded in any status, by the trainee and by staff who oversee her; a revoked
    one prints "REVOKED", the date, the reviser and the reason.
  - My committee reviews (the Trainee; "Ratified decisions, appeal status, and final outcomes."): her reviews once
    ratified, newest first: Panel, Period ("2026 S2 · 2026-01-01 to 2026-12-31"), Type ("Annual progression",
    "Pre-graduation", "Entrustment only", "Formative check-in"), State (Ratified, Under appeal, Closed), Decision (a
    category: "Satisfactory Progress", "Satisfactory with Observations", "Inadequate Progress — Additional Training",
    "Inadequate Progress — Repeat", "Release from Training", "Outcome Deferred", "Graduate (programme complete)"; or
    "No binding decision" for a check-in; "Entrustment decisions only"), and View ("View the 2026 S2 review before
    Paed Annual Review Panel"). Empty: "No decisions yet" / "Ratified committee outcomes will appear here."
  - Her review's detail: Panel, Sits for, Evidence window, Type, State; the current decision (its category, its
    rationale, "Present when this decision was taken: Thandi Zulu (chair), Sarah Botha, John van Rensburg
    (external)"); for an entrustment-only review "This review decided entrustment only, so it records no progression
    outcome." (and, once remitted, "The appeal was remitted: this decision is the appeal body's, and the entrustment
    decisions this review issued stand unless one is revoked."); the Agenda, "The EPAs this review was there to decide
    for 2026 S2, and what the committee did with each.": EPA, Window, Outcome, where Decided reads "STAR #1.", Not
    decided at this sitting reads the decision's one sentence about the EPAs not decided ("To be decided at her final
    review."), and Decided elsewhere "Decided at another sitting in this window." (D53: no deferral).
  - Appeals, lodging: only the trainee herself (the Trainee role), only against a review that is Ratified, one open
    appeal at a time, and a review that is Closed cannot be appealed again. There is no deadline and no window: a
    ratified review can be appealed at any time until it is closed. A pre-graduation or entrustment-only review can be
    appealed too. The reason is required, up to 4,000 characters, and every reader of the committee's page reads it.
    Refusals: "Only ratified reviews can be appealed."; "An unresolved appeal already exists for this review."; "The
    committee review could not be found among your own ratified reviews.". Lodged: "Appeal lodged.", the review Under
    appeal.
  - Appeals, resolving: only the panel's chair or an external member who may sit now (an active committee member at
    the panel's institution, not a trainee, never the trainee under review), and never an Administrator. Everyone else
    reads "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu, and its external member, John
    van Rensburg." (or, when none can act, "… and none of them can act now. A panel administrator must seat a chair
    who can."). Outcome: "Select an outcome…", "Dismissed: the decision stands", "Remitted: the appeal body replaces
    the decision" (no Upheld, D51), with "Dismissed leaves the committee's decision in force. Remitted replaces it with
    a decision the appeal body takes now, recorded below with who sat for it. Either closes the review, and nothing
    reopens it."; none chosen: "Choose an outcome.". Remitted asks Replacement category ("Select a category…"; none on
    an entrustment-only review, which says "This review decides entrustment only, so the replacement decision records
    no progression category, and it changes no entrustment decision: the STARs this review issued stand. To change
    one, revoke it and re-decide the EPA at a new review."), Replacement rationale, Replacement conditions (optional),
    and Present ("Tick every panel member at this sitting. A committee decision needs at least two panel members
    present: the chair and at least one other."; the chair and the resolver ticked and locked). A quorum is two, the
    chair among them; short of it: "A committee decision needs at least two panel members present: the chair and at
    least one other.". A panel that cannot seat a quorum at all shows Resolve appeal unavailable, with its reason.
    Resolved: "Appeal resolved.", the review Closed. Remitted adds the replacement above the first decision, which
    reads "Replaced on appeal by the decision above."; the replacement keeps the first decision's sentence about the
    EPAs not decided, and changes no STAR. Dismissed leaves the decision in force. Either outcome is final.
  - The committee's decision history (flow 07's, on the review page): every decision, newest first, each with its
    category, "Replaced on appeal by the decision above." where so, its rationale, "About the EPAs not decided: …",
    "Conditions: …", and "Present: Thandi Zulu (chair), Sarah Botha". The appeals list under it: "2026-10-13 {reason}
    (Open)", "(Dismissed)" or "(Remitted)"; "No appeal has been lodged against this review." when none.
  - The STAR register (Administrator, InstitutionalAdmin, SpecialityAdmin, SubSpecialityAdmin; "Issued Statements of
    Awarded Responsibility across the programme."): the trainees each oversees, newest issued first, unpaged. Filters
    Trainee ("Filter by name") and Status (All, Active, Expired, Revoked, Superseded), applied with Apply filters.
    Columns Trainee, EPA, Level, Issued, Expires (ISO, or "—"), Status (a badge), and the actions Download ("Download
    the certificate of Lerato Molefe's PAED-010 decision, issued 2026-10-13") and, on an Active row only, Revoke.
    Empty or filtered to nothing: "No entrustment decisions" / "No entrustment decisions match the current filter.".
    A STAR out of scope is not listed; a refusal reads "The entrustment decision could not be found among the
    decisions you may revoke.".
  - Revoking: "You are about to revoke the entrustment decision for PAED-002 — Managing common paediatric
    presentations held by Anele Dlamini. Revocation is immediate and irreversible. The trainee is notified."; Revocation
    reason (required, up to 1,000 characters); Confirm revocation and Cancel. Done: "Entrustment decision for PAED-002
    revoked."; the row reads Revoked with Download only. Nothing else changes: the EPA reads "Revoked: re-decide" on
    Decisions due ("STAR #5, issued for 2026 S2, was revoked. Schedule a review to decide it again.", flow 08's page),
    and the trainee's agenda still reads it Decided with its STAR number, because it records what the sitting did.
  - Access denied (flow 01's): a Trainee on the committee's review page reads "You cannot open this page" / "Your role
    (Trainee) does not open this page." with Go to Home. The Coordinator is refused the register.
  - Data volumes: none (no STARs, no reviews); typical (three STARs and one review); heavy (fifteen STARs plus a
    superseded one; fifteen agenda lines; an appealed review with two decisions; two reviews in one period, a binding
    one and a check-in; a register of twenty rows for five registrars).

WHAT FLOW 09 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01: the trail, the acting role, access denied, not found); the menus' items (flow 06
    settled them; propose a change, do not draw it as settled); flow 07's review page above its Appeals section, except
    the decision history where it is shared; Decisions due (flow 08's); the rules above (who may lodge and resolve, the
    quorum of two, the two outcomes, that either is final, that a remit changes no STAR, which STARs can be revoked).
  May, for these pages: their whole structure (the trainee's review in place, on a page of its own, or on a route of
    its own; the decision history as one component shared by both sides; an Appeals block for the trainee; where the
    lodge and resolve forms live; "No longer in force" on My authorisations; the revoke as a dialog), the order and
    grouping of parts, titles and subtitles, where each result and refusal shows and where focus goes, confirmations,
    the lists' stacking at 390, and the words. Mark PROPOSED any rule change (an appeal deadline, hiding the reason
    from some readers, a STAR status shown to the trainee, a mail sent): those are the operator's. The runbook's
    Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order:
  1. My authorisations: none yet; three STARs, one expiring (4.39); fifteen at exit level (5.8); after a revocation,
     with NEW "No longer in force" (4.42); a paused EPA's STAR; a download refused; loading; a load error.
  2. My committee reviews, the list and her review: none yet; one review (4.41); the appeal form, before lodging
     (4.43); under appeal, the row and the review agreeing, with NEW her Appeals block (4.43); closed after a remit,
     with NEW the decision history and the appeal's outcome (4.48); closed after a dismissal; an annual review and a
     check-in in one period (4.51); pre-graduation, every EPA Decided (5.8); an entrustment-only review; loading.
  3. The committee's Appeals section (on the review page): a member's view (4.44); the appeal body's resolve form
     (4.45); no outcome chosen; Remitted on a progression review and on an entrustment-only one; a remit short of a
     quorum (4.46); a panel that cannot seat a quorum; closed after a remit, the replacement heading the history
     (4.47); closed after a dismissal; no appeal lodged.
  4. The STAR register: none issued; issued (4.35); filtered to one registrar (4.35, 5.7); filtered to nothing; the
     revoke confirmation (4.36); revoked (4.36, 4.37); superseded (5.7); a paused EPA (6.22); a revoke refused;
     loading; a load error.
  5. 390 px for every page (A.7.7), and access denied (a trainee on the committee's address, 4.41).

STEPS: 4.35–4.37, 4.39, 4.41–4.48, 4.51, 5.7, 5.8, 6.22 and A.7.7. Pasted verbatim below (Role / Route / Do /
  Expect). In them, D is the day the story is replayed and <P> the semester holding D (for example 2026 S2) and <Y> its
  year. Where an Expect quotes today's wording or layout, that is today's page, not a requirement on the new one; two
  Expects describe what is not built: 4.36's dialog and mail, and 4.37's reviser by name.

STATES TO SHOW:
  Every state named in SCREENS; loading for every page (the header and a skeleton, no action offered until the read is
  in); a load error (a sentence and Try again, never an exception's text); each confirmation the design adds, its
  cancel, and the result after it with focus on it; 390 px for every page; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - A5 (BRIEF.md § 6): every action reports its outcome where the focus goes and where the person is working; a refusal
    shows beside the control that caused it and keeps what was typed; a destructive action confirms, names its target,
    and its trigger is an outline button.
  - T308: after a lodge, her row reads Under appeal at once; she sees every decision (newest first, "Replaced on appeal
    by the decision above.", rationale, Conditions, who was present) from one component shared with the committee's
    page; she sees her appeals (date lodged, reason, outcome in words, date resolved).
  - T319: My authorisations gains "No longer in force" (revoked and expired, each with its status, its date and its
    certificate; superseded stays off, T319's recommendation). The revoke confirmation says only what is true.
  - T264 and T260: Revoke is an outline button named for its row and opens a ConfirmDialog naming the trainee and the
    EPA; Confirm revocation enables as the reason is typed; the result takes the focus.
  - T324: a revocation names who revoked by name; no message prints a property name ("The Reason field is required.").
  - T325: "Expires in N days" counts on the South African calendar; any time is SAST, with the zone.
  - T328: the reason boxes use the body font. T323: nothing at 1280 is cut (the Outcome); nothing at 390 scrolls
    sideways (the trainee's list, the register).
  - T329, T272: a failed read is an alert with Try again; no exception's or framework's text on any page.
  - T280: every table has a caption; headings in order (one h1, then h2).
  - Content (BRIEF.md § 6): sentence case; people by name as stored; states, outcomes and categories by their labels;
    ISO dates (flow 05's D1), My authorisations included.
  - Done since the old brief, keep them: Outcome opening on "Select an outcome…", no Upheld (T307, D51); the refusal in
    the Appeals section with the focus (flow 07); the formative label (T309 item 2); D53's Not decided; Entrustment
    decisions in the speciality admins' menus (flow 06).

QUESTIONS THE DESIGN MUST ANSWER:
  1. Where does the trainee read one review: below the list as today, on a page of its own (a PROPOSED route), or as
     the list's opened row? Where does a trainee who types the committee's address end up?
  2. One decision history for both sides: what it shows the trainee that it shows the committee, and what not.
  3. The trainee's Appeals block, and the committee's appeal list: what each entry shows (lodged, reason, outcome,
     resolved, by whom), and where her appeal form sits.
  4. Lodging: what the form tells her (who hears it, that the outcome is final, who reads the reason), and whether a
     confirmation follows.
  5. Resolving: a confirmation for an act nothing undoes; a live quorum count as the Present ticks change; where the
     result and a refusal show.
  6. Certificates: on My authorisations only, or also on the review's Decided lines?
  7. (The operator's) The revoke says "The trainee is notified." and nothing is sent. Send the mail (T319's
     recommendation: no reason in it, a link to My authorisations), or drop the sentence?
  8. (The operator's) "No longer in force": revoked and expired only, superseded off (T319's recommendation)? Does
     the trainee read the revocation's reason on the page, or only on the certificate?
  9. (The operator's) There is no appeal deadline, and every committee reader reads the appellant's reason. Keep both?
  10. (The operator's) My authorisations has no menu item (it lights My progress). Keep, or give it one?
  11. (The operator's) T308, T319, T260, T264, T324, T325, T328, T329 and T272 touch these pages: fold them into the
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
  AgendaLineForm, EvidenceReader, DecisionForm, ReviewReturnLink. Since flow 08: one action a row, the seat
  picker, a choice of a few kinds as radios (choice-group), the trainee line, a form page of its own (Schedule a
  review).
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
  ConfirmDialog. Flow 07's review page is redrawn only in its Appeals section and its decision history.

ASK:
  - Round 1: 2–3 STRUCTURAL variations of the appeal journey as wireframes (screens 2–3): where her review lives, the
    shared decision history, her Appeals block and the lodge, the committee's Appeals section with its quorum and
    result, and both at 390. Give each its reasoning and its step count per journey, at 1280 and 390 px: Dr Molefe
    reading her review (4.41); Dr Mahlangu lodging (4.43) and reading the outcome (4.48); Dr Naidoo reading the appeal
    (4.44); Dr van Rensburg and Dr Zulu resolving, refused and then remitting (4.45–4.47). Plus ONE proposal for
    screens 1 and 4 (Dr Molefe's and Dr Dlamini's My authorisations, 4.39 and 4.42; Dr Mokoena's revocation, 4.36;
    Prof Mbatha's register, 4.35 and 5.7), wireframed, for me to confirm or correct. No colour or type choices yet.
    Stop and wait for my pick.
  - Round 2: the chosen structure at full fidelity on the design system's tokens, every screen and state above, at
    1280 and 390 px, each role's view, every confirmation, and where the focus lands after each action.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ Table system; § Form system; § Page-level patterns: List page, Detail page, and the title rule; the
    owner table's rows for My authorisations and a review; § Entrustment-only reviews, "The trainee's reviews").
  - Mark PROPOSED anything not built: a confirmation, a new route, a mail, a rule change.
  - Flag edge cases: an entrustment-only review appealed and remitted (no category; no STAR changes); a panel that
    cannot seat a quorum; an appeal body member who cannot act now; a STAR revoked on a paused EPA; a superseded STAR
    beside its successor; a graduate (the page goes with the Trainee role: flow 13's); the 120-character EPA title
    ("PAED-015 — Teaching and applying evidence-based care responsibly and ethically in clinical decision-making and
    research"); a 4,000-character appeal reason.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, badges read as words, focus after every action and confirmation; targets ≥ 24 px (44 at
    390).

ATTACHED: act-4/4.39-1-molefe-my-authorisations.png, act-4/4.41-1-molefe-my-review.png,
  act-4/4.43-2-mahlangu-appeal-lodged.png, act-4/4.48-1-mahlangu-remitted-outcome.png,
  act-4/4.42-1-dlamini-authorisations-after-revocation.png, act-4/4.36-1-mokoena-revoke-reason-typed.png,
  act-5/5.7-2-mbatha-molefe-all-superseded.png, act-4/4.46-1-zulu-remit-quorum-refused.png, and
  act-4/4.47-1-zulu-appeal-remitted.png as two crops (the decision history; the Appeals section). The trainee's pages
  are 2,500–3,500 px, which you will see scaled. In every tall capture at 1280 the sidebar's colour stops at 800 px:
  that is the capture, not the page.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| 4.35 | `/` → `/admin/entrustment-decisions` | Prof Mbatha | from her Home's quick link (no menu item): five Active rows issued `D`; the PAED-010 certificate; "Molefe" leaves three rows |
| 4.36 | `/admin/entrustment-decisions` | Dr Mokoena | from her menu, lit: Revoke on Dr Dlamini's PAED-002 opens a section below the table (not a dialog), "The trainee is notified." (false: nothing is sent, F-4.36a); Confirm enables only on blur (T260); "Entrustment decision for PAED-002 revoked.", the row Revoked with Download only |
| 4.37 | `/admin/entrustment-decisions` → `/committee/decisions-due` | Dr Sithole | Revoked leaves one row, Active four; the revoked certificate names the reviser by user id (F-4.37a, T324); Decisions due's "Revoked: re-decide" with Schedule (flow 08's) |
| 4.39 | `/` → `/portfolio/authorisations` | Dr Molefe | from her Home's card: three cards, PAED-010 "Expires in 20 days"; dates long-form; each Download named by its EPA |
| 4.41 | `/committee/my-reviews` → `/committee/reviews/{ReviewId:int}` → `/access-denied` | Dr Molefe | the list; View opens the detail below it (no route); the decision with who was present; the agenda, Decided "STAR #n." or Not decided with the sentence (D53); the committee's address is access denied |
| 4.42 | `/` → `/portfolio/authorisations` → `/committee/my-reviews` | Dr Dlamini | one card: PAED-002 has gone without a trace (T319); her agenda still reads PAED-002 Decided "STAR #5." |
| 4.43 | `/committee/my-reviews` | Dr Mahlangu | the decision, who was present, fifteen Not decided lines; the Appeal reason form; "Appeal lodged." at the top, the detail Under appeal, the row still Ratified (F-4.43a, T308) |
| 4.44 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr Naidoo | at the page's foot, the Appeals section: who may resolve, the appeal "(Open)", no form |
| 4.45 | `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Dr van Rensburg | the resolve form: "Select an outcome…", "Choose an outcome." when none; Remitted's four fields, the chair and himself locked; the Outcome cut short (F-4.45c) |
| 4.46 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | the quorum refusal at the section's head, focused, the form keeping its values; no live count of the ticks |
| 4.47 | `/committee/reviews/{ReviewId:int}` | Dr Zulu | "Appeal resolved.", Closed; the replacement heads the history with its Conditions; the first "Replaced on appeal by the decision above."; "(Remitted)"; no STAR changed |
| 4.48 | `/committee/my-reviews` | Dr Mahlangu | Closed, Satisfactory with Observations, who was present; no trace of her appeal, its outcome, the replaced decision or the Conditions (F-4.48a, T308) |
| 4.51 | `/committee/my-reviews` | Dr Ndlovu | two rows for the period: Ratified, Outcome Deferred; Formative check-in, Closed, "No binding decision" |
| 5.7 | `/` → `/admin/entrustment-decisions` | Prof Mbatha | fifteen Active rows for Molefe; with All, Act 4's PAED-010 Superseded, Download only |
| 5.8 | `/committee/my-reviews` → `/` → `/portfolio/authorisations` | Dr Molefe | Pre-graduation, Ratified, Graduate (programme complete); every line Decided with its STAR; the appeal form offered; fifteen cards, "No expiry" |
| 6.22 | `/admin/epas` → `/admin/curricula` → `/admin/curricula/{Id:int}/items` → `/admin/entrustment-decisions` | Prof Mbatha | after the College pauses PAED-012: her fifteen still Active, PAED-012 "(no longer in use)" (T255) |
| A.7.7 | `/` → `/committee/panels` → `/committee/decisions-due` → `/admin/entrustment-decisions` | Dr Mokoena at 390 | the register lit in her menu; its table scrolls inside its box, Status and actions off-screen |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` § Portfolio and § Committee. The step
captures are from the T364 replay (2026-10-13) on the shell as built. The `states/my-reviews--*` and
`states/review-detail--*` captures are from 2026-10-12 on the shell as built; most are the same image as a step
capture. The `states/my-authorisations--*` captures (except `--loading`) and every `states/entrustment-decisions--*`
capture are from 2026-09-26 on the old frame (a flat Title Case sidebar, an email in the top bar): use the step
capture where there is one, and the old one for its body only.

| Page | State | Screenshot | Reached at |
|---|---|---|---|
| `/portfolio/authorisations` | None yet: "No active authorisations yet" | `states/my-authorisations--empty.png` (old frame) | Dr Mahlangu, Step 4.43 |
| `/portfolio/authorisations` | Three, one expiring | `act-4/4.39-1-molefe-my-authorisations.png` (`states/my-authorisations--expiring.png`, old) | Step 4.39 |
| `/portfolio/authorisations` | The Home card that leads here | `act-4/4.39-3-molefe-home-authorisations-card.png` | Step 4.39 |
| `/portfolio/authorisations` | After a revocation (design "No longer in force" here) | `act-4/4.42-1-dlamini-authorisations-after-revocation.png` (`states/my-authorisations--after-revocation.png`, old) | Step 4.42 |
| `/portfolio/authorisations` | Fifteen | `act-5/5.8-3-molefe-my-authorisations.png` (`states/my-authorisations--fifteen.png`, old) | Step 5.8 |
| `/portfolio/authorisations` | Loading | `states/my-authorisations--loading.png` | A held read |
| `/portfolio/authorisations` | 390 px | `act-4/4.39-2-molefe-my-authorisations-390.png` (`states/my-authorisations--narrow.png`, old) | Step 4.39 at 390 |
| `/committee/my-reviews` | None yet: "No decisions yet" | `states/my-reviews--empty.png` | Dr Dlamini, Step 3.50 |
| `/committee/my-reviews` | The list and a review | `act-4/4.41-1-molefe-my-review.png` (`states/my-reviews--detail.png`) | Step 4.41 |
| `/committee/my-reviews` | After a revocation, the agenda unchanged | `act-4/4.42-2-dlamini-review-after-revocation.png` | Step 4.42 |
| `/committee/my-reviews` | The appeal form | `act-4/4.43-1-mahlangu-review-before-appeal.png` (`states/my-reviews--appeal-form.png`) | Step 4.43, before lodging |
| `/committee/my-reviews` | Under appeal (the row stale: T308) | `act-4/4.43-2-mahlangu-appeal-lodged.png` (`states/my-reviews--appealed.png`) | Step 4.43 |
| `/committee/my-reviews` | Closed after a remit (no trace of the appeal: T308) | `act-4/4.48-1-mahlangu-remitted-outcome.png` (`states/my-reviews--remitted.png`) | Step 4.48 |
| `/committee/my-reviews` | An annual review and a check-in | `act-4/4.51-1-ndlovu-two-reviews.png` (`states/my-reviews--two.png` is stale: it reads "Formative") | Step 4.51 |
| `/committee/my-reviews` | Pre-graduation | `act-5/5.8-1-molefe-my-reviews.png`, `act-5/5.8-2-molefe-pre-graduation-detail.png` (`states/my-reviews--pre-graduation.png`) | Step 5.8 |
| `/committee/my-reviews` | Loading | `states/my-reviews--loading.png` | A held read |
| `/committee/my-reviews` | 390 px (the list scrolls sideways) | `act-4/4.41-2-molefe-my-review-390.png` (`states/my-reviews--narrow.png`) | Step 4.41 at 390 |
| `/committee/reviews/{ReviewId:int}` | Access denied, a trainee | `act-4/4.41-3-molefe-committee-page-denied.png` | Step 4.41 |
| `/committee/reviews/{ReviewId:int}` | Under appeal, a member | `act-4/4.44-1-naidoo-appeal-body-note.png` (`states/review-detail--appeal-member.png`; crop the foot) | Step 4.44 |
| `/committee/reviews/{ReviewId:int}` | The resolve form, Remitted chosen | `act-4/4.45-3-vanrensburg-remit-form-full.png` (`states/review-detail--appeal-form.png`; crop the foot) | Step 4.45 |
| `/committee/reviews/{ReviewId:int}` | A remit short of a quorum | `act-4/4.46-1-zulu-remit-quorum-refused.png` (`states/review-detail--remit-refused.png`) | Step 4.46 |
| `/committee/reviews/{ReviewId:int}` | Closed after a remit | `act-4/4.47-1-zulu-appeal-remitted.png` (`states/review-detail--remitted.png`; crop head and foot) | Step 4.47 |
| `/committee/reviews/{ReviewId:int}` | Closed after a dismissal | `states/review-detail--appeal-dismissed.png` (crop the foot) | Scratch (post-act 4): Dr Dlamini appeals, Dr Zulu dismisses |
| `/admin/entrustment-decisions` | None issued | `states/entrustment-decisions--empty.png` (old frame) | Prof Mbatha, Step 4.2 |
| `/admin/entrustment-decisions` | Issued, from the Home's quick link | `act-4/4.35-1-mbatha-home-quick-links.png`, `act-4/4.35-2-mbatha-entrustment-decisions.png` (`states/entrustment-decisions--issued.png`, old) | Step 4.35 |
| `/admin/entrustment-decisions` | Filtered to one registrar | `act-4/4.35-3-mbatha-stars-filtered-molefe.png`, `act-5/5.7-1-mbatha-molefe-active.png` (`states/entrustment-decisions--filtered.png`, old) | Steps 4.35, 5.7 |
| `/admin/entrustment-decisions` | The revoke confirmation | `act-4/4.36-1-mokoena-revoke-reason-typed.png` (`states/entrustment-decisions--revoke-form.png`, old and a broken stitch) | Step 4.36 |
| `/admin/entrustment-decisions` | Revoked | `act-4/4.36-2-mokoena-dlamini-paed002-revoked.png`, `act-4/4.37-1-sithole-filter-revoked.png` (`states/entrustment-decisions--revoked.png`, old) | Steps 4.36, 4.37 |
| `/admin/entrustment-decisions` | Superseded | `act-5/5.7-2-mbatha-molefe-all-superseded.png` (`states/entrustment-decisions--superseded.png`, old) | Step 5.7 |
| `/admin/entrustment-decisions` | A paused EPA | `act-6/6.22-3-molefe-stars-paused.png` (Prof Mbatha's register; `states/entrustment-decisions--paused-epa.png`, old) | Step 6.22 |
| `/admin/entrustment-decisions` | Loading | `states/entrustment-decisions--loading.png` (old frame) | A held read |
| `/admin/entrustment-decisions` | 390 px | `act-A/A.7.7-5-entrustment-decisions-390.png` (`states/entrustment-decisions--narrow.png`, old) | Step A.7.7 |
| `/committee/decisions-due` | Revoked, to decide again (flow 08's) | `act-4/4.37-2-sithole-revoked-re-decide.png` (`states/decisions-due--revoked.png`) | Step 4.37 |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- On the new shell: My authorisations none yet (Dr Mahlangu); the register empty and loading; the register at 768.
- Anywhere: "Choose an outcome." under the select (Step 4.45 presses it, but neither of its viewport captures reaches
  the form); the Appeals section at a viewport for a member (only a 7,800 px page); Remitted on an entrustment-only
  review; a panel that cannot seat a quorum; a load error on any of these pages; a download or a revoke refused; a
  review appealed again after closing (refused: the form is not offered).
- Not built: "No longer in force" (T319); the trainee's decision history and Appeals block (T308); a confirmation for
  revoke, lodge or resolve; draw them from § 1.

**Belongs to other flows:** flow 01: access denied. Flow 08: Decisions due (`act-4/4.37-2…`, `act-A/A.7.7-4…`). Flow
06: the speciality admin's Home (`act-A/A.7.7-1-home-390.png`). Flow 13: the graduate refused both trainee pages (Step
5.22). Flow 18: the revocation's audit entry (Step 4.38). Flow 20: the certificate PDF (T310, T324's PDF half).

## 4. Attach

Paths are relative to `design/baseline/`. Every key screenshot below was opened and checked on 2026-10-14; none shows a
link, a token or a password. Crop the four long review pages as § How to run step 1 says; the trainee's pages go whole
and the canvas sees them scaled. In every tall capture at 1280 the sidebar's colour stops at 800 px: an artefact of
the capture.

**Key screenshots (attach these first, with § 1):**
1. `act-4/4.39-1-molefe-my-authorisations.png`: three cards, "Expires in 20 days", long-form dates
2. `act-4/4.41-1-molefe-my-review.png`: the list, the detail below it, the agenda (2,643 px)
3. `act-4/4.43-2-mahlangu-appeal-lodged.png`: "Appeal lodged.", the detail Under appeal, the row still Ratified (T308)
4. `act-4/4.48-1-mahlangu-remitted-outcome.png`: the replacement only, no trace of the appeal (T308)
5. `act-4/4.42-1-dlamini-authorisations-after-revocation.png`: one card; the revoked STAR gone (T319)
6. `act-4/4.36-1-mokoena-revoke-reason-typed.png`: the inline revoke section below the table, "The trainee is notified."
7. `act-5/5.7-2-mbatha-molefe-all-superseded.png`: the register, Superseded with Download only
8. `act-4/4.46-1-zulu-remit-quorum-refused.png`: the Appeals section: the refusal at its head, the Outcome cut short
9. `act-4/4.47-1-zulu-appeal-remitted.png`: the decision history after a remit, and "(Remitted)" (two crops)

**My authorisations:** `act-4/4.39-3-molefe-home-authorisations-card.png`, `act-4/4.39-2-molefe-my-authorisations-390.png`,
`act-5/5.8-3-molefe-my-authorisations.png` (fifteen, 3,368 px), `states/my-authorisations--loading.png`.

**My committee reviews:** `act-4/4.43-1-mahlangu-review-before-appeal.png`,
`act-4/4.42-2-dlamini-review-after-revocation.png`, `act-4/4.51-1-ndlovu-two-reviews.png`,
`act-5/5.8-1-molefe-my-reviews.png`, `act-5/5.8-2-molefe-pre-graduation-detail.png`,
`act-4/4.41-2-molefe-my-review-390.png` (the list scrolling sideways), `act-4/4.41-3-molefe-committee-page-denied.png`,
`states/my-reviews--empty.png`, `states/my-reviews--loading.png`.

**The Appeals section (crop each to its foot):** `act-4/4.44-1-naidoo-appeal-body-note.png`,
`act-4/4.45-3-vanrensburg-remit-form-full.png`, `states/review-detail--appeal-dismissed.png`.

**The register:** `act-4/4.35-1-mbatha-home-quick-links.png`, `act-4/4.35-2-mbatha-entrustment-decisions.png`,
`act-4/4.35-3-mbatha-stars-filtered-molefe.png`, `act-4/4.36-2-mokoena-dlamini-paed002-revoked.png`,
`act-4/4.37-1-sithole-filter-revoked.png`, `act-5/5.7-1-mbatha-molefe-active.png`, `act-6/6.22-3-molefe-stars-paused.png`
(Prof Mbatha's register, despite its name), `act-A/A.7.7-5-entrustment-decisions-390.png`, and, optionally, flow 08's
`act-4/4.37-2-sithole-revoked-re-decide.png`.

That is 34 files.

**Do not attach:** `states/review-detail--appeal-upheld.png` (old frame; an outcome D51 removed); `act-A/A.7.7-1-home-390.png` (flow 06's);
`act-4/4.45-1-vanrensburg-choose-an-outcome.png` and `act-4/4.45-2-vanrensburg-remit-form.png` (both the page's top, no form, despite their names);
`act-4/4.46-2-zulu-remit-quorum-refused-full.png` (the 8,384 px page of what 4.46-1 shows);
`states/entrustment-decisions--revoke-form.png` (old frame, a broken stitch); `states/my-reviews--two.png` (a stale label);
every old-frame `states/my-authorisations--*` capture except `--loading`, and every `states/entrustment-decisions--*` capture.

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `adfa3700`.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T308** (P3, queued) | Item 1: `LodgeAppealAsync` replaces `_selectedReview` but never the row in `_reviews` (`CD/MyReviews.razor:188–215`), so the row stays Ratified. Item 2: the detail renders only `Decisions[0]` (`:65–86`): no Conditions, no earlier decision, no "Replaced on appeal", though the DTO carries them. The committee's history is the read branch of `CD/Review/DecisionForm.razor:142–185`, not a component of its own: sharing it means lifting it out (question 2). Item 3: no Appeals block; the DTO's `Appeals` (`A/CommitteeDecisions/CommitteeDecisionDtos.cs:155–185`) is never read. | `act-4/4.43-2…`, `act-4/4.48-1…`; Steps 4.43, 4.48 |
| **T319** (P2, queued) | The revoke section says "The trainee is notified." (`W/Admin/EntrustmentDecisions/Index.razor:106`); the handler sends nothing (`A/EntrustmentDecisions/RevokeEntrustmentDecision.cs:35–78`, no sender; Step 4.36's Actual: the mail count stayed 29). The runbook's 4.36 Expect asks for the mail. T319 recommends sending it (no reason in the mail, a link to My authorisations) and "No longer in force" for revoked and expired STARs; the query reads Active only (`A/EntrustmentDecisions/GetActiveDecisionsForTrainee.cs:49`). Question 7, question 8. | `act-4/4.36-1…`, `act-4/4.42-1…` |
| **T260**, **T264** (P3, queued) | Revoke is `btn btn-sm btn-danger` in the row (`Index.razor:86`) and opens an inline `section.detail-card` below the table (`:98–116`), with no focus move; Confirm revocation is disabled until the reason binds, on blur (`:112`); no `InFlight`, so a double press can send twice; the result and refusals are page-top Alerts (`:215`). | `act-4/4.36-1…`, `act-4/4.36-2…` |
| **T324** (P3, queued) | The certificate prints "Revoked on … by {RevokedByUserId}" (`src/Wombat.Infrastructure/Reporting/EntrustmentCertificatePdfService.cs:189`; Step 4.37's Actual, F-4.37a). The appeal form's blank reason prints DataAnnotations' "The Reason field is required." (`CD/MyReviews.razor:127–139`). The register's Status prints the enum's names. | Step 4.37 |
| **T325** (P3, queued) | "Expires in N days" counts from `DateTime.UtcNow` (`W/Portfolio/MyAuthorisations.razor:39`, `:132`). | `act-4/4.39-1…` |
| **T328** (P3, queued) | The revoke reason box renders monospace. | `act-4/4.36-1…` |
| **T323** (P3, queued; F-4.45c) | The Outcome `FormField` is not full-width (`CD/ReviewDetail.razor:1378`) in a `.form-grid` of `auto-fit, minmax(min(250px,100%),1fr)` (`wwwroot/app.css:564–568`): once Remitted adds its fields the select is a third of the card and its choice is cut. At 390 the trainee's list (`DataTable` with no `Stack`) and the register (a raw table) scroll sideways in their boxes. | `act-4/4.46-1…`, `act-4/4.41-2…`, `act-A/A.7.7-5…` |
| **T329**, **T272** (P2, P3, queued) | My authorisations' load error and download refusal print `exception.Message` (`MyAuthorisations.razor:92`, `:121`); no page here but the review page passes `OnRetry`. | — |
| **T280** (P3, queued) | My committee reviews' table has no caption and no row `scope`; the trainee's detail goes h1 → h3 → h4. (The review PDF leaving out a review under appeal is flow 20's.) | `act-4/4.41-1…` |
| **A5** (BRIEF § 6; no task) | The quorum refusal shows in the Appeals section with the focus (`CD/ReviewDetail.razor:1372`, flow 07; Step 4.46's Gap: F-4.46a no longer occurs), but at the section's head, above the whole Remitted form, not by Resolve appeal; the Present ticks feed no live count (the sitting's Record has `p.quorum-line`, `CD/Review/DecisionForm.razor:130`). Resolve appeal and Lodge appeal have no `ConfirmDialog` though resolving is final. "Appeal lodged." is a page-top Alert that takes no focus. | `act-4/4.46-1…` |
| **T310** (P3, queued) | The certificate's institution comes from whichever profile the database returns first. A PDF, flow 20's; this flow only links the download. | — |
| **T311** (P2, queued) | A graduate loses My authorisations and My committee reviews with the Trainee role (Step 5.22): flow 13's home. Do not assume either page outlives the role. | Step 5.8's note |
| **T320** (P2, queued) | No committee outcome sends mail (a ratification, an appeal's outcome); five templates have no caller. Flow 19's; name it here only if the design promises a mail (question 7). | — |
| **T362** (proposed) | Members' pages follow the chair live during a sitting; a member reading an appeal sees its resolution only on reload. Not this flow's unless the operator folds it in. | — |
| **T363** (proposed) | The review page's gaps: a line's earlier STAR, a decided line's level and expiry, among others. Touches what the trainee's agenda could show beside "STAR #n." (question 6). | — |
| **T365** (proposed) | An as-opportunity-allows EPA labelled "Due by year end" at an S1 sitting: the agenda's rule, shared by the trainee's agenda. Not this flow's to fix. | — |
| The trainee's review (observed, not filed) | No route, no focus move on View, no loading state (`CD/MyReviews.razor:57–140`); the committee's address refuses a Trainee by the page's `[Authorize]` (`CD/ReviewDetail.razor:2`) and access denied does not point to My committee reviews (question 1). | `act-4/4.41-3…` |
| The appeal rules (observed; question 9) | No deadline: `CommitteeReview.LodgeAppeal` checks only that the review is Ratified and has no open appeal (`D/CommitteeDecisions/CommitteeReview.cs:621–637`); a Closed review cannot be appealed again. The reason is shown to every reader of the committee's page (`CD/ReviewDetail.razor:1453–1458`). | Steps 4.43, 4.44 |
| The appeal body (observed) | The chair or an external member who may sit now (`A/CommitteeDecisions/CommitteeDecisionAuthorization.cs:1041–1042`, `:1106–1116`; `A/CommitteeDecisions/ResolveAppeal.cs:97`, `:106`); no Administrator bypass (D46). Quorum 2, the chair among them (`CommitteeReview.cs:17`, `:20`). Remitted replaces the decision, carries its undecided sentence and changes no STAR; either outcome closes the review for good (`CommitteeReview.cs:652–723`). | Steps 4.44–4.47 |
| The register (observed, not filed) | No revocation date, reviser or reason in the row, no link to the issuing review (`Index.razor`); unpaged; one empty state for "none issued" and "no match"; the InstitutionalAdmin has no menu item for it (`Nav/NavItems.cs:150–156`), only the Home's quick link. | `act-4/4.35-1…`, `act-4/4.35-2…` |
| Dates (observed) | My authorisations prints "2 July 2026" (`MyAuthorisations.razor:30`, `:36`, `d MMMM yyyy`); the register and the review pages print ISO (D1). | `act-4/4.39-1…` |

**Dropped from the old brief (fixed since):** the Outcome select opening on Dismissed, and Upheld (T307, `d03732d`,
D51); the quorum refusal "only at the head of the page" (flow 07, `44de180b`: it is in the Appeals section, focused);
the formative check-in typed "Annual progression" on the trainee's list (T309 item 2, `44de180b`); Entrustment
decisions missing from the speciality admins' menus (flow 06, `85d5a508`); the badge contrast (T322, T335's tokens).

## 6. Questions the design must answer

1. **Where the trainee reads one review** (4.41): below the list, a page of its own (a PROPOSED route), or the list's
   opened row; and where a trainee on the committee's address is sent.
2. **One decision history** for both sides (T308): lifted out of `DecisionForm`; what the trainee sees of it.
3. **The Appeals entries** on both sides: lodged, reason, outcome, resolved, by whom; where her form sits.
4. **Lodging** (4.43): what the form says (who hears it, final, who reads the reason), and a confirmation or not.
5. **Resolving** (4.45–4.47): a confirmation; a live quorum count; where the result and a refusal show.
6. **Certificates:** on My authorisations only, or also on the review's Decided lines (T363 names a line's STAR).
7. **(The operator's) The revocation notice:** send the mail (T319's recommendation), or drop "The trainee is
   notified."? Either way the runbook's 4.36 Expect changes.
8. **(The operator's) "No longer in force":** revoked and expired, superseded off (T319)? The reason on the page, or
   only on the certificate?
9. **(The operator's) The appeal rules:** no deadline, and the reason read by every committee reader. Keep both?
10. **(The operator's) My authorisations in the menu** (BRIEF § 7 B8): keep it under My progress, or give it an item
    (a DESIGN.md change: `NavMenuAuthorizationTests` parses the menu table)?
11. **(The operator's) T308, T319, T260, T264, T324, T325, T328, T329, T272:** folded into the build, or any landed
    first? T319's mail and T324's certificate half are backend; T310, T311, T320, T362, T363 and T365 stay their own
    flows' unless the operator says otherwise.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps span Acts 4, 5, 6 and the appendix; § 9): every
  Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** 4.36 (the dialog, and the
  mail or its sentence: question 7); 4.37 (the reviser by name, if T324 is folded in); 4.42 ("No longer in force");
  4.43 and 4.48 (the row, the history, the Appeals block); 4.41 (the review's route, if it gets one); 4.45–4.47 (the
  form, its confirmation, the quorum line); 4.39 and 5.8 (ISO dates); A.7.7 (the register at 390); search the whole
  runbook (BRIEF.md § 9 item 7) for the words that change. A new page or route needs a step
  (`Scenario/ScenarioRunbookTests`). A new step takes a letter when its number is taken (§ 11, flow 06).
- **Checks the design adds:** 4.43, the row Under appeal at once; 4.48, the replaced decision, the Conditions and the
  appeal's outcome; 4.42, "No longer in force"; 4.36, a ConfirmDialog, Confirm enabled as the reason is typed, the
  result focused; 4.46, the refusal by Resolve appeal and a live quorum count; 4.45, the Outcome readable in full at
  1280; A.7.7 and 4.41 at 390, nothing scrolls sideways, every page checked at 768 as well; one h1 and headings in
  order on every page; every table captioned.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include:
  - `CommitteeDecisions/ReviewDetailAppealFormTests` (T307), a new test of the trainee's history and Appeals block
    (T308), a new test of "No longer in force" (T319); `Admin/EntrustmentDecisionsList*Tests`;
  - `Navigation/NavMenuAuthorizationTests`, `ActiveNavItemTests`; `Hosting/PageTitleTests`;
  - `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`, `IdReferencesTests`;
  - `Design/*`, including `NarrowLayoutTests`, `DefinedClassTests`, `ContrastTests`; the scenario guard in `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones on scratch copies, as `states.md` says; `--appeal-dismissed` on
  its scratch database) and the steps' own captures, keeping their names; `check_baseline_paths.py` at missing 0.
  Compare them with the chosen artboards.
- **Browser check** at 1280, 768 and 390 px as each person in § 1: Dr Molefe, Dr Dlamini, Dr Mahlangu and Dr Ndlovu;
  Dr Zulu, Dr Naidoo and Dr van Rensburg; Dr Mokoena, Dr Sithole and Prof Mbatha.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/` (Role, Route, Do and Expect only). Each Expect
describes the product as flow 08's replay left it (T364, 2026-10-13), except where its step's Gap says otherwise:
4.36's dialog and mail (F-4.36a) and 4.37's reviser by name (F-4.37a). `D` is the replay day.

```text
Step 4.35 — Prof Mbatha reads the issued STARs
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/entrustment-decisions
Do: Open Entrustment decisions from her dashboard's quick links. Download Dr Molefe's PAED-010 certificate. Filter by
  trainee "Molefe" and apply.
Expect:
  - **The list:** five rows, all Active, issued `D`.
    - Molefe: PAED-001 `5`, PAED-010 `4` expiring `D+20`, PAED-012 `5`.
    - Dlamini: PAED-001 `4`, PAED-002 `3b`.
    - Expires reads "—" where none was set.
  - **The certificate PDF** names:
    - Kgosi Kgari Teaching Hospital, "Statement of Awarded Responsibility", Lerato Molefe, PAED-010 and level `4`;
    - Issued on `D` and Expires on `D+20`, Status Active;
    - the rationale, and the evidence summary of the two snapshot lines;
    - Panel Paed Annual Review Panel, the review number, and Chair Thandi Zulu.
  - **The filter** leaves Molefe's three rows.

Step 4.36 — Dr Mokoena revokes one of Dr Dlamini's STARs
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: /admin/entrustment-decisions
Do: Open Entrustment decisions from the menu. Choose Revoke on Dr Dlamini's PAED-002 decision. Give the reason "Staged
  on evidence about PAED-001, not PAED-002; to be decided again." and confirm the revocation.
Expect:
  - **The menu:** Entrustment decisions is lit on its page, with no trail: a list her menu offers (T358, R2-Menus m5;
    as Step A.7.7).
  - **The list:** the same five rows as Step 4.35, since all five registrars are in her speciality.
  - **The dialog** names PAED-002 and Anele Dlamini, and says "Revocation is immediate and irreversible. The trainee is
    notified." Confirm revocation is enabled once a reason is typed.
  - **After confirming:** "Entrustment decision for PAED-002 revoked." The row reads Revoked, with no Revoke button.
  - **The trainee is notified,** as the dialog promises: the application log holds a mail to Dr Dlamini about the
    revocation.

Step 4.37 — Dr Sithole reads the list and what must be decided again
Role: SubSpecialityAdmin — Dr Kabelo Sithole
Route: /admin/entrustment-decisions → /committee/decisions-due
Do: Open Entrustment decisions from the menu. Filter Status to Revoked and apply, then to Active and apply. Download
  the revoked certificate. Then open Decisions due, read the count, set Status to "Every status", press Show, and open
  By EPA.
Expect:
  - **The menu:** Entrustment decisions is lit on its page, with no trail (T358, R2-Menus m5).
  - **Filtered:** Revoked leaves Dlamini's PAED-002, and Active leaves four rows.
  - **The revoked certificate** reads Status Revoked, with "REVOKED", the date, who revoked it (Dr Mokoena, by name,
    T142) and the reason.
  - **Decisions due:** Dlamini's PAED-002 reads "Revoked: re-decide". Its detail reads "STAR #n, issued for `<P>`, was
    revoked. Schedule a review to decide it again." Its one action is Schedule (C3).
  - **By EPA:** PAED-002 reads Decided 0, Not decided 4 and To schedule 1.
  - **Count, default filter:** "71 of 75 decisions due in `<P>` shown."

Step 4.39 — Dr Molefe reads her STARs and downloads a certificate
Role: Trainee — Dr Lerato Molefe
Route: / → /portfolio/authorisations
Do: From her Home's My authorisations card, Open My authorisations. Download the PAED-001 certificate.
Expect: Home's My authorisations card reads "2 at or above · 1 below · 12 with no decision, of 15 EPAs" and lists one
  STAR, "PAED-010 — Leading and operating within a clinical team", a link to its EPA's page, with "4, below training
  year 4's level of 5 · expires `D+20`" under it; its footer is Open My authorisations (T355). My authorisations holds
  three cards, each issued on `D` with the committee's rationale: PAED-001 and PAED-012 at Authorised level `5`, and
  PAED-010 at `4`.
  - PAED-001 and 012 read "No expiry".
  - PAED-010 reads the date `D+20`, with the badge "Expires in 20 days".
  Each Download certificate is named by its EPA. The PAED-001 certificate downloads with Status Active and its evidence
  summary.

Step 4.41 — Dr Molefe reads her review, and not the committee's page
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → /committee/reviews/{ReviewId:int} → /access-denied
Do: Open My committee reviews and view her review. Then type the committee's own address for the same review.
Expect:
  - **The list:** one row, with Period "`<P>` · `<Y>`-01-01 to `<Y>`-12-31", Type Annual progression, State Ratified and
    Decision Satisfactory Progress. Its View is named by the review.
  - **The detail** shows Sits for, Evidence window, Type and State. Its decision is headed "Satisfactory Progress", with
    the rationale and "Present when this decision was taken: …" naming all four.
  - **The agenda** has three columns:
    - Decided with "STAR #n." on PAED-001, 010 and 012;
    - "Not decided at this sitting" with the committee's sentence "To be decided at her final review." on the other
      twelve, due and optional alike: no reason, and no Deferred (T361, D53, E1).
  - **The committee's page** lands on access-denied.

Step 4.42 — Dr Dlamini reads what stands after the revocation
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/authorisations → /committee/my-reviews
Do: From her Home's My authorisations card, Open My authorisations. Then open her review.
Expect: One card, PAED-001 at `4`: the revoked PAED-002 is no longer listed. Her review reads Ratified, Satisfactory
  Progress. The agenda still reads Decided with STAR numbers on PAED-001 and 002, because it records what the sitting
  did.

Step 4.43 — Dr Mahlangu lodges an appeal
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews
Do: View her ratified review and read it. Lodge an appeal with the reason "The single DOPS reflects the start of the
  year, and the Mini-CEX waited eight days on the assessor's side before it was rated. I ask for reconsideration."
Expect:
  - **Before lodging:**
    - the decision reads "Inadequate Progress — Additional Training", with who was present;
    - the agenda shows all fifteen lines "Not decided at this sitting", each with "Not enough observed evidence yet
      this year." (T361, D53, E1);
    - an Appeal reason form is offered.
  - **After lodging:** "Appeal lodged." The state reads Under appeal, the form is gone and the agenda stays in view.

Step 4.44 — Dr Naidoo sees who hears the appeal
Role: CommitteeMember — Dr David Naidoo
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Dr Mahlangu's review.
Expect: The list shows her review Under appeal. The Appeals card lists the appeal, with the date lodged, its reason and
  "(Open)". It offers him no form. It says "Only the appeal body can resolve the appeal: the panel's chair, Thandi Zulu,
  and its external member, John van Rensburg." (T213, T237)

Step 4.45 — Dr van Rensburg, the external member, is offered the appeal
Role: CommitteeMember (external) — Dr John van Rensburg
Route: /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Open Committee reviews, then Dr Mahlangu's review. Read the resolve form, and press Resolve appeal without choosing
  an outcome. Then choose Remitted to see what it asks, and leave without resolving.
Expect:
  - **The list:** the panel's five reviews.
  - **The Appeals card** offers him the resolve form with an Outcome select opening on "Select an outcome…", then
    "Dismissed: the decision stands" and "Remitted: the appeal body replaces the decision" (T307, D51: there is no
    Upheld). Its help text says Dismissed leaves the decision in force, Remitted replaces it, and either closes the
    review.
  - **Resolve appeal with no outcome** says "Choose an outcome." under the select, and resolves nothing.
  - **Remitted** reveals:
    - Replacement category, opening on "Select a category…";
    - Replacement rationale;
    - Replacement conditions, optional;
    - Present, with both Thandi Zulu (chair) and himself ticked and locked.
  - **Nothing changes** until he submits.

Step 4.46 — A remit without a quorum is refused
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: On Dr Mahlangu's review, choose Remitted, the replacement category Satisfactory with Observations and a
  rationale. Tick nobody besides herself as present, and resolve.
Expect: Resolve appeal is refused with "A committee decision needs at least two panel members present: the chair and
  at least one other." (T165: a remit records its own quorate sitting), in the Appeals section beside the form, with
  the focus on it (T361, round 3 item 15). The review stays Under appeal.

Step 4.47 — Dr Zulu remits the decision
Role: CommitteeMember (chair) — Dr Thandi Zulu
Route: /committee/reviews/{ReviewId:int}
Do: Keep Remitted, Satisfactory with Observations, and the rationale "The stalled Mini-CEX was the assessor's delay;
  progress is adequate, with observed assessments to follow." Write the Replacement conditions "Two observed Mini-CEX
  and one DOPS before the next review." Tick Sarah Botha as present, and resolve the appeal.
Expect: "Appeal resolved." The state reads Closed (T250).
  - **Decision:** the replacement heads it, "Satisfactory with Observations" with "About the EPAs not decided: Not
    enough observed evidence yet this year." (carried from the first decision, T361, E5), "Conditions: Two observed
    Mini-CEX and one DOPS before the next review." (T307) and "Present: Thandi Zulu (chair), Sarah Botha". The first
    decision stays below it, marked "Replaced on appeal by the decision above.".
  - **Appeals card:** the appeal reads "(Remitted)".
  - **No STAR changed:** the review issued none.

Step 4.48 — Dr Mahlangu reads the outcome
Role: Trainee — Dr Nomsa Mahlangu
Route: /committee/my-reviews
Do: Open My committee reviews and view the review.
Expect: The row reads State Closed and Decision Satisfactory with Observations. The detail's current decision is the
  replacement, with "Present when this decision was taken: Thandi Zulu (chair), Sarah Botha". The agenda's fifteen lines
  still read "Not decided at this sitting" with "Not enough observed evidence yet this year.": the replacement carries
  the first decision's sentence (T361, E5). No appeal form is offered.

Step 4.51 — Dr Ndlovu reads both reviews
Role: Trainee — Dr Sipho Ndlovu
Route: /committee/my-reviews
Do: Open My committee reviews.
Expect: Two rows for `<P>`. The annual review reads Annual progression, Ratified, Outcome Deferred. The check-in reads
  Type "Formative check-in" (T309 item 2; T364, C8), State Closed, with Decision "No binding decision"; its detail's Type
  reads "Formative check-in".

Step 5.7 — Prof Mbatha reads Dr Molefe's STARs
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: / → /admin/entrustment-decisions
Do: Open Entrustment decisions from her dashboard's quick links. Filter by the trainee name "Molefe", first with the
  status Active and then with All, applying the filters each time.
Expect: There are fifteen Active rows, one for each EPA from PAED-001 to PAED-015. Each row:
  - is at its EPA's exit level;
  - was issued on `D`, except PAED-001's and PAED-012's, which Act 4 issued;
  - has no expiry;
  - offers Download and Revoke.
  With All, Act 4's PAED-010 STAR, which Step 5.6 superseded, reads Superseded and offers Download only. She revokes
  nothing.

Step 5.8 — Dr Molefe reads her final review and keeps a STAR certificate
Role: Trainee — Dr Lerato Molefe
Route: /committee/my-reviews → / → /portfolio/authorisations
Do: Open My committee reviews and view the pre-graduation review. Then open My authorisations from her Home's card (its
  Open My authorisations), and download the certificate for PAED-001.
Expect: The review is listed as Pre-graduation, Ratified, Graduate (programme complete). Its detail shows:
  - the review type;
  - the decision and its rationale;
  - who was present;
  - an agenda in which each EPA reads Decided, with its STAR number.
  The Lodge appeal form is offered, and she lodges no appeal. My authorisations holds 15 cards, each with its level, its
  issue date and "No expiry". The certificate downloads as `star-certificate-PAED-001-<id>-<8 hex digits>.pdf`.

Step 6.22 — Prof Mbatha sees the pause, and cannot lift it
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/epas → /admin/curricula → /admin/curricula/{Id:int}/items → /admin/entrustment-decisions
Do: Read the EPA list, then 11.1's items. Then open Entrustment decisions and filter by Dr Molefe's name. Revoke
  nothing.
Expect:
  - **The EPA list.** PAED-012 reads Inactive. No national EPA offers her Edit: only its College changes one (T211's
    rule; `UpdateEpa`).
  - **11.1's items.** PAED-012's row is marked "(inactive: not in force)", under the same notice. Every row reads "Set
    by the College".
  - **Dr Molefe's STARs.** Her fifteen from Act 5 still read Active, beside Act 4's PAED-010, Superseded (the status
    filter starts on All). The PAED-012 row names its EPA "PAED-012 — Communicating
    with and counselling patients, caregivers and healthcare teams (no longer in use)" (T255): a pause leaves a STAR
    standing.

Step A.7.7 — Dr Mokoena on her phone
Role: SpecialityAdmin — Dr Refilwe Mokoena
Route: / → /committee/panels → /committee/decisions-due → /admin/entrustment-decisions
Do: At 390 px, open the dashboard, then Decision panels, Decisions due and Entrustment decisions from the menu.
Expect: Home stacks Waiting for assessors, badged "3 waiting, 1 overdue", Registrars, badged "2 registrars", and Targets
  by EPA, KGK-001 first among the EPAs neither registrar has met, by code (T358). Decision panels' list and Who
  decides each EPA stack, a block per panel and per row of EPAs (T364), and Decisions due reads as at Step A.7.5.
  Entrustment decisions is in the menu, lit on its page, with no trail: it is a list the menu offers (R2-Menus m5).
```
