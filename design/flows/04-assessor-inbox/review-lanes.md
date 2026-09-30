<!-- Adapted from design/flows/03-trainee-files-activity/review-lanes.md (T342) for flow 04. -->

# T350 step 6: the review (read-only)

You review the branch `t350` of Wombat before it is squashed onto master as one T350 commit. `t350` is flow 04 of the
restructure: an assessor works their inbox.
- **Where to read it:** the worktree `C:\Users\Renier\Wombat\.claude\worktrees\t350`.
- **The diff:** `git -C <worktree> diff master...t350`.
- **The commits:** `git log --oneline master..t350` lists the lanes' commits:
  - wave 1: A1 "waiting" (the one waiting read, Decided by you, the waiting row) and A2 "words" (sentence-case moves,
    running labels, the note-panel words, the two-renderings CSS);
  - wave 2: B "lists" (the Assessor's Home, the other-role line, the two-section inbox), C "page" (the way on, the note
    panel, Discard's reason, Activity unavailable, About on a phone), D "form" (the rated-level picker, the request
    fold, people by name);
  - the runbook-and-docs lane (runbook, states.md, DESIGN.md as one account), and the integration merges.

**Read-only.**
- Edit nothing, commit nothing, and do not build or run tests in that worktree.
- If you must run something, create your own worktree from `t350` under `C:\Users\Renier\Wombat\.claude\worktrees\`
  (for example `t350-rv-<your lens>`), and remove it (`git worktree remove`) when you are done.
- Never touch ports 5080 or 5180 or their databases. Never read `pwd_DO_NOT_COMMIT.txt`, `recovery/` or secrets.

**What the design is.**
- The accepted boards are `design/flows/04-assessor-inbox/round-3/project/`: `R3-C-Home`, `R3-C-Inbox` and
  `R3-C-Activity`, each a component whose `renderVals` holds every state's data and words.
- `R3-Spec.dc.html` is the contract: messages, kinds, rules, the DESIGN.md changes and the new parts.
- The review that shaped the build is `round-2-review.md`: corrections C1–C13, decisions E1–E5, and § For the build; round 1's
  decisions (Q1–Q7, E1–E5, R1–R4) are in `round-1-review.md`. All were accepted by the operator.
- The contracts between the lanes are in `build-contracts.md`.
- The runbook and states.md were updated on this branch from the built code: a stale Expect IS a finding.

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
  - the committee Home beyond the other-role line's slot (flow 07);
  - the trainee's progress and Home beyond Needs you (flow 05);
  - the builder (flow 17);
  - emails (flow 19).

Return at most 15 findings, most severe first, then one line on what you checked and found sound.
