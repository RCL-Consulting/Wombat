# Flow 04 · Round 2 review (2026-09-30)

Four reviewers (T tokens and CSS; B the build against the code; S states, copy and the cast; A accessibility) read
round 2 (`round-2/`, canvas version `1790766420-7fb9`: three component boards, 80 state wrappers, Spec and Steps)
against `round-2-ask.txt`, `round-1-review.md` (accepted whole: A with corrections a–d, Q1–Q7, E1–E5, R1–R4), the
seeds, the runbook and the code at `3fea34ac`. This is the synthesis; every must-fix was checked against the source it
cites. Ids in brackets are the reviewers' own.

**Verdict: accept with changes.** All 40 states are drawn at 1280 and 390, each wrapper matches its name, and every
round-1 decision is drawn as decided [S, B holds]. The six descriptors, the feedback labels, the seeds' field orders
and the status card's words match the seed and the build. No literal colour, no undefined class, and every contrast
figure on the Spec recomputes [T holds]. Nothing promises an email. What needs a correction round is:
- **two result sentences the code does not produce** ("It is now Discussed.");
- **cast rows that are not the runbook's** (Dr Zulu's decisions, 6.18's and A.7.2's requests, 3.26's way on);
- **About rows and built words redrawn without a decision**;
- **WaitingList drawn as a second NeedsYouList**;
- **four accessibility gaps**: the radios' identity, 28 px buttons at 390, a fold that always says Show, and no focus
  rules for the new moves.

## Corrections for the canvas (round 3)

- **C1. A move into a final state reads as that state.** `MoveOutcome.ResultSentence` (MoveOutcome.cs:43-50) gives
  "Discussed." and "Signed off.", not "It is now Discussed."; "It is now Draft." is right for Return, which leads on.
  Draw "Discussed. Nothing else waits for you." (R2-C-Activity:557), give the review's "Signed off. …" on the Spec, fix
  Steps 3.17, and delete the Spec's "to confirm". [B1, S5]
- **C2. Two assessors had more waiting in 3.26 and 3.28.** Dr Patel also held the portfolio review, Dr Khumalo the CBD
  (runbook 3.26 and 3.28 Actual). Their completions read "Completed. 1 more waits for you." with that row as the way
  on; Naidoo, Botha and Zulu read "Nothing else waits for you." Fix both Steps rows. [S6]
- **C3. The cast, to the runbook.**
  - Dr Zulu's Recent decisions, newest first: CBD (Paediatrics) · PAED-001 · 2026-09-21 (Anele Dlamini, 08:50);
    Direct Observation (Paediatrics) · PAED-010 · 2026-09-26 (Lerato Molefe, 08:45); Mini-CEX (Paediatrics) ·
    PAED-001 · 2026-09-18 (Lerato Molefe, 08:44) (3.25, 3.27). Spec § 7 stops calling them "mine". [S1]
  - Dr Mahlangu's Mini-CEX (3.29) plays after 3.28: Create 08:53, Submit 08:55, and "since 2026-09-22 08:55"
    everywhere, Steps 3.33 too. Du Plessis's CBD (3.21) plays before the 08:06 review: its "since" is before 08:06,
    not 09:05. [S2, S14]
  - 6.18's request is Outpatient clinic, "Counselling the parents of a toddler newly diagnosed with type 1 diabetes"
    (act 6, 6.16). A.7.2's setting is Ward (A.7.1). Both leave Spec § 7's "mine" list. [S3, S4]
  - Steps 3.30 says once that the ageing leaves a Submit dated before its encounter (09-22 for a 09-27 Mini-CEX; the
    review filed 09-22 for a period to 09-29), and that the review's "filled in 2026-09-22" is E5's consequence.
    The volume board states its figures: a 31-character name, a 108-character EPA (PAED-015). [S2, S14]
- **C4. About as built.** Every type's date row is "Encounter" (`ActivityPageModel.About`, :384; runbook 3.22 quotes
  it): "Encounter 2026-09-10" for the reflection, "Encounter 2026-09-29" for the review, not "Case or incident" or
  "Period". The review's Credit is "None: a portfolio and logbook review credits nothing" (the type's noun, :439).
  Nothing decided a change; a per-type date label would change flow 03's pages too. [B4, S7]
