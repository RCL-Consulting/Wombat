# T350 step 6 — rules common to every build lane (flow 04, an assessor works their inbox)

Adapted from `design/flows/03-trainee-files-activity/build-lanes.md` (T342). You are one lane of the build of flow 04 of
Wombat's GUI restructure, task T350 (`execution/tasks/in_progress/T350-*.md`). The design is final: Claude Design canvas
round 3 (version `1790768786-6b8d`). The lane plan is at the end of this file; the names two or more lanes share are in
`build-contracts.md`, beside it.

## Read first (in your worktree)
- `design/flows/04-assessor-inbox/README.md` — the design record. `design/flows/04-assessor-inbox.md` — the brief (the
  cast, the 17 runbook steps, what is wrong today).
- `design/flows/04-assessor-inbox/round-3/project/` — the accepted boards. The substance is in `R3-C-Home.dc.html`,
  `R3-C-Inbox.dc.html` and `R3-C-Activity.dc.html`: each is one component with a `state` prop. Read its markup and its
  `renderVals` script, which holds each state's data and words. The `R3-H-*`, `R3-I-*` and `R3-P-*` files only pick a
  state and a width.
  - `R3-Spec.dc.html` is the contract: every message by kind and its words (§ 1), the DESIGN.md rules R1–R4 ready to
    paste (§ 2), the NEW components and their classes (§ 3), focus (§ 4), 44 px on a phone (§ 5), the contrast figures
    (§ 6), the reads (§ 7) and the shell (§ 8).
  - `R3-Steps.dc.html` lists the runbook Expects that change.
  - `flow04-r3.css` (and the older `flow04*.css`, `flow03*.css`) are the canvas's proposal for the new classes. They are
    a picture, not code to copy: rebuild them in `app.css` on the spacing scale, reusing the built classes the round-2
    review names (C6: `.needs-you`, `.needs-you-row`, `.needs-you-rule`, `.needs-you-why`; `.move-reasons`), with the
    phone rules scoped to these pages. One board slip: the inbox's cell draws `.waiting-badges`, which no stylesheet
    defines; it is `.needs-you-badges` (`build-contracts.md`).
- `design/flows/04-assessor-inbox/round-1-review.md` — the pick (A, "Two pages, one list", with corrections a–d), Q1–Q7,
  E1–E5 and R1–R4, all accepted.
- `design/flows/04-assessor-inbox/round-2-review.md` — corrections C1–C13, decisions E1–E5 (all accepted), and
  **§ For the build** (build notes 1–15) and **§ Nits, for the build**: your lane's items are listed below by id, with
  the reviewers' evidence (file:line) in the review.
- `design/flows/04-assessor-inbox/round-3-check.md`.
- `design/flows/04-assessor-inbox/build-contracts.md` — the names you provide or rely on. Use them exactly.
- `execution/architecture/DESIGN.md` — mandatory before touching any Razor or CSS. CLAUDE.md — conventions and
  footguns, and the Activity platform section (the act gate, `ActorRuleMatcher`, `FieldPermissionEvaluator`).
- Flow 03's `build-contracts.md` and `build-review.md` — the vocabulary flow 04 builds on (`ActivitySummaryDto`'s row
  fields, `ActivityLink`, `NeedsYouList`, the move words, the status card).

## Where you work
- Only in your own worktree directory (given in your prompt), on its branch. Use absolute paths for every command.
  - Never touch `C:\Users\Renier\Wombat` itself (master).
  - Never merge, push or switch branch.
- Commit in your worktree as you go.
  - Write each message to a file and use `git commit -F <file>`.
  - End every message with the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Other lanes run in parallel on other branches. Stay inside the files your lane owns (the plan below). If you must touch
  a file another lane owns, keep the change minimal and name it in your report. A wave-1 lane that changes a shape the
  Web project reads keeps the Web project compiling with the least change there (as T342's lane A1 touched
  `MyActivities.razor`); wave 2 rewrites the page.

