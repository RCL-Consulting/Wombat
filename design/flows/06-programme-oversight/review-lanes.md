<!-- Adapted from design/flows/05-trainee-progress/review-lanes.md (T355) for flow 06, three reviewers. -->

# T358 step 6: the review (read-only)

You review the branch `t358` of Wombat before it is squashed onto master as one T358 commit. `t358` is flow 06 of the
restructure: watching the programme, who is behind and what has stalled.
- **Where to read it:** the worktree `C:\dev\Wombat\.claude\worktrees\t358`.
- **The diff:** `git -C C:\dev\Wombat\.claude\worktrees\t358 diff master...t358`.
- **The commits:** `git -C C:\dev\Wombat\.claude\worktrees\t358 log --oneline master..t358` lists the lanes' commits:
  - wave 0: A0 "base" (`ProgrammeScope`, the acting role's scope; `ActivityReminder` and `TraineeProfile.AdmittedOn` in
    one migration; the three pages' shells);
  - wave 1: A1 "waiting" (Waiting for assessors' read, the reminder command, its recipient port and mail, the nudge's
    setting, `WaitingList`'s staff reading, `ReminderAction`, the filter bar's pattern) and A2 "roster" (Programme
    trainees' read, filing and Nothing filed in 30 days, the digest's rule, Targets by EPA's order, one registrar's read,
    their reviews, the programme words);
  - wave 2: B "homes" (the four Homes, Application and Web; `EpaTargetCoverageList`, `RegistrarRoster`; the menus and the
    owner table), C "trainees" (Programme trainees; the registrar page; `EndedRecord` extracted from My progress) and D
    "waiting page" (Waiting for assessors);
  - the runbook-and-docs lane (runbook, coverage.md, states.md, DESIGN.md as one account), and the merges.

**Read-only.**
- Edit nothing, commit nothing, and do not build or run tests in that worktree.
- If you must run something, create your own worktree from `t358` under `C:\dev\Wombat\.claude\worktrees\` (for example
  `t358-rv-<your lens>`), and remove it (`git worktree remove`) when you are done.
- Never touch ports 5080 or 5180 or their databases. Never read `pwd_DO_NOT_COMMIT.txt`, `recovery/` or secrets.
- Nothing is pushed, though the repo has a remote.

**What the design is.**
- The accepted boards are `design/flows/06-programme-oversight/round-3/project/`: `R2-Home`, `R2-Trainees`,
  `R2-Registrar`, `R2-Waiting` and `R2-Menus`, each a component whose `renderVals` holds every state's data and words.
- `R2-Spec.dc.html` is the contract: round 3 item by item, the NEW and changed components and classes, the DESIGN.md
  rule texts, contrast, targets and the accessibility review. `R2-Steps.dc.html` lists the runbook Expects that change
  and the new steps.
- The decisions that shaped it: `round-2-ask.txt` (the pick A; Q1–Q10; C1–C11), `round-2-review.md` (findings 1–33 and
  the operator's E1–E6), `round-3-ask.txt`, and `round-3-check.md` (what the build carries, and r6 read: a withdrawn
  registrar's request is remindable). All were accepted by the operator.
- The fence (what flows 01, 04 and 05 built and flow 06 may not change) is in the brief,
  `design/flows/06-programme-oversight.md` § 1.
- The contracts between the lanes, and decisions D1–D11 (the admission day, the role a page reads as, filters in the
  address, Decision panels' owner row, who the pages admit, the same-day race, an unseen id, the registrar page's charts,
  a heading for several filters, the badge tone, Furthest short), are in `build-contracts.md`; how the lanes were cut is
  in `build-lanes.md`.
- The runbook, coverage.md and states.md were updated on this branch from the built code: a stale Expect IS a finding.

**Your lens.** Each reviewer reads the whole diff but reports through one lens:
- **R, the code, the data and authorization** (the back end and the command): `ProgrammeScope` and every read built on
  it (`WaitingForAssessorsReader`, `ProgrammeRosterReader`, `GetProgrammeTraineeQuery`,
  `ListReviewsForProgrammeTraineeQuery`, the four dashboard reads), `FilingMoments` and the weekly digest,
  `SendActivityReminderCommand`, `IReminderRecipients`, the mail, the migration (its Designer and snapshot; the backfill;
  the unique index), `ErasureExecutor`'s new rows. Can a caller read another institution's or another speciality's
  registrars or requests, or another profile than the preferred one, by naming an id or by acting in another role
  (E4: never the union)? Is an out-of-scope or unknown id "not found", never a refusal? Does the command refuse
  everything before it stages its row (the audit pipeline commits a failed handler's staged rows), leave the activity's
  `UpdatedOn`, state and transitions untouched, send to an opted-out assessor (E1), block a second sender the same South
  African day (the race too), and pseudonymise on erasure? Are role-held requests and the reader's own never listed
  (E3, E4)? Do the roster's counts agree with My progress's for the same registrar and day, exemption computed, ties by
  surname (T298), the 30 days from the later of admission and today − 30 (E5)? Is "today" South African everywhere
  (T325)?
- **D, the pages, the design system and accessibility**: every `R2-Home`, `R2-Trainees`, `R2-Registrar`, `R2-Waiting` and
  `R2-Menus` state's words and structure against the boards and the Spec; the DESIGN.md rules as pasted (the flat limit,
  the nav and owner tables, header actions, the link rule, the staff target cards, the progress figures' programme form
  and its one exemption wording, the progressbar exception, filters applied with Show); the fence (the other-role line,
  flow 04's words, flow 05's pages and words, My progress's ended view byte-equal); no "n / m", percentage,
  "inactive", "Pending reviews", role key or pronoun; ISO dates; every class defined and only the five NEW ones
  (`.roster-row`, `.coverage-row`, `.filter-actions`, `.reminder-action`, `.registrar-stack`) added, rebuilt on the
  spacing scale, the canvas's `.r2-*` and the dropped classes never shipped; no inline styles. And accessibility: every
  link named for its registrar, EPA or activity, two alike told apart "(1 of 2)" (T280); no card a link around its links;
  each card and section named by its heading; tables captioned with a row header and every stacked cell labelled; Show
  and a page turn focus the list's heading; Try again answers into the result region (Homes, lists) or its section; the
  reminder's dialog opens on Don't send, its in-flight state keeps the focus, its result or refusal takes the focus;
  every target 44 px below 641 px (Send a reminder, the filter buttons, names, feet) and ≥ 24 px above; nothing scrolls
  sideways at 390 or 768, the row grids one column below 900 px; the bars `aria-hidden` under their words.
- **G, the runbook, the cast and regressions**: every Expect `R2-Steps` lists and the new steps (3.55–3.57,
  A.5.14–A.5.16, A.7.9) against the built pages and the cast's real rows at their moment (3.30's "3 waiting, 2
  overdue", 3.53's "2 waiting, 1 overdue", A.5.10's "3 waiting, 1 overdue"; 2.33's surname order; 3.52's and 3.55's
  figures; the reminder at 3.31 and 3.32's three stub mails; A.5.14's two not-found cases; A.5.15's scratch database);
  coverage.md and states.md; steps whose setup cannot occur. And what flow 06 must not have broken: the Assessor's Home
  and inbox (flow 04's `WaitingList`, `WaitingWords`), My progress and the EPA page for the registrar (flow 05), the
  committee review page, Decision panels, Decisions due, Entrustment decisions' body, the nightly nudge and the weekly
  digest for every other recipient, the Institutional admin's and the Administrator's Homes and menus (E2's new count
  must not regroup or ungroup them), My data rights' help text; and whether each test asserts what it names, with a
  setup that can occur.

**What counts as a finding.**
- A real defect:
  - wrong behaviour;
  - a security weakness, such as a read the caller should not have, an id walk, or a reminder sent outside the scope;
  - a broken contract with the design or with DESIGN.md;
  - a test that does not test what it claims;
  - a doc that now says something false.
- A test that models a state which cannot occur passes while the real case fails. Ask of each test whether its setup can
  happen (the cast: five registrars at KGK on 11.1, two current by the appendix; Dr du Plessis's portfolio review to Dr
  Patel and Case-Based Discussion to Dr Khumalo; Dr Mahlangu's Mini-CEX to Dr Zulu, then to Dr Khumalo at A.2.7; the
  external member with no sub-speciality).
- For each finding give:
  - its file and line;
  - a concrete failure scenario (inputs or state, then the wrong result);
  - a severity: high, medium or low;
  - the fix you would make.
- Verify each finding against the code before reporting it. Say "confirmed" if you traced it and "plausible" if you
  could not fully.
- Do not report style preferences, or things the design deliberately chose (the operator's decisions in the round files,
  and D1–D11 as recorded in `build-contracts.md`).
- Do not report work assigned to later flows:
  - the committee review page, Decisions due and Decision panels' bodies (flows 07 and 08);
  - Entrustment decisions' body (flow 09);
  - Reassign (filed as its own task, Q4);
  - the reminder and nudge mails' wording beyond the one-request reminder and its days phrase (flow 19);
  - the Institutional admin's and the Administrator's Homes (flows 12 and 18).

Return at most 15 findings, most severe first, then one line on what you checked and found sound.
