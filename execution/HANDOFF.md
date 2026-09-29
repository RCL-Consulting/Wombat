# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-29 (Opus): flow 03 started (T342, in progress)

The operator asked to start T342. Steps 1–4 are done; the loop now waits on the operator's send of round 1.

### Done

- **Brief restated** (`6df626e6`): W flow, 2–3 structural variations; "what is wrong today" re-checked at `77b5892d`
  (font and tab title done; new: no "who has it now", assessor sections look open, refusal only at the head, raw
  exception text, system copy, the registrar's inbox in no nav, question 6). Key screenshots are T339's new-shell
  captures, each opened; one old-shell capture kept and marked. Quoted steps re-synced (pointers only). All three
  checks clean. Staged: 85 files plus the two seed folders (`design/upload/seeds/`, gitignored).
- **Canvas** https://claude.ai/artifact/6Uar9yce4Dxx9iPwgE8JY5, the mark copied in (`/_blob/c1b21826…`), its URL in
  § 1 and `round-1-ask.txt` (`689761f1`). Empty canvas version `1790661931-bcb3` recorded in the record README.

- **Round 1** drawn (27 boards, `round-1/`), reviewed (`round-1-review.md`): the operator accepted all, A "Pick, then
  file; status on top" with B's collapsed history at 390 and C's check line; the registrar's inbox goes (Q6); File it
  again (E1). Round 2's ask written (`round-2-ask.txt`, five corrections) (`c3ef5893`).

### Next session: T342 continues. **Model: Opus.**

- The operator sends `design/flows/03-trainee-files-activity/round-2-ask.txt`. Then: check the canvas version moved
  from `1790663484-624c`, save round 2 into `round-2/`, and run the four-sided review (flow 02's `round-2-review.md`:
  one reviewer on the code, one on the cast), then the decisions table, then round 3.
- For the build: check whether `ActivityWaiting.LoadActionableAsync` counts a cancel-only request (round-1-review.md).

### For the operator

- **P2s before real users (16):** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T342.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now also carries T339 (no migration).
- **Slips, local only:** the act 1 replay agent printed two invitation tokens (already spent) and a grep of the tracked
  dev-only DevUserSeeder passwords to the session console. Nothing reached a tracked file.

### Environment left

- **The replay app on `:5180` is stopped** (2026-09-28). Its database `wombat_scenario_t339` holds flow 02's end state;
  restart with `tools/scenario-replay.ps1 start wombat_scenario_t339 5180`.
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
