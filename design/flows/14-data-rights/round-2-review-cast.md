# Flow 14 round 2: the cast and runbook review

Reviewed 2026-10-30 from the boards' text and markup in `round-2/` (R2-0-spec, -spec-2, -spec-3, R2-0-steps, -steps-2, and
the 133 page boards: M01 to M18, Q01 to Q13, D01 to D19, C01 to C10, K01, U01, U02, at 1280, 390 and 768), against
`round-2-ask.txt` (FIXTURES, DECISIONS, CORRECTIONS), the brief, and the runbook (`execution/knowledge/scenario-paediatrics/`:
README cast, `appendix-cross-cutting.md` A.1.1 to A.1.14 with their T379 Actual lines, A.2.3 to A.2.6, A.5.7, A.7.3, A.7.5 and the
Outcome state, `states.md` data-rights rows, `act-1-setup.md` Step 1.1, `act-4-annual-review.md` for Ndlovu's reviews) and
the source (`ErasureExecutor.cs`, the trainee pages' `[Authorize]` policy). Severity: M must-fix, S should-fix, N nit.
D = 2026-10-28; every stored time is UTC, two hours behind SAST.

## What holds

- **Names, roles, institutions.** Anele Dlamini, Nomsa Mahlangu, Sipho Ndlovu, Pieter du Plessis, Pieter Smit and "Demo
  Administrator" are spelled as the README and FIXTURES have them. No title and no gendered pronoun on any page body (word search
  over the text and every `aria-label`, `title` and `alt` of all 138 boards: no he, she, his, her, him, Dr, Mr, Prof). M4 holds.
- **Menus** equal FIXTURES and Step 1.1: Dlamini, Mahlangu and Ndlovu the Trainee's (Home, Log an activity, My activities, MSF
  reports, My committee reviews, Export portfolio, My progress, My data rights, "Acting as Trainee"); du Plessis Home, My
  progress, Export portfolio, My data rights with no "Acting as" (M05, M06); Smit the Coordinator's with Data rights requests
  then My data rights; devadmin the grouped Administrator's menu under "Acting as Administrator" (every D06 to D14, Q05 to Q10,
  C07), never a Coordinator's (S1). M18's menu (Home, My data rights) is the PendingTrainee's (`states.md` 294).
- **Ids and times.** Request ids 01a17f61-f16f-77c1-860c-661586d5b733 (Dlamini), 01a17f63-7af4-7ac5-a6d1-d01726b57bc0 (du
  Plessis), 01a17f63-8c25-7c6e-8724-8593f3d7a145 (Mahlangu), 01a17f64-f566-75f0-91ba-5d71863b77d0 (Ndlovu); user ids Dlamini,
  Ndlovu, Smit, devadmin as FIXTURES (the runbook prints only the first 8 characters, A.1.3 line 112, A.1.12 line 267, line 510;
  the full ids come from the brief only). Times: 00:01, 00:03, 00:03, 00:04 SAST against 22:01, 22:03, 22:03, 22:04 UTC (A.1.2 to
  A.1.11 Actual); Smit's approval 00:01, his rejection 00:03, devadmin's approval and the completion 00:04 (22:04:50 UTC). Every
  time carries "SAST"; the one UTC string on a board is none.
- **Reasons and notes** are character-for-character the runbook's (A.1.2, A.1.6, A.1.7, A.1.8, A.1.11, A.1.12). `deleted_user_0ae1d64b`
  and "Erased 2026-10-28" are A.1.12's and A.1.14's.
- **The erasure dialog matches what Sipho Ndlovu has at A.1.12** (D07; the result in D09 agrees):
  - 1 open committee review, Formative check-in #8, Paed Annual Review Panel, Scheduled (A.1.10 Actual; withdrawn at A.1.12, state 7).
  - No MSF campaign (only du Plessis's and Molefe's campaigns exist, A.7.5 Actual).
  - The active programme, Paediatric EPA Curriculum (11.2), ended 2026-10-28 as Withdrawn (A.1.14 Actual).
  - Kept: Act 4's ratified review, the closed check-in #6, activities, progress and audit entries, the request (A.1.14, Outcome SQL).
  - "An open session ends within a minute" (51 s at A.1.13). D09's "1 committee review was withdrawn and the programme ended today"
    is the same count.
- **Queues.** KGK's three rows at A.1.8 (Q04: Mahlangu Submitted, du Plessis Withdrawn, Dlamini Completed, newest first), the
  one-row Export/Submitted of A.1.3 (Q03), devadmin's five at A.1.12 (four in the replay plus A.1.8a's, Q05) and one after the
  Erasure filter (Q06), the pseudonym row (Q07, Q08, Q13). Statuses and types per row are right; the A.1.8a row sits between
  Ndlovu's and Mahlangu's older one, which is where "after 00:03, before Ndlovu's" puts it.
