# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-26/27 (Opus): the redesign's pilot, flow 01, end to end

The operator asked whether Claude Design needs a workflow list (yes), then "do it all", then "restructure because I
want it to look at UX also, include mails and pdf and set up the pilot", and "accept all" on round 2's decisions.

### Done

- **Groundwork (2026-09-26):** the runbook became the journey catalogue (T292–T296), and the brief was written (T332,
  `design/BRIEF.md`). Group 1 (T297, T300, T302, T303, T307) was fixed before designing.
- **The pilot, T335, done:**
  - **Design:** three structural variations; A picked, with C's breadcrumbs; round 2 reviewed from four sides; D1–D11
    accepted; round 3 checked on 30 fixes. Canvas https://claude.ai/artifact/R86QvLEyyfKx98fT4MENcD; record in
    `design/flows/01-shell/`. Decisions W-008 to W-011.
  - **Build (`b347e11c`):**
    - six lanes: tokens and type, the acting role (stored with the account), reconnect and the error bar, titles, the
      shell, and Home with the failure pages;
    - a four-sided review, then fixes;
    - 8,226 tests green.
  - **Replay:** the whole runbook on a fresh database (`wombat_scenario_t335`). 243 of 325 steps had no gap, and there
    was no regression from flow 01. The other steps are open tasks already cited, one Expect corrected, and three not
    played (T337).
  - **Lessons:** BRIEF § 11 written; § 0, § 2.3, § 2.4, § 5.1 (the digest, in all 18 briefs) and § 9 corrected; flow
    briefs' capture citations checked against the images.
  - **The design system re-synced** to flow 01 and republished (`064cde00`; https://claude.ai/artifact/RsbreZ2d94q2NUNQMLch18
    v4). The real mark is copied into the canvas.
- **Closed by the pilot:** T190, T321, T322, T330 and T331. "As built" notes are on T317, T328 and T329 (still open).
  Filed: T337 (the replay tool's gaps) and T338 (small leftovers).

### Next session: T339, flow 02 (sign-in and account). **Model: Opus.**

- **2026-09-28: step 1 done (`ca3b19ff`):** the brief is restated, and the upload set is staged (109 files in
  `design/upload/`). Canvas "Wombat 02" https://claude.ai/artifact/5BnSniM2QS8NouqWS6wsPB, mark copied in (`ea1f316e`).
  Rounds 1–2 reviewed and decided (`round-1-review.md`, `round-2-review.md`: E1–E11 accepted; T340 filed). Next: the
  Round 3 checked and final (`round-3-check.md`). Next: step F, the build, with the lane plan in `round-2-review.md`.
- Reusable from T335:
  - `design/pilot/build-lanes.md` (the lane rules);
  - `design/pilot/review-lanes.md` (the reviewers' brief);
  - `design/pilot/replay-workflow.js` (step G). Its Playwright install in the old job's scratch folder is gone; reinstall
    in a new scratch folder with `npm install playwright@1.63` and `channel: 'chrome'`.

### For the operator
- **P2s before real users (14):** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now also carries T335's migration (the
  account's acting role).
- **One slip:** a redaction command printed two of the previous replay's cast passwords into the session transcript.
  They open only cast accounts in local scratch databases.

### Environment left

- **The replay app on `:5180`**, holding the new story's end state: `wombat_scenario_t335`, built from `b347e11c`.
  - Stop it with `tools/scenario-replay.ps1 stop 5180`.
  - Its cast's passwords are in `pwd_DO_NOT_COMMIT.txt`, under "T335 step G replay".
- **Snapshots:** `recovery/scenario-t335-post-act{1..6,A}.dump` (new), and the older `scenario-post-act*.dump`.
- **Droppable scratch databases:** `wombat_scenario_t335_states_*`, `wombat_scenario_states*`, `_rc*` and `_t292`.
- **The dev app on `:5080`** still runs the build from before 2026-09-26. Its next restart applies T307's and T335's
  migrations.
- **Branches:** the merged lanes `t335*` and the `t335` integration branch are kept locally. Their worktrees are
  removed, except `.claude/worktrees/verify-master`.

### Traps

- **Replays rename captures.** A re-take must keep its file name. After a replay, run `check_baseline_paths.py`, then
  check citations against the images: a path can exist and show something else (BRIEF § 11).
- **A canvas does not copy the design system's logos.** Copy the mark into it first.
- **Design-system publish order:** uploads first, then one publish of the changed `project/` files, then the index read
  again and sent last. A publish is refused until this session has read the artifact.
- **No browser MCP is needed.** Use the Playwright library with `channel: 'chrome'`, installed in a scratch folder.
- **Dev settings** send mail to `localhost:25` and links to `:5080`; `scenario-replay.ps1 start` overrides both.
- **Downloads:** IDM takes them into `Downloads\Compressed`; check them in the page.
- **From bash,** `… sql` needs a `.sql` file (quoted identifiers).
- **Task titles** over ~150 characters break Windows paths.
- **Two test helpers** named `StyleSheet.cs` and `Stylesheet.cs` are one file on Windows. Lanes name shared helpers up
  front.
- **Earlier traps hold:** the audit pipeline commits a failed handler's staged rows (T201); merges are squashed per task.
