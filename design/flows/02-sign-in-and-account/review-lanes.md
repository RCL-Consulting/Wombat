<!-- Adapted from design/pilot/review-lanes.md (T335) for flow 02. -->

# T339 step F: the review (read-only)

You review the branch `t339` of Wombat, flow 02 of the restructure (sign-in and account), before it is squashed onto
master as one T339 commit. Read the branch in its worktree, `C:\Users\Renier\Wombat\.claude\worktrees\t339`. The diff
is `git -C <worktree> diff master...t339`; `git log --oneline master..t339` lists the lanes' commits (lane A words,
outcomes and endpoints; lane C My account and T286's half; lane B the password pages, the toggle and DESIGN.md).

**Read-only.**
- Edit nothing, commit nothing, and do not build or run tests in that worktree.
- If you must run something, create your own worktree from `t339` under `C:\Users\Renier\Wombat\.claude\worktrees\`
  (for example `t339-rv-<your lens>`), and remove it (`git worktree remove`) when done.
- Never touch ports 5080 or 5180 or their databases. Never read `pwd_DO_NOT_COMMIT.txt`, `recovery/` or secrets.

**What the design is.**
- The accepted boards are `design/flows/02-sign-in-and-account/round-3/*.dc.html` (and `round-2/` for screens round 3
  did not redraw). `R3-Spec.dc.html` is the contract: messages, owners, kinds, the focus rule, the six rules, the
  DESIGN.md changes, the new parts.
- The record is `design/flows/02-sign-in-and-account/README.md`; the review that shaped the build is `round-2-review.md`
  (C1–C9, E1–E11, "For the build"); `round-3-check.md` § Carried into the build.
- The runbook is being updated on another branch; do not report stale runbook Expects.

**What counts as a finding.**
- A real defect: wrong behaviour, a security weakness, a broken contract with the design or with DESIGN.md, a test
  that does not test what it claims, or a doc that now says something false.
- A test that models a state which cannot occur (a cross-site post carrying a SameSite=Lax cookie; a stylesheet rule
  read without the cascade) passes while the real case fails: ask of each test whether its setup can happen.
- Give each finding: its file and line; a concrete failure scenario (inputs or state → wrong result); a severity (high,
  medium or low); the fix you would make.
- Verify each one against the code before reporting it. Say "confirmed" if you traced it and "plausible" if you could
  not fully.
- Do not report style preferences, or things the design deliberately chose. Do not report work assigned to later flows
  (register's and the admin reset card's own redesign: flows 11 and 12; emails: flow 19; T340).

Return at most 15 findings, most severe first, then one line on what you checked and found sound.
