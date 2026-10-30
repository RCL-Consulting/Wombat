# Flow 14 round 2: the review

Round 2 (the boards saved in `round-2/`: 138 boards, the 133 page boards at 1280 and 390 with 768 for the eight tables, R2-Q13
at 390 only, plus `R2-0-spec`, `-spec-2`, `-spec-3`, `R2-0-steps` and `-steps-2`) drew My data rights (M), the queue (Q), a
request and its decision (D), the correction (C), the kept request (K) and Request unavailable (U), with the 20
Illustrations marked. Reviewed 2026-10-30 from three sides, each a Sonnet agent: **the code** (`round-2-review-code.md`,
K1-K17 and N1-N8: `src/`, the ask, Q1-Q28), **the cast** (`round-2-review-cast.md`, C1-C11 and N1: the runbook, the
fixtures, the appendix) and **the design system** (`round-2-review-ds.md`, D1-D17: `bundle.css` v37, DESIGN.md, `app.css`,
a static pass; checklist 7 of 9 done, corrections 23 of 25). The three source files are kept as they are; this file merges
them. The integrator checked every must-fix against the code or the boards before carrying it, and rechecked D1, D2, D3
and D5 against the two stylesheets the boards link, which the design reviewer could not see and which are now saved as
`round-2/f14r2.css` (the NEW rules, PROPOSED for `app.css`) and `round-2/f14r2-canvas.css` (`.anno`, `.pv-focus`,
`.pv-overlay`, canvas only). What was narrowed or is not a fault is said where it was. Severity: M must-fix, S should-fix, N
nit. Round 1's M1-M6 are fixed on the boards (K's and D's closing tables); its S1-S12 and N1-N7 are done, but for N3 and N7,
done in part (M4 and the D11 nit below).

**What holds, by all three:** every person, address, role, menu, request id, user id, time (to the minute, SAST), reason,
note and the pseudonym against FIXTURES and the runbook's T379 Actuals; the erasure dialog's counts and keeps against
`ErasureExecutor` and Sipho Ndlovu's state at A.1.12; Q1-Q28 as decided; the built words quoted as built, verbatim; the
390 and 768 twins carry their 1280 words (133 boards, text and every `aria-label`, `title`, `alt`, `href`); one h1 a board,
no inline `style=`, no raw colour, no unknown class, no dangling id, no gendered pronoun, the brand mark and shell flow
01's; the dialogs are flow 12's `ConfirmDialog`, the results and refusals in `ActionResult`, the badges Q22's five; the
Spec says plainly that the captures did not arrive as attachments (10 read from the folder, 8 staged and not opened, the
appendix not in the folder); no board draws a replay-reached state under the Illustration chip.

## Must fix