## Never
- Touch the apps running on ports 5080 and 5180, or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any admin credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves; each works in its own throwaway `it_<guid>` schema, which is expected.
- Pass `--no-build` to `dotnet test` or `dotnet ef`.
- Upgrade MediatR, add Bootstrap/MudBlazor/Radzen/jQuery, put raw hex colours outside `:root`, or use `<i class="bi">`.
- Load anything from a third-party host at runtime (the CSP allows only 'self'); no inline scripts or `onclick`. The
  folds that are open at 1280 and shut at 390 are two renderings toggled by CSS at 641 px, never a script (E2).
- Name a test helper `StyleSheet.cs` (it is the same file as `Design/Stylesheet.cs` on Windows).
- Change a seed's field order, labels, help or descriptors. The move labels change in `WorkflowTransition.LabelFor`, never
  by a DSL `label` (round 1, E1).
- Show the rater the registrar's year minimum or a semester count, before or after rating (Q7).
- Print an exception's text on a page: load errors use the fixed words, and the failure goes to the log.

## How
- Test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density. This codebase comments *why*, with task ids: cite
  T350 and the reviews' ids, e.g. "(T350, note 5)", "(T350, C7)", "(T350, round 1 E3)". Round 2's build notes are
  "note 1"–"note 15"; its nits by the reviewer's id ("T2", "A8").
- The audit pipeline commits a failed handler's staged rows (`AuditPipelineBehavior`): do every throwable check before
  any mutation. Flow 04 adds reads only; no lane adds a command.
- Application handlers go through `IApplicationDbContext`, never EF types. Web components use `IScopedSender` and DTOs,
  never Domain types in `.razor`. A handler that reads `Set<Activity>()` must satisfy `ActivityReadBoundaryTests` and
  `DashboardStateLiteralTests` (no literal state keys).
- Times shown are South African, with the zone: `ActivityMoments.When` ("2026-09-30 07:58 SAST") and
  `ActivityListWords.DateOf`, never `ToLocalTime()` (note 8).
- Straight apostrophes in app strings; curly ones only in the College's seeded text (C5).
- Change a test deliberately, never loosen it to pass. Keep the Design tests (`tests/Wombat.Web.Tests/Design/*`)
  truthful: `DefinedClassTests` fails a class a page uses that `app.css` does not define, so a class is defined by the
  lane that owns it (the plan) and used by the others only after the merge that brings it.
