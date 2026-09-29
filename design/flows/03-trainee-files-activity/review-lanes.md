<!-- Adapted from design/flows/02-sign-in-and-account/review-lanes.md (T339) for flow 03. -->

# T342 step 6: the review (read-only)

You review the branch `t342` of Wombat before it is squashed onto master as one T342 commit. `t342` is flow 03 of the
restructure: a registrar files an activity.
- **Where to read it:** the worktree `C:\Users\Renier\Wombat\.claude\worktrees\t342`.
- **The diff:** `git -C <worktree> diff master...t342`.
- **The commits:** `git log --oneline master..t342` lists the lanes' commits:
  - wave 1: A1 "lists" and A2 "filing" (backend);
  - wave 2: B Log an activity and the form, C the activity page, D My activities and Home's card;
  - two integration commits.

**Read-only.**
- Edit nothing, commit nothing, and do not build or run tests in that worktree.
- If you must run something, create your own worktree from `t342` under `C:\Users\Renier\Wombat\.claude\worktrees\`
  (for example `t342-rv-<your lens>`), and remove it (`git worktree remove`) when you are done.
- Never touch ports 5080 or 5180 or their databases. Never read `pwd_DO_NOT_COMMIT.txt`, `recovery/` or secrets.

**What the design is.**
- The accepted boards are `design/flows/03-trainee-files-activity/round-3/project/`: `R3-C-Log`, `R3-C-Activity` and
  `R3-C-Mine`, each a component whose `renderVals` holds every state's data and words.
- `R3-Spec.dc.html` is the contract: messages, kinds, rules, the DESIGN.md changes and the new parts.
- The review that shaped the build is `round-2-review.md`: corrections C1–C15, decisions E1–E9, and § For the build.
- The contracts between the lanes are in `build-contracts.md`.
- The runbook is being updated on another branch. Do not report stale runbook Expects.

**What counts as a finding.**
- A real defect:
  - wrong behaviour;
  - a security weakness, such as a read or write the caller should not have, or an id walk;
  - a broken contract with the design or with DESIGN.md;
  - a test that does not test what it claims;
  - a doc that now says something false.
- A test that models a state which cannot occur passes while the real case fails. Ask of each test whether its setup can
  happen.
- For each finding give:
  - its file and line;
  - a concrete failure scenario (inputs or state, then the wrong result);
  - a severity: high, medium or low;
  - the fix you would make.
- Verify each finding against the code before reporting it. Say "confirmed" if you traced it and "plausible" if you
  could not fully.
- Do not report style preferences, or things the design deliberately chose.
- Do not report work assigned to later flows:
  - the assessor's full redesign of the activity page, including rung radios (flow 04);
  - Home beyond the Needs you card (flow 05);
  - the builder (flow 17);
  - emails (flow 19).

Return at most 15 findings, most severe first, then one line on what you checked and found sound.
