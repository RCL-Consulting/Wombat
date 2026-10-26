# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-26/27 (Opus): T376, flow 12, step 7's acts

The operator said "yes" (push and start the replay), then "carry on through the remaining acts". Pushed
`a95e1daa..c5a38bf2`.

- **The replay brief** `design/flows/12-people-admin/replay-brief.md` (`c583ae44`), generated from flow 11's by scratch
  `g376/make_replay_brief.py`, which also writes the per-act prompts `g376/prompt-act-*.txt`.
- **The acts played** (fresh `wombat_scenario_t376`, D = 2026-10-26; one Sonnet agent an act, each committed): 328 steps,
  283 with no gap, **no flow 12 regression, no new defect**; every gap a known task (T370's Present order is
  runbook-wrong at 4.25/4.29/4.41/4.42/5.8). Commits `085fa299`, `68373790`, `d4acd49f`, `6e4ff201`, `4c7cd1db`,
  `01d58381`, `4688c4d6`. T376's notes carry the counts, the closed gaps and what was measured.
- **For review:** Escape does nothing in the Add a role… panel; DESIGN.md says Cancel only, so the brief over-asked (no
  finding). Home has no locked-out line ("People 1 of 14 are locked out" was a reviewer's guess). Not played: the
  rows-per-page focus (no pager at this size) and Record training at 390 (every assessor already recorded).
- **Next:** step 7's states pass: flow 12's rows in `states.md` on scratch copies (g373's `make_states_prompts.py` is the
  template), T333's A.3.1 check and the SSO states on a scratch instance with a provider configured; then step 8
  (re-sync the design system, v29 now) and step 9.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t376` (the appendix's end state). Dumps
  `recovery/scenario-t376-post-act{1..6,A}`, `-pre-a515-A`. Scratch DB `wombat_scenario_t376_s515`: drop on the
  operator's word. Old: `wombat_scenario_t373` and its `_s*` scratch DBs, `wombat_scenario_t369`.
- Scratch `g376` (this session's scratchpad): act scripts, prompts, `replay-pw.env` (never print it).
- Branches `t376*` and their worktrees kept. Commits after `c5a38bf2` unpushed (push on the operator's word).

## Session 2026-10-23 (Opus): T376, flow 12 (people admin), started

The operator said "push, then start flow 12". Pushed `9d979834..98584921`. T376 created and started (from T373's shape).

- **Steps 3–4 done:** canvas "Wombat 12" https://claude.ai/artifact/EUfPe9d3zgnGxvQ9LeSzz8, design system v29
  (1792508645-23ab) installed, `tokens.css` by hand (scratchpad `c12/`), mark `/_blob/b8532f490e278114d4c3b43f9a0533f6`.
- **Steps 1–2 done** (one Opus agent, then checked here): brief restated, `round-1-ask.txt` written, canvas and mark
  filled in, completeness/verbatim/baseline checks all exit 0, staged (`design/upload/`, 123 files; the four
  crop-first captures opened, nothing to crop). T376's notes have the Mode, the drops and § 6's Q1–Q16.
- **Round 1 drawn and assessed:** 44 boards in `round-1/` (`62f6ec41`); `round-1-assessment.md` (picks S1, H1, UA;
  4 must-fixes; Q1–Q22 for the operator). **T377 proposed** (SSO can strip a hand-given role). **The operator took Q1–Q22 as recommended**
  (2026-10-23). Recorded in the assessment's § 10.
- **Round 2 drawn:** 178 files in `round-2/` (`a944726e`); the captures were read from `design/upload/` (not attached).
  Reviewed by three Sonnet agents (`round-2-review-{code,cast,ds}.md`, committed); K1, K3, D3, D4 and C9 checked
  here. Merged in `round-2-review.md` (`9a51ee7a`: 10 must, 24 should, 16 nits). **The operator took F1–F11 as
  recommended.** Recorded; `round-3-ask.txt` written.
- **Round 3 drawn and checked final** (175 boards, `a83bf182`; `round-3-check.md`: 78/80 done, the rest build notes in
  T376's notes). Steps 1–5 done. Pushed through `bc8218c7`.
- **The prerequisites landed** (Q16, F5), each its own commit, unpushed: **T289** `b8ed8fd6` (the assessor-profile save
  refuses a trainee-admin first and any cross-institution move, the Administrator's too); **T288** `964a2c4e` (a mapping
  only for its provider's own institution; the mapper applies only those; trainee-first on list, create, delete; the
  page shows the T278 note and only her providers); **T333** (`WombatRoles.AdmissionManaged`: no mapping gives or takes
  Trainee or PendingTrainee; admission's refusal and DOMAIN.md reworded). Suites green: Domain 829, Application 4001,
  Web 3983, Infrastructure 1029, Architecture 52, Integration SSO 25.
- **Traps:** T333's browser item (A.3.1) is unticked, carried to T376 step 7: with no provider the page draws no role at
  all. T289 handed scope ownership and the picker to T376's build (M1, F4, S10): the profile save still writes scope
  until then.
- **Step 6, built:** squashed onto master as `e53d4ae7` (`feat(people): flow 12 …`), tree identical to `t376`
  (`acecd43e`). Planner § 0, four lanes, runbook lane, three Sonnet reviews (1 must, 14 should), one fix pass (all
  fixed). Suites: Domain 830, Application 4124, Infrastructure 1033, Architecture 52, Web 4135, Integration 510. T376's
  notes carry step 7's cautions. Branches `t376*` and worktrees kept. **Not pushed** since `a95e1daa`.
- **Next: step 7, the replay** on a fresh database (flow 10's `replay-brief.md`), on the operator's word.
