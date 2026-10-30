# F14 A person exercises data rights, and staff decide them

**Who and why:** anyone signed in may ask Wombat for access to her data, an export of it, a correction or erasure, and
may opt out of the digest emails; a Coordinator (his institution's requests) or the Administrator (every institution's)
decides each request with a note. These are legal rights (POPIA, CUSTOMIZATION.md § 5), and an erasure cannot be undone,
so each page must say exactly what happened and what will. Dr Anele Dlamini exports her record, Dr Pieter du Plessis
(a former trainee since flow 13) asks for access and withdraws it, Dr Nomsa Mahlangu asks for a correction that is
rejected, Dr Sipho Ndlovu asks to be erased and `devadmin` approves it; Prof Mbatha reads what the erasure left. Pages:
My data rights (the requester's preferences, request form and requests), Data rights requests (the queue), a request's
page (decide, and the correction that no page records today), the download endpoint, and the erasure's aftermath on
pages other flows own. None of the three data-rights pages has been redesigned: flow 01 gave them the shell, the
" · Wombat" titles, the heading "My data rights" and the request's crumb; flow 06 the digest help's day count; nothing
else since 2026-09-26. The step captures are from flow 13's replay (2026-10-28), on the shell as built; the
`states/data-rights-request--*` and `states/data-rights-requests--*` captures are from 2026-09-26, on the old frame.

Restated 2026-10-30 for T387 (the brief of 2026-09-26 predates flows 01–13: its CONTENT quoted the T295 replay's ids,
times and pseudonym, its aftermath had the erased registrar missing from Trainees, which flow 13 changed, and its layout
line described the pre-shell frame). Paths to screenshots are under `design/baseline/` (gitignored). Code is cited at
`bcf65236`; `W/` is `src/Wombat.Web/Components/Pages/`, `DR/` is `W/Admin/DataRights/`, `A/` is
`src/Wombat.Application/Features/DataRights/`, `I/` is `src/Wombat.Infrastructure/DataRights/`, `Nav/` is
`src/Wombat.Web/Navigation/`.

| | |
|---|---|
| Decision | **Restructure, with UX in scope** (W-008; BRIEF.md § 4), inside flow 01's shell. Marked **F/W** in BRIEF.md § 8: the queue and a request's decision exist and are expected to change in their parts; applying a correction (BRIEF § 7 B3, T316) does not exist at all, and the requester's page must now carry each decision's answer (T319), which changes its shape. |
| Frequency and stakes | Rare: a few requests a year per institution; the digest opt-out by anyone at any time. High: legal rights with statutory deadlines (inferred: POPIA's), and an approved erasure is irreversible and ends the person's account, open reviews and campaigns in one transaction. |
| Mode | **Restated by BRIEF.md § 2.3 step 4 and § 2.4** (the old row's "straight to fidelity … page shapes hold: BRIEF § 4" cites the pre-W-008 recommendation; read as F). **Wireframe first** (2–3 STRUCTURES each) for the requester's page (Q4: preferences and requests on one page or two, in which order, and where a decided request shows its answer, Q5) and for a request's page with the correction that carries an approved Rectification through (Q1, Q2: who, where it is recorded, how the data is changed on its owning page, and Complete). **Straight to fidelity** (2–3 variations) for the queue (Q8, Q10: labels, filters before Show, the pager, the requester named) and for a request's decision with the erasure's confirmation (Q3, Q12: what the dialog counts and keeps, where the refusal and the result show). **One proposal at fidelity** each, to confirm or correct, for not found (Q13) and for the erasure's aftermath as this flow touches it (the kept request, Q11; the other pages are their flows'). Round 2: the picks with every state. Then a review round from the boards' text, the operator's decisions, and one correction round. |
| Viewports | Desktop 1280×800 and phone 390×844; the build is also checked at 768 (§ 11, flow 06) |
| Variations | 2–3 structures (wireframes) for the requester's page and for a request's page with its correction; 2–3 variations (fidelity) for the queue and for the decision with the erasure dialog; one proposal each for not found and the kept request after an erasure; the chosen ones in round 2 |
| People | Trainees: Dr Anele Dlamini (KGK, Paediatrics, training year 3; an Export), Dr Nomsa Mahlangu (year 1, started 2026-01-15; a Rectification, rejected), Dr Sipho Ndlovu (year 1, curriculum 11.2 since Act 6; an Erasure, approved; becomes `deleted_user_0ae1d64b` in the T379 replay). Former trainee: Dr Pieter du Plessis (no role, a trainee record, D54; an Access, withdrawn). Coordinator: Mr Pieter Smit (KGK; decides KGK's requests; opts out of and back into the digest, A.2.3, A.2.5). Administrator: `devadmin` ("Demo Administrator"; every institution's requests; approves the erasure). InstitutionalAdmin: Prof Nolwazi Mbatha (KGK; reads the aftermath, A.1.14). `D` is the replay day (2026-10-28 in the T379 replay, whose appendix crossed SAST midnight) |
| Runbook steps | 19, pasted verbatim in § 8: A.1.1–A.1.14, A.2.3, A.2.5, A.5.7, A.7.3 and A.7.5 (appendix). A.1.10 is flow 08's Schedule a review, played to give the erasure an open review to end; A.1.13 crosses flow 02's session-ended sign-in; A.1.4 crosses flow 01's not found; A.1.14 reads flows 12's Users, 13's Trainees and 07/08's review pages; A.7.3 and A.7.5 cross flows 03, 05, 06, 08 and 10's pages at 390 px |
| Pages (`coverage.md` templates) | `/account/data-rights` (`W/Profile/DataRights.razor`, 314 lines, a bare `[Authorize]` (`:2`): every signed-in person; interactive), `/admin/data-rights` (`DR/RequestsList.razor`, 146, `[Authorize(Roles = "Administrator,Coordinator")]` (`:2`)), `/admin/data-rights/{Id:guid}` (`DR/RequestDetail.razor`, 194, the same roles (`:2`)); the endpoint `/account/data-rights/download/{id:guid}` (`src/Wombat.Web/Program.cs:900–920`, `RequireAuthorization`, `DownloadAccessReportQuery`; 404 for anyone but the subject and a global Administrator, `A/DataRightsAuthorization.cs:97–106`, and for a request that is not a Completed Access or Export, `A/Queries/DownloadAccessReport.cs:44–48`); and, as the erasure's aftermath, `/account/session-ended` (`src/Wombat.Web/Security/SessionEnd.cs:37`, `:56–88`: to `/account/login?error=SessionEnded`), `/account/login` (flow 02's), `/not-found` (flow 01's; A.1.4), `/admin/users` (flow 12's `W/Admin/Users/UsersList.razor`), `/admin/trainees` (flow 11/13's `W/Admin/Trainees/PendingTraineesList.razor`: the erased profile listed under the pseudonym in Completed and closed, `src/Wombat.Application/Features/Trainees/ListEndedProfiles.cs:100–102`), `/committee/reviews` (flow 08's) and `/committee/reviews/{ReviewId:int}` (flows 07 and 09's). Since 2026-09-26: My data rights `b347e11c` (flow 01: the title and the heading "My data rights", was "Your data rights") and `85d5a508` (flow 06: the digest help's day count from `DashboardThresholds.AssessorNudgeDays`); the queue and a request `b347e11c` only (titles; the request's crumb is its id once loaded, `DR/RequestDetail.razor:10`). `A/`, the domain and `I/ObjectionFlagService.cs` unchanged since `97700425` (T258, 2026-09-25); `I/ErasureExecutor.cs` changed only in what it pseudonymises (`b347e11c` ActingRole, `85d5a508` reminders, `68327c9d` MSF `ClosedByUserId`, `c9d6fe13` T312's ended list). |
| Menus (`Nav/NavItems.cs`) | My data rights (`/account/data-rights`, icon `lock`, `:133`) is a personal link under the rule for **everyone signed in** (`PersonalFor`, `:218–227`): a Trainee's are My progress and My data rights, a former trainee's My progress, Export portfolio and My data rights (T379), everyone else's My data rights alone. Data rights requests (`/admin/data-rights`, icon `shield-check`, `:129`) is in the Administrator's "Platform" group (`:144`) and the Coordinator's flat list (`:176`). A request's page lights Data rights requests for both (`Nav/NavOwners.cs:139`); its trail is Home › Data rights requests › ‹the request's id› (observed A.1.3), with "Back to list" beside it (`DR/RequestDetail.razor:12–15`), which flows 12 and 13 dropped on their record pages. |
| Components drawn at runtime | `PageHeader` (My data rights: "My data rights" / "Request access to your data, correct it, export it, manage processing preferences, or request erasure.", `W/Profile/DataRights.razor:9`; the queue: "Data rights requests" / "Review and act on data subject rights requests." with **Apply filters as its header action, before the filters** (`DR/RequestsList.razor:8–14`); a request: "Data rights request" / "Review and act on this request." with Back to list (`DR/RequestDetail.razor:10–15`)), `ActionResult` with danger and success `Alert`s at each page's top (every result and refusal; `FocusAfterRender` after a success only), `.details-grid` of `detail-card`s with **`<h3>`** titles and no `<h2>` (My data rights: Processing preferences, Submit a request, Your requests, `:28`, `:64`, `:93`; a request: Request, Decision, Action, `DR/RequestDetail.razor:39–88`), `FormField` (a checkbox each for "Opt out of optional processing (e.g. analytics)" and "Opt out of digest emails" with its help; Request type, a select whose glosses truncate at 1280, observed A.1.9; Reason, a textarea; Decision note), `FormActions`, `InFlight` ("Saving...", "Submitting...", "Processing..."), `StatePanel` (My data rights' preferences and requests; the queue with `LoadError`; a request with no `LoadError`, so a failed load shows the exception's text over "Not found"), `DataTable` (Your requests: Date, Type, Status, Actions; the queue: Date, Requester, Type, Status, Review), a hand-built `.pager` at 25 a page (`DR/RequestsList.razor:76–93`), the `.search-container` filter card (Type, Status: "All" plus the raw enum names, Objection and UnderReview included, `:22–39`), status badges by `BadgeFor.DataRightsRequest` (`W/../Shared/BadgeFor.cs:153–160`) printing the enum name. No `ConfirmDialog`. Times are the stored UTC with no zone (`yyyy-MM-dd HH:mm` in lists, `HH:mm:ss` on a request), in `<code>`. Requests: `GetObjectionFlagsQuery`/`UpdateObjectionFlagsCommand`, `SubmitDataRightsRequestCommand`, `GetMyDataRightsRequestsQuery`, `WithdrawDataRightsRequestCommand`, `ListDataRightsRequestsQuery`, `GetDataRightsRequestByIdQuery`, `ApproveDataRightsRequestCommand` (an Erasure runs `ErasureExecutor` in the same transaction; Access, Export and Objection complete at once; a Rectification stays Approved, `A/Commands/ApproveDataRightsRequest.cs:85–110`), `RejectDataRightsRequestCommand`; `ApplyRectificationCommand` and `CompleteRectificationRequestCommand` exist with no caller (T316). |
| Held | None. Every step capture is from the T379 replay (2026-10-28), on the shell as built. `states/data-rights--*` (2026-09-28) are on the shell. `states/data-rights-requests--*` and `states/data-rights-request--*` (2026-09-26) are on the old frame (a Title Case sidebar with Logout, an email in the top bar: checked by opening `data-rights-requests--coordinator`); read their bodies only. `states/not-found--download` is from 2026-09-27; `review-detail--withdrawn` (2026-10-12), `reviews-schedule--pseudonym` (2026-10-13), `users-list--after-erasure` (2026-10-27) and `pending-trainees-list--erased` (2026-10-28) are on the shell |
| Depends on | Flow 01, built (`b347e11c`): the shell, the personal links, access denied, not found, `ActionResult`, `ConfirmDialog`, `StatePanel` with Try again. Flow 02 (`f50dffb2`): session ended and the sign-in refusal (A.1.13). Flow 06 (`85d5a508`): filters applied with Show. Flow 08 (`622c5ad1`): Schedule a review (A.1.10), `PagerControls` with the focus on the list's heading. Flow 11 (`69a17c70`): refusals under their field, focused; the required mark; `StackWide`. Flow 12 (`e53d4ae7`): a detail page in one column of `<h2>` sections with no details-grid; a destructive action confirmed, its trigger an outline button; Users after an erasure ("People 14", A.1.14). Flow 13 (`5cc10c66`, `920f778c`): the former trainee's menu with Export portfolio (A.1.6); Trainees' Completed and closed listing the erased profile under the pseudonym (A.1.14, F7); no export offered for a trainee with no account. The design system is **version 37**, re-synced to flow 13 as built |

**How to run this thread** (BRIEF.md § 2.3 step 4; § 11):
1. **Stage the upload set:** `powershell -File design/tools/stage_upload.ps1 -Flow 14`. Open each screenshot. The
   script puts any capture whose name contains "invit", "issued", "resent", "resend" or "reset" in
   `design/upload/crop-first/`; § 4 names none. No capture here shows a password; the request captures show user ids,
   request ids and scenario addresses, which are the pages' own record (BRIEF § 6 A8) and go as they are.
2. **Start a new canvas** from the main app's Design page, "Wombat 14", on the Wombat design system v37
   (https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18). Not an earlier flow's canvas. Write its `tokens.css` by hand
   from app.css's `:root` before round 1 (§ 11, flow 09: the system serves only `tokens.json`).
3. **Copy the mark into the canvas before round 1** (`Artifact publish`, `asset: true`, `from_url` = the design
   system, the mark's asset id as flow 13 copied it), and write its `/_blob/` URL into § 1 and
   `design/flows/14-data-rights/round-1-ask.txt` in place of `<MARK>`.
4. **Check on the canvas that the attachments arrive** before round 1 (§ 11, flow 13: the canvas never saw them). Then
   **send § 1 and § 8 as ONE message** (`design/flows/14-data-rights/round-1-ask.txt`), with the key screenshots in
   § 4. Read `project/canvas.json`'s version before and after: a send can draw nothing. Say again that every person,
   address, id and value comes from § 1 and § 8.
5. **Answer § 6's operator questions first** (Q1, Q2 and Q5–Q11, Q14–Q16 are the operator's): Q1 decides screen 3.
   A question whose answer rests on an open task names it, and the task lands before the build when it changes data or
   rules (§ 11, flow 05). Then pick a structure for the requester's page and for a request's page with its correction,
   a variation for the queue and the decision, and confirm or correct the two proposals; say why in a sentence. Ask for
   round 2 at fidelity, with its Spec and Steps boards.
6. **Review round 2 from the boards' text** (`Artifact read`), as `design/flows/13-graduation-and-exit/round-2-review.md`
   did: one reviewer on the code (and the ask), one on the cast and the runbook, one on the design system; the check
   reads the boards' stacked rows against DESIGN.md as well as the ask (§ 11, flow 11). Put the decisions to the
   operator, then ask for one correction round.
7. **Record every round** in `design/flows/14-data-rights/` before moving on.

---

## 1. The ask (paste this first, with § 8 in the same message)

```text
FLOW 14 — I ask Wombat for my data, a correction or erasure, and read the answer; staff decide my request and carry it through

This is flow 14 of a RESTRUCTURE with UX in scope (design/BRIEF.md § 4). Flows 01 (the shell), 02 (sign-in and
  account), 03 (a registrar files an activity), 04 (an assessor's inbox), 05 (a registrar reads where she stands),
  06 (watching the programme), 07 (the review sitting), 08 (panels, what is due, the schedule), 09 (STARs, the
  register, appeals), 10 (MSF campaigns), 11 (invitation to admission), 12 (the institution's people) and 13
  (graduation and exit) are designed and built; every page here sits inside the shell. Round 1: 2–3 STRUCTURES as
  wireframes for My data rights (the requester's page), and 2–3 for a request's page together with the correction that
  carries an approved Rectification through; 2–3 variations at FULL FIDELITY each for the queue (Data rights
  requests) and for deciding a request, with the erasure's confirmation; ONE proposal at fidelity each for not found
  and for a request kept after its requester was erased, to confirm or correct. I pick before round 2.
  The runbook steps follow below in this same message. The brand mark is at <MARK>: use it, never a drawn disc.
  Every person, address, role, date, id and count comes from this message and the steps below; invent none.

GOAL:
  Anyone signed in (a registrar, a former trainee, staff) must be able to:
    - make a request (access, an export, a correction, erasure) with a reason, told what each type does and what
      happens next, and withdraw it while it waits;
    - read every request she made with its answer: the decision, its date and the institution's note, never who decided;
      download a completed export; know that an erasure, once approved, ends her account;
    - opt out of the digest emails (and back in), told exactly which mails stop and which are still sent.
  A Coordinator (his institution's requests) and the Administrator (every institution's) must be able to:
    - find a request by type and status, each named by its requester, newest first, the filters applied when asked;
    - read a request and decide it with a note, refused beside the note when it is missing;
    - for an approved correction: record each correction made (which record, the value before, the value after), reach
      the page that holds the data, and complete the request once at least one correction is recorded;
    - for an erasure: read, before confirming, what the erasure will end (the account and its sign-in, every open
      committee review, every unreleased MSF campaign, a still-active programme) and what it keeps, and that it cannot be
      undone; then confirm in a dialog;
    - be told "not found" for a request that does not exist or is not theirs, never that it exists.
  What is wrong today (the attached screenshots show each; on the shell as built, 2026-10-28):
  - An approved correction can never be carried through: Approve leaves a Rectification "Approved" for good; no page
    records a correction or completes it; the role allowed to record one (Speciality admin) cannot open the request, and
    the Coordinator who approves cannot open the trainee profile that holds a programme start.
  - The requester never reads an answer: "Your requests" shows Date, Type and Status only; Dr Mahlangu's reads
    "Rejected" with no date and no note (A.1.9). "Request submitted. You will be notified when it is reviewed." promises
    mail nothing sends.
  - "Opt out of optional processing (e.g. analytics)" saves a flag nothing reads. The digest help says "a committee or
    entrustment decision" is still emailed; none is sent.
  - The queue's filters offer code names ("UnderReview", which nothing ever sets) and "Objection", which no one can
    submit; badges and row names print code names too. "Apply filters" is the header action, so it comes before the
    filters in reading and tab order, and above them at 390 px (A.7.5). The pager is hand-built at 25 a page.
  - Approving an erasure asks nothing first and lists nothing it will end (A.1.12); the account, review #8 and every
    sign-in go in one press.
  - A request's page: two cards side by side; at 1280 the Request card is run past by its address and ids by 35 px
    (A.1.3); "Decided by" is a bare user id; every time is UTC with no zone ("2026-10-27 22:01" for 00:01 SAST on
    2026-10-28); "A decision note is required." shows at the page's top, the note unmarked; "Back to list" repeats the
    trail; an unknown or foreign id reads "You are not authorized to access this data-rights request." over "Not found"
    (A.5.7); a failed load prints the exception's text over "Not found".
  - My data rights' three parts are <h3> cards with no <h2>; the request type's glosses are cut off in the select
    ("Access — download everything we h…").
  - After an erasure the kept request still names the person by his address in the queue and on its page (T276).

AUDIENCE:
  - Requesters: Dr Anele Dlamini (Trainee), Dr Nomsa Mahlangu (Trainee), Dr Sipho Ndlovu (Trainee, erased), Dr Pieter
    du Plessis (former trainee: no role, a trainee record). Any user is a requester; Mr Smit uses the same page for the
    digest opt-out. Rarely; a legal right, read carefully.
  - Coordinator: Mr Pieter Smit, Kgosi Kgari Teaching Hospital (KGK). Decides KGK's requests.
  - Administrator: devadmin ("Demo Administrator"). Every institution's requests; approves the erasure.
  - InstitutionalAdmin: Prof Nolwazi Mbatha (KGK). Reads what the erasure left (A.1.14).
  - Frequency: a few requests a year per institution. Stakes: high; an erasure is irreversible. Desktop 1280×800 and
    phone 390×844.

THE RULES THE PAGES CARRY TODAY (real words; they are the data to draw with, and the rules hold whatever the shape):
  - My data rights (/account/data-rights; every signed-in person; "My data rights" / "Request access to your data,
    correct it, export it, manage processing preferences, or request erasure."):
    Processing preferences: "You may opt out of processing that is not required for your training programme. You cannot
    object to processing required for programme operation, assessment records, or regulatory compliance."; "Opt out of
    optional processing (e.g. analytics)" (nothing reads it); "Opt out of digest emails", help "Stops three regular
    reminders: the weekly coordinator digest, the reminder of drafts left untouched for 14 days, and the reminder of
    activities that have waited more than 5 days for you to assess. Email about one particular thing is still sent, such
    as a request to assess, an invitation to give feedback, or a committee or entrustment decision."; Save preferences →
    "Processing preferences saved." (focused).
    Submit a request: "Under data protection law, you have the right to access, export, correct, or request erasure of
    your personal data."; Request type* ("Access — download everything we hold about you", "Export — machine-readable
    data export", "Rectification — correct inaccurate data", "Erasure — request deletion of your data"); Reason*
    ("Describe what you need and why..."); Submit request → "Request submitted. You will be notified when it is
    reviewed." (focused); no reason: "Please provide a reason for your request." (nothing sent, the focus stays).
    Your requests: "No requests" / "You have not submitted any data rights requests yet."; Date, Type, Status; Withdraw
    ("Withdraw the Export request made 2026-10-27 22:01") while Submitted, at once, no dialog → "Request withdrawn."
    (focused); Download ("Download the export of the Export request made 2026-10-27 22:01") on a Completed Access or
    Export only; no decision, date or note on any row.
  - The download (/account/data-rights/download/{id}): the data subject or a global Administrator only; anyone else,
    the reviewer who approved it included, gets Page not found (404), naming nothing (A.1.4). data-export-<UTC
    yyyyMMdd-HHmmss>.zip (data-export-20261027-220208.zip) holding data-export.json (schema version 2: profile,
    activities, committee reviews, MSF campaigns, progress, newest 500 audit entries, portfolio exports) and
    portfolio-summary.pdf; built when downloaded, not when approved.
  - Data rights requests (/admin/data-rights; Administrator: every institution's; Coordinator: his institution's;
    "Data rights requests" / "Review and act on data subject rights requests."): Type and Status ("All" or a value),
    acting only on Apply filters; Date, Requester (the address), Type, Status badge, Review ("Review the Export request
    from dlamini@kgk.wombat.local, 2026-10-27 22:01"); newest first; "No requests" / "No data rights requests match the
    current filters."; "Showing 1–3 of 3".
  - A request (/admin/data-rights/{Id}; "Data rights request" / "Review and act on this request."; trail Home › Data
    rights requests › its id; Back to list): Request: Requester, User ID, Submitted, Type, Status, Reason. Decision
    (once decided): Decided by (a user id), Decided on, Note, Completed on. Action (while Submitted): Decision note*,
    Approve, Reject; no note: "A decision note is required."; "Request approved." / "Request rejected." (focused).
    Approve: an Export or Access reads Completed at once; an Erasure is approved, completed and run in one transaction;
    a Rectification stays Approved. Refusals the build has today: "PseudonymSalt is not configured. Cannot execute
    erasure without it." (the request stays Submitted); "Something about this person changed while they were being
    erased: a review, a feedback campaign or their account was changed elsewhere at the same moment. Nothing has been
    erased, and the request has not been approved. Approve it again."; unknown or another institution's id: "You are
    not authorized to access this data-rights request." over "Not found / The data rights request was not found."
  - Recording a correction (built in the Application, no page): ApplyRectification takes the record, the value before
    and the value after, only on an Approved Rectification ("The request must be approved before rectifications can be
    applied."); CompleteRectificationRequest refuses with none ("At least one rectification must be applied before
    completing the request."). Allowed today to a Speciality admin (and the Administrator), not the Coordinator.
  - What an approved erasure does (one transaction): the account becomes deleted_user_<8 hex> with no email, name,
    password, institution, role, scope or institutional sign-in, locked; its open session ends within a minute ("Your
    session has ended. Sign in again."), and signing in reads "Invalid email or password." as for any unknown address.
    Every open committee review of the person (Scheduled, In progress, Decided) is withdrawn: "Withdrawn because the
    trainee's personal data was erased at their request. Nothing more is decided at this review."; decisions staged at
    it are removed. Every MSF campaign about the person not yet released is withdrawn. A still-active trainee profile
    ends that day, as Withdrawn. Kept under the pseudonym: ratified reviews, a review under appeal (still answerable),
    issued STARs, released feedback reports, activities, progress and audit entries. The request keeps the requester's
    address. Not yet ended (T276): an activity about him still in its workflow can complete and credit the pseudonym;
    user ids inside activity data are not rewritten; his address as a respondent on someone else's open campaign stays.
  - The aftermath (other flows' pages; draw only what links to this flow): Users "People 14", no pseudonym (flow 12);
    Trainees' Completed and closed "3 ended", the erased profile first as deleted_user_0ae1d64b, Withdrawn 2026-10-28,
    "View deleted_user_0ae1d64b's record" (flow 13); Committee reviews' three deleted_user_0ae1d64b rows, the check-in
    "Withdrawn", "None: the review was withdrawn" (flow 08); the check-in's sitting bar "Withdrawn on 2026-10-28. …",
    no action (flow 07).
  - Data volumes: none (no request; nothing matches); typical (four requests, three at KGK); heavy (fifty across four
    institutions, a pager; a reason and a note of a long paragraph; an address of 60 characters; a request id and two
    user ids on one page at 390 px; a correction with five records).

CONTENT (real scenario values from the T379 replay; ids change on every replay, so draw them as given and let them wrap):
  - Dr Dlamini, Export, "A copy of my training record for my own files.", submitted 2026-10-28 00:01 SAST (stored
    2026-10-27 22:01:27 UTC), request 01a17f61-f16f-77c1-860c-661586d5b733, her user id
    67083efa-bbaa-48d5-a753-b01fdf38530d; approved by Mr Smit (43ad0cb7-5ce6-46d7-8595-7117d130c7dc) at 00:01:54 SAST
    with "Identity confirmed; export released to the data subject."; Completed at once; downloads
    data-export-20261027-220208.zip.
  - Dr du Plessis, Access, "Everything held about me.", 00:03 SAST, then withdrawn.
  - Dr Mahlangu, Rectification, "My programme start is recorded as 15 January 2026; I started work on 19 January.",
    00:03 SAST, request 01a17f63-8c25-7c6e-8724-8593f3d7a145; rejected by Mr Smit with "The whole January 2026 intake
    is admitted on 15 January, the semester boundary (D42). Nothing to correct."
  - Dr Ndlovu, Erasure, "I am leaving the programme and want my personal data removed.", 00:04 SAST, request
    01a17f64-f566-75f0-91ba-5d71863b77d0, his user id 4e5ce9ca-438b-4d98-8635-a7259638d7c9; approved by devadmin
    (abd4ed66-e9ad-4187-b87f-c3c481d05c30) at 00:04:50 SAST with "Identity confirmed. Programme records are kept under a
    pseudonym."; he becomes deleted_user_0ae1d64b; it ends formative check-in #8 (Scheduled) and his active profile, and
    no campaign (none was open); it keeps Act 4's ratified review, closed check-in #6, his activities and progress.
  - For the correction (illustrative, not in the story; a new step will play it, Q2): an approved Rectification from Dr
    Mahlangu whose account's last name reads "Mahlngu"; the correction: Account › Last name, before "Mahlngu", after
    "Mahlangu".

WHAT FLOW 14 MAY CHANGE OF WHAT EARLIER FLOWS BUILT, AND WHAT IT MAY NOT:
  May not: the shell (flow 01: the personal links, access denied, not found); session ended and the sign-in refusal
    (flow 02); Users, Trainees, Committee reviews and the review page (flows 12, 11/13, 08 and 07); the export's JSON
    and PDF (flow 20's); the rules above (the subject or a global Administrator downloads; the reviewer never does; an
    out-of-scope request is not found; an erasure is one transaction; no committee review refuses an erasure request,
    T258; the request keeps its record).
  May, for these pages: the requester's page's structure, its order and its words; how a decided request shows its
    answer; the queue's filters, columns, pager and words; a request's page structure, title and sections; where the
    correction is recorded and how it reaches the owning page; the decision's form, refusals and results; the erasure's
    confirmation; the not-found state; stacking at 390. Mark PROPOSED any rule change (a role newly allowed, a new
    route, a new action, a new refusal, a new email, data a page does not read today): those are mine. The runbook's
    Expects change with the words.
  Say which of these each variation changes, and why.

SCREENS, in order:
  1. My data rights (the requester): nothing requested (A.1.1); preferences saved; no reason (A.1.2); submitted, with
     Withdraw (A.1.2, A.1.7, A.1.11); withdrawn (A.1.6); completed with Download (A.1.5); rejected with its date and note
     (A.1.9, T319); approved and completed for an erasure (never seen by its requester: his session ends); a correction
     approved, then completed; the digest opted out (A.2.3) and back in (A.2.5), Mr Smit's; loading; a load error;
     390 px (A.7.3). For Dr Dlamini (Trainee), Dr du Plessis (former trainee) and Mr Smit (staff).
  2. Data rights requests (the queue): the Coordinator's, filtered to Export and Submitted (A.1.3); nothing matches
     (A.1.3); three, newest first (A.1.8); the Administrator's, every institution's, filtered to Erasure (A.1.12); after
     the erasure (A.1.12); fifty with a pager; loading; a load error; 390 px (A.7.5).
  3. A request: submitted, with its decision form (A.1.3); the note missing (A.1.3); approved and completed (A.1.3);
     withdrawn, offering nothing (A.1.8); rejected (A.1.8); a Rectification approved, then each correction recorded,
     Complete refused with none, completed (T316; new step); an Erasure before approval, its confirmation dialog, its
     cancel, approved (A.1.12); the two erasure refusals above; not found (A.5.7); loading; a load error; 390 px.
  4. The erasure's aftermath as this flow draws it: the kept request in the queue and on its page after the erasure
     (Q11); the requester's tab sent to sign-in and refused (A.1.13, flow 02's, for context only); Trainees, Users and
     the withdrawn review (A.1.14, their flows', for context only).

STEPS: A.1.1–A.1.14, A.2.3, A.2.5, A.5.7, A.7.3 and A.7.5 (19). Pasted verbatim below (Role / Route / Do / Expect). In
  them, D is the day the story is replayed. Where an Expect quotes today's wording or layout, that is today's page, not
  a requirement on the new one. A.1.10 is flow 08's (Schedule a review); A.1.13 is flow 02's; A.1.14's Users, Trainees
  and review pages are flows 12, 13, 08 and 07's; A.7.3 and A.7.5 cross other flows' pages at 390 (keep them; draw
  only what is on these pages).

STATES TO SHOW:
  Every state named in SCREENS; loading for every page (the header and a skeleton, no action offered until the read is
  in); a load error (a sentence and Try again, never an exception's text, and no empty state under it); each
  confirmation, its cancel, and the result after it with the focus on it; a refusal beside the field that caused it,
  keeping what was typed; 390 px for every page, ids and addresses wrapping, nothing sideways; the heavy volumes.

REQUIREMENTS FROM KNOWN DEFECTS:
  - T316: an approved Rectification shows, to whoever may carry it through (Q1), a correction part: the corrections
    recorded so far; a form recording one (the record, the value before, the value after); a link to the page that
    owns the data where the caller may open it, else who can (Q2); Complete request, refused until one correction is
    recorded ("Record at least one correction before completing the request.", PROPOSED words). Then the request and the
    requester's row read Completed.
  - T319: the requester's row shows, on a decided request, the decision, its date and the note (DATA: the summary
    carries neither today), never who decided; "You will be notified when it is reviewed." stays only if the decision
    notice is sent (Q5); an erasure's confirmation mail, if any, is read before the erasure runs.
  - T318: "Opt out of optional processing" goes (nothing reads it); the preferences part says what can be opted out of
    (the three digest reminders) and that processing the programme needs cannot be objected to. Each action shows one
    result: a refusal never stands beside an earlier "Processing preferences saved.".
  - T320 (the help's half): the digest help names only one-off mail that is sent.
  - T324 / A8: types and statuses by label ("Under review", not "UnderReview") on filters, badges and row names; the
    Type filter offers the four types the form offers, not Objection (Q8). Ids stay on the queue and a request (A8's
    exception), wrapping.
  - T264 / A5: approving an erasure asks first, in a ConfirmDialog naming the person and counting what it ends (open
    reviews, other institutions' and panels' included; unreleased campaigns; an active programme), saying what is
    kept and that it cannot be undone; its trigger an outline button, its confirm the danger button. The queue's pager
    keeps the focus (the list's heading, flow 08's).
  - T323 / A3: at 1280 an address, a request id and a user id wrap inside their section; at 390 nothing scrolls sideways.
  - T328 / A4: the filters come before Apply (or Show) in reading and tab order, and above it at 390; controls use the
    body font.
  - T325 / A9: every time in South African time with its zone, "2026-10-28 00:01 SAST" (today the stored UTC prints with
    no zone, and the day with it).
  - T329 / A6: a skeleton while loading, the alert on a load error with no empty state under it, no action before the
    record has loaded; no page prints an exception's text.
  - T280: one <h1>, then <h2> in order (today every section is an <h3> under the <h1>).
  - T276 (its request half, Q11): after an erasure the kept request names the pseudonym, not the address.
  - Not found (T112): an unknown id and another institution's read alike, one state with no "not authorized" alert.
  - Done since the old brief, keep them: T239 (every row action named for its row); T240 (the digest opt-out stops the
    three reminders, by one policy); T279 (an erased session ends within a minute, via session ended); T258 (erasure
    ends what is open and keeps what is settled; no review refuses an erasure request); T112 (institution scope; the
    reviewer never downloads); flow 13's Completed and closed under the pseudonym and no export for an erased trainee
    (`920f778c`).

QUESTIONS THE DESIGN MUST ANSWER:
  1. (Mine) Who carries an approved correction through: whoever may approve it (the Coordinator at the request's
     institution, and the Administrator), so that one person approves, records and completes?
  2. (Mine) The correction: recorded on the request (record, before, after) while the data itself is changed on its
     owning page; and when the approver cannot open that page (a Coordinator and the trainee profile), what the card
     says.
  3. How an erasure's finality is made unmistakable before approval: what the dialog counts, what it keeps, its words.
  4. The requester's page: preferences and requests on one page or two; in which order (her requests first?).
  5. (Mine) The decision notice (T319): an email with no note, linking to the page; the requester sees the date and note.
  6. (Mine) T318: the optional-processing flag and its column removed.
  7. (Mine) The digest help: name only the one-off mail sent today (T320's mails stay its own task).
  8. (Mine) Objection refused at submission; UnderReview dropped from the filter (nothing sets it).
  9. The times: SAST with the zone on all three pages, to the minute.
  10. The queue: filters then Show, the requester by name over the address, PagerControls, a caption.
  11. (Mine) After an erasure the kept request names the pseudonym (T276's request half folded in)?
  12. A request's page: its title, one column of <h2> sections (Request, Decision, Correction, Decide), the decider by
      name with the id, the note's refusal under the field, Back to list gone.
  13. Not found: one state for unknown and foreign ids.
  14. (Mine) Withdraw stays immediate, with no confirmation?
  15. (Mine) Who decides stays the Coordinator and the Administrator (T112's data-rights officer still open)?
  16. (Mine) What the build folds in.

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
  Since flow 02 also: the auth card, PasswordField with its Show toggle and PasswordRules. Since flow 03:
  RefusalSummary, DataTable's stacked rows (Stack). Since flow 05: ISO dates, the training year's wording, the ended
  record (R5). Since flow 06: the role Homes; filters applied with Show. Since flow 07: the container-query stack.
  Since flow 08: one action a row, a choice of a few kinds as radios (choice-group), a form page of its own, the
  pager's focus on the list's heading. Since flow 09: a page of its own per list row; a dialog with a field. Since flow
  10: a create page of its own reached from Home's header action. Since flow 11: the list is the page and its create
  form a page of its own; DataTable's StackWide (stacked from 900 px down); a role chosen from described radios; the
  required mark; only the chosen role's fields drawn; refusals under their field, focused. Since flow 12: a detail
  page in one column of <h2> sections with no details-grid (the user page); a form in a note panel opened in place
  (Add a role…); a destructive action confirmed, its trigger an outline button. Since flow 13: a record card naming
  an end and linking the record (FormerTraineeRecordCard); a page's header action named for the person; an Alert with
  its first sentence bold over a details list (Verify's answer); an end section of its own apart from Save.
Already designed by flow 01, reuse them: the active nav item, the acting-role switch and its result alert, access
  denied, not found, the error page, the reconnect dialog and the in-app error bar. Designed by flow 02: session ended.
  Still to design where a flow meets them: field validation (invalid border plus a stripe, not colour alone; message
  under the field; the summary).
Content: sentence case everywhere; people by name as stored (no titles); states and types by label; times in South
  African time with the zone shown; an out-of-scope record is "not found", never "forbidden".
Accessibility: WCAG 2.1 AA; text 4.5:1, control borders and focus ring 3:1 (the sidebar's white ring included); targets
  ≥ 24 px, 44 px on a phone; focus moves to an action's result; every page works at 390 px with no sideways scroll;
  reduced motion honoured.
Viewports: 1280×800 and 390×844.
Also: each page has exactly one h1, and its headings are in order. A confirmation is flow 01's native <dialog>
  ConfirmDialog. Ids stay visible on the queue and on a request (the id is the record there), wrapping, never hidden;
  never on the requester's own page. A date is ISO (YYYY-MM-DD) on every screen.

ASK:
  - Round 1: 2–3 structures as wireframes for screen 1 (Dr Dlamini at A.1.1, A.1.2 and A.1.5; Dr Mahlangu at A.1.9; Mr
    Smit at A.2.3; and at 390, A.7.3) and for screen 3 with its correction (Mr Smit on Dr Mahlangu's request approved,
    a correction recorded, then completed; and A.1.3), each with its reasoning and its step count; 2–3 variations at
    full fidelity each for screen 2 (A.1.3, A.1.8, A.1.12, A.7.5) and for the decision on screen 3 with the erasure's
    dialog (A.1.3, A.1.12); ONE proposal at fidelity each for not found (A.5.7) and screen 4's kept request. At 1280
    and 390 px. With them, two boards of their own: a Spec board (every component each variation uses, NEW marked; the
    DESIGN.md rules each changes; everything PROPOSED; the data a page would need that it does not read today, marked
    DATA) and a Steps board (each variation's journeys counted, presses + fields typed, at 1280 and 390, and which of
    the 19 Expects below it would change). Stop and wait for my pick.
  - Round 2: the chosen designs with every screen and state above, at 1280 and 390 px, every confirmation, and where
    the focus lands after each action; with two boards of their own:
    - a Spec board: every NEW and changed component, its classes, the DESIGN.md text each changes, the tokens, the
      data each page needs that it does not read today (marked DATA), and every message by kind (result, refusal,
      hint, notice, empty) with its words;
    - a Steps board: each of the 19 runbook steps below, and what its Expect will say once built (quoting today's words
      and giving the new ones; "unchanged" where so), with each journey's count (presses + fields typed) at 1280 and
      390: making an export request and downloading it; reading a rejected request's answer; deciding a request;
      carrying a correction through; approving an erasure; opting out of the digest.
  - Name every design-system component you use, and mark anything else NEW. Say which DESIGN.md rule a variation
    changes (§ The NavMenu's personal links and owner table; § Form page; § Detail page and the title rule; § Table
    system; § Button system's destructive action; § Alerts; § Badges' DataRightsRequest tints; the ids rule of § Content).
  - Mark PROPOSED anything not built: a role newly allowed, a confirmation, a new route, a new action, a refusal, an
    email, a rule change.
  - Flag edge cases: a requester with no institution (only the Administrator decides); a staff member's own request in
    the queue he reviews; an erasure of a person with a review under appeal (kept, answerable); an erasure refused for
    PseudonymSalt or a concurrent change; a request withdrawn while its page is open on another screen; an export whose
    PDF fails (the zip holds the JSON only, saying nothing); a correction to an ended (archived, read-only) programme;
    two requests of one type on one day; a 60-character address; a reason of 4000 characters.
  - Run an accessibility review against WCAG 2.1 AA: one h1, headings in order, every table captioned, every row
    action named for its row, focus after every action and confirmation, refusals tied to their fields
    (aria-describedby), targets ≥ 24 px (44 at 390), status in words, never colour alone.

ATTACHED: act-A/A.1.1-1-dlamini-preferences-saved.png, act-A/A.1.2-2-export-submitted.png,
  act-A/A.1.9-1-mahlangu-reads-rejected.png, act-A/A.1.8-1-smit-queue-three.png,
  act-A/A.1.3-2-export-approved-completed.png, act-A/A.1.8-2-mahlangu-rectification-rejected.png,
  act-A/A.1.12-2-erasure-approved.png, act-A/A.5.7-1-request-not-found.png, act-A/A.1.13-1-ndlovu-session-ended.png,
  act-A/A.1.14-4-checkin-withdrawn.png, act-A/A.7.3-5-data-rights-390.png and act-A/A.7.5-5-data-rights-390.png. Every
  other capture dated 2026-09-26 is an older frame (a Title Case sidebar with Logout, an email in the top bar): read its
  body only. No attached capture shows a password.
```

---

## 2. The journey

| Step | Page | Who does | They must be able to see |
|---|---|---|---|
| A.1.1 | `/` → `/account/data-rights` | Dr Dlamini (Trainee) | My data rights under the rule, lit; preferences, a request form and "Your requests" ("No requests"); the digest help; "Processing preferences saved." focused. **The optional-processing box saves a flag nothing reads (T318); the help promises decision mail (T320); `<h3>` sections (T280)** |
| A.1.2 | `/account/data-rights` | Dr Dlamini | the four types; "Please provide a reason for your request.", nothing sent; then the row with Withdraw. **"You will be notified …" (T319); the time in UTC, the day with it (T325); the select cuts its glosses (new)** |
| A.1.3 | `/admin/data-rights` → `/admin/data-rights/{Id:guid}` | Mr Smit (Coordinator) | KGK's requests only; filters acting on Apply; the request; "A decision note is required."; Completed and the Decision card. **Apply before the filters (T328); "UnderReview" and "Objection" (T324); the Request card run past by 35 px (T323); the refusal at the top (new); "Decided by" an id** |
| A.1.4 | `/account/data-rights/download/{id:guid}` | Mr Smit | Page not found (404), naming nothing (flow 01's) |
| A.1.5 | `/account/data-rights` → download | Dr Dlamini | Completed with a Download named for the request; the zip |
| A.1.6 | `/` → `/account/data-rights` | Dr du Plessis (former) | flow 13's Home and menu; Submitted with Withdraw; "Request withdrawn." focused, the row Withdrawn, nothing offered |
| A.1.7 | `/account/data-rights` | Dr Mahlangu (Trainee) | Submitted, with Withdraw |
| A.1.8 | `/admin/data-rights` → `/admin/data-rights/{Id:guid}` ×2 | Mr Smit | three rows newest first, badged; the withdrawn request offering nothing; "Request rejected." focused. **Had he approved, it would stay Approved for good (T316)** |
| A.1.9 | `/account/data-rights` | Dr Mahlangu | Rejected, nothing offered. **No date, no note, no mail (T319)** |
| A.1.10 | `/committee/reviews` → `new` → `{ReviewId}` → `/committee/decisions-due` | Mr Smit | flow 08's: check-in #8 for Dr Ndlovu, open, for the erasure to end |
| A.1.11 | `/account/data-rights` | Dr Ndlovu (Trainee) | Erasure accepted although #8 is scheduled (T258); Submitted with Withdraw |
| A.1.12 | `/admin/data-rights` → `/admin/data-rights/{Id:guid}` | devadmin | every institution's; Completed at once, the Decision card. **No confirmation, nothing counted (T264); the kept request names his address (T276)** |
| A.1.13 | `/account/session-ended` → `/account/login` | Dr Ndlovu's open tab | flow 02's "Your session has ended. Sign in again."; "Invalid email or password." |
| A.1.14 | `/admin/users` → `/admin/trainees` → `/committee/reviews` → `/committee/reviews/{ReviewId:int}` | Prof Mbatha | flows 12, 13, 08 and 07's: "People 14"; his ended programme under `deleted_user_…` in Completed and closed; his reviews under the pseudonym; #8 Withdrawn with its reason. **The withdrawn review still draws the live Annexure A card (flow 07's, noted)** |
| A.2.3 | `/account/data-rights` | Mr Smit | "Processing preferences saved.", the box ticked |
| A.2.5 | `/account/data-rights` | Mr Smit | "Processing preferences saved.", the box clear |
| A.5.7 | `/admin/data-rights/{Id:guid}` | Mr Smit | **"You are not authorized to access this data-rights request." over "Not found" (one state wanted, T112)** |
| A.7.3 | `/` → … → `/account/data-rights` | Dr Dlamini, 390 px | the cards stack; Your requests (367 px) scrolls in its 358 px container. **The table scrolls rather than stacks** |
| A.7.5 | `/` → … → `/admin/data-rights` | Mr Smit, 390 px | the filters stack above the table (623 px, scrolling in its container). **Apply filters above them (T328)** |

## 3. States to design

Each screenshot is under `design/baseline/`. How to reach each is in `states.md` (the line numbers below are its rows on
2026-10-30). The step captures are from the T379 replay (2026-10-28), on the shell as built; `states/data-rights--*`
(2026-09-28) are on the shell; `states/data-rights-requests--*` and `states/data-rights-request--*` (2026-09-26) are
on the old frame: use the step capture where there is one, and the old one for its body only.

| Page | State | Screenshot | Who | Reached at (`states.md` line) |
|---|---|---|---|---|
| `/account/data-rights` | Nothing requested | `states/data-rights--empty.png` | Dr Dlamini | Step A.1.1, before saving (:279) |
| `/account/data-rights` | Preferences saved | `act-A/A.1.1-1-dlamini-preferences-saved.png` (`states/data-rights--saved.png`) | Dr Dlamini | Step A.1.1 (:280) |
| `/account/data-rights` | No reason given | `act-A/A.1.2-1-export-no-reason-refused.png` (`states/data-rights--reason-missing.png`) | Dr Dlamini | Step A.1.2 (:281) |
| `/account/data-rights` | Submitted | `act-A/A.1.2-2-export-submitted.png` (`states/data-rights--submitted.png`) | Dr Dlamini | Step A.1.2 (:282) |
| `/account/data-rights` | Completed, with Download | `act-A/A.1.5-1-dlamini-completed-download.png` (`states/data-rights--completed.png`) | Dr Dlamini | Step A.1.5 (:283) |
| `/account/data-rights` | Withdrawn | `act-A/A.1.6-2-duplessis-withdrawn.png` (`states/data-rights--withdrawn.png`) | Dr du Plessis | Step A.1.6 (:284) |
| `/account/data-rights` | Rejected | `act-A/A.1.9-1-mahlangu-reads-rejected.png` (`states/data-rights--rejected.png`) | Dr Mahlangu | Step A.1.9 (:285) |
| `/account/data-rights` | Loading | `states/data-rights--loading.png` | Dr Dlamini | held read (:286) |
| `/account/data-rights` | Narrow | `act-A/A.7.3-5-data-rights-390.png` (`states/data-rights--narrow.png`) | Dr Dlamini | Step A.7.3 (:287) |
| `/account/data-rights` | Digest opted out, then in | `act-A/A.2.3-1-smit-digest-opt-out.png`, `act-A/A.2.5-1-smit-digest-opt-in.png` | Mr Smit | Steps A.2.3, A.2.5 |
| `/admin/data-rights` | A coordinator's queue, filtered | `act-A/A.1.3-1-smit-queue-filtered.png` (`states/data-rights-requests--coordinator.png`) | Mr Smit | Step A.1.3 (:1036) |
| `/admin/data-rights` | Nothing matches | `states/data-rights-requests--empty.png` | Mr Smit | Step A.1.3 (:1037) |
| `/admin/data-rights` | Three requests | `act-A/A.1.8-1-smit-queue-three.png` (`states/data-rights-requests--three.png`) | Mr Smit | Step A.1.8 (:1038) |
| `/admin/data-rights` | Every institution, Erasure | `act-A/A.1.12-1-devadmin-queue-erasure.png` (`states/data-rights-requests--administrator.png`) | devadmin | Step A.1.12 (:1039) |
| `/admin/data-rights` | After the erasure | `act-A/A.1.12-3-queue-after-erasure.png` | devadmin | Step A.1.12 |
| `/admin/data-rights` | Loading | `states/data-rights-requests--loading.png` | Mr Smit | held read (:1040) |
| `/admin/data-rights` | Narrow | `act-A/A.7.5-5-data-rights-390.png` (`states/data-rights-requests--narrow.png`) | Mr Smit | Step A.7.5 (:1041) |
| `/admin/data-rights/{Id:guid}` | Submitted | `states/data-rights-request--submitted.png` | Mr Smit | Step A.1.3, before approving (:1042) |
| `/admin/data-rights/{Id:guid}` | No decision note | `states/data-rights-request--note-required.png` | Mr Smit | Step A.1.3 (:1043) |
| `/admin/data-rights/{Id:guid}` | Approved | `act-A/A.1.3-2-export-approved-completed.png` (`states/data-rights-request--approved.png`) | Mr Smit | Step A.1.3 (:1044) |
| `/admin/data-rights/{Id:guid}` | Approved, at 1280 (the card run past) | `act-A/A.1.3-3-request-detail-1280.png` | Mr Smit | Step A.1.3 |
| `/admin/data-rights/{Id:guid}` | Withdrawn | `act-A/A.1.8-3-duplessis-withdrawn-detail.png` (`states/data-rights-request--withdrawn.png`) | Mr Smit | Step A.1.8 (:1045) |
| `/admin/data-rights/{Id:guid}` | Rejected | `act-A/A.1.8-2-mahlangu-rectification-rejected.png` (`states/data-rights-request--rejected.png`) | Mr Smit | Step A.1.8 (:1046) |
| `/admin/data-rights/{Id:guid}` | Erasure approved | `act-A/A.1.12-2-erasure-approved.png` (`states/data-rights-request--erasure.png`) | devadmin | Step A.1.12 (:1047) |
| `/admin/data-rights/{Id:guid}` | Not his | `act-A/A.5.7-1-request-not-found.png` (`states/data-rights-request--not-found.png`) | Mr Smit | Step A.5.7 (:1048) |
| `/admin/data-rights/{Id:guid}` | Narrow | `states/data-rights-request--narrow.png` | Mr Smit | Step A.1.8 at 390 (:1049) |
| `/not-found` | A download that is not his | `act-A/A.1.4-1-smit-download-not-found.png` (`states/not-found--download.png`) | Mr Smit | Step A.1.4 (:207) |
| `/account/login` | Session ended; sign-in refused | `act-A/A.1.13-1-ndlovu-session-ended.png`, `act-A/A.1.13-2-ndlovu-signin-refused.png` | Dr Ndlovu | Step A.1.13 |
| `/admin/users` | After an erasure | `act-A/A.1.14-1-users-no-ndlovu.png` (`states/users-list--after-erasure.png`) | Prof Mbatha | Step A.1.14 (:902) |
| `/admin/trainees` | After an erasure | `act-A/A.1.14-2-trainees-no-ndlovu.png` (`states/pending-trainees-list--erased.png`) | Prof Mbatha | Step A.1.14 (:960) |
| `/committee/reviews` | After an erasure | `act-A/A.1.14-3-reviews-pseudonym.png` (`states/reviews-schedule--pseudonym.png`) | Prof Mbatha | Step A.1.14 (:530) |
| `/committee/reviews/{ReviewId:int}` | Withdrawn by an erasure | `act-A/A.1.14-4-checkin-withdrawn.png` (`states/review-detail--withdrawn.png`) | Prof Mbatha | Step A.1.14 (:576) |

Not captured, so describe them in words (`states.md`; `coverage.md` § Flows and states not played):
- Never built: a decided request's date and note on the requester's row (T319); the correction part, empty, with one
  correction, Complete refused, completed (T316, BRIEF § 7 B3; `coverage.md:187`); the erasure's confirmation dialog
  (T264); a single not-found state (T112).
- Anywhere: a load error on My data rights and on a request (both print the exception's text; a request's draws it over
  "Not found"); the queue's load error (its `LoadError` slot, uncaptured).
- The erasure refusals: "PseudonymSalt is not configured …" (`states.md:1135`, the replay host sets the salt) and
  `ErasureExecutor.PersonChanged` (needs a concurrent write, `coverage.md:216`).
- An export whose PDF fails: the zip holds `data-export.json` alone and nothing says so
  (`I/AccessReportBuilder.cs:182–201`; code).

**Belongs to other flows:** flow 01: not found (A.1.4). Flow 02: session ended and the sign-in refusal (A.1.13). Flow
08: Schedule a review and Decisions due (A.1.10, `act-A/A.1.10-*`). Flow 07: the review page (A.1.14's Annexure A card
on a withdrawn review). Flow 12: Users (A.1.14, `act-A/A.1.14-5-*`). Flow 13: the former trainee's Home (A.1.6,
`act-A/A.1.6-0…`), Trainees' Completed and closed (`act-A/A.1.14-6-*`) and the erased trainee's export entry fixed in
`920f778c` (`act-A/A.1.14-7…`). Flows 03, 05, 06 and 10: the other pages of A.7.3 and A.7.5. Flow 18: the jobs list
(A.2.4, A.2.6, which A.2.3 and A.2.5 set up). Flow 20: the export's `portfolio-summary.pdf`.

## 4. Attach

Paths are relative to `design/baseline/`. The twelve key screenshots were opened on 2026-10-30 or are read from their
step's Actual; none shows a password or a one-time link. The request captures show request ids, user ids and scenario
addresses, which are the pages' own record (A8).

**Key screenshots (attach these first, with § 1):**
1. `act-A/A.1.1-1-dlamini-preferences-saved.png`: the requester's three `<h3>` cards, the unread opt-out, the help
2. `act-A/A.1.2-2-export-submitted.png`: the promise of a notification; a UTC time
3. `act-A/A.1.9-1-mahlangu-reads-rejected.png`: "Rejected" with no date and no note; the cut-off type gloss
4. `act-A/A.1.8-1-smit-queue-three.png`: Apply filters before the filters; the badges
5. `act-A/A.1.3-2-export-approved-completed.png`: the two cards, the run-past values, ids, UTC
6. `act-A/A.1.8-2-mahlangu-rectification-rejected.png`: a decided Rectification
7. `act-A/A.1.12-2-erasure-approved.png`: an erasure approved with nothing asked first; the address kept
8. `act-A/A.5.7-1-request-not-found.png`: "not authorized" over "Not found"
9. `act-A/A.1.13-1-ndlovu-session-ended.png`: the erased requester's tab (flow 02's notice)
10. `act-A/A.1.14-4-checkin-withdrawn.png`: the review the erasure withdrew (flow 07's page)
11. `act-A/A.7.3-5-data-rights-390.png`: My data rights at 390, the table scrolling
12. `act-A/A.7.5-5-data-rights-390.png`: the queue at 390, Apply above the filters

**The requester:** `act-A/A.1.2-1-export-no-reason-refused.png`, `act-A/A.1.5-1-dlamini-completed-download.png`,
`act-A/A.1.6-1-duplessis-access-submitted.png`, `act-A/A.1.6-2-duplessis-withdrawn.png`,
`act-A/A.1.7-1-mahlangu-rectification-submitted.png`, `act-A/A.1.11-1-ndlovu-erasure-submitted.png`,
`act-A/A.2.3-1-smit-digest-opt-out.png`, `act-A/A.2.5-1-smit-digest-opt-in.png`, `states/data-rights--empty.png`,
`states/data-rights--loading.png`.

**The queue:** `act-A/A.1.3-1-smit-queue-filtered.png`, `act-A/A.1.12-1-devadmin-queue-erasure.png`,
`act-A/A.1.12-3-queue-after-erasure.png`, and (old frame) `states/data-rights-requests--empty.png`,
`states/data-rights-requests--loading.png`.

**A request:** `act-A/A.1.3-3-request-detail-1280.png`, `act-A/A.1.8-3-duplessis-withdrawn-detail.png`, and (old frame)
`states/data-rights-request--submitted.png`, `states/data-rights-request--note-required.png`,
`states/data-rights-request--narrow.png`.

**The aftermath:** `act-A/A.1.4-1-smit-download-not-found.png`, `act-A/A.1.13-2-ndlovu-signin-refused.png`,
`act-A/A.1.14-1-users-no-ndlovu.png`, `act-A/A.1.14-2-trainees-no-ndlovu.png`, `act-A/A.1.14-3-reviews-pseudonym.png`.

That is 37 files.

**Do not attach:** the Home captures of this flow's phone and former-trainee steps (`act-A/A.7.3-1-home-390.png`, `act-A/A.7.5-1-home-390.png`, `act-A/A.1.6-0-duplessis-home.png`: flows 01, 05, 06 and 13's); the other pages of A.7.3 and A.7.5 (`act-A/A.7.3-2…` to `-4…`, `-6…`, `act-A/A.7.5-2…` to `-4…`); A.1.10's (`act-A/A.1.10-*`, flow 08's); A.1.14's width checks and the fixed export entry (`act-A/A.1.14-5-*`, `-6-*`, `-7…`); the `states/` captures a step capture replaces (`data-rights--saved`, `--reason-missing`, `--submitted`, `--completed`, `--withdrawn`, `--rejected`, `--narrow`; `data-rights-requests--coordinator`, `--three`, `--administrator`, `--narrow`; `data-rights-request--approved`, `--withdrawn`, `--rejected`, `--erasure`, `--not-found`; `not-found--download`; `users-list--after-erasure`; `pending-trainees-list--erased`; `reviews-schedule--pseudonym`; `review-detail--withdrawn`).

