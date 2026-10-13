# Flow 08 build review (T364 step 6)

Three read-only reviewers on Sonnet (`review-lanes.md`) reviewed the branch `t364` after the three page lanes merged
(`9b68dedd`, `13c2d26e`): R covered the code, the data and authorization; D the pages, the design system and
accessibility; G the runbook, the cast and regressions, after the runbook lane. Severity: H high, M medium, L low. Each
finding was traced in the code by its reviewer; P marks one the reviewer could not fully trace.

## R: the code, the data and authorization

No high or medium finding. **Sound:**
- F1's own-period rule holds for semester, annual and optional rows, ended periods and revoked STARs.
- One seat check (`CommitteeReviewSeats.HolderAsync`) serves the rows, the seat lines, `GetSchedulingSeat` and the
  create.
- The create refuses before staging.
- The Trainees list stays in reach.
- `GetSchedulingSeat` returns null outside scope.
- `InstitutionId` is honoured only for an Administrator.
- `DecisionBodyLock` names the review `SetDecisionPanelBody` refuses with.
- `ScheduleReturn` is no open redirect.
- "Today" is South African.

| # | Sev | Finding | Fix |
|---|---|---|---|
| R1 | L–M (P) | `PanelsList.razor` `LoadAsync` has no request token: an Administrator switching institution twice quickly can see A's panels under B | A request counter, as `DecisionsDue.LoadAsync` |
| R2 | L | `GetSchedulingSeat`'s validator refuses `PanelId <= 0` while the doc promises null, never a refusal | Drop the id rules; the handler answers null |
| R3 | L | The trainee picker falls back to the display name for a blank surname; `ThenBySurname` does not, so the two orders differ | One rule: `ThenBySurname` with the fallback, used by both |
| R4 | L | `ScheduleReview.razor` `CreateAsync` and `PanelEdit.razor` `ShowSaveRefusal` print any `InvalidOperationException`'s message; framework errors share the type (T338) | Show only the handler's refusals (a marker or known messages); else the fixed words, logged |
| R5 | L | `CreateDecisionPanel` checks `MaximumLength(200)` on the untrimmed name but stores it trimmed | Validate the trimmed length |
| R6 | L | `ListDecisionPanels` reads member, institution and speciality names for the scheduling picker too, which uses none | Skip them when `ForScheduling` |
| R7 | L | Any non-empty `?created=` announces "created" on an old panel | Accept only `1` |

## D: the pages, the design system and accessibility

No high finding. **Sound:**
- Every class used is defined, and only in the four Flow 08 sections; phone rules are scoped.
- No inline style or raw hex.
- Row actions start with their visible words.
- One h1 per page; tables are captioned, with row headers.
- The pager is named, and the focus goes to "Decisions, page n of m".
- Refusal summaries take the focus.
- The group refusal has `aria-describedby`, and the Agenda has one live region.
- Dates are ISO.

| # | Sev | Finding | Fix |
|---|---|---|---|
| D1 | M | Below 641 px the held-seat actions `column-reverse` puts "Create review is not offered…" after Open review and Cancel | The reason first on phones (`order: -1`, or outside the reversed row) |
| D2 | L | Between choosing and the reads returning, the Agenda's live region says "Choose a panel, a trainee and a period…" | A reading state: "Reading the agenda." |
| D3 | L | `PanelEdit.razor` Save panel and Save committee are natively `disabled` while the other form saves | `InFlight.AriaDisabled(Busy)`; `Busy` already ignores a second press |
| D4 | L | A body panel's Entrustment-only stays selected after switching to a general panel in S1 (F4: the default) | Take `DefaultFor` on a panel or period change unless the type was chosen by hand |
| D5 | L | `.committee-calendar .clinic-table--stack tbody th` strips every stacked row header on the five pages, not only the seat picker's | Scope it to `.panel-seats` |
| D6 | L | `PanelWords.NoneCreatable` says "their own speciality at their own institution" (no pronoun) | Reword without the pronoun |
| D7 | L (P) | Review type's radio labels hold the whole help sentence, so the names are long | Help as `aria-describedby` under each label |

## G: the runbook, the cast and regressions

Reviewed at `2a327ea1`, after the runbook lane (`b200c97b`, `ff83870b`). No high finding.

**Sound:**
- Every changed step's quoted sentences and counts match the words classes and handlers.
- 4.1 and 4.2 keep their checks (M10), and 4.2 presses a Missed row's Schedule (F1).
- 4.8 and 4.13 check the warnings first (F5).
- coverage.md's committee step lists match the Route lines.
- Old words survive only in Actual lines.
- No regression found:
  - the review page's route does not clash with `/new`;
  - `NavOwners` covers the new page;
  - nothing else links the old `?panel=` form;
  - the pager's label and the grey Scheduled badge touch nothing else;
  - flow 06's Homes link no changed route.

| # | Sev | Finding | Fix |
|---|---|---|---|
| G1 | M | 4.51: the trainee's My committee reviews now prints "Formative check-in" (`FormativeTypeShortLabel` changed), but the Expect and Gap say "Formative". Comments in `ReviewRowNames.cs:16` and `RowNamesTests.cs:138` say the same | Reword the Expect and the comments |
| G2 | L | 2.23 says "David Naidoo and Sarah Botha between"; the list reads Botha, then Naidoo (picker order, then member id) | Name them in that order |
| G3 | L | 4.2 expects By EPA's Missed 5 but its Do never opens the fold | Add "open By EPA" |
| G4 | L | 5.2 presses "Schedule a review on Dr Molefe's line", but 5.1 ends on Every trainee, where no line shows | 5.2 filters to Molefe first |
| G5 | L | states.md: two scratch rows end "Withdraw that review after"; no such action exists | "Drop the scratch database after" |
| G6 | L (P) | coverage.md lists A.7.13 but not A.7.14 for `/committee/reviews/{ReviewId:int}` (predates T364) | Add A.7.14 |

## The fix pass

One fix pass on Opus (`t364-fix`: `fb6eb4d4`, `577e5c93`, `6a4711b6`).
- **Outcome:** all 20 findings fixed, none judged not a defect. Each code fix has a test that failed first, except
  D4's hand-choice test, which pins behaviour that already held.
- **R4** brought in `CommitteeRefusedException`, the marker the committee handlers' own refusals throw.
  `DecisionBodyLockedException` derives from it. The schedule page, a panel's page and Decisions due print only its
  message; any other `InvalidOperationException` gets the page's fixed sentence and is logged.
- **R3:** `ThenBySurname` falls back to the name shown for a blank surname, and serves the picker, Decisions due, the
  programme roster and curriculum coverage.
- **D1** is `order: 1` inside the reversed column (the review's `-1` would have put the reason last).
- **D7:** each Review type label is the type's name; its help is a description under it.
- **G1–G6** edited the runbook, coverage.md and states.md, then re-synced the briefs (424 verbatim steps, 0 problems).
- **Suites** after the fix, each with its own build:

  | Domain | Application | Infrastructure | Architecture | Web | Integration |
  |---|---|---|---|---|---|
  | 814 | 3782 | 1022 | 52 | 3636 | 491 |

  All passed, none failed or skipped. The solution build has 0 warnings.