- **Row action names** (`Review the Export request from Anele Dlamini, 2026-10-28 00:01 SAST`, `Withdraw the Export request made
  2026-10-28 00:01 SAST`, `Download the export of ...`) equal the built pattern in SAST; the erased row reads
  `Review the Erasure request from deleted_user_0ae1d64b, 2026-10-28 00:04 SAST`.
- **Authorisation text.** "Changed on Nomsa Mahlangu's programme page, which an Institutional admin at Kgosi Kgari Teaching
  Hospital opens; you cannot." is true: `/admin/trainees/edit` is `AdministratorOrInstitutionalAdmin` (a Coordinator, Smit, cannot).
  "You cannot download it yourself" for Smit equals A.1.4 (404 for a Coordinator, an Administrator may).
- **A.5.7, A.7.3, A.7.5.** U01's id (the last digit 6) and the Steps quotes of today's Expect and Actual are right; A.7.3's "scrolls
  inside its container" and A.7.5's "filters stack above its table" are quoted correctly. A.2.4 and A.2.6 (which the Steps board
  could not read) are confirmed unchanged: both observe the digest in the log, not the page's words.
- **390 and 768 twins.** All 69 twins at 390 and the 8 at 768 diffed against their 1280 twins on body text and on every
  `aria-label`, `title`, `alt`, `value` and `href`: the sorted text lines are identical apart from the chip's width. Q13 has no 1280
  twin, as asked.
- **Illustrations.** The chip and the title agree on every Illustration board (M13, M16 to M18, Q09, Q10, D10 to D16, D19, C04,
  C07 to C10, U02). No board draws a replay-reached state under the chip. M08, M09, C01 to C03, C05, C06, Q05, Q08 and Q13 carry
  A.1.8a's brackets and are labelled by the Spec as "a NEW step not yet played".
- **Captures.** R2-0-spec says first, and plainly: the twelve did not arrive as attachments, they were read from the folder;
  10 were opened (named), 8 staged and not opened (named), the rest of the 37 not staged; and the appendix was not in the folder.
  That is what the ask required. See N1 for what the unopened ones cost.

## Findings

