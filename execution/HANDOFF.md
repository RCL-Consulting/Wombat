# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-03 (Opus): T350 step 7, the replay, finished; the states capture next

The operator asked to carry on with the GUI redesign. **The repo is now `C:\dev\Wombat`** (moved from
`C:\Users\Renier\Wombat`), and **it is disconnected from its remote: commit locally, never push.**

### Done

- **Act 2 resumed, not replayed.** The previous session's agent died in Step 2.26's hour's wait, but its database
  (`wombat_scenario_t350`) stood exactly at the end of 2.25 and its lines were written; the operator chose to resume
  there. 2.1–2.25 checkpointed (`90cd0112`); 2.26–2.44 played 2026-10-03 (`60854d97`). Mahlangu's 2.18 tab was gone, so
  2.31 used a fresh sign-in before her admission (its Actual says so).
- **Acts 3–6 and A** (`4b7a04d0`, `9bc09e15`, `8f634de9`, `7c30795d`, `d5f9dccd`). **Whole runbook: 325 steps, 249 with
  no gap; every other gap an open task; 0 regression-t350, 0 new defect.** `check_baseline_paths` missing 0. Every
  watched step holds: 3.5, 3.11, 3.15, 3.24, 3.30 (ageing ages the history), 3.33, 5.25, 6.18, A.7.2 at 390 (stacks).
  T350's replay item is ticked. Dumps `scenario-t350-post-act{1..6,A}`.
- **Act 5's script slip:** a stale date helper filed an extra request, #24 (Cancelled); the step was filed as written,
  #25. From 24 on, ids are one higher than T342's lines. Harmless: the runbook keys by email, not id.
- **6.2 and 6.14's restart claims** were skipped by the agent; the integrator restarted the app and found both rows
  identical (written into their Actual lines). A restart keeps the old log beside the new one.
- The replay brief names the new repo path.

### Decisions (the operator's)

- Resume act 2 from 2.26 on the same database rather than restore post-act1 (2026-10-03).
- Earlier: flow 03's 3.10 (keep the refusal summary until the next submit); flow 04's rounds 1–2 accepted, Q7.
- CLAUDE.md § Multi-agent workflows (the operator's edit, uncommitted, theirs to commit).

### Next session: T350, the rest of step 7, then 8 and 9. **Model: Opus.**

- **The states capture:** about 35 flow-04 rows in `states.md` (Home's assessor and committee-line rows, the inbox, the
  activity page's assessor side; the 06aa51d7 diff of states.md lists them). No republish is needed: no code changed
  since `.scenario-app/bin` was published from `5aad596e`. As T342 did: scratch databases restored from the post-act
  dumps (`restore <dump> wombat_scenario_t350_states_518x`), apps on 5184/5186/5188, never the replay's database.
- Then step 8 (design system re-sync: sentence-case moves, the pager, the new components; publish as version 11; one
  implementer and one Sonnet review) and step 9 (BRIEF § 11 "Flow 04": only what is new — the resume-in-place, the
  date-helper slip, the agent that skipped a restart).

### For the operator

- **P2s before real users (16):** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T350.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries flows 02–04 (no migration).

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t350` (final state, after the appendix), published from
  `5aad596e`; its log is `.scenario-app/wombat_scenario_t350.log` (acts 2–6's mail in the `-<stamp>.log` beside it).
- **Replay scratch:** `C:\Users\Renier\AppData\Local\Temp\claude\c--Users-Renier-Wombat\b4d7c8bc-…\scratchpad\g350`
  (`replay-pw.env`, the act scripts; `act-2\common2.mjs` has the new paths). The appendix changed Khumalo's and du
  Plessis's passwords; the old ones are kept as `_A` keys (and du Plessis's admin-set one as `_ADMINSET`).
- **Droppable databases:** `wombat_scenario_t342`, `_t342b`, `_t342_states_518{4,6,8}*`, and the older `_t339*`,
  `_t335*`, `_states*`, `_rc*`, `_t292`.
- **Branches:** `t350*` and `t342*` lanes kept locally (squashed). `verify-master` stays for test runs.

### Traps

- **No push.** The remote is disconnected (the operator, 2026-10-03).
- **A design-system publish:** upload first, then `read` the artifact (no path), read each file to change, send the files
  in one publish with `root` = a folder holding `project/…`, then read the index again and send it last.
- **Copied replay scripts carry stale paths and date helpers;** compute every date from D.
- **A lane may report a clean state its safety check never saw:** verify its commit.
- **Task titles** over ~150 characters break Windows paths. **Two test helpers** named `StyleSheet.cs` and
  `Stylesheet.cs` are one file on Windows. The audit pipeline commits a failed handler's staged rows (T201).