| # | Side | Finding | Fix |
|---|---|---|---|
| M1 | code (K1), cast (C1) | **The requester's name has no source, and A.1.8a contradicts itself on it.** Checked: `RequesterDisplayName` is written once at submission from `ClaimTypes.Name`, the user name, i.e. the address (`SubmitDataRightsRequest.cs:49-51`); nothing reads the account's name. A.1.8a's Before says the account's last name reads "Mahlngu" (`‹how the runbook sets it›`), yet C01-C04 and C07 draw the h1 "Rectification request from Nomsa Mahlangu", the Requester, the tab title and the three record lines ("Changed by Nomsa Mahlangu on My account") in the corrected spelling before the correction is saved; "Mahlngu" appears only as C05's and C06's Value before. A live name would make the boards wrong before the save; a frozen one would make them wrong after it (C06, Q08); either way Q05 and Q08 must agree on one spelling for both of her rows | Decide the rule (F1) and write it in Spec 1: the account's first and last name read live, the column only a fallback that holds the pseudonym once erased; submission stores the full name, not the user name. Then draw "Nomsa Mahlngu" on C01-C04 and C07 (and on M08 if it is read before the save), "Nomsa Mahlangu" from C05 on, and say on the Spec how the Before is set (F2) |
| M2 | code (K2, N5) | **The erasure rewrites one request's name; every other request by the same person keeps the address, and their open ones stay Submitted forever.** Checked: `ErasureExecutor` does not touch `DataRightsRequests` at all ("the requester display name was denormalized at submission time and stays", `:425-427`), and `ApproveDataRightsRequest` hands it only the one entity, so Q11's rewrite of the approved request leaves a person's earlier Export or Access row carrying the address the erasure promised to remove. `DecidedByUserId` (and the proposed `AppliedByUserId`) are not rewritten when the erased person decided or recorded, while every other user-id column the executor handles is (`CommitteeDecisions.DecidedByChairUserId`, `:223`). The replay does not show it: Dr Ndlovu has one request | F3: rewrite `RequesterDisplayName` on every request whose `RequesterUserId` is the person, `DecidedByUserId` and `AppliedByUserId` to the pseudonym, and withdraw the person's other open requests in the same save, counted in the forecast. Say so in Spec 1 and the completeness test; draw (or list in Spec 3's edges) an erased person with two requests |
| M3 | cast (C2), code (K10) | **The erasure's notice is decided (Q5) but neither the Steps nor the sender allow it.** The Steps mark A.1.13 "unchanged: the erased person is sent none"; Q5 says the erasure's confirmation is read before it runs and sent with no link after it commits, so A.1.13's Expect ("No email is sent to him") and Actual ("No mail") invert, and so do the "No mail" Actuals of A.1.3, A.1.8 and A.1.12 and the Outcome state's "6 Stub emails" (5 more: 11). And the mail cannot go through `AccountNotices` as written: checked, `SendAsync(userId, …)` loads the account after the save and skips one that is deactivated or has no address (`AccountNotices.cs:60-66`), which an erased account is both; its class comment also forbids free text and requires "by whom", which Q5's notice drops | F4: an overload taking the address, read in the handler before `ExecuteAsync`; the class comment gains the data-rights exception; `DataRightsDecisionEmail` a NEW template file. Steps: A.1.13 "Expect changes: a mail is logged for the old address"; A.1.3, A.1.8, A.1.12 and the Outcome count listed |
| M4 | ds (D1) | **The queue's Type and Status selects are 38 px and the pager's page-size select 28 px at 390.** Rechecked against `f14r2.css`: the queue's filters sit in `.search-container`, the page-size select in `.pager`, and no NEW rule raises a `.form-select` (the only 390 rules are `.data-rights-page .btn, .form-control` and labels); `bundle.css`'s select rules are scoped to `.committee-calendar`, `.msf-page`, `.people-page`, `.user-page`, `.activity-form`. Round 1's N3 asked for exactly these | Add one NEW rule below 641 px, `.search-container .form-select, .pager .form-select {min-height: 2.75rem}`, as a system rule for every list (flow 13's D8 found the pager's on every flow), list it in the NEW table and Spec 3's accessibility line |
| M5 | ds (D2) | **The three stacked tables drop DataTable's roles and its base stack class.** Rechecked: `R2-Q04-390`, `R2-M04-390`, `R2-C05-390` write `<table class="clinic-table clinic-table--stack-wide data-rights-table">` with no `role=` on any table, row or cell; `DataTable` with `StackWide` emits `clinic-table--stack clinic-table--stack-wide`, `role="table"`, `rowgroup`, `row`, `columnheader`, `rowheader`, `cell`, because a row laid out as a grid loses its semantics (`bundle.css:2556-2562`); flow 13's boards drew them | Both classes and every role on Your requests, the queue and Corrections recorded; Spec 3 says the roles are DataTable's. `f14r2.css`'s `.data-rights-table tbody th[scope="row"]` rule stays (it styles the row header) |
| M6 | ds (D3) | **The Status cell has no `data-label`**, so stacked (below 900 px) the badge stands with no "Status:" before it. Rechecked: 0 `data-label="Status"` on Q04 and M04; every other text cell is labelled; shipped tables label it (`EntrustmentDecisions/Index.razor:138`) | `data-label="Status"` on the badge cell in both tables that carry one |

## Should fix

- **S1 The requester's email is a DATA item with no row** (K3; C01-C10, D01-D16): the request page draws "Email
  mahlangu@…"; `DataRightsRequestDto` carries none and the row stores none. Add "the requester's email (none once erased)",
  read through `GetContactsAsync` with the name (M1), to the request page's DATA row and the queue's second line; K01 rightly
  draws none.
- **S2 The Programme start line needs four reads** (K4; C01, C07, C08): the requester's preferred profile id (an `int` for
  `/admin/trainees/edit?id=`), whether it has ended, whether the caller may open it, and whether there is a profile at all
  (hide the radio for an Assessor). `TargetId` is a Guid, so a recorded Programme start cannot be followed back to the
  profile: say so. C08 prints "(T305)" in a sentence: use flow 13's "This programme has ended, so its record is archived and
  cannot be changed." (`TraineeProfileEdit.razor:76`).
