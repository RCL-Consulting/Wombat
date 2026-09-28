# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-28 (Opus): flow 02, sign-in and account, end to end (T339, done)

The operator asked to tackle flow 02, then "proceed" through each step. T339 closed with all six checkboxes backed.

### Done

- **Brief restated** (`ca3b19ff`): structure-first ASK, one proposal (an F flow), new-shell key captures checked.
- **Canvas** "Wombat 02" https://claude.ai/artifact/5BnSniM2QS8NouqWS6wsPB, mark copied in first.
- **Rounds** (record: `design/flows/02-sign-in-and-account/`):
  - R1 one structure; Q1–Q4 and T287 decided (institutions above the form; forgot = plain words, the emailed reset
    filed as T340; one-press sign-out; My account lists and removes institutional sign-ins; one refusal for a lock).
  - R2 at fidelity; four-sided review (`round-2-review.md`); E1–E11 accepted as recommended.
  - R3 checked item by item (`round-3-check.md`).
- **Build** (`f50dffb2`, pushed): waves — lane A words/endpoints and lane C My account in parallel, lane B password
  pages after; a four-sided review; one fix pass (cross-site sign-out, a stranger's lockout, the Remove dialog's
  focus, the details list, test gaps); runbook lane. 8,450 tests green. T341 filed from the review.
- **Replay** (`6da0cdff`): the whole runbook on `wombat_scenario_t339`, 246 of 325 steps with no gap; the one flow 02
  regression (Remember me at 390 px) fixed in `6735fa98`; the Account and sign-in states captured (63 rows).
- **Design system** re-synced and republished as version 7 (`56628efb`, `e33b563b`; 4 components added, 4 icons).
- **Lessons** in BRIEF § 11 "Flow 02" (`937fe19c`); § 2.3 now sends § 1 and § 8 as one message.
- **Filed:** T340 (P3, emailed reset), T341 (P2, account outcomes in the address), T342 (P2, flow 03).

### Next session: T342, flow 03 (a registrar asks for an assessment or logs one). **Model: Opus.**

- Its task file lists the loop. Templates: flow 02's `build-lanes.md`, `review-lanes.md`, `replay-workflow.js`.
- Flow 03 is marked W: round 1 asks for 2–3 structural variations. Send § 1 and § 8 together.
- The operator creates a new canvas on the Wombat design system (version 7) and gives its link.

### For the operator

- **P2s before real users (16):** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T342.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now also carries T339 (no migration).
- **Slips, local only:** the act 1 replay agent printed two invitation tokens (already spent) and a grep of the tracked
  dev-only DevUserSeeder passwords to the session console. Nothing reached a tracked file.

### Environment left

- **The replay app on `:5180`**: `wombat_scenario_t339`, built from `f50dffb2` (before `6735fa98`'s CSS). Stop it with
  `tools/scenario-replay.ps1 stop 5180`. The cast's passwords were kept only in the session scratchpad, which is
  cleared with the session: a later replay registers its own.
- **Snapshots:** `recovery/scenario-t339-post-act{1..6,A}.dump` (new), and the T335 ones.
- **Droppable scratch databases:** `wombat_scenario_t339_states_5184`, `_5184_2`, and the older `wombat_scenario_t335*`,
  `wombat_scenario_states*`, `_rc*`, `_t292`.
- **The dev app on `:5080`** still runs the build from before 2026-09-26; it locks the Web project's Release output, so run
  suites from a throwaway worktree. Its next restart applies T307's and T335's migrations.
- **Branches:** `t339*` lanes kept locally (squashed into `f50dffb2`); their worktrees are removed.

### Traps

- **The canvas does not wait for a second message,** and a send can draw nothing: read `canvas.json`'s version after
  each round.
- **A design-system publish needs a fresh read of the artifact itself** (`read` with the url, no path) after any asset
  upload; then the changed files; the index read again and sent last.
- **Replays keep capture names**; still run `check_baseline_paths.py` and open the cited images.
- **Task titles** over ~150 characters break Windows paths. **Two test helpers** named `StyleSheet.cs` and
  `Stylesheet.cs` are one file on Windows.
- **Earlier traps hold:** the audit pipeline commits a failed handler's staged rows (T201); merges are squashed per task.
