# Flow 04 · The build's review (2026-09-30)

Four read-only reviewers read `t350` after the integration (`b5c0e837`, all lanes merged, all six suites green). Their
lenses:
- **R**: the backend's correctness and security;
- **D**: the pages against the design and DESIGN.md;
- **A**: accessibility and focus;
- **G**: regressions outside flow 04, and how honest the tests are.

The ids are the reviewers' own. No high; six medium (R 2, A 3, G 1, and G's duplicates A's low A4); twenty lows. Every
medium and every low acted on was checked against the code first; the "plausible" ones (R1, R2, A2) were traced, and R1
and A2 were then shown by a test that fails without the fix.

## The fix pass (stopping line: every medium that holds, and every low that changes what a user sees, hears or can reach)

Branch `t350-fix`, from `b5c0e837`. Each fix is test-first; the tests named were mutation-checked (the fix undone, the
test fails, the fix restored).

**Medium**
- **R1.** Decided by you left out only the caller's own subject rows, so a supervisor who logged a procedure on a
  registrar's behalf (a type born terminal) read it as his decision, on the inbox and on Home.
  - Fixed: an activity whose last move is its create is left out, whoever created it (`DecidedByYou`), `3d8efbcf`.
  - Test: `ListDecidedByYouQueryTests.ACreateOnSomeoneElsesBehalf_IsNotADecision_EvenOnATypeBornTerminal`.
  - Changed deliberately: `AssessorDashboardQueryTests.AnAssessorWhoIsAlsoATrainee_…` (T203's) counted a create for a
    trainee as the caller's decision. Its row was a legacy Mini-CEX drawn born "completed", which that workflow cannot
    produce (it is born "requested"). The row is now a procedure the caller logged for a trainee, and it is not a
    decision. The rename is `…_OnlyWhatTheyDecidedForOthers`.
- **A1.** A change of page size moved the focus to the heading, so a keyboard user could not arrow from 20 to 50 to 100.
  - Fixed: only a page turn focuses the heading; a size change leaves the focus on the select, `4c048132`.
  - Test: `ActivityInboxTests.APageSizeChange_ReadsTheFirstPageAtThatSize_AndLeavesTheFocusOnTheSelect`.
- **A2.** Home's "All your decisions" (`/activities/inbox#decided-h`) landed on the h1: the heading does not exist
  while the page reads, so the browser's jump found nothing.
  - Fixed: once loaded, a `#decided-h` arrival focuses Decided by you, `4c048132`.
  - Tests: `ArrivingAtDecidedH_FocusesDecidedByYou_OnceLoaded`, and `ArrivingWithNoFragment_MovesNoFocus`.
- **A3.** A failed page turn replaced both sections and the pressed button with an alert, moved no focus and said
  nothing.
  - Fixed: `StatePanel.FocusFailureAfterRender`; the inbox asks for it when a page read fails, and the failure takes the
    focus as a failed Try again's does, `4c048132`.
  - Test: `APageTurnThatFails_FocusesTheFailure`.
- **G1 (= A4, low).** A refusal naming a field of a filled section was unreachable on a phone: the summary link
  focused the wide card, which is `display:none` below 641 px, and the fold's copy sat in a shut `<details>`.
  - Fixed, both halves, `0ca8f18a`:
    - A refusal naming one of a fold's fields opens the fold, so the refusal line is on the screen.
    - `wombat.focusById`, given an element that is not displayed, focuses its `-narrow` copy and opens its fold first.
  - Tests: `RatedLevelPickerTests.ARefusalNamingAFilledField_OpensItsFold` and
    `WombatJs_FocusById_FallsBackToTheNarrowCopy` (the script's text, as `PasswordFieldTests` checks wombat.js).

**Low (each changes what a user sees, hears or can reach)**
- **D5.** The Return result, "It is now Draft. Nothing else waits for you.", was the only result that named no act,
  and "Draft" read as the assessor's own.
  - Fixed: made by anyone but the author, a Return reads "Returned to Sipho Ndlovu. Nothing else waits for you."
    (`WaitingWords.ReturnedTo`), `db69ddd8`. The code names the registrar (`ActivityPageModel.SubjectName`, read before
    the move reloads the page), so the sentence says the act and whose it now is in round 2 C1's "<Sentence>. <tail>"
    pattern. D's longer proposal ("… It is now Draft. …") was not taken, because the status card straight below already
    says Draft, and three sentences in the result is one too many.
  - Test: `ActivityPageTests.AReturn_SaysItWentBackToTheRegistrar_ByName_NotTheStateItLeftBehind`.
- **D1.** The pager said "Showing 1-20 of 45" with a hyphen; DESIGN.md, the board and the other lists write an en dash.
  - Fixed: "Showing 1–20 of 45", `4c048132`.
  - "Per page:" keeps its colon and the sizes stay 10, 20, 50 and 100. DESIGN.md and the code agree on both; the board
    drew neither, and the component is shared by My activities and Decisions due. DESIGN.md now states both.
  - Tests: `PagerControlsTests.ItsWords_AreDesignMds`, and the three pages' pager assertions.
- **D2.** DESIGN.md § Pager was not one account, and the board's `nav` landmark did not exist.
  - Fixed: § Pager now gives the words, the aria-disabled ends, the page's focus duty (turn, size change, failure) and
    the name. `PagerControls` takes `Label` and is then a `nav` by that name: the inbox's is "Decided by you, pages".
    Unnamed, it is a plain block, as before, `4c048132`.
  - Test: `Named_ItIsANavLandmark_ElseAPlainBlock`, and the inbox's `Decided45_…` asserts the name.
- **D3, G6.** A running builder-authored move misspelt a doubling verb ("Refering…", "Admiting…").
  - Fixed: `FilingWords` holds a table of the seeds' and the likely builder verbs whose last consonant doubles. A verb
    outside it takes the plain -ing, the limit DESIGN.md now states, `16d029ec`.
  - Test: `MoveWordsTests.ABuilderMove_WhoseLastConsonantDoubles_IsSpelledRight` (and verbs that must not double).
- **D4.** Try again on a registrar's empty inbox moved no focus.
  - Fixed in `StatePanel`, so every page with a retry has it: a Try again answered by the empty state focuses that
    state (now `tabindex="-1"`, the region that replaces the error), `4c048132`. My activities' empty state had the same
    gap and is covered by it.
  - Test: `ActivityInboxTests.ARegistrarsTryAgain_FocusesTheEmptyState`.
- **A5.** The refusal reached the rung picker's radios only through the fieldset's description, and `aria-invalid` sat
  on each radio, where ARIA 1.2 does not permit it.
  - Fixed: the fieldset is `role="radiogroup"` and carries `aria-invalid`; each radio names the message (`<key>-msg`)
    while the field is refused, `0ca8f18a`.
  - Tests: `ARefusalThatNamedTheRatedField_LandsOnTheFirstRadio_AndMarksEachOne` (changed deliberately: the group is
    invalid, the radios are not) and `Unrefused_TheGroupIsNotInvalid_AndNoRadioNamesAMessage`.

**The runbook, the brief and the design system follow the pages:**
- Step 3.15's Expect (`act-3-operations.md`), its states.md row and the brief's quoted step now read "Returned to Sipho
  Ndlovu. …". The design system's ActivityWorkflowActions note follows too.
- states.md's paged-decisions row reads "Showing 1–10 of N", the focus still on the select. The PagerControls README
  and preview use the en dash.
- Dated Actual lines are left as they were written: act-3's replay of 3.15, and act-4's "Showing 1-20 of 75".
- DESIGN.md: § Pager; the result rule (`WaitingWords.ReturnedTo`); the running label's limit; the picker's radiogroup
  and message; the request fold's refusal.

## Filed, not fixed here
- **T351** (P3, Sonnet), "Flow 04 reads: the waiting clock restarts on a save, the reads' cost, and no way on for a
  creator":
  - **R3.** The overdue clock restarts on an assessor's own save. It is user-visible: a seven-day request stops being
    overdue and drops to the bottom. But `UpdatedOn` is the one clock the Coordinator's stall and the assessor nudge
    read too (note 10). Moving only the inbox would make the three disagree, so the fix has to move all three together.
    The scenario never saves a waiting request without a move.
  - **R4, G2.** Home's card reads every decision and every waiting row with transitions, names and forms, to show five
    of each; the way on and the other-role line read the whole list for a count. Performance.
  - **R5.** A creator who is also the named assessor gets flow 03's inbox sentence, not the way on. Deciding by arm
    needs the server to say, for each action, which arm admitted the viewer. The case is not in the scenario.
- **T352** (P3, Sonnet), "Flow 04 polish: a dead naming helper, two test gaps, and small screen-reader and wording nits":
  - **G3.** `ActivityRowNames.For(withSubject)` has no production caller.
  - **G4.** A widened lookup assertion.
  - **G5.** Lost page-level coverage of a departed registrar's name.
  - **R6.** One-letter and mixed-case words in `LabelFor`.
  - **A6.** Descriptors are read twice in browse mode.
  - **A7.** The loading statuses are born filled.
  - **A8.** Headings sit inside `<summary>`.
  - **A9.** Quick arrowing flickers, mention only.

## Dropped
- **R2** ("has a move left" ignores whose move it is). It is the accepted definition, not a defect. Round 1's Q1 and T297
  say a decision is "moved last by you, and finished or with no move left"; a state left waiting on its author is "back
  with its author". A Return to draft is the case the class doc names, and the test `AReturnToDraftIsNotADecision…`
  pins it.
  - Checked against every seeded workflow: the only non-author move into a state with a move left is such a return
    (`return` in the reflection, audit and portfolio review; the demo reflective note's `decline`, which the subject
    revises).
  - No workflow has an assessor move that leaves the next move to someone other than the author.
- **D's judgement 2** ("Nothing to discard yet." on the trainee's draft). D found it consistent with the design, not a
  regression.
- **D's judgements 3 and 4.** Covered by D1 and D2.