- **S3 My data rights' institution name has no read** (K5; M01, M18): "Staff at Kgosi Kgari Teaching Hospital decide it"
  shows for a person with no requests, so it cannot ride on `GetMyDataRightsRequestsQuery`'s rows. A separate read of the
  caller's institution (null for none) or a wrapper result; say which.
- **S4 The Coordinator's queue names the caller's institution in five places** (K6; Q01, Q02, Q11, Q12): subtitle, caption,
  empty line. Add it to the DATA row; a Coordinator with no institution claim sees an empty queue today: draw or state it
  ("your institution", or the empty state).
- **S5 The erasure forecast's scope** (K7; D07, D13, D14): checked, the executor withdraws every open review of the person
  by `TraineeUserId` at any institution (`ErasureExecutor.cs:195-206`), so a forecast "gated as the command" would show a
  Coordinator another institution's review, panel and state, which the review pages hide (404, not 403). F5; draw D13 for it.
- **S6 The erasure's result needs a result type** (K8; D09): `ExecuteAsync` returns the `DataRightsErasureRecord`, not "none";
  `ApproveDataRightsRequestCommand` returns `DataRightsRequestDto`, and the executor counts nothing. Widen the result
  (reviews withdrawn, campaigns withdrawn, profiles ended, requests withdrawn per M2, the pseudonym), write the sentence as a
  sum of the clauses present, and draw the case with none ("the programme ended today" is conditional).
- **S7 A name corrected on My account reverts at the next institutional sign-in** (K9; C01-C10, A.1.8a): checked,
  `ExternalLoginHandler` rewrites first and last name from the provider's `name` claim whenever they differ (`:540-547`).
  F6; an edge row on Spec 3 either way.
- **S8 Q21 for the Administrator, and the flags' source** (K11, N4; D15): no board or words for the Administrator's own
  request ("needs another Administrator"); with one `devadmin` it can never be decided. Compute the page's may-decide and
  may-record flags from `CanReview`/`CanRectify` as changed, not from roles. F9, with N4's last Administrator.
- **S9 "Stop the regular reminder emails" and the expiry job** (K12; M01, M02, M11-M13, M16): checked, the entrustment
  expiry job writes to trainees and honours neither the opt-out nor a lock (`ReminderRecipientPolicy.cs`, "A known gap").
  F7.
- **S10 The requester's own DTO** (K13; M04, M08, M09): `DataRightsRequestSummaryDto` also feeds the staff queue, so widening
  it puts decision notes in the queue's payload. A separate `MyDataRightsRequestDto`; the corrections' labels by one grouped
  read of `DataRightsRectifications`.