## 5. Known problems this design must solve

The evidence column is for the operator and for Claude Code. Attach only what § 4 lists. Code is cited at `bcf65236`.
"Observed" is seen in a capture or a replay's Actual; "code" is read in the source and not replayed; "inferred" is
neither.

| Task | What it means for the design | Evidence |
|---|---|---|
| **T316** (P2, queued; BRIEF § 7 B3) | Q1 and Q2. Approve leaves a Rectification Approved (`A/Commands/ApproveDataRightsRequest.cs:107–110`); `ApplyRectificationCommand` and `CompleteRectificationRequestCommand` have no caller in Web (code; `coverage.md:187`). Who may record one is `RectificationRoles = [SpecialityAdmin]` (`A/DataRightsAuthorization.cs:32`), who cannot open either page (`DR/RequestDetail.razor:2`); who may approve is `ReviewRoles = [Coordinator]` at the request's institution (`:29`, `:123–142`). **New:** the Coordinator who would record it cannot open the trainee profile that holds a programme start (`W/Admin/Trainees/TraineeProfileEdit.razor:2`, `AdministratorOrInstitutionalAdmin`), and an ended programme's profile is read-only (T305), so "link to the page that owns the data" does not always lead somewhere he may act (code). A new runbook step plays it end to end. | `act-A/A.1.8-2…` (a decided Rectification); `coverage.md:187` |
| **T319** (P2, queued; its data-rights half, the revocation half done in T367) | Q5. The requester's list reads `DataRightsRequestSummaryDto` (Id, RequesterDisplayName, RequestedOn, Type, Status: `A/DataRightsDtos.cs:18–23`), so no decision, date or note reaches her row (observed A.1.9). "Request submitted. You will be notified when it is reviewed." (`W/Profile/DataRights.razor:265`) promises mail no handler sends; there is no data-rights template (code; `src/Wombat.Application/Common/Email/AccountNotices.cs:10` notes T319 owes it). T367 built the one-off notice path it reuses. | `act-A/A.1.9-1…`, `act-A/A.1.2-2…` |
| **T318** (P3, queued) | Q6. "Opt out of optional processing (e.g. analytics)" (`W/Profile/DataRights.razor:36–39`) writes `OptOutOfOptionalProcessing` (`I/ObjectionFlagService.cs:26–27`), which only the erasure reads (it sets it) (code; A.1.1's Note). A refusal can stand beside an earlier success (one `ActionResult` for three actions, `:14–24`). | `act-A/A.1.1-1…` |
| **T320** (P2, queued; the help's half) | Q7. The digest help (`W/Profile/DataRights.razor:164–168`) says "a committee or entrustment decision" is still emailed; T320 records that none is (`DataRightsDigestOptOutTests` pins the sentence). T240 (done) wrote it. | `act-A/A.1.1-1…` |
| **T324** (P3, queued; the data-rights part open, A.1.3) | Q8. The filters loop the enums: "UnderReview" and "Objection" (`DR/RequestsList.razor:22–39`; observed A.1.3); badges, type cells and row names print enum names (`DR/RequestDetail.razor:44–45`; `W/Profile/DataRights.razor:110–117`; `DR/RequestsList.razor:60–68`). **New:** UnderReview is never set: `DataRightsRequest.Review()` has no caller (code), so the filter offers a status no request reaches; Objection is admitted by the validator (`IsInEnum`, `A/Commands/SubmitDataRightsRequest.cs:29–31`) and completes at once on approval (`ApproveDataRightsRequest.cs:101–105`). | `act-A/A.1.8-1…` |
| **T264** (P3, queued; its erasure note and the queue's pager) | Q3. Approve on an Erasure runs it at once (`DR/RequestDetail.razor:118–150`; observed A.1.12); no `ConfirmDialog` on these pages; CUSTOMIZATION.md § 5 records "Approving an erasure asks for no confirmation and does not list what it will withdraw". The hand-built pager (`DR/RequestsList.razor:76–93`) moves no focus. | `act-A/A.1.12-2…` |
| **T323** (A3; the request card) | Q12. At 1280 the Request card's values run past it by 35 px (observed A.1.3: right edge 599, DDs 634); at 390 Your requests (367 px) and the queue (623 px) scroll in their containers rather than stack (observed A.7.3, A.7.5). | `act-A/A.1.3-3…`, `act-A/A.7.5-5…` |
| **T328** (A4; Apply's place, open) | Q10. Apply filters is the page header's action (`DR/RequestsList.razor:8–14`), before the filters in tab order and above them at 390 (observed A.7.5: y 141, the filters 280 and 374). The monospace textareas were fixed by T335's `font: inherit` (`b347e11c`). | `act-A/A.7.5-5…` |
| **T325** (A9; the data-rights part, open) | Q9. `W/Profile/DataRights.razor:109`, `DR/RequestsList.razor:59`, `DR/RequestDetail.razor:43`, `:56`, `:60` print the stored UTC with no zone (observed A.1.2: "2026-10-27 22:01" for 00:01 SAST on 2026-10-28, the day wrong too). | `act-A/A.1.2-2…` |
| **T329** (A6; no data-rights page closed) | My data rights and a request print `ex.Message` for a failed load or action (`W/Profile/DataRights.razor:187`, `:201`, `:232`, `:275`, `:307`; `DR/RequestDetail.razor:117`, `:150`, `:187`); a request has no `LoadError`, so a failed load reads the text over "Not found" (code). | `states/data-rights--loading.png` |
| **T280** (headings) | Q4, Q12. Every section is an `<h3>` under the `<h1>`, with no `<h2>` (`W/Profile/DataRights.razor:28`, `:64`, `:93`; `DR/RequestDetail.razor:39`, `:53`, `:70`); captions repeat headings ("Your data rights requests" under "Your requests"). | `act-A/A.1.1-1…` |
| **T276** (P3, queued; its request half) | Q11. The erasure leaves the request as submitted (`I/ErasureExecutor.cs:425–427`), so the queue and the request still read ndlovu@kgk.wombat.local (observed A.1.12; F-A.1.12a). Its activity leftovers (credit to the pseudonym, nominee ids in `DataJson`, respondent addresses) are rules, not screens: out of this flow. | `act-A/A.1.12-2…`, `-3…` |
| **T112** (done; its not-found half) | Q13. Not found and out of scope both throw "You are not authorized to access this data-rights request." (`A/Queries/GetDataRightsRequestById.cs:40`, `:42`), drawn as an alert over the empty state (observed A.5.7). The rule (one answer for both) holds; the words and the double state are the defect. T112's open items (who the data-rights officer is, Q15) stay open. | `act-A/A.5.7-1…` |
| **New: the decision note's refusal** (code; observed old frame) | Q12. "A decision note is required." is set at the page's top (`DR/RequestDetail.razor:130`, `:167`), the field unmarked and not focused; the same for the reason on My data rights (`W/Profile/DataRights.razor:249`; observed A.1.2: the focus stays on Submit request). Flow 11's rule puts a refusal under its field, focused. | `states/data-rights-request--note-required.png`, `act-A/A.1.2-1…` |
| **New: the request type's glosses cut off** (observed) | Q4. The select shows "Access — download everything we h…" at 1280 (observed A.1.9-1, `states/data-rights--submitted`): the gloss is in the option, not beside it. Flow 11's described radios are the system's choice of a few kinds. | `act-A/A.1.9-1…` |
| **New: Back to list** (code) | Q12. A request's header repeats its trail with "Back to list" (`DR/RequestDetail.razor:12–15`), which flows 12 and 13 dropped from their record pages. | `act-A/A.1.3-2…` |
| **New: the domain lets a decided request be withdrawn** (code) | `DataRightsRequest.Withdraw()` refuses only Completed and Withdrawn (`src/Wombat.Domain/DataRights/DataRightsRequest.cs:115–121`), so an Approved Rectification (stranded today, T316) or a Rejected request may be withdrawn by the command, though the page offers Withdraw only while Submitted or Under review (`W/Profile/DataRights.razor:113`). The design states when Withdraw is offered; the build aligns the rule (inferred: Submitted only). | — |
| **New: an export's PDF may be dropped silently** (code) | A failed `portfolio-summary.pdf` is caught and left out of the zip (`I/AccessReportBuilder.cs:182–185`, `:197–201`); the requester is not told. Flow 20 owns the PDF; this flow decides whether the download says so (an edge case in the ask). | — |

**Dropped from the old brief (done since, or decided):**
- T328's monospace textareas and Arial select (`b347e11c`, flow 01's `font: inherit`; observed A.1.1).
- T239 (`0097df6e`): every row action named for its row (observed A.1.2, A.1.3); T240 (`691d0307`): the digest
  opt-out stops the three reminders by one policy (observed A.2.4); T234 (`2b1b0357`): the focus on each result.
- T279 (done): an erased session ends within a minute through session ended (observed A.1.13: 51 s).
- T258 (`97700425`): an erasure ends every open review and campaign and every profile, keeps the settled record, and
  is one transaction; no review refuses an erasure request (observed A.1.11, A.1.12, A.1.14). The old brief's Q1 list
  is checked against `I/ErasureExecutor.cs` and holds; it adds what goes beside the account (roles, institutional
  sign-ins, scopes, staged decisions, `:205–219`, `:400–422`) and T276's third leftover (a respondent's address).
- Flow 13 (`5cc10c66`, `920f778c`): the old brief's "the trainees list no longer shows him" is void: Completed and
  closed lists his ended programme under the pseudonym (F7, A.1.14); his review no longer offers an export that
  dead-ends.
- The old brief's LAYOUT line (the pre-shell frame), its CONTENT values (the T295 replay's ids, times and
  `deleted_user_622063bc`: replaced by the T379 replay's), and its "Data Rights Requests" nav label (flow 01's
  sentence case).

## 6. Questions for the operator

Each with a recommendation. The canvas answers 3, 4, 9, 10, 12 and 13 in its variations; 1, 2, 5–8, 11 and 14–16 are
the operator's, and 1 and 2 decide screen 3's structure, so they are answered before round 1.

**Decided 2026-10-30: the operator took Q1–Q16 as recommended**, and with them Q17–Q28 of the round-1 assessment
(`14-data-rights/round-1-assessment.md` § 8) as it recommends, which reword Q3's dialog (Q19), Q4's heading (Q24), Q7's
list (Q27), Q10's page size (Q18) and Q13's words (Q17). The round-1 picks are accepted: My data rights A (with what the
assessment takes from B and C), a request A with B's fixed list of records (Q26), the queue V1, deciding V1, the kept
request confirmed with its corrections, not found redrawn per Q17; the assessment's M1–M6 and its shoulds apply. Round 2's
ask is `14-data-rights/round-2-ask.txt`.

**Decided 2026-10-30, after round 2's review: the operator took F1–F10 as recommended**
(`14-data-rights/round-2-review.md`, § Decisions for the operator). F1: the requester's name is the account's, read
live; the stored name only a fallback holding the pseudonym. F2: A.1.8a plays "Mahlngu" (Dr Mahlangu saves it on My
account first). F3: the erasure rewrites every request of the person and withdraws their other open ones. F4: the
erasure's mail goes to the address read before it runs. F5: a Coordinator's forecast names what is at their
institution and counts the rest. F6: a name from an institutional sign-in is said to be the institution's, recorded as
Other. F7: Q24's and Q27's words stay. F8: a concurrency token on the request. F9: the Administrator's own request
needs another Administrator; the last Administrator cannot be erased. F10: the queue's pager only past one page. Two
follow-ups are proposed as tasks: T388 (F6) and T389 (F7). Round 3's ask is `14-data-rights/round-3-ask.txt`.

1. **Q1. (The operator's) Who carries an approved correction through** (T316). *Recommendation:* whoever may approve
   the request: the Coordinator at the request's institution and the Administrator. `RectificationRoles` folds into
   `ReviewRoles`, so one person approves, records each correction and completes the request; the Speciality admin, who
   today may record but cannot open the request, is dropped (PROPOSED; recorded as a D-number in EPA-PROGRAMME § 3).
2. **Q2. (The operator's) What a correction is, and where the data changes.** *Recommendation:* the data is changed on
   the page that owns it, by whoever may change it there; the request records each correction (which record, the value
   before, the value after, who, when) and links that page where the caller may open it; where he may not (a
   Coordinator and the trainee profile, or an ended programme's archived record), the card says who can ("Your
   Institutional admin changes a programme start on the trainee profile.", PROPOSED words) and the correction is
   recorded once made. Complete request is refused until one is recorded. A new step (A.1.8a, Dr Mahlangu's account
   name) plays it end to end; A.1.8 keeps its rejection. An archived record's correction path (T305) stays open.
3. **Q3. An erasure's finality.** *Recommendation:* Approve on an Erasure opens a `ConfirmDialog` "Erase ‹address›'s
   personal data?" that counts what it ends ("1 open committee review (formative check-in #8), no unreleased MSF
   campaign, an active programme, ended today", DATA, PROPOSED: a forecast query gated as the command), says in one
   sentence what is kept under a pseudonym (ratified reviews, a review under appeal, STARs, released reports,
   activities, progress, audit), and ends "This cannot be undone."; its confirm the danger button "Erase and approve",
   its trigger an outline button apart from Reject. T276's leftovers are not listed in the dialog; they are its task's.
4. **Q4. The requester's page: one page or two.** *Recommendation:* one page, three `<h2>` sections in this order: Your
   requests (each with its answer), Make a request (the type as described radios, flow 11's, not a select; the reason),
   Email preferences (the digest opt-out alone, once T318 lands). Two pages would split a rare page in two.
5. **Q5. (The operator's) The decision notice** (T319). *Recommendation:* as T319: one `DataRightsDecisionEmail`
   (type, date, the outcome, a link to My data rights), no note, sent after the save, a failed send never failing the
   decision; an erasure's confirmation read before it runs and sent with no link after it commits. The row shows the
   decision, its date and the note, never who decided. "You will be notified when it is reviewed." stays.
6. **Q6. (The operator's) T318.** *Recommendation:* remove the optional-processing box, its flag, the column (a
   migration) and the erasure's write; the preferences section says the digest reminders can be stopped and that
   processing the programme needs cannot be objected to.
7. **Q7. (The operator's) The digest help** (T320's half). *Recommendation:* fold only the sentence: name the one-off
   mail sent today (a request to assess, an MSF invitation, a password reset, an invitation to register; the build
   checks the list against the templates sent); T320's activity and committee mails stay its own task and change the
   help again when they land.
8. **Q8. (The operator's) Objection and Under review** (T324). *Recommendation:* the validator refuses Objection (the
   digest opt-out is the objection Wombat offers); the Type filter offers the four types; Under review leaves the
   Status filter and the badges, since nothing sets it (or, if triage is wanted, a "Start review" action: not
   recommended for a rare queue).
9. **Q9. Times.** *Recommendation:* "2026-10-28 00:01 SAST" on all three pages, to the minute; the export's JSON keeps
   the stored UTC (the record, DESIGN.md:3393).
10. **Q10. The queue.** *Recommendation:* filters (Type, Status, by label) then Show, flow 06's; Date, Requester (the
    name over the address), Type, Status (a badge in words), one Review named for the row; newest first;
    `PagerControls` at 50 with the focus on the list's heading (flow 08's); `StackWide`; a caption that does not repeat
    the heading.
11. **Q11. (The operator's) The kept request after an erasure** (T276's request half). *Recommendation:* fold it in:
    the erasure's save rewrites the request's display name to the pseudonym, and the queue and the request read
    `deleted_user_0ae1d64b`; A.1.12's Expect changes. T276's activity leftovers stay its own.
12. **Q12. A request's page.** *Recommendation:* titled by type and person ("Erasure request from Sipho Ndlovu"; the
    crumb stays its id), one column of `<h2>` sections with no details-grid (flow 12's): Request, Decision (decided by
    name with the id under it, the date, the note), Correction (Q2), Decide (the note, then Approve and Reject; the
    refusal under the note, focused, the note kept); Back to list gone; ids wrap.
13. **Q13. Not found.** *Recommendation:* one state for an unknown and a foreign id: "Request not found" / "There is no
    data rights request with this id that you can open." with no alert above it (PROPOSED words); A.5.7's Expect changes.
14. **Q14. (The operator's) Withdraw.** *Recommendation:* stays immediate with no dialog (nothing is lost: she can ask
    again), offered only while Submitted; the domain's rule narrows to match (the new defect in § 5).
15. **Q15. (The operator's) Who decides.** *Recommendation:* unchanged: the Coordinator at the request's institution and
    the Administrator; a request with no institution, the Administrator only. T112's data-rights officer stays open.
16. **Q16. (The operator's) What the build folds in.** *Recommendation:* fold T316 (with Q1's role decision, a new step),
    T319's data-rights half (the DTO, the row, the email), T318 (with its migration), T320's help sentence, T324's
    data-rights part with Q8, T264's erasure dialog and the queue's pager, T276's request half, and these pages'
    halves of T323, T328, T325, T329 and T280. T320's mails, T276's activity leftovers, T305's archived-record correction
    and T112's officer stay their own; flow 07's Annexure A card on a withdrawn review is noted for flow 07's owner.

## 7. Acceptance

A flow is done when BRIEF.md § 9's four checks hold. For this flow:

- **Replay the whole runbook** on a fresh database (the flow's steps are A.1.1–A.1.14, A.2.3, A.2.5, A.5.7, A.7.3 and
  A.7.5; § 9): every Expect must hold, the refusals' exact words included. **Rewrite the steps that change:** A.1.1 (no
  optional-processing box; the sections; the help), A.1.2 (the type's control; the time in SAST; the promise), A.1.3
  and A.1.8 (labels, Show after the filters, the page's sections, the refusal under the note), A.1.5 and A.1.9 (the
  answer on the row), A.1.12 (the confirmation; the pseudonym on the kept request, if Q11), A.5.7 (one not-found
  state), A.7.3 and A.7.5 (stacked rows); add the correction's step (Q2; a new step takes a letter, § 11 flow 06);
  search the whole runbook (BRIEF.md § 9 item 7) for the words that change, including A.2.4 and A.2.6, which follow
  A.2.3 and A.2.5. Record passwords only in `pwd_DO_NOT_COMMIT.txt`.
- **Checks the design adds:** A.1.9, the rejected row shows its date and note and no decider; the new step, a
  correction recorded and the request completed by Mr Smit, Complete refused with none; A.1.12, the dialog's counts
  match what the erasure ends (review #8, the profile); A.5.7, an unknown and a foreign id read alike; every time with
  SAST; 390 and 768 with nothing scrolling sideways; no exception's text on any failed read.
- **The tests pass** (`dotnet test tests/Wombat.Web.Tests/Wombat.Web.Tests.csproj`, never with `--no-build`), with all
  six suites green. They must include `Privacy/DataRightsDigestOptOutTests` (the help, rewritten), the data-rights
  pages' own tests with new ones for the row's answer, the correction, the erasure dialog and not found; the handler
  tests T316, T319, T318 and Q8 bring; `Navigation/ActiveNavItemTests`; `Hosting/PageTitleTests`;
  `Accessibility/RowNamesTests`, `ActionFocusTests`, `AccessibleNamesTests`, `IdReferencesTests`; `Design/*`,
  including `NarrowLayoutTests`, `DefinedClassTests`, `ContrastTests`; the scenario guard in `Scenario/`.
- **Re-capture** the states in § 3 (the old-frame ones, and the new ones on scratch copies, as `states.md` says) and
  the steps' own captures, keeping their names; `check_baseline_paths.py` at missing 0. Compare them with the chosen
  artboards.
- **Browser check** at 1280, 768 and 390 px as Dr Dlamini, Dr Mahlangu, Dr du Plessis, Mr Smit, `devadmin` and Prof
  Mbatha.

## 8. The runbook steps, verbatim (sent with § 1, in the same message)

These are pasted from `execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md` (Role, Route, Do and Expect
only), as flow 13's replay left them (T379, 2026-10-28; the Gaps it recorded are A.1.1's T318, A.1.2's T319 and T325,
A.1.3's T324 and T323, A.1.9's T319 and A.1.12's T276 and T264). Since flow 13, A.1.6 reads the former trainee's menu
with Export portfolio and A.1.14 reads Trainees' Completed and closed listing the erased programme under the pseudonym.
`D` is the replay day (README § How to play).

```text
Step A.1.1 — Dr Dlamini reads her data rights and sets a preference (appendix-cross-cutting.md)
Role: Trainee — Dr Anele Dlamini
Route: / → /account/data-rights
Do: Open My data rights, under the rule at the foot of her menu. Read the page, tick "Opt out of optional processing",
  and save her preferences.
Expect: The page has three parts: processing preferences, a request form, and "Your requests", which is empty ("No
  requests"). The digest opt-out's help names the three reminders it stops (the weekly coordinator digest, the 14-day
  draft reminder and the 5-day assessment reminder), and says that email about one particular thing is still sent
  (T240). Saving says "Processing preferences saved." and the focus moves to that message (T234).

Step A.1.2 — Dr Dlamini requests an export of her data (appendix-cross-cutting.md)
Role: Trainee — Dr Anele Dlamini
Route: /account/data-rights
Do: Choose the request type Export and submit it with no reason. Then submit it again with the reason "A copy of my
  training record for my own files."
Expect: The form offers four types: Access, Export, Rectification and Erasure. With no reason the request is refused
  ("Please provide a reason for your request.") and nothing is sent. The second submit says the request was submitted
  and that she will be notified when it is reviewed. "Your requests" lists it: today, Export, Submitted, with a
  Withdraw button named for it and no Download.

Step A.1.3 — Mr Smit approves the export from KGK's queue (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Open Data rights requests from the menu. Narrow it to type Export and status Submitted, apply the filters, and
  review Dr Dlamini's request. Press Approve with no decision note. Then approve it with the note "Identity confirmed;
  export released to the data subject."
Expect: The queue holds only KGK's requests (T112), and the filters change it only when they are applied. On the
  request's page Data rights requests stays lit, and the trail reads Home › Data rights requests › the request's id
  once it has loaded (R2-Rules § 3; DESIGN.md's owner table).
  - The row names the requester by the address she signs in with, and its Review action is named for the row (T239).
  - The detail page shows the requester, her user id, when it was submitted, its type, its status and her reason.
  - Approving with no note is refused: "A decision note is required."
  - Once approved, the request reads Completed at once, because an export is built when it is downloaded. A Decision
    card appears (decided by, as a user id; decided on; the note; completed on), and Approve and Reject are gone.

Step A.1.4 — Mr Smit cannot download what he approved (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights/download/{id:guid}
Do: Open the download address of the request he has just approved. Its id is in the detail page's address.
Expect: Page not found, with status 404. Nothing on the page says that the request exists. The reviewer approves; only
  the data subject, or a global Administrator, collects the bundle (T112).

Step A.1.5 — Dr Dlamini downloads her export (appendix-cross-cutting.md)
Role: Trainee — Dr Anele Dlamini
Route: /account/data-rights → /account/data-rights/download/{id:guid}
Do: Reload My data rights and download the completed request.
Expect: The row reads Completed and offers a Download named for the request.
  - The browser saves `data-export-<date>-<time>.zip`.
  - It holds `data-export.json`: schema version 2, with her profile, every activity she is the subject or author of,
    her committee reviews, the MSF campaigns about her, her progress by semester, her newest 500 audit entries and her
    portfolio exports.
  - It also holds `portfolio-summary.pdf`.
  - No email is sent.

Step A.1.6 — Dr du Plessis asks for access, then withdraws the request (appendix-cross-cutting.md)
Role: Former trainee (no role; a trainee record) — Dr Pieter du Plessis
Route: /account/data-rights
Do: Submit an Access request with the reason "Everything held about me." Then withdraw it from "Your requests".
Expect: He signs in to Home with no role: the menu reads Home, My progress, Export portfolio and My data rights, and
  Home is the former trainee's card as at Step 5.28, "Your programme ended on `D−2`." with the record's four rows, STARs
  plain text (T379, R3-H02). The request lists as Submitted, with a Withdraw named for its type and time. Once
  withdrawn, the page says "Request withdrawn.", the focus moves to that message, and the row reads Withdrawn and offers
  nothing.

Step A.1.7 — Dr Mahlangu asks for a correction (appendix-cross-cutting.md)
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Submit a Rectification request with the reason "My programme start is recorded as 15 January 2026; I started work
  on 19 January."
Expect: The request lists as Submitted, with a Withdraw.

Step A.1.8 — Mr Smit reads the queue and rejects the correction (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights → /admin/data-rights/{Id:guid} → /admin/data-rights/{Id:guid}
Do: Clear both filters and apply them. Open Dr du Plessis's withdrawn request, then Dr Mahlangu's. Reject hers with the
  note "The whole January 2026 intake is admitted on 15 January, the semester boundary (D42). Nothing to correct."
Expect: The queue lists all three KGK requests, newest first, each with its status badge.
  - The withdrawn request's page shows the request and offers no decision.
  - Dr Mahlangu's reads Rejected once rejected, with its Decision card, and the focus moves to "Request rejected."

Step A.1.9 — Dr Mahlangu reads the decision (appendix-cross-cutting.md)
Role: Trainee — Dr Nomsa Mahlangu
Route: /account/data-rights
Do: Reload My data rights and read her request.
Expect: The request reads Rejected and offers nothing: no Withdraw, no Download. The page shows neither the decision
  note nor who decided, and no email told her of the decision.

Step A.1.10 — Mr Smit schedules a formative check-in for Dr Ndlovu (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /committee/reviews → /committee/reviews/new → /committee/reviews/{ReviewId:int} → /committee/decisions-due
Do: Press Schedule a review and schedule a review of Dr Ndlovu before the Paed Annual Review Panel for the period
  holding `D`, Scheduled on `D`, with Review type Formative check-in. Note the review's id from its address. Then open Decisions
  due, set Trainee to Dr Ndlovu and press Show.
Expect: The review opens as at Step 4.49: the subtitle "Formative check-in · Paed Annual Review Panel · Training year
  1", and the sitting bar Scheduled with "Formative: an interim check-in, no binding decision · evidence …" (T361). It
  is open, so the erasure at A.1.12 has a review to end. Decisions due's line reads "Formative check-in #N is open: it
  takes no seat." with Open check-in #N, as at Step 4.49.

Step A.1.11 — Dr Ndlovu requests erasure (appendix-cross-cutting.md)
Role: Trainee — Dr Sipho Ndlovu
Route: /account/data-rights
Do: Submit an Erasure request with the reason "I am leaving the programme and want my personal data removed." Stay
  signed in in this browser.
Expect: The request is accepted although a review of him is scheduled: no review's state refuses an erasure request
  (T258). It lists as Submitted, with a Withdraw.

Step A.1.12 — devadmin approves the erasure (appendix-cross-cutting.md)
Role: Administrator — devadmin
Route: /admin/data-rights → /admin/data-rights/{Id:guid}
Do: Narrow the queue to Erasure and apply the filter, review Dr Ndlovu's request, and approve it with the note "Identity
  confirmed. Programme records are kept under a pseudonym."
Expect: The Administrator's queue holds every institution's requests.
  - Once approved, the request reads Completed at once, with its Decision card. The approval and the erasure are one
    transaction (T258).
  - No confirmation is asked before this irreversible action (noted on T264).
  - The queue and the request still name the requester by the address he signed in with: the erasure keeps the request
    as it was submitted (`ErasureExecutor`).

Step A.1.13 — Dr Ndlovu's open session ends, and he cannot sign in again (appendix-cross-cutting.md)
Role: Trainee — Dr Sipho Ndlovu (the browser left signed in at A.1.11)
Route: /account/session-ended → /account/login
Do: Wait on any page for up to a minute. Then sign in with his old address and password.
Expect: The tab leaves for the sign-in page by itself, which says "Your session has ended. Sign in again." (T279), an
  information notice. Signing in is refused in the words an unknown address gets, "Invalid email or password.", with
  no mention of an erasure (T156). No email is sent to him.

Step A.1.14 — What the erasure left, as KGK sees it (appendix-cross-cutting.md)
Role: InstitutionalAdmin — Prof Nolwazi Mbatha
Route: /admin/users → /admin/trainees → /committee/reviews → /committee/reviews/{ReviewId:int}
Do: Look for Dr Ndlovu among KGK's users, its trainees and its committee reviews. Open the check-in from A.1.10.
Expect: No Dr Ndlovu remains among KGK's users or current trainees, and no row names him (T026, T258).
  - Users reads "People 14", with no pseudonym among them. Dr du Plessis's and Dr Molefe's Roles cells read "No role"
    over "A completed or withdrawn registrar keeps an account and its records.", each with Scope Paediatrics /
    Paediatrics and Status Active (T376, S2, U07).
  - Committee reviews lists his reviews under a pseudonym, `deleted_user_…`, that names nobody: Act 4's ratified
    review as it was; the closed check-in, Type "Formative check-in", State Closed and Decision "—" (read as "None");
    and A.1.10's check-in, Type "Formative check-in", State Withdrawn and Decision "None: the review was withdrawn".
  - The check-in's sitting bar reads Withdrawn and "Withdrawn on `D`. Withdrawn because the trainee's personal data
    was erased at their request. Nothing more is decided at this review." (T258, T361), and the page offers no action:
    no Start, no Refresh.
  - Trainees' "Completed and closed" reads "3 ended" and lists his ended programme under the pseudonym, as Withdrawn
    on the erasure day (T379, F7, R3-C07): the latest end first, so his row (`deleted_user_…`, Paediatric EPA
    Curriculum (11.2) over Paediatrics, `J` to `D` over "Training year 1 when it ended", Withdrawn `D`, View named
    "View deleted_user_…'s record") stands first, before Lerato Molefe's (Completed `D`, the earlier profile) and
    Pieter du Plessis's (Withdrawn on Act 5's `D−2`). It shows no address, and no row names him.
  - His activities and progress stay under the same pseudonym; the account itself is checked in the outcome's SQL.

Step A.2.3 — Mr Smit opts out of digest emails (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Tick "Opt out of digest emails" and save his preferences.
Expect: "Processing preferences saved."

Step A.2.5 — Mr Smit opts back in (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /account/data-rights
Do: Untick "Opt out of digest emails" and save.
Expect: "Processing preferences saved."

Step A.5.7 — A data-rights request that is not his, by id (appendix-cross-cutting.md)
Role: Coordinator — Mr Pieter Smit
Route: /admin/data-rights/{Id:guid}
Do: Open the address of Dr Mahlangu's request with its last digit changed.
Expect: The refusal "You are not authorized to access this data-rights request." and the empty state "Not found". An
  unknown id and another institution's request read the same, so the page does not tell them apart (T112).

Step A.7.3 — Dr Dlamini on her phone (appendix-cross-cutting.md)
Role: Trainee — Dr Anele Dlamini
Route: / → /portfolio/progress → /portfolio/progress/{EpaId:int} → /activities/mine → /account/data-rights
Do: At 390 px, sign in and open her dashboard, My progress and one EPA's page from its index, My activities and My data
  rights from the menu.
Expect: The dashboard's cards stack, each Furthest short row's figure under its name. On My progress, This period, the
  index and the Entrustment panel stack: each EPA's link is a 44 px block, every cell but the link labelled by its
  column, and each Latest rating link in the panel is 44 px tall (T328); nothing scrolls sideways. The EPA's page draws
  its chart at 322 px, never scaled, with its table stacked under it (T355, T323). My activities' rows stack, each cell
  but the link and the state labelled by its column (T342), its Credit link 44 px tall, and the table of "Your requests"
  scrolls inside its container; each row's link or action stays reachable.

Step A.7.5 — Mr Smit on his phone (appendix-cross-cutting.md)
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
  one action a 44px button the block's width; the pager's buttons are 44px (T364). The queue's filters stack above its table.
```