| # | Sev | Board | Finding |
|---|---|---|---|
| C1 | M | R2-C01 to C04, C07 (and R2-M08 if it is read before the save), R2-0-steps-2 A.1.8a | **The name on the pages contradicts the step's own premise.** A.1.8a's Before says "the account's last name reads 'Mahlngu'", and its Do has Mr Smit approve, read the section and press Complete request (C01 to C03), and only then Dr Mahlangu save "Mahlangu" on My account. Until that save the h1 ("Rectification request from Nomsa Mahlangu"), Requester, tab title, the three record lines ("Changed by Nomsa Mahlangu on My account") and the closing line ("Nomsa Mahlangu's My data rights") read the stored name, "Nomsa Mahlngu". The boards print "Mahlangu" throughout and "Mahlngu" only as C05's Value before. The runbook offers no help: every step before and after prints "Nomsa Mahlangu" (A.1.7 line 172, A.1.8 line 186, Act 4, the README cast), and `‹how the runbook sets it›` is still a bracket. Fix: say on the Spec when "Mahlngu" is set. The only legal route is Dr Mahlangu saving it herself on My account (no staff page changes a name, Q25), or a stated SQL line in the Before. Then draw "Nomsa Mahlngu" in C01 to C04 and C07 (the Illustration may bracket it), "Nomsa Mahlangu" from C05 on, and put the top bar of her own boards right (M08 when read before the save). |
| C2 | M | R2-0-steps "Words that change" row "No email"; Step A.1.13 | **A.1.13 is marked "unchanged: the erased person is sent none", against Q5.** Q5: "an erasure's address is read before it runs and the mail sent, with no link, after it commits." The runbook's A.1.13 Expect says "No email is sent to him." and its Actual "No mail" (lines 280, 284); after the build both are false (a stub mail to the old address is logged). Also not listed as changing: the "No mail" Actuals of A.1.3 (line 113), A.1.8 (line 198) and A.1.12 (a Q5 decision mail now each), and the Outcome state's "6 Stub emails since the app's restart" (line 1762; the digests three, the two nudges three), which gains one per decision and completion and the erasure mail (Approve Dlamini, Reject Mahlangu, Approve and Complete for A.1.8a, Erasure Ndlovu: 5 more, 11). Fix: A.1.13 "Expect changes: a mail is logged for the old address, none is read by him"; add the four steps and the Outcome line to the list. |
| C3 | S | R2-0-steps-2 "Words that change, searched" | **The search ran over the brief's copy of the Expects, not the runbook, so its "none" lines are wrong.** Real hits: "Apply filters" at A.1.3 Actual (line 107), A.7.5 Actual (lines 1419, 1421) and Gap F-A.7.5b; "UnderReview" at A.1.3 Actual and Gap F-A.1.3b (lines 113, 116); "Back to list" at A.5.7 Actual (line 859); the UTC times at A.1.2 (lines 85 to 87, Gap F-A.1.2c), A.1.3 (109, 113), A.1.5 (142 to 147), A.1.6 (167, 168), A.1.7 (179), A.1.8 (197), A.1.11 (246), A.1.12 (267) and the Outcome state (1742); "optional processing" at A.1.1 Do and Note (55, 61), the Shared state bullet (1761) and the SQL (1808 to 1810). The Gaps these close are not named either: F-A.1.1a (T318), F-A.1.2a and F-A.1.9a (T319), F-A.1.2c (T325), F-A.1.3b (T324), F-A.1.3c (T323), F-A.1.12a (T276 and T264), F-A.5.7a, F-A.7.5b (T328). Fill the table from the appendix now it is in hand. |
| C4 | S | R2-0-steps-2 "Expects A.1.8a moves" | **The Outcome state is only half listed.** It says "Goal ... four data-rights requests -> five" and "gains the completed Rectification". Also changing (appendix lines 1742 to 1746, 1761, 1789, 1808 to 1812): the request tuples `(0,5,1) (1,3,1) (2,4,1) (4,4,1)` gain a Rectification Completed; the audit counts "Submit 4 ... Approve 2" become 5 and 3 (and the `IN (...)` list gains ApplyRectificationCommand and CompleteRectificationRequestCommand, or the Rectification rows go uncounted); the "Shared state" bullets "KGK has four data-rights requests" and "Dr Dlamini has opted out of optional processing" (the SQL and the closing list are named, these two bullets are not); the `OptOutOfOptionalProcessing` column in the SELECT stops existing (T318, a hard SQL error, not a changed value); "Dr Mahlangu's last name was saved once, to its old value" belongs in the list beside "Prof Mbatha saved her name unchanged" (line 1752). |
| C5 | S | R2-0-steps-2 A.1.8a, R2-M08 | **M08 is cited by the Expect but no Do line reaches it.** The Expect says "Before completion Nomsa Mahlangu's row reads Approved, 'Being corrected', no Withdraw (R2-M08)". The Do has Dr Mahlangu read the row only once, at the end (Completed, M09). Add the read between Mr Smit's approval and the account save, or retitle M08 an optional state in `states.md` (as flow 13 did with H04). Also the Do skips C04 (no Illustration) and C05's second chance: it says "records Account > Last name" once, which equals the one-correction journey count. |
| C6 | S | R2-Q13 (390) against R2-Q04, Q08 | **Clear filters is drawn with both filters at their defaults** ("All types", "All statuses", page size 20). The same defaults on Q04 and Q08 show no Clear filters, and the built rule (A.7.5 Actual, Decisions due: "Clear filters (shown once a filter is off its default)") is the one the ask cites for N3. Either set a filter on Q13 (and read the rows it would give) or drop the button. Q13 is A.7.5's board, and the Steps row for A.7.5 says "Show and Clear filters each 44 px". |
| C7 | S | R2-D06 to D09, K01, Q05 to Q08, Q13, M10, U01, R2-0-steps A.1.12 | **The pinned values are T379's, and A.1.8a moves them.** In the T379 replay the run from A.1.8 to A.1.11 took 66 seconds (00:03:39 to 00:04:45). A.1.8a adds a submit, an approval, an account save, a record and a completion before A.1.11, so the next replay mints new ids (the 01a17f6x prefix is time-based), new minutes (Ndlovu's 00:04), a new user id and so a new pseudonym (`deleted_user_0ae1d64b` is derived from the user id and the salt), and a new "51 s". Say once on the Spec that every hard value is T379's and the build re-pins from the replay that plays A.1.8a; and that the two-in-a-minute naming rule of M17 is also needed for the queue's Review names if A.1.8a's request lands in the same minute as the rejected one (both read `Review the Rectification request from Nomsa Mahlangu, 2026-10-28 00:03 SAST`, Q05, Q08, Q13). |
| C8 | N | R2-D04 to D05, R2-C01 to C10, D13 to D15, D19 | `‹user id›` for du Plessis (D04) and Mahlangu (D05 and the C boards) can be filled from the T379 database (`wombat_scenario_t379`, `AspNetUsers`), not from any capture: no step prints them. See the table below. |
| C9 | N | R2-C04, C07, C09, C10, M16, M17, Q09, Q10, D19 | Illustrations that keep a cast member over a state the cast never has: C04, C07, C09 and C10 keep Nomsa Mahlangu with a bracketed request; M16 and M17 give Anele Dlamini two Export requests in a minute and a failed submit; D19 puts Pieter Smit's id under a bracketed requester; Q09 and Q10 print `kgk.wombat.local` under a bracketed institution. Bracket the person where the state is not hers, or say so in the chip (flow 13's C11). |
| C10 | N | R2-D07 | The dialog says "the ratified review" and "closed check-in #6" but names no number for the review. Review #5 is the annual review (Act 4, Step 4.10); #5 and #6 are both kept. Harmless; add "#5" if the others carry one. |
| C11 | N | R2-0-steps-2 A.1.8a | The Role line and the Do use the runbook's titles ("Dr Mahlangu", "Mr Smit"), as the runbook's own steps do; no page body does. Fine; noted because the brief asks for no titles on pages only. |
| N1 | N | R2-0-spec "The captures" | Three of the twelve captures the ask names (A.1.8-2-mahlangu-rectification-rejected, A.1.13-1-ndlovu-session-ended, A.1.14-4-checkin-withdrawn) and four it says to read before drawing (A.1.2-1, A.1.5-1, A.1.6-2, A.1.3-3, the request at 1280) were staged and not opened. The spec is honest and "draws nothing from them", but M02, M04, M06 and D01 to D03 stand on the runbook's words alone; they agree with the Actual lines, so nothing is wrong, only unproved. |

