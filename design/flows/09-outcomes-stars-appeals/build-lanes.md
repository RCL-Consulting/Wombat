# T366 step 6: rules common to every build lane (flow 09, outcomes, STARs and appeals)

Adapted from `design/flows/08-committee-calendar/build-lanes.md` (T364). You are one lane of the build of flow 09 of
Wombat's GUI restructure, task T366 (`execution/tasks/in_progress/T366-*.md`). The design is final: Claude Design canvas
round 3 (version `1791966935-56f8`, `round-3-check.md`). The lane plan is at the end of this file. The names two or more
lanes share are in `build-contracts.md`, beside it.

## Read first (in your worktree)
- `design/flows/09-outcomes-stars-appeals.md`: the brief. It holds the cast, the pages table, § 5's known problems with
  file:line evidence, and the 17 runbook steps in § 8.
- `design/flows/09-outcomes-stars-appeals/round-2-ask.txt`: the pick (B: the trainee's review on a page of its own, at
  `/committee/my-reviews/{id}`, with A's inline resolve) and C1–C26.
- `design/flows/09-outcomes-stars-appeals/round-2-review.md`: M1–M11, the should-fix list, "Settled from the evidence"
  and **"Data the build must add"**.
- `design/flows/09-outcomes-stars-appeals/round-3-ask.txt`: the operator's decisions F1–F9, M1–M11 and S1–S13.
- `design/flows/09-outcomes-stars-appeals/round-3-check.md`: what the build carries (the Outcome select at 390; the
  sitting bar's words are already `ReviewWords`').
- `design/flows/09-outcomes-stars-appeals/round-3/project/`: the accepted boards (`R3-*`). `round-3/boards-text.md` is
  their text.
  - `R3-0-spec.dc.html` is the contract: round 3 item by item, the components (CHANGED and NEW), the classes, the
    DESIGN.md changes, "Data the build must add", the Flags and the accessibility notes.
  - `R3-0-steps.dc.html` lists every runbook Route, Do and Expect that changes.
  - The boards' `<helmet>` CSS is the canvas's picture, not code to copy. Rebuild each rule in `app.css` on the spacing
    scale, reusing what exists (S9).
  - A board caption is never a shipped word. A bracketed or placeholder figure (‹name›, ‹hh:mm›) is read from the data.
- **T367 is built** (`e9b29081`): `GetDecisionsForTraineeQuery` (Active, Revoked, Expired; `RevokedByName`), the
  revocation mail (`AccountNotices`, `EntrustmentDecisionRevokedEmail`), and an interim "No longer in force" on My
  authorisations that lane S replaces.
- `execution/knowledge/EPA-PROGRAMME.md`: D51 (an appeal is Dismissed or Remitted, never Upheld), D53 (no deferral).
- `execution/architecture/DESIGN.md`: mandatory before touching any Razor or CSS. Also read CLAUDE.md: its conventions
  and footguns, § Roles, and the InstitutionalAdmin scope rules (a null or not-found for an out-of-scope id, never a
  refusal that leaks it).

## Where you work
- Only in your own worktree, `C:\dev\Wombat\.claude\worktrees\<branch>` (given in your prompt), on its branch. Use
  absolute paths for every command.
  - Never touch `C:\dev\Wombat` itself (master).
  - Never merge, push or switch branch. No lane pushes, and the integrator does not either.
- Commit in your worktree as you go. Write each message to a file and use `git commit -F <file>`. End every message with
  the line `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`.
- Other lanes run in parallel on other branches. Stay inside the files your lane owns.
  - `app.css`, DESIGN.md, `CommitteeDecisionDtos.cs` and `EntrustmentDecisionDtos.cs` are shared by section: each lane writes only under its own
    heading, or adds only its own records.
  - If you must touch a file another lane owns, keep the change minimal and name it in your report.

## Never
- Touch the apps on ports 5080 and 5180 or their databases, or start anything on those ports.
- Read `pwd_DO_NOT_COMMIT.txt`, any credential, `recovery/`, or user secrets' values. Integration tests read the
  connection themselves, and each works in its own throwaway `it_<guid>` schema.
- Pass `--no-build` to `dotnet test` or `dotnet ef`.
- Add a migration. Flow 09 needs none. If you find you need one, stop and say so in your report.
- Upgrade MediatR; add Bootstrap, MudBlazor, Radzen or jQuery.
- Put any of these in shipped markup or CSS:
  - raw hex colours outside `:root`;
  - an inline `style=`;
  - `<i class="bi">`;
  - anything loaded from a third-party host;
  - an inline script or `onclick`.
- Use the native `disabled` attribute on an action the design says is unavailable. Use `aria-disabled="true"` with
  `.is-unavailable`, and put the reason beside it, named by `aria-describedby` (DESIGN.md).
  - The one exception is Cancel while busy: a disabled button, not a followable link (flow 08).
- Use a pronoun in any shipped word. Use the person's name, "the chair", "the registrar".
- Print a date in any form but ISO, or "today" from anything but `QuotaCalendar.Today(TimeProvider)` /
  `ProgrammeCalendar.DateOf(utc)`. A time is "hh:mm SAST" (`ActivityMoments.When`).
- Print an exception's text on a page. An unexpected failure is a fixed sentence, and the failure goes to the log
  (T329, T338). A handler's own refusal (an `InvalidOperationException` or validation message written in Wombat's
  words) still shows (flow 08, S4).
