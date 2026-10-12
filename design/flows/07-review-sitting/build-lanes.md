# T361 step 6 — rules common to every build lane (flow 07, the review sitting)

Adapted from `design/flows/06-programme-oversight/build-lanes.md` (T358). You are one lane of the build of flow 07 of
Wombat's GUI restructure, task T361 (`execution/tasks/in_progress/T361-*.md`). The design is final: Claude Design canvas
round 3 (version `1791774971-ed50`, `round-3-check.md`). The lane plan is at the end of this file; the names two or more
lanes share are in `build-contracts.md`, beside it.

## Read first (in your worktree)
- `design/flows/07-review-sitting.md` — the brief: the cast, the 26 runbook steps, what is wrong today, and § 1's fence.
- `design/flows/07-review-sitting/round-2-ask.txt` — the pick (A, "the agenda is the page"), Q1–Q12, C1–C9 and **D53, no
  deferral**. `round-3-ask.txt` — the operator's E1–E9, the review's corrections 7–31 by number, and the fills.
- `design/flows/07-review-sitting/round-2-review.md` — findings 1–31 with file:line evidence (findings 1–6 are D53's
  domain change); `round-3-check.md` — what **the build carries** and the three points settled there (Refresh while
  the review can still change; "No change."; the hint beside an available Record).
- `design/flows/07-review-sitting/round-3/project/` — the accepted boards (the files keep their `R2-` names).
  `round-3/boards-text.md` is their text.
  - `R2-Spec.dc.html` is the contract: round 3 item by item, the NEW and changed components and their classes, the
    DESIGN.md rule texts, the D53 words table, the colour pairs and targets, what is still bracketed.
  - `R2-Steps.dc.html` lists every runbook Expect that changes, and the recount.
  - `r2.css` is the canvas's picture, not code to copy: rebuild each rule in `app.css` on the spacing scale.
  - A bracketed figure on a board is read from the data; a board caption is never a shipped word.
