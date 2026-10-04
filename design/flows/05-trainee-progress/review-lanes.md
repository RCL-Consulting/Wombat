<!-- Adapted from design/flows/04-assessor-inbox/review-lanes.md (T350) for flow 05. -->

# T355 step 6: the review (read-only)

You review the branch `t355` of Wombat before it is squashed onto master as one T355 commit. `t355` is flow 05 of the
restructure: a registrar reads where they stand.
- **Where to read it:** the worktree `C:\dev\Wombat\.claude\worktrees\t355`.
- **The diff:** `git -C C:\dev\Wombat\.claude\worktrees\t355 diff master...t355`.
- **The commits:** `git -C C:\dev\Wombat\.claude\worktrees\t355 log --oneline master..t355` lists the lanes' commits:
  - wave 1: A1 "counts" (the counts' new fields and the paused group, one EPA's read, the trajectory's read with
    "against the minimum then" computed live, ISO dates, the progress, EPA-page and trajectory words, the paused mark)
    and A2 "decisions" (Recent decisions on her requests, the count a decision made, the standing's shared reader,
    Home's read reshaped, the activities on one EPA, the count-line, Home and standing words, the 44 px block links);
  - wave 2: B "home" (the Trainee's Home in four cards; the completed card's count and where "Open My progress" lands;
    My activities' Credit link), C "progress" (My progress: This period, the EPA index, the paused group, section errors;
    the standing panel whole, stacked and linked, for both pages), D "epa" (the EPA page; the trajectory in its new form
    on the EPA page and the committee page; the owner rule for a page under a personal link);
  - the integration (`ReviewDetail`'s panel links), the runbook-and-docs lane (runbook, coverage.md, states.md,
    DESIGN.md as one account), and the merges.

**Read-only.**
- Edit nothing, commit nothing, and do not build or run tests in that worktree.
- If you must run something, create your own worktree from `t355` under `C:\dev\Wombat\.claude\worktrees\` (for example
  `t355-rv-<your lens>`), and remove it (`git worktree remove`) when you are done.
- Never touch ports 5080 or 5180 or their databases. Never read `pwd_DO_NOT_COMMIT.txt`, `recovery/` or secrets.
- Nothing is pushed; there is no remote to push to.

**What the design is.**
- The accepted boards are `design/flows/05-trainee-progress/round-3/project/`: `R3-C-Home`, `R3-C-Progress`,
  `R3-C-Epa`, `R3-C-Trajectory` and `R3-C-WaysIn`, each a component whose `renderVals` holds every state's data and words.
- `R3-Spec.dc.html` is the contract: messages and their words, the DESIGN.md rules R1–R5, the new parts and classes,
  contrast, focus, the reads. `R3-Steps.dc.html` lists the runbook Expects that change.
- The reviews that shaped the build: `round-1-review.md` (V1 with V2's Credited lately widened; corrections 1–6; Q1–Q9,
  O1, O2, R1–R5) and `round-2-review.md` (C1–C13, E1–E6, and the build notes 1–16: § For the build is notes 1–7, §
  Nits notes 8–16 in their order). `round-3-check.md` explains the three "partly" rows. All were accepted by the
  operator; `README.md` records the decisions.
- The fence (what flows 01, 03 and 04 built and flow 05 may not change) is in the brief,
  `design/flows/05-trainee-progress.md` § 1.
- The contracts between the lanes, and decisions D1 (ISO dates) and D2 (the committee chart's window), are in
  `build-contracts.md`; how the lanes were cut is in `build-lanes.md`.
- The runbook, coverage.md and states.md were updated on this branch from the built code: a stale Expect IS a finding.

**Your lens.** Each reviewer reads the whole diff but reports through one lens:
- **R, correctness and security** (the back end): every new read (`GetEpaProgressForTraineeQuery`, `DecidedOnYours`,
  `EpaCountLines` and `GetActivityCountLineQuery`, `EntrustmentStandingReader`'s summary mode, the trajectory's new
  fields and EPA filter, `ListActivitiesBySubjectQuery`'s EPA filter, the reshaped Home summary). Can a caller read
  another trainee's EPA, count, decisions or ratings by naming an id? Is an out-of-scope id "not found", never a refusal
  or a different empty? Do the counts agree with My progress's and with the stored tallies; does "against the minimum
  then" agree with what credit decided for the same rating (E2: the same comparer, the training year at the
  encounter, the item's pinned scale); does a paused EPA count nowhere (D48), an encounter after the last day credit
  nothing (T281), December count into Semester 2 (D40); does Recent decisions follow the rule (a create, a return to her
  draft, her own log and MSF are never decisions; Declined, Rejected and every terminal state are); is "today" South
  African (T325)?
- **D, the pages against the design and DESIGN.md**: every `R3-H-*`, `R3-P-*`, `R3-E-*`, `R3-T-*` and `R3-W-*` state's
  words and structure against the boards and the Spec's § 1; R1–R5 as pasted; the fence (NeedsYouList, My activities
  but its Credit cell, the status card's other parts and words, the other-role line, the menu: Q8); the three "per"s
  never mixed and no "n / m", percentage, lifetime total or bare "year"; ISO dates (D1); MSF never counted or plotted,
  `MsfCoverageText`'s words whole; the panel whole (C1) and the same on both pages; tokens only, every class defined,
  the CSS rebuilt on the spacing scale rather than copied (the canvas's `.r2-*` and `.standing-summary` never shipped).
- **A, accessibility and focus**: the EPA page's h1 focused on arrival and again once loaded (C8), never by a script or
  `?epa`; the always-present loading statuses; Try again's answers (This period's heading, `#standing-h`, the EPA page's
  h1, the alert when it fails again); every target 44 px below 641 px (C9's list) and ≥ 24 px above; every link named
  for its EPA or activity (T280), no card a link around its links; the chart's region named and in the tab order, its
  SVG named for what it adds, its table visible with a row header (note 11), nothing under 11 px (note 13); the index's
  and panel's captions and row headers, stacked cells labelled; "(no longer in use)" and the KGK badge in text; a
  figure never a bare bar; contrast as the Spec's § 4.
- **G, regressions outside flow 05, and the tests' honesty**: the committee review page (the panel's links, the chart's
  review window D2, no "Today", headings under its h3; nothing else of flow 07 changed), the ended view of My progress
  (flow 13's, R5) and the graduate's Home, My authorisations, the portfolio PDF (`PortfolioEpaProgress`, D1's
  boundary), the coordinator's MSF text, the owner table's other pages (E3's one rule must not light My progress where
  it should not), My activities and the activity page for every other reader; and whether each test asserts what it
  names, with a setup that can occur.

**What counts as a finding.**
- A real defect:
  - wrong behaviour;
  - a security weakness, such as a read the caller should not have, or an id walk;
  - a broken contract with the design or with DESIGN.md;
  - a test that does not test what it claims;
  - a doc that now says something false.
- A test that models a state which cannot occur passes while the real case fails. Ask of each test whether its setup can
  happen (the cast's real rows: Dr Molefe's six WBAs and two MSF rows, Dr Dlamini's paused PAED-012, KGK-001, the 11.2
  move with T304's replay).
- For each finding give:
  - its file and line;
  - a concrete failure scenario (inputs or state, then the wrong result);
  - a severity: high, medium or low;
  - the fix you would make.
- Verify each finding against the code before reporting it. Say "confirmed" if you traced it and "plausible" if you
  could not fully.
- Do not report style preferences, or things the design deliberately chose (the operator's decisions in `README.md`, and
  D1 and D2 as recorded in `build-contracts.md`).
- Do not report work assigned to later flows:
  - the committee review page beyond the trajectory's window and the panel's links (flow 07);
  - My progress and Home once the programme has ended, beyond keeping them as built (flow 13);
  - My authorisations and its certificates (flows 09 and 13);
  - the portfolio PDF (flow 20);
  - emails, including the missing decline mail (flow 19, T320).

Return at most 15 findings, most severe first, then one line on what you checked and found sound.
