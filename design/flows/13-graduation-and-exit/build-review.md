# Flow 13 build: the review (T379 step 6)

The build on `t379` (lanes R, E, V, P and the runbook lane merged; HEAD `95d3f765`) was reviewed 2026-10-27 from three
sides, each a read-only Sonnet agent: **the code** (`build-review-code.md`, B1–B9: correctness, data, scope and
authorization), **the pages** (`build-review-pages.md`, P1–P11: the boards, the design system, accessibility) and **the
runbook and cast** (`build-review-runbook.md`, R1–R11). The three files are kept as they are; this one merges them.
Severity: M must-fix, S should-fix, N nit. No reviewer ran the six suites; the runbook reviewer ran the Scenario tests
(49 passed) and `check_verbatim_steps.py` (0 problems).

Two should-fixes were the integrator's to decide, and were decided before the fix pass: **P1**, build "Your record:"
under the ended notice, as the boards draw it, through an optional fragment on `EndedRecord`; **P2**, keep Spec 3's
empty-card words for a reader with no Trainee role, drop the empty copy block, and correct DESIGN.md's citation.

## Must fix

None.

## Should fix (all taken into the fix pass)

- **B1** `LastRatifiedDecision` names a closed formative review: `CommitteeReview.Close` stamps `RatifiedOn`, so the
  reader must leave formative reviews out, as its comment says.
- **B2** the trainee profile's press-time day check reads `_today` cached at the page's read: a page left open past SAST
  midnight refuses today's date with yesterday's named. Read the South African today at the press.
- **P1** My progress draws "Your record:" above the ended notice; R3-R01, R3-R02 and R3-R10 draw it under, and DESIGN.md
  does not own the difference.
- **P2** My authorisations' empty card for a former trainee follows Spec 3's Empty row, but DESIGN.md cites R3-R09 (which
  draws the built words); and `StatePanel` draws an empty `div.state-panel-copy` for an empty body.
- **P3** My activities draws `PagerControls` for any non-empty list; R3-R07's seven rows draw none, and every flow 12 and
  13 list guards it with `TotalCount > PageSize`.
- **P4** the profile's Profile details form keeps `DataAnnotationsValidator` and a `ValidationSummary`; its refusals are
  not under their field, unlike every other flow 13 form and DESIGN.md's rule.
- **R1** Step 4.33 says Molefe's review "offers Smit no action"; since T379 its header offers the export.
- **R2** Steps 5.28 and 6.41 give the "counts towards nothing" note row to the Mini-CEX alone; the code draws it under
  every row observed after the last day (the cancelled DOPS and the open Portfolio review too).
- **R3** A.7.12 and `states.md`'s `verify-export--narrow` expect the hash "in the field wrapping", which a text input
  cannot do; and A.7.12's Do does not say which file it verifies.
- **R4** `states.md`'s `shell--no-role` gives the former trainee's menu without Export portfolio.

## Nits (taken where they cost a line)