- `execution/knowledge/EPA-PROGRAMME.md` § 3D, **D53** (the operator's): no deferral; it overrules T131's O2 and
  Decision 6.
- `execution/architecture/DESIGN.md` — mandatory before touching any Razor or CSS. CLAUDE.md — conventions and footguns,
  § Roles, § EF Core migrations.
- Flow 05's and flow 06's `build-contracts.md` for the vocabulary reused here: `EntrustmentStandingPanel`,
  `MsfCoveragePanel`, `TrajectoryChart`, `PageFocus`, `QuotaText.Iso`, `ActivityMoments.When`, `ProgrammeReadAs.RoleFor`.

## Where you work
- Only in your own worktree, `C:\dev\Wombat\.claude\worktrees\<branch>` (given in your prompt), on its branch. Use
  absolute paths for every command.
  - Never touch `C:\dev\Wombat` itself (master).
  - Never merge, push or switch branch. No lane pushes, and the integrator does not either.
- Commit in your worktree as you go: write each message to a file and use `git commit -F <file>`; end every message
  with the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Other lanes run in parallel on other branches. Stay inside the files your lane owns. `app.css` and DESIGN.md are
  shared by section: each lane writes only under its own heading. If you must touch a file another lane owns, keep the
  change minimal and name it in your report.

## Never
- Touch the apps on ports 5080 and 5180 or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves; each works in its own throwaway `it_<guid>` schema.
- Pass `--no-build` to `dotnet test` or `dotnet ef`.
- Upgrade MediatR; add Bootstrap/MudBlazor/Radzen/jQuery; put raw hex colours outside `:root`, inline `style=` in shipped
  markup, or `<i class="bi">`; load anything from a third-party host; write an inline script or `onclick`.
- Use the native `disabled` attribute on Record decision… or Stage pending decision: `aria-disabled="true"` with
  `.is-unavailable`, the reason beside it and named by `aria-describedby` (DESIGN.md's rule; round 3 item 16).
- Use a pronoun in any shipped word (round-3-check 3): the person's name, "the chair", "the registrar".
- Print a date in any form but ISO, a time without "SAST", or "today" from anything but `QuotaCalendar.Today(TimeProvider)`
  / `ProgrammeCalendar.DateOf(utc)` (T325).
- Print an exception's text on a page: the load error is a fixed sentence, the failure goes to the log (T329).
- Offer, draw or mention deferral anywhere: Defer, Reinstate, a deferral reason, "or deferred with a reason", "still to
  stage or defer" (D53). "Outcome Deferred" is a progression category (`CommitteeDecisionCategory.OutcomeDeferred`) and
  **stays**: never confuse the two.
- Build live following (T362): a reader keeps up by Refresh.
- Change a seed, the credit engine, or any migration but the one lane A writes.

**One migration.** Lane A's alone, written once: `<timestamp>_T361_NoDeferral`. If `dotnet ef migrations add` cannot
run, hand-write it **with its `.Designer.cs`** (copy the latest Designer, `20261005074408_T358_ActivityRemindersAndAdmission`,
change the timestamp, class name and `BuildTargetModel`) and update `ApplicationDbContextModelSnapshot.cs`. Without the
Designer `MigrateAsync` silently skips it. Read the generated migration before trusting it: a stale build writes an empty
one.

`AuditPipelineBehavior` commits a failed handler's staged rows (T201): every command validates first and stages only once
nothing can refuse it.

## How
- Test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density. This codebase comments *why*, with task ids: cite
  T361 and the ids of the round that asked, e.g. "(T361, D53)", "(T361, E4)", "(T361, round 3 item 21)",
  "(T361, review 7)".
- Application handlers go through `IApplicationDbContext`. Web components use `IScopedSender` and DTOs, never Domain types
  in `.razor`. A read about a trainee goes through the existing scope helpers; an out-of-scope or unknown id reads as not
  found (null), never a refusal.
- Straight apostrophes in app strings; every sentence that counts has its singular ("1 item", "PAED-002 and 1 other").
- Change a test deliberately, never loosen it to pass. `DefinedClassTests` fails a class a page uses that `app.css` does
  not define: a class is defined by the lane that owns it (`build-contracts.md` § CSS).
- Each lane adds its classes under its own `/* Flow 07: … (T361) */` heading in `app.css`.
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`: the runbook lane does
  both after the merge. T294's guard (`tests/Wombat.Web.Tests/Scenario`) must stay green. Name in your report every
  runbook line whose Expect your change alters.
- Before finishing:
  - `dotnet build C:\dev\Wombat\.claude\worktrees\<branch>\Wombat.sln -c Release` clean, with no new warnings.
  - Then all six suites, each with its own build: Domain, Application, Infrastructure, Architecture, Web, Integration
    (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message goes to the integrator, not to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and what another lane or the integrator must know: the contracts you provide or rely on (exact
  type, member and parameter names, and any way they differ from `build-contracts.md`), files you touched outside your
  list, runbook steps whose Expect your change alters.

## The lane plan

Three lanes in two waves, then the runbook lane. Worktrees under `C:\dev\Wombat\.claude\worktrees\<branch>`. Integration
branch **`t361`**, cut from master at `8236dd8b` or later; each wave's lanes are cut from `t361` and merged back into it,
and the whole is squashed into one commit on master. Nothing is pushed.

**Why this shape.** D53 changes the domain the page reads, and the page is rewritten whole, so the domain goes first and
the page second. The page's new reads, the flow 03 return link and the shell's stacking touch none of lane A's files, so
they run beside it.

### Wave 1 (parallel)

**Lane A "no deferral"** (`t361-domain`). D53 end to end, below the review page.
- Scope (round 2 review findings 1–6; E1, E3, E4, E5; round 3 item 29; T309 item 2):
  - **Domain.** `CommitteeAgendaLineState.Deferred` goes (the other values keep their numbers: `DecidedNamesItsStar`
    uses 3). `Defer`, `Reinstate`, `DeferralReason` go. `BlocksRatify`, `OutstandingClosingLines`, `EnsureAgendaClosable`
    go; `EnsureAgendaSettled` keeps only its empty-agenda part, if any caller still needs it. `CloseUndecided` drops its
    closing-line guard. `IsClosing` stays for the label. Ratify (`CloseAgenda`) writes NotDecided on every line left
    unstaged, due and optional alike (E1). NEW `CommitteeDecision.UndecidedNote` (nullable, the decision's one sentence
    about the EPAs not decided).
  - **E4.** An entrustment-only review refuses Record with nothing staged: "This review decides entrustment only, so
    stage at least one decision before recording it." In the domain (the backstop) and as a reason the page can show
    before the press (`build-contracts.md`).
  - **E5.** A remitted appeal's replacement decision carries the original's `UndecidedNote` (`ResolveAppeal`).
  - **Application.** `DeferAgendaLine.cs` deleted (both commands). `RecordCommitteeDecisionCommand` takes
    `UndecidedNote` (optional; validated for length like Conditions; `[Redact]` like Rationale) and no longer waits on
    closing lines (Record is held back only by the quorum, and E4). The agenda DTOs lose `BlocksRatify`,
    `OutstandingClosingLines`, `RatifyBlockedReason` and the closing-line half of `RecordBlockedReason`; a line DTO
    tells the page when it reads "Not decided at this sitting" (state NotDecided, or the review Decided and the line
    unstaged: settled, the same words between Record and Ratify). The staging refusal "is deferred … Reinstate it"
    goes. `ReviewChanged`'s and the who-may-act sentences lose "defer agenda lines" ("Only the panel's chair, Thandi
    Zulu, can stage entrustment decisions and record the committee's decision.").
  - **The decisions-due page (E3).** `CommitteeAgendaStatus.Standing`, `GetEntrustmentDecisionsDue`: the Deferred status
    becomes **Not decided**, and reads so only when the line was due at that sitting (closing) or its window has ended;
    an annual EPA left unstaged at a semester-1 sitting still reads "Due by year end". Missed keeps its meaning.
    `DecisionsDue.razor`, `DecisionsDueText.cs` (Missed's "nothing decided, deferred or on an open review's agenda"
    loses "deferred"), `BadgeFor.cs` (the status's badge, `BadgeForStatusTableTests`).
  - **The registrar's side.** `CommitteeAgendaText.cs` and `MyReviews.razor`: a ratified review's not-decided line reads
    "Not decided at this sitting." and the decision's sentence; no reason, no Deferred.
  - **The schedule preview (29).** `PreviewCommitteeAgenda` / `ScheduleCommitteeReview` / their page words: "must be
    decided at this sitting, or deferred with a reason, before it is ratified" becomes "is due at this sitting".
  - **T309 item 2.** `CommitteeDecisionWording.ReviewTypeLabel`: "Formative check-in" when the review is formative,
    else the type's label; used by the DTO the review page reads (and the review lists that print the type, if they
    read the same wording).
  - **The migration** `T361_NoDeferral`: Deferred rows become NotDecided; drop `CK_CommitteeAgendaLines_DeferredHasAReason`
    and `DeferralReason`; add `CommitteeDecisions.UndecidedNote`. The configuration files follow.
  - **Keep the Web project compiling** with the least change in `ReviewDetail.razor`: delete the Defer/Reinstate UI and
    its handlers, show nothing new. Lane P rewrites the page.
- Files owned: `src/Wombat.Domain/CommitteeDecisions/{CommitteeAgendaLine,CommitteeReview,CommitteeDecision}.cs`;
  `src/Wombat.Application/Features/CommitteeDecisions/{DeferAgendaLine (deleted),CommitteeAgendaDtos,GetCommitteeAgenda,
  CommitteeDecisionWording,GetEntrustmentDecisionsDue,RecordCommitteeDecision,RatifyCommitteeDecision,ResolveAppeal,
  PreviewCommitteeAgenda,ScheduleCommitteeReview,CommitteeDecisionMappings}.cs`, `CommitteeDecisionDtos.cs` (the
  decision DTO's `UndecidedNote` and the doc comments that name deferral, **only**),
  `…/Features/EntrustmentDecisions/StagePendingEntrustmentDecision.cs` (the refusal only);
  `src/Wombat.Infrastructure/Persistence/Configurations/CommitteeDecisions/*` (the two affected), the migration, its
  Designer, the snapshot; `src/Wombat.Web/Components/Pages/CommitteeDecisions/{DecisionsDue.razor,DecisionsDueText.cs,
  CommitteeAgendaText.cs,MyReviews.razor}`, `ReviewsSchedule.razor` (the preview's words only), `Components/Shared/BadgeFor.cs`,
  `ReviewDetail.razor` (compile-only deletions).
- Tests: Domain — `CommitteeAgendaTests`, `CommitteeReviewTests` (Ratify writes NotDecided on every unstaged line; Record
  is not held by closing lines; E4 refused; nothing deferrable). Application — `CommitteeAgendaHandlerTests`,
  `EntrustmentDecisionsDueHandlerTests` (E3: closing or window ended → Not decided; an annual line at a semester-1
  sitting → Due by year end; Missed unchanged), `EntrustmentOnlyReviewHandlerTests` (E4), `ChairMaySitTests`,
  `CommitteeRequestsAuthoriseFirstTests`, the record handler (the note stored, redacted in the audit), the appeal
  handler (E5). Infrastructure — the configuration. Integration — `CommitteeAgendaPostgresTests`,
  `CommitteeReviewRacePostgresTests`, `EntrustmentDecisionsDuePostgresTests`; the migration applies and converts a
  Deferred row. Web — `DecisionsDuePageTests`, `MyReviewsAgendaTests`, `BadgeForStatusTableTests`; the deferral halves
  of `ReviewDetailAgendaTests`/`ReviewDetailChairControlsTests`/`ActionFocusTests` deleted (lane P rewrites the rest).
- Provides: § A of `build-contracts.md`.

**Lane R "reads, return link, stacking"** (`t361-reads`). What the new page reads that it cannot read today, flow 03's
return link, and the shell's stacking.
- Scope (round 2 review 9, 10, 11, 13 (T273), 14, 15; E2, E6; round 3 items 11, 14, 15):
  - **The review detail DTO gains** (`GetCommitteeReviewById`): the trainee's preferred profile id while the trainee
    trains at the panel's institution (`TraineeScopeResolver.PreferredProfiles`; null otherwise: the page draws the
    progress link only for the four roles flow 06's registrar page admits, by `ProgrammeReadAs.RoleFor`), and the
    training year at the end of the review's period (E6).
  - **Start.** NEW `GetStartPreviewQuery`: what Start would freeze, counted by the same code Start runs (extract it from
    `StartCommitteeReview`, never a second copy), and the campaigns left out (`CountMsfCampaignsOutsideSnapshot`, T173).
    `StartCommitteeReview` refuses, before anything is staged, while the chair cannot act (Q11, E2), with the review's
    `ChairCannotAct` sentence, so a stale tab cannot start it.
  - **T273.** `EpaSamplingConcentrationDto.EpaInForce` and `PendingEntrustmentDecisionDto.EpaInForce`, from the EPA rows
    the queries already load.
  - **Flow 03's return link (9).** `ActivityView.razor` reads `?from=review-{id}&at=snap-{itemId}`, both integers only;
    when the reader may read that review (`GetCommitteeReviewByIdQuery` returns it), the review's "Back to Lerato Molefe ·
    2026 S2" **replaces** the page's own back link and goes to `/committee/reviews/{id}#snap-{itemId}`; else the page is
    unchanged. A forged or unreadable id shows nothing. NEW `ReviewLinks` (Web) builds both addresses, so lane P links the
    same way.
  - **Stacking and focus clearance (14, 15; T356).** `.sidebar` z-index 10 at 641 px and wider; `html {
    scroll-padding-top: calc(56px + var(--sitting-bar-h, 0px)); }`; NEW `wwwroot/js/sticky-offset.js` (loaded as the
    other scripts are, CSP-clean) sets `--sitting-bar-h` on `html` from a ResizeObserver on the stuck chair's bar
    (`.sitting-bar:not(.sitting-bar--reader)` at 641 px and wider), 0 otherwise. A `StylesheetRuleTests.Flow07` rule pins
    both. Check every other positioned element in the shell (T356's list): dialogs keep their order above the bar.
- Files owned: `src/Wombat.Application/Features/CommitteeDecisions/{GetCommitteeReviewById,StartCommitteeReview,
  GetSamplingConcentrationWarnings}.cs`, NEW `…/GetStartPreview.cs`, `CommitteeDecisionDtos.cs` (the detail DTO's new
  members only), the pending-decision DTO and its query (`Features/EntrustmentDecisions/`, the flag only);
  `src/Wombat.Web/Components/Pages/Activities/ActivityView.razor` (the return link only), NEW
  `src/Wombat.Web/Components/Shared/ReviewLinks.cs`, `src/Wombat.Web/wwwroot/js/sticky-offset.js`, the script tag's file,
  `app.css` (`/* Flow 07: stacking and focus clearance (T361) */` and the `.sidebar` rule).
- Tests: Application — the detail query (profile id: preferred only, null when elsewhere; the training year at the
  period's end, Molefe's 4), `GetStartPreviewQuery` (Molefe at Act 4: 9 items, 8 activities and the campaign report, 0
  left out; a closed unreleased campaign: 1; equal to what Start then freezes), Start refused while the chair cannot act
  (nothing staged), the two `EpaInForce` flags. Integration — the start preview on Postgres. Web — `ActivityView` (the
  link replaces the back link; a reader who may not read the review, a non-integer and an unknown id: the page's own
  link, no name leaked), `StylesheetRuleTests.Flow07`.
- Provides: § R of `build-contracts.md`.

After wave 1: the integrator merges A, then R, into `t361`, runs the six suites, and corrects `build-contracts.md` to what
was built ("As built after wave 1") before wave 2.

### Wave 2: the page

**Lane P "the review page"** (`t361-page`). `/committee/reviews/{ReviewId:int}` as round 3 draws it, at 1280, 768 and 390.
- Scope: every board of `R2-1280-start`, `-stage`, `-decide`, `-more`, `R2-768`, `R2-P-*`, the Spec's components and
  DESIGN.md texts; Q1–Q12, C1–C9, E1, E2, E4, E7, E8, E9; round 3 items 7, 8, 11, 13, 15–28, 30, 31; the settled points;
  T329, T273, T325, T260 and T324 on this page.
  - **Split the page.** `ReviewDetail.razor` becomes a host over NEW components in
    `Components/Pages/CommitteeDecisions/Review/`: `SittingBar`, `AgendaTable` (with the open line, the draft, the
    results, the foot form "Stage a decision on an EPA not on this agenda"), `AgendaLineForm`, `EvidenceReader`,
    `DecisionForm`, and one words class `ReviewWords` (every shipped sentence of the boards, singular and plural). The
    other cards (notes, standing, MSF, snapshot, trajectories, appeals) keep their components, reordered as the Spec's
    § Detail page says.
  - **The sitting bar:** state, count, the mode and evidence window before Start (28), Present from the ticks, the next
    act; the chair's sticky compact bar (E8), readers' static bar with the who-may-act sentence and "As of 14:02 SAST" /
    "As of 2026-10-04 14:02 SAST" with Refresh while the review can still change (Scheduled, In progress, Decided;
    round-3-check), which re-reads in place, keeps scroll and focus, and says what changed ("As of 14:31 SAST. Now
    Decided." / "As of 14:31 SAST. No change."). Start for whoever `CallerMayStart`, never while the chair cannot act;
    T194's sentence for others (E2).
  - **The agenda:** one line per EPA (code over title, Window, State, Evidence, Action for the chair); one open line at a
    time with "Draft kept" (E9); the open line's standing in one line, each of its parts failing apart ("Standing could
    not be loaded.", 11); evidence grouped by instrument A–Z (30); Read swaps the form for `EvidenceReader` (flow 03's
    status, about and read-only form, no action, no move; "Now Completed. Frozen at Start as Awaiting review.", 8), Back
    restores the form as typed; the chart pair (C2); staged lines in 30's words with Remove… (`.btn .btn-sm
    .btn-outline`, 17); "(no longer in use)" (13, T273); "Not decided at this sitting" with the decision's sentence (E1);
    the pre-graduation caption and "Already decided in this window, so not on the agenda: …" (25); stacking by a
    container query with explicit table roles (20).
  - **The decision form:** Category, Rationale, "About the EPAs not decided (optional)" with its help, Conditions,
    Present; the quorum line from the ticks (7, the only live region besides results, 21) with `PanelShortfall` taking
    priority; Record decision… `aria-disabled` while the quorum falls short or (entrustment-only) nothing is staged, the
    reason beside it (16, E4); T324's refusals marked on their fields; the decision card rises above the agenda once
    recorded.
  - **Dialogs** (`ConfirmDialog`): Start (the preview's count; T173's sentence when it applies), Remove ("Keep it"),
    Record (E1's lists), Ratify ("PAED-002 and 11 others were not decided at this sitting."), Close review; "Not yet"
    everywhere else (22); focus on the safe button.
  - **States:** loading (header "Committee review", skeleton, no action; T329), load error (fixed sentence, Try again),
    not found (flow 01's); every result and refusal beside its control, taking the focus (`scroll-margin-top`, 15).
  - The links: "Lerato Molefe's progress" in PageHeader's `SubtitleContent` (19), for the four admitted roles only;
    snapshot lines to flow 03's page through `ReviewLinks`, with `id="snap-{itemId}"` targets.
  - DESIGN.md: the Spec's rule texts (§ Committee agenda, § The chair's controls, § Page-level patterns · page actions
    and the title rule, § Entrustment-only reviews, § Detail page, § Stacking), written without pronouns.
  - CSS (`/* Flow 07: the review sitting (T361) */`): `.sitting-bar` and its parts (flat names, 18), the stacked line's
    stripe, `.agenda-form*`, `.snapshot-pick*`, `.evidence-reader`, `.chart-jump`/`.chart-back`, `.quorum-line`; the
    open row reuses `tr.is-editing`.
- Files owned: `ReviewDetail.razor`, `Components/Pages/CommitteeDecisions/Review/*` (new), `EvidenceSnapshotGroups.cs`,
  `ReviewRowNames.cs`; `app.css` (its section); DESIGN.md (the sections above); tests
  `tests/Wombat.Web.Tests/CommitteeDecisions/*` for the review page (rewrite `ReviewDetailAgendaTests`,
  `ReviewDetailChairControlsTests`, `EntrustmentOnlyReviewPageTests`; NEW per component), `Accessibility/ActionFocusTests`
  (its review half), `Design/*` where the page's classes are pinned.
- Tests: bUnit for every board state named in the Spec's round-3 table (the frames by number), the dialogs' words and
  focus, no native `disabled` on the two buttons, no pronoun, no "defer" in any rendered word, the singular forms.
- Consumes: § A and § R. Provides: nothing further.

### After wave 2 (the integrator, then its own lanes)
- Merge P into `t361`; run the six suites; make `app.css`'s flow-07 sections and DESIGN.md one account, the banner naming
  flow 07.
- **Runbook-and-docs lane** (`t361-runbook`), from the built code: every Expect `R2-Steps` lists (4.15–4.32, 4.50,
  5.3–5.6, A.7.6, A.7.13; 4.21 deleted and the steps renumbered only if the runbook's convention says so), the round 2
  review's D53 line list (act-4, act-5, `coverage.md`, `states.md`), the recount; `design/flows/07-review-sitting.md` and
  any other flow brief that quotes a changed Expect (flow 08's schedule preview and Decisions due, flow 09's appeals).
- **The build's review**: three read-only reviewers on Sonnet (`review-lanes.md`): code, data and authorization (D53 is a
  domain change); the page, the design system and accessibility; the runbook and the cast. `build-review.md`; one fix
  pass (`t361-fix`); then the squash onto master as one T361 commit with the six suites' counts. Not pushed.
- The squash names the task symptoms the build closes (T356, T273, T309 item 2, T329's review half, T260's review half,
  T324's decision form) for the replay (step 7) to confirm before any task is closed.