- Offer, draw or mention deferral (D53). "Outcome Deferred" is a progression category and stays.
- Change a seed, the credit engine, the agenda planner's rules, or the review page (`ReviewDetail.razor`, flow 07's),
  except where your lane says so.

`AuditPipelineBehavior` commits a failed handler's staged rows (T201): every command validates first, and stages only
once nothing can refuse it.

## How
- Work test-first for behaviour: write or change the test, see it fail for the right reason, then build.
- Follow the surrounding code's idiom, naming and comment density.
  - This codebase comments *why*, with task ids. Cite T366 and the item that asked, e.g. "(T366, F1)",
    "(T366, round 3 M6)", "(T366, C12; T260)".
- Layers:
  - Application handlers go through `IApplicationDbContext`.
  - Web components use `IScopedSender` and DTOs, never Domain types in `.razor`.
  - A read about a trainee or a panel goes through the existing scope helpers (`CommitteeTraineeScope`,
    `PanelAdministrationReach`, `CommitteeReviewReadAccess`). An out-of-scope or unknown id reads as not found (null).
- Strings:
  - Use straight apostrophes in app strings.
  - Every sentence that counts has its singular ("1 shown", "1 day", "1 STAR").
- Tests:
  - Change a test deliberately; never loosen it to pass.
  - `DefinedClassTests` fails any class a page uses that `app.css` does not define. Use the shared classes as given, and
    define your own under your heading.
- Each lane adds its classes under its own `/* Flow 09: … (T366) */` heading in `app.css`. Scope every phone rule to the
  page's own class (S9): `.my-review-page`, `.my-authorisations`, `.entrustment-list`, `.review-appeals`.
- Do not edit the runbook (`execution/knowledge/scenario-paediatrics/*.md`) or `design/flows/*.md`; the runbook lane
  does both after the merge.
  - T294's guard (`tests/Wombat.Web.Tests/Scenario`) must stay green. A new route needs a runbook step, so if the guard
    fails only for `/committee/my-reviews/{id}`, say so: the runbook lane adds it.
  - Name in your report every runbook line whose Expect your change alters.
- Before finishing:
  - `dotnet build C:\dev\Wombat\.claude\worktrees\<branch>\Wombat.sln -c Release`: clean, with no new warnings.
  - Then run all six suites, each with its own build: Domain, Application, Infrastructure, Architecture, Web,
    Integration (`dotnet test tests/<Project>/<Project>.csproj -c Release`). Quote the pass/fail/skip counts.

