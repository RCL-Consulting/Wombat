# Flow 14 round 1: assessment (T387 step 5)

Read-only, at HEAD `f3cddf88`. Canvas "Wombat 14" (https://claude.ai/artifact/FrvjJMZLwRd2Y3AMDEJeGs), design system
copied at version 1793144981-e3ba (`canvas.json`, `designSystems`). Boards: `design/flows/14-data-rights/round-1/*.dc.html`
(18, every board's text and markup read) and `canvas.json`. `W/` = `src/Wombat.Web/Components/Pages/`, `DR/` =
`W/Admin/DataRights/`, `A/` = `src/Wombat.Application/Features/DataRights/`, `I/` = `src/Wombat.Infrastructure/DataRights/`,
`Nav/` = `src/Wombat.Web/Navigation/`, `App` = `execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md`.
**Obs** = observed in the code, the runbook or the boards. **Inf** = inferred. Brief sections are
`design/flows/14-data-rights.md`'s.

**Q1–Q16 are assumed taken as recommended; confirm.** The operator overruled none of them before round 1 was sent, and
the boards follow them. They have not said "yes" to them in words. As in flow 13, a board that contradicts one is a
must-fix unless it argues to reopen it. Where it argues, the point is a question in § 8.

## Summary

- **My data rights: pick A** (one page, three `<h2>` sections: Your requests, Make a request, Reminder emails; the type
  as four described radios; each decided row carries its answer in an Answer column).
  - **Why A:** it is Q4 as decided and the cheapest: 4 + 1 to make and download an export, 1 + 0 to read a rejected
    answer (Steps, recomputed).
  - **Reject B.** Its form page is a PROPOSED route and costs a press and a page load. Q4 keeps one page.
  - **Reject C.** It brings a NEW RequestCard, an `<h3>` per request, and a legal form behind a note panel. "Download
    your export" is wrong for an Access request.
  - **Take from C:** nothing structural. Its "The answer, ‹date›:" phrasing may serve the 390 block's label (N).
- **A request and its correction: pick A** (one column of `<h2>` sections Request, Decision, Correction, Decide; the
  correction recorded in its own section, Complete request at its foot).
  - **Why A:** it is Q2 and Q12 as decided, and the cheapest: 4 + 3 for one correction, 1 + 3 for each one after it.
  - **Correct it with B's fixed list of records (M5, Q26).** The command cannot take a free-text record.
  - **Reject B.** It needs a PROPOSED route and page, and costs 6 + 2 with a page load each way.
  - **Reject C.** Its NEW RequestStatusCard moves Decide and Complete out of Q12's sections into a card above the record,
    and repeats the state in two places.
- **The queue: pick V1** (one table; Type and Status, then Show; Requester by name over the address; a Request id
  column; PagerControls; StackWide).
  - **Why V1:** it is Q10 as decided.
  - **Reject V2.** It drops Q10's Status filter and sorts Waiting oldest first against the GOAL's "newest first". Its
    "On" date for a withdrawn request needs a time the domain does not store (`DataRightsRequest.Withdraw()` keeps none).
  - **Take from V2:** nothing. An approved Rectification is found by Status = Approved in V1.
- **Deciding a request: pick V1** (the note, then Reject and Approve; on an Erasure, "Approve and erase…" as an outline
  trigger opening the ConfirmDialog that counts and keeps).
  - **Why V1:** it is Q3 and Q12 as decided, and it is a press cheaper than V2 on every decision (Steps).
  - **Reject V2.** Its radio plus "Record decision…" adds a press and a NEW refusal ("Choose Approve or Reject."). Its
    standing "What erasure ends" section repeats the dialog and lists T276's leftovers with "[n] such items", against
    Q3.
- **Not found: correct (M1).** The board draws flow 01's Page not found. Q13, and DESIGN.md's Unavailable pattern that
  flows 12 and 13 followed, ask for one page of the request's own (Q17).
- **The kept request: confirm, with corrections.** The pseudonym in the h1, the Requester row and the queue's row, and
  an info line "erased on 2026-10-28, at this request" are right (Q11). The trail drops Data rights requests at 1280,
  and at 390 its crumb is an empty link (S10).
- **The must-fixes** (§ 7):
  - **M1:** Not found as Page not found, against Q13 and DESIGN.md.
  - **M2:** the digest help still names "a request to assess", which no code mails.
  - **M3:** Q5's notice drawn as undecided.
  - **M4:** gendered pronouns that Wombat has no data for.
  - **M5:** the correction's record as free text, against the command's shape, and a "Recorded by" nothing stores.
  - **M6:** the Steps board misses the new correction step, A.2.4/A.2.6 and the outcome SQL.
- **The attachments:** no board says whether they arrived. No board names a capture or writes an Assumptions note, and
  nothing on the boards could only have come from one. Re-attach § 4's twelve and ask round 2 to say (§ 11, flow 13).
- **The Spec and Steps boards were drawn**, as the ask required. Their counts recompute (§ 2–5), but the Steps
  board's per-step table is incomplete (M6).

## 1. What was drawn

18 boards in five rows (`canvas.json` notes t1–t5).

- **The wireframes** (Main, WF-Requester-B/C, WF-Request-A/B/C) are grey boxes in hex, which is right for a wireframe.
- **The fidelity boards** (Queue, Decide, NotFound-Kept) link `ds/wombat/tokens.css` and `components/bundle.css`, plus
  a board stylesheet blob (`/_blob/2be8180b…`) for the `.pv-*` captions.
- **Every class the fidelity boards use is in `design/system/components/bundle.css`**, except the board's own `.pv-focus`,
  `.pv-id`, `.pv-muted`, `.pv-note`, `.pv-stage`, `.pv-state` and `.pv-tag` (Obs, script over the 11 fidelity boards).
- **The mark** is the copied `/_blob/616538c681cf4475edf7c466864e408d` on every fidelity board.
- **Two variations, not three,** for the queue and for the decision. The ask allowed 2–3.

| Row | Boards | State drawn |
|---|---|---|
| 1 · My data rights (wireframes) | Main (= A), WF-Requester-B, WF-Requester-C | A.1.1 nothing requested; A.1.2 no reason, then submitted; A.1.5 completed; A.1.6 withdrawn; A.1.9 rejected; an illustrative Rectification approved, then completed; A.2.3/A.2.5; loading and load error (in words); A.7.3 at 390 |
| 2 · A request and its correction (wireframes) | WF-Request-A, B, C | Mahlangu's Rectification approved with one correction; Complete refused with none; completed; A.1.3 Submitted with Decide; 390 |
| 3 · The queue (fidelity) | Queue V1/V2 at 1280 and 390 | A.1.3 filtered; nothing matches; A.1.8 three; A.1.12 the Administrator's, Erasure; after the erasure; fifty with a pager; loading; load error |
| 4 · Deciding (fidelity) | Decide V1/V2 at 1280 and 390 | A.1.3 Submitted, the note refused, approved; A.1.8 withdrawn, rejected; A.1.12 before, the dialog, Cancel, approved; the two erasure refusals and withdrawn-meanwhile; loading; load error |
| 5 · One proposal each; Spec, Steps | NotFound-Kept at 1280 and 390, Spec, Steps | A.5.7; the kept request on its page and in the queue; the aftermath in words |

**Not drawn in round 1** (round 2's):
- **My data rights:**
  - at fidelity at all (it is a wireframe);
  - Dr du Plessis's and Mr Smit's own menus (former trainee: Home, My progress, Export portfolio, My data rights,
    `Nav/NavItems.cs:218–227`);
  - the erasure request while Submitted, with its Withdraw;
  - an approved Rectification on the requester's row before any correction;
  - a 4,000-character reason and a long note on the row;
  - two requests of one type in one minute;
  - a requester with no institution ("the Wombat administrator decides it").
- **A request:**
  - Decide at fidelity for a Rectification approved with no correction yet;
  - the Correction section's "who can" line for the Administrator (a link to the profile) and for an ended programme;
  - five corrections (heavy);
  - a request withdrawn while open, at 390;
  - a staff member's own request (Q21).
- **The queue:**
  - a 768 px row;
  - the Administrator's real grouped menu (S1).
- **The decision:**
  - an erasure of a person with a review under appeal (the dialog's Kept line);
  - an erasure of someone holding a staff role (S11);
  - the dialog's failure path: the refusal after the dialog closes, and where the focus goes.
- **768 px** anywhere (§ 11, flow 06).

## 2. My data rights: A, B, C

All three keep, rightly (Obs):
- one `<h1>` "My data rights" and `<h2>` sections (T280; today `<h3>` cards, `W/Profile/DataRights.razor:28, :64, :93`);
- no optional-processing box (Q6, T318), and the line that what the programme needs cannot be objected to;
- the four types by label, with the reason required;
- "Please provide a reason for your request." under Reason, focused, nothing sent (words unchanged; today at the page's
  top, `:249`);
- times "2026-10-28 00:01 SAST" (stored 22:01 UTC, `App` A.1.2 Actual) and "00:03 SAST" (A.1.6, A.1.7);
- the answer on a decided row with its date and note, never who decided (T319);
- Download only on a Completed Access or Export (`:122–123`);
- Withdraw only while Submitted (Q14; today also Under review, `:113`);
- no ids on her own page (§ Content's ids rule).

- **A, one page, requests first.** Your requests is a table (Submitted, Type, Status, Answer, the action). Make a request
  has four described radios ("Access: Everything Wombat holds about you, as a download, once the request is approved",
  "Export: The same download, to take your data elsewhere…", Rectification, Erasure "…Once approved it cannot be
  undone, and you can no longer sign in."). Reminder emails holds the digest box, Save and its own result.
  - **Counts (Steps, recomputed):** export and download 4 + 1 (menu, Export, Submit, Download; the reason typed); a
    rejected answer 1 + 0; the digest 3 + 0; at 390 one more each (Menu).
  - **At 390** Your requests stacks, a labelled block a row, Download 44 px. This changes A.7.3's "scrolls inside its
    container", which is the right change (T323).
- **B, a form page.** "+ Make a request" is a primary header action opening `/account/data-rights/new` (PROPOSED). The
  answer is a second line of each row.
  - **Counts:** 5 + 1 and 6 + 1 at 390. One press and a page load more than A for a rare form.
  - **Its "What happens next" box** is good words, and A can carry them as its section's intro line.
- **C, a card per request.** An `<h3>` card each, with the reason shown back and "The answer, 2026-10-28:" quoted. The
  form sits in a note panel behind "+ Make a request…".
  - **NEW:** the RequestCard.
  - **Its checkbox** "Stop the regular reminder emails" changes A.2.3's and A.2.5's Do.
  - **Counts:** 5 + 1 and 6 + 1.

**Recommend A.**

**Correct in A:**
- **M2:** the digest help's "such as a request to assess or an invitation to give feedback". No code mails a request to
  assess (T320; `IEmailSender` is injected only in invitations, MSF open and resend, the coordinator's one-activity
  reminder, the graduation mail and the five jobs). Q27 has the words.
- **M3:** "Export request submitted. Its answer shows here once it is decided." drops Q5's promise. Q5 was taken as
  recommended: the decision notice is built, and "You will be notified…" stays. Draw it as decided, for example "Export
  request submitted. You will be emailed when it is decided, and its answer will show here."
- **The section's heading** is "Reminder emails", where Q4 decided "Email preferences" (Q24).
- **"0 of 4,000 characters"** is a live counter: per-keystroke work over SignalR (§ 5.1). One exists already (T366's
  revocation reason in a ConfirmDialog field), so it is not NEW, but mark it PROPOSED on this page (N1).
- **"Staff at Kgosi Kgari Teaching Hospital decide it"** reads the institution's name from the claim's id. That is
  DATA, not marked (S8).
- **A.1.1's frame** draws Dr Dlamini ticking the digest opt-out, because the optional box has gone. That changes A.1.1's
  Do, and it also changes the appendix's outcome SQL ("Dlamini opted out of optional processing only", `App:1746`) (Q28).
- **Draw A at fidelity** in round 2, with the Trainee's, the former trainee's and the Coordinator's real menus.

## 3. A request and its correction: A, B, C

All three draw, rightly (Obs):
- the h1 by type and person, "Rectification request from Nomsa Mahlangu" (Q12), with the crumb its id;
- Back to list gone (Q12; today `DR/RequestDetail.razor:12–15`);
- "Decided by" as the name with the id, "Pieter Smit · 43ad0cb7-5ce6-46d7-8595-7117d130c7dc" (DATA: the name);
- Complete refused with none recorded, in PROPOSED words "Record at least one correction before completing the
  request." (today "At least one rectification must be applied before completing the request.");
- Q1's role: the Coordinator and the Administrator carry it through. A's intro asks "Speciality admin keeps it too?".
  Q1 decided no: `RectificationRoles` folds into `ReviewRoles` (`A/DataRightsAuthorization.cs:29, :32`).

- **A, in its section.** The Correction section holds:
  - the corrections recorded (Record, Value before, Value after, Recorded);
  - an inline form (Record free text with the hint "Where it is held, such as Account › Last name", Value before, Value
    after);
  - Record correction;
  - a guide "Where the data is changed";
  - Complete request at its foot.
  - **Counts (Steps):** 4 + 3 for one correction (menu, Review, Record correction, Complete; three typed), 1 + 3 for
    each further one.
  - **WF-Request-A's own intro says 3 + 3,** without the menu link (N2).
- **B, a form page.** `/admin/data-rights/{Id}/corrections/new` (PROPOSED), with the record chosen from described radios
  (Account › First name, Account › Last name, Trainee profile › Programme start, Other), each saying where it is changed.
  Complete request has an end section of its own (flow 13's).
  - **Counts:** 6 + 2, and 3 + 2 for each further one, with a page load each way.
- **C, a status card.** A NEW RequestStatusCard heads the page, "Approved · next: …" with ticked steps and Complete
  request in it. The form opens in a note panel; the record is a select.
  - **Counts:** 6 + 2.
  - **Against Q12:** Decide moves into the card in every state.

**Recommend A, with B's fixed list of records** (as radios inside A's section, flow 08's choice of a few kinds).

**Correct in A:**
- **M5, the record.** `ApplyRectificationCommand` takes `TargetType` (a string of up to 200), `TargetId` (a **Guid**),
  `FromValueJson` and `ToValueJson` (`A/Commands/ApplyRectification.cs:21–27, :32–34`). A typed "Account › Last name" fills
  no TargetId, and a typed value is not JSON.
  - **Choose the record from a short fixed list** that the build maps to a type and an id: Account › First name,
    Account › Last name, Trainee profile › Programme start, Other (named in the before value).
  - **Or change the command** (PROPOSED). Either way it is DATA (Q26).
- **M5, the "Recorded" column.** "Pieter Smit · [date time] SAST": `DataRightsRectification` stores `AppliedOn` but no
  actor (`src/Wombat.Domain/DataRights/DataRightsRectification.cs:5–11`). Add a recorder (DATA, a column) or drop "by".
- **M5, the guide's "Account (name, email): by Nomsa Mahlangu on My account".** No page changes an email: My account
  shows it read-only (`W/Account/Profile.razor:53–54`), and only an SSO sign-in writes one (`ExternalLoginHandler`).
  - **Names:** only the person changes a name, on My account (`UpdateCurrentUserProfile` is `UpdateNamesAsync`'s one
    caller). That much is right.
  - **The guide should say:** a name, by the person on My account; an email, by nobody in Wombat (the institution's
    sign-in, or not at all); a programme start, by an Institutional admin or the Administrator on the trainee profile
    (`W/Admin/Trainees/TraineeProfileEdit.razor:2`), which the Coordinator cannot open (Q25).
- **M4:** "Her programme has ended: its record cannot be changed" (the Spec's edge) and "on her trainee profile" use a
  pronoun. Name the person.
- **The Administrator's view:** draw the link "Open Nomsa Mahlangu's trainee profile" that C's note describes, and the
  ended-programme line (T305 stays open, Q2).
- **Complete's refusal sits under the button,** focused (A). It names no field, so DESIGN.md puts it in the section's
  `ActionResult` with the focus on the button pressed (§ Form system, "A refusal that names no field"; DESIGN.md:1854)
  (S6).
- **The completed state** says "Request completed. Nomsa Mahlangu reads it as Completed on My data rights." Keep it, and
  add Q5's notice: she is emailed.

## 4. The queue: V1, V2

Both draw, rightly (Obs):
- the Coordinator's flat eight, Data rights requests lit with `aria-current="page"`, and My data rights under the rule
  (`Nav/NavItems.cs:175–176`);
- the subtitle by institution;
- the filters before Show in reading and tab order, with Show and Clear filters in `.filter-actions` (T328, flow 06);
- the types by label, with no Objection (Q8), and the statuses by label, with no Under review (Q8);
- the requester by name, the address under it (DATA: `RequesterDisplayName` is the sign-in name, the address,
  `A/Commands/SubmitDataRightsRequest.cs:44–46`);
- the times in SAST;
- Review named for the row, with the name and the SAST time;
- the request id in its own column, wrapping;
- `list-section-title` with the focus after the pager (flow 08);
- "No requests match: No Access request is Completed at Kgosi Kgari Teaching Hospital. Clear filters to see every
  request." in `.detail-card--empty`;
- loading as skeletons with nothing offered;
- the load error with Try again and no exception's text (T329);
- after the erasure, the row named `deleted_user_0ae1d64b` over "Erased 2026-10-28" (Q11).

- **V1, one table.** "Requests (n)" over a captioned `clinic-table--stack-wide` table.
  - **For the Administrator** it adds an Institution filter and column (PROPOSED, DATA; Q23).
  - **Counts:** A.1.3 6 + 1 (menu, Type, Status, Show, Review, Approve; the note), as today. A.1.12 6 + 1 with the
    dialog's confirm (today 5 + 1, no confirmation).
- **V2, Waiting and Decided.** No Status filter; "Waiting for you · 1 to decide", oldest first; "Decided", newest first;
  one link a row.
  - **Counts:** a press cheaper (5 + 1).
  - **Against Q10:** the Status filter goes.
  - **Against the GOAL:** Waiting is "oldest first".
  - **DATA unmarked:** the "On" date of a Withdrawn row. `Withdraw()` stores no time
    (`src/Wombat.Domain/DataRights/DataRightsRequest.cs:115–121`).

**Recommend V1.**

**Correct in V1:**
- **The pager's size.** Both draw 20 (options 20, 50, 100). Q10 said 50, but `PagerControls` defaults to 20
  (`src/Wombat.Web/Components/Shared/PagerControls.razor:51`), and flows 08, 12 and 13 took 20 (Q18).
- **The badges (Spec, Q22):**
  - Approved as `badge-accepted` (amber) is a change: today Approved is green with Completed (`BadgeFor.cs:153–160`).
  - Withdrawn as `badge-withdrawn` is only a rename. The Spec and the A.1.8 caption say "Withdrawn moves from red";
    it is grey today (`badge-draft`, the switch's default; DESIGN.md's table says "withdrawn grey"). Correct the claim
    (S3).
- **The Administrator's states are drawn under the Coordinator's menu** ("Acting as Coordinator", the flat eight) on
  Queue V1's A.1.12 rows. devadmin's menu is grouped, with Data rights requests under Platform
  (`Nav/NavItems.cs:144`) (S1).
- **The requester's cell is a `<td>`.** It is the row's name: make it `th scope="row"`, as flows 12 and 13's lists do
  (RowNames) (S4).
- **Placeholders the cast fills:** "[address]" is mahlangu@, duplessis@ and ndlovu@kgk.wombat.local (README § Cast:148,
  :147). Dr du Plessis's request id starts 01a17f63-7af4- (`App` A.1.8 Actual). Draw the prefix and mark the rest
  unreplayed (S5).
- **At 390:**
  - drop the inline `min-height:2.75rem;width:100%` on Review and on the pager. `.clinic-table--stack-wide .btn` and
    `.pager .btn` are 44 px already below 641 px (bundle.css:785–787, :2730);
  - Clear filters is missing, though the caption names it;
  - the page-size select is gone, though PagerControls always draws it (N3).
- **`.pv-id` becomes the NEW `.request-id`** the Spec names. `.pv-muted` is the system's `.muted` (N4).

## 5. Deciding a request: V1, V2

Both draw, rightly (Obs):
- the trail Home › Data rights requests › the id, Data rights requests lit (`aria-current="true"`, the owner table;
  `Nav/NavOwners.cs:139`);
- the h1 by type and person (Q12);
- the subtitle "Submitted 2026-10-28 00:01 SAST · Kgosi Kgari Teaching Hospital" (DATA: the institution's name from
  `DataRightsRequest.InstitutionId`);
- the sections in flow 12's `.user-page` and `.user-section`, with `dl.detail-list` and its `overflow-wrap: anywhere`
  (bundle.css:6423–6437), so the ids wrap (T323);
- "A decision note is required." under the note, `aria-invalid`, focused, the words unchanged;
- the results in the section that acted, focused, as flow 12's user page does (DESIGN.md:3617);
- the values: approved 00:01 SAST (22:01:54 UTC), rejected 00:03 SAST (22:03:39 UTC), erased 00:04 SAST (22:04:50
  UTC), "Demo Administrator · abd4ed66-…", check-in #8 Scheduled before the Paed Annual Review Panel, Paediatric EPA
  Curriculum (11.2), `deleted_user_0ae1d64b` (`App` A.1.3, A.1.8, A.1.10, A.1.12 Actuals);
- the dialog `dialog-scrim dialog-card` with `.form-container`, `.dialog-body` and `ul.dialog-list`, the safe button
  first in the DOM and focused. That is ConfirmDialog's action mode with BodyContent and CancelLabel
  (`src/Wombat.Web/Components/Shared/ConfirmDialog.razor:87–150`).

- **V1, the note then two buttons.**
  - **The page:** for an Export, a help line "Approving makes the export ready at once…". For an Erasure, "Approving
    erases Sipho Ndlovu's personal data at once and cannot be undone. You are asked to confirm…", and the trigger
    "Approve and erase…" as an outline button (flow 12's destructive rule).
  - **The dialog** "Erase Sipho Ndlovu's personal data?":
    - "This cannot be undone." bold first;
    - "It ends:" his account and sign-in; 1 open committee review (Formative check-in #8, Paed Annual Review Panel,
      Scheduled); no feedback campaign; his programme, ending today as Withdrawn;
    - what it keeps under a pseudonym;
    - the buttons "Keep his data" and "Erase Sipho Ndlovu".
  - **After it:** "**The personal data is erased.** The request is approved and completed. 1 committee review was
    withdrawn and the programme ended today. The account is now deleted_user_0ae1d64b; its open session ends within a
    minute." The page then names the pseudonym.
  - **Counts:** A.1.3 6 + 1; A.1.12 6 + 1 (Steps, recomputed).
- **V2, a choice then one button.** Approve and Reject as described radios, then "Record decision", with a
  RefusalSummary when both the choice and the note are missing. On an Erasure, a standing warning section "What
  erasure ends" sits before Decide, with Approve and erase pre-checked, and a shorter dialog.
  - **Counts:** a press more everywhere (7 + 1).
  - **Its pre-checked "Approve and erase"** puts an irreversible choice on the default, which flow 13 rejected for
    the end of a programme (its V2, Inf).
  - **Its T276 line** ("[n] such items today", DATA) is what Q3 kept out.

**Recommend V1.** Its counts are the dialog's one press over today's, which T264 asks for.

**What the erasure counts, checked against `I/ErasureExecutor.cs`** (Obs):
- **Reviews:** every committee review whose trainee is the person, at any institution and before any panel, in an open
  state (`CommitteeReview.OpenStates`: Scheduled, In progress, Decided) is withdrawn with its staged decisions removed
  (`:195–218`). A ratified review and one under appeal are kept.
- **Campaigns:** every MSF campaign about the person that is not Released or Withdrawn is withdrawn (`:278–294`), at any
  institution.
- **Profiles:** every trainee profile ends; an active one ends that day as Withdrawn with its ended curriculum kept
  (T312), and one already ended keeps its end (`:323–339`).
- **The account:** user name set to the pseudonym; email, names, phone and password cleared; locked; institution,
  acting role, roles, external logins and scopes removed (`:378–422`).
- **The request:** left as submitted, keeping the address (`:425–427`). A `DataRightsErasureRecord` is written with the
  pseudonym and `ErasedOn` (`:431–437`).
- **So the dialog's lines are right for a trainee.** Its forecast is DATA: a query gated as the command, counting what
  `ExecuteAsync` would touch. The result's counts are also DATA, since `ExecuteAsync` returns nothing today.
- **For someone holding a staff role,** the erasure also removes the roles and pseudonymises panel seats, attendance,
  evidence lines and campaigns they created (`:234–253`, `:278–299`), and the dialog would count nothing of it (S11).

**Correct in V1:**
- **M4, the pronouns:** "Keep his data", "His account…", "his open session", "his programme", "his ratified review",
  "Approving ends his account" (the note's help), "Anele Dlamini can download her export", "Nomsa Mahlangu reads your
  note". Wombat stores no gender or pronoun (no such field in Domain or Identity, Obs). Flow 13 replaced "Their last day"
  with "The last day" (its S6). Use the name, "the", or a neutral label.
  - **The cancel label is Q19:** "Cancel", or "Keep the personal data".
- **The dialog's title and confirm against Q3 (Q19).** Q3 decided "Erase ‹address›'s personal data?" and "Erase and
  approve". The board names the person and confirms with "Erase Sipho Ndlovu".
- **At 390 the dialog's buttons** are stacked by an inline `flex-direction:column`, the safe one on top. bundle.css's M8
  rule (`.dialog-card .form-actions`, column-reverse, bundle.css:6180–6189) puts the action on top and keeps the safe
  button first in the DOM.
  - **The same holds for Decide's own row:** the board reverses the DOM (Approve first). `.user-page .form-actions`
    already stacks it with the primary on top (bundle.css:6513–6516).
  - **Drop both inline styles,** keep the 1280 DOM order (S2).
- **At 390 the trail** is drawn as `breadcrumb-ancestor` "Data rights requests" plus the current id. Below 641 px the
  system hides ancestors and the current crumb and shows the parent as a 44 px back link (bundle.css:3228–3239). Draw
  `breadcrumb-parent` (N5).
- **"You cannot download it yourself."** It is true for a Coordinator and false for the Administrator, who may download
  any request (`A/DataRightsAuthorization.cs:97–106`, `DemandExportAccess`). Say it only to a caller who cannot (S7).
- **The withdrawn-meanwhile refusal** "Sipho Ndlovu withdrew this request on [date time] SAST." needs a withdrawal time
  the domain does not store. Drop the time (PROPOSED words without it), or add `WithdrawnOn` (DATA). Today the command
  answers "Only submitted or under-review requests can be approved." (`DataRightsRequest.cs:84–87`) (S7).
- **The erasure refusals:**
  - Draw them after the dialog closes, with the focus on the alert. The note is kept, as drawn.
  - The PseudonymSalt rewording is PROPOSED, and right (it says nothing was erased and the request is still Submitted).
  - PersonChanged's words are today's.
- **"Decided on 2026-10-28 [time] SAST"** for the rejection is 00:03 (22:03:39 UTC, `App` A.1.8 Actual) (S5).
- **"Request id" twice:** the crumb and a Request row both show the id. Keep both (A8's exception), or drop the row (N).

## 6. Not found and the kept request

- **Not found (M1).** The board draws flow 01's "Page not found" / "There is no page at this address." with nothing lit
  and Go to Home, through PageHeader's `Page=typeof(NotFound)`.
  - **Q13 decided otherwise:** "Request not found" / "There is no data rights request with this id that you can open."
  - **So did DESIGN.md:** an Unavailable page per record, "Activity unavailable", "Portfolio unavailable", "Profile
    unavailable" with Go to Trainees (DESIGN.md:1858–1864; flow 12's C21, flow 13's S12).
  - **Right on the board:** the one state for unknown and foreign ids, no "not authorized" alert, and no exception's
    text. The command's one refusal message (`A/DataRightsAuthorization.cs:121`) stays behind the page.
  - **Recommend** "Request unavailable" / "This data rights request does not exist, or you cannot open it." / Go to
    Data rights requests, with Data rights requests lit (Q17).
  - **A.1.4's 404 for the download is unchanged,** rightly.
- **The kept request (Q11).** The h1 "Erasure request from deleted_user_0ae1d64b", the Requester row and the queue's row
  read the pseudonym. Email is gone, the user id 4e5ce9ca-… is kept, and the reason, the decision and the note are kept.
  An info alert says "This person's personal data was erased on 2026-10-28, at this request. The request is kept as the
  record of it, under the pseudonym." (PROPOSED words, right).
  - **Its DATA is half there already (Obs):**
    - `DataRightsErasureRecord` holds the pseudonym and `ErasedOn` for the request (`I/ErasureExecutor.cs:431–437`);
    - the account's user name is the pseudonym (`:378`).
    - **Recommend Q11 as decided:** the erasure rewrites `RequesterDisplayName` to the pseudonym. The stored address of
      an erased person is the defect itself (T276), and reading it at render time would leave it in the table. The
      "Erased" date reads the erasure record.
  - **Correct:**
    - the 1280 trail is "Home › id": Data rights requests is missing;
    - at 390 the crumb is an empty `<a href="#">` with broken markup;
    - the alert sits as the first child of `.user-page`, before the Request section. Say whether it is a notice (an
      `Alert` above the sections) or part of Request (S10).

## 7. Corrections

Severity: **M** must (round 2 cannot go out without it), **S** should, **N** nit.

### Wrong against the code, the runbook or a decided question

| # | Board(s) | Correction | Sev |
|---|---|---|---|
| M1 | NotFound-Kept-1280, -390; Spec | **Not found is drawn as flow 01's Page not found.** Q13 and DESIGN.md's Unavailable pattern (DESIGN.md:1858–1864; flow 12's C21, flow 13's S12) ask for a page of the request's own, one for unknown and foreign ids alike. Draw "Request unavailable" (Q17) with a way on to Data rights requests. A.5.7's Expect changes with it. | M |
| M2 | Main, WF-Requester-B, -C; Spec (Hint) | **The digest help names "a request to assess", which no code mails** (T320; `IEmailSender`'s callers: IssueInvitation, ResendInvitation, OpenMsfCampaign, ResendMsfLinks, SendActivityReminder, CompleteTraineeProfile and the five jobs; one-off notices by `AccountNotices`: admission, a STAR revoked, an MSF report released). Q7 says to name only mail sent today. The brief's own Q7 list is also wrong twice: "a request to assess", and "a password reset" (Wombat sends no reset email, `W/Account/ForgotPassword.razor:10`). Words in Q27. | M |
| M3 | Main, WF-Requester-B, -C; Spec (PROPOSED, Result) | **Q5 is drawn as undecided.** "Its answer shows here once it is decided." with "the email line stays only if Q5's notice is built". Q5 was taken as recommended: draw the notice as built, the submitted result promising it, and A.1.5/A.1.9's "no email" Expects changing. The notice names no decider and carries no note. `AccountNotices`' contract says a notice tells "what happened, when and by whom" (`src/Wombat.Application/Common/Email/AccountNotices.cs:21–22`), so the data-rights notice is its exception and must say so. | M |
| M4 | Decide V1/V2 at 1280 and 390, WF-Request-A, Spec (Result, Edge) | **Pronouns Wombat cannot know.** "Keep his data", "His account", "his programme", "her export", "Her programme has ended", "Approving ends his account". No gender or pronoun is stored. Use the person's name, "the", or neutral words (flow 13's S6 precedent). | M |
| M5 | WF-Request-A (and the pick's Correction section) | **The correction's shape does not fit the command, and "Recorded by" is stored nowhere.** `ApplyRectificationCommand` takes TargetType, a Guid TargetId and JSON values (`A/Commands/ApplyRectification.cs:21–27, :32–34`), so a free-text "Record" cannot be saved. `DataRightsRectification` keeps `AppliedOn` but no actor. Choose the record from a short fixed list (B's), or mark a command change PROPOSED; mark the recorder DATA. The guide's "Account (name, email): on My account" is wrong for email, which no page changes (`W/Account/Profile.razor:53–54`). | M |
| M6 | Steps | **Missing Expects.** The new correction step (Q2's A.1.8a, played end to end by Mr Smit) has no row. A.2.4 and A.2.6 follow A.2.3/A.2.5 (brief § 7) and are not listed. The appendix's outcome SQL ("Dlamini opted out of optional processing only", `App:1746`) changes with T318 and with A.1.1's new Do (Q28). List each with its new Expect, or "unchanged". | M |
| S1 | Queue V1/V2 1280 and 390 (A.1.12 rows), Decide V1/V2 (A.1.12) | **devadmin is drawn under the Coordinator's menu.** The Administrator's is grouped: Platform holds Data rights requests (`Nav/NavItems.cs:144`). Draw his own shell for his states. | S |
| S2 | Decide-V1-390, Decide-V2-390, Queue-V1-390, Queue-V2-390 | **Inline 390 styles that fight the system.** The dialog's and Decide's `flex-direction:column`, with the DOM reordered, put the safe button or Approve first on screen, against `.dialog-card .form-actions` and `.user-page .form-actions` (column-reverse, the action on top, the safe button first in the DOM; bundle.css:6180–6189, :6513–6516). The `min-height:2.75rem` on buttons, links and radios duplicates the system's 44 px rules (`.clinic-table--stack-wide .btn`, `.pager .btn`, `.user-page .btn`, `.user-page .choice-group .form-check label`). Drop them. | S |
| S3 | Spec (Badges), Queue V1 A.1.8 caption | **"Withdrawn moves from red to neutral"** is false: Withdrawn is grey today (`BadgeFor.DataRightsRequest`'s default, `badge-draft`; DESIGN.md:1952). The change is Approved from green to amber, plus Withdrawn's class renamed to flow 13's `.badge-withdrawn` (Q22). | S |
| S4 | Queue V1 (all) | **The requester cell is the row's name:** make it `th scope="row"` (RowNames). | S |
| S5 | Queue V1/V2, Decide V1, Main, WF-Request-* | **Placeholders the runbook fills:** "[address]" is mahlangu@, duplessis@ and ndlovu@kgk.wombat.local (README § Cast); du Plessis's request id starts 01a17f63-7af4- (`App` A.1.8 Actual); Mahlangu's rejection is 00:03 SAST. The illustrative correction's dates and note stay brackets (no step plays it yet). | S |
| S6 | WF-Request-A | **Complete's refusal names no field.** It belongs in the Correction section's `ActionResult`, focused (DESIGN.md:1854), not as a line under the button. | S |
| S7 | Decide V1 | **Two sentences the code does not back.** "You cannot download it yourself" is false for the Administrator (`DemandExportAccess`). "‹name› withdrew this request on ‹time›" needs a time `Withdraw()` does not keep: drop the time, or add `WithdrawnOn` (DATA). | S |
| S8 | Spec (DATA) | **DATA reads missing from the table:** the institution's name on My data rights ("Staff at ‹institution› decide it"); the erasure result's counts (the executor returns none); the recorder of a correction; the decider's name for "Decided by". **Drop from the table:** T276's residue in the erasure forecast (Q3 keeps it out of the dialog). | S |
| S9 | Spec (PROPOSED) | **Drawn but not marked PROPOSED:** the reason's live counter; Withdraw narrowed to Submitted (Q14's domain change, `DataRightsRequest.cs:115–121`); the kept request's info alert; the per-type help on Decide; Approved amber; the Administrator's Request id column. | S |
| S10 | NotFound-Kept-1280, -390 | **The kept request's trail** drops Data rights requests at 1280; at 390 its crumb is an empty link with unclosed markup. Say where its info alert sits. | S |
| S11 | Decide V1 (dialog) | **An erasure of someone with a staff role** also removes the roles and pseudonymises panel seats, attendance and the campaigns created (`I/ErasureExecutor.cs:234–253, :278–299`). The dialog counts only a trainee's reviews, campaigns and profile. Draw that case in round 2, with a line naming the roles that go (DATA). The pseudonymised panel seat is a leftover like T276's: note it, out of this flow. | S |
| S12 | Main (A.1.1) | **Draw A at fidelity** with each requester's real menu (Trainee's eight, the former trainee's four, the Coordinator's), and the states not drawn (§ 1). | S |
| N1 | Main | "0 of 4,000 characters": mark PROPOSED (a live count); the counter pattern is T366's. | N |
| N2 | WF-Request-A, -B (intros) | Their counts leave out the menu link; the Steps board includes it. Use one rule. | N |
| N3 | Queue-V1-390 | Clear filters missing, though the caption names it; the pager's page-size select dropped. | N |
| N4 | Queue V1/V2 | `.pv-id` is the Spec's NEW `.request-id`; `.pv-muted` is the system's `.muted`. | N |
| N5 | Decide V1/V2-390 | The 390 crumb drawn as `breadcrumb-ancestor`; it is `breadcrumb-parent` that shows. | N |
| N6 | Steps | "Q1"/"Q2" for the queue's variations reads as the operator's Q1/Q2; name them V1/V2. | N |
| N7 | WF-Requester-C | "Download your export" on what may be an Access request. | N |

### Invented people or values

- **No invented person** (Obs). Every name, address, role and id is the cast's or the brief's.
- **Right against the runbook:**
  - D = 2026-10-28;
  - submitted 00:01, 00:03, 00:03 and 00:04 SAST (22:01, 22:03, 22:03, 22:04 UTC);
  - approved 00:01 SAST (22:01:54 UTC);
  - rejected at 22:03:39 UTC;
  - erased 00:04 SAST (22:04:50 UTC);
  - data-export-20261027-220208.zip;
  - `deleted_user_0ae1d64b`;
  - check-in #8 Scheduled, Paed Annual Review Panel;
  - curriculum 11.2;
  - the queue's order (Mahlangu, du Plessis, Dlamini; `App` A.1.8 Actual);
  - the ids' prefixes 01a17f61-, 01a17f63-8c25-, 01a17f64-, 43ad0cb7- and abd4ed66-.
- **Taken from the brief, not re-checked here:**
  - the full user ids (67083efa-…, 4e5ce9ca-…, Smit's 43ad0cb7-5ce6-…);
  - the full request ids. The Actuals print only prefixes.
  - "closed check-in #6".
- **Illustrative, as the brief allows:** Mahlangu's "Mahlngu" correction (Q25).

### PROPOSED items, with a recommendation each

From the Spec's list:

| Item | Recommend |
|---|---|
| Q1: the Coordinator and the Administrator record and complete; Speciality admin dropped | Accept (decided) |
| Routes `/account/data-rights/new`, `/admin/data-rights/{Id}/corrections/new` | Drop (B and B not picked) |
| "Record at least one correction before completing the request." | Accept |
| "Choose Approve or Reject." | Drop (V2) |
| "‹name› withdrew this request on ‹time›. Nothing was decided or erased." | Accept without the time (S7) |
| PseudonymSalt's reworded refusal | Accept |
| The erasure ConfirmDialog (T264) | Accept; V2's standing section drop |
| Q5's decision notice | Accept (decided; M3) |
| Q6, Q8 | Accept (decided) |
| Q11, the pseudonym with "Erased ‹date›" | Accept, the date from `DataRightsErasureRecord.ErasedOn` |
| Queue V2's Status filter gone, Approved in Waiting | Drop (V2) |
| The Administrator's Institution filter and column | Q23 |
| "Reminder emails" and its results; C's checkbox words | Q24 |
| Q14, Q15 unchanged | Accept (decided) |
| A staff member may not decide his own request (Spec's edge) | Q21 |

Add the unmarked ones in S9.

### DATA items, with a recommendation each

| Page | Item | Recommend |
|---|---|---|
| My data rights | DecidedOn and DecisionNote on each row (T319), never DecidedBy | Accept: widen `DataRightsRequestSummaryDto` for the requester's query only |
| My data rights | A completed Rectification's records corrected | Accept |
| My data rights | The institution's name in "Staff at ‹institution› decide it" (unmarked) | Accept |
| Queue | The requester's name as stored (today the address) | Accept, through `GetDisplayNamesAsync` |
| Queue | The requester's institution's name, for the Administrator | Q23 |
| Queue, request | Erased and the pseudonym (Q11) | Accept as a rewrite of `RequesterDisplayName` at erasure, plus `DataRightsErasureRecord` for the date |
| Request | The decider's name beside the id | Accept |
| Request | The requester's name and institution | Accept |
| Request | Corrections recorded with by whom and when | Accept, adding a recorder column (M5) |
| Request | A fixed list of correctable records, each with its owning page and whether the caller may open it | Accept, short (Q26) |
| Erasure dialog | Forecast: open reviews (type, #, panel, state) at any institution or panel; unreleased campaigns about the person; an active profile and its curriculum | Accept, a query gated as the command |
| Erasure dialog | Forecast: T276's residue | Drop (Q3) |
| Erasure result | What the executor did, counted (unmarked) | Accept: `ExecuteAsync` returns its counts |
| All three pages | SAST times with the zone (T325) | Accept |
| Withdrawn rows and refusal | A withdrawal time (unmarked) | Drop the time (S7) |

### Access control and anonymity

| # | Board(s) | Finding | Sev |
|---|---|---|---|
| A1 | Decide V1 | **A reviewer may decide his own request today** (Obs). `CanAct` checks only the role and the request's institution, never that the caller is not the requester (`A/DataRightsAuthorization.cs:123–142`). So Mr Smit can approve his own export, and his own **erasure**, and the Administrator his own. The Spec's edge proposes a refusal (Q21). | — |
| A2 | Decide V1 | **The download stays the subject's or a global Administrator's** (`DemandExportAccess`). V1's help sentence must not tell the Administrator otherwise (S7). | — |
| A3 | Queue V1, NotFound | **Scope holds:** a Coordinator lists his institution's only, and a request with no institution is the Administrator's alone (`CanAct`, null stamp). The not-found page must answer an unknown and a foreign id with one page, as the board's does, under M1's words. | — |
| A4 | Main | **The requester's row never names the decider.** Q5's mail must not either (M3). | — |

### Accessibility and the 390 boards

- **Good (Obs):**
  - one h1 and `<h2>` sections in order on every page (T280);
  - every table captioned, the captions not repeating the headings;
  - Review, Withdraw and Download named for the row, with the SAST time;
  - "A decision note is required." under its field with `aria-invalid` and `aria-describedby`, the note kept;
  - the dialog's safe button first in the DOM and focused, `aria-haspopup="dialog"` on its trigger, the focus back on
    the trigger after Cancel;
  - loading announced (`role="status"`) with nothing offered;
  - status in words beside every badge;
  - ids and a 60-character address wrapping (`dd { overflow-wrap: anywhere }`, `<wbr>` at the @).
- **S2:** the inline 390 styles (above).
- **Should, round 2:**
  - the radios' 44 px rows on My data rights (scope: the requester's page has none of `.user-page`'s rules; name one
    in the Spec);
  - the dialog's own scroll at 390 with a long count list (the Spec's "not yet checked");
  - the muted second line's contrast on a hovered row;
  - 768 for the queue and the request.

### DESIGN.md and flows 01–13's patterns

The Spec names the rules each variation changes:
- § The NavMenu's owner table;
- § Form page;
- § Detail page and the title rule;
- § Table system;
- § Button system's destructive action;
- § Alerts;
- § Badges;
- § Content's ids rule.

Round 2 must also say:
- **The request page borrows flow 12's `.user-page` and `.user-section`.** Its 44 px and stacked actions at 390 come
  from them. Name the reuse, as flow 13's S8 did for the profile.
- **`.badge-withdrawn`'s comment** reads "a programme ended without completing it": widen it to a request withdrawn by
  its requester (Q22).
- **The Unavailable page** for the request (M1, Q17).
- **The requester's page's 390 scope** (above).

The CSS contract (Obs):
- **Colours:** no hex on the fidelity boards outside `tokens.css`.
- **No Bootstrap classes.**
- **Every class is defined** in the bundle, or is the board's own `.pv-*`.
- **What the build would wrongly copy:** the inline 390 styles (S2) and the skeletons' inline sizes (use `Skeleton`'s
  parameters).

## 8. The operator's decisions still needed

Q1–Q16 are assumed taken as recommended (confirm). The picks above follow them. Nothing in them is reopened except where
a board argues against one on a system rule or the code (Q17, Q18, Q19, Q24, Q27). Each question below has a
recommendation.

17. **Q17 (reopens Q13's words). The not-found page.** Q13 said "Request not found" / "There is no data rights request
    with this id that you can open."; the board draws flow 01's Page not found. *Recommend:* DESIGN.md's Unavailable
    pattern, as flows 12 and 13 did: h1 "Request unavailable", "This data rights request does not exist, or you cannot
    open it.", Go to Data rights requests, Data rights requests lit. One page for every cause; A.5.7's Expect changes.
18. **Q18 (reopens Q10's pager size). The queue's page size.** Q10 said 50. PagerControls defaults to 20, and flows 08,
    12 and 13 took 20; the boards draw 20. *Recommend:* 20.
19. **Q19 (reopens Q3's words). The erasure dialog's title and buttons.** Q3: "Erase ‹address›'s personal data?",
    confirm "Erase and approve". Drawn: "Erase Sipho Ndlovu's personal data?", confirm "Erase Sipho Ndlovu", cancel "Keep
    his data". *Recommend:* the title by name, since people are named and the address is the queue's id (§ Content).
    Confirm "Erase and approve", since it says both what it does and that it decides the request; "Erase Sipho Ndlovu"
    reads as erasing the person. Cancel "Cancel" (M4: no pronoun).
20. **Q20 (new). Access and Export: two types or one.** Both build the same zip (`AccessReportBuilder.BuildAsync(userId)`
    takes no type; `DownloadAccessReport` admits either). *Recommend:* keep two, with A's honest help ("The same
    download, to take your data elsewhere"). The record keeps which right the person invoked, and merging changes the
    enum, A.1.2 and A.1.6 for no gain on a rare page. *Inf:* whether POPIA distinguishes the two needs the College's or
    a lawyer's confirmation.
21. **Q21 (new). A reviewer deciding his own request.** Allowed today (A1), including his own erasure. *Recommend:*
    refuse it (PROPOSED). `CanReview` is false for the requester, so his own request shows in his queue with no Decide,
    and the page says "You cannot decide your own request: another Coordinator at Kgosi Kgari Teaching Hospital, or the
    Wombat administrator, decides it." The Administrator's own needs another Administrator. A new test pins it.
22. **Q22 (new). The badge tints.** Proposed: Approved amber (`badge-accepted`, "taken on and in hand"), and Withdrawn
    under flow 13's `.badge-withdrawn`. *Recommend:* both. Only a Rectification stays Approved (Access, Export and
    Erasure complete at once, `ApproveDataRightsRequest.cs:83–110`), and it is in hand until completed. Withdrawn is
    already grey: this is flow 13's F10 rule (grey under its own name), not a tint change. DESIGN.md's table row and
    `BadgeForStatusTableTests` change.
23. **Q23 (new). The Administrator's Institution filter and column.** *Recommend:* the column only, for the
    Administrator, with "None" for a request with no institution. At a few requests a year per institution, Type and
    Status narrow enough. Add the filter if the operator expects many institutions.
24. **Q24 (reopens Q4's heading). "Reminder emails".** Q4 named the section "Email preferences". Drawn: "Reminder
    emails", with the results "Reminder emails stopped." / "Reminder emails will be sent again." (A) and C's checkbox
    "Stop the regular reminder emails". *Recommend:* "Reminder emails", since after T318 it holds nothing else. Use A's
    two results, which replace "Processing preferences saved." and say what happened. The checkbox becomes "Stop the
    regular reminder emails", so the heading, the box and the results use one word. A.2.3 and A.2.5's Do and Expect
    change.
25. **Q25 (new). A correction to a name, which no staff page makes.** Only the person changes a name (My account); no
    one in Wombat changes an email. *Recommend:* no staff name edit in this flow. The Correction section says who
    changes each record ("Nomsa Mahlangu changes a name on My account"), and the staff member records it once made. The
    new step A.1.8a plays it: Mr Smit approves, Dr Mahlangu corrects her last name, Mr Smit records it and completes.
    If staff should correct names, propose it as its own task.
26. **Q26 (new, DATA). How a correction names its record.** *Recommend:* a short fixed list as radios in A's section:
    Account › First name, Account › Last name, Trainee profile › Programme start, Other. Each maps to the command's
    TargetType and TargetId (the requester's user id or profile), with the values stored as JSON strings. Add a
    recorder column.
27. **Q27 (reopens Q7's list). The digest help's one-off mail.** Q7 listed a request to assess and a password reset,
    neither of which Wombat sends (T320; `W/Account/ForgotPassword.razor:10`). *Recommend:* "Email about one particular
    thing is still sent, such as an invitation to give feedback or a coordinator's reminder about one activity."
    The build checks the list against `IEmailSender`'s and `AccountNotices`' callers. T320's mails change it again when
    they land.
28. **Q28 (new). A.1.1's Do once the optional box goes.** Drawn: Dr Dlamini ticks the digest opt-out. *Recommend:* A.1.1
    reads the page and saves nothing. Mr Smit's A.2.3/A.2.5 already play the opt-out and back, and a trainee left opted
    out changes the appendix's outcome check (`App:1746`), and *(Inf, not checked)* could change later nudge counts.

## 9. What round 2's ask must require

- **R1. The Spec and Steps boards.**
  - **The Spec board:**
    - every NEW and changed component with its classes: `.request-id`, the Correction section and its record radios,
      the Unavailable page for a request, the kept-request notice, and the reused `.user-page`/`.user-section`;
    - the requester's page's 390 scope;
    - the DESIGN.md text each changes;
    - the tokens;
    - every DATA read in § 7;
    - every message by kind, the built words and the NEW ones marked;
    - every PROPOSED item, S9's included.
  - **The Steps board:**
    - each of the 19 Expects with its new words, plus A.1.8a, A.2.4, A.2.6 and the appendix's outcome check (M6);
    - each journey's count at 1280 and 390: an export requested and downloaded; a rejected answer read; a request
      decided; a correction carried through; an erasure approved; the digest opted out of.
- **R2. The picks at fidelity:**
  - My data rights A;
  - the request A, with B's record list;
  - Queue V1;
  - Decide V1;
  - the kept request as corrected;
  - not found as Q17 decides.
- **R3. The states not yet drawn** (§ 1), each at 1280 and 390, with 768 for the queue and the request. Draw each
  requester's real menu and the Administrator's grouped menu.
- **R4. The corrections:**
  - M1–M6 (must);
  - S1–S12 (should);
  - the nits in one pass.
- **R5. Real values:**
  - D = 2026-10-28;
  - submitted 00:01 / 00:03 / 00:03 / 00:04 SAST;
  - approved 00:01, rejected 00:03, erased 00:04 SAST;
  - the cast's addresses;
  - du Plessis's request id prefix 01a17f63-7af4-;
  - `deleted_user_0ae1d64b`;
  - check-in #8;
  - curriculum 11.2;
  - data-export-20261027-220208.zip.
  - The illustrative correction stays bracketed until A.1.8a is played.
- **R6. Q1–Q16 drawn as decided, and Q17–Q28 as the operator decides.** Nothing of flows 01–13 is drawn as changed:
  the shell, Page not found for the download, session ended, Users, Trainees, Committee reviews and the review page.
- **R7. The attachments.** Re-attach § 4's twelve key screenshots and name the folder (`design/baseline/`). Ask round 2
  to say on the Spec board whether they arrived: round 1 says nothing (§ 11, flow 13). Name in the ask the values only a
  capture holds, so their brackets are expected.
