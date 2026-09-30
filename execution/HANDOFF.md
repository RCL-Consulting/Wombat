# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-09-30 (Opus): flow 03 finished (T342 done); flow 04 designed and built (T350, replay under way)

The operator asked to recover the task a machine shutdown had killed (T342 step 7), then to proceed into flow 04.

### Done

- **T342, flow 03, closed.** Recovery: acts 1–2 stood; act 3's agent had died unwritten, so post-act2 was restored into
  `wombat_scenario_t342b` and acts 3–A replayed there. 325 steps, 254 with no gap. Three fixes from the replay and
  states (`daaf6386`, `5fd5abcd`, `a341b44e`); design system re-synced and published as **version 10** (`b05f3862`);
  BRIEF § 11 "Flow 03" lessons; T348, T349 filed.
- **T350, flow 04 (an assessor works their inbox):**
  - Brief restated (`30c29c31`); canvas https://claude.ai/artifact/8JnYLZp6CTR1Mg38a5v7DX; round 1 (A, "Two pages, one
    list"), round 2 (four-sided review, C1–C13, E1–E5), round 3 checked: all hold. Every decision: accept all.
  - **Built and squashed: `06aa51d7`.** Waves A1 waiting / A2 words, then B lists / C page / D form, a runbook-and-docs
    lane, a four-sided review (`build-review.md`: 0 high, 6 medium: five fixed, one dropped), one fix pass. All six
    suites green (Domain 810, Application 3,581, Infrastructure 1,011, Architecture 50, Web 3,041, Integration 481).
    T351, T352 filed (P3). DESIGN.md banner lists flow 04.
  - **Step 7, the replay, started** on a fresh `wombat_scenario_t350` (:5180, published from `5aad596e`), brief
    `design/flows/04-assessor-inbox/replay-brief.md`. **Act 1 done** (`ef64eb8d`: 22/31 no gap, no regression; dump
    `scenario-t350-post-act1`). **Act 2 was running at the session's end**, in Step 2.26's real hour's wait (the
    Resend appears only after an hour, T283; the operator chose to wait rather than age the row).

### Decisions (the operator's)

- Flow 03's 3.10: keep the refusal summary until the next submit (STATE's adopted defaults).
- Flow 04: all of rounds 1 and 2 accepted; Q7: the rater does not see the registrar's year minimum.
- CLAUDE.md § Multi-agent workflows (the operator's edit, uncommitted, theirs to commit): followed from T342 step 8 on.

### Next session: T350 step 7 continues. **Model: Opus.**

- **First check act 2.** If `act-2-onboarding.md` has uncommitted T350 lines and a dump `scenario-t350-post-act2`
  exists, act 2 finished: commit it and go on to act 3. If there is no post-act2 dump, act 2 died: restore
  `scenario-t350-post-act1` into a new name (`tools/scenario-replay.ps1 restore scenario-t350-post-act1
  wombat_scenario_t350b`; the tool never drops), restart the app on it, discard the partial act-2 edits
  (`git checkout -- execution/knowledge/scenario-paediatrics/act-2-onboarding.md`), and replay act 2 from its start.
- Then acts 3–6 and A, one agent each (prompts: the brief with `<ACT>`, `<FILE>`, `<DIR>`, `<PW>`, `<PWURL>`, `<G>`;
  pass each act the ids and state the previous act reported). Watch 3.5, 3.11, 3.15, 3.24, 3.30 (the ageing now also
  ages the history rows), 3.33 (the other-role line) and A.7.2 at 390.
- Then the states capture (flow 04's new states.md rows; republish first), step 8 (design system re-sync: sentence-case
  moves, the pager, the new components; publish as version 11) and step 9 (BRIEF § 11 "Flow 04").

### For the operator

- **P2s before real users (16):** T288, T289, T304, T305, T311–T313, T315, T316, T319, T320, T329, T333, T334, T341, T350.
- **T157 (deploy), T128 (backup), the College questions:** unchanged. The deploy now carries flows 02–04 (no migration).

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t350`, published from `5aad596e`; its log is
  `.scenario-app/wombat_scenario_t350.log`. Stop it with `tools/scenario-replay.ps1 stop 5180` if act 2 is abandoned.
- **Replay scratch:** this session's scratchpad `…/b4d7c8bc-…/scratchpad/g350` (`replay-pw.env` with the cast's passwords,
  untracked; the act scripts). If that scratchpad is gone, the cast's passwords are lost: register them again on a fresh
  database from act 1, or reset them by SQL from a known hash.
- **Droppable databases:** `wombat_scenario_t342`, `_t342b`, `_t342_states_518{4,6,8}*`, and the older `_t339*`,
  `_t335*`, `_states*`, `_rc*`, `_t292`.
- **Branches:** `t350*` and `t342*` lanes kept locally (squashed); their worktrees are removed. `verify-master` stays for
  test runs (the dev app on `:5080` locks the main tree's Release output).

### Traps

- **A design-system publish:** upload first, then `read` the artifact (no path), read each file to change, send the files
  in one publish with `root` = a folder holding `project/…`, then read the index again and send it last.
- **Replays keep capture names**; run `check_baseline_paths.py`. **Republish before the states capture.**
- **A lane may report a clean state its safety check never saw** (the classifier was down once): verify its commit.
- **Task titles** over ~150 characters break Windows paths. **Two test helpers** named `StyleSheet.cs` and
  `Stylesheet.cs` are one file on Windows. The audit pipeline commits a failed handler's staged rows (T201).
