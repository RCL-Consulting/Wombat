<!-- Adapted from design/flows/06-programme-oversight/review-lanes.md (T358) for flow 07, three reviewers. -->

# T361 step 6: the review (read-only)

You review the branch `t361` of Wombat before it is squashed onto master as one T361 commit. `t361` is flow 07 of the
restructure: the review sitting, where a committee stages, records and ratifies entrustment decisions. It carries D53, the
operator's decision that there is no deferral: every agenda line left unstaged at Ratify is recorded "Not decided at this
sitting", with one optional sentence on the decision.
- **Where to read it:** the worktree `C:\dev\Wombat\.claude\worktrees\t361`.
- **The diff:** `git -C C:\dev\Wombat\.claude\worktrees\t361 diff master...t361`.
- **The commits:** `git -C C:\dev\Wombat\.claude\worktrees\t361 log --oneline master..t361` lists the lanes' commits:
  - wave 1: A "no deferral" (D53 in the domain: `CommitteeDecision.UndecidedNote`, NotDecided written at Ratify, Defer
    and Reinstate removed, the migration `20261012055324_T361_NoDeferral`, E4's entrustment-only refusal, the
    decisions-due page's E3 rule, the wording) and R "reads, return link, stacking" (`TraineeProfileId`, `TrainingYear`,
    `GetStartPreviewQuery`, Start refused while the chair cannot act, `EpaInForce`, `ReviewLinks`, the activity page's
    return link, the shell's z-index and `scroll-padding-top`, `js/sticky-offset.js`);
  - wave 2: P "the review page" (`ReviewDetail.razor` as a host over `Review/`: `SittingBar`, `AgendaTable`,
    `AgendaLineForm`, `EvidenceReader`, `DecisionForm`, `ReviewWords`, `ReviewSitting`; `ConfirmDialog.BodyContent`,
    `TrajectoryChart.HeadContent`, `CommitteeEvidenceDto.AssessorName`; DESIGN.md; the bUnit tests);
  - the runbook-and-docs lane (act 4, act 5, the appendix, coverage.md, states.md, the flow briefs), and the merges.

**Read-only.**
- Edit nothing, commit nothing, and do not build or run tests in that worktree.
- If you must run something, create your own worktree from `t361` under `C:\dev\Wombat\.claude\worktrees\` (for example
  `t361-rv-<your lens>`), and remove it (`git worktree remove`) when you are done. Never pass `--no-build`.
- Never touch ports 5080 or 5180 or their databases. Never read `pwd_DO_NOT_COMMIT.txt`, `recovery/` or secrets.
- Nothing is pushed, though the repo has a remote.

**What the design is.**
- The accepted boards are `design/flows/07-review-sitting/round-3/project/` (their text in `round-3/boards-text.md`):
  the eleven `R2-*` boards, edited in place in round 3, each a component whose `renderVals` holds every state's data and
  words.
- `R2-Spec` is the contract: round 3 item by item, the components and classes, the DESIGN.md rule texts, the D53 table,
  contrast, targets and the accessibility review. `R2-Steps` lists the runbook Expects that change.
- The decisions that shaped it: `round-2-ask.txt` (the pick A; C1–C9), D53 (`execution/knowledge/EPA-PROGRAMME.md`
  § 3D), `round-3-ask.txt` (E1–E9, corrections 7–31), and `round-3-check.md` (the verdict, the points settled there:
  Refresh while Scheduled, In progress or Decided; "As of 14:31 SAST. No change."; the hint beside Record; and "What the
  build carries"). All were accepted by the operator.
- The fence: flows 01–06 are built (the brief, `design/flows/07-review-sitting.md` § 1), and what they shipped stays
  unless the boards change it; § 7 is the brief's acceptance.
- The contracts between the lanes, and what each lane decided where the design was silent, are in `build-contracts.md`
  ("As built after wave 1": amber Not decided badges, E3 taken literally, no backfill of old deferral reasons); how the
  lanes were cut is in `build-lanes.md`. The page lane's own decisions (the starter's static bar `.sitting-bar--starts`;
  Refresh's three answers; a whole re-read after every action; one result region beside the control that answered,
  refusals taking the focus; client-side refusals before a send; Present offering only members who may sit) are
  deliberate.
- The runbook, coverage.md and states.md were updated on this branch from the built code: a stale Expect IS a finding.

**Your lens.** Each reviewer reads the whole diff but reports through one lens:
- **R, the code, the data and authorization** (D53 is a domain change): `CommitteeReview` (Ratify writing NotDecided
  through `CloseAgenda`, `CloseUndecided`'s guard gone, `NothingStagedOnEntrustmentOnly`), `CommitteeDecision.Create`
  and `UndecidedNote` (trimmed, empty as null, length), `RecordCommitteeDecisionCommand` (`[Redact]`), the migration (its
  order: the check constraint, State 2→4, the dropped column, the new column; its Designer and the snapshot; Down),
  `GetEntrustmentDecisionsDue`'s E3 rule and its ordering against later and revoked sittings, `CommitteeAgendaStatus`,
  `GetStartPreviewQuery` and `CommitteeEvidenceSnapshot.BuildAsync` (does the preview read exactly what Start will
  snapshot?), `StartCommitteeReview`'s refusal, `GetCommitteeReviewById` (`TraineeProfileId` only while the trainee
  trains at the panel's institution; `AssessorName`), `EpaInForce`, and every consumer of the removed Deferred state
  (the registrar's own reviews, the schedule preview, appeals, erasure, the PDF, emails). Can a caller read the
  preview, the profile id or an assessor's name for a review outside their read ladder, or by naming an id? Is an
  unknown or out-of-scope id "not found", never a refusal? Does every command refuse before it stages a row (the audit
  pipeline commits a failed handler's staged rows)? Does a remitted appeal keep the sentence? Is "today" South African
  everywhere (T325)?
- **D, the page, the design system and accessibility**: every state in the Spec's round-3 table against the built
  page's words and structure (the chair's sticky bar and the readers' static bars, Start/Record/Ratify/Close/Remove and
  their dialogs, the opened line and drafts, the evidence reader, the decision form, the entrustment-only and formative
  reviews, withdrawn, ratified, appeals); `ReviewWords` against the boards; DESIGN.md's rewritten sections as one
  account with `app.css`; round-3-check's markup items (17: Remove… `.btn .btn-sm .btn-outline`; 20: the stacked
  agenda's explicit table roles; 14, 15: the stacking and the scroll padding); `aria-disabled` with `.is-unavailable`,
  never native `disabled`; no pronoun and no "defer" in any shipped word; ISO dates and SAST times; every class used is
  defined, none elsewhere than the review sitting section; no inline style, no z-index ≥ 10 in `app.css`. And
  accessibility: each dialog opens on its safe button, and its result or refusal takes the focus after it closes; focus
  targets clear both bars; one h1, sections h2; the agenda captioned with row headers and every stacked cell labelled;
  `aria-expanded` on Stage; each link and button named for its EPA or line, two alike told apart; every target 44 px
  below 641 px and ≥ 24 px above; nothing scrolls sideways at 390 or 768.
- **G, the runbook, the cast and regressions**: every Expect `R2-Steps` lists (4.15–4.32, 4.50, 5.3–5.6, A.7.6,
  A.7.13; 4.21 removed) and lane A's list (act 4 4.2, 4.6, 4.33, 4.34, 4.37, 4.41, 4.43, 4.48, 4.51; act 5 5.1–5.5;
  3.57; A.1.14; act 4's outcome SQL) against the built pages and the cast's real rows at their moment; the recount
  (pre-graduation 93→80; act 4 36 / 54); the brackets left for the replay (round-3-check's list), never a guessed
  figure; coverage.md and states.md; the flow briefs that quoted deferral; steps whose setup cannot occur. And what
  flow 07 must not have broken: the activity page (flow 03) without `?from=`, the shell (flow 01) at every width, My
  progress and the EPA page (flow 05), the registrar page and its reviews (flow 06), Decisions due's other statuses,
  Decision panels, Entrustment decisions, the trainee's own reviews, the scheduling form, and "Outcome Deferred" as a
  progression category; and whether each test asserts what it names, with a setup that can occur.

**What counts as a finding.**
- A real defect:
  - wrong behaviour;
  - a security weakness, such as a read the caller should not have, or an id walk;
  - a broken contract with the design or with DESIGN.md;
  - a test that does not test what it claims;
  - a doc that now says something false.
- A test that models a state which cannot occur passes while the real case fails. Ask of each test whether its setup can
  happen (the cast is in act 4 of `execution/knowledge/scenario-paediatrics/`: the panel, its chair and members, the
  registrar under review, the entrustment-only and formative reviews).
- For each finding give:
  - its file and line;
  - a concrete failure scenario (inputs or state, then the wrong result);
  - a severity: high, medium or low;
  - the fix you would make.
- Verify each finding against the code before reporting it. Say "confirmed" if you traced it and "plausible" if you
  could not fully.
- Do not report style preferences, or things the design deliberately chose (the operator's decisions in the round files,
  D53, and the lanes' decisions recorded above and in `build-contracts.md`).
- Do not report:
  - work assigned to later flows: scheduling, Decision panels' and Decisions due's page shape (flow 08), Entrustment
    decisions' body and the appeal journey beyond this page (flow 09), graduation (flow 13), emails (flow 19), PDFs
    (flow 20);
  - live following for members (T362, proposed);
  - the figures the page lane could not draw for want of data (the ratified graduation summary, "STAR #2 decided it at 4
    in this window" on a chair's line, the level and expiry on a decided line, who can start when the chair cannot act).
    These are known; say only if one is wrong rather than absent.

Return at most 15 findings, most severe first, then one line on what you checked and found sound.