B3 (the graduation email's link is relative when `Wombat:BaseUrl` is unset), B4 (the profile reads the full standing
for `AnyIssued`), B5 (Verify loads every row of a hash), B6 (a pasted zero-width space or BOM is named as an invisible
character), B7 (the take-back count includes credit later reversed), B8 (a failure after the end's commit reads as a
refusal), B9 (the last decision is read at any institution); P5 (Export's dates name the help before the refusal), P6
(the last-day input has no `aria-required`), P7 (the has-note phone rule is not scoped to its page), P8 (the Verify
field is 14px, so iOS zooms), P9 (the DESIGN.md banner's flow 13 entry has no commit), P10 (Verify's result link is
20px at 390 and DESIGN.md does not name it), P11 (the closed list's empty card has no focus target); R5 (3.54b and
A.7.8a meet the export on a registrar's page), R6 (`verify-export--pasted` asks for a pasted line break in a one-line
field), R7 (A.1.14's lead sentence), R8 (5.17's quoted flow 12 user-page sentence omits the export), R9
(`EPA-PROGRAMME.md:311`'s 90-day portfolio-export constant), R10 (the README cast's Acts column), R11 (5.22's Route,
nothing to fix).

## Held (checked and fine)

Authorization and scope on every new read and command (`GetExportSubjectQuery`, the three DTOs' `MayExport`,
`GetTakeBackForecastQuery`, both end commands and their shared `EndDayRefusal`, `ListEndedProfilesQuery`,
`GetTraineeProfileById`, `GetFormerTraineeHomeQuery`, `TraineeOrFormerTrainee`, `ListActivitiesBySubject`'s own-list
fields, `VerifyExport`'s parse and encoding); the `T379_PortfolioExportCleanupRetired` migration with its Designer and
an untouched snapshot; `GraduationEmail`'s encoding. The pages' words against the boards and § 6; one h1, h2 sections,
captions; outline triggers, the filled danger only in `ConfirmDialog`; 44px targets at 390 bar the named exceptions; no
inline style, raw hex or undefined class; DESIGN.md paragraph by paragraph bar P1's and P2's. Every quoted Expect the
runbook lane added is what the code prints; the cast, counts, routes, step order and journeys; no Actual rewritten.

## The fix pass (`t379`, 2026-10-27)

Each finding was checked against the code first; all the should-fixes held (B1: `Close` sets `RatifiedOn` at
`CommitteeReview.cs:530`; R2: `ObservedAfterEnd` is set for every row of the caller's own list whatever its state, and
the cancelled DOPS is dated `D`, the open Portfolio review `D−1`, both after `D−2`; the CBD at `D−5` and the Teaching
Session Logs are before it).

- **Fixed:**
  - **B1** `LastRatifiedDecision` adds `&& !IsFormative`, its comment saying why; a test: a closed formative review newer
    than a ratified summative one, and the summative one is named.
  - **B2** `AskAsync` reads `QuotaCalendar.Today(Clock)` at the press; `_today` is the default's only. A page test moves
    the pinned clock a day after the read and the new day is accepted, the forecast asked for it.
  - **P1** `EndedRecord` takes an optional `AfterNotice` fragment, drawn straight after the notice with no whitespace of
    its own, so a caller that passes none renders the same bytes (the five golden files hold); My progress passes the
    record line there. DESIGN.md says where it sits and why; the test checks notice, then line, then Curriculum targets.
  - **P2** Spec 3's words kept; `StatePanel` draws no `.state-panel-copy` for an empty body (a leading space keeps the
    card's text as it was, which an MSF test caught); DESIGN.md and the page's comment cite Spec 3's Empty row, R3-R09
    drawing the built words; the test asserts no copy block.
  - **P3** My activities draws the pager only when `TotalCount > PageSize`; a test with seven rows finds no pager (the
    45-row test still pages).
  - **P4** the details form is `OnSubmit` with `novalidate`: no validator, no summary; Curriculum and Programme start date
    are plain controls with `aria-required`, and a refusal ("Choose a curriculum.", "Enter the programme start.", Admit's
    words) is a `p.validation-message` under its field, the field `aria-invalid` and described by it, the first one
    focused; a changed field clears its refusal. The model's two values are nullable, so a cleared date reaches the
    page's check instead of a parse error. A test clears the start date and submits. P4's held question (whether
    `InputDate` writes `aria-invalid`) no longer arises: the page's own inputs do.
  - **R1–R4** 4.33 (no Ratify or Start, only the header's export); 5.28 and 6.41 (the note row under each of the three
    rows, named, and under no other); A.7.12 (the Do names the file from Step 5.23; the ids wrap in the answer, the hash
    stays on one line in its field) and `verify-export--narrow` likewise; `shell--no-role` (Export portfolio in the menu).
  - **Nits:** B6 (U+200B and U+FEFF dropped with whitespace in Verify's parse; a test case); P5 (Export's dates name the
    refusal first, then the help; the test updated); P6 (`aria-required="true"` on the last day); P7 (the rule is
    `.my-activities .clinic-table--stack tbody tr.has-note`, the list section carrying `.my-activities`; DESIGN.md's
    owner row updated); P8 (`.verify-page .form-control.hash-value` 1rem at ≤640px; DESIGN.md's owner row); R5 (3.54b
    and A.7.8a name the header's export); R6 (the line break typed into the address as `%0A`); R7 ("users or current
    trainees, and no row names him"); R9 (the clause says T379 retired the portfolio-export retention); R10 (the cast's
    Acts column from the Role lines: Mokoena and Sithole add the appendix, Molefe and du Plessis read 2–6 and the
    appendix, Dlamini, Mahlangu and Ndlovu add 6 and the appendix). The flows' verbatim copies of 3.54b, 4.33, 5.28, 6.41,
    A.1.14, A.7.8a and A.7.12, and the appendix line references after them, were re-synced (`sync_verbatim_steps.py`;
    424 steps, 0 problems).
- **Left:**
  - **P9** no flow's banner entry carries a commit (flows 11 and 12 included); the squash commit names itself when the
    integrator lands it, and a placeholder would be the only one in the list.
  - **B3** operator configuration, HTML-encoded, the same pattern as the invitation and account links: no regression.
  - **B4, B5** one extra read on one page, a handful of rows per hash: fine at this size.
  - **B7** T281's rule, shared by the forecast and the commands, so they cannot disagree; changing it is not flow 13's.
  - **B8** a pre-existing ordering (the role write after the commit); a re-read in the catch is a change to the end
    commands' error path, not a line.
  - **B9** names nothing, gates nothing, and the profile read is scope-checked.
  - **P10** an inline link in a sentence, WCAG 2.5.8's exemption, named on the boards (round-3-check A3).
  - **P11** no rows, no page turn, so no focus is lost; no action, as the reviewer says.
  - **R8** flow 12's sentence, quoted as the code prints it; a change to it is flow 12's owner's.
  - **R11** nothing to fix.