- **C5. Built parts and words where no decision changed them.**
  - The pager is PagerControls as built: "Showing 1–20 of 45", Previous, Next and the "Per page" select. [B11, S15]
  - The declined page's locked sections keep the built value "Not filled in." and the pending RungRow
    (ActivityForm.razor:47-54), not "Not filled in: the request was declined." [B15]
  - The review form carries the seed's help under Date of the review, Evidence reviewed and Review comments
    (portfolio_review_cpsa/schema.json:66, 73, 87). [B16]
  - Straight apostrophes in app strings ("nobody's", "This activity's EPA is paused", the note help); curly only in
    the College's seeded text. [S12]
  - Steps' "was" for the inbox subtitle is the built "Work waiting for you to rate, review or record."
    (ActivityInbox.razor:63). The Home card rename is quoted wherever 6.18 and A.7.2 name "Waiting for your rating". [S13]
- **C6. WaitingList is NeedsYouList with an overdue edge.** Draw the rows with `.needs-you`, `.needs-you-row` (padding
  `space-sm`, as built), `.needs-you-rule`, `.needs-you-why`; add only `.needs-you-row--overdue` (the warning stripe)
  and `.needs-you-badges`. Home's card and My activities' then share one row. The Spec's new-class table shrinks, names
  `.waiting-more` and `.card-empty` (or uses `.text-muted`), and says the beside-the-button reason is
  `.move-reasons--beside`, a modifier of the built `.move-reasons`, not a new class. [T1, T6, T7, T14]
- **C7. The rung picker keeps its radios.** One `label` and one `input` per rung, `is-chosen` and `checked` as
  conditions on it, the `li` keyed (`@key`) so a round trip does not replace the focused radio. The Spec adds: arrow
  keys move and choose; Tab leaves the group; with none chosen Tab lands on the first. Each radio's description is a
  visually hidden span outside the `<details>` (the legend stays for the eye); the picker's "chosen" is
  `aria-hidden` (the browser says "checked"). The Spec states the 3.5rem cell (a radio above a 1.1rem label) and keeps
  the native radio with `accent-color`, its ring the browser's (4.54), as a sentence for DESIGN.md. [A1, A5, A6, T5, T10]
- **C8. 44 px on a phone.** Lift Home's "Open Activity inbox" and "All your decisions", the three "Try again", and the
  pager's Previous and Next to 2.75rem below 641 px; the note panel's Send and Keep stack at 390, 44 px, the send on
  top. List them on the Spec. [A2]
- **C9. The fold says Hide when open.** Two spans, "Show" and "Hide", one hidden per `details[open]` with
  `display: none`. [A3]
- **C10. Focus rules on the Spec.** (a) Decline or Return opens the panel: focus to the note. (b) Keep: focus back to
  the toggle, `aria-expanded="false"`. (c) Refused: `#note-summary` takes focus, its link targets `#note-in`, the panel
  stays open with the note. (d) A completed move: focus to `#activity-result`; the next Tab is Open the next. (e) Try
  again: focus to the region that replaces the error. (f) A page of Decided by you: focus to its h2; Previous on page 1
  is `aria-disabled`, not `disabled`, so the pressed button keeps its place. [A4, A15]
- **C11. Announcements and names.** Home's loading state has a `role="status"` "Loading your Home." like the inbox's
  and the activity's; every such status (and the move bar's busy one) is present before its words arrive. The
  alike-rows rule (", Requested", ", waiting since … SAST", "(1 of 2)") holds wherever WaitingList renders: Home, the
  inbox and the way on. The Spec marks "Loading the Activity inbox." as new words. [A11, A12, S16]
