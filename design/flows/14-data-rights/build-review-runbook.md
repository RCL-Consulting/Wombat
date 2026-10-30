# T387 build review: the runbook (flow 14)

Reviewer: read-only. Branch `t387` at `71190f07`, build `c47bc6e0..71190f07`. Read:
`execution/knowledge/scenario-paediatrics/appendix-cross-cutting.md` (A.1.1 to A.1.14 with the new A.1.8a, A.2.3 to A.2.6,
A.5.7, A.7.3, A.7.5, the Outcome), `coverage.md`, `states.md`, and the brief's § 8. Every Expect was set against the code on the
branch (the code wins): the three pages and their words classes, `DataRightsDecisionEmail`, `ErasureExecutor`,
`GetErasureForecast`, the enums, the entity configurations, `Profile.razor`. Nothing was run; the scripts
(`sync_verbatim_steps.py`, `check_verbatim_steps.py`, `check_flow_completeness.py`) and `ScenarioRunbookTests` were not re-run; the
commit message of `374b55c9` reports them green (425 steps, 332 steps).

Result: **0 must-fix, 4 should-fix, 5 nits.** The Expects agree with the code almost everywhere, A.1.8a plays end to end in
order, and the Outcome's counts follow from the steps. The should-fixes are state recipes that cannot produce the screenshot they
name, and two states whose recipe differs from the board they cite.

## Findings

### R1 (should-fix): "Erasure with nothing else open" is reached with a person who still has a programme
- `states.md:1077` (`data-rights-request--erasure-none.png`): "a scratch registrar with no open review or campaign submits an
  Erasure ... 'Nothing else was open to end.'" `DataRightsRequestWords.Erased` (`:~255`) writes "the programme ended today" whenever
  `ProfilesEnded` is 1 or more, and the forecast ends the active profile ("The programme, ..., which ends today as Withdrawn"). A
  registrar in a running programme therefore reads "1 ... and the programme ended today", which is R3-D09, not R3-D22.
- Fix: name a person with no active profile: Dr du Plessis (left in Act 5, no open review; his campaign is Withdrawn) or Dr Molefe
  (graduated; her campaign is Released), each of whom can submit (A.1.6 shows he can). Say "no open review, campaign, request or
  active programme".

### R2 (should-fix): the bulk and cross-institution recipes give no way to build the state
- `states.md:1053` and `:1054` ("Past one page", "Page 2 by Next"): "45 more requests across four institutions (scratch accounts at
  three new institutions, one with a 60-character address, one request with no institution), ... an Approved row". Forty-five UI
  submissions and three new institutions are not a recipe. R3-Q09 and Q10 need 50 rows, each type and status, an Institution cell
  "None", and a 60-character address. Give one `INSERT ... SELECT ... FROM generate_series(1, 45)` into `"DataRightsRequests"`
  (columns from `DataRightsRequestConfiguration`: type, status, institution or NULL, requester, reason, `RequestedOn`,
  `RequesterDisplayName`) over a few scratch users.
