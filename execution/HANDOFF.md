# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-20b (Opus) — readiness check on the new workspace, two register fixes

No application code changed. This session audited the workspace the previous one bootstrapped,
and corrected two places where the register disagreed with the prose.

### Done

- **Verified the harness end to end.** `lint --strict` clean; SessionStart `context` emits valid
  hook JSON; Stop `check-handoff` resolves and exits 0; no duplicate ids across lanes; all 115
  task files have a `status:` field matching their lane; DASHBOARD in step; tree clean and level
  with `origin/master`. `.claude/settings.local.json` is permissions-only and gitignored, so it
  does not shadow the hooks in the tracked `settings.json`.
- **Moved T128 to `blocked/`.** STATE's `## Blockers`, HANDOFF and the task file all called it
  blocked on an operator decision, but it sat in `queued/` — so the session-start bundle offered
  it as an ordinary P1 and the next session would have picked it up and stalled. `blocked_on` now
  records the decision: destination, and who holds the `age` private key.
- **Retitled T102.** Was *"A trainee can name themselves as their own assessor and self-award
  entrustment credit."* That path closed with T070 (`ThrowIfActorFieldNamesSubject`, `c33c14b`),
  which the file said only in a Progress section at the foot — while the title, which is what the
  bundle renders, still advertised the fixed defect. Now: *"A `user`-typed field accepts any user
  id, unchecked for role or scope."* The original framing is kept in the body.
  **The filename keeps the old slug deliberately** — `EPA-PROGRAMME.md:571` cites the path.

### Upstream, in `C:\dev\rcl_execution`

- **rcl-harness 2.4.3** — `fix(lint): a git ref is not a dead route`. `TICKED_REF` claims any
  backticked token carrying a slash and calls it a path, so this very handoff failed the lint
  for saying the tree was level with `origin/master`. `stale_refs` now skips `refs/`,
  `origin/` and `upstream/`; the guard is anchored, so a real path merely containing a remote
  name is still caught. A remote named anything else still trips — reword, or add it to
  `GIT_REF`. `TestGitRefsAreNotDeadRoutes`, 4 cases, three verified to fail with the guard
  removed. Suite **123 → 127**. The sentence above is the live proof.

### Spot-check of W-002 (lanes were derived, not read)

Four of 26 sampled against the code. **None misfiled.**

- **T113** — open, confirmed. `GetCurriculumProgressForTraineeQuery` and `ListReviewsForTraineeQuery`
  both still take a bare `string TraineeUserId` with no `ClaimsPrincipal`.
- **T110** — open, confirmed. `or_scale` still in the `acat`/`cbd`/`dops`/`mini_cex` source seeds;
  `CreditApplier` resolves `scale_key` by numeric id or exact name only.
- **T102** — partially shipped, as above. The file was honest; only the title was stale.
- **T099** — accurate: dev done, production outstanding.

22 remain unchecked. The derivation held everywhere it was pushed on, which is evidence, not proof.

### Next

1. **T128 is blocked on you** — destination + key holder. Nothing in code waits on it.
2. Ready P1s: **T099** (confirm what remains beyond "dev done"), **T102** (fixes 2 and 3 — validate
   a submitted `user` value against role and scope), **T121**, **T122**.

### Traps

- **The nightly backup exits 1 on purpose.** That is the control working, not a fault.
- **Both hooks hard-code `C:\dev\rcl_execution\bin\harness.py`.** That coupling is invisible from
  inside this repo; if that checkout moves, the hooks go with it.
- **`sha256sum` the deployed file against the repo before believing any doc about the server.**
  `wombat.service`, the Caddyfile and `appsettings.Production.json` are still install-once-by-hand.
  `deploy/verify/drift-check.sh` does this for you.
- **Two path styles live here**, forward-slash and Windows backslash. A sweep for one misses the other.
- The solution build and the per-project tools resolve **different** Release output trees, Any CPU
  versus x64. Never pass `--no-build`.
- Paths in `log/` and in completed task files still read `Rewrite/` or `Programme/`. They were
  correct when written and are deliberately left alone.

### Verification status

- `harness.py lint --strict` — **clean**, after both changes.
- **No build or test run this session**; no application code changed. Last known 875 green
  (2026-09-19); Integration suite is Docker-gated and was not run.
- Previous session's evidence stands: `dotnet build Wombat.sln -c Release` 0/0,
  `drift-check.sh` exit 0, `restore-rehearsal.sh` clean (444 TOC entries, 31 migrations).
