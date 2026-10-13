<!-- Adapted from design/flows/07-review-sitting/review-lanes.md (T361) for flow 08, three reviewers. -->

# T364 step 6: the review (read-only)

You review the branch `t364` of Wombat before it is squashed onto master as one T364 commit. `t364` is flow 08 of the
restructure: the committee calendar. It covers:
- Decision panels and a panel's own page, where people form the panels;
- Decisions due, where they read what the committee must decide;
- Committee reviews;
- the new schedule page, `/committee/reviews/new`, where they schedule each registrar's review.

**Where to read it:**
- The worktree: `C:\dev\Wombat\.claude\worktrees\t364`.
- The diff: `git -C C:\dev\Wombat\.claude\worktrees\t364 diff master...t364`.
- The commits: `git -C C:\dev\Wombat\.claude\worktrees\t364 log --oneline master..t364`.
  - **The shared pieces** (`94e14fee`): `ReviewLinks.Schedule` / `ScheduleReturn`, `PagerControls.PageSizeLabel`, and
    the calendar's shared CSS.
  - **Lane P, the panels** (`t364-panels`): `PanelsList.razor`, `PanelEdit.razor`, `Panels/PanelWords.cs`, the seat
    picker, `GetDecisionPanelById`'s open reviews and `DecisionBodyLock`, `DecisionBodyLockedException`, and the
    create validator's words.
  - **Lane D, Decisions due** (`t364-due`): `GetEntrustmentDecisionsDue` (rows on their own period (F1), the holding
    panel (T259), surname order, the Trainees list and seat lines, `HasEnded`), `DecisionsDue.razor` and
    `DecisionsDueText.cs`, and `BadgeFor` (Scheduled grey).
  - **Lane S, the schedule and Committee reviews** (`t364-schedule`): `ReviewsSchedule.razor` (now the list),
    `ScheduleReview.razor` and `ScheduleWords.cs` (new), `GetSchedulingSeat` (new), `CommitteeReviewSeats.HolderAsync`,
    the duplicate's wording, `WindowEndsBeforeItStarts`, the type help, the training year, and `NavOwners`.
  - **The merges.** The runbook-and-docs lane is still running; the runbook reviewer starts after it.

