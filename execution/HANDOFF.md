# Handoff — wombat

Cap: 80 lines. The most recent session only; `harness.py trim` archives the rest into `log/`.

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
- **Next: T376 step 6, the build** (flow 11's build lanes as template), on the operator's word.

## Session 2026-10-20 (Opus): T373, flow 11, done

The operator said "proceed" (push and start the replay). Pushed through `9d979834`. **Not pushed:** `c0e5d0cc` to HEAD
(push on the operator's word).

- **Step 7, the replay** (fresh `wombat_scenario_t373`, D = 2026-10-20; one Sonnet agent an act): 331 steps, 280 with no
  gap, **no flow 11 regression**; every gap a known task (T370 at 4.25/4.29/4.41/4.42/5.8). Results `42f171b5`,
  `40525efc`, `c1f03dd1`, `783af7b5`, `d771aa91`, `f67ade0c`; runbook fixes `a70f030b`. Act 3's 3.1-3.42 and 3.50-3.54c
  Actuals were re-based from T369's text, not played fresh. **States:** two agents in turn, 85 rows, 12 corrected
  (`15bee480`); **T375 proposed** (`11fd5203`: a described radio's hit area).
- **Step 8:** the live system matched HEAD; re-synced (`79e2312f`); two page faults it found fixed (`27ff8779`: the primary
  on top at 390 on Invite a person and Admit; only the admitted result's first sentence bold; Web 3980; browser-checked,
  two narrow states re-taken); one Sonnet review (`0b800f29`); icons uploaded; **design system version 29**
  (1792508645-23ab, `4829f210`).
- **Step 9:** BRIEF § 11 "Flow 11" (`18f872ee`). **T373 done.**
- **Next:** flow 12 (`design/flows/12-people-admin.md`) on the operator's word; T375 and seven others wait in proposed.

### Environment left

- **The replay app on `:5180`** runs `wombat_scenario_t373` (the appendix's end state), republished from the working tree
  at `27ff8779`'s code. Dumps `recovery/scenario-t373-post-act{1..6,A}`, `-pre-a515-A`.
- **Scratch DBs** `wombat_scenario_t373_s10`, `_s11`, `_s12`, `_s20` (Wim Gone admitted there by step 8's check; Wanda
  Waiting's scope rows restored), `_s21`, `_s515`: drop on the operator's word. Old: `wombat_scenario_t369`.
- Scratch: this session's `g373` (`replay-pw.env` must never be printed; act, states and step 8 scripts).
- **Session finalised.** Tree clean, lint ok. 18 commits after `9d979834` unpushed (push on the operator's word). The scratch
  app on :5182 is stopped; :5180 left running.