## Steps whose Expect (or Do, Note, Gap) the build must rewrite beyond the Steps board's list

- **A.1.1:** Do ("tick 'Opt out of optional processing'", line 55), Expect (three parts, "Processing preferences saved."), Note (the unread
  flag), the Actual's 23:59 save on 2026-10-27 (A.1.1 now saves nothing, Q28), Gap F-A.1.1a closes (T318).
- **A.1.2:** Expect (the "will be notified" sentence stays; the row prints SAST), Actual's "2026-10-27 22:01", Gap F-A.1.2a stays open until
  Q5 lands, F-A.1.2c closes (T325).
- **A.1.3:** Do ("apply the filters"), Expect (the row names the requester "by the address", "Decision card (decided by, as a user id)",
  "No email" lines), Actual (lines 106 to 115, all of it), Gaps F-A.1.3b and F-A.1.3c close.
- **A.1.5, A.1.9:** "No email is sent" and "no email told her" invert (Steps has these).
- **A.1.8:** Expect ("each with its status badge" is now Q22's words), Actual's "badge-draft" for Withdrawn.
- **A.1.12:** Expect ("No confirmation is asked", "name the requester by the address he signed in with"), Note, Gap F-A.1.12a closes.
- **A.1.13:** "No email is sent to him" inverts (C2).
- **A.2.3, A.2.5:** Do and Expect (the Steps board has these); A.2.4, A.2.6 confirmed unchanged.
- **A.5.7:** Expect, Actual (lines 854 to 861), Gap.
- **A.7.3:** "scrolls inside its container" (line 1334 to 1337) and the Actual's "Your requests (367 px) scrolls in its 358 px container".
- **A.7.5:** Expect's last sentence, the Actual's "623 px, scrolls in its container ... Apply filters in the page header", Gap F-A.7.5b.
- **Outcome state and Handoff:** C4. `coverage.md` and `states.md` rows 279 to 287 and 1036 to 1049 (the data-rights rows) also change.

## Brackets the runbook or a capture could fill

| Board | Bracket | Value and source |
|---|---|---|
| R2-D04, D05, D15, D19, C01 to C10 | `‹user id›` for du Plessis and Mahlangu | The T379 database (`wombat_scenario_t379`), a read, not a capture. No step or capture prints either id. |
| R2-D06 to D09 | none left | all of Ndlovu's values are in hand (T379). |
| R2-M08, M09, Q05, Q08, Q13, C01 to C06 | `‹time›`, `‹date›`, `‹request id›`, `‹reason›`, `‹note›`, the Recorded time | Only a replay that plays A.1.8a fills them. No capture can. Every one is correctly still a bracket. |
| R2-0-steps-2 | `‹how the runbook sets it›` | A decision, then the replay (C1). |
| R2-M17, D13, D14, D15, D19, C08, C09, Q09, Q10, M16, M18 | `‹name›`, `‹address›`, `‹institution›`, `‹roles›`, `‹n›`, `‹reviews›`, `‹campaigns›`, `‹curriculum›`, `‹what is settled›`, `‹review›` | Illustrations. Neither a capture nor the replay fills them. |
| R2-0-spec-2 | `‹Type›` in the route table | A template word, not a fill. |