- **S11 The record's label and values** (K14; C05, C09, M09): state the encoding (always `JsonSerializer.Serialize(string)`,
  read back with `Deserialize<string>`; compared canonical-to-canonical, CLAUDE.md's jsonb rule), keep Other's name in
  `TargetType` (`Other:‹name›`, at most 200, with a field refusal) rather than inside the before-value, and say "Enter the
  value before." is the page's rule (the validator's `NotEmpty()` passes `""`).
- **S12 The queue's paging and filters are new handlers** (K15, N8; Q01-Q04): `_pageSize` is a constant 25 and the pager
  hand-built; `PageSizeLabel` gives "Requests per page"; Show and Clear filters are new; `FilterBar` is not a component (the
  pattern is `search-container` / `search-field filter-actions`, `DecisionsDue.razor:100`), and the request page's
  `StatePanel` `LoadError` is unused today. The filter-built caption and empty sentence go in Spec 2's NEW words.
- **S13 The withdrawn refusal and the race** (K16; D12): Approve and Reject on a Withdrawn request throw the domain's "Only
  submitted or under-review requests can be approved." (`DataRightsRequest.cs:87,98`), not D12's sentence: the handlers
  check `Withdrawn` first. Checked, `DataRightsRequestConfiguration` has no concurrency token, so a Withdraw and an erasure's
  Approve that both loaded Submitted can both save. F8.
- **S14 The request page's three load outcomes** (K17; D17, D18, U01, U02): the handler returns null for unknown and foreign
  ids; the page's `catch (Exception ex) { _errorMessage = ex.Message }` goes, every exception becoming the fixed words with
  `OnRetry`; Unavailable only on null. One Spec line.
- **S15 The Steps' word search ran over the brief's copy, not the runbook** (C3): real hits for "Apply filters" (A.1.3,
  A.7.5, F-A.7.5b), "UnderReview" (A.1.3, F-A.1.3b), "Back to list" (A.5.7), the UTC times (A.1.2-A.1.12, the Outcome),
  "optional processing" (A.1.1, the Shared state, the SQL); and the Gaps they close (F-A.1.1a, F-A.1.2a, F-A.1.9a,
  F-A.1.2c, F-A.1.3b, F-A.1.3c, F-A.1.12a, F-A.5.7a, F-A.7.5b). Fill the table from the appendix, and the cast review's
  list of steps the build rewrites.
- **S16 The Outcome state is half listed** (C4): the request tuples gain a Completed Rectification; the audit counts (Submit
  5, Approve 3) and the `IN (…)` list gain `ApplyRectificationCommand` and `CompleteRectificationRequestCommand`; the two
  Shared state bullets; the `OptOutOfOptionalProcessing` SELECT becomes a hard SQL error after T318; Dr Mahlangu's name
  saves beside Prof Mbatha's.