- Each lane adds its classes under its own `/* Flow 04: … (T350) */` heading in `app.css`, and its DESIGN.md rule under
  the section the plan names; the integrator makes the account one after the merge (as T342 did in `3c239246`).
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`: the integrator's
  runbook lane does both after merging. T294's guard (`tests/Wombat.Web.Tests/Scenario`) must stay green. If a new route
  makes it fail, add the route to `coverage.md` § Pages only, and say so. Name in your report every runbook line whose
  Expect your change alters.
- Before finishing:
  - `dotnet build Wombat.sln -c Release` clean, with no new warnings.
  - Then all six suites, each with its own build: Domain, Application, Infrastructure, Architecture, Web, Integration
    (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message is returned to the integrator, not shown to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and anything another lane or the integrator must know: the contracts you provide or rely on
  (exact type, member and parameter names, and any way they differ from `build-contracts.md`), files you touched outside
  your list, runbook steps whose Expect your change alters.

## The lane plan

Five lanes in two waves, as T342 ran (`t342-lists`, `t342-filing`; then `t342-lists2`, `t342-page`, `t342-log`).
Worktrees under `.claude/worktrees/<branch>`. Integration branch **`t350`**, cut from master; each wave's lanes are cut
from `t350` and merged back into it (`Merge lane <X> (<branch>)`), and the whole is squashed into one commit on master.
Ownership is disjoint: a file is in one lane's list, and a test file goes with the code it tests unless named.

### Wave 1 (parallel): the reads and the words

**Lane A1 "waiting"** (`t350-waiting`). The one waiting read Home and the inbox share, "Decided by you", and the row that
shows them.
- Scope:
  - `ListWaitingForYouQuery` over `ActivityWaiting.LoadActionableAsync(…, ActorArms.NotAuthor)`, as a static reader
    (`WaitingForYou.ReadAsync`) that the query, the dashboard and the other-role line all call. Oldest first by
    `UpdatedOn` then `Id`; the caller's own subject rows left out (moved in from the dashboard, E5); overdue at
    `now − UpdatedOn ≥ AssessorDueDays × 24 h` (E1, so a whole-day count of 7 always carries Overdue); the waited whole
    days as the nudge counts them; names through `ActivityRowDetails.ResolveAsync` with `sharesTheRest: false`, so E7's
    nominee suffix never appends the reader's own name (note 5); the EPA and encounter date the inbox carries today.
    "Now" from `TimeProvider`.
  - `ListActivitiesByActorInboxQuery` is retired: the inbox sends `ListWaitingForYouQuery` (one read, T297). Its tests
    move to the new query; the flow 03 comments that name it are re-pointed.
  - `ListDecidedByYouQuery`, paged: moved last by the caller, and finished or with no move left (T297's definition, by
    each pinned workflow in C#), newest first by that move, the caller's own subject rows left out, the latest credit by
    T108's rule. The total reads every activity the caller moved last before slicing (note 6). Page and size clamped as
    `ListActivitiesBySubjectQuery` clamps them.
  - `GetAssessorDashboardSummaryQuery` reshaped to the two reads (`build-contracts.md`); `AwaitingReviewItem` and
    `RecentDecisionItem` retired. Keep `AssessorDashboard.razor` and `ActivityInbox.razor` compiling with the least
    change; lane B rewrites both.
  - Note 10, the two "since" clocks: keep `UpdatedOn` for the row, the one clock the dashboard, the stall and the nudge
    share (round 1, correction c; Spec § 7), and say in the reader's remarks that the status card reads the last move and
    that the two part only after a data write without a move. Step 3.30's ageing of the history rows is the runbook
    lane's (E5).
  - Web, the row: NEW `WaitingList.razor` (NeedsYouList with an overdue edge, C6) and `WaitingWords` (every waiting,
    count, overflow, result-tail and other-role-line phrase in `build-contracts.md`); `ActivityLink` gains the assessor's
    second line, "from <registrar>" (note 12's half, R2); `ActivityRowNames` reads SAST (note 8's half) and gains the
    waiting lists' alike rule, its tie-breakers on the link, not the Open button (note 9, C11); `BadgeFor.Overdue` and
    `.badge-overdue` added to the badge rule `DefinedClassTests` reads (note 14).
  - CSS (`/* Flow 04: the waiting row (T350) */`): `.badge-overdue`, `.needs-you-row--overdue`, `.needs-you-badges`, and
    `.needs-you-row .activity-link`'s wrap.
- Files owned: `src/Wombat.Application/Features/Activities/Queries/ListWaitingForYou/*` (new),
  `…/Queries/ListDecidedByYou/*` (new), `…/Queries/ListActivitiesByActorInbox/*` (deleted),
  `…/Activities/Services/WaitingForYou.cs` and `DecidedByYou.cs` (new), `…/Activities/Services/ActivityWaiting.cs`
  (comments and any shared helper only), `…/Activities/Dtos/ActivityDtos.cs` (the init members in the contracts),
  `…/Queries/ListNeedsYou/ListNeedsYouQuery.cs` (comments only), `src/Wombat.Application/Features/Dashboards/Assessor/*`;
  `src/Wombat.Web/Components/Shared/Activities/WaitingList.razor` and `WaitingWords.cs` (new), `ActivityLink.razor`,
  `ActivityListWords.cs`, `ActivityRowNames.cs`; `src/Wombat.Web/Components/Shared/BadgeFor.cs`; `app.css` (its
  section). Minimal compile-only edits: `Pages/Dashboards/AssessorDashboard.razor`, `Pages/Activities/ActivityInbox.razor`.
- Build notes: 5, 6, 8 (row names), 9, 10, 12 (`ActivityLink`), 14. Round 1: Q1 and Q4 (the reads), E5. Round 2: E1, C6
  (classes), C11 (the alike rule).
- Tests: Application — `ListWaitingForYouQueryTests` (oldest first; own subject rows out; author's arms out; `role:` arms
  in; overdue at exactly 7 × 24 h and not a minute before; waited days rounded down; the display name without the
  reader's name; `DueDays` read from `DashboardThresholds`), `ListDecidedByYouQueryTests` (the definition: a return to
  draft is not a decision, a dead end of any name is; newest first; total and clamping; page past the end; credit by
  T108), `AssessorDashboardQueryTests` rewritten, `DashboardInboxParityTests` (Home's first five are the inbox's first
  five, same order). Architecture — `ActivityReadBoundaryTests`, `DashboardStateLiteralTests` updated. Integration —
  `DashboardWaitingPostgresTests` on the new read. Web — `WaitingListTests` (bUnit: row classes, badges, why words, the
  alike rule's accessible names), `WaitingWordsTests`, `ActivityRowNames` in SAST, `ActivityLink`'s "from" line.
- Provides: every Application name in `build-contracts.md` § Reads; `WaitingList`, `WaitingWords`, `ActivityLink.FromSubject`,
  `ActivityRowNames.Waiting`, `BadgeFor.Overdue`. Consumes: nothing new.

**Lane A2 "words"** (`t350-words`). The move words, and the two CSS primitives both wave-2 page lanes need.
- Scope:
  - Sentence case in `WorkflowTransition.LabelFor` (note 1; round 1, E1): "Record discussion", "Sign off", for every type
    and both views, on the bar, in the history and in the reasons; "Decline requires a note." follows from
    `ActivityService` (it already names the move by `LabelFor`). Every test that quotes the title case changes with it.
  - `FilingWords.Running`: the first word's -ing form with the rest kept, "Recording discussion…", "Signing off…";
    "Working…" only for a first word it cannot form (note 2).
  - `RefusalWords.NotDone`: "Not returned." for `return` (a move that leads on); `RefusalWords.ForNote` gives the note
    panel's summary title with " It is still <state>." as `ForMove` does (note 3's words; lane C wires them).
  - NEW `NotePanelWords`: the panel's heading, keep, label, send and help words (note 4; round 1, E2), for lane C to wire.
  - CSS (`/* Flow 04: two renderings and the fold's words (T350, E2, C9) */`): `.only-wide`, `.only-narrow` (toggled at
    `max-width: 640.98px`), `.fold-show`, `.fold-hide` (one hidden per `details[open]`).
- Files owned: `src/Wombat.Domain/Activities/Workflow/WorkflowTransition.cs`;
  `src/Wombat.Web/Components/Shared/Activities/FilingWords.cs`, `RefusalWords.cs`, `NotePanelWords.cs` (new); `app.css`
  (its section); the test files that quote the title case — `tests/Wombat.Domain.Tests/Activities/WorkflowLabelTests.cs`,
  `tests/Wombat.Application.Tests/Activities/{ActivityStateLabelTests,TransitionRefusalLabelTests,WorkflowEvaluatorTests}.cs`,
  `tests/Wombat.Application.Tests/Features/Activities/ActivityWaitingTests.cs` (the label lines only; A1 owns the rest),
  `tests/Wombat.Web.Tests/Activities/ActivityWorkflowActionsTests.cs` (the label lines only), and any other a sweep for
  `"Record Discussion"`, `"Sign Off"` and title-cased keys finds.
- Build notes: 1, 2, 3 (words), 4 (words). Round 1: E1, E2 (words). Round 2: E2 and C9 (the primitives).
- Tests: Domain — `LabelFor` on one-word, two-word, hyphenated and separator-only keys. Web — `FilingWords.Running` for
  every seeded move; `RefusalWords.NotDone`/`ForNote` for decline, return (reflection, review) and sign off;
  `NotePanelWords`; a `StylesheetRuleTests` rule for the 641 px toggle and the Show/Hide pair.
- Provides: `LabelFor`'s sentence case, `FilingWords.Running`, `RefusalWords.ForNote`, `NotePanelWords`, the four CSS
  classes. Reports the runbook and `states.md` lines that quote the title case (act 3 lines 415, 459, 461; `states.md`).

After wave 1: the integrator merges A1 and A2 into `t350`, runs the six suites, and corrects `build-contracts.md` to what
was built (as T342's `aa4cb848`) before wave 2 starts.

### Wave 2 (parallel): the pages

**Lane B "lists"** (`t350-lists`). The Assessor's Home, the other-role line, and the Activity inbox.
- Scope:
  - `AssessorDashboard.razor` (R1; Q2): "Waiting for you" (`WaitingList` over the first five, the count in words as the
    title's badge and no "0" badge, the rule line with `DueDays`, "20 more wait in the Activity inbox." (E5), "Nothing
    is waiting for you.", foot "Open Activity inbox"); "Recent decisions" (five, dated, each linked by its full name with
    "from <registrar>", its state's badge and date; "No decisions yet."; foot "All your decisions" to the inbox's second
    section). One of `--emphasis`/`--warning` per card: warning when any row is overdue (nit T15).
  - `DashboardCard`'s words badge: a mode that badges words ("2 waiting, 1 overdue"), beside the figure-plus-`CountWords`
    mode flow 03 built (note 13).
  - `DashboardFrame`: the always-present `role="status"` "Loading your Home." (C11); Try again's focus to the region that
    replaces the error (C10 e).
  - NEW `OtherRoleLine.razor` placed in `Home.razor` between the `PageHeader` and the role's dashboard, shown when
    `Acting.Holds(Assessor) && Acting.Role != Assessor` and work waits (note 7; Q3; E4; C12): the three wordings, warning
    tint when any is overdue, info otherwise; "Open it" / "Open the oldest" opening the activity with no switch; nothing
    when nothing waits; a failed read renders nothing and is logged, never a Home error. The committee Home is flow 07's
    page: flow 04 owns the line's words and this one slot in `Home.razor`, above whichever dashboard the acting role
    draws, and changes nothing inside `CommitteeMemberDashboard.razor`.
  - `ActivityInbox.razor` (R2; Q1): subtitle "What waits for you to rate, review or discuss."; section "Waiting for you"
    (count badge in words, rule line, `DataTable Stack` with Activity, EPA, State, Waiting, the name as the link with
    "from <registrar>", Overdue beside the state, "8 days" / "Less than a day" over "since … SAST"; "Inbox clear" /
    "Nothing is waiting for you."); section "Decided by you" ("45 decisions", rule line, Activity, Decision, Decided
    (SAST), Credit; `PagerControls` as built; "No decisions yet."); "Loading the Activity inbox." (C11); the registrar's
    empty state unchanged but for the subtitle (note 12; note 8's cells; C5; C13's clip rule, never `display: none`).
  - `PagerControls`: Previous and Next `aria-disabled` at the ends, not `disabled`, so the pressed button keeps its place;
    a page change focuses the section's h2 (C10 f). Check its other pages' tests.
  - 44 px below 641 px for the card footers' buttons, the three Try again and the pager's buttons (C8).
  - CSS (`/* Flow 04: the Assessor's Home and the inbox (T350) */`): `.waiting-more`, `.card-empty`, `.decided-list`,
    `.decided-row`, `.decided-meta` (without the dead `align-items`, nit T2), `.decided-date`, `.waited-cell`,
    `.epa-cell`, `.other-role-line`, the C8 lifts.
  - DESIGN.md: R1 (§ Dashboard page) and R2 (§ Page-level patterns, List page).
- Files owned: `src/Wombat.Web/Components/Pages/Dashboards/AssessorDashboard.razor`, `Pages/Home.razor` (the slot only),
  `Pages/Activities/ActivityInbox.razor`; `Components/Shared/DashboardCard.razor`, `DashboardFrame.razor`,
  `PagerControls.razor`, `Components/Shared/OtherRoleLine.razor` (new); `app.css` (its section);
  `execution/architecture/DESIGN.md` (R1, R2); tests under `tests/Wombat.Web.Tests/Dashboards/*`,
  `tests/Wombat.Web.Tests/Navigation/DashboardLinkAuthorizationTests.cs` and the inbox's and pager's tests.
- Build notes: 7, 8 (the inbox's cells), 12 (the page), 13. Nits: T2, T15, A13. Round 1: Q1–Q4, R1, R2. Round 2: E1
  (the rule line), E4, E5, C5 (pager, subtitle), C8 (Home, inbox, pager), C10 (e, f), C11 (Home, inbox), C12, C13.
- Tests: bUnit for every `R3-H-*` and `R3-I-*` state's words and structure: empty, one waiting, several overdue,
  overdue only, switched, twenty-five (five rows and the overflow line), loading, load error; the three committee-line
  wordings, none when nothing waits, the failed read logged and silent, and no line for an Assessor acting as Assessor;
  the inbox's clear, clear-with-decisions, two, overdue, paused EPA, alike (the accessible names), twenty-five,
  decided-45 (pager words, `aria-disabled`, the h2's focus), long, registrar-only, loading, load error. `DashboardCard`'s
  words badge. `HomeFrameTests`, `WaitingCardsTests`, `AssessorDashboardSubjectNameTests` rewritten to the new cards.
- Consumes: A1's reads, `WaitingList`, `WaitingWords`, `ActivityLink.FromSubject`, `ActivityRowNames.Waiting`,
  `BadgeFor.Overdue`. Provides: the ids `card-waiting`, `card-decisions`, `waiting-h`, `decided-h` and the link
  `/activities/inbox#decided-h` (`build-contracts.md`).

**Lane C "page"** (`t350-page`). The assessor's side of the activity page, the note panel and the way on.
- Scope:
  - The result and the way on (Q6; C1; round 1, correction d): after a move commits, one more `ListWaitingForYouQuery`
    less the activity just moved. `#activity-result` (`role="status"`, focused) holds only `ResultSentence` plus
    `WaitingWords.MoreForYou(n)` ("Completed. 1 more waits for you.", "Discussed. Nothing else waits for you.", "It is
    now Draft. Nothing else waits for you."); straight after it, outside the region, NEW `WayOn.razor`: the next row
    (`WaitingList`, `WithSince`), "Open the next" (named for its item) and "Back to <owner>" by `NavOwners.OwnerFor` for
    the acting role, else "Back to Home"; nothing left: "Go to Home". The next Tab after the result is Open the next
    (C10 d).
  - The note panel (note 4; round 1, E2; note 3's wiring): `NotePanelWords` for heading, label, send, keep and help,
    the part from `ActivityPageModel.AuthorPart`'s first word; the refused summary's title from `RefusalWords.ForNote`;
    focus rules C10 a–c; the note's `aria-describedby` "note-help note-msg", never the summary (nit A8), and never empty;
    `aria-controls` only while the panel exists (nit A9); the visually hidden "required" dropped where `aria-required`
    is set (nit A7, the note); Send and Keep stacked at 390, 44 px, the send first (C8).
  - Discard changes, greyed, says why beside it: "Nothing to discard yet." in `.move-reasons--beside`, the button
    `aria-describedby` it (note 11; round 1, E4).
  - The running labels and the bar's hidden status come from A2's `FilingWords.Running` ("Recording discussion…",
    status "Recording discussion."); check it (note 2).
  - "Activity unavailable": its button goes to the acting role's list by the owner table ("Go to Activity inbox" for an
    Assessor, "Go to My activities" for a Trainee), else "Go to Home" (round 1, E4; R3).
  - On a phone, only for a reader with a section to fill: About under the bar (its twin `.only-narrow`, ids suffixed
    `-narrow`) and `FoldFilledSections` passed to the form (round 1, E3; round 2, E2). Every other reader keeps the order.
  - The status card's region gets a short accessible name (nit A10); no year minimum or semester count on it (Q7).
  - An activity opened from the other-role line by a Committee member: nothing lit, trail Home › the activity; a test
    only, since the owner table already does it (§ 8).
  - CSS (`/* Flow 04: the way on and the move bar (T350) */`): `.way-on` (gap `var(--space-md)`, nit T3; the 2 px gaps
    `var(--space-xs)`, nit T4), `.way-on-none`, `.move-reasons--beside`, the note panel's and the way on's 44 px rules.
  - DESIGN.md: R3 (§ Page-level patterns, Record page with a workflow), and R4's "one note panel" sentence.
- Files owned: `src/Wombat.Web/Components/Pages/Activities/ActivityView.razor`,
  `Components/Shared/Activities/ActivityWorkflowActions.razor`, `ActivityPageModel.cs`, `ActivityDetail.razor` (the
  forwarded parameter), `WayOn.razor` (new); `app.css` (its section); `execution/architecture/DESIGN.md` (R3); tests
  under `tests/Wombat.Web.Tests/Activities/` for the page and the move bar.
- Build notes: 2 (check), 3 (wiring), 4 (wiring), 11. Nits: T3, T4, A7 (the note), A8, A9, A10. Round 1: Q6, Q7, E2,
  E3 (order), E4 (the unavailable link, Discard), R3. Round 2: C1, C8 (note panel, way on), C10 (a–d), C11 (the bar).
- Tests: bUnit for every `R3-P-*` state's result, way on, status card, bar and panel: completed-next (the row, both
  buttons, Open the next's accessible name, Back to by owner), completed-last and discussed ("Go to Home"),
  completed-paused, declined, returned ("It is now Draft. …"), decline-note and return-note (focus to `#note-in`),
  decline-refused and return-refused (the titles, `#note-summary` focused, the note kept), completing (running label,
  hidden status), from-the-line, unavailable (by acting role), loading, load error; the About twin only for a filler.