**Read-only.**
- Edit nothing, commit nothing, and do not build or run tests in that worktree.
- If you must run something, create your own worktree from `t364` under `C:\dev\Wombat\.claude\worktrees\` (e.g.
  `t364-rv-<lens>`), and remove it (`git worktree remove`) when done. Never pass `--no-build`.
- Never touch ports 5080 or 5180 or their databases. Never read `pwd_DO_NOT_COMMIT.txt`, `recovery/` or secrets.

**What the design is.**
- **The accepted boards:** `design/flows/08-committee-calendar/round-3/project/`, with their text in
  `round-3/boards-text.md`.
  - `R2-0-spec.dc.html` is the contract. It covers round 3 item by item (F1–F6, M1–M10, S1–S8), the components, the
    NEW classes, the DESIGN.md changes, contrast, the step counts, "Data the build must add", and the Flags.
- **The decisions behind it**, all accepted by the operator:
  - `round-2-ask.txt`: pick A; Q1–Q10 and C1–C14. Q7 is reversed by F1.
  - `round-2-review.md` and `round-3-ask.txt`.
  - `round-3-check.md`. It settles that **the default review type is the code's (`CommitteeReviewTypes.DefaultFor`):
    Annual progression for a general panel in both semesters**, whatever the S1 boards draw.
  - The brief, `design/flows/08-committee-calendar.md`, with its § 5 (known problems) and § 7 (acceptance).
- **How the lanes were cut** is in `build-lanes.md` and `build-contracts.md`.
- **What the lanes decided where the design was silent is deliberate.** Do not report these:
  - Clear filters on Decisions due keeps the institution and the period.
  - Decisions due filters the read answer in the page.
  - The list is `--stack`; By EPA is `--stack-wide`.
  - No-action notes in the action cell.
  - The trainee line is folded into the decisions-due query.
  - The schedule page opens with Period empty when it has no query.
  - A withdrawn check-in's Decision keeps T258's "None: the review was withdrawn".
  - The general panel's S1 Entrustment-only help.
  - The period options keep "2026 S1 · 1 Jan to 30 Jun 2026".
  - The Agenda aside is not sticky.
  - The panels list folds the speciality into Scope ("Speciality · Paediatrics") and shows no Open reviews column.
  - An institution-wide panel reads "Whole institution".
  - A new panel goes to `?created=1`.
  - The can-no-longer-sit warning is `role="alert"`, re-read after save.
- **The fence.** Flows 01–07 are built, and what they shipped stays unless these boards change it. The review page
  (`/committee/reviews/{id}`) is flow 07's; this flow only arrives there.

**Your lens.** Each reviewer reads the whole diff but reports through one lens:
- **R, the code, the data and authorization.**
  - `GetEntrustmentDecisionsDue`: does F1's own-period rule hold for semester, annual and optional rows, an ended
    period, and a revoked STAR? Is the holding panel right (T259)? Does the Trainees list stay inside the caller's
    reach? Do the seat lines reflect `CommitteeReviewSeats` exactly as the create would?
  - `GetSchedulingSeat` and `PreviewCommitteeAgenda`: can a caller learn of a review, a trainee or a panel outside
    their scope by naming ids? Is an unknown or out-of-scope id null, never a refusal?
  - `ScheduleCommitteeReview`: the seat check is shared, not copied; the validator refuses before anything is staged
    (the audit pipeline commits a failed handler's staged rows, T201); the wording.
  - `ListDecisionPanels` with `InstitutionId` (honoured only for an Administrator?).
  - `GetDecisionPanelById`: open reviews with `MayOpen` from the read ladder; names "as stored" for erased users
    (pseudonyms); `DecisionBodyLock`.
  - `SetDecisionPanelBody` / `DecisionBodyLockedException`: the race still refuses.
  - `CreateDecisionPanel`'s and `UpdateDecisionPanel`'s refusals.
  - `ReviewLinks.ScheduleReturn` as an open-redirect guard. Is `?created=` safe?
  - Is "today" South African everywhere (T325)?
- **D, the pages, the design system and accessibility.**
  - Every state on the boards against the built pages' words and structure, at 1280, 1024 (the schedule), 768 and
    390.
  - The words classes (`PanelWords`, `DecisionsDueText`, `ScheduleWords`) against the boards and the round files.
  - DESIGN.md's flow 08 sections as one account with `app.css`'s four "Flow 08" sections:
    - every class used is defined, and none outside those sections;
    - phone rules scoped to `.committee-calendar`;
    - no inline style, no raw hex, no native `disabled` where the design says `aria-disabled` (Cancel while busy is
      the exception);
    - no pronoun in a shipped word; ISO dates.
  - Accessibility:
    - one h1 a page, sections h2, h3 below; every table captioned, with row headers; stacked cells labelled;
    - the pager's select named; the focus on "Decisions, page n of m" after Show or a page turn;
    - a refusal summary taking the focus, each line moving it to its field;
    - the seat picker's group refusal named by `aria-describedby`, its summary link focusing the first radio;
    - one live region in the Agenda aside, with no `role="alert"` inside it;
    - row actions named for their row, visible words inside the name ("Open check-in #6 of Sipho Ndlovu");
    - targets 44 px below 641 px, ≥ 24 px above;
    - nothing scrolls sideways at 390 or 768.
- **G, the runbook, the cast and regressions** (runs after the runbook lane; its brief adds the runbook's scope).

**What counts as a finding.**
- A real defect:
  - wrong behaviour;
  - a security weakness, such as a read the caller should not have, or an id walk;
  - a broken contract with the design or with DESIGN.md;
  - a test that does not test what it claims;
  - a doc that now says something false.
- A test that models a state which cannot occur passes while the real case fails. Ask of each test whether its setup can
  happen (the cast is in acts 2, 4 and 5 of `execution/knowledge/scenario-paediatrics/`).
- For each finding give:
  - its file and line;
  - a concrete failure scenario: the inputs or state, then the wrong result;
  - a severity: high, medium or low;
  - the fix you would make.
- Verify each finding against the code before reporting it. Say "confirmed" if you traced it and "plausible" if you
  could not fully.
- Do not report:
  - style preferences;
  - things the design or the lanes deliberately chose (above);
  - work assigned to later flows (Entrustment decisions and appeals, flow 09; emails, flow 19; PDFs, flow 20);
  - the stale runbook. The runbook lane is rewriting it; reviewer G checks it.

Return at most 15 findings, most severe first, then one line on what you checked and found sound.