## Your report (your final message goes to the integrator, not to a person)
- Branch and commit hashes; files changed (grouped).
- Each suite's counts.
- Decisions you made that the design left open, and why.
- What you did not do, and what another lane or the integrator must know:
  - the contracts you provide or rely on: exact type, member and parameter names, and how they differ from
    `build-contracts.md`;
  - files you touched outside your list;
  - runbook steps whose Expect your change alters.

## The lane plan

Two waves, then the runbook lane.
- **Worktrees:** under `C:\dev\Wombat\.claude\worktrees\<branch>`.
- **Branches:** the integration branch is **`t366`**, which already holds § 0 of `build-contracts.md`. Each lane is cut
  from `t366` and merged back into it. The whole is squashed into one commit on master.
- **Wave 1:** lane T and lane S side by side. **Wave 2:** lane C, cut from `t366` after lane T has merged, because it
  mounts lane T's `DecisionHistory` and `AppealEntry` on the committee's page.

### Lane T "her review" (`t366-trainee`), wave 1

Covers `/committee/my-reviews` and NEW `/committee/my-reviews/{ReviewId:int}`, at 1280, 768 and 390, and the two shared
components the committee's page mounts in wave 2.

**Scope.** Boards `R3-1-*` and `R3-2-*`; round 3 F1, F6 (Lodge appeal's dialog), F8, M1, M3, S1 (her pages at 390), S5,
S10, S13, the nits on the order line, "Decisions" and the replaced entrustment-only h3; round 2's data rows for the
lodge block and the appeal entry; T308.
- **My committee reviews** (the list only; the detail below it goes):
  - newest first: `ListReviewsForTrainee` orders by scheduled date, then review id, both descending (F1);
  - a hand-written `.clinic-table.clinic-table--stack.clinic-table--stack-wide`, captioned, `scope` on headers (S8);
  - State a badge (`ReviewSitting.StateBadge`); View a link to her review's page, named by `ReviewRowNames` (M1);
  - None yet, loading and load error as drawn.
- **Her review's page** (NEW, `MyReview.razor` or the name you give it): h1 and tab the review's type label
  ("2026 S2 annual progression review"), trail Home › My committee reviews › that title; owned by My committee reviews
  in `NavOwners`.
  - **Access (F8):** only the review's own trainee, only once it is Ratified, Under appeal or Closed, for every role;
    anything else, an unknown id included, is flow 01's not found (`NavigationManager.NotFound()`). The check is in the
    handler, never the page alone; an Administrator reads a review at the committee's address.
  - **Order:** header (its facts in a `.detail-list`), Decisions, Appeal, Agenda (the nit). A formative check-in shows
    "A formative check-in records no decision and issues no STAR." (NEW) and no Appeal.
  - **Decisions:** `DecisionHistory` (contract § 1), newest first; a replaced decision says so; the entrustment-only
    replaced h3 reads "Entrustment decisions only, replaced on appeal".
  - **Appeal:** her `AppealEntry` list (contract § 1); while Ratified, the lodge block: "What happens next" naming the
    appeal body by name and every reader truthfully (the panel's institution by name), the reason box with its counter
    (a second `.field-help`, `aria-describedby`), a blank press refused "Give your reason for appealing.", Lodge appeal
    opening the `ConfirmDialog` as built ("Lodge your appeal?", focus Keep editing; Keep editing returns the focus to
    Lodge appeal with the text kept, F6). After Lodge, "Appeal lodged." with the focus, and the page reads the review
    again (State Under appeal, the open entry, no form). A refused lodge ("Only ratified reviews can be appealed.", M3)
    shows with the focus and the page reads the review again (S5).
  - **Agenda:** read-only, flow 07's `AgendaTable` as the reader sees it.
  - Loading and failed heads read "Committee review" (S12).
- **Data** (round 2's table): the appeal body read for her own Ratified review (`AppealBodyOf`, today Under appeal
  only); the review read again after Lodge (`WithNamesFrom` copies the empty body); the panel's institution name;
  `LodgedByName` and `ResolvedByName` on `CommitteeAppealDto`; `BadgeFor.Appeal`.

**Files owned.**
- Application: `src/Wombat.Application/Features/CommitteeDecisions/{ListReviewsForTrainee,GetCommitteeReviewById,
  LodgeAppeal,CommitteeReviewReadAccess}.cs` (the last only to add the trainee's own-review read; say so), NEW
  `GetMyCommitteeReview.cs` (or the name you give it), and their records in `CommitteeDecisionDtos.cs`.
- Web: `src/Wombat.Web/Components/Pages/CommitteeDecisions/MyReviews.razor`, NEW `MyReview.razor`, NEW
  `Components/Pages/CommitteeDecisions/Review/{DecisionHistory,AppealEntry}.razor`, `Review/DecisionForm.razor` (its
  read branch becomes `DecisionHistory`, nothing else), `Components/Shared/BadgeFor.cs` (the appeal mapping only),
  `src/Wombat.Web/Navigation/{NavItems,NavOwners}.cs` (the new page's owner and title only), the counter's and the
  clamp's script module under `wwwroot/js/` (CSP nonce; no inline script).
- Shared: `app.css` (`/* Flow 09: her review (T366) */`): `.decision-history`, `.decision-record`, `.decision-replaced`,
  `.decision-note`, `.appeal-list`, `.appeal-entry*`, `.appeals-who`, `.lodge-*`, the `.detail-list dd .badge` alignment,
  the phone rule scoped to `.my-review-page` and `.appeal-entry-more`. DESIGN.md: § Badges (the appeal's three), §
  Page-level patterns (My committee reviews' State a badge, View named by the review; her page; loading heads), §
  Entrustment-only reviews "The trainee's reviews", § Form system (the counter), the owner table's new row.

**Tests.**
- Application: the list's order (F1); her page's read (own trainee only, Ratified or later only, every role; unknown and
  out of reach are null); the appeal body read while Ratified; `LodgedByName`/`ResolvedByName`; the lodge refusal's
  words; the re-read after Lodge.
- Web: `MyReviews*Tests` (rewrite); NEW for her page's states on the boards (ratified with the lodge block, lodged,
  refused then re-read, closed after a remit and a dismissal, formative, pre-graduation, not found, loading, load error);
  `DecisionForm` tests still green with `DecisionHistory` lifted; `Navigation/*`, `Hosting/PageTitleTests`,
  `Accessibility/*`.

### Lane S "the STARs" (`t366-stars`), wave 1

Covers `/portfolio/authorisations` and `/admin/entrustment-decisions`, at 1280, 768 and 390.

**Scope.** Boards `R3-5-*` and `R3-6-*`; round 3 F2, F3, F4, F9, M4, M5, M6, M7, M9, M10 (from the data), S1 (the
register's states), S2, S3, S4, S6, S7, S11; round 2's data rows for STARs; T272 (the download's words), T324 (the
certificate's reviser by name), T325 (Expires on the South African calendar).
- **My authorisations** (replaces T367's interim section): "In force" as `.star-grid` cards (equal columns: one at 390,
  as many as fit at 768 and 1280), each with its level, Issued, Expires ("No expiry"; "Expires in N days", "Expires in
  1 day", "Expires today" on the South African calendar, S3), the rationale, and "Download certificate" named "Download
  certificate for PAED-001" (F4); its empty sentence when none is in force (S2). Then "No longer in force", a
  hand-written stacking table (S8): Revoked and Expired with Since (a revoke's South African day; an expiry's day after
  `ExpiresOn`), Why (the reviser by name and the reason; "Its expiry date, 2026-11-02, passed."), and "Download" named
  "Download certificate for PAED-002 (revoked)". The download's running, refused and failed words (C19, T272; never an
  exception's text).
- **The register:** the FilterBar pattern with **Show** and Clear filters (F9); a visually hidden `role="status"` with
  "N shown" after Show (S11); a hand-written stacking table (S8), newest issued first then STAR id descending; Issued
  with its review link (`.issued-review`, 24 px, 44 px at 390, M9) only where `IssuingReviewReadable`, plain text
  otherwise (S7); the row line through flow 03's `.has-note` / `.history-note` pair generalised: "Revoked {date} at
  {hh:mm} SAST by {name}: {reason}" and "Superseded by STAR #18, issued 2026-10-13." (S4); Download on every row,
  Revoke on Active only, both named per row as today.
- **Revoke:** `ConfirmDialog` CHANGED (M5, contract § 2): "Revoke {name}'s {EPA} STAR?", the first focus in Revocation
  reason, Revoke the STAR always enabled; blank refused "Give a reason for the revocation." under the box with the
  focus there, the dialog open (F3, M6); a stale revoke refused in the dialog with "Only active entrustment decisions
  can be revoked.", the reason kept (M4); the mail sentence "… unless her account is deactivated." (S6, true since
  T367); the result "{name}'s {EPA} STAR is revoked." with the focus.
- **Data:** `RevokedByName` on the register's DTO (the trainee's is T367's); `IssuingReviewReadable` through
  `CommitteeReviewReadAccess`; the successor's STAR number and `IssuedOn` for the superseded line; the certificate's
  reviser by name (`EntrustmentCertificatePdfService`, today the id at `:189`).

**Files owned.**
- Application: `src/Wombat.Application/Features/EntrustmentDecisions/*` (T367's notice in `RevokeEntrustmentDecision.cs`
  stays as it is), and their records in `EntrustmentDecisionDtos.cs`.
- Infrastructure: `src/Wombat.Infrastructure/Reporting/EntrustmentCertificatePdfService.cs` (the reviser's name only).
- Web: `src/Wombat.Web/Components/Pages/Portfolio/MyAuthorisations.razor`,
  `src/Wombat.Web/Components/Pages/Admin/EntrustmentDecisions/Index.razor`, `Components/Shared/ConfirmDialog.razor`
  (contract § 2, additive only).
- Shared: `app.css` (`/* Flow 09: the STARs (T366) */`): `.stars-section`, `.star-grid`, `.star-card`, `.star-rationale`,
  `.download-result`, `.issued-review`, the generalised note-row selectors, the phone rule scoped to
  `.my-authorisations` and `.entrustment-list`. DESIGN.md: § Button system (M5's dialog sentence, from the Spec), §
  Table system (S8's sentence; the note row generalised), § Accessibility (F4's label in name; S11's status).

**Tests.**
- Application: the register's `RevokedByName`, `IssuingReviewReadable` (readable, not readable, out of scope), the
  superseded line's data; the certificate's reviser by name (Infrastructure).
- Web: `MyAuthorisations*` and `EntrustmentDecisions*` tests (rewrite); `ConfirmDialog` tests for the two new modes and
  the old modes unchanged; NEW per board state (none, three with one expiring, none in force, after a revocation, an
  expired one, fifteen; the register's none, five, filtered with the status, the revoke's open, blank, refused and done,
  the superseded line, plain Issued); `DefinedClassTests`.

### Lane C "the committee's appeal" (`t366-committee`), wave 2

Covers the Appeals section of `/committee/reviews/{ReviewId:int}` (flow 07's page) and its sitting bar, at 1280, 768 and
390.

**Scope.** Boards `R3-3-*`; round 3 F5, F6 (Remit and Dismiss's dialogs), F7, M2, M8; the nits on the quorum
refusal's lead and board 3 at 390; T323 (the Outcome readable in full; the resolve form one column, 48rem); T213, T237
(the who-may line).
- The Appeals section mounts lane T's `AppealEntry` for each appeal, and the page's decision history is lane T's
  `DecisionHistory` (flow 07's read branch, lifted).
- **Go to the appeal** (F5): an outline small button in the reader's sitting bar while the review is Under appeal; it
  moves the focus to the Appeals h2 (`tabindex="-1"`) through `PageFocus`, never an `href="#appeals"` (`<base href="/">`
  makes it a navigation to Home). At 1280 at the bar's right end; at 390 under the state line, full width, 44 px.
- **The resolve form:** one column, 48rem; Outcome "Select an outcome…", "Dismissed: the decision stands", "Remitted:
  the appeal body replaces the decision" (no Upheld); the quorum line from `ReviewWords.Quorum` beside Resolve appeal
  (`aria-live="polite"`); a refusal after the form's actions, led "Resolve appeal was refused:" (M8, the nit); Remit
  and Dismiss confirm through `ConfirmDialog` as built, Go back returning the focus to Resolve appeal (F6). If the
  Outcome select still cuts its option at 390 once built, say so: radios (flow 08's choice-group) are the fallback.
- **The who-may line** (F7): "Only the appeal body can resolve the appeal: …" names only those who may act now; a
  reader holding a seat who cannot act reads the reader's view.

**Files owned.**
- Web: `src/Wombat.Web/Components/Pages/CommitteeDecisions/ReviewDetail.razor` (the Appeals section, its code region,
  and the bar's jump only), `Review/SittingBar.razor`, `Review/ReviewWords.cs`, `Review/ReviewSitting.cs` (the jump only).
- Application: `ResolveAppeal.cs` only if a refusal's words must change (say so).
- Shared: `app.css` (`/* Flow 09: the committee's appeal (T366) */`): `.resolve-form`, the bar's jump, the phone rule
  scoped to `.review-appeals`. DESIGN.md: § Detail page (the reader's bar carries the jump), § The chair's controls (the
  resolve form's quorum line and refusal, from the Spec), § Form system (T323).

**Tests.**
- Web: `ReviewDetail*` appeal tests (rewrite where the section changed); NEW: the jump shown only while Under appeal and
  moving the focus; the quorum line; the refusal's place and lead; Go back's focus; C13's who-may line.

### After the waves (the integrator, then the runbook lane)
- **Merge** T and S into `t366`, run the six suites, then cut lane C; merge C. Settle `CommitteeDecisionDtos.cs`,
  `EntrustmentDecisionDtos.cs` and `app.css` by keeping every lane's section. Make DESIGN.md one account, its banner
  naming flow 09.
- **Runbook-and-docs lane** (`t366-runbook`), from the built code: every Route, Do and Expect `R3-0-steps` lists for
  the 17 steps (4.35–4.37, 4.39, 4.41–4.48, 4.51, 5.7, 5.8, 6.22, A.7.7) and the Gaps they close (F-4.36a, F-4.37a,
  F-4.43a, F-4.45c, F-4.48a, T260's entrustment half); `coverage.md` (`/committee/my-reviews/{id}`); `states.md`
  (the expired STAR as F2's row; the register's and her page's states); `design/flows/09-outcomes-stars-appeals.md`.
- **The build's review:** three read-only reviewers on Sonnet (code, data and authorization; the pages, the design
  system and accessibility; the runbook and the cast), then `build-review.md`, one fix pass (`t366-fix`), and the squash
  onto master as one T366 commit with the six suites' counts.
- **The squash names the task symptoms the build closes:** T308; T319's revocation half (with T367); T323; T324's
  certificate; T325's Expires; T272's download words; F-4.45c and the quorum refusal at the head (BRIEF § 6). The
  replay (step 7) confirms them before any task is closed.