- Consumes: A1's `ListWaitingForYouQuery`, `WaitingList`, `WaitingWords.MoreForYou`; A2's `NotePanelWords`,
  `RefusalWords.ForNote`, `FilingWords.Running`, `.only-wide`/`.only-narrow`; D's `ActivityForm.FoldFilledSections`
  (pass it; D builds it). Provides: `FoldFilledSections`' value, `#activity-result`, the note panel's ids.

**Lane D "form"** (`t350-form`). The rater's form: the rung picker, the request fold, names without email.
- Scope:
  - The rung picker (Q5; round 2, E3; C7; R4): on the writer's `rated_level_field` only, and only when its ladder loads
    (`Rungs` non-empty); any other scale field, or a ladder that fails, keeps the select. A `fieldset.rung-picker` with
    the field's label as legend, the College's help, one `label.rung-choice` and native radio per rung, `is-chosen` and
    `checked` as conditions, the `li` keyed by the rung's order (`@key`) so a round trip keeps the focused radio; each
    radio `aria-describedby` a visually hidden descriptor span outside the `<details>`; "chosen" `aria-hidden`; the
    chosen rung's descriptor under the row. A press is one round trip through today's `UpdateValue`. The first radio
    carries `<fieldKey>-in`, so flow 03's refusal summary link still lands on the field. The row is as wide as its
    ladder; one legend per picker.
  - "What each rung means": with no rung chosen, two renderings (`.only-wide` open, `.only-narrow` shut, the narrow
    copy's ids suffixed `-narrow`); once one is chosen, one, shut. Show/Hide spans (C9).
  - The request fold (round 1, E3; round 2, E2): with `FoldFilledSections`, each filled read-only section is drawn
    twice: `.only-wide` as today, and `.only-narrow` as `details.section-fold` (title, "Filled in by …", Show/Hide, no
    gist; ids suffixed `-narrow`).
  - Person fields read the name, not `NomineeDirectory`'s "name (email)" label, through `NomineeNames.NameOf` (note 15;
    round 1, E4).
  - The declined page's locked sections keep "Not filled in." and the pending rung row (C5): no change, a test.
  - The visually hidden "required" dropped where `aria-required` is set, and no empty `aria-describedby` (nit A7).
  - 44 px at 390: the rung cells (3.5rem), the folds' summaries (C8).
  - CSS (`/* Flow 04: the rung picker and the request fold (T350) */`): `.rung-picker`, `.rung-choice`, `.rung-legend`,
    `.rung-legend-marker`, `.rung-legend-list`, `.section-fold`, `.section-fold-head`, `.section-fold-show`,
    `.section-fold-owner`, `.section-fold-body`; the radio native with `accent-color` (C7).
  - DESIGN.md: R4 (§ Form system, "The activity form"), and the rung cell's sentence (Spec § 2).