- `:1090` ("Another institution's id") says "a request at a second institution" without saying how. The Demo Institution's dev
  coordinator is already on the replay database (A.2.4's Actual reads "Hi Demo,"); say "the Demo coordinator submits an Export",
  or give the SQL. A.5.7's own Actual found "no other institution holds a request".
- `:1079` ("Erasure of a staff member, a review elsewhere counted", R3-D13) needs "a person holding a staff role at KGK with an
  open review at a second institution" and gives neither the role grant nor how a review of that person comes to sit at another
  institution. The forecast counts reviews by the panel's institution (`GetErasureForecast.cs:~120-135`), so the recipe is an
  `INSERT` of one `CommitteeReviews` row (State 1 to 3) on a panel of a second institution with `TraineeUserId` = the person, or an
  `UPDATE` of an existing review's panel. Write it.
- Also `:1093` ("Long values"): "an account with a 60-character address" needs an invitation at that address; one line saying so
  (Prof Mbatha invites it, as A.4.5's scratch does).

### R3 (should-fix): "After the erasure" is described as the queue the board draws, and it is not
- `states.md:1050` (`data-rights-requests--erased.png`, R3-Q07) says "At Step A.1.12, back on the queue: the row
  deleted_user_0ae1d64b over 'Erased `D`'". R3-Q07 draws the queue with Type "Erasure" applied: "Requests (1)", the caption's tail
  ": Erasure", Clear filters, one row. A.1.12's Expect says, correctly, that the filter is not kept across pages "so all five rows
  show" (a new `RequestsList` starts unfiltered). A replay following the row captures five rows; the board has one.
- Fix, either: the row adds "choose Erasure and press Show" (then R3-Q07 and the capture agree, and the state is one more
  filtered capture), or the row says "five rows, Requests (5)" and `design/flows/14-data-rights.md` notes that R3-Q07 shows the
  filter re-applied. The first costs nothing and keeps the board valid. (This is lane W's first gap.)

### R4 (should-fix): "The person's earlier requests rewritten" produces rows K02 does not draw
- `states.md:1052`: "an Access request, which Mr Smit rejects, then an Export, left Submitted, then an Erasure": the rows are
  Erasure Completed, Export Withdrawn (by the erasure), Access Rejected. R3-K02 draws Erasure Completed, Access Withdrawn and a
  third Completed row, and the build note's point (F3, "every request of the person rewritten") is that a Completed and a
  Withdrawn row both read the pseudonym. Make the recipe: the registrar submits an Access and withdraws it, submits an Export that
  Mr Smit approves (Completed at once), then an Erasure that devadmin approves; or say that the capture differs from K02's mix.
  Add that a rejected row also reads the pseudonym if the check should cover the Rejected status.

### R5 (nit): the "No salt" row starts from a snapshot with no Erasure request
- `states.md:1080`: "Scratch (post-act6) ... on an Erasure request". No Erasure exists at post-act6 (A.1.11 is the first). Add
  "after Dr Ndlovu's A.1.11 submit" or "a registrar submits an Erasure", and that the scratch instance is started with the salt
  unset (the host list's `Wombat__PseudonymSalt` item, inverted).

### R6 (nit): A.7.3 says "every cell but Submitted labelled"
- The Actions cell carries no `data-label` (and should not: its buttons are named for their row). Say "every text cell but
  Submitted". The page has Submitted as the row header and Actions as the only unlabelled cells.

### R7 (nit): the Outcome's "Replay check" paragraph reads as a check of the new Outcome
- `appendix-cross-cutting.md:1899-1912`: "Replay check (2026-10-28, T379 replay ...): match ... the four KGK requests (0,5,1) (1,3,1)
  (2,4,1) (4,4,1) ... Dlamini opted out of optional processing only". Every Actual in the file that T387 outdates carries "predates
  T387"; this block does not, and it now contradicts the SQL below it (five requests, no opt-out column). Add the same sentence.

### R8 (nit): A.1.3's Expect does not say what the Decide buttons order is, A.1.12's names the trigger only
- Cosmetic: A.1.3 reads "Reject and Approve"; at 390 the primary is on top (`column-reverse`), which A.7.5 does not repeat for the
  request page. If the replay captures 390 for this page, say so in the row for `data-rights-request--narrow.png`
  (`states.md:~1092`, "At Step A.1.8, at 390 px"): it will show Approve above Reject.

### R9 (nit): the mail steps cannot verify a recipient
- A.1.3, A.1.8, A.1.8a and A.1.12 expect stub mails; the log carries "its subject and text and no address" (A.2.4's Note, T282). So
  the step cannot show that the erasure's confirmation went to the address read before the erasure (F4); that is held by
  `ApproveDataRightsRequestHandlerTests` only. Add a Note to A.1.12 saying so (lane W's third gap): nothing for the replay to do,
  and nothing to add to the SQL, since an erased account keeps no address.

## What was checked and holds

- **A.1.8a end to end, in order.** My account takes "Mahlngu" (`Profile.razor`, "Save name", "Name saved.", `/account/profile/submit`);
  the Rectification is Submitted; Mr Smit's queue reads "Requests (4)" (Export, Access, A.1.7's Rejected, the new one); Approve
  gives "Request approved. Nomsa Mahlngu is emailed. Record each correction below, then complete the request." (live name,
  `DataRightsRequestWords.Approved`); the Correction section shows the four records (Programme start only when the read gives it),
  `Complete request` with none refused on the page with no audit row; Dr Mahlangu's row reads "Approved on `D`: “…” Being corrected; it
  reads Completed once done." (the note ends with a full stop, so none follows the quote); the corrective save; the
  record "Account › Last name" with Mahlngu to Mahlangu, "Correction recorded: Account › Last name." heading Correction; Complete
  gives "Request completed. ..." and the mail; the row reads "Completed on `D`. Corrected: Account › Last name." A name help line
  reads the live name, so before the corrective save it says Mahlngu and after it Mahlangu, as the Expect says. Mahlangu is a
  password account, so the two name radios are enabled (C11 is the institutional sign-in).
- **The words.** A.1.1, A.1.2, A.1.3, A.1.5, A.1.6, A.1.7, A.1.8, A.1.9, A.1.11, A.1.12, A.2.3, A.2.5, A.5.7 and A.7.5 quote
  sentences that are in `MyDataRightsWords`, `DataRightsRequestWords` and `DataRightsDecisionEmail` letter for letter. The five mail
  subjects and sentences match the template (`Lines`, `Subject`). Focus claims match (`FocusOnLoad`, `ActionResult`).
- **The Outcome.** Five requests: Export (Completed), Access (Withdrawn), A.1.7's Rectification (Rejected), A.1.8a's (Completed),
  Erasure (Completed) = (0,5), (1,3), (1,4), (2,4), (4,4) with Status 3 Rejected, 4 Completed, 5 Withdrawn and Type 0, 1, 2, 4 (checked
  against the enums; Objection is 3). Audit: Submit 5 (A.1.2, A.1.6, A.1.7, A.1.8a, A.1.11; the refused empty reason is checked on the
  page, no row), Withdraw 1, Approve 3 (A.1.3, A.1.8a, A.1.12; Cancel and the no-note press send nothing), Reject 1, Apply 1, Complete 1
  (the refused Complete is a page check). `DataRightsRectifications` columns (`TargetType`, `FromValueJson` jsonb, `AppliedByUserId`)
  are right, and `#>> '{}'` works on the jsonb string. The 11 stub mails: 3 digests + 3 nudges + 5 notices (Export approved,
  Rectification rejected, Rectification approved, Rectification completed, Erasure confirmation); the Complete mail is in (the
  operator's approval removed T2's "10"). `RunScheduledJobNow 10` and the lockout counts are not T387's.
- **Counts.** `### Step` totals 332 across the acts and the appendix, as `coverage.md` says. The two Not built rows go correctly;
  A.1.8a is on the right page rows (My data rights, My account, its submit, the queue, the request).
- **The word search (T3).** "Processing preferences saved.", "Opt out of digest emails", "Apply filters", "UnderReview", "Back to list" and
  "Not found" remain only inside Actuals that carry "predates" or in a Gap that closes them; A.2.4 and A.2.6 are unchanged and
  right (they read the log).
- **42 "Not yet captured (T387)".** The count is right (`grep -c` = 42). The recipe for each is playable except the ones in R1, R2,
  R4 and R5; the rest are inline (no change), a hold-a-read, or a short scratch step. The two unchanged-state and one-result rows
  are inline and write at most an audit row the Outcome does not count (stated). Spot-checked SQL: `State = 5` is UnderAppeal and the
  dialog's "review under appeal" line follows (R3-D14); the devadmin institution update for D21 matches the forecast's
  "only active Administrator" test.

## Lane W's six gaps: what each needs

| Gap | Needs |
|---|---|
| R3-Q07's kept filter | R3 above: have the row press Erasure and Show (one sentence), so the capture matches the board; nothing in the code. A queue that keeps its filter across a visit would be a feature (a query string), not a runbook change: file it as a task if wanted. |
| M08 and M09's help | The boards' Reminder emails help lacks the expiry clause on every M board (F7 decided the build adds it). Nothing in the runbook (A.1.1 already quotes the clause); one line in `design/flows/14-data-rights.md` that the M boards predate F7's naming, so the recaptured baseline differs by that clause. |
| The mail log names no address | R9: a Note on A.1.12, no code. |
| T1's route | The step's Route has `/account/profile/submit` (twice) and `/admin/data-rights` that T1 (R3-0-steps-2) omits. The runbook's is the right one for the guard (each posted address is a page the step plays); keep it and say in § 8 of the brief that T1's route was widened. |
| Brief § 3 and § 4 stale captures | These describe the pages as found at `bcf65236`. Label them "as found" in the brief's headings (they are the record of the problem), and let the next replay's captures replace them in the baseline; do not rewrite them. |
| D13 and D21 need scratch setup | R2 for D13 (the SQL for a review at a second institution) and R1's neighbours; D21's recipe is complete and plays on a scratch copy (checked against the forecast's last-Administrator test and `LastAdministrator` refusal order). |