- **C12. The other-role line's other states.** Draw, or give specimen text for, the line with nothing overdue (the
  info tint) and with several (per E4's words), and say that a failed read of the line shows nothing and is logged:
  it never becomes a Home error, which DashboardFrame owns. [S11, B18, A16]
- **C13. The Spec's tables.** The way on's hairline is 1.24 on the page (1.30 on the surface). Add three rows: the
  other-role line's link on its tint (6.31 on warning-bg, 6.28 on info-bg); muted text on the page ground (4.83) beside
  the surface's 5.09; the locked head's muted text on header-bg (4.57). The stacked inbox hides its thead with the
  built clip rule (app.css:2659), never `display: none`. [T8, T9, A13]

## Decisions (put to the operator)

| # | Question | Recommendation |
|---|---|---|
| E1 | Overdue: whole days on the boards (`days > 7`, overdue from 8), 7 × 24 h in the code (`AssessorDueDays`, GetAssessorDashboardSummaryQuery.cs:62); the ask said both [B3] | Keep the code's 7 × 24 h, the one clock the dashboard and the stall share. Word the rule "Oldest first. Overdue once it has waited 7 days.", the number read from `AssessorDueDays`, and draw `days >= 7`. A whole-day count of 7 then always carries Overdue; no cast row changes (8 days) |
| E2 | How a fold is open at 1280 and shut at 390 on a server-rendered page (the request fold; the legend) [B6, A5] | Two renderings toggled by CSS at 641 px, no script: both are read-only, the hidden twin's ids are suffixed, and the radios' descriptions sit outside both (C7). The alternative is a small `matchMedia` script under the CSP nonce, which fails before the circuit starts |
| E3 | Which fields get the rung picker. The demo types have up to six `scale` fields on five-rung ladders; only the rated field's ladder is loaded (ActivityForm.razor:210) [B7] | The rated level field (`rated_level_field`) only, where its ladder loads; the row is as wide as its ladder; one legend per picker; any other scale field, or a ladder that fails to load, keeps the select. R4 reads so |
| E4 | "As an assessor, …": the waiting read counts every non-author arm (`ActorArms.NotAuthor`), `role:` and `scope:` moves too, so the role is not always true [B9, S11] | Keep E5's one read and word the line without the role: "1 activity waits for you in the Activity inbox, and it is overdue: <name>, from <registrar>, waiting 8 days." / "3 activities wait for you in the Activity inbox; 1 is overdue. The oldest: …" / nothing overdue: "…, from <registrar>, waiting less than a day." Narrowing the read to assessor arms is new code for no gain in the cast |
| E5 | "And 20 more." is a fragment and says not where [S10, A14] | "20 more wait in the Activity inbox." (singular "1 more waits in the Activity inbox."), above "Open Activity inbox" |

## The operator's answer (2026-09-30)

**Accept all**: C1–C13 go to the canvas as written, and E1–E5 as recommended. E1: keep the code's 7 × 24 h; the rule
line reads "Oldest first. Overdue once it has waited 7 days.", its number from `AssessorDueDays`; the boards draw
`days >= 7`. E2: two read-only renderings toggled by CSS at 641 px, no script, the hidden copy's ids suffixed. E3: the
rung picker only on the rated level field whose ladder loads; other scale fields keep the select; R4 reworded. E4: the
other-role line drops the role ("1 activity waits for you in the Activity inbox, and it is overdue: …", with its
several and not-overdue wordings). E5: "20 more wait in the Activity inbox." / "1 more waits in the Activity inbox.".
The build notes and nits stay for step F. Round 3's ask is `round-3-ask.txt`.

## For the build (step F), not the canvas

These draw words or behaviour flow 04's build changes on purpose; the boards are right.
1. **E1 (round 1): sentence case** in `WorkflowTransition.LabelFor` (WorkflowTransition.cs:24-30): bar, history,
   reasons and "Decline requires a note." (ActivityService.cs:389); tests that quote Title Case change. [B]
2. **Running words:** `FilingWords.Running` (FilingWords.cs:97-113) gives "Working…" for two words. Add the first word's
   -ing form with the rest kept ("Recording discussion…", "Signing off…"); keep "Working…" for a word it does not know.
   The hidden status follows (ActivityWorkflowActions.razor:85). [B8, S9]
3. **"Not returned. It is still Awaiting discussion.":** `RefusalWords.NotDone` gives "Return was not made." for a move
   that leads on (RefusalWords.cs:48-58); add "Not returned." for `return` (Record discussion and Sign off are final and
   already read "Not discussed." / "Not signed off."). The note panel's summary title adds " It is still <state>." as
   `ForMove` does (:30); today it prints `NotDone` alone (ActivityWorkflowActions.razor:97). [B2, S8]
4. **The note panel's words (E2, correction 8):** heading "Decline this request" / "<Move> this <part>", keep "Keep the
   <part>", the part being the first word of `AuthorPart` ("request", "reflection", "review"); label "Note for <name>"
   for both moves. Today: "{Label}: add a note", "Do not … it", "Reason for", "Note to" (:273-280). [B5, S8]
5. **ListWaitingForYouQuery:** wraps `ActivityWaiting.LoadActionableAsync(…, NotAuthor)`; oldest first (today newest,
   ActivityWaiting.cs:97); the own-subject filter moves in from the dashboard (:72); Home takes 5; names through
   `ActivityRowDetails.ResolveAsync` with `sharesTheRest: false`, or E7's `WithNominee` appends the caller's name. [B]
6. **Decided by you:** "finished or no move left" is judged per pinned workflow in C#, so the pager's total reads every
   activity the caller moved last before slicing; fine at an assessor's scale. [B12]
7. **The other-role line** is its own component in Home.razor between PageHeader and the role switch (Home.razor:17-22),
   shown when `Acting.Holds(Assessor) && Acting.Role != Assessor`. [B]
8. **SAST:** the inbox and the row names use `ToLocalTime()` (ActivityInbox.razor:44, ActivityRowNames.cs:24); use
   `ActivityMoments.When` and `ActivityListWords.DateOf`. [B13]
9. **Alike rows:** `ActivityRowNames.For` breaks ties with ", updated <local time>" on the Open button; the decided
   words go on the link. [B14]
10. **Two "since" clocks:** the row reads `UpdatedOn`, the status card the last move's `OccurredOn`; they part after a
    data write without a move. State it, or read the row's "since" from the last move too. [B19]
11. **Discard's reason beside it** is new (Discard has none today, :158-167); keep `aria-describedby`. [B20]
12. **The inbox page:** subtitle, empty body, columns (Activity, EPA, State, Waiting) and the name as the link
    (ActivityInbox.razor:32, 43, 73); `ActivityLink` gains "from <registrar>" (ActivityListWords.NomineeLine:26). [B21]
13. **The count in the badge:** DashboardCard badges a figure and reads words (`CountWords`, :59-88); the words badge
    ("2 waiting, 1 overdue") is a new mode. [B17]
14. **The Overdue badge:** `BadgeFor` forbids an undefined badge class (DefinedClassTests); reuse the built warning tint
    (`BadgeFor.State(BadgeState.Accepted)`, as Home draws Overdue today) or add `.badge-overdue` to that rule. [B10, T13]
15. **E4 (round 1): person fields by name** through `NomineeNames.NameOf`, not the option's "name (email)". [B]

## Nits, for the build

`.decided-meta`'s dead `align-items` [T2]; `.way-on`'s gap `var(--space-md)` and the 2 px gaps `var(--space-xs)` [T3,
T4]; one of `--emphasis`/`--warning` per card [T15]; drop the visually hidden "required" where `aria-required` is set,
and never emit an empty `aria-describedby` [A7]; the note's `aria-describedby` is `note-help note-msg`, not the summary
too (built so today) [A8]; `aria-controls` only while the panel exists [A9]; a short name for the status card's region
[A10]; the stacked inbox's Activity, State and Decision cells have no `data-label` (fine) [A13].

## Dropped

- **T11** (sentence case contradicts flow 03's C3): round 1's E1, accepted, moves labels to sentence case in
  `LabelFor`; "Record discussion" and "Sign off" are as decided (build note 1).
- **B5's subtitle half:** the code already gives "Pieter du Plessis's request to Mohammed Patel" for the review
  (`AuthorPart` "review request" contains "request", ActivityPageModel.cs:153); only the panel's noun is real (note 4).
- **S11(a)** ("overdue" said twice): the ask's line says "overdue" once; "waiting 8 days" is the age Q3 asks for.
- **Reclassified, not dropped:** B2, B8, S8, S9, B13, B14, B17, B10/T13 are build notes, not canvas corrections: the
  boards draw the decided words.

## What holds

- Every state at both widths; wrappers match; nothing scrolls sideways at 390; the six rungs sit abreast. [S, A]
- Corrections a–d: no switch in the line; "Back to" by the owner table (`NavOwners.OwnerFor`); the clock is `UpdatedOn`;
  the live region holds the sentence only, the way on outside it. [B, A]
- The menus (the committee's has no Decisions due), the trail from the line, and opening an activity without switching
  (the act gate reads every held role). [B]
- Q7: no year minimum and no semester count on any assessor page; "Rated 4. Credited 1 item to PAED-001." [S, B]
- The seeds' labels, orders, sections, scale legends, descriptors and help (but C5's three); the moves and states the
  workflows declare; Decline validates formats only. [B, S]
- Tokens only; type on the scale; 44 px as `2.75rem`; flow 03's parts reused (but C6). [T]
- Headings, landmarks, the note panel's labelling, `role="alert"` load errors, `aria-disabled` on the running and the
  greyed buttons, Overdue in words beside the state. [A]
