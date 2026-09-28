<!-- Kept from T335: the brief every reviewer of the integration branch read before its lens. Reuse per flow. -->

# T335 step F: the review (read-only)

You review the branch `t335` of Wombat, the flow 01 restructure (the shell), before it is squashed onto master as one
T335 commit. Read the branch in its worktree, `C:\Users\Renier\Wombat\.claude\worktrees\t335`. The diff is
`git -C <worktree> diff master...t335`; `git log --oneline master..t335` lists the lanes' commits.

**Read-only.**
- Edit nothing, commit nothing, and do not build or run tests in that worktree: a suite run is using it.
- If you must run something, create your own worktree from `t335` under
  `C:\Users\Renier\Wombat\.claude\worktrees\` (for example `t335-rv-<your lens>`), and remove it when done.
- Never touch ports 5080 or 5180 or their databases.
- Never read `pwd_DO_NOT_COMMIT.txt`, `recovery/` or secrets.

**What the design is.**
- The accepted boards are `design/flows/01-shell/round-3/*.dc.html`. They are HTML mockups; read their text and inline
  styles. The rules are in `R2-Rules.dc.html` and the token sheet in `R2-Tokens.dc.html`.
- The record is `design/flows/01-shell/README.md`.
- The review that shaped the build is `design/flows/01-shell/round-2-review.md` (S1–S22, D1–D11, N1–N7, and the Step F
  plan).
- The decisions are W-008, W-010 and W-011 in `execution/DECISIONS.md`.
- One integrator decision: D1 is built as a column on the account carried as a claim, not a cookie.

**What counts as a finding.**
- A real defect: wrong behaviour, a security weakness, a broken contract with the design or with DESIGN.md, a test
  that does not test what it claims, or a doc that now says something false.
- Give each finding:
  - its file and line;
  - a concrete failure scenario (inputs or state → wrong result);
  - a severity (high, medium or low);
  - the fix you would make.
- Verify each one against the code before reporting it. Say "confirmed" if you traced it and "plausible" if you could
  not fully.
- Do not report style preferences, or things the design deliberately chose.
- Do not report work the brief assigns to later flows: card contents, the activity page's own design, emails, PDFs.

Return at most 15 findings, most severe first, then one line on what you checked and found sound.
