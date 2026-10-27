# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

## Session 2026-10-27 (Opus): T379, flow 13 (graduation and exit), started

The operator said "push and lets start flow 13". Pushed `06913eca..82b108a6`. T379 created and started (T376's shape).

- **Steps 3–4:** canvas "Wombat 13" https://claude.ai/artifact/SujH2iB2snNe7VANE4SxdK, design system v33
  (1793082623-7a3a) installed, `tokens.css` by hand (scratchpad `c13/`), mark `/_blob/f4e05985da677926678d5d52de67cf4f`.
- **Steps 1–2** (one Opus agent, checked here): brief restated, `round-1-ask.txt` written with the mark, checks exit 0,
  staged (`design/upload/`, 122 files, none to crop) — `02120936`. **The operator took Q1–Q16 as recommended**
  (the brief's § 6): T312 lands before the build (Q10); T310 to flow 20; T374 out.
- **Next:** the operator sends `design/flows/13-graduation-and-exit/round-1-ask.txt` to the canvas with § 4's key
  screenshots; then round 1's assessment here.

## Session 2026-10-26/27 (Opus): T376, flow 12, steps 7–9, done

The operator said "yes" (push, replay), "carry on through the remaining acts", "yes" (push, states, steps 8 and 9) and
"drop the scratch db's". Pushed through `06913eca`.

- **Step 7, the replay** (fresh `wombat_scenario_t376`, D = 2026-10-26; brief `c583ae44`; one Sonnet agent an act):
  328 steps, 283 with no gap, **no flow 12 regression**; every gap a known task (T370's order is runbook-wrong). Acts
  `085fa299`–`4688c4d6`. **States:** two agents in turn, 89 rows, 7 corrected (`e402ca33`, `f12b376a`); **T333's A.3.1
  check passed** on a scratch app with a KGK provider. **T378 proposed** (the Administrator's Create institution form
  refuses with plain ValidationMessages; flow 18's page).
- **Step 8:** the live system matched HEAD (version bumps only); re-synced by one Opus agent (`a5596c52`), reviewed by
  one Sonnet (`resync-review.md`: 0 must, 2 should, 4 nits), fixed (`6bc5a0c2`, with one page fix: her record's empty
  header actions cell; Web 4135, Application 4124); published as **version 33** (`1fa34320`).
- **Step 9:** BRIEF § 11 "Flow 12" (`92576fc7`). **T376 done.**
- **For review:** the user page at 390 shows "< Users" above the heading where the narrow state row says "no Back to
  users" (left as is); Escape does nothing in a note-panel form, as DESIGN.md says (the brief over-asked).
- **Next:** flow 13 (`design/flows/13-graduation-and-exit.md`) on the operator's word; T378 and nine others in proposed.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t376` (the appendix's end state, published before `6bc5a0c2`'s
  page fix). Dumps `recovery/scenario-t376-post-act{1..6,A}`, `-pre-a515-A`. Every `_s*` scratch DB (t373's and
  t376's) dropped on the operator's word; old `wombat_scenario_t373`, `_t369`, `_t290`, `_pdf` kept.
- Scratch `C:\Users\Renier\AppData\Local\Temp\claude\c--dev-Wombat\67ce8168-e9a2-4dd0-acb1-eab0ccee0868\scratchpad\g376`: act, states and step 8 scripts, `replay-pw.env` (never print it).
- Branches `t376*` and their worktrees kept. Commits after `06913eca` unpushed (push on the operator's word).

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