- Files owned: `src/Wombat.Web/Components/Shared/Activities/ActivityForm.razor`, `RungRow.razor`, `NomineeNames.cs`,
  `RungPicker.razor` (new, if split out); `app.css` (its section); `execution/architecture/DESIGN.md` (R4); tests under
  `tests/Wombat.Web.Tests/Activities/` for the form and the rungs.
- Build notes: 15. Nits: A7 (the form). Round 1: Q5, E3 (the fold), E4 (names), R4. Round 2: E2, E3, C5 (declined),
  C7, C8 (rungs, folds), C9.
- Tests: bUnit — the picker only on the rated field with a loaded ladder (a second scale field and a failed ladder keep
  the select); six radios, one checked, `is-chosen`, `aria-describedby` to a span outside the details; the chosen
  descriptor; the legend's two renderings with none chosen and one with a choice; the first radio's id; the fold's twins
  with `-narrow` ids only under `FoldFilledSections`; a person field by name; the declined page's locked sections; no
  empty `aria-describedby`. `StylesheetRuleTests` for the 3.5rem cell and the 44 px summaries.
- Consumes: A2's `.only-wide`/`.only-narrow`/`.fold-show`/`.fold-hide`. Provides: `ActivityForm.FoldFilledSections`,
  the radio and descriptor ids (`build-contracts.md`).

### After wave 2 (the integrator, then its own lanes)
- Merge B, C and D into `t350`; run the six suites; make `app.css`'s four flow-04 sections and DESIGN.md's R1–R4 one
  account, with the banner naming flow 04 (as `3c239246`).
- **Runbook lane** (`t350-runbook`, as `t342-runbook`): the Expects `R3-Steps` lists (2.36, 3.4, 3.5, 3.11, 3.13, 3.15,
  3.17, 3.24, 3.26, 3.28, 3.33, 3.51, 5.25, 6.18, A.6.8, A.7.2, flow 03's 3.16), the title-case quotes A2 reports, and
  Step 3.30's ageing, which must also age the two items' Submit and Create history rows (round 1, E5); `states.md` and
  `coverage.md` where a page's states change.
- A four-sided review (`build-review.md`, stopping line agreed first), one fix pass (`t350-fixback`, `t350-fixweb`),
  then the squash onto master.