- **S17 M08 is cited and never reached** (C5): add the Do line where Dr Mahlangu reads her row between Mr Smit's approval
  and her save, or make M08 an optional `states.md` row (flow 13's H04).
- **S18 Clear filters drawn with no filter applied** (C6, D7; Q13 at 390): both filters read "All", the caption is
  unfiltered, and Q08 (same rows) has none; DESIGN.md shows it "once any is set". Drop it from Q13 and show N3's 44 px
  Clear filters on Q02, Q03, Q06, Q07, which have a filter.
- **S19 Every pinned value is T379's** (C7): A.1.8a adds five actions before A.1.11, so the next replay mints new ids, minutes,
  Dr Ndlovu's user id and so a new pseudonym and a new "51 s". One Spec line: the build re-pins from the replay that plays
  A.1.8a; and M17's same-minute rule also names the queue's Review links if A.1.8a's request lands in the minute of the
  rejected one.
- **S20 The two required radio groups do not say "required"** (D4; M01-M18, C01-C10): add `<span class="visually-hidden">
  required</span>` after the legend's star (DESIGN.md:1299-1302).
- **S21 Three NEW rules repeat `.user-page`** (D5, narrowed): rechecked against `f14r2.css`. The request and correction pages
  already carry `class="user-page request-page"`, so `.request-page .detail-list` (`minmax(0, 1fr)`) repeats `.user-page
  .detail-list dd {min-width: 0; overflow-wrap: anywhere}` and `.request-page .correction-records .form-check label` repeats
  `.user-page .choice-group .form-check label` (the group is a `.choice-group`). `.data-rights-page` is `.user-page` rule for
  rule, except its `.form-check label` outside a choice group (the reminder checkbox) and the stacked table's full-width
  `.btn`. Put My data rights on `.user-page`; keep one scoped rule for the checkbox label if `.user-page` lacks it; drop
  `.request-page`, `.correction-records` and `.data-rights-page` from the NEW table, or say what each adds.
- **S22 The stylesheets** (D6, resolved in part): saved now as `f14r2.css` and `f14r2-canvas.css`; the Spec names them
  `r2-new.css` and `r2-canvas.css` and the boards link `/_blob/` copies. Round 3 names the files as saved and links them;
  the rendered measurement at 1280, 1100, 768 and 390 (overflow, 390 control boxes, a 60-character address and a
  4,000-character reason) is still owed before the build: none of the three reviews rendered a board.
- **S23 The pager on lists of one to five rows** (D8; Q01, Q03, Q04, Q06-Q08, Q13): flow 13's closed list and the People
  list draw it only past one page. F10.
- **S24 The 4,000-character reason is not drawn** (D9; M17): fill the textarea with a bracketed long reason and the counter
  "4,000 of 4,000 characters"; check the box does not widen at 390.

## Nits

- PseudonymSalt's new words need the exception text in `ApproveDataRightsRequest.cs:76` changed: one code row; the config
  key is for the operator, not the Coordinator (K N1).
- The may-download flag has no consumer on any board: draw an Administrator's Download on a Completed Export, or drop it
  (K N2).
- "Reminder emails stopped." depends on the saved state; Save with the box unchanged has no words: give it one or disable
  Save until changed; name the reader, updater and DTO T318 changes (K N3).
- Three requests in one minute: M17's "the later / the earlier of two" has no third; use the seconds or the id's tail
  (K N6).
- K01's "erased on" is the SAST date of `ErasedOn` (`ProgrammeCalendar.DateOf`); add "(SAST date)" to DATA (K N7).
- Fill du Plessis's and Mahlangu's `‹user id›` from `wombat_scenario_t379`'s `AspNetUsers` (a read, no capture prints them),
  or re-pin with S19 (C8).
- Illustrations that keep a cast member over a state they never have (C04, C07, C09, C10, M16, M17, D19, Q09, Q10): bracket
  the person or say so in the chip, as flow 13's C11 (C9).
- The dialog's "the ratified review" could carry "#5" if the others carry a number (C10).
- The Steps' "Dr Mahlangu" and "Mr Smit" are the runbook's titles, not page copy: accepted, no change (C11).
- Seven captures named in the ask were staged and not opened; M02, M04, M06, D01-D03 stand on the runbook's words alone,
  which agree: open them in round 3 (cast N1).
- Spec 2's contrast lines: `badge-accepted`'s text is `--text-color` on the tint (the border is `--warning-color`, 5.4:1);
  the focus ring is 4.86:1 on white, not 5.2:1. Both pass (D10).
- Download's accessible name for an Access request is unstated; write it per type, starting with the visible word (D11).
- K01's Requester dd shows the pseudonym alone; Q11 says "over 'Erased 2026-10-28'": add the `.person-cell` second line or
  say the notice carries it (D12).
- D17 and D18 (loading, failed) drop the id crumb the loaded page ends in; draw Home › Data rights requests › the id (D13).
- Three chips say focus returns to the h1 but no h1 has `tabindex="-1"`: draw it, or say `FocusOnNavigate` does it (D14).
- No field has `required`; say in Spec 3 that the pages validate on press, as built (D15).
- `aria-describedby` puts the refusal before the help; DESIGN.md contradicts itself (help first in § Form system, refusal
  first in § Button system): decide once in DESIGN.md, as flow 13's D14 left open (D16).
- `h2#s-dec` and `h2#s-cor` carry `tabindex="-1"` where nothing focuses them; keep it only where the build does (D17).

## Decisions for the operator

The operator's Q1-Q28 (`14-data-rights.md` § 6, `round-1-assessment.md` § 8) stand as given; nothing below reopens them.

| | Question | Recommendation | Alternatives |
|---|---|---|---|
| F1 | **Where the requester's name comes from** (M1) | **The account's current first and last name, read live** (`GetContactsAsync`) on the queue and the request page; the column a fallback that holds the pseudonym once erased (F3); submission stores the full name, not the user name. The page then shows a correction land, which is what A.1.8a plays | Frozen at submission: the request keeps the name it was filed under, and C06 and Q08 read "Mahlngu" after the correction (state it on the Spec) |
| F2 | **How A.1.8a's Before sets "Mahlngu"** (M1, C1) | **Played, not seeded**: the step's first Do line has Dr Mahlangu save "Mahlngu" on My account (the only route, Q25), then submit the Rectification; the replay re-pins (S19) | A stated SQL line in the Before, outside the pages |
| F3 | **What the erasure rewrites in the data-rights tables** (M2) | **Every request of the person**: `RequesterDisplayName` to the pseudonym; `DecidedByUserId` and `AppliedByUserId` to the pseudonym where the person decided or recorded; the person's other open requests withdrawn in the same save and counted in the forecast and the result. `RequesterUserId` stays, as the erasure record (`DataRightsErasureRecord.UserId`, `RequestId`) already pairs the id and the pseudonym (*needs-confirmation*: rewrite it too if "nothing under the id" should hold here) | Only the approved request (Q11 as worded), the others listed as T276's leftovers; not recommended: the address survives the erasure |
| F4 | **The erasure's email** (M3) | **Keep Q5**: an `AccountNotices` overload that takes the address read before `ExecuteAsync`, sent with no link after the commit; the class comment names the exception; the Steps' "No mail" lines and the Outcome count change | No mail to an erased person (Q5's erasure half reversed); A.1.13 stays unchanged |
| F5 | **The forecast for a Coordinator** (S5) | **In full at the caller's institution, counted elsewhere** ("and 1 open review at another institution"); the Administrator sees every name. The dialog still counts everything the erasure ends | The Administrator alone decides an erasure that reaches another institution |
| F6 | **A name that reverts at the next institutional sign-in** (S7) | **Say so where it is recorded**: for a requester with an institutional sign-in (`GetSignInMethodsAsync`), the First and Last name radios carry "This name comes from ‹institution›'s sign-in; the institution corrects it." and record as Other; propose a task for My account's name form for such a person | Stop `ExternalLoginHandler` overwriting a name the person set (an SSO product change, T155's ground, not this flow) |
| F7 | **"Stop the regular reminder emails" and the expiry notice** (S9) | **Keep Q24's words**: the expiry notice is mail about one decision, which Q27's help sentence ("Email about one particular thing is still sent") covers; the build names it in that sentence's examples when it checks the list against the senders; the lock gap (`ReminderRecipientPolicy`'s "known gap") proposed as its own task | Honour the opt-out in the expiry job; or narrow the label to the three jobs |
| F8 | **A concurrency token on `DataRightsRequest`** (S13) | **Add one** (`xmin`, a migration; nothing is live) before an irreversible approval relies on a status read; a lost race reads the row again and shows its state's words (D12's for a Withdrawn one); the handlers check `Withdrawn` first | Lock the row (`SELECT … FOR UPDATE`) in the erasure's approval only |
| F9 | **The Administrator's own request, and the last Administrator** (S8) | **Draw "Another Administrator decides it."** (Q21); with one Administrator the request waits, accepted; an erasure of the last Administrator is refused with its own words | Let a sole Administrator decide their own non-erasure request |
| F10 | **When the queue's pager shows** (S23) | **Only past one page** (`MatchCount > PageSize`), as the People list and flow 13's closed list; drawn on Q09, Q10 | On every list, disabled when one page, with a DESIGN.md line saying so |

## What round 3's ask must do

- **Restate F1-F10 as decided**, beside Q1-Q28, and draw what each decides (F1 and F2: C01-C07 and M08's spellings and the
  Before; F3: an erased person with two requests; F5: D13 at another institution; F6: the institutional sign-in line; F9:
  the Administrator's own request; F10: the pager).
- **Fix M1-M6 and S1-S24 board by board**, citing the id; the nits as one list, no agent each. Redraw C01-C04 and C07 (M1);
  every stacked table's roles and the Status label (M5, M6); the 390 selects (M4); Q13 (S18); M17's reason (S24).
- **The Spec**: the name rule and its read (M1, S1); the erasure's rewrites, withdrawn requests and result type (M2, S6); the
  address overload and the new template (M3); the select rule (M4); the profile reads for Programme start (S2); the
  institution reads (S3, S4); the forecast's scope (S5); `MyDataRightsRequestDto` (S10); the record encoding and Other
  (S11); the real filter pattern and handlers (S12); the Withdrawn check and the token (S13); the load outcomes (S14); the
  NEW table cut to what `.user-page` lacks (S21); the stylesheets named as saved (S22).
- **The Steps** as M3, S15-S17 and S19; recompute A.1.8a's journey count after F2.
- **Measure** the boards at 1280, 1100, 768 and 390 with `f14r2.css` linked (S22), and open the seven captures not opened
  (cast N1); keep every fixture, word and order the reviews found true, invent nothing.

## Who raised what

| Here | Code review | Cast review | Design review |
|---|---|---|---|
| M1 | K1 | C1 | |
| M2 | K2, N5 | | |
| M3 | K10 | C2 | |
| M4 | | | D1 |
| M5 | | | D2 |
| M6 | | | D3 |
| S1-S14 | K3-K9, K11-K17, N4, N8 (S1 = K3, S2 = K4, S3 = K5, S4 = K6, S5 = K7, S6 = K8, S7 = K9, S8 = K11 and N4, S9 = K12, S10 = K13, S11 = K14, S12 = K15 and N8, S13 = K16, S14 = K17) | | |
| S15-S19 | | C3, C4, C5, C6, C7 | S18 also D7 |
| S20-S24 | | | D4, D5 (narrowed), D6 (resolved in part), D8, D9 |
| Nits | K N1-N3, N6, N7 | C8-C11, N1 | D10-D17 |

Merged from 7 must, 26 should and 21 nits across the three reviews (54) into **6 must, 24 should and 18 nits** (C1 into M1,
K N5 into M2, K10 into M3, D7 into S18, K N4 into S8, K N8 into S12). Where the reviews meet, none contradicts another:
K1 and C1 are one fault seen from the code and from the runbook; K10 gives C2's mechanism; D7 and C6 are the same board.

## The operator's decisions (2026-10-30)

The operator took F1–F10 as recommended, with every must-fix, should-fix and nit above as round 3's ask requires them.
Two follow-ups were proposed as tasks rather than folded in: T388 (F6: a name changed on My account reverts at the next
institutional sign-in) and T389 (F7: the digest opt-out does not reach every scheduled reminder; a locked account's mail
and the entrustment expiry notice ignore it). F3 is taken with `RequesterUserId` kept, as recommended. Q1–Q28 stand.
Round 3's ask is `round-3-ask.txt`; the captures round 2 did not open go with it (attached, or read from the upload
folder).
